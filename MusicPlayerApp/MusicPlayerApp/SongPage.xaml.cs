﻿using System;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MusicPlayer.Models;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkID=390556 dokumentiert.

namespace FolderMusic
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class SongPage : Page
    {
        private Song song;

        public SongPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Wird aufgerufen, wenn diese Seite in einem Frame angezeigt werden soll.
        /// </summary>
        /// <param name="e">Ereignisdaten, die beschreiben, wie diese Seite erreicht wurde.
        /// Dieser Parameter wird normalerweise zum Konfigurieren der Seite verwendet.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            song = e.Parameter as Song;

            if (song == null) return;

            tblPath.Text = song.Path;

            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(song.Path);

                DataContext = await file.Properties.GetMusicPropertiesAsync();
            }
            catch (Exception exc)
            {
                await new MessageDialog(exc.Message, "Load song data error").ShowAsync();
            }
        }

        private async void AbbSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MusicProperties props = (MusicProperties)DataContext;

                await props.SavePropertiesAsync();

                if (song != null) await song.Reset();
            }
            catch (Exception exc)
            {
                await new MessageDialog(exc.Message, exc.GetType().Name).ShowAsync();
            }
        }
    }
}
