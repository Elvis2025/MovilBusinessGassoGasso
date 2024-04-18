using Microsoft.AppCenter.Crashes;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using MovilBusiness.Views.Components.Modals;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class AuditoriaPreciosViewModel : BaseViewModel
    {
        public ICommand MenuItemCommand { get; private set; }
        public ICommand SearchCommand { get; private set; }


        private string searchvalue;
        public string SearchValue { get => searchvalue; set { searchvalue = value; RaiseOnPropertyChanged(); } }

        public List<CategoriasAuditoriasPrecios> Categorias { get; private set; }

        private CategoriasAuditoriasPrecios currentcategoria;
        public CategoriasAuditoriasPrecios CurrentCategoria { get => currentcategoria; set { currentcategoria = value; CargarMarcas(); RaiseOnPropertyChanged(); } }

        private ObservableCollection<MarcasAuditoriasPrecios> marcas;
        public ObservableCollection<MarcasAuditoriasPrecios> Marcas { get => marcas; private set { marcas = value; RaiseOnPropertyChanged(); } }

        private MarcasAuditoriasPrecios currentmarca;
        public MarcasAuditoriasPrecios CurrentMarca { get => currentmarca; set { currentmarca = value; SearchProducts(); RaiseOnPropertyChanged(); } }

        private ObservableCollection<ProductosTemp> productos;
        public ObservableCollection<ProductosTemp> Productos { get => productos; set { productos = value; RaiseOnPropertyChanged(); } }

        public int NumeroTransaccion { get; private set; }

        private int cantidaditems;
        public int CantidadItems { get => cantidaditems; set { cantidaditems = value; RaiseOnPropertyChanged(); } }

        private double total;
        public double Total { get => total; set { total = value; RaiseOnPropertyChanged(); } }

        public bool ShowPresencia { get; private set; }
        public bool ShowPrecioOferta { get; private set; }
        public bool ShowCaras { get; private set; }
        public bool ShowFacing { get; private set; }

        public bool ShowSecondFilter { get { return CurrentFilter != null && CurrentFilter.FilTipo == 2; } set { RaiseOnPropertyChanged(); } }

        private FiltrosDinamicos currentfilter;
        public FiltrosDinamicos CurrentFilter { get => currentfilter; set { currentfilter = value; OnFilterValueSelected(); RaiseOnPropertyChanged(); } }

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

        public bool FirtFilter { get => !DS_RepresentantesParametros.GetInstance().GetSecondFilterProduct(); }
        public bool SecondFilter { get => DS_RepresentantesParametros.GetInstance().GetSecondFilterProduct(); }
        public string BtnSearchLogo { get => CurrentFilter != null && CurrentFilter.FilTipo == 3 ? "ic_close_white" : CurrentFilter != null && CurrentFilter.FilTipo == 1 && CurrentFilter.IsCodigoBarra ? "ic_photo_camera_white_24dp" : "ic_search_white_24dp"; set { RaiseOnPropertyChanged(); } }
        public List<FiltrosDinamicos> FiltrosSource { get; private set; }

        private DS_AuditoriasPrecios myAud;
        private DS_Productos myProd;

        public string LblCantidadDescripcion { get; set; } = AppResource.QuantityLabel;

        public AuditoriaPreciosViewModel(Page page) : base(page)
        {
            BindFiltros();
            myAud = new DS_AuditoriasPrecios();
            myProd = new DS_Productos();
            NumeroTransaccion = DS_RepresentantesSecuencias.GetLastSecuencia("AuditoriasPrecios");

            Categorias = myAud.GetCategorias();

            MenuItemCommand = new Command(OnMenuItemClick);
            SearchCommand = new Command(() => { SearchProducts(); });

            ShowPresencia = myParametro.GetParAuditoriaPrecioCapturarPresencia();
            ShowPrecioOferta = myParametro.GetParAuditoriaPrecioCapturarPrecioOferta();
            ShowCaras = myParametro.GetParAuditoriaPrecioCapturarCaras();
            ShowFacing = myParametro.GetParAuditoriaPreciosCapturarFacing();

            var ParlabelCantidadDesc = myParametro.GetParLabelCantidadDescriptionAudPrecio();

            if (!string.IsNullOrWhiteSpace(ParlabelCantidadDesc))
            {
                if (!ParlabelCantidadDesc.Trim().EndsWith(":"))
                {
                    ParlabelCantidadDesc = ParlabelCantidadDesc.Trim() + ": ";
                }

                LblCantidadDescripcion = ParlabelCantidadDesc;
            }
        }

        public async void SearchProducts(bool resumen = false)
        {
            IsBusy = true;

            try
            {
                var catCodigo = CurrentCategoria != null ? CurrentCategoria.CatCodigo : null;

                var marCodigo = CurrentMarca != null ? CurrentMarca.MarCodigo : null;

                var args = new ProductosArgs
                {
                    valueToSearch = SearchValue,
                    filter = CurrentFilter,
                };

                if (CurrentSecondFiltro != null && CurrentSecondFiltro.Key != "-1")
                {
                    args.secondFilter = CurrentSecondFiltro.Key;
                }
                await Task.Run(() =>
                {
                    Productos = new ObservableCollection<ProductosTemp>(myAud.GetProductos(SearchValue, catCodigo, marCodigo, resumen, args));
                });


            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.ErrorLoadingProducts, e.Message);
            }
            finally
            {
                SearchValue = "";
                IsBusy = false;
            }
        }

        private ProductosTemp CurrentProducto;
        private void AgregarProducto(ProductosTemp product)
        {
            var item = product.Copy();

            myProd.InsertInTemp(item, isEntrega: false, isfromprecios:true);

            var index = Productos.IndexOf(product);

            if (index != -1)
            {
                Productos[index] = item;
            }

            ActualizarTotales();
        }

        public async void OnProductSelected(ProductosTemp item)
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            CurrentProducto = item;
            var dialogAgregarProducto = new AgregarProductosModal(myProd)
            {
                IsEntregandoTraspaso = false,
                OnCantidadAccepted = (s) =>
                {
                    try
                    {
                        //if (s.Cantidad <= 0)
                        //{
                        //    s.Precio = 0;
                        //}

                        CurrentProducto.Cantidad = s.Cantidad;
                        CurrentProducto.Precio = s.Precio;
                        CurrentProducto.IndicadorPresencia = s.Presencia;
                        CurrentProducto.PrecioOferta = s.PrecioOferta;
                        CurrentProducto.Caras = s.Caras;

                        AgregarProducto(CurrentProducto);
                    }catch(Exception e)
                    {
                        DisplayAlert(AppResource.Warning, e.Message);
                    }                    
                }
            };
            dialogAgregarProducto.CurrentProduct = item;
            dialogAgregarProducto.IsDppActive = false;
            dialogAgregarProducto.IsBusy = false;

            await PushAsync(dialogAgregarProducto);

            IsBusy = false;
        }    

        private void CargarMarcas()
        {
            try
            {
                if (CurrentCategoria == null)
                {
                    Marcas = new ObservableCollection<MarcasAuditoriasPrecios>();
                    return;
                }

                Marcas = new ObservableCollection<MarcasAuditoriasPrecios>(myAud.GetMarcasByCategoria(CurrentCategoria.CatCodigo));
            }catch(Exception e)
            {
                DisplayAlert(AppResource.ErrorLoadingBrands, e.Message);
            }
        }

        private void OnMenuItemClick(object Id)
        {
            switch (Id.ToString())
            {
                case "1":
                    SearchProducts(true);
                    break;
                case "2":
                    GuardarAuditoria();
                    break;
            }
        }

        private async void GuardarAuditoria()
        {
            try
            {

                if (IsBusy)
                {
                    return;
                }

                var productos = myAud.GetProductos(resumen: true);

                if (productos == null || productos.Count == 0)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.YouHaveNotAddAnyProductWarning);
                    return;
                }

                IsBusy = true;

                var task = new TaskLoader()
                {
                    SqlTransactionWhenRun = true
                };

                await task.Execute(() => 
                {
                    myAud.GuardarAuditoria(productos);
                });

                await DisplayAlert(AppResource.Success, AppResource.PriceAuditSavedSuccessfully);

                await PopAsync(false);

            }catch(Exception e)
            {
               await DisplayAlert(AppResource.ErrorSavingAudit, e.Message);
            }

            IsBusy = false;
        }

        private void ActualizarTotales()
        {
            try
            {
                var productos = myAud.GetProductos(resumen: true);

                Total = Math.Round(productos.Sum(x => x.Precio * x.Cantidad), 2, MidpointRounding.AwayFromZero);
                CantidadItems = productos.Count();
            }catch(Exception e)
            {
                DisplayAlert(AppResource.ErrorUpdatingTotals, e.Message);
            }
        }

        public void ClearTemp()
        {
            try
            {
                myProd.ClearTemp((int)Arguments.Values.CurrentModule);
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                Console.Write(e.Message);
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
                FiltrosSource = new DS_FiltrosDinamicos().GetFiltrosProductosAuditoriasPrecios();

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
                        new FiltrosDinamicos() { FilKey = "PRODAUDFILTRO", FilCampo = "p.ProDescripcion", FilTipo = 1, FilCondicion = "LIKE", FilDescripcion = AppResource.Description },
                        new FiltrosDinamicos() { FilKey = "PRODAUDFILTRO", FilCampo = "p.ProCodigo", FilTipo = 1, FilCondicion = "LIKE", FilDescripcion = AppResource.Code },
                    };

                    CurrentFilter = FiltrosSource[0];
                }
            }
        }
    }
}
