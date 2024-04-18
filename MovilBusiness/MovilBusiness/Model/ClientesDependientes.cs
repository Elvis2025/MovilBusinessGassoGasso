using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class ClientesDependientes
    {
        public int Cliid { get; set; }
        public string ClDCedula { get; set; }
        public string ClDNombre { get; set; }
        public string CldTelefono { get; set; }
        public int FopID { get; set; }
        public int BanID { get; set; }
        public int CldTipoCuentaBancaria { get; set; }
        public string CldCuentaBancaria { get; set; }
        public string FopDescripcion { get; set; }
        public string BanNombre { get; set; }
        public string TipoCuentaDescripcion { get; set; }

        public override string ToString()
        {
            return ClDNombre;
        }

    }
}
