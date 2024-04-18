using SQLite;

namespace MovilBusiness.model.Internal.Structs.Args
{
    public class DevolucionesProductosArgs
    {
        public string Accion { get; set; }
        public string Condicion { get; set; }
        public string Documento { get; set; }
        public int MotId { get; set; }
        public double cantidad { get; set; }
        public double cantidaddetalle { get; set; }
        public int cantidadoferta { get; set; }
        public string lote { get; set; }
        public string FechaVencimiento { get; set; }

        [Ignore] public int TipoLote { get; set; }


    }
}
