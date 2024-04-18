using MovilBusiness.DataAccess;
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
    public partial class ConsultaVencimientoProductosModal : ContentPage
    {
        private List<InventariosAlmacenesLotes> Productos;

        public ConsultaVencimientoProductosModal(int proId, string proDescripcion)
        {
            Productos = new DS_InventariosAlmacenesLotes().GetInventariosAlmacenesLotes(proId);

            BindingContext = this;

            InitializeComponent();

            lblProDescripcion.Text = proDescripcion;
            list.ItemsSource = Productos;
        }

        private void list_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if(e.SelectedItem == null)
            {
                return;
            }

            list.SelectedItem = null;
        }

        private async void Dismiss(object sender, EventArgs args)
        {
            await Navigation.PopModalAsync(false);
        }
    }
}