using Android.Content;
using MovilBusiness.Controls;
using MovilBusiness.Droid.Renders;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(MyDatePicker), typeof(DatePickerRenderers))]
namespace MovilBusiness.Droid.Renders
{
    public class DatePickerRenderers : DatePickerRenderer
    {
        public DatePickerRenderers(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.DatePicker> e)
        {
            base.OnElementChanged(e);

            MyDatePicker element = Element as MyDatePicker;
            if (Element.IsEnabled && !string.IsNullOrWhiteSpace(element.Placeholder))
            {                
               Control.Text = element.Placeholder;
            }
        }
    }
}