using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.model.Internal.Structs.Args;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using MovilBusiness.views;
using MovilBusiness.Views;
using MovilBusiness.Views.Components.Modals;
using Plugin.Geolocator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class SacViewModel : BaseViewModel
    {

        private UsosMultiples currenttipocliente;
        public UsosMultiples CurrentTipoCliente { get => currenttipocliente; set { currenttipocliente = value; RaiseOnPropertyChanged(); } }

        //Cliente a visitar depues de
        private Clientes currenClienteAVisitarDespuesDe;
        public Clientes CurrenClienteAVisitarDespuesDe { get => currenClienteAVisitarDespuesDe; set { currenClienteAVisitarDespuesDe = value; RaiseOnPropertyChanged(); } }
        public bool OrdenAVisitarEsVisible { get => DS_RepresentantesParametros.GetInstance().GetParProspectoCliRutPosicion(); }
        public bool OrdenRutaEsVisible { get => DS_RepresentantesParametros.GetInstance().GetParProspectoCliRutOrden(); }

        private Clientes currentclient;
        public Clientes CurrentClient { get => currentclient; set { currentclient = value; RaiseOnPropertyChanged(); } }

        private ObservableCollection<UsosMultiples> tiposclientes;
        public ObservableCollection<UsosMultiples> TiposClientes { get => tiposclientes; set { tiposclientes = value; RaiseOnPropertyChanged(); } }
        private ObservableCollection<UsosMultiples> frecuencias { get; set; }
        public ObservableCollection<UsosMultiples> Frecuencias { get => frecuencias; set { frecuencias = value; RaiseOnPropertyChanged(); } }
        private UsosMultiples currentfrecuencias { get; set; }
        public UsosMultiples CurrentFrecuencias { get => currentfrecuencias; set { currentfrecuencias = value; RaiseOnPropertyChanged(); } }

        private ObservableCollection<UsosMultiples> tiposLocales;
        public ObservableCollection<UsosMultiples> TiposLocales { get => tiposLocales; set { tiposLocales = value; RaiseOnPropertyChanged(); } } 

        private UsosMultiples currenttipolocal;
        public UsosMultiples CurrentTipoLocal { get => currenttipolocal; set { currenttipolocal = value; RaiseOnPropertyChanged(); } }

        private UsosMultiples currenttipocomprobante;
        public UsosMultiples CurrentTipoComprobante { get => currenttipocomprobante; set { currenttipocomprobante = value; RaiseOnPropertyChanged(); } }
        private bool isantesenabled { get; set; }
        public bool IsAntesEnabled { get => isantesenabled; set { isantesenabled = value; RaiseOnPropertyChanged(); } }
        private bool isdespuesenabled { get; set; }
        public bool IsDespuesEnabled { get => isdespuesenabled; set { isdespuesenabled = value; RaiseOnPropertyChanged(); } }
        private bool isvsibleentryeector { get; set; }
        public bool IsVisibleEntrySector { get => isvsibleentryeector; set { isvsibleentryeector = value; RaiseOnPropertyChanged(); } }
        private bool isvisiblepickersector { get; set; }
        public bool IsVisiblePickerSector { get => isvisiblepickersector; set { isvisiblepickersector = value; RaiseOnPropertyChanged(); } }
        private bool istoggledvisitar { get; set; }
        public bool IsToggledVisitar { get => istoggledvisitar; set { istoggledvisitar = value; CalcularVisitasToBoolUnAsync(true); RaiseOnPropertyChanged(); } }
        public Action<bool> IsPositionOut { get; set; }
        public Action<bool> IsOrdenOut { get; set; }

        private ObservableCollection<UsosMultiples> tiposcomprobante;
        public ObservableCollection<UsosMultiples> TiposComprobante { get => tiposcomprobante; set { tiposcomprobante = value; RaiseOnPropertyChanged(); } }

        private ObservableCollection<SolicitudActualizacionClientesHorarios> horariosagregados;
        public ObservableCollection<SolicitudActualizacionClientesHorarios> HorariosAgregados { get => horariosagregados; set { horariosagregados = value; RaiseOnPropertyChanged(); } }

        private DS_Municipios myMun;
        private DS_SolicitudActualizacionClientes mySol;
        private DS_CanalDistribucion canal;
        private DS_Clientes myCli;
        private double clilatitud;
        private double clilongitud;

        private ObservableCollection<TiposNegocio> tipNegocio;
        public ObservableCollection<TiposNegocio> TipNegocio { get => tipNegocio; set { tipNegocio = value; RaiseOnPropertyChanged(); } }

        private TiposNegocio currenttipNegocio;
        public TiposNegocio CurrentTipNegocio { get => currenttipNegocio; set { currenttipNegocio = value; RaiseOnPropertyChanged(); } }

        public double CliLatitud { get => clilatitud; set { clilatitud = value; RaiseOnPropertyChanged(); } }
        public double CliLongitud { get => clilongitud; set { clilongitud = value; RaiseOnPropertyChanged(); } }

        public List<Clientes> ClientesNombres { get; private set; }

        private ObservableCollection<CanalDistribucion> tiposdecanales;
        public ObservableCollection<CanalDistribucion> TiposDeCanales { get => tiposdecanales; set { tiposdecanales = value; RaiseOnPropertyChanged(); } }
        public List<string> TiposCanales { get; private set; }

        private CanalDistribucion currentcanalventa;
        public CanalDistribucion CurrentCanalVenta { get => currentcanalventa; set { currentcanalventa = value; RaiseOnPropertyChanged(); } }
        public List<Provincias> Provincias { get; set; }

        private ObservableCollection<Municipios> municipios;
        public ObservableCollection<Municipios> Municipios { get => municipios; set { municipios = value; RaiseOnPropertyChanged(); } }

        private Municipios currentmunicipio;
        private Provincias currentprovincia;
        private SectoresMunicipios currentmunsector;

        private int CurrentSACSecuencia = -1;

        public Municipios CurrentMunicipio { get => currentmunicipio; 
            set { currentmunicipio = value; if (value != null) 
                { Sectores = new ObservableCollection<SectoresMunicipios>
                        (myMun.GetSectoresMunicipios(value.MunID));
                    IsVisibleEntrySector = Sectores == null || Sectores.Count <= 0;
                    IsVisiblePickerSector = !IsVisibleEntrySector;
                } else { Sectores = null; } RaiseOnPropertyChanged(); } }
        public Provincias CurrentProvincia { get => currentprovincia;set { currentprovincia = value; if (value != null) { Municipios = new ObservableCollection<Municipios>(myMun.GetMunicipiosByProvincia(value.ProID)); } else { Municipios = null; } RaiseOnPropertyChanged(); } }
        public SectoresMunicipios CurrentMunSector { get => currentmunsector; set { currentmunsector = value; RaiseOnPropertyChanged(); } }

        private ObservableCollection<SectoresMunicipios> sectores;
        public ObservableCollection<SectoresMunicipios> Sectores { get => sectores; set { sectores = value; RaiseOnPropertyChanged(); } }

        private string clisector;
        public string CliSector { get => clisector; set { clisector = value; RaiseOnPropertyChanged(); } }

        private string cliurbanizacion;
        public string CliUrbanizacion { get => cliurbanizacion; set { cliurbanizacion = value; RaiseOnPropertyChanged(); } }

        private Clasifica currentclasificacion;
        public Clasifica CurrentClasificacion { get => currentclasificacion; set { currentclasificacion = value; RaiseOnPropertyChanged(); } }

        private List<Clasifica> clasificacion;
        public List<Clasifica> Clasificacion { get => clasificacion; set { clasificacion = value; RaiseOnPropertyChanged(); } }

        public ICommand GeoRefreshCommand { get; private set; }
        public ICommand OpenMapCommand { get; private set; }

        public ICommand AddHorarioCommand { get; private set; }

        private DateTime contactofechanac = DateTime.Now;
        public DateTime ContactoFechaNac { get => contactofechanac; set { contactofechanac = value; RaiseOnPropertyChanged(); } }

        private string CamposDisponibles;
        public int tinid { get; set; }
        public bool NombreEnabled { get => CamposDisponibles.Contains("clinomb"); }
        public bool PropietarioEnabled { get => CamposDisponibles.Contains("pronomb"); }
        public bool ContactoEnabled { get => CamposDisponibles.Contains("cont"); }
        public bool CedulaEnabled { get => CamposDisponibles.Contains("ced"); }
        public bool EmailEnabled { get => CamposDisponibles.Contains("corel"); }
        public bool EncPagoEnabled { get => CamposDisponibles.Contains("encpa"); }
        public bool DirEnabled { get => CamposDisponibles.Contains("dir"); }
        public bool FaxEnabled { get => CamposDisponibles.Contains("fax"); }
        public bool WebEnabled { get => CamposDisponibles.Contains("web"); }
        public bool TelefonoEnabled { get => CamposDisponibles.Contains("tel"); }
        public bool DireccionEnabled { get => CamposDisponibles.Contains("dir"); }
        public bool SectorEnabled { get => CamposDisponibles.Contains("sec"); }
        public bool ProvinciaEnabled { get => CamposDisponibles.Contains("pro"); }
        public bool MunicipioEnabled { get => CamposDisponibles.Contains("mun"); }
        public bool DepFacturaEnabled { get => CamposDisponibles.Contains("dfac"); }
        public bool DepositosEnabled { get => CamposDisponibles.Contains("dep"); }
        public bool OrdenCompraEnabled { get => CamposDisponibles.Contains("dcomp"); }
        public bool FechaNacContEnabled { get => CamposDisponibles.Contains("fenacon"); }
        public bool Clasifica { get => CamposDisponibles.Contains("claf"); }
        public bool CliRegMercantilEnabled { get => CamposDisponibles.Contains("regmer"); }
        public bool TipoNegocioEnabled { get => CamposDisponibles.Contains("tipneg"); }
        public bool CanalDeVentaEnabled { get => CamposDisponibles.Contains("canvent"); }
        public bool TipoLocalEnabled { get => CamposDisponibles.Contains("tiploc"); }
        public bool TipoClienteEnabled { get => CamposDisponibles.Contains("tipcli"); }
        public bool TipoComprobanteEnabled { get => CamposDisponibles.Contains("tipcomp"); }
        public bool BarrioEnabled { get => CamposDisponibles.Contains("barrio"); }
        public bool CliRutPosicionEnabled { get => CamposDisponibles.Contains("posicion"); }

        private bool clitipnegocio;
        public bool CliTipNegocio { get => clitipnegocio; set { clitipnegocio = value; RaiseOnPropertyChanged(); } }
        public Action IsGoingToSave { get; set; }

        private string clirutposiciones;
        public string CliRutPosiciones { get => clirutposiciones; set { clirutposiciones = value; RaiseOnPropertyChanged(); } }

        private string cliordenrutas;
        public string CliOrdenRutas { get => cliordenrutas; set { cliordenrutas = value; RaiseOnPropertyChanged(); } }
        //Cliente a Ordenar depues de
        private Clientes currenClienteAOrdenarDespuesDe;
        public Clientes CurrenClienteAOrdenarDespuesDe { get => currenClienteAOrdenarDespuesDe; set { currenClienteAOrdenarDespuesDe = value; RaiseOnPropertyChanged(); } }

        private TimeSpan horaapertura;
        public TimeSpan HoraApertura { get => horaapertura; set { horaapertura = value; RaiseOnPropertyChanged(); } }

        private TimeSpan horacierre;
        public TimeSpan HoraCierre { get => horacierre; set { horacierre = value; RaiseOnPropertyChanged(); } }

        public List<UsosMultiples> DiasVisita { get; set; }

        private UsosMultiples currentdiavisita;
        public UsosMultiples CurrentDiaVisita { get => currentdiavisita; set { currentdiavisita = value; RaiseOnPropertyChanged(); } }

        public string CurrentHorario { get; set; }
        public bool ShowCurrentHorario => !string.IsNullOrEmpty(CurrentHorario);

        public string UpdateDistance
        {
            get
            {
                return myParametro.GetParGpsDistanciaMinimaActualizacion().ToString("N2") + " " + AppResource.Meters;
            }
        }

        public SacViewModel(Page page, Clientes CurrentClient) : base(page)
        {
            IsAntesEnabled = true;            
            var uso = new DS_UsosMultiples();
            myMun = new DS_Municipios();
            mySol = new DS_SolicitudActualizacionClientes();
            canal = new DS_CanalDistribucion();
            myCli = new DS_Clientes();

            HorariosAgregados = new ObservableCollection<SolicitudActualizacionClientesHorarios>();

            AddHorarioCommand = new Command(AddHorarioCliente);

            DiasVisita = uso.GetUsoByCodigoGrupo("DIASSEMANA", "CodigoUso");

           

            var CurrenClienteForSac = myCli.GetClientesForSac(CurrentClient.CliID, Arguments.CurrentUser.RepCodigo);

            if(CurrenClienteForSac != null)
            {
                this.CurrentClient = CurrenClienteForSac;
                CurrentClient = CurrenClienteForSac;
                CliUrbanizacion = CurrenClienteForSac.CliUrbanizacion;
                CliSector = CurrenClienteForSac.cliSector;
            }
            else
                this.CurrentClient = CurrentClient.Copy();

            LoadClienteHorario();

            SaveCommand = new Command(() =>
            {
                SaveSac();

            }, () => IsUp);

            GeoRefreshCommand = new Command(RefreshGeoReference);
            OpenMapCommand = new Command(OpenMap);


            if (OrdenAVisitarEsVisible)
            {
                ClientesNombres = myCli.GetAllClientesRutPosicion();
                if (ClientesNombres != null && ClientesNombres.Count > 0)
                {
                    CurrenClienteAVisitarDespuesDe = ClientesNombres.FirstOrDefault(v => v.CliID == Arguments.Values.CurrentClient.CliID);
                    CurrenClienteAOrdenarDespuesDe = CurrenClienteAVisitarDespuesDe;

                    if(CurrenClienteAOrdenarDespuesDe != null)
                    {
                        CurrenClienteAOrdenarDespuesDe.CliNombre = CurrenClienteAOrdenarDespuesDe.CliOrdenRuta
                            + "/" + CurrenClienteAOrdenarDespuesDe.CliNombre;
                        CliRutPosiciones = currenClienteAVisitarDespuesDe.CliRutPosicion.ToString();
                        CliOrdenRutas = CurrenClienteAOrdenarDespuesDe.CliOrdenRuta.ToString();
                    }
                }
            }

            if (OrdenRutaEsVisible)
            {
                ClientesNombres = myCli.GetAllClientesRutPosicion();
                if (ClientesNombres != null && ClientesNombres.Count > 0)
                {
                    CurrenClienteAOrdenarDespuesDe = ClientesNombres.FirstOrDefault(v => v.CliID == Arguments.Values.CurrentClient.CliID);
                    if (CurrenClienteAOrdenarDespuesDe != null)
                    {
                        CurrenClienteAOrdenarDespuesDe.CliNombre = CurrenClienteAOrdenarDespuesDe.CliOrdenRuta
                            + "/" + CurrenClienteAOrdenarDespuesDe.CliNombre;                        
                        CliOrdenRutas = CurrenClienteAOrdenarDespuesDe.CliOrdenRuta.ToString();
                    }
                }
            }

            CamposDisponibles = myParametro.GetParSACGeneralCamposDisponibles();

            if (CamposDisponibles == null)
            {
                CamposDisponibles = "";
            }
            else
            {
                CamposDisponibles = CamposDisponibles.ToLower();
            }

            Frecuencias = new ObservableCollection<UsosMultiples>(uso.GetUsoByCodigoGrupo("CliFrecuenciaVisita"));

            TiposDeCanales = new ObservableCollection<CanalDistribucion>(canal.GetAllCanales());
            CurrentCanalVenta = TiposDeCanales.Where(x => x.CanID == CurrentClient.CanID).FirstOrDefault();

            TiposClientes = new ObservableCollection<UsosMultiples>(uso.GetUsoByCodigoGrupo("SOLTIPOCLIENTE", "Descripcion"));
            CurrentTipoCliente = TiposClientes.Where(x => x.CodigoUso == CurrentClient.CliTipoCliente.ToString()).FirstOrDefault();

            TiposLocales = new ObservableCollection<UsosMultiples>(uso.GetUsoByCodigoGrupo("SOLTIPOLOCAL", "Descripcion"));
            CurrentTipoLocal = TiposLocales.Where(x => x.CodigoUso == CurrentClient.CliTipoLocal.ToString()).FirstOrDefault();

            TiposComprobante = new ObservableCollection<UsosMultiples>(uso.GetUsoByCodigoGrupo("NCFTIPO2018", "Descripcion"));
            CurrentTipoComprobante = TiposComprobante.Where(x => x.CodigoUso == CurrentClient.CliTipoComprobanteFAC).FirstOrDefault();
            
            if(Arguments.Values.CurrentClient.TiNID > 0)
            {
                TipNegocio = new ObservableCollection<TiposNegocio>()
                { new DS_TiposNegocio().GetTipoById(Arguments.Values.CurrentClient.TiNID) };

                CurrentTipNegocio = TipNegocio?.FirstOrDefault();
            }else
            {
                TipNegocio = new ObservableCollection<TiposNegocio>(new DS_TiposNegocio().GetTipo());
                if (TipNegocio.Count > 0)
                {
                    CurrentTipNegocio = TipNegocio[0];
                }
            }            
            

            var myProv = new DS_Provincias();
            Provincias = myProv.GetProvincias();
            
            if (CurrentClient == null)
            {
                return;
            }

            CurrentProvincia = Provincias.Where(x => x.ProID == CurrentClient.ProID).FirstOrDefault();

            CurrentClient.CliTelefono = PhoneNumber(CurrentClient.CliTelefono);
            CurrentClient.CliFax = PhoneNumber(CurrentClient.CliFax);            

            if(DateTime.TryParse(CurrentClient.CliContactoFechaNacimiento, out DateTime date))
            {
                ContactoFechaNac = date;
            }
            else
            {
                ContactoFechaNac = DateTime.Now;
            }

            CliLatitud = CurrentClient.CliLatitud;
            CliLongitud = CurrentClient.CliLongitud;

            var myCla = new DS_Clientes();
            Clasificacion = myCla.GetClasificacionClientes();

        }


        private void LoadClienteHorario()
        {
            try
            {
                var horarios = myCli.GetClienteHorarios(CurrentClient.CliID);
                CurrentHorario = "";

                for(var i = 0; i < horarios.Count; i++)
                {
                    var horario = horarios[i];
                    //String.Format("{0,-27}", s);
                    CurrentHorario += string.Format("{0,-20}", horario.ClhDia) + (horario.GetHorarioFormat(horario.clhHorarioApertura) + " - " + horario.GetHorarioFormat(horario.clhHorarioCierre)) + (i == horarios.Count-1 ? "" : "\n");
                }

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }
        }

        private void RefreshGeoReference()
        {
            if (!CrossGeolocator.Current.IsGeolocationEnabled)
            {
                DisplayAlert("Aviso", "El GPS esta apagado. Para continuar con la visita tiene que encender el GPS para eso tiene que ir a configuraciones", "Aceptar");
                return;
            }

            Functions.StartListeningForLocations();

           if(Arguments.Values.CurrentLocation != null)
            {
                CliLatitud = Arguments.Values.CurrentLocation.Latitude;
                CliLongitud = Arguments.Values.CurrentLocation.Longitude;
            }
            else
            {
                CliLatitud = 0;
                CliLongitud = 0;
            }

        }

        private void OpenMap()
        {
            if(CliLatitud != 0 && CliLongitud != 0)
            {
                PushAsync(new MapsPage(CliLatitud, CliLongitud, "Ubicación actual"));
            }
        }

        private void AddHorarioCliente()
        {
            if(CurrentDiaVisita == null)
            {
                DisplayAlert(AppResource.Warning, AppResource.MustSelectVisitDay);
                return;
            }

            if(HoraApertura == null)
            {
                DisplayAlert(AppResource.Warning, AppResource.MustSpecifyOpeningHour);
                return;
            }

            if(HoraCierre == null)
            {
                DisplayAlert(AppResource.Warning, AppResource.MustSpecifyClosingHour);
                return;
            }

            var data = new SolicitudActualizacionClientesHorarios();
            data.ClhDia = CurrentDiaVisita.CodigoUso;
            data.ClhDiaDescripcion = CurrentDiaVisita.Descripcion;
            data.ClhHorarioApertura = DateTime.Today.Add(HoraApertura).ToString("HH:mm");//HoraApertura.ToString("HH':'mm");
            data.ClhHorarioCierre = DateTime.Today.Add(HoraCierre).ToString("HH:mm");
            data.ClhHorarioAperturaAMPM = DateTime.Today.Add(HoraApertura).ToString("hh:mm tt");
            data.ClhHorarioCierreAMPM = DateTime.Today.Add(HoraCierre).ToString("hh:mm tt");

            HorariosAgregados.Add(data);
        }

        private async void SaveSac()
        {
            IsUp = false;

            if (CurrentClient == null || IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;
                CurrentClient.CliTelefono = PhoneNumber(CurrentClient.CliTelefono);
                CurrentClient.CliFax = PhoneNumber(CurrentClient?.CliFax);
                ValidarNumerosTelefonicos();

                if (myParametro.GetParSacCampoObligatorios().Contains("CLIVISPOS") && OrdenRutaEsVisible)
                {
                    if (string.IsNullOrEmpty(CliRutPosiciones))
                    {
                        throw new Exception("La secuencia del cliente a visitar es ogligatoria");
                    }

                    ValidadRutPosicion();
                }
                if (myParametro.GetParSacCampoObligatorios().Contains("CLIVISORDRUT") && OrdenAVisitarEsVisible)
                {
                    if (string.IsNullOrEmpty(CliOrdenRutas))
                    {
                        throw new Exception("La ruta del cliente a visitar es ogligatoria");
                    }

                    if (!await CalcularVisitasToBool())
                    {
                        IsBusy = false;
                        IsUp = true;
                        return;
                    }                    
                }

                if (myParametro.GetParSacCampoObligatorios().Contains("CLICONT") && string.IsNullOrEmpty(CurrentClient.CliContacto))                
                    throw new Exception("El contacto del cliente no puede estar vacio");
                if (myParametro.GetParSacCampoObligatorios().Contains("CLIPROP") && string.IsNullOrEmpty(CurrentClient.CliPropietario))                
                    throw new Exception("El propietario no puede estar vacio");

                if (!string.IsNullOrWhiteSpace(CurrentClient.CliCedulaPropietario))
                {                      
                        if(myParametro.GetValidarRncSAC())
                        { 
                            if (Functions.ValidarDocumento(CurrentClient.CliCedulaPropietario.Trim()) == false)
                            {
                                IsUp = true;
                                throw new Exception("Debes digitar un RNC o una cédula valida");
                            }
                        }
                }
                if (myParametro.GetParSacCampoObligatorios().Contains("CLILATLON") && (CliLatitud == 0 || CliLongitud == 0))
                {
                    throw new Exception("Debes de capturar la georeferencia ");
                }

                var tipoLocal = -1;
                var canid = -1;

                if(CurrentCanalVenta != null)
                {
                    canid = CurrentCanalVenta.CanID;
                }

                if (CurrentTipoLocal != null)
                {
                    int.TryParse(CurrentTipoLocal.CodigoUso, out int tipoLocalRaw);
                    tipoLocal = tipoLocalRaw;
                } 
                var tipo = -1;
                if (CurrentTipoCliente != null)
                {
                    int.TryParse(CurrentTipoCliente.CodigoUso, out int tipoRaw);
                    tipo = tipoRaw;
                }

                IsGoingToSave?.Invoke();
                string resultofsem = string.IsNullOrEmpty(SacPage.resultofsemanas) &&
                    !myParametro.GetParSacCampoObligatorios().Contains("CLIVISPOS") ? "00000000000000000000000000000000000"
                    : SacPage.resultofsemanas;

                var args = new SACArgs
                {
                    Cedula = CurrentClient.CliCedulaPropietario ?? "",
                    Contacto = CurrentClient.CliContacto ?? "",
                    Depositos = CurrentClient.CliIndicadorDeposito,
                    DepositaFactura = CurrentClient.CliIndicadorDepositaFactura,
                    Direccion = CurrentClient.CliCalle ?? "",
                    Email = CurrentClient.CliCorreoElectronico ?? "",
                    EncargadoPago = CurrentClient.CliEncargadoPago ?? "",
                    CliCasa = CurrentClient.CliCasa ?? "",
                    Fax = CurrentClient.CliFax ?? "",
                    FechaNacContacto = ContactoFechaNac,
                    Nombre = CurrentClient.CliNombre ?? "",
                    Propietario = CurrentClient.CliPropietario ?? "",
                    Telefono = CurrentClient.CliTelefono ?? "",
                    SitioWeb = CurrentClient.CliPaginaWeb ?? "",
                    Sector = CurrentMunSector != null ? CurrentMunSector.SecCodigo : "",
                    OrdenCompra = CurrentClient.CliIndicadorOrdenCompra,
                    MunicipioId = CurrentMunicipio != null ? CurrentMunicipio.MunID : -1,
                    ProvinciaId = CurrentProvincia != null ? CurrentProvincia.ProID : -1,
                    Latitud = CliLatitud,
                    Longitud = CliLongitud,
                    ClaID = CurrentClasificacion != null ? CurrentClasificacion.ClaID : -1,
                    CliRegMercantil = CurrentClient.CliRegMercantil ?? "",
                    TinID = CurrentTipNegocio != null ? CurrentTipNegocio.TinID : -1,
                    CanID = canid,
                    CliTipoLocal = tipoLocal,
                    CliTipoCliente = tipo,
                    CliTipoComprobanteFAC = CurrentTipoComprobante?.CodigoUso,
                    CliSector = CurrentMunSector != null ? CurrentMunSector.SecNombre : CliSector,
                    CliUrbanizacion = CliUrbanizacion,
                    CliRutPosicion = CurrenClienteAVisitarDespuesDe != null ?
                    CurrenClienteAVisitarDespuesDe.CliID == Arguments.Values.CurrentClient.CliID ?
                    !string.IsNullOrEmpty(CliRutPosiciones) ?
                    int.Parse(CliRutPosiciones) :
                    CurrenClienteAVisitarDespuesDe.CliRutPosicion : (currenClienteAVisitarDespuesDe != null &&
                    currenClienteAVisitarDespuesDe.CliRutPosicion > 0) ? !string.IsNullOrEmpty(CliRutPosiciones) ?
                    (IsDespuesEnabled ?
                    int.Parse(CliRutPosiciones) :
                    int.Parse(CliRutPosiciones)) : (IsDespuesEnabled ?
                    currenClienteAVisitarDespuesDe.CliRutPosicion + 1 : CurrenClienteAVisitarDespuesDe.CliRutPosicion - 1) : 0 : -1,
                    CliFrecuenciaVisita = CurrentFrecuencias != null ? CurrentFrecuencias.CodigoUso : "",
                    CliRutSemana1 = resultofsem.Substring(0, 7),
                    CliRutSemana2 = resultofsem.Substring(7, 7),
                    CliRutSemana3 = resultofsem.Substring(14, 7),
                    CliRutSemana4 = SacPage.copytofirts ? resultofsem.Substring(0, 7) : resultofsem.Substring(21, 7),
                    CliOrdenRuta = !string.IsNullOrEmpty(CliOrdenRutas) ? int.Parse(CliOrdenRutas) : -1,
                    Horarios = HorariosAgregados.ToList()
                };

                var task = new TaskLoader() { SqlTransactionWhenRun = true };

               // await DisplayAlert("Exito", "Solicitud guardada correctamente");


                if (ClientesViewModel.isFromSAC)
                {
                    var VisLatitud = Arguments.Values.CurrentLocation.Latitude;
                    var VisLongitud = Arguments.Values.CurrentLocation.Longitude;

                    var DiferenciaDistancia = myParametro.GetParDistanciaVisita();

                    var DistanciaKM = 6371 * Math.Acos(Math.Cos(DegreeToRadian((90 - VisLatitud))) * Math.Cos(DegreeToRadian((90 - CliLatitud)))
                   + Math.Sin(DegreeToRadian((90 - VisLatitud))) * Math.Sin(DegreeToRadian((90 - CliLatitud)))
                   * Math.Cos(DegreeToRadian((VisLongitud - CliLongitud))));

                    var DistanciaM = DistanciaKM * 1000;

                    int GenerarVisitaVirtual = -1;

                    if (myParametro.GetParCalcularSegunTipoVisita() == 2)
                    {
                        if (DiferenciaDistancia >= DistanciaM)
                        {
                            GenerarVisitaVirtual = -1;
                        }
                        else
                        {
                            GenerarVisitaVirtual = 1;
                        }

                    }

                    await task.Execute(() =>
                    {
                        Arguments.Values.CurrentVisSecuencia = new DS_Visitas().CrearVisita(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentLocation, GenerarVisitaVirtual);

                        if (myParametro.GetOfertasConSegmento())
                        {
                            new DS_Ofertas().GuardarProductosValidosParaOfertasPorSegmento(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentClient.TiNID);
                        }
                        else
                        {
                            new DS_Ofertas().GuardarProductosValidosParaOfertas(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentClient.TiNID);
                        }

                        mySol.GuardarSAC(args);
                    });

                    await DisplayAlert("Exito", "Solicitud guardada correctamente");

                    ClientesViewModel.isFromSAC = false;
                    App.Current.MainPage.Navigation.RemovePage(App.Current.MainPage.Navigation.NavigationStack[App.Current.MainPage.Navigation.NavigationStack.Count-1]);

                    if (myParametro.GetParSectores() > 2)
                    {
                        await PushAsync(new ElegirSectorPage());
                    }else
                    {
                        await PushAsync(new OperacionesPage());
                    }                    
                }
                else
                {
                    await task.Execute(() =>
                    {
                        CurrentSACSecuencia= mySol.GuardarSAC(args);
                    });

                    Arguments.Values.CurrentModule = Enums.Modules.SAC;
                    SacPage.Finish = true;
                    await PushAsync(new SuccessPage("SOLICITUD GUARDADA", CurrentSACSecuencia, Ispreliminar: false));
                    //await DisplayAlert("Exito", "Solicitud guardada correctamente");
                    //await PopAsync(true);
                }

                IsUp = false;
            }
            catch(Exception e)
            {
                await DisplayAlert("Error guardando SAC", e.Message);
            }

            IsBusy = false;
            IsUp = true;
        }

        private async void CalcularVisitasToBoolUnAsync(bool isFromToggled = false)
        {
            await CalcularVisitasToBool(isFromToggled);
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
                    (!string.IsNullOrEmpty(CliRutPosiciones) ? int.Parse(CliRutPosiciones) :
                             IsToggledVisitar ? CurrenClienteAVisitarDespuesDe.CliRutPosicion + 1 :
                             CurrenClienteAVisitarDespuesDe.CliRutPosicion - 1);

                if (ifromtoggled)
                {
                    CliRutPosiciones = CurrenClienteAVisitarDespuesDe != null? IsDespuesEnabled ? (currenClienteAVisitarDespuesDe.CliRutPosicion + 1).ToString() :
                            (currenClienteAVisitarDespuesDe.CliRutPosicion - 1).ToString() : CliRutPosiciones;
                }
            }

            if (cliente != null && cliente.CliID != Arguments.Values.CurrentClient.CliID)
            {
                bool result = await DisplayAlert("Aviso", $"Debe utilizar otra posicion, la que intenta utilizar, ya esta ocupada por este cliente: {cliente.CliRutPosicion} - {cliente.CliNombre} ", "aceptar", "cancelar");                
                IsPositionOut?.Invoke(result);
                return false;
            }

            if (!string.IsNullOrEmpty(CliRutPosiciones) && clientesforvalid != null && (int.Parse(CliRutPosiciones) > (clientesforvalid.CliRutPosicion + 1) || int.Parse(CliRutPosiciones) <= 0))
            {
                await DisplayAlert("Aviso",$"La Secuencia de visita introducida debe de estar dentro del rango permitido (1 y {clientesforvalid.CliRutPosicion + 1})","aceptar");
                return false;
            }
            return true;
        }

        public void ValidadRutPosicion()
        {

            var clientesforvalid = new DS_Clientes().GetAllClientesRutPosicionForValid(cliordenruta:true);

            bool parserut = int.TryParse(CliOrdenRutas, out int rutpos);

            if ((!parserut || clientesforvalid != null) && !string.IsNullOrWhiteSpace(CliOrdenRutas)  && (rutpos > clientesforvalid.CliOrdenRuta + 1 || rutpos <= 0))
            {
                throw new Exception($"Orden de ruta introducido debe de estar dentro del rango permitido (1 y {clientesforvalid.CliOrdenRuta + 1})");                
            }
        }

        public void ValidarNumerosTelefonicos()
        {

            if (TelefonoEnabled && !string.IsNullOrEmpty(CurrentClient?.CliTelefono) && CurrentClient.CliTelefono.Length < "###-###-####".Length)
            {
                throw new Exception("Numero de Telefono invalido");
            }

            if (FaxEnabled && !string.IsNullOrEmpty(CurrentClient?.CliFax) && CurrentClient.CliFax.Length < "###-###-####".Length)
            {
                throw new Exception("Numero de Whatsapp invalido");
            }
        }


        public static string PhoneNumber(string value)
        {
            if (value == null || value.Length > 15)
            {
                return value;
            }

            if (string.IsNullOrEmpty(value)) return string.Empty;
            value = new System.Text.RegularExpressions.Regex(@"\D")
                .Replace(value, string.Empty);
            value = value.TrimStart('1');
            if (value.Length == 7)
                return Convert.ToInt64(value).ToString("###-####");
            if (value.Length == 10)
                return Convert.ToInt64(value).ToString("###-###-####");
            if (value.Length > 10)
                return Convert.ToInt64(value)
                    .ToString("###-###-#### " + new String('#', (value.Length - 10)));
            return value;
        }

        public double DegreeToRadian(double angle)
        {
            var Radians = Math.PI * angle / 180.0;
            return Radians;
        }


        public async void AttempDeleteShedule(SolicitudActualizacionClientesHorarios data)
        {
            var result = await DisplayAlert("Eliminar horario", "Deseas eliminar este horario?", "Eliminar", "Cancelar");

            if (result)
            {
                HorariosAgregados.Remove(data);
            }
        }
    }
}
