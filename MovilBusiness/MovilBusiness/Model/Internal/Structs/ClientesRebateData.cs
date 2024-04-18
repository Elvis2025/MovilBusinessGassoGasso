using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model.Internal.structs
{
    public struct ClientesRebateData
    {
        public double Rebate { get; set; }
        public double Acumulado { get; set; }
        public double Diferencia { get => Rebate - Acumulado; }
    }
}
