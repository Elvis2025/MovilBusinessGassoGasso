using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class PushMoneyPagos
    {
        public string RepCodigo { get; set; }
        public int pusSecuencia { get; set; }
        public int CliID { get; set; }
        public string pusFecha { get; set; }
        public string pusNumero { get; set; }
        public int pusEstatus { get; set; }
        public double pusMontoNcr { get; set; }
        public double pusMontoEfectivo { get; set; }
        public double pusMontoBono { get; set; }
        public double pusMontoTotal { get; set; }
        public int CuaSecuencia { get; set; }
        public int VisSecuencia { get; set; }
        public int DepSecuencia { get; set; }
        public double pusRetencion { get; set; }
        public string pusDivision { get; set; }
        public string MonCodigo { get; set; }
        public string SecCodigo { get; set; }
        public string OrvCodigo { get; set; }
        public string odvCodigo { get; set; }
        public int pusCantidadDetalleAplicacion { get; set; }
        public int pusCantidadDetalleFormaPago { get; set; }
        public int pusCantidadImpresion { get; set; }
        public string UsuInicioSesion { get; set; }
        public string pusFechaActualizacion { get; set; }
        public string rowguid { get; set; }
        public string pusFechaSincronizacion { get; set; }
        public string mbVersion { get; set; }
        public string RepSupervisor { get; set; }
        public double RecTasa { get; set; }

        public string CliCodigo { get; set; }
        public string CliNombre { get; set; }
        public string CliContacto { get; set; }
        public string CliCalle { get; set; }
        public string CliUrbanizacion { get; set; }
        public string CliRNC { get; set; }
        public string CliTelefono { get; set; }
        public string ClDCedula { get; set; }
        public string ClDNombre { get; set; }

        public bool confirmado { get; set; }
    }
}
