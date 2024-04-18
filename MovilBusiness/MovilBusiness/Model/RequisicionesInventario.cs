using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class RequisicionesInventario
    {
        public string RepCodigo { get; set; }
        public string ReqSecuencia { get; set; }
        public string ReqFecha { get; set; }
        public int ReqEstatus { get; set; }
        public int ReqCantidadDetalle { get; set; }
        public int CuaSecuencia { get; set; }
        public string rowguid { get; set; }

        public string EstDescripcion { get; set; }
    }
}
