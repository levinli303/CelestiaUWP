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
using CelestiaUWP.Web;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CelestiaUWP.Addon
{
    public sealed partial class ResourceItemPage : Page, INotifyPropertyChanged
    {
        private CelestiaAppCore AppCore;
        private CelestiaRenderer Renderer;

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
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var parameter = ((CelestiaAppCore, CelestiaRenderer, ResourceItem))e.Parameter;
            AppCore = parameter.Item1;
            Renderer = parameter.Item2;
            Item = parameter.Item3;

            ResourceManager.Shared.ProgressUpdate += Shared_ProgressUpdate;
            ResourceManager.Shared.DownloadSuccess += Shared_DownloadSuccess;
            ResourceManager.Shared.DownloadFailure += Shared_DownloadFailure;

            UpdateState();
            ReloadItem();

            var queryItems = System.Web.HttpUtility.ParseQueryString("");
            queryItems.Add("lang", LocalizationHelper.Locale);
            queryItems.Add("item", Item.id);
            queryItems.Add("platform", "uwp");
            queryItems.Add("titleVisibility", "visible");
            queryItems.Add("transparentBackground", "1");
            var builder = new UriBuilder("https://celestia.mobi/resources/item");
            builder.Query = queryItems.ToString();
            var args = new CommonWebArgs();
            args.Renderer = Renderer;
            args.AppCore = AppCore;
            args.Uri = builder.Uri;
            args.MatchingQueryKeys = new string[] { "item" };
            args.ContextDirectory = ResourceManager.Shared.ItemPath(Item);
            WebContent.Navigate(typeof(CommonWebPage), args);
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

            if (State == ResourceManager.ItemState.Installed && Item.objectName != null && AppCore.Simulation.Find(Item.objectName).IsEmpty)
            {
                GoButton.Visibility = Visibility.Visible;
            }
            else
            {
                GoButton.Visibility = Visibility.Collapsed;
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
