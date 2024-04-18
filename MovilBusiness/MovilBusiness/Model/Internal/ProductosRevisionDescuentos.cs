
namespace MovilBusiness.Model.Internal
{
    public class ProductosRevisionDescuentos
    {
        public double Precio { get; set; }
        public int ProID { get; set; }
        public string ProDescripcion { get; set; }
        public double PorcDescuentoOriginal { get; set; }
        public double DescuentoValorOriginal { get; set; }

        public double PorcDescuentoEditado { get; set; }
        public double DescuentoValorEditado { get; set; }

        public bool DescuentoManual { get; set; }

        public ProductosRevisionDescuentos Copy()
        {
            return (ProductosRevisionDescuentos)MemberwiseClone();
        }

        public string DescuentoFormatted { get => DescuentoValorEditado.ToString("N2") + "(" + PorcDescuentoEditado + "%)"; }
    }
}
