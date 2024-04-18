using MovilBusiness.model.Internal.Structs.Args.services;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model.Internal.Structs.services
{
    public class SubirSqliteDbArgs
    {
        public UsuarioArgs User { get; set; }
        public string Base64DataBase { get; set; }
    }
}
