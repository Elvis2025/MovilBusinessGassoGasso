using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    class CambiosDetalle
    {
        public string RepCodigo { get; set; }
        public int CamSecuencia { get; set; }
        public int CamPosicion { get; set; }
        public int ProID { get; set; }
        public int CamCantidad { get; set; }
        public double CamPrecio { get; set; }
        public double CamItbis { get; set; }
        public double CamSelectivo { get; set; }
        public double CamAdValorem { get; set; }
        public double CamDescuento { get; set; }
        public double CamTotalItbis { get; set; }
        public double CamTotalDescuento { get; set; }
        public int CamCantidadDetalle { get; set; }
        public int CamTipoTransaccion { get; set; }
        public bool CamIndicadorOferta { get; set; }

        public string CamFechaActualizacion { get; set; }
        public string RepSupervisor { get; set; }

        public string ProCodigo { get; set; }
        public string ProDescripcion { get; set; }
    }
}