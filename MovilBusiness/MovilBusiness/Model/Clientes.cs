using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Utils;
using SQLite;
using System;

namespace MovilBusiness.model
{
    public class Clientes
    {
        public int CliID { get; set; }
        public string RepCodigo { get; set; }
        public string CliNombre { get; set; }
        public string CliCodigo { get; set; }
        public string CliTelefono { get; set; }
        public string CliFormasPago { get; set; }
        public string CliCalle { get; set; }
        public string CliCasa { get; set; }
        public string TerCodigo { get; set; }
        public string SecCodigo { get; set; }
        public string CliUrbanizacion { get; set; }
        public string CliContacto { get; set; }
        public string CliContactoCelular { get; set; }
        public double CliLimiteCredito { get; set; }
        public double CliLimiteCreditoSolicitado { get; set; }
        public int TiNID { get; set; }
        public int CanID { get; set; }
        public double CliPrecio { get; set; }
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
        public string CliCodigoDescuento { get; set; }
        public bool CliIndicadorExonerado { get; set; }
        public double CliPromedioCompra { get; set; }
        public int CliTipoCliente { get; set; }
        public int CliTipoLocal { get; set; }
        public string CliTipoComprobanteFAC { get; set; }
        public string CliNombreEmisionCheques { get; set; }
        public double CliEstimadoCompras { get; set; }
        public string CliTipoComprobanteNC { get; set; }
        public bool CliIndicadorPresentacion { get; set; }
        public string CliFechaUltimaVenta { get; set; }
        public double CliVentasAnioAnterior { get; set; }
        public double CliInventario { get; set; }
        public string cliSector { get; set; }
        public double CliPorcientoColocacion { get; set; }
        public string LiPCodigo { get; set; }
        public double CliLatitud { get; set; }
        public double CliLongitud { get; set; }
        public string rowguid { get; set; }
        public string UsuInicioSesion { get; set; }
        public int CliEstatusVisita { get; set; }
        public int ConID { get; set; }
        public string MonCodigo { get; set; }
        public string CliLicencia { get; set; }
        public string CldDirTipo { get; set; }
        public string CliEncargadoPago { get; set; }
        public bool CliIndicadorDeposito { get; set; }
        public string CliCorreoElectronico { get; set; }
        public string CliPropietario { get; set; }
        public string CliPropietarioDireccion { get; set; }
        public bool CliIndicadorOrdenCompra { get; set; }
        public bool CliIndicadorDepositaFactura { get; set; }
        public string CliPaginaWeb { get; set; }
        public string CliCedulaPropietario { get; set; }
        public string CliPropietarioTelefono { get; set; }
        public int RutEntregaID { get; set; }
        public string CliFamiliarNombre { get; set; }
        public string CliFamiliarDireccion { get; set; }
        public string CliFamiliarCedula { get; set; }
        public string CliFamiliarTelefono { get; set; }
        public string CliFamiliarCelular { get; set; }
        public string CliPropietarioCelular { get; set; }
        public string CliDatosOtros { get; set; }
        public string LipCodigoPM { get; set; }
        public string LipCodigoPMR { get; set; }
        public double CliMontoUltimaVenta { get; set; }
        public double CliVentasAnioActual { get; set; }
        public string CliFechaUltimoCobro { get; set; }
        public double CliMontoUltimoCobro { get; set; }
        public string CliContactoFechaNacimiento { get; set; }
        public string CliNombreComercial { get; set; }
        public double CliMontoPedidoSugerido { get; set; }
        public double CliValorDescuentoPromedio { get; set; }
        public double CliTasaDescuentoPromedio { get; set; }
        public int CliDiasCarteraNC { get; set; }
        public int CliDiasCartera { get; set; }
        public string CliFuente { get; set; }
        public string CedCodigo { get; set; }
        public string CliMotivo { get; set; }
        public string CliFrecuenciaVisita { get; set; }
        public string CliRutSemana1 { get; set; }
        public string CliRutSemana2 { get; set; }
        public string CliRutSemana3 { get; set; }
        public string CliRutSemana4 { get; set; }
        public string CliFechaCreacion { get; set; }
        public int CliTotal { get; set; }

        public int CliRutPosicion { get; set; }
        public int CliOrdenRuta { get; set; }
        public int CliEnumerator { get; set; }
        public string CliRegMercantil { get; set; }
        public string ProNombre { get; set; }
        public double CliPorcientoDsctoGlobal { get; set; }

