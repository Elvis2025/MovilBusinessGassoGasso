using FFImageLoading.Forms;
using MovilBusiness.model.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CatalogoView : CarouselView
    {
        public Action<ProductosTemp> OnItemTapped { get; set; }
        public CatalogoView()
        {
            InitializeComponent();
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            if(OnItemTapped != null && sender is Grid view && view.BindingContext != null && view.BindingContext is ProductosTemp data)
            {
                OnItemTapped.Invoke(data);
            }
        }

        private void ImageButton_Clicked(object sender, EventArgs e)
        {
            if (sender is Button view && view.BindingContext != null && view.BindingContext is ProductosTemp data)
            {
                Xamarin.Forms.Application.Current.MainPage.Navigation.PushAsync(new ShowImages((data).ProImage));
            }
        }

        private void AgregarCantidad(object sender, EventArgs args)
        {
            if(sender is Label view && view.BindingContext is ProductosTemp data)
            {
                MessagingCenter.Send("", "AgregarCantidad", data.ProID.ToString());
            }
            
        }

        private void RestarCantidad(object sender, EventArgs args)
        {
            if(sender is Label view && view.BindingContext is ProductosTemp data)
            {
                MessagingCenter.Send("", "RestarCantidad", data.ProID.ToString());
            }
            
        }

        private void ShowDetalle(object sender, EventArgs args)
        {
            if(sender is ContentView view  && view.BindingContext is ProductosTemp data)
            {
                MessagingCenter.Send("", "ShowDetalle", data.ProID.ToString());
            }
            
        }

        private void ZoomInClicked(object sender, EventArgs e)
        {
            ZoomInOut(sender, false);
        }

        private void ZoomOutClicked(object sender, EventArgs e)
        {
            ZoomInOut(sender, true);
        }

        private void ZoomInOut(object sender, bool zoomOut)
        {
            if (sender is ImageButton view && view.BindingContext is ProductosTemp data && view.Parent is StackLayout parent
                && parent.Parent is StackLayout parent2 && parent2.Parent is Grid parent3
                && parent3.Children[0] is ContentView container && container.Content is CachedImage image)
            {
                if (data.pinchArgs == null)
                {
                    data.pinchArgs = new Model.Internal.PinchArgs();
                }

                var currentScale = image.Scale;//data.pinchArgs.currentScale;

                var next = zoomOut ? currentScale - 0.5 : currentScale + 0.5;

                if (zoomOut)
                {
                    if (next < data.pinchArgs.MIN_SCALE)
                    {
                        return;
                    }
                }
                else
                {
                    if (next > data.pinchArgs.MAX_SCALE)
                    {
                        return;
                    }
                }

                // view.ScaleTo(data.pinchArgs.MIN_SCALE, 250, Easing.SpringOut);
                data.pinchArgs.currentScale = next;
                image.ScaleTo(next, 250, Easing.SpringOut);
            }
        }
    }
}