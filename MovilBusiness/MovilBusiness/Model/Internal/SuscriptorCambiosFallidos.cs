using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model.Internal
{
    public class SuscriptorCambiosFallidos
    {
        [PrimaryKey] [AutoIncrement] public int Posicion { get; set; }
        public string Query { get; set; }
        public string Fecha { get; set; }
    }
}
