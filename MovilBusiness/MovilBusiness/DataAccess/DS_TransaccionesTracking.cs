using MovilBusiness.Configuration;
using MovilBusiness.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.DataAccess
{
    public class DS_TransaccionesTracking
    {
        public List<TransaccionesTracking> GetTracking(int traId, string traKey)
        {
            string where = "";

            /*var first = true;

            foreach(var key in traKey)
            {
                if (first)
                {
                    first = false;
                    where = "[" + key + "]";
                }
                else
                {
                    where += ",[" + key + "]";
                }
            }*/

            where = traKey;

            return SqliteManager.GetInstance().Query<TransaccionesTracking>("select TraMensaje, TraID, TraEstado, TraFecha, TraKey " +
                "from TransaccionesTracking where TraID = ? and trim(RepCodigo) = ? and TraKey = ? " +
                "order by TraFecha", 
                new string[] { traId.ToString(), Arguments.CurrentUser.RepCodigo.Trim(), where});
        }
    }
}
