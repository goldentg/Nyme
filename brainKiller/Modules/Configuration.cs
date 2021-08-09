using System;
using System.Linq;
using System.Threading.Tasks;
using brainKiller.Common;
using brainKiller.Utilities;
using Discord;
using Discord.Commands;
using Infrastructure;

namespace brainKiller.Modules
{
    public class Configuration : ModuleBase<SocketCommandContext>
    {
        private readonly AutoRoles _autoRoles;
        private readonly AutoRolesHelper _autoRolesHelper;
        private readonly Ranks _ranks;
        private readonly RanksHelper _ranksHelper;
        private readonly ServerHelper _serverHelper;
        private readonly Servers _servers;

        public Configuration(RanksHelper ranksHelper, Servers servers, Ranks ranks, AutoRolesHelper autoRolesHelper,
            AutoRoles autoRoles, ServerHelper serverHelper)
        {
            _ranksHelper = ranksHelper;
            _autoRolesHelper = autoRolesHelper;
            _servers = servers;
            _ranks = ranks;
            _autoRoles = autoRoles;
            _serverHelper = serverHelper;
        }

        [Command("prefix", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Sets the prefix for this bot\n(Administrator permissions required)")]
        public async Task Prefix(string prefix = null)
        {
            if (prefix == null)
            {
                var guildPrefix = await _servers.GetGuildPrefix(Context.Guild.Id) ?? "!";
                await ReplyAsync($"The current prefix of this bot is `{guildPrefix}`.");
                return;
            }

            if (prefix.Length > 8)
            {
                await Context.Channel.SendErrorAsync("Error", "The length of the new prefix is too long!");
                return;
            }

            await _servers.ModifyGuildPrefix(Context.Guild.Id, prefix);
            await ReplyAsync($"The prefix has been adjusted to `{prefix}`.");
            await _serverHelper.SendLogAsync(Context.Guild, "Prefix adjusted",
                $"{Context.User.Mention} modifed the prefix to `{prefix}`.");
        }

        [Command("dmwelcome")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Sets up a event that will auto dm all new members of server")]
        public async Task DmSetup(string message = null)
        {
            if (message == null)
            {
                var GuildDmMessage = await _servers.GetDmMessageAsync(Context.Guild.Id);
                await ReplyAsync($"The current dm welcome message is `{GuildDmMessage}`");
                return;
            }

            if (message.Length > 200)
            {
                await Context.Channel.SendErrorAsync("Error", "This message is too long");
                return;
            }

            await _servers.ModifyDmChannelAsync(Context.Guild.Id, message);
            await Context.Channel.SendSuccessAsync("Success", "Welcome dm message has been changed");
            await _serverHelper.SendLogAsync(Context.Guild, "Dm Welcome Message Changed",
                $"{Context.User.Mention} modified the welcome dm message to `{message}`");
        }

        [Command("ranks", RunMode = RunMode.Async)]
        [Summary("Lists all the inputted self roles on this server")]
        public async Task Ranks()
        {
            var ranks = await _ranksHelper.GetRanksAsync(Context.Guild);
            if (ranks.Count == 0)
            {
                await Context.Channel.SendErrorAsync("Error", "This server doesn't have any ranks!");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            var description =
                "This message lists all available ranks \nIn order to add a rank you can use the name or ID of the rank";
            foreach (var rank in ranks) description += $"\n{rank.Mention} ({rank.Id})";

            await Context.Channel.SendSuccessAsync("Ranks", $"{description}");
        }

        [Command("addrank", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Summary("Add a rank/self role for this server\n(Admin permissions required)")]
        public async Task AddRank([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _ranksHelper.GetRanksAsync(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x =>
                string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await Context.Channel.SendErrorAsync("Error", "That role does not exist!");
                return;
            }

            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await Context.Channel.SendErrorAsync("Error", "That role has a higher position then the bot");
                return;
            }

            if (ranks.Any(x => x.Id == role.Id))
            {
                await Context.Channel.SendErrorAsync("Error", "That role is already a rank");
                return;
            }

            await _ranks.AddRankAsync(Context.Guild.Id, role.Id);
            await Context.Channel.SendSuccessAsync("Success", $"The role {role.Mention} has been added to the ranks!");
            await _serverHelper.SendLogAsync(Context.Guild, "Rank Added",
                $"{Context.User.Mention} has added the `{role.Name}` rank to the rank list");
        }

        [Command("delrank", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Summary("Delete a rank/selfrole from this server\n(Admin permissions required)")]
        public async Task DelRank([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _ranksHelper.GetRanksAsync(Context.Guild);
            var role = Context.Guild.Roles.FirstOrDefault(x =>
                string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await Context.Channel.SendErrorAsync("Error", "That role does not exist!");
                return;
            }

            if (ranks.Any(x => x.Id != role.Id))
            {
                await Context.Channel.SendErrorAsync("Error", "That role isnt a rank yet!");
                return;
            }

            await _ranks.RemoveRankAsync(Context.Guild.Id, role.Id);
            await Context.Channel.SendSuccessAsync("Success",
                $"The role {role.Mention} has been removed from the ranks");
            await _serverHelper.SendLogAsync(Context.Guild, "Rank Removed",
                $"{Context.User.Mention} has removed `{role.Name}` from the rank list");
        }


        [Command("autoroles", RunMode = RunMode.Async)]
        [Summary("Displays all autoroles set for this server")]
        public async Task AutoRoles()
        {
            var autoRoles = await _autoRolesHelper.GetAutoRolesAsync(Context.Guild);
            if (autoRoles.Count == 0)
            {
                await Context.Channel.SendErrorAsync("Error", "This server doesn't have any auto roles!");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            var description = "This message lists all auto roles \nIn order to remove an autorole, use the name or ID";
            foreach (var autoRole in autoRoles) description += $"\n{autoRole.Mention} ({autoRole.Id})";

            await Context.Channel.SendSuccessAsync("Autoroles", $"{description}");
        }

        [Command("addautorole", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Summary("Add an autorole for this server\n(Admin permissions required)")]
        public async Task AddAutoRole([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var autoRoles = await _autoRolesHelper.GetAutoRolesAsync(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x =>
                string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await Context.Channel.SendErrorAsync("Error", "That role does not exist!");
                return;
            }

            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await Context.Channel.SendErrorAsync("Error", "That role has a higher position then the bot");
                return;
            }

            if (autoRoles.Any(x => x.Id == role.Id))
            {
                await Context.Channel.SendErrorAsync("Error", "That role is already an autorole");
                return;
            }

            await _autoRoles.AddAutoRoleAsync(Context.Guild.Id, role.Id);
            await Context.Channel.SendSuccessAsync("Success",
                $"The role {role.Mention} has been added to the autoroles!");
            await _serverHelper.SendLogAsync(Context.Guild, "Auto-Role Added",
                $"{Context.User.Mention} has added the `{role.Name}` role to the auto-role list");
        }

        [Command("delautorole", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Summary("Delete an autorole from this server\n(Admin permissions required)")]
        public async Task DelAutoRole([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var autoRoles = await _autoRolesHelper.GetAutoRolesAsync(Context.Guild);
            var role = Context.Guild.Roles.FirstOrDefault(x =>
                string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await Context.Channel.SendErrorAsync("Error", "That role does not exist!");
                return;
            }

            if (autoRoles.Any(x => x.Id != role.Id))
            {
                await Context.Channel.SendErrorAsync("Error", "That role isnt an autorole yet!");
                return;
            }

            await _autoRoles.RemoveAutoRoleAsync(Context.Guild.Id, role.Id);
            await Context.Channel.SendSuccessAsync("Success",
                $"The role {role.Mention} has been removed from the autorole list");
            await _serverHelper.SendLogAsync(Context.Guild, "Auto-Role Deleted",
                $"{Context.User.Mention} has removed the `{role.Name} autorole from the autoroles list`");
        }

        [Command("welcome")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Settings/Setup for welcoming a new member\n(Admin permissions required)")]
        public async Task Welcome(string option = null, string value = null)
        {
            if (option == null && value == null)
            {
                var fetchedChannelId = await _servers.GetWelcomeAsync(Context.Guild.Id);
                if (fetchedChannelId == 0)
                {
                    await ReplyAsync("There has not been set a welcome channel yet!");
                    return;
                }

                var fetchedChannel = Context.Guild.GetTextChannel(fetchedChannelId);
                if (fetchedChannel == null)
                {
                    await ReplyAsync("There has not been set a welcome channel yet!");
                    await _servers.ClearWelcomeAsync(Context.Guild.Id);
                    return;
                }

                var fetchedBackground = await _servers.GetBackgroundAsync(Context.Guild.Id);

                if (fetchedBackground != null)
                    await ReplyAsync(
                        $"The channel used for the welcome module is {fetchedChannel.Mention}.\nThe background is set to {fetchedBackground}");
                else await ReplyAsync($"The channel used for the welcome module is {fetchedChannel.Mention}.");

                return;
            }

            if (option == "channel" && value != null)
            {
                if (!MentionUtils.TryParseChannel(value, out var parsedId))
                {
                    await ReplyAsync("Please pass through a valid channel!");
                    return;
                }

                var parsedChannel = Context.Guild.GetTextChannel(parsedId);

                if (parsedChannel == null)
                {
                    await ReplyAsync("Please pass through a valid channel!");
                    return;
                }

                await _servers.ModifyWelcomeAsync(Context.Guild.Id, parsedId);
                await ReplyAsync($"Successfully modified the welcome channel to {parsedChannel.Mention}.");
                return;
            }

            if (option == "background" && value != null)
            {
                if (value == "clear")
                {
                    await _servers.ClearBackgroundAsync(Context.Guild.Id);
                    await ReplyAsync("Successfully cleared the background for this server");
                    return;
                }

                await _servers.ModifyBackgroundAsync(Context.Guild.Id, value);
                await ReplyAsync($"Successfully modified the background to {value}.");
                return;
            }

            if (option == "clear" && value == null)
            {
                await _servers.ClearWelcomeAsync(Context.Guild.Id);
                await ReplyAsync("Successfully cleared the welcome channel");
                return;
            }

            await ReplyAsync("You did not use this command properly!");
        }


        [Command("logs")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ViewAuditLog)]
        [Summary(
            "Settings/Setup for setting up logs\n(Admin permissions required.\nbot requires View Audit Log permissions)")]
        public async Task Logs(string value = null)
        {
            if (value == null)
            {
                var fetchedChannelId = await _servers.GetLogsAsync(Context.Guild.Id);
                if (fetchedChannelId == 0)
                {
                    await Context.Channel.SendErrorAsync("Error", "There has not been set a logs channel yet!");
                    return;
                }

                var fetchedChannel = Context.Guild.GetTextChannel(fetchedChannelId);
                if (fetchedChannel == null)
                {
                    await Context.Channel.SendErrorAsync("Error", "There has not been set a logs channel yet!");
                    await _servers.ClearLogsAsync(Context.Guild.Id);
                    return;
                }

                await ReplyAsync($"The channel used for the logs is set to {fetchedChannel.Mention}.");

                return;
            }

            if (value != "clear")
            {
                if (!MentionUtils.TryParseChannel(value, out var parsedId))
                {
                    await Context.Channel.SendErrorAsync("Error", "Please pass in a valid channel!");
                    return;
                }

                var parsedChannel = Context.Guild.GetTextChannel(parsedId);
                if (parsedChannel == null)
                {
                    await ReplyAsync("Please pass in a valid channel!");
                    return;
                }

                await _servers.ModifyLogsAsync(Context.Guild.Id, parsedId);
                await Context.Channel.SendSuccessAsync("Success",
                    $"Successfully modified the logs channel to {parsedChannel.Mention}.");
                return;
            }

            if (value == "clear")
            {
                await _servers.ClearLogsAsync(Context.Guild.Id);
                await Context.Channel.SendSuccessAsync("Success", "Successfully cleared the logs channel.");
                return;
            }

            await Context.Channel.SendErrorAsync("Error", "You did not use this command properly.");
        }
    }
}