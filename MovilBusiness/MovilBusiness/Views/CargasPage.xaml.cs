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
	public partial class CargasPage : ContentPage
	{
		public CargasPage (int carSecuenciaToView = -1)
		{
            BindingContext = new CargasViewModel(this, carSecuenciaToView);
			InitializeComponent ();

            if(carSecuenciaToView != -1)
            {
                ToolbarItems.Clear();
            }

            dialogImpresion.OnCancelar = OnCancelarImpresion;
            dialogImpresion.OnAceptar = OnAceptarImpresion;
        }

        private void OnCancelarImpresion()
        {
            ((CargasViewModel)BindingContext).ShowPrinter = false;
        }

        private void OnAceptarImpresion(int copias)
        {
            ((CargasViewModel)BindingContext).AceptarImpresion(copias);
        }

        /*private void OnListItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if(e.SelectedItem == null)
            {
                return;
            }

            list.SelectedItem = null;
        }*/
    }
}