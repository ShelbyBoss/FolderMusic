﻿using System;
using System.Linq;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using MusicPlayer.Communication.Messages;
using MusicPlayer.Models;
using MusicPlayer.Models.Enums;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using MusicPlayer.Models.Foreground.Interfaces;
using System.Threading.Tasks;

namespace MusicPlayer.Communication
{
    class ForegroundCommunicator
    {
        private bool isRunning, gotAnyMessage, isUpdatingCurrentSong;

        public event EventHandler<bool> IsPlayingReceived;
        public event EventHandler<string> CurrentSongReceived;

        public async Task Start()
        {
            gotAnyMessage = false;
            BackgroundMediaPlayer.MessageReceivedFromBackground += OnMessageReceived;

            MobileDebug.Service.WriteEvent("ForeComStart1");
            while (!gotAnyMessage)
            {
                Send(ForegroundMessageType.Ping, force: true);
                await Task.Delay(100);
            }
            MobileDebug.Service.WriteEvent("ForeComStart2");

            isRunning = true;
        }

        public void Stop()
        {
            BackgroundMediaPlayer.MessageReceivedFromForeground -= OnMessageReceived;
            isRunning = false;
        }

        public void SendPlaylist(IPlaylist playlist)
        {
            PlaylistMessage message = new PlaylistMessage()
            {
                PositionTicks = playlist?.Position.Ticks ?? 0,
                CurrentSong = playlist?.CurrentSong,
                Loop = playlist?.Loop ?? LoopType.Off,
                Songs = playlist?.Songs.Shuffle.ToArray() ?? new Song[0],
            };
            Send(ForegroundMessageType.SetPlaylist, XmlConverter.Serialize(message));
        }

        public void SendLoop(LoopType loop)
        {
            Send(ForegroundMessageType.SetLoop, loop.ToString());
        }

        public void SendSongs(Song[] songs)
        {
            string value = XmlConverter.Serialize(songs);
            Send(ForegroundMessageType.SetSongs, value);
        }

        public void SeekPosition(TimeSpan position)
        {
            Send(ForegroundMessageType.SetPosition, position.Ticks.ToString());
        }

        public void SendCurrentSong(Song? song, TimeSpan position)
        {
            if (isUpdatingCurrentSong) return;

            string value = XmlConverter.Serialize(new CurrentSongMessage(song, position.Ticks));
            Send(ForegroundMessageType.SetCurrentSong, value);
        }

        public void Play()
        {
            Send(ForegroundMessageType.Play);
        }

        public void Pause()
        {
            Send(ForegroundMessageType.Pause);
        }

        public void Next()
        {
            Send(ForegroundMessageType.Next);
        }

        public void Previous()
        {
            Send(ForegroundMessageType.Previous);
        }

        private void Send(ForegroundMessageType type, string value = "", bool force = false)
        {
            if (!isRunning && !force)
            {
                MobileDebug.Service.WriteEvent("ForeComDontSend", type);
                return;
            }

            ValueSet vs = new ValueSet()
            {
                {Constants.TypeKey, type.ToString()},
                {Constants.ValueKey, value}
            };
            try
            {
                BackgroundMediaPlayer.SendMessageToBackground(vs);
            }
            catch (Exception e)
            {
                MobileDebug.Service.WriteEvent("Fore send error", e, type, value);
            }
        }

        private async void OnMessageReceived(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            gotAnyMessage = true;

            BackgroundMessageType type = GetType(e.Data);
            string value = e.Data[Constants.ValueKey].ToString();
            MobileDebug.Service.WriteEvent("ForeCom_Receive", type, value.Length);
            try
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => HandleReceivedMessage(type, value));
            }
            catch (Exception exc)
            {
                MobileDebug.Service.WriteEvent("ForeComReceiveError", exc, type, value);
            }
        }

        private void HandleReceivedMessage(BackgroundMessageType type, string value)
        {
            switch (type)
            {
                case BackgroundMessageType.SetCurrentSong:
                    isUpdatingCurrentSong = true;
                    CurrentSongReceived?.Invoke(this, value);
                    isUpdatingCurrentSong = false;
                    break;

                case BackgroundMessageType.SetIsPlaying:
                    IsPlayingReceived?.Invoke(this, bool.Parse(value));
                    break;

                case BackgroundMessageType.Ping:
                    break;
            }
        }

        private static BackgroundMessageType GetType(ValueSet vs)
        {
            return Utils.ParseEnum<BackgroundMessageType>(vs[Constants.TypeKey].ToString());
        }
    }
}
