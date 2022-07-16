using FireSharp.Response;
using Kerlyn_Liner_App.DataProperties;
using Kerlyn_Liner_App.DB;
using Kerlyn_Liner_App.Services;
using Plugin.Geolocator;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace Kerlyn_Liner_App.Pages.DriverPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DriverMapsPage : ContentPage
    {
        public static readonly BindableProperty VMStringProperty
        = BindableProperty.CreateAttached("VMString",
            typeof(string),
            typeof(DriverMapsPage),
            default(string),
            propertyChanged: VMStringChanged);
        public static string GetVMString(BindableObject target)
        {
            return (string)target.GetValue(VMStringProperty);
        }
        private static void VMStringChanged(BindableObject target, object oldValue, object newValue)
        {
            var mapsPage = target as DriverMapsPage;
            var vmString = newValue as string; // or GetVMString(target)
            mapsPage.GetBusNumber(vmString); // ApplyLogic is an instance method in your NewPage class
        }
        string Long;
        string Lat;
        string TravelID;
        int counter = 1;
        bool isSharingLoc;
        Database Cloud_Database = new Database();
        InternetConnectionChecker ConnectionStatus = new InternetConnectionChecker();
        private void GetBusNumber(string value)
        {
            TravelID = value;
        }
        private Pin MyLocation = new Pin
        {
            Label = "My Location"
        };
        public DriverMapsPage()
        {
            InitializeComponent();
            MapConfig();
            Cloud_Database.Cloud_DB_Connect();
            getLiveLocation();
            map.Pins.Add(MyLocation);
        }
        public async void getLiveLocation() // This triggers the getting live location
        {
            try
            {
                await Task.Run(() => CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(1), 0));
                PassengerListener();
            }
            catch
            {
                
            }
        }
        public void MapConfig()
        {
            map.HasZoomEnabled = true;
            map.HasScrollEnabled = true;
            map.IsShowingUser = false;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                CrossGeolocator.Current.PositionChanged += CrossGeolocator_Current_PositionChanged;
                CrossGeolocator.Current.PositionError += CrossGeolocator_Current_PositionError;
               
            }
            catch
            {
            }
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            try
            {
                CrossGeolocator.Current.PositionChanged -= CrossGeolocator_Current_PositionChanged;
                CrossGeolocator.Current.PositionError -= CrossGeolocator_Current_PositionError;
                CrossGeolocator.Current.StopListeningAsync();
                map.Pins.Clear();
            }
            catch
            {
            }
        }
        void CrossGeolocator_Current_PositionError(object sender, Plugin.Geolocator.Abstractions.PositionErrorEventArgs e)
        {
            var location_error = "Location error: " + e.Error.ToString();
            DisplayAlert("", location_error, "OK");
        }
        async void CrossGeolocator_Current_PositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
        {
            var position = e.Position;
            MyLocation.Position = new Position(position.Latitude, position.Longitude);
            map.MoveToRegion(MapSpan.FromCenterAndRadius(MyLocation.Position, Distance.FromKilometers(0.10)));
            if (isSharingLoc)
            {
                try
                {
                    Cloud_Database.response = await Cloud_Database.client.SetAsync($"BusOnTrip/{TravelID}/Location", MyLocation.Position);
                }
                catch
                {
                    await DisplayAlert("Connection Error", "You are not connected to internet.", "OK");
                }
                map.MoveToRegion(MapSpan.FromCenterAndRadius(MyLocation.Position, Distance.FromKilometers(0.10)));
            }
        }
        private async void PassengerListener()
        {
            if (ConnectionStatus.ConnectedToInternet())
            {
                if (await ConnectionStatus.TestConnectivity())
                {
                    try
                    {
                        EventStreamResponse response = await Cloud_Database.client.OnAsync($"BusOnTrip/{TravelID}/",
                        added: (s, args, context) => { Device.BeginInvokeOnMainThread(() => { LoadPassengersOnMap(); }); },
                        changed: (s, args, context) => { Device.BeginInvokeOnMainThread(() => { LoadPassengersOnMap(); }); },
                        removed: (s, args, context) => { Device.BeginInvokeOnMainThread(() => { LoadPassengersOnMap(); }); });
                    }
                    catch
                    {
                        await DisplayAlert("Internet Connection Error", "A connection problem occured.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("", "Please check your internet connection.", "OK");
                }
            }
            else
            {
                await DisplayAlert("", "You are not connected to internet.", "OK");
            }
        }
        private async void LoadPassengersOnMap()
        {
            try
            {
                map.Pins.Clear();
                Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{TravelID}/PassengerLocation/");
                if (Cloud_Database.response.Body.ToString() != "null")
                {
                    Dictionary<string, UserAccountProperties> getPassengerLocation = Cloud_Database.response.ResultAs<Dictionary<string, UserAccountProperties>>();
                    foreach (var passenger in getPassengerLocation)
                    {
                        map.Pins.Add(new Pin() { Position = new Position(passenger.Value.Latitude, passenger.Value.Longitude), Label = "Passenger" });
                    }
                }
                map.Pins.Add(MyLocation);
            }
            catch
            {
                return;
            }
        }
        private async void btnShareLocation_Clicked(object sender, EventArgs e)
        {
            string dateID = DateTime.Now.ToString("MMddyyyy");
            if (ConnectionStatus.ConnectedToInternet())
            {
                if (await ConnectionStatus.TestConnectivity())
                {
                    Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{TravelID}");
                    if (Cloud_Database.response.Body.ToString() != "null")
                    {
                        if (btnShareLocation.Text.Equals("Share Location"))
                        {
                            btnShareLocation.Text = "Stop Sharing Location";
                            isSharingLoc = true;
                        }
                        else
                        {
                            btnShareLocation.Text = "Share Location";
                            isSharingLoc = false;
                            Cloud_Database.response = await Cloud_Database.client.DeleteAsync($"BusOnTrip/{TravelID}/Location");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Share Location", "Trip for today not available.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Connection Error", "Please check your internet connection.", "OK");
                }
            }
            else
            {
                await DisplayAlert("Connection Error", "You are not connected to internet.", "OK");
            }
        }
    }
}