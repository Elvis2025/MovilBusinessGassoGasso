using System;
using System.Threading;
using Foundation;
using MovilBusiness.Abstraction;
using UIKit;

namespace MovilBusiness.iOS.services
{
    public class AppUpgraderImpl : IApplicationUpgrader
    {
        public void DownloadFile(string url, string fileName, Action<double> progressUpdated, CancellationTokenSource cancelToken)
        {
            UIApplication.SharedApplication.OpenUrl(new NSUrl("itms-services://?action=download-manifest&url=" + url));
        }
    }
}