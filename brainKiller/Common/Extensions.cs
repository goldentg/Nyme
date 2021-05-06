using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace brainKiller.Common
{
    public static class Extensions
    {
        public static async Task<IMessage> SendSuccessAsync(this ISocketMessageChannel channel, string title,
            string description)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(43, 182, 115))
                .WithDescription(description)
                .WithAuthor(author =>
                {
                    author
                        .WithIconUrl(
                            "https://icons-for-free.com/iconfiles/png/512/complete+done+green+success+valid+icon-1320183462969251652.png")
                        .WithName(title);
                })
                .Build();

            var message = await channel.SendMessageAsync(embed: embed);
            return message;
        }

        public static async Task<IMessage> SendErrorAsync(this ISocketMessageChannel channel, string title,
            string description)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(231, 76, 60))
                .WithDescription(description)
                .WithAuthor(author =>
                {
                    author
                        .WithIconUrl(
                            "https://icons.iconarchive.com/icons/paomedia/small-n-flat/1024/sign-error-icon.png")
                        .WithName(title);
                })
                .Build();

            var message = await channel.SendMessageAsync(embed: embed);
            return message;
        }

        public static async Task<IMessage> SendErrorTextChannelAsync(this ITextChannel channel, string title,
            string description)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(231, 76, 60))
                .WithDescription(description)
                .WithAuthor(author =>
                {
                    author
                        .WithIconUrl(
                            "https://icons.iconarchive.com/icons/paomedia/small-n-flat/1024/sign-error-icon.png")
                        .WithName(title);
                })
                .Build();

            var message = await channel.SendMessageAsync(embed: embed);
            return message;
        }

        public static async Task<IMessage> SendSuccessTextChannelAsync(this ITextChannel channel, string title,
            string description)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(43, 182, 115))
                .WithDescription(description)
                .WithAuthor(author =>
                {
                    author
                        .WithIconUrl(
                            "https://icons-for-free.com/iconfiles/png/512/complete+done+green+success+valid+icon-1320183462969251652.png")
                        .WithName(title);
                })
                .Build();

            var message = await channel.SendMessageAsync(embed: embed);
            return message;
        }

        public static async Task<IMessage> Music(this ISocketMessageChannel channel, string title, string song,
            string thumb)
        {
            if (thumb == null)
                thumb =
                    "https://external-content.duckduckgo.com/iu/?u=http%3A%2F%2Ficons.iconarchive.com%2Ficons%2Fiynque%2Fios7-style%2F1024%2FMusic-icon.png&f=1&nofb=1";
            var embed = new EmbedBuilder()
                .WithColor(new Color(91, 7, 175))
                .WithDescription(song)
                .WithTitle(title)
                .WithThumbnailUrl(thumb)
                .Build();

            var message = await channel.SendMessageAsync(embed: embed);
            return message;
        }

        public static async Task<IMessage> TextMusic(this ITextChannel channel, string title, string song, string thumb)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(91, 7, 175))
                .WithDescription(song)
                .WithTitle(title)
                .WithThumbnailUrl(thumb)
                .Build();

            var message = await channel.SendMessageAsync(embed: embed);
            return message;
        }

        public static async Task<IMessage> SendStats(this ISocketMessageChannel channel, string title,
            string description)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(43, 182, 115))
                .WithDescription(description)
                .WithAuthor(author =>
                {
                    author
                        .WithIconUrl(
                            "https://external-content.duckduckgo.com/iu/?u=https%3A%2F%2Fbeeimg.com%2Fimages%2Fa70879147153.png&f=1&nofb=1")
                        .WithName(title);
                })
                .Build();

            var message = await channel.SendMessageAsync(embed: embed);
            return message;
        }

        public static async Task<IMessage> HugAsync(this ISocketMessageChannel channel, string title,
            string description)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(43, 182, 115))
                .WithDescription(description)
                .WithTitle(title)
                .WithThumbnailUrl(
                    "https://external-content.duckduckgo.com/iu/?u=https%3A%2F%2Fstatic.thenounproject.com%2Fpng%2F1580530-200.png&f=1&nofb=1")
                .Build();

            var message = await channel.SendMessageAsync(embed: embed);
            return message;
        }

        public static async Task<IMessage> SlapAsync(this ISocketMessageChannel channel, string title,
            string description)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(43, 182, 115))
                .WithDescription(description)
                .WithTitle(title)
                .WithThumbnailUrl(
                    "https://external-content.duckduckgo.com/iu/?u=https%3A%2F%2Fstatic.thenounproject.com%2Fpng%2F641819-200.png&f=1&nofb=1")
                .Build();

            var message = await channel.SendMessageAsync(embed: embed);
            return message;
        }

        public static async Task<IMessage> SendLogAsync(this ITextChannel channel, string title,
            string description)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(26, 155, 233))
                .WithDescription(description)
                .WithAuthor(author =>
                {
                    author
                        .WithIconUrl(
                            "https://www.shareicon.net/data/2016/12/12/862832_internet_512x512.png")
                        .WithName(title);
                })
                .Build();

            var message = await channel.SendMessageAsync(embed: embed);
            return message;
        }

        public static async Task<IMessage> CoinflipMessageAsync(this ISocketMessageChannel channel, string output,
            string author)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(216, 206, 13))
                .WithTitle("Coinflip")
                .WithDescription($"The coin landed on __**{output}**__")
                .WithThumbnailUrl("http://clipart-library.com/data_images/296207.png")
                .WithCurrentTimestamp()
                .WithFooter($"Coin flipped by {author}")
                .Build();

            var message = await channel.SendMessageAsync(embed: embed);
            return message;
        }

        public static async Task<IMessage> WelcomeGuildAsync(this ISocketMessageChannel channel, string guildName)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(43, 182, 115))
                .WithTitle($"Hello {guildName}!")
                .WithDescription(
                    $"Hello members and staff of {guildName}. Thank you for\ngiving Nyme a try. Nyme is an extremely advanced and fast public discord bot packed with features.\nNyme is being worked upon and improved often and will continue to be. You can see\nsome of what Nyme has to offer by running **!help**\aalthough Nyme has a lot more to offer but you can read that in the [documentation](https://top.gg/bot/808888674900508723)\nif you are interested. I recommend that staff of {guildName} read the Nyme documentation before doing any configuration\n\n[Nyme Documentation](https://top.gg/bot/808888674900508723) [Support Server](https://discord.gg/RJ4kMhKvM3)")
                .WithThumbnailUrl(
                    "https://images.discordapp.net/avatars/808888674900508723/ba4ef16cc7e6905bde2a91f7cab19b3e.png?size=128")
                .WithCurrentTimestamp()
                .WithFooter(
                    "Developed by BlackLung#6950")
                .Build();

            var message = await channel.SendMessageAsync(embed: embed);
            return message;
        }

        public static async Task<IUserMessage> WelcomeDm(this IUserMessage user, IGuild guild, string msg)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(43, 182, 115))
                .WithTitle($"**Welcome to {guild.Name}**")
                .WithDescription(msg)
                .WithThumbnailUrl(guild.IconUrl)
                .WithCurrentTimestamp()
                .Build();

            var message = await user.ReplyAsync(embed: embed);
            return message;
        }

        public static async Task<IMessage> eightBallMsgAsync(this ISocketMessageChannel channel, string question,
            string eightResponse,
            IUser user)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(0, 0, 0))
                .WithTitle("Magic 8-Ball")
                .WithDescription(
                    $"{user.Mention}'s Question For The Magical 8-Ball: `{question}`\n\nThe Magical 8-Ball's Response Is: `{eightResponse}\n`")
                .WithFooter($"8-Ball response for: {user.Username + "#" + user.Discriminator}")
                .WithThumbnailUrl(
                    "https://external-content.duckduckgo.com/iu/?u=https%3A%2F%2F1001freedownloads.s3.amazonaws.com%2Fvector%2Fthumb%2F110366%2F8_Ball.png&f=1&nofb=1")
                .WithCurrentTimestamp()
                .Build();

            var message = await channel.SendMessageAsync(embed: embed);
            return message;
        }
    }
}