using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Kerlyn_Liner_App.Droid;
using Kerlyn_Liner_App.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

[assembly: Dependency(typeof(LocationUpdateService))]
namespace Kerlyn_Liner_App.Droid
{
    public class LocationEventArgs : ILocationEventArgs
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
    public class LocationUpdateService : Java.Lang.Object, ILocationUpdateService, ILocationListener
    {
        LocationManager locationManager;
        public event EventHandler<ILocationEventArgs> LocationChanged;

        event EventHandler<ILocationEventArgs> ILocationUpdateService.LocationChanged
        {
            add 
            {
                LocationChanged += value;
            }
            remove 
            {
                LocationChanged -= value;
            }
        }
        public void GetUserLocation() 
        {
            locationManager = (LocationManager)MainActivity.Context.GetSystemService(Context.LocationService);
            locationManager.RequestLocationUpdates(LocationManager.GpsProvider, 0, 0, this);
            locationManager.RequestLocationUpdates(LocationManager.NetworkProvider, 0, 0, this);
        }
        ~LocationUpdateService()
        {
            locationManager.RemoveUpdates(this);
        }
        public void OnLocationChanged(Location location)
        {
            if(location != null) 
            {
                LocationEventArgs args = new LocationEventArgs
                {
                    Latitude = location.Latitude,
                    Longitude = location.Longitude
                };
                LocationChanged(this, args);
            }
        }

        public void OnProviderDisabled(string provider)
        {
            
        }

        public void OnProviderEnabled(string provider)
        {
            
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            
        }
    }
}