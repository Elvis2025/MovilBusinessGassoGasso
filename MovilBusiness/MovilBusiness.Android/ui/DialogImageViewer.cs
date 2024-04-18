using System;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using MovilBusiness.Utils;

namespace MovilBusiness.Droid.view.components.dialogs
{
    public class DialogImageViewer : Dialog
    {
        private LinearLayout container;
        public Action<int> OnPictureDelete { get; set; }

        public DialogImageViewer(Context context) : base(context)
        {

            //HideTitle();
            Window.RequestFeature(WindowFeatures.NoTitle);

            SetContentView(Resource.Layout.dialog_image_viewer);
            
            Window.DecorView.SetBackgroundResource(Android.Resource.Color.Transparent);

            Window.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            
            container = FindViewById<LinearLayout>(Resource.Id.container);
        }

        public void AddPicture(Bitmap pic)
        {
            var view = LayoutInflater.Inflate(Resource.Layout.image_preview_item, container, false);
            var item = view.FindViewById<ImageView>(Resource.Id.imagen);

            view.FindViewById(Resource.Id.btnEliminar).Tag = container.ChildCount;
            view.FindViewById(Resource.Id.btnEliminar).Click += (sender, args)=> 
            {
                try
                {
                    container.RemoveView(view);
                    container.RequestLayout();

                    if (container.ChildCount == 0)
                    {
                        Dismiss();
                    }

                    var v = (View)sender;

                    OnPictureDelete?.Invoke((int)v.Tag);

                }catch(Exception e)
                {
                    Functions.DisplayAlert("Error", e.Message);
                }
            };
            item.SetImageBitmap(pic);
            container.AddView(view);
        }

        public bool IsEmpty
        {
            get=>!(container != null && container.ChildCount > 0);
        }

        public int ImageCount { get => container != null ? container.ChildCount : 0; }
    }
}