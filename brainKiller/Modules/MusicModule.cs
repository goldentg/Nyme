using System;
using System.Collections.Generic;
using System.Linq;
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

        // private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens;
        private readonly LavaNode _lavaNode;

        public MusicModule(LavaNode lavaNode)
        {
            _lavaNode = lavaNode;
            // _disconnectTokens = disconnectTokens;

            //_disconnectTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();
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

            var voiceState = Context.User as IVoiceState;
            var player = _lavaNode.GetPlayer(Context.Guild);

            if (!_lavaNode.HasPlayer(Context.Guild) && player.PlayerState != PlayerState.Connected
            ) //Check if bot is in voice channel and if its not in any voice channel join a new one
                try
                {
                    await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                    await Context.Channel.SendSuccessAsync("Success", $"Joined {voiceState.VoiceChannel.Name}");
                }
                catch (Exception exception)
                {
                    await Context.Channel.SendErrorAsync("Error", exception.Message);
                }

            var searchResponse = await _lavaNode.SearchYouTubeAsync(query);
            if (searchResponse.LoadStatus == LoadStatus.LoadFailed ||
                searchResponse.LoadStatus == LoadStatus.NoMatches)
            {
                await Context.Channel.SendErrorAsync("Error", $"I could not find anything for {query}");
                //await ReplyAsync($"I wasn't able to find anything for `{query}`.");
                return;
            }

            if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
            {
                var track = searchResponse.Tracks[0];
                var thumbnail = await track.FetchArtworkAsync(); //get album cover for current song

                player.Queue.Enqueue(track);
                await Context.Channel.Music("Enqued:", track.Title, thumbnail);
            }
            else
            {
                var track = searchResponse.Tracks[0];
                var thumbnail = await track.FetchArtworkAsync(); //get album cover for current song

                await player.PlayAsync(track);
                await Context.Channel.Music("Playing:", track.Title, thumbnail);
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
            await Context.Channel.Music("Skipped", player.Track.Title, thumbnail);
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
    }
}