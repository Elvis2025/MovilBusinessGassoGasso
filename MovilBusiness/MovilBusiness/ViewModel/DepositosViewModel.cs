using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.model.Internal.Structs.Args;
using MovilBusiness.Printer;
using MovilBusiness.printers.formats;
using MovilBusiness.Utils;
using Plugin.Geolocator;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using MovilBusiness.Views;
using MovilBusiness.views;
using System.ComponentModel;
using MovilBusiness.Views.Components.Modals;
using MovilBusiness.Resx;

namespace MovilBusiness.viewmodel
{
    public class DepositosViewModel : BaseViewModel
    {
        public ICommand SaveDepositoCommand { get; private set; }
        public ICommand VerRecibosCommand { get; private set; }
        public ICommand GoFotoCommand { get; private set; }

        public List<UsosMultiples> TiposDepositos { get; private set; }
        public List<CuentasBancarias> CuentasBancarias { get; private set; }

        private UsosMultiples currenttipodeposito;
        public UsosMultiples CurrentTipoDeposito { get => currenttipodeposito; set { currenttipodeposito = value; RaiseOnPropertyChanged(); ConfigTipoDeposito(); } }

        public CuentasBancarias CurrentCuenta{ get;set; }

        private bool enabledbanco = false;
        public bool EnabledBanco { get => enabledbanco; set { enabledbanco = value; RaiseOnPropertyChanged(); } }

        private bool showprinter = false;
        public bool ShowPrinter { get => showprinter; set { showprinter = value; RaiseOnPropertyChanged(); } }

        public bool includeordenpago = false;
        public bool IncludeOrdenPago { get => includeordenpago; set { includeordenpago = value; RaiseOnPropertyChanged(); } }

        public string NumeroDeposito { get; set; }
        private string sector = null;
        private bool isSociedad = false;
        private int _depsecuencia = -1;
        private DS_Depositos myDep;

        //private DepositosArgs args;
        public DepositosArgs Args { get; set; } 

        private string totaldepositolabel;
        public string TotalDepositoLabel{ get => totaldepositolabel; set { totaldepositolabel = value; RaiseOnPropertyChanged();} }

        private double TotalDeposito; 

        private int CurrentDepositoSecuencia = -1;
        private string CurrentMonCodigo;

        public bool ShowMoneda { get => !string.IsNullOrWhiteSpace(CurrentMonCodigo) && !string.IsNullOrWhiteSpace(CurrentMonedaSigla); }

        private string currentmonedasigla;
        public string CurrentMonedaSigla { get => currentmonedasigla; set { currentmonedasigla = value; RaiseOnPropertyChanged(); } }

        private DepositosFormats DepositoPrinter;
        public DateTime dt1;
        public DateTime dt2;

        public DepositosViewModel(Page page, string sector = null, bool isSociedad = false, string monCodigo = null, bool includeOrdenPago = false, int depsecuencia = -1, bool isconfirmado = false, bool isFromConteo = false) : base(page)
        {
            SaveDepositoCommand = new Command(() => GuardarDeposito(isFromConteo: isFromConteo));
            VerRecibosCommand = new Command(VerRecibos);
            GoFotoCommand = new Command(GoPhoto);
            myDep = new DS_Depositos();
            Args = new DepositosArgs();
            _depsecuencia = depsecuencia;

            if (depsecuencia > 0)
            {
                TiposDepositos = new List<UsosMultiples>();
                CuentasBancarias = new List<CuentasBancarias>();
                var deposito = new DS_Depositos().GetDepositobySecuenciaForVerDetalle(depsecuencia, isconfirmado);
                if(deposito != null)
                {
                    EnabledBanco = false;
                    NumeroDeposito = deposito.DepNumero.ToString();
                    CurrentMonCodigo = deposito.MonCodigo;
                    TiposDepositos.Add(new UsosMultiples { CodigoGrupo = deposito.CodigoGrupo, CodigoUso = deposito.CodigoUso, Descripcion = string.IsNullOrEmpty(deposito.usoDescripcion)? " " : deposito.usoDescripcion });
                    CurrentTipoDeposito = TiposDepositos[0];
                    this.sector = deposito.SocCodigo;
                    Arguments.Values.CurrentSector = new DS_Sectores().GetSectorByCodigo(deposito.SocCodigo);
                    this.isSociedad = isSociedad;
                    CuentasBancarias.Add(new CuentasBancarias { CuBNombre = string.IsNullOrEmpty(deposito.CuBNombre)? " ": deposito.CuBNombre, CuBID = deposito.CuBID });
                    CurrentCuenta = CuentasBancarias[0];
                }
            }
            else
            {
                CurrentMonCodigo = monCodigo;
                TiposDepositos = new DS_UsosMultiples().GetTiposDepositos();
                this.sector = sector;
                Arguments.Values.CurrentSector = new DS_Sectores().GetSectorByCodigo(sector);
                this.isSociedad = isSociedad;
                CuentasBancarias = new DS_CuentasBancarias().GetCuentasBancarias(!isSociedad ? sector : null, (CurrentMonCodigo != "" ? CurrentMonCodigo : ""));
            }


            DepositoPrinter = new DepositosFormats(myDep);
            var location = CrossGeolocator.Current;
            if (!location.IsListening && myParametro.GetParGPS())
            {             
                location.PositionChanged += (sender, a) => { Arguments.Values.CurrentLocation = new Location(a.Position.Latitude, a.Position.Longitude); };
                location.StartListeningAsync(TimeSpan.FromSeconds(20), 15);
            }

            if (!string.IsNullOrWhiteSpace(CurrentMonCodigo))
            {
                var moneda = new DS_Monedas().GetMoneda(CurrentMonCodigo);

                if(moneda != null)
                {
                    CurrentMonedaSigla = moneda.MonNombre;
                }
            }
        }

