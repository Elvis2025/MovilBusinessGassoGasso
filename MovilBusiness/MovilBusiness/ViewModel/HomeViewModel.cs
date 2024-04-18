using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.Model;
using MovilBusiness.Printer;
using MovilBusiness.Printer.Formats;
using MovilBusiness.Resx;
using MovilBusiness.Services;
using MovilBusiness.Utils;
using MovilBusiness.views;
using MovilBusiness.Views;
using MovilBusiness.Views.Components.Modals;
using Plugin.Connectivity;
using Xamarin.Forms;

namespace MovilBusiness.viewmodel
{
    public class HomeViewModel : BaseViewModel
    {
        private ObservableCollection<model.Internal.MenuItem> menusource;
        public ObservableCollection<model.Internal.MenuItem> MenuSource { get => menusource; private set { menusource = value; RaiseOnPropertyChanged(); } }
        private model.Internal.MenuItem selecteditem;
        public model.Internal.MenuItem SelectedItem { get { return selecteditem; } set { selecteditem = value; RaiseOnPropertyChanged(); OnOptionItemSelectedAsync(); } }
        private PrinterManager printer;

        private CuadresFormats Printer;

        private model.Internal.MenuItem menunoticias;
        public model.Internal.MenuItem MenuNoticias { get => menunoticias; set { menunoticias = value; RaiseOnPropertyChanged(); } }
        

        private int pendingcargascount;
        public int PendingCargasCount { get => pendingcargascount; set { pendingcargascount = value; RaiseOnPropertyChanged(); } }
        
        public string ProductosModuloDescripcion { get; private set; } = AppResource.Products;

        public ICommand GoModuleCommand { get; private set; }
        public Action OnOptionMenuItemSelected { get; set; }

        public bool ShowBtnCargas { get; set; }
        public bool ShowBtnCargasCancelar { get; set; }
        public bool ShowBtnReporte { get; set; }
        public bool ShowBtnConInv { get; set; }
        public bool ShowBtnProspectos { get; set; }
        public bool ShowBtnDepositos { get; set; }
        public bool ShowBtnCuadre { get; set; }
        public bool ShowBtnInvFisico { get; set; }
        public bool ShowBtnProductos { get; set; }
        public bool ShowBtnPresupuestos { get; set; }

        public bool ShowBtnClientes { get; set; } = true;
        public bool ShowBtnReqInv { get; set; } = true;


        private bool showFromNoticias = false, visitaPendienteVerificada;
        private DS_Depositos myDep;
        private DS_Compras myCom;
        private DS_TransaccionesImagenes myImg;
        private DS_Gastos myGas;
        private DS_Cuadres myCua;
        private DS_Noticias myNot;
        private DS_Cargas myCar;
        private DS_ConteosFisicos myCont;
        private DS_Productos myProd;
        private DS_Inventarios myInv;
        private DS_Mensajes myMen;
        private ConsultarOfertasModal dialogConsultaOfertas;

        public static HomeViewModel instance;
        public List<int> listalmid;
        public bool inTemp;

        public HomeViewModel(Page page) : base(page)
        {
            myDep = new DS_Depositos();
            myCom = new DS_Compras();
            myImg = new DS_TransaccionesImagenes();
            myGas = new DS_Gastos();
            myCua = new DS_Cuadres();
            myNot = new DS_Noticias();
            myCar = new DS_Cargas();
            myCont = new DS_ConteosFisicos();
            myProd = new DS_Productos();
            myInv = new DS_Inventarios();
            myMen = new DS_Mensajes();
            Printer = new CuadresFormats(myCua);
            inTemp = false;

            instance = this;

            ProductosModuloDescripcion = myParametro.GetParCatalogoProductos() ? AppResource.ProductCatalog : AppResource.Products;

            CargarCantidadNoticias();

            BindMenu();

            GoModuleCommand = new Command(GoToModule);

            IScreen screen = DependencyService.Get<IScreen>();

            ShowBtnCargas = myParametro.GetParHomeBtnCargas() || myParametro.GetParCargasInventario();
            ShowBtnCargasCancelar = myParametro.GetParHomeBtnCargasCancelar();
            ShowBtnCuadre = myParametro.GetParHomeBtnCuadres() || (myParametro.GetParCuadres() > 0);
            ShowBtnProspectos = myParametro.GetParHomeBtnProspectos();
            ShowBtnInvFisico = myParametro.GetParHomeBtnConteoFisico();
            ShowBtnReporte = myParametro.GetParHomeBtnReportes();
            ShowBtnConInv = myParametro.GetParHomeBtnConsultaInventarios();
            ShowBtnReqInv = myParametro.GetParHomeBtnRequisicionInventario();
            ShowBtnDepositos = myParametro.GetParHomeBtnDepositos();
            ShowBtnClientes = myParametro.GetParHomeClientes();
            ShowBtnProductos = myParametro.GetParHomeBtnProductos();
            ShowBtnPresupuestos = myParametro.GetParHomeBtnPresupuestos();


            if (screen != null)
            {
                screen.KeepLightsOn(false);
            }

            if (myParametro.GetParGPS())
            {
                Functions.StartListeningForLocations();
            }

            ResetCuaSecuencia();
        }

        public static HomeViewModel getInstance()
        {
            return instance;
        }

        private void CargarCantidadNoticias()
        {

            var noticiasSinLeer = myNot.GetCantidadNoticiasSinLeer();

            string badge = null;

            if (noticiasSinLeer > 0)
            {
                badge = noticiasSinLeer > 99 ? "99+" : noticiasSinLeer.ToString();
            }

            if (MenuNoticias == null || MenuSource == null)
            {
                MenuNoticias = new model.Internal.MenuItem() { Title = AppResource.News, Id = 2, Icon = "ic_event_black", Badge = badge };
            }
            else
            {
                var newMenu = MenuNoticias.Copy();
                newMenu.Badge = badge;

                var index = MenuSource.IndexOf(MenuNoticias);

                if (index == -1)
                {
                    return;
                }
                MenuSource[index] = newMenu;
                MenuNoticias = newMenu;
            }

        }

