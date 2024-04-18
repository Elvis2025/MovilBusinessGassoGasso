
using MovilBusiness.model.Internal.Structs.Args.services;
using MovilBusiness.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model.webservice
{
    public class ImagenesInsertarArgs
    {
        public List<TransaccionesImagenesTemp> Imagenes { get; set; }
        public UsuarioArgs User { get; set; }
    }
}
