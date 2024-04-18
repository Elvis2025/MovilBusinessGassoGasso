using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Foundation;
using MovilBusiness.Abstraction;
using MovilBusiness.iOS.Configuration;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(AppInfoManager))]
namespace MovilBusiness.iOS.Configuration
{
    public class AppInfoManager : IAppInfo
    {
        public string AppVersion()
        {
            try
            {
                return NSBundle.MainBundle.InfoDictionary["CFBundleVersion"].ToString();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return "";
        }

        public string ProductsImagePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Imagenes");            
        }

        public string DocumentsPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/";// Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library");
        }

        public string DatabasePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "..", "Library"); ;
        }

        public double BatteryLevel()
        {
            return UIDevice.CurrentDevice.BatteryLevel;
        }
    }
}