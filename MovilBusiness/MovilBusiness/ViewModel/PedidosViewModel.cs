
using Microsoft.AppCenter.Crashes;
using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;

using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Internal;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.model.Internal.structs;
using MovilBusiness.model.Internal.Structs.Args;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal.Structs.Args;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using MovilBusiness.views;
using MovilBusiness.Views;
using MovilBusiness.Views.Components.Modals;
using MovilBusiness.Views.Components.Popup;
using Rg.Plugins.Popup.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using ZXing.Mobile;

namespace MovilBusiness.ViewModel
{
    public class PedidosViewModel : BaseViewModel
    {
        public ICommand CommandAutorizarCondicionPago { get; private set; }
        public bool FirstTime { get; set; } = true;
        public bool Guardado { get; set; } = false;
        public bool ShowTipoPedido { get => ParCotizacionesTipos || (Arguments.Values.CurrentModule == Modules.PEDIDOS && TiposPedidos != null && TiposPedidos.Count > 0); }

        public bool MostrarCanastos { get => (Arguments.Values.CurrentModule == Modules.VENTAS && IsGonnaBeActive > 0) || (Arguments.Values.CurrentModule == Modules.PROMOCIONES && DS_RepresentantesParametros.GetInstance().GetParEntregasPromocionesUsarCanastos()); }
        private static PedidosViewModel _instance;
        public bool ShowCentrosDistribucion { get => DS_RepresentantesParametros.GetInstance().GetParPedidosCentroDistribucion(); }
        public bool UsarCondicionPago { get => Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.VENTAS || Arguments.Values.CurrentModule == Modules.COTIZACIONES; }

        private string descuentomanual;
        public string DescuentoManual { get => descuentomanual; set { descuentomanual = value; RaiseOnPropertyChanged(); } }
        public bool ShowDescuentoManual { get; private set; }
       // public bool UcarCentrosDistribucion { get => }
        public bool UseTipoPagoCompras { get; private set; }
        public bool UseTipoTrasporte { get; private set; }

        public Totales DatosPedido { get; set; }
        public bool UseCliIDMaster { get; private set; }
        public bool ShowPrioridad { get => (Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.COTIZACIONES) && Prioridades != null && Prioridades.Count > 0; }

        public bool ObtenerMonedaDesdeTipoPedido { get => myParametro.GetParPedidosObtenerMonedaDesdeTipoPedido() && Arguments.Values.CurrentModule == Modules.PEDIDOS; }
        public bool UseMultiMoneda { get => myParametro.GetParPedidoMultiMoneda() && ((Arguments.Values.CurrentModule == Modules.PEDIDOS) || (Arguments.Values.CurrentModule == Modules.VENTAS) || Arguments.Values.CurrentModule == Modules.COTIZACIONES); }
        public bool IsEnableMultiMoneda { get; set; } = true;

        public string TipoTranDescripcion { get => Arguments.Values.CurrentModule == Modules.COTIZACIONES ? AppResource.QuoteType : AppResource.OrderType; }
        public string ElijaTranDescripcion { get => Arguments.Values.CurrentModule == Modules.COTIZACIONES ?  AppResource.ChooseQuoteType : AppResource.ChooseOrderType; }
        public string PrioridadTranDescripcion { get => Arguments.Values.CurrentModule == Modules.COTIZACIONES ? AppResource.QuotePriority : AppResource.OrderPriority; }
        public bool FirtFilter { get => !DS_RepresentantesParametros.GetInstance().GetSecondFilterProduct(); }
        public bool SecondFilter { get => DS_RepresentantesParametros.GetInstance().GetSecondFilterProduct(); }

        public string BtnSearchLogo { get => CurrentFilter != null && CurrentFilter.FilTipo == 3 ? "ic_close_white" : CurrentFilter != null && CurrentFilter.FilTipo == 1 && CurrentFilter.IsCodigoBarra ? "ic_photo_camera_white_24dp" : "ic_search_white_24dp"; set { RaiseOnPropertyChanged(); } }

        public DependientesViewModel DependientesViewModel { get; private set; } //legendario

        private ObservableCollection<model.Internal.MenuItem> menusource;
        public ObservableCollection<model.Internal.MenuItem> MenuSource { get => menusource; set { menusource = value; RaiseOnPropertyChanged(); } }

        private model.Internal.MenuItem selecteditem;
        public model.Internal.MenuItem SelectedItem { get { return selecteditem; } set { selecteditem = value; RaiseOnPropertyChanged(); OnOptionItemSelected(); } }

        private ObservableCollection<ProductosTemp> productos;
        public ObservableCollection<ProductosTemp> Productos { get { return productos; } set { productos = value; RaiseOnPropertyChanged(); } }

        private ProductosTemp currentproducto;
        public ProductosTemp CurrentProducto { get => currentproducto; set { currentproducto = value; RaiseOnPropertyChanged(); } }

        public List<FiltrosDinamicos> FiltrosSource { get; private set; }
        private bool ParCotizacionesTipos;

        public bool IsDpp { get; set; }

        public bool ShowClientCreditInfo { get; private set; }

        private FiltrosDinamicos currentfilter;
        public FiltrosDinamicos CurrentFilter { get => currentfilter; set { currentfilter = value; OnFilterValueSelected(); RaiseOnPropertyChanged(); } }

        private bool enableCondicionPago = false;
        public bool EnableCondicionPago { get => enableCondicionPago; set { enableCondicionPago = value; RaiseOnPropertyChanged(); } }
        

        private bool isOnHold { get; set; }
        public bool IsOnHold { get => isOnHold; set { isOnHold = value; RaiseOnPropertyChanged(); } }
        public bool EnableProntoPago { get; private set; } = true;
        public bool IsPushMoneyRotacion { get; set; }

        private List<KV> secondfiltersource;
        public List<KV> SecondFiltroSource { get { return secondfiltersource; } set { secondfiltersource = value; RaiseOnPropertyChanged(); } }
        private KV currentsecondfiltro;
        public KV CurrentSecondFiltro
        {
            get { return currentsecondfiltro; }
            set
            {
                try
                {
                    currentsecondfiltro = value;

                    RaiseOnPropertyChanged();

                    if (value != null)
                    {
                        if (value.Value ==  "(Seleccione)" || value.Value == AppResource.Select)
                        {
                            return;
                        }

                        if (ParDevolucionesProductosFacturas && FacturaId == "-1")
                        {
                            return;
                        }

                        SearchUnAsync(FirstTime && HasProductsInTemp());
                    }

                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                }
            }
        }
        public ICommand SearchCommand { get; private set; }
        public ICommand BtnSearchCommand { get; private set; }
        public ICommand MenuItemCommand { get; private set; }
        public ICommand AgregarCantidad { get; private set; }
        public ICommand RestarCantidad { get; private set; }
        public ICommand AddFormaPagoCommand { get; private set; }

        private string venCantidadCanastos;
        public string VenCantidadCanastos { get => venCantidadCanastos; set { venCantidadCanastos = value; RaiseOnPropertyChanged(); } }

        public string LastValueSearch;

        private string searchvalue;
        public string SearchValue { get { return searchvalue; } set { searchvalue = value; if (DS_RepresentantesParametros.GetInstance().GetBuscarProductosAlEscribir()) { SearchOrClean(LastValueSearch); } RaiseOnPropertyChanged(); } }

        public bool ShowSecondFilter { get { return CurrentFilter != null && CurrentFilter.FilTipo == 2; } set { RaiseOnPropertyChanged(); } }
        public bool IsPedidos => Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.COTIZACIONES;
        public bool IsTraspaso => Arguments.Values.CurrentModule == Modules.TRASPASOS;

        public bool IsHora => (Arguments.Values.CurrentModule == Modules.PEDIDOS) && (DS_RepresentantesParametros.GetInstance().GetHabilitarHoraenEntrega());
        public bool IsEditing { get; set; }
        public bool FromCopy { get; set; }

        public bool ParOfertasManuales = false;

        private string currentcintillo;
        public string CurrentCintillo { get => currentcintillo; set { currentcintillo = value; RaiseOnPropertyChanged(); } }

        private string currentdevdocumento;
        public string CurrentDevDocumento { get => currentdevdocumento; set { currentdevdocumento = value; RaiseOnPropertyChanged(); } }

        private DateTime FechaValid = DateTime.Now;

        private DateTime currentfechaentrega = DateTime.Now;
        public DateTime CurrentFechaEntrega { get => currentfechaentrega; set { currentfechaentrega = value; ValidarFechaEntrega(); RaiseOnPropertyChanged(); } }

        private TimeSpan currenttimeentrega = new TimeSpan(00, 00, 00);
        public TimeSpan CurrentTimeEntrega { get => currenttimeentrega; set { currenttimeentrega = value; RaiseOnPropertyChanged(); } }

        private string currenttipotraspaso;
        public string CurrentTipoTraspaso { get => currenttipotraspaso; set { currenttipotraspaso = value; IsEntregandoTraspaso = !string.IsNullOrEmpty(value) && value.Equals("Entregar productos"); } }

        private bool IsEntregandoTraspaso;

        private bool pickerEnabled = true;
        public bool PickerEnabled { get => pickerEnabled; set { pickerEnabled = value; RaiseOnPropertyChanged(); } }

        public bool IsNumOrdenVisible => Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.COTIZACIONES || DS_RepresentantesParametros.GetInstance().GetNumOrdenObligatorio();

        private ObservableCollection<CondicionesPago> condicionespago;
        public ObservableCollection<CondicionesPago> CondicionesPago { get => condicionespago; private set { condicionespago = value; RaiseOnPropertyChanged(); } }

        private CondicionesPago currentcondicionpago = null;
        public CondicionesPago CurrentCondicionPago { get => currentcondicionpago; set { currentcondicionpago = value; /*SetChageCondicionPago(value, (value.ConID != Arguments.Values.CurrentClient.ConID && value.ConID != myParametro.GetParVenConIDCandado() ? false : true)); */RaiseOnPropertyChanged(); } }

        private string pedordencompra;
        public string PedOrdenCompra { get => pedordencompra; set { pedordencompra = value; RaiseOnPropertyChanged(); } }
 
        private DS_Productos myProd;
        private DS_Devoluciones myDev;
        private DS_UsosMultiples myUso;
        private DS_GrupoProductos myGrp;
        private DS_Clientes myCli;
        private DS_Monedas myMon;
        private DS_Inventarios myInv;
        private DS_CentrosDistribucion myCenDis;
        private DS_CondicionesPago myConPago;
        private DS_Cuadres myCua;
        private DS_Pedidos myPed;
        private DS_Cotizaciones myCot;
        private DS_CuentasxCobrar mycxc;
        private DS_InventariosFisicos myInvF;

        private DS_EntregasRepartidorTransacciones myEnt;

        public event EventHandler<int> OnRowDesignChanged;

        private Totales resumen;
        public Totales Resumen { get => resumen; set { resumen = value; RaiseOnPropertyChanged(); } }

        private int NumeroTransaccion = 1;

        //parametros devolucion
        public bool UseMotivoDevolucion { get; set; } = false;
        public bool UseCondicionDevolucion { get; set; } = false;
        public bool UseAccion { get; set; } = false;
        public bool UseCintillo { get; set; } = false;
        public bool UseDocumento { get; set; } = false;

        public List<MotivosDevolucion> MotivosDevolucion { get; set; }
        public List<UsosMultiples> CondicionDevolucion { get; set; }

        /// <summary>
        /// muestra centro de distribucion
        /// </summary>
        /// 
        public List<CentrosDistribucion> CentrosDistribucion { get; set; }

        public List<UsosMultiples> AccionesDevolucion { get; set; }
        public List<UsosMultiples> InvAreas { get; set; }
        public List<UsosMultiples> TiposPedidos { get; private set; }
        public List<UsosMultiples> TiposPago { get; private set; }
        public List<UsosMultiples> TipoTrasporte { get; private set; }
        public List<UsosMultiples> Prioridades { get; private set; }

        private ObservableCollection<ClientesDirecciones> clientesdirecciones;
        public ObservableCollection<ClientesDirecciones> Clientedirecciones { get => clientesdirecciones; set { clientesdirecciones = value; RaiseOnPropertyChanged(); } }

        public List<Representantes> RepresentantesTraspasos { get; set; }

        private Representantes currentreptraspaso;
        public Representantes CurrentRepTraspaso { get => currentreptraspaso; set { currentreptraspaso = value; RaiseOnPropertyChanged(); } }

        public List<Monedas> MonedasSource { get; set; }

        private MotivosDevolucion currentmotivodevolucion = null;
        public MotivosDevolucion CurrentMotivoDevolucion { get => currentmotivodevolucion; set { if (currentmotivodevolucion != null && !IsMotivoValido(value)) { return; } currentmotivodevolucion = value; RaiseOnPropertyChanged(); } }
        
        private UsosMultiples currentcondiciondevolucion = null;
        public UsosMultiples CurrentCondicionDevolucion { get => currentcondiciondevolucion; set {  currentcondiciondevolucion = value; RaiseOnPropertyChanged(); } }

        /// <summary>
        /// centro de distribucion
        /// </summary>
        /// 
        private CentrosDistribucion currentCentrosDistribucion;

        public CentrosDistribucion CurrentCentrosDistribucion  { get => currentCentrosDistribucion; set { currentCentrosDistribucion = value; RaiseOnPropertyChanged(); } }

        private Clientes clientemaster;

        public Clientes ClienteMaster { get => clientemaster; set { clientemaster = value; RaiseOnPropertyChanged(); } }


        private UsosMultiples currentacciondevolucion = null;
        public UsosMultiples CurrentAccionDevolucion { get => currentacciondevolucion; set { currentacciondevolucion = value; RaiseOnPropertyChanged(); } }

        private UsosMultiples currentinvarea = null;
        public UsosMultiples CurrentInvArea { get => currentinvarea; set { currentinvarea = value; RaiseOnPropertyChanged(); } }

        private UsosMultiples currenttipopedido;
        public UsosMultiples CurrentTipoPedido { get => currenttipopedido; set { currenttipopedido = value; CambiarMoneda(); RaiseOnPropertyChanged(); } }

        private UsosMultiples currenttipopago;
        public UsosMultiples CurrentTipoPago { get => currenttipopago; set { currenttipopago = value; RaiseOnPropertyChanged(); } }
        
        private UsosMultiples currenttipotrasporte;
        public UsosMultiples CurrentTipoTrasporte { get => currenttipotrasporte; set { currenttipotrasporte = value; RaiseOnPropertyChanged(); } }

        private UsosMultiples currentprioridad;
        public UsosMultiples CurrentPrioridad { get => currentprioridad; set { currentprioridad = value; RaiseOnPropertyChanged(); } }

        private ClientesCreditoData currentclientdata;
        public ClientesCreditoData CurrentClientData { get => currentclientdata; private set { currentclientdata = value; RaiseOnPropertyChanged(); } }

        public ClientesDirecciones ClientesDireccionesDetalle { get; set; }

        bool NotInitDireccion = false;

        private ClientesDirecciones cldDirTipo = null;
        public ClientesDirecciones CurrentCldDirTipo { get => cldDirTipo; set { cldDirTipo = value; if (!NotInitDireccion) { PushModalAsync(new DetalleDeDireccionModal(this)); } RaiseOnPropertyChanged(); } }

        private Monedas currentmoneda = null;
        public Monedas CurrentMoneda { get => currentmoneda; set { currentmoneda = value; ActualizarPreciosNuevaMoneda(); RaiseOnPropertyChanged(); } }
        public string MonedaActual { get; set; }
        
        public bool UseInvArea { get; set; } = false;
        public bool UsePedDir { get; set; } = false;
        public bool UsePedDirForNueva => Arguments.Values.CurrentModule == Modules.PEDIDOS? myParametro.GetParPedidosDirrecion() == 2 :
                myParametro.GetParCotizacionDirrecion() == 2;
        private AgregarProductoDevolucionModal dialogDevProducto;
        private ConsultarOfertasModal dialogConsultaOfertas;
        private ConsultaDescuentosModal dialogConsultaDescuentos;
        private AgregarProductosModal dialogAgregarProducto;
        private BusquedaAvanzadaModal dialogBusquedaAvanzada;
        public EntregasRepartidorTransacciones CurrentPedidoAEntregar;
        private string repAuditor;
        private int AlmID;

        private int EditedTranSecuencia = -1;
        private string FacturaId = "-1";
        private bool ParDevolucionesProductosFacturas;
        private int ParCliIDForRep;
        private IScreen screen;
        public bool ShowAlmacen { get; set; }

        public bool ShowIndicadorRevision { get => myParametro.GetParPedIndicadorRevision() && Arguments.Values.CurrentModule == Modules.PEDIDOS; }

        public Almacenes CurrentAlmacenForConteo { get; set; } = null;

        public Action OnOptionMenuItemSelected { get; set; }

        private List<FiltrosDinamicos> FiltroOrdenarLista;
        public bool noSetear = false;

        private int IsGonnaBeActive;

        private bool IsVenAutlImcre;

        public bool UseMultiEntrega { get; set; }

