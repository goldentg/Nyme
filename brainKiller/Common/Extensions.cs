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

        public static async Task<IMessage> SendLogAsync(this ITextChannel channel, string title, string description)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(26, 155, 226))
                .WithDescription(description)
                .WithAuthor(author =>
                {
                    author
                        .WithIconUrl("https://i.imgur.com/gLR4k7d.png")
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
    }
}