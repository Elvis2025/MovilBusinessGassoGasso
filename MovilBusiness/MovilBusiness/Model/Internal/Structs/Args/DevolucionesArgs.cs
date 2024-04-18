using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model.Internal.Structs.Args
{
    public struct DevolucionesArgs
    {
        public string Accion { get; set; }
        public string Documento { get; set; }
        public string DevNCF { get; set; }
        public string DevCintillo { get; set; }
        public string DevTipo { get; set; }
        public int MotId { get; set; }
    }
}
