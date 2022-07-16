using Kerlyn_Liner_App.DataProperties;
using Kerlyn_Liner_App.DB;
using Kerlyn_Liner_App.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using ZXing;

namespace Kerlyn_Liner_App.ViewModel
{
    class QR_ScanViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        Database Cloud_Database = new Database();
        InternetConnectionChecker ConnectionStatus = new InternetConnectionChecker();
        public Result Result { get; set; }
        private bool isScanning = true;
        public bool IsScanning
        {
            get { return isScanning; }
            set
            {
                isScanning = value;
                OnPropertyChanged(nameof(IsScanning));
            }
        }
        public Command ScanCommand{ get; }
        public QR_ScanViewModel(string bus_fare)
        {
            Cloud_Database.Cloud_DB_Connect();
            ScanCommand = new Command(async () =>
            {
                if (isScanning)
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        if (ConnectionStatus.ConnectedToInternet())
                        {
                            if (await ConnectionStatus.TestConnectivity())
                            {
                                Cloud_Database.response = await Cloud_Database.client.GetAsync($"Users/{Result.Text}");
                                if (Cloud_Database.response.Body.ToString() != "null")
                                {
                                    UserAccountProperties UserAccount = Cloud_Database.response.ResultAs<UserAccountProperties>();
                                    double BusFare = double.Parse(bus_fare);
                                    double NewAccountBalance = double.Parse(UserAccount.Account_Balance) - BusFare;
                                    var isProceed = await Application.Current.MainPage.DisplayAlert("QR Payment", $"Proceed payment for Account ID: {Result.Text} with a fare of ₱ {BusFare:0,0.00}", "Proceed", "Cancel");
                                    if (isProceed)
                                    {
                                        Transactions_Data Transaction = new Transactions_Data
                                        {
                                            Transac_Date = DateTime.Now.ToString("dd, MMMM, yyyy"),
                                            Account_ID = Result.Text,
                                            Amount = $"- {BusFare:0,0.00}"
                                        };
                                        Cloud_Database.response = await Cloud_Database.client.SetAsync($"Users/{Result.Text}/Account_Balance", NewAccountBalance.ToString("0,0.00"));
                                        Cloud_Database.response = await Cloud_Database.client.PushAsync($"Users/{Result.Text}/Transactions/", Transaction);
                                        // PUSH PASSENGER TO BUS
                                        await DisplayMessage("QR Payment Success.");
                                        isScanning = true;
                                    }
                                    else
                                    {
                                        isScanning = true;
                                    }
                                }
                            }
                            else
                            {
                                await DisplayErrorMessage("Please check your internet connection.");
                                isScanning = true;
                            }
                        }
                        else
                        {
                            await DisplayErrorMessage("You are not connected to internet.");
                        }
                    });
                    isScanning = false;
                }
            });
        }
        private async Task DisplayMessage(string msg)
        {
            await Application.Current.MainPage.DisplayAlert("QR Payment", msg, "OK");
        }
        private async Task DisplayErrorMessage(string msg)
        {
            await Application.Current.MainPage.DisplayAlert("QR Payment", msg, "Try Again");
        }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
