using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class PushMoneyPagosAplicacion
    {
        public string PushMoneyPagosrowguid { get; set; }
        public string RepCodigo { get; set; }
        public int ppaSecuencia { get; set; } 
        public string PxpReferencia { get; set; }
        public string PxpDocumento { get; set; }
        public double pxpValor { get; set; }

        public string RepSupervisor { get; set; }
        public string repCodigo2 { get; set; }
        public string RecNumeroERP { get; set; }
        public string rowguid { get; set; }
        public string ProCodigo { get; set; }
        public string ProDescripcion { get; set; }
        public double PxpCantidad { get; set; }
        public double PxpCantidadDetalle { get; set; }
        public double PxpPrecio { get; set; }
        public double PxpItbis { get; set; }
   
    }
}
