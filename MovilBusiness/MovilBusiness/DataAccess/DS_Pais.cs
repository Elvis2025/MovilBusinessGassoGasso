
using MovilBusiness.model;
using System;
using System.Collections.Generic;


namespace MovilBusiness.DataAccess
{
    public class DS_Pais
    {
        public Pais GetById(int PaiId)
        {
            try
            {
                List<Pais> list = SqliteManager.GetInstance().Query<Pais>("select PaiID, PaiNombre, PaiReferencia " +
                    "from Pais where PaiID = ?", new string[] { PaiId.ToString() });

                if (list != null && list.Count > 0)
                {
                    return list[0];
                }

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return new Pais();
        }
    }
}
