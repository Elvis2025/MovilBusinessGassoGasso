using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class TiposTransaccionesCXC
    {
        public int ttcID { get; set; }
        public string ttcDescripcion { get; set; }
        public string ttcReferencia { get; set; }
        public int ttcOrigen { get; set; }
        public bool ttcAplicaDescuento { get; set; }
        public string ttcSigla { get; set; }
        public string UsuInicioSesion { get; set; }
        public string TipFechaActualizacion { get; set; }
        public string rowguid { get; set; }
        public string ttcCaracteristicas { get; set; }
    }
}
