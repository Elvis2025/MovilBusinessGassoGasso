using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class HistoricoFacturas
    {
        public string RepCodigo { get; set; }
        public string idReferencia { get; set; }
        public int CliID { get; set; }
        public string HifDocumento { get; set; }
        public string HifFecha { get; set; }
        public string HifFechaVencimiento { get; set; }
        public double HifMonto { get; set; }

        public string HiFNCF { get; set; }

        public double HifPorcientoDsctoGlobal { get; set; }
    }
}
