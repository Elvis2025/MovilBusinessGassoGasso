using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    class Cambios
    {
        public string RepCodigo { get; set; }
        public int CamSecuencia { get; set; }
        public int CliID { get; set; }
        public string CamFecha { get; set; }
        public int CamEstatus { get; set; }
        public double CamTotal { get; set; }
        public string CamNCF { get; set; }
        public string CamReferencia { get; set; }
        public int CuaSecuencia { get; set; }
        public int VisSecuencia { get; set; }

        public string CamFechaActualizacion { get; set; }
        public string RepSupervisor { get; set; }

        public string CliNombre { get; set; }
        public string CliNombreComercial { get; set; }
        public string CliCodigo { get; set; }
        public string CliCalle { get; set; }
        public string CliUrbanizacion { get; set; }

    }
}
