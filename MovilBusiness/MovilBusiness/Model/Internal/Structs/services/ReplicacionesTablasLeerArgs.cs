using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model.Internal.Structs.Args.services
{
    public struct ReplicacionesTablasLeerArgs
    {
        public string RepCodigo { get; set; }
        public string RepClave { get; set; } //must be in md5
        public string RepNombre { get; set; }
        public string RepSuscriptor { get; set; }
    }
}