        public PedidosViewModel(Page page, int EditedTranSecuencia = -1, bool isEditing = false, bool fromCopy = false, string repAuditor = null, int almId = -1, bool  isFromCot2Ven = false, int conId = -1) : base(page)
        {
            this.repAuditor = repAuditor;
            AlmID = almId;
            _instance = this;
            IsGonnaBeActive = myParametro.GetParCaracteristicaCanasto();

            IsVenAutlImcre = myParametro.GetParVENAUTLIMCRE();

            ParDevolucionesProductosFacturas = myParametro.GetParDevolucionesProductosFacturas();
            ///Clientes direcciones en pedidos
            UsePedDir = (Arguments.Values.CurrentModule == Modules.PEDIDOS && myParametro.GetParPedidosDirrecion() > 0)
                || (Arguments.Values.CurrentModule == Modules.COTIZACIONES && myParametro.GetParCotizacionDirrecion() > 0);

            UseTipoTrasporte = Arguments.Values.CurrentModule == Modules.PEDIDOS && myParametro.GetParTipoTrasporte();

            UseCliIDMaster = Arguments.Values.CurrentModule == Modules.PEDIDOS && myParametro.GetParCliIdMaster();

            screen = DependencyService.Get<IScreen>();

            ShowDescuentoManual = myParametro.GetParPedidosDescuentoManualGeneral() > 0.0;

            this.EditedTranSecuencia = EditedTranSecuencia;
            IsEditing = isEditing;
            FromCopy = fromCopy;
            EnableCondicionPago = true;//Deshabilita o habilita la forma de pago en el tab de configuraciones de pedidos

            CommandAutorizarCondicionPago = new Command(AutorizarCondicionPago);

            myProd = new DS_Productos();
            myUso = new DS_UsosMultiples();
            myGrp = new DS_GrupoProductos();
            myCli = new DS_Clientes();
            myMon = new DS_Monedas();
            myEnt = new DS_EntregasRepartidorTransacciones();
            myInv = new DS_Inventarios();
            myCenDis = new DS_CentrosDistribucion();
            myConPago = new DS_CondicionesPago();
            myCua = new DS_Cuadres();
            myPed = new DS_Pedidos();
            myCot = new DS_Cotizaciones();
            mycxc = new DS_CuentasxCobrar();
            myInvF = new DS_InventariosFisicos();

            int porciento = myParametro.GetParCantidadDividirPrecioDevolucion();
            if (!Arguments.Values.IsUpdatePrecioForDev && porciento != -1 && Arguments.Values.CurrentModule == Modules.DEVOLUCIONES)
            {
                new DS_ListaPrecios().UpdatePrecioForDev(" / " + porciento.ToString(), 1, 0);
                Arguments.Values.IsUpdatePrecioForDev = true;
            }

            if (Arguments.Values.CurrentModule == Modules.PEDIDOS && porciento != -1)
            {
                new DS_ListaPrecios().UpdatePrecioForDev(" * " + porciento.ToString(), 0, 1);
                Arguments.Values.IsUpdatePrecioForDev = false;
            }

            ShowClientCreditInfo = myParametro.GetParPedidosMostrarInformacionCrediticiaCliente();

            ShowAlmacen = myParametro.GetParConteoFisicoPorAlmacen() && Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS;
            
            if (ShowAlmacen)
            {
                CurrentAlmacenForConteo = new DS_Almacenes().GetAlmacenById(almId);
            }

            if (UseTipoTrasporte)
            {
                TipoTrasporte = myUso.GetTiposTransporte();
            }

            if (IsTraspaso)
            {
                RepresentantesTraspasos = new DS_Representantes().GetAllRepresentantes(-1, true, Arguments.CurrentUser.RepCodigo);
            }

            /*if ((!isEditing && !fromCopy) && !ParDevolucionesProductosFacturako.;s && Arguments.Values.CurrentModule != Modules.PEDIDOS)
            {
                myProd.ClearTemp((int));
            }*/

            SearchCommand = new Command(async () => { await Search(false); });
            BtnSearchCommand = new Command(() => { SearchOrClean(); });
            MenuItemCommand = new Command(OnMenuItemSelected);
            AgregarCantidad = new Command((s) => { AumentarCantidadProducto(s.ToString(), false); });
            RestarCantidad = new Command((s) => { AumentarCantidadProducto(s.ToString(), true); });

            FiltroOrdenarLista = new DS_FiltrosDinamicos().GetFiltroOrdenarProductos();

            ParOfertasManuales = myParametro.GetParPedidosOfertasyDescuentosManuales();
            ParCliIDForRep = myParametro.GetParClienteForRepresentantes();

            BindDrawerMenu();

            Productos = new ObservableCollection<ProductosTemp>();
            myDev = new DS_Devoluciones();

            if (Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS && myCli.VarificarSiExisteCliente(ParCliIDForRep))
            {
                Arguments.Values.CurrentClient = myCli.GetClienteById(ParCliIDForRep);

                if (Arguments.CurrentUser.TipoRelacionClientes == 2)
                {
                    var detalle = myCli.GetDetalleFromCliente(Arguments.Values.CurrentClient.CliID, Arguments.CurrentUser.RepCodigo.Trim());

                    if (detalle != null)
                    {
                        if (!string.IsNullOrWhiteSpace(detalle.LipCodigo))
                        {
                            Arguments.Values.CurrentClient.LiPCodigo = detalle.LipCodigo;
                        }
                        if (detalle.ConID > 0)
                            Arguments.Values.CurrentClient.ConID = detalle.ConID;
                    }
                }
            }

            var rawInv = myParametro.GetParInventarioFisicoArea();
            if (UsePedDir)
            {
                Clientedirecciones = new ObservableCollection<ClientesDirecciones>(myCli.getDirPedCli(Arguments.Values.CurrentClient.CliID));

                string pedtipodir = myPed.GetBySecuenciaForCldDirTipo(EditedTranSecuencia, false)?.CldDirTipo;

                if (isEditing && !string.IsNullOrEmpty(pedtipodir))
                {
                    NotInitDireccion = true;
                    CurrentCldDirTipo = Clientedirecciones.Where(c => c.CldDirTipo?.ToUpper() == pedtipodir.ToUpper()).FirstOrDefault();
                    NotInitDireccion = false;
                }
            }


            if (UseMultiMoneda)
            {
                MonedasSource = myMon.GetMonedasSelect();
                //CurrentMoneda = myMon.GetMoneda(Arguments.Values.CurrentClient.MonCodigo);
              
                 
                if (ObtenerMonedaDesdeTipoPedido)
                {
                    IsEnableMultiMoneda = false;
                    if (!string.IsNullOrWhiteSpace(Arguments.Values.CurrentClient.MonCodigo))
                    {
                        var MonedaCliente = MonedasSource.FirstOrDefault(c => c.MonCodigo == Arguments.Values.CurrentClient.MonCodigo);
                        if (MonedaCliente != null)
                            MonedaActual = AppResource.CustomerCurrencyLabel + MonedaCliente.MonNombre;
                    }
                }
                else
                {
                    IsEnableMultiMoneda = true;
                    if (!string.IsNullOrWhiteSpace(Arguments.Values.CurrentClient.MonCodigo))
                    {
                        CurrentMoneda = MonedasSource.FirstOrDefault(c => c.MonCodigo == Arguments.Values.CurrentClient.MonCodigo);
                        if (CurrentMoneda != null)
                        {
                            MonedaActual = AppResource.CustomerCurrencyLabel + CurrentMoneda.MonNombre;
                        }
                    }
                }
            }

            UseInvArea = Arguments.Values.CurrentModule == Modules.INVFISICO && !string.IsNullOrWhiteSpace(rawInv) && rawInv.ToUpper().Trim() == "C";

            if(Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS)
            {
                var colInvArea = myParametro.GetParColocacionProductosCapturarArea();

                UseInvArea = !string.IsNullOrWhiteSpace(colInvArea) && colInvArea.ToUpper().Trim() == "C";
            }

            if (UseInvArea)
            {
                InvAreas = myUso.GetInvAreas();
            }

            CondicionesPago = myConPago.GetAllCondicionesPago();

            SetNumeroTransaccion();

            SetResumenTotales();

            BindFiltros();

            ConfigDevArgs();

            if(Arguments.Values.CurrentModule == Modules.CAMBIOSMERCANCIA)
            {
                UseMotivoDevolucion = myParametro.GetParCambiosUsarMotivos() == 1;

                if (UseMotivoDevolucion)
                {
                    MotivosDevolucion = myDev.GetMotivosDevolucion();
                }
            }

            var parPrioridad = myParametro.GetParModulosPrioridad();

            
            if (Arguments.Values.CurrentModule == Modules.COTIZACIONES)
            {
                ParCotizacionesTipos = myParametro.GetParCotizacionesTipos();
                var CondicionP = myConPago.GetAllCondicionesPago();
                var MonedaP = myMon.GetMonedas();

                if (parPrioridad != null && parPrioridad.Length > 0 && parPrioridad[0] != null && parPrioridad[0].Equals("1"))
                {
                    Prioridades = myUso.GetCotizacionesPrioridades();

                    if(parPrioridad.Count() > 1 && parPrioridad[1] != null)
                    {
                        CurrentPrioridad = Prioridades.Where(x => x.CodigoUso.Trim().ToUpper().Equals(parPrioridad[1].Trim().ToUpper())).FirstOrDefault();
                    }
                    else
                    {
                        CurrentPrioridad = Prioridades.FirstOrDefault();
                    }
                }

                if (IsEditing && EditedTranSecuencia != -1)
                {
                    var cotizacionEditada = myCot.GetBySecuencia(EditedTranSecuencia, false);

                    if (cotizacionEditada != null)
                    {
                        var condicion = CondicionP.Where(x => x.ConID == cotizacionEditada.ConID).FirstOrDefault();
                        var moneda = MonedaP.Where(x => x.MonCodigo == cotizacionEditada.MonCodigo).FirstOrDefault();
                        CurrentCondicionPago = condicion;
                        CurrentMoneda = moneda;

                    }
                }

                if (ParCotizacionesTipos)
                {
                    TiposPedidos = myUso.GetTiposCotizaciones();
                }
            }
            else if (Arguments.Values.CurrentModule == Modules.PEDIDOS)
            {
                TiposPedidos = myUso.GetTiposPedidos();
                var CondicionP = myConPago.GetAllCondicionesPago();
                var MonedaP = myMon.GetMonedas();

                if (parPrioridad != null && parPrioridad.Length > 0 && parPrioridad[0] != null && parPrioridad[0].Equals("1"))
                {
                    Prioridades = myUso.GetPedidosPrioridades();

                    if (parPrioridad[1] != null)
                    {
                        CurrentPrioridad = Prioridades.Where(x => x.CodigoUso.Trim().ToUpper().Equals(parPrioridad[1].Trim().ToUpper())).FirstOrDefault();
                    }
                    else
                    {
                        CurrentPrioridad = Prioridades.FirstOrDefault();
                    }
                }
                if (DS_RepresentantesParametros.GetInstance().GetDeshabilitarConfig() && Arguments.Values.CurrentClient.CliLimiteCredito == 0)
                    EnableCondicionPago = false;

                if (condicionPagoAutorizada)
                {
                    EnableCondicionPago = true;
                }
                if (IsEditing && EditedTranSecuencia != -1)
                {
                    var PedidoAEditar = myPed.GetBySecuencia(EditedTranSecuencia, false);
                    IsOnHold = PedidoAEditar.PedIndicadorRevision > 0;
                    if (TiposPedidos != null && TiposPedidos.Count > 0)
                    {
                        if (PedidoAEditar != null)
                        {
                            var condicion = CondicionP.Where(x => x.ConID == PedidoAEditar.ConID).FirstOrDefault();
                            var moneda = MonedaP.Where(x => x.MonCodigo == PedidoAEditar.MonCodigo).FirstOrDefault();
                            CurrentCondicionPago = condicion;
                            CurrentMoneda = moneda;

                            if (PedidoAEditar.PedTipoPedido > -1)
                            {
                                CurrentTipoPedido = TiposPedidos.FirstOrDefault(t => t.CodigoUso == PedidoAEditar.PedTipoPedido.ToString());
                            }
                        }
                    }                    
                }
                else 
                {
                    int tipoPedidoPorDefecto = myParametro.GetParTipoPedidoDefault();
                    if (TiposPedidos != null && TiposPedidos.Count > 0 && tipoPedidoPorDefecto > -1)
                    {
                        CurrentTipoPedido = TiposPedidos.FirstOrDefault(t => t.CodigoUso == tipoPedidoPorDefecto.ToString());
                    }
                }
            }
            else if (Arguments.Values.CurrentModule == Modules.COMPRAS)
            {
                UseTipoPagoCompras = myParametro.GetParComprasTipoPago();

                FormasPagoAgregadas = new ObservableCollection<FormasPagoTemp>();

                if (EditedTranSecuencia != -1 && IsEditing)
                {
                    FormasPagoAgregadas = new ObservableCollection<FormasPagoTemp>(new DS_Compras().GetFormasPago(EditedTranSecuencia));
                }

                AddFormaPagoCommand = new Command(ShowAlertAddComprasFormaPago);

                if (UseTipoPagoCompras)
                {
                    TiposPago = myUso.GetTiposPagoCompras();
                }

                ClientesDependientes dep = null;

                if (isEditing)
                {
                    var compra = new DS_Compras().GetBySecuencia(EditedTranSecuencia, false);

                    if (compra != null)
                    {
                        dep = myCli.GetDependienteByCedula(compra.CLDCedula, Arguments.Values.CurrentClient.CliID);
                    }
                }

                DependientesViewModel = new DependientesViewModel(page, myUso, dep);
            }

            //centrosDistribucion          
            if (Arguments.Values.CurrentModule == Modules.PEDIDOS && ShowCentrosDistribucion)
            {
                List<CentrosDistribucion> listCentros = new List<CentrosDistribucion>();
                var centrosDistribucion = myCenDis.GetCentrosDistribucions(Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.SecCodigo : "");
                var validos = myParametro.GetParFiltroCentroDistribucion();

                foreach (var centro in centrosDistribucion)
                {
                    if (validos == null ||  validos.Length == 0 || validos.Contains(centro.CedCodigo.ToString()))
                    {
                        listCentros.Add(centro);
                    }
                }
                CentrosDistribucion = listCentros;
                //CentrosDistribucion = myCenDis.GetCentrosDistribucions(Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.SecCodigo : "");

                if (Arguments.Values.CurrentSector != null)
                {                  
                    CurrentCentrosDistribucion = CentrosDistribucion.FirstOrDefault(c => c.CedCodigo.ToString() == myCli.GetClienteCedCodigoFromDetalleBySector(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentSector.SecCodigo));
                }
            }

            //Verificación si contiene parámetro de días de entrega
            if(myParametro.GetParPedidosDiasEntrega() > 0 && Arguments.Values.CurrentModule == Modules.PEDIDOS)
            {
                CurrentFechaEntrega = DateTime.Now.AddDays(myParametro.GetParPedidosDiasEntrega());
                PickerEnabled = false;
            }

            //Verificación si contiene parámetro de días de entrega
            if (myParametro.GetParPedidosDiasEntregaSinFeriado() > 0 && Arguments.Values.CurrentModule == Modules.PEDIDOS)
            {
                var FechaEntrega = DateTime.Now.AddDays(myParametro.GetParPedidosDiasEntregaSinFeriado());
                var DiaFeriado = myPed.ExistsDiaFeriado(FechaEntrega.ToString("yyyy-MM-dd"));
                var day = Convert.ToInt32(FechaEntrega.DayOfWeek);
                if (DiaFeriado)
                {
                    FechaEntrega = FechaEntrega.AddDays(1);
                    day = Convert.ToInt32(FechaEntrega.DayOfWeek);

                }

                if (day == 7 || day == 0)
                {
                    FechaEntrega = FechaEntrega.AddDays(1);
                    DiaFeriado = myPed.ExistsDiaFeriado(FechaEntrega.ToString("yyyy-MM-dd"));
                    if (DiaFeriado)
                    {
                        FechaEntrega = FechaEntrega.AddDays(1);
                    }

                }

                FechaValid = FechaEntrega;
                CurrentFechaEntrega = FechaEntrega;
                PickerEnabled = true;
            }

            if (ShowClientCreditInfo && Arguments.Values.CurrentClient != null)
            {
                CurrentClientData = new DS_CuentasxCobrar().GetDatosCreditoCliente(Arguments.Values.CurrentClient.CliID);
            }

            if (isFromCot2Ven)
            {
                SetCondicionPagoCliente(conId);
                SearchUnAsync(true);
            }

        }


        //Parámetro para presentar alerta si el cliente tiene facturas con más de 30 o 60 días sin saldar
        public bool Checkdays()
        {
            DS_CuentasxCobrar CuentasxCobrar = new DS_CuentasxCobrar();
            string Info = CuentasxCobrar.CheckDays();

            switch(Info)
            {
                case "60":
                         DisplayAlert(AppResource.Warning, AppResource.CannotMakeOrdersCustomerHasInvoicesMoreThan60DaysOlder, AppResource.Aceptar);
                         return true;
                case "30":
                         DisplayAlert(AppResource.Warning, AppResource.CustomerHasInvoicesMoreThan30Days, AppResource.Aceptar);
                         return false;
                case "1":
                         DisplayAlert(AppResource.Warning, AppResource.CustomerHasOverdueInvoicesMustPay, AppResource.Aceptar);
                         return true;
                default:
                    return false;
            }
        }
        public static PedidosViewModel Instance()
        {
            return _instance;
        }

        //Parametro que no permite hacer pedidos si el cliente tiene cheques devueltos pendientes de saldar
        public bool CheckChequesDevueltosSinSaldar()
        {
            if (!myParametro.GetParPedidosBloquearChequesDevueltos())
            {
                return false;
            }
            else if (Arguments.Values.CurrentModule != Modules.PEDIDOS)
            {
                return false;
            }
            bool ChkDevueltosSinSaldar = false;
            List<CuentasxCobrar> ChkDevueltos = new List<CuentasxCobrar>();
            DS_CuentasxCobrar myCxC = new DS_CuentasxCobrar();
            List<CuentasxCobrar> Documentos = myCxC.GetAllCuentasByCliente(Arguments.Values.CurrentClient.CliID, CurrentMoneda != null ? CurrentMoneda.MonCodigo : null);
            foreach(var cxc in Documentos)
            {
                if(cxc.CxcSIGLA == "CKD")
                {
                    ChkDevueltos.Add(cxc);
                    double Recibosaplicados = myCxC.GetAllRecibosAplicados(cxc.CxcReferencia);
                    cxc.CxcBalance = cxc.CxcBalance - Recibosaplicados;

                    if(cxc.CxcBalance > 0)
                    {
                        ChkDevueltosSinSaldar = true;
                    }
                }
            }

            if(ChkDevueltosSinSaldar)
            {
                DisplayAlert(AppResource.Warning, AppResource.CannotMakeOrderCustomerHasReturnedUnpaidChecks, AppResource.Aceptar);
            }

            return ChkDevueltosSinSaldar;
        }

