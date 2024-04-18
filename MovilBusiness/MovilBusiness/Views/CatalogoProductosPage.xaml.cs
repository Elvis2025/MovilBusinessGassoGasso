using FFImageLoading.Forms;
using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.model.Internal;
using MovilBusiness.Resx;
using MovilBusiness.ViewModel;
using MovilBusiness.Views.Components.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CatalogoProductosPage : ContentPage
    {
        private bool _isfroohome;

        public CatalogoProductosPage(bool isfroohome = false)
        {
            _isfroohome = isfroohome;
            Init();
        }

        private void Init()
        {
            BindingContext = new CatalogoProductosViewModel(this, _isfroohome);

            Arguments.Values.CurrentModule = Enums.Modules.PRODUCTOS;

            InitializeComponent();

            if (DS_RepresentantesParametros.GetInstance().GetParCatalogoProductosVertical())
            {
                listaProductos.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
                {
                    ItemSpacing = 8
                };
            }


            var list = new List<string>();
            list.Add(AppResource.Description);
            list.Add(AppResource.Code);

            comboOrder.ItemsSource = list;
            comboOrder.SelectedIndex = 0;

            /*var pref = new PreferenceManager();

            switch (pref.GetOrientation())
            {
                case "Portrait":
                    CurrentOrientation = Enums.ScreenOrientation.PORTRAIT;
                    break;
                case "Landscape":
                    CurrentOrientation = Enums.ScreenOrientation.LANDSCAPE;
                    break;
            }*/

        }

        private bool firstTime = true;
        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is CatalogoProductosViewModel vm)
            {
                firstTime = false;

               // vm.SearchProducts();
            }

            if (!firstTime)
            {
                DependencyService.Get<IScreen>().ChangeDeviceOrientation(CurrentOrientation);
            }
        }

        private void EntryBuscarTextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue != e.OldTextValue && !string.IsNullOrWhiteSpace(e.NewTextValue) && BindingContext is CatalogoProductosViewModel vm)
            {
                if (vm.CurrentFilter != null && vm.CurrentFilter.FilTipo == 3)
                {
                    vm.SearchProducts();
                }
            }
        }

        protected override void OnDisappearing()
        {
            /*try
            {
                var pref = new PreferenceManager();

                switch (pref.GetOrientation())
                {
                    case "Portrait":
                        DependencyService.Get<IScreen>().ChangeDeviceOrientation(Enums.ScreenOrientation.PORTRAIT);
                        break;
                    case "Landscape":
                        DependencyService.Get<IScreen>().ChangeDeviceOrientation(Enums.ScreenOrientation.LANDSCAPE);
                        break;
                }

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }*/
            
            base.OnDisappearing();            
        }

        private Enums.ScreenOrientation CurrentOrientation = Enums.ScreenOrientation.PORTRAIT;


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
            if(sender is ImageButton view && view.BindingContext is ProductosTemp data && view.Parent is StackLayout parent 
                && parent.Parent is StackLayout parent2 && parent2.Parent is Grid parent3
                && parent3.Children[0] is ContentView container && container.Content is CachedImage image)
            {
                if(data.pinchArgs == null)
                {
                    data.pinchArgs = new Model.Internal.PinchArgs();
                }

                var currentScale = image.Scale;//data.pinchArgs.currentScale;

                var next = zoomOut ? currentScale - 0.5 : currentScale + 0.5;

                if (zoomOut)
                {
                    if(next < data.pinchArgs.MIN_SCALE)
                    {
                        return;
                    }
                }
                else
                {
                    if(next > data.pinchArgs.MAX_SCALE)
                    {
                        return;
                    }
                }

                // view.ScaleTo(data.pinchArgs.MIN_SCALE, 250, Easing.SpringOut);
                data.pinchArgs.currentScale = next;
                image.ScaleTo(next, 250, Easing.SpringOut);
            }
        }

        private void ToolbarItem_Clicked(object sender, EventArgs ex)
        {
            try
            {
                DependencyService.Get<IScreen>().ChangeDeviceOrientation(CurrentOrientation == Enums.ScreenOrientation.PORTRAIT ? Enums.ScreenOrientation.LANDSCAPE : Enums.ScreenOrientation.PORTRAIT);
                CurrentOrientation = CurrentOrientation == Enums.ScreenOrientation.PORTRAIT ? Enums.ScreenOrientation.LANDSCAPE : Enums.ScreenOrientation.PORTRAIT;

                (BindingContext as CatalogoProductosViewModel).IsVisibleOrden = CurrentOrientation == Enums.ScreenOrientation.PORTRAIT;

                /*gridContainer.Children.Remove(listaProductos);

                gridContainer.Children.Add(listaProductos);*/
                //(BindingContext as CatalogoProductosViewModel).Productos = null;
                (BindingContext as CatalogoProductosViewModel).SearchProducts();

            }
            catch(Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new ShowImages(((ProductosTemp)listaProductos.CurrentItem).ProImage));
        }

        private void OnProductSelected(object sender, EventArgs e)
        {
            if(sender is Grid view && view.BindingContext != null && view.BindingContext is ProductosTemp data)
            {
                ((CatalogoProductosViewModel)BindingContext).OnProductSelected(data);
            }
        }
    }
}