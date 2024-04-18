using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model.Internal.Structs.Args.services
{
    public class ReplicacionesSuscriptoresCambiosLeerArgs
    {
        public UsuarioArgs User { get; set; }
        public int Limit { get; set; }
        public string DeleteQuery { get; set; }
        public bool IsSincronizar { get; set; }
    }
}
