using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class MuestrasRespuestas
    {
        public string RepCodigo { get; set; }
        public int MueSecuencia { get; set; }
        public int PreID { get; set; }
        public string ResRespuesta { get; set; }
        public string ResFecha { get; set; }
        public string ResHora { get; set; }

        public string PreDescripcion { get; set; }
    }
}
