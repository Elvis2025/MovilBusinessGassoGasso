using Microsoft.AppCenter.Crashes;
using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Internal;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.Model.Internal.Structs.Args;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.views;
using MovilBusiness.Views.Components.Modals;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MovilBusiness.viewmodel
{
    public class PedidosDetalleViewModel : BaseViewModel
    {
        private Totales totales;

        private ProductosTemp totalQuintales;
        //public decimal TotalQuintales { get; set; }
        public bool ShowDescuentoOfertasRow1 { get; set; }
        public bool ShowDescuentoOfertasRow2 { get; set; }
        public Totales Totales { get => totales; private set { totales = value; RaiseOnPropertyChanged(); } }
        public ProductosTemp TotalQuintales { get => totalQuintales; private set { totalQuintales = value; RaiseOnPropertyChanged(); } }

        private ObservableCollection<ProductosTemp> productos;
        public ObservableCollection<ProductosTemp> Productos { get => productos; set { productos = value; RaiseOnPropertyChanged(); } }

        private DS_Productos myProd;
        private DS_Pedidos myPed;
        private DS_Devoluciones myDev;
        private DS_InventariosFisicos myInvF;
        private DS_Ofertas myOfe;
        private DS_DescuentosRecargos myDes;
        private DS_Visitas myVis;
        private DS_Ventas myVen;
        private DS_Compras myCom;
        private DS_Cotizaciones myCot;
        private DS_ConteosFisicos myContFis;
        private DS_Clientes myCli;
        private DS_CondicionesPago myCond;
        private DS_RequisicionesInventario myReq;
        private DS_ColocacionProductos myColMerc;
        private DS_HistoricoFacturas myHistf;
        private DS_UsosMultiples myUsu;
        private DS_Monedas myMon;

        private int CurrentTransaccionSecuencia = -1;
        //private int CurrentTransaccionSecuencia2 = -1;
        private bool FromVerDetalle { get; set; } = false;
        private bool showprinter = false;
        public bool ShowPrinter { get => showprinter; set { showprinter = value; RaiseOnPropertyChanged(); } }


        private bool ParPedidosAceptarDecimales = false;
        public bool ShowQuintales { get; set; }
        public bool ShowFlete { get; set; }
        
        private PedidosDetalleArgs Args { get; set; }

        //NO USAR VARIABLES ESTATICAS Y USAR NOMBRES MAS ENTENDIBLES
        //public static bool Back;
        private bool canceloOfertasMancomunadas = false;

        public List<FormasPagoTemp> ComprasFormasPago { get; set; }
        public List<ProductosTemp> ProdFaltantes { get; set; }
        public List<ProductosTemp> ProdFaltantesCant { get; set; }

        public PedidosDetalleViewModel(Page page, PedidosDetalleArgs args, bool FromTransacciones = false, CuentasxCobrar documento = null) : base(page)
        {

            SaveCommand = new Command(() =>
            {
                SaveOrder(args: args);

            }, () => IsUp);

            ShowDescuentoOfertasRow1 = myParametro.GetParShowDescuentoOfertasTipoRow()==1 && (CurrentModules == Modules.VENTAS || CurrentModules == Modules.PEDIDOS );
            ShowDescuentoOfertasRow2 = (myParametro.GetParShowDescuentoOfertasTipoRow()==2 || myParametro.GetParShowDescuentoOfertasTipoRow() ==-1 ) && (CurrentModules == Modules.VENTAS || CurrentModules == Modules.PEDIDOS );
            Args = args;
            FromVerDetalle = FromTransacciones;
            ShowQuintales = myParametro.GetParPedidosMostrarQuintales();
            ShowFlete = myParametro.GetCalculaFlete();
            ComprasFormasPago = args.comprasFormasPago;
            myProd = new DS_Productos();
            myPed = new DS_Pedidos();
            myDev = new DS_Devoluciones();
            myInvF = new DS_InventariosFisicos();
            myOfe = new DS_Ofertas();
            myDes = new DS_DescuentosRecargos();
            myVis = new DS_Visitas();
            myVen = new DS_Ventas(myProd);
            myCom = new DS_Compras(myProd);
            myCot = new DS_Cotizaciones(myProd);
            myContFis = new DS_ConteosFisicos();
            myCli = new DS_Clientes();
            myCond = new DS_CondicionesPago();
            myReq = new DS_RequisicionesInventario();
            myColMerc = new DS_ColocacionProductos();
            myHistf = new DS_HistoricoFacturas();
            myUsu = new DS_UsosMultiples();
            myMon = new DS_Monedas();

            canceloOfertasMancomunadas = false;
            CurrentModules = Arguments.Values.CurrentModule;


            if (CurrentModules == Modules.INVFISICO || CurrentModules == Modules.COLOCACIONMERCANCIAS || CurrentModules == Modules.REQUISICIONINVENTARIO || CurrentModules == Modules.DEVOLUCIONES || CurrentModules == Modules.CONTEOSFISICOS || CurrentModules == Modules.TRASPASOS)
            {
                Totales = new Totales();
            }
            else
            {
                if (Arguments.Values.ANTSMODULES != Modules.ANTMODULE && CurrentModules != Modules.CONTEOSFISICOS)
                {
                    double porcientoDescuentoGlobal = 0;

                    if (CurrentModules == Modules.DEVOLUCIONES && myParametro.GetParDevolucionesProductosFacturas() && !myParametro.GetParHistoricoFacturasFromCuentasxCobrar())
                    {
                        porcientoDescuentoGlobal = myHistf.GetPorcientoDescuentoGeneralByFactura(Args.devArgs);
                    }
                    else
                    {
                        porcientoDescuentoGlobal = myCond.GetPorcientoDescuentoGeneralByCondicionPago(Args.ConId) > 0 ? myCond.GetPorcientoDescuentoGeneralByCondicionPago(Args.ConId) : myCli.GetPorcientoDescuentoGeneralByCliente(Arguments.Values.CurrentClient.CliID);
                    }
                    Totales = myProd.GetTempTotales((int)CurrentModules, documento: documento, porcDescuentoGeneral: porcientoDescuentoGlobal);
                }
            }
 
        }

        private async void SaveOrder(List<ProductosTemp> ProductosAjustadosConteoFisico = null, PedidosDetalleArgs args = null, bool montoMinimoAutorizado = false)
        {

            IsUp = false;

            if (IsBusy)
            {
                return;
            }

            try
            {
                if (myParametro.GetParImageForLogo())
                {
                    var t = Task.Run(() =>
                    {
                        DependencyService.Get<IPlatformImageConverter>()
                            .DecodeForEscPos(
                                 new DS_Empresa().GetEmpresa(
                                    DS_RepresentantesParametros.GetInstance().GetParEmpresasBySector() ?
                                    Arguments.Values.CurrentSector?.SecCodigo : ""
                                 ).EmpLogo, 370, 180
                                 );
                    });

                }

                if (!myParametro.GetParComprasNoUsarDependiente())
                {
                    if (!myParametro.GetDependienteNoObligatorio() && CurrentModules == Modules.COMPRAS && Args.CompraDependiente == null)
                    {
                        throw new Exception(AppResource.DependentIsNullWarning);
                    }
                }                

                IsBusy = true;
                var loader = new TaskLoader() { SqlTransactionWhenRun = true };

                foreach(var prod in Productos.Where(p => p.CheckValueForInvFis))
                {
                    SqliteManager.GetInstance().Execute($"update ProductosTemp set CheckValueForInvFis = 1 where ProID = {prod.ProID}");
                }

                var isPreliminar = false;

                if ((CurrentModules == Modules.PEDIDOS || CurrentModules == Modules.COTIZACIONES) && myParametro.GetParPedidosPreliminar(CurrentModules.ToString()) && !montoMinimoAutorizado)
                {
                    var result = "";
                    if (myParametro.GetParPedidosNoCambiarDefinitivoAPreliminar())
                    {
                        var pedidoEdit = myPed.GetBySecuencia(Args.IsEditing ? Args.EditedTraSecuencia : -1, false);
                        var estadoPedido = pedidoEdit != null ? pedidoEdit.PedEstatus : -1;
                        result = (estadoPedido == 1 || estadoPedido == 2) ? AppResource.Definitive : await DisplayActionSheet(CurrentModules == Modules.PEDIDOS ? AppResource.HowWantSaveOrderQuestion : AppResource.HowWantSaveQuoteQuestion, AppResource.Cancel, new string[] {   AppResource.Preliminary, AppResource.Definitive });
                    }
                    else
                    {
                        result = await DisplayActionSheet(CurrentModules == Modules.PEDIDOS ? AppResource.HowWantSaveOrderQuestion : AppResource.HowWantSaveQuoteQuestion, AppResource.Cancel, new string[] { AppResource.Preliminary, AppResource.Definitive });
                    }

                    if(result == AppResource.Preliminary)
                    {
                        isPreliminar = true;
                    }
                    else if(result == AppResource.Definitive)
                    {
                        if(CurrentModules == Modules.PEDIDOS && myParametro.GetParPedidosValidaMontoMinimo() == 2)
                        {
                            int NumeroTransaccion = Args.IsEditing ? Args.EditedTraSecuencia : DS_RepresentantesSecuencias.GetLastSecuencia("Pedidos");
                            var resultTotal = Math.Round(Totales.Total, 2);
                            var resultClienteBalance = Math.Round(Arguments.Values.CurrentClient.Cliente_Balance + Totales.Total, 2);
                            if (myParametro.GetParConvertirBalanceADolares() && Args.MonCodigo != "USD")
                            {
                                var monedas = new DS_Monedas().GetMonedas("USD");
                                resultTotal = Math.Round(resultTotal / monedas[0].MonTasa, 2);
                                resultClienteBalance = Math.Round(Arguments.Values.CurrentClient.Cliente_Balance + (Totales.Total / monedas[0].MonTasa), 2);
                            }

                            double pedidoMontoMinimo = !string.IsNullOrWhiteSpace(Args.MonCodigo) ? myParametro.GetParPedidosMontoMinimo(Args.MonCodigo) : 0.00;
                            string monedaSigla = !string.IsNullOrWhiteSpace(Args.MonCodigo) ? myMon.GetMonedaByMonCodForDep(Args.MonCodigo).MonSigla : "RD$";

                            if (pedidoMontoMinimo > resultTotal && Arguments.Values.CurrentModule == Modules.PEDIDOS)
                            {
                                var resultMinimo = await DisplayAlert(AppResource.Warning, AppResource.OrderAmountMustGreaterThanMinimumConfigured.Replace("@1", monedaSigla).Replace("@2", pedidoMontoMinimo.ToString()).Replace("@3", monedaSigla).Replace("@4", resultTotal.ToString()), AppResource.Authorize, AppResource.Cancel);

                                if (resultMinimo)
                                {
                                    await Application.Current.MainPage.Navigation.PushModalAsync(new AutorizacionesModal(false, NumeroTransaccion, 1, null, false, false)
                                    {
                                        OnAutorizacionUsed = (autId) =>
                                        {
                                            isPreliminar = false;
                                            SaveOrder(ProductosAjustadosConteoFisico, args, true);
                                        }
                                    });
                                    IsBusy = false;
                                    IsUp = true;
                                    return;
                                }
                                IsBusy = false;
                                IsUp = true;
                                return;
                            }
                        }

                        isPreliminar = false;
                    }
                    else
                    {
                        IsBusy = false;
                        IsUp = true;
                        return;
                    }
                }

                var parCrearFacturaPorFaltanteConteoFisico = myParametro.GetParCrearFacturaByConteoFisico();
                var parCrearPedidoPorFaltanteInventarioFisico = myParametro.GetParCrearPedidoByInventarioFisico();

                var cantidadProductosFaltantes = 0;

                if (CurrentModules == Modules.CONTEOSFISICOS || (CurrentModules == Modules.INVFISICO && myParametro.GetParInventarioFisico() == 2))
                {
                    int almid = -1;
                    if (Args.CurrentAlmacenConteo != null)
                    {
                        almid = myParametro.GetParAlmacenIdParaMelma() == Args.CurrentAlmacenConteo.AlmID ? Args.CurrentAlmacenConteo.AlmID : myParametro.GetParAlmacenVentaRanchera();
                    }

                    if (Args.CurrentAlmacenConteo != null && myParametro.GetParConteoConVariosAlmacenes())
                    {
                        almid =  Args.CurrentAlmacenConteo.AlmID ;
                    }

                    if (Args.CurrentAlmacenConteo != null && myParametro.GetParConteoConAlmacenDespachoyDevolucion())
                    {
                        almid = myParametro.GetParAlmacenIdParaDevolucion() == Args.CurrentAlmacenConteo.AlmID ? Args.CurrentAlmacenConteo.AlmID : myParametro.GetParAlmacenIdParaDespacho();
                    }
                    
                    if(CurrentModules == Modules.INVFISICO && myParametro.GetParInventarioFisico() == 2)
                    {
                        cantidadProductosFaltantes = myInvF.GetProductosInInventarioConFaltantes(Arguments.Values.CurrentClient.CliID, IsForValid: true).Count;
                    }
                    else
                    {
                        cantidadProductosFaltantes = myParametro.GetParConteosFisicosLotesAgrupados() ? myContFis.GetProductosInInventarioConFaltantesyLotesAgrupados(myParametro.GetParClienteForRepresentantes(), Args.CurrentAlmacenConteo != null ? almid : -1, IsForValid: true).Count : myContFis.GetProductosInInventarioConFaltantes(myParametro.GetParClienteForRepresentantes(), Args.CurrentAlmacenConteo != null ? almid : -1, IsForValid: true).Count;
                    }
                    
                }    

                if (parCrearFacturaPorFaltanteConteoFisico && !DS_Representantes.HasNCF(Arguments.CurrentUser.RepCodigo) && CurrentModules == Modules.CONTEOSFISICOS && cantidadProductosFaltantes > 0)
                {
                    IsUp = true;
                    throw new Exception(AppResource.UserHasNotReceiptTypeDefined);
                }

                if (parCrearFacturaPorFaltanteConteoFisico && !myCli.ClienteTieneNcfValido(Arguments.Values.CurrentClient) && CurrentModules == Modules.CONTEOSFISICOS && cantidadProductosFaltantes > 0)
                {
                    IsUp = true;
                    throw new Exception(AppResource.CustomerHasNotValidReceiptType);
                }

                if (parCrearFacturaPorFaltanteConteoFisico && string.IsNullOrWhiteSpace(Arguments.Values.CurrentClient.LiPCodigo) && !myParametro.GetParNoListaPrecios() && CurrentModules == Modules.CONTEOSFISICOS && cantidadProductosFaltantes > 0)
                {
                    IsUp = true;
                    throw new Exception(AppResource.CannotMakeSalesCustomerHasNotPriceList);
                }

                if (myParametro.GetParCrearFacturaByConteoFisico() && !myCli.VarificarSiExisteCliente(myParametro.GetParClienteForRepresentantes()) && CurrentModules == Modules.CONTEOSFISICOS)
                {
                    IsUp = true;
                    throw new Exception(AppResource.CustomerNotExists);
                }

                if (CurrentModules == Modules.DEVOLUCIONES && myParametro.GetParNotaCreditoPorDevolucion())
                {
                    var result = true;
                    result = await DisplayAlert(AppResource.Warning, AppResource.CreditNoteWillBeCreatedBasedInReturn, AppResource.Continue, AppResource.Cancel);
                    IsBusy = false;
                    IsUp = true;

                    if (!result)
                    {
                        return;
                    }

                }

                if (CurrentModules == Modules.CONTEOSFISICOS && parCrearFacturaPorFaltanteConteoFisico && cantidadProductosFaltantes > 0)
                {
                    var result = true;

                    IsBusy = false;
                    IsUp = true;
                    if (myParametro.GetParConteoFisicoAjustarFaltante())
                    {
                        if (ProductosAjustadosConteoFisico == null) {

                            await DisplayAlert(AppResource.Warning, AppResource.InvoiceWillbeCreatedChargedToSellerByDifferenceInCount + ", " + AppResource.SelectDesiredOptionBelow);

                            var option = await DisplayActionSheet(AppResource.SelectAnOption, buttons: new string[] { AppResource.AdjustDifference, AppResource.Continue, AppResource.Cancel });


                            if(option == AppResource.AdjustDifference)
                            {
                                await PushModalAsync(new AjustarProductosFaltantesModal((p) => { SaveOrder(p); }, Args.CurrentAlmacenConteo != null ? Args.CurrentAlmacenConteo.AlmID : -1));
                            }
                            else if(option == AppResource.Continue)
                            {
                                result = true;
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        result = await DisplayAlert(AppResource.Warning, AppResource.InvoiceWillbeCreatedChargedToSellerByDifferenceInCount, AppResource.Continue, AppResource.Cancel);
                        
                        if(myParametro.GetParMostrarFacturasVentasEnConteos() && result && Arguments.Values.ANTSMODULES == Modules.NULL)
                        {
                            Arguments.Values.ANTSMODULES = Modules.ANTMODULE;
                            CurrentModules = Modules.VENTAS;
                            await PushAsync(new PedidosDetallePage(args));
                            return;
                        }
                    }                    

                    if (!result)
                    {
                        return;
                    }
                }
                else if (Arguments.Values.ANTSMODULES == Modules.ANTMODULE)
                {
                    CurrentModules = Modules.CONTEOSFISICOS;
                }
                else if ((CurrentModules == Modules.INVFISICO && myParametro.GetParInventarioFisico() == 2) 
                    && parCrearPedidoPorFaltanteInventarioFisico && cantidadProductosFaltantes > 0 )
                {
                    var result = true;

                    IsBusy = false;
                    IsUp = true;
                    
                    result = await DisplayAlert(AppResource.Warning, AppResource.OrderWillCreateToCustomerByDifferenceInCount, AppResource.Continue, AppResource.Cancel);

                    if (!result)
                    {
                        return;
                    }
                }

                if ((CurrentModules == Modules.VENTAS || CurrentModules == Modules.PEDIDOS) && canceloOfertasMancomunadas)
                {
                    await DisplayAlert(AppResource.Warning,AppResource.OffersHaveNotBeenApplied, AppResource.Aceptar);

                    if (IsBusy)
                    {
                        await PopAsync(true);
                        return;

                    }
                }

                bool saveReciboFromVenta = false;

                CurrentTransaccionSecuencia = -1;
       
                await loader.Execute(() =>
                {
                    switch (CurrentModules)
                    {
                        case Modules.PEDIDOS:
                            CurrentTransaccionSecuencia = myPed.SavePedido(Args.ConId, Args.FechaEntrega.ToString("yyyy-MM-dd HH:mm:ss"), Args.Prioridad, Args.IsEditing, Args.EditedTraSecuencia, Args.PedOrdenCompra, isPreliminar, Args.TipoPedido, Args.PedCamposAdicionales, Args.CldDirTipo, Args.CedCodigo,Args.EnEspera, Args.PedTipoTrans, Args.CliIDMaster, Args.MonCodigo, Args.FromCopy, Totales.SubTotal, Totales.Total, Args.IsMultiEntrega,Totales.Itbis,Totales.DescuentoGeneral, Totales.PorCientoDsctoGeneral);
                            break;
                        case Modules.DEVOLUCIONES:
                            CurrentTransaccionSecuencia = myDev.SaveDevolucion(Args.devArgs, Args.IsEditing, Args.EditedTraSecuencia, Args.PedCamposAdicionales, Totales.SubTotal, Totales.Total, Totales.Itbis, Totales.DescuentoGeneral, Totales.PorCientoDsctoGeneral);
                            break;
                        case Modules.INVFISICO:
                            CurrentTransaccionSecuencia = myInvF.SaveInventario(Args.InvArea, myVis, Args.IsEditing, Args.EditedTraSecuencia, ProdFaltantes);
                            break;
                        case Modules.COLOCACIONMERCANCIAS:
                            CurrentTransaccionSecuencia = myColMerc.SaveColocacion(Args.InvArea);
                            break;
                        case Modules.VENTAS:
                            var ncf = new DS_Clientes().GetSiguienteNCF(Arguments.Values.CurrentClient);
                            CurrentTransaccionSecuencia = myVen.SaveVenta(Args.ConId, ncf,  out saveReciboFromVenta, Args.CurrentEntrega, Args.VenCantidadCanastos, Args.motivodevolucion, Totales.Total, Totales.SubTotal, Totales.Itbis, Totales.DescuentoGeneral, Totales.PorCientoDsctoGeneral, Args.PedOrdenCompra, Args.FromCopy);
                            break;
                        case Modules.COMPRAS:
                            CurrentTransaccionSecuencia = myCom.SaveCompra(Args.CompraDependiente, Args.ComTipoPago, ComprasFormasPago, Args.IsEditing, Args.EditedTraSecuencia, Totales.Total);
                            break;
                        case Modules.COTIZACIONES:
                            CurrentTransaccionSecuencia = myCot.SaveCotizacion(Args.FechaEntrega.ToString("yyyy-MM-dd HH:mm:ss"), Args.ConId, Args.TipoPedido, Args.Prioridad, Args.IsEditing, Args.EditedTraSecuencia, isPreliminar, Args.MonCodigo, Args.IsMultiEntrega, Totales.SubTotal, Totales.Total, Totales.Itbis, Totales.DescuentoGeneral, Totales.PorCientoDsctoGeneral, Args.PedCamposAdicionales);
                            break;
                        case Modules.REQUISICIONINVENTARIO:
                            CurrentTransaccionSecuencia = myReq.SaveRequisicion();
                            break;
                        case Modules.CONTEOSFISICOS:
                            int almId = -1;

                            if(Args.CurrentAlmacenConteo != null && myParametro.GetParConteoFisicoPorAlmacen())
                            {
                                almId = Args.CurrentAlmacenConteo.AlmID;
                            }
                            CurrentTransaccionSecuencia = myContFis.GuardarConteo(Arguments.Values.CurrentCuaSecuencia, Args.RepAuditor, ProductosAjustadosConteoFisico, almId, ProdFaltantes);
                            break;
                        case Modules.CAMBIOSMERCANCIA:
                            CurrentTransaccionSecuencia = new DS_Cambios().SaveCambiosMercancia(Arguments.Values.CurrentCuaSecuencia, Args.RepAuditor);
                            break;
                        case Modules.TRASPASOS:
                            CurrentTransaccionSecuencia = new DS_TransferenciasAlmacenes().GuardarTransferencia(Args.IsEntregandoTraspaso, Args.RepCodigoTraspaso);                            
                            break;
                        case Modules.PROMOCIONES:
                            CurrentTransaccionSecuencia = new DS_Entregas().GuardarEntrega(true, Args.VenCantidadCanastos);
                            break;
                        case Modules.ENTREGASMERCANCIA:
                            CurrentTransaccionSecuencia = new DS_Entregas().GuardarEntrega(false);
                            break;
                    }
                });

                PedidosPage.Finish = true;

                if (myParametro.GetParCerrarVisitaDespuesTransaccion() && CurrentModules != Modules.CONTEOSFISICOS && CurrentModules != Modules.TRASPASOS)
                {
                    OperacionesPage.CloseVisit = true;
                }

                if (Arguments.Values.ANTSMODULES == Modules.ANTMODULE)
                {
                    CurrentModules = Modules.VENTAS;
                }

                string title = AppResource.OrderSavedUpper;

                switch (CurrentModules)
                {
                    case Modules.DEVOLUCIONES:
                        title = AppResource.ReturnSavedUpper;
                        break;
                    case Modules.INVFISICO:
                        title = AppResource.InventorySavedUpper;
                        break;
                    case Modules.COLOCACIONMERCANCIAS:
                        title = AppResource.PlacementsOfMerchandiseSaved.ToUpper();
                        break;
                    case Modules.VENTAS:
                        title = AppResource.SaleSavedUpper;
                        break;
                    case Modules.COTIZACIONES:
                        title = AppResource.QuoteSavedUpper;
                        break;
                    case Modules.COMPRAS:
                        title = myParametro.GetParCambiarNombreComprasPorPushMoney() ? AppResource.PushMoneySavedUpper : AppResource.PurchaseSavedUpper;
                        break;
                    case Modules.CONTEOSFISICOS:
                        title = AppResource.PhysicalCountSavedUpper;
                        break;
                    case Modules.CAMBIOSMERCANCIA:
                        title = AppResource.MerchansideChangesSavedUpper;
                        break;
                    case Modules.TRASPASOS:
                        title = AppResource.TransferSavedUpper;
                        break;
                    case Modules.REQUISICIONINVENTARIO:
                        title = AppResource.RequisitionSavedUpper;
                        break;
                }

                if(CurrentModules == Modules.PEDIDOS && Args.ConId == myParametro.GetParPedidosConIdPrepago())
                {
                    var moneda = new DS_Monedas().GetMoneda(Arguments.Values.CurrentClient.MonCodigo);

                    await PushAsync(new RecibosTabPage(CurrentTransaccionSecuencia, moneda));
                }
                else
                if (CurrentModules == Modules.VENTAS && saveReciboFromVenta)
                {
                    var moneda = new DS_Monedas().GetMoneda(Arguments.Values.CurrentClient.MonCodigo);

                    await PushAsync(new RecibosTabPage(CurrentTransaccionSecuencia, moneda, ConId:Args.ConId, IsFromCopy: Args.FromCopy));
                }
                else if (CurrentModules == Modules.CONTEOSFISICOS && cantidadProductosFaltantes > 0)
                {
                    await PushAsync(new SuccessPage(title, CurrentTransaccionSecuencia, (DS_RepresentantesSecuencias.GetLastSecuencia("Ventas")-1)));
                }
                else if (CurrentModules == Modules.INVFISICO && cantidadProductosFaltantes > 0 && myParametro.GetParInventarioFisico() == 2)
                {
                    await PushAsync(new SuccessPage(title, CurrentTransaccionSecuencia, (DS_RepresentantesSecuencias.GetLastSecuencia("Pedidos") - 1)));
                }
                else if(Arguments.Values.ANTSMODULES == Modules.ANTMODULE)
                {
                    await PushAsync(new SuccessPage(title, (DS_RepresentantesSecuencias.GetLastSecuencia("Ventas") - 1)));
                }
                else
                {
                    await PushAsync(new SuccessPage(title, CurrentTransaccionSecuencia, Ispreliminar:isPreliminar));
                }
                //Task.WaitAll(t);
                PedidosDetallePage.Finish = true;
            }

            catch (Exception e)
            {
                IsBusy = false;
                IsUp = true;

                Crashes.TrackError(e);
                if (!string.IsNullOrWhiteSpace(e.Message) && e.Message.Contains("Constraint"))
                {
                    await DisplayAlert(AppResource.ErrorSavingTransaction, e.Data["query"].ToString(), AppResource.Aceptar);
                }
                else
                {
                    await DisplayAlert(AppResource.ErrorSavingTransaction, e.InnerException != null ? e.InnerException.Message : e.Message, AppResource.Aceptar);
                }
            }

            IsBusy = false;
            IsUp = true;
        }

        public async void LoadProducts(bool calcularOfertasYDescuentos = false, bool showOfertas = false, bool fromshow = false, bool recalcularTotales = false)
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            try
            {
                if (ShowQuintales)
                {
                    TotalQuintales = myProd.GetTotalQuintalesInTemp();
                }
                await Task.Run(() =>
                {
                    if(myParametro.GetParClientesTodosInventarios())
                    {
                        var isvalid = SqliteManager.GetInstance().Query<ProductosTemp>
                        (
                            $@"select 1 from Productos P   inner join InventariosAlmacenes I on I.ProID = P.ProID AND I.AlmID = 
                            '{myParametro.GetParAlmacenDefault()}' AND (I.invCantidad > 0 or I.InvCantidadDetalle > 0) left join ListaPrecios 
                            l on l.proid = p.proid  and l.LipCodigo = '{Arguments.Values.CurrentClient.LiPCodigo}' where  p.proid not in 
                            (select proid from ProductosTemp) And P.Proid in (select Proid from Clientesproductosvendidos where CLiid =
                            {Arguments.Values.CurrentClient.CliID}) order by p.ProDescripcion");

                        Arguments.IsValidToGetOut = isvalid == null || isvalid.Count <= 0 ? 1 : 2;
                    }


                    var parOfertasManuales = myParametro.GetParPedidosOfertasyDescuentosManuales();
                    var parDescuentoManuales = myParametro.GetParPedidosDescuentoManual();
                    if (calcularOfertasYDescuentos && !parOfertasManuales && !parDescuentoManuales  && !FromVerDetalle && !myParametro.GetParPedDescLip() && Args.CurrentEntrega == null)
                    {
                        if (!myParametro.GetDescuentoxPrecioNegociado())
                        {
                            SqliteManager.GetInstance().Execute("update ProductosTemp set Descuento = 0, DesPorciento = 0 and TitID = ? ", new string[] { ((int)CurrentModules).ToString() });
                        }  
                    }

                    if (!calcularOfertasYDescuentos && !fromshow)
                    {
                        SqliteManager.GetInstance().Execute("update ProductosTemp set Precio = 0 where ifnull(IndicadorOferta, 0) = 1 and TitID = ? ", new string[] { ((int)CurrentModules).ToString() });
                    }

                    var groupBy = myParametro.GetParTransCantAgrupar() &&
                    calcularOfertasYDescuentos && 
                    !parOfertasManuales && !FromVerDetalle;

                    Productos = new ObservableCollection<ProductosTemp>(myProd.GetResumenProductos((int)CurrentModules, !showOfertas && !parOfertasManuales, true, true, showCombo: false, mostrarPromociones: parOfertasManuales && myParametro.GetParPedidosPromociones(), forCalcularofertas: calcularOfertasYDescuentos && !parOfertasManuales && !FromVerDetalle, entrega: Args.CurrentEntrega, groupBy:groupBy, VerDetalleInCont: CurrentModules == Modules.CONTEOSFISICOS && FromVerDetalle, isVerDetalleTrans: FromVerDetalle, isFromPedidoDetalle:true));

                    if (recalcularTotales)
                    {
                        double porcientoDescuentoGlobal = 0;
                        if (CurrentModules == Modules.DEVOLUCIONES && myParametro.GetParDevolucionesProductosFacturas() && !myParametro.GetParHistoricoFacturasFromCuentasxCobrar())
                        {
                            porcientoDescuentoGlobal = myHistf.GetPorcientoDescuentoGeneralByFactura(Args.devArgs);
                        }
                        else
                        {
                            porcientoDescuentoGlobal = myCond.GetPorcientoDescuentoGeneralByCondicionPago(Args.ConId) > 0 ? myCond.GetPorcientoDescuentoGeneralByCondicionPago(Args.ConId) : myCli.GetPorcientoDescuentoGeneralByCliente(Arguments.Values.CurrentClient.CliID);
                        }
                        
                        Totales = myProd.GetTempTotales((int)CurrentModules, withoferta: true, porcDescuentoGeneral: porcientoDescuentoGlobal);
                    }
                    
                    if (calcularOfertasYDescuentos && !parOfertasManuales && !FromVerDetalle /*&& Args.CurrentEntrega == null*/)
                    {
                        if (CurrentModules == Modules.CONTEOSFISICOS || (CurrentModules == Modules.INVFISICO && myParametro.GetParInventarioFisico() == 2))
                        {
                            int almid = -1;
                            if (Args.CurrentAlmacenConteo != null)
                            {
                                almid = myParametro.GetParAlmacenIdParaMelma() == Args.CurrentAlmacenConteo.AlmID ? Args.CurrentAlmacenConteo.AlmID : myParametro.GetParAlmacenVentaRanchera();
                            }

                            if (Args.CurrentAlmacenConteo != null && myParametro.GetParConteoConVariosAlmacenes())
                            {
                                almid = Args.CurrentAlmacenConteo.AlmID;
                            }

                            if (Args.CurrentAlmacenConteo != null && myParametro.GetParConteoConAlmacenDespachoyDevolucion())
                            {
                                almid = myParametro.GetParAlmacenIdParaDevolucion() == Args.CurrentAlmacenConteo.AlmID ? Args.CurrentAlmacenConteo.AlmID : myParametro.GetParAlmacenIdParaDespacho();
                            }

                            if (CurrentModules == Modules.INVFISICO && myParametro.GetParInventarioFisico() == 2)
                            {
                                ProdFaltantes = myInvF.GetProductosInInventarioConFaltantes(Arguments.Values.CurrentClient.CliID);
                            }
                            else
                            {
                                ProdFaltantes = myParametro.GetParConteosFisicosLotesAgrupados() ? myContFis.GetProductosInInventarioConFaltantesyLotesAgrupados(myParametro.GetParClienteForRepresentantes(), Args.CurrentAlmacenConteo != null ? /*Args.CurrentAlmacenConteo.AlmID*/almid : -1) : myContFis.GetProductosInInventarioConFaltantes(myParametro.GetParClienteForRepresentantes(), Args.CurrentAlmacenConteo != null ? /*Args.CurrentAlmacenConteo.AlmID*/almid : -1);
                            }                          

                            if (ProdFaltantes.Count > 0)
                            {
                                //if (myInv.GrabarProductosTemporalesSinExistenciaInInventario())
                                //{
                                //  ProdFaltantes = myContFis.GetProductosConFaltantes();
                                // }                   
                                myProd.InsertInTemp(ProdFaltantes, IsFromCont: true);

                                if(myParametro.GetParMostrarConteosCiegos() || (CurrentModules == Modules.INVFISICO && myParametro.GetParInventarioFisico() == 2))
                                {
                                    ShowProductosConFaltantes(ProdFaltantes);
                                }
                            }

                            if (CurrentModules == Modules.INVFISICO && myParametro.GetParInventarioFisico() == 2)
                            {
                                var prodCantidadesIguales = myInvF.GetProductosInInventarioCantidadesIguales(Arguments.Values.CurrentClient.CliID);
                                if (prodCantidadesIguales.Count > 0)
                                {
                                    myProd.InsertInTemp(prodCantidadesIguales, IsFromCont: true);
                                }
                            }
                            
                        }

                        CalcularDescuentos();
                        CalcularOfertas(out bool revisionDescuentosShowed);
                        if (myParametro.GetCalculaFlete() && !string.IsNullOrEmpty(Args.PedCamposAdicionales))
                        {
                            CalcularFlete(Args.PedCamposAdicionales);
                        }
                        
                        InsertarOfertaDevolucion();

                        Productos = new ObservableCollection<ProductosTemp>(myProd.GetResumenProductos((int)CurrentModules, false, true, true, showCombo: false, mostrarPromociones: parOfertasManuales && myParametro.GetParPedidosPromociones(), forCalcularofertas: calcularOfertasYDescuentos && !parOfertasManuales && !FromVerDetalle, entrega: Args.CurrentEntrega , AgrupaLote: myParametro.GetParVentasNoAgruparxLote(), isFromPedidoDetalle: true));


                        if (!revisionDescuentosShowed && myParametro.GetParPedidosDescuentoManualGeneral() <= 0)
                        {
                            ShowRevisionDescuentos();
                        }

                        if (FromVerDetalle)//Si viene desde ver detalles no va a recalcular las ofertas dadas
                        {
                            Productos = new ObservableCollection<ProductosTemp>(myProd.GetResumenProductos((int)CurrentModules, !showOfertas, true, true, showCombo: false, showDescuentoIndicator: false, entrega:Args.CurrentEntrega, isFromPedidoDetalle: true));
                            PedidosDetallePage.ClearFromDetalle = true;
                        }
                        if (Arguments.Values.ANTSMODULES != Modules.ANTMODULE && CurrentModules != Modules.CONTEOSFISICOS && CurrentModules != Modules.REQUISICIONINVENTARIO)
                        {
                            double porcientoDescuentoGlobal = 0;
                            if (CurrentModules == Modules.DEVOLUCIONES && myParametro.GetParDevolucionesProductosFacturas() && !myParametro.GetParHistoricoFacturasFromCuentasxCobrar())
                            {
                                porcientoDescuentoGlobal = myHistf.GetPorcientoDescuentoGeneralByFactura(Args.devArgs);
                            }
                            else
                            {
                                porcientoDescuentoGlobal = myCond.GetPorcientoDescuentoGeneralByCondicionPago(Args.ConId) > 0 ? myCond.GetPorcientoDescuentoGeneralByCondicionPago(Args.ConId) : myCli.GetPorcientoDescuentoGeneralByCliente(Arguments.Values.CurrentClient.CliID);
                            }

                            Totales = myProd.GetTempTotales((int)CurrentModules, withoferta:true, porcDescuentoGeneral: porcientoDescuentoGlobal);
                        }
                    }

                    if (Arguments.Values.ANTSMODULES == Modules.ANTMODULE)
                    {
                        int almid = -1;
                        if (Args.CurrentAlmacenConteo != null)
                        {
                            almid = myParametro.GetParAlmacenIdParaMelma() == Args.CurrentAlmacenConteo.AlmID ? Args.CurrentAlmacenConteo.AlmID : myParametro.GetParAlmacenVentaRanchera();
                        }

                        if (Args.CurrentAlmacenConteo != null && myParametro.GetParConteoConVariosAlmacenes())
                        {
                            almid = Args.CurrentAlmacenConteo.AlmID;
                        }

                        if (Args.CurrentAlmacenConteo != null && myParametro.GetParConteoConAlmacenDespachoyDevolucion())
                        {
                            almid = myParametro.GetParAlmacenIdParaDevolucion() == Args.CurrentAlmacenConteo.AlmID ? Args.CurrentAlmacenConteo.AlmID : myParametro.GetParAlmacenIdParaDespacho();
                        }

                        ProdFaltantes = myParametro.GetParConteosFisicosLotesAgrupados() ? myContFis.GetProductosInInventarioConFaltantesyLotesAgrupados(myParametro.GetParClienteForRepresentantes(), Args.CurrentAlmacenConteo != null ? /*Args.CurrentAlmacenConteo.AlmID*/almid : -1) : myContFis.GetProductosInInventarioConFaltantes(myParametro.GetParClienteForRepresentantes(), Args.CurrentAlmacenConteo != null ? /*Args.CurrentAlmacenConteo.AlmID*/almid : -1);

                        if (ProdFaltantes.Count > 0)
                        {
                            //if (myInv.GrabarProductosTemporalesSinExistenciaInInventario())
                            //{
                            //  ProdFaltantes = myContFis.GetProductosConFaltantes();
                            // }
                            int numtosum = 0;
                            ProdFaltantesCant = new List<ProductosTemp>();
                            foreach (var pro in ProdFaltantes)
                            {
                                var cantidadTotal = (pro.CantidadManual * pro.ProUnidades) + pro.CantidadManualDetalle;
                                var cantidadInvTotal = (pro.InvCantidad * pro.ProUnidades) + pro.InvCantidadDetalle;

                                var resultRaw = cantidadInvTotal - cantidadTotal;

                                var resultRaw2 = resultRaw / pro.ProUnidades;

                                var cantidad = Math.Truncate(resultRaw2);

                                ProdFaltantesCant.Add(pro);
                                ProdFaltantesCant[numtosum].Cantidad = cantidad;
                                numtosum++;
                            }

                            Totales = myProd.GetTempTotales((int)Modules.CONTEOSFISICOS, productos: ProdFaltantes);
                            Productos = new ObservableCollection<ProductosTemp>(ProdFaltantesCant);
                        }
                    }

                    for (int i = 0; i < Productos.Count; i++)
                    {
                        Productos[i].ShowImagesPro = false;
                    }

                });
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        private void InsertarOfertaDevolucion()
        {
            if(CurrentModules != Modules.DEVOLUCIONES)
            {
                return;
            }

            myProd.InsertarOfertaDevolucion();
        }

        private void CalcularOfertas(out bool revisionDescuentosTriggered)
        {
            ParPedidosAceptarDecimales = myParametro.GetPedidosAceptarOfertasDecimales();
            revisionDescuentosTriggered = false;
            
            if (CurrentModules != Modules.PEDIDOS && CurrentModules != Modules.VENTAS && !myParametro.GetParCotizacionesOfertasyDescuentos())
            {
                return;
            }

            var productosOfertados = new List<ProductosTemp>();

            bool productosSinExistenciaVentas = false;

            foreach (var prod in Productos)
            {
                var ofertas = new List<Model.Ofertas>();                

                    if (myParametro.GetOfertasConSegmento())
                    {
                    ofertas = myOfe.GetOfertasDisponiblesPorSegmento(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentClient.TiNID, prod.ProID, myParametro.GetParOfertasEspecificasCliente(), Args.CurrentEntrega, prod.IndicadorOfertaForShow, Args.ConId);
                    }
                    else
                    {
                        ofertas = myOfe.GetOfertasDisponibles(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentClient.TiNID, prod.ProID, myParametro.GetParOfertasEspecificasCliente(), Args.CurrentEntrega, prod.IndicadorOfertaForShow, Args.ConId);
                    }
                
                if (ofertas.Count == 0 && myParametro.GetParOfertasEspecificasCliente())
                {
                    if (myParametro.GetOfertasConSegmento())
                    {
                        ofertas = myOfe.GetOfertasDisponiblesPorSegmento(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentClient.TiNID, prod.ProID,false, Args.CurrentEntrega, prod.IndicadorOfertaForShow, Args.ConId);
                    }
                    else
                    {
                        ofertas = myOfe.GetOfertasDisponibles(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentClient.TiNID, prod.ProID, false, Args.CurrentEntrega, prod.IndicadorOfertaForShow, Args.ConId);
                    }                    
                }

                foreach (var oferta in ofertas)
                {
                    var ofertados = new List<ProductosTemp>();
                    var productos = new List<ProductosTemp>();

                    if  (CurrentModules == Modules.VENTAS)
                    {       
                        ofertados = myOfe.CalcularOfertaParaProducto(oferta, prod.Copy(), ParPedidosAceptarDecimales, myProd, (int)CurrentModules, out productosSinExistenciaVentas, Args.CurrentEntrega, IsEditin: Args.IsEditing, productos: productos);
                    }
                    else
                    {
                         ofertados = myOfe.CalcularOfertaParaProducto(oferta, prod.Copy(), ParPedidosAceptarDecimales, myProd, (int)CurrentModules, out productosSinExistenciaVentas, IsEditin: Args.IsEditing, productos: productos);
                    }
                    
                    if (ofertados.Count > 0)
                    {
                        //int index = -1;

                        //if (oferta.OfeTipo == "7" && productos.Count > 0)
                        //{
                        //    //List<ProductosTemp> result = (List<ProductosTemp>) productosOfertados.Join(productos, p => p.ProID, pa => pa.ProID, (produ,pd) => pd);

                        //   index = productosOfertados.IndexOf(productosOfertados.Where(p => productos.Select
                        //   (o => o.ProID.ToString()).Contains(p.ProID.ToString()
                        //   )).FirstOrDefault());
                        //}                        

                        //if(index > -1)
                        //{
                        //   productosOfertados[index] = productos.Where(o => productosOfertados.Select
                        //   (p => p.ProID.ToString()).Contains(o.ProID.ToString()
                        //   )).FirstOrDefault();

                        //   productosOfertados.Add(ofertados.Where(o => !productosOfertados.Select
                        //   (p => p.ProID.ToString()).Contains(o.ProID.ToString()
                        //   )).FirstOrDefault());
                        //}
                        //else
                        //{
                            productosOfertados.AddRange(ofertados);
                        //}

                        break;
                    }

                    if (myParametro.GetParOfertasEspecificasCliente() && ofertados.Count == 0)
                    {
                        continue;
                    }

                }
            }

            if (productosSinExistenciaVentas && CurrentModules == Modules.VENTAS)
            {
                DisplayAlert(AppResource.Warning, AppResource.OffersDontGivenNotEnoughtStock);
            }


            var mancomunadas = myOfe.GetOfertasMancomunadasDisponibles(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentClient.TiNID, (int)CurrentModules, Args.CurrentEntrega, Args.ConId);

            if (mancomunadas.Count > 0 && mancomunadas != null)
            {
                var r = new RevisionProductosOfertasMancomunadasModal(mancomunadas, () =>
                {
                    LoadProducts(false, true, true,true);

                    if (myParametro.GetParRevisionOfertas() || myProd.HayOfertasConLotes())
                    {
                        var productosOfertas = myProd.GetProductosOfertas((int)CurrentModules);

                        if (productosOfertados != null && productosOfertados.Count > 0 && !productosOfertados.Any(x => x.OfeCaracteristica == "O" || x.OfeCaracteristica == "P"))
                        {
                            ShowRevisionOfertas(productosOfertados, myParametro.GetParPedidosRevisionDeDescuentos() && myProd.HayProductosConDescuento((int)CurrentModules) && !myParametro.GetParPedidosDescuentoManual() || !myParametro.GetParCotizacionesDescuentoManual() && !myParametro.GetParDevolucionesDescuentoManual());
                        }
                    }

                }, () => { canceloOfertasMancomunadas = true; }, myProd, true);
                
                //r.AplicarOfertaMancomunadaPorDefault(mancomunadas);

                r.onchange = true;
                if (!r.OfertaAutomatica && r.IsOfeMancomunadaCombo) {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        /*PushModalAsync(new RevisionProductosOfertasMancomunadasModal(mancomunadas, () =>
                        {
                            LoadProducts(false, true);

                            if (myParametro.GetParRevisionOfertas() || myProd.HayOfertasConLotes())
                            {
                                var productosOfertas = myProd.GetProductosOfertas((int)CurrentModules);

                                if (productosOfertados != null && productosOfertados.Count > 0)
                                {
                                    ShowRevisionOfertas(productosOfertados, myParametro.GetParPedidosRevisionDeDescuentos() && myProd.HayProductosConDescuento((int)CurrentModules) && !myParametro.GetParPedidosDescuentoManual() || !myParametro.GetParCotizacionesDescuentoManual());
                                }
                            }

                        },()=>{ canceloOfertasMancomunadas = true; }, myProd));*/
                        PushModalAsync(r);

                    });
                }
                return;
            }

            CalcularOfeIndicadorRebajaVenta();

            if (((myParametro.GetParRevisionOfertas()  && productosOfertados.Count > 0 && mancomunadas.Count == 0)
                || (myParametro.GetParRevisionOfertas() && myProd.HayOfertasConLotes() && mancomunadas.Count == 0)) && !productosOfertados.Any(x => x.OfeCaracteristica == "O" || x.OfeCaracteristica == "P"))
            {
                revisionDescuentosTriggered = myParametro.GetParPedidosRevisionDeDescuentos() && myProd.HayProductosConDescuento((int)CurrentModules) && !myParametro.GetParPedidosDescuentoManual() || myParametro.GetParCotizacionesDescuentoManual();

                ShowRevisionOfertas(productosOfertados, revisionDescuentosTriggered);
            }
        }

        private void CalcularDescuentos()
        {
            if (!myParametro.GetParDescuentosEnDevoluciones() && CurrentModules != Modules.PEDIDOS && CurrentModules != Modules.COTIZACIONES && CurrentModules != Modules.VENTAS)
            {
                return;
            }

            if(myParametro.GetParPedidosDescuentoManualGeneral() > 0.0)
            {
                if (Args.PorcientoDescuentoManual > 0.0)
                {
                    myDes.ActualizarDescuentoGeneralManual(Args.PorcientoDescuentoManual, (int)CurrentModules);
                }
                return;
            }

            myDes.CalcularDescuentosForProductsInTemp(Arguments.Values.CurrentClient.CliID, Args.ConId, (int)CurrentModules, Args.CurrentEntrega);
        }

        private void ShowRevisionDescuentos()
        {
            if (!myParametro.GetParPedidosRevisionDeDescuentos() || myParametro.GetParPedidosDescuentoManualGeneral() > 0.0 || !myProd.HayProductosConDescuento((int)CurrentModules) || myParametro.GetParPedidosDescuentoManual() || myParametro.GetParCotizacionesDescuentoManual() || myParametro.GetParDevolucionesDescuentoManual())
            {
                return;
            }

            Device.BeginInvokeOnMainThread(async () =>
            {
                await PushModalAsync(new RevisionDescuentosModalxaml((int)CurrentModules) { OnProductsAccepted = () => { LoadProducts(false, true); } });
            });
        }

        private void ShowRevisionOfertas(List<ProductosTemp> productos, bool goRevisionDescuentos)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                IsBusy = true;
                await PushModalAsync(new RevisionProductosOfertasModal(productos, myProd, goRevisionDescuentos, (int)CurrentModules, ventaDesdeEntrega:Args.CurrentEntrega != null) { OnProductsAccepted = () => { CalcularOfeIndicadorRebajaVenta();  LoadProducts(false, true, true,true); } });
                IsBusy = false;
            });
        }

        private void CalcularOfeIndicadorRebajaVenta()
        {            
            myProd.ActualizarIndicadorRebajaVenta();
        }

        private void ShowProductosConFaltantes(List<ProductosTemp> productos)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                IsBusy = true;
                await PushModalAsync(new RevisionProductosConFaltantes(productos, () =>
                {
                    LoadProducts(false, true);
                }));
                IsBusy = false;
            });
        }

        public async void GoBack()
        {
            await PopAsync(true);
        }


        public void ClearTemp()
        {
            myProd.ClearTemp((int)CurrentModules);
            PedidosDetallePage.ClearFromDetalle = false;
        }

        public void SubscribeToListeners()
        {

            MessagingCenter.Subscribe<string, string>(this, "ShowInventario", (sender, args) =>
            {
                if (int.TryParse(args, out int proId))
                {
                    ShowInventario(proId);
                }
            });
        }

        private InventarioAlmacenesModal prodInventarioAlmacenModal;
        private async void ShowInventario(int proId)
        {
            try
            {
                if (IsBusy)
                {
                    return;
                }

                IsBusy = true;

                if (prodInventarioAlmacenModal == null)
                {
                    prodInventarioAlmacenModal = new InventarioAlmacenesModal();
                }

                prodInventarioAlmacenModal.LoadInvDisponible(proId);

                await PushModalAsync(prodInventarioAlmacenModal);
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        private void CalcularFlete(string camposAdicionales)
        {
            dynamic jsonObj = JsonConvert.DeserializeObject(camposAdicionales);
            string destino = "";
            int index = 0;

            JArray jsonPreservar = JArray.Parse(camposAdicionales);
            foreach (JObject jsonOperaciones in jsonPreservar.Children<JObject>())
            {
                foreach (JProperty jsonOPropiedades in jsonOperaciones.Properties())
                {
                    string propiedad = jsonOPropiedades.Name;
                    if (propiedad.Equals("CodigoGrupo"))
                    {
                        if(jsonOPropiedades.Value.ToString() == "Destino")
                        {
                            destino= jsonObj[index]["Valor"].ToString();
                        }
                    }
                }
                index++;
            }

            if (destino != "")
            {
                double montoDivisor = myParametro.GetMontoDivisorFlete();
                double precioDestino = myUsu.GetPrecioDestino("PrecioFlete", codigoUso: destino);
                var moneda = new DS_Monedas().GetMoneda(Args.MonCodigo);
                double tasa = 1;
                if (moneda != null)
                {
                    tasa = moneda.MonTasa;
                }

                SqliteManager.GetInstance().Execute($"update ProductosTemp set PedFlete = Round((Round((((ProPeso * Cantidad) / {montoDivisor}) * {precioDestino}),2)) / {tasa},2) where ProDatos3 like '%H%' ", new string[] { });
            }

        }

    }
}
