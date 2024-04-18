using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model.Internal
{
    public class RecibosResumen
    {
        public double Sobrante { get; set; }
        public double Efectivo { get; set; }
        public double Cheques { get; set; }
        public double Transferencias { get; set; }
        public double Retencion { get; set; }
        public double TarjCredito { get; set; }
        public double Facturas { get; set; }
        public double NotasCredito { get; set; }
        public double Descuentos { get; set; }
        public double OrdenPago { get; set; }
        public double DiferenciaCambiaria { get; set; }
        public double Redondeo { get; set; }


        public double SobranteRaw { get; set; }
        public string LabelFaltanteText { get => SobranteRaw > 0 ? "SOBRANTE" : SobranteRaw < 0 ? "FALTANTE" : "SOBRANTE/FALTANTE"; }
    }
}
