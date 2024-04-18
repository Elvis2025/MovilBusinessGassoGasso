using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class Provincias
    {
        public int ProID { get; set; }
        public int PaiID { get; set; }
        public string ProNombre { get; set; }
        public string rowguid { get; set; }

        public override string ToString()
        {
            return ProNombre;
        }
    }
}
