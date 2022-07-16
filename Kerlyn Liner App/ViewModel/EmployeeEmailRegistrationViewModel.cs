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
    class EmployeeEmailRegistrationViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        Database Cloud_Database = new Database();
        EmailService emailService = new EmailService();
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public string Email { get; set; }
        private bool emailValid;
        public bool EmailValid
        {
            get => emailValid;
            set
            {
                emailValid = value;
                OnPropertyChanged(nameof(EmailValid));
            }
        }
        public Command ProceedCommand { get; set; }
        public INavigation Navigation { get; set; }
        private string EmailVerificationCode = string.Empty;
        private bool EmailMatch = false;
        public EmployeeEmailRegistrationViewModel(string user, INavigation navigation)
        {
            Cloud_Database.Cloud_DB_Connect();
            ButtonCommands(user, navigation);
        }
        private void ButtonCommands(string user, INavigation navigation)
        {
            ProceedCommand = new Command(async () =>
            {
                if (!string.IsNullOrEmpty(Email) || !string.IsNullOrWhiteSpace(Email))
                {
                    if (emailValid)
                    {
                        Cloud_Database.response = await Cloud_Database.client.GetAsync("Users");
                        if (Cloud_Database.response.Body.ToString() != "null")
                        {
                            Dictionary<string, UserAccountProperties> getUsers = Cloud_Database.response.ResultAs<Dictionary<string, UserAccountProperties>>();
                            foreach (var User in getUsers)
                            {
                                if (Email.Equals(User.Value.Email, StringComparison.OrdinalIgnoreCase))
                                {
                                    EmailMatch = true;
                                    break;
                                }
                                else
                                {
                                    EmailMatch = false;
                                }
                            }
                            if (EmailMatch)
                            {
                                await Application.Current.MainPage.DisplayAlert("","This email is already in use.","OK");
                            }
                            else
                            {
                                GenerateVerificationCode();
                                emailService.SendMailMessage(Email, $"Hi, Your verification code is: {EmailVerificationCode}");
                                Navigation = navigation;
                                await Navigation.PushAsync(new EmailVerificationPage(user, Email, EmailVerificationCode, false));
                            }
                        }
                        else
                        {
                            GenerateVerificationCode();
                            emailService.SendMailMessage(Email, $"Hi, Your verification code is: {EmailVerificationCode}");
                            Navigation = navigation;
                            await Navigation.PushAsync(new EmailVerificationPage(user, Email, EmailVerificationCode, false));
                        }
                    }
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
