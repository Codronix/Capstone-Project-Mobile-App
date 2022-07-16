using Kerlyn_Liner_App.DataProperties;
using Kerlyn_Liner_App.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Kerlyn_Liner_App.ViewModel
{
    class ReceiptViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        Database Cloud_Databse = new Database();
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public string Purpose { get; set; }
        public string Date { get; set; }
        public string Amount { get; set; }
        public string Passenger_ID { get; set; }
        public string Reference_ID { get; set; }
        public ReceiptViewModel(string User, string ReferenceID)
        {
            Cloud_Databse.Cloud_DB_Connect();
            LoadReceiptData(User, ReferenceID);
        }
        private async void LoadReceiptData(string User, string ReferenceID)
        {
            Cloud_Databse.response = await Cloud_Databse.client.GetAsync($"Transactions_Data/{User}/{ReferenceID}");
            if (Cloud_Databse.response.Body.ToString() != "null")
            {
                Transactions_Data transactions_Data = Cloud_Databse.response.ResultAs<Transactions_Data>();
                Purpose = transactions_Data.Purpose;
                Date = transactions_Data.Transac_Date;
                Amount = transactions_Data.Amount;
                Passenger_ID = transactions_Data.Account_ID;
                Reference_ID = transactions_Data.Reference_ID;
                OnPropertyChanged(nameof(Purpose));
                OnPropertyChanged(nameof(Date));
                OnPropertyChanged(nameof(Amount));
                OnPropertyChanged(nameof(Passenger_ID));
                OnPropertyChanged(nameof(Reference_ID));
            }
        }
    }
}
