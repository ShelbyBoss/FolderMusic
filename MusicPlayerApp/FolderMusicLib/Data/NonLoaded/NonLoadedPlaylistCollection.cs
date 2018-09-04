﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

namespace MusicPlayer.Data.NonLoaded
{
    class NonLoadedPlaylistCollection : IPlaylistCollection
    {
        private List<IPlaylist> list;

        public event PlaylistCollectionChangedEventHandler Changed;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public int Count { get { return list.Count; } }

        public ILibrary Parent { get; private set; }

        public NonLoadedPlaylistCollection(ILibrary parent,
            IPlaylistCollection actualPlaylists, IPlaylist actualCurrentPlaylist)
        {
            Parent = parent;
            list = actualPlaylists.Select(p => new NonLoadedPlaylist(this, p, p == actualCurrentPlaylist)).
                ToList<IPlaylist>();
        }

        public NonLoadedPlaylistCollection(ILibrary parent, XmlReader reader)
        {
            Parent = parent;
            ReadXml(reader);
        }

        public int IndexOf(IPlaylist item)
        {
            return list.IndexOf(item);
        }

        public void Add(IPlaylist playlist)
        {
            Change(Enumerable.Range(0, 1).Select(i => playlist), Enumerable.Empty<IPlaylist>());
        }

        public void Remove(IPlaylist playlist)
        {
            Change(Enumerable.Empty<IPlaylist>(), Enumerable.Range(0, 1).Select(i => playlist));
        }

        public void Change(IEnumerable<IPlaylist> adds, IEnumerable<IPlaylist> removes)
        {
        }

        public void Reset(IEnumerable<IPlaylist> newPlaylists)
        {
            list = new List<IPlaylist>(newPlaylists);
        }

        public IEnumerator<IPlaylist> GetEnumerator()
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
            list = new List<IPlaylist>();

            reader.ReadStartElement();

            while (reader.NodeType == XmlNodeType.Element)
            {
                try
                {
                    list.Add(new NonLoadedPlaylist(this, reader));
                }
                catch (Exception e)
                {
                    MobileDebug.Manager.WriteEvent("XmlReadNonLodedPlaylistCollectionFail",
                        e, list.Count, "Node: " + reader.NodeType);
                }

                reader.Read();
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (var playlist in this)
            {
                writer.WriteStartElement("Playlist");

                try
                {
                    playlist.WriteXml(writer);
                }
                catch (Exception e)
                {
                    MobileDebug.Manager.WriteEvent("XmlWriteNonLoadedPlaylistCollectionFail", e);
                }

                writer.WriteEndElement();
            }
        }
    }
}