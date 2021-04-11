using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using brainKiller.Common;
using brainKiller.Utilities;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Victoria;
using Victoria.EventArgs;

namespace brainKiller.Services
{
    public class CommandHandler : InitializedService
    {
        public static List<Mute> Mutes = new List<Mute>();
        private readonly AutoRolesHelper _autoRolesHelper;
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _config;
        private readonly Images _images;
        private readonly LavaNode _lavaNode;
        private readonly IServiceProvider _provider;
        private readonly ServerHelper _serverHelper;
        private readonly Servers _servers;
        private readonly CommandService _service;

        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service,
            IConfiguration config, Servers servers, Images images, AutoRolesHelper autoRolesHelper, LavaNode lavaNode,
            ServerHelper serverHelper)
        {
            _provider = provider;
            _client = client;
            _service = service;
            _config = config;
            _servers = servers;
            _images = images;
            _autoRolesHelper = autoRolesHelper;
            _lavaNode = lavaNode;
            _serverHelper = serverHelper;
        }

        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived += OnMessageReceived;

            _client.UserJoined += OnMemberJoin;

            _client.ChannelCreated += OnChannelCreated;

            _client.ChannelDestroyed += OnChannelDestroyed;

            _client.RoleCreated += OnRoleCreated;

            _client.RoleDeleted += OnRoleDeleted;

            _client.RoleUpdated += OnRoleUpdated;

            _client.UserBanned += OnUserBan;

            _client.UserUnbanned += OnUserUnban;

            _client.ChannelUpdated += OnChannelUpdated;

            _client.GuildUpdated += OnGuildUpdate;


            var newTask = new Task(async () => await MuteHandler());
            newTask.Start();

            _service.CommandExecuted += OnCommandExecuted;

            _lavaNode.OnTrackEnded += OnTrackEnded;

            _client.Ready += OnReadyAsync;

            _client.JoinedGuild += OnJoinedGuild;

            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        private async Task OnJoinedGuild(SocketGuild arg)
        {
            await arg.DefaultChannel.WelcomeGuildAsync(arg.Name);
        }

