using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kerlyn_Liner_App.Services
{
    class InternetConnectionChecker
    {
        public bool ConnectedToInternet()
        {
            if (CrossConnectivity.Current.IsConnected)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public async Task<bool> TestConnectivity()
        {
            bool canReach = await CrossConnectivity.Current.IsReachable("8.8.8.8");
            return canReach;
        }
    }
}
