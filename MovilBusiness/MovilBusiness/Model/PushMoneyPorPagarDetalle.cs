using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class PushMoneyPorPagarDetalle
    {
        public string PxpReferencia { get; set; }
        public int PxpPosicion { get; set; }
        public int ProID { get; set; }
        public double PxpCantidad { get; set; }
        public int PxpCantidadDetalle { get; set; }
        public double PxpPrecio { get; set; }
        public double PxpDescuento { get; set; }
        public double PxpItbis { get; set; }
        public bool PxpIndicadorOferta { get; set; }
        public string RepCodigo { get; set; }
        public string PxpLote { get; set; }

        public string ProDescripcion { get; set; }
        public string UnmCodigo { get; set; }

        [Ignore] public string Cantidad { get => PxpCantidad.ToString() + (PxpCantidadDetalle > 0 ? "/" + PxpCantidadDetalle : ""); }
        [Ignore] public double MontoTotal { get => ((PxpPrecio - PxpDescuento) + ((PxpPrecio - PxpDescuento) * (PxpItbis / 100.0))) * PxpCantidad; }
        [Ignore] public string MontoItbis { get => ((PxpPrecio - PxpDescuento) * (PxpItbis / 100.0)).ToString("F2") + " (" + PxpItbis + "%)"; }
    }
}
