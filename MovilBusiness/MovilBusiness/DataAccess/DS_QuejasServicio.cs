using MovilBusiness.Configuration;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.DataAccess
{
    public class DS_QuejasServicio
    {
        public void GuardarQueja(TiposMensaje Motivo, string repCodigovendedor, string comentario)
        {
            var queSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("QuejasServicio");

            var map = new Hash("QuejasServicio");
            map.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            map.Add("QueSecuencia", queSecuencia);
            map.Add("CliID", Arguments.Values.CurrentClient.CliID);
            map.Add("QueFecha", Functions.CurrentDate());
            map.Add("QueIDMotivo", Motivo.MenID);
            map.Add("QueMotDescripcion", Motivo.MenDescripcion);
            map.Add("QueComentario", comentario);
            map.Add("VisSecuencia", Arguments.Values.CurrentVisSecuencia);
            if (Arguments.Values.CurrentSector != null)
            {
                map.Add("SecCodigo", Arguments.Values.CurrentSector.SecCodigo);
            }
            map.Add("QueEstatus", 1);
            map.Add("RepCodigoVendedor", repCodigovendedor);
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map.Add("RecFechaActualizacion", Functions.CurrentDate());
            map.Add("rowguid", Guid.NewGuid().ToString());
            map.Add("mbVersion", Functions.AppVersion);
            map.ExecuteInsert();

            DS_RepresentantesSecuencias.UpdateSecuencia("QuejasServicio", queSecuencia);

            if (DS_RepresentantesParametros.GetInstance().GetParVisitasResultados())
            {
                ActualizarVisitasResultados(comentario);
            }
            
            new DS_Visitas().ActualizarVisitaEfectiva(Arguments.Values.CurrentVisSecuencia);
            
        }

        private void ActualizarVisitasResultados(string comentario)
        {
            var list = SqliteManager.GetInstance().Query<VisitasResultados>("select 54 as TitID, count(*) as VisCantidadTransacciones " +
                "from QuejasServicio where RepCodigo = '"+Arguments.CurrentUser.RepCodigo.Trim()+"' and VisSecuencia = ?", 
                new string[] { Arguments.Values.CurrentVisSecuencia.ToString() });

            if(list != null && list.Count > 0)
            {
                var item = list[0];
                item.VisComentario = comentario;

                new DS_Visitas().GuardarVisitasResultados(item);
            }
        }

        public void EditarQueja(int queSecuencia, TiposMensaje motivo, string repCodigoVendedor, string comentario)
        {
            var map = new Hash("QuejasServicio");
            map.Add("QueIDMotivo", motivo.MenID);
            map.Add("QueMotDescripcion", motivo.MenDescripcion);
            map.Add("QueComentario", comentario);
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map.Add("RecFechaActualizacion", Functions.CurrentDate());
            map.Add("mbVersion", Functions.AppVersion);
            map.ExecuteUpdate("QueSecuencia = " + queSecuencia.ToString() + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' ");
        }

        public void AnularQueja(int queSecuencia)
        {
            var map = new Hash("QuejasServicio");
            map.Add("QueEstatus", 0);
            map.Add("RecFechaActualizacion", Functions.CurrentDate());
            map.ExecuteUpdate("QueSecuencia = " + queSecuencia.ToString() + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");
        }

        public QuejasServicio GetQuejaBySecuencia(int queSecuencia)
        {
            var list = SqliteManager.GetInstance().Query<QuejasServicio>("select * from QuejasServicio where QueSecuencia = ? and trim(RepCodigo) = ? ", 
                new string[] { queSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            if(list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }
    }
}
