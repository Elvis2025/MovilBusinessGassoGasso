using Microcharts;
using Microsoft.AppCenter.Crashes;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.Resx;
using MovilBusiness.viewmodel;
using MovilBusiness.Views.Components.Modals;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class PresupuestosViewModel : BaseViewModel
    {
        private DS_Presupuestos myPresu;
        private DS_QuerysDinamicos myQue;
        private DS_UsosMultiples myUso;

        public ICommand SelectAnyoCommand { get; private set; }
        public ICommand SelectMesCommand { get; private set; }
        public ICommand ChangeVisualizationCommand { get; private set; }
        public ICommand FiltrarLogros { get; set; }

        private Representantes currentrepresentante = Arguments.CurrentUser;
        public Representantes CurrentRepresentante { get => currentrepresentante; set { if (value != currentrepresentante) { currentrepresentante = value; CargarTiposPresupuestos(); } else { currentrepresentante = value; } RaiseOnPropertyChanged(); } }

        private List<Representantes> representantes;
        public List<Representantes> Representantes { get => representantes; set { representantes = value; RaiseOnPropertyChanged(); } }

        private ChartTypes CurrentChartType = ChartTypes.LINEAR;

        private ObservableCollection<UsosMultiples> tipospresupuestos;
        public ObservableCollection<UsosMultiples> TiposPresupuestos { get => tipospresupuestos; private set { tipospresupuestos = value; RaiseOnPropertyChanged(); } }

        private UsosMultiples currenttipopresupuesto;
        public UsosMultiples CurrentTipoPresupuesto { get => currenttipopresupuesto; set { currenttipopresupuesto = value; TipoPresupuestoChanged(); RaiseOnPropertyChanged(); } }

        private bool isonline;
        public bool IsOnline { get => isonline; set { isonline = value; TipoPresupuestoChanged(); RaiseOnPropertyChanged(); } }

        private bool showtableview = true;
        public bool ShowTableView { get => showtableview; set { showtableview = value; RaiseOnPropertyChanged(); } }

        private bool emptylabelvisible;
        public bool EmptyLabelVisible { get => emptylabelvisible; set { emptylabelvisible = value; RaiseOnPropertyChanged(); } }

        private List<KV> anyos;
        public List<KV> Anyos { get => anyos; set { anyos = value; RaiseOnPropertyChanged(); } }

        private List<UsosMultiples> meses;
        public List<UsosMultiples> Meses { get => meses; set { meses = value; RaiseOnPropertyChanged(); } }

        private KV currentanyo;
        public KV CurrentAnyo { get => currentanyo; set { currentanyo = value; CargarMeses(); RaiseOnPropertyChanged(); } }

        private UsosMultiples currentmes;
        public UsosMultiples CurrentMes { get => currentmes; set { currentmes = value; CargarPresupuestos(); RaiseOnPropertyChanged(); } }

        private ObservableCollection<Presupuestos> presupuestos;
        public ObservableCollection<Presupuestos> Presupuestos { get => presupuestos; set { presupuestos = value; RaiseOnPropertyChanged(); } }

        private ObservableCollection<Microcharts.Entry> presupuestoscharts;
        public ObservableCollection<Microcharts.Entry> PresupuestosCharts { get => presupuestoscharts; set { presupuestoscharts = value; RaiseOnPropertyChanged(); } }

        private Chart presuchart;
        public Chart PresuChart { get => presuchart; set { presuchart = value; RaiseOnPropertyChanged(); } }

        private string menorValue { get; set; } = "";
        public string MenorValue { get=> menorValue; set { menorValue = value; } }

        private string mayorValue { get; set; } = "";
        public string MayorValue { get=> mayorValue; set { mayorValue = value; } }

        private readonly int CurrentCliId;

        private readonly bool fromPedidos;

        private ProductosTemp currentproducto;
        public ProductosTemp CurrentProducto { get => currentproducto; set { currentproducto = value; RaiseOnPropertyChanged(); } }

        private Monedas currentmoneda = null;
        public Monedas CurrentMoneda { get => currentmoneda; set { currentmoneda = value; RaiseOnPropertyChanged(); } }

        private AgregarProductosModal dialogAgregarProducto;
        private DS_Productos myProd;

        private bool isvisibleadd;
        public bool isVisibleAdd { get => isvisibleadd; set { isvisibleadd = value; RaiseOnPropertyChanged(); } }
        public PresupuestosViewModel(Page page, int cliId = -1, bool isFromPedidos = false, Monedas monedas = null) : base(page)
        {
            CurrentCliId = cliId;
            fromPedidos = isFromPedidos;
            CurrentMoneda = monedas;
            myProd = new DS_Productos();
            myQue = new DS_QuerysDinamicos();
            myPresu = new DS_Presupuestos();
            myUso = new DS_UsosMultiples();
            
            SelectAnyoCommand = new Command(ShowAlertSelectAnyo);
            SelectMesCommand = new Command(ShowAlertSelectMes);
            ChangeVisualizationCommand = new Command(AttempChangeVisualization);
            FiltrarLogros = new Command(SetLogros);
            IsOnline = myParametro.GetParPresupuestosOnlineDefault();

            if (Arguments.CurrentUser.RepIndicadorSupervisor)
            {
                Representantes = new DS_Representantes().GetAllRepresentantes();
            }

            CurrentRepresentante = Arguments.CurrentUser;

            CargarTiposPresupuestos();

        }

        private async void ShowAlertSelectMes()
        {
            if(Meses == null || Meses.Count == 0)
            {
                return;
            }

            List<string> raw = new List<string>();

            foreach(var mes in Meses)
            {
                raw.Add(mes.Descripcion);
            }

            var result = await DisplayActionSheet(AppResource.SelectMonth, buttons: raw.ToArray());

            var item = Meses.Where(x => x.Descripcion == result).FirstOrDefault();

            if(item == null)
            {
                return;
            }

            CurrentMes = item;

        }

        private async void ShowAlertSelectAnyo()
        {
            if (Anyos == null || Anyos.Count == 0)
            {
                return;
            }


            List<string> raw = new List<string>();

            foreach (var anyo in Anyos)
            {
                raw.Add(anyo.Value);
            }

            var result = await DisplayActionSheet(AppResource.SelectYear, buttons: raw.ToArray());

            var item = Anyos.Where(x => x.Value == result).FirstOrDefault();

            if (item == null)
            {
                return;
            }

            CurrentAnyo = item;
        }

        private async void AttempChangeVisualization()
        {
            var items = new string[] { AppResource.Table, AppResource.BarChart, AppResource.PointChart,
                AppResource.LineChart, AppResource.DonutChart, AppResource.RadialGaugeChart, AppResource.RadarChart };

            var result = await DisplayActionSheet(AppResource.SelectDisplayForm, buttons: items);


            if(result == AppResource.Table)
            {
                CurrentChartType = ChartTypes.LINEAR;
            }else if(result == AppResource.BarChart)
            {
                CurrentChartType = ChartTypes.BARRAS;
            }else if(result == AppResource.PointChart)
            {
                CurrentChartType = ChartTypes.PUNTOS;
            }else if(result == AppResource.LineChart)
            {
                CurrentChartType = ChartTypes.GRAFICOLINEAS;
            }else if(result == AppResource.DonutChart)
            {
                CurrentChartType = ChartTypes.DONUT;
            }else if(result == AppResource.RadialGaugeChart)
            {
                CurrentChartType = ChartTypes.RADIAL;
            }else if(result == AppResource.RadarChart)
            {
                CurrentChartType = ChartTypes.RADAR;
            }
            else
            {
                return;
            }

            VisualizationChanged();

        }

        private void LoadPresupuestosForCharts()
        {
            PresupuestosCharts = new ObservableCollection<Microcharts.Entry>();

            if (Presupuestos == null || Presupuestos.Count == 0)
            {
                return;
            }

            if((Presupuestos.Count > 10 && Device.Idiom == TargetIdiom.Tablet) || (Presupuestos.Count > 6 && Device.Idiom == TargetIdiom.Phone))
            {
                EmptyLabelVisible = true;
                return;
            }

            foreach (var presu in Presupuestos)
            {
                if (presu.PreTipo == "Total" && presu.Descripcion == "Total")
                {
                    continue;
                }

                var color = RandomColor();

                double presupuesto = 100 - (presu.Cumplimiento > 0 ? presu.Cumplimiento : 0);

                PresupuestosCharts.Add(new Microcharts.Entry((float)presupuesto) { Color = color, Label = AppResource.Budget, ValueLabel = presu.Descripcion.Trim() + " " + presupuesto.ToString("N2") + "%" });

                if (Presupuestos.Count == 2 || Presupuestos.Count == 1)
                {
                    color = RandomColor();
                }

                PresupuestosCharts.Add(new Microcharts.Entry((float)presu.Cumplimiento) { Color = color, Label = AppResource.Executed, ValueLabel = presu.Descripcion.Trim() + " " + presu.CumplimientoString });
            }
        }

        private async void CargarTiposPresupuestos()
        {
            TiposPresupuestos = new ObservableCollection<UsosMultiples>(await myUso.GetTiposPresupuestos(CurrentRepresentante.RepCodigo, CurrentRepresentante.RepClave, CurrentCliId,isonline));
        }

        private SKColor[] colors = { SKColor.Parse("#266489"), SKColor.Parse("#ff80ff"), SKColor.Parse("#B7A8F4"), SKColor.Parse("#68B9C0"), SKColor.Parse("#90D585") };
        private Random random = new Random();
        private SKColor RandomColor() {
            return colors[random.Next(colors.Length)];
        }

        private async void TipoPresupuestoChanged()
        {
            if(CurrentTipoPresupuesto == null)
            {
                Anyos = null;
                return;
            }

            try
            {
                IsBusy = true;

                //await Task.Run(() => {  });
                Anyos = await myPresu.GetPreAnyoByPreTipo(CurrentTipoPresupuesto.CodigoUso, CurrentRepresentante.RepCodigo, CurrentRepresentante.RepClave, IsOnline);

                if (Anyos != null && Anyos.Count > 0)
                {
                    CurrentAnyo = Anyos[Anyos.Count - 1];
                }

            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;

        }

        private async void CargarMeses()
        {
            if(CurrentTipoPresupuesto == null || CurrentAnyo == null)
            {
                Meses = null;
                return;
            }

            IsBusy = true;

            try
            {
                Meses = await myPresu.GetPreMesByPreTipo(CurrentTipoPresupuesto.CodigoUso, CurrentAnyo.Value, CurrentRepresentante.RepCodigo, CurrentRepresentante.RepClave, IsOnline,myParametro.GetParMesesByUsomultiples().ToUpper());
                //await Task.Run(() => {  });

                if (Meses != null && Meses.Count > 0)
                {
                    var item = Meses.Where(x => x.CodigoUso == DateTime.Now.Month.ToString()).FirstOrDefault();

                    if (item != null)
                    {

                        CurrentMes = item;
                    }
                    else
                    {
                        CurrentMes = Meses[0];
                    }
                }

            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;

        }

        private void VisualizationChanged()
        {
            if(Presupuestos == null)
            {
                return;
            }

            EmptyLabelVisible = false;

            if (CurrentChartType != ChartTypes.LINEAR)
            {
                LoadPresupuestosForCharts();
            }

            ShowTableView = false;

            switch (CurrentChartType)
            {
                case ChartTypes.GRAFICOLINEAS:
                    PresuChart = new LineChart() { Entries = PresupuestosCharts, MaxValue = 100 };
                    break;
                case ChartTypes.BARRAS:
                    PresuChart = new BarChart() { Entries = PresupuestosCharts, MaxValue = 100 };
                    break;
                case ChartTypes.DONUT:
                    PresuChart = new DonutChart() { Entries = PresupuestosCharts, MaxValue = 100 };
                    break;
                case ChartTypes.PUNTOS:
                    PresuChart = new PointChart() { Entries = PresupuestosCharts, MaxValue = 100 };
                    break;
                case ChartTypes.RADAR:
                    PresuChart = new RadarChart() { Entries = PresupuestosCharts, MaxValue = 100 };
                    break;
                case ChartTypes.RADIAL:
                    PresuChart = new RadialGaugeChart() { Entries = PresupuestosCharts, MaxValue = 100 };
                    break;
                case ChartTypes.LINEAR:
                    ShowTableView = true;
                    break;
            }
        }

        public async void CargarPresupuestos()
        {


            if(CurrentTipoPresupuesto == null || CurrentAnyo == null || CurrentMes == null)
            {
                Presupuestos = null;
                return;
            }



            if(PresuChart == null)
            {
                VisualizationChanged();
            }

            try
            {
                EmptyLabelVisible = false;
                IsBusy = true;

                var query = "";

                if (!IsOnline)
                {
                    query = myQue.GetQuerysPresupuestos(CurrentTipoPresupuesto.CodigoUso, CurrentRepresentante.RepCodigo, IsOnline, myPresu.api);

                    if (string.IsNullOrEmpty(query))
                    {
                        Presupuestos = new ObservableCollection<Presupuestos>();
                        return;
                    }
                }

                Presupuestos = new ObservableCollection<Presupuestos>(await myPresu.GetPresupuestosByQuery(query, CurrentTipoPresupuesto.CodigoUso, CurrentAnyo.Value, CurrentMes.CodigoUso, CurrentRepresentante.RepCodigo, CurrentRepresentante.RepClave, IsOnline, CurrentCliId, MayorValue.ToString(), MenorValue.ToString()));

                AddTotalesForTable();

                if (DS_RepresentantesParametros.GetInstance().GetParPresupuestosAgregaPedidos())
                {
                    foreach (var presu in Presupuestos)
                    {
                        presu.isVisibleAdd = CurrentTipoPresupuesto.CodigoUso.Contains("PROD") && presu.Descripcion != "Total" && fromPedidos && presu.ProID > 0 ? true : false;
                    }
                }
                
                if (CurrentChartType != ChartTypes.LINEAR)
                {
                    VisualizationChanged();
                }

            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.ErrorLoadingBudgets, e.Message);
            }

            IsBusy = false;

        }

        private void AddTotalesForTable()
        {
            if (Presupuestos == null || Presupuestos.Count == 0)
            {
                return;
            }

            var totalEjecutado = Presupuestos.Sum(x => x.PreEjecutado);
            var totalPresu = Presupuestos.Sum(x => x.PrePresupuesto);

            Presupuestos.Add(new Presupuestos() { Descripcion = "Total", PreEjecutado = totalEjecutado, PrePresupuesto = totalPresu, PreTipo = "Total" });

        }

        public void SetLogros() { PushModalAsync(new SetLogrosModal(this)); }

        public async void OnListItemSelected(Presupuestos presupuestos)
        {
            if (!DS_RepresentantesParametros.GetInstance().GetParPresupuestosAgregaPedidos() || presupuestos == null || IsBusy || !CurrentTipoPresupuesto.CodigoUso.Contains("PROD") || !fromPedidos)
            {
                return;
            }

            if (presupuestos.ProID <= 0)
            {
                return;
            }

            IsBusy = true;

            try
            {
                ProductosTemp producto = myProd.GetProductoById(presupuestos.ProID, CurrentMoneda != null ? CurrentMoneda.MonCodigo : null);

                if (producto == null)
                {
                    return;
                }

                CurrentProducto = producto;

                dialogAgregarProducto = new AgregarProductosModal(myProd, false)
                {
                    OnCantidadAccepted = (s) =>
                    {

                        if ((myParametro.GetParPedidosProductosColoresYTamanos() || myParametro.GetParPedidosProductosUnidades()) && s.ProductToAdd != null)
                        {
                            CurrentProducto = s.ProductToAdd;
                        }

                        var PrecioLista = CurrentProducto.Precio;
                        var PrecioListaConImpuesto = PrecioLista * ((CurrentProducto.Itbis / 100) + 1);

                        CurrentProducto.Cantidad = s.Cantidad;
                        CurrentProducto.CantidadDetalle = s.Unidades;
                        CurrentProducto.InvAreaId = s.InvArea;
                        CurrentProducto.InvAreaDescr = s.InvAreaDescr;
                        CurrentProducto.PrecioTemp = s.Precio;
                        CurrentProducto.IndicadorDocena = s.IndicadorDocena;
                        CurrentProducto.CantidadFacing = s.Facing;
                        CurrentProducto.ProAtributo1 = s.Atributo1?.Key;
                        CurrentProducto.ProAtributo2 = s.Atributo2?.Key;
                        CurrentProducto.ProAtributo1Desc = s.Atributo1?.Value;
                        CurrentProducto.ProAtributo2Desc = s.Atributo2?.Value;
                        CurrentProducto.CedCodigo = s.CedCodigo;
                        CurrentProducto.CedDescripcion = s.CedDescripcion;
                        CurrentProducto.IndicadorOfertaForShow = CurrentProducto.IndicadorOferta;

                        if (myParametro.GetParInventariosTomarCantidades() == 1 || myParametro.GetParInventariosTomarCantidades() == 3 || myParametro.GetParColocacionProductosTomarCantidades() == 1)
                        {
                            CurrentProducto.CanTidadGond = s.CanTidadGond;
                            CurrentProducto.CantidadAlm = s.CantidadAlm;
                            CurrentProducto.CanTidadTramo = s.CanTidadTramo;
                        }
                        else if (myParametro.GetParInventariosTomarCantidades() == 2 || myParametro.GetParColocacionProductosTomarCantidades() == 2)
                        {
                            CurrentProducto.CanTidadGond = s.CanTidadGond;
                            CurrentProducto.CantidadAlm = s.CantidadAlm;
                            CurrentProducto.UnidadGond = s.UnidadGond;
                            CurrentProducto.UnidadAlm = s.UnidadAlm;
                        }

                        if (!string.IsNullOrWhiteSpace(Arguments.Values.CurrentClient.CliTipoComprobanteFAC) && Arguments.Values.CurrentClient.CliTipoComprobanteFAC == "14" && (Arguments.Values.CurrentModule == Modules.VENTAS || Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.COTIZACIONES))
                        {
                            CurrentProducto.Itbis = 0;
                        }

                        if (s.MotId != -1)
                        {
                            CurrentProducto.MotIdDevolucion = s.MotId;
                        }

                        CurrentProducto.CantidadOferta = myParametro.GetParCotOfertasManuales() || myParametro.GetParVenOfertasManuales() || myParametro.GetParPedOfertasManuales() ? s.CantidadOfertaManual : 0;

                        CurrentProducto.CantidadPiezas = s.CantidadPiezas;

                        if (myParametro.GetParCambioMercanciaInsertarLotesParaRecivir())
                        {
                            CurrentProducto.LoteRecibido = s.LoteRecibido;
                            CurrentProducto.LoteEntregado = s.LoteEntregado;
                        }
                        else
                        {
                            CurrentProducto.Lote = s.Lote;
                        }

                        if (myParametro.GetParRevenimiento())
                        {
                            CurrentProducto.CantidadDetalleR = s.CantidadDetalleR;
                        }

                        if (s.Precio > 0 && Arguments.Values.CurrentModule != Modules.INVFISICO)
                        {
                            CurrentProducto.Precio = s.Precio;
                        }

                        CurrentProducto.IndicadorEliminar = s.IndicadorEliminar;
                        CurrentProducto.DesPorcientoManual = s.DescuentoManual;
                        CurrentProducto.ValorOfertaManual = s.ValorOfertaManual;

                        if (s.IndicadorPromocion)
                        {
                            CurrentProducto.ShowDescuento = false;
                        }
                        else if (CurrentProducto.DesPorcientoManual > 0)
                        {
                            CurrentProducto.Descuento = (CurrentProducto.DesPorcientoManual * CurrentProducto.Precio) / 100;
                            CurrentProducto.DesPorciento = CurrentProducto.DesPorcientoManual;
                            CurrentProducto.ShowDescuento = true;
                        }
                        else if (myParametro.GetParPedDescLip())
                        {
                            CurrentProducto.Descuento = s.DescuentoXLipCodigo;
                            CurrentProducto.ShowDescuento = true;
                        }
                        else if (!myParametro.GetParDescuentosProductosMostrarPreview())
                        {
                            CurrentProducto.Descuento = 0;
                            CurrentProducto.DesPorciento = 0;
                            CurrentProducto.ShowDescuento = false;
                        }

                        if (myParametro.GetDescuentoxPrecioNegociado())
                        {
                            CurrentProducto.PrecioTemp = s.Precio;
                            if (PrecioLista > 0 && Arguments.Values.CurrentModule != Modules.INVFISICO)
                            {
                                CurrentProducto.Precio = PrecioLista;
                            }

                            if (myParametro.GetParPedidosEditarPrecioNegconItebis())
                            {
                                var precionegociadosinimpuesto = s.Precio;// (s.Precio / ((CurrentProducto.Itbis / 100) + 1));
                                CurrentProducto.Descuento = (PrecioLista) - (precionegociadosinimpuesto);
                                CurrentProducto.DesPorciento = (PrecioLista - precionegociadosinimpuesto) / 100;
                                CurrentProducto.ShowDescuento = true;
                            }
                            else
                            {
                                CurrentProducto.Descuento = (PrecioLista) - (s.Precio);
                                CurrentProducto.DesPorciento = (PrecioLista - s.Precio) / 100;
                                CurrentProducto.ShowDescuento = true;
                            }
                        }

                        CurrentProducto.IndicadorPromocion = s.IndicadorPromocion;

                        AgregarProducto(CurrentProducto);  //Se inserta primero el producto seleccionado y luego la oferta manual si existe

                        var usarColoresYTamanos = myParametro.GetParPedidosProductosColoresYTamanos();

                        var rawCode = CurrentProducto.ProCodigo.Split('-');

                        if (!(rawCode.Length > 2))
                        {
                            usarColoresYTamanos = false;
                        }

                        if (!usarColoresYTamanos)
                        {
                            CurrentProducto = null;//Se pone null aqui en vez de hacerlo en AgregarProducto(CurrentProducto)
                        }

                        
                    }
                };


                dialogAgregarProducto.CurrentProduct = producto;
                dialogAgregarProducto.IsBusy = false;

                await PushAsync(dialogAgregarProducto);

            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        public void AgregarProducto(ProductosTemp producto, bool clearSearch = true)
        {
            try
            {
                var item = producto.Copy();

                item.IndicadorOferta = false;
                bool indicadorOferta = producto.IndicadorOferta;

                myProd.InsertInTemp(item, isEntrega: false, IsMultiEntrega: false);
                item.IndicadorEliminar = false;
                item.IndicadorOferta = indicadorOferta;
                SetResumenTotales();

            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                DisplayAlert(AppResource.ErrorAddingProduct, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
        }

        private void SetResumenTotales()
        {
            PedidosViewModel.Instance().Resumen = myProd.GetTempTotales(1);
        }

    }
}
