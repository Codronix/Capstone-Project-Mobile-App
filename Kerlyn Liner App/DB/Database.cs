using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Kerlyn_Liner_App.DB
{
    class Database
    {
        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "<Enter your Firebase API Key here>",
            BasePath = "<Firebase Realtime database link here>"
        };

        public IFirebaseClient client;
        public FirebaseResponse response;

        public void Cloud_DB_Connect()
        {
            try
            {
                client = new FireSharp.FirebaseClient(config);
            }
            catch(Exception ex)
            {
                Application.Current.MainPage.DisplayAlert("ERROR CONNECTION",ex.ToString(),"OK");
            }
        }
    }
}
