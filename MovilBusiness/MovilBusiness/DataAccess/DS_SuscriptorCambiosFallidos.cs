using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.DataAccess
{
    public class DS_SuscriptorCambiosFallidos
    {

        public DS_SuscriptorCambiosFallidos()
        {

        }

        public void SaveSuscriptorCambiosFallidos(string query)
        {
            var sus = new Hash("SuscriptorCambiosRecibidos");
            sus.SaveScriptForServer = false;
            sus.Add("Query", query);
            sus.Add("Fecha", Functions.CurrentDate());
            sus.ExecuteInsert();
        }

        public void Delete()
        {
            var delete = new Hash("SuscriptorCambiosRecibidos");
            delete.SaveScriptForServer = false;
            delete.ExecuteDelete("julianday('now') - julianday(Fecha) > 30");
        }

    }
}
