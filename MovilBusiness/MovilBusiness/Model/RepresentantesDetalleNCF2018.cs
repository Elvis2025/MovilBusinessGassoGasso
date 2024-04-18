using MovilBusiness.Views.Components.TemplateSelector;

namespace MovilBusiness.Model
{
    public class RepresentantesDetalleNCF2018 : RowLinker
    {
        public string RepCodigo { get; set; }
        public string RedTipoComprobante { get; set; }
        public int ReDNCFMax { get; set; }
        public string ReDSerie { get; set; }
        public string ReDAICF { get; set; }
        public int ReDNCFActual { get; set; }
        public string ReDFechaVencimiento { get; set; }

        public string rowguid { get; set; }
    }
}
