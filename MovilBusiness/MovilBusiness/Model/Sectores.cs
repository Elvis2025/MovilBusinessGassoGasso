
namespace MovilBusiness.model
{
    public class Sectores
    {
        public string SecCodigo { get; set; }
        public string SecDescripcion { get; set; }
        public string SecReferencia { get; set; }

        public int ConID { get; set; }
        public string MonCodigo { get; set; }
        public string AreaCtrlCredit { get; set; }
        public string LipCodigo { get; set; }
        public bool CliIndicadorExonerado { get; set; }
        public int estatus { get; set; }

        public override string ToString()
        {
            return SecDescripcion;
        }
    }
}
