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
using Discord.Rest;
using Discord.WebSocket;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Victoria;
using Victoria.EventArgs;

namespace brainKiller.Services
{
    public class CommandHandler : InitializedService
    {
        public const ulong BotListBotId = 808888674900508723;
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

            // if (Program.BotListToken == null) return;

            // await BotList.Instantiate(BotListBotId, Program.BotListToken);

            //if (!Program.IsBotListBot) return;
            //  await Program.BotList.ThisBot.UpdateStatsAsync(_client.Guilds.Count);
        }


        private async Task OnGuildUpdate(SocketGuild arg1, SocketGuild arg2)
        {
            if (arg2 is IGuild ig)
            {
                var channelId = await _servers.GetLogsAsync(arg2.Id);
                var chnlIdInt = Convert.ToInt64(channelId);
                if (chnlIdInt == 0) return;
                if (!arg2.CurrentUser.GuildPermissions.ViewAuditLog)
                {
                    await _serverHelper.SendLogAsync(arg2, "Bot Does Not Have Sufficient Permissions",
                        "A event logger has failed its task because I do\nnot have `View Audit Log` permissions. To solve this error please\ngive me that permission");
                    return;
                }

                var auditlogs = await arg2.GetAuditLogsAsync(1, null, arg1.Id, null, ActionType.GuildUpdated)
                    .FlattenAsync();


                foreach (var audit in auditlogs)
                    if (audit.User is IUser data && audit.Data is GuildUpdateAuditLogData d1)
                    {
                        //if (ReferenceEquals(d1.After.Name, d1.After.Name))      //Check for guild name change
                        // {
                        //     await _serverHelper.SendLogAsync(arg2, "Guild Name Changed",
                        //        $"The guild name has been changed to `{ig.Name}` by {audit.User.Mention}");
                        //    return;
                        //} else 
                        if (ReferenceEquals(d1.After.Owner.Id, d1.Before.Owner.Id)
                        ) //Check for server ownership transfer
                        {
                            await _serverHelper.SendLogAsync(arg2, "Guild Ownership Transfered",
                                $"{audit.User.Mention} has transfered this guilds ownership to {arg2.Owner.Mention}");
                            return;
                        }

                        if (ReferenceEquals(d1.After.IconHash, d1.Before.IconHash)) //Check for guild icon change
                        {
                            await _serverHelper.SendLogAsync(arg2, "Guild Icon Changed",
                                $"{audit.User.Mention} has changed the guild icon to {ig.IconUrl}");
                            return;
                        }

                        if (ReferenceEquals(d1.After.AfkChannelId.ToString(), d1.Before.AfkChannelId.ToString())
                        ) //Check if AFK channel moved
                        {
                            await _serverHelper.SendLogAsync(arg2, "AFK Channel Changed",
                                $"The AFK channel has been changed to `{arg2.AFKChannel.Name}` by {audit.User.Mention}");
                            return;
                        }

                        if (ReferenceEquals(d1.After.AfkTimeout.Value.ToString(),
                            d1.After.AfkTimeout.Value.ToString())) //Check if AFK timeout has changed
                        {
                            await _serverHelper.SendLogAsync(arg2, "AFK Timeout Has Been Modified",
                                $"AFK Timeout has been changed to `{ig.AFKTimeout.ToString()} seconds` by {audit.User.Mention}");
                            return;
                        }

                        if (ReferenceEquals(d1.After.RegionId, d1.Before.RegionId)
                        ) //Check if guild server region has been changed
                        {
                            await _serverHelper.SendLogAsync(arg2, "Guild Server Region Changed",
                                $"This guilds server region has been changed to by {audit.User.Mention}");
                            return;
                        }

                        if (ReferenceEquals(d1.After.VerificationLevel.Value, d1.Before.VerificationLevel.Value)
                        ) //Check if verification level has been changed 
                        {
                            await _serverHelper.SendLogAsync(arg2, "Guild Verification Level Modified",
                                $"{audit.User.Mention} has changed the guild verification level to `{ig.VerificationLevel.ToString()}`");
                            return;
                        }

                        if (ReferenceEquals(d1.After.MfaLevel.Value, d1.Before.MfaLevel.Value)
                        ) //Check if Multi-Factor Authentication protocol has been modified
                        {
                            await _serverHelper.SendLogAsync(arg2, "Multi-Factor Authentication Policy Modified",
                                $"{audit.User.Mention} has modified the Multi-Factor Authentication policy to `{ig.MfaLevel.ToString()}`");
                            return;
                        }

                        if (ReferenceEquals(d1.After.SystemChannelId.Value.ToString(),
                            d1.Before.SystemChannelId.Value.ToString())) //Check if System Channel has been changed
                        {
                            await _serverHelper.SendLogAsync(arg2, "System Channel Changed",
                                $"The system Channel has been modified to `{ig.SystemChannelId.Value}` by {audit.User.Mention}");
                            return;
                        }

                        if (ReferenceEquals(d1.After.ExplicitContentFilter.Value.ToString(),
                            d1.Before.ExplicitContentFilter.Value.ToString())
                        ) //Check if explit content filter has been modified
                            await _serverHelper.SendLogAsync(arg2, "Explit Content Filter Has Been Modified",
                                $"The explit content filer has been changed to `{ig.ExplicitContentFilter.ToString()}`");
                    }
            }
        }


