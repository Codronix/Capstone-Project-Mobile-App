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
    public partial class EmailVerificationPage : ContentPage
    {
        public EmailVerificationPage(string Email, string Username, string FirstName, string LastName, string Password, string VerificationCode)
        {
            InitializeComponent();
            BindingContext = new EmailVerificationViewModel(Email, Username, FirstName,LastName, Password, VerificationCode,Navigation);
            NavigationPage.SetHasNavigationBar(this, false);
        }
        public EmailVerificationPage(string user, string Email, string VerificationCode, bool ResetPassword)
        {
            InitializeComponent();
            BindingContext = new EmailVerificationViewModel(user, Email, VerificationCode, ResetPassword, Navigation);
            NavigationPage.SetHasNavigationBar(this,false);
        }
    }
}