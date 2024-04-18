using MovilBusiness.Abstraction;
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
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MovilBusiness.DataAccess
{
    public class DS_Visitas : DS_Controller
    {
        private DS_Mensajes myMen;
        private DS_RutaVisitas myRut;

        public DS_Visitas()
        {
            myMen = new DS_Mensajes();
            myRut = new DS_RutaVisitas();
        }

        public int CrearVisita(int cliId, Location location, int visSecuenciaOrigenFromVirtual = -1)
        {
            int delay = myParametro.GetParVisitaGPSDelay();

            if (delay > 0 && myParametro.GetParGPS())
            {
                Functions.StartListeningForLocations();
                Task.Delay(new TimeSpan(0, 0, delay)).Wait();
                Functions.StartListeningForLocations();
            }

            int visSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Visitas");
            Hash visita = new Hash("Visitas");
            visita.Add("VisSecuencia", visSecuencia);
            visita.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            visita.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            visita.Add("CliID", cliId);
            visita.Add("VisFechaEntrada", Functions.CurrentDate());
            visita.Add("rowguid", Guid.NewGuid().ToString());
            visita.Add("VisEstatus", 1);

            try
            {
                var battery = DependencyService.Get<IAppInfo>();

                visita.Add("VisPorcientoBateria", battery.BatteryLevel());

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            if (visSecuenciaOrigenFromVirtual != -1)
            {
                visita.Add("VisTipoVisita", "2");
            }

            visita.Add("VisFechaActualizacion", Functions.CurrentDate());

            visita.Add("VisIndicadorFueraRuta", IsFueraDeRuta(cliId) ? 1 : 0);
            if (IsFueraDeRuta(cliId))
            {
                visita.Add("VisIndicadorFueraOrden", 1);
            }
            else
            {
                visita.Add("VisIndicadorFueraOrden", IsFueraOrden(cliId) ? 1 : 0);
            }

            if (location != null)
            {
                visita.Add("VisLatitud", location.Latitude);
                visita.Add("VisLongitud", location.Longitude);
            }
            visita.Add("mbVersion", Functions.AppVersion);

            visita.ExecuteInsert();

            DS_RepresentantesSecuencias.UpdateSecuencia("Visitas", visSecuencia);

            if (visSecuenciaOrigenFromVirtual != -1)
            {
                SqliteManager.GetInstance().Execute("update Visitas Set VisSecuenciaOrigen = ? where VisSecuencia = ? and ltrim(rtrim(RepCodigo)) = ? ",
                    new string[] { visSecuenciaOrigenFromVirtual.ToString(), visSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });
            }
            var myciclo = new DS_CiclosAuditorRutasClientes();
            if (myParametro.GetParCiclosAudRutasCli() && !myciclo.GetResultOfCiclos(cliId))
            {
                myciclo.UpdateCiclosAuditor(cliId);
            }

            return visSecuencia;
        }

        private bool IsFueraOrden(int cliId)
        {
            int CicloSemana = 4;

            int parCicloSemana = myParametro.GetParCiclosSemanas();
            if (parCicloSemana > 0)
            {
                CicloSemana = parCicloSemana;
            }
            int dayNumber = (int)(DateTime.Now).DayOfWeek - 1;
            var weekNumber = Functions.GetWeekOfMonth(DateTime.Now);
            weekNumber = weekNumber % CicloSemana;
            if (myParametro.GetParSemanasAnios())
            {
                weekNumber = myRut.GetNumeroSemana(DateTime.Now);

            }
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
            var TipoVisita = DS_RepresentantesParametros.GetInstance().GetParRutaVisitaTipo();

            if (TipoVisita == -1)
            {
                var list = SqliteManager.GetInstance().Query<Visitas>("select CliID from RutaVisitas " +
                "where CliID not in (select CliID from Visitas where RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and VisFechaEntrada like '" + DateTime.Now.ToString("yyyy-MM-dd") + "%') " +
                "and RutSemana" +
                weekNumber.ToString() + " like '" + semanaValues + "' AND RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' " +
                "order by cast(RutPosicion as integer) asc limit 1", new string[] { });

                if (list != null && list.Count > 0)
                {
                    return list[0].CliID != cliId;
                }
            }
            else
            {
                var list = SqliteManager.GetInstance().Query<Visitas>("select CliID from RutaVisitasFecha " +
                "where ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' " + "and RutFecha like '" + DateTime.Now.ToString("yyyy-MM-dd") + "%' " +
                "and CliID not in (select CliID from Visitas where RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and VisFechaEntrada like '" + DateTime.Now.ToString("yyyy-MM-dd") + "%') " +
                "order by cast(RutPosicion as integer) asc limit 1", new string[] { });

                if (list != null && list.Count > 0)
                {
                    return list[0].CliID != cliId;
                }
            }

            return true;
        }

        private bool IsFueraDeRuta(int cliid)
        {
            int CicloSemana = 4;

            int parCicloSemana = myParametro.GetParCiclosSemanas();
            if (parCicloSemana > 0)
            {
                CicloSemana = parCicloSemana;
            }
            int dayNumber = (int)(DateTime.Now).DayOfWeek - 1;
            var weekNumber = Functions.GetWeekOfMonth(DateTime.Now);
            weekNumber = weekNumber % CicloSemana;
            if (myParametro.GetParSemanasAnios())
            {
                weekNumber = myRut.GetNumeroSemana(DateTime.Now);

            }
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
            var TipoVisita = DS_RepresentantesParametros.GetInstance().GetParRutaVisitaTipo();

            if (TipoVisita == -1)
            {
                var Isinruta = SqliteManager.GetInstance().Query<Visitas>("select CliID from RutaVisitas where CliID = " + cliid + " AND RutSemana" +
                   weekNumber.ToString() + " like '" + semanaValues + "' AND RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "'", new string[] { });

                return Isinruta.Count == 0;
            }
            else
            {
                var Isinruta = SqliteManager.GetInstance().Query<Visitas>("SELECT RutaVisitasFecha.RepCodigo "
                        + "FROM RutaVisitasFecha as RutaVisitasFecha "
                        + "INNER JOIN Clientes ON Clientes.CliID = RutaVisitasFecha.CliID "
                        + "WHERE RutaVisitasFecha.CliID = " + cliid.ToString() + " "
                        + "AND RutaVisitasFecha.RutFecha like '" + DateTime.Now.ToString("yyyy-MM-dd") + "%' ", new string[] { });

                return Isinruta.Count == 0;
            }
        }

        public void CrearVisitaFallida(int cliId, string motivo, Location location, bool isVirtual = false)
        {
            int delay = myParametro.GetParVisitaGPSDelay();

            if (delay > 0 && myParametro.GetParGPS())
            {
                Functions.StartListeningForLocations();
                Task.Delay(new TimeSpan(0, 0, delay)).Wait();
                Functions.StartListeningForLocations();
            }

            int visSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Visitas");

            Hash visita = new Hash("Visitas");
            visita.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            visita.Add("CliID", cliId);
            visita.Add("VisSecuencia", visSecuencia);
            visita.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            visita.Add("rowguid", Guid.NewGuid().ToString());
            visita.Add("VisEstatus", 0);
            visita.Add("VisFechaEntrada", Functions.CurrentDate());
            visita.Add("VisFechaSalida", Functions.CurrentDate());
            visita.Add("VisTipoVisita", isVirtual ? 2 : 1);
            visita.Add("VisFechaActualizacion", Functions.CurrentDate());
            visita.Add("mbVersion", Functions.AppVersion);


            if (location != null)
            {
                visita.Add("VisLatitud", location.Latitude);
                visita.Add("VisLongitud", location.Longitude);
                visita.Add("VisLatitudSalida", location.Latitude);
                visita.Add("VisLongitudSalida", location.Longitude);
            }

            var parSectores = myParametro.GetParSectores();

            if(parSectores == 3 || parSectores == 4)
            {
                var sectores = new DS_Sectores().GetSectoresByCliente(Arguments.Values.CurrentClient.CliID);

                foreach(var sector in sectores)
                {
                    CrearVisitaSector(cliId, sector.SecCodigo, visSecuencia, true);
                }
            }

            visita.ExecuteInsert();

            myMen.CrearMensaje(cliId, motivo, visSecuencia, visSecuencia, 13, 0);

            DS_RepresentantesSecuencias.UpdateSecuencia("Visitas", visSecuencia);
        }

        public void CerrarVisita(int Cliid, int visSecuencia, Location location, string tipoVisita, bool isVisitaVirtualHija = false)
        {
            Hash map = new Hash("Visitas");
            map.Add("VisFechaSalida", Functions.CurrentDate());
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map.Add("VisCantidadImagenes", new DS_TransaccionesImagenes().GetCountImgByVis());

            if (!isVisitaVirtualHija)
            {
                map.Add("VisTipoVisita", tipoVisita);
            }

            if (location != null)
            {
                map.Add("VisLatitudSalida", location.Latitude);
                map.Add("VisLongitudSalida", location.Longitude);
            }

            var visita = GetVisitaBySecuencia(visSecuencia);

            // map.ExecuteUpdate("rowguid = '"+visita.rowguid.Trim()+"'", true);

            var values = new DbUpdateValue[] { new DbUpdateValue() { Value = visita.rowguid.Trim(), IsText = true } };
            map.ExecuteUpdate(new string[] { "rowguid" }, values, true);

            if (myParametro.GetParSectores() > 2 && Arguments.Values.CurrentSector != null && !isVisitaVirtualHija)
            {
                CerrarVisitaSector(Cliid, visSecuencia, Arguments.Values.CurrentSector.SecCodigo);
            }
        }

        private int MaxVisitaSectorPosicion(int visSecuencia)
        {
            var list = SqliteManager.GetInstance().Query<Visitas>("select max(VisPosicion) as VisSecuencia " +
                "from VisitasSectores where VisSecuencia = ? and ltrim(rtrim(RepCodigo)) = ? ", new string[] { visSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            if (list != null && list.Count > 0)
            {
                return list[0].VisSecuencia;
            }

            return 0;
        }

        public void CrearVisitaSector(int cliId, string secCodigo, int visSecuencia, bool fallida = false)
        {
            
            var map = new Hash("VisitasSectores");
            map.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            map.Add("Vissecuencia", visSecuencia);
            map.Add("VisPosicion", MaxVisitaSectorPosicion(visSecuencia) + 1);
            map.Add("SecCodigo", secCodigo);
            map.Add("VisFechaentrada", Functions.CurrentDate());
            map.Add("VisEstatus ", fallida ? "0" : "1");

            if (myParametro.GetParGPS() && Arguments.Values.CurrentLocation != null)
            {
                map.Add("VisLatitud", Arguments.Values.CurrentLocation.Latitude);
                map.Add("VisLongitud", Arguments.Values.CurrentLocation.Longitude);
            }
            else
            {
                map.Add("VisLatitud", 0);
                map.Add("VisLongitud", 0);
            }

            map.Add("VisFechaActualizacion", Functions.CurrentDate());
            map.Add("rowguid", Guid.NewGuid().ToString());
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);

            map.ExecuteInsert();
        }

        private void CerrarVisitaSector(int cliId, int visSecuencia, string secCodigo)
        {
            var map = new Hash("VisitasSectores");
            map.Add("VisFechaActualizacion", Functions.CurrentDate());
            map.Add("VisfechaSalida", Functions.CurrentDate());

            var list = SqliteManager.GetInstance().Query<Visitas>("select rowguid from VisitasSectores " +
                "where VisSecuencia = ? and trim(SecCodigo) = ? and trim(RepCodigo) = ? and VisfechaSalida is null", 
                new string[] { visSecuencia.ToString(), secCodigo, Arguments.CurrentUser.RepCodigo.Trim() });

            if(list != null && list.Count > 0)
            {
                // map.ExecuteUpdate("rowguid = '" + list[0].rowguid.Trim() + "'", true);
                map.ExecuteUpdate(new string[] { "rowguid" }, new DbUpdateValue[] { new DbUpdateValue() { Value = list[0].rowguid.Trim(), IsText = true } }, true);
            }

           // map.ExecuteUpdate("RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and VisSecuencia = " + visSecuencia + " and SecCodigo = '" + secCodigo + "' and VisfechaSalida IS NULL");
        }

        public void ActualizarVisitaEfectiva(int visSecuencia)
        {
            Hash map = new Hash("Visitas");
            map.Add("VisEstatus", 2);

            var visita = GetVisitaBySecuencia(visSecuencia);

            map.ExecuteUpdate(new string[] { "rowguid" }, new DbUpdateValue[] { new DbUpdateValue() { Value = visita.rowguid.Trim(), IsText = true } }, true);

            var parSectores = myParametro.GetParSectores();

            if((parSectores == 3 || parSectores == 4) && Arguments.Values.CurrentSector != null)
            {
                var list = SqliteManager.GetInstance().Query<Visitas>("select rowguid from VisitasSectores " +
                "where VisSecuencia = ? and trim(SecCodigo) = ? and trim(RepCodigo) = ? and VisfechaSalida is null",
                new string[] { visSecuencia.ToString(), Arguments.Values.CurrentSector.SecCodigo, Arguments.CurrentUser.RepCodigo.Trim() });

                if (list != null && list.Count > 0)
                {
                    var sec = new Hash("VisitasSectores");
                    sec.Add("VisEstatus", 2);
                    sec.ExecuteUpdate(new string[] { "rowguid" }, new DbUpdateValue[] { new DbUpdateValue() { Value = list[0].rowguid.Trim(), IsText = true } }, true);
                }
            }
        }

        public bool IsVisitaEfectiva(int visSecuencia, int cliId)
        {
            try
            {
                return SqliteManager.GetInstance().Query<Visitas>("select VisSecuencia from Visitas where VisEstatus = 2 and VisSecuencia = ? and ltrim(rtrim(RepCodigo)) = ? and CliID = ? limit 1", new string[] { visSecuencia.ToString(), Arguments.CurrentUser.RepCodigo, cliId.ToString() }).Count > 0;
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return false;
        }   

        public List<Visitas> GetVisitasByClienteAndRepresentante(int CliId)
        {
            return SqliteManager.GetInstance().Query<Visitas>("select v.VisSecuencia as VisSecuencia, VisFechaEntrada, " +
                "VisFechaSalida, MenDescripcion AS Comentario, ifnull(VisEstatus, 1) as VisEstatus from Visitas v " +
                "left join Mensajes m on m.VisSecuencia = v.VisSecuencia and m.TraID = 13 " +
                "where v.RepCodigo = ? and v.CliID = ? order by v.VisSecuencia Desc", new string[] { Arguments.CurrentUser.RepCodigo, CliId.ToString() });
        }

        public List<Visitas> GetVisitasByCliente(int CliId)
        {
            return SqliteManager.GetInstance().Query<Visitas>("select v.VisSecuencia as VisSecuencia, VisFechaEntrada, " +
                "VisFechaSalida, MenDescripcion AS Comentario, ifnull(VisEstatus, 1) as VisEstatus from Visitas v " +
                "left join Mensajes m on m.VisSecuencia = v.VisSecuencia and m.TraID = 13 " +
                "where  v.CliID = ? order by v.VisSecuencia Desc", new string[] {  CliId.ToString() });
        }

        public List<ResumenVisitas> GetResumenVisita(int visSecuencia, int CliID)
        {
            string sql = "select * from (";

            int tipoAdValorem = myParametro.GetParTipoAdValorem();

            string porcionAdvalorem = "0";

            if (tipoAdValorem == 1)
            {
                porcionAdvalorem = "( CAST(PD.VenPrecio * CAST((PD.VenAdvalorem / 100.0) AS REAL) AS REAL) )";
            }
            else if (tipoAdValorem == 2)
            {
                porcionAdvalorem = "PD.VenAdvalorem";
            }

            bool isWithUnion = false;

            if (myParametro.GetParPedidos())
            {
                isWithUnion = true;
                sql += "select 'Pedido' as TipoTransaccion, ifnull(sum(Cantidad), 0) as Cantidad, ifnull(sum(MontoTotal), 0) as MontoTotal from ( " +
                 "Select (Select count(PedSecuencia) from Pedidos  where VisSecuencia = " + visSecuencia + " AND CliID = " + CliID + ") as Cantidad , ifnull(sum( ( (PD.PedPrecio - PD.PedDescuento + PD.PedSelectivo + " + porcionAdvalorem + " ) *  ( CAST(1 + CAST((PD.PedItbis/100.0) AS REAL) AS REAL)   ) ) * (CAST(ifnull(PD.PedCantidad, 0) as REAL) + (CAST(ifnull(PD.PedCantidadDetalle, 0) as REAL) / CAST(ifnull(PRO.ProUnidades, 1) as REAL))  )    ), 0 ) as MontoTotal " +
                 "from Pedidos P " +
                 "INNER JOIN  PedidosDetalle PD on P.PedSecuencia = PD.PedSecuencia " +
                 "INNER JOIN Productos PRO on PD.ProID = PRO.ProID " +
                 "where P.CliID = " + CliID + " AND P.VisSecuencia = " + visSecuencia + " ";

                if (SqliteManager.ExistsTable("PedidosConfirmados"))
                {
                    sql += "UNION Select(Select count(PedSecuencia) from PedidosConfirmados  where VisSecuencia = " + visSecuencia + " AND CliID = " + CliID + ") as Cantidad , ifnull(sum(((PD.PedPrecio - PD.PedDescuento + PD.PedSelectivo + " + porcionAdvalorem + ") * (CAST(1 + CAST(PD.PedItbis / 100.0 AS REAL) AS REAL))) * (CAST(ifnull(PD.PedCantidad, 0) as REAL) + (CAST(ifnull(PD.PedCantidadDetalle, 0) as REAL) / CAST(ifnull(PRO.ProUnidades, 1) as REAL)))), 0) as MontoTotal " +
                    "from PedidosConfirmados P " +
                    "INNER JOIN  PedidosDetalleConfirmados PD on P.PedSecuencia = PD.PedSecuencia " +
                    "INNER JOIN Productos PRO on PD.ProID = PRO.ProID " +
                    "where P.CliID = " + CliID + " AND P.VisSecuencia = " + visSecuencia;
                }

                sql += ") as P ";
            }

            if (myParametro.GetParCotizaciones())
            {
                sql += "  "+(isWithUnion ? " union " : " ") +" select 'Cotizacion' as TipoTransaccion,ifnull(sum(Cantidad), 0) as Cantidad, ifnull(sum(MontoTotal), 0) as MontoTotal from ( " +
                "Select  (Select count(CotSecuencia) from Cotizaciones  where VisSecuencia = " + visSecuencia + " AND CliID = " + CliID + ") as Cantidad , ifnull(sum( ( (PD.CotPrecio - PD.CotDescuento + PD.CotSelectivo + " + porcionAdvalorem + " ) *  ( CAST(1 + CAST(PD.CotItbis/100.0 AS REAL) AS REAL)   ) ) * (CAST(ifnull(PD.CotCantidad, 0) as REAL) + (CAST(ifnull(PD.CotCantidadDetalle, 0) as REAL) / CAST(ifnull(PRO.ProUnidades, 1) as REAL))  )    ), 0 ) as MontoTotal " +
                "from Cotizaciones P " +
                "INNER JOIN  CotizacionesDetalle PD on P.CotSecuencia = PD.CotSecuencia " +
                "INNER JOIN Productos PRO on PD.ProID = PRO.ProID " +
                "where P.CliID = " + CliID + " AND P.VisSecuencia = " + visSecuencia + " ";

                if (SqliteManager.ExistsTable("CotizacionesConfirmados"))
                {
                    sql += "UNION " +
                    "Select (Select count(CotSecuencia) from CotizacionesConfirmados  where VisSecuencia = " + visSecuencia + " AND CliID = " + CliID + ") as Cantidad , ifnull(sum( ( (PD.CotPrecio - PD.CotDescuento + PD.CotSelectivo + " + porcionAdvalorem + " ) *  ( CAST(1 + CAST(PD.CotItbis/100.0 AS REAL) AS REAL)   ) ) * (CAST(ifnull(PD.CotCantidad, 0) as REAL) + (CAST(ifnull(PD.CotCantidadDetalle, 0) as REAL) / CAST(ifnull(PRO.ProUnidades, 1) as REAL))  )    ), 0 ) as MontoTotal " +
                    "from CotizacionesConfirmados P " +
                    "INNER JOIN  CotizacionesDetalleConfirmados PD on P.CotSecuencia = PD.CotSecuencia " +
                    "INNER JOIN Productos PRO on PD.ProID = PRO.ProID " +
                    "where P.CliID = " + CliID + " AND P.VisSecuencia = " + visSecuencia + " ";
                }
                sql += ") as C ";
            }

            if (myParametro.GetParVentas())
            {
                sql += "" + (isWithUnion ? " union " : " ") + " select 'Venta' as TipoTransaccion,ifnull(sum(Cantidad), 0) as Cantidad, ifnull(sum(MontoTotal), 0) as MontoTotal from ( " +
                "Select  (Select count(VenSecuencia) from Ventas  where VisSecuencia = " + visSecuencia + " AND CliID = " + CliID + ") as Cantidad ,ifnull(sum( ( (PD.VenPrecio - PD.VenDescuento + PD.VenSelectivo + " + porcionAdvalorem + " ) *  ( CAST(1 + CAST(PD.VenItbis/100.0 AS REAL) AS REAL)   ) ) * (CAST(ifnull(PD.VenCantidad, 0) as REAL) + (CAST(ifnull(PD.VenCantidadDetalle, 0) as REAL) / CAST(ifnull(PRO.ProUnidades, 1) as REAL))  )    ), 0 ) as MontoTotal " +
                "from Ventas P " +
                "INNER JOIN  VentasDetalle PD on P.VenSecuencia = PD.VenSecuencia " +
                "INNER JOIN Productos PRO on PD.ProID = PRO.ProID " +
                "where P.CliID = " + CliID + " AND P.VisSecuencia = " + visSecuencia + " ";

                if (SqliteManager.ExistsTable("VentasConfirmados"))
                {
                    sql += "UNION " +
                    "Select (Select count(VenSecuencia) from VentasConfirmados  where VisSecuencia = " + visSecuencia + " AND CliID = " + CliID + ") as Cantidad , ifnull(sum( ( (PD.VenPrecio - PD.VenDescuento + PD.VenSelectivo + " + porcionAdvalorem + " ) *  ( CAST(1 + CAST(PD.VenItbis/100.0 AS REAL) AS REAL)   ) ) * (CAST(ifnull(PD.VenCantidad, 0) as REAL) + (CAST(ifnull(PD.VenCantidadDetalle, 0) as REAL) / CAST(ifnull(PRO.ProUnidades, 1) as REAL))  )    ), 0 ) as MontoTotal " +
                    "from VentasConfirmados P " +
                    "INNER JOIN  VentasDetalleConfirmados PD on P.VenSecuencia = PD.VenSecuencia " +
                    "INNER JOIN Productos PRO on PD.ProID = PRO.ProID " +
                    "where P.CliID = " + CliID + " AND P.VisSecuencia = " + visSecuencia + " ";
                }

                sql += ") as V";
            }

            sql += ") as tabla";

            return SqliteManager.GetInstance().Query<ResumenVisitas>(sql, new string[] { });
        }

        public Visitas GetVisitaAbierta()
        {
            var list = SqliteManager.GetInstance().Query<Visitas>("select RepCodigo, VisSecuencia, CliID, ifnull(VisTipoVisita, 0) as VisTipoVisita from Visitas " +
                "where VisFechaSalida IS NULL and ltrim(rtrim(RepCodigo)) = '"+Arguments.CurrentUser.RepCodigo.Trim()+"' " +
                //por si tienes muchas visitas abiertas de versiones posteriores para que no tenga que cerrarlas una a una
                "and cast(replace(ifnull(mbVersion, ''), '.', '') as int) >= cast(replace('7.1.0', '.', '') as int) and ifnull(mbVersion, '') not like '6.%' limit 1",
                new string[] {  });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public Visitas GetVisitaBySecuencia(int visSecuencia)
        {
            var list = SqliteManager.GetInstance().Query<Visitas>("select rowguid, RepCodigo, VisSecuencia, CliID, ifnull(VisTipoVisita, 0) as VisTipoVisita from Visitas " +
                "where VisSecuencia = ? and ltrim(rtrim(RepCodigo)) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' limit 1",
                new string[] { visSecuencia.ToString() });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public bool IsVisitaVirtual(int visSecuencia)
        {
            try
            {
                return SqliteManager.GetInstance().Query<Visitas>("select VisSecuenciaOrigen from Visitas " +
                    "where VisSecuencia = ? and ltrim(rtrim(RepCodigo)) = ? and ifnull(VisSecuenciaOrigen, 0) > 0 " +
                    "and VisTipoVisita = 2",
                    new string[] { visSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() }).Count > 0;

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return false;
        }

        public int GetVisSecuenciaOrigen(int visSecuencia)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<Visitas>("select VisSecuenciaOrigen from Visitas " +
                    "where VisSecuencia = ? and VisTipoVisita = 2 and ltrim(rtrim(RepCodigo)) = ? ",
                    new string[] { visSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

                if (list != null && list.Count > 0)
                {
                    return list[0].VisSecuenciaOrigen;
                }

                return -1;
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return -1;
            }
        }


        public List<Visitas> GetVisitados(string CliIds)
        {
            return SqliteManager.GetInstance().Query<Visitas>("select CliID from Visitas where CliID in (" + CliIds + ") and VisFechaEntrada like '%" + Functions.CurrentDate("yyyy-MM-dd") + "%'and VisTipoVisita <> '3' group by CliID", new string[] { });
        }

        public void CrearVisitasFallidas(List<Clientes> clientes, string menIdMotivo, Location location)
        {
            var motivo = SqliteManager.GetInstance().Query<TiposMensaje>("select MenDescripcion from TiposMensaje where MenID = ? and TraID = 0", new string[] { menIdMotivo }).FirstOrDefault();

            if (motivo == null)
            {
                throw new Exception("No se cargo el motivo de visita fallida");
            }

            try
            {
                SqliteManager.GetInstance().BeginTransaction();

                foreach (var cliente in clientes)
                {
                    CrearVisitaFallida(cliente.CliID, motivo.MenDescripcion, location, true);
                }

                SqliteManager.GetInstance().Commit();
            }
            catch (Exception e)
            {
                SqliteManager.GetInstance().Rollback();
                throw e;
            }
        }

        public void GuardarVisitaPresentacion(VisitasPresentacion visita, bool Editar = false)
        {
            var vis = new Hash("VisitasPresentacion");
            vis.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            vis.Add("VisSecuencia", visita.VisSecuencia);
            vis.Add("VisNombre", visita.VisNombre);
            vis.Add("VisPropietario", visita.VisPropietario);
            vis.Add("VisContacto", visita.VisContacto);
            vis.Add("VisEmail", visita.VisEmail);
            vis.Add("VisCalle", visita.VisCalle);
            vis.Add("VisCiudad", visita.VisCiudad);
            vis.Add("VisTelefono", visita.VisTelefono);
            vis.Add("VisRNC", visita.VisRNC);
            vis.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            vis.Add("VisFechaActualizacion", Functions.CurrentDate());

            if (Editar)
            {
                vis.ExecuteUpdate("RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' and VisSecuencia = " + visita.VisSecuencia);
            }
            else
            {
                vis.Add("rowguid", Guid.NewGuid().ToString());
                vis.ExecuteInsert();
            }

        }

        public VisitasPresentacion GetClientePresentacion(int visSecuencia)
        {
            try
            {
                return SqliteManager.GetInstance().Query<VisitasPresentacion>("select * from VisitasPresentacion where VisSecuencia = ? and trim(RepCodigo) = ?",
                    new string[] { visSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() }).FirstOrDefault();
            }catch(Exception)
            {
                return null;
            }
        }

        public bool VerificarDeTransacciones(int vissecuencia, int Cliid)
        {
            string query = "";
            string sql = "";

            if (myParametro.GetParVentas())
            {
                query = " union all select venSecuencia from Ventas where visSecuencia = '" + vissecuencia + "' and CliID = '" + Cliid + "' ";
            }
            else if (myParametro.GetParDevoluciones())
            {
                query += " union all select devSecuencia from Devoluciones where visSecuencia = '" + vissecuencia + "' and CliID = '" + Cliid + "' ";
            }
            else if (myParametro.GetParCotizaciones())
            {
                query += " union all  select cotSecuencia from Cotizaciones where visSecuencia = '" + vissecuencia + "' and CliID = '" + Cliid + "' ";
            }

            sql = " select result.* from(" +
                    " select pedSecuencia from Pedidos where visSecuencia = '" + vissecuencia + "' and CliID = '" + Cliid + "' " +
                    query +
                    " union all " +
                    " select MenID from Mensajes where visSecuencia = '" + vissecuencia + "' and CliID = '" + Cliid + "' " +
                    " ) as result";
            var list = SqliteManager.GetInstance().Query<VisitasPresentacion>(sql, new string[] { });

            if (list.Count > 0)
            {
                return true;
            }

            return false;
        }

        public void GuardarVisitasResultados(VisitasResultados data, bool createNewOne = false)
        {
            var map = new Hash("VisitasResultados");

            /*var titId = Functions.GetTitIdByModule(module);

            var montoTotalTemp = 0.0;
            var montoSinItbisTemp = 0.0;
            var qtyTran = 0;
            var virSecuencia = -1;

            var inTemp = SqliteManager.GetInstance().Query<VisitasResultados>("select virSecuencia, ifnull(count(*), 0) as VisCantidadTransacciones, sum(VisMontoTotal) as VisMontoTotal, sum(VisMontoSinItbis) as VisMontoSinItbis " +
                "from VisitasResultados where VisSecuencia = ? and RepCodigo = '"+Arguments.CurrentUser.RepCodigo.Trim()+"' and TitID = ? group by virSecuencia", 
                new string[] { visSecuencia.ToString(), titId.ToString() });

            if(inTemp != null && inTemp.Count > 0)
            {
                montoTotalTemp = inTemp[0].VisMontoTotal;
                montoSinItbisTemp = inTemp[0].VisMontoSinItbis;
                qtyTran = inTemp[0].VisCantidadTransacciones;
                virSecuencia = inTemp[0].virSecuencia;
            }      */

            
            map.Add("VisComentario", data.VisComentario);
            map.Add("VisMontoTotal", data.VisMontoTotal);
            map.Add("VisMontoSinItbis", data.VisMontoSinItbis);
            map.Add("VisCantidadTransacciones", data.VisCantidadTransacciones);           
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map.Add("VisFechaActualizacion", Functions.CurrentDate());

            if (data.VisCantidadTransacciones > 1 && !createNewOne)
            {
                var where = "";

                if(data.TitID == 15 && string.IsNullOrWhiteSpace(data.VisComentario)) //si es la cabecera del SAC
                {
                    where = " and ifnull(VisComentario, '') = '' ";
                }

                map.ExecuteUpdate("RepCodigo = '"+Arguments.CurrentUser.RepCodigo.Trim()+"' and TitID = " + data.TitID.ToString() + " and VisSecuencia = " + Arguments.Values.CurrentVisSecuencia.ToString() + " " +
                    "and virSecuencia = " + GetLastVirSecuencia(Arguments.Values.CurrentVisSecuencia, data.TitID).ToString() + where);
            }
            else
            {
                map.Add("rowguid", Guid.NewGuid().ToString());
                map.Add("TitID", data.TitID.ToString());
                map.Add("RepCodigo", Arguments.CurrentUser.RepCodigo.Trim());
                map.Add("VisSecuencia", Arguments.Values.CurrentVisSecuencia);
                map.Add("virSecuencia", GetLastVirSecuencia(Arguments.Values.CurrentVisSecuencia) + 1);
                map.ExecuteInsert();
            }

        }

        private int GetLastVirSecuencia(int visSecuencia, int titId = -1)
        {
            var virSecuencia = 0;

            var where = "";

            if(titId != -1)
            {
                where = " and TitID = " + titId.ToString() + " ";
            }

            var list = SqliteManager.GetInstance().Query<VisitasResultados>("select max(virSecuencia) as virSecuencia " +
                "from VisitasResultados where RepCodigo = '"+Arguments.CurrentUser.RepCodigo+"' and VisSecuencia = ? " + where, 
                new string[] { visSecuencia.ToString() });

            if(list != null && list.Count > 0)
            {
                virSecuencia = list[0].virSecuencia;
            }

            return virSecuencia;
        }

        public void ActualizarVisitasResultadosComentario(string comentario, int visSecuencia)
        {
            var map = new Hash("VisitasResultados");
            map.Add("VisComentario", comentario);
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map.Add("VisFechaActualizacion", Functions.CurrentDate());

            var virSecuencia = GetLastVirSecuencia(visSecuencia);

            map.ExecuteUpdate("RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' and VisSecuencia = " + visSecuencia.ToString() + " and virSecuencia = " + virSecuencia.ToString());
        }

        public bool HasVisitaPresencial(int CliID)
        {
            return SqliteManager.GetInstance().Query<Visitas>("select 1 from Visitas where CliID =" + CliID + " and VisFechaEntrada like '%" + Functions.CurrentDate("yyyy-MM-dd") + "%'and VisTipoVisita = '1'", new string[] { }).Count > 0;
        }
    }
}
