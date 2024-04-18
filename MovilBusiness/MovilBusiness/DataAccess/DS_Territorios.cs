using MovilBusiness.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.DataAccess
{
    public class DS_Territorios
    {
        public List<Territorios> GetTerritorios()
        {
            try
            {
                return SqliteManager.GetInstance().Query<Territorios>("select TerCodigo, TerDescripcion from Territorios order by TerDescripcion", new string[] { });
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }
            return null;
        }
    }
}
