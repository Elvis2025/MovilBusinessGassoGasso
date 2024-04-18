using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model.Internal.Structs.services
{
    public class CambiarContrasenaArgs
    {
        public string OldPass { get; set; }
        public string NewPass { get; set; }
        public string RepCodigo { get; set; }
        public string Suscriptor { get; set; }
    }
}
