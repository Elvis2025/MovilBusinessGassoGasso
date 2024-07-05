using MovilBusiness.Configuration;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.model.Internal.Structs.Args;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Services;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace MovilBusiness.DataAccess
{
    public class DS_Clientes : DS_Controller
    {

        private static DS_Clientes Instance;

        public static DS_Clientes GetInstance()
        {
            if (Instance == null)
            {
                Instance = new DS_Clientes();
            }

            return Instance;
        }
        public ApiManager api { get; set; }


        public DS_Clientes()
        {
            api = ApiManager.GetInstance(new PreferenceManager().GetConnection().Url);
        }
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
                case FiltroEstatusVisitaClientes.TODOS:
                    break;
            }

            /*    if (args.RepCodigo != null && args.RepCodigo.Length > 0 && Arguments.CurrentUser.RepIndicadorSupervisor)
                {
                    if(Arguments.CurrentUser.TipoRelacionClientes == 2)
                    {
                        condition += " AND (c.CliID IN (Select CliID from ClientesDetalle where ltrim(rtrim(RepCodigo)) = '" + args.RepCodigo + "'))";
                    }
                    else
                    {
                        condition += " AND (ltrim(rtrim(c.RepCodigo)) = '" + args.RepCodigo + "')";
                    }             
                }*/

            string whereCondition = "";

            if (args.filter != null/* && !string.IsNullOrWhiteSpace(args.SearchValue)*/)
            {
                if (args.filter != null)
                {
                    whereCondition = Functions.DinamicFiltersGenerateScript(args.filter, args.SearchValue, args.secondFilter);
                }
            }

            var extraWhere = "";

            //var clientes = SqliteManager.GetInstance().Query<Clientes>("SELECT RepCodigo FROM Clientes");

           // var checkRepCodigo = clientes.Where(cliente => cliente.RepCodigo != null).Select(cliente => cliente.RepCodigo).ToList();

            var parRelacionCliente = myParametro.GetParTipoRelacionClientes();

            bool isValidRelacionCliente = false;

            if (!string.IsNullOrWhiteSpace(args.RepCodigo) && parRelacionCliente == 2)
                isValidRelacionCliente = true;
            else if ((!string.IsNullOrWhiteSpace(args.RepCodigo) && Arguments.CurrentUser.RepIndicadorSupervisor))
                extraWhere = " and (trim(c.RepCodigo) = '" + args.RepCodigo.Trim() + "' )";

           // if (checkRepCodigo.Count == 0 && parRelacionCliente == 1) 
           //     extraWhere = "";

            if(!myParametro.GetParListaClientesMostrarProspectos())
                extraWhere += "And ifnull(c.CliDatosOtros,'') not like '%P%' ";


            if(args.ProID > 0)
            {
                extraWhere += " and c.ProID = " + args.ProID + " ";
            }

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


            var selectAtributosCliente = (!string.IsNullOrWhiteSpace(parCliCat1) ? "case when ifnull(c.CliCat1,'') <> '' then '"+parCliCat1+": ' || cat1.Descripcion else '' end" : "''") + " as CliCat1, " +
                (!string.IsNullOrWhiteSpace(parCliCat2) ? "case when ifnull(c.CliCat2, '') <> '' then '"+parCliCat2+": ' || cat2.Descripcion else '' end " : "''" )+"as CliCat2, " +
                (!string.IsNullOrWhiteSpace(parCliCat3) ? "case when ifnull(c.CliCat3, '') <> '' then '"+parCliCat3+": ' || cat3.Descripcion else '' end" : "''" )+" as CliCat3, " +
                (!string.IsNullOrWhiteSpace(parCliTipo1) ? "case when ifnull(c.CliTipoCliente1, '') <> '' then '"+parCliTipo1+": '||tip1.Descripcion else '' end " : "''")+" as CliTipoCliente1, " +
                (!string.IsNullOrWhiteSpace(parCliTipo2) ? "case when ifnull(c.CliTipoCliente2, '') <> '' then '"+parCliTipo2+": '||tip2.Descripcion else '' end " : "''")+" as CliTipoCliente2, " +
                (!string.IsNullOrWhiteSpace(parCliTipo3) ? "case when ifnull(c.CliTipoCliente3, '') <> '' then '"+parCliTipo3+": '||tip3.Descripcion else '' end " : "''")+" as CliTipoCliente3, ";

            string query = "select "+ /*selectAtributosCliente*/ "" + " CliEncargadoPago, CliPaginaWeb, ltrim(rtrim(CliNombre)) as CliNombre, c.CliContactoFechaNacimiento as CliContactoFechaNacimiento, CliIndicadorPresentacion, ifnull(ConID, -1) as ConID, CliID,CliNombreComercial, " +
                "CliCodigo, c.RepCodigo, CliCalle, ifnull(MonCodigo, '') as MonCodigo, IFNULL(CliFormasPago, '000000') as CliFormasPago, LipCodigoPM, TiNID, " +
                "ifnull((select ifnull(VisEstatus, 5) from Visitas v where v.CliID = c.CliID and VisFechaEntrada like '" + Functions.CurrentDate("yyyy-MM-dd") + "%' order by cast(ifnull(v.VisEstatus, 5) as integer) desc), 5) as CliEstatusVisita, " +
                "ifnull(LiPCodigo, '') as LiPCodigo, CliTelefono, CliFax, CliContacto, c.ProID, MunID, CliRNC, CliIndicadorDeposito, " +
                "CliCorreoElectronico, CliPropietario, CliIndicadorOrdenCompra, " +
                "CliIndicadorDepositaFactura, cliSector, c.SecCodigo as SecCodigo, ifnull(CliLatitud, 0) as CliLatitud, c.CliDatosOtros as CliDatosOtros, " +
                "ifnull(CliLongitud, 0) as CliLongitud, CliIndicadorCheque, CliLimiteCredito, CliIndicadorExonerado, ifnull(CliRutPosicion, 0) as CliRutPosicion, " +
                "CliPromedioPago, CliMontoUltimoCobro, CAST(replace(strftime('%m-%d-%Y', SUBSTR(CliFechaUltimoCobro,1,10)),' ','' ) as varchar) as CliFechaUltimoCobro, CliEstatus, CliCodigoDescuento, CliPromedioCompra, " +
                "CliTipoComprobanteFAC, CliTipoComprobanteNC, CAST(replace(strftime('%m-%d-%Y', SUBSTR(CliFechaUltimaVenta,1,10)),' ','' ) as varchar) as CliFechaUltimaVenta, CliVentasAnioAnterior, ifnull(c.CanID, '') as CanID, ifnull(CliUrbanizacion, '') as CliUrbanizacion, " +
                "CliMontoUltimaVenta, CliVentasAnioActual,CldDirTipo,ifnull(CliRegMercantil,'') CliRegMercantil,  " + (DS_RepresentantesParametros.GetInstance().GetParSACCliRNCCedula() ? " CliRNC as CliCedulaPropietario " : "ifnull(CliCedulaPropietario, '') as CliCedulaPropietario") + "  " +
                "from clientes c " +
                //joinAtributosCliente +
                condition + " " + whereCondition + " "+(isValidRelacionCliente? $" and (trim(c.RepCodigo) = '{args.RepCodigo} ') " : " ") +" " + extraWhere + " " +
                " "+ (isValidRelacionCliente ? "" : " order by CliNombre") + " ";


            if(isValidRelacionCliente)
            {
                query += " Union All select DISTINCT "+ selectAtributosCliente + " CliEncargadoPago, CliPaginaWeb, ltrim(rtrim(CliNombre)) as CliNombre, c.CliContactoFechaNacimiento as CliContactoFechaNacimiento, CliIndicadorPresentacion, ifnull(c.ConID, -1) as ConID, c.CliID,CliNombreComercial, " +
                "c.CliCodigo, c.RepCodigo, CliCalle, ifnull(c.MonCodigo, '') as MonCodigo, IFNULL(c.CliFormasPago, '000000') as CliFormasPago, LipCodigoPM, TiNID, " +
                "ifnull((select ifnull(VisEstatus, 5) from Visitas v where v.CliID = c.CliID and VisFechaEntrada like '" + Functions.CurrentDate("yyyy-MM-dd") + "%' order by cast(ifnull(v.VisEstatus, 5) as integer) desc), 5) as CliEstatusVisita, " +
                "ifnull(c.LiPCodigo, '') as LiPCodigo, CliTelefono, CliFax, CliContacto, c.ProID, MunID, CliRNC, CliIndicadorDeposito, " +
                "CliCorreoElectronico, CliPropietario, CliIndicadorOrdenCompra, " +
                "CliIndicadorDepositaFactura, cliSector, c.SecCodigo as SecCodigo, ifnull(CliLatitud, 0) as CliLatitud, c.CliDatosOtros as CliDatosOtros, " +
                "ifnull(CliLongitud, 0) as CliLongitud, CliIndicadorCheque, CliLimiteCredito, c.CliIndicadorExonerado, ifnull(CliRutPosicion, 0) as CliRutPosicion, " +
                "CliPromedioPago, CliMontoUltimoCobro, CAST(replace(strftime('%m-%d-%Y', SUBSTR(CliFechaUltimoCobro,1,10)),' ','' ) as varchar) as CliFechaUltimoCobro, CliEstatus, CliCodigoDescuento, CliPromedioCompra, " +
                "CliTipoComprobanteFAC, CliTipoComprobanteNC, CAST(replace(strftime('%m-%d-%Y', SUBSTR(CliFechaUltimaVenta,1,10)),' ','' ) as varchar) as CliFechaUltimaVenta, CliVentasAnioAnterior, ifnull(c.CanID, '') as CanID, ifnull(CliUrbanizacion, '') as CliUrbanizacion, " +
                "CliMontoUltimaVenta, CliVentasAnioActual,CldDirTipo,ifnull(CliRegMercantil,'') CliRegMercantil,  " + (DS_RepresentantesParametros.GetInstance().GetParSACCliRNCCedula() ? " CliRNC as CliCedulaPropietario " : "ifnull(CliCedulaPropietario, '') as CliCedulaPropietario") + "  " +
                "from clientes c " +
                "inner join ClientesDetalle cd on cd.cliid = c.cliid "+ (Arguments.CurrentUser.RepIndicadorSupervisor ? " and trim(c.RepCodigo) = '" + args.RepCodigo + "' " : " ") + " " +
                "left join Provincias p2 on p2.proid = c.proid " +
                joinAtributosCliente +
                condition + " " + whereCondition + " " + extraWhere + " ";
            }
            
            return new ObservableCollection<Clientes>(SqliteManager.GetInstance().Query<Clientes>(query).OrderBy(c => c.CliNombre));
        }

        public double GetClienteLimiteCredito(int cliId)
        {
            string sql = "select ifnull(CliLimiteCredito, 0.0) as CliLimiteCredito from Clientes where CliID = ?";

            if (Arguments.CurrentUser.TipoRelacionClientes == 2)
            {
                sql = "select ifnull(limiteCredito, 0.0) as CliLimiteCredito from ClientesDetalle where CliID = ?";
            }

            List<Clientes> list = SqliteManager.GetInstance().Query<Clientes>(sql, new string[] { cliId.ToString() });

            if (list != null && list.Count > 0)
            {
                return list[0].CliLimiteCredito;
            }

            return 0;
        }

        public Clientes GetClienteById(int cliid)
        {
            var list = new ObservableCollection<Clientes>(SqliteManager.GetInstance().Query<Clientes>("select c.CliContactoFechaNacimiento as CliContactoFechaNacimiento, CliIndicadorPresentacion,CliNombreComercial, CliNombre, LipCodigoPM, ifnull(ConID, -1) as ConID, CliID, " +
               "CliCodigo, RepCodigo, trim(CliCalle) as CliCalle, ifnull(CliCasa, '') as CliCasa, ifnull(MonCodigo, '') as MonCodigo, CliContactoCelular, CliNombreEmisionCheques, CliEstimadoCompras, CliInventario, CliLimiteCreditoSolicitado, CliPropietarioDireccion, " +
               "CliPropietarioTelefono, CliPropietarioCelular, CliFamiliarNombre, CliFamiliarDireccion, CliFamiliarCedula, CliFamiliarTelefono, CliFamiliarCelular, IFNULL(CliFormasPago, '000000') as CliFormasPago, TiNID, CliCedulaPropietario, " +
               "ifnull((select ifnull(VisEstatus, 5) from Visitas v where v.CliID = c.CliID and VisFechaEntrada like '" + Functions.CurrentDate("yyyy-MM-dd") + "%' order by cast(ifnull(v.VisEstatus, 5) as integer) desc), 5) as CliEstatusVisita, " +
               "ifnull(LiPCodigo, '') as LiPCodigo, CliTelefono, CliFax, RutEntregaID, CliContacto, ProID, MunID, CliRNC, CliIndicadorDeposito, " +
               "ifnull(CliCorreoElectronico, '') as CliCorreoElectronico, CliPropietario, CliIndicadorOrdenCompra, SecCodigo, TerCodigo, CliTipoLocal, CliTipoCliente, " +
               "CliIndicadorDepositaFactura, cliSector, ifnull(CliLatitud, 0) as CliLatitud, c.CliDatosOtros, " +
               "ifnull(CliLongitud, 0) as CliLongitud, CliIndicadorCheque, CliLimiteCredito, CliIndicadorExonerado, ifnull(CliRutPosicion, 0) as CliRutPosicion, " +
               "CliPromedioPago, CliMontoUltimoCobro, CAST(replace(strftime('%m-%d-%Y', SUBSTR(CliFechaUltimoCobro,1,10)),' ','' ) as varchar) as CliFechaUltimoCobro, CliEstatus, CliCodigoDescuento, CliPromedioCompra, " +
               "CliTipoComprobanteFAC, CliTipoComprobanteNC, CAST(replace(strftime('%m-%d-%Y', SUBSTR(CliFechaUltimaVenta,1,10)),' ','' ) as varchar) as CliFechaUltimaVenta, CliVentasAnioAnterior, " +
               "CliMontoUltimaVenta, CliVentasAnioActual,ifnull(CliUrbanizacion, '') as CliUrbanizacion,ifnull(CliRegMercantil,'') CliRegMercantil, " + (DS_RepresentantesParametros.GetInstance().GetParSACCliRNCCedula() ? " CliRNC as CliCedulaPropietario " : "ifnull(CliCedulaPropietario, '') as CliCedulaPropietario") +
               ", ifnull(c.CanID, '') as CanID from clientes c " +
               "where c.CliID = ?  "+(!myParametro.GetParListaClientesMostrarProspectos()? " And ifnull(c.CliDatosOtros,'') not like '%P%' " : " ") + " " +
               "order by CliNombre", new string[] { cliid.ToString() }));

            if (list != null && list.Count > 0)
            {
                if (Arguments.CurrentUser != null && Arguments.CurrentUser.TipoRelacionClientes == 2)
                {
                    var detalle = GetDetalleFromCliente(cliid, Arguments.CurrentUser.RepCodigo.Trim());

                    if (detalle != null)
                    {
                        if (!string.IsNullOrWhiteSpace(detalle.LipCodigo))
                        {
                            list[0].LiPCodigo = detalle.LipCodigo;
                        }
                        if (detalle.ConID > 0)
                            list[0].ConID = detalle.ConID;
                    }
                }

                return list[0];
            }

            return null;
        }

        public Clientes GetClienteProspectoById(int cliid)
        {
            var list = new ObservableCollection<Clientes>(SqliteManager.GetInstance().Query<Clientes>("select c.CliContactoFechaNacimiento as CliContactoFechaNacimiento, CliIndicadorPresentacion,CliNombreComercial, CliNombre, LipCodigoPM, ifnull(ConID, -1) as ConID, CliID, " +
               "CliCodigo, RepCodigo, trim(CliCalle) as CliCalle, ifnull(CliCasa, '') as CliCasa, ifnull(MonCodigo, '') as MonCodigo, CliContactoCelular, CliNombreEmisionCheques, CliEstimadoCompras, CliInventario, CliLimiteCreditoSolicitado, CliPropietarioDireccion, " +
               "CliPropietarioTelefono, CliPropietarioCelular, CliFamiliarNombre, CliFamiliarDireccion, CliFamiliarCedula, CliFamiliarTelefono, CliFamiliarCelular, IFNULL(CliFormasPago, '000000') as CliFormasPago, TiNID, CliCedulaPropietario, " +
               "ifnull((select ifnull(VisEstatus, 5) from Visitas v where v.CliID = c.CliID and VisFechaEntrada like '" + Functions.CurrentDate("yyyy-MM-dd") + "%' order by cast(ifnull(v.VisEstatus, 5) as integer) desc), 5) as CliEstatusVisita, " +
               "ifnull(LiPCodigo, '') as LiPCodigo, CliTelefono, CliFax, RutEntregaID, CliContacto, ProID, MunID, CliRNC, CliIndicadorDeposito, " +
               "ifnull(CliCorreoElectronico, '') as CliCorreoElectronico, CliPropietario, CliIndicadorOrdenCompra, SecCodigo, TerCodigo, CliTipoLocal, CliTipoCliente, " +
               "CliIndicadorDepositaFactura, cliSector, ifnull(CliLatitud, 0) as CliLatitud, c.CliDatosOtros, " +
               "ifnull(CliLongitud, 0) as CliLongitud, CliIndicadorCheque, CliLimiteCredito, CliIndicadorExonerado, ifnull(CliRutPosicion, 0) as CliRutPosicion, " +
               "CliPromedioPago, CliMontoUltimoCobro, CAST(replace(strftime('%m-%d-%Y', SUBSTR(CliFechaUltimoCobro,1,10)),' ','' ) as varchar) as CliFechaUltimoCobro, CliEstatus, CliCodigoDescuento, CliPromedioCompra, " +
               "CliTipoComprobanteFAC, CliTipoComprobanteNC, CAST(replace(strftime('%m-%d-%Y', SUBSTR(CliFechaUltimaVenta,1,10)),' ','' ) as varchar) as CliFechaUltimaVenta, CliVentasAnioAnterior, " +
               "CliMontoUltimaVenta, CliVentasAnioActual,ifnull(CliUrbanizacion, '') as CliUrbanizacion,ifnull(CliRegMercantil,'') CliRegMercantil, " + (DS_RepresentantesParametros.GetInstance().GetParSACCliRNCCedula() ? " CliRNC as CliCedulaPropietario " : "ifnull(CliCedulaPropietario, '') as CliCedulaPropietario") +
               ", ifnull(c.CanID, '') as CanID from clientes c " +
               "where c.CliID = ?  And ifnull(c.CliDatosOtros,'') like '%P%' " +
               "order by CliNombre", new string[] { cliid.ToString() }));

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public Clientes GetClienteByVisita(int visSecuencia)
        {
            var list = new ObservableCollection<Clientes>(SqliteManager.GetInstance().Query<Clientes>("select c.CliContactoFechaNacimiento as CliContactoFechaNacimiento, CliIndicadorPresentacion, CliNombreComercial, CliNombre, LipCodigoPM, ifnull(ConID, -1) as ConID, c.CliID as CliID, " +
               "c.CliCodigo, c.RepCodigo as RepCodigo, trim(c.CliCalle) as CliCalle, ifnull(MonCodigo, '') as MonCodigo, CliContactoCelular, CliNombreEmisionCheques, CliEstimadoCompras, CliInventario, CliLimiteCreditoSolicitado, CliPropietarioDireccion, " +
               "CliPropietarioTelefono, CliPropietarioCelular, CliFamiliarNombre, CliFamiliarDireccion, CliFamiliarCedula, CliFamiliarTelefono, CliFamiliarCelular, IFNULL(CliFormasPago, '000000') as CliFormasPago, TiNID, CliCedulaPropietario, " +
               "ifnull((select ifnull(VisEstatus, 5) from Visitas v where v.CliID = c.CliID and VisFechaEntrada like '" + Functions.CurrentDate("yyyy-MM-dd") + "%' order by cast(ifnull(v.VisEstatus, 5) as integer) desc), 5) as CliEstatusVisita, " +
               "ifnull(c.LiPCodigo, '') as LiPCodigo, CliTelefono, CliFax, RutEntregaID, CliContacto, ProID, MunID, CliRNC, CliIndicadorDeposito, " +
               "CliCorreoElectronico, CliPropietario, CliIndicadorOrdenCompra, c.SecCodigo, TerCodigo, CliTipoLocal, CliTipoCliente, " +
               "CliIndicadorDepositaFactura, cliSector, ifnull(CliLatitud, 0) as CliLatitud, CliDatosOtros, " +
               "ifnull(CliLongitud, 0) as CliLongitud, CliIndicadorCheque, CliLimiteCredito, CliIndicadorExonerado, ifnull(CliRutPosicion, 0) as CliRutPosicion," +
               "CliPromedioPago, CliMontoUltimoCobro, CAST(replace(strftime('%m-%d-%Y', SUBSTR(CliFechaUltimoCobro,1,10)),' ','' ) as varchar) as CliFechaUltimoCobro, CliEstatus, CliCodigoDescuento, CliPromedioCompra, " +
               "CliTipoComprobanteFAC, CliTipoComprobanteNC, CAST(replace(strftime('%m-%d-%Y', SUBSTR(CliFechaUltimaVenta,1,10)),' ','' ) as varchar) as CliFechaUltimaVenta, CliVentasAnioAnterior, " +
               "CliMontoUltimaVenta, CliVentasAnioActual, CliIndicadorCredito from clientes c " +
               "inner join Visitas s on s.VisSecuencia = ? and s.CliID = c.CliID and ltrim(rtrim(s.RepCodigo)) = ? " +
               "where 1 = 1  " + (!myParametro.GetParListaClientesMostrarProspectos() ? " And ifnull(c.CliDatosOtros, '') not like '%P%' " : " ") + "" +
               "order by CliNombre", new string[] { visSecuencia.ToString(), Arguments.CurrentUser.RepCodigo }));

            if (list != null && list.Count > 0)
            {
                if (Arguments.CurrentUser != null && Arguments.CurrentUser.TipoRelacionClientes == 2)
                {
                    var detalle = GetDetalleFromCliente(list[0].CliID, Arguments.CurrentUser.RepCodigo.Trim());

                    if (detalle != null)
                    {
                        if (!string.IsNullOrWhiteSpace(detalle.LipCodigo))
                        {
                            list[0].LiPCodigo = detalle.LipCodigo;
                        }
                        if (detalle.ConID > 0)
                            list[0].ConID = detalle.ConID;
                    }
                }

                return list[0];
            }

            return null;
        }

        public List<Clientes> GetAll()
        {
            try
            {
                return SqliteManager.GetInstance().Query<Clientes>("select CliIndicadorPresentacion,CliNombreComercial, CliCodigo, CliNombre, LipCodigoPM, CliID, LiPCodigo, SecCodigo from Clientes where CliEstatus = ? order by ltrim(rtrim(CliNombre))", new string[] { "1" });
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return new List<Clientes>();
            }
        }

        public List<Clientes> GetAllClientesRutPosicion()
        {
            try
            {
                string query = "";

                if(App.Current.Properties.ContainsKey("CurrentRep"))
                {
                    query = " inner join clientesdetalle cd on cd.cliid = c.cliid and cd.repcodigo = " + App.Current.Properties["CurrentRep"].ToString() + " ";
                }

                var list = SqliteManager.GetInstance().Query<Clientes>("select rutsemana1 as CliRutSemana1, rutsemana2 as CliRutSemana2, rutsemana3 as CliRutSemana3, rutsemana4 as CliRutSemana4, c.cliID, ifnull(CliRutPosicion,0) CliRutPosicion, ifnull(c.CliOrdenRuta,0) CliOrdenRuta, RutEntregaID, (ifnull(CliRutPosicion,'0') || '/' || trim(c.CliCodigo)  || '/' || trim(CliNombre)) CliNombre from Clientes c " + query + " left join RutaVisitas r on r.cliid = c.cliid GROUP by(c.cliid) order by CliOrdenRuta, CliRutPosicion ");

                    if (myParametro.GetParMostrarClientesPosMayorAcero())
                    {                        
                        list = list.Where(i => i.CliRutPosicion > 0 || i.CliOrdenRuta > 0).ToList();
                    }

                //int cliEnumerator = 1;
                //foreach (var cli in list)
                //{
                //    cli.CliEnumerator = cliEnumerator++;
                //}
                return list;
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return new List<Clientes>();
            }
        }

        public string GetClienteCedCodigoFromDetalleBySector(int cliId, string sector)
        {
            List<Clientes> list = SqliteManager.GetInstance().Query<Clientes>("select CedCodigo from ClientesDetalle where CliID = ? and SecCodigo = ? limit 1", new string[] { cliId.ToString(), sector });

            if (list != null && list.Count > 0)
            {
                return list[0].CedCodigo;
            }

            return "";
        }



        public double GetMontoPedidoSugerido(int cliId)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<Totales>("select CliMontoPedidoSugerido as Total from ClientesDetalle where CliID = ? and ltrim(rtrim(RepCodigo)) = ? ", new string[] { cliId.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

                if (list != null && list.Count > 0)
                {
                    return list[0].Total;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return 0;
        }

        public string GetMonCodigoByClienteRelacion(int cliId)
        {
            try
            {
                string query;

                if (Arguments.CurrentUser.TipoRelacionClientes == 2)
                {
                    query = "select ifnull(MonCodigo, '') as MonCodigo from ClientesDetalle where CliID = ? and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'";
                }
                else
                {
                    query = "select ifnull(MonCodigo, '') as MonCodigo from Clientes where CliID = ? ";
                }

                var list = SqliteManager.GetInstance().Query<Clientes>(query, new string[] { cliId.ToString() });

                if (list != null && list.Count > 0)
                {
                    return list[0].MonCodigo;
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return null;
        }

        public List<ProductosTemp> GetListaProductosProximosAVencer(int cliId)
        {
            var query = "select p.ProCodigo as ProCodigo, p.ProDescripcion as Descripcion, c.CFPLLote as Lote, c.CFPLFactura as Documento, ifnull(replace( strftime('%d-%m-%Y', SUBSTR(c.CFPLFechaVencimiento,1,10)),' ','' ), '') as FechaVencimiento," +
                "sum(c.CFPLCantidadVendida) as Cantidad from Productos p " +
                "inner join ClientesFacturasProductosLotes c on c.ProID = p.ProID " +
                "where c.Cliid = ? ";

            var groupBy = myParametro.GetParProductosProximosAVencerGroupByColumnas();
            var orderBy = myParametro.GetParProductosProximosAVencerOrderByColumnas();

            var orderByQuery = "";
            var groupByQuery = "";

            if (!string.IsNullOrWhiteSpace(groupBy))
            {
                groupByQuery = GenerateGroupByFromParams(groupBy);
            }

            if (string.IsNullOrWhiteSpace(groupByQuery))
            {
                groupByQuery = " group by p.ProCodigo, p.ProDescripcion, c.CFPLLote, p.ProID ";
            }

            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                orderByQuery = GenerateOrderByFromParams(orderBy);
            }

            query += groupByQuery;

            if (!string.IsNullOrWhiteSpace(orderByQuery))
            {
                query += orderByQuery;
            }

            return SqliteManager.GetInstance().Query<ProductosTemp>(query, new string[] { cliId.ToString() });
        }

        private string GenerateGroupByFromParams(string parameter)
        {
            string columnNumber = "";

            switch (parameter)
            {
                case "1":
                    columnNumber = " group by p.ProCodigo, p.ProDescripcion, c.CFPLFactura, c.CFPLCantidadVendida, c.CFPLLote, c.CFPLFechaVencimiento ";
                    break;
                case "2":
                    columnNumber = " group by p.ProDescripcion, p.ProCodigo, c.CFPLFactura, c.CFPLCantidadVendida, c.CFPLLote, c.CFPLFechaVencimiento ";
                    break;
                //case "3": columnNumber = "X.CFPLFactura, P.ProCodigo, P.ProDescripcion, X.CFPLCantidadVendida, X.CFPLLote, X.CFPLFechaVencimiento ";
                //	break;
                case "4":
                    columnNumber = " group by p.ProCodigo, p.ProDescripcion, c.CFPLLote, p.ProID ";
                    break;
                case "5":
                    columnNumber = " group by c.CFPLCantidadVendida, p.ProCodigo, p.ProDescripcion, c.CFPLFactura, c.CFPLLote, c.CFPLFechaVencimiento ";
                    break;
                case "6":
                    columnNumber = " group by c.CFPLFechaVencimiento, p.ProCodigo, p.ProDescripcion, c.CFPLFactura, c.CFPLCantidadVendida, c.CFPLLote ";
                    break;

            }

            return columnNumber;
        }

        private string GenerateOrderByFromParams(string parameter)
        {
            string[] pos;
            pos = parameter.Split('|');
            string columnNumber = pos[0];
            string split = parameter.Replace(pos[0], " ");

            switch (pos[0])
            {
                case "1":
                    columnNumber = "Order by p.ProCodigo" + split.Replace("|", " ");
                    break;
                case "2":
                    columnNumber = "Order by p.ProDescripcion" + split.Replace("|", " ");
                    break;
                //case "3": columnNumber = "X.CFPLFactura"+split.replace("|"," ");
                //	break;
                case "4":
                    columnNumber = "Order by c.CFPLLote" + split.Replace("|", " ");
                    break;
                case "5":
                    columnNumber = "Order by c.CFPLCantidadVendida" + split.Replace("|", " ");
                    break;
                case "6":
                    columnNumber = "Order by c.CFPLFechaVencimiento" + split.Replace("|", " ");
                    break;

            }

            return columnNumber;
        }

        public NCF GetSiguienteNCF(Clientes cliente, bool forNC = false)
        {
            List<RepresentantesDetalleNCF2018> list;

            if (forNC)
            {
                list = SqliteManager.GetInstance().Query<RepresentantesDetalleNCF2018>("SELECT ReDSerie, ReDNCFActual+1 as ReDNCFActual, ReDFechaVencimiento, ReDNCFMax, rowguid " +
                "from "+ (myParametro.GetParTakeFromNCF2021() ? "RepresentantesDetalleNCF2021" : "RepresentantesDetalleNCF2018") + "  where cast(strftime('%Y%m%d',ReDFechaVencimiento) as integer) " +
                "BETWEEN cast(strftime('%Y%m%d',DateTime('now')) as integer) and cast(strftime('%Y%m%d',ReDFechaVencimiento) as integer) " +
                "AND ReDTipoComprobante = ? and ltrim(rtrim(RepCodigo)) = ? and ReDNCFActual+1 <= ReDNCFMax " +
                "order by ReDNCFMax asc", new string[] { cliente.CliTipoComprobanteNC, Arguments.CurrentUser.RepCodigo.Trim() });
            }
            else if (cliente.CliTipoComprobanteFAC == "01")
            {
                list = SqliteManager.GetInstance().Query<RepresentantesDetalleNCF2018>("SELECT ReDSerie, ReDNCFActual+1 as ReDNCFActual, ReDFechaVencimiento, ReDNCFMax, rowguid " +
                "from " + (myParametro.GetParTakeFromNCF2021() ? "RepresentantesDetalleNCF2021" : "RepresentantesDetalleNCF2018") + "  WHERE cast(strftime('%Y%m%d',ReDFechaVencimiento) as integer) " +
                "between cast(strftime('%Y%m%d',DateTime('now')) as integer) and cast(strftime('%Y%m%d',ReDFechaVencimiento) as integer) " +
                "and ReDTipoComprobante = ? and ltrim(rtrim(RepCodigo)) = ? and ReDNCFActual+1 <= ReDNCFMax " +
                "order by ReDNCFMax asc", new string[] { cliente.CliTipoComprobanteFAC, Arguments.CurrentUser.RepCodigo.Trim() });
            }
            else
            {
                list = SqliteManager.GetInstance().Query<RepresentantesDetalleNCF2018>("SELECT ReDSerie, ReDNCFActual+1 as ReDNCFActual, ReDFechaVencimiento, ReDNCFMax, rowguid " +
                "from " + (myParametro.GetParTakeFromNCF2021() ? "RepresentantesDetalleNCF2021" : "RepresentantesDetalleNCF2018") + "  where /*ReDFechaVencimiento like '1900%' " +
                "and*/ ReDTipoComprobante = ? and ltrim(rtrim(RepCodigo)) = ? and ReDNCFActual+1 <= ReDNCFMax " +
                "order by ReDNCFMax asc", new string[] { cliente.CliTipoComprobanteFAC, Arguments.CurrentUser.RepCodigo.Trim() });
            }

            if (list != null && list.Count > 0)
            {
                var comprobante = list[0];

                if (comprobante == null)
                {
                    return null;
                }

                var ncfActual = comprobante.ReDNCFActual.ToString();
                //Arguments.Values.CurrentClient.ncfSecuencia = ncfActual;
                while (ncfActual.Length < 8)
                {
                    ncfActual = "0" + ncfActual;
                }

                var ncf = new NCF
                {
                    NCFCompleto = comprobante.ReDSerie + (forNC ? cliente.CliTipoComprobanteNC : cliente.CliTipoComprobanteFAC) + ncfActual,
                    Secuencia = comprobante.ReDNCFActual,
                    FechaVencimiento = comprobante.ReDFechaVencimiento,
                    rowguid = comprobante.rowguid
                };

                return ncf;
            }

            return null;
        }

        public bool ClienteTieneNcfValido(Clientes cliente)
        {
            var ncf = GetSiguienteNCF(cliente);

            return ncf != null && ncf.NCFCompleto.Length >= 11;
        }

        public bool IsRncValido(Clientes cliente)
        {

            cliente.CliRNC = cliente.CliRNC.Trim();

            if (string.IsNullOrWhiteSpace(cliente.CliRNC) || (cliente.CliRNC.Length != 9 && cliente.CliRNC.Length != 11))
            {
                return false;
            }

            var rncValido = "";

            foreach (var caracter in cliente.CliRNC.ToCharArray())
            {
                if (!char.IsDigit(caracter))
                {
                    return false;
                }

                rncValido += caracter;
            }

            return rncValido.Length == 9 || rncValido.Length == 11;
        }

        public List<ClientesDependientes> GetClientesDependientes(int cliId)
        {
            var list = new List<ClientesDependientes>()
            {
                new ClientesDependientes() { Cliid = -1, ClDNombre = "(Seleccione)" },
                new ClientesDependientes() { Cliid = -2, ClDNombre = "(Agregar Nuevo)" }
            };

            list.AddRange(SqliteManager.GetInstance().Query<ClientesDependientes>("select Cliid, ClDCedula, ClDNombre, CldTelefono, FopID, BanID, CldTipoCuentaBancaria, CldCuentaBancaria " +
                "from ClientesDependientes where Cliid = ? ", new string[] { cliId.ToString() }));

            return list;
        }

        public ClientesDependientes GetDependienteByCedula(string cedula, int cliId)
        {
            var list = SqliteManager.GetInstance().Query<ClientesDependientes>("select Cliid, ClDCedula, ClDNombre, CldTelefono, c.FopID as FopID, c.BanID as BanID, " +
                "CldTipoCuentaBancaria, CldCuentaBancaria, f.FopDescripcion as FopDescripcion, b.BanNombre as BanNombre, u.Descripcion as TipoCuentaDescripcion " +
                "from ClientesDependientes c " +
                "left join FormasPago f on f.FopID = c.FopID " +
                "left join Bancos b on b.BanID = c.BanID " +
                "left join UsosMultiples u on ltrim(rtrim(upper(u.CodigoGrupo))) = upper('TipoCtaBancaria') and u.CodigoUso = c.CldTipoCuentaBancaria " +
                "where Cliid = ? and ltrim(rtrim(upper(ClDCedula))) = ltrim(rtrim(upper(?)))", new string[] { cliId.ToString(), cedula });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public void EditarDependiente(ClientesDependientes Old, ClientesDependientes New)
        {
            var d = new Hash("ClientesDependientes");
            d.Add("ClDCedula", New.ClDCedula);
            d.Add("ClDNombre", New.ClDNombre);
            d.Add("CldTelefono", New.CldTelefono);
            d.Add("FopID", New.FopID);
            d.Add("BanID", New.BanID);
            d.Add("CldTipoCuentaBancaria", New.CldTipoCuentaBancaria);
            d.Add("CldCuentaBancaria", New.CldCuentaBancaria);
            d.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            d.Add("CliFechaActualizacion", Functions.CurrentDate());
            d.ExecuteUpdate("Cliid = " + Old.Cliid + " and ltrim(rtrim(ClDCedula)) = ltrim(rtrim('" + Old.ClDCedula + "'))");
        }

        public void CrearDependiente(ClientesDependientes New)
        {
            var d = new Hash("ClientesDependientes");
            d.Add("ClDCedula", New.ClDCedula);
            d.Add("ClDNombre", New.ClDNombre);
            d.Add("CldTelefono", New.CldTelefono);
            d.Add("FopID", New.FopID);
            d.Add("BanID", New.BanID);
            d.Add("CldTipoCuentaBancaria", New.CldTipoCuentaBancaria);
            d.Add("CldCuentaBancaria", New.CldCuentaBancaria);
            d.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            d.Add("CliFechaActualizacion", Functions.CurrentDate());
            d.Add("rowguid", Guid.NewGuid().ToString());
            d.Add("Cliid", New.Cliid);
            d.ExecuteInsert();
        }


        public List<ClientesProductos> GetLastThreeProductsByClient(int cliid)
        {
            var query = $@"SELECT * FROM ClientesProductosVendidos 
                           WHERE cliid = {cliid} 
                           ORDER BY CliFechaActualizacion DESC LIMIT 3";
            return SqliteManager.GetInstance().Query<ClientesProductos>(query);
        
        }

        public List<ProductosTemp> GetProductsNoSaledByClient(int cliid)
        {
            var query = $@"SELECT * 
                            FROM ClientesProductosNoVendidos cpnv
                            INNER JOIN Productos p ON p.Proid = cpnv.Proid
                            WHERE cliid = {cliid} AND RepCodigo = '{Arguments.CurrentUser.RepCodigo}'";
            return SqliteManager.GetInstance().Query<ProductosTemp>(query);
        
        }


        public bool ExistsDependiente(int cliid, string cedula)
        {
            return SqliteManager.GetInstance().Query<ClientesDependientes>("select Cliid from ClientesDependientes where Cliid = ? and ltrim(rtrim(upper(CLDCedula))) = ltrim(rtrim(upper(?))) limit 1", new string[] { cliid.ToString(), cedula }).Count > 0;
        }

        public ClientesDetalle GetDetalleFromCliente(int cliId, string repCodigo)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<ClientesDetalle>("select CliID, RepCodigo, LipCodigo, SecCodigo, OrvCodigo, ofvCodigo, ifnull(ConID, -1) as ConID " +
                    "from ClientesDetalle where CliID = ? /*and ltrim(rtrim(RepCodigo)) = ?*/ ", new string[] { cliId.ToString()/*, repCodigo.Trim() */});

                if (list != null && list.Count > 0)
                {
                    return list[0];
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            return null;
        }

        public (string,string) GetofvCodigoAndorvCodigo(int cliId, string SecCodigo)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<ClientesDetalle>("select OrvCodigo, ofvCodigo " +
                    "from ClientesDetalle where CliID = ? and SecCodigo = ?", new string[] { cliId.ToString(), SecCodigo}).FirstOrDefault();

                if (list != null)
                {
                    return (list.OrvCodigo,list.ofvCodigo);
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            return ("","");
        }
        

        public string GetareaCtrlCreditOfClienteDetalle(int cliId, string SecCodigo)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<ClientesDetalle>("select areaCtrlCredit " +
                    "from ClientesDetalle where CliID = ? and SecCodigo = ?", new string[] { cliId.ToString(), SecCodigo}).FirstOrDefault();

                if (list != null)
                {
                    return list.AreaCtrlCredit;
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            return "";
        }

        public List<Estados> GetTiposEstadosClientes()
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<Estados>("select EstDescripcion, ifnull(EstSiguientesEstados, '') as EstSiguientesEstados, EstEstado from Estados " +
                    "where trim(upper(EstTabla)) = 'CLIENTES' and EstEstado = 3", new string[] { });

                if (list != null && list.Count > 0)
                {
                    var estado = list[0];

                    var ests = estado.EstSiguientesEstados.Replace("|", ",");

                    return SqliteManager.GetInstance().Query<Estados>("select EstDescripcion, EstEstado from Estados " +
                        "where trim(upper(EstTabla)) = 'CLIENTES' and EstEstado in (" + ests + ") order by EstDescripcion ", new string[] { });
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return new List<Estados>();
        }

        public int GuardarProspecto(Clientes prospecto, ObservableCollection<ClientesReferencias> referencias, DS_TransaccionesImagenes myTraImg)
        {
            var cliid = 0;
            if (DS_RepresentantesSecuencias.ExistsTablaEnSecuencia("SolicitudesCredito"))
            {
                cliid = DS_RepresentantesSecuencias.GetLastSecuencia("SolicitudesCredito");
            }
            else
            {
                cliid = DS_RepresentantesSecuencias.GetLastSecuencia("Clientes");
            }
            
            var map = new Hash("Clientes");
            map.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            map.Add("CliID", cliid);
            map.Add("CliCodigo", cliid);
            map.Add("CliEstatus", prospecto.CliEstatus);
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map.Add("CliFechaActualizacion", Functions.CurrentDate());
            map.Add("rowguid", Guid.NewGuid().ToString());
            map.Add("CanId", prospecto.CanID);
            map.Add("CliTipoCliente", prospecto.CliTipoCliente);
            map.Add("CliTipoLocal", prospecto.CliTipoLocal);
            map.Add("CliNombre", prospecto.CliNombre);
            map.Add("CliCalle", prospecto.CliCalle);
            map.Add("CliCasa", prospecto.CliCasa);
            map.Add("cliSector", prospecto.cliSector);
            map.Add("CliRNC", prospecto.CliRNC);
            map.Add("CliTelefono", prospecto.CliTelefono);
            map.Add("TerCodigo", prospecto.TerCodigo);
            map.Add("ProID", prospecto.ProID);
            map.Add("MunID", prospecto.MunID);
            map.Add("SecCodigo", prospecto.SecCodigo);
            map.Add("CliContacto", prospecto.CliContacto);
            map.Add("CliContactoCelular", prospecto.CliContactoCelular);
            map.Add("CliLatitud", prospecto.CliLatitud);
            map.Add("CliLongitud", prospecto.CliLongitud);
            map.Add("LipCodigo", prospecto.LiPCodigo);
            map.Add("CliTipoComprobanteFAC", prospecto.CliTipoComprobanteFAC);
            map.Add("CliDatosOtros", "P");
            map.Add("ClaID", prospecto.ClaID);
            map.Add("RutEntregaID",  prospecto.RutEntregaID);
            map.Add("CliOrdenRuta",  prospecto.CliOrdenRuta);
            map.Add("CliFechaCreacion", Functions.CurrentDate());

            if(prospecto.TiNID > 0)
            {
                map.Add("TiNID", prospecto.TiNID);
            }
            else
            {
                map.Add("TiNID", "1");
            }

            map.Add("CliFax", prospecto.CliFax);
            map.Add("ConID", prospecto.ConID);
            map.Add("CliNombreEmisionCheques", prospecto.CliNombreEmisionCheques);
            map.Add("CliEstimadoCompras", prospecto.CliEstimadoCompras);
            map.Add("CliInventario", prospecto.CliInventario);
            map.Add("CliLimiteCreditoSolicitado", prospecto.CliLimiteCreditoSolicitado);
            map.Add("CliLimiteCreditoAprobado", 0.0);
            map.Add("CliPropietario", prospecto.CliPropietario);
            map.Add("CliPropietarioDireccion", prospecto.CliPropietarioDireccion);
            map.Add("CliCedulaPropietario", prospecto.CliCedulaPropietario);
            map.Add("CliPropietarioTelefono", prospecto.CliPropietarioTelefono);
            map.Add("CliPropietarioCelular", prospecto.CliPropietarioCelular);
            map.Add("CliFamiliarNombre", prospecto.CliFamiliarNombre);
            map.Add("CliFamiliarDireccion", prospecto.CliFamiliarDireccion);
            map.Add("CliFamiliarCedula", prospecto.CliFamiliarCedula);
            map.Add("CliFamiliarTelefono", prospecto.CliFamiliarTelefono);
            map.Add("CliFamiliarCelular", prospecto.CliFamiliarCelular);
            map.Add("CliRutPosicion", prospecto.CliRutPosicion);
            map.Add("CliFormasPago", prospecto.CliFormasPago);
            map.Add("CliCorreoElectronico", prospecto.CliCorreoElectronico);
            map.Add("CliFrecuenciaVisita", prospecto.CliFrecuenciaVisita);
            map.Add("CliRutSemana1", prospecto.CliRutSemana1);
            map.Add("CliRutSemana2", prospecto.CliRutSemana2);
            map.Add("CliRutSemana3", prospecto.CliRutSemana3);
            map.Add("CliRutSemana4", prospecto.CliRutSemana4);

            map.Add("CliRegMercantil", prospecto.CliRegMercantil);
            map.Add("CliUrbanizacion", prospecto.CliUrbanizacion);

            map.ExecuteInsert();

            var i = 1;
            foreach (var Ref in referencias)
            {
                var r = new Hash("ClientesReferencias");
                r.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                r.Add("CliID", cliid);
                r.Add("CliRefSecuencia", i); i++;
                r.Add("CliRefTipo", Ref.CliRefTipo);
                r.Add("CliRefNombre", Ref.CliRefNombre);
                r.Add("CliRefTelefono", Ref.CliRefTelefono);
                r.Add("CliRefCuenta", Ref.CliRefCuenta);
                r.Add("BanID", Ref.BanID);
                r.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                r.Add("CliFechaActualizacion", Functions.CurrentDate());
                r.Add("rowguid", Guid.NewGuid().ToString());
                r.ExecuteInsert();
            }

            //CREAR CLIENTE DETALLE SI LA RELACION DE CLIENTE VENDEDOR ES DE UNO A MUCHO
            if(myParametro.GetParTipoRelacionClientes() == 2)
            {
              
                var mySec = new DS_Sectores();
                var sectores = mySec.GetSectores();
                foreach (var sec in sectores)
                {
                    var detalle = new Hash("ClientesDetalle");
                    detalle.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                    detalle.Add("CliID", cliid);
                    detalle.Add("SecCodigo",sec.SecCodigo);
                    //detalle.Add("OrvCodigo", "");
                    //detalle.Add("CanID", "");
                    //detalle.Add("ofvCodigo","");
                    //detalle.Add("zonaCliente", "");
                    //detalle.Add("grupoCliente", "");
                    detalle.Add("areaCtrlCredit", sec.SecCodigo);
                    detalle.Add("estatus", 1);
                    //detalle.Add("segmento", "");
                    //detalle.Add("limiteCredito", "");
                    //detalle.Add("fechaUltPago", "");
                    //detalle.Add("montoUltPago", "");
                     detalle.Add("MonCodigo", "DOP");
                    //detalle.Add("PedidoMinimo", "");
                    //detalle.Add("CedCodigo", "");
                    //detalle.Add("CliPorcientoColocacion", "");
                    detalle.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                    //detalle.Add("CliFechaActualizacion", "");
                    detalle.Add("rowguid", Guid.NewGuid().ToString());
                    //detalle.Add("clicodigo","");
                    detalle.Add("LipCodigo", prospecto.LiPCodigo);
                    //detalle.Add("GrpCodigo", "");
                    //detalle.Add("CliMultMonedas", "");
                    detalle.Add("ConId", prospecto.ConID);
                    //detalle.Add("CliMontoPedidoSugerido", "");
                    //detalle.Add("CliIndicadorExonerado", "");
                    detalle.Add("CliFuente", "MB");
                    detalle.ExecuteInsert();

                }
            }
            myTraImg.MarkToSendToServer("Prospectos", cliid.ToString());

            if (DS_RepresentantesSecuencias.ExistsTablaEnSecuencia("SolicitudesCredito"))
            {
                DS_RepresentantesSecuencias.UpdateSecuencia("SolicitudesCredito", cliid);
            }
            else
            {
                DS_RepresentantesSecuencias.UpdateSecuencia("Clientes", cliid);
            }
            

            return cliid;
        }

        public List<ClientesReferencias> GetReferencias(int cliId)
        {
            return SqliteManager.GetInstance().Query<ClientesReferencias>("select * from ClientesReferencias where CliID = ? order by CliRefNombre", new string[] { cliId.ToString() });
        }

        public bool PertenceAlgunSector(int cliId)
        {
            try
            {
                return SqliteManager.GetInstance().Query<Sectores>("select s.SecCodigo as SecCodigo from ClientesDetalle c " +
                    "inner join Clientes q on q.CliID = c.CliID " +
                    "inner join Sectores s on s.SecCodigo = c.SecCodigo where " +
                    "c.CliID = ? limit 1", new string[] { cliId.ToString() }).Count > 0;
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return false;
        }


        public List<ClientesDirecciones> getDirPedCli(int Cliid)
        {
            return SqliteManager.GetInstance().Query<ClientesDirecciones>("SELECT ifnull(CldCalle, '') as CldCalle, ifnull(CldDirTipo, '') as CldDirTipo, CldCasa, CldSector, CldContacto, PaiID, ProID, MunID, CldTelefono, CldLatitud, CldLongitud FROM ClientesDirecciones WHERE CLIID = ? ORDER BY CldCalle", new string[] { Cliid.ToString() });
        }


        public ClientesDirecciones GetDireccionDetallada(string CldDirTipo, int Cliid)
        {

            try
            {

                var list = SqliteManager.GetInstance().Query<ClientesDirecciones>("SELECT CliiD, ifnull(CldCalle, '') as CldCalle, ifnull(CldDirTipo, '') as CldDirTipo, CldCasa, CldContacto, CldFax, CldSector, CldTelefono FROM ClientesDirecciones WHERE  CldDirTipo = ? AND  CLIID = ? ORDER BY CldCalle", new string[] { CldDirTipo, Cliid.ToString() });

                if (list != null && list.Count > 0)
                {
                    return list[0];
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            return null;

        }

        public async Task<List<Clientes>> GetClientesNoVendidos(string Tabla, string LinID, bool isOnline, bool IsRutaVisita, string Cliids, bool FromProducts)
        {
            string sql;
            if (FromProducts)
            {
                if (myParametro.GetParProductosNoVendidosNewQuery())
                {
                    sql = "select DISTINCT cd.CliID,c.CliNombre, cd.CliCodigo from ClientesDetalle cd inner join clientes c on cd.cliid = c.CliID where cd.CliID " +
                          "not IN(select DISTINCT cp.cliid from " + Tabla + " cp inner join Productos p on p.ProID = cp.ProID " +
                          "where p.proid = '" + LinID + "' " +
                           (Arguments.Values.CurrentClient != null ? " and  cp.CliID = " + Arguments.Values.CurrentClient.CliID + "" : "") + ")" + (IsRutaVisita ? "and cd.cliid in (" + Cliids + ")" : "") + ""
                           + " and cd.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' order by cd.CliID";
                }
                else
                {
                    sql = "select DISTINCT CliID, CliNombre, CliCodigo from Clientes " +
                                  "where CliID not IN (select cp.cliid from " + Tabla + " cp "
                                  + "inner join ClientesDetalle c on c.CliID = cp.CliID "
                                  + " inner join GrupoProductosDetalle g on g.GrpCodigo = c.GrpCodigo "
                                  + "where cp.ProID = '" + LinID + "' and c.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' " + (Arguments.Values.CurrentClient != null ? " and  cp.CliID = " + Arguments.Values.CurrentClient.CliID + "" : "") + ")" + (IsRutaVisita ? "and cliid in (" + Cliids + ")" : "") + ""
                                  + " and RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' order by CliID";
                }

            }
            else
            {
                if (myParametro.GetParProductosNoVendidosNewQuery())
                {
                    sql = "select DISTINCT cd.CliID,c.CliNombre, cd.CliCodigo from ClientesDetalle cd inner join clientes c on cd.cliid = c.CliID where cd.CliID " +
                          "not IN(select DISTINCT cp.cliid from " + Tabla + " cp inner join Productos p on p.ProID = cp.ProID " +
                          "where linid = '" + LinID + "'" +
                           (Arguments.Values.CurrentClient != null ? " and  cp.CliID = " + Arguments.Values.CurrentClient.CliID + "" : "") + ")" + (IsRutaVisita ?
                          "and cd.cliid in (" + Cliids + ")" : "") + " and cd.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' order by cd.CliID";
                }
                else
                {
                    sql = " select DISTINCT CliID, CliNombre, CliCodigo from Clientes " +
                                     "where CliID not IN (select DISTINCT cp.cliid from " + Tabla + " cp " +
                                     "inner join ClientesDetalle c on c.CliID = cp.CliID and c.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' " +
                                     "inner join GrupoProductosDetalle gp on gp.GrpCodigo = c.GrpCodigo and gp.ProID = cp.ProID " +
                                     "inner join Productos p on p.ProID = gp.ProID " +
                                     "where p.linid = '" + LinID + "' " + (Arguments.Values.CurrentClient != null ? " and  cp.CliID = " + Arguments.Values.CurrentClient.CliID + "" : "") + ")" + (IsRutaVisita ? "and cliid in (" + Cliids + ")" : "") + " and RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "'"
                                     + " order by CliID";
                }
            }
            if (isOnline)
            {
                return await api.RawQuery<Clientes>(Arguments.CurrentUser.RepCodigo, Arguments.CurrentUser.RepClave, sql);
            }
            return new List<Clientes>(SqliteManager.GetInstance().Query<Clientes>(sql));

        }

        public bool GetCliIndicadorCredito(int cliId)
        {
            try
            {
                string query;

                query = "select ifnull(CliIndicadorCredito, 0) as CliIndicadorCredito from Clientes where CliID = ? ";


                var list = SqliteManager.GetInstance().Query<Clientes>(query, new string[] { cliId.ToString() });

                if (list != null && list.Count > 0)
                {
                    return list[0].CliIndicadorCredito;
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return false;
        }

        public bool VarificarSiExisteCliente(int CliID)
        {
            var cliente = SqliteManager.GetInstance().Query<Clientes>("Select CliID from Clientes where CliID = ? ", new string[] { CliID.ToString() });
            if (cliente.Count > 0)
            {
                return true;
            }

            return false;

        }

        public string GetCliNombreByRnc(string rnc)
        {
            var list = SqliteManager.GetInstance().Query<Clientes>("select ifnull(trim(CliNombre), '') as CliNombre from Clientes where replace(trim(upper(ifnull(CliRNC,''))), '-', '') = trim(upper(ifnull('"+rnc.Replace("-", "")+"',''))) ", new string[] { });

            if(list != null && list.Count > 0)
            {
                return list[0].CliNombre;
            }

            return null;
        }

        public bool GetSACinVisita(int VisSecuencia)
        {
            var list = new List<SolicitudActualizacionClientes>(SqliteManager.GetInstance().Query<SolicitudActualizacionClientes>("SELECT " +
                "SACSecuencia FROM SolicitudActualizacionClientes WHERE VisSecuencia = '"+VisSecuencia+"' and ifnull(CliLatitud,0) <> 0 and ifnull(CliLongitud,0) <> 0 ", new string[] {}));

            return list.Count > 0;
        }

        public string GetClasificacion(int ClaID)
        {
            try
            {
                string query;
                query = "SELECT ClaDescripcion from Clasifica where ClaID = "+ClaID+" ";
                var list = SqliteManager.GetInstance().Query<Clasifica>(query, new string[] { });

                if (list != null && list.Count > 0)
                {
                    return list[0].ClaDescripcion;
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return null;
        }

        public int GetClaID(int Cliid)
        {
            try
            {
                string query;
                query = "SELECT ClaID from Clientes where Cliid = " + Cliid + " ";
                var list = SqliteManager.GetInstance().Query<Clasifica>(query, new string[] { });

                if (list != null && list.Count > 0)
                {
                    return list[0].ClaID;
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return 0;
        }

        public int GetCliMunID(int Cliid)
        {
            try
            {
                string query;
                query = "SELECT MunID from Clientes where Cliid = " + Cliid + " ";
                var list = SqliteManager.GetInstance().Query<Clientes>(query, new string[] { });

                if (list != null && list.Count > 0)
                {
                    return list[0].MunID;
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return 0;
        }

        public int GetCliPaiID(int Cliid)
        {
            try
            {
                string query;
                query = "SELECT PaiID from Clientes where Cliid = " + Cliid + " ";
                var list = SqliteManager.GetInstance().Query<Clientes>(query, new string[] { });

                if (list != null && list.Count > 0)
                {
                    return list[0].PaiID;
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return 0;
        }

        public string GetListaPrecioCliente(int Cliid)
        {
            try
            {
                string query;
                query = "SELECT LiPCodigo from Clientes where Cliid = " + Cliid + " ";
                var list = SqliteManager.GetInstance().Query<Clientes>(query, new string[] { });

                if (list != null && list.Count > 0)
                {
                    query = "SELECT Descripcion from UsosMultiples where CodigoGrupo = 'LIPCODIGO' AND CodigoUso = '" + list[0].LiPCodigo + "' ";
                    var list1 = SqliteManager.GetInstance().Query<UsosMultiples>(query, new string[] { });

                    if (list1 != null && list.Count > 0)
                    {
                        return list1[0].Descripcion;
                    }
                }
                return "";

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return "";
        }

        public string GetLipCodigoPMR(int cliId)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<Clientes>("select LipCodigoPMR from Clientes where CliID = ? ", 
                    new string[] { cliId.ToString() });

                if(list != null)
                {
                    return list[0].LipCodigoPMR;
                }
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return "";
        }

        public string GetCliRNC(int Cliid)
        {
            try
            {
                string query;
                query = "SELECT CliRNC from Clientes where Cliid = " + Cliid + " ";
                var list = SqliteManager.GetInstance().Query<Clientes>(query, new string[] { });

                if (list != null && list.Count > 0)
                {
                        return list[0].CliRNC;
                    
                }
                return "";

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return "";
        }

        public Clientes UltimoClienteVisitado()
        {
            //var cliente = new Clientes();
            string query = $@"select c.* from Clientes c 
                            inner join Visitas v on v.CliID = c.CliID
                            where    v.RepCodigo = '{Arguments.CurrentUser.RepCodigo}' and v.VisFechaentrada like '{Functions.CurrentDate("yyyy-MM-dd")}%' order by  v.Vissecuencia desc limit 1";
            var cliente = SqliteManager.GetInstance().Query<Clientes>(query, new string[] { }).FirstOrDefault();
            if(cliente == null)
            {
                cliente = GetAllClientesRutPosicion().FirstOrDefault();
            }
            return cliente;

        }

        public List<Clasifica> GetClasificacionClientes()
        {
            string query = "SELECT ClaDescripcion, ClaID from Clasifica ";
            var list = SqliteManager.GetInstance().Query<Clasifica>(query, new string[] { }).ToList();

            if(list.Count > 0)
            {
                return list;
            }
            return new List<Clasifica>();
        }

        public void GuardarClienteDireccion(ClientesDirecciones dir)
        {
            var map = new Hash("ClientesDirecciones");
            map.Add("CliID", dir.CliiD);

            var dirTipo = DateTime.UtcNow.Ticks.ToString().Substring(8);

            map.Add("CldDirTipo", dirTipo);
            map.Add("CldCalle", dir.CldCalle);
            map.Add("CldCasa", dir.CldCasa);
            map.Add("CldSector", dir.CldSector);
            map.Add("CldContacto", dir.CldContacto);
            map.Add("CldTelefono", dir.CldTelefono);
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map.Add("CldFechaActualizacion", Functions.CurrentDate());
            map.Add("CliFechaActualizacion", Functions.CurrentDate());
            map.Add("rowguid", Guid.NewGuid().ToString());
            map.Add("CliFuente", "MB");

            map.ExecuteInsert();

        }

        public Clientes GetClienteByIdForVerf(int cliid, bool RNC = false)
        {
            string query = "select clinombre,clicalle,CliCodigo,CliUrbanizacion" + (RNC ? ", CliRNC " : " ") + "from clientes where cliid =? ";
            var cliente = SqliteManager.GetInstance().Query<Clientes>(query, new string[] { cliid.ToString() }).FirstOrDefault();
            return cliente;
        }
        /*
        public List<Clientes> GetClienteForRep()
        {
            string query = "select clinombre,CliID, MonCodigo from clientes ";
            var cliente = SqliteManager.GetInstance().Query<Clientes>(query, new string[] { }).ToList();

            return cliente;
        }*/
        /*public List<Clientes> GetClienteForRepByPueblo(int proid)
        {
            string query = "select clinombre,CliID,MonCodigo from clientes where proid =?";
            
            return SqliteManager.GetInstance().Query<Clientes>(query, new string[] { proid.ToString() }).ToList();
        }*/
        public int GetClienteForCounts()
        {
            string query = "select count(*) as CliTotal from clientes";
            var list = SqliteManager.GetInstance().Query<Clientes>(query, new string[] {});
            return list[0].CliTotal;
        }
        public int GetClientesPendientesAContar()
        {
            DateTime timespan = DateTime.Now;
            int time = Math.Abs(timespan.Day - DateTime.Now.Day);
            List<Clientes> clientes = new List<Clientes>();

            //int totaldays = (int)Math.Abs(time.TotalDays);

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

            return clientes.Count;
        }

        public Clientes GetAllClientesRutPosicion(int rutPosicion = -1, int cliordenruta = -1)
        {
            try
            {
                string query = "";

                if (App.Current.Properties.ContainsKey("CurrentRep"))
                {
                    query = " inner join clientesdetalle cd on cd.cliid = c.cliid and cd.repcodigo = '" + App.Current.Properties["CurrentRep"].ToString() + "' ";
                }
                else
                {
                    query = " inner join clientesdetalle cd on cd.cliid = c.cliid and cd.repcodigo = '" + Arguments.CurrentUser.RepCodigo + "' ";
                }

                return SqliteManager.GetInstance().Query<Clientes>($@"SELECT CliRutPosicion,CliNombre,rutsemana1 as CliRutSemana1, c.Cliid as Cliid, 
                                                                    rutsemana2 as CliRutSemana2, rutsemana3 as CliRutSemana3, rutsemana4 as
                                                                 CliRutSemana4, c.CliOrdenRuta from clientes c {query} left join RutaVisitas r on r.cliid = c.Cliid where 
                                                                  {(rutPosicion != -1? "CliRutPosicion = "+ rutPosicion +" order by trim(CliNombre)"
                                                                  : "c.CliOrdenRuta = "+ cliordenruta + " order by trim(CliNombre)")}").FirstOrDefault();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return new Clientes();
            }
        }

        public Clientes GetAllClientesRutPosicionForValid(bool rutPosicion = false, bool cliordenruta = false)
        {            
            try
            {
                string query = "";

                if (App.Current.Properties.ContainsKey("CurrentRep"))
                {
                    query = " inner join clientesdetalle cd on cd.cliid = c.cliid and cd.repcodigo = '" + App.Current.Properties["CurrentRep"].ToString() + "' ";
                }else
                {
                    query = " inner join clientesdetalle cd on cd.cliid = c.cliid and cd.repcodigo = '" + Arguments.CurrentUser.RepCodigo + "' ";
                }

                return SqliteManager.GetInstance().Query<Clientes>($@"select CliRutPosicion CliRutPosicion, c.CliOrdenRuta 
                                                                  CliOrdenRuta from Clientes c {query} left join RutaVisitas r on r.cliid = 
                                                                  c.cliid GROUP by(c.cliid)
                                                                  {(rutPosicion ? " order by CliRutPosicion DESC" : " ")} {(cliordenruta ?
                                                                  " order by CliOrdenRuta DESC" : " ")}").FirstOrDefault();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return new Clientes();
            }
        }

        public Clientes GetAllClientesByIdForPus(int id)
        {
            try
            {
                return SqliteManager.GetInstance().Query<Clientes>("SELECT CliLimiteCredito, CliIndicadorCredito, CliNombre, CliCodigo from clientes where cliid = "+ id +"").FirstOrDefault();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return new Clientes();
            }
        }


        public string GetSiguienteNCFForReportes(string ncftipocomprobante)
        {            
            var list = SqliteManager.GetInstance().Query<RepresentantesDetalleNCF2018>("SELECT ReDSerie, ReDNCFActual+1 as ReDNCFActual, ReDFechaVencimiento, ReDNCFMax, rowguid " +
            "from " + (myParametro.GetParTakeFromNCF2021() ? "RepresentantesDetalleNCF2021" : "RepresentantesDetalleNCF2018") + "  where cast(strftime('%Y%m%d',ReDFechaVencimiento) as integer) " +
            "BETWEEN cast(strftime('%Y%m%d',DateTime('now')) as integer) and cast(strftime('%Y%m%d',ReDFechaVencimiento) as integer) " +
            "AND ReDTipoComprobante = ? and ltrim(rtrim(RepCodigo)) = ? and ReDNCFActual+1 <= ReDNCFMax " +
            "order by ReDNCFMax asc", new string[] { ncftipocomprobante, Arguments.CurrentUser.RepCodigo.Trim() });            

            if (list != null && list.Count > 0)
            {
                var comprobante = list[0];

                if (comprobante == null)
                {
                    return null;
                }

                var ncfActual = comprobante.ReDNCFActual.ToString("00000000") + comprobante.ReDNCFActual.ToString();
                //Arguments.Values.CurrentClient.ncfSecuencia = ncfActual;
                //while (ncfActual.Length < 8)
                //{
                //    ncfActual = "0" + ncfActual;
                //}

                return comprobante.ReDSerie + ncftipocomprobante + ncfActual;
            }

            return null;
        }

        public Clientes GetClientesForSac(int Cliid, string RepCodigo)
        {
            string condition = " WHERE CliID = "+ Cliid + "";

            string whereCondition = "";

            var extraWhere = "";

            //var clientes = SqliteManager.GetInstance().Query<Clientes>("SELECT RepCodigo FROM Clientes");

            //var checkRepCodigo = clientes.Where(cliente => cliente.RepCodigo != null).Select(cliente => cliente.RepCodigo).ToList();

            var parRelacionClientes = myParametro.GetParTipoRelacionClientes();

            if (!string.IsNullOrWhiteSpace(RepCodigo) && parRelacionClientes == 2)
            {               
                extraWhere = " and (trim(c.RepCodigo) = '" + RepCodigo + "' OR c.CliID in (select CliID from ClientesDetalle where CliID = c.CliID )) ";
            }
            else if ((!string.IsNullOrWhiteSpace(RepCodigo) && Arguments.CurrentUser.RepIndicadorSupervisor))
            {
                extraWhere = " and trim(c.RepCodigo) = '" + RepCodigo.Trim() + "' ";
            }

            //if (checkRepCodigo.Count == 0 && myParametro.GetParTipoRelacionClientes() == 1)
               // extraWhere = "";


            string query = "select CliUrbanizacion, CliEncargadoPago, CliPaginaWeb, ltrim(rtrim(CliNombre)) as CliNombre, c.CliContactoFechaNacimiento as CliContactoFechaNacimiento, CliIndicadorPresentacion, ifnull(ConID, -1) as ConID, CliID,CliNombreComercial, " +
                "CliCodigo, RepCodigo, CliCalle, ifnull(MonCodigo, '') as MonCodigo, IFNULL(CliFormasPago, '000000') as CliFormasPago, LipCodigoPM, TiNID, " +
                "ifnull((select ifnull(VisEstatus, 5) from Visitas v where v.CliID = c.CliID and VisFechaEntrada like '" + Functions.CurrentDate("yyyy-MM-dd") + "%' order by cast(ifnull(v.VisEstatus, 5) as integer) desc), 5) as CliEstatusVisita, " +
                "ifnull(LiPCodigo, '') as LiPCodigo, CliTelefono, CliFax, CliContacto, c.ProID, MunID, CliRNC, CliIndicadorDeposito, " +
                "CliCorreoElectronico, CliPropietario, CliIndicadorOrdenCompra, " +
                "CliIndicadorDepositaFactura, cliSector, c.SecCodigo as SecCodigo, ifnull(CliLatitud, 0) as CliLatitud, c.CliDatosOtros as CliDatosOtros, " +
                "ifnull(CliLongitud, 0) as CliLongitud, CliIndicadorCheque, CliLimiteCredito, CliIndicadorExonerado, ifnull(CliRutPosicion, 0) as CliRutPosicion, " +
                "CliPromedioPago, CliMontoUltimoCobro, CAST(replace(strftime('%m-%d-%Y', SUBSTR(CliFechaUltimoCobro,1,10)),' ','' ) as varchar) as CliFechaUltimoCobro, CliEstatus, CliCodigoDescuento, CliPromedioCompra, " +
                "CliTipoComprobanteFAC, CliTipoComprobanteNC, CAST(replace(strftime('%m-%d-%Y', SUBSTR(CliFechaUltimaVenta,1,10)),' ','' ) as varchar) as CliFechaUltimaVenta, CliVentasAnioAnterior, " +
                "CliMontoUltimaVenta, CliVentasAnioActual,CldDirTipo,ifnull(CliRegMercantil,'') CliRegMercantil,  " + (DS_RepresentantesParametros.GetInstance().GetParSACCliRNCCedula() ? " CliRNC as CliCedulaPropietario " : "ifnull(CliCedulaPropietario, '') as CliCedulaPropietario") + "  from SolicitudActualizacionClientes c left join Provincias p on p.proid = c.proid" + condition + " " + whereCondition + " " + extraWhere + " " +
                "order by SACSecuencia DESC ";

            return new ObservableCollection<Clientes>(SqliteManager.GetInstance().Query<Clientes>(query, new string[] { })).FirstOrDefault();
        }

        public List<ClientesHorarios>  GetClienteHorarios(int cliid)
        {
            var list = SqliteManager.GetInstance().Query<ClientesHorarios>("select CliID, ClhSecuencia, u.Descripcion as ClhDia, clhHorarioApertura, clhHorarioCierre  " +
                "from ClientesHorarios c " +
                "inner join UsosMultiples u on trim(upper(u.CodigoGrupo)) = 'DIASSEMANA' and u.CodigoUso = c.ClhDia " +
                "where c.CliID = ? order by u.CodigoUso", new string[] { cliid.ToString() });

            return list;
        }

        public double GetMontoPedidoMinimo(int cliid)
        {
            try
            {
                var query = "select PedidoMinimo from ClientesDetalle where CliID = ? and trim(RepCodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ";

                if (Arguments.Values.CurrentSector != null && !string.IsNullOrWhiteSpace(Arguments.Values.CurrentSector.SecCodigo))
                {
                    query += " and SecCodigo = '" + Arguments.Values.CurrentSector.SecCodigo + "'";
                }

                var list = SqliteManager.GetInstance().Query<ClientesDetalle>(query, new string[] { cliid.ToString() });

                if (list != null && list.Count > 0)
                {
                    return list[0].PedidoMinimo;
                }

                return -1;
            }
            catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return -1;
        }

        public double GetPorcientoDescuentoGeneralByCliente(int cliId)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<Clientes>("select ifnull(CliPorcientoDsctoGlobal,0) as CliPorcientoDsctoGlobal from Clientes where CliID = ? ", new string[] { cliId.ToString() });

                if (list != null && list.Count > 0)
                {
                    return list[0].CliPorcientoDsctoGlobal;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return 0;
        }

        public List<Clientes> GetClientesByClienteOferta(int cliid, int tinid)
        {
            return SqliteManager.GetInstance().Query<Clientes>("select c.CliID as CliID, ifnull(c.CliNombre,'Sin Descripcion') as CliNombre, " +
                "ifnull(c.CliCodigo, 'Sin Codigo') as CliCodigo from Clientes c " +
                "where c.CliID = ? and (c.TinID = " + tinid.ToString() + " or " + tinid.ToString() + " = 0) " +
                "order by c.CliNombre", new string[] { cliid.ToString() });
        }

        public List<Clientes> GetClientesByGrupoClienteOferta(string grcCodigo, int tinid)
        {
            return SqliteManager.GetInstance().Query<Clientes>("select c.CliID as CliID, ifnull(c.CliNombre,'Sin Descripcion') as CliNombre, " +
                "ifnull(c.CliCodigo, 'Sin Codigo') as CliCodigo from GrupoClientesDetalle g " +
                "INNER JOIN Clientes c on c.CliID = g.CliID " +
                "where g.GrcCodigo = ? and (c.TinID = " + tinid.ToString() + " or " + tinid.ToString() + " = 0) " +
                "order by c.CliNombre", new string[] { grcCodigo.ToString() });
        }

        public List<Clientes> GetClientesByTodosClientesOferta(int tinid)
        {
            return SqliteManager.GetInstance().Query<Clientes>("select c.CliID as CliID, ifnull(c.CliNombre,'Sin Descripcion') as CliNombre, " +
                "ifnull(c.CliCodigo, 'Sin Codigo') as CliCodigo from Clientes c " +
                "where (c.TinID = " + tinid.ToString() + " or " + tinid.ToString() + " = 0) " +
                "order by c.CliNombre", new string[] { });
        }

        public string GetTiposYCategoriasByCliente(int cliId, int numofparms)
        {
            var joinAtributosCliente = "left join UsosMultiples cat1 on cat1.CodigoUso = c.CliCat1 and trim(upper(cat1.CodigoGrupo)) = upper('CliCat1')" +
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


            string selectAtributosCliente = "";


            if (numofparms == 1)
            {
                selectAtributosCliente = (!string.IsNullOrWhiteSpace(parCliCat1) ? "case when ifnull(c.CliCat1,'') <> '' then '" + parCliCat1 + ": ' || cat1.Descripcion else '' end" : "''") + " as CliCat1, " +
                (!string.IsNullOrWhiteSpace(parCliCat2) ? "case when ifnull(c.CliCat2, '') <> '' then '" + parCliCat2 + ": ' || cat2.Descripcion else '' end " : "''") + "as CliCat2, " +
                (!string.IsNullOrWhiteSpace(parCliCat3) ? "case when ifnull(c.CliCat3, '') <> '' then '" + parCliCat3 + ": ' || cat3.Descripcion else '' end" : "''") + " as CliCat3, " +
                (!string.IsNullOrWhiteSpace(parCliTipo1) ? "case when ifnull(c.CliTipoCliente1, '') <> '' then '" + parCliTipo1 + ": '||tip1.Descripcion else '' end " : "''") + " as CliTipoCliente1, " +
                (!string.IsNullOrWhiteSpace(parCliTipo2) ? "case when ifnull(c.CliTipoCliente2, '') <> '' then '" + parCliTipo2 + ": '||tip2.Descripcion else '' end " : "''") + " as CliTipoCliente2, " +
                (!string.IsNullOrWhiteSpace(parCliTipo3) ? "case when ifnull(c.CliTipoCliente3, '') <> '' then '" + parCliTipo3 + ": '||tip3.Descripcion else '' end " : "''") + " as CliTipoCliente3, ";
            }
            else if (numofparms == 2)
            {
                selectAtributosCliente = "case when ifnull(c.CliCat1,'') <> '' then cat1.Descripcion else '' end as CliCat1, " +
                                         "case when ifnull(c.CliCat2, '') <> '' then  cat2.Descripcion else '' end as CliCat2, " +
                                         "case when ifnull(c.CliCat3, '') <> '' then  cat3.Descripcion else '' end as CliCat3, " +
                                         "case when ifnull(c.CliTipoCliente1, '') <> '' then tip1.Descripcion else '' end as CliTipoCliente1, " +
                                         "case when ifnull(c.CliTipoCliente2, '') <> '' then tip2.Descripcion else '' end as CliTipoCliente2, " +
                                         "case when ifnull(c.CliTipoCliente3, '') <> '' then tip3.Descripcion else '' end  as CliTipoCliente3, ";
            }


            var query = "select " + selectAtributosCliente + "CliId from Clientes c "
                + joinAtributosCliente + 
                " where c.CliID = " + cliId;

            var list = SqliteManager.GetInstance().Query<Clientes>(query, new string[] { });

            var result = "";

            if(list != null && list.Count > 0)
            {
                var item = list[0];

                if (!string.IsNullOrWhiteSpace(item.CliCat1))
                {
                    result += item.CliCat1 + "\n";
                }
                if (!string.IsNullOrWhiteSpace(item.CliCat2))
                {
                    result += item.CliCat2 + "\n";
                }
                if (!string.IsNullOrWhiteSpace(item.CliCat3))
                {
                    result += item.CliCat3 + "\n";
                }

                if (!string.IsNullOrWhiteSpace(item.CliTipoCliente1))
                {
                    result += item.CliTipoCliente1 + "\n";
                }
                if (!string.IsNullOrWhiteSpace(item.CliTipoCliente2))
                {
                    result += item.CliTipoCliente2 + "\n";
                }
                if (!string.IsNullOrWhiteSpace(item.CliTipoCliente3))
                {
                    result += item.CliTipoCliente3 + "\n";
                }
            }

            return result;

        }

        
    }
}