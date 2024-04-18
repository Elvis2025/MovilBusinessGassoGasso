using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class PedidosDetalle
    {
        public string RepCodigo { get; set; }
        public int PedSecuencia { get; set; }
        public int PedPosicion { get; set; }
        public int ProID { get; set; }
        public string ProCodigo { get; set; }
        public int CliID { get; set; }
        public double PedCantidad { get; set; }
        public int PedCantidadDetalle { get; set; }
        public double PedPrecio { get; set; }
        public double PedItbis { get; set; }
        public double PedSelectivo { get; set; }
        public double PedAdValorem { get; set; }
        public double PedDescuento { get; set; }
        public string ProDescripcion { get; set; }
        public string Referencia { get; set; }
        public bool PedIndicadorCompleto { get; set; }
        public bool PedIndicadorOferta { get; set; }
        public string CedCodigo { get; set; }
        public int OfeID { get; set; }
        public double PedDesPorciento { get; set; }
        public int PedTipoOferta { get; set; }
        public string UnmCodigo { get; set; }
        public string PedFechaActualizacion { get; set; }
        public string UsuInicioSesion { get; set; }
        public string CliNombre { get; set; }
        public string rowguid { get; set; }
        public int PedEstatus { get; set; }
        public double PedCantidadInventario { get; set; }
        public int AutSecuencia { get; set; }
        public string RepSupervidor { get; set; }
        public double PedFlete { get; set; }
        public double ProUnidades { get; set; }
        public double PedidosTotal { get; set; }
        public double PedTotalItbis { get; set; }
        public double PedTotalDescuento { get; set; }

        public int PedCantidadConfirmada { get; set; }

        public string ProReferencia { get; set; }
    }
}
