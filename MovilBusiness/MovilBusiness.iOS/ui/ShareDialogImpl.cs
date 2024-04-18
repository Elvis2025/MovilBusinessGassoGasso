using System.Threading.Tasks;
using Foundation;
using MovilBusiness.Abstraction;
using MovilBusiness.iOS.ui;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(ShareDialogImpl))]
namespace MovilBusiness.iOS.ui
{
    public class ShareDialogImpl : IShareDialog
    {
        public async Task Show(string title, string message, string filePath)
        {
            Device.BeginInvokeOnMainThread(async () => 
            {
                var items = new NSObject[] { NSObject.FromObject(title), NSUrl.FromFilename(filePath) };
                var activityController = new UIActivityViewController(items, null);
                var vc = GetVisibleViewController();

                NSString[] excludedActivityTypes = null;

                if (excludedActivityTypes != null && excludedActivityTypes.Length > 0)
                    activityController.ExcludedActivityTypes = excludedActivityTypes;

                if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
                {
                    if (activityController.PopoverPresentationController != null)
                    {
                        activityController.PopoverPresentationController.SourceView = vc.View;
                    }
                }
                await vc.PresentViewControllerAsync(activityController, true);
            });

            await Task.Delay(1);
        }

        private UIViewController GetVisibleViewController()
        {
            var rootController = UIApplication.SharedApplication.KeyWindow.RootViewController;

            if (rootController.PresentedViewController == null)
                return rootController;

            if (rootController.PresentedViewController is UINavigationController)
            {
                return ((UINavigationController)rootController.PresentedViewController).TopViewController;
            }

            if (rootController.PresentedViewController is UITabBarController)
            {
                return ((UITabBarController)rootController.PresentedViewController).SelectedViewController;
            }

            return rootController.PresentedViewController;
        }
    }
}