using Microsoft.AppCenter.Crashes;
using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.model.Internal.Structs.Args;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
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
using ZXing.Mobile;

namespace MovilBusiness.viewmodel
{
    public class ClientesViewModel : BaseViewModel
    {
        private DS_Clientes myCli;
        private DS_Visitas myVis;
        private DS_Cuadres myCua;
        private DS_RutaVisitas myRut;
        private DS_Cargas myCar;

        private FiltroEstatusVisitaClientes filtroestatusvisita = FiltroEstatusVisitaClientes.TODOS;
        public FiltroEstatusVisitaClientes FiltroEstatusVisitas { get => filtroestatusvisita; set { filtroestatusvisita = value; Search(value); } }

        public ObservableCollection<Clientes> ClientSource { get { return clientsource; } set { clientsource = value; RaiseOnPropertyChanged(); } }
        private ObservableCollection<Clientes> clientsource;

        public List<FiltrosDinamicos> FiltrosSource { get; private set; }

        private FiltrosDinamicos currentfilter;
        public FiltrosDinamicos CurrentFilter { get => currentfilter; set { currentfilter = value; OnFilterValueSelected(); RaiseOnPropertyChanged(); } }
        private List<KV> secondfiltersource;
        public List<KV> SecondFiltroSource { get { return secondfiltersource; } set { secondfiltersource = value; RaiseOnPropertyChanged(); } }
        private KV currentsecondfiltro;
        public KV CurrentSecondFiltro { get { return currentsecondfiltro; } set { currentsecondfiltro = value; if (value != null) { Search(FiltroEstatusVisitas); } RaiseOnPropertyChanged(); } }

        public ICommand SearchCommand { get; private set; }
        public ICommand MenuCommand { get; private set; }
        public ICommand FiltersCommand { get; private set; }

        public string ANombreDe { get; set; }
        public int CliIDMaster { get; set; }

        //cambiar el boton de busqueda 
        public string BtnSearchLogo { get => CurrentFilter != null && CurrentFilter.FilTipo == 3 ? "ic_close_white" : CurrentFilter != null && CurrentFilter.FilTipo == 1 && CurrentFilter.IsCodigoBarra ? "ic_photo_camera_black_24dp" : "ic_search_black_24dp"; set { RaiseOnPropertyChanged(); } }

        private string searchvalue;
        public string SearchValue { get { return searchvalue; } set { searchvalue = value; if (DS_RepresentantesParametros.GetInstance().GetBuscarClienteAlEscribir()) { Search(FiltroEstatusVisitas); } RaiseOnPropertyChanged(); } }

        public bool IsSupervisor { get => Arguments.CurrentUser != null && (Arguments.CurrentUser.RepIndicadorSupervisor || (Arguments.CurrentUser.IsAuditor && IsRutaVisita)); }

        public bool ShowSecondFilter { get { return CurrentFilter != null && CurrentFilter.FilTipo == 2; } set { RaiseOnPropertyChanged(); } }

        public bool IsRutaVisita { get; private set; }
        public bool ShowTodosFilter { get => ParRutaVisitasTipo != 2 || !IsRutaVisita || (ParRutaVisitasTipo == 2 && ParRutaVisitasTipo2Mixto);   }
        public string DateFormat { get => IsRutaVisita && ParRutaVisitasTipo == 2 ? "dddd dd-MM-yyyy" : "dd-MM-yyyy"; }

        private bool showsecondsearch = false;
        public bool ShowSecondSearch { get => showsecondsearch; set { showsecondsearch = value; RaiseOnPropertyChanged(); } }

        private bool showdialogopciones;
        public bool ShowDialogOpciones { get { return showdialogopciones; } set { showdialogopciones = value; RaiseOnPropertyChanged(); } }
        private bool showingfilters = false;
        public bool ShowingFilters { get => showingfilters; set { showingfilters = value; RaiseOnPropertyChanged(); } }

        private int cantidadclientes = 0;
        public int CantidadClientes { get { return cantidadclientes; } set { cantidadclientes = value; RaiseOnPropertyChanged(); } }

        private RutasVisitasArgs rutavisitadata;
        public RutasVisitasArgs RutaVisitaData { get => rutavisitadata; set { rutavisitadata = value; RaiseOnPropertyChanged(); } }

        private DateTime currentfecha = DateTime.Now;
        public DateTime CurrentFecha { get => currentfecha; set { currentfecha = value; RaiseOnPropertyChanged(); LoadDataForRutaVisita(true); } }

        public List<Representantes> Representantes { get; private set; }

        private bool canSearchByRepresentante = false;

        private Representantes currentrepresentante;
        public Representantes CurrentRepresentante { get => currentrepresentante; set { currentrepresentante = value; if (canSearchByRepresentante) { Search(FiltroEstatusVisitas); } RaiseOnPropertyChanged(); } }

