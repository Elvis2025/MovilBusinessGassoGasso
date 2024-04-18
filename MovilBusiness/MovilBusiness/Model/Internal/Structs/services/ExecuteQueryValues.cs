using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model.Internal.Structs.Args.services
{
    public class ExecuteQueryValues
    {
        public string Query { get; set; }
        public string TableName { get; set; }
        public string rowguid { get; set; }
        public string TipoScript { get; set; }
        public string rowguidtemp { get; set; }
        public string Posicion { get; set; }
    }
}