        private bool condicionPagoAutorizada = false;
        private async void AutorizarCondicionPago()
        {
            var titId = 1;

            switch (Arguments.Values.CurrentModule)
            {
                case Modules.PEDIDOS:
                    titId = 1;
                    break;
                case Modules.VENTAS:
                    titId = 4;
                    break;
                case Modules.COTIZACIONES:                   
                    titId = 5;
                    break;

            }
            
            var modal = new AutorizacionesModal(false, NumeroTransaccion, titId, null, false, false);
            modal.OnAutorizacionUsed = (autId) => 
            {
                condicionPagoAutorizada = true;
                EnableCondicionPago = true;
                UseAuthorizePaymentTerm = false;
            };
            await PushModalAsync(modal);
        }

        private bool ValidarCantidadMultiAlmacen()
        {
            var cantidadSolicitada = ((CurrentProducto.CantidadDetalle / CurrentProducto.ProUnidades) + CurrentProducto.Cantidad);//(int)(CurrentProducto.Cantidad * CurrentProducto.ProUnidades) + CurrentProducto.CantidadDetalle;
            var cantidadEntregar = myEnt.GetCantidadTotalEntregarVentas(CurrentPedidoAEntregar.EnrSecuencia, CurrentPedidoAEntregar.TraSecuencia, CurrentProducto.ProID);
            var cantidadHolgura = myProd.CantidadHolgura(CurrentProducto.ProID);

            if (myParametro.GetParUsarMultiAlmacenes())
            {
                var cantidadAlmacenDevolucion = 0.0;
                double cantidadAlmacenDespacho;

                if (cantidadSolicitada > (cantidadEntregar * cantidadHolgura) )
                {
                    cantidadAlmacenDevolucion = cantidadSolicitada - cantidadEntregar;
                    cantidadAlmacenDespacho = cantidadEntregar;
                }
                else
                {
                    cantidadAlmacenDespacho = cantidadSolicitada;
                }

                if ((!myInv.HayExistencia(CurrentProducto.ProID, cantidad: cantidadAlmacenDespacho, almId: myParametro.GetParAlmacenIdParaDespacho(), isCantidadTotal: true))
                    || (cantidadAlmacenDevolucion > 0 && !myInv.HayExistencia(CurrentProducto.ProID, cantidad: cantidadAlmacenDevolucion, almId: myParametro.GetParAlmacenIdParaDevolucion(), isCantidadTotal: true)))
                {
                    DisplayAlert(AppResource.Warning, AppResource.QuantityGreaterThanStock);
                    return false;
                }

            }
            else if (cantidadSolicitada > cantidadEntregar)
            {
                DisplayAlert(AppResource.Warning, AppResource.QuantityRequestedGreaterThanQuantityToDelivered);
                return false;
            }

            return true;
        }

        public void SubscribeToListeners()
        {
            MessagingCenter.Subscribe<string, string>(this, "AgregarCantidad", (sender, arg) =>
            {
                AumentarCantidadProducto(arg, false);
            });

            MessagingCenter.Subscribe<string, string>(this, "RestarCantidad", (sender, arg) =>
            {
                AumentarCantidadProducto(arg, true);
            });

            MessagingCenter.Subscribe<string, string>(this, "ShowDetalle", (sender, arg) =>
            {

                var item = Productos.Where(x => x.ProID.ToString() == arg).FirstOrDefault();

                if (item != null)
                {
                    OnListItemSelected(item);
                }

            });

            MessagingCenter.Subscribe<string, string>(this, "ShowProductoCombo", (sender, args) =>
            {
                if (int.TryParse(args, out int proId))
                {
                    ShowProductosCombo(proId);
                }
            });

            MessagingCenter.Subscribe<string, string>(this, "ShowInventario", (sender, args) =>
            {
                if (int.TryParse(args, out int proId))
                {
                    ShowInventario(proId);
                }
            });
        }

        public void UnSubscribeFromListeners()
        {
            MessagingCenter.Unsubscribe<string, string>(this, "AgregarCantidad");
            MessagingCenter.Unsubscribe<string, string>(this, "RestarCantidad");
            MessagingCenter.Unsubscribe<string, string>(this, "ShowDetalle");
            MessagingCenter.Unsubscribe<string, string>(this, "ShowProductoCombo");
            MessagingCenter.Unsubscribe<string, string>(this, "ShowInventario");
        }

        private void OnMenuItemSelected(object Id)
        {
            switch (Id)
            {
                case "1":
                    SearchUnAsync(true);
                    break;
                case "2":
                    GoPrepareSave();
                    break;
                case "3":
                    //busqueda avanzada
                    if (dialogBusquedaAvanzada == null)
                    {
                        dialogBusquedaAvanzada = new BusquedaAvanzadaModal
                        {
                            OnAceptarFiltros = FiltrarBusquedaAvanzada
                        };
                    }
                    PushModalAsync(dialogBusquedaAvanzada);
                    break;
                case "4": //agregar nueva direccion
                    PushModalAsync(new AgregarClienteDireccionModal(()=> 
                    {
                        try
                        {
                            Clientedirecciones = new ObservableCollection<ClientesDirecciones>(myCli.getDirPedCli(Arguments.Values.CurrentClient.CliID));
                        }catch(Exception e)
                        {
                            Console.Write(e.Message);
                        }
                    }, myCli));
                    break;
            }
        }

        private async void FiltrarBusquedaAvanzada(BusquedaAvanzadaProductosArgs filtro)
        {
            await Search(false, filtro);
        }

        private void ConfigDevArgs()
        {
            if (Arguments.Values.CurrentModule != Modules.DEVOLUCIONES)
            {
                return;
            }            

            UseMotivoDevolucion = myParametro.GetParDevolucionesMotivoUnico();
            UseCondicionDevolucion = myParametro.GetParDevolucionesCondicionUnico();
            UseAccion = myParametro.GetParDevolucionesAccion() == "C";
            UseCintillo = myParametro.GetParDevolucionesCintillo();
            UseDocumento = (myParametro.GetParDevolucionesNumeroDocumento() && !ParDevolucionesProductosFacturas) || (myParametro.GetParDevolucionesNumeroDocumento() && myParametro.GetParHistoricoFacturasFromCuentasxCobrar());


            if (UseMotivoDevolucion)
            {
                MotivosDevolucion = myDev.GetMotivosDevolucion();

                if(IsEditing && EditedTranSecuencia != -1)
                {
                    var motivo = myDev.GetDevolucionDetalleMotivo(EditedTranSecuencia, false).FirstOrDefault();

                    if(motivo != null)
                    {
                        CurrentMotivoDevolucion = MotivosDevolucion.Where(x => x.MotID == motivo.MotID).FirstOrDefault();
                    }
                }
                else
                {
                    var motivoPorDefecto = myParametro.GetParDevolucionesMotivoPorDefecto();

                    if(!string.IsNullOrWhiteSpace(motivoPorDefecto) && MotivosDevolucion != null && MotivosDevolucion.Count > 0)
                    {
                        var mot = MotivosDevolucion.Where(x => x.MotID.ToString().Equals(motivoPorDefecto)).FirstOrDefault();
                        CurrentMotivoDevolucion = mot;
                    }
                }
            }

            if (UseCondicionDevolucion)
            {
                CondicionDevolucion = myUso.GetDevolucionCondicion();

                if (IsEditing && EditedTranSecuencia != -1)
                {
                    var condicion = myDev.GetDevolucionDetalleCondicion(EditedTranSecuencia, false).FirstOrDefault();

                    if (condicion != null)
                    {
                        CurrentCondicionDevolucion = CondicionDevolucion.Where(x => x.CodigoUso == condicion.CodigoUso).FirstOrDefault();
                    }
                }
                else
                {
                    var condicionPorDefecto = myParametro.GetParDevolucionesCondicionPorDefecto();

                    if (!string.IsNullOrWhiteSpace(condicionPorDefecto) && CondicionDevolucion != null && CondicionDevolucion.Count > 0)
                    {
                        var cod = CondicionDevolucion.Where(x => x.CodigoUso.ToString().Equals(condicionPorDefecto)).FirstOrDefault();
                        CurrentCondicionDevolucion = cod;
                    }
                }
            }

            if (UseAccion)
            {
                AccionesDevolucion = myUso.GetDevolucionAccion();
            }

        }

        private bool IsMotivoValido(MotivosDevolucion newMotivo)
        {

            if (currentmotivodevolucion == null)
            {
                return true;
            }

            if (Arguments.Values.CurrentModule == Modules.DEVOLUCIONES && !myDev.ValidarMotivoUnico(newMotivo.MotID))
            {
                DisplayAlert(AppResource.Warning, AppResource.CannotAddProductsWithDifferentsReasons, AppResource.Aceptar);
                return false;
            }

            return true;
        }

        private void SetNumeroTransaccion()
        {
            switch (Arguments.Values.CurrentModule)
            {
                case Modules.PEDIDOS:
                    NumeroTransaccion = DS_RepresentantesSecuencias.GetLastSecuencia("Pedidos");
                    break;
                case Modules.DEVOLUCIONES:
                    NumeroTransaccion = DS_RepresentantesSecuencias.GetLastSecuencia("Devoluciones");
                    break;
                case Modules.INVFISICO:
                    NumeroTransaccion = DS_RepresentantesSecuencias.GetLastSecuencia("InventarioFisico");
                    break;
                case Modules.COMPRAS:
                    NumeroTransaccion = DS_RepresentantesSecuencias.GetLastSecuencia("Compras");
                    break;
                case Modules.CONTEOSFISICOS:
                    NumeroTransaccion = DS_RepresentantesSecuencias.GetLastSecuencia("ConteosFisicos");
                    break;
                case Modules.REQUISICIONINVENTARIO:
                    NumeroTransaccion = DS_RepresentantesSecuencias.GetLastSecuencia("RequisicionesInventario");
                    break;
                case Modules.CAMBIOSMERCANCIA:
                    NumeroTransaccion = DS_RepresentantesSecuencias.GetLastSecuencia("CambiosMercancia");
                    break;
                case Modules.VENTAS:
                    NumeroTransaccion = DS_RepresentantesSecuencias.GetLastSecuencia("Ventas");
                    break;
                case Modules.PROMOCIONES:
                    NumeroTransaccion = DS_RepresentantesSecuencias.GetLastSecuencia("Entregas");
                    break;
                case Modules.ENTREGASMERCANCIA:
                    NumeroTransaccion = DS_RepresentantesSecuencias.GetLastSecuencia("Entregas");
                    break;
                case Modules.COLOCACIONMERCANCIAS:
                    NumeroTransaccion = DS_RepresentantesSecuencias.GetLastSecuencia("ColocacionProductos");
                    break;
            }
        }

        private async void SearchOrClean(string value = "")
        {
            LastValueSearch = SearchValue;
            if (CurrentFilter != null && CurrentFilter.IsCodigoBarra)
            {
                GoScanQr();
                return;
            }

            if (CurrentFilter != null && CurrentFilter.FilTipo == 3)
            {
                SearchValue = "";
            }
            else
            {
                await Search(false);
            }
        }

