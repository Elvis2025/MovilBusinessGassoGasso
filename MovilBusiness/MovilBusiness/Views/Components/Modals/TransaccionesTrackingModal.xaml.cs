using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class TransaccionesTrackingModal : ContentPage
	{
        private bool FirstTime = true;

		public TransaccionesTrackingModal (Transaccion transaccion, ExpandListItem<Estados> datosTransaccion)
		{
            BindingContext = new TransaccionesTrackingViewModel(this, transaccion, datosTransaccion);

			InitializeComponent ();
		}

        private async void Dismiss(object sender, EventArgs args)
        {
            await Navigation.PopModalAsync(false);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (FirstTime && BindingContext is TransaccionesTrackingViewModel vm)
            {
                FirstTime = false;
                vm.LoadTracking();
            }
        }
    }
}