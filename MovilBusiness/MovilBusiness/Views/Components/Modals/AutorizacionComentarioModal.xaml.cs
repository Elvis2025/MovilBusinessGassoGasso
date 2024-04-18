using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.Model;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	public partial class AutorizacionComentarioModal : ContentPage, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;

        private DS_Autorizaciones myAut;

        private bool isbusy;
        public new bool IsBusy { get => isbusy; set { isbusy = value; RaiseOnPropertyChanged(); } }

        private ObservableCollection<Autorizaciones> Autorizaciones { get; set; }

        private string pin;
        public string Pin { get => pin; set { pin = value; RaiseOnPropertyChanged(); } }

        private int CurrentTraSecuencia = -1, CurrentTitId = -1;

        public Action<int> OnAutorizacionUsed { get; set; }

        private string cxcDocumento = null;

        public List<TiposMensaje> Predeterminados { get; set; }

        private bool FromLogin;
        private bool fromDescuento;

        private string charactersinfo = "0 / ";
        public string CharactersInfo { get => charactersinfo; set { charactersinfo = value; RaiseOnPropertyChanged(); } }

        private TiposMensaje currentmensaje = null;
        public TiposMensaje CurrentMensaje { get => currentmensaje; set { currentmensaje = value; IsDetail = true; SetDetalle(); RaiseOnPropertyChanged(); } }

        private string currentdetalle;
        public string CurrentDetalle { get => currentdetalle; set { currentdetalle = value; CalculateCharacters(); RaiseOnPropertyChanged(); } }
        public int ComentLenght { get; set; } = 500;
        public bool IsDetail { get => CurrentMensaje != null && !string.IsNullOrEmpty(CurrentMensaje.MenDescripcion) && CurrentMensaje.MenDescripcion.Trim().ToUpper().Replace("S", "") == "OTRO"; set { RaiseOnPropertyChanged(); } }

        private DS_Mensajes myMen;

        public AutorizacionComentarioModal (bool fromDescuento, int traSecuencia, int titId, string cxcDocumento, bool FromLogin = false)
		{
            Predeterminados =  DS_RepresentantesParametros.GetInstance().GetParTipoMensajeSinOTROS() ? new DS_TiposMensaje().GetTipoMensajeSinOtros(titId) : new DS_TiposMensaje().GetTipoMensaje(titId);
            myAut = new DS_Autorizaciones();
            this.fromDescuento = fromDescuento;
            this.cxcDocumento = cxcDocumento;

            CurrentTraSecuencia = traSecuencia;
            CurrentTitId = titId;
            this.FromLogin = FromLogin;
            InitializeComponent();

            BindingContext = this;

            CargarAutorizaciones();

            myMen = new DS_Mensajes();
            
        }

        private void CalculateCharacters()
        {
            if (!IsDetail)
            {
                return;
            }

            if (string.IsNullOrEmpty(CurrentDetalle))
            {
                CharactersInfo = "0 / " + ComentLenght.ToString() + "";
            }

            CharactersInfo = CurrentDetalle.Length + " / " + ComentLenght.ToString() + "";
        }

        private void SetDetalle()
        {
            if (CurrentMensaje != null && !IsDetail)
            {
                CurrentDetalle = CurrentMensaje.MenDescripcion;

            }
            else
            {
                CurrentDetalle = "";
            }
        }

        private void CargarAutorizaciones()
        {
            if (CurrentTitId == 3 && fromDescuento && DS_RepresentantesParametros.GetInstance().GetParRecibosDescuentoFromAutorizaciones() && Arguments.Values.CurrentClient != null) //recibos
            {
                Autorizaciones = new ObservableCollection<Autorizaciones>(myAut.GetAutorizacionesByCxcDocumento(cxcDocumento, Arguments.Values.CurrentClient.CliID));

                return;
            }

            Autorizaciones = new ObservableCollection<Autorizaciones>(FromLogin ? myAut.GetAutorizacionesByTitIdfromLogin() : myAut.GetAutorizacionesActivas());

            comboAutorizacion.ItemsSource = Autorizaciones;

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

            if (CurrentTraSecuencia == -1 && !FromLogin)
            {
                await DisplayAlert(AppResource.Warning, AppResource.TransactionSequenceIsNotValid, AppResource.Aceptar);
                return;
            }

            if (comboAutorizacion.SelectedItem == null)
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

            if (IsDetail && string.IsNullOrWhiteSpace(CurrentDetalle))
            {
                await DisplayAlert(AppResource.Warning, AppResource.EnterCommentWarning, AppResource.Aceptar);
                return;
            }

            try
            {
                IsBusy = true;

                var task = new TaskLoader() { SqlTransactionWhenRun = true };

                int autSecuecia = autorizacion.AutSecuencia;

                await task.Execute(() =>
                {
                    myAut.MarkAutorizationAsUsed(autSecuecia, CurrentTraSecuencia,CurrentTitId, FromLogin);
                });

                CargarAutorizaciones();

                List<Recibos> listRecibo = new List<Recibos>();
                List<Ventas> listVenta = new List<Ventas>();

                if (CurrentTitId == 4)
                {
                    listVenta = SqliteManager.GetInstance().Query<Ventas>("select ifnull(Cliid,0) as Cliid from Ventas WHERE RepCodigo = ? " +
                               "and VenSecuencia = ? limit 1", new string[] { Arguments.CurrentUser.RepCodigo.ToString(), CurrentTraSecuencia.ToString() });
                }
                else 
                {
                    listRecibo = SqliteManager.GetInstance().Query<Recibos>("select ifnull(Cliid,0) as Cliid from Recibos WHERE RepCodigo = ? " +
                                "and RecSecuencia = ? limit 1", new string[] { Arguments.CurrentUser.RepCodigo.ToString(), CurrentTraSecuencia.ToString() });  
                }


                await task.Execute(() => { myMen.CrearMensaje(CurrentTitId == 4 ? listVenta[0].CliID : listRecibo[0].CliID, CurrentDetalle, Arguments.Values.CurrentVisSecuencia, CurrentTraSecuencia, CurrentTitId, 0); });

                Dismiss(null, null);

                OnAutorizacionUsed?.Invoke(autSecuecia);

                //await DisplayAlert("Exito", "Autorizacion realizada correctamente", AppResource.Aceptar);

            }
            catch (Exception e)
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