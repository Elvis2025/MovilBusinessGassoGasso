using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class VisitasResultados
    {
        public string RepCodigo { get; set; }
        public int VisSecuencia { get; set; }
        public int virSecuencia { get; set; }
        public int TitID { get; set; }
        public string VisComentario { get; set; }
        public double VisMontoTotal { get; set; }
        public double VisMontoSinItbis { get; set; }
        public int VisCantidadTransacciones { get; set; }

        public double VisMontoItbis { get; set; }
    }
}