        /*
        private async Task OnGuildUpdate(SocketGuild arg1, SocketGuild arg2)
        {
            if (arg1 is SocketGuild gld1)
                if (arg2 is SocketGuild gld2)
                {
                    var channelId = await _servers.GetLogsAsync(gld2.Id);
                    if (channelId == 0) return;
                    if (!gld2.CurrentUser.GuildPermissions.ViewAuditLog)
                    {
                        await _serverHelper.SendLogAsync(gld2, "Bot Does Not Have Sufficient Permissions",
                            "A event logger has failed its task because I do\nnot have `View Audit Log` permissions. To solve this error please\ngive me that permission");
                        return;
                    }

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
        */
        /*
        private static async void RefreshBotListDocs()
        {
            if (BotListToken == null)
            {
                return;
            }

            BotList = await BotList.Instantiate(BotListBotId, BotListToken);

            if (!IsBotListBot)
            {
                return;
            }


            //_client.JoinedNewGuild += () => { BotList.ThisBot.UpdateStatsAsync(Client.Guilds.Count); };
        }
        */

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
                var channelId = await _servers.GetLogsAsync(g.Id);
                var chnlIdInt = Convert.ToInt64(channelId);
                if (chnlIdInt == 0) return;
                if (!g.CurrentUser.GuildPermissions.ViewAuditLog)
                {
                    await _serverHelper.SendLogAsync(g, "Bot Does Not Have Sufficient Permissions",
                        "A event logger has failed its task because I do\nnot have `View Audit Log` permissions. To solve this error please\ngive me that permission");
                    return;
                }

