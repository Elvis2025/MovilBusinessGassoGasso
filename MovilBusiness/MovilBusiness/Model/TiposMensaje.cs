using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class TiposMensaje
    {
        public int TraID { get; set; }
        public int MenID { get; set; }
        public string MenDescripcion { get; set; }
        public string rowguid { get; set; }
        public string UsuInicioSesion { get; set; }
        public string TipFechaActualizacion { get; set; }

        public override string ToString()
        {
            return MenDescripcion;
        }
    }
}
