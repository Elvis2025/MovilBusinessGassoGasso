using MovilBusiness.Model;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.ViewModel;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CuadresPage : ContentPage
	{
		public CuadresPage (Cuadres CuadreParaCerrar = null, Action<bool,int> CuadreGuardado = null, string RepAuditor = "")
		{
            Title = CuadreParaCerrar != null ? AppResource.CloseSquare : AppResource.OpenSquare;

            BindingContext = new CuadresViewModel(this, CuadreParaCerrar, CuadreGuardado, RepAuditor: RepAuditor);

            InitializeComponent ();
		}

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (((CuadresViewModel)BindingContext).Vehiculos == null || ((CuadresViewModel)BindingContext).Vehiculos.Count <= 0)
            {
                DisplayAlert(AppResource.Warning, AppResource.MustAsignVehicleAndSync, AppResource.Aceptar);
                Navigation.PopAsync();
            }
            else
            {
                ((CuadresViewModel)BindingContext).ListenForLocationUpdatesIfPermission();
                ((CuadresViewModel)BindingContext).LoadCurrentVehicle();
            }

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopLocationsUpdates();
        }

        private async void StopLocationsUpdates()
        {
            await Functions.StopListeningForLocations();
        }
    }
}