using System;
using System.ComponentModel;
using MovilBusiness.Controls;
using MovilBusiness.iOS.renders;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(BorderlessPicker), typeof(BorderlessPickerRenderer))]
namespace MovilBusiness.iOS.renders
{
    public class BorderlessPickerRenderer : PickerRenderer
    {
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if(Control == null)
                {
                    return;
                }

                base.OnElementPropertyChanged(sender, e);
                Control.Layer.BorderWidth = 0;
                //Control.VerticalAlignment = UIControlContentVerticalAlignment.Center;
                Control.BorderStyle = UITextBorderStyle.None;
            }catch(Exception ex)
            {
                Console.Write(ex.Message);
            }
        }
    }
}