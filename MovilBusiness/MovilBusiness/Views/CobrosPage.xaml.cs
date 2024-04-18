using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.viewmodel;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CobrosPage : MasterDetailPage
    {
		public CobrosPage (bool IsConsulting)
		{
            var vm = new CobrosViewModel(this, IsConsulting) { OnOptionMenuItemSelected = () => { IsPresented = false; } };
            BindingContext = vm;

			InitializeComponent ();

            dialogImpresion.OnCancelar = vm.CancelarImpresion;
            dialogImpresion.OnAceptar = vm.AceptarImpresion;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Arguments.Values.CurrentModule = Modules.COBROS;

            var columnasCobros = DS_RepresentantesParametros.GetInstance().GetParOrdencolumnasCobros();

            if (columnasCobros > 0)
            {
                Orden_1.IsVisible = columnasCobros == 1;
                Orden_2.IsVisible = columnasCobros == 2;
            }
            else
            {
                Orden_2.IsVisible = false;
            }

            entregaDocumento.IsVisible = Arguments.Values.CurrentClient.CliIndicadorDepositaFactura;

            if (BindingContext is CobrosViewModel vm)
            {
                if (vm.FirstTime)
                {
                    vm.SelectCurrentMoneda();
                    vm.DisplayAlertCheques(true);
                    vm.ClearCxcDetalleTemp();
                }

                vm.Load();
            }  
        }

        protected override bool OnBackButtonPressed()
        {
            Finish();
            return true;
        }

        private bool finishing;
        private async void Finish()
        {
            if (finishing)
            {
                return;
            }
            finishing = true;
            await Navigation.PopAsync(false);
            finishing = false;
        }

        /*private void OnCancelarImpresion()
        {
            ((CobrosViewModel)BindingContext).CancelarImpresion();
        }

        private void OnAceptarImpresion(int copias)
        {
            ((CobrosViewModel)BindingContext).AceptarImpresion(copias);
        }*/

        private void OnListItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                return;
            }

            if (BindingContext is CobrosViewModel vm)
            {
                vm.OnDocumentSelected(e.SelectedItem as CuentasxCobrar);
            }

            cobrosList.SelectedItem = null;
            cobrosList2.SelectedItem = null;
        }

    }
}