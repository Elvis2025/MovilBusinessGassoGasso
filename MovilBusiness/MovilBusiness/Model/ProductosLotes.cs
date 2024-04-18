using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class ProductosLotes
    {
        public int ProID { get; set; }
        public string PrlLote { get; set; }
        public string ProFechaActualizacion { get; set; }
        public string PrlFechaVencimiento { get; set; }
        public string rowguid { get; set; }

        public override string ToString()
        {
            return PrlLote;
        }
    }
}
