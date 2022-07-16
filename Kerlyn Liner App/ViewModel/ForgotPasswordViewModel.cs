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
    class ForgotPasswordViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        EmailService emailService = new EmailService();
        Database Cloud_Database = new Database();
        InternetConnectionChecker ConnectionStatus = new InternetConnectionChecker();
        protected void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
        public Command ContinueCommand { get; set; }
        public INavigation Navigation { get; set; }
        public string Email { get; set; }
        private string EmailVerificationCode;
        private string UserRegisteredEmail;
        private bool EmailFound = false;
        private string User;
        public ForgotPasswordViewModel(INavigation navigation)
        {
            Cloud_Database.Cloud_DB_Connect();
            ContinueCommand = new Command(async () =>
            {
                if (ConnectionStatus.ConnectedToInternet())
                {
                    if (await ConnectionStatus.TestConnectivity())
                    {
                        Cloud_Database.response = await Cloud_Database.client.GetAsync("Users");
                        if (Cloud_Database.response.Body.ToString() != "null")
                        {
                            Dictionary<string, UserAccountProperties> getUsers = Cloud_Database.response.ResultAs<Dictionary<string, UserAccountProperties>>();
                            foreach (var UserEmail in getUsers)
                            {
                                UserRegisteredEmail = UserEmail.Value.Email;
                                if (UserRegisteredEmail.Equals(Email))
                                {
                                    User = UserEmail.Value.ID;
                                    EmailFound = true;
                                    break;
                                }
                                else
                                {
                                    EmailFound = false;
                                }
                            }
                            if (EmailFound)
                            {
                                GenerateVerificationCode();
                                emailService.SendMailMessage(Email, $"Hi, Your reset password verification code is: {EmailVerificationCode}");
                                Navigation = navigation;
                                await Navigation.PushAsync(new EmailVerificationPage(User, Email, EmailVerificationCode, true));
                            }
                            else
                            {
                                await Application.Current.MainPage.DisplayAlert("", "Email is not found registered.", "Try Again");
                            }
                        }
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Connection Error", "Internet may not be available. Please check your connection.", "OK");
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("No Internet", "You are not connected to the internet.", "OK");
                }
                
            });
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
    }
}
