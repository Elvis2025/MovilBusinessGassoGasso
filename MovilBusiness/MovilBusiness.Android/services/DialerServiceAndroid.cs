﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Telephony;
using Android.Views;
using Android.Widget;
using MovilBusiness.Abstraction;
using MovilBusiness.Droid.services;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(DialerServiceAndroid))]
namespace MovilBusiness.Droid.services
{
  
   public class DialerServiceAndroid : IDialerService
    {

        
            public bool Call(string number)
            {
               var context = MainActivity.Instance;
                if (context == null)
                    return false;

                var intent = new Intent(Intent.ActionDial);

                intent.SetData(Android.Net.Uri.Parse("tel:" + number));

                if (IsIntentAvailable(context, intent))
                {
                    context.StartActivity(intent);
                    return true;
                }

                return false;
            }

            private static bool IsIntentAvailable(Context context, Intent intent)
            {

                var packageManager = context.PackageManager;

                var list = packageManager.QueryIntentServices(intent, 0)
                    .Union(packageManager.QueryIntentActivities(intent, 0));
                if (list.Any())
                    return true;

                TelephonyManager mgr = TelephonyManager.FromContext(context);
                return mgr.PhoneType != PhoneType.None;
            }
        
    }
}
