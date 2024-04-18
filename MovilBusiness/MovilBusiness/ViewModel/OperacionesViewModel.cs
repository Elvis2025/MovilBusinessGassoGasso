
using Microsoft.AppCenter.Crashes;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.model.Internal.structs;
using MovilBusiness.Model;
using MovilBusiness.Printer;
using MovilBusiness.Printer.Formats;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.views;
using MovilBusiness.Views;
using MovilBusiness.Views.Components.Modals;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.viewmodel
{
    public class OperacionesViewModel : BaseViewModel
    {
        private DS_Visitas myVis;
        private DS_CuentasxCobrar myCxc;
        private DS_AuditoriasMercados myAud;
        private DS_Productos myProd;
        private DS_Mensajes myMen;
        private DS_Clientes myCli;
        private DS_Tareas Mytar;
        private DS_Noticias myNot;
        private DS_EntregasRepartidorTransacciones myEnt;
        private DS_Estudios myEst;
        private DS_Presupuestos myPre;
        private DS_Recibos myRec;

        private bool showvisitavirtual;
        public bool ShowVisitaVirtual { get => showvisitavirtual; set { showvisitavirtual = value; RaiseOnPropertyChanged(); } }

        private bool iscerrarvisita;
        public bool IsCerrarVisita { get => iscerrarvisita; set { iscerrarvisita = value; RaiseOnPropertyChanged(); } }

        private bool isbusyload;
        public bool IsBusyLoad { get => isbusyload; set { isbusyload = value; RaiseOnPropertyChanged(); } }

        public string ProductosModuloDescripcion { get; private set; } = AppResource.Products;

        public ICommand GoModuleCommand { get; private set; }

        private ClientesCreditoData currentclientdata;
        public ClientesCreditoData CurrentClientData { get => currentclientdata; private set { currentclientdata = value; RaiseOnPropertyChanged(); } }

        private ClientesRebateData currentrebatedata;
        public ClientesRebateData CurrentRebateData { get => currentrebatedata; private set { currentrebatedata = value; RaiseOnPropertyChanged(); } }

        public List<UsosMultiples> TiposVisita { get; private set; }
        public List<Sectores> Sectores { get; private set; }

        public Sectores CurrentSector { get => Arguments.Values.CurrentSector; set { Arguments.Values.CurrentSector = value; OnSectorChanged(); RaiseOnPropertyChanged(); } }

        private UsosMultiples currenttipovisita;
        public UsosMultiples CurrentTipoVisita { get => currenttipovisita; set { currenttipovisita = value; OnTipoVisitaChanged(); RaiseOnPropertyChanged(); } }

        public string ComprasTitle { get => myParametro.GetParCambiarNombreComprasPorPushMoney() ? "PushMoney" : AppResource.Purchases; }

        //private bool showvisitavirtual;
        private bool enablecombovisita = true;
        public bool EnableComboTipoVisita { get => enablecombovisita; set { enablecombovisita = value; ConfigTipoVisitaManual(); RaiseOnPropertyChanged(); } }
        public bool ShowListaPrecio { get; set; }
        public bool ShowCondicionPagoVisita { get; set; }
        public string ListaPrecio { get; set; }
        //public bool ComboTipoVisitaEnabled { get; set; } = true;

        public OperacionesParamsArgs Params { get; private set; }
        
        public bool SectoresEnabled { get => Params.ParSectores && Params.parSectorValor < 3; }

        public bool VerDatosCredito { get; set; }
        public bool VerLimiteCredito { get; set; }

        public bool ShowBalance { get; set; }

        public bool ShowBonoRebate { get; set; } = false;
        public bool VerDatosCreditoBalance { get => VerDatosCredito || ShowBalance || VerLimiteCredito; }
        public bool ShowMontoPedidoSugerido { get => Arguments.Values.CurrentClient != null && Arguments.Values.CurrentClient.CliMontoPedidoSugerido > 0.0; }
       
        public string pendingtaskCount;
        public string PendingTaskCount { get => pendingtaskCount; private set { pendingtaskCount = value; RaiseOnPropertyChanged(); } }
        
        public string pendingventasCount;
        public string PendingVentasCount { get => pendingventasCount; private set { pendingventasCount = value; RaiseOnPropertyChanged(); } }
        
        public string pendingentregafacturaCount;
        public string PendingEntregaFacturaCount { get => pendingentregafacturaCount; private set { pendingentregafacturaCount = value; RaiseOnPropertyChanged(); } }

        public string pendingNewsCount;
        public string PendingNewsCount { get => pendingNewsCount; private set { pendingNewsCount = value; RaiseOnPropertyChanged(); } }

        private readonly bool IsConsulting = false;

        private Action<int> VisitaVirtualFinalizada;

        private Action<bool> _IsBusyLoad;

        public VisitasPresentacion CurrentVisitaPresentacion { get; set; }
        private Sectores Sectoranterior = Arguments.Values.CurrentSector;

        public EntregasRepartidorTransacciones CurrentPedidoAEntregar;
        private CondicionesPago currentcondicionpago = null;
        public CondicionesPago CurrentCondicionPago { get => currentcondicionpago; set { currentcondicionpago = value; RaiseOnPropertyChanged(); } }

        private ObservableCollection<CondicionesPago> condicionespago;
        public ObservableCollection<CondicionesPago> CondicionesPago { get => condicionespago; private set { condicionespago = value; RaiseOnPropertyChanged(); } }

        private string condicionesPagoClienteVisita ;
        public string CondicionesPagoClienteVisita { get => condicionesPagoClienteVisita; set { condicionesPagoClienteVisita = value; RaiseOnPropertyChanged(); } }

        private bool showlblchequef;
        public bool ShowlblChequeF { get => showlblchequef; set { showlblchequef = value; RaiseOnPropertyChanged(); } }

        private string _TypeOfVisit;

        private bool isVisitaVirtual = false;

        public bool ShowComboTipoVisita { get; set; } = true;



        public OperacionesViewModel(Page page, bool IsConsulting = false, Sectores sector = null, Action<int> VisitaVirtualFinalizada = null, bool visitavirtual = false, string TypeOfVisit = "", Action<bool> IsBusyLoad = null) : base(page)
        {
            this.IsConsulting = IsConsulting;
            this.VisitaVirtualFinalizada = VisitaVirtualFinalizada;
            _IsBusyLoad = IsBusyLoad;
            _TypeOfVisit = TypeOfVisit;
            ShowComboTipoVisita = !IsConsulting;

            if (myParametro.GetParInitPrinterManager() && SuccessPage.Printer == null && !myParametro.GetParEmpresasBySector())
            {
                InitPrinterManager();
            }            

            if (sector == null)
            {
                Arguments.Values.CurrentSector = null;
            }

            GoModuleCommand = new Command(GoModule);
            myMen = new DS_Mensajes();
            myVis = new DS_Visitas();
            myCxc = new DS_CuentasxCobrar();
            myAud = new DS_AuditoriasMercados();
            myProd = new DS_Productos();
            myCli = new DS_Clientes();
            Mytar = new DS_Tareas();
            myNot = new DS_Noticias();
            myEnt = new DS_EntregasRepartidorTransacciones();
            myEst = new DS_Estudios();
            myPre = new DS_Presupuestos();
            myRec = new DS_Recibos();

            ProductosModuloDescripcion = myParametro.GetParCatalogoProductos() ? AppResource.ProdutsCatalog : AppResource.Products;

            var parAuditoria = myParametro.GetParAuditoriaMercado();

            var parComentarioObligatorio = myParametro.GetParVisitaComentarioObligatorio();
            ShowlblChequeF = myParametro.GetParCobrosMuestralblBalance();

            isVisitaVirtual = myVis.IsVisitaVirtual(Arguments.Values.CurrentVisSecuencia) && !visitavirtual;

            if (myParametro.GetParCalcularSegunTipoVisita() == 2)
            {
                EnableComboTipoVisita = false;
            }
            else
            {
                EnableComboTipoVisita = (myParametro.GetNoVisitaVirtual() ? false : !isVisitaVirtual && !IsConsulting);
            }
           
            ShowListaPrecio = myParametro.GetParShowListaPrecio();

            if (ShowListaPrecio)
            {
                ListaPrecio = new DS_Clientes().GetListaPrecioCliente(Arguments.Values.CurrentClient.CliID);
            }

            LoadStaticData();

            Params = new OperacionesParamsArgs()
            {
                ParCobros = myParametro.GetParCobros(),
                ParPedidos = myParametro.GetParPedidos(),
                ParDevoluciones = myParametro.GetParDevoluciones(),
                ParVentas = myParametro.GetParVentas(),
                ParDatos = myParametro.GetParDatosClienteEnVisita(),
                ParProductos = myParametro.GetParProductosEnVisitas() || myParametro.GetParCatalogoProductos(),
                ParSAC = myParametro.GetParSAC() > 0,
                ParCompras = myParametro.GetParCompras(),
                ParComprasRotacion = myParametro.GetParComprasRotacion(),
                ParProductosProximoVencer = myParametro.GetParProductosProximosAVencer(),
                ParInvFisico = myParametro.GetParInventarioFisico() > 0,
                ParCotizaciones = myParametro.GetParCotizaciones(),
                ParPedidosBackOrder = myParametro.GetParPedidosBackOrder(),
                ParReclamaciones = myParametro.GetParReclamaciones(),
                parSectorValor = myParametro.GetParSectores(),
                ParPresupuestoClientes = myParametro.GetParPresupuestosClientes(),
                ParVisitaFoto = myParametro.GetParVisitasCamara(),
                ParAuditoriaMercado = parAuditoria == 1 || parAuditoria == 2 || parAuditoria == 3,
                ParComentarioVisitas = myParametro.GetParComentarioEnVisita() || parComentarioObligatorio == 1 || parComentarioObligatorio == 2,
                ParEncuestas = myParametro.GetParEncuestas(),
                ParEntregasRepartidor = myParametro.GetParEntregasRepartidor() == 1 || myParametro.GetParEntregasRepartidor() == 3,
                ParTareas = myParametro.GetParTareas(),
                ParRecepcionDevolucion = myParametro.GetParRecepcionDevolucion(),
                ParConduces = (myParametro.GetParConduces() && myParametro.GetParConducesAClientesNombrados() ? !string.IsNullOrWhiteSpace(Arguments.Values.CurrentClient.CliDatosOtros) && Arguments.Values.CurrentClient.CliDatosOtros.ToUpper().Contains("K") : myParametro.GetParConduces()),
                ParCambiosMercancia = myParametro.GetParCambiosMercancia(),
                ParImprimirEntregas = myParametro.GetParImprimirEntregasDeLaVisita(),
                ParCalculadoraNegociacion = myParametro.GetParVentasCalculadoraDeNegociacion(true),
                ParHistoricoFacturas = myParametro.GetParHistoricoFacturasEnOperaciones(),
                ParPushMoneyPorPagar = myParametro.GetParPushMoneyPorPagar(),
                ParVisitaSincronizar = myParametro.GetParSincronizarDentroDeVisita(),
                ParConsultarEntregas = myParametro.GetParConsultarEntregas(),
                ParQuejasServicio = myParametro.GetParQuejasServicio(),
                ParCanastos = myParametro.GetParCanastos(),
                ParPromociones = myParametro.GetParPromociones(),
                ParEntregasMercancias = myParametro.GetParEntregasMercancia(),
                ParConsultaTransacciones = myParametro.GetParVisitasConsultarTransacciones(),
                ParExcluirClientes = myParametro.GetParSolicitudExclusionClientes(),
                ParSacd = myParametro.GetParSolicitudActualizacionClienteDireccion(),
                ParAuditoriaPrecios = myParametro.GetParAuditoriaPrecios(),
                ParConsultaInventarioFisico = myParametro.GetParConsultaInventarioFisico(),
                ParColocacionMercancias = myParametro.GetParColocacionProductos(),

                ParNoticiasOP = myParametro.GetParNoticiasEnOperaciones(),

            };

            ShowVisitaVirtual = myParametro.GetParVisitaVirtual() && !isVisitaVirtual;

            if (Params.ParSectores)
            {
                Application app = Application.Current;

                Sectores = new DS_Sectores().GetSectoresByCliente(
                    Arguments.Values.CurrentClient.CliID);

                if(Sectores != null && Sectores.Count > 0 && sector == null)
                {
                    if (myParametro.GetParSectoresPendientes() && app.Properties.ContainsKey("SecCodigo"))
                    {
                        CurrentSector = Sectores.FirstOrDefault
                            (
                               s => s.SecCodigo == app.Properties["SecCodigo"].ToString()
                            );
                    }
                    else
                    {
                        CurrentSector = Sectores[0];
                    }                    
                }

                if (myParametro.GetParSeleccionarSectorAutomaticamenteSiTieneEntrega() && Sectores != null && (Params.parSectorValor == 1 || Params.parSectorValor == 2))
                {
                    var sectoresConEntregas = myEnt.GetSectoresConEntregas();

                    if(sectoresConEntregas != null && sectoresConEntregas.Count > 0)
                    {
                        if(CurrentSector == null || sectoresConEntregas.Where(x=>x.SecCodigo == CurrentSector.SecCodigo).FirstOrDefault() == null)
                        {
                            CurrentSector = Sectores.Where(x => x.SecCodigo == sectoresConEntregas[0].SecCodigo).FirstOrDefault();
                        }
                    }
                }
            }
            //Cantidad Tareas pendientes
            if(myParametro.GetParTareas())
            {
                PendingTaskCount = Mytar.getTareasPendientes().ToString();
            }

            if (myParametro.GetParVentas())
            {
                PendingVentasCount = myEnt.getPedidosPorEntregar(Arguments.Values.CurrentClient.CliID).ToString();
            }

            if (myParametro.GetParEntregasRepartidor() == 1 || myParametro.GetParEntregasRepartidor() == 3)
            {

                PendingEntregaFacturaCount = myEnt.GetEntregasFacturasDisponibles(Arguments.Values.CurrentClient.CliID, myParametro.GetParEntregasRepartidor() == 3).ToString();
          
            }

            if (myParametro.GetParNoticiasEnOperaciones())
            {
                PendingNewsCount = myNot.GetCantidadNoticiasSinLeer().ToString();
            }

            if (myParametro.GetParVisRebate())
            {
                ShowBonoRebate = true;
                CurrentRebateData = myPre.GetMontosRebate(Arguments.Values.CurrentClient.CliID);
            }

            var parLimitecredito = myParametro.GetParVisitaVerClienteCredito();
            var parverLimiteCreditosolo = myParametro.GetParOcultarLimiteCredito();

            switch (parLimitecredito)
            {
                case 1:
                case 2:
                    VerDatosCredito = true;
                    if (!ShowlblChequeF)
                    {
                        ShowBalance = true;
                    }
                    
                    if (parverLimiteCreditosolo)
                    {
                        VerLimiteCredito = false;

                    }
                    else
                    {
                        VerLimiteCredito = true;
                        
                    }
                    break;
                case 3:
                    if (!ShowlblChequeF)
                    {
                        ShowBalance = true;
                    }
                    break;
            }
             ShowCondicionPagoVisita = myParametro.GetParVisitaVerClienteCondicioPago();
            if (ShowCondicionPagoVisita)
            { 
                var myConP = new DS_CondicionesPago().GetByConId(Arguments.Values.CurrentClient.ConID);
                if (myConP != null)
                {
                    CondicionesPagoClienteVisita = $"{myConP.ConReferencia} - {myConP.ConDescripcion}";
                }
            }

            TiposVisita = new DS_UsosMultiples().GetTipoVisita(_TypeOfVisit);

            if (!EnableComboTipoVisita && string.IsNullOrEmpty(_TypeOfVisit))
            {
                TiposVisita = new List<UsosMultiples>() { new UsosMultiples() { CodigoGrupo = "TIPOVISITA", Descripcion = "Virtual", CodigoUso = "2" } };
            }

            if (myParametro.GetParVisitasTipoVisitas() != null)
            {
                TiposVisita = TiposVisita.Where(t => myParametro.GetParVisitasTipoVisitas().Contains(t.CodigoUso)).ToList();
            }

            if (Arguments.Values.CurrentClient.CliIndicadorPresentacion)
            {
                CurrentVisitaPresentacion = myVis.GetClientePresentacion(Arguments.Values.CurrentVisSecuencia);
            }
        }

        private void LoadStaticData()
        {
            Arguments.Values.CurrentClient.CliMontoPedidoSugerido = myCli.GetMontoPedidoSugerido(Arguments.Values.CurrentClient.CliID);
            var monCodigo = myCli.GetMonCodigoByClienteRelacion(Arguments.Values.CurrentClient.CliID);
            if (!string.IsNullOrWhiteSpace(monCodigo))
            {
                Arguments.Values.CurrentClient.MonCodigo = monCodigo;
            }
            Arguments.Values.CurrentClient.CliIndicadorCredito = myCli.GetCliIndicadorCredito(Arguments.Values.CurrentClient.CliID);
            Arguments.Values.CurrentClient.Cliente_Balance = myCxc.GetBalanceTotalByCliid(Arguments.Values.CurrentClient.CliID
                , WithChD: !myParametro.GetParNoTomarEnCuentaChequesDiferidos());


            if (myParametro.GetParConvertirBalanceADolares())
            {
                var monedas = new DS_Monedas().GetMonedas("USD");
                double Balance_Dolares = myCxc.GetBalanceTotalByCliid(Arguments.Values.CurrentClient.CliID
                , monCodigo: "USD", WithChD: !myParametro.GetParNoTomarEnCuentaChequesDiferidos());

                double Balance_Pesos = myCxc.GetBalanceTotalByCliid(Arguments.Values.CurrentClient.CliID
                    , monCodigo: "RD$", WithChD: !myParametro.GetParNoTomarEnCuentaChequesDiferidos());

                Arguments.Values.CurrentClient.Cliente_Balance = (Balance_Dolares + (Balance_Pesos / monedas[0].MonTasa));
            }
            

            if (Arguments.CurrentUser.TipoRelacionClientes == 2)
            {
                var detalle = myCli.GetDetalleFromCliente(Arguments.Values.CurrentClient.CliID, Arguments.CurrentUser.RepCodigo.Trim());

                if (detalle != null)
                {
                    if (!string.IsNullOrWhiteSpace(detalle.LipCodigo))
                    {
                        Arguments.Values.CurrentClient.LiPCodigo = detalle.LipCodigo;
                    }
                    if(detalle.ConID > 0)
                    Arguments.Values.CurrentClient.ConID = detalle.ConID;
                }
            }
        }

        public void LoadClientData()
        {
            if(Arguments.Values.CurrentClient == null)
            {
                return;
            }

            CurrentClientData = myCxc.GetDatosCreditoCliente(Arguments.Values.CurrentClient.CliID);
            Arguments.Values.CurrentClient.Cliente_Balance = CurrentClientData.Balance;

            string tiposYCategorias;

            switch (myParametro.GetParClientesMostrarCategoriasYTiposEnOperaciones())
            {
                case 1:

                     tiposYCategorias = myCli.GetTiposYCategoriasByCliente(Arguments.Values.CurrentClient.CliID, 1);

                    if (!string.IsNullOrWhiteSpace(tiposYCategorias))
                    {
                        DisplayAlert("Tipos y categorias", tiposYCategorias);
                    }
                    break;
                case 2:

                     tiposYCategorias = myCli.GetTiposYCategoriasByCliente(Arguments.Values.CurrentClient.CliID, 2);

                    if (!string.IsNullOrWhiteSpace(tiposYCategorias))
                    {
                        DisplayAlert("Estatus del Cliente en SAP", tiposYCategorias);
                    }
                    break;
            }
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
                        await DisplayActionSheet(AppResource.AutomaticallyAppliedLoads, "Aceptar", null, cargasAceptadas.ToArray());
                    }
                }

            }
        }

        private void OnSectorChanged()
        {
            if(CurrentSector == null)
            {
                return;
            }

            if (myParametro.GetParLipCodigoClientePorSector())
            {
                Arguments.Values.CurrentClient.LiPCodigo = CurrentSector.LipCodigo;
            }

            if (myParametro.GetParCliEstatusPorSector())
            {
                Arguments.Values.CurrentClient.CliEstatus = CurrentSector.estatus;
            }

            CurrentClientData = myCxc.GetDatosCreditoCliente(Arguments.Values.CurrentClient.CliID);
            Arguments.Values.CurrentClient.MonCodigo = CurrentSector.MonCodigo;
            if(Arguments.Values.CurrentClient.MonCodigo == null)
            {
                Arguments.Values.CurrentClient.MonCodigo = "";
            }

            Application app = Application.Current;
            Application.Current.Properties["SecCodigo"] = CurrentSector.SecCodigo;
            app.SavePropertiesAsync();
        }

        private async Task RecoverProductsInTemp()
        {
            if (!myProd.NothingAddedInTemp((int)Arguments.Values.CurrentModule))
            {
                var result = await DisplayAlert(AppResource.Warning, AppResource.RestoreTemporaryProductsQuestion, AppResource.Yes, AppResource.No);

                if (!result)
                {
                    myProd.ClearTemp((int)Arguments.Values.CurrentModule);
                }
            }
        }

        public double DegreeToRadian(double angle)
        {
            var Radians = Math.PI * angle / 180.0;
            return Radians;
        }
        private async void GoModule(object id)
        {
            try
            {
                if (id == null || IsBusy)
                {
                    return;
                }
                    
                IsBusy = true;                

                if (IsConsulting && id.ToString() != "4" && id.ToString() != "12" && id.ToString() != "10" && id.ToString() != "28" && id.ToString() != "19")
                {
                    throw new Exception(AppResource.ConsultationModeVisitMessage);
                }

                if(Params.ParSectores && CurrentSector == null)
                {
                    throw new Exception(AppResource.MustSelectSector);
                }

                if(CurrentTipoVisita != null && CurrentTipoVisita.CodigoUso  == "4" && id.ToString() != "6" && myParametro.GetParSoloComentarioEnTipoVisitaNoVisitado())
                {
                    throw new Exception(AppResource.VisitTypeOnlyAllowComment);
                }
                bool clientBlock = Arguments.Values.CurrentClient.CliEstatus == 0 || Params.ParSectores && CurrentSector.estatus == 0;
                bool ProspectoBlock = Arguments.Values.CurrentClient.CliEstatus != 1 && myParametro.GetParNoVenForProspecto();

                if(Arguments.Values.CurrentClient.CliIndicadorPresentacion && CurrentVisitaPresentacion == null && id.ToString() != "")
                {
                    throw new Exception(AppResource.MustCompletePresentationCustomerData);
                }

                Arguments.Values.CurrentTipoVisita = -1;

                if (CurrentTipoVisita != null)
                {
                    Arguments.Values.CurrentTipoVisita = int.Parse(CurrentTipoVisita.CodigoUso);
                }

                Arguments.Values.IsPushMoneyRotacion = false;

                var parSac = myParametro.GetParSAC();
                var haySacEnLaVisita = false;

                if(parSac == 2 || parSac == 3)
                {
                    haySacEnLaVisita = myCli.GetSACinVisita(Arguments.Values.CurrentVisSecuencia);
                }

                var validarSac = false;

                var idString = id.ToString();
                var modules = new string[] { "1", "2", "3", "4", "5", "7", "20", "23", "24", "25", "29", "32", "35", "36" };
                if((idString == "8" && (parSac == 2 || parSac == 3) || (parSac == 3 && (modules.Contains(idString)))))
                {
                    validarSac = true;
                }

                if (validarSac && Arguments.Values.CurrentClient.CliLatitud == 0 && Arguments.Values.CurrentClient.CliLongitud == 0)
                {
                    if (!haySacEnLaVisita)
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.MustCompleteCustomerGeoreferenceToContinue, AppResource.Aceptar);
                        IsBusy = false;
                        return;
                    }
                }

                if (myParametro.GetParTransaccionesAlertaConfirmarCliente()
                    && id.ToString() != "12" && id.ToString() != "18" && id.ToString() != "19"
                    && id.ToString() != "10" && id.ToString() != "22" && id.ToString() != "27" 
                    && id.ToString() != "28" && id.ToString() != "26" && id.ToString() != "30"
                    && id.ToString() != "31" && id.ToString() != "37" && id.ToString() != "41")
                {
                    var keep = await DisplayAlert(AppResource.Warning, AppResource.TransactionWillBeCreatedInTheNameOf + Arguments.Values.CurrentClient.CliNombreCompleto, AppResource.Continue, AppResource.Cancel);

                    if (!keep)
                    {
                        IsBusy = false;
                        return;
                    }
                }

                switch (id.ToString())
                {
                    case "1": //pedidos                      
                        Arguments.Values.CurrentModule = Modules.PEDIDOS;
                        await RecoverProductsInTemp();
                        GoPedidos(clientBlock, ProspectoBlock);
                        break;
                    case "2": //cotizaciones
                        Arguments.Values.CurrentModule = Modules.COTIZACIONES;
                        await RecoverProductsInTemp();
                        GoCotizaciones(clientBlock);
                        break;
                    case "3": //devoluciones
                        //if (clientBlock)
                        //{
                        //    throw new Exception("No puedes hacer devoluciones a este cliente porque esta bloqueado");
                        //}
                        Arguments.Values.CurrentModule = Modules.DEVOLUCIONES;
                        await RecoverProductsInTemp();
                        await PushAsync(new PedidosPage());
                        break;
                    case "4": //cobros
                        Arguments.Values.CurrentModule = Modules.COBROS;
                        myRec.EliminarTodasFormaPagoInTemp();
                        await PushAsync(new CobrosPage(IsConsulting));
                        break;
                    case "5": //compras
                        Arguments.Values.CurrentModule = Modules.COMPRAS;

                        await RecoverProductsInTemp();
                        myRec.EliminarTodasFormaPagoInTemp();
                        await PushAsync(new PedidosPage());
                        break;
                    case "6": //comentarios
                        Arguments.Values.CurrentModule = Modules.VISITAS;
                        await PushAsync(new ComentariosPage(Arguments.Values.CurrentVisSecuencia));
                        break;
                    case "7": //inventario fisico
                        Arguments.Values.CurrentModule = Modules.INVFISICO;
                        await RecoverProductsInTemp();
                        GoInventarioFisico(clientBlock);
                        break;
                    case "8": //ventas
                        GoVentas(clientBlock, ProspectoBlock);
                        break;
                    case "9": //sac
                        Arguments.Values.CurrentModule = Modules.SAC;
                        await PushAsync(new SacPage(Arguments.Values.CurrentClient));
                        break;
                    case "10": //presupuestos
                        await PushAsync(new PresupuestosPage(Arguments.Values.CurrentClient.CliID));
                        break;
                    case "11":
                        Arguments.Values.CurrentModule = Modules.PRODUCTOS;
                        if (myParametro.GetParCatalogoProductos())
                        {
                            await PushAsync(new CatalogoProductosPage());
                        }
                        else
                        {
                            await PushAsync(new ProductosPage(Arguments.Values.CurrentClient));
                        }

                        break;
                    case "12"://datos
                        await PushAsync(new InfoClientePage(Arguments.Values.CurrentClient));
                        break;
                    case "13"://encuestas
                        await PushAsync(new EncuestasPage(false));
                        break;
                    case "15": //auditorias mercados
                        /*if (myAud.HasProductsInTemp())
                        {
                            var result = await DisplayAlert(AppResource.Warning, "Existe una auditoria pendiente, deseas seguirla o empezar una nueva?", "Resumir", "Empezar nueva");

                            if (!result)
                            {
                                myAud.DeleteTemp();
                            }
                        }*/
                        myAud.DeleteTemp();

                        Arguments.Values.CurrentModule = Modules.AUDITORIAMERCADO;
                        await PushAsync(new AuditoriaMercadoPage());
                        break;
                    case "16": // camara
                        await PushAsync(new CameraPage(Arguments.Values.CurrentVisSecuencia.ToString(), "Visitas"));
                        break;
                    case "17": //reclamaciones
                        await PushAsync(new ReclamacionesPage());
                        break;
                    case "18"://pedidos back order
                        await PushModalAsync(new PedidosBackOrderModal(Arguments.Values.CurrentClient.CliID));
                        break;
                    case "19": //productos proximo a vencer
                        await PushAsync(new ProductosProximoVencerPage(Arguments.Values.CurrentClient.CliID));
                        break;
                    case "20": //entregas facturas repartidor
                        if (myParametro.GetParUsarMultiAlmacenes() && myParametro.GetParAlmacenIdParaDespacho() == -1)
                        {
                            throw new Exception(AppResource.DispatchWarehouseNotDefined);
                        }
                        Arguments.Values.CurrentModule = Modules.ENTREGASREPARTIDOR;
                        await PushAsync(new EntregasRepartidorPage());
                        break;
                    case "21": //visita virtual
                        //Sectoranterior = Arguments.Values.CurrentSector;
                        await PushModalAsync(new SeleccionarClienteVisitaVirtual(async () =>
                        {
                            await PushAsync(new OperacionesPage(false, null, LoadDatosVisita, IsVirtual:true));
                            await new PreferenceManager().Save("ISVISITA", true);
                        }));
                        break;
                    case "22": //TAREAS                       
                        if (PendingTaskCount == "0")
                        { await DisplayAlert(AppResource.Warning, AppResource.NoAssignedTasks); }
                        else { await PushModalAsync(new TareasPage()); }
                        break;
                    case "23": //recepcion de devolucion
                        Arguments.Values.CurrentModule = Modules.RECEPCIONDEVOLUCION;
                        await PushAsync(new EntregasRepartidorPage());
                        break;
                    case "24": //conduces
                        IsBusy = false;
                        GoConduces();
                        break;
                    case "25": //Cambios Mercancia
                        Arguments.Values.CurrentModule = Modules.CAMBIOSMERCANCIA;
                        await RecoverProductsInTemp();
                        GoCambiosMercancia(clientBlock);
                        break;
                    case "26": //imprimir entregas
                        IsBusy = false;
                        ImprimirEntregas();
                        break;
                    case "27": ///Calculadora de Negociacion
                        await PushModalAsync(new PedidosEntregarModal((pedido) =>
                        {
                            try
                            {
                                CurrentPedidoAEntregar = pedido;

                                if (CondicionesPago != null)
                                {

                                    var item = CondicionesPago.Where(x => x.ConID == pedido.ConID).FirstOrDefault();

                                    CurrentCondicionPago = item;
                                }

                                IsBusy = false;

                                PushModalAsync(new CalculadoraNegociacionModal(myProd, CurrentPedidoAEntregar != null ? CurrentPedidoAEntregar.ConID : CurrentCondicionPago != null ? CurrentCondicionPago.ConID : -1, false));

                            }
                            catch (Exception e)
                            {
                                DisplayAlert(AppResource.Warning, e.Message);
                            }
                        }
                        ));
                        break;
                    case "28": //historico facturas
                        await PushAsync(new HistoricoFacturasPage(false));
                        break;
                    case "29"://pushmoney por pagar
                        Arguments.Values.CurrentModule = Modules.PUSHMONEYPORPAGAR;
                        myRec.EliminarTodasFormaPagoInTemp();
                        await PushAsync(new PushMoneyPorPagarPage());
                        break;
                    case "30"://sincronizar
                        ShowAlertSincronizar();
                        break;
                    case "31": //consultar entregas
                        Arguments.Values.CurrentModule = Modules.ENTREGASREPARTIDOR;
                        await PushAsync(new EntregasRepartidorPage(true));
                        break;
                    case "32": //push money rotacion
                        Arguments.Values.CurrentModule = Modules.COMPRAS;
                        Arguments.Values.IsPushMoneyRotacion = true;
                        await RecoverProductsInTemp();
                        await PushAsync(new PedidosPage(IsPushMoneyRotacion: true));
                        break;
                    case "33": //quejas al servicio
                        await PushAsync(new QuejasServicioPage());
                        break;
                    case "34": //canastos
                        await PushModalAsync(new CanastosModal());
                        break;
                    case "35": //promociones
                        Arguments.Values.CurrentModule = Modules.PROMOCIONES;
                        await RecoverProductsInTemp();
                        await PushAsync(new PedidosPage());
                        break;
                    case "36": //Entregas mercancias
                        if (!ValidarValoresParaVenta(true))
                        {
                            IsBusy = false;
                            return;
                        }
                        Arguments.Values.CurrentModule = Modules.ENTREGASMERCANCIA;
                        await RecoverProductsInTemp();
                        await PushAsync(new PedidosPage());
                        break;
                    case "37": //consultar transacciones
                        await PushAsync(new ConsultaTransaccionesPage(Arguments.Values.CurrentClient.CliID));
                        break;
                    case "38": //excluir cliente
                        await PushModalAsync(new ExcluirClienteModal(Arguments.Values.CurrentClient));
                        break;
                    case "39": //solicitud actualizacion cliente direccion
                        await PushAsync(new SACDPage());
                        break;
                    case "40": //auditoria de precios
                        Arguments.Values.CurrentModule = Modules.AUDITORIAPRECIOS;

                        await RecoverProductsInTemp();

                        await PushAsync(new AuditoriaPreciosPage());
                        break;
                    case "41": //consulta inventario fisico
                        await PushAsync(new ConsultaInventarioFisicoPage());
                        break;
                    case "42": //colocacion de mercancias
                        if (clientBlock)
                        {
                            throw new Exception(AppResource.CannotMakePlaceMerchandisesClientBlocked);
                        }
                        Arguments.Values.CurrentModule = Modules.COLOCACIONMERCANCIAS;

                        await RecoverProductsInTemp();

                        await PushAsync(new PedidosPage());
                        break;
                    case "43": //NOTICIAS

                        await PushAsync(new NoticiasPage());

                        break;
                }

            }catch(Exception e)
            {
                Crashes.TrackError(e);
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        private async void GoVentas(bool clientBlock, bool ProspectoBlock, bool authorized = false)
        {   
            if (clientBlock)
            {
                throw new Exception(AppResource.CannotMakeSalesCustomerBlocked);
            }

            if (ProspectoBlock)
            {
                throw new Exception(AppResource.CannotMakeSalesCustomerIsNotActive);
            }

            if (string.IsNullOrWhiteSpace(Arguments.Values.CurrentClient.LiPCodigo) && !myParametro.GetParNoListaPrecios())
            {
                throw new Exception(AppResource.CannotMakeSalesCustomerHasNotPriceList);
            }

            if (!ValidarValoresParaVenta(false))
            {
                IsBusy = false;
                return;
            }            

            if (myParametro.GetParCargasAplicacionAutomaticas() && !authorized)
            {
                AplicarCargasAutomaticas();
            }

            bool isValidDatosOtros = !string.IsNullOrEmpty(Arguments.Values.CurrentClient.CliDatosOtros);

            

            bool isBlockInVentas = isValidDatosOtros && Arguments.Values.CurrentClient.CliDatosOtros.ToUpper().Contains("S");
            if (isBlockInVentas)
            {
                await DisplayAlert(AppResource.Warning, AppResource.CustomerBlockedForSales);
                IsBusy = false;
                return;
            }


            bool isAcuerdoPago = isValidDatosOtros && Arguments.Values.CurrentClient.CliDatosOtros.ToUpper().Contains("M");
            bool isnotvalidtakelimit = isValidDatosOtros && !Arguments.Values.CurrentClient.CliDatosOtros.Contains("L");

            bool limit = !isnotvalidtakelimit && Arguments.Values.CurrentClient.CliLimiteCredito <= 0 && !new DS_CondicionesPago().GetIfIsContado(Arguments.Values.CurrentClient.ConID);
            bool isvalidForValidFac = myCxc.GetForValidFac();
            if (!isAcuerdoPago && myParametro.GetParValidarVenSiTieneCredito(true) && (limit || isvalidForValidFac))
            {
                throw new Exception(isvalidForValidFac ?
                      AppResource.CustomerHasOverdueInvoicesMustPayToSale : AppResource.CustomerHasNoCreditLimit);
            }


            if (!isnotvalidtakelimit && myParametro.GetNoVentaFacturapendiente() > 0 && !Arguments.Values.CurrentClient.CliIndicadorCredito)
            {
                var facturasAPagar = myCxc.GetAllCuentasPendientesByCliente(Arguments.Values.CurrentClient.CliID);
                if (facturasAPagar.Count > myParametro.GetNoVentaFacturapendiente())
                {
                    await DisplayAlert(AppResource.Warning, AppResource.CustomerHasPendingInvoicesCannotSale.Replace("@", facturasAPagar.Count.ToString()));
                    IsBusy = false;
                    return;
                }

            }

            /* if (!myCli.IsRncValido(Arguments.Values.CurrentClient) && !string.IsNullOrWhiteSpace(Arguments.Values.CurrentClient.CliRNC))
             {
                 throw new Exception("Este cliente no tiene RNC/Cedula valido");
             }*/

            if (myParametro.GetParUsarMultiAlmacenes() && (myParametro.GetParEntregasRepartidor() == 1 || myParametro.GetParEntregasRepartidor() == 3))
            {
                if (myParametro.GetParAlmacenIdParaDespacho() == -1)
                {
                    throw new Exception(AppResource.DispatchWarehouseNotDefined);
                }

                if (myParametro.GetParAlmacenIdParaDevolucion() == -1 && !myParametro.GetParNoDevolucionEnEntrega())
                {
                    throw new Exception(AppResource.ReturnWarehouseNotDefined);
                }
            }

            if (myParametro.GetParVentasConReciboObligatorio())
            {

                var parPagoMinimo = myParametro.GetParVentasPorcientoBalancePagoMinimo();
                double Recibosaplicados = 0.00;
                if ((parPagoMinimo > 0) || myParametro.GetParVentasCalculadoraDeNegociacion(false))
                {

                    DS_CuentasxCobrar myCxC = new DS_CuentasxCobrar();
                    List<CuentasxCobrar> Documentos = myCxC.GetAllCuentasByCliente(Arguments.Values.CurrentClient.CliID);
                    foreach (var cxc in Documentos)
                    {
                        if (cxc.CxcSIGLA == "RCB")
                        {
                            Recibosaplicados = cxc.CxcMontoTotal;
                        }
                    }

                    if (Recibosaplicados == 0.00 && Documentos.Count != 0)
                    {
                        throw new Exception(AppResource.NoCollectionMakeToSale);
                    }
                }
            }

            if (myParametro.GetNumberChkDPermit() > 0)
            {
                var CantidadChkD = myCxc.GetCountChequesDevueltos(Arguments.Values.CurrentClient.CliID);
                if (CantidadChkD >= 3)
                {
                    await DisplayActionSheet(AppResource.Warning, AppResource.CustomerHaveBouncedChecksCannotSale.Replace("@", CantidadChkD.ToString()));
                    IsBusy = false;
                    return;
                }

            }

            if (myParametro.GetParBloqueVentasLimiteDecreditoFacturasVencidas() && !Arguments.Values.CurrentClient.CliIndicadorCredito && !authorized)
            {
                var CantFacturasPendites = myCxc.GetFacturasVencidad(Arguments.Values.CurrentClient.CliID);
                var list = new List<string>
                {
                    AppResource.Days + "   " + AppResource.Invoices
                };

                foreach (var facturasVencidas in CantFacturasPendites)
                {
                    double balance = facturasVencidas.CxcBalance;
                    string cxcReferencia = facturasVencidas.CxcReferencia;
                    double aplicaciones = myCxc.getTotalAplicadoByReferencia(cxcReferencia);
                    double restante = balance - aplicaciones;

                    if (restante > 0.01 && facturasVencidas.CxcDias < 0)
                    {
                        list.Add(facturasVencidas.CxcDias + " - " + cxcReferencia);
                    }
                }


                if (list.Count >= 2)
                {
                    string btnDestruction = null;

                    var parAuthorize = myParametro.GetParVentasAutorizarVentaConFacturasVencidas();

                    if (parAuthorize)
                    {
                        btnDestruction = AppResource.Authorize;
                    }

                    //"Este cliente tiene " + list.ToArray() + " facturas pendientes.\nNo puede realizar la venta.
                    var result = await DisplayActionSheet(AppResource.CannotMakeSalesOverdueInvoices, AppResource.Cancel, destruction: btnDestruction, buttons: list.ToArray());

                    if (parAuthorize && result == AppResource.Authorize)
                    {
                        await Application.Current.MainPage.Navigation.PushModalAsync(new AutorizacionesModal(false, 0, 4, "")
                        {
                            OnAutorizacionUsed = (autSec) =>
                            {
                                GoVentas(clientBlock, ProspectoBlock, true);
                            }
                        });
                    }

                    IsBusy = false;
                    return;
                }

            }

            Arguments.Values.CurrentModule = Modules.VENTAS;

            await RecoverProductsInTemp();

            await PushAsync(new PedidosPage());
        }

        private bool ValidarValoresParaVenta(bool validarFacturasVencidas)
        {
            if (!DS_Representantes.HasNCF(Arguments.CurrentUser.RepCodigo))
            {
                throw new Exception(AppResource.UserHasNotReceiptTypeDefined);
            }

            if (!myCli.ClienteTieneNcfValido(Arguments.Values.CurrentClient))
            {
                throw new Exception(AppResource.CustomerHasNotValidReceiptType);
            }

            if (validarFacturasVencidas && myParametro.GetParBloqueVentasLimiteDecreditoFacturasVencidas() && !Arguments.Values.CurrentClient.CliIndicadorCredito)
            {
                var CantFacturasPendites = myCxc.GetFacturasVencidad(Arguments.Values.CurrentClient.CliID);
                var list = new List<string>();
                list.Add(AppResource.Days + "   " + AppResource.Invoices);
                foreach (var facturasVencidas in CantFacturasPendites)
                {
                    double balance = facturasVencidas.CxcBalance;
                    string cxcReferencia = facturasVencidas.CxcReferencia;
                    double aplicaciones = myCxc.getTotalAplicadoByReferencia(cxcReferencia);
                    double restante = balance - aplicaciones;

                    if (restante > 0.01 && facturasVencidas.CxcDias < 0)
                    {
                        list.Add(facturasVencidas.CxcDias + " - " + cxcReferencia);
                    }
                }


                if (list.Count >= 2)
                {
                    //"Este cliente tiene " + list.ToArray() + " facturas pendientes.\nNo puede realizar la venta.
                    DisplayActionSheet(AppResource.CannotMakeSalesOverdueInvoices, buttons: list.ToArray());
                    IsBusy = false;
                    return false;
                }

            }

            return true;
        }


        private async void GoPedidos(bool clientBlock = false, bool ProspectoBlock = false, bool isLimitAutorize = false, bool isFacVenAutorize= false)
        {

            if (clientBlock)
            {
                bool result = await DisplayAlert(AppResource.Warning, AppResource.CannotOrderCustomerBlocked, AppResource.Authorize, AppResource.Cancel);
                if (result)
                {
                    await Application.Current.MainPage.Navigation.PushModalAsync(new AutorizacionesModal(false, 0, 1, "")
                    {
                        OnAutorizacionUsed = (autSec) =>
                        {
                            GoPedidos(false);
                        }
                    });
                }
                IsBusy = false;
                return;
            }

            if (ProspectoBlock)
            {
                bool result = await DisplayAlert(AppResource.Warning, AppResource.CannotOrderCustomerNotActive, AppResource.Authorize, AppResource.Cancel);
                if (result)
                {
                    await Application.Current.MainPage.Navigation.PushModalAsync(new AutorizacionesModal(false, 0, 1, "")
                    {
                        OnAutorizacionUsed = (autSec) =>
                        {
                            GoPedidos(clientBlock,false);
                        }
                    });
                }
                IsBusy = false;
                return;
            }

            Arguments.Values.CurrentModule = Modules.PEDIDOS;
            var myPed = new DS_Pedidos();
            var pedlast = myPed.GetPedidosByMaxSecuencia();

            if (myParametro.GetParPedidosMultiClientes() == 0 && pedlast != null)
            {
                var pedFecha = pedlast.PedFecha;

                if (pedFecha != null && pedlast.PedEstatus != 0 && pedlast.PedEstatus != 3)
                {
                    DateTime oDate = DateTime.Parse(pedFecha);
                    TimeSpan time = oDate - DateTime.Now;

                    if (time.Days == 0)
                    {
                        throw new Exception(AppResource.NotAllowedMakeMoreThanOneOrderPerDay);
                    }
                }

            }

            bool limite = Arguments.Values.CurrentClient.CliLimiteCredito <= 0;
            if (myParametro.LimiteCreditoEnPedidos() == 3 && limite && !isLimitAutorize)
            {
                bool result = await DisplayAlert(AppResource.CantOrder, AppResource.CustomerHasNoCreditLimit, AppResource.Authorize, AppResource.Cancel);
                if (result)
                {
                    await Application.Current.MainPage.Navigation.PushModalAsync(new AutorizacionesModal(false, 0, 1, "")
                    {
                        OnAutorizacionUsed =  (autSec) =>
                        {
                            //Retorna en true para evaluar las demas autorizaciones
                            GoPedidos(clientBlock,ProspectoBlock,true);
                        }
                    });
                }
                IsBusy = false;
                return;
            }

            if (myParametro.GetParValidarPedidoSiTieneFacturaVencida() && myCxc.GetForValidFac() && !isFacVenAutorize)
            {
                bool result = await DisplayAlert(AppResource.CantOrder, AppResource.CustomerHasOverdueInvoicesMustPay, AppResource.Authorize, AppResource.Cancel);
                if (result)
                {
                    await Application.Current.MainPage.Navigation.PushModalAsync(new AutorizacionesModal(false, 0, 1, "")
                    {
                        OnAutorizacionUsed =  (autSec) =>
                        {
                            //Retorna en true para evaluar las demas autorizaciones
                            GoPedidos(clientBlock, ProspectoBlock, isLimitAutorize, true);
                        }
                    });
                }
                IsBusy = false;
                return;
            }

            if (myParametro.GetParPedidosAutorizarChequediferido())
            {
                var myRec = new DS_Recibos();
                bool ExistenChequesDiferidos = myRec.GetChequesDiferidosSinDespositar();

                if (ExistenChequesDiferidos)
                {
                    bool result = await DisplayAlert(AppResource.CantOrder, AppResource.CannotOrderWithFuturistChecks, AppResource.Authorize, AppResource.Cancel);
                    if (result)
                    {
                        await Application.Current.MainPage.Navigation.PushModalAsync(new AutorizacionesModal(false, 0, 1, "")
                        {
                            OnAutorizacionUsed = (autSec) =>
                            {
                                Arguments.Values.CurrentModule = Modules.PEDIDOS;
                                PushAsync(new PedidosPage(autorizado: true));
                            }
                        });
                    }
                    IsBusy = false;
                    return;
                }
                else
                {
                    await PushAsync(new PedidosPage(autorizado: true));
                }
            }
            else 
            {
                await PushAsync(new PedidosPage());
            }

        }

        private async void GoCotizaciones(bool clientBlock = false)
        {
            if (clientBlock)
            {
                bool result = await DisplayAlert(AppResource.Warning, AppResource.CannotMakeQuotesCustomerBlocked, AppResource.Authorize, AppResource.Cancel);
                if (result)
                {
                    await Application.Current.MainPage.Navigation.PushModalAsync(new AutorizacionesModal(false, 0, 5, "")
                    {
                        OnAutorizacionUsed = (autSec) =>
                        {
                            GoCotizaciones(false);
                        }
                    });
                }
                IsBusy = false;
                return;
            }

            Arguments.Values.CurrentModule = Modules.COTIZACIONES;
            await PushAsync(new PedidosPage());
        }

        private async void GoInventarioFisico(bool clientBlock = false)
        {
            if (clientBlock)
            {
                bool result = await DisplayAlert(AppResource.Warning, AppResource.CannotMakePhysicalInventoriesCustomerBlocked, AppResource.Authorize, AppResource.Cancel);
                if (result)
                {
                    await Application.Current.MainPage.Navigation.PushModalAsync(new AutorizacionesModal(false, 0, 6, "")
                    {
                        OnAutorizacionUsed = (autSec) =>
                        {
                            GoInventarioFisico(false);
                        }
                    });
                }
                IsBusy = false;
                return;
            }

            Arguments.Values.CurrentModule = Modules.INVFISICO;
            await PushAsync(new PedidosPage());
        }

        private async void GoCambiosMercancia(bool clientBlock = false)
        {
            if (clientBlock)
            {
                bool result = await DisplayAlert(AppResource.Warning, AppResource.CannotMakeChangesCustomerBlocked, AppResource.Authorize, AppResource.Cancel);
                if (result)
                {
                    await Application.Current.MainPage.Navigation.PushModalAsync(new AutorizacionesModal(false, 0, 18, "")
                    {
                        OnAutorizacionUsed = (autSec) =>
                        {
                            GoCambiosMercancia(false);
                        }
                    });
                }
                IsBusy = false;
                return;
            }

            Arguments.Values.CurrentModule = Modules.CAMBIOSMERCANCIA;
            await PushAsync(new PedidosPage());
        }

        private async void ImprimirEntregas()
        {
            try
            {
                IsBusy = true;

                var copias = await DisplayActionSheet(AppResource.ChoosePrinterCopies, buttons: new string[] { "1", "2", "3", "4", "5" });

                if(int.TryParse(copias, out int intCopias) && intCopias > 0)
                {
                    PrinterManager printer = null;
                    var formats = new EntregasRepartidorFormats(myEnt);

                    for(int i = 0; i < intCopias; i++)
                    {
                        await Task.Run(() => 
                        {
                            if(printer == null)
                            {
                                printer = new PrinterManager();
                            }

                            formats.Print(-1, false, printer);
                        });

                        IsBusy = false;

                        if (intCopias > 1 && i != intCopias - 1)
                        {
                            await DisplayAlert(AppResource.PrintCopy, AppResource.CutPapelMessage, AppResource.Print);
                        }
                    }
                }

            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        private async void GoConduces()
        {
            IsBusy = true;

            try
            {
                Arguments.Values.CurrentModule = Modules.CONDUCES;

                var controller = new DS_Conduces();

                await Task.Run(() => 
                {
                    controller.InsertProductInTempForConduce();
                });

                var conSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Conduces");

                var entrega = new EntregasRepartidorTransacciones();
                entrega.CliCodigo = Arguments.Values.CurrentClient.CliCodigo;
                entrega.CliID = Arguments.Values.CurrentClient.CliID;
                entrega.CliNombre = Arguments.Values.CurrentClient.CliNombre;
                entrega.ConID = Arguments.Values.CurrentClient.ConID;
                entrega.EnrSecuencia = 1;
                entrega.RepCodigo = Arguments.CurrentUser.RepCodigo;
                entrega.TitID = 51;
                entrega.TraSecuencia = conSecuencia;
                entrega.venNumeroERP = conSecuencia.ToString();

                await PushAsync(new EntregasRepartidorDetallePage(entrega));

                IsBusy = false;
            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
            
        }

        private void LoadDatosVisita(int visSecuencia)
        {
            Arguments.Values.CurrentClient = myCli.GetClienteByVisita(visSecuencia);
            Arguments.Values.CurrentVisSecuencia = visSecuencia;

            if (Arguments.CurrentUser != null && Arguments.CurrentUser.TipoRelacionClientes == 2)
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

            LoadStaticData();

            if (Arguments.Values.CurrentClient != null)
            {
                if (myParametro.GetOfertasConSegmento())
                {
                    new DS_Ofertas().GuardarProductosValidosParaOfertasPorSegmento(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentClient.TiNID);
                }
                else
                {
                    new DS_Ofertas().GuardarProductosValidosParaOfertas(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentClient.TiNID);
                }
            }
        }

        public async void CerrarVisita(bool forceClose = false)
        {
            try
            {
                if (CurrentTipoVisita == null)
                {
                    if (forceClose)
                    {
                        CurrentTipoVisita = new UsosMultiples() { CodigoUso = "1" };
                    }
                    else
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.MustSelectVisitType, AppResource.Aceptar);
                        IsBusy = false;
                        return;
                    }
                }
                IsBusy = true;
                myProd.ClearProductosTempGeneral();
                var CliIDVisVirtual = Arguments.Values.CurrentClient.CliID;
                int visSecuencia = Arguments.Values.CurrentVisSecuencia;

                bool isVisitaVirtualHija = myVis.IsVisitaVirtual(visSecuencia) && VisitaVirtualFinalizada != null;

                if (myParametro.GetParSectores() < 3 || isVisitaVirtualHija) {
                    await Task.Run(() => {
                        myVis.CerrarVisita(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentVisSecuencia, Arguments.Values.CurrentLocation, CurrentTipoVisita.CodigoUso, isVisitaVirtualHija);
                    });
                }
                else
                {
                    await Task.Run(() => {
                        myVis.CerrarVisita(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentVisSecuencia, Arguments.Values.CurrentLocation, CurrentTipoVisita.CodigoUso, isVisitaVirtualHija);                        
                    });

                    ElegirSectorPage.ClosingVisit = true;
                }
                
                if (isVisitaVirtualHija)
                {
                    int visSecuenciaOrigen = myVis.GetVisSecuenciaOrigen(visSecuencia);

                    if (visSecuenciaOrigen != -1)
                    {
                        VisitaVirtualFinalizada?.Invoke(visSecuenciaOrigen);
                    }
                }

                bool noSyncAuto = true;

                string secCodigo = null;
                var parSector = myParametro.GetParSectores();
                if (parSector > 2 && Arguments.Values.CurrentSector != null)
                {
                    secCodigo = Arguments.Values.CurrentSector.SecCodigo;
                }
                var parComentarioObligatorio = myParametro.GetParVisitaComentarioObligatorio();
                var insertoComentario = false;
               
                if (isVisitaVirtualHija)
                {
                    insertoComentario = myMen.Exists(13, visSecuencia, CliIDVisVirtual, visSecuencia, secCodigo);

                }
                else
                {
                    insertoComentario = myMen.Exists(13, Arguments.Values.CurrentVisSecuencia, Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentVisSecuencia, secCodigo);

                }

                var sincronizarSiTieneMuchosRegistros = myParametro.GetParSincronizacionAutomaticaByCantidadRegistros();

                if(sincronizarSiTieneMuchosRegistros != null && sincronizarSiTieneMuchosRegistros.Moment == MomentToSync.LUEGO_CERRAR_VISITA && sincronizarSiTieneMuchosRegistros.QuantityOfPendingToSync > 0)
                {
                    var pendientes = new DS_SuscriptoresCambios().GetAll().Count;

                    if (pendientes >= sincronizarSiTieneMuchosRegistros.QuantityOfPendingToSync)
                    {
                        if (PushModalAsync(new SincronizarModal()).GetAwaiter().IsCompleted)
                        {
                            await PopModalAsync(true);
                        }
                    }

                    await PopAsync(true);

                }
                else if (myVis.IsVisitaEfectiva((isVisitaVirtualHija ? visSecuencia : Arguments.Values.CurrentVisSecuencia), (isVisitaVirtualHija ? CliIDVisVirtual : Arguments.Values.CurrentClient.CliID)) && !myParametro.GetSyncAuto())  //else if (isTransaccion) ESTA CONDICION CAUSABA QUE NO ENTRARA A LA FUNCIONALIDAD DEL PARAMETRO SYNCAUTO
                {
                    await PopAsync(true);
                }
                else if (parComentarioObligatorio == 1 && !insertoComentario && !myVis.IsVisitaEfectiva((isVisitaVirtualHija ? visSecuencia : Arguments.Values.CurrentVisSecuencia), (isVisitaVirtualHija ? CliIDVisVirtual : Arguments.Values.CurrentClient.CliID)))
                {
                    IsBusy = false;
                }
                else
                {
                    var pageRemoved = false;
                    if (myParametro.GetSyncAuto() && CurrentTipoVisita.CodigoUso == "1" && !myParametro.GetParNoNoticiasAUTOSYNC())
                    {
                        noSyncAuto = false;
                        var navigation = App.Current.MainPage.Navigation;
                        if (PushModalAsync(new SincronizarModal()).GetAwaiter().IsCompleted)
                        {
                            await PopModalAsync(true);
                        }
                        else
                            noSyncAuto = true;
                        {
                        }

                        var previousPage = navigation.NavigationStack.LastOrDefault();
                        if (myNot.GetCantidadNoticiasSinLeer() == 0)
                            await App.Current.MainPage.DisplayAlert(AppResource.Warning, AppResource.NoNewsToRead, "OK");
                        else
                            await PushAsync(new NoticiasPage());
                        navigation.RemovePage(previousPage);

                        pageRemoved = true;
                    }
                    else if(myParametro.GetSyncAuto() && CurrentTipoVisita.CodigoUso == "1")
                    {
                        noSyncAuto = false;
                        if (PushModalAsync(new SincronizarModal()).GetAwaiter().IsCompleted)
                        {
                            await PopModalAsync(true);
                        }
                        else
                        {
                            noSyncAuto = true;
                        }
                    }


                    if ((noSyncAuto || forceClose) && !pageRemoved)
                    {
                        await PopAsync(true);
                    }
                    else if ((myVis.IsVisitaEfectiva(Arguments.Values.CurrentVisSecuencia, Arguments.Values.CurrentClient.CliID) || insertoComentario) && !pageRemoved)
                    {
                        IsBusy = true;
                        await PopAsync(true);
                    }
                    else
                    {
                        IsBusy = false;
                    }

                }               
                
            }
            catch(Exception e)
            {
                Crashes.TrackError(e);
                await DisplayAlert(AppResource.ErrorClosingVisit, e.Message, AppResource.Aceptar);
            }
            
            IsBusy = false;            
        }

        public override bool OnBackButtonPressed()
        {
            if (DS_RepresentantesParametros.GetInstance().GetParSeleccioneVisita() && CurrentTipoVisita == null)
            {
                App.Current.MainPage.DisplayAlert(AppResource.Warning, AppResource.MustSelectVisitTypeToCloseVisit, "OK");
                return false;
            }     

            AlertCerrarVisita();

            return true;
        }

        public async void AlertCerrarVisita()
        {
            try
            {
                if(IsBusy){
                    return;
                }

               // var isTransaccion = myVis.VerificarDeTransacciones(Arguments.Values.CurrentVisSecuencia, Arguments.Values.CurrentClient.CliID);

                IsBusy = true;

                if (IsConsulting)
                {
                    await PopAsync(false);
                    IsBusy = false;
                    return;
                }
                string secCodigo = null;
                var parSector = myParametro.GetParSectores();
                if (parSector  > 2 && Arguments.Values.CurrentSector != null)
                {
                    secCodigo = Arguments.Values.CurrentSector.SecCodigo;
                }

                var parComentarioObligatorio = myParametro.GetParVisitaComentarioObligatorio();
                var insertoComentario = myMen.Exists(13, Arguments.Values.CurrentVisSecuencia, Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentVisSecuencia, secCodigo);

                if (myParametro.GetParClientesTodosInventarios() && Arguments.IsValidToGetOut != 1)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.CannotLeaveVisitWithoutFillInventory, AppResource.Aceptar);                    
                    IsBusy = false;
                    return;
                }

                Arguments.IsValidToGetOut = -1;

                if (!insertoComentario)
                {
                   /* if (isTransaccion)
                    {
                        IsBusy = true;
                    }
                    else */if((parComentarioObligatorio == 1 && !myVis.IsVisitaEfectiva(Arguments.Values.CurrentVisSecuencia, Arguments.Values.CurrentClient.CliID)) || parComentarioObligatorio == 2 || (parComentarioObligatorio == 3 && myCxc.GetFacturasVencidad(Arguments.Values.CurrentClient.CliID).Count > 0))
                    {
                        await DisplayAlert(AppResource.Warning, (parComentarioObligatorio == 1 ? AppResource.CannotLeaveWithoutMakingTransactions : (parComentarioObligatorio == 3 ? AppResource.ThereArePastDueInvoicesCustomer : "")) + AppResource.PutCommentToLeave);
                        IsBusy = false;
                        return;
                    }
                     
                }


                if (myParametro.GetParEncuestas() && 
                    ((myParametro.GetParEncuestaObligatoria() && myEst.GetEncuestasVigentes(Arguments.Values.CurrentClient.CliID).Count > 0)
                    || myEst.HayEncuestasVigentesForEstTipoMuestra(Arguments.Values.CurrentClient.CliID)))
                {
                    await DisplayAlert(AppResource.Warning, AppResource.SurveyAvailableMustTakeToContinue, AppResource.Aceptar);
                    IsBusy = false;
                    return;
                }

                /* if(myParametro.GetParSAC() == 2 && Arguments.Values.CurrentClient.CliLatitud == 0 && Arguments.Values.CurrentClient.CliLongitud == 0)
                 {
                     bool SACinVisita = myCli.GetSACinVisita(Arguments.Values.CurrentVisSecuencia);

                     if (!SACinVisita)
                     {
                         await DisplayAlert(AppResource.Warning, "Debe colocar la georeferencia del cliente para continuar", AppResource.Aceptar);
                         IsBusy = false;
                         return;
                     }
                 }*/


                bool isVirtual = myVis.IsVisitaVirtual(Arguments.Values.CurrentVisSecuencia);

                var result = await DisplayAlert(AppResource.CloseVisit, AppResource.WantCloseVisitQuestion + " " + (isVirtual ? "virtual" : "") + "?", AppResource.Yes, "No");

                IsBusy = myVis.IsVisitaEfectiva(Arguments.Values.CurrentVisSecuencia, Arguments.Values.CurrentClient.CliID) ? true : false;

                if (result)
                {
                    IsBusy = true;

                    if (Params.ParImprimirEntregas && myEnt.HayEntregasSinImprimirByVisita(Arguments.Values.CurrentVisSecuencia))
                    {
                        bool mostrar = true;
                        if ((Params.parSectorValor > 2 && new DS_Sectores().GetNextSectorAVisitar(Arguments.Values.CurrentClient.CliID) != null))
                        {
                            mostrar = false;
                        }

                        if (mostrar)
                        {
                            var seguir = await DisplayAlert(AppResource.Warning, AppResource.DeliveriesWithoutPrintingWantContinueQuestion, AppResource.Continue, AppResource.Cancel);

                            if (!seguir)
                            {
                                IsBusy = false;
                                return;
                            }
                        }
                    }

                    if (isVirtual)
                    {
                        Arguments.Values.CurrentSector = Sectoranterior;
                    }
                    _IsBusyLoad.Invoke(true);
                    CerrarVisita();
                    _IsBusyLoad.Invoke(false);
                }

            }
            catch(Exception e)
            {
                Crashes.TrackError(e);
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        public async void GoModuleAutomaticIfValid()
        {
            if (IsConsulting)
            {
                return;
            }
            if (Arguments.Values.CurrentClient.CliIndicadorPresentacion && CurrentVisitaPresentacion == null)
            {
                await Task.Delay(500);

                await DisplayAlert(AppResource.Warning, AppResource.VisitToPresentationCustomerMustCompleteData);

                await PushModalAsync(new ClientesPresentacionModal() { OnSave = (visita)=> 
                {
                    CurrentVisitaPresentacion = visita;

                    if (myParametro.GetParVisitaCobrosAutomatica() == 1)
                    {
                        GoModule("4");
                    }
                }
                });
                return;
            }

            if(myParametro.GetParEntregasEntrarAutomatico())
            {
                if (myEnt.HayEntregasDisponibles(CurrentSector != null && myParametro.GetParEntregasRepartidorPorSector() ? CurrentSector.SecCodigo : null))
                {
                    GoModule("20");
                    return;
                }
            }

            if(myParametro.GetParVisitaCobrosAutomatica() == 1)
            {
                GoModule("4");
            }
        }

        public struct OperacionesParamsArgs
        {
            public bool ParPedidos { get; set; }
            public bool ParDevoluciones { get; set; }
            public bool ParProductos { get; set; }
            public bool ParSAC { get; set; }
            public bool ParVentas { get; set; }
            public bool ParCobros { get; set; }
            public bool ParCompras { get; set; }
            public bool ParComprasRotacion { get; set; }
            public bool ParInvFisico { get; set; }
            public bool ParCotizaciones { get; set; }
            public bool ParVisitaFoto { get; set; }
            public bool ParAuditoriaMercado { get; set; }
            public bool ParComentarioVisitas { get; set; }
            public bool ParEncuestas { get; set; }
            public bool ParPresupuestoClientes { get; set; }
            public bool ParDatos { get; set; }
            public bool ParPedidosBackOrder { get; set; }
            public bool ParSectores { get => parSectorValor > 0; }
            public bool ParReclamaciones { get; set; }
            public bool ParProductosProximoVencer { get; set; }
            public bool ParEntregasRepartidor { get; set; }
            public bool ParTareas { get; set; }
            public bool ParRecepcionDevolucion { get; set; }
            public bool ParConduces { get; set; }
            public bool ParCambiosMercancia { get; set; }
            public bool ParImprimirEntregas { get; set; }
            public bool ParCalculadoraNegociacion { get; set; }
            public bool ParHistoricoFacturas { get; set; }
            public bool ParPushMoneyPorPagar { get; set; }
            public bool ParVisitaSincronizar { get; set; }
            public bool ParConsultarEntregas { get; set; }
            public bool ParQuejasServicio { get; set; }

            public bool ParPromociones { get; set; }

            public bool ParEntregasMercancias { get; set; }

            public bool ParCanastos { get; set; }
            public int parSectorValor { get; set; }

            public bool ParConsultaTransacciones { get; set; }

            public bool ParExcluirClientes { get; set; }

            public bool ParSacd { get; set; }

            public bool ParAuditoriaPrecios { get; set; }

            public bool ParConsultaInventarioFisico { get; set; }

            public bool ParColocacionMercancias { get; set; }
            public bool ParNoticiasOP { get; set; }
        }

        public void LoadSectorArbitrario(Sectores sector)
        {
            if(Params.ParSectores && Params.parSectorValor > 2 && Sectores != null && Sectores.Count > 0 && sector != null)
            {
                CurrentSector = Sectores.Where(x => x.SecCodigo == sector.SecCodigo).FirstOrDefault();
            }
        }


        public void ResfreshTaskPending()
        {
            if (myParametro.GetParTareas())
            {
                PendingTaskCount = Mytar.getTareasPendientes().ToString();
            }
            if (myParametro.GetParNoticiasEnOperaciones())
            {
                PendingNewsCount = myNot.GetCantidadNoticiasSinLeer().ToString();
            }
            if (myParametro.GetParGPS() && Arguments.Values.CurrentClient.CliLatitud != 0 && Arguments.Values.CurrentClient.CliLongitud != 0 && Arguments.Values.CurrentLocation != null)
            {
                var metros = myParametro.GetParVisitaDistanciaMinimaVisitaPresencial();

                if (metros > 0)
                {
                    var distancia = Functions.DistanceTo(Arguments.Values.CurrentClient.CliLatitud, Arguments.Values.CurrentClient.CliLongitud, Arguments.Values.CurrentLocation.Latitude, Arguments.Values.CurrentLocation.Longitude, 'M');

                    if (distancia > metros)
                    {
                        var item = TiposVisita.Where(x => x.Descripcion.ToUpper().Trim().Equals("VIRTUAL")).FirstOrDefault();

                        if (item != null)
                        {
                            CurrentTipoVisita = item;
                            EnableComboTipoVisita = false;
                        }
                    }
                }
            }

        }


        public void ResfreshVentasPending()
        {
            if (myParametro.GetParVentas())
            {
                PendingVentasCount = myEnt.getPedidosPorEntregar(Arguments.Values.CurrentClient.CliID).ToString();
            }

            if (myParametro.GetParGPS() && Arguments.Values.CurrentClient.CliLatitud != 0 && Arguments.Values.CurrentClient.CliLongitud != 0 && Arguments.Values.CurrentLocation != null)
            {
                var metros = myParametro.GetParVisitaDistanciaMinimaVisitaPresencial();

                if (metros > 0)
                {
                    var distancia = Functions.DistanceTo(Arguments.Values.CurrentClient.CliLatitud, Arguments.Values.CurrentClient.CliLongitud, Arguments.Values.CurrentLocation.Latitude, Arguments.Values.CurrentLocation.Longitude, 'M');

                    if (distancia > metros)
                    {
                        var item = TiposVisita.Where(x => x.Descripcion.ToUpper().Trim().Equals("VIRTUAL")).FirstOrDefault();

                        if (item != null)
                        {
                            CurrentTipoVisita = item;
                            EnableComboTipoVisita = false;
                        }
                    }
                }
            }

        }

        public void ResfreshEntregasFacturasPending()
        {
            if (myParametro.GetParEntregasRepartidor() == 1 || myParametro.GetParEntregasRepartidor() == 3)
            {
                PendingEntregaFacturaCount = myEnt.GetEntregasFacturasDisponibles(Arguments.Values.CurrentClient.CliID, myParametro.GetParEntregasRepartidor() == 3).ToString();
            }

            if (myParametro.GetParGPS() && Arguments.Values.CurrentClient.CliLatitud != 0 && Arguments.Values.CurrentClient.CliLongitud != 0 && Arguments.Values.CurrentLocation != null)
            {
                var metros = myParametro.GetParVisitaDistanciaMinimaVisitaPresencial();

                if (metros > 0)
                {
                    var distancia = Functions.DistanceTo(Arguments.Values.CurrentClient.CliLatitud, Arguments.Values.CurrentClient.CliLongitud, Arguments.Values.CurrentLocation.Latitude, Arguments.Values.CurrentLocation.Longitude, 'M');

                    if (distancia > metros)
                    {
                        UsosMultiples item = new UsosMultiples();

                        if (!string.IsNullOrEmpty(_TypeOfVisit))
                        {
                            item = TiposVisita.FirstOrDefault(x => x.Descripcion.ToUpper()
                            .Trim().Equals(_TypeOfVisit.ToUpper().Trim()));
                        }
                        else
                        {
                            item = TiposVisita.Where(x => x.Descripcion.ToUpper().Trim().Equals("VIRTUAL")).FirstOrDefault();
                        }                       

                        if (item != null)
                        {
                            if (!string.IsNullOrEmpty(_TypeOfVisit))
                            {
                                CurrentTipoVisita = null;
                                CurrentTipoVisita = item;
                                EnableComboTipoVisita = true;
                            }
                            else
                            {                                
                                CurrentTipoVisita = item;
                                EnableComboTipoVisita = false;
                            }                            
                        }
                    }
                }
            }

        }


        public bool LoadOtherSectorForEntrega()
        {
            if (string.IsNullOrWhiteSpace(Arguments.Values.SecCodigoParaCrearVisita) || !Params.ParSectores || Sectores == null || Sectores.Count == 0 || (myVis.IsVisitaVirtual(Arguments.Values.CurrentVisSecuencia) && VisitaVirtualFinalizada != null))
            {
                Arguments.Values.SecCodigoParaCrearVisita = null;
                return false;
            }

            if(Params.parSectorValor < 3)
            {
                CurrentSector = Sectores.Where(x => x.SecCodigo == Arguments.Values.SecCodigoParaCrearVisita).FirstOrDefault();

                Arguments.Values.SecCodigoParaCrearVisita = null;

                GoModule("20");
            }
            else
            {
                CerrarVisita(true);
            }

            return true;
        }

        private async void ShowAlertSincronizar()
        {
            var alert = await DisplayAlert(AppResource.SyncUp, AppResource.GoSyncQuestion, AppResource.SyncUp, AppResource.Cancel);

            if (alert)
            {
                Sincronizar();
            }

        }

        private async void Sincronizar()
        {
            try
            {
                await PushModalAsync(new SincronizarModal());
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.SynchronizationFailed, e.Message, AppResource.Aceptar);
            }
        }

        public async void InitPrinterManager()
        {
            await Task.Run(() =>
            {
                SuccessPage.Printer = new PrinterManager();
            });
        }

        private void OnTipoVisitaChanged()
        {
            if(CurrentTipoVisita == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(CurrentTipoVisita.Descripcion) && CurrentTipoVisita.Descripcion.ToUpper().Contains("VIRTUAL"))
            {
                ShowVisitaVirtual = false;
            }else if (!isVisitaVirtual && myParametro.GetParVisitaVirtual())
            {
                ShowVisitaVirtual = true;
            }
        }

        public void ShowAlertNoRecibeProductoMayorAlAnio()
        {
            try
            {
                if (Arguments.Values.CurrentClient != null && !string.IsNullOrWhiteSpace(Arguments.Values.CurrentClient.CliDatosOtros)
                    && Arguments.Values.CurrentClient.CliDatosOtros.ToUpper().Contains("J")
                    && new DS_InventariosAlmacenesLotes().ExistenProductosConVencimientoMenorAlAnio())
                {
                    DisplayAlert(AppResource.Warning, AppResource.CustomerNotReceiveProductsWithLessThanAYearExpiration);
                }
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }
        }

        private void ConfigTipoVisitaManual()
        {
            if (myParametro.GetParVisitasSeleccionarTipoManual() == 0 || IsConsulting || isVisitaVirtual)
            {
                return;
            }
            if (myParametro.GetParVisitasSeleccionarTipoManual() == 2)
            {
                enablecombovisita = false;
            }
            else
            {
                enablecombovisita = true;
            }
            RaiseOnPropertyChanged(nameof(EnableComboTipoVisita));
        }

    }
}
