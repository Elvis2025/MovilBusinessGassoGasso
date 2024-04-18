using MovilBusiness.Configuration;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.viewmodel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MovilBusiness.DataAccess;
using System;
using System.Globalization;
using MovilBusiness.Utils;
using Microsoft.AppCenter.Crashes;
using MovilBusiness.Views.Components.Modals;
using MovilBusiness.Resx;

namespace MovilBusiness.views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ConsultaTransaccionesDetallePage : ContentPage
	{
		public ConsultaTransaccionesDetallePage (ExpandListItem<Estados> datos, bool forFecha, int cliId = -1)
		{
            BindingContext = new ConsultaTransaccionesDetalleViewModel(this, datos, forFecha, cliId);

			InitializeComponent();

            dialogImpresion.OnCancelar = OnCancelarImpresion;
            dialogImpresion.OnAceptar = OnAceptarImpresion;
            dialogImpresion.SetCopiasImpresionByTitId(datos.TitId);
                       
            if(cliId != -1)
            {
                containerSearch1.IsVisible = false;
                containerSearch2.IsVisible = false;
            }
            else if (DS_RepresentantesParametros.GetInstance().GetParConsultaTrancacionBusquedaDiferente() == 2)
            {               
                containerSearch1.IsVisible = false;
                containerSearch2.IsVisible = true;
            }
            else
            {
                containerSearch1.IsVisible = true;
                containerSearch2.IsVisible = false; 
            }
        }

        private void OnCancelarImpresion()
        {
            ((ConsultaTransaccionesDetalleViewModel)BindingContext).ShowPrinter = false;
        }

        private void OnAceptarImpresion(int copias)
        {
            ((ConsultaTransaccionesDetalleViewModel)BindingContext).AceptarImpresion(copias);
        }

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if(e.Item == null)
            {
                return;
            }

          ((ConsultaTransaccionesDetalleViewModel)BindingContext).ShowAlertOpcionesTransaccion(e.Item as Transaccion);
            
            list.SelectedItem = null;
        }

        protected override void OnAppearing()
        {

            try
            {
                base.OnAppearing();
                if (BindingContext is ConsultaTransaccionesDetalleViewModel vm)
                {   
                    vm.LoadTransaccionesAsync();
                }
                Arguments.Values.CurrentModule = Enums.Modules.NULL;
            }
            catch(Exception e)
            {
                Crashes.TrackError(e);
                DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }                      
          
        }

        private void Picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            list.ItemsSource = ((ConsultaTransaccionesDetalleViewModel)BindingContext)
              .CurrentSectorTransferencias((((Picker)sender)
              .SelectedItem as Sectores)
              .SecCodigo);
        }
    }
}