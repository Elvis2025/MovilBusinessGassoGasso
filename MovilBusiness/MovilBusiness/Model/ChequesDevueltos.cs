using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    class ChequesDevueltos
    {
        public string RepCodigo { get; set; }
        public string CliID { get; set; }
        public string BanID { get; set; }
        public string CheNumero { get; set; }
        public double CheMonto { get; set; }
        public string CheFecha { get; set; }
    }
}