        private void GoPhoto()
        {
            try
            {
                var DepSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Depositos");
                
                PushAsync(new CameraPage(DepSecuencia.ToString(), "Depositos"));

            }catch (Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }
        }

        private async void VerRecibos()
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                if (CurrentTipoDeposito == null)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.MustSelectDepositType, AppResource.Aceptar);
                    return;
                }

                IsBusy = true;
                List<Recibos> recSecuencias;
                if (_depsecuencia > 0)
                {
                     recSecuencias = myDep.GetRecibosParaDepositarForVerDetalle(CurrentTipoDeposito.CodigoUso, sector, isSociedad, CurrentMonCodigo, includeOrdenPago: IncludeOrdenPago, tiposrecibos: (CurrentTipoDeposito.CodigoUso == "1" ? DS_RepresentantesParametros.GetInstance().GetParDepositosFormasPago() : null), _depsecuencia);
                }else
                {
                    recSecuencias = myDep.GetRecibosParaDepositar(CurrentTipoDeposito.CodigoUso, sector, isSociedad, CurrentMonCodigo, includeOrdenPago: IncludeOrdenPago, tiposrecibos: (CurrentTipoDeposito.CodigoUso == "1" ? DS_RepresentantesParametros.GetInstance().GetParDepositosFormasPago() : null));
                }
               // var RecibosDisponiblesParaVer = new List<Recibos>();
                
               /* foreach (var rec in recSecuencias) {

                    DateTime dt1 = DateTime.Now.Date;
                    DateTime dt2 = Convert.ToDateTime(rec.RefFecha).Date;
                    if (dt2 <= dt1)
                    {
                        RecibosDisponiblesParaVer.Add(rec);
                    }
                }*/

                if (recSecuencias == null || recSecuencias.Count == 0)
                {
                    throw new Exception(AppResource.NoReceiptToView);
                }
                
                await PushAsync(new RecibosDepositosViewerModal(recSecuencias));

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        private async void GuardarDeposito(bool validatePhotoDeposit = false, bool isFromConteo = false)
        {
            try { 

                if (IsBusy)
                {
                    return;
                }

                if(CurrentTipoDeposito == null)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.MustSelectDepositType, AppResource.Aceptar);
                    return;
                }

                if(CurrentTipoDeposito.CodigoUso == "1") //banco
                {
                    if(CurrentCuenta == null)
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.MustSelectBankAccount, AppResource.Aceptar);
                        return;
                    }

                    int.TryParse(NumeroDeposito, out int Numero);

                    if(Numero == 0 || string.IsNullOrWhiteSpace(NumeroDeposito))
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.MustEnterValidDepositNumber, AppResource.Aceptar);
                        return;
                    }
                }

                if (myParametro.GetParGPS() && Arguments.Values.CurrentLocation == null)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.NoGeolocationMessage, AppResource.Aceptar);
                    return;
                }


                bool ParDepCapturarFoto = myParametro.GetParDepositosCapturarFoto();
                if (ParDepCapturarFoto)
                {
                    var DepSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Depositos");
                    var myTraImg = new DS_TransaccionesImagenes();
                    int cantidadFotos = myTraImg.GetCantidadImagenesInTemp("Depositos", DepSecuencia.ToString());
                    if (cantidadFotos < 1)
                    {
                        //var aut = await Functions.DisplayAlert(AppResource.Warning, "No has realizado la foto del deposito, de no tomar la foto luego requerira una autorizacion, deseas continuar de todos modos?", "Continuar", "Cancelar");

                        if (!validatePhotoDeposit && DepSecuencia > -1)
                        {
                            var aut = await Functions.DisplayAlert(AppResource.Warning, AppResource.DepositPhotoNotTakenMessage, AppResource.Authorize, AppResource.Aceptar);

                            if (aut)
                            {
                                await Application.Current.MainPage.Navigation.PushModalAsync(new AutorizacionesModal(false, DepSecuencia, 9, "")
                                {
                                    OnAutorizacionUsed = (autSec) =>
                                    {
                                        GuardarDeposito(true);
                                    }
                                });
                            }
                            return;
                        }
                    }
                }
              

                var recibos = myDep.GetRecibosParaDepositar(CurrentTipoDeposito.CodigoUso, sector, isSociedad, CurrentMonCodigo, includeOrdenPago: IncludeOrdenPago, tiposrecibos: (CurrentTipoDeposito.CodigoUso == "1" ? DS_RepresentantesParametros.GetInstance().GetParDepositosFormasPago() : null));

                if(recibos == null || recibos.Count == 0)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.NoReceiptsToDeposit, AppResource.Aceptar);
                    return;
                }

                TaskLoader task = new TaskLoader() { SqlTransactionWhenRun = true };

                int.TryParse(NumeroDeposito, out int numero);

                Args.Numero = numero;

                Args.Referencia = "";// EditReferencia.Text.Trim();
                int.TryParse(CurrentTipoDeposito.CodigoUso, out int tipo);
                Args.Tipo = tipo;
                Args.RecibosADepositar = recibos;
                Args.CuBID = CurrentTipoDeposito != null && CurrentTipoDeposito.CodigoUso == "1" && CurrentCuenta != null ? CurrentCuenta.CuBID : 0;
                Args.location = Arguments.Values.CurrentLocation;
                Args.SocCodigo = sector;
                Args.MonCodigo = CurrentMonCodigo;

                //Monto de orden de pago agregado para que no muestre esta alerta en caso de que solo sean Ordenes de Pago
                if (!includeordenpago)
                {
                    Args.MontoOrdenPago = 0;
                }
                if (Args.MontoChk == 0 && Args.MontoChkFut == 0 && Args.MontoEfectivo == 0 && Args.MontoTarjeta == 0 && Args.MontoTransferencia == 0 && Args.MontoRetencion == 0 && Args.MontoOrdenPago == 0)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.DepositAmountMustBeGreaterThanZero, AppResource.Aceptar);
                    return;
                }

                IsBusy = true;

                await task.Execute(() => 
                {
                    CurrentDepositoSecuencia = myDep.GuardarDeposito(Args);

                    if (ParDepCapturarFoto)
                    {
                        var imagenesTomadas =new DS_TransaccionesImagenes().MarkToSendToServer("Depositos", CurrentDepositoSecuencia.ToString());

                        if(imagenesTomadas > 0)
                        {
                            myDep.ActualizarIndicadorDepositoFoto(CurrentDepositoSecuencia);
                        }
                    }

                });


                IsBusy = false;

                if (myParametro.GetParDepositoconConteo() && !myParametro.GetParDepositoNoCerrarCuadre() && isFromConteo)
                {
                    await PopAsync(false);
                    HomeViewModel.getInstance().GoAbrirCerrarCuadres(true);
                }
                else
                {
                    Arguments.Values.CurrentModule = Enums.Modules.DEPOSITOS;
                    DepositosPage.Finish = true;
                   // await PopAsync(false);
                    await PushAsync(new SuccessPage(AppResource.DepositSavedUpper, CurrentDepositoSecuencia, Ispreliminar: false));
                }

            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.ErrorSavingDeposit, e.InnerException != null ? e.InnerException.Message : e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
        }

        private Task Imprimir()
        {
            return Task.Run(() =>
            {
                var printer = new PrinterManager(sector);

                DepositoPrinter.Print(CurrentDepositoSecuencia, false, printer);

            });
        }

        private void ConfigTipoDeposito()
        {
            EnabledBanco = CurrentTipoDeposito.CodigoUso == "1";
            CalcularDeposito();
        }

        public void CalcularDeposito(bool includeOrdenPago = false, string tipodeposito = null,  string tiposrecibos = null)
        {
            Args.MontoChk = 0.0;
            Args.MontoChkFut = 0.0;
            Args.MontoEfectivo = 0.0;
            Args.MontoOrdenPago = 0.0;
            Args.MontoRetencion = 0.0;
            Args.MontoTarjeta = 0.0;
            Args.MontoTransferencia = 0.0;

            if (CurrentTipoDeposito == null)
            {
                return;
            }
            try
            {
                TotalDeposito = 0;
                var recibos = myDep.GetRecibosMontoForDeposito(CurrentTipoDeposito.CodigoUso, sector, isSociedad, CurrentMonCodigo, _depsecuencia);

                foreach (var rec in recibos)
                {

                    switch (rec.ForID)
                    {
                        case 1: //EFECTIVO    
                            if ((tiposrecibos != null && tiposrecibos.Contains("E")) || tiposrecibos == null)
                            {
                                Args.MontoEfectivo = rec.RefValor;
                                TotalDeposito += rec.RefValor;
                            }else if(tiposrecibos != null)
                            {
                                Args.MontoEfectivo = 0.0;                                
                            }
                                                       
                            break;
                        case 2: //CHEQUE
                            if (rec.RefIndicadorDiferido)
                            {
                                if ((tiposrecibos != null && tiposrecibos.Contains("CF")) || tiposrecibos == null )
                                {
                                    TotalDeposito += rec.RefValor;
                                    Args.MontoChkFut = rec.RefValor;
                                }
                                else if (tiposrecibos != null)
                                {
                                Args.MontoChkFut = 0.0;
                                }                            
                            }
                            else
                            {
                                if ((tiposrecibos != null && tiposrecibos.Contains("CH")) || tiposrecibos == null)
                                {
                                    TotalDeposito += rec.RefValor;
                                    Args.MontoChk = rec.RefValor;
                                }
                                else if (tiposrecibos != null)
                                {
                                Args.MontoChk = 0.0;
                                }                           
                            }
                            break;
                        case 3: //NOTA CREDITO                            
                            //    TotalDeposito -= rec.RefValor;
                            break;
                        case 4: //TRANSFERENCIA
                            if ((tiposrecibos != null && tiposrecibos.Contains("TF")) || tiposrecibos == null)
                            {
                                TotalDeposito += rec.RefValor;
                                Args.MontoTransferencia = rec.RefValor;
                            }
                            else if (tiposrecibos != null)
                            {
                                Args.MontoTransferencia = 0.0;
                            }                           
                            break;
                        case 5: //RETENCION   
                            if ((tiposrecibos != null && tiposrecibos.Contains("R")) || tiposrecibos == null)
                            {
                                TotalDeposito += rec.RefValor;
                                Args.MontoRetencion = rec.RefValor;
                            }
                            else if (tiposrecibos != null)
                            {
                                Args.MontoRetencion = 0.0;
                            }                           
                            break;
                        case 6: //TARJETA DE CREDITO
                            if ((tiposrecibos != null && tiposrecibos.Contains("TC")) || tiposrecibos == null)
                            {
                                TotalDeposito += rec.RefValor;
                                Args.MontoTarjeta = rec.RefValor;
                            }
                            else if (tiposrecibos != null)
                            {
                                Args.MontoTarjeta = 0.0;
                            }                            
                            break;                        
                    }
                }

                var ordenesPagoRecibos = myDep.GetRecibosOrdenPagoForDeposito(CurrentTipoDeposito.CodigoUso, sector, isSociedad, CurrentMonCodigo);
                foreach(var rec in ordenesPagoRecibos)
                {
                    if (includeOrdenPago)
                    {                   
                        TotalDeposito += rec.RefValor;
                    }
                        Args.MontoOrdenPago = rec.RefValor;
                   
                }
                
                RaiseOnPropertyChanged("Args");
                TotalDepositoLabel = TotalDeposito.ToString("N2");
                
            } catch(Exception e)
            {
                DisplayAlert(AppResource.ErrorCalculatingDeposit, e.Message, AppResource.Aceptar);
            }      
            
        }
        public bool isFromEfectivo(int RecSecuencia)
        {
            List<Recibos> list = new List<Recibos>();

            try
            {
                list = SqliteManager.GetInstance().Query<model.Recibos>("Select RecSecuencia " +
                    "from recibos where Recibos.RecSecuencia = " + RecSecuencia + " and RecMontoEfectivo > 0", new string[] { });

            }catch(Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message);
            }
            return list.Count > 0;
        }

        public void CancelarImpresion()
        {
            ShowPrinter = false;
            PopAsync(false);
        }

        public async void AceptarImpresion(int Copias)
        {
            try
            {

                for (int x = 0; x < Copias; x++)
                {
                    IsBusy = true;

                    await Imprimir();

                    IsBusy = false;

                    if (Copias > 1 && x != Copias - 1)
                    {
                        await DisplayAlert(AppResource.PrintCopy, AppResource.CutPapelMessage, AppResource.Print);
                    }

                }
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.ErrorPrinting, e.Message, AppResource.Aceptar);
            }
            IsBusy = false;
            ShowPrinter = false;
            await PopAsync(false);
        }
    }
}
