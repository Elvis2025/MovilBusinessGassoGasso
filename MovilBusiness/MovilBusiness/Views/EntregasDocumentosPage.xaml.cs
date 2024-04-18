using MovilBusiness.model;
using MovilBusiness.viewmodel;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class EntregasDocumentosPage : ContentPage
	{
        public static bool Finish;
        public bool verdetalle;

        public EntregasDocumentosPage (EntregasDocumentos entrega = null, bool verDetalle = false, bool confirmados = false)
		{
            BindingContext = new EntregasDocumentosViewModel(this, entrega, verDetalle, confirmados);
			InitializeComponent ();
            verdetalle = verDetalle;
            dialogImpresion.OnCancelar = OnCancelarImpresion;
            dialogImpresion.OnAceptar = OnAceptarImpresion;
            dialogImpresion.SetCopiasImpresionByTitId(10);
        }

        private void OnListItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            if(args.SelectedItem == null)
            {
                return;
            }

            if(BindingContext is EntregasDocumentosViewModel vm)
            {
                vm.ShowDetalleFactura(args.SelectedItem as EntregasDocumentosDetalle);
            }

            listFacturas.SelectedItem = null;
        }

        private void OnCancelarImpresion()
        {
            ((EntregasDocumentosViewModel)BindingContext).CancelarImpresion();
        }

        private void OnAceptarImpresion(int copias)
        {
            ((EntregasDocumentosViewModel)BindingContext).AceptarImpresion(copias);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            lblRecibido.IsVisible = !verdetalle;
            entRecibido.IsVisible = !verdetalle;

            if (Finish)
            {
                Finish = false;
                Navigation.PopAsync(false);
            }
        } 

    }
}