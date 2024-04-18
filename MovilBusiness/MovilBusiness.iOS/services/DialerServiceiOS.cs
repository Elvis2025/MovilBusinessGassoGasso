using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using MovilBusiness.Abstraction;
using MovilBusiness.iOS.services;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(DialerServiceiOS))]
namespace MovilBusiness.iOS.services
{
   public class DialerServiceiOS: IDialerService
    {
        public bool Call(string number)
        {
            return UIApplication.SharedApplication.OpenUrl(
                new NSUrl("tel:" + number));
        }
    }
}