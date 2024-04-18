using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class AuditoriasPreciosDetalle
    {
        public string ProCodigo { get; set; }
        public string CatCodigo { get; set; }
        public string MarCodigo { get; set; }
        public int AudCantidad { get; set; }
        public double AudPrecio { get; set; }

        public string ProDescripcion { get; set; }
    }
}
