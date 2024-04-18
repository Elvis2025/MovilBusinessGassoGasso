using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Model;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using MovilBusiness.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PedidosEntregarModal : ContentPage, INotifyPropertyChanged
	{
        public new event PropertyChangedEventHandler PropertyChanged;

        private DS_Mensajes myMen;

        private DS_EntregasRepartidorTransacciones myEnt;

        EntregasRepartidorTransacciones CurrentPedido;

        public List<EntregasRepartidorTransacciones> Pedidos { get; set; }

        private Action<EntregasRepartidorTransacciones> OnPedidoSelected;

        public string motivo = "";
        public string motran = "";


        public PedidosEntregarModal (Action<EntregasRepartidorTransacciones> PedidoSelected)
		{
            OnPedidoSelected = PedidoSelected;

            myMen = new DS_Mensajes();
            myEnt = new DS_EntregasRepartidorTransacciones();
			InitializeComponent ();

            Pedidos = myEnt.GetEntregasDisponibles(Arguments.Values.CurrentClient.CliID, null, true);

            BindingContext = this;
		}

        private async void Dismiss(object sender, EventArgs args)
        {
            if (BindingContext is PedidosViewModel vm)
            {
                vm.CurrentPedidoAEntregar = null;
            }
            await Navigation.PopModalAsync(false);
        }

        private async void RechazarPedido(object sender = null, EventArgs args = null)
        {
            try
            {
                IsBusy = true;

                if (list.SelectedItem == null)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.SelectOrderToRejectWarning, AppResource.Aceptar);
                    return;
                }

                if (string.IsNullOrEmpty(motran) && string.IsNullOrEmpty(motivo))
                {
                    var result = await DisplayAlert(AppResource.Warning, AppResource.RejectOrderQuestion, AppResource.Yes, AppResource.No);

                    if (!result)
                    {
                        IsBusy = false;
                        return;
                    }
                }

                CurrentPedido = list.SelectedItem as EntregasRepartidorTransacciones;
                var parMensajeDevolucion = DS_RepresentantesParametros.GetInstance().GetParEntregasRepartidorMensajeRechazado();
                //Metodo usado especificar motivo rechazo via TipoMensaje
                if (string.IsNullOrEmpty(motran) && CurrentPedido != null && parMensajeDevolucion)
                {
                    IsBusy = false;
                    await Navigation.PushModalAsync(new EntregaRepartidorRechazadoModals(AttempSaveMessage));
                    return;
                }

                //RaiseOnPropertyChanged(nameof(IsBusy));

                var parMotivoDevolucion = DS_RepresentantesParametros.GetInstance().GetParEntregasRepartidorMotivoDevolucion();
                var parRechazadasMotivoDevolucion = DS_RepresentantesParametros.GetInstance().GetParEntregasRepartidorRechazadasMotivoDevolucion();
                //Metodo usado especificar motivo rechazo via MotivoDevolucion
                if ((parMotivoDevolucion || parRechazadasMotivoDevolucion) && string.IsNullOrEmpty(motivo) && CurrentPedido != null && !parMensajeDevolucion)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.MustSpecifyReasonForReturn, AppResource.Aceptar);
                    IsBusy = false;
                    await Navigation.PushModalAsync(new SeleccionarMotivoDevolucionModal() { OnMotivoAceptado = (motId) =>
                    {
                        motivo = motId.ToString();
                        RechazarPedido(sender, args); 
                    }});

                    return; 
                }

                RaiseOnPropertyChanged(nameof(IsBusy));

                await myEnt.RechazarEntregaPedido(CurrentPedido, 1, motivo);

                await DisplayAlert(AppResource.Warning, AppResource.DeliveryUpdatedSuccessfully, AppResource.Aceptar);

                Pedidos = myEnt.GetEntregasDisponibles(Arguments.Values.CurrentClient.CliID, null, true);

                list.ItemsSource = Pedidos;

                list.SelectedItem = null;

                motran = "";
                motivo = "";

            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
            RaiseOnPropertyChanged("IsBusy");
        }
        
        private async void AceptarPedido(object sender, EventArgs args)
        {
            if(list.SelectedItem == null)
            {
                await DisplayAlert(AppResource.Warning, AppResource.MustSelectOrderToDeliver, AppResource.Aceptar);
                return;
            }

            try
            {
                IsBusy = true;
                RaiseOnPropertyChanged("IsBusy");

                var ent = list.SelectedItem as EntregasRepartidorTransacciones;

                var hayProductosSinExistencia = false;

                await new TaskLoader().Execute(() => 
                {
                    myEnt.InsertProductInTempForVentas(ent.EnrSecuencia, ent.TraSecuencia, (int)Arguments.Values.CurrentModule, out hayProductosSinExistencia);
                });

                if (hayProductosSinExistencia)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.NoSufficientStockAvailableWillBeGiven, AppResource.Aceptar);
                }

                await Navigation.PopModalAsync(false);

                OnPedidoSelected?.Invoke(ent);
            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
            RaiseOnPropertyChanged("IsBusy");
        }

        private async void AttempSaveMessage(string Motivo)
        {
            try
            {
                int visSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Visitas");

                motran = Motivo;

                TaskLoader task = new TaskLoader() { SqlTransactionWhenRun = true };

                await task.Execute(() =>
                {
                    myMen.CrearMensaje(Arguments.Values.CurrentClient.CliID,Motivo, visSecuencia, CurrentPedido.TraSecuencia, 27, 0);
                });

                RechazarPedido();
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }        
    }
}