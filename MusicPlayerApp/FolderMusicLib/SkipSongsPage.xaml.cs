﻿using LibraryLib;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FolderMusicLib
{
    public sealed partial class SkipSongsPage : Page
    {
        private static volatile bool open = false;
        private static SkipSongsPage page;

        public static bool Open { get { return open; } }

        private SkipSongs skipSongs;

        public SkipSongsPage()
        {
            this.InitializeComponent();
            page = this;
        }

        public static async Task NavigateToIfSkipSongsExists()
        {
            if (!Open && Library.IsLoaded)
            {
                open = true;
                SkipSongs skipSongs = SkipSongs.GetNew();

                if (skipSongs.HaveSongs)
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.
                        RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        (Window.Current.Content as Frame).Navigate(typeof(SkipSongsPage), skipSongs);
                    });
                }
                else open = false;
            }
        }

        public static void GoBack()
        {
            if (!Open) return;

            open = false;
            page.Frame.GoBack();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            skipSongs = e.Parameter as SkipSongs;
            SetCurrentSongPath();
        }

        private void SetCurrentSongPath()
        {
            if (skipSongs.CurrentSong.IsEmptyOrLoading) GoBack();
            else tbl_Path.Text = skipSongs.CurrentSong.RelativePath;
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            skipSongs.Yes_Click();
            SetCurrentSongPath();
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            skipSongs.No_Click();
            SetCurrentSongPath();
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {
            skipSongs.Skip_Click();
            SetCurrentSongPath();
        }
    }
}
