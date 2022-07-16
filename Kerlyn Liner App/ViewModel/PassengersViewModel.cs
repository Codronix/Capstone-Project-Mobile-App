using FireSharp.Response;
using Kerlyn_Liner_App.DataProperties;
using Kerlyn_Liner_App.DB;
using Kerlyn_Liner_App.Pages.PassengerPage;
using Kerlyn_Liner_App.Pages.ResetPasswordPage;
using Kerlyn_Liner_App.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Kerlyn_Liner_App.ViewModel
{
    class PassengersViewModel : INotifyPropertyChanged
    {
        Database Cloud_Database = new Database();
        public event PropertyChangedEventHandler PropertyChanged;
        InternetConnectionChecker ConnectionStatus = new InternetConnectionChecker();
        public ObservableCollection<Transactions_Data> Transaction_LIST { get; set; }
        public ObservableCollection<BusTripRecords_Data> OnTrip_LIST { get; set; }
        public INavigation Navigation { get; set; }
        public string Account_ID { get; set; }
        public string FullName { get; set; }
        public string Account_Balance { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public Command OpenQRCommand { get; set; }
        public Command SelectionCommand { get; set; }
        BusTripRecords_Data selectedBus;
        public BusTripRecords_Data SelectedBus
        {
            get { return selectedBus; }
            set
            {
                if (selectedBus != value)
                {
                    selectedBus = value;
                }
            }
        }
        public Command OpenChangePasswordPageCommand { get; set; }
        public Command OpenSettingsCommand { get; set; }
        public PassengersViewModel(string user, INavigation navigation)
        {
            Cloud_Database.Cloud_DB_Connect();
            UserData_Listener(user);
            TransactionsrData_Listener(user);
            BussesOnTrip_Listener();
            OpenQRCommand = new Command(async () =>
            {
                Navigation = navigation;
                await Navigation.PushModalAsync(new UserQRPage(user));
            });
            SelectionCommand = new Command(async () =>
            {
                Navigation = navigation;
                if (selectedBus == null)
                {
                    return;
                }
                await Navigation.PushModalAsync(new PassengerMapsPage(selectedBus.Bus_Number, Account_ID));
                selectedBus = null;
                OnPropertyChanged(nameof(SelectedBus));
            });
            OpenSettingsCommand = new Command(async () =>
            {
                Navigation = navigation;
                await Navigation.PushAsync(new PassengerSettingsPage(Account_ID));
            });
        }
        private async void LoadTransactionData(object user)
        {
            if (Transaction_LIST != null)
            {
                Transaction_LIST.Clear();
            }
            if (ConnectionStatus.ConnectedToInternet())
            {
                if (await ConnectionStatus.TestConnectivity())
                {
                    Cloud_Database.response = await Cloud_Database.client.GetAsync($"Transactions_Data/{user}");
                    if (Cloud_Database.response.Body.ToString() != "null")
                    {
                        Dictionary<string, Transactions_Data> Transactions = Cloud_Database.response.ResultAs<Dictionary<string, Transactions_Data>>();
                        Transaction_LIST = new ObservableCollection<Transactions_Data>();
                        foreach (var Transac_Data in Transactions)
                        {
                            Transaction_LIST.Add(new Transactions_Data
                            {
                                Account_ID = Transac_Data.Value.Account_ID,
                                Reference_ID = Transac_Data.Value.Reference_ID,
                                Purpose = Transac_Data.Value.Purpose,
                                Amount = Transac_Data.Value.Amount,
                                Transac_Date = Transac_Data.Value.Transac_Date
                            });
                        }
                        OnPropertyChanged(nameof(Transaction_LIST));
                    }
                }
            }
        }
        private async void LoadUserData(string user)
        {
            Cloud_Database.response = await Cloud_Database.client.GetAsync(user);
            if (Cloud_Database.response.Body.ToString() != "null")
            {
                UserAccountProperties account_data = Cloud_Database.response.ResultAs<UserAccountProperties>();
                Account_ID = account_data.ID;
                Username = account_data.Username;
                Email = account_data.Email;
                FullName = $"{account_data.FirstName} {account_data.LastName}";
                Account_Balance = account_data.Account_Balance;
                
            }
            OnPropertyChanged(nameof(Account_ID));
            OnPropertyChanged(nameof(Username));
            OnPropertyChanged(nameof(FullName));
            OnPropertyChanged(nameof(Account_Balance));
        }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private async void TransactionsrData_Listener(string user)
        {
            if (ConnectionStatus.ConnectedToInternet())
            {
                if (await ConnectionStatus.TestConnectivity())
                {
                    EventStreamResponse response = await Cloud_Database.client.OnAsync($"Transactions_Data/{user}",
                    added: (s, args, context) => { LoadTransactionData(user); },
                    changed: (s, args, context) => { LoadTransactionData(user); },
                    removed: (s, args, context) => { LoadTransactionData(user); });
                }
                else
                {
                    DisplayMessage("Please check your internet connection.");
                }
            }
            else
            {
                DisplayMessage("You are not connect to internet");
            }
        }
        private async void UserData_Listener(string user)
        {
            if (ConnectionStatus.ConnectedToInternet())
            {
                if (await ConnectionStatus.TestConnectivity())
                {
                    EventStreamResponse response = await Cloud_Database.client.OnAsync($"Users/{user}",
                    added: (s, args, context) => { LoadUserData($"Users/{user}"); },
                    changed: (s, args, context) => { LoadUserData($"Users/{user}"); },
                    removed: (s, args, context) => { LoadUserData($"Users/{user}"); });
                }
                else
                {
                    DisplayMessage("Please check your internet connection.");
                }
            }
            else
            {
                DisplayMessage("You are not connect to internet");
            }
        }
        private async void BussesOnTrip_Listener()
        {
            if (ConnectionStatus.ConnectedToInternet())
            {
                if (await ConnectionStatus.TestConnectivity())
                {
                    EventStreamResponse response = await Cloud_Database.client.OnAsync($"BusOnTrip/",
                    added: (s, args, context) => { LoadBusses_OnTrip(); },
                    changed: (s, args, context) => { LoadBusses_OnTrip(); },
                    removed: (s, args, context) => { LoadBusses_OnTrip(); });
                }
                else
                {
                    DisplayMessage("Please check your internet connection.");
                }
            }
            else
            {
                DisplayMessage("You are not connect to internet");
            }
        }
        private async void LoadBusses_OnTrip()
        {
            if (OnTrip_LIST != null)
            {
                OnTrip_LIST.Clear();
            }
            Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/");
            if (Cloud_Database.response.Body.ToString() != "null")
            {
                Dictionary<string, BusTripRecords_Data> Busses_On_Trip = Cloud_Database.response.ResultAs<Dictionary<string, BusTripRecords_Data>>();
                OnTrip_LIST = new ObservableCollection<BusTripRecords_Data>();
                foreach (var get in Busses_On_Trip)
                {
                    OnTrip_LIST.Add(new BusTripRecords_Data
                    {
                        Bus_Number = get.Value.Bus_Number,
                        Bus_Route = get.Value.Bus_Route,
                        Bus_Capacity = get.Value.Bus_Capacity,
                        Bus_Sitting = get.Value.Bus_Sitting,
                        Bus_Standing = get.Value.Bus_Standing
                    });
                }
                OnPropertyChanged(nameof(OnTrip_LIST));
            }
        }
        private async void DisplayMessage(string msg)
        {
            await Application.Current.MainPage.DisplayAlert("", msg, "Reconnect");
        }
    }
}