using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.Resx;
using MovilBusiness.ViewModel;
using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PresupuestosPage : ContentPage
	{
        public PresupuestosPage(int cliId = -1, bool isFromPedidos = false, Monedas monedas= null)
		{
            BindingContext = new PresupuestosViewModel(this, cliId, isFromPedidos, monedas);
			InitializeComponent ();
		}

        private void OnListItemSelected(object sender, ItemTappedEventArgs e)
        {
            try
            {
                if (e.Item == null)
                {
                    return;
                }

                ((PresupuestosViewModel)BindingContext).OnListItemSelected(e.Item as Presupuestos);

                tableList.SelectedItem = null;

            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.Warning, ex.Message, AppResource.Aceptar);
            }
        }
    }
}