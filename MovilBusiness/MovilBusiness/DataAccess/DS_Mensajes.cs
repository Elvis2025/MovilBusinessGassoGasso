using MovilBusiness.Configuration;
using MovilBusiness.model;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MovilBusiness.DataAccess
{
    public class DS_Mensajes
    {
        private int GetMaxID()
        {
            List<int> list = SqliteManager.GetInstance().Query<int>("select ifnull(MAX(MenID),0) from Mensajes", new string[] { });

            if (list.Count > 0)
            {
                return list[0];
            }

            return 0;
        }

        public void CrearMensaje(int cliId, string descripcion, int visSecuencia, int traSecuencia, int traId, int depId = 0)
        {
            Hash map = new Hash("Mensajes");
            map.Add("CliID", cliId);
            map.Add("MenID", GetMaxID()+1);
            map.Add("MenDescripcion", descripcion.Replace("'", ""));
            map.Add("VisSecuencia", visSecuencia);
            map.Add("TraSecuencia", traSecuencia);
            map.Add("TraID", traId);
            map.Add("DepID", depId);
            map.Add("rowguid", Guid.NewGuid().ToString());
            map.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);

            if(Arguments.Values.CurrentSector != null)
            {
                map.Add("SecCodigo", Arguments.Values.CurrentSector.SecCodigo);
            }
            map.Add("MenFecha", Functions.CurrentDate());
            map.Add("MenFechaActualizacion", Functions.CurrentDate());
            map.ExecuteInsert();

           /*if (Arguments.Values.CurrentModule == Enums.Modules.VISITAS)
            {
                new DS_Visitas().GuardarVisitasResultados(Arguments.Values.CurrentVisSecuencia, Enums.Modules.VISITAS, descripcion.Replace("'", ""), 0, 0);
            }/*
            else
            {
                new DS_Visitas().ActualizarVisitasResultadosComentario(descripcion.Replace("'", ""), Arguments.Values.CurrentVisSecuencia);
            }*/
        }

        /*private void ActualizarVisitasResultados(string comentario)
        {
            var item = new VisitasResultados();
            item.TitID = 13;
            item.VisComentario = comentario;

        }*/

        public Mensajes GetByTraSecuencia(int traSecuencia, int traId, int cliId)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<Mensajes>("select MenDescripcion, MenID, CliID from Mensajes where TraSecuencia = ? and TraID = ? and CliID = ? order by cast(MenID as int) desc", new string[] { traSecuencia.ToString(), traId.ToString(), cliId.ToString() });

                if(list != null && list.Count > 0)
                {
                    return list[0];
                }

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return null;
        }

        public bool Exists(int traId, int traSecuencia, int cliId, int visSecuencia, string secCodigo = null)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(secCodigo))
            {
                where += " and ifnull(SecCodigo, '') = '" + secCodigo + "' ";
            }
            return SqliteManager.GetInstance().Query<Mensajes>("select MenID from Mensajes where TraID = ? and TraSecuencia = ? and CliID = ? and VisSecuencia = ? " + where + " Limit 1", new string[] { traId.ToString(), traSecuencia.ToString(), cliId.ToString(), visSecuencia.ToString() }).Count > 0;
        }

        public string GetMensajeForSincronizacion(int traid)
        {
            return SqliteManager.GetInstance().Query<TiposMensaje>
                ($"select MenDescripcion from TiposMensaje where TraID = {traid}").FirstOrDefault().MenDescripcion;
        }

    }

}
