using MovilBusiness.Configuration;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.model.Internal.structs;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.DataAccess
{
    public class DS_PushMoneyPorPagar
    {

        public List<PushMoneyPorPagar> GetAllPushMoneyByCliente(int cliId)
        {
            var sql = "select result.* from (select RepCodigo, ifnull(replace( strftime('%d-%m-%Y', SUBSTR(PxpFechaEntrega,1,10)),' ','' ), '') as PxpFechaEntrega, ifnull(replace( strftime('%d-%m-%Y', SUBSTR(PxpFecha,1,10)),' ','' ), '') as PxpFecha, " +
                                  "julianday(Cast(strftime('%Y-%m-%d','now','" + Functions.GetDiferenciaHorariaSqlite() + " hours') as Varchar)) - julianday(SUBSTR(ifnull(PxpFecha, PxpFecha),1,10)) as PxpDias, " +
                                  "julianday(Cast(strftime('%Y-%m-%d','now','" + Functions.GetDiferenciaHorariaSqlite() + " hours') as Varchar)) - julianday(SUBSTR(ifnull(PxpFechaEntrega, ''),1,10)) as PxpDiasEntrega, " +
                                  "PxpDocumento, ifnull(ltrim(rtrim(upper(PxpSIGLA))), '') as PxpSIGLA, replace(ifnull(strftime('%d-%m-%Y', SUBSTR(PxpFechaVencimiento,1,10)),'0'),'','' ) as PxpFechaVencimiento, IFNULL(ttc.ttcOrigen, 1) as Origen, " +
                                  "'#ffffff' as color, " +
                                  "PxpBalance, (abs(PxpMontoSinItbis) * ifnull(ttcOrigen, 1)) as PxpMontoSinItbis, (abs(PxpMontoTotal) * ifnull(ttcOrigen, 1)) as PxpMontoTotal, Pxp.PxpNCF as PxpNCF, Pxp.PxpNCFAfectado as PxpNCFAfectado, Pxp.PxpComentario as PxpComentario, Pxp.CliID as CliID, PxpReferencia " +
                                  "from PushMoneyPorPagar Pxp left join TiposTransaccionesCxc ttc on trim(ttc.ttcReferencia) = PxpSigla where CliID = ? " +
                                  "UNION ALL " +
                                  "select RepCodigo, '' as PxpFechaEntrega, ifnull(replace( strftime('%d-%m-%Y', SUBSTR(pusFecha,1,10)),' ','' ), '') as PxpFecha, " +
                                  "ifnull(replace(cast(julianday('now', '" + Functions.GetDiferenciaHorariaSqlite() + " hours') - julianday(replace(pusFecha, 'T', ' ')) as integer),' ', ''), '') as PxpDias, 0 as PxpDiasEntrega, " +
                                  "pusSecuencia as PxpDocumento, 'RCB' as PxpSIGLA, '' as PxpFechaVencimiento, -1 as Origen, '0' as color, CAST(pusMontoEfectivo + pusMontoBono AS MONEY) as PxpBalance, (CAST(pusMontoEfectivo + pusMontoBono AS MONEY) * ifnull(ttcOrigen, 1)), " +
                                  "(CAST(pusMontoEfectivo + pusMontoBono AS MONEY) * ifnull(ttcOrigen, 1)) as PxpMontoTotal, '' as PxpNCF, '' as PxpNCFAfectado, '' as PxpComentario, r.CliID as CliID, '' as PxpReferencia " +
                                  "from PushMoneyPagos r left join TiposTransaccionesCxc ttc on trim(ttc.ttcReferencia) = 'RCB' " +
                                  "where CliID = ? and pusEstatus <> 0) as result ORDER BY CAST(PxpDias as int) DESC, PxpDocumento ASC";


            return SqliteManager.GetInstance().Query<PushMoneyPorPagar>(sql, new string[] { cliId.ToString(), cliId.ToString() });

        }

        public List<RecibosDocumentosTemp> GetAllPushMoneyPendientes()
        {
            var sql = "SELECT PxpFecha as FechaSinFormatear, CAST(replace(strftime('%d-%m-%Y', SUBSTR(PxpFecha,1,10)),' ','') as varchar) as Fecha, C.PxpDocumento as Documento, C.PxpReferencia as Referencia, " +
                        "C.PxpSigla as Sigla, 0 AS Aplicado, 0 as Descuento, C.PxpMontoTotal as MontoTotal, (C.PxpBalance - COALESCE(SUM(A.RecValor + A.RecDescuento),0)) AS Balance, " +
                        "(C.PxpBalance - COALESCE(SUM(A.RecValor + A.RecDescuento ),0)) AS Pendiente, 'Pendiente' as Estado, 0  as Credito, " +
                        "CAST(replace(strftime('%m-%d-%Y', SUBSTR(PxpFecha,1,10)),' ','' ) as varchar) as FechaIngles, ttcOrigen as Origen, PxpMontoSinItbis as MontoSinItbis, " +
                        "0 DescPorciento, 0 AutID, strftime('%d-%m-%Y','now','" + Functions.GetDiferenciaHorariaSqlite() + " hours') as FechaDescuento, julianday(Cast(strftime('%Y-%m-%d','now','" + Functions.GetDiferenciaHorariaSqlite() + " hours') as Varchar)) - julianday(SUBSTR(ifnull(PxpFechaEntrega, PxpFecha),1,10)) as Dias, 0 DescuentoFactura, " +
                        "IFNULL(PxpClasificacion, '') AS 'Clasificacion', IFNULL(date(PxpFechaVencimiento), '') AS 'FechaVencimiento', ifnull(PxpRetencion, 0) as Retencion, " +
                        "ifnull(C.PxpNotaCredito, 0) as CxcNotaCredito, 0 as CalcularDesc, C.PxpNCF as CXCNCF, C.PxpComentario as cxcComentario, ifnull(C.PxpFechaEntrega, '') as FechaEntrega " +
                        "FROM PushMoneyPorPagar C " +
                        "INNER JOIN TiposTransaccionesCxc T ON ltrim(rtrim(T.ttcSigla)) = ltrim(rtrim(C.PxpSigla)) " +
                        "LEFT JOIN (Select PxpReferencia, PushMoneyPagosrowguid, RepCodigo, ifnull(pxpValor, 0.0) as RecValor, 0 as RecDescuento from PushMoneyPagosAplicacion) A " +
                        "ON A.PxpReferencia = C.PxpReferencia and a.PushMoneyPagosrowguid in (select rowguid from PushMoneyPagos r where repcodigo = A.repcodigo and pusEstatus <>0) " +
                        "WHERE ltrim(rtrim(ttcReferencia)) <> 'NC' AND UPPER(ltrim(rtrim(ttcReferencia))) <> 'PNF' AND C.CliID = " + Arguments.Values.CurrentClient.CliID.ToString() + " " +
                        "GROUP BY C.PxpDocumento, C.PxpReferencia, C.PxpSigla, PxpFecha, C.PxpMontoTotal, C.PxpBalance, ttcOrigen, PxpMontoSinItbis " +
                        "HAVING (abs(C.PxpBalance) - ROUND(COALESCE(SUM(A.RecValor + A.RecDescuento), 0), 2) >= 0.01) " +                        
                        "ORDER BY Dias DESC";

            return SqliteManager.GetInstance().Query<RecibosDocumentosTemp>(sql, new string[] { });
        }

        public ClientesCreditoData GetDatosCreditoCliente(int cliid, string monCodigo = null)
        {
            return new ClientesCreditoData()
            {
                Balance = GetBalanceTotalByCliid(cliid),
                LimiteCredito = Arguments.Values.CurrentClient.CliLimiteCredito,
                IndicadorCredito = Arguments.Values.CurrentClient.CliIndicadorCredito

            };
        }

        public double GetBalanceTotalByCliid(int Id)
        {

            var sql = "Select CAST(IFNULL(SUM((ABS(Pxp.PxpBalance) * ttc.ttcOrigen)), 0.0) as REAL) as PxpBalance from PushMoneyPorPagar Pxp "
                            + "inner join TiposTransaccionesCxc ttc on trim(UPPER(Pxp.PxpSigla)) = trim(UPPER(ttc.ttcSigla)) where CliID = ? ";

           
            List<PushMoneyPorPagar> list = SqliteManager.GetInstance().Query<PushMoneyPorPagar>(sql, new string[] { Id.ToString() });

            if (list.Count > 0)
            {
                if (list[0].PxpBalance > 0)
                {
                    return list[0].PxpBalance - GetMontoTotalRecibosPushMoney(Id);
                }
            }

            return 0;

        }

        private double GetMontoTotalRecibosPushMoney(int cliId)
        {
            var list = SqliteManager.GetInstance().Query<Recibos>("select CAST(SUM(pusMontoEfectivo + pusMontoBono) AS REAL) as RecTotal from PushMoneyPagos " +
               "where CliID = ? and pusEstatus <> 0 ", new string[] { cliId.ToString() });

            if (list.Count > 0)
            {
                return list[0].RecTotal;
            }

            return 0;

        }

        public List<PushMoneyPorPagar> GetAllPushMoneyBySecuencia(int pussecuencia)
        {
            var sql =  "select RepCodigo, '' as PxpFechaEntrega, ifnull(replace( strftime('%d-%m-%Y', SUBSTR(pusFecha,1,10)),' ','' ), '') as PxpFecha, " +
                                  "ifnull(replace(cast(julianday('now', '" + Functions.GetDiferenciaHorariaSqlite() + " hours') - julianday(replace(pusFecha, 'T', ' ')) as integer),' ', ''), '') as PxpDias, 0 as PxpDiasEntrega, " +
                                  "pusSecuencia as PxpDocumento, 'RCB' as PxpSIGLA, '' as PxpFechaVencimiento, -1 as Origen, '0' as color, CAST(pusMontoEfectivo + pusMontoBono AS MONEY) as PxpBalance, (CAST(pusMontoEfectivo + pusMontoBono AS MONEY) * ifnull(ttcOrigen, 1)), " +
                                  "(CAST(pusMontoEfectivo + pusMontoBono AS MONEY) * ifnull(ttcOrigen, 1)) as PxpMontoTotal, '' as PxpNCF, '' as PxpNCFAfectado, '' as PxpComentario, r.CliID as CliID, '' as PxpReferencia " +
                                  "from PushMoneyPagos r left join TiposTransaccionesCxc ttc on trim(ttc.ttcReferencia) = 'RCB' " +
                                  "where pusEstatus <> 0 and pusSecuencia = ? ";


            return SqliteManager.GetInstance().Query<PushMoneyPorPagar>(sql, new string[] {pussecuencia.ToString() });

        }
        public ClientesCreditoData GetDatosCreditoPushMoneyPorPagar(int pussecuencia)
        {
            int cliid = GetRecibosPushMoneyCliid(pussecuencia);
            Clientes cliente = new DS_Clientes().GetAllClientesByIdForPus(cliid);
            return new ClientesCreditoData()
            {
                Balance = GetBalanceTotalByCliid(cliid),
                LimiteCredito = cliente.CliLimiteCredito,
                IndicadorCredito = cliente.CliIndicadorCredito,
                CliNombre = cliente.CliCodigo + " - " + cliente.CliNombre,
            };
        }

        private int GetRecibosPushMoneyCliid(int pusSecuencia)
        {
            var list = SqliteManager.GetInstance().Query<Recibos>("select cliid from PushMoneyPagos " +
               "where pusSecuencia = ? ", new string[] { pusSecuencia.ToString() });

            if (list.Count > 0)
            {
                return list[0].CliID;
            }

            return 0;
        }

        public void InsertProductInTempForDetail(string cxcReferencia, int titId)
        {
            var list = SqliteManager.GetInstance().Query<ProductosTemp>("select " + titId.ToString() + " as TitID, e.ProID as ProID, p.ProCodigo as ProCodigo, " +
                "e.PxpCantidad as Cantidad, e.PxpCantidadDetalle as CantidadDetalle," +
                "case when ifnull(p.ProUnidades, 0) = 0 then 1 else p.ProUnidades end as ProUnidades, " +
                "p.ProDescripcion as Descripcion, p.ProDatos3 as ProDatos3, PxpIndicadorOferta as IndicadorOferta, PxpPrecio as Precio, PxpItbis as Itbis, " +
                "PxpDescuento as Descuento, 0 as DesPorciento, " +
                "p.UnmCodigo as UnmCodigo, p.ProIndicadorDetalle as IndicadorDetalle, ProPrecio3, ProDescripcion2, ProDescripcion3, ProDatos2, " +
                "ProDatos1, ProDescripcion1, PxpLote as Lote, e.PxpPosicion as Posicion from PushMoneyPorPagarDetalle e " +
                "inner join Productos p on p.ProID = e.ProID " +
               "where ltrim(rtrim(upper(e.PxpReferencia))) = ? ",
                new string[] { cxcReferencia.Trim().ToUpper() });

            if (list == null || list.Count == 0)
            {
                throw new Exception("Este documento no tiene detalle");
            }

            new DS_Productos().ClearTemp(titId);

            foreach (var prod in list)
            {
                prod.rowguid = Guid.NewGuid().ToString();
            }

            SqliteManager.GetInstance().InsertAll(list);
        }

        public List<Productos> GetProductosFromPushMoneyxPagarDetalle(string pxpReferencia)
        {
            return SqliteManager.GetInstance().Query<Productos>("select ProCodigo, ProDescripcion, PxpCantidad as ProCantidad, PxpPrecio as ProPrecio, PxpItbis as ProItbis, PxpDescuento as ProDescuentoMaximo, UnmCodigo   " +
                "from PushMoneyPorPagarDetalle cxc " +
                "inner join Productos p on p.ProID = cxc.ProID " +
                "where ltrim(rtrim(upper(cxc.PxpReferencia))) = ? " +
                "Order By PxpPosicion", new string[] { pxpReferencia.Trim().ToUpper() });
        }

    }
}
