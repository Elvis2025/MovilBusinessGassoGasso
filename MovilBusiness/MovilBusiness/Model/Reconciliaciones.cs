using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class Reconciliaciones
    {
        public string RepCodigo { get; set; }
        public int RecSecuencia { get; set; }
        public int RecEstatus { get; set; }
        public string RecFecha { get; set; }
        public string RecTipo { get; set; }
        public int CliID { get; set; }


        //from clientes
        public string CliSector { get; set; }
        public string CliCodigo { get; set; }
        public string CliNombre { get; set; }



        public int RecCantidadImpresion { get; set; }
    }
}
