using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class InventariosAlmacenesRepresentantes
    {
        public string RepCodigo { get; set; }
        public int ProID { get; set; }
        public int invCantidad { get; set; }
        public int InvCantidadDetalle { get; set; }
        public string ProDescripcion { get; set; }
        public string ProCodigo { get; set; }
        public int ProUnidades { get; set; }
        public int AlmID { get; set; }

        public bool FormatForMoney { get; set; }

        [Ignore] public string InvCantidadLabel { get => FormatForMoney ? invCantidad.ToString("C2") : invCantidad.ToString() + (InvCantidadDetalle > 0 ? "/" + InvCantidadDetalle.ToString() : ""); }
        [Ignore] public string ProIndicator { get => invCantidad > 0 ? "#1976D2" : "LightGray"; }
    }
}
