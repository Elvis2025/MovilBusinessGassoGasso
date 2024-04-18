using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class DescuentoFacturas
    {
        public string RepCodigo { get; set; }
        public string CXCReferencia { get; set; }
        public int DeFDiaInicial { get; set; }
        public int DeFDiaFinal { get; set; }
        public double DeFPorciento { get; set; }
        public bool DefIndicadorItbis { get; set; }
        public double DefDescuento { get; set; }
        public double MontoTotal { get; set; }
        public string CxcFecha { get; set; }
        public string FechaInicio { get => (DateTime.Parse(CxcFecha).AddDays(DeFDiaInicial).ToString()); }
        public string FechaFin { get => (DateTime.Parse(CxcFecha).AddDays(DeFDiaFinal).ToString()); }
        public string FechaInicioFormat { get => DateTime.Parse(FechaInicio).ToString("dd-MM-yyyy"); }
        public string FechaFinFormat { get => DateTime.Parse(FechaFin).ToString("dd-MM-yyyy"); }

    }
}
