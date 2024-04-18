using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class RepresentantesEntregasSemanal
    {
        public string RepCodigo { get; set; }
        public int ProID { get; set; }
        public int ProCantidad { get; set; }
        public int ProCantidadDetalle { get; set; }
        public int SemSemana { get; set; }
        public int SemAno { get; set; }
        public string rowguid { get; set; }
    }
}
