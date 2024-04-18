using MovilBusiness.Controls;
using MovilBusiness.iOS.renders;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(AutoFocusEntry), typeof(AutoFocusEntryRenderer))]
namespace MovilBusiness.iOS.renders
{
    public class AutoFocusEntryRenderer : EntryRenderer
    {
        public AutoFocusEntryRenderer()
        {
        }
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (this.Control != null)
            {
               Control.BecomeFirstResponder();
            }
        }
    }
}