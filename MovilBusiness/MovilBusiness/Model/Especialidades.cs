using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class Especialidades
    {
        public int EspID { get; set; }
        public string EspNombre { get; set; }
        public string EspReferencia { get; set; }

        public override string ToString()
        {
            return EspNombre;
        }
    }
}
