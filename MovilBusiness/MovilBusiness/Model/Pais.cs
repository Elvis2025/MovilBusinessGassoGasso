using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class Pais
    {
        public int PaiID { get; set; }
        public string PaiNombre { get; set; }
        public string PaiReferencia { get; set; }
        public string rowguid { get; set; }

        public override string ToString()
        {
            return PaiNombre;
        }
    }
}
