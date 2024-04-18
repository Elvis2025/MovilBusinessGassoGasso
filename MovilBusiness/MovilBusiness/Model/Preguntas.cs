using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class Preguntas
    {
        public int EstID { get; set; }
        public int PreID { get; set; }
        public int PreTipoPregunta { get; set; }
        public string PreDescripcion { get; set; }
        public int PreIdPMultiple { get; set; }
        public int PreIndicadorAleatorio { get; set; }
        public int PreOrden { get; set; }
    }
}
