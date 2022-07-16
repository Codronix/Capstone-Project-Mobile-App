using Kerlyn_Liner_App.DataProperties;
using Kerlyn_Liner_App.DB;
using Kerlyn_Liner_App.Pages.BookerPage;
using Kerlyn_Liner_App.Pages.DigitalReceiptPage;
using Kerlyn_Liner_App.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

namespace Kerlyn_Liner_App.ViewModel
{
    class TransactionsViewModel : INotifyPropertyChanged
    {
        Database Cloud_Database = new Database();
        InternetConnectionChecker ConnectionStatus = new InternetConnectionChecker();
        public ObservableCollection<Transactions_Data> Transaction_LIST { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public INavigation Navigation { get; set; }
        public Command SelectionCommand { get; set; }
        Transactions_Data selectedTransaction;
        public Transactions_Data SelectedTransaction
        {
            get { return selectedTransaction; }
            set
            {
                if (selectedTransaction != value)
                {
                    selectedTransaction = value;
                }
            }
        }
        public TransactionsViewModel(string User, INavigation navigation)
        {
            Cloud_Database.Cloud_DB_Connect();
            LoadTransactionData(User);
            ButtonCommands(User, navigation);
        }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void ButtonCommands(string User, INavigation navigation)
        {
            SelectionCommand = new Command(async () =>
            {
                Navigation = navigation;
                if (selectedTransaction == null)
                {
                    return;
                }
                await Navigation.PushModalAsync(new ReceiptPage(User, SelectedTransaction.Reference_ID));
                selectedTransaction = null;
                OnPropertyChanged(nameof(SelectedTransaction));
            });
        }
        private async void LoadTransactionData(object user)
        {
            if (ConnectionStatus.ConnectedToInternet())
            {
                if (await ConnectionStatus.TestConnectivity())
                {
                    Cloud_Database.response = await Cloud_Database.client.GetAsync($"Transactions_Data/{user}");
                    if (Cloud_Database.response.Body.ToString() != "null")
                    {
                        Dictionary<string, Transactions_Data> Transactions = Cloud_Database.response.ResultAs<Dictionary<string, Transactions_Data>>();
                        Transaction_LIST = new ObservableCollection<Transactions_Data>();
                        foreach (var Transac_Data in Transactions)
                        {
                            Transaction_LIST.Add(new Transactions_Data
                            {
                                Account_ID = Transac_Data.Value.Account_ID,
                                Reference_ID = Transac_Data.Value.Reference_ID,
                                Purpose = Transac_Data.Value.Purpose,
                                Amount = Transac_Data.Value.Amount,
                                Transac_Date = Transac_Data.Value.Transac_Date
                            });
                        }
                        OnPropertyChanged(nameof(Transaction_LIST));
                    }
                }
            }
        }
    }
}
