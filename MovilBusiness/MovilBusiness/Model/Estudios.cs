using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class Estudios
    {
        public int EstID { get; set; }
        public string EstNombre { get; set; }
        public string EstFechaDesde { get; set; }
        public string EstFechaHasta { get; set; }
        public int EstCantidadPreguntas { get; set; }
        public int EstEstatus { get; set; }
        public string EstTipoMuestra { get; set; }
    }
}
