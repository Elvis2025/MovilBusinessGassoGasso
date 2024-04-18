using Android.Views;
using Java.Util;
using MovilBusiness.Abstraction;
using MovilBusiness.Droid.services;
using MovilBusiness.Enums;
using System;
using Xamarin.Forms;

[assembly: Dependency(typeof(ScreenManipulator))]
namespace MovilBusiness.Droid.services
{
    public class ScreenManipulator : IScreen
    {
        public void ChangeDeviceOrientation(ScreenOrientation orientation)
        {
            try
            {
                switch (orientation)
                {
                    case ScreenOrientation.PORTRAIT:
                        MainActivity.Instance.RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
                        break;
                    case ScreenOrientation.LANDSCAPE:
                        MainActivity.Instance.RequestedOrientation = Android.Content.PM.ScreenOrientation.Landscape;
                        break;
                    case ScreenOrientation.UNSPECIFIED:
                        MainActivity.Instance.RequestedOrientation = Android.Content.PM.ScreenOrientation.Unspecified;
                        break;
                }
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }
        }

        public void KeepLightsOn(bool value)
        {
            Window window = MainActivity.Instance.Window;
            
            if (value)
            {
                window.AddFlags(WindowManagerFlags.KeepScreenOn);
            }
            else
            {
                window.ClearFlags(WindowManagerFlags.KeepScreenOn);
            }
        }

        public void ChangeLanguage(string language)
        {
            try
            {
                
                var res = MainActivity.Instance.Resources;

                var config = res.Configuration;

                var locale = new Locale(language);

                //Locale.SetDefault(locale);
                Locale.Default = locale;

                config.SetLocale(locale);
                //config.SetLayoutDirection(locale);

                //res.UpdateConfiguration(config, res.DisplayMetrics);

                MainActivity.Instance.CreateConfigurationContext(config);

                res.UpdateConfiguration(config, res.DisplayMetrics);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }
    }
}