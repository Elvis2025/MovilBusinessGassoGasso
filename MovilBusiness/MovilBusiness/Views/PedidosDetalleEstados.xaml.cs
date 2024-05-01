using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
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
    public partial class PedidosDetalleEstados : ContentPage
    {
        public Pedidos CurrentPedido { get; set; }
        public string tratamientoGlobal { get; set; }
        public string estatusRechazo { get; set; }
        public string estatusEntrega { get; set; }
        public string tratamientoCredito { get; set; }
        public string estadosERP { get; set; }
        public DS_Pedidos pedido{get;set;}

		public PedidosDetalleEstados (Pedidos CurrentPedido)
		{
			InitializeComponent ();
            this.CurrentPedido = CurrentPedido;
            pedido = new DS_Pedidos();
            getEstadosERPByPedidos();
            lbTratamientoGlobal.Text = tratamientoGlobal;
            lbEstatusRechazo.Text = estatusRechazo;
            lbEstatusEntrega.Text = estatusEntrega;
            lbTratamientoCredito.Text = tratamientoCredito;
        }

        public void getEstadosERPByPedidos()
        {

            estadosERP = pedido.GetPedidoEstadoERPBySecuencia(CurrentPedido.PedSecuencia);
            int index = 1;
            foreach(var estado in estadosERP)
            {
                switch (index)
                {
                    case 1:
                        tratamientoGlobal = pedido.GetEstadosErpByPedidos(estado.ToString(), Enum.GetName(typeof(EstadosErp), index));
                        break;
                    case 2:
                        estatusRechazo = pedido.GetEstadosErpByPedidos(estado.ToString(), Enum.GetName(typeof(EstadosErp), index));
                        break;
                    case 3:
                        estatusEntrega = pedido.GetEstadosErpByPedidos(estado.ToString(), Enum.GetName(typeof(EstadosErp), index));
                        break;
                    case 4:
                        tratamientoCredito = pedido.GetEstadosErpByPedidos(estado.ToString(), Enum.GetName(typeof(EstadosErp), index));
                        break;
                    default:
                        break;
                }

                index++;
            }

        }



    }
}