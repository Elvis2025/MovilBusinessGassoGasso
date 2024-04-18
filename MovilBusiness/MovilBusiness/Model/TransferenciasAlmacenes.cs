using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class TransferenciasAlmacenes
    {
        public string RepCodigo { get; set; }
        public int TraID { get; set; }
        public int CuaSecuencia { get; set; }
        public int TraTipo { get; set; }
        public string AlmCodigoOrigen { get; set; }
        public string AlmCodigoDestino { get; set; }
        public int TraEstado { get; set; }
        public string TraFecha { get; set; }
        public string rowguid { get; set; }

        public string RepNombreDestino { get; set; }
    }
}
