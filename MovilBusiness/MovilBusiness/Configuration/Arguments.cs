using MovilBusiness.Abstraction;
using MovilBusiness.model;
using MovilBusiness.model.Internal;

namespace MovilBusiness.Configuration
{
    public static class Arguments
    {
        public static Representantes CurrentUser { get; set; } = null;
        public static string CurrentAud { get; set; }
        public static int IsValidToGetOut { get; set; } = -1;
        public static Runtime Values { get; set; } = new Runtime();
        public static bool IsDownloadingImages { get; set; }
        public static byte[] Imagenes { get; set; }

        public const bool UseMicrosoftAuth = false;

        public static IPlatform PlatformService { get; set; }

        public static void LogOut()
        {
            App.Current.Properties.Remove("CurrentRep");
            IsValidToGetOut = -1;
            CurrentUser = null;
            CurrentAud = "";
            Imagenes = null;
            Values.Clear();
        }
        
    }
}
