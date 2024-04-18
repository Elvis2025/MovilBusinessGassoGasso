using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class EntregasDocumentos
    {
        public string RepCodigo { get; set; }
        public int EntSecuencia { get; set; }
        public string EntRecibidoPor { get; set; }
        public string EntFecha { get; set; }
        public int EntEstatus { get; set; }
        public int CliID { get; set; }
        public string rowguid { get; set; }
    }
}
