using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class MarcasAuditoriasPrecios
    {
        public string CatCodigo { get; set; }
        public string MarCodigo { get; set; }
        public string MarDescripcion { get; set; }

        public override string ToString()
        {
            return MarDescripcion;
        }
    }
}
