using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model.Internal
{
    public class FormasPagoPermitidasCliente
    {
        public bool PermitePagoEfectivo { get; set; } = true;
        public bool PermiteCheque { get; set; } = true;
        public bool PermiteChequeRegular { get; set; } = true;
        public bool PermiteChequeDiferido { get; set; } = true;
        public bool PermiteNotaCredito { get; set; } = true;
        public bool PermiteTransferencia { get; set; } = true;
        public bool PermiteRetencion { get; set; } = true;
        public bool PermiteTarjetaCredito { get; set; } = true;
        public bool PermiteOrdenPago { get; set; } = true;
        public bool PermiteDiferenciaCambiaria { get; set; } = false;
        public bool PermiteRedondeo { get; set; } = false;
    }
}
