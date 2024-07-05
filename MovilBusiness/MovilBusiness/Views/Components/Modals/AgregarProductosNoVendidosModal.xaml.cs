using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AgregarProductosNoVendidosModal : ContentPage
    {
        public Action<double> OnAceeptCuantity;
        public AgregarProductosNoVendidosModal(ProductosTemp currentProducto)
        {
            InitializeComponent();
            lbNameProducts.Text = currentProducto.Descripcion;
        }

        private async void Aceetp(object sender, EventArgs e)
        {
            OnAceeptCuantity?.Invoke(Convert.ToDouble(txtCantidad.Text));
            await Navigation.PopModalAsync();
        }
        private async void Cancelar(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}