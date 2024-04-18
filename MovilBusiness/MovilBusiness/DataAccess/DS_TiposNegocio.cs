
using MovilBusiness.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MovilBusiness.DataAccess
{
    public class DS_TiposNegocio
    {

        public TiposNegocio GetTipoById(int tinId)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<TiposNegocio>("select TinID, TinDescripcion from TiposNegocio where TinID = ?", new string[] { tinId.ToString() });

                if (list != null && list.Count > 0)
                {
                    return list[0];
                }

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return null;

        }
 
        public List<TiposNegocio> GetTipo()
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<TiposNegocio>("select TinID, TinDescripcion from TiposNegocio ", new string[] {}).ToList();

                if (list.Count > 0)
                {
                    return list;
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return new List<TiposNegocio>();

        }
    }
}
