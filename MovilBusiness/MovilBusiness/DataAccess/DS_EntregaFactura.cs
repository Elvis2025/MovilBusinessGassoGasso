using MovilBusiness.Configuration;
using MovilBusiness.model;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;

namespace MovilBusiness.DataAccess
{
    public class DS_EntregaFactura
    {
        public List<EntregasDocumentosDetalle> GetDocumentosPorEntregar(int Cliid)
        {
            return SqliteManager.GetInstance().Query<EntregasDocumentosDetalle>("select replace(strftime('%d-%m-%Y', SUBSTR(CxcFecha,1,10)),' ','' ) as formattedFecha, " +
                "replace(cast(julianday('now','" + Functions.GetDiferenciaHorariaSqlite() + " hours') - julianday(cxcfecha) as integer),' ', '') as Dias, 1 as Estatus, " +
                "CxcDocumento as EntDocumento, CxcSIGLA as cxcSigla, CxcBalance as EntMonto, CxcFecha as cxcFecha from CuentasxCobrar CC where CliID = ? and CxcDocumento NOT IN (" +
                "Select EntDocumento From EntregasDocumentos E " +
                "inner join EntregasDocumentosDetalle ED on E.EntSecuencia = ED.EntSecuencia and E.RepCodigo = ED.RepCodigo where EntEstatus <> 0) " +
                "order by Date(CxcFecha) ASC ", new string[] { Cliid.ToString() });
        }

        public EntregasDocumentos GetEntregaBySecuencia(int entSecuencia, bool entConfirmado)
        {
            List<EntregasDocumentos> list = SqliteManager.GetInstance().Query<EntregasDocumentos>("select EntRecibidoPor, CliID, EntEstatus, " +
                "strftime('%d-%m-%Y %H:%M:%S', EntFecha) as EntFecha, EntSecuencia from " + (entConfirmado ? "EntregasDocumentosConfirmados" : "EntregasDocumentos") + " where EntSecuencia = ? " +
                "and RepCodigo = ?", new string[] { entSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public List<EntregasDocumentosDetalle> GetEntregasDetalleBySecuencia(int entSecuencia, bool entConfirmado)
        {
            return SqliteManager.GetInstance().Query<EntregasDocumentosDetalle>("select E.cxcSigla as cxcSigla, strftime('%d-%m-%Y', E.cxcFecha) as cxcFecha, ifnull(Cxc.AreaCtrlCredit,'') as AreaCtrlCredit, " +
                "E.EntDocumento as EntDocumento, E.EntMonto as EntMonto, strftime('%d-%m-%Y', E.cxcFecha) as formattedFecha  from " + (entConfirmado ? "EntregasDocumentosDetalleConfirmados" : "EntregasDocumentosDetalle") + " E left join CuentasxCobrar Cxc on E.EntDocumento = Cxc.CxcReferencia where EntSecuencia = ? and E.RepCodigo = ?", new string[] { entSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });
        }

        public int GuardarEntrega(List<EntregasDocumentosDetalle> Documentos, string RecibidoPor)
        {
            int entSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("EntregasDocumentos");

            Hash ent = new Hash("EntregasDocumentos");
            ent.Add("EntEstatus", 1);
            ent.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            ent.Add("CliID", Arguments.Values.CurrentClient.CliID);            
            ent.Add("EntFecha", Functions.CurrentDate());
            ent.Add("EntRecibidoPor", RecibidoPor);
            ent.Add("EntSecuencia", entSecuencia);
            ent.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            ent.Add("EntFechaActualizacion", Functions.CurrentDate());
            ent.Add("rowguid", Guid.NewGuid().ToString());
            ent.Add("mbVersion", Functions.AppVersion);
            ent.ExecuteInsert();

            int pos = 1;
            foreach (EntregasDocumentosDetalle documento in Documentos)
            {
                var fecha = documento.cxcFecha != null ? documento.cxcFecha.Replace("T", " ") : null;

                Hash map = new Hash("EntregasDocumentosDetalle");
                map.Add("cxcFecha", fecha);
                map.Add("cxcSigla", documento.cxcSigla);
                map.Add("DocID", 1);
                map.Add("EntCantidad", 1);
                map.Add("EntDocumento", documento.EntDocumento);
                map.Add("EntMonto", documento.EntMonto);
                map.Add("EntPosicion", pos); pos++;
                map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                map.Add("EntSecuencia", entSecuencia);
                map.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                map.Add("rowguid", Guid.NewGuid().ToString());
                map.ExecuteInsert();
            }

            if (DS_RepresentantesParametros.GetInstance().GetParVisitasResultados())
            {
                ActualizarVisitasResultados();
            }

            DS_RepresentantesSecuencias.UpdateSecuencia("EntregasDocumentos", entSecuencia);

            return entSecuencia;
        }

        private void ActualizarVisitasResultados()
        {
            var list = SqliteManager.GetInstance().Query<VisitasResultados>("select 10 as TitID, count(*) as VisCantidadTransacciones, " +
                "sum(EntMonto) as VisMontoTotal, sum(EntMonto) as VisMontoSinItbis, '' as VisComentario " +
                "from EntregasDocumentosDetalle where RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim()+"' and VisSecuencia = ?", 
                new string[] { Arguments.Values.CurrentVisSecuencia.ToString() });

            if(list != null && list.Count > 0)
            {
                new DS_Visitas().GuardarVisitasResultados(list[0]);
            }
        }

        public void EstEntregaDocumento(string rowguid,int est)
        {
            Hash ped = new Hash("EntregasDocumentos");
            ped.Add("EntEstatus", est);
            ped.Add("EntFechaActualizacion", Functions.CurrentDate());
            ped.Add("UsuInicioSesion", /*Arguments.CurrentUser.RepCodigo*/"mdsoft");
            //ped.ExecuteUpdate("EntSecuencia = " + EntSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");

            if (est == 0)
            {
                if (new DS_SuscriptoresCambios().UpdateCambioEstadoInsertByRowguid(rowguid, est))
                {
                    ped.SaveScriptForServer = false;
                }
            }

            ped.ExecuteUpdate("rowguid = '" + rowguid + "'");
        }
    }
}
