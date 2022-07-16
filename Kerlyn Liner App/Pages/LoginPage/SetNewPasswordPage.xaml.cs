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
    public partial class SetNewPasswordPage : ContentPage
    {
        public SetNewPasswordPage(string User)
        {
            InitializeComponent();
            BindingContext = new SetNewPasswordViewModel(User, Navigation);
            NavigationPage.SetHasBackButton(this, false);
        }
    }
}