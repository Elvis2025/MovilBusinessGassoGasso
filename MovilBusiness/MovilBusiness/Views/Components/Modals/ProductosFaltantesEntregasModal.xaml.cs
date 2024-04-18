using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Model.Internal;
using MovilBusiness.Resx;
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
    public partial class ProductosFaltantesEntregasModal : ContentPage
    {
        public List<EntregasDetalleTemp> ProductosConFaltantes { get; set; }
        private DS_EntregasRepartidorTransacciones myEnt;
        public Action OnAccepted { get; set; }
        public Action OnCancel { get; set; }

        public ProductosFaltantesEntregasModal(List<EntregasDetalleTemp> productos, DS_EntregasRepartidorTransacciones ds)
        {
            myEnt = ds;
            BindingContext = this;

            ProductosConFaltantes = productos;

            InitializeComponent();
        }

        private async void Continuar(object sender, EventArgs args)
        {
            if (Arguments.Values.CurrentModule == Modules.ENTREGASREPARTIDOR && DS_RepresentantesParametros.GetInstance().GetParEntregasRepartidorValidarOfertas() && myEnt.SeQuitaraOferta())
            {
                if (DS_RepresentantesParametros.GetInstance().GetParEntregasOfertasTodoONada())
                {
                    await DisplayAlert(AppResource.Warning, AppResource.SomeProductsNotFullDeliveredWarning, AppResource.Aceptar);
                    return;
                }
                else
                {
                    await DisplayAlert(AppResource.Warning, AppResource.SomeProductsNotFullDeliveredOfferWillBeLost, AppResource.Aceptar);
                }
            }

            OnAccepted?.Invoke();

            await Navigation.PopModalAsync(false);
        }

        private async void Dismiss(object sender, EventArgs args)
        {
            OnCancel?.Invoke();
            await Navigation.PopModalAsync(false);
        }
    }
}