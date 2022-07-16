using Kerlyn_Liner_App.DataProperties;
using Kerlyn_Liner_App.DB;
using Kerlyn_Liner_App.Pages.ConductorPage;
using Kerlyn_Liner_App.Pages.DriverPage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

namespace Kerlyn_Liner_App.ViewModel
{
    class TravelVerificationViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        Database Cloud_Database = new Database();
        protected void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
        public string Travel_ID { get; set; }
        public Command ConfirmCommand { get; set; }
        private string EmployeeID;
        public INavigation Navigation { get; set; }
        public TravelVerificationViewModel(string User, INavigation navigation)
        {
            Cloud_Database.Cloud_DB_Connect();
            LoadEmployeeInfo(User);
            ButtonCommands(User, navigation);
        }
        private async void LoadEmployeeInfo(string User)
        {
            Cloud_Database.response = await Cloud_Database.client.GetAsync(User);
            if (Cloud_Database.response.Body.ToString() != "null")
            {
                Account_Data EmployeeInfo = Cloud_Database.response.ResultAs<Account_Data>();
                EmployeeID = EmployeeInfo.id;
            }
        }
        private void ButtonCommands(string User, INavigation navigation)
        {
            ConfirmCommand = new Command(async () =>
            {
                Cloud_Database.response = await Cloud_Database.client.GetAsync($"BusOnTrip/{Travel_ID}");
                if (Cloud_Database.response.Body.ToString() != "null")
                {
                    BusTripRecords_Data BusOnTrip = Cloud_Database.response.ResultAs<BusTripRecords_Data>();
                    if (EmployeeID.Equals(BusOnTrip.DriverOne_ID) || EmployeeID.Equals(BusOnTrip.DriverTwo_ID))
                    {
                        Navigation = navigation;
                        await Navigation.PushAsync(new DriverTabbedPage(User, Travel_ID));
                    }
                    else if (EmployeeID.Equals(BusOnTrip.Conductor_ID))
                    {
                        Navigation = navigation;
                        await Navigation.PushAsync(new ConductorTabbedPage(User, Travel_ID));
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("","You are not assigned to this Travel ID.","OK");
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("","Travel ID is not registered.","OK");
                }
            });
        }
    }
}
