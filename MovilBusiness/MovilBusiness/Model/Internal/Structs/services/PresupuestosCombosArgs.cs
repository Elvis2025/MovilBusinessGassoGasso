using MovilBusiness.model.Internal.Structs.Args.services;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model.Internal.Structs.services
{
   public class PresupuestosCombosArgs
    {
        public UsuarioArgs User { get; set; }
        public int Tipo { get; set; }
        public int Campo { get; set; } 
        public string PreTipo { get; set; } = null;
        public string PreAnio { get; set; }
    }
}
