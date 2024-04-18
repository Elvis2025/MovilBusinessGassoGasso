using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MovilBusiness.Controls;
using MovilBusiness.Droid.renders;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ContentViewGradient), typeof(ContentViewGradientRenderer))]
namespace MovilBusiness.Droid.renders
{
    public class ContentViewGradientRenderer : VisualElementRenderer<ContentView>
    {
        private Color StartColor
        {
            get;
            set;
        }
        private Color EndColor
        {
            get;
            set;
        }

        public ContentViewGradientRenderer(Context context) : base(context)
        {
        }

        private void SetBackground()
        {
            /*var startColor = App.StartColor.ToAndroid();
            var endColor = App.EndColor.ToAndroid();*/

            var colors = new int[] { StartColor.ToAndroid(), EndColor.ToAndroid() };

            Background = new GradientDrawable(GradientDrawable.Orientation.LeftRight, colors);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<ContentView> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || Element == null)
            {
                return;
            }

            try
            {
                var stack = e.NewElement as ContentViewGradient;
                this.StartColor = stack.StartColor;
                this.EndColor = stack.EndColor;

                SetBackground();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ERROR:", ex.Message);
            }
        }
    }
}