using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class Autorizaciones
    {
        public int AutSecuencia { get; set; }
        public string RepCodigo { get; set; }
        public string AutFecha { get; set; }
        public int AutEstatus { get; set; }
        public string AutPin { get; set; }
        public int TitID { get; set; }
        public int TraSecuencia { get; set; }
        public string AutFechaAplicacion { get; set; }
        public string AutComentario { get; set; }
        public string rowguid { get; set; }
        public string RepSupervisor { get; set; }
        public string AutReferencia { get; set; }

        public override string ToString()
        {
            if (!String.IsNullOrEmpty(AutReferencia) && !String.IsNullOrWhiteSpace(AutReferencia))
            {
                var Split = AutReferencia.Split('-');
                return "Autorización #" + AutSecuencia + " - " + Split[1];
            }
            else
            {
                return "Autorización #" + AutSecuencia;
            }
       
        }
    }
}
