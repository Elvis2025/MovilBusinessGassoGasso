using Microsoft.AppCenter.Crashes;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.Model.Internal;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using MovilBusiness.Views.Components.Modals;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using ZXing.Mobile;

namespace MovilBusiness.ViewModel
{
    public class CatalogoProductosViewModel : BaseViewModel
    {
        public ICommand ChangeOrderCommand { get; private set; }
        public string OrderIcon { get => order == 1 ? "baseline_arrow_upward_black_24" : "baseline_arrow_downward_black_24"; }       

        private string imagendetalle;
        public string ImagenDetalle { get => imagendetalle; set { imagendetalle = value; RaiseOnPropertyChanged(); } }

        //0 asc, 1 desc
        private int order = 0;

        private int pickerorderselectedindex = 0;
        public int pickerOrderSelectedIndex { get => pickerorderselectedindex; set { pickerorderselectedindex = value; reloadList(); RaiseOnPropertyChanged(); } }

        private bool isvisibleorden;
        public bool IsVisibleOrden { get => isvisibleorden; set { isvisibleorden = value; RaiseOnPropertyChanged(); } }

        private ObservableCollection<ProductosTemp> productos;
        public ObservableCollection<ProductosTemp> Productos { get => productos; set { productos = value; RaiseOnPropertyChanged(); } }

        private DS_Productos myPro;

        private bool _isfromhome;

        public bool FirtFilter { get => !DS_RepresentantesParametros.GetInstance().GetSecondFilterProduct(); }
        public bool SecondFilter { get => DS_RepresentantesParametros.GetInstance().GetSecondFilterProduct(); }

        public bool ShowSecondFilter { get { return CurrentFilter != null && CurrentFilter.FilTipo == 2; } set { RaiseOnPropertyChanged(); } }

        public string BtnSearchLogo { get => CurrentFilter != null && CurrentFilter.FilTipo == 3 ? "ic_close_white" : CurrentFilter != null && CurrentFilter.FilTipo == 1 && CurrentFilter.IsCodigoBarra ? "ic_photo_camera_white_24dp" : "ic_search_white_24dp"; set { RaiseOnPropertyChanged(); } }


        private FiltrosDinamicos currentfilter;
        public FiltrosDinamicos CurrentFilter { get => currentfilter; set { currentfilter = value; OnFilterValueSelected(); RaiseOnPropertyChanged(); } }
        
        public List<FiltrosDinamicos> FiltrosSource { get; private set; }

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
                        if (value.Value == "(Seleccione)" || value.Value == AppResource.Select)
                        {
                            return;
                        }

                       

