using MovilBusiness.DataAccess;
using MovilBusiness.Utils;
using SQLite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MovilBusiness.Model
{
    public class HistoricoFacturasDetalle
    {
        public string RepCodigo { get; set; }
        public int idReferencia { get; set; }
        public int ProID { get; set; }
        public double HiFCantidad { get; set; }
        public double HiFPrecio { get; set; }
        public double HifDesPorciento { get; set; }
        public double HifDescuento { get; set; }
        public double HifItbis { get; set; }
        public string ProDescripcion { get; set; }
        public string HifLote { get; set; }
        public string UnidadVenta { get; set; }

        [Ignore] public string cantidad_Unidad { get => (DS_RepresentantesParametros.GetInstance().GetParHistoricoFacturaCantidadUnidad() ? (Math.Truncate(HiFCantidad) + "/" + ((HiFCantidad - Math.Truncate(HiFCantidad)) * Functions.GetProUnidades(ProID))) : HiFCantidad.ToString()); }
        [Ignore] public string MontoItbis { get => Functions.RoundTwoPositions(((HiFPrecio - HifDescuento) * (HifItbis / 100.0)), 2) + "("+HifItbis+"%)"; }
        [Ignore] public double MontoTotal { get => (DS_RepresentantesParametros.GetInstance().GetParHistoricoFacturasTotalPrecioProducto() ? (HiFPrecio * HiFCantidad) : ((HiFPrecio - HifDescuento) + ((HiFPrecio - HifDescuento) * (HifItbis / 100.0))) * HiFCantidad); }
        [Ignore] public string Descuento { get => Functions.RoundTwoPositions(HifDescuento, 2) + "(" + HifDesPorciento + "%)"; }
    }
}
