using MovilBusiness.Model;
using System;
using System.Collections.Generic;

namespace MovilBusiness.DataAccess
{
    public class DS_CanalDistribucion
    {
        public List<CanalDistribucion> GetAllCanales()
        {
            try
            {
                return SqliteManager.GetInstance().Query<CanalDistribucion>("select CanID, CanDescripcion, CanReferencia from CanalDistribucion order by CanDescripcion");
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return new List<CanalDistribucion>();
        }

        public int GetCanIDbyCanDescripcion(string CanDescripcion)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<CanalDistribucion>("select CanID from CanalDistribucion where CanDescripcion = '"+CanDescripcion+"'");
                return list[0].CanID;
            }
            catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return -1;
        }
    }
}
