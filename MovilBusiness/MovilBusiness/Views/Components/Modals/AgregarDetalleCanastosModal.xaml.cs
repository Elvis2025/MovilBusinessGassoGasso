using Microsoft.AppCenter.Crashes;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Mobile;

namespace MovilBusiness.Views.Components.Modals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AgregarDetalleCanastosModal : ContentPage, INotifyPropertyChanged
    {
        private TipoCapturaCanastos CurrentTipo;
        private List<string> detalles;

        private int counter = 0;
        private int CantidadCanastos;

        private Action OnSaved;
        public AgregarDetalleCanastosModal(TipoCapturaCanastos tipo, int cantidadCanastos, Action onDetalleSaved)
        {
            OnSaved = onDetalleSaved;
            counter = 0;
            detalles = new List<string>();
            CurrentTipo = tipo;
            CantidadCanastos = cantidadCanastos;

            InitializeComponent();

            lblTitle.Text = tipo == TipoCapturaCanastos.ENTREGARCANASTOS ? AppResource.DetailDeliveryBasket : AppResource.BasketsReceptionDetail;

            lblCounter.Text = AppResource.PendingBasketsZero.Replace("0/0", "0/") + cantidadCanastos;
            lblCanastosAgregados.Text = AppResource.AddedBasketsLabel;
        }

        private void editSearch_Completed(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(editSearch.Text))
            {
                DisplayAlert(AppResource.Warning, AppResource.MustSpecifyCode, AppResource.Aceptar);
                return;
            }

            if(counter == CantidadCanastos)
            {
                return;
            }

            if (detalles.Contains(editSearch.Text))
            {
                DisplayAlert(AppResource.Warning, AppResource.BasketCodeHasBeenScanned, AppResource.Aceptar);
                return;
            }

            detalles.Add(editSearch.Text.Trim());

            lblCanastosAgregados.Text += counter == 0 ? editSearch.Text.Trim() : ", " + editSearch.Text.Trim();

            editSearch.Text = "";
            counter++;
            lblCounter.Text = AppResource.PendingBasketsLabel + counter + "/" + CantidadCanastos;
            editSearch.Focus();
        }

        private bool isScanning;
        private async void GoScanBarCode(object sender, EventArgs e)
        {
            try
            {
                if (isScanning)
                {
                    return;
                }

                isScanning = true;
                var options = new MobileBarcodeScanningOptions
                {
                    PossibleFormats = new List<ZXing.BarcodeFormat>() {
                    ZXing.BarcodeFormat.All_1D, ZXing.BarcodeFormat.CODABAR
                }
                };

                var scanner = new MobileBarcodeScanner
                {
                    UseCustomOverlay = false
                };

                var result = await scanner.Scan(options);

                if (result != null)
                {
                    editSearch.Text = result.Text;
                    editSearch.Focus();
                }
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                await DisplayAlert(AppResource.Warning, ex.Message, AppResource.Aceptar);
            }

            isScanning = false;
        }

        private void Dismiss(object sender, EventArgs args)
        {
            Navigation.PopModalAsync(false);
        }

        private int savedTraSecuencia = -1;
        private async void Save(object sender, EventArgs args)
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                savedTraSecuencia = -1;

                if (detalles.Count == 0)
                {
                    throw new Exception(AppResource.MustAddBasketsCode);
                }

                if(counter < CantidadCanastos)
                {
                    throw new Exception(AppResource.ThereAreBasketsToScan);
                }

                IsBusy = true;

                var task = new TaskLoader() { SqlTransactionWhenRun = true };

                await task.Execute(() => 
                {
                    savedTraSecuencia = new DS_TransaccionesCanastos().SaveCanastos(CurrentTipo, CantidadCanastos, detalles);
                });

                IsBusy = false;

                var imprimir = await DisplayAlert(AppResource.Success, (CurrentTipo == TipoCapturaCanastos.ENTREGARCANASTOS ? AppResource.DeliveryBasketSavedWantPrint : AppResource.ReceiptBasketSavedWantPrint), AppResource.Print, AppResource.Continue);

                if (!imprimir)
                {
                    await Navigation.PopModalAsync(false);

                    OnSaved?.Invoke();
                }
                else
                {
                    GoPrint(savedTraSecuencia);
                }

            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
        }

        private async void GoPrint(int traSecuencia)
        {
            try
            {
                var copias = await DisplayActionSheet(AppResource.ChoosePrinterCopies, AppResource.Cancel, null, new string[] { "1", "2", "3", "4", "5" });

                if (int.TryParse(copias, out int elegidas) && elegidas > 0)
                {
                    await new DS_TransaccionesCanastos().AceptarImpresion(traSecuencia, elegidas);
                }
            }catch(Exception e)
            {
                await DisplayAlert(AppResource.ErrorPrinting, e.Message, AppResource.Aceptar);
            }

            await Navigation.PopModalAsync(false);

            OnSaved?.Invoke();

        }

        private bool isbusy;
        public new bool IsBusy { get => isbusy; set { isbusy = value; RaiseOnPropertyChanged(); } }

        public new event PropertyChangedEventHandler PropertyChanged;
        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}