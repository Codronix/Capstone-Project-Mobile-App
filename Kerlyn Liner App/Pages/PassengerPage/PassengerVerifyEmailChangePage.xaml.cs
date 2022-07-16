using Kerlyn_Liner_App.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Kerlyn_Liner_App.Pages.PassengerPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PassengerVerifyEmailChangePage : ContentPage
    {
        public PassengerVerifyEmailChangePage(string User, string new_email, string VerificationCode)
        {
            InitializeComponent();
            BindingContext = new VerifyChangeEmailViewModel(User, new_email, VerificationCode, Navigation);
        }
    }
}