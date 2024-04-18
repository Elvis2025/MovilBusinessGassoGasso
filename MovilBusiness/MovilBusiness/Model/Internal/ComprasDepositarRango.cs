using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model.Internal
{
    public class ComprasDepositarRango
    {
        public int MinComSecuencia { get; set; }
        public int MaxComSecuencia { get; set; }

        public int CantidadCompras { get; set; }
        public double MontoComprado { get; set; }
    }
}
