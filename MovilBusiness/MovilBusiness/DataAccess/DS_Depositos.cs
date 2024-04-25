using MovilBusiness.Configuration;
using MovilBusiness.model;
using MovilBusiness.model.Internal.Structs.Args;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MovilBusiness.DataAccess
{
    public class DS_Depositos
    {
        

        public List<RecibosFormaPago> GetRecibosMontoForDeposito(string tipo, string sector = null, bool isSociedad = false, string monCodigo = null, int depsecuencia = 0)
        {
            var whereMoneda = "";
           
            if (!string.IsNullOrWhiteSpace(monCodigo))
            {
                whereMoneda = " and trim(upper(r.MonCodigo)) = trim(upper('"+monCodigo+"')) ";
            }

            if(depsecuencia > 0)
            {
                tipo = "3";
            }

            var recEstatusRecibos = " RecEstatus not in (0,3) ";
            var recEstatusRecibosConfirmados = " RecEstatus not in (0,1,3) ";
            if (DS_RepresentantesParametros.GetInstance().GetParDepositosConRecibosAnulados())
            {
                recEstatusRecibos = " RecEstatus  not in (3) ";
                recEstatusRecibosConfirmados = " RecEstatus not in (1,3) ";
            }


            string sql = "select ForID, RefIndicadorDiferido, Sum(RefValor) as RefValor, RecSecuencia, RefFecha from ("; ;
            DateTime dt = DateTime.Now.Date;
            var dt1 = String.Format("{0:s}", dt);
            switch (tipo)
            {
                case "1":
                    sql += "select ForID, RefIndicadorDiferido, Sum(/*RecPrima*/RefValor) as RefValor, RecSecuencia, RefFecha from RecibosFormaPago r " +
                       "where ((RefIndicadorDiferido = 1 AND (Cast(replace(Date(RefFecha), '-', '') as INTEGER) <= cast(replace('"+ dt1+"', '-', '') as integer)) or " +
                       "RefIndicadorDiferido <> 1) AND RecSecuencia in (select RecSecuencia from Recibos where RepCodigo = r.RepCodigo "+ whereMoneda + " and "+ recEstatusRecibos + "  " + (sector != null ? (isSociedad? " and trim(substr(AreaCtrlCredit,1,2)) = '"+sector.Trim()+"' " : " and ltrim(rtrim(SecCodigo)) = '" + sector.Trim() + "'") : "") + " and (DepSecuencia = 0 or DepSecuencia is null))" +
                       "and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "') group by ForID, RefIndicadorDiferido, RecSecuencia " +
                       "UNION " +
                       "select ForID, RefIndicadorDiferido, Sum(/*RecPrima*/RefValor) as RefValor, r.RecSecuencia, RefFecha from RecibosFormaPagoConfirmados r " +
                       "inner join RecibosConfirmados rc on rc.RecSecuencia = r.RecSecuencia and rc.RepCodigo =r.RepCodigo "+
                       "where ((RefIndicadorDiferido = 1 AND cast(replace(Date(RefFecha), '-', '') as integer) <= cast(replace('" + dt1 + "', '-', '') as integer)) or RefIndicadorDiferido <> 1) " +
                       //"where ((RefIndicadorDiferido = 1 AND cast(replace(Date(RefFecha), '-', '') as integer) <= cast(replace(Date('now'), '-', '') as integer)) or RefIndicadorDiferido <> 1) and RecSecuencia in " +
                       //"(Select RecSecuencia from RecibosConfirmados where RepCodigo = r.RepCodigo "+ whereMoneda + " and RecEstatus  not in(0,3)  " + (sector != null ? (isSociedad? " and trim(substr(AreaCtrlCredit,1,2)) = '"+sector.Trim()+"' " : " and ltrim(rtrim(SecCodigo)) = '" + sector + "'") : "") + " and (DepSecuencia = 0 or DepSecuencia is null)) " +
                       ""+ whereMoneda + " and " + recEstatusRecibosConfirmados + " " + (sector != null ? (isSociedad ? " and trim(substr(AreaCtrlCredit,1,2)) = '" + sector.Trim() + "' " : " and ltrim(rtrim(SecCodigo)) = '" + sector + "'") : "") + " and (DepSecuencia = 0 or DepSecuencia is null) "+
                       "and Ltrim(rtrim(r.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' group by ForID, RefIndicadorDiferido, r.Recsecuencia";
                    break;
                case "2":
                    sql += "select ForID, RefIndicadorDiferido, Sum(/*RecPrima*/RefValor) as RefValor, RecSecuencia, RefFecha from RecibosFormaPago r " +
                        "where RecSecuencia in (select RecSecuencia from Recibos where RepCodigo = r.RepCodigo "+ whereMoneda + " and " + recEstatusRecibos + " " + (sector != null ? (isSociedad ? " and trim(substr(AreaCtrlCredit,1,2)) = '"+sector.Trim()+"' " : " and ltrim(rtrim(SecCodigo)) = '" + sector.Trim() + "'" ): "") + " and (DepSecuencia = 0 or DepSecuencia is null)) " +
                        "and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' group by ForID, RefIndicadorDiferido, RecSecuencia " +
                        "UNION " +
                        "select ForID, RefIndicadorDiferido, Sum(/*RecPrima*/RefValor) as RefValor, r.RecSecuencia, RefFecha from RecibosFormaPagoConfirmados r " +
                       "inner join RecibosConfirmados rc on rc.RecSecuencia = r.RecSecuencia and rc.RepCodigo =r.RepCodigo " +
                       "where ((RefIndicadorDiferido = 1 AND cast(replace(Date(RefFecha), '-', '') as integer) <= cast(replace(Date('now'), '-', '') as integer)) or RefIndicadorDiferido <> 1) " +
                       //"where ((RefIndicadorDiferido = 1 AND cast(replace(Date(RefFecha), '-', '') as integer) <= cast(replace(Date('now'), '-', '') as integer)) or RefIndicadorDiferido <> 1) and RecSecuencia in " +
                       //"(Select RecSecuencia from RecibosConfirmados where RepCodigo = r.RepCodigo "+ whereMoneda + " and RecEstatus  not in(0,3)  " + (sector != null ? (isSociedad? " and trim(substr(AreaCtrlCredit,1,2)) = '"+sector.Trim()+"' " : " and ltrim(rtrim(SecCodigo)) = '" + sector + "'") : "") + " and (DepSecuencia = 0 or DepSecuencia is null)) " +
                       "" + whereMoneda + " and " + recEstatusRecibosConfirmados + "  " + (sector != null ? (isSociedad ? " and trim(substr(AreaCtrlCredit,1,2)) = '" + sector.Trim() + "' " : " and ltrim(rtrim(SecCodigo)) = '" + sector + "'") : "") + " and (DepSecuencia = 0 or DepSecuencia is null) " +
                       "and Ltrim(rtrim(r.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' group by ForID, RefIndicadorDiferido, r.Recsecuencia";

                    //"select ForID, RefIndicadorDiferido, Sum(RecPrima) as RefValor, RecSecuencia from RecibosFormaPagoConfirmados r where " +
                      //  "RecSecuencia in (select RecSecuencia from RecibosConfirmados where RepCodigo = r.RepCodigo "+ whereMoneda + " and RecEstatus not in(0,3) " + (sector != null ? (isSociedad ? " and trim(substr(AreaCtrlCredit,1,2)) = '"+sector.Trim()+"' " : " and ltrim(rtrim(SecCodigo)) = '" + sector + "'" ) : "") + " and (DepSecuencia = 0 or DepSecuencia is null))" +
                      //  "and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' group by ForID, RefIndicadorDiferido, RecSecuencia";
                    break;
                case "3":
                    sql += "select ForID, RefIndicadorDiferido, Sum(/*RecPrima*/RefValor) as RefValor, RecSecuencia, RefFecha from RecibosFormaPago r " +
                        "where RecSecuencia in (select RecSecuencia from Recibos where RepCodigo = r.RepCodigo "+ whereMoneda + " and " + recEstatusRecibos + " " + (sector != null ? (isSociedad ? " and trim(substr(AreaCtrlCredit,1,2)) = '"+sector.Trim()+"' " : " and ltrim(rtrim(SecCodigo)) = '" + sector.Trim() + "'" ): "") + " and DepSecuencia = "+ depsecuencia+") " +
                        "and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' group by ForID, RefIndicadorDiferido, RecSecuencia " +
                        "UNION " +
                        "select ForID, RefIndicadorDiferido, Sum(/*RecPrima*/RefValor) as RefValor, r.RecSecuencia, RefFecha from RecibosFormaPagoConfirmados r " +
                       "inner join RecibosConfirmados rc on rc.RecSecuencia = r.RecSecuencia and rc.RepCodigo =r.RepCodigo " +
                       "where ((RefIndicadorDiferido = 1 AND cast(replace(Date(RefFecha), '-', '') as integer) <= cast(replace(Date('now'), '-', '') as integer)) or RefIndicadorDiferido <> 1) " +
                       "" + whereMoneda + " and " + recEstatusRecibosConfirmados + "  " + (sector != null ? (isSociedad ? " and trim(substr(AreaCtrlCredit,1,2)) = '" + sector.Trim() + "' " : " and ltrim(rtrim(SecCodigo)) = '" + sector + "'") : "") + " and DepSecuencia = " + depsecuencia + " " +
                       "and Ltrim(rtrim(r.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' group by ForID, RefIndicadorDiferido, r.Recsecuencia";
                    break;
            }

            sql += ") tabla group by ForID, RefIndicadorDiferido";


            return SqliteManager.GetInstance().Query<RecibosFormaPago>(sql, new string[] { });
        }

        public List<RecibosFormaPago> GetRecibosOrdenPagoForDeposito(string tipo, string sector = null, bool isSociedad = false, string monCodigo = null, int depsecuencia = 0)
        {
            var whereMoneda = "";

            if (!string.IsNullOrWhiteSpace(monCodigo))
            {
                whereMoneda = " and trim(upper(r.MonCodigo)) = trim(upper('" + monCodigo + "')) ";
            }

            string sql = "select ForID, RefIndicadorDiferido, Sum(RefValor) as RefValor, RecSecuencia from ("; 

            sql += "select ForID, RefIndicadorDiferido, Sum(RecPrima) as RefValor, RecSecuencia from RecibosFormaPago r " +
            "where r.RecSecuencia in (select RecSecuencia from Recibos where RepCodigo = r.RepCodigo " + whereMoneda + " and ForID = 18 " + (sector != null ? (isSociedad ? " and trim(substr(AreaCtrlCredit,1,2)) = '" + sector.Trim() + "' " : " and ltrim(rtrim(SecCodigo)) = '" + sector.Trim() + "'") : "") + " and DepSecuencia = "+depsecuencia+" ) " +
            "and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' group by ForID, RefIndicadorDiferido, r.RecSecuencia " +
            "UNION " +
            "select ForID, RefIndicadorDiferido, Sum(RecPrima) as RefValor, r.RecSecuencia from RecibosFormaPagoConfirmados r " +
            "Inner join RecibosConfirmados rc on rc.RepCodigo = r.RepCodigo " + whereMoneda + " and ForID = 18 " + (sector != null ? (isSociedad ? " and trim(substr(AreaCtrlCredit,1,2)) = '" + sector.Trim() + "' " : " and ltrim(rtrim(SecCodigo)) = '" + sector + "'") : "") + " and rc.DepSecuencia = " + depsecuencia + " and rc.RecSecuencia = r.RecSecuencia " +
            "where ltrim(rtrim(r.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' group by ForID, RefIndicadorDiferido, r.RecSecuencia";
               
            sql += ") tabla group by ForID, RefIndicadorDiferido";

            return SqliteManager.GetInstance().Query<RecibosFormaPago>(sql, new string[] { });
        }

        public List<RecibosFormaPago> GetRecibosOrdenPago(string tipo, string sector = null, bool isSociedad = false, string monCodigo = null)
        {
            var whereMoneda = "";

            if (!string.IsNullOrWhiteSpace(monCodigo))
            {
                whereMoneda = " and trim(upper(MonCodigo)) = trim(upper('" + monCodigo + "')) ";
            }

            string sql = "select ForID, RefIndicadorDiferido, Sum(RefValor) as RefValor, RecSecuencia from ("; ;

                   sql += "select ForID, RefIndicadorDiferido, Sum(RecPrima) as RefValor, RecSecuencia from RecibosFormaPago r where " +
                       "RecSecuencia in (select RecSecuencia from Recibos where RepCodigo = r.RepCodigo " + whereMoneda + " and RecEstatus = 3  " + (sector != null ? (isSociedad ? " and trim(substr(AreaCtrlCredit,1,2)) = '" + sector.Trim() + "' " : " and ltrim(rtrim(SecCodigo)) = '" + sector.Trim() + "'") : "") + " and (DepSecuencia = 0 or DepSecuencia is null)" +
                       "and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "') group by ForID, RefIndicadorDiferido, RecSecuencia";                      
                   sql += ") tabla group by ForID, RefIndicadorDiferido";

            return SqliteManager.GetInstance().Query<RecibosFormaPago>(sql, new string[] { });
        }

        public List<Recibos> GetRecibosParaDepositar(string tipo, string sector = null, bool isSociedad = false, string monCodigo = null, bool includeOrdenPago = false, string tiposrecibos = null)
        {
            string sql = "";

            var whereMonCodigo = "";

            DateTime dt = DateTime.Now.Date;
            var dt1 = String.Format("{0:s}", dt);

            if (!string.IsNullOrWhiteSpace(monCodigo))
            {
                whereMonCodigo = " trim(upper(r.MonCodigo)) = trim(upper('"+monCodigo+"')) ";
            }
            if(tiposrecibos == "")
            {
                tiposrecibos = null;
            }
            if (tiposrecibos != null)
            {
                sql = "Select DISTINCT r.RecSecuencia as RecSecuencia, r.RecTipo as RecTipo, 0 as confirmado, rc.rowguid " +
                "FROM RecibosFormaPago r " +
                "INNER JOIN Recibos rc on rc.RecSecuencia= r.RecSecuencia and rc.RepCodigo= r.RepCodigo and rc.RecTipo=r.RecTipo and (rc.DepSecuencia = 0 or rc.DepSecuencia is null)" +
                "WHERE " +
                " " + whereMonCodigo + (sector != null ? (isSociedad ? " and trim(substr(AreaCtrlCredit,1,2)) = '" + sector.Trim() + "' " : " and ltrim(rtrim(SecCodigo)) = '" + sector.Trim() + "'") : "") + " " + 
                (tiposrecibos.Contains("E") ? " and r.ForID = 1 ":"") + (tiposrecibos.Contains("CF") ? " or (r.ForID = 2 and RefIndicadorDiferido = 1) " : "") + 
                (tiposrecibos.Contains("CH") ? " or (r.ForID = 2 and RefIndicadorDiferido = 0) " : "") + (tiposrecibos.Contains("TF") ? " or r.ForID = 4 ": "") + (tiposrecibos.Contains("R") ? " or r.ForID = 5 " : "") + (tiposrecibos.Contains("TC") ? " or r.ForID = 6 " : "") + 
                "AND r.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' " +
                "UNION  " +
                "Select DISTINCT r.RecSecuencia as RecSecuencia, r.RecTipo as RecTipo, 1 as confirmado, rc.rowguid from RecibosFormaPagoConfirmados r " +
                "INNER JOIN RecibosConfirmados rc on rc.RecSecuencia= r.RecSecuencia and rc.RepCodigo= r.RepCodigo and rc.RecTipo=r.RecTipo and (rc.DepSecuencia = 0 or rc.DepSecuencia is null) " +
                " WHERE " +
                " " + whereMonCodigo + (sector != null ? (isSociedad ? " and trim(substr(AreaCtrlCredit,1,2)) = '" + sector.Trim() + "' " : " and trim(SecCodigo) = '" + sector.Trim() + "' ") : "") + " and r.ForID = 1 " +
                "AND r.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "'";
            }
            else
            {
                switch (tipo)
                {
                    case "1":
                        sql = "Select DISTINCT r.RecSecuencia as RecSecuencia, r.RecTipo, 0 as confirmado, rc.rowguid, r.RefFecha as RefFecha " +
                            "FROM RecibosFormaPago r " +
                            "INNER JOIN Recibos rc on rc.RecSecuencia= r.RecSecuencia and rc.RepCodigo= r.RepCodigo and rc.RecTipo=r.RecTipo " +
                            "WHERE ((r.RefIndicadorDiferido = 1 AND (CAST(replace(DATE(r.RefFecha), '-', '') AS INTEGER) <= CAST(replace('"+ dt1+"', '-', '') AS INTEGER)) ) OR r.RefIndicadorDiferido<>1) AND r.RecSecuencia In " +
                            "       (SELECT RecSecuencia FROM Recibos WHERE " +
                            "   RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' AND (DepSecuencia = 0 or DepSecuencia is null) " + ( string.IsNullOrEmpty(whereMonCodigo)? " " : "and" + whereMonCodigo) + (sector != null ? (isSociedad ? " and trim(substr(AreaCtrlCredit,1,2)) = '" + sector.Trim() + "' " : " and ltrim(rtrim(SecCodigo)) = '" + sector.Trim() + "'") : "") + ") " + (includeOrdenPago ? "and r.ForID <> 3 " : "and r.ForID <> 3 and r.ForID <>18 ") +
                            "AND r.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' " +
                            "UNION  " +
                            "Select DISTINCT r.RecSecuencia as RecSecuencia, r.RecTipo as RecTipo, 1 as confirmado, rc.rowguid,  r.RefFecha as RefFecha  from RecibosFormaPagoConfirmados r " +
                            "INNER JOIN RecibosConfirmados rc on rc.RecSecuencia= r.RecSecuencia and rc.RepCodigo= r.RepCodigo and rc.RecTipo=r.RecTipo and (rc.DepSecuencia = 0 or rc.DepSecuencia is null) " +
                            " WHERE ((r.RefIndicadorDiferido = 1 AND (CAST(replace(DATE(r.RefFecha), '-', '') AS INTEGER) <= CAST(replace('" + dt1 + "', '-', '') AS INTEGER)) ) OR r.RefIndicadorDiferido<>1) " +
                             (string.IsNullOrWhiteSpace(whereMonCodigo) ? " " : " AND " + whereMonCodigo) + (sector != null ? (isSociedad ? " and trim(substr(AreaCtrlCredit,1,2)) = '" + sector.Trim() + "' " : " and trim(SecCodigo) = '" + sector.Trim() + "' ") : "") + " " + (includeOrdenPago ? "and r.ForID <> 3 " : "and r.ForID <> 3 and r.ForID <>18 ") +
                            "AND r.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "'";
                        break;
                    case "2":
                        sql = "SELECT DISTINCT r.RecSecuencia as RecSecuencia, r.RecTipo as RecTipo, 0 as confirmado, rc.rowguid " +
                            "FROM RecibosFormaPago r " +
                            "INNER JOIN Recibos rc on rc.RecSecuencia= r.RecSecuencia and rc.RepCodigo= r.RepCodigo and rc.RecTipo=r.RecTipo " +
                            "WHERE r.RecSecuencia In " +
                            "(Select RecSecuencia FROM Recibos where  " +
                            "RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' AND (DepSecuencia = 0 or DepSecuencia is null) " + (string.IsNullOrEmpty(whereMonCodigo) ? " " : "and" + whereMonCodigo) + (sector != null ? (isSociedad ? " and trim(substr(AreaCtrlCredit,1,2)) = '" + sector.Trim() + "' " : " and ltrim(rtrim(SecCodigo)) = '" + sector.Trim() + "'") : "") + ") " + (includeOrdenPago ? "and r.ForID <> 3 " : "and r.ForID <> 3 and r.ForID <>18 ") +
                            //(myParametro.getParRecFpUno() ? " AND ForID IN (" + Depositos.formaPago + ") " : " ") +
                            "AND r.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' " +
                            "UNION " +
                            "SELECT DISTINCT r.RecSecuencia as RecSecuencia, r.RecTipo as RecTipo, 1 as confirmado, rc.rowguid " +
                            "FROM RecibosFormaPagoConfirmados r " +
                            "INNER JOIN RecibosConfirmados rc on rc.RecSecuencia= r.RecSecuencia and rc.RepCodigo= r.RepCodigo and rc.RecTipo=r.RecTipo and (rc.DepSecuencia = 0 or rc.DepSecuencia is null) " +
                            //"WHERE " + whereMonCodigo + (sector != null ? (isSociedad ? " and trim(substr(AreaCtrlCredit,1,2)) = '" + sector.Trim() + "' " : " and trim(SecCodigo) = '" + sector.Trim() + "' ") : "") + " " + (includeOrdenPago ? "and r.ForID <> 3 " : "and r.ForID <> 3 and r.ForID <>18 ") +
                              "WHERE " + (string.IsNullOrWhiteSpace(whereMonCodigo) ? "" : whereMonCodigo + " AND ") + (sector != null ? (isSociedad ? " trim(substr(AreaCtrlCredit,1,2)) = '" + sector.Trim() + "' " : " trim(SecCodigo) = '" + sector.Trim() + "' ") : "") + " " + (sector != null ? " AND " : " ") + (includeOrdenPago ? " r.ForID <> 3 " : " r.ForID <> 3 and r.ForID <>18 ") +
                            // (myParametro.getParRecFpUno() ? " AND ForID IN (" + Depositos.formaPago + ") " : " ") +
                            "AND r.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' ";
                        break;
                }
            }

            return SqliteManager.GetInstance().Query<Recibos>(sql, new string[] { });
        }

        public List<Recibos> GetRecibosParaDepositarForVerDetalle(string tipo, string sector = null, bool isSociedad = false, string monCodigo = null, bool includeOrdenPago = false, string tiposrecibos = null, int depsecuencia = 0)
        {
            string sql = "";

            var whereMonCodigo = "";

            DateTime dt = DateTime.Now.Date;
            var dt1 = String.Format("{0:s}", dt);

            if (!string.IsNullOrWhiteSpace(monCodigo))
            {
                whereMonCodigo = " and trim(upper(r.MonCodigo)) = trim(upper('"+monCodigo+"')) ";
            }
            if(tiposrecibos == "")
            {
                tiposrecibos = null;
            }
            if (tiposrecibos != null)
            {
                sql = "Select DISTINCT r.RecSecuencia as RecSecuencia, r.RecTipo as RecTipo, 0 as confirmado, rc.rowguid " +
                "FROM RecibosFormaPago r " +
                "INNER JOIN Recibos rc on rc.RecSecuencia= r.RecSecuencia and rc.RepCodigo= r.RepCodigo and rc.RecTipo=r.RecTipo " +
                "WHERE r.RecSecuencia In " +
                "       (SELECT RecSecuencia FROM Recibos WHERE " +
                "        DepSecuencia = "+ depsecuencia + " " + whereMonCodigo + (sector != null ? (isSociedad ? " and trim(substr(AreaCtrlCredit,1,2)) = '" + sector.Trim() + "' " : " and ltrim(rtrim(SecCodigo)) = '" + sector.Trim() + "'") : "") + ") " + 
                (tiposrecibos.Contains("E") ? " and r.ForID = 1 ":"") + (tiposrecibos.Contains("CF") ? " or (r.ForID = 2 and RefIndicadorDiferido = 1) " : "") + 
                (tiposrecibos.Contains("CH") ? " or (r.ForID = 2 and RefIndicadorDiferido = 0) " : "") + (tiposrecibos.Contains("TF") ? " or r.ForID = 4 ": "") + (tiposrecibos.Contains("R") ? " or r.ForID = 5 " : "") + (tiposrecibos.Contains("TC") ? " or r.ForID = 6 " : "") + 
                "AND r.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' " +
                "UNION  " +
                "Select DISTINCT r.RecSecuencia as RecSecuencia, r.RecTipo as RecTipo, 1 as confirmado, rc.rowguid from RecibosFormaPagoConfirmados r " +
                "INNER JOIN RecibosConfirmados rc on rc.RecSecuencia= r.RecSecuencia and rc.RepCodigo= r.RepCodigo and rc.RecTipo=r.RecTipo " +
                " WHERE r.RecSecuencia In " +
                "       (SELECT RecSecuencia FROM RecibosConfirmados WHERE DepSecuencia = " +depsecuencia+ " " + whereMonCodigo + (sector != null ? (isSociedad ? " and trim(substr(AreaCtrlCredit,1,2)) = '" + sector.Trim() + "' " : " and trim(SecCodigo) = '" + sector.Trim() + "' ") : "") + ") and r.ForID = 1 " +
                "AND r.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "'";
            }
            else
            {
                switch (tipo)
                {
                    case "1":
                        sql = "Select DISTINCT r.RecSecuencia as RecSecuencia, r.RecTipo, 0 as confirmado, rc.rowguid, r.RefFecha as RefFecha " +
                            "FROM RecibosFormaPago r " +
                            "INNER JOIN Recibos rc on rc.RecSecuencia= r.RecSecuencia and rc.RepCodigo= r.RepCodigo and rc.RecTipo=r.RecTipo " +
                            "WHERE ((r.RefIndicadorDiferido = 1 AND (CAST(replace(DATE(r.RefFecha), '-', '') AS INTEGER) <= CAST(replace('"+ dt1+"', '-', '') AS INTEGER)) ) OR r.RefIndicadorDiferido<>1) AND r.RecSecuencia In " +
                            "       (SELECT RecSecuencia FROM Recibos WHERE " +
                            "        DepSecuencia = " + depsecuencia + " " + whereMonCodigo + (sector != null ? (isSociedad ? " and trim(substr(AreaCtrlCredit,1,2)) = '" + sector.Trim() + "' " : " and ltrim(rtrim(SecCodigo)) = '" + sector.Trim() + "'") : "") + ") " + (includeOrdenPago ? "and r.ForID <> 3 " : "and r.ForID <> 3 and r.ForID <>18 ") +
                            "AND r.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' " +
                            "UNION  " +
                            "Select DISTINCT r.RecSecuencia as RecSecuencia, r.RecTipo as RecTipo, 1 as confirmado, rc.rowguid,  r.RefFecha as RefFecha  from RecibosFormaPagoConfirmados r " +
                            "INNER JOIN RecibosConfirmados rc on rc.RecSecuencia= r.RecSecuencia and rc.RepCodigo= r.RepCodigo and rc.RecTipo=r.RecTipo " +
                            " WHERE ((r.RefIndicadorDiferido = 1 AND (CAST(replace(DATE(r.RefFecha), '-', '') AS INTEGER) <= CAST(replace('" + dt1 + "', '-', '') AS INTEGER)) ) OR r.RefIndicadorDiferido<>1)  AND r.RecSecuencia In " +
                            "       (SELECT RecSecuencia FROM RecibosConfirmados WHERE DepSecuencia = " + depsecuencia + " " + whereMonCodigo + (sector != null ? (isSociedad ? " and trim(substr(AreaCtrlCredit,1,2)) = '" + sector.Trim() + "' " : " and trim(SecCodigo) = '" + sector.Trim() + "' ") : "") + ") " + (includeOrdenPago ? "and r.ForID <> 3 " : "and r.ForID <> 3 and r.ForID <>18 ") +
                            "AND r.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "'";
                        break;
                    case "2":
                        sql = "SELECT DISTINCT r.RecSecuencia as RecSecuencia, r.RecTipo as RecTipo, 0 as confirmado, rc.rowguid " +
                            "FROM RecibosFormaPago r " +
                            "INNER JOIN Recibos rc on rc.RecSecuencia= r.RecSecuencia and rc.RepCodigo= r.RepCodigo and rc.RecTipo=r.RecTipo " +
                            "WHERE r.RecSecuencia In " +
                            "(Select RecSecuencia FROM Recibos where  " +
                            "  DepSecuencia = " + depsecuencia + " " + whereMonCodigo + (sector != null ? (isSociedad ? " and trim(substr(AreaCtrlCredit,1,2)) = '" + sector.Trim() + "' " : " and ltrim(rtrim(SecCodigo)) = '" + sector.Trim() + "'") : "") + ") " + (includeOrdenPago ? "and r.ForID <> 3 " : "and r.ForID <> 3 and r.ForID <>18 ") +
                            //(myParametro.getParRecFpUno() ? " AND ForID IN (" + Depositos.formaPago + ") " : " ") +
                            "AND r.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' " +
                            "UNION " +
                            "SELECT DISTINCT r.RecSecuencia as RecSecuencia, r.RecTipo as RecTipo, 1 as confirmado, rc.rowguid " +
                            "FROM RecibosFormaPagoConfirmados r " +
                            "INNER JOIN RecibosConfirmados rc on rc.RecSecuencia= r.RecSecuencia and rc.RepCodigo= r.RepCodigo and rc.RecTipo=r.RecTipo " +
                            "WHERE r.RecSecuencia In (Select RecSecuencia FROM RecibosConfirmados where DepSecuencia = " + depsecuencia + " " + whereMonCodigo + (sector != null ? (isSociedad ? " and trim(substr(AreaCtrlCredit,1,2)) = '" + sector.Trim() + "' " : " and trim(SecCodigo) = '" + sector.Trim() + "' ") : "") + ") " + (includeOrdenPago ? "and r.ForID <> 3 " : "and r.ForID <> 3 and r.ForID <>18 ") +
                            // (myParametro.getParRecFpUno() ? " AND ForID IN (" + Depositos.formaPago + ") " : " ") +
                            "AND r.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' ";
                        break;
                }
            }

            return SqliteManager.GetInstance().Query<Recibos>(sql, new string[] { });
        }

        public bool HayRecibosParaDepositar()
        {
            var recEstatusRecibos = " r.RecEstatus not in (0,3) ";
            if (DS_RepresentantesParametros.GetInstance().GetParDepositosConRecibosAnulados())
            {
                recEstatusRecibos = " r.RecEstatus  not in (3) ";
            }

            var list = SqliteManager.GetInstance().Query<Recibos>("select r.RecSecuencia as RecSecuencia from Recibos r where exists(select 1 from RecibosFormaPago where RepCodigo = r.RepCodigo and RecSecuencia = r.RecSecuencia) and ifnull(r.DepSecuencia, 0) = 0 and ltrim(rtrim(r.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and " + recEstatusRecibos + " " +
                "union select r.RecSecuencia as RecSecuencia from RecibosConfirmados r where exists(select 1 from RecibosFormaPagoConfirmados where RepCodigo = r.RepCodigo and RecSecuencia = r.RecSecuencia limit 1) and ifnull(DepSecuencia, 0) = 0 and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' limit 1", new string[] { });

            return list != null && list.Count > 0;
        }
        public bool HayRecibosParaDepositarForCuadres()
        {
            var recEstatusRecibos = " r.RecEstatus not in (0,3) ";
            if (DS_RepresentantesParametros.GetInstance().GetParDepositosConRecibosAnulados())
            {
                recEstatusRecibos = " r.RecEstatus not in (3) ";
            }

            var list = SqliteManager.GetInstance().Query<Recibos>("select r.RecSecuencia as RecSecuencia from Recibos r where exists(select 1 from RecibosFormaPago where RepCodigo = r.RepCodigo and RecSecuencia = r.RecSecuencia and strftime('%m/%d/%Y', RecFecha) <= strftime('%m/%d/%Y', datetime('now')) and ifnull(r.DepSecuencia, 0) = 0 and ltrim(rtrim(r.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and " + recEstatusRecibos + " ) " +
                "union select r.RecSecuencia as RecSecuencia from RecibosConfirmados r where exists(select 1 from RecibosFormaPagoConfirmados where RepCodigo = r.RepCodigo and RecSecuencia = r.RecSecuencia  and strftime('%m/%d/%Y', RefFecha) <= strftime('%m/%d/%Y', datetime('now')) limit 1) and ifnull(DepSecuencia, 0) = 0 and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' limit 1", new string[] { }).ToList();

            return list.Count > 0;
        }

        public int GuardarDeposito(DepositosArgs args)
        {
            int depSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Depositos");

            DS_Recibos myRec = new DS_Recibos();

            foreach (var rec in args.RecibosADepositar)
            {
                myRec.ActualizarDepSecuencia(rec.RecSecuencia, depSecuencia, rec.rowguid);
            }

            Hash map = new Hash("Depositos");
            map.Add("DepEstatus", 1);
            map.Add("RepCodigo", Arguments.CurrentUser.RepCodigo.Trim());
            map.Add("DepSecuencia", depSecuencia);
            map.Add("SocCodigo", args.SocCodigo);
            map.Add("DepFecha", Functions.CurrentDate());           
            map.Add("DepMontoCheque", args.MontoChk);
            map.Add("DepMontoChequeDiferido", args.MontoChkFut);
            map.Add("DepMontoEfectivo", args.MontoEfectivo);
            map.Add("DepMontoTarjeta", args.MontoTarjeta);
            map.Add("DepMontoTransferencia", args.MontoTransferencia);
            map.Add("DepNumero", args.Numero);
            map.Add("DepReciboInicial", args.RecibosADepositar[0].RecSecuencia);
            map.Add("DepReciboFinal", args.RecibosADepositar[args.RecibosADepositar.Count - 1].RecSecuencia);
            map.Add("DepTipo", args.Tipo);
            map.Add("DepCantidadRecibos", args.RecibosADepositar.Count);
            map.Add("DepCantTarjetas", 0);
            map.Add("CuBID", args.CuBID);
            map.Add("DepReferencia", args.Referencia);
            map.Add("DepMontoPushMoney", 0);
            map.Add("mbVersion", Functions.AppVersion);
            map.Add("DepMontoOrdenPago", args.MontoOrdenPago);
            if (Arguments.Values.CurrentCuaSecuencia != -1)
            {
                map.Add("CuaSecuencia", Arguments.Values.CurrentCuaSecuencia);
            }

            if (args.location != null)
            {
                map.Add("DepLatitud", args.location.Latitude);
                map.Add("DepLongitud", args.location.Longitude);
            }

            if (args.MonCodigo != null)
            {
                map.Add("MonCodigo", args.MonCodigo);
            }

            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo.Trim());
            map.Add("rowguid", Guid.NewGuid().ToString());
            map.Add("DepFechaActualizacion", Functions.CurrentDate());
            map.ExecuteInsert();

            DS_RepresentantesSecuencias.UpdateSecuencia("Depositos", depSecuencia);

            return depSecuencia;
        }

        public Depositos GetDepositobySecuencia(int depSecuencia, bool confirmado)
        {
            List<Depositos> list = SqliteManager.GetInstance().Query<Depositos>("select DepCantidadRecibos, DepReciboInicial, ifnull(DepReferencia, '') as DepReferencia, DepReciboFinal, DepMontoEfectivo, DepMontoCheque, DepMontoChequeDiferido, DepNumero, " +
                "DepMontoTransferencia, strftime('%d-%m-%Y', DepFecha) as DepFecha, DepTipo, ifnull(CuBNombre, '') as CuBNombre,ifnull(d.MonCodigo, '') as MonCodigo, ifnull(CuBReferencia, '') as CuBReferencia, d.RepCodigo as RepCodigo, DepSecuencia, u.Descripcion as DepTipoDescripcion, DepMontoTarjeta " +
                "from "+(confirmado ? "DepositosConfirmados" : "Depositos") +" d " +
                "left join CuentasBancarias c on c.CuBID = d.CuBID " +
                "left join UsosMultiples u on ltrim(rtrim(upper(u.CodigoGrupo))) = 'DEPTIPO' and CodigoUso = d.DepTipo " +
                "where d.DepSecuencia = ? and d.RepCodigo = ?",
                new string[] { depSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public Depositos GetDepositobySecuenciaForVerDetalle(int depSecuencia, bool confirmado = false)
        {
                Depositos list = SqliteManager.GetInstance().Query<Depositos>("select u.CodigoGrupo as CodigoGrupo, u.CodigoUso as CodigoUso, d.CuBID, " +
                "DepTipo, ifnull(CuBNombre, '') as CuBNombre, ifnull(d.MonCodigo, '') as MonCodigo, d.DepNumero, u.Descripcion as usoDescripcion " +
                "from "+(confirmado ? "DepositosConfirmados" : "Depositos") +" d " +
                "left join CuentasBancarias c on c.CuBID = d.CuBID " +
                "left join UsosMultiples u on ltrim(rtrim(upper(u.CodigoGrupo))) = 'DEPTIPO' and CodigoUso = d.DepTipo " +
                "where DepSecuencia = ? and RepCodigo = ?",
                new string[] { depSecuencia.ToString(), Arguments.CurrentUser.RepCodigo }).FirstOrDefault();

            if (list != null)
            {
                return list;
            }
            return null;
        }

        public Depositos GetDepositobySecuenciaOrdenPago(int depSecuencia, bool confirmado)
        {
            List<Depositos> list = SqliteManager.GetInstance().Query<Depositos>("select DepCantidadRecibos, DepReciboInicial, ifnull(DepReferencia, '') as DepReferencia, DepReciboFinal, DepMontoEfectivo, DepMontoCheque, DepMontoChequeDiferido," +
                "DepMontoTransferencia, DepMontoOrdenPago, strftime('%d-%m-%Y', DepFecha) as DepFecha, DepTipo, ifnull(CuBNombre, '') as CuBNombre, ifnull(CuBReferencia, '') as CuBReferencia, d.RepCodigo as RepCodigo, DepSecuencia, u.Descripcion as DepTipoDescripcion " +
                "from " + (confirmado ? "DepositosConfirmados" : "Depositos") + " d " +
                "left join CuentasBancarias c on c.CuBID = d.CuBID " +
                "left join UsosMultiples u on ltrim(rtrim(upper(u.CodigoGrupo))) = 'DEPTIPO' and CodigoUso = d.DepTipo " +
                "where DepSecuencia = ? and RepCodigo = ?",
                new string[] { depSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public void EstDepositos(int depSecuencia, string rowguid, int est)
        {
            Hash ped = new Hash("Depositos");
            ped.Add("DepEstatus", est);
            ped.Add("UsuInicioSesion", /*Arguments.CurrentUser.RepCodigo*/"mdsoft");
            ped.Add("DepFechaActualizacion", Functions.CurrentDate());
            //ped.ExecuteUpdate("DepSecuencia = " + depSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");

            if (est == 0)
            {
                if (new DS_SuscriptoresCambios().UpdateCambioEstadoInsertByRowguid(rowguid, est))
                {
                    ped.SaveScriptForServer = false;
                }
            }

            ped.ExecuteUpdate("rowguid = '" + rowguid + "'");


            if (est == 0)
            {          
                ///Si se anula un deposito se deben actualizar los recibos que esten depositados
                Hash Rec = new Hash("Recibos");
                Rec.Add("DepSecuencia", 0);
                Rec.Add("RecFechaActualizacion", Functions.CurrentDate());
                Rec.ExecuteUpdate("DepSecuencia = " + depSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");


                Hash RecConf = new Hash("RecibosConfirmados");
                RecConf.Add("DepSecuencia", 0);
                RecConf.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                RecConf.Add("RecFechaActualizacion", Functions.CurrentDate());
                RecConf.ExecuteUpdate("DepSecuencia = " + depSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");

            }
        }

        public int GetCantidadChequesDepositados(int depSecuencia)
        {
            var list = SqliteManager.GetInstance().Query<Depositos>("Select Sum(DepNumero) as DepNumero from (select count(ForID) as DepNumero from RecibosFormaPago F INNER JOIN Recibos R on R.RecSecuencia = F.RecSecuencia and R.RepCodigo = F.RepCodigo " +
                "where F.ForID = 2 and R.DepSecuencia = " + depSecuencia + " and ltrim(rtrim(R.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' " +
                "UNION ALL " +
                "select Count(ForID) as DepNumero from RecibosFormaPagoConfirmados F INNER JOIN RecibosConfirmados R on R.RecSecuencia = F.RecSecuencia and R.RepCodigo = F.RepCodigo " +
                "where F.ForID = 2 and R.DepSecuencia = " + depSecuencia + " and ltrim(rtrim(R.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' ) as result", new string[] { });

            if (list != null && list.Count > 0)
            {
                return list[0].DepNumero;
            }

            return 0;
        }

        public List<Monedas> GetMonedasDeLosRecibosAdepositar()
        {
            return SqliteManager.GetInstance().Query<Monedas>("select m.MonCodigo as MonCodigo, m.MonSigla as MonSigla, m.MonNombre as MonNombre from Monedas m " +
                "inner join RecibosFormaPago p on p.RepCodigo = r.RepCodigo and  trim(upper(p.MonCodigo)) = trim(upper(m.MonCodigo)) "+
                "inner join Recibos r on r.RecSecuencia = p.RecSecuencia "+
                "where ifnull(r.DepSecuencia, 0) = 0 and RecEstatus <> 0 and trim(r.RepCodigo) = ? " +
                "group by m.MonCodigo, m.MonSigla, m.MonNombre " +
                "union " +
                "select m.MonCodigo as MonCodigo, m.MonSigla as MonSigla, m.MonNombre as MonNombre from Monedas m " +
                "inner join RecibosFormaPagoConfirmados p on p.RepCodigo = r.RepCodigo and trim(upper(p.MonCodigo)) = trim(upper(m.MonCodigo)) "+
                "inner join RecibosConfirmados r on r.RecSecuencia = p.RecSecuencia and RecEstatus <> 0 "+
                "where ifnull(r.DepSecuencia, 0) = 0 and trim(r.RepCodigo) = ? " +
                "group by m.MonCodigo, m.MonSigla, m.MonNombre order by MonNombre", 
                new string[] { Arguments.CurrentUser.RepCodigo.Trim(), Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public double GetRecibosRetencion(int DepSecuencia)
        {
            string query = "SELECT SUM(RecRetencion) FROM Recibos Where DepSecuencia = " + DepSecuencia + " and recEstatus <> 0 " +
                    "UNION SELECT SUM(RecRetencion) FROM RecibosConfirmados Where DepSecuencia = " + DepSecuencia + " and recEstatus <> 0 ";
            var list = SqliteManager.GetInstance().Query<Recibos>(query, new string[] { });
            if (list != null && list.Count > 0)
            {
                return list[0].RecRetencion;
            }
                return 0;  

        }

        public void ActualizarIndicadorDepositoFoto(int depSecuencia)
        {
            try
            {
                SqliteManager.GetInstance().Execute("update Depositos set TieneFoto = '1' where DepSecuencia = ? and trim(RepCodigo) = ?",
                    new string[] { depSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }
        }
    }
        
}
