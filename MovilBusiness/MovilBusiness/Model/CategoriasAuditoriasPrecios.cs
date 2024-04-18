using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class CategoriasAuditoriasPrecios
    {
        public string CatCodigo { get; set; }
        public string CatDescripcion { get; set; }

        public override string ToString()
        {
            return CatDescripcion;
        }
    }
}
