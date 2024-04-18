using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class RecibosFormaPago
    {
        public string RepCodigo { get; set; }
        public int RecTipo { get; set; }
        public int RecSecuencia { get; set; }
        public int RefSecuencia { get; set; }
        public int ForID { get; set; }
        public int BanID { get; set; }
        public string RefNumeroCheque { get; set; }
        public string RefFecha { get; set; }
        public string RefFechaDocumento { get; set; }
        public bool RefIndicadorDiferido { get; set; }
        public string RefNumeroAutorizacion { get; set; }
        public double RefValor { get; set; }
        public string CXCReferencia { get; set; }
        public double RecPrima { get; set; }
        public double RecTasa { get; set; }
        public string MonCodigo { get; set; }
        public string SocCodigo { get; set; }
        public string cxcDocumento { get; set; }
        public int AutSecuencia { get; set; }
        public int cliid { get; set; }
        public int cliid2 { get; set; }
        public string rowguid { get; set; }

        public string BanNombre { get; set; }

        [Ignore]public string Futurista { get => RefIndicadorDiferido ? "Si" : "No"; }
        [Ignore]public string FormaPago
        {
            get
            {
                switch (ForID)
                {
                    case 1:
                        return "Efectivo";
                    case 2:
                        return "Cheque";
                    case 4:;
                        return "Transferencia";
                    case 5:
                        return "Retención";
                    case 6:
                        return "Tarjeta de crédito";
                    case 18:
                        return "Orden pago";
                    default:
                        return "";
                }
            }
        }

    }
}
