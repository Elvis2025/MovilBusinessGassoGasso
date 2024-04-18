using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Views.Components.TemplateSelector;
using Newtonsoft.Json;
using SQLite;

namespace MovilBusiness.model.Internal
{
    public class Totales : RowLinker
    {
        public double SubTotal { get; set; }
        public double Descuento { get; set; }
        public double DescuentoGeneral { get; set; }
        public double PorCientoDsctoGeneral { get; set; }
        public double DescuentoOfertas { get; set; }
        public double Selectivo { get; set; }
        public double AdValorem { get; set; }
        public double Itbis { get; set; }
        public double Total { get; set; }

        public int NumeroTransaccion { get; set; }
        public int CantidadTotal { get; set; }
        /// <summary>
        /// ////////////////////////
        /// </summary>
        public double Cantidad { get; set; }
        public double CantidadDetalle { get; set; }
        public double AdValoremU { get; set; }
        public double SelectivoU { get; set; }
        public double Precio { get; set; }
        public double DescuentoU { get; set; }
        public int ItbisT { get; set; }
        public int ProUnidades { get; set; }
        public double DesPorciento { get; set; }
        public string CantidadCobros { get; set; }
        public double Flete { get; set; }

        [Ignore] public int CantidadTotalDocenas { get => CantidadTotal / 12; }
        [Ignore] public bool SeeCantDoc { get => DS_RepresentantesParametros.GetInstance().GetParShowCantidadDocenas(); }
        [Ignore][JsonIgnore]public bool ShowDescuentoOfertas { get => Arguments.Values.CurrentModule == Modules.VENTAS || Arguments.Values.CurrentModule == Modules.PEDIDOS; }
    }
}
