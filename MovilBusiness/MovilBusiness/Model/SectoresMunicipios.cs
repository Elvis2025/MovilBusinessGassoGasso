using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class SectoresMunicipios
    {
        public string MunCodigo { get; set; }
        public string SecCodigo { get; set; }
        public string SecNombre { get; set; }

        public override string ToString()
        {
            return SecNombre;
        }
    }
}
