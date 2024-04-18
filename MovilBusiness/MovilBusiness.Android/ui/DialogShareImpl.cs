using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using MovilBusiness.Abstraction;
using MovilBusiness.Droid.ui;
using Xamarin.Forms;

[assembly: Dependency(typeof(DialogShareImpl))]
namespace MovilBusiness.Droid.ui
{
    public class DialogShareImpl : IShareDialog
    {
        public Task Show(string title, string message, string filePath)
        {
            var uri = Android.Net.Uri.Parse("file://" + filePath);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
            {
                /*
                 https://developer.android.com/training/secure-file-sharing/setup-sharing
                 https://www.c-sharpcorner.com/article/how-to-handle-android-os-fileuriexposedexception-in-android/
                 */
                Java.IO.File file = new Java.IO.File(filePath);
                uri = FileProvider.GetUriForFile(Android.App.Application.Context, Android.App. Application.Context.PackageName + ".provider", file);                
            }
             

            var contentType = "application/pdf";
            var intent = new Intent(Intent.ActionSend);
            intent.PutExtra(Intent.ExtraStream, uri);
            intent.PutExtra(Intent.ExtraText, string.Empty);
            intent.PutExtra(Intent.ExtraSubject, message ?? string.Empty);
            intent.SetType(contentType);
            var chooserIntent = Intent.CreateChooser(intent, title ?? string.Empty);
            chooserIntent.SetFlags(ActivityFlags.ClearTop);
            chooserIntent.SetFlags(ActivityFlags.NewTask);
            MainActivity.Instance.StartActivity(chooserIntent);

            return Task.FromResult(true);
        }
    }
}