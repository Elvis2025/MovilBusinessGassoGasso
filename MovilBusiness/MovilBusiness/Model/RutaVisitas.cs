using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class RutaVisitas
    {
        public string RepCodigo { get; set; }
        public int CliID { get; set; }
        public string RutSemana1 { get; set; }
        public string RutSemana2 { get; set; }
        public string RutSemana3 { get; set; }
        public string RutSemana4 { get; set; }
        public int RutPosicion { get; set; }
    }
}
