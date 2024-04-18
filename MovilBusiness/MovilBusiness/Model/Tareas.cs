using MovilBusiness.Utils;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
   public class Tareas
    {
        public int TarSecuencia { get; set; }
        public string RepCodigo { get; set; }
        public int CliID { get; set; }
        public string TarFecha { get; set; }
        public string TarFechaLimite { get; set; }
        public string TarAsunto { get; set; }
        public string TarDescripcion { get; set; }
        public int TarEstado { get; set; }
        public int TitId { get; set; }
        public string TarEstadoDescripcion { get; set;}
        public int VisSecuencia { get; set; }

        [Ignore] public string FechaFormateada { get { return Functions.FormatDate(TarFechaLimite, "dd-MM-yyyy"); } }
        [Ignore] public string IndicadorCompleto { get { return  TarEstado==1? "light_yellow" : TarEstado == 2 ? "light_green" : ""; } }
    }
}
