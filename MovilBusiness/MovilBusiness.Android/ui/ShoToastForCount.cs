using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MovilBusiness.Abstraction;
using MovilBusiness.Droid.ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using Xamarin.Forms;

[assembly: Dependency(typeof(ShoToastForCount))]
namespace MovilBusiness.Droid.ui
{
    public class ShoToastForCount: IAppToast
    {
        public ShoToastForCount(){}

        public void ShowToast(string order)
        {
            Toast.MakeText(Android.App.Application.Context, order, ToastLength.Short).Show();
        }
    }
}