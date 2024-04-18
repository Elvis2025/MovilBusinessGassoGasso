using MovilBusiness.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.DataAccess
{
    public class DS_ProductosCombos
    {
        public List<ProductosCombos> GetProductosCombo(int proId)
        {
            return SqliteManager.GetInstance().Query<ProductosCombos>("select p2.ProCodigo||' - '||p2.ProDescripcion as ProDescripcion, PrcCantidad, c.ProID, c.ProIDCombo from ProductosCombos c " +
                "inner join Productos p on p.ProID = c.ProIDCombo " +
                "inner join Productos p2 on p2.ProID = c.ProID " +
                "where c.ProIDCombo = ? order by p.ProDescripcion", new string[] { proId.ToString() });
        }

        public double GetCombosDisponiblesxCantidad(int proId, int proIdCombo, double cantidadCombo = 1)
        {
            var list =  SqliteManager.GetInstance().Query<ProductosCombos>("select  ( "+ cantidadCombo.ToString() + " / PrcCantidad) as  CombosDisponibles from ProductosCombos c " +
                "where c.ProID = ? and c.ProIDCombo=? ", new string[] { proId.ToString(), proIdCombo.ToString() });

            if (list != null && list.Count > 0)
            {
                return list[0].CombosDisponibles;
            }

            return 0;
        }
    }
}
