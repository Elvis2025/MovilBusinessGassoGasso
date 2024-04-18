using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class EntregasRepartidorTransaccionesDetalle
    {
        public int EnrSecuencia { get; set; }
        public int TitID { get; set; }
        public string RepCodigo { get; set; }
        public int TraSecuencia { get; set; }
        public int TraPosicion { get; set; }
        public int ProID { get; set; }
        public int CliID { get; set; }
        public double TraCantidad { get; set; }
        public double TraCantidadDetalle { get; set; }
        public double TraPrecio { get; set; }
        public double TraItbis { get; set; }
        public double TraSelectivo { get; set; }
        public double TraAdValorem { get; set; }
        public double TraDescuento { get; set; }
        public bool TraIndicadorCompleto { get; set; }
        public bool TraIndicadorOferta { get; set; }
        public string CedCodigo { get; set; }
        public int OfeID { get; set; }
        public double TraDesPorciento { get; set; }
        public int TraTipoOferta { get; set; }
        public string UnmCodigo { get; set; }
        public double TraCantidadInventario { get; set; }
        public int AutSecuencia { get; set; }
        public double TraFlete { get; set; }
        public string RepSupervisor { get; set; }
        public int TraEstatus { get; set; }

        public double CantidadEntregada { get; set; }
        public double CantidadEntregadaDetalle { get; set; }

        public int MotIdDevolucion { get; set; }
        public string Documento { get; set; }
        public string FechaVencimiento { get; set; }

        public bool UsaLote { get; set; }

        public string ProCodigo { get; set; }
        public string ProDescripcion { get; set; }
        public int LinID { get; set; }
        public string LinReferencia { get; set; }
        public int ProUnidades { get; set; }
        public string Lote { get; set; }

    }
}
