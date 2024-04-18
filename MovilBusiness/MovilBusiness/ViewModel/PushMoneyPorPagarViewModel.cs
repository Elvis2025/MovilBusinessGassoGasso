using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.model.Internal.structs;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal.Structs.Args;
using MovilBusiness.Printer;
using MovilBusiness.Printer.Formats;
using MovilBusiness.Resx;
using MovilBusiness.viewmodel;
using MovilBusiness.views;
using MovilBusiness.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class PushMoneyPorPagarViewModel : BaseViewModel
    {
        public ICommand GoNewReceiptCommand { get; private set; }

        public ObservableCollection<model.Internal.MenuItem> MenuSource { get; private set; }
        private model.Internal.MenuItem selecteditem;
        public model.Internal.MenuItem SelectedItem { get { return selecteditem; } set { selecteditem = value; RaiseOnPropertyChanged(); OnOptionItemSelected(); } }

        private ObservableCollection<PushMoneyPorPagar> documentos;
        public ObservableCollection<PushMoneyPorPagar> Documentos { get => documentos; set { documentos = value; RaiseOnPropertyChanged(); } }

        private ClientesCreditoData clientdata;
        public ClientesCreditoData ClientData { get => clientdata; set { clientdata = value; RaiseOnPropertyChanged(); } }

        private bool isfordetalle;
        public bool IsForDetalle { get => isfordetalle; set { isfordetalle = value; RaiseOnPropertyChanged(); } }

        private bool isnotfordetalle;
        public bool IsNotForDetalle { get => isnotfordetalle; set { isnotfordetalle = value; RaiseOnPropertyChanged(); } }

        private bool showprinter;
        public bool ShowPrinter { get => showprinter; set { showprinter = value; RaiseOnPropertyChanged(); } }

        private DS_PushMoneyPorPagar myPxp;
        private PushMoneyPorPagarDetalleFormats pxpPrinter;
        private bool IsConsulting;

        int _pussecuencia;

        public PushMoneyPorPagarViewModel(Page page, bool IsConsulting = false, int pussecuencia = -1) : base(page)
        {
            _pussecuencia = pussecuencia;
            this.IsConsulting = IsConsulting;
            myPxp = new DS_PushMoneyPorPagar();
            IsForDetalle = pussecuencia < 0;
            IsNotForDetalle = pussecuencia > 0;
            GoNewReceiptCommand = new Command(GoNewReceipt);
            pxpPrinter = new PushMoneyPorPagarDetalleFormats();

            BindMenu();
        }

        public async void CargarDocumentos()
        {

            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                await Task.Run(() => 
                {
                    if(_pussecuencia > 0)
                    {
                        Documentos = new ObservableCollection<PushMoneyPorPagar>(myPxp.GetAllPushMoneyBySecuencia(_pussecuencia));
                    }else
                    {
                        Documentos = new ObservableCollection<PushMoneyPorPagar>(myPxp.GetAllPushMoneyByCliente(Arguments.Values.CurrentClient.CliID));
                    }

                    double anterior = 0;
                    if (Documentos != null && Documentos.Count > 0)
                    {
                        foreach (var obj in Documentos)
                        {
                            #region Esto no parece ser necesario
                            /*
                            if (cont == 0)
                            {
                                if (obj.CxcSIGLA == "NC")
                                {
                                    obj.CxcBalanceAcumulado = obj.CxcBalance * -1;
                                    anterior = obj.CxcBalanceAcumulado;
                                    cont++;
                                }
                                else
                                {
                                    obj.CxcBalanceAcumulado = obj.CxcBalance;
                                    anterior = obj.CxcBalanceAcumulado;
                                    cont++;
                                }
                            }
                            else
                            {
                                if (obj.CxcSIGLA == "NC")
                                {
                                    obj.CxcBalanceAcumulado = (obj.CxcBalance * -1) + anterior;
                                    anterior = obj.CxcBalanceAcumulado;
                                }
                                else
                                {
                                    obj.CxcBalanceAcumulado = obj.CxcBalance + anterior;
                                    anterior = obj.CxcBalanceAcumulado;
                                }
                            } 
                            */
                            #endregion
                            obj.PxpBalanceAcumulado = (obj.PxpBalance * obj.Origen) + anterior;
                            anterior = obj.PxpBalanceAcumulado;
                        }

                    }

                });
                
                if(_pussecuencia > 0)
                {                    
                    ClientData = myPxp.GetDatosCreditoPushMoneyPorPagar(_pussecuencia);
                }else
                {
                    ClientData = myPxp.GetDatosCreditoCliente(Arguments.Values.CurrentClient.CliID);
                }                

            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        private async void GoNewReceipt()
        {
            IsBusy = true;
            try
            {
                await PushAsync(new RecibosPushMoneyTabPage());
            }catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }
            IsBusy = false;
        }

        private async void OnOptionItemSelected()
        {
            if (SelectedItem == null || IsBusy)
            {
                return;
            }

            IsBusy = true;

            if (IsConsulting && SelectedItem.Id != 2 && SelectedItem.Id != 5 && SelectedItem.Id != 0)
            {
                IsBusy = false;
                await DisplayAlert(AppResource.Warning, AppResource.ConsultationModeVisitMessage);
                return;
            }

            try
            {
                switch (SelectedItem.Id)
                {
                    case 0:
                        await PopAsync(true);
                        break;
                    case 1:
                        await PushAsync(new RecibosPushMoneyTabPage());
                        break;
                }

                //OnOptionMenuItemSelected?.Invoke();

            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
        }

        private void BindMenu()
        {
            MenuSource = new ObservableCollection<model.Internal.MenuItem>();

            if (myParametro.GetParCrearRecibos())
            {
                MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.NewPaymentReceipt, Icon = "ic_monetization_on_black_24dp", Id = 1 });
            }

            MenuSource.Add(new model.Internal.MenuItem() { Title = AppResource.GetOut, Id = 0, Icon = "ic_exit_to_app_black.png" });
            
        }

        private PushMoneyPorPagar CurrentDocument;
        public async void OnDocumentSelected(PushMoneyPorPagar document)
        {
            if(_pussecuencia != -1)
            {
                return;
            }

            CurrentDocument = document;

            var result = await DisplayActionSheet(AppResource.SelectDesiredOption, buttons: new string[] { AppResource.SeeDetail, AppResource.Print });

            if(result == AppResource.SeeDetail)
            {
                GoDetalleFactura(document);
            }else if(result == AppResource.Print)
            {
                ShowPrinter = true;
            }
        }

        private async void GoDetalleFactura(PushMoneyPorPagar document)
        {
            try
            {
                IsBusy = true;

                Arguments.Values.CurrentModule = Modules.PUSHMONEYPORPAGAR;

                await Task.Run(() => { myPxp.InsertProductInTempForDetail(document.PxpReferencia, (int)Arguments.Values.CurrentModule); });               

                var args = new PedidosDetalleArgs
                {
                    FechaEntrega = DateTime.Now,
                    ConId = 0,
                    DisenoDelRow = myParametro.GetFormatoVisualizacionProductos(),
                    PedOrdenCompra = null,
                    IsEditing = true
                };

                var cxc = new CuentasxCobrar
                {
                    CxcReferencia = document.PxpReferencia,
                    CxcDocumento = document.PxpDocumento,
                    CxcMontoTotal = document.PxpMontoTotal,
                    CxcMontoSinItbis = document.PxpMontoSinItbis
                };

                await PushAsync(new PedidosDetallePage(args, true, document.PxpReferencia, documento: cxc) { Title = AppResource.DocumentDetail, IsDetail = true });
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        public async void AceptarImpresion(int Copias)
        {
            try
            {

                for (int x = 0; x < Copias; x++)
                {
                    IsBusy = true;

                    await ImprimirDocumento(CurrentDocument);

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
        }

        private Task ImprimirDocumento(PushMoneyPorPagar documento)
        {
            return Task.Run(() =>
            {
                var printer = new PrinterManager(myParametro.GetParSectores() > 0 ? Arguments.Values.CurrentSector.SecCodigo : "");

                pxpPrinter.Print(documento, printer);

            });
        }

    }
}
