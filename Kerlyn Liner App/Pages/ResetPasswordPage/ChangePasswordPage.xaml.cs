using Kerlyn_Liner_App.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Kerlyn_Liner_App.Pages.ResetPasswordPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChangePasswordPage : ContentPage
    {
        public ChangePasswordPage(string user)
        {
            InitializeComponent();
            BindingContext = new PasswordsViewModel(user);
        }
    }
}