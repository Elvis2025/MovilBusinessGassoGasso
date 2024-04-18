using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model.Internal
{
    public class DescFactura
    {
        public double DescuentoValor{ get; set; }
        public double DescPorciento { get; set; }
        public double DefDescuento { get; set; }
        public bool IndicadorItbis { get; set; }
    }
}
