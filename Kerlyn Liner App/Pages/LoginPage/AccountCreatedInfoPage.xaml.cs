using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Kerlyn_Liner_App.Pages.LoginPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AccountCreatedInfoPage : ContentPage
    {
        public AccountCreatedInfoPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private void btnBackToLogin_Clicked(object sender, EventArgs e)
        {
            Navigation.PopToRootAsync();
        }
    }
}