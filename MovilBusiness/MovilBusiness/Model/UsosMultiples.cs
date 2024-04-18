

namespace MovilBusiness.model
{
    public class UsosMultiples
    {
        public string CodigoGrupo { get; set; }
        public string CodigoUso { get; set; }
        public string Descripcion { get; set; }
        public string UsuInicioSesion { get; set; }
        public string UsuFechaActualizacion { get; set; }
        public string rowguid { get; set; }

        public int Orden { get; set; }

        public override string ToString()
        {
            return Descripcion==null?"<Null>": Descripcion;
        }
    }
}