        private async void GoPrepareSave(bool showRecordarProductos = true,int motivodevolucion= -1, bool montoMinimoAutorizado = false, bool descManualAutorizado = false, bool showRecordarLineas = true)
        {
            try
            {
                if (IsBusy)
                {
                    return;
                }

                IsBusy = true;

                var prod = productos.Where(p => p.Cantidad <= 0 && p.ProDatos3.Equals("R")).FirstOrDefault();

                if (prod != null && Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS && myParametro.GetParCantidadMaximaForReturn())
                {
                    var cuadre = myCua.GetCuadreForCant(Arguments.Values.CurrentCuaSecuencia, prod.ProID);
                    if (cuadre == null)
                    {
                        throw new Exception(AppResource.InitialQuantityNotFound);
                    }

                    var producto = myProd.ProCantidadHolgura(prod.ProID);
                    if (producto == null)
                    {
                        throw new Exception(AppResource.SlackQuantityNotFound);
                    }

                    var obtporcent = (producto.ProHolgura / 100) * cuadre.CuaCantidadInicial;
                    string prodat = prod.ProDatos3;

                    if (prod.Cantidad < obtporcent && prod.ProDatos3.Equals("R"))
                    {
                        throw new Exception(AppResource.ReturnableProductCountCannotBeLessThan.Replace("@", producto.ProHolgura.ToString()));
                    }
                }

                var tipoInventario = DS_RepresentantesParametros.GetInstance().GetParInventarioFisico();

                if (myProd.NothingAddedInTemp((int)Arguments.Values.CurrentModule)
                    && (Arguments.Values.CurrentModule != Modules.CONTEOSFISICOS && Arguments.Values.CurrentModule != Modules.INVFISICO
                        || Arguments.Values.CurrentModule == Modules.INVFISICO && tipoInventario == 1))
                {
                    throw new Exception(AppResource.YouHaveNotAddAnyProductWarning);
                }
                else if (myProd.NothingAddedInTemp((int)Arguments.Values.CurrentModule)
                        && Arguments.Values.CurrentModule == Modules.INVFISICO && tipoInventario == 2 &&
                        (myInvF.GetProductosInInventarioConFaltantes(Arguments.Values.CurrentClient.CliID) == null
                        || myInvF.GetProductosInInventarioConFaltantes(Arguments.Values.CurrentClient.CliID).Count == 0))
                {
                    throw new Exception(AppResource.YouHaveNotAddedAnyProductsAndTheCustomerHasNoLogicalInventory);
                }

                if (DS_RepresentantesParametros.GetInstance().GetParTasaMonedas())
                {
                    var date = DateTime.Now;

                    if (UseMultiMoneda && CurrentMoneda != null)
                    {
                        var mon = myMon.GetMonedaByMonCod(CurrentMoneda.MonCodigo);

                        TimeSpan time = mon.MonFechaActualizacion - DateTime.Now;

                        if (time.Hours < -24)
                        {
                            throw new Exception(AppResource.RateExceed24HoursWarning);
                        }
                    }
                    else
                    {
                        var mon = myMon.GetMonedaByMonCod(Arguments.Values.CurrentClient.MonCodigo);

                        TimeSpan time = mon.MonFechaActualizacion - DateTime.Now;

                        if (time.Hours < -24)
                        {
                            throw new Exception(AppResource.RateExceed24HoursWarning);
                        }
                    }
                }


                if (!string.IsNullOrWhiteSpace(Arguments.Values.CurrentClient.CliDatosOtros) 
                    && Arguments.Values.CurrentClient.CliDatosOtros.ToUpper().Contains("U") && myParametro.GetNumOrdenObligatorio()
                    && string.IsNullOrWhiteSpace(PedOrdenCompra))
                {
                    throw new Exception(AppResource.YouMustSpecifyThePurchaseOrderNumberConfigurationTab);
                }

                if (UseAccion && CurrentAccionDevolucion == null && Arguments.Values.CurrentModule == Modules.DEVOLUCIONES)
                {
                    throw new Exception(AppResource.ActionNotSelectedYet);
                }

                if (UseCintillo && string.IsNullOrWhiteSpace(CurrentCintillo))
                {
                    throw new Exception(AppResource.MustSpecifyHeadband);
                }

                if (UseDocumento && string.IsNullOrWhiteSpace(CurrentDevDocumento) && !myParametro.GetParAccionesNoObligatorias())
                {
                    throw new Exception(AppResource.MustSpecifyDocument);
                }

                if (UseMotivoDevolucion && CurrentMotivoDevolucion == null)
                {
                    throw new Exception(AppResource.MustSpecifyReasonForReturnInConfigTab);
                }


                if (ParDevolucionesProductosFacturas && !myParametro.GetParDevolucionesNoValidaMotivo())
                {
                    var productos = myProd.GetResumenProductos((int)Arguments.Values.CurrentModule).Where(x => x.MotIdDevolucion == -1).FirstOrDefault();
                    if(productos != null)
                    {
                        throw new Exception("Debes de seleccionar el motivo de devolución para cada articulo.");
                    }
                }

                if (UseCondicionDevolucion && CurrentCondicionDevolucion == null)
                {
                    throw new Exception(AppResource.MustSpecifyProductReturnConditionInConfigTab);
                }

                if (IsTraspaso && string.IsNullOrWhiteSpace(CurrentTipoTraspaso))
                {
                    throw new Exception(AppResource.MustSelectTransferTypeInConfigTab);
                }

                if(IsTraspaso && CurrentRepTraspaso == null)
                {
                    throw new Exception(IsEntregandoTraspaso ? AppResource.MustSelectRepresentativeToAssignTransfer : AppResource.MustSelectRepresentativeFromReceiveTheTransfer);
                }

                if(IsTraspaso && IsEntregandoTraspaso)
                {
                    var productos = myProd.GetResumenProductos((int)Arguments.Values.CurrentModule);

                    foreach(var p in productos)
                    {
                        if(!myInv.HayExistencia(p.ProID, p.Cantidad, out Inventarios existencia, p.CantidadDetalle))
                        {
                            throw new Exception(AppResource.ProductQuantityIsGreaterThanInventoryWithDescription.Replace("@", p.Descripcion) + (existencia != null ? ": " + existencia.invCantidad.ToString() + (existencia.InvCantidadDetalle > 0 ? "/" + existencia.InvCantidadDetalle.ToString() : "") : ""));
                        }
                    }
                }

                int.TryParse(VenCantidadCanastos, out int cantidadCanastos);

                var resultTotal = Math.Round(Resumen.Total, 2);
                var resultClienteBalance = Math.Round(Arguments.Values.CurrentClient.Cliente_Balance + Resumen.Total, 2);

                if (myParametro.GetParConvertirBalanceADolares() && CurrentMoneda.MonCodigo != "USD")
                {
                    var monedas = new DS_Monedas().GetMonedas("USD");
                    resultTotal = Math.Round(resultTotal / monedas[0].MonTasa, 2);
                    resultClienteBalance = Math.Round(Arguments.Values.CurrentClient.Cliente_Balance + (Resumen.Total / monedas[0].MonTasa), 2);
                }

                if (Arguments.Values.CurrentModule == Modules.VENTAS || Arguments.Values.CurrentModule == Modules.PROMOCIONES)
                {                    
                    if ((IsGonnaBeActive == 2 || (myParametro.GetParEntregasPromocionesUsarCanastos() && Arguments.Values.CurrentModule == Modules.PROMOCIONES)) && cantidadCanastos <= 0)
                    {
                        throw new Exception(AppResource.NumberOfBasketsCannotBeZero);
                    }

                    if (Arguments.Values.CurrentModule == Modules.VENTAS)
                    {
                        ValidarMontoFacturarEsMenorMaxFacturar();
                    }
                }

                if ((Arguments.Values.CurrentModule == Modules.VENTAS || Arguments.Values.CurrentModule == Modules.PEDIDOS) && CurrentCondicionPago == null)
                {
                    throw new Exception(AppResource.MustSpecifyPaymentCondition);
                }
                else if (CurrentCondicionPago != null)
                {
                    myProd.SetCondicionPagoInTemp(CurrentCondicionPago.ConID);
                }

                if (myParametro.GetValidaRNCWithComprobanteFiscal())
                {

                    if ((string.IsNullOrWhiteSpace(Arguments.Values.CurrentClient.CliRNC) && (Arguments.Values.CurrentModule == Modules.PEDIDOS)  && (Arguments.Values.CurrentClient.CliTipoComprobanteFAC == "01")))
                    {
                        throw new Exception(AppResource.FiscalCreditOrderCannotBeMadeCustomerHasNoId);
                    }
                    else if ((Arguments.Values.CurrentModule == Modules.PEDIDOS) && string.IsNullOrWhiteSpace(Arguments.Values.CurrentClient.CliRNC) && (Arguments.Values.CurrentClient.CliTipoComprobanteFAC == "02") && Resumen.Total > myParametro.GetValorMaximoPedidoWithConsumidorFinal())
                    {
                        throw new Exception(AppResource.CustomerDontHaveIdAndOrderAmountExceedFinalConsumer + myParametro.GetValorMaximoPedidoWithConsumidorFinal());
                    }

                }

                string monedaValidar = (UseMultiMoneda && CurrentMoneda != null) ? CurrentMoneda.MonCodigo : Arguments.Values.CurrentClient.MonCodigo;
                double pedidoMontoMinimo = !string.IsNullOrWhiteSpace(monedaValidar)? myParametro.GetParPedidosMontoMinimo(monedaValidar) : 0.00;
                string monedaSigla = !string.IsNullOrWhiteSpace(monedaValidar) ? myMon.GetMonedaByMonCodForDep(monedaValidar) != null ? myMon.GetMonedaByMonCodForDep(monedaValidar).MonSigla : "RD$" : "RD$";
                
                if (pedidoMontoMinimo > resultTotal && Arguments.Values.CurrentModule == Modules.PEDIDOS && !montoMinimoAutorizado && myParametro.GetParPedidosValidaMontoMinimo() == 1)
                {
                   var result = await DisplayAlert(AppResource.Warning, AppResource.OrderAmountMustGreaterThanMinimumConfigured.Replace("@1", monedaSigla).Replace("@2", pedidoMontoMinimo.ToString()).Replace("@3", monedaSigla).Replace("@4", resultTotal.ToString()), AppResource.Authorize, AppResource.Cancel);

                   if (result)
                   {
                       await Application.Current.MainPage.Navigation.PushModalAsync(new AutorizacionesModal(false, NumeroTransaccion, 1, null, false, false)
                       {
                           OnAutorizacionUsed = (autId) =>
                           {
                               GoPrepareSave(showRecordarProductos, motivodevolucion, true);
                           }
                       });
                       IsBusy = false;
                       return;
                    }
                   IsBusy = false;
                   return;
                }
                //Seleccionar el tipo de pedido
                if (UsePedDir && CurrentCldDirTipo == null && (myParametro.GetParPedidosDirrecion() == 2 ||
                    myParametro.GetParCotizacionDirrecion() == 2))
                {
                    throw new Exception(AppResource.MustSelectCustomerAddress);
                }

                if (UseTipoTrasporte && CurrentTipoTrasporte == null && Arguments.Values.CurrentModule == Modules.PEDIDOS)
                {
                    throw new Exception(AppResource.MustSelectTransportType);
                }

                if (Arguments.Values.CurrentModule == Modules.COMPRAS)
                {
                    if (CurrentTipoPago == null && UseTipoPagoCompras)
                    {
                        throw new Exception(AppResource.MustSelectPaymentType);
                    }

                    if (!myParametro.GetParComprasNoUsarDependiente())
                    {
                        if (!myParametro.GetDependienteNoObligatorio() && (DependientesViewModel.CurrentDependiente == null || DependientesViewModel.CurrentDependiente.Cliid == -1 || DependientesViewModel.CurrentDependiente.Cliid == -2))
                        {
                            throw new Exception(AppResource.MustSelectDependent);
                        }
                    }

                    if (myParametro.GetParComprasFormaPagoAutomatica() < 1)
                    {
                        if (FormasPagoAgregadas == null || FormasPagoAgregadas.Count == 0)
                        {
                            throw new Exception(AppResource.MustAddPaymentway);
                        }

                        var montoFormasPago = FormasPagoAgregadas.Sum(x => x.Valor);
                        if (montoFormasPago < resultTotal || montoFormasPago > resultTotal)
                        {
                            throw new Exception(AppResource.PaymentMethodAmountCannotBeDifferentOrder);
                        }

                    }
                }

                var descuentoGeneralManual = 0.0;
                if (ShowDescuentoManual && double.TryParse(DescuentoManual, out double descuentoManual))
                {
                    var descuentoMaximo = myParametro.GetParPedidosDescuentoManualGeneral();

                    if (descuentoManual > descuentoMaximo)
                    {
                        if (myParametro.GetParPedidosDescuentoGeneralAutorizar())
                        {
                            if (!descManualAutorizado)
                            {
                                var modal = new AutorizacionesModal(false, NumeroTransaccion, 1, null, false, false)
                                {
                                    OnAutorizacionUsed = (autId) =>
                                    {
                                        GoPrepareSave(showRecordarProductos, motivodevolucion,montoMinimoAutorizado, true);
                                    }
                                };
                                IsBusy = false;
                                await DisplayAlert(AppResource.Warning, AppResource.MustAuthorizeToGiveDiscountGreaterThanMaximum + descuentoMaximo.ToString() + "%");
                                await PushModalAsync(modal);
                                return;
                            }
                        }
                        else
                        {
                            throw new Exception(AppResource.GeneralDiscountCannotExceedMaximumOf + descuentoMaximo + " %");
                        }
                    }
                    descuentoGeneralManual = descuentoManual;
                    //args.PorcientoDescuentoManual = descuentoManual;
                }

                if (ParOfertasManuales)
                {
                    List<ProductosTemp> productosInvalidosDes = new List<ProductosTemp>();

                    List<ProductosTemp> productosInvalidos = myProd.ValidarProductosOfertasManuales((int)Arguments.Values.CurrentModule, IsDpp);

                    if (myParametro.GetParDescuentosMaximosForPedidos())
                    {
                        productosInvalidosDes = myProd.ValidarProductosDescuentosManuales((int)Arguments.Values.CurrentModule, IsDpp);
                    }


                    if (productosInvalidos.Count > 0 || productosInvalidosDes.Count > 0)
                    {
                        IsBusy = false;
                        var revisar = await DisplayAlert(AppResource.Warning, AppResource.ProductsExceedMaximumAmountFor + (productosInvalidos.Count > 0 ? AppResource.Offer : AppResource.Discount) + " ", AppResource.Inspect, AppResource.Aceptar);

                        if (revisar)
                        {
                            await PushModalAsync(new ProductosExcedeDescuentoModal(productosInvalidos.Count > 0? productosInvalidos : productosInvalidosDes));
                        }
                        IsBusy = false;
                        return;
                    }
                }

                if(CurrentPedidoAEntregar != null && Arguments.Values.CurrentModule == Modules.VENTAS && myParametro.GetParUsarMultiAlmacenes())
                {
                    if (myProd.HayProductosSinLoteAgregados())
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.ProductsThaUseLotAndLotIsNotSpecifyWillBeIgnored);
                    }
                }

                var invArea = -1;
                if (UseInvArea && CurrentInvArea == null)
                {
                    throw new Exception(AppResource.MustSelectWarehouseAreaInConfigTab);
                }

                if (Arguments.Values.CurrentModule == Modules.PEDIDOS && showRecordarProductos)
                {
                    var productosRecordar = myGrp.GetProductosRecordar();

                    if (productosRecordar != null && productosRecordar.Count > 0)
                    {
                        IsBusy = false;
                        await PushModalAsync(new PedidosProductosRecordarModal(productosRecordar) { OnAccept = () => { GoPrepareSave(false,motivodevolucion,montoMinimoAutorizado,descManualAutorizado); } });
                        return;
                    }
                }

                if (showRecordarLineas)
                {
                    List<Productos> lineasRecordar = null;
                    if (myParametro.GetLineasRecordar() > 0)
                    {
                        switch (Arguments.Values.CurrentModule)
                        {
                            case Modules.PEDIDOS:
                                lineasRecordar = myParametro.GetLineasRecordar() == 1 ? myGrp.GetLineasRecordar("PEDLINRECORDAR", "PEDLINREC" + Arguments.CurrentUser.RepCodigo.ToUpper())
                                    : myGrp.GetLineasRecordarByBusqueda("PEDLINRECORDAR", "PEDLINREC" + Arguments.CurrentUser.RepCodigo.ToUpper(), NumeroTransaccion.ToString());
                                break;
                            case Modules.COTIZACIONES:
                                lineasRecordar = myParametro.GetLineasRecordar() == 1 ? myGrp.GetLineasRecordar("COTLINRECORDAR", "COTLINREC" + Arguments.CurrentUser.RepCodigo.ToUpper())
                                    : myGrp.GetLineasRecordarByBusqueda("COTLINRECORDAR", "COTLINREC" + Arguments.CurrentUser.RepCodigo.ToUpper(), NumeroTransaccion.ToString());
                                break;
                            case Modules.VENTAS:
                                lineasRecordar = myParametro.GetLineasRecordar() == 1 ? myGrp.GetLineasRecordar("VENLINRECORDAR", "VENLINREC" + Arguments.CurrentUser.RepCodigo.ToUpper())
                                    : myGrp.GetLineasRecordarByBusqueda("VENLINRECORDAR", "VENLINREC" + Arguments.CurrentUser.RepCodigo.ToUpper(), NumeroTransaccion.ToString());
                                break;
                        }
                    }

                    if (lineasRecordar != null && lineasRecordar.Count > 0)
                    {
                        IsBusy = false;
                        if(myParametro.GetLineasRecordar() == 3)
                        {
                            await PushModalAsync(new LineasObligatoriasModal(lineasRecordar) { });
                        }
                        else
                        {
                            await PushModalAsync(new LineasRecordarModal(lineasRecordar) { OnAccept = () => { GoPrepareSave(false, motivodevolucion, montoMinimoAutorizado, descManualAutorizado, false); } });
                        }
                        return;
                    }
                }

                if (UseInvArea)
                {
                    int.TryParse(CurrentInvArea.CodigoUso, out int result);

                    invArea = result;

                    if (invArea == 0)
                    {
                        invArea = -1;
                    }
                }
                
                var devArgs = new DevolucionesArgs();

                if (Arguments.Values.CurrentModule == Modules.DEVOLUCIONES || (Arguments.Values.CurrentModule == Modules.CAMBIOSMERCANCIA && UseMotivoDevolucion))
                {
                    if (Arguments.Values.CurrentModule == Modules.DEVOLUCIONES && DS_RepresentantesParametros.GetInstance().GetParHistoricoFacturasFromCuentasxCobrar())
                    {
                        devArgs.DevCintillo = CurrentDevDocumento;
                        devArgs.DevNCF = CurrentCintillo;
                        devArgs.Accion = (CurrentAccionDevolucion?.CodigoUso);
                    }
                    else if (Arguments.Values.CurrentModule == Modules.DEVOLUCIONES)
                    {
                        devArgs.Documento = CurrentDevDocumento;
                        devArgs.DevNCF = CurrentCintillo;
                        devArgs.Accion = (CurrentAccionDevolucion?.CodigoUso);
                    }
                    devArgs.MotId = CurrentMotivoDevolucion != null ? CurrentMotivoDevolucion.MotID : -1;

                    if (ParDevolucionesProductosFacturas)
                    {
                        devArgs.Documento = FacturaId.ToString();
                    }

                    if (CurrentMotivoDevolucion != null)
                    {
                        SqliteManager.GetInstance().Execute("update ProductosTemp set MotIdDevolucion = ? where TitID = ? ", 
                            new string[] { CurrentMotivoDevolucion.MotID.ToString(), ((int)Arguments.Values.CurrentModule).ToString() });
                    }
                    if (CurrentCondicionDevolucion != null)
                    {
                        SqliteManager.GetInstance().Execute("update ProductosTemp set DevCondicion = ? where TitID = ? ", 
                            new string[] { CurrentCondicionDevolucion.CodigoUso.ToString(), ((int)Arguments.Values.CurrentModule).ToString() });
                    }
                }

                if (UseInvArea && CurrentInvArea != null && int.TryParse(CurrentInvArea.CodigoUso, out int invAreaId))
                {
                    SqliteManager.GetInstance().Execute("update ProductosTemp set InvAreaId = ?, InvAreaDescr = '"+CurrentInvArea.Descripcion+"' where TitID = ? ",
                            new string[] { invAreaId.ToString(), ((int)Arguments.Values.CurrentModule).ToString() });
                }

                var visualizacion = myParametro.GetFormatoVisualizacionProductos(); 

                if (visualizacion == -1)
                {
                    visualizacion = myParametro.GetFormatoVisualizacionProductosLocal();
                }

                bool parTipoPedidoObligatorio = myParametro.GetParTipoPedidoObligatorio();
                if (parTipoPedidoObligatorio && CurrentTipoPedido == null)
                {
                    throw new Exception(AppResource.MustSelectOrderTypeInConfigTab);
                }

                int tipoPedido = 1;               
                if ((Arguments.Values.CurrentModule == Modules.PEDIDOS || ParCotizacionesTipos) && CurrentTipoPedido != null)
                {
                    int.TryParse(CurrentTipoPedido.CodigoUso, out int result);

                    tipoPedido = result;
                }

                string fecha = CurrentFechaEntrega.ToString("MM/dd/yyyy");
                string hora = CurrentTimeEntrega.ToString();
                var newDateTime = CurrentFechaEntrega;

                if (IsHora)
                {
                   newDateTime = Convert.ToDateTime(fecha).Add(TimeSpan.Parse(hora));
                }

                List<EntregasRepartidorTransaccionesDetalle> NoEntregados = null;

                if (CurrentPedidoAEntregar != null && myParametro.GetParMotDevolucionPedidoParcial())
                {
                    NoEntregados = myEnt.GetProductosNoEntregadosxPedidos(CurrentPedidoAEntregar.EnrSecuencia, CurrentPedidoAEntregar.TraSecuencia, CurrentPedidoAEntregar.TitID);
                }

                if (NoEntregados != null && NoEntregados.Count > 0 && motivodevolucion == -1 && myParametro.GetParMotDevolucionPedidoParcial())
                {
                    await DisplayAlert(AppResource.Warning, AppResource.ProductsNotDeliveredFullyMustSpecifyReason);


                    await PushModalAsync(new SeleccionarMotivoDevolucionModal() { OnMotivoAceptado = (motId) => { GoPrepareSave(motivodevolucion: motId,montoMinimoAutorizado: montoMinimoAutorizado,descManualAutorizado: descManualAutorizado, showRecordarLineas: false); } });


                    IsBusy = false;
                    return;
                }

                if (NoEntregados != null && NoEntregados.Count > 0 && myParametro.GetParNoEntregarVentasParaciales())
                {
                    await DisplayAlert(AppResource.Warning, AppResource.SomeProductsHaveNotBeenDeliveredEntirelyYouMustAdd);
                    IsBusy = false;
                    return;
                }

                Arguments.Values.IsPedidoAutorizado = false;
                if (Arguments.Values.CurrentClient.CliDatosOtros != null && Arguments.Values.CurrentClient.CliDatosOtros.Contains("X") && Arguments.Values.CurrentModule == Modules.PEDIDOS &&
                    (Arguments.Values.CurrentClient.CliLimiteCredito < resultClienteBalance
                    || mycxc.GetForValidFac()
                    || mycxc.GetCountChequesDevueltos(Arguments.Values.CurrentClient.CliID) > 0
                    || mycxc.GetForValidFacPendientes(Arguments.Values.CurrentClient.CliID)))
                {
                    Arguments.Values.IsPedidoAutorizado = true;
                }

