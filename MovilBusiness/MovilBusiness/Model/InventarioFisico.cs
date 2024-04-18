using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class InventarioFisico
    {
        public string RepCodigo { get; set; }
        public int invSecuencia { get; set; }
        public int CliID { get; set; }
        public string infFecha { get; set; }
        public int CuaSecuencia { get; set; }
        public int VisSecuencia { get; set; }
        public int InvEstatus { get; set; }
        public string UsuInicioSesion { get; set; }
        public string rowguid { get; set; }
        public int InvArea { get; set; }
        public string InvAreaDescr { get; set; }
        public string mbVersion { get; set; }

        public int InvTotal { get; set; }
        public string InvEstado { get; set; }
    }

}
