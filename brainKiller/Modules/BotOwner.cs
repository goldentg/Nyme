using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using brainKiller.Utilities;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace brainKiller.Modules
{
    public class BotOwner : ModuleBase<SocketCommandContext>
    {
        private readonly Images _images;
        private readonly ILogger<General> _logger;
        private readonly ServerHelper _serverHelper;
        private readonly Servers _servers;

        public BotOwner(ILogger<General> logger, Servers servers, Images images, ServerHelper serverHelper)
        {
            _logger = logger;
            _servers = servers;
            _images = images;
            _serverHelper = serverHelper;
        }

        // public BotList BotList { get; private set; }


        [Command("random", RunMode = RunMode.Async)]
        [RequireOwner]
        [Summary("For bot owner testing only")]
        public async Task Random()
        {
            string[] responses = {"First", "Second", "Third"};
            await ReplyAsync(responses[new Random().Next(0, responses.Count())]);
        }


        [Command("announce")]
        [RequireOwner]
        [Summary("Send an announcement to all guilds\nBOT OWNER ONLY")]
        public async Task Announce([Remainder] string message)
        {
            var guilds = Context.Client.Guilds.ToList();
            foreach (var guild in guilds)
            {
                var messageChannel = guild.DefaultChannel as ISocketMessageChannel;
                if (messageChannel != null)
                {
                    var embed = new EmbedBuilder();
                    embed.Title = "Public Announcement";
                    embed.Description = message;
                    embed.WithFooter("From BlackLung");
                    embed.ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl();
                    await messageChannel.SendMessageAsync("", false, embed.Build());
                    Thread.Sleep(5000);
                }
            }
        }

        /*

        [Command("votes")]
        [RequireOwner]
        public async Task SendVotes(ISocketMessageChannel channel)
        {
            BotList = await BotList.Instantiate(Program.BotListBotId, Program.BotListToken);
            if (Program.BotList == null)
                await Context.Channel.SendErrorAsync("Error", "Bot is not allowed to see this information");
            var voters = await BotList.ThisBot.GetVotersAsync();
            var countedVotes = voters.GroupBy(voter => voter.Id).Select(voter =>
                new {voter.First().Id, Count = voter.Count(), voter.First().Username});

            var countedVotesList = countedVotes.ToList();
            var message =
                $"Total Votes: {BotList.ThisBot.Points}\nThis Month Votes: coming soon\nVoters Count: {countedVotesList.Count}\nTheir total votes: {voters.Count}";
            message = countedVotesList.Aggregate(message,
                (current, voter) => current + $"\n{voter.Username} ({voter.Count})");
            //await Messaging.Instance.SendMessage(channel, message);
            //await Context.Channel.SendMessageAsync(channel, message);
            await Context.Channel.SendSuccessAsync("Votes", $"{message}");
        }
        */
        /*
        [Command("botLists")]
        [RequireOwner]
        [Summary("Updates the bot lists")]
        public async Task UpdateBotList()
        {
            var serverCount = Context.Client.Guilds.Count;
            var usersCount = Context.Client.Guilds.Sum(x => x.MemberCount);
            var topApiUrl = $"https://top.gg/api/bots/{Context.Client.CurrentUser.Id}/stats";
            var httpClient = APIHelper.HttpClient;

            var topContent = new StringContent($"{{\"server_count\":\"{serverCount}\"}}", Encoding.UTF8,
                "application/json");

            var topRequest = new HttpRequestMessage
            {
                RequestUri = new Uri(topApiUrl),
                Method = HttpMethod.Post,
                Headers =
                {
                    {"Authorization", APIHelper.TopggApiKey}
                },
                Content = topContent
            };

            await httpClient.SendAsync(topRequest);
            Context.Channel.SendSuccessAsync("Success",
                $"Bot lists updated with {serverCount} servers and {usersCount} users.");
        }
        */
        [Command("guilds")]
        [RequireOwner]
        [Summary("BOT OWNER ONLY")]
        public async Task glds()
        {
            var totalGuilds = Context.Client.Guilds.Count();


            var builder = new EmbedBuilder
            {
                Color = new Color(114, 137, 218),
                Description =
                    $"*Total Guilds:* **{totalGuilds}**\n> ***Here is a list of guilds that the bot is connected to!***"
            };
            foreach (var guild in Context.Client.Guilds)
            {
                string description = null;
                var guildName = guild.Name;
                var guildId = guild.Id;
                var owner = guild.Owner.Username;
                var totalUsers = guild.Users.Count();
                var onlineUsers = guild.Users.Where(x => x.Status != UserStatus.Offline).Count();
                var totalBots = guild.Users.Where(x => x.IsBot != true).Count();
                var actualUsers = totalUsers - totalBots;


                description +=
                    $"__*Guild ID:*__ **{guildId}** __*Server Owner:*__ **{owner}** __*Total Users:*__ **{actualUsers}** __*Total Bots:*__ **{totalBots}** __*Total All:*__  **{totalUsers}** __*Total Online:*__ **{onlineUsers}**";
                // Context.Channel.SendMessageAsync(
                //    $"__*Server Name:*__ **{guildName}** __*Server Owner:*__ **{owner}** __*Total Users:*__ **{actualUsers}** __*Total Bots:*__ **{totalBots}** __*Total All:*__  **{totalUsers}**");

                if (!string.IsNullOrWhiteSpace(description))
                    builder.AddField(x =>
                    {
                        x.Name = $"__**{guildName}**__";
                        x.Value = description;
                        x.IsInline = false;
                    });
            }


            await ReplyAsync("", false, builder.Build());
        }
    }
}