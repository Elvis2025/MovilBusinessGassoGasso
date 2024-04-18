using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class Entregas
    {
        public string RepCodigo { get; set; }
        public int EntSecuencia { get; set; }
        public int EntTipo { get; set; }
        public int CliID { get; set; }
        public string EntFecha { get; set; }
        public string rowguid { get; set; }

        public int EntCantidadCanastos { get; set; }
    }
}
