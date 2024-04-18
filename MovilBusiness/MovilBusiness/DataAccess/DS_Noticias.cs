using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.model;
using Plugin.LocalNotifications;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace MovilBusiness.DataAccess
{
    public class DS_Noticias
    {

        public List<Noticias> GetNoticias()
        {
            return SqliteManager.GetInstance().Query<Noticias>("select NotFecha, notCorta, NotDescripcion, NotIndicadorLeido, NotID, rowguid " +
                "from Noticias where RepCodigo = ? order by NotIndicadorLeido asc, NotFecha desc", 
                new string[] { Arguments.CurrentUser.RepCodigo });
        }

        public int GetCantidadNoticiasSinLeer()
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<Noticias>("select NotDescripcion, NotID " +
                    "from Noticias where RepCodigo = ? and NotIndicadorLeido = 0 ",
                    new string[] { Arguments.CurrentUser.RepCodigo });
                
                if (list != null && list.Count > 0)
                {
                    return list.Count;
                }
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return 0;
        }

        public void MarcarNoticiaLeida(/*int notId*/ string rowguid)
        {
            Hash map = new Hash("Noticias");
            map.Add("NotIndicadorLeido", 1);
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);

            ///obj.updateObject("ltrim(rtrim(RepCodigo)) = '"+ SystemState.RepCodigo.trim()+"' and NotID = "+noticias.getNotID());
            //map.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and NotID = " + notId);
            map.ExecuteUpdate(" rowguid = '" + rowguid + "' " );
        }
    }
}
