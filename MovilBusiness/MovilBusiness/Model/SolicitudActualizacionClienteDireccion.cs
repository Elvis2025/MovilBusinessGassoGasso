using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class SolicitudActualizacionClienteDireccion
    {
        public string RepCodigo { get; set; }
        public int SolSecuencia { get; set; }
        public string SolFecha { get; set; }
        public int SolEstado { get; set; }
        public int CliID { get; set; }
        public string CldDirTipo { get; set; }
        public string CldCalle { get; set; }
        public string CldCasa { get; set; }
        public string CldSector { get; set; }
        public string CldContacto { get; set; }
        public int PaiID { get; set; }
        public int ProID { get; set; } = -1;
        public int MunID { get; set; } = -1;
        public string CldTelefono { get; set; }
        public string CldWhatsapp { get; set; }
        public double CliLatitud { get; set; } = -1;
        public double CliLongitud { get; set; } = -1;
        public string rowguid { get; set; }
    }
}
