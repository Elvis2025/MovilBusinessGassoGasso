using System;

namespace MovilBusiness.model.Internal.Structs.Args
{
    public struct AgregarFormaPagoArgs
    {
        public string Banco { get; set; }
        public string BanID { get; set; }
        public DateTime Fecha { get; set; }
        public bool Futurista { get; set; }
        public double Valor { get; set; }
        public string NoCheque { get; set; }
        public int AutSecuencia { get; set; }

        public Monedas Moneda { get; set; }
        public double Prima { get; set; }

        public int BonoCantidad { get; set; }
        public int DenId { get; set; }
        public int PusCantidad { get; set; }
        public double BonoDenominacion { get; set; }
        public string TipTarjeta { get; set; }
    }
}
