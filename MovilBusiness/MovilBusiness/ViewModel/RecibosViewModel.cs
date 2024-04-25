using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Internal;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.model.Internal.Structs.Args;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.Views.Components.Modals;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.viewmodel
{
    public class RecibosViewModel : BaseViewModel
    {
        public ICommand SaveCommnand { get; private set; }
        public ICommand AddFormaPagoCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand SaldarCommand { get; private set; }
        public ICommand DetailsCommand { get; private set; }

        public ICommand AutorizarDescGeneralCommand { get; private set; }

        private List<RecibosDocumentosTemp> documentos;
        public List<RecibosDocumentosTemp> Documentos { get => documentos; private set { documentos = value; RaiseOnPropertyChanged(); } }
        private List<FormasPagoTemp> formaspagosource;
        public List<FormasPagoTemp> FormasPagoSource { get => formaspagosource; private set { formaspagosource = value; RaiseOnPropertyChanged(); } }

        private RecibosDocumentosTemp currentdocument;
        public RecibosDocumentosTemp CurrentDocument { get => currentdocument; set { currentdocument = value;  Arguments.Values.CurrenRecDocumentosTemp = value; RaiseOnPropertyChanged(); } }

        private RecibosDocumentosTemp currentnc;
        public RecibosDocumentosTemp CurrentNotaCredito { get => currentnc; set { currentnc = value; RaiseOnPropertyChanged(); } }

        private bool showdetallerecibo = false;
        public bool ShowDetalleRecibo { get => showdetallerecibo; set { showdetallerecibo = value; RaiseOnPropertyChanged(); } }

        private bool detailsDescuentosVisible = true;
        public bool DetailsDescuentosVisible { get => detailsDescuentosVisible; set { detailsDescuentosVisible = value; RaiseOnPropertyChanged(); } }

        private bool detailsAplicacionVisible;
        public bool DetailsAplicacionVisible { get => detailsAplicacionVisible; set { detailsAplicacionVisible = value; RaiseOnPropertyChanged(); } }

        private FormasPagoPermitidasCliente FormasDePagoPermitidas = null;

        private FormasPago currentformapago = FormasPago.Null;
        public FormasPago CurrentFormaPago { get => currentformapago; set { currentformapago = value; RaiseOnPropertyChanged(); } }

        private double totalapagar = 0;
        public double TotalAPagar { get => totalapagar; set { totalapagar = value; RaiseOnPropertyChanged(); } }

        public bool ParAutorizarDescuentoGeneral { get => myParametro.GetParRecibosAutorizarDescuento() && myParametro.GetParRecibosTabGeneral(); }

        public string ReciboNumero { get => AppResource.Receipt + " #: " + GettingSecBySet(); }

        private string recnumero;
        public string RecNumero { get => recnumero; set { recnumero = value; RaiseOnPropertyChanged(); } }

        private RecibosResumen resumen;
        public RecibosResumen Resumen { get => resumen; set { resumen = value; RaiseOnPropertyChanged(); } }

        private DS_Recibos myRec;
        private DS_Clientes myCli;
        private DS_CuentasxCobrar myCxc;

        private int CurrentRecSecuencia = -1;
        public Monedas CurrentMoneda;
        public double LastTasa;

        private List<Monedas> monedas;
        public List<Monedas> Monedas { get => monedas; set { monedas = value; RaiseOnPropertyChanged(); } }

        private List<DescuentoFacturas> descuentosFacturas;
        public List<DescuentoFacturas> DescuentosFacturas { get => descuentosFacturas; set { descuentosFacturas = value; RaiseOnPropertyChanged(); } }

        private List<CuentasXCobrarAplicaciones> cuentasXCobrarAplicacionesList;
        public List<CuentasXCobrarAplicaciones> CuentasXCobrarAplicacionesList { get => cuentasXCobrarAplicacionesList; set { cuentasXCobrarAplicacionesList = value; RaiseOnPropertyChanged(); } }

        public bool ShowChkDiferidoGeneral { get; set; }
        public bool IsFirstChkDiferidoGeneral = true;

        private bool ischkdiferidogeneral;
        public bool IsChkDiferidoGeneral { get => ischkdiferidogeneral; set { ischkdiferidogeneral = value; if (!value) { FechaChkDiferidoGeneral = FechaMinimaChkDiferido; } ChkDiferidoGeneralChanged(value); RaiseOnPropertyChanged(); } }     
     
        private bool detailsDocumentVisible;
        public bool DetailsDocumentVisible { get => detailsDocumentVisible; set { detailsDocumentVisible = value; RaiseOnPropertyChanged(); } }        

        private DateTime fechachkdiferidogeneral;
        public DateTime FechaChkDiferidoGeneral { get => fechachkdiferidogeneral; set { fechachkdiferidogeneral = value; RaiseOnPropertyChanged();
                if (IsChkDiferidoGeneral) 
                {
                    CargarDocumentos(false, value.ToString("yyyy-MM-dd"));
                }}
        }

        public DateTime FechaMinimaChkDiferido { get; set; } = DateTime.Now.AddDays(1);

        public List<int> EntregasSecuencias { get; set; }

        public string MonSiglaCliente { get; private set; }

        private AgregarFormaPagoModal dialogAddFormaPago;
        private DetalleNotaCreditoModal dialogDetalleNC;

        private Action reciboGuardado;
        public Action<int> OnCurrentPageChanged { get; set; }
        public bool IsEditing { get; set; }
        public bool FromCopy { get; set; }
        private readonly int VenSecuencia = -1;
        private AgregarTazaModal dialogAgregarTaza;
        public bool ShowTaza = false;
        public bool FirstTime { get; set; } = true;
        public Monedas monedaSinActualizar;

        private readonly bool _IsConsulting = false;

        private int _ConId = -1;
        public bool ReloadDocuments { get; set; }

        public RecibosViewModel(Page page, Action reciboGuardado, int venSecuencia = -1, Monedas moneda = null, bool IsFromEditar = false, int ConId = -1, bool IsFromCopy = false, bool IsConsulting = false) : base(page)
        {
            _ConId = ConId;
            IsEditing = IsFromEditar;
            FromCopy = IsFromCopy;
            VenSecuencia = venSecuencia;

            _IsConsulting = IsConsulting;

            myCli = new DS_Clientes();

            if (moneda != null) {
                CurrentMoneda = moneda;
                LastTasa = moneda.MonTasa;
            }
            FirstTime = true;
            try
            {
                this.reciboGuardado = reciboGuardado;

                SaveCommnand = new Command(async () => { await GuardarRecibo(); });
                AddFormaPagoCommand = new Command(ShowAlertAddFormaPago);
                DeleteCommand = new Command(()=> { EliminarFactura(); DetailsDocumentVisible = false; });
                DetailsCommand = new Command(() => { ShowDetalleRecibo = true; DetailsDocumentVisible = false;  });
                SaldarCommand = new Command( async () => { await SaldarFacturaByCommand(); DetailsDocumentVisible = false; });
                myRec = new DS_Recibos();
                myCxc = new DS_CuentasxCobrar();
                ShowTaza = myParametro.GetParRecibosAutorizacionTazaFactura() > 0;

                FormasDePagoPermitidas = new FormasPagoPermitidasCliente()
                {
                    PermiteCheque = true,
                    PermiteChequeDiferido = true,
                    PermiteChequeRegular = true,
                    PermiteNotaCredito = true,
                    PermitePagoEfectivo = true,
                    PermiteRetencion = true,
                    PermiteTarjetaCredito = true,
                    PermiteTransferencia = true,
                    PermiteOrdenPago = true,
                    PermiteDiferenciaCambiaria= true,
                    PermiteRedondeo= true
                };

                if (moneda != null)
                {
                    MonSiglaCliente = moneda.MonSigla;
                }

                AutorizarDescGeneralCommand = new Command(ShowAlertAutorizarDescuento);

                CargarFormasDePagoPermitidas();

                if (IsFromEditar)
                {
                    // myRec.CalcularDescuentoChkDiferidoADocumentosSaldados(-1,IsFromEditar);
                    Documentos = myRec.GetDocumentsInTemp();

                    TotalAPagar = myRec.GetTotalAPagar();
                    Resumen = myRec.GetResumenFormasPagoInTemp();
                }

                ShowChkDiferidoGeneral = FormasDePagoPermitidas.PermiteChequeDiferido && !myParametro.GetParRecibosTiposCheques();

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            
        }

        private string GettingSecBySet()
        {

            string AreaCtrlsubto = "";
            if (myParametro.GetParAreaCrtlCreditoClienteSubString() && Arguments.Values.CurrentSector != null && Arguments.Values.CurrentClient != null)
            {
                string areaSub = myCli.GetareaCtrlCreditOfClienteDetalle(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentSector.SecCodigo);
                if (!string.IsNullOrEmpty(areaSub))
                {
                    AreaCtrlsubto = areaSub.Length > 1 ? areaSub.Substring(0, 2) : "";
                }
            }

            int RecSecuenciaParams = myParametro.GetParRecibosSecuenciaPorSector();

            switch (RecSecuenciaParams)
            {
                case 1:
                    return DS_RepresentantesSecuencias.GetLastSecuencia("Recibos-" + Arguments.Values.CurrentSector.SecCodigo).ToString();
                case 2:
                    return DS_RepresentantesSecuencias.GetLastSecuencia("Recibos-" + AreaCtrlsubto).ToString();
                default:
                    return DS_RepresentantesSecuencias.GetLastSecuencia("Recibos").ToString();
            }

        }

        private async void ShowAlertAutorizarDescuento()
        {
            try
            {
                if (IsBusy)
                {
                    return;
                }

               /* if (!CurrentFactura.CalcularDesc)
                {
                    await Functions.DisplayAlert(AppResource.Warning, "Presione el boton (Aplicar Desc) antes de autorizar.");
                    return;
                }*/

                IsBusy = true;

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

                await PushModalAsync(new AutorizacionesModal(true, RecSecuencia, 3, null)
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

            IsBusy = false;
        }

        private void AutorizarDescuento(int autSecuencia)
        {
            try
            {
                myRec.UpdateAutSecuenciaFacturaInTemp(null, autSecuencia);

                DisplayAlert(AppResource.Success, AppResource.DiscountAuthorized);

                ReloadDocuments = true;
            }
            catch (Exception e)
            {
                Functions.DisplayAlert(AppResource.Warning, e.Message);
            }
        }

        private async Task GuardarRecibo(bool authorized = false)
        {
            if (IsBusy)
            {
                return;
            }

            if (_IsConsulting)
            {
                IsBusy = false;
                await DisplayAlert(AppResource.Warning, AppResource.ConsultationModeVisitMessage);
                return;
            }

            if (Arguments.Values.CurrentModule == Modules.RECONCILIACION)
            {
                GuardarReconciliacion();
                return;
            }





            bool Haydocaplicados = myRec.HayDocumentosAplicadosInTemp();

            if (!Haydocaplicados && !myParametro.GetParReciboSinAplicacion())
            {
                await DisplayAlert(AppResource.Warning, AppResource.NotAppliedAnyInvoice, AppResource.Aceptar);
                return;
            }

            if (myParametro.GetParRecibosTabGeneral())
            {
                if (myParametro.GetParRecibosRecNumeroObligatorio() && string.IsNullOrWhiteSpace(RecNumero))
                {
                    OnCurrentPageChanged?.Invoke(0);
                    await DisplayAlert(AppResource.Warning, AppResource.ReceiptNumberCannotBeEmpty);
                    return;
                }
            }

            if (myParametro.GetParRecibosNoPermitirSobranteSiHayAbono() && myRec.HaySobrantesConAbonos())
            {
                await DisplayAlert(AppResource.Warning, AppResource.AreInvoicesPaidAndSurplusCannotContinue, AppResource.Aceptar);
                return;
            }

            if (myRec.PermiteRecibosSplitNotasDeCredito() && myRec.HayNotasCreditoFraccionadasConPendiente())
            {
                await DisplayAlert(AppResource.Warning, "Las notas de credito deben aplicarse completamente para relizar el recibo");
                return;
            }

            bool validarsobrantes = myRec.GetFormasPagoAgregadasForValid(out string[] parNoAceptarSobrante);

            if (resumen?.Sobrante > 0 && (parNoAceptarSobrante.Length > 0 && parNoAceptarSobrante[0] == "1" && !validarsobrantes))
            {
                await DisplayAlert(AppResource.Warning, AppResource.CannotMakeReceiptWithSurplus, AppResource.Aceptar);
                return;
            }


            var Toselect = myRec.GetDocumentosAplicadosForSelectFac();

            double faltanteosobranteResult = myParametro.GetParRecAceptarFaltanteToSel();

            if (resumen?.Sobrante > 0 && faltanteosobranteResult > 0.00 && faltanteosobranteResult > Math.Abs(resumen.Sobrante) && Toselect != null && Toselect.Length > 0)
            {
                bool isSeleccted = await DisplayAlert(AppResource.Warning, AppResource.WantApplySurplusToDiscountQuestion, AppResource.Yes, AppResource.No);

                if (isSeleccted)
                {
                    myRec.UpdateRecibosDocumentosTempForSelect(await DisplayActionSheet(AppResource.SelectInvoice, buttons: Toselect), resumen.Sobrante, true);
                }
                else
                {
                    return;
                }
            }else if (resumen?.Sobrante > 0 && (parNoAceptarSobrante[0] == "2" && !validarsobrantes) && myRec.GetMontoPendienteInTemp() > 0)
            {
                await DisplayAlert(AppResource.Warning, AppResource.ArePendingInvoicesCannotBeSurplus, AppResource.Aceptar);
                return;
            }


            if (CurrentMoneda != null)
            {
                var parRecAceptarSobrante = myParametro.GetParRecAceptarSobrante(CurrentMoneda.MonCodigo);
                if(parRecAceptarSobrante > 0)
                {
                    bool aceptaSobrante = (parRecAceptarSobrante >= Math.Abs(resumen.Sobrante));
                    if (!aceptaSobrante)
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.CannotMakeReceiptWithSurplusGreaterThan + parRecAceptarSobrante.ToString() + "-"+ CurrentMoneda.MonCodigo, AppResource.Aceptar);
                        return;
                    }
                }
                
            }

            if (!myRec.HayFormasDePagoAgregadasInTemp())
            {
                await DisplayAlert(AppResource.Warning, AppResource.MustAddPaymentway, AppResource.Aceptar);
                return; 
            }

            if (Arguments.Values.CurrentClient.CliDatosOtros != null && Arguments.Values.CurrentClient.CliDatosOtros.Contains("T"))
            {
                double total = myRec.GetTotal();
                double PorcentajeRetencion = myParametro.GetParRecibosPorcientoRetencion();
                double MontoRetencion = myRec.GetMontoTotalFormaPagoByName("RETENCION");
                double Retencion = (Math.Abs(total) * PorcentajeRetencion) / 100;
                MontoRetencion = Math.Round(MontoRetencion, 2);
                Retencion = Math.Round(Retencion, 2);

                if ( (!myRec.HayFormasDePagoAgregadasRetencion()) || (MontoRetencion != Retencion) )
                {
                    await DisplayAlert(AppResource.Warning, AppResource.CustomerAppliesForRetentionYouMustAdd + Retencion.ToString(), AppResource.Aceptar);
                    return;
                }

            }

            if (!IsEditing)
            {
                if (CurrentMoneda != null)
                {
                    var parRecAceptarFaltante = myParametro.GetParRecAceptarFaltante(CurrentMoneda.MonCodigo);
                    bool aceptaFaltane = (parRecAceptarFaltante > 0 && parRecAceptarFaltante > Math.Abs(TotalAPagar));

                    if (TotalAPagar < 0 && faltanteosobranteResult > 0.00 && faltanteosobranteResult > Math.Abs(TotalAPagar) && Toselect != null && Toselect.Length > 0)
                    {
                       bool isSeleccted = await DisplayAlert(AppResource.Warning, AppResource.WantApplyMissingToDiscount, AppResource.Yes, AppResource.No);

                        if(isSeleccted)
                        {
                            myRec.UpdateRecibosDocumentosTempForSelect(await DisplayActionSheet(AppResource.SelectInvoice, buttons: Toselect), TotalAPagar, false);
                        }else
                        {
                            return;
                        }
                    }
                    else if (!aceptaFaltane && TotalAPagar < -1)
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.CannotMakeReceiptWithMissing, AppResource.Aceptar);
                        return;
                    }
                }
            }

            var parFacturasViejasSinSaldar = myParametro.GetParNoAceptarReciboSiHayFacturasViejasSinSaldar();
            if (myRec.ifExistOldFact() && parFacturasViejasSinSaldar > 0 && !authorized)
            {
                if (parFacturasViejasSinSaldar == 2)
                {
                    var aut = await DisplayAlert(AppResource.Warning, AppResource.CannotMakePaymentHaveOlderInvoicesWantAuthorize, AppResource.Authorize, AppResource.Cancel);
                    if (aut)
                    {
                        var modal = new AutorizacionesModal(false, myRec.GetNextSecuenciaRecibos(), 3, null);
                        modal.OnAutorizacionUsed = (autSecuencia) =>
                        {
                            GuardarRecibo(true);
                        };
                        await PushModalAsync(modal);
                    }
                }
                else
                {
                    await DisplayAlert(AppResource.Warning, AppResource.CannotMakePaymentHaveOlderInvoices, AppResource.Aceptar);
                }
                return;
            }


            var task = new TaskLoader() { SqlTransactionWhenRun = true };

            try
            {
                if (myParametro.GetParRecibosTabGeneral())
                {
                    OnCurrentPageChanged?.Invoke(1);
                }
                else
                {
                    OnCurrentPageChanged?.Invoke(0);
                }

                IsBusy = true;

                await task.Execute(() =>
                {
                    //La VenSecuencia que se esta enviando es el Recibo que se esta editando
                    CurrentRecSecuencia = myRec.GuardarRecibo(RecNumero, CurrentMoneda, IsEditing, VenSecuencia, fromCopy: FromCopy);
                    //Arguments.Values.CurrentIsReciboHuerfano = false;
                });

                if (!Haydocaplicados && myParametro.GetParReciboSinAplicacion())
                {
                    await DisplayAlert(AppResource.Warning, AppResource.ReceiptWasSavedWithoutApplication, AppResource.Aceptar);
                }

                Arguments.Values.CurrentSector = Arguments.Values.CurrentSector?? new DS_Sectores().GetSectorByRecibos(CurrentRecSecuencia);

                if (EntregasSecuencias != null && EntregasSecuencias.Count > 0)
                {
                    Arguments.Values.CurrentModule = Modules.COBROS;
                }

                var modal = new SuccessPage(AppResource.ReceiptSavedUpper, VenSecuencia != -1 && (Arguments.Values.CurrentModule == Modules.VENTAS || Arguments.Values.CurrentModule == Modules.PEDIDOS) ? VenSecuencia : CurrentRecSecuencia);
                modal.EntregasSecuencias = EntregasSecuencias;                

                await PushAsync(modal);

                reciboGuardado();
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.ErrorSavingReceipt, e.InnerException != null ? e.InnerException.Message : e.Message, AppResource.Aceptar);
            }

            IsBusy = false;

        }

        private async void GuardarReconciliacion()
        {

            IsBusy = true;

            try
            {
               /* bool Haydocaplicados = myRec.HayDocumentosAplicadosInTemp();

                if (!Haydocaplicados)
                {
                    IsBusy = false;
                    await DisplayAlert(AppResource.Warning, "No has aplicado ninguna factura, no puedes continuar", AppResource.Aceptar);
                    return;
                }*/

                bool hayNCAplicadas = myRec.HayNotasCreditosAplicadas();

                if (!hayNCAplicadas)
                {
                    IsBusy = false;
                    await DisplayAlert(AppResource.Warning, AppResource.NotAppliedAnyCreditNoteCannotContinue);
                    return;
                }

                var task = new TaskLoader() { SqlTransactionWhenRun = true };

                await task.Execute(() => 
                {
                    CurrentRecSecuencia = new DS_Reconciliaciones().GuardarReconciliacion(CurrentMoneda);
                });

                await PushAsync(new SuccessPage(AppResource.ReconciliationSavedUpper, CurrentRecSecuencia));

                reciboGuardado();

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.ErrorSavingReconciliation, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
        }

        private void CargarFormasDePagoPermitidas()
        {
            if (!myParametro.GetParFiltrarClienteFormasPago() || string.IsNullOrWhiteSpace(Arguments.Values.CurrentClient.CliFormasPago))
            {
                return;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(myParametro.GetParFormasPagoValidasGeneral()))
                {
                    char[] valorxParametro = myParametro.GetParFormasPagoValidasGeneral().ToCharArray();

                    if (valorxParametro.Length == 9)
                    {
                        FormasDePagoPermitidas = new FormasPagoPermitidasCliente
                        {
                            PermitePagoEfectivo = valorxParametro[0] == '1',
                            PermiteCheque = valorxParametro[1] == '1' || valorxParametro[1] == '2',
                            PermiteChequeRegular = valorxParametro[1] == '1',
                            PermiteChequeDiferido = valorxParametro[1] == '2',
                            PermiteNotaCredito = valorxParametro[2] == '1',
                            PermiteTransferencia = valorxParametro[3] == '1',
                            PermiteRetencion = valorxParametro[4] == '1',
                            PermiteTarjetaCredito = valorxParametro[5] == '1',
                            PermiteOrdenPago = valorxParametro[6] == '1',
                            PermiteDiferenciaCambiaria = valorxParametro[7] == '1',
                            PermiteRedondeo = valorxParametro[8] == '1',
                        };
                    }
                    else
                    {
                        FormasDePagoPermitidas = new FormasPagoPermitidasCliente
                        {
                            PermitePagoEfectivo = valorxParametro[0] == '1',
                            PermiteCheque = valorxParametro[1] == '1' || valorxParametro[1] == '2',
                            PermiteChequeRegular = valorxParametro[1] == '1',
                            PermiteChequeDiferido = valorxParametro[1] == '2',
                            PermiteNotaCredito = valorxParametro[2] == '1',
                            PermiteTransferencia = valorxParametro[3] == '1',
                            PermiteRetencion = valorxParametro[4] == '1',
                            PermiteTarjetaCredito = valorxParametro[5] == '1',
                            PermiteOrdenPago = valorxParametro[6] == '1',
                        };
                    }

                }
                else
                {

                    char[] valor = Arguments.Values.CurrentClient.CliFormasPago.ToCharArray();

                    if (valor.Length == 9)
                    {
                        FormasDePagoPermitidas = new FormasPagoPermitidasCliente
                        {
                            PermitePagoEfectivo = valor[0] == '1',
                            PermiteCheque = valor[1] == '1' || valor[1] == '2',
                            PermiteChequeRegular = valor[1] == '1',
                            PermiteChequeDiferido = valor[1] == '2',
                            PermiteNotaCredito = valor[2] == '1',
                            PermiteTransferencia = valor[3] == '1',
                            PermiteRetencion = valor[4] == '1',
                            PermiteTarjetaCredito = valor[5] == '1',
                            PermiteOrdenPago = valor[6] == '1',
                            PermiteDiferenciaCambiaria = valor[7] == '1',
                            PermiteRedondeo = valor[8] == '1',
                        };
                    }
                    else
                    {
                        FormasDePagoPermitidas = new FormasPagoPermitidasCliente
                        {
                            PermitePagoEfectivo = valor[0] == '1',
                            PermiteCheque = valor[1] == '1' || valor[1] == '2',
                            PermiteChequeRegular = valor[1] == '1',
                            PermiteChequeDiferido = valor[1] == '2',
                            PermiteNotaCredito = valor[2] == '1',
                            PermiteTransferencia = valor[3] == '1',
                            PermiteRetencion = valor[4] == '1',
                            PermiteTarjetaCredito = valor[5] == '1',
                            PermiteOrdenPago = valor[6] == '1',
                        };
                    }

                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        private async void ShowAlertAddFormaPago()
        {
            try
            {
                var list = new List<string>();

                if(_ConId != -1 && _ConId == myParametro.GetParRecElegirFormapagoParaVentas())
                {
                    list.Add("Tarjeta Crédito");
                }
                else
                {
                    if (FormasDePagoPermitidas.PermiteCheque || FormasDePagoPermitidas.PermiteChequeDiferido /*|| FormasDePagoPermitidas.PermiteNotaCredito*/)
                    {
                        list.Add("Cheque");
                    }

                    if (FormasDePagoPermitidas.PermitePagoEfectivo)
                    {
                        list.Add("Efectivo");
                    }

                    if (FormasDePagoPermitidas.PermiteRetencion)
                    {
                        list.Add("Retención");
                    }

                    if (FormasDePagoPermitidas.PermiteTarjetaCredito)
                    {
                        list.Add("Tarjeta Crédito");
                    }

                    if (FormasDePagoPermitidas.PermiteTransferencia)
                    {
                        list.Add("Transferencia");
                    }

                    if (FormasDePagoPermitidas.PermiteOrdenPago && !IsEditing)
                    {
                        list.Add("Orden de pago");
                    }

                    if (FormasDePagoPermitidas.PermiteDiferenciaCambiaria && !IsEditing)
                    {
                        list.Add("Diferencia Cambiaria");
                    }
                    
                    if (FormasDePagoPermitidas.PermiteRedondeo && !IsEditing)
                    {
                        list.Add("Redondeo");
                    }

                    if (IsChkDiferidoGeneral)
                    {
                        list.Clear();
                        list.Add("Cheque diferido");
                    }
                }


                var result = await DisplayActionSheet(AppResource.ChoosePaymentway, buttons: list.ToArray());

                if (dialogAddFormaPago == null)
                {
                    dialogAddFormaPago = new AgregarFormaPagoModal(myRec, CurrentMoneda)
                    {
                        FillMonto = () => {
                            return Math.Abs(myRec.GetTotalAPagar(dialogAddFormaPago.IsFuturista ? dialogAddFormaPago.fechaFuturista.ToString("yyyy-MM-dd HH:mm:ss") : null, true));
                        },
                        OnAccepted = AgregarFormaPago
                    };
                }

                switch (result)
                {
                    case "Cheque":
                    case "Cheque diferido":
                        CurrentFormaPago = FormasPago.Cheque;
                        break;
                    case "Efectivo":
                        CurrentFormaPago = FormasPago.Efectivo;
                        break;
                    case "Transferencia":
                        CurrentFormaPago = FormasPago.Transferencia;
                        break;
                    case "Tarjeta Crédito":
                        CurrentFormaPago = FormasPago.TarjetaCredito;
                        break;
                    case "Retención":
                        CurrentFormaPago = FormasPago.Retencion;
                        break;
                    case "Orden de pago":
                        CurrentFormaPago = FormasPago.OrdenPago;
                        break;
                    case "Diferencia Cambiaria":
                        CurrentFormaPago = FormasPago.DiferenciaCambiaria;
                        break;
                    case "Redondeo":
                        CurrentFormaPago = FormasPago.Redondeo;
                        break;
                    default:
                        return;
                }
                dialogAddFormaPago.CurrentFormaPago = CurrentFormaPago;
                dialogAddFormaPago.MontoAPagar = TotalAPagar; //< 0 ? TotalAPagar : 0 ;
                dialogAddFormaPago.IsChkDiferidoGeneral = IsChkDiferidoGeneral;
                dialogAddFormaPago.FirstTime = true;

                if (IsChkDiferidoGeneral)
                {
                    dialogAddFormaPago.fechaFuturista = FechaChkDiferidoGeneral;
                    dialogAddFormaPago.ChangeToDiferido();
                }

                await PushAsync(dialogAddFormaPago);

            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }
        }

        public void CargarDocumentos() { CargarDocumentos(false); }
        public async void CargarDocumentos(bool insert, string ChckDif = null)
        {
            IsBusy = true;

            try
            {
                ReloadDocuments = false;

                await Task.Run(() =>
                {
                    if (insert && VenSecuencia == -1 && Arguments.Values.CurrentModule != Modules.VENTAS && Arguments.Values.CurrentModule != Modules.PEDIDOS && Arguments.Values.CurrentModule != Modules.ENTREGASREPARTIDOR && !IsEditing)
                    {
                        myCxc.InsertPendingDocumentsInTemp(CurrentMoneda != null ? CurrentMoneda.MonCodigo : null);
                    }
                    else if (insert && (myParametro.GetParNotaCreditoPorDevolucion() || myParametro.GetParNotaCreditoVerPagoContado()))
                    {
                        myCxc.InsertNCPendingDocumentsInTemp(CurrentMoneda != null ? CurrentMoneda.MonCodigo : null);
                    }

                    if(myParametro.GetParRecibosusarMonedaUnica() && CurrentMoneda != null && insert)
                    {
                        myCxc.CambiarMonedaDocumentos(CurrentMoneda);
                    }

                    if(!string.IsNullOrEmpty(ChckDif))
                    {
                        myRec.UpdateDateRec(ChckDif);
                        myRec.CalcularDescuentoChkDiferidoADocumentosSaldados(withCalcDesc: true, isForAgregarFormaPago: true, datechfdif: ChckDif);
                    }

                    Documentos = myRec.GetDocumentsInTemp();

                    TotalAPagar = myRec.GetTotalAPagar();
                    Resumen = myRec.GetResumenFormasPagoInTemp();
                });

            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.ErrorLoadingDocuments, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
        }

        public async void CargarFormasPago()
        {
            IsBusy = true;

            try
            {
                await Task.Run(() =>
                {
                    FormasPagoSource = myRec.GetFormasPagoInTemp();
                    TotalAPagar = myRec.GetTotalAPagar();
                    Resumen = myRec.GetResumenFormasPagoInTemp();
                });
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.ErrorLoadingPaymentway, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
        }

        public void DocumentSelected(RecibosDocumentosTemp data)
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            CurrentDocument = data;

            if (data.Origen == 1)
            {
                if( Arguments.Values.CurrentModule != Enums.Modules.VENTAS &&
                    Arguments.Values.CurrentModule != Enums.Modules.PEDIDOS &&
                    Arguments.Values.CurrentModule != Enums.Modules.ENTREGASREPARTIDOR)
                {
                    ShowAlertRecibo();
                }
            }
            else if (data.Origen == -1)
            {
                ShowAlertNC();
            }

            IsBusy = false;
        }

        public async void FormaPagoSelected(FormasPagoTemp forma)
        {
            if (forma == null)
            {
                return;
            }

            var result = await DisplayAlert(AppResource.Select, AppResource.PaymentwayLabel+ " " + forma.FormaPago + "\n" + AppResource.BankLabel + forma.Banco + "\n" + AppResource.CheckNo + " " + forma.NoCheque + "\n" + AppResource.Value + ": " + forma.Valor, AppResource.Remove, AppResource.Cancel);

            if (result)
            {

                myRec.EliminarFormaPagoInTemp(forma);
                if (IsEditing) {
                    myRec.UpdateRefValorRecDocTemp(forma.RefSecuencia, VenSecuencia);
                }
                else
                {
                    myRec.CalcularDescuentosFacturasInTemp();
                }
                CargarFormasPago();

                //TotalAPagar = myRec.GetTotalAPagar();
                CargarDocumentos();

                ReloadDocuments = true;
            }
        }

        private async void ShowAlertNC()
        {
            try
            {
                CurrentNotaCredito = CurrentDocument;

                string[] buttons;

                if (Arguments.Values.CurrentModule != Modules.RECONCILIACION && myParametro.GetParRecibosNotasCreditoAplicacionAutomatica())
                {
                    buttons = new string[] { AppResource.AutomaticApplication, AppResource.ManualApplication, AppResource.Remove };
                }
                else
                {
                    buttons = new string[] { AppResource.ManualApplication, AppResource.Remove };
                }

                var result = await DisplayActionSheet(AppResource.SelectDesiredOption, buttons: buttons);


                if(result == AppResource.ManualApplication)
                {
                    if (dialogDetalleNC == null)
                    {
                        dialogDetalleNC = new DetalleNotaCreditoModal
                        {
                            OnAccepted = AplicarNC
                        };
                    }
                    dialogDetalleNC.NotaCredito = CurrentDocument;
                    await PushModalAsync(dialogDetalleNC);
                }
                else if(result == AppResource.AutomaticApplication)
                {
                    AplicarNC(new AplicarNCArgs() { NC = CurrentDocument }, true);
                }
                else if(result == AppResource.Remove)
                {
                    EliminarNC();
                }
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }
        }

        private async void ShowAlertRecibo()
        {

            if(myParametro.GetParRecDetalleV2())
            {
                DetailsDocumentVisible = true;

                DescuentosFacturas = new DS_DescuentoFacturas()
                    .GetDescuentoFacturaByCXCReferencia(CurrentDocument.Referencia, CurrentDocument.Balance, CurrentDocument.DefIndicadorItbis ? CurrentDocument.MontoTotal : CurrentDocument.MontoSinItbis);
                CuentasXCobrarAplicacionesList = new DS_CuentasXCobrarAplicaciones()
                    .GetCxcAplicaciones(CurrentDocument.Referencia);

                CurrentDocument.AplicacionesMonto = CuentasXCobrarAplicacionesList != null ?
                    CuentasXCobrarAplicacionesList.Sum(c => c.CxcMonto) : 0.00;
                return;
            }


            string msg = CurrentDocument.Sigla + " - " + CurrentDocument.Documento + "\n" +
                AppResource.TotalAmountLabel + " " + CurrentDocument.MontoTotal.ToString("C2") + "\n" +
                "Balance: " + CurrentDocument.Balance.ToString("C2") + "\n" +
                AppResource.PendingLabel + CurrentDocument.Pendiente.ToString("C2");

            var btnDetalle = AppResource.Detail;

            var parBtnDetalleDescription = myParametro.GetParRecibosDialogDetalleBtnDetalleDescripcion();

            if (!string.IsNullOrWhiteSpace(parBtnDetalleDescription))
            {
                btnDetalle = parBtnDetalleDescription;
            }

            string result;

            if(Arguments.Values.CurrentModule == Modules.RECONCILIACION)
            {
                result = await DisplayActionSheet(msg, buttons: new string[] { AppResource.Detail });
            }
            else
            {
                result = await DisplayActionSheet(msg, buttons: new string[] { AppResource.PayOff, btnDetalle, AppResource.Uncheck });
            }

            if (result == btnDetalle || (Arguments.Values.CurrentModule == Modules.RECONCILIACION && result == AppResource.Detail))
            {
                ShowDetalleRecibo = true;
            }
            else if (result == AppResource.PayOff)
            {
                if (myParametro.GetParRecibosTasaFacturas_O_TasaDia())
                {
                        if (myCxc.ValidarFacturasSaldadasNoVencidas(CurrentDocument.Referencia) > 0 )
                        {
                            if (myCxc.GetTasaFactura(CurrentDocument.Referencia) > 0)
                            {
                                if (CurrentDocument.Tasa > myRec.GetMaxTasaAplicada(CurrentDocument.Referencia)) 
                                {
                                    var resul = await DisplayAlert(AppResource.Warning, AppResource.RateChangeToUpper + myCxc.GetTasaFactura(CurrentDocument.Referencia), AppResource.Aceptar, AppResource.Cancel);
                                    IsBusy = false;
                                    if (resul)
                                    {
                                        CurrentMoneda.MonTasa = myCxc.GetTasaFactura(CurrentDocument.Referencia);
                                        SaldarFactura();
                                    }
                                }
                                else
                                {
                                    //CurrentMoneda.MonTasa = myCxc.GetTasaFactura(CurrentDocument.Referencia);
                                    SaldarFactura();
                                }   
                            }
                            else
                            {
                                await DisplayAlert(AppResource.Warning, AppResource.TheRateOfTheDayWillBeUsed + LastTasa, AppResource.Aceptar);
                                CurrentMoneda.MonTasa = LastTasa;
                                SaldarFactura();
                            }

                        }
                        else
                        {
                            if (myCxc.GetCountFacturasSaldadas() > 1)
                            {
                                if (myCxc.GetTasaFactura(CurrentDocument.Referencia) > 0)
                                {
                                    if (CurrentDocument.Tasa > myRec.GetMaxTasaAplicada(CurrentDocument.Referencia))
                                    {
                                        var resul = await DisplayAlert(AppResource.Warning, AppResource.RateChangeToUpper + myCxc.GetTasaFactura(CurrentDocument.Referencia), AppResource.Aceptar, AppResource.Cancel);
                                        IsBusy = false;
                                        if (resul)
                                        {
                                            CurrentMoneda.MonTasa = myCxc.GetTasaFactura(CurrentDocument.Referencia);
                                            SaldarFactura();
                                        }
                                    }
                                    else
                                    {
                                        //CurrentMoneda.MonTasa = myCxc.GetTasaFactura(CurrentDocument.Referencia);
                                        SaldarFactura();
                                    }
                                }
                                else
                                {
                                    await DisplayAlert(AppResource.Warning, AppResource.TheRateOfTheDayWillBeUsed + LastTasa, AppResource.Aceptar);
                                    CurrentMoneda.MonTasa = LastTasa;
                                    SaldarFactura();
                                }
                            }
                            else
                            {
                                await DisplayAlert(AppResource.Warning, AppResource.TheRateOfTheDayWillBeUsed + LastTasa, AppResource.Aceptar);
                                CurrentMoneda.MonTasa = LastTasa;

                                SaldarFactura();
                            }

                        }
                    
                }
                else {

                    SaldarFactura();
                }
            }
            else if (result == AppResource.Uncheck)
            {
                EliminarFactura();
            }
        }

        private void SaldarFactura()
        {
            if (CurrentDocument.Estado == "Saldo")
            {
                DisplayAlert(AppResource.Warning, AppResource.DocumentAlreadyBeenPaid, AppResource.Aceptar);
                return;
            }

            if (myParametro.GetParRecNoSaldoWithAbonoDiferido() && myRec.ExistAbonadoDiferido(CurrentDocument.Referencia))
            {
                DisplayAlert(AppResource.Warning, AppResource.CannotPayBillHaveFuturisticCreditMemo, AppResource.Aceptar);
                return;
            }

            try
            {
                if(CurrentMoneda == null)
                {
                    DisplayAlert(AppResource.Warning, "La moneda del documento no esta registrada", AppResource.Aceptar);
                    return;
                }

                myRec.SaldarFacturaInTemp(CurrentDocument, myParametro.GetParRecibosNoDescuentoAlSaldar(), CurrentMoneda.MonTasa);

                if (myParametro.GetParNotaCreditoAutoFactura() && !string.IsNullOrWhiteSpace(CurrentDocument.CXCNCF))
                {
                    var NCs = myRec.GetNotaCreditoAutomaticaParaFactura(CurrentDocument.Documento, CurrentDocument.CXCNCF).ToList();                    
                    foreach (var nc in NCs)
                    {
                        AplicarNC(new AplicarNCArgs() { NC = nc }, true);                    
                    }
                }

                myRec.CalcularDescuentoChkDiferidoADocumentosSaldados(withCalcDesc:true);

                CargarDocumentos();
            }
            catch (Exception e)
            {
                DisplayAlert(AppResource.ErrorPayingInvoice, e.Message, AppResource.Aceptar);
            }
        }

        private void EliminarFactura()
        {
            if (CurrentDocument.Estado == "Pendiente" && !CurrentDocument.IndicadorNotaCreditoAplicada) //y no tiene nota credito aplicada
            {
                return;
            }

            if(Arguments.Values.CurrentModule == Modules.ENTREGASREPARTIDOR)
            {
                DisplayAlert(AppResource.Warning, AppResource.CannotUncheckDocuments);
                return;
            }

            try
            {
                myRec.EliminarFacturaInTemp(CurrentDocument);

                CargarDocumentos();
            }
            catch (Exception e)
            {
                DisplayAlert(AppResource.ErrorRemovingInvoice, e.Message, AppResource.Aceptar);
            }
        }

        private void EliminarNC()
        {
            try
            {
                if (CurrentDocument.Estado != "Aplicada")
                {
                    DisplayAlert(AppResource.Warning, AppResource.DocumentIsNotApplied, AppResource.Aceptar);
                    return;
                }

                if (CurrentDocument.Estado == "Aplicada" && !string.IsNullOrWhiteSpace(CurrentDocument.CXCNCFAfectado)   && myParametro.GetParNotaCreditoAutoFactura())
                {
                    DisplayAlert(AppResource.Warning, AppResource.CannotRemoveDocumentBelongToCheckInvoice, AppResource.Aceptar);
                    return;
                }

                myRec.EliminarNCInTemp(CurrentDocument);

                DismissDialogs();

                CargarDocumentos();

            }
            catch (Exception e)
            {
                DisplayAlert(AppResource.ErrorRemovingCreditNote, e.Message, AppResource.Aceptar);
            }
        }

        public void DismissDialogs()
        {
            //ShowDialogNC = false;
            CurrentNotaCredito = null;
            CurrentDocument = null;

            if (ShowDetalleRecibo)
            {
                ShowDetalleRecibo = false;
                CargarDocumentos();
            }

        }

        public void AplicarNC(AplicarNCArgs datos)
        {
            AplicarNC(datos, false);
        }

        public async void AplicarNC(AplicarNCArgs datos, bool Automatica)
        {
            try
            {
                if (datos.NC == null)
                {
                    return;
                }


                if (FormasDePagoPermitidas != null && !FormasDePagoPermitidas.PermiteNotaCredito)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.CannotApplyCreditNotesToThisCustomer, AppResource.Aceptar);
                    return;
                }

                RecibosDocumentosTemp factura;
                

                if (Automatica)
                {
                    //factura = myRec.GetFacturaConAplicadoMayor(CurrentDocument.MontoTotal, datos.NC.CXCNCFAfectado);                   
                    if (myParametro.GetParNotaCreditoAutoFactura())
                    {
                        factura = myRec.GetFacturaAsociadaNotaCredito(CurrentDocument.Referencia);
                    }
                    else
                    {
                        factura = myRec.GetFacturaConAplicadoMayor(CurrentDocument.Balance, datos.NC.CXCNCFAfectado);
                    }

                    if (factura == null)
                    {
                        await DisplayAlert(AppResource.Warning, Automatica ? AppResource.CreditNoteCouldNotBeApplied : AppResource.MustSelectInvoice, AppResource.Aceptar);
                        return;
                    }

                    if (Math.Abs(datos.NC.Pendiente) > Math.Abs(factura.Balance))
                    {
                        datos.ValorAplicarManual = factura.Balance;
                    }
                    else if (datos.NC.CXCNCFAfectado != factura.CXCNCF && !string.IsNullOrWhiteSpace(datos.NC.CXCNCFAfectado))
                    {
                        return;
                    }
                    else
                    {
                        datos.ValorAplicarManual = Math.Abs(datos.NC.Pendiente);
                    }
                }
                else
                {
                    factura = datos.Factura;
                }

                if (factura == null)
                {
                    await DisplayAlert(AppResource.Warning, Automatica ? AppResource.CreditNoteCouldNotBeApplied : AppResource.MustSelectInvoice, AppResource.Aceptar);
                    return;
                }

                if (datos.NC.Balance == 0 || Math.Abs(datos.NC.Pendiente) <= 0)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.CreditNoteHasBeenFullyApplied);
                    return;
                }

                if (/*!myParametro.GetParRecibosSplitNotasDeCredito(datos.NC.Referencia) && */(Math.Abs(datos.ValorAplicarManual) + Math.Abs(factura.Credito)) > Math.Abs(factura.Balance))
                {
                    await DisplayAlert(AppResource.Warning, AppResource.CannotApplyValueGreaterThanBalance, AppResource.Aceptar);
                    return;
                }

                IsBusy = true;

                if (factura.Sigla == "FTN")
                {
                    datos.NC.MontoSinItbis = (datos.NC.Balance * datos.NC.MontoSinItbis) / datos.NC.MontoTotal;
                }

                if (myParametro.GetParRecAplicarNCByAutorizacion())
                {

                    int RecSecuencia = 0;
                    int RecSecuenciaParams = myParametro.GetParRecibosSecuenciaPorSector();
                    string AreaCtrlsubto = Arguments.Values.CurrentSector != null && Arguments.Values.CurrentClient != null ? myCli.GetareaCtrlCreditOfClienteDetalle(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentSector.SecCodigo).Substring(0, 2) : "";
                    if (RecSecuenciaParams >= 1 && myParametro.GetParRecibosPorSector())
                    {
                        RecSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Recibos-" + (RecSecuenciaParams == 2 ? AreaCtrlsubto : Arguments.Values.CurrentSector.SecCodigo));
                    }
                    else
                    {
                        RecSecuencia = myParametro.GetParRecibosRecTipoChkDiferidos() && myRec.ExistsChkDiferidos()
                            ? DS_RepresentantesSecuencias.GetLastSecuencia("Recibos-1")
                            : DS_RepresentantesSecuencias.GetLastSecuencia("Recibos");
                    }

                    var result = await DisplayAlert(AppResource.Warning, AppResource.AuthorizationIsNeededToApplyTheCreditNote, AppResource.Authorize, AppResource.Cancel);

                    if (result)
                    {
                        await Application.Current.MainPage.Navigation.PushModalAsync(new AutorizacionesModal(false, RecSecuencia, 3, null, false, false)
                        {
                            OnAutorizacionUsed = (autSec) =>
                            {
                                myRec.AplicarNotaCredito(datos.NC, factura, datos.ValorAplicarManual);

                                if (myParametro.GetParNotaCreditoAutoFactura())
                                {
                                    DisplayAlert(AppResource.Warning, AppResource.CreditNoteAppliedToInvoice.Replace("@", datos.NC.Documento), AppResource.Aceptar);
                                }
                                else if (!Automatica)
                                {
                                    dialogDetalleNC.ClearValues();
                                }

                                IsBusy = false;
                                DismissDialogs();
                                CargarDocumentos();
                            }
                        });
                        IsBusy = false;
                        return;
                    }
                    else
                    {
                        IsBusy = false;
                        return;
                    }
                }
                else
                {
                    Task.Run(() =>
                    {
                        myRec.AplicarNotaCredito(datos.NC, factura, datos.ValorAplicarManual);
                    }).Wait();
                }

                //await Task.Run(() =>
                //{
                //    myRec.AplicarNotaCredito(datos.NC, factura, datos.ValorAplicarManual);  ///
                //});
                

                if (myParametro.GetParNotaCreditoAutoFactura())
                {
                    await DisplayAlert(AppResource.Warning, AppResource.CreditNoteAppliedToInvoice.Replace("@", datos.NC.Documento), AppResource.Aceptar);
                }
                else if(!Automatica)
                {
                    dialogDetalleNC.ClearValues();
                }

               /// dialogDetalleNC.ClearValues();

                IsBusy = false;
                DismissDialogs();

                CargarDocumentos();

            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.ErrorApplyingCreditNote, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
        }

        public void AgregarFormaPago(AgregarFormaPagoArgs forma)
        {
            try
            {

                //int.TryParse(forma.NoCheque.Substring(12,4), out int numero);
                Int64.TryParse(forma.NoCheque, out Int64 numero);

                FormasPagoTemp value = new FormasPagoTemp();
                value.Banco = forma.Banco;

                int.TryParse(forma.BanID, out int banId);

                value.BanID = banId;
                value.Fecha = forma.Fecha.ToString("yyyy-MM-dd HH:mm:ss");
                value.Valor = forma.Valor;
                value.Prima = forma.Prima;
                value.AutSecuencia = forma.AutSecuencia;

                if (forma.Moneda != null)
                {
                    value.MonCodigo = forma.Moneda.MonCodigo;
                    value.Tasa = forma.Moneda.MonTasa;
                }
                else if (CurrentMoneda != null)
                {
                    value.MonCodigo = CurrentMoneda.MonCodigo;
                    value.Tasa = CurrentMoneda.MonTasa;
                }

                value.RefSecuencia = myRec.GetLastRefSecuenciaInTemp() + 1;
                value.rowguid = Guid.NewGuid().ToString();
                value.Futurista = forma.Futurista ? "Si" : "No";
                value.NoCheque = numero;

                switch (CurrentFormaPago)
                {
                    case FormasPago.Cheque:
                        value.ForID = 2;
                        value.FormaPago = "Cheque";
                        break;
                    case FormasPago.Transferencia:
                        value.ForID = 4;
                        value.FormaPago = "Transferencia";
                        break;
                    case FormasPago.TarjetaCredito:
                        value.ForID = 6;
                        value.FormaPago = "TARJETA";//"Tarjeta de crédito";
                        value.TipTarjeta = forma.TipTarjeta;
                        break;
                    case FormasPago.Efectivo:
                        value.ForID = 1;
                        value.FormaPago = "Efectivo";
                        break;
                    case FormasPago.OrdenPago:
                        value.ForID = 18;
                        value.FormaPago = "Orden pago";
                        break;
                    case FormasPago.Retencion:
                        value.ForID = 5;
                        value.FormaPago = "RETENCION";
                        break;
                    case FormasPago.DiferenciaCambiaria:
                        value.ForID = 8;
                        value.FormaPago = "Diferencia Cambiaria";
                        break;
                    case FormasPago.Redondeo:
                        value.ForID = 9;
                        value.FormaPago = "Redondeo";
                        break;

                }

                myRec.AgregarFormaPago(value);

                if (CurrentFormaPago == FormasPago.Cheque || CurrentFormaPago == FormasPago.OrdenPago && forma.Futurista)
                {
                    int DiasParaDescuento = -1;
                    if (currentdocument != null)
                    {
                        //DateTime dt = DateTime.ParseExact(CurrentDocument.FechaEntrega, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                        bool dtD = DateTime.TryParseExact(CurrentDocument.FechaEntrega, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None,  out DateTime dt);
                        if (!dtD)
                        {
                            throw new Exception("El Campo FechaEntrega esta nulo");
                        }
                        string fechafactura = dt.ToString("yyyy-MM-dd");
                        DateTime date = DateTime.Parse(value.Fecha);//Fecha diferida
                        DateTime facFecha = DateTime.Parse(fechafactura);//Fecha Actual
                        DiasParaDescuento = Math.Abs((int)(facFecha - date).TotalDays);
                    }

                    if (currentdocument == null)
                    {
                        DateTime dt = DateTime.Now;
                        string fechafactura = dt.ToString("yyyy-MM-dd");
                        DateTime date = DateTime.Parse(value.Fecha);//Fecha diferida
                        DateTime facFecha = DateTime.Parse(fechafactura);//Fecha Actual
                        DiasParaDescuento = Math.Abs((int)(facFecha - date).TotalDays);
                    }

                    //if (!new DS_DescuentoFacturas().IsValidDescuentoFactura(CurrentDocument.Referencia,
                       // DiasParaDescuento))
                    //{
                        myRec.CalcularDescuentoChkDiferidoADocumentosSaldados(withCalcDesc: true, isForAgregarFormaPago: true);
                   // }

                    ReloadDocuments = true;

                    if (myRec.ExistsFacturasSaldadasConDescuento() || (CurrentFormaPago == FormasPago.Cheque && forma.Futurista))
                    {
                        if (CurrentFormaPago == FormasPago.OrdenPago)
                        {
                            AlertDescuentoOrderPago();
                        }
                        else
                        {
                            AlertDescuentoChkDiferido();
                        }

                        return;
                    }
                }

                dialogAddFormaPago.Dismiss();

                CargarFormasPago();

            }
            catch (Exception e)
            {
                DisplayAlert(AppResource.ErrorAddingPaymentway, e.Message, AppResource.Aceptar);
            }

        }

        private async void AlertDescuentoChkDiferido()
        {
            dialogAddFormaPago.Dismiss();
            await DisplayAlert(AppResource.Warning, AppResource.SomeInvoicesNotApplyForDiscountForDateOfDeposit);
            CargarFormasPago();
        }

        private async void AlertDescuentoOrderPago()
        {
            dialogAddFormaPago.Dismiss();
            await DisplayAlert(AppResource.Warning, AppResource.SomeInvoicesNotApplyForDiscountForPaymentOrderDate);
            CargarFormasPago();
        }

        public async void AceptarDetalleFactura(RecibosDocumentosDetalleArgs args)
        {
            try
            {
                if (Arguments.Values.CurrentModule == Modules.ENTREGASREPARTIDOR)
                {
                    ShowDetalleRecibo = false;
                    return;
                }

                if(Arguments.Values.CurrentModule == Modules.RECONCILIACION)
                {
                    ShowDetalleRecibo = false;
                    return;
                }

                if (myParametro.GetParRecNoSaldoWithAbonoDiferido() && myRec.ExistAbonadoDiferido(CurrentDocument.Referencia))
                {
                    await DisplayAlert(AppResource.Warning, AppResource.CannotPayBillHaveFuturisticCreditMemo, AppResource.Aceptar);
                    return;
                }

                if (myParametro.GetParRecibosTasaFacturas_O_TasaDia())
                {
                    if (myCxc.ValidarFacturasSaldadasNoVencidas(CurrentDocument.Referencia) > 0)
                    {
                        if (myCxc.GetTasaFactura(CurrentDocument.Referencia) > 0)
                        {
                            if (CurrentDocument.Tasa > myRec.GetMaxTasaAplicada(CurrentDocument.Referencia))
                            {
                                var resul = await DisplayAlert(AppResource.Warning, AppResource.RateChangeToUpper + myCxc.GetTasaFactura(CurrentDocument.Referencia), AppResource.Aceptar, AppResource.Cancel);
                                IsBusy = false;
                                if (resul)
                                {
                                    CurrentMoneda.MonTasa = myCxc.GetTasaFactura(CurrentDocument.Referencia);
                                   
                                }
                            }
                           
                        }
                        else
                        {
                            await DisplayAlert(AppResource.Warning, AppResource.TheRateOfTheDayWillBeUsed + LastTasa, AppResource.Aceptar);
                            CurrentMoneda.MonTasa = LastTasa;                            
                        }
                    }
                    else
                    {
                        if (myCxc.GetCountFacturasSaldadas() > 1)
                        {
                            if (myCxc.GetTasaFactura(CurrentDocument.Referencia) > 0)
                            {
                                if (CurrentDocument.Tasa > myRec.GetMaxTasaAplicada(CurrentDocument.Referencia))
                                {
                                    var resul = await DisplayAlert(AppResource.Warning, AppResource.RateChangeToUpper + myCxc.GetTasaFactura(CurrentDocument.Referencia), AppResource.Aceptar, "Cancelar");
                                    IsBusy = false;
                                    if (resul)
                                    {
                                        CurrentMoneda.MonTasa = myCxc.GetTasaFactura(CurrentDocument.Referencia);
                                        
                                    }
                                }                               
                            }
                            else
                            {
                                await DisplayAlert(AppResource.Warning, AppResource.TheRateOfTheDayWillBeUsed + LastTasa, AppResource.Aceptar);
                                CurrentMoneda.MonTasa = LastTasa;                                
                            }
                        }
                        else
                        {
                            await DisplayAlert(AppResource.Warning, AppResource.TheRateOfTheDayWillBeUsed + LastTasa, AppResource.Aceptar);
                            CurrentMoneda.MonTasa = LastTasa;
                        }
                    }
                }

                myRec.UpdateIndicadorCalcularDescuentoInTemp(args.Factura.Referencia, args.CalcularDesc, args.RecVerificarDesc);
                args.Factura.CalcularDesc = args.CalcularDesc;
                args.Factura.CalcularDesmonte = args.CalcularDesmonte;
                args.Factura.Desmonte = args.Desmonte;

                if (args.IsForSaldo)
                {
                    myRec.SaldarFacturaInTemp(args.Factura, !args.Factura.CalcularDesc, args.Descuento);
                }
                else
                {
                    myRec.AbonarFacturaInTemp(args.Factura, args.Aplicado, !args.Factura.CalcularDesc, args.Descuento);
                }

                if (args.CalcularDesc && (args.IsForSaldo || myParametro.GetParDescuentoAbonos()))
                {
                    myRec.CalcularDescuentoChkDiferidoADocumentosSaldados(withCalcDesc:true);
                }

                ShowDetalleRecibo = false;
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.ErrorAcceptingInvoice, e.Message, AppResource.Aceptar);
            }

            CargarDocumentos();
        }

        public async void AlertaTaza()
        {
            IsBusy = false;

            var result = await DisplayAlert(AppResource.Warning, AppResource.CurrentRateIs + CurrentMoneda.MonTasa + ", " + AppResource.WantChangeRateQuestion, AppResource.Yes, AppResource.No);
            if (result)
            {
                dialogAgregarTaza = new AgregarTazaModal(DS_RepresentantesParametros.GetInstance().GetParRecibosAutorizacionTazaFactura(), DS_RepresentantesSecuencias.GetLastSecuencia("Recibos"), CurrentMoneda)
                {
                    CurrentMoneda = (dialogAgregarTaza != null ? dialogAgregarTaza.GetTazaMoneda() : CurrentMoneda)
                };
                await PushModalAsync(dialogAgregarTaza);
            }

        }

        private void ChkDiferidoGeneralChanged(bool check)
        {       
            if (FormasPagoSource == null || FormasPagoSource.Count == 0)
            {
                CargarDocumentos(IsFirstChkDiferidoGeneral, check? FechaMinimaChkDiferido.ToString("yyyy-MM-dd") : DateTime.Now.ToString("yyyy-MM-dd"));
                return;
            }

            foreach(var forma in FormasPagoSource)
            {
                myRec.EliminarFormaPagoInTemp(forma);

                if (IsEditing)
                {
                    myRec.UpdateRefValorRecDocTemp(forma.RefSecuencia, VenSecuencia);
                }
            }
            
            if (!IsEditing)
            {
                myRec.CalcularDescuentosFacturasInTemp();
            }

            CargarFormasPago();

            CargarDocumentos(IsFirstChkDiferidoGeneral, FechaMinimaChkDiferido.ToString("yyyy-MM-dd"));

            ReloadDocuments = true;
        }

        public async void ChekAndUpdateTasa(double value_Tasa)
        {
            if (myCxc.ValidarFacturasSaldadasNoVencidas(CurrentDocument.Referencia) > 0)
            {
                if (myCxc.GetTasaFactura(CurrentDocument.Referencia) > 0)
                {
                    var resul = await DisplayAlert(AppResource.Warning, AppResource.RateWillBeUsed + myCxc.GetTasaFactura(CurrentDocument.Referencia), AppResource.Aceptar, AppResource.Cancel);
                    IsBusy = false;
                    if (resul)
                    {
                        CurrentMoneda.MonTasa = myCxc.GetTasaFactura(CurrentDocument.Referencia);
                    }
                }
                else
                {
                    await DisplayAlert(AppResource.Warning, AppResource.TheRateOfTheDayWillBeUsed + value_Tasa, AppResource.Aceptar);
                    CurrentMoneda.MonTasa = value_Tasa;
           
                }
                
            }
            else
            {
                if (myCxc.GetCountFacturasSaldadas() > 1)
                {
                    if (myCxc.GetTasaFactura(CurrentDocument.Referencia) > 0) {
                        var resul =  await DisplayAlert(AppResource.Warning, AppResource.RateWillBeUsed + myCxc.GetTasaFactura(CurrentDocument.Referencia), AppResource.Aceptar, "Cancelar");
                        IsBusy = false;
                        if (resul)
                        {
                            CurrentMoneda.MonTasa = myCxc.GetTasaFactura(CurrentDocument.Referencia);
                   
                        }
                    }
                    else
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.TheRateOfTheDayWillBeUsed + value_Tasa, AppResource.Aceptar);
                        CurrentMoneda.MonTasa = value_Tasa;
                   
                    }
                }
                else
                {
                    await DisplayAlert(AppResource.Warning, AppResource.TheRateOfTheDayWillBeUsed + value_Tasa, AppResource.Aceptar);
                    CurrentMoneda.MonTasa = value_Tasa;
              

                }

            }
    
        }

        public async Task SaldarFacturaByCommand()
        {
            if (myParametro.GetParRecibosTasaFacturas_O_TasaDia())
            {
                if (myCxc.ValidarFacturasSaldadasNoVencidas(CurrentDocument.Referencia) > 0)
                {
                    if (myCxc.GetTasaFactura(CurrentDocument.Referencia) > 0)
                    {
                        if (CurrentDocument.Tasa > myRec.GetMaxTasaAplicada(CurrentDocument.Referencia))
                        {
                            var resul = await DisplayAlert(AppResource.Warning, AppResource.RateChangeToUpper + myCxc.GetTasaFactura(CurrentDocument.Referencia), AppResource.Aceptar, AppResource.Cancel);
                            IsBusy = false;
                            if (resul)
                            {
                                CurrentMoneda.MonTasa = myCxc.GetTasaFactura(CurrentDocument.Referencia);
                                SaldarFactura();
                            }
                        }
                        else
                        {
                            //CurrentMoneda.MonTasa = myCxc.GetTasaFactura(CurrentDocument.Referencia);
                            SaldarFactura();
                        }
                    }
                    else
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.TheRateOfTheDayWillBeUsed + LastTasa, AppResource.Aceptar);
                        CurrentMoneda.MonTasa = LastTasa;
                        SaldarFactura();
                    }

                }
                else
                {
                    if (myCxc.GetCountFacturasSaldadas() > 1)
                    {
                        if (myCxc.GetTasaFactura(CurrentDocument.Referencia) > 0)
                        {
                            if (CurrentDocument.Tasa > myRec.GetMaxTasaAplicada(CurrentDocument.Referencia))
                            {
                                var resul = await DisplayAlert(AppResource.Warning, AppResource.RateChangeToUpper + myCxc.GetTasaFactura(CurrentDocument.Referencia), AppResource.Aceptar, AppResource.Cancel);
                                IsBusy = false;
                                if (resul)
                                {
                                    CurrentMoneda.MonTasa = myCxc.GetTasaFactura(CurrentDocument.Referencia);
                                    SaldarFactura();
                                }
                            }
                            else
                            {
                                //CurrentMoneda.MonTasa = myCxc.GetTasaFactura(CurrentDocument.Referencia);
                                SaldarFactura();
                            }
                        }
                        else
                        {
                            await DisplayAlert(AppResource.Warning, AppResource.TheRateOfTheDayWillBeUsed + LastTasa, AppResource.Aceptar);
                            CurrentMoneda.MonTasa = LastTasa;
                            SaldarFactura();
                        }
                    }
                    else
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.TheRateOfTheDayWillBeUsed + LastTasa, AppResource.Aceptar);
                        CurrentMoneda.MonTasa = LastTasa;

                        SaldarFactura();
                    }

                }

            }
            else
            {

                SaldarFactura();
            }
        }

    }
}
