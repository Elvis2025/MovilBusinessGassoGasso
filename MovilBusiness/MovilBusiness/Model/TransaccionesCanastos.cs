using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class TransaccionesCanastos
    {
        public string RepCodigo { get; set; }
        public int TraID { get; set; }
        public int TraSecuencia { get; set; }
        public string TraFecha { get; set; }
        public string RepVendedor { get; set; }
        public int CliID { get; set; }
        public int TitOrigen { get; set; }
        public int TitID { get; set; }
        public string rowguid { get; set; }

        public int TraCantidadCanastos { get; set; }

        public string CliNombre { get; set; }
        public string CliCodigo { get; set; }
        public string CliCalle { get; set; }
        public string CliUrbanizacion { get; set; }
    }
}
