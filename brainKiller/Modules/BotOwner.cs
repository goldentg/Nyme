using brainKiller.Utilities;
using Discord.Commands;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace brainKiller.Modules
{
    public class BotOwner : ModuleBase<SocketCommandContext>
    {
        private readonly Images _images;
        private readonly ILogger<General> _logger;
        private readonly Servers _servers;

        public BotOwner(ILogger<General> logger, Servers servers, Images images)
        {
            _logger = logger;
            _servers = servers;
            _images = images;
        }
    }
}