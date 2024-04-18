using MovilBusiness.Configuration;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.DataAccess
{
    public class DS_SolicitudExclusionCliente
    {
        public void GuardarSolicitud(int cliid, string motivo, string comentario)
        {
            int solSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("SolicitudExclusionClientes");

            var map = new Hash("SolicitudExclusionClientes");
            map.Add("SolSecuencia", solSecuencia);
            map.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            map.Add("CliID", cliid);
            map.Add("SolFecha", Functions.CurrentDate());
            map.Add("rowguid", Guid.NewGuid().ToString());
            map.Add("SolFechaActualizacion", Functions.CurrentDate());
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map.Add("SolEstado", 1);
            map.Add("SolMotivo", motivo);
            map.Add("SolComentario", comentario);


            map.ExecuteInsert();

            DS_RepresentantesSecuencias.UpdateSecuencia("SolicitudExclusionClientes", solSecuencia);
            new DS_Visitas().ActualizarVisitaEfectiva(Arguments.Values.CurrentVisSecuencia);
        }
    }
}
