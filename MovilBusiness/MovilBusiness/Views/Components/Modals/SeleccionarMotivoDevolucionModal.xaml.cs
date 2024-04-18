using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.Resx;
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
	public partial class SeleccionarMotivoDevolucionModal : ContentPage
	{
        public Action<int> OnMotivoAceptado { get; set; }

		public SeleccionarMotivoDevolucionModal()
		{
			InitializeComponent ();

            comboMotivo.ItemsSource = new DS_Devoluciones().GetMotivosDevolucion();
		}

        private async void Dismiss(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync(true);
        }

        private void AceptarMotivo(object sender, EventArgs e)
        {
            var motivo = comboMotivo.SelectedItem as MotivosDevolucion;

            if(motivo == null){
                DisplayAlert(AppResource.Warning, AppResource.MustSpecifyReasonForReturn, AppResource.Aceptar);
                return;
            }

            OnMotivoAceptado?.Invoke(motivo.MotID);

            Navigation.PopModalAsync(false);
        }
    }
}