using Kerlyn_Liner_App.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Kerlyn_Liner_App
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = new LoginViewModel(Navigation);
            NavigationPage.SetHasNavigationBar(this, false);
        }
    }
}
