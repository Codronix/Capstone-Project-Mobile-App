using Kerlyn_Liner_App.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Kerlyn_Liner_App.Pages.ConductorPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConductorTabbedPage : TabbedPage
    {
        public ConductorTabbedPage(string user, string TravelID)
        {
            InitializeComponent();
            BindingContext = new ConductorViewModel(user, TravelID, Navigation);
            NavigationPage.SetHasNavigationBar(this, false);
        }
        protected override bool OnBackButtonPressed()
        {
            // Begin an asyncronous task on the UI thread because we intend to ask the users permission.
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (await DisplayAlert("Logout", "Do you wish to Logout?", "Yes", "No"))
                {
                    base.OnBackButtonPressed();

                    await Navigation.PopToRootAsync();
                }
            });
            // Always return true because this method is not asynchronous.
            // We must handle the action ourselves: see above.
            return true;
        }
    }
}