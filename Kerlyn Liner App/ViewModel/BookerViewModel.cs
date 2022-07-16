using FireSharp.Response;
using Kerlyn_Liner_App.DataProperties;
using Kerlyn_Liner_App.DB;
using Kerlyn_Liner_App.Pages.BookerPage;
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
    class BookerViewModel : INotifyPropertyChanged
    {
        Database Cloud_Database = new Database();
        public ObservableCollection<Bus_Data> BusData { get; set; }
        public ObservableCollection<BusTripHistory_Data> BusTripHistory { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        InternetConnectionChecker ConnectionStatus = new InternetConnectionChecker();
        public ObservableCollection<PassengerRecords_Data> PassengerHistory { get; set; }
        public Command OpenCommand_AuthorizedTripPage { get; set; }
        public INavigation Navigation { get; set; }
        public string Employee_Number { get; set; }
        public string FullName { get; set; }
        public string Assigned_To { get; set; }
        public Command LoadAccountCommand { get; set; }
        public string AccountID { get; set; }
        public string LoadAmount { get; set; }
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
        public Command ScanQRCommand { get; set; }
        public Command AuthorizeBusTripCommand { get; set; }
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
        public string Bus_Fare
        {
            get { return _Bus_Fare; }
            set
            {
                _Bus_Fare = value;
            }
        }
        public string PassengerName { get => passengerName; set => passengerName = value; }
        public string PassengerContactNum { get => passengerContactNum; set => passengerContactNum = value; }
        private Bus_Data _BusNumber;
        public Bus_Data SelectedBus
        {
            get { return _BusNumber; }
            set
            {
                _BusNumber = value;
                OnPropertyChanged(nameof(SelectedBus));
                BusCapacityDataListener(SelectedBus.bus_number);
                LoadPassengerFareList(SelectedBus.bus_number);
            }
        }
        private Bus_Data _AuthorizeBus;
        public Bus_Data AuthorizeBus
        {
            get { return _AuthorizeBus; }
            set 
            { 
                _AuthorizeBus = value; OnPropertyChanged(nameof(AuthorizeBus));
                LoadEmployeePicker();
                
                try
                {
                    DriverOneID = AuthorizeBus.bus_driver_id_1;
                    DriverOneName = AuthorizeBus.bus_driver_name_1;
                    DriverTwoID = AuthorizeBus.bus_driver_id_2;
                    DriverTwoName = AuthorizeBus.bus_driver_name_2;
                    ConductorID = AuthorizeBus.bus_conductor_id;
                    ConductorName = AuthorizeBus.bus_conductor_name;
                }
                catch
                { }
            }
        }
        private Bus_Data _SearchBusHistory;
        public Bus_Data SearchBusHistory
        {
            get { return _SearchBusHistory; }
            set { _SearchBusHistory = value; OnPropertyChanged(nameof(SearchBusHistory)); LoadBusTravelHistory(SearchBusHistory.bus_number); }
        }
        private DateTime _SelectedDate = DateTime.Now;
        public DateTime SelectedDate
        {
            get { return _SelectedDate; }
            set { _SelectedDate = value; OnPropertyChanged(nameof(SelectedDate));}
        }
        public string SearchID { get; set; }
        public Command SearchID_COMMAND { get; set; }
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
        private string ID_Number, ID_Alphabet;
        private string TravelID_Alphabet, TravelID_Number_1, TravelID_Number_2, GeneratedTravel_ID;
        public string GeneratedID { get; set; }
        public string BusTripRound { get; set; } = "1";
        public Command ChangeDriverOneCommand { get; set; }
        public Command ChangeDriverTwoCommand { get; set; }
        public Command ChangeConducterCommand { get; set; }
        public ObservableCollection<Account_Data> BusDriverEmployee { get; set; }
        public ObservableCollection<Account_Data> BusConductorEmployee { get; set; }
        public string ChangeDriverOne_Text { get; set; } = "CHANGE";
        public string ChangeDriverTwo_Text { get; set; } = "CHANGE";
        public string ChangeConductor_Text { get; set; } = "CHANGE";

        private bool _ChangeDriverOne;
        public bool ChangeDriverOne
        {
            get => _ChangeDriverOne;
            set
            {
                _ChangeDriverOne = value;
                OnPropertyChanged(nameof(ChangeDriverOne));
            }
        }
        private bool _ChangeDriverTwo;
        public bool ChangeDriverTwo
        {
            get => _ChangeDriverTwo;
            set
            {
                _ChangeDriverTwo = value;
                OnPropertyChanged(nameof(ChangeDriverTwo));
            }
        }
        private bool _ChangeConductor;
        public bool ChangeConductor
        {
            get => _ChangeConductor;
            set
            {
                _ChangeConductor = value;
                OnPropertyChanged(nameof(ChangeConductor));
            }
        }
        private Account_Data _SelectedDriverOneID;
        public Account_Data SelectedDriverOneID
        {
            get { return _SelectedDriverOneID; }
            set 
            { 
                _SelectedDriverOneID = value;
                OnPropertyChanged(nameof(SelectedDriverOneID));
                DriverOneID = SelectedDriverOneID.id;
                if (DriverOneID.Equals(DriverTwoID))
                {
                    DisplayMessage("",$"{DriverOneID} is already assigned to Driver 2.", "OK");
                }
                else
                {
                    DriverOneID = SelectedDriverOneID.id;
                    DriverOneName = $"{SelectedDriverOneID.firstname} {SelectedDriverOneID.surname}";
                }
                
            }
        }
        private Account_Data _SelectedDriverTwoID;
        public Account_Data SelectedDriverTwoID
        {
            get { return _SelectedDriverTwoID; }
            set
            {
                _SelectedDriverTwoID = value;
                OnPropertyChanged(nameof(SelectedDriverTwoID));
                DriverTwoID = SelectedDriverTwoID.id;
                if (DriverTwoID.Equals(DriverOneID))
                {
                    DisplayMessage("", $"{DriverOneID} is already assigned to Driver 1.", "OK");
                }
                else
                {
                    DriverTwoID = SelectedDriverTwoID.id;
                    DriverTwoName = $"{SelectedDriverTwoID.firstname} {SelectedDriverTwoID.surname}";
                }
                
            }
        }
        private Account_Data _SelectedConductorID;
        public Account_Data SelectedConductorID
        {
            get { return _SelectedConductorID; }
            set
            {
                _SelectedConductorID = value;
                OnPropertyChanged(nameof(SelectedConductorID));
                ConductorID = SelectedConductorID.id;
                try
                {
                    ConductorID = SelectedConductorID.id;
                    ConductorName = $"{SelectedConductorID.firstname} {SelectedConductorID.surname}";
                }
                catch { }
                
            }
        }
        private string _DriverOneID;
        public string DriverOneID
        {
            get => _DriverOneID;
            set
            {
                _DriverOneID = value; OnPropertyChanged(nameof(DriverOneID));
            }
        }
        private string _DriverOneName;
        public string DriverOneName 
        { 
            get => _DriverOneName;
            set 
            {
                _DriverOneName = value; OnPropertyChanged(nameof(DriverOneName));
            } 
        }
        private string _DriverTwoID;
        public string DriverTwoID
        {
            get => _DriverTwoID;
            set
            {
                _DriverTwoID = value; OnPropertyChanged(nameof(DriverTwoID));
            }
        }
        private string _DriverTwoName;
        public string DriverTwoName
        {
            get => _DriverTwoName;
            set
            {
                _DriverTwoName = value; OnPropertyChanged(nameof(DriverTwoName));
            }
        }

        private string _ConductorID;
        public string ConductorID
        {
            get => _ConductorID;
            set
            {
                _ConductorID = value; OnPropertyChanged(nameof(ConductorID));
            }
        }
        private string _ConductorName;
        public string ConductorName
        {
            get => _ConductorName;
            set
            {
                _ConductorName = value; OnPropertyChanged(nameof(ConductorName));
            }
        }
        public Command OpenPassengerListCommand { get; set; }
        public Command BusArrivedCommand { get; set; }
        public ObservableCollection<BusTripRecords_Data> BussesOnTrip { get; set; }
        private BusTripRecords_Data _SelectedBusOnTrip;
        public BusTripRecords_Data SelectedBusOnTrip
        {
            get { return _SelectedBusOnTrip; }
            set
            {
                _SelectedBusOnTrip = value;
                OnPropertyChanged(nameof(SelectedBusOnTrip));
            }
        }
        public ObservableCollection<PassengerRecords_Data> OldPassengersList { get; set; }
        private PassengerRecords_Data _SelectedPassenger;
        public PassengerRecords_Data SelectedPassenger
        {
            get { return _SelectedPassenger; }
            set
            {
                _SelectedPassenger = value;
                OnPropertyChanged(nameof(SelectedPassenger));
                GeneratedID = SelectedPassenger.PassengerID;
                PassengerName = SelectedPassenger.PassengerName;
                PassengerContactNum = SelectedPassenger.PassengerContactNumber;
                OnPropertyChanged(nameof(GeneratedID));
                OnPropertyChanged(nameof(PassengerName));
                OnPropertyChanged(nameof(PassengerContactNum));
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
        private bool BusOnTravel = false;
        public string BusTravel_ID { get; set; }
        private bool BusNumberFound = false;
        public Command OpenTransactionsPageCommand { get; set; }
        private bool isScannedPay { get; set; } = false;
        private static Random random = new Random();
        private string RefID = string.Empty;
        public BookerViewModel(string user, INavigation navigation)
        {
            Cloud_Database.Cloud_DB_Connect();
            LoadUserData(user);
            LoadBusPicker();
            LoadBussesOnTrip();
            LoadSeniorDiscount();
            TextChangeCommand();
            GenerateID();
            LoadOldPassengerPicker();
            ButtonCommands(navigation);
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
        private async void LoadBussesOnTrip()
        {
            if (BussesOnTrip != null)
            {
                BussesOnTrip.Clear();
            }
            Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip");
            if (Cloud_Database.response.Body.ToString() != "null")
            {
                BussesOnTrip = new ObservableCollection<BusTripRecords_Data>();
                Dictionary<string, BusTripRecords_Data> getBusses = Cloud_Database.response.ResultAs<Dictionary<string, BusTripRecords_Data>>();
                foreach (var Busses in getBusses)
                {
                    BussesOnTrip.Add(new BusTripRecords_Data
                    {
                        Bus_Number = Busses.Value.Bus_Number,
                        Bus_Route = Busses.Value.Bus_Route,
                        Travel_ID = Busses.Value.Travel_ID
                    });
                }
                OnPropertyChanged(nameof(BussesOnTrip));
            }
        }
        
        private async void LoadBusTravelHistory(string BusNumber)
        {
            Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusTripHistory/{BusNumber}");
            if (Cloud_Database.response.Body.ToString() != "null")
            {
                BusTripHistory = new ObservableCollection<BusTripHistory_Data>();
                Dictionary<string, BusTripHistory_Data> getData = Cloud_Database.response.ResultAs<Dictionary<string, BusTripHistory_Data>>();
                foreach (var BusRecord in getData)
                {
                    BusTripHistory.Add(new BusTripHistory_Data
                    {
                        Date = BusRecord.Value.Date,
                        BusNumber = BusRecord.Value.BusNumber,
                        BusRoute = BusRecord.Value.BusRoute,
                        BusDriverID_1 = BusRecord.Value.BusDriverID_1,
                        BusDriverName_1 = BusRecord.Value.BusDriverName_1,
                        BusDriverID_2 = BusRecord.Value.BusDriverID_2,
                        BusDriverName_2 = BusRecord.Value.BusDriverName_2,
                        BusConductorID = BusRecord.Value.BusConductorID,
                        BusConductorName = BusRecord.Value.BusConductorName,
                    });
                }
                OnPropertyChanged(nameof(BusTripHistory));
            }
            else
            {
                DisplayMessage("Null","Null","OK");
            }
        }
        private async void LoadPassengerHistory()
        {
            Cloud_Database.response = await Cloud_Database.client.GetAsync($"PassengerHistory/{SelectedDate.ToString("MMddyyyy")}/{SearchID}");
            if (Cloud_Database.response.Body.ToString() != "null")
            {
                PassengerHistory = new ObservableCollection<PassengerRecords_Data>();
                Dictionary<string, PassengerRecords_Data> getPassengerHistory_Data = Cloud_Database.response.ResultAs<Dictionary<string, PassengerRecords_Data>>();
                foreach (var get in getPassengerHistory_Data)
                {
                    PassengerHistory.Add(new PassengerRecords_Data
                    {
                        PassengerName = get.Value.PassengerName,
                        PassengerContactNumber = get.Value.PassengerContactNumber,
                        Date_Of_Trip = get.Value.Date_Of_Trip,
                        Boarded = get.Value.Boarded
                    });
                }
                OnPropertyChanged(nameof(PassengerHistory));
            }
        }
        private async void LoadUserData(string user)
        {
            if (ConnectionStatus.ConnectedToInternet())
            {
                if (await ConnectionStatus.TestConnectivity())
                {
                    Cloud_Database.response = await Cloud_Database.client.GetAsync(user);
                    if (Cloud_Database.response.Body.ToString() != "null")
                    {
                        Account_Data account_Data = Cloud_Database.response.ResultAs<Account_Data>();
                        Employee_Number = account_Data.id;
                        FullName = $"{account_Data.firstname} {account_Data.mi} {account_Data.surname}";
                        Assigned_To = $"{account_Data.position} - {account_Data.assigned_to}";
                    }
                    OnPropertyChanged(nameof(Employee_Number));
                    OnPropertyChanged(nameof(FullName));
                    OnPropertyChanged(nameof(Assigned_To));
                }
                else
                {
                    DisplayMessage("Connection Error", "Please check your internet connection.", "OK");
                }
            }
        }
        public async void LoadBusCapacity(string bus_number)
        {
            Cloud_Database.response = await Cloud_Database.client.GetAsync("BusOnTrip");
            if (Cloud_Database.response.Body.ToString() != "null")
            {
                Dictionary<string, BusTripRecords_Data> getBusInfo = Cloud_Database.response.ResultAs<Dictionary<string, BusTripRecords_Data>>();
                foreach (var Bus in getBusInfo)
                {
                    if (Bus.Value.Bus_Number.Equals(bus_number))
                    {
                        BusTravel_ID = Bus.Value.Travel_ID;
                        OnPropertyChanged(nameof(BusTravel_ID));
                        BusNumberFound = true;
                        break;
                    }
                }
                if (BusNumberFound)
                {
                    Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{BusTravel_ID}");
                    if (Cloud_Database.response.Body.ToString() != "null")
                    {
                        BusTripRecords_Data BusDetails = Cloud_Database.response.ResultAs<BusTripRecords_Data>();
                        Bus_Seats = BusDetails.Bus_Capacity;
                        Bus_Way_Point = BusDetails.Bus_Route;
                        Count_Sitting = BusDetails.Bus_Sitting;
                        _Count_Sitting = int.Parse(BusDetails.Bus_Sitting);
                        Count_Standing = BusDetails.Bus_Standing;
                        _Count_Standing = int.Parse(BusDetails.Bus_Standing);
                        Bus_SeatsLimit = int.Parse(BusDetails.Bus_Capacity);
                        int Total_Passengers = int.Parse(Count_Sitting) + int.Parse(Count_Standing);
                        Count_Total_Passenger = Total_Passengers.ToString();
                        //LoadPaymentHistory();
                    }
                }
                else
                {
                    DisplayMessage("","Bus Number is not authorized for trip.","OK");
                }
            }
            OnPropertyChanged(nameof(Bus_Way_Point));
            OnPropertyChanged(nameof(Bus_Seats));
            OnPropertyChanged(nameof(Count_Sitting));
            OnPropertyChanged(nameof(Count_Standing));
            OnPropertyChanged(nameof(Count_Total_Passenger));
        }
        private async void BusCapacityDataListener(string bus_number)
        {
            if (ConnectionStatus.ConnectedToInternet())
            {
                if (await ConnectionStatus.TestConnectivity())
                {
                    Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{BusTravel_ID}");
                    if (Cloud_Database.response.Body.ToString() != "null")
                    {
                        EventStreamResponse response = await Cloud_Database.client.OnAsync($"BusOnTrip/{BusTravel_ID}",
                        added: (s, args, context) => { LoadBusCapacity(bus_number);},
                        changed: (s, args, context) => { LoadBusCapacity(bus_number);},
                        removed: (s, args, context) => { LoadBusCapacity(bus_number);});
                    }
                }
                else
                {
                    SelectedBus = null;
                    DisplayMessage("Connection Error","Please check your internet connection.","OK");
                }
            }
            else
            {
                SelectedBus = null;
                DisplayMessage("Connection Error","You are not connected to internet.","OK");
            }
        }
        private async void LoadEmployeePicker()
        {
            try
            {
                Cloud_Database.response = await Cloud_Database.client.GetAsync($"Account_Data/Driver");
                if (Cloud_Database.response.Body.ToString() != "null")
                {
                    BusDriverEmployee = new ObservableCollection<Account_Data>();
                    Dictionary<string, Account_Data> getData = await Task.Run(() => Cloud_Database.response.ResultAs<Dictionary<string, Account_Data>>());
                    foreach (var get in getData)
                    {
                        BusDriverEmployee.Add(new Account_Data()
                        {
                            id = get.Value.id,
                            firstname = get.Value.firstname,
                            surname = get.Value.surname
                        });
                    }
                    OnPropertyChanged(nameof(BusDriverEmployee));
                }
                Cloud_Database.response = await Cloud_Database.client.GetAsync($"Account_Data/Conductor");
                if (Cloud_Database.response.Body.ToString() != "null")
                {
                    BusConductorEmployee = new ObservableCollection<Account_Data>();
                    Dictionary<string, Account_Data> getData = await Task.Run(() => Cloud_Database.response.ResultAs<Dictionary<string, Account_Data>>());
                    foreach (var get in getData)
                    {
                        BusConductorEmployee.Add(new Account_Data()
                        {
                            id = get.Value.id,
                            firstname = get.Value.firstname,
                            surname = get.Value.surname
                        });
                    }
                    OnPropertyChanged(nameof(BusConductorEmployee));
                }
            }
            catch
            {
                
            }
        }
        private async void LoadBusPicker()
        {
            Cloud_Database.response = await Cloud_Database.client.GetAsync($"Bus_Data/Busses");
            if (Cloud_Database.response.Body.ToString() != "null")
            {
                BusData = new ObservableCollection<Bus_Data>();
                Dictionary<string, Bus_Data> getData = Cloud_Database.response.ResultAs<Dictionary<string, Bus_Data>>();
                foreach (var bus in getData)
                {
                    BusData.Add(new Bus_Data()
                    {
                        bus_number = bus.Value.bus_number,
                        bus_seats = bus.Value.bus_seats,
                        bus_route = bus.Value.bus_route,
                        bus_fare = bus.Value.bus_fare,
                        bus_driver_id_1 = bus.Value.bus_driver_id_1,
                        bus_driver_name_1 = bus.Value.bus_driver_name_1,
                        bus_driver_id_2 = bus.Value.bus_driver_id_2,
                        bus_driver_name_2 = bus.Value.bus_driver_name_2,
                        bus_conductor_id = bus.Value.bus_conductor_id,
                        bus_conductor_name = bus.Value.bus_conductor_name
                    });
                }
                OnPropertyChanged(nameof(BusData));
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
        public void generateID_Alpahbet()
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
        public void generateTravelID_Alpahbet()
        {
            string num = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int len = num.Length;
            string otp = string.Empty;
            int otpDigit = 2;
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
            TravelID_Alphabet = otp;
        }
        public void generateTravelID_Number_1()
        {
            string num = "0123456789";
            int len = num.Length;
            string otp = string.Empty;
            int otpDigit = 2;
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
            TravelID_Number_1 = otp;
        }
        public void generateTravelID_Number_2()
        {
            string num = "0123456789";
            int len = num.Length;
            string otp = string.Empty;
            int otpDigit = 2;
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
            TravelID_Number_2 = otp;
        }
        private void GenerateID()
        {
            generateID_Number();
            generateID_Alpahbet();
            GeneratedID = $"{ID_Alphabet}{ID_Number}";
            OnPropertyChanged(nameof(GeneratedID));
        }
        private static string GenerateReferenceID(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
        private void GenerateTravelID()
        {
            generateTravelID_Alpahbet();
            generateTravelID_Number_1();
            generateTravelID_Number_2();
            GeneratedTravel_ID = $"{TravelID_Alphabet}-{TravelID_Number_1}{TravelID_Number_2}";
        }
        private void ButtonCommands(INavigation navigation)
        {
            OpenTransactionsPageCommand = new Command(async () =>
            {
                Navigation = navigation;
                await Navigation.PushModalAsync(new BookerTransactionsPage(Employee_Number));
            });
            BusArrivedCommand = new Command(async () =>
            {
                string TravelID = await Application.Current.MainPage.DisplayPromptAsync("","Enter Bus Travel ID:");
                if (!string.IsNullOrEmpty(TravelID) || !string.IsNullOrWhiteSpace(TravelID))
                {
                    Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{TravelID}");
                    if (Cloud_Database.response.Body.ToString() != "null")
                    {
                        BusTripRecords_Data BusRecord = Cloud_Database.response.ResultAs<BusTripRecords_Data>();
                        DateTime startTime = DateTime.Parse(BusRecord.Bus_TravelStartTime);
                        DateTime endTime = DateTime.Parse(DateTime.Now.ToString("hh:mm tt"));
                        TimeSpan difference = startTime - endTime;
                        //DisplayMessage("Total Time", difference.Hours.ToString().Replace("-",""), "OK");
                        BusTripRecords_Data BusTripRecords_Profile = new BusTripRecords_Data
                        {
                            TripRound = BusRecord.TripRound,
                            Date_Of_Trip = BusRecord.Date_Of_Trip,
                            Bus_Number = BusRecord.Bus_Number,
                            Bus_Capacity = BusRecord.Bus_Capacity,
                            Bus_Route = BusRecord.Bus_Route,
                            Fare_Amount = BusRecord.Fare_Amount,
                            Total_Profit = BusRecord.Total_Profit,
                            TotalHoursTravel = difference.Hours.ToString().Replace("-", ""),
                            Travel_ID = TravelID
                        };
                        Cloud_Database.response = await Cloud_Database.client.SetAsync($"BusTripRecords_Data/{DateTime.Now.ToString("MMddyyyy")}/{TravelID}", BusTripRecords_Profile);
                        Cloud_Database.response = await Cloud_Database.client.DeleteAsync($"BusOnTrip/{TravelID}");
                        LoadBussesOnTrip();
                        DisplayMessage("Bus Travel Ended.", "Registerd Passenger Record", "OK");
                    }
                    else
                    {
                        DisplayMessage("","Travel ID is not registered.","OK");
                    }
                }
                //if (SelectedBusOnTrip == null)
                //{
                //    return;
                //}
            });
            OpenPassengerListCommand = new Command(async () =>
            {
                //if (SelectedBus == null)
                //{
                //    return;
                //}
                if (ConnectionStatus.ConnectedToInternet())
                {
                    if (await ConnectionStatus.TestConnectivity())
                    {

                        Navigation = navigation;
                        await Navigation.PushModalAsync(new BookerPassengerListPage(Employee_Number));
                    }
                    else
                    {
                        DisplayMessage("Cconnection Error", "Please check your internet connection.", "OK");
                    }
                }
                else
                {
                    DisplayMessage("Connection Error", "You are not connected to internet.", "OK");
                }
            });
            ChangeDriverOneCommand = new Command(() => 
            {
                if (ChangeDriverOne_Text.Equals("CHANGE"))
                {
                    ChangeDriverOne_Text = "CANCEL";
                    ChangeDriverOne = true;
                    OnPropertyChanged(nameof(ChangeDriverOne_Text));
                }
                else
                {
                    ChangeDriverOne_Text = "CHANGE";
                    ChangeDriverOne = false;
                    OnPropertyChanged(nameof(ChangeDriverOne_Text));
                }
            });
            ChangeDriverTwoCommand = new Command(() =>
            {
                if (ChangeDriverTwo_Text.Equals("CHANGE"))
                {
                    ChangeDriverTwo_Text = "CANCEL";
                    ChangeDriverTwo = true;
                    OnPropertyChanged(nameof(ChangeDriverTwo_Text));
                }
                else
                {
                    ChangeDriverTwo_Text = "CHANGE";
                    ChangeDriverTwo = false;
                    OnPropertyChanged(nameof(ChangeDriverTwo_Text));
                }
            });
            ChangeConducterCommand = new Command(() =>
            {
                if (ChangeConductor_Text.Equals("CHANGE"))
                {
                    ChangeConductor_Text = "CANCEL";
                    ChangeConductor = true;
                    OnPropertyChanged(nameof(ChangeConductor_Text));
                }
                else
                {
                    ChangeConductor_Text = "CHANGE";
                    ChangeConductor = false;
                    OnPropertyChanged(nameof(ChangeConductor_Text));
                }
            });
            SearchID_COMMAND = new Command(() =>
            {
                LoadPassengerHistory();
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
            AuthorizeBusTripCommand = new Command(async () =>
            {
                if (ConnectionStatus.ConnectedToInternet())
                {
                    if (await ConnectionStatus.TestConnectivity())
                    {
                        AuthorizeTrip();
                    }
                }
            });
            LoadAccountCommand = new Command(async () =>
            {
                if (ConnectionStatus.ConnectedToInternet())
                {
                    if (await ConnectionStatus.TestConnectivity())
                    {
                        Cloud_Database.response = await Cloud_Database.client.GetAsync($"Users/{AccountID}");
                        if (Cloud_Database.response.Body.ToString() != "null")
                        {
                            UserAccountProperties Account_Bal = Cloud_Database.response.ResultAs<UserAccountProperties>();
                            double Balance = double.Parse(Account_Bal.Account_Balance);
                            double AddAmount = double.Parse(LoadAmount);
                            double NewBalance = Balance + AddAmount;
                            DisplayMessage("Error", AddAmount.ToString(), "OK");
                            Cloud_Database.response = await Cloud_Database.client.SetAsync($"Users/{AccountID}/Account_Balance", NewBalance.ToString("0,0.00"));
                            Transactions_Data Transaction = new Transactions_Data
                            {
                                Transac_Date = DateTime.Now.ToString("dd, MMMM, yyyy"),
                                Account_ID = AccountID,
                                Amount = $"+ {AddAmount:0,0.00}"
                            };
                            Cloud_Database.response = await Cloud_Database.client.PushAsync($"Transactions_Data/{Employee_Number}", Transaction);
                            Cloud_Database.response = await Cloud_Database.client.PushAsync($"Users/{AccountID}/Transactions/", Transaction);
                            DisplayMessage("Success!", "Account Loaded Successfully.", "OK");
                        }
                        else
                        {
                            DisplayMessage("Invalid Account", "Account ID does not exist.", "Try Again");
                        }
                    }
                    else
                    {
                        DisplayMessage("Connection Error", "Please check your internet connection.", "OK");
                    }
                }
                else
                {
                    DisplayMessage("Connection Error", "You are not connected to internet", "OK");
                }
            });
            RegisterPayment = new Command(async () =>
            {
                if (ConnectionStatus.ConnectedToInternet())
                {
                    if (await ConnectionStatus.TestConnectivity())
                    {
                        RefID = GenerateReferenceID(12);
                        string dateID = dateNow.Replace("/", string.Empty);
                        if (isFreeOfCharge)
                        {
                            bool isContinue = await Application.Current.MainPage.DisplayAlert("Free of Charge", "Boarding as Free of Charge. Continue?", "YES", "NO");
                            if (isContinue)
                            {
                                ProcessPayment("0", "Board");
                                RegisterPassengerHistory("0", "To Board");
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(PassengerName) || !string.IsNullOrWhiteSpace(PassengerName) || !string.IsNullOrEmpty(PassengerContactNum) || !string.IsNullOrWhiteSpace(PassengerContactNum))
                            {
                                if (string.IsNullOrEmpty(_AmountPaid) || string.IsNullOrWhiteSpace(_AmountPaid) || _AmountPaid.Equals("0") || double.Parse(_AmountPaid) < double.Parse(_Bus_Fare))
                                {
                                    DisplayMessage("Invalid Amount","Invalid Amount Paid","OK");
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
                                                ProcessPayment(Discounted_Fare.ToString(), "Board");
                                                RegisterPassengerHistory(Discounted_Fare.ToString(), "To Board");
                                            }
                                        }
                                        else if (isNormalPay)
                                        {
                                            ProcessPayment(_Bus_Fare, "Board");
                                            RegisterPassengerHistory(_Bus_Fare, "To Board");;
                                        }
                                    }
                                    
                                }
                            }
                            else
                            {
                                DisplayMessage("","Passenger Name and Contact Number is Required.","OK");
                            }
                        }
                    }
                    else
                    {
                        DisplayMessage("Cconnection Error","Please check your internet connection.","OK");
                    }
                }
                else
                {
                    DisplayMessage("Connection Error","You are not connected to internet.","OK");
                }

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
                        await Navigation.PopModalAsync();
                        Cloud_Database.response = await Cloud_Database.client.GetAsync($"Users/{result.ToString()}");
                        if (Cloud_Database.response.Body.ToString() != "null")
                        {
                            UserAccountProperties PassengerInfo = Cloud_Database.response.ResultAs<UserAccountProperties>();
                            GeneratedID = result.ToString();
                            PassengerName = $"{PassengerInfo.FirstName} {PassengerInfo.LastName}";
                            OnPropertyChanged(nameof(GeneratedID));
                            OnPropertyChanged(nameof(PassengerName));
                            isScannedPay = true;
                        }
                    });
                };
                await Navigation.PushModalAsync(ScannerPage);
            });
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
                        DisplayMessage("Payment Failed", "Insuficient Balance.", "OK");
                    }
                    else
                    {
                        Transactions_Data Transaction = new Transactions_Data
                        {
                            Transac_Date = DateTime.Now.ToString("dd, MMMM, yyyy"),
                            Account_ID = GeneratedID,
                            Amount = $"- {BusFare:0,0.00}",
                            Purpose = $"Board {SelectedBus.bus_number} ({BusTravel_ID})",
                            Reference_ID = RefID,
                        };
                        Cloud_Database.response = await Cloud_Database.client.SetAsync($"Users/{GeneratedID}/Account_Balance", NewAccountBalance.ToString("0,0.00"));
                        Cloud_Database.response = await Cloud_Database.client.SetAsync($"Transactions_Data/{GeneratedID}/{RefID}", Transaction);
                        ProcessPayment(bus_fare, "Board");
                        RegisterPassengerHistory(bus_fare, "To Board");
                        RegisterPassengerRecord();
                        DisplayMessage("", "QR Payment Success.", "OK");
                    }
                }
            }
        }
        private async void AuthorizeTrip()
        {
            if (AuthorizeBus != null)
            {
                if (!AuthorizeBus.bus_driver_name_1.Equals("N/A") && !AuthorizeBus.bus_conductor_name.Equals("N/A"))
                {
                    GenerateTravelID();
                    bool isContinue = await Application.Current.MainPage.DisplayAlert("Authorize", $"Authorize Bus Trip for {AuthorizeBus.bus_number}?", "YES", "NO");
                    if (isContinue)
                    {
                        Cloud_Database.response = await Cloud_Database.client.GetAsync($"PassengerList/{DateTime.Now.ToString("MMddyyyy")}/{AuthorizeBus.bus_number}");
                        if (Cloud_Database.response.Body.ToString() != "null")
                        {
                            BusTripRecords_Data busTripRecords_ = Cloud_Database.response.ResultAs<BusTripRecords_Data>();
                            BusTripRecords_Data BusTripRecords_Profile = new BusTripRecords_Data
                            {
                                TripRound = BusTripRound,
                                Date_Of_Trip = DateTime.Now.ToString("MM/dd/yyyy"),
                                Bus_Number = AuthorizeBus.bus_number,
                                Bus_Capacity = busTripRecords_.Bus_Capacity,
                                Bus_Route = busTripRecords_.Bus_Route,
                                Fare_Amount = busTripRecords_.Fare_Amount,
                                Bus_Sitting = "0",
                                Bus_Standing = "0",
                                Total_Profit = busTripRecords_.Total_Profit,
                                Bus_TravelStartTime = DateTime.Now.ToString("hh:mm tt"),
                                Travel_ID = GeneratedTravel_ID,
                                DriverOne_ID = DriverOneID,
                                DriverTwo_ID = DriverTwoID,
                                Conductor_ID = ConductorID
                            };

                            Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/");
                            if (Cloud_Database.response.Body.ToString() != "null")
                            {
                                Dictionary<string, BusTripRecords_Data> BusTripData = Cloud_Database.response.ResultAs<Dictionary<string, BusTripRecords_Data>>();
                                foreach (var BusInfo in BusTripData)
                                {
                                    if (BusInfo.Value.Bus_Number.Equals(AuthorizeBus.bus_number))
                                    {
                                        BusOnTravel = true;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                BusOnTravel = false;
                            }
                            if (BusOnTravel)
                            {
                                DisplayMessage("Authorized", $"Bus Trip for {AuthorizeBus.bus_number} already Authorized", "OK");
                            }
                            else
                            {
                                Cloud_Database.response = await Cloud_Database.client.SetAsync($"BusOnTrip/{GeneratedTravel_ID}", BusTripRecords_Profile);
                                Cloud_Database.response = await Cloud_Database.client.GetAsync($"PassengerList/{DateTime.Now.ToString("MMddyyyy")}/{AuthorizeBus.bus_number}/Passengers");
                                if (Cloud_Database.response.Body.ToString() != "null")
                                {
                                    Dictionary<string, PassengerRecords_Data> getPassengers = Cloud_Database.response.ResultAs<Dictionary<string, PassengerRecords_Data>>();
                                    foreach (var passengers in getPassengers)
                                    {
                                        PassengerRecords_Data Passengers = new PassengerRecords_Data()
                                        {
                                            PassengerID = passengers.Value.PassengerID,
                                            PassengerName = passengers.Value.PassengerName,
                                            PassengerContactNumber = passengers.Value.PassengerContactNumber,
                                            Date_Of_Trip = passengers.Value.Date_Of_Trip,
                                            Boarded = passengers.Value.Boarded,
                                            Fare_Amount = passengers.Value.Fare_Amount,
                                            Travel_ID = GeneratedTravel_ID
                                        };
                                        Cloud_Database.response = await Cloud_Database.client.SetAsync($"ConductorConfirmation/{GeneratedTravel_ID}/Passengers/{passengers.Value.PassengerID}", Passengers);
                                    }
                                    BusTripHistory_Data BusTrip = new BusTripHistory_Data()
                                    {
                                        Date = DateTime.Now.ToString("MM/dd/yyyy h:mm tt"),
                                        BusNumber = AuthorizeBus.bus_number,
                                        BusRoute = AuthorizeBus.bus_route,
                                        BusDriverID_1 = DriverOneID,
                                        BusDriverName_1 = DriverOneName,
                                        BusDriverID_2 = DriverTwoID,
                                        BusDriverName_2 = DriverTwoName,
                                        BusConductorID = ConductorID,
                                        BusConductorName = ConductorName,
                                        BusTripRound = BusTripRound,
                                        Travel_ID = GeneratedTravel_ID,
                                    };
                                    Cloud_Database.response = await Cloud_Database.client.SetAsync($"BusTripHistory/{AuthorizeBus.bus_number}/{DateTime.Now.ToString("MMddyyyy")}", BusTrip);
                                    DisplayMessage("Authorized", "Bus Trip Authorized", "OK");
                                }
                            }
                        }
                        else
                        {
                            Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/");
                            if (Cloud_Database.response.Body.ToString() != "null")
                            {
                                Dictionary<string, BusTripRecords_Data> BusTripData = Cloud_Database.response.ResultAs<Dictionary<string, BusTripRecords_Data>>();
                                foreach (var BusInfo in BusTripData)
                                {
                                    if (BusInfo.Value.Bus_Number.Equals(AuthorizeBus.bus_number))
                                    {
                                        BusOnTravel = true;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                BusOnTravel = false;
                            }
                            if (BusOnTravel)
                            {
                                DisplayMessage("Authorized", $"Bus Trip for {AuthorizeBus.bus_number} already Authorized", "OK");
                            }
                            else
                            {
                                BusTripRecords_Data BusTripRecords_Profile = new BusTripRecords_Data
                                {
                                    TripRound = BusTripRound,
                                    Date_Of_Trip = DateTime.Now.ToString("MM/dd/yyyy"),
                                    Bus_Number = AuthorizeBus.bus_number,
                                    Bus_Capacity = AuthorizeBus.bus_seats,
                                    Bus_Route = AuthorizeBus.bus_route,
                                    Fare_Amount = AuthorizeBus.bus_fare,
                                    Bus_Sitting = "0",
                                    Bus_Standing = "0",
                                    Total_Profit = "0",
                                    Bus_TravelStartTime = DateTime.Now.ToString("hh:mm tt"),
                                    Travel_ID = GeneratedTravel_ID,
                                    DriverOne_ID = DriverOneID,
                                    DriverTwo_ID = DriverTwoID,
                                    Conductor_ID = ConductorID
                                };
                                Cloud_Database.response = await Cloud_Database.client.SetAsync($"BusOnTrip/{GeneratedTravel_ID}", BusTripRecords_Profile);
                                BusTripHistory_Data BusTrip = new BusTripHistory_Data()
                                {
                                    Date = DateTime.Now.ToString("MM/dd/yyyy h:mm tt"),
                                    BusNumber = AuthorizeBus.bus_number,
                                    BusRoute = AuthorizeBus.bus_route,
                                    BusDriverID_1 = DriverOneID,
                                    BusDriverName_1 = DriverOneName,
                                    BusDriverID_2 = DriverTwoID,
                                    BusDriverName_2 = DriverTwoName,
                                    BusConductorID = ConductorID,
                                    BusConductorName = ConductorName,
                                    BusTripRound = BusTripRound,
                                    Travel_ID = GeneratedTravel_ID
                                };
                                Cloud_Database.response = await Cloud_Database.client.SetAsync($"BusTripHistory/{AuthorizeBus.bus_number}/{DateTime.Now.ToString("MMddyyyy")}", BusTrip);
                                DisplayMessage("Authorized", "Bus Trip Authorized", "OK");
                            }
                        }
                    }
                    LoadBussesOnTrip();
                }
                else
                {
                    DisplayMessage("Cannot Proceed", "Please assign atleast one(1) driver and conductor.", "OK");
                }
            }
            else
            {
                DisplayMessage("Cannot Proceed", "Please Select Bus.", "OK");
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
        private async void ProcessPayment(string Fare, string mode)
        {
            Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{BusTravel_ID}");
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
                }
                await UpdatePassengerRecords(Fare, mode, "To Board");
                await UpdateTotalProfit(Fare, mode);
                //LoadPaymentHistory();
            }
            else
            {
                DisplayMessage("","Bus Trip for today is not available.","OK");
            }
        }
        private async Task UpdatePassengerRecords(string Fare, string mode, string status)
        {
            if (mode.Equals("Board"))
            {
                PassengerRecords_Data Passengers = new PassengerRecords_Data
                {
                    PassengerID = GeneratedID,
                    Date_Of_Trip = DateTime.Now.ToString("MM/dd/yyyy h:mm tt"),
                    PassengerName = PassengerName,
                    PassengerContactNumber = PassengerContactNum,
                    Boarded = SelectedBus.bus_number,
                    Fare_Amount = Fare,
                };
                Cloud_Database.response = await Cloud_Database.client.SetAsync($"ConductorConfirmation/{BusTravel_ID}/Passengers/{GeneratedID}", Passengers);
                Transactions_Data transactions_Data = new Transactions_Data
                {
                    Account_ID = GeneratedID,
                    Transac_Date = DateTime.Now.ToString("dd, MMMM, yyyy hh:mm tt"),
                    Reference_ID = RefID,
                    Amount = Fare,
                    Purpose = $"Board {SelectedBus.bus_number} ({BusTravel_ID})"
                };
                Cloud_Database.response = await Cloud_Database.client.SetAsync($"Transactions_Data/{Employee_Number}/{RefID}", transactions_Data);
                DisplayMessage("", "Passenger Boarded.", "OK");
                GenerateID();
            }
        }
        private async Task UpdateTotalProfit(string Fare, string mode)
        {
            string Total_Profit_Path = $"BusOnTrip/{BusTravel_ID}/Total_Profit";
            Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{BusTravel_ID}");
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
                Cloud_Database.response = await Cloud_Database.client.SetAsync($"BusOnTrip/{BusTravel_ID}/Bus_Sitting", _Count_Sitting.ToString());
            }
            else if (pos.Equals("Stand"))
            {
                Cloud_Database.response = await Cloud_Database.client.SetAsync($"BusOnTrip/{BusTravel_ID}/Bus_Standing", _Count_Standing.ToString());
            }
        }
        private async void RegisterPassengerHistory(string bus_fare, string Status)
        {
            PassengerRecords_Data PassengerHistory = new PassengerRecords_Data()
            {
                PassengerID = GeneratedID,
                PassengerName = PassengerName,
                PassengerContactNumber = PassengerContactNum,
                Boarded = SelectedBus.bus_number,
                PassengerStatus = Status,
                Fare_Amount = bus_fare,
                Date_Of_Trip = DateTime.Now.ToString("MM/dd/yyyy h:mm tt")
            };
            Cloud_Database.response = await Cloud_Database.client.PushAsync($"PassengerHistory/{DateTime.Now.ToString("MMddyyyy")}/{GeneratedID}", PassengerHistory);
            RegisterPassengerRecord();
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
            
            //LoadOldPassengerPicker();
        }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private async void DisplayMessage(string note, string message, string button_string) 
        {
            await Application.Current.MainPage.DisplayAlert(note,message,button_string);
        }
    }
}