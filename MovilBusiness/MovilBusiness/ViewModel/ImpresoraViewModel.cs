
using Microsoft.AppCenter.Crashes;
using MovilBusiness.Abstraction;
using MovilBusiness.Enums;
using MovilBusiness.Printer;
using MovilBusiness.Printer.Model;
using MovilBusiness.Resx;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.viewmodel
{
    public class ImpresoraViewModel : BaseViewModel
    {
        public ICommand RestartDiscoveryCommand { get; private set; }
        private ObservableCollection<BTDevice> impresoras;
        public ObservableCollection<BTDevice> Impresoras { get => impresoras; set { impresoras = value; RaiseOnPropertyChanged(); } }

       // public Action OnStartDiscovery { get; set; }

        private bool isloading = false;
        public bool IsLoading { get => isloading; set { isloading = value; RaiseOnPropertyChanged(); } }

        private IPrinterDiscovery PrinterManager;

        private PrinterMetaData printerData;

        private PrinterLanguageMB language = PrinterLanguageMB.NULL;

        public ImpresoraViewModel(Page page, PrinterLanguageMB language) : base(page)
        {
            this.language = language;
            RestartDiscoveryCommand = new Command(StartDiscovery);
            Impresoras = new ObservableCollection<BTDevice>();

            PrinterManager = DependencyService.Get<IPrinterDiscovery>();

            PrinterManager.SetOnDevicesFound((d) =>
            {
                if (string.IsNullOrWhiteSpace(d.Address))
                {
                    return;
                }

                var device = Impresoras.Where(x => x.Address.ToUpper().Trim() == d.Address.ToUpper().Trim()).FirstOrDefault();

                if(device != null)
                {
                    var index = Impresoras.IndexOf(device);

                    Impresoras[index] = d;
                }
                else
                {
                    Impresoras.Add(d);
                }
            });

            PrinterManager.SetOnError((message) => { DisplayAlert(AppResource.Warning, message); IsBusy = false; });
            PrinterManager.SetOnDevicePaired((address) =>
            {
                OnDevicePaired(address);
            });

        }

        public void StartDiscovery()
        {
            try
            {
                if (PrinterManager != null && PrinterManager.IsEnabled())
                {
                    IsLoading = true;
                    if(Impresoras == null)
                    {
                        Impresoras = new ObservableCollection<BTDevice>();
                    }
                    else
                    {
                        Impresoras.Clear();
                    }

                    PrinterManager?.StartDiscovery(language == PrinterLanguageMB.ESCPOS);
                }
                else
                {
                    DisplayAlert(AppResource.Warning, AppResource.BluetoothIsNotOn);
                }
            }catch(Exception e)
            {
                Crashes.TrackError(e);
                DisplayAlert(AppResource.Warning, e.Message);
            }
        }

        public void StopDiscovery()
        {
            try
            {
                IsLoading = false;

                if (PrinterManager.IsEnabled())
                {
                    PrinterManager?.StopDiscovery();
                }
            }catch(Exception e)
            {
                Crashes.TrackError(e);
                DisplayAlert(AppResource.Warning, e.Message);
            }
        }

        private async void OnDevicePaired(string printer)
        {
            try
            {
                IsBusy = true;

                myParametro.SaveImpresora(printerData);

                await Task.Run(() =>
                {
                    var m = new PrinterManager();
                    m.TestPrinter();
                });

                await DisplayAlert(AppResource.Success, AppResource.PrinterConfiguredCorrectly, AppResource.Aceptar);

                await PopAsync(true);

            }catch(Exception e)
            {
                Crashes.TrackError(e);
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        public async void PairDevice(string printer)
        {
            try
            {
                if (!PrinterManager.IsEnabled())
                {
                    await DisplayAlert(AppResource.Warning, AppResource.BluetoothIsNotOn);
                }

                var lenguaje = "";

                if (language == PrinterLanguageMB.NULL)
                {
                    lenguaje = await DisplayActionSheet(AppResource.SelectPrinterType, buttons: new string[] { "CPCL", "ESCPOS" });
                }
                else
                {
                    switch (language)
                    {
                        case PrinterLanguageMB.CPCL:
                            lenguaje = "CPCL";
                            break;
                        case PrinterLanguageMB.ESCPOS:
                            lenguaje = "ESCPOS";
                            break;
                    }
                }

                if(lenguaje != "CPCL" && lenguaje != "ESCPOS" && lenguaje != "CPCLRON")
                {
                    return;
                }

                IsBusy = true;

                printerData = new PrinterMetaData()
                {
                    PrinterLanguage = lenguaje,
                    PrinterMac = printer
                };

                PrinterManager.PairDevice(printer);

            }catch(Exception e)
            {
                Crashes.TrackError(e);
                await DisplayAlert(AppResource.ErrorConfiguringPrinter, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
        }
    }
}
