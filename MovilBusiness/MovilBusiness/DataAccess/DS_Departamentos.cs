using MovilBusiness.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.DataAccess
{
    public class DS_Departamentos
    {
        public List<Departamentos> GetDepartamentos()
        {
            return SqliteManager.GetInstance().Query<Departamentos>("select DepID, DepDescripcion from Departamentos order by DepDescripcion", new string[] { });
        }
    }
}
