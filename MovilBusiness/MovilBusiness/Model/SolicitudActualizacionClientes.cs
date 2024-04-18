using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class SolicitudActualizacionClientes
    {
        public string RepCodigo { get; set; }
        public int SACSecuencia { get; set; }
        public int VisSecuencia { get; set; }
        public string SACFecha { get; set; }
        public int CliID { get; set; }
        public string CliNombre { get; set; }
        public string CliCodigo { get; set; }
        public int TiNID { get; set; }
        public string CliTelefono { get; set; }
        public string CliCalle { get; set; }
        public string CliCasa { get; set; }
        public string CliUrbanizacion { get; set; }
        public string CliContacto { get; set; }
        public double CliLimiteCredito { get; set; }
        public int CanID { get; set; }
        public int CliPrecio { get; set; }
        public int ZonID { get; set; }
        public bool CliIndicadorCredito { get; set; }
        public int ClaID { get; set; }
        public int CliEstatus { get; set; }
        public int PaiID { get; set; }
        public int ProID { get; set; }
        public int MunID { get; set; }
        public string CliFax { get; set; }
        public string CliRNC { get; set; }
        public bool CliIndicadorCheque { get; set; }
        public double CliPromedioPago { get; set; }
        public string LiPCodigo { get; set; }
        public string CliCodigoDescuento { get; set; }
        public bool CliIndicadorExonerado { get; set; }
        public double CliPromedioCompra { get; set; }
        public string CliTipoComprobanteFAC { get; set; }
        public string CliTipoComprobanteNC { get; set; }
        public bool CliIndicadorPresentacion { get; set; }
        public string CliFechaUltimaVenta { get; set; }
        public string cliSector { get; set; }
        public int ConId { get; set; }
        public double CliPorcientoColocacion { get; set; }
        public string MonCodigo { get; set; }
        public string CliLicencia { get; set; }
        public float CliLongitud { get; set; }
        public float CliLatitud { get; set; }
        public string CldDirTipo { get; set; }
        public string CliEncargadoPago { get; set; }
        public bool CliIndicadorDeposito { get; set; }
        public string CliCorreoElectronico { get; set; }
        public string CliPropietario { get; set; }
        public int SACEstado { get; set; }
        public bool CliIndicadorOrdenCompra { get; set; }
        public bool CliIndicadorDepositaFactura { get; set; }
        public string CliPaginaWeb { get; set; }
        public string CliCedulaPropietario { get; set; }
        public string CliFormasPago { get; set; }
        public string CliDatosOtros { get; set; }
        public string CliContactoFechaNacimiento { get; set; }
        public int CliRutPosicion { get; set; }
        public string CliFrecuenciaVisita { get; set; }
        public string CliRutSemana1 { get; set; }
        public string CliRutSemana2 { get; set; }
        public string CliRutSemana3 { get; set; }
        public string CliRutSemana4 { get; set; }
        public int CliOrdenRuta { get; set; }
        public int CliTipoLocal { get; set; }
        public int CliTipoCliente { get; set; }

    }
}
