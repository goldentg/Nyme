using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using brainKiller.Common;
using Discord;
using Discord.WebSocket;
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

        public async Task MessageBotJoinOwnerAsync(IUser user, IGuild guild)
        {
            var channel = await user.GetOrCreateDMChannelAsync();
            try
            {
                if (guild is SocketGuild sGuild) {
                    var totalUsers = sGuild.Users.Count();
                    var guildOwner = sGuild.Owner.Username + "#" + sGuild.Owner.Discriminator;
                    await channel.SendMessageAsync($"Nyme has joined a new guild!\nNew Guild: **{guild.Name}**\nTotal Users: **{totalUsers}**\nGuild Owner: **{guildOwner}**");
                }
            }
            catch (Discord.Net.HttpException ex) when (ex.HttpCode == HttpStatusCode.Forbidden)
            {
                Console.WriteLine($"I cannot message {user} on guild join.");
            }
        }

        public async Task MessageBotLeaveOwnerAsync(IUser user, IGuild guild)
        {
            var channel = await user.GetOrCreateDMChannelAsync();
            try
            {
                if (guild is SocketGuild sGuild)
                {
                    var totalUsers = sGuild.Users.Count();
                    var guildOwner = sGuild.Owner.Username + "#" + sGuild.Owner.Discriminator;
                    await channel.SendMessageAsync($"Nyme has been removed from a guild\nLeft Guild: **{guild.Name}**\nTotal Users: **{totalUsers}**\nGuild Owner: **{guildOwner}**");
                }
            }
            catch (Discord.Net.HttpException ex) when (ex.HttpCode == HttpStatusCode.Forbidden)
            {
                Console.WriteLine($"I cannot message {user} on guild leave.");
            }
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