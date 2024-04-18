using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Internal;
using MovilBusiness.model.Internal;
using MovilBusiness.model.Internal.Structs.Args;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using MovilBusiness.Views.Components.Modals;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class RecibosPushMoneyViewModel : BaseViewModel
    {
        public ICommand AddFormaPagoCommand { get; private set; }

        private ObservableCollection<RecibosDocumentosTemp> documentos;
        public ObservableCollection<RecibosDocumentosTemp> Documentos { get => documentos; set { documentos = value; RaiseOnPropertyChanged(); } }

        private double totalapagar = 0;
        public double TotalAPagar { get => totalapagar; set { totalapagar = value; RaiseOnPropertyChanged(); } }
        
        private ObservableCollection<FormasPagoTemp> formaspagoagregadas;
        public ObservableCollection<FormasPagoTemp> FormasPagoAgregadas { get => formaspagoagregadas; set { formaspagoagregadas = value; RaiseOnPropertyChanged(); } }

        private DS_PushMoneyPorPagar myPxp;
        private DS_FormasPago myFor;
        private DS_PushMoneyPagos myRec;

        private Action reciboGuardado;

        public DependientesViewModel DependientesViewModel { get; private set; }

        public RecibosPushMoneyViewModel(Page page, Action reciboGuardado) : base(page)
        {
            this.reciboGuardado = reciboGuardado;

            SaveCommand = new Command(() =>
            {
                Guardar();

            }, () => IsUp);

            AddFormaPagoCommand = new Command(ShowAlertAddFormaPago);

            myPxp = new DS_PushMoneyPorPagar();
            myFor = new DS_FormasPago();
            myRec = new DS_PushMoneyPagos();

            FormasPagoAgregadas = new ObservableCollection<FormasPagoTemp>();

            DependientesViewModel = new DependientesViewModel(page, new DS_UsosMultiples(), null);
        }

        public void SeleccionarDocumento(RecibosDocumentosTemp item)
        {
            try
            {
                var newItem = item.Copy();
                newItem.Estado = item.Estado == "Aplicada" ? "Pendiente" : "Aplicada";

                var index = Documentos.IndexOf(item);

                Documentos[index] = newItem;

                TotalAPagar = GetTotalAPagar();
            }catch(Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message);
            }
        }

        public async void OnFormaPagoSelected(FormasPagoTemp item)
        {
            var result = await DisplayAlert(AppResource.Warning, AppResource.WantDeletePaymentwayQuestion, AppResource.Remove, AppResource.Cancel);

            if (result)
            {
                FormasPagoAgregadas.Remove(item);

                TotalAPagar = GetTotalAPagar();
            }
        }

        private AgregarFormaPagoModal dialogAddFormaPago;
        private FormasPago CurrentFormaPago = FormasPago.Null;
        private async void ShowAlertAddFormaPago()
        {
            try
            {
                var list = new List<string>();

                list.Add("Efectivo");
                list.Add("Bono");

                var result = await DisplayActionSheet(AppResource.ChoosePaymentway, buttons: list.ToArray());

                if (dialogAddFormaPago == null)
                {
                    dialogAddFormaPago = new AgregarFormaPagoModal(new DS_Recibos())
                    {
                        FillMonto = () =>
                        {
                            return Math.Abs(GetTotalAPagar());
                        },
                        OnAccepted = AgregarFormaPago
                    };
                }

                CurrentFormaPago = FormasPago.Null;

                switch (result)
                {
                    case "Cheque":
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
                    case "Bono":
                        CurrentFormaPago = FormasPago.Bono;
                        break;
                    default:
                        return;
                }
                dialogAddFormaPago.CurrentFormaPago = CurrentFormaPago;
                dialogAddFormaPago.MontoAPagar = TotalAPagar; //< 0 ? TotalAPagar : 0 ;
                await PushAsync(dialogAddFormaPago);

            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }
        }

        public async void CargarDocumentos()
        {
            IsBusy = true;

            try
            {
                await Task.Run(() => 
                {
                    Documentos = new ObservableCollection<RecibosDocumentosTemp>(myPxp.GetAllPushMoneyPendientes());
                });

                TotalAPagar = GetTotalAPagar();

            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        private double GetTotalAPagar()
        {
            try
            {
                double aplicado = Documentos != null ? Documentos.Where(x => x.Estado == "Aplicada").Sum(x => x.Pendiente) : 0;

                double totalFormaPago = FormasPagoAgregadas.Sum(x => x.Prima);

                return totalFormaPago - aplicado;
            }
            catch (Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message);
            }

            return 0;
        }

        private async void Guardar()
        {
            IsUp = false;

            try
            {
                if (FormasPagoAgregadas.Count == 0)
                {
                    IsUp = true;
                    await DisplayAlert(AppResource.Warning, AppResource.NotAddedAnyPaymentway);
                    return;
                }

                if (Documentos.Where(x => x.Estado == "Aplicada").ToList().Count == 0 && !myParametro.GetParNoObligarFacturasEnPushMoney())
                {
                    IsUp = true;
                    await DisplayAlert(AppResource.Warning, AppResource.NotSelectedAnyInvoice);
                    return;
                }

                if(DependientesViewModel.CurrentDependiente == null || DependientesViewModel.CurrentDependiente.ClDNombre == "(Seleccione)" || DependientesViewModel.CurrentDependiente.ClDNombre == "(Agregar Nuevo)")
                {
                    IsUp = true;
                    await DisplayAlert(AppResource.Warning, AppResource.MustSelectDependent);
                    return;
                }

                if(TotalAPagar > 0 && !myParametro.GetParNoObligarHacerRecibosConSobrante())
                {
                    IsUp = true;
                    await DisplayAlert(AppResource.Warning, AppResource.CannotMakeReceiptWithSurplus, AppResource.Aceptar);
                    return;
                }

                var recSecuencia = 0;

                var task = new TaskLoader() { SqlTransactionWhenRun = true };

                IsBusy = true;

                await task.Execute(() =>
                {
                    recSecuencia = myRec.GuardarRecibo(Documentos.Where(x => x.Estado == "Aplicada").ToList(), FormasPagoAgregadas.ToList(), DependientesViewModel.CurrentDependiente.ClDCedula);
                });

                await PushAsync(new SuccessPage(AppResource.PaymentReceiptSavedUpper, recSecuencia));

                reciboGuardado();
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.InnerException != null ? e.InnerException.Message : e.Message);
            }

            IsBusy = false;
            IsUp = true;
        }

        public void AgregarFormaPago(AgregarFormaPagoArgs forma)
        {
            try
            {
                int.TryParse(forma.NoCheque, out int numero);

                FormasPagoTemp value = new FormasPagoTemp();
                value.Banco = forma.Banco;

                int.TryParse(forma.BanID, out int banId);

                value.BanID = banId;
                value.Fecha = forma.Fecha.ToString("yyyy-MM-dd HH:mm:ss");
                value.Valor = forma.Valor;
                value.Prima = forma.Prima;
                value.AutSecuencia = forma.AutSecuencia;
                value.DenID = forma.DenId;
                value.PusCantidad = forma.PusCantidad;
                value.BonoCantidad = forma.BonoCantidad;
                /*if (forma.Moneda != null)
                {
                    value.MonCodigo = forma.Moneda.MonCodigo;
                    value.Tasa = forma.Moneda.MonTasa;
                }
                else if (CurrentMoneda != null)
                {
                    value.MonCodigo = CurrentMoneda.MonCodigo;
                    value.Tasa = CurrentMoneda.MonTasa;
                }*/

                value.RefSecuencia = FormasPagoAgregadas.Count + 1;//myRec.GetLastRefSecuenciaInTemp() + 1;
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
                        value.FormaPago = "Tarjeta de crédito";
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
                        value.FormaPago = "Retención";
                        break;
                    case FormasPago.Bono:
                        value.ForID = 20;
                        value.FormaPago = "Bono";
                        break;

                }

                //myRec.AgregarFormaPago(value);
                FormasPagoAgregadas.Add(value);

                dialogAddFormaPago.Dismiss();

                TotalAPagar = GetTotalAPagar();
                
            }
            catch (Exception e)
            {
                DisplayAlert(AppResource.ErrorAddingPaymentway, e.Message, AppResource.Aceptar);
            }

        }
        
    }
}
