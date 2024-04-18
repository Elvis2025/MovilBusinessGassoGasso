using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class ConducesDetalle
    {
        public string RepCodigo { get; set; }
        public int conSecuencia { get; set; }
        public int ConPosicion { get; set; }
        public int ProID { get; set; }
        public int ConCantidad { get; set; }
        public double ConPrecio { get; set; }
        public double ConItbis { get; set; }
        public double ConSelectivo { get; set; }
        public double ConAdValorem { get; set; }
        public double ConDescuento { get; set; }
        public bool ConIndicadorOferta { get; set; }
        public string CedCodigo { get; set; }
        public int OfeID { get; set; }
        public double ConDesPorciento { get; set; }
        public int ConTipoOferta { get; set; }
        public string UnmCodigo { get; set; }
        public string rowguid { get; set; }

        public string ProCodigo { get; set; }
        public string ProDescripcion { get; set; }
        public int ProUnidades { get; set; }

        public int DevSecuencia { get; set; }
    }
}
