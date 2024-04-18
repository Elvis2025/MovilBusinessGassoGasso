using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SeleccionarClienteVisitaVirtual : ContentPage
	{
        private Action goOperaciones;
        public SeleccionarClienteVisitaVirtual (Action goOperaciones)
		{
            this.goOperaciones = goOperaciones;

            BindingContext = new ClientesViewModel(this, false) { FiltroEstatusVisitas = FiltroEstatusVisitaClientes.TODOS };

			InitializeComponent();
		}

        private void Dismiss(object sender, EventArgs args)
        {
            Navigation.PopModalAsync(false);
        }

        private void List_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if(e.SelectedItem == null)
            {
                return;
            }

            ShowAlertCrearVisita(e.SelectedItem as Clientes);

            list.SelectedItem = null;
        }

        private async void ShowAlertCrearVisita(Clientes cliente)
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                var result = await DisplayAlert(AppResource.Warning, AppResource.WantCreateVirtualVisitQuestion, AppResource.CreateVisit, AppResource.Cancel);

                if (result)
                {
                    IsBusy = true;
                    xboxviewindicator.IsVisible = true;
                    xframeindicator.IsVisible = true;
                    xprogresindicator.IsRunning = true;

                    var task = new TaskLoader() { SqlTransactionWhenRun = true };

                    await task.Execute(() =>
                    {
                        Arguments.Values.CurrentVisSecuencia = new DS_Visitas().CrearVisita(cliente.CliID, Arguments.Values.CurrentLocation, Arguments.Values.CurrentVisSecuencia);

                        if (DS_RepresentantesParametros.GetInstance().GetOfertasConSegmento())
                        {
                            new DS_Ofertas().GuardarProductosValidosParaOfertasPorSegmento(cliente.CliID, cliente.TiNID);
                        }
                        else
                        {
                            new DS_Ofertas().GuardarProductosValidosParaOfertas(cliente.CliID, cliente.TiNID);
                        }
                    });

                    Arguments.Values.CurrentClient = cliente;                    

                    goOperaciones?.Invoke();                    

                    Dismiss(null, null);                    
                    
                    //await Navigation.PushAsync(new OperacionesPage());
                }
            }catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
            xboxviewindicator.IsVisible = false;
            xframeindicator.IsVisible = false;
            xprogresindicator.IsRunning = false;
        }
    }
}