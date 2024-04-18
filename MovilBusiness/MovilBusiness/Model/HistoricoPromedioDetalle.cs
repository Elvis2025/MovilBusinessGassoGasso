using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class HistoricoPromedioDetalle
    {

        public int CliID { get; set; }       
        public string ProCodigo { get; set; }
        public string ProDescripcion { get; set; }
        public double HiPCantidadVendida { get; set; }
        public double HipMonto { get; set; }
        public double HipCantidadPromedio{ get; set; }
        public double hipcantidad{ get; set; }
        public string UnidadVenta { get; set; }
        public string SecCodigo{ get; set; }
    }
}
