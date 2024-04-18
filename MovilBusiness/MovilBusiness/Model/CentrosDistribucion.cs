using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class CentrosDistribucion
    {
        public string CedCodigo { get; set; }
        public string CedDescripcion { get; set; }

        public string CedReferencia { get; set; }

        [Ignore] public string ShowCedDescripcion { get => CedDescripcion.ToString(); }


    }
}
