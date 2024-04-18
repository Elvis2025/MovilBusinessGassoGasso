
using MovilBusiness.model;
using MovilBusiness.Model;
using System;
using System.Collections.Generic;


namespace MovilBusiness.DataAccess
{
    public class DS_Municipios
    {
        public List<Municipios> GetMunicipiosByProvincia(int ProId) { return GetMunicipiosByProvincia(-1, ProId); }
        private List<Municipios> GetMunicipiosByProvincia(int munId, int ProId)
        {
            string sql = "select MunID, ProID, MunDescripcion from Municipios where 1=1 ";

            if(munId != -1)
            {
                sql += " and MunID = " + munId.ToString();
            }

            if(ProId != -1)
            {
                sql += " and ProID = "+  ProId.ToString();
            }

            sql += " order by MunDescripcion";

            return SqliteManager.GetInstance().Query<Municipios>(sql, new string[] { });
        }

        public Municipios GetMunicipioById(int MunID)
        {
            try
            {
                List<Municipios> list = GetMunicipiosByProvincia(MunID, -1);

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

        public List<SectoresMunicipios> GetSectoresMunicipios(int munId)
        {
            try
            {
                return SqliteManager.GetInstance().Query<SectoresMunicipios>("select MunCodigo, SecCodigo, SecNombre " +
                    "from SectoresMunicipios where cast(MunCodigo as integer) = "+munId.ToString()+" order by SecNombre", new string[] { });
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }
            return new List<SectoresMunicipios>();
        }
    }
}
