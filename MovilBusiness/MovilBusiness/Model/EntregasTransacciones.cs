

namespace MovilBusiness.Model
{
    public class EntregasTransacciones
    {
        public string rowguid { get; set; }
        public string RepCodigo { get; set; }
        public int EntSecuencia { get; set; }
        public int TitID { get; set; }
        public int CliID { get; set; }
        public string EntFecha { get; set; }
        public int EntEstatus { get; set; }
        public double EntTotal { get; set; }
        public string EntNCF { get; set; }
        public bool EntIndicadorCompleto { get; set; }
        public int CuaSecuencia { get; set; }
        public int EntTipo { get; set; }
        public string RepVendedor { get; set; }
        public int VenSecuencia { get; set; }
        public int EntCantidadDetalle { get; set; }
        public int VisSecuencia { get; set; }
        public int ConID { get; set; }
        public string MonCodigo { get; set; }

        public string CliCodigo { get; set; }
        public string CliCalle { get; set; }
        public string CliNombre { get; set; }
        public string CliUrbanizacion { get; set; }
        public string CliTipoComprobanteFAC { get; set; }
        public string CliRNC { get; set; }

        public string SecCodigo { get; set; }

        public bool ShowVerDetalleBtn { get; set; } = true;

        public int EntCantidadDetalleLote { get; set; }
    }
}
