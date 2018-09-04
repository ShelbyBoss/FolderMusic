﻿using LibraryLib;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage;

namespace MusicPlayerLib
{
    public sealed class BackgroundAudioTask : IBackgroundTask
    {
        private static BackgroundAudioTask task;
        private BackgroundTaskDeferral deferral;
        private SystemMediaTransportControls systemMediaTransportControl;
        private SystemMediaTransportControlsButton lastPressedButton;

        private bool autoPlay = false, pauseAllowed = true, playNext = true;

        public static BackgroundAudioTask Current { get { return task; } }

        public bool IsPlaying { get { return pauseAllowed ? BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Playing : true; } }

        private double CurrentSongPositionTotalMilliseconds { get { return Library.Current.CurrentPlaylist.SongPositionMilliseconds; } }

        private Song CurrentSong { get { return Library.Current.CurrentPlaylist.CurrentSong; } }

        private Playlist CurrentPlaylist { get { return Library.Current.CurrentPlaylist; } }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            task = this;
            systemMediaTransportControl = SystemMediaTransportControls.GetForCurrentView();

            SetSystemMediaTransportControlDefaultSettings();

            BackgroundMediaPlayer.MessageReceivedFromForeground += ForegroundCommunicator.MessageReceivedFromForeground;
            BackgroundMediaPlayer.Current.CurrentStateChanged += BackgroundMediaPlayer_CurrentStateChanged;
            BackgroundMediaPlayer.Current.MediaEnded += BackgroundMediaPlayer_MediaEnded;
            BackgroundMediaPlayer.Current.MediaOpened += BackgroundMediaPlayer_MediaOpened;
            BackgroundMediaPlayer.Current.MediaFailed += BackgroundMediaPlayer_MediaFailed;

            taskInstance.Canceled += OnCanceled;
            taskInstance.Task.Completed += Taskcompleted;

            deferral = taskInstance.GetDeferral();

            LoadCurrentSongAndLibrary();
        }

        private async void LoadCurrentSongAndLibrary()
        {
            if (!await Library.LoadPlayCommand()) lastPressedButton = SystemMediaTransportControlsButton.Pause;

            if (lastPressedButton == SystemMediaTransportControlsButton.Play)
            {
                await Library.Current.LoadCurrentSong();
                SetCurrentSong(true);

                await Library.Current.LoadAsync();
            }
            else
            {
                await Library.Current.LoadAsync();

                if (lastPressedButton != SystemMediaTransportControlsButton.Pause) MediaTransportControlButtonPressed(lastPressedButton);
                else SetCurrentSong(false);
            }

            SetLoopToBackgroundPlayer();
        }

        private void SetSystemMediaTransportControlDefaultSettings()
        {
            systemMediaTransportControl.IsEnabled = true;
            systemMediaTransportControl.IsPauseEnabled = true;
            systemMediaTransportControl.IsPlayEnabled = true;
            systemMediaTransportControl.IsPreviousEnabled = true;
            systemMediaTransportControl.IsNextEnabled = true;
            systemMediaTransportControl.IsRewindEnabled = true;
            systemMediaTransportControl.IsFastForwardEnabled = true;

            systemMediaTransportControl.ButtonPressed += MediaTransportControlButtonPressed;
        }

        public void SetLoopToBackgroundPlayer()
        {
            BackgroundMediaPlayer.Current.IsLoopingEnabled = CurrentPlaylist.Loop == LoopKind.Current;
        }

        public void Play()
        {
            autoPlay = true;

            if (BackgroundMediaPlayer.Current.NaturalDuration.TotalMilliseconds == 0)
            {
                SetCurrentSong(true);
                return;
            }

            BackgroundMediaPlayer.Current.Play();
        }

        public void Pause()
        {
            autoPlay = false;
            BackgroundMediaPlayer.Current.Pause();
        }

        public void Previous()
        {
            playNext = false;
            CurrentPlaylist.SetPreviousSong();

            if (!CurrentSong.Failed) SetCurrentSong(IsPlaying);
        }

        public void Next(bool autoPlay)
        {
            Next(autoPlay, false);
        }

        public void Next(bool autoPlay, bool fromEnded)
        {
            playNext = true;
            bool stop = CurrentPlaylist.SetNextSong();
            this.autoPlay = fromEnded ? autoPlay && !stop : autoPlay;

            if (!CurrentSong.Failed) SetCurrentSong();
        }

        public void SetCurrentSong()
        {
            SetCurrentSong(autoPlay);
        }

