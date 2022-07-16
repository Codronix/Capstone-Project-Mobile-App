using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kerlyn_Liner_App.CustomRenderer;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Kerlyn_Liner_App.Droid;

[assembly: ExportRenderer(typeof(CustomPicker), typeof(MyPickerRenderer))]
namespace Kerlyn_Liner_App.Droid
{
    class MyPickerRenderer : PickerRenderer
    {
        public MyPickerRenderer(Context context) : base(context)
        {
        }
        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Rgb(0, 102, 204));
            }
        }
    }
}