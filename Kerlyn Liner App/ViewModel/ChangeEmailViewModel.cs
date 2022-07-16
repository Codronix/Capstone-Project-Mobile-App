using Kerlyn_Liner_App.DataProperties;
using Kerlyn_Liner_App.DB;
using Kerlyn_Liner_App.Pages.PassengerPage;
using Kerlyn_Liner_App.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

namespace Kerlyn_Liner_App.ViewModel
{
    class ChangeEmailViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        Database Cloud_Database = new Database();
        EmailService emailService = new EmailService();
        public INavigation Navigation { get; set; }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public string NewEmail { get; set; }
        private string AccountID;
        private string CurrentEmail;
        private string EmailVerificationCode;
        public string InputedPassword { get; set; }
        public string InputedVerificationCode { get; set; }
        public Command ChangeEmailCommand { get; set; }
        public Command VerifyChangeCommand { get; set; }
        private bool MatchingEmailFound = false;
        private bool emailValid;
        public bool EmailValid
        {
            get { return emailValid; }
            set { emailValid = value; OnPropertyChanged(nameof(EmailValid)); }
        }
        public ChangeEmailViewModel(string User, INavigation navigation)
        {
            Cloud_Database.Cloud_DB_Connect();
            LoadAccountDetails(User);
            ButtonCommands(navigation);
        }
        private async void LoadAccountDetails(string user)
        {
            Cloud_Database.response = await Cloud_Database.client.GetAsync($"Users/{user}");
            UserAccountProperties User = Cloud_Database.response.ResultAs<UserAccountProperties>();
            AccountID = User.ID;
            CurrentEmail = User.Email;
        }
        private void GenerateVerificationCode()
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
            EmailVerificationCode = otp;
        }
        private void ButtonCommands(INavigation navigation)
        {
            ChangeEmailCommand = new Command(async () => 
            {
                if (CurrentEmail.Equals(NewEmail))
                {
                    await Application.Current.MainPage.DisplayAlert("","Email entered is already the current email.","OK");
                }
                else
                {
                    Cloud_Database.response = await Cloud_Database.client.GetAsync("Users");
                    Dictionary<string, UserAccountProperties> getUsersData = Cloud_Database.response.ResultAs<Dictionary<string, UserAccountProperties>>();
                    foreach (var UserEmail in getUsersData)
                    {
                        if (UserEmail.Value.Email.Equals(NewEmail))
                        {
                            MatchingEmailFound = true;
                            break;
                        }
                        else
                        {
                            MatchingEmailFound = false;
                        }
                    }
                    if (MatchingEmailFound)
                    {
                        await Application.Current.MainPage.DisplayAlert("", "This email is already in use.", "OK");
                    }
                    else
                    {
                        GenerateVerificationCode();
                        emailService.SendMailMessage(NewEmail, $"Hi, Your verification code is: {EmailVerificationCode}");
                        Navigation = navigation;
                        await Navigation.PushAsync(new PassengerVerifyEmailChangePage(AccountID, NewEmail, EmailVerificationCode));
                    }
                }
            });
        }
    }
}
