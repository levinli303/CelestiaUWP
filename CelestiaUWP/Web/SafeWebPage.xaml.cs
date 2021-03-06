//
// SafeWebPage.xaml.cs
//
// Copyright © 2022 Celestia Development Team. All rights reserved.
//
// This program is free software, you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation, either version 2
// of the License, or (at your option) any later version.
//

using CelestiaUWP.Helper;
using Microsoft.Web.WebView2.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CelestiaUWP.Web
{
    public sealed partial class SafeWebPage : Page
    {
        public SafeWebPage()
        {
            this.InitializeComponent();
            WebViewNotFoundView.Text = LocalizationHelper.Localize("WebView is not available.");
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var parameter = (CommonWebArgs)e.Parameter;
            string webViewVersion = null;
            try
            {
                webViewVersion = CoreWebView2Environment.GetAvailableBrowserVersionString();
            }
            catch {}
            if (webViewVersion == null)
            {
                WebViewNotFoundView.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                WebContent.Visibility = Windows.UI.Xaml.Visibility.Visible;
                WebContent.Navigate(typeof(CommonWebPage), parameter);
            }
        }
    }
}
