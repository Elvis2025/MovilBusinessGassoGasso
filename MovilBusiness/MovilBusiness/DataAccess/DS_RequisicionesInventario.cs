using MovilBusiness.Configuration;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovilBusiness.DataAccess
{
    public class DS_RequisicionesInventario
    {
        public int SaveRequisicion()
        {
            var reqSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("RequisicionesInventario");

            var map = new Hash("RequisicionesInventario");
            map.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            map.Add("ReqSecuencia", reqSecuencia);
            map.Add("ReqFecha", Functions.CurrentDate());
            map.Add("ReqEstatus", 1);

            var myProd = new DS_Productos();

            var productos = myProd.GetResumenProductos((int)Arguments.Values.CurrentModule);

            map.Add("ReqCantidadDetalle", productos.Count);

            if (Arguments.Values.CurrentCuaSecuencia != -1)
            {
                map.Add("CuaSecuencia", Arguments.Values.CurrentCuaSecuencia);
            }
            map.Add("Motid", 1);
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map.Add("ReqFechaActualizacion", Functions.CurrentDate());
            map.Add("rowguid", Guid.NewGuid().ToString());
            map.Add("mbVersion", Functions.AppVersion);
            map.ExecuteInsert();

            var line = 1;
            foreach(var prod in productos)
            {
                var det = new Hash("RequisicionesInventarioDetalle");
                det.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                det.Add("ReqSecuencia", reqSecuencia);
                det.Add("ReqPosicion", line); line++;
                det.Add("ProID", prod.ProID);
                det.Add("ReqCantidad", prod.Cantidad);
                det.Add("ReqCantidadDetalle", prod.CantidadDetalle);
                det.Add("UnmCodigo", prod.UnmCodigo);
                det.Add("ReqFechaActualizacion", Functions.CurrentDate());
                det.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                det.Add("rowguid", Guid.NewGuid().ToString());
                det.ExecuteInsert();
            }

            DS_RepresentantesSecuencias.UpdateSecuencia("RequisicionesInventario", reqSecuencia);

            return reqSecuencia;
        }

        public RequisicionesInventario GetBySecuencia(int reqSecuencia)
        {
            return SqliteManager.GetInstance().Query<RequisicionesInventario>("select *, e.EstDescripcion as EstDescripcion from RequisicionesInventario t " +
                "left join Estados e on e.EstTabla = 'RequisicionesInventario' and e.EstEstado = t.ReqEstatus " +
                "where t.ReqSecuencia = ? and t.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' ",
                new string[] { reqSecuencia.ToString() }).FirstOrDefault();
        }

        public List<RequisicionesInventarioDetalle> GetDetalleBySecuencia(int traId)
        {
            return SqliteManager.GetInstance().Query<RequisicionesInventarioDetalle>("select * from RequisicionesInventarioDetalle t " +
                "inner join Productos p on p.ProID = t.ProID " +
                "where t.ReqSecuencia = ? and t.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' order by p.ProDescripcion",
                new string[] { traId.ToString() });
        }

        public RequisicionesInventario GetRequisicionesInventarioByMaxSecuencia(bool confirmado = false)
        {
            var query = "SELECT ReqFecha,ReqEstatus,ReqSecuencia FROM " + (confirmado ? "RequisicionesInventarioConfirmados" : "RequisicionesInventario") + "  WHERE ReqSecuencia = (SELECT MAX(ReqSecuencia) FROM " + (confirmado ? "RequisicionesInventarioConfirmados" : "RequisicionesInventario") + ") and RepCodigo = " + Arguments.CurrentUser.RepCodigo;

            var list = SqliteManager.GetInstance().Query<RequisicionesInventario>(query, new string[] { });

            if (list.Count > 0)
            {
                return list[0];
            }

            return null;
        }


    }
}
