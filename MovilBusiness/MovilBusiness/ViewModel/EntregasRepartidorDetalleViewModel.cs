using Microsoft.AppCenter.Crashes;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
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
using ZXing.Mobile;

namespace MovilBusiness.ViewModel
{
    public class EntregasRepartidorDetalleViewModel : BaseViewModel 
    {

        private ObservableCollection<EntregasDetalleTemp> productos;
        public ObservableCollection<EntregasDetalleTemp> Productos { get => productos; set { productos = value; RaiseOnPropertyChanged(); } }

        public string BtnSearchLogo { get => CurrentFilter != null && CurrentFilter.FilTipo == 3 ? "ic_close_white" : CurrentFilter != null && CurrentFilter.FilTipo == 1 && CurrentFilter.IsCodigoBarra ? "ic_photo_camera_white_24dp" : "ic_search_white_24dp"; set { RaiseOnPropertyChanged(); } }

        public List<FiltrosDinamicos> FiltrosSource { get; private set; }
        private FiltrosDinamicos currentfilter;
        public FiltrosDinamicos CurrentFilter { get => currentfilter; set { currentfilter = value; OnFilterValueSelected(); RaiseOnPropertyChanged(); } }

        private string searchvalue;
        public string SearchValue { get { return searchvalue; } set { searchvalue = value; RaiseOnPropertyChanged(); } }

        private List<KV> secondfiltersource;
        public List<KV> SecondFiltroSource { get { return secondfiltersource; } set { secondfiltersource = value; RaiseOnPropertyChanged(); } }
        private KV currentsecondfiltro;
        public KV CurrentSecondFiltro
        {
            get { return currentsecondfiltro; }
            set
            {
                try
                {
                    currentsecondfiltro = value;

                    RaiseOnPropertyChanged();

                    if (value != null)
                    {
                        if (value.Value == AppResource.Select)
                        {
                            return;
                        }

                        EscanearProducto();
                    }

                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                }
            }
        }

        public bool ShowSecondFilter { get { return CurrentFilter != null && CurrentFilter.FilTipo == 2; } set { RaiseOnPropertyChanged(); } }

        public ICommand SearchCommand { get; private set; }
        public ICommand BtnSearchCommand { get; private set; }
        public ICommand ResumeCommand { get; private set; }

        private DS_EntregasRepartidorTransacciones myEnt;
        private DS_Inventarios myInv;

        public EntregasRepartidorTransacciones CurrentEntrega { get; private set; }
        public List<EntregasRepartidorTransacciones> Entregas { get; private set; }

        private Totales resumen;
        public Totales Resumen { get => resumen; set { resumen = value; RaiseOnPropertyChanged(); } }
        
        private int nextEntSecuencia;
        private bool ParNoEntregaParcialClientesContado;
        private bool IsClienteDeContado = false;

        public EntregasRepartidorDetalleViewModel(Page page, List<EntregasRepartidorTransacciones> entregas) : base(page)
        {
            Entregas = entregas;
            Init();

            bool first = true;

            foreach(var entrega in entregas)
            {
                myEnt.InsertProductInTemp(entrega.EnrSecuencia, entrega.TraSecuencia, entrega.TitID, entrega.CliID, deleteTemp: first);

                if (first)
                {
                    first = false;
                }
            }
        }

        public EntregasRepartidorDetalleViewModel(Page page, EntregasRepartidorTransacciones entrega) : base(page)
        {
            CurrentEntrega = entrega;
            Init();
            myEnt.InsertProductInTemp(entrega.EnrSecuencia, entrega.TraSecuencia, entrega.TitID, entrega.CliID);
        }

        private void Init()
        {
            
            myEnt = new DS_EntregasRepartidorTransacciones();
            myInv = new DS_Inventarios();

            ParNoEntregaParcialClientesContado = myParametro.GetParEntregasRepartidorNoParcialClientesDeContado();

            if (ParNoEntregaParcialClientesContado)
            {
                var condicionpago = new DS_CondicionesPago().GetByConId(Arguments.Values.CurrentClient.ConID);

                IsClienteDeContado = condicionpago != null && condicionpago.ConDiasVencimiento == 0;
            }

            switch (Arguments.Values.CurrentModule)
            {
                case Modules.CONDUCES:
                    nextEntSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Conduces");
                    break;
                case Modules.ENTREGASREPARTIDOR:
                    nextEntSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("EntregasTransacciones");
                    break;
            }

            SearchCommand = new Command(EscanearProducto);
            BtnSearchCommand = new Command(SearchOrClean);
            SaveCommand = new Command(() =>
            {
                GuardarEntrega();

            }, () => IsUp);

            ResumeCommand = new Command(() => { EscanearProducto(null, true); });

            Productos = new ObservableCollection<EntregasDetalleTemp>();

            BindFiltros();  
        }

