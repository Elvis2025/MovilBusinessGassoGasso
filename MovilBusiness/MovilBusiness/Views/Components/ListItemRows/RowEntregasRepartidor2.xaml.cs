using MovilBusiness.Abstraction;
using MovilBusiness.DataAccess;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using MovilBusiness.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.ListItemRows
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RowEntregasRepartidor2 : ViewCell
    {
        public RowEntregasRepartidor2()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if(BindingContext is EntregasRepartidorTransacciones data)
            {
                if (data.enrEstatusEntrega == 0 && ContextActions.Count > 0)
                {
                    ContextActions.Clear();
                }

                /*if (!data.ShowVerDetalleBtn)
                {
                    lblVerDetalle.Text = "";
                }*/
            }
        }

        private async void MenuItem_Clicked(object sender, EventArgs e)
        {
            var item = (MenuItem)sender;

            if (Parent is ListView list && list.BindingContext is EntregasRepartidorViewModel vm)
            {
                var rowguid = item.CommandParameter.ToString();

                var rechazar = await Functions.DisplayAlert("Aviso", vm.IsConsulta ? "Deseas anular esta entrega?" : "Deseas rechazar esta entrega?", vm.IsConsulta ? "Anular" : "Rechazar", "Cancelar");

                if (rechazar)
                {
                    vm.RechazarEntrega(rowguid);
                }
            }
        }

        private void VerDetalleTapped(object sender, EventArgs args)
        {
            if(Parent is ListView list && list.BindingContext is ConsultaEntregasRepartidorViewModel vm && BindingContext is EntregasRepartidorTransacciones data)
            {
                vm.VerDetalleEntrega(data);
            }
            else if(Parent is ListView liste && liste.BindingContext is EntregasRepartidorViewModel evm && BindingContext is EntregasRepartidorTransacciones datae)
            {
                evm.VerDetalleEntrega(datae);
            }
        }

        private void GoDetalle(object sender, EventArgs args)
        {
            if (!DS_RepresentantesParametros.GetInstance().GetParEntregasMultiples() && Parent is ListView list && list.BindingContext is EntregasRepartidorViewModel vm && BindingContext is EntregasRepartidorTransacciones data)
            {
                vm.GoDetalle(data);
            }
        }

        private void StartCall(object sender, EventArgs e)
        {
            if (sender is ContentView con && con.Content is Label lbl)
            {
                BtnStartCall(lbl.Text);
            }
        }

        private async void BtnStartCall(string Phone)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Phone))
                {
                    return;
                }

                var result = await Functions.DisplayAlert("Llamar", "Desea llamar a este numero?", "Llamar " + Phone + "", "Cancelar");

                if (result)
                {
                    var dialer = DependencyService.Get<IDialerService>();
                    dialer.Call(Phone);
                }

            }
            catch (Exception e)
            {
                await Functions.DisplayAlert("Aviso", e.Message);
            }
        }
    }
}