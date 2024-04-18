
namespace MovilBusiness.Model
{
    public class NCDPP
    {
        public string RepCodigo { get; set; }
        public int NCDSecuencia { get; set; }
        public string NCDFecha { get; set; }
        public string RecTipo { get; set; }
        public int RecSecuencia { get; set; }
        public string CxcReferencia { get; set; }
        public string CxcDocumento { get; set; }
        public string CxCNCFAfectado { get; set; }
        public double NCDMonto { get; set; }
        public double NCDItbis { get; set; }
        public string NCDNCF { get; set; }
        public int NCDEstatus { get; set; }
        public int CliID { get; set; }

        public string CliNombre { get; set; }
        public string CliCodigo { get; set; }
    }
}
