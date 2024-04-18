using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Model;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AutorizacionesModal : ContentPage, INotifyPropertyChanged
	{
        public new event PropertyChangedEventHandler PropertyChanged;

        private DS_Autorizaciones myAut;

        private bool isbusy;
        public new bool IsBusy { get => isbusy; set { isbusy = value; RaiseOnPropertyChanged(); } }

        private ObservableCollection<Autorizaciones> Autorizaciones { get; set; }

        private string pin;
        public string Pin { get => pin; set { pin = value; RaiseOnPropertyChanged(); } }

        private bool labelautorizar;
        public bool LabelAutorizar { get => labelautorizar; set { labelautorizar = value; RaiseOnPropertyChanged(); } }

        private int CurrentTraSecuencia = -1, CurrentTitId = -1;

        public Action<int> OnAutorizacionUsed { get; set; }

        private string cxcDocumento = null;

        private bool FromLogin;
        private bool fromDescuento;
       // private bool fromPedidos;

        public AutorizacionesModal(bool fromDescuento, int traSecuencia, int titId, string cxcDocumento, bool FromLogin = false, bool FromPedidos = false)
		{
            //fromPedidos = FromPedidos;
            LabelAutorizar = FromPedidos;
            myAut = new DS_Autorizaciones();
            this.fromDescuento = fromDescuento;
            this.cxcDocumento = cxcDocumento;

            CurrentTraSecuencia = traSecuencia;
            CurrentTitId = titId;
            this.FromLogin = FromLogin;
            InitializeComponent ();

            BindingContext = this;

            CargarAutorizaciones();
        }

        private void CargarAutorizaciones()
        {
            if (CurrentTitId == 3 && fromDescuento && DS_RepresentantesParametros.GetInstance().GetParRecibosDescuentoFromAutorizaciones() && Arguments.Values.CurrentClient != null) //recibos
            {
                Autorizaciones = new ObservableCollection<Autorizaciones>(myAut.GetAutorizacionesByCxcDocumento(cxcDocumento, Arguments.Values.CurrentClient.CliID));
                comboAutorizacion.ItemsSource = Autorizaciones;              
                return;
            }
            
            Autorizaciones = new ObservableCollection<Autorizaciones>(FromLogin? myAut.GetAutorizacionesByTitIdfromLogin() : myAut.GetAutorizacionesActivas());
            comboAutorizacion.ItemsSource = Autorizaciones;

            comboAutorizacion.SelectedIndex = 0;
            
        }

        private void Dismiss(object sender, EventArgs args)
        {
            Navigation.PopModalAsync(true);
        }

        private async void AceptarAutorizacion(object sender, EventArgs args)
        {
            if (IsBusy)
            {
                return;
            }

            if(CurrentTraSecuencia == -1 && !FromLogin)
            {
                await DisplayAlert(AppResource.Warning, AppResource.TransactionSequenceIsNotValid, AppResource.Aceptar);
                return;
            }

            if(comboAutorizacion.SelectedItem == null)
            {
                await DisplayAlert(AppResource.Warning, AppResource.SelectAuthorizationFromList, AppResource.Aceptar);
                return;
            }

            if (string.IsNullOrWhiteSpace(Pin))
            {
                await DisplayAlert(AppResource.Warning, AppResource.EnterPasswordWarning, AppResource.Aceptar);
                return;
            }

            var autorizacion = comboAutorizacion.SelectedItem as Autorizaciones;

            if (Pin != autorizacion.AutPin)
            {
                await DisplayAlert(AppResource.Warning, AppResource.IncorrectPassword, AppResource.Aceptar);
                return;
            }

            try
            {
                IsBusy = true;

                var task = new TaskLoader() { SqlTransactionWhenRun = true };

                int autSecuecia = autorizacion.AutSecuencia;

                await task.Execute(() =>
                {
                    myAut.MarkAutorizationAsUsed(autSecuecia, CurrentTraSecuencia, CurrentTitId, FromLogin);
                });

                CargarAutorizaciones();

                await DisplayAlert(AppResource.Success, AppResource.AuthorizationSuccessful, AppResource.Aceptar);

                Dismiss(null, null);

                OnAutorizacionUsed?.Invoke(autSecuecia);

                

            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.ErrorUpdatingAuthorization, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;

        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}