using MovilBusiness.Views.Components.TemplateSelector;

namespace MovilBusiness.Model.Internal
{
    public class FacturasAvencerDelMes : RowLinker
    {
        public string Cliente { get; set; }
        public string Nombre { get; set; }
        public decimal Balance { get; set; }
        public string Factura { get; set; }
        public string FechaFact { get; set; }
        public string FechaVenc { get; set; }
        public int diasCredito { get; set; }

    }
}
