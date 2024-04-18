using Android.Content;
using Android.Support.Design.Widget;
using MovilBusiness.Droid.Renders;
using MovilBusiness.Views.Components.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android.AppCompat;

[assembly: ExportRenderer(typeof(ScrollableTabPage), typeof(TabScrollablePageRenderer))]
namespace MovilBusiness.Droid.Renders
{
    public class TabScrollablePageRenderer : TabbedPageRenderer
    {
        public TabScrollablePageRenderer(Context context) : base(context)
        {
        }

        public override void OnViewAdded(Android.Views.View child)
        {
            base.OnViewAdded(child);

            if (child is TabLayout tabLayout)
            {
                tabLayout.TabMode = TabLayout.ModeScrollable;
            }
        }
    }
}