        public bool IsFirstTime = true;
        public int ParRutaVisitasTipo = -1;
        private string CliidNovendidos="";
        public bool ParRutaVisitasTipo2Mixto = false;

        public static bool isFromSAC=false;
        private Action OnSyncReset;

        public ClientesViewModel(Page page, bool IsRutaVisita) : base(page)
        {            

            this.IsRutaVisita = IsRutaVisita;
            ClientSource = new ObservableCollection<Clientes>();
            SearchCommand = new Command(()=> { Search(FiltroEstatusVisitas); });
            MenuCommand = new Command(OnOptionMenuItemSelected);
            FiltersCommand = new Command(HandlerFiltersVisibility);
           
            BindFiltrosClientes();

            if (IsRutaVisita)                                    
            {
                ParRutaVisitasTipo = myParametro.GetParRutaVisitaTipo();
            }

            ParRutaVisitasTipo2Mixto = IsRutaVisita && ParRutaVisitasTipo == 2 && DS_RepresentantesParametros.GetInstance().GetParRutaVisitaTipo2Mixto();

            myCli = new DS_Clientes();
            myCua = new DS_Cuadres();
            myVis = new DS_Visitas();
            myRut = new DS_RutaVisitas();
            myCar = new DS_Cargas();

            if (myParametro.GetParGPS())
            {
                Functions.StartListeningForLocations();
            }

            LoadDataForRutaVisita(false);

            if (IsSupervisor)
            {   
                if (Arguments.CurrentUser.IsAuditor)
                {
                    Representantes = new DS_Representantes().GetAllRepresentantesFromClienteDetalle(Arguments.CurrentUser.RepCodigo);
                }
                else
                {
                    Representantes = new DS_Representantes().GetAllRepresentantes();
                }
            }
        }

        private void BindFiltrosClientes()
        {
            FiltrosSource = new DS_FiltrosDinamicos().GetFiltrosClientes();

            if(FiltrosSource != null && FiltrosSource.Count > 0)
            {
                CurrentFilter = FiltrosSource.Where(x => x.FilIndicadorDefault).FirstOrDefault();

                if(CurrentFilter == null)
                {
                    CurrentFilter = FiltrosSource.FirstOrDefault();
                }
            }
        }

        private void HandlerFiltersVisibility()
        {
            ShowingFilters = !ShowingFilters;
        }

        private void LoadDataForRutaVisita(bool reloadList)
        {
            if (!IsRutaVisita)
            {
                return;
            }

            int numeroSemana;

            if (myParametro.GetParSemanasAnios())
            {
                numeroSemana = myRut.GetNumeroSemana(CurrentFecha);
            }
            else {
                numeroSemana = myParametro.GetParPedidosSearchicloSemanaAnt()? Functions.GetWeekOfMonth(CurrentFecha) : Functions.GetWeekOfMonth(CurrentFecha, CurrentRepresentante);
                if (numeroSemana > 4)
                {
                    numeroSemana = 4;
                }
            }

            RutaVisitaData = new RutasVisitasArgs()
            {
                Fecha = CurrentFecha,
                DiaDeLaSemana = (int)(CurrentFecha).DayOfWeek-1,
                NumeroSemana = numeroSemana
            };

            if (reloadList && !IsFirstTime)
            {
                Search(FiltroEstatusVisitas);
            }
        }

