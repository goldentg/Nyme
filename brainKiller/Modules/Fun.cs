using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using brainKiller.API;
using brainKiller.Common;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

        [Command("slap", RunMode = RunMode.Async)]
        [Summary("Slap a member")]
        public async Task Slap([Remainder] SocketGuildUser user)
        {
            await Context.Channel.SlapAsync("Slap", $"{Context.User.Mention} slapped {user.Mention}");
        }

        [Command("coinflip", RunMode = RunMode.Async)]
        [Summary("Flip a coin")]
        public async Task CoinFlip()
        {
            string[] coin =
            {
                "Heads",
                "Tails"
            };

            var r = new Random();
            var index = r.Next(coin.Length);

            await Context.Channel.CoinflipMessageAsync($"{coin[index]}", $"{Context.User.Username}");
        }

        [Command("8ball", RunMode = RunMode.Async)]
        [Summary("Ask the magical 8-Ball for their wisdom\nYou must input your question")]
        public async Task eightBall([Remainder] string question = null)
        {
            if (question == null)
            {
                await Context.Channel.SendErrorAsync("Error", "You must include a question for the magical 8-Ball");
                return;
            }

            string[] responses =
            {
                //no responses
                "My reply is no",
                "Don’t count on it",
                "Very doubtful",
                "Outlook not so good",
                "My sources say no",

                //affirmative responses
                "Yes",
                "Most likely",
                "It is certain",
                "Signs point to yes",
                "Outlook good",
                "As I see it, yes",
                "Yes, definitely",
                "It is decidedly so",
                "Without a doubt",
                "You may rely on it",

                //non commital responses
                "Better not tell you now",
                "Reply hazy try again",
                "Concentrate and ask again",
                "Cannot predict now",
                "Ask again later"
            };
            // await ReplyAsync(responses[new Random().Next(0, responses.Count())]);
            var rndmResponse = responses[new Random().Next(0, responses.Count())];
            await Context.Channel.eightBallMsgAsync(question, rndmResponse, Context.User);
        }

        [Command("fact", RunMode = RunMode.Async)]
        [Summary("Sends a random fact")]
        public async Task Fact()
        {
            var client = new HttpClient();
            var url = "https://useless-facts.sameerkumar.website/api";
            var result = await client.GetStringAsync(url);
            var fact = JsonConvert.DeserializeObject<fact.Root>(result);
            await ReplyAsync(fact.Data);
        }

        [Command("chucknorris", RunMode = RunMode.Async)]
        [Alias("chuck")]
        [Summary("Sends a chuck norris fact\n({prefix}chuck can also be used)")]
        public async Task chuckNorris()
        {
            var client = new HttpClient();
            var url = "https://api.chucknorris.io/jokes/random";
            var result = await client.GetStringAsync(url);
            var post = JsonConvert.DeserializeObject<fact.norris>(result);
            await ReplyAsync(post.Value);
        }

        [Command("wikipedia", RunMode = RunMode.Async)]
        [Alias("wiki")]
        [Summary("Searches Article From Wikipedia")]
        public async Task SearchFromWikipediaAsync([Remainder] string wikisearch = null)
        {
            if (wikisearch == null)
            {
                await Context.Channel.SendErrorAsync("Error", "You must include a search phrase");
                return;
            }

            var client = new HttpClient();
            var json = await client.GetStringAsync(
                $"https://en.wikipedia.org/w/api.php?format=json&action=query&prop=extracts|pageimages&exintro&explaintext&titles={wikisearch}");

            var myPages = JsonConvert.DeserializeObject<wikisearchhelper.WRoot>(json);
            var first = myPages.Query.Pages.Values.First();
            var title = first.Title;
            //var thumbnail = first.Thumbnail.source;

            var extract = first.Extract.Substring(0, 500) + "...";

            var builder = new EmbedBuilder()
                .WithColor(new Color(33, 176, 255))
                //.WithThumbnailUrl(thumbnail)
                .AddField("Searched Article", wikisearch)
                .AddField("Found Article", title)
                .AddField($"Article about {title}", extract)
                .AddField("Wikipedia Url",
                    $"[https://en.wikipedia.org/wiki/{title}](https://en.wikipedia.org/wiki/{title})")
                .WithAuthor(Context.Client.CurrentUser)
                .WithFooter(footer =>
                    footer.Text = $"Searched by: {Context.User.Username + "#" + Context.User.Discriminator}")
                .WithCurrentTimestamp()
                .Build();
            await ReplyAsync(embed: builder);
        }
    }
}