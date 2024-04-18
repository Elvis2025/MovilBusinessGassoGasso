using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.viewmodel;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ConsultaTransaccionesFechaTab : ContentPage
	{
		public ConsultaTransaccionesFechaTab ()
		{
			InitializeComponent ();
		}


        private void OnListItemTapped(object sender, ItemTappedEventArgs args)
        {
            if (args.Item == null)
            {
                return;
            }

            if (BindingContext is ConsultaTransaccionesViewModel vm)
            {
                vm.ItemTapped(args.Item as ExpandListItem<Estados>, true);
            }

            list.SelectedItem = null;

        }
    }
}