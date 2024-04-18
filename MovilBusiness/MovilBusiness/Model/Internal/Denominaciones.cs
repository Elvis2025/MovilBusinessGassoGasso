using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model.Internal
{
    public class Denominaciones
    {
        public int DenID { get; set; }
        public int DenTipo { get; set; }
        public string DenDescripcion { get; set; }
        public double DenValor { get; set; }
        public string DenImagen { get; set; }
        public string DenFechaActualizacion { get; set; }
        public string UsuInicioSesion { get; set; }
        public string rowguid { get; set; }
    }
}
