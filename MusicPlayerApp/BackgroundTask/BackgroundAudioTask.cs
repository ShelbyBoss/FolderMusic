﻿using System;
using System.Threading;
using Windows.ApplicationModel.Background;
using Windows.Media.Playback;
using MusicPlayer;
using MusicPlayer.Handler;
using MusicPlayer.Models;
using MusicPlayer.Models.EventArgs;
using MusicPlayer.Models.Enums;
using System.Threading.Tasks;

namespace BackgroundTask
{
    public sealed class BackgroundAudioTask : IBackgroundTask
    {
        private const string dataFileName = "backgroundData.xml";

        private static BackgroundAudioTask lastTask;

        private BackgroundTaskDeferral deferral;
        private BackgroundPlayerHandler musicPlayer;
        private Timer timer;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            string taskId = taskInstance.InstanceId.ToString();
            MobileDebug.Service.SetIsBackground(taskId);
            MobileDebug.Service.WriteEventPair("Run1",
                "task == null", lastTask == null, "this.Hash", GetHashCode());

            try
            {
                deferral = taskInstance.GetDeferral();
                taskInstance.Canceled += OnCanceled;
                taskInstance.Task.Completed += TaskCompleted;

                lastTask?.musicPlayer?.Dispose();
                lastTask = this;

                Song[] songs;

                try
                {
                    songs = await IO.LoadObjectAsync<Song[]>(dataFileName);
                }
                catch (Exception e)
                {
                    MobileDebug.Service.WriteEvent("Load background playlist error1", e);
                    songs = new Song[0];
                }

                Song? song = CurrentPlaySong.Current.Song;
                TimeSpan position = TimeSpan.FromTicks(CurrentPlaySong.Current.PositionTicks);
                LoopType loop = CurrentPlaySong.Current.Loop;
                musicPlayer = new BackgroundPlayerHandler(song, position, loop, songs);
                musicPlayer.CurrentSongChanged += MusicPlayer_CurrentSongChanged;
                musicPlayer.LoopChanged += MusicPlayer_LoopChanged;
                musicPlayer.SongsChanged += MusicPlayer_SongsChanged;
                await musicPlayer.Start();

                timer = new Timer(Timer_Tick, null, Timeout.Infinite, Timeout.Infinite);
                BackgroundMediaPlayer.Current.CurrentStateChanged += OnStateChanged;
            }
            catch (Exception e)
            {
                MobileDebug.Service.WriteEvent("Run error", e);
            }

            MobileDebug.Service.WriteEventPair("RunFinish",
                "This", GetHashCode(), "mp", musicPlayer.GetHashCode());
        }

        private void OnStateChanged(MediaPlayer sender, object args)
        {
            if (sender.CurrentState == MediaPlayerState.Playing) timer.Change(0, 2000);
            else timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private static void Timer_Tick(object state)
        {
            CurrentPlaySong.Current.PositionTicks = BackgroundMediaPlayer.Current.Position.Ticks;
        }

        private static void MusicPlayer_LoopChanged(object sender, ChangedEventArgs<LoopType> e)
        {
            CurrentPlaySong.Current.Loop = e.NewValue;
        }

        private static async void MusicPlayer_SongsChanged(object sender, ChangedEventArgs<Song[]> e)
        {
            await SavePlaylist(e.NewValue);
        }

        private static async Task SavePlaylist(Song[] songs)
        {
            try
            {
                await IO.SaveObjectAsync(dataFileName, songs);
            }
            catch (Exception e)
            {
                MobileDebug.Service.WriteEvent("Save playlist error", e);
            }
        }

        private static void MusicPlayer_CurrentSongChanged(object sender, ChangedEventArgs<Song?> e)
        {
            CurrentPlaySong.Current.Song = e.NewValue;
        }

        private void TaskCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            //MobileDebug.Service.WriteEvent("TaskCompleted");
            Cancel();
        }

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            //MobileDebug.Service.WriteEvent("OnCanceled", reason);
            Cancel();
        }

        private void Cancel()
        {
            musicPlayer?.Stop();
            timer.Dispose();

            BackgroundMediaPlayer.Shutdown();
            deferral.Complete();
        }
    }
}
