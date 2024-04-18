using MovilBusiness.model.Internal;
using MovilBusiness.ViewModel;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ProductosProximoVencerPage : ContentPage
	{
		public ProductosProximoVencerPage (int cliId)
		{
            BindingContext = new ProductosProximosVencerViewModel(cliId, this);
			InitializeComponent ();
		}

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ((ProductosProximosVencerViewModel)BindingContext).CargarProductos();
        }

        private void OnListItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if(e.SelectedItem == null)
            {
                return;
            }

            ((ProductosProximosVencerViewModel)BindingContext).OpenDetalleFactura(e.SelectedItem as ProductosTemp);

            list.SelectedItem = null;
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

    }
}