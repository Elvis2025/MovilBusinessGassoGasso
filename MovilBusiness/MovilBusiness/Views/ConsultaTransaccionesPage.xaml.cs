using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Printer;
using MovilBusiness.viewmodel;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ConsultaTransaccionesPage : TabbedPage
	{
        DS_RepresentantesParametros myparm;
        public ConsultaTransaccionesPage (int cliId = -1)
		{
             myparm = DS_RepresentantesParametros.GetInstance();
            if (myparm.GetParInitPrinterManager() && !myparm.GetParEmpresasBySector())
            {
                InitPrinterManager();
            }
            BindingContext = new ConsultaTransaccionesViewModel(this, cliId);
			InitializeComponent ();
		}

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if(BindingContext is ConsultaTransaccionesViewModel vm)
            {
                Arguments.Values.IsPushMoneyRotacion = false;
                vm.LoadList();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if(myparm.GetParInitPrinterManager())
            {
                PrinterManager.ConnToClose();
            }
        }

        public async void InitPrinterManager()
        {
            await Task.Run(() =>
            {
                ConsultaTransaccionesDetalleViewModel.printerManager = new PrinterManager();
            });
        }

    }
}