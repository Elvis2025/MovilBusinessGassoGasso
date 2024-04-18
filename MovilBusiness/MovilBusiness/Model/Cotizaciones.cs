using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class Cotizaciones
    {
        public string RepCodigo { get; set; }
        public int CotSecuencia { get; set; }
        public int CliID { get; set; }
        public string CotFecha { get; set; }
        public int CotEstatus { get; set; }
        public int CotTotal { get; set; }
        public int CotIndicadorCompleto { get; set; }
        public string CotFechaEntrega { get; set; }
        public int CotIndicadorRevision { get; set; }
        public int CotTipo { get; set; }
        public int VisSecuencia { get; set; }
        public string MonCodigo { get; set; }
        public string SecCodigo { get; set; }
        public string orvCodigo { get; set; }
        public string ofvCodigo { get; set; }
        public bool CotIndicadorContado { get; set; }
        public string CldDirTipo { get; set; }
        public int CotCantidadImpresion { get; set; }

        public string CliNombre { get; set; }
        public string CliCalle { get; set; }
        public string CliUrbanizacion { get; set; }
        public string CliCodigo { get; set; }

        public string CliRnc { get; set; }
        public string CliTelefono { get; set; }

        public int ConID { get; set; }
    }
}
