using System;
using Discord;
using Discord.WebSocket;

namespace brainKiller.Common
{
    public class Mute
    {
        public DateTime End;
        public SocketGuild Guild;
        public IRole Role;
        public SocketGuildUser User;
    }
}