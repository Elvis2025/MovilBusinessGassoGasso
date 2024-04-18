using MovilBusiness.Views.Components.TemplateSelector;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class Gastos : RowLinker
    {
        public string RepCodigo { get; set; }
        public int GasSecuencia { get; set; }
        public string GasFecha { get; set; }
        public string GasFechaDocumento { get; set; }
        public string GasRNC { get; set; }
        public string GasNombreProveedor { get; set; }
        public string GasNCF { get; set; }
        public int GasTipo { get; set; }
        public string GasTipoDescripcion { get; set; }
        public string GasComentario { get; set; }
        public double GasMontoTotal { get; set; }
        public double GasMontoItebis { get; set; }
        public double GasItebis { get; set; }
        public int GasEstatus { get; set; }
        public int FopID { get; set; }
        public string GasCentroCosto { get; set; }
        public string FopDescripcion { get; set; }
        public string GasNoDocumento { get; set; }
        public string GasNCFFechaVencimiento { get; set; }
        public double GasPropina { get; set; }
        public string rowguid { get; set; }
        public string GasTipoComprobante { get; set; }
        public double GasBaseImponible { get; set; }

        [Ignore]public string FechaLabel
        {
            get
            {
                DateTime.TryParse(GasFecha, out DateTime fecha);

                return fecha.ToString("dd/MM/yyyy");
            }
        }
    }
}
