﻿using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace brainKiller.Modules
{
    public class Moderation : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<Moderation> _logger;

        public Moderation(ILogger<Moderation> logger)
        {
            _logger = logger;
        }


        [Command("purge")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Deletes x amount of masseages from a channel. moderator perms only")]
        public async Task Purge(int amount)
        {
            var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);

            var message =
                await Context.Channel.SendMessageAsync($"{messages.Count() - 1} messages deleted successfully");
            await Task.Delay(2500);
            await message.DeleteAsync();
        }


        [Command("kick")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [Summary("Kicks a member from the server. kick perms required")]
        public async Task Kick([Remainder] SocketGuildUser user)
        {
            await ReplyAsync($"Cya {user.Mention}");
            await user.KickAsync();
        }
    }
}