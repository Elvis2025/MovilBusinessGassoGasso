using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class RepresentantesSecuencias
    {
        public string RepCodigo { get; set; }
        public string RepTabla { get; set; }
        public int RepSecuencia { get; set; }
        public string UsuInicioSesion { get; set; }
        public string RepFechaActualizacion { get; set; }
        public string rowguid { get; set; }
    }
}
