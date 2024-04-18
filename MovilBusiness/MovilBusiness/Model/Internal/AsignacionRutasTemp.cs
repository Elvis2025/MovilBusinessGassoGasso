using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model.Internal
{
    public class AsignacionRutasTemp
    {
        public string RutFecha { get; set; }
        public int CliID { get; set; }

        public bool DeleteReal { get; set; }
        public string rowguidReal { get; set; } //si existe en rutavisitasfecha este campo viene lleno

        public int Posicion { get; set; }

        public bool ExisteEnReal { get; set; }
    }
}
