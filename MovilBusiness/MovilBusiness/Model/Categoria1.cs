
namespace MovilBusiness.model
{
    public class Categoria1
    {
        public int Cat1ID { get; set; }
        public string Cat1Referencia { get; set; }
        public string Cat1Descripcion { get; set; }
        public string rowguid { get; set; }

        public override string ToString()
        {
            return Cat1Descripcion;
        }
    }
}