        private async Task OnGuildUpdate(SocketGuild arg1, SocketGuild arg2)
        {
            if (arg1 is SocketGuild gld1)
                if (arg2 is SocketGuild gld2)
                {
                    if (arg1.Name != arg2.Name)
                    {
                        var auditlogs = await arg2.GetAuditLogsAsync(1, null, arg1.Id, null, ActionType.GuildUpdated)
                            .FlattenAsync();
                        foreach (var audit in auditlogs)
                            if (audit.User is IUser data)
                                await _serverHelper.SendLogAsync(gld2, "Guild Updated",
                                    $"Guild **{gld1.Name}** has been updated to **{gld2.Name}** by **{audit.User.Username + "#" + audit.User.Discriminator}**");
                    }
                    else
                    {
                        var auditlogs = await arg2.GetAuditLogsAsync(1, null, arg1.Id, null, ActionType.EmojiCreated)
                            .FlattenAsync();
                        foreach (var audit in auditlogs)
                            if (audit.User is IUser data)
                                await _serverHelper.SendLogAsync(gld2, "Guild Updated",
                                    $"Guild **{gld1.Name}** has been updated by **{audit.User.Username + "#" + audit.User.Discriminator}**");
                    }
                }
        }


        // private async Task OnGuildUpdate(SocketGuild arg1, SocketGuild arg2)
        //  {
        // if (arg1 is SocketGuild gld1)
        //   if (arg2 is SocketGuild gld2)
        // {
        /*
        if (arg1.Name != arg2.Name)
        {
            var auditlogs = await arg2.GetAuditLogsAsync(1, null, arg1.Id, null, ActionType.GuildUpdated)
                .FlattenAsync();
            foreach (var audit in auditlogs)
                if (audit.User is IUser data)
                    await _serverHelper.SendLogAsync(gld2, "Guild Updated",
                        $"Guild **{gld1.Name}** has been updated to **{gld2.Name}** by **{audit.User.Username + "#" + audit.User.Discriminator}**");
        }
        else
        {
        */
        /*
        var auditlogs = await arg2.GetAuditLogsAsync(1, null, arg1.Id, null, ActionType.EmojiCreated)
            .FlattenAsync();
        foreach (var audit in auditlogs)
            if (audit.Data is IEmote data)
            {
                await _serverHelper.SendLogAsync(arg2, "Emote Added",
                    $"Emoji {audit.Data} Has been created");
                return;
            }
    }
        
    }
        */


        private async Task OnChannelCreated(SocketChannel arg)
        {
            if (arg is SocketGuildChannel guildChannel)
            {
                var g = guildChannel.Guild;
                var auditlogs = await g.GetAuditLogsAsync(1, null, null, null, ActionType.ChannelCreated)
                    .FlattenAsync();
                foreach (var audit in auditlogs)
                    if (audit.User is IUser data)
                        await _serverHelper.SendLogAsync(g, "Channel Created",
                            $"Channel **#{guildChannel.Name}** has been created by **{audit.User.Username + "#" + audit.User.Discriminator}**");
            }
        }

        private async Task OnChannelDestroyed(SocketChannel arg)
        {
            if (arg is SocketGuildChannel guildChannel)
            {
                var g = guildChannel.Guild;
                var auditlogs = await g.GetAuditLogsAsync(1, null, guildChannel.Id, null, ActionType.ChannelDeleted)
                    .FlattenAsync();
                foreach (var audit in auditlogs)
                    if (audit.User is IUser data)
                        await _serverHelper.SendLogAsync(g, "Channel Deleted",
                            $"Channel **#{guildChannel.Name}** has been deleted by __**{audit.User.Username + "#" + audit.User.Discriminator}**__");
            }
        }

        private async Task OnChannelUpdated(SocketChannel arg1, SocketChannel arg2)
        {
            if (arg2 is SocketGuildChannel guildChannel)
                if (arg1 is SocketGuildChannel gld2Channel)
                {
                    if (guildChannel.Name != gld2Channel.Name)
                    {
                        var g = guildChannel.Guild;
                        var auditlogs = await g.GetAuditLogsAsync(1, null, arg1.Id, null, ActionType.ChannelUpdated)
                            .FlattenAsync();


                        foreach (var audit in auditlogs)
                            if (audit.User is IUser data)
                                await _serverHelper.SendLogAsync(g, "Channel Updated",
                                    $"The **#{gld2Channel.Name}** channel has been updated to **#{guildChannel.Name}** by __**{audit.User.Username + "#" + audit.User.Discriminator}**__");
                    }
                    else
                    {
                        var g = guildChannel.Guild;
                        var auditlogs = await g.GetAuditLogsAsync(1, null, arg1.Id, null, ActionType.ChannelUpdated)
                            .FlattenAsync();


                        foreach (var audit in auditlogs)
                            if (audit.User is IUser data)
                                await _serverHelper.SendLogAsync(g, "Channel Updated",
                                    $"The **#{gld2Channel.Name}** channel has been updated by **{audit.User.Username + "#" + audit.User.Discriminator}**");
                    }
                }
        }

        private async Task OnUserBan(SocketUser arg1, SocketGuild arg2)
        {
            var auditlogs = await arg2.GetAuditLogsAsync(1, null, null, arg1.Id, ActionType.Ban)
                .FlattenAsync();
            foreach (var audit in auditlogs)
                if (audit.User is IUser data)
                    await _serverHelper.SendLogAsync(arg2, "User Banned",
                        $"User **{arg1.Username + "#" + arg1.Discriminator}** has been banned by **{audit.User.Username + "#" + audit.User.Discriminator}**");
        }

        private async Task OnUserUnban(SocketUser arg1, SocketGuild arg2)
        {
            var auditlogs = await arg2.GetAuditLogsAsync(1, null, null, arg1.Id, ActionType.Unban)
                .FlattenAsync();
            foreach (var audit in auditlogs)
                if (audit.User is IUser data)
                    await _serverHelper.SendLogAsync(arg2, "User Un-Banned",
                        $"User **{arg1.Username + "#" + arg1.Discriminator}** has been unbanned by __**{audit.User.Username + "#" + audit.User.Discriminator}**__");
        }

        private async Task OnRoleCreated(SocketRole arg)
        {
            var g = arg.Guild;
            var auditlogs = await g.GetAuditLogsAsync(1, null, null, null, ActionType.RoleCreated)
                .FlattenAsync();
            foreach (var audit in auditlogs)
                if (audit.User is IUser data)
                    await _serverHelper.SendLogAsync(g, "Role Created",
                        $"Role **{arg.Name}** has been created by __**{audit.User.Username + "#" + audit.User.Discriminator}**__");
        }

        private async Task OnRoleDeleted(SocketRole arg)
        {
            var g = arg.Guild;
            var auditlogs = await g.GetAuditLogsAsync(1, null, arg.Id, null, ActionType.RoleDeleted)
                .FlattenAsync();
            foreach (var audit in auditlogs)
                if (audit.User is IUser data)
                    await _serverHelper.SendLogAsync(g, "Role Deleted",
                        $"Role **{arg.Name}** has been deleted by __**{audit.User.Username + "#" + audit.User.Discriminator}**__");
        }

        private async Task OnRoleUpdated(SocketRole arg1, SocketRole arg2)
        {
            if (arg1.Name != arg2.Name)
            {
                var g = arg2.Guild;
                var auditlogs = await g.GetAuditLogsAsync(1, null, arg1.Id, null, ActionType.RoleUpdated)
                    .FlattenAsync();
                foreach (var audit in auditlogs)
                    if (audit.User is IUser data)
                        await _serverHelper.SendLogAsync(g, "Role Updated",
                            $"Role **{arg1.Name}** has been updated to **{arg2.Name}** by __**{audit.User.Username + "#" + audit.User.Discriminator}**__");
            }
            else
            {
                var g = arg2.Guild;
                var auditlogs = await g.GetAuditLogsAsync(1, null, arg1.Id, null, ActionType.RoleUpdated)
                    .FlattenAsync();
                foreach (var audit in auditlogs)
                    if (audit.User is IUser data)
                        await _serverHelper.SendLogAsync(g, "Role Updated",
                            $"Role **{arg2.Name}** has been updated by __**{audit.User.Username + "#" + audit.User.Discriminator}**__");
            }
        }

        private async Task MuteHandler()
        {
            var Remove = new List<Mute>();
            foreach (var mute in Mutes)
            {
                if (DateTime.Now < mute.End)
                    continue;

                var guild = _client.GetGuild(mute.Guild.Id);


                if (guild.GetRole(mute.Role.Id) == null)
                {
                    Remove.Add(mute);
                    continue;
                }

                var role = guild.GetRole(mute.Role.Id);

                if (guild.GetUser(mute.User.Id) == null)
                {
                    Remove.Add(mute);
                    continue;
                }

                var user = guild.GetUser(mute.User.Id);

                if (role.Position > guild.CurrentUser.Hierarchy)
                {
                    Remove.Add(mute);
                    continue;
                }

                await user.RemoveRoleAsync(mute.Role);
                Remove.Add(mute);
            }

            Mutes = Mutes.Except(Remove).ToList();

            await Task.Delay(1 * 60 * 1000);
            await MuteHandler();
        }


        private async Task OnReadyAsync()
        {
            await _client.SetGameAsync("!help for help");

            if (!_lavaNode.IsConnected) await _lavaNode.ConnectAsync();
        }


        private async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            if (!args.Reason.ShouldPlayNext()) return;

            var player = args.Player;
            if (!player.Queue.TryDequeue(out var queueable))
            {
                // await player.TextChannel.SendMessageAsync("Queue completed! Please add more tracks to rock n' roll!");
                await player.TextChannel.TextMusic("Queue Completed",
                    "Add more songs to the queue to keep the party going!",
                    "https://external-content.duckduckgo.com/iu/?u=http%3A%2F%2Ficons.iconarchive.com%2Ficons%2Fiynque%2Fios7-style%2F1024%2FMusic-icon.png&f=1&nofb=1");

                return;
            }

            if (!(queueable is LavaTrack track))
            {
                //await player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
                await player.TextChannel.SendErrorTextChannelAsync("Error",
                    "The next item in the queue is not a track");
                return;
            }

            await args.Player.PlayAsync(track);
            await args.Player.TextChannel.TextMusic("Now Playing:", track.Title,
                "https://external-content.duckduckgo.com/iu/?u=http%3A%2F%2Ficons.iconarchive.com%2Ficons%2Fiynque%2Fios7-style%2F1024%2FMusic-icon.png&f=1&nofb=1");
            //await args.Player.TextChannel.SendMessageAsync(
            //    $"{args.Reason}: {args.Track.Title}\nNow playing: {track.Title}");
        }

        private async Task OnMemberJoin(SocketGuildUser arg)
        {
            var newTask = new Task(async () => await HandleUserJoined(arg));
            newTask.Start();
            /*
            var guildId = await _servers.GetWelcomeDmAsync(arg.Guild.Id);
            var wlcmdmmsg = await _servers.GetDmMessageAsync(arg.Guild.Id);


            if (guildId == 0)
                return;

            var embed = new EmbedBuilder()
                .WithColor(new Color(43, 182, 115))
                .WithTitle("**Welcome**")
                .WithDescription(wlcmdmmsg)
                .WithCurrentTimestamp()
                .Build();

            if (arg is IUser iu)
            {
                var userch = await iu.GetOrCreateDMChannelAsync();
                try
                {
                    await userch.SendMessageAsync(embed: embed);
                }
                catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.Forbidden)
                {
                    Console.WriteLine($"Cannot DM {iu.Username + "#" + iu.Discriminator}.");
                }

                //iu.SendMessageAsync(embed: embed);
            }
            */
        }


        private async Task HandleUserJoined(SocketGuildUser arg)
        {
            var roles = await _autoRolesHelper.GetAutoRolesAsync(arg.Guild);
            if (roles.Count > 0)
                await arg.AddRolesAsync(roles);


            // var channelId = await _servers.GetWelcomeAsync(arg.Guild.Id);
            // if (channelId == 0)
            //    return;

            //  var channel = arg.Guild.GetTextChannel(channelId);
            //   if (channel == null)
            //  {
            //      await _servers.ClearWelcomeAsync(arg.Guild.Id);
            //        return;
            //    }

            //  var background = await _servers.GetBackgroundAsync(arg.Guild.Id) ??
            //                    "https://images.unsplash.com/photo-1500534623283-312aade485b7?ixid=MXwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHw%3D&ixlib=rb-1.2.1&auto=format&fit=crop&w=1350&q=80";
            //  var path = await _images.CreateImageAsync(arg, background);
            //  await channel.SendFileAsync(path, null);
            // File.Delete(path);
        }


        private async Task OnMessageReceived(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage message)) return;
            if (message.Channel is SocketDMChannel) return;
            if (message.Source != MessageSource.User) return;


            var argPos = 0;
            //if no value on left, it will use "value" as prefix instead
            var prefix = await _servers.GetGuildPrefix((message.Channel as SocketGuildChannel).Guild.Id) ?? "!";
            if (!message.HasStringPrefix(prefix, ref argPos) &&
                !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(_client, message);
            await _service.ExecuteAsync(context, argPos, _provider);
        }

        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (command.IsSpecified && !result.IsSuccess)
                await context.Channel.SendMessageAsync(
                    $"Error: {result}.\n*If you think this error is a bug or issue with the bot please let me know on the support server https://discord.gg/a5SmPbSGEJ*");
        }
    }
}