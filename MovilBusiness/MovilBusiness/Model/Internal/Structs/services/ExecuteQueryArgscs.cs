using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model.Internal.Structs.Args.services
{
    public class ExecuteQueryArgs
    {
        public UsuarioArgs User { get; set; }
        public List<ExecuteQueryValues> Values { get; set; }
    }
}
