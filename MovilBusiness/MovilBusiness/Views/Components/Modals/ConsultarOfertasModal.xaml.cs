using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Model;
using MovilBusiness.Resx;
using MovilBusiness.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ConsultarOfertasModal : ContentPage, INotifyPropertyChanged
	{
        public new event PropertyChangedEventHandler PropertyChanged;

        private DS_Ofertas myOfe;
        private DS_Productos myProd;

        private ObservableCollection<Ofertas> ofertas;
        public ObservableCollection<Ofertas> Ofertas { get => ofertas; set { ofertas = value; RaiseOnPropertyChanged(); } }

        private ObservableCollection<OfertasDetalle> ofertadetalles;
        public ObservableCollection<OfertasDetalle> OfertaDetalles { get => ofertadetalles; set { ofertadetalles = value;RaiseOnPropertyChanged(); } }

        private Ofertas currentoferta;
        public Ofertas CurrentOferta { get => currentoferta; set { currentoferta = value; if (value != null) { Cargar(); } RaiseOnPropertyChanged(); } }

        public bool CanSelectOferta { get; set; } = true;

        private string productoaplicar;
        public string ProductoAplicar { get => productoaplicar; set { productoaplicar = value; RaiseOnPropertyChanged(); } }

        private bool prductosvalidos;
        public bool PrductosValidos { get => prductosvalidos; set { prductosvalidos = value; RaiseOnPropertyChanged(); } }
        public bool isConsultaGeneral { get; set; } = false;
        public ICommand VerProductosDescuentoCommand { get; private set; }
        public ICommand VerClientesCommand { get; private set; }

        public ConsultarOfertasModal ()
		{
            myOfe = new DS_Ofertas();
            myProd = new DS_Productos();

            VerProductosDescuentoCommand = new Command(GoProductosDescuento);
            VerClientesCommand = new Command(GoClientesAplican);

            BindingContext = this;

            PrductosValidos = false;

            InitializeComponent ();
            
		}

        public void LoadOfertas(int proId = -1,bool isGeneral= false)
        {
            try
            {
                isConsultaGeneral = isGeneral;
                var ofecurrentpro = isGeneral ? null : PedidosViewModel.Instance().CurrentPedidoAEntregar;

                if (DS_RepresentantesParametros.GetInstance().GetOfertasConSegmento())
                {
                    Ofertas = new ObservableCollection<Ofertas>(myOfe.GetOfertasDisponiblesPorSegmento(isGeneral ? -1 : Arguments.Values.CurrentClient.CliID, isGeneral ? -1 : Arguments.Values.CurrentClient.TiNID, proId,false, ofecurrentpro, isConsultaGeneral: isGeneral));
                }
                else
                {
                    Ofertas = new ObservableCollection<Ofertas>(myOfe.GetOfertasDisponibles(isGeneral ? -1 : Arguments.Values.CurrentClient.CliID, isGeneral ? -1 : Arguments.Values.CurrentClient.TiNID,false, ofecurrentpro, isConsultaGeneral: isGeneral));
                }

                
            }catch(Exception e)
            {
                DisplayAlert(AppResource.ErrorLoadingOffers, e.Message, AppResource.Aceptar);
            }
        }

        private void Dismiss(object sender, EventArgs args)
        {
            Navigation.PopModalAsync(true);
        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Cargar()
        {
            if(CurrentOferta == null)
            {
                return;
            }

            ProductoAplicar = CurrentOferta.ProID != 0 ? myProd.GetProductCodigoAndDescByProId(CurrentOferta.ProID) : "";

            PrductosValidos = !string.IsNullOrEmpty(ProductoAplicar);

            OfertaDetalles = new ObservableCollection<OfertasDetalle>(myOfe.GetDetalleByOfeId(CurrentOferta,IsConsultaGeneral: isConsultaGeneral));
        }

        private async void GoProductosDescuento()
        {
            if (CurrentOferta == null || !CurrentOferta.IsMancomunada)
            {
                return;
            }

            var response = await DisplayActionSheet(AppResource.SelectAnOption, AppResource.Cancel, null, new string[] { AppResource.ProductToGiveUpper, AppResource.ProductThatApply });
            bool aRegalar = false;

            string grpCodigo;


            if(response == AppResource.ProductToGiveUpper)
            {
                if (string.IsNullOrEmpty(CurrentOferta.grpCodigoOferta) || CurrentOferta.grpCodigoOferta == "0")
                {
                    await DisplayAlert(AppResource.Warning, AppResource.OfferDontHaveProductToGive, AppResource.Aceptar);
                    return;
                }

                grpCodigo = CurrentOferta.grpCodigoOferta;
                aRegalar = true;
            }
            else if(response == AppResource.ProductThatApply)
            {
                if (string.IsNullOrEmpty(CurrentOferta.GrpCodigo) || CurrentOferta.GrpCodigo == "0")
                {
                    await DisplayAlert(AppResource.Warning, AppResource.OfferDontHaveProductThatApply, AppResource.Aceptar);
                    return;
                }
                grpCodigo = CurrentOferta.GrpCodigo;
            }
            else
            {
                return;
            }

            await Navigation.PushModalAsync(new DescuentoMancomunadoProductosModal(grpCodigo, aRegalar));
        }

        private async void GoClientesAplican()
        {
            if (CurrentOferta == null)
            {
                return;
            }

            string grcCodigo = CurrentOferta.GrcCodigo;
            int cliID = CurrentOferta.CliID;
            int tinID = CurrentOferta.TinID;
            

            await Navigation.PushModalAsync(new ConsultaClientesAplicaOfertaModal(cliID, grcCodigo, tinID));
        }
    }
}