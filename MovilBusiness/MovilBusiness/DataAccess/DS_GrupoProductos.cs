using MovilBusiness.Configuration;
using MovilBusiness.model;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.DataAccess
{
    public class DS_GrupoProductos
    {
        public List<Productos> GetProductosRecordar()
        {
            return SqliteManager.GetInstance().Query<Productos>("select distinct p.ProCodigo as ProCodigo, p.ProDescripcion as ProDescripcion from Productos p "+
                "inner join GrupoProductosDetalle g on ltrim(rtrim(upper(g.GrpCodigo))) in ('PRECORDAR', 'PEDREC"+Arguments.CurrentUser.RepCodigo.ToUpper()+"') and g.ProID = p.ProID " +
                "and p.ProID not in (select ProID from ProductosTemp where ifnull(IndicadorOferta, 0) = 0) " +
                "order by ProDescripcion", new string[] { });
        }

        public List<Productos> GetLineasRecordar(string usoMultipleGeneral, string usoMultipleUsuario)
        {
            return SqliteManager.GetInstance().Query<Productos>("select distinct l.LinID as ProCodigo, l.LinDescripcion as ProDescripcion from Lineas l " +
                "inner join UsosMultiples u on ltrim(rtrim(upper(u.CodigoGrupo))) in ('" + usoMultipleGeneral + "', '" + usoMultipleUsuario + "') and u.CodigoUso = l.LinID " +
                "where not exists (select 1 from ProductosTemp p where p.Linea = l.linid and ifnull(IndicadorOferta, 0) = 0)  " +
                "order by l.LinDescripcion", new string[] { });
        }

        public List<Productos> GetLineasRecordarByBusqueda(string usoMultipleGeneral, string usoMultipleUsuario, string pedSecuencia)
        {
            return SqliteManager.GetInstance().Query<Productos>("select distinct l.LinID as ProCodigo, l.LinDescripcion as ProDescripcion from Lineas l " +
                "inner join UsosMultiples u on ltrim(rtrim(upper(u.CodigoGrupo))) in ('" + usoMultipleGeneral + "', '" + usoMultipleUsuario + "') and u.CodigoUso = l.LinID " +
                "where not exists (select 1 from PedidosBusquedas p where ifnull(p.PedCodigo,'0') = u.CodigoUso and PedCampo like '%Linea%' and p.PedSecuencia = ? and p.Repcodigo = ? ) " +
                "order by l.LinDescripcion", new string[] { pedSecuencia , Arguments.CurrentUser.RepCodigo });
        }

        public List<Productos> GetProductosByGrpCodigo(string grpCodigo)
        {
            return SqliteManager.GetInstance().Query<Productos>("select distinct p.ProCodigo as ProCodigo, p.ProDescripcion as ProDescripcion from Productos p " +
                "inner join GrupoProductosDetalle g on ltrim(rtrim(upper(g.GrpCodigo))) = upper('" + grpCodigo+"') and g.ProID = p.ProID " +
                "order by ProDescripcion", new string[] { });
        }

        public string GetGrpDescripcion(string grpCodigo)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<UsosMultiples>("select ifnull(GrpDescripcion, '') as Descripcion from GrupoProductos " +
                    "where trim(GrpCodigo) = '" + grpCodigo.Trim() + "'", new string[] { });

                if (list != null && list.Count > 0)
                {
                    return list[0].Descripcion;
                }

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return "";
        }
    }
}
