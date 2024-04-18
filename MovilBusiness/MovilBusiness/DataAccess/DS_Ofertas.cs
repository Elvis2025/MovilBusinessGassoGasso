using MovilBusiness.Configuration;
using MovilBusiness.Enums;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace MovilBusiness.DataAccess
{
    public class DS_Ofertas : DS_Controller
    {
        public List<Ofertas> GetOfertasDisponibles(int cliiId, int tinId, bool OfertaespecificaCliente = false, EntregasRepartidorTransacciones entrega = null, bool tieneoferta = true, int conId = -1, bool isConsultaGeneral= false)
        {

            if (!tieneoferta && entrega == null)
            {
                return new List<Ofertas>();
            }

            var query = "";
            if (isConsultaGeneral)
            {
                query = "SELECT case when ifnull(o.ConID, 0) = 0 then 'Todas' else ifnull(c.ConDescripcion, 'Todas') end as ConIdDescripcion, o.OfeID AS 'OfeID', " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? " o.UnmCodigo, " : "") + " o.OfeDescripcion as 'OfeDescripcion', ifnull(ltrim(rtrim(o.OfeTipo)), '') as OfeTipo, OfeFechainicio, OfeFechaFin, U.Descripcion as OfeTipoDescripcion, OfeCantidadMax, o.grpCodigoOferta as grpCodigoOferta, OfeCantidadMaximaTransaccion, o.grpCodigo as grpCodigo, 1 as isConsultaGeneral, ifnull(o.CliID,0) as CliID, ifnull(o.GrcCodigo,0) as GrcCodigo, o.TinID  FROM Ofertas"
                            + " o " +
                            "LEFT JOIN CondicionesPago c on c.ConId = o.ConID " +
                            "LEFT JOIN UsosMultiples U ON ltrim(rtrim(upper(U.CodigoGrupo))) = 'OFETIPO' AND CodigoUso = o.OfeTipo " +
                            "WHERE STRFTIME('%Y-%m-%d',  " + (entrega != null ? "'" + entrega.VenFecha + "'" : " DATETIME('NOW', 'localtime')") + ", '" + Functions.GetDiferenciaHorariaSqlite() + " hours')"
                            + " BETWEEN STRFTIME('%Y-%m-%d', o.OfeFechaInicio) AND"
                            + " " + (myParametro.GetOfertaConDia() ? " STRFTIME('%Y-%m-%d', o.OfeFechaFin, '+1 day') " : " STRFTIME('%Y-%m-%d', o.OfeFechaFin) ") + " ";
            }
            else
            {
                query = "SELECT case when ifnull(o.ConID, 0) = 0 then 'Todas' else ifnull(c.ConDescripcion, 'Todas') end as ConIdDescripcion, o.OfeID AS 'OfeID', " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? " o.UnmCodigo, " : "") + " o.OfeDescripcion as 'OfeDescripcion', ifnull(ltrim(rtrim(o.OfeTipo)), '') as OfeTipo, OfeFechainicio, OfeFechaFin, U.Descripcion as OfeTipoDescripcion, OfeCantidadMax, o.grpCodigoOferta as grpCodigoOferta, OfeCantidadMaximaTransaccion, o.grpCodigo as grpCodigo, 0 as isConsultaGeneral  FROM Ofertas"
                            + " o " +
                            "LEFT JOIN CondicionesPago c on c.ConId = o.ConID " +
                            "LEFT JOIN UsosMultiples U ON ltrim(rtrim(upper(U.CodigoGrupo))) = 'OFETIPO' AND CodigoUso = o.OfeTipo " +
                            "LEFT join OfertasDetalleTiposNegocio otn on otn.OfeID = O.OfeID and otn.TinID = " + tinId.ToString() + " " +
                            "WHERE " + (OfertaespecificaCliente ? " o.CliID = " + cliiId : " (((o.CliID = " + cliiId + " OR IFNULL(o.CliID, 0) = 0) AND (IFNULL(GrcCodigo, '0') = '0' or ifnull(GrcCodigo, '') = '')) OR " + cliiId + " "
                            + " IN (SELECT CliID FROM GrupoClientesDetalle WHERE LTRIM(RTRIM(GrcCodigo)) = LTRIM(RTRIM(o.GrcCodigo)))) ")
                            + " AND STRFTIME('%Y-%m-%d',  " + (entrega != null ? "'" + entrega.VenFecha + "'" : " DATETIME('NOW', 'localtime')") + ", '" + Functions.GetDiferenciaHorariaSqlite() + " hours')"
                            + " BETWEEN STRFTIME('%Y-%m-%d', o.OfeFechaInicio) AND"
                            + " " + (myParametro.GetOfertaConDia() ? " STRFTIME('%Y-%m-%d', o.OfeFechaFin, '+1 day') " : " STRFTIME('%Y-%m-%d', o.OfeFechaFin) ") + " "
                            + "AND ((o.OfeTipo = 4 and o.ProNoVendido not in (select CPV.ProID from ClientesProductosVendidos CPV " +
                            "where CPV.cliID = " + cliiId + " and CPV.proID = o.ProNoVendido)) OR o.OfeTipo <> 4) ";
            }

            var orderBy = " ORDER BY LTRIM(RTRIM(OfeDescripcion)) ASC "+(OfertaespecificaCliente ? " LIMIT 1 ": "" );

            if (conId != -1)
            {
                query += " and (ifnull(o.ConID, 0) = 0 or o.ConID = " + conId.ToString() + ") ";
            }

            query += "/* AND ( " + tinId + "  IN (SELECT TinID FROM OfertasDetalleTiposNegocio WHERE OfeID = O.OfeID))*/ " +
                "AND o.OfeID NOT IN (Select OfeID from OfertasClientesExcepciones where CliID = "+ cliiId + " AND OfeID = o.OfeID) " + orderBy;


            return SqliteManager.GetInstance().Query<Ofertas>(query, new string[] { });
        }

        public List<Ofertas> GetOfertasDisponiblesPorSegmento(int cliiId, int tinId, int proId = -1, bool OfertaespecificaCliente = false ,EntregasRepartidorTransacciones entrega=null, bool tieneoferta = true, int conId = -1, bool isConsultaGeneral = false)
        {
            if(!tieneoferta && entrega == null)
            {
                return new List<Ofertas>();
            }

            var query = "";
            if (isConsultaGeneral)
            {
                query = "SELECT case when ifnull(o.ConID, 0) = 0 then 'Todas' else ifnull(c.ConDescripcion, 'Todas') end as ConIdDescripcion, o.OfeID AS 'OfeID', " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? " o.UnmCodigo, " : "") + " o.OfeDescripcion as 'OfeDescripcion', ifnull(ltrim(rtrim(o.OfeTipo)), '') as OfeTipo, OfeFechainicio, OfeFechaFin, U.Descripcion as OfeTipoDescripcion, OfeCantidadMax, o.grpCodigoOferta as grpCodigoOferta, OfeCantidadMaximaTransaccion, 1 as isConsultaGeneral, ifnull(o.CliID,0) as CliID, ifnull(o.GrcCodigo,0) as GrcCodigo, o.TinID, ifnull(o.OfeCaracteristicas,'') as OfeCaracteristicas FROM Ofertas"
                            + " o " +
                            "LEFT JOIN CondicionesPago c on c.ConId = o.ConID " +
                            "LEFT JOIN UsosMultiples U ON ltrim(rtrim(upper(U.CodigoGrupo))) = 'OFETIPO' AND CodigoUso = o.OfeTipo " +
                            "WHERE STRFTIME('%Y-%m-%d', " + (entrega != null ? "'" + entrega.VenFecha + "'" : " DATETIME('NOW', 'localtime')") + ", '" + Functions.GetDiferenciaHorariaSqlite() + " hours')"
                            + " BETWEEN STRFTIME('%Y-%m-%d', o.OfeFechaInicio) AND"
                            + " " + (myParametro.GetOfertaConDia() ? " STRFTIME('%Y-%m-%d', o.OfeFechaFin, '+1 day') " : " STRFTIME('%Y-%m-%d', o.OfeFechaFin) ") + " ";
            }
            else
            {
                query = "SELECT case when ifnull(o.ConID, 0) = 0 then 'Todas' else ifnull(c.ConDescripcion, 'Todas') end as ConIdDescripcion, o.OfeID AS 'OfeID', " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? " o.UnmCodigo, " : "") + " o.OfeDescripcion as 'OfeDescripcion', ifnull(ltrim(rtrim(o.OfeTipo)), '') as OfeTipo, OfeFechainicio, OfeFechaFin, U.Descripcion as OfeTipoDescripcion, OfeCantidadMax, o.grpCodigoOferta as grpCodigoOferta, OfeCantidadMaximaTransaccion, 0 as isConsultaGeneral, ifnull(o.OfeCaracteristicas,'') as OfeCaracteristicas FROM Ofertas"
                            + " o " +
                            "LEFT JOIN CondicionesPago c on c.ConId = o.ConID " +
                            "LEFT JOIN UsosMultiples U ON ltrim(rtrim(upper(U.CodigoGrupo))) = 'OFETIPO' AND CodigoUso = o.OfeTipo " +
                            "INNER join OfertasDetalleTiposNegocio otn on otn.OfeID = O.OfeID and otn.TinID = " + tinId.ToString() + " " +
                            "WHERE " + (OfertaespecificaCliente ? " o.CliID = " + cliiId : " (((o.CliID = " + cliiId + " OR IFNULL(o.CliID, 0) = 0) AND (IFNULL(GrcCodigo, '0') = '0' or ifnull(GrcCodigo, '') = '')) OR " + cliiId + " "
                            + " IN (SELECT CliID FROM GrupoClientesDetalle WHERE LTRIM(RTRIM(GrcCodigo)) = LTRIM(RTRIM(o.GrcCodigo)))) ")
                            + " AND STRFTIME('%Y-%m-%d', " + (entrega != null ? "'" + entrega.VenFecha + "'" : " DATETIME('NOW', 'localtime')") + ", '" + Functions.GetDiferenciaHorariaSqlite() + " hours')"
                            + " BETWEEN STRFTIME('%Y-%m-%d', o.OfeFechaInicio) AND"
                            + " " + (myParametro.GetOfertaConDia() ? " STRFTIME('%Y-%m-%d', o.OfeFechaFin, '+1 day') " : " STRFTIME('%Y-%m-%d', o.OfeFechaFin) ") + " "
                            + "AND ((o.OfeTipo = 4 and o.ProNoVendido not in (select CPV.ProID from ClientesProductosVendidos CPV " +
                            "where CPV.cliID = " + cliiId + " and CPV.proID = o.ProNoVendido)) OR o.OfeTipo <> 4) ";
            }

            if (proId != -1)
            {
                string porcionGrpCodigo = "";
                if (myParametro.GetParGrupoProductosJson())
                {
                    porcionGrpCodigo = " ((instr((select trim(proGrupoProductos) from Productos where ProID = " + proId + "), '\"grpcodigo\":\"'||trim(o.GrpCodigo) ) > 0))";
                }
                else
                {
                    porcionGrpCodigo = " " + proId + " IN (SELECT ProID FROM GrupoProductosDetalle WHERE TRIM(o.GrpCodigo) = TRIM(GrpCodigo) and ProID = " + proId + " ) ";
                }

                query += " AND ((IFNULL(o.ProID, 0) = " + proId + " and o.OfeTipo <> 3) " +
                        " OR ((IFNULL(o.ProID, 0) = 0 " +
                        "AND IFNULL(o.GrpCodigo, '0') <> '0' AND " + porcionGrpCodigo + "  )) " +
                        "OR ((IFNULL(o.ProID, 0) = 0 AND IFNULL(o.GrpCodigo, '0') = '0')) )";
            }

            var orderBy = " ORDER BY " + (proId != -1 ? "OfeNivel ASC" : "LTRIM(RTRIM(OfeDescripcion)) ASC " + (OfertaespecificaCliente ? " LIMIT 1 " : ""));

            if (conId != -1)
            {
                query += " and (ifnull(o.ConID, 0) = 0 or o.ConID = " + conId.ToString() + ") ";
            }

            query += "/* AND ( " + tinId + "  IN (SELECT TinID FROM OfertasDetalleTiposNegocio WHERE OfeID = O.OfeID))*/ " +
                "AND o.OfeID NOT IN (Select OfeID from OfertasClientesExcepciones where CliID = " + cliiId + " AND OfeID = o.OfeID) " + orderBy;


            return SqliteManager.GetInstance().Query<Ofertas>(query, new string[] { });
        }


        public List<Ofertas> GetOfertasDisponibles(int cliiId, int tinId, int proId = -1, bool OfertaespecificaCliente = false, EntregasRepartidorTransacciones entrega = null, bool tieneoferta = true, int conId = -1, bool isConsultaGeneral = false)
        {

            if (!tieneoferta && entrega == null)
            {
                return new List<Ofertas>();
            }

            var query = "SELECT case when ifnull(o.ConID, 0) = 0 then 'Todas' else ifnull(c.ConDescripcion, 'Todas') end as ConIdDescripcion, OfeNivel, o.OfeID AS 'OfeID', " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? " o.UnmCodigo, " : "") + " o.OfeDescripcion as 'OfeDescripcion', ifnull(ltrim(rtrim(o.OfeTipo)), '') as OfeTipo, OfeFechainicio, OfeFechaFin, U.Descripcion as OfeTipoDescripcion, OfeCantidadMax, o.grpCodigoOferta as grpCodigoOferta, OfeCantidadMaximaTransaccion, o.grpCodigo as grpCodigo, " + (!isConsultaGeneral ? " 0 as isConsultaGeneral, " : " case when (ifnull(o.GrcCodigo,0) = 0 or o.GrcCodigo = '') and ifnull(o.CliID,0) = 0 then 0 else 1 end as isConsultaGeneral, ") + " ifnull(o.CliID,0) as CliID, ifnull(o.GrcCodigo,0) as GrcCodigo, ifnull(o.OfeCaracteristicas,'') as OfeCaracteristicas FROM Ofertas"
                            + " o " +
                            "LEFT JOIN CondicionesPago c on c.ConId = o.ConID " +
                            "LEFT JOIN UsosMultiples U ON ltrim(rtrim(upper(U.CodigoGrupo))) = 'OFETIPO' AND CodigoUso = o.OfeTipo " +
                            "LEFT join OfertasDetalleTiposNegocio otn on otn.OfeID = O.OfeID and otn.TinID = " + tinId.ToString() + " " +
                            " " + (proId != -1 && !myParametro.GetParGrupoProductosJson() ? " inner join GrupoProductosDetalle grp on grp.GrpCodigo = o.GrpCodigo and grp.ProID = " + proId + " " : "") + " " +
                            "WHERE " + (OfertaespecificaCliente ? " o.CliID = " + cliiId : " (((o.CliID = " + cliiId + " OR IFNULL(o.CliID, 0) = 0) AND (IFNULL(GrcCodigo, '0') = '0' or ifnull(GrcCodigo, '') = '')) OR " + cliiId + " "
                            + " IN (SELECT CliID FROM GrupoClientesDetalle WHERE LTRIM(RTRIM(GrcCodigo)) = LTRIM(RTRIM(o.GrcCodigo)))) ")
                            + " AND STRFTIME('%Y-%m-%d',  " + (entrega != null ? "'" + entrega.VenFecha + "'" : " DATETIME('NOW', 'localtime')") + ", '" + Functions.GetDiferenciaHorariaSqlite() + " hours')"
                            + " BETWEEN STRFTIME('%Y-%m-%d', o.OfeFechaInicio) AND"
                            + " " + (myParametro.GetOfertaConDia() ? " STRFTIME('%Y-%m-%d', o.OfeFechaFin, '+1 day') " : " STRFTIME('%Y-%m-%d', o.OfeFechaFin) ") + " "
                            + "AND ((o.OfeTipo = 4 and o.ProNoVendido not in (select CPV.ProID from ClientesProductosVendidos CPV " +
                            "where CPV.cliID = " + cliiId + " and CPV.proID = o.ProNoVendido)) OR o.OfeTipo <> 4) ";

            if (proId != -1)
            {
                string porcionGrpCodigo = "";
                if (myParametro.GetParGrupoProductosJson())
                {
                    porcionGrpCodigo = " AND ((instr((select trim(proGrupoProductos) from Productos where ProID = " + proId + "), '\"grpcodigo\":\"'||trim(o.GrpCodigo) ) > 0))";
                }

                query += " AND (IFNULL(o.ProID, 0) = 0) " + porcionGrpCodigo + "";
            }            

            query += "/* AND ( " + tinId + "  IN (SELECT TinID FROM OfertasDetalleTiposNegocio WHERE OfeID = O.OfeID))*/ " +
                "AND o.OfeID NOT IN (Select OfeID from OfertasClientesExcepciones where CliID = " + cliiId + " AND OfeID = o.OfeID) ";

            query += " Union All SELECT case when ifnull(o.ConID, 0) = 0 then 'Todas' else ifnull(c.ConDescripcion, 'Todas') end as ConIdDescripcion, OfeNivel, o.OfeID AS 'OfeID', " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? " o.UnmCodigo, " : "") + " o.OfeDescripcion as 'OfeDescripcion', ifnull(ltrim(rtrim(o.OfeTipo)), '') as OfeTipo, OfeFechainicio, OfeFechaFin, U.Descripcion as OfeTipoDescripcion, OfeCantidadMax, o.grpCodigoOferta as grpCodigoOferta, OfeCantidadMaximaTransaccion, o.grpCodigo as grpCodigo, " + (!isConsultaGeneral ? "0 as isConsultaGeneral, " : " case when (ifnull(o.GrcCodigo,0) = 0 or o.GrcCodigo = '') and ifnull(o.CliID,0) = 0 then 0 else 1 end as isConsultaGeneral, ") + " ifnull(o.CliID,0) as CliID, ifnull(o.GrcCodigo,0) as GrcCodigo, ifnull(o.OfeCaracteristicas,'') as OfeCaracteristicas FROM Ofertas"
                            + " o " +
                            "LEFT JOIN CondicionesPago c on c.ConId = o.ConID " +
                            "LEFT JOIN UsosMultiples U ON ltrim(rtrim(upper(U.CodigoGrupo))) = 'OFETIPO' AND CodigoUso = o.OfeTipo " +
                            "LEFT join OfertasDetalleTiposNegocio otn on otn.OfeID = O.OfeID and otn.TinID = " + tinId.ToString() + " " +                            
                            "WHERE " + (OfertaespecificaCliente ? " o.CliID = " + cliiId : " (((o.CliID = " + cliiId + " OR IFNULL(o.CliID, 0) = 0) AND (IFNULL(GrcCodigo, '0') = '0' or ifnull(GrcCodigo, '') = '')) OR " + cliiId + " "
                            + " IN (SELECT CliID FROM GrupoClientesDetalle WHERE LTRIM(RTRIM(GrcCodigo)) = LTRIM(RTRIM(o.GrcCodigo)))) ")
                            + " AND STRFTIME('%Y-%m-%d',  " + (entrega != null ? "'" + entrega.VenFecha + "'" : " DATETIME('NOW', 'localtime')") + ", '" + Functions.GetDiferenciaHorariaSqlite() + " hours')"
                            + " BETWEEN STRFTIME('%Y-%m-%d', o.OfeFechaInicio) AND"
                            + " " + (myParametro.GetOfertaConDia() ? " STRFTIME('%Y-%m-%d', o.OfeFechaFin, '+1 day') " : " STRFTIME('%Y-%m-%d', o.OfeFechaFin) ") + " "
                            + "AND ((o.OfeTipo = 4 and o.ProNoVendido not in (select CPV.ProID from ClientesProductosVendidos CPV " +
                            "where CPV.cliID = " + cliiId + " and CPV.proID = o.ProNoVendido)) OR o.OfeTipo <> 4) ";


            string orderBy = "";

            if (proId != -1)
            {
                query += " AND ((IFNULL(o.ProID, 0) = " + proId + " and o.OfeTipo <> 3) " +
                        "OR ((IFNULL(o.ProID, 0) = 0 AND IFNULL(o.GrpCodigo, '0') = '0')) ) ";

                orderBy = " ORDER BY OfeNivel ASC ";
            }
            else
            {
                orderBy = " LTRIM(RTRIM(OfeDescripcion)) ASC " + (OfertaespecificaCliente ? " LIMIT 1 " : "");
            }

            if (conId != -1)
            {
                query += " and (ifnull(o.ConID, 0) = 0 or o.ConID = " + conId.ToString() + ") ";
            }

            query += "/* AND ( " + tinId + "  IN (SELECT TinID FROM OfertasDetalleTiposNegocio WHERE OfeID = O.OfeID))*/ " +
                "AND o.OfeID NOT IN (Select OfeID from OfertasClientesExcepciones where CliID = " + cliiId + " AND OfeID = o.OfeID) " + orderBy;

            return SqliteManager.GetInstance().Query<Ofertas>(query);
        }


        public List<Ofertas> GetOfertasMancomunadasDisponibles(int cliid, int tinId, int titId, EntregasRepartidorTransacciones entrega = null, int conId = -1)
        {
            var sql = "";
            var whereGrpCodigo = "";

            if (myParametro.GetParGrupoProductosJson())
            {
                /*whereGrpCodigo = "INNER JOIN Productos GPD on (instr(trim(GPD.ProGrupoProductos), '\"grpcodigo\":\"'||trim(o.GrpCodigo) ) > 0) "
                                    + "INNER JOIN ProductosTemp pt on GPD.ProID = pt.ProID ";*/
                whereGrpCodigo = " (instr(trim(ProGrupoProductos), '\"grpcodigo\":\"'||trim(o.GrpCodigo) ) > 0) ";
            }
            else
            {
                /* whereGrpCodigo = "INNER JOIN GrupoProductosDetalle GPD on trim(GPD.GrpCodigo) = trim(o.GrpCodigo) "
                                       + "INNER JOIN ProductosTemp pt on GPD.Proid = pt.ProID ";*/
                whereGrpCodigo = " trim(GrpCodigo) = trim(o.GrpCodigo) ";
            }

            sql = "SELECT case when ifnull(o.ConID, 0) = 0 then 'Todas' else ifnull(c.ConDescripcion, 'Todas') end as ConIdDescripcion, o.OfeID as OfeID, " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? " o.UnmCodigo, " : "") + " o.OfeDescripcion as OfeDescripcion, o.GrpCodigo as GrpCodigo, (select case when o.OfeTipo = 13 then SUM(ifnull(CantidadDetalle, 0) + (ifnull(Cantidad, 0) * case when ifnull(ProUnidades, 0) = 0 then 1 else ProUnidades end)) else sum(Cantidad) end from ProductosTemp pt where ifnull(pt.IndicadorOferta,0) = 0 and " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? " pt.UnmCodigo = o.UnmCodigo and " : "") + " TitID = " + titId.ToString() + " and ProID in (select ProID from GrupoProductosDetalle where " + whereGrpCodigo + ")) AS CantidadTemp, " +
                "ifnull(o.Grccodigo, 0) as Grccodigo, ifnull(o.grpCodigoOferta, 0) as grpCodigoOferta,  o.OfeTipo as OfeTipo, " +
                "ifnull(od.OfePorciento, 0) as OfePorciento , ifnull(o.OfeCantidadMax,0) as OfeCantidadMax, OfeCantidadMaximaTransaccion, od.ProID as ProID "
              + "FROM Ofertas o "
              + "LEFT JOIN CondicionesPago c on c.ConId = o.ConID "
              + "LEFT JOIN OfertasDetalleTiposNegocio odtn on o.OfeID = odtn.OfeID and odtn.TinID = " + tinId.ToString() + " "
              + "LEFT JOIN  GrupoClientesDetalle gcd on  gcd.GrcCodigo = o.GrcCodigo and gcd.CliID =  " + cliid.ToString() + " "
              //+ porcionJoinGrpCodigo
              + "INNER JOIN OfertasDetalle od on od.OfeId = o.Ofeid "
              //"inner join Productos p on p.ProID = pt.ProID "
              + "where  o.OfeTipo in ('3', '13', '14','9') "
              + "AND (ifnull(o.TinID, 0) = 0 OR (ifnull(o.TinID, 0) <> 0 and  odtn.ofeID = o.ofeID) )"
              + "AND ((ifnull(o.CliiD, 0) = " + cliid.ToString() + " and (ifnull(o.grcCodigo, '') = '' or ifnull(o.grcCodigo, '0') = '0')) or (ifnull(o.cliid, 0) = 0 and o.GrcCodigo = gcd.GrcCodigo) or (ifnull(o.CliID, 0) = 0 and (ifnull(o.grcCodigo, '') = '' or ifnull(o.grcCodigo, '0') = '0'))) "
              + "AND " + cliid.ToString() + " not in (select CliID from OfertasClientesExcepciones where CliID = " + cliid.ToString() + " and OfeID = o.OfeID) "
              + "AND STRFTIME('%Y-%m-%d',  " + (entrega != null ? "'" + entrega.VenFecha + "'" : " DATETIME('NOW', 'localtime')") + ", '0.0 hours') BETWEEN STRFTIME('%Y-%m-%d', o.OfeFechaInicio) AND  " + (myParametro.GetOfertaConDia() ? " STRFTIME('%Y-%m-%d', o.OfeFechaFin, '+1 day') " : " STRFTIME('%Y-%m-%d', o.OfeFechaFin) ") + " "
              + "AND ((o.OfeTipo = '14' and CantidadTemp >= od.OfeCantidad ) or (CantidadTemp  >= od.OfeCantidad and o.OfeTipo = '3') or (CantidadTemp >= od.OfeCantidadDetalle and o.OfeTipo = '13') or (CantidadTemp >= od.OfeCantidadDetalle and o.OfeTipo = '9')) "
              + (conId != -1 ? " and (ifnull(o.ConID, 0) = 0 or o.ConID = " + conId.ToString() + ") " : " ")
              + "GROUP by o.OfeID, o.OfeDescripcion, o.GRPcodigo, o.Grccodigo/*, od.OfeCantidad*/ ";
              //+ "having ((o.OfeTipo = '14' and CantidadTemp >= od.OfeCantidad ) or (CantidadTemp  >= od.OfeCantidad and o.OfeTipo = '3') or (CantidadTemp >= od.OfeCantidadDetalle and o.OfeTipo = '13')) ";

           return SqliteManager.GetInstance().Query<Ofertas>(sql, new string[] { });
        }


        public List<Ofertas> GetOfertasMancomunadasDisponiblesForAgregarProductosModal(int cliid, int tinId, int titId, EntregasRepartidorTransacciones entrega = null, int proid = -1)
        {
            var sql = "";
           

            sql = "SELECT  o.OfeID as OfeID, " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? " o.UnmCodigo, " : "") + " o.OfeDescripcion as OfeDescripcion, o.GrpCodigo as GrpCodigo, " +
                "ifnull(o.Grccodigo, 0) as Grccodigo, ifnull(o.grpCodigoOferta, 0) as grpCodigoOferta,  o.OfeTipo as OfeTipo, " +
                "ifnull(od.OfePorciento, 0) as OfePorciento , ifnull(o.OfeCantidadMax,0) as OfeCantidadMax, OfeCantidadMaximaTransaccion, od.ProID as ProID "
              + "FROM Ofertas o "
              + "LEFT JOIN OfertasDetalleTiposNegocio odtn on o.OfeID = odtn.OfeID and odtn.TinID = " + tinId.ToString()+" "
              + "INNER JOIN  GrupoProductosDetalle gpd on  gpd.GrpCodigo = o.GrpCodigo and gpd.Proid =  " + proid.ToString()+" "
              //+ porcionJoinGrpCodigo
              + "INNER JOIN OfertasDetalle od on od.OfeId = o.Ofeid "
              //"inner join Productos p on p.ProID = pt.ProID "
              + "where  o.OfeTipo in ('3', '13', '14') "
              + "AND (ifnull(o.TinID, 0) = 0 OR (ifnull(o.TinID, 0) <> 0 and  odtn.ofeID = o.ofeID) )"
              + "AND ((ifnull(o.CliiD, 0) = "+cliid.ToString()+" and (ifnull(o.grcCodigo, '') = '' or ifnull(o.grcCodigo, '0') = '0')) or (ifnull(o.cliid, 0) = 0 and o.GrpCodigo = gpd.GrpCodigo) or (ifnull(o.CliID, 0) = 0 and (ifnull(o.grcCodigo, '') = '' or ifnull(o.grcCodigo, '0') = '0'))) "
              + "AND "+cliid.ToString()+" not in (select CliID from OfertasClientesExcepciones where CliID = "+cliid.ToString()+" and OfeID = o.OfeID) "
              + "AND STRFTIME('%Y-%m-%d',  " + (entrega != null ? "'" + entrega.VenFecha + "'" : " DATETIME('NOW', 'localtime')") + ", '0.0 hours') BETWEEN STRFTIME('%Y-%m-%d', o.OfeFechaInicio) AND  " + (myParametro.GetOfertaConDia() ? " STRFTIME('%Y-%m-%d', o.OfeFechaFin, '+1 day') " : " STRFTIME('%Y-%m-%d', o.OfeFechaFin) ") + " " 
              + "GROUP by o.OfeID, o.OfeDescripcion, o.GRPcodigo, o.Grccodigo/*, od.OfeCantidad*/ "
              + "having ((o.OfeTipo = '14') or ( o.OfeTipo = '3') or (o.OfeTipo = '13')) ";

            return SqliteManager.GetInstance().Query<Ofertas>(sql, new string[] { });
        }
        
        public List<Ofertas> GetOfertasMancomunadasDisponiblesPorSegmento(int cliid, int tinId, int titId, EntregasRepartidorTransacciones entrega = null)
        {
            var sql = "";
            var whereGrpCodigo = "";

            if (myParametro.GetParGrupoProductosJson())
            {
                /*whereGrpCodigo = "INNER JOIN Productos GPD on (instr(trim(GPD.ProGrupoProductos), '\"grpcodigo\":\"'||trim(o.GrpCodigo) ) > 0) "
                                    + "INNER JOIN ProductosTemp pt on GPD.ProID = pt.ProID ";*/
                whereGrpCodigo = " (instr(trim(ProGrupoProductos), '\"grpcodigo\":\"'||trim(o.GrpCodigo) ) > 0) ";
            }
            else
            {
                /* whereGrpCodigo = "INNER JOIN GrupoProductosDetalle GPD on trim(GPD.GrpCodigo) = trim(o.GrpCodigo) "
                                       + "INNER JOIN ProductosTemp pt on GPD.Proid = pt.ProID ";*/
                whereGrpCodigo = " trim(GrpCodigo) = trim(o.GrpCodigo) ";
            }

            sql = "SELECT  o.OfeID as OfeID, " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? " o.UnmCodigo, " : "") + " o.OfeDescripcion as OfeDescripcion, o.GrpCodigo as GrpCodigo, (select case when o.OfeTipo = 13 then SUM(ifnull(CantidadDetalle, 0) + (ifnull(Cantidad, 0) * case when ifnull(ProUnidades, 0) = 0 then 1 else ProUnidades end)) else sum(Cantidad) end from ProductosTemp where TitID = " + titId.ToString() + " and ProID in (select ProID from GrupoProductosDetalle where " + whereGrpCodigo + ")) AS CantidadTemp, " +
                "ifnull(o.Grccodigo, 0) as Grccodigo, ifnull(o.grpCodigoOferta, 0) as grpCodigoOferta,  o.OfeTipo as OfeTipo, " +
                "ifnull(od.OfePorciento, 0) as OfePorciento , ifnull(o.OfeCantidadMax,0) as OfeCantidadMax, OfeCantidadMaximaTransaccion, od.ProID as ProID "
              + "FROM Ofertas o "
              + "INNER JOIN OfertasDetalleTiposNegocio odtn on o.OfeID = odtn.OfeID and odtn.TinID = " + tinId.ToString() + " "
              + "LEFT JOIN  GrupoClientesDetalle gcd on  gcd.GrcCodigo = o.GrcCodigo and gcd.CliID =  " + cliid.ToString() + " "
              //+ porcionJoinGrpCodigo
              + "INNER JOIN OfertasDetalle od on od.OfeId = o.Ofeid "
              //"inner join Productos p on p.ProID = pt.ProID "
              + "where  o.OfeTipo in ('3', '13', '14') "
              + "AND (ifnull(o.TinID, 0) = 0 OR (ifnull(o.TinID, 0) <> 0 and  odtn.ofeID = o.ofeID) )"
              + "AND ((ifnull(o.CliiD, 0) = " + cliid.ToString() + " and (ifnull(o.grcCodigo, '') = '' or ifnull(o.grcCodigo, '0') = '0')) or (ifnull(o.cliid, 0) = 0 and o.GrcCodigo = gcd.GrcCodigo) or (ifnull(o.CliID, 0) = 0 and (ifnull(o.grcCodigo, '') = '' or ifnull(o.grcCodigo, '0') = '0'))) "
              + "AND " + cliid.ToString() + " not in (select CliID from OfertasClientesExcepciones where CliID = " + cliid.ToString() + " and OfeID = o.OfeID) "
              + "AND STRFTIME('%Y-%m-%d',  " + (entrega != null ? "'" + entrega.VenFecha + "'" : " DATETIME('NOW', 'localtime')") + ", '0.0 hours') BETWEEN STRFTIME('%Y-%m-%d', o.OfeFechaInicio) AND  " + (myParametro.GetOfertaConDia() ? " STRFTIME('%Y-%m-%d', o.OfeFechaFin, '+1 day') " : " STRFTIME('%Y-%m-%d', o.OfeFechaFin) ") + " "
              + "GROUP by o.OfeID, o.OfeDescripcion, o.GRPcodigo, o.Grccodigo/*, od.OfeCantidad*/ "
              + "having ((o.OfeTipo = '14' and CantidadTemp >= cast(od.OfeCantidad as integer)) or (CantidadTemp  >= cast((od.OfeCantidad) as integer) and o.OfeTipo = '3') or (CantidadTemp >= od.OfeCantidadDetalle and o.OfeTipo = '13')) ";

            return SqliteManager.GetInstance().Query<Ofertas>(sql, new string[] { });
        }

        /*Si el proId no es igual a -1, se busca una oferta valida para este*/
        public List<OfertasDetalle> GetDetalleByOfeId(Ofertas oferta, int proId = -1, double cantidad = -1,string productoUnidad="", bool IsConsultaGeneral= false)
        {
            var whereProductoValido = proId != -1 && cantidad > 0 ? " and " + cantidad.ToString() + " >= d.OfeCantidad " : "";
            var InnerOfertas = "";
            var LeftListaPrecios = "";
            var orderBy = "";

            if ((oferta.OfeTipo == "11") && proId != -1 && cantidad > 0)
            {
                whereProductoValido = " and " + ((int)cantidad).ToString() + " between d.OfeCantidad and d.OfeCantidadDetalle ";
               
            }

            if(oferta.ProID != 0 && string.IsNullOrWhiteSpace(oferta.GrpCodigo))
            {
                whereProductoValido += " and d.ProID = " + proId.ToString();
            }

            if (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() && !IsConsultaGeneral)
            {
                
                if (productoUnidad != "")
                {
                    whereProductoValido += " and l.UnmCodigo = d.UnmCodigo ";
                    InnerOfertas = "Inner Join Ofertas o on o.OfeID=d.OfeID and o.UnmCodigo = '" + productoUnidad.ToString() + "' ";
                    orderBy = proId != -1 && cantidad > 0 ? " order by OfeCantidad Desc" : " order by OfeCantidad ASC";
                }
                else
                {
                    whereProductoValido += " and l.UnmCodigo = d.UnmCodigo ";
                }

            }
            else
            {
                orderBy = proId != -1 && cantidad > 0 ? " order by OfeCantidad Desc" : " order by OfeCantidad ASC";
            }

            if (!IsConsultaGeneral)
            {
                LeftListaPrecios = "left join ListaPrecios l on l.ProID = " + (proId != -1 ? " IFNULL(NULLIF(IFNULL(d.ProID, 0), 0), " + proId.ToString() + ")" : "p.ProID") + " and ltrim(rtrim(l.LipCodigo)) = '" + Arguments.Values.CurrentClient.LiPCodigo + "' and (ltrim(rtrim(l.MonCodigo)) = '" + Arguments.Values.CurrentClient.MonCodigo + "' or ifnull(l.MonCodigo, '') = '') ";
            }

            var query = "select d.*, " + (!IsConsultaGeneral ? "l.LipPrecio" : "0") + " as PrecioLista, ifnull(p.ProDatos3, '') as ProDatos3, " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? " d.UnmCodigo as UnmCodigo, (select ofe.UnmCodigo from ofertas ofe where ofe.OfeID=d.OfeID limit 1) as UnmCodigoCabecera, " : " p.UnmCodigo as UnmCodigo,") + "   p.ProDescripcion as ProDescripcion, p.ProDescripcion1 as ProDescripcion1 " + (proId != -1 ? ",  IFNULL(NULLIF(IFNULL(d.ProID, 0), 0), "+ proId.ToString() + ") as ProID" : "") + " " +
                "from OfertasDetalle d " +
                " "+ InnerOfertas + " " +
                "left join Productos p on p.ProID = d.ProID " +
                 " " + LeftListaPrecios + " " +
                "where d.OfeID = ? " + whereProductoValido  + " GROUP by OfeSecuencia " + orderBy;

            return SqliteManager.GetInstance().Query<OfertasDetalle>(query, new string[] { oferta.OfeID.ToString() });
        }

        public List<OfertasDetalle> GetDetalleOfertaById(int ofeId, int cantidadtemp = 0)
        {

            return SqliteManager.GetInstance().Query<OfertasDetalle>("select * from OfertasDetalle od " +
                " where od.OfeID = ? "+
                /*" and "+cantidadtemp+" >= cast((od.OfeCantidad) as integer) GROUP by o.OfeID, o.OfeDescripcion, o.GRPcodigo, o.Grccodigo " +*/
                " order by od.OfeCantidad desc", new string[] { ofeId.ToString() });
        }

        public List<Ofertas> GetOfertaById(int ofeId)
        {
            return SqliteManager.GetInstance().Query<Ofertas>("select * from Ofertas od " +
                " where od.OfeID = ? " +
                " order by od.OfeID desc", new string[] { ofeId.ToString() });
        }

        public List<OfertasDetalle> GetDetalleOfertaMancomunadaById(int ofeId, int cantidadtemp = 0)
        {

            return SqliteManager.GetInstance().Query<OfertasDetalle>("select * from OfertasDetalle od " +
                " where od.OfeID = ? " +
                " and " + cantidadtemp + " >= cast((od.OfeCantidad) as integer) " +
                " order by od.OfeCantidad desc", new string[] { ofeId.ToString() });
        }

        private List<OfertasDetalle> GetDetalleOfertaProductosNoVendidos(int OfeId, int proId, double cantidad)
        {
            var parGrupoProductosJson = myParametro.GetParGrupoProductosJson();

            var where = "";

            if (parGrupoProductosJson)
            {
                where += " and trim(o.GrpCodigo) in (trim(ifnull(replace(replace(replace(replace(replace(p.ProGrupoProductos, '{\"grpcodigo\"}', ''), '{', ''), '}', ''), '[', ''), ']', ''),''))) ";
            }
            else
            {
                where += " and trim(o.GrpCodigo) in (select trim(GrpCodigo) from GrupoProductosDetalle where ProID = " + proId.ToString() + ") ";
            }

            var query = "select distinct d.*, l.LipPrecio as PrecioLista, p.ProDatos3 as ProDatos3, p.UnmCodigo as UnmCodigo, p.proGrupoProductos as ProGrupoProductos, ifnull(d.ProID, " + proId.ToString()+") as ProID, d.OfePrecio as OfePrecio, p.ProDescripcion as ProDescripcion from OfertasDetalle d " +
                "inner join Ofertas o on o.OfeID = d.OfeID " +
                "left join Productos p on p.ProID = d.ProID " +
                "left join ListaPrecios l on l.ProID = p.ProID and ltrim(rtrim(l.LipCodigo)) = '"+Arguments.Values.CurrentClient.LiPCodigo+"' and (ltrim(rtrim(l.MonCodigo)) = '"+Arguments.Values.CurrentClient.MonCodigo+"' or ifnull(l.MonCodigo, '') = '') " +
                "where d.OfeID = ? and o.OfeTipo = '4' " +
                "and (ifnull(o.ProID, 0) = "+proId.ToString()+" or (ifnull(o.ProID, 0) = 0 " + where + ") or (ifnull(o.ProID, 0) = 0 and (ifnull(o.GrpCodigo, '') = '' or ifnull(o.GrpCodigo, 0) = 0))) " +
                "and o.ProNoVendido not in (select ProID from ClientesProductosVendidos where ProID = o.ProNoVendido and CliID = "+Arguments.Values.CurrentClient.CliID.ToString()+") " +
                "and "+cantidad.ToString()+" >= d.OfeCantidad order by d.OfeCantidad desc";

            return SqliteManager.GetInstance().Query<OfertasDetalle>(query, new string[] { OfeId.ToString() });
        }

        private List<OfertasDetalle> GetDetalleOfertaMancomunadaParaGrupoProductos(int ofeId, int titId)
        {
            var porcionGrpCodigo = "";
            var porcionTotalMancomunado = "";

            if (myParametro.GetParGrupoProductosJson())
            {
                porcionTotalMancomunado = "INNER JOIN Productos  GPD2 on T.ProID = GPD2.ProID " +
                    "AND  trim(O2.GrpCodigo) in (trim(ifnull(replace(replace(replace(replace(replace(GPD2.proGrupoProductos, '{\"grpcodigo\":', ''), '}', ''), '[', ''), ']', ''), '\"',''), '')) ) " +
                    "AND T.IndicadorOferta <>1";

                porcionGrpCodigo = "INNER JOIN Productos p on trim(o.GrpCodigo) in (trim(ifnull(replace(replace(replace(replace(replace(p.ProGrupoProductos, '{\"grpcodigo\":', ''), '}', ''), '[', ''), ']', ''), '\"',''), '')) )   AND   pt.ProID = p.ProID ";
            }
            else
            {
                porcionTotalMancomunado = "INNER JOIN GrupoProductosDetalle  GPD2 on trim(GPD2.GrpCodigo) = trim(O2.GrpCodigo) AND   T.ProID in (GPD2.ProID) and T.IndicadorOferta <>1";
                porcionGrpCodigo = "INNER JOIN GrupoProductosDetalle GPD on trim(GPD.GrpCodigo) = trim(o.GrpCodigo) AND   pt.ProID in (GPD.ProID) "
                                   + "INNER JOIN Productos p on p.ProID = pt.ProID  ";
            }

            var  query =
                    "SELECT DISTINCT pt.ProID as ProID, p.ProDatos3 as ProDatos3, od.OfePrecio as OfePrecio, l.UnmCodigo as UnmCodigo, pt.Descripcion as ProDescripcion, od.OfeCantidadOferta as OfeCantidadOferta, "
                            + "l.LipPrecio as PrecioLista, od.ofecantidad as OfeCantidad, o.OfeCantidadMax as OfeCantidadMax, OfeCantidadMaximaTransaccion, "
                            + "od.OfePorciento AS OfePorciento, "
                            + "(select sum(T.Cantidad) from ProductosTemp T  "
                            +	"INNER JOIN Ofertas O2 on O2.OfeID = ? and T.TitID = "+titId.ToString()+" "
                            +	" "+porcionTotalMancomunado+") as TotalMancomunado "
                            +	"FROM ProductosTemp pt "
                            +	"INNER JOIN Ofertas o ON o.OfeID = ? and pt.TitID = "+titId.ToString()+" "
                            +	"INNER JOIN OfertasDetalle od on od.OfeID = o.OfeID "
                            +   porcionGrpCodigo + " "
                            +   "left join ListaPrecios l on l.ProID = p.ProID and ltrim(rtrim(l.LipCodigo)) = '"+Arguments.Values.CurrentClient.LiPCodigo+"' and (ltrim(rtrim(l.MonCodigo)) = '"+Arguments.Values.CurrentClient.MonCodigo+"' or ifnull(l.MonCodigo, '') = '') "
                            +	"WHERE o.OfeTipo IN ('8', '9')  AND pt.IndicadorOferta <> 1 AND TotalMancomunado >= OfeCantidad  "
                            +	"GROUP BY pt.ProID "
                            +	"HAVING OfeCantidad = max(OfeCantidad) "
                            +	"ORDER BY OfeCantidad DESC ";

            return SqliteManager.GetInstance().Query<OfertasDetalle>(query, new string[] { ofeId.ToString(), ofeId.ToString() });

        }

        public List<OfertasDetalle> GetDetalleOfertaByCantidadMayor(int ofeId, int cantidad, string ofeTipo = null)
        {
            var where = " and " + cantidad + " >= cast(OfeCantidad as integer) ";

            if(!string.IsNullOrWhiteSpace(ofeTipo) && ofeTipo == "13")
            {
                //to do del calculo de la cantidad para poder comparar
                where = " and " + cantidad + " between OfeCantidad and OfeCantidadDetalle ";
            }

            return SqliteManager.GetInstance().Query<OfertasDetalle>("select " + cantidad + " as OfeCantidadMax, OfeCantidad, OfeCantidadDetalle, OfeCantidadDetalleOferta " +
                "OfeCantidadOferta, ifnull(OfePorciento, 0) as OfePorciento from OfertasDetalle where OfeID = ? "+where+" " +
                "order by OfeCantidad desc", new string[] { ofeId.ToString(), cantidad.ToString() });
        }

        public List<ProductosTemp> GetDetalleProductosOfertaMancomunada(string grpCodigo, int OfeID, int titId)
        {
            var sql = "";
            var porcionJoinGrpCodigo = "";
            var porcionWhereGrpCodigo = "";
            var JoinOfertasDetalle = "";
            var parOfertaMancomunada = myParametro.GetParOfertasMancomunadas();

            if (myParametro.GetParGrupoProductosJson())
            {
                porcionJoinGrpCodigo = "INNER JOIN Productos p  on (instr(trim(p.proGrupoProductos), '\"grpcodigo\":\"'||trim('" + grpCodigo + "') ) > 0)  ";

                if (parOfertaMancomunada == 1)
                {
                    porcionJoinGrpCodigo = "INNER JOIN Productos p on (instr(trim(p.proGrupoProductos), '\"grpcodigo\":\"'||trim('" + grpCodigo + "') ) > 0)  ";
                }
                else if (parOfertaMancomunada == 2)
                {
                    porcionJoinGrpCodigo = "INNER JOIN Productos p on (instr(trim(p.proGrupoProductos), '\"grpcodigo\":\"'||trim('" + grpCodigo + "') ) > 0)  " +
                                         "INNER JOIN ProductosTemp pt on pt.TitID = "+titId.ToString()+" and pt.ProID = p.ProID and ifnull(IndicadorOferta, 0) <> 1 ";
                }
            }
            else
            {
                if (parOfertaMancomunada == 1)
                {
                    porcionJoinGrpCodigo = "INNER JOIN GrupoProductosDetalle GPD on GPD.GrpCodigo = o.grpCodigoOferta " +
                                          "INNER JOIN Productos p on p.ProId = GPD.ProID ";
                }
                else if (parOfertaMancomunada == 2)
                {
                    porcionJoinGrpCodigo = "INNER JOIN GrupoProductosDetalle GPD on trim(GPD.GrpCodigo) = trim(o.grpCodigoOferta) " +
                                           "INNER JOIN ProductosTemp pt on pt.TitID = "+titId.ToString()+" and pt.ProID = GPD.ProID and ifnull(pt.IndicadorOferta, 0) <> 1 " +
                                           "INNER JOIN Productos p on p.proid = pt.ProID ";
                }
                porcionWhereGrpCodigo = " trim(GPD.GrpCodigo) = trim('" + grpCodigo + "')  and ";
            }

            if (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida())
            {
                    JoinOfertasDetalle = " Inner Join OfertasDetalle d on o.OfeID=d.OfeID ";
            }

            sql = "select o.OfeID as OfeID, t.ProIDOferta as ProIDOferta, p.ProDescripcion as Descripcion , " +
                        "p.ProItbis as Itbis, p.ProID as ProID, p.ProCodigo as ProCodigo , l.LipPrecio as Precio , '0' as Selectivo, ifnull(t.Cantidad, 0) as Cantidad, " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? " d.UnmCodigo " : " p.UnmCodigo ") + " as UnmCodigo, p.ProUnidades as ProUnidades " +
                        "from Ofertas o " +
                        " " + JoinOfertasDetalle + " " +
                        porcionJoinGrpCodigo + " " +
                        "left join ListaPrecios l on l.ProID = p.ProID and ltrim(rtrim(l.LipCodigo)) = '"+Arguments.Values.CurrentClient.LiPCodigo+"' and (ltrim(rtrim(l.MonCodigo)) = '"+Arguments.Values.CurrentClient.MonCodigo+"' or ifnull(l.MonCodigo, '') = '') " + 
                        "left join ProductosTemp t on t.TitID = "+titId.ToString()+" and t.ProID = p.ProID and t.OfeID = o.OfeID and ifnull(t.IndicadorOferta, 0) = 1 " +
                        "where " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? " l.UnmCodigo = d.UnmCodigo and " : " ") + " " + porcionWhereGrpCodigo + " o.OfeID = " + OfeID + " order by p.ProDescripcion";

            return SqliteManager.GetInstance().Query<ProductosTemp>(sql, new string[] { });
        }
        
        public List<ProductosTemp> CalcularOfertaParaProducto(Ofertas oferta, ProductosTemp producto, bool ParAceptarDecimales, DS_Productos myProd, int titId, out bool ProductosSinExistenciaVentas, EntregasRepartidorTransacciones Entrega = null, bool IsEditin = false, bool IsToverificar = false, List<ProductosTemp> productos = null)
        {
            ProductosSinExistenciaVentas = false;
            var detalle = new List<OfertasDetalle>();
            var productosAgregadosPorOferta = new List<ProductosTemp>();
            var inv = new DS_Inventarios(); 
            double totalVentasAcumuladas = 0.0;
            double totalOfertasAcumuladas = 0.0;
            var ventasAcumuladas = new List<VentasDetalle>();

            if (oferta.OfeTipo == "18" && producto.ProID != -1 && producto.Cantidad > 0)
            {
                DateTime.TryParse(oferta.OfeFechainicio, out DateTime desde);
                DateTime.TryParse(oferta.OfeFechaFin, out DateTime hasta);

                ventasAcumuladas = new DS_Ventas().GetDetalleByClienteyFechas(desde, hasta);
                totalVentasAcumuladas  = ventasAcumuladas.Where(v => v.ProID == producto.ProID && !v.VenindicadorOferta).Sum(x => x.VenCantidad);
                producto.Cantidad = producto.Cantidad + totalVentasAcumuladas;
            }

            if (oferta.OfeTipo == "1" || oferta.OfeTipo == "2" || oferta.OfeTipo == "7" || oferta.OfeTipo == "10" || oferta.OfeTipo == "11" || oferta.OfeTipo == "15" || oferta.OfeTipo == "5" || oferta.OfeTipo == "16" || oferta.OfeTipo == "18")
            {
                detalle = GetDetalleByOfeId(oferta, producto.ProID, producto.Cantidad, productoUnidad: (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? producto.UnmCodigo : ""));
            }
            else if (oferta.OfeTipo == "4")
            {
                detalle = GetDetalleOfertaProductosNoVendidos(oferta.OfeID, producto.ProID, producto.Cantidad);
            }
            else if (oferta.OfeTipo == "8")
            {
                if (myProd.ExistsOfertaInTemp(oferta.OfeID, titId))
                {
                    return null;
                }

                detalle = GetDetalleOfertaMancomunadaParaGrupoProductos(oferta.OfeID, titId);
            }

            double cantidadOferta = 0;
            double cantidadEscogida = 0;
            double cantidadOfertada = 0;
            double cantidadAdar = 0;
            double cantidadRestante = 0;

            var almIdDespacho = myParametro.GetParAlmacenIdParaDespacho();
            var almIdDevolucion = myParametro.GetParAlmacenIdParaDevolucion();
            var parVentasLote = myParametro.GetParVentasLote();
            var parVentasLoteAutomatico = myParametro.GetParVentasLotesAutomaticos();

            foreach (var ofeDetalle in detalle)
            {
                if (oferta.OfeTipo == "1" || oferta.OfeTipo == "4" || oferta.OfeTipo == "10" || oferta.OfeTipo == "5" || oferta.OfeTipo == "16" || oferta.OfeTipo == "18") //oferta normal
                {
                    cantidadAdar = 0;
                    cantidadOferta = 0;
                    cantidadEscogida = producto.Cantidad - cantidadRestante;
                    cantidadOfertada = ofeDetalle.OfeCantidad;
                    
                    if (cantidadEscogida >= cantidadOferta)
                    {
                        cantidadAdar = (oferta.OfeTipo == "16" ? (cantidadEscogida / cantidadOfertada) : (int)(cantidadEscogida / cantidadOfertada));
                        cantidadRestante += cantidadOfertada * cantidadAdar;
                        cantidadEscogida -= cantidadOfertada;
                    }
                    else
                    {
                        cantidadAdar = 0;
                    }

                    //sumando las ofertas que le tocan
                    if (cantidadEscogida >= 0 && oferta.OfeTipo != "16")
                    {
                        cantidadOferta += ofeDetalle.OfeCantidadOferta * (int)cantidadAdar;
                    }

                    if ((int)cantidadAdar > ofeDetalle.OfeCantidadOferta && oferta.OfeTipo == "4")
                    {
                        //cantidadOferta = ofeDetalle.OfeCantidadOferta - cantidadOfertada;
                    }

                    if (oferta.OfeTipo == "18")
                    {
                        totalOfertasAcumuladas = ventasAcumuladas.Where(v => v.ProID == ofeDetalle.ProID && v.VenindicadorOferta && v.OfeID == oferta.OfeID).Sum(x => x.VenCantidad);
                        cantidadOferta = cantidadOferta - totalOfertasAcumuladas;
                        if (cantidadOferta == 0)
                        {
                            cantidadAdar = 0;
                        }
                    }

                    if (oferta.OfeTipo == "16")
                    {
                        var residuo = cantidadAdar - (int)cantidadAdar;
                        cantidadOferta += (residuo > 0 ? 0 : ofeDetalle.OfeCantidadOferta * (int)cantidadAdar);
                        if (cantidadOferta == 0)
                        {
                            cantidadAdar = 0;
                        }
                    }

                }
                else if (oferta.OfeTipo == "2") //oferta porcentual
                {
                    cantidadOferta = producto.Cantidad * (ofeDetalle.OfePorciento / 100);
                    cantidadOferta = Math.Round(cantidadOferta, 2);
                    cantidadAdar = cantidadOferta;
                    cantidadEscogida = producto.Cantidad;
                }
                else if (oferta.OfeTipo == "7") //oferta multi detalle
                {
                    cantidadEscogida = producto.Cantidad;
                    cantidadOfertada = ofeDetalle.OfeCantidad;

                    if (cantidadEscogida >= cantidadOfertada)
                    {
                        cantidadAdar = (cantidadEscogida / cantidadOfertada);
                    }
                    else
                    {
                        cantidadAdar = 0;
                    }
                    //sumando las ofertas que le tocan

                    if (cantidadEscogida >= 0)
                    {
                        cantidadOferta = ofeDetalle.OfeCantidadOferta * Math.Round(cantidadAdar);

                        //var temp = myProd.ExistsProductoAgregadoPorOferta(ofeDetalle.ProID, titId);

                        //if (temp != null)
                        //{
                        //    if ((parVentasLote != 1 && parVentasLote != 2) || !ofeDetalle.UsaLote)
                        //    {
                        //        if (Arguments.Values.CurrentModule == Modules.VENTAS && !inv.HayExistencia(producto.ProID, cantidadOferta + temp.Cantidad, almId: almIdDevolucion != -1 && Entrega == null ? almIdDevolucion : almIdDespacho))
                        //        {
                        //            continue;

                        //        }
                        //    }

                        //    myProd.ActualizarCantidadProductoOferta(ofeDetalle.ProID, cantidadOferta, titId);

                        //    productos.Add(myProd.ExistsProductoAgregadoPorOferta(ofeDetalle.ProID, titId));

                        //    continue;
                        //}
                    }
                }
                else if (oferta.OfeTipo == "8") //mancomunada
                {
                    var cantidadProducto = 0;
                    int cantidadMancomunada = 0;
                    int cantidadProporcion = 0;
                    int ofeCantidadOferta = 0;
                    int ofeCantidadMax = ofeDetalle.OfeCantidadMax;

                    cantidadProducto = (int)producto.Cantidad;
                    cantidadMancomunada = (int)ofeDetalle.OfeCantidad;
                    ofeCantidadOferta = (int)ofeDetalle.OfeCantidadOferta;
                    cantidadProporcion = (cantidadProducto * ofeCantidadOferta) / cantidadMancomunada;
                    cantidadProporcion = (ofeCantidadMax > -1 && cantidadProporcion > ofeCantidadMax) ? ofeCantidadMax : cantidadProporcion;

                    cantidadEscogida = producto.Cantidad;
                    cantidadOferta = cantidadProporcion;
                    cantidadAdar = cantidadProporcion;

                }else if(oferta.OfeTipo == "11")
                {
                    int cantidadTemp = (int)producto.Cantidad;

                    cantidadAdar = (int)(cantidadTemp / ofeDetalle.OfeCantidadDetalleOferta) * ofeDetalle.OfeCantidadOferta;
                    cantidadEscogida = producto.Cantidad;
                    cantidadOferta = cantidadAdar;
                }
                else if (oferta.OfeTipo == "15") //oferta porcentual con redondeo hacia arriba Ej. Si la oferta es 0.99 la lleva al proximo numero entero.
                {
                    var residuo = 0.0;
                    
                    cantidadOferta = producto.Cantidad * (ofeDetalle.OfePorciento / 100);
                    if (myParametro.GetParRedondearOfertasDecimalMayorA5())
                    {
                        residuo = cantidadOferta - (int)cantidadOferta;
                    }
                    cantidadAdar = (residuo > 0.5 ? Math.Round((producto.Cantidad * (ofeDetalle.OfePorciento / 100))) : cantidadOferta);//cantidadOferta;
                    if (cantidadAdar != cantidadOferta)
                    {
                        cantidadOferta = cantidadAdar;
                    }
                    cantidadEscogida = producto.Cantidad;
                }

                if (cantidadAdar <= 0) //no oferta para este producto, seguir con el siguiente
                {
                    continue;
                }

               // double cantidadTotal = cantidadOferta + cantidadEscogida;

                /*if(!myInv.ComprobarInventario(proId, cantidadTotal) && VentasSAFE && !myParametro.GetParVentasRojoGas())
                 * {
                 *  cantidadOferta = 0;
                 *  noInventario = true;
                 *  
                 * }*/

                ProductosTemp productoOferta = new ProductosTemp();

                if (oferta.OfeTipo == "2" || oferta.OfeTipo == "15")
                {
                    if (ParAceptarDecimales)
                    {
                        productoOferta.Cantidad = cantidadOferta;
                    }
                    else
                    {
                        productoOferta.Cantidad = (int)cantidadOferta;
                    }

                }
                else if (ParAceptarDecimales)
                {
                    productoOferta.Cantidad = cantidadOferta;
                }
                else
                {
                    productoOferta.Cantidad = (int)cantidadOferta;
                }

                if ((parVentasLote != 1 && parVentasLote != 2 && parVentasLoteAutomatico < 1) || !ofeDetalle.UsaLote)
                {

                    Inventarios existencia = new Inventarios();
                    if (!producto.ProDatos3.Contains("x") && Arguments.Values.CurrentModule == Modules.VENTAS && !inv.HayExistencia(ofeDetalle.ProID, productoOferta.Cantidad + (ofeDetalle.ProID != producto.ProID? 0 : producto.CantidadOferta > 0 ? producto.Cantidad - producto.CantidadOferta : producto.Cantidad),out existencia, almId: almIdDevolucion > 0 && Entrega == null ? almIdDevolucion : almIdDespacho))
                    {
                        Device.InvokeOnMainThreadAsync(() =>
                        {
                            App.Current.MainPage.DisplayAlert(AppResource.Warning, "El producto: " + ofeDetalle.ProDescripcion + ", no tiene existencia suficiente para dar oferta. La cantidad en inventario es de: " + existencia.invCantidad + ". La oferta no se agregará a la venta.", AppResource.Aceptar);
                        });                        
                        continue;
                    }
                }

                if (oferta.OfeCaracteristicas.ToUpper().Contains("C") && Entrega != null)
                {
                    var NoEntregados = new DS_EntregasRepartidorTransacciones().GetProductosNoEntregadosxPedidos(Entrega.EnrSecuencia, Entrega.TraSecuencia, Entrega.TitID);

                    if (NoEntregados.Any(x => x.ProID == producto.ProID))
                    {
                        continue;
                    }
                }

                productoOferta.OfeDescripcion = oferta.OfeDescripcion + " - " + oferta.OfeID;
                productoOferta.TitID = titId;
                productoOferta.ProID = ofeDetalle.ProID;
                productoOferta.Precio = ofeDetalle.OfePrecio > 0? (ofeDetalle.OfePrecio * ofeDetalle.PrecioLista) / 100 : 0;
                productoOferta.ProIDOferta = producto.ProID;

                var item = myProd.GetById(ofeDetalle.ProID);

                productoOferta.Descripcion = item != null ? item.ProDescripcion : ofeDetalle.ProDescripcion;

                if(item != null)
                {
                    productoOferta.ProCodigo = item.ProCodigo;
                }

                productoOferta.Itbis = producto.Itbis;
                productoOferta.Selectivo = producto.Selectivo;
                productoOferta.Descuento = 0;
                productoOferta.PrecioTemp = ofeDetalle.PrecioLista;
                productoOferta.IndicadorOferta = true;
                productoOferta.DesPorcientoManual = 0;
                productoOferta.UnmCodigo = DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? ofeDetalle.UnmCodigo : producto.UnmCodigo;
                productoOferta.OfeID = oferta.OfeID;
                productoOferta.CantidadOferta = productoOferta.Cantidad;
                productoOferta.rowguid = Guid.NewGuid().ToString();
                productoOferta.ProDescripcionOferta = producto.Descripcion;
                productoOferta.ProDatos3 = ofeDetalle.ProDatos3;
                productoOferta.CantidadMaximaOferta = (int)oferta.OfeCantidadMaximaTransaccion;
                productoOferta.OfeCantidadMaximaTransaccion = oferta.OfeCantidadMaximaTransaccion;
                productoOferta.EnrSecuencia = producto.EnrSecuencia;
                productoOferta.TraSecuencia = producto.TraSecuencia;


                if(productoOferta.OfeCantidadMaximaTransaccion > 0 && productoOferta.Cantidad > productoOferta.OfeCantidadMaximaTransaccion)
                {
                    productoOferta.Cantidad = oferta.OfeCantidadMaximaTransaccion;
                    productoOferta.CantidadOferta = oferta.OfeCantidadMaximaTransaccion;
                }
                
                producto.OfeID = oferta.OfeID;

                if (!string.IsNullOrWhiteSpace(producto.UnmCodigo))
                {
                    //borra oferta faranch, buscar en MB standard
                }

                int pos = 0;
                bool firstTime = true;

                foreach (var prod in productosAgregadosPorOferta.ToList())
                {
                    if (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida())
                    {
                        if (prod.ProID == productoOferta.ProID && prod.UnmCodigo == productoOferta.UnmCodigo)
                        {
                            firstTime = false;

                            prod.Cantidad += productoOferta.Cantidad;
                            prod.CantidadOferta += productoOferta.Cantidad;
                            productosAgregadosPorOferta[pos] = prod;
                        }
                        else
                        {
                            firstTime = false;
                            productosAgregadosPorOferta.Add(productoOferta);
                        }
                        pos++;
                    }
                    else
                    {
                        if (prod.ProID == productoOferta.ProID)
                        {
                            firstTime = false;

                            prod.Cantidad += productoOferta.Cantidad;
                            prod.CantidadOferta += productoOferta.Cantidad;
                            productosAgregadosPorOferta[pos] = prod;
                        }
                        pos++;
                    }                    
                }


                if (firstTime)
                {
                    productosAgregadosPorOferta.Add(productoOferta);
                }

                if (oferta.OfeTipo == "2" || oferta.OfeTipo == "10" || oferta.OfeTipo == "15" || oferta.OfeTipo == "5" ) //no comprobar el siguiente
                {
                    break;
                }
            }


            if (IsToverificar)
            {
                return productosAgregadosPorOferta;
            }

            if (productosAgregadosPorOferta.Count > 0)
            {
                int ofeIdSustituir = GetTempOfeIDaSustituir(oferta.OfeID, producto.ProID, titId);

                if (ofeIdSustituir != -1)
                {
                    myProd.DeleteOfertaInTemp(titId, oferta.OfeID, producto.ProID, false, -1, (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? producto.UnmCodigo : ""));
                }

                bool isOfertaUnificada = false;
                double cantOfertaU = 0, cantTotalU = 0, totalSinDescuentoU = 0, totalLineaU = 0, descPorConOfertaU = 0, descConOfertaU = 0, precioConOferta = 0;

                isOfertaUnificada = (Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.VENTAS) && (oferta.OfeCaracteristicas.Contains('O') || oferta.OfeCaracteristicas.Contains('P'));
                
                myProd.InsertInTemp(productosAgregadosPorOferta, true, Entrega != null ? true : false, proID: producto.ProID, isOfertaUnificada: isOfertaUnificada);

                if (isOfertaUnificada)
                {
                    producto.DesPorciento= SqliteManager.GetInstance().Query<ProductosTemp>("select ifnull(DesPorciento, 0) as DesPorciento from ProductosTemp t " +
                        "where ProID = ? and TitID =? and ifnull(IndicadorOferta, 0) = 0 and rowguid = ? ", new string[] { producto.ProID.ToString(), titId.ToString(), producto.rowguid })[0].DesPorciento;

                    cantOfertaU = productosAgregadosPorOferta.Where(x => x.ProID == producto.ProID).Sum(x => x.Cantidad);
                    cantTotalU = producto.Cantidad + cantOfertaU;

                    string query;
                    if (cantOfertaU > 0 && oferta.OfeCaracteristicas.Contains('O'))
                    {
                        totalSinDescuentoU = producto.Precio * cantTotalU;
                        totalLineaU = (producto.Precio - (producto.Precio * producto.DesPorciento / 100)) * producto.Cantidad;
                        descPorConOfertaU = (1 - totalLineaU / totalSinDescuentoU) * 100;
                        descConOfertaU = producto.Precio * descPorConOfertaU / 100;

                        query = "update ProductosTemp set OfeCaracteristica = 'O', PedOfeCantidad = " + cantOfertaU + ", PrecioBase = " + producto.Precio + ", DesPorcientoOriginal = " + producto.DesPorciento + ", DesPorcientoManual = " + producto.DesPorciento + ", CantidadOferta = " + cantOfertaU + ", Descuento = " + descConOfertaU + ", DesPorciento = " + descPorConOfertaU + ", Cantidad = " + cantTotalU + ", OfeID = " + oferta.OfeID + " " +
                        "where ProID = ? and TitID = ? and ifnull(IndicadorOferta, 0) = 0 and rowguid = ? ";
                    }
                    else if(cantOfertaU > 0 && oferta.OfeCaracteristicas.Contains('P'))
                    {
                        precioConOferta = (producto.Precio * producto.Cantidad) / cantTotalU;
                        totalSinDescuentoU = precioConOferta * cantTotalU;
                        totalLineaU = (precioConOferta - (precioConOferta * producto.DesPorciento / 100)) * cantTotalU;
                        descPorConOfertaU = (1 - totalLineaU / totalSinDescuentoU) * 100;
                        descConOfertaU = precioConOferta * descPorConOfertaU / 100;

                        query = "update ProductosTemp set OfeCaracteristica = 'P', PedOfeCantidad = " + cantOfertaU + ", Precio = " + precioConOferta + ", PrecioBase = " + producto.Precio + ", DesPorcientoOriginal = " + producto.DesPorciento + ", DesPorcientoManual = " + producto.DesPorciento + ", CantidadOferta = " + cantOfertaU + ", Descuento = " + descConOfertaU + ", DesPorciento = " + descPorConOfertaU + ", Cantidad = " + cantTotalU + ", OfeID = " + oferta.OfeID + " " +
                        "where ProID = ? and TitID = ? and ifnull(IndicadorOferta, 0) = 0 and rowguid = ? ";
                    }
                    else
                    {
                        query = "update ProductosTemp set OfeID = " + oferta.OfeID + " " +
                        "where ProID = ? and TitID =? and ifnull(IndicadorOferta, 0) = 0 and rowguid = ? ";
                    }

                    SqliteManager.GetInstance().Execute(query, new string[] { producto.ProID.ToString(), titId.ToString(), producto.rowguid });
                }
                else
                {
                    SqliteManager.GetInstance().Execute("update ProductosTemp set OfeID = " + oferta.OfeID + " " +
                    "where ProID = ? and TitID =? and ifnull(IndicadorOferta, 0) = 0 and rowguid = ? ", new string[] { producto.ProID.ToString(), titId.ToString(), producto.rowguid });
                }

            }
            else
            {
                if (DS_RepresentantesParametros.GetInstance().GetParDeleteOfertasInTemp())
                {
                    myProd.DeleteOfertaInTemp(titId, oferta.OfeID, -1, false, -1);
                }
                else if (!DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida())
                {
                    myProd.DeleteOfertaInTemp(titId, oferta.OfeID, producto.ProID, false, -1);
                }
                
            }

            return productosAgregadosPorOferta;
        }

        public int GetTempOfeIDaSustituir(int ofeID, int proID, int titId)
        {
            int TempOfeID = -1;
            var sql = "Select pt.OfeID as OfeID " +
                    "from ProductosTemp pt " +
                    "inner join Ofertas o1 on o1.OfeID = pt.OfeID " +
                    "Inner join Ofertas o2 on o2.OfeID = " + ofeID + " " +
                    "where pt.IndicadorOferta = 1 and pt.TitID = "+titId.ToString()+" AND pt.ProIDOferta = " + proID + " " +
                    "AND o1.OfeNivel > o2.ofeNivel";

            try
            {
                var list = SqliteManager.GetInstance().Query<Ofertas>(sql, new string[] { });

                if(list != null && list.Count > 0)
                {
                    return list[0].OfeID;
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return TempOfeID;


        }

        public void GuardarProductosValidosParaOfertas(int cliId, int tinId, EntregasRepartidorTransacciones entrega = null)
        {

            try
            {
                SqliteManager.GetInstance().Execute("delete from ProductosValidosOfertas", new string[] { });

                string porcionGrpCodigo = "";

                if (myParametro.GetParGrupoProductosJson())
                {
                    porcionGrpCodigo = " ((instr((select trim(proGrupoProductos) from Productos where ProID = p.ProID), '\"grpcodigo\":\"'||trim(o.GrpCodigo) ) > 0))";
                }
                else
                {
                    porcionGrpCodigo = " exists(select 1 from GrupoProductosDetalle where GrpCodigo = o.GrpCodigo and ProID = p.ProID) ";
                }

                var lipcodigo = myParametro.GetParSectores() >= 2 && Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.LipCodigo : Arguments.Values.CurrentClient != null && !string.IsNullOrEmpty(Arguments.Values.CurrentClient.LiPCodigo) ? Arguments.Values.CurrentClient.LiPCodigo : "Default";

                var useProPrecios = !string.IsNullOrWhiteSpace(lipcodigo) && (lipcodigo == "*P.ProPrecio*" || lipcodigo == "*P.ProPrecio2*" || lipcodigo == "*P.ProPrecio3*");
                bool parNoListaPrecios = myParametro.GetParNoListaPrecios();

                var query = "select distinct lpc.UnmCodigo, o.OfeID as OfeID, " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? " o.UnmCodigo, " : "") + " p.ProID as ProID, " + cliId.ToString() + " as CliID, CASE WHEN (ifnull(o.OfeCaracteristicas, '') not like '%N%'  or(ifnull(o.OfeCaracteristicas, '') like '%N%' and (ifnull(lpc.UnmCodigo, '') = '' or lpc.UnmCodigo not in (ifnull((select codigouso from UsosMultiples where codigogrupo = 'OfeUnmIgnor'), ''))))) then 1 ELSE 0 end as TieneOferta, 0 as TieneDescuento, o.OfeCaracteristicas as OfeCaracteristicas from Ofertas o " +
                            "inner join Productos p on "+(Arguments.Values.CurrentSector != null && !DS_RepresentantesParametros.GetInstance().GetParNoDarOfertaXSector() ? " (seccodigo = '" + Arguments.Values.CurrentSector.SecCodigo + "' or seccodigo is null)  and " : "" )+" ((ifnull(o.GrpCodigo, 0) <> 0 " +
                            "and " + porcionGrpCodigo + ") " +
                            "or(ifnull(o.ProID, 0) <> 0 and ifnull(o.ProID, 0) = p.ProID and (ifnull(o.GrpCodigo, '') = '' or ifnull(o.GrpCodigo, '0') = '0') and ifnull(o.OfeTipo, '') <> '3') or(ifnull(o.ProID, 0) = 0 and (ifnull(o.GrpCodigo, '') = '' or ifnull(o.GrpCodigo, '0') = '0'))) " +
                            "LEFT join OfertasDetalleTiposNegocio otn on otn.OfeID = o.OfeID and otn.TinID = " + tinId.ToString() + " "+
                           (Arguments.Values.CurrentModule == Modules.DEVOLUCIONES || Arguments.Values.CurrentModule == Modules.INVFISICO || useProPrecios || parNoListaPrecios || cliId == -1 ? "left" : "inner") + " join ListaPrecios lpc on " +                           
                         " " + (!string.IsNullOrWhiteSpace(myParametro.GetParUnidadesMedidasVendedorUtiliza()) ? " ifnull(lower(lpc.UnmCodigo), '') in (" + myParametro.GetParUnidadesMedidasVendedorUtiliza() + ") and " : "") + " lpc.ProID = P.ProID and lpc.LipCodigo = '" + lipcodigo.Replace("*", "") + "' " +
                           "where STRFTIME('%Y-%m-%d',  " + (entrega != null ? "'" + "'" + entrega.VenFecha + "'" + "'" : " DATETIME('NOW', 'localtime')") + ", '0.0 hours') BETWEEN STRFTIME('%Y-%m-%d', o.OfeFechaInicio) " +
                            " " + (myParametro.GetOfertaConDia() ? " AND STRFTIME('%Y-%m-%d', o.OfeFechaFin, '+1 day') " : " AND STRFTIME('%Y-%m-%d', o.OfeFechaFin) ") + " " +
                            "and((ifnull(o.GrcCodigo, '') <> '' and ifnull(o.GrcCodigo, '0') <> '0' and exists(select 1 from GrupoClientesDetalle where trim(GrcCodigo) = trim(o.GrcCodigo) and CliID = " + cliId.ToString() + ") ) " +
                            "or ifnull(o.CliID, 0) = " + cliId.ToString() + " "+(Arguments.Values.CurrentModule == Modules.PRODUCTOS && cliId == -1 ? " or (ifnull(o.CliID, 0) != -1 and ifnull(o.GrcCodigo,'-1') != '-1' ) " : "")+" or (ifnull(o.CliID, 0) = 0 and (ifnull(o.GrcCodigo, '') = '' or ifnull(o.GrcCodigo, '0') = '0'))) " +
                            "/*and exists(select 1 from OfertasDetalleTiposNegocio where OfeID = o.OfeID and TinID = " + tinId.ToString() + ") */ " +
                            "and not exists(select 1 from OfertasClientesExcepciones where OfeID = o.OfeID and CliID = " + cliId.ToString() + ") " +
                            "and (CASE WHEN (ifnull(o.OfeCaracteristicas, '') not like '%N%'  or(ifnull(o.OfeCaracteristicas, '') like '%N%' and (ifnull(lpc.UnmCodigo, '') = '' or lpc.UnmCodigo not in (ifnull((select codigouso from UsosMultiples where codigogrupo = 'OfeUnmIgnor'), ''))))) then 1 ELSE 0 end = 1) " +
                            "and (o.OfeTipo <> 4 or(o.OfeTipo = 4 and not exists(select 1 from ClientesProductosVendidos where ProID = o.ProNoVendido and CliID = " + cliId.ToString() + "))) ";

                var query2 = "select distinct t.UnmCodigo, t.ProID as ProID, " + cliId.ToString() + " as CliID, t.TieneOferta as TieneOferta, 0 as TieneDescuento , t.OfeCaracteristicas as OfeCaracteristicas " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? " , IFNULL(UnmCodigo,'') UnmCodigoOferta " : "") + ", 1 as TitId from ( " + query + " ) as t  GROUP by cliid, t.UnmCodigo, t.Proid ";

                var list = SqliteManager.GetInstance().Query<ProductosValidosOfertas>(query2, new string[] { });

                SqliteManager.GetInstance().InsertAll(list);

                bool noMarcarProductosValidosParaDescuento = myParametro.GetParDescuentosProductosNoMarcarValidosParaDescuento();
                if (!noMarcarProductosValidosParaDescuento && myParametro.GetParPedidosDescuentoManualGeneral() <= 0.0)
                {
                    if (myParametro.GetParDescuentosProductosMostrarPreview())
                    {
                        new DS_DescuentosRecargos().GuardarProductosValidosParaDescuentoPreview(cliId);
                    }
                    else
                    {
                        new DS_DescuentosRecargos().GuardarProductosValidosParaDescuento(cliId);
                    }
                }

            }
            catch(Exception ex)
            {
                if (ex.Message.Contains("Constraint"))
                {
                    throw new Exception("Error insertando valores duplicados en la tabla ProductosValidosOfertas.");
                }
                throw ex;
            }

        }

        public void GuardarProductosValidosParaOfertasPorSegmento(int cliId, int tinId, EntregasRepartidorTransacciones entrega = null)
        {

            try
            {


                var lipcodigo = myParametro.GetParSectores() >= 2 && Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.LipCodigo : Arguments.Values.CurrentClient != null && !string.IsNullOrEmpty(Arguments.Values.CurrentClient.LiPCodigo) ? Arguments.Values.CurrentClient.LiPCodigo : "Default";

                var useProPrecios = !string.IsNullOrWhiteSpace(lipcodigo) && (lipcodigo == "*P.ProPrecio*" || lipcodigo == "*P.ProPrecio2*" || lipcodigo == "*P.ProPrecio3*");


                SqliteManager.GetInstance().Execute("delete from ProductosValidosOfertas", new string[] { });

                string porcionGrpCodigo = "";

                if (myParametro.GetParGrupoProductosJson())
                {
                    porcionGrpCodigo = " ((instr((select trim(proGrupoProductos) from Productos where ProID = p.ProID), '\"grpcodigo\":\"'||trim(o.GrpCodigo) ) > 0))";
                }
                else
                {
                    porcionGrpCodigo = " exists(select 1 from GrupoProductosDetalle where GrpCodigo = o.GrpCodigo and ProID = p.ProID) ";
                }

                var query = "select distinct lpc.UnmCodigo, LipPrecio,  o.OfeID as OfeID, " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? " o.UnmCodigo, " : "") + " p.ProID as ProID, " + cliId.ToString() + " as CliID, CASE WHEN (ifnull(o.OfeCaracteristicas, '') not like '%N%'  or(ifnull(o.OfeCaracteristicas, '') like '%N%' and lpc.UnmCodigo not in (ifnull((select codigouso from UsosMultiples where codigogrupo = 'OfeUnmIgnor'), '')))) then 1 ELSE 0 end  as TieneOferta, 0 as TieneDescuento from Ofertas o " +
                            "inner join Productos p on " + (Arguments.Values.CurrentSector != null && !DS_RepresentantesParametros.GetInstance().GetParNoDarOfertaXSector() ? " (seccodigo = '" + Arguments.Values.CurrentSector.SecCodigo + "' or seccodigo is null) and " : "") + " ((ifnull(o.GrpCodigo, 0) <> 0 " +
                            "and " + porcionGrpCodigo + ") " +
                            "or(ifnull(o.ProID, 0) <> 0 and ifnull(o.ProID, 0) = p.ProID and (ifnull(o.GrpCodigo, '') = '' or ifnull(o.GrpCodigo, '0') = '0') and ifnull(o.OfeTipo, '') <> '3') or(ifnull(o.ProID, 0) = 0 and (ifnull(o.GrpCodigo, '') = '' or ifnull(o.GrpCodigo, '0') = '0'))) " +
                            "INNER join OfertasDetalleTiposNegocio otn on otn.OfeID = o.OfeID and otn.TinID = " + tinId.ToString() + " " +
                            (Arguments.Values.CurrentModule == Modules.DEVOLUCIONES || Arguments.Values.CurrentModule == Modules.INVFISICO || useProPrecios ? "left" : "inner") + " join ListaPrecios lpc on " +
                            " " + (!string.IsNullOrWhiteSpace(myParametro.GetParUnidadesMedidasVendedorUtiliza()) ? " ifnull(lower(lpc.UnmCodigo), '') in (" + myParametro.GetParUnidadesMedidasVendedorUtiliza() + ") and " : "") + " lpc.ProID = P.ProID and lpc.LipCodigo = '" + lipcodigo.Replace("*", "") + "' " +
                            "where STRFTIME('%Y-%m-%d',  " + (entrega != null ? "'" + entrega.VenFecha + "'" : " DATETIME('NOW', 'localtime')") + ", '0.0 hours') BETWEEN STRFTIME('%Y-%m-%d', o.OfeFechaInicio) " +
                            " "+ (myParametro.GetOfertaConDia() ? " AND STRFTIME('%Y-%m-%d', o.OfeFechaFin, '+1 day') " : " AND STRFTIME('%Y-%m-%d', o.OfeFechaFin) ") + " "+
                            "and((ifnull(o.GrcCodigo, '') <> '' and ifnull(o.GrcCodigo, '0') <> '0' and exists(select 1 from GrupoClientesDetalle where trim(GrcCodigo) = trim(o.GrcCodigo) and CliID = " + cliId.ToString() + ") ) " +
                            "or ifnull(o.CliID, 0) = " + cliId.ToString() + " or(ifnull(o.CliID, 0) = 0 and (ifnull(o.GrcCodigo, '') = '' or ifnull(o.GrcCodigo, '0') = '0'))) " +
                            "/*and exists(select 1 from OfertasDetalleTiposNegocio where OfeID = o.OfeID and TinID = " + tinId.ToString() + ") */ " +
                            " and (ifnull(o.OfeCaracteristicas, '') not like '%N%'  || (ifnull(o.OfeCaracteristicas, '') like '%N%' and lpc.UnmCodigo not in(ifnull((select codigouso from UsosMultiples where codigogrupo = 'OfeUnmIgnor'), '')))) " +
                            "and not exists(select 1 from OfertasClientesExcepciones where OfeID = o.OfeID and CliID = " + cliId.ToString() + ") " +
                            "and (o.OfeTipo <> 4 or(o.OfeTipo = 4 and not exists(select 1 from ClientesProductosVendidos where ProID = o.ProNoVendido and CliID = " + cliId.ToString() + "))) ";

                var query2 = "select distinct t.UnmCodigo,t.LipPrecio, t.ProID as ProID, " + cliId.ToString() + " as CliID, t.TieneOferta as TieneOferta, 0 as TieneDescuento " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? " , IFNULL(UnmCodigo,'') UnmCodigoOferta " : " , '' as UnmCodigoOferta") + ", 1 as TitId from ( " + query + " ) as t ";

                var list = SqliteManager.GetInstance().Query<ProductosValidosOfertas>(query2, new string[] { });

                SqliteManager.GetInstance().InsertAll(list);

                bool noMarcarProductosValidosParaDescuento = myParametro.GetParDescuentosProductosNoMarcarValidosParaDescuento();
                if (!noMarcarProductosValidosParaDescuento && myParametro.GetParPedidosDescuentoManualGeneral() <= 0.0)
                {
                    if (myParametro.GetParDescuentosProductosMostrarPreview())
                    {
                        new DS_DescuentosRecargos().GuardarProductosValidosParaDescuentoPreview(cliId);
                    }
                    else
                    {
                        new DS_DescuentosRecargos().GuardarProductosValidosParaDescuento(cliId);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Constraint"))
                {
                    throw new Exception("Error insertando valores duplicados en la tabla ProductosValidosOfertas.");
                }
                throw ex;
            }

        }

        public string GetTipoOferta(int OfeID)
        {
            string query = "Select OfeTipo Ofertas where OfeID = "+OfeID; 

            var list = SqliteManager.GetInstance().Query<Ofertas>(query, new string[] { });

            return list[0].OfeTipo;
        }

        public int GetCantidadInTemp(int proid)
        {
            string query = "Select cantidadDetalle from ProductosTemp where Proid = " + proid+ " and IndicadorOferta = 1 ";

            var list = SqliteManager.GetInstance().Query<ProductosTemp>(query, new string[] { });

            if (list.Count>0)
            {
                return (int)list[0].CantidadDetalle;
            }

            return 0;
        }

        public ProductosTemp CrearProductoParaOfertaMancomunada(int proId)
        {
            var list = SqliteManager.GetInstance().Query<ProductosTemp>("select ProDescripcion as Descripcion, p.ProItbis as Itbis, p.ProID as ProID, " +
                "p.UnmCodigo as UnmCodigo, p.ProUnidades as ProUnidades from " +
                "Productos p " +
                "left join ListaPrecios l on l.ProID = p.ProID and ltrim(rtrim(l.LipCodigo)) = '" + Arguments.Values.CurrentClient.LiPCodigo + "' and (ltrim(rtrim(l.MonCodigo)) = '" + Arguments.Values.CurrentClient.MonCodigo + "' or ifnull(l.MonCodigo, '') = '') " +
                "where p.ProID = ?", new string[] { proId.ToString() });

            if(list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }


        public List<OfertasDetalle> CalcularOfertaParaProducto(Ofertas oferta, ProductosTemp producto, DS_Productos myProd, int titId)
        {
            var detalle = new List<OfertasDetalle>();
            if (oferta.OfeTipo == "1" || oferta.OfeTipo == "2" || oferta.OfeTipo == "7" || oferta.OfeTipo == "10" || oferta.OfeTipo == "11" || oferta.OfeTipo == "15" || oferta.OfeTipo == "5" || oferta.OfeTipo == "16")
            {
                detalle = GetDetalleByOfeId(oferta, producto.ProID, producto.Cantidad, productoUnidad: (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? producto.UnmCodigo : ""));
            }
            else if (oferta.OfeTipo == "4")
            {
                detalle = GetDetalleOfertaProductosNoVendidos(oferta.OfeID, producto.ProID, producto.Cantidad);
            }
            else if (oferta.OfeTipo == "8")
            {
                if (myProd.ExistsOfertaInTemp(oferta.OfeID, titId))
                {
                    return null;
                }

                detalle = GetDetalleOfertaMancomunadaParaGrupoProductos(oferta.OfeID, titId);
            }

            return detalle;
        }

        public (List<(int, double)>, double) CrearProductoParaOfertaMancomunadaParaCalcular(string grpcodigo, int proid)
        {
            var list = SqliteManager.GetInstance().Query<ProductosTemp>($@"select (select SUM(ifnull(Cantidad, 0) + ifnull(CantidadDetalle, 0)
                                                    * ifnull(ProUnidades, 1)) from ProductosTemp  where ProID in (select ProID from GrupoProductosDetalle 
                                                    where grpCodigo = '{grpcodigo}') and (proid not in ({proid}) or IndicadorOferta = 1)) As CantidadToCalcular, ifnull(Cantidad, 0) as Cantidad, ProID from 
                                                    ProductosTemp where ProID in (select ProID from GrupoProductosDetalle 
                                                    where grpCodigo = '{grpcodigo}') and (proid not in ({proid}) or IndicadorOferta = 1) ", new string[] {});

            if (list != null && list.Count > 0)
            {
                return (list.Select(l => (l.ProID, l.Cantidad)).ToList(),list[0].CantidadToCalcular);
            }

            return (new List<(int,double)>(), 0.00);
        }

        public void InsertOfeIdInProductTemp(Ofertas ofe)
        {
            SqliteManager.GetInstance().Execute($@"update ProductosTemp set OfeID = {ofe.OfeID}
                                       where ProID in (select proid from GrupoProductosDetalle where grpCodigo = '{ofe.GrpCodigo}')
                                       and TitID = {(int)Arguments.Values.CurrentModule} and ifnull(IndicadorOferta, 0) = 0");
        }

    }
}
