using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using brainKiller.Common;
using brainKiller.Utilities;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace brainKiller.Modules
{
    public class General : ModuleBase<SocketCommandContext>
    {
        private readonly Images _images;
        private readonly ILogger<General> _logger;
        private readonly RanksHelper _ranksHelper;

        public General(ILogger<General> logger, Images images, RanksHelper ranksHelper)
        {
            _logger = logger;
            _images = images;
            _ranksHelper = ranksHelper;
        }

        [Command("ping")]
        [Summary("Ping the bot")]
        public async Task Ping()
        {
            await Context.Channel.SendSuccessAsync("Ping", "Pong!");
        }

        [Command("info")]
        [Summary("See information about a fellow member")]
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
                    .AddField("Date User Joined Server",
                        (Context.User as SocketGuildUser).JoinedAt.Value.ToString("yyyy/MM/dd"), true)
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
        [Summary("View information about the server")]
        public async Task Server()
        {
            var builder = new EmbedBuilder()
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithDescription("Server Information")
                .WithTitle($"{Context.Guild.Name} Server Stats")
                .WithColor(new Color(33, 176, 252))
                .AddField("Created At", Context.Guild.CreatedAt.ToString("yyyy/MM/dd"), true)
                .AddField("Member Count", Context.Guild.MemberCount + " members", true)
                .AddField("Online Users",
                    Context.Guild.Users.Where(x => x.Status != UserStatus.Offline).Count() + " members", true);
            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(null, false, embed);
        }


        [Command("image", RunMode = RunMode.Async)]
        public async Task Image(SocketGuildUser user)
        {
            var path = await _images.CreateImageAsync(user);
            await Context.Channel.SendFileAsync(path);
            File.Delete(path);
        }

        [Command("say")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Make the bot say something. Admin perms required")]
        public async Task Say([Remainder] string msg)
        {
            await ReplyAsync(msg);
            await Context.Message.DeleteAsync();
        }

        [Command("rank", RunMode = RunMode.Async)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Summary("Add a rank/role to yourself")]
        public async Task Rank([Remainder] string identifier)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _ranksHelper.GetRanksAsync(Context.Guild);

            IRole role;

            if (ulong.TryParse(identifier, out var roleId))
            {
                var roleById = Context.Guild.Roles.FirstOrDefault(x => x.Id == roleId);
                if (roleById == null)
                {
                    await ReplyAsync("That role does not exist!");
                    return;
                }

                role = roleById;
            }
            else
            {
                var roleByName = Context.Guild.Roles.FirstOrDefault(x =>
                    string.Equals(x.Name, identifier, StringComparison.CurrentCultureIgnoreCase));
                if (roleByName == null)
                {
                    await ReplyAsync("That role does not exist!");
                    return;
                }

                role = roleByName;
            }

            if ((Context.User as SocketGuildUser).Roles.Any(x => x.Id == role.Id))
            {
                await (Context.User as SocketGuildUser).RemoveRoleAsync(role);
                await ReplyAsync($"Succesfully removed the rank {role.Mention} from you.");
                return;
            }

            await (Context.User as SocketGuildUser).AddRoleAsync(role);
            await ReplyAsync($"Succesfully added the rank {role.Mention} to you.");
        }
    }
}