using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class ColocacionProductosDetalle
    {
        public string RepCodigo { get; set; }
        public int ColSecuencia { get; set; }
        public int ColPosicion { get; set; }
        public int ProID { get; set; }
        public int? ColCantidad { get; set; }
        public int? ColCantidadDetalle { get; set; }
        public int CliID { get; set; }
        public int InvArea { get; set; }
        public string rowguid { get; set; }

        public string ProCodigo { get; set; }
        public string ProDescripcion { get; set; }

        public int ProUnidades { get; set; }
    }
}
