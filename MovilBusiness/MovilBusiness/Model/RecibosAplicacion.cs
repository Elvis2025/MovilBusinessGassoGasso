using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class RecibosAplicacion
    {
        public string rowguid { get; set; }
        public string RepCodigo { get; set; }
        public string RecTipo { get; set; }
        public int RecSecuencia { get; set; }
        public int ReaSecuencia { get; set; }
        public string SocCodigo { get; set; }
        public string CXCReferencia { get; set; }
        public double RecValor { get; set; }
        public double RecDescuento { get; set; }
        public double RecDescuentoDesmonte { get; set; }
        public bool RecIndicadorSaldo { get; set; }
        public string CxcSigla { get; set; }
        public double RecPorcDescuento { get; set; }
        public double RecMontoADescuento { get; set; }
        public string CxCDocumento { get; set; }
        public string CXCFechaVencimiento { get; set; }
        public string CXCFecha { get; set; }
        public double RecValorSinImpuesto { get; set; }
        public double CxcBalance { get; set; }
        public int cliid { get; set; }
        public int cliid2 { get; set; }
        public int AutID { get; set; }
        public bool DefIndicadorItbis { get; set; }
        public double RecTasa { get; set; }
        public string MonCodigo { get; set; }
        public double RecItbis { get; set; }
        [Ignore] public string Saldo { get => RecIndicadorSaldo ? "Si" : "No"; }
    }
}
