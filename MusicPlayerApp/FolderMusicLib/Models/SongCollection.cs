﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using MusicPlayer.Models.EventArgs;
using MusicPlayer.Models.Interfaces;
using MusicPlayer.Models.Shuffle;
using MusicPlayer.Models.Simple;

namespace MusicPlayer.Models
{
    public class SongCollection : ISongCollection
    {
        private List<Song> list;

        public event EventHandler<SongCollectionChangedEventArgs> Changed;
        public event EventHandler<ShuffleChangedEventArgs> ShuffleChanged;

        public int Count => list.Count;

        public IPlaylist Parent { get; set; }

        private IShuffleCollection shuffle;

        public IShuffleCollection Shuffle
        {
            get { return shuffle; }
            set
            {
                if (value == shuffle) return;

                ShuffleChangedEventArgs args = new ShuffleChangedEventArgs(shuffle, value);
                shuffle?.Dispose();
                shuffle = value;
                ShuffleChanged?.Invoke(this, args);
                OnPropertyChanged(nameof(Shuffle));
            }
        }

        public SongCollection()
        {
            list = new List<Song>();
            Shuffle = new ShuffleOffCollection(this);
        }

        public SongCollection(IEnumerable<Song> songs, ShuffleType type, Song currentSong)
        {
            list = new List<Song>(songs);

            foreach (Song song in list) song.Parent = this;
            
            Shuffle = CreateShuffle(type, currentSong);
        }

        public int IndexOf(Song song)
        {
            return list.IndexOf(song);
        }

        public void Add(Song song)
        {
            Change(null, Utils.RepeatOnce(song));
        }

        public void Remove(Song song)
        {
            Change(Utils.RepeatOnce(song), null);
        }

        public void Change(IEnumerable<Song> removes, IEnumerable<Song> adds)
        {
            //Song oldCurrentSong, newCurrentSong;
            //newCurrentSong = oldCurrentSong = Parent.CurrentSongFileName;

            //int currentSongIndex = list.IndexOf(oldCurrentSong);
            Song[] removeArray = removes?.ToArray() ?? new Song[0];
            Song[] addArray = adds?.ToArray() ?? new Song[0];

            List<ChangeCollectionItem<Song>> removeChanges = new List<ChangeCollectionItem<Song>>();
            List<ChangeCollectionItem<Song>> addChanges = new List<ChangeCollectionItem<Song>>();

            foreach (Song song in removeArray)
            {
                int index = list.IndexOf(song);

                if (index == -1) continue;

                list.RemoveAt(index);
                removeChanges.Add(new ChangeCollectionItem<Song>(index, song));
            }

            foreach (Song song in addArray)
            {
                int index = list.IndexOf(song);

                if (index == -1) continue;

                list.RemoveAt(index);
                removeChanges.Add(new ChangeCollectionItem<Song>(index, song));
            }

            foreach (Song song in addArray)
            {
                addChanges.Add(new ChangeCollectionItem<Song>(Count, song));
                list.Add(song);

                song.Parent = this;
            }

            SongCollectionChangedEventArgs args = new SongCollectionChangedEventArgs(addChanges.ToArray(), removeChanges.ToArray());
            Changed?.Invoke(this, args);
            OnPropertyChanged(nameof(Count));
        }

        public ISongCollection ToSimple()
        {
            return new SimpleSongCollection(Shuffle, Parent.CurrentSong);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public IEnumerator<Song> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            ShuffleType shuffleType = (ShuffleType)Enum.Parse(typeof(ShuffleType),
                reader.GetAttribute("Shuffle") ?? Enum.GetName(typeof(ShuffleType), ShuffleType.Off));
            IShuffleCollection shuffle = GetShuffleType(shuffleType);

            reader.ReadStartElement();
            list = XmlConverter.DeserializeList<Song>(reader, "Song").ToList();

            foreach (Song song in list) song.Parent = this;

            shuffle.ReadXml(XmlConverter.GetReader(reader.ReadOuterXml()));
            Shuffle = shuffle;
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Shuffle", Enum.GetName(typeof(ShuffleType), Shuffle.Type));

            foreach (Song song in this)
            {
                writer.WriteStartElement("Song");
                song.WriteXml(writer);
                writer.WriteEndElement();
            }

            writer.WriteStartElement("Shuffle");
            Shuffle.WriteXml(writer);
            writer.WriteEndElement();
        }

        public void SetShuffleType(ShuffleType type)
        {
            if (type == Shuffle.Type) return;

            Shuffle = CreateShuffle(type, Parent?.CurrentSong);
        }

        private IShuffleCollection GetShuffleType(ShuffleType type)
        {
            switch (type)
            {
                case ShuffleType.Complete:
                    return new ShuffleCompleteCollection(this);

                case ShuffleType.Off:
                    return new ShuffleOffCollection(this);

                case ShuffleType.OneTime:
                    return new ShuffleOneTimeCollection(this);

                case ShuffleType.Path:
                    return new ShufflePathCollection(this);
            }

            throw new NotImplementedException("Value \"" + type + "\"of LoopType is not implemented in GetShuffleType");
        }

        private IShuffleCollection CreateShuffle(ShuffleType type, Song currentSong)
        {
            MobileDebug.Service.WriteEvent("CreateShuffle1", type,currentSong);
            switch (type)
            {
                case ShuffleType.Complete:
                    return new ShuffleCompleteCollection(this, currentSong);

                case ShuffleType.Off:
                    return new ShuffleOffCollection(this);

                case ShuffleType.OneTime:
                    return new ShuffleOneTimeCollection(this, currentSong);

                case ShuffleType.Path:
                    return new ShufflePathCollection(this);
            }

            throw new NotImplementedException("Value \"" + type + "\"of LoopType is not implemented in CreateShuffle");
        }
    }
}
