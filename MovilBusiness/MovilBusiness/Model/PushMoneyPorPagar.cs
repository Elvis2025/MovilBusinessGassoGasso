using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class PushMoneyPorPagar
    {
        public string RepCodigo { get; set; }
        public string PxpReferencia { get; set; }
        public int PxpTipoTransaccion { get; set; }
        public int PxpDias { get; set; }
        public string PxpSIGLA { get; set; }
        public int CliID { get; set; }
        public string PxpFecha { get; set; }
        public string PxpDocumento { get; set; }
        public double PxpBalance { get; set; }
        public double PxpMontoSinItbis { get; set; }
        public double PxpMontoTotal { get; set; }
        public string MonCodigo { get; set; }
        public string AreaCtrlCredit { get; set; }
        public double PxpNotaCredito { get; set; }
        public string PxpNCF { get; set; }
        public string PxpFechaEntrega { get; set; }
        public string PxpClasificacion { get; set; }
        public string PxpFechaActualizacion { get; set; }
        public string UsuInicioSesion { get; set; }
        public string rowguid { get; set; }
        public string PxpReferencia2 { get; set; }
        public string PxpFechaVencimiento { get; set; }
        public string PxpNCFAfectado { get; set; }
        public string PxpComentario { get; set; }
        public double PxpRetencion { get; set; }
        public int Estatus { get; set; }//usado en el modulo de entrega documentos para saber cuando fue agregada
        public int Origen { get; set; }
        public int PxpDiasEntrega { get; set; }
        public double PxpBalanceAcumulado { get; set; }


    }
}
