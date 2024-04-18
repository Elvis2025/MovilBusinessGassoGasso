using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using SQLite;

namespace MovilBusiness.Model.Internal
{
    public class EntregasDetalleTemp
    {
        [PrimaryKey]public string rowguid { get; set; }
        public int TraSecuencia { get; set; }
        public int ProID { get; set; }
        public string ProDescripcion { get; set; }
        public string ProCodigo { get; set; }
        public bool UsaLote { get; set; }
        public double Cantidad { get; set; }
        public double CantidadDetalle { get; set; }
        public string Lote { get; set; }
        //public string Accion { get; set; }
        public string Documento { get; set; }
        public int MotIdDevolucion { get; set; }
        public string MotDescripcion { get; set; }
        public string FechaVencimiento { get; set; }
        public double CantidadSolicitada { get; set; }
        public double CantidadSolicitadaDetalle { get; set; }
        public int Posicion { get; set; }
        public bool IndicadorMalEstado { get; set; }
        public bool TraIndicadorOferta { get; set; }
        public int OfeID { get; set; }
        public string UnmCodigo { get; set; }

        public double ProUnidades { get; set; }
        public int InvCantidad { get; set; }
        public int InvCantidadDetalle { get; set; }

        public double CantidadDisponibleOriginal { get; set; }
        public double CantidadDisponibleDetalleOriginal { get; set; }

        [Ignore] public string ShowProIDoferta { get => DS_RepresentantesParametros.GetInstance().GetParProIDOferta() ? ":" + OfeID.ToString() : ""; }

        public double Precio { get; set; }
        public double Descuento { get; set; }
        public double Itbis { get; set; }
        [Ignore] public double PrecioNeto { get { return Precio > 0.0 ? ((Precio + AdValorem + Selectivo) - Descuento) * (Itbis / 100.0 + 1.0) : 0; } }
        [Ignore] public double PrecioBruto { get => Precio + AdValorem + Selectivo; }
        public double AdValorem { get; set; }
        public double Selectivo { get; set; }
        public bool IsAdded { get; set; }
        public int EnrSecuencia { get; set; }

        public EntregasDetalleTemp Copy()
        {
            return (EntregasDetalleTemp)MemberwiseClone();
        }

        [Ignore] public bool LoteIsSet { get => !string.IsNullOrWhiteSpace(Lote); }

        [Ignore] public string RowColor { get => Arguments.Values.CurrentModule == Modules.ENTREGASREPARTIDOR && Cantidad > 0 && ((UsaLote && !string.IsNullOrEmpty(Lote)) || !UsaLote) ? "#43A047" : "White"; }
    }
}
