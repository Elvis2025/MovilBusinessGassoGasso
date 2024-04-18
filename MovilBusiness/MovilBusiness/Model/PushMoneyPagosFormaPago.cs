using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class PushMoneyPagosFormaPago
    {
        public string PushMoneyPagosrowguid { get; set; }
        public int ppfSecuencia { get; set; }
        public string RepCodigo { get; set; }
        public int ForID { get; set; }
        public double RefValor { get; set; }
        public double RecPrima { get; set; }
        public double RecTasa { get; set; }
        public double PusBonoCantidad { get; set; }
        public string MonCodigo { get; set; }
        public string SocCodigo { get; set; }
        public int AutSecuencia { get; set; }
        public string rowguid { get; set; }
        public int DenID { get; set; }
        public int PusCantidad { get; set; }
        public string DenDescripcion { get; set; }

        [Ignore]
        public string FormaPago
        {
            get
            {
                switch (ForID)
                {
                    case 1:
                        return "Efectivo";
                    case 2:
                        return "Cheque";
                    case 4:                        
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
