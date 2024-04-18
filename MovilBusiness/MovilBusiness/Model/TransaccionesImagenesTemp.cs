using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class TransaccionesImagenesTemp
    {
        [JsonIgnore] [PrimaryKey] public string Rowguid { get; set; }
        public string RepCodigo { get; set; }
        public string RepTabla { get; set; }
        public string RepTablaKey { get; set; }
       // [JsonIgnore] public int TraSecuencia { get; set; }
        public int TraPosicion { get; set; }
        public byte[] TraImagen { get; set; }
        public string TraFormato { get; set; }
        public string TraTamano { get; set; }
        public int TitId { get; set; }
        public bool ForFirma { get; set; }
        [JsonIgnore] public int Ready { get; set; }
    }
}
