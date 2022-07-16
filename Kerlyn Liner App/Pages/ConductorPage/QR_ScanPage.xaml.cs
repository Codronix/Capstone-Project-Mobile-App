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
    public partial class QR_ScanPage
    {
        public QR_ScanPage()
        {
            InitializeComponent();
        }
        public void SetBusFare(string BusFare)
        {
            BindingContext = new QR_ScanViewModel(BusFare);
        }
    }
}