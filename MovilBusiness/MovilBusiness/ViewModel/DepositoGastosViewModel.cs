using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Model;
using MovilBusiness.Printer;
using MovilBusiness.Printer.Formats;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class DepositoGastosViewModel : BaseViewModel
    {
        public List<Gastos> Gastos { get; set; }

        private bool showprintdialog;
        public bool ShowPrintDialog { get => showprintdialog; set { showprintdialog = value; RaiseOnPropertyChanged(); } }

        public string NoDeposito { get => AppResource.DepositNumberUpper + DS_RepresentantesSecuencias.GetLastSecuencia("DepositosGastos"); }
        public string Usuario { get => AppResource.UserLabel + Arguments.CurrentUser.RepCodigo; }
        public string MontoDeposito { get; set; } = "$0.00";

        private DS_DepositosGastos myDep;
        private DepositoGastosFormats depPrinter;

        private int CurrentDepSecuencia = -1;

        public DepositoGastosViewModel(Page page) : base(page)
        {

            SaveCommand = new Command(() =>
            {
                SaveDeposito();

            }, () => IsUp);

            myDep = new DS_DepositosGastos();

            depPrinter = new DepositoGastosFormats(myDep);

            Gastos = new DS_Gastos().GetGastosParaDepositar();

            if(Gastos != null && Gastos.Count > 0)
            {
                MontoDeposito = Gastos.Sum(x => x.GasMontoTotal).ToString("C2");
            }
            
        }

        private async void SaveDeposito()
        {
            IsUp = false;

            try
            {
                if(Gastos == null || Gastos.Count == 0)
                {
                    IsUp = true;
                    await DisplayAlert(AppResource.Warning, AppResource.NoFeesToDeposits);
                    return;
                }

                IsBusy = true;

                var task = new TaskLoader() { SqlTransactionWhenRun = true };

                await task.Execute(() => { CurrentDepSecuencia = myDep.SaveDepositoGastos(Gastos); });

                var result = await DisplayAlert(AppResource.Success, AppResource.DepositSavedSuccessfully, AppResource.Print, AppResource.Aceptar);

                if (result)
                {
                    ShowPrintDialog = true;
                    IsBusy = false;
                    IsUp = true;
                    return;
                }

                await PopAsync(true);

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.ErrorSavingDeposit, e.Message);
            }

            IsBusy = false;
            IsUp = true;
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
            catch(Exception e)
            {
                await DisplayAlert(AppResource.ErrorPrinting, e.Message);
            }

            IsBusy = false;
            ShowPrintDialog = false;
            await PopAsync(false);
        }

        private Task Imprimir()
        {
            return Task.Run(() => 
            {
                var printer = new PrinterManager();

                depPrinter.Print(CurrentDepSecuencia, false, printer);
            });
        }
    }
}
