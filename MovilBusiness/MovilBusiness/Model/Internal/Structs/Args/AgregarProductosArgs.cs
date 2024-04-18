using MovilBusiness.model.Internal;

namespace MovilBusiness.Model.Internal.Structs.Args
{
    public class AgregarProductosArgs
    {
        public double Cantidad { get; set; }

        public double CantidadDetalleR { get; set; }
        public int Unidades { get; set; }
        public int InvArea { get; set; }
        public string InvAreaDescr { get; set; }
        public double Precio { get; set; }
        public bool IndicadorEliminar { get; set; }
        public double DescuentoManual { get; set; }
        public double DescPorcientoManual { get; set; }
        public string ComprasNoFactura { get; set; }
        public double DescuentoXLipCodigo { get; set; }
        public double? CantidadAlm { get; set; } = null;
        public double? CanTidadTramo { get; set; } = null;
        public double? CanTidadGond { get; set; } = null;
        public double? UnidadAlm { get; set; } = null;
        public double? UnidadGond { get; set; } = null;
        public bool IndicadorPromocion { get; set; }
        public int CantidadPiezas { get; set; }
        public string Lote { get; set; }
        public string LoteEntregado { get; set; }
        public string LoteRecibido { get; set; }

        public int CantidadOfertaManual { get; set; }
        public double ValorOfertaManual { get; set; }
        public ProductosTemp ProductoOferta { get; set; }
        public int ProUnidades { get; set; }
        public int MotId { get; set; } = -1;

        public bool IndicadorDocena { get; set; }

        public ProductosTemp ProductToAdd { get; set; } = null;

        public string FechaEntrega { get; set; }
        public string CedCodigo { get; set; }
        public string CedDescripcion { get; set; }

        public bool Presencia { get; set; }

        public int Facing { get; set; }

        public KV Atributo1 { get; set; }
        public KV Atributo2 { get; set; }

        public double PrecioOferta { get; set; }
        public int Caras { get; set; }
    }
}
