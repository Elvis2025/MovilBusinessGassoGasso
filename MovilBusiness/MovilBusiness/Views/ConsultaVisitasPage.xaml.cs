using MovilBusiness.model;
using MovilBusiness.Resx;
using MovilBusiness.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ConsultaVisitasPage : ContentPage
	{
        private bool firstTime = true;

		public ConsultaVisitasPage (Clientes cliente)
		{
            BindingContext = new ConsultaVisitasViewModel(this, cliente);

			InitializeComponent ();
		}

        private void OnListItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            try
            {
                if (args.SelectedItem == null) { return; }

                if (BindingContext is ConsultaVisitasViewModel vm)
                {
                    vm.VerResumenVisita((args.SelectedItem as Visitas).VisSecuencia);
                }               

            }catch(Exception e)
            {
                DisplayAlert(AppResource.ErrorLoadingVisitsSummary, e.Message, AppResource.Aceptar);
            }

            list.SelectedItem = null;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (firstTime)
            {
                firstTime = false;
                if (BindingContext is ConsultaVisitasViewModel vm)
                {
                    vm.LoadVisitas();
                }
            }           
        }

    }
}