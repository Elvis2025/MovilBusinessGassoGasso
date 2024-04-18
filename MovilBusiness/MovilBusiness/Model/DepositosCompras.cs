
namespace MovilBusiness.Model
{
    public class DepositosCompras
    {
        public string RepCodigo { get; set; }
        public int DepSecuencia { get; set; }
        public double DepMonto { get; set; }
        public int DepCompraDesde { get; set; }
        public int DepCompraHasta { get; set; }
        public int DepCantidadCompra { get; set; }
        public string DepReferencia { get; set; }
        public double DepMontoCajaChica { get; set; }
        public string DepFecha { get; set; }
        public int DepEstatus { get; set; }
    }
}
