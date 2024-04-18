using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.model.Internal.structs;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal.Structs.Args;
using MovilBusiness.Pdf.Formats;
using MovilBusiness.Printer;
using MovilBusiness.printers.formats;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.views;
using MovilBusiness.Views.Components.Modals;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.viewmodel
{
    public class CobrosViewModel : BaseViewModel
    {
        public bool FirstTime { get; set; } = true;

        public ObservableCollection<model.Internal.MenuItem> MenuSource { get; private set; }
        private model.Internal.MenuItem selecteditem;
        public model.Internal.MenuItem SelectedItem { get { return selecteditem; } set { selecteditem = value; RaiseOnPropertyChanged(); OnOptionItemSelected(); } }

        private ClientesCreditoData currentclientdata;
        public ClientesCreditoData CurrentClientData { get => currentclientdata; private set { currentclientdata = value; RaiseOnPropertyChanged(); } }

        private ObservableCollection<CuentasxCobrar> documentos;
        public ObservableCollection<CuentasxCobrar> Documentos { get => documentos; set { documentos = value; RaiseOnPropertyChanged(); } }

        private List<Monedas> monedas;
        public List<Monedas> Monedas { get => monedas; set { monedas = value; RaiseOnPropertyChanged(); } }

        private Monedas currentmoneda;
        public Monedas CurrentMoneda { get => currentmoneda; set { currentmoneda = value; CurrentMonedaChanged(); RaiseOnPropertyChanged(); } }

        private bool showprinter = false;
        public bool ShowPrinter { get => showprinter; set { showprinter = value; RaiseOnPropertyChanged(); } }

        private bool showmoneda;
        public bool ShowMoneda { get => showmoneda; set { showmoneda = value; RaiseOnPropertyChanged(); } }
        
        private bool showlblchequef;
        public bool ShowlblChequeF { get => showlblchequef; set { showlblchequef = value; RaiseOnPropertyChanged(); } }
        public bool IsVisibleEstCuentas => !myParametro.GetParCxcShowEstadoDeCuentas();

        private CuentasxCobrar CurrentDocument;
        private bool PrintDocument = false;

        private DS_Productos myProd;
        private DS_CuentasxCobrar myCxc;
        private DS_Recibos myrec;
        private CxcFormatos CxcPrinter;
        private IPrinterFormatter Printer;

        private readonly bool IsConsulting = false;

        public Action OnOptionMenuItemSelected { get; set; }
        public ICommand ButtonCommand { get; private set; }
        public bool MENUCOBRO { get; set; }
        // public static double TasaOriginal = 0.0;

        public EntregasRepartidorTransacciones CurrentPedidoAEntregar;
        private CondicionesPago currentcondicionpago = null;
        public CondicionesPago CurrentCondicionPago { get => currentcondicionpago; set { currentcondicionpago = value; RaiseOnPropertyChanged(); } }

        private ObservableCollection<CondicionesPago> condicionespago;
        public ObservableCollection<CondicionesPago> CondicionesPago { get => condicionespago; private set { condicionespago = value; RaiseOnPropertyChanged(); } }


        public CobrosViewModel(Page page, bool IsConsulting) : base(page)
        {
            this.IsConsulting = IsConsulting;
            myCxc = new DS_CuentasxCobrar();
            myrec = new DS_Recibos();
            myProd = new DS_Productos();
            MENUCOBRO = myParametro.GetParMenuCobros() && myParametro.GetParCrearRecibos();
            CxcPrinter = new CxcFormatos(myCxc);
            ButtonCommand = new Command(OnButtoSelected);
            BindMenu();

            Monedas = myCxc.GetMonedasDeLosDocumentos(Arguments.Values.CurrentClient.CliID);
            ShowMoneda = Monedas != null && Monedas.Count > 1;
            ShowlblChequeF = myParametro.GetParCobrosMuestralblBalance();
        }

        //Se muestra una alerta en caso de que dicho cliente tenga cheques devueltos de N tiempo hasta el día
        public async void DisplayAlertCheques(bool ft)
        {
            bool FT = ft;
            if (myParametro.GetParCobrosChequesDevueltosNtiempo() > 0)
            {
                DateTime Fechaindicada = DateTime.Now.AddDays(-1 * DS_RepresentantesParametros.GetInstance().GetParCobrosChequesDevueltosNtiempo());
                int Cantidad = myCxc.GetCountChequesDevueltosinNDays(Arguments.Values.CurrentClient.CliID, Fechaindicada);

                if (Cantidad > 0 && FT)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.CustomerHasBouncedCheckLastYear, AppResource.Aceptar);               
                }
            }
        }

        private void CurrentMonedaChanged()
        {
            if (CurrentMoneda == null || FirstTime)
            {
                return;
            }

            Load();
        }

        private void BindMenu()
        {
            MenuSource = new ObservableCollection<model.Internal.MenuItem>();

            if (myParametro.GetParCrearRecibos())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.NewReceipt, Icon = "ic_monetization_on_black_24dp", Id = 1 });
            }

            MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.Leave, Id = 0, Icon = "ic_exit_to_app_black.png" });

            MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.BalanceDueToSeniority, Id = 2, Icon = "ic_event_black" });


            if (myParametro.GetParCobrosChequesDevueltos())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.ReturnedChecks, Id = 3, Icon = "ic_assignment_return_black_24dp" });
            }

            if (Arguments.Values.CurrentClient.CliIndicadorDepositaFactura)
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.DocumentsDelivery, Icon = "ic_receipt_black_24dp", Id = 4 });
            }

            if(IsVisibleEstCuentas)
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.AccountStatus, Id = 5, Icon = "ic_account_balance_wallet_black_24dp" });
            }            

            if (myParametro.GetParReconciliacion())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.Reconciliation, Id = 6, Icon = "ic_inbox_black_24dp" });
            }

            if (myParametro.GetParCobrosConsultarNotasCreditos())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.OtherDocuments, Id = 7, Icon = "ic_chrome_reader_mode_black_24dp" });
            }

            if (myParametro.GetParVentasCalculadoraDeNegociacion(true))
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.TradingCalculator, Id = 8, Icon = "calculator_icon" });
            }
        }

        public void CancelarImpresion()
        {
            ShowPrinter = false;
        }

        public async void AceptarImpresion(int Copias)
        {
            try
            {

                for (int x = 0; x < Copias; x++)
                {
                    IsBusy = true;

                    if (PrintDocument)
                    {
                        if (CurrentDocument == null)
                        {
                            await DisplayAlert(AppResource.Warning, AppResource.DocumentDataNotLoaded);
                            return;
                        }

                        if (CurrentDocument.CxcSIGLA == "RCB")
                        {
                            await ImprimirRecibo(CurrentDocument);
                        }
                        else
                        {
                            await ImprimirDocumento(CurrentDocument);
                        }
                        
                    }
                    else
                    {
                        await ImprimirEstadoCuenta();
                    }

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

            IsBusy = false;
            ShowPrinter = false;
        }

        public async void Load()
        {
            if (IsBusy || Arguments.Values.CurrentClient == null)
            {
                return;
            }

            IsBusy = true;

            try
            {
                Documentos = new ObservableCollection<CuentasxCobrar>(myCxc.GetAllCuentasByCliente(Arguments.Values.CurrentClient.CliID, CurrentMoneda != null ? CurrentMoneda.MonCodigo : null));
                //int cont = 0;
                double anterior = 0;
                if (Documentos != null && Documentos.Count > 0)
                {
                    foreach (var obj in Documentos)
                    {
                        #region Esto no parece ser necesario
                        /*
                        if (cont == 0)
                        {
                            if (obj.CxcSIGLA == "NC")
                            {
                                obj.CxcBalanceAcumulado = obj.CxcBalance * -1;
                                anterior = obj.CxcBalanceAcumulado;
                                cont++;
                            }
                            else
                            {
                                obj.CxcBalanceAcumulado = obj.CxcBalance;
                                anterior = obj.CxcBalanceAcumulado;
                                cont++;
                            }
                        }
                        else
                        {
                            if (obj.CxcSIGLA == "NC")
                            {
                                obj.CxcBalanceAcumulado = (obj.CxcBalance * -1) + anterior;
                                anterior = obj.CxcBalanceAcumulado;
                            }
                            else
                            {
                                obj.CxcBalanceAcumulado = obj.CxcBalance + anterior;
                                anterior = obj.CxcBalanceAcumulado;
                            }
                        } 
                        */
                        #endregion
                        obj.CxcBalanceAcumulado = (obj.Origen > 0 || obj.CxcSIGLA == "RCB" ? obj.CxcBalance * obj.Origen : obj.CxcBalance) + anterior;
                        anterior = obj.CxcBalanceAcumulado;
                    }

             
                }

                CurrentClientData = myCxc.GetDatosCreditoCliente(Arguments.Values.CurrentClient.CliID, CurrentMoneda != null ? CurrentMoneda.MonCodigo : null);

            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.ErrorLoadingDocuments, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
        }

        private async void OnOptionItemSelected()
        {
            if (SelectedItem == null || IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                switch (SelectedItem.Id)
                {
                    case 0:
                        await PopAsync(true);
                        break;
                    case 1:
                        GoReciboNuevo();
                        
                        break;
                    case 2: //saldo por antiguedad
                        await PushAsync(new SaldoPorAntiguedadPage(CurrentMoneda != null ? CurrentMoneda.MonCodigo : null));
                        break;
                    case 3: //cheques devueltos
                        break;
                    case 4: //entrega documentos
                        Arguments.Values.CurrentModule = Modules.ENTREGADOCUMENTOS;
                        await PushAsync(new EntregasDocumentosPage());
                        break;
                    case 5: //estado de cuenta
                        ShowAlertEstadoCuenta();
                        break;
                    case 6: //reconciliacion
                        if (ShowMoneda && CurrentMoneda == null)
                        {
                            throw new Exception(AppResource.ChooseCurrencyWarning);
                        }
                        Arguments.Values.CurrentModule = Modules.RECONCILIACION;
                        await PushAsync(new RecibosTabPage(- 1, CurrentMoneda, tasaOriginal:CurrentMoneda != null ? CurrentMoneda.MonTasa : 0));
                        break;
                    case 7: //otros documentos (consultar nc)
                        break;
                    case 8: ///Calculadora de Negociacion
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
                        }));
                        break;
                }

                OnOptionMenuItemSelected?.Invoke();

            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
        }

        private async void GoReciboNuevo()
        {
            if (ShowMoneda && CurrentMoneda == null && !myParametro.GetParRecibosusarMonedaUnica())
            {
                throw new Exception(AppResource.ChooseCurrencyWarning);
            }

            Monedas monedaForce = null;

            if (myParametro.GetParRecibosusarMonedaUnica())
            {
                var monedas = new DS_Monedas().GetMonedas();


                if(monedas != null && monedas.Count > 0)
                {
                    var buttons = new List<string>();

                    foreach(var mon in monedas)
                    {
                        buttons.Add(mon.MonNombre);
                    }

                    var monedaElegida = await DisplayActionSheet("Elija la moneda a utilizar", "Cancelar", buttons.ToArray());

                    var index = buttons.IndexOf(monedaElegida);
                    if (index != -1)
                    {
                        monedaForce = monedas[index];
                    }
                    else
                    {
                        return;
                    }
                }
            }

            myrec.ClearTemps();
            new DS_TransaccionesImagenes().DeleteTemp(false, tabla: "RecibosFormaPago", marked: true);

            await PushAsync(new RecibosTabPage(-1, monedaForce != null ? monedaForce : CurrentMoneda, tasaOriginal: monedaForce != null ? monedaForce.MonTasa : CurrentMoneda != null ? CurrentMoneda.MonTasa : 0, IsConsulting: IsConsulting));
        }

        private async void ShowAlertEstadoCuenta()
        {
            //var result = await DisplayAlert("Estado de cuentas", "Deseas imprimir el estado de cuenta?", "Imprimir", "Cancelar");

            var result = await DisplayActionSheet(AppResource.AccountStatus, buttons: new string[] { AppResource.Share, AppResource.Print });

            if(result == AppResource.Share)
            {
                EnviarCorreoEstadoDeCuenta();
            }
            else if(result == AppResource.Print)
            {
                PrintDocument = false;
                ShowPrinter = true;
            }
        }

        private async void EnviarCorreoEstadoDeCuenta() {
            IsBusy = true;


            try
            {
                var shareDialog = DependencyService.Get<IShareDialog>();
                var manager = new PdfCuentasxCobrar(myCxc);

                var filePath = await manager.GenerateEstadoDeCuenta(Arguments.Values.CurrentClient.CliID);

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    throw new Exception(AppResource.ErrorGeneratingPdfFile);
                }

                await shareDialog.Show(AppResource.Share, AppResource.AccountsStatusLabel + Arguments.Values.CurrentClient.CliNombre.Trim(), filePath);
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Error, e.Message);
            }

            IsBusy = false;
        }

        private async void CompartirDocumento()
        {
            if (CurrentDocument == null)
            {
                return;
            }

            try
            {
                IsBusy = true;

                var shareDialog = DependencyService.Get<IShareDialog>();
                var manager = new PdfCuentasxCobrar(myCxc, myParametro.GetParSectores() > 0 ? Arguments.Values.CurrentSector.SecCodigo : "");

                var filePath = await manager.ReportePdfDocumento(CurrentDocument);

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    throw new Exception(AppResource.ErrorGeneratingPdfFile);
                }

                await shareDialog.Show(AppResource.Share, AppResource.DocumentLabel + CurrentDocument.CxcDocumento, filePath);
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Error, e.Message);
            }

            IsBusy = false;
        }

        private async void GoDetalleFactura()
        {
            try
            {
                IsBusy = true;

                await Task.Run(() => { myCxc.InsertProductInTempForDetail(CurrentDocument.CxcReferencia, (int)Arguments.Values.CurrentModule); });

                Arguments.Values.CurrentModule = Modules.COBROS;

                var args = new PedidosDetalleArgs
                {
                    FechaEntrega = DateTime.Now,
                    ConId = 0,
                    DisenoDelRow = myParametro.GetFormatoVisualizacionProductos(),
                    PedOrdenCompra = null,
                    IsEditing = true
                };

                await PushAsync(new PedidosDetallePage(args, true, CurrentDocument.CxcDocumento, CurrentDocument) { Title = AppResource.DocumentDetail, IsDetail = true });
            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        public async void OnDocumentSelected(CuentasxCobrar document)
        {
            CurrentDocument = document;

            if (document.CxcSIGLA.Trim().ToUpper() == "NC" || document.CxcSIGLA.Trim().ToUpper() == "FT" || document.CxcSIGLA.Trim().ToUpper() == "FC" || document.CxcSIGLA.Trim().ToUpper() == "FAT")
            {

                var Buttons = new string[] { AppResource.Share, AppResource.Print };

                if (myParametro.GetParCobrosVerDetalleDocumentos())
                {
                    Buttons = new string[] { AppResource.Share, AppResource.SeeDetail, AppResource.Print };
                }

                var result = await DisplayActionSheet(AppResource.SelectDesiredOption, buttons: Buttons);

                if(result == AppResource.Share)
                {
                    CompartirDocumento();
                }
                else if(result == AppResource.Print)
                {
                    PrintDocument = true;
                    ShowPrinter = true;
                }
                else if(result == AppResource.SeeDetail)
                {
                    IsBusy = false;
                    GoDetalleFactura();
                }
            } 
            else if (document.CxcSIGLA.Trim().ToUpper() == "RCB")
            {
                var Buttons = document.RecEstatus == 1 ? new string[] { AppResource.Print, AppResource.Revoke } : new string[] { AppResource.Print };

                var result = await DisplayActionSheet(AppResource.SelectDesiredOption, buttons: Buttons);

                if (result == AppResource.Print)
                {
                    PrintDocument = true;
                    ShowPrinter = true;
                }
                else if (result == AppResource.Revoke)
                {
                    var resultado = await DisplayAlert(AppResource.Warning, AppResource.WantRevokeReceiptQuestion, AppResource.Revoke, AppResource.Continue);

                    if (resultado)
                    {
                        if (myParametro.GetParAutorizacionAnularRecibo())
                        {
                            await PushModalAsync(new AutorizacionComentarioModal(false, Convert.ToInt32(document.CxcDocumento), 3, "", false)
                            {
                                OnAutorizacionUsed = (autSec) =>
                                {
                                    myrec.EstRecibos(document.rowguid);
                                    Load();
                                    IsBusy = false;
                                }
                            });
                        }
                        else
                        {
                            myrec.EstRecibos(document.rowguid);
                        }

                    }
                    Load();
                    IsBusy = false;
                }

            }

        }

        private Task ImprimirDocumento(CuentasxCobrar document)
        {
            return Task.Run(() =>
            {
                var printer = new PrinterManager(myParametro.GetParSectores() > 0 ? Arguments.Values.CurrentSector.SecCodigo : "");

                CxcPrinter.ImprimirDetalleDocumento(document, printer);

            });
        }

        private Task ImprimirRecibo(CuentasxCobrar document)
        {
            Printer = new FormatosRecibos(myrec);
            var rowguid = Functions.GetrowguidTransaccion("RECIBOS", "RecSecuencia", Convert.ToInt32(document.CxcDocumento.ToString()));

            return Task.Run(() =>
            {
                var printer = new PrinterManager(myParametro.GetParSectores() > 0 ? Arguments.Values.CurrentSector.SecCodigo : "");

                Printer.Print(Convert.ToInt32(document.CxcDocumento.ToString()), false, printer, rowguid, -1);

                if (DS_RepresentantesParametros.GetInstance().GetParRecibosNCPorDescuentoProntoPago() == 1 && myrec.GetReciboTieneNCPorDpp(Convert.ToInt32(document.CxcDocumento.ToString()))  )
                {
                    PrintNC(Convert.ToInt32(document.CxcDocumento.ToString()), printer, new FormatosRecibos(new DS_Recibos()));
                }
  
            });
        }

        private void PrintNC(int transaccionID, PrinterManager printer, FormatosRecibos formats)
        {
            var ncs = myrec.GetNCDppRecibos(transaccionID);

            foreach (var NC in ncs)
            {
                IsBusy = true;
                formats.PrintNCDpp(NC, printer);
                IsBusy = false;
            }
        }
        private Task ImprimirEstadoCuenta()
        {
            return Task.Run(() =>
            {
                var printer = new PrinterManager(myParametro.GetParSectores() > 0 ? Arguments.Values.CurrentSector.SecCodigo : "");

                CxcPrinter.PrintEstadoCuentas(Arguments.Values.CurrentClient.CliID, printer);

            });
        }


        private async void OnButtoSelected(object Id)
        {

            if (IsBusy)
            {
                return;
            }

            IsBusy = true;


            try
            {
                switch (Id.ToString())
                {
                    case "1": 
                        if(myParametro.GetParRecibosusarMonedaUnica())
                        {
                            GoReciboNuevo();
                        } 
                        else 
                        {
                            await PushAsync(new RecibosTabPage(-1, CurrentMoneda, tasaOriginal: CurrentMoneda != null ? CurrentMoneda.MonTasa : 0, IsConsulting: IsConsulting));
                        }
                        break;
                    case "2": ShowAlertEstadoCuenta(); break;
                    case "3": await PushAsync(new EntregasDocumentosPage()); break;
                }
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Error, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
        }

        public void SelectCurrentMoneda()
        {
            if (Monedas != null && Monedas.Count > 0)
            {
                CurrentMoneda = Monedas[0];
            }
            else
            {
                CurrentMoneda = new DS_Monedas().GetMoneda(Arguments.Values.CurrentClient.MonCodigo);
            }

            //TasaOriginal = CurrentMoneda.MonTasa;

            FirstTime = false;
        }

        public void ClearCxcDetalleTemp()
        {
            try
            {
                myProd.ClearTemp((int)Modules.COBROS);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }
        
    }
}
