using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class Muestras
    {
        public string RepCodigo { get; set; }
        public int MueSecuencia { get; set; }
        public string MueFecha { get; set; }
        public int EstID { get; set; }
        public int CLIID { get; set; }
        public string CliNombre { get; set; }

        public string EstNombre { get; set; }
    }
}
