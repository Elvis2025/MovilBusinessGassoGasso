using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class QuejasServicio
    {
        public string RepCodigo { get; set; }
        public int QueSecuencia { get; set; }
        public int CliID { get; set; }
        public string QueFecha { get; set; }
        public int QueIDMotivo { get; set; }
        public string queMotDescripcion { get; set; }
        public string QueComentario { get; set; }
        public int VisSecuencia { get; set; }
        public int QueEstatus { get; set; }
        public string RepCodigoVendedor { get; set; }
        public string rowguid { get; set; }
    }
}
