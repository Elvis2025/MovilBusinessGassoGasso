using MovilBusiness.Model;
using System.Collections.Generic;

namespace MovilBusiness.DataAccess
{
    public class DS_FormasPago
    {
        public List<FormasPago> GetFormasPago()
        {
            return SqliteManager.GetInstance().Query<FormasPago>("select FopID, ifnull(FopDescripcion,'') as FopDescripcion, FopReferencia from FormasPago order by FopDescripcion", new string[] { });
        }

        public int GetFormasPagoCantidadPermitida(int formaPago)
        {
            var list = SqliteManager.GetInstance().Query<FormasPago>("select * from FormasPago where FopID = ? ", new string[] { formaPago.ToString() });
            
            if(list != null && list.Count > 0)
            {
                return list[0].FopCantidadPermitida == 0  ? int.MaxValue : list[0].FopCantidadPermitida;
            }

            return int.MaxValue;
        }
    }
}
