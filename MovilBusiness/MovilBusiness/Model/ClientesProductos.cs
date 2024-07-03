using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class ClientesProductos
    {
        public string RepCodigo { get; set; }
        public int CliID { get; set; }
        public int ProId { get; set; }
        public string CliFechaActualizacion { get; set; }
        public string CliFechasYCantidades { get; set; }

        public DateTime fecha1 { get; set; }
        public DateTime fecha2 { get; set; }
        public DateTime fecha3 { get; set; }

        public int catidad1 { get; set; }
        public int catidad2 { get; set; }
        public int catidad3 { get; set; }
    }
}
