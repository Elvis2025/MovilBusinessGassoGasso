
using MovilBusiness.Enums;

namespace MovilBusiness.Abstraction
{
    public interface IScreen
    {
        void KeepLightsOn(bool value);
        void ChangeDeviceOrientation(ScreenOrientation orientation);

        void ChangeLanguage(string language);
    }
}
