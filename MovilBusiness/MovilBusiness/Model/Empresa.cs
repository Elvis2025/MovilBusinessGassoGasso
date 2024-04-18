using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class Empresa
    {
        public int EmpID { get; set; }
        public string EmpNombre { get; set; }
        public string EmpDireccion { get; set; }
        public string EmpDireccion2 { get; set; }
        public string EmpDireccion3 { get; set; }
        public string EmpRNC { get; set; }
        public string EmpTelefono { get; set; }
        public string EmpFax { get; set; }
        public byte[] EmpLogo { get; set; }
        //public string FechaUltimaActualizacion { get; set; }
        public string OrvCodigo { get; set; }
        public string OfvCodigo { get; set; }
        public string SocCodigo { get; set; }

        //private string _rowguid = Guid.NewGuid().ToString();
        public string rowguid { get; set; }
        //public string empFechaActualizacion { get; set; }
    }
}
