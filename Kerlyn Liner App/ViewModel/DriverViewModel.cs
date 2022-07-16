using FireSharp.Response;
using Kerlyn_Liner_App.DataProperties;
using Kerlyn_Liner_App.DB;
using Kerlyn_Liner_App.Pages.DriverPage;
using Kerlyn_Liner_App.Pages.ResetPasswordPage;
using Kerlyn_Liner_App.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Kerlyn_Liner_App.ViewModel
{
    class DriverViewModel : INotifyPropertyChanged
    {
        Database Cloud_Database = new Database();
        InternetConnectionChecker ConnectionStatus = new InternetConnectionChecker();
        // Event Handler
        public event PropertyChangedEventHandler PropertyChanged;
        // Profile Properties
        public string Employee_Number { get; set; }
        public string FullName { get; set; }
        public string Assigned_To { get; set; }
        // Bus Profile Properties
        public string Bus_Seats { get; set; } = "N/A";
        public string Bus_Way_Point { get; set; } = "N/A";
        public string Bus_Capacity_Sitting { get; set; } = "0";
        public string Bus_Capacity_Standing { get; set; } = "0";
        public string Count_Total_Passenger { get; set; } = "~";
        // Bus Passenger Counter Properties
        public string Count_Sitting { get; set; } = "~";
        public string Count_Standing { get; set; } = "~";
        // Bus Trip Records Fields
        private string dateNow = DateTime.Now.ToString("MM/dd/yyyy");
        public string _BusNumber { get; set; }
        public string BusNumber { get; set; }
        public string BusWayPoint { get; set; }
        public string TravelID { get; set; }
        public Command OpenChangePasswordPageCommand { get; set; }
        public ObservableCollection<PassengerRecords_Data> PaymentRecords { get; set; }
        public INavigation Navigation { get; set; }
        public ObservableCollection<PassengerRecords_Data> PassengerList { get; set; }
        public bool isRefreshing { get; set; }
        public Command RefreshCommand { get; set; }
        public DriverViewModel(string path, string TravelID, INavigation navigation)
        {
            Cloud_Database.Cloud_DB_Connect();
            DataProfileListener(path);
            BusCapacityDataListener($"BusOnTrip/{TravelID}");
            LoadPassengerList(TravelID);
            ButtonCommands(navigation);
        }
        
        private async void LoadProfileData(string DB_Path)
        {
            string dateID = dateNow.Replace("/", string.Empty);
            Cloud_Database.response = await Cloud_Database.client.GetAsync(DB_Path);
            if (Cloud_Database.response.Body.ToString() != "null")
            {
                Account_Data account_Data = Cloud_Database.response.ResultAs<Account_Data>();
                Employee_Number = account_Data.id;
                FullName = $"{account_Data.firstname} {account_Data.mi} {account_Data.surname}";
                Assigned_To = $"{account_Data.assigned_to} - {account_Data.position}";
                if (account_Data.assigned_to.Equals("N/A"))
                {
                    Bus_Seats = "N/A";
                }
                else
                {
                    Cloud_Database.response = await Cloud_Database.client.GetAsync($"Bus_Data/Busses/{account_Data.assigned_to}");
                    if (Cloud_Database.response.Body.ToString() != "null")
                    {
                        Bus_Data Bus = Cloud_Database.response.ResultAs<Bus_Data>();
                        Bus_Seats = Bus.bus_seats;
                        Bus_Way_Point = Bus.bus_route;
                        _BusNumber = Bus.bus_number;
                        //PassengerListListener($"BusTripRecords_Data/{dateID}/{_BusNumber}/Payment_History/Payments");
                    }
                }
            }
            OnPropertyChanged(nameof(Employee_Number));
            OnPropertyChanged(nameof(FullName));
            OnPropertyChanged(nameof(Assigned_To));
            OnPropertyChanged(nameof(Bus_Seats));
            OnPropertyChanged(nameof(Bus_Way_Point));
            OnPropertyChanged(nameof(_BusNumber));
        }
        private async void DataProfileListener(string path)
        {
            if (ConnectionStatus.ConnectedToInternet())
            {
                if (await ConnectionStatus.TestConnectivity())
                {
                    EventStreamResponse response = await Cloud_Database.client.OnAsync(path,
                    added: (s, args, context) => { LoadProfileData(path); },
                    changed: (s, args, context) => { LoadProfileData(path); },
                    removed: (s, args, context) => { LoadProfileData(path); });
                }
                else
                {
                    DisplayMessage("Please check your internet connection.");
                }
            }
            else
            {
                DisplayMessage("You are not connected to the internet.");
            }
        }
        private async void LoadBusCapacity(string path)
        {
            string dateID = dateNow.Replace("/", string.Empty);
            Cloud_Database.response = await Cloud_Database.client.GetAsync(path);
            if (Cloud_Database.response.Body.ToString() != "null")
            {
                BusTripRecords_Data Capacity = Cloud_Database.response.ResultAs<BusTripRecords_Data>();
                Count_Sitting = Capacity.Bus_Sitting;
                Count_Standing = Capacity.Bus_Standing;
                BusNumber = Capacity.Bus_Number;
                BusWayPoint = Capacity.Bus_Route;
                TravelID = Capacity.Travel_ID;
                int Total_Passengers = int.Parse(Count_Sitting) + int.Parse(Count_Standing);
                Count_Total_Passenger = Total_Passengers.ToString();
                
            }
            OnPropertyChanged(nameof(Count_Sitting));
            OnPropertyChanged(nameof(Count_Standing));
            OnPropertyChanged(nameof(Count_Total_Passenger));
            OnPropertyChanged(nameof(BusNumber));
            OnPropertyChanged(nameof(BusWayPoint));
            OnPropertyChanged(nameof(TravelID));
        }
        private async void BusCapacityDataListener(string path)
        {
            if (ConnectionStatus.ConnectedToInternet())
            {
                if (await ConnectionStatus.TestConnectivity())
                {
                    EventStreamResponse response = await Cloud_Database.client.OnAsync(path,
                    added: (s, args, context) => { Device.BeginInvokeOnMainThread(() => { LoadBusCapacity(path); LoadPassengerList(TravelID); }); },
                    changed: (s, args, context) => { Device.BeginInvokeOnMainThread(() => { LoadBusCapacity(path); LoadPassengerList(TravelID); }); },
                    removed: (s, args, context) => { Device.BeginInvokeOnMainThread(() => { LoadBusCapacity(path); LoadPassengerList(TravelID); }); });
                }
                else
                {
                    DisplayMessage("Please check your internet connection.");
                }
            }
            else
            {
                DisplayMessage("You are not connected to internet.");
            }
        }
        private void ButtonCommands(INavigation navigation)
        {
            OpenChangePasswordPageCommand = new Command(async () => 
            {
                Navigation = navigation;
                await Navigation.PushAsync(new ChangePasswordPage(Employee_Number));
            });
            RefreshCommand = new Command(() =>
            {
                LoadPassengerList(TravelID);
                isRefreshing = false;
                OnPropertyChanged(nameof(isRefreshing));
            });
        }
        private async void LoadPassengerList(string TravelID)
        {
            if (PassengerList != null)
            {
                PassengerList.Clear();
            }
            if (ConnectionStatus.ConnectedToInternet())
            {
                if (await ConnectionStatus.TestConnectivity())
                {
                    Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{TravelID}/Passengers");
                    if (Cloud_Database.response.Body.ToString() != "null")
                    {
                        Dictionary<string, PassengerRecords_Data> getPassengers = Cloud_Database.response.ResultAs<Dictionary<string, PassengerRecords_Data>>();
                        PassengerList = new ObservableCollection<PassengerRecords_Data>();
                        foreach (var Passenger in getPassengers)
                        {
                            PassengerList.Add(new PassengerRecords_Data
                            {
                                Date_Of_Trip = Passenger.Value.Date_Of_Trip,
                                Boarded = Passenger.Value.Boarded,
                                Fare_Amount = Passenger.Value.Fare_Amount,
                                PassengerID = Passenger.Value.PassengerID,
                                PassengerName = Passenger.Value.PassengerName,
                                PassengerContactNumber = Passenger.Value.PassengerContactNumber
                            });
                        }
                        OnPropertyChanged(nameof(PassengerList));
                    }
                }
            }
        }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private async void DisplayMessage(string message) 
        {
            await Application.Current.MainPage.DisplayAlert("", message, "OK");
        }
    }
}