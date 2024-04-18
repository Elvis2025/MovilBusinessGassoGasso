using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Model.Internal;
using MovilBusiness.Printer;
using MovilBusiness.Printer.Formats;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class DepositosComprasViewModel : BaseViewModel
    {
        public string MontoCajaChica { get; private set; }
        public string MontoComprado { get; private set; }
        public string MontoReponer { get; private set; }
        public string User { get; private set; }
        public string ComprasRango { get; private set; }

        private bool isvisiblebutton;
        public bool IsVisibleButton { get { return isvisiblebutton; } set { isvisiblebutton = value; RaiseOnPropertyChanged(); } }
        public string Title { get => myParametro.GetParCambiarNombreComprasPorPushMoney() ? AppResource.PushmoneyDeposits.ToUpper() : AppResource.PurchaseDeposits.ToUpper(); }

        private bool showprinter;
        public bool ShowPrinter { get => showprinter; set { showprinter = value; RaiseOnPropertyChanged(); } }

        private DS_Compras myCom;
        private DS_DepositosCompras myDepCom;

        private ComprasDepositarRango RangoCompras;
        private double montocajachica;

        private int CurrentDepSecuencia = -1;
        private bool UsePushMoneyName;

        public DepositosComprasViewModel(Page page, int depsecuencia) : base(page)
        {
            IsVisibleButton = true;
            myCom = new DS_Compras();
            myDepCom = new DS_DepositosCompras();

            SaveCommand = new Command(() =>
            {
                GuardarDeposito();

            }, () => IsUp);

            User = AppResource.UserLabel + Arguments.CurrentUser.RepCodigo;

            double.TryParse(myParametro.GetParDepComprasMontoCajaChica(), out double monto);

            montocajachica = monto;

            if(depsecuencia > 0)
            {
                IsVisibleButton = false;
                RangoCompras = myCom.GetRangoComprasADepositarFromDetalle(depsecuencia);
            }
            else
            {
                RangoCompras = myCom.GetRangoComprasADepositar();
            }

            MontoCajaChica = montocajachica.ToString("C2");

            UsePushMoneyName = myParametro.GetParCambiarNombreComprasPorPushMoney();

            if(RangoCompras != null)
            {
                MontoComprado = RangoCompras.MontoComprado.ToString("C2");
                ComprasRango = (UsePushMoneyName ? "PushMoney: " : AppResource.PurchasesLabel) + RangoCompras.MinComSecuencia.ToString() + " - " + RangoCompras.MaxComSecuencia.ToString();

                MontoReponer = (montocajachica - RangoCompras.MontoComprado).ToString("C2");
            }
            else
            {
                MontoComprado = "$0.00";
                MontoReponer = "$0.00";
                ComprasRango = UsePushMoneyName ? "PushMoney: 0 - 0" : AppResource.PurchasesLabel + "0 - 0";
            }
        }

        private async void GuardarDeposito()
        {
            IsUp = false;

            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {

                if (RangoCompras == null || RangoCompras.MontoComprado == 0)
                {
                    IsUp = true;
                    throw new Exception(UsePushMoneyName ? AppResource.NoPushMoneyToDeposit : AppResource.NoPurchasesToDeposits);
                }

                if(myParametro.GetParCuadres() > 0 && Arguments.Values.CurrentCuaSecuencia == -1)
                {
                    IsUp = true;
                    throw new Exception(AppResource.OpenSquareToDeposit);
                }

                var task = new TaskLoader() { SqlTransactionWhenRun = true };

                await task.Execute(() => 
                {
                    CurrentDepSecuencia = myDepCom.SaveDeposito(RangoCompras, montocajachica);
                });

                var result = await DisplayAlert(AppResource.Success, AppResource.DepositSavedSuccessfully, AppResource.Print, AppResource.Continue);

                if (result)
                {
                    ShowPrinter = true;
                }
                else
                {
                    await PopModalAsync(true);
                }

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsUp = true;
            IsBusy = false;
        }

        public async void AceptarImpresion(int copias)
        {
            try
            {
                if (IsBusy)
                {
                    return;
                }

                IsBusy = true;

                var printer = new DepositosComprasFormats(myDepCom);

                PrinterManager printerManager = null;

                for (int i = 0; i < copias; i++)
                {
                    IsBusy = true;

                    await Task.Run(() => 
                    {
                        if(printer == null)
                        {
                            if(printerManager == null)
                            {
                                printerManager = new PrinterManager();
                            }

                            printer.Print(CurrentDepSecuencia, false, printerManager);
                        }
                    });

                    IsBusy = false;

                    if (copias > 1 && i != copias - 1)
                    {
                        await DisplayAlert(AppResource.PrintCopy, AppResource.CutPapelMessage, AppResource.Print);
                    }
                }

                ShowPrinter = false;

                await PopModalAsync(true);

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.ErrorPrinting, e.Message);
            }

            IsBusy = false;
        }
    }
}
