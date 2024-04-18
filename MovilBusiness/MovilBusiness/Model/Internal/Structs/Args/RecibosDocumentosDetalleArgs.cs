using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model.Internal.Structs.Args
{
    public class RecibosDocumentosDetalleArgs
    {
        public RecibosDocumentosTemp Factura { get; set; }
        public double Aplicado { get; set; }
        public DescFactura Descuento { get; set; }
        public bool IsForSaldo { get; set; }
        public bool CalcularDesc { get; set; }
        public bool RecVerificarDesc { get; set; }

        public double Desmonte { get; set; }
        public bool CalcularDesmonte { get; set; }
        public bool RecVerificarCalcularDesmonte { get; set; }
    }
}
