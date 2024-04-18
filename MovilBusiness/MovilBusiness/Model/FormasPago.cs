using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class FormasPago
    {
        public int FopID { get; set; }
        public string FopDescripcion { get; set; }
        public string FopReferencia { get; set; }
        public int FopCantidadPermitida { get; set; }
    }
}
