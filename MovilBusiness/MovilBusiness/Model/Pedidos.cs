using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class Pedidos
    {
        public string RepCodigo { get; set; }
        public int PedSecuencia { get; set; }
        public int CliID { get; set; }
        public string PedFecha { get; set; }
        public int PedTotal { get; set; }
        public string CliNombre { get; set; }
        public string CliCalle { get; set; }
        public string CliUrbanizacion { get; set; }
        public string CliCodigo { get; set; }
        public string MonCodigo { get; set; }
        public string CliPropietario { get; set; }
        public string CliRnc { get; set; }
        public string CliTelefono { get; set; }
        public string ConDescripcion { get; set; }
        public int PedCantidadImpresion { get; set; }
        public string rowguid { get; set; }
        public int CliIndicadorPresentacion { get; set; }
        public int VisSecuencia { get; set; }
        public int PedTipoPedido { get; set; }
        public string PedFechaEntrega { get; set; }
        public int PedEstatus { get; set; }
        public int VenSecuencia { get; set; }
        public double PedMontoSinITBIS { get; set; }
        public double PedMontoITBIS { get; set; }
        public double PedMontoTotal { get; set; }
        public double PedSubTotal { get; set; }
        public double PedPorCientoDsctoGlobal { get; set; }
        public double PedMontoDsctoGlobal { get; set; }
        public string CldDirTipo { get; set; }
        public string SecCodigo { get; set; }
        public int ConID { get; set; }
        public string PedFechaActualizacion { get; set; }
        public string mbVersion { get; set; }
        public string PedOtrosDatos { get; set; }
        public int PedIndicadorRevision { get; set; }

        /*public int PedIndicadorCompleto { get; set; }
        public string PedFechaEntrega { get; set; }
        public int PedIndicadorRevision { get; set; }
        public int PedTipoPedido { get; set; }
        public string PedOrdenCompra { get; set; }
        public int VisSecuencia { get; set; }
        public string MonCodigo { get; set; }
        public string SecCodigo { get; set; }
        public string OrvCodigo { get; set; }
        public string OfvCodigo { get; set; }
        public bool PedIndicadorContado { get; set; }
        public int VenSecuencia { get; set; }
        public string RepVendedor { get; set; }
        public int ConID { get; set; }
        public int CuaSecuencia { get; set; }
        public int Motid { get; set; }
        public string CldDirTipo { get; set; }
        public string UsuInicioSesion { get; set; }
        public string PedFechaActualizacion { get; set; }
        public bool PedIndicadorPushMoney { get; set; }
        public int PedPrioridad { get; set; }
        public string rowguid { get; set; }
        public string PedFechaSincronizacion { get; set; }
        public string LipCodigo { get; set; }
        public string mbVersion { get; set; }
        public int AlmID { get; set; }
        public string RepSupervisor { get; set; }
        public int CliIDMaster { get; set; }*/
    }
}
