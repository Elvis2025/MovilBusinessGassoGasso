using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model.Internal.Structs.Args
{
    public class AplicarNCArgs
    {
        public RecibosDocumentosTemp Factura { get; set; }
        public RecibosDocumentosTemp NC { get; set; }

        public double ValorAplicarManual { get; set; }
    }
}
