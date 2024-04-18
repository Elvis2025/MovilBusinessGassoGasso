using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class ListaPrecios
    {
        public string LipCodigo { get; set; }
        public int ProID { get; set; }
        public string MonCodigo { get; set; }
        public string UnmCodigo { get; set; }
        public double LipPrecio { get; set; }
        public double LipPrecioSugerido { get; set; }
        public string UsuInicioSesion { get; set; }
        public string LisFechaActualizacion { get; set; }
        public string rowguid { get; set; }
        public double LipDescuento { get; set; }
        public double LipFlete { get; set; }
        public string LipRangoPrecioMinimo { get; set; }
    }
}
