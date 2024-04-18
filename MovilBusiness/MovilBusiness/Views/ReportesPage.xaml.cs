using MovilBusiness.ViewModel;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ReportesPage : ContentPage
	{
		public ReportesPage()
		{
            var vm = new ReportesViewModel(this)
            {
                DateChanged = (date, range) => 
                {
                    if(range == Enums.DateRange.DATEFROM && lblFechaDesde != null)
                    {
                        lblFechaDesde.Text = date.ToString("dd/MM/yyyy");
                    }
                    else if(lblFechaHasta != null)
                    {
                        lblFechaHasta.Text = date.ToString("dd/MM/yyyy");
                    }
                }
            };

            BindingContext = vm;

			InitializeComponent ();

            lblFechaDesde.Text = DateTime.Now.ToString("dd/MM/yyyy");
            lblFechaHasta.Text = DateTime.Now.ToString("dd/MM/yyyy");

            vm.CurrentTipoReporte = null;
            vm.IsReady = true;
        }

        private void SelectFechaDesde(object sender, EventArgs e)
        {
            pickerFechaDesde.IsVisible = true;
            pickerFechaDesde.Focus();
        }

        private void SelectFechaHasta(object sender, EventArgs e)
        {
            pickerFechaHasta.IsVisible = true;
            pickerFechaHasta.Focus();
        }
    }
}