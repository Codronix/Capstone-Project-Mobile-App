using Kerlyn_Liner_App.Pages.BookerPage;
using Kerlyn_Liner_App.Pages.ConductorPage;
using Kerlyn_Liner_App.Pages.DigitalReceiptPage;
using Kerlyn_Liner_App.Pages.DriverPage;
using Kerlyn_Liner_App.Pages.InspectorPage;
using Kerlyn_Liner_App.Pages.LoginPage;
using Kerlyn_Liner_App.Pages.PassengerPage;
using Kerlyn_Liner_App.Pages.ResetPasswordPage;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Kerlyn_Liner_App
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new MainPage());
        }
        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
