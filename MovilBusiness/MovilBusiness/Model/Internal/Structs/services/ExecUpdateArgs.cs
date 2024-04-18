using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model.Internal.Structs.Args.services
{
    public class ExecUpdateArgs
    {
        public UsuarioArgs User { get; set; }
        public string Query { get; set; }
    }
}