        private async Task SearchScaneado(FiltroEstatusVisitaClientes estatusVisita, bool showAlert)
        {

            try
            {

                if (IsBusy)
                {
                    return;
                }

                IsBusy = true;

                CantidadClientes = 0;

                var repcodigo = Arguments.CurrentUser.RepCodigo;

                if (IsSupervisor && canSearchByRepresentante)
                {
                    if (CurrentRepresentante != null)
                    {
                        repcodigo = CurrentRepresentante.RepCodigo;
                        var app = Application.Current;
                        app.Properties["CurrentRep"] = CurrentRepresentante.RepCodigo;
                        await app.SavePropertiesAsync();
                    }
                    else
                    {
                        IsBusy = false;
                        if (showAlert)
                        {
                            await DisplayAlert(AppResource.Warning, AppResource.SelectRepresentativeWarning);
                        }
                        return;
                    }
                }

                var args = new ClientesArgs() { filter = CurrentFilter, secondFilter = CurrentSecondFiltro != null ? CurrentSecondFiltro.Key : "", SearchValue = SearchValue, Estatus = estatusVisita, DiaNumero = -1, NumeroSemana = -1, RepCodigo = repcodigo };

                if (IsRutaVisita)
                {
                    args.NumeroSemana = RutaVisitaData.NumeroSemana;
                    args.DiaNumero = RutaVisitaData.DiaDeLaSemana;
                    args.RutFecha = RutaVisitaData.Fecha;
                }

                if(!myParametro.GetParSemanasAnios() && args.NumeroSemana == 0)
                {
                    IsBusy = false;
                    return;
                }

                await Task.Run(() => { ClientSource = IsRutaVisita ? myRut.GetClientes(args) : myCli.GetClientes(args); });

                CantidadClientes = ClientSource != null ? ClientSource.Count : 0;

                if (ClientSource != null && ClientSource.Count == 1)
                {
                    Arguments.Values.CurrentClient = ClientSource[0];
                    await GoCrearVisita();
                }

            }
            catch (Exception e)
            {
                IsBusy = false;

                await DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;

        }

        public async void Search(FiltroEstatusVisitaClientes estatusVisita, bool showAlert = true)
        {
            try
            {

                if (CurrentFilter != null && CurrentFilter.IsCodigoBarra )
                {
                    GoScanQr();

                    return;
                }
                else
                {
                        if (IsBusy)
                        {
                            return;
                        }

                        IsBusy = true;

                        CantidadClientes = 0;

                        var repcodigo = Arguments.CurrentUser.RepCodigo;
                      
                    if (IsSupervisor && canSearchByRepresentante)
                    {
                         if (CurrentRepresentante != null)
                         {
                             repcodigo = CurrentRepresentante.RepCodigo;
                            var app = Application.Current;
                            app.Properties["CurrentRep"] = CurrentRepresentante.RepCodigo;
                            await app.SavePropertiesAsync();
                            LoadDataForRutaVisita(true);
                         }
                         else
                         {
                             IsBusy = false;
                             if (showAlert)
                             {
                                await DisplayAlert(AppResource.Warning, AppResource.SelectRepresentativeWarning);
                             }
                             return;
                         }
                    }

                        var args = new ClientesArgs() { filter = CurrentFilter, secondFilter = CurrentSecondFiltro != null ? CurrentSecondFiltro.Key : "", SearchValue = SearchValue, Estatus = estatusVisita, DiaNumero = -1, NumeroSemana = -1, RepCodigo = repcodigo };

                        if (IsRutaVisita)
                        {
                            args.NumeroSemana = RutaVisitaData.NumeroSemana;
                            args.DiaNumero = RutaVisitaData.DiaDeLaSemana;
                            args.RutFecha = RutaVisitaData.Fecha;
                        }

                    if(!myParametro.GetParSemanasAnios() && args.NumeroSemana == 0)
                    {
                        IsBusy = false;
                        return;
                    }

                        await Task.Run(() => { ClientSource = IsRutaVisita ? myRut.GetClientes(args) : myCli.GetClientes(args); });

                        CantidadClientes = ClientSource != null ? ClientSource.Count : 0;

                        if (myParametro.GetParProductosNoVendidosOnline())
                        {
                            var CliidProductosNoVendidos = ClientSource;

                            foreach (var cli in CliidProductosNoVendidos)
                            {
                                CliidNovendidos += string.IsNullOrEmpty(CliidNovendidos) ? cli.CliID.ToString() : "," + cli.CliID.ToString();
                            }
                        }
                }
            }
            catch(Exception e)
            {
                IsBusy = false;

                await DisplayAlert(AppResource.ErrorLoadingClients, e.Message, AppResource.Aceptar);
            }
            
            IsBusy = false;
            
        }


        //sistema para buscar clientes por codigo de barra
        public async void SearchUnAsync(bool resumen, bool showAlert = true) { await SearchScaneado(FiltroEstatusVisitas, showAlert); }
        public bool isScanning;
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

        public async void ShowDialogOpcionesVisita(Clientes cliente)
        {
            if(cliente == null)
            {
                return;
            }

            try
            {

               // CurrentCliente = cliente;

                Arguments.Values.CurrentClient = cliente;


                if (myParametro.GetParClientesCambiarValorPorCanid())
                {
                    new DS_TablasRelaciones().GetTablasRelaciones("Clientes", "UsosMultiples");
                }

                if (!myParametro.GetParMostrarVisitaMenu())
                {
                    await GoCrearVisita();
                    return;
                }

                //ShowDialogOpciones = true;

                var dialog = DependencyService.Get<Abstraction.IDialogOpcionesVisita>();
                dialog.SetEventHandler(ClientOptionSelected);

                dialog.Show(cliente.CliNombre, cliente.CliCodigo);

            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.Error, e.Message, AppResource.Aceptar);
            }
        }

        private void OnOptionMenuItemSelected(object Id)
        {
            switch (Id.ToString())
            {
                case "0": OpenClientsOnMap(); break;
                case "1": Sincronizar(true); break;
                case "2": ShowProductsNoVendidos(CliidNovendidos, CantidadClientes, IsRutaVisita); break;//Productos no vendidos
            }
        }

