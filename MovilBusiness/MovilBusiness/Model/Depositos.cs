using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class Depositos
    {
        public string RepCodigo { get; set; }
        public int DepSecuencia { get; set; }
        public int DepNumero { get; set; }
        public int DepTipo { get; set; }
        public double DepMontoEfectivo { get; set; }
        public double DepMontoCheque { get; set; }
        public double DepMontoTarjeta { get; set; }
        public double DepMontoTransferencia { get; set; }
        public double DepMontoChequeDiferido { get; set; }
        public double DepMontoOrdenPago { get; set; }
        public double DepMontoPushMoney { get; set; }
        public int DepReciboInicial { get; set; }
        public int DepReciboFinal { get; set; }
        public string DepFecha { get; set; }
        public int DepEstatus { get; set; }
        public int CuBID { get; set; }
        public int DepCantidadRecibos { get; set; }
        public int DepCantTarjetas { get; set; }
        public double DepMontoDepositado { get; set; }
        public string rowguid { get; set; }
        public string DepComentario { get; set; }
        public string DepReferencia { get; set; }
        public string CuBNombre { get; set; }
        public string DepTipoDescripcion { get; set; }
        public string MonCodigo { get; set; }
        public string SocCodigo { get; set; }
        public string CodigoGrupo { get; set; }
        public string CodigoUso { get; set; }
        public string usoDescripcion { get; set; }
    }
}
