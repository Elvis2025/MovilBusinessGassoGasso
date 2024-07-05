using MovilBusiness.DataAccess;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Views.Components.Modals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProductosClientesNoVendidos : ContentPage
    {
        private ProductosTemp _previousSelectedProduct;
        public List<ProductosTemp> allProducts;
        public Action<ProductosTemp> cargar;
        public Action next;
        public DS_Productos myProd;
        public ProductosClientesNoVendidos(List<ProductosTemp> productosClientesNoVendidos)
        {
            allProducts = new List<ProductosTemp>();
            myProd = new DS_Productos();
            InitializeComponent();
            cvProductsNoSaled.ItemsSource = productosClientesNoVendidos;
            var fechaString = productosClientesNoVendidos.FirstOrDefault().CliFechaActualizacion;
            DateTime.TryParse(fechaString, out DateTime date);
            txtFecha.Text = "Ultima Actualización: "+date.ToString();
        }

        private async void Atras(object sender, EventArgs e)
        {
           await Navigation.PopModalAsync();
        }
         private async void Continuar(object sender, EventArgs e)
        {
           await Navigation.PopModalAsync();
           allProducts?.Clear();
           next?.Invoke();
        }
        private void Cargar(object sender, EventArgs e)
        {
             insertProductsInTemp();
        }

        private async void insertProductsInTemp()
        {
            if(allProducts.FirstOrDefault() != null)
            {
                //var agregarProductosModal = new AgregarProductosModal(myProd);
                foreach(var currentProducts in allProducts)
                {
                    cargar?.Invoke(currentProducts);
                }

                //SqliteManager.GetInstance().InsertAll(allProducts);
                await DisplayAlert("Aviso", "Productos agregado a la lista correctamente.", "OK");
                await Navigation.PopModalAsync();

            }
            else
            {
                await DisplayAlert("Aviso", "Debes seleccionar un producto para agregarlo a la lista.", "OK");
            }
        }
        private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_previousSelectedProduct != null)
            {
                _previousSelectedProduct.IsSelected = false;
            }

            var currentSelectedProduct = e.CurrentSelection.FirstOrDefault() as ProductosTemp;
            if (currentSelectedProduct != null)
            {
                //currentSelectedProduct.IsSelected = true;

                _previousSelectedProduct = myProd.GetProductoById(currentSelectedProduct.ProID);
                _previousSelectedProduct.IsSelected = true;

                var agragarCantidad = new AgregarProductosNoVendidosModal(_previousSelectedProduct);
                agragarCantidad.OnAceeptCuantity += (cantidad) =>
                {
                    _previousSelectedProduct.Cantidad = cantidad;
                    allProducts.Add(_previousSelectedProduct);
                };

               await Navigation.PushModalAsync(agragarCantidad);
            }

        }
    }
}