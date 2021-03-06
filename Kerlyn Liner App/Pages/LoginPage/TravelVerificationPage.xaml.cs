using Kerlyn_Liner_App.ViewModel;
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
    public partial class TravelVerificationPage : ContentPage
    {
        public TravelVerificationPage(string User)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            BindingContext = new TravelVerificationViewModel(User, Navigation);
        }
    }
}