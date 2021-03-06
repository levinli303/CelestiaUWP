//
// GotoObjectDialog.xaml.cs
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
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace CelestiaUWP
{
    public sealed partial class GotoObjectDialog : ContentDialog
    {
        public string Text
        {
            get => ObjectNameText.Text;
        }
        public string LongitudeString = "";
        public string LatitudeString = "";
        public string DistanceString = "";

        private CelestiaAppCore Core;

        public float? Latitude
        {
            get => float.TryParse(LatitudeString, out float result) ? result : (float?)null;
            set => LatitudeString = value == null ? "" : ((float)value).ToString();
        }

        public float? Longitude
        {
            get => float.TryParse(LongitudeString, out float result) ? result : (float?)null;
            set => LongitudeString = value == null ? "" : ((float)value).ToString();
        }

        public double? Distance
        {
            get => double.TryParse(DistanceString, out double result) ? result : (double?)null;
            set => DistanceString = value == null ? "" : ((double)value).ToString();
        }

        public int Unit = 0;
        private readonly string[] Units = new string[]
        {
            LocalizationHelper.Localize("km"),
            LocalizationHelper.Localize("radii"),
            LocalizationHelper.Localize("au")
        };

        public GotoObjectDialog(CelestiaAppCore core)
        {
            Core = core;
            this.InitializeComponent();
            Title = LocalizationHelper.Localize("Go to Object");
            PrimaryButtonText = LocalizationHelper.Localize("OK");
            SecondaryButtonText = LocalizationHelper.Localize("Cancel");
            ObjectNameText.PlaceholderText = LocalizationHelper.Localize("Object Name");
            LongitudeText.PlaceholderText = LocalizationHelper.Localize("Longitude");
            LatitudeText.PlaceholderText = LocalizationHelper.Localize("Latitude");
            DistanceText.PlaceholderText = LocalizationHelper.Localize("Distance");
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async void ObjectNameText_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput) return;

            var text = sender.Text;
            if (text == "")
            {
                sender.ItemsSource = new string[] { };
                return;
            }

            var results = await GetCompletion(sender.Text);

            if (ObjectNameText.Text != text) return;

            sender.ItemsSource = results;
        }

        private async Task<string[]> GetCompletion(string key)
        {
            var simulation = Core.Simulation;
            return await Task.Run(() =>
            {
                return simulation.GetCompletion(key) ?? (new string[] { });
            });
        }
    }
}
