using MovilBusiness.Model.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.DataAccess
{
    public class DS_Denominaciones
    {
        public DS_Denominaciones(){}

        public List<Denominaciones> GetDenominacionesByDenTipo(int dentipo)
        {
            var list = SqliteManager.GetInstance().Query<Denominaciones>("select DenDescripcion, DenValor, DenID from Denominaciones where DenTipo =? ", new string[] { dentipo.ToString()});

           if(list != null && list.Count > 0)
           {
                return list;
           }
           return new List<Denominaciones>();
        }

        public string GetDenominacionesByDenId(int denid)
        {
            var list = SqliteManager.GetInstance().Query<Denominaciones>("select DenDescripcion from Denominaciones where DenId =? ", new string[] { denid.ToString()});

           if(list != null && list.Count > 0)
           {
                return list[0].DenDescripcion;
           }
           return "";
        }

    }
}
