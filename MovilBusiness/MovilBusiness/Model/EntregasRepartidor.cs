using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class EntregasRepartidor
    {
        public int EnrSecuencia { get; set; }
        public string RepCodigo { get; set; }
        public string EnrFecha { get; set; }
        public string EnrFechaInicial { get; set; }
        public string EnrFechaFinal { get; set; }
        public int EnrEstatus { get; set; }
        public string RepAyudante1 { get; set; }
        public string RepAyudante2 { get; set; }
        public double enrMontoTotal { get; set; }
        public int VehID { get; set; }
        public int AlmIDOrigen { get; set; }
        public int AlmID { get; set; }
        public string EnrNumeroERP { get; set; }

        public string Descripcion { get => EnrSecuencia + " - " + EnrFecha; }
 
    }
}
