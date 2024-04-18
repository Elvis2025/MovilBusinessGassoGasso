using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.model.Internal;
using MovilBusiness.Resx;
using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SaldoPorAntiguedadPage : ContentPage
	{
        public List<SaldoPorAntiguedad> Documentos { get; set; }

		public SaldoPorAntiguedadPage (string monCodigo = null)
		{
            if(Arguments.Values.CurrentClient == null)
            {
                throw new Exception(AppResource.ClientIsNullWarning);
            }

            Documentos = new DS_CuentasxCobrar().GetSaldoPorAntiguedadByClienteV2(Arguments.Values.CurrentClient.CliID, monCodigo);

            BindingContext = this;

            InitializeComponent();
        }

        private async void Salir(Exception e)
        {
            await DisplayAlert("Error cargando el saldo por antiguedad", e.Message, AppResource.Aceptar);
            await Navigation.PopAsync(true);
        }

       /* private void OnListItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if(e.SelectedItem == null)
            {
                return;
            }

            list.SelectedItem = null;
        }*/

    }
}