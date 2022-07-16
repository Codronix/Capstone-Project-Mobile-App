using Kerlyn_Liner_App.Pages.ConductorPage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

namespace Kerlyn_Liner_App.ViewModel
{
    class PaymentViewModel : INotifyPropertyChanged
    {
        public Command Scan_QR { get; set; }
        public INavigation Navigation { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public PaymentViewModel(INavigation navigation)
        {
           Scan_QR = new Command(async () =>
           {
               Navigation = navigation;
               await Navigation.PushModalAsync(new QR_ScanPage());
           });
        }
    }
}
