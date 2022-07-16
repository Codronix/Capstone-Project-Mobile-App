using FireSharp.Response;
using Kerlyn_Liner_App.DataProperties;
using Kerlyn_Liner_App.DB;
using Kerlyn_Liner_App.Pages.ResetPasswordPage;
using Kerlyn_Liner_App.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

namespace Kerlyn_Liner_App.ViewModel
{
    class InspectorViewModel : INotifyPropertyChanged
    {
        Database Cloud_Database = new Database();
        public event PropertyChangedEventHandler PropertyChanged;
        InternetConnectionChecker ConnectionStatus = new InternetConnectionChecker();
        public ObservableCollection<PassengerRecords_Data> PassengerList { get; set; }
        public ObservableCollection<BusTripRecords_Data> BusData { get; set; }
        // Profile Properties
        public string Employee_Number { get; set; }
        public string FullName { get; set; }
        public string Assigned_To { get; set; }
        private BusTripRecords_Data _BusNumber;
        public BusTripRecords_Data SelectedBus
        {
            get { return _BusNumber; }
            set { _BusNumber = value; OnPropertyChanged(nameof(SelectedBus)); LoadPassengerList(SelectedBus.Travel_ID); }
        }
        public Command OpenChangePasswordPageCommand { get; set; }
        public INavigation Navigation { get; set; }
        public InspectorViewModel(string user, INavigation navigation)
        {
            Cloud_Database.Cloud_DB_Connect();
            LoadBusPicker();
            DataProfileListener(user);
            ButtonCommands(navigation);
        }
        private async void LoadBusPicker()
        {
            Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip");
            if (Cloud_Database.response.Body.ToString() != "null")
            {
                BusData = new ObservableCollection<BusTripRecords_Data>();
                Dictionary<string, BusTripRecords_Data> getData = Cloud_Database.response.ResultAs<Dictionary<string, BusTripRecords_Data>>();
                foreach (var bus in getData)
                {
                    BusData.Add(new BusTripRecords_Data()
                    {
                        Travel_ID = bus.Value.Travel_ID,
                    });
                }
                OnPropertyChanged(nameof(BusData));
            }
        }
        private async void LoadProfileData(string DB_Path)
        {
            string dateID = DateTime.Now.ToString("MMddyyyy");
            Cloud_Database.response = await Cloud_Database.client.GetAsync(DB_Path);
            if (Cloud_Database.response.Body.ToString() != "null")
            {
                Account_Data account_Data = Cloud_Database.response.ResultAs<Account_Data>();
                Employee_Number = account_Data.id;
                FullName = $"{account_Data.firstname} {account_Data.mi} {account_Data.surname}";
                Assigned_To = $"{account_Data.assigned_to} - {account_Data.position}";
            }
            OnPropertyChanged(nameof(Employee_Number));
            OnPropertyChanged(nameof(FullName));
            OnPropertyChanged(nameof(Assigned_To));
        }
        private async void PassengerListListening(string BusNumber)
        {
            if (ConnectionStatus.ConnectedToInternet())
            {
                if (await ConnectionStatus.TestConnectivity())
                {
                    EventStreamResponse response = await Cloud_Database.client.OnAsync(BusNumber,
                    added: (s, args, context) => { LoadPassengerList(BusNumber); },
                    changed: (s, args, context) => { LoadPassengerList(BusNumber); },
                    removed: (s, args, context) => { LoadPassengerList(BusNumber); });
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
        private async void LoadPassengerList(string BusNumber)
        {
            if (ConnectionStatus.ConnectedToInternet())
            {
                if (await ConnectionStatus.TestConnectivity())
                {
                    if (PassengerList != null)
                    {
                        PassengerList.Clear();
                    }
                    string dateID = DateTime.Now.ToString("MMddyyyy");
                    Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{BusNumber}/Passengers");
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
        private void ButtonCommands(INavigation navigation)
        {
            OpenChangePasswordPageCommand = new Command(async () =>
            {
                Navigation = navigation;
                await Navigation.PushAsync(new ChangePasswordPage(Employee_Number));
            });
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
