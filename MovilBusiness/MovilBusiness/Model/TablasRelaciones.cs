using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class TablasRelaciones
    {
        public string TabNombre1 { get; set; }
        public string TabNombre2 { get; set; }
        public string TabPk1 { get; set; }
        public string TabPk2 { get; set; }
        public string UsuInicioSesion { get; set; }
        public string TabFechaActualizacion { get; set; }
        public Guid rowguid { get; set; }
    }
}
