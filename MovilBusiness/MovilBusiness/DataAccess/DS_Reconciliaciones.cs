using MovilBusiness.Configuration;
using MovilBusiness.model;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovilBusiness.DataAccess
{
    public class DS_Reconciliaciones : DS_Controller
    {

        public int GuardarReconciliacion(Monedas moneda)
        {
            var recSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Reconciliaciones");

            var myRec = new DS_Recibos();

            var map = new Hash("Reconciliaciones");
            map.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            map.Add("RecSecuencia", recSecuencia);
            map.Add("CliID", Arguments.Values.CurrentClient.CliID);
            map.Add("RecFecha", Functions.CurrentDate());
            map.Add("RecNumero", "");
            map.Add("RecEstatus", 1);
            map.Add("RecMontoNcr", Math.Abs(myRec.GetMontoNCTotalAplicado()));
            map.Add("RecTipo", "1");
            map.Add("CuaSecuencia", Arguments.Values.CurrentCuaSecuencia);
            map.Add("VisSecuencia", Arguments.Values.CurrentVisSecuencia);
            map.Add("DepSecuencia", 0);
            map.Add("AreactrlCredit", Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.AreaCtrlCredit : null);
            map.Add("MonCodigo", moneda != null ? moneda.MonCodigo : Arguments.Values.CurrentClient.MonCodigo);
            map.Add("SecCodigo", (Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.SecCodigo : ""));
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map.Add("OrvCodigo", "");
            map.Add("OfvCodigo", "");
            map.Add("RecCantidadImpresion", 0);
            map.Add("RecTotal", Math.Abs(myRec.GetMontoNCTotalAplicado()));
            map.Add("RecFechaActualizacion", Functions.CurrentDate());
            map.Add("rowguid", Guid.NewGuid().ToString());
            map.Add("mbVersion", Functions.AppVersion);

            var ncs = myRec.GetNotasCreditoAplicadas();

            map.Add("RecCantidadDetalleAplicacion", ncs.Select(x=>x.FacturaReferencia).Distinct().Count());
            map.Add("RecCantidadDetalleFormaPago", ncs.Count);

            map.ExecuteInsert();            

            var pos = 1;
            var montoTotal = 0.0;
            foreach(var nc in ncs)
            {
                var det = new Hash("ReconciliacionesDetalle");
                det.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                det.Add("RecSecuencia", recSecuencia);
                det.Add("SocCodigo", Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.AreaCtrlCredit : null);
                det.Add("RecTipo", "1");
                det.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                det.Add("RefSecuencia", pos); pos++;
                det.Add("CxcReferencia", nc.NCReferencia);
                det.Add("cxcDocumento", nc.NCDocumento);
                det.Add("CxcReferenciaAplica", nc.FacturaReferencia);
                det.Add("CxcDocumentoAplica", nc.FacturaDocumento);
                det.Add("RefTasa", moneda != null ? moneda.MonTasa : 0);
                det.Add("RefValor", nc.ValorAplicado);
                det.Add("RefPrima", 0);
                det.Add("MonCodigo", moneda != null ? moneda.MonCodigo : null);
                det.Add("rowguid", Guid.NewGuid().ToString());
                det.Add("RecFechaActualizacion", Functions.CurrentDate());

                montoTotal += nc.ValorAplicado;
                det.ExecuteInsert();
            }

            DS_RepresentantesSecuencias.UpdateSecuencia("Reconciliaciones", recSecuencia);

            if (DS_RepresentantesParametros.GetInstance().GetParVisitasResultados())
            {
                ActualizarVisitasResultados();
            }

            myRec.ClearTemps();

            return recSecuencia;

        }

        private void ActualizarVisitasResultados()
        {
            var list = SqliteManager.GetInstance().Query<VisitasResultados>("select '' as VisComentario, 28 as TitID, count(*) as VisCantidadTransacciones, sum(d.RefValor) as VisMontoTotal, " +
                "sum(d.RefValor) as VisMontoSinItbis form Reconciliaciones r " +
                "inner join ReconciliacionesDetalle d on d.RepCodigo = r.RepCodigo and d.RecSecuencia = r.RecSecuencia and r.RecTipo = d.RecTipo" +
                "where r.VisSecuencia = ? and r.RepCodigo = '"+Arguments.CurrentUser.RepCodigo.Trim()+"' ", new string[] { Arguments.Values.CurrentVisSecuencia.ToString() });

            if(list != null && list.Count > 0)
            {
                new DS_Visitas().GuardarVisitasResultados(list[0]);
            }
        }

        public Reconciliaciones GetReconciliacionBySecuencia(int recSecuencia, string recTipo, bool confirmado)
        {
            var list = SqliteManager.GetInstance().Query<Reconciliaciones>("select r.RepCodigo as RepCodigo, r.RecSecuencia, r.RecTipo, r.RecEstatus, " +
                "r.RecCantidadImpresion, r.RecFecha, ifnull(c.CliSector, '') as CliSector, c.CliCodigo as CliCodigo, c.CliNombre as CliNombre " +
                "from " + (confirmado ? "ReconciliacionesConfirmados" : "Reconciliaciones") + " r " +
                "inner join Clientes c on c.CliID = r.CliID " +
                "where RecSecuencia = ? and r.RepCodigo = ? and r.RecTipo = ?", 
                new string[] { recSecuencia.ToString(), Arguments.CurrentUser.RepCodigo, recTipo });

            if(list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public List<ReconciliacionesDetalle> GetReconciliacionDetalleBySecuencia(int recSecuencia, string recTipo, bool confirmado)
        {
            return SqliteManager.GetInstance().Query<ReconciliacionesDetalle>("select * from " + (confirmado?"ReconciliacionesDetalleConfirmados":"ReconciliacionesDetalle") + " r " +
                "where RecSecuencia = ? and RecTipo = ? and trim(RepCodigo) = ?", new string[] { recSecuencia.ToString(), recTipo, Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public void ActualizarCantidadImpresion(int recSecuencia, bool confirmado, string recTipo, int cantidadImpresion)
        {
            var map = new Hash(confirmado ? "ReconciliacionesConfirmados" : "Reconciliaciones");
            map.Add("RecCantidadImpresion", cantidadImpresion);
            map.Add("RecFechaActualizacion", Functions.CurrentDate());
            map.ExecuteUpdate("RecSecuencia = " + recSecuencia + " and ltrim(rtrim(RecTipo)) = '" + recTipo.Trim() + "' and ltrim(rtrim(RepCodigo)) = '"+Arguments.CurrentUser.RepCodigo.Trim()+"'");
        }
    }
}
