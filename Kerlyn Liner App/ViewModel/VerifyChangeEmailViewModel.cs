using Kerlyn_Liner_App.DataProperties;
using Kerlyn_Liner_App.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

namespace Kerlyn_Liner_App.ViewModel
{
    class VerifyChangeEmailViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        Database Cloud_Database = new Database();
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public INavigation Navigation { get; set; }
        public Command VerifyChangeCommand { get; set; }
        public string NewEmail { get; set; }
        public string InputedPassword { get; set; }
        public string InputedVerificationCode { get; set; }
        private string AccountID;
        private string Password;
        private string EmailVerificationCode;
        public VerifyChangeEmailViewModel(string user, string new_email, string VerificationCode, INavigation navigation)
        {
            Cloud_Database.Cloud_DB_Connect();
            LoadAccountDetails(user, new_email, VerificationCode);
            ButtonCommands(navigation);
        }
        private async void LoadAccountDetails(string user, string new_email, string VerificationCode)
        {
            Cloud_Database.response = await Cloud_Database.client.GetAsync($"Users/{user}");
            UserAccountProperties User = Cloud_Database.response.ResultAs<UserAccountProperties>();
            AccountID = User.ID;
            Password = User.Password;
            NewEmail = new_email;
            EmailVerificationCode = VerificationCode;
            OnPropertyChanged(nameof(NewEmail));
        }
        private void ButtonCommands(INavigation navigation)
        {
            VerifyChangeCommand = new Command(async () =>
            {
                if (Password.Equals(InputedPassword))
                {
                    if (EmailVerificationCode.Equals(InputedVerificationCode))
                    {
                        Navigation = navigation;
                        Cloud_Database.response = await Cloud_Database.client.SetAsync($"Users/{AccountID}/Email", NewEmail);
                        await Application.Current.MainPage.DisplayAlert("", "Email has been successfully changed.", "OK");
                        await Navigation.PopAsync();
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("", "Incorrect Code Entered.", "OK");
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("", "Incorrect Password.", "OK");
                }
            });
        }
    }
}
