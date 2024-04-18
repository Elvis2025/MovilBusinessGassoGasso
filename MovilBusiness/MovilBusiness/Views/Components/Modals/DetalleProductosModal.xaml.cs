using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Resx;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DetalleProductosModal : ContentPage, INotifyPropertyChanged
    {
        private DS_ListaPrecios myLipPre;

        public new event PropertyChangedEventHandler PropertyChanged;

        public bool NotUseClient { get; set; }

        public bool ShowProDescripcion3 { get => DS_RepresentantesParametros.GetInstance().GetProDescripcion3ProductoPed() > 0; }

        public List<UsosMultiples> ListasPrecios { get; set; }

        public ProductosTemp CurrentProduct { get; set; }

        private UsosMultiples currentlistaprecios = null;
        public UsosMultiples CurrentListaPrecios { get => currentlistaprecios; set { currentlistaprecios = value; OnListaPreciosChanged(); RaiseOnPropertyChanged(); } }

        private ObservableCollection<Ofertas> ofertas;
        public ObservableCollection<Ofertas> Ofertas { get => ofertas; set { ofertas = value; RaiseOnPropertyChanged(); } }

        private ObservableCollection<OfertasDetalle> ofertasdetalles;
        public ObservableCollection<OfertasDetalle> OfertaDetalles { get => ofertasdetalles; set { ofertasdetalles = value; RaiseOnPropertyChanged(); } }

        private ObservableCollection<DescuentosRecargos> descuentos;
        public ObservableCollection<DescuentosRecargos> Descuentos { get => descuentos; set { descuentos = value; RaiseOnPropertyChanged(); } }

        private ObservableCollection<DescuentosRecargosDetalle> descuentodetalles;
        public ObservableCollection<DescuentosRecargosDetalle> DescuentoDetalles { get => descuentodetalles; set { descuentodetalles = value; RaiseOnPropertyChanged(); } }

        private DescuentosRecargos currentdescuento;
        public DescuentosRecargos CurrentDescuento { get => currentdescuento; set { currentdescuento = value; CargarDetalleDescuento(); RaiseOnPropertyChanged(); } }

        private int cantidaddescuentosshow;
        public int CantidadDescuentosShow { get => cantidaddescuentosshow; set { cantidaddescuentosshow = value; RaiseOnPropertyChanged(); } }

        private bool canSelectDescuento;
        public bool CanSelectDescuento { get => canSelectDescuento; set { canSelectDescuento = value; RaiseOnPropertyChanged(); } }

        private bool notIsConsultaGeneral;
        public bool NotIsConsultaGeneral { get => notIsConsultaGeneral; set { notIsConsultaGeneral = value; RaiseOnPropertyChanged(); } }

        private bool notIsConsultaGeneralByDescuento;
        public bool NotIsConsultaGeneralByDescuento { get => notIsConsultaGeneralByDescuento; set { notIsConsultaGeneralByDescuento = value; RaiseOnPropertyChanged(); } }

        private Ofertas currentferta;
        public Ofertas CurrentOferta
        {
            get => currentferta; set
            {
                currentferta = value;

                if (value != null)
                {
                    OfertaDetalles = new ObservableCollection<OfertasDetalle>(myOfe.GetDetalleByOfeId(value, CurrentProduct.ProID, IsConsultaGeneral: Arguments.Values.CurrentClient == null));
                    NotIsConsultaGeneral = (value.GrcCodigo == "0" || value.GrcCodigo == "") && !value.isConsultaGeneral;
                }

                RaiseOnPropertyChanged();
            }
        }

        private bool parOfertasYDescuentosVisualizar, parOfertasManuales;

        private DS_RepresentantesParametros myParametro;
        private DS_Ofertas myOfe;
        private DS_DescuentosRecargos myDes;
        public ICommand ChangeTabCommand { get; private set; }
        public ICommand VerClientesCommand { get; private set; }
        public ICommand VerClientesCommandByDescuento { get; private set; }
        public DetalleProductosModal(ProductosTemp currentProduct, List<UsosMultiples> listaPrecios, UsosMultiples currentListaPrecios, bool notUseClient = true)
        {
            myParametro = DS_RepresentantesParametros.GetInstance();
            myDes = new DS_DescuentosRecargos();
            myLipPre = new DS_ListaPrecios();
            myOfe = new DS_Ofertas();
            parOfertasManuales = myParametro.GetParPedidosOfertasyDescuentosManuales();
            parOfertasYDescuentosVisualizar = myParametro.GetParPedidosVisualizarOfertasYDescuentosEnProductosDetalle() && !parOfertasManuales;

            CurrentProduct = currentProduct;

            ListasPrecios = listaPrecios;

            NotUseClient = notUseClient;

            if (notUseClient)
            {
                CurrentListaPrecios = currentListaPrecios;
            }
            ChangeTabCommand = new Command(OnTabPageChange);
            VerClientesCommand = new Command(GoClientesAplican);
            VerClientesCommandByDescuento = new Command(GoClientesAplicanByDescuento);
            BindingContext = this;
            InitializeComponent();
            LoadOfertas();

            detalleOfertasView.BindingContext = this;
            descuentosView.BindingContext = this;

            tabGeneralDetalle.IsVisible = true;
            descuentosView.IsVisible = false;
            detalleOfertasView.IsVisible = false;
            indicatorOfertas.IsVisible = false;
            indicatorDescuentos.IsVisible = false;
            tabOfertasTitle.TextColor = Color.LightGray;
            tabDescuentosTitle.TextColor = Color.LightGray;
            tabGeneralTitle.TextColor = Color.White;
        }

        private void Dismiss(object sender, EventArgs args)
        {
            Navigation.PopModalAsync(true);
        }

        private void OnListaPreciosChanged()
        {
            try
            {
                var item = CurrentProduct.Copy();

                item.Precio = CurrentListaPrecios != null ? myLipPre.GetLipPrecio(CurrentListaPrecios.CodigoUso, item.ProID) : 0;

                CurrentProduct = item;

            }
            catch (Exception e)
            {

                DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }
        }

        private void OpenPresentation(object sender, EventArgs args)
        {
            try
            {
                var starter = DependencyService.Get<IOfficeManager>();

                if (starter != null && CurrentProduct != null)
                {
                    starter.OpenPowerPoint(CurrentProduct.ProCodigo);
                }

            }
            catch (Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }
        }

        private void LoadOfertas()
        {

            bool cliidIsValid = !(Arguments.Values.CurrentClient is null);

            if (myParametro.GetOfertasConSegmento())
            {
                Ofertas = new ObservableCollection<Ofertas>(myOfe.GetOfertasDisponiblesPorSegmento(cliidIsValid ? Arguments.Values.CurrentClient.CliID : 0, cliidIsValid ? Arguments.Values.CurrentClient.TiNID : 0, CurrentProduct.ProID, false, null, CurrentProduct.IndicadorOferta));
            }
            else
            {
                Ofertas = new ObservableCollection<Ofertas>(myOfe.GetOfertasDisponibles(cliidIsValid ? Arguments.Values.CurrentClient.CliID : 0, cliidIsValid ? Arguments.Values.CurrentClient.TiNID : 0, CurrentProduct.ProID, false, null, CurrentProduct.IndicadorOferta, isConsultaGeneral: Arguments.Values.CurrentClient == null));
            }

            if (Ofertas != null && Ofertas.Count > 0)
            {
                Ofertas[0].IsOfertaAcumulada = false;

                if (Ofertas[0].OfeTipo == "18")
                {
                    DateTime.TryParse(Ofertas[0].OfeFechainicio, out DateTime desde);
                    DateTime.TryParse(Ofertas[0].OfeFechaFin, out DateTime hasta);
                    var ventas = new DS_Ventas().GetDetalleByClienteyFechas(desde, hasta);
                    var ofertado = myOfe.GetDetalleOfertaById(Ofertas[0].OfeID);

                    Ofertas[0].IsOfertaAcumulada = true;
                    Ofertas[0].CantidadVentasAcumulada = (int)ventas.Where(v => v.ProID == CurrentProduct.ProID && !v.VenindicadorOferta).Sum(x => x.VenCantidad);
                    Ofertas[0].CantidadOfertasAcumulada = (int)ventas.Where(v => v.ProID == ofertado[0].ProID && v.VenindicadorOferta && v.OfeID == Ofertas[0].OfeID).Sum(x => x.VenCantidad);
                }
                CurrentOferta = Ofertas[0];
            }
            else
            {
                CurrentOferta = null;
                OfertaDetalles = null;
            }


            if (myParametro.GetParPedidosDescuentoManualGeneral() <= 0.0)
            {

                Descuentos = new ObservableCollection<DescuentosRecargos>(myDes.GetDescuentosDisponibles(cliidIsValid ? Arguments.Values.CurrentClient.TiNID : 0, cliidIsValid ? Arguments.Values.CurrentClient.CliID : 0, CurrentProduct.ProID, isConsultaGeneralByDescuento: Arguments.Values.CurrentClient == null));
            }
            else
            {
                Descuentos = null;
            }

            if (Descuentos != null && Descuentos.Count > 0)
            {
                CanSelectDescuento = (CantidadDescuentosShow = Descuentos.Count) > 1;

                CurrentDescuento = Descuentos[0];

                DescuentoDetalles = new ObservableCollection<DescuentosRecargosDetalle>(myDes.GetDetalles(CurrentDescuento.DesID));
            }
            else
            {
                Descuentos = null;
                DescuentoDetalles = null;
            }

            if (CurrentDescuento != null)
            {
                descuentosView.IsVisible = true;
            }

            if (CurrentOferta != null)
            {
                detalleOfertasView.IsVisible = true;
            }

            tabLayout.IsVisible = CurrentDescuento != null || CurrentOferta != null;

        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CargarDetalleDescuento()
        {
            try
            {
                if (CurrentDescuento == null)
                {
                    return;
                }

                NotIsConsultaGeneralByDescuento = (CurrentDescuento.GrcCodigo == "0" || CurrentDescuento.GrcCodigo == "") && !CurrentDescuento.IsConsultaGeneral;

                DescuentoDetalles = new ObservableCollection<DescuentosRecargosDetalle>(myDes.GetDetalles(CurrentDescuento.DesID, CurrentDescuento.DesMetodo == 5 ? CurrentProduct.ProID : -1));
            }
            catch (Exception e)
            {
                DisplayAlert(AppResource.ErrorLoadingDiscountDetail, e.Message, AppResource.Aceptar);
            }
        }
        private void OnTabPageChange(object pos)
        {
            if (pos == null)
            {
                return;
            }

            switch (pos.ToString())
            {
                case "0": //ofertas
                    descuentosView.IsVisible = false;
                    detalleOfertasView.IsVisible = true;
                    indicatorOfertas.IsVisible = true;
                    indicatorDescuentos.IsVisible = false;
                    indicatorGeneral.IsVisible = false;
                    tabGeneralDetalle.IsVisible = false;
                    tabOfertasTitle.TextColor = Color.White;
                    tabDescuentosTitle.TextColor = Color.LightGray;
                    tabGeneralTitle.TextColor = Color.LightGray;
                    break;
                case "1": //descuentos
                    descuentosView.IsVisible = true;
                    detalleOfertasView.IsVisible = false;
                    indicatorOfertas.IsVisible = false;
                    tabGeneralDetalle.IsVisible = false;
                    indicatorGeneral.IsVisible = false;
                    indicatorDescuentos.IsVisible = true;
                    tabOfertasTitle.TextColor = Color.LightGray;
                    tabGeneralTitle.TextColor = Color.LightGray;
                    tabDescuentosTitle.TextColor = Color.White;
                    break;
                case "2": //General
                    tabGeneralDetalle.IsVisible = true;
                    descuentosView.IsVisible = false;
                    detalleOfertasView.IsVisible = false;
                    indicatorOfertas.IsVisible = false;
                    indicatorDescuentos.IsVisible = false;
                    indicatorGeneral.IsVisible = true;
                    tabOfertasTitle.TextColor = Color.LightGray;
                    tabDescuentosTitle.TextColor = Color.LightGray;
                    tabGeneralTitle.TextColor = Color.White;
                    break;
            }
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

        private async void GoClientesAplicanByDescuento()
        {
            if (CurrentDescuento == null)
            {
                return;
            }

            string grcCodigo = CurrentDescuento.GrcCodigo;
            int cliID = CurrentDescuento.CliID;

            await Navigation.PushModalAsync(new ConsultaClientesAplicaOfertaModal(cliID, grcCodigo));
        }
    }
}