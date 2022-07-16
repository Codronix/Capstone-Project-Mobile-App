using Kerlyn_Liner_App.DB;
using Kerlyn_Liner_App.Pages.LoginPage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

namespace Kerlyn_Liner_App.ViewModel
{
    class SetNewPasswordViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        Database Cloud_Databse = new Database();
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public Command SetNewPasswordCommand { get; set; }
        public Command ShowNewPassword { get; set; }
        public Command ShowConfirmPassword { get; set; }
        private bool _ShowingNewPassword = true;
        public bool ShowingNewPassword
        {
            get { return _ShowingNewPassword; }
            set
            {
                _ShowingNewPassword = value;
                OnPropertyChanged(nameof(ShowingNewPassword));
            }
        }
        private bool _ShowingConfirmPassword = true;
        public bool ShowingConfirmPassword
        {
            get { return _ShowingConfirmPassword; }
            set
            {
                _ShowingConfirmPassword = value;
                OnPropertyChanged(nameof(ShowingConfirmPassword));
            }
        }
        private string _NewPassword;
        public string NewPassword
        {
            get { return _NewPassword; }
            set
            {
                _NewPassword = value;
                OnPropertyChanged(nameof(NewPassword));
            }
        }
        public string ConfirmPassword { get; set; }
        public INavigation Navigation { get; set; }
        private bool _PasswordLengthAllowed;
        public bool PasswordLengthAllowed
        {
            get { return _PasswordLengthAllowed; }
            set
            {
                _PasswordLengthAllowed = value; 
                OnPropertyChanged(nameof(PasswordLengthAllowed));
            }
        }
        private bool _PasswordMatch;
        public bool PasswordMatch
        {
            get { return _PasswordMatch; }
            set
            {
                _PasswordMatch = value;
                OnPropertyChanged(nameof(PasswordMatch));
            }
        }
        private string UserID;
        public SetNewPasswordViewModel(string User, INavigation navigation)
        {
            Cloud_Databse.Cloud_DB_Connect();
            LoadUser(User);
            SetNewPasswordCommand = new Command(async () =>
            {
                if (PasswordLengthAllowed && PasswordMatch)
                {
                    Cloud_Databse.response = await Cloud_Databse.client.SetAsync($"Users/{UserID}/Password", NewPassword);
                    Navigation = navigation;
                    await Navigation.PushAsync(new AccountPasswordResetInfoPage());
                }
            });
            ShowNewPassword = new Command(() =>
            {
                if (ShowingNewPassword)
                {
                    ShowingNewPassword = false;
                }
                else
                {
                    ShowingNewPassword = true;
                }
            });
            ShowConfirmPassword = new Command(() =>
            {
                if (ShowingConfirmPassword)
                {
                    ShowingConfirmPassword = false;
                }
                else
                {
                    ShowingConfirmPassword = true;
                }
            });
        }
        private void LoadUser(string User)
        {
            UserID = User;
        }
    }
}
