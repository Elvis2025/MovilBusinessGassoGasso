
using System;
using System.Collections.Generic;
using Xamarin.Essentials;
//using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapsPage : ContentPage
    {
        private Position CurrentPosition;

        public MapsPage(List<model.Internal.Location> locations, double latitude, double longitude)
        {
            InitializeComponent();

            if (locations == null || locations.Count == 0)
            {
                Navigation.PopAsync(false);
                return;
            }

            if (latitude > 0 && longitude > 0)
            {
                CurrentPosition = new Position(latitude, longitude);
            }
            else
            {
                CurrentPosition = new Position(locations[0].Latitude, locations[0].Longitude);
            }


            mapView.MoveToRegion(MapSpan.FromCenterAndRadius(
                CurrentPosition, Distance.FromMiles(7)));

            slider.Value = 11;

            foreach (var loc in locations)
            {
                if (loc.Latitude == 0 && loc.Longitude == 0)
                {
                    continue;
                }

                var position = new Position(loc.Latitude, loc.Longitude); // Latitude, Longitude
                var pin = new Pin
                {
                    Type = PinType.Place,
                    Position = position,
                    Label = loc.Label,
                    Address = loc.Label,
                    BindingContext = loc.Position
                    
                    //Address = "custom detail info"
                };

                mapView.Pins.Add(pin);
            }
        }

        public MapsPage(double latitude, double longitude, string label = null)
        {
            InitializeComponent();

            CurrentPosition = new Position(latitude, longitude);

            mapView.MoveToRegion(MapSpan.FromCenterAndRadius(
                    CurrentPosition, Distance.FromMiles(0.4)));

            var pin = new Pin
            {
                Type = PinType.Place,
                Position = CurrentPosition,
                Label = label,
                Address = label
                //Address = "custom detail info"
            };

            mapView.Pins.Add(pin);
        }

        private void OnZoomValueChanged(object sender, ValueChangedEventArgs e)
        {
            var zoomLevel = e.NewValue; // between 1 and 18
            var latlongdegrees = 360 / (Math.Pow(2, zoomLevel));

            var pos = CurrentPosition;

            if (pos == null)
            {
                pos = mapView.VisibleRegion.Center;
            }

            mapView.MoveToRegion(new MapSpan(pos, latlongdegrees, latlongdegrees));
        }

        private void MapStreet(object sender, EventArgs args)
        {
            ResetMapTypeButtons();
            mapView.MapType = Xamarin.Forms.Maps.MapType.Street;
            btnStreet.FontAttributes = FontAttributes.Bold;
            btnStreet.TextColor = Color.FromHex("#1976D2");
        }

        private void MapHybrid(object sender, EventArgs args)
        {
            ResetMapTypeButtons();
            mapView.MapType = Xamarin.Forms.Maps.MapType.Hybrid;
            btnHybrid.FontAttributes = FontAttributes.Bold;
            btnHybrid.TextColor = Color.FromHex("#1976D2");
        }

        private void MapSatellite(object sender, EventArgs args)
        {
            ResetMapTypeButtons();
            mapView.MapType = Xamarin.Forms.Maps.MapType.Satellite;
            btnSatellite.FontAttributes = FontAttributes.Bold;
            btnSatellite.TextColor = Color.FromHex("#1976D2");
        }

        private async void Openwith(object sender, EventArgs args)
        {
            if (CurrentPosition != null)
            {
                double latitud = CurrentPosition.Latitude;
                double longitud = CurrentPosition.Longitude;
                string placeName = "Home";

                var supportsUri = await Launcher.CanOpenAsync("comgooglemaps://");

                if (supportsUri)
                    await Launcher.OpenAsync($"comgooglemaps://?q={latitud},{longitud}({placeName})");

                else
                    await Xamarin.Essentials.Map.OpenAsync(latitud, longitud, new MapLaunchOptions { Name = "Client" });

            }


        }

        private void ResetMapTypeButtons()
        {
            btnStreet.FontAttributes = FontAttributes.None;
            btnSatellite.FontAttributes = FontAttributes.None;
            btnHybrid.FontAttributes = FontAttributes.None;

            btnStreet.TextColor = Color.FromHex("#42A5F5");
            btnSatellite.TextColor = Color.FromHex("#42A5F5");
            btnHybrid.TextColor = Color.FromHex("#42A5F5");
        }
    }
}