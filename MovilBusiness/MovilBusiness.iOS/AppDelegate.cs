using Foundation;
using MovilBusiness.Utils;
using System;
using System.Net;
using System.Threading.Tasks;
using UIKit;
using UserNotifications;
using Xamarin.Forms;

namespace MovilBusiness.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Forms.SetFlags("CarouselView_Experimental");
            //Forms.SetFlags("CollectionView_Experimental");
            Rg.Plugins.Popup.Popup.Init();
            global::Xamarin.Forms.Forms.Init();
           // DependencyService.Register<Abstraction.IScreen, services.ScreenManipulator>();
            DependencyService.Register<Abstraction.IDialogOpcionesVisita, ui.DialogOpcionesVisitasiOS>();
            DependencyService.Register<Abstraction.IDialogCargaInicial, ui.DialogCargaInicialImpl>();

            LoadApplication(new App());

            Xamarin.FormsMaps.Init();

            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            ZXing.Net.Mobile.Forms.iOS.Platform.Init();

            // Check for permissions
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                // Ask the user for permission to get notifications on iOS 10.0+
                UNUserNotificationCenter.Current.RequestAuthorization(
                    UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound,
                    (approved, error) => {
                        if (approved)
                        {
                            UNUserNotificationCenter.Current.Delegate = new UNUserNotificationCenterDelegate();
                        }
                    });
            }
            else if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                // Ask the user for permission to get notifications on iOS 8.0+
                var settings = UIUserNotificationSettings.GetSettingsForTypes(
                    UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
                    new NSSet());

                UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
            }

            var notificationSettings = UIUserNotificationSettings.GetSettingsForTypes(
            UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound, null);

            app.RegisterUserNotificationSettings(notificationSettings);

            ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;

            if (UIDevice.CurrentDevice.CheckSystemVersion(15, 0))
            {
                var appearance = new UINavigationBarAppearance();
                appearance.ConfigureWithTransparentBackground();
                appearance.LargeTitleTextAttributes = new UIStringAttributes() { ForegroundColor = UIColor.White };
                appearance.BackgroundColor = UIColor.FromRGB(25, 118, 210);
                appearance.TitleTextAttributes = new UIStringAttributes() { ForegroundColor = UIColor.White };

                UINavigationBar.Appearance.CompactAppearance = appearance;
                UINavigationBar.Appearance.StandardAppearance = appearance;
                UINavigationBar.Appearance.ScrollEdgeAppearance = appearance;
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            UINavigationBar.Appearance.Translucent = false;

            return base.FinishedLaunching(app, options);
        }


        private async void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            await Functions.DisplayAlert("Aviso", "Ha ocurrido un error, la aplicacion se reiniciara");
            Functions.WriteExceptionLog(e.ExceptionObject as Exception, true);
        }

        private async void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            await Functions.DisplayAlert("Aviso", "Ha ocurrido un error, la aplicacion se reiniciara");
            Functions.WriteExceptionLog(e.Exception as Exception, true);
        }

        //prevent screenshots
        /*public override void OnResignActivation(UIApplication application)
        {
            var view = new UIView(Window.Frame)
            {
                Tag = new nint(101),
                BackgroundColor = UIColor.White
            };

            Window.AddSubview(view);
            Window.BringSubviewToFront(view);
        }

        // Remove window hiding app content when app is resumed
        public override void OnActivated(UIApplication application)
        {
            var view = Window.ViewWithTag(new nint(101));
            view?.RemoveFromSuperview();
        }*/
    }
}
