using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class ReplicacionesTablas
    {
        public int RepID { get; set; }
        public string RepTabla { get; set; }
        public string RepColumnas { get; set; }
        public string RepCriterio { get; set; }
        public string RepScriptCreacion { get; set; }
        public string UsuInicioSesion { get; set; }
        public string RepFechaActualizacion { get; set; }
        public string rowguid { get; set; }

        public string RepTipoTabla { get; set; }
    }
}
