using MovilBusiness.Internal;
using MovilBusiness.model.Internal.Structs.Args;
using MovilBusiness.viewmodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class RecibosFormaPagoTabPage : ContentPage
	{
		public RecibosFormaPagoTabPage ()
		{
			InitializeComponent ();
		}

        private void AgregarFormaPago(object sender, AgregarFormaPagoArgs forma)
        {
            if(BindingContext is RecibosViewModel vm)
            {
                vm.AgregarFormaPago(forma);
            }
        }

        private void OnListItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                return;
            }

            if (BindingContext is RecibosViewModel vm)
            {
                vm.FormaPagoSelected(e.SelectedItem as FormasPagoTemp);
            }

            listaFormasPago.SelectedItem = null;
        }
        
    }
}