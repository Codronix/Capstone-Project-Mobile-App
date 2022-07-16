using Kerlyn_Liner_App.DataProperties;
using Kerlyn_Liner_App.DB;
using Kerlyn_Liner_App.Pages.PassengerPage;
using Kerlyn_Liner_App.Pages.ResetPasswordPage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

namespace Kerlyn_Liner_App.ViewModel
{
    class PassengerSettingsViewModel : INotifyPropertyChanged
    {
        Database Cloud_Database = new Database();
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public INavigation Navigation { get; set; }
        public string AccountID { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public Command OpenChangeEmailPageCommnad { get; set; }
        public Command OpenChangePasswordPageCommand { get; set; }
        public PassengerSettingsViewModel(string user, INavigation navigation)
        {
            Cloud_Database.Cloud_DB_Connect();
            LoadAccountDetails(user);
            ButtonCommnads(navigation);
        }
        private async void LoadAccountDetails(string user)
        {
            Cloud_Database.response = await Cloud_Database.client.GetAsync($"Users/{user}");
            UserAccountProperties User = Cloud_Database.response.ResultAs<UserAccountProperties>();
            AccountID = User.ID;
            Username = User.Username;
            FullName = $"{User.FirstName} {User.LastName}";
            Email = User.Email;
            OnPropertyChanged(nameof(AccountID));
            OnPropertyChanged(nameof(Username));
            OnPropertyChanged(nameof(FullName));
            OnPropertyChanged(nameof(Email));
        }
        private void ButtonCommnads(INavigation navigation)
        {
            OpenChangePasswordPageCommand = new Command(async () =>
            {
                Navigation = navigation;
                await Navigation.PushAsync(new ChangePasswordPage(AccountID));
            });
            OpenChangeEmailPageCommnad = new Command(async () => 
            {
                Navigation = navigation;
                await Navigation.PushAsync(new PassengerChangeEmailPage(AccountID));
            });
        }
    }
}