using Kerlyn_Liner_App.DataProperties;
using Kerlyn_Liner_App.DB;
using Kerlyn_Liner_App.Pages.LoginPage;
using Kerlyn_Liner_App.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

namespace Kerlyn_Liner_App.ViewModel
{
    class EmailVerificationViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        Database Cloud_Database = new Database();
        InternetConnectionChecker ConnectionStatus = new InternetConnectionChecker();
        protected void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
        public INavigation Navigation { get; set; }
        public string Email { get; set; }
        public string InputCode { get; set; }

        string userID = string.Empty;
        public Command VerifyCommand { get; set; }
        public EmailVerificationViewModel(string _Email, string Username, string FirstName, string LastName, string Password, string VerificationCode, INavigation navigation)
        {
            Cloud_Database.Cloud_DB_Connect();
            LoadEmailText(_Email);
            VerifyCommand = new Command(() =>
            {
                if (InputCode.Equals(VerificationCode))
                {
                    CreateAccount(_Email, Username, FirstName, LastName, Password, navigation);
                }
                else
                {
                    Application.Current.MainPage.DisplayAlert("","Incorrect Code Entered.","OK");
                }
            });
        }
        public EmailVerificationViewModel(string user, string _Email, string VerificationCode, bool ResetPassword, INavigation navigation)
        {
            Cloud_Database.Cloud_DB_Connect();
            LoadEmailText(_Email, user);
            VerifyCommand = new Command(async () =>
            {
                if (ResetPassword)
                {
                    if (InputCode.Equals(VerificationCode))
                    {
                        Navigation = navigation;
                        await Navigation.PushAsync(new SetNewPasswordPage(userID));
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("", "Incorrect Code Entered.", "OK");
                    }
                }
                else
                {
                    if (InputCode.Equals(VerificationCode))
                    {
                        SetEmployeeEmail(user, _Email, navigation);
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("", "Incorrect Code Entered.", "OK");
                    }
                }
            });
        }
        private void LoadEmailText(string emailtext)
        {
            Email = emailtext;
            OnPropertyChanged(nameof(Email));
        }
        private void LoadEmailText(string emailtext, string user)
        {

            Email = emailtext;
            userID = user;
            OnPropertyChanged(nameof(Email));
        }
        private async void CreateAccount(string _Email, string Username, string FirstName, string LastName, string Password, INavigation navigation)
        {
            if (ConnectionStatus.ConnectedToInternet())
            {
                if (await ConnectionStatus.TestConnectivity())
                {
                    GenerateID(Username);
                    UserAccountProperties UserAccount = new UserAccountProperties
                    {
                        ID = userID,
                        Email = _Email,
                        Username = Username,
                        FirstName = FirstName,
                        LastName = LastName,
                        Password = Password,
                        Position = "Passenger",
                        Account_Balance = "0"
                    };
                    Cloud_Database.response = await Cloud_Database.client.SetAsync($"Users/{userID}", UserAccount);
                    Navigation = navigation;
                    await Navigation.PushAsync(new AccountCreatedInfoPage());
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("", "Please check your internet connection and try again.", "OK");
                }
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("","You are not connected to internet.","OK");
            }
        }
        private async void SetEmployeeEmail(string User, string Email, INavigation navigation)
        {
            if (ConnectionStatus.ConnectedToInternet())
            {
                if (await ConnectionStatus.TestConnectivity())
                {
                    Cloud_Database.response = await Cloud_Database.client.SetAsync($"Users/{User}/Email", Email);
                    Navigation = navigation;
                    await Navigation.PushAsync(new AccountCreatedInfoPage());
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("", "Please check your internet connection and try again.", "OK");
                }
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("", "You are not connected to internet.", "OK");
            }
        }
        
        private void GenerateID(string Username)
        {
            string num = "1234567890";
            int len = num.Length;
            string otp = string.Empty;
            int otpDigit = 6;
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
            userID = $"{Username}@{otp}";
        }
    }
}
