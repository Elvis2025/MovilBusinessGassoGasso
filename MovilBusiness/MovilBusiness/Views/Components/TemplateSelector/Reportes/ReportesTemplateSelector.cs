using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using Xamarin.Forms;

namespace MovilBusiness.Views.Components.TemplateSelector.Reportes
{
    public class ReportesTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FacturasVencidasTemplate { get; set; }//**
        public DataTemplate GastosTemplate { get; set; } //**
        public DataTemplate GastosTotalesTemplate { get; set; } //**

        public DataTemplate InventarioTemplate { get; set; }//**
        public DataTemplate PedidosClientesTemplate { get; set; }//** para pedidos y ventas y devoluciones
        public DataTemplate SubTitleTemplate { get; set; }//**
        public DataTemplate SubTitleXPueblos { get; set; }//**
        public DataTemplate RecibosMontoTemplate { get; set; }//**
        //public DataTemplate RecibosDetalladoTemplate { get; set; } //usar el de inventario
        public DataTemplate BreakLineTemplate { get; set; } //**
        public DataTemplate MontoTotalTemplate { get; set; } //**
        public DataTemplate SaldoXAntiguedadTemplate { get; set; } //**
        public DataTemplate FacturasDelMesTemplate { get; set; } //**
        public DataTemplate NcfTemplate { get; set; } //**
        public DataTemplate PosiblesCobrosxDiaTemplate { get; set; } //**

        /*public DataTemplate VisitasTemplate { get; set; } // usar el mismo de inventario
        public DataTemplate VisitasDesempenoTemplate { get; set; }*/

        //public DataTemplate Header3ColumnsTemplate { get; set; }
        public DataTemplate PreventaporLineadeProductosTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {

            if (item is ReportesNameQuantity)
            {
                return PedidosClientesTemplate;
            } else if (item is FacturasVencidas)
            {
                return FacturasVencidasTemplate;
            } else if (item is Totales)
            {
                return MontoTotalTemplate;
            } else if (item is GastosReportes)
            {
                return GastosTemplate;
            } else if (item is GastosTotales)
            {
                return GastosTotalesTemplate;
            } else if (item is Inventarios)
            {
                return InventarioTemplate;
            } else if (item is SubTitlePueblos)
            {
                return SubTitleXPueblos;
            }else if (item is SubTitle)
            {
                return SubTitleTemplate;
            } else if(item is RecibosMontoResumen)
            {
                return RecibosMontoTemplate;
            }else if(item is ResumenLineaProductos)
            {
                return PreventaporLineadeProductosTemplate;
            }
            else if (item is SaldoXAntiguedadTitle)
            {
                return SaldoXAntiguedadTemplate;
            }
            else if (item is FacturasAvencerDelMes)
            {
                return FacturasDelMesTemplate;  
            }
            else if (item is RepresentantesDetalleNCF2018)
            {
                return NcfTemplate;  
            }
            else if (item is PosiblesCobrosDias)
            {
                return PosiblesCobrosxDiaTemplate;  
            }

            return BreakLineTemplate;
        }
    }
}
