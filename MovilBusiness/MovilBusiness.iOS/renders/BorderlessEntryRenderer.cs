using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Foundation;
using MovilBusiness.Controls;
using MovilBusiness.iOS.renders;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(BorderlessEntry), typeof(BorderlessEntryRenderer))]
namespace MovilBusiness.iOS.renders
{
    public  class BorderlessEntryRenderer : EntryRenderer
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
                //Control.Frame = new CoreGraphics.CGRect(Control.Frame.Location, new CoreGraphics.CGSize(Control.Frame.Width, Control.Frame.Height + 20));
                Control.BorderStyle = UITextBorderStyle.None;
            }catch(Exception ex)
            {
                Console.Write(ex.Message);
            }
        }
    }
}