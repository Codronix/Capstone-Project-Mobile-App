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
    public partial class PaymentPage : ContentPage
    {
        public PaymentPage()
        {
            InitializeComponent();
        }

        private void cbFreeOfCharge_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (cbSeniorDiscount.IsChecked || cbOrdinaryPassenger.IsChecked)
            {
                cbSeniorDiscount.IsChecked = false;
                cbOrdinaryPassenger.IsChecked = false;
            }
            else
            {
                cbFreeOfCharge.IsChecked = true;
            }
        }

        private void cbSeniorDiscount_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (cbFreeOfCharge.IsChecked || cbOrdinaryPassenger.IsChecked)
            {
                cbFreeOfCharge.IsChecked = false;
                cbOrdinaryPassenger.IsChecked = false;
            }
            else
            {
                cbSeniorDiscount.IsChecked = true;
            }
        }

        private void cbOrdinaryPassenger_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (cbFreeOfCharge.IsChecked || cbSeniorDiscount.IsChecked)
            {
                cbFreeOfCharge.IsChecked = false;
                cbSeniorDiscount.IsChecked = false;
            }
            else
            {
                cbOrdinaryPassenger.IsChecked = true;
            }
        }
    }
}