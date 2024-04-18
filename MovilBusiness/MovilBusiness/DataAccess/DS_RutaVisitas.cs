using MovilBusiness.Configuration;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.model.Internal.Structs.Args;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace MovilBusiness.DataAccess
{
    public class DS_RutaVisitas : DS_Controller
    {

        public ObservableCollection<Clientes> GetClientes(ClientesArgs args)
        {
            string condition = " WHERE c.CliID > -1";


            switch (args.Estatus)
            {
                case FiltroEstatusVisitaClientes.PENDIENTES:
                    condition += " AND c.CliID not in (select CliID from Visitas where VisFechaEntrada like '" + Functions.CurrentDate("yyyy-MM-dd") + "%')";
                    break;
                case FiltroEstatusVisitaClientes.VISITADO:
                    condition += " AND c.CliID in (select CliID from Visitas where VisFechaEntrada like '" + Functions.CurrentDate("yyyy-MM-dd") + "%')";
                    break;
                case FiltroEstatusVisitaClientes.TODOS: break;
            }

            if (!Arguments.CurrentUser.IsAuditor)
            {
                if ((Arguments.CurrentUser.RepIndicadorSupervisor) && !string.IsNullOrWhiteSpace(args.RepCodigo) && myParametro.GetParTipoRelacionClientes() == 2)
                {
                    condition += " and (trim(c.RepCodigo) = '" + args.RepCodigo.Trim() + "' OR c.CliID in (select CliID from ClientesDetalle where CliID = c.CliID and trim(RepCodigo) = '" + args.RepCodigo.Trim() + "')) ";
                }
                else if ((!string.IsNullOrWhiteSpace(args.RepCodigo) && myParametro.GetParTipoRelacionClientes() == 1) || (Arguments.CurrentUser.RepIndicadorSupervisor))
                {
                    condition += " and trim(c.RepCodigo) = '" + args.RepCodigo.Trim() + "' ";
                }
            }

            string whereCondition = "";

            if (args.filter != null)
            {
                if (args.filter != null)
                {
                    whereCondition = Functions.DinamicFiltersGenerateScript(args.filter, args.SearchValue, args.secondFilter);
                }
            }

            int weekNumber = args.NumeroSemana;
            int dayNumber = args.DiaNumero;

            if (dayNumber == -1)
            {
                dayNumber = 6;
            }

            char[] diasSemana = new char[] { '_', '_', '_', '_', '_', '_', '_' };

            diasSemana[dayNumber] = '1';
            string semanaValues = new string(diasSemana);
            string newWhere = " AND R.RutSemana" + weekNumber.ToString() + " like '" + semanaValues + "' AND R.RepCodigo = '" + args.RepCodigo + "' ";
            
            var joinAtributosCliente = " left join Provincias p on p.proid = c.proid " +
                "left join UsosMultiples cat1 on cat1.CodigoUso = c.CliCat1 and trim(upper(cat1.CodigoGrupo)) = upper('CliCat1')" +
                "left join UsosMultiples cat2 on cat2.CodigoUso = c.CliCat2 and trim(upper(cat2.CodigoGrupo)) = upper('CliCat2') " +
                "left join UsosMultiples cat3 on cat3.CodigoUso = c.CliCat3 and trim(upper(cat3.CodigoGrupo)) = upper('CliCat3') " +
                "left join UsosMultiples tip1 on tip1.CodigoUso = c.CliTipoCliente1 and trim(upper(tip1.CodigoGrupo)) = upper('CliTipoCliente1') " +
                "left join UsosMultiples tip2 on tip2.CodigoUso = c.CliTipoCliente2 and trim(upper(tip2.CodigoGrupo)) = upper('CliTipoCliente2') " +
                "left join UsosMultiples tip3 on tip3.CodigoUso = c.CliTipoCliente3 and trim(upper(tip3.CodigoGrupo)) = upper('CliTipoCliente3') ";

            var parCliCat1 = myParametro.GetParCliCat1();
            var parCliCat2 = myParametro.GetParCliCat2();
            var parCliCat3 = myParametro.GetParCliCat3();
            var parCliTipo1 = myParametro.GetParCliTipoCliente1();
            var parCliTipo2 = myParametro.GetParCliTipoCliente2();
            var parCliTipo3 = myParametro.GetParCliTipoCliente3();


            var selectAtributosCliente = (!string.IsNullOrWhiteSpace(parCliCat1) ? "case when ifnull(c.CliCat1,'') <> '' then '" + parCliCat1 + ": ' || cat1.Descripcion else '' end" : "''") + " as CliCat1, " +
                (!string.IsNullOrWhiteSpace(parCliCat2) ? "case when ifnull(c.CliCat2, '') <> '' then '" + parCliCat2 + ": ' || cat2.Descripcion else '' end " : "''") + "as CliCat2, " +
                (!string.IsNullOrWhiteSpace(parCliCat3) ? "case when ifnull(c.CliCat3, '') <> '' then '" + parCliCat3 + ": ' || cat3.Descripcion else '' end" : "''") + " as CliCat3, " +
                (!string.IsNullOrWhiteSpace(parCliTipo1) ? "case when ifnull(c.CliTipoCliente1, '') <> '' then '" + parCliTipo1 + ": '||tip1.Descripcion else '' end " : "''") + " as CliTipoCliente1, " +
                (!string.IsNullOrWhiteSpace(parCliTipo2) ? "case when ifnull(c.CliTipoCliente2, '') <> '' then '" + parCliTipo2 + ": '||tip2.Descripcion else '' end " : "''") + " as CliTipoCliente2, " +
                (!string.IsNullOrWhiteSpace(parCliTipo3) ? "case when ifnull(c.CliTipoCliente3, '') <> '' then '" + parCliTipo3 + ": '||tip3.Descripcion else '' end " : "''") + " as CliTipoCliente3, ";

            var query = "select " + selectAtributosCliente + " c.CliContactoFechaNacimiento as CliContactoFechaNacimiento, CliIndicadorPresentacion,CliNombreComercial, trim(CliNombre) as CliNombre, ifnull(c.ConID, -1) as ConID, c.CliID as CliID, LipCodigoPM, cast(R.RutPosicion as integer) as CliRutPosicion, " +
                "CliCodigo, R.RepCodigo as RepCodigo, CliCalle, c.MonCodigo, IFNULL(CliFormasPago, '000000') as CliFormasPago, TiNID, " +
                "ifnull((select ifnull(VisEstatus, 5) from Visitas v where v.CliID = c.CliID and VisFechaEntrada like '" + Functions.CurrentDate("yyyy-MM-dd") + "%' order by cast(ifnull(VisEstatus, 5) as integer) desc), 5) as CliEstatusVisita, " +
                "ifnull(LiPCodigo, '') as LiPCodigo, CliTelefono, CliFax, c.SecCodigo as SecCodigo, CliContacto, c.ProID as ProID, c.MunID as MunID, CliRNC, CliIndicadorDeposito, " +
                "CliCorreoElectronico, CliPropietario, CliIndicadorOrdenCompra, " +
                "CliIndicadorDepositaFactura, cliSector, ifnull(CliLatitud, 0) as CliLatitud, c.CliDatosOtros as CliDatosOtros, " +
                "ifnull(CliLongitud, 0) as CliLongitud, CliIndicadorCheque, CliLimiteCredito, CliIndicadorExonerado, " +
                "CliPromedioPago, CliMontoUltimoCobro, CAST(replace(strftime('%m-%d-%Y', SUBSTR(CliFechaUltimoCobro,1,10)),' ','' ) as varchar) as CliFechaUltimoCobro, CliEstatus, CliCodigoDescuento, CliPromedioCompra, " +
                "CliTipoComprobanteFAC, CliTipoComprobanteNC, CAST(replace(strftime('%m-%d-%Y', SUBSTR(CliFechaUltimaVenta,1,10)),' ','' ) as varchar) as CliFechaUltimaVenta, CliVentasAnioAnterior, " +
                "CliMontoUltimaVenta, CliVentasAnioActual, CldDirTipo, " + (DS_RepresentantesParametros.GetInstance().GetParSACCliRNCCedula() ? " CliRNC as CliCedulaPropietario " : "ifnull(CliCedulaPropietario, '') as CliCedulaPropietario") + ", ifnull(c.CanID, '') as CanID " +
                "from clientes c " + joinAtributosCliente;
            var parEstado = myParametro.GetParRutaVisitasFechaEstado();
            var limit = "";

            string fullQuery;

            if (Arguments.CurrentUser.IsAuditor)
            {
                fullQuery = GetQueryRutaVisitasFechaAuditor(condition + whereCondition, args.RepCodigo, args.Estatus, args.RutFecha);

            }else if (myParametro.GetParRutaVisitaTipo() == 2 && args.Estatus == FiltroEstatusVisitaClientes.PENDIENTES)
            {
                if (myParametro.GetParRutaVisitasOnebyOne())
                {
                    limit = args.Estatus == FiltroEstatusVisitaClientes.PENDIENTES ? " limit 1 " : "";
                }
                var order = " order by cast(R.RutPosicion as integer) asc ";
                var aprobada = myParametro.GetParRutaVisitasFechaEstado() ? " and ifnull(R.RutEstado, '0') = '4' " : "";

                var lastWhere = "inner join RutaVisitasFecha R on R.CliID = c.CliID and trim(r.RepCodigo) = '" + args.RepCodigo + "' and RutFecha like '" + Functions.CurrentDate("yyyy-MM-dd") + "%' "
                    + condition + " " + whereCondition + aprobada;

                if (args.Estatus == FiltroEstatusVisitaClientes.PENDIENTES)
                {
                    var list = SqliteManager.GetInstance().Query<Clientes>((myParametro.GetParRutaVisitasOnebyOne() ? query + lastWhere + order + limit : query + lastWhere + order), new string[] { });

                    if (list == null || list.Count == 0)
                    {
                        if (myParametro.GetParRutaVisitasOnebyOne())
                        {
                            query += "inner join RutaVisitas R on R.CliID = c.CliID and trim(r.RepCodigo) = '" + args.RepCodigo + "' " +
                            condition + newWhere + " " + whereCondition + " " + order + limit;
                        }
                        else
                        {
                            query += "inner join RutaVisitas R on R.CliID = c.CliID and trim(r.RepCodigo) = '" + args.RepCodigo + "' " +
                            condition + newWhere + " " + whereCondition + " " + order;
                        }

                        fullQuery = query;
                    }
                    else
                    {
                        return new ObservableCollection<Clientes>(list);
                    }
                }
                else
                {
                    var newQuery = query + lastWhere + " union " + query + "inner join RutaVisitas R on R.CliID = c.CliID and trim(r.RepCodigo) = '" + args.RepCodigo + "' " +
                        condition + newWhere + " " + whereCondition + " " + order;
                    return new ObservableCollection<Clientes>(SqliteManager.GetInstance().Query<Clientes>(newQuery, new string[] { }));
                }
            }
            else
            {
                string tempQuery;
                if (myParametro.GetParRutaVisitaRepartidor() == 2)
                {
                    int dayNumberRep;
                    if (dayNumber == 0) { dayNumberRep = 4; } else { dayNumberRep = dayNumber - 1; }
                    char[] diasSemanaRep = new char[] { '_', '_', '_', '_', '_', '_', '_' };
                    diasSemanaRep[dayNumberRep] = '1';
                    string semanaValuesRep = new string(diasSemanaRep);

                    tempQuery = query +
                        " inner join RutaVisitas R on R.CliID = c.CliID and trim(R.RepCodigo) in  (select RepVendedor from RepresentantesVendedor rv WHERE rv.RepCodigo = '" + args.RepCodigo + "' ) and R.cliid not in (SELECT cliid from RutaVisitasFecha) AND R.RutSemana" + weekNumber.ToString() + " like '" + semanaValuesRep + "' " +
                        " inner join CuentasxCobrar cc on r.cliid = cc.cliid " +
                        condition + " " + whereCondition + " and c.CliID in (select CliID from CuentasxCobrar) union " +
                        query + " inner join RutaVisitasFecha R on R.CliID = c.CliID " + (parEstado ? " and ifnull(R.RutEstado, '0') = '4' " : "") + " and trim(R.RepCodigo) = '" + args.RepCodigo + "' " +
                        "and RutFecha like '" + args.RutFecha.ToString("yyyy-MM-dd") + "%' " + condition + " " + whereCondition + " order by cast(R.RutPosicion as integer) asc";

                }
                else
                {
                    tempQuery = query +
                        " inner join RutaVisitas R on R.CliID = c.CliID  and trim(R.RepCodigo) = '" + args.RepCodigo + "' " + newWhere + " " +
                        condition + " " + whereCondition + " union " +
                        query + " inner join RutaVisitasFecha R on R.CliID = c.CliID " + (parEstado ? " and ifnull(R.RutEstado, '0') = '4' " : "") + " and trim(R.RepCodigo) = '" + args.RepCodigo + "' " +
                        "and RutFecha like '" + args.RutFecha.ToString("yyyy-MM-dd") + "%' " + condition + " " + whereCondition + " order by cast(R.RutPosicion as integer) asc";

                }

                fullQuery = tempQuery;
            }

            return new ObservableCollection<Clientes>(SqliteManager.GetInstance().Query<Clientes>(fullQuery, new string[] { }));
        }

        private string GetQueryRutaVisitasFechaAuditor(string extraWhere, string repCodigo, FiltroEstatusVisitaClientes status, DateTime fecha)
        {
            var query = "select CliIndicadorPresentacion, CliNombreComercial, trim(CliNombre) as CliNombre, ifnull(c.ConID, -1) as ConID, c.CliID as CliID, LipCodigoPM, 0 as RutPosicion, " +
                "CliCodigo, R.RepCodigo as RepCodigo, CliCalle, c.MonCodigo, IFNULL(CliFormasPago, '000000') as CliFormasPago, TiNID, " +
                "ifnull((select ifnull(VisEstatus, 5) from Visitas v where v.CliID = c.CliID and VisFechaEntrada like '" + Functions.CurrentDate("yyyy-MM-dd") + "%' order by cast(ifnull(VisEstatus, 5) as integer) desc), 5) as CliEstatusVisita, " +
                "ifnull(LiPCodigo, '') as LiPCodigo, CliTelefono, CliFax, c.SecCodigo as SecCodigo, CliContacto, ProID, MunID, CliRNC, CliIndicadorDeposito, " +
                "CliCorreoElectronico, CliPropietario, CliIndicadorOrdenCompra, " +
                "CliIndicadorDepositaFactura, cliSector, ifnull(CliLatitud, 0) as CliLatitud, c.CliDatosOtros as CliDatosOtros, " +
                "ifnull(CliLongitud, 0) as CliLongitud, CliIndicadorCheque, CliLimiteCredito, CliIndicadorExonerado, " +
                "CliPromedioPago, CliMontoUltimoCobro, CAST(replace(strftime('%m-%d-%Y', SUBSTR(CliFechaUltimoCobro,1,10)),' ','' ) as varchar) as CliFechaUltimoCobro, CliEstatus, CliCodigoDescuento, CliPromedioCompra, " +
                "CliTipoComprobanteFAC, CliTipoComprobanteNC, CAST(replace(strftime('%m-%d-%Y', SUBSTR(CliFechaUltimaVenta,1,10)),' ','' ) as varchar) as CliFechaUltimaVenta, CliVentasAnioAnterior, " +
                "CliMontoUltimaVenta, CliVentasAnioActual, CldDirTipo, " + (DS_RepresentantesParametros.GetInstance().GetParSACCliRNCCedula() ? " CliRNC as CliCedulaPropietario " : "ifnull(CliCedulaPropietario, '') as CliCedulaPropietario") + " from clientes c ";
            //"inner join RutaVisitasFecha R on trim(R.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and R.CliID = c.CliID and R.RutFecha like '" + fecha.ToString("yyyy-MM-dd") + "%' " +
            // "" + extraWhere + " and c.CliID in (select CliID from ClientesDetalle where trim(RepCodigo) = '" + repCodigo + "' and CliID = c.CliID) order by cast(R.RutPosicion as integer) asc ";
                
            var where = "" + extraWhere + " and c.CliID in (select CliID from ClientesDetalle where trim(RepCodigo) = '" + repCodigo + "' and CliID = c.CliID) order by cast(R.RutPosicion as integer) asc ";
            var limit = "";

            if (myParametro.GetParRutaVisitaTipo() == 2 && status == FiltroEstatusVisitaClientes.PENDIENTES)
            {
                if (myParametro.GetParRutaVisitasOnebyOne())
                {
                    limit = status == FiltroEstatusVisitaClientes.PENDIENTES ? " limit 1 " : "";
                }
                // var order = " order by cast(R.RutPosicion as integer) asc ";
                var aprobada = myParametro.GetParRutaVisitasFechaEstado() ? " and ifnull(R.RutEstado, '0') = '4' " : "";

                query += "inner join RutaVisitasFecha R on R.CliID = c.CliID and trim(r.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and RutFecha like '" + Functions.CurrentDate("yyyy-MM-dd") + "%' "
                    + aprobada;

                /*var list = SqliteManager.GetInstance().Query<Clientes>((myParametro.GetParRutaVisitasOnebyOne() ? query + lastWhere + order + limit : query + lastWhere + order), new string[] { });

                if (list == null || list.Count == 0)
                {
                    if (myParametro.GetParRutaVisitasOnebyOne())
                    {
                        query += "inner join RutaVisitas R on R.CliID = c.CliID and trim(r.RepCodigo) = '" + args.RepCodigo + "' " +
                        condition + newWhere + " " + whereCondition + " " + order + limit;
                    }
                    else
                    {
                        query += "inner join RutaVisitas R on R.CliID = c.CliID and trim(r.RepCodigo) = '" + args.RepCodigo + "' " +
                        condition + newWhere + " " + whereCondition + " " + order;
                    }

                    fullQuery = query;
                }
                else
                {
                    return new ObservableCollection<Clientes>(list);
                }*/
            }
            else
            {
                //string tempQuery;
                var parEstado = myParametro.GetParRutaVisitasFechaEstado();

                if (myParametro.GetParRutaVisitaRepartidor() == 2)
                {
                    /*int dayNumberRep;
                    if (dayNumber == 0) { dayNumberRep = 4; } else { dayNumberRep = dayNumber - 1; }
                    char[] diasSemanaRep = new char[] { '_', '_', '_', '_', '_', '_', '_' };
                    diasSemanaRep[dayNumberRep] = '1';
                    string semanaValuesRep = new string(diasSemanaRep);

                    tempQuery = query +
                        " inner join RutaVisitas R on R.CliID = c.CliID and trim(R.RepCodigo) in  (select RepVendedor from RepresentantesVendedor rv WHERE rv.RepCodigo = '" + args.RepCodigo + "' ) and R.cliid not in (SELECT cliid from RutaVisitasFecha) AND R.RutSemana" + weekNumber.ToString() + " like '" + semanaValuesRep + "' " +
                        " inner join CuentasxCobrar cc on r.cliid = cc.cliid " +
                        condition + " " + whereCondition + " and c.CliID in (select CliID from CuentasxCobrar) union " +
                        query + " inner join RutaVisitasFecha R on R.CliID = c.CliID " + (parEstado ? " and ifnull(R.RutEstado, '0') = '4' " : "") + " and trim(R.RepCodigo) = '" + args.RepCodigo + "' " +
                        "and RutFecha like '" + args.RutFecha.ToString("yyyy-MM-dd") + "%' " + condition + " " + whereCondition + " order by cast(R.RutPosicion as integer) asc";
                    */
                    query += " inner join RutaVisitasFecha R on R.CliID = c.CliID " + (parEstado ? " and ifnull(R.RutEstado, '0') = '4' " : "") + " and trim(R.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' " +
                        "and RutFecha like '" + fecha.ToString("yyyy-MM-dd") + "%' ";
                }
                else
                {
                    /* tempQuery = query +
                         " inner join RutaVisitas R on R.CliID = c.CliID  and trim(R.RepCodigo) = '" + args.RepCodigo + "' " + newWhere + " " +
                         condition + " " + whereCondition + " union " +
                         query + " inner join RutaVisitasFecha R on R.CliID = c.CliID " + (parEstado ? " and ifnull(R.RutEstado, '0') = '4' " : "") + " and trim(R.RepCodigo) = '" + args.RepCodigo + "' " +
                         "and RutFecha like '" + args.RutFecha.ToString("yyyy-MM-dd") + "%' " + condition + " " + whereCondition + " order by cast(R.RutPosicion as integer) asc";
                    */
                    query += " inner join RutaVisitasFecha R on R.CliID = c.CliID " + (parEstado ? " and ifnull(R.RutEstado, '0') = '4' " : "") + " and trim(R.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' " +
                         "and RutFecha like '" + fecha.ToString("yyyy-MM-dd") + "%' ";
                }
            }

            query += where + limit;

            return query;
        }

        public List<RutaVisitasFecha> GetClientesSinAsignar(ClientesArgs args, DateTime fecha, bool resumen = false, bool forSave = false)
        {
            var whereCondition = "";

            if (args.filter != null)
            {
                whereCondition = Functions.DinamicFiltersGenerateScript(args.filter, args.SearchValue, args.secondFilter);
            }

            var where = "";
            string orderBY = " order by CliNombre ";
            if (resumen)
            {
                where = " and (a.RutFecha = '" + fecha.ToString("yyyy-MM-dd") + "' or r.RutFecha like '" + fecha.ToString("yyyy-MM-dd") + "%' or r1.RutFecha like '" + fecha.ToString("yyyy-MM-dd") + "%')";
                orderBY = " order by RutPosicion ";
            }
            
           

            /*if (!resumen || !parEstado)
            {
                where += " and c.CliID not in (select CliID from RutaVisitasFecha where CliID = c.CliID and r.RepCodigo = '"+Arguments.CurrentUser.RepCodigo+"' and RutFecha like '"+fecha.ToString("yyyy-MM-dd")+"%') ";
            }*/

           // var estado = parEstado ? " and ifnull(a.DeleteReal, 0) = 0 " : "";

            var query = "select CliIndicadorPresentacion, CliNombre, case ifnull(a.DeleteReal,0) when 1 then 0 else (case when a.RutFecha is not null or r.RutFecha is not null or r1.RutFecha is not null then 1 else 0 end ) end as IsAsignado, ifnull(r.RutEstado, 1) as RutEstado, ifnull(r.rowguid,r1.rowguid) as rowguid, " +
                "CliCodigo, c.CliID as CliID, CliCalle, c.CliDatosOtros as CliDatosOtros, '" + fecha.ToString("yyyy-MM-dd") + "' as RutFecha " +
                ", ifnull(a.Posicion,ifnull(r.RutPosicion, ifnull(r1.RutPosicion,0))) as RutPosicion " +
                "from Clientes c " +
                "left join AsignacionRutasTemp a on a.CliID = c.CliID /*and ifnull(a.DeleteReal, 0) = 0 */ and a.RutFecha = '" + fecha.ToString("yyyy-MM-dd") + "' " +
                "left join SolicitudRutaVisitasFecha r on r.CliID = c.CliID and r.RepCodigo = '" + args.RepCodigo + "' and r.RutFecha like '" + fecha.ToString("yyyy-MM-dd") + "%' " +
                "left join RutaVisitasFecha r1 on r1.CliID = c.CliID and r1.RepCodigo = '" + args.RepCodigo + "' and r1.RutFecha like '" + fecha.ToString("yyyy-MM-dd") + "%' " +
                "where c.CliID > -1 " +
                whereCondition + " " + where + " " + orderBY;

            if (forSave)
            {
                query = "select CliIndicadorPresentacion, CliNombre, a.DeleteReal as DeleteReal, case when a.RutFecha is not null or r.RutFecha is not null or r1.RutFecha is not null then 1 else 0 end as IsAsignado, ifnull(r.RutEstado, 1) as RutEstado, a.rowguidReal as rowguid, " +
                "CliCodigo, c.CliID as CliID, CliCalle, c.CliDatosOtros as CliDatosOtros, a.RutFecha as RutFecha " +
                "from Clientes c " +
                "inner join AsignacionRutasTemp a on a.CliID = c.CliID " +
                "left join SolicitudRutaVisitasFecha r on r.CliID = c.CliID and trim(r.RepCodigo) = '"+ (string.IsNullOrWhiteSpace(args.RepCodigo) ? Arguments.CurrentUser.RepCodigo : args.RepCodigo).Trim()+"' and r.RutFecha = a.RutFecha " +
                "left join RutaVisitasFecha r1 on r1.CliID = c.CliID and trim(r1.RepCodigo) = '" + (string.IsNullOrWhiteSpace( args.RepCodigo) ? Arguments.CurrentUser.RepCodigo : args.RepCodigo).Trim() + "' and r1.RutFecha = a.RutFecha " +
                "where ifnull(a.DeleteReal, 0) = 0 order by a.RutFecha, a.Posicion asc";
                //"where 1=1  order by a.RutFecha, ifnull(a.DeleteReal, 0) asc, a.Posicion asc";
            }

            return SqliteManager.GetInstance().Query<RutaVisitasFecha>(query, new string[] { });
        }

        public ClienteCalendarioVisitas GetCalendarioVisitas(int cliId)
        {
            var calendario = new ClienteCalendarioVisitas();

            try
            {

                var list = SqliteManager.GetInstance().Query<RutaVisitas>("select RutSemana1, RutSemana2, RutSemana3, RutSemana4, RutPosicion from RutaVisitas where CliID = ? ", new string[] { cliId.ToString() });

                string[] dias = new string[] { "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado", "Domingo" };

                if (list == null || list.Count == 0)
                {
                    return calendario;
                }

                var ruta = list[0];
                calendario.RutPosicion = ruta.RutPosicion;

                bool semana1First = true;
                bool semana2First = true;
                bool semana3First = true;
                bool semana4First = true;

                for (int i = 0; i < dias.Length; i++)
                {

                    if (ruta.RutSemana1[i] == '1')
                    {
                        if (semana1First)
                        {
                            semana1First = false;
                            calendario.DiasVisitaSemana1 = dias[i];
                        }
                        else
                        {
                            calendario.DiasVisitaSemana1 += ", " + dias[i];
                        }
                    }

                    if (ruta.RutSemana2[i] == '1')
                    {
                        if (semana2First)
                        {
                            semana2First = false;
                            calendario.DiasVisitaSemana2 = dias[i];
                        }
                        else
                        {
                            calendario.DiasVisitaSemana2 += ", " + dias[i];
                        }
                    }

                    if (ruta.RutSemana3[i] == '1')
                    {
                        if (semana3First)
                        {
                            semana3First = false;
                            calendario.DiasVisitaSemana3 = dias[i];
                        }
                        else
                        {
                            calendario.DiasVisitaSemana3 += ", " + dias[i];
                        }
                    }

                    if (ruta.RutSemana4[i] == '1')
                    {
                        if (semana4First)
                        {
                            semana4First = false;
                            calendario.DiasVisitaSemana4 = dias[i];
                        }
                        else
                        {
                            calendario.DiasVisitaSemana4 += ", " + dias[i];
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return calendario;

        }

        public bool HayVisitaProgramada(int CliId, string fecha)
        {
            string query = "select CliID from SolicitudRutaVisitasFecha where CliID = ? and RutFecha like '" + fecha + "%' and RepCodigo = ? " +
                                " union "+
                            " select CliID from RutaVisitasFecha where CliID = ? and RutFecha like '" + fecha + "%' and RepCodigo = ?";
            //return SqliteManager.GetInstance().Query<RutaVisitasFecha>("select CliID from SolicitudRutaVisitasFecha " +
            //    "where CliID = ? and RutFecha = ? and RepCodigo = ?", new string[] { CliId.ToString(), fecha, Arguments.CurrentUser.RepCodigo }).Count > 0;
            return SqliteManager.GetInstance().Query<RutaVisitasFecha>(query, new string[]
            {
                CliId.ToString(),   Arguments.CurrentUser.RepCodigo, CliId.ToString(),  Arguments.CurrentUser.RepCodigo
            }).Count > 0;
        }

        public void InsertInTemp(int cliId, string rutFecha, bool forDelete = false, string rowguid = null, int rutPosicion = 0)
        {
            Hash map = new Hash("AsignacionRutasTemp") { SaveScriptForServer = false };
            map.Add("CliID", cliId);
            map.Add("RutFecha", rutFecha);
            map.Add("DeleteReal", forDelete ? "1" : "0");
            map.Add("Posicion", rutPosicion == 0 ? GetMaxPosicionRutaFechaInTemp(rutFecha) + 1 : rutPosicion);

            string rowguidToUse;

            if (forDelete)
            {
                rowguidToUse = rowguid;
                map.Add("rowguidReal", rowguid);
            }
            else
            {
                rowguidToUse = string.IsNullOrWhiteSpace(rowguid) ? Guid.NewGuid().ToString() : rowguid;
                map.Add("rowguidReal", rowguidToUse);
            }

            SqliteManager.GetInstance().Execute("delete from AsignacionRutasTemp where rowguidReal = ?", new string[] { rowguidToUse });
            map.ExecuteInsert();
        }

        public bool ExistsInTemp(int cliId, string fecha)
        {
            return SqliteManager.GetInstance().Query<RutaVisitas>("select CliID from AsignacionRutasTemp where CliID = ? and RutFecha = ?", 
                new string[] { cliId.ToString(), fecha }).Count > 0;
        }

        public bool ExistsInReal(int cliId, string fecha)
        {
            string query = "select CliID from SolicitudRutaVisitasFecha where CliID = ? and RutFecha like '" + fecha + "%' and RepCodigo = ? " +
                                " union " +
                            " select CliID from RutaVisitasFecha where CliID = ? and RutFecha like '" + fecha + "%' and RepCodigo = ?";
            return SqliteManager.GetInstance().Query<RutaVisitas>(query,
               new string[] { cliId.ToString(),   Arguments.CurrentUser.RepCodigo.Trim(), cliId.ToString(), Arguments.CurrentUser.RepCodigo.Trim() }).Count > 0;
            //return SqliteManager.GetInstance().Query<RutaVisitas>("select CliID from SolicitudRutaVisitasFecha where CliID = ? and RutFecha = ? and trim(RepCodigo) = ?",
            //    new string[] { cliId.ToString(), fecha, Arguments.CurrentUser.RepCodigo.Trim() }).Count > 0;
        }

        public bool ExistsEnSolicitudRutaVisitaFecha(int cliId, string fecha)
        {
            string query = "select CliID from SolicitudRutaVisitasFecha where CliID = ? and RutFecha like '" + fecha + "%' and RepCodigo = ? ";
            return SqliteManager.GetInstance().Query<RutaVisitas>(query,
               new string[] { cliId.ToString(), Arguments.CurrentUser.RepCodigo.Trim(), cliId.ToString(), Arguments.CurrentUser.RepCodigo.Trim() }).Count > 0;
 
        }

        public void DeleteInTemp(int cliid, string rutFecha, string rowguid = null, int rutPosicion = 0)
        {
            // if (!ExistsInReal(cliid, rutFecha)) //si lo estoy eliminando de la tabla real
            if (ExistsInReal(cliid, rutFecha))
            {
                InsertInTemp(cliid, rutFecha, true, rowguid, rutPosicion);
                return;
            }

            /*if (myParametro.GetParRutaVisitasFechaEstado())
            {
                SqliteManager.GetInstance().Execute("update AsignacionRutasTemp set DeleteReal = 1 where CliID = " + cliid + " and RutFecha = '" + rutFecha + "'");
            }
            else
            {
                SqliteManager.GetInstance().Execute("delete from AsignacionRutasTemp where CliID = " + cliid + " and RutFecha = '" + rutFecha + "'");
            }*/

            SqliteManager.GetInstance().Execute("delete from AsignacionRutasTemp where CliID = " + cliid + " and RutFecha = '" + rutFecha + "'");

        }

        public void ClearTemp()
        {
            SqliteManager.GetInstance().Execute("delete from AsignacionRutasTemp");
        }

        public List<AsignacionRutasTemp> GetAsignacionInTemp()
        {
            return SqliteManager.GetInstance().Query<AsignacionRutasTemp>("select CliID, RutFecha, DeleteReal, rowguidReal from AsignacionRutasTemp", new string[] { });
        }

        public int GetMaxPosicionRutaFecha(string fecha)
        {
            var list = SqliteManager.GetInstance().Query<RutaVisitasFecha>("select ifnull(max(RutPosicion), 0) as RutPosicion from SolicitudRutaVisitasFecha " +
                "where ltrim(rtrim(RepCodigo)) = ? and RutFecha like '"+fecha+"%'", new string[] { Arguments.CurrentUser.RepCodigo.Trim() });

            if (list != null && list.Count > 0)
            {
                return list[0].RutPosicion;
            }
           

            return 0;
        }

        public int GetMaxPosicionUnionRutaFecha(string fecha)
        {
            string query = "select max(RutPosicion) RutPosicion from( " + 
                "select ifnull(max(RutPosicion), 0) as RutPosicion from SolicitudRutaVisitasFecha " +
                "where ltrim(rtrim(RepCodigo)) = ? and RutFecha like '" + fecha + "%'" +
                " union " +
                "select ifnull(max(RutPosicion), 0) as RutPosicion from RutaVisitasFecha " +
                "where ltrim(rtrim(RepCodigo)) = ? and RutFecha like '" + fecha + "%') as Posicion" ;
            var list = SqliteManager.GetInstance().Query<RutaVisitasFecha>(query, new string[] { Arguments.CurrentUser.RepCodigo.Trim(), Arguments.CurrentUser.RepCodigo.Trim() });

            if (list != null && list.Count > 0)
            {
                return list[0].RutPosicion;
            }


            return 0;
        }

        public int GetMaxPosicionRutaFechaInTemp(string fecha = null)
        {
            int posicion = 0;
            var where = "";

            if (!string.IsNullOrWhiteSpace(fecha))
            {
                where = " and RutFecha like '"+fecha+ "%'";
            }

            var list = SqliteManager.GetInstance().Query<RutaVisitasFecha>("select ifnull(max(Posicion), 0) as RutPosicion from AsignacionRutasTemp " +
                "where 1=1 " + where, new string[] { });

            if (list != null && list.Count > 0)
            {
                posicion = list[0].RutPosicion;                                
            }
            if (!string.IsNullOrWhiteSpace(fecha))
            {
                posicion = GetMaxPosicionUnionRutaFecha(fecha) + (posicion == 0 ? 1 : posicion);
            }

            return posicion;
        }

        public void GuardarRutaVisitaFromTemp()
        {

            var list = GetClientesSinAsignar(new ClientesArgs(), DateTime.Now, forSave: true);

            if (list == null || list.Count == 0)
            {
                throw new Exception("Aun no has agregado ningun relacionado!");
            }

            var lastDate = "";
            int position = 0;
            foreach (var a in list)
            {
                
                Hash map = new Hash("SolicitudRutaVisitasFecha");
                DateTime.TryParseExact(a.RutFecha, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha);

                if (a.DeleteReal)
                {
                    if (!string.IsNullOrWhiteSpace(a.rowguid) && a.RutEstado == "1")
                    {
                        map.ExecuteDelete("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and CliID = " + a.CliID + " " +
                            "and RutEstado = 1 and rowguid = '" + a.rowguid + "'");
                    }
                }
                else
                {
                    if(lastDate != fecha.ToString("yyyy-MM-dd"))
                    {
                        lastDate = fecha.ToString("yyyy-MM-dd");

                        position = GetMaxPosicionRutaFecha(a.RutFecha);
                    }
                    position += 5;
                    
                    map.Add("CliID", a.CliID);
                    map.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                    map.Add("RutEstado", 1);
                    map.Add("RutFecha", fecha.ToString("yyyy-MM-dd"));
                    map.Add("RutPosicion", position);
                   
                    map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                    map.Add("RutFechaActualizacion", Functions.CurrentDate());

                    if(ExistsEnSolicitudRutaVisitaFecha(a.CliID, fecha.ToString("yyyy-MM-dd")))
                    {
                        map.ExecuteUpdate("CliID = " + a.CliID + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and " +
                            "RutFecha = '" + fecha.ToString("yyyy-MM-dd") + "'");
                    }
                    else
                    {
                        map.Add("rowguid", Guid.NewGuid().ToString());
                        map.ExecuteInsert();
                    }
                    
                }
            }
        }

        public int GetNumeroSemana(DateTime fecha)
        {
            var list = SqliteManager.GetInstance().Query<RutaVisitasFecha>("Select SemNumeroSemana from SemanasAnios  where '" + fecha.ToString("yyyy-MM-dd") + "' between SemFechaInicio and  SemFechaFin Order by SemFechaInicio asc", new string[] { });

            if (list != null && list.Count > 0)
            {
                return list[0].SemNumeroSemana;
            }

            return 0;
        }

        public List<Clientes> GetClientesSinVisitar()
        {
            var useRutaVisitaFecha = myParametro.GetParRutaVisitaTipo() == 2;

            int CicloSemana = 4;

            int parCicloSemana = myParametro.GetParCiclosSemanas();
            if (parCicloSemana > 0)
            {
                CicloSemana = parCicloSemana;
            }

            int weekNumber = Functions.GetWeekOfMonth(DateTime.Now);//args.NumeroSemana;
            int dayNumber = (int)(DateTime.Now).DayOfWeek - 1;

            if (weekNumber > 4)
            {
                weekNumber = 4;
            }

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
            string where = " AND R.RutSemana" + weekNumber.ToString() + " like '" + semanaValues + "' AND trim(R.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' ";

            var query = "select distinct CliID from RutaVisitas R where R.CliID " +
                "not in (select distinct CliID from Visitas where VisFechaEntrada like '" + Functions.CurrentDate("yyyy-MM-dd") + "%') " + where;

            if (useRutaVisitaFecha)
            {
                query += " union select distinct CliID from RutaVisitasFecha " +
                    "where trim(RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' " +
                    "and RutFecha like '" + Functions.CurrentDate("yyyy-MM-dd") + "%'" + (myParametro.GetParRutaVisitasFechaEstado() ? " and ifnull(RutEstado, '0') = '4' " : "") + " " +
                    "and CliID not in (select CliID from Visitas where VisFechaEntrada like '" + Functions.CurrentDate("yyyy-MM-dd") + "%') ";
            }

            return SqliteManager.GetInstance().Query<Clientes>(query, new string[] { });
        }

        public void ActualizarPosicionInTemp(ObservableCollection<RutaVisitasFecha> clientes)
        {
           foreach(var data in clientes)
            {
                var index = clientes.IndexOf(data);

                SqliteManager.GetInstance().Execute("update AsignacionRutasTemp set Posicion = " + index + " " +
                    "where CliID = " + data.CliID + " and RutFecha = '"+data.RutFecha+"'", new string[] { });
            }
        }

        public List<Clientes> GetClientesAVisitar()
        {
            var useRutaVisitaFecha = myParametro.GetParRutaVisitaTipo() == 2;

            int CicloSemana = 4;

            int parCicloSemana = myParametro.GetParCiclosSemanas();
            if (parCicloSemana > 0)
            {
                CicloSemana = parCicloSemana;
            }

            int weekNumber = Functions.GetWeekOfMonth(DateTime.Now);//args.NumeroSemana;
            int dayNumber = (int)(DateTime.Now).DayOfWeek - 1;

            if (weekNumber > 4)
            {
                weekNumber = 4;
            }

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
            string where = " R.RutSemana" + weekNumber.ToString() + " like '" + semanaValues + "' AND trim(R.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' ";

            var query = "select distinct CliID from RutaVisitas R where " + where;

            if (useRutaVisitaFecha)
            {
                query += " union select distinct CliID from RutaVisitasFecha " +
                    "where trim(RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' " +
                    "and RutFecha like '" + Functions.CurrentDate("yyyy-MM-dd") + "%'" + (myParametro.GetParRutaVisitasFechaEstado() ? " and ifnull(RutEstado, '0') = '4' " : "") + " ";
            }

            return SqliteManager.GetInstance().Query<Clientes>(query, new string[] { });
        }
    }
}
