using MovilBusiness.Utils;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class Noticias
    {
        public int NotID { get; set; }
        public string RepCodigo { get; set; }
        public int TitID { get; set; }
        public int Traid { get; set; }
        public string NotFecha { get; set; }
        public string notCorta { get; set; }
        public string NotDescripcion { get; set; }
        public bool NotIndicadorLeido { get; set; }
        public string rowguid { get; set; }

        [Ignore]public string NotIndicadorLeidoImg { get { return NotIndicadorLeido ? "light_green" : "light_gray"; } }
        [Ignore] public string NotTitulo { get { return NotID + " - " + notCorta; } }
        [Ignore] public string NotFechaFormateada { get { return Functions.FormatDate(NotFecha, "dd-MM-yyyy"); } }
    }
}
