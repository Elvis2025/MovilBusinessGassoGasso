using MovilBusiness.Configuration;
using MovilBusiness.Enums;
using MovilBusiness.Internal;
using MovilBusiness.ViewModel;
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
	public partial class RecibosPushMoneyFormaPagos : ContentPage
	{
		public RecibosPushMoneyFormaPagos ()
		{
			InitializeComponent ();

            if(Arguments.Values.CurrentModule == Modules.COMPRAS)
            {
                layoutTotalPagar.IsVisible = false;
            }
		}

        private void OnListItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if(e.SelectedItem == null)
            {
                return;
            }

            if(BindingContext is RecibosPushMoneyViewModel vm)
            {
                vm.OnFormaPagoSelected(e.SelectedItem as FormasPagoTemp);
            }else if(Arguments.Values.CurrentModule == Modules.COMPRAS && BindingContext is PedidosViewModel vm2)
            {
                vm2.OnComprasFormaPagoSelected(e.SelectedItem as FormasPagoTemp);
            }

            listaFormasPago.SelectedItem = null;
        }
    }
}