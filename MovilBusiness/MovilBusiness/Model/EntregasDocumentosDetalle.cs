using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class EntregasDocumentosDetalle
    {
        public string RepCodigo { get; set; }
        public int EntSecuencia { get; set; }
        public int EntPosicion { get; set; }
        public string cxcSigla { get; set; }
        public string cxcFecha { get; set; }
        public int DocID { get; set; }
        public int EntCantidad { get; set; }
        public double EntMonto { get; set; }
        public string EntDocumento { get; set; }
        public string AreaCtrlCredit { get; set; }
        public string rowguid { get; set; }

        public int Dias { get; set; }
        public int Estatus { get; set; }
        public string formattedFecha { get; set; }
        [Ignore] public string RowColor { get => Estatus == 2 ? "#42A5F5" : "#FFFFFF"; }

        public EntregasDocumentosDetalle Copy()
        {
            return (EntregasDocumentosDetalle)MemberwiseClone();
        }
    }
}
