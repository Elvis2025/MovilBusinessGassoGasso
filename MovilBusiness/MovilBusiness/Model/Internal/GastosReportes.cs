

using MovilBusiness.Views.Components.TemplateSelector;

namespace MovilBusiness.Model.Internal
{
    public class GastosReportes : RowLinker
    {
        public string GasNombreProveedor { get; set; }
        public string GasRNC { get; set; }
        public string GasMontoTotal { get; set; }
        public string GasFecha { get; set; }
        public string GasNCF { get; set; }
        public string TipoGasto { get; set; }
        public string GasNoDocumento { get; set; }
        public string GasItebis { get; set; }
        public string GasPropina { get; set; }

        public string GasFechaDocumento { get; set; }

        public string GasBaseImponible { get; set; }

    }
}
