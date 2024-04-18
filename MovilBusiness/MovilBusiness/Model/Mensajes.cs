using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class Mensajes
    {
        public string rowguid { get; set; }
        public int CliID { get; set; }
        public int MenID { get; set; }
        public string MenDescripcion { get; set; }
        public string MenFecha { get; set; }
        public int TraID { get; set; }
        public string RepCodigo { get; set; }
        public int DepID { get; set; }
        public int VisSecuencia { get; set; }
        public int TraSecuencia { get; set; }
        public string UsuInicioSesion { get; set; }
        public string MenFechaActualizacion { get; set; }

        public string CliNombre { get; set; }

    }
}
