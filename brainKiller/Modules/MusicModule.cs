using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using brainKiller.Common;
using Victoria.EventArgs;

namespace brainKiller.Modules
{
    public class MusicModule : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;

        public MusicModule(LavaNode lavaNode)
        {
            _lavaNode = lavaNode;
        }
        private async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            if (!args.Reason.ShouldPlayNext())
            {
                return;
            }

            var player = args.Player;
            if (!player.Queue.TryDequeue(out var queueable))
            {
                await player.TextChannel.SendMessageAsync("Queue completed! Please add more tracks to rock n' roll!");
                return;
            }

            if (!(queueable is LavaTrack track))
            {
                await player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
                return;
            }

            await args.Player.PlayAsync(track);
            await args.Player.TextChannel.SendMessageAsync(
                $"{args.Reason}: {args.Track.Title}\nNow playing: {track.Title}");
        }

        [Command("Join", RunMode = RunMode.Async)]
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

    [Command("Play", RunMode = RunMode.Async)]
    public async Task PlayAsync([Remainder] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
                //await ReplyAsync("Please provide search terms.");
                await Context.Channel.SendErrorAsync("Error", "Please provide search terms");
            return;
        }

        if (!_lavaNode.HasPlayer(Context.Guild))
        {
                //await ReplyAsync("I'm not connected to a voice channel.");
                await Context.Channel.SendErrorAsync("Error", "I'm not connected to a voice channel");
            return;
        }

            var searchResponse = await _lavaNode.SearchYouTubeAsync(query);
            if (searchResponse.LoadStatus == LoadStatus.LoadFailed ||
                searchResponse.LoadStatus == LoadStatus.NoMatches)
            {
                await Context.Channel.SendErrorAsync("Error", $"I could not find anything for {query}");
                //await ReplyAsync($"I wasn't able to find anything for `{query}`.");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
            {
                var track = searchResponse.Tracks[0];
                var thumbnail = await track.FetchArtworkAsync();

                player.Queue.Enqueue(track);
                await Context.Channel.Music("Enqued:", track.Title, thumbnail);
                //await ReplyAsync($"Enqued {track.Title}");
            }
            else {
                var track = searchResponse.Tracks[0];
                var thumbnail = await track.FetchArtworkAsync();

                    await player.PlayAsync(track);
                    await Context.Channel.Music("Playing:", track.Title, thumbnail);
                   // await ReplyAsync($"Now Playing: {track.Title}");
                }
            }

        [Command("skip", RunMode = RunMode.Async)]
        public async Task Skip()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await Context.Channel.SendErrorAsync("Error", "You must be connected to a voice channel");
                //await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await Context.Channel.SendErrorAsync("Error", "I'm not connected to a voice channel");
                //await ReplyAsync("I'm not connected to a voice channel!");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await Context.Channel.SendErrorAsync("Error", "You must be in the same channel as me");
                //await ReplyAsync("You must be in the same channel as me");
                return;
            }

            if (player.Queue.Count == 0)
            {
                await Context.Channel.SendErrorAsync("Error", "There are no more songs in the queue");
                //await ReplyAsync("There are no more songs in the queue!");
                return;
            }

            await player.SkipAsync();
            var thumbnail = await player.Track.FetchArtworkAsync();
            await Context.Channel.Music("Skipped", player.Track.Title, thumbnail);
            //await ReplyAsync($"Skipped! Now playing: **{player.Track.Title}**!");
        }

        [Command("pause", RunMode = RunMode.Async)]
        public async Task Pause()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await Context.Channel.SendErrorAsync("Error", "You must be connected to a voice channel");
                //await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await Context.Channel.SendErrorAsync("Error", "I'm not connected to a voice channel");
                //await ReplyAsync("I'm not connected to a voice channel!");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await Context.Channel.SendErrorAsync("Error", "You must be in the same channel as me");
                //await ReplyAsync("You must be in the same channel as me");
                return;
            }

            if (player.PlayerState == PlayerState.Paused || player.PlayerState == PlayerState.Stopped)
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
        public async Task Resume()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                Context.Channel.SendErrorAsync("Error", "You must be connected to a voice channel");
                //await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await Context.Channel.SendErrorAsync("Error", "I'm not connected to a voice channel");
                //await ReplyAsync("I'm not connected to a voice channel!");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                Context.Channel.SendErrorAsync("Error", "You must be in the same channel as me");
                //await ReplyAsync("You must be in the same channel as me");
                return;
            }

            if (player.PlayerState == PlayerState.Playing)
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

