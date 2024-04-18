

using MovilBusiness.Abstraction;

namespace MovilBusiness.model
{
    public class Bancos : IKV
    {
        public int BanID { get; set; }
        public string BanNombre { get; set; }
        public string BanReferencia { get; set; }
        public string UsuInicioSesion { get; set; }
        public string BanFechaActualizacion { get; set; }
        public string rowguid { get; set; }
        public string BanTipo { get; set; }

        public string GetKey()
        {
            return BanID.ToString();
        }

        public string GetValue()
        {
            return BanNombre;
        }

        public override string ToString()
        {
            return BanNombre;
        }
    }
}
