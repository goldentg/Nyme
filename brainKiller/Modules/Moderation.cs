using System;
using System.Linq;
using System.Threading.Tasks;
using brainKiller.Common;
using brainKiller.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace brainKiller.Modules
{
    public class Moderation : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<Moderation> _logger;

        public Moderation(ILogger<Moderation> logger)
        {
            _logger = logger;
        }


        [Command("purge", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Deletes x amount of messages from a channel\n(ManageMessages permissions required)")]
        public async Task Purge(int amount)
        {
            var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);

            var message =
                await Context.Channel.SendMessageAsync($"{messages.Count() - 1} messages deleted successfully");
            await Task.Delay(2500);
            await message.DeleteAsync();
        }


        [Command("kick", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [Summary("Kicks a member from the server\n(Kick permissions required)")]
        public async Task Kick([Remainder] SocketGuildUser user)
        {
            await Context.Channel.SendSuccessAsync("Success!",
                $"{Context.User.Mention} has successfully kicked {user.Mention}");
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
            await user.BanAsync();
        }

        [Command("mute")]
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
            await Context.Channel.SendSuccessAsync($"Muted {user.Username}",
                $"Duration: {minutes} minuets\nReason: {reason ?? "None"}");
        }

        [Command("unmute")]
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
            await Context.Channel.SendSuccessAsync($"Unmuted {user.Username}",
                "Successfully unmuted the user");
        }
    }
}