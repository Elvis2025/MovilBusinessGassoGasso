using MovilBusiness.Configuration;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovilBusiness.DataAccess
{
    public class DS_Cargas : DS_Controller
    {

        public List<Cargas> GetCargasDisponibles()
        {
            return SqliteManager.GetInstance().Query<Cargas>("select CarSecuencia, CarReferencia, CarFecha, RepCodigo, CarCantidadTotal, AlmID, rowguid from " +
                "Cargas where CarEstatus in (1,7) and ltrim(rtrim(RepCodigo)) = ? order by CarSecuencia", new string[] { Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public List<Cargas> GetCargasDeAplicacionAutomaticaDisponibles()
        {
            return SqliteManager.GetInstance().Query<Cargas>("select CarSecuencia, CarReferencia, CarFecha, RepCodigo, CarCantidadTotal, AlmID, rowguid from " +
                "Cargas where CarEstatus = 7 and ltrim(rtrim(RepCodigo)) = ? order by CarSecuencia", new string[] { Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public List<Cargas> GetCargasNegativasDisponibles()
        {
            return SqliteManager.GetInstance().Query<Cargas>("select CarSecuencia, CarReferencia, CarFecha, RepCodigo, CarCantidadTotal, AlmID, rowguid from " +
                "Cargas where CarEstatus in (1,7) and ltrim(rtrim(RepCodigo)) = ? and CarCantidadTotal < 0 order by CarSecuencia", new string[] { Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public bool HayCargasDisponibles()
        {
            return SqliteManager.GetInstance().Query<Cargas>("select CarSecuencia from Cargas where CarEstatus in (1,7) and trim(RepCodigo) = ? limit 1", 
                new string[] { Arguments.CurrentUser.RepCodigo.Trim() }).Count > 0;
        }

        public List<Cargas> GetCargasAceptadasByCuaSecuencia(int cuaSecuencia)
        {
            return SqliteManager.GetInstance().Query<Cargas>("select CarSecuencia, CarReferencia, CarFecha, rowguid from " +
                "Cargas where CarEstatus = 2 and CuaID = " + cuaSecuencia.ToString() + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' order by CarSecuencia", new string[] { });
        }

        public List<Cargas> GetCargasByEstatus(int cuaSecuencia)
        {
            return SqliteManager.GetInstance().Query<Cargas>("SELECT  CarSecuencia, CarReferencia, CarFecha, e.EstDescripcion as EstadoDescripcion, c.rowguid FROM Cargas c " +
                "INNER JOIN Estados e ON c.CarEstatus = e.EstEstado where esttabla = 'Cargas' AND CuaID = " + cuaSecuencia.ToString() + " AND ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' order by CarSecuencia", new string[] { });
        }

        public List<Cargas> GetCargasAceptadasByCuaSecuenciaByAlmacen(int cuaSecuencia)
        {
            return SqliteManager.GetInstance().Query<Cargas>("select c.CarSecuencia, c.CarReferencia, c.CarFecha, c.almID as almID, a.AlmDescripcion  from " +
                "Cargas c inner join cargasdetalle cd on cd.CarSecuencia = c.CarSecuencia " +
                "inner join Almacenes a on a.almID = c.almID " +
                "where CarEstatus in (2,8) and CuaID = " + cuaSecuencia.ToString() + " and ltrim(rtrim(c.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' GROUP by c.CarSecuencia order by c.CarSecuencia", new string[] { });
        }

        public List<CargasDetalle> GetProductosCargados(int cuaSecuencia , int Almacen = -1)
        {
            return SqliteManager.GetInstance().Query<CargasDetalle>("select cd.ProID as ProID, p.ProCodigo as ProCodigo, p.ProUnidades as ProUnidades, " +
                "p.ProDescripcion as ProDescripcion, sum(cd.CarCantidad) as CarCantidad, sum(cd.CarCantidadDetalle) as CarCantidadDetalle " +(Almacen > -1 ? ", ifnull(cd.CarLote, '')  as carlote " : "")+
                "from Cargas c inner join CargasDetalle cd on c.repcodigo = cd.repcodigo and c.carsecuencia = cd.carsecuencia " +
                "inner join Productos p on p.ProID = cd.ProID " +
                (Almacen > -1 ? "inner join InventariosAlmacenesRepresentantes ia on ia.Proid = cd.Proid and c.almID = ia.almID and ifnull(ia.InvLote, '')  = ifnull(cd.CarLote, '') " : " ")+
                "where CuaID = ? and ltrim(rtrim(c.RepCodigo)) = ? and c.CarEstatus = 2 " +
                (Almacen > -1 ? "and c.almID ="+Almacen+" " : " ") +
                "Group by cd.proid, p.proCodigo, p.ProDescripcion, p.ProUnidades " + (Almacen > -1 ? ", ifnull(cd.CarLote, '') " : "") +" " +
                "order by p.LinID, p.Cat1ID, p.ProID", new string[] { cuaSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public List<CargasDetalle> GetProductosCargadosAgrupadosxProducto(int cuaSecuencia, int Almacen = -1)
        {
            return SqliteManager.GetInstance().Query<CargasDetalle>("select cd.ProID as ProID, p.ProCodigo as ProCodigo, p.ProUnidades as ProUnidades, " +
                "p.ProDescripcion as ProDescripcion, sum(cd.CarCantidad) as CarCantidad, sum(cd.CarCantidadDetalle) as CarCantidadDetalle "  +
                "from Cargas c inner join CargasDetalle cd on c.repcodigo = cd.repcodigo and c.carsecuencia = cd.carsecuencia " +
                "inner join Productos p on p.ProID = cd.ProID " +
                "where CuaID = ? and ltrim(rtrim(c.RepCodigo)) = ? and c.CarEstatus = 2 " +
                (Almacen > -1 ? "and c.almID =" + Almacen + " " : " ") +
                "Group by cd.proid, p.proCodigo, p.ProDescripcion, p.ProUnidades  " +
                "order by p.LinID, p.Cat1ID, p.ProID", new string[] { cuaSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
        }


        public List<CargasDetalle> GetProductosCarga(int carSecuencia)
        {
            var list =  SqliteManager.GetInstance().Query<CargasDetalle>("select "+(myParametro.GetParCargasMostrarCajasUnidades()? "1 as UsarCajasUnidades, " : "")+" p.ProUnidades as ProUnidades, CarSecuencia, ifnull(c.CarLote, '') as CarLote, c.RepCodigo as RepCodigo, c.ProID as ProID, p.ProCodigo as ProCodigo, " +
                "p.ProDescripcion as ProDescripcion, c.CarCantidad as CarCantidad, c.CarCantidadDetalle as CarCantidadDetalle "+ (myParametro.GetParAsignacionLotesByFechaVencimiento() ? ", CarLoteFechaVencimiento" : "" ) + " from CargasDetalle c " +
                "inner join Productos p on p.ProID = c.ProID where CarSecuencia = ? and ltrim(rtrim(c.RepCodigo)) = ? " +
                "order by cast(p.Cat1ID as integer), cast(p.Cat2ID as integer), cast(p.ProID as integer)", 
                new string[] { carSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            return list;
        }

        public int GetTotalProductosCarga(int carSecuencia)
        {
            var list = SqliteManager.GetInstance().Query<CargasDetalle>("select SUM(c.CarCantidad) as CarCantidad from CargasDetalle c " +
                "inner join Productos p on p.ProID = c.ProID where CarSecuencia = ? and ltrim(rtrim(c.RepCodigo)) = ? ",
                new string[] { carSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            if (list != null && list.Count > 0)
            {
                int cantidad = Convert.ToInt32(list[0].CarCantidad);
                return cantidad;
            }
            return 0;
        }

        public Cargas GetCargaBySecuencia(int carSecuencia, bool rawDate = false)
        {
            var list = SqliteManager.GetInstance().Query<Cargas>("select RepCodigo, e.EstDescripcion as EstadoDescripcion, CarSecuencia, c.AlmID, CarCantidadTotal, "+(rawDate ? "CarFecha" : "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(CarFecha,1,10)),' ','' ), '')") + " as CarFecha, CarReferencia, CuaID, a.AlmDescripcion from " +
                "Cargas c inner join Estados e on e.EstTabla = 'Cargas' and e.EstEstado = c.CarEstatus " +
                "inner join Almacenes a on a.AlmId = c.AlmId " +
                "where ltrim(rtrim(c.RepCodigo)) = ? and c.CarSecuencia = ?", new string[] { Arguments.CurrentUser.RepCodigo.Trim(), carSecuencia.ToString() });

            if(list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }
        public Cargas GetCargaBySecuenciaConRefEntrega(int carSecuencia, bool rawDate = false)
        {
            var list = SqliteManager.GetInstance().Query<Cargas>("select RepCodigo, e.EstDescripcion as EstadoDescripcion, CarSecuencia, c.AlmID, CarCantidadTotal, " + (rawDate ? "CarFecha" : "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(CarFecha,1,10)),' ','' ), '')") + " as CarFecha, CarReferencia, CuaID, a.AlmDescripcion, CarReferenciaEntrega from " +
                "Cargas c inner join Estados e on e.EstTabla = 'Cargas' and e.EstEstado = c.CarEstatus " +
                "inner join Almacenes a on a.AlmId = c.AlmId " +
                "where ltrim(rtrim(c.RepCodigo)) = ? and c.CarSecuencia = ?", new string[] { Arguments.CurrentUser.RepCodigo.Trim(), carSecuencia.ToString() });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public void AceptarCarga(string rowguid, List<CargasDetalle> Productos, int almId = -1, string EnrSecuencia = "", string RepSupervisor ="")
        {
            try
            {
                var dsInv = new DS_Inventarios();

                //foreach (var prod in Productos.Where(p => p.CarCantidad < 0 || p.CarCantidadDetalle < 0))
                //{
                //    if (!dsInv.HayExistencia(prod.ProID, Math.Abs(prod.CarCantidad), Math.Abs(prod.CarCantidadDetalle), almId, lote: prod.CarLote))
                //    {
                //        throw new Exception($"no es posible restarle inventario a este producto:  {prod.ProCodigo}");
                //    }
                //}

                foreach (var producto in Productos)
                {
                    dsInv.AgregarInventario(producto.ProID, producto.CarCantidad, producto.CarCantidadDetalle, almId, producto.CarLote, myParametro.GetParAsignacionLotesByFechaVencimiento() ? producto.CarLoteFechaVencimiento : "");
                }

                var c = new Hash("Cargas");
                c.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                c.Add("CarEstatus", 2);
                c.Add("CarFechaAplicacion", Functions.CurrentDate());
                c.Add("CarFechaActualizacion", Functions.CurrentDate());
                c.Add("CuaID", Arguments.Values.CurrentCuaSecuencia);

                if(RepSupervisor != "")
                {
                    c.Add("RepSupervisor", RepSupervisor);
                }

                //c.ExecuteUpdate("CarSecuencia = " + carga.CarSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");
                c.ExecuteUpdate(new string[] { "rowguid" }, new Model.Internal.DbUpdateValue[] { new Model.Internal.DbUpdateValue() { IsText = true, Value = rowguid } }, true);

                if (EnrSecuencia != "")
                {
                    var entrega = new Hash("EntregasRepartidor");
                    entrega.Add("EnrEstatus", 4);
                    entrega.Add("EntFechaActualizacion", Functions.CurrentDate());
                    entrega.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                    entrega.ExecuteUpdate("EnrSecuencia = " + EnrSecuencia);


                    if (myParametro.GetParRutaVisitasFechaFromEntregasRepartidorRutaVisitasFecha())
                    {
                        var query = "select EnrSecuencia, RepCodigo, RutFecha, CliID, RutPosicion, RutEstado from EntregasRepartidorRutaVisitasFecha e " +
                        "where ltrim(rtrim(e.RepCodigo)) = ? and e.EnrSecuencia = ? ";

                        var args = new string[] { Arguments.CurrentUser.RepCodigo.Trim(), EnrSecuencia.ToString() };
                        var list = SqliteManager.GetInstance().Query<EntregasRepartidorRutaVisitasFecha>(query, args);

                        if (list == null || list.Count == 0)
                        {
                            throw new Exception("Error no se pudo cargar la ruta de la Entrega");
                        }

                        var entregaRepartidorTransacciones = new Hash("EntregasRepartidorTransacciones");
                        entregaRepartidorTransacciones.Add("enrEstatusEntrega", 1);
                        entregaRepartidorTransacciones.Add("EntFechaActualizacion", Functions.CurrentDate());
                        entregaRepartidorTransacciones.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                        entregaRepartidorTransacciones.ExecuteUpdate("EnrSecuencia = " + EnrSecuencia + " and enrEstatusEntrega = 8");

                        
                        foreach (var entregaRutaVisita in list)
                        {
                            var queryRuta = "select RepCodigo, RutFecha, CliID, RutPosicion, RutEstado from RutaVisitasFecha r " +
                            "where ltrim(rtrim(r.RepCodigo)) = ? and r.CliID = ? and r.RutFecha = ? ";

                            var argsRuta = new string[] { entregaRutaVisita.RepCodigo.ToString(), entregaRutaVisita.CliID.ToString(), entregaRutaVisita.RutFecha.ToString() };
                            var listRuta = SqliteManager.GetInstance().Query<RutaVisitasFecha>(queryRuta, argsRuta);

                            if(listRuta == null || listRuta.Count == 0)
                            {
                                var rutavisita = new Hash("RutaVisitasFecha");
                                rutavisita.Add("RepCodigo", entregaRutaVisita.RepCodigo);
                                rutavisita.Add("RutFecha", entregaRutaVisita.RutFecha);
                                rutavisita.Add("CliID", entregaRutaVisita.CliID);
                                rutavisita.Add("RutPosicion", entregaRutaVisita.RutPosicion);
                                rutavisita.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                                rutavisita.Add("RutFechaActualizacion", Functions.CurrentDate());
                                rutavisita.Add("rowguid", Guid.NewGuid().ToString());
                                rutavisita.Add("RutEstado", entregaRutaVisita.RutEstado);
                                rutavisita.ExecuteInsert();
                            }
                            
                        }
                        
                    }
                    

                }
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public void RechazarCarga(string rowguid)
        {
            var c = new Hash("Cargas");
            c.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            c.Add("CarEstatus", 3);
            c.Add("CarFechaAplicacion", Functions.CurrentDate());
            c.Add("CarFechaActualizacion", Functions.CurrentDate());
            c.Add("CuaID", Arguments.Values.CurrentCuaSecuencia);
            //c.ExecuteUpdate("CarSecuencia = " + carSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");

            c.ExecuteUpdate(new string[] { "rowguid" }, new Model.Internal.DbUpdateValue[] { new Model.Internal.DbUpdateValue() { IsText = true, Value = rowguid } }, true);
        }

        public void CancelarCarga(string rowguid, string EnrSecuencia = "")
        {
            var c = new Hash("Cargas");
            c.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            c.Add("CarEstatus", 5);
            c.Add("CarFechaAplicacion", Functions.CurrentDate());
            c.Add("CarFechaActualizacion", Functions.CurrentDate());
            c.Add("CuaID", Arguments.Values.CurrentCuaSecuencia);
            //c.ExecuteUpdate("CarSecuencia = " + carSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");

            c.ExecuteUpdate(new string[] { "rowguid" }, new Model.Internal.DbUpdateValue[] { new Model.Internal.DbUpdateValue() { IsText = true, Value = rowguid } }, true);

            if (EnrSecuencia != "")
            {
                var entrega = new Hash("EntregasRepartidor");
                entrega.Add("EnrEstatus", 12);
                entrega.Add("EntFechaActualizacion", Functions.CurrentDate());
                entrega.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                entrega.ExecuteUpdate("EnrSecuencia = " + EnrSecuencia);

            }
        }
        //public bool VerificarCargasSiDejanInventarioNegativo(int CarSecuencia)
        //{
        //    foreach(var Cantidades_Car_Inv in NoAceptarCargasInventarioNegativo(CarSecuencia))

        //    {
        //        if (Cantidades_Car_Inv.CarCantidad < 0 || Cantidades_Car_Inv.CarCantidadDetalle < 0)
        //        {
        //            return true;
        //        }
        //    }

        //    return false;
        //}

        public bool NoAceptarCargasInventarioNegativo(int CargSecuencia)
        {
            var myProd = new DS_Productos();

            var result = SqliteManager.GetInstance().Query<CargasDetalle>("Select i.invCantidad as InvCantidad, c.CarCantidad as CarCantidad, i.invCantidadDetalle as InvCantidadDetalle, c.CarCantidadDetalle as CarCantidadDetalle, i.ProID from Inventarios as i INNER JOIN CargasDetalle as c ON i.ProID = c.ProID "
                        + "WHERE c.CarSecuencia ='" + CargSecuencia + "'  AND c.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' ", new string[] { });

            foreach(var item in result)
            {
                if (item.CarCantidad < 0 || item.CarCantidadDetalle < 0)
                {
                    var TotalCaja = myProd.ConvertirUnidadesACajas(
                    myProd.ConvertirCajasAunidadesByCarga(item.InvCantidad, item.InvCantidadDetalle, myProd.GetProUnidades(item.ProID),
                    item.CarCantidad, item.CarCantidadDetalle), myProd.GetProUnidades(item.ProID)
                    );

                    var DiferenciaUnidades = Math.Round(TotalCaja * myProd.GetProUnidades(item.ProID), 2);

                    if (TotalCaja > 0 || DiferenciaUnidades > 0)
                        return true;
                }       
            }
            return false;
        }

        public int GetTotalProductosCargaInCount(int carSecuencia)
        {
            var list = SqliteManager.GetInstance().Query<CargasDetalle>("select Count(c.CarCantidad) from CargasDetalle c " +
                "inner join Productos p on p.ProID = c.ProID where CarSecuencia = ? and ltrim(rtrim(c.RepCodigo)) = ? ",
                new string[] { carSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            if (list != null && list.Count > 0)
            {
                int cantidad = Convert.ToInt32(list[0].CarCantidad);
                return cantidad;
            }
            return 0;
        }

        public bool HayCargasDisponiblesInCuadreByDia(string fechaCuadre)
        {
            return SqliteManager.GetInstance().Query<Cargas>("select CarSecuencia from Cargas where CarEstatus in (1,7) and trim(RepCodigo) = ? and STRFTIME('%Y-%m-%d', CarFecha) <= STRFTIME('%Y-%m-%d', '" + fechaCuadre + "')  limit 1",
                new string[] { Arguments.CurrentUser.RepCodigo.Trim() }).Count > 0;
        }

        public bool HayCargasDisponiblesInCuadre(string fechaCuadre)
        {
            return SqliteManager.GetInstance().Query<Cargas>("select CarSecuencia from Cargas where CarEstatus in (1,7) and trim(RepCodigo) = ? and STRFTIME('%Y-%m-%d', CarFecha)  between STRFTIME('%Y-%m-%d', '" + fechaCuadre + "') and STRFTIME('%Y-%m-%d', DATETIME('NOW', 'localtime'))) limit 1",
                new string[] { Arguments.CurrentUser.RepCodigo.Trim() }).Count > 0;
        }
    }
}
