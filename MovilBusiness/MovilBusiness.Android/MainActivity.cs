using Android.App;
using Android.Content.PM;
using Android.Views;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V4.App;
using MovilBusiness.Views.Components.Views;
using System;
using System.Net;
using System.Threading.Tasks;
using MovilBusiness.Utils;
using ZXing.Mobile;
using Android.Runtime;
using Plugin.CurrentActivity;
using Plugin.Fingerprint;
using Xamarin.Forms;
using Android.Content;
using Android.Media;
using Microsoft.Identity.Client;
using MovilBusiness.Abstraction;
using System.Web;
using MovilBusiness.DataAccess;

namespace MovilBusiness.Droid
{
    [Activity(Label = "MovilBusiness", Icon = "@drawable/appicon", MainLauncher = true, Theme = "@style/splashscreen", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.Locale, ScreenOrientation = ScreenOrientation.Portrait, LaunchMode = LaunchMode.SingleTop)]
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault },//, DataHost = "auth", DataScheme = "msala1c98f46-d140-49ef-8d18-91202a65b4a9z")//,
        DataPath = "/M61nf%2BaC69kCXmFY1ejcX83rDNc%3D",
        DataHost = "com.mdsoft.MovilBusiness",
        DataScheme = "msauth")]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IPlatform
    {
        internal static Activity Instance { get; private set; }

        protected override void OnCreate(Bundle bundle)
        {
            base.SetTheme(Resource.Style.MainTheme);

            Instance = this;
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            
            base.OnCreate(bundle);

            Xamarin.Essentials.Platform.Init(this, bundle);

            //Forms.SetFlags("FastRenderers_Experimental");
            Forms.SetFlags("CarouselView_Experimental");
            //Forms.SetFlags("CollectionView_Experimental");
            Rg.Plugins.Popup.Popup.Init(this, bundle);
            CrossFingerprint.SetCurrentActivityResolver(() => CrossCurrentActivity.Current.Activity);
            CrossCurrentActivity.Current.Init(this, bundle);
            global::Xamarin.Forms.Forms.Init(this, bundle);

            MessagingCenter.Subscribe<string,string[]>(this, "Notification", (msg,args) =>
            {
                if (msg != null && msg == "Notification")
                {
                    SendNotifacation("MovilBusiness", args);
                }

            });

            LoadApplication(new App(this));

            FFImageLoading.Forms.Platform.CachedImageRenderer.Init(false);
            ZXing.Net.Mobile.Forms.Android.Platform.Init();
            MobileBarcodeScanner.Initialize(Application);

            Xamarin.FormsMaps.Init(this, bundle);

            bool permission = ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.WriteExternalStorage) == Permission.Granted
                && ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.ReadExternalStorage) == Permission.Granted
                && ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.AccessCoarseLocation) == Permission.Granted
                && ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.AccessFineLocation) == Permission.Granted
                && ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.Bluetooth) == Permission.Granted
                && ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.BluetoothAdmin) == Permission.Granted
                && ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.Camera) == Permission.Granted
                && ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.Flashlight) == Permission.Granted
                && ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.InstallPackages) == Permission.Granted
                && ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.RequestInstallPackages) == Permission.Granted;

            if (!permission)
            {
                ActivityCompat.RequestPermissions(this, new string[] {
                    Android.Manifest.Permission.ReadExternalStorage,
                    Android.Manifest.Permission.WriteExternalStorage,
                    Android.Manifest.Permission.AccessCoarseLocation,
                    Android.Manifest.Permission.AccessFineLocation,
                    Android.Manifest.Permission.Camera,
                    Android.Manifest.Permission.Flashlight,
                    Android.Manifest.Permission.Bluetooth,
                    Android.Manifest.Permission.BluetoothAdmin,
                    Android.Manifest.Permission.InstallPackages,
                    Android.Manifest.Permission.RequestInstallPackages}, 549);
            }

            ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;

          //  AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            //AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironment_UnhandledExceptionRaiser;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironment_UnhandledExceptionRaiser;

            if (DS_RepresentantesParametros.GetInstance().GetParOrientationPantallaSensor())
            {
                RequestedOrientation = ScreenOrientation.Sensor;
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(requestCode, resultCode, data);
        }

        private async void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            await Functions.DisplayAlert("Aviso", "Ha ocurrido un error, la aplicacion se reiniciara");
            Functions.WriteExceptionLog(e.ExceptionObject as Exception, true);
        }

        private void AndroidEnvironment_UnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            e.Handled = true;
            Functions.DisplayAlert("Aviso", "Ha ocurrido un error: " + e.Exception.Message);
            Functions.WriteExceptionLog(e.Exception, false);
        }

        private async void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            await Functions.DisplayAlert("Aviso", "Ha ocurrido un error, la aplicacion se reiniciara");
            Functions.WriteExceptionLog(e.Exception, true);
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            // Set the ActionBar's support to allow the OnOptionsItemSelected method to be trigger
            Android.Support.V7.Widget.Toolbar toolbar = this.FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);

            if(toolbar != null)
            {
                SetSupportActionBar(toolbar);
            }

            base.OnPostCreate(savedInstanceState);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            // check if the current item id 
            // is equals to the back button id
            if (item.ItemId == 16908332) // xam forms nav bar back button id
            {
                // retrieve the current xamarin 
                // forms page instance
                if (Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack.Count > 0 && Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack[Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack.Count - 1] is BasePage currentpage)
                {                  
                    if (currentpage?.CustomBackButtonAction != null)
                    {
                        currentpage?.CustomBackButtonAction.Invoke();
                        return false;
                    }
                }              
            }

            return base.OnOptionsItemSelected(item);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            global::ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            NotificationClickedOn(intent);
        }

        void NotificationClickedOn(Intent intent)
        {
            if (intent.Action == "ASushiNotification" && intent.HasExtra("MessageFromSushiHangover"))
            {
                /// Do something now that you know the user clicked on the notification...

                //var notificationMessage = intent.Extras.GetString("MessageFromSushiHangover");
                //var winnerToast = Toast.MakeText(this, $"{notificationMessage}.\n\n🍣 Please send 2 BitCoins to SushiHangover to process your winning ticket! 🍣", ToastLength.Long);
                //winnerToast.SetGravity(Android.Views.GravityFlags.Center, 0, 0);
                //winnerToast.Show();

                //Xamarin.Forms.Application.Current.MainPage.Navigation.PushAsync(new NoticiasPage());
                //LoadApplication(new App(true));
            }
        }
        void SendNotifacation(string title, string[] information)
        {
            try
            {
                var intent = new Intent(BaseContext, typeof(MainActivity));
                intent.SetAction("ASushiNotification");
                intent.PutExtra("MessageFromSushiHangover", information[0]);
                var pending = PendingIntent.GetActivity(BaseContext, 0, intent, PendingIntentFlags.CancelCurrent);

                using (var notificationManager = NotificationManager.FromContext(BaseContext))
                {
                    Notification notification;
                    if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.O)
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        notification = new Notification.Builder(BaseContext)
                                                                    .SetContentTitle(title)
                                                                    .SetContentText(information[0])
                                                                    .SetStyle(new Notification.BigTextStyle().BigText(information[0]))
                                                                    .SetAutoCancel(true)
                                                                    .SetSmallIcon(Resource.Drawable.appicon)
                                                                    .SetDefaults(NotificationDefaults.All)
                                                                    .SetContentIntent(pending)
                                                                    .Build();
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                    else
                    {
                        var myUrgentChannel = BaseContext.PackageName;
                        const string channelName = "Messages from SushiHangover";

                        NotificationChannel channel;
                        channel = notificationManager.GetNotificationChannel(myUrgentChannel);
                        if (channel == null)
                        {
                            channel = new NotificationChannel(myUrgentChannel, channelName, NotificationImportance.High);
                            channel.EnableVibration(true);
                            channel.EnableLights(true);
                            channel.SetSound(
                                RingtoneManager.GetDefaultUri(RingtoneType.Notification),
                                new AudioAttributes.Builder().SetUsage(AudioUsageKind.Notification).Build()
                            );
                            channel.LockscreenVisibility = NotificationVisibility.Public;
                            notificationManager.CreateNotificationChannel(channel);
                        }
                        channel?.Dispose();

                        notification = new Notification.Builder(BaseContext)
                                            .SetChannelId(myUrgentChannel)
                                            .SetContentTitle(title)
                                            .SetContentText(information[0])
                                            .SetStyle(new Notification.BigTextStyle().BigText(information[0]))
                                            .SetAutoCancel(true)
                                            .SetSmallIcon(Resource.Drawable.appicon)
                                            .SetContentIntent(pending)
                                            .Build();
                    }
                    notificationManager.Notify(int.Parse(information[1]), notification);
                    notification.Dispose();
                }
            }
            catch { }
        }

        public IPublicClientApplication GetIdentityClient(string applicationId)
        {
            var identityClient = PublicClientApplicationBuilder.Create(applicationId)
               //.WithAuthority(AzureCloudInstance.AzurePublic, "common")
               .WithRedirectUri("msauth://com.mdsoft.MovilBusiness/M61nf%2BaC69kCXmFY1ejcX83rDNc%3D")// $"msal{applicationId}://auth")
                                                                                                     //.WithRedirectUri($"{applicationId}")
                                                                                                     //.WithRedirectUri($"msal{applicationId}://auth")
              .WithAuthority("https://login.microsoftonline.com/beb96c0c-8412-4b1f-8cc6-358ec0579b46")
               .WithParentActivityOrWindow(() => this)
               .Build();

            return identityClient;
        }
    }
}

