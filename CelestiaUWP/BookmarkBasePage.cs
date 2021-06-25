﻿//
// BookmarkBasePage.cs
//
// Copyright © 2021 Celestia Development Team. All rights reserved.
//
// This program is free software, you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation, either version 2
// of the License, or (at your option) any later version.
//

using CelestiaComponent;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CelestiaUWP
{
    public class BookmarkNode
    {
        public bool IsFolder;
        public string Name;
        public string URL;
        public ObservableCollection<BookmarkNode> Children;
    }

    public class BookmarkArgs
    {
        public CelestiaAppCore AppCore;
        public CelestiaRenderer Renderer;

        public BookmarkArgs(CelestiaAppCore AppCore, CelestiaRenderer Renderer)
        {
            this.AppCore = AppCore;
            this.Renderer = Renderer;
        }
    }

    public class BookmarkBasePage : Page, INotifyPropertyChanged
    {
        protected CelestiaAppCore AppCore;
        protected CelestiaRenderer Renderer;

        protected ObservableCollection<BookmarkNode> Bookmarks = new ObservableCollection<BookmarkNode>();
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var parameter = (BookmarkArgs)e.Parameter;
            AppCore = parameter.AppCore;
            Renderer = parameter.Renderer;
            ReadBookmarks();
        }

        async private void ReadBookmarks()
        {
            ObservableCollection<BookmarkNode> bookmarks;
            Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            try
            {
                var serializer = new JsonSerializer();
                using (var bookmarkStream = await localFolder.OpenStreamForReadAsync("bookmarks.json"))
                using (var sr = new StreamReader(bookmarkStream))
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    bookmarks = serializer.Deserialize<ObservableCollection<BookmarkNode>>(jsonTextReader);
                }
            }
            catch
            {
                bookmarks = new ObservableCollection<BookmarkNode>();
            }
            Bookmarks = bookmarks;
            OnPropertyChanged("Bookmarks");
        }

        async public void WriteBookmarks()
        {
            Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            try
            {
                var serializer = new JsonSerializer();
                using (var bookmarkStream = await localFolder.OpenStreamForWriteAsync("bookmarks.json", Windows.Storage.CreationCollisionOption.ReplaceExisting))
                using (var sr = new StreamWriter(bookmarkStream))
                using (var jsonTextWriter = new JsonTextWriter(sr))
                {
                    serializer.Serialize(jsonTextWriter, Bookmarks);
                }
            }
            catch { }
        }
        protected ObservableCollection<BookmarkNode> FindParent(BookmarkNode node, ObservableCollection<BookmarkNode> root)
        {
            foreach (var n in root)
            {
                if (n == node) return root;
                var p = FindParent(node, n.Children);
                if (p != null)
                    return p;
            }
            return null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