        public void RecycleImages()
        {
            myImg.DeleteTemp(false);
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
                            myCar.AceptarCarga(carga.rowguid, productosCarga.ToList(), carga.AlmID, referenciaEntrega, "mdsoft");
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

        public void CarPendingCount()
        {
            int pendingcount = myCar.GetCargasDisponibles().Count;

            PendingCargasCount = pendingcount;
        }

        private async void GoToModule(object args)
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                var pref = new PreferenceManager();

                if (DS_Representantes.RepresentateIsInactive(Arguments.CurrentUser.RepCodigo) && args.ToString() != "5")
                {
                    await DisplayAlert(AppResource.Warning, AppResource.UserIsInactive, AppResource.Aceptar);
                    IsBusy = false;
                    return;
                }

                switch (args.ToString())
                {
                    case "1": //ruta visita
                        await PushAsync(new ClientesPage(true));
                        break;
                    case "2": //clientes
                        await PushAsync(new ClientesPage(false));
                        break;
                    case "3": //productos
                        if (myParametro.GetParCatalogoProductos()){
                            await PushAsync(new CatalogoProductosPage(true));
                        }
                        else
                        {
                            await PushAsync(new ProductosPage());
                        }
                       
                        break;
                    case "4": //presupuestos                  
                        await PushAsync(new PresupuestosPage());
                        break;
                    case "5": //sincronizar
                        AlertSincronizar();
                        break;
                    case "6": //cuadres
                        var cuadreAbierto = myCua.GetCuadreAbierto();
                        if (cuadreAbierto == null)
                        {
                            if (myParametro.GetParAperturarCuadrePorAuditor())
                            {
                                await PushModalAsync(new LoginAuditorModal((auditor) => { GoAbrirCerrarCuadres(RepAuditor: auditor); }));

                                return;
                            }
                            else
                            {
                                GoAbrirCerrarCuadres();
                            }
                        }
                        else
                        {
                            GoAbrirCerrarCuadres();
                        }

                        break;
                    case "7": //cargas
                        if (!myCar.HayCargasDisponibles())
                        {
                            await DisplayAlert(AppResource.Warning, AppResource.NoLoadsAvailables);
                            IsBusy = false;
                            return;
                        }
                        await PushAsync(new CargasPage());
                        break;
                    case "8": //inv fisico vendedor
                        await RecoverProductsInTemp();
                        if (!inTemp) {
                            if (myParametro.GetParConteoFisicoPorAuditor())
                            {
                                await PushModalAsync(new LoginAuditorModal((auditor) => { PrepareGoConteoFisico(false, -1, false, auditor); }));

                                return;
                            }
                            else
                            {
                                PrepareGoConteoFisico(false);
                            }
                        }
                        else
                        {
                            GoConteoFisico(true);
                            return;
                        }
                        
                        break;
                    case "9": //reportes
                        await PushAsync(new ReportesPage());
                        break;
                    case "10": //Consulta Inventario
                        await PushAsync(new ConsultaInventarioPage());
                        break;
                    case "11": //depositos
                        await PushAsync(new DepositosPage());
                        break;
                    case "12": //prospectos
                        await PushAsync(new ProspectosTabPage());
                        break;
                    case "13": //cargas canceladas
                        if (myParametro.GetParCancelarCargaPorAuditor())
                        {
                            await PushModalAsync(new LoginAuditorModal((auditor) => { CancelarCargaAceptada(RepAuditor: auditor); }));

                            return;
                        }
                        else
                        {
                            CancelarCargaAceptada();
                        }
                        
                        break;
                    case "14": //RequisicionInventario
                        GoTraspasos(true);
                        break;
                }
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        public async void PrepareGoConteoFisico(bool fromCuadre = false, int almId = -1, bool selectNewAlmacen = false, string Auditor = "",bool isfromsucces = false)
        {

            var parCuadreDiarios = myParametro.GetParCuadresDiarios();

            if (myParametro.GetParCuadres() > 0 && (Arguments.Values.CurrentCuaSecuencia == -1 || (parCuadreDiarios && myCua.GetCuadreAbierto(DateTime.Now.ToString("dd-MM-yyyy")) == null)))
            {

                if (myParametro.GetParAperturarCuadrePorAuditor())
                {
                    await DisplayAlert(AppResource.Warning, "No tienes un cuadre abierto" + (myParametro.GetParCuadresDiarios() ? " para el dia de hoy" : "") + ", debes de abrir un cuadre para poder realizar el conteo.", AppResource.Aceptar);
                    return;
                }

                myCua.AbrirCerrarCuadre(parCuadreDiarios, (isCerrarCuadre, isimprimir) =>
                {
                    if ((isCerrarCuadre && myParametro.GetParSincronizarAlCerrarCuadre()) || (!isCerrarCuadre && myParametro.GetParSincronizarAlAbrirCuadre()))
                    {
                        Sincronizar();
                    }
                });
                return;
            }

            if (myParametro.GetParConteoFisicoPorAlmacen() && almId == -1 || selectNewAlmacen)
            {
                var almacenes = new DS_Almacenes().GetAlmacenesByAlmIDParameter(myParametro.GetParConteoFisicoAlmacenesParaContar());

                if (myParametro.GetParConteoFisicoNoContarAlmacenesEnCero())
                {
                    string almacenesConCantidad = "";
                    var count = 0;
                    var totalAlmid = almacenes.Count();
                    foreach (var almacen in almacenes)
                    {
                        if (myInv.GetInventario(true, almacen.AlmID).Count > 0)
                        {

                            if (count != totalAlmid && count != 0)
                            {
                                almacenesConCantidad += ',';
                            }

                            almacenesConCantidad += almacen.AlmID.ToString();
                            count += 1;
                        }

                    }
                    almacenes = new DS_Almacenes().GetAlmacenesByAlmIDParameter(almacenesConCantidad);
                }
                

                if (isfromsucces)
                {
                    almacenes = almacenes.Where(a => a.AlmID != Arguments.Values.AlmId).ToList();
                }
                Arguments.Values.ClearAlm();
                var buttons = new List<string>();
                listalmid = new List<int>();
                foreach (var almacen in almacenes)
                {
                    if (!myCont.ValidateSiAlmaceneTieneConConteoFisico(almacen.AlmID, Arguments.Values.CurrentCuaSecuencia))
                    {
                        buttons.Add(almacen.AlmDescripcion);
                        listalmid.Add(almacen.AlmID);
                    }

                }

                if (!selectNewAlmacen)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.CountAtEatchWarehouseMessage, AppResource.Aceptar);
                }
                
                var result = await DisplayActionSheet(AppResource.SelectWarehouse, buttons: buttons.ToArray());
                if (string.IsNullOrEmpty(result) && !myCont.ValidateAlmacenesConConteoFisico(myParametro.GetParConteoFisicoAlmacenesParaContar(), Arguments.Values.CurrentCuaSecuencia))
                {
                    PrepareGoConteoFisico(fromCuadre, Auditor: Auditor);
                    return;
                }
                var selectedAlmacen = almacenes.Where(x => x.AlmDescripcion == result).FirstOrDefault();
                if (selectedAlmacen == null)
                {
                    return;
                }

                Arguments.Values.AlmRef = selectedAlmacen.AlmReferencia;
                Arguments.Values.AlmId = selectedAlmacen.AlmID;

                PrepareGoConteoFisico(fromCuadre, selectedAlmacen.AlmID, Auditor: Auditor);
                return;

            }

