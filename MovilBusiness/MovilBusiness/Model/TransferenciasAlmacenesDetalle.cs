using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class TransferenciasAlmacenesDetalle
    {
        public int TraID { get; set; }
        public string RepCodigo { get; set; }
        public double TadCantidad { get; set; }
        public double TadCantidadDetalle { get; set; }
        public int TadPosicion { get; set; }
        public string ProCodigo { get; set; }
        public string rowguid { get; set; }

        public string ProDescripcion { get; set; }
    }
}
