using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class ConteosFisicos
    {
        public string RepCodigo { get; set; }
        public int ConSecuencia { get; set; }
        public string ConFecha { get; set; }
        public int CuaSecuencia { get; set; }
        public int ConEstatus { get; set; }
        public int ConEstatusConteo { get; set; }
        public string RepAuditor { get; set; }
        public string EstatusDescripcion { get; set; }
        public int AlmID { get; set; }
        public int ConCantidadLineas { get; set; }
    }
}
