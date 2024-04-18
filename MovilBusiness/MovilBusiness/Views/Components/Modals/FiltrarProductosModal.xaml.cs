using MovilBusiness.DataAccess;
using MovilBusiness.model;
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
    public partial class FiltrarProductosModal : ContentPage
    {
        private Action<List<Productos>> productsSelected;
        private List<Productos> productos;

        public FiltrarProductosModal(Action<List<Productos>> productsSelected)
        {
            this.productsSelected = productsSelected;

            InitializeComponent();

            list.ItemsSource = productos = new DS_Productos().GetProductsForFilter(DS_RepresentantesParametros.GetInstance().GetParCargasInventario());
        }

        private void Dismiss(object sender, EventArgs args)
        {
            Navigation.PopModalAsync(false);
        }

        private void AceptarProductos(object sender, EventArgs args)
        {
            var selected = productos.Where(x => x.IsSelected).ToList();

            productsSelected?.Invoke(selected);

            Navigation.PopModalAsync(false);
        }

        private void list_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if(e.SelectedItem == null)
            {
                return;
            }

            list.SelectedItem = null;
        }
    }
}