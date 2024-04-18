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
    public class DS_ColocacionProductos : DS_Controller
    {
        public int SaveColocacion(int invArea = -1)
        {
            var col = new Hash("ColocacionProductos");
           
            int colSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("ColocacionProductos");

            col.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            col.Add("ColSecuencia", colSecuencia);
            col.Add("CliID", Arguments.Values.CurrentClient.CliID);
            col.Add("ColFecha", Functions.CurrentDate());

            if (Arguments.Values.CurrentCuaSecuencia != -1)
            {
                col.Add("CuaSecuencia", Arguments.Values.CurrentCuaSecuencia);
            }
            col.Add("VisSecuencia", Arguments.Values.CurrentVisSecuencia);
            col.Add("ColEstatus", 1);
            col.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            col.Add("ColFechaActualizacion", Functions.CurrentDate());
            col.Add("rowguid", Guid.NewGuid().ToString());

            if(invArea > 0)
            {
                col.Add("InvArea", invArea);
            }

            col.Add("mbVersion", Functions.AppVersion);

            col.ExecuteInsert();

            var myProd = new DS_Productos();

            var productos = myProd.GetResumenProductos((int)Modules.COLOCACIONMERCANCIAS, isfromsave: true);

            var parArea = myParametro.GetParColocacionProductosCapturarArea();

            bool parInvArea = !string.IsNullOrWhiteSpace(parArea) && parArea.ToUpper().Trim() == "D";

            if (productos != null)
            {
                int pos = 1;

                var parTomarCantidades = myParametro.GetParColocacionProductosTomarCantidades() > 0;
                //var parCapturarCantidadFacing = myParametro.GetParInventarioFisicoCapturarFacing();

                foreach (var prod in productos)
                {
                    Hash cold = new Hash("ColocacionProductosDetalle");

                    cold.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                    cold.Add("ColSecuencia", colSecuencia);
                    cold.Add("ColPosicion", pos); pos++;
                    cold.Add("ProID", prod.ProID);
                    // invd.Add("infTipoInventario", 1);
                    cold.Add("rowguid", Guid.NewGuid().ToString());

                    if (parTomarCantidades)
                    {
                        cold.Add("ColCantidad", prod.CanTidadGond);
                        cold.Add("ColCantidadDetalle", prod.CantidadAlm);
                    }
                    else
                    {
                        cold.Add("ColCantidad", prod.Cantidad);
                        cold.Add("ColCantidadDetalle", prod.CantidadDetalle);
                    }

                    cold.Add("CliID", Arguments.Values.CurrentClient.CliID);
                    cold.Add("ColFechaActualizacion", Functions.CurrentDate());
                    cold.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);

                    if (parInvArea)
                    {
                        cold.Add("InvArea", prod.InvAreaId);
                    }

                    cold.ExecuteInsert();
                }
            }


            if (DS_RepresentantesParametros.GetInstance().GetParVisitasResultados())
            {
                ActualizarVisitasResultados();
            }

            myProd.ClearTemp((int)Modules.COLOCACIONMERCANCIAS);

            DS_RepresentantesSecuencias.UpdateSecuencia("ColocacionProductos", colSecuencia);

            return colSecuencia;
        }

        private void ActualizarVisitasResultados()
        {
            var list = SqliteManager.GetInstance().Query<VisitasResultados>("select '' as VisComentario, 68 as TitID, count(*) as VisCantidadTransacciones, '' as VisComentarios " +
                "from ColocacionProductos where RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and VisSecuencia = ? ",
                new string[] { Arguments.Values.CurrentVisSecuencia.ToString() });

            if (list != null && list.Count > 0)
            {
                new DS_Visitas().GuardarVisitasResultados(list[0]);
            }
        }

        public ColocacionProductos GetColocacionBySecuencia(int colSecuencia)
        {
            string query = "select VisSecuencia, CliID, rowguid, ifnull(replace( strftime('%d-%m-%Y', SUBSTR(ColFecha,1,10)),' ','' ), '') as ColFecha from ColocacionProductos where ColSecuencia = ? ";

            var colProd = SqliteManager.GetInstance().Query<ColocacionProductos>(query, new string[] { colSecuencia.ToString() }).FirstOrDefault();

            return colProd;
        }

        public List<ColocacionProductosDetalle> GetColocacionProductosDetalles(int colSecuencia)
        {
            string query = $@"select p.ProDescripcion as ProDescripcion, p.ProCodigo as ProCodigo, 
                           ColCantidad, ind.ProID, ColCantidadDetalle from
                           ColocacionProductosDetalle ind inner join Productos p on p.proid = ind.ProID  
                           where ColSecuencia = {colSecuencia}";

            if (myParametro.GetParColocacionProductosTomarCantidades() > 0)
            {
                query = $@"select InvArea, ProUnidades, ProDescripcion, ProCodigo,
                      SUM(ColCantidad) as ColCantidad, SUM(ColCantidadDetalle) as ColCantidadDetalle from(   
                      select ProUnidades, ind.ProID as ProID, p.ProDescripcion as ProDescripcion, p.ProCodigo as ProCodigo, 
                      ColCantidad, NULL ColCantidadDetalle, InvArea from ColocacionProductosDetalle
                      ind inner join Productos p on p.proid = ind.ProID where ColSecuencia = {colSecuencia}
                      union
                      select ProUnidades, ind.ProID as ProID, p.ProDescripcion as ProDescripcion, p.ProCodigo as ProCodigo,
                      NULL ColCantidad, ColCantidadDetalle, InvArea from ColocacionProductosDetalle
                      ind inner join Productos p on p.proid = ind.ProID where ColSecuencia = {colSecuencia}) t
                      group by ProID";
            }

            return SqliteManager.GetInstance().Query<ColocacionProductosDetalle>(query);
        }

        public void InsertarColocacionInTemp(int colSecuencia)
        {
            new DS_Productos().ClearTemp((int)Modules.COLOCACIONMERCANCIAS);

            string query = "select  distinct " + ((int)Modules.COLOCACIONMERCANCIAS).ToString() + " as TitID, i.ProID as ProID, p.ProDescripcion as ProDescripcion, p.UnmCodigo as UnmCodigo, p.ProCodigo as ProCodigo, i.ColCantidad as Cantidad," +
                           "0 as PrecioTemp, i.rowguid as rowguid, p.ProItbis as Itbis, i.ColCantidadDetalle as CantidadDetalle from ColocacionProductosDetalle i " +
                           "inner join Productos p on p.proid = i.Proid where ltrim(rtrim(i.RepCodigo)) = '"+ Arguments.CurrentUser.RepCodigo.Trim()+"' and ColSecuencia = ? ";

            var list = SqliteManager.GetInstance().Query<ProductosTemp>(query, new string[] { colSecuencia.ToString() });

            foreach (var ent in list)
            {
                ent.rowguid = Guid.NewGuid().ToString();
            }

            SqliteManager.GetInstance().InsertAll(list);
        }

        public void EstColocacion(string rowguid, int est)
        {
            var ped = new Hash("ColocacionProductos");
            ped.Add("ColEstatus", est);
            ped.Add("ColFechaActualizacion", Functions.CurrentDate());
            ped.Add("UsuInicioSesion", "mdsoft");

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
