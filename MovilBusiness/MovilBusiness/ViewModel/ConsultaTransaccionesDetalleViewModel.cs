
using Microsoft.AppCenter.Crashes;
using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.model.Internal.Structs.Args;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal.Structs.Args;
using MovilBusiness.Pdf.Formats;
using MovilBusiness.Printer;
using MovilBusiness.Printer.Formats;
using MovilBusiness.printers.formats;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.views;
using MovilBusiness.Views;
using MovilBusiness.Views.Components.Modals;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.viewmodel
{
    public class ConsultaTransaccionesDetalleViewModel : BaseViewModel
    {
        private DS_Transacciones myTran;
        private DS_Pedidos myPed;
        private DS_Recibos myRec;
        private DS_Depositos myDep;
        private DS_DepositosCompras myDepCom;
        private DS_EntregaFactura myEnt;
        private DS_Devoluciones myDev;
        private DS_Clientes myCli;
        private DS_Ventas myVen;
        private DS_Compras myCom;
        private DS_Cotizaciones myCot;
        private DS_Gastos mygas;
        private DS_EntregasRepartidorTransacciones myEntRep;
        private DS_ConteosFisicos myCon;
        private DS_Cuadres myCua;
        private DS_Estados myEst;
        private DS_Conduces myCond;
        private DS_Cambios myCam;
        private DS_PushMoneyPagos myPxpRec;
        private DS_TransferenciasAlmacenes myTraAlm;
        private DS_InventariosFisicos myinvfis;
        private DS_ColocacionProductos myColProd;
        private DS_AuditoriasPrecios myAudPre;

        int pedSecuenciaAut;
        public ICommand SearchCommand { get; private set; }
        public List<Sectores> Sectores { get; private set; }
        public Sectores SectorActual { get => Arguments.Values.CurrentSector; set { Arguments.Values.CurrentSector = value;} }

        public static PrinterManager printerManager = null;
        public ExpandListItem<Estados> DatosTransaction { get; set; }

        private ObservableCollection<Transaccion> transacciones;
        public ObservableCollection<Transaccion> Transacciones { get => transacciones; set { transacciones = value; RaiseOnPropertyChanged(); } }

        private List<Transaccion> TransaccionsTogive;

        private bool showprinter = false;
        public bool ShowPrinter { get => showprinter; set { showprinter = value; RaiseOnPropertyChanged(); } }

        private string trasecuencia;
        public string TraSecuencia { get => trasecuencia; set { trasecuencia = value; RaiseOnPropertyChanged(); } }

        // curren para mostrar las diferentes formas de busqueda
        public List<FiltrosDinamicos> FiltroSource { get; private set; }

        private FiltrosDinamicos currenfiltro;
        public FiltrosDinamicos CurrentFiltro { get => currenfiltro; set { currenfiltro = value; RaiseOnPropertyChanged(); } }


        public List<KV> secondfiltersource;
        public List<KV> SecondFiltroSource { get { return secondfiltersource;  } set { secondfiltersource = value; RaiseOnPropertyChanged(); } }
        
        private KV currentsecondfiltro;
        public KV CurrentSecondFiltro { get { return currentsecondfiltro; } set { currentsecondfiltro = value; if(value != null) { Search(); } RaiseOnPropertyChanged(); } }

        
        public bool showsecondsearch = false;
        
        public bool ShowSecondSearch { get => showsecondsearch; set { showsecondsearch = value; RaiseOnPropertyChanged(); } }

        public bool ShowSecondFilter { get { return CurrentFiltro != null && CurrentFiltro.FilTipo == 2;  } set { RaiseOnPropertyChanged(); } }
      
        public ICommand SearchSegundoFiltro { get; private set; }

        private string searchvalue;
        public  string SearchValue { get { return searchvalue; } set { searchvalue = value; if (DS_RepresentantesParametros.GetInstance().GetBuscarClienteAlEscribir()) { Search(); }  RaiseOnPropertyChanged(); } }

        private int cantidadclientes = 0;
        public int CantidadClientes { get { return cantidadclientes; } set { cantidadclientes = value; RaiseOnPropertyChanged(); } }

        //aqui termina 

        public List<Clientes> Clientes { get; set; }

        private Clientes currentclient;
        private CuadresFormats PrinterCuadre;
        public Clientes CurrentClient { get => currentclient; set { if (value == currentclient) { return; } currentclient = value; LoadTransaccionesAsync(); } }

        private Sectores currentsector;
        public Sectores CurrentSector { get => currentsector; set { if (value == currentsector) { return; } currentsector = value; LoadTransaccionesAsync();} }
        public bool UseSector { get; set; }
        private Transaccion CurrentTransaccion;

        private bool isForFecha;

        private IPrinterFormatter Printer;
        private IPrinterFormatter PrinterValidation;

        public bool primeraCarga;
        private int currentCliId = -1;

        public ConsultaTransaccionesDetalleViewModel(Page page, ExpandListItem<Estados> Tran, bool forFecha, int cliid = -1) : base(page)
        {
            isForFecha = forFecha;
            currentCliId = cliid;
            
            myTran = new DS_Transacciones();
            myCli = new DS_Clientes();
            myPed = new DS_Pedidos();
            myRec = new DS_Recibos();
            myDep = new DS_Depositos();
            myDev = new DS_Devoluciones();
            myEnt = new DS_EntregaFactura();
            myVen = new DS_Ventas();
            myCom = new DS_Compras();
            myCot = new DS_Cotizaciones();
            myDepCom = new DS_DepositosCompras();
            mygas = new DS_Gastos();
            myEntRep = new DS_EntregasRepartidorTransacciones();
            myCon = new DS_ConteosFisicos();
            myCua = new DS_Cuadres();
            myEst = new DS_Estados();
            myCond = new DS_Conduces();
            myCam = new DS_Cambios();
            myPxpRec = new DS_PushMoneyPagos();
            myTraAlm = new DS_TransferenciasAlmacenes();
            myinvfis = new DS_InventariosFisicos();
            myAudPre = new DS_AuditoriasPrecios();
            myColProd = new DS_ColocacionProductos();
            // myCua = new DS_Cuadres();

            primeraCarga = false;

            SearchCommand = new Command(LoadTransaccionesAsync);
            SearchSegundoFiltro = new Command(LoadTransaccionesAsync);

            DatosTransaction = Tran;

            Clientes = new List<Clientes>();

            BindFiltrosClientes();

            if (Tran == null || Tran.Data == null)
            {
                return;
            }

            if (Tran.Data.UseClient)
            {
                Clientes.Add(new Clientes() { CliID = -1, CliNombre = AppResource.Allind });
                Clientes.AddRange(new DS_Clientes().GetAll());
            }

            if (myParametro.GetParSectores() > 0)
            {
               
                Sectores = new DS_Sectores().GetSectores();
                UseSector = Sectores.Count > 1;
                if(UseSector)
                {
                    Sectores.Insert(0, new Sectores() { SecCodigo = "", SecDescripcion = AppResource.Allind });
                }

            }
          
            LoadTransaccionesAsync();

            InitPrinter();
        }

        private void InitPrinter()
        {           
            switch (DatosTransaction.Data.EstTabla.ToUpper())
            {
                case "PEDIDOSCONFIRMADOS":
                case "PEDIDOS":
                    Printer = new PedidosFormats(myPed);
                    break;
                case "DEPOSITOS":
                case "DEPOSITOSCONFIRMADOS":
                    Printer = new DepositosFormats(myDep);
                    break;
                case "DEPOSITOSCOMPRAS":
                case "DEPOSITOSCOMPRASCONFIRMADOS":
                    Printer = new DepositosComprasFormats(myDepCom);
                    break;
                case "DEVOLUCIONES":
                case "DEVOLUCIONESCONFIRMADAS":
                    Printer = new DevolucionesFormats(myDev);
                    break;
                case "RECIBOSCONFIRMADOS":
                case "RECIBOS":
                    Printer = new FormatosRecibos(myRec);
                    break;
                case "PUSHMONEYPAGOS":
                    Printer = new PushMoneyPorPagarFormats();
                    break;
                case "ENTREGASDOCUMENTOSCONFIRMADOS":
                case "ENTREGASDOCUMENTOS":
                    Printer = new EntregaDocumentosFormats(myEnt);
                    break;
                case "VENTAS":
                case "VENTASCONFIRMADOS":
                    Printer = new VentasFormats(myVen);
                    break;
                case "CARGAS":
                    Printer = new CargasFormats(new DS_Cargas());
                    break;
                case "COMPRAS":
                case "COMPRASCONFIRMADOS":
                    Printer = new ComprasFormats(new DS_Compras());
                    break;
                case "COTIZACIONES":
                case "COTIZACIONESCONFIRMADOS":
                    Printer = new CotizacionesFormats(myCot);
                    break;
                case "GASTOS":
                case "GASTOSCONFIRMADOS":
                    Printer = new GastosFormats(mygas);
                    break;
                case "CLIENTES":
                    Printer = new ProspectosFormats(myCli);
                    break;
                case "ENTREGASTRANSACCIONES":
                case "ENTREGASTRANSACCIONESCONFIRMADOS":
                    Printer = new EntregasRepartidorFormats(myEntRep, true);
                    break;
                case "NCDPP":
                    Printer = new FormatosRecibos(myRec);                  
                    break;
                case "CONTEOSFISICOS":
                case "CONTEOSFISICOSCONFIRMADOS":
                    Printer = new ConteosFisicosFormats();
                    break;
                case "CUADRES":
                    Printer = new CuadresFormats(myCua);
                    PrinterValidation = Printer;
                    PrinterCuadre = new CuadresFormats(myCua);
                    break;
                case "CONDUCES":
                case "CONDUCESCONFIRMADOS":
                    Printer = new ConducesFormats(myCond);
                    break;
                case "CAMBIOS":
                case "CAMBIOSCONFIRMADOS":
                    Printer = new CambiosFormats(myCam);
                    break;
                case "TRANSACCIONESCANASTOS":
                    Printer = new TransaccionesCanastosFormats();
                    break;
                case "ENTREGAS":
                    Printer = new EntregasFormats();
                    break;
                case "INVENTARIOFISICO":
                    Printer = new InventarioFisicoFormats(myinvfis);
                    break;
                case "COMPRASPUSHMONEY":
                case "COMPRASPUSHMONEYCONFIRMADOS":
                    Printer = new ComprasFormats(new DS_Compras(), true);
                    break;
                case "REQUISICIONESINVENTARIO":
                    Printer = new RequisicionesInventarioFormats();
                    break;
            }
        }

        private void BindFiltrosClientes()
        {
            FiltroSource = new DS_FiltrosDinamicos().GetFiltrosClientes();


            try { 
                    if (FiltroSource != null  && FiltroSource.Count > 0)
                    {
                        CurrentFiltro = FiltroSource.Where(x => x.FilIndicadorDefault).FirstOrDefault();

                        if(CurrentFiltro == null)
                        {
                            CurrentFiltro = FiltroSource.FirstOrDefault();
                        }

               
                    }
            }catch (Exception e)
            {
                DisplayAlert(e.Message, AppResource.Aceptar);
                Crashes.TrackError(e);
            }

        }

        public async void Search()
        {
            try
            {

                if (IsBusy)
                {
                    return;
                }

                IsBusy = true;

                var repcodigo = Arguments.CurrentUser.RepCodigo;

                var args = new ClientesArgs() { filter = CurrentFiltro, secondFilter = CurrentSecondFiltro != null ? CurrentSecondFiltro.Key : "", SearchValue = SearchValue, DiaNumero = -1, NumeroSemana = -1, RepCodigo = repcodigo };


                await Task.Run(() => { Clientes = myCli.GetClientes(args).ToList(); });

         
            }
            catch(Exception e)
            {
                IsBusy = false;
                await DisplayAlert(AppResource.ErrorLoadingClients, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;

        }


        private Task Imprimir(int preFormat)
        {
            var estable = DatosTransaction.Data.EstTabla.ToUpper();

            switch (estable)
            {
                case "COMPRASPUSHMONEY":
                    estable = "COMPRAS";
                    break;
                case "COMPRASPUSHMONEYCONFIRMADOS":
                    estable = "COMPRASCONFIRMADOS";
                    break;
                default:
                    break;
            }

            var rowguid = Functions.GetrowguidTransaccion(estable,estable.Contains("NCDPP")? "RecSecuencia" : estable.Substring(0, 3) + (estable.Substring(0, 3) == "CLI"? "Id" : "Secuencia"), CurrentTransaccion.TransaccionID);
           
            return Task.Run(() =>
            {
                if(printerManager == null)
                {
                    printerManager = new PrinterManager(CurrentTransaccion.Seccodigo);
                }else if(myParametro.GetParEmpresasBySector())
                {
                    printerManager.Empresa = new DS_Empresa().GetEmpresa(CurrentTransaccion.Seccodigo);
                }
                if (estable == "NCDPP")
                {
                    PrintNC(CurrentTransaccion.TransaccionID, printerManager, new FormatosRecibos(new DS_Recibos()));
                }
                else
                {
                    if(Printer == PrinterValidation)
                    {
                        if (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCuadres() == 4)
                        {
                            PrinterCuadre.PrintFacturas(CurrentTransaccion.TransaccionID, estable.Contains("CONFIRMADOS") || DatosTransaction.Data.EstTabla.ToUpper().Contains("CONFIRMADAS"), printerManager);
                        }
                        else
                        {
                            Printer.Print(CurrentTransaccion.TransaccionID, estable.Contains("CONFIRMADOS") || DatosTransaction.Data.EstTabla.ToUpper().Contains("CONFIRMADAS"), printerManager, rowguid);
                        }
                    }
                    else
                    {
                        Printer.Print(CurrentTransaccion.TransaccionID, estable.Contains("CONFIRMADOS") || DatosTransaction.Data.EstTabla.ToUpper().Contains("CONFIRMADAS"), printerManager, rowguid, preFormat);
                    }

                    if ((estable == "RECIBOS" || estable == "RECIBOSCONFIRMADOS") && DS_RepresentantesParametros.GetInstance().GetParRecibosNCPorDescuentoProntoPago() == 1 && myRec.GetReciboTieneNCPorDpp(CurrentTransaccion.TransaccionID))
                    {
                       PrintNC(CurrentTransaccion.TransaccionID, printerManager, new FormatosRecibos(new DS_Recibos()));
                    }

                }
               
            });
        }
  
        public async void AceptarImpresion(int Copias)
        {
            try
            {
                if(Printer == null)
                {
                    return;
                }

                var preFormat = -1;

                if((DatosTransaction.Data.EstTabla.ToUpper().Equals("PEDIDOS") || DatosTransaction.Data.EstTabla.ToUpper().Equals("PEDIDOSCONFIRMADOS")) && myParametro.GetFormatoImpresionPedidos().Count() > 1)
                {
                    preFormat = await ShowAlertChooseFormat();

                    if(preFormat == -1)
                    {
                        return;
                    }
                }

                for (int x = 0; x < Copias; x++)
                {
                    IsBusy = true;

                    await Imprimir(preFormat);
                 
                    IsBusy = false;

                    if (Copias > 1 && x != Copias - 1)
                    {
                        await DisplayAlert(AppResource.PrintCopy, AppResource.CutPapelMessage, AppResource.Print);
                    }

                }
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                await DisplayAlert(AppResource.ErrorPrinting + ": " + DatosTransaction.Title, e.Message, AppResource.Aceptar);
            }
            IsBusy = false;
            ShowPrinter = false;
        }

        private async Task<int> ShowAlertChooseFormat()
        {
            var formatos = myParametro.GetFormatoImpresionPedidos();

            var nombres = new DS_UsosMultiples().GetFormatosImpresionPedidos();

            var buttons = new List<string>();

            foreach (var f in formatos)
            {
                var item = nombres.Where(x => x.CodigoUso == f.ToString()).FirstOrDefault();

                if (item != null)
                {
                    buttons.Add(item.Descripcion);
                }
            }

            if(buttons.Count == 0)
            {
                return -1;
            }

            var result = await DisplayActionSheet(AppResource.SelectPrintFormat, AppResource.Cancel, buttons:buttons.ToArray());

            var picked = nombres.Where(x => x.Descripcion == result).FirstOrDefault();

            if (picked != null && int.TryParse(picked.CodigoUso, out int format))
            {
                return format;
            }

            return -1;
        }


        public async void LoadTransaccionesAsync()
        {
            try
            {
                if (DatosTransaction == null || DatosTransaction.Data == null)
                {
                    return;
                }

                IsBusy = true;

                var result = int.TryParse(TraSecuencia, out int secuencia);

                int traSecuencia = -1;

                if (result)
                {
                    traSecuencia = secuencia;
                }

                if (myParametro.GetParConsultaTransaccionesUsarSectores() && DatosTransaction.Data.EstTabla.ToUpper().StartsWith("ENTREGASTRANSACCIONES"))
                {
                    Sectores = new DS_Sectores().GetSectores();
                    if (Sectores != null && Sectores.Count > 0)
                    {
                        UseSector = Sectores.Count > 1;
                        if (UseSector)
                        {
                            Sectores.Insert(0, new Sectores() { SecCodigo = "", SecDescripcion = AppResource.Allind });
                        }
                    }
                }

                var name = DatosTransaction.Data.EstTabla.ToUpper();

                if(name == "TRANSACCIONESCANASTOS" || DatosTransaction.Title.ToUpper() == "PROMOCIONES")
                {
                    name = DatosTransaction.Title;
                }

                if (DS_RepresentantesParametros.GetInstance().GetParConsultaTrancacionBusquedaDiferente() == 2 && primeraCarga == true)
                {
                    string filtro = CurrentFiltro.FilDescripcion;
                    var SearchVal = SearchValue;
                    var sector = CurrentSector != null && !string.IsNullOrWhiteSpace(CurrentSector.SecCodigo) ? CurrentSector.SecCodigo : "";
                    var value = isForFecha ? DatosTransaction.Data.EstDescripcion : DatosTransaction.Data.EstEstado;

                    await Task.Run(() =>
                    {
                        TransaccionsTogive = myTran.GetByNameAndEstatus(name, value, currentCliId, traSecuencia,
                            sector, isForFecha, SearchVal, filtro);
                        Transacciones = new ObservableCollection<Transaccion>(TransaccionsTogive);
                    });
                }
                else
                {
                    var cliid = CurrentClient != null && CurrentClient.CliID != -1 ? CurrentClient.CliID : -1;

                    if(currentCliId != -1)
                    {
                        cliid = currentCliId;
                    }
                    var sector = CurrentSector != null && !string.IsNullOrWhiteSpace(CurrentSector.SecCodigo) ? CurrentSector.SecCodigo : "";
                    var value = isForFecha ? DatosTransaction.Data.EstDescripcion : DatosTransaction.Data.EstEstado;

                    await Task.Run(() =>
                    {
                        TransaccionsTogive = myTran.GetByNameAndEstatus(name, value, cliid,
                            traSecuencia, sector, isForFecha);
                        Transacciones = new ObservableCollection<Transaccion>(TransaccionsTogive);
                    });

                    primeraCarga = true;
                }
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                await DisplayAlert(AppResource.ErrorLoadingTransactions, e.Message, AppResource.Aceptar);
            }
            IsBusy = false;
        }

        public async void ShowAlertOpcionesTransaccion(Transaccion tran)
        {
            try
            {
                IsBusy = true;

                CurrentTransaccion = tran;
               var estado = DatosTransaction.Data;

                if (myParametro.GetParSectores() > 0)
                {
                    Application app = Application.Current;
                    Application.Current.Properties["SecCodigo"]
                        = CurrentTransaccion.Seccodigo;
                    await app.SavePropertiesAsync();
                }


                if (myParametro.GetParImageForLogo())
                {
                    var t = Task.Run(() =>
                    {
                        DependencyService.Get<IPlatformImageConverter>()
                        .DecodeForEscPos(
                             new DS_Empresa().GetEmpresa(
                                DS_RepresentantesParametros.GetInstance().GetParEmpresasBySector() ?
                                tran.Seccodigo : ""
                             ).EmpLogo, 370, 180
                             );
                    });
                }

                    if (isForFecha)
                    {
                        estado = myEst.GetByTablaAndEstatus(DatosTransaction.Data.EstTabla, tran.Estatus);
                    }

                    if (estado == null || string.IsNullOrWhiteSpace(estado.estOpciones))
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.NoOptionsAvailables);
                        IsBusy = false;
                        return;
                    }

                    var opciones = new List<string>();

                    if (estado.estOpciones.ToUpper().Contains("V"))
                    {
                        opciones.Add(AppResource.SeeDetail);
                    }
                    if (estado.estOpciones.ToUpper().Contains("I"))
                    {
                        opciones.Add(AppResource.Print);
                    }
                    if (estado.estOpciones.ToUpper().Contains("E"))
                    {
                        if (DatosTransaction.Data.EstTabla.ToUpper() == "DEVOLUCIONES")
                        {
                            var list = SqliteManager.GetInstance().Query<Devoluciones>("SELECT DevTipo FROM Devoluciones WHERE DevSecuencia = '" + tran.TransaccionID + "' AND RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "'", new string[] { });
                            if (list != null)
                            {
                                if (list[0].DevTipo.ToString() != "3")
                                {
                                    opciones.Add(AppResource.Edit);
                                }
                            }
                        }
                        else
                        {
                            opciones.Add(AppResource.Edit);
                        }    
                    }
                    if (estado.estOpciones.ToUpper().Contains("A"))
                    {
                        if (DatosTransaction.Data.EstTabla.ToUpper() == "VENTAS" && DS_RepresentantesParametros.GetInstance().GetAnularUltimaVenta() > 0
                            && myVen.isVentaUltimoCuadre(CurrentTransaccion.TransaccionID, DS_RepresentantesParametros.GetInstance().GetAnularUltimaVenta()))
                        {
                            opciones.Add(AppResource.Revoke);
                        }
                        else if (DatosTransaction.Data.EstTabla.ToUpper() == "CAMBIOS" && myCam.GetAllCambiosMercanciaByCuadreByCuaSecuencia(CurrentTransaccion.TransaccionID))
                        {
                            opciones.Add(AppResource.Revoke);
                        }
                        else if (DatosTransaction.Data.EstTabla.ToUpper() == "DEVOLUCIONES")
                        {
                            var list = SqliteManager.GetInstance().Query<Devoluciones>("SELECT DevTipo FROM Devoluciones WHERE DevSecuencia = '" + tran.TransaccionID + "' AND RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "'", new string[] { });
                            if (list != null)
                            {
                                if (list[0].DevTipo.ToString() != "3")
                                {
                                    opciones.Add(AppResource.Revoke);
                                }
                            }
                        }
                        else if (DS_RepresentantesParametros.GetInstance().GetAnularUltimaVenta() <= 0)
                        {
                            if (DatosTransaction.Data.EstTabla.ToUpper() == "RECIBOS")
                            {
                                var list = SqliteManager.GetInstance().Query<Recibos>("SELECT DepSecuencia FROM Recibos WHERE RecSecuencia = '" + tran.TransaccionID + "' AND RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "'", new string[] { });
                                if (list != null)
                                {
                                    if ((string.IsNullOrWhiteSpace(list[0].DepSecuencia.ToString())) || list[0].DepSecuencia.ToString() == "0")
                                    {
                                        opciones.Add(AppResource.Revoke);
                                    }
                                }
                            }
                            else
                            {
                                opciones.Add(AppResource.Revoke);
                            }

                        }
                        else if (DS_RepresentantesParametros.GetInstance().GetAnularUltimaVenta() > 0
                                 && DatosTransaction.Data.EstTabla.ToUpper() != "VENTAS" && DatosTransaction.Data.EstTabla.ToUpper() != "CAMBIOS")
                             {
                                 opciones.Add(AppResource.Revoke);
                             }

                    }
                    if (estado.estOpciones.ToUpper().Contains("M"))
                    {
                        opciones.Add(AppResource.Share);
                    }
                    if (estado.estOpciones.ToUpper().Contains("C"))
                    {
                        opciones.Add(AppResource.CopyTransaction);
                    }
                    if (estado.estOpciones.ToUpper().Contains("P") && myParametro.GetParConvertirCotizacionesAPedidos() > 0)
                    {
                        opciones.Add(AppResource.ChangeCotToped);
                    }
                    if (estado.estOpciones.ToUpper().Contains("B") && myParametro.GetParConvertirCotizacionesAVentas())
                    {
                        opciones.Add(AppResource.ChangeCotToVen);
                    }
                    if (estado.estOpciones.ToUpper().Contains("R"))
                    {
                        opciones.Add("Tracking");
                    }
                    if (estado.EstSiguientesEstados != null && estado.estOpciones.ToUpper().Contains("S"))
                    {
                        opciones.Add(AppResource.ChangeStatus);
                    }
                    if (estado.estOpciones.ToUpper().Contains("T"))
                    {
                        opciones.Add(AppResource.ToRePeat);
                    }

                    Arguments.Values.CurrentModule = Functions.GetModuleByTableName(DatosTransaction.Data.EstTabla);
                    IsBusy = false;

                    var result = await DisplayActionSheet(AppResource.SelectDesiredOption, buttons: opciones.ToArray());


                if(result == AppResource.SeeDetail)
                {
                    if (DatosTransaction.Data.EstTabla.ToUpper() == "NCDPP")
                    {
                        await PushModalAsync(new NCDPPDetalleModal(tran.TransaccionID));
                    }
                    else
                    {
                        //Se carga el cliente aqui porque el metodo LoadProduct realiza un calculo de descuentos y este utiliza el CLIID.
                        //Ya no se recalculan las ofertas cuando se ve el detalle
                        // Arguments.Values.CurrentClient = myCli.GetClienteById(tran.CliID);
                        GoVerDetalleTransaccion();
                    }
                }
                else if(result == AppResource.Print)
                {
                    bool istoauthorize = false;

                    switch (DatosTransaction.TitId)
                    {
                        case 3:
                            if (myParametro.GetParRecPinAutorizacion())
                            {
                                istoauthorize = true;
                                await PushModalAsync(new AutorizacionesModal(false, 0, 3, "")
                                {
                                    OnAutorizacionUsed = (autSec) =>
                                    {
                                        ShowPrinter = true;
                                    }
                                });
                            }
                            break;
                        case 1:
                            if (myParametro.GetParPedPinAutorizacion())
                            {
                                istoauthorize = true;
                                await PushModalAsync(new AutorizacionesModal(false, 0, 1, "")
                                {
                                    OnAutorizacionUsed = (autSec) =>
                                    {
                                        ShowPrinter = true;
                                    }
                                });
                            }
                            break;
                        case 2:
                            if (myParametro.GetParDevPinAutorizacion())
                            {
                                istoauthorize = true;
                                await PushModalAsync(new AutorizacionesModal(false, 0, 2, "")
                                {
                                    OnAutorizacionUsed = (autSec) =>
                                    {
                                        ShowPrinter = true;
                                    }
                                });
                            }
                            break;
                    }

                    if (!istoauthorize)
                    {
                        ShowPrinter = true;
                    }
                }else if(result == AppResource.Edit)
                {
                    await GoEditarTransaccion();
                }else if(result == AppResource.Revoke)
                {
                    ShowAlertEstTransaccion(0);
                }else if(result == AppResource.Share)
                {
                    CompartirTransaccion();
                }else if(result == AppResource.CopyTransaction)
                {
                    await GoEditarTransaccion(true);
                }else if(result == AppResource.ChangeCotToped)
                {
                    switch (myParametro.GetParConvertirCotizacionesAPedidos())
                    {
                        case 1:
                            await GoEditarTransaccion(true, 1);
                            break;
                        case 2:
                            await GoEditarTransaccion(true, 2);
                            break;                       
                    }
                    
                }
                else if (result == AppResource.ChangeCotToVen)
                {
                    await GoEditarTransaccion(true, 1, true);

                }
                else if(result == "Tracking")
                {
                    GoTracking();
                }else if(result == AppResource.ChangeStatus)
                {
                    ShowCambiarEstado(tran);
                }else if(result == AppResource.ToRePeat)
                {
                    await ReTransmitirTransactionDataInTemp();
                }
            }catch(Exception e)
            {
                Crashes.TrackError(e);
                await DisplayAlert(AppResource.Warning, e.Message);
            }
        }

        public void ShowOrHideSecondSearch()
        {
            ShowSecondSearch = myParametro.GetParConsultaTrancacionBusquedaDiferente() == 2;
        }

        private async void ShowCambiarEstado(Transaccion tran)
        {
            var estado = DatosTransaction.Data; //si es preliminar, traer las opciones de preliminar:EstSiguientesEstados, utilizar GetByTablaAndEstatus
            var opciones = new List<string>();


            if (isForFecha)
            {
                estado = myEst.GetByTablaAndEstatus(DatosTransaction.Data.EstTabla, tran.Estatus);
            }

            if (estado == null || string.IsNullOrWhiteSpace(estado.estOpciones))
            {
                return;
            }

            if (estado.EstSiguientesEstados.Contains("[0]"))
            {
                if (DatosTransaction.Data.EstTabla.ToUpper() == "VENTAS" && DS_RepresentantesParametros.GetInstance().GetAnularUltimaVenta() > 0
                    && myVen.isVentaUltimoCuadre(CurrentTransaccion.TransaccionID, DS_RepresentantesParametros.GetInstance().GetAnularUltimaVenta()))
                {
                    opciones.Add(AppResource.Revoke);
                }
                else if (DS_RepresentantesParametros.GetInstance().GetAnularUltimaVenta() <= 0)
                {
                    if (DatosTransaction.Data.EstTabla.ToUpper() == "RECIBOS")
                    {
                        var list = SqliteManager.GetInstance().Query<Recibos>("SELECT DepSecuencia FROM Recibos WHERE RecSecuencia = '" + tran.TransaccionID + "' AND RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "'", new string[] { });
                        if (list != null)
                        {
                            if ((string.IsNullOrWhiteSpace(list[0].DepSecuencia.ToString())) || list[0].DepSecuencia.ToString() == "0")
                            {
                                opciones.Add(AppResource.Revoke);
                            }
                        }
                    }
                    else
                    {
                        opciones.Add(AppResource.Revoke);
                    }

                }
                else if (DS_RepresentantesParametros.GetInstance().GetAnularUltimaVenta() > 0 && DatosTransaction.Data.EstTabla.ToUpper() != "VENTAS")
                {
                    opciones.Add(AppResource.Revoke);
                }
            }

            if (estado.EstSiguientesEstados.Contains("[1]"))
            {
                opciones.Add(AppResource.TypedBySeller);
            }
            if (estado.EstSiguientesEstados.Contains("[2]"))
            {
                opciones.Add(AppResource.Transmitted);
            }
            if (estado.EstSiguientesEstados.Contains("[3]"))
            {
                opciones.Add(AppResource.Preliminary);
            }
            if (estado.EstSiguientesEstados.Contains("[4]"))
            {
                opciones.Add(AppResource.SentERP);
            }
            if (estado.EstSiguientesEstados.Contains("[5]"))
            {
                opciones.Add(AppResource.ERPRejected);
            }
            if (estado.EstSiguientesEstados.Contains("[7]"))
            {
                opciones.Add(AppResource.PendingAuthorization);
            }
            if (estado.EstSiguientesEstados.Contains("[8]"))
            {
                opciones.Add(AppResource.Rejected);
            }
            if (estado.EstSiguientesEstados.Contains("[9]"))
            {
                opciones.Add(AppResource.PendingDelivery);
            }
            if (estado.EstSiguientesEstados.Contains("[10]"))
            {
                opciones.Add(AppResource.InShippingProcess);
            }
            if (estado.EstSiguientesEstados.Contains("[11]"))
            {
                opciones.Add(AppResource.Invoiced);
            }
            if (estado.EstSiguientesEstados.Contains("[12]"))
            {
                opciones.Add(AppResource.Counting);
            }
            if (estado.EstSiguientesEstados.Contains("[13]"))
            {
                opciones.Add(AppResource.ReviewedByERP);
            }
            if (estado.EstSiguientesEstados.Contains("[14]"))
            {
                opciones.Add(AppResource.Authorized);
            }
            if (estado.EstSiguientesEstados.Contains("[15]"))
            {
                opciones.Add(AppResource.CanceledByCustomerService);
            }
            if (estado.EstSiguientesEstados.Contains("[16]"))
            {
                opciones.Add(AppResource.RejectedClient);
            }
            if (estado.EstSiguientesEstados.Contains("[20]"))
            {
                opciones.Add(AppResource.WaitingHANAResponse);
            }
            if (estado.EstSiguientesEstados.Contains("[21]"))
            {
                opciones.Add(AppResource.DelayResponseInSAP);
            }

            var result = await DisplayActionSheet(AppResource.SelectDesiredOption, buttons: opciones.ToArray());


            if(result == AppResource.Revoke)
            {
                ShowAlertEstTransaccion(0);
            }else if(result == AppResource.TypedBySeller)
            {
                ShowAlertEstTransaccion(1);
            }else if(result == AppResource.Transmitted)
            {
                ShowAlertEstTransaccion(2);
            }else if(result == AppResource.Preliminary)
            {
                ShowAlertEstTransaccion(3);
            }else if(result == AppResource.SentERP)
            {
                ShowAlertEstTransaccion(4);
            }else if(result == AppResource.ERPRejected)
            {
                ShowAlertEstTransaccion(5);
            }else if(result == AppResource.PendingAuthorization)
            {
                ShowAlertEstTransaccion(7);
            }else if(result == AppResource.Rejected)
            {
                ShowAlertEstTransaccion(8);
            }else if(result == AppResource.PendingDelivery)
            {
                ShowAlertEstTransaccion(9);
            }else if(result == AppResource.InShippingProcess)
            {
                ShowAlertEstTransaccion(10);
            }else if(result == AppResource.Invoiced)
            {
                ShowAlertEstTransaccion(11);
            }else if(result == AppResource.Counting)
            {
                ShowAlertEstTransaccion(12);
            }else if(result == AppResource.ReviewedByERP)
            {
                ShowAlertEstTransaccion(13);
            }else if(result == AppResource.Authorized)
            {
                ShowAlertEstTransaccion(14);
            }else if(result == AppResource.CanceledByCustomerService)
            {
                ShowAlertEstTransaccion(15);
            }else if(result == AppResource.RejectedClient)
            {
                ShowAlertEstTransaccion(16);
            }else if(result == AppResource.WaitingHANAResponse)
            {
                ShowAlertEstTransaccion(20);
            }else if(result == AppResource.DelayResponseInSAP)
            {
                ShowAlertEstTransaccion(21);
            }

        }

        private async void GoTracking()
        {
            try
            {

                await PushModalAsync(new TransaccionesTrackingModal(CurrentTransaccion, DatosTransaction));

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }
        }

       
        private void GoVerDetalleTransaccion()
        {
            try
            {
                var title = "";

                var transaccion = DatosTransaction.Data.EstTabla.ToUpper();

                bool VerDetalle = transaccion == "PEDIDOS" || transaccion == "PEDIDOSCONFIRMADOS"
                               || transaccion == "VENTAS" || transaccion == "VENTASCONFIRMADOS"
                               || transaccion == "COMPRAS" || transaccion == "COMPRASCONFIRMADOS"
                               || transaccion == "COMPRASPUSHMONEY" || transaccion == "COMPRASPUSHMONEYCONFIRMADOS"
                               || transaccion == "CAMBIOS" || transaccion == "CAMBIOSCONFIRMADOS"
                               || transaccion == "CONTEOSFISICOS" || transaccion == "CONTEOSFISICOSCONFIRMADOS"
                               || transaccion == "DEVOLUCIONES" || transaccion == "DEVOLUCIONESCONFIRMADAS" || transaccion == "DEVOLUCIONESCONFIRMADAS"
                               || transaccion == "TRANSFERENCIASALMACENES" || transaccion == "INVENTARIOFISICO"
                               || transaccion == "COTIZACIONES" || transaccion == "COTIZACIONESCONFIRMADOS"
                               || transaccion == "COLOCACIONPRODUCTOS";

                if (transaccion != "RECIBOS")
                {
                    title = InsertTransactionDataInTemp(false,true, CurrentTransaccion.RepCodigo);
                    Arguments.Values.CurrentClient = myCli.GetClienteById(CurrentTransaccion.CliID);
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
                }               

                if (transaccion == "PEDIDOS" || transaccion == "PEDIDOSCONFIRMADOS"
                    || transaccion == "DEVOLUCIONES" || transaccion == "DEVOLUCIONESCONFIRMADAS" ||  transaccion == "DEVOLUCIONESCONFIRMADAS"
                    || transaccion == "COMPRAS" || transaccion == "COMPRASCONFIRMADOS"
                    || transaccion == "COMPRASPUSHMONEY" || transaccion == "COMPRASPUSHMONEYCONFIRMADOS"
                    || transaccion == "COTIZACIONES" || transaccion == "COTIZACIONESCONFIRMADOS"
                    || transaccion == "VENTAS" || transaccion == "VENTASCONFIRMADOS"
                    || transaccion == "CONTEOSFISICOS" || transaccion == "CONTEOSFISICOSCONFIRMADOS"
                    || transaccion == "CAMBIOS" || transaccion == "CAMBIOSCONFIRMADOS"
                    || transaccion == "TRANSFERENCIASALMACENES" || transaccion == "INVENTARIOFISICO"
                    || transaccion == "COLOCACIONPRODUCTOS")
                {
                    if (transaccion == "DEVOLUCIONES" || transaccion == "DEVOLUCIONESCONFIRMADAS")
                        Arguments.Values.CurrentModule = Modules.DEVOLUCIONES;


                    var pedido = myPed.GetBySecuencia(CurrentTransaccion.TransaccionID, transaccion == "PEDIDOSCONFIRMADOS");
                    var venta = myVen.GetBySecuencia(CurrentTransaccion.TransaccionID, transaccion == "VENTASCONFIRMADOS");
                    var cotizacion = myCot.GetBySecuencia(CurrentTransaccion.TransaccionID, transaccion == "COTIZACIONESCONFIRMADOS");

                    var args = new PedidosDetalleArgs
                    {
                        FechaEntrega = DateTime.Now,
                        ConId = transaccion == "PEDIDOSCONFIRMADOS" || transaccion == "PEDIDOS" ? pedido.ConID 
                        : transaccion == "VENTASCONFIRMADOS" || transaccion == "VENTAS" ? venta.ConID 
                        : transaccion == "COTIZACIONESCONFIRMADOS" || transaccion == "COTIZACIONES" ? cotizacion.ConID 
                        : 0,
                        DisenoDelRow = myParametro.GetFormatoVisualizacionProductos(),
                        PedOrdenCompra = null,
                        IsEditing = true,
                    };
                 
                    if(transaccion == "PEDIDOSCONFIRMADOS" && myParametro.GetParTabbedPageEstados())
                    {
                        PushAsync(new PedidosConfirmadosTap(pedido, args, VerDetalle));
                        IsBusy = false;
                        return;
                    }
                    PushAsync(new PedidosDetallePage(args, VerDetalle) { Title = title, IsDetail = true });
                }else if(transaccion == "CLIENTES" && int.TryParse(TraSecuencia, out int cliId))
                {
                    var cliente = myCli.GetClienteById(cliId);

                    if(cliente != null)
                    {
                       // PushAsync(new ProspectosTabPage(cliente));
                    }
                    
                }else if (transaccion == "RECIBOS" || transaccion == "RECIBOSCONFIRMADOS")
                {
                    PushAsync(new ConsultaRecibosModal(CurrentTransaccion.TransaccionID, CurrentTransaccion.DataField, CurrentTransaccion.CliID, transaccion == "RECIBOSCONFIRMADOS"));
                }else if (transaccion == "GASTOS" || transaccion == "GASTOSCONFIRMADOS")
                {
                    GoDetalleGastos();
                }else if (transaccion == "ENTREGASTRANSACCIONES" || transaccion == "ENTREGASTRANSACCIONESCONFIRMADOS" || transaccion == "CONDUCES" || transaccion == "CONDUCESCONFIRMADOS")
                {
                    Arguments.Values.CurrentModule = (transaccion == "CONDUCES" || transaccion == "CONDUCESCONFIRMADOS") ? Modules.CONDUCES : Modules.ENTREGASREPARTIDOR;
                    if (Arguments.Values.CurrentModule == Modules.CONDUCES)
                    {
                        var conduce = myCond.GetBySecuencia (CurrentTransaccion.TransaccionID, transaccion == "CONDUCESCONFIRMADOS");

                        PushAsync(new EntregasRepartidorDetalleRevisionPage(new EntregasRepartidorTransacciones()
                        {
                            EnrSecuencia = conduce.ConSecuencia,
                            TraSecuencia = conduce.ConSecuencia,
                            TitID = 51,
                            CliID = conduce.SupID
                        }, true));
                    }
                    else
                    {
                        var entrega = myEntRep.GetEntregaTransaccion(CurrentTransaccion.TransaccionID, transaccion == "ENTREGASTRANSACCIONESCONFIRMADOS");

                        PushAsync(new EntregasRepartidorDetalleRevisionPage(new EntregasRepartidorTransacciones()
                        {
                            EnrSecuencia = entrega.EntSecuencia,
                            TraSecuencia = 0,//entrega.VenSecuencia,
                            TitID = entrega.TitID,
                            CliID = entrega.CliID
                        }, true));
                    }

                }
                else if (transaccion == "ENTREGASDOCUMENTOSCONFIRMADOS" || transaccion == "ENTREGASDOCUMENTOS")
                {
                    Arguments.Values.CurrentModule = Modules.ENTREGADOCUMENTOS;

                    var entrega = myEnt.GetEntregaBySecuencia(CurrentTransaccion.TransaccionID, transaccion == "ENTREGASTRANSACCIONESCONFIRMADOS");

                    PushAsync(new EntregasDocumentosPage(new EntregasDocumentos()
                    {
                        EntSecuencia = entrega.EntSecuencia,
                        CliID = entrega.CliID
                    }, true, transaccion == "ENTREGASTRANSACCIONESCONFIRMADOS" ? true : false));
                }
                else if(transaccion == "CARGAS")
                {
                    PushAsync(new CargasPage(CurrentTransaccion.TransaccionID));
                }else if(transaccion == "QUEJASSERVICIO")
                {
                    PushAsync(new QuejasServicioPage(CurrentTransaccion.TransaccionID, true));
                }else if (transaccion == "ENTREGASREPARTIDOR" || transaccion == "ENTREGASREPARTIDORCONFIRMADOS")
                {
                    PushAsync(new ConsultaEntregasRepartidorPage(CurrentTransaccion.TransaccionID, transaccion == "ENTREGASREPARTIDORCONFIRMADOS"));
                }
                else if(transaccion ==  "DEPOSITOS" || transaccion == "DEPOSITOSCONFIRMADOS")
                {
                    PushAsync(new DepositosPage(isSociedad: myParametro.GetParDepositosPorSociedad(), depsecuencia: CurrentTransaccion.TransaccionID, isconfirmado: transaccion == "DEPOSITOSCONFIRMADOS"));
                }
                else if(transaccion ==  "DEPOSITOSCOMPRAS" || transaccion == "DEPOSITOSCOMPRASCONFIRMADOS")
                {
                    PushModalAsync(new DepositoComprasModal(CurrentTransaccion.TransaccionID));
                }
                else if(transaccion == "PUSHMONEYPAGOS")
                {
                    PushAsync(new PushMoneyPorPagarPage(CurrentTransaccion.TransaccionID));
                }else if(transaccion == "MUESTRAS")
                {
                    PushModalAsync(new DetalleMuestrasModal(CurrentTransaccion.TransaccionID));
                }
            }
            catch (Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
                Crashes.TrackError(e);
            }
        }

        private void GoDetalleGastos()
        {
            var gasto = mygas.GetGastoBysecuencia(CurrentTransaccion.TransaccionID, DatosTransaction.Data.EstTabla == "GASTOSCONFIRMADOS");

            PushAsync(new AgregarGastosModal(new DS_TransaccionesImagenes(), gasto, true));
        }

        private string InsertTransactionDataInTemp(bool forEditing, bool IsFromCopy, string repcodigo)
        {
            var title = "";
            switch (DatosTransaction.Data.EstTabla.ToUpper())
            {
                case "PEDIDOS":
                    myPed.InsertarPedidoInTemp(CurrentTransaccion.TransaccionID, false, repcodigo, forEditing);
                    title = "Pedidos detalle";
                    if (forEditing)
                    {
                        Arguments.Values.CurrentModule = Modules.PEDIDOS;
                    }
                    break;
                case "VENTAS":
                case "VENTASCONFIRMADOS":
                    myVen.InsertarVentaInTemp(CurrentTransaccion.TransaccionID, DatosTransaction.Data.EstTabla.ToUpper() == "VENTASCONFIRMADOS", forEditing);
                    title = "Ventas detalle";
                    if (forEditing)
                    {
                        Arguments.Values.CurrentModule = Modules.VENTAS;
                    }
                    break;
                case "PEDIDOSCONFIRMADOS":
                    if (IsFromCopy)
                    {
                        myPed.InsertarPedidoInTemp(CurrentTransaccion.TransaccionID, true, repcodigo, IsFromCopy);
                        title = "Pedidos detalle";
                        Arguments.Values.CurrentModule = Modules.PEDIDOS;
                    }
                    break;
                case "DEVOLUCIONES":
                    myDev.InsertarDevolucionInTemp(CurrentTransaccion.TransaccionID, false);
                    title = "Devoluciones detalle";
                    if (forEditing)
                    {
                        Arguments.Values.CurrentModule = Modules.DEVOLUCIONES;
                    }
                    break;
                case "DEVOLUCIONESCONFIRMADAS":
                    if (!forEditing)
                    {
                        myDev.InsertarDevolucionInTemp(CurrentTransaccion.TransaccionID, true);
                        title = "Devoluciones detalle";
                    }
                    break;
                case "COMPRAS":
                    myCom.InsertarCompraInTemp(CurrentTransaccion.TransaccionID, false);
                    title = "Compras detalle";
                    if (forEditing)
                    {
                        Arguments.Values.CurrentModule = Modules.COMPRAS;
                    }
                    break;
                case "COMPRASCONFIRMADOS":
                    myCom.InsertarCompraInTemp(CurrentTransaccion.TransaccionID, true);
                    title = "Compras detalle";
                    if (forEditing)
                    {
                        Arguments.Values.CurrentModule = Modules.COMPRAS;
                    }
                    break;
                case "COMPRASPUSHMONEY":
                    myCom.InsertarCompraInTemp(CurrentTransaccion.TransaccionID, false);
                    title = "ComprasPushMoney detalle";
                    if (forEditing)
                    {
                        Arguments.Values.CurrentModule = Modules.COMPRAS;
                    }
                    break;
                case "RECIBOS":
                    myRec.InsertReciboInTemp(CurrentTransaccion.TransaccionID, forEditing);
                    if (forEditing)
                    {
                        Arguments.Values.CurrentModule = Modules.COBROS;
                    }
                    break;
                case "COTIZACIONES":
                    myCot.InsertarCotizacionInTemp(CurrentTransaccion.TransaccionID, false);
                    title = "Cotizaciones detalle";
                    if (forEditing)
                    {
                        Arguments.Values.CurrentModule = Modules.COTIZACIONES;
                    }
                    break;
                case "COTIZACIONESCONFIRMADOS":
                    myCot.InsertarCotizacionInTemp(CurrentTransaccion.TransaccionID, true);
                    title = "Cotizaciones detalle";
                    if (forEditing)
                    {
                        Arguments.Values.CurrentModule = Modules.COTIZACIONES;
                    }
                    break;
                case "CAMBIOS":
                case "CAMBIOSCONFIRMADOS":
                    myCam.InsertarCambiosInTemp(CurrentTransaccion.TransaccionID, DatosTransaction.Data.EstTabla.ToUpper() == "CAMBIOSCONFIRMADOS");
                    title = "Cambios detalle";
                    if (forEditing)
                    {
                        Arguments.Values.CurrentModule = Modules.CAMBIOSMERCANCIA;
                    }
                    break;
                case "ENTREGASTRANSACCIONES":
                case "ENTREGASTRANSACCIONESCONFIRMADOS":
                    title = "Entregas Repartidor Detalle";
                    myEntRep.InsertProductInTempForDetail(CurrentTransaccion.TransaccionID, DatosTransaction.Data.EstTabla.ToUpper().Equals("ENTREGASTRANSACCIONESCONFIRMADOS"));
                    break;
                case "CONDUCES":
                case "CONDUCESCONFIRMADOS":
                    title = "Conduces Detalle";
                    myCond.InsertProductInTempForDetailConPrecioYFactura(CurrentTransaccion.TransaccionID, DatosTransaction.Data.EstTabla.ToUpper().Equals("CONDUCESCONFIRMADOS"));
                    break;
                case "TRANSFERENCIASALMACENES":
                    title = "Traspasos detalle";
                    myTraAlm.InsertarTraspasoInTemp(CurrentTransaccion.TransaccionID, false);
                    break;
                case "INVENTARIOFISICO":
                    myinvfis.InsertarInventarioFisicoInTemp(CurrentTransaccion.TransaccionID, false);
                    title = "Inventario fisico detalle";
                    if (forEditing)
                    {
                        Arguments.Values.CurrentModule = Modules.INVFISICO;
                    }
                    break;
                case "COLOCACIONPRODUCTOS":
                    myColProd.InsertarColocacionInTemp(CurrentTransaccion.TransaccionID);
                    title = "Colocacion de mercancias detalle";
                    if (forEditing)
                    {
                        Arguments.Values.CurrentModule = Modules.COLOCACIONMERCANCIAS;
                    }
                    break;
                case "CONTEOSFISICOS":
                case "CONTEOSFISICOSCONFIRMADOS":
                    myCon.InsertarConteoInTemp(CurrentTransaccion.TransaccionID, DatosTransaction.Data.EstTabla.ToUpper() == "CONTEOSFISICOSCONFIRMADOS");
                    title = "Conteos fisicos detalle";
                    if (forEditing)
                    {
                        Arguments.Values.CurrentModule = Modules.CONTEOSFISICOS;
                    }
                    break;
            }

            return title;
        }
        

        private async Task ReTransmitirTransactionDataInTemp()
        {
            //switch (DatosTransaction.Data.EstTabla.ToUpper())
            //{
            //    case "PEDIDOS":
            //        Functions.RetransmitirTransacciones(new Pedidos(),CurrentTransaccion.TransaccionID);
            //        break;
            //    case "PEDIDOSCONFIRMADOS":
            //        Functions.RetransmitirTransacciones(new Pedidos(), CurrentTransaccion.TransaccionID, true);
            //        break;
            //    case "VENTAS":
            //        Functions.RetransmitirTransacciones(new Ventas(), CurrentTransaccion.TransaccionID);
            //        break;
            //    case "VENTASCONFIRMADOS":
            //        Functions.RetransmitirTransacciones(new Ventas(), CurrentTransaccion.TransaccionID, true);
            //        break;
            //    case "DEVOLUCIONES":
            //        Functions.RetransmitirTransacciones(new Devoluciones(), CurrentTransaccion.TransaccionID);
            //        break;
            //    case "DEVOLUCIONESCONFIRMADAS":
            //        Functions.RetransmitirTransacciones(new Devoluciones(), CurrentTransaccion.TransaccionID, true);
            //        break;
            //    case "COMPRAS":
            //        Functions.RetransmitirTransacciones(new Compras(), CurrentTransaccion.TransaccionID);
            //        break;
            //    case "COMPRASPUSHMONEY":
            //        Functions.RetransmitirTransacciones(new PushMoneyPagos(), CurrentTransaccion.TransaccionID);
            //        break;
            //    case "RECIBOS":
            //        Functions.RetransmitirTransacciones(new Recibos(), CurrentTransaccion.TransaccionID);
            //        break;
            //    case "COTIZACIONES":
            //        Functions.RetransmitirTransacciones(new Cotizaciones(), CurrentTransaccion.TransaccionID);
            //        break;
            //    case "COTIZACIONESCONFIRMADOS":
            //        Functions.RetransmitirTransacciones(new Cotizaciones(), CurrentTransaccion.TransaccionID, true);
            //        break;
            //    case "CAMBIOS":
            //        Functions.RetransmitirTransacciones(new Cambios(), CurrentTransaccion.TransaccionID);
            //        break;
            //    case "CAMBIOSCONFIRMADOS":
            //        Functions.RetransmitirTransacciones(new Cambios(), CurrentTransaccion.TransaccionID, true);
            //        break;
            //    case "ENTREGASTRANSACCIONES":
            //        Functions.RetransmitirTransacciones(new EntregasTransacciones(), CurrentTransaccion.TransaccionID);
            //        break;
            //    case "ENTREGASTRANSACCIONESCONFIRMADOS":
            //        Functions.RetransmitirTransacciones(new EntregasTransacciones(), CurrentTransaccion.TransaccionID, true);
            //        break;
            //    case "CONDUCES":
            //        Functions.RetransmitirTransacciones(new Conduces(), CurrentTransaccion.TransaccionID);
            //        break;
            //    case "CONDUCESCONFIRMADOS":
            //        Functions.RetransmitirTransacciones(new Conduces(), CurrentTransaccion.TransaccionID, true);
            //        break;
            //    case "TRANSFERENCIASALMACENES":
            //        Functions.RetransmitirTransacciones(new TransferenciasAlmacenes(), CurrentTransaccion.TransaccionID);
            //        break;
            //    case "INVENTARIOFISICO":
            //        Functions.RetransmitirTransacciones(new InventarioFisico(), CurrentTransaccion.TransaccionID);
            //        break;
            //    case "CONTEOSFISICOS":
            //        Functions.RetransmitirTransacciones(new ConteosFisicos(), CurrentTransaccion.TransaccionID);
            //        break;
            //    case "CONTEOSFISICOSCONFIRMADOS":
            //        Functions.RetransmitirTransacciones(new ConteosFisicos(), CurrentTransaccion.TransaccionID, true);
            //        break;
            //}

            var type = Assembly.GetExecutingAssembly().GetTypes()
                .FirstOrDefault(t => t.IsClass &&  t.Name.ToUpper().Contains(DatosTransaction.Data.EstTabla.ToUpper()) 
                && t.Name.Length == DatosTransaction.Data.EstTabla.Length);

            var instance = Activator.CreateInstance(type);
            Functions.RetransmitirTransacciones(instance, CurrentTransaccion.TransaccionID, type.Name.ToUpper().Contains("CONFIRMADOS"));

            await PushModalAsync(new SincronizarModal());
        }

        private async Task GoEditarTransaccion(bool FromCopiar=false,int FromConver = -1, bool IsCot2Ven = false)
        {
            try
            {
                if(IsBusy)
                {
                    return;
                }

                IsBusy = true;
                int codicionPago = -1;
                bool isFromCot2Ven = false;

                switch (FromConver)
                {
                    case 1:
                        if (IsCot2Ven)
                        {
                            
                            if (myParametro.GetParUsarMultiAlmacenes() && myParametro.GetParAlmacenVentaRanchera() == -1)
                            {
                                throw new Exception("No tienes definido el almacén de venta ranchera");
                            }

                            if (!DS_Representantes.HasNCF(Arguments.CurrentUser.RepCodigo))
                            {
                                throw new Exception(AppResource.UserHasNotReceiptTypeDefined);
                            }

                            Arguments.Values.CurrentClient = myCli.GetClienteById(CurrentTransaccion.CliID);
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
                            if (!myCli.ClienteTieneNcfValido(Arguments.Values.CurrentClient))
                            {
                                throw new Exception(AppResource.CustomerHasNotValidReceiptType);
                            }

                            codicionPago = myCot.GetBySecuencia(CurrentTransaccion.TransaccionID, DatosTransaction.Data.EstTabla.ToUpper() == "COTIZACIONESCONFIRMADOS").ConID;
                            isFromCot2Ven = true;
                            ConvertirCotizacionAVentas();
                            
                        }
                        else
                        {
                            ConvertirCotizacionApedido();
                        }
                        break;
                    case 2:
                        Arguments.Values.CurrentClient = myCli.GetClienteById(CurrentTransaccion.CliID);
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
                        ConvertirCotizacionApedidoAut();
                        await PushAsync(new SuccessPage(AppResource.Orders, pedSecuenciaAut));
                        IsBusy = false;
                        return;
                    default:
                        InsertTransactionDataInTemp(true, FromCopiar, CurrentTransaccion.RepCodigo);
                        break;
                }
          
                if (DatosTransaction.Data.EstTabla.ToUpper() == "PEDIDOS"
                    || DatosTransaction.Data.EstTabla.ToUpper() == "PEDIDOSCONFIRMADOS"
                    || (DatosTransaction.Data.EstTabla.ToUpper() == "DEVOLUCIONES" && !FromCopiar)
                    || DatosTransaction.Data.EstTabla.ToUpper() == "COMPRAS"
                    || DatosTransaction.Data.EstTabla.ToUpper() == "COMPRASPUSHMONEY"
                    || DatosTransaction.Data.EstTabla.ToUpper() == "COTIZACIONES"
                    || DatosTransaction.Data.EstTabla.ToUpper() == "COTIZACIONESCONFIRMADOS"
                    || DatosTransaction.Data.EstTabla.ToUpper() == "CAMBIOS"
                    || DatosTransaction.Data.EstTabla.ToUpper() == "INVENTARIOFISICO")
                {
                    Arguments.Values.CurrentClient = myCli.GetClienteById(CurrentTransaccion.CliID);
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

                    if (DatosTransaction.Data.EstTabla.ToUpper() != "COMPRAS"
                      || DatosTransaction.Data.EstTabla.ToUpper() != "CAMBIOS"
                      || DatosTransaction.Data.EstTabla.ToUpper() != "COMPRASPUSHMONEY"
                      || DatosTransaction.Data.EstTabla.ToUpper() != "INVENTARIOFISICO"
                      || DatosTransaction.Data.EstTabla.ToUpper() != "DEVOLUCIONES")
                    {
                        var task = new TaskLoader() { SqlTransactionWhenRun = true };
                        await task.Execute(() =>
                        {
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
                        });
                    }

                    if (myParametro.GetParSectores() > 0)
                    {
                        Sectores = new DS_Sectores().GetSectoresByCliente(Arguments.Values.CurrentClient.CliID, CurrentTransaccion.Seccodigo);

                        if (Sectores != null && Sectores.Count > 0)
                        {
                            SectorActual = Sectores[0];
                        }
                    }

                    if (Arguments.Values.CurrentClient == null)
                    {
                        return;
                    }

                    if (myParametro.GetParAutorizacionEditarDevolucion() && DatosTransaction.Data.EstTabla.ToUpper().Equals("DEVOLUCIONES") && !FromCopiar)
                    {
                        await PushModalAsync(new AutorizacionesModal(false, CurrentTransaccion.TransaccionID, 2, "", false)
                        {
                            OnAutorizacionUsed =  (autSec) =>
                            {
                                 PushAsync(new PedidosPage(!FromCopiar, CurrentTransaccion.TransaccionID, FromCopiar));
                            }
                        });
                    }
                    else
                    {
                        await PushAsync(new PedidosPage(!FromCopiar, CurrentTransaccion.TransaccionID, FromCopiar, isFromCot2Ven: isFromCot2Ven, conId: codicionPago));
                    }
                }
                else if(DatosTransaction.Data.EstTabla.ToUpper() == "RECIBOS" && DatosTransaction.Data.EstEstado=="3")
                {
                    Arguments.Values.CurrentClient = myCli.GetClienteById(CurrentTransaccion.CliID);
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

                    var Monedas = myRec.GetMonedaReciboOrdenPago(CurrentTransaccion.TransaccionID, myParametro.GetParTasaDelDiaODelRecibo() > -1 ? myParametro.GetParTasaDelDiaODelRecibo() : 1);


                    bool IsEditing = true;
                    await PushAsync(new RecibosTabPageOP(CurrentTransaccion.TransaccionID, (Monedas != null ? Monedas : null), IsEditing));
                }else if(DatosTransaction.Data.EstTabla.ToUpper() == "GASTOS")
                {
                    var gasto = mygas.GetGastoBysecuencia(CurrentTransaccion.TransaccionID, false);
                    await PushAsync(new AgregarGastosModal(new DS_TransaccionesImagenes(), gasto, false, true) { OnEditarGasto = ()=> { LoadTransaccionesAsync();  } });
                }
            }
            catch (Exception e)
            {
                 await DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
                Crashes.TrackError(e);
            }

            IsBusy = false;
        }

        private async void ShowAlertEstTransaccion(int est)
        {
            var result = await DisplayAlert(AppResource.Warning, AppResource.WantChangeStatusQuestion, AppResource.Modify, AppResource.Cancel);

            if (result)
            {
                  if (myParametro.GetParAutorizacionAnularRecibo() && DatosTransaction.Data.EstTabla.ToUpper().Equals("RECIBOS"))
                  {
                      await PushModalAsync(new AutorizacionComentarioModal(false, CurrentTransaccion.TransaccionID, 3, "", false)
                      {
                          OnAutorizacionUsed = (autSec) =>
                          {                      
                              EstTransaccion(est);
                          }
                      });
                  }
                  else if (myParametro.GetParAutorizacionAnularDevolucion() && DatosTransaction.Data.EstTabla.ToUpper().Equals("DEVOLUCIONES"))
                  {
                    await PushModalAsync(new AutorizacionesModal(false, CurrentTransaccion.TransaccionID, 2, "", false)
                    {
                        OnAutorizacionUsed = (autSec) =>
                        {
                            EstTransaccion(est);
                        }
                    });
                  }
                  else if (myParametro.GetParAutorizacionAnularVenta() && DatosTransaction.Data.EstTabla.ToUpper().Equals("VENTAS"))
                  {
                      await PushModalAsync(new AutorizacionesModal(false, CurrentTransaccion.TransaccionID, 4, "", false)
                      {
                          OnAutorizacionUsed = (autSec) =>
                          {
                              EstTransaccion(est);
                          }
                      });
                  }
                  else if (myParametro.GetParAutorizacionAnularVentaComentarioObligatorio() && DatosTransaction.Data.EstTabla.ToUpper().Equals("VENTAS"))
                  {
                      await PushModalAsync(new AutorizacionComentarioModal(false, CurrentTransaccion.TransaccionID, 4, "", false)
                      {
                          OnAutorizacionUsed = (autSec) =>
                          {
                              EstTransaccion(est);
                          }
                      });
                  }
                  else
                  {
                      EstTransaccion(est);
                  }
             }
        }
        
        private async void EstTransaccion(int est)
        {
            var task = new TaskLoader() { SqlTransactionWhenRun = true };
            try
            {
                IsBusy = true;

                var rowguid = Functions.GetrowguidTransaccion(DatosTransaction.Data.EstTabla.ToUpper(), (DatosTransaction.Data.EstTabla.ToUpper() == "PUSHMONEYPAGOS"? DatosTransaction.Data.EstTabla.ToUpper().Substring(0,3) : DatosTransaction.Data.EstTabla.ToUpper().Substring(0, 3).Replace("PUS", "Com")) + "Secuencia",CurrentTransaccion.TransaccionID);

                await task.Execute(() =>
                {
                    switch (DatosTransaction.Data.EstTabla.ToUpper())
                    {
                        case "PEDIDOS":
                            myPed.EstPedido(rowguid,est);
                            break;
                        case "DEVOLUCIONES":
                            myDev.EstDevolucion(rowguid, est);
                            break;
                        case "RECIBOS":
                            myRec.EstRecibos(/*CurrentTransaccion.TransaccionID, CurrentTransaccion.DataField*/rowguid,est);
                            break;
                        case "PUSHMONEYPAGOS":
                            myPxpRec.EstRecibos(rowguid,est);
                            break;
                        case "DEPOSITOS":
                            myDep.EstDepositos(CurrentTransaccion.TransaccionID, rowguid, est);
                            break;
                        case "DEPOSITOSCOMPRAS":
                            myDepCom.EstDepositos(rowguid, est);
                            break;
                        case "VENTAS":
                            myVen.EstVenta(CurrentTransaccion.TransaccionID, est);
                            break;
                        case "COMPRAS":
                            if(myParametro.GetParComprasNoAnularSiFueDepositado() && myCom.CompraFueDepositada(CurrentTransaccion.TransaccionID))
                            {
                                DisplayAlert(AppResource.Warning, AppResource.CannotChangePurchaseStatusHasBeenDeposited);
                                return;
                            }
                            myCom.EstCompra(rowguid, est);
                            break;
                        case "COMPRASPUSHMONEY":
                            if(myParametro.GetParComprasNoAnularSiFueDepositado() && myCom.CompraFueDepositada(CurrentTransaccion.TransaccionID))
                            {
                                DisplayAlert(AppResource.Warning, AppResource.CannotChangePushMoneyRotationStatusHasBeenDeposited);
                                return;
                            }
                            myCom.EstCompra(rowguid, est);
                            break;
                        case "COTIZACIONES":
                            myCot.EstCotizacion(rowguid, est);
                            break;
                        case "CAMBIOS":
                            myCam.EstCambios(CurrentTransaccion.TransaccionID, est, rowguid);
                            break;
                        case "ENTREGASDOCUMENTOS":
                            myEnt.EstEntregaDocumento(rowguid, est);
                            break;
                        case "CONDUCES":
                            myCond.EstConduce(CurrentTransaccion.TransaccionID, rowguid, est);
                            break;
                        case "INVENTARIOFISICO":
                            myinvfis.EstInventarioFisico(rowguid, est);
                            break;
                        case "CONTEOSFISICOS":
                            myCon.EstConteo(rowguid, est);
                            break;
                        case "AUDITORIASPRECIOS":
                            myAudPre.EstAuditoria(rowguid, est);
                            break;
                        case "COLOCACIONPRODUCTOS":
                            myColProd.EstColocacion(rowguid, est);
                            break;
                    }
                });

                LoadTransaccionesAsync();

            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                await DisplayAlert(AppResource.Error, e.Message, AppResource.Aceptar);
            }
            IsBusy = false;
        }

        private async void CompartirTransaccion()
        {
            try
            {
                IsBusy = true;

                var shareDialog = DependencyService.Get<IShareDialog>();
                IPdfGenerator manager = null;

                var filePath = "";
                bool confirmado = DatosTransaction.Data.EstTabla.ToUpper().Trim().Contains("CONFIRMADOS");
                var title = "";


                string[] clinombre = CurrentTransaccion.TransacionDescripcion.Split('-');

                switch (DatosTransaction.Data.EstTabla.ToUpper().Trim())
                {
                    case "PEDIDOS":
                    case "PEDIDOSCONFIRMADOS":
                        manager = new PdfPedidos(CurrentTransaccion.Seccodigo);
                        title = "Pedido No. ";
                        break;
                    case "RECIBOS":
                    case "RECIBOSCONFIRMADOS":
                        manager = new PdfRecibos(CurrentTransaccion.Seccodigo);
                        title = "Recibo No. ";
                        break;
                    case "VENTAS":
                    case "VENTASCONFIRMADOS":
                        manager = new PdfVentas(CurrentTransaccion.Seccodigo);
                        title = "Venta No. ";
                        break;
                    case "COMPRAS":
                    case "COMPRASCONFIRMADOS":
                        title = "Compra No. ";
                        manager = new PdfCompras(myCom, CurrentTransaccion.Seccodigo);
                        break;
                    case "COMPRASPUSHMONEY":
                    case "COMPRASPUSHMONEYCONFIRMADOS":
                        title = "Compra No. ";
                        manager = new PdfCompras(myCom, CurrentTransaccion.Seccodigo);
                        break;
                    case "COTIZACIONES":
                    case "COTIZACIONESCONFIRMADOS":
                        title = "Cotizacion No. ";
                        manager = new PdfCotizaciones(myCot, CurrentTransaccion.Seccodigo);
                        break;
                    case "DEVOLUCIONES":
                    case "DEVOLUCIONESCONFIRMADAS":
                        manager = new PdfDevoluciones(new DS_Devoluciones(), CurrentTransaccion.Seccodigo);
                        break; 
                    case "NCDPP":
                        manager = new PdfRecibos();
                        break;
                    case "INVENTARIOFISICO":
                        manager = new PdfInventariosFsicos(CurrentTransaccion.Seccodigo);
                        title = clinombre[1] + " - " + CurrentTransaccion.CliID + "  Inventario Fisico No. ";
                        break;
                    case "CONTEOSFISICOS":
                    case "CONTEOSFISICOSCONFIRMADOS":
                        manager = new PdfConteosFisicos(CurrentTransaccion.Seccodigo);
                        title = "Conteo Fisico No. ";
                        break;
                    case "DEPOSITOS":
                    case "DEPOSITOCONFIRMADOS":
                        manager = new PdfDepositos();
                        title = "Deposito No. ";
                        break;
                }

                if(manager == null)
                {
                    throw new Exception(AppResource.ErrorProcessing);
                }

                if(DatosTransaction.Data.EstTabla.ToUpper().Trim()== "NCDPP")
                {
                    filePath = await new PdfRecibos().GenerateNCDPPPdf(CurrentTransaccion.TransaccionID, CurrentTransaccion.TransaccionIDNCDPP);
                }
                else 
                { 
                    filePath = await manager.GeneratePdf(CurrentTransaccion.TransaccionID, confirmado); 
                }
            

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    throw new Exception(AppResource.ErrorGeneratingPdfFile);
                }

                await shareDialog.Show(AppResource.Share, title + CurrentTransaccion.TransaccionID + " - "+AppResource.UserLabel + Arguments.CurrentUser.RepCodigo, filePath);
            }
            catch(Exception e)
            {
                Crashes.TrackError(e);
                await DisplayAlert(AppResource.Error, e.Message);
            }

            IsBusy = false;
        }

        private void OnFilterValueSelected()
        {
            try
            {
                if (CurrentFiltro != null)
                {
                    ShowSecondFilter = true;
                    SecondFiltroSource = Functions.DinamicQuery(CurrentFiltro.FilComboSelect);

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
                }
            }
            catch (Exception e)
            {
                DisplayAlert(AppResource.ErrorLoadingFilters, e.Message);
            }
        }

        private void PrintNC(int transaccionID, PrinterManager printer, FormatosRecibos formats)
        {
            var ncs = myRec.GetNCDppRecibos(transaccionID);

            foreach (var NC in ncs)
            {
                IsBusy = true;
                formats.PrintNCDpp(NC, printer);
                IsBusy = false;
            }
        }
        public ObservableCollection<Transaccion> CurrentSectorTransferencias(string seccodigo = "")
        {
          return Transacciones = !string.IsNullOrEmpty(seccodigo) ? new ObservableCollection<Transaccion>(TransaccionsTogive
              .Where(t => t.Seccodigo == seccodigo)
              .ToList()) :
              new ObservableCollection<Transaccion>(TransaccionsTogive);
        }

        private void ConvertirCotizacionApedido()
        {
            switch (DatosTransaction.Data.EstTabla.ToUpper())
            {
                case "COTIZACIONES":
                    myPed.InsertarPedidoInTempFromCotizaciones(CurrentTransaccion.TransaccionID, false, CurrentTransaccion.RepCodigo);
                    break;
                case "COTIZACIONESCONFIRMADOS":
                    myPed.InsertarPedidoInTempFromCotizaciones(CurrentTransaccion.TransaccionID, true, CurrentTransaccion.RepCodigo);
                    break;
            }
            Arguments.Values.CurrentModule = Modules.PEDIDOS;
        }

        private  void ConvertirCotizacionAVentas()
        {
            var hayProductosSinExistencia = false;
            Arguments.Values.CurrentModule = Modules.VENTAS;
            Arguments.Values.CurrentVisSecuencia = myCot.GetBySecuencia(CurrentTransaccion.TransaccionID, DatosTransaction.Data.EstTabla.ToUpper() == "COTIZACIONESCONFIRMADOS").VisSecuencia ;
            myVen.InsertarVentasInTempFromCotizaciones(CurrentTransaccion.TransaccionID, DatosTransaction.Data.EstTabla.ToUpper() == "COTIZACIONESCONFIRMADOS", (int)Modules.VENTAS, out hayProductosSinExistencia);

            if (hayProductosSinExistencia)
            {
                DisplayAlert(AppResource.Warning, AppResource.NoSufficientStockAvailableWillBeGiven, AppResource.Aceptar);
            }

        }

        private void ConvertirCotizacionApedidoAut()
        {
            switch (DatosTransaction.Data.EstTabla.ToUpper())
            {
                case "COTIZACIONES":
                    myPed.InsertarPedidoInTempFromCotizacionesAut(CurrentTransaccion.TransaccionID, false, out pedSecuenciaAut);
                    break;
                case "COTIZACIONESCONFIRMADOS":
                    myPed.InsertarPedidoInTempFromCotizacionesAut(CurrentTransaccion.TransaccionID, true, out pedSecuenciaAut);
                    break;
            }
            Arguments.Values.CurrentModule = Modules.PEDIDOS;
        }

    }
}
