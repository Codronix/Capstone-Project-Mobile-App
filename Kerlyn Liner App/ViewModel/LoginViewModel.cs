using Kerlyn_Liner_App.DataProperties;
using Kerlyn_Liner_App.DB;
using Kerlyn_Liner_App.Pages.BookerPage;
using Kerlyn_Liner_App.Pages.ConductorPage;
using Kerlyn_Liner_App.Pages.DriverPage;
using Kerlyn_Liner_App.Pages.InspectorPage;
using Kerlyn_Liner_App.Pages.LoginPage;
using Kerlyn_Liner_App.Pages.PassengerPage;
using Kerlyn_Liner_App.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Kerlyn_Liner_App.ViewModel
{
    class LoginViewModel : INotifyPropertyChanged
    {
        Database Cloud_Database = new Database();
        InternetConnectionChecker ConnectionStatus = new InternetConnectionChecker();
        public Command SignInCommand { get; set; }
        public Command SignUpCommand { get; set; }
        public Command ResetPasswordCommand { get; set; }
        public INavigation Navigation { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        private string Position;
        private string _Username;
        private string _Password;
        private string Email;

        public event PropertyChangedEventHandler PropertyChanged;

        public LoginViewModel(INavigation navigation)
        {
            Cloud_Database.Cloud_DB_Connect();
            SignInCommand = new Command(async () =>
            {
                Login(navigation);
            });
            SignUpCommand = new Command(async () =>
            {
                Navigation = navigation;
                await Navigation.PushAsync(new SignUpPage());
            });
            ResetPasswordCommand = new Command(async () => 
            {
                Navigation = navigation;
                await Navigation.PushAsync(new ResetPasswordPage());
            });
        }

        private void ClearFields()
        {
            Username = string.Empty;
            Password = string.Empty;
            OnPropertyChanged(nameof(Username));
            OnPropertyChanged(nameof(Password));
        }
        private async void Login(INavigation navigation)
        {
            if (ConnectionStatus.ConnectedToInternet())
            {
                // Test if it can connect to internet
                if (await ConnectionStatus.TestConnectivity())
                {
                    if (Username != null && Password != null)
                    {
                        Cloud_Database.response = await Cloud_Database.client.GetAsync($"Users");
                        if (Cloud_Database.response.Body.ToString() != "null")
                        {
                            Dictionary<string, UserAccountProperties> getUser = Cloud_Database.response.ResultAs<Dictionary<string, UserAccountProperties>>();
                            foreach (var User in getUser)
                            {
                                if (Username.Equals(User.Value.Username) && Password.Equals(User.Value.Password) && User.Value.Position.Equals("Driver"))
                                {
                                    _Username = User.Value.Username;
                                    _Password = User.Value.Password;
                                    Position = User.Value.Position;
                                    Email = User.Value.Email;
                                    break;
                                }
                                else if (Username.Equals(User.Value.Username) && Password.Equals(User.Value.Password) && User.Value.Position.Equals("Conductor"))
                                {
                                    _Username = User.Value.Username;
                                    _Password = User.Value.Password;
                                    Position = User.Value.Position;
                                    Email = User.Value.Email;
                                    break;
                                }
                                else if (Username.Equals(User.Value.Username) && Password.Equals(User.Value.Password) && User.Value.Position.Equals("Booker"))
                                {
                                    _Username = User.Value.Username;
                                    _Password = User.Value.Password;
                                    Position = User.Value.Position;
                                    Email = User.Value.Email;
                                    break;
                                }
                                else if (Username.Equals(User.Value.Username) && Password.Equals(User.Value.Password) && User.Value.Position.Equals("Inspector"))
                                {
                                    _Username = User.Value.Username;
                                    _Password = User.Value.Password;
                                    Position = User.Value.Position;
                                    Email = User.Value.Email;
                                    break;
                                }
                                else if (Username.Equals(User.Value.Username) && Password.Equals(User.Value.Password) && User.Value.Position.Equals("Passenger"))
                                {
                                    _Username = User.Value.Username;
                                    _Password = User.Value.Password;
                                    Position = User.Value.Position;
                                    break;
                                }
                            }
                            if (Username.Equals(_Username) && Password.Equals(_Password) && Position.Equals("Driver"))
                            {
                                if (string.IsNullOrEmpty(Email) || string.IsNullOrWhiteSpace(Email))
                                {
                                    Navigation = navigation;
                                    await Navigation.PushAsync(new EmployeeRegistrationPage(_Username));
                                }
                                else
                                {
                                    Navigation = navigation;
                                    await Navigation.PushAsync(new TravelVerificationPage($"Account_Data/Driver/{Username}"));
                                    ClearFields();
                                }
                            }
                            else if (Username.Equals(_Username) && Password.Equals(_Password) && Position.Equals("Conductor"))
                            {
                                if (string.IsNullOrEmpty(Email) || string.IsNullOrWhiteSpace(Email))
                                {
                                    Navigation = navigation;
                                    await Navigation.PushAsync(new EmployeeRegistrationPage(_Username));
                                }
                                else
                                {
                                    Navigation = navigation;
                                    await Navigation.PushAsync(new TravelVerificationPage($"Account_Data/Conductor/{Username}"));
                                    ClearFields();
                                }
                            }
                            else if (Username.Equals(_Username) && Password.Equals(_Password) && Position.Equals("Booker"))
                            {
                                if (string.IsNullOrEmpty(Email) || string.IsNullOrWhiteSpace(Email))
                                {
                                    Navigation = navigation;
                                    await Navigation.PushAsync(new EmployeeRegistrationPage(_Username));
                                }
                                else
                                {
                                    Navigation = navigation;
                                    await Navigation.PushAsync(new BookerTabbedPage($"Account_Data/Booker/{Username}"));
                                    ClearFields();
                                }
                            }
                            else if (Username.Equals(_Username) && Password.Equals(_Password) && Position.Equals("Inspector"))
                            {
                                if (string.IsNullOrEmpty(Email) || string.IsNullOrWhiteSpace(Email))
                                {
                                    Navigation = navigation;
                                    await Navigation.PushAsync(new EmployeeRegistrationPage(_Username));
                                }
                                else
                                {
                                    Navigation = navigation;
                                    await Navigation.PushAsync(new InspectorTabbedPage($"Account_Data/Inspector/{Username}"));
                                    ClearFields();
                                }
                            }
                            else if (Username.Equals(_Username) && Password.Equals(_Password) && Position.Equals("Passenger"))
                            {
                                Dictionary<string, UserAccountProperties> UserAccount = Cloud_Database.response.ResultAs<Dictionary<string, UserAccountProperties>>();
                                foreach (var get in UserAccount)
                                {
                                    if (Username.Equals(get.Value.Username))
                                    {
                                        Navigation = navigation;
                                        await Navigation.PushAsync(new PassengerTabbedPage(get.Value.ID));
                                        ClearFields();
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                DisplayMessage("Username or Password is Incorrect");
                            }
                        }
                    }
                    else
                    {
                        DisplayMessage("Username and Password is empty");
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
        protected void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
        private async void DisplayMessage(string Message)
        {
            await Application.Current.MainPage.DisplayAlert("Login", Message, "Try Again");
        }
    }
}
