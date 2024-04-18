using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class Territorios
    {
        public string TerCodigo { get; set; }
        public string TerDescripcion { get; set; }

        public override string ToString()
        {
            return TerDescripcion;
        }
    }
}
