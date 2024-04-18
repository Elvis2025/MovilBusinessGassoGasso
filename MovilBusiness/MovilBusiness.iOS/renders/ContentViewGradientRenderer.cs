using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using MovilBusiness.Controls;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace MovilBusiness.iOS.renders
{
    public class ContentViewGradientRenderer : VisualElementRenderer<ContentView>
    {
        public override void Draw(CGRect rect)
        {
            base.Draw(rect);
            ContentViewGradient stack = (ContentViewGradient)this.Element;
            CGColor startColor = stack.StartColor.ToCGColor();
            CGColor endColor = stack.EndColor.ToCGColor();
            #region for Vertical Gradient  
            //var gradientLayer = new CAGradientLayer();     
            #endregion
            #region for Horizontal Gradient  
            var gradientLayer = new CAGradientLayer()
            {
                StartPoint = new CGPoint(0, 0.5),
                EndPoint = new CGPoint(1, 0.5)
            };
            #endregion
            gradientLayer.Frame = rect;
            gradientLayer.Colors = new CGColor[] {
                startColor,
                endColor
            };
            NativeView.Layer.InsertSublayer(gradientLayer, 0);
        }
    }
}