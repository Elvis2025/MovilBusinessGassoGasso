using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class CuentasXCobrarAplicaciones
    {
        public string RepCodigo { get; set; }
        public string CxcReferencia { get; set; }
        public int CXCSecuencia { get; set; }
        public string CxcTipoTransaccion { get; set; }
        public string CxcDocumento { get; set; }
        public string CxcFecha { get; set; }
        public string CxcFechaAplicacion { get; set; }
        public double CxcMonto { get; set; }
        public string CXCNCF { get; set; }
        public string CueFechaActualizacion { get; set; }
        public string UsuInicioSesion { get; set; }
        public string rowguid { get; set; }
        public string Sigla { get; set; }
        public string Descripcion { get; set; }
    }
}
