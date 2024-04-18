using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class PoliticasDevolucionDetalle
    {
        public int PodId { get; set; }
        public int PodSecuencia { get; set; }
        public int ProID { get; set; }
        public int PodTipo { get; set; }
        public int PodCantidadMeses { get; set; }
        public string rowguid { get; set; }
    }
}
