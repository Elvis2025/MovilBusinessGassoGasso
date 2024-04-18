using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class FiltrosDinamicos
    {
        public string FilKey { get; set; }
        public string FilCampo { get; set; }
        public string FilCondicion { get; set; }
        public string FilDescripcion { get; set; }
        public int FilTipo { get; set; }
        public string FilComboSelect { get; set; }
        public int FilOrden { get; set; }
        public bool FilIndicadorDefault { get; set; }

        [Ignore]public bool IsCodigoBarra { get => !string.IsNullOrWhiteSpace(FilDescripcion) && FilTipo == 1 && FilDescripcion.Replace("ó", "o").Trim().ToLower().Contains("codigo") && FilDescripcion.Trim().ToLower().Contains("barra"); }
    }
}
