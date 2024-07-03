using FFImageLoading;
using Microsoft.AppCenter.Crashes;
using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.Controls;
using MovilBusiness.Controls.Behavior;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal.Structs.Args;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.ViewModel;
using MovilBusiness.views;
using MovilBusiness.Views.Components.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AgregarProductosModal : ScrollableTabPage, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;

        private bool canSelectOferta;
        public bool CanSelectOferta { get => canSelectOferta; set { canSelectOferta = value; RaiseOnPropertyChanged(); } }
        
        private bool canSelectDescuento;
        public bool CanSelectDescuento { get => canSelectDescuento; set { canSelectDescuento = value; RaiseOnPropertyChanged(); } }
        public bool ShowEliminar { get; set; }
        public bool IsDppActive { get; set; }

        public bool ShowCentrosDistribucion { get => DS_RepresentantesParametros.GetInstance().GetParPedidosCentroDistribucion(); }

        private bool isbusy;
        public new bool IsBusy { get => isbusy; set { isbusy = value; RaiseOnPropertyChanged(); } }

        public ICommand ChangeTabCommand { get; private set; }
        public ICommand SeleccionarProductoOfertaCommand { get; private set; }
        public ICommand SaveProducto { get; private set; }
        public ICommand VerProductosDescuentoCommand { get; private set; }
        public ICommand ChangeColorAndSize { get; private set; }
        public ObservableCollection<CentrosDistribucion> centrosdistribucion { get; set; }
        public ObservableCollection<CentrosDistribucion> CentrosDistribucion { get => centrosdistribucion; set { centrosdistribucion = value; RaiseOnPropertyChanged(); } }

        private ObservableCollection<Ofertas> ofertas;
        public ObservableCollection<Ofertas> Ofertas { get => ofertas; set { ofertas = value; RaiseOnPropertyChanged(); } }

        private ObservableCollection<OfertasDetalle> ofertasdetalles;
        public ObservableCollection<OfertasDetalle> OfertaDetalles { get => ofertasdetalles; set { ofertasdetalles = value; RaiseOnPropertyChanged(); } }

        private ObservableCollection<DescuentosRecargos> descuentos;
        public ObservableCollection<DescuentosRecargos> Descuentos { get => descuentos; set { descuentos = value; RaiseOnPropertyChanged(); } }

        private ObservableCollection<DescuentosRecargosDetalle> descuentodetalles;
        public ObservableCollection<DescuentosRecargosDetalle> DescuentoDetalles { get => descuentodetalles; set { descuentodetalles = value; RaiseOnPropertyChanged(); } }

        private List<ProductosLotes> lotes;
        public List<ProductosLotes> Lotes { get => lotes; set { lotes = value; RaiseOnPropertyChanged(); } }

        private int cantidadofertasshow;
        public int CantidadOfertasShow { get => cantidadofertasshow; set { cantidadofertasshow = value; RaiseOnPropertyChanged(); } }
        
        private int cantidaddescuentosshow;
        public int CantidadDescuentosShow { get => cantidaddescuentosshow; set { cantidaddescuentosshow = value; RaiseOnPropertyChanged(); } }

        private ProductosLotes currentlote;
        public ProductosLotes CurrentLote { get => currentlote; set { currentlote = value; RaiseOnPropertyChanged(); } }

        private CentrosDistribucion currentCentrosDistribucion;
        public CentrosDistribucion CurrentCentrosDistribucion { get => currentCentrosDistribucion; set { currentCentrosDistribucion = value; RaiseOnPropertyChanged(); } }

        private DescuentosRecargos currentdescuento;
        public DescuentosRecargos CurrentDescuento { get => currentdescuento; set { currentdescuento = value; CargarDetalleDescuento(); RaiseOnPropertyChanged(); } }

        private bool btnofertaenabled = true;
        public bool btnOfertaEnabled { get => btnofertaenabled; set { btnofertaenabled = value; RaiseOnPropertyChanged(); } }

        private Ofertas currentferta;
        public Ofertas CurrentOferta { get => currentferta; set { currentferta = value;
                OfertaDetalles = value != null? new ObservableCollection<OfertasDetalle>(myOfe.GetDetalleByOfeId(value, CurrentProduct.ProID))
                    : OfertaDetalles;
                RaiseOnPropertyChanged(); } }

        private ProductosTemp currentproduct;
        public ProductosTemp CurrentProduct { get => currentproduct; set { currentproduct = value; OnCurrentProductChanged(); RaiseOnPropertyChanged(); } }

        private string invCantidad_Und;
        public string InvCantidad { get => invCantidad_Und; set { invCantidad_Und = currentproduct.InvCantidad.ToString() + "/" + currentproduct.InvCantidadDetalle.ToString(); OnCurrentProductChanged(); RaiseOnPropertyChanged(); } }

        private string tipodescuento;
        public string TipoDescuento { get => tipodescuento; set { tipodescuento = value; RaiseOnPropertyChanged(); } }

        private bool isCambiotipoDescuento;
        public bool IsCambioTipoDescuento { get => isCambiotipoDescuento; set { isCambiotipoDescuento = value; RaiseOnPropertyChanged(); } }

        private bool usecolorandsize;
        public bool UseColorAndSize { get => usecolorandsize; set { usecolorandsize = value; RaiseOnPropertyChanged(); } }
        public bool ShowProDescripcion3 { get => DS_RepresentantesParametros.GetInstance().GetProDescripcion3ProductoPed() == 2; }
        public List<UsosMultiples> InvAreas { get; set; }
        private List<UsosMultiples> colores, tamanos;
        private List<MotivosDevolucion> MotivosDevolucion;

        double parnumdos = 0.0;

        private MotivosDevolucion currentmotivo;
        public MotivosDevolucion CurrentMotivo { get => currentmotivo; set { currentmotivo = value; RaiseOnPropertyChanged(); } }

        private DS_RepresentantesParametros myParametro;
        private DS_Ofertas myOfe;
        private DS_DescuentosRecargos myDes;
        private DS_Inventarios myInv;
        private DS_Productos myProd;
        private DS_UsosMultiples myUso;
        private DS_EntregasRepartidorTransacciones myEnt;
        private DS_ProductosLotes myLote;
        private DS_InventariosAlmacenesLotes myinvalmlot;
        private DS_Compras myCom;
        private DS_Cuadres myCua;
        private DS_CentrosDistribucion myCenDis;
        private DS_Clientes myCli;
        private DS_GrupoProductos myGrupoProductos;

        private bool parInvArea, parUsePrecioEditable, parOfertasYDescuentosVisualizar, parPedidosDescuentosManuales, ParCotizacionesDescuentoManual, isCompraFactura, parOfertasManuales, ParPedidosMultiplos, parRevenimiento, parDescuentoNegociado, ParPedidosDescuentoMaximo;

        public Action<AgregarProductosArgs> OnCantidadAccepted { get; set; }

        private bool enableCondicionPago = true;
        public bool Deshabilitar { get => enableCondicionPago; set { enableCondicionPago = value; RaiseOnPropertyChanged(); } }

        private bool UsarPromociones;

        /// <summary>
        /// DESCUENTO Y OFERTAS MANUALES    
        /// </summary>
        private string cantidadoferta;
        private string elegirprodlabel;
        public string CantidadOfertaManual { get => cantidadoferta; set { cantidadoferta = value; RaiseOnPropertyChanged(); } }
        public string ElegirProdLabel { get => elegirprodlabel; set { elegirprodlabel = value; RaiseOnPropertyChanged(); } }
        private ProductosTemp CurrentProductoOferta = null;

        public EntregasRepartidorTransacciones CurrentEntrega { get; set; } = null;

        private string ParLabelCantidadDescripcion = null;
        private int ParVentasLote = -1;
        private bool ParVentasCantidadPiezas;
        private bool IsEntrega = false;
        private bool ParVentasLotesAutomaticos;
        public List<string> valuesNamesViewsVisibles;
        public bool IsEntregandoTraspaso { get; set; }
        public bool ParUseMotivoCambio { get; set; }

        private bool IsMultiEntrega;

        private int NumeroTransaccion = 1;
        private int parPedidosTipoDescuentoManual = 0;

        private List<KV> Atributos1;
        private List<KV> Atributos2;

        private bool carasHablitada;
        public bool CarasHablitada { get => carasHablitada; set { carasHablitada = value; RaiseOnPropertyChanged(); } }
        
        public AgregarProductosModal(DS_Productos myProd, bool isEntrega = false, CentrosDistribucion centrodistribucion = null, bool IsMultiEntrega = false)
        {  
            IsEntrega = isEntrega;           
            myParametro = DS_RepresentantesParametros.GetInstance();
            myOfe = new DS_Ofertas();
            myDes = new DS_DescuentosRecargos();
            myInv = new DS_Inventarios();
            myUso = new DS_UsosMultiples();
            myEnt = new DS_EntregasRepartidorTransacciones();
            myLote = new DS_ProductosLotes();
            myinvalmlot = new DS_InventariosAlmacenesLotes();
            myCua = new DS_Cuadres();
            myCenDis = new DS_CentrosDistribucion();
            myCli = new DS_Clientes();
            myGrupoProductos = new DS_GrupoProductos();
            this.myProd = myProd;
            valuesNamesViewsVisibles = new List<string>();
            UseColorAndSize = myParametro.GetParPedidosProductosColoresYTamanos();            
            this.IsMultiEntrega = IsMultiEntrega;

            TipoDescuento = AppResource.PercentDiscountLabel;

            ParUseMotivoCambio = myParametro.GetParCambiosUsarMotivos() == 2;

            var parInvAreaRaw = myParametro.GetParInventarioFisicoArea();

            if(Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS)
            {
                parInvAreaRaw = myParametro.GetParColocacionProductosCapturarArea();
            }
            parOfertasManuales = myParametro.GetParPedidosOfertasyDescuentosManuales();

            parInvArea = !string.IsNullOrWhiteSpace(parInvAreaRaw) && parInvAreaRaw.ToUpper().Trim() == "D" && (Arguments.Values.CurrentModule == Modules.INVFISICO || Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS);
            parUsePrecioEditable = (Arguments.Values.CurrentModule == Modules.INVFISICO && myParametro.GetParInventariosFisicosPrecios())
                || myParametro.GetParCotizacionesEditarPrecio()
                || myParametro.GetParPedidosEditarPrecio()
                || myParametro.GetParVentasEditarPrecio()
                || Arguments.Values.CurrentModule == Modules.AUDITORIAPRECIOS;

            parRevenimiento = myParametro.GetParRevenimiento();
            parDescuentoNegociado = myParametro.GetDescuentoxPrecioNegociado();
            parOfertasYDescuentosVisualizar = myParametro.GetParPedidosVisualizarOfertasYDescuentosEnProductosDetalle() && !parOfertasManuales;
            ShowEliminar = myParametro.GetParInventarioFisicoAceptarProductosCantidadCero();
            parPedidosDescuentosManuales = Arguments.Values.CurrentModule == Modules.PEDIDOS && myParametro.GetParPedidosDescuentosManuales() > 0;
            parPedidosTipoDescuentoManual = myParametro.GetParPedidosDescuentosManuales();
            ParCotizacionesDescuentoManual = myParametro.GetParCotizacionesDescuentoManual();
            ParPedidosDescuentoMaximo = myParametro.GetParPedidosDescuentoMaximo();
            isCompraFactura = Functions.IsCompraFactura;
            ParVentasCantidadPiezas = myParametro.GetParVentasCantidadPiezas();

            UsarPromociones = myParametro.GetParPedidosPromociones();

            ParPedidosMultiplos = myParametro.GetParProductoMultiplos();

            LoadCurrentAlmId();
            SetNumeroTransaccion();

            if (parInvArea)
            {                
                InvAreas = new DS_UsosMultiples().GetInvAreas();
            }

            //ChangeTabCommand = new Command(OnTabPageChange);
            SeleccionarProductoOfertaCommand = new Command(GoSeleccionarProducto);
            SaveProducto = new Command(()=> { AttempAddProduct(null, null); });
            ChangeColorAndSize = new Command(SelectColor);
            VerProductosDescuentoCommand = new Command(GoProductosDescuento);


            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) => true;

            var client = new HttpClient(handler);
            ImageService.Instance.Initialize(new FFImageLoading.Config.Configuration
            {
                HttpClient = client
            });

            InitializeComponent();

            BindingContext = this;

            comboInvArea.IsVisible = parInvArea;
            lblInvArea.IsVisible = parInvArea;

            lblPrecio.IsVisible = parUsePrecioEditable;
            editPrecio.IsVisible = parUsePrecioEditable;

            lblRevenimiento.IsVisible = parRevenimiento;
            editRevenimiento.IsVisible = parRevenimiento;

            lblCantidadPiezas.IsVisible = ParVentasCantidadPiezas;
            editCantidadPiezas.IsVisible = ParVentasCantidadPiezas;

            if (Arguments.Values.CurrentModule == Modules.PEDIDOS && ShowCentrosDistribucion)
            {
                ObservableCollection<CentrosDistribucion> listCentros = new ObservableCollection<CentrosDistribucion>();
                var centroDist = new ObservableCollection<CentrosDistribucion>(myCenDis.GetCentrosDistribucions(Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.SecCodigo : ""));
                var validos = myParametro.GetParFiltroCentroDistribucion();

                foreach (var centro in centroDist)
                {
                    if (validos == null || validos.Length == 0 || validos.Contains(centro.CedCodigo.ToString()))
                    {
                        listCentros.Add(centro);
                    }
                }
                CentrosDistribucion = listCentros;

                if (validos != null && validos.Length > 0)
                {
                    CurrentCentrosDistribucion = CentrosDistribucion.FirstOrDefault(c => c.CedCodigo.ToString() == validos[0].ToString());
                }

                //CentrosDistribucion = new ObservableCollection<CentrosDistribucion>(myCenDis.GetCentrosDistribucions(Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.SecCodigo : ""));

                if (Arguments.Values.CurrentSector != null)
                {
                    var codigobysector = centrodistribucion != null ? centrodistribucion.CedCodigo : myCli.GetClienteCedCodigoFromDetalleBySector(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentSector.SecCodigo);
                    CurrentCentrosDistribucion = CentrosDistribucion.FirstOrDefault(c => c.CedCodigo.ToString() == codigobysector);
                }
            }

            if (ParUseMotivoCambio)
            {
                lblMotivo.IsVisible = true;
                comboMotivo.IsVisible = true;
                MotivosDevolucion = new DS_Devoluciones().GetMotivosDevolucion();

                comboMotivo.ItemsSource = MotivosDevolucion;
            }
                        
            if (myParametro.GetParInventarioFisicoCapturarFacing())
            {
                lblFacing.IsVisible = true;
                editFacing.IsVisible = true;
            }

            if (myParametro.GetParPedidosConsultarVencimientosProductos())
            {
                btnConsultarVencimiento.IsVisible = true;
            }

            if (IsMultiEntrega){
                lblFechaEntrega.IsVisible = true;
                pickerFechaEntrega.IsVisible = true;
                pickerFechaEntrega.MinimumDate = DateTime.Now;
            }

            if ((parUsePrecioEditable || isCompraFactura) && (Arguments.Values.CurrentModule != Modules.INVFISICO && Arguments.Values.CurrentModule != Modules.COLOCACIONMERCANCIAS))
            {
                lblPrecio.Text = AppResource.PriceLabel;
            }
            if (parDescuentoNegociado)
            {
                lblPrecio.Text = AppResource.NegotiatedPriceLabel;
            }

            if (UsarPromociones)
            {
                promocionContainer.IsVisible = true;
                lblPromocion.IsVisible = true;
                checkPromocion.IsVisible = true;
                checkPromocion.Toggled += CheckPromocion_Toggled;
            }

            if(Arguments.Values.CurrentModule == Modules.AUDITORIAPRECIOS)
            {
                //SwapPriceAndQuantityViewsPosition();
                
                if (myParametro.GetParAuditoriaPrecioCapturarPresencia())
                {
                    layoutPresencia.IsVisible = true;
                }
                else
                {
                    layoutPresencia.IsVisible = false;
                }

                if (myParametro.GetParAuditoriaPrecioCapturarPrecioOferta())
                {
                    lblPrecioOferta.IsVisible = true;
                    editPrecioOferta.IsVisible = true;
                }

                if (myParametro.GetParAuditoriaPrecioCapturarCaras())
                {
                    lblCaras.IsVisible = true;
                    editCaras.IsVisible = true;

                    if (myParametro.GetParAuditoriaPrecioCapturarCarasPorPresencia())
                    {
                        if (switchPresencia.IsToggled)
                        {
                            CarasHablitada = true;
                        }
                        else
                        {
                            CarasHablitada = false;
                        }
                    }
                    else
                    {
                        CarasHablitada = true;
                    }

                }

                if (!myParametro.GetParAuditoriaPreciosCapturarFacing())
                {
                    lblCantidad.IsVisible = false;
                    editCantidad.IsVisible = false;
                }
            }
            else
            {
                layoutPresencia.IsVisible = false;
            }

            if (myParametro.GetParCambiarPrecioPorCantidadEnOrdenDePosicion())
            {
                SwapPriceAndQuantityViewsPosition();
            }

            promocionContainer.IsVisible = true;

            if(myParametro.GetParMostrarDocenasEnAgregarProductos())
            {
                lblDocenas.IsVisible = true;
                checkDocenas.IsVisible = true;
            }

            var parComprasFacturas = myParametro.GetParComprasUsarFacturas();

            lblFacturaCompra.IsVisible = isCompraFactura || parComprasFacturas;
            editFacturaCompra.IsVisible = isCompraFactura || parComprasFacturas;

            if (parComprasFacturas)
            {
                myCom = new DS_Compras();
            }

            if (myParametro.GetParPedidosReferenciaColoresYTamanosSoloNumeros())
            {
                editReferencia.Behaviors.Add(new NumericValidation());
                editReferencia.Keyboard = Keyboard.Numeric;
            }           

            var ParLabelCantidadAuditoriaPrecios = myParametro.GetParLabelCantidadDescriptionAudPrecio();

            if (!string.IsNullOrWhiteSpace(ParLabelCantidadAuditoriaPrecios))
            {
                lblCantidad.Text = ParLabelCantidadAuditoriaPrecios.Trim();
            }
            else
            {
                ParLabelCantidadDescripcion = myParametro.GetParLabelCantidadDescripcion();
                var ParLabelCantidadDescripcionConteoFisico = myParametro.GetParLabelCantidadDescripcionConteoFisico();

                if(!string.IsNullOrWhiteSpace(ParLabelCantidadDescripcion) && !string.IsNullOrWhiteSpace(ParLabelCantidadDescripcionConteoFisico) 
                    && Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS)
                {
                    lblCantidad.Text = ParLabelCantidadDescripcionConteoFisico;
                }
                else if (!string.IsNullOrWhiteSpace(ParLabelCantidadDescripcion))
                {
                    lblCantidad.Text = ParLabelCantidadDescripcion;
                }
            }

            ParVentasLotesAutomaticos = myParametro.GetParVentasLotesAutomaticos() > 0;

            if (!ParVentasLotesAutomaticos)
            {
                ParVentasLote = Arguments.Values.CurrentModule == Modules.CAMBIOSMERCANCIA ? myParametro.GetParCambiosMercanciaLotes() : Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS ? myParametro.GetParConteosFisicosLotes() : Arguments.Values.CurrentModule == Modules.INVFISICO ? myParametro.GetParInventarioFisicosLotes() : myParametro.GetParVentasLote();
            }

            controlTipoCambio.IsVisible = Arguments.Values.CurrentModule == Modules.CAMBIOSMERCANCIA && (ParVentasLote == 1 || ParVentasLote == 2);

            if (ParVentasLote == 2)
            {
                comboLote.SelectedIndexChanged += (s, e)=> 
                {
                    if (comboLote.SelectedIndex != -1)
                    {
                        LoadCantidadInventarioMultiAlmacen();
                    }
                };
            }

            if (UseColorAndSize)
            {
                lblCantidad.IsVisible = false;
                editCantidad.IsVisible = false;
                lblUnidades.IsVisible = false;
                editUnidades.IsVisible = false;

                editReferencia.Completed += delegate { ChangeProductByReference(); };
            }

            //El incluir este parametro myParametro.GetParPedidosOfertasyDescuentosManuales() estaba dañando otra funcionalidad
            if (myParametro.GetParCotOfertasManuales() || myParametro.GetParVenOfertasManuales() || myParametro.GetParPedOfertasManuales())// || myParametro.GetParPedidosOfertasyDescuentosManuales())
            {
                lblofertamanual.IsVisible = true;
                editofertamanual.IsVisible = true;
            }

            //Habilitar inventario cantidad si el modulo es ventas
            if ((Arguments.Values.CurrentModule == Modules.VENTAS && (!ParVentasLotesAutomaticos || myParametro.GetParVerInventarioConLoteAutomatico())) 
                || Arguments.Values.CurrentModule == Modules.TRASPASOS
                || Arguments.Values.CurrentModule == Modules.PROMOCIONES
                || Arguments.Values.CurrentModule == Modules.ENTREGASMERCANCIA
                || Arguments.Values.CurrentModule == Modules.CAMBIOSMERCANCIA)
            {
                lblinvcantidad.IsVisible = true;
                lblcantidadinventario.IsVisible = true;
            }

            if (Arguments.Values.CurrentModule == Modules.COMPRAS)
            {
                lblPrecio.IsVisible = isCompraFactura;
                editPrecio.IsVisible = isCompraFactura;
                editUnidades.IsVisible = false;
                lblUnidades.IsVisible = false;

            }

            if (parOfertasYDescuentosVisualizar)
            {
                if (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida())
                {
                    if(Device.Idiom == TargetIdiom.Tablet)
                    {
                        detalleOfertasUndView1.BindingContext = this;
                        descuentosUndView1.BindingContext = this;
                    }
                    else
                    {
                        detalleOfertasUndView.BindingContext = this;
                        descuentosUndView.BindingContext = this;
                    }
                }
                else
                {
                    if(Device.Idiom == TargetIdiom.Tablet)
                    {
                        detalleOfertasView1.BindingContext = this;
                        descuentosView1.BindingContext = this;
                    }else
                    {
                        detalleOfertasView.BindingContext = this;
                        descuentosView.BindingContext = this;
                    }
                }

            }
            else if (myParametro.GetParPedidosVisualizarFotoEnDetalleProductos() > -1)
            {
                imagenProducto.IsVisible = true;
            }
            else
            {
                imagenProducto.IsVisible = false;
            }


            if ((parPedidosDescuentosManuales && parPedidosTipoDescuentoManual == 1) || ParCotizacionesDescuentoManual)
            {
                lblDescuento.IsVisible = true;
                editDescuento.IsVisible = true;
                lblTiposDescuentos.IsVisible = true;
                swtDescuento.IsVisible = true;
                if (myParametro.GetParPedidosOcultarDescuentoManualMonto())
                {
                    lblTiposDescuentos.IsVisible = false;
                    swtDescuento.IsVisible = false;
                }
                layoutPorcDescuento.IsVisible = true;
            }
            

            if (parOfertasManuales)
            {
                ofertaManualContainer.IsVisible = true;               

                var view = new PedidosOfertasManualesView
                {
                    BindingContext = this
                };

                ofertaManualContainer.Content = view;
            }

            if (myParametro.GetLastThreeClientesVendidos())
            {
                var productosVendidos = myCli.GetLastThreeProductsByClient(Arguments.Values.CurrentClient.CliID);
                fmProductosVendidos.IsVisible = productosVendidos.Count() > 0 ? true : false;
                if(productosVendidos.Count() > 0)
                {
                    CargarDatosEnEntry(productosVendidos);
                }
               
                //var fechaycantidad = new ClientesProductos();
                //var lasrTreeProd = new ClientesProductos();

                //for (int i = 0; i < productosVendidos.Count(); i++)
                //{

                //}

            }
            else
            {
                fmProductosVendidos.IsVisible = false;

            }
        }


        private void CargarDatosEnEntry(List<ClientesProductos> data)
        {
            // Inicializa un contador para los Entry
            int entryIndex = 1;

            foreach (var record in data)
            {
                // Separar los segmentos por ';'
                var segments = record.CliFechasYCantidades.Split(';');

                foreach (var segment in segments)
                {
                    // Verificar si ya hemos llenado todos los Entry
                    if (entryIndex > 3)
                    {
                        break; // No procesar más si ya llenamos todos los Entry
                    }

                    // Separar la fecha y el número por '|'
                    var parts = segment.Split('|');
                    if (parts.Length == 2)
                    {
                        var fechaString = parts[0];
                        var numero = parts[1];

                        // Convertir la cadena de fecha a DateTime y formatearla
                        DateTime fecha;
                        if (DateTime.TryParseExact(fechaString, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out fecha))
                        {
                            var fechaFormateada = fecha.ToString("dd/MM/yyyy"); // Formato personalizado, puedes ajustarlo según necesites

                            // Asignar a los Entry correspondientes por su nombre
                            if (entryIndex == 1)
                            {
                                date1.Text = fechaFormateada;
                                cantidad1.Text = numero;
                            }
                            else if (entryIndex == 2)
                            {
                                date2.Text = fechaFormateada;
                                cantidad2.Text = numero;
                            }
                            else if (entryIndex == 3)
                            {
                                date3.Text = fechaFormateada;
                                cantidad3.Text = numero;
                            }

                            // Incrementar el índice de Entry
                            entryIndex++;
                        }
                        else
                        {
                            // Manejo de error si la conversión de fecha falla
                            Console.WriteLine($"Error al convertir la fecha: {fechaString}");
                        }
                    }
                }
            }

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
                case Modules.COLOCACIONMERCANCIAS:
                    NumeroTransaccion = DS_RepresentantesSecuencias.GetLastSecuencia("ColocacionProductos");
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
            }
        }
        private void SwapPriceAndQuantityViewsPosition()
        {
            lblPrecio.SetValue(Grid.RowProperty, 4);
            editPrecio.SetValue(Grid.RowProperty, 4);

            lblCantidad.SetValue(Grid.RowProperty, 2);
            editCantidad.SetValue(Grid.RowProperty, 2);
        }

        private async void GoProductosDescuento(object id)
        {

            if(id == null)
            {
                if(CurrentDescuento == null || !CurrentDescuento.IsMancomunado)
                {
                    return;
                }
            }else
            {
                if(CurrentOferta == null || !CurrentOferta.IsMancomunada)
                {
                    return;
                }
            }

            var response = await DisplayActionSheet(AppResource.SelectDesiredOption, AppResource.Cancel, null, new string[] {  AppResource.ProductToGiveUpper, AppResource.ProductThatApply });
            bool aRegalar = false;

            string grpCodigo;

            if(response == AppResource.ProductThatApply)
            {
                grpCodigo = id != null ? CurrentOferta.GrpCodigo : CurrentDescuento.GrpCodigo;
                if (string.IsNullOrEmpty(grpCodigo) || grpCodigo == "0")
                {
                    await DisplayAlert(AppResource.Warning, AppResource.OfferDontHaveProductThatApply, AppResource.Aceptar);
                    return;
                }
            }
            else if(response == AppResource.ProductToGiveUpper)
            {
                grpCodigo = id != null ? CurrentOferta.grpCodigoOferta : CurrentDescuento.GrpCodigoDescuento;
                if (string.IsNullOrEmpty(grpCodigo) || grpCodigo == "0")
                {
                    await DisplayAlert(AppResource.Warning, AppResource.OfferDontHaveProductToGive, AppResource.Aceptar);
                    return;
                }
                aRegalar = true;
            }
            else
            {
                return;
            }           

            await Navigation.PushModalAsync(new DescuentoMancomunadoProductosModal(grpCodigo, aRegalar));
        }

        private void ChangeProductByReference()
        {
            try
            {
                var reference = editReferencia.Text;

                var args = new ProductosArgs
                {
                    valueToSearch = null,
                    lipCodigo = myParametro.GetParSectores() >= 2 && Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.LipCodigo : Arguments.Values.CurrentClient != null ? Arguments.Values.CurrentClient.LiPCodigo : "Default",
                    filter = null,
                    IdFactura = null,
                    MonCodigo = Arguments.Values.CurrentClient.MonCodigo,
                    FiltrarProductosPorSector = myParametro.GetParSectores() > 0 && myParametro.GetParFiltrarProductosPorSector(),
                    IsEntregandoTraspaso = false,
                    referenceSplit = reference
                };

                var prods = myProd.GetProductos(args);

                if (prods != null && prods.Count() > 0)
                {
                    var item = prods.FirstOrDefault();

                    if (item != null)
                    {
                        btnColor.Text = AppResource.Select;
                        currentColor = null;
                        CurrentProduct = item;
                    }                    
                }
                else
                {
                    editReferencia.Text = "";
                    throw new Exception(AppResource.ProductNotFound);
                }
            }catch(Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }
        }

        //private string currentSize;

        private string _currentcolor;
        private string currentColor { get => _currentcolor; set { _currentcolor = value; CargarTamanos(); RaiseOnPropertyChanged(); } }

        private void CargarTamanos()
        {

            tamanos = new List<UsosMultiples>();

            if (!string.IsNullOrEmpty(currentColor))
            {
                lblSizes.IsVisible = true;
                var reference = CurrentProduct.ProCodigo.Split('-');

                var sizesValidos = myProd.GetProductosSizesAndColorByReferenceSplit(reference[0], false, currentColor);

                var todosTamanos = myUso.GetProductosTamanos();

                foreach (var s in sizesValidos)
                {
                    var item = todosTamanos.Where(x => x.CodigoUso == s).FirstOrDefault();

                    if (item != null && !tamanos.Contains(item))
                    {
                        tamanos.Add(item);
                    }
                }
            }
            else
            {
                lblSizes.IsVisible = false;
            }

            sizeContainer.Children.Clear();

            foreach(var size in tamanos.OrderBy(x=>x.Orden).ToList()){
                var view = new InputItemView();
                view.HorizontalOptions = LayoutOptions.Center;
                view.SetTitle(size.Descripcion);
                view.GetEdit().Text = "";
                view.BindingContext = size.CodigoUso;
                view.SetReturnType(ReturnType.Next);

                sizeContainer.Children.Add(view);
            }

            try
            {
                for (int i = 0; i < sizeContainer.Children.Count(); i++)
                {
                    if (sizeContainer.Children[i] is InputItemView input && i != sizeContainer.Children.Count() - 1 && sizeContainer.Children.ElementAtOrDefault(i + 1) is InputItemView next && next != null)
                    {
                        input.GetEdit().Completed += delegate
                        {
                            next.GetEdit().Focus();
                            next.GetEdit().CursorPosition = next.GetEdit().Text.Length;
                            next.GetEdit().SelectionLength = 1;
                        };
                    }
                }
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            LoadSizeQuantityes();

            //currentSize = null;
            //btnSize.Text = "Seleccione";
        }

        public void CargarUnidades()
        {
            var todasunidades = myUso.GetProductosUnidades();
            string lipcodigo = myParametro.GetParSectores() >= 2 && Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.LipCodigo : Arguments.Values.CurrentClient != null ? Arguments.Values.CurrentClient.LiPCodigo : "Default";
            var listasprecios = new DS_ListaPrecios().GetLipUnmCodigo(lipcodigo, CurrentProduct.ProID);
            undContainer.Children.Clear();

            if(myParametro.GetParPedidosProductosUnidadesForOrder())
            {
                todasunidades = todasunidades.Where(c => listasprecios.Select(i => i.UnmCodigo).
                Contains(c.CodigoUso)).OrderByDescending(l => l.Descripcion.Substring(0, 6)).ToList();
            }
            else
            {
                todasunidades = todasunidades.Where(c => listasprecios.Select(i => i.UnmCodigo).
                Contains(c.CodigoUso)).OrderBy(l => l.Descripcion.Substring(0, 6)).ToList();
            }
            
            foreach (var und in todasunidades)
            {
                var view = new InputItemView();
                view.HorizontalOptions = LayoutOptions.Center;
                view.SetTitle(und.CodigoUso);
                view.GetEdit().Text = "";
                view.BindingContext = und.CodigoUso;
                view.SetReturnType(ReturnType.Next);
                undContainer.Children.Add(view);
            }

            try
            {
                for (int i = 0; i < undContainer.Children.Count(); i++)
                {
                    if (undContainer.Children[i] is InputItemView input && i != undContainer.Children.Count() - 1 && undContainer.Children.ElementAtOrDefault(i + 1) is InputItemView next && next != null)
                    {
                        input.GetEdit().Completed += delegate
                        {
                            next.GetEdit().Focus();
                            next.GetEdit().CursorPosition = next.GetEdit().Text.Length;
                            next.GetEdit().SelectionLength = 1;
                        };
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            LoadUndQuantityes(todasunidades);

        }
        private async void SelectColor()
        {
            var rawCode = CurrentProduct.ProCodigo.Split('-');

            var haySizesSinGuardar = false;

            if (currentColor != null && sizeContainer.Children.Count > 0 && rawCode.Length > 2)
            {
                foreach(var view in sizeContainer.Children.ToList())
                {
                    if(view is InputItemView input && double.TryParse(input.GetEdit().Text, out double qty) && !string.IsNullOrWhiteSpace(input.GetEdit().Text) && !myProd.IsAddedInTempByColor(rawCode[0], rawCode[2], checkDocenas.IsToggled ? qty * 12 : qty))
                    {
                        haySizesSinGuardar = true;
                        break;
                    }
                }
            }

            if (haySizesSinGuardar)
            {
                var continuar = await DisplayAlert(AppResource.Warning, AppResource.SizesWithoutSaveWantContinue, AppResource.Continue, AppResource.Cancel);

                if (!continuar)
                {
                    return;
                }
            }

            var result = await DisplayActionSheet(AppResource.ChooseColor, AppResource.Cancel, null, buttons: colores.Select((x => x.Descripcion)).ToArray());

            var item = colores.Where(x => x.Descripcion == result).FirstOrDefault();

            if (item == null)
            {
                return;
            }

            currentColor = item.CodigoUso;
            btnColor.Text = item.Descripcion;

        }

        private void LoadCurrentAlmId()
        {
            if (myParametro.GetParUsarMultiAlmacenes())
            {
                if (myEnt.HayPedidosPorEntregar(Arguments.Values.CurrentClient.CliID))
                {
                    if (CurrentEntrega == null && !IsEntrega)
                    {
                        if (myParametro.GetParAlmacenVentaRanchera() > 0)
                        {
                            CurrentAlmId = myParametro.GetParAlmacenVentaRanchera();
                        }
                        else
                        {
                            CurrentAlmId = myParametro.GetParAlmacenIdParaDevolucion();
                        }
                    }
                    else
                    {
                        CurrentAlmId = myParametro.GetParAlmacenIdParaDespacho();
                    }
                }
                else if (CurrentEntrega == null)
                {
                    if (myParametro.GetParAlmacenVentaRanchera() > 0)
                    {
                        CurrentAlmId = myParametro.GetParAlmacenVentaRanchera();
                    }
                    else
                    {
                        CurrentAlmId = myParametro.GetParAlmacenIdParaDevolucion();
                    }
                }
                else
                {
                    CurrentAlmId = myParametro.GetParAlmacenIdParaDespacho();
                }
            }
            else
            {
                CurrentAlmId = -1;
            }
        }

        private void GoSeleccionarProducto()
        {
            if (checkPromocion.IsToggled)
            {
                DisplayAlert(AppResource.Warning, AppResource.ProductMarkedHasPromotionCannotBeOffer, AppResource.Aceptar);
                return;
            }
            var page = new ProductosPage(Arguments.Values.CurrentClient, true)
            {
                OnProductSelected = ((p) =>
                {
                    CurrentProductoOferta = p;
                    ElegirProdLabel = CurrentProductoOferta.Descripcion;
                })
            };

            Navigation.PushModalAsync(page);
        }

        private void Dismiss(object sender = null, EventArgs args = null)
        {
            ClearComponents();

            try
            {
                var keyboard = DependencyService.Get<IkeyboardHelper>();
                if (keyboard != null)
                {
                    keyboard.HideKeyboard();
                }
            }catch(Exception e)
            {
                Crashes.TrackError(e);
            }

            Navigation.PopAsync(false);
        }

        private void Eliminar(object sender, EventArgs args) { IndEliminar = true; AttempAddProduct(sender, args); }

        private bool IndEliminar = false;
        private async void AttempAddProduct(object sender, EventArgs a)
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            
            try
            {
                // var parDocenas = true;

                if (UseColorAndSize && !string.IsNullOrWhiteSpace(CurrentProduct.ProCodigo) && CurrentProduct.ProCodigo.Contains("-"))
                {
                    editCantidad.Text = "";

                    if (string.IsNullOrWhiteSpace(currentColor))
                    {
                        throw new Exception(AppResource.MustChooseColor);
                    }

                    var sizeViews = sizeContainer.Children.Where(x => x is InputItemView view && !string.IsNullOrWhiteSpace(view.GetEdit().Text)).ToList();

                    if (sizeViews == null || sizeViews.Count == 0)
                    {
                        throw new Exception(AppResource.MustSpecifySize);
                    }

                    bool somethingAdded = false;

                    foreach (InputItemView view in sizeViews)
                    {
                        var proCodigo = CurrentProduct.ProCodigo.Split('-')[0] + "-" + view.BindingContext.ToString() + "-" + currentColor;

                        var args = new ProductosArgs
                        {
                            valueToSearch = null,
                            lipCodigo = myParametro.GetParSectores() >= 2 && Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.LipCodigo : Arguments.Values.CurrentClient != null ? Arguments.Values.CurrentClient.LiPCodigo : "Default",
                            filter = null,
                            IdFactura = null,
                            MonCodigo = Arguments.Values.CurrentClient.MonCodigo,
                            FiltrarProductosPorSector = myParametro.GetParSectores() > 0 && myParametro.GetParFiltrarProductosPorSector(),
                            IsEntregandoTraspaso = false,
                            ProCodigo = proCodigo
                        };

                        var item = myProd.GetProductos(args).FirstOrDefault();

                        if(!string.IsNullOrEmpty(view.GetEdit().Text) && view.GetEdit().Text.EndsWith(".")){
                            view.GetEdit().Text += "0";
                        }

                        if (item != null && double.TryParse(view.GetEdit().Text, out double quantity))
                        {
                            if (checkDocenas.IsToggled)
                            {
                                quantity *= 12;
                            }

                            somethingAdded = true;

                            // item.Cantidad = quantity;

                            editCantidad.Text = quantity.ToString();

                           await AceptarProducto(item);
                        }
                    }

                    if (somethingAdded)
                    {
                        await DisplayAlert(AppResource.Success, AppResource.ProductSavedCorrectly, AppResource.Aceptar);
                    }

                    IsBusy = false;
                    return;
                }

                if (myParametro.GetParPedidosProductosUnidades() && !string.IsNullOrWhiteSpace(CurrentProduct.ProCodigo))
                {
                    editCantidad.Text = "";

                    var undViews = undContainer.Children.Where(x => x is InputItemView view).ToList();

                    if (undViews == null || undViews.Count == 0)
                    {
                        throw new Exception(AppResource.MustSpecifyAUnit);
                    }

                    foreach (InputItemView view in undViews)
                    {
                        var proCodigo = view.BindingContext.ToString();

                        var args = new ProductosArgs
                        {
                            valueToSearch = null,
                            lipCodigo = myParametro.GetParSectores() >= 2 && Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.LipCodigo : Arguments.Values.CurrentClient != null ? Arguments.Values.CurrentClient.LiPCodigo : "Default",
                            filter = null,
                            IdFactura = null,
                            MonCodigo = Arguments.Values.CurrentClient.MonCodigo,
                            FiltrarProductosPorSector = myParametro.GetParSectores() > 0 && myParametro.GetParFiltrarProductosPorSector(),
                            ProID = CurrentProduct.ProID,
                            ProUndMedidas = view.GetLabel().Text,
                        };

                        var item = myProd.GetProductos(args).FirstOrDefault();

                        if (string.IsNullOrEmpty(view.GetEdit().Text))
                        {
                            view.GetEdit().Text = "0";
                        }

                        if (item != null && double.TryParse(view.GetEdit().Text, out double quantity))
                        {
                            if (checkDocenas.IsToggled)
                            {
                                quantity *= 12;
                            }

                            editCantidad.Text = quantity.ToString();

                            await AceptarProducto(item);
                        }
                    }

                    IsBusy = false;                    
                    Dismiss();
                    return;
                }

                if (parnumdos <= 0 && checkDocenas.IsToggled && int.TryParse(editCantidad.Text, out int qty))
                {
                    qty *= 12;
                    parnumdos = qty;
                    editCantidad.Text = parnumdos.ToString();
                }

                await AceptarProducto(CurrentProduct);
            }
            catch(Exception e)
            {
               await DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
        }

        public async Task AceptarProducto(ProductosTemp CurrentProduct,bool isDescuentoAuthorize= false)
        {
            var precioVenta = 0.0;
            var revenimiento = 0.0;
            string invAreaDescr = null;
            var invArea = -1;

            if (parRevenimiento)
            {
                double.TryParse(editRevenimiento.Text, out double Revenimiento);
                revenimiento = Revenimiento; 
            }

            double.TryParse(editCantidad.Text, out double cantidad);

            if (parInvArea || isCompraFactura || parUsePrecioEditable)
            {
                double.TryParse(editPrecio.Text, out double precio);

                if (Arguments.Values.CurrentModule != Modules.AUDITORIAPRECIOS)
                {
                    switch (myParametro.GetParEditarPrecio())
                    {
                        case 1:
                            if (precio > CurrentProduct.LipPrecioMinimo)
                            {
                                if (myParametro.GetParAutorizarParaPedidosPrecioMinimo())
                                {
                                    if (await DisplayAlert(AppResource.Warning, AppResource.CannotEnterPriceLowerThanExisting, AppResource.Authorize, AppResource.Cancel))
                                    {
                                        var modal = new AutorizacionesModal(false, NumeroTransaccion, 1, null, false, false)
                                        {
                                            OnAutorizacionUsed = (autId) =>
                                            {
                                                precioVenta = precio;
                                            }
                                        };
                                        IsBusy = false;
                                        await Navigation.PushModalAsync(modal);
                                        return;
                                    }
                                    else
                                    {
                                        IsBusy = false;
                                        return;
                                    }
                                }
                                else
                                {
                                    throw new Exception(AppResource.CannotEnterPriceLowerThanMinimun);

                                }
                            }
                            break;
                        case 2:

                            if (precio < CurrentProduct.Precio)
                            {

                                if (myParametro.GetParAutorizarParaPedidosPrecioMinimo())
                                {
                                    if (await DisplayAlert(AppResource.Warning, AppResource.CannotEnterPriceLowerThanExisting, AppResource.Authorize, AppResource.Cancel))
                                    {
                                        var modal = new AutorizacionesModal(false, NumeroTransaccion, 1, null, false, false)
                                        {
                                            OnAutorizacionUsed = (autId) =>
                                            {
                                                precioVenta = precio;
                                            }
                                        };
                                        IsBusy = false;
                                        await Navigation.PushModalAsync(modal);
                                        return;
                                    }
                                    else
                                    {
                                        IsBusy = false;
                                        return;
                                    }
                                }
                                else
                                {
                                    throw new Exception(AppResource.CannotEnterPriceLowerThanExisting);

                                }
                            }
                            break;
                        case 3:
                            if (precio > CurrentProduct.Precio)
                            {
                                if (myParametro.GetParAutorizarParaPedidosPrecioMinimo())
                                {
                                    if (await DisplayAlert(AppResource.Warning, AppResource.CannotEnterPriceHigherThanExisting, AppResource.Authorize, AppResource.Cancel))
                                    {
                                        var modal = new AutorizacionesModal(false, NumeroTransaccion, 1, null, false, false)
                                        {
                                            OnAutorizacionUsed = (autId) =>
                                            {
                                                precioVenta = precio;
                                            }
                                        };
                                        IsBusy = false;
                                        await Navigation.PushModalAsync(modal);
                                        return;
                                    }
                                    else
                                    {
                                        IsBusy = false;
                                        return;
                                    }
                                }
                                else
                                {
                                    throw new Exception(AppResource.CannotEnterPriceHigherThanExisting);

                                }

                            }
                            break;
                        case 5:
                            if (precio < CurrentProduct.LipPrecioMinimo)
                            {
                                if (myParametro.GetParAutorizarParaPedidosPrecioMinimo())
                                {
                                    if (await DisplayAlert(AppResource.Warning, AppResource.CannotEnterPriceLowerThanExisting, AppResource.Authorize, AppResource.Cancel))
                                    {
                                        var modal = new AutorizacionesModal(false, NumeroTransaccion, 1, null, false, false)
                                        {
                                            OnAutorizacionUsed = (autId) =>
                                            {
                                                precioVenta = precio;
                                            }
                                        };
                                        IsBusy = false;
                                        await Navigation.PushModalAsync(modal);
                                        return;
                                    }
                                    else
                                    {
                                        IsBusy = false;
                                        return;
                                    }
                                }
                                else
                                {
                                    throw new Exception(AppResource.CannotEnterPriceLowerThanLipPrecioMinimo + ": " + CurrentProduct.LipPrecioMinimo.ToString());

                                }
                            }
                            break;
                    }
                }
                else
                {
                    if(precio <= 0 && cantidad > 0)
                    {
                        throw new Exception(AppResource.PriceMustBeGreaterThanZero);
                    }
                }

                if (myParametro.GetParPedidosEditarPrecioNegconItebis())
                {
                    precioVenta = (precio / ((CurrentProduct.Itbis / 100) + 1)) - CurrentProduct.AdValorem - CurrentProduct.Selectivo;
                    //CurrentProduct.PrecioTemp = precioVenta;
                    //CurrentProduct.Precio = precioVenta;
                }
                else
                {
                    precioVenta = precio;
                }

            }

            if (checkPromocion.IsToggled)
            {
                precioVenta = 0;
            }

            if (parInvArea && (comboInvArea.SelectedIndex == -1 || comboInvArea.SelectedItem == null) && !IndEliminar)
            {
                throw new Exception(AppResource.MustSelectArea);
            }                     

            int.TryParse(editUnidades.Text, out int unidades);
            bool oferta = int.TryParse(editofertamanual.Text, out int cantidadOferta);

            if (myParametro.GetParCotOfertasManuales())
            {
                var maximoOferta = myParametro.GetParCotizacionesLimiteMaximoOfertaManual();

                if(maximoOferta > 0)
                {
                    var porcientoOferta = (cantidadOferta / cantidad) * 100.0;

                    if(porcientoOferta > maximoOferta)
                    {
                        throw new Exception(AppResource.BidCannotExceedMaximun.Replace("@", maximoOferta.ToString()));
                    }
                }
            }

            int cantFacing = 0;
            if (myParametro.GetParInventarioFisicoCapturarFacing())
            {
                int.TryParse(editFacing.Text, out cantFacing);

                if(cantFacing > 0 && cantidad <= 0 && unidades <= 0)
                {
                    throw new Exception(AppResource.MustSpecifyPhysicalQuantity);
                }

                if (parInvArea)
                {
                    if (comboInvArea.SelectedItem != null && comboInvArea.SelectedItem is UsosMultiples area && area != null)
                    {
                        if (area.CodigoUso.Equals("2") && (cantidad > 0 || unidades > 0) && cantFacing <= 0) //gondola
                        {
                            throw new Exception(AppResource.MustSpecifyFacingQuantity);
                        }
                    }
                }
            }

            if (checkPromocion.IsToggled)
            {
                cantidadOferta = 0;
            }

            if (checkPromocion.IsToggled && myParametro.GetParLimiteMaximoPromociones() > -1)
            {
                int limitePromociones = myParametro.GetParLimiteMaximoPromociones();
                if (cantidad > limitePromociones)
                {
                    throw new Exception(AppResource.CannotEnterMoreThanOfferLimit + limitePromociones.ToString());
                }
            }

            if (Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS && myParametro.GetParCantidadMaximaForReturn())
            {
                var cuadre = myCua.GetCuadreForCant(Arguments.Values.CurrentCuaSecuencia, CurrentProduct.ProID);
                var producto = myProd.ProCantidadHolgura(currentproduct.ProID);

                if(cuadre == null)
                {
                    throw new Exception(AppResource.InitialQuantityNotFound);
                }

                if(producto == null)
                {
                    throw new Exception(AppResource.SlackQuantityNotFound);
                }

                var obtporcent = (producto.ProHolgura / 100.0) * cuadre.CuaCantidadInicial;
                string prodat = CurrentProduct.ProDatos3;

                if (cantidad < obtporcent && String.Equals(CurrentProduct.ProDatos3,"R"))
                {
                    throw new Exception(AppResource.ReturnableProductCountCannotBeLessThan.Replace("@", producto.ProHolgura.ToString()));
                }
            }

            var almId = -1;

            if (myParametro.GetParUsarMultiAlmacenes())
            {
                if (myEnt.HayPedidosPorEntregar(Arguments.Values.CurrentClient.CliID))
                {
                    if (CurrentEntrega == null && !IsEntrega)
                    {
                        if (myParametro.GetParAlmacenVentaRanchera() > 0)
                        {
                            almId = myParametro.GetParAlmacenVentaRanchera();
                        }
                        else
                        {
                            almId = myParametro.GetParAlmacenIdParaDevolucion();
                        }
                    }
                    else
                    {
                        almId = myParametro.GetParAlmacenIdParaDespacho();
                    }

                }
                else if (CurrentEntrega == null)
                {

                    if (myParametro.GetParAlmacenVentaRanchera() > 0)
                    {
                        almId = myParametro.GetParAlmacenVentaRanchera();
                    }
                    else
                    {
                        almId = myParametro.GetParAlmacenIdParaDevolucion();
                    }
                }
                else
                {
                    almId = myParametro.GetParAlmacenIdParaDespacho();
                }
            }

            if(Ofertas != null && (Arguments.Values.CurrentModule == Modules.VENTAS || myParametro.GetParPedidosOfertasEnInventarios()) &&  cantidad > 0)
            {
                foreach (var str in Arguments.Values.ProidForExclurIdOfertas.Split('|'))
                {
                    if (!string.IsNullOrEmpty(str) && string.Equals(str.Substring(str.LastIndexOf("(") + 1, str.Substring(str.LastIndexOf("(") + 1).Length)
                        , CurrentProduct.ProID.ToString() + ")") || !string.IsNullOrEmpty(str.Substring(str.LastIndexOf(")") + 1, str.Substring(str.LastIndexOf(")") + 1).Length)))
                    {
                        Arguments.Values.ProidForExclurIdOfertas = Arguments.Values.ProidForExclurIdOfertas.Replace(str, " ");
                    }
                }

                var ofertados = new List<ProductosTemp>();
                foreach (var ofe in Ofertas)
                {
                    var productosSinExistenciaVentas = false;
                    ofertados.AddRange(myOfe.CalcularOfertaParaProducto(ofe, CurrentProduct, myParametro.GetPedidosAceptarOfertasDecimales(), myProd, (int)Arguments.Values.CurrentModule,
                        out productosSinExistenciaVentas, IsToverificar: true));
                }

                var mancomunadas = myOfe.GetOfertasMancomunadasDisponiblesForAgregarProductosModal(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentClient.TiNID,
                    (int)Arguments.Values.CurrentModule, proid: CurrentProduct.ProID);


                int CantidadAdarOferta = 0;
                int ofeCantidadaDar;
                int cantidadAEvaluarParaCalcular = (int)(unidades + (cantidad * (CurrentProduct.ProUnidades > 0? CurrentProduct.ProUnidades : 1)));
                int cantidadAEvaluar;
                foreach (var ofe in mancomunadas)
                {
                        CantidadAdarOferta = 0;
                        ofeCantidadaDar = 0;
                        var ofertaenproductos = myOfe.CrearProductoParaOfertaMancomunadaParaCalcular(ofe.GrpCodigo, CurrentProduct.ProID);    
                        int ofecalcular = (int)ofertaenproductos.Item2;
                        cantidadAEvaluar = cantidadAEvaluarParaCalcular + (ofecalcular > 0? ofecalcular : 0);
                        var detalles = myOfe.GetDetalleOfertaById(ofe.OfeID, (int)ofe.CantidadTemp);
                        var listProModificada = new List<ProductosTemp>();                        

                        foreach (var detalle in detalles)
                        {
                            if (!string.IsNullOrWhiteSpace(ofe.OfeTipo) && ofe.OfeTipo.Trim() == "14")
                            {
                                if (cantidadAEvaluar >= detalle.OfeCantidad)
                                {
                                    var cantidadRegalar = (int)(((int)(cantidadAEvaluar / detalle.OfeCantidad)) * detalle.OfeCantidadOferta);

                                    CantidadAdarOferta += cantidadRegalar;
                                    ofeCantidadaDar += cantidadRegalar;

                                    var prod = myOfe.CrearProductoParaOfertaMancomunada(detalle.ProID);

                                    if (prod != null)
                                    {
                                        prod.CantidadMaximaOferta = cantidadRegalar;
                                        listProModificada.Add(prod);
                                    }
                                }
                            }
                            else if (!string.IsNullOrWhiteSpace(ofe.OfeTipo) && ofe.OfeTipo.Trim() == "13")
                            {
                                if (detalle.OfeCantidadDetalleOferta > 0 && ofe.CantidadTemp >= detalle.OfeCantidadDetalle)
                                {
                                    CantidadAdarOferta += (int)(Math.Round(((int)(cantidadAEvaluar / detalle.OfeCantidadDetalle) * detalle.OfeCantidadDetalleOferta)));
                                    ofeCantidadaDar = (CantidadAdarOferta <= ofe.OfeCantidadMax ? CantidadAdarOferta : ofe.OfeCantidadMax);
                                    CantidadAdarOferta = ofeCantidadaDar;
                                }
                            }
                            else
                            {
                                if (cantidadAEvaluar >= detalle.OfeCantidad)
                                {
                                    if (detalle.OfePorciento > 0)
                                    {
                                        CantidadAdarOferta += (int)(Math.Round((detalle.OfePorciento / 100.0) * cantidadAEvaluar));

                                    }
                                    else
                                    {
                                        CantidadAdarOferta += (int)Math.Round((cantidadAEvaluar / (int)detalle.OfeCantidad) * detalle.OfeCantidadOferta);

                                    }
                                    ofeCantidadaDar = (CantidadAdarOferta <= ofe.OfeCantidadMax ? CantidadAdarOferta : ofe.OfeCantidadMax);
                                    cantidadAEvaluar -= (CantidadAdarOferta / (int)detalle.OfeCantidadOferta) * (int)detalle.OfeCantidad;

                                    if (ofe.OfeCantidadMax == 0)
                                    {
                                        ofeCantidadaDar = CantidadAdarOferta;
                                    }
                                    else
                                    {
                                        CantidadAdarOferta = ofeCantidadaDar;
                                    }
                                }
                            }
                        }

                    var oferman = myOfe.GetDetalleProductosOfertaMancomunada(ofe.grpCodigoOferta, ofe.OfeID, (int)Arguments.Values.CurrentModule);

                    if(oferman != null && oferman.Count == 1)
                    {
                        var result = ofertaenproductos.Item1.Where(l => l.Item1 == oferman[0].ProID).FirstOrDefault();
                        oferman.FirstOrDefault().Cantidad = result.Item1 != 0 ? result.Item2 + CantidadAdarOferta : oferman[0].ProID == CurrentProduct.ProID? CantidadAdarOferta + cantidadAEvaluarParaCalcular : CantidadAdarOferta;
                        ofertados.Add(oferman.FirstOrDefault());
                    }
                }
                string resultofofe = "";

                if (string.IsNullOrEmpty(Arguments.Values.ProidForExclurIdOfertas))
                {
                    Arguments.Values.ProidForExclurIdOfertas = "|";
                }

                foreach (var ofecount in ofertados)
                {
                    if (!myInv.HayExistencia(ofecount.ProID, ofecount.Cantidad, out Inventarios inventarios, almId: almId, lote: CurrentProduct.UsaLote &&
                        (ParVentasLote == 1 || ParVentasLote == 2) && !ParVentasLotesAutomaticos ? CurrentLote.PrlLote : null, currrentalmId: CurrentProduct.AlmID))
                    {
                        if(!resultofofe.Contains(ofecount.ProCodigo))
                        {
                            resultofofe += ofecount.ProCodigo + ", ";
                        }                        

                        if (!Arguments.Values.ProidForExclurIdOfertas.Split('|').Contains(CurrentProduct.ProID + "(" + ofecount.ProID.ToString() + ")"))
                        {
                            int numtoresult = 0;
                            for (int i = 0; i < CantidadAdarOferta; i++)
                            {
                                if(inventarios.invCantidad >= ofecount.Cantidad - i)
                                {
                                    numtoresult = i;
                                }
                            }

                            Arguments.Values.ProidForExclurIdOfertas +=CurrentProduct.ProID +"("+ofecount.ProID.ToString() +")"+ (numtoresult == 0 ? "" : numtoresult.ToString()) + "|";
                        }
                    }
                }

                if (!string.IsNullOrEmpty(resultofofe))
                {
                    bool result = await DisplayAlert(AppResource.Warning, AppResource.FollowingProductDontHaveInventory + resultofofe +" " + AppResource.WillNotBeAddedToSale, AppResource.Aceptar, AppResource.Cancel);

                    if (!result)
                    {
                        return;
                    }
                }
            }

            bool mercanciaCalcular = myParametro.GetParCambioMercanciaInsertarLotesParaRecivir();

            if (mercanciaCalcular)
            {

                string resultoflote = editLote.Text;

                switch (CurrentProduct.TipoCambio)
                {
                    case 1:
                        if (string.IsNullOrEmpty(resultoflote) && string.IsNullOrEmpty(CurrentProduct.LoteEntregado))
                        {
                            if(CurrentLote != null)
                            {
                                CurrentProduct.LoteEntregado = CurrentLote.PrlLote;
                                break;
                            }

                            break;
                        }
                        CurrentProduct.LoteEntregado = resultoflote;
                        break;
                    case 0:
                        if (string.IsNullOrEmpty(resultoflote) && string.IsNullOrEmpty(CurrentProduct.LoteRecibido))
                        {
                            if (CurrentLote != null)
                            {
                                CurrentProduct.LoteRecibido = CurrentLote.PrlLote;
                                break;
                            }
                            break;
                        }
                        CurrentProduct.LoteRecibido = resultoflote;
                        break;
                }                
            }

            string lote = null;

            if (!string.IsNullOrWhiteSpace(CurrentProduct.ProDatos3) && CurrentProduct.ProDatos3.ToUpper().Contains("L") 
                && (ParVentasLote == 1 || ParVentasLote == 2)
                && !ParVentasLotesAutomaticos)
            {
                if(ParVentasLote == 1)
                {
                    if (string.IsNullOrWhiteSpace(editLote.Text) || (mercanciaCalcular && string.IsNullOrWhiteSpace(CurrentProduct.LoteRecibido)) || (mercanciaCalcular && string.IsNullOrWhiteSpace(CurrentProduct.LoteEntregado)))
                    {
                        throw new Exception(AppResource.MustSpecifyLotWarning + (myParametro.GetParCambioMercanciaInsertarLotesParaRecivir() ? !string.IsNullOrWhiteSpace(CurrentProduct.LoteEntregado) ? " " + AppResource.Received : " " + AppResource.Delivered : ""));
                    }

                    lote = editLote.Text;
                }else if(ParVentasLote == 2)
                {
                    if(CurrentLote == null)
                    {
                        throw new Exception(AppResource.MustSpecifyLotWarning);
                    }

                    lote = CurrentLote.PrlLote;
                }
            }
            if (!string.IsNullOrWhiteSpace(CurrentProduct.ProDatos3) && CurrentProduct.ProDatos3.Contains("x"))
            {
                var productosCombo = new DS_ProductosCombos().GetProductosCombo(CurrentProduct.ProID);
                double combosExistentes = 0;
                foreach (var proCombo in productosCombo)
                {

                    var cantidadTotal = myInv.GetCantidadTotalInventario(proCombo.ProID, almId);
                    var cantidadCombosDisponibles = new DS_ProductosCombos().GetCombosDisponiblesxCantidad(proCombo.ProID, proCombo.ProIDCombo, cantidadTotal);
                    combosExistentes = (cantidadCombosDisponibles < combosExistentes || combosExistentes == 0) ? cantidadCombosDisponibles : combosExistentes;

                    if ((Arguments.Values.CurrentModule == Modules.VENTAS
                    || Arguments.Values.CurrentModule == Modules.ENTREGASMERCANCIA
                    || Arguments.Values.CurrentModule == Modules.PROMOCIONES
                    || (Arguments.Values.CurrentModule == Modules.CAMBIOSMERCANCIA && (!mercanciaCalcular ?
                    (((ParVentasLote == 1 || ParVentasLote == 2) && CurrentProduct.TipoCambio == 1) || ParVentasLote != 1 && ParVentasLote != 2) :
                    (((ParVentasLote == 1 || ParVentasLote == 2)) || ParVentasLote != 1 && ParVentasLote != 2)))
                    || (Arguments.Values.CurrentModule == Modules.PEDIDOS && myParametro.GetParCantInvAlmacenes())
                    || (Arguments.Values.CurrentModule == Modules.TRASPASOS && IsEntregandoTraspaso))
                    && !myInv.HayExistencia(proCombo.ProID, cantidad * proCombo.PrcCantidad, out Inventarios existencia, unidades, almId: almId, lote: CurrentProduct.UsaLote && (ParVentasLote == 1 || ParVentasLote == 2) && !ParVentasLotesAutomaticos ? mercanciaCalcular ? CurrentProduct.LoteEntregado : lote : null, currrentalmId: CurrentProduct.AlmID))
                    {
                        throw new Exception(AppResource.QuantityGreaterThanStock + (existencia != null ? ": " + cantidadCombosDisponibles.ToString() + (existencia.InvCantidadDetalle > 0 ? "/" + existencia.InvCantidadDetalle.ToString() : "") : ""));
                    }
                }

            }
            else if ((Arguments.Values.CurrentModule == Modules.VENTAS
                || Arguments.Values.CurrentModule == Modules.ENTREGASMERCANCIA
                || Arguments.Values.CurrentModule == Modules.PROMOCIONES 
                || (Arguments.Values.CurrentModule == Modules.CAMBIOSMERCANCIA && (!mercanciaCalcular ? 
                (((ParVentasLote == 1 || ParVentasLote == 2) && CurrentProduct.TipoCambio == 1) || ParVentasLote != 1 && ParVentasLote != 2) : 
                (((ParVentasLote == 1 || ParVentasLote == 2)) || ParVentasLote != 1 && ParVentasLote != 2)))
                || (Arguments.Values.CurrentModule == Modules.PEDIDOS && myParametro.GetParCantInvAlmacenes())  
                || (Arguments.Values.CurrentModule == Modules.TRASPASOS && IsEntregandoTraspaso)) 
                && !myInv.HayExistencia(CurrentProduct.ProID, cantidad, out Inventarios existencia, unidades, almId: almId, lote: CurrentProduct.UsaLote && (ParVentasLote == 1 || ParVentasLote == 2) && !ParVentasLotesAutomaticos ? mercanciaCalcular? CurrentProduct.LoteEntregado :  lote : null,currrentalmId: CurrentProduct.AlmID))
            {
                throw new Exception(AppResource.QuantityGreaterThanStock + (existencia != null ? ": " + existencia.invCantidad.ToString() + (existencia.InvCantidadDetalle > 0 ? "/" + existencia.InvCantidadDetalle.ToString() : "") : ""));
            }

            var compraFactura = "";

            var parCompraFactura = myParametro.GetParComprasUsarFacturas();
            if (isCompraFactura || parCompraFactura)
            {
                if (isCompraFactura && string.IsNullOrWhiteSpace(editFacturaCompra.Text))
                {
                    throw new Exception(AppResource.InvoiceCannotBeEmpty);
                }

                compraFactura = editFacturaCompra.Text;

                if (!string.IsNullOrWhiteSpace(compraFactura))
                {
                    compraFactura = compraFactura.Trim();
                }
                else
                {
                    compraFactura = "";
                }

                if (parCompraFactura && myCom != null && !string.IsNullOrWhiteSpace(compraFactura) && myCom.FacturaYaFueUsada(compraFactura))
                {
                    throw new Exception(AppResource.InvoiceNumberAlreadyUsed);
                }
            }

            if (parInvArea)
            {
                if (comboInvArea.SelectedItem is UsosMultiples item)
                {
                    invAreaDescr = item.Descripcion;

                    int.TryParse(item.CodigoUso, out int result);

                    invArea = result;

                }
            }

            double cantidadtotal = (cantidad * CurrentProduct.ProUnidades) + unidades;
            if (Arguments.Values.CurrentModule != Modules.AUDITORIAPRECIOS)
            {
                if (ParPedidosMultiplos && !string.IsNullOrWhiteSpace(CurrentProduct.ProCantidadMultiploVenta.ToString()) && CurrentProduct.ProCantidadMultiploVenta > 0 /*&& !CurrentProduct.UnmCodigo.ToUpper().Contains("UND")*/)
                {
                    var multiplo = cantidad % CurrentProduct.ProCantidadMultiploVenta;

                    if ((multiplo != 0 && cantidad > 0))
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.EnterMultipleQuantity.Replace("@", CurrentProduct.ProCantidadMultiploVenta.ToString()), AppResource.Aceptar);
                        return;
                    }
                }
            }

            if (Arguments.Values.CurrentModule == Modules.ENTREGASMERCANCIA && myParametro.GetParEntregasMercanciaLimiteEntregar())
            {
                var myEnt = new DS_Entregas();

                var currentWeek = Functions.GetWeekOfYear(DateTime.Now);
                var currentYear = DateTime.Now.Year;

                var cantidadLimiteProducto = myEnt.GetCantidadLimiteParaEntregar(CurrentProduct.ProID);
                var cantidadLimiteSemanal = myEnt.GetCantidadLimiteSemanalParaEntregar(CurrentProduct.ProID, currentYear, currentWeek);

                if (cantidadtotal + cantidadLimiteSemanal > cantidadLimiteProducto)
                {
                    throw new Exception(AppResource.CannotDeliverThisWeek.Replace("@", cantidadLimiteProducto.ToString()));
                }
            }

            /*No cambiar aunque la conversion a decimal sea redundante,ya que la division estaba devolviendo
             un numero entero y no estaba siendo validado correctamente */
            decimal productoUnidades = CurrentProduct.ProUnidades <= 0 ? 1 : (decimal)CurrentProduct.ProUnidades;
            decimal unidadesConvertidas = (decimal)unidades / productoUnidades;
            decimal cantidadTotalEvaluar = (decimal)cantidad + unidadesConvertidas;
            if (cantidadTotalEvaluar > (decimal)CurrentProduct.ProCantidadMaxVenta && CurrentProduct.ProCantidadMaxVenta != 0 && cantidadTotalEvaluar > 0)
            {
               await DisplayAlert(AppResource.Warning, AppResource.EnterQuantityLessThanMaximun.Replace("@", CurrentProduct.ProCantidadMaxVenta.ToString()), AppResource.Aceptar);
                return;
            }

            if (cantidadtotal < CurrentProduct.ProCantidadMinVenta && CurrentProduct.ProCantidadMinVenta != 0 && cantidadtotal > 0)
            {
               await DisplayAlert(AppResource.Warning, AppResource.EnterQuantityGreaterThanMinimun.Replace("@", CurrentProduct.ProCantidadMinVenta.ToString()), AppResource.Aceptar);
                return;
            }

            double descManual = 0;

            if (parPedidosDescuentosManuales || ParCotizacionesDescuentoManual)
            {
                double.TryParse(editDescuento.Text, out double result);
                descManual = result;

                if (IsCambioTipoDescuento)
                {
                    var porDescTest = ((descManual / (cantidad * CurrentProduct.Precio))) * 100;
                    var porDesc = Math.Round(((descManual / (cantidad * CurrentProduct.Precio))) * 100, 2, MidpointRounding.AwayFromZero);
                    descManual = porDesc;
                }

                double descMaximoByRepresentante = myParametro.GetDescuentoMaximoByRepresentante();
                if (descMaximoByRepresentante >= 0 && !isDescuentoAuthorize && descManual > descMaximoByRepresentante)
                {
                    if (await DisplayAlert(AppResource.Warning, "Debes introducir un descuento menor o igual al permitido por usuario: "+ myParametro.GetDescuentoMaximoByRepresentante().ToString() + " %", AppResource.Authorize, AppResource.Cancel))
                    {
                        var modal = new AutorizacionesModal(false, NumeroTransaccion, 1, null, false, false)
                        {
                            OnAutorizacionUsed = async (autId) =>
                            {
                                await AceptarProducto(CurrentProduct, true);
                            }
                        };
                        IsBusy = false;
                        await Navigation.PushModalAsync(modal);
                        return;
                    }
                    else
                    {
                        IsBusy = false;
                        return;
                    }
                }

                if (ParPedidosDescuentoMaximo)
                {
                    if (descManual > CurrentProduct.ProDescuentoMaximo && CurrentProduct.ProDescuentoMaximo != 0)
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.MustEnterDiscountLessThanConfigured.Replace("@", CurrentProduct.ProDescuentoMaximo.ToString()), AppResource.Aceptar);
                        return;
                    }
                }

                double montoDescuento;
                if (!IsCambioTipoDescuento)
                {
                    montoDescuento = CurrentProduct.Precio * (result / 100.0);
                }
                else
                {
                    montoDescuento = result / cantidad;
                }

                if(!myDes.IsDiscountManualValid(montoDescuento, CurrentProduct, (int)Arguments.Values.CurrentModule, Arguments.Values.CurrentClient.CliID, cantidad, out double descMaximoPermitido)){

                    if (!IsCambioTipoDescuento)
                    {
                        var porDescTest = ((descMaximoPermitido / (CurrentProduct.Precio))) * 100;
                        var porDesc = (((descMaximoPermitido / (CurrentProduct.Precio))) * 100);
                        descMaximoPermitido = porDesc;
                    }
                    else
                    {
                        descMaximoPermitido *= cantidad;
                    }
                    
                    await DisplayAlert(AppResource.Warning, AppResource.MustEnterDiscountLessThanMaximunForThisQuantity + string.Format("{0:n0}", descMaximoPermitido) + (!IsCambioTipoDescuento?"%":""), AppResource.Aceptar);
                    return;
                }

            }

            if (checkPromocion.IsToggled)
            {
                descManual = 0;
            }

            var cantOfertaManual = 0;
            var valorOfertaManual = 0.0;
            if (parOfertasManuales)
            {
                int.TryParse(CantidadOfertaManual, out int result);

                cantOfertaManual = result;

                if (checkPromocion.IsToggled)
                {
                    cantOfertaManual = 0;
                }

                if (CurrentProductoOferta == null && cantOfertaManual > 0)
                {
                    throw new Exception(AppResource.MustSpecifyOfferProduct);
                }
                bool valdesmax = myParametro.GetParDescuentosMaximosForPedidos();
                var lipCodigo = myParametro.GetParSectores() >= 2 
                    && Arguments.Values.CurrentSector != null ?
                    Arguments.Values.CurrentSector.LipCodigo :
                    Arguments.Values.CurrentClient != null ?
                    Arguments.Values.CurrentClient.LiPCodigo : "Default";

                if (valdesmax)
                    CurrentProduct.LipDescuento = new DS_ListaPrecios().GetLipDescuento(CurrentProduct.ProID, lipCodigo, CurrentProduct.UnmCodigo);

                if ((cantidad > 0 && cantOfertaManual > 0) || (cantidad > 0 && descManual > 0))
                {
                    var porcientoDpp = 0;
                    if (IsDppActive)
                    {
                        porcientoDpp = myParametro.GetParPedidosDpp();

                        if (porcientoDpp < 0)
                        {
                            porcientoDpp = 0;
                        }
                    }

                    var valorDesc = 0.0;
                    var VarDescTotal = 0.0;
                    var MontoBruto = 0.0;
                    var Montodescuento = 0.0;

                    /* if (IsCambioTipoDescuento) {
                         valorDesc = descManual;//(cantidad * CurrentProduct.Precio) * ((descManual + porcientoDpp) / 100.0);
                         valorOfertaManual = (CurrentProductoOferta != null ? CurrentProductoOferta.Precio * cantOfertaManual : 0.0);
                         VarDescTotal = (cantidad * CurrentProduct.Precio) - valorOfertaManual - valorDesc;
                         descManual = (valorDesc / (cantidad * CurrentProduct.Precio));
                     }
                     else {*/
                    //bool valdesmax = myParametro.GetParDescuentosMaximosForPedidos();
                    //var lipCodigo = myParametro.GetParSectores() >= 2 && Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.LipCodigo : Arguments.Values.CurrentClient != null ? Arguments.Values.CurrentClient.LiPCodigo : "Default";
                    double precio = 0.0;

                    if (CurrentProductoOferta != null)
                    {
                       precio = new DS_ListaPrecios().GetLipPrecioCompl(CurrentProductoOferta.ProID, lipCodigo, CurrentProduct.UnmCodigo);
                    }
                    valorDesc = (cantidad * CurrentProduct.Precio) * ((descManual + porcientoDpp) / 100.0);
                    valorOfertaManual = precio * cantOfertaManual;

                    if(valdesmax)
                    {
                        //CurrentProduct.LipDescuento = new DS_ListaPrecios().GetLipDescuento(CurrentProduct.ProID, lipCodigo, CurrentProduct.UnmCodigo);
                        MontoBruto = cantidad * CurrentProduct.Precio;
                        Montodescuento = ((valorOfertaManual / MontoBruto) * 100) + descManual + porcientoDpp;
                    }

                    VarDescTotal = (cantidad * CurrentProduct.Precio) - valorOfertaManual - valorDesc;
                    var precioMinimo = CurrentProduct.LipPrecioMinimo <= 0 ? CurrentProduct.Precio : CurrentProduct.LipPrecioMinimo;
                    //var precioMinimo = new DS_ListaPrecios().GetPrecioMinimo(CurrentProduct.ProID,lipCodigo,CurrentProduct.UnmCodigo);

                    if (myParametro.GetParPedidosRangoPrecioMinimoOfertasManuales())
                    {
                        var rangos = new DS_ListaPrecios().GetRangoPrecioMinimo(CurrentProduct.ProID, Arguments.Values.CurrentClient.LiPCodigo, CurrentProduct.UnmCodigo);

                        if (rangos.Count > 0)
                        {
                            var rango = rangos.Where(x => cantidad >= x.LipCantidadInicial && cantidad <= x.LipCantidadFinal).FirstOrDefault();

                            if (rango != null)
                            {
                                precioMinimo = rango.LipPrecioMinimo;
                            }
                            else
                            {
                                throw new Exception(AppResource.OfferAmountIsOutsideTheRange);
                            }
                        }
                    }

                    if (VarDescTotal < (precioMinimo * cantidad))
                    {
                        throw new Exception(AppResource.BidAmountExceedLimitForProduct);
                    }

                    if(valdesmax && Montodescuento > CurrentProduct.LipDescuento)
                    {
                        throw new Exception(AppResource.DiscountAmountExceedLimitForProduct);
                    }
                }
            }

            var cantidadPiezas = 0;
            if (ParVentasCantidadPiezas)
            {
                int.TryParse(editCantidadPiezas.Text, out int cntPiezas);

                cantidadPiezas = cntPiezas;

                if (cantidadPiezas == 0 && IsEntrega)
                {
                    throw new Exception(AppResource.PiecesMustBeGreaterThanZero);
                }
            }

            if (checkPromocion.IsToggled)
            {
                valorOfertaManual = 0;
                descManual = 0;
                precioVenta = 0;
            }

            var values = new AgregarProductosArgs() { CantidadPiezas = cantidadPiezas, IndicadorPromocion = checkPromocion.IsToggled, ValorOfertaManual = valorOfertaManual, ComprasNoFactura = compraFactura, DescuentoManual = descManual, Cantidad = cantidad, CantidadDetalleR = revenimiento, Precio = precioVenta, Unidades = unidades, InvArea = invArea, InvAreaDescr = invAreaDescr, IndicadorEliminar = IndEliminar, IndicadorDocena = checkDocenas.IsToggled };

            values.ProUnidades = CurrentProduct.ProUnidades;
            values.Presencia = switchPresencia.IsToggled;
            values.Facing = cantFacing;

            int.TryParse(editCaras.Text, out int caras);
            double.TryParse(editPrecioOferta.Text, out double precioOferta);

            values.Caras = caras;
            values.PrecioOferta = precioOferta;

            if (CurrentProduct.UseAttribute1 && comboAttribute1.SelectedItem != null)
            {
                var attr1 = comboAttribute1.SelectedItem as KV;
                values.Atributo1 = attr1;
            }

            if(CurrentProduct.UseAttribute2 && comboAttribute2.SelectedItem != null)
            {
                var attr2 = comboAttribute2.SelectedItem as KV;
                values.Atributo2 = attr2;
            }

            if (ParUseMotivoCambio)
            {
                if (comboMotivo.SelectedItem == null) 
                {
                    throw new Exception(AppResource.SpecifyReasonWarning);
                }

                values.MotId = (comboMotivo.SelectedItem as MotivosDevolucion).MotID;
            }

            if (IsMultiEntrega)
            {
                values.FechaEntrega = pickerFechaEntrega.Date.ToString("yyyy-MM-dd");
            }

            if (!string.IsNullOrWhiteSpace(CurrentProduct.ProDatos3) && CurrentProduct.ProDatos3.ToUpper().Contains("L"))
            {
                if(myParametro.GetParVentasLotesAutomaticos() == 2)
                {
                    values.Lote = CurrentProduct.Lote;
                }
                else
                {
                    values.Lote = lote;
                }
                

                var cantidadHolgura = myProd.CantidadHolgura(CurrentProduct.ProID);

                if (IsEntrega && cantidadtotal > 0 && ((CurrentProduct.UsaLote && !ParVentasLotesAutomaticos && !myProd.QuantityIsValidForDelivery(CurrentProduct.ProID, CurrentProduct.Posicion, cantidad, CurrentProduct.UsaLote ? values.Lote : "", cantidadHolgura)) || (!CurrentProduct.UsaLote)))
                {
                    if(myParametro.GetParNoEntregarVentasParaciales() && cantidad < CurrentProduct.CantidadEntrega)
                    {
                        throw new Exception(AppResource.QuantityIsLessThanTotalForDeliver);
                    }else if(cantidad > CurrentProduct.CantidadEntrega)
                    {
                        throw new Exception(AppResource.QuantityIsGreaterThanTotalForDeliver);
                    }                    
                }
            }

            if (myParametro.GetParCajasUnidadesProductos())
            {
                var proUnidades = CurrentProduct.ProUnidades;

                if (proUnidades == 0)
                {
                    proUnidades = 1;
                }

                values.Cantidad = (cantidad * proUnidades) + unidades;
                values.Unidades = 0;
            }
            if (myParametro.GetParRevenimiento())
            {
                values.CantidadDetalleR = revenimiento;
            }

            if (myParametro.GetParPedDescLip())
            {
                values.DescuentoXLipCodigo += myDes.
                        getLipDescuento(Arguments.Values.CurrentClient.LiPCodigo,
                        CurrentProduct.ProID);
            }

            if (parOfertasManuales && !checkPromocion.IsToggled)
            {
                if (CurrentProductoOferta != null && cantOfertaManual > 0)
                {
                    CurrentProductoOferta.IndicadorOferta = true;
                    var precio = CurrentProductoOferta.Precio;
                    CurrentProductoOferta.PrecioTemp = precio;
                    CurrentProductoOferta.Precio = 0;
                    CurrentProductoOferta.OfeID = CurrentProduct.ProID;
                    CurrentProductoOferta.Cantidad = cantOfertaManual;
                    CurrentProductoOferta.rowguid = Guid.NewGuid().ToString();
                }
                values.ProductoOferta = parOfertasManuales && cantOfertaManual > 0 ? CurrentProductoOferta : null;
                values.CantidadOfertaManual = cantOfertaManual;
            }
            //Ofertas manuales del mismo producto para el modulo de cotizaciones y ventas
            if ((myParametro.GetParCotOfertasManuales() || myParametro.GetParVenOfertasManuales() || myParametro.GetParPedOfertasManuales()) && !checkPromocion.IsToggled)
            {
                values.CantidadOfertaManual = cantidadOferta;
            }

            //preciominimo
            if (myParametro.GetParPedidosRangoPrecioMinimo() && !checkPromocion.IsToggled)
            {
                var PrecioMinimo = new DS_ListaPrecios().GetPrecioMinimo(CurrentProduct.ProID, Arguments.Values.CurrentClient.LiPCodigo, CurrentProduct.UnmCodigo);

                if (PrecioMinimo > 0)
                {
                    var rangoPrecio = Convert.ToDouble(editPrecio.Text) < PrecioMinimo;

                    if (rangoPrecio)
                    {
                        throw new Exception(AppResource.AmountProductIsOutsideLimits);

                    }
                }
            }

            if (myParametro.GetParPedidosRangoDescuentoMinimoEnPrecioModificado() > 0 && !checkPromocion.IsToggled &&  Arguments.Values.CurrentModule == Modules.PEDIDOS)
            {
                var DescuentoMinimo = myParametro.GetParPedidosRangoDescuentoMinimoEnPrecioModificado();
                var precioOriginal = new DS_ListaPrecios().GetLipPrecio(Arguments.Values.CurrentClient.LiPCodigo, CurrentProduct.ProID);

                    var rangoPrecio = Convert.ToDouble(editPrecio.Text) < precioOriginal - DescuentoMinimo;

                    if (rangoPrecio)
                    {
                        throw new Exception(AppResource.AmountProductIsOutsideLimits);

                    }
            }

            if (UseColorAndSize || myParametro.GetParPedidosProductosUnidades())
            {
                values.ProductToAdd = CurrentProduct;
            }

            string clidatosotros = !string.IsNullOrEmpty(Arguments.Values.CurrentClient.CliDatosOtros) ? Arguments.Values.CurrentClient.CliDatosOtros : "";
            if (clidatosotros.Contains("J"))
            {
                //var lote = myLote.GetFechaVencimientoProductoXLote(CurrentProduct.ProID);
                var inventariosalmacenes = myinvalmlot.GetInventarioAlmaceneseByProductos(CurrentProduct.ProID, CurrentProduct.AlmID);

                if (inventariosalmacenes != null && inventariosalmacenes.InvFechaVencimiento != null)
                {
              
                      DateTime timespan = inventariosalmacenes.InvFechaVencimiento;
                      TimeSpan time = timespan - DateTime.Now;
                 
                      int days = 365;
                      int totaldays =(int) Math.Abs(time.TotalDays);

                      if (totaldays <= days)
                      {
                          var result = await DisplayAlert(AppResource.Warning, AppResource.LotsDontExceedTheYear, AppResource.Continue, AppResource.Cancel);

                          if (!result)
                          {
                            IsBusy = false;
                            return;
                          }
                      }
                }
            }

            if (myParametro.GetParFiltroCentroDistribucion() != null && myParametro.GetParFiltroCentroDistribucion().Length > 0 && CurrentCentrosDistribucion == null && Arguments.Values.CurrentModule == Modules.PEDIDOS)
            {
                throw new Exception("Debe de seleccionar un Centro de Distribución");
            }

            values.CedCodigo = CurrentCentrosDistribucion != null && Arguments.Values.CurrentModule == Modules.PEDIDOS ? CurrentCentrosDistribucion.CedCodigo.ToString() : comboCentroDistribucion.SelectedItem != null && Arguments.Values.CurrentModule == Modules.PEDIDOS ? (comboCentroDistribucion.SelectedItem as CentrosDistribucion).CedCodigo : "";
            values.CedDescripcion = CurrentCentrosDistribucion != null && Arguments.Values.CurrentModule == Modules.PEDIDOS ? CurrentCentrosDistribucion.CedDescripcion.ToString() : comboCentroDistribucion.SelectedItem != null && Arguments.Values.CurrentModule == Modules.PEDIDOS ? (comboCentroDistribucion.SelectedItem as CentrosDistribucion).CedDescripcion : "";

            if(comboInvArea != null && parInvArea)
            {
                if (myParametro.GetParInventariosTomarCantidades() == 1 || myParametro.GetParInventariosTomarCantidades() == 3 || myParametro.GetParColocacionProductosTomarCantidades() == 1)
                {
                    switch ((comboInvArea.SelectedItem as UsosMultiples).CodigoUso)
                    {
                        case "1":
                            if (string.IsNullOrEmpty(editCantidad.Text) && CurrentProduct.CantidadAlm == null)
                            {
                                break;
                            }
                            CurrentProduct.CantidadAlm = cantidad;
                            break;
                        case "2":
                            if (string.IsNullOrEmpty(editCantidad.Text) && CurrentProduct.CanTidadGond == null)
                            {
                                break;
                            }
                            CurrentProduct.CanTidadGond = cantidad;
                            break;
                        case "3":
                            if (string.IsNullOrEmpty(editCantidad.Text) && CurrentProduct.CanTidadTramo == null)
                            {
                                break;
                            }
                            CurrentProduct.CanTidadTramo = cantidad;
                            break;
                    }

                    values.CanTidadGond = CurrentProduct.CanTidadGond;
                    values.CantidadAlm = CurrentProduct.CantidadAlm;
                    values.CanTidadTramo = CurrentProduct.CanTidadTramo;

                }else if(myParametro.GetParInventariosTomarCantidades() == 2 || myParametro.GetParColocacionProductosTomarCantidades() == 2)
                {
                    switch ((comboInvArea.SelectedItem as UsosMultiples).CodigoUso)
                    {
                        case "1":
                            CurrentProduct.CantidadAlm = cantidad;
                            if (!string.IsNullOrEmpty(editUnidades.Text))
                                CurrentProduct.UnidadAlm = unidades;
                            break;
                        case "2":
                            CurrentProduct.CanTidadGond = cantidad;
                            if (!string.IsNullOrEmpty(editUnidades.Text))
                                CurrentProduct.UnidadGond = unidades;
                            break;
                    }

                    values.CanTidadGond = CurrentProduct.CanTidadGond;
                    values.CantidadAlm = CurrentProduct.CantidadAlm;
                    values.UnidadGond = CurrentProduct.UnidadGond;
                    values.UnidadAlm = CurrentProduct.UnidadAlm;
                }         
            }

            if(mercanciaCalcular)
            {
                values.LoteRecibido = CurrentProduct.LoteRecibido;
                values.LoteEntregado = CurrentProduct.LoteEntregado;
            }

            OnCantidadAccepted?.Invoke(values);

            Deshabilitar = myProd.NothingAddedInTemp((int)Arguments.Values.CurrentModule);

            IndEliminar = false;

            if (!UseColorAndSize && !myParametro.GetParPedidosProductosUnidades())
            {
                Dismiss();
            }
        }

        private void EditCantidad_Focused(object sender, FocusEventArgs e)
        {
            if (myParametro.GetParPedidosVisualizarFotoEnDetalleProductos() > -1 && myParametro.GetParFiltroCentroDistribucion() != null 
                && myParametro.GetParOcultarImagenInFocus())
            {
                imagenProducto.IsVisible = false;
                mainContainer.VerticalOptions = LayoutOptions.Start;
            }
        }


        private void CheckPromocion_Toggled(object sender, ToggledEventArgs e)
        {
            editPrecio.IsEnabled = !e.Value;
            editDescuento.IsEnabled = !e.Value;
            editofertamanual.IsEnabled = !e.Value;
            btnOfertaEnabled = !e.Value;

            if (!checkPromocion.IsToggled)
            {
                if (CurrentProduct.PrecioSaved > 0)
                {
                    editPrecio.Text = CurrentProduct.PrecioSaved.ToString();
                }
                else
                {
                    editPrecio.Text = CurrentProduct.Precio.ToString();
                }
            }
        }

        private void ClearComponents()
        {
            editCantidad.Text = "";
            editFacturaCompra.Text = "";
            CurrentLote = null;
            editLote.Text = "";
            comboLote.SelectedIndex = -1;
            editUnidades.Text = "";
            editPrecio.Text = "";
            editDescuento.Text = "";
            editofertamanual.Text = "";
            editCantidadPiezas.Text = "";
            CurrentOferta = null;
            Ofertas = null;
            Descuentos = null;
            CurrentDescuento = null;
            DescuentoDetalles = null;
            OfertaDetalles = null;
            checkPromocion.IsToggled = false;
            CurrentProductoOferta = null;
            ElegirProdLabel = AppResource.ChooseProduct;
            Lotes = null;
            currentColor = null;
            //currentSize = null;

            if (parInvArea)
            {
                comboInvArea.SelectedIndex = -1;
            }
        }

        private int CurrentAlmId = -1;
        private int CurrentSelected = -1;

        private void comboInvArea_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(((Picker)sender).SelectedItem != null && parInvArea)
            {
                string edit = editCantidad.Text;
                string editund = editUnidades.Text;
                double.TryParse(editCantidad.Text, out double cantidad);
                double.TryParse(editUnidades.Text, out double cantidadund);

                if(myParametro.GetParInventariosTomarCantidades() == 1 || myParametro.GetParInventariosTomarCantidades() == 3 || myParametro.GetParColocacionProductosTomarCantidades() == 1)
                {
                    switch ((((Picker)sender).SelectedItem as UsosMultiples).CodigoUso)
                    {
                        case "2":
                            editCantidad.Text = CurrentProduct.CanTidadGond.ToString();
                            switch (CurrentSelected)
                            {
                                case 1:
                                    CurrentProduct.CantidadAlm = string.IsNullOrEmpty
                                      (edit) ? CurrentProduct.CantidadAlm
                                      : cantidad;
                                    break;
                                case 3:
                                    CurrentProduct.CanTidadTramo = string.IsNullOrEmpty
                                      (edit) ? CurrentProduct.CanTidadTramo
                                      : cantidad;
                                    break;
                            }
                            CurrentSelected = 2;
                            break;

                        case "1":
                            editCantidad.Text = CurrentProduct.CantidadAlm.ToString();
                            switch (CurrentSelected)
                            {
                                case 2:
                                    CurrentProduct.CanTidadGond = string.IsNullOrEmpty
                                      (edit) ? CurrentProduct.CanTidadGond
                                      : cantidad;
                                    break;
                                case 3:
                                    CurrentProduct.CanTidadTramo = string.IsNullOrEmpty
                                      (edit) ? CurrentProduct.CanTidadTramo
                                      : cantidad;
                                    break;
                            }
                            CurrentSelected = 1;
                            break;

                        case "3":

                            editCantidad.Text = CurrentProduct.CanTidadTramo.ToString();
                            switch (CurrentSelected)
                            {
                                case 1:
                                    CurrentProduct.CantidadAlm = string.IsNullOrEmpty
                                      (edit) ? CurrentProduct.CantidadAlm
                                      : cantidad;
                                    break;
                                case 2:
                                    CurrentProduct.CanTidadGond = string.IsNullOrEmpty
                                      (edit) ? CurrentProduct.CanTidadGond
                                      : cantidad;
                                    break;
                            }
                            CurrentSelected = 3;
                            break;
                    }
                }

                if(myParametro.GetParInventariosTomarCantidades() == 2 || myParametro.GetParColocacionProductosTomarCantidades() == 2)
                {
                    switch ((((Picker)sender).SelectedItem as UsosMultiples).CodigoUso)
                    {
                        case "2":
                            editCantidad.Text = CurrentProduct.CanTidadGond.ToString();
                            editUnidades.Text = CurrentProduct.UnidadGond.ToString();
                            CurrentProduct.CantidadAlm = string.IsNullOrEmpty
                                (edit) ? CurrentProduct.CantidadAlm
                                : cantidad;
                            CurrentProduct.UnidadAlm = string.IsNullOrEmpty
                                (editund) ? CurrentProduct.UnidadAlm
                                : cantidadund;
                            break;

                        case "1":
                            editCantidad.Text = CurrentProduct.CantidadAlm.ToString();
                            editUnidades.Text = CurrentProduct.UnidadAlm.ToString();
                            CurrentProduct.CanTidadGond = string.IsNullOrEmpty
                                (edit) ? CurrentProduct.CanTidadGond
                                : cantidad;
                            CurrentProduct.UnidadGond = string.IsNullOrEmpty
                                (editund) ? CurrentProduct.UnidadGond
                                : cantidadund;
                            break;
                    }
                }                
            }
        }

               
        private void controlTipoCambio_OnSegmentSelected(object sender, int pos)
        {

            string edit = editLote.Text;

            switch (pos)
            {
                case 0:
                    if(Lotes != null)
                    {
                        var currentlot = CurrentLote;
                        Lotes = Lotes.Where(l => l.PrlLote != CurrentProduct?.LoteEntregado).ToList();
                        CurrentLote = Lotes.FirstOrDefault(l => l.PrlLote == CurrentProduct.LoteEntregado);
                        CurrentProduct.LoteEntregado = string.IsNullOrEmpty(currentlot?.PrlLote) ?
                        CurrentProduct?.LoteEntregado : currentlot?.PrlLote;
                    }
                    else
                    {
                        editLote.Text = CurrentProduct?.LoteRecibido;
                        if (CurrentProduct != null)
                        {
                            CurrentProduct.LoteEntregado = string.IsNullOrEmpty
                                (edit) ? CurrentProduct?.LoteEntregado
                                : edit;
                        }
                    }
                    break;
                case 1:
                    if (Lotes != null)
                    {
                        var currentlot = CurrentLote;
                        Lotes = Lotes.Where(l => l.PrlLote != CurrentProduct?.LoteRecibido).ToList();
                        CurrentLote = Lotes.FirstOrDefault(l => l.PrlLote == CurrentProduct.LoteRecibido);
                        CurrentProduct.LoteRecibido = string.IsNullOrEmpty(currentlot?.PrlLote) ?
                        CurrentProduct?.LoteRecibido : currentlot?.PrlLote;
                    }else
                    {
                        editLote.Text = CurrentProduct?.LoteEntregado;
                        if (CurrentProduct != null)
                        {
                            CurrentProduct.LoteRecibido = string.IsNullOrEmpty
                                (edit) ? CurrentProduct?.LoteRecibido
                                : edit;
                        }
                    }
                    break;
            }

            if(CurrentProduct != null)
            {
                CurrentProduct.TipoCambio = pos;
            }            
        }


        private async void btnConsultarVencimiento_Clicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushModalAsync(new ConsultaVencimientoProductosModal(CurrentProduct.ProID, CurrentProduct.Descripcion));
            }catch(Exception ex)
            {
                await DisplayAlert(AppResource.Warning, ex.Message, AppResource.Aceptar);
            }
        }

        private void comboCentroDistribucion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((Picker)sender).SelectedItem != null)
            {
                CurrentCentrosDistribucion = CentrosDistribucion.FirstOrDefault(c => c.CedCodigo.ToString() == (((Picker)sender).SelectedItem as CentrosDistribucion).CedCodigo.ToString());
            }
        }

        private void editCantidad_Unfocused(object sender, FocusEventArgs e)
        {
            if (myParametro.GetParPedidosVisualizarFotoEnDetalleProductos() > -1 && myParametro.GetParFiltroCentroDistribucion() != null
                && myParametro.GetParOcultarImagenInFocus())
            {
                imagenProducto.IsVisible = true;
                mainContainer.VerticalOptions = LayoutOptions.Center;
            }
        }

        private void switchPresencia_Toggled(object sender, ToggledEventArgs e)
        {
            if (myParametro.GetParAuditoriaPrecioCapturarCarasPorPresencia())
            {
                CarasHablitada = switchPresencia.IsToggled;
                editCaras.Text ="";
            }
             
        }

        private void LoadCantidadInventarioMultiAlmacen()
        {
            if (myParametro.GetParUsarMultiAlmacenes())
            {
                var lote = "";

                if (CurrentProduct.UsaLote)
                {
                    if (ParVentasLote == 1)
                    {
                        lote = editLote.Text;
                    }
                    else if (ParVentasLote == 2)
                    {
                        lote = CurrentLote != null ? CurrentLote.PrlLote : "";
                    }
                }

                var invTotal = CurrentProduct.UsaLote && string.IsNullOrWhiteSpace(lote) ? 0.0 : myInv.GetCantidadTotalInventario(CurrentProduct.ProID, CurrentAlmId, lote);
                if (myParametro.GetParVerInventarioConLoteAutomatico())
                {
                    invTotal= myInv.GetCantidadTotalInventario(CurrentProduct.ProID, CurrentAlmId, string.IsNullOrWhiteSpace(lote) ? "" : lote);
                }

                lblcantidadinventario.Text = invTotal.ToString();
            }
        }

        private void LoadSizeQuantityes()
        {
            if(currentColor == null)
            {
                return;
            }

            var reference = CurrentProduct.ProCodigo.Split('-');

            foreach (var s in tamanos)
            {
                var proCodigo = reference[0] + "-" + s.CodigoUso + "-" + currentColor;

                var inTemp = myProd.GetProductInTempByProCodigo(proCodigo);

                if (inTemp != null)
                {
                    if (sizeContainer.Children.Where(x => x is InputItemView v && v.BindingContext.ToString() == s.CodigoUso).FirstOrDefault() is InputItemView view)
                    {
                        var quantity = inTemp.IndicadorDocena ? (inTemp.Cantidad / 12) : inTemp.Cantidad;

                        view.GetEdit().Text = quantity.ToString();
                    }
                }
            }
        }
        private void LoadUndQuantityes(List<UsosMultiples> unidadesTogive)
        {
           foreach(var prod in myProd.GetProductInTempByProId(CurrentProduct.ProID))
           {
              if (undContainer.Children.Where(x => x is InputItemView v && v.BindingContext.ToString() == prod.UnmCodigo).FirstOrDefault() is InputItemView view)
              {
                  var quantity = prod.IndicadorDocena ? (prod.Cantidad / 12) : prod.Cantidad;
                  view.GetEdit().Text = quantity.ToString();
              }                
           }          
        }

        private void LoadAttributes()
        {
            if (CurrentProduct.UseAttribute1)
            {
                Atributos1 = myProd.GetAttributosProducto(CurrentProduct.ProID);
                comboAttribute1.ItemsSource = Atributos1;

                if (!string.IsNullOrWhiteSpace(CurrentProduct.ProAtributo1))
                {
                    var item = Atributos1.FirstOrDefault(x => x.Key == CurrentProduct.ProAtributo1);
                    comboAttribute1.SelectedItem = item;
                }
            }

            if (CurrentProduct.UseAttribute2)
            {
                Atributos2 = myProd.GetAttributosProducto(CurrentProduct.ProID, true);
                comboAttribute2.ItemsSource = Atributos2;

                if (!string.IsNullOrWhiteSpace(CurrentProduct.ProAtributo2))
                {
                    var item = Atributos2.FirstOrDefault(x => x.Key == CurrentProduct.ProAtributo2);
                    comboAttribute2.SelectedItem = item;
                }
            }
        }

        private void OnCurrentProductChanged()
        {
            if (CurrentProduct == null)
            {
                ClearComponents();
                return;
            }
            if(myParametro.GetParPedidosProductosUnidades())
            {
                lblCantidad.IsVisible = false;
                editCantidad.IsVisible = false;
                lblUnidades.IsVisible = false;
                editUnidades.IsVisible = false;
                lblund.IsVisible = true;
                scrollContaiter.IsVisible = true;
                CargarUnidades();
            }


            if (CurrentProduct != null)
            {
                var myDev = new DS_Devoluciones();
                if (myDev.GetProductoProDatos3(CurrentProduct)) //Revisión de columna ProDatos3.-
                {
                    editCantidad.Behaviors.Clear();
                }
                else
                {
                    editCantidad.Behaviors.Add(new NumericValidation());
                }
            }

            if (myParametro.GetParPedidosCantidadesDocenasVenirActivo() && myParametro.GetParMostrarDocenasEnAgregarProductos())
            {
                checkDocenas.IsToggled = true;
            }

            LoadAttributes();

            if (Arguments.Values.CurrentModule != Modules.AUDITORIAPRECIOS)
            {
                if (string.IsNullOrWhiteSpace(ParLabelCantidadDescripcion))
                {
                    var uso = myUso.GetUsoByCodigoGrupo("UNMCODIGO", codigoUso: CurrentProduct.UnmCodigo);

                    if (uso != null && uso.Count > 0 && !string.IsNullOrEmpty(uso[0].Descripcion))
                    {
                        lblCantidad.Text = !uso[0].Descripcion.Trim().EndsWith(":") ? uso[0].Descripcion.Trim() + ": " : uso[0].Descripcion.Trim();
                    }
                }
            }

            if(editofertamanual.IsVisible && CurrentProduct.CantidadOferta > 0)
            {
                editofertamanual.Text = CurrentProduct.CantidadOferta.ToString();
            }

            if (myParametro.GetParInventarioFisicoCapturarFacing() && CurrentProduct.CantidadFacing > 0)
            {
                editFacing.Text = CurrentProduct.CantidadFacing.ToString();
            }
            else
            {
                editFacing.Text = "";
            }

            if (editPrecioOferta.IsVisible && CurrentProduct.PrecioOferta > 0)
            {
                editPrecioOferta.Text = CurrentProduct.PrecioOferta.ToString();
            }

            if (editCaras.IsVisible && CurrentProduct.Caras > 0)
            {
                editCaras.Text = CurrentProduct.Caras.ToString();
            }

            if (switchPresencia.IsVisible)
            {
                switchPresencia.IsToggled = CurrentProduct.IndicadorPresencia;
            }

            //lotes de ventas
            if ((ParVentasLote == 1 || ParVentasLote == 2) 
                && (Arguments.Values.CurrentModule == Modules.VENTAS || Arguments.Values.CurrentModule == Modules.INVFISICO ||
                Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS || Arguments.Values.CurrentModule == Modules.CAMBIOSMERCANCIA) 
                && !string.IsNullOrWhiteSpace(CurrentProduct.ProDatos3)
                && CurrentProduct.ProDatos3.ToUpper().Contains("L") && !ParVentasLotesAutomaticos)
            {
                if (ParVentasLote == 1)
                {
                    editLote.IsVisible = true;
                }
                else
                {
                    comboLote.IsVisible = true;
                }
                
                lblLote.IsVisible = true;

                if (ParVentasLote == 1 && !myParametro.GetParCambioMercanciaInsertarLotesParaRecivir())
                {
                    editLote.Text = CurrentProduct.Lote;
                }
                else if (ParVentasLote == 2)
                {
                    Lotes = myLote.GetLotesByProId(CurrentProduct.ProID, CurrentAlmId);

                    if (Lotes != null && !string.IsNullOrWhiteSpace(CurrentProduct.Lote))
                    {
                        var item = Lotes.Where(x => x.PrlLote.Trim().ToUpper() == CurrentProduct.Lote.Trim().ToUpper()).FirstOrDefault();

                        /* var index = Lotes.IndexOf(item);
                         comboLote.SelectedIndex = index;*/

                        CurrentLote = item;
                    }
                    else
                    {
                        comboLote.SelectedItem = null;
                    }

                    comboLote.ItemsSource = Lotes;
                }
            }
            else
            {
                comboLote.IsVisible = false;
                editLote.IsVisible = false;
                lblLote.IsVisible = false;
                Lotes = null;
                CurrentLote = null;
            }      
            
            if(Arguments.Values.CurrentModule == Modules.CAMBIOSMERCANCIA && (ParVentasLote == 1 || ParVentasLote == 2))
            {
                controlTipoCambio.Select(CurrentProduct.TipoCambio);
            }

            if ((Arguments.Values.CurrentModule == Modules.VENTAS && (!ParVentasLotesAutomaticos || myParametro.GetParVerInventarioConLoteAutomatico()) ) || Arguments.Values.CurrentModule == Modules.TRASPASOS
                || Arguments.Values.CurrentModule == Modules.PROMOCIONES || Arguments.Values.CurrentModule == Modules.ENTREGASMERCANCIA
                || Arguments.Values.CurrentModule == Modules.CAMBIOSMERCANCIA)
            {   //cuando es venta la variable invCantidad y invCantidadDetalle vienen de la tabla de inventarios en vez de InventariosAlmacen
                lblcantidadinventario.Text = CurrentProduct.InvCantidad.ToString();

                if (CurrentProduct.InvCantidadDetalle > 0)
                {
                    lblcantidadinventario.Text = CurrentProduct.InvCantidad.ToString() + "/" + CurrentProduct.InvCantidadDetalle.ToString();
                }

                LoadCantidadInventarioMultiAlmacen();
            }

            if(Arguments.Values.CurrentModule == Modules.CAMBIOSMERCANCIA && ParUseMotivoCambio)
            {
                var motivo = new DS_Devoluciones().GetMotivoDevolucionbyId(CurrentProduct.MotIdDevolucion);

                if(motivo != null)
                {
                    var item = MotivosDevolucion.Where(x => x.MotID == motivo.MotID).FirstOrDefault();

                    if (item != null)
                    { 
                        var index = MotivosDevolucion.IndexOf(item);

                        if (index != -1)
                        {
                            CurrentMotivo = MotivosDevolucion[index];
                        }
                    }
                }
                else
                {
                    CurrentMotivo = null;
                }
            }

            if (IsMultiEntrega)
            {
                if (string.IsNullOrWhiteSpace(CurrentProduct.PedFechaEntrega))
                {
                    pickerFechaEntrega.Date = DateTime.Now;
                }
                else
                {
                    pickerFechaEntrega.Date = Convert.ToDateTime(CurrentProduct.PedFechaEntrega);
                }
            }

            var parProductosUnidades = myParametro.GetParCajasUnidadesProductos();

            UseColorAndSize = myParametro.GetParPedidosProductosColoresYTamanos();

            if (UseColorAndSize)
            {
                var rawCode = CurrentProduct.ProCodigo.Split('-');

                editReferencia.Text = rawCode[0];

                lblDescripcion.Text = rawCode[0] + "-" + CurrentProduct.ProDescripcion1;

                if (rawCode.Length > 2)
                {
                    lblCantidad.IsVisible = false;
                    editCantidad.IsVisible = false;
                    lblUnidades.IsVisible = false;
                    editUnidades.IsVisible = false;

                    var todosColores = myUso.GetProductosColores();
                    var todosTamanos = myUso.GetProductosTamanos();

                    var reference = CurrentProduct.ProCodigo.Split('-');

                    colores = new List<UsosMultiples>();
                    tamanos = new List<UsosMultiples>();

                    if (reference != null && reference.Length > 0)
                    {
                        var sizesValidos = myProd.GetProductosSizesAndColorByReferenceSplit(reference[0]);
                        var coloresValidos = myProd.GetProductosSizesAndColorByReferenceSplit(reference[0], true);

                        foreach (var c in coloresValidos)
                        {
                            var col = todosColores.Where(x => x.CodigoUso == c).FirstOrDefault();
                            if (col != null && !colores.Contains(col))
                            {
                                colores.Add(col);
                            }
                        }

                        var color = colores.Where(x => x.CodigoUso == rawCode[2]).FirstOrDefault();

                        if (color != null)
                        {
                            currentColor = color.CodigoUso;
                            btnColor.Text = color.Descripcion;
                        }

                    }
                }
                else
                {
                    UseColorAndSize = false;
                    lblCantidad.IsVisible = true;
                    editCantidad.IsVisible = true;
                    editReferencia.IsVisible = true;
                    lblReferencia.IsVisible = true;

                }
            }

            if (UsarPromociones)
            {
                checkPromocion.IsToggled = CurrentProduct.IndicadorPromocion;
            }            

            if (Arguments.Values.CurrentModule != Modules.COMPRAS && !UseColorAndSize && !myParametro.GetParConvertirCajasAUnidadesSinDetalleProductos())
            {
                editUnidades.IsVisible = CurrentProduct.IndicadorDetalle || parProductosUnidades;
                lblUnidades.IsVisible = CurrentProduct.IndicadorDetalle || parProductosUnidades;
            }

            if (CurrentProduct.Cantidad > 0)
            {
                if (CurrentProduct.IndicadorDocena)
                {
                    editCantidad.Text = (CurrentProduct.Cantidad / 12).ToString();
                }
                else
                {
                    editCantidad.Text = CurrentProduct.Cantidad.ToString();
                }
               
            }

            if (!string.IsNullOrWhiteSpace(CurrentProduct.CedCodigo))
            {
                CurrentCentrosDistribucion = CentrosDistribucion.FirstOrDefault(c => c.CedCodigo.ToString() == CurrentProduct.CedCodigo.ToString());
            }

            if (CurrentProduct.CantidadDetalleR > 0)
            {
                editRevenimiento.Text = CurrentProduct.CantidadDetalleR.ToString();
            }

            if (CurrentProduct.CantidadDetalle > 0)
            {
                editUnidades.Text = CurrentProduct.CantidadDetalle.ToString();
            }

            if(CurrentProduct.CantidadPiezas > 0)
            {
                editCantidadPiezas.Text = CurrentProduct.CantidadPiezas.ToString();
            }

            if (CurrentProduct.DesPorciento > 0)
            {
                editDescuento.Text = CurrentProduct.DesPorciento.ToString();
            }

            if (parProductosUnidades /*&& !CurrentProduct.UnmCodigo.ToUpper().Contains("UND")*/)
            {
                int Paquetes = 0;
                int Unidades = 0;

                var proUnidades = CurrentProduct.ProUnidades;

                if (proUnidades == 0)
                {
                    proUnidades = 1;
                }

                Paquetes = (int)CurrentProduct.Cantidad / proUnidades;
                Unidades = (int)CurrentProduct.Cantidad % proUnidades;

                editCantidad.Text = Paquetes > 0 ? Paquetes.ToString() : "";
                editUnidades.Text = Unidades > 0 ? Unidades.ToString() : "";
            }

            if (ParVentasLotesAutomaticos && CurrentProduct.UsaLote)
            {
                var cantidad = myProd.GetCantidadTotalInTemp((int)Arguments.Values.CurrentModule, proID: CurrentProduct.ProID, posicion:CurrentProduct.Posicion);
                               
                editCantidad.Text = cantidad > 0 ? cantidad.ToString() : "";
            }

            if ((parUsePrecioEditable || isCompraFactura))
            {
                if (CurrentProduct.PrecioTemp > 0)
                {
                    if (myParametro.GetParPedidosEditarPrecioNegconItebis())
                    {
                        //double.TryParse(editDescuento.Text, out double result);
                        //editPrecio.Text = (Math.Round(CurrentProduct.PrecioTemp * ((currentproduct.Itbis / 100) + 1),2)).ToString();
                        editPrecio.Text = (CurrentProduct != null && CurrentProduct.PrecioNeto == 0) && Arguments.Values.CurrentModule == Modules.AUDITORIAPRECIOS ? "" : Math.Round(CurrentProduct.PrecioNeto, 2).ToString();
                    }
                    else
                    {
                        editPrecio.Text = (CurrentProduct != null && CurrentProduct.PrecioTemp == 0) && Arguments.Values.CurrentModule == Modules.AUDITORIAPRECIOS ? "" : CurrentProduct.PrecioTemp.ToString();
                    }
                    
                }
                else if (Arguments.Values.CurrentModule != Modules.INVFISICO && Arguments.Values.CurrentModule != Modules.COLOCACIONMERCANCIAS && CurrentProduct.PrecioTemp == 0)
                {
                    if (myParametro.GetParPedidosEditarPrecioNegconItebis())
                    {
                        editPrecio.Text = (CurrentProduct != null && CurrentProduct.PrecioNeto == 0) && Arguments.Values.CurrentModule == Modules.AUDITORIAPRECIOS ? "" : (Math.Round(/*CurrentProduct.Precio * ((currentproduct.Itbis / 100) + 1)*/CurrentProduct.PrecioNeto, 2)).ToString();
                    }
                    else
                    {
                        editPrecio.Text = (CurrentProduct != null && CurrentProduct.Precio == 0) && Arguments.Values.CurrentModule == Modules.AUDITORIAPRECIOS ? "" : CurrentProduct.Precio.ToString();
                    }
                }
                else
                {
                    editPrecio.Text = "";
                }
            }

            if (parOfertasManuales)
            {
                //CantidadOfertaManual = (int)CurrentProduct.CantidadOferta > 0 ? ((int)CurrentProduct.CantidadOferta).ToString() : "";

                if (CurrentProduct.ProIDOferta > 0)
                {
                    CurrentProductoOferta = myProd.ExistsProductoAgregadoPorOfertaManu((int)Arguments.Values.CurrentModule,CurrentProduct.ProID);

                    if (CurrentProductoOferta != null)
                    {
                        CantidadOfertaManual = CurrentProductoOferta.Cantidad.ToString();
                        ElegirProdLabel = CurrentProductoOferta.Descripcion;
                        CurrentProductoOferta.Precio = CurrentProductoOferta.PrecioTemp;
                    }
                    else
                    {
                        CantidadOfertaManual = "";
                        ElegirProdLabel = AppResource.ChooseProduct;
                    }
                }
                else
                {
                    CantidadOfertaManual = "";
                    ElegirProdLabel = AppResource.ChooseProduct;
                    CurrentProductoOferta = null;
                }
            }

            if (isCompraFactura || myParametro.GetParComprasUsarFacturas())
            {
                editFacturaCompra.Text = CurrentProduct.Documento;
            }

            if (parInvArea && CurrentProduct.InvAreaId != -1 && CurrentProduct.InvAreaId != 0 && InvAreas != null)
            {
                var item = InvAreas.Where(x => x.CodigoUso == CurrentProduct.InvAreaId.ToString()).FirstOrDefault();

                if (item != null && myParametro.GetParInventariosTomarCantidades() <= 0 && myParametro.GetParColocacionProductosTomarCantidades() <= 0)
                {
                    comboInvArea.SelectedItem = item;
                }
            }

            if ((parPedidosDescuentosManuales && CurrentProduct.DesPorcientoManual > 0) || (ParCotizacionesDescuentoManual && CurrentProduct.DesPorcientoManual > 0))
            {
                editDescuento.Text = CurrentProduct.DesPorcientoManual.ToString();
            }
           
            if (parOfertasYDescuentosVisualizar)
            {

                var ofecurrentpro = PedidosViewModel.Instance().CurrentPedidoAEntregar;

                if (myParametro.GetOfertasConSegmento())
                {
                    Ofertas = new ObservableCollection<Ofertas>(myOfe.GetOfertasDisponiblesPorSegmento(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentClient.TiNID, CurrentProduct.ProID,false, ofecurrentpro, CurrentProduct.IndicadorOferta));
                }
                else
                {
                    Ofertas = new ObservableCollection<Ofertas>(myOfe.GetOfertasDisponibles(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentClient.TiNID, CurrentProduct.ProID, false, ofecurrentpro, CurrentProduct.IndicadorOferta));
                }

                if (Ofertas != null && Ofertas.Count > 0)
                {
                    CanSelectOferta = (CantidadOfertasShow = Ofertas.Count) > 1;
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

                //tabOfertas.IsVisible = CurrentOferta != null;
                mainContainer.VerticalOptions = myParametro.GetParPedidosVisualizarFotoEnDetalleProductos() > -1 ? LayoutOptions.Center : LayoutOptions.Start;

                if (myParametro.GetParPedidosDescuentoManualGeneral() <= 0.0)
                {
                    Descuentos = new ObservableCollection<DescuentosRecargos>(myDes.GetDescuentosDisponibles(Arguments.Values.CurrentClient.TiNID, Arguments.Values.CurrentClient.CliID, CurrentProduct.ProID));
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
                    if(Device.Idiom == TargetIdiom.Tablet)
                        descuentosView1.IsVisible = true;
                    else
                        descuentosView.IsVisible = true;
                }
                else
                {
                    Children.RemoveAt(2);
                }

                if(CurrentOferta != null)
                {
                    if (Device.Idiom == TargetIdiom.Tablet)
                        detalleOfertasView1.IsVisible = true;     
                    else
                        detalleOfertasView.IsVisible = true;
                }
                else
                {
                    Children.RemoveAt(1);
                }                
            }
            else
            {
                Children.RemoveAt(2);
                Children.RemoveAt(1);
                Descuentos = null;
                Ofertas = null;
            }

            if(Device.Idiom == TargetIdiom.Tablet)
            {
                for(int i = Children.Count - 1; i >= 1; i--)
                    Children.RemoveAt(i);
            }

            if (myParametro.GetParPedidosVisualizarFotoEnDetalleProductos() > -1)// && !tabLayout.IsVisible)
            {
                imagenProducto.IsVisible = true;
            }

            if (myParametro.GetParInventariosTomarCantidades() > 0 || myParametro.GetParColocacionProductosTomarCantidades() > 0)
            {
                editCantidad.Text = "";
                editUnidades.Text = "";
            }

            if (parPedidosDescuentosManuales && parPedidosTipoDescuentoManual == 2 && CurrentProduct.IndicadorDescuento)
            {
                lblDescuento.IsVisible = true;
                editDescuento.IsVisible = true;
                lblTiposDescuentos.IsVisible = true;
                swtDescuento.IsVisible = true;
                if (myParametro.GetParPedidosOcultarDescuentoManualMonto())
                {
                    lblTiposDescuentos.IsVisible = false;
                    swtDescuento.IsVisible = false;
                }
                layoutPorcDescuento.IsVisible = true;
            }

            if (myParametro.GetParCotOfertasManuales() || myParametro.GetParVenOfertasManuales() || myParametro.GetParPedOfertasManuales())
            {
                string grupoNoOferta = "";

                grupoNoOferta = myParametro.GetParCotOfertasManuales()
                    ? "COTNOOFERTA"
                    : myParametro.GetParVenOfertasManuales()
                        ? "VENNOOFERTA"
                        : myParametro.GetParPedOfertasManuales()
                            ? "PEDNOOFERTA"
                            : "";


                if (!string.IsNullOrWhiteSpace(grupoNoOferta))
                {
                    var prod = myGrupoProductos.GetProductosByGrpCodigo(grupoNoOferta).Where(x => x.ProCodigo == CurrentProduct.ProCodigo).FirstOrDefault();

                    if (prod != null)
                    {
                        lblofertamanual.IsVisible = false;
                        editofertamanual.IsVisible = false;
                    }
                }

            }

            SetKeyBoardReturn();        
        }

        //protected override void OnAppearing()
        protected  override void OnAppearing()
        { 
            base.OnAppearing();
            try
            {

                if (Ofertas == null)
                {
                    if (!string.IsNullOrWhiteSpace(ElegirProdLabel) && ElegirProdLabel.ToLower() != AppResource.ChooseProduct)// && string.IsNullOrWhiteSpace(CantidadOfertaManual))
                    {
                        if (Arguments.Values.CurrentModule == Modules.AUDITORIAPRECIOS)
                        {
                            Functions.FocusInternal(editPrecio);
                        }
                        else
                        {
                            Functions.FocusInternal(editofertamanual);
                        }
                    }
                    else
                    {
                        if (!UseColorAndSize && !myParametro.GetParPedidosProductosUnidades())
                        {
                            if (Arguments.Values.CurrentModule == Modules.AUDITORIAPRECIOS)
                            {
                                Functions.FocusInternal(editPrecio);
                            }
                            else
                            {
                                Functions.FocusInternal(editCantidad);
                            }
                            if (myParametro.GetParPedidosVisualizarFotoEnDetalleProductos() > -1 && myParametro.GetParFiltroCentroDistribucion() != null
                                && myParametro.GetParOcultarImagenInFocus())
                            {
                                imagenProducto.IsVisible = false;
                                mainContainer.VerticalOptions = LayoutOptions.Start;
                            }
                            

                        }
                    }

                }
                else if(!UseColorAndSize && !myParametro.GetParPedidosProductosUnidades())
                {
                    if (Arguments.Values.CurrentModule == Modules.AUDITORIAPRECIOS)
                    {
                        Functions.FocusInternal(editPrecio);
                    }
                    else
                    {
                        Functions.FocusInternal(editCantidad);
                    }
                    
                    if (myParametro.GetParPedidosVisualizarFotoEnDetalleProductos() > -1 && myParametro.GetParFiltroCentroDistribucion() != null
                        && myParametro.GetParOcultarImagenInFocus())
                    {
                        imagenProducto.IsVisible = false;
                        mainContainer.VerticalOptions = LayoutOptions.Start;
                    }
                }
            }
            catch (Exception ex) { Console.Write(ex.Message); }

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

                DescuentoDetalles = new ObservableCollection<DescuentosRecargosDetalle>(myDes.GetDetalles(CurrentDescuento.DesID, CurrentDescuento.DesMetodo == 5 ? CurrentProduct.ProID : -1));
            }
            catch (Exception e)
            {
                DisplayAlert(AppResource.ErrorLoadingDiscountDetail, e.Message, AppResource.Aceptar);
            }
        }

        public void OnToggled(object sender, EventArgs args)
        {
            TipoDescuento = AppResource.PercentDiscountLabel;
            if (!string.IsNullOrWhiteSpace(editDescuento.Text) && currentproduct != null && CurrentProduct.DesPorciento > 0)
            {
                editDescuento.Text = CurrentProduct.DesPorciento.ToString();
            }
            if (IsCambioTipoDescuento)
            {
                TipoDescuento = "$ " +AppResource.DiscountLabel;
                if (!string.IsNullOrWhiteSpace(editDescuento.Text) && currentproduct != null && CurrentProduct.DesPorciento > 0)
                {
                    double desc = Math.Round((CurrentProduct.DesPorciento / 100) * CurrentProduct.Precio, 2, MidpointRounding.AwayFromZero);
                    //desc = Math.Round(desc, 2, MidpointRounding.AwayFromZero);
                    double desc1 = desc * CurrentProduct.Cantidad;
                    desc = Math.Round(desc1, 2, MidpointRounding.AwayFromZero);
                    editDescuento.Text = desc.ToString();//string.Format("{0:n0}", desc);
                }
            }

        }

        public void SetKeyBoardReturn()
        {
            if (editFacturaCompra.IsVisible) 
            {
                valuesNamesViewsVisibles.Add("editFacturaCompra");
            }
            if (editPrecio.IsVisible)
            {
                valuesNamesViewsVisibles.Add("editPrecio");
            }
            if (editCantidad.IsVisible)
            {
                valuesNamesViewsVisibles.Add("editCantidad");
            }
            if (editCantidadPiezas.IsVisible)
            {
                valuesNamesViewsVisibles.Add("editCantidadPiezas");
            }
            if (editUnidades.IsVisible)
            {
                valuesNamesViewsVisibles.Add("editUnidades");
            }
            
            if (editDescuento.IsVisible)
            {
                valuesNamesViewsVisibles.Add("editDescuento");
            }
            if (editofertamanual.IsVisible)
            {
                valuesNamesViewsVisibles.Add("editofertamanual");
            }
            if (editRevenimiento.IsVisible)
            {
                valuesNamesViewsVisibles.Add("editRevenimiento");
            }
            if (editLote.IsVisible)
            {
                valuesNamesViewsVisibles.Add("editLote");
            }

            if(valuesNamesViewsVisibles.Count == 0)
            {
                return;
            }

            switch (valuesNamesViewsVisibles[valuesNamesViewsVisibles.Count-1])
            {
                case "editFacturaCompra":
                    editFacturaCompra.ReturnType = ReturnType.Go;
                    editFacturaCompra.Completed += OnComplete;
                    /*ReturnType="Go" Completed="OnComplete"*/
                    break;
                case "editPrecio":
                    editPrecio.ReturnType = ReturnType.Go;
                    editPrecio.Completed += OnComplete;
                    break;
                default:
                case "editCantidad":
                    editCantidad.ReturnType = ReturnType.Go;
                    editCantidad.Completed += OnComplete;
                    break;
                case "editCantidadPiezas":
                    editCantidadPiezas.ReturnType = ReturnType.Go;
                    editCantidadPiezas.Completed += OnComplete;
                    break;
                case "editUnidades":
                    editUnidades.ReturnType = ReturnType.Go;
                    editUnidades.Completed += OnComplete;
                    break;
                case "editDescuento":
                    editDescuento.ReturnType = ReturnType.Go;
                    editDescuento.Completed += OnComplete;
                    break;
                case "editofertamanual":
                    editofertamanual.ReturnType = ReturnType.Go;
                    editofertamanual.Completed += OnComplete;
                    break;
                case "editRevenimiento":
                    editRevenimiento.ReturnType = ReturnType.Go;
                    editRevenimiento.Completed += OnComplete;
                    break;
                case "editLote":
                    editLote.ReturnType = ReturnType.Go;
                    editLote.Completed += OnComplete;
                    break;

            }



        }

        void OnComplete(object sender, EventArgs e)
        {
            switch (valuesNamesViewsVisibles[valuesNamesViewsVisibles.Count - 1])
            {
                case "editFacturaCompra":
                    editFacturaCompra.ReturnType = ReturnType.Go;
                    editFacturaCompra.Completed += OnComplete;
                    editFacturaCompra.ReturnCommand = SaveProducto;
                    /*ReturnType="Go" Completed="OnComplete"*/
                    break;
                case "editPrecio":
                    editPrecio.ReturnType = ReturnType.Go;
                    editPrecio.Completed += OnComplete;
                    editPrecio.ReturnCommand = SaveProducto;
                    break;
                default:
                case "editCantidad":
                    editCantidad.ReturnType = ReturnType.Go;
                    editCantidad.Completed += OnComplete;
                    editCantidad.ReturnCommand = SaveProducto;
                    break;
                case "editCantidadPiezas":
                    editCantidadPiezas.ReturnType = ReturnType.Go;
                    editCantidadPiezas.Completed += OnComplete;
                    editCantidadPiezas.ReturnCommand = SaveProducto;
                    break;
                case "editUnidades":
                    editUnidades.ReturnType = ReturnType.Go;
                    editUnidades.Completed += OnComplete;
                    editUnidades.ReturnCommand = SaveProducto;
                    break;
                case "editDescuento":
                    editDescuento.ReturnType = ReturnType.Go;
                    editDescuento.Completed += OnComplete;
                    editDescuento.ReturnCommand = SaveProducto;
                    break;
                case "editofertamanual":
                    editofertamanual.ReturnType = ReturnType.Go;
                    editofertamanual.Completed += OnComplete;
                    editofertamanual.ReturnCommand = SaveProducto;
                    break;
                case "editRevenimiento":
                    editRevenimiento.ReturnType = ReturnType.Go;
                    editRevenimiento.Completed += OnComplete;
                    editRevenimiento.ReturnCommand = SaveProducto;
                    break;
                case "editLote":
                    editLote.ReturnType = ReturnType.Go;
                    editLote.Completed += OnComplete;
                    editLote.ReturnCommand = SaveProducto;
                    break;
            }
        }

    }
}