using FireSharp.Response;
using Kerlyn_Liner_App.DataProperties;
using Kerlyn_Liner_App.DB;
using Kerlyn_Liner_App.ViewModel;
using Plugin.Geolocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace Kerlyn_Liner_App.Pages.PassengerPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PassengerMapsPage : ContentPage
    {
        Database Cloud_Database = new Database();
        string userID;
        bool isSharingLoc;
        int counter = 1;
        string TravelID;
        private Pin MyLocation = new Pin
        {
            Label = "My Location",
        };
        
        public PassengerMapsPage(string bus_number, string user)
        {
            InitializeComponent();
            Cloud_Database.Cloud_DB_Connect();
            BindingContext = new PassengerMapViewModel(bus_number);
            PassengerMapViewModel.map = map;
            MapConfig();
            GetBusData(bus_number);
            userID = user;
            getLiveLocation();
            map.Pins.Add(MyLocation);
        }
        public void MapConfig()
        {
            map.HasZoomEnabled = true;
            map.HasScrollEnabled = true;
            map.IsShowingUser = false;
        }
        private async void GetBusData(string bus_number)
        {
            Cloud_Database.response = await Cloud_Database.client.GetAsync("BusOnTrip");
            if (Cloud_Database.response.Body.ToString() != "null")
            {
                Dictionary<string, BusTripRecords_Data> getBusData = Cloud_Database.response.ResultAs<Dictionary<string, BusTripRecords_Data>>();
                foreach (var BusInfo in getBusData)
                {
                    if (bus_number.Equals(BusInfo.Value.Bus_Number))
                    {
                        TravelID = BusInfo.Value.Travel_ID;
                        break;
                    }
                }
            }
        }
        public async void getLiveLocation() // This triggers the getting live location
        {
            try
            {
                await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(5), 0);
            }
            catch //(Exception ex)
            {
                //Xamarin.Insights.Report(ex);
                // await DisplayAlert("Uh oh", "Something went wrong, but don't worry we captured it in Xamarin Insights! Thanks.", "OK");
            }
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
        void CrossGeolocator_Current_PositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
        {
            var position = e.Position;
            MyLocation.Position = new Position(position.Latitude, position.Longitude);
            if (counter == 1)
            {
                map.MoveToRegion(MapSpan.FromCenterAndRadius(MyLocation.Position, Distance.FromKilometers(0.10)));
                counter--;
            }
            if (isSharingLoc)
            {
                StartUpdatingLoc();
            }
        }
        private async void StartUpdatingLoc()
        {
            try
            {
                Cloud_Database.response = await Cloud_Database.client.SetAsync($"BusOnTrip/{TravelID}/PassengerLocation/{userID}", MyLocation.Position);
            }
            catch
            {
                await DisplayAlert("Connection Error", "You are not connected to internet.", "OK");
            }
            map.MoveToRegion(MapSpan.FromCenterAndRadius(MyLocation.Position, Distance.FromKilometers(0.10)));
        }
        private async void btnShareLocation_Clicked(object sender, EventArgs e)
        {
            try
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
                        Cloud_Database.response = await Task.Run(() => Cloud_Database.client.DeleteAsync($"BusOnTrip/{TravelID}/PassengerLocation/{userID}"));
                    }
                }
                else
                {
                    await DisplayAlert("Share Location", "Trip for today not available.", "OK");
                }
            }
            catch
            {
                await DisplayAlert("Connection Error", "An Internet Connection Problem Occured.", "OK");
            }
        }
    }
}