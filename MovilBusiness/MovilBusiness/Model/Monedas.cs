using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class Monedas
    {
        public string MonCodigo { get; set; }
        public string MonNombre { get; set; }
        public string MonSigla { get; set; }
        public double MonTasa { get; set; }
        public DateTime MonFechaActualizacion { get; set; }
        public override string ToString()
        {
            return MonNombre;
        }
    }
}
