using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using MovilBusiness.Abstraction;
using MovilBusiness.Droid.utils;
using MovilBusiness.Model.Internal;
using Xamarin.Forms;

[assembly: Dependency(typeof(NotificationManagerImpl))]
namespace MovilBusiness.Droid.utils
{
    public class NotificationManagerImpl : INotificationManager
    {
        private static readonly int Notification_Id = 471;
        private NotificationManager manager;
        private NotificationCompat.Builder builder;

        public NotificationManagerImpl()
        {
            manager = (NotificationManager)MainActivity.Instance.GetSystemService(Context.NotificationService);
            builder = new NotificationCompat.Builder(MainActivity.Instance, "alertas");
        }

        public void Notify(Model.Internal.Notification data)
        {
            builder.SetContentTitle(data.Message).SetContentText(data.Title).SetOngoing(!data.IsCancelable).SetSmallIcon(Android.Resource.Drawable.StatSysDownload).SetProgress(0, 0, data.Indeterminate); ;

            if (data.SuccessIcon)
            {
                builder.SetSmallIcon(Resource.Drawable.ic_file_download_white_24dp);
            }

            manager.Notify(Notification_Id, builder.Build());

        }

        public void CancelAll()
        {
            manager.Cancel(Notification_Id);
        }
    }
}