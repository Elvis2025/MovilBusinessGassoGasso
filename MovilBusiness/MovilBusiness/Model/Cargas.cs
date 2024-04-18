using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class Cargas
    {
        public string RepCodigo { get; set; }
        public int CarSecuencia { get; set; }
        public string CarFecha { get; set; }
        public string CarReferencia { get; set; }
        public int CarCantidadTotal { get; set; }
        public int CarEstatus { get; set; }
        public int CuaID { get; set; }
        public int AlmID { get; set; }
        public string CarFechaAplicacion { get; set; }
        public string EstadoDescripcion { get; set; }
        public string rowguid { get; set; }
        public string CarReferenciaEntrega { get; set; }

        public string AlmDescripcion { get; set; }
        public Cargas Copy()
        {
            return (Cargas)MemberwiseClone();
        }

        public string CarDescripcion
        {
            get{

                if(DateTime.TryParse(CarFecha, out DateTime fecha))
                {
                    return CarReferencia + " - Fecha: " + fecha.ToString("dd/MM/yyyy");
                }

                return CarReferencia;
            }
        }
    }
}
