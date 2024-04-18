using MovilBusiness.ViewModel;
using MovilBusiness.Views.Components.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ComentariosPage : ContentPage
	{
		public ComentariosPage (int CurrentTraSecuencia)
		{
            BindingContext = new ComentariosViewModel(this, CurrentTraSecuencia);
			InitializeComponent ();
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