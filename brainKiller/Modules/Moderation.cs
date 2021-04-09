using System;
using System.Linq;
using System.Threading.Tasks;
using brainKiller.Common;
using brainKiller.Services;
using brainKiller.Utilities;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace brainKiller.Modules
{
    public class Moderation : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<Moderation> _logger;
        private readonly ServerHelper _serverHelper;

        public Moderation(ILogger<Moderation> logger, ServerHelper serverHelper)
        {
            _logger = logger;
            _serverHelper = serverHelper;
        }


        [Command("purge", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Alias("clear")]
        [Summary("Deletes x amount of messages from a channel\n(ManageMessages permissions required)")]
        public async Task Purge(int amount)
        {
            var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);

            var message =
                await Context.Channel.SendMessageAsync($"{messages.Count() - 1} messages deleted successfully");
            await Task.Delay(2500);
            await message.DeleteAsync();
            await _serverHelper.SendLogAsync(Context.Guild, "Messages Purged",
                $"{Context.User.Mention} purged `{messages.Count() - 1}` messages from `{Context.Channel}`");
        }


        [Command("kick", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [Summary("Kicks a member from the server\n(Kick permissions required)")]
        public async Task Kick([Remainder] SocketGuildUser user)
        {
            await Context.Channel.SendSuccessAsync("Success!",
                $"{Context.User.Mention} has successfully kicked {user.Mention}");
            await _serverHelper.SendLogAsync(Context.Guild, "Kicked User",
                $"{Context.User.Mention} has kicked {user.Mention}");
            await user.KickAsync();
        }

        [Command("ban", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [Summary("Bans a member\n(Ban permissions required)")]
        public async Task Ban([Remainder] SocketGuildUser user)
        {
            await Context.Channel.SendSuccessAsync("Success!",
                $"{Context.User.Mention} has successfully banned {user.Mention}");
            //this log is no longer used since it is already on a onban event
            // await _serverHelper.SendLogAsync(Context.Guild, "Banned User",
            //     $"{Context.User.Mention} has kicked {user.Mention}");
            await user.BanAsync();
        }

        [Command("mute", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Summary("Mute a user for specified time\n(Kick permissions required)")]
        public async Task Mute(SocketGuildUser user, int minutes, [Remainder] string reason = null)
        {
            if (user.Hierarchy > Context.Guild.CurrentUser.Hierarchy)
            {
                await Context.Channel.SendErrorAsync("Invalid User", "Mentioned user has a higher role than the bot");
                return;
            }

            var role = (Context.Guild as IGuild).Roles.FirstOrDefault(x => x.Name == "Muted");
            if (role == null)
                role = await Context.Guild.CreateRoleAsync("Muted", new GuildPermissions(sendMessages: false), null,
                    false, null);

            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await Context.Channel.SendErrorAsync("Invalid Permissions",
                    "The muted role has a higher role than the bot");
                return;
            }

            if (user.Roles.Contains(role))
            {
                await Context.Channel.SendErrorAsync("Target User Already Muted", "That user is already muted");
                return;
            }

            await role.ModifyAsync(x => x.Position = Context.Guild.CurrentUser.Hierarchy);

            foreach (var channel in Context.Guild.TextChannels)
                if (!channel.GetPermissionOverwrite(role).HasValue ||
                    channel.GetPermissionOverwrite(role).Value.SendMessages == PermValue.Allow)
                    await channel.AddPermissionOverwriteAsync(role,
                        new OverwritePermissions(sendMessages: PermValue.Deny));
            CommandHandler.Mutes.Add(new Mute
            {
                Guild = Context.Guild,
                User = user,
                End = DateTime.Now + TimeSpan.FromMinutes(minutes),
                Role = role
            });
            await user.AddRoleAsync(role);
            await Context.Channel.SendSuccessAsync($"Muted {user.Mention}",
                $"Duration: {minutes} minuets\nReason: {reason ?? "None"}");
            await _serverHelper.SendLogAsync(Context.Guild, "User Muted",
                $"{Context.User.Mention} has muted {user.Mention} for `{minutes}. Reason: {reason ?? "None"}`");
        }

        [Command("unmute", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Summary("Unmute a muted user\n(Kick permissions required)")]
        public async Task UnMute(SocketGuildUser user)
        {
            var role = (Context.Guild as IGuild).Roles.FirstOrDefault(x => x.Name == "Muted");
            if (role == null)
            {
                await Context.Channel.SendErrorAsync("Not Muted",
                    "This user has not been muted");
                return;
            }

            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await Context.Channel.SendErrorAsync("Invalid Permissions",
                    "The muted role has a higher role than the bot");
                return;
            }

            if (!user.Roles.Contains(role))
            {
                await Context.Channel.SendErrorAsync("Target User Is Not Muted", "This user has not been muted");
                return;
            }

            await user.RemoveRoleAsync(role);
            await Context.Channel.SendSuccessAsync($"Unmuted {user.Mention}",
                "Successfully unmuted the user");
            await _serverHelper.SendLogAsync(Context.Guild, "User Un-muted",
                $"{Context.User.Mention} has unmuted {user.Mention}");
        }

        [Command("say", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Make the bot say something\n(Admin permissions required)")]
        public async Task Say([Remainder] string msg)
        {
            await ReplyAsync(msg);
            await Context.Message.DeleteAsync();
        }


        [Command("lock", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Summary("Lock a channel\n(Manage Roles permissions required)")]
        public async Task Lock(SocketTextChannel channel = null)
        {
            if (channel == null)
            {
                var ch = Context.Channel as SocketTextChannel;
                var currentPerms = ch.GetPermissionOverwrite(Context.Guild.EveryoneRole) ??
                                   new OverwritePermissions();
                await ch.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole,
                    currentPerms.Modify(sendMessages: PermValue.Deny));
                await Context.Channel.SendSuccessAsync("Locked", $"{ch.Mention} has been locked");
                await _serverHelper.SendLogAsync(Context.Guild, "Channel Locked",
                    $"{Context.User.Mention} has locked {ch.Mention}");
            }
            else
            {
                var currentPerms = channel.GetPermissionOverwrite(Context.Guild.EveryoneRole) ??
                                   new OverwritePermissions();
                await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole,
                    currentPerms.Modify(sendMessages: PermValue.Deny));
                await Context.Channel.SendSuccessAsync("Locked", $"{channel.Mention} has been locked");
                await _serverHelper.SendLogAsync(Context.Guild, "Channel Locked",
                    $"{Context.User.Mention} has unlocked {channel.Mention}");
            }
        }

        [Command("unlock", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Summary("Unlock a locked channel\n(Manage Roles permissions required)")]
        public async Task Unlock(SocketTextChannel channel = null)
        {
            if (channel == null)
            {
                var ch = Context.Channel as SocketTextChannel;
                var currentPerms = ch.GetPermissionOverwrite(Context.Guild.EveryoneRole) ??
                                   new OverwritePermissions();
                await ch.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole,
                    currentPerms.Modify(sendMessages: PermValue.Allow));
                await Context.Channel.SendSuccessAsync("Locked", $"{ch.Mention} has been unlocked");
                await _serverHelper.SendLogAsync(Context.Guild, "Channel Unlocked",
                    $"{Context.User.Mention} has unlocked {ch.Mention}");
            }
            else
            {
                var currentPerms = channel.GetPermissionOverwrite(Context.Guild.EveryoneRole) ??
                                   new OverwritePermissions();
                await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole,
                    currentPerms.Modify(sendMessages: PermValue.Allow));
                await Context.Channel.SendSuccessAsync("Locked", $"{channel.Mention} has been unlocked");
                await _serverHelper.SendLogAsync(Context.Guild, "Channel Unlocked",
                    $"{Context.User.Mention} has unlocked {channel.Mention}");
            }
        }
    }
}