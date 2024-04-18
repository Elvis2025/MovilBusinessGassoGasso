using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Internal
{
    public class FormasPagoTemp
    {
        public int ForID { get; set; }
        public string FormaPago { get; set; }
        public string Banco { get; set; }
        public int BanID { get; set; }
        public Int64 NoCheque { get; set; }
        public string Futurista { get; set; }
        public string Fecha { get; set; }
        public double Valor { get; set; }
        public double Tasa { get; set; }
        public double Prima { get; set; }
        public string rowguid { get; set; }
        public int RefSecuencia { get; set; }
        public string MonCodigo { get; set; }
        public int AutSecuencia { get; set; }
        public string TipTarjeta { get; set; }
        public int BonoCantidad { get; set; }
        public int DenID { get; set; }
        public int PusCantidad { get; set; }

        public double BonoDenominacion { get; set; }

        [Ignore]public string FormattedDate { get => DateTime.Parse(Fecha).ToString("dd-MM-yyyy"); }
    }
}
