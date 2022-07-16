using FireSharp.Response;
using Kerlyn_Liner_App.DataProperties;
using Kerlyn_Liner_App.DB;
using Kerlyn_Liner_App.Pages.BookerPage;
using Kerlyn_Liner_App.Pages.ConductorPage;
using Kerlyn_Liner_App.Pages.ResetPasswordPage;
using Kerlyn_Liner_App.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace Kerlyn_Liner_App.ViewModel
{
    class ConductorViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        Database Cloud_Database = new Database();
        InternetConnectionChecker ConnectionStatus = new InternetConnectionChecker();
        // Profile Properties
        public string Employee_Number { get; set; }
        public string FullName { get; set; }
        public string Assigned_To { get; set; }
        // Bus Profile Properties
        private string _Bus_Fare;
        private string _Way_Point;
        private string _Bus_Number;
        private string dateNow = DateTime.Now.ToString("MM/dd/yyyy");
        public string Bus_Seats { get; set; } = "N/A";
        public string Bus_Way_Point { get; set; } = "N/A";
        public string Bus_Capacity_Sitting { get; set; } = "0";
        public string Bus_Capacity_Standing { get; set; } = "0";
        // Bus Passenger Counter Properties
        public string Count_Sitting { get; set; } = "~";
        public string Count_Standing { get; set; } = "~";
        public string Count_Total_Passenger { get; set; } = "~";
        private int Bus_SeatsLimit;
        private int _Count_Sitting;
        private int _Count_Standing;
        // Payment Calculation Properties
        private string _AmountPaid;
        public Command TextChangedCommand;
        private string passengerName;
        private string passengerContactNum;

        public Command RegisterPayment { get; set; }
        public Command RefundPayment { get; set; }
        public Command ScanQRCommand { get; set; }
        public Command UnboardPassenger { get; set; }
        public Command OpenTransactions { get; set; }
        public Command CancelPassengerCommand { get; set; }
        public INavigation Navigation { get; set; }
        public ObservableCollection<PassengerRecords_Data> PassengerList { get; set; }
        public string AmountPaid
        {
            get { return _AmountPaid; }
            set
            {
                _AmountPaid = value;
                TextChangedCommand.Execute(_AmountPaid);
            }
        }
        public string AmountChange { get; set; }
        public bool isSeniorDiscount_Checked { get; set; }
        public bool isFreeOfCharge { get; set; }
        public bool isNormalPay { get; set; } = true;
        public string SeniorDiscount { get; set; }
        public string ConfirmationCount { get; set; }
        public string Bus_Fare
        {
            get { return _Bus_Fare; }
            set
            {
                _Bus_Fare = value;
            }
        }
        public ObservableCollection<PassengerRecords_Data> PassengerConfirmation { get; set; }
        public string PassengerName { get => passengerName; set => passengerName = value; }
        public string PassengerContactNum { get => passengerContactNum; set => passengerContactNum = value; }
        public Command ConfirmPassengerCommand { get; set; }
        PassengerRecords_Data _SelectedPassenger_Confirmation;
        public PassengerRecords_Data SelectedPassenger_Confirmation
        {
            get { return _SelectedPassenger_Confirmation; }
            set
            {
                if (_SelectedPassenger_Confirmation != value)
                {
                    _SelectedPassenger_Confirmation = value;
                }
            }
        }
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
        public Command TransferPassengerCommand { get; set; }
        public Command TransferAllPassengerCommand { get; set; }
        public string ID_Number, ID_Alphabet;
        public string GeneratedID { get; set; }
        public Command NewPassengerCommand { get; set; }
        public string NewPassenger_TEXT { get; set; } = "NEW";
        private bool _isNewEntry = false;
        public bool isNewEntry
        {
            get => _isNewEntry;
            set
            {
                _isNewEntry = value;
                OnPropertyChanged(nameof(isNewEntry));
            }
        }
        public ObservableCollection<Bus_Data> PassengerFareList { get; set; }
        private Bus_Data _SelectedAreaOfStop;
        public Bus_Data SelectedAreaOfStop
        {
            get { return _SelectedAreaOfStop; }
            set
            {
                _SelectedAreaOfStop = value;
                OnPropertyChanged(nameof(SelectedAreaOfStop));
                Bus_Fare = SelectedAreaOfStop.bus_fare;
                OnPropertyChanged(nameof(Bus_Fare));
            }
        }

        public Command OpenChangePasswordPageCommand { get; set; }
        public string BusNumber { get; set; }
        public string BusWayPoint { get; set; }
        public string TravelID { get; set; }
        private bool isScannedPay { get; set; } = false;
        private static Random random = new Random();
        private string RefID = string.Empty;
        public ObservableCollection<PassengerRecords_Data> OldPassengersList { get; set; }
        private PassengerRecords_Data _SelectedOldPassenger;
        public PassengerRecords_Data SelectedOldPassenger
        {
            get { return _SelectedOldPassenger; }
            set
            {
                _SelectedOldPassenger = value;
                OnPropertyChanged(nameof(SelectedOldPassenger));
                GeneratedID = SelectedOldPassenger.PassengerID;
                PassengerName = SelectedOldPassenger.PassengerName;
                PassengerContactNum = SelectedOldPassenger.PassengerContactNumber;
                OnPropertyChanged(nameof(GeneratedID));
                OnPropertyChanged(nameof(PassengerName));
                OnPropertyChanged(nameof(PassengerContactNum));
            }
        }
        public bool isRefreshing { get; set; }
        public Command RefreshCommand { get; set; }
        public ConductorViewModel(string path, string TravelID, INavigation navigation)
        {
            Cloud_Database.Cloud_DB_Connect();
            ReloadPassengerConfirmationList(TravelID);
            DataProfileListener(path, navigation);
            BusCapacityDataListener(TravelID, navigation);
            LoadOldPassengerPicker();
            ButtonCommands(navigation);
            TextChangeCommand();
            GenerateID();
        }
        private async void LoadOldPassengerPicker()
        {
            Cloud_Database.response = await Cloud_Database.client.GetAsync($"PassengerRecords");
            if (Cloud_Database.response.Body.ToString() != "null")
            {
                OldPassengersList = new ObservableCollection<PassengerRecords_Data>();
                Dictionary<string, PassengerRecords_Data> getData = Cloud_Database.response.ResultAs<Dictionary<string, PassengerRecords_Data>>();
                foreach (var Passengers in getData)
                {
                    OldPassengersList.Add(new PassengerRecords_Data
                    {
                        PassengerID = Passengers.Value.PassengerID,
                        PassengerName = Passengers.Value.PassengerName,
                        PassengerContactNumber = Passengers.Value.PassengerContactNumber
                    });
                }
                OnPropertyChanged(nameof(OldPassengersList));
            }
        }
        private async void LoadPassengerFareList(string BusNumber)
        {
            Cloud_Database.response = await Cloud_Database.client.GetAsync($"Bus_Data/Busses/{BusNumber}/PassengerFareList/");
            if (Cloud_Database.response.Body.ToString() != "null")
            {
                PassengerFareList = new ObservableCollection<Bus_Data>();
                Dictionary<string, Bus_Data> getData = Cloud_Database.response.ResultAs<Dictionary<string, Bus_Data>>();
                foreach (var stops in getData)
                {
                    PassengerFareList.Add(new Bus_Data
                    {
                        bus_stop = stops.Value.bus_stop,
                        bus_fare = stops.Value.bus_fare
                    });
                }
                OnPropertyChanged(nameof(PassengerFareList));
            }
        }
        //public async void LoadPassengerConfirmation(string path, string Travel_ID)
        //{
        //    int counter = 0;
        //    if (ConnectionStatus.ConnectedToInternet())
        //    {
        //        if (await ConnectionStatus.TestConnectivity())
        //        {
        //            PassengerConfirmation = new ObservableCollection<PassengerRecords_Data>();
        //            LoadPassengerList(Travel_ID);
        //            Cloud_Database.response = await Cloud_Database.client.GetAsync($"ConductorConfirmation/{Travel_ID}/Passengers");
        //            if (Cloud_Database.response.Body.ToString() != "null")
        //            {
        //                Dictionary<string, PassengerRecords_Data> getPassengers = Cloud_Database.response.ResultAs<Dictionary<string, PassengerRecords_Data>>();
        //                foreach (var Passenger in getPassengers)
        //                {
        //                    PassengerConfirmation.Add(new PassengerRecords_Data
        //                    {
        //                        Date_Of_Trip = Passenger.Value.Date_Of_Trip,
        //                        Boarded = Passenger.Value.Boarded,
        //                        Fare_Amount = Passenger.Value.Fare_Amount,
        //                        PassengerID = Passenger.Value.PassengerID,
        //                        PassengerName = Passenger.Value.PassengerName,
        //                        PassengerContactNumber = Passenger.Value.PassengerContactNumber
        //                    });
        //                    counter++;
        //                }
        //            }
        //            ConfirmationCount = counter.ToString();
        //            OnPropertyChanged(nameof(ConfirmationCount));
        //            OnPropertyChanged(nameof(PassengerConfirmation));
        //        }
        //    }
        //}
        private async void ReloadPassengerConfirmationList(string Travel_ID)
        {
            if (PassengerConfirmation != null)
            {
                PassengerConfirmation.Clear();
            }
            if (ConnectionStatus.ConnectedToInternet())
            {
                if (await ConnectionStatus.TestConnectivity())
                {
                    int counter = 0;
                    PassengerConfirmation = new ObservableCollection<PassengerRecords_Data>();
                    LoadPassengerList(Travel_ID);
                    Cloud_Database.response = await Cloud_Database.client.GetAsync($"ConductorConfirmation/{Travel_ID}/Passengers");
                    if (Cloud_Database.response.Body.ToString() != "null")
                    {
                        Dictionary<string, PassengerRecords_Data> getPassengers = Cloud_Database.response.ResultAs<Dictionary<string, PassengerRecords_Data>>();
                        foreach (var Passenger in getPassengers)
                        {
                            PassengerConfirmation.Add(new PassengerRecords_Data
                            {
                                Date_Of_Trip = Passenger.Value.Date_Of_Trip,
                                Boarded = Passenger.Value.Boarded,
                                Fare_Amount = Passenger.Value.Fare_Amount,
                                PassengerID = Passenger.Value.PassengerID,
                                PassengerName = Passenger.Value.PassengerName,
                                PassengerContactNumber = Passenger.Value.PassengerContactNumber
                            });
                            counter++;
                        }
                    }
                    ConfirmationCount = counter.ToString();
                    OnPropertyChanged(nameof(ConfirmationCount));
                    OnPropertyChanged(nameof(PassengerConfirmation));
                }
            }
        }
        public void generateID_Number()
        {
            string num = "1234567890";
            int len = num.Length;
            string otp = string.Empty;
            int otpDigit = 4;
            string finaldigit;

            int getindex;
            for (int i = 0; i < otpDigit; i++)
            {
                do
                {
                    getindex = new Random().Next(0, len);
                    finaldigit = num.ToCharArray()[getindex].ToString();
                } while (otp.IndexOf(finaldigit) != -1);

                otp += finaldigit;
            }
            ID_Number = otp;
        }
        private static string GenerateReferenceID(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
        private void generateID_Alpahbet()
        {
            string num = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int len = num.Length;
            string otp = string.Empty;
            int otpDigit = 3;
            string finaldigit;

            int getindex;
            for (int i = 0; i < otpDigit; i++)
            {
                do
                {
                    getindex = new Random().Next(0, len);
                    finaldigit = num.ToCharArray()[getindex].ToString();
                } while (otp.IndexOf(finaldigit) != -1);

                otp += finaldigit;
            }
            ID_Alphabet = otp;
        }
        private void GenerateID()
        {
            generateID_Number();
            generateID_Alpahbet();
            GeneratedID = $"{ID_Alphabet}{ID_Number}";
            OnPropertyChanged(nameof(GeneratedID));
        }
        private async void ScannedPayment(string bus_fare)
        {
            Cloud_Database.response = await Cloud_Database.client.GetAsync($"Users/{GeneratedID}");
            if (Cloud_Database.response.Body.ToString() != "null")
            {
                UserAccountProperties UserAccount = Cloud_Database.response.ResultAs<UserAccountProperties>();
                double BusFare = double.Parse(bus_fare);
                double NewAccountBalance = double.Parse(UserAccount.Account_Balance) - BusFare;
                var isProceed = await Application.Current.MainPage.DisplayAlert("QR Payment", $"Proceed payment for Account ID: {GeneratedID} with a fare of ₱ {BusFare:0,0.00}", "Proceed", "Cancel");
                if (isProceed)
                {
                    if (double.Parse(UserAccount.Account_Balance) < BusFare)
                    {
                        await Application.Current.MainPage.DisplayAlert("Payment Failed", "Insuficient Balance.", "OK");
                    }
                    else
                    {
                        Transactions_Data Transaction = new Transactions_Data
                        {
                            Transac_Date = DateTime.Now.ToString("dd, MMMM, yyyy hh:mm tt"),
                            Purpose = $"Board { BusNumber } ({ TravelID })",
                            Reference_ID = RefID,
                            Account_ID = GeneratedID,
                            Amount = $"- {BusFare:0,0.00}"
                        };
                        Cloud_Database.response = await Cloud_Database.client.SetAsync($"Users/{GeneratedID}/Account_Balance", NewAccountBalance.ToString("0,0.00"));
                        Cloud_Database.response = await Cloud_Database.client.SetAsync($"Transactions_Data/{GeneratedID}/{RefID}", Transaction);
                        await ProcessPayment(bus_fare, "Board");
                        RegisterPassengerHistory(bus_fare, GeneratedID, "To Board");
                        RegisterPassengerRecord();
                        await Application.Current.MainPage.DisplayAlert("", "QR Payment Success.", "OK");
                        isScannedPay = false;

                    }
                }
            }
        }
        private void ButtonCommands(INavigation navigation)
        {
            RefreshCommand = new Command(() =>
            {
                ReloadPassengerConfirmationList(TravelID);
                isRefreshing = false;
                OnPropertyChanged(nameof(isRefreshing));
            });
            NewPassengerCommand = new Command(() =>
            {
                if (NewPassenger_TEXT.Equals("NEW"))
                {
                    GenerateID();
                    NewPassenger_TEXT = "OLD";
                    isNewEntry = true;
                    OnPropertyChanged(nameof(NewPassenger_TEXT));
                }
                else
                {
                    NewPassenger_TEXT = "NEW";
                    isNewEntry = false;
                    OnPropertyChanged(nameof(NewPassenger_TEXT));
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
                                    Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{TravelID}/Passengers");
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
                                            PassengerStatus = $"Transfered from {BusNumber}({TravelID})",
                                            PassengerContactNumber = Passenger.Value.PassengerContactNumber
                                        };
                                        Cloud_Database.response = await Task.Run(() => Cloud_Database.client.SetAsync($"BusOnTrip/{travel_id}/Passengers/{Passenger.Value.PassengerID}", PassengerToTransfer));
                                        Cloud_Database.response = await Task.Run(() => Cloud_Database.client.PushAsync($"PassengerHistory/{DateTime.Now.ToString("MMddyyyy")}/{Passenger.Value.PassengerID}", PassengerToTransfer));
                                        Cloud_Database.response = await Task.Run(() => Cloud_Database.client.DeleteAsync($"BusOnTrip/{TravelID}/Passengers/{Passenger.Value.PassengerID}"));
                                    }
                                    LoadPassengerList(TravelID);
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
                                    Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{TravelID}/Passengers/{SelectedPassenger.PassengerID}");
                                    PassengerRecords_Data Passenger = Cloud_Database.response.ResultAs<PassengerRecords_Data>();
                                    PassengerRecords_Data PassengerToTransfer = new PassengerRecords_Data()
                                    {
                                        Date_Of_Trip = DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"),
                                        Boarded = $"Boarded {bus_number}({travel_id})",
                                        Fare_Amount = Passenger.Fare_Amount,
                                        PassengerID = Passenger.PassengerID,
                                        PassengerName = Passenger.PassengerName,
                                        PassengerStatus = $"Transfered from {BusNumber}({TravelID})",
                                        PassengerContactNumber = Passenger.PassengerContactNumber
                                    };
                                    Cloud_Database.response = await Task.Run(() => Cloud_Database.client.SetAsync($"BusOnTrip/{travel_id}/Passengers/{SelectedPassenger.PassengerID}", PassengerToTransfer));
                                    Cloud_Database.response = await Task.Run(() => Cloud_Database.client.PushAsync($"PassengerHistory/{DateTime.Now.ToString("MMddyyyy")}/{SelectedPassenger.PassengerID}", PassengerToTransfer));
                                    Cloud_Database.response = await Task.Run(() => Cloud_Database.client.DeleteAsync($"BusOnTrip/{TravelID}/Passengers/{SelectedPassenger.PassengerID}"));
                                    LoadPassengerList(TravelID);
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
            ConfirmPassengerCommand = new Command(async () =>
            {
                if (SelectedPassenger_Confirmation == null)
                {
                    return;
                }
                if (ConnectionStatus.ConnectedToInternet())
                {
                    if (await ConnectionStatus.TestConnectivity())
                    {
                        Cloud_Database.response = await Cloud_Database.client.GetAsync($"ConductorConfirmation/{TravelID}/Passengers/{SelectedPassenger_Confirmation.PassengerID}");
                        if (Cloud_Database.response.Body.ToString() != "null")
                        {
                            PassengerRecords_Data Passenger = Cloud_Database.response.ResultAs<PassengerRecords_Data>();
                            PassengerRecords_Data ConfirmPassenger = new PassengerRecords_Data()
                            {
                                Date_Of_Trip = DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"),
                                Boarded = $"Boarded {BusNumber}({TravelID})",
                                Fare_Amount = Passenger.Fare_Amount,
                                PassengerID = Passenger.PassengerID,
                                PassengerName = Passenger.PassengerName,
                                PassengerContactNumber = Passenger.PassengerContactNumber
                            };
                            Cloud_Database.response = await Cloud_Database.client.SetAsync($"BusOnTrip/{TravelID}/Passengers/{SelectedPassenger_Confirmation.PassengerID}", ConfirmPassenger);
                            Cloud_Database.response = await Cloud_Database.client.SetAsync($"BusPassengers_History/{TravelID}/Passengers/{SelectedPassenger_Confirmation.PassengerID}", ConfirmPassenger);
                            Cloud_Database.response = await Cloud_Database.client.DeleteAsync($"ConductorConfirmation/{TravelID}/Passengers/{SelectedPassenger_Confirmation.PassengerID}");
                            Cloud_Database.response = await Cloud_Database.client.PushAsync($"PassengerHistory/{DateTime.Now.ToString("MMddyyyy")}/{SelectedPassenger_Confirmation.PassengerID}", ConfirmPassenger);
                            _Count_Sitting++;
                            LoadPassengerList(TravelID);
                            ReloadPassengerConfirmationList(TravelID);
                            DisplayMessage("Passenger Confirmed.");
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
                SelectedPassenger_Confirmation = null;
                OnPropertyChanged(nameof(SelectedPassenger_Confirmation));
            });
            CancelPassengerCommand = new Command(async () =>
            {
               if (SelectedPassenger_Confirmation == null)
               {
                   return;
               }
               if (ConnectionStatus.ConnectedToInternet())
               {
                   if (await ConnectionStatus.TestConnectivity())
                   {
                       Cloud_Database.response = await Cloud_Database.client.GetAsync($"ConductorConfirmation/{TravelID}/Passengers/{SelectedPassenger_Confirmation.PassengerID}");
                       if (Cloud_Database.response.Body.ToString() != "null")
                       {
                           PassengerRecords_Data Passenger = Cloud_Database.response.ResultAs<PassengerRecords_Data>();
                           PassengerRecords_Data ConfirmPassenger = new PassengerRecords_Data()
                           {
                               Date_Of_Trip = DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"),
                               Boarded = $"Board canceled {BusNumber}({TravelID})",
                               Fare_Amount = Passenger.Fare_Amount,
                               PassengerID = Passenger.PassengerID,
                               PassengerName = Passenger.PassengerName,
                               PassengerContactNumber = Passenger.PassengerContactNumber
                           };
                           Cloud_Database.response = await Cloud_Database.client.SetAsync($"Canceled_Boarding/{SelectedPassenger_Confirmation.PassengerID}", ConfirmPassenger);
                           Cloud_Database.response = await Cloud_Database.client.DeleteAsync($"ConductorConfirmation/{TravelID}/Passengers/{SelectedPassenger_Confirmation.PassengerID}");
                           Cloud_Database.response = await Cloud_Database.client.PushAsync($"PassengerHistory/{DateTime.Now.ToString("MMddyyyy")}/{SelectedPassenger_Confirmation.PassengerID}", ConfirmPassenger);
                           _Count_Sitting++;
                           LoadPassengerList(TravelID);
                           ReloadPassengerConfirmationList(TravelID);
                           DisplayMessage("Passenger Confirmed.");
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
               SelectedPassenger_Confirmation = null;
               OnPropertyChanged(nameof(SelectedPassenger_Confirmation));
           });
            RegisterPayment = new Command(async () =>
            {
                if (ConnectionStatus.ConnectedToInternet())
                {
                    if (await ConnectionStatus.TestConnectivity())
                    {
                        RefID = GenerateReferenceID(12);
                        if (isFreeOfCharge)
                        {
                            bool isContinue = await Application.Current.MainPage.DisplayAlert("Free of Charge", "Boarding as Free of Charge. Continue?", "YES", "NO");
                            if (isContinue)
                            {
                                await ProcessPayment("0", "Board");
                                RegisterPassengerHistory("0", GeneratedID, "Boarded");
                                RegisterPassengerRecord();
                                LoadPassengerList(TravelID);
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(PassengerName) || !string.IsNullOrWhiteSpace(PassengerName) || !string.IsNullOrEmpty(PassengerContactNum) || !string.IsNullOrWhiteSpace(PassengerContactNum))
                            {
                                if (string.IsNullOrEmpty(_AmountPaid) || string.IsNullOrWhiteSpace(_AmountPaid) || _AmountPaid.Equals("0") || double.Parse(_AmountPaid) < double.Parse(_Bus_Fare))
                                {
                                    DisplayMessage("Invalid Amount Paid");
                                }
                                else
                                {
                                    if (isScannedPay)
                                    {
                                        if (isSeniorDiscount_Checked)
                                        {
                                            bool isContinue = await Application.Current.MainPage.DisplayAlert("Senior Discount", "Are you giving discount to a senior citizen?", "YES", "NO");
                                            if (isContinue)
                                            {
                                                string Discount = double.Parse($".{SeniorDiscount}").ToString(".00");
                                                double Discount_Price = double.Parse(_Bus_Fare) * double.Parse(Discount);
                                                double Discounted_Fare = double.Parse(_Bus_Fare) - Discount_Price;
                                                ScannedPayment(Discounted_Fare.ToString());
        
                                            }
                                        }
                                        else if (isNormalPay)
                                        {
                                            ScannedPayment(_Bus_Fare);
                                        }
                                    }
                                    else
                                    {
                                        if (isSeniorDiscount_Checked)
                                        {
                                            bool isContinue = await Application.Current.MainPage.DisplayAlert("Senior Discount", "Are you giving discount to a senior citizen?", "YES", "NO");
                                            if (isContinue)
                                            {
                                                string Discount = double.Parse($".{SeniorDiscount}").ToString(".00");
                                                double Discount_Price = double.Parse(_Bus_Fare) * double.Parse(Discount);
                                                double Discounted_Fare = double.Parse(_Bus_Fare) - Discount_Price;
                                                await ProcessPayment(Discounted_Fare.ToString(), "Board");
                                                RegisterPassengerHistory(Discounted_Fare.ToString(), GeneratedID, "To Board");
                                                RegisterPassengerRecord();
                                            }
                                        }
                                        else if (isNormalPay)
                                        {
                                            await ProcessPayment (_Bus_Fare, "Board");
                                            RegisterPassengerHistory(_Bus_Fare, GeneratedID, "To Board");
                                            RegisterPassengerRecord();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                DisplayMessage("Passenger Name and Contact Number is Required.");
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
                
            });
            RefundPayment = new Command(async () =>
            {
                if (SelectedPassenger == null)
                {
                    return;
                }
                if (ConnectionStatus.ConnectedToInternet())
                {
                    if (await ConnectionStatus.TestConnectivity())
                    {                        
                        Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{TravelID}");
                        if (Cloud_Database.response.Body.ToString() != "null")
                        {
                            Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{TravelID}/Passengers/{SelectedPassenger.PassengerID}");
                            PassengerRecords_Data Passenger = Cloud_Database.response.ResultAs<PassengerRecords_Data>();
                            PassengerRecords_Data PassengerToRefund = new PassengerRecords_Data()
                            {
                                Date_Of_Trip = DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"),
                                Boarded = $"Unboarded {BusNumber}({TravelID})",
                                Fare_Amount = $"-{Passenger.Fare_Amount}",
                                PassengerID = Passenger.PassengerID,
                                PassengerName = Passenger.PassengerName,
                                PassengerContactNumber = Passenger.PassengerContactNumber
                            };
                            bool isRefund = await Application.Current.MainPage.DisplayAlert("Refund", $"Continue refund for {Passenger.PassengerID}: {Passenger.PassengerName}", "Yes", "No");
                            if (isRefund)
                            {
                                Cloud_Database.response = await Task.Run(() => Cloud_Database.client.PushAsync($"PassengerHistory/{DateTime.Now.ToString("MMddyyyy")}/{SelectedPassenger.PassengerID}", PassengerToRefund));
                                Cloud_Database.response = await Task.Run(() => Cloud_Database.client.DeleteAsync($"BusOnTrip/{TravelID}/Passengers/{SelectedPassenger.PassengerID}"));
                                await ProcessPayment(Passenger.Fare_Amount, "Refund");
                                LoadPassengerList(TravelID);
                                DisplayMessage($"Passenger {SelectedPassenger.PassengerID} Refunded: -{Passenger.Fare_Amount}");
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
            });
            UnboardPassenger = new Command(async () => 
            {
                if (SelectedPassenger == null)
                {
                    return;
                }
                if (ConnectionStatus.ConnectedToInternet())
                {
                    if (await ConnectionStatus.TestConnectivity())
                    {
                        Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{TravelID}");
                        if (Cloud_Database.response.Body.ToString() != "null")
                        {
                            Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{TravelID}/Passengers/{SelectedPassenger.PassengerID}");
                            PassengerRecords_Data Passenger = Cloud_Database.response.ResultAs<PassengerRecords_Data>();
                            PassengerRecords_Data PassengerToUnboard = new PassengerRecords_Data()
                            {
                                Date_Of_Trip = DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"),
                                Boarded = $"Unboarded {BusNumber}({TravelID})",
                                Fare_Amount = $"{Passenger.Fare_Amount}",
                                PassengerID = Passenger.PassengerID,
                                PassengerName = Passenger.PassengerName,
                                PassengerContactNumber = Passenger.PassengerContactNumber
                            };
                            bool isUnboard = await Application.Current.MainPage.DisplayAlert("Unboard Passenger", $"Continue unboard passenger {Passenger.PassengerID}: {Passenger.PassengerName}", "Yes", "No");
                            if (isUnboard)
                            {
                                Cloud_Database.response = await Task.Run(() => Cloud_Database.client.PushAsync($"PassengerHistory/{DateTime.Now.ToString("MMddyyyy")}/{SelectedPassenger.PassengerID}", PassengerToUnboard));
                                Cloud_Database.response = await Task.Run(() => Cloud_Database.client.DeleteAsync($"BusOnTrip/{TravelID}/Passengers/{SelectedPassenger.PassengerID}"));
                                await ProcessPayment(Passenger.Fare_Amount, "Unboard");
                                LoadPassengerList(TravelID);
                                DisplayMessage($"Passenger {SelectedPassenger.PassengerID} Unboarded");
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
            });
            OpenChangePasswordPageCommand = new Command(async () => 
            {
                Navigation = navigation;
                await Navigation.PushAsync(new ChangePasswordPage(Employee_Number));
            });
            ScanQRCommand = new Command(async () =>
            {
                ZXingScannerPage ScannerPage = new ZXingScannerPage();
                Navigation = navigation;
                ScannerPage.OnScanResult += (result) =>
                {
                    ScannerPage.IsScanning = false;
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        // DO SOMETHING WITH SCAN RESULT
                        await Navigation.PopModalAsync();
                        Cloud_Database.response = await Cloud_Database.client.GetAsync($"Users/{result.ToString()}");
                        if (Cloud_Database.response.Body.ToString() != "null")
                        {
                            UserAccountProperties PassengerInfo = Cloud_Database.response.ResultAs<UserAccountProperties>();
                            GeneratedID = result.ToString();
                            PassengerName = $"{PassengerInfo.FirstName} {PassengerInfo.LastName}";
                            //PassengerContactNum = PassengerInfo.PassengerContactNumber;
                            OnPropertyChanged(nameof(GeneratedID));
                            OnPropertyChanged(nameof(PassengerName));
                            //OnPropertyChanged(nameof(PassengerContactNum));
                            isScannedPay = true;
                        }
                    });
                };
                await Navigation.PushModalAsync(ScannerPage);
            });
            OpenTransactions = new Command(async () => 
            {
                Navigation = navigation;
                await Navigation.PushModalAsync(new BookerTransactionsPage(Employee_Number));
            });
        }
        private async void RegisterPassengerHistory(string bus_fare, string PassID, string status)
        {
            PassengerRecords_Data PassengerHistory = new PassengerRecords_Data()
            {
                PassengerID = PassID,
                PassengerName = PassengerName,
                PassengerContactNumber = PassengerContactNum,
                Boarded = $"Boarded {BusNumber}({TravelID})",
                PassengerStatus = status,
                Fare_Amount = bus_fare,
                Date_Of_Trip = DateTime.Now.ToString("MM/dd/yyyy h:mm tt")
            };
            Cloud_Database.response = await Cloud_Database.client.PushAsync($"PassengerHistory/{DateTime.Now.ToString("MMddyyyy")}/{GeneratedID}", PassengerHistory);
        }
        private async void RegisterPassengerRecord()
        {
            PassengerRecords_Data PassengerHistory = new PassengerRecords_Data()
            {
                PassengerID = GeneratedID,
                PassengerName = PassengerName,
                PassengerContactNumber = PassengerContactNum,
            };
            Cloud_Database.response = await Cloud_Database.client.SetAsync($"PassengerRecords/{GeneratedID}", PassengerHistory);
            GenerateID();
        }
        private async void LoadProfileData(string DB_Path, INavigation navigation)
        {
            try
            {
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
                            // Bus Profile Properties
                            Bus_Seats = Bus.bus_seats;
                            Bus_Way_Point = Bus.bus_route;
                            // Bus Trip Records Fields
                            _Bus_Number = Bus.bus_number;
                            _Way_Point = Bus.bus_route;
                            Bus_SeatsLimit = int.Parse(Bus.bus_seats);
                            LoadSeniorDiscount();
                            LoadPassengerFareList(BusNumber);
                        }
                    }
                }
                OnPropertyChanged(nameof(Employee_Number));
                OnPropertyChanged(nameof(FullName));
                OnPropertyChanged(nameof(Assigned_To));
                OnPropertyChanged(nameof(Bus_Seats));
                OnPropertyChanged(nameof(Bus_Way_Point));
            }
            catch
            {
                DisplayMessage("A internet connection problam occured. Please login again.");
                Navigation = navigation;
                await Navigation.PopToRootAsync();
            }
        }
        private async void DataProfileListener(string path, INavigation navigation)
        {
            if (ConnectionStatus.ConnectedToInternet())
            {
                if (await ConnectionStatus.TestConnectivity())
                {
                    try
                    {
                        EventStreamResponse response = await Cloud_Database.client.OnAsync(path,
                        added: (s, args, context) => { LoadProfileData(path, navigation); },
                        changed: (s, args, context) => { LoadProfileData(path, navigation); },
                        removed: (s, args, context) => { LoadProfileData(path, navigation); });
                    }
                    catch
                    {
                        DisplayMessage("A internet connection problam occured. Please login again.");
                        Navigation = navigation;
                        await Navigation.PopToRootAsync();
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
        private async void LoadBusCapacity(string Travel_ID, INavigation navigation)
        {
            try
            {
                string dateID = dateNow.Replace("/", string.Empty);
                Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{Travel_ID}");
                if (Cloud_Database.response.Body.ToString() != "null")
                {
                    BusTripRecords_Data Capacity = Cloud_Database.response.ResultAs<BusTripRecords_Data>();
                    Count_Sitting = Capacity.Bus_Sitting;
                    _Count_Sitting = int.Parse(Capacity.Bus_Sitting);
                    Count_Standing = Capacity.Bus_Standing;
                    _Count_Standing = int.Parse(Capacity.Bus_Standing);
                    int Total_Passengers = int.Parse(Count_Sitting) + int.Parse(Count_Standing);
                    Count_Total_Passenger = Total_Passengers.ToString();
                    BusNumber = Capacity.Bus_Number;
                    BusWayPoint = Capacity.Bus_Route;
                    TravelID = Capacity.Travel_ID;
                }
                OnPropertyChanged(nameof(Count_Sitting));
                OnPropertyChanged(nameof(Count_Standing));
                OnPropertyChanged(nameof(Count_Total_Passenger));
                OnPropertyChanged(nameof(BusNumber));
                OnPropertyChanged(nameof(BusWayPoint));
                OnPropertyChanged(nameof(TravelID));
            }
            catch
            {
                Navigation = navigation;
                DisplayMessage("A internet connection problam occured. Please login again.");
                await Navigation.PopToRootAsync();
            }
        }
        private async void BusCapacityDataListener(string Travel_ID, INavigation navigation)
        {
            if (ConnectionStatus.ConnectedToInternet())
            {
                if (await ConnectionStatus.TestConnectivity())
                {
                    try
                    {
                        string dateID = dateNow.Replace("/", string.Empty);
                        EventStreamResponse response = await Cloud_Database.client.OnAsync($"BusOnTrip/{Travel_ID}",
                        added: (s, args, context) => { LoadBusCapacity(Travel_ID, navigation); },
                        changed: (s, args, context) => { LoadBusCapacity(Travel_ID, navigation); },
                        removed: (s, args, context) => { LoadBusCapacity(Travel_ID, navigation); });
                    }
                    catch
                    {
                        Navigation = navigation;
                        DisplayMessage("A internet connection problam occured. Please login again.");
                        await Navigation.PopToRootAsync();
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
        private async void LoadSeniorDiscount()
        {
            if (ConnectionStatus.ConnectedToInternet())
            {
                if (await ConnectionStatus.TestConnectivity())
                {
                    Cloud_Database.response = await Cloud_Database.client.GetAsync("System_Settings/Senior_Discount");
                    if (Cloud_Database.response.Body.ToString() != "null")
                    {
                        SeniorDiscount_Data seniorDiscount = Cloud_Database.response.ResultAs<SeniorDiscount_Data>();
                        SeniorDiscount = seniorDiscount.Senior_Discount;
                        OnPropertyChanged(nameof(SeniorDiscount));
                    }
                }
            }
        }
        private async Task TextChanged(string p)
        {
            try
            {
                double ChangeAmount = double.Parse(_AmountPaid) - double.Parse(_Bus_Fare);
                AmountChange = ChangeAmount.ToString();
                OnPropertyChanged(nameof(AmountChange));
            }
            catch { AmountChange = string.Empty; OnPropertyChanged(nameof(AmountChange)); }
        }
        private void TextChangeCommand()
        {
            TextChangedCommand = new Command<string>(async (_AmountPaid) => await TextChanged(_AmountPaid));
        }
        private async Task ProcessPayment(string Fare, string mode)
        {
            Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{TravelID}");
            if (Cloud_Database.response.Body.ToString() != "null")
            {
                BusTripRecords_Data Bus_Capacity = Cloud_Database.response.ResultAs<BusTripRecords_Data>();
                if (Bus_SeatsLimit != int.Parse(Bus_Capacity.Bus_Sitting))
                {
                    if (mode.Equals("Board"))
                    {
                        _Count_Sitting++;
                        await UpdateCapacity("Sit");
                    }
                    else if (mode.Equals("Refund"))
                    {
                        _Count_Sitting--;
                        await UpdateCapacity("Sit");
                    }
                    else if (mode.Equals("Unboard"))
                    {
                        _Count_Sitting--;
                        await UpdateCapacity("Sit");
                    }
                }
                else
                {
                    if (mode.Equals("Board"))
                    {
                        _Count_Standing++;
                        await UpdateCapacity("Stand");

                    }
                    else if (mode.Equals("Refund"))
                    {
                        if (Bus_SeatsLimit == int.Parse(Bus_Capacity.Bus_Sitting) && Bus_Capacity.Bus_Standing.Equals("0"))
                        {
                            _Count_Sitting--;
                            await UpdateCapacity("Sit");
                        }
                        else
                        {
                            _Count_Standing--;
                            await UpdateCapacity("Stand");
                        }
                    }
                    else if (mode.Equals("Unboard"))
                    {
                        if (Bus_SeatsLimit == int.Parse(Bus_Capacity.Bus_Sitting) && Bus_Capacity.Bus_Standing.Equals("0"))
                        {
                            _Count_Sitting--;
                            await UpdateCapacity("Sit");
                        }
                        else
                        {
                            _Count_Standing--;
                            await UpdateCapacity("Stand");
                        }
                    }
                }
                await UpdatePassengerRecords(Fare, mode);
                await UpdateTotalProfit(Fare, mode);
                LoadPassengerList(TravelID);
                ReloadPassengerConfirmationList(TravelID);
            }
            else
            {
                DisplayMessage("Bus Trip for today is not available.");
            }
        }
        private async Task UpdatePassengerRecords(string Fare, string mode)
        {
            if (mode.Equals("Board"))
            {
                PassengerRecords_Data Passengers = new PassengerRecords_Data
                {
                    PassengerID = GeneratedID,
                    Date_Of_Trip = DateTime.Now.ToString("MM/dd/yyyy h:mm tt"),
                    PassengerName = PassengerName,
                    PassengerContactNumber = PassengerContactNum,
                    Boarded = $"Boarded {BusNumber} ({TravelID})",
                    Fare_Amount = Fare,
                };
                Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{TravelID}");
                if (Cloud_Database.response.Body.ToString() != "null")
                {
                    Cloud_Database.response = await Cloud_Database.client.SetAsync($"ConductorConfirmation/{TravelID}/Passengers/{GeneratedID}", Passengers);
                    Transactions_Data transactions_Data = new Transactions_Data
                    {
                        Account_ID = GeneratedID,
                        Transac_Date = DateTime.Now.ToString("dd, MMMM, yyyy hh:mm tt"),
                        Reference_ID = RefID,
                        Amount = Fare,
                        Purpose = $"Board {BusNumber} ({TravelID})"
                    };
                    Cloud_Database.response = await Cloud_Database.client.SetAsync($"Transactions_Data/{Employee_Number}/{RefID}", transactions_Data);
                    DisplayMessage("Passenger Boarded.");
                }
            }
            //else if (mode.Equals("Refund"))
            //{
            //    for (int Number_OF_Passengers = 1; Number_OF_Passengers <= num_passenger; Number_OF_Passengers++)
            //    {
            //        PassengerRecords_Data BusTripRecords_PaymentsHistory = new PassengerRecords_Data
            //        {
            //            Date_Of_Trip = dateNow,
            //            PassengerName = PassengerName,
            //            PassengerContactNumber = PassengerContactNum,
            //            Fare_Amount = Fare,
            //        };
            //        Cloud_Database.response = await Cloud_Database.client.PushAsync(Payments_Path, BusTripRecords_PaymentsHistory);
            //    }
            //    DisplayMessage($"You have refunded {num_passenger} passenger.");
            //}
        }
        private async Task UpdateTotalProfit(string Fare, string mode)
        {
            string Total_Profit_Path = $"BusOnTrip/{TravelID}/Total_Profit";
            Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{TravelID}");
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
            PassengerName = string.Empty;
            PassengerContactNum = string.Empty;
            AmountPaid = string.Empty;
            AmountChange = string.Empty;
            OnPropertyChanged(nameof(PassengerName));
            OnPropertyChanged(nameof(PassengerContactNum));
            OnPropertyChanged(nameof(AmountPaid));
            OnPropertyChanged(nameof(AmountChange));
        }
        private async Task UpdateCapacity(string pos)
        {
            if (pos.Equals("Sit"))
            {
                Cloud_Database.response = await Cloud_Database.client.SetAsync($"BusOnTrip/{TravelID}/Bus_Sitting", _Count_Sitting.ToString());
            }
            else if (pos.Equals("Stand"))
            {
                Cloud_Database.response = await Cloud_Database.client.SetAsync($"BusOnTrip/{TravelID}/Bus_Standing", _Count_Standing.ToString());
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