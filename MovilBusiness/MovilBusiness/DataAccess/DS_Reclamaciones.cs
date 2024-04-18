using MovilBusiness.Configuration;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.DataAccess
{
    public class DS_Reclamaciones
    {
        public void SaveReclamacion(string motivo, int cliId, string descripcion, int cantidad, int unidades, string lote, int proId)
        {
            int recSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Reclamaciones");

            var map = new Hash("Reclamaciones");
            map.Add("Repcodigo", Arguments.CurrentUser.RepCodigo);
            map.Add("RecSecuencia", recSecuencia);
            map.Add("CliID", cliId);
            map.Add("RecFecha", Functions.CurrentDate());
            map.Add("ProID", proId);
            map.Add("ProLote", lote);
            map.Add("RecCantidad", cantidad);
            map.Add("RecCantidadDetalle", unidades);
            map.Add("RecMotivo", motivo);
            map.Add("RecDescripcion", descripcion);
            map.Add("VisSecuencia", Arguments.Values.CurrentVisSecuencia);
            map.Add("RecEstatus", 1);
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map.Add("RecFechaActualizacion", Functions.CurrentDate());
            map.Add("rowguid", Guid.NewGuid().ToString());
            map.Add("mbVersion", Functions.AppVersion);
            map.ExecuteInsert();

            DS_RepresentantesSecuencias.UpdateSecuencia("Reclamaciones", recSecuencia);

            //new DS_Visitas().GuardarVisitasResultados(Arguments.Values.CurrentVisSecuencia, Enums.Modules.RECLAMACIONES, descripcion, 0, 0);
            if (DS_RepresentantesParametros.GetInstance().GetParVisitasResultados())
            {
                ActualizarVisitasResultados(descripcion);
            }

        }

        private void ActualizarVisitasResultados(string comentario)
        {
            var list = SqliteManager.GetInstance().Query<VisitasResultados>("select 23 as TitID, count(*) as VisCantidadTransacciones, '' as VisComentarios " +
                "from Reclamaciones where RepCodigo = '"+Arguments.CurrentUser.RepCodigo.Trim()+"' and VisSecuencia = ?", new string[] { Arguments.Values.CurrentVisSecuencia.ToString() });

            if(list != null && list.Count > 0)
            {
                var item = list[0];
                item.VisComentario = comentario;
                new DS_Visitas().GuardarVisitasResultados(item);
            }
        }
    }
}