                var args = new PedidosDetalleArgs
                {

                    FechaEntrega = newDateTime,
                    ConId = CurrentCondicionPago != null && (Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.COTIZACIONES || Arguments.Values.CurrentModule == Modules.VENTAS) ? CurrentCondicionPago.ConID : -1,
                    DisenoDelRow = visualizacion,
                    PedOrdenCompra = PedOrdenCompra,
                    IsEditing = IsEditing,
                    devArgs = devArgs,
                    InvArea = invArea,
                    EditedTraSecuencia = EditedTranSecuencia,
                    TipoPedido = tipoPedido,
                    ComTipoPago = UseTipoPagoCompras && CurrentTipoPago != null ? CurrentTipoPago.CodigoUso : null,
                    CompraDependiente = DependientesViewModel != null ? DependientesViewModel.CurrentDependiente : null,
                    CldDirTipo = CurrentCldDirTipo != null ? CurrentCldDirTipo.CldDirTipo : Arguments.Values.CurrentClient.CldDirTipo,
                    RepAuditor = repAuditor,
                    CurrentEntrega = CurrentPedidoAEntregar,
                    CedCodigo = CurrentCentrosDistribucion != null && Arguments.Values.CurrentModule == Modules.PEDIDOS ? CurrentCentrosDistribucion.CedCodigo.ToString() : "",
                    CurrentAlmacenConteo = CurrentAlmacenForConteo,
                    PedTipoTrans = UseTipoTrasporte && CurrentTipoTrasporte != null ? Convert.ToInt32(CurrentTipoTrasporte.CodigoUso) : -1,
                    CliIDMaster = UseCliIDMaster && Arguments.Values.CurrentModule == Modules.PEDIDOS && ClienteMaster != null ? ClienteMaster.CliID : -1,
                    MonCodigo = (UseMultiMoneda && CurrentMoneda != null) ? CurrentMoneda.MonCodigo : Arguments.Values.CurrentClient.MonCodigo,
                    RepCodigoTraspaso = IsTraspaso ? CurrentRepTraspaso.RepCodigo : null,
                    IsEntregandoTraspaso = IsEntregandoTraspaso,
                    VenCantidadCanastos = cantidadCanastos,
                    motivodevolucion= motivodevolucion > -1 ? motivodevolucion : -1,
                    FromCopy = FromCopy,
                    IsMultiEntrega = UseMultiEntrega,
                    EnEspera = isOnHold,
                };

                if(ShowDescuentoManual && descuentoGeneralManual > 0.0)
                {
                    args.PorcientoDescuentoManual = descuentoGeneralManual;
                }

                if (Arguments.Values.CurrentModule == Modules.COMPRAS && FormasPagoAgregadas != null)
                {
                    args.comprasFormasPago = FormasPagoAgregadas.ToList();
                }

                if (myParametro.GetParPedidosCamposAdicionales() || myParametro.GetParDevolucionesCamposAdicionales() || myUso.GetCotizacionesCamposAdicionales() != null)
                {
                    if (page is MasterDetailPage md && md.Detail != null && md.Detail is NavigationPage np && np.RootPage is TabbedPage tPage)
                    {
                        if (tPage.Children[1] is PedidosConfigurarPage tab)
                        {

                            var CamposObligatorios = DS_RepresentantesParametros.GetInstance().GetParCamposAdicionalesCamposObligatorios();
                            bool DestinoObligatorio = CamposObligatorios.Contains("Destino");

                            args.PedCamposAdicionales = tab.GetCamposAdicionales();

                            if (args.PedCamposAdicionales.ToString().Contains("ERROR") && DestinoObligatorio)
                            {
                                throw new Exception(AppResource.MustSelectDestinationInConfigTab);
                            }                          

                        }
                    }
                }

                if (CurrentPrioridad != null && (Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.COTIZACIONES))
                {
                    int.TryParse(CurrentPrioridad.CodigoUso, out int prioridad);
                    args.Prioridad = prioridad;
                }

                bool validLimiteCredito = true;
                if(Arguments.Values.CurrentClient.CliDatosOtros != null
                    && Arguments.Values.CurrentClient.CliDatosOtros.Contains("L"))
                {
                    validLimiteCredito = false;
                }

                if (validLimiteCredito && myParametro.LimiteCreditoInVentas()
                    && Arguments.Values.CurrentClient.CliLimiteCredito < resultClienteBalance
                    && CurrentCondicionPago.ConDiasVencimiento != 0 && IsVenAutlImcre)
                {

                    var result = await DisplayAlert(AppResource.Warning, AppResource.SaleAmountMustBeLessThanCreditLimitWarning.Replace("@", ((Arguments.Values.CurrentClient.CliLimiteCredito - Arguments.Values.CurrentClient.Cliente_Balance)).ToString()) + Resumen.Total, AppResource.Authorize, AppResource.Cancel);

                    if (result)
                    {
                        await Application.Current.MainPage.Navigation.PushModalAsync(new AutorizacionesModal(false, NumeroTransaccion, 4, "")
                        {
                            OnAutorizacionUsed = (autSec) =>
                            {
                                PushAsync(new PedidosDetallePage(args));
                            }
                        });
                        IsBusy = false;
                        return;
                    }
                    else
                    {
                        IsBusy = false;
                        return;
                    }
                }
                else if (validLimiteCredito && !IsVenAutlImcre && myParametro.LimiteCreditoInVentas() && Arguments.Values.CurrentModule == Modules.VENTAS && Arguments.Values.CurrentClient.CliLimiteCredito < (Arguments.Values.CurrentClient.Cliente_Balance + Resumen.Total)
                         && CurrentCondicionPago.ConDiasVencimiento != 0)
                     {

                            throw new Exception(AppResource.SaleAmountMustBeLessThanCreditLimitWarning.Replace("@", (Arguments.Values.CurrentClient.CliLimiteCredito - Arguments.Values.CurrentClient.Cliente_Balance).ToString())+ Resumen.Total);

                     }

                if (myParametro.LimiteCreditoEnPedidos() == 1 && (Arguments.Values.CurrentModule == Modules.PEDIDOS) && Arguments.Values.CurrentClient.CliLimiteCredito < resultClienteBalance
                    && (CurrentCondicionPago == null || CurrentCondicionPago.ConDiasVencimiento != 0))
                {
                    var result = await DisplayAlert(AppResource.Warning, AppResource.OrderAmountPlusCustomerBalanceGreaterThanCreditLimitWarning.Replace("@1", (Arguments.Values.CurrentClient.CliLimiteCredito - Arguments.Values.CurrentClient.Cliente_Balance).ToString("N2")).Replace("@2", Resumen.Total.ToString("N2")), AppResource.Continue, AppResource.Cancel);
                    IsBusy = false;
                    if (!result)
                    {
                        return;
                    }
                    await PushAsync(new PedidosDetallePage(args));

                }
                else if (myParametro.LimiteCreditoEnPedidos() == 2 && (Arguments.Values.CurrentModule == Modules.PEDIDOS) && Arguments.Values.CurrentClient.CliLimiteCredito < resultClienteBalance
                   && (CurrentCondicionPago == null || CurrentCondicionPago.ConDiasVencimiento != 0))
                {
                    bool result = await DisplayAlert(AppResource.Warning, AppResource.CannotMakeOrdersAmountPlusBalanceGreaterThanAvailableCredit.Replace("@1", (Arguments.Values.CurrentClient.CliLimiteCredito - Arguments.Values.CurrentClient.Cliente_Balance).ToString("N2")).Replace("@2", Resumen.Total.ToString("N2")), AppResource.Authorize, AppResource.Cancel);

                    if (result)
                    {
                        await Application.Current.MainPage.Navigation.PushModalAsync(new AutorizacionesModal(false, 0, 1, "")
                        {
                            OnAutorizacionUsed = (autSec) =>
                            {
                                PushAsync(new PedidosDetallePage(args));
                            }
                        });
                    }
                }
                else if (myParametro.LimiteCreditoEnPedidos() == 4 && (Arguments.Values.CurrentModule == Modules.PEDIDOS) && Arguments.Values.CurrentClient.CliLimiteCredito < Resumen.Total
                   && (CurrentCondicionPago == null || CurrentCondicionPago.ConDiasVencimiento != 0))
                {
                    bool result = await DisplayAlert(AppResource.Warning, AppResource.CannotMakeOrdersAmountPlusBalanceGreaterThanCreditLimit.Replace("@1", (Arguments.Values.CurrentClient.CliLimiteCredito).ToString("N2")).Replace("@2", Resumen.Total.ToString("N2")), AppResource.Authorize, AppResource.Cancel);

                    if (result)
                    {
                        await Application.Current.MainPage.Navigation.PushModalAsync(new AutorizacionesModal(false, 0, 1, "")
                        {
                            OnAutorizacionUsed = (autSec) =>
                            {
                                PushAsync(new PedidosDetallePage(args));
                            }
                        });
                    }
                }
                else
                {
                  await PushAsync(new PedidosDetallePage(args));
                }

            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                IsBusy = false;
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        private void ValidarMontoFacturarEsMenorMaxFacturar()
        {

            var parPagoMinimo = myParametro.GetParVentasPorcientoBalancePagoMinimo();

            if(!(parPagoMinimo > 0) || !myParametro.GetParVentasCalculadoraDeNegociacion(false))
            {
                return;
            }

            //var cliData = new DS_CuentasxCobrar().GetDatosCreditoCliente(Arguments.Values.CurrentClient.CliID);
            var cliData = new DS_CuentasxCobrar().GetDatosCreditoClienteByFecha(Arguments.Values.CurrentClient.CliID);

            var parAdicionalPedido = myParametro.GetParVentasPorcientoAdicionalPedido();
            var parAdicionalLimiteCredito = myParametro.GetParVentasPorcientoAdicionalLimiteCredito();
            var montoAbono = new DS_Recibos().GetMontoTotalRecibosByCliIdyFecha(Arguments.Values.CurrentClient.CliID, fecha: DateTime.Now.ToString("yyyy-MM-dd"));
            //var montoAbono = new DS_Recibos().GetMontoTotalRecibosByCliId(Arguments.Values.CurrentClient.CliID, fecha: DateTime.Now.ToString("yyyy-MM-dd"));

            var balance = cliData.Balance + montoAbono;
            
            var montoMinimoPagar = balance * (parPagoMinimo / 100.0);

            DatosPedido = myProd.GetTempTotales((int)Modules.VENTAS, true, true);
            var montoPedido = Resumen.Total;
            var conIdPedido = Arguments.Values.CurrentModule == Modules.VENTAS ? CurrentPedidoAEntregar != null ? CurrentPedidoAEntregar.ConID : CurrentCondicionPago != null ? CurrentCondicionPago.ConID : -1 : -1;
            var conIdContado = myParametro.GetParConIdFormaPagoContado();
            var montoMaximoFacturar = 0.0;

            var montoAdicionalPedido = 0.0;
            var montoAdicionalLimiteCredito = 0.0;

            if (parAdicionalPedido > 0)
            {
                montoAdicionalPedido = montoPedido * (parAdicionalPedido / 100.0);
            }

            if(parAdicionalLimiteCredito > 0)
            {
                montoAdicionalLimiteCredito = cliData.LimiteCredito * (parAdicionalLimiteCredito / 100.0);
            }

            if (montoAbono >= montoMinimoPagar && conIdPedido == conIdContado)
            {
                montoMaximoFacturar = montoPedido + montoAdicionalPedido;
            }else if (montoAbono < montoMinimoPagar)
            {
                montoMaximoFacturar = 0;
            }else if(conIdPedido != conIdContado && Math.Round(montoAbono, 2) == Math.Round(balance, 2))
            {
                if(montoPedido < montoMinimoPagar)
                {
                    montoMaximoFacturar = montoPedido + montoAdicionalPedido;
                }
                else
                {
                    montoMaximoFacturar = cliData.LimiteCredito + montoAdicionalLimiteCredito;
                }
            }else if(conIdPedido != conIdContado && Math.Round(montoAbono, 2) >= Math.Round(montoMinimoPagar, 2) && montoAbono < balance)
            {
                if ((montoAbono - montoMinimoPagar) > montoPedido)
                {
                    montoMaximoFacturar = montoPedido + montoAdicionalPedido;
                }
                else
                {
                    montoMaximoFacturar = montoAbono - montoMinimoPagar;
                }
            }

            //if(montoMaximoFacturar < (Resumen.Total==0 ? DatosPedido.Total : Resumen.Total) )
            if (montoMaximoFacturar < Resumen.Total && CurrentPedidoAEntregar != null )
            {
               
                throw new Exception(AppResource.SaleAmountIsGreaterThanAllowed + montoMaximoFacturar.ToString("N2"));
                
            }
            
        }

        private void BindFiltros()
        {
            try
            {
                FiltrosSource = new DS_FiltrosDinamicos().GetFiltrosProductos();

                if (FiltrosSource != null && FiltrosSource.Count > 0)
                {
                    FiltrosDinamicos item = null;
                    if (myParametro.GetParBusquedaCombinadaPorDefault())
                        item = FiltrosSource.Where(x => x.FilDescripcion == "Combinada").FirstOrDefault();
                    else
                        item = FiltrosSource.Where(x => x.FilIndicadorDefault).FirstOrDefault();

                    if (item == null)
                    {
                        item = FiltrosSource.FirstOrDefault();
                    }

                    if (item != null)
                    {
                        CurrentFilter = item;
                    }
                }

            }
            catch (Exception e)
            {
                DisplayAlert(e.Message, AppResource.Aceptar);
                Crashes.TrackError(e);
                if (FiltrosSource == null)
                {
                    FiltrosSource = new List<FiltrosDinamicos>()
                    {
                        new FiltrosDinamicos() { FilKey = "PRODFILTRO", FilCampo = "p.ProDescripcion", FilTipo = 1, FilCondicion = "LIKE", FilDescripcion = AppResource.Description },
                        new FiltrosDinamicos() { FilKey = "PRODFILTRO", FilCampo = "p.ProCodigo", FilTipo = 1, FilCondicion = "LIKE", FilDescripcion = AppResource.Code },
                        new FiltrosDinamicos() { FilKey = "PRODFILTRO", FilCampo = "p.LinID", FilTipo = 2, FilDescripcion = AppResource.Line, FilComboSelect = "select LinID as Key, LinDescripcion as Value FROM Lineas Order by LinDescripcion", FilCondicion = "=" }
                    };

                    CurrentFilter = FiltrosSource[0];
                }
            }
        }

        private void OnFilterValueSelected()
        {
            if (CurrentFilter == null)
            {
                return;
            }

            try
            {
                if (CurrentFilter.FilTipo == 2)
                {
                    ShowSecondFilter = true;
                    CurrentSecondFiltro = null;

                    SecondFiltroSource = Functions.DinamicQuery(CurrentFilter.FilComboSelect, true);

                    if (SecondFiltroSource != null && SecondFiltroSource.Count > 0)
                    {
                        CurrentSecondFiltro = SecondFiltroSource[0];
                    }
                }
                else
                {
                    CurrentSecondFiltro = null;
                    SecondFiltroSource = new List<KV>();
                    ShowSecondFilter = false;

                    BtnSearchLogo = "";
                }
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                DisplayAlert(AppResource.ErrorLoadingFilters, e.Message);
            }
        }

        private void BindDrawerMenu()
        {
            var msg = AppResource.Pedido;

            switch (Arguments.Values.CurrentModule)
            {
                case Modules.DEVOLUCIONES:
                    msg = AppResource.Return;
                    break;
                case Modules.INVFISICO:
                    msg = AppResource.Inventory;
                    break;
                case Modules.COTIZACIONES:
                    msg = AppResource.Quote;
                    break;
                case Modules.VENTAS:
                    msg = AppResource.Sale;
                    break;
                case Modules.CONTEOSFISICOS:
                    msg = AppResource.Count;
                    break;
                case Modules.COMPRAS:
                    msg = myParametro.GetParCambiarNombreComprasPorPushMoney() ? "PushMoney" : AppResource.Purchase;
                    break;
                case Modules.CAMBIOSMERCANCIA:
                    msg = AppResource.Change;
                    break;
                case Modules.TRASPASOS:
                    msg = AppResource.Transfer;
                    break;
                case Modules.PROMOCIONES:
                    msg = AppResource.Promotion;
                    break;
                case Modules.ENTREGASMERCANCIA:
                    msg = AppResource.Delivery;
                    break;
                case Modules.REQUISICIONINVENTARIO:
                    msg = AppResource.Requisition;
                    break;
                case Modules.COLOCACIONMERCANCIAS:
                    msg = AppResource.PlacementsOfMerchandise;
                    break;
            }

            MenuSource = new ObservableCollection<model.Internal.MenuItem>()
            {
                new model.Internal.MenuItem(){ Title = AppResource.GetOut, Icon = "ic_exit_to_app_black.png", Id = 0 },
                new model.Internal.MenuItem(){ Title = AppResource.Save + " " + msg, Icon = "ic_shopping_basket_black_24dp.png", Id = 1 },
                new model.Internal.MenuItem(){ Title = AppResource.ReviewTransaction, Icon = "ic_shopping_cart_black_24dp", Id = 2 }
            };

            if (FiltroOrdenarLista != null && FiltroOrdenarLista.Count > 0)
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.SortList, Icon = "baseline_filter_list_black_24", Id = 14 });

