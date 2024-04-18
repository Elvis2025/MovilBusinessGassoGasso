

namespace MovilBusiness.model
{
    public class Categoria2 
    {
        public int Cat2ID { get; set; }
        public string Cat2Referencia { get; set; }
        public string Cat2Descripcion { get; set; }
        public string rowguid { get; set; }

        public override string ToString()
        {
            return Cat2Descripcion;
        }
    }
}
