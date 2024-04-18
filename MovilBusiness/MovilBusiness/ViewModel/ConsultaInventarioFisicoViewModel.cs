using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal.Structs.Args;
using MovilBusiness.Resx;
using MovilBusiness.viewmodel;
using MovilBusiness.views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class ConsultaInventarioFisicoViewModel : BaseViewModel
    {
        private DS_InventariosFisicos myInv;

        public List<InventarioFisico> Inventarios { get; set; }

        public ConsultaInventarioFisicoViewModel(Page page) : base(page)
        {
            myInv = new DS_InventariosFisicos();

            Inventarios = myInv.GetByInventariosByClient(Arguments.Values.CurrentClient.CliID);
        }

        public async void GoDetalle(InventarioFisico inventario)
        {
            try
            {
                if (IsBusy)
                {
                    return;
                }

                IsBusy = true;
                myInv.InsertarInventarioFisicoInTemp(inventario.invSecuencia, false);
                var title = AppResource.PhysicalInventoryDetail;

                var args = new PedidosDetalleArgs
                {
                    FechaEntrega = DateTime.Now,
                    ConId = 0,
                    DisenoDelRow = myParametro.GetFormatoVisualizacionProductos(),
                    PedOrdenCompra = null,
                    IsEditing = true,
                };

                Arguments.Values.CurrentModule = Enums.Modules.INVFISICO;

                await PushAsync(new PedidosDetallePage(args, true) { Title = title, IsDetail = true });

                IsBusy = false;

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }
    }
}