                if(FiltroOrdenarLista != null && FiltroOrdenarLista.Count > 0)
                {

                    var def = FiltroOrdenarLista.Where(x => x.FilIndicadorDefault).FirstOrDefault();
                    if (def == null)
                    {
                        def = FiltroOrdenarLista.FirstOrDefault();
                    }
                   

                    if (def != null)
                    {
                        currentOrderBy = def.FilCampo.Trim().Equals(DS_RepresentantesParametros.GetInstance().GetParProdOrden().Trim()) || DS_RepresentantesParametros.GetInstance().GetParProdOrden().Equals("") ? def.FilCampo : DS_RepresentantesParametros.GetInstance().GetParProdOrden();
                    }
                }
            }

            if (myParametro.GetParPedidosCambiarDiseñoRow())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.RowDesign, Icon = "ic_tune_black_24dp", Id = 3 });
            }                      

            if (myParametro.GetParPedidosConsultarOfertas() || myParametro.GetParVentasConsultarOfertas() || myParametro.GetParCotizacionesConsultarOfertas())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.CheckOffers, Icon = "ic_add_shopping_cart_black_24dp", Id = 4 });
            }

            if (myParametro.GetParConsultarOfertasGenerales())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = "Consultar Ofertas Generales", Icon = "ic_chrome_reader_mode_black_24dp", Id = 17 });
            }

            if (Arguments.Values.CurrentModule == Modules.PEDIDOS 
                || Arguments.Values.CurrentModule == Modules.VENTAS 
                || Arguments.Values.CurrentModule == Modules.PROMOCIONES
                || Arguments.Values.CurrentModule == Modules.ENTREGASMERCANCIA)
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.CheckDiscounts, Icon = "ic_monetization_on_black_24dp.png", Id = 5 });
            }

            if (myParametro.GetParPedidosHistoricoFacturas())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.HistoricalInvoices, Icon = "ic_receipt_black_24dp", Id = 6 });
            }

            if (myParametro.GetParHistoricosPedidos())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.OrderHistory, Icon = "ic_receipt_black_24dp", Id = 7 });
            }

            if (myParametro.GetParHistoricosPromedio() && (Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.VENTAS))
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.HistoricalAverage, Icon = "ic_assessment_black_24dp", Id = 8 });
            }

            if(myParametro.GetParConsultaInventarioFisico() && Arguments.Values.CurrentModule == Modules.PEDIDOS)
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.CheckPhysicalInventories, Icon = "ic_archive_black_24dp", Id = 16 });
            }

            if (myParametro.GetParPedidosMostrarPresupuesto())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.Budget, Icon = "ic_assessment_black_24dp", Id = 15 });
            }

            if (myParametro.GetParCambiarVisualizacionProductos())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.ChangeVisualization, Icon = "ic_view_quilt_black_24dp", Id = 9 });
            }

            if (myParametro.GetParProductoNoVendido() > 0 && (Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.VENTAS))
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.UnSoldProducts, Icon = "ic_remove_shopping_cart_black_24dp", Id = 10 });
            }

            if (myParametro.GetParVentasCalculadoraDeNegociacion(false))
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.TradingCalculator, Icon = "ic_monetization_on_black_24dp", Id = 11 });
            }

            if (myParametro.GetParDevolucionesDevolverFacturaCompleta())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.ReturnFullInvoice, Icon = "ic_receipt_black_24dp", Id = 12 });
            }

            if (myParametro.GetParOpcionesDesarrollador())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = "Insertar productos para pruebas", Icon = "ic_add_shopping_cart_black_24dp", Id = 13 });
            }
        }

        private async void OnOptionItemSelected()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                if (SelectedItem == null) return;

                switch (SelectedItem.Id)
                {
                    case 0:
                        ClearTemp();
                        await PopAsync(true);
                        break;
                    case 1:
                        IsBusy = false;
                        GoPrepareSave();
                        break;
                    case 2:
                        IsBusy = false;
                        await Search(true);
                        break;
                    case 3:
                        //show alert diseño row
                        IsBusy = false;

                        string rowItem = "";
                        switch (myParametro.GetFormatoVisualizacionProductosLocal())
                        {
                            case 1:
                                rowItem = "Diseño No.1";
                                break;
                            case 2:
                                rowItem = "Diseño No.2";
                                break;
                            case 3:
                                rowItem = "Diseño No.3";
                                break;
                            case 5:
                                rowItem = "Diseño No.5";
                                break;
                            case 6:
                                rowItem = "Diseño No.6";
                                break;
                            case 7:
                                rowItem = "Diseño No.7";
                                break;
                            case 9:
                                rowItem = "Diseño No.9";
                                break;
                            case 10:
                                rowItem = "Diseño No.10";
                                break;
                            case 11:
                                rowItem = "Diseño No.11";
                                break;
                            case 12:
                                rowItem = "Diseño No.12";
                                break;
                            case 13:
                                rowItem = "Diseño No.13";
                                break;
                            case 19:
                                rowItem = "Diseño No.19";
                                break;
                            case 20:
                                rowItem = "Diseño No.20";
                                break;
                            case 26:
                                rowItem = "Diseño No.26";
                                break;
                            case 28:
                                rowItem = "Diseño No.28";
                                break;
                            case 29:
                                rowItem = "Diseño No.29";
                                break;
                            case 15:
                                rowItem = "Diseño Grid";
                                break;
                            default:
                                rowItem = " ";
                                break;
                        }

                        await App.Current.MainPage.Navigation.PushPopupAsync(new PopupDesignRow(rowItem) { OnOptionItemSelected = (item) =>
                        {
                            if (item != -1)
                            {
                                OnRowDesignChanged?.Invoke(this, item);
                            }
                        }});                        
                        /*var row = await Functions.ShowAlertChangeRowDesign();

                        if (row != -1)
                        {
                            OnRowDesignChanged?.Invoke(this, row);
                        }*/
                        break;

                    case 4://consulta ofertas
                        if (myParametro.GetParConsultarOfertasGenerales())
                        {
                            dialogConsultaOfertas = new ConsultarOfertasModal();
                            dialogConsultaOfertas.LoadOfertas();
                        }
                        else if (dialogConsultaOfertas == null)
                        {
                            dialogConsultaOfertas = new ConsultarOfertasModal();
                            dialogConsultaOfertas.LoadOfertas();
                        }
                        await PushModalAsync(dialogConsultaOfertas);
                        break;
                    case 5: //consulta descuentos
                        if (dialogConsultaDescuentos == null)
                        {
                            dialogConsultaDescuentos = new ConsultaDescuentosModal((int)Arguments.Values.CurrentModule);
                        }

                        await PushModalAsync(dialogConsultaDescuentos);
                        break;
                    case 6: //historico factura
                        await PushAsync(new HistoricoFacturasPage(false));
                        break;
                    case 7: //historico de pedidos
                        await PushAsync(new HistoricoFacturasPage(true));
                        break;
                    case 8://HISTORICO PROMEDIO
                        await PushAsync(new HistoricoPromedioPage());
                        IsBusy = false;
                        break;
                    case 9: //cambiar visualizacion grid productos
                        IsBusy = false;
                        ChangeVisualizacionColumnas();
                        break;
                    case 10:
                        //Productos No vendidos by cliente
                        await PushAsync(new ProductosNoVendidosPage());
                        break;
                    case 11:
                        await PushModalAsync(new CalculadoraNegociacionModal(myProd, Arguments.Values.CurrentModule == Modules.VENTAS ? CurrentPedidoAEntregar != null ? CurrentPedidoAEntregar.ConID : CurrentCondicionPago != null ? CurrentCondicionPago.ConID : -1 : -1));
                        break;
                    case 12:
                        await PushModalAsync(new DevolverFacturaCompletaModal(() => { SearchUnAsync(true); }));
                        break;
                    case 13:
                        IsBusy = false;
                        AlertInsertarProductosParaPruebas();
                        break;
                    case 14: //ordenar lista
                        await ShowAlertOrdenarLista();
                        break;
                    case 15: //presupuesto
                        await PushAsync(new PresupuestosPage(Arguments.Values.CurrentClient.CliID, true, CurrentMoneda));
                        break;
                    case 16://consulta inventario fisico
                        FromConsultInventory = true;
                        await PushAsync(new ConsultaInventarioFisicoPage());
                        break;
                    case 17://consulta ofertas general
                        dialogConsultaOfertas = new ConsultarOfertasModal();
                        dialogConsultaOfertas.LoadOfertas(isGeneral:true);
                        await PushModalAsync(dialogConsultaOfertas);
                        break;
                }

                OnOptionMenuItemSelected?.Invoke();

            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                await DisplayAlert(AppResource.Error, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
        }

        public bool FromConsultInventory { get; set; } = false;

        private string currentOrderBy = null;
        private async Task ShowAlertOrdenarLista()
        {
            if (FiltroOrdenarLista == null)
            {
                return;
            }

            var list = new List<string>();

            foreach (var filtro in FiltroOrdenarLista)
            {
                list.Add(filtro.FilDescripcion);
            }

            var result = await DisplayActionSheet(AppResource.ChooseTheOrder, buttons: list.ToArray());

            var item = FiltroOrdenarLista.Where(x => x.FilDescripcion == result).FirstOrDefault();

            if (item == null)
            {
                return;
            }

            IsBusy = false;
            if (!myParametro.GetParProdOrden().Equals(""))
            {
                Functions.UpdateParametroProOrden(item.FilCampo, Functions.GetrowguidParametro("PRODORDEN"));
            }
            await Search(LastTimeWasResume, null, item.FilCampo);
        }

        private void AlertInsertarProductosParaPruebas()
        {
            var dialog = DependencyService.Get<IDialogInput>();
            dialog.Show("Insertar productos", "Digite la cantidad de productos a insertar", async (result) => 
            {
                try
                {
                   /* if (IsBusy)
                    {
                        return;
                    }*/

                    IsBusy = true;

                    if (!double.TryParse(result, out double cantidad))
                    {
                        await DisplayAlert(AppResource.Warning, "La cantidad introducida no es valida");
                        return;
                    }

                    var segundos = 0.0;

                    await Task.Run(() => 
                    {
                        myProd.InsertProductosParaPruebas((int)cantidad, out segundos);
                    });

                    IsBusy = false;

                    await DisplayAlert(AppResource.Warning, "El total de segundos fueron: " + segundos);

                    SearchUnAsync(true);
                }catch(Exception e)
                {
                   await DisplayAlert(AppResource.Warning, e.Message);
                }

                IsBusy = false;

            }, Keyboard.Numeric);
        }

        private async void ChangeVisualizacionColumnas()
        {
            var Id = await DisplayActionSheet(AppResource.ChooseVisualizationForm, buttons: new string[] { AppResource.ThreeColumns, AppResource.TwoColumns, AppResource.OneColumn, AppResource.Lines, AppResource.Catalog });

            IsBusy = false;


            if(Id == AppResource.ThreeColumns)
            {
                OnRowDesignChanged?.Invoke(this, 15);
            }
            else if(Id == AppResource.TwoColumns)
            {
                OnRowDesignChanged?.Invoke(this, 16);
            }
            else if(Id == AppResource.OneColumn){
                OnRowDesignChanged?.Invoke(this, 17);
            }
            else if(Id == AppResource.Lines)
            {
                OnRowDesignChanged?.Invoke(this, myParametro.GetFormatoVisualizacionProductos());
            }
            else if(Id == AppResource.Catalog)
            {
                OnRowDesignChanged?.Invoke(this, 20);
            }
        }

        public void SaveVisualizacion(int Id)
        {
            myParametro.SaveFormatoVisualizacionProductos(Id);
            IsBusy = false;
        }

        private void SetResumenTotales()
        {
            Resumen = myProd.GetTempTotales((int)Arguments.Values.CurrentModule);
            Resumen.NumeroTransaccion = NumeroTransaccion;
        }

        public void DeleteOfertaAndDescuentosInTemp()
        {
            if (CurrentPedidoAEntregar == null)
            {
                if (!ParOfertasManuales)
                {
                    myProd.DeleteOfertaInTemp((int)Arguments.Values.CurrentModule);
                }
                myProd.EliminarDescuentoInTemp((int)Arguments.Values.CurrentModule);
            }

            myProd.ReiniciarCantidadOfertaRebajaVenta();
        }

        private bool ReloadByPromotion = false;
        public async void OnListItemSelected(ProductosTemp producto)
        {
            if (producto == null || IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                CurrentProducto = producto;

                var item = myProd.GetById(CurrentProducto.ProID);
                if (item != null)
                {
                    if (!string.IsNullOrWhiteSpace(item.ProDatos3) &&  item.ProDatos3.ToUpper().Contains("C"))
                    {
                        if (!string.IsNullOrWhiteSpace(Arguments.Values.CurrentClient.CliDatosOtros) && !Arguments.Values.CurrentClient.CliDatosOtros.ToUpper().Contains("O"))
                        {
                            throw new Exception(AppResource.UnLicenseClientForControlledProduct);
                        }
                    }   
                }
                

                if (Arguments.Values.CurrentModule == Modules.DEVOLUCIONES)
                {
                    dialogDevProducto = new AgregarProductoDevolucionModal(CurrentMotivoDevolucion)
                    {
                        OnAceptar = AgregarProductoDevolucion
                    };
                    dialogDevProducto.CurrentProducto = producto.Copy();
                    await PushModalAsync(dialogDevProducto);
                    IsBusy = false;
                    return;
                }
                else
                {
                    dialogAgregarProducto = new AgregarProductosModal(myProd, CurrentPedidoAEntregar != null && Arguments.Values.CurrentModule == Modules.VENTAS, CurrentCentrosDistribucion ?? null, UseMultiEntrega)
                    {
                        IsEntregandoTraspaso = IsEntregandoTraspaso,
                        OnCantidadAccepted = (s) =>
                        {

                            if((myParametro.GetParPedidosProductosColoresYTamanos() || myParametro.GetParPedidosProductosUnidades()) && s.ProductToAdd != null)
                            {
                                CurrentProducto = s.ProductToAdd;
                            }

                            var PrecioLista = CurrentProducto.Precio;
                            var PrecioListaConImpuesto = PrecioLista * ((CurrentProducto.Itbis / 100) + 1);

                            CurrentProducto.Cantidad = s.Cantidad;
                            CurrentProducto.CantidadDetalle = s.Unidades;
                            CurrentProducto.InvAreaId = s.InvArea;
                            CurrentProducto.InvAreaDescr = s.InvAreaDescr;
                            CurrentProducto.PrecioTemp = s.Precio;
                            CurrentProducto.IndicadorDocena = s.IndicadorDocena;
                            CurrentProducto.CantidadFacing = s.Facing;
                            CurrentProducto.ProAtributo1 = s.Atributo1?.Key;
                            CurrentProducto.ProAtributo2 = s.Atributo2?.Key;
                            CurrentProducto.ProAtributo1Desc = s.Atributo1?.Value;
                            CurrentProducto.ProAtributo2Desc = s.Atributo2?.Value;
                            CurrentProducto.CedCodigo = s.CedCodigo;
                            CurrentProducto.CedDescripcion = s.CedDescripcion;

                            CurrentProducto.IndicadorOfertaForShow = CurrentProducto.IndicadorOferta;

                            if (myParametro.GetParInventariosTomarCantidades() == 1 || myParametro.GetParInventariosTomarCantidades() == 3 || myParametro.GetParColocacionProductosTomarCantidades() == 1)
                            {
                                CurrentProducto.CanTidadGond = s.CanTidadGond;
                                CurrentProducto.CantidadAlm = s.CantidadAlm;
                                CurrentProducto.CanTidadTramo = s.CanTidadTramo;
                            }
                            else if (myParametro.GetParInventariosTomarCantidades() == 2 || myParametro.GetParColocacionProductosTomarCantidades() == 2)
                            {
                                CurrentProducto.CanTidadGond = s.CanTidadGond;
                                CurrentProducto.CantidadAlm = s.CantidadAlm;
                                CurrentProducto.UnidadGond = s.UnidadGond;
                                CurrentProducto.UnidadAlm = s.UnidadAlm;
                            }

                            if(!string.IsNullOrWhiteSpace(Arguments.Values.CurrentClient.CliTipoComprobanteFAC) && Arguments.Values.CurrentClient.CliTipoComprobanteFAC == "14" && (Arguments.Values.CurrentModule == Modules.VENTAS || Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.COTIZACIONES))
                            {
                                CurrentProducto.Itbis = 0;
                            }

                            if (UseMultiEntrega)
                            {
                                CurrentProducto.PedFechaEntrega = s.FechaEntrega;
                            }

                            if(s.MotId != -1)
                            {
                                CurrentProducto.MotIdDevolucion = s.MotId;
                            }

                            //if (CurrentPedidoAEntregar != null && CurrentProducto.IndicadorOferta)
                            //{
                            //    CurrentProducto.IndicadorOferta = false;
                            //}

                            CurrentProducto.CantidadOferta = myParametro.GetParCotOfertasManuales() || myParametro.GetParVenOfertasManuales() || myParametro.GetParPedOfertasManuales() ? s.CantidadOfertaManual : 0;
                            
                            CurrentProducto.CantidadPiezas = s.CantidadPiezas;                            

                            if (myParametro.GetParCambioMercanciaInsertarLotesParaRecivir())
                            {
                                CurrentProducto.LoteRecibido = s.LoteRecibido;
                                CurrentProducto.LoteEntregado = s.LoteEntregado;
                            }else
                            {
                                CurrentProducto.Lote = s.Lote;
                            }

                            if (myParametro.GetParRevenimiento())
                            {
                                CurrentProducto.CantidadDetalleR = s.CantidadDetalleR;
                            }

                            if (CurrentPedidoAEntregar != null)
                            {
                                if (!ValidarCantidadMultiAlmacen())
                                {
                                    return;
                                }
                            }

                            if (ParOfertasManuales && s.ProductoOferta != null)
                            {
                                CurrentProducto.ProIDOferta = s.ProductoOferta.ProID;
                            }
                            if (s.Precio > 0 && Arguments.Values.CurrentModule != Modules.INVFISICO)
                            {
                                CurrentProducto.Precio = s.Precio;
                            }

                            CurrentProducto.IndicadorEliminar = s.IndicadorEliminar;
                            CurrentProducto.DesPorcientoManual = s.DescuentoManual;
                            CurrentProducto.ValorOfertaManual = s.ValorOfertaManual;

                            if (ParOfertasManuales && s.ProductoOferta != null)
                            {
                                CurrentProducto.CantidadOferta = s.CantidadOfertaManual;
                                CurrentProducto.ProIDOferta = s.ProductoOferta.ProID;
                            }

                            if (Functions.IsCompraFactura || myParametro.GetParComprasUsarFacturas())
                            {
                                CurrentProducto.Documento = s.ComprasNoFactura;
                            }

                            if (s.IndicadorPromocion)
                            {
                                CurrentProducto.ShowDescuento = false;
                            }else if (CurrentProducto.DesPorcientoManual > 0)
                            {
                                CurrentProducto.Descuento = (CurrentProducto.DesPorcientoManual * CurrentProducto.Precio) / 100;
                                CurrentProducto.DesPorciento = CurrentProducto.DesPorcientoManual;
                                CurrentProducto.ShowDescuento = true;
                            }
                            else if (myParametro.GetParPedDescLip())
                            {
                                CurrentProducto.Descuento = s.DescuentoXLipCodigo;
                                CurrentProducto.DesPorciento = Math.Round(((s.DescuentoXLipCodigo / CurrentProducto.Precio)) * 100, 4, MidpointRounding.AwayFromZero);
                                CurrentProducto.ShowDescuento = true;
                            }
                            else if(!myParametro.GetParDescuentosProductosMostrarPreview())
                            {                                
                                CurrentProducto.Descuento = 0;
                                CurrentProducto.DesPorciento = 0;
                                CurrentProducto.ShowDescuento = false;
                            }

                            //if (myParametro.GetDescuentoxPrecioNegociado())
                            //{
                            //    CurrentProducto.PrecioTemp = s.Precio;
                            //    if (PrecioLista > 0 && Arguments.Values.CurrentModule != Modules.INVFISICO)
                            //    {
                            //        CurrentProducto.Precio = PrecioLista;
                            //    }

                            //    CurrentProducto.Descuento = PrecioLista - s.Precio;
                            //    CurrentProducto.DesPorciento = (PrecioLista - s.Precio) / 100;
                            //    CurrentProducto.ShowDescuento = true;
                            //}

                            if (myParametro.GetDescuentoxPrecioNegociado())
                            {
                                CurrentProducto.PrecioTemp = s.Precio;
                                if (PrecioLista > 0 && Arguments.Values.CurrentModule != Modules.INVFISICO)
                                {
                                    CurrentProducto.Precio = PrecioLista;
                                }

                                if (myParametro.GetParPedidosEditarPrecioNegconItebis())
                                {
                                    var precionegociadosinimpuesto = s.Precio;// (s.Precio / ((CurrentProducto.Itbis / 100) + 1));
                                    CurrentProducto.Descuento = (PrecioLista) - (precionegociadosinimpuesto);
                                    CurrentProducto.DesPorciento = (PrecioLista - precionegociadosinimpuesto) / 100;
                                    CurrentProducto.ShowDescuento = true;
                                }
                                else
                                {
                                    CurrentProducto.Descuento = (PrecioLista) - (s.Precio);
                                    CurrentProducto.DesPorciento = (PrecioLista - s.Precio) / 100;
                                    CurrentProducto.ShowDescuento = true;
                                }
                            }

                            /*if (Arguments.Values.CurrentModule == Modules.PEDIDOS && CurrentProducto.IndicadorPromocion != s.IndicadorPromocion && !string.IsNullOrWhiteSpace(CurrentProducto.rowguid))
                            {
                                myProd.DeleteTempByRowguid(CurrentProducto.rowguid);
                            }*/

                            if (CurrentProducto.IndicadorPromocion != s.IndicadorPromocion && !string.IsNullOrWhiteSpace(CurrentProducto.rowguid) && myProd.ExistsInTemp(CurrentProducto.ProID, (int)Arguments.Values.CurrentModule, s.IndicadorPromocion))
                            {
                                ReloadByPromotion = true;
                            }

                            CurrentProducto.IndicadorPromocion = s.IndicadorPromocion;

                          /*  if (Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.VENTAS || Arguments.Values.CurrentModule == Modules.COTIZACIONES)
                            {
                                CurrentProducto.ConID = CurrentCondicionPago.ConID;
                            }*/

                            AgregarProducto(CurrentProducto);  //Se inserta primero el producto seleccionado y luego la oferta manual si existe

                            if (s.ProductoOferta == null && CurrentProducto.ProIDOferta > 0 && myProd.ExistsOfertaInTemp(CurrentProducto.ProID, (int)Arguments.Values.CurrentModule))
                            {
                                myProd.DeleteOfertaInTemp((int)Arguments.Values.CurrentModule, CurrentProducto.ProID, CurrentProducto.ProIDOferta, false, UnmCodigo: (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? CurrentProducto.UnmCodigo : ""));
                            }

                            if (ParOfertasManuales)
                            {
                                if (s.ProductoOferta != null)
                                {
                                    CurrentProducto.PrecioTemp = 0;
                                    CurrentProducto.CantidadOferta = 0;

                                    if (myProd.ExistsOfertaInTemp(CurrentProducto.ProID, (int)Arguments.Values.CurrentModule))
                                    {
                                        myProd.DeleteOfertaInTemp((int)Arguments.Values.CurrentModule, CurrentProducto.ProID, -1, false, UnmCodigo: (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? CurrentProducto.UnmCodigo : ""));
                                    }
                                    if (myProd.ExistsOfertaInTemp(CurrentProducto.ProIDOferta, (int)Arguments.Values.CurrentModule))
                                    {
                                        myProd.DeleteOfertaInTemp((int)Arguments.Values.CurrentModule, CurrentProducto.ProIDOferta, -1, false, UnmCodigo: (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? CurrentProducto.UnmCodigo : ""));
                                    }

                                    if ((CurrentProducto.Cantidad > 0 || CurrentProducto.CantidadDetalle > 0) || Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS)
                                    {
                                        myProd.InsertInTemp(s.ProductoOferta, true);
                                    }
                                }

                                //CurrentProducto.CantidadOferta = s.CantidadOfertaManual;
                            }

                            var parBlockCondicionPago = DS_RepresentantesParametros.GetInstance().BloquearCondicionPago();
                            if (parBlockCondicionPago == 1 || parBlockCondicionPago == 2)
                            {
                                //Deshabilita la condicion de pago del tab Configuracion de pedidos si cumple la condicion
                                if ((DS_RepresentantesParametros.GetInstance().BloquearCondicionPago() == 1 && HasProductsInTemp()
                                       && (Arguments.Values.CurrentModule == Modules.VENTAS || Arguments.Values.CurrentModule == Modules.PEDIDOS))
                                       || (DS_RepresentantesParametros.GetInstance().BloquearCondicionPago() == 2 && Arguments.Values.CurrentModule == Modules.VENTAS)
                                       || (CurrentPedidoAEntregar != null && Arguments.Values.CurrentModule == Modules.VENTAS))
                                {
                                    EnableCondicionPago = false;
                                }
                                else { EnableCondicionPago = true; }
                            }

                            if (condicionPagoAutorizada)
                            {
                                EnableCondicionPago = true; 
                            }

                            var usarColoresYTamanos = myParametro.GetParPedidosProductosColoresYTamanos();

                            var rawCode = CurrentProducto.ProCodigo.Split('-');

                            if(!(rawCode.Length > 2))
                            {
                                usarColoresYTamanos = false;
                            }

                            if (!usarColoresYTamanos)
                            {
                                CurrentProducto = null;//Se pone null aqui en vez de hacerlo en AgregarProducto(CurrentProducto)
                            }
                            
                            if(myParametro.GetParPedidosSearchAutomatico())
                            {
                                SearchUnAsync(true);
                            }else if(myParametro.GetParNoShowProInTemp() || myParametro.GetParProdUseUnmCodigo())
                            {
                                if (!string.IsNullOrWhiteSpace(LastValueSearch))
                                {
                                    SearchValue = LastValueSearch;
                                }
                                SearchUnAsync(false);
                            }

                            if (ReloadByPromotion && !usarColoresYTamanos)
                            {
                                ReloadByPromotion = false;
                                SearchUnAsync(LastTimeWasResume);
                            }
                        }
                    };
                }

                dialogAgregarProducto.CurrentProduct = producto;
                dialogAgregarProducto.IsDppActive = IsDpp;
                dialogAgregarProducto.IsBusy = false;

                await PushAsync(dialogAgregarProducto);

            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        private bool ValidarDatosDevolucion(DevolucionesProductosArgs args)
        {
            if (Arguments.Values.CurrentModule != Modules.DEVOLUCIONES)
            {
                return true;
            }

            var cantidadMaxima = myParametro.GetParDevolucionesCantidadMaximaProductos();

            if (cantidadMaxima > 0 && myProd.GetCantidadProductosAgregadosSinOfertas((int)Arguments.Values.CurrentModule) > cantidadMaxima && (args.cantidad > 0 || args.cantidaddetalle > 0))
            {
                DisplayAlert(AppResource.Warning, AppResource.CannotExceedItemsLimitsToReturn + cantidadMaxima.ToString(), AppResource.Aceptar);
                return false;
            }

            if (myParametro.GetParDevolucionesCantidadOUnidades())
            {
                if (args.cantidad > 0 && args.cantidaddetalle > 0)
                {
                    DisplayAlert(AppResource.Warning, AppResource.CannotSpecifyQuantityAndUnitsAtSameTime);
                    return false;
                }
            }

            if (myParametro.GetParDevolucionesValidarCantidadDetalleConProUnidades())
            {
                if (args.cantidaddetalle > CurrentProducto.ProUnidades)
                {
                    DisplayAlert(AppResource.Warning, AppResource.QuantityInUnitsExceedTheProductsUnits + CurrentProducto.ProUnidades);
                    return false;
                }
            }

            var diaslimite = myParametro.GetParDevolucionesDiasVencimiento();

            if (diaslimite > 0)
            {
                if (!myDev.ValidarFechaVencimientoContraPoliticaDevolucion(CurrentProducto.ProID, args.FechaVencimiento))
                {
                    DisplayAlert(AppResource.Warning, AppResource.ExpirationDateNotMeetReturnPolicy);
                    return false;
                }
            }

            return true;
        }

        public void AgregarProductoDevolucion(DevolucionesProductosArgs args)
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            if (!ValidarDatosDevolucion(args))
            {
                IsBusy = false;
                return;
            }

            CurrentProducto.Cantidad = args.cantidad;
            CurrentProducto.CantidadDetalle = args.cantidaddetalle;
            CurrentProducto.CantidadOferta = args.cantidadoferta;
            CurrentProducto.Accion = args.Accion;
            CurrentProducto.MotIdDevolucion = args.MotId;
            CurrentProducto.Documento = args.Documento;
            CurrentProducto.FechaVencimiento = args.FechaVencimiento;
            CurrentProducto.Lote = args.lote;

            var UseCondicion =  myParametro.GetParDevolucionCondicion();
            if (UseCondicion)
            {
                CurrentProducto.DevCondicion = args.Condicion;
            }
            

            AgregarProducto(CurrentProducto);

            if (myParametro.GetParNoShowProInTemp() || myParametro.GetParProdUseUnmCodigo())
            {
                if (!string.IsNullOrWhiteSpace(LastValueSearch))
                {
                    SearchValue = LastValueSearch;
                }
                SearchUnAsync(false);
            }
        }

        public void AgregarProducto(ProductosTemp producto, bool clearSearch = true)
        {
            try
            {
                var item = producto.Copy();

                if(CurrentPedidoAEntregar == null)
                {
                    item.IndicadorOferta = false;
                }               

                bool indicadorOferta = producto.IndicadorOferta;

                if (UseMotivoDevolucion && CurrentMotivoDevolucion != null)
                {
                    item.MotIdDevolucion = CurrentMotivoDevolucion.MotID;
                }

                if (CurrentPedidoAEntregar != null && indicadorOferta)
                {
                    item.IndicadorOferta = false;
                }

                if (UseCondicionDevolucion && CurrentCondicionDevolucion != null)
                {
                    item.DevCondicion = CurrentCondicionDevolucion.CodigoUso;
                }

                if (item.IndicadorPromocion)
                {                   
                    if (!Guardado)
                    {
                        item.PrecioSaved = item.Precio;
                        item.Precio = 0;
                        Guardado = true;
                    }
                    else
                    {
                        item.Precio = 0;
                    }
                    
                }

                if (myParametro.GetParVentasLotesAutomaticos() > 0)
                {
                    item.Lote = "";
                }

                item.RepSupervisor = this.repAuditor;

                if(!myParametro.GetParCantInvAlmacenes())
                {
                    item.AlmID = AlmID;
                }

                myProd.InsertInTemp(item, isEntrega:CurrentPedidoAEntregar != null && Arguments.Values.CurrentModule == Modules.VENTAS, IsMultiEntrega:UseMultiEntrega);

                item.IndicadorEliminar = false;
                item.IndicadorOferta = indicadorOferta;

                var index = Productos.IndexOf(producto);

                if (index != -1)
                {
                    Productos[index] = item;
                }

                if (item.CantidadOferta > 0 && (myParametro.GetParCotOfertasManuales() || myParametro.GetParVenOfertasManuales() || myParametro.GetParPedOfertasManuales()))
                {
                    var itemOfe = producto.Copy();
                    itemOfe.Cantidad = item.CantidadOferta;
                    itemOfe.CantidadDetalle = 0;
                    item.CantidadOferta = item.CantidadOferta;
                    itemOfe.rowguid = null;
                    itemOfe.IndicadorOferta = true;
                    if (myParametro.GetParOfertasManualesConDescuento100Porciento())
                    {
                        itemOfe.DesPorciento = 100;
                        itemOfe.Descuento = itemOfe.Precio;
                    }
                    else
                    {
                        itemOfe.Precio = 0;
                        itemOfe.DesPorciento = 0;
                        itemOfe.Descuento = 0;
                    }
                    item.rowguid = Guid.NewGuid().ToString();
                    myProd.InsertInTemp(itemOfe, byOferta:true);

                }

                SetResumenTotales();

                // CurrentProducto = null;

                if (clearSearch && !myParametro.GetBuscarProductosAlEscribir())
                {
                    SearchValue = "";
                }

                if((Arguments.Values.CurrentModule == Modules.VENTAS || myParametro.GetParConteosFisicosLotes() > 0) && item.UsaLote)
                {
                    SearchUnAsync(true);
                }
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                DisplayAlert(AppResource.ErrorAddingProduct, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
        }

        public void AumentarCantidadProducto(string proId, bool restar)
        {
            try
            {
                var item = Productos.Where(x => x.ProID.ToString() == proId).FirstOrDefault();

                if (item == null)
                {
                    return;
                }

                //var copy = item.Copy();

                if (restar)
                {
                    if (item.Cantidad >= 1)
                    {
                        item.Cantidad--;
                        AgregarProducto(item, false);
                    }
                }
                else
                {
                    item.Cantidad++;
                    AgregarProducto(item, false);
                }

                //Productos[Productos.IndexOf(item)] = item;

            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                DisplayAlert(AppResource.Warning, e.Message);
            }

        }

        public async void SearchUnAsync(bool resumen) { await Search(resumen); }

        private bool LastTimeWasResume = false;
       // public Task Search() { return Search(false); }
        public async Task Search(bool resumen, BusquedaAvanzadaProductosArgs filtroAvanzado = null, string orderBy = null)
        {
            if (IsBusy)
            {
                return;
            }


            if (myParametro.GetParPedidoMultiMoneda(true) && 
                (Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.COTIZACIONES)
                && string.IsNullOrWhiteSpace(Arguments.Values.CurrentClient.MonCodigo) 
                && CurrentMoneda == null)
            {
                await DisplayAlert(AppResource.Warning, AppResource.MustSelectCurrencyInConfigTab);
                try
                {
                    if (page is MasterDetailPage mdPage)
                    {
                        ((TabbedPage)((NavigationPage)mdPage.Detail).CurrentPage).CurrentPage = ((TabbedPage)((NavigationPage)mdPage.Detail).CurrentPage).Children[1];
                    }
                }catch(Exception e)
                {
                    Console.Write(e.Message);
                }

                return;
            }


            IsBusy = true;

            LastTimeWasResume = resumen;
            LastValueSearch = SearchValue;

            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                currentOrderBy = orderBy;
            }

            var args = new ProductosArgs
            {
                valueToSearch = SearchValue,
                lipCodigo = myParametro.GetParSectores() >= 2 && Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.LipCodigo : Arguments.Values.CurrentClient != null && !string.IsNullOrEmpty(Arguments.Values.CurrentClient.LiPCodigo) ? Arguments.Values.CurrentClient.LiPCodigo : "Default",
                filter = CurrentFilter,
                IdFactura = FacturaId,
                MonCodigo = UseMultiMoneda && CurrentMoneda != null ? CurrentMoneda.MonCodigo : Arguments.Values.CurrentClient.MonCodigo,
                FiltrarProductosPorSector = myParametro.GetParSectores() > 0 && myParametro.GetParFiltrarProductosPorSector(),
                useAlmacenDespacho = Arguments.Values.CurrentModule == Modules.VENTAS && CurrentPedidoAEntregar != null && myParametro.GetParUsarMultiAlmacenes(),
                IsEntregandoTraspaso = IsEntregandoTraspaso
            };

            if(Arguments.Values.CurrentModule == Modules.REQUISICIONINVENTARIO)
            {
                args.lipCodigo = "Default";
            }

            if (Arguments.Values.CurrentModule == Modules.COMPRAS)
            {
                if (IsPushMoneyRotacion)
                {
                    args.lipCodigo = myCli.GetLipCodigoPMR(Arguments.Values.CurrentClient.CliID);
                }
                else
                {
                    args.lipCodigo = Arguments.Values.CurrentClient.LipCodigoPM;
                }
            }

            args.orderBy = currentOrderBy;

            if (CurrentSecondFiltro != null)
            {
                args.secondFilter = CurrentSecondFiltro.Key;
            }

            if(myParametro.GetLineasRecordar() == 2 || myParametro.GetLineasRecordar() == 3)
            {
                var dataPedidosBusquedas = new PedidosBusquedas
                {
                    RepCodigo = Arguments.CurrentUser.RepCodigo,
                    PedSecuencia = NumeroTransaccion,
                    PedCampo = CurrentFilter.FilDescripcion,
                    PedTipo = CurrentFilter.FilTipo,
                    PedCodigo = CurrentSecondFiltro != null ? CurrentSecondFiltro.Key : "",
                    PedFiltro = CurrentSecondFiltro != null && CurrentFilter.FilTipo == 2 ? CurrentSecondFiltro.Value : SearchValue
                };

                myPed.GuardarPedidosBusquedas(dataPedidosBusquedas);
            }

            if ((CurrentPedidoAEntregar != null || myParametro.GetParNoVentasRancherasParaRepartidor()) && Arguments.Values.CurrentModule == Modules.VENTAS)
            {
                resumen = true; //que no permita agregar productos que no estan en la entrega si no el parametro de multialmacenes
                //y entonces???
            }

            screen?.KeepLightsOn(true);

            try
            {
                await Task.Run(() =>
                {
                    if (resumen)
                    {
                        if (ParOfertasManuales)
                        {
                            myProd.ActualizarPreciosMinInTemp();
                        }

                        if (myParametro.GetParShowOfertasAlAgregar())
                        {
                            Productos = new ObservableCollection<ProductosTemp>(myProd.GetResumenProductos((int)Arguments.Values.CurrentModule, false, true, true));

                            foreach (var prod in Productos)
                            {
                                if (prod.IndicadorOferta)
                                {
                                    prod.Cantidad = Productos.Where(p => !p.IndicadorOferta && p.ProID == prod.ProID).FirstOrDefault().Cantidad;
                                }
                            }
                        }
                        else
                        {
                            Productos = new ObservableCollection<ProductosTemp>(myProd.GetResumenProductos((int)Arguments.Values.CurrentModule, validarCantidades: !ParDevolucionesProductosFacturas, showValidForOfertas: (Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.VENTAS) && !ParOfertasManuales, showDescuentoIndicator: Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.VENTAS, isEntrega: CurrentPedidoAEntregar != null));
                        }
                        if (ShowAlmacen)
                        {
                            CurrentAlmacenForConteo = new DS_Almacenes().GetAlmacenById(Productos.Count > 0? Productos[0].AlmID : -1);
                        }
                    }
                    else
                    {
                        Productos = new ObservableCollection<ProductosTemp>(myProd.GetProductos(args, filtroAvanzado));
                    }
                });

            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                await DisplayAlert(AppResource.ErrorLoadingProducts, e.Message, AppResource.Aceptar);
            }

            if (Productos.Count > 0 && !myParametro.GetBuscarProductosAlEscribir())
            {
                SearchValue = "";
            }

            screen?.KeepLightsOn(false);
            IsBusy = false;
        }


        /// <summary>
        /// metodo para los centros de distribucion
        /// </summary>
        /// 
        public void SetCentroDisribucion()
        {

            if(Arguments.Values.CurrentModule == Modules.PEDIDOS && ShowCentrosDistribucion && CentrosDistribucion != null)
            {
                var centro = CentrosDistribucion.FirstOrDefault(c => c.CedCodigo.ToString() == myCli.GetClienteCedCodigoFromDetalleBySector(Arguments.Values.CurrentClient.CliID, Arguments.Values?.CurrentSector?.SecCodigo ?? ""));

                if (centro != null)
                {
                    CurrentCentrosDistribucion = centro;  
                }
            }

        }

        private bool useAuthorizePaymentTerm { get; set; }
        public bool UseAuthorizePaymentTerm { get => useAuthorizePaymentTerm; private set { useAuthorizePaymentTerm = value; RaiseOnPropertyChanged(); } }
        public async void SetCondicionPagoCliente(int conId = -1)
        {
            CondicionesPago = myConPago.GetAllCondicionesPago(myParametro.GetParCondicionesPagoClienteyContado(), Arguments.Values.CurrentClient.ConID);

            var condicion = CondicionesPago.Where(x => x.ConID == (conId == -1 ? Arguments.Values.CurrentClient.ConID : conId)).FirstOrDefault();

            var parBloqueoCondicionPago = myParametro.GetParBloqueoCondicionPago();  

            switch (parBloqueoCondicionPago)
            {
                case 1:
                    EnableCondicionPago = false;
                    break;
                case 2:
                    if (condicion != null)
                    {
                        var newCondiciones = CondicionesPago.Where(x => x.ConDiasVencimiento <= condicion.ConDiasVencimiento).ToList();
                        CondicionesPago = new ObservableCollection<CondicionesPago>(newCondiciones);
                        EnableCondicionPago = true;
                    }
                    break;
                case 3:
                    UseAuthorizePaymentTerm = true;
                    EnableCondicionPago = false;
                    break;
                case 0:
                case -1:
                    EnableCondicionPago = true;
                    break;
            }
            if (!string.IsNullOrEmpty(myProd.CondicionPagoInTemp()) && condicion == null) 
            {
                condicion = CondicionesPago.Where(x => x.ConID == int.Parse(myProd.CondicionPagoInTemp())).FirstOrDefault();
            }
            else if (myParametro.GetParConidDefault() > 0)
            {
                condicion = CondicionesPago.Where(x => x.ConID == myParametro.GetParConidDefault()).FirstOrDefault();
            }
            else if (myParametro.GetParConidContadoEn0())
            {
                condicion = CondicionesPago.Where(x => x.ConID == 0 ).FirstOrDefault();
            }
            else if (myParametro.GetParSectores() >= 2 && Arguments.Values.CurrentSector != null && conId == -1 && Arguments.Values.CurrentSector.ConID > 0)
            {
                condicion = CondicionesPago.Where(x => x.ConID == Arguments.Values.CurrentSector.ConID).FirstOrDefault();
            }

            if (condicion != null)
            {
                if(conId != -1)
                {
                    await Task.Delay(100);
                }

                CurrentCondicionPago = condicion;
               
            }

            if ((DS_RepresentantesParametros.GetInstance().BloquearCondicionPago() == 1 && HasProductsInTemp()
            && (Arguments.Values.CurrentModule == Modules.VENTAS || Arguments.Values.CurrentModule == Modules.PEDIDOS)) 
                || DS_RepresentantesParametros.GetInstance().BloquearCondicionPago() == 2 && Arguments.Values.CurrentModule == Modules.VENTAS)
            {
                EnableCondicionPago = false;
            }

            if (DS_RepresentantesParametros.GetInstance().BloquearCondicionPago() == 3 && Arguments.Values.CurrentModule == Modules.PEDIDOS)
            {
                EnableCondicionPago = false;
                if (CurrentCondicionPago?.ConDiasVencimiento == 0)
                    EnableProntoPago = false;
                else
                    EnableProntoPago = true;
            }

            if (condicionPagoAutorizada)
            {
                EnableCondicionPago = true;
            }
        }

        public async void SeleccionarProductosHistoricoFactura()
        {
            
            await PushModalAsync(new HistoricoFacturasPage(false, true) { onCancel = () => { PopAsync(true); }, onAceptarProductos = (id) => { FacturaId = id; SearchUnAsync(true); } });
            
        }

        public void ClearTemp()
        {
            try
            {
                myProd.ClearTemp((int)Arguments.Values.CurrentModule);
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                Console.Write(e.Message);
            }
        }

        public bool HasProductsInTemp() { return !myProd.NothingAddedInTemp((int)Arguments.Values.CurrentModule); }

        private bool isScanning;
        public async void GoScanQr()
        {

            try
            {
                if (isScanning)
                {
                    return;
                }

                isScanning = true;
                var options = new MobileBarcodeScanningOptions
                {
                    PossibleFormats = new List<ZXing.BarcodeFormat>() {
                    ZXing.BarcodeFormat.All_1D, ZXing.BarcodeFormat.CODABAR
                }
                };

                var scanner = new MobileBarcodeScanner
                {
                    UseCustomOverlay = false
                };

                var result = await scanner.Scan(options);

                if (result != null)
                {
                    SearchValue = result.Text;
                    SearchUnAsync(false);
                }
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            isScanning = false;

        }

        private ProductosCombosModal prodComboModal;
        private async void ShowProductosCombo(int proId)
        {
            try
            {
                if (IsBusy)
                {
                    return;
                }

                IsBusy = true;

                if (prodComboModal == null)
                {
                    prodComboModal = new ProductosCombosModal();
                }

                prodComboModal.LoadCombo(proId);

                await PushModalAsync(prodComboModal);
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        public void ClientesDetalleDireccion()
        {
            try
            {
                ClientesDireccionesDetalle = myCli.GetDireccionDetallada(CurrentCldDirTipo.CldDirTipo.ToString(), Arguments.Values.CurrentClient.CliID);


            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                DisplayAlert(AppResource.Warning, e.Message);
            }
        }


        private InventarioAlmacenesModal prodInventarioAlmacenModal;
        private async void ShowInventario(int proId)
        {
            try
            {
                if (IsBusy)
                {
                    return;
                }

                IsBusy = true;

                if (prodInventarioAlmacenModal == null)
                {
                    prodInventarioAlmacenModal = new InventarioAlmacenesModal();
                }

                prodInventarioAlmacenModal.LoadInvDisponible(proId);

                await PushModalAsync(prodInventarioAlmacenModal);
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        public bool HayPedidosPorEntregar()
        {
            return myEnt.HayPedidosPorEntregar(Arguments.Values.CurrentClient.CliID);
        }

        public async void OnButtonClicked(object sender, EventArgs args)
        {
            await PushModalAsync(new SeleccionarClienteConfiguracion()
            {
                OnValueSelected = (cliente) =>
                {
                    ClienteMaster = cliente;
                }
            });

        }

        public async void SeleccionarPedidoParaEntregar()
        {
            try
            {
                
                await PushModalAsync(new PedidosEntregarModal((pedido) => 
                {
                    try
                    {
                        
                        CurrentPedidoAEntregar = pedido;
                       
                        SetCondicionPagoCliente(pedido.ConID);

                        SearchUnAsync(true);
                    }catch(Exception e)
                    {
                        DisplayAlert(AppResource.Warning, e.Message);
                    }
                }
                ));
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }
        }

        public ObservableCollection<FormasPagoTemp> FormasPagoAgregadas { get => formaspagoagregadas; set { formaspagoagregadas = value; RaiseOnPropertyChanged(); } }
        private ObservableCollection<FormasPagoTemp> formaspagoagregadas = new ObservableCollection<FormasPagoTemp>();

        public async void OnComprasFormaPagoSelected(FormasPagoTemp item)
        {
            if(FormasPagoAgregadas == null)
            {
                return;
            }

            var result = await DisplayAlert(AppResource.Warning, AppResource.WantDeletePaymentwayQuestion, AppResource.Remove, AppResource.Cancel);

            if (result)
            {
                FormasPagoAgregadas.Remove(item);

                //TotalAPagar = GetTotalAPagar();
            }
        }

        private AgregarFormaPagoModal dialogAddFormaPago;
        private Enums.FormasPago CurrentFormaPago = Enums.FormasPago.Null;
        private async void ShowAlertAddComprasFormaPago()
        {
            try
            {
                var list = new List<string>();


                var formasPago = new DS_FormasPago().GetFormasPago();

                var validas = myParametro.GetParComprasFormasPagoValidas();

                foreach (var forma in formasPago)
                {
                    if (validas == null || validas.Length == 0 || validas.Contains(forma.FopID.ToString()))
                    {
                        list.Add(forma.FopDescripcion.Trim());
                    }
                }

                //list.Add("Efectivo");
                // list.Add("Bono");

                var result = await DisplayActionSheet(AppResource.ChoosePaymentway, buttons: list.ToArray());

                if (dialogAddFormaPago == null)
                {
                    dialogAddFormaPago = new AgregarFormaPagoModal(new DS_Recibos())
                    {
                        FillMonto = () =>
                        {
                            var montototal = Resumen.Total;

                            if(FormasPagoAgregadas != null)
                            {
                                montototal -= FormasPagoAgregadas.Sum(x => x.Prima);

                                if(montototal < 0)
                                {
                                    montototal = 0;
                                }
                            }
                            
                            return Math.Abs(montototal);
                        },
                        OnAccepted = AgregarComprasFormaPago
                    };
                }

                CurrentFormaPago = Enums.FormasPago.Null;

                switch (result)
                {
                    case "Cheque":
                        CurrentFormaPago = Enums.FormasPago.Cheque;
                        break;
                    case "Efectivo":
                        CurrentFormaPago = Enums.FormasPago.Efectivo;
                        break;
                    case "Transferencia":
                        CurrentFormaPago = Enums.FormasPago.Transferencia;
                        break;
                    case "Tarjeta Crédito":
                        CurrentFormaPago = Enums.FormasPago.TarjetaCredito;
                        break;
                    case "Retención":
                        CurrentFormaPago = Enums.FormasPago.Retencion;
                        break;
                    case "Orden de pago":
                        CurrentFormaPago = Enums.FormasPago.OrdenPago;
                        break;
                    case "Bono":
                        CurrentFormaPago = Enums.FormasPago.Bono;
                        break;
                    default:
                        return;
                }
                dialogAddFormaPago.CurrentFormaPago = CurrentFormaPago;

                var total = Resumen.Total;
                if(FormasPagoAgregadas != null)
                {
                    total -= FormasPagoAgregadas.Sum(x => x.Prima);

                    if(total < 0)
                    {
                        total = 0;
                    }
                }
                dialogAddFormaPago.MontoAPagar = total; //< 0 ? TotalAPagar : 0 ;
                await PushAsync(dialogAddFormaPago);

            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }
        }

        public void AgregarComprasFormaPago(AgregarFormaPagoArgs forma)
        {
            try
            {
                var montoapagar = Resumen.Total;

                if (FormasPagoAgregadas != null)
                {
                    montoapagar -= FormasPagoAgregadas.Sum(x => x.Prima);

                    if (montoapagar < 0)
                    {
                        montoapagar = 0;
                    }
                }

                if (CurrentFormaPago == Enums.FormasPago.Bono)
                {
                    double cantidadBonosValida = Math.Round(montoapagar / forma.BonoDenominacion);

                    if(forma.BonoCantidad > cantidadBonosValida)
                    {
                        throw new Exception(AppResource.BonsExceedTheAmountPendingToPay + ((int)cantidadBonosValida));
                    }
                }
                else if(forma.Valor > montoapagar)
                {
                    throw new Exception(AppResource.AmountExceedAmountPendingPayment + montoapagar.ToString("N2"));
                }

                int.TryParse(forma.NoCheque, out int numero);

                FormasPagoTemp value = new FormasPagoTemp();
                value.Banco = forma.Banco;

                int.TryParse(forma.BanID, out int banId);

                value.BanID = banId;
                value.Fecha = forma.Fecha.ToString("yyyy-MM-dd HH:mm:ss");
                value.Valor = forma.Valor;
                value.Prima = forma.Prima;
                value.AutSecuencia = forma.AutSecuencia;

                value.RefSecuencia = FormasPagoAgregadas.Count + 1;//myRec.GetLastRefSecuenciaInTemp() + 1;
                value.rowguid = Guid.NewGuid().ToString();
                value.Futurista = forma.Futurista ? "Si" : "No";
                value.NoCheque = numero;

                switch (CurrentFormaPago)
                {
                    case Enums.FormasPago.Cheque:
                        value.ForID = 2;
                        value.FormaPago = "Cheque";
                        break;
                    case Enums.FormasPago.Transferencia:
                        value.ForID = 4;
                        value.FormaPago = "Transferencia";
                        break;
                    case Enums.FormasPago.TarjetaCredito:
                        value.ForID = 6;
                        value.FormaPago = "Tarjeta de crédito";
                        break;
                    case Enums.FormasPago.Efectivo:
                        value.ForID = 1;
                        value.FormaPago = "Efectivo";
                        break;
                    case Enums.FormasPago.OrdenPago:
                        value.ForID = 18;
                        value.FormaPago = "Orden pago";
                        break;
                    case Enums.FormasPago.Retencion:
                        value.ForID = 5;
                        value.FormaPago = "Retención";
                        break;
                    case Enums.FormasPago.Bono:
                        value.ForID = 20;
                        value.FormaPago = "Bono";
                        value.BonoCantidad = forma.BonoCantidad;
                        value.BonoDenominacion = forma.BonoDenominacion;
                        break;

                }

                //myRec.AgregarFormaPago(value);
                FormasPagoAgregadas.Add(value);

                dialogAddFormaPago.Dismiss();

                //TotalAPagar = GetTotalAPagar();

            }
            catch (Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }

        }

        private void CambiarMoneda()
        {
            if(CurrentTipoPedido == null)
            {
                return;
            }
        
            if (ObtenerMonedaDesdeTipoPedido && UseMultiMoneda)
            {
                var splitTipoPedido = CurrentTipoPedido.Descripcion.Split('-');
                var moncodigoFromSplit = splitTipoPedido.Last();
                if (!string.IsNullOrWhiteSpace(moncodigoFromSplit))
                {
                    CurrentMoneda = MonedasSource.FirstOrDefault(c => c.MonCodigo == moncodigoFromSplit);
                    if (CurrentMoneda == null)
                    {
                        CurrentMoneda = MonedasSource.FirstOrDefault(c => c.MonCodigo == Arguments.Values.CurrentClient.MonCodigo);                        
                    }
                }
            }
            
        }

        private async void ActualizarPreciosNuevaMoneda()
        {
            try
            {
                if (CurrentMoneda == null)
                {
                    return;
                }

                var productos = myProd.GetResumenProductos((int)Arguments.Values.CurrentModule);

                if (Productos == null || Productos.Count() == 0)
                {
                    return;
                }

                foreach (var prod in productos)
                {
                    var item = myProd.GetProductoById(prod.ProID, CurrentMoneda.MonCodigo);

                    myProd.ActualizarPreciosInTemp(prod.rowguid, Math.Round(item.PrecioMoneda,2), item.LipPrecioMinimo);

                }

                SearchValue = LastValueSearch;

                await Search(false);

                SetResumenTotales();

            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.ErrorUpdatingPrices, e.Message);
            }

        }

        private async void ValidarFechaEntrega()
        {
            try
            {
                if (myParametro.GetParPedidosDiasEntregaSinFeriado() > 0 && Arguments.Values.CurrentModule == Modules.PEDIDOS)
                {
                    var FechaEntrega = FechaValid;
                    var FechaEntregaValid = DateTime.Now.AddDays(myParametro.GetParPedidosDiasEntregaSinFeriado());
                    var DiaFeriado = myPed.ExistsDiaFeriado(CurrentFechaEntrega.ToString("yyyy-MM-dd"));
                    var day = Convert.ToInt32(CurrentFechaEntrega.DayOfWeek);


                    if (DiaFeriado)
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.CannotSelectDateIsHoliday);
                        CurrentFechaEntrega = FechaEntrega;
                        return;

                    }

                    if (day == 7 || day == 0)
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.CannotSelectDateIsSunday);
                        CurrentFechaEntrega = FechaEntrega;
                        return;
                    }

                    if (CurrentFechaEntrega.DayOfYear < FechaEntrega.DayOfYear)
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.CannotSelectDateIsLessThanDefault);
                        CurrentFechaEntrega = FechaEntrega;
                        return;
                    }


                }

            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.ErrorUpdatingDeliveryDate, e.Message);
            }

        }

    }
}
