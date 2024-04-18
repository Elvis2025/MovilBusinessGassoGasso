using MovilBusiness.Configuration;
using MovilBusiness.Enums;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovilBusiness.DataAccess
{
    public class DS_TransferenciasAlmacenes
    {
        public int GuardarTransferencia(bool IsEntregandoTraspaso, string RepCodigoAsignado)
        {
            var traSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("TransferenciasAlmacenes");

            var map = new Hash("TransferenciasAlmacenes");
            map.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            map.Add("TraID", traSecuencia);
            map.Add("CuaSecuencia", Arguments.Values.CurrentCuaSecuencia);
            map.Add("TraTipo", IsEntregandoTraspaso ? -1 : 1);
            map.Add("AlmCodigoOrigen", IsEntregandoTraspaso ? Arguments.CurrentUser.RepCodigo : RepCodigoAsignado);
            map.Add("AlmCodigoDestino", IsEntregandoTraspaso ? RepCodigoAsignado : Arguments.CurrentUser.RepCodigo);
            map.Add("TraEstado", 1);
            map.Add("TraFecha", Functions.CurrentDate());
            map.Add("TraFechaActualizacion", Functions.CurrentDate());
            map.Add("TraReferencia", "");
            map.Add("rowguid", Guid.NewGuid().ToString());
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);

            map.ExecuteInsert();

            var myProd = new DS_Productos();

            var productos = myProd.GetResumenProductos((int)Arguments.Values.CurrentModule);
            var myInv = new DS_Inventarios();

            int pos = 1;
            foreach(var producto in productos)
            {
                var det = new Hash("TransferenciasAlmacenesDetalle");
                det.Add("TraID", traSecuencia);
                det.Add("ProCodigo", producto.ProCodigo);
                det.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                det.Add("TadCantidad", producto.Cantidad);
                det.Add("TadCantidadDetalle", producto.CantidadDetalle);
                det.Add("TadPosicion", pos); pos++;
                det.Add("TraFechaActualizacion", Functions.CurrentDate());
                det.Add("rowguid", Guid.NewGuid().ToString());
                det.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                det.ExecuteInsert();

                if (IsEntregandoTraspaso)
                {
                    myInv.RestarInventario(producto.ProID, producto.Cantidad, producto.CantidadDetalle);
                }
                else
                {
                    myInv.AgregarInventario(producto.ProID, producto.Cantidad, producto.CantidadDetalle);
                }
            }

            DS_RepresentantesSecuencias.UpdateSecuencia("TransferenciasAlmacenes", traSecuencia);

            myProd.ClearTemp((int)Modules.TRASPASOS);

            return traSecuencia;
        }

        public TransferenciasAlmacenes GetBySecuencia(int traId)
        {
            return SqliteManager.GetInstance().Query<TransferenciasAlmacenes>("select t.*, r.RepNombre as RepNombreDestino from TransferenciasAlmacenes t " +
                "left join Representantes r on r.RepCodigo = case when t.TraTipo = 1 then t.AlmCodigoOrigen else t.AlmCodigoDestino end " +
                "where t.TraID = ? and t.RepCodigo = '"+Arguments.CurrentUser.RepCodigo+"' ", 
                new string[] { traId.ToString() }).FirstOrDefault();
        }

        public List<TransferenciasAlmacenesDetalle> GetDetalleBySecuencia(int traId)
        {
            return SqliteManager.GetInstance().Query<TransferenciasAlmacenesDetalle>("select * from TransferenciasAlmacenesDetalle t " +
                "inner join Productos p on p.ProCodigo = t.ProCodigo " +
                "where t.TraID = ? and t.RepCodigo = '"+Arguments.CurrentUser.RepCodigo+"' order by p.ProDescripcion", 
                new string[] { traId.ToString() });
        }

        public void InsertarTraspasoInTemp(int traId, bool confirmado)
        {
            new DS_Productos().ClearTemp((int)Modules.TRASPASOS);

            var query = "select distinct " + ((int)Modules.TRASPASOS).ToString() + " as TitID, pd.TadCantidad as Cantidad, pd.TadCantidadDetalle as CantidadDetalle, pd.rowguid as rowguid, p.ProID as ProID, 0 as Precio, " +
                    "p.ProDescripcion as Descripcion, 0 as Itbis, 0 as Selectivo, 0 as Advalorem, 0 as DesPorciento,  0 as DesPorcientoManual, ifnull(p.UnmCodigo, '') as UnmCodigo, " +
                    "0 as IndicadorOferta, 0 as Descuento from TransferenciasAlmacenesDetalle pd " +
                    "inner join Productos p on p.ProCodigo = pd.ProCodigo where ltrim(rtrim(pd.RepCodigo)) = '"+Arguments.CurrentUser.RepCodigo.Trim()+"' and pd.TraID = ? order by p.ProDescripcion";

            var list = SqliteManager.GetInstance().Query<ProductosTemp>(query, new string[] { traId.ToString() });

            SqliteManager.GetInstance().InsertAll(list);

        }
    }
}
