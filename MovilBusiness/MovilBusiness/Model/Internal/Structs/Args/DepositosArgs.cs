using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model.Internal.Structs.Args
{
    public class DepositosArgs
    {
        public double Numero { get; set; }
        public string Referencia { get; set; }
        public int Tipo { get; set; }
        public int CuBID { get; set; }
        public string SocCodigo { get; set; }
        public Location location { get; set; }
        public List<Recibos> RecibosADepositar { get; set; }
        public double MontoChk { get; set; }
        public double MontoChkFut { get; set; }
        public double MontoEfectivo { get; set; }
        public double MontoTarjeta { get; set; }
        public double MontoTransferencia { get; set; }
        public double MontoRetencion { get; set; }
        public double MontoOrdenPago { get; set; }
        public string MonCodigo { get; set; }
    }
}
