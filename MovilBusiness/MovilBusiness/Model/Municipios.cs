using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class Municipios
    {
        public int MunID { get; set; }
        public int ProID { get; set; }
        public string MunDescripcion { get; set; }
        public string rowguid { get; set; }

        public override string ToString()
        {
            return MunDescripcion;
        }
    }
}
