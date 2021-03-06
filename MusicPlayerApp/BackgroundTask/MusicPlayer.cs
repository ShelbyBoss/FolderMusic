﻿using MusicPlayer;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage;
using MusicPlayer.Models;
using MusicPlayer.Models.EventArgs;
using MusicPlayer.Models.Interfaces;
using MusicPlayer.SubscriptionsHandler;

namespace BackgroundTask
{
    class MusicPlayer : IBackgroundPlayer
    {
        private const int maxFailOrSetCount = 15, updateSongPositionMillis = 200;

        private bool playNext = true, mediaEndedHappened, isUpdatingSongPosition;
        private int failedCount, setSongCount;
        private Song openSong;
        private DateTime setPositionTime;
        private TimeSpan setPositionPosition;
        private readonly ILibrary library;
        private readonly LibrarySubscriptionsHandler lsh;
        private readonly Timer timer;
        private readonly SystemMediaTransportControls smtc;

        private Song CurrentSong => library.CurrentPlaylist?.CurrentSong;

        private IPlaylist CurrentPlaylist => library.CurrentPlaylist;

        public MusicPlayer(SystemMediaTransportControls smtControls, ILibrary library)
        {
            smtc = smtControls;
            this.library = library;

            lsh = LibrarySubscriptionsHandler.GetInstance(library);
            lsh.CurrentPlaylist.CurrentSongPositionChanged += OnCurrentSongPositionChanged;
            lsh.CurrentPlaylist.CurrentSong.ArtistChanged += OnCurrentSongArtistOrTitleChanged;
            lsh.CurrentPlaylist.CurrentSong.TitleChanged += OnCurrentSongArtistOrTitleChanged;

            timer = new Timer(Timer_Tick, null, Timeout.Infinite, Timeout.Infinite);

            ActivateSystemMediaTransportControl();
            SetNextSongIfMediaEndedNotHappens();
        }

        private void ActivateSystemMediaTransportControl()
        {
            smtc.IsEnabled = smtc.IsPauseEnabled = smtc.IsPlayEnabled =
                //smtc.IsRewindEnabled = smtc.IsFastForwardEnabled = 
                smtc.IsPreviousEnabled = smtc.IsNextEnabled = true;
        }

        private async void SetNextSongIfMediaEndedNotHappens()
        {
            MobileDebug.Service.WriteEvent("SetNextSongIfMediaEndedNotHappens0", GetHashCode());

            while (!mediaEndedHappened)
            {
                TimeSpan position = BackgroundMediaPlayer.Current.Position;
                TimeSpan duration = BackgroundMediaPlayer.Current.NaturalDuration;

                if (duration > TimeSpan.Zero && position >= duration)
                {
                    MobileDebug.Service.WriteEventPair("SetNextSongIfMediaEndedNotHappens1", "Pos", position,
                        "Dur", duration, "Player", BackgroundMediaPlayer.Current.CurrentState,
                        "Lib", library.PlayerState, "IsPlaying", library.IsPlaying,
                        "CurrentSongFileName", CurrentSong);
                    await Task.Delay(5000);
                    MobileDebug.Service.WriteEventPair("SetNextSongIfMediaEndedNotHappens2", "Pos", position,
                        "Dur", duration, "Player", BackgroundMediaPlayer.Current.CurrentState,
                        "Lib", library.PlayerState, "IsPlaying", library.IsPlaying,
                        "CurrentSongFileName", CurrentSong);

                    if (mediaEndedHappened) break;

                    MobileDebug.Service.WriteEvent("SetNextSongIfMediaEndedNotHappens3");
                    Next(true);
                }

                await Task.Delay(1000);
            }

            MobileDebug.Service.WriteEventPair("SetNextSongIfMediaEndedNotHappens4", "Hash", GetHashCode(),
                "Player", BackgroundMediaPlayer.Current.CurrentState, "Lib", library.PlayerState,
                "IsPlaying", library.IsPlaying, "CurrentSongFileName", CurrentSong);
        }

