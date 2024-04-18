
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using MovilBusiness.Views.Components.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class HomePage : MasterDetailPage
    {
		public HomePage ()
		{
            var vm = new HomeViewModel(this) { OnOptionMenuItemSelected=()=> { IsPresented = false; } };

            BindingContext = vm;

			InitializeComponent();

            NavigationPage.SetBackButtonTitle(this, AppResource.Start);

            SqliteManager.GetInstance().AddVirtualColumns();

            if (vm.ShowBtnCargas || vm.ShowBtnCuadre || vm.ShowBtnProspectos || vm.ShowBtnInvFisico || vm.ShowBtnReporte || vm.ShowBtnConInv || vm.ShowBtnDepositos)
            {
                content.Content = new HomeDashboardPlus()
                {
                    BindingContext = vm
                };
            }
            else
            {
                vm.ShowBtnReporte = DS_RepresentantesParametros.GetInstance().GetParHomeStandardMostrarReporte();
                vm.ShowBtnPresupuestos = DS_RepresentantesParametros.GetInstance().GetParHomeStandardMostrarPresupuesto();

                content.Content = new HomeDashboardStandard()
                {
                    BindingContext = vm
                };
            }
            
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            Arguments.Values.CurrentModule = Enums.Modules.NULL;
            Arguments.Values.CliDatosOtros = null;
            Arguments.Values.CurrentClient = null;
            OperacionesPage.CloseVisit = false;
            Arguments.Values.IsPushMoneyRotacion = false;

            if (BindingContext is HomeViewModel vm)
            {
                vm.RecycleImages();
                vm.ListenForLocationUpdatesIfPermission();
                //vm.SaldarReciboHuerfano();
                vm.VerificarVisitaPendienteCierre();
                vm.RefreshNewsCount();
                vm.ResetCuaSecuencia();
                vm.AplicarCargasAutomaticas();
                vm.CarPendingCount();
                
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

        protected override bool OnBackButtonPressed()
        {
            ((HomeViewModel)BindingContext).AlertCerrarSesion();
            return true;
        }
   
    }
}