
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Graphics.Drawables;
using MovilBusiness.Droid.Renders;
using MovilBusiness.viewmodel;
using MovilBusiness.views;
using MovilBusiness.Views;
using MovilBusiness.Views.Components.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.Android;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(MDMap), typeof(MapViewRenderer))]
namespace MovilBusiness.Droid.Renders
{
    public class MapViewRenderer : MapRenderer//, GoogleMap.IInfoWindowAdapter
    {
        //public IntPtr Handle => throw new NotImplementedException();

        //public List<MapPin> Pins { get; set; }

        public MapViewRenderer(Context context) : base(context)
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Map> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                //NativeMap.InfoWindowClick -= OnInfoWindowClick;
            }

            if (e.NewElement != null)
            {
                var formsMap = (MDMap)e.NewElement;
                //Pins = formsMap.Pins;
            }
        }

        protected override void OnMapReady(GoogleMap map)
        {
            base.OnMapReady(map);

            //NativeMap.InfoWindowClick += OnInfoWindowClick;
            //NativeMap.SetInfoWindowAdapter(this);
        }

        protected override MarkerOptions CreateMarker(Pin pin)
        {
            if(pin.BindingContext == null)
            {
                return base.CreateMarker(pin);
            }

            var marker = new MarkerOptions();
            marker.SetPosition(new LatLng(pin.Position.Latitude, pin.Position.Longitude));
            marker.SetTitle(pin.Label);            
            marker.SetSnippet(pin.Address);

            int.TryParse(pin.BindingContext.ToString(), out int pos);

            var icon = WriteTextOnDrawable(Resource.Drawable.ic_map_pin_empty, pos > 0 ? pos.ToString() : "");

            marker.SetIcon(BitmapDescriptorFactory.FromBitmap(icon.Bitmap));
            //marker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.ic_map_pin));
            //pin.Address = "Posicion visita: " + pin.Address;

            /*if (Xamarin.Forms.Application.Current.MainPage is MapsPage p && !p.IsRutaVisita)
            {
                var icon = WriteTextOnDrawable(Resource.Drawable.ic_map_pin_empty, pin.Address);

                marker.SetIcon(BitmapDescriptorFactory.FromBitmap(icon.Bitmap));
                pin.Address = "Posicion visita: " + pin.Address;
            }
            else
            {
                marker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.ic_map_pin));
            }*/
            return marker;

        }

        private BitmapDrawable WriteTextOnDrawable(int drawableId, string text)
        {

            Bitmap bm = BitmapFactory.DecodeResource(Resources, drawableId)
                    .Copy(Bitmap.Config.Argb8888, true);

            string ff = null;
            //Typeface tf = Typeface.Create(ff, TypefaceStyle.Bold);

            Paint paint = new Paint();
            paint.SetStyle(Paint.Style.Fill);
            paint.Color = Android.Graphics.Color.White;
            //paint.SetTypeface(tf);
            paint.TextAlign = Paint.Align.Center;
            paint.TextSize = ConvertToPixels(Context, 10);
            paint.SetTypeface(Typeface.Create(Typeface.Default, TypefaceStyle.Bold));

            Rect textRect = new Rect();
            paint.GetTextBounds(text, 0, text.Length, textRect);

            Canvas canvas = new Canvas(bm);

            //If the text is bigger than the canvas , reduce the font size
            if (textRect.Width() >= (canvas.Width - 4))     //the padding on either sides is considered as 4, so as to appropriately fit in the text
                paint.TextSize = ConvertToPixels(Context, 7);        //Scaling needs to be used for different dpi's

            //Calculate the positions
            int xPos = (canvas.Width / 2);// - 2;     //-2 is for regulating the x position offset

            //"- ((paint.descent() + paint.ascent()) / 2)" is the distance from the baseline to the center.
            int yPos = (int)((canvas.Height / 2) - ((paint.Descent() + paint.Ascent()) / 2))-2;

            canvas.DrawText(text, xPos, yPos, paint);

            return new BitmapDrawable(Resources, bm);
        }



        public static int ConvertToPixels(Context context, int nDP)
        {
            float conversionScale = context.Resources.DisplayMetrics.Density;

            return (int)((nDP * conversionScale) + 0.5f);

        }

        /*void OnInfoWindowClick(object sender, GoogleMap.InfoWindowClickEventArgs e)
        {
            var customPin = GetCustomPin(e.Marker);
            if (customPin == null)
            {
                throw new Exception("Custom pin not found");
            }

            if (!string.IsNullOrWhiteSpace(customPin.Url))
            {
                var url = Android.Net.Uri.Parse(customPin.Url);
                var intent = new Intent(Intent.ActionView, url);
                intent.AddFlags(ActivityFlags.NewTask);
                Android.App.Application.Context.StartActivity(intent);
            }
        }*/


        /*Android.Views.View GoogleMap.IInfoWindowAdapter.GetInfoContents(Marker marker)
        {
            return base.GetInfoCon
        }

        Android.Views.View GoogleMap.IInfoWindowAdapter.GetInfoWindow(Marker marker)
        {
            throw new NotImplementedException();
        }*/
    }
}