        public async Task Play()
        {
            timer.Change(0, updateSongPositionMillis);

            if (setSongCount >= maxFailOrSetCount)
            {
                setSongCount = 0;

                Volume0To1();

                MobileDebug.Service.WriteEvent("PlayBecauseOfSetSongCount", BackgroundMediaPlayer.Current.CurrentState);
            }
            else if (BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Closed ||
                 BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Stopped)
            {
                MobileDebug.Service.WriteEventPair("SetOnPlayClosedAndStopped", "SetCount", setSongCount,
                    "CurrentSongFileName", library.CurrentPlaylist?.CurrentSong, "OpenSong", openSong);
                await SetCurrent();
            }
            else if (BackgroundMediaPlayer.Current.NaturalDuration.Ticks == 0)
            {
                MobileDebug.Service.WriteEvent("SetOnPlayDurationZero", library.CurrentPlaylist?.CurrentSong);
                await SetCurrent();
            }
            else if (BackgroundMediaPlayer.Current.CurrentState != MediaPlayerState.Playing)
            {
                double percent = library?.CurrentPlaylist?.CurrentSongPosition ?? -1;
                double duration = library?.CurrentPlaylist?.CurrentSong?.DurationMilliseconds ?? Song.DefaultDuration;

                MobileDebug.Service.WriteEvent("PlayNormal", BackgroundMediaPlayer.Current.CurrentState, CurrentSong, percent);
                if (percent >= 0) BackgroundMediaPlayer.Current.Position = TimeSpan.FromMilliseconds(percent * duration);

                Volume0To1();

                setSongCount = 0;
            }

            smtc.PlaybackStatus = MediaPlaybackStatus.Playing;

            BackgroundMediaPlayer.Current.Play();
        }

        private static async void Volume0To1()
        {
            BackgroundMediaPlayer.Current.Volume = 0;
            BackgroundMediaPlayer.Current.Play();

            const double step = 0.1;

            for (double i = step; i < 1; i += step)
            {
                BackgroundMediaPlayer.Current.Volume = Math.Sqrt(i);
                await Task.Delay(10);
            }
        }

        public async void Pause()
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);

            smtc.PlaybackStatus = MediaPlaybackStatus.Paused;

            if (BackgroundMediaPlayer.Current.CurrentState != MediaPlayerState.Paused) await Volume1To0AndPause();

