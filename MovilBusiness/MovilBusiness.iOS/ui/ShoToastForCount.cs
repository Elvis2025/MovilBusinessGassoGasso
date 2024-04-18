using Foundation;
using MovilBusiness.Abstraction;
using MovilBusiness.iOS.ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(ShoToastForCount))]
namespace MovilBusiness.iOS.ui
{
    public class ShoToastForCount : IAppToast
    {
        public void ShowToast(string order)
        {
            //throw new NotImplementedException();
        }
    }
}