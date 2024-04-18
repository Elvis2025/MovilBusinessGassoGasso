
namespace MovilBusiness.Model
{
    public class CotizacionesDetalle
    {
        public string RepCodigo { get; set; }
        public int CotSecuencia { get; set; }
        public int CotPosicion { get; set; }
        public int ProID { get; set; }
        public int CliID { get; set; }
        public double CotCantidad { get; set; }
        public double CotCantidadDetalle { get; set; }
        public double CotPrecio { get; set; }
        public double CotItbis { get; set; }
        public double CotSelectivo { get; set; }
        public double CotAdValorem { get; set; }
        public double CotDescuento { get; set; }
        public bool CotIndicadorCompleto { get; set; }
        public bool CotIndicadorOferta { get; set; }
        public double CotDesPorciento { get; set; }
        public string UnmCodigo { get; set; }

        public string ProDescripcion { get; set; }
        public string ProCodigo { get; set; }
        public string CotFechaEntrega { get; set; }
        public string CedCodigo { get; set; }
        public double ProUnidades { get; set; }
        public int OfeID { get; set; }
        public string rowguid { get; set; }
    }
}
