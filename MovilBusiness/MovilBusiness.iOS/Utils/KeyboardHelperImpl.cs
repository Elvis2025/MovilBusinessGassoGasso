using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using Microsoft.AppCenter.Crashes;
using MovilBusiness.Abstraction;
using MovilBusiness.iOS.Utils;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(KeyboardHelperImpl))]
namespace MovilBusiness.iOS.Utils
{
    public class KeyboardHelperImpl : IkeyboardHelper
    {
        public void HideKeyboard()
        {
            try
            {
                UIApplication.SharedApplication.KeyWindow.EndEditing(true);
            }
            catch(Exception e)
            {
                Crashes.TrackError(e);
            }
        }
    }
}