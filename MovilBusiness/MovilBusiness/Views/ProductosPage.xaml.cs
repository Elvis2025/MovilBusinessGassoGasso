using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
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
	public partial class ProductosPage : MasterDetailPage
    {
        private bool FirstTime = true;
        private bool ModalMode;

        public Action<ProductosTemp> OnProductSelected { get; set; }

		public ProductosPage (Clientes currentclient = null, bool ModalMode = false)
		{
            this.ModalMode = ModalMode;

            if (!DS_RepresentantesParametros.GetInstance().GetParPedidosOfertasyDescuentosManuales())
            {
                Arguments.Values.CurrentModule = Modules.PRODUCTOS;
            }

            var vm = new ProductosViewModel(this, currentclient, ModalMode) { OnOptionMenuItemSelected=()=> { IsPresented = false; } };

            vm.OnRowDesignChanged += (sender, s)=> { /*SetListDataTemplate(s);*/ Functions.SetListViewItemTemplateById(ListaProductos, s, !FirstTime); vm.SaveVisualizacion(s); } ;

            BindingContext = vm;

            InitializeComponent ();

            var visualization = DS_RepresentantesParametros.GetInstance().GetFormatoVisualizacionProductos(); 

            if (visualization == -1)
            {
                visualization = DS_RepresentantesParametros.GetInstance().GetFormatoVisualizacionProductosLocal();
            }

            //SetListDataTemplate(visualization);
            Functions.SetListViewItemTemplateById(ListaProductos, visualization, !FirstTime);

            FirstTime = false;
        }

        private void EntryBuscarTextChanged(object sender, TextChangedEventArgs e)
        {
            /*if (e.NewTextValue != e.OldTextValue && !string.IsNullOrWhiteSpace(e.NewTextValue) && BindingContext is PedidosViewModel vm)
            {
                if (vm.CurrentFilter != null && vm.CurrentFilter.FilTipo == 3)
                {
                    vm.Search();
                }
            }*/
        }

        private void OnListItemSelected(object sender, ItemTappedEventArgs args)
        {
            try
            {
                if (args.Item == null)
                {
                    return;
                }

                if (OnProductSelected == null && BindingContext is ProductosViewModel vm)
                {
                    vm.OnProductSelected(args.Item as ProductosTemp);
                }
                else
                {
                    AlertSeleccionarProducto(args.Item as ProductosTemp);
                }

                ListaProductos.SelectedItem = null;

            }catch(Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }
        }

        private async void AlertSeleccionarProducto(ProductosTemp data)
        {
            var result = await DisplayAlert(AppResource.Warning, AppResource.WantSelectProductQuestion, AppResource.Yes, AppResource.No);

            if (result)
            {
                OnProductSelected?.Invoke(data);
                await Navigation.PopModalAsync(false);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if(BindingContext is ProductosViewModel vm)
            {
                vm.FirstTime = false;
                vm.GuardarProductosValidosParaDescuento();
            }
        }

        protected override bool OnBackButtonPressed()
        {
            Finish();
            return true;
        }

        private bool finishing;
        private async void Finish()
        {
            if (finishing)
            {
                return;
            }
            finishing = true;
            if (ModalMode)
            {
                await Navigation.PopModalAsync(false);
            }
            else
            {
                await Navigation.PopAsync(false);
            }
            
            finishing = false;
        }
    }
}