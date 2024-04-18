using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class ConteosFisicosDetalle
    {
        public string RepCodigo { get; set; }
        public int ConSecuencia { get; set; }
        public int ConPosicion { get; set; }
        public int ProID { get; set; }
        public double ConCantidadLogica { get; set; }
        public double ConCantidadDetalleLogica { get; set; }
        public double ConCantidad { get; set; }
        public double ConCantidadDetalle { get; set; }
        public string ConLote { get; set; }

        public string ProDescripcion { get; set; }
        public string ProCodigo { get; set; }
        public double Precio { get; set; }
        public double Itbis { get; set; }

        [Ignore] public int ProUnidades { get; set; }
        [Ignore] public string Cantidad_UnidadesLogica { get => ConCantidadLogica.ToString() + "/" + ConCantidadDetalleLogica.ToString(); }
        [Ignore] public string Cantidad_UnidadesFisica { get => ConCantidad.ToString() + "/" + ConCantidadDetalle.ToString(); }
    }
}
