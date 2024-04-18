using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Model;
using MovilBusiness.Resx;
using MovilBusiness.ViewModel;
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class HistoricoFacturasPage : ContentPage
	{
        public Action onCancel { set { ((HistoricoFacturasViewModel)BindingContext).onCancel = value; } }
        public Action<string> onAceptarProductos { set { ((HistoricoFacturasViewModel)BindingContext).onAceptarProductos = value; } }

        private readonly bool IsForDevolucion;

		public HistoricoFacturasPage (bool HistoricoPedidos, bool forDevolucion = false)
		{
            IsForDevolucion = forDevolucion;

            if (HistoricoPedidos)
            {
                Title = AppResource.OrderHistory;
            }
            else
            {
                Title = AppResource.InvoiceHistory;
            }

            BindingContext = new HistoricoFacturasViewModel(this, HistoricoPedidos, forDevolucion);

			InitializeComponent ();

            if (HistoricoPedidos)
            {
                lblReferencia.Text = AppResource.Sequence;
                lblDocumento.Text = AppResource.Description;
                lblMonto.Text = "Total";
            }
		}

        private void OnListItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            if(args.SelectedItem == null)
            {
                return;
            }

            if(BindingContext is HistoricoFacturasViewModel vm)
            {
                vm.OnDocumentoSelected(args.SelectedItem as HistoricoFacturas);
            }

           // list.SelectedItem = null;
        }

        private void OnDetalleListItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            if(args.SelectedItem == null)
            {
                return;
            }

            detailList.SelectedItem = null;
        }

        protected override bool OnBackButtonPressed()
        {
            return Arguments.Values.CurrentModule == Modules.DEVOLUCIONES && DS_RepresentantesParametros.GetInstance().GetParDevolucionesProductosFacturas() && IsForDevolucion;
        }
    }
}