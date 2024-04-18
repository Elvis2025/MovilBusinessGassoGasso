using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class InventarioFisicoDetalle
    {
        public string RepCodigo { get; set; }
        public int invSecuencia { get; set; }
        public int invPosicion { get; set; }
        public int ProID { get; set; }
        public int infTipoInventario { get; set; }
        public int? infCantidad { get; set; }
        public int? infCantidadDetalle { get; set; }
        public int? infCantidadLogica { get; set; }
        public int CliID { get; set; }
        public string infLote { get; set; }
        public DateTime infFechaVencimiento { get; set; }
        public string UsuInicioSesion { get; set; }
        public DateTime InvFechaActualizacion { get; set; }
        public string rowguid { get; set; }
        public int InvArea { get; set; }
        public int ProUnidades { get; set; }
        public decimal InvPrecioVenta { get; set; }
        public string ProDescripcion { get; set; }
        public string ProCodigo { get; set; }
        public double Itbis { get; set; }
        public string UnmCodigo { get; set; }
    }
}
