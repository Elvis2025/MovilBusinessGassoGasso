using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class RequisicionesInventarioDetalle
    {
        public int ReqSecuencia { get; set; }
        public int ReqPosicion { get; set; }
        public string RepCodigo { get; set; }
        public int ProID { get; set; }
        public double ReqCantidad { get; set; }
        public double ReqCantidadDetalle { get; set; }
        public string ProCodigo { get; set; }
        public string ProDescripcion { get; set; }
        public string UnmCodigo { get; set; }
    }
}
