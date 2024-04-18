
using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Internal;
using MovilBusiness.model;
using MovilBusiness.model.Internal.Structs.Args;
using MovilBusiness.Model.Internal;
using MovilBusiness.Resx;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AgregarFormaPagoModal : ContentPage, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;

        private DS_Monedas myMon;
        public ObservableCollection<IKV> PickerSource { get => CurrentFormaPago == FormasPago.Transferencia ? CuentasSource : CurrentFormaPago == FormasPago.OrdenPago? CooperativasSource: BancosSource; set { RaiseOnPropertyChanged(); comboBancos.SelectedIndex = -1; } }
        private ObservableCollection<IKV> BancosSource, CuentasSource, CooperativasSource;

        private IKV currentpickeritem = null;
        public IKV CurrentPickerItem { get => currentpickeritem; set { currentpickeritem = value; RaiseOnPropertyChanged(); } }

        private int RecSecuencia;
        private int refSecuencia;

        private FormasPago currentformapago;
        public FormasPago CurrentFormaPago { get => currentformapago; set { currentformapago = value; ConfigFormaPago(); RaiseOnPropertyChanged(); } }

        private ObservableCollection<UsosMultiples> pickerTarjeta;
        public ObservableCollection<UsosMultiples> PickerTarjeta { get => pickerTarjeta; set { pickerTarjeta = value; RaiseOnPropertyChanged(); } }

        private ObservableCollection<Denominaciones> bonosource;
        public ObservableCollection<Denominaciones> BonoSource { get => bonosource; set { bonosource = value; RaiseOnPropertyChanged(); } }

        private Denominaciones currentbonoItem;
        public Denominaciones CurrentBonoItem { get => currentbonoItem; set { currentbonoItem = value; RaiseOnPropertyChanged(); } }

        private Denominaciones dendescripcion;
        public Denominaciones DenDescripcion { get => dendescripcion; set { dendescripcion = value; RaiseOnPropertyChanged(); } }

        private UsosMultiples currentpickerTarjeta;
        public UsosMultiples CurrentPickerTarjeta { get => currentpickerTarjeta; set { currentpickerTarjeta = value; RaiseOnPropertyChanged(); } }

        public List<Monedas> Monedas { get; private set; }

        //private string MonCodigoDoc;

        private Monedas currentmoneda;
        public Monedas CurrentMoneda { get => currentmoneda; set { currentmoneda = value; MonSiglaCliente = CurrentMoneda.MonSigla; MontoAPagar = ChangeBalanceCambioMoneda(); Monto = ""; if (CurrentFormaPago == FormasPago.Transferencia) { CargarCuentasBancarias(); } RaiseOnPropertyChanged(); } }

        private bool isfuturista = false;
        public bool IsFuturista { get => isfuturista; set { isfuturista = value; if (CurrentFormaPago != FormasPago.Transferencia) { IsEnabledFecha = value; } if (!value) { pickerFecha.Date = DateTime.Now; } RaiseOnPropertyChanged(); } }

        private bool isEnabledFecha = false;
        public bool IsEnabledFecha { get => isEnabledFecha; set { isEnabledFecha = value; RaiseOnPropertyChanged(); } }

        public DateTime fechafuturista { get; set; }// { get => pickerFecha.Date; }
        public DateTime fechaFuturista { get => fechafuturista; set { fechafuturista = value; RaiseOnPropertyChanged(); } }// { get => pickerFecha.Date; }

        public Action<AgregarFormaPagoArgs> OnAccepted { get; set; }
        public Func<double> FillMonto { get; set; }

        private string monsiglacliente;
        public string MonSiglaCliente { get => monsiglacliente; set { monsiglacliente = value; RaiseOnPropertyChanged(); } }

        private double montoapagar;
        public double MontoAPagar { get => montoapagar; set { montoapagar = value; RaiseOnPropertyChanged(); } }

        public bool IsChkDiferidoGeneral { get; set; }

        private string monto;
        public string Monto { get => monto; set { monto = value; RaiseOnPropertyChanged(); } }

        private double montobase;
        public double montoBase { get => montobase; set { montobase = value; RaiseOnPropertyChanged(); } }

        public bool ShowMoneda { get; private set; }

        public bool ShowTarjeta;

        private DS_Recibos myRec;
        private Monedas MonedaDocumentos;
        private DS_UsosMultiples usos;
        private DS_TransaccionesImagenes TransaccionesImagenes;
        private DS_RepresentantesParametros myparms;
        
        public AgregarFormaPagoModal (DS_Recibos myRec, Monedas monedaDeLosDocumentos = null)
		{
            myMon = new DS_Monedas();
            usos = new DS_UsosMultiples();
            myparms = DS_RepresentantesParametros.GetInstance();

            this.myRec = myRec;
            if (monedaDeLosDocumentos != null)
            {
                MonedaDocumentos = monedaDeLosDocumentos;
                //MonCodigoDoc = monedaDeLosDocumentos.MonCodigo;
            }

            ShowMoneda = DS_RepresentantesParametros.GetInstance().GetParRecibosPuedeCobrarEnVariasMonedas() && MonedaDocumentos != null;
            PickerTarjeta = new ObservableCollection<UsosMultiples>(usos.GetTiposTarjeta());
            var dsMon = new DS_Monedas();

            if (ShowMoneda)
            {   
                if (myparms.GetParRecibosusarMonedaUnica())
                {
                    Monedas = new List<Monedas>()
                    {
                        MonedaDocumentos
                    };
                }
                else
                {
                    var monedasValidas = DS_RepresentantesParametros.GetInstance().GetParRecibosFormaPagoMonedaUnica();
                    if (monedasValidas != null && monedasValidas.Length == 2 && MonedaDocumentos.MonCodigo == monedasValidas[0].ToString())
                    {
                        Monedas = dsMon.GetMonedas(monedasValidas[1].ToString(), MonedaDocumentos);
                    }
                    else
                    {
                        Monedas = dsMon.GetMonedas(null, MonedaDocumentos);
                    }
                    
                }
                if (Monedas != null && Monedas.Count > 0)
                {
                    var item = Monedas.Where(x => x.MonCodigo.Trim().ToUpper() == MonedaDocumentos.MonCodigo.Trim().ToUpper()).FirstOrDefault();

                    if(item != null)
                    {
                        CurrentMoneda = item;
                    }
                    else
                    {
                        CurrentMoneda = Monedas[0];
                    }
                }
            }

            MonSiglaCliente = MonedaDocumentos != null ? MonedaDocumentos.MonSigla : "";

            BindingContext = this;

            InitializeComponent ();

            pickerFecha.MinimumDate = DateTime.Now;

            /*Se agrega un datetime.now porque en ordenes de pago traia 
             fecha por defecto ocasionando error en calculo de total*/
            fechaFuturista = DateTime.Now;

            pickerFecha.DateSelected += PickerFecha_DateSelected;           
            
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (CurrentFormaPago == FormasPago.Transferencia) { IsEnabledFecha = true; }

            if (FirstTime)
            {
                FirstTime = false;
                //ConfigChkDiferidoGeneral();
            }
        }

        public bool FirstTime { get; set; } = true;
        //private void ConfigChkDiferidoGeneral()
        //{
            //if (IsChkDiferidoGeneral)
            //{
            //    IsFuturista = IsChkDiferidoGeneral;
            //    pickerFecha.IsEnabled = false;
            //    checkDiferido.IsEnabled = false;
            //    pickerFecha.Date = fechaFuturista;
            //}
            //else
            //{
            //    IsFuturista = false;
            //    pickerFecha.IsEnabled = true;
            //    checkDiferido.IsEnabled = true;
            //    pickerFecha.Date = DateTime.Now;
            //    fechaFuturista = pickerFecha.Date;
            //}
        //}

        private void PickerFecha_DateSelected(object sender, DateChangedEventArgs e)
        {
            fechaFuturista = e.NewDate;

            var monto = FillMonto?.Invoke();

            if ((ShowMoneda && CurrentMoneda != null && MonedaDocumentos != null && CurrentMoneda.MonTasa != MonedaDocumentos.MonTasa && !DS_RepresentantesParametros.GetInstance().GetParRecibosTasaFacturas_O_TasaDia())
                || (DS_RepresentantesParametros.GetInstance().GetParRecibosTasaFacturas_O_TasaDia() && CurrentMoneda.MonCodigo != MonedaDocumentos.MonCodigo))
            {
                var montoMonedaBase = monto * MonedaDocumentos.MonTasa;

                var montoCalculado = montoMonedaBase / CurrentMoneda.MonTasa;
                //editMonto.Text = montoCalculado.ToString();
                MontoAPagar = montoCalculado * -1 ?? MontoAPagar;
                return;
            }

            //editMonto.Text = monto?.ToString("N2");
            MontoAPagar = monto * -1 ?? MontoAPagar;
        }

        private void FillMontoFormaPago(object sender, EventArgs args)
        {
            var monto = FillMonto?.Invoke();

            if ((ShowMoneda && CurrentMoneda != null && MonedaDocumentos != null && CurrentMoneda.MonTasa != MonedaDocumentos.MonTasa && !DS_RepresentantesParametros.GetInstance().GetParRecibosTasaFacturas_O_TasaDia()) 
                || (DS_RepresentantesParametros.GetInstance().GetParRecibosTasaFacturas_O_TasaDia() && CurrentMoneda.MonCodigo != MonedaDocumentos.MonCodigo))
            {

                var montoMonedaBase = monto * MonedaDocumentos.MonTasa;

                var montoCalculado = montoMonedaBase / CurrentMoneda.MonTasa;
                //editMonto.Text = montoCalculado.ToString();
                double.TryParse(montoCalculado.ToString(), out double montovalor);
                montoBase = montovalor;
                double montoRedondeado = Math.Round(montovalor, 2);
                Monto = montoRedondeado.ToString();//montoCalculado.ToString();
                return;
            }

            //editMonto.Text = monto?.ToString("N2");
            Monto = monto.ToString();
        }

        public void Dismiss(object sender = null, EventArgs args = null)
        {
            Navigation.PopAsync(true);
        }

        private async void AgregarFormaPago(bool authorized = false, int autSecuencia = -1)
        {
            if (ShowMoneda && CurrentMoneda == null)
            {
                await DisplayAlert(AppResource.Warning, AppResource.ChooseCurrencyWarning, AppResource.Aceptar);
                return;
            }

            if(ShowTarjeta && CurrentPickerTarjeta == null)
            {
                await DisplayAlert(AppResource.Warning, AppResource.SelectTypeOfCard, AppResource.Aceptar);
                return;
            }

            if (ShowTarjeta  && DS_RepresentantesParametros.GetInstance().GetParTarjetaSoloPesosDominicano() && CurrentMoneda != null )
            {
                if  (CurrentMoneda.MonCodigo != "RD$" && CurrentMoneda.MonCodigo != "DOP")
                {
                    await DisplayAlert(AppResource.Warning, AppResource.PaymentOnlyInDRP, AppResource.Aceptar);
                    return;
                }
            }

            if (DS_RepresentantesParametros.GetInstance().GetParTasaMonedas())
            {
                if (CurrentMoneda != null)
                {
                    var mon = myMon.GetMonedaByMonCod(CurrentMoneda.MonCodigo);

                    TimeSpan time = mon.MonFechaActualizacion - DateTime.Now;

                    if (time.Hours < -24)
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.RateExceed24HoursWarning, AppResource.Aceptar);
                        return;
                    }
                }
            }
            if(myparms.GetParBonoToPickers())
            {
                Monto = CurrentBonoItem != null ? CurrentBonoItem.DenValor.ToString() : Monto;
            }
           
            double.TryParse(Monto, out double monto);
            bool canttovalue = int.TryParse(editCantidad.Text, out int cantidad);

            if (CurrentFormaPago == FormasPago.Bono && canttovalue && cantidad > 0)
            {
                monto *= cantidad;
            }

            if (monto <= 0)
            {
                await DisplayAlert(AppResource.Warning, AppResource.AmountMustBeGreaterThanZero, AppResource.Aceptar);
                return;
            }

            //double.TryParse(editMonto.Text, out double monto);
            double prima = monto;

            if (ShowMoneda && CurrentMoneda.MonCodigo.Trim().ToUpper() != MonedaDocumentos.MonCodigo.Trim().ToUpper() && CurrentMoneda.MonTasa != MonedaDocumentos.MonTasa)
            {
               
                var montoMonedaBase = montoBase * CurrentMoneda.MonTasa;
                var montoCalculado = montoMonedaBase / MonedaDocumentos.MonTasa;

                prima = montoCalculado;

                if (double.TryParse(prima.ToString("N2"), out double result))
                {
                    prima = result;
                }

                if (double.TryParse(monto.ToString("N2"), out double montoRedondeado))
                {
                    monto = montoRedondeado;
                    montoMonedaBase = (montoBase == monto || Math.Round(montoBase,2) == monto ? montoBase : monto) * CurrentMoneda.MonTasa;
                    montoCalculado = Math.Round(montoMonedaBase / MonedaDocumentos.MonTasa, 2);

                    if (prima != montoCalculado)
                    {
                        prima = montoCalculado;
                    }
                }
            }

            var aplicado = SqliteManager.GetInstance().Query<FormasPagoTemp>("select ifnull(SUM(cast(Aplicado as real)), 0.0) as Valor, ifnull(SUM(case when Estado = 'Saldo' then cast(Descuento as real) else 0.0 end), 0.0) as Tasa from RecibosDocumentosTemp where Origen = '1'", new string[] { });

            string calmotsob = DS_RepresentantesParametros.GetInstance().GetParRecibosMontoSobrantes();

            if (!string.IsNullOrEmpty(calmotsob))
            {
                if (aplicado[0].Valor > 0)
                {
                    double totdif = prima - aplicado[0].Valor;

                    if (calmotsob.Contains("%"))
                    {
                        calmotsob = calmotsob.Substring(0, calmotsob.Length - 1);
                        var obtporcent = (double.Parse(calmotsob) / 100) * prima;

                        if (totdif > obtporcent)
                        {
                            await DisplayAlert(AppResource.Warning, AppResource.ExceedRemainingAmountOfReceiptWarning + calmotsob + "%", AppResource.Aceptar);
                            return;
                        }
                    }
                    else
                    {
                        var obtporcent = (double.Parse(calmotsob));
                        if (totdif > obtporcent)
                        {
                            await DisplayAlert(AppResource.Warning, AppResource.ExceedRemainingAmountOfReceiptWarning + calmotsob, AppResource.Aceptar);
                            return;
                        }
                    }

                }

            }

            string numeroTarjeta = !string.IsNullOrWhiteSpace(editTarjeta.Text) ? editTarjeta.Text.Replace(" ", "") : !string.IsNullOrWhiteSpace(editTarjetaUlt.Text) ? editTarjetaUlt.Text.Replace(" ", "") : "";
            var forma = new AgregarFormaPagoArgs
            {
                Valor = monto,
                // NoCheque = editNumero.Text,
                NoCheque = CurrentFormaPago == FormasPago.TarjetaCredito ? numeroTarjeta : editNumero.Text,
                Futurista = IsFuturista,
                Fecha = pickerFecha.Date,
                Banco = CurrentPickerItem?.GetValue(),
                BanID = CurrentPickerItem?.GetKey(),
                Moneda = CurrentMoneda,
                Prima = prima,
                DenId = (CurrentBonoItem != null ? CurrentBonoItem.DenID : 0),
                PusCantidad = cantidad,
               //TipTarjeta = CurrentPickerTarjeta != null ? CurrentPickerTarjeta.Descripcion : null,
               TipTarjeta = CurrentPickerTarjeta != null ? CurrentPickerTarjeta.CodigoUso : "",
            };

            if(CurrentFormaPago == FormasPago.Bono)
            {
                int.TryParse(editCantidad.Text, out int cantidadbono);
                double.TryParse(Monto, out double montoBono);
                forma.BonoCantidad = cantidadbono;
                forma.BonoDenominacion = montoBono;
            }

            if (authorized)
            {
                forma.AutSecuencia = autSecuencia;
            }

            if (CurrentFormaPago == FormasPago.Efectivo || CurrentFormaPago == FormasPago.Retencion || CurrentFormaPago == FormasPago.Bono)
            {
                forma.Banco = "";
                forma.BanID = "";
            }

            if (await ValidarValores(forma))
            {
                if (DS_RepresentantesParametros.GetInstance().GetParRecibosAutorizarPagoTransferencia() && !authorized && CurrentFormaPago == FormasPago.Transferencia)
                {
                    var result = await DisplayAlert(AppResource.Warning, AppResource.AuthorizeTransferQuestion, AppResource.Authorize, AppResource.Cancel);

                    if (!result)
                    {
                        return;
                    }

                    var dialog = new AutorizacionesModal(false, myRec.GetNextSecuenciaRecibos(), 3, null)
                    {
                        OnAutorizacionUsed = (aut) =>
                        {
                            AgregarFormaPago(true, aut);
                        }
                    };

                    await Navigation.PushModalAsync(dialog);
                    return;
                }

                OnAccepted?.Invoke(forma);
            }
        }

        private void AddFormaPago(object sender, EventArgs args)
        {
            AgregarFormaPago();
        }

        private async Task<bool> ValidarValores(AgregarFormaPagoArgs forma)
        {
            var myParametro = DS_RepresentantesParametros.GetInstance();
            int forIDAgregada = 0;
            string formaPagoName = "";

            switch (CurrentFormaPago)
            {
                case FormasPago.Efectivo:
                    forIDAgregada = 1;
                    formaPagoName = "Efectivo";
                    break;
                case FormasPago.Cheque:
                    forIDAgregada = 2;
                    formaPagoName = "Cheque";
                    break;
                case FormasPago.Transferencia:
                    forIDAgregada = 4;
                    formaPagoName = "Transferencia";
                    break;
                case FormasPago.Retencion:
                    forIDAgregada = 5;
                    formaPagoName = "Retencion";
                    break;
                case FormasPago.TarjetaCredito:
                    forIDAgregada = 6;
                    formaPagoName = "Tarjeta de Credito";
                    break;
                case FormasPago.DiferenciaCambiaria:
                    forIDAgregada = 8;
                    formaPagoName = "Diferencia Cambiaria";
                    break;
                case FormasPago.Redondeo:
                    forIDAgregada = 9;
                    formaPagoName = "Redondeo";
                    break;
                case FormasPago.OrdenPago:
                    forIDAgregada = 18;
                    formaPagoName = "Orden de Pago";
                    break;
                case FormasPago.Bono:
                    forIDAgregada = 20;
                    formaPagoName = "Bono";
                    break;
            }
             
            int fopCantidadPermitida=  new DS_FormasPago().GetFormasPagoCantidadPermitida(forIDAgregada);
            if (myRec.ExistsMaximoFormaDePago(forIDAgregada, forma.Moneda != null ? forma.Moneda.MonCodigo : "", fopCantidadPermitida))
            {
                await DisplayAlert(AppResource.Warning, AppResource.YouCannotAddMoreThanPaymentMethod.Replace("@1", fopCantidadPermitida.ToString()).Replace("@2", formaPagoName.ToString()), AppResource.Aceptar);
                return false;
            }

            bool useBanco = CurrentFormaPago == FormasPago.Cheque || CurrentFormaPago == FormasPago.Transferencia || currentformapago == FormasPago.OrdenPago;
            if ((useBanco || CurrentFormaPago == FormasPago.TarjetaCredito) && forma.BanID == null)
            {
                // DisplayAlert("Aviso", ("Debe de elegir " + (CurrentFormaPago == FormasPago.Transferencia ? "el banco" : "la cuenta bancaria")), AppResource.Aceptar);
                await DisplayAlert(AppResource.Warning, CurrentFormaPago == FormasPago.OrdenPago ? AppResource.MustChooseCooperativeWarning : AppResource.MustChooseBankWarning, AppResource.Aceptar);
                return false;
            }

            if (useBanco && forma.Futurista && forma.Fecha.ToString("yyyy-MM-dd") == DateTime.Now.ToString("yyyy-MM-dd") && currentformapago != FormasPago.OrdenPago)
            {
                await DisplayAlert(AppResource.Warning, CurrentFormaPago == FormasPago.Cheque ? AppResource.DeferredCheckDateOlderThanToday : AppResource.TransferDateOlderThanToday, AppResource.Aceptar);
                return false;
            }

            if (myParametro.GetParRecibosPorcientoLimiteDiferenciaCambiaria() > 0 && CurrentFormaPago == FormasPago.DiferenciaCambiaria)
            {
                double total = myRec.GetMontoTotal();
                double PorcentajeDiferencia = myParametro.GetParRecibosPorcientoLimiteDiferenciaCambiaria();
                double MontoDiferencia = forma.Valor;
                double DiferenciaPermitida = (Math.Abs(total) * PorcentajeDiferencia) / 100;
                MontoDiferencia = Math.Round(MontoDiferencia, 2);
                DiferenciaPermitida = Math.Round(DiferenciaPermitida, 2);

                if ( MontoDiferencia > DiferenciaPermitida)
                {
                    await DisplayAlert(AppResource.Warning, "La diferencia cambiaria digitada sobrepasa el limite establecido: " + DiferenciaPermitida.ToString(), AppResource.Aceptar);
                    return false;
                }

            }

            if (myParametro.GetParRecibosLimiteRedondeo() > 0 && CurrentFormaPago == FormasPago.Redondeo)
            {
                double limiteRedondeo = myParametro.GetParRecibosLimiteRedondeo();
                double valor = forma.Valor;

                if (valor > limiteRedondeo)
                {
                    await DisplayAlert(AppResource.Warning, "El redondeo digitado sobrepasa el limite establecido: " + limiteRedondeo.ToString(), AppResource.Aceptar);
                    return false;
                }
            }

            if (myparms.GetParNoValidarMontoCeroConBonoToPickers())
            {
                if (forma.Valor <= 0 && !myparms.GetParBonoToPickers())
                {
                    await DisplayAlert(AppResource.Warning, AppResource.AmountMustBeGreaterThanZero, AppResource.Aceptar);
                    return false;
                }
            }
            else
            {
                if (forma.Valor <= 0)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.AmountMustBeGreaterThanZero, AppResource.Aceptar);
                    return false;
                }
            }

            if (myRec.ExistsFormaDePago("Orden pago"))
            {
                await DisplayAlert(AppResource.Warning, AppResource.CannotAddAnotherFormWithPaymentOrder, AppResource.Aceptar);
                return false;
            }
            if (myParametro.GetParRecNoAgregarVariasFormPagTrans() && myRec.ExistsFormaDePago("Transferencia"))
            {
                await DisplayAlert(AppResource.Warning, AppResource.CannotAddAnotherFormWithTrans, AppResource.Aceptar);
                return false;
            }

            if(ShowMoneda && CurrentMoneda != null && myRec.ExistsFormaDePagoMonedaDiferente(CurrentMoneda.MonCodigo))
            {
                await DisplayAlert(AppResource.Warning, AppResource.CannotPaymentWithDifferentCurrencies, AppResource.Aceptar);
                return false;
            }

            if (currentformapago == FormasPago.OrdenPago && myRec.IsFormasPagoInTemp())
            {
                await DisplayAlert(AppResource.Warning, AppResource.CannotAddAnotherFormWithPaymentOrder, AppResource.Aceptar);
                return false;
            }

            if (myParametro.GetParRecibosSoloUnaFormaDePago() && myRec.IsFormasPagoInTemp())
            {
                await DisplayAlert(AppResource.Warning, AppResource.CannotAddMoreThanOnePayment, AppResource.Aceptar);
                return false;
            }

            if ((!forma.Futurista && currentformapago != FormasPago.Redondeo && currentformapago != FormasPago.DiferenciaCambiaria) && myRec.ExistsChkDiferidos())
            {
                await DisplayAlert(AppResource.Warning, AppResource.CannotAddAnotherFormWithDeferredCheck, AppResource.Aceptar);
                return false;
            }

            if ((myRec.ExistsFormaPago() && myParametro.GetParCobrosEfectivoSolo() && CurrentFormaPago == FormasPago.Efectivo)
                || (myRec.ExistsFormaEfectivo() && myParametro.GetParCobrosEfectivoSolo()))
            {
                await DisplayAlert(AppResource.Warning, AppResource.CannotAddAnotherFormWithCash, AppResource.Aceptar);
                return false;
            }

            long.TryParse(forma.NoCheque, out long numero);

            if ((CurrentFormaPago == FormasPago.Cheque || CurrentFormaPago == FormasPago.Transferencia) && (string.IsNullOrWhiteSpace(forma.NoCheque) || !(numero > 0)) || (CurrentFormaPago == FormasPago.TarjetaCredito && (numero.ToString().Substring(0,1) == "3" ? numero.ToString().Length != 15 : numero.ToString().Length != 16) && !DS_RepresentantesParametros.GetInstance().GetParDigitaUlt4DigitosTarjeta()) ||  (CurrentFormaPago == FormasPago.TarjetaCredito && numero.ToString().Length != 4 && DS_RepresentantesParametros.GetInstance().GetParDigitaUlt4DigitosTarjeta()))
            {
                await DisplayAlert(AppResource.Warning, "El número " + (CurrentFormaPago == FormasPago.Cheque ? "del cheque" : (CurrentFormaPago == FormasPago.Transferencia ? "de la transferencia" : "de la tarjeta")) + " no es valido", AppResource.Aceptar);
                return false;
            }

            if(CurrentFormaPago == FormasPago.Bono && (!int.TryParse(editCantidad.Text, out int cantidadBonos) || !(cantidadBonos > 0)))
            {
                await DisplayAlert(AppResource.Warning, AppResource.MustSpecifiedBonusAmount, AppResource.Aceptar);
                return false;
            }

            if (CurrentFormaPago == FormasPago.Cheque)
            {
                if (myRec.ExistsChk(forma.Banco, forma.NoCheque))
                {
                    await DisplayAlert(AppResource.Warning, AppResource.CheckNumberRepeatedWarning, AppResource.Aceptar);
                    return false;
                }

                if (forma.Futurista && myRec.ExistsFormasDePagoDiferentesAChkDiferidos())
                {
                    await DisplayAlert(AppResource.Warning, AppResource.CannotAddAnotherFormWithDeferredCheck, AppResource.Aceptar);
                    return false;
                }

                if (forma.Futurista && myRec.ExistsChkDiferidos(false, forma.Fecha.ToString("yyyy-MM-dd HH:mm:ss")))
                {
                    await DisplayAlert(AppResource.Warning, AppResource.CannotAddDeferredCheckWithDifferentDates, AppResource.Aceptar);
                    return false;
                }
            }

            if (CurrentFormaPago == FormasPago.Efectivo && myRec.ExistsFormaDePago("Efectivo", forma.Moneda != null ? forma.Moneda.MonCodigo : ""))
            {
                await DisplayAlert(AppResource.Warning, AppResource.CannotAddMoreThanOneCash, AppResource.Aceptar);
                return false;
            }

            if (CurrentFormaPago == FormasPago.Transferencia && myRec.ExistsFormaPagoTransferencia(forma.Banco, forma.NoCheque))
            {
                await DisplayAlert(AppResource.Warning, AppResource.TransferNumberRepeated, AppResource.Aceptar);
                return false;
            }

            if (myParametro.GetParCobrosTarjetaCreditoSolo())
            {
                if ((CurrentFormaPago == FormasPago.TarjetaCredito && myRec.ExistsFormasDePagoDiferentesATarjetaCredito())
                    || (CurrentFormaPago != FormasPago.TarjetaCredito && myRec.ExistsFormaTarjetaCredito()))
                {
                    await DisplayAlert(AppResource.Warning, AppResource.CannotAddAnotherFormWithCreditCard, AppResource.Aceptar);
                    return false;
                }
            }

            if ((myParametro.GetParRecibosFotoChequeObligatorio() && CurrentFormaPago == FormasPago.Cheque)
                || (myParametro.GetParFotoParaTransferenciaBancarias() == 2 && CurrentFormaPago == FormasPago.Transferencia))
            {
                TransaccionesImagenes= new DS_TransaccionesImagenes();

                RecSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Recibos");
                refSecuencia = myRec.GetLastRefSecuenciaInTemp() + 1;

                var cantidad = TransaccionesImagenes.GetCantidadImagenesInTemp("RecibosFormaPago", RecSecuencia.ToString() + "|" + refSecuencia.ToString());
                if ((CurrentFormaPago == FormasPago.Cheque || CurrentFormaPago == FormasPago.Transferencia) && cantidad == 0)
                {
                    var msg = CurrentFormaPago == FormasPago.Cheque ? AppResource.CannotAddCheckWithoutPicture : AppResource.CannotAddTransfersWithoutPicture;

                    await DisplayAlert(AppResource.Warning, msg, AppResource.Aceptar);
                    return false;
                }
                RecSecuencia = 0;
                refSecuencia = 0;
            }

            if(CurrentFormaPago == FormasPago.Cheque)
            {
                var parLimiteChequeDiferidos = myParametro.GetParChequesDiferidosLimiteDias();

                if (forma.Futurista && parLimiteChequeDiferidos != null && (forma.Fecha.Date - DateTime.Now.Date).Days > parLimiteChequeDiferidos.DiasLimite)
                {
                    if (parLimiteChequeDiferidos.Autorizar)
                    {
                        var autorizar = await DisplayAlert(AppResource.Warning, AppResource.DateCheckIsGreaterThanLimit.Replace("@", parLimiteChequeDiferidos.DiasLimite.ToString()) + AppResource.WantAuthorizeAppend, AppResource.Authorize, AppResource.Cancel);
                        if (autorizar)
                        {
                            var modal = new AutorizacionesModal(false, myRec.GetNextSecuenciaRecibos(), 3, null);
                            modal.OnAutorizacionUsed = (autSecuencia) => 
                            {
                                OnAccepted?.Invoke(forma);
                            };
                            await Navigation.PushModalAsync(modal);
                        }
                    }
                    else
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.DateCheckIsGreaterThanLimit.Replace("@", parLimiteChequeDiferidos.DiasLimite.ToString()), AppResource.Aceptar);
                    }
                    return false;
                }
            }

            return true;
        }

        private void GoPhoto(object sender, EventArgs args)
        {
            try
            {
                RecSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Recibos");

                refSecuencia = myRec.GetLastRefSecuenciaInTemp() + 1;

                Navigation.PushAsync(new CameraPage(RecSecuencia + "|" + refSecuencia, "RecibosFormaPago"));

            }catch(Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }
        }

        private void ConfigFormaPago()
        {
            if (CurrentFormaPago == FormasPago.Null)
            {
                //editMonto.Text = "";
                Monto = null;
                editNumero.Text = "";
                editCantidad.Text = "";
                IsFuturista = false;
                IsEnabledFecha = false;
                checkDiferido.IsEnabled = true;
                comboBancos.SelectedIndex = 0;
                pickerFecha.Date = DateTime.Now;
                btnFoto.IsVisible = false;
                lblCantidad.IsVisible = false;
                editCantidad.IsVisible = false;
                lblMonto.Text = AppResource.AmountLabel;
                fillIcon.IsVisible = true;
                editTarjeta.Text = "";
                editTarjeta.IsVisible = false;
                editTarjetaUlt.Text = "";
                editTarjetaUlt.IsVisible = false;
                labelTarjetaUlt.IsVisible = false;
                labelTarjeta.IsVisible = false;
                return;
            }

            //imgFoto.IsVisible = false;
            stackgeneric.IsVisible = true;
            labelNumero.IsVisible = false;
            labelTarjetaUlt.IsVisible = false;
            labelTarjeta.IsVisible = false;
            comboBonos.IsVisible = false;
            editNumero.IsVisible = false;
            editTarjeta.IsVisible = false;
            editTarjetaUlt.IsVisible = false;
            layoutDiferido.IsVisible = false;
            pickerFecha.IsVisible = false;
            lblBanco.IsVisible = false;
            lblTarjeta.IsVisible = false;
            pickertarjeta.IsVisible = false;
            ShowTarjeta = false;
            comboBancos.IsVisible = false;
            btnFoto.IsVisible = false;
            //editMonto.Text = "";
            Monto = null;
            editNumero.Text = "";
            editCantidad.Text = "";
            editTarjeta.Text = "";
            editTarjetaUlt.Text = "";
            IsFuturista = false;
            checkDiferido.IsEnabled = true;
            comboBancos.SelectedIndex = 0;
            pickerFecha.Date = DateTime.Now;               
            lblCantidad.IsVisible = false;
            editCantidad.IsVisible = false;
            lblMonto.Text = AppResource.AmountLabel;
            fillIcon.IsVisible = true;
            //editNumero.MaxLength = 10;

            switch (CurrentFormaPago)
            {
                case FormasPago.Cheque:
                    labelTitle.Text = AppResource.CheckUpper;
                    //imgFoto.IsVisible = true;
                    labelNumero.IsVisible = true;
                    lblBanco.IsVisible = true;
                    editNumero.IsVisible = true;
                    //editNumero.MaxLength = 10;
                    layoutDiferido.IsVisible = true;
                    pickerFecha.IsVisible = true;
                    lblBanco.Text = AppResource.BankLabel;
                    comboBancos.IsVisible = true;
                    btnFoto.IsVisible = true;
                    pickerFecha.MinimumDate = DateTime.Now;
                    pickerFecha.MaximumDate = DateTime.MaxValue;
                    if (BancosSource == null)
                    {
                        BancosSource = new ObservableCollection<IKV>(new DS_Bancos().GetBancos());
                    }
                    PickerSource = BancosSource;

                    //var parLimiteNumeroCheque = DS_RepresentantesParametros.GetInstance().GetParNumeroChequesLongitudMaxima();

                    //if(parLimiteNumeroCheque > 0)
                    //{
                    //    editNumero.MaxLength = parLimiteNumeroCheque <= 10 ? parLimiteNumeroCheque : 10;
                    //}

                    break;
                case FormasPago.Efectivo:
                    labelTitle.Text = AppResource.Cash.ToUpper();
                    break;
                case FormasPago.Bono:
                    labelTitle.Text = AppResource.Bonus.ToUpper();
                    lblCantidad.IsVisible = true;
                    editCantidad.IsVisible = true;
                    fillIcon.IsVisible = false;
                    lblMonto.Text = AppResource.DenominationLabel;
                    if (myparms.GetParBonoToPickers())
                    {
                        BonoSource = new ObservableCollection<Denominaciones>(new DS_Denominaciones().GetDenominacionesByDenTipo(3));
                        stackgeneric.IsVisible = false;
                        comboBonos.IsVisible = true;
                    }
                    break;
                case FormasPago.OrdenPago:
                    labelTitle.Text = AppResource.PaymentOrder.ToUpper();                  
                    labelNumero.IsVisible = true;
                    editNumero.IsVisible = true;
                    editNumero.MaxLength = 10;
                    lblBanco.IsVisible = true;
                    lblBanco.Text = "COOP: ";
                    pickerFecha.IsVisible = true;
                    pickerFecha.MaximumDate = DateTime.MaxValue;
                    comboBancos.IsVisible = true;
                    IsFuturista = true;
                    if (CooperativasSource == null)
                    {
                        CooperativasSource = new ObservableCollection<IKV>(new DS_Bancos().GetBancosOrdenPago());
                    }
                    PickerSource = CooperativasSource;
                    break;
                case FormasPago.Retencion:
                    labelTitle.Text = AppResource.Retention;
                    break;
                case FormasPago.TarjetaCredito:
                    labelTitle.Text = AppResource.CreditCard.ToUpper();
                    if (DS_RepresentantesParametros.GetInstance().GetParDigitaUlt4DigitosTarjeta())
                    {
                        labelTarjetaUlt.IsVisible = true;
                        editTarjetaUlt.IsVisible = true;
                        labelTarjeta.IsVisible = false;
                        editTarjeta.IsVisible = false;
                    }
                    else
                    {
                        labelTarjetaUlt.IsVisible = false;
                        editTarjetaUlt.IsVisible = false;
                        labelTarjeta.IsVisible = true;
                        editTarjeta.IsVisible = true;
                    }
                    lblBanco.IsVisible = true;
                    lblBanco.Text = AppResource.BankLabel;
                    comboBancos.IsVisible = true;
                    lblTarjeta.IsVisible = true;
                    pickertarjeta.IsVisible = true;
                    ShowTarjeta = true;
                    pickerFecha.IsVisible = true;
                    if (BancosSource == null)
                    {
                        BancosSource = new ObservableCollection<IKV>(new DS_Bancos().GetBancos());
                    }
                    PickerSource = BancosSource;
                    //editNumero.MaxLength = 16;
                    break;
                case FormasPago.Transferencia:
                    labelTitle.Text = AppResource.BankTransfer.ToUpper();
                    labelNumero.IsVisible = true;
                    editNumero.IsVisible = true;
                    //editNumero.MaxLength = 10;
                    lblBanco.IsVisible = true;
                    lblBanco.Text = AppResource.AccountLabel;
                    comboBancos.IsVisible = true;
                    pickerFecha.IsVisible = true;
                    pickerFecha.MinimumDate = Convert.ToDateTime("1900-01-01");
                    pickerFecha.MaximumDate = DateTime.Now;

                    var parFoto = myparms.GetParFotoParaTransferenciaBancarias();
                    if (parFoto == 1 || parFoto == 2)
                    {
                        btnFoto.IsVisible = true;
                    }

                    CargarCuentasBancarias();
                    break;
                case FormasPago.DiferenciaCambiaria:
                    labelTitle.Text = AppResource.ExchangeDifferent.ToUpper();
                    PickerSource = null;
                    break;
                case FormasPago.Redondeo:
                    labelTitle.Text = AppResource.Rounding.ToUpper();
                    PickerSource = null;
                    break;
            }

            if(Arguments.Values.CurrentModule == Modules.COMPRAS)
            {
                btnFoto.IsVisible = false;
            }
        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CargarCuentasBancarias()
        {
            string secCodigo = null;

            if (Arguments.Values.CurrentSector != null)
            {
                secCodigo = Arguments.Values.CurrentSector.SecCodigo;
            }

            CuentasSource = new ObservableCollection<IKV>(new DS_CuentasBancarias().GetCuentasBancarias(secCodigo, CurrentMoneda != null ? CurrentMoneda.MonCodigo : null));

            PickerSource = CuentasSource;
        }

        public double ChangeBalanceCambioMoneda()
        {
            var monto = FillMonto?.Invoke();

            if ((ShowMoneda && CurrentMoneda != null && MonedaDocumentos != null && CurrentMoneda.MonTasa != MonedaDocumentos.MonTasa && !DS_RepresentantesParametros.GetInstance().GetParRecibosTasaFacturas_O_TasaDia())
                || (DS_RepresentantesParametros.GetInstance().GetParRecibosTasaFacturas_O_TasaDia() && CurrentMoneda.MonCodigo != MonedaDocumentos.MonCodigo))
            {
                
                var montoMonedaBase = monto * MonedaDocumentos.MonTasa;

                var montoCalculado = montoMonedaBase / CurrentMoneda.MonTasa;
                //editMonto.Text = montoCalculado.ToString();
                double.TryParse(montoCalculado.ToString(), out double montovalor);
                montoBase = montovalor;
                double montoRedondeado = Math.Round(montovalor, 2);
                var Monto = montoRedondeado;//montoCalculado.ToString();
                return Monto;
            }

            double result = monto != null ? double.Parse(monto.ToString()) : 0.00;

            return result;
        }

        public void ChangeToDiferido()
        {
                IsFuturista = true;
                IsEnabledFecha = false;
                pickerFecha.Date = fechaFuturista;
                checkDiferido.IsEnabled = false;
        }

    }
}