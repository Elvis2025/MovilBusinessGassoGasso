using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class ReplicacionesSuscriptores
    {
        public int RepID { get; set; }
        public string RepSuscriptor { get; set; }
        public string RepFechaCargaInicial { get; set; }
        public string RepFechaUltimaSincronizacion { get; set; }
        public int resEstado { get; set; }
        public string UsuInicioSesion { get; set; }
        public string RepFechaActualizacion { get; set; }
        public int RsuTipo { get; set; }
        public string rowguid { get; set; }
    }
}
