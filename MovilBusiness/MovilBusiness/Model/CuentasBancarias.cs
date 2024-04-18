using MovilBusiness.Abstraction;

namespace MovilBusiness.model
{
    public class CuentasBancarias : IKV
    {
        public int CuBID { get; set; }
        public string CuBNombre { get; set; }
        public string CubReferencia { get; set; }
        public string SecCodigo { get; set; }
        public int BanID { get; set; }
        public string cubCuentaContable { get; set; }
        public string rowguid { get; set; }

        public string GetKey()
        {
            return CuBID.ToString();
        }

        public string GetValue()
        {
            return CuBNombre;
        }

        public override string ToString()
        {
            return CuBNombre;
        }
    }
}
