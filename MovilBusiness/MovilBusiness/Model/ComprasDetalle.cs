
namespace MovilBusiness.Model
{
    public class ComprasDetalle
    {
        public string RepCodigo { get; set; }
        public int ComSecuencia { get; set; }
        public int ComPosicion { get; set; }
        public int ProID { get; set; }
        public int ComCantidad { get; set; }
        public int ComCantidadDetalle { get; set; }
        public double ComPrecio { get; set; }
        public double ComItbis { get; set; }
        public double ComSelectivo { get; set; }
        public double ComAdValorem { get; set; }
        public double ComDescuento { get; set; }
        public double ComTotalItbis { get; set; }
        public double ComTotalDescuento { get; set; }
        public bool ComindicadorOferta { get; set; }
        public string cxcDocumento { get; set; }
        public string ProNombre { get; set; }
        public int ComCantidadAprobada { get; set; }

        public int ProUnidades { get; set; }
        public string ProCodigo { get; set; }
        public string ProDescripcion { get; set; }
    }
}
