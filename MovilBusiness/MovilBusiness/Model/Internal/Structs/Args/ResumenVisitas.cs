using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model.Internal.Structs.Args
{
    public class ResumenVisitas
    {
        public string TipoTransaccion { get; set; }
        public int Cantidad { get; set; }
        public double MontoTotal { get; set; }
    }
}
