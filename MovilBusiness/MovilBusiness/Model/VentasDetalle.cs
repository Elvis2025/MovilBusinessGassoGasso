using Newtonsoft.Json;
using SQLite;

namespace MovilBusiness.Model
{
    public class VentasDetalle
    {
        public string RepCodigo { get; set; }
        public int VenSecuencia { get; set; }
        public int VenPosicion { get; set; }
        public int ProID { get; set; }
        public double VenCantidad { get; set; }
        public double VenCantidadDetalle { get; set; }
        public double VenPrecio { get; set; }
        public double VenItbis { get; set; }
        public double VenSelectivo { get; set; }
        public double VenAdValorem { get; set; }
        public double VenDescuento { get; set; }
        public double VenTotalItbis { get; set; }
        public double VenTotalDescuento { get; set; }
        public bool VenindicadorOferta { get; set; }
        public double VenDescPorciento { get; set; }
        public int VenCantidadEntregada { get; set; }
        public int VenCantidadEntregadaDetalle { get; set; }
        public string UnmCodigo { get; set; }
        public int VenContadoInicial { get; set; }
        public int VenContadorFinal { get; set; }
        public int OfeID { get; set; }
        public bool VenIndicadorIntroduccion { get; set; }

        public string ProCodigo { get; set; }
        public string ProDescripcion { get; set; }
        public int ProUnidades { get; set; }
        public string VenNCF { get; set; }
        public string CliNombre { get; set; }
        public double VentaTotal { get; set; }
        public string ProReferencia { get; set; }
        public string ProDatos3 { get; set; }
        public double PrecioUnidades { get; set; }        
        public double PrecioCajas { get; set; }
        public string VenLote { get; set; }


        [JsonIgnore] [Ignore] public double CantidadReal { get => (VenCantidadDetalle / (ProUnidades > 0 ? ProUnidades : 1)) + VenCantidad; }
        [JsonIgnore][Ignore] public double SubTotal { get => (VenPrecio + VenSelectivo + VenAdValorem - VenDescuento) * CantidadReal; }
    }
}
