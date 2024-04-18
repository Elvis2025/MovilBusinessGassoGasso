using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class CuentasxCobrarDetalle
    {
        public string CxcReferencia { get; set; }
        public int CxcPosicion { get; set; }
        public int ProID { get; set; }
        public double CxcCantidad { get; set; }
        public int CxcCantidadDetalle { get; set; }
        public double CxcPrecio { get; set; }
        public double CxcDescuento { get; set; }
        public double CxcItbis { get; set; }
        public bool CxcIndicadorOferta { get; set; }
        public string RepCodigo { get; set; }
        public string CxcLote { get; set; }

        public string ProDescripcion { get; set; }
        public string UnmCodigo { get; set; }

        [Ignore]public string Cantidad { get => CxcCantidad.ToString() + (CxcCantidadDetalle > 0 ? "/" + CxcCantidadDetalle : ""); }
        [Ignore] public double MontoTotal { get => ((CxcPrecio - CxcDescuento) + ((CxcPrecio - CxcDescuento) * (CxcItbis / 100.0))) * CxcCantidad; }
        [Ignore] public string MontoItbis { get => ((CxcPrecio - CxcDescuento) * (CxcItbis / 100.0)).ToString("F2") + " (" + CxcItbis + "%)"; }
        
    }
}
