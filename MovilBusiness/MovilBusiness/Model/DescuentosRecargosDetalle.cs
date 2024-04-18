using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class DescuentosRecargosDetalle
    {
        public int DesID { get; set; }
        public int DRDSecuencia { get; set; }
        public double DRDCantidadInicial { get; set; }
        public double DRDCantidadFinal { get; set; }
        public double DRDPorciento { get; set; }
        public double DRDValor { get; set; }
        public string LipCodigo { get; set; }
        public double DRDMontoInicial { get; set; }
        public double DRDMontoFinal { get; set; }
        public string grpCodigo{ get; set; }
        public string UnmCodigo{ get; set; }
}
}
