using MovilBusiness.Configuration;
using MovilBusiness.Controls.Behavior;
using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.model.Internal.Structs.Args;
using MovilBusiness.Model;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AgregarProductoDevolucionModal : ContentPage, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;

        private ProductosTemp currentproducto;
        public ProductosTemp CurrentProducto { get => currentproducto; set { currentproducto = value; if (value != null) { LoadProductoAgregado(); } RaiseOnPropertyChanged(); } }

        private DS_Devoluciones myDev;
        private DS_UsosMultiples myUso;
        private DS_RepresentantesParametros myParametro;
        private DS_ProductosLotes myPrlLote;

        private ProductosLotes currentlote = null;
        public ProductosLotes CurrentLote { get => currentlote; set { currentlote = value;  DateTime.TryParse(value?.PrlFechaVencimiento, out DateTime result); CurrentFechaVencimiento = currentlote == null ? DateTime.Today.AddDays(-1) : result;  OnCurrentLoteChanged(); RaiseOnPropertyChanged(); } }

        public List<MotivosDevolucion> MotivosDevolucion { get; set; }
        public List<UsosMultiples> Acciones { get; set; }
        public List<UsosMultiples> Condicion { get; set; }

        public ICommand VerProductosDescuentoCommand { get; private set; }

        private List<ClientesFacturasProductosLotes> facturas;
        public List<ClientesFacturasProductosLotes> Facturas { get => facturas; set { facturas = value; RaiseOnPropertyChanged(); } }

        private ClientesFacturasProductosLotes currentfactura;
        public ClientesFacturasProductosLotes CurrentFactura { get => currentfactura; set { currentfactura = value; OnCurrentFacturaChanged(); RaiseOnPropertyChanged(); } }

        private ObservableCollection<ProductosLotes> lotes;
        public ObservableCollection<ProductosLotes> Lotes { get => lotes; set { lotes = value; RaiseOnPropertyChanged(); } }

        private MotivosDevolucion currentmotivo;
        public MotivosDevolucion CurrentMotivo { get => currentmotivo; set { currentmotivo = value; OnMotivoDevolucionChanged(); RaiseOnPropertyChanged(); } }

        private UsosMultiples currentaccion = null;
        public UsosMultiples CurrentAccion { get => currentaccion; set { currentaccion = value; RaiseOnPropertyChanged(); } }

        private ObservableCollection<DescuentosRecargos> descuentos;
        public ObservableCollection<DescuentosRecargos> Descuentos { get => descuentos; set { descuentos = value; RaiseOnPropertyChanged(); } }

        private DescuentosRecargos currentdescuento;
        public DescuentosRecargos CurrentDescuento { get => currentdescuento; set { currentdescuento = value; CargarDetalleDescuento(); RaiseOnPropertyChanged(); } }

        private ObservableCollection<DescuentosRecargosDetalle> descuentodetalles;
        public ObservableCollection<DescuentosRecargosDetalle> DescuentoDetalles { get => descuentodetalles; set { descuentodetalles = value; RaiseOnPropertyChanged(); } }


        private UsosMultiples currentcondicion = null;
        public UsosMultiples CurrentCondicion { get => currentcondicion; set { currentcondicion = value; RaiseOnPropertyChanged(); } }

        private DateTime currentfechavencimiento;
        public DateTime CurrentFechaVencimiento { get => currentfechavencimiento; set { currentfechavencimiento = value; RaiseOnPropertyChanged(); } }

        private  bool TipoLoteSeleccion = false;
        public bool isLoteXSeleccion { get => TipoLoteSeleccion; set { TipoLoteSeleccion = value; RaiseOnPropertyChanged(); } }
        private  bool TipoLoteManual = false;
        public bool isLoteManual { get => TipoLoteManual; set { TipoLoteManual = value; RaiseOnPropertyChanged(); } }

        public ICommand CommandLoteBySeletion { get; private set; }
        public ICommand CommandLoteManual { get; private set; }
        // public ICommand SaveProducto { get; private set; }

        public bool ParMotivoUnico { get; set; }
        public bool ParCondicionUnico { get; set; }
        public bool ParComboLote { get; set; }
        public bool UseAccion { get; set; }
        public bool UseCondicion { get; set; }
        public bool UseFacturaManual { get; set; }
        public bool UseComboFactura { get; set; }

        bool noUnidades;
        public bool NoUnidades { get => noUnidades; set { noUnidades = value; RaiseOnPropertyChanged(); } }
        public bool ParOculDevUnidad { get; set; }
        public bool NoUseCantidadOf { get; set; }
        public bool ParLoteDinamico { get; set; }
        public bool ParClienteFacturaLote { get; set; }

        public bool ParOcultarFactura { get; private set; }

        char[] CamposObligatorios;
        public Action<DevolucionesProductosArgs> OnAceptar { get; set; }
        public string LoteSeleccionado;

        private DS_DescuentosRecargos myDes;
        private DS_HistoricoFacturas myHis;
        private bool parDescuentosVisualizar;

        public AgregarProductoDevolucionModal(MotivosDevolucion MotivoUnico = null)
        {
            CamposObligatorios = MotivoUnico?.MotCamposObligatorios?.ToCharArray();
            myDev = new DS_Devoluciones();
            myUso = new DS_UsosMultiples();
            myParametro = DS_RepresentantesParametros.GetInstance();
            myPrlLote = new DS_ProductosLotes();
            myDes = new DS_DescuentosRecargos();
            myHis = new DS_HistoricoFacturas();
            CurrentFechaVencimiento = DateTime.Today.AddDays(-1);

            parDescuentosVisualizar = myParametro.GetParDescuentosEnDevoluciones();

            CommandLoteBySeletion = new Command((s) => { OnChangeTipoLote(true, false); });
            CommandLoteManual = new Command((s) => { OnChangeTipoLote(false, true); });

            VerProductosDescuentoCommand = new Command(GoProductosDescuento);
            // SaveProducto = new Command(SaveProductoInTemp);

            ParMotivoUnico = myParametro.GetParDevolucionesMotivoUnico();
            ParCondicionUnico = myParametro.GetParDevolucionesCondicionUnico();
            ParComboLote = myParametro.GetParDevolucionesLotes() == 1 || myParametro.GerParClientesFacturaProductosLotes();
            NoUnidades = !myParametro.GetParOcultarUnidadesEnDevolucion();
            ParOculDevUnidad = myParametro.GetParOcultarUnidadesEnDevolucion();
            NoUseCantidadOf = !myParametro.GetParDevolucionesOcultarCantidadOferta();
            var parAccion = myParametro.GetParDevolucionesAccion();
            ParLoteDinamico = myParametro.GetParDevDynamicLote();
            ParClienteFacturaLote = myParametro.GerParClientesFacturaProductosLotes();

            UseCondicion =  myParametro.GetParDevolucionCondicion();
            UseAccion = !string.IsNullOrWhiteSpace(parAccion) && parAccion.ToUpper().Trim() == "D";
            UseFacturaManual = !myParametro.GetParDevolucionesProductosFacturas();
            UseComboFactura = myParametro.GetParDevolucionesFacturaProductoCombo();
            

            if (UseAccion)
            {
                Acciones = myUso.GetDevolucionAccion();

                if(Acciones != null && Acciones.Count > 0)
                {
                    CurrentAccion = Acciones[0];
                }
            }

            if (UseCondicion && !ParCondicionUnico)
            {
                Condicion = myUso.GetDevolucionCondicion();
            }

            if (!ParMotivoUnico)
            {
                MotivosDevolucion = myDev.GetMotivosDevolucion();
            }
             

            if (ParMotivoUnico && ParOculDevUnidad)
            {              
                //MUESTRA EL CAMPO UNIDAD PARA MOTIVOS ESPECIFICOS
                if (MotivoUnico != null && !string.IsNullOrWhiteSpace(MotivoUnico.MotCaracteristicas) && MotivoUnico.MotCaracteristicas.ToUpper().Contains("U"))
                {
                    NoUnidades = true;
                }
                else
                {
                    NoUnidades = false;
                }
            }

            InitializeComponent();

            lblMotivo.IsVisible = !ParMotivoUnico;
            comboMotivo.IsVisible = !ParMotivoUnico;

            lblCondicion.IsVisible = !ParCondicionUnico && UseCondicion;
            comboCondicion.IsVisible = !ParCondicionUnico && UseCondicion;
            OnChangeTipoLote(true, false);

            ParOcultarFactura = myParametro.GetParDevolucionesOcultarFactura();
            if (ParOcultarFactura)
            {
                lblFactura.IsVisible = false;
                EditFactura.IsVisible = false;
                comboFactura.IsVisible = false;
            }

            if (ParComboLote)
            {
                EditLote.IsVisible = false;
                comboLote.IsVisible = true;
                if (!myParametro.GetParValidarFechaVencimiento())
                {
                    pickerFechaVenc.IsEnabled = false;
                }
            }
            else
            {
                bool parOcultarLote = myParametro.GetParDevolucionesOcultarLoteYFechaVencimiento();
                if (parOcultarLote)
                {
                    lblLote.IsVisible = false;
                    EditLote.IsVisible = false;
                    comboLote.IsVisible = false;
                    lblFechaVencimiento.IsVisible = false;
                    pickerFechaVenc.IsVisible = false;
                }
                else
                {
                    EditLote.IsVisible = true;
                    comboLote.IsVisible = false;
                    pickerFechaVenc.IsEnabled = true;
                }
            }

            if (myParametro.GetParConvertirCajasAUnidadesSinDetalleProductos())
            {
                EditUnidades.IsVisible = false;
                lblUnidades.IsVisible = false;
            }

            if (myParametro.GetParDevolucionesOcultarFechaVencimiento())
            {
                lblFechaVencimiento.IsVisible = false;
                pickerFechaVenc.IsVisible = false;
            }

            if (ParLoteDinamico && isLoteXSeleccion)
            {
                EditLote.IsVisible = false;
                comboLote.IsVisible = true;

                if(!myParametro.GetParValidarFechaVencimiento())
                {
                    pickerFechaVenc.IsEnabled = false;
                }

            }
            else if(ParLoteDinamico && isLoteManual)
            {
                EditLote.IsVisible = true;
                comboLote.IsVisible = false;
                pickerFechaVenc.IsEnabled = true;
            }

            if (parDescuentosVisualizar)
            {
                if (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida())             
                    descuentosUndView.BindingContext = this;        
                else
                    descuentosView.BindingContext = this;
            }

            if (myParametro.GetParDevolucionesSwitchSeleccionFactura())
            {
                SwitchManual.IsToggled = true;
            }
            

            BindingContext = this;
        }

        private void Cancelar(object sender, EventArgs args)
        {
            Navigation.PopModalAsync(true);
        }

        private async void ValidarProducto(object sender, EventArgs ex)
        {
            if (IsBusy)
            {
                return;
            }
            IsBusy = true;

            try
            {
                bool cantidades, unidad;
                dynamic cantidad, unidades;

                if (CurrentProducto.ProDatos3 != null && CurrentProducto.ProDatos3.Contains("A"))
                {
                    cantidades = double.TryParse(EditCantidad.Text, out double cantidaddou);
                    cantidad = cantidaddou;
                    unidad = double.TryParse(EditUnidades.Text, out double unidadesdou);
                    unidades = unidadesdou;
                }
                else
                {
                    cantidades = int.TryParse(EditCantidad.Text, out int cantidadint);
                    cantidad = cantidadint;
                    unidad = int.TryParse(EditUnidades.Text, out int unidadesint);
                    unidades = unidadesint;
                }

                bool oferta = int.TryParse(EditCantOferta.Text, out int cantidadOferta);


                var args = new DevolucionesProductosArgs
                {
                    cantidad = cantidad,
                    cantidaddetalle = unidades,
                    cantidadoferta = cantidadOferta,

                    MotId = CurrentMotivo == null ? 0 : CurrentMotivo.MotID,
                    lote = string.IsNullOrWhiteSpace(EditLote.Text) ? "" : EditLote.Text.Trim()
                };

                if (myParametro.GetParCajasUnidadesProductos())
                {
                    var proUnidades = CurrentProducto.ProUnidades;

                    if (proUnidades == 0)
                    {
                        proUnidades = 1;
                    }

                    args.cantidad = (cantidad * proUnidades) + unidades;
                    args.cantidaddetalle = 0;
                }

                /*    if(cantidad == 0 && unidades == 0)  //Se quita validacion para que pueda editar y eliminar productos de la devolucion
                    {
                        Functions.DisplayAlert(AppResource.Warning, "Debes de especificar la cantidad.");
                        return;
                    }*/

                if ((((CurrentMotivo != null && !string.IsNullOrWhiteSpace(CurrentMotivo.MotCaracteristicas) && 
                    CurrentMotivo.MotCaracteristicas.ToUpper().Contains("V")) || isLoteObligatorio) && CurrentLote == null)
                    //|| (myParametro.GetParDevolucionesLoteObligatorio() || CamposObligatorios?[0] == '1') &&
                    || (myParametro.GetParDevolucionesLoteObligatorio() || (CamposObligatorios?.Length > 0  && CamposObligatorios?[0] == '1')) &&
                    (((string.IsNullOrEmpty(EditLote.Text)) && !ParComboLote) || (ParComboLote && CurrentLote == null)))
                {
                    await Functions.DisplayAlert(AppResource.Warning, AppResource.MustSpecifyLotWarning);
                    IsBusy = false;
                    return;
                }

                if (ParLoteDinamico)
                {
                    args.lote = isLoteXSeleccion ? CurrentLote.PrlLote : EditLote.Text.Trim();
                }
                else if (ParComboLote || isLoteObligatorio)
                {
                    args.lote = CurrentLote == null ? "" : CurrentLote.PrlLote;
                }
                else
                {
                    args.lote = EditLote.Text;
                }

                if (UseAccion && CurrentAccion != null)
                {
                    args.Accion = CurrentAccion.CodigoUso;
                }

                if (UseCondicion && CurrentCondicion != null)
                {
                    args.Condicion = CurrentCondicion.CodigoUso;
                }

                double cantidadtotal = (cantidad * CurrentProducto.ProUnidades) + unidades;
                if (!string.IsNullOrWhiteSpace(CurrentProducto.ProCantidadMultiploVenta.ToString()) && CurrentProducto.ProCantidadMultiploVenta > 0 /*&& !CurrentProduct.UnmCodigo.ToUpper().Contains("UND")*/)
                {
                    var multiplo = cantidad % CurrentProducto.ProCantidadMultiploVenta;

                    if ((multiplo != 0 && cantidad > 0))
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.EnterMultipleQuantity.Replace("@", CurrentProducto.ProCantidadMultiploVenta.ToString()), AppResource.Aceptar);
                        IsBusy = false;
                        return;
                    }
                }

                /*if (cantidad > CurrentProducto.ProCantidadMaxVenta && CurrentProducto.ProCantidadMaxVenta != 0 && cantidad > 0)
                {
                    DisplayAlert(AppResource.Warning, AppResource.EnterQuantityLessThanMaximun.Replace("@", CurrentProducto.ProCantidadMaxVenta.ToString()), AppResource.Aceptar);
                    return;
                }

                if (cantidadtotal < CurrentProducto.ProCantidadMinVenta && CurrentProducto.ProCantidadMinVenta != 0 && cantidadtotal > 0)
                {
                    DisplayAlert(AppResource.Warning, AppResource.EnterQuantityGreaterThanMinimun.Replace("@", CurrentProducto.ProCantidadMinVenta.ToString()), AppResource.Aceptar);
                    return;
                }*/
                
                args.FechaVencimiento = CurrentFechaVencimiento != DateTime.Today.AddDays(-1)? CurrentFechaVencimiento.ToString("yyyy-MM-dd HH:mm:ss") : "";

                if(CurrentFactura != null)
                {
                    args.Documento = UseFacturaManual && EditFactura.IsVisible ? string.IsNullOrWhiteSpace(EditFactura.Text) ? null : EditFactura.Text.Trim() : CurrentFactura.CFPLFactura;
                }
                else
                {
                    args.Documento = UseFacturaManual && EditFactura.IsVisible ? string.IsNullOrWhiteSpace(EditFactura.Text) ? null : EditFactura.Text.Trim() : CurrentProducto.Documento;
                }

                if (!myParametro.GetParDevolucionesNoValidaLimitexFactura()) 
                { 
                    if (myParametro.GetParDevolucionesProductosFacturas() && args.Documento != null)
                    {
                        var cantidadFactura = myHis.GetCantidadProductoByHistoricoFacturasDetalle(args.Documento, Arguments.CurrentUser.RepCodigo, CurrentProducto.ProID);
                        var cantOferta = myHis.GetCantidadOfertaProductoByHistoricoFacturasDetalle(args.Documento, Arguments.CurrentUser.RepCodigo, CurrentProducto.ProID);
                        if (cantidad > cantidadFactura)
                        {
                            await Functions.DisplayAlert(AppResource.Warning, "No puedes realizar una devolución con una cantidad mayor a la de la factura.");
                            IsBusy = false;
                            return;
                        }

                        if (args.cantidadoferta > cantOferta)
                        {
                            await Functions.DisplayAlert(AppResource.Warning, "No puedes realizar una devolución con una cantidad de oferta mayor a la de la factura.");
                            IsBusy = false;
                            return;
                        }
                    }
                    else if (myParametro.GetParDevolucionesFacturaProductoCombo() && args.Documento != null && args.lote != null)
                    {
                        var cantFactura = myPrlLote.GetCantidadByClientesFacturasProductosLotes(args.Documento, args.lote, CurrentProducto.ProID);
                        var cantOferta = myPrlLote.GetCantidadOfertaByClientesFacturasProductosLotes(args.Documento, args.lote, CurrentProducto.ProID);

                        if (cantidad > cantFactura)
                        {
                            await Functions.DisplayAlert(AppResource.Warning, "No puedes realizar una devolución con una cantidad mayor a la de la factura.");
                            IsBusy = false;
                            return;
                        }

                        if (args.cantidadoferta > cantOferta)
                        {
                            await Functions.DisplayAlert(AppResource.Warning, "No puedes realizar una devolución con una cantidad de oferta mayor a la de la factura.");
                            IsBusy = false;
                            return;
                        }
                    }
                }


                int[] CamposObligatorio = new int[6];

                if(CamposObligatorios != null && CamposObligatorios.Count() > 0)
                {
                    for (int i = 0; i <= (CamposObligatorios.Count() - 1) && i <= 5; i++)
                    {
                        CamposObligatorio[i] = CamposObligatorios[i];
                    }
                }

                if (CurrentProducto.ProDatos3.Contains("L") && CamposObligatorio[0] != '1' && string.IsNullOrWhiteSpace(args.lote) && myParametro.GetParDevolucionesLoteObligatorio())
                {
                    await Functions.DisplayAlert(AppResource.Warning, AppResource.MustSpecifyLotWarning);
                    IsBusy = false;
                    return;
                }

                if (myParametro.GetParDevolucionesFechaObligatoria() && CamposObligatorio[1] != '1' && string.IsNullOrWhiteSpace(args.FechaVencimiento))
                {
                    await Functions.DisplayAlert(AppResource.Warning, AppResource.MustSpecifyExpirationDate);
                    IsBusy = false;
                    return;
                }

                if (CurrentProducto.ProDatos3.Contains("F") && CamposObligatorio[1] !=  '1' && string.IsNullOrWhiteSpace(args.FechaVencimiento))
                {
                    await Functions.DisplayAlert(AppResource.Warning, AppResource.MustSpecifyExpirationDate);
                    IsBusy = false;
                    return;
                }

                int parVencimientoDev = myParametro.GetParDevolucionesDiasVencimiento();
                var motivo = myDev.GetMotivoDevolucionbyId(args.MotId);
                bool valPoliticaVencimiento = motivo != null ? motivo.MotCaracteristicas.ToUpper().Contains("X") : false;

                if (parVencimientoDev > 0 || valPoliticaVencimiento || CamposObligatorio[1] == '1')
                {
                    if (string.IsNullOrWhiteSpace(args.FechaVencimiento))
                    {
                        await Functions.DisplayAlert(AppResource.Warning, AppResource.MustSpecifyExpirationDate);
                        IsBusy = false;
                        return;
                    }

                    if (!myDev.ValidarFechaVencimientoContraPoliticaDevolucion(CurrentProducto.ProID, args.FechaVencimiento))
                    {
                        await Functions.DisplayAlert(AppResource.Warning, AppResource.ExpirationDateNotMeetReturnPolicy);
                        IsBusy = false;
                        return;
                    }
                }

                if (!cantidades && CamposObligatorio[2] == '1')
                {
                    await Functions.DisplayAlert(AppResource.Warning, AppResource.QuantityFieldCannotBeEmpty);
                    IsBusy = false;
                    return;
                }
                if (!unidad && CamposObligatorio[3] == '1')
                {
                    await Functions.DisplayAlert(AppResource.Warning, AppResource.UnitsFieldCannotBeEmpty);
                    IsBusy = false;
                    return;
                }

                if (!oferta && CamposObligatorio[4] == '1')
                {
                    await Functions.DisplayAlert(AppResource.Warning, AppResource.OfferFieldCannotBeEmpty);
                    IsBusy = false;
                    return;
                }


                if (myParametro.GetParDevolucionesProductoMotivoVencido() && myPrlLote.ValidarProductoFechaVencimiento(CurrentProducto.ProID, args.lote, CurrentFechaVencimiento.ToString("yyyy-MM-dd")) && CurrentMotivo.MotID == 2)
                {
                    await Functions.DisplayAlert(AppResource.Warning, AppResource.ProductDateIsGreaterThanExpirationDateWarning);
                    IsBusy = false;
                    return;
                }

                if ((myParametro.GetParDevolucionesFacturaObligatoria() || CamposObligatorio[5] == '1') && string.IsNullOrWhiteSpace(args.Documento))
                {
                    await Functions.DisplayAlert(AppResource.Warning, AppResource.MustSpecifyInvoiceNumber);
                    IsBusy = false;
                    return;
                }

                OnAceptar?.Invoke(args);
                await Navigation.PopModalAsync(true);
                IsBusy = false;
            }
            catch (Exception e)
            {
                await Functions.DisplayAlert(AppResource.ErrorAddingProduct, e.Message);
                IsBusy = false;
            }
        }

        private void LoadProductoAgregado()
        {
            try
            {
                if (ParClienteFacturaLote)
                {
                    Lotes = new ObservableCollection<ProductosLotes>(myPrlLote.GetProductoLotesByCliente(CurrentProducto.ProID, Arguments.Values.CurrentClient.CliID));
                }
                else if (ParComboLote)
                {
                    Lotes = new ObservableCollection<ProductosLotes>(myPrlLote.GetLotesByProId(CurrentProducto.ProID));
                }
               
                if (CurrentProducto.Cantidad == 0 && CurrentProducto.CantidadDetalle == 0)
                {
                    ClearValues();
                    if (CurrentProducto != null)
                    {
                        if (myDev.GetProductoProDatos3(CurrentProducto)) //Revisión de columna ProDatos3.-
                        {
                            EditCantidad.Behaviors.Clear();
                        }
                        else
                        {
                            EditCantidad.Behaviors.Add(new NumericValidation());
                        }
                    }

                    if (!ParMotivoUnico)
                    {
                        var parMotivoPorDefecto = myParametro.GetParDevolucionesMotivoPorDefecto();

                        if (!string.IsNullOrWhiteSpace(parMotivoPorDefecto) && MotivosDevolucion != null && MotivosDevolucion.Count > 0)
                        {
                            CurrentMotivo = MotivosDevolucion.Where(x => x.MotID.ToString().Equals(parMotivoPorDefecto)).FirstOrDefault();
                        }
                    }

                    if (parDescuentosVisualizar)
                    {
                        Descuentos = new ObservableCollection<DescuentosRecargos>(myDes.GetDescuentosDisponibles(Arguments.Values.CurrentClient.TiNID, Arguments.Values.CurrentClient.CliID, CurrentProducto.ProID));
                        if (Descuentos != null && Descuentos.Count > 0)
                        {
                            CurrentDescuento = Descuentos[0];

                            DescuentoDetalles = new ObservableCollection<DescuentosRecargosDetalle>(myDes.GetDetalles(CurrentDescuento.DesID));
                        }
                        else
                        {
                            Descuentos = null;
                            DescuentoDetalles = null;
                        }

                        if (CurrentDescuento != null)
                        {
                            if (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida())
                                descuentosUndView.IsVisible = true;
                            else
                                descuentosView.IsVisible = true;
                        }
                    }
                    else
                        Descuentos = null;
                 
                    return;
                }
                else
                {
                    if (CurrentProducto != null)
                    {
                        if (myDev.GetProductoProDatos3(CurrentProducto)) //Revisión de columna ProDatos3.-
                        {

                            EditCantidad.Behaviors.Clear();

                        }
                        else
                        {
                            EditCantidad.Behaviors.Add(new NumericValidation());
                        }
                    }

                    if (parDescuentosVisualizar)
                    {
                        Descuentos = new ObservableCollection<DescuentosRecargos>(myDes.GetDescuentosDisponibles(Arguments.Values.CurrentClient.TiNID, Arguments.Values.CurrentClient.CliID, CurrentProducto.ProID));
                        if (Descuentos != null && Descuentos.Count > 0)
                        {
                            CurrentDescuento = Descuentos[0];

                            DescuentoDetalles = new ObservableCollection<DescuentosRecargosDetalle>(myDes.GetDetalles(CurrentDescuento.DesID));
                        }
                        else
                        {
                            Descuentos = null;
                            DescuentoDetalles = null;
                        }

                        if (CurrentDescuento != null)
                        {
                            if (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida())
                                descuentosUndView.IsVisible = true;
                            else
                                descuentosView.IsVisible = true;
                        }
                    }
                    else
                        Descuentos = null;

                }

                if (!ParMotivoUnico)
                {
                    MotivosDevolucion motivo = myDev.GetMotivoDevolucionbyId(CurrentProducto.MotIdDevolucion);

                    if (motivo != null && MotivosDevolucion != null && MotivosDevolucion.Count > 0)
                    {
                        var parMotivoPorDefecto = myParametro.GetParDevolucionesMotivoPorDefecto();

                        CurrentMotivo = MotivosDevolucion.Where(x => x.MotID == motivo.MotID).FirstOrDefault();
                    }

                    if(motivo == null)
                    {
                        var parMotivoPorDefecto = myParametro.GetParDevolucionesMotivoPorDefecto();

                        if (!string.IsNullOrWhiteSpace(parMotivoPorDefecto) && MotivosDevolucion != null && MotivosDevolucion.Count > 0)
                        {
                            CurrentMotivo = MotivosDevolucion.Where(x => x.MotID.ToString().Equals(parMotivoPorDefecto)).FirstOrDefault();
                        }
                    }
                }

                if (UseCondicion)
                {
                    CurrentCondicion = Condicion.Where(x => x.CodigoUso == CurrentProducto.DevCondicion).FirstOrDefault();
                }
                
                EditCantidad.Text = CurrentProducto.Cantidad > 0 ? CurrentProducto.Cantidad.ToString() : "";
                EditUnidades.Text = CurrentProducto.CantidadDetalle > 0 ? CurrentProducto.CantidadDetalle.ToString() : "";
                EditCantOferta.Text = CurrentProducto.CantidadOferta > 0 ? CurrentProducto.CantidadOferta.ToString() : "";
                EditFactura.Text = CurrentProducto.Documento;

                if (myParametro.GetParCajasUnidadesProductos())
                {
                    int Paquetes = 0;
                    int Unidades = 0;

                    var proUnidades = CurrentProducto.ProUnidades;

                    if (proUnidades == 0)
                    {
                        proUnidades = 1;
                    }

                    Paquetes = (int)CurrentProducto.Cantidad / proUnidades;
                    Unidades = (int)CurrentProducto.Cantidad % proUnidades;

                    EditCantidad.Text = Paquetes > 0 ? Paquetes.ToString() : "";
                    EditUnidades.Text = Unidades > 0 ? Unidades.ToString() : "";
                }

                DateTime.TryParse(CurrentProducto.FechaVencimiento, out DateTime fecha);
                CurrentFechaVencimiento = fecha;

                if (ParComboLote)
                {
                    CurrentLote = null;
                   // ProductosLotes lote = myPrlLote.GetByProIDAndLote(CurrentProducto.Lote, CurrentProducto.ProID);

                    if (!string.IsNullOrWhiteSpace(CurrentProducto.Lote))
                    {
                        CurrentLote = Lotes.Where(x => x.PrlLote.Trim() == CurrentProducto.Lote.Trim() && x.ProID == CurrentProducto.ProID).FirstOrDefault();
                        //Lotes.ElementAtOrDefault(myPrlLote.GetIndexLotesByProId(CurrentLote.ProID, CurrentLote.PrlLote));
                        // LoteSeleccionado = CurrentLote.PrlLote;
                        //Task.Delay(200);
                    }
                }
                else
                {
                    EditLote.Text = CurrentProducto.Lote;
                }

                if (UseComboFactura)
                {
                    ClientesFacturasProductosLotes clientesFacturasProductosLotes = myPrlLote.GetFacturasProductosLotesByFactura(CurrentProducto.Documento, ParComboLote ? CurrentLote.PrlLote : EditLote.Text, CurrentProducto.ProID);

                    if (clientesFacturasProductosLotes != null && Facturas != null && Facturas.Count > 0)
                    {
                        CurrentFactura = Facturas.Where(x => x.CFPLFactura == clientesFacturasProductosLotes.CFPLFactura).FirstOrDefault();
                    }
                }
            }
            catch (Exception e)
            {
                Functions.DisplayAlert(AppResource.ErrorLoadingProductData, e.Message);
            }

        }

        private void ClearValues()
        {
            try
            {
                EditCantidad.Text = null;
                EditUnidades.Text = null;
                EditLote.Text = null;
                CurrentLote = null;
                EditFactura.Text = null;
                EditCantOferta.Text = null;
                Descuentos = null;
                CurrentDescuento = null;
                DescuentoDetalles = null;
                CurrentMotivo = null;
                comboLote.SelectedIndex = 0;
                comboMotivo.SelectedIndex = 0;
            }
            catch (Exception e)
            {
                Functions.DisplayAlert(AppResource.Error, e.Message);
            }
        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OnChangeTipoLote(bool LoteSeletion, bool LoteManual)
        {
            isLoteXSeleccion = LoteSeletion;
            isLoteManual = LoteManual;
            
            EditLote.IsVisible = LoteManual;
            comboLote.IsVisible = LoteSeletion;

            pickerFechaVenc.IsEnabled = LoteManual;

            if (currentproducto != null)
            {
                if (currentproducto.ProDatos3.Contains("F"))
                {
                    pickerFechaVenc.IsEnabled = LoteManual;
                    if (LoteManual)
                    {
                        EditLote.Text = CurrentProducto.Lote;
                        CurrentFechaVencimiento = CurrentFechaVencimiento != null ? CurrentFechaVencimiento : DateTime.Now;
                    }
                    else
                    {
                       // Lotes.ElementAt(myPrlLote.GetIndexLotesByProId(CurrentLote.ProID, CurrentLote.PrlLote));
                        DateTime.TryParse(myPrlLote.GetFechaVencimientoProductoLote(currentproducto.ProID, (CurrentLote !=null ? CurrentLote.PrlLote : "")), out DateTime fecha);
                        CurrentFechaVencimiento = fecha;
                    }
                }
                else if (currentproducto.ProDatos3.Equals("L") && !myParametro.GetParValidarFechaVencimiento())
                {
                    pickerFechaVenc.IsEnabled = false;
                }
                else if (currentproducto.ProDatos3.Equals("") && !myParametro.GetParValidarFechaVencimiento())
                {
                    pickerFechaVenc.IsEnabled = false;
                }
            }

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                if (myParametro.GetParCargarComboFacturasFromHistoricoFacturas())
                {
                    Facturas = myPrlLote.GetFacturasProductosbyProducto(CurrentProducto.ProID);
                }

                if (ParComboLote && CurrentLote == null)
                {
                    //CurrentLote = null;
                    // ProductosLotes lote = myPrlLote.GetByProIDAndLote(CurrentProducto.Lote, CurrentProducto.ProID);

                    if (!string.IsNullOrWhiteSpace(CurrentProducto.Lote))
                    {
                        CurrentLote = Lotes.Where(x => x.PrlLote.Trim() == CurrentProducto.Lote.Trim() && x.ProID == CurrentProducto.ProID).FirstOrDefault();
                        //Lotes.ElementAtOrDefault(myPrlLote.GetIndexLotesByProId(CurrentLote.ProID, CurrentLote.PrlLote));
                        // LoteSeleccionado = CurrentLote.PrlLote;
                        if (!myPrlLote.GetLoteExistente(CurrentProducto.Lote, CurrentProducto.ProID))
                        {
                            isLoteXSeleccion = false;
                            isLoteManual = true;
                            EditLote.IsVisible = true;
                            comboLote.IsVisible = false;
                            EditLote.Text = CurrentProducto.Lote;
                        }
                    }
                }
                else if(!ParComboLote)
                {
                    EditLote.Text = CurrentProducto.Lote;
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        private void ComboMotivo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboMotivo.SelectedItem == null)
            {
                return;
            }
            if (!(comboMotivo.SelectedItem is MotivosDevolucion motivo))
            {
                return;
            }
            if (ParOculDevUnidad)
            {
                //MUESTRA EL CAMPO UNIDAD PARA MOTIVOS ESPECIFICOS
                if (!string.IsNullOrWhiteSpace(motivo.MotCaracteristicas) && motivo.MotCaracteristicas.ToUpper().Contains("U"))
                {
                    NoUnidades = true;
                }
                else
                {
                    NoUnidades = false;
                }
            }
        }

        private void Switch_Toggled(object sender, ToggledEventArgs e)
        {
            if (e.Value)
            {
                EditFactura.IsVisible = false;
                comboFactura.IsVisible = true;
                EditCantOferta.IsEnabled = false;
            }
            else
            {
                comboFactura.IsVisible = false;
                EditFactura.IsVisible = true;
            }
        }

        private void OnCurrentFacturaChanged()
        {
            if(CurrentFactura == null)
            {

                EditCantidad.Text = null;
                EditCantOferta.Text = null;
                return;
            }

            EditCantidad.Text = CurrentFactura.CFPLCantidadVendida.ToString();
            EditCantOferta.Text = CurrentFactura.CFPLCantidadOferta.ToString();
        }

        private void OnCurrentLoteChanged()
        {
            if(((ParComboLote && CurrentLote == null) || (!ParComboLote && string.IsNullOrWhiteSpace(EditLote.Text))) || ParOcultarFactura)
            {
                return;
            }

            Facturas = myPrlLote.GetFacturasProductosLotes(ParComboLote ? CurrentLote.PrlLote : EditLote.Text, CurrentProducto.ProID);
            CurrentFactura = Facturas[0];
        }

        private void EditLote_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!UseComboFactura || ParOcultarFactura)
            {
                return;
            }

            if (myParametro.GetParCargarComboFacturasFromHistoricoFacturas())
            {
                Facturas = myPrlLote.GetFacturasProductosbyProducto(CurrentProducto.ProID);
            }
            else
            {
                OnCurrentLoteChanged();
            }
            
        }

        private bool isLoteObligatorio = false;
        private void OnMotivoDevolucionChanged()
        {
            try
            {
                if (CurrentMotivo != null && ParComboLote && !ParClienteFacturaLote)
                {
                    Lotes = new ObservableCollection<ProductosLotes>(myPrlLote.GetLotesByProId(CurrentProducto.ProID, isfecha:CurrentMotivo.MotID == 1));
                }
                else if (CurrentMotivo == null || ParLoteDinamico || ParMotivoUnico || myParametro.GetParDevolucionesOcultarLoteYFechaVencimiento() || ParComboLote)
                {
                    return;
                }
                else
                {
                    comboLote.IsVisible = false;
                    EditLote.IsVisible = true;
                    isLoteObligatorio = false;
                }
            }catch(Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }
        }

        private void CargarDetalleDescuento()
        {
            try
            {
                if (CurrentDescuento == null)
                {
                    return;
                }

                DescuentoDetalles = new ObservableCollection<DescuentosRecargosDetalle>(myDes.GetDetalles(CurrentDescuento.DesID, CurrentDescuento.DesMetodo == 5 ? CurrentProducto.ProID : -1));
            }
            catch (Exception e)
            {
                DisplayAlert("Error cargando detalle", e.Message, "Aceptar");
            }
        }

        private async void GoProductosDescuento(object id)
        {
            var response = await DisplayActionSheet("Seleccione una opción", "Cancelar", null, new string[] { "PRODUCTOS A REGALAR", "PRODUCTOS QUE APLICAN" });
            bool aRegalar = false;

            string grpCodigo;
            switch (response)
            {
                case "PRODUCTOS QUE APLICAN":
                    grpCodigo = CurrentDescuento.GrpCodigo;
                    if (string.IsNullOrEmpty(grpCodigo) || grpCodigo == "0")
                    {
                        await DisplayAlert("Aviso", "Esta oferta no tiene productos que aplican.", "Aceptar");
                        return;
                    }
                    break;
                case "PRODUCTOS A REGALAR":
                    grpCodigo = CurrentDescuento.GrpCodigoDescuento;
                    if (string.IsNullOrEmpty(grpCodigo) || grpCodigo == "0")
                    {
                        await DisplayAlert("Aviso", "Esta oferta no tiene productos a regalar.", "Aceptar");
                        return;
                    }
                    aRegalar = true;
                    break;
                default:
                    return;
            }

            await Navigation.PushModalAsync(new DescuentoMancomunadoProductosModal(grpCodigo, aRegalar));
        }

    }
}