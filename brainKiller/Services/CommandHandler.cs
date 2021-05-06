using System;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens;
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
            // _disconnectTokens = disconnectTokens;
            _disconnectTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();
        }


        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived += OnMessageReceived;

            _client.ChannelCreated += OnChannelCreated;

            _client.ChannelDestroyed += OnChannelDestroyed;

            _client.RoleCreated += OnRoleCreated;

            _client.RoleDeleted += OnRoleDeleted;

            _client.RoleUpdated += OnRoleUpdated;

            _client.UserBanned += OnUserBan;

            _client.UserJoined += OnMemberJoin;

            _client.UserLeft += OnUserLeft;

            // _client.GuildMemberUpdated += OnGuildMemberUpdated;

            _client.UserUnbanned += OnUserUnban;

            _client.ChannelUpdated += OnChannelUpdated;

            _client.MessageUpdated += OnMessageUpdate;

            _client.GuildUpdated += OnGuildUpdate;

            _client.InviteCreated += OnInviteCreated;

            _client.InviteDeleted += OnInviteDeleted;

            var newTask = new Task(async () => await MuteHandler());
            newTask.Start();

            _service.CommandExecuted += OnCommandExecuted;

            _lavaNode.OnTrackEnded += OnTrackEnded;

            _client.Ready += OnReadyAsync;

            _client.JoinedGuild += OnJoinedGuild;

            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        private async Task OnInviteDeleted(SocketGuildChannel arg1, string arg2)
        {
            if (arg1 is ITextChannel argTextChannel)
            {
                var g = arg1.Guild;
                var channelId = await _servers.GetLogsAsync(g.Id);
                var chnlIdInt = Convert.ToInt64(channelId);
                if (chnlIdInt == 0) return;
                if (!g.CurrentUser.GuildPermissions.ViewAuditLog)
                {
                    await _serverHelper.SendLogAsync(g, "Bot Does Not Have Sufficient Permissions",
                        "A event logger has failed its task because I do\nnot have `View Audit Log` permissions. To solve this error please\ngive me that permission");
                    return;
                }

                var auditlogs = await g.GetAuditLogsAsync(1, null, null, null, ActionType.InviteDeleted)
                    .FlattenAsync();
                foreach (var audit in auditlogs)
                    if (audit.User is IUser data && audit.Data is InviteDeleteAuditLogData inviteDeleteAuditLog)
                    {
                        var mxage = inviteDeleteAuditLog.MaxAge / 60;
                        await _serverHelper.SendLogAsync(g, "Invite Deleted",
                            $"**An invite to this guild has been deleted.**\n*Invite:* {"https://discord.gg/" + inviteDeleteAuditLog.Code}\n*Invite Uses:* `{inviteDeleteAuditLog.MaxUses.ToString()}`\n*Max Time:* `{mxage.ToString()} minuets`\n*Invite bound to channel:* {argTextChannel.Mention}\n*Invite temporary:* `{inviteDeleteAuditLog.Temporary.ToString()}`\n*Invite deleted by:* {inviteDeleteAuditLog.Creator.Mention}");
                        return;
                    }
            }
        }

        private async Task OnInviteCreated(SocketInvite arg)
        {
            var chnl = arg.Channel;
            if (chnl is ITextChannel argTextChannel)
            {
                var g = chnl.Guild;
                var channelId = await _servers.GetLogsAsync(g.Id);
                var chnlIdInt = Convert.ToInt64(channelId);
                if (chnlIdInt == 0) return;
                if (!g.CurrentUser.GuildPermissions.ViewAuditLog)
                {
                    await _serverHelper.SendLogAsync(g, "Bot Does Not Have Sufficient Permissions",
                        "A event logger has failed its task because I do\nnot have `View Audit Log` permissions. To solve this error please\ngive me that permission");
                    return;
                }

                var auditlogs = await g.GetAuditLogsAsync(1, null, null, null, ActionType.InviteCreated)
                    .FlattenAsync();
                foreach (var audit in auditlogs)
                    if (audit.User is IUser data && audit.Data is InviteCreateAuditLogData inviteCreateAuditLog)
                    {
                        var mxuse = inviteCreateAuditLog.MaxUses;
                        if (!mxuse.Equals(0))
                        {
                            var mxage = inviteCreateAuditLog.MaxAge / 60;
                            await _serverHelper.SendLogAsync(g, "Invite Created",
                                $"**An invite to this guild has been created.**\n*Invite:* {"https://discord.gg/" + inviteCreateAuditLog.Code}\n*Invite Uses:* `{mxuse.ToString()}`\n*Max Time:* `{mxage.ToString()} minuets`\n*Invite bound to channel* {argTextChannel.Mention}\n*Invite temporary:* `{inviteCreateAuditLog.Temporary.ToString()}`\n*Invite created by:* {inviteCreateAuditLog.Creator.Mention}");
                            return;
                        }
                        else
                        {
                            var mxage = inviteCreateAuditLog.MaxAge / 60;
                            await _serverHelper.SendLogAsync(g, "Invite Created",
                                $"**An invite to this guild has been created.**\n*Invite:* {"https://discord.gg/" + inviteCreateAuditLog.Code}\n*Invite Uses:* `{inviteCreateAuditLog.MaxUses.ToString()}`\n*Max Time:* `unlimited`\n*Invite bound to channel* {argTextChannel.Mention}\n*Invite temporary:* `{inviteCreateAuditLog.Temporary.ToString()}`\n*Invite created by:* {inviteCreateAuditLog.Creator.Mention}");
                            return;
                        }
                    }
            }
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

                var auditlogs = await arg2.GetAuditLogsAsync(1, null, null, null, ActionType.GuildUpdated)
                    .FlattenAsync();

                var eauditlogs = await arg2.GetAuditLogsAsync(1, null, null, null, ActionType.EmojiCreated)
                    .FlattenAsync();

                var edauditlogs = await arg2.GetAuditLogsAsync(1, null, null, null, ActionType.EmojiDeleted)
                    .FlattenAsync();

                var upauditlogs = await arg2.GetAuditLogsAsync(1, null, null, null, ActionType.EmojiUpdated)
                    .FlattenAsync();

                //  var webauditlogs = await arg2.GetAuditLogsAsync(1, null, null, null, ActionType.WebhookCreated)
                //    .FlattenAsync();

                //   var webdelauditlogs = await arg2.GetAuditLogsAsync(1, null, null, null, ActionType.WebhookDeleted)
                //   .FlattenAsync();

                //  var webupauditlogs = await arg2.GetAuditLogsAsync(1, null, null, null, ActionType.WebhookUpdated)
                //  .FlattenAsync();


                foreach (var audit in auditlogs)
                    if (audit.User is IUser data && audit.Data is GuildUpdateAuditLogData d1)
                    {
                        if (d1.Before.Name != d1.After.Name) //Check for modified guild name
                        {
                            await _serverHelper.SendLogAsync(arg2, "Guild Name Changed",
                                $"**The guild name has been changed.**\n*Old Guild Name:* `{arg1.Name}`\n*New Guild Name:* `{arg2.Name}`\n*Modified By:* {audit.User.Mention}");
                            return;
                        }

                        if (arg1.OwnerId.ToString() != arg2.OwnerId.ToString()) //Check for server ownership transfer
                        {
                            await _serverHelper.SendLogAsync(arg2, "Guild Ownership Transfered",
                                $"{audit.User.Mention} has transfered this guilds ownership to {arg2.Owner.Mention}");
                            return;
                        }

                        if (arg1.IconUrl != arg2.IconUrl) //Check for guild icon change
                        {
                            await _serverHelper.SendLogAsync(arg2, "Guild Icon Changed",
                                $"**The guild icon has been changed.**\n*New Icon:* {arg2.IconUrl}\n*Modified By:* {audit.User.Mention}");
                            return;
                        }

                        if (arg1.AFKChannel.Id != arg2.AFKChannel.Id) //Check for modified AFK channel
                        {
                            await _serverHelper.SendLogAsync(arg2, "AFK Channel Changed",
                                $"**The AFK channel has been changed.**\n*AFK Channel Name:* `{arg2.AFKChannel.Name}`\n*Modified By:* {audit.User.Mention}");
                            return;
                        }

                        if (arg1.AFKTimeout != arg2.AFKTimeout) //Check for modified afk timeout interval
                        {
                            var beforetimeinmins = arg1.AFKTimeout / 60;
                            var aftertimeinmins = arg2.AFKTimeout / 60;
                            await _serverHelper.SendLogAsync(arg2, "AFK Timeout Has Been Modified",
                                $"AFK Timeout has been modified.\nOld Timeout Interval: `{beforetimeinmins.ToString()} minuets`\nNew Timeout Interval: `{aftertimeinmins.ToString()} minuets`\nModified By: {audit.User.Mention}");
                            return;
                        }

                        if (arg1.VoiceRegionId != arg2.VoiceRegionId
                        ) //Check if guild server voice region has been changed
                        {
                            await _serverHelper.SendLogAsync(arg2, "Guild Server Region Changed",
                                $"**This guilds server region has been changed.**\n*New Region Id:* `<{arg2.VoiceRegionId}>`\n*Modified By:* {audit.User.Mention}");
                            return;
                        }

                        if (arg1.VerificationLevel.ToString() != arg2.VerificationLevel.ToString()
                        ) //Check if verification level has been changed 
                        {
                            await _serverHelper.SendLogAsync(arg2, "Guild Verification Level Modified",
                                $"**This guilds verification level has been modified.**\n*Old Verification Level:* `{arg1.VerificationLevel.ToString()}`\n*New Verification Level:* `{arg2.VerificationLevel.ToString()}`\n*Modified By:* {audit.User.Mention}");
                            return;
                        }

                        if (arg1.MfaLevel != arg2.MfaLevel
                        ) //Check if Multi-Factor Authentication protocol has been modified
                        {
                            await _serverHelper.SendLogAsync(arg2, "Multi-Factor Authentication Policy Modified",
                                $"**This guilds Multi-Factor Authentification level has been modified.**\n*Old MFA Level:* `{arg1.MfaLevel.ToString()}`\n*New MFA Level:* `{arg2.MfaLevel.ToString()}`\n*Modified By:* {audit.User.Mention}");
                            return;
                        }

                        if (arg1.SystemChannel.Id != arg2.SystemChannel.Id) //Check for modified system Channel
                        {
                            await _serverHelper.SendLogAsync(arg2, "System Channel Changed",
                                $"**The system Channel has been modified.**\n*Old System Channel:* {arg1.SystemChannel.Mention}\n*New System Channel:* {arg2.SystemChannel.Mention}\n*Modified By:* {audit.User.Mention}");
                            return;
                        }

                        if (arg1.ExplicitContentFilter != arg2.ExplicitContentFilter
                        ) //Check if explit content filter has been modified
                        {
                            await _serverHelper.SendLogAsync(arg2, "Explit Content Filter Has Been Modified",
                                $"**The explit content filer has been changed.**\n*Old Explicit Content Filter:* `{arg1.ExplicitContentFilter.ToString()}`*New Explicit Content Filter:* `{arg2.ExplicitContentFilter.ToString()}`\nModified By {audit.User.Mention}");
                            return;
                        }

                        if (arg1.BannerUrl != arg2.BannerUrl) //Check for modified guild banner
                        {
                            await _serverHelper.SendLogAsync(arg2, "Guild Banner Modified",
                                $"**This guilds banner has been modified.**\n*Old Banner:* {arg1.BannerUrl}\n*New Banner:* {arg2.BannerUrl}");
                            return;
                        }

                        if (arg1.PublicUpdatesChannel != arg2.PublicUpdatesChannel
                        ) //Check for modified PublicUpdatesChannel
                        {
                            await _serverHelper.SendLogAsync(arg2, "Public Updates Channel Modified",
                                $"This guilds public updates channel has been modified to {arg2.PublicUpdatesChannel.Mention} by {audit.User.Mention}");
                            return;
                        }

                        if (arg1.RulesChannel != arg2.RulesChannel) //Check for modified rules channel
                        {
                            await _serverHelper.SendLogAsync(arg2, "Rules Channel Modified",
                                $"**This guilds rules channel has been modified.**\n*Rules Channel:* {arg2.RulesChannel.Mention}\n*Modified By:* {audit.User.Mention}");
                            return;
                        }

                        if (arg1.PremiumSubscriptionCount != arg2.PremiumSubscriptionCount)
                        {
                            await _serverHelper.SendLogAsync(arg2,
                                "Guild Boosted",
                                $"**This guild has been boosted.**\n*Booster:* {audit.User.Mention}\n*Current Guild Tier:* `{arg2.PremiumTier.ToString()}`\n*Total Current Boosts:* `{arg2.PremiumSubscriptionCount.ToString()}`");
                            return;
                        }

                        if (arg1.PremiumTier != arg2.PremiumTier)
                        {
                            await _serverHelper.SendLogAsync(arg2, "Guild Boost Tier Rank Up",
                                $"**This guilds boost tier has changed.**\n*Guild Boost Tier:* `{arg2.PremiumTier.ToString()}`");
                            return;
                        }
                        /*
                        foreach (var webaudit in webauditlogs)
                            if (webaudit.Data is WebhookCreateAuditLogData webhookCreateAudit &&
                                webaudit.User is IUser webauditUser) //Check for new webhook
                                if (webhookCreateAudit is IWebhook iweb)
                                {
                                    await _serverHelper.SendLogAsync(arg2, "Webhook Created",
                                        $"A webhook has been added to this guild.\nName: `{webhookCreateAudit.Name}`\nId: `{webhookCreateAudit.WebhookId.ToString()}`\nBound to: {iweb.Channel.Mention}\nAdded by: {webauditUser.Mention}");
                                    return;
                                }

                        foreach (var webdelaudit in webdelauditlogs)
                            if (webdelaudit.Data is WebhookDeleteAuditLogData webhookDeleteAuditLogData &&
                                webdelaudit.User is IUser webdelauditUser) //Check for deleted webhook
                                if (webhookDeleteAuditLogData is IWebhook iweb)
                                {
                                    await _serverHelper.SendLogAsync(arg2, "Webhook Deleted",
                                        $"A webhook has been removed from this guild.\nName: `{webhookDeleteAuditLogData.Name}`\nId: `{webhookDeleteAuditLogData.WebhookId.ToString()}`\nBound to: {iweb.Channel.Mention}\nRemoved by: {webdelaudit.User.Mention}");
                                    return;
                                }

                        foreach (var webupaudit in webupauditlogs)
                            if (webupaudit.Data is WebhookUpdateAuditLogData webhookUpdateAudit &&
                                webupaudit.User is IUser webupauditUser) //Check for updated webhook
                                if (webhookUpdateAudit is IWebhook iweb)
                                {
                                    await _serverHelper.SendLogAsync(arg2, "Webhook Updated",
                                        $"A webhook has been updated.\nOld name: `{webhookUpdateAudit.Before.Name}`\nNew name: `{webhookUpdateAudit.After.Name}`\nId: `{webhookUpdateAudit.Webhook.Id.ToString()}`\nBound to: {iweb.Channel.Mention}\nAvatar: {iweb.GetAvatarUrl()}\nModified by: {webupaudit.User.Mention}");
                                    return;
                                }
                        */

                        foreach (var eaudit in eauditlogs)
                            if (eaudit.Data is EmoteCreateAuditLogData emoteCreateAuditLogData &&
                                eaudit.User is IUser eauditUser
                            )
                                if (arg1.Emotes.Count < arg2.Emotes.Count) //Check for new emote
                                {
                                    var emote = _client.Guilds
                                        .SelectMany(x => x.Emotes)
                                        .FirstOrDefault(x => x.Name.IndexOf(
                                            emoteCreateAuditLogData.Name, StringComparison.OrdinalIgnoreCase) != -1);
                                    if (emote == null) return;


                                    await _serverHelper.SendLogAsync(arg2, "Emote Added",
                                        $"**A emote has been added to this guild.**\n*Emote:* {emote}\n*Emote Name:* `{emote.Name}`\n*Emote Id:* `{emote.Id}`\n*Added By:* {eaudit.User.Mention}");
                                    return;
                                }

                        foreach (var edaudit in edauditlogs)
                            if (edaudit.Data is EmoteDeleteAuditLogData emoteDeleteAuditLogData &&
                                edaudit.User is IUser edauditUser
                            ) //Check if emote was deleted
                                if (arg1.Emotes.Count > arg2.Emotes.Count)
                                {
                                    var emote = _client.Guilds
                                        .SelectMany(x => x.Emotes)
                                        .FirstOrDefault(x => x.Name.IndexOf(
                                            emoteDeleteAuditLogData.Name, StringComparison.OrdinalIgnoreCase) != -1);
                                    if (emote == null) return;
                                    await _serverHelper.SendLogAsync(arg2, "Emote Deleted",
                                        $"**An emote has been deleted from the guild.**\n*Emote Name:* `{emote.Name}`\n*Removed By:* {edaudit.User.Mention}");
                                    return;
                                }

                        foreach (var upaudit in upauditlogs)
                            if (upaudit.Data is EmoteUpdateAuditLogData emoteUpdateAuditLogData &&
                                upaudit.User is IUser upauditUser)
                                if (emoteUpdateAuditLogData.OldName != emoteUpdateAuditLogData.NewName
                                ) //Check for new emote name
                                {
                                    var emote = _client.Guilds
                                        .SelectMany(x => x.Emotes)
                                        .FirstOrDefault(x => x.Name.IndexOf(
                                                                 emoteUpdateAuditLogData.NewName,
                                                                 StringComparison.OrdinalIgnoreCase) !=
                                                             -1);
                                    if (emote == null) return;
                                    await _serverHelper.SendLogAsync(arg2, "Emote Name Modified",
                                        $"**An emote has been modified.**\n*Emote:* {emote}\n*Emote Old Name:* `{emoteUpdateAuditLogData.OldName}`\n*Emote New Name:* `{emoteUpdateAuditLogData.NewName}`\n*Emote Id:* `{emote.Id}`\n*Modified By:* {upaudit.User.Mention}");
                                    return;
                                }
                    }
            }
        }

        private async Task OnMessageUpdate(Cacheable<IMessage, ulong> arg1, SocketMessage arg2,
            ISocketMessageChannel arg3)
        {
            var chan = arg2.Channel;
            if (chan is SocketGuildChannel gldchannel)
            {
                var g = gldchannel.Guild;
                var channelId = await _servers.GetLogsAsync(g.Id);
                var chnlIdInt = Convert.ToInt64(channelId);
                if (chnlIdInt == 0) return;
                if (!g.CurrentUser.GuildPermissions.ViewAuditLog)
                {
                    await _serverHelper.SendLogAsync(g, "Bot Does Not Have Sufficient Permissions",
                        "A event logger has failed its task because I do\nnot have `View Audit Log` permissions. To solve this error please\ngive me that permission");
                    return;
                }

                var auditlogs = await g.GetAuditLogsAsync(1, null, null, null, ActionType.MessagePinned)
                    .FlattenAsync();

                var unauditlogs = await g.GetAuditLogsAsync(1, null, null, null, ActionType.MessageUnpinned)
                    .FlattenAsync();

                foreach (var audit in auditlogs)
                    if (audit.User is IUser data && audit.Data is MessagePinAuditLogData d1)
                        if (arg2.IsPinned) //Check if message was pinned
                        {
                            await _serverHelper.SendLogAsync(g, "Message Pinned",
                                $"{audit.User.Mention} has pinned a message in `#{arg3.Name}`.\nMessage: `{arg2.Content}`");
                            return;
                        }

                foreach (var unaudit in unauditlogs)
                    if (unaudit.User is IUser data && unaudit.Data is MessageUnpinAuditLogData d2
                    ) //Check if message was unpinned

                    {
                        await _serverHelper.SendLogAsync(g, "Message Unpinned",
                            $"{unaudit.User.Mention} has unpinned a message in `#{arg3.Name}`\nMessage: `{arg2.Content}`");
                        return;
                    }
            }
        }

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

                if (arg is ITextChannel) //if channel was a text channel
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
                else //if channel was a voice channel
                {
                    var auditlogs = await g.GetAuditLogsAsync(1, null, null, null, ActionType.ChannelCreated)
                        .FlattenAsync();
                    foreach (var audit in auditlogs)
                        if (audit.User is IUser data)
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

                if (arg is ITextChannel) //if channel was a text channel
                {
                    var auditlogs = await g.GetAuditLogsAsync(1, null, guildChannel.Id, null, ActionType.ChannelDeleted)
                        .FlattenAsync();
                    foreach (var audit in auditlogs)
                        if (audit.User is IUser data)
                            await _serverHelper.SendLogAsync(g, "Channel Deleted",
                                $"Channel `#{guildChannel.Name}` has been deleted by{audit.User.Mention}");
                }
                else //if channel was a voice channel
                {
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

                    var auditlogs = await g.GetAuditLogsAsync(1, null, null, null, ActionType.ChannelUpdated)
                        .FlattenAsync();

                    var webauditlogs = await g.GetAuditLogsAsync(1, null, null, null, ActionType.WebhookCreated)
                        .FlattenAsync();

                    var webdelauditlogs = await g.GetAuditLogsAsync(1, null, null, null, ActionType.WebhookDeleted)
                        .FlattenAsync();

                    var webupauditlogs = await g.GetAuditLogsAsync(1, null, null, null, ActionType.WebhookUpdated)
                        .FlattenAsync();


                    if (arg2 is ITextChannel)
                    {
                        if (arg1 is ITextChannel itChannel && arg2 is ITextChannel itChannel2)
                            foreach (var audit in auditlogs)
                                if (audit.User is IUser && audit.Data is ChannelUpdateAuditLogData d1)
                                {
                                    if (guildChannel.Name != gld2Channel.Name) //Check for modified channel name
                                    {
                                        await _serverHelper.SendLogAsync(g, "Channel Name Updated",
                                            $"The `#{gld2Channel.Name}` channel's name has been updated to `#{guildChannel.Name}` by {audit.User.Mention}");
                                        return;
                                    }

                                    if (itChannel.Topic != itChannel2.Topic) //Check for channel topic modification
                                    {
                                        await _serverHelper.SendLogAsync(g, "Channel Topic Updated",
                                            $"The `#{itChannel2.Mention}` channel's topic has been changed to `{itChannel2.Topic}` by {audit.User.Mention}");
                                        return;
                                    }

                                    if (itChannel.IsNsfw != itChannel2.IsNsfw
                                    ) //Check for NSFW toggle change
                                    {
                                        await _serverHelper.SendLogAsync(g, "Channel Toggled NSFW",
                                            $"{itChannel2.Mention}'s NSFW has been toggled from `{itChannel.IsNsfw.ToString()}` to `{itChannel2.IsNsfw.ToString()}` by {audit.User.Mention}");
                                        return;
                                    }

                                    if (itChannel.SlowModeInterval != itChannel2.SlowModeInterval
                                    ) //Check for slowmode changes
                                    {
                                        await _serverHelper.SendLogAsync(g,
                                            "Slowmode Modified",
                                            $"The slowmode for `#{itChannel2.Mention}` has been modified to {itChannel2.SlowModeInterval.ToString()} seconds by {audit.User.Mention}");
                                        return;
                                    }

                                    if (itChannel.PermissionOverwrites != itChannel2.PermissionOverwrites
                                    ) //Check for modified channel permissions
                                    {
                                        await _serverHelper.SendLogAsync(g, "Channel Permissions Modified",
                                            $"{itChannel2.Mention}'s permissions have been modified by {audit.User.Mention}");
                                        return;
                                    }

                                    if (itChannel.Position != itChannel2.Position
                                    ) //Check for modifiied channel position
                                    {
                                        await _serverHelper.SendLogAsync(g, "Channel Position Modified",
                                            $"{itChannel2.Mention}'s position has been changed by {audit.User.Mention}");
                                        return;
                                    }


                                    foreach (var webaudit in webauditlogs)
                                        if (webaudit.Data is WebhookCreateAuditLogData webhookCreateAudit &&
                                            webaudit.User is IUser webauditUser) //Check for new webhook

                                            // if (webhookCreateAudit is IWebhook iweb)
                                        {
                                            await _serverHelper.SendLogAsync(g, "Webhook Created",
                                                $"A webhook has been added to this guild.\nName: `{webhookCreateAudit.Name}`\nId: `{webhookCreateAudit.WebhookId.ToString()}`\nBound to: {webhookCreateAudit.Webhook.Channel.Mention}\nAdded by: {webauditUser.Mention}");
                                            return;
                                        }

                                    foreach (var webdelaudit in webdelauditlogs)
                                        if (webdelaudit.Data is WebhookDeleteAuditLogData webhookDeleteAuditLogData &&
                                            webdelaudit.User is IUser webdelauditUser) //Check for deleted webhook
                                            if (webhookDeleteAuditLogData is IWebhook iweb)
                                            {
                                                await _serverHelper.SendLogAsync(g, "Webhook Deleted",
                                                    $"A webhook has been removed from this guild.\nName: `{webhookDeleteAuditLogData.Name}`\nId: `{webhookDeleteAuditLogData.WebhookId.ToString()}`\nBound to: {iweb.Channel.Mention}\nRemoved by: {webdelaudit.User.Mention}");
                                                return;
                                            }

                                    foreach (var webupaudit in webupauditlogs)
                                        if (webupaudit.Data is WebhookUpdateAuditLogData webhookUpdateAudit &&
                                            webupaudit.User is IUser webupauditUser) //Check for updated webhook
                                            if (webhookUpdateAudit is IWebhook iweb)
                                            {
                                                await _serverHelper.SendLogAsync(g, "Webhook Updated",
                                                    $"A webhook has been updated.\nOld name: `{webhookUpdateAudit.Before.Name}`\nNew name: `{webhookUpdateAudit.After.Name}`\nId: `{webhookUpdateAudit.Webhook.Id.ToString()}`\nBound to: {iweb.Channel.Mention}\nAvatar: {iweb.GetAvatarUrl()}\nModified by: {webupaudit.User.Mention}");
                                                return;
                                            }
                                }
                    }
                    else
                    {
                        foreach (var audit in auditlogs)
                            if (audit.User is IUser && audit.Data is ChannelUpdateAuditLogData d1)
                                if (arg1 is SocketVoiceChannel vc1 && arg2 is SocketVoiceChannel vc2)
                                    if (arg1 is IVoiceChannel ivc && arg2 is IVoiceChannel ivc2)
                                    {
                                        if (vc1.Name != vc2.Name)
                                        {
                                            await _serverHelper.SendLogAsync(g, "Voice Channel Name Updated",
                                                $"The `{vc1.Name}` voice channel has been updated to `{vc2.Name}` by {audit.User.Mention}");
                                            return;
                                        }

                                        if (ivc.Bitrate != ivc2.Bitrate) //Check for voice channel bitrate changes
                                        {
                                            await _serverHelper.SendLogAsync(g, "Voice Channel Bitrate Changed",
                                                $"The bitrate for the `{gld2Channel.Name}` voice channel has been changed to `{vc2.Bitrate / 1000}kbps` by {audit.User.Mention}`");
                                            return;
                                        }

                                        if (ivc.UserLimit.Value.ToString() != ivc2.UserLimit.Value.ToString()
                                        ) //Check for voice channel user limit changes
                                        {
                                            await _serverHelper.SendLogAsync(g, "Voice Channel User Limit Modified",
                                                $"The `{ivc2.Name}` voice channel's user limit has been modified from `{ivc.UserLimit.Value.ToString()} users` to `{ivc2.UserLimit.Value.ToString()} users` by {audit.User.Mention}");
                                            return;
                                        }


                                        if (ivc.PermissionOverwrites != ivc2.PermissionOverwrites
                                        ) //Check for voice channel permission changes
                                        {
                                            await _serverHelper.SendLogAsync(g, "Voice Channel Permissions Modified",
                                                $"The `{ivc2.Name}` voice channel's permissions have been modified by {audit.User.Mention}");
                                            return;
                                        }
                                    }
                    }
                }
        }

        private async Task OnMemberJoin(SocketGuildUser arg)
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

            // var invites = await g.GetInvitesAsync();

            //   var auditlogs = await g.GetAuditLogsAsync(1, null, null, null, ActionType.InviteUpdated)
            //    .FlattenAsync();

            //foreach (var invite in invites)
            //  {

            //    if (invite.Uses.Value) return;

            //    await _serverHelper.SendLogAsync(g, "Invite Used",
            //        $"An invite has been used.\nInvite: {invite.Url}\nTotal invite uses: `{invite.Uses.ToString()}`\nInvite created by: {invite.Inviter.Mention}");
            //    return;
            // }
            /*
            foreach (var audit in auditlogs)
                if (audit.User is IUser && audit.Data is InviteUpdateAuditLogData d1)

                {
                    var inv3 = _client.Guilds
                        .OfType<InviteUpdateAuditLogData>()
                        .FirstOrDefault(x => x.After.Code.ToString().IndexOf(
                                                 d1.After.Code,
                                                 StringComparison.OrdinalIgnoreCase) !=
                                             -1);
                    if (inv3 == null) return;

                    if (inv3 is IInvite iInvite)

                    {
                        var uses = d1.Before.MaxUses.Value - d1.After.MaxUses.Value;
                        await _serverHelper.SendLogAsync(g, "Invite Used",
                            $"An invite has been used.\nInvite: {iInvite.Url}\nTotal invite uses: `{uses.ToString()}`\nInvite created by: {iInvite.Inviter.Mention}");
                        return;
                    }
                }
            */

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

        private async Task OnUserLeft(SocketGuildUser arg)
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

            if (arg is IGuildUser guildUser)
            {
                var auditlogs = await g.GetAuditLogsAsync(1, null, null, null, ActionType.Kick)
                    .FlattenAsync();

                foreach (var audit in auditlogs)
                    if (audit.User is IUser data && audit.Data is KickAuditLogData d1)
                        if (d1.Target.ToString() == arg.ToString()) //Check if user was kicked
                        {
                            await _serverHelper.SendLogAsync(g, "User Kicked",
                                $"User `{d1.Target.Username + "#" + d1.Target.Discriminator}` has been kicked by {audit.User.Mention}. Reason: `{audit.Reason ?? "No reason provided"}`");
                            return;
                        }
                        else
                        {
                            return;
                        }
            }
        }

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
                        $"User `{arg1.Username + "#" + arg1.Discriminator}` has been banned by {audit.User.Mention} Reason: `{audit.Reason ?? "No reason provided"}`");
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
                {
                    if (arg1.Name != arg2.Name)
                    {
                        await _serverHelper.SendLogAsync(g, "Role Name Updated",
                            $"Role `{arg1.Name}`'s name has been changed to `{arg2.Name}`");
                        return;
                    }

                    if (arg1.Color.RawValue != arg2.Color.RawValue)
                    {
                        await _serverHelper.SendLogAsync(g, "Role Color Modified",
                            $"Role `{arg2.Name}`'s color has been changed");
                        return;
                    }

                    if (arg1.Position != arg2.Position)
                    {
                        await _serverHelper.SendLogAsync(g, "Role Hiarchy Position Modified",
                            $"Role `{arg2.Name}`'s position on the hierarchy has been modified by {audit.User.Mention}");
                        return;
                    }

                    if (d1.Before.Mentionable.Value != d1.After.Mentionable.Value)
                    {
                        await _serverHelper.SendLogAsync(g, "Role Mentionability Modified",
                            $"Role `{arg2.Name}`'s mentionability has been toggled to `{arg2.IsMentionable.ToString()}`");
                        return;
                    }


                    if (ReferenceEquals(d1.After.Permissions.Value.ToString(),
                        d1.Before.Permissions.Value.ToString())) //Check for modified permissions for the role
                    {
                        await _serverHelper.SendLogAsync(g, "Role Permissions Modified",
                            $"Permissions for the {arg2.Mention} role have been modified by {audit.User.Mention}");
                        return;
                    }
                }
        }
        /*
        private async Task OnGuildMemberUpdated(SocketGuildUser arg1, SocketGuildUser arg2)
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

            var auditlogs = await g.GetAuditLogsAsync(1, null, null, null, ActionType.MemberUpdated)
                .FlattenAsync();
            foreach (var audit in auditlogs)
                if (audit.User is IUser data && audit.Data is MemberUpdateAuditLogData d1)
                    // if (arg1 is IGuildUser guildUser && arg2 is IGuildUser guildUser2)
                {
                    if (d1.Before.Mute.Value != d1.After.Mute.Value) //Check for is muted
                    {
                        await _serverHelper.SendLogAsync(g, "Member Muted",
                            $"User {arg2.Mention} has been muted by {audit.User.Mention}");
                        return;
                    }

                    if (d1.Before.Deaf.Value != d1.After.Deaf.Value) //check for is deafened
                    {
                        await _serverHelper.SendLogAsync(g, "Member Deafened",
                            $"User {arg2.Mention} has been deafened by {audit.User.Mention}");
                        return;
                    }

                    if (arg1.Username != arg2.Username) //Check for new username
                    {
                        await _serverHelper.SendLogAsync(g, "Username Changed",
                            $"`{arg1.Username + "#" + arg1.Discriminator}` has changed their username to `{arg2.Username + "#" + arg1.Discriminator}`");
                        return;
                    }

                    if (arg1.AvatarId != arg2.AvatarId) //Check for new profile pic
                    {
                        await _serverHelper.SendLogAsync(g, "User Profile Pic Modified",
                            $"User {arg2.Mention} has changed their profile pic.\nNew Profile pic: {arg2.GetAvatarUrl() ?? arg2.GetDefaultAvatarUrl()}");
                        return;
                    }

                    if (d1.Before.Nickname != d1.After.Nickname) //Check for new nickname
                    {
                        await _serverHelper.SendLogAsync(g, "Nickname Modified",
                            $"User `{arg1.Username + "#" + arg1.Discriminator}` has changed their nickname to `{arg2.Nickname}`");
                        return;
                    }
                }
        }
        */

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

            // var inv = await before.GetOrDownloadAsync();

            // var inv = _client.Guilds
            // .SelectMany(x => x.GetInvitesAsync().Result);

            // foreach (var guild in _client.Guilds)
            //  {
            //var inv = await guild.GetInvitesAsync();
            //    var ini = await guild.GetInvitesAsync();
            //  }
        }


        private async Task InitiateDisconnectAsync(LavaPlayer player, TimeSpan timeSpan)
        {
            if (!_disconnectTokens.TryGetValue(player.VoiceChannel.Id, out var value))
            {
                value = new CancellationTokenSource();
                _disconnectTokens.TryAdd(player.VoiceChannel.Id, value);
            }
            else if (value.IsCancellationRequested)
            {
                _disconnectTokens.TryUpdate(player.VoiceChannel.Id, new CancellationTokenSource(), value);
                value = _disconnectTokens[player.VoiceChannel.Id];
            }

            await player.TextChannel.SendMessageAsync($"Auto disconnect initiated! Disconnecting in {timeSpan}...");
            var isCancelled = SpinWait.SpinUntil(() => value.IsCancellationRequested, timeSpan);
            if (isCancelled) return;

            await _lavaNode.LeaveAsync(player.VoiceChannel);
            await player.TextChannel.SendMessageAsync("Invite me again sometime");
        }

        /*
        private async Task OnTrackStarted(TrackStartEventArgs arg)
        {
            if (!_disconnectTokens.TryGetValue(arg.Player.VoiceChannel.Id, out var value)) return;

            if (value.IsCancellationRequested) return;

            value.Cancel(true);
            await arg.Player.TextChannel.SendMessageAsync("Auto disconnect has been cancelled!");
        }
        */
        private async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            if (!args.Reason.ShouldPlayNext()) return;

            var player = args.Player;
            if (!player.Queue.TryDequeue(out var queueable)
            ) //Check if there are more tracks in the queue or if not disconnect after timespan
            {
                await player.TextChannel.TextMusic("Queue Completed",
                    "Add more songs to the queue to keep the party going!\nI will auto disconnect from this voice channel in 20 seconds otherwise",
                    "https://external-content.duckduckgo.com/iu/?u=http%3A%2F%2Ficons.iconarchive.com%2Ficons%2Fiynque%2Fios7-style%2F1024%2FMusic-icon.png&f=1&nofb=1");
                _ = InitiateDisconnectAsync(args.Player,
                    TimeSpan.FromSeconds(20)); //Disconnect from voice channel if nothing played after time value
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
            if (!_disconnectTokens.TryGetValue(player.VoiceChannel.Id, out var value)) return;

            if (value.IsCancellationRequested) return;

            value.Cancel(true);
            //await args.Player.TextChannel.SendMessageAsync(
            //    $"{args.Reason}: {args.Track.Title}\nNow playing: {track.Title}");
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