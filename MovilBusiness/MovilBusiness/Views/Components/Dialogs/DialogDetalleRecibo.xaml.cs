
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.model.Internal.Structs.Args;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.Views.Components.Modals;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Dialogs
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DialogDetalleRecibo : ContentView, INotifyPropertyChanged
    {
        public ICommand AplicarDescCommand { get; private set; }
        public ICommand QuitarDesmonteCommand { get; private set; }
        public ICommand PorcDescCommand { get; private set; }
        public ICommand AutorizarDescCommand { get; private set; }
        //private bool editardescuento = false;
        //public bool EditarDescuento { get => editardescuento; set { editardescuento = value; RaiseOnPropertyChanged(); } }

        private double montoadescuento = 0;
        public double MontoADescuento { get => montoadescuento; set { montoadescuento = value; RaiseOnPropertyChanged(); } }

        private double montoadesmonte = 0;
        public double MontoADesmonte { get => montoadesmonte; set { montoadesmonte = value; RaiseOnPropertyChanged(); } }
        private DS_Recibos myRec;
        private DS_DescuentoFacturas myDesFac;
        private DS_Autorizaciones myAut;
        private DS_RepresentantesParametros myParametro;
        private DS_Clientes myCli;

        private DescFactura CurrentDescuento;
       // private int DescManualLimite;
        private MontoCreditoAplicadoFactura CreditoAplicado = new MontoCreditoAplicadoFactura();
        private MontoDesmonteAplicadoFactura DesmonteAplicado = new MontoDesmonteAplicadoFactura();
        private RecibosDocumentosDetalleArgs Args;
        //private bool AbonoAutorizado;
        //private bool AutFromDescuento, AutFromAbono;

        private bool handlingdesc = false;

        private string btnaplicardesctext = AppResource.RemoveDisc;
        public string BtnAplicarDescText { get => btnaplicardesctext; set { btnaplicardesctext = value; RaiseOnPropertyChanged(); } }

        private string btnquitardesmontetext = "Quitar Descarga";
        public string BtnQuitarDesmonteText { get => btnquitardesmontetext; set { btnquitardesmontetext = value; RaiseOnPropertyChanged(); } }

        private List<double> porcientosdescuentos;
        public List<double> PorcientosDescuentos { get => porcientosdescuentos; set { porcientosdescuentos = value; RaiseOnPropertyChanged(); } }

        private List<double> porcientosdescuentosEx;
        public List<double> PorcientosDescuentosEx { get => porcientosdescuentosEx; set { porcientosdescuentosEx = value; RaiseOnPropertyChanged(); } }

        private bool descConitbis = false;
        public bool DescConItbis { get => descConitbis; set { descConitbis = value; RaiseOnPropertyChanged(); DescConItbisChanged(); } }

        private string porcdescforlabel;
        public string PorcDescForLabel { get => porcdescforlabel; set { porcdescforlabel = value; RaiseOnPropertyChanged(); } }

        private double porcdescuento;
        public double PorcDescuento { get => porcdescuento; set { if (value % 1 == 0) { PorcDescForLabel = (value).ToString(); } else { PorcDescForLabel = value.ToString(); } if (value == porcdescuento) { return; } porcdescuento = value; RaiseOnPropertyChanged(); if(value != 0) OnPorcDescuentoChanged(); } }

        private bool ParDigitarPorcDescuento = false;

        private bool AbonoAutorizado = false;
        public new event PropertyChangedEventHandler PropertyChanged;

        public static readonly BindableProperty CurrentFacturaProperty = BindableProperty.Create(
                                                         propertyName: "CurrentFactura",
                                                         returnType: typeof(RecibosDocumentosTemp),
                                                         declaringType: typeof(DialogDetalleRecibo),
                                                         defaultBindingMode: BindingMode.TwoWay,
                                                         propertyChanged: CurrentFacturaPropertyChanged);

        public RecibosDocumentosTemp CurrentFactura
        {
            get { return (RecibosDocumentosTemp)GetValue(CurrentFacturaProperty); }
            set { SetValue(CurrentFacturaProperty, value); RaiseOnPropertyChanged(); }
        }

        public event EventHandler OnCancel;
        public event EventHandler<RecibosDocumentosDetalleArgs> OnAccepted;
        
        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int lastControlSelect;

        public DialogDetalleRecibo()
        {
            InitializeComponent();

            AplicarDescCommand = new Command(AplicarQuitarDescuento);
            QuitarDesmonteCommand = new Command(AplicarQuitarDesmonte);
            PorcDescCommand = new Command(SeleccionarPorcientoDescuento);
            AutorizarDescCommand = new Command(ShowAlertAutorizarDescuento);

            myRec = new DS_Recibos();
            myDesFac = new DS_DescuentoFacturas();
            myAut = new DS_Autorizaciones();
            myParametro = DS_RepresentantesParametros.GetInstance();
            myCli = new DS_Clientes();

            //   EditarDescuento = myParametro.GetParRecibosEditarDescuentoManual();
            ParDigitarPorcDescuento =  myParametro.GetParRecibosPorcientoDescuentoDigitable() && Arguments.Values.CurrentModule != Modules.RECONCILIACION;

            editPorcDescuento.IsVisible = ParDigitarPorcDescuento;

           // DescManualLimite = myParametro.GetParDescuentoManual();

            contenedor.BindingContext = this;

            if (Arguments.Values.CurrentModule == Modules.RECONCILIACION)
            {
                ControlSaldo.IsEnabled = false;
            }
        }

        private bool isbusy;
        private async void ShowAlertAutorizarDescuento()
        {
            try
            {
                if (CurrentFactura == null || isbusy || Arguments.Values.CurrentModule == Modules.RECONCILIACION)
                {
                    return;
                }

                if (!CurrentFactura.CalcularDesc)
                {
                    await Functions.DisplayAlert(AppResource.Warning, AppResource.PressApplyDiscBeforeAuthorize);
                    return;
                }

                isbusy = true;

                int RecSecuencia = 0;
                int RecSecuenciaParams = myParametro.GetParRecibosSecuenciaPorSector();
                string AreaCtrlsubto = Arguments.Values.CurrentSector != null && Arguments.Values.CurrentClient != null ? myCli.GetareaCtrlCreditOfClienteDetalle(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentSector.SecCodigo).Substring(0, 2) : "";
                if (RecSecuenciaParams >= 1 && myParametro.GetParRecibosPorSector())
                {
                    RecSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Recibos-" + (RecSecuenciaParams == 2 ? AreaCtrlsubto : Arguments.Values.CurrentSector.SecCodigo));
                }
                else if (myParametro.GetParRecibosRecTipoChkDiferidos() && myRec.ExistsChkDiferidos())
                {
                    RecSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Recibos-1");
                }
                else
                {
                    RecSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Recibos");
                }

                await Navigation.PushModalAsync(new AutorizacionesModal(true, RecSecuencia, 3, CurrentFactura.Documento)
                {
                    OnAutorizacionUsed = (autSec) =>
                    {
                        AutorizarDescuento(autSec);
                    }
                });

            }
            catch (Exception e)
            {
                await Functions.DisplayAlert(AppResource.Warning, e.Message);
            }

            isbusy = false;
        }

        private void AutorizarDescuento(int autSecuencia)
        {
            try
            {
                myRec.UpdateAutSecuenciaFacturaInTemp(CurrentFactura.Referencia, autSecuencia);

                CurrentFactura.AutSecuencia = autSecuencia;

                loadDescuentoByAutSecuencia(autSecuencia);
            }
            catch (Exception e)
            {
                Functions.DisplayAlert(AppResource.Warning, e.Message);
            }
        }

        private void loadDescuentoByAutSecuencia(int autSecuencia)
        {
            if (myParametro.GetParRecibosDescuentoFromAutorizaciones())
            {
                PorcientosDescuentos = myAut.GetPorcientoDescuentoByAutSecuencia(autSecuencia);
                PorcientosDescuentosEx =myAut.GetPorcientoDescuentoByAutSecuenciadoub(autSecuencia);
                PorcDescuento = PorcientosDescuentosEx.FirstOrDefault();
            }
            else
            {
                var descEscalonado = myParametro.GetParRecibosDescuentoEscalonado();

                if (descEscalonado > 0)
                {
                    var list = new List<double>();

                    for (int i = 0; i <= descEscalonado; i++)
                    {
                        list.Add(i);
                    }



                    PorcientosDescuentos = list;
                    PorcientosDescuentosEx = list.ConvertAll(Convert.ToDouble);
                }
                else
                {
                    PorcientosDescuentos = myDesFac.GetPorcientoDescuentoPin(CurrentFactura.Referencia);

                    if (myParametro.GetParRecibosPorcientoDescuentoDisponibleParaFacturas() > 0)
                    {
                        PorcientosDescuentosEx = PorcientosDescuentos.ConvertAll(Convert.ToDouble);
                    }
                    else
                    {
                        PorcientosDescuentosEx = myDesFac.GetPorcientoDescuentoPinDou(CurrentFactura.Referencia);
                    }
                }
            }
        }

        private async void SeleccionarPorcientoDescuento()
        {

                if (PorcientosDescuentosEx == null || PorcientosDescuentosEx.Count == 0 || Arguments.Values.CurrentModule == Modules.RECONCILIACION)
                {
                    return;
                }
            
                var buttons = new List<string>();

                foreach (double value in PorcientosDescuentosEx)
                {
                    buttons.Add(value.ToString());
                }

                var result = await Functions.DisplayActionSheet(AppResource.SelectDiscountPercentage, buttons.ToArray());

                if (!double.TryParse(result, out double porc))
                {
                    return;
                }

                PorcDescuento = porc;
           
        }

        private void AplicarQuitarDescuento()
        {
            if(Arguments.Values.CurrentModule == Modules.RECONCILIACION)
            {
                return;
            }

            if (ControlSaldo.SelectedSegment == 1 && !myParametro.GetParDescuentoAbonos())
            {
                Functions.DisplayAlert(AppResource.Warning, AppResource.DiscountsOnlyApplyToPaidInvoices, AppResource.Aceptar);
                return;
            }

            //handlingdesc = true;

            try
            {

                if (CurrentFactura.CalcularDesc) //quitar desc
                {
                    handlingdesc = false;
                    CurrentFactura.CalcularDesc = false;
                    comboDescuento.IsEnabled = false;
                    editPorcDescuento.IsEnabled = false;
                    myRec.UpdateIndicadorCalcularDescuentoInTemp(CurrentFactura.Referencia, false);
                    CurrentFactura.DescPorciento = 0;
                    CurrentFactura.Descuento = 0;
                    PorcDescuento = 0;
                    CurrentDescuento.DescuentoValor = 0;
                    CurrentDescuento.DescPorciento = 0;
                    double aplicado = CurrentFactura.Balance - Math.Abs(DesmonteAplicado.Desmonte) -  Math.Abs(CreditoAplicado.Credito);
                    CurrentFactura.Aplicado = Math.Round(aplicado, 2);
                    Args.Aplicado = aplicado;
                    Args.Descuento = CurrentDescuento;
                    Args.Desmonte = Math.Abs(DesmonteAplicado.Desmonte);
                    Args.CalcularDesmonte = CurrentFactura.CalcularDesmonte;

                }
                else //aplicar desc
                {
                    handlingdesc = true;
                    CurrentDescuento = myDesFac.GetMontoDescuentoFactura(CurrentFactura);
                    CurrentFactura.CalcularDesc = true;
                    comboDescuento.IsEnabled = true;
                    editPorcDescuento.IsEnabled = true;
                    CurrentFactura.Descuento = CurrentDescuento.DescuentoValor;
                    CurrentFactura.DescPorciento = CurrentDescuento.DescPorciento;
                    PorcDescuento = CurrentDescuento.DescPorciento;

                    myRec.UpdateIndicadorCalcularDescuentoInTemp(CurrentFactura.Referencia, true);
                    if(myParametro.GetParRecibosPorcientoDescuentoDisponibleParaFacturas() > 0 && !(myParametro.GetParDescuentoManual() > 0))
                    {
                        SetAdapterComboFactura(CurrentFactura.AutSecuencia);
                    }
                    
                }
                Args.CalcularDesc = CurrentFactura.CalcularDesc;

                BtnAplicarDescText = !CurrentFactura.CalcularDesc ? AppResource.ApplyDiscount : AppResource.RemoveDisc;
                RaiseOnPropertyChanged(nameof(CurrentFactura));

                Args.Descuento = CurrentDescuento;

            }
            catch (Exception e)
            {
                Functions.DisplayAlert(AppResource.ErrorModifyingDiscount, e.Message);
                handlingdesc = false;
            }

        }

        private void AplicarQuitarDesmonte()
        {
            var Desmonte = 0.00;
            try
            {

                if (CurrentFactura.CalcularDesmonte) //quitar desc
                {
                    CurrentFactura.CalcularDesmonte = false;
                    CurrentFactura.Desmonte = 0.00;
                    myRec.UpdateIndicadorCalcularDesmonteInTemp(CurrentFactura.Referencia, false);
                    double aplicado = CurrentFactura.Balance - CurrentDescuento.DescuentoValor - Math.Abs(CreditoAplicado.Credito);
                    CurrentFactura.Aplicado = Math.Round(aplicado, 2);
                    Args.Aplicado = aplicado;
                    Args.Desmonte = CurrentFactura.Desmonte;
                    MontoADesmonte = 0.00;
                    DesmonteAplicado.Desmonte = 0.00;

                }
                else //aplicar desc
                {
                    CurrentFactura.CalcularDesmonte = true;
                    DesmonteAplicado.Desmonte= new DS_CuentasxCobrar().GetCuentaByReferencia(CurrentFactura.Referencia, CurrentFactura.Documento).cxcDesmonte;
                    myRec.UpdateIndicadorCalcularDesmonteInTemp(CurrentFactura.Referencia, true);
                    double aplicado = CurrentFactura.Balance - CurrentDescuento.DescuentoValor - Math.Abs(DesmonteAplicado.Desmonte) - Math.Abs(CreditoAplicado.Credito);
                    CurrentFactura.Aplicado = Math.Round(aplicado, 2);
                    Args.Aplicado = aplicado;
                    Args.Desmonte = Math.Abs(DesmonteAplicado.Desmonte);
                    MontoADesmonte = Math.Abs(DesmonteAplicado.Desmonte);
                }

                Args.CalcularDesmonte = CurrentFactura.CalcularDesmonte;

                BtnQuitarDesmonteText = !CurrentFactura.CalcularDesmonte ? "Aplicar Descarga" : "Quitar Descarga";
                RaiseOnPropertyChanged(nameof(CurrentFactura));

                Args.Desmonte = Desmonte;
            }
            catch (Exception e)
            {
                Functions.DisplayAlert(AppResource.ErrorModifyingDiscount, e.Message);
                handlingdesc = false;
            }

        }

        private void DescConItbisChanged()
        {
            if (!IsVisible || CurrentFactura == null || Arguments.Values.CurrentModule == Modules.RECONCILIACION)
            {
                return;
            }

            try
            {
                if (DescConItbis)
                {
                    MontoADescuento = (CurrentFactura.MontoTotal - Math.Abs(CreditoAplicado.Credito));
                }
                else
                {
                    MontoADescuento = (CurrentFactura.MontoSinItbis - Math.Abs(CreditoAplicado.CreditoSinItbis));
                }

                CurrentFactura.DefIndicadorItbis = DescConItbis;

                
                if (CurrentFactura.DescPorciento == 0)
                {
                    CurrentDescuento = myRec.ExistsChkDiferidos() ? new DescFactura() { DescuentoValor = CurrentFactura.Descuento, DescPorciento = CurrentFactura.DescPorciento, IndicadorItbis = CurrentFactura.DefIndicadorItbis } : myDesFac.GetMontoDescuentoFactura(CurrentFactura, -1);

                    if (!CurrentFactura.CalcularDesc)
                    {
                        CurrentDescuento = new DescFactura() { DescuentoValor = 0, DescPorciento = 0 };
                    }

                    CurrentFactura.Descuento = CurrentDescuento.DescuentoValor;
                    CurrentFactura.DescPorciento = CurrentDescuento.DescPorciento;

                    RaiseOnPropertyChanged(nameof(CurrentFactura));

                    double aplicado = Math.Round(CurrentFactura.Aplicado, 2);

                    if ((myParametro.GetParDescuentoAbonos() && (ControlSaldo.SelectedSegment == 1)))
                    {
                        aplicado = CurrentFactura.Pendiente - CurrentDescuento.DescuentoValor - Math.Abs(DesmonteAplicado.Desmonte) -  Math.Abs(CreditoAplicado.Credito);
                        CurrentFactura.Aplicado = Math.Round(aplicado, 2);
                    }
                    else
                    {
                        double.TryParse(EditAplicado.Text, out double result);
                        aplicado = result;
                    }

                    RaiseOnPropertyChanged(nameof(CurrentFactura));

                    Args.Aplicado = aplicado;
                    Args.Descuento = CurrentDescuento;
                }
                else
                {
                    //CurrentDescuento = new DescFactura() { DescPorciento = CurrentFactura.DescPorciento, DescuentoValor = CurrentFactura.Descuento, IndicadorItbis = DescConItbis };
                   
                    PorcDescuento = CurrentFactura.DescPorciento;
                              
                    OnPorcDescuentoChanged();
                }

                myRec.UpdateDescuentoConDescuentoInTemp(CurrentFactura.Referencia, DescConItbis);

            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        private void OnPorcDescuentoChanged()
        {
            try
            {
                if (!IsVisible || CurrentFactura == null || /*(DescManualLimite < 0 && !myParametro.GetParRecibosAutorizarDescuento()) ||*/ (ControlSaldo.SelectedSegment == 1 && !myParametro.GetParDescuentoAbonos()))
                {
                    return;
                }                

                if (!CurrentFactura.CalcularDesc && !handlingdesc)
                {
                    Functions.DisplayAlert(AppResource.Warning, AppResource.PressApplyDiscToChooseDiscount, AppResource.Aceptar);
                    return;
                }

                if (ParDigitarPorcDescuento && PorcDescuento > 0)
                {
                    editPorcDescuento.Text = PorcDescuento.ToString();
                }
                else
                {
                    editPorcDescuento.Text = "";
                }

                bool exischdiferidos =  myRec.ExistsChkDiferidos();

                //Se comenta esta parte ya que si existia cheque futurista no calculaba descuento
                //if (!exischdiferidos)
                //{
                //    Args.CalcularDesc = true;
                //}
                Args.CalcularDesc = true;
                CurrentDescuento = exischdiferidos ? new DescFactura() { DescPorciento = CurrentFactura.DescPorciento, DescuentoValor = CurrentFactura.Descuento } : myDesFac.GetMontoDescuentoFactura(PorcDescuento, CurrentFactura);
                CurrentDescuento.DescPorciento = PorcDescuento;
                CreditoAplicado = myRec.GetMontoTotalCreditoAplicadoFactura(CurrentFactura);

                var parDescuentoEnAbonos = myParametro.GetParDescuentoAbonos();

                if (CurrentDescuento.DescPorciento > 0 && Arguments.Values.CurrentModule != Modules.RECONCILIACION)
                {
                    var monto = 0.0;

                    if (ControlSaldo.SelectedSegment == 1 && parDescuentoEnAbonos)
                    {
                        double.TryParse(EditAplicado.Text, out double app);
                        
                        CurrentDescuento.DescuentoValor = Math.Round(myRec.GetMontoDescuentoParaAbono(CurrentFactura, PorcDescuento, app, ControlSaldo.SelectedSegment == 0), 2);
                    }
                    else
                    {
                        if (DescConItbis)
                        {
                            monto = (CurrentFactura.MontoTotal - Math.Abs(CreditoAplicado.Credito));
                        }
                        else
                        {
                            monto = (CurrentFactura.MontoSinItbis - Math.Abs(CreditoAplicado.CreditoSinItbis));
                        }

                        if (monto > 0)
                        {
                            if (parDescuentoEnAbonos)
                            {
                                CurrentDescuento.DescuentoValor = Math.Round(myRec.GetMontoDescuentoParaAbono(CurrentFactura, PorcDescuento, monto, ControlSaldo.SelectedSegment == 0), 2);
                            }
                            else if (CurrentDescuento.DescPorciento > 0)
                            {
                                CurrentDescuento.DescuentoValor = CurrentDescuento.DefDescuento > 0 ? CurrentDescuento.DefDescuento : Math.Round(monto * (CurrentDescuento.DescPorciento / 100), 2);
                            }
                        }
                        else
                        {
                            CurrentDescuento.DescuentoValor = 0;
                            CurrentDescuento.DescPorciento = 0;
                        }   
                    }
                }

                CurrentFactura.Descuento = CurrentDescuento.DescuentoValor;
                CurrentFactura.DescPorciento = CurrentDescuento.DescPorciento;

                if (!handlingdesc)
                {
                    BtnAplicarDescText = AppResource.RemoveDisc;
                }

                double aplicado;

                if (myParametro.GetParDescuentoAbonos() && ControlSaldo.SelectedSegment == 1 && CurrentFactura.Estado != "Pendiente")
                {
                    double.TryParse(EditAplicado.Text, out double result);
                    
                    aplicado = result;
                }
                else
                {
                    aplicado = CurrentFactura.Balance - CurrentDescuento.DescuentoValor - Math.Abs(DesmonteAplicado.Desmonte) -  Math.Abs(CreditoAplicado.Credito);
                    CurrentFactura.Aplicado = Math.Round(aplicado,2);
                }

                RaiseOnPropertyChanged(nameof(CurrentFactura));

                Args.Aplicado = aplicado;
                Args.Descuento = CurrentDescuento;

            }
            catch (Exception e)
            {
                Functions.DisplayAlert(AppResource.ErrorSettingDiscountPercent, e.Message);
            }

            handlingdesc = false;
        }

        private void SaldoControl_OnValueChanged(object sender, int e)
        {
            if (!IsVisible || CurrentFactura == null)
            {
                return;
            }

            if(myParametro.GetParNoAgregarAbonosCKD() && CurrentFactura.Sigla.Contains("CKD"))
            {
               ControlSaldo.IsVisibleAbonosLabel(false);      
            }

            if (myParametro.GetParNoAgregarAbonos())
            {
                ControlSaldo.IsVisibleAbonosLabel(false);
            }

            if (ControlSaldo.SelectedSegment == lastControlSelect)
            {
                return;
            }

            try
            {
                DescConItbis = CurrentFactura.DefIndicadorItbis;

                //EditarDescuento = myParametro.GetParRecibosEditarDescuentoManual();

                var parDescuentoAAbonos = myParametro.GetParDescuentoAbonos();

                if (ControlSaldo.SelectedSegment == 0 || Arguments.Values.CurrentModule == Modules.RECONCILIACION) //saldo
                {
                    lastControlSelect = 0;

                    CheckDescuentoConImpuesto.IsEnabled = true;
                    BtnAutorizarDesc.IsEnabled = true;
                    BtnQuitarDesmonte.IsEnabled = true;
                    BtnAplicarDesc.IsEnabled = true;
                    EditAplicado.IsEnabled = false;
                    EditAplicado.Text = Math.Round(CurrentFactura.Aplicado, 2).ToString();
                    if (!myParametro.GetParQuitarDescuentoVisible())
                    {
                        comboDescuento.IsEnabled = CurrentFactura.CalcularDesc;
                    }
                    editPorcDescuento.IsEnabled = CurrentFactura.CalcularDesc;

                    BtnAplicarDescText = string.IsNullOrWhiteSpace(BtnAplicarDescText) || BtnAplicarDescText == AppResource.RemoveDisc || !CurrentFactura.CalcularDesc ? AppResource.ApplyDiscount : AppResource.RemoveDisc;

                    if (!CurrentFactura.CalcularDesc || Arguments.Values.CurrentModule == Modules.RECONCILIACION)
                    {
                        CurrentFactura.DescPorciento = 0;
                        CurrentFactura.Descuento = 0;
                        CurrentDescuento = new DescFactura() { DescPorciento = 0, DescuentoValor = 0 };
                    } else if (CurrentFactura.DescPorciento == 0 && CurrentFactura.CalcularDesc)
                    {
                        CurrentDescuento = myDesFac.GetMontoDescuentoFactura(CurrentFactura);

                        if (CurrentFactura.Estado == "Saldo")
                        {
                            CurrentDescuento.DescPorciento = CurrentFactura.DescPorciento;
                            CurrentDescuento.DescuentoValor = CurrentFactura.Descuento;
                        }
                        else
                        {
                            if (myParametro.GetParRecibosDescuentoFromAutorizaciones())
                            {
                                    CurrentDescuento = myDesFac.GetMontoDescuentoFactura(myAut.GetPorcientoDescuentoAutorizadoByReferencia(CurrentFactura.Referencia), CurrentFactura);
                                    CurrentFactura.DescPorciento = !ParDigitarPorcDescuento ? CurrentDescuento.DescPorciento : CurrentDescuento.DescPorciento;
                            }
                            else
                            {
                               CurrentFactura.DescPorciento = CurrentDescuento.DescPorciento;
                            }
                        };

                        CurrentFactura.Descuento = CurrentDescuento.DescuentoValor;
                    }
                    else
                    {
                        CurrentDescuento = new DescFactura() { DescPorciento = CurrentFactura.DescPorciento, DescuentoValor = CurrentFactura.Descuento };
                    }

                    if (parDescuentoAAbonos && Arguments.Values.CurrentModule != Modules.RECONCILIACION)
                    {
                        var monto = 0.0;

                        if (DescConItbis)
                        {
                            monto = (CurrentFactura.MontoTotal - Math.Abs(CreditoAplicado.Credito) );
                        }
                        else
                        {
                            monto = (CurrentFactura.MontoSinItbis - Math.Abs(CreditoAplicado.CreditoSinItbis));
                        }

                        CurrentDescuento.DescuentoValor = Math.Round(myRec.GetMontoDescuentoParaAbono(CurrentFactura, PorcDescuento, monto, ControlSaldo.SelectedSegment == 0), 2);
                    }

                    RaiseOnPropertyChanged(nameof(CurrentFactura));

                    double desc = CurrentDescuento.DescuentoValor;

                    if (!CurrentFactura.CalcularDesc)
                    {
                        desc = 0;
                    }
                    double credito = Math.Abs(CurrentFactura.Credito);
                    double desmonte = Math.Abs(DesmonteAplicado.Desmonte);
                    //double cxcNotaCredito = Math.Abs(CurrentFactura.CxcNotaCredito);
                    double balance = CurrentFactura.Balance;

                    double aplicado = balance - desmonte - (desc + credito);
                    CurrentFactura.Aplicado = Math.Round(aplicado, 2);
                    RaiseOnPropertyChanged(nameof(CurrentFactura));

                    Args.Desmonte = desmonte;
                    MontoADesmonte = desmonte;
                    Args.CalcularDesmonte = CurrentFactura.CalcularDesmonte;
                    Args.Descuento = CurrentDescuento;
                    Args.Aplicado = aplicado;

                }
                else //abono
                {
                    lastControlSelect = 1;

                    if (parDescuentoAAbonos && Arguments.Values.CurrentModule != Modules.RECONCILIACION)
                    {
                        CheckDescuentoConImpuesto.IsEnabled = true;
                        BtnAutorizarDesc.IsEnabled = true;
                        BtnAplicarDesc.IsEnabled = true;
                        EditAplicado.IsEnabled = true;
                        //EditAplicado.Text = "";
                        EditAplicado.Focus();
                        if (!myParametro.GetParQuitarDescuentoVisible())
                        {
                            comboDescuento.IsEnabled = CurrentFactura.CalcularDesc;
                        }
                        editPorcDescuento.IsEnabled = CurrentFactura.CalcularDesc;
                    }
                    else
                    {
                        BtnAutorizarDesc.IsEnabled = false;
                        BtnAplicarDesc.IsEnabled = false;
                        BtnQuitarDesmonte.IsEnabled = false;
                        MontoADesmonte = 0;
                        EditAplicado.IsEnabled = true;
                        EditAplicado.Text = "0";
                        EditAplicado.Focus();
                        //  EditarDescuento = false;
                        comboDescuento.IsEnabled = false;
                        editPorcDescuento.IsEnabled = false;
                    }

                    CurrentFactura.Pendiente = CurrentFactura.Balance;

                    if (CurrentFactura.Estado == "Abono")
                    {
                        CurrentFactura.Aplicado = myRec.GetAplicadoInTemp(CurrentFactura.Referencia, CurrentFactura.Documento);
                    }

                    if (parDescuentoAAbonos && double.TryParse(EditAplicado.Text, out double aplicado) && CurrentFactura.CalcularDesc && Arguments.Values.CurrentModule != Modules.RECONCILIACION)
                    {
                        if (CurrentFactura.DescPorciento == 0) {
                            RecibosDocumentosTemp fact = CurrentFactura.Copy();
                            fact.Aplicado = aplicado;
                            CurrentDescuento = myDesFac.GetMontoDescuentoFactura(fact, -1, -1, -1, true);
                        }
                        else
                        {
                            CurrentDescuento.DescuentoValor = Math.Round(myRec.GetMontoDescuentoParaAbono(CurrentFactura, PorcDescuento, aplicado, ControlSaldo.SelectedSegment == 0), 2);
                        }
                    }
                    else
                    {
                        CurrentDescuento.DescuentoValor = 0;
                        CurrentDescuento.DescPorciento = 0;

                        if (CurrentFactura.DefIndicadorItbis)
                        {
                            CheckDescuentoConImpuesto.IsEnabled = false;
                        }

                        CurrentFactura.Descuento = 0;
                        CurrentFactura.DescPorciento = 0;
                    }

                    RaiseOnPropertyChanged(nameof(CurrentFactura));

                    Args.Descuento = CurrentDescuento;

                }
            }
            catch (Exception ex)
            {
                Functions.DisplayAlert(AppResource.Error, ex.Message);
            }
        }

        private async void ShowAlertAutorizacionAbono()
        {
            int RecSecuencia = 0;
            int RecSecuenciaParams = myParametro.GetParRecibosSecuenciaPorSector();
            string AreaCtrlsubto = Arguments.Values.CurrentSector != null && Arguments.Values.CurrentClient != null ? myCli.GetareaCtrlCreditOfClienteDetalle(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentSector.SecCodigo).Substring(0, 2) : "";
            if (RecSecuenciaParams >= 1 && myParametro.GetParRecibosPorSector())
            {
                RecSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Recibos-" + (RecSecuenciaParams == 2 ? AreaCtrlsubto : Arguments.Values.CurrentSector.SecCodigo));
            }
            else if (myParametro.GetParRecibosRecTipoChkDiferidos() && myRec.ExistsChkDiferidos())
            {
                RecSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Recibos-1");
            }
            else
            {
                RecSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Recibos");
            }

            var result = await Functions.DisplayAlert(AppResource.Warning, AppResource.YouCannotApplyCreditsToThisDocument, AppResource.Authorize, AppResource.Cancel);
            
            if (result)
            {
                await Application.Current.MainPage.Navigation.PushModalAsync(new AutorizacionesModal(false, RecSecuencia, 3, null, false, false)
                {
                    OnAutorizacionUsed = (autSec) =>
                    {
                        AbonoAutorizado = true;
                        AttempAccept(null, null);
                    }
                });
                AbonoAutorizado = false;
                isbusy = false;
                return;
            }
            else
            {
                AbonoAutorizado = false;
                isbusy = false;
                return;
            }
        }

        private void LoadData()
        {
            if (!IsVisible)
            {
                return;
            }

            Args = new RecibosDocumentosDetalleArgs();

            if (new DS_CuentasxCobrar().GetCuentaByReferencia(CurrentFactura.Referencia, CurrentFactura.Documento).cxcDesmonte > 0)
            {
                BtnQuitarDesmonte.IsVisible = true;
                EditDescuentoDesmonte.IsVisible = true;
                lblDescDesmonte.IsVisible = true;
                GridQuitarDesmonte.IsVisible = true;
            }
            else
            {
                BtnQuitarDesmonte.IsVisible = false;
                EditDescuentoDesmonte.IsVisible = false;
                lblDescDesmonte.IsVisible = false;
                GridQuitarDesmonte.IsVisible = false;
            }

            DesmonteAplicado.Desmonte = 0;
            if (CurrentFactura.CalcularDesmonte)
            {
                DesmonteAplicado.Desmonte = new DS_CuentasxCobrar().GetCuentaByReferencia(CurrentFactura.Referencia, CurrentFactura.Documento).cxcDesmonte;
            }

            if (myParametro.GetParCalcularDescuentoEnBaseItbis() && CurrentFactura.Dias <= 30)
            {
                lblDescItbis.IsVisible = true;
                CheckDescuentoConImpuesto.IsVisible = true;

                if (CurrentFactura.DescPorciento == 0)
                {
                    DescConItbis = true;
                }
                else
                {
                    DescConItbis = CurrentFactura.DefIndicadorItbis;
                }
            }
            else
            {
                lblDescItbis.IsVisible = false;
                CheckDescuentoConImpuesto.IsVisible = false;
                DescConItbis = CurrentFactura.DefIndicadorItbis;
            }

            CheckDescuentoConImpuesto.IsEnabled = CurrentFactura.DefIndicadorItbis;

            if (!CurrentFactura.CalcularDesc || Arguments.Values.CurrentModule == Modules.RECONCILIACION)
            {
                CurrentDescuento = new DescFactura() { DescPorciento = 0, DescuentoValor = 0 };
            }
            if (CurrentFactura.DescPorciento == 0 && !myRec.ExistsChkDiferidos())
            {
                CurrentDescuento = myDesFac.GetMontoDescuentoFactura(CurrentFactura);
            }
            else
            {
                CurrentDescuento = new DescFactura() { DescPorciento = CurrentFactura.DescPorciento, DescuentoValor = CurrentFactura.Descuento, IndicadorItbis = CurrentFactura.DefIndicadorItbis };
            }

            if ((!CurrentFactura.CalcularDesc || CurrentFactura.Estado != "Saldo") && !myParametro.GetParDescuentoAbonos())
            {
                CurrentDescuento.DescuentoValor = 0;
                CurrentDescuento.DescPorciento = 0;
            }

            BtnAutorizarDesc.IsVisible = myParametro.GetParRecibosAutorizarDescuento() || myParametro.GetParRecibosDescuentoFromAutorizaciones();
            //EditarDescuento = myParametro.GetParRecibosEditarDescuentoManual();

            CreditoAplicado = myRec.GetMontoTotalCreditoAplicadoFactura(CurrentFactura);
            
            PorcDescuento = CurrentDescuento.DescPorciento;

            if(Arguments.Values.CurrentModule == Modules.RECONCILIACION)
            {
                ControlSaldo.SelectedSegment = 0;
            }
            else if (CurrentFactura.Estado == "Abono")
            {
                ControlSaldo.SelectedSegment = 1;
            }
            else if (CurrentFactura.Estado == "Saldo")
            {
                ControlSaldo.SelectedSegment = 0;
            }

            if (ControlSaldo.SelectedSegment == 0)
            {
                EditAplicado.IsEnabled = false;

                if (CurrentFactura.Estado == "Saldo" || myParametro.GetParDescuentoAbonos())
                {
                    if (myParametro.GetParDescuentoManual() >= 0)
                    {
                         CurrentDescuento.DescPorciento = CurrentFactura.DescPorciento;                      
                    }

                    CurrentDescuento = new DescFactura()
                    {
                        DescuentoValor = CurrentFactura.Descuento,
                        DescPorciento = CurrentFactura.DescPorciento
                    };

                    Args.Descuento = CurrentDescuento;
                    Args.Desmonte = Math.Abs(DesmonteAplicado.Desmonte);
                    Args.CalcularDesmonte = CurrentFactura.CalcularDesmonte;
                }
                else
                {
                    CurrentFactura.Descuento = CurrentDescuento.DescuentoValor;

                    RaiseOnPropertyChanged(nameof(CurrentFactura));

                    double aplicado = CurrentFactura.Balance - Math.Abs(DesmonteAplicado.Desmonte) - CurrentDescuento.DescuentoValor - Math.Abs(CreditoAplicado.Credito);

                    Args.Desmonte = Math.Abs(DesmonteAplicado.Desmonte);
                    Args.CalcularDesmonte = CurrentFactura.CalcularDesmonte;
                    Args.Aplicado = aplicado;
                    Args.Descuento = CurrentDescuento;
                }

                SaldoControl_OnValueChanged(null, -1);

            }

            //the very last
            LoadDescuentoFactura();
        }

        private void Dismiss(object sender, EventArgs args)
        {
            if (myParametro.GetParNoAgregarAbonosCKD() && CurrentFactura.Sigla.Contains("CKD"))
            {
                ControlSaldo.IsVisibleAbonosLabel(true);
            }

            OnCancel?.Invoke(this, null);
        }

        private async void AttempAccept(object sender, EventArgs args)
        {
            try
            {
                if (OnAccepted == null)
                {
                    return;
                }

                Args.Factura = CurrentFactura;
                Args.Descuento = CurrentDescuento;
                Args.IsForSaldo = ControlSaldo.SelectedSegment == 0;

                 //if(CurrentFactura.Descuento > 0)
                 //{
                 //   Args.CalcularDesc = true;
                 //}

                var useDescManual = false;
                double porcDescManual = 0.0;

                if ((ControlSaldo.SelectedSegment == 0 || myParametro.GetParDescuentoAbonos()) && Arguments.Values.CurrentModule != Modules.RECONCILIACION)
                {
                    if (ParDigitarPorcDescuento && !string.IsNullOrWhiteSpace(editPorcDescuento.Text) && double.TryParse(editPorcDescuento.Text, out porcDescManual))
                    {
                        var maxPermitido = 0.00;

                        if (PorcientosDescuentos != null && PorcientosDescuentos.Count > 0)
                        {
                            maxPermitido = PorcientosDescuentos.Max();
                        }

                        if (porcDescManual > maxPermitido)
                        {
                            await Functions.DisplayAlert(AppResource.Warning, AppResource.ManualDiscountPercentExceedMaximun + maxPermitido);
                            return;
                        }

                        var desc = myDesFac.GetMontoDescuentoFactura(porcDescManual, CurrentFactura);

                        Args.Descuento.DescuentoValor = desc.DescuentoValor;
                        Args.Descuento.DescPorciento = porcDescManual;                        
                        useDescManual = true;
                    }
                }

                Args.RecVerificarDesc = PorcDescuento > 0;

                if (ControlSaldo.SelectedSegment == 1)
                {
                    if (EditAplicado.Text.Trim().Length == 0)
                    {
                        await Functions.DisplayAlert(AppResource.Warning, AppResource.AppliedCannobBeEmpty);
                        return;
                    }

                    double.TryParse(EditAplicado.Text, out double aplicado);

                    if (aplicado <= 0)
                    {
                        await Functions.DisplayAlert(AppResource.Warning, AppResource.AmountToApplyMustBeGreaterThanZero);
                        return;
                    }

                    var parAceptarExcedente = myParametro.GetParRecibosAplicacionExcedente();
                    if (aplicado > CurrentFactura.Balance && !parAceptarExcedente)
                    {
                        await Functions.DisplayAlert(AppResource.Warning, AppResource.AmountToApplyCannotBeGreaterThanPending);
                        return;
                    }

                    if(parAceptarExcedente && aplicado > CurrentFactura.Balance)
                    {
                        var result = await Functions.DisplayAlert(AppResource.Warning, AppResource.AmountToApplyGreaterThanPendingWannaContinue, AppResource.Yes, AppResource.No);

                        if (!result)
                        {
                            return;
                        }
                    }

                    if (myParametro.GetParDescuentoAbonos())
                    {
                        Args.Descuento.DescuentoValor = myRec.GetMontoDescuentoParaAbono(CurrentFactura, !useDescManual ? PorcDescuento : porcDescManual, aplicado, ControlSaldo.SelectedSegment == 0);
                    }

                    if (myParametro.GetParRecibosSoloAbonoConAutorizacion() == 1 && CurrentFactura.Dias > myParametro.GetParRecibosAbonoDiasPermitidos() && !AbonoAutorizado)
                    {
                        ShowAlertAutorizacionAbono();
                        return;
                    }

                    if (myParametro.GetParRecibosSoloAbonoConAutorizacion() == 2 && !AbonoAutorizado)
                    {
                        ShowAlertAutorizacionAbono();
                        return;
                    }

                    Args.Aplicado = aplicado;
                }

                //AbonoAutorizado = false;

                if (myParametro.GetParNoAgregarAbonosCKD() && CurrentFactura.Sigla.Contains("CKD"))
                {
                    ControlSaldo.IsVisibleAbonosLabel(true);
                }

                OnAccepted?.Invoke(this, Args);
            }catch(Exception e)
            {
                await Functions.DisplayAlert(AppResource.Warning, e.Message);
            }
        }

        private static void CurrentFacturaPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((DialogDetalleRecibo)bindable).CurrentFactura = (RecibosDocumentosTemp)newValue;
        }

        private void LoadDescuentoFactura()
        {
            BtnAplicarDescText = !CurrentFactura.CalcularDesc ? AppResource.ApplyDiscount : AppResource.RemoveDisc;

            if (CurrentFactura.Estado != "Pendiente")
            {
                if (!myParametro.GetParQuitarDescuentoVisible()) 
                {
                    comboDescuento.IsEnabled = CurrentFactura.CalcularDesc;
                }                
                editPorcDescuento.IsEnabled = CurrentFactura.CalcularDesc;
            }

            bool ocultarDescManual = true;

            if (myParametro.GetParRecibosOcultarDescuentoSiNoTiene() && CurrentFactura.CalcularDesc && Arguments.Values.CurrentModule != Modules.RECONCILIACION)
            {
                if (CurrentDescuento.DescuentoValor > 0 || CurrentDescuento.DescPorciento > 0)
                {
                    comboDescuento.IsVisible = true;
                    editPorcDescuento.IsVisible = ParDigitarPorcDescuento;
                    ocultarDescManual = false;
                }

                if (ocultarDescManual)
                {
                    comboDescuento.IsVisible = false;
                    editPorcDescuento.IsVisible = false;
                }

                if (CurrentFactura.Dias <= 30)
                {
                    MontoADescuento = CurrentFactura.MontoTotal;
                }
                else
                {
                    MontoADescuento = myRec.GetMontoADescuentoFactura(CurrentFactura);
                }
            }
            else
            {
                MontoADescuento = myRec.GetMontoADescuentoFactura(CurrentFactura);
                ocultarDescManual = false;
                comboDescuento.IsVisible = true;
                editPorcDescuento.IsVisible = ParDigitarPorcDescuento;
            }

            if(myParametro.GetParQuitarDescuentoVisible())
            {
                BtnAplicarDesc.IsVisible = false;
                comboDescuento.IsEnabled = false;
            }

            MontoADesmonte = Math.Abs(DesmonteAplicado.Desmonte);

            int descManualLimite = myParametro.GetParDescuentoManual();

            if (descManualLimite >= 0 && !ocultarDescManual && Arguments.Values.CurrentModule != Modules.RECONCILIACION)
            {
               // EditarDescuento = true;

                List<double> desc = new List<double>();

                PorcientosDescuentosEx = new List<double>();

                int limiteDesc = descManualLimite + 1;

                if (myParametro.GetParRecibosDescuentoFacturasSemiAutomatico())
                {
                    limiteDesc = (int)CurrentDescuento.DescPorciento;
                }

                for (int i = 0; i < limiteDesc; i++)
                {
                    desc.Add(i);
                    PorcientosDescuentosEx.Add(i);
                }

                PorcientosDescuentos = desc;

                if (!CurrentFactura.CalcularDesc)
                {
                    PorcDescuento = 0;
                }
                else if (PorcientosDescuentos != null && PorcientosDescuentos.Count > 0)
                {
                    //PorcDescuento = PorcientosDescuentos.Where(x => x == CurrentFactura.DescPorciento).FirstOrDefault();
                     PorcDescuento = CurrentFactura.DescPorciento;
                }

                //PorcDescuento = (int)CurrentDescuento.DescPorciento;
            }
            else
            {
               // EditarDescuento = false;

                SetAdapterComboFactura(CurrentFactura.AutSecuencia);
            }

        }

        private void SetAdapterComboFactura(int AutSecuencia)
        {
            var desc = new List<double>();
            var descex = new List<double>();

            if (Arguments.Values.CurrentModule == Modules.RECONCILIACION)
            {
                desc.Add(0);
            }else if (myParametro.GetParRecibosDescuentoFromAutorizaciones())
            {
                try
                {
                    if (AutSecuencia != 0)
                    {
                        desc = myAut.GetPorcientoDescuentoByAutSecuencia(CurrentFactura.AutSecuencia);
                    }
                    else
                    {
                        descex = myDesFac.GetPorcientoDescuentoDisponible(CurrentFactura.Referencia, CurrentFactura.Dias);
                    }
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                }
            }
            else
            {
                if (!myParametro.GetParRecibosAutorizarDescuento() || (myParametro.GetParRecibosAutorizarDescuento() && AutSecuencia == 0))
                {
                    int parDesc = myParametro.GetParRecibosPorcientoDescuentoDisponibleParaFacturas();

                    if (parDesc > 0)
                    {
                        switch (parDesc)
                        {
                            case 1:
                                for (int i = 0; i <= CurrentDescuento.DescPorciento; i++)
                                {
                                    desc.Add(i);
                                    descex.Add(i);
                                }
                                break;
                            case 2:
                                for (int i = (int)CurrentDescuento.DescPorciento; i >= 0; i--)
                                {
                                    desc.Add(i);
                                    descex.Add(i);
                                }
                                break;
                            case 3:
                                var list = SqliteManager.GetInstance().Query<DescuentoFacturas>("select distinct DeFPorciento from DescuentoFacturas where ltrim(rtrim(CXCReferencia)) = ? " +
                                "order by DeFPorciento desc", new string[] { CurrentFactura.Referencia.Trim() });
                                double porciento = 0;

                                if (list != null && list.Count > 0)
                                {
                                    porciento = (double)list.Max(x => x.DeFPorciento);
                                }

                                porciento = CurrentFactura.DescPorciento > porciento ? porciento : CurrentFactura.DescPorciento;
                                for (int i = (int)porciento; i >= 0; i--)
                                {
                                    desc.Add(i);
                                    descex.Add(i);
                                }

                                if ((int)porciento > 0)
                                {
                                    desc = desc.OrderByDescending(x => x).ToList();
                                    descex = descex.OrderByDescending(x => x).ToList();

                                    if (!desc.Contains(myParametro.GetDescuentoManualAdicional()))
                                    {
                                        desc.Add(myParametro.GetDescuentoManualAdicional());
                                        descex.Add(myParametro.GetDescuentoManualAdicional());
                                    }

                                    if (!desc.Contains(myParametro.Get2doDescuentoManualAdicional()))
                                    {
                                        desc.Add(myParametro.Get2doDescuentoManualAdicional());
                                        descex.Add(myParametro.Get2doDescuentoManualAdicional());
                                    }
                                    
                                }
                                break;
                        }
                    }
                    else
                    {
                       // descex = myDesFac.GetPorcientoDescuentoDisponible(CurrentFactura.Referencia, CurrentFactura.Dias);
                        descex = myDesFac.GetPorcientoDescuentoDisponible(CurrentFactura.Referencia, CurrentFactura.DiasChequeDif);
                        //desc = myDesFac.GetPorcientoDescuentoDisponible(CurrentFactura.Referencia, CurrentFactura.Dias);
                        // desc.Add((int)CurrentDescuento.DescPorciento);
                    }
                     
                }
                else
                {

                    try
                    {
                        if (CurrentFactura.AutSecuencia != 0)
                        {
                            loadDescuentoByAutSecuencia(CurrentFactura.AutSecuencia);
                            desc = PorcientosDescuentos;
                            descex = PorcientosDescuentosEx;
                        }

                    }
                    catch (Exception e)
                    {
                        Console.Write(e.Message);
                    }
                }
            }

            PorcientosDescuentos = desc;
            PorcientosDescuentosEx = descex;
            PorcDescuento = descex.Count > 0 && CurrentFactura.CalcularDesc ? descex[0] : 0;
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == nameof(IsVisible) && IsVisible) //si se esta mostrando el dialogo
            {
                LoadData();
            }
        }
    }
}