using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
  public  class ClientesDirecciones
    {
        public int CliiD { get; set; }
        public string CldDirTipo { get; set; }
        //public string Calle { get; set; }
        //public string DirTipo { get; set; }
        public string CldCalle { get; set; }
        public string CldCasa { get; set; }
        public string CldContacto { get; set; }
        public string CldFax { get; set; }
        public string CldSector { get; set; }
        public string CldTelefono { get; set; }

        public int ProID { get; set; }
        public int MunID { get; set; }

        public string LipCodigo { get; set; }
        public double CldLatitud { get; set; }
        public double CldLongitud { get; set; }

        public override string ToString()
        {
            return CldCalle;
        }
    }
}