        private void EscanearProducto() { EscanearProducto(null); }
        public async void EscanearProducto(EntregasDetalleTemp selectedProduct, bool resumen = false)
        {
            if (IsBusy)
            {
                return;
            }

            try
            {

                IsBusy = true;

                EntregasDetalleTemp item = null;

                if (selectedProduct == null)
                {
                    var args = new ProductosArgs
                    {
                        filter = CurrentFilter,
                        valueToSearch = SearchValue
                    };

                    if (CurrentSecondFiltro != null)
                    {
                        args.secondFilter = CurrentSecondFiltro.Key;
                    }

                    var prods = new List<EntregasDetalleTemp>();

                    await Task.Run(() => 
                    {
                        prods = myEnt.BuscarProductosInTemp(args, resumen);
                    });

                    Productos = new ObservableCollection<EntregasDetalleTemp>(prods);

                    if(prods != null && prods.Count == 1)
                    {
                        item = prods.FirstOrDefault();
                    }
                }
                else
                {
                    item = selectedProduct;
                }

                if(!myParametro.GetParEntregasRepartidorUsarRowDeProductosSinDialog() && !myParametro.GetParConducesUsarRowSinDialog())
                {
                    var isEditing = selectedProduct != null && (selectedProduct.Cantidad > 0 || selectedProduct.CantidadDetalle > 0) && ((item.UsaLote && !string.IsNullOrWhiteSpace(item.Lote)) || !item.UsaLote);

                    if (item != null)
                    {
                        await PushModalAsync(new AgregarProductoEntregaModal(item, AgregarProducto, Arguments.Values.CurrentModule == Modules.RECEPCIONDEVOLUCION, isEditing));
                    }
                }                

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        public void AgregarProducto(EntregasDetalleTemp producto, AgregarProductoEntregaModal dialog)
        {
            try
            {

                var parRowSinDialog = myParametro.GetParEntregasRepartidorUsarRowDeProductosSinDialog() || myParametro.GetParConducesUsarRowSinDialog();

                bool isEditing = dialog != null ? dialog.IsEditing : parRowSinDialog;

                //if (!myEnt.CantidadIsValida(producto.ProID, producto.Cantidad, producto.Posicion, producto.TraSecuencia, producto.CantidadSolicitada, producto.Lote, isEditing, producto.rowguid))
                //{
                //    if (myParametro.GetParNoEntregasParaciales())
                //        DisplayAlert(AppResource.Warning, AppResource.QuantityDifferentFromRequestedQuantity);
                //    else
                //        DisplayAlert(AppResource.Warning, AppResource.QuantityExceedRequestedQuantity);
                //    if (parRowSinDialog)
                //    {
                //        CargarProductos();
                //    }
                //    return;
                //}

                if (!myEnt.CantidadIsValida(producto.ProID, producto.Cantidad, producto.Posicion, producto.TraSecuencia, producto.CantidadSolicitada, producto.Lote, isEditing, producto.rowguid))
                {
                    DisplayAlert(AppResource.Warning, AppResource.QuantityExceedRequestedQuantity);
                    if (parRowSinDialog)
                    {
                        CargarProductos();
                    }
                    return;
                }

                var almId = Arguments.Values.CurrentModule == Modules.CONDUCES ? myParametro.GetParAlmacenIdParaDevolucion() : myParametro.GetParAlmacenIdParaDespacho();
                var parMultiAlmacenes = myParametro.GetParUsarMultiAlmacenes();

                if(almId == -1 && parMultiAlmacenes)
                {
                    DisplayAlert(AppResource.Warning, AppResource.WarehouseIdNotConfigured);
                    if (parRowSinDialog)
                    {
                        CargarProductos();
                    }
                    return;
                }
                
                if (Arguments.Values.CurrentModule != Modules.RECEPCIONDEVOLUCION && (producto.Cantidad > 0 || producto.CantidadDetalle > 0) && (parMultiAlmacenes || myParametro.GetParCargasInventario()) && !myInv.HayExistencia(producto.ProID, producto.Cantidad, (int)producto.CantidadDetalle, almId))
                {
                    DisplayAlert(AppResource.Warning, AppResource.QuantityGreaterThanStock);
                    if (parRowSinDialog)
                    {
                        CargarProductos();
                    }
                    return;
                }

                myEnt.AgregarProducto(producto, Arguments.Values.CurrentModule == Modules.RECEPCIONDEVOLUCION, isEditing, !myParametro.GetParConducesUsarRowSinDialog());

                SearchValue = "";

                CargarProductos();

                dialog?.Dismiss();

            }catch(Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message);
            }
        }

        public async void CargarProductos()
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;

                await Task.Run(() => 
                {
                    Productos = new ObservableCollection<EntregasDetalleTemp>(myEnt.GetProductosEntregaInTemp());
                });

                SetTotales();
            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        private void SetTotales()
        {
            var total = myEnt.GetTempTotales(false, -1, Arguments.Values.CurrentModule != Modules.CONDUCES || !myParametro.GetParConducesUsarRowSinDialog());

            total.NumeroTransaccion = nextEntSecuencia;

            Resumen = total;
        }

        private async void GuardarEntrega()
        {
            IsUp = false;
            bool IsMostrarMensajeOn = false;
            try
            {
                if (!myEnt.IsSomethingAdded())
                {
                    if(myParametro.GetParEntregasAgregarCero())
                    {
                        bool result = await DisplayAlert(AppResource.Warning, $"{(string.IsNullOrEmpty(myParametro.GetParEntregasMostrarMensaje()) ? "esta intentando continuar sin entregar o recibir Productos" : ""+ myParametro.GetParEntregasMostrarMensaje() + "")}", "Aceptar","Cancelar");

                        IsMostrarMensajeOn = true;

                        if (!result)
                        {
                            IsUp = true;
                            IsMostrarMensajeOn = false;
                            return;
                        }                    
                    
                    }else
                    {
                        var lotes = Productos.Where(x => x.UsaLote).FirstOrDefault();
                        IsUp = true;
                        await DisplayAlert(AppResource.Warning, AppResource.YouHaveNotAddAnyProductWarning + (lotes != null ? ", " + AppResource.IfProductsUseLotYouMustSpecifyIt : ""));
                        return;
                    }

                }

                var validarLote = true;

                if (Arguments.Values.CurrentModule == Modules.CONDUCES && myParametro.GetParConducesUsarRowSinDialog())
                {
                    validarLote = false;
                }

                var parValidarOfertas = myParametro.GetParEntregasRepartidorValidarOfertas() || myParametro.GetParEntregasOfertasTodoONada() || (ParNoEntregaParcialClientesContado && IsClienteDeContado);

                if(parValidarOfertas && myEnt.HayOfertasSinEntregarCompletamente(validarLote))
                {
                    if (ParNoEntregaParcialClientesContado && IsClienteDeContado)
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.CannotMakePartialDeliveriesToCashCustomers);
                        IsUp = true;
                        return;
                    }

                    IsUp = true;
                    await DisplayAlert(AppResource.Warning, AppResource.OfferProductNotFullyDelivered);
                    return;
                }

                List<EntregasDetalleTemp> NoEntregados = null;

                if(Entregas != null)
                {
                    NoEntregados = myEnt.GetProductosNoEntregadosForModal(Entregas);
                }
                else
                {
                    NoEntregados = myEnt.GetProductosNoEntregadosForModal(CurrentEntrega.EnrSecuencia, CurrentEntrega.TraSecuencia, CurrentEntrega.TitID);
                }

                if(NoEntregados.Count > 0 && Arguments.Values.CurrentModule != Modules.RECEPCIONDEVOLUCION && !IsMostrarMensajeOn)
                {
                    if (ParNoEntregaParcialClientesContado && IsClienteDeContado)
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.CannotMakePartialDeliveriesToCashCustomers);
                        IsUp = true;
                        return;
                    }

