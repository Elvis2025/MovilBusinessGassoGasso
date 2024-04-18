using System;
using System.IO;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using MovilBusiness.Abstraction;
using MovilBusiness.Droid.Configuration;
using Xamarin.Forms;

[assembly: Dependency(typeof(AppInfoManager))]
namespace MovilBusiness.Droid.Configuration
{
    public class AppInfoManager : IAppInfo
    {
        public string AppVersion()
        {
            try
            {
                PackageInfo pInfo = MainActivity.Instance.PackageManager.GetPackageInfo(MainActivity.Instance.PackageName, 0);
                string version = pInfo.VersionName;

                return version;
            }
            catch (PackageManager.NameNotFoundException e)
            {
                Console.Write(e.Message);
            }

            return "";
        }

        public string ProductsImagePath()
        {
            return Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/external_sd/images/";
        }

        public string DocumentsPath()
        {
            //return Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/external_sd/documentos/";

            Context context = Android.App.Application.Context;
            var filePath = context.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads);

            //var filePath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads);

            return filePath.ToString();
        }

        public string DatabasePath()
        {
            var path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/external_sd/";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        public double BatteryLevel()
        {
            try
            {
                IntentFilter ifilter = new IntentFilter(Intent.ActionBatteryChanged);
                Intent batteryStatus = MainActivity.Instance.ApplicationContext.RegisterReceiver(null, ifilter);
                int level = batteryStatus.GetIntExtra(BatteryManager.ExtraLevel, -1);
                int scale = batteryStatus.GetIntExtra(BatteryManager.ExtraScale, -1);
                float batteryPct = level / (float)scale;
                float p = batteryPct * 100;

                return Math.Round(p);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return 0;
            }

        }

        public byte[] ReadCertificate()
        {
            var input = MainActivity.Instance.Assets.Open("4307611_identity.p12");

            var memoryStream = new MemoryStream();

            // Use the .CopyTo() method and write current filestream to memory stream
            input.CopyTo(memoryStream);

            // Convert Stream To Array
            return memoryStream.ToArray();

        }
    }
}