        public void SetCurrentSong(bool autoPlay)
        {
            if (CurrentSong.IsEmptyOrLoading) return;

            StorageFile file;
            this.autoPlay = autoPlay;
            pauseAllowed = !autoPlay;
            BackgroundMediaPlayer.Current.AutoPlay = false;

            try
            {
                file = CurrentSong.GetStorageFile();
                BackgroundMediaPlayer.Current.SetFileSource(file);
            }
            catch
            {
                Task.Delay(100).Wait();
                Library.AddSkipSongAndSave(CurrentSong);

                if (playNext) Next(true);
                else Previous();
            }
        }

        private void BackgroundMediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            if (sender.CurrentState == MediaPlayerState.Playing)
            {
                systemMediaTransportControl.PlaybackStatus = MediaPlaybackStatus.Playing;
                pauseAllowed = true;
            }
            else if (sender.CurrentState == MediaPlayerState.Paused && pauseAllowed)
            {
                systemMediaTransportControl.PlaybackStatus = MediaPlaybackStatus.Paused;
                ForegroundCommunicator.SendPause();
            }
        }

        private void BackgroundMediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            if (CurrentSongPositionTotalMilliseconds != 0)
            {
                sender.Position = TimeSpan.FromMilliseconds(CurrentSongPositionTotalMilliseconds);
            }

            if (autoPlay) sender.Play();
            else systemMediaTransportControl.PlaybackStatus = MediaPlaybackStatus.Paused;
            playNext = true;

            if (CurrentSong.NaturalDurationMilliseconds == 1)
            {
                CurrentSong.NaturalDurationMilliseconds = sender.NaturalDuration.TotalMilliseconds;
            }

            UpdateSystemMediaTransportControl();
            Library.Current.SaveCurrentSongMilliseconds();

            ForegroundCommunicator.SendSongsIndexAndShuffleIfComplete();
        }

        private void UpdateSystemMediaTransportControl()
        {
            systemMediaTransportControl.DisplayUpdater.Type = MediaPlaybackType.Music;

            if (systemMediaTransportControl.DisplayUpdater.MusicProperties.Title != CurrentSong.Title ||
                systemMediaTransportControl.DisplayUpdater.MusicProperties.Artist != CurrentSong.Artist)
            {
                systemMediaTransportControl.DisplayUpdater.MusicProperties.Title = CurrentSong.Title;
                systemMediaTransportControl.DisplayUpdater.MusicProperties.Artist = CurrentSong.Artist;
                systemMediaTransportControl.DisplayUpdater.Update();
            }
        }

        private void BackgroundMediaPlayer_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            Task.Delay(100).Wait();

            if (args.Error == MediaPlayerError.Unknown)
            {
                SetCurrentSong();
                return;
            }

            CurrentSong.SetFailed();
            Library.AddSkipSongAndSave(CurrentSong);

            if (playNext) Next(true);
            else Previous();
        }

        private void BackgroundMediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            Next(true, true);
        }

        private void MediaTransportControlButtonPressed(SystemMediaTransportControls sender,
            SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            if (Library.IsLoaded) MediaTransportControlButtonPressed(args.Button);
            else lastPressedButton = args.Button;
        }

        private void MediaTransportControlButtonPressed(SystemMediaTransportControlsButton button)
        {
            switch (button)
            {
                case SystemMediaTransportControlsButton.Play:
                    Play();
                    break;

                case SystemMediaTransportControlsButton.Pause:
                    Pause();
                    break;

                case SystemMediaTransportControlsButton.Previous:
                    Previous();
                    break;

                case SystemMediaTransportControlsButton.Next:
                    Next(IsPlaying);
                    break;
            }
        }

        private void Taskcompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            BackgroundMediaPlayer.Shutdown();
            deferral.Complete();
        }

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            Library.Current.SaveCurrentSongMilliseconds();
            Library.Current.SaveAsync();

            BackgroundMediaPlayer.Shutdown();
            deferral.Complete();
        }

        private async Task SaveText(object obj)
        {
            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("Text.txt", CreationCollisionOption.ReplaceExisting);

                await PathIO.WriteTextAsync(file.Path, obj.ToString());
            }
            catch { }
        }

        private async Task SaveText2(object obj)
        {
            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("Text2.txt", CreationCollisionOption.ReplaceExisting);

                await PathIO.WriteTextAsync(file.Path, obj.ToString());
            }
            catch { }
        }
    }
}