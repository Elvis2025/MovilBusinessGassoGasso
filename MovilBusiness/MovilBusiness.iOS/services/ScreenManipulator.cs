
using MovilBusiness.Abstraction;
using MovilBusiness.Enums;
using MovilBusiness.iOS.services;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(ScreenManipulator))]
namespace MovilBusiness.iOS.services
{
    public class ScreenManipulator : IScreen
    {

        public ScreenManipulator() { }

        public void ChangeDeviceOrientation(ScreenOrientation orientation)
        {
           
        }

        public void ChangeLanguage(string language)
        {
            throw new System.NotImplementedException();
        }

        public void KeepLightsOn(bool value)
        {
            UIApplication.SharedApplication.IdleTimerDisabled = value;
        }
    }
}