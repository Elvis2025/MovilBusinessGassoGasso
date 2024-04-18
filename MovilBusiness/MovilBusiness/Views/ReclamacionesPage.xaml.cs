using MovilBusiness.model;
using MovilBusiness.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ReclamacionesPage : ContentPage
	{
		public ReclamacionesPage ()
		{
            BindingContext = new ReclamacionesViewModel(this);
			InitializeComponent ();

		}

        private void List_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if(e.SelectedItem == null)
            {
                return;
            }


            ((ReclamacionesViewModel)BindingContext).OnProductSelected(e.SelectedItem as Productos);

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