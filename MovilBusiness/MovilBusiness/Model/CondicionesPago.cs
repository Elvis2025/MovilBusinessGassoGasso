using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class CondicionesPago
    {
        public int ConID { get; set; }
        public string ConReferencia { get; set; }
        public string ConDescripcion { get; set; }
        public int ConDiasVencimiento { get; set; }
        public string rowguid { get; set; }
        public string UsuInicioSesion { get; set; }
        public string ConFechaActualizacion { get; set; }
        public double ConPorcientoDsctoGlobal { get; set; }

        public override string ToString()
        {
            return ConDescripcion;
        }
    }
}
