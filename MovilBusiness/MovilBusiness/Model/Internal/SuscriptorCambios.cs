using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model.Internal
{
    public class SuscriptorCambios
    {
        [PrimaryKey][AutoIncrement] public int Posicion { get; set; }
        public string RowguidTabla { get; set; }
        public string Script { get; set; }
        public string Tabla { get; set; }
        public string FechaActualizacion { get; set; }
        public string TipoScript { get; set; }
        public string rowguid { get; set; }
    }
}
