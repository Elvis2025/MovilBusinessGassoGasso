using MovilBusiness.DataAccess;
using MovilBusiness.model.Internal;
using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PedidosBackOrderModal : ContentPage
	{
        public List<ProductosTemp> Productos { get; set; }

		public PedidosBackOrderModal (int cliId)
		{
            BindingContext = this;

            Productos = new DS_PedidosBackOrder().GetBackOrderByCliente(cliId);

            InitializeComponent ();
		}

        private void Dismiss(object sender, EventArgs args)
        {
            Navigation.PopModalAsync(true);
        }

        private void OnList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if(e.SelectedItem == null)
            {
                return;
            }

            list.SelectedItem = null;
        }
    }
}