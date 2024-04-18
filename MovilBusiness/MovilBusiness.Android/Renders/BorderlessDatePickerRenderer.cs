using Android.Content;
using MovilBusiness.Controls;
using MovilBusiness.Droid.renders;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(BorderlessDatePicker), typeof(BorderlessDatePickerRenderer))]
namespace MovilBusiness.Droid.renders
{
    public class BorderlessDatePickerRenderer : DatePickerRenderer
    {
        public BorderlessDatePickerRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.DatePicker> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement == null)
            {
                Control.Background = null;
                Control.InputType = Android.Text.InputTypes.TextFlagNoSuggestions;
                Control.Focusable = false;
                Control.FocusableInTouchMode = false;
            }
        }
    }
}