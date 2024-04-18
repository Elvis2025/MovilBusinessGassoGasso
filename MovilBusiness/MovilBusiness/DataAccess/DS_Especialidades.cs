using MovilBusiness.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.DataAccess
{
    public class DS_Especialidades
    {

        public List<Especialidades> GetEspecialidades()
        {
            try
            {
                return SqliteManager.GetInstance().Query<Especialidades>("select EspID, EspReferencia, EspNombre from Especialidades order by EspNombre",
                    new string[] { });
            }catch(Exception)
            {
                return new List<Especialidades>();
            }
        }
    }
}
