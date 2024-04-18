using MovilBusiness.DataAccess;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class OfertasDetalle
    {
        public int OfeID { get; set; }
        public int OfeSecuencia { get; set; }
        public int ProID { get; set; }
        public double OfeCantidad { get; set; }
        public double OfeCantidadOferta { get; set; }
        public double OfePrecio { get; set; }
        public double OfeCantidadDetalle { get; set; }
        public double OfeCantidadDetalleOferta { get; set; }
        public double OfePorciento { get; set; }

        public string ProDescripcion1 { get; set; }
        public string ProDescripcion { get; set; }
        public string ProGrupoProductos { get; set; }
        public int OfeCantidadMax { get; set; }
        public double PrecioLista { get; set; }
        public int TotalMancomunado { get; set; }
        public string UnmCodigo { get; set; }
        public string UnmCodigoCabecera { get; set; }
        public string ProDatos3 { get; set; }

        //TODO : Agregar parametro para que use redondeo de 2 unidades
        [Ignore] public double PorcientoCantidad { get { return (DS_RepresentantesParametros.GetInstance().GetParRedondeoCantidadesDecimales() ? (OfePorciento / 100) : OfeCantidad * (OfePorciento / 100)); } }
        [Ignore] public string ValorOferta { get { return (DS_RepresentantesParametros.GetInstance().GetParRedondeoCantidadesDecimales() ? OfePorciento > 0 ? OfePorciento.ToString("N2") + "%(" + PorcientoCantidad.ToString("N4") + ")" : OfePrecio.ToString("N2") : OfePorciento > 0 ? (int)OfePorciento + "%(" + (int)PorcientoCantidad + ")" : OfePrecio.ToString("N2")); } }
        //[Ignore] public string ValorOferta { get => OfePorciento > 0 ? OfePorciento.ToString("N2") + "%(" + PorcientoCantidad.ToString("N4") + ")" : OfePrecio.ToString("N2"); }
        //[Ignore] public double PorcientoCantidad { get => OfeCantidad * (OfePorciento / 100); }
        //[Ignore] public string ValorOferta { get => OfePorciento > 0 ? (int)OfePorciento + "%("+(int) PorcientoCantidad+")" : OfePrecio.ToString("N2"); }
        [Ignore] public bool UsaLote { get => !string.IsNullOrWhiteSpace(ProDatos3) && ProDatos3.ToUpper().Contains("L"); }
    }
}
