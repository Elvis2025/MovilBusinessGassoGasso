using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.Model;
using MovilBusiness.Resx;
using MovilBusiness.viewmodel;
using MovilBusiness.Views.Components.Modals;
using MovilBusiness.Views.Components.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class OperacionesPage : BasePage
	{
        private bool IsConsulting, FirstTime = true;
        public static bool CloseVisit { private get; set; } = false;
        private Sectores sectorArbitrario;

        public OperacionesPage(bool IsConsulting = false, Sectores sector = null, Action<int> visitaVirtualFinalizada = null, string TypeOfVisit = "", bool IsVirtual = false )
		{
            sectorArbitrario = sector;
            this.IsConsulting = IsConsulting;
            FirstTime = true;
            CloseVisit = false;

            var vm = new OperacionesViewModel(this, IsConsulting, sector, visitaVirtualFinalizada, TypeOfVisit: TypeOfVisit , IsBusyLoad:
                (value) => { progressind.IsVisible = value; progressindchill.IsRunning = value;});

            BindingContext = vm;

            InitializeComponent();

            if (EnableBackButtonOverride)
            {
                if (!IsConsulting)
                {
                    CustomBackButtonAction = vm.AlertCerrarVisita;
                }
                else
                {
                    CustomBackButtonAction = async () => { await Navigation.PopAsync(true); };
                }              
            }

            if(DS_RepresentantesParametros.GetInstance().GetParSeleccioneVisita() && !IsVirtual) 
                comboTipoVisita.SelectedItem = null;

            var parNombreConduce = DS_RepresentantesParametros.GetInstance().GetParConducesNombreModulo();

            if (!string.IsNullOrWhiteSpace(parNombreConduce))
            {
                lblConduce.Text = parNombreConduce;
            }

            if (!vm.Params.ParVisitaSincronizar)
            {
                ToolbarItems.Clear();
            }

            NavigationPage.SetBackButtonTitle(this, AppResource.Operations);

            if(Arguments.Values.CurrentClient.CliIndicadorPresentacion && !IsConsulting)
            {
                var menuClientePresentacion = new ToolbarItem() { Text = AppResource.ModifyPresentationCustomerData, Order = ToolbarItemOrder.Secondary };
                menuClientePresentacion.Clicked += (s, e) => 
                {
                    VisitasPresentacion visita = null;

                    if(BindingContext is OperacionesViewModel viewModel)
                    {
                        visita = viewModel.CurrentVisitaPresentacion;
                    }

                    Navigation.PushModalAsync(new ClientesPresentacionModal(visita) { OnSave = (v)=> 
                    {
                        if(BindingContext is OperacionesViewModel vmm)
                        {
                            vmm.CurrentVisitaPresentacion = v;
                        }
                       
                    } });
                };
                ToolbarItems.Add(menuClientePresentacion);
            }

        }

        protected override bool OnBackButtonPressed()
        {

            ((OperacionesViewModel)BindingContext).AlertCerrarVisita();
            progressind.IsVisible = false;
            progressindchill.IsRunning = false;
            return true;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                if (BindingContext is OperacionesViewModel vm)
                {
                    vm.LoadClientData();
                    vm.ResfreshTaskPending();
                    vm.ResfreshVentasPending();
                    vm.ResfreshEntregasFacturasPending();
                    vm.AplicarCargasAutomaticas();
                    if (!IsConsulting)
                    {
                        new DS_TransaccionesImagenes().DeleteTemp(false);

                        if (FirstTime && !CloseVisit)
                        {
                            vm.LoadSectorArbitrario(sectorArbitrario);
                            FirstTime = false;
                            vm.GoModuleAutomaticIfValid();
                            vm.ShowAlertNoRecibeProductoMayorAlAnio();
                            return;
                        }

                        if (!string.IsNullOrWhiteSpace(Arguments.Values.SecCodigoParaCrearVisita))
                        {
                            if (vm.LoadOtherSectorForEntrega())
                            {
                                return;
                            }
                        }

                        if (CloseVisit)
                        {
                            CloseVisit = false;
                            vm.CerrarVisita(true);
                        }
                    }                    
                }

            }
            catch(Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }

            Arguments.Values.CurrentModule = Modules.VISITAS;
            Arguments.Values.IsPushMoneyRotacion = false;            
        }
    }
}