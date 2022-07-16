using Kerlyn_Liner_App.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Kerlyn_Liner_App.Pages.DigitalReceiptPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReceiptPage : ContentPage
    {
        public ReceiptPage(string User, string ReferenceID)
        {
            InitializeComponent();
            BindingContext = new ReceiptViewModel(User, ReferenceID);
        }
    }
}