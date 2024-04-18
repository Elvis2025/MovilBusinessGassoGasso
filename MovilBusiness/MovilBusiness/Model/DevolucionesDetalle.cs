using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class DevolucionesDetalle
    {
        public string RepCodigo { get; set; }
        public string DevTipo { get; set; }
        public int DevSecuencia { get; set; }
        public int DevPosicion { get; set; }
        public int ProID { get; set; }
        public string ProDescripcion { get; set; }
        public string ProCodigo { get; set; }
        public double DevCantidad { get; set; }
        public double DevCantidadDetalle { get; set; }
        public string DevDocumento { get; set; }
        public int CliID { get; set; }
        public double DevPrecio { get; set; }
        public double DevDescuento { get; set; }
        public double DevItebis { get; set; }
        public double DevTotalItebis { get; set; }
        public double DevTotalDescuento { get; set; }
        public double DevSelectivo { get; set; }
        public double DevAdvalorem { get; set; }
        public int DevAccion { get; set; }
        public int DevEstatus { get; set; }
        public int MotID { get; set; }
        public string DevLote { get; set; }
        public bool DevIndicadorOferta { get; set; }
        public string DevFecha { get; set; }
        public string DevFechaFormatted { get; set; }
        public string rowguid { get; set; }
        public string DevGrupo { get; set; }
        public string DevComentario { get; set; }
        public string MotDescripcion { get; set; }

        public string UnmCodigo { get; set; }

        public double ProUnidades { get; set; }
        public string MotReferencia { get; set; }
        public string Name { get; set; }

        public int EnrSecuencia { get; set; }
        public decimal DevDescPorciento { get; set; }
        public string DevNumeroERP { get; set; }

        [Ignore]public double Itbis
        {
            get {
                return (DevPrecio - DevDescuento) * (DevItebis / 100.0);
            }
        }

        [Ignore] public string CantidadConUnidades { get => DevCantidad + (DevCantidadDetalle > 0 ? "/" + DevCantidadDetalle : ""); }
        [Ignore] public double PrecioNeto { get { return DevPrecio > 0.0 ? ((DevPrecio + DevAdvalorem + DevSelectivo) - DevDescuento) * (DevItebis / 100.0 + 1.0) : 0; } }
        [Ignore] public double PrecioBruto { get { return DevPrecio > 0.0 ? DevPrecio + DevAdvalorem + DevSelectivo : 0; } }
    }
}
