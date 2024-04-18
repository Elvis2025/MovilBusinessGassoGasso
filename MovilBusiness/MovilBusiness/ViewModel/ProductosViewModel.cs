
using Microsoft.AppCenter.Crashes;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using MovilBusiness.Views.Components.Modals;
using Newtonsoft.Json;
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
    public class ProductosViewModel : BaseViewModel
    {
        public bool FirstTime { get; set; } = true;
        private DS_Productos myPro;
        private DS_Monedas myMon;

        public ICommand SearchCommand { get; private set; }

        public Clientes CurrentClient { get; set; }
        public bool UseClient { get => CurrentClient != null; }

        public List<UsosMultiples> ListasPrecios { get; private set; }

        private ObservableCollection<ProductosTemp> productos;
        public ObservableCollection<ProductosTemp> Productos { get { return productos; } set { productos = value; RaiseOnPropertyChanged(); } }

        public List<model.Internal.MenuItem> MenuSource { get; private set; }
        private model.Internal.MenuItem selecteditem;
        public model.Internal.MenuItem SelectedItem { get { return selecteditem; } set { selecteditem = value; RaiseOnPropertyChanged(); OnOptionItemSelected(); } }

        private UsosMultiples currentlistaprecios;
        public UsosMultiples CurrentListaPrecios { get => currentlistaprecios; set { currentlistaprecios = value; if(!string.IsNullOrEmpty(SearchValue)) { Search(); } RaiseOnPropertyChanged(); } }

        public bool UseMultiMoneda { get; set; }

        public List<FiltrosDinamicos> FiltrosSource { get; private set; }

        public string BtnSearchLogo { get => CurrentFilters != null && CurrentFilters.FilTipo == 3 ? "ic_close_white" : CurrentFilters != null && CurrentFilters.FilTipo == 1 && CurrentFilters.IsCodigoBarra ? "ic_photo_camera_white_24dp" : "ic_search_white_24dp"; set { RaiseOnPropertyChanged(); } }

        //private FiltrosDinamicos currentfilter;
        //public FiltrosDinamicos CurrentFilter { get => currentfilter; set { currentfilter = value; OnFilterValueSelected(); RaiseOnPropertyChanged(); } }

        private FiltrosDinamicos currentfilter;
        public FiltrosDinamicos CurrentFilters { get => currentfilter; set { currentfilter = value; OnFilterValueSelected(); RaiseOnPropertyChanged(); } }

        private List<KV> secondfiltersource;
        public List<KV> SecondFiltroSource { get { return secondfiltersource; } set { secondfiltersource = value; RaiseOnPropertyChanged(); } }
        private KV currentsecondfiltro;
        public KV CurrentSecondFiltro { get { return currentsecondfiltro; } set { currentsecondfiltro = value; if (CurrentFilters.FilTipo == 2) { Search(); } RaiseOnPropertyChanged(); } }

        public bool ShowSecondFilter { get { return CurrentFilters != null && CurrentFilters.FilTipo == 2; } set { RaiseOnPropertyChanged(); } }

        public bool ShowListaPrecios { get => CurrentClient == null; set { RaiseOnPropertyChanged(); } }

        private string searchvalue;
        public string SearchValue { get { return searchvalue; } set { searchvalue = value; RaiseOnPropertyChanged(); } }

        public int CantidadProductos { get => Productos != null ? Productos.Count : 0; set { RaiseOnPropertyChanged(); } }

        public event EventHandler<int> OnRowDesignChanged;
        public Action OnOptionMenuItemSelected { get; set; }

        private bool IsModalMode = false;

        public List<Monedas> MonedasSource { get; set; }

        private Monedas currentmoneda = null;
        public Monedas CurrentMoneda { get => currentmoneda; set { currentmoneda = value; if (value != null) { if (!string.IsNullOrEmpty(SearchValue)) { Search(); } } RaiseOnPropertyChanged(); } }
        public ICommand BtnSearchCommand { get; private set; }

        public ProductosViewModel(Page page, Clientes currentClient = null, bool isModalMode = false) : base(page)
        {
            IsModalMode = isModalMode;
            myPro = new DS_Productos();
            myMon = new DS_Monedas();
            CurrentClient = currentClient;
            SearchCommand = new Command(Search);            
            ListasPrecios = new DS_UsosMultiples().GetAllListaPrecios();
            BtnSearchCommand = new Command(SearchOrClean);

            UseMultiMoneda = myParametro.GetParPedidoMultiMoneda();
            var useset = myParametro.GetParMultimonedasSet();
            BindFiltros();

            if (UseMultiMoneda)
            {
                MonedasSource = myMon.GetMonedasSelect();
            }


            if (CurrentClient == null)
            {
                if (ListasPrecios != null && ListasPrecios.Count > 0)
                {
                    CurrentListaPrecios = ListasPrecios[0];
                }

                if (UseMultiMoneda && string.IsNullOrEmpty(useset))
                {
                    CurrentMoneda = MonedasSource.FirstOrDefault();
                }
                else if(!string.IsNullOrEmpty(useset))
                {
                    CurrentMoneda = MonedasSource.Where(p => p.MonCodigo == useset).FirstOrDefault();
                }
               
            }
            else if(UseMultiMoneda)
            {
                if(string.IsNullOrEmpty(useset))
                {
                    CurrentMoneda = MonedasSource.FirstOrDefault(c => c.MonCodigo == CurrentClient.MonCodigo);
                }
                else
                {
                    CurrentMoneda = MonedasSource.Where(p => p.MonCodigo == useset).FirstOrDefault();
                }
            }

            BindMenu();            
        }

        //private async void SearchUnAsync(bool resumen) { await Search(resumen); }
        //public Task Search() { return Search(false); }

        private void BindMenu()
        {
            MenuSource = new List<model.Internal.MenuItem>
            {
                new model.Internal.MenuItem() { Title = AppResource.GetOut, Id = 0, Icon = "ic_arrow_back_black_24dp"}
            };

            if (myParametro.GetParCambiarVisualizacionProductos())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.ChangeVisualization, Id = 1, Icon = "ic_view_quilt_black_24dp" });
            }
        }

        private void SearchOrClean()
        {
            if (CurrentFilters != null && CurrentFilters.IsCodigoBarra)
            {
                GoScanQr();
                return;
            }

            if (CurrentFilters != null && CurrentFilters.FilTipo == 3)
            {
                SearchValue = "";
            }
            else
            {
                Search();
            }
        }

        private async void Search()
        {
            if (IsBusy)
            {
                return;
            }

            if(UseMultiMoneda && CurrentMoneda == null)
            {
                return;
            }

            if(CurrentListaPrecios == null)
            {
                return;
            }

            IsBusy = true;

            try
            {
                var filtrarSector = CurrentClient != null && myParametro.GetParSectores() > 0 && myParametro.GetParFiltrarProductosPorSector();

                var args = new ProductosArgs
                {
                    valueToSearch = SearchValue,
                    lipCodigo = ShowListaPrecios ? CurrentListaPrecios != null ? CurrentListaPrecios.CodigoUso : "Default" : myParametro.GetParSectores() >= 2 && Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.LipCodigo : CurrentClient.LiPCodigo,
                    filter = CurrentFilters,
                    NotUseTemp = true,
                    FiltrarProductosPorSector = filtrarSector,
                    MonCodigo = UseMultiMoneda && CurrentMoneda != null ? CurrentMoneda.MonCodigo : CurrentClient != null ?  CurrentClient.MonCodigo : "",
                };

                if (CurrentSecondFiltro != null)
                {
                    args.secondFilter = CurrentSecondFiltro.Key;
                }
            
                await Task.Run(() =>
                {
                    Productos = new ObservableCollection<ProductosTemp>(myPro.GetProductos(args));
                });

                var json = JsonConvert.SerializeObject(args);

                CantidadProductos = Productos != null ? Productos.Count : 0;
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                await DisplayAlert(AppResource.ErrorLoadingProducts, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
        }

        private void BindFiltros()
        {
            try
            {
                FiltrosSource = new DS_FiltrosDinamicos().GetFiltrosProductos();

                if (FiltrosSource != null && FiltrosSource.Count > 0)
                {
                    var item = FiltrosSource.Where(x => x.FilIndicadorDefault).FirstOrDefault();

                    if (item != null)
                    {
                        CurrentFilters = item;
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
                }
            }
        }

        private void OnFilterValueSelected()
        {
            if (CurrentFilters.FilTipo == 2)
            {
                ShowSecondFilter = true;
                SecondFiltroSource = Functions.DinamicQuery(CurrentFilters.FilComboSelect);

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

        private async void ShowAlertChangeVisualizacion()
        {
            var Id = await DisplayActionSheet(AppResource.ChooseVisualizationForm, buttons: 
                new string[] { AppResource.ThreeColumns, AppResource.TwoColumns, AppResource.OneColumn, AppResource.Lines, AppResource.Catalog });

            if(Id == AppResource.ThreeColumns)
            {
                OnRowDesignChanged?.Invoke(this, 15);
            }else if(Id == AppResource.TwoColumns)
            {
                OnRowDesignChanged?.Invoke(this, 16);
            }else if(Id == AppResource.OneColumn)
            {
                OnRowDesignChanged?.Invoke(this, 17);
            }else if(Id == AppResource.Lines)
            {
                OnRowDesignChanged?.Invoke(this, myParametro.GetFormatoVisualizacionProductos());
            }else if(Id == AppResource.Catalog)
            {
                OnRowDesignChanged?.Invoke(this, 20);
            }
        }

        public void OnProductSelected(ProductosTemp item)
        {
            PushModalAsync(new DetalleProductosModal(item, ListasPrecios, CurrentListaPrecios, !UseClient));
        }

        public void SaveVisualizacion(int Id)
        {
            myParametro.SaveFormatoVisualizacionProductos(Id);
        }

        private void OnOptionItemSelected()
        {
            try
            {
                if (SelectedItem == null) return;

                switch (SelectedItem.Id)
                {
                    case 0: //salir
                        if (IsModalMode)
                        {
                            PopModalAsync(false);
                        }
                        else
                        {
                            PopAsync(true);
                        }
                        
                        break;
                    case 1: //cambiar visualizacion
                        ShowAlertChangeVisualizacion();
                        break;
                }
            } catch (Exception e)
            {
                Crashes.TrackError(e);
                DisplayAlert(AppResource.Warning, e.Message);
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

        private void SearchUnAsync(bool resumen) { Search(); }

        public async void GuardarProductosValidosParaDescuento()
        {
            if (!myParametro.GetParDescuentosProductosMostrarPreview() || myParametro.GetParPedidosDescuentoManualGeneral() > 0.0)
            {
                GuardarProductosValidosParaOfertas();
                return;
            }

            IsBusy = true;

            try
            {
                await Task.Run(() => 
                {
                    if (myParametro.GetParDescuentosProductosMostrarPreview())
                    {
                        new DS_DescuentosRecargos().GuardarProductosValidosParaDescuentoPreview(-1);
                    }
                    else
                    {
                        new DS_DescuentosRecargos().GuardarProductosValidosParaDescuento(-1);
                    }
                });
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            IsBusy = false;

            GuardarProductosValidosParaOfertas();
        }

        public async void GuardarProductosValidosParaOfertas()
        {
            if(CurrentClient != null)
            {
                return;
            }

            IsBusy = true;

            try
            {

                await Task.Run(() => 
                {
                    new DS_Ofertas().GuardarProductosValidosParaOfertas(-1, -1);
                });

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }
    }
}
