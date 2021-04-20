using Discord.Commands;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace brainKiller.Modules
{
    public class DevTesting : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<DevTesting> _logger;
        private readonly Servers _servers;

        public DevTesting(ILogger<DevTesting> logger, Servers servers)
        {
            _logger = logger;
            _servers = servers;
        }
    }
}