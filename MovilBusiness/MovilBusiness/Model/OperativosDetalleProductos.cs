using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class OperativosDetalleProductos
    {
        public string RepCodigo { get; set; }
        public int OpeID { get; set; }
        public int OpeSecuencia { get; set; }
        public int ProID { get; set; }
        public int OpeProductoCantidad { get; set; }

        public int OpeProductoCantidadPrescrita { get; set; }
        public string rowguid { get; set; }

        public string ProDescripcion { get; set; }

        public OperativosDetalleProductos Copy()
        {
            return (OperativosDetalleProductos)MemberwiseClone();
        }
    }
}
