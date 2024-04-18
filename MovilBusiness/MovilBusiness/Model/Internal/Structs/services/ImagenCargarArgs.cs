using MovilBusiness.model.Internal.Structs.Args.services;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model.Internal.Structs.services
{
    public class ImagenCargarArgs
    {
        public string ImagePath { get; set; }
        public UsuarioArgs User { get; set; }
    }
}
