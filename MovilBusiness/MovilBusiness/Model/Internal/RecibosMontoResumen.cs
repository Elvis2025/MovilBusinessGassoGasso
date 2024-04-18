using MovilBusiness.Views.Components.TemplateSelector;

namespace MovilBusiness.Model.Internal
{
    public class RecibosMontoResumen : RowLinker
    {
        public string RecSecuencia { get; set; }
        public string CliNombre { get; set; }
        public string CliCodigo { get; set; }
        public string RecDescuento { get; set; }
        public string RecMonto { get; set; }
        public string DepFecha { get; set; } //added
        public double RecMontoNcr { get; set; }
        public string Descuento { get => double.TryParse(RecDescuento, out double desc) ? desc.ToString("C2") : RecDescuento; }
        public string Monto { get => double.TryParse(RecMonto, out double monto) ? monto.ToString("C2") : RecMonto; }
    }
}