        private void OpenClientsOnMap()
        {
            try
            {
                var clients = ClientSource.Where(x => x.CliLatitud >= -90 && x.CliLatitud <= 90 && x.CliLongitud >= -180 && x.CliLongitud <= 180).ToList();

                var locations = new List<Location>();

                int step = 1;
                foreach (var cli in clients.OrderBy(x=>x.CliRutPosicion))
                {
                    locations.Add(new Location(cli.CliLatitud, cli.CliLongitud) { Label = cli.CliNombre + " - " + cli.CliCodigo, Position = step.ToString() });
                    step++;
                }

                if (locations.Count > 0)
                {
                    var lat = 0.0;
                    var longitude = 0.0;

                    if (Arguments.Values.CurrentLocation != null)
                    {
                        lat = Arguments.Values.CurrentLocation.Latitude;
                        longitude = Arguments.Values.CurrentLocation.Longitude;
                    }

                    PushAsync(new MapsPage(locations, lat, longitude));
                }
                else
                {
                    throw new Exception(AppResource.NoGeoreferencedClient);
                }

            }catch(Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message);
            }
        }

        private async void Sincronizar(bool ask = false, bool fromSyncVis = false)
        {
            try
            {
                if (ask && !await DisplayAlert(AppResource.SyncUp, AppResource.GoSyncQuestion, AppResource.SyncUp, AppResource.Cancel))
                {
                    return;
                }

                if(fromSyncVis) 
                    await PushModalAsync(new SincronizarModal(OnSyncCompleted: () => { OnSyncReset.Invoke(); } ));
                else
                    await PushModalAsync(new SincronizarModal());
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.SynchronizationFailed, e.Message, AppResource.Aceptar);
            }
        }

        public void OpenClientLocation(Clientes cliente = null)
        {
            if(cliente != null)
            {
                Arguments.Values.CurrentClient = cliente;
            }

            if(Arguments.Values.CurrentClient == null || (Arguments.Values.CurrentClient.CliLatitud == 0 && Arguments.Values.CurrentClient.CliLongitud == 0))
            {
                DisplayAlert(AppResource.Warning, AppResource.ClientIsNotGeoreferenced);
                return;
            }

            var latitud = Arguments.Values.CurrentClient.CliLatitud;
            var longitud = Arguments.Values.CurrentClient.CliLongitud;

            //Functions.OpenMap(latitud, longitud, Arguments.Values.CurrentClient.CliNombre);

            PushAsync(new MapsPage(
                latitud,
                longitud, 
                Arguments.Values.CurrentClient.CliNombre + " - " + Arguments.Values.CurrentClient.CliCodigo
                ));

        }

        private async void ClientOptionSelected(OpcionesClientes option)
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                switch (option)
                {
                    case OpcionesClientes.ConsultarCliente:

                        await PushAsync(new OperacionesPage(true));

                        //if(myParametro.GetParVisitasSeleccionarTipoManual() == 2)
                        //{
                        //    var result = await DisplayActionSheet(AppResource.Select, buttons: new DS_UsosMultiples()
                        //        .GetTipoVisita().Select(v => v.Descripcion).ToArray());

                        //    await PushAsync(new OperacionesPage(true, TypeOfVisit: result));
                        //}else
                        //{
                        //    await PushAsync(new OperacionesPage(true));
                        //}

                        break;
                    case OpcionesClientes.InformacionCliente:
                        await PushAsync(new InfoClientePage(Arguments.Values.CurrentClient));
                        break;
                    case OpcionesClientes.UltimasVisitas: //consultar ultimas visitas
                        await PushAsync(new ConsultaVisitasPage(Arguments.Values.CurrentClient));
                        break;
                    case OpcionesClientes.CrearVisita:
                        await GoCrearVisita();
                        break;
                    case OpcionesClientes.VisitaFallida:
                        if (myParametro.GetParNoCrearVisitas())
                        {

                            await DisplayAlert(AppResource.Warning, AppResource.ProfileCannotMakeVisits);
                            
                        }
                        else
                        {
                            await PushModalAsync(new VisitaFallidaModal(AttempSaveVisitaFallida));
                            
                        }
                        break;
                    case OpcionesClientes.UbicacionCliente:
                        OpenClientLocation();
                        break;
                }
            }catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
        }

        private async Task GoCrearVisita(bool VisitClientOutOfRoute = false, bool CustomerHasNoGeoRegistered = false, bool VisitOutsideClientLocation= false)
        {
            try
            {
                if (Arguments.CurrentUser != null &&  Arguments.CurrentUser.TipoRelacionClientes == 2)
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

                bool isAutorizeCustomerHasNoGeoRegistered = CustomerHasNoGeoRegistered;
                bool isAutorizeVisitOutsideClientLocation = VisitOutsideClientLocation;
                bool isAutorizeVisitClientOutOfRoute = VisitClientOutOfRoute;

                int NumeroTransaccion = DS_RepresentantesSecuencias.GetLastSecuencia("Visitas");
                int GenerarVisitaVirtual = -1;

                if (myParametro.GetParNoCrearVisitas())
                {

                    await DisplayAlert(AppResource.Warning, AppResource.ProfileCannotMakeVisits);
                    return;
                }

                var parCuadreDiarios = myParametro.GetParCuadresDiarios();

                if (myParametro.GetParCuadres() > 0 && (Arguments.Values.CurrentCuaSecuencia == -1 || (parCuadreDiarios && myCua.GetCuadreAbierto(DateTime.Now.ToString("dd-MM-yyyy")) == null)))
                {

                    if (myParametro.GetParAperturarCuadrePorAuditor())
                    {
                        await DisplayAlert(AppResource.Warning, "No tienes un cuadre abierto" + (myParametro.GetParCuadresDiarios() ? " para el dia de hoy" : "") + ", debes de abrir un cuadre para poder realizar la visita", AppResource.Aceptar);
                        return;
                    }

                    myCua.AbrirCerrarCuadre(parCuadreDiarios, (isCerrarCuadre,isimprimir)=> 
                    {
                        if((isCerrarCuadre && myParametro.GetParSincronizarAlCerrarCuadre()) || (!isCerrarCuadre && myParametro.GetParSincronizarAlAbrirCuadre()))
                        {
                            Sincronizar();
                        }
                    });
                    return;
                }

                var parRecibosHorasMaximasSinDepositar = myParametro.GetParRecibosCantidadHorasMaximasSinDepositar();

                if(parRecibosHorasMaximasSinDepositar > 0 && new DS_Recibos().HayRecibosSinDepositarExcedenHoras(parRecibosHorasMaximasSinDepositar))
                {
                    await DisplayAlert(AppResource.Warning, AppResource.ReceiptsExceedHours + parRecibosHorasMaximasSinDepositar.ToString());
                    return;
                }

                if (myParametro.GetParCargaObligatoria() && myCar.GetCargasDisponibles().Count > 0)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.LoadsBeforeVisits);
                    return;
                }


                if (myParametro.GetParCargaNegativaObligatoria() && myCar.GetCargasNegativasDisponibles().Count > 0)
                {
                    await DisplayAlert(AppResource.Warning, "Debe aceptar/rechazar las cargas negativas antes de crear la visita");
                    return;
                }

                if (myParametro.GetParGPS() && Arguments.Values.CurrentLocation == null)
                {
                    if (!CrossGeolocator.Current.IsGeolocationEnabled)
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.GpsOffMessage, AppResource.Aceptar);
                        return;
                    }
                    await DisplayAlert(AppResource.Warning, AppResource.NoGeolocationMessage, AppResource.Aceptar);
                    return;
                }

                var parSectores = myParametro.GetParSectores();

                if(parSectores > 2 && !myCli.PertenceAlgunSector(Arguments.Values.CurrentClient.CliID))
                {
                    await DisplayAlert(AppResource.Warning, AppResource.ClientHasNoSector);
                    return;
                }

                if(myParametro.GetParNoVisitarClienteFueraRuta())
                {
                    var clientesAVisitar = new DS_RutaVisitas().GetClientesAVisitar();
                    var clienteEnRuta = clientesAVisitar.FirstOrDefault(c => c.CliID == Arguments.Values.CurrentClient.CliID);

                    if (clienteEnRuta == null && !VisitClientOutOfRoute)
                    {
                        var result = await DisplayAlert(AppResource.Warning, AppResource.VisitClientOutOfRoute, AppResource.Authorize, AppResource.Cancel);

                        if (result)
                        {
                            await Application.Current.MainPage.Navigation.PushModalAsync(new AutorizacionesModal(false, NumeroTransaccion, 13, null, false, false)
                            {
                                OnAutorizacionUsed = async (autSec) =>
                                {
                                    isAutorizeVisitClientOutOfRoute = true;
                                    await GoCrearVisita(isAutorizeVisitClientOutOfRoute);
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
                }

                var CliLatitud = Arguments.Values.CurrentClient.CliLatitud;
                var CliLongitud = Arguments.Values.CurrentClient.CliLongitud;

                if (myParametro.GetParCalcularSegunTipoVisita() > 1)
                {
                    var VisLatitud = Arguments.Values.CurrentLocation.Latitude;
                    var VisLongitud = Arguments.Values.CurrentLocation.Longitude;

                    if (CliLatitud == 0 || CliLongitud == 0)
                    {
                        if (myParametro.GetParGoToSACfromClientes()==1)
                        {
                            await DisplayAlert(AppResource.Warning, AppResource.RequestClientGeoUpdateMessage, AppResource.Aceptar);
                            await PushAsync(new SacPage(Arguments.Values.CurrentClient));
                            return;
                        }
                        else if (!CustomerHasNoGeoRegistered)
                        {
                            var result = await DisplayAlert(AppResource.Warning, AppResource.CustomerHasNoGeoRegistered, AppResource.Authorize, AppResource.Cancel);

                            if (result)
                            {
                                await Application.Current.MainPage.Navigation.PushModalAsync(new AutorizacionesModal(false, NumeroTransaccion, 13, null, false, false)
                                {
                                    OnAutorizacionUsed  = async (autSec) =>
                                    {
                                        isAutorizeCustomerHasNoGeoRegistered = true;
                                        await GoCrearVisita(isAutorizeVisitClientOutOfRoute,isAutorizeCustomerHasNoGeoRegistered);
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

                            //await DisplayAlert(AppResource.Warning, AppResource.CustomerHasNoGeoRegistered, AppResource.Aceptar);
                            //return;
                        }
                        
                    }
                    else if(VisLatitud == 0 || VisLongitud == 0)
                    {
                        if (!CrossGeolocator.Current.IsGeolocationEnabled)
                        {
                            await DisplayAlert(AppResource.Warning, AppResource.GpsOffMessage, AppResource.Aceptar);
                            return;
                        }
                        await DisplayAlert(AppResource.Warning, AppResource.NoGeolocationMessage, AppResource.Aceptar);
                        return;
                    }
                    var DiferenciaDistancia = myParametro.GetParDistanciaVisita();

                    var DistanciaKM = 6371 * Math.Acos(Math.Cos(DegreeToRadian((90 - VisLatitud))) * Math.Cos(DegreeToRadian((90 - CliLatitud)))
                   + Math.Sin(DegreeToRadian((90 - VisLatitud))) * Math.Sin(DegreeToRadian((90 - CliLatitud)))
                   * Math.Cos(DegreeToRadian((VisLongitud - CliLongitud))));

                    var DistanciaM = DistanciaKM * 1000;
                    

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
                    else if (myParametro.GetParCalcularSegunTipoVisita() == 3)
                    {
                        if ((DiferenciaDistancia <= DistanciaM) && (!VisitOutsideClientLocation))
                        {
                            var result = await DisplayAlert(AppResource.Warning, AppResource.VisitOutsideClientLocation, AppResource.Authorize, AppResource.Cancel);

                            if (result)
                            {
                                await Application.Current.MainPage.Navigation.PushModalAsync(new AutorizacionesModal(false, NumeroTransaccion, 13, null, false, false)
                                {
                                    OnAutorizacionUsed = async (autSec) =>
                                    {
                                        isAutorizeVisitOutsideClientLocation = true;
                                        await GoCrearVisita(isAutorizeVisitClientOutOfRoute,isAutorizeCustomerHasNoGeoRegistered, isAutorizeVisitOutsideClientLocation);
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

                            //await DisplayAlert(AppResource.Warning, AppResource.VisitOutsideClientLocation, AppResource.Aceptar);
                            //return;
                        }
                    }
                    else if (myParametro.GetParCalcularSegunTipoVisita() == 4)
                    {
                        if ((DiferenciaDistancia <= DistanciaM) && (!VisitOutsideClientLocation) && !myVis.HasVisitaPresencial(Arguments.Values.CurrentClient.CliID))
                        {
                            var result = await DisplayAlert(AppResource.Warning, AppResource.VisitOutsideClientLocation, AppResource.Authorize, AppResource.Cancel);

                            if (result)
                            {
                                await Application.Current.MainPage.Navigation.PushModalAsync(new AutorizacionesModal(false, NumeroTransaccion, 13, null, false, false)
                                {
                                    OnAutorizacionUsed = async (autSec) =>
                                    {
                                        isAutorizeVisitOutsideClientLocation = true;
                                        await GoCrearVisita(isAutorizeVisitClientOutOfRoute,isAutorizeCustomerHasNoGeoRegistered, isAutorizeVisitOutsideClientLocation);
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
                    }

                }

                IsBusy = true;

                if (myParametro.GetParGoToSACfromClientes() == 2 && (CliLatitud == 0 || CliLongitud == 0))
                {
                    isFromSAC = true;
                    await DisplayAlert(AppResource.Warning, AppResource.RequestClientGeoUpdateMessage, AppResource.Aceptar);
                    await PushAsync(new SacPage(Arguments.Values.CurrentClient));
                    return;
                }

                var task = new TaskLoader() { SqlTransactionWhenRun = true };

                await task.Execute(() =>
                {
                    Arguments.Values.CurrentVisSecuencia = myVis.CrearVisita(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentLocation, GenerarVisitaVirtual);

                    if (myParametro.GetOfertasConSegmento())
                    {
                        new DS_Ofertas().GuardarProductosValidosParaOfertasPorSegmento(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentClient.TiNID);
                    }
                    else
                    {
                        new DS_Ofertas().GuardarProductosValidosParaOfertas(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentClient.TiNID);
                    }
                });

                if(parSectores > 2)
                {
                    await PushAsync(new ElegirSectorPage());
                }
                else
                {
                    Application app = Application.Current;
                    if (app.Properties.ContainsKey("SecCodigo"))
                    {
                        app.Properties.Remove("SecCodigo");
                    }


                    if (myParametro.GetSyncAutoInVisita())
                    {
                        Sincronizar(fromSyncVis:true);

                        OnSyncReset = async () =>
                        {
                            if (myParametro.GetParVisitasSeleccionarTipoManual() == 2)
                            {
                                var result = await DisplayActionSheet(AppResource.Select, buttons: new DS_UsosMultiples()
                                    .GetTipoVisita().Select(v => v.Descripcion).ToArray());

                                await PushAsync(new OperacionesPage(TypeOfVisit: result));
                            }
                            else
                            {
                                await PushAsync(new OperacionesPage());
                            }
                        };
                        

                    }
                    else 
                    { 

                        if (myParametro.GetParVisitasSeleccionarTipoManual() == 2)
                        {
                            var result = await DisplayActionSheet(AppResource.Select, buttons: new DS_UsosMultiples()
                                .GetTipoVisita().Select(v => v.Descripcion).ToArray());
                            
                            await PushAsync(new OperacionesPage(TypeOfVisit: result));
                        }else
                        {
                            await PushAsync(new OperacionesPage());
                        }
                    }

                }                
            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
        }


        public double DegreeToRadian(double angle)
        {
            var Radians = Math.PI * angle / 180.0;
            return Radians;
        }

        public async void AttempSaveVisitaFallida(string Motivo)
        {
            if (string.IsNullOrWhiteSpace(Motivo))
            {
                await DisplayAlert(AppResource.Warning, AppResource.SpecifyReasonWarning, AppResource.Aceptar);
                return;
            }

            if (myParametro.GetParGPS() && Arguments.Values.CurrentLocation == null)
            {
                if (!CrossGeolocator.Current.IsGeolocationEnabled)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.GpsOffMessage, AppResource.Aceptar);
                    return;
                }
                await DisplayAlert(AppResource.Warning, AppResource.NoGeolocationMessage, AppResource.Aceptar);
                return;
            }

            try
            {

                double CliLatitud = 0, CliLongitud = 0, VisLatitud = 0, VisLongitud = 0;
                var distanciaLimite = myParametro.GetParDistanciaVisita();
                bool GenerarVisitaVirtual = false;

                if (Arguments.Values.CurrentLocation != null && Arguments.Values.CurrentClient != null)
                {
                    CliLatitud = Arguments.Values.CurrentClient.CliLatitud;
                    CliLongitud = Arguments.Values.CurrentClient.CliLongitud;
                    VisLatitud = Arguments.Values.CurrentLocation.Latitude;
                    VisLongitud = Arguments.Values.CurrentLocation.Longitude;
                }

                if ((VisLatitud == 0 || VisLongitud == 0) && myParametro.GetParGPS())
                {
                    if (!CrossGeolocator.Current.IsGeolocationEnabled)
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.GpsOffMessage, AppResource.Aceptar);
                        return;
                    }
                    await DisplayAlert(AppResource.Warning, AppResource.NoGeolocationMessage, AppResource.Aceptar);
                    return;
                }

                if(CliLatitud != 0 && CliLongitud != 0)
                {
                    var DistanciaKM = 6371 * Math.Acos(Math.Cos(DegreeToRadian((90 - VisLatitud))) * Math.Cos(DegreeToRadian((90 - CliLatitud)))
                    + Math.Sin(DegreeToRadian((90 - VisLatitud))) * Math.Sin(DegreeToRadian((90 - CliLatitud)))
                    * Math.Cos(DegreeToRadian((VisLongitud - CliLongitud))));

                    var DistanciaM = DistanciaKM * 1000;
                    GenerarVisitaVirtual = DistanciaM > distanciaLimite;
                }

                IsBusy = true;
                TaskLoader task = new TaskLoader() { SqlTransactionWhenRun = true };

                await task.Execute(() => 
                {
                    myVis.CrearVisitaFallida(Arguments.Values.CurrentClient.CliID, Motivo, Arguments.Values.CurrentLocation, GenerarVisitaVirtual);
                });

                await DisplayAlert(AppResource.Warning, AppResource.FailedVisitsSuccessfullySaved, AppResource.Aceptar);

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.ErrorSavingVisitFailed, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
            Search(FiltroEstatusVisitas);
        }

        private void OnFilterValueSelected()
        {
            try
            {
                if (CurrentFilter != null && CurrentFilter.FilTipo == 2)
                {
                    ShowSecondFilter = true;
                    SecondFiltroSource = Functions.DinamicQuery(CurrentFilter.FilComboSelect);
                    if (SecondFiltroSource != null && SecondFiltroSource.Count > 0)
                    {
                        CurrentSecondFiltro = SecondFiltroSource[0];
                    }
                }
                else
                {
                    CurrentSecondFiltro = null;
                    SecondFiltroSource = new List<KV>();
                    CurrentSecondFiltro = null;
                    ShowSecondFilter = false;

                    BtnSearchLogo = "";
                }
            }catch(Exception e)
            {
                DisplayAlert(AppResource.ErrorLoadingFilters, e.Message);
            }
        }

        public void ShowOrHideSecondSearch()
        {
            ShowSecondSearch = IsRutaVisita && ParRutaVisitasTipo == 2 && FiltroEstatusVisitas == FiltroEstatusVisitaClientes.VISITADO;
        }

        public void SelectDefaultFilter()
        {
            if(FiltrosSource == null)
            {
                CurrentFilter = null;
                return;
            }

            if (myParametro.GetParBusquedaCombinadaPorDefault())
                CurrentFilter = FiltrosSource.Where(x => x.FilDescripcion == "Combinada").FirstOrDefault();
            else
                CurrentFilter = FiltrosSource.Where(x => x.FilIndicadorDefault).FirstOrDefault();

            if(CurrentFilter == null)
            {
                CurrentFilter = FiltrosSource.FirstOrDefault();
            }

            if (IsSupervisor)
            {


                if (App.Current.Properties.ContainsKey("CurrentRep"))
                {
                    CurrentRepresentante = Representantes.FirstOrDefault(r => r.RepCodigo == App.Current.Properties["CurrentRep"].ToString());
                }
                else
                {
                    CurrentRepresentante = Representantes.FirstOrDefault(x => x.RepCodigo.Trim().ToUpper() == Arguments.CurrentUser.RepCodigo.Trim().ToUpper());
                }                

                if(CurrentRepresentante == null)
                {
                    CurrentRepresentante = Representantes.FirstOrDefault();
                }

                canSearchByRepresentante = true;
            }
        }

        public void CargarCliDatosOtros()
        {
            Arguments.Values.CliDatosOtros = new DS_UsosMultiples().GetCliCaracteristicas();
        }

        public async void AplicarCargasAutomaticas()
        {
            //Aplica cargas automaticamente que tengan estatus 7- Aplicacion Automatica
            if (myParametro.GetParCargasAplicacionAutomaticas())
            {
                DS_Cargas myCar = new DS_Cargas();
                var cargasAutomaticas = myCar.GetCargasDeAplicacionAutomaticaDisponibles();
                List<string> cargasAceptadas = new List<string>();
                if (cargasAutomaticas != null && cargasAutomaticas.Count > 0)
                {
                    foreach (var carga in cargasAutomaticas)
                    {
                        var referenciaEntrega = "";
                        if (myParametro.GetParCargasConReferenciaEntrega())
                        {
                            referenciaEntrega = myCar.GetCargaBySecuenciaConRefEntrega(carga.CarSecuencia).CarReferenciaEntrega;
                        }

                        if (carga.CarCantidadTotal == myCar.GetTotalProductosCarga(carga.CarSecuencia))
                        {
                            var productosCarga = myCar.GetProductosCarga(carga.CarSecuencia);
                            myCar.AceptarCarga(carga.rowguid, productosCarga.ToList(), carga.AlmID, referenciaEntrega,"mdsoft");
                            cargasAceptadas.AddRange(cargasAutomaticas.Where(c => (carga.CarSecuencia == c.CarSecuencia)).Select(c => (c.CarSecuencia + "-" + c.CarReferencia).ToString()).Distinct().ToList());
                        }
                    }

                    if (cargasAceptadas != null && cargasAceptadas.Count > 0)
                    {
                        await DisplayActionSheet(AppResource.AutomaticallyAppliedLoads, "Aceptar", cargasAceptadas.ToArray());
                    }
                }

            }
        }

        public void ReciclarCliDatosOtros()
        {
            Arguments.Values.CliDatosOtros = null;
        }

        private async void ShowProductsNoVendidos(string Cliids,int cantidadClientes,bool IsRutaVisita=false)
        {
            await PushAsync(new ProductosNoVendidosPage(Cliids, IsRutaVisita));
        }

        public void SubscribeToListeners()
        {
           
            MessagingCenter.Subscribe<string, string>(this, "BtnStartCall", (sender, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args))
                {
                    BtnStartCall(args);
                }
            });
        }


        public void UnSubscribeFromListeners()
        {
            MessagingCenter.Unsubscribe<string, string>(this, "BtnStartCall");
      
        }

        private async void BtnStartCall(string  Phone)
        {
            try
            {
                if (IsBusy)
                {
                    return;
                }

                IsBusy = true;


               var result =await DisplayAlert(AppResource.Call, AppResource.WantCallMessage, AppResource.Call +" "+ Phone + "",AppResource.Cancel);

               if(result)
                {
                    var dialer = DependencyService.Get<IDialerService>();
                    dialer.Call(Phone);
                }

            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }
    }
}
