﻿

#pragma checksum "C:\Users\Clemens\Documents\Visual Studio 2015\Projects\FolderMusic\MusicPlayerApp\MusicPlayerApp\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "A8C0EDFAB8492A54448A402C435A9BEE"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MusicPlayerApp
{
    partial class MainPage : global::Windows.UI.Xaml.Controls.Page, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 10 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.FrameworkElement)(target)).Loaded += this.Page_Loaded;
                 #line default
                 #line hidden
                #line 10 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).PointerExited += this.Page_PointerExited;
                 #line default
                 #line hidden
                break;
            case 2:
                #line 174 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).PointerEntered += this.sld_PointerEntered;
                 #line default
                 #line hidden
                break;
            case 3:
                #line 192 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).Tapped += this.Loop_Tapped;
                 #line default
                 #line hidden
                #line 193 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).PointerEntered += this.LoopImage_PointerEntered;
                 #line default
                 #line hidden
                #line 193 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).PointerExited += this.LoopImage_PointerExited;
                 #line default
                 #line hidden
                break;
            case 4:
                #line 164 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).Tapped += this.Shuffle_Tapped;
                 #line default
                 #line hidden
                #line 165 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).PointerEntered += this.ShuffleImage_PointerEntered;
                 #line default
                 #line hidden
                #line 165 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).PointerExited += this.ShuffleImage_PointerExited;
                 #line default
                 #line hidden
                break;
            case 5:
                #line 116 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).Tapped += this.CurrentSong_Tapped;
                 #line default
                 #line hidden
                break;
            case 6:
                #line 140 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.FrameworkElement)(target)).Loaded += this.tbxArtist_Loaded;
                 #line default
                 #line hidden
                break;
            case 7:
                #line 129 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.FrameworkElement)(target)).Loaded += this.tbxTitle_Loaded;
                 #line default
                 #line hidden
                break;
            case 8:
                #line 63 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).Holding += this.PlaylistsPlaylist_Holding;
                 #line default
                 #line hidden
                break;
            case 9:
                #line 71 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.MenuFlyoutItem)(target)).Click += this.RefreshPlaylist_Click;
                 #line default
                 #line hidden
                break;
            case 10:
                #line 72 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.MenuFlyoutItem)(target)).Click += this.UpdatePlaylist_Click;
                 #line default
                 #line hidden
                break;
            case 11:
                #line 73 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.MenuFlyoutItem)(target)).Click += this.SearchForNewSongsPlaylist_Click;
                 #line default
                 #line hidden
                break;
            case 12:
                #line 74 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.MenuFlyoutItem)(target)).Click += this.DeletePlaylist_Click;
                 #line default
                 #line hidden
                break;
            case 13:
                #line 85 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).Tapped += this.Playlist_Tapped;
                 #line default
                 #line hidden
                break;
            case 14:
                #line 80 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).Tapped += this.PlayPlaylist_Tapped;
                 #line default
                 #line hidden
                break;
            case 15:
                #line 82 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).Tapped += this.DetailPlaylist_Tapped;
                 #line default
                 #line hidden
                break;
            case 16:
                #line 26 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.FrameworkElement)(target)).DataContextChanged += this.lbxCurrentPlaylist_DataContextChanged;
                 #line default
                 #line hidden
                #line 26 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.FrameworkElement)(target)).LayoutUpdated += this.lbxCurrentPlaylist_LayoutUpdated;
                 #line default
                 #line hidden
                break;
            case 17:
                #line 29 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).Holding += this.CurrentPlaylistSong_Holding;
                 #line default
                 #line hidden
                break;
            case 18:
                #line 37 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.MenuFlyoutItem)(target)).Click += this.RefreshSong_Click;
                 #line default
                 #line hidden
                break;
            case 19:
                #line 38 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.MenuFlyoutItem)(target)).Click += this.DeleteSong_Click;
                 #line default
                 #line hidden
                break;
            case 20:
                #line 205 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.Previous_Click;
                 #line default
                 #line hidden
                break;
            case 21:
                #line 206 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.PlayPause_Click;
                 #line default
                 #line hidden
                break;
            case 22:
                #line 207 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.Next_Click;
                 #line default
                 #line hidden
                break;
            case 23:
                #line 211 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.ResetLibraryFromStorage_Click;
                 #line default
                 #line hidden
                break;
            case 24:
                #line 212 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.UpdateExistingPlaylists_Click;
                 #line default
                 #line hidden
                break;
            case 25:
                #line 213 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.AddNotExistingPlaylists_Click;
                 #line default
                 #line hidden
                break;
            case 26:
                #line 214 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.Settings_Click;
                 #line default
                 #line hidden
                break;
            case 27:
                #line 215 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.TestFunktion_Click;
                 #line default
                 #line hidden
                break;
            case 28:
                #line 216 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.TestFunktion_Click2;
                 #line default
                 #line hidden
                break;
            case 29:
                #line 217 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.AppBarButton_Click;
                 #line default
                 #line hidden
                break;
            case 30:
                #line 218 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.TestFunktion_Click3;
                 #line default
                 #line hidden
                break;
            case 31:
                #line 219 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.TestFunktion_Click4;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}


