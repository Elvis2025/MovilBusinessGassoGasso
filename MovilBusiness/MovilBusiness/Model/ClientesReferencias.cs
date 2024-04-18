using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class ClientesReferencias
    {
        public string RepCodigo { get; set; }
        public int CliID { get; set; }
        public int CliRefSecuencia{get;set;}
        public string CliRefTipo { get; set; }
        public string CliRefNombre { get; set; }
        public string CliRefTelefono { get; set; }
        public string rowguid { get; set; }
        public string RefTipoDesc { get; set; }
        public string CliRefCuenta { get; set; }

        public int? BanID { get; set; }
    }
}
