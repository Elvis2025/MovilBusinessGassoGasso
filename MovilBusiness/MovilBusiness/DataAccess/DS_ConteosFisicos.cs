using MovilBusiness.Configuration;
using MovilBusiness.Enums;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MovilBusiness.DataAccess
{
    public class DS_ConteosFisicos : DS_Controller
    {
        private DS_Productos myProd;
        private DS_Inventarios inv;
        public DS_Recibos myRec;
        private DS_Ventas myVen;
        private DS_CondicionesPago myCondPago;
        private DS_UsosMultiples myUsosMul;

        public DS_ConteosFisicos()
        {
            myProd = new DS_Productos();
            inv = new DS_Inventarios();
            myRec = new DS_Recibos();
            myVen = new DS_Ventas();
            myCondPago = new DS_CondicionesPago();
            myUsosMul = new DS_UsosMultiples();
        }

        public int GuardarConteo(int cuaSecuencia, string repAuditor = null, List<ProductosTemp> ProductosFaltantesAjustados = null, int almId = -1, List<ProductosTemp> ProductosFaltantes = null)
        {
            var total = 0.0;
            var conSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("ConteosFisicos");

            var con = new Hash("ConteosFisicos");
            con.Add("ConEstatus", 1);
            con.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            con.Add("ConSecuencia", conSecuencia);
            con.Add("ConFecha", Functions.CurrentDate());
            con.Add("CuaSecuencia", cuaSecuencia);            
            con.Add("ConEstatusConteo", (myParametro.GetParConteosFisicosLotesAgrupados() ? GetProductosInInventarioConFaltantesyLotesAgrupados(myParametro.GetParClienteForRepresentantes(), almId, IsForValid: true).Count : GetProductosInInventarioConFaltantes(myParametro.GetParClienteForRepresentantes(), almId, IsForValid: true).Count) <= 0 ? 1 : 2); //1 cuadrado, 2 descuadrado
            con.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            con.Add("ConFechaActualizacion", Functions.CurrentDate());
            con.Add("RepAuditor", repAuditor);
            con.Add("rowguid", Guid.NewGuid().ToString());
            con.Add("mbVersion", Functions.AppVersion);

            if (myParametro.GetParConteoFisicoPorAlmacen())
            {
                con.Add("AlmID", almId);
            }


            var almIdRanchero = myParametro.GetParAlmacenVentaRanchera();
            var parMultiAlmacenes = myParametro.GetParUsarMultiAlmacenes();
            var almIdDevolucion = myParametro.GetParAlmacenIdParaDevolucion();
            var almIdMelma = myParametro.GetParAlmacenIdParaMelma();
            var almIdToUse = parMultiAlmacenes ? almIdDevolucion : -1;

            if (almIdRanchero > 0)
            {
                almIdToUse = almIdRanchero;
            }

            if (almId != -1)
            {
                almIdToUse = almId;
            }
            var almid = myParametro.GetParAlmacenIdParaMelma() == almIdToUse ? almIdToUse : myParametro.GetParAlmacenVentaRanchera();

            if (myParametro.GetParConteoConAlmacenDespachoyDevolucion())
            {
                almid = myParametro.GetParAlmacenIdParaDevolucion() == almIdToUse ? almIdToUse : myParametro.GetParAlmacenIdParaDespacho();
            }

            if (myParametro.GetParConteoConVariosAlmacenes())
            {
                almid = almIdToUse;
            }


            var productos = myParametro.GetParConteosFisicosLotesAgrupados() ? GetProductosInTempWithNoLote(almid) : GetProductosInTemp(almid);
            var productoswitcant = productos.Where(p => p.InvCantidad > 0 || (p.Cantidad > 0 || p.CantidadDetalle > 0)).ToList();
            var ConCantidadLineas = 0;

            if (myParametro.GetParConteosFisicosLotesAgrupados())
            {
                foreach (var pro in productoswitcant)
                {
                    var inventario = inv.GetInventarioAlmacenTotalForLot(pro.ProID, almId);
                    ConCantidadLineas += inventario.Count();
                }
            }

            con.Add("ConCantidadLineas", myParametro.GetParConteosFisicosLotesAgrupados() ? ConCantidadLineas : productoswitcant.Count);
            con.ExecuteInsert();

            if (GetProductosInTempForInvCant(almid))
            {
                DS_RepresentantesSecuencias.UpdateSecuencia("ConteosFisicos", conSecuencia);
                myProd.ClearTemp((int)Modules.CONTEOSFISICOS);
                return conSecuencia;
            }

            int pos = 1;

            foreach (var prod in productoswitcant)
            {
                if (myParametro.GetParConteosFisicosLotesAgrupados())
                {
                    var inventario = inv.GetInventarioAlmacenTotalForLot(prod.ProID, almId);

                    if(inventario.Count() == 0)
                    {
                        var det = new Hash("ConteosFisicosDetalle");
                        det.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                        det.Add("ConSecuencia", conSecuencia);
                        det.Add("ConPosicion", pos); pos++;
                        det.Add("ProID", prod.ProID);
                        det.Add("ConCantidadLogica", 0);
                        det.Add("ConCantidadDetalleLogica", 0);
                        det.Add("ConCantidad", prod.Cantidad);
                        det.Add("ConCantidadDetalle", prod.CantidadDetalle);
                        det.Add("ConPrecioVenta", prod.Precio);
                        det.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                        det.Add("ConFechaActualizacion", Functions.CurrentDate());
                        det.Add("rowguid", Guid.NewGuid().ToString());
                        det.Add("ConLote", "000000");
                        det.ExecuteInsert();
                        continue;
                    }
                    var CountInventario = inventario.Count();
                    var countInv = 0;
                    var cantidadRestante = prod.Cantidad;
                    inventario = inventario.OrderByDescending(i => i.invCantidad).ToList();
                    //if (myParametro.GetParAsignacionLotesByFechaVencimiento()) { inventario = inventario.OrderBy(i => i.InvLoteFechaVencimiento).ToList(); }
                    foreach (var inve in inventario)
                    {
                        countInv++;
                        var cantidadUsar = 0.0;

                        if (inve.invCantidad >= cantidadRestante)
                        {
                            cantidadUsar = cantidadRestante;
                            cantidadRestante -= cantidadUsar;
                        }
                        else if (cantidadRestante > inve.invCantidad && countInv == CountInventario)
                        {
                            cantidadUsar = cantidadRestante;
                            cantidadRestante -= cantidadUsar;
                        }
                        else if (cantidadRestante > inve.invCantidad)
                        {
                            cantidadUsar = inve.invCantidad;
                            cantidadRestante -= cantidadUsar;
                        }

                        var det = new Hash("ConteosFisicosDetalle");
                        det.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                        det.Add("ConSecuencia", conSecuencia);
                        det.Add("ConPosicion", pos); pos++;
                        det.Add("ProID", prod.ProID);
                        det.Add("ConCantidadLogica", inve.invCantidad);
                        det.Add("ConCantidadDetalleLogica", prod.InvCantidadDetalle);
                        det.Add("ConCantidad", cantidadUsar);
                        det.Add("ConCantidadDetalle", prod.CantidadDetalle);
                        det.Add("ConPrecioVenta", prod.Precio);
                        det.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                        det.Add("ConFechaActualizacion", Functions.CurrentDate());
                        det.Add("rowguid", Guid.NewGuid().ToString());
                        det.Add("ConLote", inve.InvLote);
                        det.ExecuteInsert();

                        if (myParametro.GetParConteoFisicoPorAlmacen() && parMultiAlmacenes && almIdToUse != -1 && ((int)cantidadUsar > 0 || prod.CantidadDetalle > 0) && (myParametro.GetParConteoConAlmacenDespachoyDevolucion() || myParametro.GetParConteoConVariosAlmacenes()))
                        {
                            var almacenesExcluir = new DS_Almacenes().GetAlmacenesByAlmIDParameter(myParametro.GetParNoCuadrarAlmacen());
                            if (almacenesExcluir == null || !almacenesExcluir.Exists(x => x.AlmID == almId))
                            {
                                if (inv.HayExistencia(prod.ProID, cantidadUsar, almId: almId))
                                {
                                    inv.RestarInventario(prod.ProID, (int)cantidadUsar, prod.CantidadDetalle, almId: almId, lote: prod.UsaLote ? inve.InvLote : null);
                                }
                                else
                                {
                                    double existenciaProducto = inv.GetCantidadTotalInventario(prod.ProID, almId);
                                    if (existenciaProducto > 0)
                                    {
                                        inv.RestarInventario(prod.ProID, existenciaProducto, prod.CantidadDetalle, almId: almId);
                                    }

                                }
                            }
                        }
                        else if (myParametro.GetParConteoFisicoPorAlmacen() && parMultiAlmacenes && almIdToUse != -1 && ((int)cantidadUsar > 0 || prod.CantidadDetalle > 0) && almIdMelma != almId)
                        {
                            inv.RestarInventario(prod.ProID, (int)cantidadUsar, prod.CantidadDetalle, almId: almIdRanchero, lote: prod.UsaLote ? inve.InvLote : null);
                            inv.AgregarInventario(prod.ProID, (int)cantidadUsar, prod.CantidadDetalle, almId: almIdToUse, lote: prod.UsaLote ? inve.InvLote : null);
                        }
                    }
                }
                else
                {
                    var det = new Hash("ConteosFisicosDetalle");
                    det.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                    det.Add("ConSecuencia", conSecuencia);
                    det.Add("ConPosicion", pos); pos++;
                    det.Add("ProID", prod.ProID);
                    det.Add("ConCantidadLogica", prod.InvCantidad);
                    det.Add("ConCantidadDetalleLogica", prod.InvCantidadDetalle);
                    det.Add("ConCantidad", prod.Cantidad);
                    det.Add("ConCantidadDetalle", prod.CantidadDetalle);
                    det.Add("ConPrecioVenta", prod.Precio);
                    det.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                    det.Add("ConFechaActualizacion", Functions.CurrentDate());
                    det.Add("rowguid", Guid.NewGuid().ToString());
                    det.Add("ConLote", prod.Lote);
                    det.ExecuteInsert();


                    if (!myParametro.GetParConteoFisicoPorAlmacen())
                    {
                        var cantidadTotal = (prod.Cantidad * prod.ProUnidades) + prod.CantidadDetalle;
                        var cantidadInvTotal = (prod.InvCantidad * prod.ProUnidades) + prod.InvCantidadDetalle;

                        if (cantidadTotal > cantidadInvTotal)
                        {
                            cantidadTotal = cantidadInvTotal;
                        }

                        var resultRaw2 = cantidadTotal / prod.ProUnidades;

                        var cantidad = Math.Truncate(resultRaw2);
                        var cantidadDetalle = (int)Math.Round((resultRaw2 - Math.Truncate(resultRaw2)) * prod.ProUnidades);

                        if (cantidad < 0)
                        {
                            cantidad = 0;
                        }

                        if (cantidadDetalle < 0)
                        {
                            cantidadDetalle = 0;
                        }

                        if ((cantidad > 0 || cantidadDetalle > 0) && parMultiAlmacenes && almIdToUse != -1)
                        {
                            inv.RestarInventario(prod.ProID, cantidad, cantidadDetalle, almId: almIdToUse, lote: prod.UsaLote ? prod.Lote : null);
                        }
                    }


                    if (myParametro.GetParConteoFisicoPorAlmacen() && parMultiAlmacenes && almIdToUse != -1 && ((int)prod.Cantidad > 0 || prod.CantidadDetalle > 0) && (myParametro.GetParConteoConAlmacenDespachoyDevolucion() || myParametro.GetParConteoConVariosAlmacenes()))
                    {
                        var almacenesExcluir = new DS_Almacenes().GetAlmacenesByAlmIDParameter(myParametro.GetParNoCuadrarAlmacen());
                        if (almacenesExcluir == null || !almacenesExcluir.Exists(x => x.AlmID == almId))
                        {
                            if (inv.HayExistencia(prod.ProID, prod.Cantidad, almId: almId))
                            {
                                inv.RestarInventario(prod.ProID, (int)prod.Cantidad, prod.CantidadDetalle, almId: almId, lote: prod.UsaLote ? prod.Lote : null);
                            }
                            else
                            {
                                double existenciaProducto = inv.GetCantidadTotalInventario(prod.ProID, almId);
                                if (existenciaProducto > 0)
                                {
                                    inv.RestarInventario(prod.ProID, existenciaProducto, prod.CantidadDetalle, almId: almId);
                                }

                            }
                        }
                    }
                    else if (myParametro.GetParConteoFisicoPorAlmacen() && parMultiAlmacenes && almIdToUse != -1 && ((int)prod.Cantidad > 0 || prod.CantidadDetalle > 0) && almIdMelma != almId)
                    {
                        inv.RestarInventario(prod.ProID, (int)prod.Cantidad, prod.CantidadDetalle, almId: almIdRanchero, lote: prod.UsaLote ? prod.Lote : null);
                        inv.AgregarInventario(prod.ProID, (int)prod.Cantidad, prod.CantidadDetalle, almId: almIdToUse, lote: prod.UsaLote ? prod.Lote : null);
                    }


                }

            }           

            var productosFaltantes = myParametro.GetParConteosFisicosLotesAgrupados() ? GetProductosInInventarioFaltantesAgrupados(almid) :  GetProductosInInventarioFaltantes(almid);
            var parCrearFacturaPorFaltante = DS_RepresentantesParametros.GetInstance().GetParCrearFacturaByConteoFisico();
            //productos que no estan en el inventario
            foreach (var prod in productosFaltantes)
            {
                var det = new Hash("ConteosFisicosDetalle");
                det.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                det.Add("ConSecuencia", conSecuencia);
                det.Add("ConPosicion", pos); pos++;
                det.Add("ProID", prod.ProID);
                det.Add("ConCantidadLogica", prod.InvCantidad);
                det.Add("ConCantidadDetalleLogica", prod.InvCantidadDetalle);
                det.Add("ConCantidad", 0);
                det.Add("ConCantidadDetalle", 0);
                det.Add("ConPrecioVenta", 0);
                det.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                det.Add("ConFechaActualizacion", Functions.CurrentDate());
                det.Add("rowguid", Guid.NewGuid().ToString());
                det.Add("ConLote", prod.Lote);
                det.ExecuteInsert();

                if (!myParametro.GetParConteoFisicoPorAlmacen() && parMultiAlmacenes && almIdToUse != -1 && !parCrearFacturaPorFaltante)
                {
                    inv.RestarInventario(prod.ProID, prod.InvCantidad, prod.InvCantidadDetalle, almId: almIdToUse, lote: prod.UsaLote ? prod.Lote : null);
                }

                if (myParametro.GetParConteoFisicoPorAlmacen() && parMultiAlmacenes && almIdToUse != -1 && ((int)prod.Cantidad > 0 || prod.CantidadDetalle > 0) && almIdMelma != almId && !myParametro.GetParConteoConVariosAlmacenes())
                {
                    inv.RestarInventario(prod.ProID, prod.InvCantidad, prod.InvCantidadDetalle, almId: almIdToUse, lote: prod.UsaLote ? prod.Lote : null);
                    inv.AgregarInventario(prod.ProID, prod.InvCantidad, prod.InvCantidadDetalle, almId: almId, lote: prod.UsaLote ? prod.Lote : null);
                }
               
            }

            int venSecuencia = 0;

           // var almid = myParametro.GetParAlmacenIdParaMelma() == almIdToUse ? almIdToUse : myParametro.GetParAlmacenVentaRanchera();
            var faltantes = ProductosFaltantes.Where(p => ((p.InvCantidad * p.ProUnidades) + p.InvCantidadDetalle) > ((p.Cantidad * p.ProUnidades) + p.CantidadDetalle) && !p.ProDatos3.Contains("B")).ToList();//GetProductosInInventarioConFaltantes(DS_RepresentantesParametros.GetInstance().GetParClienteForRepresentantes(), almid);//.Where(x => x.InvCantidad > x.Cantidad).ToList();
            //GetProductosInInventarioAndTemp(DS_RepresentantesParametros.GetInstance().GetParClienteForRepresentantes())

            if (ProductosFaltantesAjustados != null)
            {
                faltantes = ProductosFaltantesAjustados;
            }

            if (parCrearFacturaPorFaltante && faltantes.Count > 0)
            {
                venSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Ventas");

                if (Arguments.Values.ANTSMODULES == Modules.ANTMODULE)
                {
                    Arguments.Values.CurrentTraSecuencia = venSecuencia;
                }

                var ncf = new DS_Clientes().GetSiguienteNCF(Arguments.Values.CurrentClient);

                if (ncf == null)
                {
                    throw new Exception("No se encontro secuencia de NCF");
                }

                var VenTotal = 0;
                if (myParametro.GetParConteosFisicosLotesAgrupados())
                {
                    foreach (var pro in faltantes)
                    {
                        var inventario = inv.GetInventarioAlmacenTotalForLot(pro.ProID, almId);
                        VenTotal += inventario.Count();
                    }
                }

                Hash ven = new Hash("Ventas");
                ven.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                ven.Add("VenSecuencia", venSecuencia);
                ven.Add("CliID", DS_RepresentantesParametros.GetInstance().GetParClienteForRepresentantes());//Arguments.CurrentUser.RepCodigo);
                ven.Add("VisSecuencia", -1);
                ven.Add("VenFecha", Functions.CurrentDate());
                ven.Add("VenEstatus", 1);
                ven.Add("VenTotal", myParametro.GetParConteosFisicosLotesAgrupados() ? VenTotal : faltantes.Count);
                ven.Add("mbVersion", Functions.AppVersion);
                ven.Add("ConID", myCondPago.GetConIdCliente(DS_RepresentantesParametros.GetInstance().GetParClienteForRepresentantes()));
                ven.Add("VenNCF", (Arguments.Values.CurrentClient.CliTipoComprobanteFAC == "99" ? venSecuencia + "-" + Arguments.CurrentUser.RepCodigo : ncf.NCFCompleto));
                ven.Add("VenNCFFechaVencimiento", "");
                ven.Add("RutID", Arguments.CurrentUser.RutID);
                ven.Add("VenReferencia", (DS_RepresentantesParametros.GetInstance().GetParVentasFaltantesPorAlmacen()) ? almid.ToString() : "");
                ven.Add("CuaSecuencia", Arguments.Values.CurrentCuaSecuencia);
                ven.Add("VenCantidadCanastos", 0);
                ven.Add("VenCantidadImpresion", 0);
                ven.Add("MonCodigo", Arguments.Values.CurrentClient.MonCodigo);
                ven.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                ven.Add("VenFechaOrden", Functions.CurrentDate());
                ven.Add("VenFechaActualizacion", Functions.CurrentDate());
                ven.Add("rowguid", Guid.NewGuid().ToString());

                //var valAlmId = almIdRanchero > 0 ? almIdRanchero : almIdDevolucion;

                var almacen = new DS_Almacenes().GetAlmacenById(almId);

                if(almacen != null && almacen.AlmCaracteristicas.ToUpper().Contains("P") && myParametro.GetParConteoFisicoPorAlmacen())
                {
                    ven.Add("RepVendedor", Arguments.CurrentUser.RepCodigo);
                    ven.Add("PedSecuencia", venSecuencia);
                }

                ven.ExecuteInsert();

                pos = 1;
                var MontoDiferencia = 0.0;
                var VenTotalU = 0;

                foreach (var pro in faltantes)
                {

                    if (myParametro.GetParConteosFisicosLotesAgrupados())
                    {
                        var inventario = inv.GetInventarioAlmacenTotalForLot(pro.ProID, almId);
                        var cantidadRestante = pro.Cantidad;
                        inventario = inventario.OrderByDescending(i => i.invCantidad).ToList();
                        //if (myParametro.GetParAsignacionLotesByFechaVencimiento()) { inventario = inventario.OrderBy(i => i.InvLoteFechaVencimiento).ToList(); }
                        foreach (var inve in inventario)
                        {
                            var cantidadUsar = 0.0;
                            var almacenesExcluir = new DS_Almacenes().GetAlmacenesByAlmIDParameter(myParametro.GetParNoCuadrarAlmacen());
                            if (almacenesExcluir == null || !almacenesExcluir.Exists(x => x.AlmID == almId))
                            {
                                if (inve.invCantidad >= cantidadRestante)
                                {
                                    cantidadUsar = cantidadRestante;
                                    cantidadRestante -= cantidadUsar;
                                }
                                else if (cantidadRestante > inve.invCantidad)
                                {
                                    cantidadUsar = inve.invCantidad;
                                    cantidadRestante -= cantidadUsar;
                                }
                            }

                            var cantidadInvTotal = (inve.invCantidad * pro.ProUnidades) + pro.InvCantidadDetalle;
                            var resultRaw = cantidadInvTotal - cantidadUsar;
                            var resultRaw2 = resultRaw / pro.ProUnidades;
                            var cantidad = Math.Truncate(resultRaw2);
                            var cantidadDetalle = (int)Math.Round((resultRaw2 - Math.Truncate(resultRaw2)) * pro.ProUnidades);
                            if (myParametro.GetCantidadxLibras())
                            {
                                cantidad = resultRaw2;
                                cantidadDetalle = 0;
                            }

                            if (cantidad == 0)
                            {
                                continue;
                            }

                            if (cantidad < 0)
                            {
                                cantidad = 0;
                            }

                            if (cantidadDetalle < 0)
                            {
                                cantidadDetalle = 0;
                            }

                            VenTotalU++;
                            Hash det = new Hash("VentasDetalle");
                            det.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                            det.Add("VenSecuencia", venSecuencia);
                            det.Add("VenPosicion", pos); pos++;
                            det.Add("ProID", pro.ProID);
                            det.Add("VenCantidad", cantidad);
                            det.Add("VenCantidadDetalle", cantidadDetalle);
                            det.Add("VenPrecio", pro.Precio);
                            det.Add("VenItbis", pro.Itbis);
                            det.Add("VenSelectivo", pro.Selectivo);
                            det.Add("VenAdValorem", pro.AdValorem);
                            det.Add("VenDescuento", pro.Descuento);
                            det.Add("VenTotalItbis", (Math.Round((pro.Precio + pro.Selectivo + pro.AdValorem - pro.Descuento) * (pro.Itbis / 100), 2)) * (cantidadDetalle > 0 ? cantidad + (double.Parse(cantidadDetalle.ToString()) / double.Parse(pro.ProUnidades.ToString())) : 0 / pro.ProUnidades > 0 ? pro.ProUnidades : 0 + cantidad));
                            det.Add("VenTotalDescuento", pro.Descuento * cantidad);
                            det.Add("VenindicadorOferta", pro.IndicadorOferta);
                            det.Add("VenDescPorciento", pro.DesPorciento);
                            det.Add("VenCantidadEntregada", 0);
                            det.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                            det.Add("VenFechaActualizacion", Functions.CurrentDate());
                            det.Add("rowguid", Guid.NewGuid().ToString());
                            det.Add("UnmCodigo", pro.UnmCodigo);
                            det.Add("VenLote", inve.InvLote);
                            det.ExecuteInsert();

                            double itbis = double.Parse(pro.Itbis.ToString()) / 100;
                            double PrecioProducto = Math.Round(pro.Precio + (pro.Precio * (itbis)), 2);
                            MontoDiferencia += Math.Abs(PrecioProducto * resultRaw);

                            if (!myParametro.GetParVentasNoRebajaInventario())
                            {
                                if (myParametro.GetParConteoFisicoPorAlmacen() && parMultiAlmacenes && almIdToUse != -1)
                                {

                                    inv.RestarInventario(pro.ProID, cantidad, cantidadDetalle, almid, lote: pro.UsaLote ? inve.InvLote : null);

                                }
                                else
                                {
                                    inv.RestarInventario(pro.ProID, cantidad, cantidadDetalle, almId: almIdToUse, lote: pro.UsaLote ? inve.InvLote : null);
                                }
                            }
                            
                        }

                        var v = new Hash("Ventas");
                        v.Add("VenTotal", VenTotalU);
                        v.ExecuteUpdate("VenSecuencia = " + venSecuencia.ToString()+ " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'" );



                    }
                    else
                    {
                        Hash det = new Hash("VentasDetalle");
                        det.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                        det.Add("VenSecuencia", venSecuencia);
                        det.Add("VenPosicion", pos); pos++;
                        det.Add("ProID", pro.ProID);

                        var cantidadTotal = (pro.CantidadManual * pro.ProUnidades) + pro.CantidadManualDetalle;
                        var cantidadInvTotal = (pro.InvCantidad * pro.ProUnidades) + pro.InvCantidadDetalle;

                        var resultRaw = cantidadInvTotal - cantidadTotal;

                        var resultRaw2 = resultRaw / pro.ProUnidades;

                        var cantidad = Math.Truncate(resultRaw2);
                        var cantidadDetalle = (int)Math.Round((resultRaw2 - Math.Truncate(resultRaw2)) * pro.ProUnidades);

                        if (myParametro.GetCantidadxLibras())
                        {
                            cantidad = resultRaw2;
                            cantidadDetalle = 0;
                        }

                        if (cantidad < 0)
                        {
                            cantidad = 0;
                        }

                        if (cantidadDetalle < 0)
                        {
                            cantidadDetalle = 0;
                        }

                        if (pro.Cantidad == 0 && pro.CantidadDetalle == 0)
                        {
                            cantidad = pro.InvCantidad;
                            cantidadDetalle = pro.InvCantidadDetalle;
                        }

                        det.Add("VenCantidad", cantidad);
                        det.Add("VenCantidadDetalle", cantidadDetalle);
                        det.Add("VenPrecio", pro.Precio);
                        det.Add("VenItbis", pro.Itbis);
                        det.Add("VenSelectivo", pro.Selectivo);
                        det.Add("VenAdValorem", pro.AdValorem);
                        det.Add("VenDescuento", pro.Descuento);
                        det.Add("VenTotalItbis", (Math.Round((pro.Precio + pro.Selectivo + pro.AdValorem - pro.Descuento) * (pro.Itbis / 100), 2)) * (cantidadDetalle > 0 ? cantidad + (double.Parse(cantidadDetalle.ToString()) / double.Parse(pro.ProUnidades.ToString())) : 0 / pro.ProUnidades > 0 ? pro.ProUnidades : 0 + cantidad));
                        det.Add("VenTotalDescuento", pro.Descuento * cantidad);
                        det.Add("VenindicadorOferta", pro.IndicadorOferta);
                        det.Add("VenDescPorciento", pro.DesPorciento);
                        det.Add("VenCantidadEntregada", 0);
                        det.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                        det.Add("VenFechaActualizacion", Functions.CurrentDate());
                        det.Add("UnmCodigo", pro.UnmCodigo);
                        det.Add("rowguid", Guid.NewGuid().ToString());
                        det.Add("VenLote", pro.Lote);

                       

                        det.ExecuteInsert();

                        double itbis = double.Parse(pro.Itbis.ToString()) / 100;
                        double PrecioProducto = Math.Round(pro.Precio + (pro.Precio * (itbis)), 2);
                        MontoDiferencia += Math.Abs(PrecioProducto * resultRaw);


                        var cantidadTotalRestar = (pro.CantidadManual * pro.ProUnidades) + pro.CantidadDetalle;
                        var cantidadInvTotalRestar = (pro.InvCantidad * pro.ProUnidades) + pro.InvCantidadDetalle;

                        resultRaw = cantidadInvTotalRestar - cantidadTotalRestar;

                        resultRaw2 = resultRaw / pro.ProUnidades;

                        cantidad = Math.Truncate(resultRaw2);
                        cantidadDetalle = (int)Math.Round((resultRaw2 - Math.Truncate(resultRaw2)) * pro.ProUnidades);

                        if (myParametro.GetCantidadxLibras())
                        {
                            cantidad = resultRaw2;
                            cantidadDetalle = 0;
                        }

                        if (cantidad < 0)
                        {
                            cantidad = 0;
                        }

                        if (cantidadDetalle < 0)
                        {
                            cantidadDetalle = 0;
                        }

                        if (pro.Cantidad == 0 && pro.CantidadDetalle == 0)
                        {
                            cantidad = pro.InvCantidad;
                            cantidadDetalle = pro.InvCantidadDetalle;
                        }

                        /* if (parMultiAlmacenes && almIdToUse != -1)
                         {*/
                        //  inv.RestarInventario(pro.ProID, cantidad, cantidadDetalle, almId: almIdToUse, lote: pro.UsaLote ? pro.Lote : null);
                        // }

                        if (!myParametro.GetParVentasNoRebajaInventario())
                        {
                            if (myParametro.GetParConteoFisicoPorAlmacen() && parMultiAlmacenes && almIdToUse != -1)
                            {

                                inv.RestarInventario(pro.ProID, cantidad, cantidadDetalle, /*almIdDespacho*/almid, lote: pro.UsaLote ? pro.Lote : null);
                                //   inv.RestarInventario(pro.ProID, cantidad, cantidadDetalle, almId: almIdMelma, lote: pro.UsaLote ? pro.Lote : null);
                            }
                            else
                            {
                                inv.RestarInventario(pro.ProID, cantidad, cantidadDetalle, almId: almIdToUse, lote: pro.UsaLote ? pro.Lote : null);
                            }
                        }
                        
                    }
 
                }

                DS_RepresentantesSecuencias.UpdateSecuencia("Ventas", venSecuencia);
                myVen.ActualizarNcfActual(ncf.Secuencia.ToString(), ncf.rowguid);

                string LPCuadre = "";
                bool NoUseLP = DS_RepresentantesParametros.GetInstance().GetParNoListaPrecios();
                if (!NoUseLP)
                {
                    if (!String.IsNullOrEmpty(DS_RepresentantesParametros.GetInstance().GetParListaPreciosCuadre()))
                    {
                        LPCuadre = DS_RepresentantesParametros.GetInstance().GetParListaPreciosCuadre();
                    }
                    else
                    {
                        LPCuadre = Arguments.Values.CurrentClient.LiPCodigo; //myUsosMul.GetFirstListaPrecio();
                    }
                }

                var productosprue = GetDetalleConteoBySecuencia(conSecuencia, LPCuadre);
                double MontoDiferencias = 0.0;

                foreach (var prod in productosprue)
                {
                    var TotalCaja = myProd.ConvertirUnidadesACajas(
                        myProd.ConvertirCajasAunidades(prod.ConCantidadLogica, prod.ConCantidadDetalleLogica, myProd.GetProUnidades(prod.ProID),
                        prod.ConCantidad, prod.ConCantidadDetalle), myProd.GetProUnidades(prod.ProID));

                    var DiferenciaCaja = TotalCaja;
                    var DiferenciaUnidades = Math.Round((DiferenciaCaja - DiferenciaCaja) * myProd.GetProUnidades(prod.ProID), 0);

                    double itbis = double.Parse(prod.Itbis.ToString()) / 100;
                    double PrecioProducto = Math.Round(prod.Precio + (prod.Precio * (itbis)), 2);
                    if (TotalCaja < 0)
                    {
                        MontoDiferencias += Math.Abs(PrecioProducto * DiferenciaCaja);
                    }
                }

                Hash cxc = new Hash("CuentasxCobrar");
                cxc.Add("CxcReferencia", venSecuencia);
                cxc.Add("CxcTipoTransaccion", 4);
                cxc.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                cxc.Add("CxcDias", 0);
                cxc.Add("CxcSIGLA", "FAT");
                cxc.Add("CliID", DS_RepresentantesParametros.GetInstance().GetParClienteForRepresentantes());
                cxc.Add("CxcFecha", Functions.CurrentDate());
                cxc.Add("CxcDocumento", Arguments.CurrentUser.RepCodigo + "-" + venSecuencia);
                cxc.Add("CxcBalance", MontoDiferencias);
                cxc.Add("CxcMontoSinItbis", null);
                cxc.Add("CxcMontoTotal", MontoDiferencias);
                cxc.Add("MonCodigo", Arguments.Values.CurrentClient.MonCodigo);
                cxc.Add("AreaCtrlCredit", 0);
                cxc.Add("CxcNotaCredito", 0);
                cxc.Add("CXCNCF", (Arguments.Values.CurrentClient.CliTipoComprobanteFAC == "99" ? venSecuencia + "-" + Arguments.CurrentUser.RepCodigo : ncf.NCFCompleto));
                cxc.Add("rowguid", Guid.NewGuid().ToString());
                cxc.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                cxc.Add("CueFechaActualizacion", Functions.CurrentDate());
                cxc.Add("CxcFechaVencimiento", null/*myCxCobrar.GetCxcFechaVencimiento(conId)*/);
                cxc.Add("ConID", Arguments.Values.CurrentClient.ConID);
                cxc.ExecuteInsert();

                /////////////////////////////////
                ///
                if (!DS_RepresentantesParametros.GetInstance().GetParNoReciboAutomatico() && Arguments.Values.CurrentClient.ConID == myParametro.GetParConIdFormaPagoContado() && Arguments.Values.CurrentClient.ConID != -1) //si la venta es al contado
                {
                    int RecSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Recibos");
                    int reaSecuencia = 0;

                    Hash rec = new Hash("Recibos");
                    rec.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                    rec.Add("RecTipo", 1);
                    rec.Add("RecSecuencia", RecSecuencia);
                    rec.Add("CliID", Arguments.Values.CurrentClient.CliID);
                    rec.Add("RecFecha", Functions.CurrentDate());
                    rec.Add("RecNumero", RecSecuencia);
                    rec.Add("RecEstatus", 1);
                    rec.Add("RecMontoNcr", 0);
                    rec.Add("RecMontoDescuento", 0);
                    rec.Add("RecMontoEfectivo", MontoDiferencias);
                    rec.Add("RecMontoCheque", 0);
                    rec.Add("RecMontoChequeF", 0);
                    rec.Add("RecMontoTransferencia", 0);
                    rec.Add("RecMontoTarjeta", 0);
                    rec.Add("RecMontoSobrante", 0);
                    rec.Add("CuaSecuencia", Arguments.Values.CurrentCuaSecuencia);
                    rec.Add("VisSecuencia", -1);
                    rec.Add("DepSecuencia", 0);
                    rec.Add("RecRetencion", 0);
                    rec.Add("RecDivision", 0);
                    rec.Add("SecCodigo", (Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.SecCodigo : ""));
                    rec.Add("AreactrlCredit", Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.AreaCtrlCredit : null);
                    rec.Add("MonCodigo", Arguments.Values.CurrentClient.MonCodigo);
                    rec.Add("OrvCodigo", "");
                    rec.Add("OfvCodigo", "");
                    rec.Add("RecCantidadImpresion", 0);
                    rec.Add("RecTotal", MontoDiferencias);
                    rec.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                    rec.Add("RecFechaActualizacion", Functions.CurrentDate());
                    rec.Add("RecCantidadDetalleAplicacion", 1);
                    rec.Add("RecCantidadDetalleFormaPago", 1);
                    rec.Add("rowguid", Guid.NewGuid().ToString());
                    rec.Add("mbVersion", Functions.AppVersion);
                    rec.Add("RecTasa", 1);
                    rec.ExecuteInsert();

                    ///////////Aplicado//////////////
                    ///
                    Hash ap = new Hash("RecibosAplicacion");
                    ap.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                    ap.Add("RecTipo", 1);
                    ap.Add("RecSecuencia", RecSecuencia);
                    //reaSecuencia++;
                    ap.Add("ReaSecuencia", reaSecuencia);
                    ap.Add("SocCodigo", Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.AreaCtrlCredit : null);
                    ap.Add("CXCReferencia", (Arguments.Values.CurrentClient.CliTipoComprobanteFAC == "99" ? venSecuencia + "-" + Arguments.CurrentUser.RepCodigo : ncf.NCFCompleto));
                    ap.Add("RecValor", MontoDiferencias);
                    ap.Add("RecDescuento", 0);
                    ap.Add("RecIndicadorSaldo", 1);
                    ap.Add("CxcSigla", "FAT");
                    ap.Add("repCodigo2", "");
                    ap.Add("AutID", null);
                    ap.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                    ap.Add("RecFechaActualizacion", Functions.CurrentDate());
                    ap.Add("rowguid", Guid.NewGuid().ToString());
                    ap.Add("RecPorcDescuento", 0);
                    ap.Add("RecMontoADescuento", 0);
                    ap.Add("CxCDocumento", Arguments.CurrentUser.RepCodigo + "-" + venSecuencia);
                    ap.Add("DefIndicadorItbis", 0);
                    ap.Add("cliid", Arguments.Values.CurrentClient.CliID);
                    ap.ExecuteInsert();

                    //////////Forma Pago
                    ///
                    Hash pago = new Hash("RecibosFormaPago");
                    pago.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                    pago.Add("RecTipo", 1);
                    pago.Add("RecSecuencia", RecSecuencia);
                    pago.Add("RefSecuencia", 1);
                    pago.Add("AutSecuencia", null);
                    pago.Add("ForID", 1);
                    pago.Add("BanID", 0);
                    pago.Add("RefNumeroCheque", 0);
                    pago.Add("RefFecha", Functions.CurrentDate());
                    pago.Add("RefIndicadorDiferido", 0);
                    pago.Add("RefNumeroAutorizacion", 0);
                    pago.Add("RefValor", MontoDiferencias);
                    pago.Add("CXCReferencia", (Arguments.Values.CurrentClient.CliTipoComprobanteFAC == "99" ? venSecuencia + "-" + Arguments.CurrentUser.RepCodigo : ncf.NCFCompleto));
                    pago.Add("cliid", Arguments.Values.CurrentClient.CliID);
                    //if de multimoneda
                    pago.Add("RecPrima", MontoDiferencias);
                    pago.Add("MonCodigo", Arguments.Values.CurrentClient.MonCodigo);
                    pago.Add("RecTasa", 0);
                    pago.Add("SocCodigo", Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.AreaCtrlCredit : null);
                    pago.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                    pago.Add("RecFechaActualizacion", Functions.CurrentDate());
                    pago.Add("rowguid", Guid.NewGuid().ToString());
                    pago.ExecuteInsert();
                    DS_RepresentantesSecuencias.UpdateSecuencia("Recibos", RecSecuencia);
                }
            }

            DS_RepresentantesSecuencias.UpdateSecuencia("ConteosFisicos", conSecuencia);

            myProd.ClearTemp((int)Modules.CONTEOSFISICOS);
            myProd.ClearTemp((int)Modules.VENTAS);

            return conSecuencia;
        }

        private List<ProductosTemp> GetProductosInTemp(int almId = -1)
        {
            var join = "left join Inventarios i on i.ProID = t.ProID ";

            if(almId != -1)
            {
                join = "left join InventariosAlmacenesRepresentantes i on i.ProID = t.ProID and i.RepCodigo = '"+Arguments.CurrentUser.RepCodigo+"' and i.AlmID = " + almId + " and ifnull(i.InvLote, '') = ifnull(t.Lote, '') ";
            }

            return SqliteManager.GetInstance().Query<ProductosTemp>("select t.ProID as ProID, t.Lote as Lote, t.ProDatos3 as ProDatos3, t.Cantidad as Cantidad, t.CantidadDetalle as CantidadDetalle, " +
                "i.invCantidad as InvCantidad, i.InvCantidadDetalle as InvCantidadDetalle, t.ProUnidades as ProUnidades " +
                "from ProductosTemp t " +
                join +
                "where t.TitID = ? ", new string[] { ((int)Modules.CONTEOSFISICOS).ToString() });
        }
        private bool GetProductosInTempForInvCant(int almId = -1)
        {
            var join = "left join Inventarios i on i.ProID = t.ProID ";

            if (almId != -1)
            {
                join = "left join InventariosAlmacenesRepresentantes i on i.ProID = t.ProID and i.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' and i.AlmID = " + almId + " and ifnull(i.InvLote, '') = ifnull(t.Lote, '') ";
            }

            var list = SqliteManager.GetInstance().Query<ProductosTemp>("select t.ProID as ProID, t.Lote as Lote, t.ProDatos3 as ProDatos3, t.Cantidad as Cantidad, t.CantidadDetalle as CantidadDetalle, " +
                "i.invCantidad as InvCantidad, i.InvCantidadDetalle as InvCantidadDetalle, t.ProUnidades as ProUnidades " +
                "from ProductosTemp t " +
                join +
                "where t.TitID = ? ", new string[] { ((int)Modules.CONTEOSFISICOS).ToString() });

            if (list.Count == 0)
            {
                return true;
            }

            return false;
        }

        private List<ProductosTemp> GetProductosInInventarioFaltantes(int almId = -1)
        {
            var sql = "select " + (almId != -1 ? "i.InvLote as Lote, " : "") + "i.ProID as ProID, i.invCantidad as InvCantidad, i.InvCantidadDetalle as InvCantidadDetalle, p.Proprecio as Precio, p.proitbis as Itbis, p.prounidades as ProUnidades, p.ProDatos3 as ProDatos3 " +
                "from " + (almId != -1 ? "InventariosAlmacenesRepresentantes" : "Inventarios") + " i " +
                "left join productos p on p.proid = i.proid " +
                "where " + (almId != -1 ? "i.AlmID = " + almId + " and " : "") + " i.proid not in (select proid from ProductosTemp where " + (almId != -1 ? "case when ifnull(p.ProDatos3, '') like '%L%' then ifnull(i.InvLote, '') = ifnull(Lote, '') else 1=1 end and " : "") + " TitID = " + ((int)Modules.CONTEOSFISICOS).ToString() + ") and (i.invCantidad >0 or i.invCantidadDetalle > 0) ";


            return SqliteManager.GetInstance().Query<ProductosTemp>(sql, new string[] { });
        }

        public List<ProductosTemp> GetProductosInInventarioConFaltantesyLotesAgrupados(int CliID, int almId = -1, bool proParaFacturar = false, bool IsForValid = false)
        {
            var query = "";
            var parInventarioLote = -1;
            var parTipoRelacionClientes = myParametro.GetParTipoRelacionClientes();
            var parUmnCodigoByLista = myParametro.GetParConteoFisicoUsaUnidadMedidaByListaPrecio();

            if (IsForValid)
            {
                query = "select " + (almId != -1 ? " i.AlmID, " : "") + " ifnull(p.ProDatos3, '') as ProDatos3, " + (parUmnCodigoByLista ? "l.UnmCodigo" : "p.UnmCodigo") + " as UnmCodigo, i.ProID as ProID, p.ProCodigo as ProCodigo, p.ProDescripcion as Descripcion, sum(i.invCantidad) as InvCantidad, i.InvCantidadDetalle as InvCantidadDetalle, " +
                  "l.lipprecio as Precio, p.proitbis as Itbis, p.prounidades as ProUnidades, /*ifnull(i.invCantidad, 0) - */ ifnull(t.Cantidad,0) as Cantidad, /*ifnull(i.invCantidad, 0) -*/ ifnull(t.Cantidad,0) as CantidadManual, " +
                  "0 as CantidadManualDetalle, 0 as CantidadDetalle, ifnull(t.Lote, '') as Lote " +
                  "from " + (almId != -1 ? "InventariosAlmacenesRepresentantes" : "Inventarios") + " i " +
                  "inner join Productos p on p.proid = i.proid " +
                  "left join productostemp t on t.proid = i.proid and t.TitID = " + ((int)Modules.CONTEOSFISICOS).ToString() + " " +
                  "left join listaprecios l on l.proid = i.proid " +
                  "where " + (almId != -1 ? "i.AlmID = " + almId + " and " + (parInventarioLote > 0 ? "case when ifnull(p.ProDatos3, '') like '%L%' then ifnull(i.InvLote, '') = ifnull(t.Lote, '') else 1=1 end and " : "") : "") + " l.lipcodigo = (select LipCodigo from " + (parTipoRelacionClientes == 2 ? "ClientesDetalle" : "Clientes") + " Where CliID = '" + CliID + "' limit 1) " +
                  "and trim(upper(l.unmCodigo)) = trim(upper(t.UnmCodigo)) " +
                  "Group By p.ProID " +
                  "HAVING(((sum(i.invCantidad) * p.ProUnidades) + ifnull(i.InvCantidadDetalle, 0)) > ((ifnull(t.Cantidad, 0) * p.ProUnidades) + ifnull(t.CantidadDetalle, 0))) " +
                  @" union 
	            select " + (almId != -1 ? " i.AlmID, " : "") + @" ifnull(p.ProDatos3, '') as ProDatos3, " + (parUmnCodigoByLista ? "l.UnmCodigo" : "p.UnmCodigo") + @" as UnmCodigo, i.ProID as ProID, p.ProCodigo as ProCodigo, p.ProDescripcion as Descripcion, i.invCantidad as InvCantidad, i.InvCantidadDetalle as InvCantidadDetalle, 
                l.lipprecio as Precio, p.proitbis as Itbis, p.prounidades as ProUnidades, 0 as Cantidad, i.invCantidad as CantidadManual, i.InvCantidadDetalle as CantidadManualDetalle, 
	            0 as CantidadDetalle, " + (parInventarioLote > 0 && almId != -1 ? "ifnull(i.InvLote, '')" : "''") + @" as Lote
                from " + (almId != -1 ? "InventariosAlmacenesRepresentantes" : "Inventarios") + @" i 
                inner join Productos p on p.proid = i.proid 
                left join listaprecios l on l.proid = i.proid 
                where " + (almId != -1 ? "i.AlmID = " + almId + " and " : "") + @" l.lipcodigo = (select LipCodigo from " + (parTipoRelacionClientes == 2 ? "ClientesDetalle" : "Clientes") + " Where CliID = '" + CliID + "' limit 1) " +
                  @" " + (parUmnCodigoByLista ? "" : "and trim(upper(l.unmCodigo)) = trim(upper(p.UnmCodigo))") + "  and  i.proid not in (select proid from Productostemp where TitID = " + ((int)Modules.CONTEOSFISICOS).ToString() + " " + (parInventarioLote > 0 && almId != -1 ? " and case when ifnull(p.ProDatos3, '') like '%L%' then ifnull(Lote, '') = ifnull(i.invLote, '') else 1=1 end " : "") + " )" +
                "Group By p.ProID " +
                "HAVING((((sum(i.invCantidad) * p.ProUnidades) + ifnull(i.InvCantidadDetalle, 0)) >  0)) ";
            }
            else
            {
                query = "select " + (almId != -1 ? " i.AlmID, " : "") + " ifnull(p.ProDatos3, '') as ProDatos3, " + (parUmnCodigoByLista ? "l.UnmCodigo" : "p.UnmCodigo") + " as UnmCodigo, i.ProID as ProID, p.ProCodigo as ProCodigo, p.ProDescripcion as Descripcion, sum(i.invCantidad) as InvCantidad, i.InvCantidadDetalle as InvCantidadDetalle, " +
                  "l.lipprecio as Precio, p.proitbis as Itbis, p.prounidades as ProUnidades, /*ifnull(i.invCantidad, 0) - */ ifnull(t.Cantidad,0) as Cantidad, /*ifnull(i.invCantidad, 0) -*/ ifnull(t.Cantidad,0) as CantidadManual, " +
                  "0 as CantidadManualDetalle, 0 as CantidadDetalle, ifnull(t.Lote, '') as Lote " +
                  "from " + (almId != -1 ? "InventariosAlmacenesRepresentantes" : "Inventarios") + " i " +
                  "inner join Productos p on p.proid = i.proid " +
                  "left join productostemp t on t.proid = i.proid and t.TitID = " + ((int)Modules.CONTEOSFISICOS).ToString() + " " +
                  "left join listaprecios l on l.proid = i.proid " +
                  "where " + (almId != -1 ? "i.AlmID = " + almId + " and " + (parInventarioLote > 0 ? "case when ifnull(p.ProDatos3, '') like '%L%' then ifnull(i.InvLote, '') = ifnull(t.Lote, '') else 1=1 end and " : "") : "") + " l.lipcodigo = (select LipCodigo from " + (parTipoRelacionClientes == 2 ? "ClientesDetalle" : "Clientes") + " Where CliID = '" + CliID + "' limit 1) " +
                  "and trim(upper(l.unmCodigo)) = trim(upper(t.UnmCodigo)) " +
                  "Group By p.ProID " +
                  " HAVING(((sum(i.invCantidad) * p.ProUnidades) + ifnull(i.InvCantidadDetalle, 0)) > ((ifnull(t.Cantidad, 0) * p.ProUnidades) + ifnull(t.CantidadDetalle, 0))or((sum(i.invCantidad) * p.ProUnidades) + ifnull(i.InvCantidadDetalle, 0)) < ((ifnull(t.Cantidad, 0) * p.ProUnidades) + ifnull(t.CantidadDetalle, 0))) " +
                  @" union 
	            select " + (almId != -1 ? " i.AlmID, " : "") + @" ifnull(p.ProDatos3, '') as ProDatos3, " + (parUmnCodigoByLista ? "l.UnmCodigo" : "p.UnmCodigo") + @" as UnmCodigo, i.ProID as ProID, p.ProCodigo as ProCodigo, p.ProDescripcion as Descripcion, sum(i.invCantidad) as InvCantidad, i.InvCantidadDetalle as InvCantidadDetalle, 
                l.lipprecio as Precio, p.proitbis as Itbis, p.prounidades as ProUnidades, 0 as Cantidad, sum(i.invCantidad) as CantidadManual, i.InvCantidadDetalle as CantidadManualDetalle, 
	            0 as CantidadDetalle, " + (parInventarioLote > 0 && almId != -1 ? "ifnull(i.InvLote, '')" : "''") + @" as Lote
                from " + (almId != -1 ? "InventariosAlmacenesRepresentantes" : "Inventarios") + @" i 
                inner join Productos p on p.proid = i.proid 
                left join listaprecios l on l.proid = i.proid 
                where " + (almId != -1 ? "i.AlmID = " + almId + " and " : "") + @" l.lipcodigo = (select LipCodigo from " + (parTipoRelacionClientes == 2 ? "ClientesDetalle" : "Clientes") + " Where CliID = '" + CliID + "' limit 1) " +
                  @" " + (parUmnCodigoByLista ? "" : "and trim(upper(l.unmCodigo)) = trim(upper(p.UnmCodigo))") + " and  i.proid not in (select proid from Productostemp where TitID = " + ((int)Modules.CONTEOSFISICOS).ToString() + " " + (parInventarioLote > 0 && almId != -1 ? " and case when ifnull(p.ProDatos3, '') like '%L%' then ifnull(Lote, '') = ifnull(i.invLote, '') else 1=1 end " : "") + " ) " +
                "Group By p.ProID " +
                "HAVING((((sum(i.invCantidad) * p.ProUnidades) + ifnull(i.InvCantidadDetalle, 0)) >  0) or (((sum(i.invCantidad) * p.ProUnidades) + ifnull(i.InvCantidadDetalle, 0)) < 0)) " +
                "union " +
                "select " + (almId != -1 ? " AlmID, " : "") + " ifnull(p.ProDatos3, '') as ProDatos3, " + (parUmnCodigoByLista ? "l.UnmCodigo" : "p.UnmCodigo") + " as UnmCodigo, t.ProID as ProID, p.ProCodigo as ProCodigo, p.ProDescripcion as Descripcion, 0 as InvCantidad, 0 as InvCantidadDetalle, " +
                  "l.lipprecio as Precio, p.proitbis as Itbis, p.prounidades as ProUnidades, /*ifnull(i.invCantidad, 0) - */ ifnull(t.Cantidad,0) as Cantidad, /*ifnull(i.invCantidad, 0) -*/ ifnull(t.Cantidad,0) as CantidadManual, " +
                  "0 as CantidadManualDetalle, 0 as CantidadDetalle, ifnull(t.Lote, '') as Lote " +
                  "from productostemp t " +
                  "inner join Productos p on p.proid = t.proid " +
                  "left join listaprecios l on l.proid = t.proid " +
                  "where t.proid not in (select proid from " + (almId != -1 ? "InventariosAlmacenesRepresentantes" : "Inventarios") + " ) " +
                  "and l.lipcodigo = (select LipCodigo from " + (parTipoRelacionClientes == 2 ? "ClientesDetalle" : "Clientes") + " Where CliID = '" + CliID + "' limit 1) " +
                  "and trim(upper(l.unmCodigo)) = trim(upper(t.UnmCodigo)) ";
            }

            if (proParaFacturar)
            {
                query = "select ifnull(p.ProDatos3, '') as ProDatos3, " + (parUmnCodigoByLista ? "l.UnmCodigo" : "p.UnmCodigo") + " as UnmCodigo, i.ProID as ProID, p.ProCodigo as ProCodigo, p.ProDescripcion as Descripcion, i.invCantidad as InvCantidad, i.InvCantidadDetalle as InvCantidadDetalle, " +
                "l.lipprecio as Precio, p.proitbis as Itbis, p.prounidades as ProUnidades, /*ifnull(i.invCantidad, 0) - */ ifnull(t.Cantidad,0) as Cantidad, /*ifnull(i.invCantidad, 0) -*/ ifnull(t.Cantidad,0) as CantidadManual, " +
                "0 as CantidadManualDetalle, 0 as CantidadDetalle, ifnull(t.Lote, '') as Lote " +
                "from " + (almId != -1 ? "InventariosAlmacenesRepresentantes" : "Inventarios") + " i " +
                "inner join Productos p on p.proid = i.proid " +
                "left join productostemp t on t.proid = i.proid and t.TitID = " + ((int)Modules.CONTEOSFISICOS).ToString() + " " +
                "left join listaprecios l on l.proid = i.proid " +
                "where " + (almId != -1 ? "i.AlmID = " + almId + " and " + (parInventarioLote > 0 ? "case when ifnull(p.ProDatos3, '') like '%L%' then ifnull(i.InvLote, '') = ifnull(t.Lote, '') else 1=1 end and " : "") : "") + " l.lipcodigo = (select LipCodigo from " + (parTipoRelacionClientes == 2 ? "ClientesDetalle" : "Clientes") + " Where CliID = '" + CliID + "' limit 1) " +
                "and trim(upper(l.unmCodigo)) = trim(upper(t.UnmCodigo)) and " +
                "((i.invCantidad * p.ProUnidades) + ifnull(i.InvCantidadDetalle,0)) > (((ifnull(t.Cantidad, 0) * p.ProUnidades) + ifnull(t.CantidadDetalle, 0)) +(select ((invCantidad * p.ProUnidades) + ifnull(InvCantidadDetalle,0)) " +
                "from InventariosAlmacenesRepresentantes where almid = " + myParametro.GetParAlmacenVentaRanchera() + " )) " +
                @" union 
	            select ifnull(p.ProDatos3, '') as ProDatos3, " + (parUmnCodigoByLista ? "l.UnmCodigo" : "p.UnmCodigo") + @" as UnmCodigo, i.ProID as ProID, p.ProCodigo as ProCodigo, p.ProDescripcion as Descripcion, i.invCantidad as InvCantidad, i.InvCantidadDetalle as InvCantidadDetalle, 
                l.lipprecio as Precio, p.proitbis as Itbis, p.prounidades as ProUnidades, 0 as Cantidad, i.invCantidad as CantidadManual, i.InvCantidadDetalle as CantidadManualDetalle, 
	            0 as CantidadDetalle, " + (parInventarioLote > 0 && almId != -1 ? "ifnull(i.InvLote, '')" : "''") + @" as Lote
                from " + (almId != -1 ? "InventariosAlmacenesRepresentantes" : "Inventarios") + @" i 
                inner join Productos p on p.proid = i.proid 
                left join listaprecios l on l.proid = i.proid 
                where " + (almId != -1 ? "i.AlmID = " + almId + " and " : "") + @" l.lipcodigo = (select LipCodigo from " + (parTipoRelacionClientes == 2 ? "ClientesDetalle" : "Clientes") + " Where CliID = '" + CliID + "' limit 1) " +
                @" " + (parUmnCodigoByLista ? "" : "and trim(upper(l.unmCodigo)) = trim(upper(p.UnmCodigo))") + " and ((i.invCantidad * p.ProUnidades) + ifnull(i.InvCantidadDetalle,0)) > 0  and i.proid not in (select proid from Productostemp where TitID = " + ((int)Modules.CONTEOSFISICOS).ToString() + " " + (parInventarioLote > 0 && almId != -1 ? " and case when ifnull(p.ProDatos3, '') like '%L%' then ifnull(Lote, '') = ifnull(i.invLote, '') else 1=1 end " : "") + " ) ";

            }

            return SqliteManager.GetInstance().Query<ProductosTemp>(query, new string[] { });
        }

        public List<ProductosTemp> GetProductosInInventarioConFaltantes(int CliID, int almId = -1, bool proParaFacturar = false, bool IsForValid = false)
        {
            var query = "";
            var parInventarioLote = myParametro.GetParConteosFisicosLotes();
            var parTipoRelacionClientes = myParametro.GetParTipoRelacionClientes();
            var parUmnCodigoByLista = myParametro.GetParConteoFisicoUsaUnidadMedidaByListaPrecio();

            if (IsForValid)
            {
                query = "select " + (almId != -1 ? " i.AlmID, " : "") + " ifnull(p.ProDatos3, '') as ProDatos3, " + (parUmnCodigoByLista ? "l.UnmCodigo" : "p.UnmCodigo") + " as UnmCodigo, i.ProID as ProID, p.ProCodigo as ProCodigo, p.ProDescripcion as Descripcion, i.invCantidad as InvCantidad, i.InvCantidadDetalle as InvCantidadDetalle, " +
                  "l.lipprecio as Precio, p.proitbis as Itbis, p.prounidades as ProUnidades, /*ifnull(i.invCantidad, 0) - */ ifnull(t.Cantidad,0) as Cantidad, /*ifnull(i.invCantidad, 0) -*/ ifnull(t.Cantidad,0) as CantidadManual, " +
                  "0 as CantidadManualDetalle, 0 as CantidadDetalle, ifnull(t.Lote, '') as Lote " +
                  "from " + (almId != -1 ? "InventariosAlmacenesRepresentantes" : "Inventarios") + " i " +
                  "inner join Productos p on p.proid = i.proid " +
                  "left join productostemp t on t.proid = i.proid and t.TitID = " + ((int)Modules.CONTEOSFISICOS).ToString() + " " +
                  "left join listaprecios l on l.proid = i.proid " +
                  "where " + (almId != -1 ? "i.AlmID = " + almId + " and " + (parInventarioLote > 0 ? "case when ifnull(p.ProDatos3, '') like '%L%' then ifnull(i.InvLote, '') = ifnull(t.Lote, '') else 1=1 end and " : "") : "") + " l.lipcodigo = (select LipCodigo from " + (parTipoRelacionClientes == 2 ? "ClientesDetalle" : "Clientes") + " Where CliID = '" + CliID + "' limit 1) " +
                  "and trim(upper(l.unmCodigo)) = trim(upper(t.UnmCodigo)) and " +
                  "(((i.invCantidad * p.ProUnidades) + ifnull(i.InvCantidadDetalle,0)) > ((ifnull(t.Cantidad, 0) * p.ProUnidades) + ifnull(t.CantidadDetalle, 0))) " +
                  @" union 
	            select " + (almId != -1 ? " i.AlmID, " : "") + @" ifnull(p.ProDatos3, '') as ProDatos3, " + (parUmnCodigoByLista ? "l.UnmCodigo" : "p.UnmCodigo") + @" as UnmCodigo, i.ProID as ProID, p.ProCodigo as ProCodigo, p.ProDescripcion as Descripcion, i.invCantidad as InvCantidad, i.InvCantidadDetalle as InvCantidadDetalle, 
                l.lipprecio as Precio, p.proitbis as Itbis, p.prounidades as ProUnidades, 0 as Cantidad, i.invCantidad as CantidadManual, i.InvCantidadDetalle as CantidadManualDetalle, 
	            0 as CantidadDetalle, " + (parInventarioLote > 0 && almId != -1 ? "ifnull(i.InvLote, '')" : "''") + @" as Lote
                from " + (almId != -1 ? "InventariosAlmacenesRepresentantes" : "Inventarios") + @" i 
                inner join Productos p on p.proid = i.proid 
                left join listaprecios l on l.proid = i.proid 
                where " + (almId != -1 ? "i.AlmID = " + almId + " and " : "") + @" l.lipcodigo = (select LipCodigo from " + (parTipoRelacionClientes == 2 ? "ClientesDetalle" : "Clientes") + " Where CliID = '" + CliID + "' limit 1) " +
                  @" " + (parUmnCodigoByLista ? "" : "and trim(upper(l.unmCodigo)) = trim(upper(p.UnmCodigo))") + " and (((i.invCantidad * p.ProUnidades) + ifnull(i.InvCantidadDetalle,0)) > 0)  and i.proid not in (select proid from Productostemp where TitID = " + ((int)Modules.CONTEOSFISICOS).ToString() + " " + (parInventarioLote > 0 && almId != -1 ? " and case when ifnull(p.ProDatos3, '') like '%L%' then ifnull(Lote, '') = ifnull(i.invLote, '') else 1=1 end " : "") + " ) ";
            }
            else
            {
                query = "select " + (almId != -1 ? " i.AlmID, " : "") + " ifnull(p.ProDatos3, '') as ProDatos3, " + (parUmnCodigoByLista ? "l.UnmCodigo" : "p.UnmCodigo") + " as UnmCodigo, i.ProID as ProID, p.ProCodigo as ProCodigo, p.ProDescripcion as Descripcion, i.invCantidad as InvCantidad, i.InvCantidadDetalle as InvCantidadDetalle, " +
                  "l.lipprecio as Precio, p.proitbis as Itbis, p.prounidades as ProUnidades, /*ifnull(i.invCantidad, 0) - */ ifnull(t.Cantidad,0) as Cantidad, /*ifnull(i.invCantidad, 0) -*/ ifnull(t.Cantidad,0) as CantidadManual, " +
                  "0 as CantidadManualDetalle, 0 as CantidadDetalle, ifnull(t.Lote, '') as Lote " +
                  "from " + (almId != -1 ? "InventariosAlmacenesRepresentantes" : "Inventarios") + " i " +
                  "inner join Productos p on p.proid = i.proid " +
                  "left join productostemp t on t.proid = i.proid and t.TitID = " + ((int)Modules.CONTEOSFISICOS).ToString() + " " +
                  "left join listaprecios l on l.proid = i.proid " +
                  "where " + (almId != -1 ? "i.AlmID = " + almId + " and " + (parInventarioLote > 0 ? "case when ifnull(p.ProDatos3, '') like '%L%' then ifnull(i.InvLote, '') = ifnull(t.Lote, '') else 1=1 end and " : "") : "") + " l.lipcodigo = (select LipCodigo from " + (parTipoRelacionClientes == 2 ? "ClientesDetalle" : "Clientes") + " Where CliID = '" + CliID + "' limit 1) " +
                  "and trim(upper(l.unmCodigo)) = trim(upper(t.UnmCodigo)) and " +
                  "(((i.invCantidad * p.ProUnidades) + ifnull(i.InvCantidadDetalle,0)) > ((ifnull(t.Cantidad, 0) * p.ProUnidades) + ifnull(t.CantidadDetalle, 0)) or " +
                  "((i.invCantidad * p.ProUnidades) + ifnull(i.InvCantidadDetalle, 0)) < ((ifnull(t.Cantidad, 0) * p.ProUnidades) + ifnull(t.CantidadDetalle, 0))) " +
                  @" union 
	            select " + (almId != -1 ? " i.AlmID, " : "")  + @" ifnull(p.ProDatos3, '') as ProDatos3, " + (parUmnCodigoByLista ? "l.UnmCodigo" : "p.UnmCodigo") + @" as UnmCodigo, i.ProID as ProID, p.ProCodigo as ProCodigo, p.ProDescripcion as Descripcion, i.invCantidad as InvCantidad, i.InvCantidadDetalle as InvCantidadDetalle, 
                l.lipprecio as Precio, p.proitbis as Itbis, p.prounidades as ProUnidades, 0 as Cantidad, i.invCantidad as CantidadManual, i.InvCantidadDetalle as CantidadManualDetalle, 
	            0 as CantidadDetalle, " + (parInventarioLote > 0 && almId != -1 ? "ifnull(i.InvLote, '')" : "''") + @" as Lote
                from " + (almId != -1 ? "InventariosAlmacenesRepresentantes" : "Inventarios") + @" i 
                inner join Productos p on p.proid = i.proid 
                left join listaprecios l on l.proid = i.proid 
                where " + (almId != -1 ? "i.AlmID = " + almId + " and " : "") + @" l.lipcodigo = (select LipCodigo from " + (parTipoRelacionClientes == 2 ? "ClientesDetalle" : "Clientes") + " Where CliID = '" + CliID + "' limit 1) " +
                  @" " + (parUmnCodigoByLista ? "" : "and trim(upper(l.unmCodigo)) = trim(upper(p.UnmCodigo))") + " and ((((i.invCantidad * p.ProUnidades) + ifnull(i.InvCantidadDetalle,0)) > 0) or (((i.invCantidad * p.ProUnidades) + ifnull(i.InvCantidadDetalle,0)) < 0))  and i.proid not in (select proid from Productostemp where TitID = " + ((int)Modules.CONTEOSFISICOS).ToString() + " " + (parInventarioLote > 0 && almId != -1 ? " and case when ifnull(p.ProDatos3, '') like '%L%' then ifnull(Lote, '') = ifnull(i.invLote, '') else 1=1 end " : "") + " ) " +
                "union "+
                "select " + (almId != -1 ? " AlmID, " : "") + " ifnull(p.ProDatos3, '') as ProDatos3, " + (parUmnCodigoByLista ? "l.UnmCodigo" : "p.UnmCodigo") + @" as UnmCodigo, t.ProID as ProID, p.ProCodigo as ProCodigo, p.ProDescripcion as Descripcion, 0 as InvCantidad, 0 as InvCantidadDetalle, " +
                  "l.lipprecio as Precio, p.proitbis as Itbis, p.prounidades as ProUnidades, /*ifnull(i.invCantidad, 0) - */ ifnull(t.Cantidad,0) as Cantidad, /*ifnull(i.invCantidad, 0) -*/ ifnull(t.Cantidad,0) as CantidadManual, " +
                  "0 as CantidadManualDetalle, 0 as CantidadDetalle, ifnull(t.Lote, '') as Lote " +
                  "from productostemp t " +
                  "inner join Productos p on p.proid = t.proid " +                  
                  "left join listaprecios l on l.proid = t.proid " +
                  "where t.proid not in (select proid from " + (almId != -1 ? "InventariosAlmacenesRepresentantes" : "Inventarios") + " ) " +
                  "and l.lipcodigo = (select LipCodigo from " + (parTipoRelacionClientes == 2 ? "ClientesDetalle" : "Clientes") + " Where CliID = '" + CliID + "' limit 1) " +
                  "and trim(upper(l.unmCodigo)) = trim(upper(t.UnmCodigo)) ";
            }

            if (proParaFacturar)
            {
                query = "select ifnull(p.ProDatos3, '') as ProDatos3, " + (parUmnCodigoByLista ? "l.UnmCodigo" : "p.UnmCodigo") + " as UnmCodigo, i.ProID as ProID, p.ProCodigo as ProCodigo, p.ProDescripcion as Descripcion, i.invCantidad as InvCantidad, i.InvCantidadDetalle as InvCantidadDetalle, " +
                "l.lipprecio as Precio, p.proitbis as Itbis, p.prounidades as ProUnidades, /*ifnull(i.invCantidad, 0) - */ ifnull(t.Cantidad,0) as Cantidad, /*ifnull(i.invCantidad, 0) -*/ ifnull(t.Cantidad,0) as CantidadManual, " +
                "0 as CantidadManualDetalle, 0 as CantidadDetalle, ifnull(t.Lote, '') as Lote " +
                "from " + (almId != -1 ? "InventariosAlmacenesRepresentantes" : "Inventarios") + " i " +
                "inner join Productos p on p.proid = i.proid " +
                "left join productostemp t on t.proid = i.proid and t.TitID = " + ((int)Modules.CONTEOSFISICOS).ToString() + " " +
                "left join listaprecios l on l.proid = i.proid " +
                "where " + (almId != -1 ? "i.AlmID = " + almId + " and " + (parInventarioLote > 0 ? "case when ifnull(p.ProDatos3, '') like '%L%' then ifnull(i.InvLote, '') = ifnull(t.Lote, '') else 1=1 end and " : "") : "") + " l.lipcodigo = (select LipCodigo from " + (parTipoRelacionClientes == 2 ? "ClientesDetalle" : "Clientes") + " Where CliID = '" + CliID + "' limit 1) " +
                "and trim(upper(l.unmCodigo)) = trim(upper(t.UnmCodigo)) and " +
                "((i.invCantidad * p.ProUnidades) + ifnull(i.InvCantidadDetalle,0)) > (((ifnull(t.Cantidad, 0) * p.ProUnidades) + ifnull(t.CantidadDetalle, 0)) +(select ((invCantidad * p.ProUnidades) + ifnull(InvCantidadDetalle,0)) "+
                "from InventariosAlmacenesRepresentantes where almid = "+ myParametro.GetParAlmacenVentaRanchera() + " )) " +
                @" union 
	            select ifnull(p.ProDatos3, '') as ProDatos3, " + (parUmnCodigoByLista ? "l.UnmCodigo" : "p.UnmCodigo") + @" as UnmCodigo, i.ProID as ProID, p.ProCodigo as ProCodigo, p.ProDescripcion as Descripcion, i.invCantidad as InvCantidad, i.InvCantidadDetalle as InvCantidadDetalle, 
                l.lipprecio as Precio, p.proitbis as Itbis, p.prounidades as ProUnidades, 0 as Cantidad, i.invCantidad as CantidadManual, i.InvCantidadDetalle as CantidadManualDetalle, 
	            0 as CantidadDetalle, " + (parInventarioLote > 0 && almId != -1 ? "ifnull(i.InvLote, '')" : "''") + @" as Lote
                from " + (almId != -1 ? "InventariosAlmacenesRepresentantes" : "Inventarios") + @" i 
                inner join Productos p on p.proid = i.proid 
                left join listaprecios l on l.proid = i.proid 
                where " + (almId != -1 ? "i.AlmID = " + almId + " and " : "") + @" l.lipcodigo = (select LipCodigo from " + (parTipoRelacionClientes == 2 ? "ClientesDetalle" : "Clientes") + " Where CliID = '" + CliID + "' limit 1) " +
                @" " + (parUmnCodigoByLista ? "" : "and trim(upper(l.unmCodigo)) = trim(upper(p.UnmCodigo))") + " and ((i.invCantidad * p.ProUnidades) + ifnull(i.InvCantidadDetalle,0)) > 0  and i.proid not in (select proid from Productostemp where TitID = " + ((int)Modules.CONTEOSFISICOS).ToString() + " " + (parInventarioLote > 0 && almId != -1 ? " and case when ifnull(p.ProDatos3, '') like '%L%' then ifnull(Lote, '') = ifnull(i.invLote, '') else 1=1 end " : "") + " ) ";

            }

            return SqliteManager.GetInstance().Query<ProductosTemp>(query, new string[] { });
        }

        public bool IsConteoCuadrado(int almId)
        {
            var sql = $@"select t.ProID, * from ( 
                      select ProID, sum(t.Cantidad)as Cantidad, sum(t.CantidadDetalle) as CantidadDetalle, sum(invCantidad) as invCantidad, sum(InvCantidadDetalle) as InvCantidadDetalle  
                      from(  
                      select t.ProID as ProID, ifnull(t.Cantidad, 0) as Cantidad, ifnull(t.CantidadDetalle, 0) as CantidadDetalle, 0 as invCantidad, 0 as InvCantidadDetalle  
                      from ProductosTemp t  
                      where t.TitID =  '{(int)Modules.CONTEOSFISICOS}'
                      union  
                      select i.ProID as ProID, 0 as Cantidad, 0 as CantidadDetalle, ifnull(i.invCantidad, 0) as invCantidad,  
                      ifnull(i.InvCantidadDetalle, 0) as InvCantidadDetalle from {(almId == -1? "Inventarios" : "InventariosAlmacenesRepresentantes")} i) t  
                      group by t.ProID  
                      ) t  
                      group by t.ProID having(t.Cantidad <> t.invCantidad) || (t.CantidadDetalle <> t.InvCantidadDetalle)";

            var list = SqliteManager.GetInstance().Query<ProductosTemp>(sql, new string[] { });

            return list == null || list.Count == 0;
        }

        public ConteosFisicos GetConteoBySecuencia(int conSecuencia)
        {
            var list = SqliteManager.GetInstance().Query<ConteosFisicos>("select e.EstDescripcion as EstatusDescripcion, ConSecuencia, ifnull(ConFecha, '') as ConFecha, CuaSecuencia, ConEstatus, ConEstatusConteo, ifnull(r.RepNombre, '') as RepAuditor, c.AlmID as AlmID " +
                "from ConteosFisicos c " +
                "left join Estados e on trim(upper(e.EstTabla)) = upper('ConteosFisicos') and e.EstEstado = c.ConEstatus " +
                "left join Representantes r on r.RepCodigo = c.RepAuditor and trim(upper(r.RepCargo)) = 'AUDITOR'  " +
                "where ConSecuencia = ? and trim(c.RepCodigo) = ? ", new string[] { conSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public List<ConteosFisicosDetalle> GetDetalleConteoBySecuencia(int conSecuencia, string LipCodigo = "")
        {
            var sql = "select c.ProID as ProID, p.ProCodigo as ProCodigo, ConCantidadLogica, ConCantidadDetalleLogica, ConCantidad, ConCantidadDetalle, p.ProDescripcion as ProDescripcion, p.ProUnidades as ProUnidades, " + (!string.IsNullOrEmpty(LipCodigo) ? " l.lipprecio as Precio, " : "P.ProPrecio as Precio, ") + " p.ProItbis as Itbis " +
                "from ConteosFisicosDetalle c " +
                "inner join Productos p on p.ProID = c.ProID " +
                (!string.IsNullOrEmpty(LipCodigo) ? "inner join ListaPrecios l on l.proid = c.proid and l.lipcodigo =  '" + LipCodigo + "' " : "") +
                "where c.ConSecuencia = ? and trim(c.RepCodigo) = ? " +
                "group by p.proid " +
                "Order by p.LinID, p.Cat1ID, p.ProID ";

            return SqliteManager.GetInstance().Query<ConteosFisicosDetalle>(sql, new string[] { conSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public List<ConteosFisicosDetalle> GetDetalleConteoBySecuenciaConLotesAgrupados(int conSecuencia, string LipCodigo = "")
        {
            var sql = "select c.ProID as ProID, p.ProCodigo as ProCodigo, sum(ConCantidadLogica) as ConCantidadLogica, ConCantidadDetalleLogica, sum(ConCantidad) as ConCantidad, ConCantidadDetalle, p.ProDescripcion as ProDescripcion, p.ProUnidades as ProUnidades, " + (!string.IsNullOrEmpty(LipCodigo) ? " l.lipprecio as Precio, " : "P.ProPrecio as Precio, ") + " p.ProItbis as Itbis " +
                "from ConteosFisicosDetalle c " +
                "inner join Productos p on p.ProID = c.ProID " +
                (!string.IsNullOrEmpty(LipCodigo) ? "inner join ListaPrecios l on l.proid = c.proid and l.lipcodigo =  '" + LipCodigo + "' " : "") +
                "where c.ConSecuencia = ? and trim(c.RepCodigo) = ? " +
                "group by p.proid " +
                "Order by p.LinID, p.Cat1ID, p.ProID ";

            return SqliteManager.GetInstance().Query<ConteosFisicosDetalle>(sql, new string[] { conSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public List<ConteosFisicosDetalle> GetProductosSobrantes(int ConSecuencia, bool Multialmacen = false, int almid = -1)
        {
            var sql = "select t.ConCantidadDetalleLogica, t.ConCantidadLogica, t.ProID as ProID, t.ConCantidad as ConCantidad, t.ConCantidadDetalle as ConCantidadDetalle, p.ProDescripcion as ProDescripcion,  p.ProCodigo as ProCodigo, p.ProUnidades as ProUnidades " +
              "from ConteosFisicosDetalle t " +
              "inner join Productos p on p.Proid = t.Proid " +
              "where ConSecuencia = " + ConSecuencia + " and (t.ConCantidadLogica < t.ConCantidad or t.ConCantidadDetalleLogica < t.ConCantidadDetalle) and ((t.proid not in (select proid from " + (Multialmacen ? " InventariosAlmacenesRepresentantes where almid = 20 " : " inventarios ") + " ) ) " +
              "or(t.proid in (select proid from " + (Multialmacen ? " InventariosAlmacenesRepresentantes " : " inventarios ") + " where  " + (Multialmacen ? " almid = " + almid + " " : "") + " " +
              " " + (Multialmacen ? " and " : " ") + " (t.ConCantidad > 0 or t.ConCantidadDetalle > 0)))) Order by p.LinID, p.Cat1ID, p.ProID";
            return SqliteManager.GetInstance().Query<ConteosFisicosDetalle>(sql, new string[] { });

        }

        public List<ConteosFisicosDetalle> GetProductosSobrantesConLotesAgrupados(int ConSecuencia, bool Multialmacen = false, int almid = -1)
        {
            var sql = "select sum(t.ConCantidadLogica) as ConCantidadLogica, t.ProID as ProID, sum(t.ConCantidad)  as ConCantidad, sum( t.ConCantidadDetalle) as ConCantidadDetalle, p.ProDescripcion as ProDescripcion,  p.ProCodigo as ProCodigo, p.ProUnidades as ProUnidades " +
              "from ConteosFisicosDetalle t " +
              "inner join Productos p on p.Proid = t.Proid " +
              "where ConSecuencia = " + ConSecuencia + "  and ((t.proid not in (select proid from " + (Multialmacen ? " InventariosAlmacenesRepresentantes where almid = 20 " : " inventarios ") + " ) ) " +
              "or(t.proid in (select proid from " + (Multialmacen ? " InventariosAlmacenesRepresentantes " : " inventarios ") + " where  " + (Multialmacen ? " almid = " + almid + " " : "") + " " +
              " " + (Multialmacen ? " and " : " ") + " (t.ConCantidad > 0 or t.ConCantidadDetalle > 0)))) " +
              "Group By p.ProID " +
              "HAVING(sum(t.ConCantidadLogica) < sum(t.ConCantidad) ) ";
             
            return SqliteManager.GetInstance().Query<ConteosFisicosDetalle>(sql, new string[] { });

        }

        public bool CantidadesEnCero()
        {
            var list = SqliteManager.GetInstance().Query<ProductosTemp>("select t.Cantidad as Cantidad, t.CantidadDetalle as CantidadDetalle " +
              "from ProductosTemp t " +
              "where "+
              " t.Cantidad > 0 or t.CantidadDetalle > 0 ", new string[] { });

            if (list.FirstOrDefault().Cantidad == 0 && list.FirstOrDefault().CantidadDetalle == 0)
            {
                return true;
            }

            return false;
        }

        public bool ProductosEnCero()
        {
            var list = SqliteManager.GetInstance().Query<ProductosTemp>("select t.Cantidad as Cantidad, t.CantidadDetalle as CantidadDetalle " +
              "from ProductosTemp t " +
              "where " +
              " t.Cantidad > 0 or t.CantidadDetalle > 0 ", new string[] { });

            if (list.Count() == 0)
            {
                return true;
            }

            return false;
        }
        

        public string ProductosEnInventarioSinListaDePrecios()
        {
            var list = SqliteManager.GetInstance().Query<ProductosTemp>($@"
                                        select ProCodigo from InventariosAlmacenesRepresentantes i inner join Productos p on p.proid = i.proid where 
                                        i.proid not in (SELECT i.Proid from InventariosAlmacenesRepresentantes i inner join ListaPrecios
                                        l on l.Proid = i.Proid inner join clientes c on c.LipCodigo = l.LipCodigo where 
                                        cliid = '{myParametro.GetParClienteForRepresentantes()}')", new string[] { });
            string proids = "";
            foreach(var productos in list) 
            {
                proids += productos.ProCodigo + ", ";
            }

            return proids;
        }

        public bool ValidateAlmacenesConConteoFisico(/*List<int>*/ string almacenes, int Cuasecuencia)
        {
            var validate = true;
            var almacenesID = almacenes.Split(',');

            if (string.IsNullOrWhiteSpace(almacenes))
            {
                return false;
            }

            foreach (var almid in almacenesID)
            {
                //var list = SqliteManager.GetInstance().Query<ProductosTemp>("select 1 from ConteosFisicos where almid = ? and cuaSecuencia = ? ", new string[] { almid.ToString(), Cuasecuencia.ToString() });
                var list = SqliteManager.GetInstance().Query<ProductosTemp>("select 1 from ConteosFisicos where almid = "+ almid + " and cuaSecuencia = ? ", new string[] { Cuasecuencia.ToString() });
                if (list == null || list.Count == 0)
                {
                    validate = false;
                }
               
            }

            return validate;
        }

        public bool ValidateSiAlmaceneTieneConConteoFisico(int almID, int Cuasecuencia)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<ProductosTemp>("select 1 from ConteosFisicos where almid = ? and cuaSecuencia = ? ", new string[] { almID.ToString(), Cuasecuencia.ToString() });
                if (list != null && list.Count > 0)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                e.GetBaseException();
            }

            return false;
        }

        public int ValidateSiTodosAlmacenesTienenConConteoFisico(string almacenesID, int Cuasecuencia)
        {
            var count = 0;
            try
            {
                
                var almID = almacenesID.Split(',');
                foreach(var almid in almID) {
                    var list = SqliteManager.GetInstance().Query<ProductosTemp>("select 1 " +
                        "from ConteosFisicos where ( almid in ( " + almid + " ) or almid is null ) and cuaSecuencia = ? " +
                        "UNION ALL " +
                        "select 1 from ConteosFisicosConfirmados where ( almid in ( " + almid + " ) or almid is null ) and cuaSecuencia = ? ", new string[] { Cuasecuencia.ToString(), Cuasecuencia.ToString() });
                    if (list != null && list.Count > 0)
                    {
                        count += list.Count;
                    }
                }
            }
            catch (Exception e)
            {
                e.GetBaseException();
            }

            return count;
        }
        public void InsertarConteoInTemp(int ConSecuencia, bool confirmado)
        {
            myProd.ClearTemp((int)Modules.CONTEOSFISICOS);

            string query = $@"SELECT distinct {(int)Modules.CONTEOSFISICOS} as TitID, cf.ConCantidad as ConCantidad, cf.ConCantidadDetalle as ConCantidadDetalle, cf.rowguid as rowguid, cf.Proid as Proid, 
                cf.ConPrecioVenta as Precio, p.ProDescripcion as Descripcion from {(confirmado ? "ConteosFisicosDetalleConfirmados" : "ConteosFisicosDetalle")} cf inner join Productos p on p.ProID = cf.ProID where 
                ltrim(rtrim(cf.RepCodigo)) = ? and cf.ConSecuencia = ? order by p.ProDescripcion";

            var list = SqliteManager.GetInstance().Query<ProductosTemp>(query, new string[] { Arguments.CurrentUser.RepCodigo.Trim(), ConSecuencia.ToString() });

            SqliteManager.GetInstance().InsertAll(list);

        }
        public void EstConteo(string rowguid, int est)
        {
            Hash ped = new Hash("ConteosFisicos");
            ped.Add("ConEstatus", est);
            ped.Add("ConFechaActualizacion", Functions.CurrentDate());
            ped.Add("UsuInicioSesion", "mdsoft");

            if (est == 0)
            {
                if (new DS_SuscriptoresCambios().UpdateCambioEstadoInsertByRowguid(rowguid, est))
                {
                    ped.SaveScriptForServer = false;
                }
            }

            ped.ExecuteUpdate("rowguid = '" + rowguid + "'");
        }

        private List<ProductosTemp> GetProductosInInventarioFaltantesAgrupados(int almId = -1)
        {
            var sql = "select i.ProID as ProID, sum(i.invCantidad) as InvCantidad, sum(i.InvCantidadDetalle) as InvCantidadDetalle, p.Proprecio as Precio, p.proitbis as Itbis, p.prounidades as ProUnidades, p.ProDatos3 as ProDatos3 " +
                "from InventariosAlmacenesRepresentantes i " +
                "left join productos p on p.proid = i.proid " +
                "where i.AlmID = " + almId + " and  i.proid not in (select proid from ProductosTemp where  TitID = " + ((int)Modules.CONTEOSFISICOS).ToString() + ") " +
                "Group By i.ProID " +
                "HAVING(sum(i.invCantidad) >0 or sum(i.invCantidadDetalle) > 0)  ";


            return SqliteManager.GetInstance().Query<ProductosTemp>(sql, new string[] { });
        }

        private List<ProductosTemp> GetProductosInTempWithNoLote(int almId = -1)
        {
            var join = "left join Inventarios i on i.ProID = t.ProID ";

            if (almId != -1)
            {
                join = "left join InventariosAlmacenesRepresentantes i on i.ProID = t.ProID and i.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' and i.AlmID = " + almId + " ";
            }

            return SqliteManager.GetInstance().Query<ProductosTemp>("select t.ProID as ProID, t.Lote as Lote, t.ProDatos3 as ProDatos3, t.Cantidad as Cantidad, t.CantidadDetalle as CantidadDetalle, " +
                "sum(i.invCantidad) as InvCantidad, i.InvCantidadDetalle as InvCantidadDetalle, t.ProUnidades as ProUnidades " +
                "from ProductosTemp t " +
                join +
                "where t.TitID = ? Group By t.ProID ", new string[] { ((int)Modules.CONTEOSFISICOS).ToString() });
        }

    }
}
