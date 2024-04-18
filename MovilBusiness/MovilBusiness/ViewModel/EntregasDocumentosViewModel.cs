
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.Printer;
using MovilBusiness.printers.formats;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.views;
using MovilBusiness.Views.Components.Modals;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.viewmodel
{
    public class EntregasDocumentosViewModel : BaseViewModel
    {

        private ObservableCollection<EntregasDocumentosDetalle> facturas;
        public ObservableCollection<EntregasDocumentosDetalle> Facturas{ get => facturas; set { facturas = value; RaiseOnPropertyChanged(); } }

        public string EntregaNumero { get => AppResource.DeliveryNumberLabel + DS_RepresentantesSecuencias.GetLastSecuencia("EntregasDocumentos") + 1; }

        private string recibidopor;
        public string RecibidoPor { get => recibidopor; set { recibidopor = value; RaiseOnPropertyChanged(); } }
     
        private List<EntregasDocumentosDetalle> DocumentosAgregados;

        private DS_EntregaFactura myEnt;

        private bool showprinter = false;
        public bool ShowPrinter { get => showprinter; set { showprinter = value; RaiseOnPropertyChanged(); } }

        private EntregaDocumentosFormats EntregaPrinter;

        private int CurrentEntSecuencia = -1;
        public bool verdetalle;

        public EntregasDocumentosViewModel(Page page, EntregasDocumentos entregasDoc, bool verDetalle = false, bool confirmado = false) : base(page)
        {
            myEnt = new DS_EntregaFactura();

            EntregaPrinter = new EntregaDocumentosFormats(myEnt);
            verdetalle = verDetalle;
            DocumentosAgregados = new List<EntregasDocumentosDetalle>();
            if (verDetalle)
            {
                Facturas = new ObservableCollection<EntregasDocumentosDetalle>(myEnt.GetEntregasDetalleBySecuencia(entregasDoc.EntSecuencia, confirmado));
            }
            else
            {
                Facturas = new ObservableCollection<EntregasDocumentosDetalle>(myEnt.GetDocumentosPorEntregar(Arguments.Values.CurrentClient.CliID));
            }
          
            SaveCommand = new Command(() =>
            {
                SaveOrder();

            }, () => IsUp);

        }

        public async  void ShowDetalleFactura(EntregasDocumentosDetalle data)
        {
            if (IsBusy)
            {
                return;
            }

            if (verdetalle)
            {
                return;
            }

            var result = await DisplayAlert(AppResource.Select, AppResource.DocumentLabel + data.EntDocumento + "\n"+AppResource.DateLabel + data.formattedFecha + "\n"+AppResource.InitialsLabel + data.cxcSigla + "\n"+AppResource.AmountLabel + data.EntMonto.ToString("N2"), AppResource.Add, AppResource.Remove);

            if (result)
            {
                AddDocument(data);
            }
            else
            {
                DeleteDocument(data);
            }
        }

        private void AddDocument(EntregasDocumentosDetalle data)
        {
            var element = DocumentosAgregados.Where(x => x.EntDocumento == data.EntDocumento).FirstOrDefault();

            if (element != null)
            {
                DisplayAlert(AppResource.Warning, AppResource.DocumentAlreadyBeenAdded, AppResource.Aceptar);
            }
            else
            {
                var item = data.Copy();
                item.Estatus = 2;
                DocumentosAgregados.Add(data);
                Facturas[Facturas.IndexOf(data)] = item;
            }
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

        private Task Imprimir()
        {
            return Task.Run(() =>
            {
                var printer = new PrinterManager();

            EntregaPrinter.Print(CurrentEntSecuencia, printer);

            });
        }

        private void DeleteDocument(EntregasDocumentosDetalle data)
        {

            var element = DocumentosAgregados.Where(x => x.EntDocumento == data.EntDocumento).FirstOrDefault();

            if (element != null)
            {
                DocumentosAgregados.Remove(element);
                var item = data.Copy();
                item.Estatus = 1;
                Facturas[Facturas.IndexOf(data)] = item;
            }

        }

        private async void SaveOrder()
        {
            IsUp = false;

            if (IsBusy)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(RecibidoPor))
            {
                IsUp = true;
                await DisplayAlert(AppResource.Warning, AppResource.ReceivedByCannotBeEmpty, AppResource.Aceptar);
                return;
            }

            if(DocumentosAgregados.Count == 0)
            {
                IsUp = true;
                await DisplayAlert(AppResource.Warning, AppResource.NotAddedAnyDocumentToDeliver, AppResource.Aceptar);
                return;
            }

            IsBusy = true;

            try
            {
                TaskLoader task = new TaskLoader() { SqlTransactionWhenRun = true };

                await task.Execute(() => 
                {
                    CurrentEntSecuencia = myEnt.GuardarEntrega(DocumentosAgregados, RecibidoPor);
                });

                EntregasDocumentosPage.Finish = true;
                Arguments.Values.CurrentModule = Modules.ENTREGADOCUMENTOS;
                await PushAsync(new SuccessPage(AppResource.DeliverySavedUpper, CurrentEntSecuencia));

               

            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.ErrorSavingDelivery, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
            IsUp = true;
        }
    }
}
