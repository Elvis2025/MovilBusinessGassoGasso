using MovilBusiness.Configuration;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.model.Internal.Structs.Args;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Services;
using MovilBusiness.Utils;
using MovilBusiness.Views.Components.TemplateSelector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovilBusiness.DataAccess
{
    public class DS_Reportes : DS_Controller
    {
        private ApiManager api;

        public DS_Reportes()
        {
            try
            {
                api = ApiManager.GetInstance(new PreferenceManager().GetConnection().Url);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        public List<ReportesNameQuantity> GetResumenDevoluciones(string desde = null, string hasta = null, string secCodigo = null)
        {
            string where = "";

            if(!string.IsNullOrWhiteSpace(desde) && !string.IsNullOrWhiteSpace(hasta))
            {
                where = " and cast(strftime('%Y%m%d', d.DevFecha) as integer) between cast(strftime('%Y%m%d', '"+desde+ "') as integer) and cast(strftime('%Y%m%d', '"+hasta+"') as integer) ";
            }

            if (!string.IsNullOrWhiteSpace(secCodigo))
            {
                where += " and d.SecCodigo = '"+secCodigo+"' ";
            }

            return SqliteManager.GetInstance().Query<ReportesNameQuantity>("select 1 as IsMoneyAmount, c.CliNombre as Name, 1 as IsMoneyAmount, count(dd.ProID) as Quantity, 0.0 as Amount " +
                "from Devoluciones d " +
                "inner join DevolucionesDetalle dd on dd.DevSecuencia = d.DevSecuencia and dd.RepCodigo = d.RepCodigo " +
                "inner join Clientes c on c.CliID = d.CliID " +
                "where trim(d.RepCodigo) = ? "+where+" group by d.DevSecuencia, trim(d.RepCodigo) " + 
                "order by d.DevSecuencia", new string[] { Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public List<ReportesNameQuantity> GetResumenPushMoney(string desde = null, string hasta = null, string secCodigo = null)
        {
            string where = "";

            if (!string.IsNullOrWhiteSpace(desde) && !string.IsNullOrWhiteSpace(hasta))
            {
                where = " and cast(strftime('%Y%m%d', co.ComFecha) as integer) between cast(strftime('%Y%m%d', '" + desde + "') as integer) and cast(strftime('%Y%m%d', '" + hasta + "') as integer)  and co.ComEstatus <> 0  ";
            }

            if (!string.IsNullOrEmpty(secCodigo))
            {
                where += " and co.SecCodigo = '"+secCodigo+"' ";
            }

            string query = " select 1 as IsMoneyAmount, (result.proCodigo ||'-'|| result.proNombre) as Name, 1 as IsMoneyAmount, result.ComPrecio as Amount, sum(result.ComCantidad) as Quantity from ( " +
            "select p.Procodigo, p.ProDescripcion as proNombre, sum(cd.Comcantidad) as ComCantidad, cd.ComPrecio as ComPrecio " +
            "from Compras co inner join ComprasDetalle cd on co.repcodigo = cd.repcodigo and co.Comsecuencia = cd.Comsecuencia " +
            "left outer join productos p on p.Proid = cd.proid where trim(co.RepCodigo) = ? " + where
            + "group by p.Procodigo, p.ProDescripcion " +
            "Union All " +
            " select p.Procodigo, p.ProDescripcion as proNombre, sum(cd.Comcantidad) as ComCantidad, cd.ComPrecio as ComPrecio from ComprasConfirmados co " +
            "inner join ComprasDetalleConfirmados cd on co.repcodigo = cd.repcodigo and co.Comsecuencia = cd.Comsecuencia "
            + "left outer join productos p on p.Proid = cd.proid  " +
            "where trim(co.RepCodigo) = ? " + where + "group by p.Procodigo, p.ProDescripcion) as result " +
            "GROUP BY result.proCodigo, result.proNombre ";
            return SqliteManager.GetInstance().Query<ReportesNameQuantity>(query, new string[] { Arguments.CurrentUser.RepCodigo.Trim(), Arguments.CurrentUser.RepCodigo.Trim() });


        }

        public List<FacturasVencidas> GetResumenFacturasVencidas()
        {
           return SqliteManager.GetInstance().Query<FacturasVencidas>("select  0 as IsVisbleMoneda, c.CliNombre as CliNombre, cxc.CxcDocumento as Factura, " +
                "strftime('%Y-%m-%d', cxc.cxcFechaVencimiento) as Fecha, " +
                "printf('%.2f', cxc.CxcBalance) as Balance from CuentasxCobrar cxc " +
                "inner join Clientes c on c.CliID = cxc.CliID " +
                "where ifnull(julianday(cxc.cxcFechaVencimiento), 0) < julianday(strftime('%Y-%m-%d','now','" + Functions.GetDiferenciaHorariaSqlite() + " hours')) " +
                "order by c.CliNombre", new string[] { });
        }
        
        public List<FacturasAvencerDelMes> GetResumenFacturasVencidasDelMes(int cliid)
        {

            string query = "select CxcDocumento as Factura, " +
                     "strftime('%Y-%m-%d', cxcFechaVencimiento) as FechaVenc, strftime('%Y-%m-%d', cxcFecha) as FechaFact, " +
                     "printf('%.2f', CxcBalance) as Balance from CuentasxCobrar c inner join TiposTransaccionesCXC cc " +
                     "on cc.ttcID = CxcTipoTransaccion " +
                     "where cliid = '" + cliid + "' and ifnull(julianday(cxcFechaVencimiento), 0) < julianday(strftime('%Y-%m-%d','now','" + Functions.GetDiferenciaHorariaSqlite() + " hours')) and cc.ttcOrigen <> -1";

           var list = SqliteManager.GetInstance().Query<FacturasAvencerDelMes>(query, new string[] { });
            
            list = list.Where(cxc => DateTime.Parse(cxc.FechaVenc) < DateTime.Now.AddDays(30)).ToList();

            foreach(var cxc in list)
            {
                cxc.diasCredito = DateTime.Parse(cxc.FechaVenc).Day - DateTime.Now.Day;
            }

            return list;
        }

        public List<Clientes> GetClientesForFacturasDelMes()
        {
            return SqliteManager.GetInstance().Query<Clientes>("select DISTINCT c.cliid as cliid, c.CliCodigo as CliCodigo, c.clinombre as CliNombre from CuentasxCobrar cxc " +
                 "inner join Clientes c on c.CliID = cxc.CliID " +
                 "where ifnull(julianday(cxc.cxcFechaVencimiento), 0) < julianday(strftime('%Y-%m-%d','now','" + Functions.GetDiferenciaHorariaSqlite() + " hours')) " +
                 "order by c.CliNombre", new string[] { });
        }

        public List<PosiblesCobrosDias> GetResumenFacturasxDia(int Cliid, DateTime fechaActual)
        {

            string query = "select cl.CliCodigo as Cliente, cl.CliNombre as Nombre, CxcDocumento as Factura, " +
                     "strftime('%Y-%m-%d', cxcFechaVencimiento) as FechaVenc, strftime('%Y-%m-%d', cxcFecha) as FechaFact, " +
                     "printf('%.2f', CxcBalance) as Balance from CuentasxCobrar c " +
                     "inner join clientes cl on cl.CliID = c.CliID inner join TiposTransaccionesCXC cc " +
                     "on cc.ttcID = CxcTipoTransaccion " +
                     "where c.CliID = '" + Cliid + "'  and cc.ttcOrigen <> -1";

            var list = SqliteManager.GetInstance().Query<PosiblesCobrosDias>(query, new string[] { });
            list = list.Where(cxc => DateTime.Parse(cxc.FechaFact) <= fechaActual).ToList();

            foreach (var cxc in list)
            {
                TimeSpan difFechas = fechaActual - DateTime.Parse(cxc.FechaFact);
                cxc.diasCredito = difFechas.Days;
            }

            return list;
        }

        public List<CuentasxCobrar> GetForFechasFacturas(string desde = null, string hasta = null)
        {
            string where = "";

            if (!string.IsNullOrWhiteSpace(desde) && !string.IsNullOrWhiteSpace(hasta))
            {
                where = "  CAST(strftime('%Y%m%d',cxc.CxcFecha) AS INTEGER) between cast(strftime('%Y%m%d', '" + desde + "') as integer) and cast(strftime('%Y%m%d', '" + hasta + "') as integer) ";
            }

            return SqliteManager.GetInstance().Query<CuentasxCobrar>("select strftime('%Y-%m-%d', CxcFecha) as CxcFecha from CuentasxCobrar cxc where " + where + " " +
                 "group by cxc.cxcFecha " +
                 "order by cxc.cxcFecha", new string[] { });
        }

        public List<Clientes> GetClientesxRutaVisita(string desde = null, string hasta = null)
        {
            DateTime timespan = DateTime.Parse(desde);
            int time = Math.Abs(timespan.Day - DateTime.Parse(hasta).Day);
            List<Clientes> clientes = new List<Clientes>();


            for (int i = 0; i <= time; i++)
            {
                DateTime dates = timespan.AddDays(i);
                var numeroSemana = 0;
                if (myParametro.GetParSemanasAnios())
                {
                    numeroSemana = new DS_RutaVisitas().GetNumeroSemana(dates);
                }
                else
                {
                    var rep = new DS_Representantes().GetAllRepresentantes().Where(x => x.RepCodigo.Trim().ToUpper() == Arguments.CurrentUser.RepCodigo.Trim().ToUpper()).FirstOrDefault();
                    numeroSemana = Functions.GetWeekOfMonth(dates, rep);
                    if (numeroSemana > 4)
                    {
                        numeroSemana = 4;
                    }
                }

                RutasVisitasArgs RutaVisitaData = new RutasVisitasArgs()
                {
                    DiaDeLaSemana = (int)(dates).DayOfWeek - 1 == -1 ? 6 : (int)(dates).DayOfWeek - 1,
                    NumeroSemana = numeroSemana
                };

                char[] diasSemana = new char[] { '_', '_', '_', '_', '_', '_', '_' };

                diasSemana[RutaVisitaData.DiaDeLaSemana] = '1';
                string semanaValues = new string(diasSemana);
                string where = " AND R.RutSemana" + RutaVisitaData.NumeroSemana.ToString() + " like '" + semanaValues + "'";

                clientes.AddRange(SqliteManager.GetInstance().Query<Clientes>("select c.CliNombre, c.CliID from Clientes c inner join RutaVisitas R on R.cliid = c.CliID " +
                        "where R.RutSemana" + RutaVisitaData.NumeroSemana.ToString() + " like '" + semanaValues + "'" +
                         "order by CliNombre", new string[] { }));
            }

            return clientes;
        }

        public List<FacturasVencidas> GetResumenFacturasClientesPendientes(int cliid)
        {

            var list = SqliteManager.GetInstance().Query<FacturasVencidas>("select 1 as IsVisbleMoneda, cxc.CxcDocumento as Factura, " +
                "strftime('%Y-%m-%d', cxc.cxcFechaVencimiento) as Fecha, ifnull(m.monsigla, '') as MonSigla, " +
                "printf('%.2f', cxc.CxcBalance) as Balance from CuentasxCobrar cxc " +
                "inner join Clientes c on c.CliID = cxc.CliID left join Monedas m on m.monCodigo = c.monCodigo where c.CliID = ?", new string[] { cliid.ToString() });

            return list;
        }

        public List<Clientes> GetClientesPendientes()
        {
         
            var numeroSemana = 0;
            if (myParametro.GetParSemanasAnios())
            {
                numeroSemana = new DS_RutaVisitas().GetNumeroSemana(DateTime.Now);
            }
            else
            {
                var rep =  new DS_Representantes().GetAllRepresentantes().Where(x => x.RepCodigo.Trim().ToUpper() == Arguments.CurrentUser.RepCodigo.Trim().ToUpper()).FirstOrDefault();
                numeroSemana = Functions.GetWeekOfMonth(DateTime.Now, rep);
                if (numeroSemana > 4)
                {
                    numeroSemana = 4;
                }
            }

            RutasVisitasArgs RutaVisitaData = new RutasVisitasArgs()
            {
                DiaDeLaSemana = (int)(DateTime.Now).DayOfWeek - 1 == -1 ? 6 : (int)(DateTime.Now).DayOfWeek - 1,
                NumeroSemana = numeroSemana
            };

            char[] diasSemana = new char[] { '_', '_', '_', '_', '_', '_', '_' };

            diasSemana[RutaVisitaData.DiaDeLaSemana] = '1';
            string semanaValues = new string(diasSemana);
            string where = " AND R.RutSemana" + RutaVisitaData.NumeroSemana.ToString() + " like '" + semanaValues + "'";

            return SqliteManager.GetInstance().Query<Clientes>("select c.CliNombre, c.CliID from Clientes c inner join RutaVisitas R on R.cliid = c.CliID " +
                    "where c.CliID not in (select CliID from Visitas where VisFechaEntrada like '" + Functions.CurrentDate("yyyy-MM-dd") + "%') "+
                     "AND R.RutSemana" + RutaVisitaData.NumeroSemana.ToString() + " like '" + semanaValues + "'" +
                     "order by CliNombre", new string[] { });
        }

        public List<GastosReportes> GetReporteGastos(string desde = null, string hasta = null)
        {
            string where = "";

            if(!string.IsNullOrWhiteSpace(desde) && !string.IsNullOrWhiteSpace(hasta))
            {
                where = "and cast(strftime('%Y%m%d', g.GasFecha) as integer) between cast(strftime('%Y%m%d', '"+desde+ "') as integer) and cast(strftime('%Y%m%d', '"+hasta+"') as integer) ";
            }

            return SqliteManager.GetInstance().Query<GastosReportes>("select GasRNC, GasNoDocumento, GasNombreProveedor, printf('%.2f', GasMontoTotal) as GasMontoTotal, GasBaseImponible, GasFechaDocumento, " +
                "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(GasFecha,1,10)),' ','' ), '') as GasFecha, GasNCF, u.Descripcion as TipoGasto, printf('%.2f', GasItebis) as GasItebis, printf('%.2f', GasPropina) as GasPropina " +
                "from Gastos g " +
                "left join UsosMultiples u on trim(upper(u.CodigoGrupo)) = 'TIPOGASTOS' and trim(upper(CAST(CodigoUso as INT))) = trim(upper(CAST(g.GasTipo as INT))) " +
                "where trim(RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' "+where+" order by GasSecuencia", new string[] { });
        }

        public List<RowLinker> GetResumenPedidos(string desde = null, string hasta = null, bool withItbis = false, string secCodigo = null)
        {
            string where = "";

            if (!string.IsNullOrWhiteSpace(desde) && !string.IsNullOrWhiteSpace(hasta))
            {
                where = " and cast(strftime('%Y%m%d',o.Pedfecha) as integer) between cast(strftime('%Y%m%d', '" + desde + "') as integer) and cast(strftime('%Y%m%d', '" + hasta + "') as integer) ";
            }

            if (!string.IsNullOrWhiteSpace(secCodigo))
            {
                where += " and o.SecCodigo = '"+secCodigo+"' ";
            }

            string monto;

            if (withItbis)
            {
                monto = "ifnull(SUM(((cast(pd.PedPrecio as float) - cast(pd.PedDescuento as float)) * (1 + cast(pd.PedItbis as float)/100)) * (cast(pd.PedCantidad as float) + ifnull((cast(ifnull(pd.PedCantidadDetalle, 0) as float) / cast(ifnull(p.ProUnidades, 1) as float)),0))), 0) as Amount,  cast(ifnull(pd.PedDescuento,0)  as float) as DiscountAmount ";
           
            }
            else
            {
                monto = "ifnull(SUM(((cast(pd.PedPrecio as float) - cast(pd.PedDescuento as float)) ) * (cast(pd.PedCantidad as float) + ifnull((cast(ifnull(pd.PedCantidadDetalle, 0) as float) / cast(ifnull(p.ProUnidades, 1) as float)),0) ) ), 0)  as Amount, cast(ifnull(pd.PedDescuento,0) as float) as DiscountAmount ";
            }

            var list = SqliteManager.GetInstance().Query<ReportesNameQuantity>("select 1 as IsMoneyAmount, o.PedSecuencia||' - '||c.CliNombre as Name, 1 as IsMoneyAmount, sum(pd.PedCantidad) as Quantity, " + monto + " " +
                "from Pedidos o " +
                "inner join PedidosDetalle pd on pd.PedSecuencia = o.PedSecuencia and trim(pd.RepCodigo) = trim(o.RepCodigo) " +
                "inner join Clientes c on c.CliID = o.CliID " +
                "inner join Productos p on p.ProID = pd.ProID " +
                "where trim(o.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and o.PedEstatus /*in (1,2,4)*/ <> 0 " + where + " group by o.PedSecuencia, c.CliNombre, c.CliID " +
                "union " +
                "select 1 as IsMoneyAmount, o.PedSecuencia||' - '||c.CliNombre as Name, 1 as IsMoneyAmount, ifnull(sum(pd.PedCantidad), 0) as Quantity, " + monto + " " +
                "from PedidosConfirmados o " +
                "inner join PedidosDetalleConfirmados pd on pd.PedSecuencia = o.PedSecuencia and trim(o.RepCodigo) = trim(pd.RepCodigo) " +
                "inner join Clientes c on c.CliID = o.CliID " +
                "inner join Productos p on p.ProID = pd.ProID " +
                "where trim(o.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and o.PedEstatus /*in (1,2,4)*/ <> 0 " + where + " group by o.PedSecuencia and c.CliNombre, c.CliID " +
                "order by 1 asc", new string[] { });

            var result = new List<RowLinker>();

            string query = "select round(sum(a.cantidad),2) as invCantidad " +
                "from (select case when upper(ifnull(d.UnmCodigo, '')) = 'CJ' then sum(d.PedCantidad) else sum( (d.PedCantidad * 1.0) / p.ProUnidades) end as cantidad from " +
                "Pedidos o " +
                "inner join PedidosDetalle d on d.RepCodigo = o.RepCodigo and d.PedSecuencia = o.PedSecuencia " +
                "inner join Productos p on p.ProID = d.ProID " +
                "where o.RepCodigo = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' " + where + " and o.PedEstatus /*in (1,2,4)*/ <> 0 " +
                "union " +
                "select case when upper(ifnull(d.UnmCodigo, '')) = 'CJ' then sum(d.PedCantidad) else sum((d.PedCantidad * 1.0) / p.ProUnidades) end as cantidad from " +
                "PedidosConfirmados o " +
                "inner join PedidosDetalleConfirmados d on d.RepCodigo = o.repCodigo and d.PedSecuencia = o.PedSecuencia " +
                "inner join Productos p on p.ProID = d.ProID " +
                "where o.RepCodigo = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' " + where + " and o.PedEstatus /*in (1,2,4)*/ <> 0) a";
            var cajasList = SqliteManager.GetInstance().Query<Inventarios>(query, 
                new string[] { }).FirstOrDefault();

            var cajas = 0.0;

            if(cajasList != null)
            {
                cajas = cajasList.invCantidad;
            }

            result.AddRange(list);

            result.Add(new RowLinker());
            result.Add(new Inventarios() { ProDescripcion = "Total items: ", invCantidad = list.Sum(x => double.Parse(x.Quantity)), Bold = true });
            result.Add(new Inventarios() { ProDescripcion = "Cajas vendidas: ", invCantidad = cajas, Bold = true });
            result.Add(new Inventarios() { ProDescripcion = "Total desc: ", invCantidad = list.Sum(x => double.Parse(x.DiscountAmount)), Bold = true, FormatForMoney = true });

            result.Add(new Inventarios() { ProDescripcion = "Monto total: ", invCantidad = list.Sum(x => double.Parse(x.Amount)), Bold = true, FormatForMoney = true, });
            result.Add(new SubTitle() { Description = "DETALLE DE PEDIDOS" });
            result.Add(new ReportesNameQuantity() { Name = "Productos", Quantity = "Cantidad", Amount = "Monto", Bold = true });
            result.AddRange(SqliteManager.GetInstance().Query<ReportesNameQuantity>("select 1 as IsMoneyAmount, case when pd.PedIndicadorOferta = 1 then p.ProDescripcion||' (Oferta)' else p.ProDescripcion end as Name, sum(pd.PedCantidad) as Quantity, " + monto + " " +
                "from Pedidos o " +
                "inner join PedidosDetalle pd on pd.PedSecuencia = o.PedSecuencia and trim(pd.RepCodigo) = trim(o.RepCodigo) " +
                "inner join Productos p on p.ProID = pd.ProID " +
                "where trim(o.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and o.PedEstatus /*in (1,2,4)*/ <> 0 " + where + " group by p.ProDescripcion, p.ProID, pd.PedIndicadorOferta " +
                "union " +
                "select 1 as IsMoneyAmount, case when pd.PedIndicadorOferta = 1 then p.ProDescripcion||' (Oferta)' else p.ProDescripcion end as Name, sum(pd.PedCantidad) as Quantity, " + monto + " " +
                "from PedidosConfirmados o " +
                "inner join PedidosDetalleConfirmados pd on pd.PedSecuencia = o.PedSecuencia and trim(o.RepCodigo) = trim(pd.RepCodigo) " +
                "inner join Productos p on p.ProID = pd.ProID " +
                "where trim(o.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and o.PedEstatus /*in (1,2,4)*/ <> 0 " + where + " group by p.ProDescripcion, p.ProID, pd.PedIndicadorOferta " +
                "order by 1 asc", new string[] { }));

            result.Add(new RowLinker());
            result.Add(new Inventarios() { ProDescripcion = "Total items: ", invCantidad = list.Sum(x => double.Parse(x.Quantity)), Bold = true });

            result.Add(new Inventarios() { ProDescripcion = "Total desc: ", invCantidad = list.Sum(x => double.Parse(x.DiscountAmount)), Bold = true, FormatForMoney = true });

            result.Add(new Inventarios() { ProDescripcion = "Monto total: ", invCantidad = list.Sum(x => double.Parse(x.Amount)), Bold = true, FormatForMoney = true, });
            result.Add(new SubTitle() { Description = "PEDIDOS ANULADOS" });
            result.Add(new ReportesNameQuantity() { Name = "Clientes", Quantity = "Lineas", Amount = "Monto", Bold = true });
            result.AddRange(SqliteManager.GetInstance().Query<ReportesNameQuantity>("select 1 as IsMoneyAmount, o.PedSecuencia||' - '||c.CliNombre as Name, sum(pd.PedCantidad) as Quantity, " + monto + " " +
                "from Pedidos o " +
                "inner join PedidosDetalle pd on pd.PedSecuencia = o.PedSecuencia and trim(pd.RepCodigo) = trim(o.RepCodigo) " +
                "inner join Clientes c on c.CliID = o.CliID " +
                "inner join Productos p on p.ProID = pd.ProID " +
                "where trim(o.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and o.PedEstatus = 0 " + where + " group by o.PedSecuencia, c.CliNombre, c.CliID " +
                "order by 1 asc", new string[] { }));

          

            return result;
        }

        public List<RowLinker> GetResumenPedidosToQuantityConversion(string desde = null, string hasta = null, bool withItbis = false, string secCodigo = null)
        {
            string where = "";

            if (!string.IsNullOrWhiteSpace(desde) && !string.IsNullOrWhiteSpace(hasta))
            {
                where = " and cast(strftime('%Y%m%d',o.Pedfecha) as integer) between cast(strftime('%Y%m%d', '" + desde + "') as integer) and cast(strftime('%Y%m%d', '" + hasta + "') as integer) ";
            }

            if (!string.IsNullOrWhiteSpace(secCodigo))
            {
                where += " and o.SecCodigo = '" + secCodigo + "' ";
            }

            string monto;

            if (withItbis)
            {
                monto = "ifnull(SUM(((cast(pd.PedPrecio as float) - cast(pd.PedDescuento as float)) * (1 + cast(pd.PedItbis as float)/100)) * (cast(pd.PedCantidad as float) + ifnull((cast(ifnull(pd.PedCantidadDetalle, 0) as float) / cast(ifnull(p.ProUnidades, 1) as float)),0))), 0) as Amount,  cast(ifnull(pd.PedDescuento,0)  as float) as DiscountAmount ";

            }
            else
            {
                monto = "ifnull(SUM(((cast(pd.PedPrecio as float) - cast(pd.PedDescuento as float)) ) * (cast(pd.PedCantidad as float) + ifnull((cast(ifnull(pd.PedCantidadDetalle, 0) as float) / cast(ifnull(p.ProUnidades, 1) as float)),0) ) ), 0)  as Amount, cast(ifnull(pd.PedDescuento,0) as float) as DiscountAmount ";
            }

            var list = SqliteManager.GetInstance().Query<ReportesNameQuantity>("select 1 as IsMoneyAmount, 1 as IsDecimalQuantity, o.PedSecuencia||' - '||c.CliNombre as Name, 1 as IsMoneyAmount, 1 as IsDecimalQuantity, ifnull(sum(Cast((pd.PedCantidad * p.ProCantidad) as decimal) / 125.00 ), 0) as Quantity, " + monto + " " +
                "from Pedidos o " +
                "inner join PedidosDetalle pd on pd.PedSecuencia = o.PedSecuencia and trim(pd.RepCodigo) = trim(o.RepCodigo) " +
                "inner join Clientes c on c.CliID = o.CliID " +
                "inner join Productos p on p.ProID = pd.ProID " +
                "where trim(o.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and o.PedEstatus /*in (1,2,4)*/ <> 0 " + where + " group by o.PedSecuencia, c.CliNombre, c.CliID " +
                "union " +
                "select 1 as IsMoneyAmount, 1 as IsDecimalQuantity, o.PedSecuencia||' - '||c.CliNombre as Name, 1 as IsMoneyAmount, 1 as IsDecimalQuantity, ifnull(sum(Cast((pd.PedCantidad * p.ProCantidad) as decimal) / 125.00 ), 0)  as Quantity, " + monto + " " +
                "from PedidosConfirmados o " +
                "inner join PedidosDetalleConfirmados pd on pd.PedSecuencia = o.PedSecuencia and trim(o.RepCodigo) = trim(pd.RepCodigo) " +
                "inner join Clientes c on c.CliID = o.CliID " +
                "inner join Productos p on p.ProID = pd.ProID " +
                "where trim(o.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and o.PedEstatus /*in (1,2,4)*/ <> 0 " + where + " group by o.PedSecuencia and c.CliNombre, c.CliID " +
                "order by 1 asc", new string[] { });

            var result = new List<RowLinker>();

            result.AddRange(list);

            result.Add(new RowLinker());
            result.Add(new Inventarios() { ProDescripcion = "Total items: ", invCantidad = list.Sum(x => double.Parse(x.Quantity)), Bold = true });
            result.Add(new Inventarios() { ProDescripcion = "Total desc: ", invCantidad = list.Sum(x => double.Parse(x.DiscountAmount)), Bold = true, FormatForMoney = true });

            result.Add(new Inventarios() { ProDescripcion = "Monto total: ", invCantidad = list.Sum(x => double.Parse(x.Amount)), Bold = true, FormatForMoney = true, });
            result.Add(new SubTitle() { Description = $"DETALLE DE PEDIDOS - (125 LB)" });
            result.Add(new ReportesNameQuantity() { Name = "Productos", Quantity = "Cantidad", Amount = "Monto", Bold = true });
            result.AddRange(SqliteManager.GetInstance().Query<ReportesNameQuantity>("select 1 as IsMoneyAmount, 1 as IsDecimalQuantity, case when pd.PedIndicadorOferta = 1 then p.ProDescripcion||' (Oferta)' else p.ProDescripcion end as Name, Cast((sum(pd.PedCantidad) * p.ProCantidad) as decimal) / 125.00 as Quantity, " + monto + " " +
                "from Pedidos o " +
                "inner join PedidosDetalle pd on pd.PedSecuencia = o.PedSecuencia and trim(pd.RepCodigo) = trim(o.RepCodigo) " +
                "inner join Productos p on p.ProID = pd.ProID " +
                "where trim(o.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and o.PedEstatus /*in (1,2,4)*/ <> 0 " + where + " group by p.ProDescripcion, p.ProID, pd.PedIndicadorOferta " +
                "union " +
                "select 1 as IsMoneyAmount, 1 as IsDecimalQuantity, case when pd.PedIndicadorOferta = 1 then p.ProDescripcion||' (Oferta)' else p.ProDescripcion end as Name, Cast((sum(pd.PedCantidad) * p.ProCantidad) as decimal) / 125.00 as Quantity, " + monto + " " +
                "from PedidosConfirmados o " +
                "inner join PedidosDetalleConfirmados pd on pd.PedSecuencia = o.PedSecuencia and trim(o.RepCodigo) = trim(pd.RepCodigo) " +
                "inner join Productos p on p.ProID = pd.ProID " +
                "where trim(o.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and o.PedEstatus /*in (1,2,4)*/ <> 0 " + where + " group by p.ProDescripcion, p.ProID, pd.PedIndicadorOferta " +
                "order by 1 asc", new string[] { }));

            result.Add(new RowLinker());
            result.Add(new Inventarios() { ProDescripcion = "Total items: ", invCantidad = list.Sum(x => double.Parse(x.Quantity)), Bold = true });

            result.Add(new Inventarios() { ProDescripcion = "Total desc: ", invCantidad = list.Sum(x => double.Parse(x.DiscountAmount)), Bold = true, FormatForMoney = true });

            result.Add(new Inventarios() { ProDescripcion = "Monto total: ", invCantidad = list.Sum(x => double.Parse(x.Amount)), Bold = true, FormatForMoney = true, });
            result.Add(new SubTitle() { Description = "PEDIDOS ANULADOS" });
            result.Add(new ReportesNameQuantity() { Name = "Clientes", Quantity = "Lineas", Amount = "Monto", Bold = true });
            result.AddRange(SqliteManager.GetInstance().Query<ReportesNameQuantity>("select 1 as IsMoneyAmount, 1 as IsDecimalQuantity, o.PedSecuencia||' - '||c.CliNombre as Name, Cast((sum(pd.PedCantidad) * p.ProCantidad) as decimal) / 125.00 as Quantity, " + monto + " " +
                "from Pedidos o " +
                "inner join PedidosDetalle pd on pd.PedSecuencia = o.PedSecuencia and trim(pd.RepCodigo) = trim(o.RepCodigo) " +
                "inner join Clientes c on c.CliID = o.CliID " +
                "inner join Productos p on p.ProID = pd.ProID " +
                "where trim(o.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and o.PedEstatus = 0 " + where + " group by o.PedSecuencia, c.CliNombre, c.CliID " +
                "order by 1 asc", new string[] { }));



            return result;
        }

        public List<RowLinker> GetResumenCotizaciones(string desde = null, string hasta = null, string secCodigo = null,bool isdetalle = false)
        {
            string where = "";

            if (!string.IsNullOrWhiteSpace(desde) && !string.IsNullOrWhiteSpace(hasta))
            {
                where = " and cast(strftime('%Y%m%d',o.Cotfecha) as integer) between cast(strftime('%Y%m%d', '" + desde + "') as integer) and cast(strftime('%Y%m%d', '" + hasta + "') as integer) ";
            }

            if (!string.IsNullOrWhiteSpace(secCodigo))
            {
                where += " and o.SecCodigo = '" + secCodigo + "' ";
            }

            string monto;

            monto = "SUM(((cast(cd.CotPrecio as float) - cast(cd.CotDescuento as float)) ) * (cast(cd.CotCantidad as float) + (cast(ifnull(cd.CotCantidadDetalle, 0) as float) / cast(ifnull(p.ProUnidades, 1) as float))  ) )  as Amount, cast(ifnull(cd.CotDescuento,0) as float) as DiscountAmount ";


            var list = SqliteManager.GetInstance().Query<ReportesNameQuantity>("select 1 as IsMoneyAmount, o.CotSecuencia||' - '||c.CliNombre as Name, 1 as IsMoneyAmount, sum(cd.cotCantidad) as Quantity, " + monto + " " +
                "from Cotizaciones o " +
                "inner join CotizacionesDetalle cd on cd.CotSecuencia = o.CotSecuencia and trim(cd.RepCodigo) = trim(o.RepCodigo) " +
                "inner join Clientes c on c.CliID = o.CliID " +
                "inner join Productos p on p.ProID = cd.ProID " +
                "where trim(o.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and o.CotEstatus /*in (1,2,4)*/ <> 0 " + where + " group by o.CotSecuencia, c.CliNombre, c.CliID " +
                "union " +
                "select 1 as IsMoneyAmount, o.CotSecuencia||' - '||c.CliNombre as Name, 1 as IsMoneyAmount, sum(cd.CotCantidad) as Quantity, " + monto + " " +
                "from CotizacionesConfirmados o " +
                "inner join CotizacionesDetalleConfirmados cd on cd.CotSecuencia = o.CotSecuencia and trim(o.RepCodigo) = trim(cd.RepCodigo) " +
                "inner join Clientes c on c.CliID = o.CliID " +
                "inner join Productos p on p.ProID = cd.ProID " +
                "where trim(o.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and o.CotEstatus /*in (1,2,4)*/ <> 0 " + where + " group by o.CotSecuencia and c.CliNombre, c.CliID " +
                "order by 1 asc", new string[] { });

            var result = new List<RowLinker>();

            string query = "select round(sum(a.cantidad),2) as invCantidad " +
                "from (select case when upper(ifnull(d.UnmCodigo, '')) = 'CJ' then sum(d.CotCantidad) else sum( (d.CotCantidad * 1.0) / p.ProUnidades) end as cantidad from " +
                "Cotizaciones o " +
                "inner join CotizacionesDetalle d on d.RepCodigo = o.RepCodigo and d.CotSecuencia = o.CotSecuencia " +
                "inner join Productos p on p.ProID = d.ProID " +
                "where o.RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' " + where + " and o.CotEstatus /*in (1,2,4)*/ <> 0 " +
                "union " +
                "select case when upper(ifnull(d.UnmCodigo, '')) = 'CJ' then sum(d.CotCantidad) else sum((d.CotCantidad * 1.0) / p.ProUnidades) end as cantidad from " +
                "CotizacionesConfirmados o " +
                "inner join CotizacionesDetalleConfirmados d on d.RepCodigo = o.repCodigo and d.CotSecuencia = o.CotSecuencia " +
                "inner join Productos p on p.ProID = d.ProID " +
                "where o.RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' " + where + " and o.CotEstatus /*in (1,2,4)*/ <> 0) a";
            var cajasList = SqliteManager.GetInstance().Query<Inventarios>(query,
                new string[] { }).FirstOrDefault();

            var cajas = 0.0;

            if (cajasList != null)
            {
                cajas = cajasList.invCantidad;
            }

            result.AddRange(list);
            result.Add(new RowLinker());
            result.Add(new Inventarios() { ProDescripcion = "Total items: ", invCantidad = list.Sum(x => double.Parse(x.Quantity)), Bold = true });
            result.Add(new Inventarios() { ProDescripcion = "Cajas vendidas: ", invCantidad = cajas, Bold = true });
            result.Add(new Inventarios() { ProDescripcion = "Total desc: ", invCantidad = list.Sum(x => double.Parse(x.DiscountAmount)), Bold = true, FormatForMoney = true });

            result.Add(new Inventarios() { ProDescripcion = "Monto total: ", invCantidad = list.Sum(x => double.Parse(x.Amount)), Bold = true, FormatForMoney = true, });
           
            if(!isdetalle)
            {
                result.Add(new SubTitle() { Description = "DETALLE DE COTIZACIONES" });
                result.Add(new ReportesNameQuantity() { Name = "Productos", Quantity = "Cantidad", Amount = "Monto", Bold = true });
                result.AddRange(SqliteManager.GetInstance().Query<ReportesNameQuantity>("select 1 as IsMoneyAmount, case when cd.CotIndicadorOferta = 1 then p.ProDescripcion||' (Oferta)' else p.ProDescripcion end as Name, sum(cd.CotCantidad) as Quantity, " + monto + " " +
                    "from Cotizaciones o " +
                    "inner join CotizacionesDetalle cd on cd.CotSecuencia = o.CotSecuencia and trim(cd.RepCodigo) = trim(o.RepCodigo) " +
                    "inner join Productos p on p.ProID = cd.ProID " +
                    "where trim(o.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and o.CotEstatus /*in (1,2,4)*/ <> 0 " + where + " group by p.ProDescripcion, p.ProID, cd.CotIndicadorOferta " +
                    "union " +
                    "select 1 as IsMoneyAmount, case when cd.CotIndicadorOferta = 1 then p.ProDescripcion||' (Oferta)' else p.ProDescripcion end as Name, sum(cd.CotCantidad) as Quantity, " + monto + " " +
                    "from CotizacionesConfirmados o " +
                    "inner join CotizacionesDetalleConfirmados cd on cd.CotSecuencia = o.CotSecuencia and trim(o.RepCodigo) = trim(cd.RepCodigo) " +
                    "inner join Productos p on p.ProID = cd.ProID " +
                    "where trim(o.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and o.CotEstatus /*in (1,2,4)*/ <> 0 " + where + " group by p.ProDescripcion, p.ProID, cd.CotIndicadorOferta " +
                    "order by 1 asc", new string[] { }));

                result.Add(new RowLinker());
                result.Add(new Inventarios() { ProDescripcion = "Total items: ", invCantidad = list.Sum(x => double.Parse(x.Quantity)), Bold = true });

                result.Add(new Inventarios() { ProDescripcion = "Total desc: ", invCantidad = list.Sum(x => double.Parse(x.DiscountAmount)), Bold = true, FormatForMoney = true });

                result.Add(new Inventarios() { ProDescripcion = "Monto total: ", invCantidad = list.Sum(x => double.Parse(x.Amount)), Bold = true, FormatForMoney = true, });
            }

            
            result.Add(new SubTitle() { Description = "COTIZACIONES ANULADAS" });
            result.Add(new ReportesNameQuantity() { Name = "Clientes", Quantity = "Lineas", Amount = "Monto", Bold = true });
            result.AddRange(SqliteManager.GetInstance().Query<ReportesNameQuantity>("select 1 as IsMoneyAmount, o.CotSecuencia||' - '||c.CliNombre as Name, sum(cd.CotCantidad) as Quantity, " + monto + " " +
                "from Cotizaciones o " +
                "inner join CotizacionesDetalle cd on cd.CotSecuencia = o.CotSecuencia and trim(cd.RepCodigo) = trim(o.RepCodigo) " +
                "inner join Clientes c on c.CliID = o.CliID " +
                "inner join Productos p on p.ProID = cd.ProID " +
                "where trim(o.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and o.CotEstatus = 0 " + where + " group by o.CotSecuencia, c.CliNombre, c.CliID " +
                "order by 1 asc", new string[] { }));



            return result;
        }

        public List<RowLinker> GetDesempeno(string fechaRaw)
        {
            var list = new List<RowLinker>()
            {
                new SubTitle(){Description = "DESEMPEÑO"}
            };

            int CicloSemana = 4;

            int parCicloSemana = myParametro.GetParCiclosSemanas();
            if (parCicloSemana > 0)
            {
                CicloSemana = parCicloSemana;
            }

            var fecha = DateTime.Now;

            if(DateTime.TryParse(fechaRaw, out DateTime date))
            {
                fecha = date;
            }

            int weekNumber = Functions.GetWeekOfMonth(fecha);
            int dayNumber = (int)fecha.DayOfWeek - 1;

            weekNumber = weekNumber % CicloSemana;

            if (weekNumber == 0)
            {
                weekNumber = CicloSemana;
            }

            if (dayNumber == -1)
            {
                dayNumber = 6;
            }

            char[] diasSemana = new char[] { '_', '_', '_', '_', '_', '_', '_' };

            diasSemana[dayNumber] = '1';
            string semanaValues = new string(diasSemana);
            string newWhere = " AND r.RutSemana" + weekNumber.ToString() + " like '" + semanaValues + "' ";

            var aRealizar = SqliteManager.GetInstance().Query<Inventarios>("select count(r.CliID) as invCantidad, 'Visitas a realizar' as ProDescripcion " +
                "from RutaVisitas r " +
                "where trim(r.RepCodigo) = ? " + newWhere, new string[] { Arguments.CurrentUser.RepCodigo.Trim() });

            list.Add(new Inventarios() { ProDescripcion = "Visitas a realizar: ", invCantidad = aRealizar != null && aRealizar.Count > 0 ? aRealizar[0].invCantidad : 0 });

            var realizadas = SqliteManager.GetInstance().Query<Inventarios>("select count(distinct CliID) as invCantidad, 'Visitas realizadas:' as ProDescripcion " +
                "from Visitas where trim(RepCodigo) = ? and VisFechaEntrada like '"+fecha.ToString("yyyy-MM-dd")+"%' ", 
                new string[] { Arguments.CurrentUser.RepCodigo.Trim()});

            list.Add(new Inventarios() { ProDescripcion = "Visitas realizadas: ", invCantidad = realizadas != null && realizadas.Count > 0 ? realizadas[0].invCantidad : 0 });

            var positivas = SqliteManager.GetInstance().Query<Inventarios>("select count(distinct CliID) as invCantidad, 'Visitas positivas' as ProDescripcion " +
                "from Pedidos where PedFecha like '"+fecha.ToString("yyyy-MM-dd")+"%' and trim(RepCodigo) = ? ", new string[] { Arguments.CurrentUser.RepCodigo.Trim() });

            list.Add(new Inventarios() { ProDescripcion = "Visitas positivas: ", invCantidad = positivas != null && positivas.Count > 0 ? positivas[0].invCantidad : 0 });

            return list;
        }

        public List<RowLinker> GetResumenVentas(string desde = null, string hasta = null, bool itbis = false)
        {
            string where = "";

            if (!string.IsNullOrWhiteSpace(desde) && !string.IsNullOrWhiteSpace(hasta))
            {
                where = " and cast(strftime('%Y%m%d',o.VenFecha) as integer) between cast(strftime('%Y%m%d', '"+desde+ "') as integer) and cast(strftime('%Y%m%d', '"+hasta+"') as integer) ";
            }

            string monto;

            monto = itbis ? "sum(((cast(pd.VenPrecio as float) + cast(pd.VenSelectivo as float) + cast(pd.VenAdValorem as float) - cast(pd.VenDescuento as float)) * (cast(pd.VenCantidad as float) + (cast(pd.VenCantidadDetalle as float) / cast(p.ProUnidades as float)))) + pd.venTotalitbis) as Amount "
            : "SUM(((cast(pd.VenPrecio as float) - cast(pd.VenDescuento as float)) ) * (cast(pd.VenCantidad as float) + (cast(ifnull(pd.VenCantidadDetalle, 0) as float) / cast(ifnull(p.ProUnidades, 1) as float))   ))  as Amount ";

            string query = "select 1 as IsMoneyAmount, o.VenSecuencia||' - '||c.CliNombre as Name, sum(pd.VenCantidad) as Quantity, " + monto + " " +
                "from Ventas o " +
                "inner join VentasDetalle pd on pd.VenSecuencia = o.VenSecuencia and trim(pd.RepCodigo) = trim(o.RepCodigo) " +
                "inner join Clientes c on c.CliID = o.CliID " +
                "inner join Productos p on p.ProID = pd.ProID " +
                "where trim(o.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus /* in (1,2,4)*/ <> 0 " + where + " group by o.VenSecuencia, c.CliNombre, c.CliID " +
                "union " +
                "select 1 as IsMoneyAmount, o.VenSecuencia||' - '||c.CliNombre as Name, sum(pd.VenCantidad) as Quantity, " + monto + " " +
                "from VentasConfirmados o " +
                "inner join VentasDetalleConfirmados pd on pd.VenSecuencia = o.VenSecuencia and trim(o.RepCodigo) = trim(pd.RepCodigo) " +
                "inner join Clientes c on c.CliID = o.CliID " +
                "inner join Productos p on p.ProID = pd.ProID " +
                "where trim(o.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus /*in (1,2,4)*/ <> 0 " + where + " group by o.VenSecuencia and c.CliNombre, c.CliID " +
                "order by 1 asc";
            var list = SqliteManager.GetInstance().Query<ReportesNameQuantity>(query, 
                new string[] {});

            var result = new List<RowLinker>();

            result.AddRange(list);

            ///////////////////////////DETALLE VENTAS//////////////////////////
            result.Add(new SubTitle() { Description = "DETALLE DE VENTAS" });

            var listVentasDetalle = SqliteManager.GetInstance().Query<ReportesNameQuantity>("select 1 as IsMoneyAmount, case when pd.VenindicadorOferta = 1 then p.ProDescripcion||' (Oferta)' else p.ProDescripcion end as Name, sum(pd.VenCantidad) as Quantity, " + monto + " " +
            "from Ventas o " +
            "inner join VentasDetalle pd on pd.VenSecuencia = o.VenSecuencia and trim(pd.RepCodigo) = trim(o.RepCodigo) " +
            "inner join Productos p on p.ProID = pd.ProID " +
            "where trim(o.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus /*in (1,2,4) */ <> 0 and pd.venIndicadorOferta <> 1 " + where + " group by p.ProDescripcion, p.ProID, pd.VenindicadorOferta " +
            "union " +
            "select 1 as IsMoneyAmount, case when pd.VenindicadorOferta = 1 then p.ProDescripcion||' (Oferta)' else p.ProDescripcion end as Name, sum(pd.VenCantidad) as Quantity, " + monto + " " +
            "from VentasConfirmados o " +
            "inner join VentasDetalleConfirmados pd on pd.VenSecuencia = o.VenSecuencia and trim(o.RepCodigo) = trim(pd.RepCodigo) " +
            "inner join Productos p on p.ProID = pd.ProID " +
            "where trim(o.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus /*in (1,2,4)*/ <> 0  and pd.venIndicadorOferta <> 1 " + where + " group by p.ProDescripcion, p.ProID, pd.VenindicadorOferta " +
            "order by 1 asc", new string[] { });

            result.AddRange(listVentasDetalle);

            /////////////////////////////////////TOTAL DE VENTAS///////////////////////////////////////////////////////
            var totalVentas = SqliteManager.GetInstance().Query<ReportesNameQuantity>("Select 1 as IsMoneyAmount, Round(sum(resultado.Amount),2) as Amount from ( select  " + monto + " from Ventas o " +
                "inner join VentasDetalle pd on pd.VenSecuencia = o.VenSecuencia and trim(pd.RepCodigo) = trim(o.RepCodigo) " +
                "inner join Clientes c on c.CliID = o.CliID " +
                "inner join Productos p on p.ProID = pd.ProID " +
                "where trim(o.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus  <> 0 /*(1,2,4)*/ " + where + " " +
                "union " +
                "select " + monto + " from VentasConfirmados o " +
                "inner join VentasDetalleConfirmados pd on pd.VenSecuencia = o.VenSecuencia and trim(o.RepCodigo) = trim(pd.RepCodigo) " +
                "inner join Clientes c on c.CliID = o.CliID " +
                "inner join Productos p on p.ProID = pd.ProID " +
                "where trim(o.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus /*in (1,2,4)*/ <> 0 " + where + " ) as resultado ", 
                new string[] { });
            
            result.Add(new SubTitle() { Description = "Total Ventas : " + totalVentas[0].AmountFormated });



            ////////////////////////////////////VENTAS A CREDITO/////////////////////////
            var listVentasCreditoCabecera = SqliteManager.GetInstance().Query<ReportesNameQuantity>("select 1 as IsMoneyAmount, o.VenSecuencia||' - '||c.CliNombre as Name, sum(pd.VenCantidad) as Quantity, " + monto + " " +
               "from Ventas o " +
               "inner join VentasDetalle pd on pd.VenSecuencia = o.VenSecuencia and trim(pd.RepCodigo) = trim(o.RepCodigo) " +
               "inner join Clientes c on c.CliID = o.CliID " +
               "inner join Productos p on p.ProID = pd.ProID " +
               "inner join CondicionesPago cp on cp.ConId = o.ConID "+
               "where trim(o.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus  <> 0 /*(1,2,4)*/ and cp.ConDiasVencimiento <> 0 " + where + " group by o.VenSecuencia, c.CliNombre, c.CliID " +
               "union " +
               "select 1 as IsMoneyAmount, o.VenSecuencia||' - '||c.CliNombre as Name, sum(pd.VenCantidad) as Quantity, " + monto + " " +
               "from VentasConfirmados o " +
               "inner join VentasDetalleConfirmados pd on pd.VenSecuencia = o.VenSecuencia and trim(o.RepCodigo) = trim(pd.RepCodigo) " +
               "inner join Clientes c on c.CliID = o.CliID " +
               "inner join Productos p on p.ProID = pd.ProID " +
               "inner join CondicionesPago cp on cp.ConId = o.ConID " +
               "where trim(o.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus /*in (1,2,4)*/ <> 0 and cp.ConDiasVencimiento <> 0 " + where + " group by o.VenSecuencia and c.CliNombre, c.CliID " +
               "order by 1 asc", new string[] {});
            

            if (listVentasCreditoCabecera.Count > 0) {
                result.Add(new SubTitle() { Description = "VENTAS CREDITO" });
                result.AddRange(listVentasCreditoCabecera);

                ////////////////////////////////////VENTAS A CREDITO DETALLE//////////////////////////
                var listVentasCredito = SqliteManager.GetInstance().Query<ReportesNameQuantity>("select 1 as IsMoneyAmount, case when pd.VenindicadorOferta = 1 then p.ProDescripcion||' (Oferta)' else p.ProDescripcion end as Name, sum(pd.VenCantidad) as Quantity, " + monto + " " +
                    "from Ventas o " +
                    "inner join VentasDetalle pd on pd.VenSecuencia = o.VenSecuencia and trim(pd.RepCodigo) = trim(o.RepCodigo) " +
                    "inner join Productos p on p.ProID = pd.ProID " +
                    "inner join CondicionesPago cp on cp.ConID = o.ConID " +
                    "where trim(o.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus /*in (1,2,4)*/ <> 0 and cp.ConDiasVencimiento <> 0 and pd.venIndicadorOferta <> 1 " + where + " group by p.ProDescripcion, p.ProID, pd.VenindicadorOferta " +
                    "union " +
                    "select 1 as IsMoneyAmount, case when pd.VenindicadorOferta = 1 then p.ProDescripcion||' (Oferta)' else p.ProDescripcion end as Name, sum(pd.VenCantidad) as Quantity, " + monto + " " +
                    "from VentasConfirmados o " +
                    "inner join VentasDetalleConfirmados pd on pd.VenSecuencia = o.VenSecuencia and trim(o.RepCodigo) = trim(pd.RepCodigo) " +
                    "inner join Productos p on p.ProID = pd.ProID " +
                    "inner join CondicionesPago cp on cp.ConID = o.ConID " +
                    "where trim(o.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus /*in (1,2,4)*/ <> 0 and cp.ConDiasVencimiento <> 0 and pd.venIndicadorOferta <> 1 " + where + " group by p.ProDescripcion, p.ProID, pd.VenindicadorOferta " +
                    "order by 1 asc", new string[] {});

                result.Add(new SubTitle() { Description = "DETALLE DE VENTAS CREDITO" });
                result.AddRange(listVentasCredito);

                var TotalVentasCredito = SqliteManager.GetInstance().Query<ReportesNameQuantity>("Select 1 as IsMoneyAmount, Round(sum(resultado.Amount),2) as Amount from ( select  " + monto + " " +
                "from Ventas o " +
                "inner join VentasDetalle pd on pd.VenSecuencia = o.VenSecuencia and trim(pd.RepCodigo) = trim(o.RepCodigo) " +
                "inner join Productos p on p.ProID = pd.ProID " +
                "inner join CondicionesPago cp on cp.ConID = o.ConID " +
                "where trim(o.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus /*in (1,2,4)*/ <> 0 and cp.ConDiasVencimiento <> 0 and pd.venIndicadorOferta <> 1 " + where + "  " +
                "union " +
                "select  " + monto + " " +
                "from VentasConfirmados o " +
                "inner join VentasDetalleConfirmados pd on pd.VenSecuencia = o.VenSecuencia and trim(o.RepCodigo) = trim(pd.RepCodigo) " +
                "inner join Productos p on p.ProID = pd.ProID " +
                "inner join CondicionesPago cp on cp.ConID = o.ConID " +
                "where trim(o.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus /*in (1,2,4)*/ <> 0 and cp.ConDiasVencimiento <> 0 and pd.venIndicadorOferta <> 1  " + where + " ) as resultado ", 
                new string[] {});

                result.Add(new SubTitle() { Description = "Total Ventas Credito : " + TotalVentasCredito[0].AmountFormated });
            }



            ////////////////////////////////////VENTAS A CONTADO/////////////////////////
            var listVentasContadoCabecera = SqliteManager.GetInstance().Query<ReportesNameQuantity>("select 1 as IsMoneyAmount, o.VenSecuencia||' - '||c.CliNombre as Name, sum(pd.VenCantidad) as Quantity, " + monto + " " +
               "from Ventas o " +
               "inner join VentasDetalle pd on pd.VenSecuencia = o.VenSecuencia and trim(pd.RepCodigo) = trim(o.RepCodigo) " +
               "inner join Clientes c on c.CliID = o.CliID " +
               "inner join Productos p on p.ProID = pd.ProID " +
               "inner join CondicionesPago cp on cp.ConId = o.ConID " +
               "where trim(o.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus /*in (1,2,4)*/ <> 0 and cp.ConDiasVencimiento = 0 " + where + " group by o.VenSecuencia, c.CliNombre, c.CliID " +
               "union " +
               "select 1 as IsMoneyAmount, o.VenSecuencia||' - '||c.CliNombre as Name, sum(pd.VenCantidad) as Quantity, " + monto + " " +
               "from VentasConfirmados o " +
               "inner join VentasDetalleConfirmados pd on pd.VenSecuencia = o.VenSecuencia and trim(o.RepCodigo) = trim(pd.RepCodigo) " +
               "inner join Clientes c on c.CliID = o.CliID " +
               "inner join Productos p on p.ProID = pd.ProID " +
               "inner join CondicionesPago cp on cp.ConId = o.ConID " +
               "where trim(o.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus /*in (1,2,4)*/ <> 0 and cp.ConDiasVencimiento = 0 " + where + " group by o.VenSecuencia and c.CliNombre, c.CliID " +
               "order by 1 asc", new string[] {});

           

            if (listVentasContadoCabecera.Count > 0)
            {
                result.Add(new SubTitle() { Description = "VENTAS CONTADO" });
                result.AddRange(listVentasContadoCabecera);

                ////////////////////////////////////VENTAS A CONTADO DETALLE//////////////////////////
                var listVentasContado = SqliteManager.GetInstance().Query<ReportesNameQuantity>("select 1 as IsMoneyAmount, case when pd.VenindicadorOferta = 1 then p.ProDescripcion||' (Oferta)' else p.ProDescripcion end as Name, sum(pd.VenCantidad) as Quantity, " + monto + " " +
                    "from Ventas o " +
                    "inner join VentasDetalle pd on pd.VenSecuencia = o.VenSecuencia and trim(pd.RepCodigo) = trim(o.RepCodigo) " +
                    "inner join Productos p on p.ProID = pd.ProID " +
                    "inner join CondicionesPago cp on cp.ConID = o.ConID " +
                    "where trim(o.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus /*in (1,2,4)*/ <> 0 and cp.ConDiasVencimiento = 0 and pd.venIndicadorOferta <> 1 " + where + " group by p.ProDescripcion, p.ProID, pd.VenindicadorOferta " +
                    "union " +
                    "select 1 as IsMoneyAmount, case when pd.VenindicadorOferta = 1 then p.ProDescripcion||' (Oferta)' else p.ProDescripcion end as Name, sum(pd.VenCantidad) as Quantity, " + monto + " " +
                    "from VentasConfirmados o " +
                    "inner join VentasDetalleConfirmados pd on pd.VenSecuencia = o.VenSecuencia and trim(o.RepCodigo) = trim(pd.RepCodigo) " +
                    "inner join Productos p on p.ProID = pd.ProID " +
                    "inner join CondicionesPago cp on cp.ConID = o.ConID " +
                    "where trim(o.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus /*in (1,2,4)*/ <> 0 and cp.ConDiasVencimiento = 0 and pd.venIndicadorOferta <> 1 " + where + " group by p.ProDescripcion, p.ProID, pd.VenindicadorOferta " +
                    "order by 1 asc", new string[] { });

                result.Add(new SubTitle() { Description = "DETALLE DE VENTAS CONTADO" });
                result.AddRange(listVentasContado);

                var TotalVentasContado = SqliteManager.GetInstance().Query<ReportesNameQuantity>("Select 1 as IsMoneyAmount, Round(sum(resultado.Amount),2) as Amount from ( select " + monto + " " +
               "from Ventas o " +
               "inner join VentasDetalle pd on pd.VenSecuencia = o.VenSecuencia and trim(pd.RepCodigo) = trim(o.RepCodigo) " +
               "inner join Productos p on p.ProID = pd.ProID " +
               "inner join CondicionesPago cp on cp.ConID = o.ConID " +
               "where trim(o.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus /*in (1,2,4)*/ <> 0 and cp.ConDiasVencimiento = 0 and pd.venIndicadorOferta <> 1 " + where + " "+
               "union " +
               "select " + monto + " " +
               "from VentasConfirmados o " +
               "inner join VentasDetalleConfirmados pd on pd.VenSecuencia = o.VenSecuencia and trim(o.RepCodigo) = trim(pd.RepCodigo) " +
               "inner join Productos p on p.ProID = pd.ProID " +
               "inner join CondicionesPago cp on cp.ConID = o.ConID " +
               "where trim(o.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus /*in (1,2,4)*/ <> 0 and cp.ConDiasVencimiento = 0 and pd.venIndicadorOferta <> 1  " + where + " ) as resultado ", 
               new string[] { });

                result.Add(new SubTitle() { Description = "Total Ventas Contado : " + TotalVentasContado[0].AmountFormated });
            }


            ////////////////////////////////////VENTAS A ANULADAS//////////////////////////
            var listVentasAnuladas = SqliteManager.GetInstance().Query<ReportesNameQuantity>("select 1 as IsMoneyAmount, o.VenSecuencia||' - '||c.CliNombre as Name, sum(pd.VenCantidad) as Quantity, " + monto + " " +
                "from Ventas o " +
                "inner join VentasDetalle pd on pd.VenSecuencia = o.VenSecuencia and trim(pd.RepCodigo) = trim(o.RepCodigo) " +
                "inner join Clientes c on c.CliID = o.CliID " +
                "inner join Productos p on p.ProID = pd.ProID " +
                "where trim(o.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus = 0 and pd.venIndicadorOferta <> 1 " + where + " group by o.VenSecuencia, c.CliNombre, c.CliID " +
                "order by 1 asc", new string[] {});
            
            if (listVentasAnuladas.Count > 0) {
                result.Add(new SubTitle() { Description = "VENTAS ANULADAS" });
                result.AddRange(listVentasAnuladas);

                ////////////////////////////////////VENTAS A ANULADAS DETALLE//////////////////////////
                var listVentasAnuladasDetalle = SqliteManager.GetInstance().Query<ReportesNameQuantity>("select 1 as IsMoneyAmount, case when pd.VenindicadorOferta = 1 then p.ProDescripcion||' (Oferta)' else p.ProDescripcion end as Name, sum(pd.VenCantidad) as Quantity, " + monto + " " +
                    "from Ventas o " +
                    "inner join VentasDetalle pd on pd.VenSecuencia = o.VenSecuencia and trim(pd.RepCodigo) = trim(o.RepCodigo) " +
                    "inner join Productos p on p.ProID = pd.ProID " +
                    "where trim(o.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus = 0   and pd.venIndicadorOferta <> 1 " + where + " group by p.ProDescripcion, p.ProID, pd.VenindicadorOferta " +
                    "order by 1 asc", new string[] { });

                result.Add(new SubTitle() { Description = "DETALLE DE VENTAS ANULADAS" });
                result.AddRange(listVentasAnuladasDetalle);

                var TotalVentasAnuladas = SqliteManager.GetInstance().Query<ReportesNameQuantity>("Select Round(sum(resultado.Amount),2) as Amount, 1 as IsMoneyAmount from ( select " + monto + " " +
               "from Ventas o " +
               "inner join VentasDetalle pd on pd.VenSecuencia = o.VenSecuencia and trim(pd.RepCodigo) = trim(o.RepCodigo) " +
               "inner join Clientes c on c.CliID = o.CliID " +
               "inner join Productos p on p.ProID = pd.ProID " +
               "where trim(o.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus = 0 " + where + " ) as resultado ", 
               new string[] { });

                result.Add(new SubTitle() { Description = "Total Ventas Anuladas : " + TotalVentasAnuladas[0].AmountFormated });
            }
           
            return result;
        }

        public List<RowLinker> GetResumenRecibos(string desde = null, string hasta = null, bool futurista = false, string secCodigo = null, bool isfromanular = false)
        {
            string where = "";

            if(!string.IsNullOrWhiteSpace(desde) && !string.IsNullOrWhiteSpace(hasta))
            {
                where = " and CAST(strftime('%Y%m%d',r.RecFecha) AS INTEGER) between cast(strftime('%Y%m%d', '"+desde+ "') as integer) and cast(strftime('%Y%m%d', '"+hasta+"') as integer) ";
            }

            if (!string.IsNullOrWhiteSpace(secCodigo))
            {
                where += " and r.SecCodigo = '"+secCodigo+"' ";
            }

            where += isfromanular ? "and RecEstatus = 0 " : "and RecEstatus != 0 ";

            var futuristaWhere = "";
            var futuristaWhereConfirmados = "";
            string[] select = new string[3] { "", "", "" };
                
                //{ , ,
                            //    };

            if (futurista)
            {
                futuristaWhereConfirmados = " and r.RecSecuencia in (select RecSecuencia from RecibosFormaPagoConfirmados where RefIndicadorDiferido = 1 and RecSecuencia = r.RecSecuencia and RepCodigo = r.RepCodigo) ";
                futuristaWhere = " and r.RecSecuencia in (select RecSecuencia from RecibosFormaPago where RefIndicadorDiferido = 1 and RecSecuencia = r.RecSecuencia and RepCodigo = r.RepCodigo) ";
                //select[0] =   " strftime('%Y-%m-%d',d.DepFecha) as DepFecha, ";
                //select[1] =   "inner join Depositos d ON d.DepSecuencia = r.DepSecuencia ";
                //select[2] =   "AND d.DepFecha in (SELECT DepFecha FROM Depositos WHERE RepCodigo = r.RepCodigo)";
                select[0] = "";
                select[1] = "";
                select[2] = "";
            }



            var result = new List<RowLinker>()
            {
                new RecibosMontoResumen(){RecSecuencia = "RECIBOS", CliNombre = "CLIEN", CliCodigo = "CLIEN", RecDescuento = "DESC.", RecMonto = "MONTO", DepFecha = "Fecha dep." , Bold = true }
            };

            var list = SqliteManager.GetInstance().Query<RecibosMontoResumen>($"select {select[0]} r.RecSecuencia as RecSecuencia, c.CliNombre as CliNombre, c.CliCodigo as CliCodigo, " +
                "r.RecMontoDescuento as RecDescuento, r.RecTotal RecMonto, r.RecMontoNcr as RecMontoNcr from Recibos r " +
                "inner join Clientes c on c.CliID = r.CliID " + select[1] +
                "where trim(r.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' " + select[2] + where + futuristaWhere +
                " union " +
                $"select {select[0]} r.RecSecuencia as RecSecuencia, c.CliNombre as CliNombre, c.CliCodigo as CliCodigo,  r.RecMontoDescuento as RecDescuento, r.RecTotal as RecMonto, r.RecMontoNcr as RecMontoNcr from RecibosConfirmados r " +
                "inner join Clientes c on c.CliID = r.CliID " + select[1] +
                "where trim(r.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' " + where + " "+ futuristaWhereConfirmados +
                "order by RecSecuencia asc ", new string[] { });

            result.AddRange(list);
            result.Add(new RowLinker());

            string formaPagoName = "case when rf.ForID = 2 and ifnull(rf.RefIndicadorDiferido,0) = 0 then 'Cheque:' " +
                "when rf.ForID = 2 and ifnull(rf.RefIndicadorDiferido, 0) = 1 then 'Cheque futurista:' " +
                "when rf.ForID = 1 then 'Efectivo:' " +
                "when rf.ForID = 5 then 'Retencion:' " +
                "when rf.ForID = 4 then 'Transferencia:' " +
                "when rf.ForID = 6 then 'Tarjeta credito:' " +
                "else '' end ";

            var rfWhere = "";

            if (futurista)
            {
                rfWhere = " and rf.RefIndicadorDiferido = 1 ";
            }

            result.AddRange(SqliteManager.GetInstance().Query<Inventarios>("select 1 as FormatForMoney, rf.ForID as ForID, sum(rf.RefValor) as invCantidad, " + formaPagoName + " as ProDescripcion " +
                "from RecibosFormaPago rf " +
                "where trim(rf.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' "+ rfWhere + " and rf.RecSecuencia in (select RecSecuencia from Recibos r where trim(RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and RecSecuencia = rf.RecSecuencia "+where+")" +
                "group by rf.ForID, ifnull(rf.RefIndicadorDiferido, 0) " +
                "union " +
                "select 1 as FormatForMoney, rf.ForID as ForID, sum(rf.RefValor) as invCantidad, " + formaPagoName+ " as ProDescripcion from RecibosFormaPagoConfirmados rf " +
                "where trim(rf.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' "+ rfWhere + " and rf.RecSecuencia in (select RecSecuencia from RecibosConfirmados r where trim(RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and RecSecuencia = rf.RecSecuencia "+where+") group by rf.ForID, ifnull(rf.RefIndicadorDiferido,0) " +
                "order by rf.ForID ", new string[] { }));

            result.Add(new RowLinker());

            result.Add(new Inventarios() { ProDescripcion = "Cantidad: ", invCantidad = list.Count });

            result.Add(new Inventarios() { ProDescripcion = "Total desc: ", invCantidad = list.Sum(x => double.Parse(x.RecDescuento)), FormatForMoney = true });

            result.Add(new Inventarios() { ProDescripcion = "Monto total: ", invCantidad = list.Sum(x => double.Parse(x.RecMonto)), FormatForMoney = true });  

            return result;
        }

        public List<Inventarios> GetResumenVisitas(int numeroSemana, int diaNumero, string fechadesde= null, string fechahasta= null, string secCodigo = null, bool itbis = false)
        {
            int CicloSemana = 4;

            int parCicloSemana = myParametro.GetParCiclosSemanas();
            if (parCicloSemana > 0)
            {
                CicloSemana = parCicloSemana;
            }

            int weekNumber = numeroSemana;
            int dayNumber = diaNumero;

            weekNumber = weekNumber % CicloSemana;

            if (weekNumber == 0)
            {
                weekNumber = CicloSemana;
            }

            if (dayNumber == -1)
            {
                dayNumber = 6;
            }

            char[] diasSemana = new char[] { '_', '_', '_', '_', '_', '_', '_' };

            diasSemana[dayNumber] = '1';
            string semanaValues = new string(diasSemana);
            string newWhere = " AND R.RutSemana" + weekNumber.ToString() + " like '" + semanaValues + "' AND R.RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' ";

            var list = SqliteManager.GetInstance().Query<RutaVisitas>("select c.CliID from RutaVisitas r " +
                "inner join Clientes c on c.CliID = r.CliID where trim(r.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' " + newWhere + " " +
                "union " +
                "select c.CliID from RutaVisitasFecha r " +
                "inner join Clientes c on c.CliID = r.CliID where trim(r.RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and r.RutFecha like '" + fechadesde + "%' ",
                new string[] {});


            var whereSector = "";

            if (!string.IsNullOrWhiteSpace(secCodigo) && myParametro.GetParSectores() > 2)
            {
                whereSector = " and VisSecuencia in (select distinct VisSecuencia from VisitasSectores where RepCodigo = v.RepCodigo and VisSecuencia = v.VisSecuencia and SecCodigo = '"+secCodigo+"') ";
            }

            var resumen = SqliteManager.GetInstance().Query<Visitas>("select CliID, VisIndicadorFueraRuta, VisTipoVisita, VisIndicadorFueraOrden from Visitas v " +
                "where trim(RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and VisFechaEntrada like '" + fechadesde + "%' "+whereSector+" group by CliID",
                new string[] {});

            var ventas = SqliteManager.GetInstance().Query<Ventas>("select distinct CliID from Pedidos " +
                "where trim(RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and PedFecha like '"+ fechadesde + "%' and PedEstatus /*in (1,2,4)*/ <> 0 " +
                "union " +
                "select distinct CliID from PedidosConfirmados " +
                "where trim(RepCodigo) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' and PedFecha like '"+ fechadesde + "%' and PedEstatus /*in (1,2,4)*/ <> 0 ", 
                new string[] {});

            int visitados = 0;
            int cantidadVentas = 0;

            foreach (var ruta in list)
            {
                var vis = resumen.Where(x => x.CliID == ruta.CliID).FirstOrDefault();

                if (vis != null)
                {
                    visitados++;
                }

                var ven = ventas.Where(x => x.CliID == ruta.CliID).FirstOrDefault();

                if(ven != null)
                {
                    cantidadVentas++;
                }
            }

            int aVisitar = list.Count;
            int noVisitados = 0;

            if (aVisitar > visitados)
            {
                noVisitados = aVisitar - visitados;
            }

            var result = new List<Inventarios>();

            var efectividad = cantidadVentas > 0 ? (int)((visitados / cantidadVentas) * 100.0) : 0;

            result.Add(new Inventarios() { ProDescripcion = "No. de negocios a visitar: ", invCantidad = aVisitar });
            result.Add(new Inventarios() { ProDescripcion = "No. de negocios visitados: ", invCantidad = visitados });
            result.Add(new Inventarios() { ProDescripcion = "No. de negocios no visitados: ", invCantidad = noVisitados });
            //result.Add(new Inventarios() { ProDescripcion = "No. negocios visitados fuera de ruta: ", invCantidad = resumen.Where(x => x.VisIndicadorFueraRuta == 1).Count() });
            result.Add(new Inventarios() { ProDescripcion = "Visitas presenciales: ", invCantidad = resumen.Where(x => x.VisTipoVisita == 1).Count() });
            result.Add(new Inventarios() { ProDescripcion = "Visitas virtuales: ", invCantidad = resumen.Where(x => x.VisTipoVisita == 2).Count() });
            result.Add(new Inventarios() { ProDescripcion = "Visitas fuera de ruta: ", invCantidad = resumen.Where(x => x.VisIndicadorFueraRuta == 1).Count() });
            result.Add(new Inventarios() { ProDescripcion = "Visitas fuera de orden: ", invCantidad = resumen.Where(x => x.VisIndicadorFueraOrden == 1).Count() });
            result.Add(new Inventarios() { ProDescripcion = "% Efectividad: ", invCantidad = efectividad });


            ///Con Parametro
            if (Arguments.CurrentUser.RepCargo.ToUpper() ==  "PREVENDEDOR")
            {

                string where = "";

                if (!string.IsNullOrWhiteSpace(fechadesde) && !string.IsNullOrWhiteSpace(fechahasta))
                {
                    where = " and cast(strftime('%Y%m%d',o.Pedfecha) as integer) between cast(strftime('%Y%m%d', '" + fechadesde + "') as integer) and cast(strftime('%Y%m%d', '" + fechahasta + "') as integer) ";
                }

                if (!string.IsNullOrWhiteSpace(secCodigo))
                {
                    where += " and o.SecCodigo = '" + secCodigo + "' ";
                }

                string monto;

                if (itbis)
                {
                    monto = "SUM(((cast(pd.PedPrecio as float) - cast(pd.PedDescuento as float)) * (1 + cast(pd.PedItbis as float)/100)) * (cast(pd.PedCantidad as float) + ifnull((cast(ifnull(pd.PedCantidadDetalle, 0) as float) / cast(ifnull(p.ProUnidades, 1) as float)),0))) as Amount,  cast(ifnull(pd.PedDescuento,0)  as float) as DiscountAmount ";
                }
                else
                {
                    monto = "SUM(((cast(pd.PedPrecio as float) - cast(pd.PedDescuento as float)) ) * (cast(pd.PedCantidad as float) + ifnull((cast(ifnull(pd.PedCantidadDetalle, 0) as float) / cast(ifnull(p.ProUnidades, 1) as float)),0)  ) )  as Amount, cast(ifnull(pd.PedDescuento,0) as float) as DiscountAmount ";
                }

                var Cantpedidos = SqliteManager.GetInstance().Query<Pedidos>("select distinct PedSecuencia from Pedidos o " +
                    "where trim(RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and PedEstatus  <> 0 " + where + " " +
                    "union " +
                    "select distinct PedSecuencia from PedidosConfirmados o " +
                    "where trim(RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and PedEstatus  <> 0 " + where + " ",
                    new string[] { });

                var TotalMontoPedidos = SqliteManager.GetInstance().Query<ReportesNameQuantity>("select 1 as IsMoneyAmount, o.PedSecuencia||' - '||c.CliNombre as Name, 1 as IsMoneyAmount, sum(pd.PedCantidad) as Quantity, " + monto + " " +
                    "from Pedidos o " +
                    "inner join PedidosDetalle pd on pd.PedSecuencia = o.PedSecuencia and trim(pd.RepCodigo) = trim(o.RepCodigo) " +
                    "inner join Clientes c on c.CliID = o.CliID " +
                    "inner join Productos p on p.ProID = pd.ProID " +
                    "where trim(o.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and o.PedEstatus  <> 0 " + where + " group by o.PedSecuencia, c.CliNombre, c.CliID " +
                    "union " +
                    "select 1 as IsMoneyAmount, o.PedSecuencia||' - '||c.CliNombre as Name, 1 as IsMoneyAmount, sum(pd.PedCantidad) as Quantity, " + monto + " " +
                    "from PedidosConfirmados o " +
                    "inner join PedidosDetalleConfirmados pd on pd.PedSecuencia = o.PedSecuencia and trim(o.RepCodigo) = trim(pd.RepCodigo) " +
                    "inner join Clientes c on c.CliID = o.CliID " +
                    "inner join Productos p on p.ProID = pd.ProID " +
                    "where trim(o.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and o.PedEstatus  <> 0 " + where + " group by o.PedSecuencia and c.CliNombre, c.CliID " +
                    "order by 1 asc", new string[] { });


                result.Add(new Inventarios() { ProDescripcion = "Cantidad total de Pedidos: ", invCantidad = Cantpedidos.Count() });
                if (TotalMontoPedidos.Count > 0 && !string.IsNullOrWhiteSpace(TotalMontoPedidos[0].Amount)) 
                { result.Add(new Inventarios() { ProDescripcion = "Monto total de Pedidos Sin ITEBIS: ", invCantidad = TotalMontoPedidos.Sum(x => double.Parse(x.Amount)), FormatForMoney = true }); }
                else { result.Add(new Inventarios() { ProDescripcion = "Monto total de Pedidos Sin ITEBIS: ", invCantidad = 0 }); }

                
            }

            if (Arguments.CurrentUser.RepCargo.ToUpper() == "REPARTIDOR")
            {
                string where = "";

                if (!string.IsNullOrWhiteSpace(fechadesde) && !string.IsNullOrWhiteSpace(fechahasta))
                {
                    where = " and cast(strftime('%Y%m%d',o.VenFecha) as integer) between cast(strftime('%Y%m%d', '" + fechadesde + "') as integer) and cast(strftime('%Y%m%d', '" + fechahasta + "') as integer) ";
                }

                string whererecibos = "";

                if (!string.IsNullOrWhiteSpace(fechadesde) && !string.IsNullOrWhiteSpace(fechahasta))
                {
                    whererecibos = " and CAST(strftime('%Y%m%d',r.RecFecha) AS INTEGER) between cast(strftime('%Y%m%d', '" + fechadesde + "') as integer) and cast(strftime('%Y%m%d', '" + fechahasta + "') as integer) ";
                }

                string monto;

                monto = itbis ? "sum(((cast(pd.VenPrecio as float) + cast(pd.VenSelectivo as float) + cast(pd.VenAdValorem as float) - cast(pd.VenDescuento as float)) * (cast(pd.VenCantidad as float) + (cast(pd.VenCantidadDetalle as float) / cast(p.ProUnidades as float)))) + pd.venTotalitbis) as Amount "
                : "SUM(((cast(pd.VenPrecio as float) - cast(pd.VenDescuento as float)) ) * (cast(pd.VenCantidad as float) + (cast(ifnull(pd.VenCantidadDetalle, 0) as float) / cast(ifnull(p.ProUnidades, 1) as float))   ))  as Amount ";


                var listVentasContadoCabecera = SqliteManager.GetInstance().Query<ReportesNameQuantity>("select o.VenSecuencia as Name " +
                   "from Ventas o " +
                   "inner join VentasDetalle pd on pd.VenSecuencia = o.VenSecuencia and trim(pd.RepCodigo) = trim(o.RepCodigo) " +
                   "inner join Productos p on p.ProID = pd.ProID " +
                   "inner join CondicionesPago cp on cp.ConId = o.ConID " +
                   "where trim(o.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus /*in (1,2,4)*/ <> 0 and cp.ConDiasVencimiento = 0 " + where + " " +
                   "union " +
                   "select o.VenSecuencia as Name " +
                   "from VentasConfirmados o " +
                   "inner join VentasDetalleConfirmados pd on pd.VenSecuencia = o.VenSecuencia and trim(o.RepCodigo) = trim(pd.RepCodigo) " +
                   "inner join Productos p on p.ProID = pd.ProID " +
                   "inner join CondicionesPago cp on cp.ConId = o.ConID " +
                   "where trim(o.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus /*in (1,2,4)*/ <> 0 and cp.ConDiasVencimiento = 0 " + where + " " +
                   "order by 1 asc", new string[] { });

                var TotalVentasContado = SqliteManager.GetInstance().Query<ReportesNameQuantity>("Select Round(sum(resultado.Amount),2) as Amount from ( select " + monto + " " +
                   "from Ventas o " +
                   "inner join VentasDetalle pd on pd.VenSecuencia = o.VenSecuencia and trim(pd.RepCodigo) = trim(o.RepCodigo) " +
                   "inner join Productos p on p.ProID = pd.ProID " +
                   "inner join CondicionesPago cp on cp.ConID = o.ConID " +
                   "where trim(o.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus /*in (1,2,4)*/ <> 0 and cp.ConDiasVencimiento = 0 and pd.venIndicadorOferta <> 1 " + where + " " +
                   "union " +
                   "select " + monto + " " +
                   "from VentasConfirmados o " +
                   "inner join VentasDetalleConfirmados pd on pd.VenSecuencia = o.VenSecuencia and trim(o.RepCodigo) = trim(pd.RepCodigo) " +
                   "inner join Productos p on p.ProID = pd.ProID " +
                   "inner join CondicionesPago cp on cp.ConID = o.ConID " +
                   "where trim(o.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus /*in (1,2,4)*/ <> 0 and cp.ConDiasVencimiento = 0 and pd.venIndicadorOferta <> 1  " + where + " ) as resultado ",
                   new string[] { });


                result.Add(new Inventarios() { ProDescripcion = "Cantidad total de Fact. a Contado: ", invCantidad = listVentasContadoCabecera.Count() });
                if (TotalVentasContado.Count > 0 && !string.IsNullOrWhiteSpace(TotalVentasContado[0].Amount) ) { result.Add(new Inventarios() { ProDescripcion = "Monto total de Fact. a Contado Sin ITEBIS: ", invCantidad = TotalVentasContado.Sum(x => double.Parse(x.Amount)), FormatForMoney = true }); }
                else { result.Add(new Inventarios() { ProDescripcion = "Monto total de Fact. a Contado Sin ITEBIS: ", invCantidad = 0 });}
           
            


                var listVentasCreditoCabecera = SqliteManager.GetInstance().Query<ReportesNameQuantity>("select o.VenSecuencia as Name " +
                   "from Ventas o " +
                   "inner join VentasDetalle pd on pd.VenSecuencia = o.VenSecuencia and trim(pd.RepCodigo) = trim(o.RepCodigo) " +
                   "inner join Clientes c on c.CliID = o.CliID " +
                   "inner join Productos p on p.ProID = pd.ProID " +
                   "inner join CondicionesPago cp on cp.ConId = o.ConID " +
                   "where trim(o.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus  <> 0 /*(1,2,4)*/ and cp.ConDiasVencimiento <> 0 " + where + " group by o.VenSecuencia, c.CliNombre, c.CliID " +
                   "union " +
                   "select o.VenSecuencia as Name " +
                   "from VentasConfirmados o " +
                   "inner join VentasDetalleConfirmados pd on pd.VenSecuencia = o.VenSecuencia and trim(o.RepCodigo) = trim(pd.RepCodigo) " +
                   "inner join Clientes c on c.CliID = o.CliID " +
                   "inner join Productos p on p.ProID = pd.ProID " +
                   "inner join CondicionesPago cp on cp.ConId = o.ConID " +
                   "where trim(o.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus /*in (1,2,4)*/ <> 0 and cp.ConDiasVencimiento <> 0 " + where + " group by o.VenSecuencia and c.CliNombre, c.CliID " +
                   "order by 1 asc", new string[] { });

                var TotalVentasCredito = SqliteManager.GetInstance().Query<ReportesNameQuantity>("Select  Round(sum(resultado.Amount),2) as Amount from ( select  " + monto + " " +
                "from Ventas o " +
                "inner join VentasDetalle pd on pd.VenSecuencia = o.VenSecuencia and trim(pd.RepCodigo) = trim(o.RepCodigo) " +
                "inner join Productos p on p.ProID = pd.ProID " +
                "inner join CondicionesPago cp on cp.ConID = o.ConID " +
                "where trim(o.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus /*in (1,2,4)*/ <> 0 and cp.ConDiasVencimiento <> 0 and pd.venIndicadorOferta <> 1 " + where + "  " +
                "union " +
                "select  " + monto + " " +
                "from VentasConfirmados o " +
                "inner join VentasDetalleConfirmados pd on pd.VenSecuencia = o.VenSecuencia and trim(o.RepCodigo) = trim(pd.RepCodigo) " +
                "inner join Productos p on p.ProID = pd.ProID " +
                "inner join CondicionesPago cp on cp.ConID = o.ConID " +
                "where trim(o.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and o.VenEstatus /*in (1,2,4)*/ <> 0 and cp.ConDiasVencimiento <> 0 and pd.venIndicadorOferta <> 1  " + where + " ) as resultado ",
                new string[] { });

                result.Add(new Inventarios() { ProDescripcion = "Cantidad total de Fact. a Credito: ", invCantidad = listVentasCreditoCabecera.Count() });
                if (TotalVentasCredito.Count > 0 && !string.IsNullOrWhiteSpace(TotalVentasCredito[0].Amount)) { result.Add(new Inventarios() { ProDescripcion = "Monto total de Fact. a Credito Sin ITEBIS: ", invCantidad = TotalVentasCredito.Sum(x => double.Parse(x.Amount)), FormatForMoney = true }); }
                else { result.Add(new Inventarios() { ProDescripcion = "Monto total de Fact. a Credito Sin ITEBIS: ", invCantidad = 0 }); }

                var listRecibosCreditoCabecera = SqliteManager.GetInstance().Query<RecibosMontoResumen>("select RecSecuencia from recibos r " +
                   "where trim(r.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and r.RecEstatus  <> 0  and r.Rectipo = 2 " + whererecibos +" " +
                   "union " +
                   "select RecSecuencia from RecibosConfirmados r " +
                   "where trim(r.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and r.RecEstatus  <> 0  and r.Rectipo = 2 " + whererecibos + " " +
                   "order by 1 asc", new string[] { });

                var TotalRecibosCredito = SqliteManager.GetInstance().Query<RecibosMontoResumen>(" Select Round(sum(resultado.Monto),2) as RecMonto from ( select Sum(RecValor) as Monto from recibosaplicacion ra " +
                   "Inner join recibos r on ra.RepCodigo=r.Repcodigo and ra.RecTipo= r.Rectipo and ra.Recsecuencia=r.Recsecuencia "+
                   "where trim(ra.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and r.RecEstatus  <> 0  and ra.Rectipo = 2 " + whererecibos + " " +
                   "union " +
                   "select Sum(RecValor) as Monto  from recibosaplicacionConfirmados ra " +
                   "Inner join recibosConfirmados r on ra.RepCodigo=r.Repcodigo and ra.RecTipo= r.Rectipo and ra.Recsecuencia=r.Recsecuencia " +
                   "where trim(ra.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and r.RecEstatus  <> 0  and ra.Rectipo = 2 " + whererecibos + " ) as resultado "
                   , new string[] { });


                result.Add(new Inventarios() { ProDescripcion = "Cantidad Total de Cobros de Fact. a Credito: ", invCantidad = listRecibosCreditoCabecera.Count() });
                if (TotalRecibosCredito.Count > 0 && !string.IsNullOrWhiteSpace(TotalRecibosCredito[0].RecMonto)) { result.Add(new Inventarios() { ProDescripcion = "Monto Total de Cobros de Fact. a Credito: ", invCantidad = TotalRecibosCredito.Sum(x => double.Parse(x.RecMonto)), FormatForMoney = true }); }
                else { result.Add(new Inventarios() { ProDescripcion = "Monto Total de Cobros de Fact. a Credito: ", invCantidad = 0 }); }
                

            }

            return result;
        }


        public List<ResumenLineaProductos> GetResumenPreventaporLineadeProductos(string desde, string hasta, string secCodigo = null)
        {
            string where="";

            if (!string.IsNullOrWhiteSpace(desde) && !string.IsNullOrWhiteSpace(hasta))
            {
                where = "Where cast(strftime('%Y%m%d',Pt.PedFecha) as integer) between cast(strftime('%Y%m%d', '" + desde + "') as integer) and cast(strftime('%Y%m%d', '" + hasta + "') as integer) ";
            }

            if (!string.IsNullOrWhiteSpace(secCodigo))
            {
                where += " and Pt.SecCodigo = '"+secCodigo+"' ";
            }

            return SqliteManager.GetInstance().Query<ResumenLineaProductos>("Select LinDescripcion, ifnull(sum((pd.PedCantidad + case p.ProUnidades when 0 then 0 else (ifnull(pd.PedCantidadDetalle, 0) / ifnull(p.ProUnidades, 1)) end) * (pd.PedPrecio + ((pd.PedPrecio - pd.peddescuento) * PedItbis / 100) - peddescuento)), 0) as Monto " +
                " from pedidosdetalle pd " +
                " inner join productos p on pd.Proid = p.Proid " +
                " Inner Join Lineas l on P.LinID = L.LinID" +
                " Inner Join Pedidos Pt on Pt.PedSecuencia = pd.PedSecuencia " +
                " "+ where + "" +
                " Group by LinDescripcion " +
                " Union All " +
                " Select LinDescripcion, ifnull(sum((pd.PedCantidad + case p.ProUnidades when 0 then 0 else (ifnull(pd.PedCantidadDetalle, 0) / ifnull(p.ProUnidades, 1)) end) * (pd.PedPrecio + ((pd.PedPrecio - pd.peddescuento) * PedItbis / 100) - peddescuento)), 0) as Monto " +
                " from pedidosdetalleConfirmados pd " +
                " inner join productos p on pd.Proid = p.Proid " +
                " Inner Join Lineas l on P.LinID = L.LinID" +
                " Inner Join Pedidos Pt on Pt.PedSecuencia = pd.PedSecuencia " +
                " " + where + "" +
                " Group by LinDescripcion ", new string[] { });
        }

        public List<SaldoPorAntiguedad> GetSaldoPorAntiguedadByCliente(int Cliid, string monCodigo = null)
        {
            string sql = "";
            float diferenciaHoraria = Functions.GetDiferenciaHorariaSqlite();

            var whereMonCodigo = "";

            if (!string.IsNullOrWhiteSpace(monCodigo))
            {
                whereMonCodigo = " and trim(upper(MonCodigo)) = trim(upper('" + monCodigo + "')) ";
            }

            sql = "SELECT result.Desde AS 'Desde', result.Hasta AS 'Hasta', SUM(result.Balance) AS 'Balance' FROM (" +
                       "SELECT '0' AS Desde, '30' AS Hasta, IFNULL( CAST(SUM(CXCBalance) as numeric),0) AS Balance " +
                       "FROM CuentasxCobrar " +
                       "inner join TiposTransaccionesCxc ttc on trim(ttc.ttcReferencia) = CxcSigla and ttcOrigen = 1 " +
                       "WHERE (JULIANDAY(SUBSTR(DATETIME('now','" + diferenciaHoraria + " hours'),1,10)) - JULIANDAY(SUBSTR(CxcFecha,1,10))) BETWEEN 0 AND 30 AND CliID = " + Cliid + " " + whereMonCodigo +
                       "UNION ALL " +

                       "SELECT '0' AS Desde, '30' AS Hasta, IFNULL(CAST(RecMontoEfectivo + RecMontoCheque + RecMontoChequef + RecMontoTransferencia + RecMontoDescuento - RecMontoSobrante as String),0) AS Balance " +
                       "FROM Recibos " +
                       "WHERE (JULIANDAY(SUBSTR(DATETIME('now','" + diferenciaHoraria + " hours'),1,10)) - JULIANDAY(SUBSTR(RecFecha,1,10))) BETWEEN 0 AND 30 AND CliID = " + Cliid + " " + whereMonCodigo +
                       "UNION ALL " +

                       "SELECT '31' AS Desde, '60' AS Hasta, IFNULL( CAST(SUM(CXCBalance) as numeric),0) AS Balance " +
                       "FROM CuentasxCobrar " +
                       "inner join TiposTransaccionesCxc ttc on trim(ttc.ttcReferencia) = CxcSigla and ttcOrigen = 1 " +
                       "WHERE (JULIANDAY(SUBSTR(DATETIME('now','" + diferenciaHoraria + " hours'),1,10)) - JULIANDAY(SUBSTR(CxcFecha,1,10))) BETWEEN 31 AND 60 AND CliID = " + Cliid + " " + whereMonCodigo +
                       "UNION ALL " +

                       "SELECT '31' AS Desde, '60' AS Hasta, IFNULL(CAST(RecMontoEfectivo + RecMontoCheque + RecMontoChequef + RecMontoTransferencia + RecMontoDescuento - RecMontoSobrante as String),0) AS Balance " +
                       "FROM Recibos " +
                       "WHERE (JULIANDAY(SUBSTR(DATETIME('now','" + diferenciaHoraria + " hours'),1,10)) - JULIANDAY(SUBSTR(RecFecha,1,10))) BETWEEN 31 AND 60 AND CliID = " + Cliid + " " + whereMonCodigo +
                       "UNION ALL " +

                       "SELECT '61' as Desde, '90' as Hasta, IFNULL( CAST(SUM(CXCBalance) as numeric),0) as Balance " +
                       "FROM CuentasxCobrar " +
                       "inner join TiposTransaccionesCxc ttc on trim(ttc.ttcReferencia) = CxcSigla and ttcOrigen = 1 " +
                       "WHERE (JULIANDAY(SUBSTR(DATETIME('now','" + diferenciaHoraria + " hours'),1,10)) - JULIANDAY(SUBSTR(CxcFecha,1,10))) BETWEEN 61 AND 90 AND CliID = " + Cliid + " " + whereMonCodigo +
                       "UNION ALL " +

                       "SELECT '61' AS Desde, '90' AS Hasta, IFNULL(CAST(RecMontoEfectivo + RecMontoCheque + RecMontoChequef + RecMontoTransferencia + RecMontoDescuento - RecMontoSobrante as String),0) AS Balance " +
                       "FROM Recibos " +
                       "WHERE (JULIANDAY(SUBSTR(DATETIME('now','" + diferenciaHoraria + " hours'),1,10)) - JULIANDAY(SUBSTR(RecFecha,1,10))) BETWEEN 61 AND 90 AND CliID = " + Cliid + " " + whereMonCodigo +
                       "UNION ALL " +

                       "SELECT '91' as Desde, '' as Hasta, IFNULL( CAST(SUM(CXCBalance) as numeric),0) as Balance " +
                       "FROM CuentasxCobrar " +
                       "inner join TiposTransaccionesCxc ttc on trim(ttc.ttcReferencia) = CxcSigla and ttcOrigen = 1 " +
                       "WHERE (JULIANDAY(SUBSTR(DATETIME('now','" + diferenciaHoraria + " hours'),1,10)) - JULIANDAY(SUBSTR(CxcFecha,1,10))) >= 91 AND CliID = " + Cliid + " " + whereMonCodigo +
                       "UNION ALL " +

                       "SELECT '91' AS Desde, '' AS Hasta, IFNULL(CAST(RecMontoEfectivo + RecMontoCheque + RecMontoChequef + RecMontoTransferencia + RecMontoDescuento - RecMontoSobrante as String),0) AS Balance " +
                       "FROM Recibos " +
                       "WHERE (JULIANDAY(SUBSTR(DATETIME('now','" + diferenciaHoraria + " hours'),1,10)) - JULIANDAY(SUBSTR(RecFecha,1,10))) >= 91 AND CliID = " + Cliid + whereMonCodigo +
                       ") result GROUP BY result.Desde, result.Hasta";

            return SqliteManager.GetInstance().Query<SaldoPorAntiguedad>(sql, new string[] { });
        }

        public List<SaldoPorAntiguedad> GetSaldoPorAntiguedadByClienteV2(int Cliid, string monCodigo = null)
        {
            string sql = "";
            float diferenciaHoraria = Functions.GetDiferenciaHorariaSqlite();

            var whereMonCodigo = "";

            if (!string.IsNullOrWhiteSpace(monCodigo))
            {
                whereMonCodigo = " and trim(upper(c.MonCodigo)) = trim(upper('" + monCodigo + "')) ";
            }

            sql = "SELECT result.Desde AS 'Desde', result.Hasta AS 'Hasta', SUM(result.Balance) AS 'Balance' FROM (" +
                       "SELECT '0' AS Desde, '30' AS Hasta, IFNULL( CAST(SUM(CXCBalance) as numeric),0) - " +
                       "IFNULL((Select sum(RecValor + RecDescuento+ IFNULL(a.RecDescuentoDesmonte,0)) from RecibosAplicacion  a where a.Cliid= c.CliID " +
                       "and a.CxcReferencia  in (select cc.CxcReferencia from CuentasxCobrar cc where cc.cliid =c.cliid " +
                       "AND (JULIANDAY(SUBSTR(DATETIME(DATETIME('NOW', 'localtime'),'" + diferenciaHoraria + " hours'),1,10)) - JULIANDAY(SUBSTR(CxcFecha,1,10))) BETWEEN 0 AND 30) " +
                       "and a.recsecuencia in (select r.recSecuencia from recibos r where repcodigo = A.repcodigo and RecEstatus <>0)),0) as Balance " +
                       "FROM CuentasxCobrar c " +
                       "inner join TiposTransaccionesCxc ttc on trim(ttc.ttcReferencia) = CxcSigla and ttcOrigen = 1 " +
                       "WHERE (JULIANDAY(SUBSTR(DATETIME(DATETIME('NOW', 'localtime'),'" + diferenciaHoraria + " hours'),1,10)) - JULIANDAY(SUBSTR(CxcFecha,1,10))) BETWEEN 0 AND 30 AND CliID = " + Cliid + " " + whereMonCodigo +
                       "UNION ALL " +

                       "SELECT '31' AS Desde, '60' AS Hasta, IFNULL( CAST(SUM(CXCBalance) as numeric),0) - " +
                       "IFNULL((Select sum(RecValor + RecDescuento+ IFNULL(a.RecDescuentoDesmonte,0)) from RecibosAplicacion  a where a.Cliid= c.CliID " +
                       "and a.CxcReferencia  in (select cc.CxcReferencia from CuentasxCobrar cc where cc.cliid =c.cliid " +
                       "AND (JULIANDAY(SUBSTR(DATETIME(DATETIME('NOW', 'localtime'),'" + diferenciaHoraria + " hours'),1,10)) - JULIANDAY(SUBSTR(CxcFecha,1,10))) BETWEEN 31 AND 60) " +
                       "and a.recsecuencia in (select r.recSecuencia from recibos r where repcodigo = A.repcodigo and RecEstatus <>0)),0) as Balance " +
                       "FROM CuentasxCobrar c " +
                       "inner join TiposTransaccionesCxc ttc on trim(ttc.ttcReferencia) = CxcSigla and ttcOrigen = 1 " +
                       "WHERE (JULIANDAY(SUBSTR(DATETIME(DATETIME('NOW', 'localtime'),'" + diferenciaHoraria + " hours'),1,10)) - JULIANDAY(SUBSTR(CxcFecha,1,10))) BETWEEN 31 AND 60 AND CliID = " + Cliid + " " + whereMonCodigo +
                       "UNION ALL " +

                       "SELECT '61' as Desde, '90' as Hasta, IFNULL( CAST(SUM(CXCBalance) as numeric),0) - " +
                       "IFNULL((Select sum(RecValor + RecDescuento+ IFNULL(a.RecDescuentoDesmonte,0)) from RecibosAplicacion  a where a.Cliid= c.CliID " +
                       "and a.CxcReferencia  in (select cc.CxcReferencia from CuentasxCobrar cc where cc.cliid =c.cliid " +
                       "AND (JULIANDAY(SUBSTR(DATETIME(DATETIME('NOW', 'localtime'),'" + diferenciaHoraria + " hours'),1,10)) - JULIANDAY(SUBSTR(CxcFecha,1,10))) BETWEEN 61 AND 90) " +
                       "and a.recsecuencia in (select r.recSecuencia from recibos r where repcodigo = A.repcodigo and RecEstatus <>0)),0) as Balance " +
                       "FROM CuentasxCobrar c " +
                       "inner join TiposTransaccionesCxc ttc on trim(ttc.ttcReferencia) = CxcSigla and ttcOrigen = 1 " +
                       "WHERE (JULIANDAY(SUBSTR(DATETIME(DATETIME('NOW', 'localtime'),'" + diferenciaHoraria + " hours'),1,10)) - JULIANDAY(SUBSTR(CxcFecha,1,10))) BETWEEN 61 AND 90 AND CliID = " + Cliid + " " + whereMonCodigo +
                       "UNION ALL " +

                       "SELECT '91' as Desde, '' as Hasta, IFNULL( CAST(SUM(CXCBalance) as numeric),0) - " +
                       "IFNULL((Select sum(RecValor + RecDescuento+ IFNULL(a.RecDescuentoDesmonte,0)) from RecibosAplicacion  a where a.Cliid= c.CliID " +
                       "and a.CxcReferencia  in (select cc.CxcReferencia from CuentasxCobrar cc where cc.cliid =c.cliid " +
                       "AND (JULIANDAY(SUBSTR(DATETIME(DATETIME('NOW', 'localtime'),'" + diferenciaHoraria + " hours'),1,10)) - JULIANDAY(SUBSTR(CxcFecha,1,10))) >= 91) " +
                       "and a.recsecuencia in (select r.recSecuencia from recibos r where repcodigo = A.repcodigo and RecEstatus <>0)),0) as Balance " +
                       "FROM CuentasxCobrar c " +
                       "inner join TiposTransaccionesCxc ttc on trim(ttc.ttcReferencia) = CxcSigla and ttcOrigen = 1 " +
                       "WHERE (JULIANDAY(SUBSTR(DATETIME(DATETIME('NOW', 'localtime'),'" + diferenciaHoraria + " hours'),1,10)) - JULIANDAY(SUBSTR(CxcFecha,1,10))) >= 91 AND CliID = " + Cliid + " " + whereMonCodigo +
                       ") result GROUP BY result.Desde, result.Hasta";

            return SqliteManager.GetInstance().Query<SaldoPorAntiguedad>(sql, new string[] { });
        }
        public async Task<List<RowLinker>> GetResumenDeGestion(string desde, string hasta)
        {
            var result = new List<RowLinker>();

            string where = "";

            if (!string.IsNullOrWhiteSpace(desde) && !string.IsNullOrWhiteSpace(hasta))
            {
                where = " and cast(strftime('%Y%m%d',p.Pedfecha) as integer) between cast(strftime('%Y%m%d', '" + desde + "') as integer) and cast(strftime('%Y%m%d', '" + hasta + "') as integer) ";
            }

            string condition;
            result.Add(new SubTitle() { Description = "Clientes a Visitar" });
            result.Add(new Inventarios() { ProDescripcion = "Total de Clientes a Visitar: ", invCantidad = GetClientesPendientesAContar(desde, hasta) });

            result.Add(new SubTitle() { Description = "Clientes Visitados" });
            condition = $"select cliid from clientes where CliID in (select CliID from visitas where VisFechaEntrada between '{desde}' and '{hasta}')";

            double list = SqliteManager.GetInstance().Query<Visitas>(condition, new string[] { }).Count();
            result.Add(new Inventarios() { ProDescripcion = "Total de Clientes Visitados: ", invCantidad = list});
            double clivis = list;

            result.Add(new SubTitle() { Description = "Clientes Efectivos" });

            list = SqliteManager.GetInstance().Query<Pedidos>("select p.PedTotal from Pedidos p " +
            "where trim(p.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and p.PedEstatus <> 0 " + where + "" +
            "union all " +
            "select p.PedTotal from PedidosConfirmados  p where trim(p.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and p.PedEstatus <> 0 " + where + "", new string[] { }).Count();
            result.Add(new Inventarios() { ProDescripcion = "Total de Clientes Efectivos: ", invCantidad = list });

            double clientevisresultado = (list / clivis) * 100.0;

            clientevisresultado = double.IsNaN(clientevisresultado) || double.IsInfinity(clientevisresultado) ? 0 : clientevisresultado;

            result.Add(new SubTitle() { Description = "Efectividad de la ventas" });
            result.Add(new Inventarios() { ProDescripcion = "% Efectividad: ", invCantidad = clientevisresultado});

            DateTime datehasta = DateTime.Parse(hasta);
            DateTime datedesde = DateTime.Parse(desde);

            int resultado = Math.Abs(datedesde.Day - datehasta.Day) + 1;

            double cajasVender = await new DS_Presupuestos().GetPreMesByPreTipByDates(datehasta.Year.ToString(), datehasta.Month.ToString(),Arguments.CurrentUser.RepCodigo, Arguments.CurrentUser.RepClave) / resultado;

            cajasVender = Double.IsNaN(cajasVender) || double.IsInfinity(cajasVender) ? 0 : cajasVender;

            result.Add(new SubTitle() { Description = "Total Cajas a Vender" });
            result.Add(new Inventarios() { ProDescripcion = "Cajas a Vender: ", invCantidad = cajasVender, Bold = true });

            string query = "select round(sum(a.cantidad),2) as invCantidad " +
                           "from (select case when upper(ifnull(d.UnmCodigo, '')) = 'CJ' then sum(d.PedCantidad) else sum( (d.PedCantidad * 1.0) / pr.ProUnidades) end as cantidad from " +
                           "Pedidos p " +
                           "inner join PedidosDetalle d on d.RepCodigo = p.RepCodigo and d.PedSecuencia = p.PedSecuencia " +
                           "inner join Productos pr on pr.ProID = d.ProID " +
                           "where p.RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' " + where + " and p.PedEstatus <> 0 " +
                           "union " +
                           "select case when upper(ifnull(d.UnmCodigo, '')) = 'CJ' then sum(d.PedCantidad) else sum((d.PedCantidad * 1.0) / pr.ProUnidades) end as cantidad from " +
                           "PedidosConfirmados p " +
                           "inner join PedidosDetalleConfirmados d on d.RepCodigo = p.repCodigo and d.PedSecuencia = p.PedSecuencia " +
                           "inner join Productos pr on pr.ProID = d.ProID " +
                           "where p.RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' " + where + " and p.PedEstatus <> 0) a";
            var cajasList = SqliteManager.GetInstance().Query<Inventarios>(query, new string[] { }).FirstOrDefault();

            var cajas = cajasList != null ? cajasList.invCantidad : 0.0;

            double resultcj = cajas / (double)cajasVender;

            resultcj = double.IsNaN(resultcj) || double.IsInfinity(resultcj) ? 0 : resultcj;

            result.Add(new SubTitle() { Description = "Total Cajas Vendidas" });
            result.Add(new Inventarios() { ProDescripcion = "Cajas vendidas: ", invCantidad = cajas, Bold = true });

            result.Add(new SubTitle() { Description = "Logro del presupuesto" });
            result.Add(new Inventarios() { ProDescripcion = "Logro Cajas: ", invCantidad = resultcj, Bold = true });

            return result;
        }


        public int GetClientesPendientesAContar(string desde = null, string hasta = null)
        {
            DateTime timespan = DateTime.Parse(desde);
            int time = Math.Abs(timespan.Day - DateTime.Parse(hasta).Day);
            List<Clientes> clientes = new List<Clientes>();

            //int totaldays = (int)Math.Abs(time.TotalDays);

            for(int i = 0; i <= time; i++)
            {
                DateTime dates = timespan.AddDays(i);
                var numeroSemana = 0;
                if (myParametro.GetParSemanasAnios())
                {
                    numeroSemana = new DS_RutaVisitas().GetNumeroSemana(dates);
                }
                else
                {
                    var rep = new DS_Representantes().GetAllRepresentantes().Where(x => x.RepCodigo.Trim().ToUpper() == Arguments.CurrentUser.RepCodigo.Trim().ToUpper()).FirstOrDefault();
                    numeroSemana = Functions.GetWeekOfMonth(dates, rep);
                    if (numeroSemana > 4)
                    {
                        numeroSemana = 4;
                    }
                }

                RutasVisitasArgs RutaVisitaData = new RutasVisitasArgs()
                {
                    DiaDeLaSemana = (int)(dates).DayOfWeek - 1 == -1 ? 6 : (int)(dates).DayOfWeek - 1,
                    NumeroSemana = numeroSemana
                };

                char[] diasSemana = new char[] { '_', '_', '_', '_', '_', '_', '_' };

                diasSemana[RutaVisitaData.DiaDeLaSemana] = '1';
                string semanaValues = new string(diasSemana);
                string where = " AND R.RutSemana" + RutaVisitaData.NumeroSemana.ToString() + " like '" + semanaValues + "'";

                clientes.AddRange(SqliteManager.GetInstance().Query<Clientes>("select c.CliNombre, c.CliID from Clientes c inner join RutaVisitas R on R.cliid = c.CliID " +
                        "where R.RutSemana" + RutaVisitaData.NumeroSemana.ToString() + " like '" + semanaValues + "'" +
                         "order by CliNombre", new string[] { }));
            }

            return clientes.Count;
        }

        public List<RepresentantesDetalleNCF2018> GetRepresentantesDetalleNCF2018()
        {
            var list =  SqliteManager.GetInstance()
                .Query<RepresentantesDetalleNCF2018>("select ReDSerie, RedTipoComprobante, ReDNCFActual, ReDNCFMax, ReDFechaVencimiento from " + (myParametro.GetParTakeFromNCF2021() ? "RepresentantesDetalleNCF2021" : "RepresentantesDetalleNCF2018") + "");

            foreach(var item in list)
            {
                if(item.ReDNCFActual == item.ReDNCFMax)
                {
                    continue;
                }
                //var ncfActual = item.ReDNCFActual.ToString("00000000") + item.ReDNCFActual.ToString();
                var ncfActual = (item.ReDNCFActual + 1).ToString();
                //sArguments.Values.CurrentClient.ncfSecuencia = ncfActual;
                while (ncfActual.Length < 8)
                {
                    ncfActual = "0" + ncfActual;
                }
                item.ReDAICF = item.ReDSerie + item.RedTipoComprobante + ncfActual;
            }

            return list;

        }

    }
}
