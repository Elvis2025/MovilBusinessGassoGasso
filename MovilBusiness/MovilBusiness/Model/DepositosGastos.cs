using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class DepositosGastos
    {
        public string RepCodigo { get; set; }
        public int DepSecuencia { get; set; }
        public string DepFecha { get; set; }
        public int DepEstatus { get; set; }
        public int DepCantidadGastos { get; set; }
        public double DepMonto { get; set; }
        public int DepGastoDesde { get; set; }
        public int DepGastoHasta { get; set; }
        public int CuaSecuencia { get; set; }
    }
}
