using MovilBusiness.Configuration;
using MovilBusiness.model;
using MovilBusiness.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovilBusiness.DataAccess
{
    public class DS_InventariosAlmacenesLotes : DS_Controller
    {
        public DS_InventariosAlmacenesLotes() { }

        public List<InventariosAlmacenesLotes> GetInventariosAlmacenesLotes(int Proid)
        {
            return SqliteManager.GetInstance().Query<InventariosAlmacenesLotes>("select AlmID, ProLote, InvCantidad, InvFechaVencimiento " +
                  "from InventariosAlmacenesLotes where ProID = ? order by ProLote" , new string[] { Proid.ToString() } );
        }

        public InventariosAlmacenesLotes GetInventarioAlmaceneseByProductos(int proId, int almid)
        {
            string almacen = "";

            if (almid > 0)
            {
                almacen = "and AlmID = " + almid.ToString() + "";
            }

            return SqliteManager.GetInstance().Query<InventariosAlmacenesLotes>("Select InvCantidad,AlmID ,InvCantidadDetalle, ProID, ProLote, " +
                "InvFechaVencimiento from InventariosAlmacenesLotes where ProID = ?" + almacen, new string[] { proId.ToString() }).FirstOrDefault();
        }

        public bool ExistenProductosConVencimientoMayorAlAnio()
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<InventariosAlmacenesLotes>("select ProID from " +
                    "InventariosAlmacenesLotes where InvFechaVencimiento is not null and cast(replace(cast(julianday(InvFechaVencimiento) - julianday(datetime('now')) as integer),' ', '') as integer) > 365 limit 1", new string[] { });

                return list != null && list.Count > 0;

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return false;
        }

        public bool ExistenProductosConVencimientoMenorAlAnio()
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<InventariosAlmacenesLotes>("select ProID from " +
                    "InventariosAlmacenesLotes where InvFechaVencimiento is not null and cast(replace(cast(julianday(InvFechaVencimiento) - julianday(datetime('now')) as integer),' ', '') as integer) < 365 limit 1", new string[] { });

                return list != null && list.Count > 0;

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return false;
        }

    }
}
