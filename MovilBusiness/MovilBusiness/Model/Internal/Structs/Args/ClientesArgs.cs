using MovilBusiness.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model.Internal.Structs.Args
{
    public struct ClientesArgs
    {
        public string RepCodigo { get; set; }
        public string SearchValue { get; set; }
        public int NumeroSemana { get; set; }
        public int DiaNumero { get; set; }
        public FiltroEstatusVisitaClientes Estatus { get; set; }
        public FiltrosDinamicos filter { get; set; }
        public string secondFilter { get; set; }
        public DateTime RutFecha { get; set; }
        public int Cliid { get; set; }

        public int ProID { get; set; }
    }
}
