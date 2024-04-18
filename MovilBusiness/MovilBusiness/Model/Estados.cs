using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class Estados
    {
        public string EstTabla { get; set; }
        public string EstEstado { get; set; }
        public string EstDescripcion { get; set; }
        public string estOpciones { get; set; }
        public string rowguid { get; set; }
        public string EstSiguientesEstados { get; set; }
        public int Count { get; set; } //la cantidad de transacciones que hay de este estado

        public bool UseClient { get; set; } = true;

        public override string ToString()
        {
            return EstDescripcion;
        }
    }
}
