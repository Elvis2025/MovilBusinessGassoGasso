using Android.Content;
using Android.Widget;
using MovilBusiness.Controls;
using MovilBusiness.Droid.Renders;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CheckBoxs), typeof(MovilBusiness.Droid.Renders.CheckBoxRenderer))]
namespace MovilBusiness.Droid.Renders
{
    public class CheckBoxRenderer : ViewRenderer<CheckBoxs, Android.Widget.CheckBox>
    {
        private Android.Widget.CheckBox CheckBox;
        public CheckBoxRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<CheckBoxs> e)
        {
            if(e.NewElement != null)
            {
                var model = e.NewElement;
                base.OnElementChanged(e);
                CheckBox = new Android.Widget.CheckBox(Context);

                CheckBoxPropertyChenged(model, null);

                model.PropertyChanged += Model_PropertyChanged;                

                CheckBox.CheckedChange += (sender, e1) =>
                {
                    model.IsChecked = e1.IsChecked;
                };
                
                SetNativeControl(CheckBox);
            }
        }

        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(CheckBox != null)
            {
                CheckBoxPropertyChenged((CheckBoxs)sender, e.PropertyName);
            }
        }

        private void CheckBoxPropertyChenged(CheckBoxs model, string propertyName)
        {
            if(propertyName == null || propertyName == CheckBoxs.IsCheckedProperty.PropertyName)
            {
                CheckBox.Checked = model.IsChecked;
            }
        }

    }
}