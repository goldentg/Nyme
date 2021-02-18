using brainKiller.Utilities;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace brainKiller.Modules
{
    public class BotOwner : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<General> _logger;
        private readonly Servers _servers;
        private readonly Images _images;

        public BotOwner(ILogger<General> logger, Servers servers, Images images)
        {
            _logger = logger;
            _servers = servers;
            _images = images;
        }

        [Command("bruh")]
        [RequireOwner]
        public async Task Bruh(SocketUser user)
        {
            await Context.Channel.SendMessageAsync("Pong!");
        }

    }
}
