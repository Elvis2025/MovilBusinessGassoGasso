
using MovilBusiness.DataAccess;
using SQLite;
using System;

namespace MovilBusiness.Model
{
    public class EntregasRepartidorTransacciones
    {
        public int EnrSecuencia { get; set; }
        public int EnrSecuenciaDetalle { get; set; }
        public int TitID { get; set; }
        public string RepCodigo { get; set; }
        public string RepVendedor { get; set; }
        public int TraSecuencia { get; set; }
        public int CliID { get; set; }
        public string VenFecha { get; set; }
        public string VenNCF { get; set; }
        public string venNumeroERP { get; set; }
        public string venNumeroERPDocum { get; set; }
        public int enrEstatusEntrega { get; set; }
        public double VenDPP { get; set; }
        public string VenDPPNota { get; set; }
        public string VenDPPNumeroERP { get; set; }
        public string VenDPPNumeroERPDocum { get; set; }
        public double VenMonto { get; set; }
        public string EntCuentaContable { get; set; }
        public string NCNumeroERP { get; set; }
        public string NCNumeroERPDocum { get; set; }
        public int NCMotID { get; set; }
        public string NCNota { get; set; }
        public int NCEstatus { get; set; }
        public int NCAccion { get; set; }
        public int ConID { get; set; }
        public string rowguid { get; set; }
        public string SecDescripcion { get; set; }
        public double EntMontoTotal { get; set; }

        public string RepNombre { get; set; }
        public string RepTelefono { get; set; }

        public bool Confirmada { get; set; }

        public string estatusDescripcion { get; set; }

        public string CliNombre { get; set; }
        public string CliCodigo { get; set; }
        public int EntSecuencia { get; set; }
        public string CliCalle { get; set; }
        public string CliUrbanizacion { get; set; }
        public string CliTipoComprobanteFAC { get; set; }
        public string CliRNC { get; set; }
        public string SecCodigo { get; set; }

        [Ignore]public string FormattedDate
        {
            get
            {
                if(DateTime.TryParse(VenFecha, out DateTime result))
                {
                    return result.ToString("dd/MM/yyyy");
                }
                return VenFecha;
            }
        }

        public string RechazarBtn { get; set; } = "Rechazar entrega";
        [Ignore]public bool ShowTelefono { get => !string.IsNullOrWhiteSpace(RepTelefono); }

        public bool ShowVerDetalleBtn { get; set; } = true;

        [Ignore] public string Prevendedor { get { return (RepVendedor + '-' + RepNombre).ToString(); } }
        [Ignore] public string CondicionPago { get { return DS_CondicionesPago.GetInstance().GetByConId(ConID).ConDescripcion; } }

        [Ignore] public string Cliente { get { return  DS_Clientes.GetInstance().GetClienteById(CliID).CliCodigo + '-' + DS_Clientes.GetInstance().GetClienteById(CliID).CliNombre; } }

        [Ignore]
        public string FormattedDateEntrega
        {
            get
            {
                if (DateTime.TryParse(DS_EntregasRepartidor.GetInstance().GetFechaEntregaRepartidor(EnrSecuencia, false).EnrFecha, out DateTime result))
                {
                    return result.ToString("dd/MM/yyyy");
                }
                return DS_EntregasRepartidor.GetInstance().GetFechaEntregaRepartidor(EnrSecuencia, false).EnrFecha; ;
            }
        }

    }
}