                    if(myParametro.GetParNoEntregasParaciales())
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.CannotMakePartialDeliveries);
                        IsUp = true;
                        return;
                    }
                   
                    var result = await DisplayAlert(AppResource.Warning, AppResource.SomeProductNotDeliveredWantInspectThem, AppResource.Inspect, AppResource.Continue);

                    if (result)
                    {
                        var page = new ProductosFaltantesEntregasModal(NoEntregados, myEnt)
                        {
                            OnAccepted = async () => { if (Entregas == null) { await PushAsync(new EntregasRepartidorDetalleRevisionPage(CurrentEntrega)); } else { await PushAsync(new EntregasRepartidorDetalleRevisionPage(Entregas)); } },
                            OnCancel = () => { IsUp = true; }

                        };
                        await PushModalAsync(page);
                        return;
                    }
                }


                if((Arguments.Values.CurrentModule == Modules.ENTREGASREPARTIDOR || myParametro.GetParEntregasOfertasTodoONada()) && parValidarOfertas && myEnt.SeQuitaraOferta(validarLote, Arguments.Values.CurrentModule == Modules.CONDUCES && myParametro.GetParConducesUsarRowSinDialog()))
                {
                    if (ParNoEntregaParcialClientesContado && IsClienteDeContado)
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.CannotMakePartialDeliveriesToCashCustomers);
                        IsUp = true;
                        return;
                    }

                    if (myParametro.GetParEntregasOfertasTodoONada())
                    {
                        IsUp = true;
                        await DisplayAlert(AppResource.Warning, AppResource.ProductsThatGiveOffersHasNotBeenDelivered);
                        return;
                    }
                    else
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.SomeProductsNotFullDeliveredOfferWillBeLost);
                    }
                }

                wasOnDetail = true;

                if (Entregas == null)
                {
                    await PushAsync(new EntregasRepartidorDetalleRevisionPage(CurrentEntrega));
                }
                else
                {
                    await PushAsync(new EntregasRepartidorDetalleRevisionPage(Entregas));
                }

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }
            IsUp = true;
        }

        public void VerificarSiHayEntregasParaOtroSector()
        {
            if(Arguments.Values.CurrentSector == null || !myParametro.GetParEntregasRepartidorEntrarEntregasAutomaticamenteOtrosSector())
            {
                return;
            }

            var secCodigo = myEnt.GetEntregasDisponiblesOtroSector(Arguments.Values.CurrentSector.SecCodigo);

            if (!string.IsNullOrWhiteSpace(secCodigo))
            {
                Arguments.Values.SecCodigoParaCrearVisita = secCodigo;
            }
        }

        private void OnFilterValueSelected()
        {
            if (CurrentFilter == null)
            {
                return;
            }

            try
            {
                if (CurrentFilter.FilTipo == 2)
                {
                    ShowSecondFilter = true;
                    CurrentSecondFiltro = null;

                    SecondFiltroSource = Functions.DinamicQuery(CurrentFilter.FilComboSelect, true);

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

                    BtnSearchLogo = "";
                }
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                DisplayAlert(AppResource.ErrorLoadingFilters, e.Message);
            }
        }

        private void BindFiltros()
        {
            try
            {
                FiltrosSource = new DS_FiltrosDinamicos().GetFiltrosProductos();

                if (FiltrosSource != null && FiltrosSource.Count > 0)
                {
                    var item = FiltrosSource.Where(x => x.FilIndicadorDefault).FirstOrDefault();

                    if (item == null)
                    {
                        item = FiltrosSource[0];
                    }

                    if (item != null)
                    {
                        CurrentFilter = item;
                    }
                }

            }
            catch (Exception e)
            {
                DisplayAlert(e.Message, AppResource.Aceptar);
                Crashes.TrackError(e);
                if (FiltrosSource == null)
                {
                    FiltrosSource = new List<FiltrosDinamicos>()
                    {
                        new FiltrosDinamicos() { FilKey = "PRODFILTRO", FilCampo = "p.ProDescripcion", FilTipo = 1, FilCondicion = "LIKE", FilDescripcion = AppResource.Description },
                        new FiltrosDinamicos() { FilKey = "PRODFILTRO", FilCampo = "p.ProCodigo", FilTipo = 1, FilCondicion = "LIKE", FilDescripcion = AppResource.Code },
                        new FiltrosDinamicos() { FilKey = "PRODFILTRO", FilCampo = "p.LinID", FilTipo = 2, FilDescripcion = AppResource.Line, FilComboSelect = "select LinID as Key, LinDescripcion as Value FROM Lineas Order by LinDescripcion", FilCondicion = "=" }
                    };

                    CurrentFilter = FiltrosSource[0];
                }
            }
        }

        private void SearchOrClean()
        {
            if (CurrentFilter != null && CurrentFilter.IsCodigoBarra)
            {
                GoScanQr();
                return;
            }

            if (CurrentFilter != null && CurrentFilter.FilTipo == 3)
            {
                SearchValue = "";
            }
            else
            {
                EscanearProducto();
            }
        }

        private bool isScanning;
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
                    EscanearProducto(null, false);
                }
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            isScanning = false;

        }

        private bool wasOnDetail = false;
        public void ConducesRestablecerCantidadSolicitada()
        {
            try
            {
                if (!wasOnDetail)
                {
                    return;
                }

                wasOnDetail = false;

                if (!myParametro.GetParConducesUsarRowSinDialog())
                {
                    return;
                }

                myEnt.ConducesRestablecerCantidadSolicitada();
            }catch(Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message);
            }
        }

    }
}
