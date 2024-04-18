using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Model;
using MovilBusiness.Resx;
using MovilBusiness.ViewModel;
using MovilBusiness.views;
using MovilBusiness.Views.Components.ListItemRows;
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EntregasRepartidorPage : ContentPage
    {
        private bool FirstTime = true;
        public static bool CloseVisit = false;

        public EntregasRepartidorPage(bool IsConsulta = false)
        {
            var vm = new EntregasRepartidorViewModel(this, IsConsulta);

            BindingContext = vm;

            InitializeComponent();

            if (Arguments.Values.CurrentModule == Modules.RECEPCIONDEVOLUCION)
            {
                lblEntregasList.Text = AppResource.Receptions;
                Title = AppResource.ReturnsReception;
                editSearch.Placeholder = AppResource.FilterReceptionByNumber;
            }
            else
            {
                Arguments.Values.CurrentModule = Modules.ENTREGASREPARTIDOR;
            }

            if (IsConsulta)
            {
                Title = AppResource.CheckDeliveries;
            }

            if (DS_RepresentantesParametros.GetInstance().GetParEntregasRepartidorUsarRowDetallado()) {
                list.ItemTemplate = new DataTemplate(typeof(RowEntregasRepartidor2));
            }

            if (DS_RepresentantesParametros.GetInstance().GetParEntregasMultiples() && !IsConsulta)
            {
                ToolbarItems.Add(new ToolbarItem() { Text = AppResource.DeliverUpper, Order = ToolbarItemOrder.Primary, Command = vm.GoEntregaMultipleCommand });
            }

            if (IsConsulta)
            {
                ToolbarItems.Add(new ToolbarItem() { Text = AppResource.Print.ToUpper(), IconImageSource = "baseline_print_white_24", Order = ToolbarItemOrder.Primary, Command = vm.ImprimirEntregaCommand });
            }
        }

        private void List_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if(e.SelectedItem == null)
            {
                return;
            }

            if (!DS_RepresentantesParametros.GetInstance().GetParEntregasMultiples())
            {
                if (BindingContext is EntregasRepartidorViewModel vm)
                {
                    vm.GoDetalle(e.SelectedItem as EntregasRepartidorTransacciones);
                }
            }

            list.SelectedItem = null;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!string.IsNullOrWhiteSpace(Arguments.Values.SecCodigoParaCrearVisita) || CloseVisit)
            {
                if (CloseVisit)
                {
                    OperacionesPage.CloseVisit = true;
                }

                CloseVisit = false;
                Navigation.PopAsync(false);
                return;
            }

            if(BindingContext is EntregasRepartidorViewModel vm)
            {
                vm.CargarEntregas(FirstTime);

                FirstTime = false;
            }
        }

        private async void MenuItem_Clicked(object sender, EventArgs e)
        {
            if (BindingContext is EntregasRepartidorViewModel vm)
            {
                var item = (MenuItem)sender;

                var rowguid = item.CommandParameter.ToString();

                var rechazar = await DisplayAlert(AppResource.Warning, vm.IsConsulta ? AppResource.WantRevokeDeliveryQuestion : AppResource.WantRejectDeliveryQuestion, vm.IsConsulta ? AppResource.Revoke : AppResource.Reject, AppResource.Close);

                if (rechazar)
                {
                    vm.RechazarEntrega(rowguid);
                }
            }
        }

        protected override bool OnBackButtonPressed()
        {

            if(BindingContext is EntregasRepartidorViewModel vm && !vm.IsConsulta)
            {
                vm.ShowAlertSalir();
                return true;
            }

            return base.OnBackButtonPressed();
        }
    }
}