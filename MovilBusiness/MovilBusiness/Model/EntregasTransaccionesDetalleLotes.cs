using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class EntregasTransaccionesDetalleLotes
    {
        public string rowguid { get; set; }
        public string RepCodigo { get; set; }
        public int EntSecuencia { get; set; }
        public int EntPosicion { get; set; }
        public string EntLote { get; set; }
        public double EntCantidad { get; set; }
        public double EntCantidadDetalle { get; set; }
    }
}
