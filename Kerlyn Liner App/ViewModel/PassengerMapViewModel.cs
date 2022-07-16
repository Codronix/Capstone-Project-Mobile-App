using FireSharp.Response;
using Kerlyn_Liner_App.DataProperties;
using Kerlyn_Liner_App.DB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Kerlyn_Liner_App.ViewModel
{
    class PassengerMapViewModel : INotifyPropertyChanged
    {
        Database Cloud_Database = new Database();
        public event PropertyChangedEventHandler PropertyChanged;
        public string BusCapacity { get; set; }
        public string BusNumber { get; set; }
        public string BusWayPoint { get; set; }
        public string Count_Sitting { get; set; }
        public string Count_Standing { get; set; }
        public string Count_Total_Passenger { get; set; }
        private int Bus_SeatsLimit;
        private int _Count_Sitting;
        private int _Count_Standing;
        private bool BusNumberFound = false;
        private string TravelID = string.Empty;
        private string bus_number = string.Empty;
        public ObservableCollection<Location_Data> _Buslocation;
        public IEnumerable Locations => _Buslocation;

        public static Map map;
        private Pin BusLocation = new Pin
        {
            Label = "Bus"
        };
        public PassengerMapViewModel(string busnumber)
        {
            Cloud_Database.Cloud_DB_Connect();
            GetBusData(busnumber);
        }
        private async void LoadBusOnMap(string Travel_ID)
        {
            try
            {
                Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{Travel_ID}/Location");
                if (Cloud_Database.response.Body.ToString() != "null")
                {
                    map.Pins.Remove(BusLocation);
                    BusTripRecords_Data BusLoc = Cloud_Database.response.ResultAs<BusTripRecords_Data>();
                    BusLocation.Position = new Position(double.Parse(BusLoc.Latitude), double.Parse(BusLoc.Longitude));
                    map.Pins.Add(BusLocation);
                }
            }
            catch (Exception ex)
            {
                DisplayMessage("A internet connection problem has occured.");
            }
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
                        bus_number = BusInfo.Value.Bus_Number;
                        BusNumberFound = true;
                        break;
                    }
                }
                BusInfoListener(TravelID);
            }
        }
        private async void BusInfoListener(string Travel_ID)
        {
            try
            {
                EventStreamResponse response = await Cloud_Database.client.OnAsync($"BusOnTrip/{Travel_ID}",
               added: (s, args, context) => { Device.BeginInvokeOnMainThread(() => { LoadBusCapacityANDinfo(Travel_ID); LoadBusOnMap(Travel_ID); }); },
               changed: (s, args, context) => { Device.BeginInvokeOnMainThread(() => { LoadBusCapacityANDinfo(Travel_ID); LoadBusCapacityANDinfo(Travel_ID); LoadBusOnMap(Travel_ID); }); },
               removed: (s, args, context) => { Device.BeginInvokeOnMainThread(() => { LoadBusCapacityANDinfo(Travel_ID); LoadBusOnMap(Travel_ID); }); });
            }
            catch
            {
                DisplayMessage("A internet connection problem has occured.");
            }
        }
        private async void LoadBusCapacityANDinfo(string Travel_ID)
        {
            try
            {
                Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{Travel_ID}");
                if (Cloud_Database.response.Body.ToString() != "null")
                {
                    BusTripRecords_Data Capacity = Cloud_Database.response.ResultAs<BusTripRecords_Data>();
                    BusNumber = Capacity.Bus_Number;
                    BusWayPoint = Capacity.Bus_Route;
                    BusCapacity = Capacity.Bus_Capacity;
                    Count_Sitting = Capacity.Bus_Sitting;
                    _Count_Sitting = int.Parse(Capacity.Bus_Sitting);
                    Count_Standing = Capacity.Bus_Standing;
                    _Count_Standing = int.Parse(Capacity.Bus_Standing);
                    int Total_Passengers = int.Parse(Count_Sitting) + int.Parse(Count_Standing);
                    Count_Total_Passenger = Total_Passengers.ToString();

                }
                OnPropertyChanged(nameof(BusNumber));
                OnPropertyChanged(nameof(BusWayPoint));
                OnPropertyChanged(nameof(BusCapacity));
                OnPropertyChanged(nameof(Count_Sitting));
                OnPropertyChanged(nameof(Count_Standing));
                OnPropertyChanged(nameof(Count_Total_Passenger));
            }
            catch
            {
                DisplayMessage("A internet connection problem has occured.");
            }
        }
        
        private async void DisplayMessage(string msg)
        {
            await Application.Current.MainPage.DisplayAlert("", msg, "Reconnect");
        }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
