using System.Net.Http;
using System.Threading.Tasks;
using brainKiller.Common;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace brainKiller.Modules
{
    public class Fun : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<Fun> _logger;

        public Fun(ILogger<Fun> logger)
        {
            _logger = logger;
        }

        [Command("meme", RunMode = RunMode.Async)]
        [Alias("reddit")]
        [Summary("sends a meme or an image from a specified subreddit")]
        public async Task Meme(string subreddit = null)
        {
            var client = new HttpClient();
            var result =
                await client.GetStringAsync($"https://reddit.com/r/{subreddit ?? "memes"}/random.json?limit=1");
            if (!result.StartsWith("["))
            {
                await Context.Channel.SendMessageAsync("This subbreddit does not exist");
                return;
            }

            var arr = JArray.Parse(result);
            var post = JObject.Parse(arr[0]["data"]["children"][0]["data"].ToString());

            var builder = new EmbedBuilder()
                .WithImageUrl(post["url"].ToString())
                .WithColor(new Color(33, 176, 252))
                .WithTitle(post["title"].ToString())
                .WithUrl("https://reddit.com" + post["permalink"])
                .WithFooter($"🗨 {post["num_comments"]} ⬆️ {post["ups"]}");
            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }

        [Command("hug", RunMode = RunMode.Async)]
        [Summary("Cheer a member up by giving them a hug")]
        public async Task Hug([Remainder] SocketGuildUser user)
        {
            await Context.Channel.HugAsync("Hugs", $"{Context.User.Mention} gave {user.Mention} a hug");
        }
    }
}