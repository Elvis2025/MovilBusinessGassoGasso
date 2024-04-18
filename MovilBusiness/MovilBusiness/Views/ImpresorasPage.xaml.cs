using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using LinkOS.Plugin.Abstractions;
using MovilBusiness.viewmodel;
using MovilBusiness.Printer.Model;
using MovilBusiness.Enums;
using MovilBusiness.Printer;
using MovilBusiness.DataAccess;

namespace MovilBusiness.views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ImpresorasPage : ContentPage
	{
        
		public ImpresorasPage (PrinterLanguageMB language)
		{
            var vm = new ImpresoraViewModel(this, language)
            {
                ///OnStartDiscovery = () => { StartBluetoothDiscovery(); }
            };

            BindingContext = vm;

            InitializeComponent ();

            //StartBluetoothDiscovery();

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                ((ImpresoraViewModel)BindingContext).StartDiscovery();

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }
        }

        protected override void OnDisappearing()
        {
            try
            {
                if(DS_RepresentantesParametros.GetInstance().GetParInitPrinterManager())
                {
                    PrinterManager.ConnToClose();
                }

                ((ImpresoraViewModel)BindingContext).StopDiscovery();

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            base.OnDisappearing();
        }


        /*private void StartBluetoothDiscovery()
        {
            if(BindingContext is ImpresoraViewModel vm)
            {
                vm.IsLoading = true;
            }

            IDiscoveryEventHandler handler = DiscoveryHandlerFactory.Current.GetInstance();
            handler.OnDiscoveryError += OnDiscoveryError;
            handler.OnDiscoveryFinished += OnDiscoveryFinished;
            handler.OnFoundPrinter += OnFoundPrinter;
            //For Android 
            // BluetoothDiscoverer.Current.FindPrinters(MainActivity.Context, handler);
            //For iOS
            // BluetoothDiscoverer.Current.FindPrinters(null, handler);
            //For Forms apps: implement the previous two methods in OS code projects (PrinterDiscoveryImplementation.cs)

            var pManager = DependencyService.Get<IPrinterDiscovery>();//.FindBluetoothPrinters(handler);

        }*/

        private void OnDiscoveryFinished(IDiscoveryHandler handler)
        {
            if (BindingContext is ImpresoraViewModel vm)
            {
                vm.IsLoading = false;
            }

        }

       /* private void OnFoundPrinter(IDiscoveryHandler handler, IDiscoveredPrinter discoveredPrinter)
        {
            if(BindingContext is ImpresoraViewModel vm)
            {
                vm.AddPrinter(new BTDevice() { Address = discoveredPrinter.Address, Name = discoveredPrinter.ToString()});
            }
        }
        private void OnDiscoveryError(IDiscoveryHandler handler, string message)
        {
            System.Diagnostics.Debug.WriteLine("Discovery Error: " + message);
        }*/

        private void OnListItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                return;
            }

            if(BindingContext is ImpresoraViewModel vm)
            {
                vm.PairDevice((e.SelectedItem as BTDevice).Address);
            }

            impresorasList.SelectedItem = null;
        }

    }
}