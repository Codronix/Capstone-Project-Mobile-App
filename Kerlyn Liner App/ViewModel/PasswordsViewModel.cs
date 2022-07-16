using Kerlyn_Liner_App.DataProperties;
using Kerlyn_Liner_App.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

namespace Kerlyn_Liner_App.ViewModel
{
    class PasswordsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        Database Cloud_Database = new Database();
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public bool isHideCurrentPassword { get; set; } = true;
        public bool isHideNewPassword { get; set; } = true;
        public bool isHideReEnteredPassword { get; set; } = true;
        public Command ShowCurrentPasswordCommand { get; set; }
        public Command ShowNewPasswordCommand { get; set; }
        public Command ShowReEnteredPasswordCommand { get; set; }
        public Command ChangePasswordCommand { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ReEnteredPassword { get; set; }
        public PasswordsViewModel(string user)
        {
            Cloud_Database.Cloud_DB_Connect();
            ButtonCommands(user);
        }
        private async void ChangePassword(string user)
        {
            Cloud_Database.response = await Cloud_Database.client.GetAsync($"Users/{user}");
            UserAccountProperties User = Cloud_Database.response.ResultAs<UserAccountProperties>();
            if (CurrentPassword.Equals(User.Password))
            {
                if (NewPassword.Length >= 8 )
                {
                    if (NewPassword.Equals(ReEnteredPassword))
                    {
                        UpdatePassword(user, User.Position);
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("", "Re-Entered Password is incorrect.", "OK");
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("", "Password should be 8 characters above.", "OK");
                }
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("","Current Password is incorrect.","OK");
            }
        }
        private async void UpdatePassword(string User, string Position)
        {
            if (Position.Equals("Driver"))
            {
                Cloud_Database.response = await Cloud_Database.client.SetAsync($"Account_Data/{Position}/{User}/password", NewPassword);
                Cloud_Database.response = await Cloud_Database.client.SetAsync($"Users/{User}/Password", NewPassword);
                await Application.Current.MainPage.DisplayAlert("", "Your password has been changed successfully", "OK");
            }
            else if (Position.Equals("Conductor"))
            {
                Cloud_Database.response = await Cloud_Database.client.SetAsync($"Account_Data/{Position}/{User}/password", NewPassword);
                Cloud_Database.response = await Cloud_Database.client.SetAsync($"Users/{User}/Password", NewPassword);
                await Application.Current.MainPage.DisplayAlert("", "Your password has been changed successfully", "OK");
            }
            else if (Position.Equals("Booker"))
            {
                Cloud_Database.response = await Cloud_Database.client.SetAsync($"Account_Data/{Position}/{User}/password", NewPassword);
                Cloud_Database.response = await Cloud_Database.client.SetAsync($"Users/{User}/Password", NewPassword);
                await Application.Current.MainPage.DisplayAlert("", "Your password has been changed successfully", "OK");
            }
            else if (Position.Equals("Inspector"))
            {
                Cloud_Database.response = await Cloud_Database.client.SetAsync($"Account_Data/{Position}/{User}/password", NewPassword);
                Cloud_Database.response = await Cloud_Database.client.SetAsync($"Users/{User}/Password", NewPassword);
                await Application.Current.MainPage.DisplayAlert("", "Your password has been changed successfully", "OK");
            }
            else if (Position.Equals("Passenger"))
            {
                Cloud_Database.response = await Cloud_Database.client.SetAsync($"Users/{User}/Password", NewPassword);
                await Application.Current.MainPage.DisplayAlert("", "Your password has been changed successfully", "OK");
            }
            ClearFields();
        }
        private void ClearFields()
        {
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ReEnteredPassword = string.Empty;
            OnPropertyChanged(nameof(CurrentPassword));
            OnPropertyChanged(nameof(NewPassword));
            OnPropertyChanged(nameof(ReEnteredPassword));
        }
        private void ButtonCommands(string user)
        {
            ShowCurrentPasswordCommand = new Command(() =>
            {
                if (isHideCurrentPassword)
                {
                    isHideCurrentPassword = false;
                }
                else
                {
                    isHideCurrentPassword = true;
                }
                OnPropertyChanged(nameof(isHideCurrentPassword));
            });
            ShowNewPasswordCommand = new Command(() =>
            {
                if (isHideNewPassword)
                {
                    isHideNewPassword = false;
                }
                else
                {
                    isHideNewPassword = true;
                }
                OnPropertyChanged(nameof(isHideNewPassword));
            });
            ShowReEnteredPasswordCommand = new Command(() =>
            {
                if (isHideReEnteredPassword)
                {
                    isHideReEnteredPassword = false;
                }
                else
                {
                    isHideReEnteredPassword = true;
                }
                OnPropertyChanged(nameof(isHideReEnteredPassword));
            });
            ChangePasswordCommand = new Command(() =>
            {
                ChangePassword(user);
            });
        }
    }
}
