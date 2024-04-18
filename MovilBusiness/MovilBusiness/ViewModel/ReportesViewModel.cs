using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.model.Internal.Structs.Args;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Printer;
using MovilBusiness.Printer.Formats;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using MovilBusiness.Views.Components.TemplateSelector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class ReportesViewModel : BaseViewModel
    {
        public List<KV> TiposReportes { get; private set; }

        public ICommand PrintCommand { get; private set; }

        public bool IsReady { private get; set; } = false;

        private KV currenttiporeporte;
        public KV CurrentTipoReporte { get => currenttiporeporte; set { currenttiporeporte = value; TipoReporteChanged(); RaiseOnPropertyChanged(); } }

        private bool esfuturista;
        public bool EsFuturista { get => esfuturista; set { esfuturista = value; RaiseOnPropertyChanged(); } } 
       
        private bool isvisiblesaldos;
        public bool IsVisibleSaldos { get => isvisiblesaldos; set { isvisiblesaldos = value; RaiseOnPropertyChanged(); } } 

        private bool isvisibleNCF;
        public bool IsVisibleNCF { get => isvisibleNCF; set { isvisibleNCF = value; RaiseOnPropertyChanged(); } } 
       
        private bool isvisiblefacturasvencidas;
        public bool IsVisibleFacturasVencidas { get => isvisiblefacturasvencidas; set { isvisiblefacturasvencidas = value; RaiseOnPropertyChanged(); } }

        private bool isvisibleposiblescobrosxdia;
        public bool IsVisiblePosiblesCobrosxDia { get => isvisibleposiblescobrosxdia; set { isvisibleposiblescobrosxdia = value; RaiseOnPropertyChanged(); } }

        private bool seedescription = true;
        public bool SeeDescription { get => seedescription; set { seedescription = value; RaiseOnPropertyChanged(); } } 
       
        private bool Seedescriptionpueblos;
        public bool SeeDescriptionPueblos { get => Seedescriptionpueblos; set { Seedescriptionpueblos = value; RaiseOnPropertyChanged(); } } 

        private List<RowLinker> datareporte;
        public List<RowLinker> DataReporte { get => datareporte; set { datareporte = value; RaiseOnPropertyChanged(); } }

        private string saldo;
        public string Saldo { get => saldo; set { saldo = value; RaiseOnPropertyChanged(); } }

        private string facturasvencidas;
        public string FacturasVencidas { get => facturasvencidas; set { facturasvencidas = value; RaiseOnPropertyChanged(); } }

        private DateTime currentfechadesde = DateTime.Now, currentfechahasta = DateTime.Now;

        public DateTime CurrentFechaDesde { get => currentfechadesde; set { currentfechadesde = value; DateChanged?.Invoke(value, DateRange.DATEFROM); CargarReporte(); RaiseOnPropertyChanged(); } }

        private DateTime propertymaximumdate = DateTime.MaxValue;
        public DateTime PropertyMaximumDate { get => propertymaximumdate; set { propertymaximumdate = value; RaiseOnPropertyChanged(); } }
        public DateTime CurrentFechaHasta { get => currentfechahasta; set { currentfechahasta = value; DateChanged?.Invoke(value, DateRange.DATETO); CargarReporte(); RaiseOnPropertyChanged(); } }

        public Action<DateTime, DateRange> DateChanged { private get; set; }

        private DS_Reportes myRep;
        private DS_Inventarios myInv;
        private DS_Monedas myMon;

        private List<Sectores> Sectores = new List<Sectores>();

        private bool useSectores = false;

        public ReportesViewModel(Page page) : base(page)
        {
            myRep = new DS_Reportes();
            myInv = new DS_Inventarios();
            myMon = new DS_Monedas();

            EsFuturista = true;
            IsVisibleNCF = false;
            PrintCommand = new Command(ShowAlertPrint);

            useSectores = myParametro.GetParSectores() > 0;

            if (useSectores)
            {
                Sectores = new DS_Sectores().GetSectores();
            }

            var list = new List<KV>()
            {
                new KV("1", AppResource.Returns),
                new KV("2", AppResource.OverdueBills),
                new KV("3", AppResource.General),
                new KV("4", AppResource.Expenses),
                new KV("5", AppResource.Inventory),
                new KV("6", AppResource.OrderWithItbis),
                new KV("7", AppResource.OrdersWithoutItbis),
                new KV("8", AppResource.Receipts),
                new KV("9", AppResource.FuturisticsReceipts),
                new KV("10", AppResource.SalesWithoutItbis),
                new KV("14", AppResource.SalesWithItbis),
                new KV("11", AppResource.Visits),
                new KV("12", AppResource.PresalesByProductLine),
                new KV("13", AppResource.PushMoney),
                new KV("15", AppResource.Quotes),
                new KV("16", AppResource.QuotesWithoutDetail),
                new KV("17", AppResource.BalanceDueToSeniority),
                new KV("18", AppResource.ManagementSummary),
                new KV("19", AppResource.DueBySeniorityTowns),
                new KV("20", AppResource.OutstandingInvoicesByDay),
                new KV("21", AppResource.MonthlyOverdueBills),
                new KV("22", AppResource.VoidedReceipts),
                new KV("23", "NCF"),
                new KV("24", AppResource.PossibleChargesForDays),
            };

            if(myParametro.GetParReportePedidosConvertir125Libras())
            {
                list.Add(new KV("25", AppResource.OrderWithItbis125));
                list.Add(new KV("26", AppResource.OrdersWithoutItbis125));
            }

            TiposReportes = list.OrderBy(x => x.Value).ToList();
        }

        private async void ShowAlertPrint()
        {
            try
            {
                if(IsVisibleFacturasVencidas)
                {
                    throw new Exception(AppResource.CannotPrintMonthlyOverdueInvoices);
                }

                if(DataReporte == null || DataReporte.Count == 0)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.NoDataLoadedToPrint);
                    return;
                }

                if (IsBusy)
                {
                    return;
                }

                IsBusy = true;

                var result = await DisplayActionSheet(AppResource.ChoosePrinterCopies, buttons: new string[] { "1", "2", "3", "4", "5"});

                IsBusy = false;

                switch (result)
                {
                    case "1":
                        await Imprimir(1);
                        break;
                    case "2":
                        await Imprimir(2);
                        break;
                    case "3":
                        await Imprimir(3);
                        break;
                    case "4":
                        await Imprimir(4);
                        break;
                    case "5":
                        await Imprimir(5);
                        break;
                }

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        private async Task Imprimir(int copias)
        {
            IsBusy = true;

            ReportesFormats printer = new ReportesFormats();
            PrinterManager manager = null;
            string parSectorPorDefecto = myParametro.GetParRepresentanteSectorPorDefecto();

            for (int i = 0; i < copias; i++)
            {
                await Task.Run(() =>
                {
                    if (manager == null)
                    {
                        manager = new PrinterManager(parSectorPorDefecto);
                    }
                    if (myParametro.GetFormatoImpresionCuadres() == 5 && DataReporte[0] is SubTitle r)
                    {
                        if(r.Description == AppResource.SalesSummaryWithItbisUpper)
                        {
                            ReporteVentasconItbisFormat x = new ReporteVentasconItbisFormat();
                            x.Print(CurrentFechaDesde, CurrentFechaHasta, manager);

                           
                        }else if(r.Description == AppResource.SalesSummaryWithoutItbisUpper)
                        {
                            ReporteVentassinItbisFormat x = new ReporteVentassinItbisFormat();
                            x.Print(CurrentFechaDesde, CurrentFechaHasta, manager);
                        }
                        else
                        {
                            printer.Print(DataReporte, manager, CurrentFechaDesde, CurrentFechaHasta);
                        }

                    }
                    else
                    {
                        if(DataReporte[0] is SubTitle rec)
                        {
                            if (rec.Description == "C U A D R E  D E   R E C I B O S")
                            {
                                printer.PrintResumenRecibos(DataReporte, manager, CurrentFechaDesde, CurrentFechaHasta);
                            }
                            else
                            {
                                printer.Print(DataReporte, manager, CurrentFechaDesde, CurrentFechaHasta);
                            }
                        }                       
                    }

                });

                if (copias > 1 && i != copias - 1)
                {
                    await DisplayAlert(AppResource.PrintCopy, AppResource.CutPapelMessage, AppResource.Print);
                }

            }

            IsBusy = false;
        }

        private void TipoReporteChanged()
        {
            if (CurrentTipoReporte == null || !IsReady)
            {
                DataReporte = null;
                return;
            }

            CargarReporte();
        }

        private void CargarReporte()
        {
            try
            {
                PropertyMaximumDate = DateTime.MaxValue;

                if (CurrentTipoReporte == null)
                {
                    DataReporte = null;
                    return;
                }

                /*if (DataReporte == null)
                {
                    DataReporte = new List<RowLinker>();
                }*/

                if (useSectores && CurrentTipoReporte.Key != "4" && CurrentTipoReporte.Key != "5" && CurrentTipoReporte.Key != "10" && CurrentTipoReporte.Key != "2")
                {
                    CargarReportePorSector();
                    return;
                }

                IsVisibleFacturasVencidas = false;
                IsVisiblePosiblesCobrosxDia = false;

                IsVisibleNCF = false;

                if (CurrentTipoReporte.Key == "17")
                {
                    IsVisibleSaldos = true;
                    EsFuturista = false;
                }
                else 
                {
                    IsVisibleSaldos = false;
                }

                switch (CurrentTipoReporte.Key)
                {
                    case "1":
                        CargarReporteDevoluciones();
                        break;
                    case "2":
                        CargarReporteFacturasVencidas();
                        break;
                    case "3":
                        CargarReporteGeneral();
                        break;
                    case "4":
                        CargarReporteGastos();
                        break;
                    case "5":
                        CargarReporteInventario();
                        break;
                    case "6":
                        CargarReportePedidos();
                        break;
                    case "7":
                        CargarReportePedidos(false);
                        break;
                    case "8":
                        CargarReporteRecibos();
                        break;
                    case "9":
                        CargarReporteRecibos(true);
                        break;
                    case "10":
                        CargarReporteVentas();
                        break;
                    case "11":
                        CargarReporteVisitas();
                        break;
                    case "12":
                        PreventaporLineaProductos();
                        break;
                    case "13":
                        CargarReportePushMoney();
                        break;
                    case "14":
                        CargarReporteVentasItbis();
                        break;
                    case "15":
                        CargarReporteCotizaciones();
                        break;
                    case "16":
                        CargarReporteCotizaciones(isdetalle: true);
                        break;
                    case "17":
                        CargarReporteSaldoPorAntiguedad();
                        break;
                    case "18":
                        ReporteResumenDeGestion();
                        break;
                    case "19":
                        CargarReporteSaldoPorAntiguedadByPueblo();
                        break;
                    case "20":
                        CargarReporteClientesPendientes();
                        break;
                    case "21":
                        IsVisibleFacturasVencidas = true;
                        FacturasAvencerDelMes();
                        break;
                    case "22":
                        CargarReporteRecibos(isfromanular:true);
                        break;
                    case "23":
                        ResumenNcf();
                        break;
                    case "24":
                        IsVisiblePosiblesCobrosxDia = true;
                        PosibleCobrosxDias();
                        break;
                    case "25":
                        CargarReportePedidos(isPedidoConvertido: true);
                        break;
                    case "26":
                        CargarReportePedidos(false, isPedidoConvertido:true);
                        break;
                }
            }catch(Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message);
            }
        }

        private void CargarReportePorSector()
        {
            if(Sectores == null || Sectores.Count == 0 || CurrentTipoReporte == null || !useSectores)
            {
                DataReporte = null;
                return;
            }

            try
            {
                DataReporte = new List<RowLinker>();

                IsVisibleNCF = false;

                foreach (var sector in Sectores)
                {
                    var list = new List<RowLinker>()
                    {
                        new SubTitle(){ Description = "** " + sector.SecDescripcion.ToUpper() + " **", Bold = true, IsHeader = true}
                    };

                    list.Add(new RowLinker()); //salto de linea

                    DataReporte.AddRange(list);

                    switch (CurrentTipoReporte.Key)
                    {
                        case "1":
                            CargarReporteDevoluciones(forGeneral:true, secCodigo: sector.SecCodigo);
                            break;
                        case "3":
                            CargarReporteGeneral(sector.SecCodigo, clearList:false);
                            break;
                        case "6":
                            CargarReportePedidos(forGeneral: true, secCodigo: sector.SecCodigo);
                            break;
                        case "7":
                            CargarReportePedidos(false, forGeneral: true, secCodigo: sector.SecCodigo);
                            break;
                        case "8":
                            CargarReporteRecibos(forGeneral: true, secCodigo: sector.SecCodigo);
                            break;
                        case "9":
                            CargarReporteRecibos(true, forGeneral: true, secCodigo: sector.SecCodigo);
                            break;
                        case "11":
                            CargarReporteVisitas(forGeneral: true, secCodigo: sector.SecCodigo);
                            break;
                        case "12":
                            PreventaporLineaProductos(forGeneral: true, secCodigo: sector.SecCodigo);
                            break;
                        case "13":
                            CargarReportePushMoney(forGeneral: true, secCodigo: sector.SecCodigo);
                            break;
                        case "15":
                            CargarReporteCotizaciones(forGeneral: true, secCodigo: sector.SecCodigo);
                            break;
                        case "16":
                            CargarReporteCotizaciones(forGeneral: true, secCodigo: sector.SecCodigo, isdetalle:true);
                            break;
                        case "17":
                            CargarReporteSaldoPorAntiguedad();
                            break;
                        case "18":
                            ReporteResumenDeGestion();
                            break;
                        case "19":
                            CargarReporteSaldoPorAntiguedadByPueblo();
                            break;
                        case "22":
                            CargarReporteRecibos(forGeneral: true, secCodigo: sector.SecCodigo, isfromanular:true);
                            break;
                        case "23":
                            ResumenNcf();
                            break;
                        case "25":
                            CargarReportePedidos(forGeneral: true, secCodigo: sector.SecCodigo, isPedidoConvertido: true);
                            break;
                        case "26":
                            CargarReportePedidos(false, forGeneral: true, secCodigo: sector.SecCodigo, isPedidoConvertido: true);
                            break;
                    }
                }

                if(CurrentTipoReporte.Key == "3")
                {
                    CargarReporteInventario(true);
                }
                else
                {
                    var nlist = new List<RowLinker>(DataReporte);
                    DataReporte = nlist;
                }
            }
            catch(Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message);
            }
            
        }

        private void CargarReportePushMoney(bool forGeneral = false, string secCodigo = null)
        {
            EsFuturista = true;

            var list = new List<RowLinker>()
            {
                new SubTitle(){Description = AppResource.PushMoneySummaryTitle}
            };
            list.Add(new RowLinker()); //salto de linea
            list.Add(new ReportesNameQuantity() { Name = AppResource.CodeDescription, Amount = AppResource.Price, Quantity =  AppResource.QtyBox, Bold = true });
            var pushMoneys = myRep.GetResumenPushMoney(CurrentFechaDesde.ToString("yyyy-MM-dd"), CurrentFechaHasta.ToString("yyyy-MM-dd"), secCodigo);
            double totalCantidades = pushMoneys.Sum(c => double.Parse(c.Amount) * double.Parse(c.Quantity));
            list.AddRange(pushMoneys);

            list.Add(new RowLinker()); //salto de linea
            list.Add(new Totales() { Total = totalCantidades, Bold = true, });

            if (forGeneral)
            {
                DataReporte.AddRange(list);
            }
            else
            {
                DataReporte = list;
            }

        }

        private void CargarReporteDevoluciones(bool forGeneral = false, string secCodigo = null)
        {
            EsFuturista = true;

            var list = new List<RowLinker>()
            {
                new SubTitle(){ Description = AppResource.ReturnSummaryUpper }
            };

            list.Add(new ReportesNameQuantity() { Name = AppResource.Description, Amount = AppResource.Amount, Quantity = AppResource.Quantity, Bold = true });
            

            list.AddRange(myRep.GetResumenDevoluciones(CurrentFechaDesde.ToString("yyyy-MM-dd"), CurrentFechaHasta.ToString("yyyy-MM-dd"), secCodigo));

            if (forGeneral)
            {
                DataReporte.AddRange(list);
            }
            else
            {
                DataReporte = list;
            }
            
        }

        private void CargarReporteFacturasVencidas(bool forGeneral = false)
        {
            EsFuturista = false;
            var list = new List<RowLinker>()
            {
                new SubTitle(){Description = AppResource.OverdueInvoicesSummaryUpper }
            };

            var facturas = myRep.GetResumenFacturasVencidas();
            list.AddRange(facturas);
            list.Add(new RowLinker()); //salto de linea
            list.Add(new Totales() { Total = facturas.Sum(x=>x.Balance) });

            DataReporte = list;
        }
        private void CargarReporteGeneral(string secCodigo = null, bool clearList = true)
        {
            EsFuturista = true;

            if (clearList)
            {
                DataReporte = new List<RowLinker>();
            }

            CargarReporteRecibos(false, true, secCodigo);
            CargarReporteRecibos(false, true, secCodigo,isfromanular:true);
            CargarReportePedidos(true, true, secCodigo);
            CargarReporteCotizaciones(true, secCodigo);
            //DataReporte.AddRange(myRep.GetDesempeno(CurrentFechaDesde.ToString("yyyy-MM-dd")));
            CargarReporteDevoluciones(true, secCodigo);
            if (string.IsNullOrWhiteSpace(secCodigo))
            {
                CargarReporteInventario(true);
            }
            CargarReporteVisitas(true, secCodigo);
            //CargarReporteVentas(true);
            CargarReportePushMoney(true, secCodigo);
            var list = new List<RowLinker>(DataReporte);

            DataReporte = list;
        }

        private void CargarReporteGastos(bool forGeneral = false)
        {
            EsFuturista = true;

            var list = new List<RowLinker>()
            {
                new SubTitle(){ Description = AppResource.ExpenseSummaryUpper }
            };

            var gastos = myRep.GetReporteGastos(CurrentFechaDesde.ToString("yyyy-MM-dd"), CurrentFechaHasta.ToString("yyyy-MM-dd"));

            list.AddRange(gastos);

            list.Add(new GastosTotales() { Total = gastos.Sum(x=>double.Parse(x.GasMontoTotal)).ToString("N2"), TotalItbis = gastos.Sum(x=>double.Parse(x.GasItebis)).ToString("N2"), TotalPropina = gastos.Sum(x=>double.Parse(x.GasPropina)).ToString("N2") });

            DataReporte = list;
        }

        private void CargarReporteInventario(bool forGeneral = false)
        {
            EsFuturista = true;

            var list = new List<RowLinker>()
            {
                new SubTitle(){Description = AppResource.InventorySummaryUpper}
            };

            list.Add(new ReportesNameQuantity() { Name = AppResource.CodeDescription, Quantity = AppResource.Quantity, Amount = "Unm", Bold = true });
            list.AddRange(myInv.GetInventario());

            if (forGeneral)
            {
                DataReporte.AddRange(list);
            }
            else
            {
                DataReporte = list;
            }
    
        }

        private void CargarReportePedidos(bool withItbis = true, bool forGeneral = false, string secCodigo = null, bool isPedidoConvertido = false)
        {
            EsFuturista = true;

            var list = new List<RowLinker>()
            {
                new SubTitle(){Description = isPedidoConvertido ? $"{AppResource.OrderSummaryUpper} - (125 LB)" : AppResource.OrderSummaryUpper},
                new ReportesNameQuantity(){Name = AppResource.Customers, Quantity = AppResource.Lines, Amount = AppResource.Amount, Bold = true}
            };

            if (isPedidoConvertido)
            {
                list.AddRange(myRep.GetResumenPedidosToQuantityConversion(CurrentFechaDesde.ToString("yyyy-MM-dd"), CurrentFechaHasta.ToString("yyyy-MM-dd"), withItbis, secCodigo));
            }
            else
            {
                list.AddRange(myRep.GetResumenPedidos(CurrentFechaDesde.ToString("yyyy-MM-dd"), CurrentFechaHasta.ToString("yyyy-MM-dd"), withItbis, secCodigo));
            }
            

            if (forGeneral)
            {
                DataReporte.AddRange(list);
                 
            }
            else
            {
                DataReporte = list;
            }

        }

        private void CargarReporteCotizaciones(bool forGeneral = false, string secCodigo = null,bool isdetalle = false)
        {
            EsFuturista = true;

            List<RowLinker> list;

            if(isdetalle)
            {
                list = new List<RowLinker>()
                {
                  new SubTitle() { Description = AppResource.QuotesWithoutDetailSummaryUpper },
                  new ReportesNameQuantity() { Name = AppResource.Customers, Quantity = AppResource.Lines, Amount = AppResource.Amount, Bold = true }
                };
            }
            else
            {
                list = new List<RowLinker>()
                {
                  new SubTitle() { Description = AppResource.SummaryQuotesUpper },
                  new ReportesNameQuantity() { Name = AppResource.Customers, Quantity = AppResource.Lines, Amount = AppResource.Amount, Bold = true }
                };
            }

            list.AddRange(myRep.GetResumenCotizaciones(CurrentFechaDesde.ToString("yyyy-MM-dd"), CurrentFechaHasta.ToString("yyyy-MM-dd"), secCodigo,isdetalle: isdetalle));

            if (forGeneral)
            {
                DataReporte.AddRange(list);

            }
            else
            {
                DataReporte = list;
            }

        }

        private void CargarReporteSaldoPorAntiguedad()
        {
            Saldo = AppResource.BalanceDueToSeniorityUpper;

            var list = new List<RowLinker>();

            var cliArgs = new ClientesArgs();
            cliArgs.RepCodigo = Arguments.CurrentUser.RepCodigo;
            cliArgs.Estatus = FiltroEstatusVisitaClientes.TODOS;

            var clientes = new DS_Clientes().GetClientes(cliArgs);

            foreach(var cli in clientes)
            {
                var saldoxant = myRep.GetSaldoPorAntiguedadByCliente(cli.CliID,cli.MonCodigo);
                var monsigl = myMon.GetMonedaByMonCodForDep(cli.MonCodigo);
                var result = saldoxant != null? saldoxant[0].Balance + saldoxant[1].Balance + saldoxant[2].Balance + saldoxant[3].Balance : 0;

                if (saldoxant != null && result > 0)
                {
                    var clisaldo = new SaldoXAntiguedadTitle()
                    {
                        Cliente = cli.CliNombre,
                        Moneda = monsigl?.MonSigla,
                        Balance = result.ToString("N2"),
                        column1 = saldoxant[0].Balance.ToString("N2"),
                        column2 = saldoxant[1].Balance.ToString("N2"),
                        column3 = saldoxant[2].Balance.ToString("N2"),
                        column4 = saldoxant[3].Balance.ToString("N2"),
                    };

                    list.Add(clisaldo);
                }
            }

            DataReporte = list;
        }

        private void CargarReporteSaldoPorAntiguedadByPueblo()
        {
            IsVisibleSaldos = true;
            EsFuturista = false;

            Saldo = AppResource.BalanceDueToSeniorityTownsUpper;

            var list = new List<RowLinker>();

            var proval = new DS_Provincias().GetProvincias();

            var cliArgs = new ClientesArgs();
            cliArgs.RepCodigo = Arguments.CurrentUser.RepCodigo;
            cliArgs.Estatus = FiltroEstatusVisitaClientes.TODOS;

            var dsCli = new DS_Clientes();

            foreach(var pro in proval)
            {
                cliArgs.ProID = pro.ProID;
                var clientes = dsCli.GetClientes(cliArgs);//new DS_Clientes().GetClienteForRepByPueblo(pro.ProID);
                bool numcont = true;
                Monedas monsigl = new Monedas();
                int total = 0;

                foreach (var cli in clientes)
                {
                    var saldoxant = myRep.GetSaldoPorAntiguedadByCliente(cli.CliID, cli.MonCodigo);
                    monsigl = myMon.GetMonedaByMonCodForDep(cli.MonCodigo);
                    var result = saldoxant != null ? saldoxant[0].Balance + saldoxant[1].Balance + saldoxant[2].Balance + saldoxant[3].Balance : 0;

                    if (saldoxant != null && result > 0)
                    {
                        if(numcont)
                        {
                            list.Add(new SubTitlePueblos() { DescriptionXPueblos = pro.ProNombre });
                        }

                        var clisaldo = new SaldoXAntiguedadTitle()
                        {
                            Cliente = cli.CliNombre,
                            Moneda = monsigl.MonSigla,
                            Balance = result.ToString("N2"),
                            column1 = saldoxant[0].Balance.ToString("N2"),
                            column2 = saldoxant[1].Balance.ToString("N2"),
                            column3 = saldoxant[2].Balance.ToString("N2"),
                            column4 = saldoxant[3].Balance.ToString("N2"),
                        };
                        total += (int)result;
                        list.Add(clisaldo);
                        numcont = false;
                    }
                }

                if (!numcont)
                {
                    list.Add(new SubTitlePueblos() { DescriptionXPueblos = "Total " + pro.ProNombre + " " + monsigl.MonSigla + total.ToString("N2") });
                }
            }

            DataReporte = list;
        }



        private void CargarReporteRecibos(bool onlyFuturistas = false, bool forGeneral = false, string secCodigo = null, bool isfromanular = false)
        {
            var list = new List<RowLinker>();

            if (onlyFuturistas)
            {
                EsFuturista = false;

                list = new List<RowLinker>()
                {
                new SubTitle(){Description = AppResource.FuturisticReceiptsSummaryUpper}
                };
            }

            else 
            {
                EsFuturista = true;

                list = new List<RowLinker>()
                {
                new SubTitle(){Description = "C U A D R E  D E   R E C I B O S"}
                };
            }
             

            list.AddRange(myRep.GetResumenRecibos(CurrentFechaDesde.ToString("yyyy-MM-dd"), CurrentFechaHasta.ToString("yyyy-MM-dd"), onlyFuturistas, secCodigo, isfromanular));

            if (forGeneral)
            {
                DataReporte.AddRange(list);
            }
            else
            {
                DataReporte = list;
            }
            
        }

        private void CargarReporteVentas(bool forGeneral = false)
        {
            EsFuturista = true;

            var list = new List<RowLinker>()
            {
                new SubTitle(){Description = AppResource.SalesSummaryWithoutItbisUpper},
                new ReportesNameQuantity(){Name = AppResource.SaleCustomer, Quantity = AppResource.Quantity, Amount = AppResource.Amount, Bold = true}
            };

            list.AddRange(myRep.GetResumenVentas(CurrentFechaDesde.ToString("yyyy-MM-dd"), CurrentFechaHasta.ToString("yyyy-MM-dd")));

            DataReporte = list;
        }

        private void CargarReporteVentasItbis(bool forGeneral = false)
        {
            EsFuturista = true;

            var list = new List<RowLinker>()
            {
                new SubTitle(){Description = AppResource.SalesSummaryWithItbisUpper},
                new ReportesNameQuantity(){Name = AppResource.SaleCustomer, Quantity = AppResource.Quantity, Amount = AppResource.Amount, Bold = true}
            };

            list.AddRange(myRep.GetResumenVentas(CurrentFechaDesde.ToString("yyyy-MM-dd"), CurrentFechaHasta.ToString("yyyy-MM-dd"), true));

            DataReporte = list;
        }

        private void CargarReporteVisitas(bool forGeneral = false, string secCodigo = null)
        {
            EsFuturista = true;

            var list = new List<RowLinker>()
            {
                new SubTitle(){ Description = AppResource.VisitSummaryUpper }
            };

            var numeroSemana = Functions.GetWeekOfMonth(CurrentFechaDesde);

            if (numeroSemana > 4)
            {
                numeroSemana = 4;
            }

            list.AddRange(myRep.GetResumenVisitas(numeroSemana, (int)(CurrentFechaDesde).DayOfWeek - 1, CurrentFechaDesde.ToString("yyyy-MM-dd"), CurrentFechaHasta.ToString("yyyy-MM-dd"), secCodigo));

            if (forGeneral)
            {
                DataReporte.AddRange(list);
            }
            else
            {
                DataReporte = list;
            }
            
        }

        private async void ReporteResumenDeGestion(bool forGeneral = false, string secCodigo = null)
        {
            EsFuturista = true;

            PropertyMaximumDate = DateTime.Now.AddMonths(2);

            var list = new List<RowLinker>()
            {
                new SubTitle(){ Description = AppResource.ManagementSummaryUpper}
            };

            list.AddRange(await myRep.GetResumenDeGestion(CurrentFechaDesde.ToString("yyyy-MM-dd"), CurrentFechaHasta.ToString("yyyy-MM-dd")));

            if (forGeneral)
            {
                DataReporte.AddRange(list);
            }
            else
            {
                DataReporte = list;
            }
            
        }

        private void PreventaporLineaProductos(bool forGeneral = false, string secCodigo = null)
        {
            EsFuturista = true;

            var list = new List<RowLinker>()
            {
                new SubTitle(){ Description = AppResource.PresalesByProductLine }
            };
            list.Add(new ReportesNameQuantity() { Name = AppResource.Lines, Amount = AppResource.Amount, Bold = true });
            var montosXlinea = myRep.GetResumenPreventaporLineadeProductos(CurrentFechaDesde.ToString("yyyy-MM-dd"), CurrentFechaHasta.ToString("yyyy-MM-dd"), secCodigo);
            list.AddRange(montosXlinea);
            list.Add(new RowLinker()); //salto de linea
            list.Add(new Totales() { Total = montosXlinea.Sum(x => x.Monto) });

            if (forGeneral)
            {
                DataReporte.AddRange(list);
            }
            else
            {
                DataReporte = list;
            }
        }
        private void CargarReporteClientesPendientes(bool forGeneral = false)
        {
            EsFuturista = false;

            var list = new List<RowLinker>()
            {
                new SubTitle(){Description = AppResource.PendingInvoicesPerDaySummaryUpper }
            };

            var clientes = myRep.GetClientesPendientes();
            foreach (var cli in clientes)
            {
                var facturas = myRep.GetResumenFacturasClientesPendientes(cli.CliID);

                if (facturas.Count <= 0)
                {
                    continue;
                }

                list.Add(new SubTitlePueblos() { DescriptionXPueblos = cli.CliNombre });
                list.AddRange(facturas);
                list.Add(new RowLinker());
                list.Add(new Totales() { Total = facturas.Sum(x => x.Balance) });
            }

            DataReporte = list;
        }

        public void FacturasAvencerDelMes()
        {

            EsFuturista = false;

            FacturasVencidas = AppResource.BillsDueForTheMonthSummaryUpper;

            var list = new List<RowLinker>();

            var clientes = myRep.GetClientesForFacturasDelMes();

            foreach(var cli in clientes)
            {
                var facturas = myRep.GetResumenFacturasVencidasDelMes(cli.CliID);
                if (facturas != null && facturas.Count > 0)
                {
                    list.Add(new SubTitlePueblos() { CliCodigo = cli.CliCodigo, DescriptionXPueblos = cli.CliNombre });
                }
                else
                {
                    continue;
                }
                list.AddRange(facturas);
                list.Add(new RowLinker());
                list.Add(new Totales() { Total = (double)facturas.Sum(x => x.Balance), Bold = true });
            }
            DataReporte = list;
        }

        public void PosibleCobrosxDias()
        {

            EsFuturista = true;

            FacturasVencidas = AppResource.BillsDueForTheMonthSummaryUpper;

            var list = new List<RowLinker>();
            double totalLinea, totalGeneral = 0.0;
            int cantidadLinea, cantidadTotal = 0;

            DateTime fechaInicio = DateTime.Parse(CurrentFechaDesde.ToString("yyyy-MM-dd"));
            DateTime fechaFin = DateTime.Parse(CurrentFechaHasta.ToString("yyyy-MM-dd"));

            for (var d = fechaInicio; d <= fechaFin; d = d.AddDays(1))
            {
                var clientebyruta = myRep.GetClientesxRutaVisita(d.ToString("yyyy-MM-dd"), d.ToString("yyyy-MM-dd"));
                list.Add(new SubTitlePueblos() { DescriptionXPueblos = "Fecha: " + d.ToString("dd-MM-yyyy") });
                totalLinea = 0.0;
                cantidadLinea = 0;
                foreach (var fac in clientebyruta)
                { 
                    var facturas = myRep.GetResumenFacturasxDia(fac.CliID, d);
                    if (facturas == null || facturas.Count == 0)
                    {
                        continue;
                    }
                    list.AddRange(facturas);
                    cantidadLinea += facturas.Count;
                    totalLinea += (double)facturas.Sum(x => x.Balance);
                }
                totalGeneral += totalLinea;
                cantidadTotal += cantidadLinea;
                list.Add(new Totales() { Total = totalLinea, CantidadCobros = $"({cantidadLinea})", Bold = true });
                list.Add(new RowLinker());
            }
            list.Add(new RowLinker());
            list.Add(new Totales() { Total = totalGeneral, CantidadCobros =$"({cantidadTotal})", Bold = true });
            
            DataReporte = list;
        }

        public void ResumenNcf()
        {
            IsVisibleNCF = true;
            EsFuturista = false;

            var list = new List<RowLinker>();            
            list.AddRange(myRep.GetRepresentantesDetalleNCF2018());

            DataReporte = list;
        }
    }
}
