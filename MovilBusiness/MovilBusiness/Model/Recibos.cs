using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class Recibos
    {
        public string RepCodigo { get; set; }
        public string RecTipo { get; set; }
        public int RecSecuencia { get; set; }
        public int CliID { get; set; }
        public string RecFecha { get; set; }
        public string RecNumero { get; set; }
        public int RecEstatus { get; set; }
        public double RecMontoNcr { get; set; }
        public double RecMontoDescuento { get; set; }
        public double RecMontoEfectivo { get; set; }
        public double RecMontoCheque { get; set; }
        public double RecMontoChequeF { get; set; }
        public double RecMontoTransferencia { get; set; }
        public double RecMontoSobrante { get; set; }
        public int CuaSecuencia { get; set; }
        public int VisSecuencia { get; set; }
        public int DepSecuencia { get; set; }
        public double RecRetencion { get; set; }
        public string RecDivision { get; set; }
        public string AreactrlCredit { get; set; }
        public string MonCodigo { get; set; }
        public double RecMontoTarjeta { get; set; }
        public string SecCodigo { get; set; }
        public string OrvCodigo { get; set; }
        public string odvCodigo { get; set; }
        public int RecCantidadDetalleAplicacion { get; set; }
        public int RecCantidadDetalleFormaPago { get; set; }
        public int RecCantidadImpresion { get; set; }
        public double RecTotal { get; set; }
        public string UsuInicioSesion { get; set; }
        public string RecFechaActualizacion { get; set; }
        public string rowguid { get; set; }
        public string RecFechaSincronizacion { get; set; }
        public string mbVersion { get; set; }
        public string RepSupervisor { get; set; }
        public double RecTasa { get; set; }
        public double RecValor { get; set; }
        public string cxcDocumento { get; set; }
        public string cxcReferencia { get; set; }

        public string CliCodigo { get; set; }
        public string CliNombre { get; set; }
        public string CliContacto { get; set; }
        public string CliCalle { get; set; }
        public string CliUrbanizacion { get; set; }
        public string CliRNC { get; set; }
        public string CliTelefono { get; set; }
        public string CliSector { get; set; }

        public bool confirmado { get; set; }
        public string MonSigla { get; set; }

        public string RepVendedor { get; set; }

        public string RefFecha { get; set; }
        public int CliIndicadorPresentacion { get; set; }
        public string ofvCodigo { get; set; }

        public string CliRepCodigo { get; set; }
        public bool IsVisibleAnulado => RecEstatus == 0;

    }
}
