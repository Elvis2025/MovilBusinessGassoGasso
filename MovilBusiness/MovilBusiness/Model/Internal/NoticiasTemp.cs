using SQLite;

namespace MovilBusiness.Model.Internal
{
    public class NoticiasTemp
    {
        [PrimaryKey]
        public int NotID { get; set; }
        public string RepCodigo { get; set; }
        public int TitID { get; set; }
        public int Traid { get; set; }
        public string NotFecha { get; set; }
        public string notCorta { get; set; }
        public string NotDescripcion { get; set; }
        public bool NotIndicadorLeido { get; set; }
        public string UsuInicioSesion { get; set; }
        public string NotFechaActualizacion { get; set; }
        public string rowguid { get; set; }
    }
}
