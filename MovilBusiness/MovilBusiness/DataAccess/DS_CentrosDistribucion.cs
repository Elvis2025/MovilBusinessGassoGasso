using MovilBusiness.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MovilBusiness.DataAccess
{
    public class DS_CentrosDistribucion
    {

        public List<CentrosDistribucion> GetCentrosDistribucions(string sector)
        {
            try
            {
                string where = "";
                if (!string.IsNullOrWhiteSpace(sector) && DS_RepresentantesParametros.GetInstance().GetParPedidosCentrosDistribucionFiltrarPorSector())
                {
                    where = $" and SecCodigo = '{sector}'";
                }

                string query = "select  CedCodigo, CedDescripcion from CentrosDistribucion where 1 = 1 " + where; 
                return SqliteManager.GetInstance().Query<CentrosDistribucion>(query);

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }
            return new List<CentrosDistribucion>();
        }

        public CentrosDistribucion GetCentrosDistribucion(string repcodigo)
        {

            var list = SqliteManager.GetInstance().Query<CentrosDistribucion>($"select  c.CedCodigo, CenDireccion as CedDescripcion, CenDireccion2 as CedReferencia from CentrosDistribucion c INNER JOIN Representantes r ON c.CedCodigo = r.CedCodigo where r.Repcodigo = '{repcodigo}'");

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }
    }
}
