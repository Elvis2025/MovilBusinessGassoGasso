using Microsoft.AppCenter.Crashes;
using MovilBusiness.Model;
using System;
using System.Collections.Generic;

namespace MovilBusiness.DataAccess
{
    public class DS_Almacenes
    {
        public List<Almacenes> GetAlmacenes()
        {
            try
            {
                return SqliteManager.GetInstance().Query<Almacenes>("select AlmID, AlmDescripcion, AlmReferencia, ifnull(AlmCaracteristicas, '') as AlmCaracteristicas from Almacenes order by AlmDescripcion");
            }catch(Exception)
            {
                return SqliteManager.GetInstance().Query<Almacenes>("select AlmID, AlmDescripcion, AlmReferencia, '' as AlmCaracteristicas from Almacenes order by AlmDescripcion");
            }
        }

        public Almacenes GetAlmacenById(int almId)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<Almacenes>("select AlmID, AlmDescripcion, ifnull(AlmCaracteristicas, '') as AlmCaracteristicas " +
                    "from Almacenes where AlmID = ?", 
                    new string[] { almId.ToString() });
                
                if(list != null && list.Count > 0)
                {
                    return list[0];
                }
            }catch(Exception e)
            {
                Crashes.TrackError(e);
                var b = SqliteManager.GetInstance().Query<Almacenes>("select AlmID, AlmDescripcion, '' as AlmCaracteristicas " +
                    "from Almacenes where AlmID = ?",
                    new string[] { almId.ToString() });

                if(b != null && b.Count > 0)
                {
                    return b[0];
                }
            }

            return null;
        }

        public Almacenes GetAlmacenByReferencia(string AlmReferencia)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<Almacenes>("select AlmID, AlmDescripcion, ifnull(AlmCaracteristicas, '') as AlmCaracteristicas " +
                    "from Almacenes where AlmReferencia = ?",
                    new string[] { AlmReferencia.ToString() });

                if (list != null && list.Count > 0)
                {
                    return list[0];
                }
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                var b = SqliteManager.GetInstance().Query<Almacenes>("select AlmID, AlmDescripcion, '' as AlmCaracteristicas " +
                    "from Almacenes where AlmReferencia = ?",
                    new string[] { AlmReferencia.ToString() });

                if (b != null && b.Count > 0)
                {
                    return b[0];
                }
            }

            return null;
        }

        public string GetDescripcionAlmacen(int Almid)
        {
            var list = SqliteManager.GetInstance().Query<Almacenes>("select AlmDescripcion  from Almacenes where AlmID = ?",
                    new string[] { Almid.ToString() });

            if (list != null && list.Count > 0)
            {
                return list[0].AlmDescripcion;
            }
            return "";
        }

        public List<Almacenes> GetAlmacenesByAlmIDParameter(string Almid)
        {
            try
            {
                return SqliteManager.GetInstance().Query<Almacenes>("select AlmID, AlmDescripcion, AlmReferencia  from Almacenes where AlmID in ( " + Almid + ") ");
            }
            catch (Exception e)
            {
                e.GetBaseException();
            }
            return null;
        }
    }
}