                        SearchProducts();
                    }

                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                }
            }
        }

        public string LastValueSearch;

        private string searchvalue;
        public string SearchValue { get { return searchvalue; } set { searchvalue = value; if (DS_RepresentantesParametros.GetInstance().GetBuscarProductosAlEscribir()) { SearchOrClean(LastValueSearch); } RaiseOnPropertyChanged(); } }


        public ICommand BtnSearchCommand { get; private set; }

        public CatalogoProductosViewModel(Page page, bool isfrohome = false) : base(page)
        {
            _isfromhome = isfrohome;

            BtnSearchCommand = new Command(() => { SearchOrClean(); });

            IsVisibleOrden = true;
            ChangeOrderCommand = new Command(ChangeOrder);
            myPro = new DS_Productos();

            //Arguments.Values.CurrentModule = Modules.PRODUCTOS;

            BindFiltros();
        }

        private void SearchOrClean(string value = "")
        {
            LastValueSearch = SearchValue;
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
                SearchProducts();
            }
        }


        public void reloadList()
        {
            IOrderedEnumerable<ProductosTemp> res = null;

            if(Productos == null || Productos.Count == 0)
            {
                return;
            }

            if (pickerOrderSelectedIndex == 0)
            {

                if (order == 0)
                {
                    res = from str in Productos orderby str.Descripcion ascending select str;
                }
                else
                {
                    res = from str in Productos orderby str.Descripcion descending select str;
                }

            }
            else
            {
                if (order == 0)
                {
                    res = from str in Productos orderby str.ProCodigo ascending select str;
                }
                else
                {
                    res = from str in Productos orderby str.ProCodigo descending select str;
                }
            }

            if (res != null)
            {
                Productos = null;
                Task.Delay(100);
                Productos = new ObservableCollection<ProductosTemp>(res);
            }
        }

        private void BindFiltros()
        {
            try
            {
                FiltrosSource = new DS_FiltrosDinamicos().GetFiltrosProductos();

                if (FiltrosSource != null && FiltrosSource.Count > 0)
                {
                    FiltrosDinamicos item = null;
                    if (myParametro.GetParBusquedaCombinadaPorDefault())
                        item = FiltrosSource.Where(x => x.FilDescripcion == "Combinada").FirstOrDefault();
                    else
                        item = FiltrosSource.Where(x => x.FilIndicadorDefault).FirstOrDefault();

                    if (item == null)
                    {
                        item = FiltrosSource.FirstOrDefault();
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

        private void ChangeOrder()
        {
            //order = order == 1 ? 2 : 1;

            if(order == 0)
            {
                order = 1;
            }
            else
            {
                order = 0;
            }

            RaiseOnPropertyChanged(nameof(OrderIcon));

            //SearchProducts();


            /* var ascendingOrder = Productos.OrderBy(i => i);
             var descendingOrder = Productos.OrderByDescending(i => i);*/

            reloadList();
           
        }

        public async void SearchProducts()
        {
            string queryOrder = "";

            if(pickerOrderSelectedIndex == 0)
            {
                queryOrder = "p.ProDescripcion";
            }
            else
            {
                queryOrder = "p.ProCodigo";
            }

            if(order == 0)
            {
                queryOrder += " asc ";
            }
            else
            {
                queryOrder += " desc ";
            }

            IsBusy = true;

            try
            {

                await Task.Run(() =>
                {
                    var filtrarSector = Arguments.Values.CurrentClient != null && myParametro.GetParSectores() > 0 && myParametro.GetParFiltrarProductosPorSector();

                    var args = new ProductosArgs
                    {
                        valueToSearch = SearchValue,
                        lipCodigo = !_isfromhome ? myParametro.GetParSectores() >= 2 && Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.LipCodigo : Arguments.Values.CurrentClient != null && !string.IsNullOrEmpty(Arguments.Values.CurrentClient.LiPCodigo) ? Arguments.Values.CurrentClient.LiPCodigo : "*P.ProPrecio*" : "*P.ProPrecio*",
                        filter = CurrentFilter,
                        NotUseTemp = true,
                        FiltrarProductosPorSector = filtrarSector,
                        precioMayorQueCero = !_isfromhome? Arguments.Values.CurrentClient != null : false,
                        MonCodigo = !_isfromhome ? Arguments.Values.CurrentClient.MonCodigo : null
                    };

                    if(CurrentSecondFiltro != null)
                    {
                        args.secondFilter = CurrentSecondFiltro.Key;
                    }

                    args.orderBy = queryOrder;

                    var products = new ObservableCollection<ProductosTemp>(myPro.GetProductos(args));

                    if (myParametro.GetParCatalogoProductoSoloConImagen())
                    {
                        Productos = new ObservableCollection<ProductosTemp>(products.Where(x => !string.IsNullOrWhiteSpace(x.ProImage) && x.ProImage != "-1").ToList());
                    }
                    else
                    {
                        Productos = products;
                    }

                });
            }catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
            
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
                        SearchProducts();
                    }
                }
                catch (Exception e)
                {
                    Crashes.TrackError(e);
                    await DisplayAlert(AppResource.Warning, e.Message);
                }

                isScanning = false;

            }

        public void OnProductSelected(ProductosTemp item)
        {
            var lipCodigo = !_isfromhome ? myParametro.GetParSectores() >= 2 && Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.LipCodigo : Arguments.Values.CurrentClient != null && !string.IsNullOrEmpty(Arguments.Values.CurrentClient.LiPCodigo) ? Arguments.Values.CurrentClient.LiPCodigo : "*P.ProPrecio*" : "*P.ProPrecio*";
            var ListasPrecios = new DS_UsosMultiples().GetAllListaPrecios();
            var useClient = Arguments.Values.CurrentClient != null;

            var currentListaPrecios = ListasPrecios.FirstOrDefault(x => x.CodigoUso == lipCodigo);

            if(currentListaPrecios == null)
            {
                currentListaPrecios = ListasPrecios.FirstOrDefault();
            }

            PushModalAsync(new DetalleProductosModal(item, ListasPrecios, currentListaPrecios, !useClient));
        }
    }
    }
