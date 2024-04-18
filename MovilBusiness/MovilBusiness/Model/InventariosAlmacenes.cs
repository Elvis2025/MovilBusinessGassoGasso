using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class InventariosAlmacenes
    {
        public int AlmID { get; set; }
        public string AlmDescripcion { get; set; }
        public int InvCantidad { get; set; }
        public int InvCantidadDetalle { get; set; }


        [Ignore] public string Cantidad { get => InvCantidad+"/"+InvCantidadDetalle; }
    }
}
