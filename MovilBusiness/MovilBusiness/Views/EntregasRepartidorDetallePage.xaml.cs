using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Resx;
using MovilBusiness.ViewModel;
using MovilBusiness.Views.Components.ListItemRows;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EntregasRepartidorDetallePage : ContentPage
    {
        private bool FirstTime = true;
        public static bool Finish = false;
       
        public EntregasRepartidorDetallePage(List<EntregasRepartidorTransacciones> entregas)
        {
            BindingContext = new EntregasRepartidorDetalleViewModel(this, entregas);

            Init();

            lblNumeroEntrega.IsVisible = false;
        }

        public EntregasRepartidorDetallePage(EntregasRepartidorTransacciones entrega)
        {
            BindingContext = new EntregasRepartidorDetalleViewModel(this, entrega);

            Init();

            switch (Arguments.Values.CurrentModule)
            {
                case Enums.Modules.RECEPCIONDEVOLUCION:
                    Title = AppResource.ReturnReception;
                    lblNumeroEntrega.Text = AppResource.NumberLabel + entrega.venNumeroERP;
                    break;
                case Enums.Modules.CONDUCES:

                    var parNombreConduce = DS_RepresentantesParametros.GetInstance().GetParConducesNombreModulo();

                    if (string.IsNullOrWhiteSpace(parNombreConduce))
                    {
                        Title = "Conduces";
                        lblNumeroEntrega.Text = AppResource.NumberLabel + entrega.venNumeroERP;
                    }
                    else
                    {
                        Title = parNombreConduce.Trim();
                        lblNumeroEntrega.Text = parNombreConduce.Trim() + " "+ AppResource.NumberLabel + entrega.venNumeroERP;
                    }

                    break;
            }
        }

        private void Init()
        {
            InitializeComponent();

            if (DS_RepresentantesParametros.GetInstance().GetParEntregasRepartidorUsarRowDeProductosSinDialog())
            {
                list.ItemTemplate = new DataTemplate(typeof(RowEntregaRepartidorDetalle2));
            }
            else if (DS_RepresentantesParametros.GetInstance().GetParConducesUsarRowSinDialog())
            {
                list.ItemTemplate = new DataTemplate(typeof(RowConduces2));
            }
        }

        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if(e.SelectedItem == null)
            {
                return;
            }

            if(BindingContext is EntregasRepartidorDetalleViewModel vm)
            {
                vm.EscanearProducto(e.SelectedItem as EntregasDetalleTemp);
            }

            list.SelectedItem = null;
        }

        protected override void OnAppearing()
        {
            if (Finish)
            {
                Finish = false;

                ((EntregasRepartidorDetalleViewModel)BindingContext).VerificarSiHayEntregasParaOtroSector();

                Navigation.PopAsync(false);
                return;
            }

            base.OnAppearing();

            if (FirstTime && BindingContext is EntregasRepartidorDetalleViewModel vm)
            {
                FirstTime = false;
                vm.CargarProductos();
            }

            if(Arguments.Values.CurrentModule == Enums.Modules.CONDUCES && BindingContext is EntregasRepartidorDetalleViewModel vm2)
            {
                vm2.ConducesRestablecerCantidadSolicitada();
            }
        }

        protected override bool OnBackButtonPressed()
        {
            AlertSalir();            
            return true;
        }

        private async void AlertSalir()
        {
            var result = await DisplayAlert(AppResource.Warning, AppResource.WantToLeaveQuestion, AppResource.Yes, AppResource.No);

            if (result)
            {
                await Navigation.PopAsync(true);
            }
        }

        protected override void OnDisappearing()
        {
            (BindingContext as EntregasRepartidorDetalleViewModel).IsUp = true;
            base.OnDisappearing();
        }
    }
}