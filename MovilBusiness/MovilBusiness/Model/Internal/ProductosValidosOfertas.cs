using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model.Internal
{
    public class ProductosValidosOfertas
    {   
        public int ProID { get; set; }
        public int CliID { get; set; }

        public bool TieneOferta { get; set; }
        public bool TieneDescuento { get; set; }

        public double PorcientoDescuento { get; set; }

        public bool TieneDescuentoEscala{get;set;}
        public string UnmCodigo { get;set;}
        public string UnmCodigoOferta { get;set;}
        public string OfeCaracteristicas { get;set;}
        public int TitID { get;set;}

    }
}
