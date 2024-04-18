using MovilBusiness.Configuration;
using MovilBusiness.Internal;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace MovilBusiness.DataAccess
{
    public class DS_EntregasRepartidorTransacciones : DS_Controller
    {
        public List<EntregasRepartidorTransacciones> GetEntregasDisponibles(int cliId, string numero = null, bool forVenta = false, bool isRecepcionDevolucion = false, string secCodigo = null)
        {
            string diasPermitidos = "7";
            if (DS_RepresentantesParametros.GetInstance().GetDiasEntregasRepartidorTransaccionesVisibles() > 0)
            {
                diasPermitidos = DS_RepresentantesParametros.GetInstance().GetDiasEntregasRepartidorTransaccionesVisibles().ToString();
            }

            string where = " and t.TitID = 4 ";

            if (myParametro.GetParEntregasRepartidor() == 3)
            {
                where= " and t.TitID = 1 ";
            }

            if (forVenta)
            {
                where = " and t.TitID = 1 and (cast(replace(cast(julianday(datetime('now')) - julianday(EnrFecha) as integer),' ', '') as integer)) < " + diasPermitidos + "  ";
            }

            if (isRecepcionDevolucion)
            {
                where = " and t.TitID = 2 ";
            }

            if (!string.IsNullOrWhiteSpace(numero))
            {
                where += " and t.venNumeroERPDocum like '" + numero + "%'";
            }

            if (!string.IsNullOrWhiteSpace(secCodigo))
            {
                if(!myParametro.GetParEntregasMultiples())
                where += " and ifnull(t.SecCodigo, '') = '" + secCodigo + "' ";
            }

            var query = "select t.*, c.CliNombre, c.CliCodigo, ifnull(e.EstDescripcion, 'No entregado') as estatusDescripcion, " +
                "t.ConID as ConID, t.RepVendedor as RepVendedor, s.SecDescripcion as SecDescripcion, (select sum(((TraPrecio + ifnull(TraAdValorem, 0.0) + ifnull(TraSelectivo, 0.0)) * TraCantidad)+ (((((TraPrecio + TraAdValorem + TraSelectivo) - ifnull(TraDescuento, 0.0)) * ifnull(TraItbis, 0.0) / 100.0)) * TraCantidad)- (ifnull(TraDescuento, 0.0) * TraCantidad)) as Total " +
                "from EntregasRepartidorTransaccionesDetalle where EnrSecuencia = t.EnrSecuencia and TraSecuencia = t.TraSecuencia and RepCodigo = t.RepCodigo and TitID = t.TitID and CliID = t.CliID) as EntMontoTotal " +
                "from EntregasRepartidorTransacciones t " +
                "inner join EntregasRepartidor et on et.EnrSecuencia=t.EnrSecuencia and et.RepCodigo = t.RepCodigo " +
                "inner join Clientes c on c.CliID = t.CliID " +
                "left join Estados e on e.EstTabla = 'EntregasRepartidorTransacciones' and e.EstEstado = t.enrEstatusEntrega " +
                "left join Sectores s on s.SecCodigo = t.SecCodigo " +
                "where t.CliID = " + cliId.ToString() + " and trim(t.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and enrEstatusEntrega = 1 " + where + "  order by t.SecCodigo";
            
            return SqliteManager.GetInstance().Query<EntregasRepartidorTransacciones>(query,
                new string[] { });
        }

        public int GetEntregasFacturasDisponibles(int cliId, bool isEntregaPedidos)
        {
            string where = $"{(isEntregaPedidos ? " and t.TitID = 1 " : " and t.TitID = 4 ")} ";

            var list =  SqliteManager.GetInstance().Query<EntregasRepartidorTransacciones>("select count(t.EnrSecuencia) as EnrSecuencia " +
                "from EntregasRepartidorTransacciones t " +
                "inner join EntregasRepartidor et on et.EnrSecuencia=t.EnrSecuencia and et.RepCodigo = t.RepCodigo " +
                "inner join Clientes c on c.CliID = t.CliID " +
                "left join Estados e on e.EstTabla = 'EntregasRepartidorTransacciones' and e.EstEstado = t.enrEstatusEntrega " +
                "left join Sectores s on s.SecCodigo = t.SecCodigo " +
                "where t.CliID = " + cliId.ToString() + " and trim(t.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and enrEstatusEntrega = 1 " + where + " order by t.SecCodigo", new string[] { }); 

            if (list != null && list.Count > 0)
            {
                return list[0].EnrSecuencia;
            }

 
            return 0;
        }

        public List<EntregasRepartidorTransacciones> GetEntregasRealizadas(int cliId, string numero = null)
        {
            string where = " and TitID = 4 ";

            if (myParametro.GetParEntregasRepartidor() == 3)
            {
                where = " and TitID = 1 ";
            }

            if (!string.IsNullOrWhiteSpace(numero))
            {
                where += " and t.EntSecuencia like '" + numero + "%'";
            }

            /*string AdValorem = "EntAdValorem";

            if (myParametro.GetParADVALOREMTIPO() == 1 || myParametro.GetParADVALOREMTIPO() == -1)
            {
                AdValorem = " (CAST(EntPrecio AS REAL) * (CAST(IFNULL(EntAdValorem, 0.0) AS REAL) / 100.0)) ";
            }

            var selectMontoTotal = "(select SUM(((((EntPrecio + EntSelectivo + " + AdValorem + ") - ifnull(EntDescuento, 0)) + round(((EntPrecio + ifnull(EntSelectivo, 0) + ifnull(" + AdValorem + ", 0)) - ifnull(EntDescuento, 0))) * (EntItbis / 100.0))) * ((CAST(ifnull(EntCantidadDetalle, 0) AS REAL)  / case when ifnull(CAST(P.ProUnidades AS REAL), 0) = 0 then 1 else CAST(ifnull(P.ProUnidades, 1) AS REAL) end) + EntCantidad)) as Total " +
                "from EntregasTransaccionesDetalle f inner join Productos P on p.ProID = f.ProID where EntSecuencia = t.EntSecuencia and RepCodigo = t.RepCodigo) as EntMontoTotal, ";

            var selectMontoTotalConfirmados = "(select SUM(((((EntPrecio + EntSelectivo + " + AdValorem + ") - ifnull(EntDescuento, 0)) + round(((EntPrecio + ifnull(EntSelectivo, 0) + ifnull(" + AdValorem + ", 0)) - ifnull(EntDescuento, 0))) * (EntItbis / 100.0))) * ((CAST(ifnull(EntCantidadDetalle, 0) AS REAL)  / case when ifnull(CAST(P.ProUnidades AS REAL), 0) = 0 then 1 else CAST(ifnull(P.ProUnidades, 1) AS REAL) end) + EntCantidad)) as Total " +
                "from EntregasTransaccionesDetalleConfirmados f inner join Productos P on p.ProID = f.ProID where EntSecuencia = t.EntSecuencia and RepCodigo = t.RepCodigo) as EntMontoTotal, ";
            */
            var query = "select t.RepNombre as RepNombre, t.RepTelefono as RepTelefono, t.EntSecuencia as EntSecuencia, t.EnrSecuencia as EnrSecuencia, t.VenSecuencia as TraSecuencia, t.VenSecuencia as TraSecuencia, 'Anular entrega' as RechazarBtn, t.RepCodigo as RepCodigo, EntFecha as VenFecha, c.CliNombre, c.CliCodigo, 0 as Confirmada, " +
                "ifnull(e.EstDescripcion, '') as estatusDescripcion, t.SecCodigo as SecCodigo, " +
                "t.ConID as ConID, ifnull(t.RepVendedor, '') as RepVendedor, s.SecDescripcion as SecDescripcion, " +
                "EntEstatus as enrEstatusEntrega, EntNCF as VenNCF, EntSecuencia as venNumeroERPDocum, t.rowguid as rowguid, t.EntTotal as EntMontoTotal  " +
                "from EntregasTransacciones t " +
                "inner join Clientes c on c.CliID = t.CliID " +
                "left join Estados e on e.EstTabla = 'EntregasTransacciones' and e.EstEstado = t.EntEstatus " +
                "left join Sectores s on s.SecCodigo = t.SecCodigo " +
                "where t.CliID = " + cliId.ToString() + " and t.EntEstatus in (0,1) and trim(t.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' " + where + "" +
                "union " +
                "select t.RepNombre as RepNombre, t.RepTelefono as RepTelefono, t.EnrSecuencia as EntSecuencia, t.EnrSecuencia as EnrSecuencia, t.VenSecuencia as TraSecuencia, t.VenSecuencia as TraSecuencia, 'Anular entrega' as RechazarBtn, t.RepCodigo as RepCodigo, EntFecha as VenFecha, c.CliNombre, c.CliCodigo, 1 as Confirmada, " +
                "ifnull(e.EstDescripcion, '') as estatusDescripcion, t.SecCodigo as SecCodigo, " +
                "t.ConID as ConID, ifnull(t.RepVendedor, '') as RepVendedor, s.SecDescripcion as SecDescripcion, " +
                "EntEstatus as enrEstatusEntrega, EntNCF as VenNCF, EntSecuencia as venNumeroERPDocum, t.rowguid as rowguid, t.EntTotal as EntMontoTotal " +
                "from EntregasTransaccionesConfirmados t " +
                "inner join Clientes c on c.CliID = t.CliID " +
                "left join Estados e on e.EstTabla = 'EntregasTransacciones' and e.EstEstado = t.EntEstatus " +
                "left join Sectores s on s.SecCodigo = t.SecCodigo " +
                "where t.CliID = " + cliId.ToString() + " and t.EntEstatus in (0,1) and trim(t.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' " + where + "" +
                " order by EntSecuencia";

            return SqliteManager.GetInstance().Query<EntregasRepartidorTransacciones>(query,
                new string[] { });
        }

        public bool HayEntregasDisponibles(string sector = null)
        {

            string where = " and TitID = 4 ";

            if (myParametro.GetParEntregasRepartidor() == 3)
            {
                where = " and TitID = 1 ";
            }

            if (!string.IsNullOrWhiteSpace(sector))
            {
                where += " and ifnull(trim(SecCodigo), '') = '" + sector.Trim() + "' ";
            }

            return SqliteManager.GetInstance().Query<EntregasRepartidorTransacciones>("select EnrSecuencia from EntregasRepartidorTransacciones " +
                "where CliID = ? and trim(RepCodigo) = ? and enrEstatusEntrega = 1 " + where,
                new string[] { Arguments.Values.CurrentClient.CliID.ToString(), Arguments.CurrentUser.RepCodigo.Trim() }).FirstOrDefault() != null;
        }

        public int EntregasDisponiblesByDia()
        {

            var list = SqliteManager.GetInstance().Query<EntregasRepartidor>("select t.EnrSecuencia from EntregasRepartidor t " +
                " inner join EntregasRepartidorTransacciones et on et.EnrSecuencia=t.EnrSecuencia and et.RepCodigo = t.RepCodigo " +
                "where trim(t.RepCodigo) = ? and enrEstatusEntrega = 1 and et.TitID = 1 and t.EnrFecha like '" + Functions.CurrentDate("yyyy-MM-dd") + "%' order by t.EnrSecuencia asc limit 1",
                new string[] { Arguments.CurrentUser.RepCodigo.Trim() });

            if (list != null && list.Count > 0)
            {
                return list[0].EnrSecuencia;
            }


            return 0;
        }

        public List<EntregasRepartidorTransacciones> GetEntregasDisponiblesEnGeneral(bool forVenta = false, bool isRecepcionDevolucion = false)
        {
            string where = " and TitID = 4 ";

            if (myParametro.GetParEntregasRepartidor() == 3)
            {
                where = " and TitID = 1 ";
            }

            if (forVenta)
            {
                where = " and TitID = 1 ";
            }

            if (isRecepcionDevolucion)
            {
                where = " and TitID = 2 ";
            }

            string query = @"select t.*, c.CliNombre, c.CliCodigo, ifnull(e.EstDescripcion, 'No entregado') as estatusDescripcion, 
                 t.ConID as ConID from EntregasRepartidorTransacciones t  
                inner join Clientes c on c.CliID = t.CliID  
                left join Estados e on e.EstTabla = 'EntregasRepartidorTransacciones' and e.EstEstado = t.enrEstatusEntrega  
                where trim(t.RepCodigo) = ?  and enrEstatusEntrega = 1 " + where ;

            if (!forVenta)
            {
                query += @" and t.Cliid in(select distinct CliID from RutaVisitasFecha " +
                    "where trim(RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' " +
                    "and RutFecha like '" + Functions.CurrentDate("yyyy-MM-dd") + "%')";
            }

            query += " order by t.SecCodigo";

            return SqliteManager.GetInstance().Query<EntregasRepartidorTransacciones>(query, new string[] { Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public List<EntregasRepartidorTransacciones> GetEntregasDisponiblesbyDia(bool forVenta = false, bool isRecepcionDevolucion = false,bool isCargaAceptada=false)
        {
            string where = " and t.TitID = 4 ";

            if (myParametro.GetParEntregasRepartidor() == 3)
            {
                where = " and t.TitID = 1 ";
            }

            if (forVenta)
            {
                where = " and t.TitID = 1 ";
            }

            if (isRecepcionDevolucion)
            {
                where = " and t.TitID = 2 ";
            }

            if (isCargaAceptada)
            {
                where += " and  et.EnrEstatus=4 ";
            }

            string query = @"select t.*, c.CliNombre, c.CliCodigo, ifnull(e.EstDescripcion, 'No entregado') as estatusDescripcion, 
                 t.ConID as ConID from EntregasRepartidorTransacciones t 
                inner join EntregasRepartidor et on et.EnrSecuencia=t.EnrSecuencia and et.RepCodigo = t.RepCodigo
                inner join Clientes c on c.CliID = t.CliID  
                left join Estados e on e.EstTabla = 'EntregasRepartidorTransacciones' and e.EstEstado = t.enrEstatusEntrega  
                where trim(t.RepCodigo) = ?  and enrEstatusEntrega = 1 " + where;

            if (forVenta)
            {
                query += @" and t.Cliid in(select distinct CliID from RutaVisitasFecha " +
                    "where trim(RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' " +
                    "and RutFecha like '" + Functions.CurrentDate("yyyy-MM-dd") + "%')";
            }

            query += " order by t.SecCodigo";

            return SqliteManager.GetInstance().Query<EntregasRepartidorTransacciones>(query, new string[] { Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public List<EntregasRepartidorTransacciones> GetEntregasDisponiblesbyCuadre(bool forVenta = false, bool isRecepcionDevolucion = false, bool isCargaAceptada = false, Cuadres cuadres =null)
        {
            string where = " and t.TitID = 4 ";

            if (myParametro.GetParEntregasRepartidor() == 3)
            {
                where = " and t.TitID = 1 ";
            }

            if (forVenta)
            {
                where = " and t.TitID = 1 ";
            }

            if (isRecepcionDevolucion)
            {
                where = " and t.TitID = 2 ";
            }

            if (isCargaAceptada)
            {
                where += " and  et.EnrEstatus=4 ";
            }

            string query = @"select t.*, c.CliNombre, c.CliCodigo, ifnull(e.EstDescripcion, 'No entregado') as estatusDescripcion, 
                 t.ConID as ConID from EntregasRepartidorTransacciones t 
                inner join EntregasRepartidor et on et.EnrSecuencia=t.EnrSecuencia and et.RepCodigo = t.RepCodigo
                inner join Clientes c on c.CliID = t.CliID  
                left join Estados e on e.EstTabla = 'EntregasRepartidorTransacciones' and e.EstEstado = t.enrEstatusEntrega  
                inner join cuadres cu on cu.Repcodigo = et.Repcodigo and cu.CuaSecuencia= ?
                where trim(t.RepCodigo) = ?  and enrEstatusEntrega = 1 " + where;

            if (forVenta)
            {
                query += @" and t.Cliid in(select distinct CliID from RutaVisitasFecha " +
                    "where trim(RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' " +
                    "and STRFTIME('%Y-%m-%d', RutFecha)  between STRFTIME('%Y-%m-%d', cu.CuaFechaInicio) and STRFTIME('%Y-%m-%d', DATETIME('NOW', 'localtime')))";
            }

            query += " order by t.SecCodigo";

            return SqliteManager.GetInstance().Query<EntregasRepartidorTransacciones>(query, new string[] { cuadres.CuaSecuencia.ToString() , Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public List<EntregasRepartidorTransacciones> GetEntregasDisponiblesbyFechaInicioCuadre(bool forVenta = false, bool isRecepcionDevolucion = false, bool isCargaAceptada = false, Cuadres cuadres = null)
        {
            string where = " and t.TitID = 4 ";

            if (myParametro.GetParEntregasRepartidor() == 3)
            {
                where = " and t.TitID = 1 ";
            }

            if (forVenta)
            {
                where = " and t.TitID = 1 ";
            }

            if (isRecepcionDevolucion)
            {
                where = " and t.TitID = 2 ";
            }

            if (isCargaAceptada)
            {
                where += " and  et.EnrEstatus=4 ";
            }

            string query = @"select t.*, c.CliNombre, c.CliCodigo, ifnull(e.EstDescripcion, 'No entregado') as estatusDescripcion, 
                 t.ConID as ConID from EntregasRepartidorTransacciones t 
                inner join EntregasRepartidor et on et.EnrSecuencia=t.EnrSecuencia and et.RepCodigo = t.RepCodigo
                inner join Clientes c on c.CliID = t.CliID  
                left join Estados e on e.EstTabla = 'EntregasRepartidorTransacciones' and e.EstEstado = t.enrEstatusEntrega  
                inner join cuadres cu on cu.Repcodigo = et.Repcodigo and cu.CuaSecuencia= ?
                where trim(t.RepCodigo) = ?  and enrEstatusEntrega = 1 " + where;

            if (forVenta)
            {
                query += @" and t.Cliid in (select distinct CliID from RutaVisitasFecha " +
                    "where trim(RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' " +
                    "and STRFTIME('%Y-%m-%d', RutFecha) = STRFTIME('%Y-%m-%d', cu.CuaFechaInicio))";
            }

            query += " order by t.SecCodigo";

            return SqliteManager.GetInstance().Query<EntregasRepartidorTransacciones>(query, new string[] { cuadres.CuaSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public List<EntregasDetalleTemp> GetProductosEntregaInTemp(bool withQuantity = false, bool validarOfertas = false, bool validarLotes = false, bool isDetalle = false, bool isAdded = false)
        {
            var where = "";

            if (!isDetalle)
            {
                if (validarOfertas)
                {
                    where += " and case when ifnull(TraIndicadorOferta, 0) = 1 then OfeID not in " +
                        "(select distinct ProID from EntregasDetalleTemp where ProID = e.OfeID and TraSecuencia = e.TraSecuencia and ifnull(TraIndicadorOferta, 0) = 0 " +
                        "group by ProID, Posicion, TraSecuencia having (sum(ifnull(Cantidad, 0)) < ifnull(CantidadSolicitada, 0.0) or (ifnull(UsaLote, 0) = 1 and ifnull(Lote, '') = '')) ) else 1=1 end ";

                    if (myParametro.GetParEntregasOfertasTodoONada())
                    {
                        where += " and case when ifnull(TraIndicadorOferta, 0) = 0 and exists(select OfeID from EntregasDetalleTemp where OfeID = e.ProID and TraSecuencia = e.TraSecuencia and ifnull(TraIndicadorOferta, 0) = 1) " +
                            "then exists(select 1 from EntregasDetalleTemp where ProID = e.ProID and Posicion = e.Posicion and TraSecuencia = e.TraSecuencia and case when ifnull(UsaLote, 0) = 1 then ifnull(Lote, '') != '' else 1=1 end " +
                            "group by ProID, Posicion, TraSecuencia having sum(Cantidad) >= ifnull(CantidadSolicitada, 0.0)) else 1=1 end";
                    }
                }

                if (validarLotes)
                {
                    where += " and (case when ifnull(UsaLote, 0) = 1 then ifnull(Lote, '') != '' else 1=1 end) ";
                }
            }

            if (isAdded)
            {
                where += " and ifnull(IsAdded, 0) = 1 ";
            }

            var orderBy = " order by TraSecuencia, Posicion ";

            if(Arguments.Values.CurrentModule == Enums.Modules.CONDUCES)
            {
                orderBy = " order by ProDescripcion ";
            }

            var query = "select e.EnrSecuencia as EnrSecuencia, IsAdded, Precio, Itbis, Descuento, AdValorem, Selectivo, e.TraSecuencia as TraSecuencia, e.rowguid as rowguid, e.UnmCodigo as UnmCodigo, e.OfeID as OfeID, Posicion, " +
                "TraIndicadorOferta, IndicadorMalEstado, ProID, ProCodigo, ProDescripcion, UsaLote, Lote, MotIdDevolucion, MotDescripcion, Documento, FechaVencimiento, " +
                "CantidadSolicitada, CantidadSolicitadaDetalle, Cantidad, CantidadDetalle, CantidadDisponibleOriginal, CantidadDisponibleDetalleOriginal, ProUnidades " +
                "from EntregasDetalleTemp e where 1=1 " + where + (withQuantity && !isDetalle && !myParametro.GetParEntregasAgregarCero() ? " and (ifnull(Cantidad, 0) > 0 or ifnull(CantidadDetalle, 0) > 0) " : "") + orderBy;

            return SqliteManager.GetInstance().Query<EntregasDetalleTemp>(query, new string[] { });
        }

        public void InsertProductInTemp(int enrSecuencia, int traSecuencia, int titId, int cliId, bool isForDetail = false, bool deleteTemp = true)
        {
            if(Arguments.Values.CurrentModule == Enums.Modules.CONDUCES)
            {
                return;
            }

            if (deleteTemp)
            {
                SqliteManager.GetInstance().Execute("delete from EntregasDetalleTemp", new string[] { });
            }

            var list = SqliteManager.GetInstance().Query<EntregasDetalleTemp>("select e.TraSecuencia as TraSecuencia, "+(isForDetail?" 1 as IsAdded, ":"")+" e.UnmCodigo as UnmCodigo, e.OfeID as OfeID, e.TraIndicadorOferta as TraIndicadorOferta, e.TraPosicion as Posicion, p.ProID as ProID, p.ProCodigo as ProCodigo, p.ProDescripcion as ProDescripcion, case when upper(ProDatos3) like '%L%' then 1 else 0 end as UsaLote, " +
                "TraCantidad as CantidadSolicitada, e.TraPrecio as Precio, e.TraItbis as Itbis, e.TraDescuento as Descuento, e.TraSelectivo as Selectivo, e.TraAdValorem as AdValorem, " +
                "TraCantidadDetalle as CantidadSolicitadaDetalle, p.ProUnidades as ProUnidades " + (myParametro.GetParEntregasRepartidorProductoAutoCantidad() || isForDetail ? ", TraCantidad as Cantidad, TraCantidadDetalle as CantidadDetalle " : ", 0 as Cantidad, 0 as CantidadDetalle") + " " +
                "from EntregasRepartidorTransaccionesDetalle e " +
                "inner join Productos p on p.ProID = e.ProID " +
                "where e.EnrSecuencia = ? and e.TraSecuencia = ? and e.TitID = ? and e.CliID = ? and trim(e.RepCodigo) = ?",
                new string[] { enrSecuencia.ToString(), traSecuencia.ToString(), titId.ToString(), cliId.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            foreach(var ent in list)
            {
                ent.rowguid = Guid.NewGuid().ToString();
            }

            SqliteManager.GetInstance().InsertAll(list);
        }

        public void InsertProductInTempForDetail(int entSecuencia, bool confirmado)
        {
            SqliteManager.GetInstance().Execute("delete from EntregasDetalleTemp", new string[] { });

            var query = "select 1 as IsAdded, e.EntPrecio as Precio, e.EntItbis as Itbis, e.EntDescuento as Descuento, e.EntAdValorem as AdValorem, e.EntSelectivo as Selectivo, t.VenSecuencia as TraSecuencia, e.UnmCodigo as UnmCodigo, '' as Lote, " +
                "e.EntIndicadorOferta as TraIndicadorOferta, e.EntPosicion as Posicion, p.ProID as ProID, p.ProCodigo as ProCodigo, " +
                "p.ProDescripcion as ProDescripcion, 0 UsaLote, " +
                "e.EntCantidad as CantidadSolicitada, e.EntCantidadDetalle as CantidadSolicitadaDetalle, e.EntCantidad as Cantidad, e.EntCantidadDetalle as CantidadDetalle " +
                "from " + (confirmado ? "EntregasTransaccionesDetalleConfirmados" : "EntregasTransaccionesDetalle") + " e " +
                "inner join Productos p on p.ProID = e.ProID " +
                "inner join "+(confirmado? "EntregasTransaccionesConfirmados" : "EntregasTransacciones") +" t on t.RepCodigo = e.RepCodigo and t.EntSecuencia = e.EntSecuencia " +
                "where e.EntSecuencia = ? and trim(e.RepCodigo) = ? and ifnull(p.ProDatos3, '') not like '%L%' " +
                "union " +
                "select 1 as IsAdded, e2.EntPrecio as Precio, e2.EntItbis as Itbis, e2.EntDescuento as Descuento, e2.EntAdValorem as AdValorem, e2.EntSelectivo as Selectivo, t.VenSecuencia as TraSecuencia, e2.UnmCodigo as UnmCodigo, e.EntLote as Lote, " +
                "e2.EntIndicadorOferta as TraIndicadorOferta, e.EntPosicion as Posicion, p.ProID as ProID, p.ProCodigo as ProCodigo, " +
                "p.ProDescripcion as ProDescripcion, 1 as UsaLote, " +
                "e.EntCantidad as CantidadSolicitada, e.EntCantidadDetalle as CantidadSolicitadaDetalle, e.EntCantidad as Cantidad, e.EntCantidadDetalle as CantidaDetalle " +
                "from " + (confirmado ? "EntregasTransaccionesDetalleLotesConfirmados" : "EntregasTransaccionesDetalleLotes") + " e " +
                "inner join " + (confirmado ? "EntregasTransaccionesDetalleConfirmados" : "EntregasTransaccionesDetalle") + " e2 on e2.EntSecuencia = e.EntSecuencia and e2.RepCodigo = e.RepCodigo and e2.EntPosicion = e.EntPosicion " +
                "inner join Productos p on p.ProID = e2.ProID " +
                "inner join "+(confirmado? "EntregasTransaccionesConfirmados" : "EntregasTransacciones") +" t on t.RepCodigo = e.RepCodigo and t.EntSecuencia = e.EntSecuencia " +
                "where e.EntSecuencia = ? and trim(e.RepCodigo) = ? and ifnull(p.ProDatos3, '') like '%L%'";

            var list = SqliteManager.GetInstance().Query<EntregasDetalleTemp>(query,
                new string[] { entSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim(), entSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            foreach (var ent in list)
            {
                ent.rowguid = Guid.NewGuid().ToString();
            }

            SqliteManager.GetInstance().InsertAll(list);
        }

        public void InsertProductInTempForVentas(int enrSecuencia, int traSecuencia, int titId, out bool productosSinExistencia)
        {
            productosSinExistencia = false;
            var parloteAutomaticoByEntrega = myParametro.GetParVentasLotesAutomaticos() == 2;
            SqliteManager.GetInstance().Execute("delete from ProductosTemp where TitID = " + titId.ToString());

            var list = SqliteManager.GetInstance().Query<ProductosTemp>("select " + titId.ToString() + " as TitID, e.OfeID, e.ProID as ProID, p.ProCodigo as ProCodigo, " +
               "e.TraCantidad as Cantidad, e.TraCantidadDetalle as CantidadDetalle," +
               "case when ifnull(p.ProUnidades, 0) = 0 then 1 else p.ProUnidades end as ProUnidades, " +
               "p.ProDescripcion as Descripcion, p.ProDatos3 as ProDatos3, 0 as IndicadorOferta, TraPrecio as Precio, TraItbis as Itbis, " +
               "TraSelectivo as Selectivo, TraAdValorem as AdValorem, TraDescuento as Descuento, TraDesPorciento as DesPorciento, " +
               "e.UnmCodigo as UnmCodigo, p.ProIndicadorDetalle as IndicadorDetalle, ProPrecio3, ProDescripcion2, ProDescripcion3, ProDatos2, " +
               "ProDatos1, ProDescripcion1, " + (parloteAutomaticoByEntrega ? " ifnull(e.TraLote,'') as Lote " : " '' as Lote ") + " , e.TraCantidad as CantidadEntrega, e.TraPosicion as Posicion, e.EnrSecuencia as EnrSecuencia,e.TraSecuencia as TraSecuencia " +
               "from EntregasRepartidorTransaccionesDetalle e " +
               "inner join Productos p on p.ProID = e.ProID " +
              "where ifnull(e.TraIndicadorOferta, 0) = 0 and e.EnrSecuencia = ? and e.TitID = 1 and trim(e.RepCodigo) = ? and e.TraSecuencia = ? and e.cliid = ? ",
               new string[] { enrSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim(), traSecuencia.ToString(), Arguments.Values.CurrentClient.CliID.ToString() });

            var listOferta = SqliteManager.GetInstance().Query<ProductosTemp>("select " + titId.ToString() + " as TitID, e.OfeID, e.ProID as ProID, p.ProCodigo as ProCodigo, " +
                 "e.TraCantidad as Cantidad, e.TraCantidadDetalle as CantidadDetalle," +
                 "case when ifnull(p.ProUnidades, 0) = 0 then 1 else p.ProUnidades end as ProUnidades, " +
                 "p.ProDescripcion as Descripcion, p.ProDatos3 as ProDatos3, 1 as IndicadorOferta, TraPrecio as Precio, TraItbis as Itbis, " +
                 "TraSelectivo as Selectivo, TraAdValorem as AdValorem, TraDescuento as Descuento, TraDesPorciento as DesPorciento, " +
                 "e.UnmCodigo as UnmCodigo, p.ProIndicadorDetalle as IndicadorDetalle, ProPrecio3, ProDescripcion2, ProDescripcion3, ProDatos2, " +
                 "ProDatos1, ProDescripcion1, " + (parloteAutomaticoByEntrega ? " ifnull(e.TraLote,'') as Lote " : " '' as Lote ") + ", e.TraCantidad as CantidadEntrega, e.TraPosicion as Posicion, e.EnrSecuencia as EnrSecuencia,e.TraSecuencia as TraSecuencia " +
                 "from EntregasRepartidorTransaccionesDetalle e " +
                 "inner join Productos p on p.ProID = e.ProID " +
                "where ifnull(e.TraIndicadorOferta, 0) = 1 and e.EnrSecuencia = ? and e.TitID = 1 and trim(e.RepCodigo) = ? and e.TraSecuencia = ? and e.cliid = ? ",
                 new string[] { enrSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim(), traSecuencia.ToString(), Arguments.Values.CurrentClient.CliID.ToString() });

            var dsInv = new DS_Inventarios();
            var almIdDespacho = myParametro.GetParAlmacenIdParaDespacho();

            foreach (var producto in list)
            {

                foreach (var prodOferta in listOferta)
                {
                    var ofertaRebaja = SqliteManager.GetInstance().Query<Ofertas>("select OfeID from Ofertas o " +
                  "where ifnull(o.OfeIndicadorRebajaVenta, 0) = 1 and o.OfeID = ?",
                  new string[] { prodOferta.OfeID.ToString() });

                    if (ofertaRebaja.Count > 0 && prodOferta.ProID == producto.ProID)
                    {
                        producto.Cantidad = producto.Cantidad + prodOferta.Cantidad;
                    }
                }

                if (producto.ProDatos3.Contains("x"))
                {
                    double combosDisponibles = producto.Cantidad;
                    double combosExistentes = 0;
                    var productosCombo = new DS_ProductosCombos().GetProductosCombo(producto.ProID);
                    foreach (var proCombo in productosCombo)
                    {
                        var cantidadTotal = dsInv.GetCantidadTotalInventario(proCombo.ProID, almIdDespacho);
                        var cantidadCombosDisponibles = new DS_ProductosCombos().GetCombosDisponiblesxCantidad(proCombo.ProID,proCombo.ProIDCombo, cantidadTotal);

                        combosExistentes = (cantidadCombosDisponibles < combosExistentes || combosExistentes == 0) ? cantidadCombosDisponibles : combosExistentes;

                        if (!dsInv.HayExistencia(proCombo.ProID, (producto.Cantidad * proCombo.PrcCantidad), out Inventarios existencia, producto.CantidadDetalle, almIdDespacho, false))
                        {
                            productosSinExistencia = true;
                            combosDisponibles = (cantidadCombosDisponibles < combosDisponibles) ? cantidadCombosDisponibles : combosDisponibles;
                        }

                        if (existencia != null)
                        {
                            producto.InvCantidad =  (int)combosExistentes;
                            producto.InvCantidadDetalle = existencia.InvCantidadDetalle;

                            if (myParametro.GetCantidadxLibras())
                            {
                                producto.InvCantidad = combosExistentes;
                            }
                        }

                    }

                    if (combosDisponibles != producto.Cantidad)
                    {
                        producto.Cantidad = Math.Truncate(combosDisponibles / producto.ProUnidades);
                        if (myParametro.GetCantidadxLibras())
                        {
                            producto.Cantidad = combosDisponibles;
                            producto.CantidadDetalle = 0;
                        }
                    }

                   

                }
                else
                {
                    
                    if (!dsInv.HayExistencia(producto.ProID, producto.Cantidad, out Inventarios existencia, producto.CantidadDetalle, almIdDespacho, false, lote: parloteAutomaticoByEntrega ? producto.Lote : "" ))
                    {
                        productosSinExistencia = true;

                        var cantidadTotal = dsInv.GetCantidadTotalInventario(producto.ProID, almIdDespacho, lote: parloteAutomaticoByEntrega ? producto.Lote : "");

                        producto.Cantidad = Math.Truncate(cantidadTotal / producto.ProUnidades);

                        if (producto.CantidadDetalle > 0)
                        {
                            producto.CantidadDetalle = (int)Math.Round(((cantidadTotal / producto.ProUnidades) - Math.Truncate(cantidadTotal / producto.ProUnidades)) * producto.ProUnidades);
                        }

                        if (myParametro.GetCantidadxLibras())
                        {
                            producto.Cantidad = cantidadTotal;
                            producto.CantidadDetalle = 0;
                        }
                    }

                    if (existencia != null)
                    {
                        producto.InvCantidad = (int)existencia.invCantidad;
                        producto.InvCantidadDetalle = existencia.InvCantidadDetalle;

                        if (myParametro.GetCantidadxLibras())
                        {
                            producto.InvCantidad = existencia.invCantidad;
                        }
                    }
                }

                producto.rowguid = Guid.NewGuid().ToString();

                if (producto.Cantidad > 0 || producto.CantidadDetalle > 0) {
                    SqliteManager.GetInstance().Insert(producto);
                }                
            }

        }

        public bool CantidadIsValida(int proId, double cantidad, int posicion, int traSecuencia, double cantidadSolicitada, string lote = null, bool isEditing = false, string rowguid = null)
        {
            string whereLote = "";

            if (!string.IsNullOrWhiteSpace(lote))
            {
                whereLote = " and ifnull(Lote, '') != '' ";
            }

            if (!string.IsNullOrWhiteSpace(rowguid))
            {
                whereLote += " and rowguid != '"+rowguid+"'";
            }

            var current = SqliteManager.GetInstance().Query<EntregasDetalleTemp>("select sum(Cantidad) as Cantidad from EntregasDetalleTemp " +
                "where ProID = ? and Posicion = ? and TraSecuencia = ? " + whereLote + " group by ProID, Posicion", 
                new string[] { proId.ToString(), posicion.ToString(), traSecuencia.ToString() });

            double cantidadRestar = 0;
            if (current != null && current.Count > 0)
            {
                cantidadRestar = current[0].Cantidad;
            }

            if (string.IsNullOrWhiteSpace(lote) && isEditing)
            {
                cantidadRestar = 0;
            } else if (isEditing)
            {
                //busco el mismo producto agregado con otro lote para sumar sus cantidades y validar que no sea mayor a la solicitada
                var othersLotes = SqliteManager.GetInstance().Query<EntregasDetalleTemp>("select sum(Cantidad) as Cantidad from EntregasDetalleTemp " +
                    "where ProID = ? and Posicion = ? and TraSecuencia = ? and ifnull(Lote, '') != ? and ifnull(Lote, '') != '' and ifnull(UsaLote, 0) = 1 " 
                    + (!string.IsNullOrWhiteSpace(rowguid) ? " and rowguid != '"+rowguid+"'" : ""),
                    new string[] { proId.ToString(), posicion.ToString(), traSecuencia.ToString(), lote }).FirstOrDefault();

                if (othersLotes != null)
                {
                    cantidadRestar = othersLotes.Cantidad;
                }

            }

            //return myParametro.GetParNoEntregasParaciales()?  cantidadRestar + cantidad == cantidadSolicitada : cantidadRestar + cantidad <= cantidadSolicitada;
            return cantidadRestar + cantidad <= cantidadSolicitada;
        }

        public void DeleteFromTemp(int proId = -1, int posicion = -1, string lote = null, int traSecuencia = -1)
        {
            string where = "";

            if (!string.IsNullOrWhiteSpace(lote))
            {
                where += " and upper(trim(Lote)) = trim(upper('" + lote + "')) ";
            }

            if (proId != -1)
            {
                where += " and ProID = " + proId.ToString();
            }

            if (posicion != -1)
            {
                where += " and Posicion = " + posicion.ToString();
            }

            if(traSecuencia != -1)
            {
                where += " and TraSecuencia = " + traSecuencia.ToString();
            }

            SqliteManager.GetInstance().Execute("delete from EntregasDetalleTemp where 1=1 " + where, new string[] { });
        }

        public void AgregarProducto(EntregasDetalleTemp producto, bool isRecepcionDevolucion, bool isEditing, bool usarLote = true)
        {
            if (producto.UsaLote && usarLote)
            {
                SqliteManager.GetInstance().Execute("delete from EntregasDetalleTemp where ProID = ? and Posicion = ? and rowguid = '" + producto.rowguid + "' ",
                        new string[] { producto.ProID.ToString(), producto.Posicion.ToString() });

                if (string.IsNullOrWhiteSpace(producto.Lote))
                {
                    producto.Lote = "";
                }

                var map = new Hash("EntregasDetalleTemp") { SaveScriptForServer = false };
                map.Add("ProID", producto.ProID);
                map.Add("ProDescripcion", producto.ProDescripcion);
                map.Add("ProCodigo", producto.ProCodigo);
                map.Add("UsaLote", producto.UsaLote ? "1" : "0");
                map.Add("TraSecuencia", producto.TraSecuencia);
                map.Add("IsAdded", producto.IsAdded);
                map.Add("EnrSecuencia", producto.EnrSecuencia);

                var edited = SqliteManager.GetInstance().Query<EntregasDetalleTemp>("select Cantidad, rowguid from EntregasDetalleTemp " +
                    "where ProID = ?  and Posicion = ? and UsaLote = '1' and upper(ifnull(Lote, '')) = ?",
                    new string[] { producto.ProID.ToString(), producto.Posicion.ToString(), producto.Lote.ToUpper() }).FirstOrDefault();

                var cantidad = producto.Cantidad;

                var rowguid = producto.rowguid;

                if (edited != null && !isEditing)
                {
                    cantidad += edited.Cantidad;
                    rowguid = edited.rowguid;
                }

                var isConduceLote = myParametro.GetParEntregasOfertasTodoONada() && Arguments.Values.CurrentModule == Enums.Modules.CONDUCES;

                map.Add("Cantidad", string.IsNullOrWhiteSpace(producto.Lote) && !isConduceLote ? producto.CantidadSolicitada : cantidad);
                map.Add("CantidadDetalle", producto.CantidadDetalle);
                map.Add("Lote", producto.Lote);
                map.Add("CantidadSolicitada", producto.CantidadSolicitada);
                map.Add("CantidadSolicitadaDetalle", producto.CantidadSolicitadaDetalle);
                map.Add("CantidadDisponibleOriginal", producto.CantidadDisponibleOriginal);
                map.Add("CantidadDisponibleDetalleOriginal", producto.CantidadDisponibleDetalleOriginal);
                map.Add("Posicion", producto.Posicion);
                map.Add("TraIndicadorOferta", producto.TraIndicadorOferta);
                map.Add("OfeID", producto.OfeID);
                map.Add("UnmCodigo", producto.UnmCodigo);
                map.Add("Precio", producto.Precio);
                map.Add("Descuento", producto.Descuento);
                map.Add("Itbis", producto.Itbis);
                map.Add("AdValorem", producto.AdValorem);
                map.Add("Selectivo", producto.Selectivo);
                map.Add("ProUnidades", producto.ProUnidades);
                map.Add("Documento", producto.Documento);

                if (isRecepcionDevolucion)
                {                   
                    map.Add("MotIdDevolucion", producto.MotIdDevolucion);
                    map.Add("MotDescripcion", producto.MotDescripcion);
                    map.Add("FechaVencimiento", producto.FechaVencimiento);
                    map.Add("IndicadorMalEstado", producto.IndicadorMalEstado);
                }

                if (producto.UsaLote && edited != null)
                {
                    map.ExecuteUpdate("TraSecuencia = " + producto.TraSecuencia + " and ProID = " + producto.ProID + " and rowguid = '" + rowguid + "' and Posicion = " + producto.Posicion + " and UsaLote = '1'");
                }
                else
                {
                    map.Add("rowguid", Guid.NewGuid().ToString());
                    map.ExecuteInsert();
                }

                SqliteManager.GetInstance().Execute("delete from EntregasDetalleTemp where ifnull(UsaLote, 0) = 1 and trim(ifnull(Lote, '')) = '' " +
                    "and ProID = ? and Posicion = ? and TraSecuencia = ?", 
                    new string[] { producto.ProID.ToString(), producto.Posicion.ToString(), producto.TraSecuencia.ToString() });

                //se insertan los productos faltantes con lote ''
                SqliteManager.GetInstance().Execute("insert into EntregasDetalleTemp(Precio, Descuento, Itbis, AdValorem, Selectivo, Documento, EnrSecuencia, IsAdded, TraSecuencia, rowguid, UnmCodigo, OfeID, TraIndicadorOferta, Posicion, ProID, ProCodigo, ProDescripcion, UsaLote, CantidadSolicitada, CantidadSolicitadaDetalle, Cantidad, CantidadDetalle, CantidadDisponibleOriginal, CantidadDisponibleDetalleOriginal) " +
                "select "+producto.Precio.ToString()+", "+producto.Descuento.ToString()+", "+producto.Itbis.ToString()+", "+producto.AdValorem.ToString()+", "+producto.Selectivo.ToString()+", '"+producto.Documento+"', "+producto.EnrSecuencia.ToString()+", "+(producto.IsAdded?"1":"0")+", "+producto.TraSecuencia.ToString()+", '"+Guid.NewGuid().ToString()+"', e.UnmCodigo, e.OfeID, e.TraIndicadorOferta, e.Posicion, e.ProID, e.ProCodigo, e.ProDescripcion, '1', e.CantidadSolicitada, ifnull(e.CantidadSolicitadaDetalle,0), case when e.CantidadSolicitada - d.Cantidad = e.CantidadSolicitada then "+(myParametro.GetParEntregasRepartidorProductoAutoCantidad()?"e.CantidadSolicitada - d.Cantidad":"0")+" else e.CantidadSolicitada - d.Cantidad end as Cantidad, 0, "+producto.CantidadDisponibleOriginal+", "+producto.CantidadDisponibleDetalleOriginal+" " +
                "from EntregasDetalleTemp e " +
                "inner join (select ifnull(sum(Cantidad),0) as Cantidad, ProID, Posicion from EntregasDetalleTemp where UsaLote = '1' and Posicion = " + producto.Posicion.ToString() + " and ProID = " + producto.ProID.ToString() + " and TraSecuencia = "+producto.TraSecuencia.ToString()+" group by ProID, Posicion) d on d.Posicion = e.Posicion and d.ProID = e.ProID " +
                "where UsaLote = '1' and e.CantidadSolicitada - d.Cantidad > 0 and e.ProID = " + producto.ProID.ToString() + " and e.Posicion = " + producto.Posicion.ToString() + " and TraSecuencia = "+producto.TraSecuencia.ToString()+" " +
                "group by e.ProID, e.Posicion, e.CantidadSolicitada, d.Cantidad, e.ProCodigo, e.ProDescripcion, e.ProID, ifnull(e.CantidadSolicitadaDetalle,0) ", new string[] { });

                SqliteManager.GetInstance().Execute("Delete from EntregasDetalleTemp " +
                    "where UsaLote = 1 and trim(ifnull(Lote, '')) != '' and Cantidad = 0 and ifnull(CantidadDetalle, 0) = 0 and ProID = ? and Posicion = ? and TraSecuencia = ? ",
                    new string[] { producto.ProID.ToString(), producto.Posicion.ToString(), producto.TraSecuencia.ToString() });

                if (string.IsNullOrWhiteSpace(producto.Lote) && !ExistsInTempWithLote(producto.ProID, producto.Posicion, null, producto.TraSecuencia))
                {                   
                    map.Add("rowguid", Guid.NewGuid().ToString());
                    map.ExecuteInsert();
                }
            }
            else
            {
                SqliteManager.GetInstance().Execute("update EntregasDetalleTemp set Cantidad = " + (isEditing ? producto.Cantidad.ToString() : "ifnull(Cantidad, 0) + " + producto.Cantidad.ToString()) + (isRecepcionDevolucion ? ", " +
                        "Documento = '" + producto.Documento + "', FechaVencimiento = '" + producto.FechaVencimiento + "', MotIdDevolucion = " + producto.MotIdDevolucion + ", MotDescripcion = '" + producto.MotDescripcion + "', IndicadorMalEstado = " + (producto.IndicadorMalEstado ? 1 : 0) + " " : " ") +
                        "where ProID = " + producto.ProID.ToString() + " and Posicion = " + producto.Posicion.ToString() + " and TraSecuencia = " + producto.TraSecuencia.ToString(), new string[] { });
            }

        }

        public double GetCantidadTotalEntregarVentas(int enrSecuencia, int traSecuencia, int proId)
        {
            var list = new List<Inventarios>();

            list = SqliteManager.GetInstance().Query<Inventarios>("select (ifnull(e.TraCantidadDetalle, 0) / case when ifnull(p.ProUnidades, 0) = 0 then 1 else ProUnidades end) + ifnull(sum(TraCantidad), 0) as invCantidad " +
           "from EntregasRepartidorTransaccionesDetalle e " +
           "inner join Productos p on p.ProID = e.ProID " +
           "where e.EnrSecuencia = ? and e.TraSecuencia = ? and e.ProID = ? and trim(e.RepCodigo) = ? and e.cliid = ? and e.TraIndicadorOferta = 0  " +
           "group by e.ProID, ifnull(p.ProUnidades, 1)",
           new string[] { enrSecuencia.ToString(), traSecuencia.ToString(), proId.ToString(), Arguments.CurrentUser.RepCodigo.Trim(),
                    Arguments.Values.CurrentClient.CliID.ToString() });


            //list = SqliteManager.GetInstance().Query<Inventarios>("select (ifnull(e.TraCantidadDetalle, 0) / case when ifnull(p.ProUnidades, 0) = 0 then 1 else ProUnidades end) + ifnull(TraCantidad, 0) as invCantidad " +
            //"from EntregasRepartidorTransaccionesDetalle e " +
            //"inner join Productos p on p.ProID = e.ProID " +
            //"where e.EnrSecuencia = ? and e.TraSecuencia = ? and e.ProID = ? and trim(e.RepCodigo) = ? and e.cliid = ?  and e.TraIndicadorOferta <> 1 " +
            //"group by e.ProID, ifnull(p.ProUnidades, 1)",
            //new string[] { enrSecuencia.ToString(), traSecuencia.ToString(), proId.ToString(), Arguments.CurrentUser.RepCodigo.Trim(),
            //    Arguments.Values.CurrentClient.CliID.ToString() });



            if (list != null && list.Count > 0)
            {
                return (double)list[0].invCantidad;
            }

            return 0;
        }


        public List<int> GuardarEntrega(List<EntregasRepartidorTransacciones> entregas,
            bool isRecepcionDevolucion,
            string motivoDevolucion = null)
        {
            var entSecuencias = new List<int>();
            
            var parGuardarReciboDeContado = myParametro.GetParEntregasRepartidorGuardarReciboDeContado();
            //si se guardaran entregas del contado y se tiene el parametro para guardar recibo que limpie las tablas temporales.
            if(parGuardarReciboDeContado && entregas.FirstOrDefault(x=>x.ConID == myParametro.GetParConIdFormaPagoContado()) != null)
            {
                new DS_Recibos().ClearTemps();
            }

            foreach (var entrega in entregas)
            {
                var productosNoEntregados = GetProductosNoEntregados(entrega.EnrSecuencia, entrega.TraSecuencia, entrega.TitID);
                var entSecuencia = GuardarEntrega(entrega, isRecepcionDevolucion, productosNoEntregados, motivoDevolucion, true);
                entrega.EntSecuencia = entSecuencia;
                if (entSecuencia != -1)
                {
                    entSecuencias.Add(entSecuencia);
                }
            }

            return entSecuencias;
        }

        public int GuardarEntrega(EntregasRepartidorTransacciones entrega,
            bool isRecepcionDevolucion,
            List<EntregasRepartidorTransaccionesDetalle> productosNoEntregados,
            string motivoDevolucion = null, bool forMultiple = false)
        {
            var entSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("EntregasTransacciones");

            var validarOfertas = myParametro.GetParEntregasRepartidorValidarOfertas() || myParametro.GetParEntregasOfertasTodoONada();

            var productos = GetDetalleToSave(entrega.EnrSecuencia, entrega.TraSecuencia, entrega.TitID, validarOfertas);
            var productosLotes = GetDetalleLoteToSave(entrega.TraSecuencia, validarOfertas);
            if ((productos == null || productos.Count == 0) && myParametro.GetParEntregasMultiples())
            {
                var master2 = new Hash("EntregasRepartidorTransacciones");
                master2.Add("enrEstatusEntrega", 2);
                master2.Add("EntFechaActualizacion", Functions.CurrentDate());
                master2.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                master2.ExecuteUpdate("EnrSecuencia = " + entrega.EnrSecuencia + " and EnrSecuenciaDetalle = " + entrega.EnrSecuenciaDetalle);

                if (productosNoEntregados.Count > 0 && !isRecepcionDevolucion)
                {
                    CrearDevolucionProductosNoEntregados(productosNoEntregados, entrega, motivoDevolucion);
                }

                if (!string.IsNullOrWhiteSpace(motivoDevolucion) && myParametro.GetParEntregasMultiples())
                {
                    var entregasRechazadas = GetEntregasRechazadasSinMotivo();

                    if (entregasRechazadas.Count > 0)
                    {
                        int.TryParse(motivoDevolucion, out int motId);
                        ActualizarMotivoEntregaRechazada(motId, entregasRechazadas);
                    }
                }
                return -1;
            }

            new DS_Visitas().ActualizarVisitaEfectiva(Arguments.Values.CurrentVisSecuencia);

            var ent = new Hash("EntregasTransacciones");
            ent.Add("EntEstatus", 1);
            ent.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            ent.Add("EntSecuencia", entSecuencia);
            ent.Add("TitID", entrega.TitID);
            ent.Add("CliID", entrega.CliID);
            ent.Add("EntFecha", Functions.CurrentDate());            
            ent.Add("RepNombre", entrega.RepNombre);
            ent.Add("RepTelefono", entrega.RepTelefono);

            /*var precioTotal = productos.Sum(x => (x.TraPrecio + x.TraAdValorem + x.TraSelectivo) * x.CantidadEntregada);
            var descuentoTotal = productos.Sum(x => x.TraDescuento * x.CantidadEntregada);
            var itbisTotal = productos.Sum(x => (x.TraPrecio + x.TraAdValorem + x.TraSelectivo) * (x.TraItbis / 100.0));

            var total = precioTotal - descuentoTotal + itbisTotal;*/

            var totales = GetTempTotales(false, entrega.TraSecuencia, true);
            int cantidadLineasLotes = 0;
            if(productosLotes != null)
            {
                cantidadLineasLotes = productosLotes.Count;
            }
            ent.Add("EntTotal", totales.Total);
            ent.Add("EntNCF", entrega.VenNCF);
            ent.Add("EntIndicadorCompleto", 0);
            ent.Add("CuaSecuencia", Arguments.Values.CurrentCuaSecuencia);
            ent.Add("EntTipo", isRecepcionDevolucion ? 2 : 1);
            ent.Add("RepVendedor", entrega.RepVendedor);
            ent.Add("VenSecuencia", entrega.TraSecuencia);
            ent.Add("EntCantidadDetalle", productos.Count);
            ent.Add("VisSecuencia", Arguments.Values.CurrentVisSecuencia);
            ent.Add("ConID", Arguments.Values.CurrentClient.ConID);
            ent.Add("MonCodigo", Arguments.Values.CurrentClient.MonCodigo);
            ent.Add("mbVersion", Functions.AppVersion);
            ent.Add("EntFechaActualizacion", Functions.CurrentDate());
            ent.Add("EnrSecuencia", entrega.EnrSecuencia);
            //ent.Add("UsuInicioSesion", "mdsoft");
            ent.Add("rowguid", Guid.NewGuid().ToString());
            ent.Add("RepSupervisor", Arguments.CurrentUser.RepSupervisor);
            ent.Add("EntCantidadImpresion", 0);
            ent.Add("EntCantidadDetalleLote", cantidadLineasLotes);

            if (Arguments.Values.CurrentSector != null)
            {
                if(myParametro.GetParEntregasMultiples() && !string.IsNullOrEmpty(entrega.SecCodigo))
                {
                    ent.Add("SecCodigo", entrega.SecCodigo);
                }
                else
                {
                    ent.Add("SecCodigo", Arguments.Values.CurrentSector.SecCodigo);
                }
            }
            else if (!string.IsNullOrEmpty(entrega.SecCodigo))
            {
                ent.Add("SecCodigo", entrega.SecCodigo);
            }

            ent.ExecuteInsert();

            var master = new Hash("EntregasRepartidorTransacciones");
            master.Add("enrEstatusEntrega", 2);
            master.Add("EntFechaActualizacion", Functions.CurrentDate());
            master.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            master.ExecuteUpdate("EnrSecuencia = " + entrega.EnrSecuencia + " and EnrSecuenciaDetalle = " + entrega.EnrSecuenciaDetalle);

            var parMultiAlmacenes = myParametro.GetParUsarMultiAlmacenes();
            var almIdDespacho = myParametro.GetParAlmacenIdParaDespacho();
            var almIdDev = myParametro.GetParAlmacenIdParaDevolucion();
           // var almIdMalEstado = myParametro.GetParAlmacenIdParaMalEstado();
            //var parTipoComprobanteFiscals = myParametro.GetParTipoComprobanteFiscal();
            //var almIdNoVenta = myParametro.GetAlmacenIdProductosNoDevolucion();

            var myInv = new DS_Inventarios();

            // var motivos = new DS_Devoluciones().GetMotivosDevolucion();
            var parInvSinLote = myParametro.GetParEntregasControlInventarioSinLotes();

            foreach (var d in productos)
            {
                var det = new Hash("EntregasTransaccionesDetalle");
                det.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                det.Add("EntSecuencia", entSecuencia);
                det.Add("EntPosicion", d.TraPosicion);
                det.Add("ProID", d.ProID);
                det.Add("EntPrecio", d.TraPrecio);
                det.Add("EntCantidad", d.CantidadEntregada);
                det.Add("EntCantidadDetalle", d.CantidadEntregadaDetalle);
                det.Add("EntCantidadSolicitada", d.TraCantidad);
                det.Add("EntCantidadDetalleSolicitada", d.TraCantidadDetalle);
                det.Add("EntDescuento", d.TraDescuento);
                det.Add("EntDescPorciento", d.TraDesPorciento);
                det.Add("EntItbis", d.TraItbis);
                det.Add("EntSelectivo", d.TraSelectivo);
                det.Add("EntAdValorem", d.TraAdValorem);
                det.Add("EntIndicadorOferta", d.TraIndicadorOferta);
                //det.Add("UsuInicioSesion", "mdsoft");
                det.Add("EntFechaActualizacion", Functions.CurrentDate());
                det.Add("rowguid", Guid.NewGuid().ToString());
                det.Add("RepSupervisor", Arguments.CurrentUser.RepSupervisor);
                det.Add("UnmCodigo", d.UnmCodigo);
                det.ExecuteInsert();

                if (parMultiAlmacenes && (!d.UsaLote || parInvSinLote))
                {
                    if (isRecepcionDevolucion)
                    {
                        myInv.AgregarInventario(d.ProID, d.CantidadEntregada, (int)d.CantidadEntregadaDetalle, almIdDev);
                    }
                    else
                    {
                        myInv.RestarInventario(d.ProID, d.CantidadEntregada, (int)d.CantidadEntregadaDetalle, almIdDespacho);
                    }
                }
            }

            //guardar el detalle de los lotes            
            foreach (var lot in productosLotes)
            {
                var lote = new Hash("EntregasTransaccionesDetalleLotes");
                lote.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                lote.Add("EntSecuencia", entSecuencia);
                lote.Add("EntPosicion", lot.Posicion);
                lote.Add("EntLote", lot.Lote);
                lote.Add("EntCantidad", lot.Cantidad);
                lote.Add("EntCantidadDetalle", lot.CantidadDetalle);
                lote.Add("RepSupervisor", Arguments.CurrentUser.RepSupervisor);
                lote.Add("UsuInicioSesion", "mdsoft");
                lote.Add("EntFechaActualizacion", Functions.CurrentDate());
                lote.Add("rowguid", Guid.NewGuid().ToString());
                lote.ExecuteInsert();

                if (parMultiAlmacenes && !parInvSinLote)
                {
                    if (isRecepcionDevolucion)
                    {
                        myInv.AgregarInventario(lot.ProID, lot.Cantidad, (int)lot.CantidadDetalle, almIdDev, lot.Lote);
                    }
                    else
                    {
                        myInv.RestarInventario(lot.ProID, lot.Cantidad, (int)lot.CantidadDetalle, almIdDespacho, lot.Lote);
                    }
                }
            }

            DS_RepresentantesSecuencias.UpdateSecuencia("EntregasTransacciones", entSecuencia);

            var parGuardarReciboDeContado = myParametro.GetParEntregasRepartidorGuardarReciboDeContado();

            if(parGuardarReciboDeContado && entrega.ConID == myParametro.GetParConIdFormaPagoContado() && entrega.ConID != -1)
            {
                var cxc = new Hash("CuentasxCobrar");
                cxc.Add("CxcReferencia", "ENT-" + Arguments.CurrentUser.RepCodigo + "-" + entSecuencia);
                cxc.Add("CxcTipoTransaccion", 1);
                cxc.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                cxc.Add("CxcDias", 0);
                cxc.Add("CxcSIGLA", "FAT");
                cxc.Add("CliID", Arguments.Values.CurrentClient.CliID);
                cxc.Add("CxcFecha", Functions.CurrentDate());
                cxc.Add("CxcDocumento", entrega.venNumeroERP);//Arguments.CurrentUser.RepCodigo + "-" + entSecuencia);

                cxc.Add("CxcBalance", totales.Total);
                cxc.Add("CxcMontoSinItbis", totales.SubTotal);
                cxc.Add("CxcMontoTotal", totales.Total);
                cxc.Add("MonCodigo", Arguments.Values.CurrentClient.MonCodigo);
                cxc.Add("AreaCtrlCredit", 0);
                cxc.Add("CxcNotaCredito", 0);
                cxc.Add("CXCNCF", "");
                cxc.Add("rowguid", Guid.NewGuid().ToString());
                cxc.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                cxc.Add("CueFechaActualizacion", Functions.CurrentDate());

                var myCxc = new DS_CuentasxCobrar();

                cxc.Add("CxcFechaVencimiento", myCxc.GetCxcFechaVencimiento(entrega.ConID));
                cxc.Add("ConID", entrega.ConID);
                cxc.ExecuteInsert();

                var reciboToSave = new RecibosDocumentosTemp();
                reciboToSave.FechaSinFormatear = Functions.CurrentDate();
                reciboToSave.Fecha = Functions.CurrentDate("dd-MM-yyyy");
                reciboToSave.Documento = entrega.venNumeroERP;//Arguments.CurrentUser.RepCodigo + "-" + entSecuencia;
                reciboToSave.Referencia =  "ENT-" + Arguments.CurrentUser.RepCodigo + "-" + entSecuencia;
                reciboToSave.Sigla = "FAT";
                reciboToSave.Aplicado = totales.Total;
                reciboToSave.Descuento = 0;
                reciboToSave.MontoTotal = totales.Total;
                reciboToSave.Balance = totales.Total;
                reciboToSave.Pendiente = 0;
                reciboToSave.Estado = "Saldo";
                reciboToSave.Credito = 0;
                reciboToSave.FechaIngles = Functions.CurrentDate("MM-dd-yyyy");
                reciboToSave.Origen = 1;
                reciboToSave.MontoSinItbis = totales.SubTotal;
                reciboToSave.DescPorciento = 0;
                reciboToSave.AutSecuencia = 0;
                reciboToSave.FechaDescuento = Functions.CurrentDate("dd-MM-yyyy");
                reciboToSave.Dias = 0;
                reciboToSave.DescuentoFactura = 0;
                reciboToSave.Clasificacion = "";
                reciboToSave.FechaVencimiento = Functions.CurrentDate();
                reciboToSave.Retencion = 0;
                reciboToSave.CXCNCF = "";
                reciboToSave.cxcComentario = "";
                reciboToSave.RecTipo = 1;                

                SqliteManager.GetInstance().Insert(reciboToSave);
            }
            else if (myParametro.GetParEntregasRepartidorGuardarRecibo())
            {
                GuardarReciboFromEntrega(productos, entrega);
            }

            if (productosNoEntregados.Count > 0 && !isRecepcionDevolucion)
            {
                CrearDevolucionProductosNoEntregados(productosNoEntregados, entrega, motivoDevolucion);
            }

            if (!string.IsNullOrWhiteSpace(motivoDevolucion) && myParametro.GetParEntregasMultiples())
            {
                var entregasRechazadas = GetEntregasRechazadasSinMotivo();

                if (entregasRechazadas.Count > 0)
                {
                    int.TryParse(motivoDevolucion, out int motId);
                    ActualizarMotivoEntregaRechazada(motId, entregasRechazadas);
                }
            }

            if (DS_RepresentantesParametros.GetInstance().GetParVisitasResultados())
            {
                ActualizarVisitasResultados(isRecepcionDevolucion);
            }
            

            if (!forMultiple)
            {
                DeleteFromTemp();
            }

            return entSecuencia;
        }

        private void ActualizarVisitasResultados(bool isReturn)
        {
            var list = SqliteManager.GetInstance().Query<VisitasResultados>("select count(*) as VisCantidadTransacciones, " +
                "sum(((d.EntPrecio + d.EntAdValorem + d.EntSelectivo) - d.EntDescuento) * ((case when d.EntCantidadDetalle > 0 then d.EntCantidadDetalle / o.ProUnidades else 0 end) + d.EntCantidad)) as VisMontoSinItbis, sum(((d.EntItbis / 100.0) * ((d.EntPrecio + d.EntAdValorem + d.EntSelectivo) - d.EntDescuento)) * ((case when d.EntCantidadDetalle > 0 then d.EntCantidadDetalle / o.ProUnidades else 0 end) + d.EntCantidad)) as VisMontoItbis from EntregasTransacciones p " +
                "inner join EntregasTransaccionesDetalle d on d.RepCodigo = p.RepCodigo and d.EntSecuencia = p.EntSecuencia " +
                "inner join Productos o on o.ProID = d.ProID " +
                "where p.EntTipo = "+(isReturn ? 2 : 1)+" and  p.VisSecuencia = ? and p.RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'", new string[] { Arguments.Values.CurrentVisSecuencia.ToString() });

            if (list != null && list.Count > 0)
            {
                var item = list.FirstOrDefault();

                item.VisMontoTotal = item.VisMontoSinItbis + item.VisMontoItbis;
                item.VisComentario = "";
                item.TitID = isReturn ? 57 : 27;

                new DS_Visitas().GuardarVisitasResultados(item);
            }
        }

        private List<Devoluciones> GetDevolucionesSinMotivo()
        {
            return SqliteManager.GetInstance().Query<Devoluciones>("select distinct d.DevSecuencia as DevSecuencia from Devoluciones d " +
                "inner join DevolucionesDetalle dd on dd.RepCodigo = d.RepCodigo and dd.DevSecuencia = d.DevSecuencia " +
                "where ifnull(dd.MotID, 0) = 0 and d.DevEstatus <> 0 and d.DevTipo = 3 and trim(d.RepCodigo) = ? and d.CliID = ? " , 
                new string[] { Arguments.CurrentUser.RepCodigo.Trim(), Arguments.Values.CurrentClient.CliID.ToString() });
        }

        private void GuardarReciboFromEntrega(List<EntregasRepartidorTransaccionesDetalle> productos, EntregasRepartidorTransacciones entrega)
        {
            var dsRec = new DS_Recibos();

            var totales = GetTempTotales(false, entrega.TraSecuencia);

            dsRec.ClearTemps();

            var reciboToSave = new RecibosDocumentosTemp();
            reciboToSave.FechaSinFormatear = Functions.CurrentDate();
            reciboToSave.Fecha = Functions.CurrentDate("dd-MM-yyyy");
            reciboToSave.Documento = entrega.VenDPPNumeroERPDocum;
            reciboToSave.Referencia = entrega.venNumeroERP;
            reciboToSave.Sigla = "FAT";
            reciboToSave.Aplicado = totales.Total;
            reciboToSave.Descuento = totales.Descuento;
            reciboToSave.MontoTotal = totales.Total;
            reciboToSave.Balance = totales.Total;
            reciboToSave.Pendiente = 0;
            reciboToSave.Estado = "Saldo";
            reciboToSave.Credito = 0;
            reciboToSave.FechaIngles = Functions.CurrentDate("MM-dd-yyyy");
            reciboToSave.Origen = 1;
            reciboToSave.MontoSinItbis = totales.SubTotal;
            reciboToSave.DescPorciento = 0;
            reciboToSave.AutSecuencia = 0;
            reciboToSave.FechaDescuento = Functions.CurrentDate("dd-MM-yyyy");
            reciboToSave.Dias = 0;
            reciboToSave.DescuentoFactura = 0;
            reciboToSave.Clasificacion = "";
            reciboToSave.FechaVencimiento = Functions.CurrentDate();
            reciboToSave.Retencion = 0;
            reciboToSave.CXCNCF = "";
            reciboToSave.cxcComentario = "";
            reciboToSave.RecTipo = 4;

            SqliteManager.GetInstance().Insert(reciboToSave);

            var formaPago = new FormasPagoTemp();
            formaPago.MonCodigo = Arguments.Values.CurrentClient.MonCodigo;
            formaPago.RefSecuencia = 1;
            formaPago.FormaPago = "EFECTIVO";
            formaPago.Futurista = "No";
            formaPago.Prima = totales.Total;
            formaPago.Valor = totales.Total;
            formaPago.rowguid = Guid.NewGuid().ToString();
            formaPago.ForID = 1;

            var mon = new DS_Monedas().GetMoneda(Arguments.Values.CurrentClient.MonCodigo);

            if (mon != null)
            {
                formaPago.Tasa = mon.MonTasa;
            }

            SqliteManager.GetInstance().Insert(formaPago);

            dsRec.GuardarRecibo(entrega.EnrSecuencia.ToString(), mon, forceRecTipo:"4");
        }

        private void CrearDevolucionProductosNoEntregados(List<EntregasRepartidorTransaccionesDetalle> productos,
            EntregasRepartidorTransacciones entrega,
            string motivoDevolucion)
        {
            int devSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Devoluciones");

            var parMultiAlmacenes = myParametro.GetParUsarMultiAlmacenes();
            var almIdDespacho = myParametro.GetParAlmacenIdParaDespacho();
            var almIdDev = myParametro.GetParAlmacenIdParaDevolucion();
            var almIdMalEstado = myParametro.GetParAlmacenIdParaMalEstado();
            var parTipoComprobanteFiscal = myParametro.GetParTipoComprobanteFiscal();
            var almIdNoVenta = myParametro.GetAlmacenIdProductosNoDevolucion();
            var useAlmIdNoVenta = false;

            if (parMultiAlmacenes && !string.IsNullOrWhiteSpace(entrega.VenNCF) && !string.IsNullOrWhiteSpace(parTipoComprobanteFiscal))
            {
                if (entrega.VenNCF.Length >= 3 && entrega.VenNCF.ToUpper().Substring(1, 2).Equals(parTipoComprobanteFiscal))
                {
                    useAlmIdNoVenta = true;
                }
            }

            var myInv = new DS_Inventarios();

            var dev = new Hash("Devoluciones");
            dev.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            dev.Add("DevTipo", 3);
            dev.Add("DevSecuencia", devSecuencia);
            dev.Add("CliID", entrega.CliID);
            dev.Add("DevFecha", Functions.CurrentDate());
            dev.Add("DevEstatus", 1);
            dev.Add("DevTotal", productos.Count);
            dev.Add("DevReferencia", entrega.venNumeroERPDocum);
            dev.Add("DevCintillo", entrega.TraSecuencia);
            dev.Add("DevNCF", entrega.VenNCF);
            dev.Add("DevAccion", "");
            dev.Add("CuaSecuencia", Arguments.Values.CurrentCuaSecuencia);
            dev.Add("VisSecuencia", Arguments.Values.CurrentVisSecuencia);
            dev.Add("OrvCodigo", "");
            dev.Add("OfvCodigo", "");
            dev.Add("MonCodigo", Arguments.Values.CurrentClient.MonCodigo);
            //dev.Add("SecCodigo", Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.SecCodigo : null);
            if (Arguments.Values.CurrentSector != null)
            {
                if (myParametro.GetParEntregasMultiples() && !string.IsNullOrEmpty(entrega.SecCodigo))
                {
                    dev.Add("SecCodigo", entrega.SecCodigo);
                }
                else
                {
                    dev.Add("SecCodigo", Arguments.Values.CurrentSector.SecCodigo);
                }
            }
            else if (!string.IsNullOrEmpty(entrega.SecCodigo))
            {
                dev.Add("SecCodigo", entrega.SecCodigo);
            }
            dev.Add("DevFechaActualizacion", Functions.CurrentDate());
            dev.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            dev.Add("rowguid", Guid.NewGuid());
            dev.Add("mbVersion", Functions.AppVersion);
            dev.Add("RepSupervisor", Arguments.CurrentUser.RepSupervisor);
            dev.Add("DevCantBultos", productos.Count);
            dev.Add("EnrSecuencia", entrega.EnrSecuencia);
            if (myParametro.GetParEntregasRepartidor() != 3)
            {
                dev.ExecuteInsert();
            }
            
            foreach (var dd in productos)
            {
                var devd = new Hash("DevolucionesDetalle");
                devd.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                devd.Add("DevTipo", 3);
                devd.Add("DevSecuencia", devSecuencia);
                devd.Add("DevPosicion", dd.TraPosicion);
                devd.Add("ProID", dd.ProID);
                devd.Add("DevCantidad", dd.TraCantidad);
                devd.Add("DevCantidadDetalle", dd.TraCantidadDetalle);
                //devd.Add("DevDocumento", "");
                devd.Add("CliID", entrega.CliID);
                devd.Add("DevPrecio", dd.TraPrecio);
                devd.Add("DevDescuento", dd.TraDescuento);
                devd.Add("DevItebis", dd.TraItbis);
                devd.Add("DevTotalItebis", (dd.TraPrecio * (dd.TraItbis / 100.0) * dd.TraCantidad));
                devd.Add("DevTotalDescuento", dd.TraDescuento * dd.TraCantidad);
                devd.Add("DevSelectivo", dd.TraSelectivo);
                devd.Add("DevAdvalorem", dd.TraAdValorem);
                //devd.Add("DevAccion", null);
                devd.Add("DevEstatus", 1);

                if (!string.IsNullOrWhiteSpace(motivoDevolucion))
                {
                    devd.Add("MotID", motivoDevolucion);
                }

                devd.Add("DevIndicadorOferta", dd.TraIndicadorOferta);
                devd.Add("DevFecha", Functions.CurrentDate());
                devd.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                devd.Add("DevFechaActualizacion", Functions.CurrentDate());
                devd.Add("rowguid", Guid.NewGuid().ToString());
                devd.Add("DevGrupo", null);
                devd.Add("RepSupervisor", Arguments.CurrentUser.RepSupervisor);
                devd.Add("DevComentario", "Producto devuelto desde entrega repartidor");
                devd.Add("UnmCodigo", dd.UnmCodigo);
                devd.Add("DevDocumento", dd.OfeID);
                if (myParametro.GetParEntregasRepartidor() != 3)
                {
                    devd.ExecuteInsert();
                }

                if (parMultiAlmacenes && almIdDev > 0)
                {
                    if (myInv.HayExistencia(dd.ProID, dd.TraCantidad, almId: almIdDespacho))
                    {
                        myInv.AgregarInventario(dd.ProID, dd.TraCantidad, (int)dd.TraCantidadDetalle, useAlmIdNoVenta ? almIdNoVenta : almIdDev);
                        myInv.RestarInventario(dd.ProID, dd.TraCantidad, (int)dd.TraCantidadDetalle, almIdDespacho);
                    }
                    else
                    {
                        throw new Exception("La cantidad solicitada para: " + new DS_Productos().GetProductCodigoAndDescByProId(dd.ProID) + " es mayor que la cantidad en inventario.");
                    }
                }
            }


            DS_RepresentantesSecuencias.UpdateSecuencia("Devoluciones", devSecuencia);
        }

        public Totales GetTempTotales(bool isAdded = false, int traSecuencia = -1, bool validarLote = false)
        {
            string AdValorem = "AdValorem";

            if (myParametro.GetParADVALOREMTIPO() == 1 || myParametro.GetParADVALOREMTIPO() == -1)
            {
                AdValorem = " (CAST(Precio AS REAL) * (CAST(IFNULL(AdValorem, 0.0) AS REAL) / 100.0)) ";
            }

            var where = "";

            if(traSecuencia != -1)
            {
                where = " and t.TraSecuencia = " + traSecuencia.ToString();
            }

            var query = "select SUM(((CAST(ifnull(CantidadDetalle, 0)AS REAL)  / case when ifnull(P.ProUnidades, 0) = 0 then 1 else ifnull(P.ProUnidades, 1) end) + Cantidad)) as CantidadTotal, " +
                " SUM(ifnull(Selectivo, 0) * ifnull(Cantidad, 0)) as Selectivo, " +
                " IFNULL(SUM((CAST(IFNULL(" + AdValorem + ", 0.0) AS REAL) * ((CAST(IFNULL(CantidadDetalle, 0.0) AS REAL) / CAST(IFNULL(p.ProUnidades, 0.0) AS REAL))+ IFNULL(Cantidad, 0.0)))), 0.0) as AdValorem,  " +
                " SUM((Precio + ifnull(Selectivo,0) + ifnull(" + AdValorem + ",0) /*- ifnull(Descuento, 0.0) */) * ((CAST(ifnull(CantidadDetalle, 0) AS REAL)  / case when ifnull(P.ProUnidades, 0) = 0 then 1 else ifnull(P.ProUnidades, 1) end) + Cantidad)) as SubTotal," +
                " SUM(((((Precio + Selectivo + " + AdValorem + ") - ifnull(Descuento, 0)) + round(((Precio + ifnull(Selectivo, 0) + ifnull(" + AdValorem + ", 0)) - ifnull(Descuento, 0))) * (Itbis / 100.0))) * ((CAST(ifnull(CantidadDetalle, 0) AS REAL)  / case when ifnull(CAST(P.ProUnidades AS REAL), 0) = 0 then 1 else CAST(ifnull(P.ProUnidades, 1) AS REAL) end) + Cantidad)) as Total, " +
                " SUM((ifnull(Descuento, 0) * ((Cast(ifnull(CantidadDetalle, 0) as real)  / case when ifnull(P.ProUnidades, 1) = 0 then 1 else ifnull(P.ProUnidades, 1) end) + Cantidad))) as Descuento," +
                "  SUM((((Precio + ifnull(Selectivo,0) + ifnull(" + AdValorem + ",0)) - ifnull(Descuento, 0.0)) * (Itbis / 100.0)) *  ((CAST(ifnull(CantidadDetalle, 0)AS REAL)  / case when ifnull(P.ProUnidades, 0) = 0 then 1 else ifnull(P.ProUnidades, 1) end) + Cantidad)) as Itbis " +
                " from EntregasDetalleTemp t " +
                "inner join Productos p on p.proID = t.ProID " +
                "where t.Cantidad > 0 and ifnull(TraIndicadorOferta, 0) = 0 "+where+" "+(validarLote? " and (case when ifnull(t.UsaLote, 0) = 1 then ifnull(Lote, '') != '' else 1=1 end) " : "")+" "+(isAdded?" AND ifnull(IsAdded, 0) = 1 ":"")+" ";

            var list = SqliteManager.GetInstance().Query<Totales>(query, new string[] { Arguments.CurrentUser.RepCodigo.Trim() });

            double DescuentoOfertas = 0;

            var ofeDesc = SqliteManager.GetInstance().Query<Totales>("select sum(Precio * Cantidad) as DescuentoOfertas, SUM(((CAST(ifnull(CantidadDetalle, 0)AS REAL)  / case when ifnull(P.ProUnidades, 0) = 0 then 1 else ifnull(P.ProUnidades, 1) end) + Cantidad)) as CantidadTotal from EntregasDetalleTemp t " +
                "inner join Productos P on P.ProID = t.ProID " +
                "where ifnull(t.TraIndicadorOferta, 0) = 1 "+where+" " + (validarLote?" and case when ifnull(UsaLote, 0) = 1 then ifnull(Lote, '') != '' else 1=1 end ":"") + (isAdded?" AND ifnull(IsAdded, 0) = 1 ":""), 
                new string[] {});

            if (ofeDesc != null && ofeDesc.Count > 0)
            {
                DescuentoOfertas = ofeDesc[0].DescuentoOfertas;
            }

            if (list.Count > 0)
            {
                var montototal = Math.Round(list[0].SubTotal + list[0].Itbis - list[0].Descuento, 2);//list[0].Total;//list[0].SubTotal + list[0].Itbis - list[0].Descuento;
                list[0].Total = montototal;
                var total = list[0];
                total.DescuentoOfertas = DescuentoOfertas;

                if(ofeDesc.Count > 0)
                {
                    total.CantidadTotal += ofeDesc[0].CantidadTotal;
                }

                return total;
            }

            return new Totales();
        }

        private Totales GetEntregaTotales(int traSecuencia)
        {
            string AdValorem = "EntAdValorem";

            if (myParametro.GetParADVALOREMTIPO() == 1 || myParametro.GetParADVALOREMTIPO() == -1)
            {
                AdValorem = " (CAST(EntPrecio AS REAL) * (CAST(IFNULL(EntAdValorem, 0.0) AS REAL) / 100.0)) ";
            }

            var where = "";

            if (traSecuencia != -1)
            {
                where = " and t.EntSecuencia = " + traSecuencia.ToString();
            }

            var query = "select SUM(((CAST(ifnull(EntCantidadDetalle, 0)AS REAL)  / case when ifnull(P.ProUnidades, 0) = 0 then 1 else ifnull(P.ProUnidades, 1) end) + EntCantidad)) as CantidadTotal, " +
                " SUM(ifnull(EntSelectivo, 0) * ifnull(EntCantidad, 0)) as Selectivo, " +
                " IFNULL(SUM((CAST(IFNULL(" + AdValorem + ", 0.0) AS REAL) * ((CAST(IFNULL(EntCantidadDetalle, 0.0) AS REAL) / CAST(IFNULL(p.ProUnidades, 0.0) AS REAL))+ IFNULL(EntCantidad, 0.0)))), 0.0) as AdValorem,  " +
                " SUM((EntPrecio + ifnull(EntSelectivo,0) + ifnull(" + AdValorem + ",0) /*- ifnull(EntDescuento, 0.0) */) * ((CAST(ifnull(EntCantidadDetalle, 0) AS REAL)  / case when ifnull(P.ProUnidades, 0) = 0 then 1 else ifnull(P.ProUnidades, 1) end) + EntCantidad)) as SubTotal," +
                " SUM(((((EntPrecio + EntSelectivo + " + AdValorem + ") - ifnull(EntDescuento, 0)) + round(((EntPrecio + ifnull(EntSelectivo, 0) + ifnull(" + AdValorem + ", 0)) - ifnull(EntDescuento, 0))) * (EntItbis / 100.0))) * ((CAST(ifnull(EntCantidadDetalle, 0) AS REAL)  / case when ifnull(CAST(P.ProUnidades AS REAL), 0) = 0 then 1 else CAST(ifnull(P.ProUnidades, 1) AS REAL) end) + EntCantidad)) as Total, " +
                " SUM((ifnull(EntDescuento, 0) * ((Cast(ifnull(EntCantidadDetalle, 0) as real)  / case when ifnull(P.ProUnidades, 1) = 0 then 1 else ifnull(P.ProUnidades, 1) end) + EntCantidad))) as Descuento," +
                "  SUM((((EntPrecio + ifnull(EntSelectivo,0) + ifnull(" + AdValorem + ",0)) - ifnull(EntDescuento, 0.0)) * (EntItbis / 100.0)) *  ((CAST(ifnull(EntCantidadDetalle, 0)AS REAL)  / case when ifnull(P.ProUnidades, 0) = 0 then 1 else ifnull(P.ProUnidades, 1) end) + EntCantidad)) as Itbis " +
                " from EntregasTransaccionesDetalle t " +
                "inner join Productos p on p.proID = t.ProID " +
                "where t.EntCantidad > 0 and ifnull(EntIndicadorOferta, 0) = 0 " + where;

            var list = SqliteManager.GetInstance().Query<Totales>(query, new string[] { Arguments.CurrentUser.RepCodigo.Trim() });

            double DescuentoOfertas = 0;

            var ofeDesc = SqliteManager.GetInstance().Query<Totales>("select sum(EntPrecio * EntCantidad) as DescuentoOfertas, SUM(((CAST(ifnull(EntCantidadDetalle, 0)AS REAL)  / case when ifnull(P.ProUnidades, 0) = 0 then 1 else ifnull(P.ProUnidades, 1) end) + EntCantidad)) as CantidadTotal from EntregasTransaccionesDetalle t " +
                "inner join Productos P on P.ProID = t.ProID where ifnull(t.EntIndicadorOferta, 0) = 1 " + where, new string[] { });

            if (ofeDesc != null && ofeDesc.Count > 0)
            {
                DescuentoOfertas = ofeDesc[0].DescuentoOfertas;
            }

            if (list.Count > 0)
            {
                var montototal = Math.Round(list[0].SubTotal + list[0].Itbis - list[0].Descuento, 2);//list[0].Total;//list[0].SubTotal + list[0].Itbis - list[0].Descuento;
                list[0].Total = montototal;
                var total = list[0];
                total.DescuentoOfertas = DescuentoOfertas;

                if (ofeDesc.Count > 0)
                {
                    total.CantidadTotal += ofeDesc[0].CantidadTotal;
                }

                return total;
            }

            return new Totales();
        }

        private List<EntregasRepartidorTransaccionesDetalle> GetDetalleToSave(int enrSecuencia, int traSecuencia, int titId, bool validarOfertas = false)
        {
            var where = "";
            if (validarOfertas)
            {
                where = " and case when ifnull(t.TraIndicadorOferta, 0) = 1 then t.OfeID not in " +
                    "(select distinct ProID from EntregasDetalleTemp where ProID = t.OfeID and ifnull(TraIndicadorOferta, 0) = 0 " +
                    "group by ProID, Posicion having (sum(ifnull(Cantidad, 0)) < ifnull(CantidadSolicitada, 0) or (ifnull(UsaLote, 0) = 1 and ifnull(Lote, '') = '')) ) else 1=1 end ";

                if (myParametro.GetParEntregasOfertasTodoONada())
                {
                    where = " and case when ifnull(t.TraIndicadorOferta, 0) = 0 then t.ProID not in " +
                   "(select distinct OfeID from EntregasDetalleTemp where ProID = t.OfeID and TraSecuencia = t.TraSecuencia and ifnull(TraIndicadorOferta, 0) = 1 " +
                   "group by ProID, Posicion, TraSecuencia having (sum(ifnull(Cantidad, 0)) < ifnull(CantidadSolicitada, 0) or (ifnull(UsaLote, 0) = 1 and ifnull(Lote, '') = '')) ) else 1=1 end ";
                }
            }

            return SqliteManager.GetInstance().Query<EntregasRepartidorTransaccionesDetalle>(
                "select e.EnrSecuencia as EnrSecuencia, e.UnmCodigo as UnmCodigo, e.OfeID as OfeID, t.UsaLote as UsaLote, e.TraIndicadorOferta as TraIndicadorOferta, TraPosicion, e.ProID, TraCantidad, TraCantidadDetalle, TraPrecio, TraItbis, TraSelectivo, TraAdValorem, TraDescuento, TraIndicadorCompleto, " +
                "CedCodigo, TraDesPorciento, TraTipoOferta, TraCantidadInventario, AutSecuencia, TraFlete, RepSupervisor, " +
                "sum(t.Cantidad) as CantidadEntregada, sum(t.CantidadDetalle) as CantidadEntregadaDetalle, t.MotIdDevolucion as MotIdDevolucion, t.Documento as Documento, t.FechaVencimiento as FechaVencimiento  " +
                "from EntregasRepartidorTransaccionesDetalle e " +
                "inner join EntregasDetalleTemp t on t.ProID = e.ProID and t.Posicion = e.TraPosicion and t.TraSecuencia = e.TraSecuencia " +
                "where e.EnrSecuencia = ? and e.TraSecuencia = ? and e.TitID = ? and trim(e.RepCodigo) = ? " +
                " "+(myParametro.GetParEntregasAgregarCero()? "" : " and t.Cantidad > 0 ") +" and ((ifnull(t.UsaLote, 1) = 1 and ifnull(t.Lote, '') != '') OR ifnull(t.UsaLote, 0) = 0) " + where + " " + 
                "group by TraPosicion, e.ProID, TraCantidad, TraCantidadDetalle, TraPrecio, TraItbis, TraSelectivo, TraAdValorem, TraDescuento, TraIndicadorCompleto, e.TraIndicadorOferta, CedCodigo, e.OfeID, TraDesPorciento, TraTipoOferta, e.UnmCodigo, TraCantidadInventario, AutSecuencia, TraFlete, RepSupervisor",
                new string[] { enrSecuencia.ToString(), traSecuencia.ToString(), titId.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
        }

        private List<EntregasDetalleTemp> GetDetalleLoteToSave(int traSecuencia, bool validarOfertas)
        {
            var where = "";
            if (validarOfertas)
            {
                where = " and case when ifnull(t.TraIndicadorOferta, 0) = 1 then t.OfeID not in " +
                    "(select distinct ProID from EntregasDetalleTemp where ProID = t.OfeID and ifnull(TraIndicadorOferta, 0) = 0 " +
                    "group by ProID, Posicion, TraSecuencia having (sum(ifnull(Cantidad, 0)) < ifnull(CantidadSolicitada, 0) " +
                    "or (ifnull(UsaLote, 0) = 1 and ifnull(Lote, '') = ''))) else 1=1 end ";
            }

            return SqliteManager.GetInstance().Query<EntregasDetalleTemp>("select * from EntregasDetalleTemp t " +
                "where UsaLote = 1 and ifnull(trim(Lote), '') != '' " + where + " and t.TraSecuencia = " + traSecuencia.ToString(), new string[] { });
        }

        public bool IsSomethingAdded(bool forceValidationLote = false)
        {
            var where = " and ((ifnull(UsaLote, 0) = 1 and ifnull(Lote, '') != '') or ifnull(UsaLote, 0) = 0) ";

            if (myParametro.GetParConducesUsarRowSinDialog() && !forceValidationLote)
            {
                where = "";
            }

            return SqliteManager.GetInstance().Query<EntregasDetalleTemp>("select ProID from EntregasDetalleTemp " +
                "where (ifnull(Cantidad, 0) > 0 or ifnull(CantidadDetalle, 0) > 0) " + where, new string[] { }).Count > 0;
        }

        public List<EntregasRepartidorTransaccionesDetalle> GetProductosNoEntregados(List<EntregasRepartidorTransacciones> entregas)
        {
            var enrSecuencias = "";
            var traSecuencias = "";

            foreach(var ent in entregas)
            {
                enrSecuencias += string.IsNullOrWhiteSpace(enrSecuencias) ? ent.EnrSecuencia.ToString() : ", " + ent.EnrSecuencia.ToString();
                traSecuencias += string.IsNullOrWhiteSpace(traSecuencias) ? ent.TraSecuencia.ToString() : ", " + ent.TraSecuencia.ToString();
            }

            var parValidarOfertas = myParametro.GetParEntregasRepartidorValidarOfertas();

            var list = SqliteManager.GetInstance().Query<EntregasRepartidorTransaccionesDetalle>("select e.EnrSecuencia as EnrSecuencia, e.UnmCodigo as UnmCodigo, e.OfeID as OfeID, e.TraIndicadorOferta as TraIndicadorOferta, TraPosicion, e.ProID, e.TraCantidad - ifnull(t.Cantidad, 0) as TraCantidad, TraPrecio, TraItbis, TraSelectivo, TraAdValorem, TraDescuento, TraIndicadorCompleto, CedCodigo, TraDesPorciento, TraTipoOferta, TraCantidadInventario, AutSecuencia, TraFlete, RepSupervisor " +
                "from EntregasRepartidorTransaccionesDetalle e " +
                "left join (select sum(Cantidad) as Cantidad, ProID, Posicion, UsaLote, Lote, TraSecuencia from EntregasDetalleTemp where case when ifnull(UsaLote, 0) = 1 then ifnull(Lote, '') != '' else 1=1 end group by ProID, Posicion, TraSecuencia) t on t.ProID = e.ProID and t.Posicion = e.TraPosicion and t.TraSecuencia = e.TraSecuencia  " +
                "where e.EnrSecuencia in ("+ enrSecuencias + ") and e.TraSecuencia in ("+traSecuencias+") and e.TitID = ? " + (parValidarOfertas ? " and ifnull(e.TraIndicadorOferta, 0) = 0 " : "") + " and trim(e.RepCodigo) = ? and (ifnull(t.Cantidad, 0) < e.TraCantidad) " +
                "group by e.EnrSecuencia, e.TraIndicadorOferta, TraPosicion, e.ProID, TraCantidad, TraCantidadDetalle, TraPrecio, TraItbis, TraSelectivo, TraAdValorem, TraDescuento, TraIndicadorCompleto, CedCodigo, e.OfeID, " +
                "TraDesPorciento, TraTipoOferta, e.UnmCodigo, TraCantidadInventario, AutSecuencia, TraFlete, RepSupervisor",
                new string[] { myParametro.GetParEntregasRepartidor() == 3 ? 1.ToString() : 4.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            if (parValidarOfertas)
            {
                var ofertasNoDadas = SqliteManager.GetInstance().Query<EntregasRepartidorTransaccionesDetalle>("select e.EnrSecuencia as EnrSecuencia, e.UnmCodigo as UnmCodigo, " +
                    "e.OfeID as OfeID, e.TraIndicadorOferta as TraIndicadorOferta, TraPosicion, e.ProID, case when t.ProID is not null then e.TraCantidad else e.TraCantidad - ifnull(t.Cantidad, 0) end as TraCantidad, TraPrecio, TraItbis, " +
                    "TraSelectivo, TraAdValorem, TraDescuento, TraIndicadorCompleto, CedCodigo, TraDesPorciento, TraTipoOferta, " +
                    "TraCantidadInventario, AutSecuencia, TraFlete, RepSupervisor " +
                "from EntregasRepartidorTransaccionesDetalle e " +
                "left join (select sum(Cantidad) as Cantidad, ProID, Posicion, UsaLote, Lote, TraSecuencia from EntregasDetalleTemp where ifnull(TraIndicadorOferta, 0) = 0 and " +
                "case when ifnull(UsaLote, 0) = 1 then ifnull(Lote, '') != '' else 1=1 end " +
                "group by ProID, Posicion, TraSecuencia having sum(ifnull(Cantidad, 0)) < ifnull(CantidadSolicitada, 0)) t on t.ProID = e.OfeID and t.TraSecuencia = e.TraSecuencia " +
                "left join (select sum(Cantidad) as Cantidad, ProID, Posicion, UsaLote, Lote, TraSecuencia from EntregasDetalleTemp where TraIndicadorOferta = 1 and case when ifnull(UsaLote, 0) = 1 then ifnull(Lote, '') != '' else 1=1 end group by ProID, Posicion, TraSecuencia) t2 on t2.ProID = e.ProID and t2.Posicion = e.TraPosicion and t2.TraSecuencia = e.TraSecuencia  " +
                "where e.EnrSecuencia in ("+enrSecuencias+") and e.TraSecuencia in ("+traSecuencias+") and e.TitID = ? and ifnull(e.TraIndicadorOferta, 0) = 1 and trim(e.RepCodigo) = ? and ((ifnull(t2.Cantidad, 0) < e.TraCantidad) or (t.ProID is not null)) " +
                "group by e.TraIndicadorOferta, TraPosicion, e.ProID, TraCantidad, TraCantidadDetalle, TraPrecio, TraItbis, TraSelectivo, TraAdValorem, " +
                "TraDescuento, TraIndicadorCompleto, CedCodigo, e.OfeID, " +
                "TraDesPorciento, TraTipoOferta, e.UnmCodigo, TraCantidadInventario, AutSecuencia, TraFlete, RepSupervisor",
                    new string[] { myParametro.GetParEntregasRepartidor() == 3 ? 1.ToString() : 4.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

                list.AddRange(ofertasNoDadas);
            }

            return list;
        }
        public List<EntregasRepartidorTransaccionesDetalle> GetProductosNoEntregadosxPedidos(int enrSecuencia, int traSecuencia, int titId)
        {
            var list = SqliteManager.GetInstance().Query<EntregasRepartidorTransaccionesDetalle>("select e.EnrSecuencia as EnrSecuencia, e.UnmCodigo as UnmCodigo, e.OfeID as OfeID, e.TraIndicadorOferta as TraIndicadorOferta, TraPosicion, e.ProID, e.TraCantidad - ifnull(t.Cantidad, 0) as TraCantidad, TraPrecio, TraItbis, TraSelectivo, TraAdValorem, TraDescuento, TraIndicadorCompleto, CedCodigo, TraDesPorciento, TraTipoOferta, TraCantidadInventario, AutSecuencia, TraFlete, RepSupervisor " +
                "from EntregasRepartidorTransaccionesDetalle e " +
                "left join (select sum(Cantidad) as Cantidad, ProID, Posicion, UsaLoteP, Lote from ProductosTemp where case when ifnull(UsaLoteP, 0) = 1 then ifnull(Lote, '') != '' else 1=1 end group by ProID, Posicion) t on t.ProID = e.ProID and t.Posicion = e.TraPosicion " +
                "where e.EnrSecuencia = ? and e.TraSecuencia = ? and e.TitID = ?  and trim(e.RepCodigo) = ? and (ifnull(t.Cantidad, 0) < e.TraCantidad) and e.TraIndicadorOferta = 0 " +
                "group by e.EnrSecuencia, e.TraIndicadorOferta, TraPosicion, e.ProID, TraCantidad, TraCantidadDetalle, TraPrecio, TraItbis, TraSelectivo, TraAdValorem, TraDescuento, TraIndicadorCompleto, CedCodigo, e.OfeID, " +
                "TraDesPorciento, TraTipoOferta, e.UnmCodigo, TraCantidadInventario, AutSecuencia, TraFlete, RepSupervisor",
                new string[] { enrSecuencia.ToString(), traSecuencia.ToString(), titId.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            return list;
        }

        public List<EntregasRepartidorTransaccionesDetalle> GetProductosNoEntregadosEn0xPedidos(int enrSecuencia, int traSecuencia, int titId)
        {
            var list = SqliteManager.GetInstance().Query<EntregasRepartidorTransaccionesDetalle>("select e.EnrSecuencia as EnrSecuencia, e.UnmCodigo as UnmCodigo, e.OfeID as OfeID, e.TraIndicadorOferta as TraIndicadorOferta, TraPosicion, e.ProID, e.TraCantidad - ifnull(t.Cantidad, 0) as TraCantidad, TraPrecio, TraItbis, TraSelectivo, TraAdValorem, TraDescuento, TraIndicadorCompleto, CedCodigo, TraDesPorciento, TraTipoOferta, TraCantidadInventario, AutSecuencia, TraFlete, RepSupervisor " +
                "from EntregasRepartidorTransaccionesDetalle e " +
                "left join (select sum(Cantidad) as Cantidad, ProID, Posicion, UsaLoteP, Lote from ProductosTemp where case when ifnull(UsaLoteP, 0) = 1 then ifnull(Lote, '') != '' else 1=1 end group by ProID, Posicion) t on t.ProID = e.ProID and t.Posicion = e.TraPosicion " +
                "where e.EnrSecuencia = ? and e.TraSecuencia = ? and e.TitID = ?  and trim(e.RepCodigo) = ? and (ifnull(t.Cantidad, 0) < e.TraCantidad) and ifnull(t.Cantidad, 0)=0  and e.TraIndicadorOferta = 0  " +
                "group by e.EnrSecuencia, e.TraIndicadorOferta, TraPosicion, e.ProID, TraCantidad, TraCantidadDetalle, TraPrecio, TraItbis, TraSelectivo, TraAdValorem, TraDescuento, TraIndicadorCompleto, CedCodigo, e.OfeID, " +
                "TraDesPorciento, TraTipoOferta, e.UnmCodigo, TraCantidadInventario, AutSecuencia, TraFlete, RepSupervisor",
                new string[] { enrSecuencia.ToString(), traSecuencia.ToString(), titId.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            return list;
        }

        public List<EntregasRepartidorTransaccionesDetalle> GetProductosOfertasNoEntregados(int enrSecuencia, int traSecuencia, int titId,int proid)
        {
            var list = SqliteManager.GetInstance().Query<EntregasRepartidorTransaccionesDetalle>("select e.EnrSecuencia as EnrSecuencia, e.UnmCodigo as UnmCodigo, e.OfeID as OfeID, e.TraIndicadorOferta as TraIndicadorOferta, TraPosicion, e.ProID, e.TraCantidad - ifnull(t.Cantidad, 0) as TraCantidad, TraPrecio, TraItbis, TraSelectivo, TraAdValorem, TraDescuento, TraIndicadorCompleto, e.CedCodigo, TraDesPorciento, TraTipoOferta, TraCantidadInventario, AutSecuencia, TraFlete, e.RepSupervisor " +
                "from EntregasRepartidorTransaccionesDetalle e " +
                "left join ProductosTemp  t on t.ProID = e.ProID and t.IndicadorOferta = e.TraIndicadorOferta " +
                "where e.EnrSecuencia = ? and e.TraSecuencia = ? and e.TitID = ?  and trim(e.RepCodigo) = ? and e.ProID = ? and (ifnull(t.Cantidad, 0) < e.TraCantidad) and e.TraIndicadorOferta = 1  " +
                "group by e.EnrSecuencia, e.TraIndicadorOferta, TraPosicion, e.ProID, TraCantidad, TraCantidadDetalle, TraPrecio, TraItbis, TraSelectivo, TraAdValorem, TraDescuento, TraIndicadorCompleto, e.CedCodigo, e.OfeID, " +
                "TraDesPorciento, TraTipoOferta, e.UnmCodigo, TraCantidadInventario, AutSecuencia, TraFlete, e.RepSupervisor",
                new string[] { enrSecuencia.ToString(), traSecuencia.ToString(), titId.ToString(), Arguments.CurrentUser.RepCodigo.Trim(), proid.ToString() });

            return list;
        }

        public List<EntregasRepartidorTransaccionesDetalle> GetProductosNoEntregadosEnVenta(int enrSecuencia, int traSecuencia, int titId, int vensecuencia)
        {
            var list = SqliteManager.GetInstance().Query<EntregasRepartidorTransaccionesDetalle>("select e.ProID, e.EnrSecuencia as EnrSecuencia, e.TraCantidad - ifnull(t.Cantidad, 0) as TraCantidad " +
                "from EntregasRepartidorTransaccionesDetalle e " +
                "left join (select sum(VenCantidad) as Cantidad, ProID, PedSecuencia from VentasDetalle vd inner Join Ventas v on v.RepCodigo=vd.RepCodigo and v.VenSecuencia=vd.VenSecuencia where v.VenSecuencia=? group by ProID, PedSecuencia,v.VenSecuencia) t on t.ProID = e.ProID and t.PedSecuencia=e.TraSecuencia " +
                "where e.EnrSecuencia = ? and e.TraSecuencia = ? and e.TitID = ?  and trim(e.RepCodigo) = ? and (ifnull(t.Cantidad, 0) < e.TraCantidad) and ifnull(t.Cantidad, 0)=0 and e.TraIndicadorOferta = 0 " +
                "group by e.EnrSecuencia, TraCantidad",
                new string[] { vensecuencia.ToString(), enrSecuencia.ToString(), traSecuencia.ToString(), titId.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            return list;
        }

        public List<EntregasRepartidorTransaccionesDetalle> GetProductosNoEntregados(int enrSecuencia, int traSecuencia, int titId)
        {
            var parValidarOfertas = myParametro.GetParEntregasRepartidorValidarOfertas();

            var list = SqliteManager.GetInstance().Query<EntregasRepartidorTransaccionesDetalle>("select e.EnrSecuencia as EnrSecuencia, e.UnmCodigo as UnmCodigo, e.OfeID as OfeID, e.TraIndicadorOferta as TraIndicadorOferta, TraPosicion, e.ProID, e.TraCantidad - ifnull(t.Cantidad, 0) as TraCantidad, TraPrecio, TraItbis, TraSelectivo, TraAdValorem, TraDescuento, TraIndicadorCompleto, CedCodigo, TraDesPorciento, TraTipoOferta, TraCantidadInventario, AutSecuencia, TraFlete, RepSupervisor " +
                "from EntregasRepartidorTransaccionesDetalle e " +
                "left join (select sum(Cantidad) as Cantidad, ProID, Posicion, UsaLote, Lote, TraSecuencia from EntregasDetalleTemp where case when ifnull(UsaLote, 0) = 1 then ifnull(Lote, '') != '' else 1=1 end group by ProID, Posicion, TraSecuencia) t on t.ProID = e.ProID and t.Posicion = e.TraPosicion and t.TraSecuencia = e.TraSecuencia  " +
                "where e.EnrSecuencia = ? and e.TraSecuencia = ? and e.TitID = ? "+(parValidarOfertas? " and ifnull(e.TraIndicadorOferta, 0) = 0 " : "") +" and trim(e.RepCodigo) = ? and (ifnull(t.Cantidad, 0) < e.TraCantidad) " +
                "group by e.EnrSecuencia, e.TraIndicadorOferta, TraPosicion, e.ProID, TraCantidad, TraCantidadDetalle, TraPrecio, TraItbis, TraSelectivo, TraAdValorem, TraDescuento, TraIndicadorCompleto, CedCodigo, e.OfeID, " +
                "TraDesPorciento, TraTipoOferta, e.UnmCodigo, TraCantidadInventario, AutSecuencia, TraFlete, RepSupervisor",
                new string[] { enrSecuencia.ToString(), traSecuencia.ToString(), titId.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            if (parValidarOfertas)
            {
                var ofertasNoDadas = SqliteManager.GetInstance().Query<EntregasRepartidorTransaccionesDetalle>("select e.EnrSecuencia as EnrSecuencia, e.UnmCodigo as UnmCodigo, " +
                    "e.OfeID as OfeID, e.TraIndicadorOferta as TraIndicadorOferta, TraPosicion, e.ProID, case when t.ProID is not null then e.TraCantidad else e.TraCantidad - ifnull(t.Cantidad, 0) end as TraCantidad, TraPrecio, TraItbis, " +
                    "TraSelectivo, TraAdValorem, TraDescuento, TraIndicadorCompleto, CedCodigo, TraDesPorciento, TraTipoOferta, " +
                    "TraCantidadInventario, AutSecuencia, TraFlete, RepSupervisor " +
                "from EntregasRepartidorTransaccionesDetalle e " +
                "left join (select sum(Cantidad) as Cantidad, ProID, Posicion, UsaLote, Lote, TraSecuencia from EntregasDetalleTemp where ifnull(TraIndicadorOferta, 0) = 0 and " +
                "case when ifnull(UsaLote, 0) = 1 then ifnull(Lote, '') != '' else 1=1 end " +
                "group by ProID, Posicion, TraSecuencia having sum(ifnull(Cantidad, 0)) < ifnull(CantidadSolicitada, 0)) t on t.ProID = e.OfeID and t.TraSecuencia = e.TraSecuencia " +
                "left join (select sum(Cantidad) as Cantidad, ProID, Posicion, UsaLote, Lote, TraSecuencia from EntregasDetalleTemp where TraIndicadorOferta = 1 and case when ifnull(UsaLote, 0) = 1 then ifnull(Lote, '') != '' else 1=1 end group by ProID, Posicion, TraSecuencia) t2 on t2.ProID = e.ProID and t2.Posicion = e.TraPosicion and t2.TraSecuencia = e.TraSecuencia  " +
                "where e.EnrSecuencia = ? and e.TraSecuencia = ? and e.TitID = ? and ifnull(e.TraIndicadorOferta, 0) = 1 and trim(e.RepCodigo) = ? and ((ifnull(t2.Cantidad, 0) < e.TraCantidad) or (t.ProID is not null)) " +
                "group by e.TraIndicadorOferta, TraPosicion, e.ProID, TraCantidad, TraCantidadDetalle, TraPrecio, TraItbis, TraSelectivo, TraAdValorem, " +
                "TraDescuento, TraIndicadorCompleto, CedCodigo, e.OfeID, " +
                "TraDesPorciento, TraTipoOferta, e.UnmCodigo, TraCantidadInventario, AutSecuencia, TraFlete, RepSupervisor", 
                    new string[] { enrSecuencia.ToString(), traSecuencia.ToString(), titId.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

                list.AddRange(ofertasNoDadas);
            }

            return list;
        }

        public List<EntregasRepartidorTransaccionesDetalle> GetProductosNoEntregadosxEntrega(int entsecuencia, int enrSecuencia, int traSecuencia, int titId)
        {
            var parValidarOfertas = myParametro.GetParEntregasRepartidorValidarOfertas();

            var list = SqliteManager.GetInstance().Query<EntregasRepartidorTransaccionesDetalle>("select e.ProID, e.EnrSecuencia as EnrSecuencia, e.TraCantidad as TraCantidad, e.TraCantidadDetalle as TraCantidadDetalle " +
               "from EntregasRepartidorTransaccionesDetalle e " +
               "where e.EnrSecuencia = ? and e.TraSecuencia = ? and e.TitID = ?  and trim(e.RepCodigo) = ?   and e.TraIndicadorOferta = 0 and " +
               "not exists (select 1 from EntregasTransacciones et inner join EntregasTransaccionesDetalle t on et.EntSecuencia= t.EntSecuencia and et.RepCodigo= t.RepCodigo where et.EnrSecuencia=e.EnrSecuencia and et.VenSecuencia=e.TraSecuencia and et.RepVendedor = e.RepVendedor and  t.ProID = e.ProID and  t.EntIndicadorOferta = e.TraIndicadorOferta and et.EntSecuencia= ? ) " +
               "group by e.EnrSecuencia, TraCantidad, e.ProID",
               new string[] { enrSecuencia.ToString(), traSecuencia.ToString(), titId.ToString(), Arguments.CurrentUser.RepCodigo.Trim(), entsecuencia.ToString() });

            if (parValidarOfertas)
            {

                var ofertasNoDadas = SqliteManager.GetInstance().Query<EntregasRepartidorTransaccionesDetalle>("select e.ProID, e.EnrSecuencia as EnrSecuencia, e.TraCantidad as TraCantidad, e.TraCantidadDetalle as TraCantidadDetalle " +
               "from EntregasRepartidorTransaccionesDetalle e " +
               "where e.EnrSecuencia = ? and e.TraSecuencia = ? and e.TitID = ?  and trim(e.RepCodigo) = ?   and e.TraIndicadorOferta = 1 and " +
               "not exists (select 1 from EntregasTransacciones et inner join EntregasTransaccionesDetalle t on et.EntSecuencia= t.EntSecuencia and et.RepCodigo= t.RepCodigo where et.EnrSecuencia=e.EnrSecuencia and et.VenSecuencia=e.TraSecuencia and et.RepVendedor = e.RepVendedor and  t.ProID = e.ProID and  t.EntIndicadorOferta = e.TraIndicadorOferta and et.EntSecuencia= ? ) " +
               "group by e.EnrSecuencia, TraCantidad, e.ProID",
               new string[] { enrSecuencia.ToString(), traSecuencia.ToString(), titId.ToString(), Arguments.CurrentUser.RepCodigo.Trim(), entsecuencia.ToString() });

                list.AddRange(ofertasNoDadas);
            }

            return list;
        }

        public List<EntregasDetalleTemp> GetProductosNoEntregadosForModal(int enrSecuencia, int traSecuencia, int titId)
        {
            return SqliteManager.GetInstance().Query<EntregasDetalleTemp>("select e.TraSecuencia as TraSecuencia, e.UnmCodigo as UnmCodigo, e.TraIndicadorOferta as TraIndicadorOferta, e.OfeID as OfeID, TraPosicion as Posicion, e.ProID as ProID, p.ProDescripcion as ProDescripcion, p.ProCodigo as ProCodigo, e.TraCantidad as CantidadSolicitada, ifnull(t.Cantidad, 0) as Cantidad " +
                "from EntregasRepartidorTransaccionesDetalle e " +
                "left join (select sum(Cantidad) as Cantidad, ProID, Posicion, UsaLote, Lote, TraSecuencia from EntregasDetalleTemp where case when ifnull(UsaLote, 0) = 1 then ifnull(Lote, '') != '' else 1=1 end " +
                "group by ProID, Posicion) t on t.ProID = e.ProID and t.Posicion = e.TraPosicion and t.TraSecuencia = e.TraSecuencia " +
                "inner join Productos p on p.ProID = e.ProID  " +
                "where e.EnrSecuencia = ? and e.TraSecuencia = ? and e.TitID = ? and trim(e.RepCodigo) = ? and ifnull(t.Cantidad, 0) < e.TraCantidad " +
                "group by e.EnrSecuencia, e.UnmCodigo, e.TraIndicadorOferta, TraPosicion, e.ProID, TraCantidad, e.OfeID, p.ProCodigo, p.ProDescripcion",
                new string[] { enrSecuencia.ToString(), traSecuencia.ToString(), titId.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public List<EntregasDetalleTemp> GetProductosNoEntregadosForModal(List<EntregasRepartidorTransacciones> entregas)
        {
            var enrSecuencias = "";
            var traSecuencias = "";

            foreach (var entrega in entregas)
            {
                enrSecuencias += string.IsNullOrWhiteSpace(enrSecuencias) ? entrega.EnrSecuencia.ToString() : ", " + entrega.EnrSecuencia.ToString();
                traSecuencias += string.IsNullOrWhiteSpace(traSecuencias) ? entrega.TraSecuencia.ToString() : ", " + entrega.TraSecuencia.ToString();
            }

            string titID = " e.TitID = 4 ";

            if (myParametro.GetParEntregasRepartidor() == 3)
            {
                titID = " e.TitID = 1 ";
            }

            var query = "select e.UnmCodigo as UnmCodigo, e.TraIndicadorOferta as TraIndicadorOferta, e.ProID as ProID, p.ProDescripcion as ProDescripcion, p.ProCodigo as ProCodigo, sum(e.TraCantidad) as CantidadSolicitada, sum(ifnull(t.Cantidad, 0)) as Cantidad " +
                   "from EntregasRepartidorTransaccionesDetalle e " +
                   "left join (select sum(Cantidad) as Cantidad, ProID, Posicion, UsaLote, Lote, TraSecuencia from EntregasDetalleTemp where case when ifnull(UsaLote, 0) = 1 then ifnull(Lote, '') != '' else 1=1 end " +
                   "group by ProID, Posicion, TraSecuencia) t on t.ProID = e.ProID and t.Posicion = e.TraPosicion and t.TraSecuencia = e.TraSecuencia " +
                   "inner join Productos p on p.ProID = e.ProID  " +
                   "where e.EnrSecuencia in (" + enrSecuencias + ") and e.TraSecuencia in ("+traSecuencias+") and " + titID + " and trim(e.RepCodigo) = ? and ifnull(t.Cantidad, 0) < e.TraCantidad " +
                   "group by e.UnmCodigo, e.TraIndicadorOferta, e.ProID, p.ProCodigo, p.ProDescripcion";

            return SqliteManager.GetInstance().Query<EntregasDetalleTemp>(query,
                   new string[] {  Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public bool HayEntregasSinImprimirByVisita(int visSecuencia)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<EntregasTransacciones>("select EntSecuencia from EntregasTransacciones " +
                    "where VisSecuencia = ? and trim(RepCodigo) = '"+Arguments.CurrentUser.RepCodigo.Trim()+"' and ifnull(EntCantidadImpresion, 0) = 0 ",
                    new string[] { visSecuencia.ToString() });

                return list != null && list.Count > 0;
            }
            catch (Exception) { return false; }
        }

        public int getPedidosPorEntregar(int cliId)
        {
            try
            {
                string diasPermitidos = "7";
                if (DS_RepresentantesParametros.GetInstance().GetDiasEntregasRepartidorTransaccionesVisibles() > 0)
                {
                    diasPermitidos = DS_RepresentantesParametros.GetInstance().GetDiasEntregasRepartidorTransaccionesVisibles().ToString();
                }

                var list = SqliteManager.GetInstance().Query<EntregasRepartidorTransacciones>("select count(t.EnrSecuencia) as EnrSecuencia  " +
                    "from EntregasRepartidorTransacciones t " +
                    "inner join EntregasRepartidor et on et.EnrSecuencia=t.EnrSecuencia and et.RepCodigo = t.RepCodigo " +
                    "where t.TitID = 1 and enrEstatusEntrega = 1 and CliID = ? and (cast(replace(cast(julianday(datetime('now')) - julianday(EnrFecha) as integer),' ', '') as integer)) < " + diasPermitidos + "  limit 1", new string[] { cliId.ToString() });

                if (list != null && list.Count > 0)
                {
                    return list[0].EnrSecuencia;
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return 0;
        }

        public bool HayPedidosPorEntregar(int cliId)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<EntregasRepartidorTransacciones>("select EnrSecuencia " +
                    "from EntregasRepartidorTransacciones " +
                    "where TitID = 1 and enrEstatusEntrega = 1 and CliID = ? limit 1", new string[] { cliId.ToString() });

                return list != null && list.Count > 0 && Arguments.Values.CurrentModule == Enums.Modules.VENTAS;

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return false;
        }

        private List<EntregasRepartidorTransaccionesDetalle> GetDetalleEntrega(int enrSecuencia, int traSecuencia, int titId, string repvendedor)
        {
            return SqliteManager.GetInstance().Query<EntregasRepartidorTransaccionesDetalle>("select OfeID, TraCantidad, TraCantidadDetalle, ProID, TraIndicadorOferta, TraPosicion, TraPrecio, TraItbis, TraSelectivo, TraAdValorem, TraDescuento, TraDesPorciento, UnmCodigo, CliID from " +
                "EntregasRepartidorTransaccionesDetalle where EnrSecuencia = ? and TraSecuencia = ? and trim(RepCodigo) = ? and TitID = ?  and RepVendedor= ? ",
                new string[] { enrSecuencia.ToString(), traSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim(), titId.ToString(), repvendedor });
        }

        public bool ExistsInTempWithLote(int proId, int posicion, string lote, int traSecuencia)
        {
            var where = "";

            if (!string.IsNullOrWhiteSpace(lote))
            {
                where = " and ifnull(Lote, '') = '"+lote+"' ";
            }

            return SqliteManager.GetInstance().Query<EntregasDetalleTemp>("select ProID from EntregasDetalleTemp " +
                "where ProID = ? and Posicion = ? and TraSecuencia = ? and UsaLote = '1' "+where+" and (ifnull(Cantidad, 0.0) > 0 or ifnull(CantidadDetalle, 0) > 0)", 
                new string[] { proId.ToString(), posicion.ToString(), traSecuencia.ToString() }).FirstOrDefault() != null;
        }

        public Task RechazarEntregaPedido(EntregasRepartidorTransacciones entrega, int titId, string motivoDevolucion, bool fromCuadre=false)
        {
            return new TaskLoader()
            {
                SqlTransactionWhenRun = true
            }.Execute(() =>
            {
                if (!fromCuadre)
                {
                    new DS_Visitas().ActualizarVisitaEfectiva(Arguments.Values.CurrentVisSecuencia);
                }
                

                var e = new Hash("EntregasRepartidorTransacciones");
                e.Add("enrEstatusEntrega", 0);
                e.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                e.Add("EntFechaActualizacion", Functions.CurrentDate());
                e.Add("MotID", motivoDevolucion);

                e.ExecuteUpdate("EnrSecuencia = " + entrega.EnrSecuencia + " and EnrSecuenciaDetalle = " + entrega.EnrSecuenciaDetalle);

                if (myParametro.GetParMultialmacenConDevolucion())
                {
                    var productos = GetDetalleEntrega(entrega.EnrSecuencia, entrega.TraSecuencia, titId, entrega.RepVendedor);

                    if (myParametro.GetParUsarMultiAlmacenes() && titId == 1)
                    {
                        var dsInv = new DS_Inventarios();

                        var almIdDevolucion = myParametro.GetParAlmacenIdParaDevolucion();
                        var almIdDespacho = myParametro.GetParAlmacenIdParaDespacho();

                        foreach (var producto in productos)
                        {
                            if (dsInv.HayExistencia(producto.ProID,  producto.TraCantidad , almId: almIdDespacho))
                            {
                                dsInv.RestarInventario(producto.ProID, producto.TraCantidad, (int)producto.TraCantidadDetalle, almIdDespacho);
                                dsInv.AgregarInventario(producto.ProID, producto.TraCantidad, (int)producto.TraCantidadDetalle, almIdDevolucion);
                            }
                            else
                            {
                                double existenciaProducto = dsInv.GetCantidadTotalInventario(producto.ProID, almIdDespacho);
                                if (existenciaProducto > 0)
                                {
                                    dsInv.RestarInventario(producto.ProID, existenciaProducto, (int)producto.TraCantidadDetalle, almIdDespacho);
                                    dsInv.AgregarInventario(producto.ProID, existenciaProducto, (int)producto.TraCantidadDetalle, almIdDevolucion);
                                }
                                
                            }
                        }
                    }
                    else
                    {
                        CrearDevolucionProductosNoEntregados(productos, entrega, motivoDevolucion);
                    }
                }
            });

        }

        public Task RechazarEntrega(EntregasRepartidorTransacciones entrega, int titId, string motivoDevolucion)
        {
            return new TaskLoader()
            {
                SqlTransactionWhenRun = true
            }.Execute(() =>
            {
                new DS_Visitas().ActualizarVisitaEfectiva(Arguments.Values.CurrentVisSecuencia);

                var e = new Hash("EntregasRepartidorTransacciones");
                e.Add("enrEstatusEntrega", 0);
                e.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                e.Add("EntFechaActualizacion", Functions.CurrentDate());
                e.Add("MotID", motivoDevolucion);

                e.ExecuteUpdate("EnrSecuencia = " + entrega.EnrSecuencia + " and EnrSecuenciaDetalle = " + entrega.EnrSecuenciaDetalle);

                var productos = GetDetalleEntrega(entrega.EnrSecuencia, entrega.TraSecuencia, titId, entrega.RepVendedor);

                if (myParametro.GetParUsarMultiAlmacenes() && titId == 1)
                {
                    var dsInv = new DS_Inventarios();

                    var almIdDevolucion = myParametro.GetParAlmacenIdParaDevolucion();

                    foreach (var producto in productos)
                    {
                        dsInv.AgregarInventario(producto.ProID, producto.TraCantidad, (int)producto.TraCantidadDetalle, almIdDevolucion);
                    }
                }
                else
                {
                    CrearDevolucionProductosNoEntregados(productos, entrega, motivoDevolucion);
                }

                var entregasRechazadas = GetEntregasRechazadasSinMotivo();

                if(entregasRechazadas.Count > 0)
                {
                    int.TryParse(motivoDevolucion, out int motId);
                    ActualizarMotivoEntregaRechazada(motId, entregasRechazadas);
                }
            });

        }

        public EntregasTransacciones GetEntregaTransaccion(int entSecuencia, bool confirmada)
        {
            return SqliteManager.GetInstance().Query<EntregasTransacciones>("select * from " + (confirmada? "EntregasTransaccionesConfirmados" : "EntregasTransacciones" ) +" e " +
                "inner join Clientes c on c.CliID = e.CliID " +
                "where e.EntSecuencia = ? and trim(e.RepCodigo) = ? ", 
                new string[] { entSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() }).FirstOrDefault();
        }

        public EntregasRepartidorTransacciones GetEntregasRepartidorTransacciones(int traSecuencia, bool confirmada, int enrSecuencia)
        {
            return SqliteManager.GetInstance().Query<EntregasRepartidorTransacciones>("select * from " + (confirmada? "EntregasRepartidorTransaccionesConfirmados" : "EntregasRepartidorTransacciones") +" e " +
                "inner join Clientes c on c.CliID = e.CliID " +
                "where e.TraSecuencia = ? and trim(e.RepCodigo) = ? and e.EnrSecuencia= ? ", 
                new string[] { traSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim(), enrSecuencia.ToString() }).FirstOrDefault();
        }

        public List<EntregasTransacciones> GetEntregaTransaccionDeLaActualVisita()
        {
            return SqliteManager.GetInstance().Query<EntregasTransacciones>(@"select  
                                    e.RepCodigo, EntSecuencia, TitID, e.CliID, EntFecha, EntEstatus,
                                    EntTotal, EntNCF, EntIndicadorCompleto, CuaSecuencia, EntTipo,
                                    RepVendedor, VenSecuencia, EntCantidadDetalle, VisSecuencia, e.ConID, c.CliRNC as CliRNC,
                                    e.MonCodigo, CliCodigo, CliCalle, CliNombre, CliUrbanizacion, e.SecCodigo, c.CliTipoComprobanteFAC as CliTipoComprobanteFAC  from EntregasTransacciones e  
                                    inner join Clientes c on c.CliID = e.CliID  
                                    where e.VisSecuencia = ? and trim(e.RepCodigo) = ? and e.EntTipo = 1 ",
                new string[] { Arguments.Values.CurrentVisSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public List<EntregasTransacciones> GetEntregasTransaccionesRealizadas()
        {
            return SqliteManager.GetInstance().Query<EntregasTransacciones>(@"select  
                                    e.RepCodigo, EntSecuencia, TitID, e.CliID, EntFecha, EntEstatus,
                                    EntTotal, EntNCF, EntIndicadorCompleto, CuaSecuencia, EntTipo,
                                    RepVendedor, VenSecuencia, EntCantidadDetalle, VisSecuencia, e.ConID, c.CliRNC as CliRNC,
                                    e.MonCodigo, CliCodigo, CliCalle, CliNombre, CliUrbanizacion, e.SecCodigo, c.CliTipoComprobanteFAC as CliTipoComprobanteFAC  from EntregasTransacciones e  
                                    inner join Clientes c on c.CliID = e.CliID  
                                    where e.CliID = ? and trim(e.RepCodigo) = ? and e.EntTipo = 1 and e.EntEstatus <> 0 ",
                new string[] { Arguments.Values.CurrentClient.CliID.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public List<EntregasTransaccionesDetalle> GetEntregaTransaccionesDetalle(int entSecuencia, bool confirmada)
        {
            return SqliteManager.GetInstance().Query<EntregasTransaccionesDetalle>("select * from " + (confirmada? "EntregasTransaccionesDetalleConfirmados" : "EntregasTransaccionesDetalle") + " e " +
                "inner join Productos p on p.ProID = e.ProID " +
                "where e.EntSecuencia = ? and trim(e.RepCodigo) = ? " +
                "order by p.ProDescripcion", 
                new string[] {entSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public List<EntregasTransaccionesDetalle> GetEntregaTransaccionesDetalleBultosYUnidades(int entSecuencia, bool confirmada)
        {
            var sql = "select sum(Bultos) ProUnidades, sum(Unidades) EntCantidad from (" +
                "select p.ProUnidades,  " +
                "sum(EntCantidad) EntCantidad,  " +
                "cast((sum(EntCantidad) / (case when p.ProUnidades > 0 then p.ProUnidades else 1.0 end)) as int) Bultos," +
                "(sum(EntCantidad) - (" +
                "(cast((sum(EntCantidad) / (case when p.ProUnidades > 0 then p.ProUnidades else 1.0 end)) as int) )" +
                "* (case when p.ProUnidades > 0 then p.ProUnidades else 1.0 end))) Unidades " +
                "from " + (confirmada ? "EntregasTransaccionesDetalleConfirmados" : " EntregasTransaccionesDetalle ") + " e inner join productos p on p.proid = e.ProID " +
                "where RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim()  + "' and EntSecuencia =  " + entSecuencia.ToString() + 
                " GROUP BY  p.ProUnidades) tabla";  

            return SqliteManager.GetInstance().Query<EntregasTransaccionesDetalle>(sql);
        }

        public List<EntregasRepartidorTransaccionesDetalle> GetEntregasRepartidorTransaccionesDetalle(int traSecuecia, bool confirmada, int enrSecuencia)
        {
            return SqliteManager.GetInstance().Query<EntregasRepartidorTransaccionesDetalle>("select * from " + (confirmada? "EntregasRepartidorTransaccionesDetalleConfirmados" : "EntregasRepartidorTransaccionesDetalle") + " e " +
                "inner join Productos p on p.ProID = e.ProID " +
                "where e.TraSecuencia = ? and trim(e.RepCodigo) = ? and e.EnrSecuencia = ? " +
                "order by p.ProDescripcion", 
                new string[] { traSecuecia.ToString(), Arguments.CurrentUser.RepCodigo.Trim(), enrSecuencia.ToString() });
        }


        public List<EntregasTransaccionesDetalle> GetEntregaTransaccionesDetalleConLote(int entSecuencia)
        {
            string query = @"select distinct e.RepCodigo, e.EntSecuencia, ertd.UnmCodigo, ifnull(el.EntLote,'') Lote, el.EntPosicion,   e.EntPrecio, el.EntCantidad, EntIndicadorOferta, 
                                        EntDescuento, EntDescPorciento, EntItbis,EntSelectivo, EntAdValorem, ProCodigo, ProDescripcion,   ProUnidades from EntregasTransaccionesDetalle e  
			                            inner join EntregasTransaccionesDetalleLotes el on  el.RepCodigo = e.RepCodigo and el.EntSecuencia = e.EntSecuencia and el.EntPosicion = e.EntPosicion 
			                            inner join EntregasTransacciones et on et.RepCodigo = e.RepCodigo and et.EntSecuencia = e.EntSecuencia
				                        inner join EntregasRepartidorTransacciones ert on ert.TraSecuencia = et.VenSecuencia and ert.VenNCF = et.EntNCF
				                        inner join EntregasRepartidorTransaccionesDetalle ertd on
				                        ertd.EnrSecuencia = ert.EnrSecuencia and ertd.TraSecuencia = ert.EnrSecuenciaDetalle and ertd.RepCodigo = et.RepCodigo
				                        and ertd.ProId = e.ProId and ertd.TraPosicion = e.EntPosicion
                                        inner join Productos p on p.ProID = e.ProID  
                                        inner join Lineas l on l.LinID = p.LinID  
                                        where e.EntSecuencia = ? and trim(e.RepCodigo) = ?
                                        order by l.LinReferencia, p.ProCodigo ";
            return SqliteManager.GetInstance().Query<EntregasTransaccionesDetalle>(query,
                new string[] { entSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public List<EntregasTransaccionesDetalleLotes> GetEntregaTransaccionesDetalleLote(int entSecuencia)
        {
            return SqliteManager.GetInstance().Query<EntregasTransaccionesDetalleLotes>("select * from EntregasTransaccionesDetalleLotes " +
                "where EntSecuencia = ? and trim(RepCodigo) = ? ", 
                new string[] { entSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public bool SeQuitaraOferta(bool validarLote = true, bool validarCantidad = false)
        {
            var seQuitaranOfertas = SqliteManager.GetInstance().Query<EntregasDetalleTemp>("select ProID from EntregasDetalleTemp e " +
                "where ifnull(TraIndicadorOferta, 0) = 1 and OfeID in " +
                "(select ProID from EntregasDetalleTemp where ProID = e.OfeID and TraSecuencia = e.TraSecuencia and ifnull(TraIndicadorOferta, 0) = 0 " +
                "group by ProID, Posicion, TraSecuencia having (sum(ifnull(Cantidad, 0)) < ifnull(CantidadSolicitada, 0) " +
                (validarLote ? "or (case when ifnull(UsaLote, 0) = 1 then ifnull(Lote, '') = '' else 1=1 end)) ) and case when ifnull(UsaLote,0) = 1 then ifnull(Lote, '') != '' else 1=1 end " : ")) ") +
                "group by ProID, Posicion, TraSecuencia having sum(Cantidad) = CantidadSolicitada ", new string[] { }).FirstOrDefault() != null;

            var hayOfertantesIncompletos = false;

            if (myParametro.GetParEntregasOfertasTodoONada())
            {
                var query = "select ProID from EntregasDetalleTemp e " +
                "where ifnull(TraIndicadorOferta, 0) = 0 " + (validarCantidad ? " and Cantidad > 0 " : "") + (validarLote ? " and (case when ifnull(UsaLote, 0) = 1 then ifnull(Lote, '') != '' else 1=1 end) " : "") + " and ProID in " +
                "(select OfeID from EntregasDetalleTemp where OfeID = e.ProID and TraSecuencia = e.TraSecuencia and ifnull(TraIndicadorOferta, 0) = 1 " +
                "group by ProID, Posicion, TraSecuencia, OfeID) group by ProID, Posicion, TraSecuencia having (sum(ifnull(Cantidad, 0)) < ifnull(CantidadSolicitada, 0)) ";
                hayOfertantesIncompletos = SqliteManager.GetInstance().Query<EntregasDetalleTemp>(query, 
                    new string[] { }).FirstOrDefault() != null;
            }

            return seQuitaranOfertas || hayOfertantesIncompletos;
        }

        public bool HayOfertasSinEntregarCompletamente(bool validarLote = true, bool validarOfertaCantidad = true, bool isAdded = false)
        {
            //var ofertaTodoONada = myParametro.GetParEntregasOfertasTodoONada();

            var query = "select ProID from EntregasDetalleTemp e where " +
                "ifnull(TraIndicadorOferta, 0) = 1 "+(isAdded?" and ifnull(IsAdded, 0) = 1 ":"")+" and OfeID in (select ProID from EntregasDetalleTemp " +
                "where ifnull(TraIndicadorOferta, 0) = 0 and TraSecuencia = e.TraSecuencia " +
                (validarLote ? " and case when ifnull(UsaLote, 0) = 1 then ifnull(Lote, '') != '' else 1=1 end " : "" ) +
                "group by ProID, Posicion, TraSecuencia "+(validarOfertaCantidad? " having sum(ifnull(Cantidad, 0)) = ifnull(CantidadSolicitada, 0) " : "") +") " +
                "group by ProID, Posicion, TraSecuencia " +
                "having ((sum(ifnull(Cantidad, 0)) < ifnull(CantidadSolicitada, 0)) "+(validarLote ? "OR (ifnull(UsaLote, 0) = 1 AND ifnull(Lote, '') = '')" : "")+")";
            var list = SqliteManager.GetInstance().Query<EntregasDetalleTemp>(query, new string[] { });

            return list != null && list.Count > 0;
        }

        public void ActualizarCantidadImpresion(int entSecuencia, bool confirmado)
        {
            if (!confirmado)
            {
                Hash map = new Hash("EntregasTransacciones");
                map.Add("EntCantidadImpresion", "EntCantidadImpresion + 1", true);
                map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                map.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and EntSecuencia = " + entSecuencia);
            }
            else
            {
                Hash map2 = new Hash("EntregasTransaccionesConfirmados");
                map2.Add("EntCantidadImpresion", "EntCantidadImpresion + 1", true);
                map2.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                map2.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and EntSecuencia = " + entSecuencia);
            }
        }

        public void ActualizarRepartidorCantidadImpresion(int TraSecuencia, bool confirmado)
        {
            if (!confirmado)
            {
                Hash map = new Hash("EntregasRepartidorTransacciones");
                map.Add("EntCantidadImpresion", "EntCantidadImpresion + 1", true);
                map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                map.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and TraSecuencia = " + TraSecuencia);
            }
            else
            {
                Hash map2 = new Hash("EntregasRepartidorTransaccionesConfirmados");
                map2.Add("EntCantidadImpresion", "EntCantidadImpresion + 1", true);
                map2.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                map2.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and TraSecuencia = " + TraSecuencia);
            }
        }

        public string GetEntregasDisponiblesOtroSector(string secCodigo)
        {
            //dames las entregas disponibles de otro sector siempre y cuando el sector actual ya no tenga mas entregas
            try
            {

                string titID = " TitID = 4 ";

                if (myParametro.GetParEntregasRepartidor() == 3)
                {
                    titID = " TitID = 1 ";
                }

                var list = SqliteManager.GetInstance().Query<Sectores>("select SecCodigo from EntregasRepartidorTransacciones " +
                    "where CliID = ? and "+ titID +" and enrEstatusEntrega = 1 and SecCodigo != ? and trim(RepCodigo) = ? " +
                    "and not exists(select 1 from EntregasRepartidorTransacciones where enrEstatusEntrega = 1 and CliID = ? and trim(RepCodigo) = ? " +
                    "and " + titID + " and trim(SecCodigo) = ?) order by SecCodigo", new string[] { Arguments.Values.CurrentClient.CliID.ToString(), secCodigo,
                    Arguments.CurrentUser.RepCodigo.Trim(), Arguments.Values.CurrentClient.CliID.ToString(), Arguments.CurrentUser.RepCodigo.Trim(), secCodigo });

                if (list != null && list.Count > 0)
                {
                    return list[0].SecCodigo;
                }

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return null;
        }

        public List<Sectores> GetSectoresConEntregas()
        {
            return SqliteManager.GetInstance().Query<Sectores>("select distinct SecCodigo from EntregasRepartidorTransacciones " +
                    "where CliID = ? and enrEstatusEntrega = 1 and trim(RepCodigo) = ? " +
                    "order by SecCodigo", new string[] { Arguments.Values.CurrentClient.CliID.ToString(),
                    Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public List<EntregasDetalleTemp> BuscarProductosInTemp(ProductosArgs args, bool resumen)
        {
            var whereCondition = "";

            if (!resumen)
            {
                whereCondition = Functions.DinamicFiltersGenerateScript(args.filter, args.valueToSearch, args.secondFilter);
            }

            return SqliteManager.GetInstance().Query<EntregasDetalleTemp>("select t.* from EntregasDetalleTemp t " +
                "inner join Productos p on p.ProID = t.ProID " +
                "where 1=1 "+whereCondition+" order by p.ProDescripcion ", new string[] { });
        }


        public List<EntregasTransacciones> GetEntregasTransaccionesRealizadasFromCuadre(int cuaSecuencia)
        {
            string query = $@"select e.EntSecuencia, e.CliID,  c.CliCodigo, c.CliNombre, ifnull(sum((((cast(cd.EntPrecio as float) + cast(cd.EntSelectivo as float) + cast(cd.EntAdValorem as float) - cast(cd.EntDescuento as float)) 
							+ (cast(cd.EntPrecio as float) + cast(cd.EntSelectivo as float) + cast(cd.EntAdValorem as float) - cast(cd.EntDescuento as float)) * (cd.EntItbis /100.0))
							 * cast(cd.EntCantidad as float)  ) ),0)  as EntTotal  
                              from EntregasTransacciones e
                              inner join EntregasTransaccionesDetalle cd on cd.Repcodigo = e.Repcodigo and cd.EntSecuencia = e.EntSecuencia
                              inner join Clientes c on c.Cliid  = e.Cliid where e.Repcodigo = '{Arguments.CurrentUser.RepCodigo}' and e.CuaSecuencia = {cuaSecuencia} and e.EntEstatus != 0
                              group by  e.EntSecuencia, e.CliID,  c.CliCodigo, c.CliNombre 
                              UNION
                              select e.EntSecuencia, e.CliID,  c.CliCodigo, c.CliNombre, ifnull(sum((((cast(cd.EntPrecio as float) + cast(cd.EntSelectivo as float) + cast(cd.EntAdValorem as float) - cast(cd.EntDescuento as float)) 
							+ (cast(cd.EntPrecio as float) + cast(cd.EntSelectivo as float) + cast(cd.EntAdValorem as float) - cast(cd.EntDescuento as float)) * (cd.EntItbis /100.0))
							 * cast(cd.EntCantidad as float)  ) ),0)  as EntTotal  
                              from EntregasTransaccionesConfirmados e
                              inner join EntregasTransaccionesDetalleConfirmados cd on cd.Repcodigo = e.Repcodigo and cd.EntSecuencia = e.EntSecuencia
                              inner join Clientes c on c.Cliid  = e.Cliid where e.Repcodigo = '{Arguments.CurrentUser.RepCodigo}' and e.CuaSecuencia = {cuaSecuencia} and e.EntEstatus != 0
                              group by  e.EntSecuencia, e.CliID,  c.CliCodigo, c.CliNombre ";

            return SqliteManager.GetInstance().Query<EntregasTransacciones>(query, new string[] { });
        }

        public List<EntregasTransaccionesDetalle> getProductosEntregasTransaccionesRealizadas(int cuaSecuencia)
        {
            string query = $@"select ed.proid, p.ProCodigo, p.ProDescripcion, sum(ed.EntCantidad) EntCantidad, 0 EntCantidadDetalle ,ProUnidades    from EntregasTransaccionesDetalle ed
			   inner join productos p on p.proid = ed.Proid	
			   inner join EntregasTransacciones et on et.Repcodigo = ed.Repcodigo and et.EntSecuencia = ed.EntSecuencia
			   where ed.Repcodigo ='{Arguments.CurrentUser.RepCodigo}' and et.CuaSecuencia= {cuaSecuencia} and et.EntEstatus != 0
			   group by ed.proid, p.ProCodigo, p.ProDescripcion, ProUnidades		   
			   union 			   
			   select ed.proid, p.ProCodigo, p.ProDescripcion, sum(ed.EntCantidad) EntCantidad,  0 EntCantidadDetalle ,ProUnidades    from EntregasTransaccionesDetalleConfirmados ed
			   inner join productos p on p.proid = ed.Proid	
			   inner join EntregasTransaccionesConfirmados et on et.Repcodigo = ed.Repcodigo and et.EntSecuencia = ed.EntSecuencia
			   where ed.Repcodigo ='{Arguments.CurrentUser.RepCodigo}' and et.CuaSecuencia= {cuaSecuencia} and et.EntEstatus != 0
			   group by ed.proid, p.ProCodigo, p.ProDescripcion, ProUnidades";

            return SqliteManager.GetInstance().Query<EntregasTransaccionesDetalle>(query, new string[] { });
        }

        public List<EntregasTransaccionesDetalle> getProductosEntregasTransaccionesRealizadasCajasYUnidades(int cuaSecuencia)
        {
            string query = $@"select ed.proid, p.ProCodigo, p.ProDescripcion
               , sum(case ifnull(ed.UnmCodigo,'') when 'CJ' then ed.EntCantidad else 0 end) EntCantidad
               , sum(case when ifnull(ed.UnmCodigo,'') != 'CJ' then ed.EntCantidad else 0 end) EntCantidadDetalle
               , ProUnidades    
               from EntregasTransaccionesDetalle ed
			   inner join productos p on p.proid = ed.Proid	
			   inner join EntregasTransacciones et on et.Repcodigo = ed.Repcodigo and et.EntSecuencia = ed.EntSecuencia
			   where ed.Repcodigo ='{Arguments.CurrentUser.RepCodigo}' and et.CuaSecuencia= {cuaSecuencia} and et.EntEstatus != 0
			   group by ed.proid, p.ProCodigo, p.ProDescripcion, ProUnidades		   
			   union 			   
			   select ed.proid, p.ProCodigo, p.ProDescripcion
               , sum(case ifnull(ed.UnmCodigo,'') when 'CJ' then ed.EntCantidad else 0 end) EntCantidad
               , sum(case when ifnull(ed.UnmCodigo,'') != 'CJ' then ed.EntCantidad else 0 end) EntCantidadDetalle
               , ProUnidades    
               from EntregasTransaccionesDetalleConfirmados ed
			   inner join productos p on p.proid = ed.Proid	
			   inner join EntregasTransaccionesConfirmados et on et.Repcodigo = ed.Repcodigo and et.EntSecuencia = ed.EntSecuencia
			   where ed.Repcodigo ='{Arguments.CurrentUser.RepCodigo}' and et.CuaSecuencia= {cuaSecuencia} and et.EntEstatus != 0
			   group by ed.proid, p.ProCodigo, p.ProDescripcion, ProUnidades";

            return SqliteManager.GetInstance().Query<EntregasTransaccionesDetalle>(query, new string[] { });
        }
        public List<EntregasTransacciones> GetMontoTotalEntregasPorSectorFromCuadre(int cuaSecuencia)
        {
            string query = $@"select  e.SecCodigo , ifnull(sum((((cast(cd.EntPrecio as float) + cast(cd.EntSelectivo as float) + cast(cd.EntAdValorem as float) - cast(cd.EntDescuento as float)) 
							+ (cast(cd.EntPrecio as float) + cast(cd.EntSelectivo as float) + cast(cd.EntAdValorem as float) - cast(cd.EntDescuento as float)) * (cd.EntItbis /100.0))
							 * cast(cd.EntCantidad as float)  ) ),0)  as EntTotal  
                              from EntregasTransacciones e
                              inner join EntregasTransaccionesDetalle cd on cd.Repcodigo = e.Repcodigo and cd.EntSecuencia = e.EntSecuencia
                              inner join Sectores s on s.SecCodigo  = e.SecCodigo where e.Repcodigo = '{Arguments.CurrentUser.RepCodigo}' and e.CuaSecuencia = {cuaSecuencia} and e.EntEstatus != 0
                              group by  e.SecCodigo, s.SecDescripcion 
                              UNION
                              select  e.SecCodigo,  ifnull(sum((((cast(cd.EntPrecio as float) + cast(cd.EntSelectivo as float) + cast(cd.EntAdValorem as float) - cast(cd.EntDescuento as float)) 
							+ (cast(cd.EntPrecio as float) + cast(cd.EntSelectivo as float) + cast(cd.EntAdValorem as float) - cast(cd.EntDescuento as float)) * (cd.EntItbis /100.0))
							 * cast(cd.EntCantidad as float)  ) ),0)  as EntTotal  
                              from EntregasTransaccionesConfirmados e
                              inner join EntregasTransaccionesDetalleConfirmados cd on cd.Repcodigo = e.Repcodigo and cd.EntSecuencia = e.EntSecuencia                              
                              where e.Repcodigo = '{Arguments.CurrentUser.RepCodigo}' and e.CuaSecuencia = {cuaSecuencia} and e.EntEstatus != 0
                              group by  e.SecCodigo ";

            return SqliteManager.GetInstance().Query<EntregasTransacciones>(query, new string[] { });
        }
        public string GetEntregaTransaccionNumeroFactura(int cliId, int venSecuencia, string venNCF, bool confirmada)
        {
            string numeroFactura = "";
            //string tabla = confirmada ? "EntregasRepartidorTransaccionesConfirmados" : "EntregasRepartidorTransacciones";
            string query = $@"select ifnull(venNumeroERP,'') as venNumeroERP from  EntregasRepartidorTransacciones e  
                            where e.CliID = {cliId} and e.TraSecuencia =  {venSecuencia} and e.VenNCF ='{venNCF}'
                            union
                            select ifnull(venNumeroERP,'') as venNumeroERP from  EntregasRepartidorTransaccionesConfirmados e  
                            where e.CliID = {cliId} and e.TraSecuencia =  {venSecuencia} and e.VenNCF ='{venNCF}' ";
            var entregaRepartidor = SqliteManager.GetInstance().Query<EntregasRepartidorTransacciones>(query).FirstOrDefault();
            if (entregaRepartidor != null)
            {
                numeroFactura = entregaRepartidor.venNumeroERP;
            }
            return numeroFactura;
        }
        public string GetEntregaEntregasRepartidorTransaccionesNumeroFactura(int cliId, int TraSecuencia, string venNCF, bool confirmada, int enrSecuencia)
        {
            string numeroFactura = "";
            //string tabla = confirmada ? "EntregasRepartidorTransaccionesConfirmados" : "EntregasRepartidorTransacciones";
            string query = $@"select ifnull(venNumeroERP,'') as venNumeroERP from  EntregasRepartidorTransacciones e  
                            where e.CliID = {cliId} and e.TraSecuencia =  {TraSecuencia} and e.VenNCF ='{venNCF}' and e.EnrSecuencia= '{enrSecuencia}'
                            union
                            select ifnull(venNumeroERP,'') as venNumeroERP from  EntregasRepartidorTransaccionesConfirmados e  
                            where e.CliID = {cliId} and e.TraSecuencia =  {TraSecuencia} and e.VenNCF ='{venNCF}' and e.EnrSecuencia= '{enrSecuencia}' ";
            var entregaRepartidor = SqliteManager.GetInstance().Query<EntregasRepartidorTransacciones>(query).FirstOrDefault();
            if(entregaRepartidor != null)
            {
                numeroFactura = entregaRepartidor.venNumeroERP;
            }
            return numeroFactura;
        }

        public bool SePuedeAnularEntrega(int enrSecuencia, int trasecuencia)
        {
            var devs = SqliteManager.GetInstance().Query<Devoluciones>("select DevSecuencia from Devoluciones " +
                "where EnrSecuencia = ? and DevCintillo = ? and trim(RepCodigo) = '"+Arguments.CurrentUser.RepCodigo.Trim()+"'", 
                new string[] { enrSecuencia.ToString(), trasecuencia.ToString() });

            foreach(var dev in devs)
            {
                var conduce = SqliteManager.GetInstance().Query<Conduces>("select DevSecuencia from ConducesDetalle cd " +
                    "Inner Join Conduces c on c.ConSecuencia = cd.ConSecuencia and c.RepCodigo = cd.RepCodigo " +
                    "where DevSecuencia = ? and trim(cd.RepCodigo) = '"+Arguments.CurrentUser.RepCodigo.Trim()+"' limit 1", 
                    new string[] { dev.DevSecuencia.ToString() });

                if(conduce != null && conduce.Count > 0)
                {
                    return false;
                }
            }

            return true;
        }

      /*  private EntregasRepartidorTransacciones GetEntregaRepartidorFromEntrega(int entSecuencia)
        {
            var list = SqliteManager.GetInstance().Query<EntregasRepartidorTransacciones>("select * from EntregasRepartidorTransacciones t " +
                "inner join EntregasTransacciones e on e.RepCodigo = t.RepCodigo and e.RepVendedor = t.RepVendedor and t.EnrSecuencia = e.EnrSecuencia " +
                "and t.CliID = e.CliID and t.TraSecuencia = e.VenSecuencia where e.RepCodigo = ? and e.EntSecuencia = ? and t.CliID = ?", 
                new string[] { Arguments.CurrentUser.RepCodigo.Trim(), entSecuencia.ToString(), Arguments.Values.CurrentClient.CliID.ToString() });

            if(list != null && list.Count > 0){
                return list[0];
            }

            return null;
        }*/

        public void AnularEntrega(EntregasRepartidorTransacciones entrega)
        {
            var map = new Hash("EntregasTransacciones");
            map.Add("EntEstatus", 0);
            map.Add("EntFechaActualizacion", Functions.CurrentDate());


            if (new DS_SuscriptoresCambios().UpdateCambioEstadoInsertByRowguid(entrega.rowguid, 0))
            {
                map.SaveScriptForServer = false;
            }

            map.ExecuteUpdate("EntSecuencia = " + entrega.EntSecuencia.ToString() + " and RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");

            if (myParametro.GetParEntregasRepartidorGuardarRecibo())
            {
                var recibo = SqliteManager.GetInstance().Query<Recibos>("select RecSecuencia from Recibos where RecTipo = '4' and RecNumero = ?",
                    new string[] { entrega.EnrSecuencia.ToString() }).FirstOrDefault();

                if (recibo != null)
                {
                    var rec = new Hash("Recibos");
                    rec.Add("RecEstatus", 0);
                    rec.Add("RecFechaActualizacion", Functions.CurrentDate());
                    rec.ExecuteUpdate("RecTipo = '4' and RecSecuencia = " + recibo.RecSecuencia.ToString() + " and RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");
                }
            }

            var entregaRep = new Hash("EntregasRepartidorTransacciones");
            entregaRep.Add("enrEstatusEntrega", 1);
            entregaRep.Add("EntFechaActualizacion", Functions.CurrentDate());
            entregaRep.ExecuteUpdate("EnrSecuencia = " + entrega.EnrSecuencia + " " +
                "and RepCodigo = '"+Arguments.CurrentUser.RepCodigo+"' " +
                "and CliID = " + Arguments.Values.CurrentClient.CliID.ToString() + " and TraSecuencia = " + entrega.TraSecuencia);

            if (myParametro.GetParUsarMultiAlmacenes())
            {
                var myInv = new DS_Inventarios();

                var parInvSinLote = myParametro.GetParEntregasControlInventarioSinLotes();

                var parTipoComprobanteFiscal = myParametro.GetParTipoComprobanteFiscal();
                var almIdDespacho = myParametro.GetParAlmacenIdParaDespacho();
                var almIdDevoluciones = myParametro.GetParAlmacenIdParaDevolucion();

                var productosEntregados = SqliteManager.GetInstance().Query<EntregasTransaccionesDetalle>("select e.EntCantidad as EntCantidad, " +
                    "e.EntCantidadDetalle as EntCantidadDetalle, " +
                    "e.ProID as ProID from EntregasTransaccionesDetalle e " +
                    "inner join Productos p on p.ProID = e.ProID and ifnull(p.ProDatos3, '') not like '%L%' " +
                    "where e.EntSecuencia = ? and trim(e.RepCodigo) = '"+Arguments.CurrentUser.RepCodigo.Trim()+"'", 
                    new string[] { entrega.EntSecuencia.ToString() });

                foreach(var prod in productosEntregados)
                {
                    myInv.AgregarInventario(prod.ProID, prod.EntCantidad, prod.EntCantidadDetalle, almIdDespacho);
                }

                var productosEntregadosLotes = SqliteManager.GetInstance().Query<EntregasTransaccionesDetalle>("select l.EntCantidad as EntCantidad, l.EntCantidadDetalle as EntCantidadDetalle, " +
                    "l.EntLote as Lote, e.ProID as ProID from EntregasTransaccionesDetalleLotes l " +
                    "inner join EntregasTransaccionesDetalle e on e.EntSecuencia = l.EntSecuencia and e.RepCodigo = l.RepCodigo and l.EntPosicion = e.EntPosicion " +
                    "where l.EntSecuencia = ? and trim(l.RepCodigo) = '"+Arguments.CurrentUser.RepCodigo.Trim()+"'", new string[] { entrega.EntSecuencia.ToString() });

                foreach(var lote in productosEntregadosLotes)
                {
                    myInv.AgregarInventario(lote.ProID, lote.EntCantidad, lote.EntCantidadDetalle, almIdDespacho, parInvSinLote ? "" : lote.Lote);
                }

                if (myParametro.GetParEntregasRepartidor() != 3)
                {
                    var productosNoDespachados = SqliteManager.GetInstance().Query<DevolucionesDetalle>("select d.DevCantidad as DevCantidad, " +
                    "d.DevCantidadDetalle as DevCantidadDetalle, d.DevSecuencia as DevSecuencia, " +
                    "d.ProID as ProID from DevolucionesDetalle d " +
                    "inner join Devoluciones p on p.DevSecuencia = d.DevSecuencia and p.RepCodigo = d.RepCodigo and p.DevEstatus <> 0 " +
                    "where p.EnrSecuencia = ? and p.DevCintillo = ? and trim(p.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'",
                    new string[] { entrega.EnrSecuencia.ToString(), entrega.TraSecuencia.ToString() });

                    if (productosNoDespachados.Count > 0)
                    {
                        var dev = new Hash("Devoluciones");
                        dev.Add("DevEstatus", 0);
                        dev.ExecuteUpdate("DevSecuencia = " + productosNoDespachados[0].DevSecuencia + " and RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");
                    }

                    if (!string.IsNullOrWhiteSpace(entrega.VenNCF) && !string.IsNullOrWhiteSpace(parTipoComprobanteFiscal))
                    {
                        if (entrega.VenNCF.Length >= 3 && entrega.VenNCF.ToUpper().Substring(1, 2).Equals(parTipoComprobanteFiscal))
                        {
                            almIdDevoluciones = myParametro.GetAlmacenIdProductosNoDevolucion();
                        }
                    }

                    foreach (var noDes in productosNoDespachados)
                    {
                        myInv.RestarInventario(noDes.ProID, noDes.DevCantidad, noDes.DevCantidadDetalle, almIdDevoluciones);
                        myInv.AgregarInventario(noDes.ProID, noDes.DevCantidad, noDes.DevCantidadDetalle, almIdDespacho);
                    }
                }
                else
                {
                    var productosNoDespachados = SqliteManager.GetInstance().Query<EntregasTransaccionesDetalle>("select (e.EntCantidadSolicitada - e.EntCantidad) as EntCantidad, " +
                    "(e.EntCantidadDetalleSolicitada - e.EntCantidadDetalle) as EntCantidadDetalle, " +
                    "e.ProID as ProID from EntregasTransaccionesDetalle e " +
                    "inner join Productos p on p.ProID = e.ProID " +
                    "where e.EntSecuencia = ? and ((e.EntCantidadSolicitada - e.EntCantidad) > 0 or (e.EntCantidadDetalleSolicitada - e.EntCantidadDetalle) > 0) and trim(e.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'",
                    new string[] { entrega.EntSecuencia.ToString() });

                    foreach (var noDes in productosNoDespachados)
                    {
                        myInv.RestarInventario(noDes.ProID, noDes.EntCantidad, noDes.EntCantidadDetalle, almIdDevoluciones);
                        myInv.AgregarInventario(noDes.ProID, noDes.EntCantidad, noDes.EntCantidadDetalle, almIdDespacho);
                    }

                    var productosNoEntregados = GetProductosNoEntregadosxEntrega(entrega.EntSecuencia, entrega.EnrSecuencia, entrega.TraSecuencia, 1);
                    foreach (var noNoEnt in productosNoEntregados)
                    {
                        myInv.RestarInventario(noNoEnt.ProID, noNoEnt.TraCantidad, noNoEnt.TraCantidadDetalle, almIdDevoluciones);
                        myInv.AgregarInventario(noNoEnt.ProID, noNoEnt.TraCantidad, noNoEnt.TraCantidadDetalle, almIdDespacho);
                    }
                }

                
            }
        }

        public void ConducesActualizarCantidadSolicitada()
        {
            SqliteManager.GetInstance().Execute("update EntregasDetalleTemp set CantidadSolicitada = Cantidad, IsAdded = 1, CantidadSolicitadaDetalle = CantidadDetalle " +
                "where Cantidad > 0 or CantidadDetalle > 0 and ifnull(UsaLote, 0) = 1", 
                new string[] { });
        }

        public void ConducesRestablecerCantidadSolicitada()
        {
            SqliteManager.GetInstance().Execute("update EntregasDetalleTemp set CantidadSolicitada = CantidadDisponibleOriginal, IsAdded = 0, " +
                "CantidadSolicitadaDetalle = CantidadDisponibleDetalleOriginal where ifnull(UsaLote, 0) = 1");

            var list = SqliteManager.GetInstance().Query<EntregasDetalleTemp>("select * from EntregasDetalleTemp e where UsaLote = '1' and ifnull(Lote, '') = '' " +
                "and Posicion in (select Posicion from EntregasDetalleTemp where Posicion = e.Posicion and ProID = e.ProID and UsaLote = '1' and ifnull(Lote, '') != '' and Cantidad > 0)", 
                new string[] { });

            foreach(var p in list)
            {
                SqliteManager.GetInstance().Delete(p);
            }
            
            var conLoteDoble = SqliteManager.GetInstance().Query<EntregasDetalleTemp>("select e.ProID as ProID, sum(Cantidad) as Cantidad, e.Posicion as Posicion from EntregasDetalleTemp e " +
                "where UsaLote = '1' and ifnull(Lote, '') != '' group by e.ProID, e.Posicion having count(e.ProID) > 1", 
                new string[] { });

            foreach (var proLo in conLoteDoble)
            {
                var toInsert = SqliteManager.GetInstance().Query<EntregasDetalleTemp>("select * from EntregasDetalleTemp " +
                    "where Posicion = ? and ProID = ? and UsaLote = '1' limit 1", 
                    new string[] { proLo.Posicion.ToString(), proLo.ProID.ToString() });

                if(toInsert.Count == 0)
                {
                    continue;
                }

                SqliteManager.GetInstance().Execute("delete from EntregasDetalleTemp where Posicion = ? and ProID = ? and ifnull(UsaLote, 0) = 1 ", 
                    new string[] { proLo.Posicion.ToString(), proLo.ProID.ToString() });

                var pro = toInsert[0];
                pro.Cantidad = proLo.Cantidad;
                pro.Lote = "";
                SqliteManager.GetInstance().Insert(pro);                
            }

            SqliteManager.GetInstance().Execute("update EntregasDetalleTemp set Lote = '' where ifnull(UsaLote, 0) = 1 and ifnull(Lote, '') != ''", new string[] { });


        }

        public bool HayEntregasRechazadas(int enrSecuencia)
        {
            var whereSector = "";

            if(myParametro.GetParEntregasRepartidorPorSector() && Arguments.Values.CurrentSector != null)
            {
                whereSector += " and ifnull(SecCodigo, '') = '" + Arguments.Values.CurrentSector.SecCodigo + "' ";
            }

            string titID = " TitID = 4 ";

            if (myParametro.GetParEntregasRepartidor() == 3)
            {
                titID = " TitID = 1 ";
            }

            return SqliteManager.GetInstance().Query<EntregasTransacciones>("select 1 from EntregasRepartidorTransacciones where "+ titID + " " +
                "and trim(RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and CliID = " + Arguments.Values.CurrentClient.CliID.ToString() + " " +
                "and enrEstatusEntrega = 0 and ifnull(MotID, 0) = 0 and EnrSecuencia = ?" + whereSector,
                new string[] { enrSecuencia.ToString() }).Count > 0;
        }

        public List<EntregasRepartidorTransacciones> GetEntregasRechazadasSinMotivo()
        {
            var whereSector = "";

            if (myParametro.GetParEntregasRepartidorPorSector() && Arguments.Values.CurrentSector != null)
            {
                whereSector += " and ifnull(SecCodigo, '') = '" + Arguments.Values.CurrentSector.SecCodigo + "' ";
            }

            string titID = " TitID = 4 ";

            if (myParametro.GetParEntregasRepartidor() == 3)
            {
                titID = " TitID = 1 ";
            }

            return SqliteManager.GetInstance().Query<EntregasRepartidorTransacciones>("select EnrSecuencia, TraSecuencia from EntregasRepartidorTransacciones where " + titID + " " +
                "and trim(RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and CliID = " + Arguments.Values.CurrentClient.CliID.ToString() + " " +
                "and enrEstatusEntrega = 0 and ifnull(MotID, 0) = 0 " + whereSector,
                new string[] { });
        }

        public bool HayProductosSinLote()
        {
            return SqliteManager.GetInstance().Query<EntregasDetalleTemp>("select 1 from EntregasDetalleTemp " +
                "where ifnull(UsaLote, 0) = 1 and ifnull(Lote, '') = '' and ifnull(IsAdded, 0) = 1 limit 1", new string[] { }).Count > 0;     
        }

        public void ActualizarMotivoEntregaRechazada(int motId, List<EntregasRepartidorTransacciones> entregas)
        {

            foreach (var entrega in entregas)
            {
                var ent = new Hash("EntregasRepartidorTransacciones");
                ent.Add("MotID", motId);
                ent.ExecuteUpdate("CliID = "+Arguments.Values.CurrentClient.CliID.ToString()+" and EnrSecuencia = " + entrega.EnrSecuencia.ToString() + " and TraSecuencia = " + entrega.TraSecuencia.ToString() + " and RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");
            }

            var devoluciones = GetDevolucionesSinMotivo();

            foreach(var dev in devoluciones)
            {
                var map = new Hash("DevolucionesDetalle");
                map.Add("MotID", motId);
                map.ExecuteUpdate("RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and DevSecuencia = " + dev.DevSecuencia.ToString());
            }
        }

        public EntregasRepartidor GetEntregaRepartidor(int enrSecuencia, bool confirmado)
        {
            return SqliteManager.GetInstance().Query<EntregasRepartidor>("select * from "+(confirmado? "EntregasRepartidorConfirmados" : "EntregasRepartidor") +" where trim(RepCodigo) = ? and EnrSecuencia = ?", 
                new string[] { Arguments.CurrentUser.RepCodigo.Trim(), enrSecuencia.ToString() }).FirstOrDefault();
        }

        public List<EntregasRepartidorTransacciones> GetEntregasRepartidorTransaccionesBySecuencia(int enrSecuencia, bool confirmado, string wherecondition = "")
        {
            string titID = myParametro.GetParEntregasRepartidor() == 2 ? " " : myParametro.GetParEntregasRepartidor() == 3 ? " and TitID = 1 " : " and TitID = 4 " ;

            string where = $"{titID} and EnrSecuencia = {enrSecuencia.ToString()} ";

            var query = "select t.*, c.CliNombre, c.CliCodigo, 0 as ShowVerDetalleBtn, ifnull(e.EstDescripcion, 'No entregado') as estatusDescripcion, " +
                "t.ConID as ConID, t.RepVendedor as RepVendedor, s.SecDescripcion as SecDescripcion, " +
                "(select sum((((cast(cd.TraPrecio as float) + cast(cd.TraSelectivo as float) + cast(cd.TraAdValorem as float) - cast(cd.TraDescuento as float)) " +
                           " + (cast(cd.TraPrecio as float) + cast(cd.TraSelectivo as float) + cast(cd.TraAdValorem as float) - cast(cd.TraDescuento as float)) * (cd.TraItbis / 100.0)) " +
							" *cast(cd.TraCantidad as float)  ) )  as Total " +
                "from "+(confirmado? "EntregasRepartidorTransaccionesDetalleConfirmados" : "EntregasRepartidorTransaccionesDetalle") +" cd where cd.EnrSecuencia = t.EnrSecuencia and cd.TraSecuencia = t.TraSecuencia and cd.RepCodigo = t.RepCodigo and cd.TitID = t.TitID and CliID = t.CliID) as EntMontoTotal " +
                "from "+(confirmado? "EntregasRepartidorTransaccionesConfirmados" : "EntregasRepartidorTransacciones") +" t " +
                "inner join Clientes c on c.CliID = t.CliID " +
                "left join Estados e on e.EstTabla = 'EntregasRepartidorTransacciones' and e.EstEstado = t.enrEstatusEntrega " +
                "left join Sectores s on s.SecCodigo = t.SecCodigo " +
                "where trim(t.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' " + where + "" + wherecondition + "  order by t.SecCodigo";

            return SqliteManager.GetInstance().Query<EntregasRepartidorTransacciones>(query,
                new string[] { });
        }

        public bool HayEntregasSinImprimir(List<int> entSecuencias)
        {
            var where = "";

            foreach(var entSec in entSecuencias)
            {
                where += string.IsNullOrWhiteSpace(where) ? entSec.ToString() : "," + entSec.ToString();
            }

            string titID = " TitID = 4 ";

            if (myParametro.GetParEntregasRepartidor() == 3)
            {
                titID = " TitID = 1 ";
            }

            var list = SqliteManager.GetInstance().Query<EntregasTransacciones>("select EntSecuencia from EntregasTransacciones " +
                "where "+ titID + " and EntSecuencia in ("+where+") and trim(RepCodigo) = '"+Arguments.CurrentUser.RepCodigo.Trim()+ "' and ifnull(EntCantidadImpresion, 0) = 0", 
                new string[] { });

            return list != null && list.Count > 0;
        }

        public bool ExisteEntregaHuerfana(out string monCodigo, out Clientes cliente, out int visSecuencia, out string secCodigo, out List<int> entSecuencias)
        {
            var conIdContado = myParametro.GetParConIdFormaPagoContado();

            monCodigo = "";
            cliente = null;
            visSecuencia = -1;
            secCodigo = "";
            entSecuencias = new List<int>();

            try
            {

                var lista = SqliteManager.GetInstance().Query<EntregasTransacciones>("select * from EntregasTransacciones v where RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ConID = ? " +
                    "and not exists(select 1 from RecibosAplicacion where RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and trim(RecTipo) = '1' " +
                    "and trim(CxcDocumento) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "-'||cast(v.EntSecuencia as TEXT)) " +
                    "and not exists(select 1 from RecibosAplicacionConfirmados where RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and trim(RecTipo) in ('1') " +
                    "and trim(CxcDocumento) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "-'||cast(v.EntSecuencia as TEXT)) " +
                    "and cast(replace(ifnull(v.mbVersion, ''), '.', '') as int) >= cast(replace('7.2.42', '.', '') as int) " +
                    "and ifnull(v.mbVersion, '') not like '6.%' " +
                    "and ifnull(v.mbVersion, '') not like '7.1.%' ", new string[] { conIdContado.ToString() });

                if (lista != null && lista.Count > 0)
                {
                    new DS_Recibos().ClearTemps();

                    foreach (var entrega in lista)
                    {
                        cliente = new DS_Clientes().GetClienteById(entrega.CliID);

                        if (cliente == null)
                        {
                            return false;
                        }

                        monCodigo = cliente.MonCodigo;
                        visSecuencia = entrega.VisSecuencia;
                        secCodigo = entrega.SecCodigo;
                        entSecuencias.Add(entrega.EntSecuencia);

                        var totales = GetEntregaTotales(entrega.EntSecuencia);

                        var reciboToSave = new RecibosDocumentosTemp();
                        reciboToSave.FechaSinFormatear = Functions.CurrentDate();
                        reciboToSave.Fecha = Functions.CurrentDate("dd-MM-yyyy");
                        reciboToSave.Documento = Arguments.CurrentUser.RepCodigo + "-" + entrega.EntSecuencia;
                        reciboToSave.Referencia = "ENT-" + Arguments.CurrentUser.RepCodigo + "-" + entrega.EntSecuencia;
                        reciboToSave.Sigla = "FAT";
                        reciboToSave.Aplicado = totales.Total;
                        reciboToSave.Descuento = 0;
                        reciboToSave.MontoTotal = totales.Total;
                        reciboToSave.Balance = totales.Total;
                        reciboToSave.Pendiente = 0;
                        reciboToSave.Estado = "Saldo";
                        reciboToSave.Credito = 0;
                        reciboToSave.FechaIngles = Functions.CurrentDate("MM-dd-yyyy");
                        reciboToSave.Origen = 1;
                        reciboToSave.MontoSinItbis = totales.SubTotal;
                        reciboToSave.DescPorciento = 0;
                        reciboToSave.AutSecuencia = 0;
                        reciboToSave.FechaDescuento = Functions.CurrentDate("dd-MM-yyyy");
                        reciboToSave.Dias = 0;
                        reciboToSave.DescuentoFactura = 0;
                        reciboToSave.Clasificacion = "";
                        reciboToSave.FechaVencimiento = Functions.CurrentDate();
                        reciboToSave.Retencion = 0;
                        reciboToSave.CXCNCF = "";
                        reciboToSave.cxcComentario = "";
                        reciboToSave.RecTipo = 1;

                        SqliteManager.GetInstance().Insert(reciboToSave);
                        
                    }

                    return true;
                }

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return false;
        }

        public List<Pedidos> GetPedidosByCuadre(int CuaSecuencia)
        {
            var query = $@"select p.PedSecuencia, p.CliID,  c.CliCodigo, c.CliNombre, ifnull(sum((((cast(pd.PedPrecio as float) + cast(pd.PedSelectivo as float) + cast(pd.PedAdValorem as float) - cast(pd.PedDescuento as float)) 
                + (cast(pd.PedPrecio as float) + cast(pd.PedSelectivo as float) + cast(pd.PedAdValorem as float) - cast(pd.PedDescuento as float)) * (pd.PedItbis / 100.0))
                 *cast(pd.PedCantidad as float)  ) ),0)  as PedMontoTotal
                  from Pedidos p
                  inner
                  join PedidosDetalleConfirmados pd on pd.Repcodigo = p.Repcodigo and pd.PedSecuencia = p.PedSecuencia
                  inner join Clientes c on c.Cliid = p.Cliid where p.Repcodigo = '{Arguments.CurrentUser.RepCodigo}' and p.CuaSecuencia = {CuaSecuencia} and p.PedEstatus != 0
                  group by  p.PedSecuencia, p.CliID,  c.CliCodigo, c.CliNombre
                  UNION
                  select p.PedSecuencia, p.CliID,  c.CliCodigo, c.CliNombre, ifnull(sum((((cast(pd.PedPrecio as float) + cast(pd.PedSelectivo as float) + cast(pd.PedAdValorem as float) - cast(pd.PedDescuento as float))
                + (cast(pd.PedPrecio as float) + cast(pd.PedSelectivo as float) + cast(pd.PedAdValorem as float) - cast(pd.PedDescuento as float)) * (pd.PedItbis / 100.0))
                 * cast(pd.PedCantidad as float))), 0) as PedMontoTotal
                  from PedidosConfirmados P
                  inner
                  join PedidosDetalleConfirmados PD on pd.Repcodigo = p.Repcodigo and pd.PedSecuencia = p.PedSecuencia
                  inner join Clientes c on c.Cliid = p.Cliid where p.Repcodigo = '{Arguments.CurrentUser.RepCodigo}' and p.CuaSecuencia = {CuaSecuencia} and p.PedEstatus != 0
                  group by  p.PedSecuencia, p.CliID,  c.CliCodigo, c.CliNombre ";




            return SqliteManager.GetInstance().Query<Pedidos>(query, new string[] { });
        }


        public List<PedidosDetalle> GetProductosPedidosRealizados(int cuaSecuencia)
        {
            string query = $@"select ed.proid, p.ProCodigo, p.ProDescripcion, sum(ed.PedCantidad) PedCantidad, 0 PedCantidadDetalle ,ProUnidades    
                            from PedidosDetalle ed
                            inner join productos p on p.proid = ed.Proid	
                            inner join Pedidos et on et.Repcodigo = ed.Repcodigo and et.PedSecuencia = ed.PedSecuencia
                            where ed.Repcodigo ='{Arguments.CurrentUser.RepCodigo}' and et.CuaSecuencia= {cuaSecuencia} and et.PedEstatus != 0
                            group by ed.proid, p.ProCodigo, p.ProDescripcion, ProUnidades		   
                            union 			   
                            select ed.proid, p.ProCodigo, p.ProDescripcion, sum(ed.PedCantidad) PedCantidad,  0 PedCantidadDetalle ,ProUnidades    
                            from PedidosDetalleConfirmados ed
                            inner join productos p on p.proid = ed.Proid	
                            inner join PedidosConfirmados et on et.Repcodigo = ed.Repcodigo and et.PedSecuencia = ed.PedSecuencia
                            where ed.Repcodigo ='{Arguments.CurrentUser.RepCodigo}' and et.CuaSecuencia= {cuaSecuencia} and et.PedEstatus != 0
                            group by ed.proid, p.ProCodigo, p.ProDescripcion, ProUnidades";

          
            return SqliteManager.GetInstance().Query<PedidosDetalle>(query, new string[] { });
        }

        public List<PedidosDetalle> GetProductosPedidosRealizadosCajasUnidades(int cuaSecuencia)
        {
            

            string query = $@"select ed.proid, p.ProCodigo, p.ProDescripcion
                            , sum(case ifnull(ed.UnmCodigo,'') when 'CJ' then ed.PedCantidad else 0 end) PedCantidad
                            , sum(case when ifnull(ed.UnmCodigo,'') != 'CJ' then ed.PedCantidad else 0 end) PedCantidadDetalle
                            , ProUnidades    
                            from PedidosDetalle ed
                            inner join productos p on p.proid = ed.Proid	
                            inner join Pedidos et on et.Repcodigo = ed.Repcodigo and et.PedSecuencia = ed.PedSecuencia
                            where ed.Repcodigo ='{Arguments.CurrentUser.RepCodigo}' and et.CuaSecuencia= {cuaSecuencia} and et.PedEstatus != 0
                            group by ed.proid, p.ProCodigo, p.ProDescripcion, ProUnidades		   
                            union 			   
                            select ed.proid, p.ProCodigo, p.ProDescripcion
                            , sum(case ifnull(ed.UnmCodigo,'') when 'CJ' then ed.PedCantidad else 0 end) PedCantidad
                            , sum(case when ifnull(ed.UnmCodigo,'') != 'CJ' then ed.PedCantidad else 0 end) PedCantidadDetalle
                            , ProUnidades    
                            from PedidosDetalleConfirmados ed
                            inner join productos p on p.proid = ed.Proid	
                            inner join PedidosConfirmados et on et.Repcodigo = ed.Repcodigo and et.PedSecuencia = ed.PedSecuencia
                            where ed.Repcodigo ='{Arguments.CurrentUser.RepCodigo}' and et.CuaSecuencia= {cuaSecuencia} and et.PedEstatus != 0
                            group by ed.proid, p.ProCodigo, p.ProDescripcion, ProUnidades";

            return SqliteManager.GetInstance().Query<PedidosDetalle>(query, new string[] { });
        }
        public List<Recibos> getDocumentosRec(string RepCodigo, int CuaSecuencia)
        {
            return SqliteManager.GetInstance().Query<Recibos>(
        "select Cli.CliCodigo, ifnull(Cli.CliNombre,'Cliente Suprimido') as CliNombre, Rec.RecSecuencia as RecSecuencia, Rec.RecTotal, Rec.RecTipo, Rec.RecMontoNcr as RecMontoNcr, "
        + "Rec.RecMontoDescuento as RecMontoDescuento, Rec.RecMontoEfectivo as RecMontoEfectivo, "
        + "Rec.RecMontoCheque as RecMontoCheque, Rec.RecMontoChequeF as RecMontoChequeF, "
        + "Rec.RecMontoTransferencia as RecMontoTransferencia, Rec.RecMontoSobrante as RecMontoSobrante, Rec.RecRetencion as RecRetencion, ra.cxcReferencia as cxcReferencia, Rec.RecTipo as RecTipo  "
        + " from Recibos Rec inner join Clientes Cli on Rec.CliID = Cli.CliID " +
          "left join RecibosAplicacion ra on ra.RecSecuencia = Rec.RecSecuencia " +
              "where Rec.RecEstatus <> 0 AND Rec.cuaSecuencia = " + CuaSecuencia + " AND Rec.RepCodigo = '" + RepCodigo + "' and Rec.RecTipo in(1, 2) Group by rec.recsecuencia " +
        "Union All " +
        "select Cli.CliCodigo, ifnull(Cli.CliNombre,'Cliente Suprimido') as CliNombre, Rec.RecSecuencia as RecSecuencia, Rec.RecTotal, Rec.RecTipo, Rec.RecMontoNcr as RecMontoNcr, "
        + "Rec.RecMontoDescuento as RecMontoDescuento, Rec.RecMontoEfectivo as RecMontoEfectivo, "
        + "Rec.RecMontoCheque as RecMontoCheque, Rec.RecMontoChequeF as RecMontoChequeF, "
        + "Rec.RecMontoTransferencia as RecMontoTransferencia, Rec.RecMontoSobrante as RecMontoSobrante, Rec.RecRetencion as RecRetencion, ra.cxcReferencia as cxcReferencia, Rec.RecTipo as RecTipo  "
        + " from RecibosConfirmados Rec inner join Clientes Cli on Rec.CliID = Cli.CliID " +
          "left join RecibosAplicacionConfirmados ra on ra.RecSecuencia = Rec.RecSecuencia " +
        "where Rec.RecEstatus <> 0 AND Rec.cuaSecuencia = " + CuaSecuencia + " AND Rec.RepCodigo = '" + RepCodigo + "' and Rec.RecTipo in(1, 2)  Group by rec.recsecuencia");
        }


        public List<Recibos> getRecibosCreditoByCuaSecuencia2(string RepCodigo, int CuaSecuencia)
        {
            return SqliteManager.GetInstance().Query<Recibos>(
            "select Cli.CliCodigo, Cli.CliNombre, Rec.RecSecuencia as RecSecuencia, Rec.RecTotal, Rec.RecTipo, Rec.RecMontoNcr as RecMontoNcr, "
            + "Rec.RecMontoDescuento as RecMontoDescuento, Rec.RecMontoEfectivo as RecMontoEfectivo, "
            + "Rec.RecMontoCheque as RecMontoCheque, Rec.RecMontoChequeF as RecMontoChequeF, "
            + "Rec.RecMontoTransferencia as RecMontoTransferencia, Rec.RecMontoSobrante as RecMontoSobrante, Rec.RecRetencion as RecRetencion, Rec.RecMontoTarjeta as RecMontoTarjeta  "
            + " from Recibos Rec inner join Clientes Cli on Rec.CliID = Cli.CliID " +
              "where Rec.RecEstatus <> 0 AND Rec.cuaSecuencia = " + CuaSecuencia + " AND Rec.RepCodigo = '" + RepCodigo + "' " +
              "Union all " +
              "select Cli.CliCodigo, Cli.CliNombre, Rec.RecSecuencia as RecSecuencia, Rec.RecTotal, Rec.RecTipo, Rec.RecMontoNcr as RecMontoNcr, "
            + "Rec.RecMontoDescuento as RecMontoDescuento, Rec.RecMontoEfectivo as RecMontoEfectivo, "
            + "Rec.RecMontoCheque as RecMontoCheque, Rec.RecMontoChequeF as RecMontoChequeF, "
            + "Rec.RecMontoTransferencia as RecMontoTransferencia, Rec.RecMontoSobrante as RecMontoSobrante, Rec.RecRetencion as RecRetencion, Rec.RecMontoTarjeta as RecMontoTarjeta  "
            + " from RecibosConfirmados Rec inner join Clientes Cli on Rec.CliID = Cli.CliID " +
              "where Rec.RecEstatus <> 0 AND Rec.cuaSecuencia = " + CuaSecuencia + " AND Rec.RepCodigo = '" + RepCodigo + "' ", new string[] { });


        }

        public string getFormasPago(string cxcReferencia)
        {
            var FormasPago = SqliteManager.GetInstance().Query<RecibosFormaPago>("Select FP.ForID, ifnull(FP.RefIndicadorDiferido, 0) as RefIndicadorDiferido "
                + "from RecibosFormaPago FP inner join RecibosAplicacion RA on FP.RecSecuencia = RA.RecSecuencia "
                + "Where RA.cxcReferencia = '" + cxcReferencia + "'", new string[] { });


            string FormaPago = "";

            foreach (var FP in FormasPago)
            {
                switch (FP.ForID)
                {
                    case 1:
                        if (FormaPago == "")
                        {
                            FormaPago += "EF";
                        }
                        else
                        {
                            FormaPago += "/EF";
                        }
                        break;
                    case 2:
                        if (FormaPago == "")
                        {
                            FormaPago += "CK";
                        }
                        else
                        {
                            FormaPago += "/CK";
                        }

                        break;
                    case 3:
                        if (FormaPago == "")
                        {
                            FormaPago += "NC";
                        }
                        else
                        {
                            FormaPago += "/NC";
                        }

                        break;
                    case 4:
                        if (FormaPago == "")
                        {
                            FormaPago += "TR";
                        }
                        else
                        {
                            FormaPago += "/TR";
                        }

                        break;
                    case 5:
                        if (FormaPago == "")
                        {
                            FormaPago += "RE";
                        }
                        else
                        {
                            FormaPago += "/RE";
                        }

                        break;
                    case 6:
                        if (FormaPago == "")
                        {
                            FormaPago += "TC";
                        }
                        else
                        {
                            FormaPago += "/TC";
                        }

                        break;
                    case 18:
                        if (FormaPago == "")
                        {
                            FormaPago += "OP";
                        }
                        else
                        {
                            FormaPago += "/OP";
                        }

                        break;
                }
            }
            return FormaPago;
        }
    }
}
