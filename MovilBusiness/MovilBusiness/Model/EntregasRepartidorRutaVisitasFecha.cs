using MovilBusiness.Configuration;
using MovilBusiness.Utils;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class EntregasRepartidorRutaVisitasFecha
    {
        public int EnrSecuencia { get; set; }
        public string RepCodigo { get; set; }
        public string RutFecha { get; set; }
        public int CliID { get; set; }
        public int RutPosicion { get; set; }
        public string CliNombre { get; set; }
        public string CliCodigo { get; set; }
        public string CliDatosOtros { get; set; }
        public string CliCalle { get; set; }
        public string RutEstado { get; set; }
        public string rowguid { get; set; }

    }
}
