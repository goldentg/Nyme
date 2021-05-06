using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using brainKiller.Common;
using Discord;
using Discord.Commands;
using Victoria;
using Victoria.Enums;

namespace brainKiller.Modules
{
    public class MusicModule : ModuleBase<SocketCommandContext>
    {
        private static readonly IEnumerable<int> Range = Enumerable.Range(1900, 2000);

        private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens;
        private readonly LavaNode _lavaNode;


        public MusicModule(LavaNode lavaNode)
        {
            _lavaNode = lavaNode;
            //_disconnectTokens = disconnectTokens;

            _disconnectTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();
        }

        /*
        [Command("Join", RunMode = RunMode.Async)]
        [Summary("Adds the bot to a voice channel")]
        public async Task JoinAsync()
        {
            if (_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm already connected to a voice channel!");
                return;
            }

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                await Context.Channel.SendSuccessAsync("Success", $"Joined {voiceState.VoiceChannel.Name}");
                //await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
            }
            catch (Exception exception)
            {
                // await ReplyAsync(exception.Message);
                await Context.Channel.SendErrorAsync("Error", exception.Message);
            }
        }

        */
        [Command("disconnect", RunMode = RunMode.Async)]
        [Summary("Disconnects the bot from a voice channel")]
        public async Task DisconnectAsync()
        {
            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm not in a voice channel!");
                return;
            }

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            try
            {
                await _lavaNode.LeaveAsync(voiceState.VoiceChannel);
                await Context.Channel.SendSuccessAsync("Success", $"Disconnected from {voiceState.VoiceChannel.Name}");
                //await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
            }
            catch (Exception exception)
            {
                // await ReplyAsync(exception.Message);
                await Context.Channel.SendErrorAsync("Error", exception.Message);
            }
        }

        [Command("Play", RunMode = RunMode.Async)]
        [Summary("Plays music")]
        public async Task PlayAsync([Remainder] string query)
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
                try
                {
                    await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                    await Context.Channel.SendSuccessAsync("Success", $"Joined {voiceState.VoiceChannel.Name}");
                    //await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
                }
                catch (Exception exception)
                {
                    await Context.Channel.SendErrorAsync("Error", exception.Message);
                }

            if (string.IsNullOrWhiteSpace(query)) //Check if nothing was entered
            {
                await Context.Channel.SendErrorAsync("Error", "Please provide search terms");
                return;
            }

            if (query.StartsWith("https://")) //Check if user entered link
            {
                await Context.Channel.SendErrorAsync("Error", "Nyme does not currently support links at this moment");
                return;
            }

            var players = _lavaNode.GetPlayer(Context.Guild);

            var searchResponse = await _lavaNode.SearchYouTubeAsync(query);
            if (searchResponse.LoadStatus == LoadStatus.LoadFailed ||
                searchResponse.LoadStatus == LoadStatus.NoMatches)
            {
                await Context.Channel.SendErrorAsync("Error", $"I could not find anything for {query}");
                //await ReplyAsync($"I wasn't able to find anything for `{query}`.");
                return;
            }