        public string CliCat3 { get; set; }
        public string CliCat2 { get; set; }
        public string CliCat1 { get; set; }
        public string CliTipoCliente3 { get; set; }
        public string CliTipoCliente2 { get; set; }
        public string CliTipoCliente1 { get; set; }

        [Ignore]public bool ShowCliTipoCliente { get => !string.IsNullOrWhiteSpace(CliTipoCliente1) || !string.IsNullOrWhiteSpace(CliTipoCliente2) || !string.IsNullOrWhiteSpace(CliTipoCliente3); }
        [Ignore]public bool ShowCliCats { get => !string.IsNullOrWhiteSpace(CliCat1) || !string.IsNullOrWhiteSpace(CliCat2) || !string.IsNullOrWhiteSpace(CliCat3); }
        [Ignore]public bool ShowCliCat1 { get => !string.IsNullOrWhiteSpace(CliCat1); }
        [Ignore] public bool ShowCliCat2 { get => !string.IsNullOrWhiteSpace(CliCat2); }
        [Ignore] public bool ShowCliCat3 { get => !string.IsNullOrWhiteSpace(CliCat3); }
        [Ignore] public bool ShowCliTipo1 { get => !string.IsNullOrWhiteSpace(CliTipoCliente1); }
        [Ignore] public bool ShowCliTipo2 { get => !string.IsNullOrWhiteSpace(CliTipoCliente2); }
        [Ignore] public bool ShowCliTipo3 { get => !string.IsNullOrWhiteSpace(CliTipoCliente3); }

        [Ignore]public int ProvinciaId { get; set; }
        [Ignore]public int MunicipioId { get; set; }
    
        public string CliDatosOtrosLabel
        {
            get
            {
                if (Arguments.Values.CliDatosOtros == null || Arguments.Values.CliDatosOtros.Count == 0 || string.IsNullOrWhiteSpace(CliDatosOtros))
                {
                    return "";
                }

                var result = "";

                foreach (var d in Arguments.Values.CliDatosOtros)
                {
                    if (d.CodigoUso != null && CliDatosOtros.Contains(d.CodigoUso))
                    {
                        result += d.Descripcion + ", ";
                    }
                }

                if (!string.IsNullOrWhiteSpace(result))
                {
                    result = result.Substring(0, result.Length - 2);
                }

                return result;
            }
        }
        [Ignore] public string ClicodigoNombre { get { return CliCodigo + " - " + CliNombre; } }
        [Ignore] public string CliNombreCompleto { get { return CliNombre + " - " + CliCodigo; } }
        [Ignore] public string CliNombreRutaPos { get { return CliRutPosicion + " - " + CliNombre; } }

        [Ignore] public string CliEstadoVisitaIcon { get => Functions.GetEstatusVisitaIcon(CliEstatusVisita); }

        [Ignore] public bool ShowDatosOtros { get => !string.IsNullOrWhiteSpace(CliDatosOtros); }
        [Ignore] public bool ShowDireccion { get => !string.IsNullOrWhiteSpace(CliCalle); }
        [Ignore] public bool ShowCliNombreComercial { get => !string.IsNullOrWhiteSpace(CliNombreComercial) && DS_RepresentantesParametros.GetInstance().GetParShowCliNombreComercial(); }
        [Ignore] public string CliNombreLabel { get => Arguments.Values.CurrentModule == Enums.Modules.CONTEOSFISICOS ? "Usuario: " + CliNombre : "Cliente: " + CliNombre; }
        [Ignore] public bool ShowTelefono { get => !string.IsNullOrWhiteSpace(CliTelefono) && DS_RepresentantesParametros.GetInstance().GetCallCliTelefono(); }
        public Clientes Copy()
        {
            return (Clientes)MemberwiseClone();
        }
        [Ignore] public double Cliente_Balance { get; set; }
        [Ignore] public bool ShowLocation { get => (CliLatitud != 0 || CliLongitud != 0) && (CliLatitud != -1 && CliLongitud != -1); }
        [Ignore] public string CliNombreIndicar => DS_RepresentantesParametros.GetInstance().GetParClientesMostrarUrba()? CliUrbanizacion + "/" + cliSector : ProNombre;
    }
}