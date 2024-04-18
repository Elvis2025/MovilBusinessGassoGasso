using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class EntregasTransaccionesDetalle
    {
        public string rowguid { get; set; }
        public string RepCodigo { get; set; }
        public int EntSecuencia { get; set; }
        public int EntPosicion { get; set; }
        public int ProID { get; set; }
        public double EntPrecio { get; set; }
        public double EntCantidad { get; set; }
        public double EntCantidadDetalle { get; set; }
        public double EntCantidadSolicitada { get; set; }
        public double EntCantidadDetalleSolicitada { get; set; }
        public bool EntIndicadorOferta { get; set; }
        public double EntDescuento { get; set; }
        public double EntDescPorciento { get; set; }
        public double EntItbis { get; set; }
        public double EntSelectivo { get; set; }
        public double EntAdValorem { get; set; }


        public string ProCodigo { get; set; }
        public string ProDescripcion { get; set; }
        public int LinID { get; set; }
        public string LinReferencia { get; set; }
        public int ProUnidades { get; set; }
        public string UnmCodigo { get; set; }
        public string Lote { get; set; }

    }
}
