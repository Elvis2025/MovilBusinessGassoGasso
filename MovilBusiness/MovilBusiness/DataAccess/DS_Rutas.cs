using MovilBusiness.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MovilBusiness.DataAccess
{
    public class DS_Rutas
    {
        public List<Rutas> GetRutaByTerritorioId(string terId)
        {
            try
            {
                
                if(App.Current.Properties.ContainsKey("CurrentRep"))
                {
                    int rutid = SqliteManager.GetInstance().Query<Rutas>($@"select RutID from Representantes where repcodigo = ?",
                        new string[] { App.Current.Properties["CurrentRep"]
                                      .ToString()}).FirstOrDefault().RutID;

                    return SqliteManager.GetInstance().Query<Rutas>($@"select RutID, RutDescripcion, TerID from Rutas where TerID = ?
                                      and RutID = ? order by RutDescripcion", new string[] { terId.ToString(), rutid.ToString() });

                }

                return SqliteManager.GetInstance().Query<Rutas>($@"select RutID, RutDescripcion, TerID from Rutas where TerID = ?
                                      order by RutDescripcion", new string[] { terId.ToString() });
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            return new List<Rutas>();
        }

        public List<Rutas> GetRutaByRepresentante(string repCodigo)
        {
            try
            {

                if (!string.IsNullOrEmpty(repCodigo))
                {
                    var rutid = SqliteManager.GetInstance().Query<Rutas>($@"select RutID from Representantes where repcodigo = ? ",
                        new string[] { repCodigo }).FirstOrDefault().RutID;

                    var list = SqliteManager.GetInstance().Query<Rutas>($@"select RutID, RutDescripcion, TerID, RutPeaje, RutDieta  from Rutas where RutID = ? order by RutDescripcion", new string[] {  rutid.ToString() });

                    if (list != null && list.Count > 0)
                    {
                        return list;
                    }

                    return null;
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            return new List<Rutas>();
        }
    }
}
