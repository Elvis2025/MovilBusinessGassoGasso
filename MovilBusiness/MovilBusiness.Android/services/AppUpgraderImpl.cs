using System;
using System.Threading;
using Android.Content;
using Android.OS;
using Android.Support.V4.Content;
using Java.IO;
using Java.Net;
using MovilBusiness.Abstraction;
using MovilBusiness.Droid.services;
using Xamarin.Forms;

[assembly: Dependency(typeof(AppUpgraderImpl))]
namespace MovilBusiness.Droid.services
{
    public class AppUpgraderImpl : IApplicationUpgrader
    {
        public void DownloadFile(string address, string fileName, Action<double>progressUpdated, CancellationTokenSource cancelToken)
        {
            URL url = new URL(address);

            HttpURLConnection c = url.OpenConnection() as HttpURLConnection;
            c.RequestMethod = "GET";
            c.ReadTimeout = 60000;
            c.UseCaches = false;

            c.Connect();

            if (cancelToken.IsCancellationRequested)
            {
                return;
            }

            string path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/" + "Download" + "/";

            Java.IO.File file = new Java.IO.File(path);

            file.Mkdirs();

            Java.IO.File outputFile = new Java.IO.File(file, fileName);

            if (outputFile.Exists())
            {
                outputFile.Delete();
            }

            FileOutputStream fos = new FileOutputStream(outputFile);

            double fileLength = c.ContentLength;

            InputStream Is = new BufferedInputStream(url.OpenStream(), c.ContentLength);

            byte[] buffer = new byte[30000];

            int len1 = 0;

            double total = 0;

            while ((len1 = Is.Read(buffer)) != -1)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    fos.Flush();
                    fos.Close();
                    Is.Close();
                    return;
                }

                total += len1;

                if (fileLength > 0)
                {             
                    fos.Write(buffer, 0, len1);
                    progressUpdated?.Invoke((total / fileLength));
                }
            }
            fos.Flush();
            fos.Close();
            Is.Close();

            InstallUpdate(outputFile);
        }

        private void InstallUpdate(File file)
        {

            var uri = Android.Net.Uri.FromFile(file);
            Intent intent;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
            {
                /*
                 https://developer.android.com/training/secure-file-sharing/setup-sharing
                 https://www.c-sharpcorner.com/article/how-to-handle-android-os-fileuriexposedexception-in-android/
                 */
                uri = FileProvider.GetUriForFile(Android.App.Application.Context, Android.App.Application.Context.PackageName + ".provider", file);

                intent = new Intent(Intent.ActionInstallPackage);
                intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                intent.AddFlags(ActivityFlags.GrantWriteUriPermission);
                intent.AddFlags(ActivityFlags.GrantPersistableUriPermission);
            }
            else
            {
                intent = new Intent(Intent.ActionView);
            }

            intent.SetDataAndType(uri, "application/vnd.android.package-archive");
            MainActivity.Instance.StartActivity(intent);
            MainActivity.Instance.Finish();

        }
    }
}