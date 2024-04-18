
using MovilBusiness.model;
using System;
using System.Collections.Generic;


namespace MovilBusiness.DataAccess
{
    public class DS_Provincias
    {

        public List<Provincias> GetProvincias(int ProID = -1, string terCodigo = null)
        {
            string sql = "select ProID, PaiID, ProNombre from Provincias where 1=1 ";

            if(ProID != -1)
            {
                sql += " and ProID = ?";
            }

            if (!string.IsNullOrWhiteSpace(terCodigo))
            {
                sql += " and trim(upper(TerCodigo)) = trim(upper('"+terCodigo+"')) ";
            }

            sql += " order by ProNombre";

            return SqliteManager.GetInstance().Query<Provincias>(sql, new string[] { ProID.ToString()});
            
        }

        public Provincias GetProvinciaById(int ProId)
        {
            try
            {
                List<Provincias> list = GetProvincias(ProId);

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
    }
}
