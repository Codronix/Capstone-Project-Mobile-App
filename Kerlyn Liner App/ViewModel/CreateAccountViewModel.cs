using Kerlyn_Liner_App.DataProperties;
using Kerlyn_Liner_App.DB;
using Kerlyn_Liner_App.Services;
using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using Kerlyn_Liner_App.Pages.LoginPage;

namespace Kerlyn_Liner_App.ViewModel
{
    class CreateAccountViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        Database Cloud_Database = new Database();
        InternetConnectionChecker ConnectionStatus = new InternetConnectionChecker();
        EmailService emailService = new EmailService();
        private string EmailVerificationCode = string.Empty;
        private bool emailValid;
        private bool passwordMatch;
        private string password;
        private bool passwordLengthAllowed;
        private bool usernameLengthAllowed;
        private bool firstNameLengthAllowed;
        private bool lastNameLengthAllowed;

        public bool EmailValid
        {
            get => emailValid;
            set
            {
                emailValid = value;
                OnPropertyChanged(nameof(EmailValid));
            }
        }
        public bool PasswordMatch
        {
            get => passwordMatch;
            set
            {
                passwordMatch = value;
                OnPropertyChanged(nameof(PasswordMatch));
            }
        }
        public bool PasswordLengthAllowed
        {
            get => passwordLengthAllowed;
            set
            {
                passwordLengthAllowed = value;
                OnPropertyChanged(nameof(PasswordLengthAllowed));
            }
        }
        public bool UsernameLengthAllowed
        {
            get => usernameLengthAllowed;
            set
            {
                usernameLengthAllowed = value;
                OnPropertyChanged(nameof(UsernameLengthAllowed));
            }
        }
        public bool FirstNameLengthAllowed
        {
            get => firstNameLengthAllowed;
            set
            {
                firstNameLengthAllowed = value;
                OnPropertyChanged(nameof(FirstNameLengthAllowed));
            }
        }
        public bool LastNameLengthAllowed
        { 
            get => lastNameLengthAllowed;
            set
            {
                lastNameLengthAllowed = value; 
                OnPropertyChanged(nameof(LastNameLengthAllowed));
            }
        }
        public string Email { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Confirm_Password { get; set; }
        public string Password
        {
            get => password;
            set
            {
                password = value;
                OnPropertyChanged(nameof(Password));
            }
        }
        public Command CreateAccountCommand { get; set; }
        public INavigation Navigation { get; set; }
        public CreateAccountViewModel(INavigation navigation)
        {
            Cloud_Database.Cloud_DB_Connect();
            CreateAccountCommand = new Command(() =>
            {
                CreateAccount(navigation);
            });
        }
        private async void CreateAccount(INavigation navigation)
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(FirstName) ||
                string.IsNullOrEmpty(LastName) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(Confirm_Password) ||
                string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(FirstName) ||
                string.IsNullOrWhiteSpace(LastName) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(Confirm_Password))
            {
                DisplayMessage("Please complete all required fields");
            }
            else
            {
                if (ConnectionStatus.ConnectedToInternet())
                {
                    if (await ConnectionStatus.TestConnectivity())
                    {
                        if (emailValid && usernameLengthAllowed && firstNameLengthAllowed && lastNameLengthAllowed && passwordLengthAllowed && passwordMatch)
                        {
                            Cloud_Database.response = await Cloud_Database.client.GetAsync($"Users");
                            if (Cloud_Database.response.Body.ToString() != "null")
                            {
                                Dictionary<string, UserAccountProperties> getUsersData = Cloud_Database.response.ResultAs<Dictionary<string, UserAccountProperties>>();
                                List<string> List_Email = new List<string>();
                                List<string> List_Username = new List<string>();
                                bool Email_Exist = false;
                                bool Username_Exist = false;
                                foreach (var data in getUsersData)
                                {
                                    List_Email.Add(data.Value.Email);
                                    List_Username.Add(data.Value.Username);
                                }
                                for (int Email_Counter = 0; Email_Counter < List_Email.Count; Email_Counter++)
                                {
                                    if (Email.Equals(List_Email[Email_Counter]))
                                    {
                                        DisplayMessage("Email already exist.");
                                        Email_Exist = true;
                                        break;
                                    }
                                }
                                for (int Username_Counter = 0; Username_Counter < List_Username.Count; Username_Counter++)
                                {
                                    if (Username.Equals(List_Username[Username_Counter]))
                                    {
                                        DisplayMessage("Username already exist.");
                                        Username_Exist = true;
                                        break;
                                    }
                                }
                                if (!Email_Exist && !Username_Exist)
                                {
                                    GenerateVerificationCode();
                                    emailService.SendMailMessage(Email, $"Hi, Your verification code is: {EmailVerificationCode}");
                                    Navigation = navigation;
                                    await Navigation.PushAsync(new EmailVerificationPage(Email, Username, FirstName, LastName, Password, EmailVerificationCode));
                                }
                            }
                            else
                            {
                                GenerateVerificationCode();
                                emailService.SendMailMessage(Email, $"Hi, Your verification code is: {EmailVerificationCode}");
                                Navigation = navigation;
                                await Navigation.PushAsync(new EmailVerificationPage(Email, Username, FirstName, LastName, Password, EmailVerificationCode));
                            }
                        }
                    }
                }
                else
                {
                    DisplayMessage("You are not connected to internt.");
                }
            }
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
        protected void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
        private async void DisplayMessage(string Message)
        {
            await Application.Current.MainPage.DisplayAlert("Create Account", Message, "OK");
        }
    }
}
