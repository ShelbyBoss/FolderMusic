﻿using Windows.UI.Xaml.Media.Imaging;
using PlayerIcons;

namespace LibraryLib
{
    class LoopAll : ILoop
    {
        public BitmapImage GetIcon()
        {
            try
            {
                return Icons.LoopAll;
            }
            catch { }

            return new BitmapImage();
        }   

        public LoopKind GetKind()
        {
            return LoopKind.All;
        }

        public ILoop GetNext()
        {
            return new LoopCurrent();
        }
    }
}
