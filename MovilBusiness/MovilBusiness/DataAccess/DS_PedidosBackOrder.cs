using MovilBusiness.Configuration;
using MovilBusiness.model.Internal;
using System.Collections.Generic;

namespace MovilBusiness.DataAccess
{
    public class DS_PedidosBackOrder : DS_Controller
    {
        public List<ProductosTemp> GetBackOrderByCliente(int cliId)
        {
            var query = "select p.ProCodigo as ProCodigo, p.ProDescripcion as Descripcion, b.PedCantidad as Cantidad, " +
                "b.PedCantidadDetalle as CantidadDetalle, b.UnmCodigo as UnmCodigo, 0 as Precio, p.ProID as ProID, p.ProItbis as Itbis, " +
                "p.ProSelectivo as Selectivo, p.ProAdValorem as AdValorem from PedidosBackOrder b " +
                "inner join Productos p on p.ProID = b.ProID " +
                "inner join InventariosAlmacenes i on i.ProID = p.ProID and i.AlmID = " + myParametro.GetParAlmacenDefault() + " and (i.InvCantidad > 0 or i.InvCantidadDetalle > 0) " +
                (myParametro.GetParNoListaPrecios() ? "left" : "inner")+" join ListaPrecios l on l.ProID = p.ProID and l.LipCodigo = '" +Arguments.Values.CurrentClient.LiPCodigo+"' " +
                "where b.RepCodigo = ? and b.CliID = ? and ifnull(upper(p.ProDatos3), '') not like '%P%' " +
                "group by p.ProID order by p.ProDescripcion";

            return SqliteManager.GetInstance().Query<ProductosTemp>(query, new string[] { Arguments.CurrentUser.RepCodigo, cliId.ToString() });
        }
    }
}
