using MovilBusiness.model;
using MovilBusiness.Model.Internal.Structs.Args;
using MovilBusiness.Resx;
using MovilBusiness.views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PedidosConfirmadosTap : TabbedPage
    {
        public PedidosConfirmadosTap (Pedidos CurrentPedido,PedidosDetalleArgs args, bool fromTransacciones = false, string cxcDocumento = null, CuentasxCobrar documento = null)
        {
            InitializeComponent();
            var estadosPedidosDetallesTap = new PedidosDetalleEstados(CurrentPedido);

            var pedidosDetallesTap = new PedidosDetallePage(args, fromTransacciones)
            { 
                Title = AppResource.Detail,
                IsDetail = true
            };

            Children.Add(pedidosDetallesTap);
            Children.Add(estadosPedidosDetallesTap);
        }


    }
}