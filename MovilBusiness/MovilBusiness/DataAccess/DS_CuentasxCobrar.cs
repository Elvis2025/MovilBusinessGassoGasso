using MovilBusiness.Configuration;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.model.Internal.structs;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MovilBusiness.DataAccess
{
    public class DS_CuentasxCobrar : DS_Controller
    {
        private DS_TiposTransaccionesCXC myTTC;
        private DS_UsosMultiples myUsos;

        public DS_CuentasxCobrar()
        {
            myTTC = new DS_TiposTransaccionesCXC();
            myUsos = new DS_UsosMultiples();
        }

        public List<CuentasxCobrar> GetAllCuentasByCliente(int cliid, string monCodigo = null, bool sinRcbFuturista = false)
        {
            string sql = "";

            string whereMon1 = "";
            string whereMon2 = "";

            if (!string.IsNullOrWhiteSpace(monCodigo))
            {
                whereMon1 = " and trim(upper(cxc.MonCodigo)) = trim(upper('" + monCodigo + "')) ";
                whereMon2 = " and trim(upper(r.MonCodigo)) = trim(upper('" + monCodigo + "')) ";
            }

            var queryReconciliacion = "";

            var orderBy = " ORDER BY CAST(CxcDias as int) DESC, CxcDocumento ASC ";
            if (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionEstadosCuentas() == 5)
            {
                orderBy = " ORDER BY CxcSigla, CAST(CxcDias as int) DESC, CxcDocumento ASC ";
            }
            //ORDER BY CAST(CxcDias as int) DESC, CxcDocumento ASC

            if (myParametro.GetParRecibosOrdenarDocumentoPorFechaEntrega())
            {
                orderBy = " ORDER BY CAST(CxcDiasEntrega as int) DESC, CxcDocumento ASC ";

                if (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionEstadosCuentas() == 5)
                {
                    orderBy = " ORDER BY CxcSigla, CAST(CxcDiasEntrega as int) DESC, CxcDocumento ASC ";
                }
            }

            if (myParametro.GetParReconciliacion())
            {//CxcFechaEntrega
                queryReconciliacion = " UNION ALL "
                   + "SELECT '' as CxcClasificacion, RepCodigo as Vendedor, '' as CxcFechaEntrega,  ifnull(replace( strftime('%d-%m-%Y', SUBSTR(RecFecha,1,10)),' ','' ), '') as CxcFecha,  '' as CxcColor, " +
                   "ifnull(replace(cast(julianday(DATETIME('NOW', 'localtime'), '" + Functions.GetDiferenciaHorariaSqlite() + " hours') - julianday(RecFecha) as integer),' ', ''), '') as CxcDias, " +
                   "0 as CxcDiasVencido, 0 as CxcDiasEntrega, RecSecuencia as CxcDocumento,  " +
                   "'RCN' as Sigla, '' as cxcFechaVencimiento, '0' as color, 1 as Origen, RecMontoNcr as CxcBalance, RecMontoNcr  as CxcMontoSinItbis, RecMontoNcr as CxcMontoTotal, '' as CXCNCF, '' as CXCNCFAfectado, '' as cxcReferencia2, '' as cxcComentario, CliID,'' as rowguid, RecSecuencia as CxcReferencia, '' as RecTipo, RecEstatus as RecEstatus " +
                   "FROM Reconciliaciones " +
                   "WHERE CliID = " + cliid + " " + (!string.IsNullOrWhiteSpace(monCodigo) ? " and trim(upper(MonCodigo)) = trim(upper('" + monCodigo + "')) " : "") + " AND RecEstatus <> 0 " + (myParametro.GetParRecibosPorSector() ? " and SecCodigo = '" + (Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.SecCodigo : "") + "'" : "") +
                   " UNION ALL "
                   + "SELECT '' as CxcClasificacion, RepCodigo as Vendedor, '' as CxcFechaEntrega, ifnull(replace( strftime('%d-%m-%Y', SUBSTR(RecFecha,1,10)),' ','' ), '') as CxcFecha, '' as CxcColor, " +
                   "ifnull(replace(cast(julianday(DATETIME('NOW', 'localtime'), '" + Functions.GetDiferenciaHorariaSqlite() + " hours') - julianday(RecFecha) as integer),' ', ''), '') as CxcDias, " +
                   "0 as CxcDiasVencido, 0 as CxcDiasEntrega, RecSecuencia as CxcDocumento,  " +
                   "'RCN' as Sigla, '' as cxcFechaVencimiento, '0' as color, 1 as Origen, RecMontoNcr as CxcBalance, RecMontoNcr  as CxcMontoSinItbis, RecMontoNcr as CxcMontoTotal, '' as CXCNCF, '' as CXCNCFAfectado, '' as cxcReferencia2, '' as cxcComentario, CliID,'' as rowguid, RecSecuencia as CxcReferencia, '' as RecTipo, RecEstatus as RecEstatus " +
                   "FROM ReconciliacionesConfirmados " +
                   "WHERE CliID = " + cliid + " " + (!string.IsNullOrWhiteSpace(monCodigo) ? " and trim(upper(MonCodigo)) = trim(upper('" + monCodigo + "')) " : "") + " AND RecEstatus <> 0 " + (myParametro.GetParRecibosPorSector() ? " and SecCodigo = '" + (Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.SecCodigo : "") + "'" : "");
            }

            if (myParametro.GetParRecibosPorSector())
            {
                string crtlAreaCredito = " and AreaCtrlCredit = '" + Arguments.Values.CurrentSector.AreaCtrlCredit + "'";

                //para en el area de control de credit hacerle un subString
                if (myParametro.GetParAreaCrtlCreditoClienteSubString() && Arguments.Values.CurrentSector.AreaCtrlCredit != null && Arguments.Values.CurrentSector.AreaCtrlCredit.Length > 1)
                {
                    crtlAreaCredito = " and SUBSTR(AreaCtrlCredit,1,2) = '" + Arguments.Values.CurrentSector.AreaCtrlCredit.Substring(0, 2) + "' ";
                }
                sql = "select result.* from (select CxcClasificacion, RepCodigo, ifnull(replace( strftime('%d-%m-%Y', SUBSTR(CxcFechaEntrega,1,10)),' ','' ), '') as CxcFechaEntrega, ifnull(replace( strftime('%d-%m-%Y', SUBSTR(CxcFecha,1,10)),' ','' ), '') as CxcFecha, ifnull(CxcColor, '') as CxcColor, " +
                   "julianday(Cast(strftime('%Y-%m-%d',DATETIME('NOW', 'localtime'),'" + Functions.GetDiferenciaHorariaSqlite() + " hours') as Varchar)) - julianday(SUBSTR(ifnull(CxcFecha, ''),1,10)) as CxcDias, " +
                   "julianday(Cast(strftime('%Y-%m-%d',DATETIME('NOW', 'localtime'),'" + Functions.GetDiferenciaHorariaSqlite() + " hours') as Varchar)) - julianday(SUBSTR(ifnull(cxcFechaVencimiento, ''),1,10)) as CxcDiasVencido, " +
                   "julianday(Cast(strftime('%Y-%m-%d',DATETIME('NOW', 'localtime'),'" + Functions.GetDiferenciaHorariaSqlite() + " hours') as Varchar)) - julianday(SUBSTR(ifnull(CxcFechaEntrega, ''),1,10)) as CxcDiasEntrega, " +
                   "CxcDocumento, ifnull(ltrim(rtrim(upper(CxcSIGLA))), '') as CxcSIGLA, replace(ifnull(strftime('%d-%m-%Y', SUBSTR(cxcFechaVencimiento,1,10)),'0'),'','' ) as cxcFechaVencimiento, IFNULL(ttc.ttcOrigen, 1) as Origen, " +
                   "case when CxcFechaVencimiento < datetime(DATETIME('NOW', 'localtime'), '" + Functions.GetDiferenciaHorariaSqlite() + " hours') and cxcsigla = '" + myTTC.GetSiglaByReferencia("FAT") + "' then '" + myParametro.GetParRecFacturasVencidasColor() + "' else '#ffffff' end as color, " +
                   "CxcBalance, (abs(CxcMontoSinItbis) * ifnull(ttcOrigen, 1)) as CxcMontoSinItbis, (abs(CxcMontoTotal) * ifnull(ttcOrigen, 1)) as CxcMontoTotal, cxc.CXCNCF as CXCNCF, cxc.CXCNCFAfectado as CXCNCFAfectado, cxc.cxcReferencia2 as cxcReferencia2, cxc.cxcComentario as cxcComentario, cxc.CliID as CliID,'' as rowguid, CxcReferencia, '' as RecTipo, -1 as RecEstatus  " +
                   "from CuentasxCobrar cxc left join TiposTransaccionesCxc ttc on trim(ttc.ttcReferencia) = CxcSigla where CliID = ? " + whereMon1 + " " + crtlAreaCredito + " " +
                   "UNION ALL " +
                   "select '' as CxcClasificacion, RepCodigo, '' as CxcFechaEntrega, ifnull(replace( strftime('%d-%m-%Y', SUBSTR(RecFecha,1,10)),' ','' ), '') as CxcFecha, '' as CxcColor, " +
                   "ifnull(replace(cast(julianday(DATETIME('NOW', 'localtime'), '" + Functions.GetDiferenciaHorariaSqlite() + " hours') - julianday(replace(RecFecha, 'T', ' ')) as integer),' ', ''), '') as CxcDias, " +
                   "0 as CxcDiasVencido, 0 as CxcDiasEntrega, " +
                   "RecSecuencia as CxcDocumento, 'RCB' as CxcSIGLA, '' as cxcFechaVencimiento, -1 as Origen, '0' as color, CAST(RecMontoEfectivo + RecMontoCheque + RecMontoChequef + RecMontoTransferencia + RecMontoTarjeta + RecMontoDescuento + IFNULL(RecTotalDescuentoDesmonte,0) AS MONEY) as CxcBalance, (CAST(RecMontoEfectivo + RecMontoCheque + RecMontoChequef + RecMontoTransferencia + RecMontoTarjeta + RecMontoDescuento + IFNULL(RecTotalDescuentoDesmonte,0) AS MONEY) * ifnull(ttcOrigen, 1)), " +
                   "(CAST(RecMontoEfectivo + RecMontoCheque + RecMontoChequef + RecMontoTransferencia + RecMontoTarjeta + RecMontoDescuento + IFNULL(RecTotalDescuentoDesmonte,0) AS MONEY) * ifnull(ttcOrigen, 1)) as CxcMontoTotal, '' as CXCNCF, '' as CXCNCFAfectado, '' as cxcReferencia2, '' as cxcComentario, r.CliID as CliID,r.rowguid, '' as CxcReferencia, r.RecTipo, r.RecEstatus  " +
                   "from Recibos r left join TiposTransaccionesCxc ttc on trim(ttc.ttcReferencia) = 'RCB' " +
                   "where " +
                   (sinRcbFuturista ? " not exists (select 1 from RecibosFormaPago rfp where rfp.RecSecuencia=r.RecSecuencia and rfp.RepCodigo=r.RepCodigo and rfp.RecTipo=r.Rectipo and rfp.ForId=2 and rfp.RefIndicadorDiferido=1) and " : "") + " " +
                   "CliID = ? " + whereMon2 + " and RecEstatus <> 0 " + crtlAreaCredito + " " + queryReconciliacion + ") as result " + orderBy;
            }
            else
            {
                sql = "select result.* from (select CxcClasificacion, RepCodigo, ifnull(replace( strftime('%d-%m-%Y', SUBSTR(CxcFechaEntrega,1,10)),' ','' ), '') as CxcFechaEntrega, ifnull(replace( strftime('%d-%m-%Y', SUBSTR(CxcFecha,1,10)),' ','' ), '') as CxcFecha, ifnull(CxcColor, '') as CxcColor, " +
                                  "julianday(Cast(strftime('%Y-%m-%d',DATETIME('NOW', 'localtime'),'" + Functions.GetDiferenciaHorariaSqlite() + " hours') as Varchar)) - julianday(SUBSTR(ifnull(CxcFecha, CxcFecha),1,10)) as CxcDias," +
                                  "julianday(Cast(strftime('%Y-%m-%d',DATETIME('NOW', 'localtime'),'" + Functions.GetDiferenciaHorariaSqlite() + " hours') as Varchar)) - julianday(SUBSTR(ifnull(cxcFechaVencimiento, CxcFecha),1,10)) as CxcDiasVencido, " +
                                  "julianday(Cast(strftime('%Y-%m-%d',DATETIME('NOW', 'localtime'),'" + Functions.GetDiferenciaHorariaSqlite() + " hours') as Varchar)) - julianday(SUBSTR(ifnull(CxcFechaEntrega, ''),1,10)) as CxcDiasEntrega, " +
                                  "CxcDocumento, ifnull(ltrim(rtrim(upper(CxcSIGLA))), '') as CxcSIGLA, replace(ifnull(strftime('%d-%m-%Y', SUBSTR(cxcFechaVencimiento,1,10)),'0'),'','' ) as cxcFechaVencimiento, IFNULL(ttc.ttcOrigen, 1) as Origen, " +
                                  "case when CxcFechaVencimiento < datetime(DATETIME('NOW', 'localtime'), '" + Functions.GetDiferenciaHorariaSqlite() + " hours') and cxcsigla = '" + myTTC.GetSiglaByReferencia("FAT") + "' then '" + myParametro.GetParRecFacturasVencidasColor() + "' else '#ffffff' end as color, " +
                                  "CxcBalance, (abs(CxcMontoSinItbis) * ifnull(ttcOrigen, 1)) as CxcMontoSinItbis, (abs(CxcMontoTotal) * ifnull(ttcOrigen, 1)) as CxcMontoTotal, cxc.CXCNCF as CXCNCF, cxc.CXCNCFAfectado as CXCNCFAfectado, cxc.cxcReferencia2 as cxcReferencia2, cxc.cxcComentario as cxcComentario, cxc.CliID as CliID,'' as rowguid , CxcReferencia, '' as RecTipo, -1 as RecEstatus " +
                                  "from CuentasxCobrar cxc left join TiposTransaccionesCxc ttc on trim(ttc.ttcReferencia) = CxcSigla where CliID = ? " + whereMon1 + " " +
                                  "UNION ALL " +
                                  "select '' as CxcClasificacion, RepCodigo, '' as CxcFechaEntrega, ifnull(replace( strftime('%d-%m-%Y', SUBSTR(RecFecha,1,10)),' ','' ), '') as CxcFecha,  '' as CxcColor, " +
                                  "ifnull(replace(cast(julianday(DATETIME('NOW', 'localtime'), '" + Functions.GetDiferenciaHorariaSqlite() + " hours') - julianday(replace(RecFecha, 'T', ' ')) as integer),' ', ''), '') as CxcDias, " +
                                  "0 as CxcDiasVencido, 0 as CxcDiasEntrega, " +
                                  "RecSecuencia as CxcDocumento, 'RCB' as CxcSIGLA, '' as cxcFechaVencimiento, -1 as Origen, '0' as color, CAST(RecMontoEfectivo + RecMontoCheque + RecMontoChequef + RecMontoTransferencia + RecMontoTarjeta + RecMontoDescuento + IFNULL(RecTotalDescuentoDesmonte,0) AS MONEY) as CxcBalance, (CAST(RecMontoEfectivo + RecMontoCheque + RecMontoChequef + RecMontoTransferencia + RecMontoTarjeta + RecMontoDescuento + IFNULL(RecTotalDescuentoDesmonte,0) AS MONEY) * ifnull(ttcOrigen, 1)), " +
                                  "(CAST(RecMontoEfectivo + RecMontoCheque + RecMontoChequef + RecMontoTransferencia + RecMontoTarjeta + RecMontoDescuento + IFNULL(RecTotalDescuentoDesmonte,0) AS MONEY) * ifnull(ttcOrigen, 1)) as CxcMontoTotal, '' as CXCNCF, '' as CXCNCFAfectado, '' as cxcReferencia2, '' as cxcComentario, r.CliID as CliID, r.rowguid , '' as CxcReferencia, r.RecTipo, r.RecEstatus " +
                                  "from Recibos r left join TiposTransaccionesCxc ttc on trim(ttc.ttcReferencia) = 'RCB' " +
                                  "where " +
                                  (sinRcbFuturista ? " not exists (select 1 from RecibosFormaPago rfp where rfp.RecSecuencia=r.RecSecuencia and rfp.RepCodigo=r.RepCodigo and rfp.RecTipo=r.Rectipo and rfp.ForId=2 and rfp.RefIndicadorDiferido=1) and " : "") + " " +
                                  "CliID = ? " + whereMon2 + " and RecEstatus <> 0 " + queryReconciliacion + ") as result " + orderBy;

            }

            //nuevo parametro para ordenar por esto
            ///cxcFechaEntrega asc, cxcDocumento asc

            return SqliteManager.GetInstance().Query<CuentasxCobrar>(sql, new string[] { cliid.ToString(), cliid.ToString() });
        }

        public double GetAllRecibosAplicados(string Referencia)
        {
            var MontoAllRecibos = SqliteManager.GetInstance().Query<RecibosAplicacion>("SELECT SUM(ra.RecValor) as RecValor FROM RecibosAplicacion ra INNER JOIN Recibos r ON ra.RecSecuencia = r.RecSecuencia WHERE ra.CxcReferencia = '" + Referencia + "' AND r.RecEstatus <> 0 AND r.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' AND ra.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "'", new string[] { });
            double MontoAllRecibosAplicados = MontoAllRecibos[0].RecValor;

            return MontoAllRecibosAplicados;
        }

        public List<CuentasxCobrar> GetAllCuentasPendientesByCliente(int cliid, string monCodigo = null)
        {
            string sql = "";

            var whereCxc = "";
            var whereRec = "";

            if (!string.IsNullOrWhiteSpace(monCodigo))
            {
                whereCxc = " and trim(upper(C.MonCodigo)) = trim(upper('" + monCodigo + "')) ";
                whereRec = " and trim(upper(r.MonCodigo)) = trim(upper('" + monCodigo + "')) ";
            }

            if (myParametro.GetParRecibosPorSector())
            {
                sql = "select c.RepCodigo, ifnull(replace( strftime('%d-%m-%Y', SUBSTR(CxcFecha,1,10)),' ','' ), '') as CxcFecha, " +
                   "julianday(Cast(strftime('%Y-%m-%d',DATETIME('NOW', 'localtime'),'" + Functions.GetDiferenciaHorariaSqlite() + " hours') as Varchar)) - julianday(SUBSTR(ifnull(CxcFecha, CxcFecha),1,10)) as CxcDias, " +
                   "julianday(Cast(strftime('%Y-%m-%d',DATETIME('NOW', 'localtime'),'" + Functions.GetDiferenciaHorariaSqlite() + " hours') as Varchar)) - julianday(SUBSTR(ifnull(CxcFecha, CxcFechaVencimiento),1,10)) as CxcDiasVencido, " +
                   "CxcDocumento, ifnull(ltrim(rtrim(upper(CxcSIGLA))), '') as CxcSIGLA, replace(ifnull(strftime('%d-%m-%Y', SUBSTR(cxcFechaVencimiento,1,10)),'0'),'','' ) as cxcFechaVencimiento, IFNULL(ttc.ttcOrigen, 1) as Origen, " +
                   "case when CxcFechaVencimiento < datetime(DATETIME('NOW', 'localtime'), '" + Functions.GetDiferenciaHorariaSqlite() + " hours') and cxcsigla = '" + myTTC.GetSiglaByReferencia("FAT") + "' then '" + myParametro.GetParRecFacturasVencidasColor() + "' else '#ffffff' end as color, " +
                   "CxcBalance, (abs(CxcMontoSinItbis) * ifnull(ttcOrigen, 1)) as CxcMontoSinItbis, (abs(CxcMontoTotal) * ifnull(ttcOrigen, 1)) as CxcMontoTotal, c.CXCNCF as CXCNCF, c.CXCNCFAfectado as CXCNCFAfectado, c.cxcComentario as cxcComentario, c.CliID as CliID, c.CxcReferencia " +
                   "from CuentasxCobrar C left join TiposTransaccionesCxc ttc on trim(ttc.ttcReferencia) = CxcSigla " +
                   "LEFT JOIN(Select IFNULL(RecDescuentoDesmonte,0) as RecDescuentoDesmonte, CxcReferencia, RecSecuencia, RepCodigo, RecValor, RecDescuento from RecibosAplicacion UNION ALL Select 0 as RecDescuentoDesmonte, CxcReferencia, RecSecuencia, RepCodigo, RefValor as RecValor, 0 as RecDescuento from RecibosFormaPago) A " +
                   "ON A.CxcReferencia = C.CXCReferencia and a.recsecuencia in (select r.recSecuencia from recibos r where repcodigo = A.repcodigo " + whereRec + " and RecEstatus <>0) " + "WHERE CliID = ? " + whereCxc + " And C.AreaCtrlCredit = '" + (Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.AreaCtrlCredit : "") + "' " +
                   "GROUP BY C.CxcDocumento, C.CxcReferencia, C.CxcSigla, CxcFecha, C.CxcMontoTotal, C.CxcBalance, ttcOrigen, cxcMontoSinItbis " +
                    "HAVING (abs(C.CxcBalance) - ROUND(COALESCE(SUM(A.RecValor + A.RecDescuento + IFNULL(A.RecDescuentoDesmonte,0) ), 0), 2) >= 0.01)  ";
            }
            else
            {
                sql = "select C.RepCodigo as RepCodigo, ifnull(replace( strftime('%d-%m-%Y', SUBSTR(CxcFecha,1,10)),' ','' ), '') as CxcFecha, " +
                                  //  "julianday(Cast(strftime('%Y-%m-%d','now','" + Functions.GetDiferenciaHorariaSqlite() + " hours') as Varchar)) - julianday(SUBSTR(ifnull(CxcFecha, CxcFecha),1,10)) as CxcDias, " +
                                  "cast(julianday(Cast(strftime('%Y-%m-%d', C.cxcFechaVencimiento) as Varchar)) - julianday('now', '-4 hours') as integer) as CxcDias, " +
                                  "cast(julianday(Cast(strftime('%Y-%m-%d', C.cxcFechaVencimiento) as Varchar)) - julianday('now', '-4 hours') as integer) as CxcDiasVencido, " +
                                  "CxcDocumento, ifnull(ltrim(rtrim(upper(CxcSIGLA))), '') as CxcSIGLA, replace(ifnull(strftime('%d-%m-%Y', SUBSTR(cxcFechaVencimiento,1,10)),'0'),'','' ) as cxcFechaVencimiento, IFNULL(ttc.ttcOrigen, 1) as Origen, " +
                                  "case when CxcFechaVencimiento < datetime(DATETIME('NOW', 'localtime'), '" + Functions.GetDiferenciaHorariaSqlite() + " hours') and cxcsigla = '" + myTTC.GetSiglaByReferencia("FAT") + "' then '" + myParametro.GetParRecFacturasVencidasColor() + "' else '#ffffff' end as color, " +
                                  "CxcBalance, (abs(CxcMontoSinItbis) * ifnull(ttcOrigen, 1)) as CxcMontoSinItbis, (abs(CxcMontoTotal) * ifnull(ttcOrigen, 1)) as CxcMontoTotal, C.CXCNCF as CXCNCF, C.CXCNCFAfectado as CXCNCFAfectado, C.cxcComentario as cxcComentario, C.CliID as CliID, C.CxcReferencia as CxcReferencia, C.CxcBalance as CxcBalance  " +
                                  "from CuentasxCobrar C left join TiposTransaccionesCxc ttc on trim(ttc.ttcReferencia) = CxcSigla " +
                                  "LEFT JOIN(Select IFNULL(RecDescuentoDesmonte,0) as RecDescuentoDesmonte, CxcReferencia, RecSecuencia, RepCodigo, RecValor, RecDescuento from RecibosAplicacion UNION ALL Select 0 as RecDescuentoDesmonte, CxcReferencia, RecSecuencia, RepCodigo, RefValor as RecValor, 0 as RecDescuento from RecibosFormaPago) A " +
                                  "ON A.CxcReferencia = C.CXCReferencia and a.recsecuencia in (select r.recSecuencia from recibos r where repcodigo = A.repcodigo " + whereRec + " and RecEstatus <>0) " + "WHERE CliID = ? " + whereCxc + " " +
                                  "GROUP BY C.CxcDocumento, C.CxcReferencia, C.CxcSigla, CxcFecha, C.CxcMontoTotal, C.CxcBalance, ttcOrigen, cxcMontoSinItbis " +
                                  "HAVING (abs(C.CxcBalance) - ROUND(COALESCE(SUM(A.RecValor + A.RecDescuento + IFNULL(A.RecDescuentoDesmonte,0) ), 0), 2) >= 0.01)  ";
            }

            return SqliteManager.GetInstance().Query<CuentasxCobrar>(sql, new string[] { cliid.ToString(), cliid.ToString() });
        }


        public double GetBalanceTotalByCliid(int Id, string monCodigo = null, bool WithChD = true)
        {

            string whereMonCodigo = "";

            if (!string.IsNullOrWhiteSpace(monCodigo))
            {
                whereMonCodigo = " and trim(upper(cxc.MonCodigo)) = trim(upper('" + monCodigo + "')) ";
            }

            var sql = "Select CAST(IFNULL(SUM((ABS(cxc.CxcBalance) * ttc.ttcOrigen)), 0.0) as REAL) as CxcBalance from cuentasxcobrar cxc "
                            + "inner join TiposTransaccionesCxc ttc on trim(UPPER(cxc.CxcSigla)) = trim(UPPER(ttc.ttcSigla)) where CliID = ? " + whereMonCodigo;

            if (myParametro.GetParRecibosPorSector())
            {
                if (myParametro.GetParAreaCrtlCreditoClienteSubString())
                {
                    sql = "Select round(SUM((ABS(cxc.CxcBalance) * ttc.ttcOrigen)),2) as CxcBalance from cuentasxcobrar cxc " +
                        "inner join TiposTransaccionesCxc ttc on trim(UPPER(cxc.CxcSigla)) = trim(UPPER(ttc.ttcSigla)) " +
                        "where CliID = ? " + whereMonCodigo + " and SUBSTR(cxc.AreaCtrlCredit, 1, 2) = '" + (Arguments.Values.CurrentSector != null && Arguments.Values.CurrentSector.AreaCtrlCredit != null ? Arguments.Values.CurrentSector.AreaCtrlCredit.Substring(0, 2) : "") + "'";
                }
                else
                {
                    sql = "select ifnull(round(SUM((ABS(cxc.CxcBalance) * ttc.ttcOrigen)),2),0) as CxcBalance from cuentasxcobrar cxc " +
                        "inner join TiposTransaccionesCxc ttc on trim(UPPER(cxc.CxcSigla)) = trim(UPPER(ttc.ttcSigla)) " +
                        "where CliID = ? " + whereMonCodigo + " and AreaCtrlCredit = '" + (Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.AreaCtrlCredit : "") + "'";
                }
            }
            else
            {
                if (myParametro.GetParRecibosOtrosDocumentosBalance())
                {
                    sql = "Select round(SUM((ABS(cxc.CxcBalance) * ttc.ttcOrigen)),2) as CxcBalance from OtrosDocumentosCobros cxc " +
                        "inner join TiposTransaccionesCxc ttc on trim(UPPER(cxc.CxcSigla)) = trim(UPPER(ttc.ttcSigla)) where CliID = ? " + whereMonCodigo + " ";
                }
            }

            CuentasxCobrar cxcResult = SqliteManager.GetInstance().Query<CuentasxCobrar>(sql, new string[] { Id.ToString() }).FirstOrDefault();

            if (!(cxcResult is null))
            {
                double balance = cxcResult.CxcBalance;

                if (balance != 0)
                {
                    return WithChD
                        ? balance - new DS_Recibos().GetMontoTotalRecibosByCliId(Id, monCodigo)
                        : balance - new DS_Recibos().GetMontoTotalRecibosByCliIdSinChequeDiferido(Id, monCodigo);

                }
            }

            return 0;

        }

        public double GetBalanceTotalByCliidyfecha(int Id, string monCodigo = null)
        {

            string whereMonCodigo = "";

            if (!string.IsNullOrWhiteSpace(monCodigo))
            {
                whereMonCodigo = " and trim(upper(cxc.MonCodigo)) = trim(upper('" + monCodigo + "')) ";
            }

            var sql = "Select CAST(IFNULL(SUM((ABS(cxc.CxcBalance) * ttc.ttcOrigen)), 0.0) as REAL) as CxcBalance from cuentasxcobrar cxc "
                            + "inner join TiposTransaccionesCxc ttc on trim(UPPER(cxc.CxcSigla)) = trim(UPPER(ttc.ttcSigla)) where CliID = ? " + whereMonCodigo;

            if (myParametro.GetParRecibosPorSector())
            {
                if (myParametro.GetParAreaCrtlCreditoClienteSubString())
                {
                    sql = "Select round(SUM((ABS(cxc.CxcBalance) * ttc.ttcOrigen)),2) as CxcBalance from cuentasxcobrar cxc " +
                        "inner join TiposTransaccionesCxc ttc on trim(UPPER(cxc.CxcSigla)) = trim(UPPER(ttc.ttcSigla)) " +
                        "where CliID = ? " + whereMonCodigo + " and SUBSTR(cxc.AreaCtrlCredit, 1, 2) = '" + (Arguments.Values.CurrentSector != null && Arguments.Values.CurrentSector.AreaCtrlCredit != null ? Arguments.Values.CurrentSector.AreaCtrlCredit.Substring(0, 2) : "") + "'";
                }
                else
                {
                    sql = "select ifnull(round(SUM((ABS(cxc.CxcBalance) * ttc.ttcOrigen)),2),0) as CxcBalance from cuentasxcobrar cxc " +
                        "inner join TiposTransaccionesCxc ttc on trim(UPPER(cxc.CxcSigla)) = trim(UPPER(ttc.ttcSigla)) " +
                        "where CliID = ? " + whereMonCodigo + " and AreaCtrlCredit = '" + (Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.AreaCtrlCredit : "") + "'";
                }
            }
            else
            {
                if (myParametro.GetParRecibosOtrosDocumentosBalance())
                {
                    sql = "Select round(SUM((ABS(cxc.CxcBalance) * ttc.ttcOrigen)),2) as CxcBalance from OtrosDocumentosCobros cxc " +
                        "inner join TiposTransaccionesCxc ttc on trim(UPPER(cxc.CxcSigla)) = trim(UPPER(ttc.ttcSigla)) where CliID = ? " + whereMonCodigo + " ";
                }
            }

            List<CuentasxCobrar> list = SqliteManager.GetInstance().Query<CuentasxCobrar>(sql, new string[] { Id.ToString() });

            if (list.Count > 0)
            {
                if (list[0].CxcBalance > 0)
                {
                    double x = list[0].CxcBalance;
                    double y = new DS_Recibos().GetMontoTotalRecibosByCliIdyFecha(Id, monCodigo, fecha: DateTime.Now.ToString("yyyy-MM-dd"));

                    double result = x - y;

                    return Math.Abs(result); //list[0].CxcBalance - (new DS_Recibos().GetMontoTotalRecibosByCliIdyFecha(Id, monCodigo, fecha: DateTime.Now.ToString("yyyy-MM-dd")));
                }
            }

            return 0;

        }

        public void InsertPendingDocumentsInTemp(string monCodigo)
        {
            new DS_Recibos().ClearTemps();

            string sql;
            string porcionMultiMoneda = "";
            if (monCodigo != null && !myParametro.GetParRecibosusarMonedaUnica())
            {
                porcionMultiMoneda = " AND trim(upper(C.MonCodigo)) = trim(upper('" + monCodigo + "')) ";
            }

            //Esto es para filtrar por sectores
            if (myParametro.GetParRecibosPorSector() && Arguments.Values.CurrentSector != null)
            {
                string crtlAreaCredito = " and AreaCtrlCredit = '" + Arguments.Values.CurrentSector.AreaCtrlCredit + "'";

                //para en el area de control de credit hacerle un subString
                if (myParametro.GetParAreaCrtlCreditoClienteSubString() && Arguments.Values.CurrentSector.AreaCtrlCredit != null && Arguments.Values.CurrentSector.AreaCtrlCredit.Length > 1)
                {
                    crtlAreaCredito = " and SUBSTR(AreaCtrlCredit,1,2) = '" + Arguments.Values.CurrentSector.AreaCtrlCredit.Substring(0, 2) + "' ";
                }

                //sql = "INSERT INTO RecibosDocumentosTemp(FechaSinFormatear, Fecha, Documento, Referencia, Sigla, Aplicado, Descuento, MontoTotal, Balance, Pendiente, Estado, " +
                //        "Credito, FechaIngles, Origen, MontoSinItbis, DescPorciento, AutSecuencia, FechaDescuento, Dias, DescuentoFactura, Clasificacion, " +
                //        "FechaVencimiento, Retencion, CxcNotaCredito, CalcularDesc, CXCNCF, cxcComentario) " +
                //        "SELECT CxcFecha, CAST(replace(strftime('%d-%m-%Y', SUBSTR(CxcFecha,1,10)),' ','') as varchar) as Fecha, C.CxcDocumento, C.CxcReferencia, " +
                //        "C.CxcSigla, 0 AS Aplicado, 0 as Descuento, C.CxcMontoTotal, (C.CxcBalance - COALESCE(SUM(A.RecValor + A.RecDescuento),0)) AS CxcBalance, " +
                //        "(C.CxcBalance - COALESCE(SUM(A.RecValor + A.RecDescuento ),0)) AS Pendiente, 'Pendiente' as Estado, 0  as Credito, " +
                //        "CAST(replace(strftime('%m-%d-%Y', SUBSTR(CxcFecha,1,10)),' ','' ) as varchar) as CxcFechaIngles, ttcOrigen, cxcMontoSinItbis, " +
                //        "0 as DescPorciento, 0 as AutID, strftime('%d-%m-%Y','now','" + Functions.GetDiferenciaHorariaSqlite() + " hours') as FechaDescuento, julianday(Cast(strftime('%Y-%m-%d','now','" + Functions.GetDiferenciaHorariaSqlite() + " hours') as Varchar)) - julianday(SUBSTR(ifnull(CxcFechaEntrega, CxcFecha),1,10)) as Dias, 0 DescuentoFactura, " +
                //        "IFNULL(CxcClasificacion, '') AS 'Clasificacion', IFNULL(date(CxcFechaVencimiento), '') AS 'FechaVencimiento', ifnull(CxcRetencion, 0) as Retencion, ifnull(C.CxcNotaCredito, 0) as CxcNotaCredito, 1 as CalcularDesc, C.CXCNCF as CXCNCF, C.cxcComentario as cxcComentario " +
                //        "FROM CuentasxCobrar C " +
                //        "INNER JOIN TiposTransaccionesCXC T ON ltrim(rtrim(T.ttcSigla)) = ltrim(rtrim(C.CxcSigla)) " +
                //        "LEFT JOIN (Select CxcReferencia, RecSecuencia, RepCodigo, RecValor, RecDescuento from RecibosAplicacion UNION ALL Select CxcReferencia, RecSecuencia, RepCodigo, RefValor as RecValor, 0 as RecDescuento from RecibosFormaPago) A " +
                //        "ON A.CxcReferencia = C.CXCReferencia and a.recsecuencia in (select r.recSecuencia from recibos r where repcodigo = A.repcodigo and RecEstatus <>0) " +
                //        "WHERE ltrim(rtrim(ttcReferencia)) <> 'NC' AND UPPER(ltrim(rtrim(ttcReferencia))) <> 'PNF' AND C.ConID <> "+ DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado() + " AND C.CliID = " + Arguments.Values.CurrentClient.CliID.ToString() + " " + crtlAreaCredito + " " + porcionMultiMoneda + " " +
                //        "GROUP BY C.CxcDocumento, C.CxcReferencia, C.CxcSigla, CxcFecha, C.CxcMontoTotal, C.CxcBalance, ttcOrigen, cxcMontoSinItbis " +
                //        "HAVING (C.CxcBalance - COALESCE(SUM(A.RecValor + A.RecDescuento), 0)) > 0 " +
                //        "UNION ALL " +
                //        "SELECT CxcFecha, CAST(replace(strftime('%d-%m-%Y', SUBSTR(CxcFecha,1,10)),' ','' ) as varchar) as Fecha, C.CxcDocumento, C.CxcReferencia, " +
                //        "C.CxcSigla, 0 AS Aplicado, 0 as Descuento, abs(C.CxcMontoTotal), (abs(C.CxcBalance) - COALESCE(SUM(A.RefValor),0)) AS CxcBalance, " +
                //        "(abs(C.CxcBalance) - COALESCE(SUM(A.RefValor),0)) AS Pendiente, 'Pendiente' as Estado, 0  as Credito, " +
                //        "CAST(replace(strftime('%m-%d-%Y', SUBSTR(CxcFecha,1,10)),' ','' ) as varchar) as CxcFechaIngles, ttcOrigen, abs(cxcMontoSinItbis), " +
                //        "0 DescPorciento, 0 AutID, -1 as FechaDescuento, -1 as Dias,0 DescuentoFactura, '' AS 'Clasificacion', IFNULL(date(CxcFechaVencimiento), '') AS 'FechaVencimiento', '0' as Retencion, ifnull(C.CxcNotaCredito, 0) as CxcNotaCredito, 1 as CalcularDesc, C.CXCNCF as CXCNCF, C.cxcComentario as cxcComentario " +
                //        "FROM CuentasxCobrar C " +
                //        "INNER JOIN TiposTransaccionesCXC T ON  ltrim(rtrim(T.ttcSigla)) = ltrim(rtrim(C.CxcSigla)) " +
                //        "LEFT JOIN RecibosFormaPago A ON A.RefNumeroAutorizacion = C.CXCReferencia and a.recsecuencia in " +
                //        "(select r.recSecuencia from recibos r where repcodigo = A.repcodigo and RecEstatus <>0) " +
                //        "WHERE ltrim(rtrim(ttcReferencia)) = 'NC' AND C.ConID <> " + DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado() + " AND C.CliID = " + Arguments.Values.CurrentClient.CliID.ToString() + " " + crtlAreaCredito + " " + porcionMultiMoneda + " " +
                //        "GROUP BY C.CxcDocumento, C.CxcReferencia, C.CxcSigla, CxcFecha, C.CxcMontoTotal, C.CxcBalance, ttcOrigen, cxcMontoSinItbis " +
                //        "HAVING (abs(C.CxcBalance) - COALESCE(SUM(A.RefValor), 0)) <> 0 " +
                //        "ORDER BY Dias DESC";
                if (myParametro.GetParRecibosusarMonedaUnica())
                {
                    sql = "INSERT INTO RecibosDocumentosTemp(FechaSinFormatear, Fecha, Documento, Referencia, CxcColor, Sigla, Aplicado, Descuento, MontoTotal, Balance, Pendiente, Estado, " +
                           "Credito, FechaIngles, Origen, MontoSinItbis, DescPorciento, AutSecuencia, FechaDescuento, Dias, DescuentoFactura, Clasificacion, " +
                           "FechaVencimiento, Retencion, CxcNotaCredito, CalcularDesc, CXCNCF, cxcComentario, FechaEntrega, RepCodigo, MonCodigo, AplicaDescuento, CalcularDesmonte, Desmonte) " +
                           "SELECT CxcFecha, CAST(replace(strftime('%d-%m-%Y', SUBSTR(CxcFecha,1,10)),' ','') as varchar) as Fecha, C.CxcDocumento, C.CxcReferencia, CxcColor, " +
                           "C.CxcSigla, 0 AS Aplicado, 0 as Descuento, C.CxcMontoTotal, (C.CxcBalance - COALESCE(SUM(A.RecValor + A.RecDescuento + IFNULL(A.RecDescuentoDesmonte,0) ),0)) AS CxcBalance, " +
                           "(C.CxcBalance - COALESCE(SUM(A.RecValor + A.RecDescuento + IFNULL(A.RecDescuentoDesmonte,0)  ),0)) AS Pendiente, 'Pendiente' as Estado, 0  as Credito, " +
                           "CAST(replace(strftime('%m-%d-%Y', SUBSTR(CxcFecha,1,10)),' ','' ) as varchar) as CxcFechaIngles, ttcOrigen, cxcMontoSinItbis, " +
                           "0 as DescPorciento, 0 as AutID, strftime('%d-%m-%Y',DATETIME('NOW', 'localtime'),'" + Functions.GetDiferenciaHorariaSqlite() + " hours') as FechaDescuento, julianday(Cast(strftime('%Y-%m-%d',DATETIME('NOW', 'localtime'),'" + Functions.GetDiferenciaHorariaSqlite() + " hours') as Varchar)) - julianday(SUBSTR(ifnull(CxcFechaEntrega, CxcFecha),1,10)) as Dias, 0 DescuentoFactura, " +
                           "IFNULL(CxcClasificacion, '') AS 'Clasificacion', IFNULL(date(CxcFechaVencimiento), '') AS 'FechaVencimiento', ifnull(CxcRetencion, 0) as Retencion, ifnull(C.CxcNotaCredito, 0) as CxcNotaCredito, 1 as CalcularDesc, C.CXCNCF as CXCNCF, C.cxcComentario as cxcComentario, ifnull(C.CxcFechaEntrega, ''), C.RepCodigo , C.MonCodigo, CASE T.ttcaplicadescuento WHEN 1 THEN 'Si' ELSE 'No' END as AplicaDescuento, 1 as CalcularDesmonte, 0 as cxcDesmonte " +
                           "FROM CuentasxCobrar C " +
                           "INNER JOIN TiposTransaccionesCXC T ON trim(T.ttcSigla) = trim(C.CxcSigla) " +
                           "LEFT JOIN (Select IFNULL(ra.RecDescuentoDesmonte,0) as RecDescuentoDesmonte, ra.CxcReferencia, ra.RecSecuencia, ra.RepCodigo, case  when ra.monCodigo = 'USD' and rf.monCodigo ='USD' THEN ((ra.RecValor * ifnull(rf.RecTasa,1)) / Ifnull(rf.RecTasa,1)) when ra.monCodigo = 'USD' and rf.monCodigo != 'USD' THEN ((ra.RecValor * ifnull(rf.RecTasa,1)) / Ifnull(ra.RecTasa,1)) ELSE ((ra.RecValor * ifnull(rf.RecTasa,1))) END as RecValor, case ra.monCodigo when 'USD' THEN ((ra.RecDescuento * ifnull(rf.RecTasa,1)) / Ifnull(rf.RecTasa,1)) ELSE ((ra.RecDescuento * ifnull(rf.RecTasa,1))) END as RecDescuento from RecibosAplicacion  ra LEFT JOIN Recibos rf on rf.RecSecuencia = ra.RecSecuencia and rf.RepCodigo = rf.Repcodigo and rf.RecTipo = ra.RecTipo UNION ALL Select 0 as RecDescuentoDesmonte, CxcReferencia, RecSecuencia, RepCodigo, RefValor as RecValor, 0 as RecDescuento from RecibosFormaPago) A " +
                           "ON A.CxcReferencia = C.CXCReferencia and a.recsecuencia in (select r.recSecuencia from recibos r where repcodigo = A.repcodigo and RecEstatus <>0) " +
                           "WHERE ttcOrigen <> -1 AND UPPER(ttcReferencia) <> 'PNF' AND C.ConID <> " + DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado() + " AND C.CliID = " + Arguments.Values.CurrentClient.CliID.ToString() + " " + crtlAreaCredito + " " + porcionMultiMoneda + " " +
                           "GROUP BY C.CxcDocumento, C.CxcReferencia, C.CxcSigla, CxcFecha, C.CxcMontoTotal, C.CxcBalance, ttcOrigen, cxcMontoSinItbis " +
                           "HAVING (C.CxcBalance - COALESCE(SUM(A.RecValor + A.RecDescuento + IFNULL(A.RecDescuentoDesmonte,0) ), 0)) > 0 " +
                           "UNION ALL " +
                           "SELECT CxcFecha, CAST(replace(strftime('%d-%m-%Y', SUBSTR(CxcFecha,1,10)),' ','' ) as varchar) as Fecha, C.CxcDocumento, C.CxcReferencia, CxcColor, " +
                           "C.CxcSigla, 0 AS Aplicado, 0 as Descuento, abs(C.CxcMontoTotal), (abs(C.CxcBalance) - COALESCE(SUM(A.RefValor),0)) AS CxcBalance, " +
                           "(abs(C.CxcBalance) - COALESCE(SUM(A.RefValor),0)) AS Pendiente, 'Pendiente' as Estado, 0  as Credito, " +
                           "CAST(replace(strftime('%m-%d-%Y', SUBSTR(CxcFecha,1,10)),' ','' ) as varchar) as CxcFechaIngles, ttcOrigen, abs(cxcMontoSinItbis), " +
                           "0 DescPorciento, 0 AutID, -1 as FechaDescuento, -1 as Dias,0 DescuentoFactura, '' AS 'Clasificacion', IFNULL(date(CxcFechaVencimiento), '') AS 'FechaVencimiento', '0' as Retencion, ifnull(C.CxcNotaCredito, 0) as CxcNotaCredito, 1 as CalcularDesc, C.CXCNCF as CXCNCF, C.cxcComentario as cxcComentario, ifnull(C.CxcFechaEntrega, ''), C.RepCodigo, C.MonCodigo, CASE T.ttcaplicadescuento WHEN 1 THEN 'Si' ELSE 'No' END as AplicaDescuento, 1 as CalcularDesmonte, 0 as cxcDesmonte " +
                           "FROM CuentasxCobrar C " +
                           "INNER JOIN TiposTransaccionesCXC T ON  trim(T.ttcSigla) = trim(C.CxcSigla) " +
                           "LEFT JOIN RecibosFormaPago A ON A.RefNumeroAutorizacion = C.CXCReferencia and a.recsecuencia in " +
                           "(select r.recSecuencia from recibos r where repcodigo = A.repcodigo and RecEstatus <>0) " +
                           "WHERE ttcOrigen = -1 AND C.ConID <> " + DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado() + " AND C.CliID = " + Arguments.Values.CurrentClient.CliID.ToString() + " " + crtlAreaCredito + " " + porcionMultiMoneda + " " +
                           "GROUP BY C.CxcDocumento, C.CxcReferencia, C.CxcSigla, CxcFecha, C.CxcMontoTotal, C.CxcBalance, ttcOrigen, cxcMontoSinItbis " +
                           "HAVING (abs(C.CxcBalance) - COALESCE(SUM(A.RefValor), 0)) <> 0 " +
                           "ORDER BY Dias DESC";
                }
                else
                {
                    sql = "INSERT INTO RecibosDocumentosTemp(FechaSinFormatear, Fecha, Documento, Referencia, CxcColor, Sigla, Aplicado, Descuento, MontoTotal, Balance, Pendiente, Estado, " +
                           "Credito, FechaIngles, Origen, MontoSinItbis, DescPorciento, AutSecuencia, FechaDescuento, Dias, DescuentoFactura, Clasificacion, " +
                           "FechaVencimiento, Retencion, CxcNotaCredito, CalcularDesc, CXCNCF, cxcComentario, FechaEntrega, RepCodigo, MonCodigo, AplicaDescuento, CalcularDesmonte, Desmonte) " +
                           "SELECT CxcFecha, CAST(replace(strftime('%d-%m-%Y', SUBSTR(CxcFecha,1,10)),' ','') as varchar) as Fecha, C.CxcDocumento, C.CxcReferencia, CxcColor, " +
                           "C.CxcSigla, 0 AS Aplicado, 0 as Descuento, C.CxcMontoTotal, (C.CxcBalance - COALESCE(SUM(A.RecValor + A.RecDescuento + IFNULL(A.RecDescuentoDesmonte,0) ),0)) AS CxcBalance, " +
                           "(C.CxcBalance - COALESCE(SUM(A.RecValor + A.RecDescuento + IFNULL(A.RecDescuentoDesmonte,0)  ),0)) AS Pendiente, 'Pendiente' as Estado, 0  as Credito, " +
                           "CAST(replace(strftime('%m-%d-%Y', SUBSTR(CxcFecha,1,10)),' ','' ) as varchar) as CxcFechaIngles, ttcOrigen, cxcMontoSinItbis, " +
                           "0 as DescPorciento, 0 as AutID, strftime('%d-%m-%Y',DATETIME('NOW', 'localtime'),'" + Functions.GetDiferenciaHorariaSqlite() + " hours') as FechaDescuento, julianday(Cast(strftime('%Y-%m-%d',DATETIME('NOW', 'localtime'),'" + Functions.GetDiferenciaHorariaSqlite() + " hours') as Varchar)) - julianday(SUBSTR(ifnull(CxcFechaEntrega, CxcFecha),1,10)) as Dias, 0 DescuentoFactura, " +
                           "IFNULL(CxcClasificacion, '') AS 'Clasificacion', IFNULL(date(CxcFechaVencimiento), '') AS 'FechaVencimiento', ifnull(CxcRetencion, 0) as Retencion, ifnull(C.CxcNotaCredito, 0) as CxcNotaCredito, 1 as CalcularDesc, C.CXCNCF as CXCNCF, C.cxcComentario as cxcComentario, ifnull(C.CxcFechaEntrega, ''), C.RepCodigo , C.MonCodigo, CASE T.ttcaplicadescuento WHEN 1 THEN 'Si' ELSE 'No' END as AplicaDescuento, 1 as CalcularDesmonte, 0 as cxcDesmonte " +
                           "FROM CuentasxCobrar C " +
                           "INNER JOIN TiposTransaccionesCXC T ON trim(T.ttcSigla) = trim(C.CxcSigla) " +
                           "LEFT JOIN (Select IFNULL(RecDescuentoDesmonte,0) as RecDescuentoDesmonte, CxcReferencia, RecSecuencia, RepCodigo, RecValor, RecDescuento from RecibosAplicacion UNION ALL Select 0 as RecDescuentoDesmonte, CxcReferencia, RecSecuencia, RepCodigo, RefValor as RecValor, 0 as RecDescuento from RecibosFormaPago) A " +
                           "ON A.CxcReferencia = C.CXCReferencia and a.recsecuencia in (select r.recSecuencia from recibos r where repcodigo = A.repcodigo and RecEstatus <>0) " +
                           "WHERE ttcOrigen <> -1 AND UPPER(ttcReferencia) <> 'PNF' AND C.ConID <> " + DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado() + " AND C.CliID = " + Arguments.Values.CurrentClient.CliID.ToString() + " " + crtlAreaCredito + " " + porcionMultiMoneda + " " +
                           "GROUP BY C.CxcDocumento, C.CxcReferencia, C.CxcSigla, CxcFecha, C.CxcMontoTotal, C.CxcBalance, ttcOrigen, cxcMontoSinItbis " +
                           "HAVING (C.CxcBalance - COALESCE(SUM(A.RecValor + A.RecDescuento + IFNULL(A.RecDescuentoDesmonte,0) ), 0)) > 0 " +
                           "UNION ALL " +
                           "SELECT CxcFecha, CAST(replace(strftime('%d-%m-%Y', SUBSTR(CxcFecha,1,10)),' ','' ) as varchar) as Fecha, C.CxcDocumento, C.CxcReferencia, CxcColor, " +
                           "C.CxcSigla, 0 AS Aplicado, 0 as Descuento, abs(C.CxcMontoTotal), (abs(C.CxcBalance) - COALESCE(SUM(A.RefValor),0)) AS CxcBalance, " +
                           "(abs(C.CxcBalance) - COALESCE(SUM(A.RefValor),0)) AS Pendiente, 'Pendiente' as Estado, 0  as Credito, " +
                           "CAST(replace(strftime('%m-%d-%Y', SUBSTR(CxcFecha,1,10)),' ','' ) as varchar) as CxcFechaIngles, ttcOrigen, abs(cxcMontoSinItbis), " +
                           "0 DescPorciento, 0 AutID, -1 as FechaDescuento, -1 as Dias,0 DescuentoFactura, '' AS 'Clasificacion', IFNULL(date(CxcFechaVencimiento), '') AS 'FechaVencimiento', '0' as Retencion, ifnull(C.CxcNotaCredito, 0) as CxcNotaCredito, 1 as CalcularDesc, C.CXCNCF as CXCNCF, C.cxcComentario as cxcComentario, ifnull(C.CxcFechaEntrega, ''), C.RepCodigo, C.MonCodigo, CASE T.ttcaplicadescuento WHEN 1 THEN 'Si' ELSE 'No' END as AplicaDescuento, 1 as CalcularDesmonte, 0 as cxcDesmonte " +
                           "FROM CuentasxCobrar C " +
                           "INNER JOIN TiposTransaccionesCXC T ON  trim(T.ttcSigla) = trim(C.CxcSigla) " +
                           "LEFT JOIN RecibosFormaPago A ON A.RefNumeroAutorizacion = C.CXCReferencia and a.recsecuencia in " +
                           "(select r.recSecuencia from recibos r where repcodigo = A.repcodigo and RecEstatus <>0) " +
                           "WHERE ttcOrigen = -1 AND C.ConID <> " + DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado() + " AND C.CliID = " + Arguments.Values.CurrentClient.CliID.ToString() + " " + crtlAreaCredito + " " + porcionMultiMoneda + " " +
                           "GROUP BY C.CxcDocumento, C.CxcReferencia, C.CxcSigla, CxcFecha, C.CxcMontoTotal, C.CxcBalance, ttcOrigen, cxcMontoSinItbis " +
                           "HAVING (abs(C.CxcBalance) - COALESCE(SUM(A.RefValor), 0)) <> 0 " +
                           "ORDER BY Dias DESC";
                }


            }
            else if (myParametro.GetParRecibosImportadoraLaPlaza())
            {
                sql = "INSERT INTO RecibosDocumentosTemp(FechaSinFormatear, Fecha, Documento, Referencia, Sigla, Aplicado, Descuento, MontoTotal, Balance, Pendiente, Estado, CxcColor, " +
                        "Credito, FechaIngles, Origen, MontoSinItbis, DescPorciento, AutSecuencia, FechaDescuento, Dias, DescuentoFactura, Clasificacion, FechaVencimiento, Retencion, CxcNotaCredito, CalcularDesc, CXCNCF, cxcComentario, FechaEntrega, RepCodigo, MonCodigo, AplicaDescuento, CalcularDesmonte, Desmonte) " +
                        "SELECT result.* FROM (SELECT CxcFecha, CAST(replace(strftime('%d-%m-%Y', SUBSTR(CxcFecha,1,10)),' ','') as varchar) as Fecha, C.CxcDocumento, C.CxcReferencia, " +
                        "C.CxcSigla, 0 AS Aplicado, 0 as Descuento, C.CxcMontoTotal, (C.CxcBalance - COALESCE(SUM(A.RecValor + A.RecDescuento + IFNULL(A.RecDescuentoDesmonte,0) ),0)) AS CxcBalance, " +
                        "(C.CxcBalance - COALESCE(SUM(A.RecValor + A.RecDescuento + IFNULL(A.RecDescuentoDesmonte,0)  ),0)) AS Pendiente, 'Pendiente' as Estado, CxcColor, 0  as Credito, " +
                        "CAST(replace(strftime('%m-%d-%Y', SUBSTR(CxcFecha,1,10)),' ','' ) as varchar) as CxcFechaIngles, ttcOrigen, cxcMontoSinItbis, " +
                        //"-1 DescPorciento, 0 AutID, strftime('%d-%m-%Y','now') as FechaDescuento, "+ActivityRecibosDocumentosTab.FechaDescuento+" - julianday(SUBSTR(CxcFecha,1,10)) as Dias " +
                        "0 DescPorciento, 0 AutID, strftime('%d-%m-%Y',DATETIME('NOW', 'localtime'),'" + Functions.GetDiferenciaHorariaSqlite() + " hours') as FechaDescuento, julianday(Cast(strftime('%Y-%m-%d',DATETIME('NOW', 'localtime'),'" + Functions.GetDiferenciaHorariaSqlite() + " hours') as Varchar)) - julianday(SUBSTR(ifnull(CxcFechaEntrega, CxcFecha),1,10)) as Dias, 0 DescuentoFactura, " +
                        "IFNULL(CxcClasificacion, '') AS 'Clasificacion', IFNULL(strftime('%d-%m-%Y', date(CxcFechaVencimiento)), '') AS FechaVencimiento, ifnull(CxcRetencion, 0) as Retencion, ifnull(C.CxcNotaCredito, 0) as CxcNotaCredito, 1 as CalcularDesc, C.CXCNCF as CXCNCF, C.cxcComentario as cxcComentario, ifnull(C.CxcFechaEntrega, ''), C.RepCodigo, C.MonCodigo, CASE T.ttcaplicadescuento WHEN 1 THEN 'Si' ELSE 'No' END as AplicaDescuento, 1 as CalcularDesmonte, 0 as cxcDesmonte " +
                        "FROM CuentasxCobrar C " +
                        "INNER JOIN TiposTransaccionesCXC T ON trim(T.ttcSigla) = trim(C.CxcSigla) " +
                        "LEFT JOIN (Select IFNULL(RecDescuentoDesmonte,0) as RecDescuentoDesmonte, CxcReferencia, RecSecuencia, RepCodigo, RecValor, RecDescuento from RecibosAplicacion UNION ALL Select 0 as RecDescuentoDesmonte, CxcReferencia, RecSecuencia, RepCodigo, RefValor as RecValor, 0 as RecDescuento from RecibosFormaPago) A " +
                        "ON A.CxcReferencia = C.CXCReferencia and a.recsecuencia in (select r.recSecuencia from recibos r where repcodigo = A.repcodigo and RecEstatus <>0) " +
                        "WHERE ttcReferencia <> 'NC' AND UPPER(ttcReferencia) <> 'PNF' AND C.CliID = " + Arguments.Values.CurrentClient.CliID.ToString() + " " + porcionMultiMoneda + " " +
                        "GROUP BY C.CxcDocumento, C.CxcReferencia, C.CxcSigla, CxcFecha, C.CxcMontoTotal, C.CxcBalance, ttcOrigen, cxcMontoSinItbis " +
                        "HAVING (C.CxcBalance - ROUND(COALESCE(SUM(A.RecValor + A.RecDescuento + IFNULL(A.RecDescuentoDesmonte,0) ), 0), 2) >= 0.01) " +
                        "UNION ALL " +
                        "SELECT CxcFecha, CAST(replace(strftime('%d-%m-%Y', SUBSTR(CxcFecha,1,10)),' ','' ) as varchar) as Fecha, C.CxcDocumento, C.CxcReferencia, " +
                        "C.CxcSigla, 0 AS Aplicado, 0 as Descuento, abs(C.CxcMontoTotal), (abs(C.CxcBalance) - COALESCE(SUM(A.RefValor),0)) AS CxcBalance, " +
                        "(abs(C.CxcBalance) - COALESCE(SUM(A.RefValor),0)) AS Pendiente, 'Pendiente' as Estado, CxcColor, 0  as Credito, " +
                        "CAST(replace(strftime('%m-%d-%Y', SUBSTR(CxcFecha,1,10)),' ','' ) as varchar) as CxcFechaIngles, ttcOrigen, abs(cxcMontoSinItbis), " +
                        "0 DescPorciento, 0 AutID, -1 as FechaDescuento, -1 as Dias,0 DescuentoFactura, '' AS 'Clasificacion', " +
                        "IFNULL(strftime('%d-%m-%Y', date(CxcFechaVencimiento)), '') AS FechaVencimiento, '0' as Retencion, ifnull(C.CxcNotaCredito, 0) as CxcNotaCredito, " +
                        "1 as CalcularDesc, C.CXCNCF as CXCNCF, C.cxcComentario as cxcComentario, ifnull(C.CxcFechaEntrega, ''), C.RepCodigo, C.MonCodigo, CASE T.ttcaplicadescuento WHEN 1 THEN 'Si' ELSE 'No' END as AplicaDescuento, 1 as CalcularDesmonte, 0 as cxcDesmonte " +
                        "FROM CuentasxCobrar C " +
                        "INNER JOIN TiposTransaccionesCXC T ON  trim(T.ttcSigla) = trim(C.CxcSigla) " +
                        "LEFT JOIN RecibosFormaPago A ON A.RefNumeroAutorizacion = C.CXCReferencia and a.recsecuencia in " +
                        "(select r.recSecuencia from recibos r where repcodigo = A.repcodigo and RecEstatus <>0) " +
                        "WHERE ttcReferencia = 'NC' AND C.CliID = " + Arguments.Values.CurrentClient.CliID.ToString() + " " + porcionMultiMoneda + " " +
                        "GROUP BY C.CxcDocumento, C.CxcReferencia, C.CxcSigla, CxcFecha, C.CxcMontoTotal, C.CxcBalance, ttcOrigen, cxcMontoSinItbis " +
                        "HAVING (ROUND(abs(C.CxcBalance), 2) - ROUND(COALESCE(SUM(A.RefValor), 0), 2) <> 0)) result " +
                        "ORDER BY (substr(result.FechaVencimiento,7)||substr(result.FechaVencimiento,4,2)||substr(result.FechaVencimiento,1,2))  ASC";
            }
            else
            {
                if (myParametro.GetParRecibosusarMonedaUnica())
                {
                    sql = "INSERT INTO RecibosDocumentosTemp(FechaSinFormatear, Fecha, Documento, Referencia, Sigla, Aplicado, Descuento, MontoTotal, Balance, Pendiente, Estado, CxcColor, " +
                            "Credito, FechaIngles, Origen, MontoSinItbis, DescPorciento, AutSecuencia, FechaDescuento, Dias, DescuentoFactura, Clasificacion, FechaVencimiento, Retencion, " +
                            "CxcNotaCredito, CalcularDesc, CXCNCF, cxcComentario, FechaEntrega, RepCodigo, MonCodigo, AplicaDescuento, CalcularDesmonte, Desmonte) " +
                            "SELECT CxcFecha, CAST(replace(strftime('%d-%m-%Y', SUBSTR(CxcFecha,1,10)),' ','') as varchar) as Fecha, C.CxcDocumento, C.CxcReferencia, " +
                            "C.CxcSigla, 0 AS Aplicado, 0 as Descuento, C.CxcMontoTotal, (C.CxcBalance - COALESCE(SUM(A.RecValor + A.RecDescuento + IFNULL(A.RecDescuentoDesmonte,0) ),0)) AS CxcBalance, " +
                            "(C.CxcBalance - COALESCE(SUM(A.RecValor + A.RecDescuento + IFNULL(A.RecDescuentoDesmonte,0)  ),0)) AS Pendiente, 'Pendiente' as Estado, CxcColor, 0  as Credito, " +
                            "CAST(replace(strftime('%m-%d-%Y', SUBSTR(CxcFecha,1,10)),' ','' ) as varchar) as CxcFechaIngles, ttcOrigen, cxcMontoSinItbis, " +
                            "0 DescPorciento, 0 AutID, strftime('%d-%m-%Y',DATETIME('NOW', 'localtime'),'" + Functions.GetDiferenciaHorariaSqlite() + " hours') as FechaDescuento, julianday(Cast(strftime('%Y-%m-%d',DATETIME('NOW', 'localtime'),'" + Functions.GetDiferenciaHorariaSqlite() + " hours') as Varchar)) - julianday(SUBSTR(ifnull(CxcFechaEntrega, CxcFecha),1,10)) as Dias, 0 DescuentoFactura, " +
                            "IFNULL(CxcClasificacion, '') AS 'Clasificacion', IFNULL(date(CxcFechaVencimiento), '') AS 'FechaVencimiento', ifnull(CxcRetencion, 0) as Retencion, " +
                            "ifnull(C.CxcNotaCredito, 0) as CxcNotaCredito, 1 as CalcularDesc, C.CXCNCF as CXCNCF, C.cxcComentario as cxcComentario, ifnull(C.CxcFechaEntrega, ''), C.RepCodigo, C.MonCodigo, CASE T.ttcaplicadescuento WHEN 1 THEN 'Si' ELSE 'No' END as AplicaDescuento, 1 as CalcularDesmonte, 0 as cxcDesmonte  " +
                            "FROM CuentasxCobrar C " +
                            "INNER JOIN TiposTransaccionesCXC T ON trim(T.ttcSigla) = trim(C.CxcSigla) " +
                            "LEFT JOIN (Select IFNULL(ra.RecDescuentoDesmonte,0) as RecDescuentoDesmonte, ra.CxcReferencia, ra.RecSecuencia, ra.RepCodigo, case  when ra.monCodigo = 'USD' and rf.monCodigo ='USD' THEN ((ra.RecValor * ifnull(rf.RecTasa,1)) / Ifnull(rf.RecTasa,1)) when ra.monCodigo = 'USD' and rf.monCodigo != 'USD' THEN ((ra.RecValor * ifnull(rf.RecTasa,1)) / Ifnull(ra.RecTasa,1)) ELSE ((ra.RecValor * ifnull(rf.RecTasa,1))) END as RecValor, case ra.monCodigo when 'USD' THEN ((ra.RecDescuento * ifnull(rf.RecTasa,1)) / Ifnull(rf.RecTasa,1)) ELSE ((ra.RecDescuento * ifnull(rf.RecTasa,1))) END as RecDescuento from RecibosAplicacion  ra LEFT JOIN Recibos rf on rf.RecSecuencia = ra.RecSecuencia and rf.RepCodigo = rf.Repcodigo and rf.RecTipo = ra.RecTipo UNION ALL Select 0 as RecDescuentoDesmonte, CxcReferencia, RecSecuencia, RepCodigo, RefValor as RecValor, 0 as RecDescuento from RecibosFormaPago) A " +
                            "ON A.CxcReferencia = C.CXCReferencia and a.recsecuencia in (select r.recSecuencia from recibos r where repcodigo = A.repcodigo and RecEstatus <>0) " +
                            "WHERE ttcReferencia <> 'NC' AND UPPER(ttcReferencia) <> 'PNF' AND C.CliID = " + Arguments.Values.CurrentClient.CliID.ToString() + "  " + porcionMultiMoneda + "   " +
                            "GROUP BY C.CxcDocumento, C.CxcReferencia, C.CxcSigla, CxcFecha, C.CxcMontoTotal, C.CxcBalance, ttcOrigen, cxcMontoSinItbis " +
                            "HAVING (abs(C.CxcBalance) - ROUND(COALESCE(SUM(A.RecValor + A.RecDescuento + IFNULL(A.RecDescuentoDesmonte,0) ), 0), 2) >= 0.01) " +
                            "UNION ALL " +
                            "SELECT CxcFecha, CAST(replace(strftime('%d-%m-%Y', SUBSTR(CxcFecha,1,10)),' ','' ) as varchar) as Fecha, C.CxcDocumento, C.CxcReferencia,  " +
                            "C.CxcSigla, 0 AS Aplicado, 0 as Descuento, abs(C.CxcMontoTotal), (abs(C.CxcBalance) - COALESCE(SUM(A.RefValor),0)) AS CxcBalance, " +
                            "(abs(C.CxcBalance) - COALESCE(SUM(A.RefValor),0)) AS Pendiente, 'Pendiente' as Estado, CxcColor, 0  as Credito, " +
                            "CAST(replace(strftime('%m-%d-%Y', SUBSTR(CxcFecha,1,10)),' ','' ) as varchar) as CxcFechaIngles, ttcOrigen, abs(cxcMontoSinItbis), " +
                            "0 DescPorciento, 0 AutID, -1 as FechaDescuento, -1 as Dias,0 DescuentoFactura, '' AS 'Clasificacion', " +
                            "IFNULL(date(CxcFechaVencimiento), '') AS 'FechaVencimiento', '0' as Retencion, ifnull(C.CxcNotaCredito, 0) as CxcNotaCredito, " +
                            "1 as CalcularDesc, C.CXCNCF as CXCNCF, C.cxcComentario as cxcComentario, ifnull(C.CxcFechaEntrega, ''), C.RepCodigo, C.MonCodigo, CASE T.ttcaplicadescuento WHEN 1 THEN 'Si' ELSE 'No' END as AplicaDescuento, 1 as CalcularDesmonte, 0 as cxcDesmonte " +
                            "FROM CuentasxCobrar C " +
                            "INNER JOIN TiposTransaccionesCXC T ON  trim(T.ttcSigla) = trim(C.CxcSigla) " +
                            "LEFT JOIN RecibosFormaPago A ON A.RefNumeroAutorizacion = C.CXCReferencia and a.recsecuencia in " +
                            "(select r.recSecuencia from recibos r where repcodigo = A.repcodigo and RecEstatus <>0) " +
                            "WHERE ttcReferencia = 'NC' AND C.CliID = " + Arguments.Values.CurrentClient.CliID.ToString() + "  " + porcionMultiMoneda + " " +
                            "GROUP BY C.CxcDocumento, C.CxcReferencia, C.CxcSigla, CxcFecha, C.CxcMontoTotal, C.CxcBalance, ttcOrigen, cxcMontoSinItbis " +
                            "HAVING (ROUND(abs(C.CxcBalance), 2) - ROUND(COALESCE(SUM(A.RefValor), 0), 2) <> 0) " +
                            "ORDER BY Dias DESC";
                }
                else
                {
                    sql = "INSERT INTO RecibosDocumentosTemp(FechaSinFormatear, Fecha, Documento, Referencia, Sigla, Aplicado, Descuento, MontoTotal, Balance, Pendiente, Estado, CxcColor, " +
                        "Credito, FechaIngles, Origen, MontoSinItbis, DescPorciento, AutSecuencia, FechaDescuento, Dias, DescuentoFactura, Clasificacion, FechaVencimiento, Retencion, " +
                        "CxcNotaCredito, CalcularDesc, CXCNCF, cxcComentario, FechaEntrega, RepCodigo, MonCodigo, AplicaDescuento, CalcularDesmonte, Desmonte) " +
                        "SELECT CxcFecha, CAST(replace(strftime('%d-%m-%Y', SUBSTR(CxcFecha,1,10)),' ','') as varchar) as Fecha, C.CxcDocumento, C.CxcReferencia, " +
                        "C.CxcSigla, 0 AS Aplicado, 0 as Descuento, C.CxcMontoTotal, (C.CxcBalance - COALESCE(SUM(A.RecValor + A.RecDescuento + IFNULL(A.RecDescuentoDesmonte,0) ),0)) AS CxcBalance, " +
                        "(C.CxcBalance - COALESCE(SUM(A.RecValor + A.RecDescuento + IFNULL(A.RecDescuentoDesmonte,0)  ),0)) AS Pendiente, 'Pendiente' as Estado, CxcColor, 0  as Credito, " +
                        "CAST(replace(strftime('%m-%d-%Y', SUBSTR(CxcFecha,1,10)),' ','' ) as varchar) as CxcFechaIngles, ttcOrigen, cxcMontoSinItbis, " +
                        "0 DescPorciento, 0 AutID, strftime('%d-%m-%Y',DATETIME('NOW', 'localtime'),'" + Functions.GetDiferenciaHorariaSqlite() + " hours') as FechaDescuento, julianday(Cast(strftime('%Y-%m-%d',DATETIME('NOW', 'localtime'),'" + Functions.GetDiferenciaHorariaSqlite() + " hours') as Varchar)) - julianday(SUBSTR(ifnull(CxcFechaEntrega, CxcFecha),1,10)) as Dias, 0 DescuentoFactura, " +
                        "IFNULL(CxcClasificacion, '') AS 'Clasificacion', IFNULL(date(CxcFechaVencimiento), '') AS 'FechaVencimiento', ifnull(CxcRetencion, 0) as Retencion, " +
                        "ifnull(C.CxcNotaCredito, 0) as CxcNotaCredito, 1 as CalcularDesc, C.CXCNCF as CXCNCF, C.cxcComentario as cxcComentario, ifnull(C.CxcFechaEntrega, ''), C.RepCodigo, C.MonCodigo, CASE T.ttcaplicadescuento WHEN 1 THEN 'Si' ELSE 'No' END as AplicaDescuento, 1 as CalcularDesmonte, 0 as cxcDesmonte " +
                        "FROM CuentasxCobrar C " +
                        "INNER JOIN TiposTransaccionesCXC T ON trim(T.ttcSigla) = trim(C.CxcSigla) " +
                        "LEFT JOIN (Select IFNULL(RecDescuentoDesmonte,0) as RecDescuentoDesmonte, CxcReferencia, RecSecuencia, RepCodigo, RecValor, RecDescuento from RecibosAplicacion UNION ALL Select 0 as RecDescuentoDesmonte, CxcReferencia, RecSecuencia, RepCodigo, RefValor as RecValor, 0 as RecDescuento from RecibosFormaPago) A " +
                        "ON A.CxcReferencia = C.CXCReferencia and a.recsecuencia in (select r.recSecuencia from recibos r where repcodigo = A.repcodigo and RecEstatus <>0) " +
                        "WHERE ttcReferencia <> 'NC' AND UPPER(ttcReferencia) <> 'PNF' AND C.CliID = " + Arguments.Values.CurrentClient.CliID.ToString() + "  " + porcionMultiMoneda + "   " +
                        "GROUP BY C.CxcDocumento, C.CxcReferencia, C.CxcSigla, CxcFecha, C.CxcMontoTotal, C.CxcBalance, ttcOrigen, cxcMontoSinItbis " +
                        "HAVING (abs(C.CxcBalance) - ROUND(COALESCE(SUM(A.RecValor + A.RecDescuento + IFNULL(A.RecDescuentoDesmonte,0) ), 0), 2) >= 0.01) " +
                        "UNION ALL " +
                        "SELECT CxcFecha, CAST(replace(strftime('%d-%m-%Y', SUBSTR(CxcFecha,1,10)),' ','' ) as varchar) as Fecha, C.CxcDocumento, C.CxcReferencia,  " +
                        "C.CxcSigla, 0 AS Aplicado, 0 as Descuento, abs(C.CxcMontoTotal), (abs(C.CxcBalance) - COALESCE(SUM(A.RefValor),0)) AS CxcBalance, " +
                        "(abs(C.CxcBalance) - COALESCE(SUM(A.RefValor),0)) AS Pendiente, 'Pendiente' as Estado, CxcColor, 0  as Credito, " +
                        "CAST(replace(strftime('%m-%d-%Y', SUBSTR(CxcFecha,1,10)),' ','' ) as varchar) as CxcFechaIngles, ttcOrigen, abs(cxcMontoSinItbis), " +
                        "0 DescPorciento, 0 AutID, -1 as FechaDescuento, -1 as Dias,0 DescuentoFactura, '' AS 'Clasificacion', " +
                        "IFNULL(date(CxcFechaVencimiento), '') AS 'FechaVencimiento', '0' as Retencion, ifnull(C.CxcNotaCredito, 0) as CxcNotaCredito, " +
                        "1 as CalcularDesc, C.CXCNCF as CXCNCF, C.cxcComentario as cxcComentario, ifnull(C.CxcFechaEntrega, ''), C.RepCodigo, C.MonCodigo, CASE T.ttcaplicadescuento WHEN 1 THEN 'Si' ELSE 'No' END as AplicaDescuento, 1 as CalcularDesmonte, 0 as cxcDesmonte " +
                        "FROM CuentasxCobrar C " +
                        "INNER JOIN TiposTransaccionesCXC T ON  trim(T.ttcSigla) = trim(C.CxcSigla) " +
                        "LEFT JOIN RecibosFormaPago A ON A.RefNumeroAutorizacion = C.CXCReferencia and a.recsecuencia in " +
                        "(select r.recSecuencia from recibos r where repcodigo = A.repcodigo and RecEstatus <>0) " +
                        "WHERE ttcReferencia = 'NC' AND C.CliID = " + Arguments.Values.CurrentClient.CliID.ToString() + "  " + porcionMultiMoneda + " " +
                        "GROUP BY C.CxcDocumento, C.CxcReferencia, C.CxcSigla, CxcFecha, C.CxcMontoTotal, C.CxcBalance, ttcOrigen, cxcMontoSinItbis " +
                        "HAVING (ROUND(abs(C.CxcBalance), 2) - ROUND(COALESCE(SUM(A.RefValor), 0), 2) <> 0) " +
                        "ORDER BY Dias DESC";
                }


            }

            if (myParametro.GetParRecPedVen())
            {
                string sql2 =
                        "INSERT INTO RecibosDocumentosTemp(FechaSinFormatear, Fecha, Documento, Referencia, Sigla, Aplicado, Descuento, MontoTotal, Balance, Pendiente, Estado, " +
                                "Credito, FechaIngles, Origen, MontoSinItbis, DescPorciento, AutSecuencia, FechaDescuento, Dias, DescuentoFactura, Clasificacion, FechaVencimiento, CalcularDesc, CXCNCF, cxcComentario, CalcularDesmonte) " +
                                "SELECT Fecha, Fecha, Documento AS Documento, Documento AS Referencia, " +
                                "Sigla AS Sigla, 0 AS Aplicado, 0 AS Descuento, " +
                                "MontoTotal AS MontoTotal, MontoTotal AS Balance, MontoTotal AS Pendiente, " +
                                "'Pendiente' as Estado, 0 AS Credito, FechaIngles AS FechaIngles, 1 AS Origen, " +
                                "MontoSinItbis AS MontoSinItbis, " +
                                "0 AS DescPorciento, 0 AS AutID, Fecha AS FechaDescuento, Dias as Dias, " +
                                "0 AS DescuentoFactura, '' AS Clasificacion, '' AS FechaVencimiento, 1 as CalcularDesc, '' as CXCNCF, '' as cxcComentario, 1 as CalcularDesmonte " +
                                "FROM PedidosVentasView p " +
                                "WHERE p.CliID = ? AND Documento NOT IN(SELECT CxcReferencia FROM RecibosAplicacion " +
                                "UNION SELECT CxcReferencia FROM RecibosAplicacionConfirmados) ";


                SqliteManager.GetInstance().Execute(sql2, new string[] { Arguments.Values.CurrentClient.CliID.ToString() });
            }

            if (myParametro.GetParReconciliacion() && Arguments.Values.CurrentModule == Modules.RECONCILIACION)
            {
                sql = "INSERT INTO RecibosDocumentosTemp"
                        + "(FechaSinFormatear, Fecha, Documento, Referencia, Sigla, Aplicado, Descuento, MontoTotal, Balance, Pendiente, Estado, Credito, FechaIngles, Origen, CxcColor, " +
                        "MontoSinItbis, DescPorciento, AutSecuencia, FechaDescuento, Dias, DescuentoFactura, Clasificacion, FechaVencimiento, Retencion, CxcNotaCredito, CalcularDesc, CXCNCF, cxcComentario, RepCodigo, MonCodigo, AplicaDescuento, CalcularDesmonte, Desmonte)"
                        + "SELECT CxcFecha, CAST(replace(strftime('%d-%m-%Y', SUBSTR(CxcFecha,1,10)),' ','') as varchar) as Fecha, C.CxcDocumento, C.CxcReferencia, C.CxcSigla, 0 AS Aplicado, 0 as Descuento, " +
                        "C.CxcMontoTotal, (C.CxcBalance - COALESCE(SUM(A.RecValor + A.RecDescuento + IFNULL(A.RecDescuentoDesmonte,0) ),0) - ifnull(sum(RD.RefValor),0)) AS CxcBalance, " +
                        "(C.CxcBalance - COALESCE(SUM(A.RecValor + A.RecDescuento + IFNULL(A.RecDescuentoDesmonte,0) ),0)) AS Pendiente, 'Pendiente' as Estado, 0  as Credito, " +
                        "CAST(replace(strftime('%m-%d-%Y', SUBSTR(CxcFecha,1,10)),' ','' ) as varchar) as CxcFechaIngles, ttcOrigen, CxcColor, cxcMontoSinItbis, 0 DescPorciento, " +
                        "0 AutID, strftime('%d-%m-%Y',DATETIME('NOW', 'localtime'),'" + Functions.GetDiferenciaHorariaSqlite() + " hours') as FechaDescuento, " +
                        "julianday(Cast(strftime('%Y-%m-%d',DATETIME('NOW', 'localtime'),'" + Functions.GetDiferenciaHorariaSqlite() + " hours') as Varchar)) - julianday(SUBSTR(ifnull(CxcFechaEntrega, CxcFecha),1,10)) as Dias, " +
                        "0 DescuentoFactura, IFNULL(CxcClasificacion, '') AS 'Clasificacion', IFNULL(date(CxcFechaVencimiento), '') AS 'FechaVencimiento', " +
                        "CxcRetencion as Retencion, ifnull(C.CxcNotaCredito, 0) as CxcNotaCredito, 1 as CalcularDesc, C.CXCNCF as CXCNCF, C.cxcComentario as cxcComentario, C.RepCodigo, C.MonCodigo, CASE T.ttcaplicadescuento WHEN 1 THEN 'Si' ELSE 'No' END as AplicaDescuento, 1 as CalcularDesmonte, 0 as cxcDesmonte "
                        + "FROM CuentasxCobrar C "
                        + "INNER JOIN TiposTransaccionesCXC T ON trim(T.ttcSigla) = trim(C.CxcSigla) "
                        + "LEFT JOIN (Select IFNULL(RecDescuentoDesmonte,0) as RecDescuentoDesmonte, CxcReferencia, RecSecuencia, RepCodigo, RecValor, RecDescuento from RecibosAplicacion UNION ALL Select 0 as RecDescuentoDesmonte, CxcReferencia, RecSecuencia, RepCodigo, RefValor as RecValor, 0 as RecDescuento from RecibosFormaPago) A ON A.CxcReferencia = C.CXCReferencia AND a.recsecuencia in (select r.recSecuencia from recibos r where repcodigo = A.repcodigo and RecEstatus <>0) "
                        + "	LEFT JOIN (Select CxcReferenciaAplica, RecSecuencia, RepCodigo, sum(RefValor) as RefValor from reconciliacionesDetalle) RD ON RD.CxcReferenciaAplica = C.CXCReferencia AND RD.recsecuencia in (select RCN.recSecuencia from reconciliaciones RCN where repcodigo = RCN.repcodigo and RecEstatus <>0) "
                        + "WHERE ttcReferencia <> 'NC' AND UPPER(ttcReferencia) <> 'PNF' AND C.CliID =" + Arguments.Values.CurrentClient.CliID.ToString() + " " + porcionMultiMoneda + "  "
                        + "			  GROUP BY C.CxcDocumento, C.CxcReferencia, C.CxcSigla, CxcFecha, C.CxcMontoTotal, C.CxcBalance, ttcOrigen, cxcMontoSinItbis "
                        + "		  HAVING ((C.CxcBalance - ROUND(COALESCE(SUM(A.RecValor + A.RecDescuento + IFNULL(A.RecDescuentoDesmonte,0) ), 0), 2) >= 0.01) AND (C.CxcBalance - ROUND(COALESCE(SUM(RD.RefValor), 0), 2) >= 0.01) AND (C.CxcBalance - ROUND(COALESCE(SUM(RD.RefValor + A.RecValor + A.RecDescuento ), 0), 2) >= 0.01)) "
                        + "UNION ALL "
                        + "SELECT CxcFecha, CAST(replace(strftime('%d-%m-%Y', SUBSTR(CxcFecha,1,10)),' ','' ) as varchar) as Fecha, C.CxcDocumento, C.CxcReferencia, " +
                        "C.CxcSigla, 0 AS Aplicado, 0 as Descuento, abs(C.CxcMontoTotal), (abs(C.CxcBalance) - COALESCE(SUM(A.RefValor),0)) AS CxcBalance, " +
                        "(abs(C.CxcBalance) - COALESCE(SUM(A.RefValor),0)) AS Pendiente, 'Pendiente' as Estado, 0  as Credito, " +
                        "CAST(replace(strftime('%m-%d-%Y', SUBSTR(CxcFecha,1,10)),' ','' ) as varchar) as CxcFechaIngles, ttcOrigen, CxcColor, abs(cxcMontoSinItbis), 0 DescPorciento, 0 AutID, -1 as FechaDescuento, " +
                        "-1 as Dias,0 DescuentoFactura, '' AS 'Clasificacion', IFNULL(date(CxcFechaVencimiento), '') AS 'FechaVencimiento', '0' as Retencion, " +
                        "ifnull(C.CxcNotaCredito, 0) as CxcNotaCredito, 1 as CalcularDesc, C.CXCNCF as CXCNCF, C.cxcComentario as cxcComentario, C.RepCodigo, C.MonCodigo, CASE T.ttcaplicadescuento WHEN 1 THEN 'Si' ELSE 'No' END as AplicaDescuento, 1 as CalcularDesmonte, 0 as cxcDesmonte "
                        + "FROM CuentasxCobrar C "
                        + "INNER JOIN TiposTransaccionesCXC T ON  trim(T.ttcSigla) = trim(C.CxcSigla) "
                        + "LEFT JOIN RecibosFormaPago A ON A.RefNumeroAutorizacion = C.CXCReferencia AND a.recsecuencia in (select r.recSecuencia from recibos r where repcodigo = A.repcodigo and RecEstatus <>0) "
                        + "WHERE ttcReferencia = 'NC' AND C.CliID =" + Arguments.Values.CurrentClient.CliID.ToString() + "  " +
                        "AND C.CxcReferencia not in(select CXCReferencia from ReconciliacionesDetalle where CXCReferencia = C.CxcReferencia) " + porcionMultiMoneda + " "
                        + "GROUP BY C.CxcDocumento, C.CxcReferencia, C.CxcSigla, CxcFecha, C.CxcMontoTotal, C.CxcBalance, ttcOrigen, cxcMontoSinItbis "
                        + "HAVING (ROUND(abs(C.CxcBalance), 2) - ROUND(COALESCE(SUM(A.RefValor), 0), 2) <> 0) "
                        + "ORDER BY Dias DESC";

                SqliteManager.GetInstance().Execute("delete from RecibosDocumentosTemp");
            }

            SqliteManager.GetInstance().Execute(sql);

            try
            {
                if (!string.IsNullOrWhiteSpace(monCodigo) && !myParametro.GetParRecibosusarMonedaUnica())
                {
                    SqliteManager.GetInstance().Execute("update RecibosDocumentosTemp set MonCodigo = '" + monCodigo + "'");
                }

                if (Arguments.Values.CurrentModule == Modules.RECONCILIACION)
                {
                    SqliteManager.GetInstance().Execute("update RecibosDocumentosTemp set EsReconciliacion = 1");
                }

                if (myParametro.GetParRecibosItbisMenos30Dias())
                {
                    SqliteManager.GetInstance().Execute("update RecibosDocumentosTemp set DefIndicadorItbis = 1 where ifnull(Dias, 0) <= 30 and ifnull(Origen, 0) > 0");
                }

                if (!myParametro.GetParRecibosItbisMenos30Dias())
                {
                    var query = "select distinct Referencia, ifnull(Dias, 0) as Dias from RecibosDocumentosTemp t where ifnull(Origen, 0) > 0";
                    var recibosDocumentos = SqliteManager.GetInstance().Query<RecibosDocumentosTemp>(query);
                    foreach (var rec in recibosDocumentos)
                    {
                        var defIndicadorItbis = DS_DescuentoFacturas.GetInstance().GetIndicadorItbisDescuentoDisponible(rec.Referencia, rec.Dias);
                        SqliteManager.GetInstance().Execute("update RecibosDocumentosTemp set DefIndicadorItbis = " + defIndicadorItbis + " where Referencia = '" + rec.Referencia + "' and ifnull(Origen, 0) > 0");
                    }
                }


            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        public void InsertNCPendingDocumentsInTemp(string monCodigo)
        {
            string sql;
            string porcionMultiMoneda = "";
            if (monCodigo != null)
            {
                porcionMultiMoneda = " AND trim(upper(C.MonCodigo)) = trim(upper('" + monCodigo + "')) ";
            }


            sql = "INSERT INTO RecibosDocumentosTemp(FechaSinFormatear, Fecha, Documento, Referencia, Sigla, Aplicado, Descuento, MontoTotal, Balance, Pendiente, Estado, CxcColor, " +
                    "Credito, FechaIngles, Origen, MontoSinItbis, DescPorciento, AutSecuencia, FechaDescuento, Dias, DescuentoFactura, Clasificacion, FechaVencimiento, Retencion, " +
                    "CxcNotaCredito, CalcularDesc, CXCNCF, cxcComentario, FechaEntrega, AplicaDescuento, CalcularDesmonte, Desmonte) " +
                    "SELECT CxcFecha, CAST(replace(strftime('%d-%m-%Y', SUBSTR(CxcFecha,1,10)),' ','' ) as varchar) as Fecha, C.CxcDocumento, C.CxcReferencia,  " +
                    "C.CxcSigla, 0 AS Aplicado, 0 as Descuento, abs(C.CxcMontoTotal), (abs(C.CxcBalance) - COALESCE(SUM(A.RefValor),0)) AS CxcBalance, " +
                    "(abs(C.CxcBalance) - COALESCE(SUM(A.RefValor),0)) AS Pendiente, 'Pendiente' as Estado, CxcColor, 0  as Credito, " +
                    "CAST(replace(strftime('%m-%d-%Y', SUBSTR(CxcFecha,1,10)),' ','' ) as varchar) as CxcFechaIngles, ttcOrigen, abs(cxcMontoSinItbis), " +
                    "0 DescPorciento, 0 AutID, -1 as FechaDescuento, -1 as Dias,0 DescuentoFactura, '' AS 'Clasificacion', " +
                    "IFNULL(date(CxcFechaVencimiento), '') AS 'FechaVencimiento', '0' as Retencion, ifnull(C.CxcNotaCredito, 0) as CxcNotaCredito, " +
                    "1 as CalcularDesc, C.CXCNCF as CXCNCF, C.cxcComentario as cxcComentario, ifnull(C.CxcFechaEntrega, ''), CASE T.ttcaplicadescuento WHEN 1 THEN 'Si' ELSE 'No' END as AplicaDescuento, 1 as CalcularDesmonte, 0 as cxcDesmonte " +
                    "FROM CuentasxCobrar C " +
                    "INNER JOIN TiposTransaccionesCXC T ON trim(T.ttcSigla) = trim(C.CxcSigla) " +
                    "LEFT JOIN RecibosFormaPago A ON A.RefNumeroAutorizacion = C.CXCReferencia and a.recsecuencia in " +
                    "(select r.recSecuencia from recibos r where repcodigo = A.repcodigo and RecEstatus <>0) " +
                    "WHERE ttcReferencia IN ('NC','RCA') AND C.CliID = " + Arguments.Values.CurrentClient.CliID.ToString() + "  " + porcionMultiMoneda + " " +
                    "GROUP BY C.CxcDocumento, C.CxcReferencia, C.CxcSigla, CxcFecha, C.CxcMontoTotal, C.CxcBalance, ttcOrigen, cxcMontoSinItbis " +
                    "HAVING (ROUND(abs(C.CxcBalance), 2) - ROUND(COALESCE(SUM(A.RefValor), 0), 2) <> 0) " +
                    "ORDER BY Dias DESC";


            SqliteManager.GetInstance().Execute(sql);

            try
            {
                SqliteManager.GetInstance().Execute("update RecibosDocumentosTemp set MonCodigo = '" + monCodigo + "'");


            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        public void CambiarMonedaDocumentos(Monedas moneda)
        {
            try
            {
                var query = "select distinct t.MonCodigo as MonCodigo, t.MonTasa as MonTasa from Monedas t " +
                    "inner join RecibosDocumentosTemp r on r.MonCodigo = t.MonCodigo where r.MonCodigo != '" + moneda.MonCodigo + "'";
                //MontoTotal, Balance, Pendiente, MontoSinItbis, CxcNotaCredito
                var monedasDocumentos = SqliteManager.GetInstance().Query<Monedas>(query);


                foreach (var mon in monedasDocumentos)
                {
                    var query2 = "";
                    if (myParametro.GetParRecibosMonedaUnicaConTasaDocumento())
                    {
                        query2 = "select r.MontoTotal,r.Balance,r.Pendiente,r.MontoSinItbis,r.CxcNotaCredito,r.MonCodigo,r.Documento, c.CxcTasa as TasaDocumento from  RecibosDocumentosTemp r " +
                        "Inner Join CuentasxCobrar C on C.CxcDocumento=Documento  where r.MonCodigo = '" + mon.MonCodigo + "'";
                    }
                    else
                    {
                        query2 = "select MontoTotal,Balance,Pendiente,MontoSinItbis,CxcNotaCredito,MonCodigo,Documento from  RecibosDocumentosTemp r where MonCodigo = '" + mon.MonCodigo + "'";
                    }
                    var Documentos = SqliteManager.GetInstance().Query<RecibosDocumentosTemp>(query2);

                    foreach (var doc in Documentos)
                    {
                        var update = new Hash("RecibosDocumentosTemp");
                        update.SaveScriptForServer = false;

                        if (myParametro.GetParRecibosMonedaUnicaConTasaDocumento())
                        {
                            update.Add("MontoTotal", Math.Ceiling(((doc.MontoTotal * (moneda.MonCodigo == "USD" ? 1 : doc.TasaDocumento)) / moneda.MonTasa) * 100) / 100);
                            update.Add("Balance", Math.Ceiling(((doc.Balance * (moneda.MonCodigo == "USD" ? 1 : doc.TasaDocumento)) / moneda.MonTasa) * 100) / 100);
                            update.Add("Pendiente", Math.Ceiling(((doc.Pendiente * (moneda.MonCodigo == "USD" ? 1 : doc.TasaDocumento)) / moneda.MonTasa) * 100) / 100);
                            update.Add("MontoSinItbis", Math.Ceiling(((doc.MontoSinItbis * (moneda.MonCodigo == "USD" ? 1 : doc.TasaDocumento)) / moneda.MonTasa) * 100) / 100);
                            update.Add("CxcNotaCredito", Math.Ceiling(((doc.CxcNotaCredito * (moneda.MonCodigo == "USD" ? 1 : doc.TasaDocumento)) / moneda.MonTasa) * 100) / 100);
                            update.Add("TasaDocumento", doc.TasaDocumento);
                        }
                        else
                        {
                            update.Add("MontoTotal", Math.Ceiling(((doc.MontoTotal * mon.MonTasa) / moneda.MonTasa) * 100) / 100);
                            update.Add("Balance", Math.Ceiling(((doc.Balance * mon.MonTasa) / moneda.MonTasa) * 100) / 100);
                            update.Add("Pendiente", Math.Ceiling(((doc.Pendiente * mon.MonTasa) / moneda.MonTasa) * 100) / 100);
                            update.Add("MontoSinItbis", Math.Ceiling(((doc.MontoSinItbis * mon.MonTasa) / moneda.MonTasa) * 100) / 100);
                            update.Add("CxcNotaCredito", Math.Ceiling(((doc.CxcNotaCredito * mon.MonTasa) / moneda.MonTasa) * 100) / 100);
                        }
                        update.ExecuteUpdate("where MonCodigo = '" + mon.MonCodigo + "' and Documento= '" + doc.Documento + "'");
                    }

                }


                //foreach (var mon in monedasDocumentos)
                //{
                //    var update = "update RecibosDocumentosTemp set MontoTotal = round((MontoTotal * " + mon.MonTasa + ") / " + moneda.MonTasa + ", 2), " +
                //        "Balance = round((Balance * " + mon.MonTasa + ") / " + moneda.MonTasa + ", 2), Pendiente = round((Pendiente * " + mon.MonTasa + ") / " + moneda.MonTasa + ", 2)," +
                //        "MontoSinItbis = round((MontoSinItbis * " + mon.MonTasa + ") / " + moneda.MonTasa + ", 2), CxcNotaCredito = round((CxcNotaCredito * " + mon.MonTasa + ") / " + moneda.MonTasa + ", 2) " +
                //        "where MonCodigo = '" + mon.MonCodigo + "'";

                //    SqliteManager.GetInstance().Execute(update);
                //}

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        // Se cambia a la version 2, ya que en este salian los recibos aplicados, y se requerian que solo salieran cuenta por cobrar actualizada
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
                       "WHERE (JULIANDAY(SUBSTR(DATETIME(DATETIME('NOW', 'localtime'),'" + diferenciaHoraria + " hours'),1,10)) - JULIANDAY(SUBSTR(CxcFecha,1,10))) BETWEEN 0 AND 30 AND CliID = " + Cliid + " " + whereMonCodigo +
                       "UNION ALL " +

                       "SELECT '0' AS Desde, '30' AS Hasta, IFNULL(CAST(RecMontoEfectivo + RecMontoCheque + RecMontoChequef + RecMontoTransferencia + RecMontoDescuento + IFNULL(RecTotalDescuentoDesmonte,0) - RecMontoSobrante as String),0) AS Balance " +
                       "FROM Recibos " +
                       "WHERE (JULIANDAY(SUBSTR(DATETIME(DATETIME('NOW', 'localtime'),'" + diferenciaHoraria + " hours'),1,10)) - JULIANDAY(SUBSTR(RecFecha,1,10))) BETWEEN 0 AND 30 AND CliID = " + Cliid + " " + whereMonCodigo +
                       "UNION ALL " +

                       "SELECT '31' AS Desde, '60' AS Hasta, IFNULL( CAST(SUM(CXCBalance) as numeric),0) AS Balance " +
                       "FROM CuentasxCobrar " +
                       "inner join TiposTransaccionesCxc ttc on trim(ttc.ttcReferencia) = CxcSigla and ttcOrigen = 1 " +
                       "WHERE (JULIANDAY(SUBSTR(DATETIME(DATETIME('NOW', 'localtime'),'" + diferenciaHoraria + " hours'),1,10)) - JULIANDAY(SUBSTR(CxcFecha,1,10))) BETWEEN 31 AND 60 AND CliID = " + Cliid + " " + whereMonCodigo +
                       "UNION ALL " +

                       "SELECT '31' AS Desde, '60' AS Hasta, IFNULL(CAST(RecMontoEfectivo + RecMontoCheque + RecMontoChequef + RecMontoTransferencia + RecMontoDescuento + IFNULL(RecTotalDescuentoDesmonte,0) - RecMontoSobrante as String),0) AS Balance " +
                       "FROM Recibos " +
                       "WHERE (JULIANDAY(SUBSTR(DATETIME(DATETIME('NOW', 'localtime'),'" + diferenciaHoraria + " hours'),1,10)) - JULIANDAY(SUBSTR(RecFecha,1,10))) BETWEEN 31 AND 60 AND CliID = " + Cliid + " " + whereMonCodigo +
                       "UNION ALL " +

                       "SELECT '61' as Desde, '90' as Hasta, IFNULL( CAST(SUM(CXCBalance) as numeric),0) as Balance " +
                       "FROM CuentasxCobrar " +
                       "inner join TiposTransaccionesCxc ttc on trim(ttc.ttcReferencia) = CxcSigla and ttcOrigen = 1 " +
                       "WHERE (JULIANDAY(SUBSTR(DATETIME(DATETIME('NOW', 'localtime'),'" + diferenciaHoraria + " hours'),1,10)) - JULIANDAY(SUBSTR(CxcFecha,1,10))) BETWEEN 61 AND 90 AND CliID = " + Cliid + " " + whereMonCodigo +
                       "UNION ALL " +

                       "SELECT '61' AS Desde, '90' AS Hasta, IFNULL(CAST(RecMontoEfectivo + RecMontoCheque + RecMontoChequef + RecMontoTransferencia + RecMontoDescuento + IFNULL(RecTotalDescuentoDesmonte,0) - RecMontoSobrante as String),0) AS Balance " +
                       "FROM Recibos " +
                       "WHERE (JULIANDAY(SUBSTR(DATETIME(DATETIME('NOW', 'localtime'),'" + diferenciaHoraria + " hours'),1,10)) - JULIANDAY(SUBSTR(RecFecha,1,10))) BETWEEN 61 AND 90 AND CliID = " + Cliid + " " + whereMonCodigo +
                       "UNION ALL " +

                       "SELECT '91' as Desde, '' as Hasta, IFNULL( CAST(SUM(CXCBalance) as numeric),0) as Balance " +
                       "FROM CuentasxCobrar " +
                       "inner join TiposTransaccionesCxc ttc on trim(ttc.ttcReferencia) = CxcSigla and ttcOrigen = 1 " +
                       "WHERE (JULIANDAY(SUBSTR(DATETIME(DATETIME('NOW', 'localtime'),'" + diferenciaHoraria + " hours'),1,10)) - JULIANDAY(SUBSTR(CxcFecha,1,10))) >= 91 AND CliID = " + Cliid + " " + whereMonCodigo +
                       "UNION ALL " +

                       "SELECT '91' AS Desde, '' AS Hasta, IFNULL(CAST(RecMontoEfectivo + RecMontoCheque + RecMontoChequef + RecMontoTransferencia + RecMontoDescuento + IFNULL(RecTotalDescuentoDesmonte,0) - RecMontoSobrante as String),0) AS Balance " +
                       "FROM Recibos " +
                       "WHERE (JULIANDAY(SUBSTR(DATETIME(DATETIME('NOW', 'localtime'),'" + diferenciaHoraria + " hours'),1,10)) - JULIANDAY(SUBSTR(RecFecha,1,10))) >= 91 AND CliID = " + Cliid + whereMonCodigo +
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

        public ClientesCreditoData GetDatosCreditoCliente(int cliid, string monCodigo = null)
        {
            double Balance_Dolares = 0;
            double Balance_Pesos = 0;
            var monedas = new DS_Monedas().GetMonedas("USD");

            if (myParametro.GetParConvertirBalanceADolares())
            {
                Balance_Dolares = GetBalanceTotalByCliid(cliid, "USD", WithChD: !myParametro.GetParNoTomarEnCuentaChequesDiferidos());
                Balance_Pesos = GetBalanceTotalByCliid(cliid, "RD$", WithChD: !myParametro.GetParNoTomarEnCuentaChequesDiferidos());
            }

            return new ClientesCreditoData()
            {
                //Balance = GetBalanceTotalByCliid(cliid, monCodigo),
                Balance = Balance_Dolares != 0 && Balance_Pesos != 0 && monCodigo == null
                ? (Balance_Dolares + (Balance_Pesos / monedas[0].MonTasa))
                : GetBalanceTotalByCliid(cliid, monCodigo, WithChD: !myParametro.GetParNoTomarEnCuentaChequesDiferidos()),
                BalanceSoloChequeDif = GetMontoTotalChequeDiferido(cliid, monCodigo),
                //BalanceSinChequeDif = GetBalanceTotalByCliid(cliid, monCodigo,false),
                LimiteCredito = Arguments.Values.CurrentClient.CliLimiteCredito,
                IndicadorCredito = Arguments.Values.CurrentClient.CliIndicadorCredito,
                MontoVencido = GetCliBalanceVencidobyAreaCtrlCredit(cliid)
            };
        }

        private double GetCliBalanceVencidobyAreaCtrlCredit(int CliID)
        {
            double Result = 0.0;
            try
            {
                var where = "";

                if (myParametro.GetParRecibosPorSector() && Arguments.Values.CurrentSector != null)
                {
                    where = " and AreaCtrlCredit = '" + Arguments.Values.CurrentSector.AreaCtrlCredit + "'";
                }


                var list = SqliteManager.GetInstance().Query<CuentasxCobrar>("select ifnull(ROUND(SUM(CxcBalance),2), 0.0) as CxcBalance from CuentasxCobrar where CliID = " + CliID + " "
                    + "and CxcDias > (julianday('now') - julianday(datetime(CxcFecha))) " + where, new string[] { });

                if (list != null && list.Count > 0)
                {
                    return list[0].CxcBalance;
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            return Result;
        }

        public ClientesCreditoData GetDatosCreditoClienteByFecha(int cliid, string monCodigo = null)
        {
            return new ClientesCreditoData()
            {
                Balance = GetBalanceTotalByCliidyfecha(cliid, monCodigo),
                LimiteCredito = Arguments.Values.CurrentClient.CliLimiteCredito,
                IndicadorCredito = Arguments.Values.CurrentClient.CliIndicadorCredito

            };
        }

        public List<Productos> GetProductosFromCuentasxCobrarDetalle(string cxcReferencia)
        {
            return SqliteManager.GetInstance().Query<Productos>("select ProCodigo, ProDescripcion, CxcCantidad as ProCantidad, CxcPrecio as ProPrecio, CxcItbis as ProItbis, CxcDescuento as ProDescuentoMaximo, UnmCodigo   " +
                "from CuentasxCobrarDetalle cxc " +
                "inner join Productos p on p.ProID = cxc.ProID " +
                "where ltrim(rtrim(upper(cxc.CxcReferencia))) = ? " +
                "Order By CxcPosicion", new string[] { cxcReferencia.Trim().ToUpper() });
        }

        public void InsertProductInTempForDetail(string cxcReferencia, int titId)
        {
            var list = SqliteManager.GetInstance().Query<ProductosTemp>("select " + titId.ToString() + " as TitID, e.ProID as ProID, p.ProCodigo as ProCodigo, " +
                "e.CxcCantidad as Cantidad, e.CxcCantidadDetalle as CantidadDetalle," +
                "case when ifnull(p.ProUnidades, 0) = 0 then 1 else p.ProUnidades end as ProUnidades, " +
                "p.ProDescripcion as Descripcion, p.ProDatos3 as ProDatos3, CxcIndicadorOferta as IndicadorOferta, CxcPrecio as Precio, CxcItbis as Itbis, " +
                "CxcDescuento as Descuento, 0 as DesPorciento, " +
                "p.UnmCodigo as UnmCodigo, p.ProIndicadorDetalle as IndicadorDetalle, ProPrecio3, ProDescripcion2, ProDescripcion3, ProDatos2, " +
                "ProDatos1, ProDescripcion1, CxcLote as Lote, e.CxcPosicion as Posicion from CuentasxCobrarDetalle e " +
                "inner join Productos p on p.ProID = e.ProID " +
               "where ltrim(rtrim(upper(e.CxcReferencia))) = ? ",
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

        public List<Monedas> GetMonedasDeLosDocumentos(int cliId)
        {
            string whereSector = "";

            if (myParametro.GetParRecibosPorSector())
            {
                whereSector = " And cxc.AreaCtrlCredit = '" + (Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.AreaCtrlCredit : "") + "'";
            }

            var list = SqliteManager.GetInstance().Query<Monedas>("select m.MonCodigo, m.MonNombre, m.MonSigla, m.MonTasa from CuentasxCobrar cxc " +
                "inner join Monedas m on trim(upper(m.MonCodigo)) = trim(upper(cxc.MonCodigo))  " +
                "where CliId = ? " + whereSector + " group by m.MonCodigo, m.MonNombre, m.MonSigla, m.MonTasa order by m.MonNombre desc", new string[] { cliId.ToString() });

            return list;
        }

        public List<Monedas> GetMonedasDeLosDocumentosPendientes(int cliId, bool sinRcbFuturista = false)
        {
            var sql = "";


            if (myParametro.GetParRecibosPorSector())
            {
                string crtlAreaCredito = " And C.AreaCtrlCredit = '" + (Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.AreaCtrlCredit : "") + "'";

                //para en el area de control de credit hacerle un subString
                if (myParametro.GetParAreaCrtlCreditoClienteSubString() && Arguments.Values.CurrentSector.AreaCtrlCredit != null && Arguments.Values.CurrentSector.AreaCtrlCredit.Length > 1)
                {
                    crtlAreaCredito = " And SUBSTR(C.AreaCtrlCredit,1,2) = '" + Arguments.Values.CurrentSector.AreaCtrlCredit.Substring(0, 2) + "' ";
                }

                sql = "select result.* from (select Distinct m.MonNombre, m.MonSigla, m.MonCodigo " +
                   "from CuentasxCobrar C " +
                   "inner join Monedas m on trim(upper(m.MonCodigo)) = trim(upper(C.MonCodigo)) " +
                   "left join TiposTransaccionesCxc ttc on trim(ttc.ttcReferencia) = CxcSigla " +
                   "LEFT JOIN(Select CxcReferencia, RecSecuencia, RepCodigo, RecValor, RecDescuento from RecibosAplicacion UNION ALL Select CxcReferencia, RecSecuencia, RepCodigo, RefValor as RecValor, 0 as RecDescuento from RecibosFormaPago) A " +
                   "ON A.CxcReferencia = C.CXCReferencia and a.recsecuencia in (select r.recSecuencia from recibos r where repcodigo = A.repcodigo and RecEstatus <>0) " + " " +
                   (sinRcbFuturista ? " and not exists (select 1 from RecibosFormaPago rfp where rfp.RecSecuencia=a.RecSecuencia and rfp.RepCodigo=a.RepCodigo and rfp.ForId=2 and rfp.RefIndicadorDiferido=1) " : "") + " " +
                   "WHERE CliID = ? " + crtlAreaCredito + " " +
                   "GROUP BY C.CxcDocumento, C.CxcReferencia, C.CxcSigla, m.MonNombre, m.MonSigla " +
                   "HAVING (abs(C.CxcBalance) - ROUND(COALESCE(SUM(A.RecValor + A.RecDescuento), 0), 2) >= 0.01) ) as result  ";
            }
            else
            {
                sql = "select Distinct m.MonNombre, m.MonSigla, m.MonCodigo from CuentasxCobrar C " +
                    "inner join Monedas m on trim(upper(m.MonCodigo)) = trim(upper(C.MonCodigo)) " +
                    "left join TiposTransaccionesCxc ttc on trim(ttc.ttcReferencia) = CxcSigla " +
                    "LEFT JOIN(Select CxcReferencia, RecSecuencia, RepCodigo, RecValor, RecDescuento from RecibosAplicacion UNION ALL Select CxcReferencia, RecSecuencia, RepCodigo, RefValor as RecValor, 0 as RecDescuento from RecibosFormaPago) A " +
                    "ON A.CxcReferencia = C.CXCReferencia and a.recsecuencia in (select r.recSecuencia from recibos r where repcodigo = A.repcodigo and RecEstatus <>0) " + " " +
                    (sinRcbFuturista ? " and not exists (select 1 from RecibosFormaPago rfp where rfp.RecSecuencia=a.RecSecuencia and rfp.RepCodigo=a.RepCodigo and rfp.ForId=2 and rfp.RefIndicadorDiferido=1) " : "") + " " +
                    "WHERE CliID = ? " +
                    "GROUP BY C.CxcDocumento, C.CxcReferencia, C.CxcSigla, m.MonCodigo, m.MonSigla " +
                    "HAVING (abs(C.CxcBalance) - ROUND(COALESCE(SUM(A.RecValor + A.RecDescuento), 0), 2) >= 0.01)  " +
                    "ORDER BY m.MonSigla";
            }

            var list = SqliteManager.GetInstance().Query<Monedas>(sql, new string[] { cliId.ToString(), cliId.ToString() });

            return list;
        }

        public List<CuentasxCobrar> GetFacturasVencidad(int CliID)
        {
            /* string sql = "Select count(*) from (" +
                          "SELECT CAST(replace(strftime('%d-%m-%Y', SUBSTR(CxcFecha,1,10)),' ','') as varchar) as Fecha, C.CxcDocumento, C.CxcReferencia, " +
                                "C.CxcSigla, 0 AS Aplicado, 0 as Descuento, C.CxcMontoTotal, (C.CxcBalance - COALESCE(SUM(A.RecValor + A.RecDescuento),0)) AS CxcBalance, " +
                                "(C.CxcBalance - COALESCE(SUM(A.RecValor + A.RecDescuento ),0)) AS Pendiente, 'Pendiente' as Estado, 0  as Credito, " +
                                "CAST(replace(strftime('%m-%d-%Y', SUBSTR(CxcFecha,1,10)),' ','' ) as varchar) as CxcFechaIngles, ttcOrigen, cxcMontoSinItbis, " +
                                "0 DescPorciento, 0 AutID, strftime('%d-%m-%Y','now') as FechaDescuento, julianday(Cast(strftime('%Y-%m-%d','now') as Varchar)) - julianday(SUBSTR(CxcFecha,1,10)) as Dias, 0 DescuentoFactura, " +
                                "IFNULL(CxcClasificacion, '') AS 'Clasificacion', IFNULL(date(CxcFechaVencimiento), '') AS 'FechaVencimiento'  " +
                           "FROM CuentasxCobrar C " +
                           "INNER JOIN TiposTransaccionesCXC T ON ltrim(rtrim(T.ttcSigla)) = ltrim(rtrim(C.CxcSigla)) " +
                           "LEFT JOIN (Select CxcReferencia, RecSecuencia, RepCodigo, RecValor, RecDescuento from RecibosAplicacion UNION ALL Select CxcReferencia, RecSecuencia, RepCodigo, RefValor as RecValor, 0 as RecDescuento from RecibosFormaPago) A " +
                               "ON A.CxcReferencia = C.CXCReferencia and a.recsecuencia in (select r.recSecuencia from recibos r where repcodigo = A.repcodigo and RecEstatus <>0) " +
                         "WHERE ltrim(rtrim(ttcReferencia)) <> 'NC' AND UPPER(ltrim(rtrim(ttcReferencia))) <> 'PNF' AND CliID = " +CliID+ " " +
                           "GROUP BY C.CxcDocumento, C.CxcReferencia, C.CxcSigla, CxcFecha, C.CxcMontoTotal, C.CxcBalance, ttcOrigen, cxcMontoSinItbis " +
                         "HAVING (C.CxcBalance - ROUND(COALESCE(SUM(A.RecValor + A.RecDescuento), 0), 2) >= 0.01) " +
                         "ORDER BY Dias DESC) as result";*/
            string sql = "select C.cxcNCF as cxcNCF, Cast(strftime('%Y-%m-%d',C.cxcFechaVencimiento) as Varchar),  cast(julianday(Cast(strftime('%Y-%m-%d',C.cxcFechaVencimiento) as Varchar)) - julianday(Cast(strftime('%Y-%m-%d','now','-4 hours') as varchar)) as integer) as CxcDias, "
                    + "C.CxcDocumento, C.CxcReferencia as CxcReferencia, C.CxcSigla, CxcFecha, C.CxcMontoTotal, C.CxcBalance as CxcBalance, ttcOrigen, cxcMontoSinItbis " +
                    "FROM CuentasxCobrar C " +
                    "INNER JOIN TiposTransaccionesCXC T ON ltrim(rtrim(T.ttcSigla)) = ltrim(rtrim(C.CxcSigla)) " +
                    "LEFT JOIN (Select CxcReferencia, RecSecuencia, RepCodigo, RecValor, RecDescuento from RecibosAplicacion UNION ALL Select CxcReferencia, RecSecuencia, RepCodigo, RefValor as RecValor, 0 as RecDescuento from RecibosFormaPago) A " +
                    "ON A.CxcReferencia = C.CXCReferencia  and a.recsecuencia in (select r.recSecuencia from recibos r where repcodigo = A.repcodigo and RecEstatus <>0) " +
                    "WHERE ltrim(rtrim(ttcReferencia)) = 'FAT' AND UPPER(ltrim(rtrim(ttcReferencia))) <> 'PNF' AND CliID = " + CliID + " " +
                    "GROUP BY C.CxcDocumento, C.CxcReferencia, C.CxcSigla, CxcFecha, C.CxcMontoTotal, C.CxcBalance, ttcOrigen, cxcMontoSinItbis ";

            var list = SqliteManager.GetInstance().Query<CuentasxCobrar>(sql, new string[] { });
            return list;
        }

        public string GetCxcFechaVencimiento(int ConID)
        {
            var list = SqliteManager.GetInstance().Query<CondicionesPago>("select ConDiasVencimiento from CondicionesPago where ConId =" + ConID, new string[] { });

            int dias = list[0].ConDiasVencimiento;
            string fechaVencimiento = Functions.AddDaysOnDate(Functions.CurrentDate(), dias);
            return fechaVencimiento;
        }

        public double getTotalAplicadoByReferencia(string cxcReferencia)
        {
            double monto = 0.0;
            string sql = "select SUM(ifnull(RA.RecValor, 0.0)) as RecValor , SUM(ifnull(RA.RecDescuento, 0.0)) as RecDescuento  from RecibosAplicacion RA "
                       + "inner join CuentasxCobrar cxc on  ra.cxcReferencia = cxc.cxcReferencia "
                       + "Where trim(RA.cxcReferencia) = trim('" + cxcReferencia + "')";

            var list = SqliteManager.GetInstance().Query<RecibosAplicacion>(sql);
            foreach (var total in list)
            {
                monto = total.RecValor + total.RecDescuento;
            }

            return monto;
        }

        public int GetCountChequesDevueltos(int CliID)
        {
            var list = SqliteManager.GetInstance().Query<ChequesDevueltos>("select cd.CheNumero as CheNumero from ChequesDevueltos cd " +
                    "LEFT JOIN Bancos b ON b.BanID = cd.BanID " +
                    "WHERE CliID = " + CliID, new string[] { });
            int dias = list.Count;

            return dias;
        }

        public int GetCountChequesDevueltosinNDays(int CliID, DateTime Fecha)
        {
            int cantidad = 0;
            var list = SqliteManager.GetInstance().Query<ChequesDevueltos>("select cd.CheNumero as CheNumero from ChequesDevueltos cd " +
                    "LEFT JOIN Bancos b ON b.BanID = cd.BanID " +
                    "WHERE CliID = " + CliID + " AND (CheFecha BETWEEN '" + Fecha.ToString("yyyy-MM-dd") + "' AND '" + DateTime.Now.ToString("yyyy-MM-dd") + "')", new string[] { });

            //var list = SqliteManager.GetInstance().Query<CuentasxCobrar>("select CXCSigla from CuentasxCobrar cxc " +
            //       "WHERE CliID = " + CliID + " AND (CXCFecha BETWEEN '" + Fecha.ToString("yyyy-MM-dd") + "' AND '" + DateTime.Now.ToString("yyyy-MM-dd") + "') AND RepCodigo ='"+Arguments.CurrentUser.RepCodigo+"' AND CXCSigla = 'CKD'", new string[] { });

            if (list != null)
            {
                cantidad = list.Count;
            }
            else
            {
                return -1;
            }

            return cantidad;
        }

        public string CheckDays()
        {
            if (Arguments.Values.CurrentModule == Modules.PEDIDOS && DS_RepresentantesParametros.GetInstance().GetParAlertaFacturas60dias())
            {
                DateTime Date60daysago = DateTime.Today.AddDays(-60);
                var list = SqliteManager.GetInstance().Query<model.CuentasxCobrar>("SELECT * FROM CuentasxCobrar where CXCReferencia NOT IN " +
                "(SELECT CXCReferencia FROM RecibosAplicacion ra INNER JOIN Recibos r on r.recsecuencia = ra.recsecuencia WHERE r.RecEstatus <> 0) " +
                "AND CliID = " + Arguments.Values.CurrentClient.CliID + " AND CXCSigla = 'FAT' AND RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' AND Cxcfecha < '" + Date60daysago.ToString("yyyy-MM-dd") + "'", new string[] { });

                if (list.Count > 0)
                {
                    return "60";
                }
            }

            if (Arguments.Values.CurrentModule == Modules.PEDIDOS && DS_RepresentantesParametros.GetInstance().GetParAlertaFacturas30dias())
            {
                DateTime Date30daysago = DateTime.Today.AddDays(-30);
                var list = SqliteManager.GetInstance().Query<model.CuentasxCobrar>("SELECT * FROM CuentasxCobrar where CXCReferencia NOT IN " +
                "(SELECT CXCReferencia FROM RecibosAplicacion ra INNER JOIN Recibos r on r.recsecuencia = ra.recsecuencia WHERE r.RecEstatus <> 0) " +
                "AND CliID = " + Arguments.Values.CurrentClient.CliID + " AND CXCSigla = 'FAT' AND RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' AND Cxcfecha < '" + Date30daysago.ToString("yyyy-MM-dd") + "'", new string[] { });

                if (list.Count > 0)
                {
                    return "30";
                }

            }

            if (Arguments.Values.CurrentModule == Modules.PEDIDOS && DS_RepresentantesParametros.GetInstance().GetParAlertaFacturasVencida())
            {
                DateTime DateFacturaVencida = DateTime.Today;
                var list = SqliteManager.GetInstance().Query<model.CuentasxCobrar>("SELECT * FROM CuentasxCobrar where CXCReferencia NOT IN " +
                "(SELECT CXCReferencia FROM RecibosAplicacion ra INNER JOIN Recibos r on r.recsecuencia = ra.recsecuencia WHERE r.RecEstatus <> 0) " +
                "AND CliID = " + Arguments.Values.CurrentClient.CliID + " AND CXCSigla = 'FAT' AND RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' AND cxcFechaVencimiento < '" + DateFacturaVencida.ToString("yyyy-MM-dd") + "'", new string[] { });

                if (list.Count > 0)
                {
                    return "1";
                }
            }

            return "";
        }

        public List<CuentasxCobrarDetalle> GetCuentasxCobrarDetalle(string referencia)
        {
            var usePrecio = myParametro.GetParDevolucionesFacturaPrecioProducto();

            return SqliteManager.GetInstance().Query<CuentasxCobrarDetalle>("select p.ProDescripcion as ProDescripcion, p.UnmCodigo as UnmCodigo, CxcReferencia, CxcPosicion, p.ProID as ProID, CxcCantidad, " +
                "CxcCantidadDetalle, " + (usePrecio ? "CxcPrecio" : "0.00") + " as CxcPrecio, CxcDescuento, CxcItbis, CxcLote, CxcIndicadorOferta from CuentasxCobrarDetalle c " +
                "inner join Productos p on p.ProID = c.ProID " +
                "where c.CxcReferencia = ? /*and ifnull(c.CxcIndicadorOferta, 0) = 0*/ order by c.CxcPosicion", new string[] { referencia });
        }

        public CuentasxCobrar GetCuentaByReferencia(string referencia, string documento, int cxcTipoTransaccion = -1)
        {
            var list = SqliteManager.GetInstance().Query<CuentasxCobrar>("select * from CuentasxCobrar where RepCodigo = ? and " +
                "CxcReferencia = ? and CxcDocumento = ? " + (cxcTipoTransaccion != -1 ? " and CxcTipoTransaccion = " + cxcTipoTransaccion + " " : ""),
                new string[] { Arguments.CurrentUser.RepCodigo, referencia, documento }).FirstOrDefault();

            return list ?? new CuentasxCobrar();
        }

        public int ValidarFacturasSaldadasNoVencidas(string Referencia)
        {
            /* var query = " select 1 from cuentasxcobrar where cliid = "+ cliid + "  and CxcReferencia in ("+ Documentos +" ) "+
                       " and strftime('%Y-%m-%d', SUBSTR('now',1,10)) > strftime('%Y-%m-%d', SUBSTR(cxcFechaVencimiento,1,10))";*/

            var query = "/*select 1 from RecibosDocumentosTemp where Estado = 'Saldo' and strftime('%Y-%m-%d', SUBSTR('now',1,10)) < strftime('%Y-%m-%d', SUBSTR(FechaVencimiento,1,10)) " +
                "Union all*/ " +
                "select 1 from CuentasxCobrar where  cxcreferencia  = '" + Referencia + "' and strftime('%Y-%m-%d', SUBSTR('now', 1, 10)) < strftime('%Y-%m-%d', SUBSTR(cxcFechaVencimiento, 1, 10)) ";
            var list = SqliteManager.GetInstance().Query<RecibosDocumentosTemp>(query, new string[] { });
            if (list != null && list.Count > 0)
            {
                return list.Count;
            }

            return 0;
        }

        public double GetTasaFactura(string Referencia)
        {
            var query = " select max(r.CxcTasa) as CxcTasa from ( " +
                        "select max(ifnull(cxcTasa, 0)) as CxcTasa from CuentasxCobrar c inner join RecibosDocumentosTemp rt on c.CxcReferencia = rt.Referencia " +
                        "where rt.Estado = 'Saldo' /*and strftime('%Y-%m-%d', SUBSTR('now', 1, 10)) < strftime('%Y-%m-%d', SUBSTR(c.cxcFechaVencimiento, 1, 10))*/ " +
                        "UNION ALL " +
                        "select ifnull(cxcTasa, 0) as CxcTasa from CuentasxCobrar where CxcReferencia = '" + Referencia + "' ) as r ";
            var list = SqliteManager.GetInstance().Query<CuentasxCobrar>(query, new string[] { });
            if (list != null && list.Count > 0)
            {
                return list[0].CxcTasa;
            }

            return 0;
        }

        public int GetCountFacturasSaldadas()
        {
            var query = "select 1 from RecibosDocumentosTemp where Estado = 'Saldo' ";
            var list = SqliteManager.GetInstance().Query<RecibosDocumentosTemp>(query, new string[] { });
            if (list != null && list.Count + 1 > 0)
            {
                return list.Count + 1;
            }

            return 0;
        }

        public bool GetForValidFac()
        {

            //var list = SqliteManager.GetInstance().Query<model.CuentasxCobrar>("SELECT * FROM CuentasxCobrar where CXCReferencia NOT IN " +
            //"(SELECT CXCReferencia FROM RecibosAplicacion ra INNER JOIN Recibos r on r.recsecuencia = ra.recsecuencia WHERE r.RecEstatus <> 0) " +
            //"AND CliID = " + Arguments.Values.CurrentClient.CliID + " AND CXCSigla = 'FAT' AND strftime('%Y-%m-%d', SUBSTR('now', 1, 10)) > strftime('%Y-%m-%d', SUBSTR(cxcFechaVencimiento, 1, 10)) ", new string[] { });

            var list = SqliteManager.GetInstance().Query<model.CuentasxCobrar>("select * from CuentasxCobrar C left join TiposTransaccionesCxc ttc on trim(ttc.ttcReferencia) = CxcSigla " +
                                  "LEFT JOIN(Select CxcReferencia, RecSecuencia, RepCodigo, RecValor, RecDescuento from RecibosAplicacion UNION ALL Select CxcReferencia, RecSecuencia, RepCodigo, RefValor as RecValor, 0 as RecDescuento from RecibosFormaPago) A " +
                                  "ON A.CxcReferencia = C.CXCReferencia and a.recsecuencia in (select r.recSecuencia from recibos r where repcodigo = A.repcodigo  and RecEstatus <>0) " + "WHERE  C.CXCSigla = 'FAT' AND CliID = " + Arguments.Values.CurrentClient.CliID + "  AND strftime('%Y-%m-%d', SUBSTR('now', 1, 10)) > strftime('%Y-%m-%d', SUBSTR(cxcFechaVencimiento, 1, 10))  " +
                                  "GROUP BY C.CxcDocumento, C.CxcReferencia, C.CxcSigla, CxcFecha, C.CxcMontoTotal, C.CxcBalance, ttcOrigen, cxcMontoSinItbis " +
                                  "HAVING (abs(C.CxcBalance) - ROUND(COALESCE(SUM(A.RecValor + A.RecDescuento), 0), 2) >= 0.01)");

            return list != null && list.Count > 0;
        }


        public bool GetForValidFacPendientes(int cliid)
        {
            var list = SqliteManager.GetInstance().Query<model.CuentasxCobrar>("SELECT * FROM CuentasxCobrar where CXCReferencia NOT IN " +
            "(SELECT CXCReferencia FROM RecibosAplicacion ra INNER JOIN Recibos r on r.recsecuencia = ra.recsecuencia WHERE r.RecEstatus <> 0" +
            " union all SELECT CXCReferencia FROM RecibosAplicacionConfirmados ra INNER JOIN RecibosConfirmados r on r.recsecuencia = ra.recsecuencia WHERE r.RecEstatus <> 0 ) " +
            "AND CliID = " + Arguments.Values.CurrentClient.CliID + " AND CXCSigla = 'FAT' ", new string[] { });

            return list != null && list.Count > 1;
        }

        public bool CountFacturasPendientes(int cliid)
        {

            var list = SqliteManager.GetInstance().Query<FacturasVencidas>("select CxcDocumento as Factura from CuentasxCobrar " +
                       "where CliID = ?", new string[] { cliid.ToString() });

            return list != null && list.Count > 1;
        }


        public double GetMontoTotalChequeDiferido(int Id, string MonCodigo = null, string fecha = null /*yyyy-MM-dd*/)
        {
            var SectorCondition = "";

            if (myParametro.GetParRecibosPorSector())
            {
                SectorCondition = " and AreaCtrlCredit = '" + Arguments.Values.CurrentSector.AreaCtrlCredit + "'";

                if (myParametro.GetParAreaCrtlCreditoClienteSubString())
                {
                    SectorCondition = " and SUBSTR(AreaCtrlCredit, 1, 2) = '" + Arguments.Values.CurrentSector.AreaCtrlCredit.Substring(0, 2) + "'";
                }
            }

            if (!string.IsNullOrWhiteSpace(MonCodigo))
            {
                SectorCondition += " and trim(upper(MonCodigo)) = trim(upper('" + MonCodigo + "')) ";
            }

            if (!string.IsNullOrEmpty(fecha))
            {
                SectorCondition += " and RecFecha like '" + fecha + "%'";
            }

            var list = SqliteManager.GetInstance().Query<Recibos>(" select SUM(RecTotal) as RecTotal from ( select ifnull(CAST(SUM(RecMontoChequef) AS REAL),0) as RecTotal from Recibos " +
             "where CliID = ? and RecEstatus <> 0 " + SectorCondition +
             " UNION  " +
             "select CAST(SUM(CxcBalance) AS REAL) as RecTotal from CuentasxCobrar where CliID = ? and CxcSigla='RCB' ) r ", new string[] { Id.ToString(), Id.ToString() });


            if (list.Count > 0)
            {
                return list[0].RecTotal;
            }

            return 0;
        }

        public List<CuentasxCobrarDetalle> GetDetalleBySecuenciaBultosYUnidades(string CxcReferencia)
        {
            var sql = "select sum(Bultos) CxcCantidad, sum(Unidades) CxcCantidadDetalle from " +
                "(select p.ProUnidades,  sum(CxcCantidad) EntCantidad,  cast( " +
                "(sum(CxcCantidad) / (case when p.ProUnidades > 0 then p.ProUnidades else 1.0 end)) as int) Bultos, " +
                "(sum(CxcCantidad) - ((cast((sum(CxcCantidad) / (case when p.ProUnidades > 0 then p.ProUnidades else 1.0 end)) as int) ) " +
                "* (case when p.ProUnidades > 0 then p.ProUnidades else 1.0 end))) Unidades  " +
                "from  CuentasxCobrarDetalle e inner join productos p on p.proid = e.ProID WHERE e.CxcReferencia = '" + CxcReferencia +
                "'  GROUP BY  p.ProUnidades " +
                ") tabla";

            return SqliteManager.GetInstance().Query<CuentasxCobrarDetalle>(sql);
        }

        public string GetCxCDocumentoFacturafromAplicacion(string NCReferencia)
        { 
        var query = "Select cxc.cxcDocumento as CxcDocumento from CuentasXCobrar cxc "
                         + "inner join RecibosFormaPago rfp on trim(rfp.cxcReferencia) = trim(cxc.cxcReferencia)  "
                         + "where trim(rfp.RefNumeroAutorizacion) ='" + NCReferencia + "'";

        var list = SqliteManager.GetInstance().Query<CuentasxCobrar>(query, new string[] { });
            if (list != null && list.Count > 0)
            {
                return list[0].CxcDocumento;
            }

            return "";
        }
    }
}
