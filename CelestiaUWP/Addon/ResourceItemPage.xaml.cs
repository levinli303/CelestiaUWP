﻿//
// ResourceItemPage.xaml.cs
//
// Copyright © 2021 Celestia Development Team. All rights reserved.
//
// This program is free software, you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation, either version 2
// of the License, or (at your option) any later version.
//

using CelestiaComponent;
using CelestiaUWP.Helper;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace CelestiaUWP.Addon
{
    public delegate void RunScriptHandler(string scriptPath);

    public class AddonPageParameter
    {
        public CelestiaAppCore AppCore;
        public CelestiaRenderer Renderer;
        public ResourceItem Item;
        public RunScriptHandler Handler;
        public AddonPageParameter(CelestiaAppCore appCore, CelestiaRenderer renderer, ResourceItem item, RunScriptHandler handler)
        {
            this.AppCore = appCore;
            this.Renderer = renderer;
            this.Item = item;
            this.Handler = handler;
        }
    }
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)

        {
            if (value == null)
            {
                BitmapImage blankimage = new BitmapImage();
                return blankimage;
            }
            else
            {
                return new BitmapImage(new Uri(value.ToString()));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public sealed partial class ResourceItemPage : Page, INotifyPropertyChanged
    {
        private CelestiaAppCore AppCore;
        private CelestiaRenderer Renderer;
        private RunScriptHandler ScriptHandler;

        private ResourceItem mItem;
        ResourceItem Item
        {
            get => mItem;
            set
            {
                mItem = value;
                OnPropertyChanged("Item");
            }
        }

        ResourceManager.ItemState State
        {
            get
            {
                return ResourceManager.Shared.StateForItem(Item);
            }
        }

        public ResourceItemPage()
        {
            this.InitializeComponent();
            GoButton.Content = LocalizationHelper.Localize("Go");
            ActionButton.Content = LocalizationHelper.Localize("Install");
            RestartHint.Text = LocalizationHelper.Localize("Note: restarting Celestia is needed to use any new installed add-on.");
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var parameter = (AddonPageParameter)e.Parameter;
            AppCore = parameter.AppCore;
            Renderer = parameter.Renderer;
            Item = parameter.Item;
            ScriptHandler = parameter.Handler;

            ResourceManager.Shared.ProgressUpdate += Shared_ProgressUpdate;
            ResourceManager.Shared.DownloadSuccess += Shared_DownloadSuccess;
            ResourceManager.Shared.DownloadFailure += Shared_DownloadFailure;

            UpdateState();
            ReloadItem();
        }

        private void Shared_DownloadFailure(ResourceItem item)
        {
            UpdateState();
        }

        private void Shared_DownloadSuccess(ResourceItem item)
        {
            UpdateState();
        }

        private void Shared_ProgressUpdate(ResourceItem item, double progress)
        {
            if (item.id != Item.id) return;
            InstallProgressBar.Value = progress;
        }

        private void UpdateState()
        {
            GoButton.Content = LocalizationHelper.Localize(Item.type == "script" ? "Run" :"Go");

            switch (State)
            {
                case ResourceManager.ItemState.Downloading:
                    InstallProgressBar.Visibility = Visibility.Visible;
                    ActionButton.Content = LocalizationHelper.Localize("Cancel");
                    break;
                case ResourceManager.ItemState.Installed:
                    InstallProgressBar.Visibility = Visibility.Collapsed;
                    ActionButton.Content = LocalizationHelper.Localize("Uninstall");
                    break;
                case ResourceManager.ItemState.None:
                    InstallProgressBar.Visibility = Visibility.Collapsed;
                    ActionButton.Content = LocalizationHelper.Localize("Install");
                    break;
            }

            if (Item.type == "script")
            {
                var mainScriptName = Item.mainScriptName;
                if (State == ResourceManager.ItemState.Installed && mainScriptName != null)
                {
                    var path = ResourceManager.Shared.ItemPath(Item) + "\\" + mainScriptName;
                    if (File.Exists(path))
                    {
                        GoButton.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        GoButton.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    GoButton.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (State == ResourceManager.ItemState.Installed && Item.objectName != null && AppCore.Simulation.Find(Item.objectName).IsEmpty)
                {
                    GoButton.Visibility = Visibility.Visible;
                }
                else
                {
                    GoButton.Visibility = Visibility.Collapsed;
                }
            }
        }

        private async void ReloadItem()
        {
            Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient();
            var queryItems = System.Web.HttpUtility.ParseQueryString("");
            queryItems.Add("lang", LocalizationHelper.Locale);
            queryItems.Add("item", Item.id);
            var builder = new UriBuilder(Addon.Constants.APIPrefix + "/resource/item");
            builder.Query = queryItems.ToString();
            try
            {
                var httpResponse = await httpClient.GetAsync(builder.Uri);
                httpResponse.EnsureSuccessStatusCode();
                var httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
                var requestResult = JsonConvert.DeserializeObject<Addon.RequestResult>(httpResponseBody);
                if (requestResult.status != 0) return;
                var item = requestResult.Get<Addon.ResourceItem>();
                Item = item;
                UpdateState();
            }
            catch (Exception ignored)
            {}
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            switch (State)
            {
                case ResourceManager.ItemState.Downloading:
                    ResourceManager.Shared.Cancel(Item);
                    UpdateState();
                    break;
                case ResourceManager.ItemState.Installed:
                    ResourceManager.Shared.Uninstall(Item);
                    UpdateState();
                    break;
                case ResourceManager.ItemState.None:
                    ResourceManager.Shared.DownloadItem(Item);
                    UpdateState();
                    break;
            }
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            if (Item.type == "script")
            {
                var mainScriptName = Item.mainScriptName;
                if (mainScriptName == null) return;
                var path = ResourceManager.Shared.ItemPath(Item) + "\\" + mainScriptName;
                if (File.Exists(path))
                {
                    ScriptHandler(path);
                }
                return;
            }
            var objectName = Item.objectName;
            if (objectName == null) return;

            var selection = AppCore.Simulation.Find(Item.objectName);
            if (selection.IsEmpty)
            {
                ContentDialogHelper.ShowAlert(this, LocalizationHelper.Localize("Object not found."));
                return;
            }

            Renderer.EnqueueTask(() =>
            {
                AppCore.Simulation.Selection = selection;
                AppCore.CharEnter(103);
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
