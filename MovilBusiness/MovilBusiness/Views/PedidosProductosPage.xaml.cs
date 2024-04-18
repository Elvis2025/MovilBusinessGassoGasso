
using MovilBusiness.DataAccess;
using MovilBusiness.model.Internal;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.ViewModel;
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PedidosProductosPage : ContentPage
	{
        private bool FirstTime = true;

        public PedidosProductosPage()
        {
            InitializeComponent();
            catalogo.OnItemTapped += (data) => { ((PedidosViewModel)BindingContext).OnListItemSelected(data); };
        }

        protected override void OnBindingContextChanged()
        {
            try
            {
               // base.OnBindingContextChanged();

                if (BindingContext is PedidosViewModel vm && FirstTime)
                {
                    vm.OnRowDesignChanged += (sender, Id) =>
                    {
                        if (Id == 20)
                        {
                            ShowCatalogo();
                            vm.SaveVisualizacion(Id);
                            return;
                        }
                        catalogo.IsVisible = false;
                        ListaProductos.IsVisible = true;
                        Functions.SetListViewItemTemplateById(ListaProductos, Id);
                        vm.SaveVisualizacion(Id);
                    };

                    if (FirstTime)
                    {
                        if (DS_RepresentantesParametros.GetInstance().GetParBusquedaAvanzadaProductos())
                        {
                            ToolbarItems.Add(new ToolbarItem() { Text = AppResource.AdvancedFilter, IconImageSource = "ic_search_white_24dp", Order = ToolbarItemOrder.Primary, Command = vm.MenuItemCommand, CommandParameter = "3" });
                        }
                    }
                }

                if (FirstTime)
                {
                    var visualization = DS_RepresentantesParametros.GetInstance().GetFormatoVisualizacionProductos();
                    
                    if (visualization == -1)
                    {
                        visualization = DS_RepresentantesParametros.GetInstance().GetFormatoVisualizacionProductosLocal();
                    }

                    if (visualization == 20)
                    {
                        ShowCatalogo();
                    }
                    else
                    {
                        Functions.SetListViewItemTemplateById(ListaProductos, visualization, false);
                    }
                }

                FirstTime = false;

            }catch(Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }

            base.OnBindingContextChanged();
        }

        private void ShowCatalogo()
        {
            ListaProductos.IsVisible = false;
            catalogo.IsVisible = true;
        }

        private void OnListItemSelected(object sender, ItemTappedEventArgs e)
        {
            try
            {
                if (e.Item == null)
                {
                    return;
                }

                ((PedidosViewModel)BindingContext).OnListItemSelected(e.Item as ProductosTemp);

                ListaProductos.SelectedItem = null;

            }catch(Exception ex)
            {
                DisplayAlert(AppResource.Warning, ex.Message, AppResource.Aceptar);
            }
        }

        private async void EntryBuscarTextChanged(object sender, TextChangedEventArgs e)
        {
            if(e.NewTextValue != e.OldTextValue && !string.IsNullOrWhiteSpace(e.NewTextValue) && BindingContext is PedidosViewModel vm)
            {
                if(vm.CurrentFilter != null && vm.CurrentFilter.FilTipo == 3)
                {
                    await vm.Search(false);
                }
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if(BindingContext is PedidosViewModel vm)
            {
                vm.SubscribeToListeners();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if(BindingContext is PedidosViewModel vm)
            {
                vm.UnSubscribeFromListeners();
            }
        }
    }
}