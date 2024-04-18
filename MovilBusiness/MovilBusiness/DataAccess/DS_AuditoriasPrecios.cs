using MovilBusiness.Configuration;
using MovilBusiness.Enums;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;

namespace MovilBusiness.DataAccess
{
    public class DS_AuditoriasPrecios
    {


        public List<CategoriasAuditoriasPrecios> GetCategorias()
        {
            string sql = "SELECT * FROM (SELECT NULL AS CatCodigo, 'TODOS' AS CatDescripcion from CategoriasAuditoriasPrecios LIMIT 1) S UNION ALL SELECT CatCodigo, CatDescripcion FROM CategoriasAuditoriasPrecios";
            return SqliteManager.GetInstance().Query<CategoriasAuditoriasPrecios>(sql, new string[] { });
        }

        public List<MarcasAuditoriasPrecios> GetMarcasByCategoria(string catCodigo)
        {
            string sql = "SELECT * FROM (SELECT '01' AS CatCodigo, NULL AS MarCodigo, 'TODOS' as MarDescripcion from MarcasAuditoriasPrecios LIMIT 1) S UNION ALL SELECT CatCodigo, MarCodigo, MarDescripcion from MarcasAuditoriasPrecios where CatCodigo = '" + catCodigo + "'";
            return SqliteManager.GetInstance().Query<MarcasAuditoriasPrecios>(sql, new string[] { });
        }

        public List<ProductosTemp> GetProductos(string search = null, string catCodigo = null, string marCodigo = null, bool resumen = false, ProductosArgs args = null)
        {
            var where = "";

            if (!resumen)
            {

                if (args.filter != null)
                {
                    where += Functions.DinamicFiltersGenerateScript(args.filter, args.valueToSearch, args.secondFilter);
                }
                else if (!string.IsNullOrWhiteSpace(search))
                {
                    where += " and (p.ProCodigo = '" + search + "' or p.ProDescripcion like '%" + search + "%') ";
                }

                if (!string.IsNullOrWhiteSpace(catCodigo))
                {
                    where += " and p.CatCodigo = '" + catCodigo + "' ";
                }

                if (!string.IsNullOrWhiteSpace(marCodigo))
                {
                    where += " and p.MarCodigo = '" + marCodigo + "' ";
                }
            }

            var query = "select 1 as ProUnidades, p.ProCodigo, t.IndicadorPresencia as IndicadorPresencia, upper(p.ProDescripcion) as Descripcion, " + ((int)Arguments.Values.CurrentModule).ToString()+" as TitID, t.rowguid as rowguid, t.Cantidad as Cantidad, " +
                "t.Precio as Precio, p.CatCodigo as CatCodigo, p.MarCodigo as MarCodigo, c.CatDescripcion as ProDescripcion3, m.MarDescripcion as ProDescripcion1, ifnull(p.ProPresentacion,'') as ProDescripcion2,  t.PrecioOferta as PrecioOferta, t.Caras as Caras " +
                "from ProductosAuditoriasPrecios p " +
                "inner join CategoriasAuditoriasPrecios c on c.CatCodigo = p.CatCodigo " +
                "inner join MarcasAuditoriasPrecios m on m.CatCodigo = c.CatCodigo and m.MarCodigo = p.MarCodigo " +
                (resumen?" inner ":" left ") + " join ProductosTemp t on t.ProCodigo = p.ProCodigo and t.CatCodigo = p.CatCodigo and t.MarCodigo = p.MarCodigo and t.TitID = " + ((int)Arguments.Values.CurrentModule).ToString()+" " +
                "where 1=1 " + where + " order by p.ProDescripcion";

            return SqliteManager.GetInstance().Query<ProductosTemp>(query, new string[] { });
        }

        public void GuardarAuditoria(List<ProductosTemp> productos)
        {
            var audSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("AuditoriasPrecios");

            var map = new Hash("AuditoriasPrecios");
            map.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            map.Add("AudSecuencia", audSecuencia);
            map.Add("CliID", Arguments.Values.CurrentClient.CliID);
            map.Add("CliCodigo", Arguments.Values.CurrentClient.CliCodigo);
            map.Add("AudFecha", Functions.CurrentDate());
            map.Add("AudEstatus", 1);
            map.Add("VisSecuencia", Arguments.Values.CurrentVisSecuencia);
            if (Arguments.Values.CurrentCuaSecuencia != -1) {
                map.Add("CuaSecuencia", Arguments.Values.CurrentCuaSecuencia);
            }
            map.Add("mbVersion", Functions.AppVersion);
            map.Add("AudFechaActualizacion", Functions.CurrentDate());
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map.Add("rowguid", Guid.NewGuid().ToString());
            map.ExecuteInsert();

            //var productos = GetProductos(resumen: true);

            int pos = 1;
            foreach(ProductosTemp item in productos){
                var det = new Hash("AuditoriasPreciosDetalle");
                det.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                det.Add("AudSecuencia", audSecuencia);
                det.Add("AudPosicion", pos); pos++;
                det.Add("ProCodigo", item.ProCodigo);
                det.Add("CatCodigo", item.CatCodigo);
                det.Add("MarCodigo", item.MarCodigo);
                det.Add("AudCantidad", item.Cantidad);
                det.Add("AudPrecio", item.Precio);
                det.Add("AudFechaActualizacion", Functions.CurrentDate());
                det.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                det.Add("audPresencia", item.IndicadorPresencia ? "1" : "0");
                det.Add("AudPrecioOferta", item.PrecioOferta);
                det.Add("AudCaras", item.Caras);
                det.Add("rowguid", Guid.NewGuid().ToString());
                det.ExecuteInsert();
            }

            DS_RepresentantesSecuencias.UpdateSecuencia("AuditoriasPrecios", audSecuencia);

            new DS_Productos().ClearTemp((int)Modules.AUDITORIAPRECIOS);
        }

        public void EstAuditoria(string rowguid, int est)
        {
            var ped = new Hash("AuditoriasPrecios");
            ped.Add("AudEstatus", est);
            ped.Add("AudFechaActualizacion", Functions.CurrentDate());
            ped.Add("UsuInicioSesion",/* Arguments.CurrentUser.RepCodigo*/"mdsoft");

            if (est == 0)
            {
                if (new DS_SuscriptoresCambios().UpdateCambioEstadoInsertByRowguid(rowguid, est))
                {
                    ped.SaveScriptForServer = false;
                }
            }

            ped.ExecuteUpdate("rowguid = '" + rowguid + "'");
        }
    }
}