                if (arg is ITextChannel)
                {
                    var auditlogs = await g.GetAuditLogsAsync(1, null, null, null, ActionType.ChannelCreated)
                        .FlattenAsync();
                    foreach (var audit in auditlogs)
                        if (audit.User is IUser data)
                            // await _serverHelper.SendLogAsync(g, "Channel Created",
                            //   $"Channel **#{guildChannel.Name}** has been created by **{audit.User.Username + "#" + audit.User.Discriminator}**");
                            await _serverHelper.SendLogAsync(g, "Channel Created",
                                $"Channel `#{guildChannel.Name}` has been created by {audit.User.Mention}");
                }
                else
                {
                    // var g = guildChannel.Guild;
                    var auditlogs = await g.GetAuditLogsAsync(1, null, null, null, ActionType.ChannelCreated)
                        .FlattenAsync();
                    foreach (var audit in auditlogs)
                        if (audit.User is IUser data)
                            // await _serverHelper.SendLogAsync(g, "Channel Created",
                            //   $"Channel **#{guildChannel.Name}** has been created by **{audit.User.Username + "#" + audit.User.Discriminator}**");
                            await _serverHelper.SendLogAsync(g, "Channel Created",
                                $"Voice Channel `{guildChannel.Name}` has been created by {audit.User.Mention}");
                }
            }
        }

        private async Task OnChannelDestroyed(SocketChannel arg)
        {
            if (arg is SocketGuildChannel guildChannel)
            {
                var g = guildChannel.Guild;
                var channelId = await _servers.GetLogsAsync(g.Id);
                var chnlIdInt = Convert.ToInt64(channelId);
                if (chnlIdInt == 0) return;
                if (!g.CurrentUser.GuildPermissions.ViewAuditLog)
                {
                    await _serverHelper.SendLogAsync(g, "Bot Does Not Have Sufficient Permissions",
                        "A event logger has failed its task because I do\nnot have `View Audit Log` permissions. To solve this error please\ngive me that permission");
                    return;
                }

                if (arg is ITextChannel)
                {
                    var auditlogs = await g.GetAuditLogsAsync(1, null, guildChannel.Id, null, ActionType.ChannelDeleted)
                        .FlattenAsync();
                    foreach (var audit in auditlogs)
                        if (audit.User is IUser data)
                            await _serverHelper.SendLogAsync(g, "Channel Deleted",
                                $"Channel `#{guildChannel.Name}` has been deleted by{audit.User.Mention}");
                }
                else
                {
                    // var g = guildChannel.Guild;
                    var auditlogs = await g.GetAuditLogsAsync(1, null, guildChannel.Id, null, ActionType.ChannelDeleted)
                        .FlattenAsync();
                    foreach (var audit in auditlogs)
                        if (audit.User is IUser data)
                            await _serverHelper.SendLogAsync(g, "Channel Deleted",
                                $"Voice Channel `{guildChannel.Name}` has been deleted by {audit.User.Mention}");
                }
            }
        }


        private async Task OnChannelUpdated(SocketChannel arg1, SocketChannel arg2)
        {
            if (arg2 is SocketGuildChannel guildChannel)
                if (arg1 is SocketGuildChannel gld2Channel)
                {
                    var g = guildChannel.Guild;


                    var channelId = await _servers.GetLogsAsync(g.Id);
                    var chnlIdInt = Convert.ToInt64(channelId);
                    if (chnlIdInt == 0) return;

                    if (!g.CurrentUser.GuildPermissions.ViewAuditLog)
                    {
                        await _serverHelper.SendLogAsync(g, "Bot Does Not Have Sufficient Permissions",
                            "A event logger has failed its task because I do\nnot have `View Audit Log` permissions. To solve this error please\ngive me that permission");
                        return;
                    }

                    var auditlogs = await g.GetAuditLogsAsync(1, null, arg1.Id, null, ActionType.ChannelUpdated)
                        .FlattenAsync();


                    if (arg2 is ITextChannel)
                    {
                        foreach (var audit in auditlogs)
                            if (audit.User is IUser && audit.Data is ChannelUpdateAuditLogData d1)

                                if (d1.After.Name.ToLowerInvariant() != d1.Before.Name.ToLowerInvariant()
                                ) //Check for channel name changes
                                {
                                    await _serverHelper.SendLogAsync(g, "Channel Name Updated",
                                        $"The `#{gld2Channel.Name}` channel has been updated to `#{guildChannel.Name}` by {audit.User.Mention}");
                                    return;
                                }
                                else if (ReferenceEquals(d1.Before.Topic, d1.After.Topic)) //Check for topic changes
                                {
                                    if (arg2 is ITextChannel itChannel)
                                        await _serverHelper.SendLogAsync(g, "Channel Topic Updated",
                                            $"The `#{gld2Channel.Name}` channel's topic has been changed to `{itChannel.Topic}` by {audit.User.Mention}");
                                    return;
                                }
                                else if (ReferenceEquals(d1.Before.IsNsfw,
                                    d1.After.IsNsfw)
                                ) //Check for NSFW toggle change
                                {
                                    await _serverHelper.SendLogAsync(g, "Channel Toggled NSFW",
                                        $"The `#{gld2Channel.Name} channel's NSFW has been toggled by {audit.User.Mention}`");
                                    return;
                                }
                                else if (ReferenceEquals(d1.Before.SlowModeInterval.Value.ToString(),
                                    d1.After.SlowModeInterval.Value.ToString())) //Check for slowmode changes
                                {
                                    await _serverHelper.SendLogAsync(g,
                                        "Slowmode Modified",
                                        $"The slowmode for `#{gld2Channel.Name}` has been modified by {audit.User.Mention}");
                                    return;
                                }
                    }
                    else
                    {
                        foreach (var audit in auditlogs)
                            if (audit.User is IUser && audit.Data is ChannelUpdateAuditLogData d1)
                                if (arg2 is SocketVoiceChannel vc)
                                    //    if (d1.After.Name != d1.Before.Name)
                                    //   {
                                    //     await _serverHelper.SendLogAsync(g, "Voice Channel Name Updated",
                                    //         $"The `{gld2Channel.Name}` voice channel has been updated to `{guildChannel.Name}` by {audit.User.Mention}");
                                    //     return;
                                    //  }

                                    if (ReferenceEquals(d1.After.Bitrate,
                                        d1.Before.Bitrate))
                                    {
                                        await _serverHelper.SendLogAsync(g, "Voice Channel Bitrate Changed",
                                            $"The bitrate for the `{gld2Channel.Name}` voice channel has been changed to `{vc.Bitrate / 1000}kbps` by {audit.User.Mention}`");
                                        return;
                                    }
                    }
                }
        }


        /*
        private async Task OnChannelUpdated(SocketChannel arg1, SocketChannel arg2)
        {
            if (arg2 is SocketGuildChannel guildChannel)
                if (arg1 is SocketGuildChannel gld2Channel)
                {
                    var g = guildChannel.Guild;
                    var channelId = await _servers.GetLogsAsync(g.Id);
                    if (channelId == 0) return;
                    if (!g.CurrentUser.GuildPermissions.ViewAuditLog)
                    {
                        await _serverHelper.SendLogAsync(g, "Bot Does Not Have Sufficient Permissions",
                            "A event logger has failed its task because I do\nnot have `View Audit Log` permissions. To solve this error please\ngive me that permission");
                        return;
                    }
                    // static GuildUpdateAuditLogData getGuildAuditPermission(IUser usr);

                    if (arg2 is ITextChannel)
                    {
                        var auditlogs = await g.GetAuditLogsAsync(1, null, arg1.Id, null, ActionType.ChannelUpdated)
                            .FlattenAsync();
                        /*
                        if (guildChannel.Name != gld2Channel.Name)
                        {
                            foreach (var audit in auditlogs)
                                if (audit.User is IUser data)
                                    await _serverHelper.SendLogAsync(g, "Channel Updated",
                                        $"The `#{gld2Channel.Name}` channel has been updated to `#{guildChannel.Name}` by {audit.User.Mention}");
                            return;
                        }
                        */
        /*

                        foreach (var audit in auditlogs)
                            if (audit.User is IUser && audit.Data is ChannelUpdateAuditLogData d1)
                                if (d1.After.Name != d1.Before.Name)
                                {
                                    await _serverHelper.SendLogAsync(g, "Channel Name Changed",
                                        $"The `#{gld2Channel.Name}` channel has been updated to `#{guildChannel.Name}` by {audit.User.Mention}");
                                    return;
                                }
                                else

                                {
                                    foreach (var audit1 in auditlogs)
                                        if (audit1.User is IUser && audit1.Data is ChannelUpdateAuditLogData d11)

                                            if (d11.After.Topic != d11.Before.Topic)
                                            {
                                                await _serverHelper.SendLogAsync(g, "Channel Topic Updated",
                                                    $"The `#{gld2Channel.Name}` channel's topic has been changed to `{d11.After.Topic}` by {audit1.User.Mention}");
                                                return;
                                            }
                                            else


                                            {
                                                foreach (var audit2 in auditlogs)
                                                    if (audit2.User is IUser &&
                                                        audit2.Data is ChannelUpdateAuditLogData d12)
                                                        if (d12.After.IsNsfw != d12.Before.IsNsfw)
                                                        {
                                                            await _serverHelper.SendLogAsync(g, "Channel Made NSFW",
                                                                $"The `#{gld2Channel.Name} channel has been made NSFW by {audit2.User.Mention}`");
                                                            return;
                                                        }
                                                        else

                                                        {
                                                            foreach (var audit3 in auditlogs)
                                                                if (audit3.User is IUser &&
                                                                    audit3.Data is ChannelUpdateAuditLogData d13)
                                                                    if (d13.After.SlowModeInterval !=
                                                                        d13.Before.SlowModeInterval)
                                                                    {
                                                                        await _serverHelper.SendLogAsync(g,
                                                                            "Slowmode Modified",
                                                                            $"The slowmode for `#{gld2Channel.Name}` has been modified by {audit3.User.Mention}");
                                                                        return;
                                                                    }
                                                        }
                                            }
                                }

                        {
                            //var g = guildChannel.Guild;
                            // var auditlogs = await g.GetAuditLogsAsync(1, null, arg1.Id, null, ActionType.ChannelUpdated)
                            //   .FlattenAsync();


                            foreach (var audit in auditlogs)
                                if (audit.User is IUser data && audit.Data is ChannelUpdateAuditLogData d2)
                                    await _serverHelper.SendLogAsync(g, "Channel Updated",
                                        $"The `#{gld2Channel.Name}` channel has been updated by {audit.User.Mention} Action: {d2.After}");
                        }
                    }
                    else
                    {
                        if (guildChannel.Name != gld2Channel.Name)
                        {
                            //var g = guildChannel.Guild;
                            var auditlogs = await g.GetAuditLogsAsync(1, null, arg1.Id, null, ActionType.ChannelUpdated)
                                .FlattenAsync();


                            foreach (var audit in auditlogs)
                                if (audit.User is IUser data)
                                    await _serverHelper.SendLogAsync(g, "Channel Updated",
                                        $"The `{gld2Channel.Name}` voice channel has been updated to `{guildChannel.Name}` by {audit.User.Mention}");
                        }
                        else
                        {
                            // var g = guildChannel.Guild;
                            var auditlogs = await g.GetAuditLogsAsync(1, null, arg1.Id, null, ActionType.ChannelUpdated)
                                .FlattenAsync();


                            foreach (var audit in auditlogs)
                                if (audit.User is IUser data)
                                    await _serverHelper.SendLogAsync(g, "Channel Updated",
                                        $"The `{gld2Channel.Name}` voice channel has been updated by {audit.User.Mention}");
                        }
                    }
                }
        }
        */


        private async Task OnUserBan(SocketUser arg1, SocketGuild arg2)
        {
            var channelId = await _servers.GetLogsAsync(arg2.Id);
            var chnlIdInt = Convert.ToInt64(channelId);
            if (chnlIdInt == 0) return;
            if (!arg2.CurrentUser.GuildPermissions.ViewAuditLog)
            {
                await _serverHelper.SendLogAsync(arg2, "Bot Does Not Have Sufficient Permissions",
                    "A event logger has failed its task because I do\nnot have `View Audit Log` permissions. To solve this error please\ngive me that permission");
                return;
            }

            var auditlogs = await arg2.GetAuditLogsAsync(1, null, null, null, ActionType.Ban)
                .FlattenAsync();
            foreach (var audit in auditlogs)
                if (audit.User is IUser data)
                    await _serverHelper.SendLogAsync(arg2, "User Banned",
                        $"User `{arg1.Username + "#" + arg1.Discriminator}` has been banned by {audit.User.Mention} Reason: `{audit.Reason}`");
        }

        private async Task OnUserUnban(SocketUser arg1, SocketGuild arg2)
        {
            var channelId = await _servers.GetLogsAsync(arg2.Id);
            var chnlIdInt = Convert.ToInt64(channelId);
            if (chnlIdInt == 0) return;
            if (!arg2.CurrentUser.GuildPermissions.ViewAuditLog)
            {
                await _serverHelper.SendLogAsync(arg2, "Bot Does Not Have Sufficient Permissions",
                    "A event logger has failed its task because I do\nnot have `View Audit Log` permissions. To solve this error please\ngive me that permission");
                return;
            }

            var auditlogs = await arg2.GetAuditLogsAsync(1, null, null, null, ActionType.Unban)
                .FlattenAsync();
            foreach (var audit in auditlogs)
                if (audit.User is IUser data)
                    await _serverHelper.SendLogAsync(arg2, "User Un-Banned",
                        $"User {arg1.Mention} has been unbanned by {audit.User.Mention}");
        }

        private async Task OnRoleCreated(SocketRole arg)
        {
            var g = arg.Guild;
            var channelId = await _servers.GetLogsAsync(g.Id);
            var chnlIdInt = Convert.ToInt64(channelId);
            if (chnlIdInt == 0) return;
            if (!g.CurrentUser.GuildPermissions.ViewAuditLog)
            {
                await _serverHelper.SendLogAsync(g, "Bot Does Not Have Sufficient Permissions",
                    "A event logger has failed its task because I do\nnot have `View Audit Log` permissions. To solve this error please\ngive me that permission");
                return;
            }

            var auditlogs = await g.GetAuditLogsAsync(1, null, null, null, ActionType.RoleCreated)
                .FlattenAsync();
            foreach (var audit in auditlogs)
                if (audit.User is IUser data)
                    await _serverHelper.SendLogAsync(g, "Role Created",
                        $"Role {arg.Mention} has been created by {audit.User.Mention}");
        }

        private async Task OnRoleDeleted(SocketRole arg)
        {
            var g = arg.Guild;
            var channelId = await _servers.GetLogsAsync(g.Id);
            var chnlIdInt = Convert.ToInt64(channelId);
            if (chnlIdInt == 0) return;
            if (!g.CurrentUser.GuildPermissions.ViewAuditLog)
            {
                await _serverHelper.SendLogAsync(g, "Bot Does Not Have Sufficient Permissions",
                    "A event logger has failed its task because I do\nnot have `View Audit Log` permissions. To solve this error please\ngive me that permission");
                return;
            }

            var auditlogs = await g.GetAuditLogsAsync(1, null, null, null, ActionType.RoleDeleted)
                .FlattenAsync();
            foreach (var audit in auditlogs)
                if (audit.User is IUser data)
                    await _serverHelper.SendLogAsync(g, "Role Deleted",
                        $"Role `{arg.Name}` has been deleted by {audit.User.Mention}");
        }

        private async Task OnRoleUpdated(SocketRole arg1, SocketRole arg2)
        {
            var g = arg2.Guild;
            var channelId = await _servers.GetLogsAsync(g.Id);
            var chnlIdInt = Convert.ToInt64(channelId);
            if (chnlIdInt == 0) return;
            if (!g.CurrentUser.GuildPermissions.ViewAuditLog)
            {
                await _serverHelper.SendLogAsync(g, "Bot Does Not Have Sufficient Permissions",
                    "A event logger has failed its task because I do\nnot have `View Audit Log` permissions. To solve this error please\ngive me that permission");
                return;
            }

            var auditlogs = await g.GetAuditLogsAsync(1, null, null, null, ActionType.RoleUpdated)
                .FlattenAsync();
            foreach (var audit in auditlogs)
                if (audit.User is IUser data && audit.Data is RoleUpdateAuditLogData d1)
                    if (ReferenceEquals(d1.After.Permissions.Value.ToString(),
                        d1.Before.Permissions.Value.ToString())) //Check for modified permissions for the role
                    {
                        await _serverHelper.SendLogAsync(g, "Role Permissions Modified",
                            $"Permissions for the {arg2.Mention} role have been modified by {audit.User.Mention}");
                        return;
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