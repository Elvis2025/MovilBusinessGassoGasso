using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Microsoft.AppCenter.Crashes;
using MovilBusiness.Abstraction;
using MovilBusiness.Droid.utils;
using Xamarin.Forms;

[assembly: Dependency(typeof(KeyboardHelperImpl))]
namespace MovilBusiness.Droid.utils
{
    public class KeyboardHelperImpl : IkeyboardHelper
    {
        public void HideKeyboard()
        {
            try
            {
                var context = MainActivity.Instance;

                var inputMethodManager = context.GetSystemService(Context.InputMethodService) as InputMethodManager;
                if (inputMethodManager != null)
                {
                    var token = context.CurrentFocus?.WindowToken;
                    inputMethodManager.HideSoftInputFromWindow(token, HideSoftInputFlags.None);

                    context.Window.DecorView.ClearFocus();
                }
            }
            catch(Exception e)
            {
                Crashes.TrackError(e);
            }
        }
    }
}