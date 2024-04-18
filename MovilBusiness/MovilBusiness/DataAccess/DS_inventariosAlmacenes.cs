using MovilBusiness.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovilBusiness.DataAccess
{
      public class DS_inventariosAlmacenes
      {

            public List<InventariosAlmacenes> GetInventarioDisponibleByProductos(int proId)
            {
                return SqliteManager.GetInstance().Query<InventariosAlmacenes>("Select AlmDescripcion, InvCantidad, InvCantidadDetalle " +
                    "from Almacenes A Inner Join InventariosAlmacenes  IA ON IA.AlmID = A.AlmID where ProID = ? Order by invCantidad asc" , new string[] { proId.ToString() });
            }

            public string GetInventarioDisponibleByProductosIdForAlm(int proId)
            {
                List<InventariosAlmacenes> list = SqliteManager.GetInstance().Query<InventariosAlmacenes>("Select AlmDescripcion, InvCantidad " +
                    "from Almacenes A Inner Join InventariosAlmacenes  IA ON IA.AlmID = A.AlmID where ProID = ? Order by invCantidad asc", new string[] { proId.ToString() });

                string almacenes = "";

                foreach(var item in list)
                {
                    almacenes += item.AlmDescripcion.ToString() + ": [" + item.InvCantidad + "] ";
                }

                return almacenes;
            }

      }
}
