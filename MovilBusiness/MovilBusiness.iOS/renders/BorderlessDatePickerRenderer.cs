using System;
using System.ComponentModel;
using MovilBusiness.Controls;
using MovilBusiness.iOS.renders;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(BorderlessDatePicker), typeof(BorderlessDatePickerRenderer))]
namespace MovilBusiness.iOS.renders
{
    public class BorderlessDatePickerRenderer : DatePickerRenderer
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

                Control.BorderStyle = UITextBorderStyle.None;
            }catch(Exception ex)
            {
                Console.Write(ex.Message);
            }
                   
        }
    }
}