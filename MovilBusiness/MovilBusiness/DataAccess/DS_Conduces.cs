using MovilBusiness.Configuration;
using MovilBusiness.Internal;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MovilBusiness.DataAccess
{
    public class DS_Conduces : DS_Controller
    {
        public int GuardarConduce(EntregasRepartidorTransacciones entrega)
        {
            var conSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Conduces");

            new DS_Visitas().ActualizarVisitaEfectiva(Arguments.Values.CurrentVisSecuencia);

            var productos = GetDetalleToSave();

            var map = new Hash("Conduces");
            map.Add("ConEstatus", 1);
            map.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            map.Add("ConSecuencia", conSecuencia);
            map.Add("SupID", entrega.CliID);
            map.Add("ConFecha", Functions.CurrentDate());
            
            //map.Add("ConTotal", productos.Count);
            map.Add("ConTipo", 1);
            map.Add("VisSecuencia", Arguments.Values.CurrentVisSecuencia);
            map.Add("MonCodigo", Arguments.Values.CurrentClient.MonCodigo);

            if(Arguments.Values.CurrentSector != null)
            {
                map.Add("SecCodigo", Arguments.Values.CurrentSector.SecCodigo);
            }

            map.Add("ConID", Arguments.Values.CurrentClient.ConID);
            map.Add("CuaSecuencia", Arguments.Values.CurrentCuaSecuencia);
            map.Add("LipCodigo", Arguments.Values.CurrentClient.LiPCodigo);
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map.Add("ConFechaActualizacion", Functions.CurrentDate());
            map.Add("ConFechaSincronizacion", Functions.CurrentDate());
            map.Add("ConCantidadImpresion", 0);
            map.Add("mbVersion", Functions.AppVersion);
            map.Add("CliCodigo", entrega.CliCodigo);

            var rowguid = Guid.NewGuid().ToString();

            map.Add("rowguid", rowguid);
            map.Add("DesRepcodigo", Arguments.CurrentUser.RepCodigo);

            map.ExecuteInsert();

            var parMultiAlmacenes = myParametro.GetParUsarMultiAlmacenes();
            var almIdDevolucion = myParametro.GetParAlmacenIdParaDevolucion();
            var myInv = new DS_Inventarios();

            var pos = 1;

            var devSecuenciasUsadas = new List<int>();

            var productosParaRecibo = new List<DevolucionesDetalle>();

            var parGuardarRecibo = myParametro.GetParConducesGuardarRecibo();

            var totalLineas = 0;
            var parConducesFromDevoluciones = myParametro.GetParConducesDesdeDevoluciones();

            foreach(var p in productos)
            {
                var devoluciones = new List<DevolucionesDetalle>();
                var cantidadRestante = p.CantidadEntregada;

                if (parConducesFromDevoluciones)
                {
                    devoluciones = GetDevolucionesByProducto(p.ProID, p.EnrSecuencia, p.TraSecuencia);
                }
                else
                {
                    devoluciones.Add(new DevolucionesDetalle() { ProID = p.ProID, DevCantidad = (int)cantidadRestante, 
                    DevPrecio = p.TraPrecio, DevItebis = p.TraItbis , DevDescuento = p.TraDescuento, DevSelectivo  = p.TraSelectivo, DevAdvalorem = p.TraAdValorem, UnmCodigo = p.UnmCodigo,
                    DevDocumento = "", DevSecuencia = 0, EnrSecuencia = 0 });
                }

                foreach (var d in devoluciones)
                {
                    var det = new Hash("ConducesDetalle");

                    totalLineas++;
                    var dev = new Hash("DevolucionesDetalle");                   

                    var cantidadUsar = 0;
                    if (d.DevCantidad > cantidadRestante)
                    {
                        cantidadUsar = (int)cantidadRestante;
                        det.Add("ConCantidad", cantidadUsar);
                        dev.Add("DevCantidadConfirmada", "ifnull(DevCantidadConfirmada, 0) + " + cantidadUsar, true);
                        cantidadRestante = 0;
                    }
                    else
                    {
                        cantidadUsar = (int)d.DevCantidad;
                        det.Add("ConCantidad", cantidadUsar);
                        dev.Add("DevCantidadConfirmada", "ifnull(DevCantidadConfirmada, 0) + " + cantidadUsar, true);
                        cantidadRestante -= d.DevCantidad;
                    }

                    if (parGuardarRecibo && parConducesFromDevoluciones)
                    {
                        var itemTemp = productosParaRecibo.Where(x => x.DevSecuencia == d.DevSecuencia).FirstOrDefault();

                        var dp = new DevolucionesDetalle();
                        dp.DevSecuencia = d.DevSecuencia;
                        dp.DevSelectivo = d.DevSelectivo;
                        dp.DevAdvalorem = d.DevAdvalorem;
                        dp.DevDescuento = d.DevDescuento;
                        dp.DevItebis = d.DevItebis;
                        dp.DevPrecio = d.DevPrecio;
                        dp.DevDocumento = d.DevDocumento;
                        dp.DevCantidad = cantidadUsar;
                        dp.ProUnidades = d.ProUnidades;

                        if (itemTemp == null)
                        {   
                            productosParaRecibo.Add(dp);
                        }
                        else
                        {
                            var index = productosParaRecibo.IndexOf(itemTemp);

                            productosParaRecibo[index] = dp;
                        }
                    }

                    det.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                    det.Add("conSecuencia", conSecuencia);
                    det.Add("ConPosicion", pos);
                    det.Add("ConPosicionLote", p.TraPosicion);

                    pos++;

                    det.Add("ProID", p.ProID);
                   
                    det.Add("ConPrecio", d.DevPrecio);
                    det.Add("ConItbis", d.DevItebis);
                    det.Add("ConSelectivo", d.DevSelectivo);
                    det.Add("ConAdValorem", d.DevAdvalorem);
                    det.Add("ConDescuento", d.DevDescuento);
                    det.Add("ConIndicadorOferta", p.TraIndicadorOferta);
                    det.Add("UnmCodigo", d.UnmCodigo);
                    det.Add("ConFechaActualizacion", Functions.CurrentDate());
                    det.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                    det.Add("rowguid", Guid.NewGuid().ToString());
                    det.Add("DevSecuencia", d.DevSecuencia);
                    det.Add("EnrSecuencia", d.EnrSecuencia);
                    det.ExecuteInsert();

                    if (!devSecuenciasUsadas.Contains(d.DevSecuencia) && parConducesFromDevoluciones)
                    {
                        devSecuenciasUsadas.Add(d.DevSecuencia);
                    }

                    if (!p.UsaLote && parConducesFromDevoluciones)
                    {
                        dev.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                        dev.Add("DevFechaActualizacion", Functions.CurrentDate());
                        dev.ExecuteUpdate("ProID = " + d.ProID.ToString() + " and DevSecuencia = " + d.DevSecuencia.ToString() + " " +
                            "and DevPosicion = " + d.DevPosicion.ToString() + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'"); 
                    }

                    if (cantidadRestante == 0)
                    {
                        break;
                    }
                }

                if (parMultiAlmacenes || myParametro.GetParCondrestarinventario())
                {
                    myInv.RestarInventario(p.ProID, p.CantidadEntregada, (int)p.CantidadEntregadaDetalle, almIdDevolucion);
                }

            }

            //var update = new Hash("Conduces");
            //update.Add("ConTotal", totalLineas);
            //update.ExecuteUpdate(new string[] { "rowguid" }, new DbUpdateValue[] { new DbUpdateValue() { Value = rowguid, IsText = true } }, true);

            if (parGuardarRecibo && productosParaRecibo.Count > 0)
            {
                GuardarReciboPorConduce(productosParaRecibo, conSecuencia);
            }

            var productosLotes = GetDetalleLoteToSave();

            pos = 1;
            var totalLineasLotes = 0;
            foreach (var prod in productosLotes)
            {
                var devoluciones = new List<DevolucionesDetalle>();

                var cantidadRestante = prod.Cantidad;

                if (parConducesFromDevoluciones)
                {
                    devoluciones = GetDevolucionesByProducto(prod.ProID, prod.EnrSecuencia, prod.TraSecuencia, true);
                }
                else
                {
                    devoluciones.Add(new DevolucionesDetalle()
                    {
                        ProID = prod.ProID,
                        DevCantidad = (int)cantidadRestante,
                        DevDocumento = "",
                        DevSecuencia = 0,
                        EnrSecuencia = 0
                    });
                }

                foreach(var pdev in devoluciones)
                {
                    var lote = new Hash("ConducesDetalleLotes");
                    var dev = new Hash("DevolucionesDetalle");
                    totalLineasLotes++;

                    if (pdev.DevCantidad > cantidadRestante)
                    {
                        lote.Add("ConCantidad", cantidadRestante);
                        dev.Add("DevCantidadConfirmada", "ifnull(DevCantidadConfirmada, 0) + " + cantidadRestante, true);
                        cantidadRestante = 0;
                    }
                    else
                    {
                        lote.Add("ConCantidad", pdev.DevCantidad);
                        dev.Add("DevCantidadConfirmada", "ifnull(DevCantidadConfirmada, 0) + " + pdev.DevCantidad, true);
                        cantidadRestante -= pdev.DevCantidad;
                    }
                    
                    lote.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                    lote.Add("ConSecuencia", conSecuencia);
                    lote.Add("EnrSecuencia", pdev.EnrSecuencia);
                    lote.Add("DevSecuencia", pdev.DevSecuencia);
                    lote.Add("ConPosicion", pos); pos++;
                    lote.Add("ConLote", prod.Lote);
                    lote.Add("ConPosicionLote", prod.Posicion);
                    
                    lote.Add("ConCantidadDetalle", prod.CantidadDetalle);
                    lote.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                    lote.Add("ConFechaActualizacion", Functions.CurrentDate());
                    lote.Add("rowguid", Guid.NewGuid().ToString());
                    lote.ExecuteInsert();

                    if (!devSecuenciasUsadas.Contains(pdev.DevSecuencia) && parConducesFromDevoluciones)
                    {
                        devSecuenciasUsadas.Add(pdev.DevSecuencia);
                    }

                    if (parConducesFromDevoluciones)
                    {
                        dev.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                        dev.Add("DevFechaActualizacion", Functions.CurrentDate());
                        dev.ExecuteUpdate("ProID = " + pdev.ProID.ToString() + " and DevSecuencia = " + pdev.DevSecuencia.ToString() + " " +
                            "and DevPosicion = " + pdev.DevPosicion.ToString() + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");
                    }

                    if (cantidadRestante == 0)
                    {
                        break;
                    }
                }
            }

            var update = new Hash("Conduces");
            update.Add("ConTotal", totalLineas);
            update.Add("ConCantidadDetalleLote", totalLineasLotes);
            update.ExecuteUpdate(new string[] { "rowguid" }, new DbUpdateValue[] { new DbUpdateValue() { Value = rowguid, IsText = true } }, true);

            DS_RepresentantesSecuencias.UpdateSecuencia("Conduces", conSecuencia);

            if (parConducesFromDevoluciones)
            {
                foreach (int devSecuencia in devSecuenciasUsadas)
                {
                    AnularDevolucionSiEstaCompleta(devSecuencia);
                }
            }

            if (DS_RepresentantesParametros.GetInstance().GetParVisitasResultados())
            {
                ActualizarVisitasResultados();
            }

            return conSecuencia;
        }

        private void ActualizarVisitasResultados()
        {
            var list = SqliteManager.GetInstance().Query<VisitasResultados>("select 51 as TitID, count(*) as VisCantidadTransacciones, " +
                "sum(((d.ConPrecio + d.ConAdValorem + d.ConSelectivo) - d.ConDescuento) * (d.ConCantidad)) as VisMontoSinItbis, sum(((d.ConItbis / 100.0) * ((d.ConPrecio + d.ConAdValorem + d.ConSelectivo) - d.ConDescuento)) * (d.ConCantidad)) as VisMontoItbis from Conduces p " +
                "inner join ConducesDetalle d on d.RepCodigo = p.RepCodigo and d.ConSecuencia = p.ConSecuencia " +
                "inner join Productos o on o.ProID = d.ProID " +
                "where p.VisSecuencia = ? and p.RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'", 
                new string[] { Arguments.Values.CurrentVisSecuencia.ToString() });

            if (list != null && list.Count > 0)
            {
                var item = list.FirstOrDefault();

                item.VisMontoTotal = item.VisMontoSinItbis + item.VisMontoItbis;
                item.VisComentario = "";

                new DS_Visitas().GuardarVisitasResultados(item);
            }
        }

        private void GuardarReciboPorConduce(List<DevolucionesDetalle> facturas, int conSecuencia)
        {
            var dsRec = new DS_Recibos();

            dsRec.ClearTemps();

            var totalGeneral = 0.0;
            foreach (var fac in facturas)
            {
                var reciboToSave = new RecibosDocumentosTemp();
                reciboToSave.FechaSinFormatear = Functions.CurrentDate();
                reciboToSave.Fecha = Functions.CurrentDate("dd-MM-yyyy");
                reciboToSave.Documento = fac.DevDocumento;
                reciboToSave.Referencia = fac.DevDocumento;
                reciboToSave.Sigla = "FAT";

                var total = ((fac.DevPrecio - fac.DevDescuento + fac.DevSelectivo + fac.DevAdvalorem) * (fac.DevItebis / 100.0) + fac.DevPrecio - fac.DevDescuento + fac.DevSelectivo + fac.DevAdvalorem) * fac.DevCantidad;

                totalGeneral += total;
                reciboToSave.Aplicado = total;
                reciboToSave.Descuento = fac.DevDescuento;
                reciboToSave.MontoTotal = total;
                reciboToSave.Balance = total;
                reciboToSave.Pendiente = 0;
                reciboToSave.Estado = "Saldo";
                reciboToSave.Credito = 0;
                reciboToSave.FechaIngles = Functions.CurrentDate("MM-dd-yyyy");
                reciboToSave.Origen = 1;
                reciboToSave.MontoSinItbis = (fac.DevPrecio + fac.DevAdvalorem + fac.DevSelectivo) * fac.DevCantidad;
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
                reciboToSave.RecTipo = 5;

                SqliteManager.GetInstance().Insert(reciboToSave);   
            }

            var formaPago = new FormasPagoTemp();
            formaPago.MonCodigo = Arguments.Values.CurrentClient.MonCodigo;
            formaPago.RefSecuencia = 1;
            formaPago.FormaPago = "EFECTIVO";
            formaPago.Futurista = "No";
            formaPago.Prima = totalGeneral;
            formaPago.Valor = totalGeneral;
            formaPago.rowguid = Guid.NewGuid().ToString();
            formaPago.ForID = 1;

            var mon = new DS_Monedas().GetMoneda(Arguments.Values.CurrentClient.MonCodigo);

            if (mon != null)
            {
                formaPago.Tasa = mon.MonTasa;
            }

            SqliteManager.GetInstance().Insert(formaPago);

            dsRec.GuardarRecibo(conSecuencia.ToString(), mon, forceRecTipo:"5");
        }

        private List<EntregasRepartidorTransaccionesDetalle> GetDetalleToSave()
        {
            return SqliteManager.GetInstance().Query<EntregasRepartidorTransaccionesDetalle>(
                "select t.TraIndicadorOferta as TraIndicadorOferta, t.Posicion as TraPosicion, t.ProID as ProID, t.UsaLote as UsaLote, t.EnrSecuencia as EnrSecuencia, t.UnmCodigo as UnmCodigo, t.TraSecuencia as TraSecuencia, " +
                "p.ProUnidades as ProUnidades, sum(t.Cantidad) as CantidadEntregada, sum(t.CantidadDetalle) as CantidadEntregadaDetalle, t.Precio as TraPrecio, t.Itbis as TraItbis, t.Descuento as TraDescuento, t.Selectivo as TraSelectivo, AdValorem as TraAdValorem  " +
                "from EntregasDetalleTemp t  " +
                "inner join Productos p on p.ProID = t.ProID " +
                "where t.Cantidad > 0 and ((ifnull(t.UsaLote, 1) = 1 and ifnull(t.Lote, '') != '') OR ifnull(t.UsaLote, 0) = 0) " +
                "group by t.ProID, t.Posicion, t.UsaLote, t.UnmCodigo, p.ProUnidades, t.TraSecuencia, t.EnrSecuencia ",
                new string[] { });
        }

        private List<EntregasDetalleTemp> GetDetalleLoteToSave()
        {
            return SqliteManager.GetInstance().Query<EntregasDetalleTemp>("select * from EntregasDetalleTemp t " +
                "where UsaLote = 1 and ifnull(trim(Lote), '') != '' ", new string[] { });
        }
        
        private List<DevolucionesDetalle> GetDevolucionesByProducto(int proId, int enrSecuencia, int traSecuencia, bool forLotes = false)
        {
            return SqliteManager.GetInstance().Query<DevolucionesDetalle>("select d.EnrSecuencia as EnrSecuencia, dd.ProID as ProID, d.DevSecuencia as DevSecuencia, dd.DevPosicion as DevPosicion, " +
                "ifnull(DevCantidad, 0) - ifnull(DevCantidadConfirmada, 0) as DevCantidad, DevPrecio, DevItebis, DevSelectivo, DevAdValorem, " +
                "dd.UnmCodigo, DevDescuento, d.DevReferencia as DevDocumento from DevolucionesDetalle dd " +
                "inner join Devoluciones d on d.DevSecuencia = dd.DevSecuencia and d.RepCodigo = dd.RepCodigo " +
                "inner join Productos p on p.ProID = dd.ProID " +
                "where ifnull(dd.DevCantidad, 0) - ifnull(dd.DevCantidadConfirmada, 0) > 0  " +
                "and d.DevEstatus <> 0 and dd.ProID = ? and trim(dd.RepCodigo) = ? and d.EnrSecuencia = ? " +
                "and cast(d.DevCintillo as integer) = ? " + (forLotes ? " and upper(ifnull(p.ProDatos3, '')) like '%L%'" : ""),
                new string[] { proId.ToString(), Arguments.CurrentUser.RepCodigo.Trim(), enrSecuencia.ToString(), traSecuencia.ToString() });
        }

        private void AnularDevolucionSiEstaCompleta(int devSecuencia)
        {
            var dev = SqliteManager.GetInstance().Query<Devoluciones>("select DevSecuencia from " +
                "DevolucionesDetalle where DevSecuencia = ? and trim(RepCodigo) = ? " +
                "and ifnull(DevCantidad, 0) - ifnull(DevCantidadConfirmada, 0) > 0", 
                new string[] { devSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() }).FirstOrDefault();

            if(dev == null)
            {
                var map = new Hash("Devoluciones");
                map.Add("DevEstatus", 0);
                map.Add("DevFechaActualizacion", Functions.CurrentDate());
                map.Add("UsuInicioSesion", /*Arguments.CurrentUser.RepCodigo*/"mdsoft");
                map.ExecuteUpdate("DevSecuencia = " + devSecuencia.ToString() + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");
            }
        }

        public Conduces GetBySecuencia(int venSecuencia, bool confirmado)
        {
            var list = SqliteManager.GetInstance().Query<Conduces>("select ConSecuencia, v.RepCodigo as RepCodigo, v.SupID as CliID, " +
                "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(ConFecha,1,10)),' ','' ), '') as ConFecha, ConTotal, " +
                "ifnull(v.ConID, -1) as ConID, ConDescripcion, ConCantidadImpresion, CliNombre, CliTelefono, CliContacto as CliPropietario, cli.CliCodigo as CliCodigo, CliRnc, " +
                "CliCalle, CliUrbanizacion from " + (confirmado ? "ConducesConfirmados" : "Conduces") + " v " +
                "left join CondicionesPago c on c.ConID = v.ConID " +
                "inner join Clientes cli on cli.CliID = v.SupID " +
                "where ConSecuencia = ? and ltrim(rtrim(v.RepCodigo)) = ? ", 
                new string[] { venSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public List<ConducesDetalle> GetDetalleBySecuencia(int conSecuencia, bool confirmado)
        {
            return SqliteManager.GetInstance().Query<ConducesDetalle>("select ConSecuencia, p.ProCodigo as ProCodigo, " +
                "case when ifnull(p.ProUnidades, 1) = 0 then 1 else ifnull(p.ProUnidades, 1) end as ProUnidades, ProDescripcion, ConCantidad, " +
                "ConItbis, ConPrecio, ConDescuento, ConIndicadorOferta, ConPosicion, " +
                "ConDesPorciento, v.ProID as ProID, v.DevSecuencia " +
                "from " + (confirmado ? "ConducesDetalleConfirmados" : "ConducesDetalle") + " v " +
                "inner join Productos p on p.ProID = v.ProID " +
                "where ConSecuencia = ? and ltrim(rtrim(v.RepCodigo)) = ? " +
                "order by ProDescripcion", new string[] { conSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });
        }

        public List<ConducesDetalle> GetDetalleBySecuenciaBultosYUnidades(int conSecuencia, bool confirmado)
        {
            var sql = "select sum(Bultos) ProUnidades, sum(Unidades) ConCantidad from  " +
                "(select p.ProUnidades,  sum(ConCantidad) ConCantidad,  cast( " +
                "(sum(ConCantidad) / (case when p.ProUnidades > 0 then p.ProUnidades else 1.0 end)) as int) Bultos, " +
                "(sum(ConCantidad) - ((cast((sum(ConCantidad) / (case when p.ProUnidades > 0 then p.ProUnidades else 1.0 end)) as int) ) " +
                "* (case when p.ProUnidades > 0 then p.ProUnidades else 1.0 end))) Unidades " +
                "from " + (confirmado ? "ConducesDetalleConfirmados" : "ConducesDetalle") + " v " +
                "inner join Productos p on p.ProID = v.ProID " +
                "where ConSecuencia = " + conSecuencia.ToString() + " AND RepCodigo = " + Arguments.CurrentUser.RepCodigo +
                " GROUP BY  p.ProUnidades) tabla";

            return SqliteManager.GetInstance().Query<ConducesDetalle>(sql);
        }

        public void ActualizarCantidadImpresion(int venSecuencia, bool confirmado)
        {
            if (!confirmado)
            {
                Hash map = new Hash("Conduces");
                map.Add("ConCantidadImpresion", "ConCantidadImpresion + 1", true);
                map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                map.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ConSecuencia = " + venSecuencia);
            }
            else
            {
                Hash map2 = new Hash("ConducesConfirmados");
                map2.Add("ConCantidadImpresion", "ConCantidadImpresion + 1", true);
                map2.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                map2.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ConSecuencia = " + venSecuencia);
            }
        }

        public void InsertProductInTempForConduce()
        {

            var where = "";

            var parMultiAlmacenes = myParametro.GetParUsarMultiAlmacenes();

            if (parMultiAlmacenes)
            {
                if (myParametro.GetParAlmacenIdParaDevolucion() is int almIdDevolucion && almIdDevolucion != -1)
                {
                    where += "inner join InventariosAlmacenesRepresentantes i on i.AlmID = " + almIdDevolucion + " " +
                        "and i.ProID = p.ProID and trim(i.RepCodigo) = '"+Arguments.CurrentUser.RepCodigo.Trim()+"' " +
                        "and (ifnull(i.invCantidad, 0) > 0 or ifnull(i.InvCantidadDetalle, 0) > 0) ";
                }else if(myParametro.GetParAlmacenVentaRanchera() is int numofalmranch && numofalmranch != 1)
                {
                    where += "inner join InventariosAlmacenesRepresentantes i on i.AlmID = " + numofalmranch + " " +
                        "and i.ProID = p.ProID and trim(i.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' " +
                        "and (ifnull(i.invCantidad, 0) > 0 or ifnull(i.InvCantidadDetalle, 0) > 0) ";
                }
            }

            var parTipoComprobanteFiscal = myParametro.GetParTipoComprobanteFiscal();
            var parConducesFromDevoluciones = myParametro.GetParConducesDesdeDevoluciones();

            if (!string.IsNullOrWhiteSpace(parTipoComprobanteFiscal) && parConducesFromDevoluciones)
            {
                where += " and substr(ifnull(d.DevNCF, ''), 2,2) != '"+parTipoComprobanteFiscal+"' ";
            }
            
            SqliteManager.GetInstance().Execute("delete from EntregasDetalleTemp", new string[] { });

            var parFillCantidad = myParametro.GetParConducesProductoAutoCantidad();

            var query = "select e.UnmCodigo as UnmCodigo, t.EnrSecuencia as EnrSecuencia, d.DevCintillo as TraSecuencia, t.venNumeroERPDocum as Documento, e.DevDocumento as OfeID, " +
                "DevIndicadorOferta as TraIndicadorOferta, p.ProUnidades as ProUnidades, " + (parMultiAlmacenes ? " i.InvCantidad as InvCantidad, i.InvCantidadDetalle as InvCantidadDetalle, " : "")+" 1 as Posicion, p.ProID as ProID, p.ProCodigo as ProCodigo, " +
                "p.ProDescripcion as ProDescripcion, case when upper(p.ProDatos3) like '%L%' then 1 else 0 end as UsaLote, " +
                "sum(ifnull(DevCantidad, 0) - ifnull(DevCantidadConfirmada, 0)) as CantidadSolicitada, 0 as CantidadSolicitadaDetalle " + 
                (parFillCantidad ? ", sum(ifnull(DevCantidad, 0) - ifnull(DevCantidadConfirmada, 0)) as Cantidad, 0 as CantidadDetalle " : ", 0 as Cantidad, 0 as CantidadDetalle") + 
                ", e.DevPrecio as Precio, " +
                "e.DevDescuento as Descuento, e.DevItebis as Itbis, e.DevSelectivo as Selectivo, e.DevAdValorem as AdValorem " +
                "from DevolucionesDetalle e " +
                "inner join Productos p on p.ProID = e.ProID " +
                "inner join Devoluciones d on d.DevSecuencia = e.DevSecuencia and d.RepCodigo = e.RepCodigo " +
                "inner join EntregasRepartidorTransacciones t on cast(t.TraSecuencia as text) = d.DevCintillo and t.EnrSecuencia = d.EnrSecuencia " +
                where +
                "where d.DevEstatus <> 0 and e.RepCodigo = ? "+(!myParametro.GetParEntregasOfertasTodoONada() ? " and e.DevIndicadorOferta = 0 " : "")+" " +
                "and ifnull(DevCantidad, 0) - ifnull(DevCantidadConfirmada, 0) > 0 and d.CuaSecuencia = "+ Arguments.Values.CurrentCuaSecuencia.ToString() + " " + 
                "group by p.ProID, p.UnmCodigo, p.ProCodigo, p.ProDescripcion, p.ProDatos3, d.EnrSecuencia, d.DevCintillo, " +
                "t.venNumeroERPDocum, e.DevDocumento, DevIndicadorOferta, p.ProUnidades, e.DevPrecio, e.DevDescuento, " +
                "e.DevItebis, e.DevSelectivo, e.DevAdValorem " + (parMultiAlmacenes?", i.InvCantidad, i.InvCantidadDetalle ":"");

            if (!parConducesFromDevoluciones)
            {
                var lipCodigo = myParametro.GetParSectores() >= 2 && Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.LipCodigo : Arguments.Values.CurrentClient != null ? Arguments.Values.CurrentClient.LiPCodigo : "Default";

                var unidadesMedidasValidas = myParametro.GetParUnidadesMedidasVendedorUtiliza();

                query = "select p.UnmCodigo as UnmCodigo, 1 as EnrSecuencia, 1 as TraSecuencia, '1' as Documento, -1 as OfeID, " +
                    "0 as TraIndicadorOferta, p.ProUnidades, i.invCantidad as InvCantidad, 1 as Posicion, p.ProID as ProID, p.ProCodigo as ProCodigo, " +
                    "p.ProDescripcion as ProDescripcion, case when upper(p.ProDatos3) like '%L%' then 1 else 0 end as UsaLote, " +
                    "i.invCantidad as CantidadSolicitada " + (parFillCantidad ? ", i.invCantidad as Cantidad, 0 as CantidadDetalle " : ", 0 as Cantidad, 0 as CantidadDetalle") + 
                    ", l.LipPrecio as Precio, 0 as Descuento, p.ProItbis as Itbis, p.ProSelectivo as Selectivo, p.ProAdValorem as AdValorem " +
                    "from Productos p " +
                    "inner join ListaPrecios l on l.ProID = p.ProID and l.LipCodigo = '"+lipCodigo+"' " + (!string.IsNullOrWhiteSpace(unidadesMedidasValidas) ? " and ifnull(lower(l.UnmCodigo), '') in ("+unidadesMedidasValidas+")" : "") + " ";
        
                if (!parMultiAlmacenes)
                {
                    where = " inner join Inventarios i on i.ProID = p.ProID and trim(i.RepCodigo) = '"+Arguments.CurrentUser.RepCodigo.Trim()+"' " +
                        "and ifnull(i.invCantidad, 0.0) > 0";
                }

                query += where;
            }

            var list = SqliteManager.GetInstance().Query<EntregasDetalleTemp>(query, 
                new string[] { Arguments.CurrentUser.RepCodigo.Trim() });

            var pos = 1;
            foreach(var pro in list)
            {
                pro.Posicion = pos; pos++;
                pro.rowguid = Guid.NewGuid().ToString();

                if (parMultiAlmacenes)
                {    
                    double CantidadReal = (pro.CantidadSolicitada * pro.ProUnidades) + pro.CantidadSolicitadaDetalle;
                    double cantidadInvReal = (pro.InvCantidad * pro.ProUnidades) + pro.InvCantidadDetalle;
                    //(inv.invCantidad * inv.ProUnidades) + inv.InvCantidadDetalle;
                    if (CantidadReal > cantidadInvReal)
                    {
                        var resultRaw2 = cantidadInvReal / pro.ProUnidades;

                        var cantidad = Math.Truncate(resultRaw2);
                        var cantidadDetalle = (int)Math.Round((resultRaw2 - Math.Truncate(resultRaw2)) * pro.ProUnidades);

                        if (parFillCantidad)
                        {
                            pro.Cantidad = cantidad;
                            pro.CantidadDetalle = cantidadDetalle;
                        }
                        pro.CantidadSolicitada = cantidad;
                        pro.CantidadSolicitadaDetalle = cantidadDetalle;
                    }

                }

                pro.CantidadDisponibleOriginal = pro.CantidadSolicitada;
                pro.CantidadDisponibleDetalleOriginal = pro.CantidadSolicitadaDetalle;
            }

            SqliteManager.GetInstance().InsertAll(list);
        }

        public void InsertProductInTempForDetail(int enrSecuencia, bool confirmado = false)
        {
            SqliteManager.GetInstance().Execute("delete from EntregasDetalleTemp", new string[] { });

            var list = SqliteManager.GetInstance().Query<EntregasDetalleTemp>("select e.UnmCodigo as UnmCodigo, '' as Lote, " +
                "e.ConIndicadorOferta as TraIndicadorOferta, e.ConPosicionLote as Posicion, p.ProID as ProID, p.ProCodigo as ProCodigo, " +
                "p.ProDescripcion as ProDescripcion, 0 UsaLote, " +
                "sum(ConCantidad) as CantidadSolicitada, 0 as CantidadSolicitadaDetalle, sum(e.ConCantidad) as Cantidad, " +
                "0 as CantidadDetalle from "+(confirmado? "ConducesDetalleConfirmados" : "ConducesDetalle")+" e " +
                "inner join Productos p on p.ProID = e.ProID " +
                "where e.ConSecuencia = ? and trim(e.RepCodigo) = ? and ifnull(p.ProDatos3, '') not like '%L%' " +
                "group by e.UnmCodigo, e.ConIndicadorOferta, e.ConPosicionLote, p.ProID, p.ProCodigo, p.ProDescripcion " +
                "union " +
                "select e2.UnmCodigo as UnmCodigo, e.ConLote as Lote, " +
                "e2.ConIndicadorOferta as TraIndicadorOferta, e.ConPosicionLote as Posicion, p.ProID as ProID, p.ProCodigo as ProCodigo, " +
                "p.ProDescripcion as ProDescripcion, 1 as UsaLote, " +
                "sum(e.ConCantidad) as CantidadSolicitada, sum(e.ConCantidadDetalle) as CantidadSolicitadaDetalle, sum(e.ConCantidad) as Cantidad, sum(e.ConCantidadDetalle) as CantidaDetalle " +
                "from "+(confirmado? "ConducesDetalleLotesConfirmados" : "ConducesDetalleLotes") +" e " +
                "inner join "+(confirmado? "ConducesDetalleConfirmados" : "ConducesDetalle")+" e2 on e2.ConSecuencia = e.ConSecuencia and e2.RepCodigo = e.RepCodigo " +
                "and e2.ConPosicionLote = e.ConPosicionLote " +
                "inner join Productos p on p.ProID = e2.ProID " +
                "where e.ConSecuencia = ? and trim(e.RepCodigo) = ? and ifnull(p.ProDatos3, '') like '%L%' " +
                "group by e2.UnmCodigo, e.ConLote, e2.ConIndicadorOferta, e.ConPosicionLote, p.ProID, p.ProCodigo, p.ProDescripcion ",
                new string[] { enrSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim(), enrSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            foreach (var ent in list)
            {
                ent.TraSecuencia = 0;
                ent.rowguid = Guid.NewGuid().ToString();
            }

            SqliteManager.GetInstance().InsertAll(list);
        }

        public void InsertProductInTempForDetailConPrecioYFactura(int enrSecuencia, bool confirmado = false)
        {
            SqliteManager.GetInstance().Execute("delete from EntregasDetalleTemp", new string[] { });

            var list = SqliteManager.GetInstance().Query<EntregasDetalleTemp>("select e.UnmCodigo as UnmCodigo, '' as Lote, " +
                "e.ConIndicadorOferta as TraIndicadorOferta, e.ConPosicionLote as Posicion, p.ProID as ProID, p.ProCodigo as ProCodigo, " +
                "p.ProDescripcion as ProDescripcion, 0 UsaLote, " +
                "sum(ConCantidad) as CantidadSolicitada, 0 as CantidadSolicitadaDetalle, sum(e.ConCantidad) as Cantidad, " +
                "0 as CantidadDetalle, ConPrecio as Precio, ConItbis as Itbis,ConDescuento Descuento, ifnull(d.DevReferencia,'') as Documento from " + (confirmado ? "ConducesDetalleConfirmados" : "ConducesDetalle") + " e " +
                "inner join Productos p on p.ProID = e.ProID " +
                " left join Devoluciones d on d.RepCodigo = e.RepCodigo and d.DevSecuencia = e.DevSecuencia and d.EnrSecuencia = e.EnrSecuencia " +
                "where e.ConSecuencia = ? and trim(e.RepCodigo) = ? and ifnull(p.ProDatos3, '') not like '%L%' " +
                "group by e.UnmCodigo, e.ConIndicadorOferta, e.ConPosicionLote, p.ProID, p.ProCodigo, p.ProDescripcion " +
                "union " +
                "select e2.UnmCodigo as UnmCodigo, e.ConLote as Lote, " +
                "e2.ConIndicadorOferta as TraIndicadorOferta, e.ConPosicionLote as Posicion, p.ProID as ProID, p.ProCodigo as ProCodigo, " +
                "p.ProDescripcion as ProDescripcion, 1 as UsaLote, " +
                "sum(e.ConCantidad) as CantidadSolicitada, sum(e.ConCantidadDetalle) as CantidadSolicitadaDetalle, sum(e.ConCantidad) as Cantidad, sum(e.ConCantidadDetalle) as CantidaDetalle " +
                ", ConPrecio as Precio, ConItbis as Itbis, ConDescuento Descuento, ifnull(d.DevReferencia,'') as Documento from " + (confirmado ? "ConducesDetalleLotesConfirmados" : "ConducesDetalleLotes") + " e " +
                "inner join " + (confirmado ? "ConducesDetalleConfirmados" : "ConducesDetalle") + " e2 on e2.ConSecuencia = e.ConSecuencia and e2.RepCodigo = e.RepCodigo " +
                "and e2.ConPosicionLote = e.ConPosicionLote " +
                "inner join Productos p on p.ProID = e2.ProID " +
                "left join Devoluciones d on d.RepCodigo = e2.RepCodigo and d.DevSecuencia = e2.DevSecuencia and d.EnrSecuencia = e2.EnrSecuencia " +
                "where e.ConSecuencia = ? and trim(e.RepCodigo) = ? and ifnull(p.ProDatos3, '') like '%L%' " +
                "group by e2.UnmCodigo, e.ConLote, e2.ConIndicadorOferta, e.ConPosicionLote, p.ProID, p.ProCodigo, p.ProDescripcion ",
                new string[] { enrSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim(), enrSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            foreach (var ent in list)
            {
                ent.TraSecuencia = 0;
                ent.rowguid = Guid.NewGuid().ToString();
            }

            SqliteManager.GetInstance().InsertAll(list);
        }
        public List<Conduces> getConducesRealizados(int cuaSecuencia)
        {
            /*
             string query = $@"select e.SupID CliID,  c.CliCodigo, c.CliNombre, ifnull(sum((
                              (cast(cd.ConPrecio as float) + cast(cd.ConSelectivo as float) + cast(cd.ConAdValorem as float) - cast(cd.ConDescuento as float)) * 
                               cast(cd.ConCantidad as float)  ) ),0)  as ConTotal from Conduces e
                               inner join ConducesDetalle cd on cd.Repcodigo = e.Repcodigo and cd.conSecuencia = e.conSecuencia
                               inner join Clientes c on c.Cliid  = e.SupID where e.Repcodigo = '{Arguments.CurrentUser.RepCodigo}' and e.CuaSecuencia = {cuaSecuencia}
                               group by e.SupID, c.CliCodigo, c.CliNombre							  
                               UNION							  
                              select e.SupID CliID,  c.CliCodigo, c.CliNombre,  sum((
                              (cast(cd.ConPrecio as float) + cast(cd.ConSelectivo as float) + cast(cd.ConAdValorem as float) - cast(cd.ConDescuento as float)) * 
                               cast(cd.ConCantidad as float)  ) )  as ConTotal from ConducesConfirmados e
                             inner join ConducesDetalleConfirmados cd on cd.Repcodigo = e.Repcodigo and cd.conSecuencia = e.conSecuencia
                             inner join Clientes c on c.Cliid  = e.SupID where e.Repcodigo = '{Arguments.CurrentUser.RepCodigo}' and e.CuaSecuencia = {cuaSecuencia}
                             group by e.SupID, c.CliCodigo, c.CliNombre";
             */
            string query = $@"select  e.ConSecuencia, e.SupID CliID,  c.CliCodigo, c.CliNombre,  ifnull(sum((
							((cast(cd.ConPrecio as float) + cast(cd.ConSelectivo as float) + cast(cd.ConAdValorem as float) - cast(cd.ConDescuento as float)) 
							+ (cast(cd.ConPrecio as float) + cast(cd.ConSelectivo as float) + cast(cd.ConAdValorem as float) - cast(cd.ConDescuento as float)) * (cd.ConItbis /100.0))
							 * cast(cd.ConCantidad as float)  ) ),0)  as ConMontoTotal from Conduces e
							  inner join ConducesDetalle cd on cd.Repcodigo = e.Repcodigo and cd.conSecuencia = e.conSecuencia
                              inner join Clientes c on c.Cliid  = e.SupID where e.Repcodigo = '{Arguments.CurrentUser.RepCodigo}' and e.CuaSecuencia = {cuaSecuencia} and e.ConEstatus != 0
							  group by  e.ConSecuencia, e.SupID, c.CliCodigo, c.CliNombre							  
                              UNION							  
                             select  e.ConSecuencia, e.SupID CliID,  c.CliCodigo, c.CliNombre,   ifnull(sum((
							((cast(cd.ConPrecio as float) + cast(cd.ConSelectivo as float) + cast(cd.ConAdValorem as float) - cast(cd.ConDescuento as float)) 
							+ (cast(cd.ConPrecio as float) + cast(cd.ConSelectivo as float) + cast(cd.ConAdValorem as float) - cast(cd.ConDescuento as float)) * (cd.ConItbis /100.0))
							 * cast(cd.ConCantidad as float)  ) ),0)  as ConMontoTotal from ConducesConfirmados e
							inner join ConducesDetalleConfirmados cd on cd.Repcodigo = e.Repcodigo and cd.conSecuencia = e.conSecuencia and cd.ConNumeroERP = e.ConNumeroERP
                            inner join Clientes c on c.Cliid  = e.SupID where e.Repcodigo = '{Arguments.CurrentUser.RepCodigo}' and e.CuaSecuencia = {cuaSecuencia} and e.ConEstatus != 0
							group by e.ConSecuencia, e.SupID, c.CliCodigo, c.CliNombre";
            return SqliteManager.GetInstance().Query<Conduces>(query, new string[] { });
        }


        public List<Conduces> GetMontoTotalConducesPorSectorFromCuadre(int cuaSecuencia)
        {
            var sectores = new DS_Sectores().GetSectores();
            string sectorPorDefecto = "0";
            if (sectores != null && sectores.Count > 0)
            {
                sectorPorDefecto = sectores.FirstOrDefault().SecCodigo;
            }
            string query = $@"select  ifnull(e.SecCodigo,'{sectorPorDefecto}') SecCodigo,  ifnull(sum((
							((cast(cd.ConPrecio as float) + cast(cd.ConSelectivo as float) + cast(cd.ConAdValorem as float) - cast(cd.ConDescuento as float)) 
							+ (cast(cd.ConPrecio as float) + cast(cd.ConSelectivo as float) + cast(cd.ConAdValorem as float) - cast(cd.ConDescuento as float)) * (cd.ConItbis /100.0))
							 * cast(cd.ConCantidad as float)  ) ),0)  as ConMontoTotal from Conduces e
							  inner join ConducesDetalle cd on cd.Repcodigo = e.Repcodigo and cd.conSecuencia = e.conSecuencia
                              left join Sectores s on s.SecCodigo  = e.SecCodigo where e.Repcodigo = '{Arguments.CurrentUser.RepCodigo}' and e.CuaSecuencia = {cuaSecuencia} and e.ConEstatus != 0
							  group by  e.SecCodigo 							  
                              UNION							  
                             select ifnull(e.SecCodigo,'{sectorPorDefecto}') SecCodigo, ifnull(sum((
							((cast(cd.ConPrecio as float) + cast(cd.ConSelectivo as float) + cast(cd.ConAdValorem as float) - cast(cd.ConDescuento as float)) 
							+ (cast(cd.ConPrecio as float) + cast(cd.ConSelectivo as float) + cast(cd.ConAdValorem as float) - cast(cd.ConDescuento as float)) * (cd.ConItbis /100.0))
							 * cast(cd.ConCantidad as float)  ) ),0)  as ConMontoTotal from ConducesConfirmados e
							inner join ConducesDetalleConfirmados cd on cd.Repcodigo = e.Repcodigo and cd.conSecuencia = e.conSecuencia and cd.ConNumeroERP = e.ConNumeroERP
                            left join Sectores s on s.SecCodigo  = e.SecCodigo where e.Repcodigo = '{Arguments.CurrentUser.RepCodigo}' and e.CuaSecuencia = {cuaSecuencia} and e.ConEstatus != 0
							group by e.SecCodigo ";

            return SqliteManager.GetInstance().Query<Conduces>(query, new string[] { });
        }

        public List<ConducesDetalle> getProductosConducesRealizadas(int cuaSecuencia)
        {
            string query = $@"select cd.proid, p.ProCodigo, p.ProDescripcion, sum(cd.ConCantidad) ConCantidad, p.ProUnidades    from ConducesDetalle cd
			   inner join productos p on p.proid = cd.Proid	
			   inner join Conduces c on c.Repcodigo = cd.Repcodigo and c.ConSecuencia = cd.ConSecuencia
			   where cd.Repcodigo ='{Arguments.CurrentUser.RepCodigo}' and c.CuaSecuencia= {cuaSecuencia} and c.ConEstatus != 0
			   group by cd.proid, p.ProCodigo, p.ProDescripcion, p.ProUnidades			   
			   union 			   
			   select cd.proid, p.ProCodigo, p.ProDescripcion, sum(cd.ConCantidad) ConCantidad, p.ProUnidades    from ConducesDetalleConfirmados cd
			   inner join productos p on p.proid = cd.Proid	
			   inner join ConducesConfirmados c on c.Repcodigo = cd.Repcodigo and c.ConSecuencia = cd.ConSecuencia and c.ConNumeroERP = cd.ConNumeroERP 
			   where cd.Repcodigo ='{Arguments.CurrentUser.RepCodigo}' and c.CuaSecuencia= {cuaSecuencia} and c.ConEstatus != 0
			   group by cd.proid, p.ProCodigo, p.ProDescripcion, p.ProUnidades";

            return SqliteManager.GetInstance().Query<ConducesDetalle>(query, new string[] { });
        }

        public Totales GetTempTotales(bool validarLote = false)
        {
            string AdValorem = "AdValorem";
            if (myParametro.GetParADVALOREMTIPO() == 1 || myParametro.GetParADVALOREMTIPO() == -1)
            {
                AdValorem = " (CAST(Precio AS REAL) * (CAST(IFNULL(AdValorem, 0.0) AS REAL) / 100.0)) ";
            }

            var list = SqliteManager.GetInstance().Query<Totales>("select SUM(((CAST(ifnull(CantidadDetalle, 0)AS REAL)  / case when ifnull(P.ProUnidades, 0) = 0 then 1 else ifnull(P.ProUnidades, 1) end) + Cantidad)) as CantidadTotal, " +
                " SUM(ifnull(Selectivo, 0) * ifnull(Cantidad, 0)) as Selectivo, " +
                " IFNULL(SUM((CAST(IFNULL(" + AdValorem + ", 0.0) AS REAL) * ((CAST(IFNULL(CantidadDetalle, 0.0) AS REAL) / CAST(IFNULL(p.ProUnidades, 0.0) AS REAL))+ IFNULL(Cantidad, 0.0)))), 0.0) as AdValorem,  " +
                " SUM(round((Precio + ifnull(Selectivo,0) + ifnull(" + AdValorem + ",0)) * ((CAST(ifnull(CantidadDetalle, 0) AS REAL)  / case when ifnull(P.ProUnidades, 0) = 0 then 1 else ifnull(P.ProUnidades, 1) end) + Cantidad),2)) as SubTotal," +
                " SUM(((((Precio + Selectivo + " + AdValorem + ") - ifnull(Descuento, 0)) + round(((Precio + ifnull(Selectivo, 0) + ifnull(" + AdValorem + ", 0)) - ifnull(Descuento, 0))) * (Itbis / 100.0))) * ((CAST(ifnull(CantidadDetalle, 0) AS REAL)  / case when ifnull(CAST(P.ProUnidades AS REAL), 0) = 0 then 1 else CAST(ifnull(P.ProUnidades, 1) AS REAL) end) + Cantidad)) as Total, " +
                " SUM(Round((ifnull(Descuento, 0) * ((Cast(ifnull(CantidadDetalle, 0) as real)  / case when ifnull(P.ProUnidades, 1) = 0 then 1 else ifnull(P.ProUnidades, 1) end) + Cantidad)),2)) as Descuento," +
                "  SUM((Round(((Precio + ifnull(Selectivo,0) + ifnull(" + AdValorem + ",0)) - ifnull(Descuento, 0)) * (Itbis / 100.0),2)) *  ((CAST(ifnull(CantidadDetalle, 0)AS REAL)  / case when ifnull(P.ProUnidades, 0) = 0 then 1 else ifnull(P.ProUnidades, 1) end) + Cantidad)) as Itbis " +
                " from EntregasDetalleTemp t " +
                " inner join Productos P on P.ProID = t.ProID " +
                " WHERE ifnull(TraIndicadorOferta, 0) = 0 "+(validarLote?" and ifnull(IsAdded, 0) = 1 and case when ifnull(t.UsaLote, 0) = 1 then ifnull(Lote, '') != '' else 1=1 end ":"")+" and t.Cantidad > 0 ",
                new string[] { });

            double DescuentoOfertas = 0;

            var ofeDesc = SqliteManager.GetInstance().Query<Totales>("select sum(Precio * Cantidad) as DescuentoOfertas from EntregasDetalleTemp t " +
                "where ifnull(t.TraIndicadorOferta, 0) = 1 "+(validarLote?" and ifnull(IsAdded, 0) = 1 and case when ifnull(UsaLote, 0) = 1 then ifnull(Lote, '') != '' else 1=1 end ":"")+" and t.Cantidad > 0 ",
                new string[] { });

            if (ofeDesc != null && ofeDesc.Count > 0)
            {
                DescuentoOfertas = ofeDesc[0].DescuentoOfertas;
            }

            if (list.Count > 0)
            {
                var montototal = Math.Round(list[0].SubTotal + list[0].Itbis - list[0].Descuento, 2);//list[0].Total;//list[0].SubTotal + list[0].Itbis - list[0].Descuento;
                list[0].Total = montototal;
                var total = list[0];
                total.DescuentoOfertas = DescuentoOfertas;
                return total;
            }

            return new Totales();
        }


        public void EstConduce(int conSecuencia, string rowguid,int est)
        {
            var con = new Hash("Conduces");
            con.Add("ConEstatus", est);
            con.Add("ConFechaActualizacion", Functions.CurrentDate());
            //con.ExecuteUpdate("ConSecuencia = " + conSecuencia + " and RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");

            if (est == 0)
            {
                if (new DS_SuscriptoresCambios().UpdateCambioEstadoInsertByRowguid(rowguid, est))
                {
                    con.SaveScriptForServer = false;
                }
            }

            con.ExecuteUpdate("rowguid = '" + rowguid + "'");

            var parMultiAlmacenes = myParametro.GetParUsarMultiAlmacenes();
            var almIdDevolucion = myParametro.GetParAlmacenIdParaDevolucion();
            var parGuardarRecibo = myParametro.GetParConducesGuardarRecibo();
            var parConducesFromDevoluciones = myParametro.GetParConducesDesdeDevoluciones();

            var detalles = GetDetalleBySecuencia(conSecuencia, false);

            var myInv = new DS_Inventarios();

            var devoluciones = new List<int>();

            foreach (var detalle in detalles)
            {
                if (parConducesFromDevoluciones)
                {
                    var devolucion = new Hash("DevolucionesDetalle");
                    devolucion.Add("DevCantidadConfirmada", "ifnull(DevCantidadConfirmada, 0.0) - " + detalle.ConCantidad, true);
                    devolucion.ExecuteUpdate("RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and DevSecuencia = " + detalle.DevSecuencia + " and ProID = " + detalle.ProID);

                    devoluciones.Add(detalle.DevSecuencia);
                }

                myInv.AgregarInventario(detalle.ProID, detalle.ConCantidad, 0, parMultiAlmacenes ? almIdDevolucion : -1);
            }

            if (parConducesFromDevoluciones)
            {
                foreach (var devSecuencia in devoluciones)
                {
                    var dev = new Hash("Devoluciones");
                    dev.Add("DevEstatus", 1);
                    dev.Add("DevFechaActualizacion", Functions.CurrentDate());
                    dev.ExecuteUpdate("RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and DevSecuencia = " + devSecuencia.ToString());
                }
            }

            if (parGuardarRecibo && parConducesFromDevoluciones)
            {
                var myRec = new DS_Recibos();

                var recibo = myRec.GetReciboByNumeroYTipo(conSecuencia.ToString(), "5");
                if(recibo != null)
                {
                    myRec.EstRecibos(recibo.rowguid);
                }
            }

            
        }
    }
}
