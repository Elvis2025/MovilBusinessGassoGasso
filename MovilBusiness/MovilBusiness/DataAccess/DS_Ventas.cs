using MovilBusiness.Configuration;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace MovilBusiness.DataAccess
{
    public class DS_Ventas : DS_Controller
    {
        private DS_Productos myProd;
        private DS_CuentasxCobrar myCxCobrar;
        private DS_EntregasRepartidorTransacciones myEnt;

        public DS_Ventas(DS_Productos myProd = null)
        {
            this.myProd = myProd;

            if (this.myProd == null)
            {
                this.myProd = new DS_Productos();
            }
            myCxCobrar = new DS_CuentasxCobrar();
        }

        public int SaveVenta(int conId, NCF ncf,  out bool saveReciboFromVenta, EntregasRepartidorTransacciones Entrega, int VenCantidadCanastos, int MotID = -1, double montototal = 0.00, double subtotal = 0.00, double itbis = 0.00, double totalDescuentoGlobal = 0.00, double porDescuentoGlobal = 0.00, string venOrdenCompra="",bool fromcopy = false)
        {
            saveReciboFromVenta = false;

            var productos = new DS_Productos().GetResumenProductos((int)Modules.VENTAS, false, entrega: Entrega, isForGuardar: true);
            productos = productos.OrderBy(c => c.ProPosicion).ToList();
            var almIdRanchero = myParametro.GetParAlmacenVentaRanchera();

            if (myParametro.GetParCuadres() > 0 && Arguments.Values.CurrentCuaSecuencia < 1)
            {
                throw new Exception("No se pudo guardar la venta porque no se encontro la secuencia del cuadre");
            }

            if(ncf == null)
            {
                throw new Exception("No se cargo la secuencia de NCF");
            }

            if(productos == null || productos.Count == 0)
            {
                throw new Exception("No hay productos para guardar");
            }

            if (!string.IsNullOrWhiteSpace(ncf.FechaVencimiento))
            {
                ncf.FechaVencimiento = ncf.FechaVencimiento.Replace("T", " ");
            }

            int venSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Ventas");

            if (!fromcopy)
            {
                new DS_Visitas().ActualizarVisitaEfectiva(Arguments.Values.CurrentVisSecuencia);
            }

            Hash ven = new Hash("Ventas");
            ven.Add("VenEstatus", 1);
            ven.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            ven.Add("VenSecuencia", venSecuencia);
            ven.Add("CliID", Arguments.Values.CurrentClient.CliID);
            ven.Add("VisSecuencia", Arguments.Values.CurrentVisSecuencia);
            ven.Add("VenFecha", Functions.CurrentDate());           
            ven.Add("VenTotal", productos.Count);
            ven.Add("mbVersion", Functions.AppVersion);
            ven.Add("ConID", conId);
            ven.Add("VenNCF", (Arguments.Values.CurrentClient.CliTipoComprobanteFAC == "99" ? venSecuencia + "-" + Arguments.CurrentUser.RepCodigo : ncf.NCFCompleto));
            ven.Add("VenNCFFechaVencimiento", ncf.FechaVencimiento);
            ven.Add("RutID", Arguments.CurrentUser.RutID);
            ven.Add("VenReferencia", ""); //par rojo gas
            ven.Add("CuaSecuencia", Arguments.Values.CurrentCuaSecuencia);
            //ven.Add("VenCantidadCanastos", 0);//par rojo gas
            ven.Add("VenCantidadImpresion", 0);
            ven.Add("MonCodigo", Arguments.Values.CurrentClient.MonCodigo);
            ven.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            ven.Add("VenFechaOrden", Functions.CurrentDate());
            ven.Add("VenFechaActualizacion", Functions.CurrentDate());
            string venrowguid = Guid.NewGuid().ToString();
            ven.Add("rowguid", venrowguid);
            ven.Add("VenCantidadCanastos", VenCantidadCanastos);

            ven.Add("VenMontoSinITBIS", montototal - itbis);
            ven.Add("VenMontoITBIS", itbis);
            ven.Add("VenMontoTotal", montototal);
            ven.Add("VenSubTotal", subtotal);
            ven.Add("VenPorCientoDsctoGlobal", porDescuentoGlobal);
            ven.Add("VenMontoDsctoGlobal", totalDescuentoGlobal);

            var parCalculoPagoMinimo = myParametro.GetParVentasPorcientoBalancePagoMinimo();

            if(parCalculoPagoMinimo > 0)
            {
                var datos = new VenDatosOtros();
                datos.VENPAGOMIN = parCalculoPagoMinimo;

                var parAdicionalPedido = myParametro.GetParVentasPorcientoAdicionalPedido();
                var parAdicionalLimiteCredito = myParametro.GetParVentasPorcientoAdicionalLimiteCredito();

                if(parAdicionalPedido > 0)
                {
                    datos.VENPEDADIC = parAdicionalPedido;
                }

                if(parAdicionalLimiteCredito > 0)
                {
                    datos.VENLCADIC = parAdicionalLimiteCredito;
                }

                ven.Add("VenOtrosDatos", JsonConvert.SerializeObject(datos));
            }

            if(Arguments.Values.CurrentSector != null)
            {
                ven.Add("SecCodigo", Arguments.Values.CurrentSector.SecCodigo);
            }

            if (Entrega != null)
            {
                ven.Add("PedSecuencia", Entrega.TraSecuencia);
                ven.Add("RepVendedor", Entrega.RepVendedor);
            }

            if(venOrdenCompra != "")
            {
                ven.Add("VenOrdenCompra", venOrdenCompra);
            }

            ven.ExecuteInsert();

            int pos = 1;
            var parDecimales = myParametro.GetParPermitirDecimales();
            var parDecimalesProDatos3 = myParametro.GetParProdustosDecimalValidadosDeProDatos3();

            var inv = new DS_Inventarios();

            var parMultiAlmacenes = myParametro.GetParUsarMultiAlmacenes();
            var almIdDespacho = myParametro.GetParAlmacenIdParaDespacho();
            var almIdDevolucion = myParametro.GetParAlmacenIdParaDevolucion();
            var parVentasLote = myParametro.GetParVentasLote();
            var parEditarPrecio = myParametro.GetParVentasEditarPrecio();


            foreach (var pro in productos)
            {
                Hash det = new Hash("VentasDetalle");
                det.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                det.Add("VenSecuencia", venSecuencia);
                det.Add("VenPosicion", pos); pos++;
                det.Add("ProID", pro.ProID);
                det.Add("VenCantidadPiezas", pro.CantidadPiezas);

                if (parDecimalesProDatos3 && pro.ProDatos3.Equals("A"))
                {
                    det.Add("VenCantidad", pro.Cantidad);
                    det.Add("VenCantidadDetalle", pro.CantidadDetalle);
                }
                else if (parDecimales)
                {
                    det.Add("VenCantidad", pro.Cantidad);
                    det.Add("VenCantidadDetalle", pro.CantidadDetalle);
                }
                else
                {
                    det.Add("VenCantidad", (int)pro.Cantidad);
                    det.Add("VenCantidadDetalle", (int)pro.CantidadDetalle);
                }

                if (parVentasLote == 1 || parVentasLote == 2)
                {
                    det.Add("VenLote", pro.Lote); 
                }

                //par rojo gas
                /*det.Add("VenContadorInicial", ?);
                det.Add("VenContadorFinal", ?);
                det.Add("VenCantidad", ?);*/

                if (parEditarPrecio && pro.PrecioTemp > 0)
                {
                    det.Add("VenPrecio", pro.PrecioTemp);
                }
                else
                {
                    det.Add("VenPrecio", pro.Precio);
                }
                det.Add("VenItbis", pro.Itbis);
                det.Add("VenSelectivo", (pro.IndicadorOferta ? 0 : pro.Selectivo));
                det.Add("VenAdValorem", (pro.IndicadorOferta ? 0 : pro.AdValorem));
                det.Add("VenDescuento", pro.Descuento);
                det.Add("UnmCodigo", pro.UnmCodigo);
                //det.Add("VenTotalItbis", (pro.Precio * (pro.Itbis / 100)) * pro.Cantidad);

                double totalitbis = pro.IndicadorOferta ? 0 :
                    Math.Round((pro.Precio + pro.Selectivo + pro.AdValorem - pro.Descuento) *
                    (pro.CantidadDetalle > 0 ? pro.Cantidad + (double.Parse(pro.CantidadDetalle.ToString())
                    / double.Parse(pro.ProUnidades.ToString())) : 0 / pro.ProUnidades > 0 ? pro.ProUnidades :
                    0 + pro.Cantidad) * (pro.Itbis / 100), 2);
                
                double DecuentoTotal = 0.0;
                double PrecioTotal = 0.0;
                double Descuentos;
                double AdValorem = pro.AdValorem;
                double Selectivo = pro.Selectivo;
                double PrecioLista = (pro.IndicadorOferta ? 0.0 : pro.Precio + AdValorem + Selectivo);
                PrecioLista = Math.Round(PrecioLista, 2, MidpointRounding.AwayFromZero);

                double CantidadDetalle = Convert.ToDouble(Convert.ToDecimal(pro.CantidadDetalle));
                CantidadDetalle = Math.Round(CantidadDetalle, 2, MidpointRounding.AwayFromZero);
                double ProUnidades = Convert.ToDouble(Convert.ToDecimal(pro.ProUnidades));
                double CantidadUnica = pro.Cantidad;
                double CantidadReal = (CantidadDetalle / ProUnidades) + CantidadUnica;
                if (!myParametro.GetParCantidadRealSinRedondeo())
                {
                    CantidadReal = Math.Round(CantidadReal, 2, MidpointRounding.AwayFromZero);
                }

                PrecioTotal += (pro.IndicadorOferta ? 0.0 : PrecioLista * CantidadReal);
                PrecioTotal = Math.Round(PrecioTotal, 2, MidpointRounding.AwayFromZero);

                Descuentos = (pro.DesPorciento / 100) * pro.Precio;
                Descuentos = Math.Round(Descuentos, 2, MidpointRounding.AwayFromZero);

                if (Descuentos == 0.0)
                {
                    Descuentos = pro.Descuento;
                }

                double descTotalUnitario = (pro.IndicadorOferta ? 0.0 : Descuentos * CantidadReal);
                descTotalUnitario = Math.Round(descTotalUnitario, 2, MidpointRounding.AwayFromZero);

                DecuentoTotal += (pro.IndicadorOferta ? 0.0 : descTotalUnitario);
                DecuentoTotal = Math.Round(DecuentoTotal, 2, MidpointRounding.AwayFromZero);

                det.Add("VenTotalItbis", /*Functions.RoundTwoPositions(*/totalitbis);
                det.Add("VenTotalDescuento", DecuentoTotal);
                det.Add("VenindicadorOferta", pro.IndicadorOferta);
                det.Add("OfeID", pro.OfeID);
                det.Add("VenDescPorciento", pro.DesPorciento);
                det.Add("VenCantidadEntregada", 0);
                det.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                det.Add("VenFechaActualizacion", Functions.CurrentDate());
                det.Add("VenPrecioLista", pro.PrecioMoneda == 0 ? pro.Precio : pro.PrecioMoneda);
                det.Add("VenCantidadOferta", pro.CantidadOferta);
                det.Add("VenDesPorcientoOriginal", pro.DesPorcientoManual == 0 ? pro.DesPorciento : pro.DesPorcientoManual);
                det.Add("rowguid", Guid.NewGuid().ToString());
                det.ExecuteInsert();

                if (!myParametro.GetParVentasNoRebajaInventario())
                {
                    if (!string.IsNullOrWhiteSpace(pro.ProDatos3) && pro.ProDatos3.Contains("x"))
                    {
                        var productosCombo = new DS_ProductosCombos().GetProductosCombo(pro.ProID);

                        foreach (var proCombo in productosCombo)
                        {
                            var cantidadSolicitadaTotal = ((pro.CantidadDetalle / pro.ProUnidades) + pro.Cantidad) * proCombo.PrcCantidad;
                            if (parMultiAlmacenes && Entrega != null)
                            {
                                var cantidadAlmacenDespacho = 0.0;

                                if (almIdRanchero > 0 || almIdDespacho > 0)
                                {
                                    cantidadAlmacenDespacho = cantidadSolicitadaTotal;
                                }

                                if (inv.GetCantidadTotalInventario(proCombo.ProID, almIdDespacho) > 0 && cantidadAlmacenDespacho > 0)
                                {
                                    inv.RestarInventario(proCombo.ProID, cantidadAlmacenDespacho, 0, almIdDespacho, "", true);
                                }

                            }
                            else
                            {
                                if (almIdRanchero > 0)
                                {
                                    inv.RestarInventario(proCombo.ProID, cantidadSolicitadaTotal, 0, almIdRanchero, "");
                                }
                                else
                                {
                                    inv.RestarInventario(proCombo.ProID, cantidadSolicitadaTotal, 0, parMultiAlmacenes ? almIdDevolucion : -1, "");
                                }
                            }

                        }
                    }
                    else if (parMultiAlmacenes && Entrega != null)
                    {
                        var cantidadSolicitadaTotal = (pro.CantidadDetalle / pro.ProUnidades) + pro.Cantidad;
                        var cantidadAlmacenDespacho = 0.0;
                        var cantidadAlmacenDevolucion = 0.0;
                        var cantidadNoEntregada = 0.0;

                        if (myParametro.GetParMultialmacenConDevolucion()) {
                            if (cantidadSolicitadaTotal > pro.CantidadEntrega && pro.CantidadEntrega > 0)
                            {
                                cantidadAlmacenDevolucion = cantidadSolicitadaTotal - pro.CantidadEntrega;
                                cantidadAlmacenDespacho = pro.CantidadEntrega;
                            }
                            else
                            {
                                cantidadAlmacenDespacho = cantidadSolicitadaTotal;
                            }
                        }
                        else if (almIdRanchero > 0 || almIdDespacho > 0)
                        {
                            cantidadAlmacenDespacho = cantidadSolicitadaTotal;
                        }

                        if (inv.GetCantidadTotalInventario(pro.ProID, almIdDespacho) > 0 && cantidadAlmacenDespacho > 0)
                        {
                            inv.RestarInventario(pro.ProID, cantidadAlmacenDespacho, 0, almIdDespacho, pro.Lote, true);
                        }

                        if (inv.GetCantidadTotalInventario(pro.ProID, almIdDevolucion) > 0 && cantidadAlmacenDevolucion > 0)
                        {
                            inv.RestarInventario(pro.ProID, cantidadAlmacenDevolucion, 0, almIdDevolucion, pro.Lote, true);
                        }



                        if (myParametro.GetParMoverDevolucionDesdeEntrega())
                        {
                            if (cantidadSolicitadaTotal < pro.CantidadEntrega)
                            {
                                cantidadNoEntregada = pro.CantidadEntrega - cantidadSolicitadaTotal;
                                if (inv.HayExistencia(pro.ProID, cantidadNoEntregada, 0, almIdDespacho))
                                {
                                    inv.AgregarInventario(pro.ProID, cantidadNoEntregada, 0, almIdDevolucion, pro.Lote);
                                    inv.RestarInventario(pro.ProID, cantidadNoEntregada, 0, almIdDespacho, pro.Lote, true);
                                }
                                else if (inv.GetCantidadTotalInventario(pro.ProID, almIdDespacho) > 0)
                                {
                                    inv.AgregarInventario(pro.ProID, inv.GetCantidadTotalInventario(pro.ProID, almIdDespacho), 0, almIdDevolucion, pro.Lote);
                                    inv.RestarInventario(pro.ProID, inv.GetCantidadTotalInventario(pro.ProID, almIdDespacho), 0, almIdDespacho, pro.Lote, true);
                                }
                            }
                        }

                        if (parMultiAlmacenes && Entrega != null && myParametro.GetParMoverDevolucionDesdeEntrega() && pro.CantidadEntrega == 0)
                        {
                            myEnt = new DS_EntregasRepartidorTransacciones();
                            List<EntregasRepartidorTransaccionesDetalle> OfertasNoEntregados = null;
                            OfertasNoEntregados = myEnt.GetProductosOfertasNoEntregados(Entrega.EnrSecuencia, Entrega.TraSecuencia, Entrega.TitID, pro.ProID);
                            foreach (var ent in OfertasNoEntregados)
                            {
                                if (inv.HayExistencia(ent.ProID, ent.TraCantidad, 0, almIdDespacho))
                                {
                                    inv.AgregarInventario(ent.ProID, ent.TraCantidad, 0, almIdDevolucion, "");
                                    inv.RestarInventario(ent.ProID, ent.TraCantidad, 0, almIdDespacho, "", true);
                                }
                                else if (inv.GetCantidadTotalInventario(ent.ProID, almIdDespacho) > 0)
                                {
                                    inv.AgregarInventario(ent.ProID, inv.GetCantidadTotalInventario(ent.ProID, almIdDespacho), 0, parMultiAlmacenes ? almIdDevolucion : -1);
                                    inv.RestarInventario(ent.ProID, inv.GetCantidadTotalInventario(ent.ProID, almIdDespacho), 0, parMultiAlmacenes ? almIdDespacho : -1);
                                }

                            }
                        }

                    }
                    else
                    {
                        if (almIdRanchero > 0)
                        {
                            inv.RestarInventario(pro.ProID, pro.Cantidad, pro.CantidadDetalle, almIdRanchero, pro.Lote);
                        }
                        else {
                            inv.RestarInventario(pro.ProID, pro.Cantidad, pro.CantidadDetalle, parMultiAlmacenes ? almIdDevolucion : -1, pro.Lote);
                        }
                    }
                }
                
            }

            //GetMontosTotalesParaVentas(venSecuencia, montototal);
            if (!myParametro.GetParVentasNoRebajaInventario())
            {
                if (parMultiAlmacenes && Entrega != null && myParametro.GetParMoverDevolucionDesdeEntrega())
                {
                    myEnt = new DS_EntregasRepartidorTransacciones();
                    List<EntregasRepartidorTransaccionesDetalle> NoEntregados = null;
                    NoEntregados = myEnt.GetProductosNoEntregadosEn0xPedidos(Entrega.EnrSecuencia, Entrega.TraSecuencia, Entrega.TitID);
                    foreach (var ent in NoEntregados)
                    {
                        if(inv.HayExistencia(ent.ProID, ent.TraCantidad, 0, almIdDespacho))
                        {
                            inv.AgregarInventario(ent.ProID, ent.TraCantidad, 0, almIdDevolucion, "");
                            inv.RestarInventario(ent.ProID, ent.TraCantidad, 0, almIdDespacho, "", true);
                        }
                        else if (inv.GetCantidadTotalInventario(ent.ProID, almIdDespacho) > 0)
                        {
                            inv.AgregarInventario(ent.ProID, inv.GetCantidadTotalInventario(ent.ProID, almIdDespacho), 0, parMultiAlmacenes ? almIdDevolucion : -1);
                            inv.RestarInventario(ent.ProID, inv.GetCantidadTotalInventario(ent.ProID, almIdDespacho), 0, parMultiAlmacenes ? almIdDespacho : -1);
                        }

                    }
                }
            }

            DS_RepresentantesSecuencias.UpdateSecuencia("Ventas", venSecuencia);

            ActualizarNcfActual(ncf.Secuencia.ToString(), ncf.rowguid);

            if (Entrega != null)
            {
                Hash ent = new Hash("EntregasRepartidorTransacciones");
                ent.Add("enrEstatusEntrega", 2);
                ent.Add("MotID", MotID > -1 ? MotID : 0);
                ent.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                ent.Add("EntFechaActualizacion", Functions.CurrentDate());
                ent.ExecuteUpdate("CliID = " + Arguments.Values.CurrentClient.CliID.ToString() + " and EnrSecuencia = " + Entrega.EnrSecuencia.ToString() + " and EnrSecuenciaDetalle = " + Entrega.EnrSecuenciaDetalle.ToString() + " and TitID = 1 and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' ");
            }

            Hash cxc = new Hash("CuentasxCobrar");
            cxc.Add("CxcReferencia", (Arguments.Values.CurrentClient.CliTipoComprobanteFAC == "99" ? venSecuencia+"-"+Arguments.CurrentUser.RepCodigo : ncf.NCFCompleto));
            cxc.Add("CxcTipoTransaccion", 4);
            cxc.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            cxc.Add("CxcDias", 0);
            cxc.Add("CxcSIGLA", "FAT");
            cxc.Add("CliID", Arguments.Values.CurrentClient.CliID);
            cxc.Add("CxcFecha", Functions.CurrentDate());
            cxc.Add("CxcDocumento", Arguments.CurrentUser.RepCodigo + "-" + venSecuencia);

            var totales = myProd.GetTempTotales((int)Modules.VENTAS,porcDescuentoGeneral: porDescuentoGlobal);

            cxc.Add("CxcBalance", totales.Total);
            cxc.Add("CxcMontoSinItbis", totales.SubTotal);
            cxc.Add("CxcMontoTotal", totales.Total);
            cxc.Add("MonCodigo", Arguments.Values.CurrentClient.MonCodigo);
            cxc.Add("AreaCtrlCredit", 0);
            cxc.Add("CxcNotaCredito", 0);
            cxc.Add("CXCNCF", "");
            cxc.Add("rowguid", Guid.NewGuid().ToString());
            cxc.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            cxc.Add("CueFechaActualizacion", Functions.CurrentDate());
            cxc.Add("CxcFechaVencimiento", myCxCobrar.GetCxcFechaVencimiento(conId));
            cxc.Add("ConID", conId);
            cxc.ExecuteInsert();

            if (((conId == myParametro.GetParConIdFormaPagoContado()) || (conId == myParametro.GetParSegundoConIdFormaPagoContado())) && conId != -1) //si la venta es al contado
            {
                var reciboToSave = new RecibosDocumentosTemp();
                reciboToSave.FechaSinFormatear = Functions.CurrentDate();
                reciboToSave.Fecha = Functions.CurrentDate("dd-MM-yyyy");
                reciboToSave.Documento = Arguments.CurrentUser.RepCodigo + "-" + venSecuencia;
                reciboToSave.Referencia = (Arguments.Values.CurrentClient.CliTipoComprobanteFAC == "99" ? venSecuencia + "-" + Arguments.CurrentUser.RepCodigo : ncf.NCFCompleto);
                reciboToSave.Sigla = "FAT";
                reciboToSave.Aplicado = totales.Total;
                reciboToSave.Descuento = 0;
                reciboToSave.MontoTotal = totales.Total;
                reciboToSave.Balance = totales.Total;
                reciboToSave.Pendiente = 0;
                reciboToSave.Estado = "Saldo";
                reciboToSave.Credito = 0;
                reciboToSave.FechaIngles = Functions.CurrentDate("MM-dd-yyyy");
                reciboToSave.Origen = 1;
                reciboToSave.MontoSinItbis = totales.SubTotal;
                reciboToSave.DescPorciento = 0;
                reciboToSave.AutSecuencia = 0;
                reciboToSave.FechaDescuento = Functions.CurrentDate("dd-MM-yyyy");
                reciboToSave.Dias = 0;
                reciboToSave.DescuentoFactura = 0;
                reciboToSave.Clasificacion = "";
                reciboToSave.FechaVencimiento = Functions.CurrentDate();
                reciboToSave.Retencion = 0;
                reciboToSave.CXCNCF = "";
                reciboToSave.cxcComentario = "";
                reciboToSave.RecTipo = 1;
                
                new DS_Recibos().ClearTemps();

                SqliteManager.GetInstance().Insert(reciboToSave);

               // Arguments.Values.CurrentIsReciboHuerfano = true;


                /////////////////////////////////////////////////////////////
                ///
                if (DS_RepresentantesParametros.GetInstance().GetParVentaSaldarReciboAutomatico() && Arguments.Values.CurrentClient.CliFormasPago.Equals("1000000"))
                {
                    int RecSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Recibos");
                    int reaSecuencia = 0;
                    double PorcentajeRetencion=0.00;
                    double Retencion = 0;
                    double totalGeneral = totales.Total;

                    if (Arguments.Values.CurrentClient.CliDatosOtros.Contains("T"))
                    {
                        PorcentajeRetencion = myParametro.GetParRecibosPorcientoRetencion();
                        Retencion = (totalGeneral * PorcentajeRetencion) /100;
                        Retencion = Math.Round(Retencion, 2);
                    }

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
                    rec.Add("RecMontoEfectivo", /*myRec.GetMontoTotalFormaPagoByName("EFECTIVO")*/totales.Total - Retencion);
                    rec.Add("RecMontoCheque", 0);
                    rec.Add("RecMontoChequeF", 0);
                    rec.Add("RecMontoTransferencia", 0);
                    rec.Add("RecMontoTarjeta", 0);
                    rec.Add("RecMontoSobrante", 0);
                    rec.Add("CuaSecuencia", Arguments.Values.CurrentCuaSecuencia);
                    rec.Add("VisSecuencia", -1);
                    rec.Add("DepSecuencia", 0);
                    rec.Add("RecRetencion", Retencion);
                    rec.Add("RecDivision", 0);
                    rec.Add("SecCodigo", (Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.SecCodigo : ""));
                    rec.Add("AreactrlCredit", Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.AreaCtrlCredit : null);
                    rec.Add("MonCodigo", Arguments.Values.CurrentClient.MonCodigo);
                    rec.Add("OrvCodigo", "");
                    rec.Add("OfvCodigo", "");
                    rec.Add("RecCantidadImpresion", 0);
                    rec.Add("RecTotal", totales.Total  /*myRec.GetMontoTotalFormasPago()*/);
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
                    ap.Add("RecValor", totales.Total);
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
                    pago.Add("RefValor", totales.Total - Retencion);
                    pago.Add("CXCReferencia", (Arguments.Values.CurrentClient.CliTipoComprobanteFAC == "99" ? venSecuencia + "-" + Arguments.CurrentUser.RepCodigo : ncf.NCFCompleto));
                    pago.Add("cliid", Arguments.Values.CurrentClient.CliID);
                    //if de multimoneda
                    pago.Add("RecPrima", totales.Total - Retencion);
                    pago.Add("MonCodigo", Arguments.Values.CurrentClient.MonCodigo);
                    pago.Add("RecTasa", 0);
                    pago.Add("SocCodigo", Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.AreaCtrlCredit : null);
                    pago.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                    pago.Add("RecFechaActualizacion", Functions.CurrentDate());
                    pago.Add("rowguid", Guid.NewGuid().ToString());
                    pago.ExecuteInsert();

                    if (Arguments.Values.CurrentClient.CliDatosOtros.Contains("T"))
                    {
                        Hash pago2 = new Hash("RecibosFormaPago");
                        pago2.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                        pago2.Add("RecTipo", 1);
                        pago2.Add("RecSecuencia", RecSecuencia);
                        pago2.Add("RefSecuencia", 2);
                        pago2.Add("AutSecuencia", null);
                        pago2.Add("ForID", 5);
                        pago2.Add("BanID", 0);
                        pago2.Add("RefNumeroCheque", 0);
                        pago2.Add("RefFecha", Functions.CurrentDate());
                        pago2.Add("RefIndicadorDiferido", 0);
                        pago2.Add("RefNumeroAutorizacion", 0);
                        pago2.Add("RefValor",  Retencion);
                        pago2.Add("CXCReferencia", (Arguments.Values.CurrentClient.CliTipoComprobanteFAC == "99" ? venSecuencia + "-" + Arguments.CurrentUser.RepCodigo : ncf.NCFCompleto));
                        pago2.Add("cliid", Arguments.Values.CurrentClient.CliID);
                        pago2.Add("RecPrima",   Retencion);
                        pago2.Add("MonCodigo", Arguments.Values.CurrentClient.MonCodigo);
                        pago2.Add("RecTasa", 0);
                        pago2.Add("SocCodigo", Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.AreaCtrlCredit : null);
                        pago2.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                        pago2.Add("RecFechaActualizacion", Functions.CurrentDate());
                        pago2.Add("rowguid", Guid.NewGuid().ToString());
                        pago2.ExecuteInsert();
                    }

                    DS_RepresentantesSecuencias.UpdateSecuencia("Recibos", RecSecuencia);
                }
                else
                {
                    saveReciboFromVenta = true;
                }
            }

            if (DS_RepresentantesParametros.GetInstance().GetParVisitasResultados())
            {
                ActualizarVisitasResultados();
            }

            myProd.ClearTemp((int)Modules.VENTAS);

            try
            {
                if (DS_RepresentantesParametros.GetInstance().GetParVentaGeneraXmlFirmado())
                {
                    var xml = VentaToXml(venSecuencia);

                    var signed = Functions.SignXml(xml);

                    var xmlList = Functions.SplitByLength(signed, 5000);

                    var venpos = 1;
                    foreach(var value in xmlList)
                    {
                        Hash m = new Hash("VentasXml");
                        m.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                        m.Add("VenSecuencia", venSecuencia);
                        m.Add("VenPosicion", venpos); venpos++;
                        m.Add("rowguid", Guid.NewGuid().ToString());
                        m.Add("VenFechaActualizacion", Functions.CurrentDate());
                        m.Add("VenXml", value);
                        m.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                        m.ExecuteInsert();
                    }                   
                    
                }
            }catch(Exception e)
            {
                Microsoft.AppCenter.Crashes.Crashes.TrackError(e);
            }

            return venSecuencia;
        }

        private void ActualizarVisitasResultados()
        {
            var list = SqliteManager.GetInstance().Query<VisitasResultados>("select 4 as TitID, count(*) as VisCantidadTransacciones, " +
                "sum(((d.VenPrecio + d.VenAdValorem + d.VenSelectivo) - d.VenDescuento) * ((case when d.VenCantidadDetalle > 0 then d.VenCantidadDetalle / o.ProUnidades else 0 end) + d.VenCantidad)) as VisMontoSinItbis, sum(((d.VenItbis / 100.0) * ((d.VenPrecio + d.VenAdValorem + d.VenSelectivo) - d.VenDescuento)) * ((case when d.VenCantidadDetalle > 0 then d.VenCantidadDetalle / o.ProUnidades else 0 end) + d.VenCantidad)) as VisMontoItbis from Ventas p " +
                "inner join VentasDetalle d on d.RepCodigo = p.RepCodigo and d.VenSecuencia = p.VenSecuencia " +
                "inner join Productos o on o.ProID = d.ProID " +
                "where p.VisSecuencia = ? and p.RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'", new string[] { Arguments.Values.CurrentVisSecuencia.ToString() });

            if (list != null && list.Count > 0)
            {
                var item = list[0];

                item.VisMontoTotal = item.VisMontoSinItbis + item.VisMontoItbis;
                item.VisComentario = "";

                new DS_Visitas().GuardarVisitasResultados(item);
            }
        }

        public void ActualizarNcfActual(string NCF, string rowguid)
        {
            if (Arguments.Values.CurrentClient.CliTipoComprobanteFAC == "99")
            {
                return;
            }

            Hash n = new Hash("" + (myParametro.GetParTakeFromNCF2021() ? "RepresentantesDetalleNCF2021" : "RepresentantesDetalleNCF2018") + "");
            n.Add("ReDNCFActual", /*"ReDNCFActual + 1"*/NCF, true);
            n.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            n.ExecuteUpdate("rowguid = '"+ rowguid + "' ");
        }

        public Ventas GetBySecuencia(int venSecuencia, bool confirmado)
        {
            var list = SqliteManager.GetInstance().Query<Ventas>("select VenSecuencia, v.RepCodigo as RepCodigo, v.PedSecuencia as PedSecuencia, v.CliID as CliID, VenFecha, RepVendedor, VisSecuencia, VenReferencia, " +
                "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(VenNCFFechaVencimiento,1,10)),' ','' ), '') as VenNCFFechaVencimiento, VenTotal, " +
                "v.ConID as ConID, ConDescripcion, ltrim(rtrim(VenNCF)) as VenNCF, VenCantidadImpresion, CliNombre, ifnull(CliNombreComercial,'') as CliNombreComercial, CliTelefono, CliContacto as CliPropietario, CliCodigo, CliRnc, CliCalle, CliUrbanizacion, cli.CliTipoComprobanteFAC as CliTipoComprobanteFAC,ifnull(VenCantidadCanastos,0) as VenCantidadCanastos,v.rowguid as rowguid, " + (myParametro.GetNumOrdenObligatorio() ? " ifnull(v.VenOrdenCompra,'') " : " '' ") + " as VenOrdenCompra from " + (confirmado ? "VentasConfirmados" : "Ventas") + " v " +
                "left join CondicionesPago c on c.ConID = v.ConID " +
                "inner join Clientes cli on cli.CliID = v.CliID " +
                "where VenSecuencia = ? and ltrim(rtrim(v.RepCodigo)) = ? ", new string[] { venSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public Ventas GetBySecuenciaConTotales(int venSecuencia, bool confirmado)
        {
            var list = SqliteManager.GetInstance().Query<Ventas>("select VenSecuencia, v.RepCodigo as RepCodigo, v.PedSecuencia as PedSecuencia, v.CliID as CliID, ifnull(replace( strftime('%d/%m/%Y', SUBSTR(VenFecha,1,10)),' ','' ), '') as VenFecha, RepVendedor, VisSecuencia, VenReferencia, ifnull(VenMontoSinITBIS,0) as VenMontoSinITBIS, ifnull(VenMontoITBIS,0) as VenMontoITBIS, ifnull(VenMontoTotal,0) as VenMontoTotal, ifnull(VenSubTotal,0) as VenSubTotal, ifnull(VenPorCientoDsctoGlobal,0) as VenPorCientoDsctoGlobal, ifnull(VenMontoDsctoGlobal,0) as VenMontoDsctoGlobal, " +
                "ifnull(replace( strftime('%d/%m/%Y', SUBSTR(VenNCFFechaVencimiento,1,10)),' ','' ), '') as VenNCFFechaVencimiento, VenTotal, " +
                "v.ConID as ConID, ConDescripcion, ltrim(rtrim(VenNCF)) as VenNCF, VenCantidadImpresion, CliNombre, ifnull(CliNombreComercial,'') as CliNombreComercial, CliTelefono, CliContacto as CliPropietario, CliCodigo, CliRnc, CliCalle, CliUrbanizacion, cli.CliTipoComprobanteFAC as CliTipoComprobanteFAC,ifnull(VenCantidadCanastos,0) as VenCantidadCanastos,v.rowguid as rowguid from " + (confirmado ? "VentasConfirmados" : "Ventas") + " v " +
                "left join CondicionesPago c on c.ConID = v.ConID " +
                "inner join Clientes cli on cli.CliID = v.CliID " +
                "where VenSecuencia = ? and ltrim(rtrim(v.RepCodigo)) = ? ", new string[] { venSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public Ventas GetByNCF(string VenNCF)
        {
            var list = SqliteManager.GetInstance().Query<Ventas>("select VenSecuencia, v.RepCodigo as RepCodigo, strftime('%d-%m-%Y', VenFecha) as VenFecha, ifnull(v.VenOrdenCompra,'') as VenOrdenCompra " +
                " from Ventas v " +
                "where VenNCF = '" + VenNCF.ToString() + "' and ltrim(rtrim(v.RepCodigo)) = '"+ Arguments.CurrentUser.RepCodigo +"' " +
                "UNION ALL " +
                "select VenSecuencia, v.RepCodigo as RepCodigo, strftime('%d-%m-%Y', VenFecha) as VenFecha, ifnull(v.VenOrdenCompra,'') as VenOrdenCompra " +
                " from VentasConfirmados v " +
                "where VenNCF = '"+ VenNCF.ToString() + "' and ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' ", new string[] {   });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public List<VentasDetalle> GetDetalleByPosicion(int venSecuencia, bool confirmado)
        {
            return SqliteManager.GetInstance().Query<VentasDetalle>("select v.VenSecuencia, ProCodigo,case when ifnull(ProUnidades, 1) = 0 then 1 else ifnull(ProUnidades, 1) end as ProUnidades, ProDescripcion, sum(VenCantidad) VenCantidad, ifnull(ProReferencia, '') as  ProReferencia, " +
               "sum(VenCantidadDetalle)  VenCantidadDetalle, VenItbis, VenPrecio, ifnull(VenDescuento,0) as VenDescuento, VenindicadorOferta, VenPosicion, VenTotalItbis, VenTotalDescuento, VenDescPorciento, v.ProID as ProID, " +
               "lipprecio as PrecioUnidades, (lipprecio * ifnull(p.prounidades, 0.0)) as PrecioCajas, ifnull(v.VenLote, '') as VenLote  " +
               "from " + (confirmado ? "VentasDetalleConfirmados" : "VentasDetalle") + " v " +
               "inner join Productos p on p.ProID = v.ProID inner join " + (confirmado ? "VentasConfirmados" : "Ventas") + " vt on vt.vensecuencia = v.venSecuencia " +
               "inner join clientes c on c.cliid = vt.cliid left join listaprecios lp on lp.unmcodigo = p.unmcodigo and lp.lipcodigo = c.lipcodigo and lp.proid = v.proid " +
               "and lp.moncodigo = vt.moncodigo " +
               "where v.VenSecuencia = ? and ltrim(rtrim(v.RepCodigo)) = ? " +
               "group by ProCodigo, VenindicadorOferta, ifnull(v.VenLote, '') order by v.VenPosicion ", new string[] { venSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });
        }

        public List<VentasDetalle> GetDetalleBySecuencia(int venSecuencia, bool confirmado)
        {
            return SqliteManager.GetInstance().Query<VentasDetalle>("select v.VenSecuencia, ProCodigo,case when ifnull(ProUnidades, 1) = 0 then 1 else ifnull(ProUnidades, 1) end as ProUnidades, ProDescripcion, sum(VenCantidad) VenCantidad, ifnull(ProReferencia, '') as  ProReferencia, " +
                "sum(VenCantidadDetalle)  VenCantidadDetalle, VenItbis, VenPrecio, ifnull(VenDescuento,0) as VenDescuento, VenindicadorOferta, VenPosicion, VenTotalItbis, VenTotalDescuento, ifnull(p.ProDatos3,'') as ProDatos3, VenDescPorciento, v.ProID as ProID, " +
                "lipprecio as PrecioUnidades, (lipprecio * ifnull(p.prounidades, 0.0)) as PrecioCajas, ifnull(v.VenLote, '') as VenLote  " +
                "from " + (confirmado ? "VentasDetalleConfirmados" : "VentasDetalle") + " v " +
                "inner join Productos p on p.ProID = v.ProID inner join " + (confirmado ? "VentasConfirmados" : "Ventas") + " vt on vt.vensecuencia = v.venSecuencia " +
                "inner join clientes c on c.cliid = vt.cliid left join listaprecios lp on lp.unmcodigo = p.unmcodigo and lp.lipcodigo = c.lipcodigo and lp.proid = v.proid " +
                "and lp.moncodigo = vt.moncodigo "+
                "where v.VenSecuencia = ? and ltrim(rtrim(v.RepCodigo)) = ? " +
                "group by ProCodigo, VenindicadorOferta, ifnull(v.VenLote, '') order by ProDescripcion ", new string[] { venSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });
        }

        public List<VentasDetalle> GetDetalleBySecuenciaTabacalera(int venSecuencia, bool confirmado)
        {
            return SqliteManager.GetInstance().Query<VentasDetalle>("select VenSecuencia, ProCodigo, case when ifnull(ProUnidades, 1) = 0 then 1 else ifnull(ProUnidades, 1) end as ProUnidades, ProDescripcion, VenCantidad, " +
                "VenCantidadDetalle, VenItbis, VenAdValorem, VenSelectivo, VenPrecio, VenDescuento, VenindicadorOferta, VenPosicion, VenTotalItbis, VenTotalDescuento, VenDescPorciento, v.ProID as ProID " +
                "from " + (confirmado ? "VentasDetalleConfirmados" : "VentasDetalle") + " v " +
                "inner join Productos p on p.ProID = v.ProID " +
                "where VenSecuencia = ? and ltrim(rtrim(v.RepCodigo)) = ? " +
                "order by ProDescripcion", new string[] { venSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });
        }


        public List<VentasDetalle> GetDetalleByClienteyFechas(DateTime desde, DateTime hasta)
        {

            return SqliteManager.GetInstance().Query<VentasDetalle>("select sum(vd.VenCantidad) as VenCantidad, sum(vd.VenCantidadDetalle) as VenCantidadDetalle, vd.ProID as ProID, ProCodigo, ProDescripcion, VenindicadorOferta, OfeID  " +
                "from Ventas v " +
                "inner join VentasDetalle vd on v.repcodigo = vd.repcodigo and v.VenSecuencia = vd.VenSecuencia " +
                "inner join Productos p on p.ProID = vd.ProID " +
                "inner join clientes c on c.cliid = v.cliid " +
                "where v.CliID = ? and cast(strftime('%Y%m%d',v.VenFecha) as integer) between cast(strftime('%Y%m%d', '" + desde.ToString("yyyy-MM-dd") + "') as integer) and cast(strftime('%Y%m%d', '" + hasta.ToString("yyyy-MM-dd") + "') as integer) " +
                "and v.VenEstatus != 0 " +
                "group by ProCodigo, VenindicadorOferta, OfeID " +
                "UNION " +
                "select sum(vd.VenCantidad) as VenCantidad, sum(vd.VenCantidadDetalle) as VenCantidadDetalle, vd.ProID as ProID, ProCodigo, ProDescripcion, VenindicadorOferta, OfeID " +
                "from VentasConfirmados v " +
                "inner join VentasDetalleConfirmados vd on v.repcodigo = vd.repcodigo and v.VenSecuencia = vd.VenSecuencia " +
                "inner join Productos p on p.ProID = vd.ProID " +
                "inner join clientes c on c.cliid = v.cliid " +
                "where v.CliID = ? and cast(strftime('%Y%m%d',v.VenFecha) as integer) between cast(strftime('%Y%m%d', '" + desde.ToString("yyyy-MM-dd") + "') as integer) and cast(strftime('%Y%m%d', '" + hasta.ToString("yyyy-MM-dd") + "') as integer) " +
                "and v.VenEstatus != 0 " +
                "group by ProCodigo, VenindicadorOferta, OfeID ", new string[] { Arguments.Values.CurrentClient.CliID.ToString(), Arguments.Values.CurrentClient.CliID.ToString() });
        }

        public List<VentasDetalle> GetDetalleWithCodigoBarra(int venSecuencia, bool confirmado)
        {
            return SqliteManager.GetInstance().Query<VentasDetalle>("select VenSecuencia, ProCodigo, case when ifnull(ProUnidades, 1) = 0 then 1 else ifnull(ProUnidades, 1) end as ProUnidades, ProDescripcion, VenCantidad, " +
                "VenCantidadDetalle, VenItbis, VenAdValorem, VenSelectivo, VenPrecio, VenDescuento, VenindicadorOferta, VenPosicion, VenTotalItbis, VenTotalDescuento, VenDescPorciento, v.ProID as ProID, p.ProReferencia as ProReferencia " +
                "from " + (confirmado ? "VentasDetalleConfirmados" : "VentasDetalle") + " v " +
                "inner join Productos p on p.ProID = v.ProID " +
                "where VenSecuencia = ? and ltrim(rtrim(v.RepCodigo)) = ? " +
                "order by ProDescripcion", new string[] { venSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });
        }

        public List<VentasDetalle> GetDetalleWithCodigoBarraByVenPosicion(int venSecuencia, bool confirmado)
        {
            return SqliteManager.GetInstance().Query<VentasDetalle>("select VenSecuencia, ProCodigo, case when ifnull(ProUnidades, 1) = 0 then 1 else ifnull(ProUnidades, 1) end as ProUnidades, ProDescripcion, VenCantidad, " +
                "VenCantidadDetalle, VenItbis, VenAdValorem, VenSelectivo, VenPrecio, VenDescuento, VenindicadorOferta, VenPosicion, VenTotalItbis, VenTotalDescuento, VenDescPorciento, v.ProID as ProID, p.ProReferencia as ProReferencia " +
                "from " + (confirmado ? "VentasDetalleConfirmados" : "VentasDetalle") + " v " +
                "inner join Productos p on p.ProID = v.ProID " +
                "where VenSecuencia = ? and ltrim(rtrim(v.RepCodigo)) = ? " +
                "order by v.VenPosicion", new string[] { venSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });
        }

        public List<VentasDetalle> GetDetalleBySecuenciaSinLote(int venSecuencia, bool confirmado)
        {
            return SqliteManager.GetInstance().Query<VentasDetalle>("select v.VenSecuencia, ProCodigo,case when ifnull(ProUnidades, 1) = 0 then 1 else ifnull(ProUnidades, 1) end as ProUnidades, ProDescripcion, sum(VenCantidad) VenCantidad, ifnull(ProReferencia, '') as  ProReferencia, " +
                "sum(VenCantidadDetalle)  VenCantidadDetalle, VenItbis, VenPrecio, ifnull(VenDescuento,0) as VenDescuento, VenindicadorOferta, sum(VenTotalItbis) VenTotalItbis, sum(VenTotalDescuento) VenTotalDescuento, VenDescPorciento, v.ProID as ProID, " +
                "lipprecio as PrecioUnidades, (lipprecio * ifnull(p.prounidades, 0.0)) as PrecioCajas  " +
                "from " + (confirmado ? "VentasDetalleConfirmados" : "VentasDetalle") + " v " +
                "inner join Productos p on p.ProID = v.ProID inner join " + (confirmado ? "VentasConfirmados" : "Ventas") + " vt on vt.vensecuencia = v.venSecuencia " +
                "inner join clientes c on c.cliid = vt.cliid left join listaprecios lp on lp.unmcodigo = p.unmcodigo and lp.lipcodigo = c.lipcodigo and lp.proid = v.proid " +
                "and lp.moncodigo = vt.moncodigo " +
                "where v.VenSecuencia = ? and ltrim(rtrim(v.RepCodigo)) = ? " +
                "group by ProCodigo, VenindicadorOferta order by ProDescripcion ", new string[] { venSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });
        }

        public void ActualizarCantidadImpresion(/*int venSecuencia*/ string rowguid )
        {
            Hash map = new Hash("Ventas");
            map.Add("VenCantidadImpresion", "VenCantidadImpresion + 1", true);
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map.ExecuteUpdate(/*"ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and VenSecuencia = " + venSecuencia*/  " rowguid = '" + rowguid + "' ");

            Hash map2 = new Hash("VentasConfirmados");
            map2.Add("VenCantidadImpresion", "VenCantidadImpresion + 1", true);
            map2.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map2.ExecuteUpdate(/*"ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and VenSecuencia = " + venSecuencia*/ " rowguid = '"+ rowguid +"' ");

        }

        public void EstVenta(int venSecuencia, int est)
        {
            var venta = GetBySecuencia(venSecuencia, false);

            if (venta == null)
            {
                throw new Exception("No se encontraron los datos de la venta");
            }

            var v = new Hash("Ventas");
            v.Add("VenEstatus", est);
            v.Add("VenFechaActualizacion", Functions.CurrentDate());
            v.Add("UsuInicioSesion", /*Arguments.CurrentUser.RepCodigo*/"mdsoft");         

            if (est == 0)
            {
                if (new DS_SuscriptoresCambios().UpdateCambioEstadoInsertByRowguid(venta.rowguid, est))
                {
                    v.SaveScriptForServer = false;
                }
            }

            v.ExecuteUpdate("rowguid = '" + venta.rowguid + "' ");

            if (venta.PedSecuencia > 0)
            {
                Hash ent = new Hash("EntregasRepartidorTransacciones");
                ent.Add("enrEstatusEntrega", 1);
                ent.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                ent.Add("EntFechaActualizacion", Functions.CurrentDate());
                ent.ExecuteUpdate("TraSecuencia = " + venta.PedSecuencia.ToString() + " and TitID = 1 and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' ");
            }

            var dsInv = new DS_Inventarios();

            var list = GetDetalleBySecuencia(venSecuencia, false);
            var parMultiAlmacenes = myParametro.GetParUsarMultiAlmacenes();
            int almid = myParametro.GetParAlmacenVentaRanchera();
            int almidDevolucion = myParametro.GetParAlmacenIdParaDevolucion();

            if (!myParametro.GetParVentasNoRebajaInventario())
            {
                if (list != null && list.Count > 0)
                {
                    if (almidDevolucion > 0 && myParametro.GetParMoverDevolucionDesdeEntrega())
                    {
                        almid = myParametro.GetParAlmacenIdParaDevolucion();
                    }

                    if (venta.PedSecuencia > 0)
                    {
                        almid= myParametro.GetParAlmacenIdParaDespacho();
                    }


                    foreach (var det in list)
                    {
                        if (!string.IsNullOrWhiteSpace(det.ProDatos3) && det.ProDatos3.Contains("x"))
                        {
                            var productosCombo = new DS_ProductosCombos().GetProductosCombo(det.ProID);
                            foreach (var proCombo in productosCombo)
                            {
                                dsInv.AgregarInventario(proCombo.ProID, (det.VenCantidad * proCombo.PrcCantidad), det.VenCantidadDetalle, parMultiAlmacenes ? almid : -1, det.VenLote != null ? det.VenLote : "");
                            }
                        }
                        else
                        {
                            dsInv.AgregarInventario(det.ProID, det.VenCantidad, det.VenCantidadDetalle, parMultiAlmacenes ? almid : -1, det.VenLote != null ? det.VenLote : "");
                        }

                        if (myParametro.GetParMoverDevolucionDesdeEntrega() && almidDevolucion > 0 && venta.PedSecuencia > 0)
                        {
                            var ListEntrega = getEntregasRealizadasByPedido(venta.PedSecuencia, det.ProID);
                            if(ListEntrega != null)
                            {
                                if (ListEntrega[0].TraCantidad > det.VenCantidad)
                                {
                                    var cantidadEnDevolucion = ListEntrega[0].TraCantidad - det.VenCantidad;
                                    if (dsInv.HayExistencia(det.ProID, cantidadEnDevolucion, 0, almidDevolucion))
                                    {
                                        dsInv.AgregarInventario(det.ProID, cantidadEnDevolucion, 0, parMultiAlmacenes ? almid : -1);
                                        dsInv.RestarInventario(det.ProID, cantidadEnDevolucion, 0, parMultiAlmacenes ? almidDevolucion : -1);

                                    }
                                    else if (dsInv.GetCantidadTotalInventario(det.ProID, almidDevolucion) > 0)
                                    {
                                        dsInv.AgregarInventario(det.ProID, dsInv.GetCantidadTotalInventario(det.ProID, almidDevolucion), 0, parMultiAlmacenes ? almid : -1);
                                        dsInv.RestarInventario(det.ProID, dsInv.GetCantidadTotalInventario(det.ProID, almidDevolucion), 0, parMultiAlmacenes ? almidDevolucion : -1);
                                    }
                                }
                            }

                        }

                    }
                }

                if (parMultiAlmacenes && myParametro.GetParMoverDevolucionDesdeEntrega() && venta.PedSecuencia > 0)
                {

                    almid = myParametro.GetParAlmacenIdParaDespacho();

                    var ListEntrega = getEntregasRealizadasByPedidoGeneral(venta.PedSecuencia);
                    if (ListEntrega != null)
                    {
                        myEnt = new DS_EntregasRepartidorTransacciones();
                        List<EntregasRepartidorTransaccionesDetalle> NoEntregados = null;
                        NoEntregados = myEnt.GetProductosNoEntregadosEnVenta(ListEntrega[0].EnrSecuencia, ListEntrega[0].TraSecuencia, 1, venSecuencia);
                        foreach (var ent in NoEntregados)
                        {
                            if (dsInv.HayExistencia(ent.ProID, ent.TraCantidad, 0, almidDevolucion))
                            {
                                dsInv.RestarInventario(ent.ProID, ent.TraCantidad, 0, parMultiAlmacenes ? almidDevolucion : -1);
                                dsInv.AgregarInventario(ent.ProID, ent.TraCantidad, 0, parMultiAlmacenes ? almid : -1);
                            }
                            else if (dsInv.GetCantidadTotalInventario(ent.ProID, almidDevolucion) > 0)
                            {
                                dsInv.AgregarInventario(ent.ProID, dsInv.GetCantidadTotalInventario(ent.ProID, almidDevolucion), 0, parMultiAlmacenes ? almid : -1);
                                dsInv.RestarInventario(ent.ProID, dsInv.GetCantidadTotalInventario(ent.ProID, almidDevolucion), 0, parMultiAlmacenes ? almidDevolucion : -1);
                            }

                        }
                    }
                }
            }

            if ((venta.ConID == myParametro.GetParConIdFormaPagoContado()) || (venta.ConID == myParametro.GetParSegundoConIdFormaPagoContado())) //si la venta es al contado
            {
                var rowguid = GetRowguidByVenta(venSecuencia);

                if (rowguid == "")
                {
                    return;
                }

                var r = new Hash("Recibos");
                r.Add("RecEstatus", est);
                r.Add("RecFechaActualizacion", Functions.CurrentDate());
                r.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);

                if (est == 0)
                {
                    if (new DS_SuscriptoresCambios().UpdateCambioEstadoInsertByRowguid(rowguid, est))
                    {
                        v.SaveScriptForServer = false;
                    }
                }

                r.ExecuteUpdate("rowguid = '" + rowguid + "' ");

            }

            var cxc = new Hash("CuentasxCobrar");
            cxc.ExecuteDelete("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and ltrim(rtrim(CXCReferencia)) = '" + venta.VenNCF + "'");

        }

        private int GetRecSecuenciaByVenta(int venSecuencia)
        {
            var list = SqliteManager.GetInstance().Query<Ventas>("select RecSecuencia as VenSecuencia from RecibosAplicacion r " +
                "inner join CuentasxCobrar cxc on cxc.CxcReferencia = r.CxcReferencia and r.RepCodigo = cxc.RepCodigo " +
                "inner join Ventas v on v.RepCodigo = r.RepCodigo and v.VenNCF = cxc.CxcReferencia " +
                "where v.VenSecuencia = ? and ltrim(rtrim(v.RepCodigo)) = ?", new string[] { venSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            if (list != null && list.Count > 0)
            {
                return list[0].VenSecuencia;
            }

            return -1;
        }

        private string GetRowguidByVenta(int venSecuencia)
        {
            var list = SqliteManager.GetInstance().Query<Ventas>("select  rc.rowguid as rowguid from RecibosAplicacion r " +
                "inner join Recibos rc on rc.RecSecuencia = r.RecSecuencia and rc.RepCodigo = r.RepCodigo and rc.RecTipo = r.RecTipo " +
                "inner join CuentasxCobrar cxc on cxc.CxcReferencia = r.CxcReferencia and r.RepCodigo = cxc.RepCodigo " +
                "inner join Ventas v on v.RepCodigo = r.RepCodigo and v.VenNCF = cxc.CxcReferencia " +
                "where v.VenSecuencia = ? and ltrim(rtrim(v.RepCodigo)) = ?", new string[] { venSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            if (list != null && list.Count > 0)
            {
                return list[0].rowguid;
            }

            return "";
        }
        //Obtener ventas a Contado ConID = 1
        public List<Ventas> GetVentasaContadoByCuaSecuencia(int cuaSecuencia, int forpago = 1, bool notEntrega = false)
        {
            return SqliteManager.GetInstance().Query<Ventas>("select v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre, v.CliID as CliID, " +
                "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float)))) +vd.venTotalitbis ) "+
                " as VenTotal, v.VenNCF as VenNCF, SUM(((cast(vd.VenPrecio as float) - cast(vd.VenDescuento as float))) * (cast(vd.VenCantidad as float) + (cast(ifnull(vd.VenCantidadDetalle, 0) as float) / cast(ifnull(p.ProUnidades, 1) as float)))) as VenTotalUnitario " +
                " , sum(vd.venTotalitbis) as venTotalitbis, vd.VenDescuento as VenDescuento, vd.VenIndicadorOferta as VenIndicadorOferta " +
                "from Ventas v inner join VentasDetalle vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID inner join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and v.CuaSecuencia = " + cuaSecuencia + (forpago != 1 ? " and v.Conid = "+ forpago : " and v.Conid = 1") + " and v.venEstatus <> 0 " + (notEntrega ? " and v.PedSecuencia is null " : "") + " " +
                "group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf " +
                "UNION  " +
                "select v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre, v.CliID as CliID, sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float)))) + vd.venTotalitbis) as VenTotal, " +
                "v.VenNCF as VenNCF, sum(((vd.VenPrecio + vd.VenSelectivo + vd.VenAdValorem - vd.VenDescuento) * vd.VenCantidad) + vd.venTotalitbis) as VenTotalUnitario " +
                " , sum(vd.venTotalitbis) as venTotalitbis, vd.VenDescuento as VenDescuento, vd.VenIndicadorOferta as VenIndicadorOferta " +
                "from VentasConfirmados v inner join VentasDetalleConfirmados vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID left outer join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and v.CuaSecuencia = " + cuaSecuencia + (forpago != 1 ? " and v.Conid = " +forpago : " and v.Conid = 1") + " and v.venEstatus <> 0 " + (notEntrega ? " and v.PedSecuencia is null " : "") + " " +
                "group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf", new string[] { });
        }

        public List<Ventas> GetVentasaContadoByCuaSecuenciaConVariasFormaPago(int cuaSecuencia, string forpago = "", bool notEntrega = false)
        {
            return SqliteManager.GetInstance().Query<Ventas>("select v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre, v.CliID as CliID, " +
                "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float)))) +vd.venTotalitbis ) " +
                " as VenTotal, v.VenNCF as VenNCF, SUM(((cast(vd.VenPrecio as float) - cast(vd.VenDescuento as float))) * (cast(vd.VenCantidad as float) + (cast(ifnull(vd.VenCantidadDetalle, 0) as float) / cast(ifnull(p.ProUnidades, 1) as float)))) as VenTotalUnitario " +
                " , sum(vd.venTotalitbis) as venTotalitbis, vd.VenDescuento as VenDescuento, vd.VenIndicadorOferta as VenIndicadorOferta " +
                "from Ventas v inner join VentasDetalle vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID inner join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and v.CuaSecuencia = " + cuaSecuencia + (forpago != "" ? " and v.Conid in ( " + forpago + " ) " : " and v.Conid = 1") + " and v.venEstatus <> 0 " + (notEntrega ? " and v.PedSecuencia is null " : "") + " " +
                "group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf " +
                "UNION  " +
                "select v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre, v.CliID as CliID, sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float)))) + vd.venTotalitbis) as VenTotal, " +
                "v.VenNCF as VenNCF, sum(((vd.VenPrecio + vd.VenSelectivo + vd.VenAdValorem - vd.VenDescuento) * vd.VenCantidad) + vd.venTotalitbis) as VenTotalUnitario " +
                " , sum(vd.venTotalitbis) as venTotalitbis, vd.VenDescuento as VenDescuento, vd.VenIndicadorOferta as VenIndicadorOferta " +
                "from VentasConfirmados v inner join VentasDetalleConfirmados vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID left outer join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and v.CuaSecuencia = " + cuaSecuencia + (forpago != "" ? " and v.Conid in ( " + forpago + " ) " : " and v.Conid = 1") + " and v.venEstatus <> 0 " + (notEntrega ? " and v.PedSecuencia is null " : "") + " " +
                "group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf", new string[] { });
        }


        public List<Ventas> GetVentasaContadoByCuaSecuenciaConVariasFormaPagoConTotalCalculado(int cuaSecuencia, string forpago = "", bool notEntrega = false)
        {
            return SqliteManager.GetInstance().Query<Ventas>("select v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre, v.CliID as CliID, " +
                "Round((Select Sum((VenCantidad) * (cast(VenPrecio as float) - (cast(VenPrecio as float) * VenDescPorciento/100) +  ( (cast(VenPrecio as float) - (cast(VenPrecio as float) * VenDescPorciento/100)) * VenItbis/100)     )) from VentasDetalle vd  Inner Join Productos p on p.Proid=vd.Proid where repcodigo= v.repcodigo and vensecuencia=v.Vensecuencia),2) " +
                " as VenTotal, v.VenNCF as VenNCF, SUM(((cast(vd.VenPrecio as float) - cast(vd.VenDescuento as float))) * (cast(vd.VenCantidad as float) + (cast(ifnull(vd.VenCantidadDetalle, 0) as float) / cast(ifnull(p.ProUnidades, 1) as float)))) as VenTotalUnitario " +
                " , sum(vd.venTotalitbis) as venTotalitbis, vd.VenDescuento as VenDescuento, vd.VenIndicadorOferta as VenIndicadorOferta " +
                "from Ventas v inner join VentasDetalle vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID inner join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and v.CuaSecuencia = " + cuaSecuencia + (forpago != "" ? " and v.Conid in ( " + forpago + " ) " : " and v.Conid = 1") + " and v.venEstatus <> 0 " + (notEntrega ? " and v.PedSecuencia is null " : "") + " " +
                "group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf " +
                "UNION  " +
                "select v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre, v.CliID as CliID, Round((Select Sum((VenCantidad) * (cast(VenPrecio as float) - (cast(VenPrecio as float) * VenDescPorciento/100) +  ( (cast(VenPrecio as float) - (cast(VenPrecio as float) * VenDescPorciento/100)) * VenItbis/100)     )) from VentasDetalleConfirmados vd  Inner Join Productos p on p.Proid=vd.Proid where repcodigo= v.repcodigo and vensecuencia=v.Vensecuencia),2) as VenTotal, " +
                "v.VenNCF as VenNCF, sum(((vd.VenPrecio + vd.VenSelectivo + vd.VenAdValorem - vd.VenDescuento) * vd.VenCantidad) + vd.venTotalitbis) as VenTotalUnitario " +
                " , sum(vd.venTotalitbis) as venTotalitbis, vd.VenDescuento as VenDescuento, vd.VenIndicadorOferta as VenIndicadorOferta " +
                "from VentasConfirmados v inner join VentasDetalleConfirmados vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID left outer join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and v.CuaSecuencia = " + cuaSecuencia + (forpago != "" ? " and v.Conid in ( " + forpago + " ) " : " and v.Conid = 1") + " and v.venEstatus <> 0 " + (notEntrega ? " and v.PedSecuencia is null " : "") + " " +
                "group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf", new string[] { });
        }

        //Obtener ventas a Contado ConID = 1 sin Itbis
        public List<Ventas> GetVentasaContadoByCuaSecuenciaSinItbis(int cuaSecuencia, int forpago = 1)
        {
            string query = "select vd.VenCantidad as VenCantidad, v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre, v.CliID as CliID, " +
                 "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float)))) + vd.venTotalitbis) " +
                " as VenTotal," +
                "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float))))) " +
                " as VenTotalSinItbis," +
                "v.VenNCF as VenNCF, SUM(((cast(vd.VenPrecio as float) * (cast(vd.VenCantidad as float) + (cast(ifnull(vd.VenCantidadDetalle, 0) as float) / cast(ifnull(p.ProUnidades, 1) as float)))))) as VenTotalUnitario " +
                " , sum(vd.venTotalitbis) as venTotalitbis, vd.VenDescuento as VenDescuento, vd.VenIndicadorOferta as VenIndicadorOferta " +
                "from Ventas v inner join VentasDetalle vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID inner join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and v.CuaSecuencia = " + cuaSecuencia + (forpago != 1 ? " and v.Conid = " + forpago : " and v.Conid = 1") + " and v.venEstatus <> 0 " +
                "group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf " +
                "UNION  " +
                "select vd.VenCantidad as VenCantidad, v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre, v.CliID as CliID, " +
                "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float)))) + vd.venTotalitbis) " +
                " as VenTotal," +
                "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float))))) " +
                " as VenTotalSinItbis," +
                "v.VenNCF as VenNCF, sum(vd.VenPrecio * vd.VenCantidad) as VenTotalUnitario " +
                " , sum(vd.venTotalitbis) as venTotalitbis, vd.VenDescuento as VenDescuento, vd.VenIndicadorOferta as VenIndicadorOferta " +
                "from VentasConfirmados v inner join VentasDetalleConfirmados vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID left outer join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and v.CuaSecuencia = " + cuaSecuencia + (forpago != 1 ? " and v.Conid = " + forpago : " and v.Conid = 1") + " and v.venEstatus <> 0 " +
                "group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf " +
                "Order By VenNCF";

            return SqliteManager.GetInstance().Query<Ventas>(query);
        }


        public List<Ventas> GetVentasaContadoByVenNCFSinItbis(int cuaSecuencia, int forpago = 1)
        {
            string query = "select vd.VenCantidad as VenCantidad, v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre, v.CliID as CliID, " +
                 "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float)))) + vd.venTotalitbis) " +
                " as VenTotal," +
                "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float))))) " +
                " as VenTotalSinItbis," +
                "v.VenNCF as VenNCF, SUM(((cast(vd.VenPrecio as float) * (cast(vd.VenCantidad as float) + (cast(ifnull(vd.VenCantidadDetalle, 0) as float) / cast(ifnull(p.ProUnidades, 1) as float)))))) as VenTotalUnitario " +
                " , sum(vd.venTotalitbis) as venTotalitbis, vd.VenDescuento as VenDescuento, vd.VenIndicadorOferta as VenIndicadorOferta " +
                "from Ventas v inner join VentasDetalle vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID inner join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and v.CuaSecuencia = " + cuaSecuencia + (forpago != 1 ? " and v.Conid = " + forpago : " and v.Conid = 1") + " and v.venEstatus <> 0 " +
                "group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf " +
                "UNION  " +
                "select vd.VenCantidad as VenCantidad, v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre, v.CliID as CliID, " +
                "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float)))) + vd.venTotalitbis) " +
                " as VenTotal," +
                "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float))))) " +
                " as VenTotalSinItbis," +
                "v.VenNCF as VenNCF, sum(vd.VenPrecio * vd.VenCantidad) as VenTotalUnitario " +
                " , sum(vd.venTotalitbis) as venTotalitbis, vd.VenDescuento as VenDescuento, vd.VenIndicadorOferta as VenIndicadorOferta " +
                "from VentasConfirmados v inner join VentasDetalleConfirmados vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID left outer join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and v.CuaSecuencia = " + cuaSecuencia + (forpago != 1 ? " and v.Conid = " + forpago : " and v.Conid = 1") + " and v.venEstatus <> 0 " +
                "group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf " +
                "Order By VenNCF";

            return SqliteManager.GetInstance().Query<Ventas>(query);
        }

        //Obtener ventas a Contado ConID = 1
        public List<VentasDetalle> GetVentasDetalleaContadoByCuaSecuencia(int cuaSecuencia, int Cliid, int forpago = 1)
        {
            return SqliteManager.GetInstance().Query<VentasDetalle>("select "+
                "  vd.VenPrecio as VenPrecio, vd.VenSelectivo as VenSelectivo, vd.VenAdValorem as VenAdValorem, vd.VenDescuento as VenDescuento, vd.VenCantidad as VenCantidad,  vd.VenCantidadDetalle as VenCantidadDetalle, p.ProUnidades as ProUnidades, sum(vd.venTotalitbis) as venTotalitbis, vd.VenItbis as VenItbis " +
                "from Ventas v inner join VentasDetalle vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID inner join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and v.CuaSecuencia = " + cuaSecuencia + (forpago != 1 ? " and v.Conid = " + forpago : " and v.Conid = 1") + " and v.venEstatus <> 0  and v.cliid = "+Cliid +" "+
                "UNION  " +
                "select "+
                 " vd.VenPrecio as VenPrecio, vd.VenSelectivo as VenSelectivo, vd.VenAdValorem as VenAdValorem, vd.VenDescuento as VenDescuento, vd.VenCantidad as VenCantidad,  vd.VenCantidadDetalle as VenCantidadDetalle, p.ProUnidades as ProUnidades, sum(vd.venTotalitbis) as venTotalitbis, vd.VenItbis as VenItbis " +
                "from VentasConfirmados v inner join VentasDetalleConfirmados vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID left outer join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and v.CuaSecuencia = " + cuaSecuencia + (forpago != 1 ? " and v.Conid = " + forpago : " and v.Conid = 1") + " and v.venEstatus <> 0  and v.cliid = "+ Cliid +" ", new string[] { });
        }

        //Obtener Ventas a Credito
        public List<Ventas> GetVentasaCreditoByCuaSecuencia(int cuaSecuencia, int forpago = 1 , bool notEntrega = false)
        {
            return SqliteManager.GetInstance().Query<Ventas>("select vd.VenCantidad AS VenCantidad, sum(vd.venTotalitbis) as venTotalitbis, v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre,v.CliID as CliID,v.VenReferencia, " +
                "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float)))) + vd.venTotalitbis) "+
                " as VenTotal," +
                "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float))))) " +
                " as VenTotalSinItbis," +
                " v.VenNCF as VenNCF, SUM(((cast(vd.VenPrecio as float) - cast(vd.VenDescuento as float))) * (cast(vd.VenCantidad as float) + (cast(ifnull(vd.VenCantidadDetalle, 0) as float) / cast(ifnull(p.ProUnidades, 1) as float)))) as VenTotalUnitario " +
                "from Ventas v inner join VentasDetalle vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID inner join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and v.CuaSecuencia = " + cuaSecuencia + (forpago != 1 ? " and v.Conid <> "+ forpago : " and v.Conid <> 1" ) + " and v.venEstatus <> 0 " +(notEntrega ? " and v.PedSecuencia is null " : "") + " " +
                " group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf " +
                "UNION  " +
                "select vd.VenCantidad AS VenCantidad, sum(vd.venTotalitbis) as venTotalitbis, v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre,v.CliID as CliID,v.VenReferencia, " +
                "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float)))) + vd.venTotalitbis) " +
                " as VenTotal," +
                "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float))))) " +
                " as VenTotalSinItbis," +
                "v.VenNCF as VenNCF, sum(((vd.VenPrecio + vd.VenSelectivo + vd.VenAdValorem - vd.VenDescuento) * vd.VenCantidad) + vd.venTotalitbis) as VenTotalUnitario " +
                "from VentasConfirmados v inner join VentasDetalleConfirmados vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID left outer join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and v.CuaSecuencia = " + cuaSecuencia + (forpago != 1 ? " and v.Conid <> " + forpago : " and v.Conid <> 1") + " and v.venEstatus <> 0 " + (notEntrega ? " and v.PedSecuencia is null " : "") + " " +
                "group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf", new string[] { });
        }

        public List<Ventas> GetVentasaCreditoByVenNCF(int cuaSecuencia, int forpago = 1, bool notEntrega = false)
        {
            return SqliteManager.GetInstance().Query<Ventas>("select vd.VenCantidad AS VenCantidad, sum(vd.venTotalitbis) as venTotalitbis, v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre,v.CliID as CliID,v.VenReferencia, " +
                "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float)))) + vd.venTotalitbis) " +
                " as VenTotal," +
                "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float))))) " +
                " as VenTotalSinItbis," +
                " v.VenNCF as VenNCF, SUM(((cast(vd.VenPrecio as float) - cast(vd.VenDescuento as float))) * (cast(vd.VenCantidad as float) + (cast(ifnull(vd.VenCantidadDetalle, 0) as float) / cast(ifnull(p.ProUnidades, 1) as float)))) as VenTotalUnitario " +
                "from Ventas v inner join VentasDetalle vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID inner join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and v.CuaSecuencia = " + cuaSecuencia + (forpago != 1 ? " and v.Conid <> " + forpago : " and v.Conid <> 1") + " and v.venEstatus <> 0 " + (notEntrega ? " and v.PedSecuencia is null " : "") + " " +
                " group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf " +
                "UNION  " +
                "select vd.VenCantidad AS VenCantidad, sum(vd.venTotalitbis) as venTotalitbis, v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre,v.CliID as CliID,v.VenReferencia, " +
                "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float)))) + vd.venTotalitbis) " +
                " as VenTotal," +
                "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float))))) " +
                " as VenTotalSinItbis," +
                "v.VenNCF as VenNCF, sum(((vd.VenPrecio + vd.VenSelectivo + vd.VenAdValorem - vd.VenDescuento) * vd.VenCantidad) + vd.venTotalitbis) as VenTotalUnitario " +
                "from VentasConfirmados v inner join VentasDetalleConfirmados vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID left outer join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and v.CuaSecuencia = " + cuaSecuencia + (forpago != 1 ? " and v.Conid <> " + forpago : " and v.Conid <> 1") + " and v.venEstatus <> 0 " + (notEntrega ? " and v.PedSecuencia is null " : "") + " " +
                "group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf " +
                "ORDER BY venncf", new string[] { });
        }

        public List<Ventas> GetVentasaCreditoByCuaSecuenciaConVariasFormaPagoCalculado(int cuaSecuencia, string forpago = "", bool notEntrega = false)
        {
            return SqliteManager.GetInstance().Query<Ventas>("select v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre, " +
                "Round((Select Sum((VenCantidad) * (cast(VenPrecio as float) - (cast(VenPrecio as float) * VenDescPorciento/100) +  ( (cast(VenPrecio as float) - (cast(VenPrecio as float) * VenDescPorciento/100)) * VenItbis/100)     )) from VentasDetalle vd  Inner Join Productos p on p.Proid=vd.Proid where repcodigo= v.repcodigo and vensecuencia=v.Vensecuencia),2)  " +
                " as VenTotal, v.VenNCF as VenNCF, SUM(((cast(vd.VenPrecio as float) - cast(vd.VenDescuento as float))) * (cast(vd.VenCantidad as float) + (cast(ifnull(vd.VenCantidadDetalle, 0) as float) / cast(ifnull(p.ProUnidades, 1) as float)))) as VenTotalUnitario " +
                "from Ventas v inner join VentasDetalle vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID inner join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and v.CuaSecuencia = " + cuaSecuencia + (forpago != "" ? " and v.Conid not in ( " + forpago + " ) " : " and v.Conid <> 1") + " and v.venEstatus <> 0 " + (notEntrega ? " and v.PedSecuencia is null " : "") + " " +
                " group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf " +
                "UNION  " +
                "select v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre, Round((Select Sum((VenCantidad) * (cast(VenPrecio as float) - (cast(VenPrecio as float) * VenDescPorciento/100) +  ( (cast(VenPrecio as float) - (cast(VenPrecio as float) * VenDescPorciento/100)) * VenItbis/100)     )) from VentasDetalleConfirmados vd  Inner Join Productos p on p.Proid=vd.Proid where repcodigo= v.repcodigo and vensecuencia=v.Vensecuencia),2) as VenTotal, " +
                "v.VenNCF as VenNCF, sum(((vd.VenPrecio + vd.VenSelectivo + vd.VenAdValorem - vd.VenDescuento) * vd.VenCantidad) + vd.venTotalitbis) as VenTotalUnitario " +
                "from VentasConfirmados v inner join VentasDetalleConfirmados vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID left outer join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and v.CuaSecuencia = " + cuaSecuencia + (forpago != "" ? " and v.Conid not in ( " + forpago+ " ) " : " and v.Conid <> 1") + " and v.venEstatus <> 0 " + (notEntrega ? " and v.PedSecuencia is null " : "") + " " +
                "group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf", new string[] { });
        }

        public List<Ventas> GetVentasaCreditoByCuaSecuenciaConVariasFormaPago(int cuaSecuencia, string forpago = "", bool notEntrega = false)
        {
            return SqliteManager.GetInstance().Query<Ventas>("select v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre, " +
                "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float)))) + vd.venTotalitbis) " +
                " as VenTotal, v.VenNCF as VenNCF, SUM(((cast(vd.VenPrecio as float) - cast(vd.VenDescuento as float))) * (cast(vd.VenCantidad as float) + (cast(ifnull(vd.VenCantidadDetalle, 0) as float) / cast(ifnull(p.ProUnidades, 1) as float)))) as VenTotalUnitario " +
                "from Ventas v inner join VentasDetalle vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID inner join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and v.CuaSecuencia = " + cuaSecuencia + (forpago != "" ? " and v.Conid not in ( " + forpago + " ) " : " and v.Conid <> 1") + " and v.venEstatus <> 0 " + (notEntrega ? " and v.PedSecuencia is null " : "") + " " +
                " group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf " +
                "UNION  " +
                "select v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre, sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float)))) + vd.venTotalitbis) as VenTotal, " +
                "v.VenNCF as VenNCF, sum(((vd.VenPrecio + vd.VenSelectivo + vd.VenAdValorem - vd.VenDescuento) * vd.VenCantidad) + vd.venTotalitbis) as VenTotalUnitario " +
                "from VentasConfirmados v inner join VentasDetalleConfirmados vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID left outer join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and v.CuaSecuencia = " + cuaSecuencia + (forpago != "" ? " and v.Conid not in ( " + forpago + " ) " : " and v.Conid <> 1") + " and v.venEstatus <> 0 " + (notEntrega ? " and v.PedSecuencia is null " : "") + " " +
                "group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf", new string[] { });
        }

        public List<Ventas> GetVentasaCreditoByfecha(int cuaSecuencia, int forpago = 1, bool notEntrega = false)
        {
            return SqliteManager.GetInstance().Query<Ventas>("select v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre, cli.CliCodigo, " +
                "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float)))) + vd.venTotalitbis) " +
                " as VenTotal, v.VenNCF as VenNCF, SUM(((cast(vd.VenPrecio as float) - cast(vd.VenDescuento as float))) * (cast(vd.VenCantidad as float) + (cast(ifnull(vd.VenCantidadDetalle, 0) as float) / cast(ifnull(p.ProUnidades, 1) as float)))) as VenTotalUnitario " +
                "from Ventas v inner join VentasDetalle vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID inner join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and v.CuaSecuencia = " + cuaSecuencia + (forpago != 1 ? " and v.Conid <> " + forpago : " and v.Conid <> 1") + " and v.venEstatus <> 0 " + (notEntrega ? " and v.PedSecuencia is null " : "") + " " +
                //"where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and strftime('%Y-%m-%d',v.venfecha) =  date('now')  and v.Conid <> 0 and v.venEstatus <> 0 " + (notEntrega ? " and v.PedSecuencia is null " : "") + " " +
                " group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf " +
                "UNION  " +
                "select v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre, cli.CliCodigo,  sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float)))) + vd.venTotalitbis) as VenTotal, " +
                "v.VenNCF as VenNCF, sum(((vd.VenPrecio + vd.VenSelectivo + vd.VenAdValorem - vd.VenDescuento) * vd.VenCantidad) + vd.venTotalitbis) as VenTotalUnitario " +
                "from VentasConfirmados v inner join VentasDetalleConfirmados vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID left outer join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and v.CuaSecuencia = " + cuaSecuencia + (forpago != 1 ? " and v.Conid <> " + forpago : " and v.Conid <> 1") + " and v.venEstatus <> 0 " + (notEntrega ? " and v.PedSecuencia is null " : "") + " " +
                //"where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and strftime('%Y-%m-%d',v.venfecha) =  date('now')  and v.Conid <> 0 and v.venEstatus <> 0 " + (notEntrega ? " and v.PedSecuencia is null " : "") + " " +
                "group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf", new string[] { });
        }

        //Obtener Ventas a Credito sin Itbis
        public List<Ventas> GetVentasaCreditoByCuaSecuenciaSinItbis(int cuaSecuencia, int forpago = 1)
        {
            return SqliteManager.GetInstance().Query<Ventas>("select v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre, " +
                "sum((cast(vd.VenPrecio as float)  * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float))))) " +
                " as VenTotal, SUM(vd.venTotalitbis) as VenTotalItbis, v.VenNCF as VenNCF, SUM(((cast(vd.VenPrecio as float) - cast(vd.VenDescuento as float))) * (cast(vd.VenCantidad as float) + (cast(ifnull(vd.VenCantidadDetalle, 0) as float) / cast(ifnull(p.ProUnidades, 1) as float)))) as VenTotalUnitario " +
                "from Ventas v inner join VentasDetalle vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID inner join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and v.CuaSecuencia = " + cuaSecuencia + (forpago != 1 ? " and v.Conid <> " + forpago : " and v.Conid <> 1") + " and v.venEstatus <> 0 " +
                "group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf " +
                "UNION  " +
                "select v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre, sum((cast(vd.VenPrecio as float) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float))))) as VenTotal, " +
                "v.VenNCF as VenNCF, SUM(vd.venTotalitbis) as VenTotalItbis, sum(vd.VenPrecio * vd.VenCantidad) as VenTotalUnitario " +
                "from VentasConfirmados v inner join VentasDetalleConfirmados vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID left outer join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and v.CuaSecuencia = " + cuaSecuencia + (forpago != 1 ? " and v.Conid <> " + forpago : " and v.Conid <> 1") + " and v.venEstatus <> 0 " +
                "group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf", new string[] { });
        }

        public List<Ventas> getEntregasRealizadas(int cuaSecuencia)
        {
            return SqliteManager.GetInstance().Query<Ventas>("select distinct v.CliId, c.CliCodigo, c.CliNombre, " +
            "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) " +
            "+ (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float)))) + vd.venTotalitbis) as VenTotal " +
            "from Ventas v " +
            "inner join Productos p on p.ProID = vd.ProID "+
            "inner join VentasDetalle vd on v.VenSecuencia = vd.VenSecuencia " +
            "inner join EntregasRepartidorTransacciones e on e.TraSecuencia = v.PedSecuencia and e.enrEstatusEntrega = 2 " +
            "and e.CliID = v.CliID " +
            "inner join Clientes c on v.CliID =c.CliID " +
            "and v.Repcodigo = '"+Arguments.CurrentUser.RepCodigo+"' and v.Cuasecuencia = "+cuaSecuencia+ " and VenindicadorOferta = 0 " +
            "and vd.proid in (select ed.proid from EntregasRepartidorTransaccionesDetalle ed where ed.EnrSecuencia = e.EnrSecuencia and ed.TraSecuencia = e.TraSecuencia) " +
            "group by v.cliid, c.CliCodigo, c.CliNombre", new string[] { });
        }

        public List<EntregasRepartidorTransaccionesDetalle> getEntregasRealizadasByPedido(int PedSecuencia, int ProID)
        {
            return SqliteManager.GetInstance().Query<EntregasRepartidorTransaccionesDetalle>("select TraCantidad " +
                " from EntregasRepartidorTransacciones e" +
                " Inner Join EntregasRepartidorTransaccionesDetalle ed on ed.EnrSecuencia=e.EnrSecuencia and e.RepCodigo =ed.RepCodigo" +
                " Where e.TraSecuencia= '"+ PedSecuencia + "' and ed.TraSecuencia= '" + PedSecuencia + "' and ProID = '" + ProID + "' and TraIndicadorOferta = 0  and EnrEstatusEntrega = 1 " +
                " and e.TitID = 1 and ltrim(rtrim(e.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' ", new string[] { });
        }

        public List<EntregasRepartidorTransacciones> getEntregasRealizadasByPedidoGeneral(int PedSecuencia)
        {
            return SqliteManager.GetInstance().Query<EntregasRepartidorTransacciones>("select EnrSecuencia, TraSecuencia " +
                " from EntregasRepartidorTransacciones e" +
                " Where e.TraSecuencia= '" + PedSecuencia + "' and EnrEstatusEntrega = 1 " +
                " and e.TitID = 1 and ltrim(rtrim(e.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' ", new string[] { });
        }

        public List<VentasDetalle> getProductosEntregasRealizadas(int cuaSecuencia)
        {
            return SqliteManager.GetInstance().Query<VentasDetalle>(
                "select distinct vd.proid, ProDescripcion, vd.VenCantidad as VenCantidad from VentasDetalle vd " +
                "inner join Productos p on p.ProID = vd.ProID "+
                "inner join Ventas v on v.VenSecuencia = vd.VenSecuencia "+
                "inner join EntregasRepartidorTransacciones e on e.TraSecuencia = v.PedSecuencia and e.enrEstatusEntrega = 2 " +
                "and v.Repcodigo = '"+Arguments.CurrentUser.RepCodigo+"' and v.Cuasecuencia = "+cuaSecuencia+ " and VenindicadorOferta = 0 " +
                "and vd.proid in (select ed.proid from EntregasRepartidorTransaccionesDetalle ed where ed.EnrSecuencia = e.EnrSecuencia and ed.TraSecuencia = e.TraSecuencia) ", new string[] { });
        }

        public List<VentasDetalle> getProductosVendidos(string RepCodigo, int CuaSecuencia)
        {
            return SqliteManager.GetInstance().Query<VentasDetalle>("select ProDatos3, vd.ProID as ProID, p.ProCodigo as ProCodigo, " +
                "p.ProDescripcion as ProDescripcion, sum(vd.VenCantidad) as VenCantidad, sum(vd.VenCantidadDetalle) as VenCantidadDetalle " +
                "from Ventas v inner join VentasDetalle vd on v.repcodigo = vd.repcodigo and v.VenSecuencia = vd.VenSecuencia " +
                "inner join Productos p on p.ProID = vd.ProID where v.VenEstatus != 0 AND CuaSecuencia = ? and ltrim(rtrim(v.RepCodigo)) = ? and VenIndicadorOferta <> 1 " +
                "Group by vd.proid, p.proCodigo, p.ProDescripcion " +
                "UNION " +
                "select ProDatos3, vd.ProID as ProID, p.ProCodigo as ProCodigo, " +
                "p.ProDescripcion as ProDescripcion, sum(vd.VenCantidad) as VenCantidad, sum(vd.VenCantidadDetalle) as VenCantidadDetalle " +
                "from VentasConfirmados v inner join VentasDetalleConfirmados vd on v.repcodigo = vd.repcodigo and v.VenSecuencia = vd.VenSecuencia " +
                "inner join Productos p on p.ProID = vd.ProID where v.VenEstatus != 0 AND CuaSecuencia = ? and ltrim(rtrim(v.RepCodigo)) = ? and VenIndicadorOferta <> 1 " +
                "Group by vd.proid, p.proCodigo, p.ProDescripcion ", new string[] { CuaSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim(), CuaSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public List<VentasDetalle> getProductosVendidosSinOferta(string RepCodigo, int CuaSecuencia)
        {
            return SqliteManager.GetInstance().Query<VentasDetalle>("sELECT r.ProID AS ProID, R.ProCodigo AS ProCodigo, R.ProDescripcion AS ProDescripcion, SUM(R.VenCantidad) AS VenCantidad, SUM(R.VenCantidadDetalle) AS VenCantidadDetalle FROM ( " +
                "select vd.ProID as ProID, p.ProUnidades as ProUnidades, p.ProCodigo as ProCodigo, " +
                "p.ProDescripcion as ProDescripcion, sum(vd.VenCantidad) as VenCantidad, sum(vd.VenCantidadDetalle) as VenCantidadDetalle " +
                "from Ventas v inner join VentasDetalle vd on v.repcodigo = vd.repcodigo and v.VenSecuencia = vd.VenSecuencia " +
                "inner join Productos p on p.ProID = vd.ProID where v.VenEstatus != 0 AND CuaSecuencia = ? and ltrim(rtrim(v.RepCodigo)) = ? and VenIndicadorOferta <> 1 " +
                "Group by vd.proid, p.proCodigo, p.ProDescripcion " +
                "UNION " +
                "select vd.ProID as ProID, p.ProUnidades as ProUnidades, p.ProCodigo as ProCodigo, " +
                "p.ProDescripcion as ProDescripcion, sum(vd.VenCantidad) as VenCantidad, sum(vd.VenCantidadDetalle) as VenCantidadDetalle " +
                "from VentasConfirmados v inner join VentasDetalleConfirmados vd on v.repcodigo = vd.repcodigo and v.VenSecuencia = vd.VenSecuencia " +
                "inner join Productos p on p.ProID = vd.ProID where v.VenEstatus != 0 AND CuaSecuencia = ? and ltrim(rtrim(v.RepCodigo)) = ? and VenIndicadorOferta <> 1 " +
                "Group by vd.proid, p.proCodigo, p.ProDescripcion ) AS R "+
                "Group by R.proid, R.proCodigo, R.ProDescripcion " , new string[] { CuaSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim(), CuaSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public List<VentasDetalle> getProductosVendidosOferta(string RepCodigo, int CuaSecuencia)
        {
            return SqliteManager.GetInstance().Query<VentasDetalle>("select vd.ProID as ProID, p.ProCodigo as ProCodigo, " +
                "p.ProDescripcion as ProDescripcion, sum(vd.VenCantidad) as VenCantidad, sum(vd.VenCantidadDetalle) as VenCantidadDetalle " +
                "from Ventas v inner join VentasDetalle vd on v.repcodigo = vd.repcodigo and v.VenSecuencia = vd.VenSecuencia " +
                "inner join Productos p on p.ProID = vd.ProID where v.VenEstatus != 0 AND CuaSecuencia = ? and ltrim(rtrim(v.RepCodigo)) = ? and VenIndicadorOferta = 1 " +
                "Group by vd.proid, p.proCodigo, p.ProDescripcion " +
                "UNION  " +
                "select vd.ProID as ProID, p.ProCodigo as ProCodigo, " +
                "p.ProDescripcion as ProDescripcion, sum(vd.VenCantidad) as VenCantidad, sum(vd.VenCantidadDetalle) as VenCantidadDetalle " +
                "from VentasConfirmados v inner join VentasDetalleConfirmados vd on v.repcodigo = vd.repcodigo and v.VenSecuencia = vd.VenSecuencia " +
                "inner join Productos p on p.ProID = vd.ProID where v.VenEstatus != 0 AND CuaSecuencia = ? and ltrim(rtrim(v.RepCodigo)) = ? and VenIndicadorOferta = 1 " +
                "Group by vd.proid, p.proCodigo, p.ProDescripcion " , new string[] { CuaSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim(), CuaSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public List<Ventas> getVentasAnuladas(string RepCodigo, int CuaSecuencia)
        {
            var sql = "select v.vensecuencia as VenSecuencia, SUM(vd.venTotalitbis) as VenTotalItbis, ifnull(clinombre, 'Cliente Suprimido') as CliNombre, " +
                "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float)))) + vd.venTotalitbis) " +
                " as VenTotal," +
                "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float))))) " +
                " as VenTotalSinItbis, " +
                "v.VenNCF as VenNCF, SUM(((cast(vd.VenPrecio as float) - cast(vd.VenDescuento as float))) * (cast(vd.VenCantidad as float) + (cast(ifnull(vd.VenCantidadDetalle, 0) as float) / cast(ifnull(p.ProUnidades, 1) as float)))) as VenTotalUnitario " +
                "from Ventas v inner join VentasDetalle vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID inner join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and v.CuaSecuencia = " + CuaSecuencia + " and v.VenEstatus = 0 " +
                "group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf " +
                "UNION ALL " +
                "select v.vensecuencia as VenSecuencia, sum(vd.venTotalitbis) as venTotalitbis, ifnull(clinombre, 'Cliente Suprimido') as CliNombre,  " +
                "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float)))) + vd.venTotalitbis) " +
                " as VenTotal," +
                "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float))))) " +
                " as VenTotalSinItbis," +
                "v.VenNCF as VenNCF, sum(((vd.VenPrecio + vd.VenSelectivo + vd.VenAdValorem - vd.VenDescuento) * vd.VenCantidad) + vd.venTotalitbis) as VenTotalUnitario " +
                "from VentasConfirmados v inner join VentasDetalleConfirmados vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID left outer join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and v.CuaSecuencia = " + CuaSecuencia + " and v.VenEstatus = 0 " +
                "group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf";

            return SqliteManager.GetInstance().Query<Ventas>(sql, new string[] { });

        }


        public List<VentasDetalle> getProductosVentasAnuladas(string RepCodigo, int CuaSecuencia)
        {
            return SqliteManager.GetInstance().Query<VentasDetalle>("select vd.ProID as ProID, p.ProCodigo as ProCodigo, " +
                "p.ProDescripcion as ProDescripcion, sum(vd.VenCantidad) as VenCantidad, sum(vd.VenCantidadDetalle) as VenCantidadDetalle " +
                "from Ventas v inner join VentasDetalle vd on v.repcodigo = vd.repcodigo and v.VenSecuencia = vd.VenSecuencia " +
                "inner join Productos p on p.ProID = vd.ProID where v.VenEstatus = 0 AND CuaSecuencia = ? and ltrim(rtrim(v.RepCodigo)) = ? " +
                "Group by vd.proid, p.proCodigo, p.ProDescripcion " +
                "order by ProDescripcion", new string[] { CuaSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

        }

        public List<Compras> getPushmoney(string RepCodigo, int CuaSecuencia)
        {
            return SqliteManager.GetInstance().Query<Compras>("select result.ComSecuencia , result.cliNombre as CliNombre, sum(result.ComTotal) as ComTotal, result.ComTipoPago as ComTipoPago, result.TipoPagoDescripcion as TipoPagoDescripcion from ( " +
                "select co.Comsecuencia as ComSecuencia, ifnull(clinombre,'Cliente Suprimido') as CliNombre, "
                + "sum(((cd.ComPrecio + cd.ComSelectivo + ifnull(cd.ComAdValorem, 0) - cd.ComDescuento)*cd.ComCantidad)+cd.ComTotalitbis) as ComTotal, co.ComTipoPago as ComTipoPago, substr(fp.fopDescripcion,0,3) as TipoPagoDescripcion " +
                "from Compras co inner join ComprasDetalle cd on co.repcodigo = cd.repcodigo " +
                "and co.Comsecuencia = cd.Comsecuencia left outer join clientes c on c.cliid = co.cliid " +
                "inner join FormasPago fp on fp.fopID = co.ComTipoPago " +
                "where co.CuaSecuencia = " + CuaSecuencia + " and co.RepCodigo = '" + RepCodigo + "' and co.ComEstatus <> 0 group by co.Comsecuencia, CliNombre " +
                "Union " +
                "select co.Comsecuencia as ComSecuencia, ifnull(clinombre,'Cliente Suprimido') as CliNombre, "
                + "sum(((cd.ComPrecio + cd.ComSelectivo + ifnull(cd.ComAdValorem, 0) - cd.ComDescuento)*cd.ComCantidad)+cd.ComTotalitbis) as ComTotal, co.ComTipoPago as ComTipoPago, substr(fp.fopDescripcion,0,3) as TipoPagoDescripcion " +
                "from ComprasConfirmados co inner join ComprasDetalleConfirmados cd on co.repcodigo = cd.repcodigo " +
                "and co.Comsecuencia = cd.Comsecuencia left outer join clientes c on c.cliid = co.cliid " +
                "inner join FormasPago fp on fp.fopID = co.ComTipoPago " +
                "where co.CuaSecuencia = " + CuaSecuencia + " and co.RepCodigo = '" + RepCodigo + "'  and co.ComEstatus <> 0 group by co.Comsecuencia, CliNombre "
                + " ) as result GROUP BY result.ComSecuencia, result.CliNombre ", new string[] { });

        }

        public List<ComprasDetalle> getCanjeCajetillas(string RepCodigo, int CuaSecuencia)
        {
            return SqliteManager.GetInstance().Query<ComprasDetalle>("select result.proCodigo as Procodigo, result.proNombre as proNombre, sum(result.comCantidad) as ComCantidad from ( " +
            "select p.Procodigo as Procodigo, p.ProDescripcion as ProNombre, sum(cd.Comcantidad) as ComCantidad " +
            "from Compras co inner join ComprasDetalle cd on co.repcodigo = cd.repcodigo and co.Comsecuencia = cd.Comsecuencia " +
            "left outer join productos p on p.Proid = cd.proid where " +
            "co.CuaSecuencia = " + CuaSecuencia + "  and co.RepCodigo = '" + RepCodigo + "' and co.ComEstatus <> 0 "
            + "group by p.Procodigo, p.ProDescripcion " +
            "Union  " +
            " select p.Procodigo as Procodigo, p.ProDescripcion as proNombre, sum(cd.Comcantidad) as ComCantidad from ComprasConfirmados co " +
            "inner join ComprasDetalleConfirmados cd on co.repcodigo = cd.repcodigo and co.Comsecuencia = cd.Comsecuencia left outer join productos p on p.Proid = cd.proid  " +
            "where co.CuaSecuencia = " + CuaSecuencia + "  and co.RepCodigo = '" + RepCodigo + "' and co.ComEstatus <> 0 group by p.Procodigo, p.ProDescripcion) as result " +
            "GROUP BY result.proCodigo, result.proNombre ", new string[] { });
        }


        public List<Recibos> getChequesDevueltos(string RepCodigo, int CuaSecuencia)
        {
            return SqliteManager.GetInstance().Query<Recibos>(
        "select r.recSecuencia, ra.recValor, ra.cxcDocumento, ra.cxcReferencia, ifnull(r.RecNumero,'') as RecNumero from recibos as r inner join recibosaplicacion as ra "
        + "on r.recsecuencia = ra.recSecuencia "
        + "where ra.cxcsigla = 'CKD' and r.cuasecuencia in(select cuasecuencia from cuadres where cuasecuencia = " + CuaSecuencia + ")", new string[] { });
        }

        public List<Recibos> getDocumentosRec(string RepCodigo, int CuaSecuencia)
        {
            return SqliteManager.GetInstance().Query<Recibos>(
        "select Cli.CliCodigo, ifnull(Cli.CliNombre,'Cliente Suprimido') as CliNombre, Rec.RecSecuencia as RecSecuencia, Rec.RecTotal, Rec.RecTipo, Rec.RecMontoNcr as RecMontoNcr, "
        + "Rec.RecMontoDescuento as RecMontoDescuento, Rec.RecMontoEfectivo as RecMontoEfectivo, "
        + "Rec.RecMontoCheque as RecMontoCheque, Rec.RecMontoChequeF as RecMontoChequeF, "
        + "Rec.RecMontoTransferencia as RecMontoTransferencia, Rec.RecMontoSobrante as RecMontoSobrante, Rec.RecRetencion as RecRetencion, ra.cxcReferencia as cxcReferencia, Rec.RecTipo as RecTipo  "
        + " from Recibos Rec inner join Clientes Cli on Rec.CliID = Cli.CliID " +
          "left join RecibosAplicacion ra on ra.RecSecuencia = Rec.RecSecuencia " +
              "where Rec.RecEstatus <> 0 AND Rec.cuaSecuencia = " + CuaSecuencia + " AND Rec.RepCodigo = '" + RepCodigo + "' and Rec.RecTipo = 2 Group by rec.recsecuencia " +
        "Union All " +
        "select Cli.CliCodigo, ifnull(Cli.CliNombre,'Cliente Suprimido') as CliNombre, Rec.RecSecuencia as RecSecuencia, Rec.RecTotal, Rec.RecTipo, Rec.RecMontoNcr as RecMontoNcr, "
        + "Rec.RecMontoDescuento as RecMontoDescuento, Rec.RecMontoEfectivo as RecMontoEfectivo, "
        + "Rec.RecMontoCheque as RecMontoCheque, Rec.RecMontoChequeF as RecMontoChequeF, "
        + "Rec.RecMontoTransferencia as RecMontoTransferencia, Rec.RecMontoSobrante as RecMontoSobrante, Rec.RecRetencion as RecRetencion, ra.cxcReferencia as cxcReferencia, Rec.RecTipo as RecTipo  "
        + " from RecibosConfirmados Rec inner join Clientes Cli on Rec.CliID = Cli.CliID " +
          "left join RecibosAplicacionConfirmados ra on ra.RecSecuencia = Rec.RecSecuencia " +
        "where Rec.RecEstatus <> 0 AND Rec.cuaSecuencia = " + CuaSecuencia + " AND Rec.RepCodigo = '" + RepCodigo + "' and Rec.RecTipo = 2  Group by rec.recsecuencia");
        }

        public List<Recibos> getDocumentosRecSinFactura(string RepCodigo, int CuaSecuencia)
        {
            return SqliteManager.GetInstance().Query<Recibos>(
        "select Cli.CliCodigo, ifnull(Cli.CliNombre,'Cliente Suprimido') as CliNombre, Rec.RecSecuencia as RecSecuencia, Rec.RecTotal, Rec.RecTipo, Rec.RecMontoNcr as RecMontoNcr, "
        + "Rec.RecMontoDescuento as RecMontoDescuento, Rec.RecMontoEfectivo as RecMontoEfectivo, "
        + "Rec.RecMontoCheque as RecMontoCheque, Rec.RecMontoChequeF as RecMontoChequeF, "
        + "Rec.RecMontoTransferencia as RecMontoTransferencia, Rec.RecMontoSobrante as RecMontoSobrante, Rec.RecRetencion as RecRetencion, Rec.RecTipo as RecTipo  "
        + " from Recibos Rec inner join Clientes Cli on Rec.CliID = Cli.CliID " +
              "where Rec.RecEstatus <> 0 AND Rec.cuaSecuencia = " + CuaSecuencia + " AND Rec.RepCodigo = '" + RepCodigo + "' and Rec.RecTipo = 2 " +
        "Union All " +
        "select Cli.CliCodigo, ifnull(Cli.CliNombre,'Cliente Suprimido') as CliNombre, Rec.RecSecuencia as RecSecuencia, Rec.RecTotal, Rec.RecTipo, Rec.RecMontoNcr as RecMontoNcr, "
        + "Rec.RecMontoDescuento as RecMontoDescuento, Rec.RecMontoEfectivo as RecMontoEfectivo, "
        + "Rec.RecMontoCheque as RecMontoCheque, Rec.RecMontoChequeF as RecMontoChequeF, "
        + "Rec.RecMontoTransferencia as RecMontoTransferencia, Rec.RecMontoSobrante as RecMontoSobrante, Rec.RecRetencion as RecRetencion, Rec.RecTipo as RecTipo  "
        + " from RecibosConfirmados Rec inner join Clientes Cli on Rec.CliID = Cli.CliID " +
        "where Rec.RecEstatus <> 0 AND Rec.cuaSecuencia = " + CuaSecuencia + " AND Rec.RepCodigo = '" + RepCodigo + "' and Rec.RecTipo = 2 ", new string[] { });
        }

        public string getFormasPago(string cxcReferencia)
        {
            var FormasPago = SqliteManager.GetInstance().Query<RecibosFormaPago>("Select FP.ForID, ifnull(FP.RefIndicadorDiferido, 0) as RefIndicadorDiferido "
                + "from RecibosFormaPago FP inner join RecibosAplicacion RA on FP.RecSecuencia = RA.RecSecuencia "
                + "Where RA.cxcReferencia = '" + cxcReferencia + "'", new string[] { });


            string FormaPago = "";

            foreach (var FP in FormasPago)
            {
                switch (FP.ForID)
                {
                    case 1:
                        if (FormaPago == "")
                        {
                            FormaPago += "EF";
                        }
                        else
                        {
                            FormaPago += "/EF";
                        }
                        break;
                    case 2:
                        if (FormaPago == "")
                        {
                            FormaPago += "CK";
                        }
                        else
                        {
                            FormaPago += "/CK";
                        }

                        break;
                    case 3:
                        if (FormaPago == "")
                        {
                            FormaPago += "NC";
                        }
                        else
                        {
                            FormaPago += "/NC";
                        }

                        break;
                    case 4:
                        if (FormaPago == "")
                        {
                            FormaPago += "TR";
                        }
                        else
                        {
                            FormaPago += "/TR";
                        }

                        break;
                    case 5:
                        if (FormaPago == "")
                        {
                            FormaPago += "RE";
                        }
                        else
                        {
                            FormaPago += "/RE";
                        }

                        break;
                    case 6:
                        if (FormaPago == "")
                        {
                            FormaPago += "TC";
                        }
                        else
                        {
                            FormaPago += "/TC";
                        }

                        break;
                    case 18:
                        if (FormaPago == "")
                        {
                            FormaPago += "OP";
                        }
                        else
                        {
                            FormaPago += "/OP";
                        }

                        break;
                }
            }
            return FormaPago;
        }


        public List<Recibos> getRecibosCreditoByCuaSecuencia2(string RepCodigo, int CuaSecuencia)
        {
            return SqliteManager.GetInstance().Query<Recibos>(
            "select Cli.CliCodigo, Cli.CliNombre, Rec.RecSecuencia as RecSecuencia, Rec.RecTotal, Rec.RecTipo, Rec.RecMontoNcr as RecMontoNcr, "
            + "Rec.RecMontoDescuento as RecMontoDescuento, Rec.RecMontoEfectivo as RecMontoEfectivo, "
            + "Rec.RecMontoCheque as RecMontoCheque, Rec.RecMontoChequeF as RecMontoChequeF, "
            + "Rec.RecMontoTransferencia as RecMontoTransferencia, Rec.RecMontoSobrante as RecMontoSobrante, Rec.RecRetencion as RecRetencion, Rec.RecMontoTarjeta as RecMontoTarjeta  "
            + " from Recibos Rec inner join Clientes Cli on Rec.CliID = Cli.CliID " +
              "where Rec.RecEstatus <> 0 AND Rec.cuaSecuencia = " + CuaSecuencia + " AND Rec.RepCodigo = '" + RepCodigo + "' " +
              "Union all " +
              "select Cli.CliCodigo, Cli.CliNombre, Rec.RecSecuencia as RecSecuencia, Rec.RecTotal, Rec.RecTipo, Rec.RecMontoNcr as RecMontoNcr, "
            + "Rec.RecMontoDescuento as RecMontoDescuento, Rec.RecMontoEfectivo as RecMontoEfectivo, "
            + "Rec.RecMontoCheque as RecMontoCheque, Rec.RecMontoChequeF as RecMontoChequeF, "
            + "Rec.RecMontoTransferencia as RecMontoTransferencia, Rec.RecMontoSobrante as RecMontoSobrante, Rec.RecRetencion as RecRetencion, Rec.RecMontoTarjeta as RecMontoTarjeta  "
            + " from RecibosConfirmados Rec inner join Clientes Cli on Rec.CliID = Cli.CliID " +
              "where Rec.RecEstatus <> 0 AND Rec.cuaSecuencia = " + CuaSecuencia + " AND Rec.RepCodigo = '" + RepCodigo + "' ", new string[] { });


        }

        public ResumenCuadres getResumen(string RepCodigo, int CuaSecuencia)
        {
            var Resumen = SqliteManager.GetInstance().Query<Cuadres>("select CuaFechaInicio, " +
            "CuaFechaFin, CuaSecuencia from Cuadres where CuaSecuencia = " + CuaSecuencia + " " +
            "and RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "'", new string[] { });

            ResumenCuadres resumen = new ResumenCuadres();

            foreach (var cuadres in Resumen)
            {

                DateTime.TryParse(cuadres.CuaFechaInicio, out DateTime fecha);
                var algo = fecha.ToString("yyyy-MM-dd HH:mm tt");
                int dayNumber = (int)(fecha.DayOfWeek - 1);

                int CicloSemana = 4;


                int weekNumber = Functions.GetWeekOfMonth(fecha);
                int parCicloSemana = myParametro.GetParCiclosSemanas();
                if (parCicloSemana > 0)
                {
                    CicloSemana = parCicloSemana;
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
                string semanaValue = new string(diasSemana);

                List<ResumenCuadres> CantidadClienteAVisitar = SqliteManager.GetInstance().Query<ResumenCuadres>("select Count(*) as mCantidadClientesAVisitar from rutaVisitas r "
                + "inner join clientes c on c.cliid = r.cliid "
                + "where RutSemana" + weekNumber + " like '" + semanaValue + "'", new string[] { });
               
                double cantidadclientesavisitar = CantidadClienteAVisitar[0].mCantidadClientesAVisitar;
                //Clientes a visitar
                resumen.mCantidadClientesAVisitar = Convert.ToInt32(cantidadclientesavisitar);
                //Clientes visitados
                resumen.mCantidadClientesVisitados = getCantClientesVisitadosPorCuadre(cuadres.CuaFechaInicio, cuadres.CuaFechaFin);
                //Visitas positivas
                resumen.mCantidadVisitasPositivas = getCantidadVisitasPositivasVentasByCuadre(CuaSecuencia, cuadres.CuaFechaInicio, cuadres.CuaFechaFin);

                double efectividad = 0;

                efectividad = ((double)resumen.mCantidadVisitasPositivas / (double)resumen.mCantidadClientesVisitados) * 100;
                //Efectividad
                resumen.mEfectividad = efectividad;
                //Facturas generadas
                resumen.mNumFacturasGeneradas = (getCantidadFacturasGeneradasByCuadre(CuaSecuencia));
                //resumen.mPromVentasPorVisitas = ((getTotalVentasByCuadre(CuaSecuencia, obj) / (double)resumen.mNumFacturasGeneradas;

                //Total tiempo ruta
                resumen.mTotalTiempoRuta = getTiempoTotalVisitasPorCuadre(cuadres.CuaFechaInicio, cuadres.CuaFechaFin)[1];

                string[] tiempoPromRuta = getTiempoTotalVisitasPorCuadre(cuadres.CuaFechaInicio, cuadres.CuaFechaFin);
                resumen.mTiempoPromRuta = (tiempoPromRuta[1]);

                long secondsPromVisitas;
                if (resumen.mCantidadClientesVisitados == 0)
                {
                    secondsPromVisitas = long.Parse(tiempoPromRuta[0]);
                }
                else
                {
                    secondsPromVisitas = long.Parse(tiempoPromRuta[0]) / (long)resumen.mCantidadClientesVisitados;
                }

                long hours = secondsPromVisitas / 3600;
                long minutes = (secondsPromVisitas % 3600) / 60;

                string promTimeRut = "";
                if (hours > 0)
                {
                    promTimeRut += hours + " h ";
                }
                promTimeRut += minutes + " m ";

                resumen.mTiempoPromVisitas = promTimeRut; //tiempototal entre negocios visitados

            }
            return resumen;

        }

        public ResumenCuadres GetResumenByCua(string RepCodigo, int CuaSecuencia)
        {
            var Resumen = SqliteManager.GetInstance().Query<Cuadres>("select CuaFechaInicio, " +
            "CuaFechaFin, CuaSecuencia from Cuadres where CuaSecuencia = " + CuaSecuencia + " " +
            "and RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "'", new string[] { });

            ResumenCuadres resumen = new ResumenCuadres();

            foreach (var cuadres in Resumen)
            {

                DateTime.TryParse(cuadres.CuaFechaFin, out DateTime fecha);
                var algo = fecha.ToString("yyyy-MM-dd HH:mm tt");
                int dayNumber = (int)(fecha.DayOfWeek - 1);

                int CicloSemana = 4;


                int weekNumber = Functions.GetWeekOfMonth(fecha);
                int parCicloSemana = myParametro.GetParCiclosSemanas();
                if (parCicloSemana > 0)
                {
                    CicloSemana = parCicloSemana;
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
                string semanaValue = new string(diasSemana);

                List<ResumenCuadres> CantidadClienteAVisitar = SqliteManager.GetInstance().Query<ResumenCuadres>("select Count(*) as mCantidadClientesAVisitar from rutaVisitas r "
                + "inner join clientes c on c.cliid = r.cliid "
                + "where RutSemana" + weekNumber + " like '" + semanaValue + "'", new string[] { });

                double cantidadclientesavisitar = CantidadClienteAVisitar[0].mCantidadClientesAVisitar;
                //Clientes a visitar
                resumen.mCantidadClientesAVisitar = Convert.ToInt32(cantidadclientesavisitar);
                //Clientes visitados
                resumen.mCantidadClientesVisitados = getCantClientesVisitadosPorCuadre(cuadres.CuaFechaInicio, cuadres.CuaFechaFin);
                //Visitas positivas
                resumen.mCantidadVisitasPositivas = getCantidadVisitasPositivasVentasByCuadre(CuaSecuencia, cuadres.CuaFechaInicio, cuadres.CuaFechaFin);

                double efectividad = 0;

                efectividad = ((double)resumen.mCantidadVisitasPositivas / (double)resumen.mCantidadClientesVisitados) * 100;
                //Efectividad
                resumen.mEfectividad = efectividad;
                //Facturas generadas
                resumen.mNumFacturasGeneradas = (getCantidadFacturasGeneradasByCuadre(CuaSecuencia));
                //resumen.mPromVentasPorVisitas = ((getTotalVentasByCuadre(CuaSecuencia, obj) / (double)resumen.mNumFacturasGeneradas;

                //Total tiempo ruta
                resumen.mTotalTiempoRuta = getTiempoTotalVisitasPorCuadre(cuadres.CuaFechaInicio, cuadres.CuaFechaFin)[1];

                string[] tiempoPromRuta = getTiempoTotalVisitasPorCuadre(cuadres.CuaFechaInicio, cuadres.CuaFechaFin);
                resumen.mTiempoPromRuta = (tiempoPromRuta[1]);

                long secondsPromVisitas;
                if (resumen.mCantidadClientesVisitados == 0)
                {
                    secondsPromVisitas = long.Parse(tiempoPromRuta[0]);
                }
                else
                {
                    secondsPromVisitas = long.Parse(tiempoPromRuta[0]) / (long)resumen.mCantidadClientesVisitados;
                }

                long hours = secondsPromVisitas / 3600;
                long minutes = (secondsPromVisitas % 3600) / 60;

                string promTimeRut = "";
                if (hours > 0)
                {
                    promTimeRut += hours + " h ";
                }
                promTimeRut += minutes + " m ";

                resumen.mTiempoPromVisitas = promTimeRut; //tiempototal entre negocios visitados

            }
            return resumen;

        }



        public int getCantClientesVisitadosPorCuadre(string cuaFechaInicio, string cuaFechaFin)
        {

            
                var list = SqliteManager.GetInstance().Query<ResumenCuadres>("select count(*) as mCantidadClientesVisitados from ("
                            + "select DISTINCT Cliid from Visitas where VisFechaEntrada between '" + 
                            ((!cuaFechaInicio.Contains("T") && cuaFechaFin.Contains("T")) || (cuaFechaInicio.Contains("T") && !cuaFechaFin.Contains("T")) ? cuaFechaInicio.Replace("T", " ") + "' and '" + cuaFechaFin.Replace("T", " ") : cuaFechaInicio + "' and '" + cuaFechaFin) + "' "
                            + ") as tabla ", new string[] { });
            
            return (int)list[0].mCantidadClientesVisitados;
        }

        public int getCantidadVisitasPositivasVentasByCuadre(int CuaId, string cuaFechaInicio, string cuaFechaFin)
        {

            var list = SqliteManager.GetInstance().Query<ResumenCuadres>("select count(*) as mEfectividad from ("
                        + "select DISTINCT CliID from Visitas v where VisFechaEntrada between '" + ((!cuaFechaInicio.Contains("T") && cuaFechaFin.Contains("T")) || (cuaFechaInicio.Contains("T") && !cuaFechaFin.Contains("T")) ? cuaFechaInicio.Replace("T", " ") + "' and '" + cuaFechaFin.Replace("T", " ") : cuaFechaInicio + "' and '" + cuaFechaFin) + "' "
                        + "and (VisSecuencia in (select visSecuencia from Ventas ve where ve.VisSecuencia = v.VisSecuencia and CuaSecuencia = " + CuaId + " and VenEstatus <> 0)) or "
                        + "(VisSecuencia in (select visSecuencia from VentasConfirmados vc where vc.VisSecuencia = v.VisSecuencia and CuaSecuencia = " + CuaId + "))"
                        + ") as tabla ", new string[] { });

            return (int)list[0].mEfectividad;
        }

        public int getCantidadFacturasGeneradasByCuadre(int cuaId)
        {
            var list = SqliteManager.GetInstance().Query<ResumenCuadres>("select sum(r.mNumFacturasGeneradas) as mNumFacturasGeneradas from ( " 
                        + "select Count(VenSecuencia) as mNumFacturasGeneradas from Ventas v "
                        + "where CuaSecuencia = " + cuaId + " and RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' and VenEstatus <> 0 "
                        + "union select Count(VenSecuencia) as mNumFacturasGeneradas from VentasConfirmados where "
                        + "CuaSecuencia = " + cuaId + " and RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' and VenEstatus <> 0 ) as r ", new string[] { });

            return (int)list[0].mNumFacturasGeneradas;
        }

        public string[] getTiempoTotalVisitasPorCuadre(string cuaFechaInicio, string cuaFechaFin)
        {

            var list = SqliteManager.GetInstance().Query<ResumenCuadres>(""
                     + "select ifnull(cast((julianday(max(VisFechasalida))-julianday(min(VisFechaEntrada))) * 24 * 60 *60 As Integer), 0) as mEfectividad "
                     + "from Visitas where visFechaEntrada between '" + ((!cuaFechaInicio.Contains("T") && cuaFechaFin.Contains("T")) || (cuaFechaInicio.Contains("T") && !cuaFechaFin.Contains("T")) ? cuaFechaInicio.Replace("T", " ") + "' and '" + cuaFechaFin.Replace("T", " ") : cuaFechaInicio + "' and '" + cuaFechaFin) + "'");



            long segundos = (int)list[0].mEfectividad;

            long hours = segundos / 3600;
            long minutes = (segundos % 3600) / 60;
            //long seconds = segundos % 60;

            string time = "";
            if (hours > 0)
            {
                time += hours + " h ";
            }

            time += minutes + " m";

            return new string[] { segundos.ToString(), time };
        }

        public int getCantidadVentasByCuadre(int cuaId)
        {
            var list = SqliteManager.GetInstance().Query<ResumenCuadres>("select count(*) as mEfectividad from Ventas where " +
                "CuaSecuencia = " + cuaId + "", new string[] { });

            return (int)list[0].mEfectividad;
        }


        public void InsertarVentaInTemp(int VenSecuencia, bool confirmado, bool forEditing = false)
        {
            myProd.ClearTemp((int)Modules.VENTAS);

            var list = SqliteManager.GetInstance().Query<ProductosTemp>("select distinct " + ((int)Modules.VENTAS).ToString() + " as TitID, ( vd.VenCantidad - " + (forEditing ? "case ifnull(vd.venIndicadorOferta, 0) when 0 then ifnull(vd.VenCantidadOferta,0) else 0 end" : "0") + "  )  as Cantidad, vd.VenCantidadDetalle as CantidadDetalle, vd.rowguid as rowguid, vd.ProID as ProID, " + (forEditing ? "ifnull(vd.VenPrecioLista,vd.VenPrecio)" : "vd.VenPrecio") + " as Precio, " + (forEditing ? "ifnull(vd.VenPrecioLista,vd.VenPrecio)" : "vd.VenPrecio") + " as PrecioMoneda, " +
                "p.ProDescripcion as Descripcion, vd.VenItbis as Itbis, vd.venSelectivo as Selectivo, vd.venAdvalorem as Advalorem,  vd.venDescPorciento as DesPorciento, ifnull(vd.UnmCodigo, '') as UnmCodigo, " +
                "ifnull(vd.venIndicadorOferta, 0) as IndicadorOferta, vd.venDescuento as Descuento, p.ProCodigo as ProCodigo, case when vd.OfeID != 0 then 1 else 0 end as IndicadorOfertaForShow from " + (confirmado ? "VentasDetalleConfirmados" : "VentasDetalle") + " vd " +
                "inner join Productos p on p.ProID = vd.ProID where ltrim(rtrim(vd.RepCodigo)) = ? and vd.VenSecuencia = ? order by p.ProDescripcion", new string[] { Arguments.CurrentUser.RepCodigo.Trim(), VenSecuencia.ToString() });

            SqliteManager.GetInstance().InsertAll(list);

        }

        public List<VentasDetalle> GetProductosVentasAnuladas(int CuaSecuencia)
        {
            string query = "select p.ProCodigo as proCodigo, substr(trim(p.ProDescripcion),0,25) as ProDescripcion, sum(vd.VenCantidad) as venCantidad, p.UnmCodigo as UnmCodigo, sum(vd.VenCantidadDetalle) as VenCantidadDetalle from ventas v " +
                    "inner join ventasdetalle vd on vd.vensecuencia = v.vensecuencia and vd.repcodigo = v.repcodigo " +
                    "inner join productos p on p.ProID = vd.ProID "
                     + " where v.repcodigo = '" + Arguments.CurrentUser.RepCodigo + "' and v.CuaSecuencia = " + CuaSecuencia + " AND v.VenEstatus = 0 and vd.VenIndicadorOferta <> 1 "
                     + "GROUP BY p.procodigo,p.ProDescripcion, p.UnmCodigo "
                     + "union select p.ProCodigo as proCodigo, substr(trim(p.ProDescripcion),0,25) as ProDescripcion, sum(vd.VenCantidad) as venCantidad, p.UnmCodigo as UnmCodigo,  sum(vd.VenCantidadDetalle) as VenCantidadDetalle from ventasConfirmados v " +
                    "inner join ventasdetalleConfirmados vd on vd.vensecuencia = v.vensecuencia and vd.repcodigo = v.repcodigo " +
                    "inner join productos p on p.ProID = vd.ProID "
                     + " where v.repcodigo = '" + Arguments.CurrentUser.RepCodigo + "' and v.CuaSecuencia = " + CuaSecuencia + " AND v.VenEstatus = 0 and vd.VenIndicadorOferta <> 1 "
                     + "GROUP BY p.procodigo,p.ProDescripcion, p.UnmCodigo ";
            
            return SqliteManager.GetInstance().Query<VentasDetalle>(query,new string[] { });
        }

        public bool isVentaUltimoCuadre(int vensecuencia, int cantidadVentasPuedeAnular)
        {
            // encuentra el ultimo cuadre abierto
            try
            {

                var listcua = SqliteManager.GetInstance().Query<Cuadres>("SELECT CuaSecuencia FROM Cuadres "
                        + "INNER JOIN RepresentantesSecuencias ON RepSecuencia = CuaSecuencia and UPPER(trim(RepTabla)) = 'CUADRES' "
                        + "WHERE CuaEstatus = 1", new string[] { });

                if (listcua.Count>0)
                { //si el ultimo cuadre esta abierto
                    foreach (var i in listcua) {
                        var listVen = SqliteManager.GetInstance().Query<Ventas>("SELECT VenSecuencia FROM Ventas "
                                + "WHERE VenSecuencia = " + vensecuencia + " "
                                + "AND CuaSecuencia = " + i.CuaSecuencia + " AND VenSecuencia > "
                                + "(SELECT RepSecuencia - " + cantidadVentasPuedeAnular + " FROM "
                                + "RepresentantesSecuencias WHERE UPPER(TRIM(RepTabla)) = 'VENTAS')", new string[] { });

                        if (listVen.Count>0)
                        {
                            return true;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            return false;
        }

        public bool ExisteVentaHuerfana(out string monCodigo, out Clientes cliente, out int visSecuencia, out string secCodigo, out int VenSecuencia)
        {

            monCodigo = "";
            cliente = null;
            visSecuencia = -1;
            secCodigo = "";
            VenSecuencia = -1;

            try
            {
                if (myParametro.GetParVentaSaldarReciboAutomatico())
                {
                    return false;
                }

                var conIdContado = myParametro.GetParConIdFormaPagoContado();
                var conIdContado2 = myParametro.GetParSegundoConIdFormaPagoContado();

                var list = SqliteManager.GetInstance().Query<Ventas>("select * from Ventas v where RepCodigo = ? and (ConID = ? or ConID = ?) " +
                    "and not exists(select 1 from RecibosAplicacion where RepCodigo = ? and trim(RecTipo) in ('1', '3') " +
                    "and trim(CxcDocumento) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "-'||cast(v.VenSecuencia as TEXT)) " +
                    "and not exists(select 1 from RecibosAplicacionConfirmados where RepCodigo = ? and trim(RecTipo) in ('1', '3') " +
                    "and trim(CxcDocumento) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "-'||cast(v.VenSecuencia as TEXT)) " +
                    "and VenEstatus = 1 ",
                    new string[] { Arguments.CurrentUser.RepCodigo.Trim(), conIdContado.ToString(), conIdContado2.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

                var result = false;

                if (list != null && list.Count > 0)
                {
                    result = true;

                    monCodigo = list[0].MonCodigo;
                    visSecuencia = list[0].VisSecuencia;
                    secCodigo = list[0].SecCodigo;
                    VenSecuencia = list[0].VenSecuencia;

                    var venSecuencia = list[0].VenSecuencia;

                    cliente = new DS_Clientes().GetClienteById(list[0].CliID);

                    if (cliente == null)
                    {
                        return false;
                    }

                    var ncf = cliente.CliTipoComprobanteFAC == "99" ? venSecuencia + "-" + Arguments.CurrentUser.RepCodigo : list[0].VenNCF;

                    var cxcm = myCxCobrar.GetCuentaByReferencia(ncf, Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, 4);

                    if (cxcm == null)
                    {
                        var cxc = new Hash("CuentasxCobrar");
                        cxc.Add("CxcReferencia", ncf);
                        cxc.Add("CxcTipoTransaccion", 4);
                        cxc.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                        cxc.Add("CxcDias", 0);
                        cxc.Add("CxcSIGLA", "FAT");
                        cxc.Add("CliID", cliente.CliID);
                        cxc.Add("CxcFecha", Functions.CurrentDate());
                        cxc.Add("CxcDocumento", Arguments.CurrentUser.RepCodigo + "-" + venSecuencia);

                        var totales = GetTotalesFromVenta(venSecuencia);

                        cxc.Add("CxcBalance", totales.Total);
                        cxc.Add("CxcMontoSinItbis", totales.SubTotal);
                        cxc.Add("CxcMontoTotal", totales.Total);
                        cxc.Add("MonCodigo", monCodigo);
                        cxc.Add("AreaCtrlCredit", 0);
                        cxc.Add("CxcNotaCredito", 0);
                        cxc.Add("CXCNCF", "");
                        cxc.Add("rowguid", Guid.NewGuid().ToString());
                        cxc.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                        cxc.Add("CueFechaActualizacion", Functions.CurrentDate());
                        cxc.Add("CxcFechaVencimiento", myCxCobrar.GetCxcFechaVencimiento(conIdContado));
                        cxc.Add("ConID", conIdContado);
                        cxc.ExecuteInsert();

                        cxcm = new CuentasxCobrar();
                        cxcm.CxcBalance = totales.Total;
                        cxcm.CxcMontoSinItbis = totales.SubTotal;
                    }

                    var reciboToSave = new RecibosDocumentosTemp();
                    reciboToSave.FechaSinFormatear = Functions.CurrentDate();
                    reciboToSave.Fecha = Functions.CurrentDate("dd-MM-yyyy");
                    reciboToSave.Documento = Arguments.CurrentUser.RepCodigo + "-" + venSecuencia;
                    reciboToSave.Referencia = ncf;
                    reciboToSave.Sigla = "FAT";
                    reciboToSave.Aplicado = cxcm.CxcBalance;
                    reciboToSave.Descuento = 0;
                    reciboToSave.MontoTotal = cxcm.CxcBalance;
                    reciboToSave.Balance = cxcm.CxcBalance;
                    reciboToSave.Pendiente = 0;
                    reciboToSave.Estado = "Saldo";
                    reciboToSave.Credito = 0;
                    reciboToSave.FechaIngles = Functions.CurrentDate("MM-dd-yyyy");
                    reciboToSave.Origen = 1;
                    reciboToSave.MontoSinItbis = cxcm.CxcMontoSinItbis;
                    reciboToSave.DescPorciento = 0;
                    reciboToSave.AutSecuencia = 0;
                    reciboToSave.FechaDescuento = Functions.CurrentDate("dd-MM-yyyy");
                    reciboToSave.Dias = 0;
                    reciboToSave.DescuentoFactura = 0;
                    reciboToSave.Clasificacion = "";
                    reciboToSave.FechaVencimiento = Functions.CurrentDate();
                    reciboToSave.Retencion = 0;
                    reciboToSave.CXCNCF = "";
                    reciboToSave.cxcComentario = "";
                    reciboToSave.RecTipo = 1;

                    new DS_Recibos().ClearTemps();

                    SqliteManager.GetInstance().Insert(reciboToSave);

                   // Arguments.Values.CurrentIsReciboHuerfano = true;
                }

                return result;
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return false;
        }

        public Totales GetTotalesFromVenta(int venSecuencia)
        {
            var list = SqliteManager.GetInstance().Query<Totales>("select VenCantidad as Cantidad,  VenCantidadDetalle as CantidadDetalle, VenAdValorem as AdValoremU, VenSelectivo as SelectivoU, VenPrecio as Precio, VenDescuento as DescuentoU, VenItbis as ItbisT, p.ProUnidades, VenDescPorciento as DesPorciento " +
                " from VentasDetalle t " +
                " inner join Productos P on P.ProID = t.ProID " +
                " WHERE ifnull(VenIndicadorOferta, 0) = 0 and t.VenSecuencia = ? and t.RepCodigo = ? ", new string[] { venSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });


            double DescuentoOfertas = 0;

            var ofeDesc = SqliteManager.GetInstance().Query<Totales>("select sum(VenPrecio * VenCantidad) as DescuentoOfertas from VentasDetalle " +
                "where ifnull(VenIndicadorOferta, 0) = 1 and VenSecuencia = ? and RepCodigo = ? ", new string[] { venSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });

            double DecuentoTotal = 0.0;
            double PrecioTotal = 0.0;
            double ItebisTotal = 0.0;
            double SubTotal = 0.0;
            double Total = 0.0;
            double adValorem = 0.0;
            double selectivo = 0.0;
            double CantidadTotal = 0.0;

            foreach (var det in list)
            {
                double Descuentos;
                double Descuentos1;
                double AdValoremU = det.AdValoremU;
                double SelectivoU = det.SelectivoU;
                adValorem += AdValoremU;
                selectivo += SelectivoU;
                double PrecioLista = (det.Precio + AdValoremU + SelectivoU);
                PrecioLista = Math.Round(PrecioLista, 2, MidpointRounding.AwayFromZero);

                double CantidadDetalle = Convert.ToDouble(Convert.ToDecimal(det.CantidadDetalle));
                CantidadDetalle = Math.Round(CantidadDetalle, 2, MidpointRounding.AwayFromZero);
                double ProUnidades = Convert.ToDouble(Convert.ToDecimal(det.ProUnidades));
                double CantidadUnica = det.Cantidad;
                double CantidadReal = (CantidadDetalle > 0 ? (CantidadDetalle / ProUnidades) + CantidadUnica : CantidadUnica);
                if (!myParametro.GetParCantidadRealSinRedondeo())
                {
                    CantidadReal = Math.Round(CantidadReal, 2, MidpointRounding.AwayFromZero);
                }
                
                CantidadTotal += CantidadReal;

                PrecioTotal += PrecioLista * CantidadReal;
                PrecioTotal = Math.Round(PrecioTotal, 2, MidpointRounding.AwayFromZero);

                Descuentos1 = (det.Precio * det.Descuento) / 100;
                Descuentos1 = Math.Round(Descuentos1, 2, MidpointRounding.AwayFromZero);

                Descuentos = (det.DesPorciento / 100) * det.Precio;
                Descuentos = Math.Round(Descuentos, 2, MidpointRounding.AwayFromZero);

                if (Descuentos == 0.0)
                {
                    Descuentos = det.Descuento;
                }

                double descTotalUnitario = Descuentos * CantidadReal;
                descTotalUnitario = Math.Round(descTotalUnitario, 2, MidpointRounding.AwayFromZero);

                DecuentoTotal += descTotalUnitario;
                DecuentoTotal = Math.Round(DecuentoTotal, 2, MidpointRounding.AwayFromZero);

                double tasaItbis = det.ItbisT;

                double MontoItbis = ((PrecioLista - Descuentos) * (tasaItbis / 100));
                MontoItbis = Math.Round(MontoItbis, 2, MidpointRounding.AwayFromZero);

                ItebisTotal += (MontoItbis * CantidadReal);
                ItebisTotal = Math.Round(ItebisTotal, 2, MidpointRounding.AwayFromZero);


                if (tasaItbis != 0)
                {
                    PrecioLista = PrecioLista + (PrecioLista * (tasaItbis / 100));
                    PrecioLista = Math.Round(PrecioLista, 2, MidpointRounding.AwayFromZero);
                }

                double subTotal = PrecioLista * CantidadReal;
                subTotal = Math.Round(subTotal, 2, MidpointRounding.AwayFromZero);


                if (ofeDesc != null && ofeDesc.Count > 0)
                {
                    DescuentoOfertas = ofeDesc[0].DescuentoOfertas;
                }
            }

            Total = (PrecioTotal) - DecuentoTotal + ItebisTotal;
            SubTotal = (PrecioTotal);

            if (list.Count > 0)
            {
                var montototal = Total; //Math.Round(list[0].SubTotal + list[0].Itbis - list[0].Descuento,2);//list[0].Total;//list[0].SubTotal + list[0].Itbis - list[0].Descuento;
                list[0].Total = montototal;
                list[0].Itbis = ItebisTotal;
                list[0].Descuento = DecuentoTotal;
                list[0].SubTotal = PrecioTotal;
                list[0].AdValorem = adValorem;
                list[0].Selectivo = selectivo;
                list[0].CantidadTotal = (int)CantidadTotal;
                var total = list[0];
                total.DescuentoOfertas = DescuentoOfertas;
                return total;
            }

            return new Totales();
        }

        public List<Clientes> GetClientesnoVisitadosPorCuadre(Cuadres cuadre)
        {
            DateTime.TryParse(cuadre.CuaFechaFin, out DateTime fecha);
            var algo = fecha.ToString("yyyy-MM-dd HH:mm tt");
            int dayNumber = (int)(fecha.DayOfWeek - 1);

            int CicloSemana = 4;


            int weekNumber = Functions.GetWeekOfMonth(fecha);
            int parCicloSemana = myParametro.GetParCiclosSemanas();
            if (parCicloSemana > 0)
            {
                CicloSemana = parCicloSemana;
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
            string semanaValue = new string(diasSemana);

            var list = SqliteManager.GetInstance().Query<Clientes>("Select r.CliID as Cliid, c.CliCodigo as CliCodigo, c.Clinombre as Clinombre from RutaVisitas r INNER JOIN Clientes c on r.cliid = c.cliid where RutSemana" + weekNumber +
                " like '" + semanaValue + "' and r.cliid not in(select DISTINCT CliID from Visitas v where strftime('%Y-%m-%d', VisFechaEntrada) between " +
                "strftime('%Y-%m-%d', '" + cuadre.CuaFechaInicio + "') and strftime('%Y-%m-%d', '" + cuadre.CuaFechaFin + "')) ORDER BY CLIID ", new string[] { });

            return list;

        }

        public List<Clientes> GetClientesVisitasnoEfectivas(Cuadres cuadre)
        {
            DateTime.TryParse(cuadre.CuaFechaFin, out DateTime fecha);
            var algo = fecha.ToString("yyyy-MM-dd HH:mm tt");
            int dayNumber = (int)(fecha.DayOfWeek - 1);

            int CicloSemana = 4;


            int weekNumber = Functions.GetWeekOfMonth(fecha);
            int parCicloSemana = myParametro.GetParCiclosSemanas();
            if (parCicloSemana > 0)
            {
                CicloSemana = parCicloSemana;
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
            string semanaValue = new string(diasSemana);

            var list = SqliteManager.GetInstance().Query<Clientes>(
              "SELECT c.cliid, c.CliCodigo, CliNombre, ' ' as CliMotivo FROM Clientes c WHERE c.cliid IN " +
              "(SELECT cliid FROM RutaVisitas WHERE RutSemana" + weekNumber + " LIKE '" + semanaValue + "' AND cliid IN " +
              "(SELECT cliid FROM Visitas WHERE STRFTIME('%Y-%m-%d', VisFechaEntrada) = STRFTIME('%Y-%m-%d', '" + cuadre.CuaFechaInicio + "') " +
              "and cliid in " +
              "( " +
              "select DISTINCT CliID from Visitas v where STRFTIME('%Y-%m-%d', VisFechaEntrada) between " +
              "STRFTIME('%Y-%m-%d', '" + cuadre.CuaFechaInicio + "') and STRFTIME('%Y-%m-%d', '" + cuadre.CuaFechaFin + "') " +
              "and " +
              "NOT EXISTS " +
              "(select  visSecuencia from Ventas ve where /*ve.VisSecuencia = v.VisSecuencia " +
              "and*/ CuaSecuencia = " + cuadre.CuaSecuencia + " and VenEstatus <> 0 " +
              ") " +
              "and " +
              "NOT EXISTS " +
              "(select visSecuencia from VentasConfirmados vc where /*vc.VisSecuencia = v.VisSecuencia " +
              "and*/ CuaSecuencia = " + cuadre.CuaSecuencia + "  and VenEstatus <> 0 " +
              ")))) " +
              "UNION " +
              "Select m.Cliid, cl.CliCodigo, cl.CliNombre as CliNombre, SUBSTR(MenDescripcion,1,60) as CliMotivo from Mensajes  m " +
              "inner join clientes cl on cl.cliid = m.cliid where STRFTIME('%Y-%m-%d', menfecha) BETWEEN STRFTIME('%Y-%m-%d', '" + cuadre.CuaFechaInicio + "') " +
              "and STRFTIME('%Y-%m-%d', '" + cuadre.CuaFechaFin + "')  and m.Cliid in (select cliid from RutaVisitas where RutSemana" + weekNumber + " like '" + semanaValue + "') ", new string[] { });

            return list;

        }

        public List<Ventas> GetVentasCreditoconItbis(DateTime desde, DateTime hasta)
        {
            return SqliteManager.GetInstance().Query<Ventas>(
                "select v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre, " +
                "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float)))) + vd.venTotalitbis) " +
                " as VenTotal, v.VenNCF as VenNCF, SUM(((cast(vd.VenPrecio as float) - cast(vd.VenDescuento as float))) * (cast(vd.VenCantidad as float) + (cast(ifnull(vd.VenCantidadDetalle, 0) as float) / cast(ifnull(p.ProUnidades, 1) as float)))) as VenTotalUnitario " +
                "from Ventas v inner join VentasDetalle vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID inner join Clientes cli on cli.CliID = v.CliID " +
                "INNER JOIN CondicionesPago c ON v.Conid = c.Conid " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and c.ConDiasVencimiento > 0 and v.venEstatus <> 0 and cast(strftime('%Y%m%d',v.VenFecha) as integer) between cast(strftime('%Y%m%d', '" + desde.ToString("yyyy-MM-dd") + "') as integer) and cast(strftime('%Y%m%d', '" + hasta.ToString("yyyy-MM-dd") + "') as integer) " + 
                " group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf " +
                "UNION  " +
                "select v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre, sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float)))) + vd.venTotalitbis) as VenTotal, " +
                "v.VenNCF as VenNCF, sum(((vd.VenPrecio + vd.VenSelectivo + vd.VenAdValorem - vd.VenDescuento) * vd.VenCantidad) + vd.venTotalitbis) as VenTotalUnitario " +
                "from VentasConfirmados v inner join VentasDetalleConfirmados vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID INNER JOIN CondicionesPago c ON v.Conid = c.Conid  left outer join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and c.ConDiasVencimiento > 0 and v.venEstatus <> 0 and cast(strftime('%Y%m%d',v.VenFecha) as integer) between cast(strftime('%Y%m%d', '" + desde.ToString("yyyy-MM-dd") + "') as integer) and cast(strftime('%Y%m%d', '" + hasta.ToString("yyyy-MM-dd") + "') as integer) " +
                "group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf", new string[] { });
        }

        public List<Ventas> GetVentasCreditosinItbis(DateTime desde, DateTime hasta)
        {
            return SqliteManager.GetInstance().Query<Ventas>(
                "select v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre, " +
                "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float))))) " +
                " as VenTotal, v.VenNCF as VenNCF, SUM(((cast(vd.VenPrecio as float) - cast(vd.VenDescuento as float))) * (cast(vd.VenCantidad as float) + (cast(ifnull(vd.VenCantidadDetalle, 0) as float) / cast(ifnull(p.ProUnidades, 1) as float)))) as VenTotalUnitario " +
                "from Ventas v inner join VentasDetalle vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID inner join Clientes cli on cli.CliID = v.CliID " +
                "INNER JOIN CondicionesPago c ON v.Conid = c.Conid " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and c.ConDiasVencimiento > 0 and v.venEstatus <> 0 and cast(strftime('%Y%m%d',v.VenFecha) as integer) between cast(strftime('%Y%m%d', '" + desde.ToString("yyyy-MM-dd") + "') as integer) and cast(strftime('%Y%m%d', '" + hasta.ToString("yyyy-MM-dd") + "') as integer) " +
                " group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf " +
                "UNION  " +
                "select v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre, sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float))))) as VenTotal, " +
                "v.VenNCF as VenNCF, sum(((vd.VenPrecio + vd.VenSelectivo + vd.VenAdValorem - vd.VenDescuento) * vd.VenCantidad) + vd.venTotalitbis) as VenTotalUnitario " +
                "from VentasConfirmados v inner join VentasDetalleConfirmados vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID INNER JOIN CondicionesPago c ON v.Conid = c.Conid left outer join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and c.ConDiasVencimiento > 0 and v.venEstatus <> 0 and cast(strftime('%Y%m%d',v.VenFecha) as integer) between cast(strftime('%Y%m%d', '" + desde.ToString("yyyy-MM-dd") + "') as integer) and cast(strftime('%Y%m%d', '" + hasta.ToString("yyyy-MM-dd") + "') as integer) " +
                "group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf", new string[] { });
        }

        public List<Ventas> GetVentasaContadoconItbis(DateTime desde, DateTime hasta)
        {
            return SqliteManager.GetInstance().Query<Ventas>("select v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre, v.CliID as CliID, " +
                "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float)))) +vd.venTotalitbis ) " +
                " as VenTotal, v.VenNCF as VenNCF, SUM(((cast(vd.VenPrecio as float) - cast(vd.VenDescuento as float))) * (cast(vd.VenCantidad as float) + (cast(ifnull(vd.VenCantidadDetalle, 0) as float) / cast(ifnull(p.ProUnidades, 1) as float)))) as VenTotalUnitario " +
                " , sum(vd.venTotalitbis) as venTotalitbis " +
                "from Ventas v inner join VentasDetalle vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID inner join Clientes cli on cli.CliID = v.CliID " +
                "INNER JOIN CondicionesPago c ON v.Conid = c.Conid " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and c.ConDiasVencimiento = 0 and v.venEstatus <> 0 and cast(strftime('%Y%m%d',v.VenFecha) as integer) between cast(strftime('%Y%m%d', '" + desde.ToString("yyyy-MM-dd") + "') as integer) and cast(strftime('%Y%m%d', '" + hasta.ToString("yyyy-MM-dd") + "') as integer) " +
                "group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf " +
                "UNION  " +
                "select v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre, v.CliID as CliID, sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float)))) + vd.venTotalitbis) as VenTotal, " +
                "v.VenNCF as VenNCF, sum(((vd.VenPrecio + vd.VenSelectivo + vd.VenAdValorem - vd.VenDescuento) * vd.VenCantidad) + vd.venTotalitbis) as VenTotalUnitario " +
                " , sum(vd.venTotalitbis) as venTotalitbis " +
                "from VentasConfirmados v inner join VentasDetalleConfirmados vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID INNER JOIN CondicionesPago c ON v.Conid = c.Conid  left outer join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and c.ConDiasVencimiento = 0 and v.venEstatus <> 0 and cast(strftime('%Y%m%d',v.VenFecha) as integer) between cast(strftime('%Y%m%d', '" + desde.ToString("yyyy-MM-dd") + "') as integer) and cast(strftime('%Y%m%d', '" + hasta.ToString("yyyy-MM-dd") + "') as integer) " +
                "group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf", new string[] { });
        }

        public List<Ventas> GetVentasaContadosinItbis(DateTime desde, DateTime hasta)
        {
            return SqliteManager.GetInstance().Query<Ventas>("select v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre, v.CliID as CliID, " +
                "sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float))))) " +
                " as VenTotal, v.VenNCF as VenNCF, SUM(((cast(vd.VenPrecio as float) - cast(vd.VenDescuento as float))) * (cast(vd.VenCantidad as float) + (cast(ifnull(vd.VenCantidadDetalle, 0) as float) / cast(ifnull(p.ProUnidades, 1) as float)))) as VenTotalUnitario " +
                " , sum(vd.venTotalitbis) as venTotalitbis " +
                "from Ventas v inner join VentasDetalle vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID inner join Clientes cli on cli.CliID = v.CliID " +
                "INNER JOIN CondicionesPago c ON v.Conid = c.Conid " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and c.ConDiasVencimiento = 0 and v.venEstatus <> 0 and cast(strftime('%Y%m%d',v.VenFecha) as integer) between cast(strftime('%Y%m%d', '" + desde.ToString("yyyy-MM-dd") + "') as integer) and cast(strftime('%Y%m%d', '" + hasta.ToString("yyyy-MM-dd") + "') as integer) " +
                "group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf " +
                "UNION  " +
                "select v.vensecuencia as VenSecuencia, ifnull(clinombre, 'Cliente Suprimido') as CliNombre, v.CliID as CliID, sum(((cast(vd.VenPrecio as float) + cast(vd.VenSelectivo as float) + cast(vd.VenAdValorem as float) - cast(vd.VenDescuento as float)) * (cast(vd.VenCantidad as float) + (cast(vd.VenCantidadDetalle as float) / cast(p.ProUnidades as float))))) as VenTotal, " +
                "v.VenNCF as VenNCF, sum(((vd.VenPrecio + vd.VenSelectivo + vd.VenAdValorem - vd.VenDescuento) * vd.VenCantidad) + vd.venTotalitbis) as VenTotalUnitario " +
                " , sum(vd.venTotalitbis) as venTotalitbis " +
                "from VentasConfirmados v inner join VentasDetalleConfirmados vd  on v.repcodigo = vd.repcodigo and v.vensecuencia = vd.vensecuencia " +
                "inner join Productos p on p.ProID = vd.ProID INNER JOIN CondicionesPago c ON v.Conid = c.Conid left outer join Clientes cli on cli.CliID = v.CliID " +
                "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and c.ConDiasVencimiento = 0 and v.venEstatus <> 0 and cast(strftime('%Y%m%d',v.VenFecha) as integer) between cast(strftime('%Y%m%d', '" + desde.ToString("yyyy-MM-dd") + "') as integer) and cast(strftime('%Y%m%d', '" + hasta.ToString("yyyy-MM-dd") + "') as integer) " +
                "group by v.vensecuencia, ifnull(clinombre, 'Cliente Suprimido'), venncf", new string[] { });
        }

        public List<VentasDetalle> getProductosVendidosVentasItbis(DateTime desde, DateTime hasta)
        {
            return SqliteManager.GetInstance().Query<VentasDetalle>("select vd.ProID as ProID, p.ProCodigo as ProCodigo, " +
                "p.ProDescripcion as ProDescripcion, sum(vd.VenCantidad) as VenCantidad, sum(vd.VenCantidadDetalle) as VenCantidadDetalle " +
                "from Ventas v inner join VentasDetalle vd on v.repcodigo = vd.repcodigo and v.VenSecuencia = vd.VenSecuencia " +
                "inner join Productos p on p.ProID = vd.ProID where v.VenEstatus != 0 and ltrim(rtrim(v.RepCodigo)) = ? and VenIndicadorOferta <> 1 and cast(strftime('%Y%m%d',v.VenFecha) as integer) between cast(strftime('%Y%m%d', '" + desde.ToString("yyyy-MM-dd") + "') as integer) and cast(strftime('%Y%m%d', '" + hasta.ToString("yyyy-MM-dd") + "') as integer) " +
                "Group by vd.proid, p.proCodigo, p.ProDescripcion " +
                "UNION " +
                "select vd.ProID as ProID, p.ProCodigo as ProCodigo, " +
                "p.ProDescripcion as ProDescripcion, sum(vd.VenCantidad) as VenCantidad, sum(vd.VenCantidadDetalle) as VenCantidadDetalle " +
                "from VentasConfirmados v inner join VentasDetalleConfirmados vd on v.repcodigo = vd.repcodigo and v.VenSecuencia = vd.VenSecuencia " +
                "inner join Productos p on p.ProID = vd.ProID where v.VenEstatus != 0 AND ltrim(rtrim(v.RepCodigo)) = ? and VenIndicadorOferta <> 1 and cast(strftime('%Y%m%d',v.VenFecha) as integer) between cast(strftime('%Y%m%d', '" + desde.ToString("yyyy-MM-dd") + "') as integer) and cast(strftime('%Y%m%d', '" + hasta.ToString("yyyy-MM-dd") + "') as integer) " +
                "Group by vd.proid, p.proCodigo, p.ProDescripcion ", new string[] { Arguments.CurrentUser.RepCodigo.Trim(), Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public void GetMontosTotalesParaVentas(int vensecuencia, double montototal)
        {
            var result = SqliteManager.GetInstance().Query<ProductosTemp>($@"select sum(VenCantidad * (VenPrecio - Vendescuento)) AS PedMontoSinITBIS,
                             sum(ventotalitbis) as PedTotalItbis from VentasDetalle where repcodigo = '{Arguments.CurrentUser.RepCodigo}'
                            and Vensecuencia = {vensecuencia}").FirstOrDefault();

            SqliteManager.GetInstance().Execute($@"update Ventas set VenMontoSinItbis = {result.PedMontoSinITBIS}, VenMontoItbis = {result.PedTotalItbis},
                            VenMontoTotal = {montototal} where Vensecuencia = {vensecuencia} and repcodigo = '{Arguments.CurrentUser.RepCodigo}'");
        }

        public int GetConDiasVencimiento(int conid)
        {
            return SqliteManager.GetInstance().Query<CondicionesPago>(@"select ifnull(ConDiasVencimiento, '') 
               as ConDiasVencimiento from CondicionesPago where ConID = "+conid+" ")
                .FirstOrDefault().ConDiasVencimiento;
        }

        public string GetNCFdeVentabyNCF(string idReferencia)
        {
            var NCF = "";
            var list = SqliteManager.GetInstance().Query<Ventas>("select  v.VenNCF as VenNCF "
                        + "from Ventas v "
                        + "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and v.VenNCF = '" + idReferencia.ToString() + "' ", new string[] { });

            if (list != null && list.Count > 0)
            {
                NCF = list.FirstOrDefault().VenNCF;
            }

            return NCF;
        }

        public string GetNCFdeVentabyNumeroErp(string idReferencia)
        {
            var NCF = "";
            var list = SqliteManager.GetInstance().Query<Ventas>("select  v.VenNCF as VenNCF "
                        + "from VentasConfirmados v "
                        + "where ltrim(rtrim(v.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and v.VenNumeroErp = " + idReferencia.ToString() + " ", new string[] { });

            if (list != null && list.Count > 0)
            {
                NCF = list.FirstOrDefault().VenNCF;
            }

            return NCF;
        }

        public string VentaToXml(int venSecuencia)
        {
            var venta = GetBySecuencia(venSecuencia, false);
            var cliente = new DS_Clientes().GetClienteById(venta.CliID);
            var productos = GetDetalleBySecuencia(venSecuencia, false);
            var empresa = new DS_Empresa().GetEmpresa(venta.SecCodigo);
            var condicionPago = new DS_CondicionesPago().GetByConId(venta.ConID); 

            var doc = new XmlDocument();
            var docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);

            var efc = (XmlElement)doc.AppendChild(doc.CreateElement("ECF"));

            var enc = (XmlElement)efc.AppendChild(doc.CreateElement("Encabezado"));
            
            var version = doc.CreateElement("Version");
            version.InnerText = "1.0";
            enc.AppendChild(version);

            var idDoc = (XmlElement)enc.AppendChild(doc.CreateElement("IdDoc"));

            var tipoEcf = doc.CreateElement("TipoeCF");
            tipoEcf.InnerText = cliente.CliTipoComprobanteFAC;            
            idDoc.AppendChild(tipoEcf);

            var eNCF = doc.CreateElement("eNCF");
            eNCF.InnerText = venta.VenNCF;
            idDoc.AppendChild(eNCF);

            var FechaVencimientoSecuencia = doc.CreateElement("FechaVencimientoSecuencia");
            FechaVencimientoSecuencia.InnerText = venta.VenNCFFechaVencimiento;
            idDoc.AppendChild(FechaVencimientoSecuencia);

            var IndicadorMontoGravado = doc.CreateElement("IndicadorMontoGravado");
            IndicadorMontoGravado.InnerText = "0";
            idDoc.AppendChild(IndicadorMontoGravado);

            var TipoIngresos = doc.CreateElement("TipoIngresos");
            TipoIngresos.InnerText = "01";
            idDoc.AppendChild(TipoIngresos);

            var TipoPago = doc.CreateElement("TipoPago");
            TipoPago.InnerText = condicionPago.ConDiasVencimiento > 0 ? "2" : "1";
            idDoc.AppendChild(TipoPago);

            var FechaLimitePago = doc.CreateElement("FechaLimitePago");
            FechaLimitePago.InnerText = DateTime.Now.AddDays(condicionPago.ConDiasVencimiento).ToString("dd-MM-yyyy");
            idDoc.AppendChild(FechaLimitePago);

            var emisor = (XmlElement)enc.AppendChild(doc.CreateElement("Emisor"));

            var RNCEmisor = doc.CreateElement("RNCEmisor");
            RNCEmisor.InnerText = empresa.EmpRNC.Replace("-", "");
            emisor.AppendChild(RNCEmisor);

            var RazonSocialEmisor = doc.CreateElement("RazonSocialEmisor");
            RazonSocialEmisor.InnerText = empresa.EmpNombre;
            emisor.AppendChild(RazonSocialEmisor);

            var DireccionEmisor = doc.CreateElement("DireccionEmisor");
            DireccionEmisor.InnerText = empresa.EmpDireccion;
            emisor.AppendChild(DireccionEmisor);

            var FechaEmision = doc.CreateElement("FechaEmision");
            FechaEmision.InnerText = DateTime.Now.ToString("dd-MM-yyyy");

            var comprador = (XmlElement)enc.AppendChild(doc.CreateElement("Comprador"));

            var RNCComprador = doc.CreateElement("RNCComprador");
            RNCComprador.InnerText = cliente.CliRNC.Replace("-", "");
            comprador.AppendChild(RNCComprador);

            var RazonSocialComprador = doc.CreateElement("RazonSocialComprador");
            RazonSocialComprador.InnerText = cliente.CliNombre;
            comprador.AppendChild(RazonSocialComprador);


            var totales = (XmlElement)enc.AppendChild(doc.CreateElement("Totales"));

            var MontoGravadoTotal = doc.CreateElement("MontoGravadoTotal");
            MontoGravadoTotal.InnerText = productos.Sum(x => x.SubTotal).ToString("N2");
            totales.AppendChild(MontoGravadoTotal);

            var MontoGravadoI1 = doc.CreateElement("MontoGravadoI1");
            MontoGravadoI1.InnerText = productos.Where(x => (int)x.VenItbis == 18).Sum(x => x.SubTotal).ToString("N2");
            totales.AppendChild(MontoGravadoI1);

            var MontoGravadoI2 = doc.CreateElement("MontoGravadoI2");
            MontoGravadoI2.InnerText = productos.Where(x => (int)x.VenItbis == 16).Sum(x => x.SubTotal).ToString("N2");
            totales.AppendChild(MontoGravadoI2);

            var MontoGravadoI3 = doc.CreateElement("MontoGravadoI3");
            MontoGravadoI3.InnerText = "0.00";
            totales.AppendChild(MontoGravadoI3);

            var MontoExento = doc.CreateElement("MontoExento");
            MontoExento.InnerText = productos.Where(x => (int)x.VenItbis == 0).Sum(x => x.SubTotal).ToString("N2");
            totales.AppendChild(MontoExento);

            var ITBIS1 = doc.CreateElement("ITBIS1");
            ITBIS1.InnerText = "18";
            totales.AppendChild(ITBIS1);

            var ITBIS2 = doc.CreateElement("ITBIS2");
            ITBIS2.InnerText = "16";
            totales.AppendChild(ITBIS2);

            var ITBIS3 = doc.CreateElement("ITBIS3");
            ITBIS3.InnerText = "0";
            totales.AppendChild(ITBIS3);

            var TotalITBIS = doc.CreateElement("TotalITBIS");
            TotalITBIS.InnerText = productos.Sum(x => x.VenTotalItbis).ToString("N2");
            totales.AppendChild(TotalITBIS);

            var TotalITBIS1 = doc.CreateElement("TotalITBIS1");
            TotalITBIS1.InnerText = productos.Where(x => (int)x.VenItbis == 18).Sum(x => x.VenTotalItbis).ToString("N2");
            totales.AppendChild(TotalITBIS1);

            var TotalITBIS2 = doc.CreateElement("TotalITBIS2");
            TotalITBIS2.InnerText = productos.Where(x => (int)x.VenItbis == 16).Sum(x => x.VenTotalItbis).ToString("N2");
            totales.AppendChild(TotalITBIS2);

            var TotalITBIS3 = doc.CreateElement("TotalITBIS3");
            TotalITBIS3.InnerText = "0.00";
            totales.AppendChild(TotalITBIS3);

            var MontoTotal = doc.CreateElement("MontoTotal");
            MontoTotal.InnerText = productos.Sum(x => x.SubTotal + x.VenTotalItbis).ToString("N2");
            totales.AppendChild(MontoTotal);
            
            var detalleItems = (XmlElement)efc.AppendChild(doc.CreateElement("DetallesItems"));

            var pos = 1;
            foreach (var detalle in productos)
            {
                var item = (XmlElement)detalleItems.AppendChild(doc.CreateElement("Item"));

                var NumeroLinea = doc.CreateElement("NumeroLinea");
                NumeroLinea.InnerText = pos.ToString();
                item.AppendChild(NumeroLinea); pos++;

                var indicadorFact = "";

                if((int)detalle.VenItbis == 18)
                {
                    indicadorFact = "1";
                }else if((int)detalle.VenItbis == 16)
                {
                    indicadorFact = "2";
                }
                else
                {
                    indicadorFact = "4";
                }

                var IndicadorFacturacion = doc.CreateElement("IndicadorFacturacion");
                IndicadorFacturacion.InnerText = indicadorFact;
                item.AppendChild(IndicadorFacturacion);

                var NombreItem = doc.CreateElement("NombreItem");
                NombreItem.InnerText = detalle.ProDescripcion;
                item.AppendChild(NombreItem);

                var IndicadorBienoServicio = doc.CreateElement("IndicadorBienoServicio");
                IndicadorBienoServicio.InnerText = "1";
                item.AppendChild(IndicadorBienoServicio);

                var CantidadItem = doc.CreateElement("CantidadItem");
                CantidadItem.InnerText = ((int)Math.Round(detalle.CantidadReal,2, MidpointRounding.AwayFromZero)).ToString();
                item.AppendChild(CantidadItem);

                var PrecioUnitarioItem = doc.CreateElement("PrecioUnitarioItem");
                PrecioUnitarioItem.InnerText = (detalle.VenPrecio + detalle.VenAdValorem + detalle.VenSelectivo).ToString("N2");
                item.AppendChild(PrecioUnitarioItem);

                var DescuentoMonto = doc.CreateElement("DescuentoMonto");
                DescuentoMonto.InnerText = detalle.VenTotalDescuento.ToString("N2");
                item.AppendChild(DescuentoMonto);



                var tablaSubDesc = (XmlElement)item.AppendChild(doc.CreateElement("TablaSubDescuento"));
                var subDesc = (XmlElement)tablaSubDesc.AppendChild(doc.CreateElement("SubDescuento"));

                var TipoSubDescuento = doc.CreateElement("TipoSubDescuento");
                TipoSubDescuento.InnerText = "$";
                subDesc.AppendChild(TipoSubDescuento);

                var MontoSubDescuento = doc.CreateElement("MontoSubDescuento");
                MontoSubDescuento.InnerText = detalle.VenTotalDescuento.ToString("N2");
                subDesc.AppendChild(MontoSubDescuento);

                var otraMoneda = (XmlElement)item.AppendChild(doc.CreateElement("OtraMonedaDetalle"));

                var PrecioOtraMoneda = doc.CreateElement("PrecioOtraMoneda");
                PrecioOtraMoneda.InnerText = "0.00";
                otraMoneda.AppendChild(PrecioOtraMoneda);

                var DescuentoOtraMoneda = doc.CreateElement("DescuentoOtraMoneda");
                DescuentoOtraMoneda.InnerText = "0.00";
                otraMoneda.AppendChild(DescuentoOtraMoneda);

                var MontoItemOtraMoneda = doc.CreateElement("MontoItemOtraMoneda");
                MontoItemOtraMoneda.InnerText = "0.00";
                otraMoneda.AppendChild(MontoItemOtraMoneda);

                var MontoItem = doc.CreateElement("MontoItem");
                MontoItem.InnerText = detalle.SubTotal.ToString("N2");
                item.AppendChild(MontoItem);                
            }
            var FechaHoraFirma = doc.CreateElement("FechaHoraFirma");
            FechaHoraFirma.InnerText = DateTime.Now.ToString("dd-MM-yyyy");
            efc.AppendChild(FechaHoraFirma);

            var result = doc.OuterXml;

            if (!string.IsNullOrWhiteSpace(result))
            {
                result = result.Replace("ó", "o")
                        .Replace("é", "e")
                .Replace("í", "i")
                        .Replace("á", "a")
                .Replace("ú", "u")
                .Replace("ü", "u")
                .Replace("ñ", "n")
                        .Replace("Ñ", "N")
                .Replace("À", "A")
                .Replace("Á", "A")
                .Replace("È", "E")
                .Replace("É", "E")
                .Replace("Ì", "I")
                .Replace("Í", "I")
                .Replace("Ò", "O")
                .Replace("Ó", "O")
                .Replace("Ù", "U")
                .Replace("Ú", "U")
                .Replace("Ü", "U")
                        .Replace("½", "");

                // strips off all non-ASCII characters
                result = result.Replace("[^\\x00-\\x7F]", "?");

                // erases all the ASCII control characters
                ///result = result.replaceAll("[\\p{Cntrl}&&[^\r\n\t]]", "?");

                // removes non-printable characters from Unicode
                result = result.Replace("\\p{C}", "?");

            }

            return result;
        }

        public void InsertarVentasInTempFromCotizaciones(int cotSecuencia, bool confirmado, int titId, out bool productosSinExistencia)
        {
            productosSinExistencia = false;
            SqliteManager.GetInstance().Execute("delete from ProductosTemp where TitID = " + titId.ToString());

            var list = SqliteManager.GetInstance().Query<ProductosTemp>("select " + titId.ToString() + " as TitID, cd.OfeID, cd.ProID as ProID, p.ProCodigo as ProCodigo, " +
               "cd.CotCantidad as Cantidad, cd.CotCantidadDetalle as CantidadDetalle, case when ifnull(p.ProUnidades, 0) = 0 then 1 else p.ProUnidades end as ProUnidades, " +
               "p.ProDescripcion as Descripcion, p.ProDatos3 as ProDatos3, 0 as IndicadorOferta, cd.CotPrecio as Precio, cd.CotItbis as Itbis, " +
               "cd.CotSelectivo as Selectivo, cd.CotAdValorem as AdValorem, cd.CotDescuento as Descuento, cd.CotDesPorciento as DesPorciento, " +
               "cd.UnmCodigo as UnmCodigo, p.ProIndicadorDetalle as IndicadorDetalle, ProPrecio3, ProDescripcion2, ProDescripcion3, ProDatos2, " +
               "ProDatos1, ProDescripcion1, '' as Lote, cd.CotCantidad as CantidadEntrega, cd.CotPosicion as Posicion " +
               "from " + (confirmado ? "CotizacionesDetalleConfirmados" : "CotizacionesDetalle") + " cd " +
               "inner join Productos p on p.ProID = cd.ProID " +
              "where ifnull(cd.CotIndicadorOferta, 0) = 0 and cd.CotSecuencia = ? and trim(cd.RepCodigo) = ? ",
               new string[] { cotSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            var listOferta = SqliteManager.GetInstance().Query<ProductosTemp>("select " + titId.ToString() + " as TitID, cd.OfeID, cd.ProID as ProID, p.ProCodigo as ProCodigo, " +
                 "cd.CotCantidad  as Cantidad, cd.CotCantidadDetalle as CantidadDetalle, case when ifnull(p.ProUnidades, 0) = 0 then 1 else p.ProUnidades end as ProUnidades, " +
                 "p.ProDescripcion as Descripcion, p.ProDatos3 as ProDatos3, 1 as IndicadorOferta, cd.CotPrecio as Precio, cd.CotItbis as Itbis, " +
                 "cd.CotSelectivo as Selectivo, cd.CotAdValorem as AdValorem, cd.CotDescuento as Descuento, cd.CotDesPorciento as DesPorciento, " +
                 "cd.UnmCodigo as UnmCodigo, p.ProIndicadorDetalle as IndicadorDetalle, ProPrecio3, ProDescripcion2, ProDescripcion3, ProDatos2, " +
                 "ProDatos1, ProDescripcion1, '' as Lote, cd.CotCantidad as CantidadEntrega, cd.CotPosicion as Posicion " +
                 "from " + (confirmado ? "CotizacionesDetalleConfirmados" : "CotizacionesDetalle") + " cd " +
                 "inner join Productos p on p.ProID = cd.ProID " +
                "where ifnull(cd.CotIndicadorOferta, 0) = 1 and cd.CotSecuencia = ? and trim(cd.RepCodigo) = ? ",
                 new string[] { cotSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            var dsInv = new DS_Inventarios();
            var almIdRanchero = myParametro.GetParAlmacenVentaRanchera();

            foreach (var producto in list)
            {
                foreach (var prodOferta in listOferta)
                {
                    var ofertaRebaja = SqliteManager.GetInstance().Query<Ofertas>("select OfeID from Ofertas o " +
                  "where ifnull(o.OfeIndicadorRebajaVenta, 0) = 1 and o.OfeID = ?", new string[] { prodOferta.OfeID.ToString() });

                    if (ofertaRebaja.Count > 0 && prodOferta.ProID == producto.ProID)
                    {
                        producto.Cantidad = producto.Cantidad + prodOferta.Cantidad;
                    }
                }

                if (producto.ProDatos3.Contains("x"))
                {
                    double combosDisponibles = producto.Cantidad;
                    double combosExistentes = 0;
                    var productosCombo = new DS_ProductosCombos().GetProductosCombo(producto.ProID);
                    foreach (var proCombo in productosCombo)
                    {
                        var cantidadTotal = dsInv.GetCantidadTotalInventario(proCombo.ProID, almIdRanchero);
                        var cantidadCombosDisponibles = new DS_ProductosCombos().GetCombosDisponiblesxCantidad(proCombo.ProID, proCombo.ProIDCombo, cantidadTotal);

                        combosExistentes = (cantidadCombosDisponibles < combosExistentes || combosExistentes == 0) ? cantidadCombosDisponibles : combosExistentes;

                        if (!dsInv.HayExistencia(proCombo.ProID, (producto.Cantidad * proCombo.PrcCantidad), out Inventarios existencia, producto.CantidadDetalle, almIdRanchero, false))
                        {
                            productosSinExistencia = true;
                            combosDisponibles = (cantidadCombosDisponibles < combosDisponibles) ? cantidadCombosDisponibles : combosDisponibles;
                        }

                        if (existencia != null)
                        {
                            producto.InvCantidad = (int)combosExistentes;
                            producto.InvCantidadDetalle = existencia.InvCantidadDetalle;

                            if (myParametro.GetCantidadxLibras())
                            {
                                producto.InvCantidad = combosExistentes;
                            }
                        }

                    }

                    if (combosDisponibles != producto.Cantidad)
                    {
                        producto.Cantidad = Math.Truncate(combosDisponibles / producto.ProUnidades);
                        if (myParametro.GetCantidadxLibras())
                        {
                            producto.Cantidad = combosDisponibles;
                            producto.CantidadDetalle = 0;
                        }
                    }

                }
                else
                {

                    if (!dsInv.HayExistencia(producto.ProID, producto.Cantidad, out Inventarios existencia, producto.CantidadDetalle, almIdRanchero, false))
                    {
                        productosSinExistencia = true;

                        var cantidadTotal = dsInv.GetCantidadTotalInventario(producto.ProID, almIdRanchero);

                        producto.Cantidad = Math.Truncate(cantidadTotal / producto.ProUnidades);

                        if (producto.CantidadDetalle > 0)
                        {
                            producto.CantidadDetalle = (int)Math.Round(((cantidadTotal / producto.ProUnidades) - Math.Truncate(cantidadTotal / producto.ProUnidades)) * producto.ProUnidades);
                        }

                        if (myParametro.GetCantidadxLibras())
                        {
                            producto.Cantidad = cantidadTotal;
                            producto.CantidadDetalle = 0;
                        }
                    }

                    if (existencia != null)
                    {
                        producto.InvCantidad = (int)existencia.invCantidad;
                        producto.InvCantidadDetalle = existencia.InvCantidadDetalle;

                        if (myParametro.GetCantidadxLibras())
                        {
                            producto.InvCantidad = existencia.invCantidad;
                        }
                    }
                }

                producto.rowguid = Guid.NewGuid().ToString();

                if (producto.Cantidad > 0 || producto.CantidadDetalle > 0)
                {
                    SqliteManager.GetInstance().Insert(producto);
                }
            }

        }
    }
}

