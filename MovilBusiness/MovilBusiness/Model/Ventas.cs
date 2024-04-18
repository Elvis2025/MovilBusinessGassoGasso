
using SQLite;

namespace MovilBusiness.Model
{
    public class Ventas
    {
        public string RepCodigo { get; set; }
        public int VenSecuencia { get; set; }
        public int CliID { get; set; }
        public string VenFecha { get; set; }
        public int VenEstatus { get; set; }
        public double VenTotal { get; set; }
        public double VenTotalSinItbis { get; set; }
        public int ConID { get; set; }
        public string VenNCF { get; set; }
        public string VenReferencia { get; set; }
        public int CuaSecuencia { get; set; }
        public int PedSecuencia { get; set; }
        public int VenCantidadCanastos { get; set; }
        public int VisSecuencia { get; set; }
        public string MonCodigo { get; set; }
        public string VenNCFFechaVencimiento { get; set; }
        public int VenCantidadImpresion { get; set; }
        public string SecCodigo { get; set; }

        public string ConDescripcion { get; set; }
        public string CliNombre { get; set; }
        public string CliNombreComercial { get; set; }
        public string CliCodigo { get; set; }
        public string CliCalle { get; set; }
        public string CliUrbanizacion { get; set; }
        public string CliRnc { get; set; }
        public string CliTelefono { get; set; }
        public string CliPropietario { get; set; }
        public string CliTipoComprobanteFAC { get; set; }
        [Ignore] public double VenTotalUnitario { get; set; }
        public double VenTotalItbis { get; set; }

        public string mbVersion { get; set; }
        public string rowguid { get; set; }

        public string RepVendedor { get; set; }

        public double VenDescuento { get; set; }
        public bool VenIndicadorOferta { get; set; }
        public double VenCantidad { get; set; }
        public double VenCantidadDetalle { get; set; }
        public double VenMontoSinItbis { get; set; }
        public double VenMontoItbis { get; set; }
        public double VenMontoTotal { get; set; }
        public double VenSubTotal { get; set; }
        public double VenPorCientoDsctoGlobal { get; set; }
        public double VenMontoDsctoGlobal { get; set; }

        public string VenOrdenCompra { get; set; }

    }

}
