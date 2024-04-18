
using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Pdf.Formats;
using MovilBusiness.Printer;
using MovilBusiness.Printer.Formats;
using MovilBusiness.printers.formats;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using MovilBusiness.views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SuccessPage : ContentPage, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;

        public string MainTitle { get; set; } = AppResource.TransactionSavedSuccessfully;
        private int CurrentTransaccionSecuencia;
        private int CurrentTransaccionSecuencia2;
        private string CurrentTransaccionRowguid;

        public static PrinterManager Printer = null;

        private DS_RepresentantesParametros myParametro;
        private DS_Recibos myRec;
        private DS_Tareas mytar;
        private DS_ConteosFisicos myCont;
        private IPrinterFormatter TraPrinter, TraPrinter2;
        private IPdfGenerator pdfManager;

        public string TraPdfName, TraPdfName2;
        public string tableName, tableName2;
        public int TitId, TitId2;

        public List<int> EntregasSecuencias { get; set; }

        private bool isbusy2 = false;
        public bool ParPedidosFirmaObligatoria { get; set; }

        private bool showprinterdialog;
        public bool ShowPrinterDialog { get => showprinterdialog; set { showprinterdialog = value; RaiseOnPropertyChanged(); } }
        public SuccessPage(string title, int traSecuencia, int traSecuencia2 = -1, bool Ispreliminar = true)
        {
            MainTitle = title;

            CurrentTransaccionSecuencia = traSecuencia;
            CurrentTransaccionSecuencia2 = traSecuencia2;
            
            myParametro = DS_RepresentantesParametros.GetInstance();
            mytar = new DS_Tareas();
            ParPedidosFirmaObligatoria = myParametro.GetParPedidosFirmaObligatoria();
            myCont = new DS_ConteosFisicos();

            switch (Arguments.Values.CurrentModule)
            {
                case Modules.ENTREGADOCUMENTOS:
                    var dsE = new DS_EntregaFactura();
                    TraPrinter = new EntregaDocumentosFormats(dsE);
                    tableName = "EntregasDocumentos";
                    TitId = 10;                    
                    TraPdfName = "Entrega Factura No. ";
                    pdfManager = new PdfEntregasDocumentos(dsE);
                    break;
                case Modules.DEVOLUCIONES:
                    var ds = new DS_Devoluciones();
                    TraPrinter = new DevolucionesFormats(ds);
                    pdfManager = new PdfDevoluciones(ds, myParametro.GetParSectores() > 0 ? Arguments.Values.CurrentSector.SecCodigo : "");
                    tableName = "Devoluciones";
                    TitId = 2;
                    TraPdfName = "Devolucion No. ";
                    break;
                case Modules.CONTEOSFISICOS:
                    TitId = 8;
                    tableName = "ConteosFisicos";
                    TraPdfName = "Conteo fisico No. ";
                    TraPrinter = new ConteosFisicosFormats();
                    pdfManager = new PdfConteosFisicos(myParametro.GetParSectores() > 0 ? Arguments.Values.CurrentSector.SecCodigo : "");
                    break;
                case Modules.INVFISICO:
                    TraPrinter = new InventarioFisicoFormats(new DS_InventariosFisicos());
                    pdfManager = new PdfInventariosFsicos(myParametro.GetParSectores() > 0 ? Arguments.Values.CurrentSector.SecCodigo : "");
                    tableName = "InventarioFisico";
                    TitId = 7;
                    TraPdfName = Arguments.Values.CurrentClient.CliNombre + " - " + Arguments.Values.CurrentClient.CliID + "  Inventario No. ";
                    break;
                case Modules.COLOCACIONMERCANCIAS:
                    TraPrinter = new ColocacionProductosFormats(new DS_ColocacionProductos());
                    tableName = "ColocacionProductos";
                    TitId = 68;
                    break;
                case Modules.PEDIDOS:
                    TraPrinter = new PedidosFormats(new DS_Pedidos());
                    pdfManager = new PdfPedidos(myParametro.GetParSectores() > 0 ? Arguments.Values.CurrentSector?.SecCodigo : "");
                    tableName = "Pedidos";
                    TraPdfName = "Pedido No. ";
                    TitId = 1;
                    break;
                case Modules.COBROS:
                    myRec = new DS_Recibos();
                    TraPrinter = new FormatosRecibos(myRec);
                    pdfManager = new PdfRecibos(myParametro.GetParSectores() > 0 ? Arguments.Values.CurrentSector.SecCodigo : "");
                    tableName = "Recibos";
                    TitId = 3;
                    TraPdfName = "Recibo No. ";
                    if (myParametro.GetParImpresionAutomatica() > 0) { ImprimirAutomatico(); }
                    break;
                case Modules.RECONCILIACION:
                    TraPrinter = new ReconciliacionesFormats(new DS_Reconciliaciones());
                    //pdfManager = new PdfRecibos(myParametro.GetParSectores() > 0 ? Arguments.Values.CurrentSector.SecCodigo : "");
                    tableName = "Reconciliaciones";
                    TitId = 28;
                    TraPdfName = "Reconciliacion No. ";
                    break;
                case Modules.VENTAS:
                    TraPrinter = new VentasFormats(new DS_Ventas());
                    pdfManager = new PdfVentas(myParametro.GetParSectores() > 0 ? Arguments.Values.CurrentSector.SecCodigo : "");
                    tableName = "Ventas";
                    TitId = 4;
                    TraPdfName = "Venta No. ";
                    if (myParametro.GetParImpresionAutomatica() > 0) { ImprimirAutomatico(); }
                    break;
                case Modules.COMPRAS:
                    TraPrinter = new ComprasFormats(new DS_Compras());
                    pdfManager = new PdfCompras(new DS_Compras(), myParametro.GetParSectores() > 0 ? Arguments.Values.CurrentSector.SecCodigo : "");
                    tableName = "Compras";
                    TitId = 11;
                    TraPdfName = myParametro.GetParCambiarNombreComprasPorPushMoney() ? "PushMoney No. " : "Compra No. ";
                    break;
                case Modules.COTIZACIONES:
                    TraPrinter = new CotizacionesFormats(new DS_Cotizaciones());
                    pdfManager = new PdfCotizaciones(new DS_Cotizaciones(), myParametro.GetParSectores() > 0 ? Arguments.Values.CurrentSector.SecCodigo : "");
                    tableName = "Cotizaciones";
                    TitId = 5;
                    TraPdfName = "Cotizacion No. ";
                    break;
                case Modules.ENTREGASREPARTIDOR:
                case Modules.RECEPCIONDEVOLUCION:
                    TraPrinter = new EntregasRepartidorFormats(new DS_EntregasRepartidorTransacciones());
                    //pdfManager = new PdfEntregasDocumentos();
                    tableName = "EntregasTransacciones";
                    TitId = 27;
                    TraPdfName = Arguments.Values.CurrentModule == Modules.RECEPCIONDEVOLUCION ? "Recepcion No. " : "Entrega No. ";
                    break;
                case Modules.CONDUCES:
                    TraPrinter = new ConducesFormats(new DS_Conduces());
                    tableName = "Conduces";
                    TitId = 51;
                    TraPdfName = "Conduce No. ";
                    break;
                case Modules.CAMBIOSMERCANCIA:
                    TraPrinter = new CambiosFormats(new DS_Cambios());
                    //pdfManager = new PdfVentas(myParametro.GetParSectores() > 0 ? Arguments.Values.CurrentSector.SecCodigo : "");
                    tableName = "Cambios";
                    TitId = 6;
                    // TraPdfName = "Cambio No. ";
                    break;
                case Modules.PUSHMONEYPORPAGAR:
                    tableName = "PushMoneyPagos";
                    TitId = 52;
                    TraPrinter = new PushMoneyPorPagarFormats();
                    break;
                case Modules.TRASPASOS:
                    TitId = 20;
                    tableName = "TransferenciasAlmacenes";
                    TraPdfName = "Traspaso No. ";
                    TraPrinter = new TraspasosFormats();
                    break;
                case Modules.ENTREGASMERCANCIA:
                    TitId = 17;
                    tableName = "Entregas";
                    TraPrinter = new EntregasFormats();
                    break;
                case Modules.PROMOCIONES:
                    TitId = 19;
                    tableName = "Entregas";
                    TraPrinter = new EntregasFormats();
                    break;
                case Modules.REQUISICIONINVENTARIO:
                    TitId = 24;
                    tableName = "RequisicionesInventario";
                    TraPrinter = new RequisicionesInventarioFormats();
                    break;
                case Modules.DEPOSITOS:
                    TitId = 9;
                    tableName = "Depositos";
                    TraPrinter = new DepositosFormats(new DS_Depositos());
                    pdfManager = new PdfDepositos();
                    TraPdfName = "Deposito No. ";
                    break;
                case Modules.PROSPECTOS:
                    TitId = 53;
                    tableName = "Clientes";
                    TraPrinter = new ProspectosFormats(new DS_Clientes());
                    break;
                case Modules.SAC:
                    TitId = 15;
                    tableName = "SolicitudActualizacionClientes";
                    TraPrinter = new SACFormats(new DS_SolicitudActualizacionClientes());
                    pdfManager = new PdfSAC();
                    TraPdfName = "Solicitud de Actualizacion de Cliente No. ";
                    break;

            }

            if(myParametro.GetParTaresActualizarAuto())
                mytar.ActualizarTarea(estado: "3", TitId: TitId);

            var field = tableName.Substring(0, 3).Replace("Push", "Com") + "Secuencia";
            if(Arguments.Values.CurrentModule == Modules.TRASPASOS)
            {
                field = "TraID";
            }else if(Arguments.Values.CurrentModule == Modules.PROSPECTOS)
            {
                field = "CliID";
            }
            else if (Arguments.Values.CurrentModule == Modules.SAC)
            {
                field = "SACSecuencia";
            }

            CurrentTransaccionRowguid = Functions.GetrowguidTransaccion(tableName, field, traSecuencia);

            BindingContext = this;

            InitializeComponent();

            dialogImpresion.OnCancelar = OnCancelarImpresion;
            dialogImpresion.OnAceptar = OnAceptarImpresion;
            dialogImpresion.SetCopiasImpresionByTitId(TitId);

            if (!Ispreliminar && new DS_Estados().IsValidToPrint())
            {
                Imprimirbutton.Opacity = 0.3;
                Imprimirbutton.IsEnabled = false;
            }
        }

        private void OnCancelarImpresion()
        {
            ShowPrinterDialog = false;
        }

        private void OnAceptarImpresion(int copias)
        {
            if (isbusy2)
            {
                return;
            }

            isbusy2 = true;

            var limiteMaximoCopias = myParametro.GetParComprasLimiteMaximoCopiasImpresion();

            if (limiteMaximoCopias > 0 && copias > limiteMaximoCopias)
            {
                DisplayAlert(AppResource.Warning, AppResource.PushMoneyLimitMessage + limiteMaximoCopias.ToString(), AppResource.Aceptar);
                isbusy2 = false;
                return;
            }

            AceptarImpresion(copias);

            isbusy2 = false;
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        private async void AtacharImagen(object sender, EventArgs args)
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;
                RaiseOnPropertyChanged(nameof(IsBusy));

                await Navigation.PushAsync(new CameraPage(CurrentTransaccionSecuencia.ToString(), tableName));

            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Error, e.Message, AppResource.Aceptar);
                IsBusy = false;
                RaiseOnPropertyChanged(nameof(IsBusy));
            }

            IsBusy = false;
            RaiseOnPropertyChanged(nameof(IsBusy));
        }
        private void Firmar(object sender, EventArgs args)
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;
                RaiseOnPropertyChanged(nameof(IsBusy));

                Navigation.PushModalAsync(new FirmaModal(tableName, CurrentTransaccionSecuencia, TitId));

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                IsBusy = false;
            }

            IsBusy = false;
            RaiseOnPropertyChanged(nameof(IsBusy));
        }
        private void Imprimir(object sender, EventArgs args)
        {
            ShowPrinterDialog = true;/*

            if (isbusy2)
            {
                return;
            }

            isbusy2 = true;
            var porDefecto = myParametro.GetParDevolucionesCopiasPorDefecto();

            if (Arguments.Values.CurrentModule == Modules.COMPRAS)
            {
                porDefecto = myParametro.GetParComprasCopiasPorDefecto();
            }
            else if (Arguments.Values.CurrentModule == Modules.COBROS)
            {
                porDefecto = myParametro.GetParRecibosCopiasPorDefecto();
            }

            string copias;
            if (porDefecto > 0)
            {
                copias = porDefecto.ToString();
            }
            else
            {
                copias = await DisplayActionSheet("Elija la cantidad de copias a imprimir", "Cancelar", null, new string[] { "1", "2", "3", "4", "5" });
            }

            int.TryParse(copias, out int intCopias);

            var limiteMaximoCopias = myParametro.GetParComprasLimiteMaximoCopiasImpresion();

            if (limiteMaximoCopias > 0 && intCopias > limiteMaximoCopias)
            {
                await DisplayAlert("Aviso", "La cantidad maxima de copias para PushMoney son " + limiteMaximoCopias.ToString(), "Aceptar");
                isbusy2 = false;
                return;
            }

            if (intCopias > 0)
            {
                AceptarImpresion(intCopias);
            }

            isbusy2 = false;*/
        }

        private void ImprimirAutomatico()
        {
            if (isbusy2)
            {
                return;
            }

            isbusy2 = true;

            //var copiasAutomaticas = 0;
            var intCopias = 0;
            intCopias = myParametro.GetParImpresionAutomatica();

            //int.TryParse(copias, out int intCopias);

            if (intCopias > 0)
            {
                AceptarImpresion(intCopias);
            }

            isbusy2 = false;

        }

        private async void Compartir(object sender, EventArgs args)
        {
            try
            {
                if (pdfManager == null)
                {
                    return;
                }

                if (ParPedidosFirmaObligatoria && Arguments.Values.CurrentModule == Modules.PEDIDOS)
                {
                    var traImagen = new DS_TransaccionesImagenes();
                    var traFirma = traImagen.GetFirmaByTransaccion(TitId, CurrentTransaccionSecuencia.ToString());
                    //La firma solo si es presencial
                    if (traFirma == null && Arguments.Values.CurrentTipoVisita == 1)
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.SignNecessary, AppResource.Aceptar);
                        return;
                    }
                }

                IsBusy = true;
                RaiseOnPropertyChanged(nameof(IsBusy));

                var dialog = DependencyService.Get<IShareDialog>();

                var filePath = await pdfManager.GeneratePdf(CurrentTransaccionSecuencia, false);

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    throw new Exception(AppResource.ErrorGeneratingPdfFile);
                }

                if (myParametro.GetParAsuntoEnvio())
                {
                    await dialog.Show(AppResource.Share, TraPdfName + CurrentTransaccionSecuencia + " - " + Arguments.Values.CurrentClient.CliNombre, filePath);
                }
                else
                {
                    await dialog.Show(AppResource.Share, TraPdfName + CurrentTransaccionSecuencia + " - Usuario: " + Arguments.CurrentUser.RepCodigo, filePath);
                }
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
            RaiseOnPropertyChanged(nameof(IsBusy));
        }

        //public async Task SendEmail()
        //{
        //    try
        //    {

        //        if (pdfManager == null)
        //        {
        //            return;
        //        }

        //        IsBusy = true;
        //        RaiseOnPropertyChanged(nameof(IsBusy));

        //        var dialog = DependencyService.Get<IShareDialog>();

        //        var filePath = await pdfManager.GeneratePdf(CurrentTransaccionSecuencia, false);

        //        if (string.IsNullOrWhiteSpace(filePath))
        //        {
        //            throw new Exception(AppResource.ErrorGeneratingPdfFile);
        //        }

        //        List<string> recipients = new List<string>();
        //        recipients.Add("gerald95@gmail.com");
        //        string subject = "Envio de Correo PDF";
        //        string body = "Prueba de correo";

        //        var message = new EmailMessage
        //        {
        //            Subject = subject,
        //            Body = body,
        //            To = recipients,

        //        };

        //        var file = Path.Combine(filePath);
        //        message.Attachments.Add(new EmailAttachment(file));
        //        await Email.ComposeAsync(message);

        //    }
        //    catch (FeatureNotSupportedException fbsEx)
        //    {
        //        Email is not supported on this device
        //    }
        //    catch (Exception ex)
        //    {
        //        Some other exception occurred
        //    }
        //}


        private void PonerComentario(object sender, EventArgs args) { Navigation.PushAsync(new ComentariosPage(CurrentTransaccionSecuencia)); }
        private async void Salir(object sender, EventArgs args)
        {
            IsBusy = true;

            if (ParPedidosFirmaObligatoria && Arguments.Values.CurrentModule == Modules.PEDIDOS)
            {
                var traImagen = new DS_TransaccionesImagenes();
                var traFirma = traImagen.GetFirmaByTransaccion(TitId, CurrentTransaccionSecuencia.ToString());
                //La firma solo si es presencial
                if (traFirma == null && Arguments.Values.CurrentTipoVisita == 1)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.SignNecessary, AppResource.Aceptar);
                    IsBusy = false;
                    return;
                }
            }

            if(EntregasSecuencias != null && EntregasSecuencias.Count > 0 && new DS_EntregasRepartidorTransacciones().HayEntregasSinImprimir(EntregasSecuencias))
            {
                if (!await DisplayAlert(AppResource.Warning, AppResource.DeliveryWithoutPrintingWannaContinueQuestion, AppResource.Continue, AppResource.Cancel))
                {
                    IsBusy = false;
                    return;
                }
            }
            //mamedina:queseto???//esto se realizo para evitar pasar por pedidos y despues por pedidos detalle, a la hora de terminar una transaccion 
            if(Arguments.Values.ANTSMODULES == Modules.ANTMODULE)
            {
                Arguments.Values.CurrentModule = Modules.CONTEOSFISICOS;
                Arguments.Values.ANTSMODULES = Modules.END;
                Arguments.Values.CurrentTraSecuencia = -1;
                Navigation.RemovePage(Navigation.NavigationStack[4]);
                Navigation.RemovePage(Navigation.NavigationStack[3]);
                Navigation.RemovePage(Navigation.NavigationStack[2]);
                await Navigation.PushAsync(new SuccessPage(AppResource.PhysicalCountSavedUpper, DS_RepresentantesSecuencias.GetLastSecuencia("ConteosFisicos") - 1, (DS_RepresentantesSecuencias.GetLastSecuencia("Ventas") - 1)));
                Navigation.RemovePage(this);
            }
            else if (Arguments.Values.ANTSMODULES == Modules.END)
            {
                await Navigation.PopAsync(true);
                if (myParametro.GetParConteoFisicoMultiAlmacenAll() && !myCont.ValidateAlmacenesConConteoFisico(/*listalmid*/myParametro.GetParConteoFisicoAlmacenesParaContar(), Arguments.Values.CurrentCuaSecuencia))
                {
                    HomeViewModel.getInstance().PrepareGoConteoFisico(almId: -1, Auditor: Arguments.CurrentAud, isfromsucces: true);
                    IsBusy = false;
                    return;
                }
                if (myParametro.GetParDepositoconConteo())
                {
                    HomeViewModel.getInstance().PrepareGoDepositos(true);
                }
                else
                {
                    HomeViewModel.getInstance().GoAbrirCerrarCuadres(true);
                }
                Arguments.CurrentAud = "";
            }
            else
            {
                if (Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS)
                {
                    PedidosPage.Finish = false;
                    PedidosDetallePage.Finish = false;
                    Navigation.RemovePage(Navigation.NavigationStack[3]);
                    Navigation.RemovePage(Navigation.NavigationStack[2]);
                    await Navigation.PopAsync(true);

                    if (myParametro.GetParConteoFisicoMultiAlmacenAll() && !myCont.ValidateAlmacenesConConteoFisico(/*listalmid*/myParametro.GetParConteoFisicoAlmacenesParaContar(), Arguments.Values.CurrentCuaSecuencia))
                    {
                        HomeViewModel.getInstance().PrepareGoConteoFisico(almId: -1, Auditor: Arguments.CurrentAud, isfromsucces: true);
                        IsBusy = false;
                        return;
                    }

                    Arguments.Values.AlmId = -1;

                    if (myParametro.GetParDepositoconConteo())
                    {
                        HomeViewModel.getInstance().PrepareGoDepositos(true);
                    }
                    else
                    {
                        HomeViewModel.getInstance().GoAbrirCerrarCuadres(true);
                    }
                    Arguments.CurrentAud = "";
                }
                else
                {
                    await Navigation.PopAsync(true);
                }
            }
            IsBusy = false;
        }

        private async Task printCopies(int Copias, int preFormat)
        {
            for (int x = 0; x < Copias; x++)
            {
                IsBusy = true;
                RaiseOnPropertyChanged(nameof(IsBusy));

                await ImprimirTransaccion(preFormat);

                IsBusy = false;
                RaiseOnPropertyChanged(nameof(IsBusy));

                if (Copias > 1 && x != Copias - 1)
                {
                    await DisplayAlert(AppResource.PrintCopy, AppResource.CutPapelMessage, AppResource.Print);
                }

            }
        }

        public async void AceptarImpresion(int Copias)
        {
            try
            {
                var preFormat = -1;
                if(TitId == 1 && myParametro.GetFormatoImpresionPedidos().Count() > 1)
                {
                    preFormat = await ShowAlertChooseFormat();

                    if(preFormat == -1)
                    {
                        return;
                    }
                }

                await printCopies(Copias, preFormat);

                if (Arguments.Values.CurrentModule == Modules.COBROS && EntregasSecuencias != null && EntregasSecuencias.Count > 0)
                {
                    TraPrinter = new EntregasRepartidorFormats(new DS_EntregasRepartidorTransacciones(), true);
                    //cambio el printer para que en vez de cobros imprima entregas

                    //foreach (var entSecuencia in EntregasSecuencias)
                    //{
                    //    CurrentTransaccionSecuencia = entSecuencia;
                    //    await printCopies(Copias, printer, preFormat);
                    //}
                    if (EntregasSecuencias.Count == 1)
                    {
                        CurrentTransaccionSecuencia = EntregasSecuencias.FirstOrDefault();                        
                    }
                    else
                    {
                        CurrentTransaccionSecuencia = -1;//Para que imprima las facturas de las entregas de manera unificada
                    }
                    
                    await printCopies(Copias, preFormat);

                }
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.ErrorPrintingTransaction, e.Message, AppResource.Aceptar);
            }
            ShowPrinterDialog = false;
            IsBusy = false;
            RaiseOnPropertyChanged(nameof(IsBusy));
            if (DS_RepresentantesParametros.GetInstance().GetParRecibosNCPorDescuentoProntoPago() == 1)
            {
                ImprimirNCDpp();
            }
        }

        private async void ImprimirNCDpp()
        {
            try
            {

                if (Arguments.Values.CurrentModule != Modules.COBROS || !myRec.GetReciboTieneNCPorDpp(CurrentTransaccionSecuencia) || !(TraPrinter is FormatosRecibos formats))
                {
                    return;
                }

                var result = await DisplayAlert(AppResource.Warning, AppResource.PrintCreditCashForDiscountQuestion, AppResource.Print, AppResource.Cancel);

                if (!result)
                {
                    return;
                }

                var copias = await DisplayActionSheet(AppResource.ChoosePrinterCopies, AppResource.Cancel, null, new string[] { "1", "2", "3", "4", "5" });

                int.TryParse(copias, out int Copias);

                var ncs = myRec.GetNCDppRecibos(CurrentTransaccionSecuencia);


                foreach (var NC in ncs)
                {
                    for (int x = 0; x < Copias; x++)
                    {
                        IsBusy = true;
                        RaiseOnPropertyChanged(nameof(IsBusy));

                        await Task.Run(() =>
                        {
                            bool isempresabysector = DS_RepresentantesParametros.GetInstance().GetParEmpresasBySector();

                            if (Printer == null)
                            {
                                Printer = new PrinterManager(isempresabysector ? Arguments.Values.CurrentSector?.SecCodigo ?? "" : "");
                            }
                            else if (isempresabysector)
                            {
                                Printer.Empresa = new DS_Empresa().GetEmpresa(Arguments.Values.CurrentSector?.SecCodigo);
                            }
                            formats.PrintNCDpp(NC, Printer);
                        });

                        IsBusy = false;
                        RaiseOnPropertyChanged(nameof(IsBusy));

                        if (Copias > 1 && x != Copias - 1)
                        {
                            await DisplayAlert(AppResource.PrintCopy, AppResource.CutPapelMessage, AppResource.Print);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.ErrorPrinting, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
            RaiseOnPropertyChanged(nameof(IsBusy));
        }

        private Task ImprimirTransaccion(int preFormat)
        {
            return Task.Run(() =>
            {
                bool isempresabysector = DS_RepresentantesParametros.GetInstance().GetParEmpresasBySector();

                if (Printer == null || !Printer.IsConnectionAvailable)
                {
                    Printer = new PrinterManager(isempresabysector ? Arguments.Values.CurrentSector?.SecCodigo ?? "" : "");
                }else if(isempresabysector)
                {
                    Printer.Empresa = new DS_Empresa().GetEmpresa(Arguments.Values.CurrentSector?.SecCodigo);
                }

                var traSecuencia = CurrentTransaccionSecuencia;

                if (Arguments.Values.CurrentModule == Modules.ENTREGASREPARTIDOR && myParametro.GetParImprimirEntregasDeLaVisita() && (EntregasSecuencias == null || EntregasSecuencias.Count == 0))
                {
                    traSecuencia = -1;
                }

                TraPrinter.Print(traSecuencia, false, Printer, CurrentTransaccionRowguid, preFormat);

                if(CurrentTransaccionSecuencia2 > -1 && Arguments.Values.CurrentModule == Modules.INVFISICO 
                && myParametro.GetParInventarioFisico() == 2 && myParametro.GetParCrearPedidoByInventarioFisico())
                {
                    TraPrinter2 = new PedidosFormats(new DS_Pedidos());
                    tableName2 = "Pedidos";
                    TitId2 = 1;
                    TraPdfName2 = "Pedido No. ";

                    traSecuencia = CurrentTransaccionSecuencia2;
                    TraPrinter2.Print(traSecuencia, false, Printer, CurrentTransaccionRowguid);
                }
                else if (CurrentTransaccionSecuencia2 > -1 && !DS_RepresentantesParametros.GetInstance().GetParCuadreImprimirFacturasxFaltantes())
                {
                    TraPrinter2 = new VentasFormats(new DS_Ventas());
                    //pdfManager2 = new PdfVentas(myParametro.GetParSectores() > 0 ? Arguments.Values.CurrentSector.SecCodigo : "");
                    tableName2 = "Ventas";
                    TitId2 = 4;
                    TraPdfName2 = "Venta No. ";

                    traSecuencia = CurrentTransaccionSecuencia2;
                    TraPrinter2.Print(traSecuencia, false, Printer, CurrentTransaccionRowguid);
                }

            });
        }

        private async Task<int> ShowAlertChooseFormat()
        {
            var formatos = myParametro.GetFormatoImpresionPedidos();

            var nombres = new DS_UsosMultiples().GetFormatosImpresionPedidos();

            var buttons = new List<string>();

            foreach(var f in formatos)
            {
                var item = nombres.Where(x => x.CodigoUso == f.ToString()).FirstOrDefault();

                if(item != null)
                {
                    buttons.Add(item.Descripcion);
                }
            }

            if(buttons.Count == 0)
            {
                return -1;
            }

            var result = await DisplayActionSheet(AppResource.SelectPrintFormat, AppResource.Cancel, null, buttons: buttons.ToArray());

            var picked = nombres.Where(x => x.Descripcion == result).FirstOrDefault();

            if(picked != null && int.TryParse(picked.CodigoUso, out int format))
            {
                return format;
            }

            return -1;
        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //private bool firstTime = true;
        protected override void OnAppearing()
        {
            base.OnAppearing();

            IsBusy = false;
            RaiseOnPropertyChanged(nameof(IsBusy));

           /* if (firstTime && myParametro.GetParTransaccionesConfirmarCliente())
            {
                firstTime = false;
                DisplayAlert(AppResource.Warning, AppResource.TransactionCreatedInNameOf + Arguments.Values.CurrentClient.CliNombreCompleto, AppResource.Aceptar);
            }*/
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if(myParametro.GetParInitPrinterManager())
            {
                PrinterManager.ConnToClose();
            }
        }
    }
}