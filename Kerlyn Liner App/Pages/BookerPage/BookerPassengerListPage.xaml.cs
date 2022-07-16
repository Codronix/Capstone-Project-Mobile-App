using Kerlyn_Liner_App.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Kerlyn_Liner_App.Pages.BookerPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BookerPassengerListPage : ContentPage
    {
        public BookerPassengerListPage(string Employee_Number)
        {
            InitializeComponent();
            BindingContext = new BookerPassengerListViewModel(Employee_Number);
        }
    }
}