            if (players.PlayerState == PlayerState.Playing || players.PlayerState == PlayerState.Paused)
            {
                var track = searchResponse.Tracks[0];
                var thumbnail = await track.FetchArtworkAsync(); //get album cover for current song

                players.Queue.Enqueue(track);
                await Context.Channel.Music("Enqued:", track.Title,
                    thumbnail ??
                    "https://external-content.duckduckgo.com/iu/?u=http%3A%2F%2Ficons.iconarchive.com%2Ficons%2Fiynque%2Fios7-style%2F1024%2FMusic-icon.png&f=1&nofb=1");
            }
            else
            {
                var track = searchResponse.Tracks[0];
                var thumbnail = await track.FetchArtworkAsync(); //get album cover for current song

                await players.PlayAsync(track);
                await Context.Channel.Music("Playing:", track.Title,
                    thumbnail ??
                    "https://external-content.duckduckgo.com/iu/?u=http%3A%2F%2Ficons.iconarchive.com%2Ficons%2Fiynque%2Fios7-style%2F1024%2FMusic-icon.png&f=1&nofb=1");
            }
        }

        [Command("skip", RunMode = RunMode.Async)]
        [Summary("Skips a song")]
        public async Task Skip()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null) //Check if executed user is in a voice channel
            {
                await Context.Channel.SendErrorAsync("Error", "You must be connected to a voice channel");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild)) //Check if bot is in a voice channel
            {
                await Context.Channel.SendErrorAsync("Error", "I'm not connected to a voice channel");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (voiceState.VoiceChannel != player.VoiceChannel
            ) //Check if bot is in the same voice channel as user that executed command
            {
                await Context.Channel.SendErrorAsync("Error", "You must be in the same channel as me");
                return;
            }

            if (player.Queue.Count == 0) //Check if there are any more songs in queue 
            {
                await Context.Channel.SendErrorAsync("Error", "There are no more songs in the queue");
                return;
            }

            await player.SkipAsync();
            var thumbnail = await player.Track.FetchArtworkAsync();
            await Context.Channel.Music("Skipped", player.Track.Title,
                thumbnail ??
                "https://external-content.duckduckgo.com/iu/?u=http%3A%2F%2Ficons.iconarchive.com%2Ficons%2Fiynque%2Fios7-style%2F1024%2FMusic-icon.png&f=1&nofb=1");
            //await ReplyAsync($"Skipped! Now playing: **{player.Track.Title}**!");
        }

        [Command("pause", RunMode = RunMode.Async)]
        [Summary("pauses a song")]
        public async Task Pause()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null) //Check if executed user is in a voice channel
            {
                await Context.Channel.SendErrorAsync("Error", "You must be connected to a voice channel");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild)) //Check if bot is in a voice channel
            {
                await Context.Channel.SendErrorAsync("Error", "I'm not connected to a voice channel");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (voiceState.VoiceChannel != player.VoiceChannel
            ) //Check if user the executed is in same voice channel as bot
            {
                await Context.Channel.SendErrorAsync("Error", "You must be in the same channel as me");
                return;
            }

            if (player.PlayerState == PlayerState.Paused || player.PlayerState == PlayerState.Stopped
            ) //Check if song is already paused
            {
                await Context.Channel.SendErrorAsync("Error", "The music is already paused");
                //await ReplyAsync("The music is already paused!");
                return;
            }

            await player.PauseAsync();
            await Context.Channel.SendSuccessAsync("Paused", "Music has been paused");
            // await ReplyAsync("Paused the music!");
        }

        [Command("resume", RunMode = RunMode.Async)]
        [Summary("Resumes a song")]
        public async Task Resume()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null) //Check if user is in a voice channel
            {
                await Context.Channel.SendErrorAsync("Error", "You must be connected to a voice channel");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild)) //Check if bot is in a voice channel
            {
                await Context.Channel.SendErrorAsync("Error", "I'm not connected to a voice channel");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (voiceState.VoiceChannel != player.VoiceChannel
            ) //Check if executed user is in the same voice channel as the bot
            {
                await Context.Channel.SendErrorAsync("Error", "You must be in the same channel as me");
                return;
            }

            if (player.PlayerState == PlayerState.Playing) //Check if music is already resumed
            {
                await Context.Channel.SendErrorAsync("Error", "The music is already playing");
                //await ReplyAsync("The music is already playing!");
                return;
            }

            await player.ResumeAsync();
            await Context.Channel.SendSuccessAsync("Resumed", $"I have resumed playing {player.Track.Title}");
            //await ReplyAsync("Resuming the music!");
        }

        [Command("volume", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        [Summary("Allows users with Mute Members permission to change the volume of the bot")]
        public async Task VolumeAsync(ushort volume)
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await Context.Channel.SendErrorAsync("Error:", "I'm not connected to a voice channel.");
                return;
            }

            try
            {
                await player.UpdateVolumeAsync(volume);
                await Context.Channel.SendSuccessAsync("Success", $"I've changed my volume to `{volume}`.");
            }
            catch (Exception exception)
            {
                await Context.Channel.SendErrorAsync("Error", exception.Message);
            }
        }

        [Command("playing", RunMode = RunMode.Async)]
        [Summary("Displays what song is currently playing")]
        public async Task NowPlayingAsync()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await Context.Channel.SendErrorAsync("Error:", "I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await Context.Channel.SendErrorAsync("Error", "I'm not currently playing any tracks.");
                return;
            }

            var track = player.Track;
            var artwork = await track.FetchArtworkAsync();

            var embed = new EmbedBuilder
                {
                    Title = $"{track.Author} - {track.Title}",
                    ThumbnailUrl = artwork ??
                                   "https://external-content.duckduckgo.com/iu/?u=http%3A%2F%2Ficons.iconarchive.com%2Ficons%2Fiynque%2Fios7-style%2F1024%2FMusic-icon.png&f=1&nofb=1",
                    Url = track.Url
                }
                .AddField("Link", $"{track.Url}")
                .AddField("Duration",
                    $"`{track.Duration.Minutes + ":" + track.Duration.Seconds}`");

            await ReplyAsync(embed: embed.Build());
        }

        [Command("lyrics", RunMode = RunMode.Async)]
        [Summary("Sends the lyrics to the song that is playing")]
        public async Task ShowGeniusLyrics()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await Context.Channel.SendErrorAsync("Error:", "I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await Context.Channel.SendErrorAsync("Error", "I'm not currently playing any tracks.");
                return;
            }

            var lyrics = await player.Track.FetchLyricsFromGeniusAsync();
            if (string.IsNullOrWhiteSpace(lyrics))
            {
                await Context.Channel.SendErrorAsync("Error", $"No lyrics found for `{player.Track.Title}`");
                return;
            }

            var track = player.Track;
            var artwork = await track.FetchArtworkAsync();

            var splitLyrics = lyrics.Split('\n');
            var stringBuilder = new StringBuilder();
            foreach (var line in splitLyrics)
                if (Range.Contains(stringBuilder.Length))
                {
                    await Context.Channel.Music($"{track.Title} Lyrics", $"```{stringBuilder}```",
                        artwork ??
                        "https://external-content.duckduckgo.com/iu/?u=http%3A%2F%2Ficons.iconarchive.com%2Ficons%2Fiynque%2Fios7-style%2F1024%2FMusic-icon.png&f=1&nofb=1");
                    stringBuilder.Clear();
                }
                else
                {
                    stringBuilder.AppendLine(line);
                }

            await Context.Channel.Music($"{track.Title} Lyrics", $"```{stringBuilder}```",
                artwork ??
                "https://external-content.duckduckgo.com/iu/?u=http%3A%2F%2Ficons.iconarchive.com%2Ficons%2Fiynque%2Fios7-style%2F1024%2FMusic-icon.png&f=1&nofb=1");
        }

        [Command("OVH", RunMode = RunMode.Async)]
        [Summary("Search the OVH API for song lyrics")]
        public async Task ShowOVHLyrics()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await Context.Channel.SendErrorAsync("Error:", "I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await Context.Channel.SendErrorAsync("Error", "I'm not currently playing any tracks.");
                return;
            }

            var lyrics = await player.Track.FetchLyricsFromOVHAsync();
            if (string.IsNullOrWhiteSpace(lyrics))
            {
                await Context.Channel.SendErrorAsync("Error", $"No lyrics found for `{player.Track.Title}`");
                return;
            }

            var track = player.Track;
            var artwork = await track.FetchArtworkAsync();

            var splitLyrics = lyrics.Split('\n');
            var stringBuilder = new StringBuilder();
            foreach (var line in splitLyrics)
                if (Range.Contains(stringBuilder.Length))
                {
                    await Context.Channel.Music($"{track.Title} Lyrics", $"```{stringBuilder}```",
                        artwork ??
                        "https://external-content.duckduckgo.com/iu/?u=http%3A%2F%2Ficons.iconarchive.com%2Ficons%2Fiynque%2Fios7-style%2F1024%2FMusic-icon.png&f=1&nofb=1");
                    stringBuilder.Clear();
                }
                else
                {
                    stringBuilder.AppendLine(line);
                }

            await Context.Channel.Music($"{track.Title} Lyrics", $"```{stringBuilder}```",
                artwork ??
                "https://external-content.duckduckgo.com/iu/?u=http%3A%2F%2Ficons.iconarchive.com%2Ficons%2Fiynque%2Fios7-style%2F1024%2FMusic-icon.png&f=1&nofb=1");
        }
    }
}