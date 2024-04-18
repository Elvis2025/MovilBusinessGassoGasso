using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model.Internal
{
    public class AuditoriasMercadosTemp
    {
        //public int ProID { get; set; }
        public string AudEmpaque { get; set; }
        public string AudEmpaqueDescripcion { get; set; }
        public string UnmCodigo { get; set; }
        public string UnmCodigoDescripcion { get; set; }
        public string Ca1Codigo { get; set; }
        public string Ca1Descripcion { get; set; }
        public string Ca2Codigo { get; set; }
        public string Ca2Descripcion { get; set; }
        public string MarCodigo { get; set; }
        public string MarDescripcion { get; set; }
        public double AudCapacidad { get; set; }
        public double AudUnidadVenta { get; set; }
        public double AudPrecioPublico { get; set; }
        public double AudPrecioCompra { get; set; }
        public int AudGondolaSuelo { get; set; }
        public int AudGondolaManos { get; set; }
        public int AudGondolaOjos { get; set; }
        public int AudGondolaTecho { get; set; }
        public int AudEspacioCabecera { get; set; }
        public int AudEspacioIsla { get; set; }
        public int AudEspacioCajas { get; set; }
        public int AudEspacioFrio { get; set; }
        public double AudPrecioOferta { get; set; }
        public double InvCantidad { get; set; }
        public string AudPresentacion { get; set; }
        public string AudPresentacionDescripcion { get; set; }
        public string AudVariedad { get; set; }
        //public string AudVariedadDescripcion { get; set; }

        [PrimaryKey] public string rowguid { get; set; }
    }
}
