using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class InventariosAlmacenesLotes
    {
        public int AlmID { get; set; }
        public int ProID { get; set; }
        public string ProLote { get; set; }
        public double InvCantidad { get; set; }
        public int InvCantidadDetalle { get; set; }
        public DateTime InvFechaActualizacion { get; set; }
        public string UsuInicioSesion { get; set; }
        public string rowguid { get; set; }
        public DateTime InvFechaVencimiento { get; set; }

        public string FechaVencimientoStr { get => InvFechaVencimiento.ToString("dd/MM/yyyy"); }
    }
}
