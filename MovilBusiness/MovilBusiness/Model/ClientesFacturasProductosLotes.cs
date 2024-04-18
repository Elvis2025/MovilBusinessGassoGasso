using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class ClientesFacturasProductosLotes
    {
        public int Cliid { get; set; }
        public string CFPLFactura { get; set; }
        public int ProID { get; set; }
        public string CFPLLote { get; set; }
        public string CFPLFechaVencimiento { get; set; }
        public int CFPLCantidadVendida { get; set; }
        public int CFPLCantidadOferta { get; set; }

        public override string ToString()
        {
            return CFPLFactura;
        }
    }
}
