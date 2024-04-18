using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.Model;
using MovilBusiness.Printer;
using MovilBusiness.Printer.Formats;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using MovilBusiness.Views;
using MovilBusiness.Views.Components.Modals;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class ProspectosViewModel : BaseViewModel
    {
        public ICommand OnClickCommand { get; private set; }

        public bool InputTransparent { get; set; } = false;

        public bool HasGps { get; private set; }

        public bool OrdenAVisitarEsVisible { get => DS_RepresentantesParametros.GetInstance().GetParProspectoCliRutPosicion(); }
        public bool OrdenRutaEsVisible { get => DS_RepresentantesParametros.GetInstance().GetParProspectoCliRutOrden(); }

        private bool showemail { get; set; }
        public bool ShowEmail { get => showemail; set { showemail = value; RaiseOnPropertyChanged(); } }
        private bool showcanalventa { get; set; }
        public bool ShowCanalVenta { get => showcanalventa; set { showcanalventa = value; RaiseOnPropertyChanged(); } }
        
        private bool showtipolocales { get; set; }
        public bool ShowTipoLocales { get => showtipolocales; set { showtipolocales = value; RaiseOnPropertyChanged(); } }
        private bool showruta { get; set; }
        public bool ShowRuta { get => showruta; set { showruta = value; RaiseOnPropertyChanged(); } }
        private bool showtipocomprobante { get; set; }
        public bool ShowTipoComprobante { get => showtipocomprobante; set { showtipocomprobante = value; RaiseOnPropertyChanged(); } }
        private bool showtiponegocio { get; set; }
        public bool ShowTipoNegocio { get => showtiponegocio; set { showtiponegocio = value; RaiseOnPropertyChanged(); } }
        private bool showtipocliente { get; set; }
        public bool ShowTipoCliente { get => showtipocliente; set { showtipocliente = value; RaiseOnPropertyChanged(); } }
        private bool showfrecuencia { get; set; }
        public bool ShowFrecuencia { get => showfrecuencia; set { showfrecuencia = value; RaiseOnPropertyChanged(); } }
        private bool showclasificacion { get; set; }
        public bool ShowClasificacion { get => showclasificacion; set { showclasificacion = value; RaiseOnPropertyChanged(); } }
        private bool istoggledvisitar { get; set; }
        public bool IsToggledVisitar { get => istoggledvisitar; set { istoggledvisitar = value; CalcularVisitasToBoolUnasync(true); RaiseOnPropertyChanged(); } }
        private bool isdespuesenabled { get; set; }
        public bool IsDespuesEnabled { get => isdespuesenabled; set { isdespuesenabled = value; RaiseOnPropertyChanged(); } }
        private bool isantesenabled { get; set; }
        public bool IsAntesEnabled { get => isantesenabled; set { isantesenabled = value; RaiseOnPropertyChanged(); } }
        private bool showregmercantil { get; set; }
        public bool ShowRegMercantil { get => showregmercantil; set { showregmercantil = value; RaiseOnPropertyChanged(); } }
        public List<UsosMultiples> TiposClientes { get; private set; }
        private ObservableCollection<CanalDistribucion> tiposcanales { get; set; }
        public ObservableCollection<CanalDistribucion> TiposDeCanales{get => tiposcanales; set { tiposcanales = value; RaiseOnPropertyChanged(); } }
        private ObservableCollection<UsosMultiples> frecuencias { get; set; }
        public ObservableCollection<UsosMultiples> Frecuencias { get => frecuencias; set { frecuencias = value; RaiseOnPropertyChanged(); } }
        private UsosMultiples currentfrecuencias { get; set; }
        public UsosMultiples CurrentFrecuencias { get => currentfrecuencias; set { currentfrecuencias = value; RaiseOnPropertyChanged(); } }
        public List<string> TiposCanales { get; private set; }
        public List<UsosMultiples> TiposLocales { get; private set; }
        public ObservableCollection<CondicionesPago> CondicionesPagos { get; private set; }
        public List<Clientes> ClientesNombres { get; private set; }

        public List<Territorios> Territorios { get; private set; }
        public List<UsosMultiples> TiposComprobante { get; private set; }
        public List<UsosMultiples> TiposReferencia { get; private set; }
        public List<UsosMultiples> ListaPrecios { get; private set; }
        public List<Estados> EstadosClientes { get; private set; }


        private string cliurbanizacion;
        public string CliUrbanizacion { get => cliurbanizacion; set { cliurbanizacion = value; RaiseOnPropertyChanged(); } }
        private bool isvsibleentryeector { get; set; }
        public bool IsVisibleEntrySector { get => isvsibleentryeector; set { isvsibleentryeector = value; RaiseOnPropertyChanged(); } }
        private bool isvisiblepickersector { get; set; }
        public bool IsVisiblePickerSector { get => isvisiblepickersector; set { isvisiblepickersector = value; RaiseOnPropertyChanged(); } }


        /// <summary>
        /// para completar el campo visitas antes de
        /// </summary>
        /// 
        public List<Visitas> VisitarAntes { get; private set; }

        private bool IsProspectSaved = false;

        /// <summary>
        /// ///////////////////controles/////////////////////
        /// </summary>
        private ObservableCollection<Provincias> provincias;
        public ObservableCollection<Provincias> Provincias { get => provincias; set { provincias = value; RaiseOnPropertyChanged(); } }
        private ObservableCollection<Municipios> municipios;
        public ObservableCollection<Municipios> Municipios { get => municipios; set { municipios = value; RaiseOnPropertyChanged(); } }
        private ObservableCollection<SectoresMunicipios> munsectores;
        public ObservableCollection<SectoresMunicipios> MunSectores { get => munsectores; set { munsectores = value; RaiseOnPropertyChanged(); } }
        private ObservableCollection<Rutas> rutas;
        public ObservableCollection<Rutas> Rutas { get => rutas; set { rutas = value; RaiseOnPropertyChanged(); } }

        private CanalDistribucion currentcanalventa;
        public CanalDistribucion CurrentCanalVenta { get => currentcanalventa; set { currentcanalventa = value; RaiseOnPropertyChanged(); } }


        private string clifax;
        public string CliFax { get => clifax; set { clifax = value; RaiseOnPropertyChanged(); } }

        private UsosMultiples currenttipocliente;
        public UsosMultiples CurrentTipoCliente { get => currenttipocliente; set { currenttipocliente = value; RaiseOnPropertyChanged(); } }
        private UsosMultiples currenttipolocal;
        public UsosMultiples CurrentTipoLocal { get => currenttipolocal; set { currenttipolocal = value; RaiseOnPropertyChanged(); } }
        private CondicionesPago currentcondicionpago;
        public CondicionesPago CurrentCondicionPago { get => currentcondicionpago; set { currentcondicionpago = value; RaiseOnPropertyChanged(); } }
        private Territorios currentterritorio;
        public Territorios CurrentTerritorio { get => currentterritorio; set { currentterritorio = value; OnCurrentTerritorioChanged(); RaiseOnPropertyChanged(); } }
        private Provincias currentprovincia;
        public Provincias CurrentProvincia { get => currentprovincia; set { currentprovincia = value; OnCurrentProvinciaChanged(); RaiseOnPropertyChanged(); } }
        private Municipios currentmunicipio;
        public Municipios CurrentMunicipio { get => currentmunicipio; set { currentmunicipio = value; OnCurrentMunicipioChanged(); RaiseOnPropertyChanged(); } }
        private SectoresMunicipios currentmunsector;
        public SectoresMunicipios CurrentMunSector { get => currentmunsector; set { currentmunsector = value; RaiseOnPropertyChanged(); } }
        private UsosMultiples currenttiporeferencia;
        public UsosMultiples CurrentTipoReferencia { get => currenttiporeferencia; set { currenttiporeferencia = value; OnTipoReferenciaChanged(); RaiseOnPropertyChanged(); } }

        private Clasifica currentclasificacion;
        public Clasifica CurrentClasificacion { get => currentclasificacion; set { currentclasificacion = value; RaiseOnPropertyChanged(); } }

        private List<Clasifica> clasificacion;
        public List<Clasifica> Clasificacion { get => clasificacion; set { clasificacion = value; RaiseOnPropertyChanged(); } }

        private UsosMultiples currentlistaprecios;
        public UsosMultiples CurrentListaPrecios { get => currentlistaprecios; set { currentlistaprecios = value; RaiseOnPropertyChanged(); } }
        private Estados currentestadocliente;
        public Estados CurrentEstadoCliente { get => currentestadocliente; set { currentestadocliente = value; RaiseOnPropertyChanged(); } }
        private UsosMultiples currenttipocomprobante;
        public UsosMultiples CurrentTipoComprobante { get => currenttipocomprobante; set { currenttipocomprobante = value; RaiseOnPropertyChanged(); } }

        //Cliente a visitar depues de
        private Clientes currenClienteAVisitarDespuesDe;
        public Clientes CurrenClienteAVisitarDespuesDe { get => currenClienteAVisitarDespuesDe; set { currenClienteAVisitarDespuesDe = value; RaiseOnPropertyChanged(); } }

        //Cliente a Ordenar depues de
        private Clientes currenClienteAOrdenarDespuesDe;
        public Clientes CurrenClienteAOrdenarDespuesDe { get => currenClienteAOrdenarDespuesDe; set { currenClienteAOrdenarDespuesDe = value; RaiseOnPropertyChanged(); } }

        private Rutas currentruta;
        public Rutas CurrentRuta { get => currentruta; set { currentruta = value; RaiseOnPropertyChanged(); } }

        private string clinombre;
        public string CliNombre { get => clinombre; set { clinombre = value; RaiseOnPropertyChanged(); } }
        private string clirnc;
        public string CliRNC { get => clirnc; set { clirnc = value; RaiseOnPropertyChanged(); } }
        private string clitelefono;
        public string CliTelefono { get => clitelefono; set { clitelefono = value; RaiseOnPropertyChanged(); } }
        private string clicalle;
        public string CliCalle { get => clicalle; set { clicalle = value; RaiseOnPropertyChanged(); } }
        private string clicasa;
        public string CliCasa { get => clicasa; set { clicasa = value; RaiseOnPropertyChanged(); } }
        private string clisector;
        public string CliSector { get => clisector; set { clisector = value; RaiseOnPropertyChanged(); } }
        private string clicontacto;
        public string CliContacto { get => clicontacto; set { clicontacto = value; RaiseOnPropertyChanged(); } }
       
        private string clicontactocelular;
        public string CliContactoCelular { get => clicontactocelular; set { clicontactocelular = value; RaiseOnPropertyChanged(); } }
        
        private string clirutposiciones;
        public string CliRutPosiciones { get => clirutposiciones; set { clirutposiciones = value; RaiseOnPropertyChanged(); } }

        private string cliordenrutas;
        public string CliOrdenRutas { get => cliordenrutas; set { cliordenrutas = value; RaiseOnPropertyChanged(); } }

        //////referencias
        private string clirefnombre;
        public string CliRefNombre { get => clirefnombre; set { clirefnombre = value; RaiseOnPropertyChanged(); } }
        private string clireftelefono;
        public string CliRefTelefono { get => clireftelefono; set { clireftelefono = value; RaiseOnPropertyChanged(); } }
        private string clirefcuenta;
        public string CliRefCuenta { get => clirefcuenta; set { clirefcuenta = value; RaiseOnPropertyChanged(); } }

        private ObservableCollection<ClientesReferencias> clientesreferencias;
        public ObservableCollection<ClientesReferencias> ClientesReferencias { get => clientesreferencias; set { clientesreferencias = value; RaiseOnPropertyChanged(); } }

        private ObservableCollection<TiposNegocio> tipNegocio;
        public ObservableCollection<TiposNegocio> TipNegocio { get => tipNegocio; set { tipNegocio = value; RaiseOnPropertyChanged(); } }

        private TiposNegocio tipnombre;
        public TiposNegocio TipNombre { get => tipnombre; set { tipnombre = value; RaiseOnPropertyChanged(); } }
        public List<Bancos> Bancos { get; private set; }
        private Bancos currentBanco;
        public Bancos CurrentBanco { get => currentBanco; set { currentBanco = value; RaiseOnPropertyChanged(); } }
        public bool bancoEnable;
        public bool BancoEnable { get => bancoEnable; set { bancoEnable = value; RaiseOnPropertyChanged(); } }

        private string clinombreemisioncheque;
        public string CliNombreEmisionCheques { get => clinombreemisioncheque; set { clinombreemisioncheque = value; RaiseOnPropertyChanged(); } }
        private double cliestimadocompras;
        public double CliEstimadoCompras { get => cliestimadocompras; set { cliestimadocompras = value; RaiseOnPropertyChanged(); } }
        private double cliinventario;
        public double CliInventario { get => cliinventario; set { cliinventario = value; RaiseOnPropertyChanged(); } }
        private double clilimitecreditosolicitado;
        public double CliLimiteCreditoSolicitado { get => clilimitecreditosolicitado; set { clilimitecreditosolicitado = value; RaiseOnPropertyChanged(); } }
        private string clipropietario;
        public string CliPropietario { get => clipropietario; set { clipropietario = value; RaiseOnPropertyChanged(); } }
        private string clipropietariodireccion;
        public string CliPropietarioDireccion { get => clipropietariodireccion; set { clipropietariodireccion = value; RaiseOnPropertyChanged(); } }
        private string clicedulapropietario;
        public string CliCedulaPropietario { get => clicedulapropietario; set { clicedulapropietario = value; RaiseOnPropertyChanged(); } }
        
        private string clipropietariotelefono;
        public string CliPropietarioTelefono { get => clipropietariotelefono; set { clipropietariotelefono = value; RaiseOnPropertyChanged(); } }
        private string clipropietariocelular;
        public string CliPropietarioCelular { get => clipropietariocelular; set { clipropietariocelular = value; RaiseOnPropertyChanged(); } }
        private string clifamiliarnombre;
        public string CliFamiliarNombre { get => clifamiliarnombre; set { clifamiliarnombre = value; RaiseOnPropertyChanged(); } }
        private string clifamiliardireccion;
        public string CliFamiliarDireccion { get => clifamiliardireccion; set { clifamiliardireccion = value; RaiseOnPropertyChanged(); } }
        private string clifamiliarcedula;
        public string CliFamiliarCedula { get => clifamiliarcedula; set { clifamiliarcedula = value; RaiseOnPropertyChanged(); } }
        private string clifamiliartelefono;
        public string CliFamiliarTelefono { get => clifamiliartelefono; set { clifamiliartelefono = value; RaiseOnPropertyChanged(); } }
        private string clifamiliarcelular;
        public string CliFamiliarCelular { get => clifamiliarcelular; set { clifamiliarcelular = value; RaiseOnPropertyChanged(); } }
        private double latitud;
        public double Latitud { get => latitud; set { latitud = value; RaiseOnPropertyChanged(); } }
        private double longitud;
        public double Longitud { get => longitud; set { longitud = value; RaiseOnPropertyChanged(); } }    

        private string clicorreoelectronico;
        public string CliCorreoElectronico { get => clicorreoelectronico; set { clicorreoelectronico = value; RaiseOnPropertyChanged(); } }

        private string cliRegMercantil;
        public string CliRegMercantil { get => cliRegMercantil; set { cliRegMercantil = value; RaiseOnPropertyChanged(); } }

        private DS_Provincias myProv;
        private DS_Municipios myMun;
        private DS_Clientes myCli;
        private DS_Rutas myRut;
        private DS_TransaccionesImagenes myTraImg;

        public Action IsGoingToSave { get; set; }
        public Action IsSaved { get; set; }
        public Action<bool> IsPositionOut { get; set; }
        public Action<bool> IsOrdenOut { get; set; }

        private ProspectosFormats printer;

        private string ParCamposObligatorios;
        public string ParListaPreciosDefault;

        public ProspectosViewModel(Page page, Clientes EditedPropesto = null) : base(page)
        {
            IsAntesEnabled = true;
            ResetPickers();
            var uso = new DS_UsosMultiples();
            var canal = new DS_CanalDistribucion();
            OnClickCommand = new Command(OnClickListener);

            TipNegocio = new ObservableCollection<TiposNegocio>( new DS_TiposNegocio().GetTipo());

            HasGps = myParametro.GetParGPS();
            ShowRegMercantil = !DS_RepresentantesParametros.GetInstance().GetParShowCamposEspecificos().Contains("registromercantil");
            ShowEmail = !DS_RepresentantesParametros.GetInstance().GetParShowCamposEspecificos().Contains("email");
            ShowCanalVenta = !DS_RepresentantesParametros.GetInstance().GetParShowCamposEspecificos().Contains("canalventa");
            ShowTipoLocales = !DS_RepresentantesParametros.GetInstance().GetParShowCamposEspecificos().Contains("tipolocal");
            ShowRuta = !DS_RepresentantesParametros.GetInstance().GetParShowCamposEspecificos().Contains("ruta");
            ShowTipoComprobante = !DS_RepresentantesParametros.GetInstance().GetParShowCamposEspecificos().Contains("tipocomprobante");
            ShowTipoNegocio = !DS_RepresentantesParametros.GetInstance().GetParShowCamposEspecificos().Contains("tiponegocio");
            ShowTipoCliente = !DS_RepresentantesParametros.GetInstance().GetParShowCamposEspecificos().Contains("tipocliente");
            ShowClasificacion = !DS_RepresentantesParametros.GetInstance().GetParShowCamposEspecificos().Contains("clasificacion");
            ShowFrecuencia = !DS_RepresentantesParametros.GetInstance().GetParShowCamposEspecificos().Contains("frecuencia");



            if (HasGps && Arguments.Values.CurrentLocation != null)
            {
                Latitud = Arguments.Values.CurrentLocation.Latitude;
                Longitud = Arguments.Values.CurrentLocation.Longitude;
            }

            myCli = new DS_Clientes();
            myProv = new DS_Provincias();
            myMun = new DS_Municipios();
            myRut = new DS_Rutas();
            myTraImg = new DS_TransaccionesImagenes();

            myTraImg.DeleteTemp(false, "Prospectos", DS_RepresentantesSecuencias.GetLastSecuencia("SolicitudesCredito").ToString());

            ParCamposObligatorios = myParametro.GetParProspectosCampoObligatorios();

            if (ParCamposObligatorios == null) { ParCamposObligatorios = ""; } else { ParCamposObligatorios = ParCamposObligatorios.ToUpper(); }

            printer = new ProspectosFormats(myCli);

            ClientesReferencias = new ObservableCollection<ClientesReferencias>();
            TiposClientes = uso.GetTiposClientes();

            string espfornegocios = "", espforCliente = "", espforlocal = "", espforcomp = "", espforest = "";
            int fortipcan = -1, fortipconpago = -1;

            if (Arguments.CurrentUser.IsAuditor)
            {
                string representantes = new DS_Representantes().
                    GetRepCargoRepresentantes(App.Current.Properties["CurrentRep"].ToString());

                espforest = myParametro.GetParCamposEspecificosForEstatus("-" + representantes);
                espforcomp = myParametro.GetParCamposEspecificosForTipComprobante("-" + representantes);
                espforlocal = myParametro.GetParCamposEspecificosForTiPLocal("-" + representantes);
                espforCliente = myParametro.GetParCamposEspecificosForTiPCliente("-" + representantes);
                espfornegocios = myParametro.GetParCamposEspecificosForTiPNegocios("-" + representantes);
                fortipcan = myParametro.GetParCamposEspecificosForTiposCanales("-" + representantes);
                fortipconpago = myParametro.GetParCamposEspecificosForCondicionPago("-" + representantes);
            }
            else
            {
                espforest = myParametro.GetParCamposEspecificosForEstatus();
                espforcomp = myParametro.GetParCamposEspecificosForTipComprobante();
                espforlocal = myParametro.GetParCamposEspecificosForTiPLocal();
                espforCliente = myParametro.GetParCamposEspecificosForTiPCliente();
                espfornegocios = myParametro.GetParCamposEspecificosForTiPNegocios();
                fortipcan = myParametro.GetParCamposEspecificosForTiposCanales();
                fortipconpago = myParametro.GetParCamposEspecificosForCondicionPago();
            }

            TiposDeCanales = new ObservableCollection<CanalDistribucion>(canal.GetAllCanales());
            //TiposCanales = new List<string>();
            if (TiposDeCanales.Count > 1)
            {
                /*TiposCanales.Add("(Seleccione)");

                foreach (var canales in TiposDeCanales)
                {
                    TiposCanales.Add(canales.CanDescripcion);
                }*/

                if (fortipcan >= 0)
                {
                    CurrentCanalVenta = TiposDeCanales.Where(t => t.CanID == fortipcan).FirstOrDefault();
                }
            }
          
            var myCla = new DS_Clientes();
            Clasificacion = myCla.GetClasificacionClientes();

            TiposLocales = uso.GetTiposLocales();
            CondicionesPagos = new DS_CondicionesPago().GetAllCondicionesPago();
            var parConcionesPagosExcluidas = myParametro.GetParProspectoCondicionesPagosNOValidas().ToList();
            if (parConcionesPagosExcluidas != null && parConcionesPagosExcluidas.Count  > 0)
            {
                var condicionesPagosValidas = CondicionesPagos.Where(c => !parConcionesPagosExcluidas.Contains(c.ConReferencia));
                CondicionesPagos = new ObservableCollection<CondicionesPago>(condicionesPagosValidas);
            }

            if(fortipconpago >= 0)
            {
                CurrentCondicionPago = CondicionesPagos.Where(c => c.ConID == fortipconpago).FirstOrDefault();
            }

            ParListaPreciosDefault = myParametro.GetParProspectosListaPreciosDefault();

            Territorios = new DS_Territorios().GetTerritorios();
            TiposComprobante = uso.GetTiposComprobante2018();
            TiposReferencia = uso.GetTiposReferenciasProspectos();
            ListaPrecios = uso.GetAllListaPrecios();
            EstadosClientes = myCli.GetTiposEstadosClientes();

            Frecuencias = new ObservableCollection<UsosMultiples>(uso.GetUsoByCodigoGrupo("CliFrecuenciaVisita"));

            if (TiposComprobante != null && !string.IsNullOrEmpty(espforcomp))
            {
                CurrentTipoComprobante = TiposComprobante.Where(t => t.Descripcion == espforcomp).FirstOrDefault();
            }

            if (EstadosClientes != null && !string.IsNullOrEmpty(espforest))
            {
                CurrentEstadoCliente = EstadosClientes.Where(e => e.EstDescripcion == espforest).FirstOrDefault();
            }

            if (TiposLocales != null && !string.IsNullOrEmpty(espforlocal))
            {
                CurrentTipoLocal = TiposLocales.Where(e => e.Descripcion == espforlocal).FirstOrDefault();
            }

            if (TiposClientes != null && !string.IsNullOrEmpty(espforCliente))
            {
                CurrentTipoCliente = TiposClientes.Where(e => e.Descripcion == espforCliente).FirstOrDefault();
            }

            if (TipNegocio != null && !string.IsNullOrEmpty(espfornegocios))
            {
                TipNombre = TipNegocio.Where(e => e.TinDescripcion == espfornegocios).FirstOrDefault();
            }

            if (OrdenAVisitarEsVisible)
            {
                ClientesNombres = myCli.GetAllClientesRutPosicion();
                if (ClientesNombres != null && ClientesNombres.Count > 0)
                {
                    var ultimoCliente = myCli.UltimoClienteVisitado();
                    CurrenClienteAVisitarDespuesDe = ClientesNombres.FirstOrDefault(v => v.CliID == ultimoCliente.CliID);                    

                    if(CurrenClienteAVisitarDespuesDe != null)
                    {
                        CliRutPosiciones = IsDespuesEnabled ? (currenClienteAVisitarDespuesDe.CliRutPosicion + 1).ToString() :
                        (currenClienteAVisitarDespuesDe.CliRutPosicion - 1).ToString();
                    }
                }
            }

            if (OrdenRutaEsVisible)
            {
                ClientesNombres = myCli.GetAllClientesRutPosicion();
                if (ClientesNombres != null && ClientesNombres.Count > 0)
                {
                    var ultimoCliente = myCli.UltimoClienteVisitado();
                    CurrenClienteAOrdenarDespuesDe = ClientesNombres.FirstOrDefault(v => v.CliID == ultimoCliente.CliID);

                    if(CurrenClienteAOrdenarDespuesDe != null)
                    {
                        CurrenClienteAOrdenarDespuesDe.CliNombre = CurrenClienteAOrdenarDespuesDe.CliOrdenRuta 
                            + "/" + CurrenClienteAOrdenarDespuesDe.CliNombre;                        
                        CliOrdenRutas = CurrenClienteAOrdenarDespuesDe.CliOrdenRuta.ToString();
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(ParListaPreciosDefault))
            {
                var def = ListaPrecios.Where(x => x.CodigoUso.Trim().ToUpper().Equals(ParListaPreciosDefault.Trim().ToUpper())).FirstOrDefault();

                if(def != null)
                {
                    CurrentListaPrecios = def;
                }
                else
                {
                    ListaPrecios = new List<UsosMultiples>() { new UsosMultiples() { CodigoGrupo = "LIPCODIGO", CodigoUso = ParListaPreciosDefault, Descripcion = AppResource.DefaultList } };

                    CurrentListaPrecios = ListaPrecios[0];
                }
            }

            Bancos = new DS_Bancos().GetBancos();

            //ResetPickers();
            // SetValueSpikersDefault();
        }

        public void ResetPickers()
        {
            CurrentTipoCliente = null;
            CurrentTipoLocal = null;
            CurrentCondicionPago = null;
            CurrentTerritorio = null;
            CurrentTipoComprobante = null;
            CurrentTipoReferencia = null;
            if (string.IsNullOrWhiteSpace(ParListaPreciosDefault))
            {
                CurrentListaPrecios = null;
            }
            CurrentEstadoCliente = null;
            CurrentProvincia = null;
            CurrentRuta = null;
            CurrenClienteAVisitarDespuesDe = null;
            CurrenClienteAOrdenarDespuesDe = null;
            currentBanco = null;

        }

        private void OnCurrentTerritorioChanged()
        {
            if (CurrentTerritorio == null)
            {
                Provincias = null;
                CurrentProvincia = null;
                return;
            }

            Provincias = new ObservableCollection<Provincias>(myProv.GetProvincias(terCodigo: CurrentTerritorio.TerCodigo));
            Rutas = new ObservableCollection<Rutas>(myRut.GetRutaByTerritorioId(CurrentTerritorio.TerCodigo));
            CurrentProvincia = Provincias.Count == 1 ? Provincias.FirstOrDefault() : null;
            CurrentRuta = Rutas.Count == 1 ? Rutas.FirstOrDefault() : null;
        }

        private void OnCurrentProvinciaChanged()
        {
            if (CurrentProvincia == null)
            {
                Municipios = null;
                CurrentMunicipio = null;
                return;
            }

            Municipios = new ObservableCollection<Municipios>(myMun.GetMunicipiosByProvincia(CurrentProvincia.ProID));
            CurrentMunicipio = null;
        }

        private void OnCurrentMunicipioChanged()
        {
            if (CurrentMunicipio == null)
            {
                MunSectores = null;
                CurrentMunSector = null;
                return;
            }

            MunSectores = new ObservableCollection<SectoresMunicipios>(myMun.GetSectoresMunicipios(CurrentMunicipio.MunID));
            IsVisibleEntrySector = MunSectores == null || MunSectores.Count <= 0;
            IsVisiblePickerSector = !IsVisibleEntrySector;
            CurrentMunSector = null;
        }

        private void OnTipoReferenciaChanged()
        {
            if (CurrentTipoReferencia == null || CurrentTipoReferencia.CodigoUso != "2")
            {
                CurrentBanco = null;
                BancoEnable = false;
            }
            else
            {
                BancoEnable = true;
            }

        }

        public async void OnClickListener(object id)
        {
            if (IsBusy || IsProspectSaved)
            {
                return;
            }

            try
            {
                IsBusy = true;

                switch (id.ToString())
                {
                    case "0": //go camera
                        await PushAsync(new CameraPage(DS_RepresentantesSecuencias.GetLastSecuencia("SolicitudesCredito").ToString(), "Prospectos"));
                        break;
                    case "1": //save
                        GuardarProspecto();
                        break;
                    case "2": //agregar referencia
                        AgregarReferencia();
                        break;
                    case "3"://eliminar todas las referencias
                        ClientesReferencias.Clear();
                        break;
                    case "4"://comprobar gps
                        if (HasGps && Latitud != 0 && Longitud != 0)
                        {
                            await PushAsync(new MapsPage(Latitud, Longitud, AppResource.CurrentLocation));
                        }
                        break;
                    case "5"://actualizar gps
                        if (HasGps && Arguments.Values.CurrentLocation != null)
                        {
                            Latitud = Arguments.Values.CurrentLocation.Latitude;
                            Longitud = Arguments.Values.CurrentLocation.Longitude;
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        void LimpiarCamposClientesReferencia()
        {
            CliRefCuenta = "";
            CliRefTelefono = "";
            CliRefNombre = "";
            currentBanco = null;
            CurrentTipoReferencia = null;

        }
        private void AgregarReferencia()
        {
            if (CurrentTipoReferencia == null)
            {
                DisplayAlert(AppResource.Warning, AppResource.MustSelectReferenceType);
                return;
            }

            if (string.IsNullOrWhiteSpace(CliRefNombre))
            {
                DisplayAlert(AppResource.Warning, AppResource.NameCannotBeEmpty);
                return;
            }

            if (CurrentTipoReferencia.CodigoUso == "2" && CurrentBanco == null)
            {
                DisplayAlert(AppResource.Warning, AppResource.MustChooseBankWarning);
                return;
            }

            var referencia = new ClientesReferencias
            {
                CliRefNombre = CliRefNombre,
                CliRefTelefono = CliRefTelefono,
                CliRefTipo = CurrentTipoReferencia.CodigoUso,
                CliRefCuenta = CliRefCuenta,
                RefTipoDesc = CurrentTipoReferencia.Descripcion,
                rowguid = Guid.NewGuid().ToString(),
                BanID  = CurrentBanco?.BanID,
            };

            ClientesReferencias.Add(referencia);
    
            LimpiarCamposClientesReferencia();
        }

        public void EliminarReferencia(string rowguid)
        {
            if (string.IsNullOrWhiteSpace(rowguid))
            {
                return;
            }

            var item = ClientesReferencias.Where(x => x.rowguid == rowguid).FirstOrDefault();

            if (item == null)
            {
                return;
            }
            ClientesReferencias.Remove(item);
        }

        private void ValidarDatos()
        {
            if (string.IsNullOrWhiteSpace(ParCamposObligatorios))
            {
                return;
            }
            if (ParCamposObligatorios.Contains("CLIVISPOS") && string.IsNullOrEmpty(CliRutPosiciones))
            {
                throw new Exception(AppResource.ClientVisitSequenceRequiredWarning);
            }

            if (ParCamposObligatorios.Contains("CLIVISORDRUT") && string.IsNullOrEmpty(CliOrdenRutas))
            {
                throw new Exception(AppResource.RouteOrderRequiredWarning);
            }

            if (ParCamposObligatorios.Contains("CLITIPOLOCAL") && CurrentTipoLocal == null)
            {
                throw new Exception(AppResource.MustSpecifyVenueType);
            }

            if (ParCamposObligatorios.Contains("CONID") && CurrentCondicionPago == null)
            {
                throw new Exception(AppResource.MustSpecifyPaymentCondition);
            }

            if (ParCamposObligatorios.Contains("CLIESTATUS") && CurrentEstadoCliente == null)
            {
                throw new Exception(AppResource.MustSpecifyCustomerStatus);
            }

            if (ParCamposObligatorios.Contains("TINID") && TipNombre == null)
            {
                throw new Exception(AppResource.MustSpecifyBusinessType);
            }

            if (ParCamposObligatorios.Contains("CLITELEFONO") && string.IsNullOrWhiteSpace(CliTelefono))
            {
                throw new Exception(AppResource.MustSpecifyCustomerPhone);
            }

            if (ParCamposObligatorios.Contains("TERCODIGO") && CurrentTerritorio == null)
            {
                throw new Exception(AppResource.MustSpecifyTerritory);
            }

            if (ParCamposObligatorios.Contains("PROID") && CurrentProvincia == null)
            {
                throw new Exception(AppResource.MustSpecifyProvince);
            }

            if (ParCamposObligatorios.Contains("MUNID") && CurrentMunicipio == null)
            {
                throw new Exception(AppResource.MustSpecifyCustomerMunicipality);
            }

            if (ParCamposObligatorios.Contains("SECCODIGO") && CurrentMunSector == null)
            {
                throw new Exception(AppResource.MustSpecifySectorOfMunicipality);
            }

            if (ParCamposObligatorios.Contains("CLICALLE") && string.IsNullOrWhiteSpace(CliCalle))
            {
                throw new Exception(AppResource.MustSpecifyCustomerStreet);
            }

            if (ParCamposObligatorios.Contains("CLICASA") && string.IsNullOrWhiteSpace(CliCasa))
            {
                throw new Exception(AppResource.MustSpecifyCustomerAddrNo);
            }

            if (IsVisibleEntrySector && ParCamposObligatorios.Contains("CLISECTOR") && string.IsNullOrWhiteSpace(CliSector))
            {
                throw new Exception(AppResource.MustSpecifyCustomerNeighborhood);
            }

            if (ParCamposObligatorios.Contains("CLICONTACTO") && string.IsNullOrWhiteSpace(CliContacto))
            {
                throw new Exception(AppResource.MustSpecifyContact);
            }

            if (ParCamposObligatorios.Contains("CLICONTACTOCELULAR") && string.IsNullOrWhiteSpace(CliContactoCelular))
            {
                throw new Exception(AppResource.MustSpecifyContactPhone);
            }

            if (ParCamposObligatorios.Contains("EMAIL") && string.IsNullOrWhiteSpace(CliCorreoElectronico))
            {
                throw new Exception(AppResource.MustSpecifyEmail);
            }

            if (ParCamposObligatorios.Contains("CLIREGMERCANTIL") && string.IsNullOrWhiteSpace(CliRegMercantil))
            {
                throw new Exception(AppResource.MustSpecifyCommertialRegistry);
            }
            if (ParCamposObligatorios.Contains("CLITIPOCOMPROBANTEFAC") && CurrentTipoComprobante == null)
            {
                throw new Exception(AppResource.MustSpecifyReceiptType);
            }

            if (ParCamposObligatorios.Contains("CLINOMBREEMISIONCHEQUES") && string.IsNullOrWhiteSpace(CliNombreEmisionCheques))
            {
                throw new Exception(AppResource.MustSpecifyCheckIssuer);
            }

            if (ParCamposObligatorios.Contains("LIPCODIGO") && CurrentListaPrecios == null)
            {
                throw new Exception(AppResource.MustSpecifyPriceListInCreditTab);
            }

            if (ParCamposObligatorios.Contains("CLIPROPIETARIO") && string.IsNullOrWhiteSpace(CliPropietario))
            {
                throw new Exception(AppResource.MustSpecifyOwnerNameInOwnerTab);
            }

            if (ParCamposObligatorios.Contains("CLIPROPIETARIODIRECCION") && string.IsNullOrWhiteSpace(CliPropietarioDireccion))
            {
                throw new Exception(AppResource.MustSpecifyOwnerAddressInOwnerTab);
            }

            if (ParCamposObligatorios.Contains("CLICEDULAPROPIETARIO") && string.IsNullOrWhiteSpace(CliCedulaPropietario))
            {
                throw new Exception(AppResource.MustSpecifyOwnerIdInOwnerTab);
            }

            if (ParCamposObligatorios.Contains("CLIPROPIETARIOTELEFONO") && string.IsNullOrWhiteSpace(CliPropietarioTelefono))
            {
                throw new Exception(AppResource.MustSpecifyOwnerPhoneInOwnerTab);
            }

            if (ParCamposObligatorios.Contains("CLIPROPIETARIOCELULAR") && string.IsNullOrWhiteSpace(CliPropietarioCelular))
            {
                throw new Exception(AppResource.MustSpecifyOwnerCellPhoneInOwnerTab);
            }

            if (ParCamposObligatorios.Contains("CLIFAMILIARNOMBRE") && string.IsNullOrWhiteSpace(CliFamiliarNombre))
            {
                throw new Exception(AppResource.MustSpecifyOwnerFamilyMemberName);
            }

            if (ParCamposObligatorios.Contains("CLIFAMILIARDIRECCION") && string.IsNullOrWhiteSpace(CliFamiliarDireccion))
            {
                throw new Exception(AppResource.MustSpecifyAddressRelativeInFamilyTab);
            }

            if (ParCamposObligatorios.Contains("CLIFAMILIARCEDULA") && string.IsNullOrWhiteSpace(CliFamiliarCedula))
            {
                throw new Exception(AppResource.MustSpecifyRelativeIdInFamilyTab);
            }

            if (ParCamposObligatorios.Contains("CLIFAMILIARTELEFONO") && string.IsNullOrWhiteSpace(CliFamiliarTelefono))
            {
                throw new Exception(AppResource.MustSpecifyRelativePhoneInFamilyTab);
            }

            if (ParCamposObligatorios.Contains("CLIFAMILIARCELULAR") && string.IsNullOrWhiteSpace(CliFamiliarCelular))
            {
                throw new Exception(AppResource.MustSpecifyRelativeCellPhoneInFamilyTab);
            }

            if (ParCamposObligatorios.Contains("CLILIMCRE") && CliLimiteCreditoSolicitado == 0)
            {
                throw new Exception(AppResource.MustSpecifyCreditLimitInCreditTab);
            }

            if (ParCamposObligatorios.Contains("CLIRNC") && string.IsNullOrEmpty(CliRNC))
            {
                throw new Exception(AppResource.MustSpecifyRncId);
            }      
        }

        private int SavedCliId = -1;
        private async void GuardarProspecto()
        {
            try
            {
                IsBusy = true;                        


                if (CurrentTipoCliente == null)
                {
                    throw new Exception(AppResource.MustSpecifyCustomerType);
                }

                if (string.IsNullOrEmpty(CliNombre))
                {
                    throw new Exception(AppResource.ProspectNameCannotBeEmpty);
                }

                if (myParametro.GetParProspectosCedulaObligatorio()) { 

                        if (string.IsNullOrWhiteSpace(CliRNC))
                        {
              
                            throw new Exception(AppResource.MustTypeProspectIdWarning);

                        }
                        
                }
              if (myParametro.GetValidarRncProspectos())
                {
                    if (Functions.ValidarDocumento(CliRNC) == false)
                    {
                        throw new Exception(AppResource.MustEnterRncIdValidWarning);
                    }
                    if (!string.IsNullOrWhiteSpace(CliCedulaPropietario))
                    {
                        if (Functions.ValidarDocumento(CliCedulaPropietario) == false)
                        {
                            throw new Exception(AppResource.OwnerIdIsNotValidWarning);
                        } 
                    }
                    if (!string.IsNullOrWhiteSpace(CliFamiliarCedula))
                    {
                        if (Functions.ValidarDocumento(CliFamiliarCedula) == false)
                        {
                            throw new Exception(AppResource.FamilyIdIsNotValid);
                        }
                    }
                }

                var fotosObligatorias = myParametro.GetParProspectosCantidadFotoObligatoria();

                if (fotosObligatorias > 0 && myTraImg.GetCantidadImagenesInTemp("Prospectos", DS_RepresentantesSecuencias.GetLastSecuencia("SolicitudesCredito").ToString()) < fotosObligatorias)
                {
                    throw new Exception(AppResource.MustTakeAtLeastPhotosWarning.Replace("@", fotosObligatorias.ToString()));
                }

                if(!string.IsNullOrWhiteSpace(CliRNC))
                {
                    var nombreExistente = myCli.GetCliNombreByRnc(CliRNC);

                    if (!string.IsNullOrWhiteSpace(nombreExistente))
                    {
                        throw new Exception(AppResource.CustomerAlreadyExistsWarning.Replace("@", nombreExistente));
                    }
                    
                }

                if (ParCamposObligatorios.Contains("CLIVISPOS") && OrdenAVisitarEsVisible)
                {
                    if(!await CalcularVisitasToBool())
                    {
                        IsBusy = false;
                        return;
                    }                          
                }

                if (ParCamposObligatorios.Contains("CLIVISORDRUT") && OrdenRutaEsVisible)
                {
                    ValidadRutPosicion();
                }

                ValidarNumerosTelefonicos();

                ValidarReferenciasObligatorias();

                ValidarDatos();
                

                IsGoingToSave?.Invoke();

                var temp = new Clientes();

                var tipo = -1;
                if (CurrentTipoCliente != null)
                {
                    int.TryParse(CurrentTipoCliente.CodigoUso, out int tipoRaw);
                    tipo = tipoRaw;
                }
                var tipoLocal = -1;
                if (CurrentTipoLocal != null)
                {
                    int.TryParse(CurrentTipoLocal.CodigoUso, out int tipoLocalRaw);
                    tipoLocal = tipoLocalRaw;
                }
                var estado = 3;
                if (CurrentEstadoCliente != null)
                {
                    int.TryParse(CurrentEstadoCliente.EstEstado, out estado);
                }
                //var canales = new DS_CanalDistribucion();
                //int canID = canales.GetCanIDbyCanDescripcion(CurrentCanalVenta.CanDescripcion);
                temp.CanID = CurrentCanalVenta != null ? CurrentCanalVenta.CanID : -1;
                temp.CliTipoCliente = tipo;
                temp.CliTipoLocal = tipoLocal;
                temp.ConID = CurrentCondicionPago != null ? CurrentCondicionPago.ConID : -1;
                temp.CliEstatus = estado;
                temp.CliNombre = CliNombre;
                temp.CliRNC = CliRNC;
                temp.CliTelefono = CliTelefono;
                temp.TerCodigo = CurrentTerritorio?.TerCodigo;
                temp.ProID = CurrentProvincia != null ? CurrentProvincia.ProID : -1;
                temp.MunID = CurrentMunicipio != null ? CurrentMunicipio.MunID : -1;
                temp.SecCodigo = CurrentMunSector?.SecCodigo;
                temp.CliCalle = CliCalle;
                temp.CliCasa = CliCasa;
                temp.cliSector = CurrentMunSector?.SecNombre ?? CliSector;
                temp.CliUrbanizacion = CliUrbanizacion;
                temp.CliContacto = CliContacto;
                temp.CliContactoCelular = CliContactoCelular;
                temp.CliTipoComprobanteFAC = CurrentTipoComprobante?.CodigoUso;
                temp.RutEntregaID = CurrentRuta != null ? CurrentRuta.RutID : -1;
                temp.CliRutPosicion = (currenClienteAVisitarDespuesDe != null && 
                    currenClienteAVisitarDespuesDe.CliRutPosicion > 0) ?
                    !string.IsNullOrEmpty(CliRutPosiciones)? 
                    int.Parse(CliRutPosiciones) : 
                    (IsDespuesEnabled? currenClienteAVisitarDespuesDe.CliRutPosicion + 1: 
                    currenClienteAVisitarDespuesDe.CliRutPosicion - 1) : 0;
                temp.CliFormasPago = "1000000";
                temp.CliCorreoElectronico = CliCorreoElectronico;

                if (HasGps)
                {
                    temp.CliLatitud = Arguments.Values.CurrentLocation != null ? Arguments.Values.CurrentLocation.Latitude : 0;
                    temp.CliLongitud = Arguments.Values.CurrentLocation != null ? Arguments.Values.CurrentLocation.Longitude : 0;
                }

                temp.CliNombreEmisionCheques = CliNombreEmisionCheques;
                temp.CliEstimadoCompras = CliEstimadoCompras;
                temp.CliInventario = CliInventario;
                temp.CliLimiteCreditoSolicitado = CliLimiteCreditoSolicitado;
                temp.LiPCodigo = CurrentListaPrecios?.CodigoUso;
                temp.CliPropietario = CliPropietario;
                temp.CliPropietarioDireccion = CliPropietarioDireccion;
                temp.CliCedulaPropietario = CliCedulaPropietario;
                temp.CliPropietarioTelefono = CliPropietarioTelefono;
                temp.CliPropietarioCelular = CliPropietarioCelular;
                temp.CliFamiliarNombre = CliFamiliarNombre;
                temp.CliFamiliarDireccion = CliFamiliarDireccion;
                temp.CliFamiliarCedula = CliFamiliarCedula;
                temp.CliFamiliarTelefono = CliFamiliarTelefono;
                temp.CliFamiliarCelular = CliFamiliarCelular;
                temp.TiNID = TipNombre != null? TipNombre.TinID : 0;
                temp.CliRegMercantil = CliRegMercantil;
                temp.CliFax = CliFax ?? "";
                temp.ClaID = CurrentClasificacion != null ? CurrentClasificacion.ClaID : -1;

                string resultofsem = string.IsNullOrEmpty(ProspectosTabPage.resultofsemanas) &&
                    !ParCamposObligatorios.Contains("CLIVISPOS")? "00000000000000000000000000000000000"
                    : ProspectosTabPage.resultofsemanas;

                temp.CliFrecuenciaVisita = CurrentFrecuencias != null ? CurrentFrecuencias.CodigoUso : "";
                temp.CliRutSemana1 = resultofsem.Substring(0,7);
                temp.CliRutSemana2 = resultofsem.Substring(7, 7);
                temp.CliRutSemana3 = resultofsem.Substring(14, 7);
                temp.CliRutSemana4 = ProspectosTabPage.copytofirts? resultofsem.Substring(0, 7) : resultofsem.Substring(21, 7);
                temp.CliOrdenRuta = !string.IsNullOrEmpty(CliOrdenRutas) ? int.Parse(CliOrdenRutas) : -1;

                var task = new TaskLoader() { SqlTransactionWhenRun = true };

                await task.Execute(() => { SavedCliId = myCli.GuardarProspecto(temp, ClientesReferencias, myTraImg); });

                IsSaved?.Invoke();

                IsBusy = false;

                IsProspectSaved = true;

                /* var result = await DisplayAlert("Exito", "Prospecto guardado correctamente", "Salir", "Imprimir");

                 if (result)
                 {
                     await PopAsync(false);
                 }
                 else
                 {
                     ShowDialogImpresion();
                 }*/

                Arguments.Values.CurrentModule = Enums.Modules.PROSPECTOS;
                ProspectosTabPage.Finish = true;

                await PushAsync(new SuccessPage(AppResource.ProspectSavedUpper, SavedCliId, Ispreliminar: false));

            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.InnerException != null ? e.InnerException.Message : e.Message);
            }

            IsBusy = false;
        }

        private void ValidarReferenciasObligatorias()
        {
            var parValidarSoloSiEsCredito = myParametro.GetParProspectosValidarCantidadReferenciasSiEsCredito();

            if (parValidarSoloSiEsCredito)
            {
                if (CurrentCondicionPago == null)
                {
                    throw new Exception(AppResource.MustSpecifyPaymentCondition);
                }
                //si tiene el parametro y no es a credito no validar las referencias.
                if(CurrentCondicionPago.ConDiasVencimiento <= 0)
                {
                    return;
                }
            }

            var parReferenciasObligatorias = myParametro.GetParProspectosCantidadReferenciasObligatoria();

            if(parReferenciasObligatorias == null)
            {
                return;
            }

            foreach(var reference in parReferenciasObligatorias)
            {
                if (string.IsNullOrWhiteSpace(reference.CodigoReferencia) || reference.CantidadObligatoria <= 0)
                {
                    continue;
                }

                var cantidadReferencias = ClientesReferencias.Where(x => x.CliRefTipo.Trim().ToUpper() == reference.CodigoReferencia.Trim().ToUpper()).Count();

                if(cantidadReferencias < reference.CantidadObligatoria)
                {
                    var data = TiposReferencia.Where(x => x.CodigoUso.Trim().ToUpper() == reference.CodigoReferencia.Trim().ToUpper()).FirstOrDefault();
                    throw new Exception(AppResource.MustAddMinimumReferences.Replace("@", reference.CantidadObligatoria.ToString()) + (data != null ? AppResource.OfType + data.Descripcion : ""));
                }
            }
        }

        private async void ShowDialogImpresion()
        {
            var result = await DisplayActionSheet(AppResource.ChoosePrinterCopies, buttons: new string[] { "1", "2", "3", "4", "5" });

            if (int.TryParse(result, out int copias))
            {
                AceptarImpresion(copias);
                return;
            }
            await PopAsync(false);
        }

        private async void AceptarImpresion(int Copias)
        {
            try
            {
                if (IsBusy)
                {
                    return;
                }

                PrinterManager manager = null;

                for (int x = 0; x < Copias; x++)
                {
                    IsBusy = true;

                    await Imprimir(manager);

                    IsBusy = false;

                    if (Copias > 1 && x != Copias - 1)
                    {
                        await DisplayAlert(AppResource.PrintCopy, AppResource.CutPapelMessage, AppResource.Print);
                    }
                }
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.ErrorPrinting, e.Message, AppResource.Aceptar);
            }

            await PopAsync(true);

            IsBusy = false;
        }

        private Task Imprimir(PrinterManager manager)
        {
            return Task.Run(() =>
            {
                if (manager == null)
                {
                    manager = new PrinterManager();
                }
                printer.Print(SavedCliId, false, manager);
            });
        }

        public void SetValueSpikersDefault()
        {
            if (EstadosClientes != null && EstadosClientes.Count == 1)
            {
                currentestadocliente = EstadosClientes.FirstOrDefault();
            }
            if (TiposClientes != null && TiposClientes.Count == 1)
            {
                CurrentTipoCliente = TiposClientes.FirstOrDefault();
            }
            if (TiposLocales != null &&  TiposLocales.Count == 1)
            {
                CurrentTipoLocal = TiposLocales.FirstOrDefault();
            }
            if (CondicionesPagos != null &&  CondicionesPagos.Count == 1)
            {
                CurrentCondicionPago = CondicionesPagos.FirstOrDefault();
            }
            if (Territorios != null && Territorios.Count == 1) 
            {
                CurrentTerritorio = Territorios.FirstOrDefault();
            }
            if (Rutas != null && Rutas.Count == 1)
            {
                CurrentRuta = Rutas.FirstOrDefault();
            }
            if (Provincias != null && Provincias.Count == 1)
            {
                CurrentProvincia = Provincias.FirstOrDefault();
            }
            if (Municipios != null && Municipios.Count == 1)
            {
                CurrentMunicipio = Municipios.FirstOrDefault();
            }
            if (MunSectores != null && MunSectores.Count == 1)
            {
                CurrentMunSector = MunSectores.FirstOrDefault();
            }
            if (TiposComprobante != null && TiposComprobante.Count == 1)
            {
                CurrentTipoComprobante = TiposComprobante.FirstOrDefault();
            }

            if (!string.IsNullOrEmpty(myParametro.GetParProspectoIdTipoComprobanteDefault()))
            {
                CurrentTipoComprobante = TiposComprobante.FirstOrDefault(t => t.CodigoUso.Equals(myParametro.GetParProspectoIdTipoComprobanteDefault()));
            }
        }

        public async void CalcularVisitasToBoolUnasync(bool fromToogled)
        {
            await CalcularVisitasToBool(fromToogled);
        }
        public async Task<bool> CalcularVisitasToBool(bool ifromtoggled = false)
        {
              IsDespuesEnabled = IsToggledVisitar;
              IsAntesEnabled = !IsToggledVisitar;

              var clientesforvalid = new DS_Clientes().GetAllClientesRutPosicionForValid(true);

              Clientes cliente = null;
              if (CurrenClienteAVisitarDespuesDe != null || !string.IsNullOrEmpty(CliRutPosiciones))
              {
                  cliente = new DS_Clientes().GetAllClientesRutPosicion
                      (
                      !string.IsNullOrEmpty(CliRutPosiciones) ? IsToggledVisitar ?
                               int.Parse(CliRutPosiciones) : int.Parse(CliRutPosiciones) :
                      IsToggledVisitar ? CurrenClienteAVisitarDespuesDe.CliRutPosicion + 1 :
                      CurrenClienteAVisitarDespuesDe.CliRutPosicion - 1);


                if (ifromtoggled)
                {
                    CliRutPosiciones = CurrenClienteAVisitarDespuesDe != null ? IsDespuesEnabled ? (currenClienteAVisitarDespuesDe.CliRutPosicion + 1).ToString() :
                            (currenClienteAVisitarDespuesDe.CliRutPosicion - 1).ToString() : CliRutPosiciones;
                }
              }

              if (cliente != null)
              {
                bool result = await DisplayAlert(AppResource.Warning, AppResource.PositionAlreadyUsedByAnotherClient + " " + cliente.CliRutPosicion + " - " +cliente.CliNombre, AppResource.Aceptar, AppResource.Cancel);
                IsPositionOut?.Invoke(result);
                return false;                  
              }
              if (!string.IsNullOrEmpty(CliRutPosiciones) && clientesforvalid != null && (int.Parse(CliRutPosiciones) > (clientesforvalid.CliRutPosicion + 1) || int.Parse(CliRutPosiciones) <= 0))
              {
                await DisplayAlert(AppResource.Warning, AppResource.VisitSequenceIsOutsideAllowedRange.Replace("@", (clientesforvalid.CliRutPosicion + 1).ToString()), AppResource.Aceptar);
                return false;
              }

            return true;
        }

        //public async void CalcularOrdenToBool()
        //{
        //    IsDespuesOrdenarEnabled = IsToggledOrdenar;
        //    IsAntesOrdenarEnabled = !IsToggledOrdenar;

        //     var clientesforvalid = new DS_Clientes().GetAllClientesRutPosicionForValid
        //        (cliordenruta: !string.IsNullOrEmpty(CliOrdenRutas) ? IsToggledOrdenar ?
        //                     int.Parse(CliOrdenRutas) + 1 : int.Parse(CliOrdenRutas) - 1 : -1);

        //    Clientes cliente = null;
        //    if (CurrenClienteAOrdenarDespuesDe != null)
        //    {
        //        cliente = new DS_Clientes().GetAllClientesRutPosicion
        //            (
        //            cliordenruta:!string.IsNullOrEmpty(CliOrdenRutas) ? IsToggledOrdenar ?
        //                     int.Parse(CliOrdenRutas) + 1 : int.Parse(CliOrdenRutas) - 1 : -1
        //            );
        //    }

        //    if (cliente != null)
        //    {
        //        bool result = await DisplayAlert(AppResource.Warning, $"debe utilizar otro orden, el que intenta utilizar, ya esta ocupado por este cliente: {cliente.CliRutPosicion} - {cliente.CliNombre} ", "aceptar","cancelar");
        //        IsPositionOut?.Invoke(result);                
        //    }
        //    else if(clientesforvalid != null && int.Parse(CliOrdenRutas) >  clientesforvalid.CliOrdenRuta +1)
        //    {
        //        throw new Exception("El orden que trata de introducir es incorrecto");
        //    }

        //}
        public void ValidadRutPosicion()
        {

            var clientesforvalid = new DS_Clientes().GetAllClientesRutPosicionForValid(cliordenruta:true);

            bool parserut = int.TryParse(CliOrdenRutas, out int rutpos);

            if (!parserut || clientesforvalid != null && (rutpos > (clientesforvalid.CliOrdenRuta + 1) || rutpos <= 0))
            {
                throw new Exception(AppResource.RouteOrderOutsideAllowedRange.Replace("@", (clientesforvalid.CliOrdenRuta + 1).ToString()));                
            }
        }

        public void ValidarNumerosTelefonicos()
        {            
            if (!string.IsNullOrEmpty(CliTelefono) && CliTelefono.Length < "###-###-####".Length)
            {
                throw new Exception(AppResource.InvalidPhoneNumber);
            }

            if (!string.IsNullOrEmpty(CliFax) && CliFax.Length < "###-###-####".Length)
            {
                throw new Exception(AppResource.InvalidWhatsappNumber);
            }

            if (!string.IsNullOrEmpty(CliContactoCelular) && CliContactoCelular.Length < "###-###-####".Length)
            {
                throw new Exception(AppResource.InvalidContactPhoneNumber);
            }
            
            if (!string.IsNullOrEmpty(CliRefTelefono) && CliRefTelefono.Length < "###-###-####".Length)
            {
                throw new Exception(AppResource.InvalidReferencePhoneNumber);
            }            
            
            if (!string.IsNullOrEmpty(CliPropietarioTelefono) && CliPropietarioTelefono.Length < "###-###-####".Length)
            {
                throw new Exception(AppResource.InvalidOwnerPhoneNumber);
            }                        
            
            if (!string.IsNullOrEmpty(CliPropietarioCelular) && CliPropietarioCelular.Length < "###-###-####".Length)
            {
                throw new Exception(AppResource.InvalidOwnerCellPhoneNumber);
            }
            
            if (!string.IsNullOrEmpty(CliFamiliarTelefono) && CliFamiliarTelefono.Length < "###-###-####".Length)
            {
                throw new Exception(AppResource.InvalidFamilyMemberPhone);
            }                        
            
            if (!string.IsNullOrEmpty(CliFamiliarCelular) && CliFamiliarCelular.Length < "###-###-####".Length)
            {
                throw new Exception(AppResource.InvalidFamilyMemberCellPhoneNumber);
            }
        }
    }
}