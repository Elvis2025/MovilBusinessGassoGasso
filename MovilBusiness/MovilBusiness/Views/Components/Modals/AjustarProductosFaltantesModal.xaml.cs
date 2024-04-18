using MovilBusiness.DataAccess;
using MovilBusiness.model.Internal;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AjustarProductosFaltantesModal : ContentPage, INotifyPropertyChanged
	{
        public new event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<ProductosTemp> productos;
        public ObservableCollection<ProductosTemp> Productos { get => productos; set { productos = value; RaiseOnPropertyChanged(); } }

        private ProductosTemp currentProduct;

        private bool showsetproduct;
        public bool ShowSetProduct { get => showsetproduct; set { showsetproduct = value; RaiseOnPropertyChanged(); } }

        private Action<List<ProductosTemp>> onProductosAjustados;

        public AjustarProductosFaltantesModal (Action<List<ProductosTemp>> OnProductosAjustados, int almId = -1)
		{
            onProductosAjustados = OnProductosAjustados;

            BindingContext = this;

			InitializeComponent ();

            var visualization = DS_RepresentantesParametros.GetInstance().GetFormatoVisualizacionProductos();
            

            if (visualization == -1)
            {
                visualization = DS_RepresentantesParametros.GetInstance().GetFormatoVisualizacionProductosLocal();
            }

            Functions.SetListViewItemTemplateById(ListaProductos, visualization, false);

            Productos = new ObservableCollection<ProductosTemp>(DS_RepresentantesParametros.GetInstance().GetParConteosFisicosLotesAgrupados() ? new DS_ConteosFisicos().GetProductosInInventarioConFaltantesyLotesAgrupados(DS_RepresentantesParametros.GetInstance().GetParClienteForRepresentantes(), almId) : new DS_ConteosFisicos().GetProductosInInventarioConFaltantes(DS_RepresentantesParametros.GetInstance().GetParClienteForRepresentantes(), almId));
        }

        private void Save(object sender, EventArgs e)
        {
            var list = Productos.Where(x => x.CantidadManual > 0 || x.CantidadManualDetalle > 0).ToList();

            onProductosAjustados?.Invoke(list);

            Navigation.PopModalAsync(false);
        }

        private void ListaProductos_FlowItemTapped(object sender, ItemTappedEventArgs e)
        {
            if(e.Item == null)
            {
                return;
            }

            var prod = e.Item as ProductosTemp;

            currentProduct = prod;

            if(prod.CantidadManual > 0)
            {
                editCantidad.Text = prod.CantidadManual.ToString();
            }
            else
            {
                editCantidad.Text = "";
            }

            if(prod.CantidadManualDetalle > 0)
            {
                editUnidades.Text = prod.CantidadManualDetalle.ToString();
            }
            else
            {
                editUnidades.Text = "";
            }

            lblUnidades.IsVisible = prod.IndicadorDetalle;
            editUnidades.IsVisible = prod.IndicadorDetalle;

            lblCantidadLogica.Text = AppResource.LogicalQuantityZero.Replace("0", "") + prod.InvCantidad.ToString() + (prod.InvCantidadDetalle > 0 ? "/" + prod.InvCantidadDetalle.ToString() : "");
            lblCantidadfaltante.Text = AppResource.MissingQuantityZero.Replace("0", "") + (prod.InvCantidad - prod.Cantidad).ToString();

            ShowSetProduct = true;

            ListaProductos.SelectedItem = null;
        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Dismiss(object sender, EventArgs e)
        {
            Navigation.PopModalAsync(false);
        }

        private void OcultarDialog(object sender, EventArgs e)
        {
            ShowSetProduct = false;
        }

        private void AceptarCantidad(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(editCantidad.Text) && string.IsNullOrWhiteSpace(editUnidades.Text))
            {
                DisplayAlert(AppResource.Warning, AppResource.MustSpecifyQuantity, AppResource.Aceptar);
                return;
            }

            double.TryParse(editCantidad.Text, out double cantidad);
            int.TryParse(editUnidades.Text, out int unidades);

            if(cantidad > (currentProduct.InvCantidad - currentProduct.Cantidad))
            {
                DisplayAlert(AppResource.Warning, AppResource.QuantityCannotBeGreaterThanMissing, AppResource.Aceptar);
                return;
            }

            var index = Productos.IndexOf(currentProduct);

            currentProduct.CantidadManual = cantidad;
            currentProduct.CantidadManualDetalle = unidades;

            Productos[index] = currentProduct.Copy();

            ShowSetProduct = false;

        }
    }
}