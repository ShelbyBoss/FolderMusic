﻿using System.Collections.Generic;

namespace MusicPlayer.Data.Shuffle
{
    class ShuffleOff : IShuffle
    {
        private static IShuffle instance;

        public static IShuffle Instance
        {
            get
            {
                if (instance == null) instance = new ShuffleOff();

                return instance;
            }
        }

        private ShuffleOff() { }

        public void AddSongsToShuffleList(ref List<int> shuffleList, IList<Song> oldSongs, IList<Song> updatedSongs)
        {
            shuffleList = GenerateShuffleList(0, updatedSongs.Count);
        }

        public List<int> GenerateShuffleList(int songsIndex, int songsCount)
        {
            List<int> shuffleList = new List<int>();

            for (int i = 0; i < songsCount; i++)
            {
                shuffleList.Add(i);
            }

            return shuffleList;
        }

        public void GetChangedShuffleListBecauseOfOtherSongsIndex
            (int songsIndex, ref List<int> shuffleList, int songsCount)
        {

        }

        public ShuffleType GetShuffleType()
        {
            return ShuffleType.Off;
        }

        public IShuffle GetNext()
        {
            return ShuffleOneTime.Instance;
        }

        public int GetShuffleListIndex(int songsIndex, List<int> shuffleList, int songsCount)
        {
            return songsIndex;
        }

        public void RemoveSongsIndex(int songsIndex, ref List<int> shuffleList, int songsCount)
        {
            List<int> newShuffleList = new List<int>(shuffleList);

            if (newShuffleList.Contains(songsIndex)) newShuffleList.RemoveAt(newShuffleList.Count - 1);

            shuffleList = newShuffleList;
        }

        public void CheckShuffleList(ref List<int> shuffleList, int songsCount)
        {
            shuffleList = GenerateShuffleList(0, songsCount);
        }
    }
}
