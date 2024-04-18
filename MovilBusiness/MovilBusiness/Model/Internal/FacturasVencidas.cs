using MovilBusiness.Views.Components.TemplateSelector;

namespace MovilBusiness.Model.Internal
{
    public class FacturasVencidas : RowLinker
    {
        public string CliNombre { get; set; }
        public string Factura { get; set; }
        public string Fecha { get; set; }
        public string MonSigla { get; set; }
        public double Balance { get; set; }
        public bool IsVisbleMoneda { get; set; }
        public int DiasFacturasvencidas { get; set; }
    }
}