            if (myParametro.GetParConteoFisicoPorAuditor())
            {
                GoConteoFisico(fromCuadre, Auditor, almId);
                return;

            }

            GoConteoFisico(fromCuadre, almId:almId);
        }
        private async void GoConteoFisico(bool fromCuadre, string RepAuditor = null, int almId = -1)
        {
            try
            {
                IsBusy = true;

                Arguments.Values.CurrentVisSecuencia = -1;
                Arguments.Values.CurrentSector = null;

                Arguments.Values.CurrentModule = Modules.CONTEOSFISICOS;

                Arguments.Values.CurrentClient = new Clientes()
                {
                    CliNombre = Arguments.CurrentUser.RepNombre,
                    CliCodigo = Arguments.CurrentUser.RepCodigo
                };

                if (!inTemp)
                {
                    new DS_Productos().ClearTemp((int)Arguments.Values.CurrentModule);
                }
                
                await PushAsync(new PedidosPage(repAuditor: RepAuditor, almId: almId));
                
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        private async void GoTraspasos(bool reqInventario = false)
        {
            try
            {
                IsBusy = true;

                Arguments.Values.CurrentVisSecuencia = -1;
                Arguments.Values.CurrentSector = null;

                Arguments.Values.CurrentModule = Modules.TRASPASOS;

                if (reqInventario)
                {
                    Arguments.Values.CurrentModule = Modules.REQUISICIONINVENTARIO;

                    if (myParametro.GetParRequisicionInventarioUnaxDia())
                    {
                        var myReq = new DS_RequisicionesInventario();
                        RequisicionesInventario reqlast = myReq.GetRequisicionesInventarioByMaxSecuencia(false);
                        RequisicionesInventario reqlastConfirmado = myReq.GetRequisicionesInventarioByMaxSecuencia(true);

                        RequisicionesInventario lastReq = reqlast != null ? reqlast : reqlastConfirmado;
                        if (reqlast != null && reqlastConfirmado != null)
                        {
                            int reqSecuencia = Convert.ToInt32(lastReq.ReqSecuencia.ToString());
                            if (reqSecuencia < Convert.ToInt32(reqlastConfirmado.ReqSecuencia.ToString()))
                            {
                                lastReq = reqlastConfirmado;
                            }
                        }

                        if (lastReq?.ReqFecha != null && lastReq.ReqEstatus != 0)
                        {
                            DateTime reqFecha = DateTime.Parse(lastReq.ReqFecha);
                            TimeSpan time = reqFecha.Date - DateTime.Now.Date;

                            if (time.Days == 0)
                            {
                                throw new Exception("No está permitido realizar más de una requisición de inventario por dia");
                            }
                        }
                    }
                    
                }

                Arguments.Values.CurrentClient = new Clientes()
                {
                    CliNombre = Arguments.CurrentUser.RepNombre,
                    CliCodigo = Arguments.CurrentUser.RepCodigo
                };

                if (!inTemp)
                {
                    new DS_Productos().ClearTemp((int)Arguments.Values.CurrentModule);
                }

                await PushAsync(new PedidosPage());
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        private void BindMenu()
        {
            var separator = new model.Internal.MenuItem() { Title = AppResource.Processes, Id = 6, SeparatorVisible = true };

            MenuSource = new ObservableCollection<model.Internal.MenuItem>
            {
                new model.Internal.MenuItem() { Title = AppResource.SignOut, Id = 0, Icon = "ic_exit_to_app_black"},
                new model.Internal.MenuItem() { Title = AppResource.PrinterSetup, Id = 1, Icon = "ic_print_black" },
                //new model.Internal.MenuItem() { Title = "Enviar backup", Id = 18, Icon = "baseline_backup_black_24" },
                MenuNoticias,
                new model.Internal.MenuItem() { Title = AppResource.Reports, Id = 3, Icon = "ic_assignment_black" },
                new model.Internal.MenuItem() { Title = AppResource.Transactions, Id = 4, Icon = "ic_chrome_reader_mode_black_24dp" },
                new model.Internal.MenuItem() { Title = AppResource.SyncUp, Id = 5, Icon = "ic_sync_black_24dp" },
                new model.Internal.MenuItem() { Title = "Enviar backup", Id = 18, Icon = "baseline_backup_black_24" },
                separator
            };

            if (myParametro.GetParCambiarClave())
            {
                MenuSource.Insert(2, new model.Internal.MenuItem() { Title = AppResource.ChangeUserPassword, Id = 19, Icon = "baseline_phonelink_lock_black_24" });
            }

            bool somethingAdded = false;

            if (myParametro.GetParCuadres() > 0)
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.OpeningClosingSquares, Id = 7, Icon = "ic_inbox_black_24dp" });
                somethingAdded = true;
            }

            if (myParametro.GetParAsignarRutas())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.RouteAssignment, Id = 20, Icon = "ic_map_black_24dp" });
                somethingAdded = true;
            }

            if (myParametro.GetParCargasInventario())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.InventoryLoads, Id = 8, Icon = "ic_file_download_black_24dp" });
                somethingAdded = true;
            }

            if (myParametro.GetParEncuestas())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.Surveys, Id = 24, Icon = "baseline_list_alt_black_24" });
                somethingAdded = true;
            }

            if (myParametro.GetParHomeBtnConteoFisico())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.PhysicalCount, Id = 21, Icon = "ic_local_shipping_black_24dp" });
                somethingAdded = true;
            }

            if (myParametro.GetParTraspasos())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.Transfers, Id = 22, Icon = "ic_shopping_basket_black_24dp" });
                somethingAdded = true;
            }


            /*if (myParametro.GetParEntregaFacturaCamion())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = "Carga camion", Id = 9, Icon = "ic_local_shipping_black_24dp" });
                somethingAdded = true;
            }*/

            if (myParametro.GetParDepositos())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.Deposits, Id = 10, Icon = "ic_account_balance_black_24dp" });
                somethingAdded = true;
            }

            if (myParametro.GetParDepositosDePushMoney())
            {
                var title = AppResource.PurchaseDeposits;

                if (myParametro.GetParCambiarNombreComprasPorPushMoney())
                {
                    title = AppResource.PushmoneyDeposits;
                }

                MenuSource.Add(new model.Internal.MenuItem() { Title = title, Id = 12, Icon = "ic_shopping_cart_black_24dp" });
                somethingAdded = true;
            }

            if (myParametro.GetParVentas() || myParametro.GetParCargasInventario())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.InventoryInquiry, Id = 11, Icon = "ic_archive_black_24dp" });
                somethingAdded = true;
            }

            if (myParametro.GetParRequisicionesInventario())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.InventoryRequisition, Id = 25, Icon = "ic_add_shopping_cart_black_24dp" });
                somethingAdded = true;
            }

            if (myParametro.GetParOperativosMedicos())
            {
                menusource.Add(new model.Internal.MenuItem() { Title = AppResource.MedicalOperatives, Id = 23, Icon = "ic_folder_black_24dp" });
                somethingAdded = true;
            }

            /*if (myParametro.GetParDepositosCompras())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = "Depósitos Pushmoney", Id = 11, Icon = "ic_add_shopping_cart_black_24dp" });
                somethingAdded = true;
            }*/

            /*if (myParametro.GetParEntregaDocumentosDetalle())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = "Liquidación de entrega", Id = 12, Icon = "ic_receipt_black_24dp" });
                somethingAdded = true;
            }

            if (myParametro.GetParProyectosIngenieria())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = "Proyectos Ingenieria", Id = 13, Icon = "ic_business_black_24dp" });
                somethingAdded = true;
            }*/

            if (myParametro.GetParGastos())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.ExpenseRecord, Id = 14, Icon = "ic_monetization_on_black_24dp" });
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.ExpensesDeposit, Id = 17, Icon = "baseline_how_to_vote_black_24" });
                somethingAdded = true;
            }

            /*if (myParametro.GetParRequisiconInventario())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = "Requisición de inventario", Id = 15, Icon = "ic_shopping_basket_black_24dp" });
                somethingAdded = true;
            }
            */
            if (myParametro.GetParProspectos())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.ProspectRegistration, Id = 16, Icon = "ic_card_membership_black_24dp" });
                somethingAdded = true;
            }

            if (myParametro.GetParConsultarOfertasGenerales())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = "Consultar ofertas generales", Id = 26, Icon = "ic_add_shopping_cart_black_24dp" });
                somethingAdded = true;
            }

            if (!somethingAdded)
            {
                MenuSource.Remove(separator);
            }

        }

        private async void AlertSincronizar()
        {
            var alert = await DisplayAlert(AppResource.SyncUp, AppResource.GoSyncQuestion, AppResource.SyncUp, AppResource.Cancel);

            if (alert)
            {
                Sincronizar();
            }
        }

        private async void Sincronizar(int valcuadr = 0)
        {
            try
            {
                await PushModalAsync(new SincronizarModal(valcuadr, async () =>
                {
                    if (valcuadr > 1)
                    {
                        //Application.Current.MainPage.Navigation.RemovePage(Application.Current.MainPage.Navigation.NavigationStack[2]);
                        var navigations = Application.Current.MainPage.Navigation.NavigationStack;
                        if (navigations.Count > 2)
                        {
                            Application.Current.MainPage.Navigation.RemovePage(navigations[2]);
                        }
                        await PopModalAsync(true);
                    }

                    CargarCantidadNoticias(); 
                
                }));
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.SynchronizationFailed, e.Message, AppResource.Aceptar);
            }
        }

        private async void OnOptionItemSelectedAsync()
        {
            try
            {
                if (SelectedItem == null) return;

                if (DS_Representantes.RepresentateIsInactive(Arguments.CurrentUser.RepCodigo) && (SelectedItem.Id != 5 && SelectedItem.Id != 18 && SelectedItem.Id != 0))
                {
                    await DisplayAlert(AppResource.Warning, AppResource.UserIsInactive, AppResource.Aceptar);
                    return;
                }

                switch (SelectedItem.Id)
                {
                    case 0: //cerrar sesion
                        Arguments.LogOut();
                        await PopAsync(true);
                        break;
                    case 1:  //impresora
                        GoImpresora();
                        break;
                    case 2: //noticias
                        showFromNoticias = true;
                        await PushAsync(new NoticiasPage());
                        break;
                    case 3: //reportes
                        await PushAsync(new ReportesPage());
                        break;
                    case 4: //consulta transacciones
                        await PushAsync(new ConsultaTransaccionesPage());
                        break;
                    case 5: //sincronizar
                        AlertSincronizar();
                        break;
                    case 7: //cuadres

                        var cuadreAbierto = myCua.GetCuadreAbierto();
                        if (cuadreAbierto == null)
                        {
                            if (myParametro.GetParAperturarCuadrePorAuditor())
                            {
                                await PushModalAsync(new LoginAuditorModal((auditor) => { GoAbrirCerrarCuadres(RepAuditor: auditor); }));

                                return;
                            }
                            else
                            {
                                GoAbrirCerrarCuadres();
                            }
                        }
                        else
                        {
                            GoAbrirCerrarCuadres();
                        }

                        break;
                    case 8: //cargas
                        await PushAsync(new CargasPage());
                        break;
                    case 10: //depositos
                        PrepareGoDepositos();
                        break;
                    case 11: //consulta de inventario
                        await PushAsync(new ConsultaInventarioPage());
                        break;
                    case 12: //deposito compras
                        if (!myCom.HayComprasParaDepositar())
                        {
                            var msg = AppResource.NoPurchasesToDeposits;

                            if (myParametro.GetParCambiarNombreComprasPorPushMoney())
                            {
                                msg = AppResource.NoPushMoneyToDeposit;
                            }

                            await DisplayAlert(AppResource.Warning, msg, AppResource.Aceptar);
                            return;
                        }

                        await PushModalAsync(new DepositoComprasModal());
                        break;
                    case 14:
                        await PushAsync(new AgregarGastosModal(new DS_TransaccionesImagenes()));
                        break;
                    case 16: //registro de prospectos
                        await PushAsync(new ProspectosTabPage());
                        break;
                    case 17: //deposito de gastos
                        if (!myGas.HayGastosParaDepositar())
                        {
                            await DisplayAlert(AppResource.Warning, AppResource.NoFeesToDeposits);
                            return;
                        }
                        await PushAsync(new DepositoGastosPage());
                        break;
                    case 18: //enviar backup
                        AlertSendBackupDb();
                        break;
                    case 19: //cambiar contraseña
                        await PushModalAsync(new ChangePasswordModal(LogOut));
                        break;
                    case 20: //asignacion de rutas
                        await PushAsync(new AsignacionRutasPage());
                        break;
                    case 21:

                        await RecoverProductsInTemp();
                        if (!inTemp)
                        {
                            if (myParametro.GetParConteoFisicoPorAuditor())
                            {
                                await PushModalAsync(new LoginAuditorModal((auditor) => { PrepareGoConteoFisico(false, -1, false, auditor); }));

                                return;
                            }
                            else
                            {
                                PrepareGoConteoFisico(false);
                            }
                        }
                        else
                        {
                            GoConteoFisico(true);
                            return;
                        }

                        break;
                    case 22: //traspasos
                        GoTraspasos();
                        break;
                    case 23: //operativos medicos
                        await PushAsync(new OperativosMedicosPage());
                        break;
                    case 24: //encuestas
                        await PushAsync(new EncuestasPage(true));
                        break;
                    case 25://requisicion de inventario
                        GoTraspasos(true);
                        break;
                    case 26://CONSULTA GENERAL DE OFERTAS
                        dialogConsultaOfertas = new ConsultarOfertasModal();
                        dialogConsultaOfertas.LoadOfertas(isGeneral: true);
                        await PushModalAsync(dialogConsultaOfertas);
                        break;
                }

                OnOptionMenuItemSelected?.Invoke();

                SelectedItem = null;

            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }
        }

        private async void GoImpresora()
        {
            var language = PrinterLanguageMB.NULL;
            if(Device.RuntimePlatform == Device.iOS)
            {
                var result = await DisplayActionSheet(AppResource.SelectPrinterType, buttons:new string[] { "CPCL", "ESCPOS" });

                switch (result)
                {
                    case "CPCL":
                        language = PrinterLanguageMB.CPCL;
                        break;
                    case "ESCPOS":
                        language = PrinterLanguageMB.ESCPOS;
                        break;
                }
            }

            await PushAsync(new ImpresorasPage(language));
        }

        private async void LogOut()
        {
            Arguments.LogOut();
            await PopAsync(false);
        }

        public async void PrepareGoDepositos(bool isConteo = false)
        {
            try
            {
                if (!myDep.HayRecibosParaDepositar())
                {
                    await DisplayAlert(AppResource.Warning, AppResource.NoReceiptsToDeposit, AppResource.Aceptar);

                    if (isConteo)
                    {                       
                       GoAbrirCerrarCuadres(true);
                    }
                    
                    return;
                }

                var listaMonedas = myDep.GetMonedasDeLosRecibosAdepositar();

                string CurrentMonCodigo = "";

                if (listaMonedas.Count > 1)
                {
                    var options = new List<string>();

                    foreach (var moneda in listaMonedas)
                    {
                        options.Add(moneda.MonNombre + " - " + moneda.MonSigla);
                    }

                    var result = await DisplayActionSheet(AppResource.SelectCurrencyToDeposit, buttons: options.ToArray());

                    var CurrentMoneda = listaMonedas.Where(x => x.MonNombre + " - " + x.MonSigla == result).FirstOrDefault();

                    if (CurrentMoneda == null)
                    {
                        return;
                    }

                    CurrentMonCodigo = CurrentMoneda.MonCodigo;

                }
                else if(listaMonedas.Count == 1)
                {
                    CurrentMonCodigo = listaMonedas[0].MonCodigo;
                }

                var parSocieda = myParametro.GetParDepositosPorSociedad();

                if (myParametro.GetParDepositoSectores() || parSocieda)
                {
                    await PushModalAsync(new DepositosSectorModal(parSocieda, CurrentMonCodigo) { OnValueSelected = GoDepositosBySector });
                }
                else
                {
                    await PushAsync(new DepositosPage(null, false, CurrentMonCodigo, isFromConteo: isConteo));
                }


            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }
        }

        private async void GoDepositosBySector(string value, string monCodigo = null)
        {
            await PushAsync(new DepositosPage(value, myParametro.GetParDepositosPorSociedad(), monCodigo));
        }

        private async void AlertSendBackupDb()
        {
            try
            {
                var result = await DisplayAlert(AppResource.Warning, AppResource.SentBackupQuestion, AppResource.Sent, AppResource.Cancel);

                if (result)
                {

                    if (!CrossConnectivity.Current.IsConnected)
                    {
                        throw new Exception(AppResource.NoInternetMessage);
                    }

                    IsBusy = true;

                    await new ServicesManager().SubirSqliteDb(Arguments.CurrentUser.RepCodigo, Arguments.CurrentUser.RepClave);
                    //await Task.Run(() => { new ServicesManager().SubirSqliteDb(Arguments.CurrentUser.RepCodigo, Arguments.CurrentUser.RepClave); });
                }

                await DisplayAlert(AppResource.Success, AppResource.BackupSentSuccessfully);

            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        public async void AlertCerrarSesion()
        {
            var result = await DisplayAlert(AppResource.Warning, AppResource.SignOutQuestion, AppResource.SignOut, AppResource.Cancel);

            if (result)
            {
                Arguments.LogOut();
                await PopAsync(true);
            }
        }

        public async void GoAbrirCerrarCuadres(bool forced = false, string RepAuditor = "")
        {      
            var preguntar = true;

            if (myParametro.GetParConteoFisicoVendedor() && !forced)
            {
                var cuadreAbierto = myCua.GetCuadreAbierto();
                
                if (cuadreAbierto != null)
                {

                    var cargasAceptadas = myCar.GetCargasAceptadasByCuaSecuencia(cuadreAbierto.CuaSecuencia);

                    if(cargasAceptadas.Count > 0 || !myParametro.GetParCuadresCerrarValidandoCargaAceptada())
                    {
                    
                        if (myParametro.GetParCuadresValidarEntregasPendienteParaCerrarAntesDelConteo()==1)
                        {
                            var ent = new DS_EntregasRepartidorTransacciones();
                            var entregasPendientes = myParametro.GetParCuadresDiarios() 
                                ? ent.GetEntregasDisponiblesbyFechaInicioCuadre(forVenta: true, cuadres: cuadreAbierto) 
                                : ent.GetEntregasDisponiblesbyCuadre(forVenta: true, cuadres: cuadreAbierto);

                            if (entregasPendientes.Count > 0)
                            {
                                await DisplayAlert(AppResource.Warning, AppResource.PendingDeliveriesMessage, AppResource.Aceptar);
                                await PushModalAsync(new PedidosPendientesEntregarModal(cuadres: cuadreAbierto));
                                return;
                            }
                            else if (myParametro.GetParMoverCantidadesDespachoaDevolucion())
                            {
                               var inventariodespacho = new List<Model.Inventarios>();
                               var inventariodevolucion = new List<Model.Inventarios>();
                               var dsInv = new DS_Inventarios();

                               var almIdDevolucion = myParametro.GetParAlmacenIdParaDevolucion();
                               var almIdDespacho = myParametro.GetParAlmacenIdParaDespacho();

                                inventariodespacho = new DS_Inventarios().GetInventario(almId: almIdDespacho);
                                inventariodevolucion = new DS_Inventarios().GetInventario(almId: almIdDevolucion);
                                

                                foreach (var producto in inventariodespacho)
                                {
                                   if (producto.invCantidad > 0)
                                   {
                                       dsInv.RestarInventario(producto.ProID, producto.invCantidad, (int)producto.InvCantidadDetalle, almIdDespacho);
                                       dsInv.AgregarInventario(producto.ProID, producto.invCantidad, (int)producto.InvCantidadDetalle, almIdDevolucion);
                                   }
                                   
                                }
                            }
                        }
                        else if (myParametro.GetParCuadresValidarEntregasPendienteParaCerrarAntesDelConteo() == 2)
                        {
                            var ent = new DS_EntregasRepartidorTransacciones();
                            var entregasPendientes = myParametro.GetParCuadresDiarios()
                                ? ent.GetEntregasDisponiblesbyFechaInicioCuadre(forVenta: true, isCargaAceptada: true, cuadres: cuadreAbierto)
                                : ent.GetEntregasDisponiblesbyCuadre(forVenta: true, isCargaAceptada: true, cuadres: cuadreAbierto);
                            
                            if (entregasPendientes.Count > 0)
                            {
                                await DisplayAlert(AppResource.Warning, AppResource.PendingDeliveriesMessage, AppResource.Aceptar);
                                await PushModalAsync(new PedidosPendientesEntregarModal(true, cuadres: cuadreAbierto));
                                return;
                            }
                            else if (myParametro.GetParMoverCantidadesDespachoaDevolucion())
                            {
                                var inventariodespacho = new List<Model.Inventarios>();
                                var inventariodevolucion = new List<Model.Inventarios>();
                                var dsInv = new DS_Inventarios();

                                var almIdDevolucion = myParametro.GetParAlmacenIdParaDevolucion();
                                var almIdDespacho = myParametro.GetParAlmacenIdParaDespacho();

                                inventariodespacho = new DS_Inventarios().GetInventario(almId: almIdDespacho);
                                inventariodevolucion = new DS_Inventarios().GetInventario(almId: almIdDevolucion);


                                foreach (var producto in inventariodespacho)
                                {
                                    if (producto.invCantidad > 0)
                                    {
                                        dsInv.RestarInventario(producto.ProID, producto.invCantidad, (int)producto.InvCantidadDetalle, almIdDespacho);
                                        dsInv.AgregarInventario(producto.ProID, producto.invCantidad, (int)producto.InvCantidadDetalle, almIdDevolucion);
                                    }

                                }
                            }
                        }

                        
                        var parValidarClientesAVisitar = myParametro.GetParCuadresValidarClientesVisitadosParaCerrarAntesDelConteo();

                        if (!string.IsNullOrWhiteSpace(parValidarClientesAVisitar) && (parValidarClientesAVisitar == "1" || parValidarClientesAVisitar.Contains("2|")))
                        {
                            var clientesSinVisitar = new DS_RutaVisitas().GetClientesSinVisitar();

                            if (clientesSinVisitar.Count > 0)
                            {
                                if (parValidarClientesAVisitar == "1")
                                {
                                    await Application.Current.MainPage.DisplayAlert(AppResource.Warning, AppResource.ClientsWithoutVisitsWarning, AppResource.Aceptar);
                                    return;
                                }
                                else if (parValidarClientesAVisitar.Contains("2|"))
                                {
                                    var crearVisitasAuto = await Application.Current.MainPage.DisplayAlert(AppResource.Warning, AppResource.CreateAutomaticFailedVisits, AppResource.CreateAutomaticVisits, AppResource.Cancel);

                                    if (crearVisitasAuto)
                                    {
                                        var array = parValidarClientesAVisitar.Split('|');
                                        new DS_Visitas().CrearVisitasFallidas(clientesSinVisitar, array[1], Arguments.Values.CurrentLocation);
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                            }
                        }


                        if (myParametro.GetParConteoFisicoObligatorio() && cuadreAbierto.CuaEstatus == 1)
                        {
                            if (myParametro.GetParUsarMultiAlmacenes())
                            {
                                
                                if (myParametro.GetParConteoFisicoNoContarAlmacenesEnCero())
                                {
                                    var count = 0;
                                    var almacenes = new DS_Almacenes().GetAlmacenesByAlmIDParameter(myParametro.GetParConteoFisicoAlmacenesParaContar());
                                    var totalAlmid = almacenes.Count();
                                    string almacenesConCantidad = "";
                                    foreach (var almacen in almacenes)
                                    {
                                        if (myInv.GetInventario(true, almacen.AlmID).Count > 0)
                                        {
                                            if (count != totalAlmid && count != 0)
                                            {
                                                almacenesConCantidad += ',';
                                            }

                                            almacenesConCantidad += almacen.AlmID.ToString();
                                            count += 1;
                                        }
                                    }

                                    if (myCont.ValidateSiTodosAlmacenesTienenConConteoFisico(almacenesConCantidad, Arguments.Values.CurrentCuaSecuencia) >= new DS_Almacenes().GetAlmacenesByAlmIDParameter(almacenesConCantidad).Count)
                                    {
                                        await DisplayAlert(AppResource.Warning, AppResource.AllWarehouseCountedMessage, AppResource.Aceptar);
                                        GoAbrirCerrarCuadres(true);
                                        return;
                                    }
                                }
                                
                                if (myCont.ValidateSiTodosAlmacenesTienenConConteoFisico(myParametro.GetParConteoFisicoAlmacenesParaContar(), Arguments.Values.CurrentCuaSecuencia) >= new DS_Almacenes().GetAlmacenesByAlmIDParameter(myParametro.GetParConteoFisicoAlmacenesParaContar()).Count)
                                {
                                    await DisplayAlert(AppResource.Warning, AppResource.AllWarehouseCountedMessage, AppResource.Aceptar);
                                    GoAbrirCerrarCuadres(true);
                                    return;
                                }


                            }
                            else
                            {
                                if (myCont.ValidateSiTodosAlmacenesTienenConConteoFisico(myParametro.GetParAlmacenIdParaDespacho().ToString(), Arguments.Values.CurrentCuaSecuencia) >= new DS_Almacenes().GetAlmacenesByAlmIDParameter(myParametro.GetParAlmacenIdParaDespacho().ToString()).Count)
                                {
                                    await DisplayAlert(AppResource.Warning, AppResource.WarehouseHasBeenCounter, AppResource.Aceptar);
                                    GoAbrirCerrarCuadres(true);
                                    return;
                                }
                            }

                            string productosid = myCont.ProductosEnInventarioSinListaDePrecios();
                            if (!string.IsNullOrEmpty(productosid))
                            {
                                await DisplayAlert(AppResource.Warning, AppResource.ProductsWithoutPriceList + "{"+productosid+"} ", AppResource.Aceptar);
                                return;
                            }

                            await DisplayAlert(AppResource.Warning, AppResource.PhysicalCountPendingMessage, AppResource.Aceptar);

                            preguntar = false;
                            await RecoverProductsInTemp();
                            if (!inTemp)
                            {
                                if (myParametro.GetParConteoFisicoPorAuditor())
                                {
                                    await PushModalAsync(new LoginAuditorModal((auditor) => { PrepareGoConteoFisico(true, -1, false, auditor); }));

                                    return;
                                }
                                else
                                {
                                    PrepareGoConteoFisico(true);
                                    return;
                                }
                            }
                            else
                            {
                                GoConteoFisico(true, almId: myProd.AddedInTempForGetAlmid(14));
                                return;
                            }

                        }
                        else {
                            var result = await DisplayAlert(AppResource.Warning, AppResource.DoPhysicalCountBeforeCloseSquare, AppResource.Yes, AppResource.No);

                            preguntar = false;

                            if (result)
                            {
                                if (!inTemp)
                                {
                                    if (myParametro.GetParConteoFisicoPorAuditor())
                                    {
                                        await PushModalAsync(new LoginAuditorModal((auditor) => { PrepareGoConteoFisico(true, -1, false, auditor); }));

                                        return;
                                    }
                                    else
                                    {
                                        PrepareGoConteoFisico(true);
                                        return;
                                    }
                                }
                                else
                                {
                                    GoConteoFisico(true,almId: myProd.AddedInTempForGetAlmid(14));
                                    return;
                                } 
                            }
                        }
                    }
                }
            }

           
            myCua.AbrirCerrarCuadre(false, (isCerrarCuadre, verifImpr) =>
            {
                if(verifImpr == 1)
                {
                    Imprimir(verifImpr);
                }

                if (myParametro.GetParSincronizarAlCerrarCuadre() && verifImpr != 1)
                {
                    Sincronizar(verifImpr);
                }
            }, preguntar, forced, RepAuditor: RepAuditor);
        }

        public new void ListenForLocationUpdatesIfPermission()
        {
            if (myParametro.GetParCuadres() == 2)
            {
                base.ListenForLocationUpdatesIfPermission();
            }
        }

        public async void VerificarVisitaPendienteCierre()
        {
            try
            {
                if (await ExisteReciboHuerfano())
                {
                    return;
                }

                if(await ExisteEntregaHuerfana())
                {
                    return;
                }

                var visitaAbierta = new DS_Visitas().GetVisitaAbierta();

                if (visitaAbierta != null)
                {
                    var cliente = new DS_Clientes().GetClienteById(visitaAbierta.CliID);

                    if (cliente == null)
                    {
                        return;
                    }

                    if (myParametro.GetParGPS())
                    {
                        Functions.StartListeningForLocations();
                    }
                    
                    if (visitaPendienteVerificada)
                    {
                        visitaPendienteVerificada = false;
                        return;
                    }

                    visitaPendienteVerificada = true;               

                    await DisplayAlert(AppResource.Warning, visitaAbierta.VisTipoVisita == 2 ? AppResource.VirtualVisitPendingMessage : AppResource.VisitPendingMessage);
                    
                    IsBusy = true;

                    await LoadValuesForVisit(cliente, visitaAbierta.VisSecuencia);

                    var parSectores = myParametro.GetParSectores();

                    if (parSectores > 2)
                    {
                        await PushAsync(new ElegirSectorPage() { IsFromHome = true });
                    }
                    else
                    {
                        await PushAsync(new OperacionesPage(IsVirtual: visitaAbierta.VisTipoVisita == 2 ? true : false));
                    }
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            visitaPendienteVerificada = false;
            IsBusy = false;
        }

        public void RefreshNewsCount()
        {
            if (showFromNoticias)
            {
                showFromNoticias = false;
                CargarCantidadNoticias();
            }
        }

        private async Task LoadValuesForVisit(Clientes cliente, int visSecuencia)
        {
            await Task.Run(() =>
            {
                Arguments.Values.CurrentClient = cliente;
                Arguments.Values.CurrentVisSecuencia = visSecuencia;

                if (myParametro.GetOfertasConSegmento())
                {
                    new DS_Ofertas().GuardarProductosValidosParaOfertasPorSegmento(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentClient.TiNID);
                }
                else
                {
                    new DS_Ofertas().GuardarProductosValidosParaOfertas(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentClient.TiNID);
                }
            });
        }

        private async Task<bool> ExisteReciboHuerfano()
        {           
            if (new DS_Ventas().ExisteVentaHuerfana(out string monCodigo, out Clientes cliente, out int visSecuencia, out string secCodigo, out int venSecuencia))
            {
                var visitaAbierta = new DS_Visitas().GetVisitaBySecuencia(visSecuencia);

                if (visitaAbierta != null)
                {
                    await DisplayAlert(AppResource.OrphanReceipt, AppResource.SalePendingPaymentMessage, AppResource.Aceptar);
                    await LoadValuesForVisit(cliente, visitaAbierta.VisSecuencia);

                    if(myParametro.GetParSectores() > 0)
                    {
                        var sector = new DS_Sectores().GetSectorByCodigo(secCodigo, cliente.CliID);

                        if(sector != null)
                        {
                            Arguments.Values.CurrentSector = sector;
                        }
                    }
                    
                    Arguments.Values.CurrentModule = Modules.VENTAS;

                    var Moneda = new DS_Monedas().GetMoneda(monCodigo);

                    await PushAsync(new OperacionesPage(sector: Arguments.Values.CurrentSector));
                    await PushAsync(new RecibosTabPage(VenSecuencia:venSecuencia, moneda:Moneda));

                    return true;
                }
            }

            return false;
           
        }

        private async Task<bool> ExisteEntregaHuerfana()
        {
            if (myParametro.GetParEntregasRepartidorGuardarReciboDeContado() && new DS_EntregasRepartidorTransacciones().ExisteEntregaHuerfana(out string monCodigo, out Clientes cliente, out int visSecuencia, out string secCodigo, out List<int> secuencias))
            {
                var visitaAbierta = new DS_Visitas().GetVisitaBySecuencia(visSecuencia);

                await DisplayAlert(AppResource.OrphanReceipt, AppResource.DeliveryPendingPaymentMessage, AppResource.Aceptar);

                if (visitaAbierta != null)
                {
                    await LoadValuesForVisit(cliente, visitaAbierta.VisSecuencia);

                    if (myParametro.GetParSectores() > 0)
                    {
                        var sector = new DS_Sectores().GetSectorByCodigo(secCodigo, cliente.CliID);

                        if (sector != null)
                        {
                            Arguments.Values.CurrentSector = sector;
                        }
                    }

                    Arguments.Values.CurrentModule = Modules.ENTREGASREPARTIDOR;

                    var Moneda = new DS_Monedas().GetMoneda(monCodigo);

                    await PushAsync(new OperacionesPage(sector: Arguments.Values.CurrentSector));
                    await PushAsync(new RecibosTabPage(VenSecuencia:secuencias.Count == 1 ? secuencias[0] : -1, EntregasSecuencias: secuencias, moneda: Moneda));

                    return true;
                }
            }

            return false;

        }

        public void ResetCuaSecuencia()
        {
            if (myParametro.GetParCuadres() > 0)
            {
                var cuadre = myCua.GetCuadreAbierto();

                if (cuadre != null)
                {
                    Arguments.Values.CurrentCuaSecuencia = cuadre.CuaSecuencia;
                }
                else
                {
                    Arguments.Values.CurrentCuaSecuencia = -1;
                }
            }
        }

        private async Task RecoverProductsInTemp()
        {
            inTemp = false;
            if (!myProd.NothingAddedInTemp(14))
            {
                var result = await DisplayAlert(AppResource.Warning, AppResource.ContinuePendingCountMessage, AppResource.Yes, AppResource.No);

                if (!result)
                {
                    inTemp = false;
                    myProd.ClearTemp((int)Arguments.Values.CurrentModule);
                }
                else
                {                    
                    inTemp = true;
                    if(!myParametro.GetParMostrarFacturasVentasEnConteos())
                    {
                        Arguments.Values.ANTSMODULES = Modules.AGAIN;
                    }
                }
            }
        }

        public async void Imprimir(int valcuadr = 0)
        {
            var copias = await DisplayActionSheet(AppResource.ChoosePrinterCopies, buttons: new string[] { "1", "2", "3", "4", "5" });

            int.TryParse(copias, out int intCopias);

            if (intCopias > 0)
            {
                AceptarImpresion(intCopias, valcuadr);
            }

        }

        public async void AceptarImpresion(int Copias, int valcuadr = 0)
        {
            try
            {
                IsBusy = true;

                await Task.Run(() =>
                {
                    printer = new PrinterManager();
                });

                for (int x = 0; x < Copias; x++)
                {
                    if (DS_RepresentantesParametros.GetInstance().GetImpresionSoloFacturaCreditos())
                    {
                        await Task.Run(() =>
                        {
                            Printer.PrintFacturas(Arguments.Values.CurrentCuaSecuencia, false, printer);
                        });

                        IsBusy = false;
                    }
                    else
                    {
                        await Task.Run(() =>
                        {
                            Printer.Print(Arguments.Values.CurrentCuaSecuencia, false, printer);
                        });

                        IsBusy = false;

                        if (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCuadres() == 4)
                        {
                            await DisplayAlert(AppResource.InvoicePrinting, AppResource.CutPapelMessage, AppResource.Print);
                            IsBusy = true;
                            await Task.Run(() =>
                            {
                                Printer.PrintFacturas(Arguments.Values.CurrentCuaSecuencia, false, printer);
                            });
                            IsBusy = false;
                        }
                    }

                    if (Copias > 1 && x != Copias - 1)
                    {
                        await DisplayAlert(AppResource.PrintCopy, AppResource.CutPapelMessage, AppResource.Print);
                    }

                }
                if (myParametro.GetParSincronizarAlCerrarCuadre())
                {
                    Sincronizar(valcuadr);
                }
                IsBusy = false;
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.ErrorPrintingSquare, e.Message, AppResource.Aceptar);
                if (myParametro.GetParSincronizarAlCerrarCuadre())
                {
                    Sincronizar(valcuadr);
                }
            }
            IsBusy = false;
        
        }

        public async void CancelarCargaAceptada(string RepAuditor = "")
        {
            var cuadreAbierto = myCua.GetCuadreAbierto();
            if(cuadreAbierto != null)
            {
                var carga = myCar.GetCargasAceptadasByCuaSecuencia(cuadreAbierto.CuaSecuencia);
                if(carga.Count() > 0)
                {
                    var result = await DisplayAlert(AppResource.Warning, AppResource.WantCancelLoadAcceptedInSquare.Replace("@", carga[0].CarSecuencia.ToString()), AppResource.Yes, AppResource.No);

                    if (result)
                    {
                        bool diferencia = false;
                        var cargadetalle = myCar.GetProductosCarga(carga[0].CarSecuencia);
                        foreach (var producto in cargadetalle)
                        {
                            if(producto.CarCantidad != myInv.GetCantidadTotalInventario(producto.ProID))
                            {
                                diferencia = true;
                            }
                        }
                        
                        if (diferencia)
                        {
                            await DisplayAlert(AppResource.Warning, AppResource.LoadNotMatchInventory, AppResource.Aceptar);
                        }
                        else
                        {
                            var referenciaEntrega = "";
                            if (myParametro.GetParCargasConReferenciaEntrega())
                            {
                                referenciaEntrega = myCar.GetCargaBySecuenciaConRefEntrega(carga[0].CarSecuencia).CarReferenciaEntrega;
                            }

                            myCar.CancelarCarga(carga[0].rowguid.ToString(), referenciaEntrega);
                            if(RepAuditor != "")
                            {
                                myMen.CrearMensaje(-1, AppResource.Load + " " + carga[0].CarSecuencia.ToString() + " "+ AppResource.CanceledByAuditor + " " + RepAuditor.ToString(), -1, carga[0].CarSecuencia, 40);
                            }
                            
                            foreach (var producto in cargadetalle)
                            {
                                myInv.RestarInventario(producto.ProID, producto.CarCantidad, producto.CarCantidadDetalle);
                            }
                            await DisplayAlert(AppResource.Success, AppResource.LoadNumberCanceledSuccessfully.Replace("@", carga[0].CarSecuencia.ToString()));
                        }


                    }
                }
                else
                {
                    await DisplayAlert(AppResource.Warning, AppResource.LoadNotExistsInSquareMessage, AppResource.Aceptar);
                }
                
            }
            else
            {
                await DisplayAlert(AppResource.Warning, AppResource.OpenSquareToContinue, AppResource.Aceptar);
            }
            

        }


    }
}
