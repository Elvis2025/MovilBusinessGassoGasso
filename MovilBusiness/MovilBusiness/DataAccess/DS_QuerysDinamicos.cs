using MovilBusiness.Configuration;

using MovilBusiness.Model;
using MovilBusiness.Services;
using System;
using System.Collections.Generic;


namespace MovilBusiness.DataAccess
{
    public class DS_QuerysDinamicos
    {
        public string GetQuerysPresupuestos(string preTipo, string RepCodigo, bool IsOnline, ApiManager api)
        {
            try
            {
                var query = "select QueSelect from QuerysDinamicos where ltrim(rtrim(upper(QuePagina))) = 'PRESUPUESTOS' and ltrim(rtrim(upper(QueKey))) = '"+preTipo.Trim().ToUpper()+"'";

                List<QuerysDinamicos> list = null;

           /*     if (IsOnline && api != null)
                {
                    list = api.RawQuery<QuerysDinamicos>(Arguments.CurrentUser.RepCodigo, Arguments.CurrentUser.RepClave, query);
                }
                else*/
              //  {
                    list = SqliteManager.GetInstance().Query<QuerysDinamicos>(query, new string[] { });        
             //   }

                if (list != null && list.Count > 0)
                {
                    return list[0].QueSelect;
                }
                
            }
            catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return "";
        }
    }
}
