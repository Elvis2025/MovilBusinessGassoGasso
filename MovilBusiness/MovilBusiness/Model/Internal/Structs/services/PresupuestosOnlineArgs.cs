using MovilBusiness.model.Internal.Structs.Args.services;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model.Internal.Structs.services
{
    public class PresupuestosOnlineArgs
    {
        public UsuarioArgs User { get; set; }
        public int Cliid { get; set; }       
        public string PreTipo { get; set; }
        public string PreAnio { get; set; }
        public string PreMes { get; set; }
    }
}
