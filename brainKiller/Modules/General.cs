using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Infrastructure;

namespace brainKiller.Modules
{
    public class General : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<General> _logger;
        private readonly Servers _servers;

        public General(ILogger<General> logger, Servers servers)
        {
            _logger = logger;
            _servers = servers;
        }

        [Command("ping")]
        public async Task Ping()
        {
            await Context.Channel.SendMessageAsync("Pong!");
        }

        [Command("info")]
        public async Task Info(SocketGuildUser user = null)
        {
            if (user == null)
            {
                var builder = new EmbedBuilder()
                    .WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
                    .WithDescription("see some info about yourself")
                    .WithColor(new Color(33, 176, 252))
                    .AddField("User ID", Context.User.Id, true)
                    .AddField("Account Creation Date", Context.User.CreatedAt.ToString("yyyy/MM/dd"), true)
                    .AddField("Date User Joined Server", (Context.User as SocketGuildUser).JoinedAt.Value.ToString("yyyy/MM/dd"), true)
                    .AddField("Roles", string.Join(" ", (Context.User as SocketGuildUser).Roles.Select(x => x.Mention)))
                    .WithCurrentTimestamp();
                var embed = builder.Build();
                await Context.Channel.SendMessageAsync(null, false, embed);
            }
            else
            {
                var builder = new EmbedBuilder()
                    .WithThumbnailUrl(user.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
                    .WithDescription($"see some info about {user.Username}")
                    .WithColor(new Color(33, 176, 252))
                    .AddField("User ID", user.Id, true)
                    .AddField("Account Creation Date", user.CreatedAt.ToString("yyyy/MM/dd"), true)
                    .AddField("Date User Joined Server", user.JoinedAt.Value.ToString("yyyy/MM/dd"), true)
                    .AddField("Roles", string.Join(" ", user.Roles.Select(x => x.Mention)))
                    .WithCurrentTimestamp();
                var embed = builder.Build();
                await Context.Channel.SendMessageAsync(null, false, embed);
            }
        }

        

        [Command("server")]
        public async Task Server()
        {
            var builder = new EmbedBuilder()
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithDescription("Server Information")
                .WithTitle($"{Context.Guild.Name} Server Stats")
                .WithColor(new Color(33, 176, 252))
                .AddField("Created At", Context.Guild.CreatedAt.ToString("yyyy/MM/dd"), true)
                .AddField("Member Count", (Context.Guild as SocketGuild).MemberCount + " members", true)
                .AddField("Online Users", (Context.Guild as SocketGuild).Users.Where(x => x.Status != UserStatus.Offline).Count() + " members", true);
            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(null, false, embed);
        }

        [Command("prefix")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task Prefix(string prefix = null)
        {
            if (prefix == null)
            {
                var guildPrefix = await _servers.GetGuildPrefix(Context.Guild.Id) ?? "!";
                await Context.Channel.SendMessageAsync($"The current prefix of this bot is `{guildPrefix}`");
                return;
            }

            if (prefix.Length > 8)
            {
                await ReplyAsync("The length of the new prefix is too long!");
            }

            await _servers.ModifyGuildPrefix(Context.Guild.Id, prefix);
            await ReplyAsync($"The prefix of this bot has been changed to `{prefix}`");
        }
    }
}
