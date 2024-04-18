using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class CuadresDetalle
    {
        public string RepCodigo { get; set; }
        public int CuaSecuencia { get; set; }
        public int CuaTipoInventario { get; set; }
        public int ProID { get; set; }
        public double CuaCantidadInicial { get; set; }
        public int CuaCantidadDetalleInicial { get; set; }
        public double CuaCantidadFinal { get; set; }
        public int CuaCantidadDetalleFinal { get; set; }
        public double CuaCantidadFisica { get; set; }
        public int CuaCantidadDetalleFisica { get; set; }
        public string ProDescripcion { get; set; }
        public string ProCodigo { get; set; }
        public string UnmCodigo { get; set; }
        public string InvLote { get; set; }
        public string ProDatos3 { get; set; }

        public double ProUnidades { get; set; }
    }
}
