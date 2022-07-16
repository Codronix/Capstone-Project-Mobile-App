using Kerlyn_Liner_App.DataProperties;
using Kerlyn_Liner_App.DB;
using Kerlyn_Liner_App.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Kerlyn_Liner_App.ViewModel
{
    class BookerPassengerListViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        InternetConnectionChecker ConnectionStatus = new InternetConnectionChecker();
        Database Cloud_Database = new Database();
        public ObservableCollection<PassengerRecords_Data> PassengerList { get; set; }
        public Command RefundPayment { get; set; }
        public Command TransferPassengerCommand { get; set; }
        public Command TransferAllPassengerCommand { get; set; }
        public string Travel_ID { get; set; }
        public Command SearchTravelIDCommand { get; set; }
        PassengerRecords_Data _SelectedPassenger;
        public PassengerRecords_Data SelectedPassenger
        {
            get { return _SelectedPassenger; }
            set
            {
                if (_SelectedPassenger != value)
                {
                    _SelectedPassenger = value;
                }
            }
        }
        private int Bus_SeatsLimit;
        private int _Count_Sitting;
        private int _Count_Standing;
        private bool BusNumberFound = false;
        private string BusNumber = string.Empty;
        private static Random random = new Random();
        private string RefID = string.Empty;
        public BookerPassengerListViewModel(string Employee_Number)
        {
            Cloud_Database.Cloud_DB_Connect();
            ButtonCommands(Employee_Number);
        }
        private async void LoadBusInfo()
        {
            Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{Travel_ID}");
            if (Cloud_Database.response.Body.ToString() != "null")
            {
                BusTripRecords_Data BusInfo = Cloud_Database.response.ResultAs<BusTripRecords_Data>();
                BusNumber = BusInfo.Bus_Number;
            }
        }
        private async void LoadPassengerList()
        {
            if (ConnectionStatus.ConnectedToInternet())
            {
                if (await ConnectionStatus.TestConnectivity())
                {
                    if (PassengerList != null)
                    {
                        PassengerList.Clear();
                    }
                    Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip_Passenger_Records/{Travel_ID}");
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
        private static string GenerateReferenceID(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
        private void ButtonCommands(string Employee_Number)
        {
            SearchTravelIDCommand = new Command(async () =>
            {
                LoadBusInfo();
                LoadPassengerList();
            });
            RefundPayment = new Command(async () =>
            {
                try
                {
                    if (SelectedPassenger == null)
                    {
                        return;
                    }
                    if (ConnectionStatus.ConnectedToInternet())
                    {
                        if (await ConnectionStatus.TestConnectivity())
                        {
                            RefID = GenerateReferenceID(12);
                            Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{Travel_ID}");
                            if (Cloud_Database.response.Body.ToString() != "null")
                            {
                                BusTripRecords_Data Bus = Cloud_Database.response.ResultAs<BusTripRecords_Data>();
                                Cloud_Database.response = await Cloud_Database.client.GetAsync($"ConductorConfirmation/{Travel_ID}/Passengers");
                                if (Cloud_Database.response.Body.ToString() != "null")
                                {
                                    Cloud_Database.response = await Cloud_Database.client.GetAsync($"ConductorConfirmation/{Travel_ID}/Passengers/{SelectedPassenger.PassengerID}");
                                    if (Cloud_Database.response.Body.ToString() != "null")
                                    {
                                        PassengerRecords_Data Passenger = Cloud_Database.response.ResultAs<PassengerRecords_Data>();
                                        PassengerRecords_Data PassengerToRefund = new PassengerRecords_Data()
                                        {
                                            Date_Of_Trip = DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"),
                                            Boarded = $"Unboarded {Bus.Bus_Number}",
                                            Fare_Amount = $"-{Passenger.Fare_Amount}",
                                            PassengerID = Passenger.PassengerID,
                                            PassengerStatus = "Refunded",
                                            PassengerName = Passenger.PassengerName,
                                            PassengerContactNumber = Passenger.PassengerContactNumber
                                        };
                                        bool isRefund = await Application.Current.MainPage.DisplayAlert("Refund", $"Continue refund for {Passenger.PassengerID}: {Passenger.PassengerName}", "Yes", "No");
                                        if (isRefund)
                                        {
                                            Cloud_Database.response = await Task.Run(() => Cloud_Database.client.PushAsync($"PassengerHistory/{DateTime.Now.ToString("MMddyyyy")}/{SelectedPassenger.PassengerID}", PassengerToRefund));
                                            Cloud_Database.response = await Task.Run(() => Cloud_Database.client.DeleteAsync($"ConductorConfirmation/{Travel_ID}/Passengers/{SelectedPassenger.PassengerID}"));
                                            Cloud_Database.response = await Task.Run(() => Cloud_Database.client.DeleteAsync($"BusOnTrip_Passenger_Records/{Travel_ID}/{SelectedPassenger.PassengerID}"));
                                            Transactions_Data transactions_Data = new Transactions_Data
                                            {
                                                Account_ID = Passenger.PassengerID,
                                                Transac_Date = DateTime.Now.ToString("MM/dd/yyyy h:mm tt"),
                                                Reference_ID = RefID,
                                                Amount = $"-{Passenger.Fare_Amount}",
                                                Purpose = "Refund"
                                            };
                                            Cloud_Database.response = await Cloud_Database.client.PushAsync($"Transactions_Data/{Employee_Number}", transactions_Data);
                                            DisplayMessage($"Passenger {SelectedPassenger.PassengerID} Refunded: -{Passenger.Fare_Amount}");
                                            await ProcessPayment(DateTime.Now.ToString("MMddyyyy"), Passenger.Fare_Amount, "Refund");
                                            
                                        }
                                    }
                                }
                                else
                                {
                                    Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{Travel_ID}/Passengers/{SelectedPassenger.PassengerID}");
                                    if (Cloud_Database.response.Body.ToString() != "null")
                                    {
                                        PassengerRecords_Data Passenger = Cloud_Database.response.ResultAs<PassengerRecords_Data>();
                                        PassengerRecords_Data PassengerToRefund = new PassengerRecords_Data()
                                        {
                                            Date_Of_Trip = DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"),
                                            Boarded = $"Unboarded {Bus.Bus_Number}",
                                            Fare_Amount = $"-{Passenger.Fare_Amount}",
                                            PassengerID = Passenger.PassengerID,
                                            PassengerStatus = "Refunded",
                                            PassengerName = Passenger.PassengerName,
                                            PassengerContactNumber = Passenger.PassengerContactNumber
                                        };
                                        bool isRefund = await Application.Current.MainPage.DisplayAlert("Refund", $"Continue refund for {Passenger.PassengerID}: {Passenger.PassengerName}", "Yes", "No");
                                        if (isRefund)
                                        {
                                            Cloud_Database.response = await Task.Run(() => Cloud_Database.client.PushAsync($"PassengerHistory/{DateTime.Now.ToString("MMddyyyy")}/{SelectedPassenger.PassengerID}", PassengerToRefund));
                                            Cloud_Database.response = await Task.Run(() => Cloud_Database.client.DeleteAsync($"BusOnTrip/{Travel_ID}/Passengers/{SelectedPassenger.PassengerID}"));
                                            Transactions_Data transactions_Data = new Transactions_Data
                                            {
                                                Account_ID = Passenger.PassengerID,
                                                Transac_Date = DateTime.Now.ToString("MM/dd/yyyy h:mm tt"),
                                                Reference_ID = RefID,
                                                Amount = $"-{Passenger.Fare_Amount}",
                                                Purpose = "Refund"
                                            };
                                            Cloud_Database.response = await Cloud_Database.client.PushAsync($"Transactions_Data/{Employee_Number}", transactions_Data);
                                            DisplayMessage($"Passenger {SelectedPassenger.PassengerID} Refunded: -{Passenger.Fare_Amount}");
                                            await ProcessPayment(DateTime.Now.ToString("MMddyyyy"), Passenger.Fare_Amount, "Refund");
                                            
                                        }
                                    }
                                }
                            }
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
                    SelectedPassenger = null;
                    OnPropertyChanged(nameof(SelectedPassenger));
                }
                catch (Exception ex)
                {
                    DisplayMessage(ex.ToString());
                }
            });
            TransferAllPassengerCommand = new Command(async () =>
            {
                string bus_number = await Application.Current.MainPage.DisplayPromptAsync("Tranfer All Passengers", "Transfer to Bus Number:", keyboard: Keyboard.Numeric);
                bool BusNumberFound = false;
                string travel_id = string.Empty;
                if (bus_number != null)
                {
                    if (ConnectionStatus.ConnectedToInternet())
                    {
                        if (await ConnectionStatus.TestConnectivity())
                        {
                            Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/");
                            if (Cloud_Database.response.Body.ToString() != "null")
                            {
                                Dictionary<string, BusTripRecords_Data> getBus = Cloud_Database.response.ResultAs<Dictionary<string, BusTripRecords_Data>>();
                                foreach (var bus in getBus)
                                {
                                    if (bus.Value.Bus_Number.Equals(bus_number))
                                    {
                                        travel_id = bus.Value.Travel_ID;
                                        BusNumberFound = true;
                                        break;
                                    }
                                }
                                if (BusNumberFound)
                                {
                                    Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{Travel_ID}/Passengers");
                                    Dictionary<string, PassengerRecords_Data> getPassengers = Cloud_Database.response.ResultAs<Dictionary<string, PassengerRecords_Data>>();
                                    foreach (var Passenger in getPassengers)
                                    {
                                        PassengerRecords_Data PassengerToTransfer = new PassengerRecords_Data()
                                        {
                                            Date_Of_Trip = DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"),
                                            Boarded = $"Boarded {bus_number}({travel_id})",
                                            Fare_Amount = Passenger.Value.Fare_Amount,
                                            PassengerID = Passenger.Value.PassengerID,
                                            PassengerName = Passenger.Value.PassengerName,
                                            PassengerStatus = $"Transfered from {BusNumber}({Travel_ID})",
                                            PassengerContactNumber = Passenger.Value.PassengerContactNumber
                                        };
                                        Cloud_Database.response = await Task.Run(() => Cloud_Database.client.SetAsync($"BusOnTrip/{travel_id}/Passengers/{Passenger.Value.PassengerID}", PassengerToTransfer));
                                        Cloud_Database.response = await Task.Run(() => Cloud_Database.client.PushAsync($"PassengerHistory/{DateTime.Now.ToString("MMddyyyy")}/{Passenger.Value.PassengerID}", PassengerToTransfer));
                                        Cloud_Database.response = await Task.Run(() => Cloud_Database.client.DeleteAsync($"BusOnTrip/{Travel_ID}/Passengers/{Passenger.Value.PassengerID}"));
                                    }
                                    LoadPassengerList();
                                    DisplayMessage($"All passengers has been transfered to {bus_number}({travel_id}).");
                                }
                                else
                                {
                                    DisplayMessage("Entered Bus Number is not available.");
                                }
                            }
                            else
                            {
                                DisplayMessage("No busses currently on trip yet.");
                            }
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
            });
            TransferPassengerCommand = new Command(async () =>
            {
                if (SelectedPassenger == null)
                {
                    return;
                }
                string bus_number = await Application.Current.MainPage.DisplayPromptAsync("Tranfer Passenger", "Transfer to Bus Number:", keyboard: Keyboard.Numeric);
                bool BusNumberFound = false;
                string travel_id = string.Empty;
                if (bus_number != null)
                {
                    if (ConnectionStatus.ConnectedToInternet())
                    {
                        if (await ConnectionStatus.TestConnectivity())
                        {
                            Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/");
                            if (Cloud_Database.response.Body.ToString() != "null")
                            {
                                Dictionary<string, BusTripRecords_Data> getBus = Cloud_Database.response.ResultAs<Dictionary<string, BusTripRecords_Data>>();
                                foreach (var bus in getBus)
                                {
                                    if (bus.Value.Bus_Number.Equals(bus_number))
                                    {
                                        travel_id = bus.Value.Travel_ID;
                                        BusNumberFound = true;
                                        break;
                                    }
                                }
                                if (BusNumberFound)
                                {
                                    Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{Travel_ID}/Passengers/{SelectedPassenger.PassengerID}");
                                    PassengerRecords_Data Passenger = Cloud_Database.response.ResultAs<PassengerRecords_Data>();
                                    PassengerRecords_Data PassengerToTransfer = new PassengerRecords_Data()
                                    {
                                        Date_Of_Trip = DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"),
                                        Boarded = $"Boarded {bus_number}({travel_id})",
                                        Fare_Amount = Passenger.Fare_Amount,
                                        PassengerID = Passenger.PassengerID,
                                        PassengerName = Passenger.PassengerName,
                                        PassengerStatus = $"Transfered from {BusNumber}({Travel_ID})",
                                        PassengerContactNumber = Passenger.PassengerContactNumber
                                    };
                                    Cloud_Database.response = await Task.Run(() => Cloud_Database.client.SetAsync($"BusOnTrip/{travel_id}/Passengers/{SelectedPassenger.PassengerID}", PassengerToTransfer));
                                    Cloud_Database.response = await Task.Run(() => Cloud_Database.client.PushAsync($"PassengerHistory/{DateTime.Now.ToString("MMddyyyy")}/{SelectedPassenger.PassengerID}", PassengerToTransfer));
                                    Cloud_Database.response = await Task.Run(() => Cloud_Database.client.DeleteAsync($"BusOnTrip/{Travel_ID}/Passengers/{SelectedPassenger.PassengerID}"));
                                    LoadPassengerList();
                                    DisplayMessage($"{SelectedPassenger.PassengerID} has been transfered to {bus_number}({travel_id})");
                                }
                                else
                                {
                                    DisplayMessage("Entered Bus Number is not available.");
                                }
                            }
                            else
                            {
                                DisplayMessage("No busses currently on trip yet.");
                            }
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
                SelectedPassenger = null;
                OnPropertyChanged(nameof(SelectedPassenger));
            });
        }
        private async Task ProcessPayment(string dateID, string Fare, string mode)
        {
            Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{Travel_ID}");
            if (Cloud_Database.response.Body.ToString() != "null")
            {
                BusTripRecords_Data BusCapacity = Cloud_Database.response.ResultAs<BusTripRecords_Data>();
                if (int.Parse(BusCapacity.Bus_Capacity) != int.Parse(BusCapacity.Bus_Sitting))
                {
                    if (mode.Equals("Board"))
                    {
                        _Count_Sitting++;
                        await UpdateCapacity(dateID, "Sit", Travel_ID);
                    }
                    else if (mode.Equals("Refund"))
                    {
                        _Count_Sitting--;
                        await UpdateCapacity(dateID, "Sit", Travel_ID);
                    }
                }
                else
                {
                    if (mode.Equals("Board"))
                    {
                        _Count_Standing++;
                        await UpdateCapacity(dateID, "Stand", Travel_ID);

                    }
                    else if (mode.Equals("Refund"))
                    {
                        if (int.Parse(BusCapacity.Bus_Capacity) == int.Parse(BusCapacity.Bus_Sitting) && BusCapacity.Bus_Standing.Equals("0"))
                        {
                            _Count_Sitting--;
                            await UpdateCapacity(dateID, "Sit", Travel_ID);
                        }
                        else
                        {
                            _Count_Standing--;
                            await UpdateCapacity(dateID, "Stand", Travel_ID);
                        }
                    }
                }
                //await UpdatePassengerRecords(dateID, Fare, mode, num_passenger);
                await UpdateTotalProfit(dateID, Fare, mode);
                LoadPassengerList();
            }
        }
        private async Task UpdateTotalProfit(string dateID, string Fare, string mode)
        {
            string Total_Profit_Path = $"BusOnTrip/{Travel_ID}/Total_Profit";
            Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{Travel_ID}");
            BusTripRecords_Data Payment_History = Cloud_Database.response.ResultAs<BusTripRecords_Data>();
            double Profit = double.Parse(Payment_History.Total_Profit);
            if (mode.Equals("Board"))
            {
                Profit = double.Parse(Payment_History.Total_Profit) + double.Parse(Fare);
                double Total_Profit = Profit;
                Cloud_Database.response = await Cloud_Database.client.SetAsync(Total_Profit_Path, Total_Profit.ToString("0,0.00"));
            }
            else if (mode.Equals("Refund"))
            {
                Profit = double.Parse(Payment_History.Total_Profit) - double.Parse(Fare);
                double Total_Profit = Profit;
                Cloud_Database.response = await Cloud_Database.client.SetAsync(Total_Profit_Path, Total_Profit.ToString("0,0.00"));
            }
        }
        private async Task UpdateCapacity(string dateID, string pos, string BusNumber)
        {
            if (pos.Equals("Sit"))
            {
                Cloud_Database.response = await Cloud_Database.client.SetAsync($"BusOnTrip/{Travel_ID}/Bus_Sitting", _Count_Sitting.ToString());
            }
            else if (pos.Equals("Stand"))
            {
                Cloud_Database.response = await Cloud_Database.client.SetAsync($"BusOnTrip/{Travel_ID}/Bus_Standing", _Count_Standing.ToString());
            }
        }
        private async void DisplayMessage(string message)
        {
            await Application.Current.MainPage.DisplayAlert("", message, "OK");
        }
    }

}
