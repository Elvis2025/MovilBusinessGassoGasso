
using MovilBusiness.Configuration;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MovilBusiness.DataAccess
{
    public class DS_Estados : DS_Controller
    {
        public List<Estados> GetByTabla(string estTabla, string estadoCampo, bool NotUserClient = false, string where = null)
        {
            var condition = "";
            string diasPermitidos = "365";
            string getfecha = estTabla.Substring(0,3) + "Fecha";

            var parDiasTransaccionesVisibles = DS_RepresentantesParametros.GetInstance().GetDiasTransaccionesVisibles();
            if (parDiasTransaccionesVisibles > 0)
            {
                diasPermitidos = parDiasTransaccionesVisibles.ToString();
            }

            var whereDias = GetDiasTransaccionesVisibles(estTabla);

            /*string condition1 = " and (cast(replace(cast(julianday(datetime('now')) - julianday("+ getfecha + ")" +
                                " as integer),' ', '') as integer)) < " + diasPermitidos;*/
            string condition1 = "";

            if (!string.IsNullOrWhiteSpace(whereDias))
            {
                condition += " and " + whereDias + " ";
            }


            if (!string.IsNullOrWhiteSpace(where))
            {
                condition = " and " + where + " ";
            }

            var sql = "select EstTabla, EstEstado, EstDescripcion, estOpciones, " +
                "EstSiguientesEstados " + (NotUserClient ? ", 0 as UseClient" : "") + ", "
                + (!string.IsNullOrWhiteSpace(estadoCampo) ? "(select count(*) from " + estTabla + " where " + estadoCampo + " = e.EstEstado " + condition + condition1 + " "+(myParametro.GetParVendedorContVend() ? " " : " and trim(RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' ") + ")" : "0" ) + " as Count " +
                "from Estados e where upper(trim(estTabla)) = ? "  + (!string.IsNullOrWhiteSpace(estadoCampo) ? " and Count > 0 " : "") + "order by EstEstado ASC";
                

            return SqliteManager.GetInstance().Query<Estados>(sql, new string[] { estTabla.Trim().ToUpper() }); ;
        }

        public List<Estados> GetByFecha(string estTabla, string fechaCampo, string estadoCampo, bool NotUserClient = false, string where = null)
        {
            var condition = "";

            var whereDias = GetDiasTransaccionesVisibles(estTabla);

            if (!string.IsNullOrWhiteSpace(whereDias))
            {
                condition += " and " + whereDias + " ";
            }

            if (!string.IsNullOrWhiteSpace(where))
            {
                condition = " and " + where + " ";
            }

            var query = "select '" + estTabla + "' as EstTabla, rowguid, ifnull(replace(strftime('%d-%m-%Y', SUBSTR(" + fechaCampo + ",1,10)),' ','' ), '') as EstDescripcion " +
                (NotUserClient ? ", 0 as UseClient" : "") + ", count(*) as Count " +
                "from "+estTabla+" t " +
                "where trim(t.RepCodigo) = ? "+ condition +" " +
                "group by EstTabla, EstDescripcion " +
                "order by "+fechaCampo+" ASC";
            
            return SqliteManager.GetInstance().Query<Estados>(query,
                new string[] { Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public List<ExpandListItem<Estados>> GetAllTransaccionesEstados(int cliId = -1)
        {
            var list = new List<ExpandListItem<Estados>>();

            var currentCount = 0;

            var diasPermitidos = "365";

            var parDiasTransaccionesVisibles = DS_RepresentantesParametros.GetInstance().GetDiasTransaccionesVisibles();
            if (parDiasTransaccionesVisibles > 0)
            {
                diasPermitidos = parDiasTransaccionesVisibles.ToString();
            }

            var whereDiasPermitidos = " (cast(replace(cast(julianday(datetime('now')) - julianday(Fecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";

            if (myParametro.GetParCargasInventario())
            {
                currentCount = RegistryCount("Cargas", whereDiasPermitidos.Replace("Fecha", "CarFecha"));

                if(currentCount > 0)
                    list.Add(new ExpandListItem<Estados>() { TitId = -1, Count = currentCount, Descripcion = "Cargas de inventario", Childs = GetTransaccionesEstadosByTabla("Cargas", "CarEstatus", "Cargas de inventario", where:whereDiasPermitidos.Replace("Fecha", "CarFecha")) });
            }

            if (myParametro.GetParRequisicionesInventario())
            {
                currentCount = RegistryCount("RequisicionesInventario", whereDiasPermitidos.Replace("Fecha", "ReqFecha"));

                if (currentCount > 0)
                    list.Add(new ExpandListItem<Estados>() { TitId = 24, Count = currentCount, Descripcion = "Requisiciones de inventario", Childs = GetTransaccionesEstadosByTabla("RequisicionesInventario", "ReqEstatus", "Requisiciones de inventario", where:whereDiasPermitidos.Replace("Fecha", "ReqFecha")) });
            }

            var whereCliId = cliId != -1 ? " and CliID = " + cliId.ToString() + " " : "";

            try
            {
                if (myParametro.GetParPromociones())
                {
                    currentCount = RegistryCount("Entregas", whereDiasPermitidos.Replace("Fecha", "EntFecha") + " And EntTipo = 19 " + whereCliId);

                    if (currentCount > 0)
                        list.Add(new ExpandListItem<Estados>() { TitId = 19, Count = currentCount, Descripcion = "Promociones", Childs = GetTransaccionesEstadosByTabla("Entregas", "EntEstatus", "Promociones", where: whereDiasPermitidos.Replace("Fecha", "EntFecha") + " And EntTipo = 19 " + whereCliId) });
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            ////exclusiones de clientes
            try
            {
                currentCount = RegistryCount("SolicitudExclusionClientes", whereDiasPermitidos.Replace("Fecha", "SolFecha") + whereCliId);

                if (currentCount > 0)
                    list.Add(new ExpandListItem<Estados>() { TitId =29, Count = currentCount, Descripcion = "Solicitud Exclusion Clientes", Childs = GetTransaccionesEstadosByTabla("SolicitudExclusionClientes", "SolEstado", "SolicitudExclusionClientes", where: whereDiasPermitidos.Replace("Fecha", "SolFecha") + whereCliId) });

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            try
            {
                currentCount = RegistryCount("SolicitudActualizacionClienteDireccion", whereDiasPermitidos.Replace("Fecha", "SolFecha") + whereCliId);

                if (currentCount > 0)
                    list.Add(new ExpandListItem<Estados>() { TitId = 32, Count = currentCount, Descripcion = "Solicitud Actualizacion Cliente Dirección", Childs = GetTransaccionesEstadosByTabla("SolicitudActualizacionClienteDireccion", "SolEstado", "SolicitudActualizacionClienteDireccion", where: whereDiasPermitidos.Replace("Fecha", "SolFecha") + whereCliId) });

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            try
            {
                if (myParametro.GetParEntregasMercancia())
                {
                    currentCount = RegistryCount("Entregas", whereDiasPermitidos.Replace("Fecha", "EntFecha") + " And EntTipo = 17 " + whereCliId);

                    if (currentCount > 0)
                        list.Add(new ExpandListItem<Estados>() { TitId = 17, Count = currentCount, Descripcion = "Entregas de mercancía", Childs = GetTransaccionesEstadosByTabla("Entregas", "EntEstatus", "Entregas de mercancía", where: whereDiasPermitidos.Replace("Fecha", "EntFecha") + " And EntTipo = 17 " + whereCliId) });
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            try
            {
                if (myParametro.GetParCanastos())
                {
                    currentCount = RegistryCount("TransaccionesCanastos", whereDiasPermitidos.Replace("Fecha", "TraFecha") + " And TitOrigen = -1 " + whereCliId);

                    if (currentCount > 0)
                        list.Add(new ExpandListItem<Estados>() { TitId = 21, Count = currentCount, Descripcion = "Entrega de canastos", Childs = GetTransaccionesEstadosByTabla("TransaccionesCanastos", "TraEstatus", "Entregas de canastos", where: whereDiasPermitidos.Replace("Fecha", "EntFecha") + " And TitOrigen = -1 " + whereCliId) });

                    currentCount = RegistryCount("TransaccionesCanastos", whereDiasPermitidos.Replace("Fecha", "TraFecha") + " And TitOrigen = 1 " + whereCliId);

                    if (currentCount > 0)
                        list.Add(new ExpandListItem<Estados>() { TitId = 18, Count = currentCount, Descripcion = "Recepción de canastos", Childs = GetTransaccionesEstadosByTabla("TransaccionesCanastos", "TraEstatus", "Recepción de canastos", where: whereDiasPermitidos.Replace("Fecha", "EntFecha") + " And TitOrigen = 1 " + whereCliId) });
                }
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            if (myParametro.GetParOperativosMedicos())
            {
                currentCount = RegistryCount("Operativos", whereDiasPermitidos.Replace("Fecha", "OpeFecha"));

                if (currentCount > 0)
                    list.Add(new ExpandListItem<Estados>() { Count = currentCount, Descripcion = "Operativos médicos", Childs = GetTransaccionesEstadosByTabla("Operativos", "OpeEstado", "Operativos médicos", where:whereDiasPermitidos.Replace("Fecha", "OpeFecha")) });
            }

            if (myParametro.GetParTraspasos())
            {
                currentCount = RegistryCount("TransferenciasAlmacenes", whereDiasPermitidos.Replace("Fecha", "TraFecha"));

                if (currentCount > 0)
                    list.Add(new ExpandListItem<Estados>() { TitId = 40, Count = currentCount, Descripcion = "Transferencias de Almacenes", Childs = GetTransaccionesEstadosByTabla("TransferenciasAlmacenes", "TraEstado", "Transferencias de almacenes", where:whereDiasPermitidos.Replace("Fecha", "TraFecha")) });
            }

            if (myParametro.GetParQuejasServicio())
            {
                currentCount = RegistryCount("QuejasServicio", whereDiasPermitidos.Replace("Fecha", "QueFecha") + whereCliId);

                if(currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 54, Count = currentCount, Descripcion = "Quejas al servicio", Childs = GetTransaccionesEstadosByTabla("QuejasServicio", "QueEstatus", "Quejas al servicio", where:whereDiasPermitidos.Replace("Fecha", "QueFecha") + whereCliId) });
                }
            }

            if (myParametro.GetParConduces())
            {
                currentCount = RegistryCount("Conduces", whereDiasPermitidos.Replace("Fecha", "ConFecha") + whereCliId.Replace("CliID", "SupID"));

                if (currentCount > 0)
                    list.Add(new ExpandListItem<Estados>() { TitId = 51, Count = currentCount, Descripcion = "Conduces", Childs = GetTransaccionesEstadosByTabla("Conduces", "ConEstatus", "Conduces", where:whereDiasPermitidos.Replace("Fecha", "ConFecha") + whereCliId.Replace("CliID", "SupID")) });
                
                currentCount = RegistryCount("ConducesConfirmados", whereDiasPermitidos.Replace("Fecha", "ConFecha")  + whereCliId.Replace("CliID", "SupID"));
                if (currentCount > 0)              
                    list.Add(new ExpandListItem<Estados>() { TitId = 51, Count = currentCount, Descripcion = "Conduces confirmados", Childs = GetTransaccionesEstadosByTabla("ConducesConfirmados", "ConEstatus", "Conduces confirmados", where: whereDiasPermitidos.Replace("Fecha", "ConFecha") + whereCliId.Replace("CliID", "SupID")) });
                
            }

            if (myParametro.GetParReconciliacion())
            {
                currentCount = RegistryCount("Reconciliaciones", whereDiasPermitidos.Replace("Fecha", "RecFecha") + whereCliId);

                if(currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 28, Count = currentCount, Descripcion = "Reconciliaciones", Childs = GetTransaccionesEstadosByTabla("Reconciliaciones", "RecEstatus", "Reconciliaciones", where:whereDiasPermitidos.Replace("Fecha", "RecFecha") + whereCliId) });
                }
            }

            if (myParametro.GetParEntregasRepartidor() >= 1 && myParametro.GetParEntregasRepartidor() <= 3)
            {
                currentCount = RegistryCount("EntregasRepartidor", whereDiasPermitidos.Replace("Fecha", "EnrFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 27, Count = currentCount, Descripcion = "Orden de entregas", Childs = GetTransaccionesEstadosByTabla("EntregasRepartidor", "EnrEstatus", "Orden de entregas", true, where:whereDiasPermitidos.Replace("Fecha", "EnrFecha")) });
                }
                currentCount = RegistryCount("EntregasRepartidorConfirmados", whereDiasPermitidos.Replace("Fecha", "EnrFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 27, Count = currentCount, Descripcion = "Orden de entregas confirmados", Childs = GetTransaccionesEstadosByTabla("EntregasRepartidorConfirmados", "EnrEstatus", "Orden de entregas confirmados", true, where:whereDiasPermitidos.Replace("Fecha", "EnrFecha")) });
                }

                currentCount = RegistryCount("EntregasTransacciones", whereDiasPermitidos.Replace("Fecha", "EntFecha") + whereCliId);

                if(currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 55, Count = currentCount, Descripcion = "Entregas Transacciones", Childs = GetTransaccionesEstadosByTabla("EntregasTransacciones", "EntEstatus", "Entregas Transacciones", true, where:whereDiasPermitidos.Replace("Fecha", "EntFecha") + whereCliId) });
                }
                currentCount = RegistryCount("EntregasTransaccionesConfirmados",whereDiasPermitidos.Replace("Fecha", "EntFecha") + whereCliId);

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 55, Count = currentCount, Descripcion = "Entregas Transacciones confirmados", Childs = GetTransaccionesEstadosByTabla("EntregasTransaccionesConfirmados", "EntEstatus", "Entregas Transacciones confirmados", true, where: whereDiasPermitidos.Replace("Fecha", "EntFecha") + whereCliId) });
                }
            }

            if (myParametro.GetParCompras())
            {
                currentCount = RegistryCount("Compras", whereDiasPermitidos.Replace("Fecha", "ComFecha") + " And ComTipo = 1 " + whereCliId);

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 11, Count = currentCount, Descripcion = "Compras", Childs = GetTransaccionesEstadosByTabla("Compras", "ComEstatus", "Compras", false, where: whereDiasPermitidos.Replace("Fecha", "ComFecha") + " And ComTipo = 1 " + whereCliId) });
                }

                currentCount = RegistryCount("ComprasConfirmados", whereDiasPermitidos.Replace("Fecha", "ComFecha") + " And ComTipo = 1 " + whereCliId);

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 11, Count = currentCount, Descripcion = "Compras confirmados", Childs = GetTransaccionesEstadosByTabla("ComprasConfirmados", "ComEstatus", "Compras confirmados", true, where: whereDiasPermitidos.Replace("Fecha", "ComFecha") + " And ComTipo = 1 " + whereCliId) });
                }
            }

            if (myParametro.GetParCompras())
            {
                currentCount = RegistryCount("Compras", whereDiasPermitidos.Replace("Fecha", "ComFecha") + " And ComTipo = 2 " + whereCliId);

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 11, Count = currentCount, Descripcion = "PushMoney Rotacion", Childs = GetTransaccionesEstadosByTabla("Compras", "ComEstatus", "Compras", false, where: whereDiasPermitidos.Replace("Fecha", "ComFecha") + " And ComTipo = 2 " + whereCliId) });
                }

                currentCount = RegistryCount("ComprasConfirmados", whereDiasPermitidos.Replace("Fecha", "ComFecha") + " And ComTipo = 2 " + whereCliId);

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 11, Count = currentCount, Descripcion = "PushMoney Rotacion confirmados", Childs = GetTransaccionesEstadosByTabla("ComprasConfirmados", "ComEstatus", "Compras confirmados", true, where: whereDiasPermitidos.Replace("Fecha", "ComFecha") + " And ComTipo = 2 " + whereCliId) });
                }
            }

            if (myParametro.GetParCotizaciones())
            {
                currentCount = RegistryCount("Cotizaciones", whereDiasPermitidos.Replace("Fecha", "CotFecha") + whereCliId);

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 5, Count = currentCount, Descripcion = "Cotizaciones", Childs = GetTransaccionesEstadosByTabla("Cotizaciones", "CotEstatus", "Cotizaciones", true, where:whereDiasPermitidos.Replace("Fecha", "CotFecha") + whereCliId) });
                }

                currentCount = RegistryCount("CotizacionesConfirmados", whereDiasPermitidos.Replace("Fecha", "CotFecha") + whereCliId);

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 5, Count = currentCount, Descripcion = "Cotizaciones confirmados", Childs = GetTransaccionesEstadosByTabla("CotizacionesConfirmados", "CotEstatus", "Cotizaciones confirmados", true, where: whereDiasPermitidos.Replace("Fecha", "CotFecha") + whereCliId) });
                }
            }

            if (myParametro.GetParCuadres() > 0)
            {
                currentCount = RegistryCount("Cuadres", whereDiasPermitidos.Replace("Fecha", "CuaFechaInicio"));

                if(currentCount > 0)
                    list.Add(new ExpandListItem<Estados>() { Count = currentCount, Descripcion = "Cuadres", Childs = GetTransaccionesEstadosByTabla("Cuadres", "CuaEstatus", "Cuadres", true, where:whereDiasPermitidos.Replace("Fecha", "CuaFechaInicio")) });
            }

            if (myParametro.GetParDepositos())
            {
                currentCount = RegistryCount("Depositos", whereDiasPermitidos.Replace("Fecha", "DepFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 9, Count = currentCount, Descripcion = "Depositos", Childs = GetTransaccionesEstadosByTabla("Depositos", "DepEstatus", "Depositos", true, where:whereDiasPermitidos.Replace("Fecha", "DepFecha")) });
                }

                currentCount = RegistryCount("DepositosConfirmados");

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 9, Count = currentCount, Descripcion = "Depositos confirmados", Childs = GetTransaccionesEstadosByTabla("DepositosConfirmados", "DepEstatus", "Depositos confirmados", true) });
                }
            }

            if (myParametro.GetParDepositosCompras())
            {
                currentCount = RegistryCount("DepositosCompras", whereDiasPermitidos.Replace("Fecha", "DepFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 12, Count = currentCount, Descripcion = "Depositos compras", Childs = GetTransaccionesEstadosByTabla("DepositosCompras", "DepEstatus", "Depositos Compras", true, where:whereDiasPermitidos.Replace("Fecha", "DepFecha")) });
                }

                currentCount = RegistryCount("DepositosComprasConfirmados", whereDiasPermitidos.Replace("Fecha", "DepFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 12, Count = currentCount, Descripcion = "Depositos compras confirmados", Childs = GetTransaccionesEstadosByTabla("DepositosComprasConfirmados", "DepEstatus", "Depositos compras confirmados", true, where:whereDiasPermitidos.Replace("Fecha", "DepFecha")) });
                }
            }

            if (myParametro.GetParDevoluciones() || myParametro.GetParEntregasRepartidor() == 1 || myParametro.GetParEntregasRepartidor() == 3)
            {
                currentCount = RegistryCount("Devoluciones", whereDiasPermitidos.Replace("Fecha", "DevFecha") + whereCliId);

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 2, Count = currentCount, Descripcion = "Devoluciones", Childs = GetTransaccionesEstadosByTabla("Devoluciones", "DevEstatus", "Devoluciones", where: whereDiasPermitidos.Replace("Fecha", "DevFecha") + whereCliId) });
                }

                currentCount = RegistryCount("DevolucionesConfirmadas", whereDiasPermitidos.Replace("Fecha", "DevFecha") + whereCliId);

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 2, Count = currentCount, Descripcion = "Devoluciones confirmados", Childs = GetTransaccionesEstadosByTabla("DevolucionesConfirmadas", "DevEstatus", "Devoluciones confirmadas", where: whereDiasPermitidos.Replace("Fecha", "DevFecha") + whereCliId) });
                }
            }

            currentCount = RegistryCount("EntregasDocumentos", whereDiasPermitidos.Replace("Fecha", "EntFecha") + whereCliId);

            if (currentCount > 0)
            {
                list.Add(new ExpandListItem<Estados>() { TitId = 10, Count = currentCount, Descripcion = "Entrega documentos", Childs = GetTransaccionesEstadosByTabla("EntregasDocumentos", "EntEstatus", "Entrega documentos", where: whereDiasPermitidos.Replace("Fecha", "EntFecha") + whereCliId) });
            }

            currentCount = RegistryCount("EntregasDocumentosConfirmados", whereDiasPermitidos.Replace("Fecha", "EntFecha") + whereCliId);

            if (currentCount > 0)
            {
                list.Add(new ExpandListItem<Estados>() { TitId = 10, Count = currentCount, Descripcion = "Entrega documentos confirmados", Childs = GetTransaccionesEstadosByTabla("EntregasDocumentosConfirmados", "EntEstatus", "Entrega documentos confirmados", where: whereDiasPermitidos.Replace("Fecha", "EntFecha") + whereCliId) });
            }

            if (myParametro.GetParEncuestas())
            {
                currentCount = RegistryCount("Muestras", whereDiasPermitidos.Replace("Fecha", "MueFecha") + whereCliId);

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados> { TitId = 31, Count = currentCount, Descripcion = "Muestras (Encuestas)", Childs = GetTransaccionesEstadosByTabla("Muestras", "ifnull(MueEstatus, '1')", "Muestras", where: whereDiasPermitidos.Replace("Fecha", "MueFecha") + whereCliId) });
                }

                currentCount = RegistryCount("MuestrasConfirmadas", whereDiasPermitidos.Replace("Fecha", "MueFecha") + whereCliId);

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados> { TitId = 31, Count = currentCount,  Descripcion = "Muestras confirmados (Encuestas)", Childs = GetTransaccionesEstadosByTabla("MuestrasConfirmadas", "ifnull(MueEstatus, '1')", "Muestras confirmadas", where: whereDiasPermitidos.Replace("Fecha", "MueFecha") + whereCliId) });
                }
            }

            if (myParametro.GetParPedidos())
            {
                currentCount = RegistryCount("Pedidos", whereDiasPermitidos.Replace("Fecha", "PedFecha") + whereCliId);

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 1, Count = currentCount, Descripcion = "Pedidos", Childs = GetTransaccionesEstadosByTabla("Pedidos", "PedEstatus", "Pedidos", where: whereDiasPermitidos.Replace("Fecha", "PedFecha") + whereCliId) });
                }

                currentCount = RegistryCount("PedidosConfirmados", whereDiasPermitidos.Replace("Fecha", "PedFecha") + whereCliId);

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 1, Count = currentCount, Descripcion = "Pedidos confirmados", Childs = GetTransaccionesEstadosByTabla("PedidosConfirmados", "PedEstatus", "Pedidos confirmados", where: whereDiasPermitidos.Replace("Fecha", "PedFecha") + whereCliId) });
                }
            }

            if (myParametro.GetParAuditoriaPrecios())
            {
                currentCount = RegistryCount("AuditoriasPrecios", whereDiasPermitidos.Replace("Fecha", "AudFecha") + whereCliId);

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 67, Count = currentCount, Descripcion = "Auditorias Precios", Childs = GetTransaccionesEstadosByTabla("AuditoriasPrecios", "AudEstatus", "Auditorias Precios", where: whereDiasPermitidos.Replace("Fecha", "AudFecha") + whereCliId) });
                }
            }

            if (myParametro.GetParInventarioFisico() > 0)
            {
                currentCount = RegistryCount("InventarioFisico", whereDiasPermitidos.Replace("Fecha", "InfFecha") + whereCliId);

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 7, Count = currentCount, Descripcion = "Inventario Fisico", Childs = GetTransaccionesEstadosByTabla("InventarioFisico", "InvEstatus", "InventarioFisico", where: whereDiasPermitidos.Replace("Fecha", "InfFecha") + whereCliId) });
                }
            }

            if (myParametro.GetParColocacionProductos())
            {
                currentCount = RegistryCount("ColocacionProductos", whereDiasPermitidos.Replace("Fecha", "ColFecha") + whereCliId);

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 68, Count = currentCount, Descripcion = "Colocación Mercancías", Childs = GetTransaccionesEstadosByTabla("ColocacionProductos", "ColEstatus", "ColocacionProductos", where: whereDiasPermitidos.Replace("Fecha", "ColFecha") + whereCliId) });
                }
            }

            if (myParametro.GetParCobros())
            {
                currentCount = RegistryCount("Recibos", whereDiasPermitidos.Replace("Fecha", "RecFecha") + whereCliId);

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 3, Count = currentCount, Descripcion = "Recibos", Childs = GetTransaccionesEstadosByTabla("Recibos", "RecEstatus", "Recibos", where: whereDiasPermitidos.Replace("Fecha", "RecFecha") + whereCliId) });
                }

                currentCount = RegistryCount("RecibosConfirmados", whereDiasPermitidos.Replace("Fecha", "RecFecha") + whereCliId);

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 3, Count = currentCount, Descripcion = "Recibos confirmados", Childs = GetTransaccionesEstadosByTabla("RecibosConfirmados", "RecEstatus", "Recibos confirmados", where: whereDiasPermitidos.Replace("Fecha", "RecFecha") + whereCliId) });
                }
            }


            if (myParametro.GetParPushMoneyPorPagar())
            {
                currentCount = RegistryCount("PushMoneyPagos", whereDiasPermitidos.Replace("Fecha", "pusFecha") + whereCliId);

                if (currentCount > 0)
                    list.Add(new ExpandListItem<Estados>() { TitId = 52, Count = currentCount, Descripcion = "Recibos de pago PushMoney", Childs = GetTransaccionesEstadosByTabla("PushMoneyPagos", "pusEstatus", "Recibos de pago PushMoney", where: whereDiasPermitidos.Replace("Fecha", "pusFecha") + whereCliId) });
            }

            if (myParametro.GetParVentas())
            {
                currentCount = RegistryCount("Ventas", whereDiasPermitidos.Replace("Fecha", "VenFecha") + whereCliId);

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 4, Count = currentCount, Descripcion = "Ventas", Childs = GetTransaccionesEstadosByTabla("Ventas", "VenEstatus", "Ventas", where: whereDiasPermitidos.Replace("Fecha", "VenFecha") + whereCliId) });
                }

                currentCount = RegistryCount("VentasConfirmados", whereDiasPermitidos.Replace("Fecha", "VenFecha") + whereCliId);

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 4, Count = currentCount, Descripcion = "Ventas confirmados", Childs = GetTransaccionesEstadosByTabla("VentasConfirmados", "VenEstatus", "Ventas confirmados", where: whereDiasPermitidos.Replace("Fecha", "VenFecha") + whereCliId) });
                }
            }

            if (myParametro.GetParProspectos())
            {
                currentCount = RegistryCount("Clientes", "CliDatosOtros like '%P%' " + whereCliId);

                if(currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 53, Count = currentCount, Descripcion = "Prospectos", Childs = GetTransaccionesEstadosByTabla("Clientes", "CliEstatus", "Prospectos", true, "CliDatosOtros like '%P%' " + whereCliId) });
                }
            }

            if (myParametro.GetParGastos())
            {
                currentCount = RegistryCount("Gastos", whereDiasPermitidos.Replace("Fecha", "GasFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 16, Count = currentCount, Descripcion = "Gastos", Childs = GetTransaccionesEstadosByTabla("Gastos", "GasEstatus", "Gastos", true, where:whereDiasPermitidos.Replace("Fecha", "GasFecha")) });
                }

                currentCount = RegistryCount("GastosConfirmados", whereDiasPermitidos.Replace("Fecha", "GasFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 16, Count = currentCount, Descripcion = "Gastos confirmados", Childs = GetTransaccionesEstadosByTabla("GastosConfirmados", "GasEstatus", "Gastos confirmados", true, where:whereDiasPermitidos.Replace("Fecha", "GasFecha")) });
                }
            }

            if (myParametro.GetParRecibosNCPorDescuentoProntoPago() > 0 || myParametro.GetParNCPorDescuentoProntoPagoImpresion())
            {
                currentCount = RegistryCount("NCDPP", whereDiasPermitidos.Replace("Fecha", "NCDFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 22, Count = currentCount, Descripcion = "Nota de credito DPP", Childs = GetTransaccionesEstadosByTabla("NCDPP", "NCDEstatus", "Notas de credito", true, where:whereDiasPermitidos.Replace("Fecha", "NCDFecha")) });
                }             
            }

            currentCount = RegistryCount("Visitas", whereDiasPermitidos.Replace("Fecha", "VisFechaEntrada") + whereCliId);

            if (currentCount > 0)
            {
                list.Add(new ExpandListItem<Estados>() { TitId = 13, Count = currentCount, Descripcion = "Visitas", Childs = GetTransaccionesEstadosByTabla("Visitas", "VisEstatus", "Visitas", where: whereDiasPermitidos.Replace("Fecha", "VisFechaEntrada") + whereCliId) });
            }

            currentCount = RegistryCount("VisitasConfirmados", whereDiasPermitidos.Replace("Fecha", "VisFechaEntrada") + whereCliId);

            if (currentCount > 0)
            {
                list.Add(new ExpandListItem<Estados>() { TitId = 13, Count = currentCount, Descripcion = "Visitas confirmadas", Childs = GetTransaccionesEstadosByTabla("VisitasConfirmados", "VisEstatus", "Visitas confirmadas", where: whereDiasPermitidos.Replace("Fecha", "VisFechaEntrada") + whereCliId) });
            }

            currentCount = RegistryCount("ConteosFisicos", whereDiasPermitidos.Replace("Fecha", "ConFecha"));

            if(currentCount > 0)
            {
                list.Add(new ExpandListItem<Estados>() { TitId = 8, Count = currentCount, Descripcion = "Conteos fisicos", Childs = GetTransaccionesEstadosByTabla("ConteosFisicos", "ConEstatus", "Conteos fisicos", where:whereDiasPermitidos.Replace("Fecha", "ConFecha")) });
            }

            currentCount = RegistryCount("ConteosFisicosConfirmados", whereDiasPermitidos.Replace("Fecha", "ConFecha"));

            if(currentCount > 0)
            {
                list.Add(new ExpandListItem<Estados>() { TitId = 8, Count = currentCount, Descripcion = "Conteos fisicos confirmados", Childs = GetTransaccionesEstadosByTabla("ConteosFisicosConfirmados", "ConEstatus", "Conteos fisicos confirmados", where:whereDiasPermitidos.Replace("Fecha", "ConFecha")) });
            }

            currentCount = RegistryCount("Cambios", whereDiasPermitidos.Replace("Fecha", "CamFecha") + whereCliId);

            if (currentCount > 0)
            {
                list.Add(new ExpandListItem<Estados>() { TitId = 6, Count = currentCount, Descripcion = "Cambios Mercancia", Childs = GetTransaccionesEstadosByTabla("Cambios", "CamEstatus", "Cambios Mercancia", where: whereDiasPermitidos.Replace("Fecha", "CamFecha") + whereCliId) });
            }

            currentCount = RegistryCount("CambiosConfirmados", whereDiasPermitidos.Replace("Fecha", "CamFecha") + whereCliId);

            if (currentCount > 0)
            {
                list.Add(new ExpandListItem<Estados>() { TitId = 6, Count = currentCount, Descripcion = "Cambios Mercancia Confirmados", Childs = GetTransaccionesEstadosByTabla("CambiosConfirmados", "CamEstatus", "Cambios Mercancia", where: whereDiasPermitidos.Replace("Fecha", "CamFecha") + whereCliId) });
            }

            var temp = list.Where(x => x.Childs != null && x.Childs.Count > 0).ToList();

            list = temp;

            return list.OrderBy(x => x.Descripcion).ToList();
        }

        public List<ExpandListItem<Estados>> GetAllTransaccionesFecha(int cliId = -1)
        {
            var list = new List<ExpandListItem<Estados>>();

            var currentCount = 0;

            var diasPermitidos = "365";

            var parDiasTransaccionesVisibles = DS_RepresentantesParametros.GetInstance().GetDiasTransaccionesVisibles();
            if (parDiasTransaccionesVisibles > 0)
            {
                diasPermitidos = parDiasTransaccionesVisibles.ToString();
            }

            var whereDiasPermitidos = " (cast(replace(cast(julianday(datetime('now')) - julianday(Fecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";

            if (myParametro.GetParCargasInventario())
            {
                currentCount = RegistryCount("Cargas", whereDiasPermitidos.Replace("Fecha", "CarFecha"));

                if (currentCount > 0)
                    list.Add(new ExpandListItem<Estados>() { Count = currentCount, Descripcion = "Cargas de inventario", Childs = GetTransaccionesByFecha("Cargas", "CarFecha", "CarEstatus", "Cargas de inventario", where:whereDiasPermitidos.Replace("Fecha", "CarFecha")) });
            }

            if (myParametro.GetParRequisicionesInventario())
            {
                currentCount = RegistryCount("RequisicionesInventario", whereDiasPermitidos.Replace("Fecha", "ReqFecha"));

                if (currentCount > 0)
                    list.Add(new ExpandListItem<Estados>() { Count = currentCount, Descripcion = "Requisiciones de inventario", Childs = GetTransaccionesByFecha("RequisicionesInventario", "ReqFecha", "ReqEstatus", "Requisiciones de inventario", where:whereDiasPermitidos.Replace("Fecha", "ReqFecha")) });
            }

            try
            {
                if (myParametro.GetParPromociones())
                {
                    currentCount = RegistryCount("Entregas", whereDiasPermitidos.Replace("Fecha", "EntFecha") + " And EntTipo = 19");

                    if (currentCount > 0)
                        list.Add(new ExpandListItem<Estados>() { TitId = 19, Count = currentCount, Descripcion = "Promociones", Childs = GetTransaccionesByFecha("Entregas", "EntFecha", "EntEstatus", "Promociones", where: whereDiasPermitidos.Replace("Fecha", "EntFecha") + " And EntTipo = 19") });
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }


            //exclusiones de clientes
            try
            {
                currentCount = RegistryCount("SolicitudExclusionClientes", whereDiasPermitidos.Replace("Fecha", "SolFecha"));

                if (currentCount > 0)
                    list.Add(new ExpandListItem<Estados>() { TitId = 29, Count = currentCount, Descripcion = "Solicitud Exclusion Clientes", Childs = GetTransaccionesByFecha("SolicitudExclusionClientes", "SolFecha", "SolEstado", "Solicitud Exclusion Clientes", where: whereDiasPermitidos.Replace("Fecha", "SolFecha")) });

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            try
            {
                currentCount = RegistryCount("SolicitudActualizacionClienteDireccion", whereDiasPermitidos.Replace("Fecha", "SolFecha"));

                if (currentCount > 0)
                    list.Add(new ExpandListItem<Estados>() { TitId = 32, Count = currentCount, Descripcion = "Solicitud Actualizacion Cliente Dirección", Childs = GetTransaccionesByFecha("SolicitudActualizacionClienteDireccion", "SolFecha", "SolEstado", "Solicitud Actualizacion Cliente Dirección", where: whereDiasPermitidos.Replace("Fecha", "SolFecha")) });

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            try
            {
                if (myParametro.GetParEntregasMercancia())
                {
                    currentCount = RegistryCount("Entregas", whereDiasPermitidos.Replace("Fecha", "EntFecha") + " And EntTipo = 17");

                    if (currentCount > 0)
                        list.Add(new ExpandListItem<Estados>() { TitId = 17, Count = currentCount, Descripcion = "Entregas de mercancía", Childs = GetTransaccionesByFecha("Entregas", "EntFecha", "EntEstatus", "Entregas de mercancía", where: whereDiasPermitidos.Replace("Fecha", "EntFecha") + " And EntTipo = 17") });
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            try
            {
                if (myParametro.GetParCanastos())
                {
                    currentCount = RegistryCount("TransaccionesCanastos", whereDiasPermitidos.Replace("Fecha", "TraFecha") + " And TitOrigen = -1");

                    if (currentCount > 0)
                        list.Add(new ExpandListItem<Estados>() { TitId = 21, Count = currentCount, Descripcion = "Entregas de canastos", Childs = GetTransaccionesByFecha("TransaccionesCanastos", "TraFecha", "TraEstatus", "Entregas de canastos", where: whereDiasPermitidos.Replace("Fecha", "TraFecha") + " And TitOrigen = -1") });

                    currentCount = RegistryCount("TransaccionesCanastos", whereDiasPermitidos.Replace("Fecha", "TraFecha") + " And TitOrigen = 1");

                    if (currentCount > 0)
                        list.Add(new ExpandListItem<Estados>() { TitId = 18, Count = currentCount, Descripcion = "Recepción de canastos", Childs = GetTransaccionesByFecha("TransaccionesCanastos", "TraFecha", "TraEstatus", "Recepción de canastos", where: whereDiasPermitidos.Replace("Fecha", "TraFecha") + " And TitOrigen = 1") });
                }
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            if (myParametro.GetParOperativosMedicos())
            {
                currentCount = RegistryCount("Operativos", whereDiasPermitidos.Replace("Fecha", "OpeFecha"));

                if (currentCount > 0)
                    list.Add(new ExpandListItem<Estados>() { Count = currentCount, Descripcion = "Operativos médicos", Childs = GetTransaccionesByFecha("Operativos", "OpeFecha", "OpeEstado", "Operativos médicos", where:whereDiasPermitidos.Replace("Fecha", "OpeFecha")) });
            }

            if (myParametro.GetParTraspasos())
            {
                currentCount = RegistryCount("TransferenciasAlmacenes", whereDiasPermitidos.Replace("Fecha", "TraFecha"));

                if (currentCount > 0)
                    list.Add(new ExpandListItem<Estados>() { TitId = 40, Count = currentCount, Descripcion = "Transferencias de almacenes", Childs = GetTransaccionesByFecha("TransferenciasAlmacenes", "TraFecha", "TraEstado", "Transferencias de almacenes", where:whereDiasPermitidos.Replace("Fecha", "TraFecha")) });
            }

            if (myParametro.GetParQuejasServicio())
            {
                currentCount = RegistryCount("QuejasServicio", whereDiasPermitidos.Replace("Fecha", "QueFecha"));

                if (currentCount > 0)
                    list.Add(new ExpandListItem<Estados>() { TitId = 54, Count = currentCount, Descripcion = "Quejas al servicio", Childs = GetTransaccionesByFecha("QuejasServicio", "QueFecha", "QueEstatus", "Quejas al servicio", where:whereDiasPermitidos.Replace("Fecha", "QueFecha")) });
            }

            if (myParametro.GetParConduces())
            {
                currentCount = RegistryCount("Conduces", whereDiasPermitidos.Replace("Fecha", "ConFecha"));

                if (currentCount > 0)
                    list.Add(new ExpandListItem<Estados>() { TitId = 51, Count = currentCount, Descripcion = "Conduces", Childs = GetTransaccionesByFecha("Conduces", "ConFecha", "ConEstatus", "Conduces", where:whereDiasPermitidos.Replace("Fecha", "ConFecha")) });

                currentCount = RegistryCount("ConducesConfirmados", whereDiasPermitidos.Replace("Fecha", "ConFecha"));
                if (currentCount > 0)
                    list.Add(new ExpandListItem<Estados>() { TitId = 51, Count = currentCount, Descripcion = "Conduces confirmados", Childs = GetTransaccionesEstadosByTabla("ConducesConfirmados", "ConEstatus", "Conduces confirmados", where:whereDiasPermitidos.Replace("Fecha", "ConFecha")) });

            }

            if (myParametro.GetParEntregasRepartidor() >= 1 && myParametro.GetParEntregasRepartidor() <= 3)
            {
                currentCount = RegistryCount("EntregasRepartidor", whereDiasPermitidos.Replace("Fecha", "EnrFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 27, Count = currentCount, Descripcion = "Entregas repartidor", Childs = GetTransaccionesByFecha("EntregasRepartidor", "EnrFecha", "EnrEstatus", "Entregas repartidor", true, where:whereDiasPermitidos.Replace("Fecha", "EnrFecha")) });
                }
                currentCount = RegistryCount("EntregasRepartidorConfirmados", whereDiasPermitidos.Replace("Fecha", "EnrFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 27, Count = currentCount, Descripcion = "Entregas repartidor confirmados", Childs = GetTransaccionesEstadosByTabla("EntregasRepartidorConfirmados", "EnrEstatus", "Entregas repartidor confirmados", true, where:whereDiasPermitidos.Replace("Fecha", "EnrFecha")) });
                }

                currentCount = RegistryCount("EntregasTransacciones", whereDiasPermitidos.Replace("Fecha", "EntFecha`"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 55, Count = currentCount, Descripcion = "Entregas transacciones", Childs = GetTransaccionesByFecha("EntregasTransacciones", "EntFecha", "EntEstatus", "Entregas transacciones", true, where:whereDiasPermitidos.Replace("Fecha", "EntFecha")) });
                }
                currentCount = RegistryCount("EntregasTransaccionesConfirmados", whereDiasPermitidos.Replace("Fecha", "EntFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 55, Count = currentCount, Descripcion = "Entregas transacciones confirmados", Childs = GetTransaccionesEstadosByTabla("EntregasTransaccionesConfirmados", "EntEstatus", "Entregas transacciones confirmados", true, where:whereDiasPermitidos.Replace("Fecha", "EntFecha")) });
                }
            }

            if (myParametro.GetParReconciliacion())
            {
                currentCount = RegistryCount("Reconciliaciones", whereDiasPermitidos.Replace("Fecha", "RecFecha"));

                if(currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 28, Count = currentCount, Descripcion = "Reconciliaciones", Childs = GetTransaccionesByFecha("Reconciliaciones", "RecFecha", "RecEstatus", "Reconciliaciones", where:whereDiasPermitidos.Replace("Fecha", "RecFecha")) });
                }
            }

            if (myParametro.GetParCompras())
            {
                currentCount = RegistryCount("Compras", whereDiasPermitidos.Replace("Fecha", "ComFecha") + " And ComTipo = 1");

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 11, Count = currentCount, Descripcion = "Compras", Childs = GetTransaccionesByFecha("Compras", "ComFecha", "ComEstatus", "Compras", false, whereDiasPermitidos.Replace("Fecha", "ComFecha") + " And ComTipo = 1") });
                }

                currentCount = RegistryCount("ComprasConfirmados", whereDiasPermitidos.Replace("Fecha", "ComFecha") + " And ComTipo = 1");

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 11, Count = currentCount, Descripcion = "Compras confirmados", Childs = GetTransaccionesByFecha("ComprasConfirmados", "ComFecha", "ComEstatus", "Compras confirmados", true, whereDiasPermitidos.Replace("Fecha", "ComFecha") + " And ComTipo = 1") });
                }
            }

            if (myParametro.GetParCompras())
            {
                currentCount = RegistryCount("Compras", whereDiasPermitidos.Replace("Fecha", "ComFecha") + " And ComTipo = 2");

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 11, Count = currentCount, Descripcion = "PushMoney Rotacion", Childs = GetTransaccionesByFecha("Compras", "ComFecha", "ComEstatus", "Compras", false, whereDiasPermitidos.Replace("Fecha", "ComFecha") + " And ComTipo = 2") });
                }

                currentCount = RegistryCount("ComprasConfirmados", whereDiasPermitidos.Replace("Fecha", "ComFecha") + " And ComTipo = 2");

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 11, Count = currentCount, Descripcion = "PushMoney Rotacion confirmados", Childs = GetTransaccionesByFecha("ComprasConfirmados", "ComFecha", "ComEstatus", "Compras confirmados", true, whereDiasPermitidos.Replace("Fecha", "ComFecha") + " And ComTipo = 2") });
                }
            }

            if (myParametro.GetParCotizaciones())
            {
                currentCount = RegistryCount("Cotizaciones", whereDiasPermitidos.Replace("Fecha", "CotFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 5, Count = currentCount, Descripcion = "Cotizaciones", Childs = GetTransaccionesByFecha("Cotizaciones", "CotFecha", "CotEstatus", "Cotizaciones", true, where:whereDiasPermitidos.Replace("Fecha", "CotFecha")) });
                }

                currentCount = RegistryCount("CotizacionesConfirmados", whereDiasPermitidos.Replace("Fecha", "CotFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 5, Count = currentCount, Descripcion = "Cotizaciones confirmados", Childs = GetTransaccionesByFecha("CotizacionesConfirmados", "CotFecha", "CotEstatus", "Cotizaciones confirmados", true, where:whereDiasPermitidos.Replace("Fecha", "ComFecha")) });
                }
            }

            if (myParametro.GetParCuadres() > 0)
            {
                currentCount = RegistryCount("Cuadres", whereDiasPermitidos.Replace("Fecha", "CuaFechaInicio"));

                if (currentCount > 0)
                    list.Add(new ExpandListItem<Estados>() { Count = currentCount, Descripcion = "Cuadres", Childs = GetTransaccionesByFecha("Cuadres", "CuaFechaInicio", "CuaEstatus", "Cuadres", true, where:whereDiasPermitidos.Replace("Fecha", "CuaFechaInicio")) });
            }

            if (myParametro.GetParDepositos())
            {
                currentCount = RegistryCount("Depositos", whereDiasPermitidos.Replace("Fecha", "DepFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 9, Count = currentCount, Descripcion = "Depositos", Childs = GetTransaccionesByFecha("Depositos", "DepFecha", "DepEstatus", "Depositos", true, where:whereDiasPermitidos.Replace("Fecha", "DepFecha")) });
                }

                currentCount = RegistryCount("DepositosConfirmados", whereDiasPermitidos.Replace("Fecha", "DepFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 9, Count = currentCount, Descripcion = "Depositos confirmados", Childs = GetTransaccionesByFecha("DepositosConfirmados", "DepFecha", "DepEstatus", "Depositos confirmados", true, where:whereDiasPermitidos.Replace("Fecha", "DepFecha")) });
                }
            }

            if (myParametro.GetParDepositosCompras())
            {
                currentCount = RegistryCount("DepositosCompras", whereDiasPermitidos.Replace("Fecha", "DepFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 12, Count = currentCount, Descripcion = "Depositos compras", Childs = GetTransaccionesByFecha("DepositosCompras", "DepFecha","DepEstatus", "Depositos Compras", true, where:whereDiasPermitidos.Replace("Fecha", "DepFecha")) });
                }

                currentCount = RegistryCount("DepositosComprasConfirmados", whereDiasPermitidos.Replace("Fecha", "DepFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 12, Count = currentCount, Descripcion = "Depositos compras confirmados", Childs = GetTransaccionesByFecha("DepositosComprasConfirmados", "DepFecha", "DepEstatus", "Depositos compras confirmados", true, where:whereDiasPermitidos.Replace("Fecha", "DepFecha")) });
                }
            }

            if (myParametro.GetParDevoluciones() || myParametro.GetParEntregasRepartidor() == 1 || myParametro.GetParEntregasRepartidor() == 3)
            {
                currentCount = RegistryCount("Devoluciones", whereDiasPermitidos.Replace("Fecha", "DevFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 2, Count = currentCount, Descripcion = "Devoluciones", Childs = GetTransaccionesByFecha("Devoluciones", "DevFecha", "DevEstatus", "Devoluciones", where:whereDiasPermitidos.Replace("Fecha", "DevFecha")) });
                }

                currentCount = RegistryCount("DevolucionesConfirmadas", whereDiasPermitidos.Replace("Fecha", "DevFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 2, Count = currentCount, Descripcion = "Devoluciones confirmadas", Childs = GetTransaccionesByFecha("DevolucionesConfirmadas", "DevFecha", "DevEstatus", "Devoluciones confirmadas", where:whereDiasPermitidos.Replace("Fecha", "DevFecha")) });
                }
            }
            
             currentCount = RegistryCount("EntregasDocumentos", whereDiasPermitidos.Replace("Fecha", "EntFecha"));

             if (currentCount > 0)
             {
                 list.Add(new ExpandListItem<Estados>() { TitId = 10, Count = currentCount, Descripcion = "Entrega documentos", Childs = GetTransaccionesByFecha("EntregasDocumentos", "EntFecha", "EntEstatus", "Entrega documentos", where:whereDiasPermitidos.Replace("Fecha", "EntFecha")) });
             }

             currentCount = RegistryCount("EntregasDocumentosConfirmados", whereDiasPermitidos.Replace("Fecha", "EntFecha"));

             if (currentCount > 0)
             {
                 list.Add(new ExpandListItem<Estados>() { TitId = 10, Count = currentCount, Descripcion = "Entrega documentos confirmados", Childs = GetTransaccionesByFecha("EntregasDocumentosConfirmados", "EntFecha", "EntEstatus", "Entrega documentos confirmados", where:whereDiasPermitidos.Replace("Fecha", "EntFecha")) });
             }
            

            if (myParametro.GetParEncuestas())
            {
                currentCount = RegistryCount("Muestras", whereDiasPermitidos.Replace("Fecha", "MueFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados> { TitId = 31, Count = currentCount, Descripcion = "Muestras (Encuestas)", Childs = GetTransaccionesByFecha("Muestras", "MueFecha", "ifnull(MueEstatus, '1')", "Muestras", where:whereDiasPermitidos.Replace("Fecha", "MueFecha")) });
                }

                currentCount = RegistryCount("MuestrasConfirmadas", whereDiasPermitidos.Replace("Fecha", "MueFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados> { TitId = 31, Count = currentCount, Descripcion = "Muestras confirmados (Encuestas)", Childs = GetTransaccionesByFecha("MuestrasConfirmadas", "MueFecha", "ifnull(MueEstatus, '1')", "Muestras confirmadas", where:whereDiasPermitidos.Replace("Fecha", "MueFecha")) });
                }
            }

            if (myParametro.GetParPedidos())
            {
                currentCount = RegistryCount("Pedidos", whereDiasPermitidos.Replace("Fecha", "PedFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 1, Count = currentCount, Descripcion = "Pedidos", Childs = GetTransaccionesByFecha("Pedidos", "PedFecha", "PedEstatus", "Pedidos", where:whereDiasPermitidos.Replace("Fecha", "PedFecha")) });
                }

                currentCount = RegistryCount("PedidosConfirmados", whereDiasPermitidos.Replace("Fecha", "PedFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 1, Count = currentCount, Descripcion = "Pedidos confirmados", Childs = GetTransaccionesByFecha("PedidosConfirmados", "PedFecha", "PedEstatus", "Pedidos confirmados", where:whereDiasPermitidos.Replace("Fecha", "PedFecha")) });
                }
            }

            if (myParametro.GetParAuditoriaPrecios())
            {
                currentCount = RegistryCount("AuditoriasPrecios", whereDiasPermitidos.Replace("Fecha", "AudFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 67, Count = currentCount, Descripcion = "Auditorias Precios", Childs = GetTransaccionesByFecha("AuditoriasPrecios", "AudFecha", "AudEstatus", "Auditorias de precios", where: whereDiasPermitidos.Replace("Fecha", "AudFecha")) });
                }
            }

            if (myParametro.GetParCobros())
            {
                currentCount = RegistryCount("Recibos", whereDiasPermitidos.Replace("Fecha", "RecFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 3, Count = currentCount, Descripcion = "Recibos", Childs = GetTransaccionesByFecha("Recibos", "RecFecha", "RecEstatus", "Recibos", where:whereDiasPermitidos.Replace("Fecha", "RecFecha")) });
                }

                currentCount = RegistryCount("RecibosConfirmados", whereDiasPermitidos.Replace("Fecha", "RecFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 3, Count = currentCount, Descripcion = "Recibos confirmados", Childs = GetTransaccionesByFecha("RecibosConfirmados", "RecFecha", "RecEstatus", "Recibos confirmados", where: whereDiasPermitidos.Replace("Fecha", "RecFecha")) });
                }
            }

            if (myParametro.GetParPushMoneyPorPagar())
            {
                currentCount = RegistryCount("PushMoneyPagos", whereDiasPermitidos.Replace("Fecha", "pusFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 52, Count = currentCount, Descripcion = "Recibos de pago PushMoney", Childs = GetTransaccionesByFecha("PushMoneyPagos", "pusFecha", "pusEstatus", "Recibos de pago PushMoney", where: whereDiasPermitidos.Replace("Fecha", "pusFecha")) });
                }
                
            }

            if (myParametro.GetParVentas())
            {
                currentCount = RegistryCount("Ventas", whereDiasPermitidos.Replace("Fecha", "VenFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 4, Count = currentCount, Descripcion = "Ventas", Childs = GetTransaccionesByFecha("Ventas", "VenFecha", "VenEstatus", "Ventas", where: whereDiasPermitidos.Replace("Fecha", "VenFecha")) });
                }

                currentCount = RegistryCount("VentasConfirmados", whereDiasPermitidos.Replace("Fecha", "VenFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 4, Count = currentCount, Descripcion = "Ventas confirmados", Childs = GetTransaccionesByFecha("VentasConfirmados", "VenFecha", "VenEstatus", "Ventas confirmados", where: whereDiasPermitidos.Replace("Fecha", "VenFecha")) });
                }
            }

            if (myParametro.GetParProspectos())
            {
                currentCount = RegistryCount("Clientes", "CliDatosOtros like '%P%' ");

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 53, Count = currentCount, Descripcion = "Prospectos", Childs = GetTransaccionesByFecha("Clientes", "CliFechaActualizacion", "CliEstatus", "Prospectos", true, "CliDatosOtros like '%P%' ") });
                }
            }

            if (myParametro.GetParGastos())
            {
                currentCount = RegistryCount("Gastos", whereDiasPermitidos.Replace("Fecha", "GasFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 16, Count = currentCount, Descripcion = "Gastos", Childs = GetTransaccionesByFecha("Gastos", "GasFecha", "GasEstatus", "Gastos", true, where: whereDiasPermitidos.Replace("Fecha", "GasFecha")) });
                }

                currentCount = RegistryCount("GastosConfirmados", whereDiasPermitidos.Replace("Fecha", "GasFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { TitId = 16, Count = currentCount, Descripcion = "Gastos confirmados", Childs = GetTransaccionesByFecha("GastosConfirmados", "GasFecha", "GasEstatus", "Gastos confirmados", true, where: whereDiasPermitidos.Replace("Fecha", "GasFecha")) });
                }
            }


            if (myParametro.GetParRecibosNCPorDescuentoProntoPago() > 0 || myParametro.GetParNCPorDescuentoProntoPagoImpresion())
            {
                currentCount = RegistryCount("NCDPP", whereDiasPermitidos.Replace("Fecha", "NCDFecha"));

                if (currentCount > 0)
                {
                    list.Add(new ExpandListItem<Estados>() { Count = currentCount, Descripcion = "Nota de credito DPP", Childs = GetTransaccionesByFecha("NCDPP", "NCDFecha", "NCDEstatus", "Notas de credito", true, where: whereDiasPermitidos.Replace("Fecha", "NCDFecha")) });
                }


            }

            currentCount = RegistryCount("Visitas", whereDiasPermitidos.Replace("Fecha", "VisFechaEntrada"));

            if (currentCount > 0)
            {
                list.Add(new ExpandListItem<Estados>() { TitId = 13, Count = currentCount, Descripcion = "Visitas", Childs = GetTransaccionesByFecha("Visitas", "VisFechaEntrada", "VisEstatus", "Visitas", where: whereDiasPermitidos.Replace("Fecha", "VisFechaEntrada")) });
            }

            currentCount = RegistryCount("VisitasConfirmados", whereDiasPermitidos.Replace("Fecha", "VisFechaEntrada"));

            if (currentCount > 0)
            {
                list.Add(new ExpandListItem<Estados>() { TitId = 13, Count = currentCount, Descripcion = "Visitas confirmadas", Childs = GetTransaccionesByFecha("VisitasConfirmados", "VisFechaEntrada", "VisEstatus", "Visitas confirmadas", where: whereDiasPermitidos.Replace("Fecha", "VisFechaEntrada")) });
            }

            currentCount = RegistryCount("ConteosFisicos", whereDiasPermitidos.Replace("Fecha", "ConFecha"));

            if (currentCount > 0)
            {
                list.Add(new ExpandListItem<Estados>() { TitId = 8, Count = currentCount, Descripcion = "Conteos fisicos", Childs = GetTransaccionesByFecha("ConteosFisicos", "ConFecha", "ConEstatus", "Conteos fisicos", where: whereDiasPermitidos.Replace("Fecha", "ConFecha")) });
            }

            currentCount = RegistryCount("ConteosFisicosConfirmados", whereDiasPermitidos.Replace("Fecha", "ConFecha"));

            if (currentCount > 0)
            {
                list.Add(new ExpandListItem<Estados>() { TitId = 8, Count = currentCount, Descripcion = "Conteos fisicos confirmados", Childs = GetTransaccionesByFecha("ConteosFisicosConfirmados", "ConFecha", "ConEstatus", "Conteos fisicos confirmados", where: whereDiasPermitidos.Replace("Fecha", "ConFecha")) });
            }

            currentCount = RegistryCount("Cambios", whereDiasPermitidos.Replace("Fecha", "CamFecha"));

            if (currentCount > 0)
            {
                list.Add(new ExpandListItem<Estados>() { TitId = 6, Count = currentCount, Descripcion = "Cambios Mercancia", Childs = GetTransaccionesByFecha("Cambios", "CamFecha", "CamEstatus", "Cambios Mercancia", where: whereDiasPermitidos.Replace("Fecha", "CamFecha")) });
            }

            currentCount = RegistryCount("CambiosConfirmados", whereDiasPermitidos.Replace("Fecha", "CamFecha"));

            if (currentCount > 0)
            {
                list.Add(new ExpandListItem<Estados>() { TitId = 6, Count = currentCount, Descripcion = "Cambios Mercancia confirmados", Childs = GetTransaccionesByFecha("CambiosConfirmados", "CamFecha", "CamEstatus", "Cambios Mercancia Confirmados", where: whereDiasPermitidos.Replace("Fecha", "CamFecha")) });
            }

            currentCount = RegistryCount("ColocacionProductos", whereDiasPermitidos.Replace("Fecha", "ColFecha"));

            if (currentCount > 0)
            {
                list.Add(new ExpandListItem<Estados>() { TitId = 68, Count = currentCount, Descripcion = "Colocación Mercancía", Childs = GetTransaccionesByFecha("ColocacionProductos", "ColFecha", "ColEstatus", "Colocación Mercancía", where: whereDiasPermitidos.Replace("Fecha", "ColFecha")) });
            }

            var temp = list.Where(x => x.Childs != null && x.Childs.Count > 0).ToList();

            list = temp;

            return list.OrderBy(x => x.Descripcion).ToList();
        }

        private List<ExpandListItem<Estados>> GetTransaccionesEstadosByTabla(string tableName, string estCampo, string title, bool notUseClient = false, string where = null)
        {
            try
            {
                var list = new List<ExpandListItem<Estados>>();

                var estados = GetByTabla(tableName, estCampo, notUseClient, where);
                int verftablas = 0;

                if (tableName == "Compras" && where.Contains("2"))
                {
                    verftablas = 1;
                }
                else if (tableName == "ComprasConfirmados" && where.Contains("2"))
                {
                    verftablas = 2;
                }

                foreach (Estados est in estados)
                {
                    if(verftablas == 1)
                    {
                        est.EstTabla = "ComprasPushMoney";
                    }
                    else if(verftablas == 2)
                    {
                        est.EstTabla = "ComprasPushMoneyConfirmados";
                    }

                    var item = new ExpandListItem<Estados>
                    {
                        Title = title,
                        Descripcion = est.EstDescripcion,
                        IsChild = true,
                        Data = est,
                        Count = est.Count
                    };

                    list.Add(item);
                }

                return list;

            }
            catch(Exception e)
            {
                Console.Write(e.Message);
                return new List<ExpandListItem<Estados>>();
            }
        }

        private List<ExpandListItem<Estados>> GetTransaccionesByFecha(string tableName, string fechaCampo, string estCampo, string title, bool notUseClient = false, string where = null)
        {
            try
            {
                var list = new List<ExpandListItem<Estados>>();

                var estados = GetByFecha(tableName, fechaCampo, estCampo, notUseClient, where);
                int verftablas = 0;

                if (tableName == "Compras" && where.Contains("2"))
                {
                    verftablas = 1;
                }
                else if (tableName == "ComprasConfirmados" && where.Contains("2"))
                {
                    verftablas = 2;
                }

                foreach (Estados est in estados)
                {
                    if (verftablas == 1)
                    {
                        est.EstTabla = "ComprasPushMoney";
                    }
                    else if (verftablas == 2)
                    {
                        est.EstTabla = "ComprasPushMoneyConfirmados";
                    }

                    var item = new ExpandListItem<Estados>
                    {
                        Title = title,
                        Descripcion = est.EstDescripcion,
                        IsChild = true,
                        Data = est,
                        Count = est.Count
                    };

                    list.Add(item);
                }

                return list.Where(x=>x.Count > 0).ToList();

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return new List<ExpandListItem<Estados>>();
            }
        }

        private int RegistryCount(string tableName, string where = null)
        {
            try
            {
                
                string condition = " where 1=1 ";

                var whereDias = GetDiasTransaccionesVisibles(tableName) + " ";

                if (!string.IsNullOrWhiteSpace(whereDias))
                {
                    condition += " and " + whereDias;
                }


                if (!string.IsNullOrWhiteSpace(where))
                {
                    condition += " and " + where + " ";
                }

                var list = SqliteManager.GetInstance().Query<Totales>("select count(*) as CantidadTotal from " + tableName + condition + " limit 1", new string[] { });

                if (list != null && list.Count > 0)
                {
                    return list[0].CantidadTotal;
                }

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return 0;
        }

        public Estados GetByTablaAndEstatus(string tableName, string estado)
        {
            if(tableName == "ComprasPushMoney")
            {
                tableName = "Compras";
            }
            else if(tableName == "ComprasPushMoneyConfirmados")
            {
                tableName = "ComprasConfirmados";
            }

            return SqliteManager.GetInstance().Query<Estados>("select EstTabla, estOpciones, EstEstado from Estados " +
                "where trim(EstTabla) = ? and trim(EstEstado) = ?", new string[] { tableName, estado }).FirstOrDefault();
        }

        public string GetDiasTransaccionesVisibles(string tableName)
        {
            string diasPermitidos = "365";
            string where = null;
            if (DS_RepresentantesParametros.GetInstance().GetDiasTransaccionesVisibles() > 0)
            {
                diasPermitidos = DS_RepresentantesParametros.GetInstance().GetDiasTransaccionesVisibles().ToString();
            }

            if (tableName == "ComprasPushMoney")
            {
                tableName = "Compras";
            }
            else if (tableName == "ComprasPushMoneyConfirmados")
            {
                tableName = "ComprasConfirmados";
            }

            switch (tableName.ToUpper())
            {
                case "PEDIDOS":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(PedFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "PEDIDOSCONFIRMADOS":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(PedFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "COMPRAS":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(ComFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "CLIENTES":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(CliFechaActualizacion) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "COMPRASCONFIRMADOS":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(ComFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "COTIZACIONES":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(CotFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "COTIZACIONESCONFIRMADOS":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(CotFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "RECIBOS":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(RecFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "RECIBOSCONFIRMADOS":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(RecFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "DEVOLUCIONES":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(DevFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "DEVOLUCIONESCONFIRMADAS":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(DevFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "DEPOSITOS":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(DepFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "DEPOSITOSCONFIRMADOS":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(DepFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "DEPOSITOSCOMPRAS":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(DepFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "DEPOSITOSCOMPRASCONFIRMADOS":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(DepFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "VISITAS":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(VisFechaEntrada) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "VISITASCONFIRMADOS":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(VisFechaEntrada) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "ENTREGASDOCUMENTOS":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(EntFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "ENTREGASDOCUMENTOSCONFIRMADOS":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(EntFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "MUESTRAS":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(MueFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "MUESTRASCONFIRMADOS":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(MueFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "CUADRES":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(CuaFechaInicio) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "VENTAS":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(VenFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                      //" order by VenSecuencia desc";
                    break;
                case "VENTASCONFIRMADOS":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(VenFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "CARGAS":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(CarFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "GASTOS":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(GasFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "GASTOSCONFIRMADOS":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(GasFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "NCDPP":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(NCDFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "CONTEOSFISICOS":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(ConFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "CONTEOSFISICOSCONFIRMADOS":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(ConFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "CONDUCES":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(ConFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "CONDUCESCONFIRMADOS":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(ConFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "CAMBIOS":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(CamFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
                case "CAMBIOSCONFIRMADOS":
                    where += "(cast(replace(cast(julianday(datetime('now')) - julianday(CamFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " ";
                    break;
            }
            return where;
        
        }

        public bool IsValidToPrint()
        {
            var result = SqliteManager.GetInstance().Query<Estados>("select estOpciones from Estados where upper(EstTabla) =? and EstEstado = 1 ", new string[]
            {Arguments.Values.CurrentModule.ToString()}).FirstOrDefault();
            return result != null && !result.estOpciones.Contains("I");
        }

    }
}
