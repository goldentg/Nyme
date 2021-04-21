using System.Threading.Tasks;
using brainKiller.Common;
using Discord;
using Infrastructure;

namespace brainKiller.Utilities
{
    public class ServerHelper
    {
        private readonly Servers _servers;

        public ServerHelper(Servers servers)
        {
            _servers = servers;
        }

        public async Task SendLogAsync(IGuild guild, string title, string description)
        {
            var channelId = await _servers.GetLogsAsync(guild.Id);
            if (channelId == 0)
                return;

            var fetchedChannel = await guild.GetTextChannelAsync(channelId);
            if (fetchedChannel == null)
            {
                await _servers.ClearLogsAsync(guild.Id);
                return;
            }

            await fetchedChannel.SendLogAsync(title, description);
        }

        /*
        public async Task GuildHasLogAsyncTask(IGuild guild)
        {
            var channelId = await _servers.GetLogsAsync(guild.Id);
            if (channelId == 0)
                return;

        }
        */
        /*
        public static async Task<IUserMessage> SendWelcomeDmAsync(IUserMessage user, string description)
        {
            //var guildId = await _servers.GetWelcomeDmAsync(id);

            var embed = new EmbedBuilder()
                .WithColor(new Color(43, 182, 115))
                .WithTitle($"**Welcome**")
                .WithDescription(description)
                .WithCurrentTimestamp()
                .Build();

           var msg =  await user.ReplyAsync(embed: embed);
           return msg;
        }
        */
    }
}