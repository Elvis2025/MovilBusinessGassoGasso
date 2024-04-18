using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class CuentasxCobrar
    {
        public string RepCodigo { get; set; }
        public string CxcReferencia { get; set; }
        public int CxcTipoTransaccion { get; set; }
        public int CxcDias { get; set; }
        public int CxcDiasVencido { get; set; }
        public string CxcSIGLA { get; set; }
        public int CliID { get; set; }
        public string CxcFecha { get; set; }
        public string CxcDocumento { get; set; }
        public double CxcBalance { get; set; }
        public double CxcMontoSinItbis { get; set; }
        public double CxcMontoTotal { get; set; }
        public string MonCodigo { get; set; }
        public string AreaCtrlCredit { get; set; }
        public double CxcNotaCredito { get; set; }
        public string CXCNCF { get; set; }
        public string CxcFechaEntrega { get; set; }
        public string CxcClasificacion { get; set; }
        public string CueFechaActualizacion { get; set; }
        public string UsuInicioSesion { get; set; }
        public string rowguid { get; set; }
        public string cxcReferencia2 { get; set; }
        public string cxcFechaVencimiento { get; set; }
        public string CXCNCFAfectado { get; set; }
        public string cxcComentario { get; set; }
        public double CxcRetencion { get; set; }
        public int Estatus { get; set; }//usado en el modulo de entrega documentos para saber cuando fue agregada
        public int Origen { get; set; }
        public int CxcDiasEntrega { get; set; }
        public double CxcTasa { get; set; }

        /// <summary>
        /// Utilizados en cobros para anular recibos desde la pantalla de cobros
        /// </summary>
        public string RecTipo { get; set; }
        public int RecEstatus { get; set; }
        public double CxcBalanceAcumulado { get; set; }

        public string FacturasyDias { get => CxcDias + "-" + CXCNCF; }

        public double CxcBalanceRecibos { get => !string.IsNullOrWhiteSpace(CxcSIGLA) && CxcSIGLA.Trim().ToUpper() == "RCB" ? Math.Abs(CxcBalance) * -1 : CxcBalance; }
        public double CxcMontoTotalRecibos { get=> !string.IsNullOrWhiteSpace(CxcSIGLA) && CxcSIGLA.Trim().ToUpper() == "RCB" ? Math.Abs(CxcMontoTotal) * -1 : CxcMontoTotal; }

        public string CxcColor { get; set; }
        public string CxcColorToshow { get => !string.IsNullOrEmpty(CxcColor) && CxcColor.Length == 7 ? CxcColor : "#000000FF"; }

        public double cxcDesmonte { get; set; }
    }
}
