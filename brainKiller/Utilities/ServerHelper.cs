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
    }
}