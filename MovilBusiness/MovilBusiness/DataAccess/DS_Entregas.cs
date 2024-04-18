using MovilBusiness.Configuration;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovilBusiness.DataAccess
{
    public class DS_Entregas : DS_Controller
    {
        public int GuardarEntrega(bool forPromocion, int cantidadCanastos = -1)
        {
            var entSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Entregas");

            var ent = new Hash("Entregas");
            ent.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            ent.Add("EntSecuencia", entSecuencia);
            ent.Add("CliID", Arguments.Values.CurrentClient.CliID);
            ent.Add("EntFecha", Functions.CurrentDate());
            ent.Add("EntEstatus", 1);

            var myProd = new DS_Productos();

            var productsTemp = myProd.GetResumenProductos(forPromocion ? (int)Modules.PROMOCIONES : (int)Modules.ENTREGASMERCANCIA, false);

            ent.Add("EntTotal", productsTemp.Count); //cantidad de lineas
            ent.Add("ConID", Arguments.Values.CurrentClient.ConID);
            ent.Add("EntIndicadorCompleto", true);
            ent.Add("EntTipo", forPromocion ? 19 : 17);

            if (myParametro.GetParEntregasPromocionesUsarCanastos() && Arguments.Values.CurrentModule == Modules.PROMOCIONES)
            {
                ent.Add("EntCantidadCanastos", cantidadCanastos);

                new DS_TransaccionesCanastos().SaveCanastos(TipoCapturaCanastos.ENTREGARCANASTOS, cantidadCanastos, new List<string>());
            }

            if (Arguments.Values.CurrentCuaSecuencia != -1)
            {
                ent.Add("CuaSecuencia", Arguments.Values.CurrentCuaSecuencia);
            }

            ent.Add("VisSecuencia", Arguments.Values.CurrentVisSecuencia);
            ent.Add("MonCodigo", Arguments.Values.CurrentClient.MonCodigo);
            ent.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            ent.Add("rowguid", Guid.NewGuid().ToString());
            ent.Add("mbVersion", Functions.AppVersion);
            ent.Add("EntFechaActualizacion", Functions.CurrentDate());
            ent.ExecuteInsert();

            var pos = 1;
            var myInv = new DS_Inventarios();

            var parLimiteSemanal = myParametro.GetParEntregasMercanciaLimiteEntregar();
            var week = Functions.GetWeekOfYear(DateTime.Now);
            var year = DateTime.Now.Year;

            foreach (var prod in productsTemp)
            {
                var det = new Hash("EntregasDetalle");
                det.Add("EntSecuencia", entSecuencia);
                det.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                det.Add("EntPosicion", pos); pos++;
                det.Add("ProID", prod.ProID);
                det.Add("EntPrecio", prod.Precio);
                if (forPromocion)
                {
                    det.Add("EntCantidadSolicitada", prod.Cantidad);
                    det.Add("EntCantidadDetalleSolicitada", prod.CantidadDetalle);
                    det.Add("EntCantidad", 0);
                    det.Add("EntCantidadDetalle", 0);
                }
                else
                {
                    det.Add("EntCantidad", prod.Cantidad);
                    det.Add("EntCantidadDetalle", prod.CantidadDetalle);
                    det.Add("EntCantidadSolicitada", 0);
                    det.Add("EntCantidadDetalleSolicitada", 0);
                }
                det.Add("EntIndicadorOferta", false);
                det.Add("EntDescuento", prod.Descuento);
                det.Add("EntDescPorciento", prod.DesPorciento);
                det.Add("EntItbis", prod.Itbis);
                det.Add("EntSelectivo", prod.Selectivo);
                det.Add("EntAdValorem", prod.AdValorem);
                det.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                det.Add("EntFechaActualizacion", Functions.CurrentDate());
                det.Add("rowguid", Guid.NewGuid().ToString());
                det.ExecuteInsert();

                myInv.RestarInventario(prod.ProID, prod.Cantidad, prod.CantidadDetalle);

                if (parLimiteSemanal)
                {
                    SaveRepresentanteEntregasSemanal(year, week, prod.ProID, prod.Cantidad);
                }
            }

            DS_RepresentantesSecuencias.UpdateSecuencia("Entregas", entSecuencia);

            if (DS_RepresentantesParametros.GetInstance().GetParVisitasResultados())
            {
                ActualizarVisitasResultados(forPromocion);
            }
            

            myProd.ClearTemp((int)Arguments.Values.CurrentModule);

            return entSecuencia;
        }

        private void ActualizarVisitasResultados(bool promo)
        {
            var list = SqliteManager.GetInstance().Query<VisitasResultados>("select count(*) as VisCantidadTransacciones, " +
                "sum(((d.PedPrecio + d.PedAdValorem + d.PedSelectivo) - d.PedDescuento) * ((case when d.PedCantidadDetalle > 0 then d.PedCantidadDetalle / o.ProUnidades else 0 end) + d.PedCantidad)) as VisMontoSinItbis, sum(((d.PedItbis / 100.0) * ((d.PedPrecio + d.PedAdValorem + d.PedSelectivo) - d.PedDescuento)) * ((case when d.PedCantidadDetalle > 0 then d.PedCantidadDetalle / o.ProUnidades else 0 end) + d.PedCantidad)) as VisMontoItbis from Pedidos p " +
                "inner join PedidosDetalle d on d.RepCodigo = p.RepCodigo and d.PedSecuencia = p.PedSecuencia " +
                "inner join Productos o on o.ProID = d.ProID " +
                "where p.EntTipo = "+(promo ? 19 : 17).ToString()+" and  p.VisSecuencia = ? and p.RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'", new string[] { Arguments.Values.CurrentVisSecuencia.ToString() });

            if (list != null && list.Count > 0)
            {
                var item = list.FirstOrDefault();

                item.VisMontoTotal = item.VisMontoSinItbis + item.VisMontoItbis;
                item.VisComentario = "";
                item.TitID = promo ? 19 : 17;

                new DS_Visitas().GuardarVisitasResultados(item);
            }
        }

        private void SaveRepresentanteEntregasSemanal(int year, int week, int proId, double quantity)
        {
            var map = new Hash("RepresentantesEntregasSemanal");
            map.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            map.Add("SemSemana", week);
            map.Add("SemAno", year);
            map.Add("ProID", proId);
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map.Add("ProCantidadDetalle", 0);

            var item = SqliteManager.GetInstance().Query<RepresentantesEntregasSemanal>("select * from RepresentantesEntregasSemanal " +
                "where SemSemana = ? and SemAno = ? and ProID = ? and RepCodigo = '"+Arguments.CurrentUser.RepCodigo+"'", 
                new string[] { week.ToString(), year.ToString(), proId.ToString() }).FirstOrDefault();

            if(item != null)
            {
                map.Add("ProCantidad", item.ProCantidad + quantity);
                map.ExecuteUpdate("rowguid = '" + item.rowguid + "'");
            }
            else
            {
                map.Add("ProCantidad", quantity);
                map.Add("rowguid", Guid.NewGuid().ToString());
                map.ExecuteInsert();
            }           
        }

        public double GetCantidadLimiteParaEntregar(int proId)
        {
            var list = SqliteManager.GetInstance().Query<Productos>("Select ifnull(ProCantidad,0) as ProCantidad " +
                "from RepresentantesLimiteEntregas where ProID = ? and trim(RepCodigo) = '"+Arguments.CurrentUser.RepCodigo.Trim()+"' ", 
                new string[] { proId.ToString() });

            if(list != null && list.Count > 0)
            {
                return list[0].ProCantidad;
            }

            return 0;
        }

        public double GetCantidadLimiteSemanalParaEntregar(int proId, int year, int week)
        {
            var list = SqliteManager.GetInstance().Query<Productos>("Select ifnull(ProCantidad,0) as ProCantidad " +
                "from RepresentantesEntregasSemanal where ProID = ? and SemAno = ? and SemSemana = ? and trim(RepCodigo) = '"+Arguments.CurrentUser.RepCodigo.Trim()+"' ",
                new string[] { proId.ToString(), year.ToString(), week.ToString() });

            if (list != null && list.Count > 0)
            {
                return list[0].ProCantidad;
            }

            return 0;
        }

        public Entregas GetBySecuencia(int entSecuencia)
        {
            var list = SqliteManager.GetInstance().Query<Entregas>("select * from Entregas " +
                "where EntSecuencia = ? and RepCodigo = '"+Arguments.CurrentUser.RepCodigo.Trim()+"'", 
                new string[] { entSecuencia.ToString() });

            if(list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public List<EntregasDetalle> GetDetalleBySecuencia(int entSecuencia)
        {
            return SqliteManager.GetInstance().Query<EntregasDetalle>("select e.EntSecuencia as EntSecuencia, e.EntCantidad as EntCantidad, " +
                "p.ProCodigo as ProCodigo, p.ProDescripcion as ProDescripcion " +
                "from EntregasDetalle d " +
                "inner join Productos p on p.ProID = d.ProID " +
                "where e.EntSecuencia = ? and e.RepCodigo = '"+Arguments.CurrentUser.RepCodigo.Trim()+"'", 
                new string[] { entSecuencia.ToString() });
        }
    }
}