            try
            {
                if (library.CurrentPlaylist == null) return;

                TimeSpan position = BackgroundMediaPlayer.Current.Position;
                TimeSpan duration = BackgroundMediaPlayer.Current.NaturalDuration;
                library.CurrentPlaylist.CurrentSongPosition = position.TotalMilliseconds / duration.TotalMilliseconds;
            }
            catch (Exception e)
            {
                MobileDebug.Service.WriteEvent("MusicPauseFail", e);
            }
        }

        private static async Task Volume1To0AndPause()
        {
            const double step = 0.1;

            for (double i = 1; i > 0; i -= step)
            {
                BackgroundMediaPlayer.Current.Volume = i;
                await Task.Delay(10);
            }

            BackgroundMediaPlayer.Current.Pause();
        }

        public void Next(bool fromEnded)
        {
            CurrentPlaylist.ChangeCurrentSong(1);
            playNext = true;
            bool isLast = CurrentPlaylist.CurrentSong == CurrentPlaylist.Songs.Last();

            if (isLast && fromEnded) library.IsPlaying = false;
        }

        public void Previous()
        {
            playNext = false;
            CurrentPlaylist.ChangeCurrentSong(-1);
        }

        public async Task SetCurrent()
        {
            MobileDebug.Service.WriteEventPair("TrySet", "OpenPath", openSong?.Path,
                "CurSongEmpty", CurrentSong?.IsEmpty, "CurSongFailed", CurrentSong?.Failed,
                "IsOpen", (CurrentSong == openSong), "CurrentSongFileName", CurrentSong);

            if (CurrentSong == null || CurrentSong.IsEmpty || CurrentSong.Failed) return;

            BackgroundMediaPlayer.Current.AutoPlay = library.IsPlaying && CurrentPlaylist.CurrentSongPosition == 0;

            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(CurrentSong.Path);
                BackgroundMediaPlayer.Current.SetFileSource(file);
                setSongCount++;
                MobileDebug.Service.WriteEvent("Set", setSongCount, CurrentSong);
            }
            catch (Exception e)
            {
                MobileDebug.Service.WriteEvent("Catch", e, CurrentSong);
                //await library.SkippedSongs.Add(CurrentSongFileName);
                await Task.Delay(100);

                if (playNext) Next(false);
                else Previous();
            }
        }

        public void MediaOpened(MediaPlayer sender, object args)
        {
            MobileDebug.Service.WriteEventPair("Open", "SetSongCount", setSongCount, "Sender.State", sender.CurrentState,
                "IsPlaying", library.IsPlaying, "Pos", CurrentPlaylist.GetCurrentSongPosition().TotalSeconds,
                "CurrentSongFileName", CurrentSong);

            playNext = true;
            failedCount = 0;
            openSong = CurrentSong;

            if (library.IsLoaded && sender.NaturalDuration.TotalMilliseconds > Song.DefaultDuration)
            {
                CurrentSong.DurationMilliseconds = sender.NaturalDuration.TotalMilliseconds;
            }

            setPositionTime = DateTime.Now;
            setPositionPosition = CurrentPlaylist.GetCurrentSongPosition();
            if (setPositionPosition > TimeSpan.Zero) sender.Position = setPositionPosition;

            MobileDebug.Service.WriteEventPair("OpenSetPosition",
                "set position", setPositionPosition, "get position", sender.Position);

            if (library.IsPlaying)
            {
                if (CurrentPlaylist.CurrentSongPosition > 0) Volume0To1();
                else BackgroundMediaPlayer.Current.Play();
            }
            else smtc.PlaybackStatus = MediaPlaybackStatus.Paused;

            UpdateSystemMediaTransportControl();
        }

        private void OnCurrentSongPositionChanged(object sender, SubscriptionsEventArgs<IPlaylist, CurrentSongPositionChangedEventArgs> e)
        {
            if (isUpdatingSongPosition) return;


            setPositionTime = DateTime.Now;
            setPositionPosition = BackgroundMediaPlayer.Current.Position = e.Source.GetCurrentSongPosition();
        }

        private void OnCurrentSongArtistOrTitleChanged(object sender, EventArgs args)
        {
            UpdateSystemMediaTransportControl();
        }

        private void UpdateSystemMediaTransportControl()
        {
            SystemMediaTransportControlsDisplayUpdater du = smtc.DisplayUpdater;

            if (du.Type != MediaPlaybackType.Music) du.Type = MediaPlaybackType.Music;
            if (du.MusicProperties.Title != CurrentSong.Title || du.MusicProperties.Artist != CurrentSong.Artist)
            {
                du.MusicProperties.Title = CurrentSong.Title;
                du.MusicProperties.Artist = CurrentSong.Artist;
                du.Update();
            }
        }

        public async Task MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            MobileDebug.Service.WriteEvent("Fail", args.ExtendedErrorCode, args.Error, args.ErrorMessage, CurrentSong);
            await Task.Delay(100);

            failedCount++;

            if (failedCount >= maxFailOrSetCount) failedCount = 0;
            else if (args.Error == MediaPlayerError.Unknown)
            {
                await Task.Delay(2000);

                await SetCurrent();
                return;
            }

            //if (args.ExtendedErrorCode.Message == "")
            //{
            //    library.IsPlaying = false;
            //    return;
            //}

            CurrentSong.SetFailed();
            //await library.SkippedSongs.Add(CurrentSongFileName);

            //if (playNext) Next(true);
            //else Previous();
            library.IsPlaying = false;
        }

        public async Task MediaEnded(MediaPlayer sender, object args)
        {
            mediaEndedHappened = true;
            smtc.PlaybackStatus = MediaPlaybackStatus.Playing;

            TimeSpan durationLeft = BackgroundMediaPlayer.Current.NaturalDuration - setPositionPosition;
            bool passedEnoughTime = (DateTime.Now - setPositionTime).TotalDays > durationLeft.TotalDays / 2;

            MobileDebug.Service.WriteEventPair("MusicEnded", "SMTC-State", smtc.PlaybackStatus,
                "CurrentSongFileName", CurrentSong, "setPositionTime", setPositionTime,
                "setPositionPosition", setPositionPosition, "passedTime", passedEnoughTime);

            if (DateTime.Now - setPositionTime < TimeSpan.FromSeconds(2))
            {
                MobileDebug.Service.WriteEvent("Skipped Song bug", "CurrentSongFileName", CurrentSong,
                    "setPositionTime", setPositionTime, "setPositionPosition", setPositionPosition,
                    "passedTime", passedEnoughTime);
            }

            if (passedEnoughTime) Next(true);
            else
            {
                BackgroundMediaPlayer.Current.Position = TimeSpan.Zero;
                BackgroundMediaPlayer.Current.Play();
            }
        }

        private void Timer_Tick(object state)
        {
            if (library?.CurrentPlaylist == null) return;

            TimeSpan position = BackgroundMediaPlayer.Current.Position;
            TimeSpan duration = BackgroundMediaPlayer.Current.NaturalDuration;

            if (duration <= TimeSpan.Zero) return;

            isUpdatingSongPosition = true;
            library.CurrentPlaylist.CurrentSongPosition = position.TotalDays / duration.TotalDays;
            isUpdatingSongPosition = false;
        }

        public void Dispose()
        {
            timer.Dispose();
            mediaEndedHappened = true;
        }
    }
}
