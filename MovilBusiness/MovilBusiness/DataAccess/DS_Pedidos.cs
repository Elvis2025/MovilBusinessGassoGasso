using MovilBusiness.Configuration;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace MovilBusiness.DataAccess
{
    public class DS_Pedidos : DS_Controller
    {
        private DS_Visitas myVis;
        private DS_Productos myProd;

        public DS_Pedidos()
        {
            myVis = new DS_Visitas();
            myProd = new DS_Productos();
        }

        public void EstPedido(string rowguid,int est)
        {
            var ped = new Hash("Pedidos");
            ped.Add("PedEstatus", est);
            ped.Add("PedFechaActualizacion", Functions.CurrentDate());
            ped.Add("UsuInicioSesion", /*Arguments.CurrentUser.RepCodigo*/"mdsoft");


            if(est == 0)
            {
                if(new DS_SuscriptoresCambios().UpdateCambioEstadoInsertByRowguid(rowguid, est))
                {
                    ped.SaveScriptForServer = false;
                }
            }

            ped.ExecuteUpdate("rowguid = '" + rowguid + "'");
            //ped.ExecuteUpdate("PedSecuencia = " + pedSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");
        }

        public int SavePedido(int ConId, string FechaEntrega, int prioridad, bool IsEditing, int editedSecuencia, string pedOrdenCompra, bool IsPreliminar = false, int tipoPedido = 1, string camposAdicionales = null,string CldDirTipo=null, string CedCodigo ="", bool EnEspera = false, int TipoTrans = -1, int CliIDMaster = -1, string MonCodigo ="", bool fromcopy = false, double subtotal = 0.00, double total = 0.00, bool IsMultiEntrega = false, double itbis = 0.00, double totalDescuentoGlobal = 0.00, double porDescuentoGlobal = 0.00)
        {
            int pedSecuencia;

            if (!IsEditing && !fromcopy)
            {
                pedSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Pedidos");

                myVis.ActualizarVisitaEfectiva(Arguments.Values.CurrentVisSecuencia);
            }
            else if(fromcopy)
            {
                pedSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Pedidos");
            }
            else
            {
                pedSecuencia = editedSecuencia;
            }

            var productsTemp = myProd.GetResumenProductos((int)Modules.PEDIDOS, false, isForGuardar: true);

            var ped = new Hash("Pedidos");

            if(!IsPreliminar && Arguments.Values.IsPedidoAutorizado)
            {
                ped.Add("PedEstatus", 7);
                Arguments.Values.IsPedidoAutorizado = false;
            }
            else
            {
                ped.Add("PedEstatus", IsPreliminar ? 3 : 1);
            }

            if (!IsEditing)
            {
                ped.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                ped.Add("PedSecuencia", pedSecuencia);
                ped.Add("CliID", Arguments.Values.CurrentClient.CliID);
                ped.Add("VisSecuencia", Arguments.Values.CurrentVisSecuencia);
                if (myParametro.GetParSectores() > 0)
                {
                    ped.Add("SecCodigo", Arguments.Values.CurrentSector?.SecCodigo);
                }
            }

            if (myParametro.GetParPedIndicadorRevision())
                ped.Add("PedIndicadorRevision", EnEspera);

            ped.Add("PedFecha", Functions.CurrentDate());
            ped.Add("PedTotal", productsTemp.Count);
            ped.Add("PedTipoPedido", tipoPedido);
            ped.Add("PedOrdenCompra", pedOrdenCompra);
            ped.Add("MonCodigo", !string.IsNullOrWhiteSpace(MonCodigo) ? MonCodigo : Arguments.Values.CurrentClient.MonCodigo);
            ped.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            ped.Add("PedFechaActualizacion", Functions.CurrentDate());
            ped.Add("PedPrioridad", prioridad);

            ped.Add("PedMontoSinITBIS", total - itbis);
            ped.Add("PedMontoITBIS", itbis);
            ped.Add("PedMontoTotal", total);
            ped.Add("PedSubTotal", subtotal);
            ped.Add("PedPorCientoDsctoGlobal", porDescuentoGlobal);
            ped.Add("PedMontoDsctoGlobal", totalDescuentoGlobal);

            if (myParametro.GetParPedidosCamposAdicionales())
            {
                ped.Add("PedOtrosDatos", camposAdicionales);
            }

            if (myParametro.GetParTipoTrasporte())
            {
                ped.Add("PedTipTrans", TipoTrans);
            }

            if (myParametro.GetParCliIdMaster())
            {
                ped.Add("CliIDMaster", CliIDMaster);
            }

            ped.Add("ConID", ConId);
            ped.Add("mbVersion", Functions.AppVersion);
            ped.Add("LipCodigo", Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.LipCodigo : Arguments.Values.CurrentClient.LiPCodigo);
            if (myParametro.GetParPedidosDiasEntrega() != -1)
            {
                DateTime Dateinndays = DateTime.Now.AddDays(myParametro.GetParPedidosDiasEntrega());
                ped.Add("PedFechaEntrega", Dateinndays.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            else
            {
                ped.Add("PedFechaEntrega", FechaEntrega);
            }

            if (Arguments.Values.CurrentCuaSecuencia != 0)
            {
                ped.Add("CuaSecuencia", Arguments.Values.CurrentCuaSecuencia);
            }
            ped.Add("CldDirTipo", CldDirTipo);

            if (IsEditing)
            {
                var pedido = GetBySecuencia(pedSecuencia, false);

                //ped.ExecuteUpdate("rowguid = '"+ pedido.rowguid.Trim()+ "'", true);
                ped.ExecuteUpdate(new string[] { "rowguid" }, new Model.Internal.DbUpdateValue[] { new Model.Internal.DbUpdateValue() { Value = pedido.rowguid.Trim(), IsText = true } }, true);

                var delete = new Hash("PedidosDetalle"); //eliminando las ofertas
                delete.ExecuteDelete("PedSecuencia = " + pedSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and PedIndicadorOferta = 1");

                //eliminando los productos que quito del pedido
                var proIds = GetProIdQueryForDeleteWhileEditing(pedSecuencia, (int)Modules.PEDIDOS);
                if (!string.IsNullOrWhiteSpace(proIds))
                {
                    delete.ExecuteDelete("rowguid in (" + proIds + ")");
                }
            }
            else
            {
                ped.Add("rowguid", Guid.NewGuid().ToString());
                ped.Add("PedIndicadorCompleto", 0);
                ped.ExecuteInsert();
            }

            int pos = 1;
            int posDes = 1;

            //var parDescuentoManual = myParametro.GetParPedidosDescuentoManual();
            var parEditarPrecio = myParametro.GetParPedidosEditarPrecio();
            var parRevenimiento = myParametro.GetParRevenimiento();
            var parDescuentoNegociado = myParametro.GetDescuentoxPrecioNegociado();

            bool IsFirstTime = true;
            foreach (var temp in productsTemp)
            {
                Hash det = new Hash("PedidosDetalle");
                det.Add("PedSecuencia", pedSecuencia);
                det.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);

                det.Add("ProID", temp.ProID);
                det.Add("CliID", Arguments.Values.CurrentClient.CliID);
                det.Add("PedCantidad", temp.Cantidad);

                det.Add("PedCantidadDetalle", parRevenimiento ? temp.CantidadDetalleR : temp.CantidadDetalle);

                if (IsMultiEntrega)
                {
                    det.Add("PedFechaEntrega", temp.PedFechaEntrega);
                }

                double totalitbis = temp.IndicadorOferta ? 0 :
                    Math.Round((temp.Precio + temp.Selectivo + temp.AdValorem - temp.Descuento) *
                    (temp.CantidadDetalle > 0 ? temp.Cantidad + (double.Parse(temp.CantidadDetalle.ToString())
                    / double.Parse(temp.ProUnidades.ToString())) : temp.ProUnidades > 0 ? temp.Cantidad / temp.ProUnidades :
                    temp.Cantidad) * (temp.Itbis / 100), 2);

                det.Add("PedtotalItbis", totalitbis);

                if (temp.IndicadorPromocion)
                {
                    det.Add("PedCaracteristicas", "'P'");
                    temp.Precio = 0;
                    temp.Descuento = 0;
                    temp.AdValorem = 0;
                    temp.Selectivo = 0;
                    temp.Itbis = 0;
                    temp.DesPorciento = 0;
                    temp.DesPorcientoManual = 0;
                    temp.PrecioTemp = 0;
                }

                det.Add("CedCodigo", !string.IsNullOrEmpty(temp.CedCodigo) ? temp.CedCodigo : CedCodigo);

                if(parEditarPrecio && temp.PrecioTemp > 0 && !temp.IndicadorOferta)
                {
                    det.Add("PedPrecio", parDescuentoNegociado ? temp.Precio : temp.PrecioTemp);
                }
                else
                {
                    det.Add("PedPrecio", temp.Precio);
                }

                det.Add("PedItbis", temp.Itbis);
                det.Add("PedSelectivo", temp.Selectivo);
                det.Add("PedAdValorem", temp.AdValorem);

                det.Add("PedIndicadorOferta", temp.IndicadorOferta);
                det.Add("PedIndicadorCompleto", 0);
                det.Add("OfeID", temp.OfeID);
                //porque lo pusieron asi, si ya este calculo se hace en ds_descuentosrecargos al momento 
                //de calcular el descuento en el metodo CalcularDescuentoNormalByTipo???????
                det.Add("PedDesPorciento", temp.Precio > 0 ? temp.Descuento / temp.Precio * 100 : 0);      //  temp.DesPorciento);
                det.Add("PedDescuento", temp.Descuento);
                det.Add("UnmCodigo", temp.UnmCodigo);
                det.Add("PedFechaActualizacion", Functions.CurrentDate());
                det.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                det.Add("PedFlete", temp.PedFlete);
                det.Add("PedPrecioLista", temp.PrecioMoneda == 0 ? temp.Precio : temp.PrecioMoneda);
                det.Add("PedCantidadOferta", temp.CantidadOferta);
                det.Add("PedDesPorcientoOriginal", temp.DesPorcientoManual == 0 ? temp.DesPorciento : temp.DesPorcientoManual);

                if (!string.IsNullOrWhiteSpace(temp.ProAtributo1))
                {
                    det.Add("ProAtributo1", temp.ProAtributo1);
                }

                if (!string.IsNullOrWhiteSpace(temp.ProAtributo2))
                {
                    det.Add("ProAtributo2", temp.ProAtributo2);
                }

                if(IsEditing && ExistsDetalleInPedido(temp.rowguid, pedSecuencia) && !temp.IndicadorOferta)
                {
                    // det.ExecuteUpdate("rowguid = '"+temp.rowguid+"'", true);
                    det.ExecuteUpdate(new string[] { "rowguid" }, new Model.Internal.DbUpdateValue[] { new Model.Internal.DbUpdateValue() { Value = temp.rowguid.Trim(), IsText = true } }, true);
                }
                else
                {

                    if((IsEditing && temp.IndicadorOferta) || (IsEditing && !ExistsDetalleInPedido(temp.rowguid, pedSecuencia)))
                    {
                        pos = GetMaxPositionFromDetalle(pedSecuencia) + 1;
                    }

                    det.Add("PedPosicion", pos);
                    det.Add("rowguid", Guid.NewGuid().ToString());
                    det.ExecuteInsert();
                }
                //   Guardar PedidosDescuentos//
                if (myParametro.GetParSavePedidosDescuentos())
                {
                    if (!string.IsNullOrWhiteSpace(temp.DesIdsAplicados))
                    {
                        if (IsEditing && IsFirstTime)
                        {
                            posDes = GetMaxPositionFromDetalleDescuentos(pedSecuencia) + 1;
                            var delete = new Hash("PedidosDescuentos");
                            delete.ExecuteDelete("PedSecuencia = " + pedSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' ");
                            IsFirstTime = false;
                        }
                        var desIds = temp.DesIdsAplicados.Split('|');

                        foreach (var desId in desIds)
                        {
                            if (string.IsNullOrWhiteSpace(desId))
                            {
                                continue;
                            }
                            PedidosDescuentos argPedDescuentos = JsonConvert.DeserializeObject<PedidosDescuentos>(desId);


                            Hash PedDesc = new Hash("PedidosDescuentos");
                            PedDesc.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                            PedDesc.Add("PedSecuencia", pedSecuencia);
                            PedDesc.Add("PedPosicion", posDes);
                            PedDesc.Add("DesID", argPedDescuentos.DesID);
                            PedDesc.Add("ProID", temp.ProID);
                            PedDesc.Add("CliID", Arguments.Values.CurrentClient.CliID);
                            PedDesc.Add("DesEscalon", argPedDescuentos.DesEscalon);
                            PedDesc.Add("PedDescuento", argPedDescuentos.PedDescuento); 
                            PedDesc.Add("DesMetodo", argPedDescuentos.DesMetodo);
                            PedDesc.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                            PedDesc.Add("PedDesPorciento", argPedDescuentos.PedDesPorciento); 
                            PedDesc.Add("rowguid", Guid.NewGuid().ToString());
                            PedDesc.ExecuteInsert();


                            //hacer un objeto que tenga todas las columnas de pedidosdescuento y guardarlo como json en productostemp y 
                            //luego solo iterarlo, al guardar el pedido, pegarlo separado 
                            //entre | y luego convertirlo al objeto real para no tener que armar un json array
                        }
                        posDes++;
                    }
                }

                pos++;
            }

            //var totales = myProd.GetTempTotales((int)Modules.PEDIDOS);

            //GetMontosTotalesParaPedidos(pedSecuencia, total);

            if (!IsEditing || fromcopy)
            {

                DS_RepresentantesSecuencias.UpdateSecuencia("Pedidos", pedSecuencia);

                ///si es un pedido prepago proceder a guardar el recibo
                if(ConId == myParametro.GetParPedidosConIdPrepago())
                {
                    Hash cxc = new Hash("CuentasxCobrar");
                    cxc.Add("CxcReferencia", "PED-" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia);
                    cxc.Add("CxcTipoTransaccion", 1);
                    cxc.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                    cxc.Add("CxcDias", 0);
                    cxc.Add("CxcSIGLA", "FAT");
                    cxc.Add("CliID", Arguments.Values.CurrentClient.CliID);
                    cxc.Add("CxcFecha", Functions.CurrentDate());
                    cxc.Add("CxcDocumento", Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia);



                    cxc.Add("CxcBalance", total);
                    cxc.Add("CxcMontoSinItbis", subtotal);
                    cxc.Add("CxcMontoTotal", total);
                    cxc.Add("MonCodigo", Arguments.Values.CurrentClient.MonCodigo);
                    cxc.Add("AreaCtrlCredit", 0);
                    cxc.Add("CxcNotaCredito", 0);
                    cxc.Add("CXCNCF", "");
                    cxc.Add("rowguid", Guid.NewGuid().ToString());
                    cxc.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                    cxc.Add("CueFechaActualizacion", Functions.CurrentDate());

                    var myCxc = new DS_CuentasxCobrar();

                    cxc.Add("CxcFechaVencimiento", myCxc.GetCxcFechaVencimiento(ConId));
                    cxc.Add("ConID", ConId);
                    cxc.ExecuteInsert();

                    var reciboToSave = new RecibosDocumentosTemp();
                    reciboToSave.FechaSinFormatear = Functions.CurrentDate();
                    reciboToSave.Fecha = Functions.CurrentDate("dd-MM-yyyy");
                    reciboToSave.Documento = Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia;
                    reciboToSave.Referencia = "PED-" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia;
                    reciboToSave.Sigla = "FAT";
                    reciboToSave.Aplicado = total;
                    reciboToSave.Descuento = 0;
                    reciboToSave.MontoTotal = total;
                    reciboToSave.Balance = total;
                    reciboToSave.Pendiente = 0;
                    reciboToSave.Estado = "Saldo";
                    reciboToSave.Credito = 0;
                    reciboToSave.FechaIngles = Functions.CurrentDate("MM-dd-yyyy");
                    reciboToSave.Origen = 1;
                    reciboToSave.MontoSinItbis = subtotal;
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
                }
            }

            //myVis.GuardarVisitasResultados(Arguments.Values.CurrentVisSecuencia, Arguments.Values.CurrentModule, "", totales.Total, totales.SubTotal);
            if (myParametro.GetParVisitasResultados())
            {
                ActualizarVisitasResultados();
            }

            myProd.ClearTemp((int)Modules.PEDIDOS);

            return pedSecuencia;

        }

        private void ActualizarVisitasResultados()
        {
            var query = "select 1 as TitID, count(*) as VisCantidadTransacciones, " +
                "sum(((d.PedPrecio + d.PedAdValorem + d.PedSelectivo) - d.PedDescuento) * ((case when d.PedCantidadDetalle > 0 then d.PedCantidadDetalle / o.ProUnidades else 0 end) + d.PedCantidad)) as VisMontoSinItbis, sum(((d.PedItbis / 100.0) * ((d.PedPrecio + d.PedAdValorem + d.PedSelectivo) - d.PedDescuento)) * ((case when d.PedCantidadDetalle > 0 then d.PedCantidadDetalle / o.ProUnidades else 0 end) + d.PedCantidad)) as VisMontoItbis from Pedidos p " +
                "inner join PedidosDetalle d on d.RepCodigo = p.RepCodigo and d.PedSecuencia = p.PedSecuencia " +
                "inner join Productos o on o.ProID = d.ProID " +
                "where p.VisSecuencia = ? and p.RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'";

            var list = SqliteManager.GetInstance().Query<VisitasResultados>(query, new string[] { Arguments.Values.CurrentVisSecuencia.ToString() });

            if(list != null && list.Count > 0)
            {
                var item = list.FirstOrDefault();

                item.VisMontoTotal = item.VisMontoSinItbis + item.VisMontoItbis;
                item.VisComentario = "";

                myVis.GuardarVisitasResultados(item);
            }
        }

        private string GetProIdQueryForDeleteWhileEditing(int pedSecuencia, int titId)
        {
            var list = SqliteManager.GetInstance().Query<PedidosDetalle>("select rowguid from PedidosDetalle " +
                "where PedSecuencia = ? and ltrim(rtrim(RepCodigo)) = ? and ProID not in (select distinct ProID from ProductosTemp where TitID = ? )",
                new string[] { pedSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim(), titId.ToString() });

            if (list != null && list.Count > 0)
            {
                bool first = true;

                var proIds = "";

                foreach (var pro in list)
                {
                    if (first)
                    {
                        first = false;
                        proIds = "'"+pro.rowguid.ToString()+ "'";
                    }
                    else
                    {
                        proIds += ", '" + pro.rowguid.ToString() + "'";
                    }
                }

                return proIds;
            }

            return null;
        }

        private int GetMaxPositionFromDetalle(int pedSecuencia)
        {
            var list = SqliteManager.GetInstance().Query<PedidosDetalle>("select max(PedPosicion) as PedPosicion from PedidosDetalle " +
                "where PedSecuencia = ? and ltrim(rtrim(RepCodigo)) = ?", new string[] { pedSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            if(list != null && list.Count > 0)
            {
                return list[0].PedPosicion;
            }

            return 0;
        }

        private int GetMaxPositionFromDetalleDescuentos(int pedSecuencia)
        {
            var list = SqliteManager.GetInstance().Query<PedidosDescuentos>("select max(PedPosicion) as PedPosicionDescuento from PedidosDescuentos " +
                "where PedSecuencia = ? and ltrim(rtrim(RepCodigo)) = ?", new string[] { pedSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            if (list != null && list.Count > 0)
            {
                return list[0].PedPosicionDescuento;
            }

            return 0;
        }

        /*private void SavePedidosDescuentos(string desId, int proId, double porciento, )
        {

        }*/

        private bool ExistsDetalleInPedido(string rowguid, int pedSecuencia)
        {
            return SqliteManager.GetInstance().Query<Pedidos>("select PedSecuencia from PedidosDetalle " +
                "where rowguid = ? and PedSecuencia = ? and ltrim(rtrim(RepCodigo)) = ? ",
                new string[] { rowguid, pedSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim()}).Count > 0;
        }

        public List<PedidosDetalle> GetDetalleBySecuencia(int pedSecuencia, bool pedidoConfirmado)
        {
            return SqliteManager.GetInstance().Query<PedidosDetalle>("select PedIndicadorOferta, trim(ProDescripcion) as ProDescripcion, d.UnmCodigo, d.rowguid as rowguid, " +
                "d.PedDescuento as PedDescuento, d.PedAdValorem, d.PedSelectivo, d.PedDesPorciento as PedDesPorciento, p.ProDescripcion1 as Referencia, ifnull(p.ProUnidades, 1) as ProUnidades, ifnull(PedCantidad, 0.0) as PedCantidad, p.ProReferencia as RepSupervidor, " +
                "ifnull(PedCantidadDetalle, 0.0) as PedCantidadDetalle, trim(ifnull(p.ProCodigo, '')) as ProCodigo, d.PedItbis as PedItbis, p.ProID as ProID, " +
                "ifnull(PedPrecio, 0.0) as PedPrecio, p.ProReferencia as ProReferencia,ifnull(PedFlete, 0.0) as PedFlete " +
                "from " + (pedidoConfirmado ? "PedidosDetalleConfirmados" : "PedidosDetalle") + " d " +
                "inner join Productos p on p.ProID = d.ProID " +
                "where d.PedSecuencia = ? and ltrim(rtrim(d.RepCodigo)) = ? order by p.ProDescripcion", new string[] { pedSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public List<PedidosDetalle> GetDetalleBySecuenciaFeltrex(int pedSecuencia, bool pedidoConfirmado)
        {
            return SqliteManager.GetInstance().Query<PedidosDetalle>("select PedIndicadorOferta, trim(ProDescripcion) as ProDescripcion, d.UnmCodigo, d.rowguid as rowguid, " +
                "d.PedDescuento as PedDescuento, d.PedAdValorem, d.PedSelectivo, d.PedDesPorciento as PedDesPorciento, p.ProDescripcion1 as Referencia, ifnull(p.ProUnidades, 1) as ProUnidades, ifnull(PedCantidad, 0.0) as PedCantidad, p.ProReferencia as RepSupervidor, " +
                "ifnull(PedCantidadDetalle, 0.0) as PedCantidadDetalle, trim(ifnull(p.ProCodigo, '')) as ProCodigo, d.PedItbis as PedItbis, p.ProID as ProID, " +
                "ifnull(PedPrecio, 0.0) as PedPrecio " +
                "from " + (pedidoConfirmado ? "PedidosDetalleConfirmados" : "PedidosDetalle") + " d " +
                "inner join Productos p on p.ProID = d.ProID " +
                "where d.PedSecuencia = ? and ltrim(rtrim(d.RepCodigo)) = ? order by p.ProDescripcion, p.ProCodigo, d.PedIndicadorOferta", new string[] { pedSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public List<PedidosDetalle> GetDetalleBySecuenciaTabacalera(int pedSecuencia, bool confirmado)
        {
            return SqliteManager.GetInstance().Query<PedidosDetalle>("select PedSecuencia, ProCodigo, case when ifnull(ProUnidades, 1) = 0 then 1 else ifnull(ProUnidades, 1) end as ProUnidades, ProDescripcion, PedCantidad, " +
                "PedCantidadDetalle, PedItbis, PedAdValorem, PedSelectivo, PedPrecio, PedDescuento, PedIndicadorOferta, PedPosicion, PedTotalItbis, PedTotalDescuento, PedDesPorciento, v.ProID as ProID " +
                "from " + (confirmado ? "PedidosDetalleConfirmados" : "PedidosDetalle") + " v " +
                "inner join Productos p on p.ProID = v.ProID " +
                "where PedSecuencia = ? and ltrim(rtrim(v.RepCodigo)) = ? " +
                "order by ProDescripcion", new string[] { pedSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });
        }
        public List<PedidosDetalle> GetDetalleBySecuenciaANDEMP(int pedSecuencia, bool pedidoConfirmado)
        {
            return SqliteManager.GetInstance().Query<PedidosDetalle>("select trim(ProDescripcion) as ProDescripcion, p.UnmCodigo, d.rowguid as rowguid, " +
                "d.PedDescuento as PedDescuento, d.PedAdValorem, d.PedSelectivo, d.PedDesPorciento as PedDesPorciento, p.ProDescripcion1 as Referencia, ifnull(p.ProUnidades, 1) as ProUnidades, ifnull(PedCantidad, 0.0) as PedCantidad, p.ProCantidadMultiploVenta as PedTipoOferta, p.ProDatos1 as CedCodigo, " +
                "ifnull(PedCantidadDetalle, 0.0) as PedCantidadDetalle, trim(ifnull(p.ProCodigo, '')) as ProCodigo, d.PedItbis as PedItbis, p.ProID as ProID, " +
                "ifnull(PedPrecio, 0.0) as PedPrecio, ifnull(p.ProReferencia, '') as ProReferencia  " +
                "from " + (pedidoConfirmado ? "PedidosDetalleConfirmados" : "PedidosDetalle") + " d " +
                "inner join Productos p on p.ProID = d.ProID " +
                "where d.PedSecuencia = ? and ltrim(rtrim(d.RepCodigo)) = ? order by p.ProDescripcion", new string[] { pedSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public List<PedidosDetalle> GetDetalleBySecuenciaConPrecioNegociado(int pedSecuencia, bool pedidoConfirmado)
        {
            return SqliteManager.GetInstance().Query<PedidosDetalle>("select trim(ProDescripcion) as ProDescripcion, d.rowguid as rowguid, " +
                "d.PedDescuento as PedDescuento, d.PedAdValorem, d.PedSelectivo, d.PedDesPorciento as PedDesPorciento, p.ProDescripcion1 as Referencia, ifnull(p.ProUnidades, 1) as ProUnidades, ifnull(PedCantidad, 0.0) as PedCantidad, " +
                "ifnull(PedCantidadDetalle, 0.0) as PedCantidadDetalle, trim(ifnull(p.ProCodigo, '')) as ProCodigo, d.PedItbis as PedItbis, " +
                "(ifnull(PedPrecio, 0.0) - d.PedDescuento) as PedPrecio " +
                "from " + (pedidoConfirmado ? "PedidosDetalleConfirmados" : "PedidosDetalle") + " d " +
                "inner join Productos p on p.ProID = d.ProID " +
                "where d.PedSecuencia = ? and ltrim(rtrim(d.RepCodigo)) = ? order by p.ProDescripcion", new string[] { pedSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public string GetPedidoEstadoERPBySecuencia(int pedseCuencia)
        {
            var query = $@"SELECT PedEstadoErp,* FROM PedidosConfirmados WHERE PedSecuencia = {pedseCuencia}";

            return SqliteManager.GetInstance().ExecuteScalar<string>(query);
        }

        public string GetEstadosErpByPedidos(string estEstado,string estTabla)
        {
            var query = $@"SELECT EstDescripcion FROM Estados
                              WHERE EstEstado Like '%{estEstado}%' AND EstTabla = '{estTabla}'";

            return SqliteManager.GetInstance().ExecuteScalar<string>(query);
        }
        public string GetMotivoPedidos(string rowguid)
        {
            var query = $@"SELECT um.Descripcion FROM PedidosDetalleConfirmados pdc
                                    INNER JOIN UsosMultiples um ON um.CodigoUso = pdc.idMotivo
                                    Where pdc.rowguid = '{rowguid}'";


            var query2 = SqliteManager.GetInstance().ExecuteScalar<string>(query);

            

            return query2;
        }


        public Pedidos GetBySecuencia(int pedSecuencia, bool pedidoConfirmado)
        {
            var list = SqliteManager.GetInstance().Query<Pedidos>("select p.rowguid as rowguid, PedSecuencia,VisSecuencia, c.CliIndicadorPresentacion as CliIndicadorPresentacion,p.PedTotal as PedTotal, p.rowguid as rowguid, CliNombre, CliRnc, CliCodigo, ConDescripcion as ConDescripcion, CliCalle, PedFecha, PedFechaEntrega, CliUrbanizacion, p.MonCodigo, p.PedOrdenCompra as PedFechaEntrega, p.ConID as ConID " +
                ",ifnull(PedTipoPedido,-1) PedTipoPedido, p.PedIndicadorRevision, p.PedEstatus " +
                "from " + (pedidoConfirmado ? "PedidosConfirmados" : "Pedidos") + " p " +
                "inner join Clientes c on c.CliID = p.CliID " +
                "left join CondicionesPago cp on p.ConID = cp.ConID " +
                "where ltrim(rtrim(p.RepCodigo)) = '"+ Arguments.CurrentUser.RepCodigo.Trim()+"' " +
                "and p.PedSecuencia = ?", new string[] { pedSecuencia.ToString() });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }
         public Pedidos GetPedidosConfirmadosBySecuencia(int pedSecuencia)
        {
            var list = SqliteManager.GetInstance().Query<Pedidos>("select p.rowguid as rowguid,PedCantidadConfirmada, PedSecuencia,c.CliIndicadorPresentacion as CliIndicadorPresentacion,p.PedTotal as PedTotal, p.rowguid as rowguid, CliNombre, CliRnc, CliCodigo, ConDescripcion as ConDescripcion, CliCalle, PedFecha, PedFechaEntrega, CliUrbanizacion, p.MonCodigo, p.PedOrdenCompra as PedFechaEntrega, p.ConID as ConID " +
                ",ifnull(PedTipoPedido,-1) PedTipoPedido, p.PedIndicadorRevision, p.PedEstatus " +
                "from " + "PedidosDetalleConfirmados" + " p " +
                "inner join Clientes c on c.CliID = p.CliID " +
                "left join CondicionesPago cp on p.ConID = cp.ConID " +
                "where ltrim(rtrim(p.RepCodigo)) = '"+ Arguments.CurrentUser.RepCodigo.Trim()+"' " +
                "and p.PedSecuencia = ?", new string[] { pedSecuencia.ToString() });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }



        public Pedidos GetBySecuenciaConTotales(int pedSecuencia, bool pedidoConfirmado)
        {
            var list = SqliteManager.GetInstance().Query<Pedidos>("select p.rowguid as rowguid, PedSecuencia,VisSecuencia, c.CliIndicadorPresentacion as CliIndicadorPresentacion,p.PedTotal as PedTotal, p.rowguid as rowguid, CliNombre, CliRnc, CliCodigo, ConDescripcion as ConDescripcion, CliCalle, PedFecha, PedFechaEntrega, CliUrbanizacion, p.MonCodigo, p.PedOrdenCompra as PedFechaEntrega, p.ConID as ConID " +
                ",ifnull(PedTipoPedido,-1) PedTipoPedido, p.PedIndicadorRevision, ifnull(PedMontoSinITBIS,0) as PedMontoSinITBIS, ifnull(PedMontoITBIS,0) as PedMontoITBIS, ifnull(PedMontoTotal,0) as PedMontoTotal, ifnull(PedSubTotal,0) as PedSubTotal, ifnull(PedPorCientoDsctoGlobal,0) as PedPorCientoDsctoGlobal, ifnull(PedMontoDsctoGlobal,0) as PedMontoDsctoGlobal " +
                "from " + (pedidoConfirmado ? "PedidosConfirmados" : "Pedidos") + " p " +
                "inner join Clientes c on c.CliID = p.CliID " +
                "left join CondicionesPago cp on p.ConID = cp.ConID " +
                "where ltrim(rtrim(p.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' " +
                "and p.PedSecuencia = ?", new string[] { pedSecuencia.ToString() });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public Pedidos GetBySecuenciaForCldDirTipo(int pedSecuencia, bool pedidoConfirmado)
        {
            var list = SqliteManager.GetInstance().Query<Pedidos>("select p.CldDirTipo as CldDirTipo " +
                "from " + (pedidoConfirmado ? "PedidosConfirmados" : "Pedidos") + " p " +
                "inner join Clientes c on c.CliID = p.CliID " +
                "left join CondicionesPago cp on p.ConID = cp.ConID " +
                "where ltrim(rtrim(p.RepCodigo)) = '"+ Arguments.CurrentUser.RepCodigo.Trim()+"' " +
                "and p.PedSecuencia = ?", new string[] { pedSecuencia.ToString() });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public Pedidos GetBySecuenciaTabacalera(int pedSecuencia, bool pedidoConfirmado)
        {
            var list = SqliteManager.GetInstance().Query<Pedidos>("select PedSecuencia, p.PedTotal as PedTotal, p.rowguid as rowguid, CliNombre, CliCodigo, CliCalle, CliRnc, CliTelefono, PedFecha, ConDescripcion, CliPropietario, CliUrbanizacion from " + (pedidoConfirmado ? "PedidosConfirmados" : "Pedidos") + " p " +
                "inner join Clientes c on c.CliID = p.CliID inner join CondicionesPago cp on cp.ConID = p.ConID where ltrim(rtrim(p.RepCodigo)) = ? and p.PedSecuencia = ?", new string[] { Arguments.CurrentUser.RepCodigo.Trim(), pedSecuencia.ToString() });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public Pedidos GetBySecuenciaSued(int pedSecuencia, bool pedidoConfirmado)
        {
            var list = SqliteManager.GetInstance().Query<Pedidos>("select PedSecuencia, p.PedTotal as PedTotal, p.rowguid as rowguid, CliNombre, CliCodigo, CliCalle, PedFecha, CliUrbanizacion, PedCantidadImpresion from " + (pedidoConfirmado ? "PedidosConfirmados" : "Pedidos") + " p " +
                "inner join Clientes c on c.CliID = p.CliID where ltrim(rtrim(p.RepCodigo)) = ? and p.PedSecuencia = ?", new string[] { Arguments.CurrentUser.RepCodigo.Trim(), pedSecuencia.ToString() });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public void InsertarPedidoInTemp(int pedSecuencia, bool confirmado, string repcodigo, bool forEditing= false)
        {
            myProd.ClearTemp((int)Modules.PEDIDOS);
            string query = "select " + (!confirmado ? "pd.ProAtributo1 as ProAtributo1, pd.ProAtributo2 as ProAtributo2, " +
                "case when ifnull(p.ProAtributo1, '') != '' then 1 else 0 end as UseAttribute1, case when ifnull(p.ProAtributo2, '') != '' then 1 else 0 end as UseAttribute2, " : "");

            var cantConfirmada = myParametro.GetParPedidosMostrarCantidadConfirmada() && confirmado ? "1 as ShowCantidadConfirmada, pd.PedCantidadConfirmada as CantidadConfirmada, " : "";

            if (myParametro.GetDescuentoxPrecioNegociado())
            {
                if (myParametro.GetParRevenimiento())
                {
                    query += "" + ((int)Modules.PEDIDOS).ToString() + " as TitID, "+ cantConfirmada + " ( pd.PedCantidad  - " + (forEditing ? "case ifnull(pd.PedIndicadorOferta, 0) when 0 then ifnull(pd.PedCantidadOferta,0) else 0 end" : "0") + "  ) as Cantidad, pd.PedCantidadDetalle as CantidadDetalleR, pd.rowguid as rowguid, pd.ProID as ProID, " + (forEditing ? "ifnull(pd.PedPrecioLista,pd.PedPrecio)" : "pd.PedPrecio") + " as Precio, " + (forEditing ? "ifnull(pd.PedPrecioLista,pd.PedPrecio)" : "pd.PedPrecio") + " as PrecioMoneda, ((pd.PedPrecio - pd.PedDescuento) * ((pd.PedItbis / 100.0) + 1.0)) as PrecioTemp, I.AlmID AS AlmID, I.InvCantidad AS InvCantidad, " +
                     "p.ProDescripcion as Descripcion, pd.PedItbis as Itbis, pd.PedSelectivo as Selectivo, pd.PedAdvalorem as Advalorem, pd.PedDesPorciento as DesPorciento,  pd.PedDesPorciento as DesPorcientoManual, ifnull(pd.UnmCodigo, '') as UnmCodigo, " +
                     "ifnull(pd.PedIndicadorOferta, 0) as IndicadorOferta,ifnull(p.ProDatos3, '') as ProDatos3 , pd.PedDescuento as Descuento, pd.OfeID as OfeID, pd.ProID as ProIdOferta, ps.SecCodigo as SecCodigo, case when pd.OfeID != 0 then 1 else 0 end as IndicadorOfertaForShow,ifnull(pd.PedFlete,0) as PedFlete from " + (confirmado ? "PedidosDetalleConfirmados" : "PedidosDetalle") + " pd " +
                     "inner join Productos p on p.ProID = pd.ProID inner join " + (confirmado ? "PedidosConfirmados" : "Pedidos") + " ps on ps.pedsecuencia = pd.pedsecuencia and ps.RepCodigo = pd.RepCodigo ";
                }
                else
                {
                    query += "" + ((int)Modules.PEDIDOS).ToString() + " as TitID, "+ cantConfirmada + " ( pd.PedCantidad - " + (forEditing ? "case ifnull(pd.PedIndicadorOferta, 0) when 0 then ifnull(pd.PedCantidadOferta,0) else 0 end" : "0") + " ) as Cantidad, pd.PedCantidadDetalle as CantidadDetalle, pd.rowguid as rowguid, pd.ProID as ProID, " + (forEditing ? "ifnull(pd.PedPrecioLista,pd.PedPrecio)" : "pd.PedPrecio") + " as Precio, " + (forEditing ? "ifnull(pd.PedPrecioLista,pd.PedPrecio)" : "pd.PedPrecio") + " as PrecioMoneda, ((pd.PedPrecio - pd.PedDescuento) * ((pd.PedItbis / 100.0) + 1.0)) as PrecioTemp, I.AlmID AS AlmID, I.InvCantidad AS InvCantidad, " +
                    "p.ProDescripcion as Descripcion, pd.PedItbis as Itbis, pd.PedSelectivo as Selectivo, pd.PedAdvalorem as Advalorem, pd.PedDesPorciento as DesPorciento,  pd.PedDesPorciento as DesPorcientoManual, ifnull(pd.UnmCodigo, '') as UnmCodigo, " +
                    "ifnull(pd.PedIndicadorOferta, 0) as IndicadorOferta,ifnull(p.ProDatos3, '') as ProDatos3 , pd.PedDescuento as Descuento, pd.OfeID as OfeID, pd.ProID as ProIdOferta, ps.SecCodigo as SecCodigo, case when pd.OfeID != 0 then 1 else 0 end as IndicadorOfertaForShow,ifnull(pd.PedFlete,0) as PedFlete from " + (confirmado ? "PedidosDetalleConfirmados" : "PedidosDetalle") + " pd " +
                    "inner join Productos p on p.ProID = pd.ProID inner join " + (confirmado ? "PedidosConfirmados" : "Pedidos") + " ps on ps.pedsecuencia = pd.pedsecuencia and ps.RepCodigo = pd.RepCodigo ";
                }
            }
            else
            {
                if (myParametro.GetParRevenimiento())
                {
                    query += "" + ((int)Modules.PEDIDOS).ToString() + " as TitID, "+ cantConfirmada + " ( pd.PedCantidad - " + (forEditing ? "case ifnull(pd.PedIndicadorOferta, 0) when 0 then ifnull(pd.PedCantidadOferta,0) else 0 end" : "0") + " ) as Cantidad, pd.PedCantidadDetalle as CantidadDetalleR, pd.rowguid as rowguid, pd.ProID as ProID, " + (forEditing ? "ifnull(pd.PedPrecioLista,pd.PedPrecio)" : "pd.PedPrecio") + " as Precio, " + (forEditing ? "ifnull(pd.PedPrecioLista,pd.PedPrecio)" : "pd.PedPrecio") + " as PrecioMoneda, I.AlmID AS AlmID, I.InvCantidad AS InvCantidad, " +
                     "p.ProDescripcion as Descripcion, pd.PedItbis as Itbis, pd.PedSelectivo as Selectivo, pd.PedAdvalorem as Advalorem, pd.PedDesPorciento as DesPorciento,  pd.PedDesPorciento as DesPorcientoManual, ifnull(pd.UnmCodigo, '') as UnmCodigo, " +
                     "ifnull(pd.PedIndicadorOferta, 0) as IndicadorOferta,ifnull(p.ProDatos3, '') as ProDatos3 , pd.PedDescuento as Descuento, pd.OfeID as OfeID, pd.ProID as ProIdOferta, ps.SecCodigo as SecCodigo, case when pd.OfeID != 0 then 1 else 0 end as IndicadorOfertaForShow,ifnull(pd.PedFlete,0) as PedFlete from " + (confirmado ? "PedidosDetalleConfirmados" : "PedidosDetalle") + " pd " +
                     "inner join Productos p on p.ProID = pd.ProID inner join " + (confirmado ? "PedidosConfirmados" : "Pedidos") + " ps on ps.pedsecuencia = pd.pedsecuencia and ps.RepCodigo = pd.RepCodigo ";
                }
                else
                {
                    query += "" + ((int)Modules.PEDIDOS).ToString() + " as TitID, "+ cantConfirmada + " ( pd.PedCantidad - " + (forEditing ? "case ifnull(pd.PedIndicadorOferta, 0) when 0 then ifnull(pd.PedCantidadOferta,0) else 0 end" : "0") + " ) as Cantidad, pd.PedCantidadDetalle as CantidadDetalle, pd.rowguid as rowguid, pd.ProID as ProID, " + (forEditing ? "ifnull(pd.PedPrecioLista,pd.PedPrecio)" : "pd.PedPrecio") + " as Precio, " + (forEditing ? "ifnull(pd.PedPrecioLista,pd.PedPrecio)" : "pd.PedPrecio") + " as PrecioMoneda,  I.AlmID AS AlmID, I.InvCantidad AS InvCantidad, " +
                    "p.ProDescripcion as Descripcion, pd.PedItbis as Itbis, pd.PedSelectivo as Selectivo, pd.PedAdvalorem as Advalorem, pd.PedDesPorciento as DesPorciento,  pd.PedDesPorciento as DesPorcientoManual, ifnull(pd.UnmCodigo, '') as UnmCodigo, " +
                    "ifnull(pd.PedIndicadorOferta, 0) as IndicadorOferta,ifnull(p.ProDatos3, '') as ProDatos3 , pd.PedDescuento as Descuento, pd.OfeID as OfeID, pd.ProID as ProIdOferta, ps.SecCodigo as SecCodigo, case when pd.OfeID != 0 then 1 else 0 end as IndicadorOfertaForShow,ifnull(pd.PedFlete,0) as PedFlete from " + (confirmado ? "PedidosDetalleConfirmados" : "PedidosDetalle") + " pd " +
                    "inner join Productos p on p.ProID = pd.ProID inner join " + (confirmado ? "PedidosConfirmados" : "Pedidos") + " ps on ps.pedsecuencia = pd.pedsecuencia and ps.RepCodigo = pd.RepCodigo ";
                }
            }

            string almacenDefault = myParametro.GetParAlmacenDefault();

            query += $" {(myParametro.GetParCantInvAlmacenes()? "inner" : "left")} join InventariosAlmacenes I on I.ProID = p.ProID {(string.IsNullOrEmpty(almacenDefault)? "" : $" and I.AlmId = {almacenDefault} ")}";


            var list = SqliteManager.GetInstance().Query<ProductosTemp>(query + $" where {(myParametro.GetParVendedorContVend()? " ltrim(rtrim(pd.RepCodigo)) = '" + repcodigo + "' and " : " ltrim(rtrim(pd.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ")} pd.PedSecuencia = ? GROUP by pd.rowguid  order by p.ProDescripcion", new string[] { pedSecuencia.ToString() });

            SqliteManager.GetInstance().InsertAll(list);

            //if (DS_RepresentantesParametros.GetInstance().GetParOfertasUnificadas())
            //{
            //    SqliteManager.GetInstance().Execute("update ProductosTemp set  IndicadorOferta = 0 " +
            //        "where ifnull(IndicadorOferta, 0) = 1 and Precio > 0 and TitID = ? ", new string[] { ((int)Modules.PEDIDOS).ToString() });
            //}
        }


        public void InsertarPedidoInTempFromCotizaciones(int cotSecuencia, bool confirmado, string repcodigo)
        {
            myProd.ClearTemp((int)Modules.PEDIDOS);
            var query = "";

            var cantConfirmada = myParametro.GetParPedidosMostrarCantidadConfirmada() && confirmado ? "1 as ShowCantidadConfirmada, cd.PedCantidadConfirmada as CantidadConfirmada, " : "";

            if (myParametro.GetDescuentoxPrecioNegociado())
            {
                if (myParametro.GetParRevenimiento())
                {
                    query = "select " + ((int)Modules.PEDIDOS).ToString() + " as TitID, "+ cantConfirmada + " cd.cotCantidad as Cantidad, cd.cotCantidadDetalle as CantidadDetalleR, cd.rowguid as rowguid, cd.ProID as ProID, cd.cotPrecio as Precio, ((cd.cotPrecio - cd.cotDescuento) * ((cd.cotItbis / 100.0) + 1.0)) as PrecioTemp, I.AlmID AS AlmID, I.InvCantidad AS InvCantidad, " +
                     "p.ProDescripcion as Descripcion, cd.cotItbis as Itbis, cd.cotSelectivo as Selectivo, cd.cotAdvalorem as Advalorem, cd.cotDesPorciento as DesPorciento,  cd.cotDesPorciento as DesPorcientoManual, ifnull(cd.UnmCodigo, '') as UnmCodigo, " +
                     "ifnull(cd.cotIndicadorOferta, 0) as IndicadorOferta,ifnull(p.ProDatos3, '') as ProDatos3 , cd.cotDescuento as Descuento, cd.OfeID as OfeID, cd.ProID as ProIdOferta, cs.SecCodigo as SecCodigo, case when cd.OfeID != 0 then 1 else 0 end as IndicadorOfertaForShow from " + (confirmado ? "CotizacionesDetalleConfirmados" : "CotizacionesDetalle") + " cd " +
                     "inner join Productos p on p.ProID = cd.ProID inner join " + (confirmado ? "CotizacionesConfirmados" : "Cotizaciones") + " cs on cs.cotsecuencia = cd.cotsecuencia and cs.RepCodigo = cd.RepCodigo ";
                }
                else
                {
                    query = "select " + ((int)Modules.PEDIDOS).ToString() + " as TitID, "+ cantConfirmada + " cd.cotCantidad as Cantidad, cd.cotCantidadDetalle as CantidadDetalle, cd.rowguid as rowguid, cd.ProID as ProID, cd.cotPrecio as Precio, ((cd.cotPrecio - cd.cotDescuento) * ((cd.cotItbis / 100.0) + 1.0)) as PrecioTemp, I.AlmID AS AlmID, I.InvCantidad AS InvCantidad, " +
                    "p.ProDescripcion as Descripcion, cd.cotItbis as Itbis, cd.cotSelectivo as Selectivo, cd.cotAdvalorem as Advalorem, cd.cotDesPorciento as DesPorciento,  cd.cotDesPorciento as DesPorcientoManual, ifnull(cd.UnmCodigo, '') as UnmCodigo, " +
                    "ifnull(cd.cotIndicadorOferta, 0) as IndicadorOferta,ifnull(p.ProDatos3, '') as ProDatos3 , cd.cotDescuento as Descuento, cd.OfeID as OfeID, cd.ProID as ProIdOferta, cs.SecCodigo as SecCodigo, case when cd.OfeID != 0 then 1 else 0 end as IndicadorOfertaForShow from " + (confirmado ? "CotizacionesDetalleConfirmados" : "CotizacionesDetalle") + " cd " +
                    "inner join Productos p on p.ProID = cd.ProID inner join " + (confirmado ? "CotizacionesConfirmados" : "Cotizaciones") + " cs on cs.cotsecuencia = cd.cotsecuencia and cs.RepCodigo = cd.RepCodigo ";
                }
            }
            else
            {
                if (myParametro.GetParRevenimiento())
                {
                    query = "select " + ((int)Modules.PEDIDOS).ToString() + " as TitID, "+ cantConfirmada + " cd.cotCantidad as Cantidad, cd.cotCantidadDetalle as CantidadDetalleR, cd.rowguid as rowguid, cd.ProID as ProID, cd.cotPrecio as Precio, I.AlmID AS AlmID, I.InvCantidad AS InvCantidad, " +
                     "p.ProDescripcion as Descripcion, cd.cotItbis as Itbis, cd.cotSelectivo as Selectivo, cd.cotAdvalorem as Advalorem, cd.cotDesPorciento as DesPorciento,  cd.cotDesPorciento as DesPorcientoManual, ifnull(cd.UnmCodigo, '') as UnmCodigo, " +
                     "ifnull(cd.cotIndicadorOferta, 0) as IndicadorOferta,ifnull(p.ProDatos3, '') as ProDatos3 , cd.cotDescuento as Descuento, cd.OfeID as OfeID, cd.ProID as ProIdOferta, cs.SecCodigo as SecCodigo, case when cd.OfeID != 0 then 1 else 0 end as IndicadorOfertaForShow from " + (confirmado ? "CotizacionesDetalleConfirmados" : "CotizacionesDetalle") + " cd " +
                     "inner join Productos p on p.ProID = cd.ProID inner join " + (confirmado ? "CotizacionesConfirmados" : "Cotizaciones") + " cs on cs.cotsecuencia = cd.cotsecuencia and cs.RepCodigo = cd.RepCodigo ";
                }
                else
                {
                    query = "select " + ((int)Modules.PEDIDOS).ToString() + " as TitID, "+ cantConfirmada + " cd.cotCantidad as Cantidad, cd.cotCantidadDetalle as CantidadDetalle, cd.rowguid as rowguid, cd.ProID as ProID, cd.cotPrecio as Precio,  I.AlmID AS AlmID, I.InvCantidad AS InvCantidad, " +
                    "p.ProDescripcion as Descripcion, cd.cotItbis as Itbis, cd.cotSelectivo as Selectivo, cd.cotAdvalorem as Advalorem, cd.cotDesPorciento as DesPorciento,  cd.cotDesPorciento as DesPorcientoManual, ifnull(cd.UnmCodigo, '') as UnmCodigo, " +
                    "ifnull(cd.cotIndicadorOferta, 0) as IndicadorOferta,ifnull(p.ProDatos3, '') as ProDatos3 , cd.cotDescuento as Descuento, cd.OfeID as OfeID, cd.ProID as ProIdOferta, cs.SecCodigo as SecCodigo, case when cd.OfeID != 0 then 1 else 0 end as IndicadorOfertaForShow from " + (confirmado ? "CotizacionesDetalleConfirmados" : "CotizacionesDetalle") + " cd " +
                    "inner join Productos p on p.ProID = cd.ProID inner join " + (confirmado ? "CotizacionesConfirmados" : "Cotizaciones") + " cs on cs.cotsecuencia = cd.cotsecuencia and cs.RepCodigo = cd.RepCodigo ";
                }
            }

            string almacenDefault = myParametro.GetParAlmacenDefault();

            query += $" {(myParametro.GetParCantInvAlmacenes()? "inner" : "left")} join InventariosAlmacenes I on I.ProID = p.ProID {(string.IsNullOrEmpty(almacenDefault)? "" : $" and I.AlmId = {almacenDefault} ")}";


            var list = SqliteManager.GetInstance().Query<ProductosTemp>(query + $" where {(myParametro.GetParVendedorContVend()? " ltrim(rtrim(cd.RepCodigo)) = '" + repcodigo + "' and " : " ltrim(rtrim(cd.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ")} cd.CotSecuencia = ? GROUP by cd.rowguid  order by p.ProDescripcion", new string[] { cotSecuencia.ToString() });

            SqliteManager.GetInstance().InsertAll(list);

        }

        public void InsertarPedidoInTempFromCotizacionesAut(int cotSecuencia, bool confirmado, out int pedsecuencia)
        {
            pedsecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Pedidos");

            string query = $@"select * from {(confirmado ? "CotizacionesConfirmados" : "Cotizaciones")} 
                              Cotizaciones where CotSecuencia = {cotSecuencia}";

            var ped = new Hash("Pedidos");
            Cotizaciones values = SqliteManager.GetInstance().Query<Cotizaciones>(query).FirstOrDefault();
            var props = values.GetType().GetProperties().Where(v => !v.Name.Contains("CotSecuencia") && !v.Name.Contains("rowguid"));

            foreach (var prop in props)
            {
                if(prop.Name.Contains("CotEstatus"))
                {
                    ped.Add("PedEstatus", 2);
                    continue;
                }

                ped.Add(prop.Name.Replace("Cot", "Ped"), prop.GetValue(values));
            }

            ped.Add("PedSecuencia", pedsecuencia);
            ped.Add("rowguid", Guid.NewGuid().ToString());
            ped.ExecuteInsert();

            query = $@"select * from {(confirmado ? "CotizacionesDetalleConfirmados" : "CotizacionesDetalle")} 
                              Cotizaciones where CotSecuencia = {cotSecuencia}";

            List<CotizacionesDetalle> list = SqliteManager.GetInstance().Query<CotizacionesDetalle>(query);

            var parEditarPrecio = myParametro.GetParPedidosEditarPrecio();
            var parRevenimiento = myParametro.GetParRevenimiento();
            var parDescuentoNegociado = myParametro.GetDescuentoxPrecioNegociado();
            int pos = 1;

            foreach (var temp in list)
            {
                Hash det = new Hash("PedidosDetalle");
                det.Add("PedSecuencia", pedsecuencia);
                det.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);

                det.Add("ProID", temp.ProID);
                det.Add("CliID", Arguments.Values.CurrentClient.CliID);
                det.Add("PedCantidad", temp.CotCantidad);

                det.Add("PedCantidadDetalle", temp.CotCantidadDetalle);
                det.Add("PedPrecio", temp.CotPrecio);

                det.Add("PedFechaEntrega", temp.CotFechaEntrega);

                det.Add("CedCodigo", temp.CedCodigo);

                det.Add("PedItbis", temp.CotItbis);
                det.Add("PedSelectivo", temp.CotSelectivo);
                det.Add("PedAdValorem", temp.CotAdValorem);

                det.Add("PedIndicadorOferta", temp.CotIndicadorOferta);
                det.Add("PedIndicadorCompleto", 0);
                det.Add("OfeID", temp.OfeID);
                //porque lo pusieron asi, si ya este calculo se hace en ds_descuentosrecargos al momento 
                //de calcular el descuento en el metodo CalcularDescuentoNormalByTipo???????
                det.Add("PedDesPorciento", temp.CotDesPorciento);      //  temp.DesPorciento);
                det.Add("PedDescuento", temp.CotDescuento);
                det.Add("UnmCodigo", temp.UnmCodigo);
                det.Add("PedFechaActualizacion", Functions.CurrentDate());
                det.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);

                if (temp.CotIndicadorOferta || !ExistsDetalleInPedido(temp.rowguid, pedsecuencia))
                {
                    pos = GetMaxPositionFromDetalle(pedsecuencia) + 1;
                }

                det.Add("PedPosicion", pos);
                det.Add("rowguid", Guid.NewGuid().ToString());
                det.ExecuteInsert();
            }
        }

        public List<Pedidos> GetPedidosByCuadre(int CuaSecuencia)
        {
            var query = "Select c.CliID as CliID, c.CliNombre as CliNombre,C.CliCodigo as CliCodigo, p.PedSecuencia as PedSecuencia from Pedidos p " +
                             "Inner join Clientes c on p.CliID = c.CliID " +
                             "Where p.CuaSecuencia = " + CuaSecuencia + " and p.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "'  and p.PedEstatus <> 0 " +
                             "Order by p.PedSecuencia, c.cliID asc ";

            return SqliteManager.GetInstance().Query<Pedidos>(query, new string[] {});
        }

        public Pedidos GetPedidosByMaxSecuencia()
        {
            var query = "SELECT PedFecha,PedEstatus FROM Pedidos WHERE PedSecuencia = (SELECT MAX(PedSecuencia) FROM Pedidos) and cliid = " + Arguments.Values.CurrentClient.CliID;

            var list = SqliteManager.GetInstance().Query<Pedidos>(query, new string[] { });

            if(list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public int GetPedidosByPedSecuenciaForVenSec(int pedsecuencia)
        {
            var list = SqliteManager.GetInstance().Query<Pedidos>("select VenSecuencia from Pedidos where PedSecuencia =? ", new string[] { pedsecuencia.ToString()});

            if(list.Count > 0)
            {
                return list[0].VenSecuencia;
            }

            return 0;
        }

        public List<PedidosDetalle> GetPedidosByClientes(int CliID, int PedSecuencia)
        {
            string query = "select c.CliNombre, c.CliCodigo as CliCodigo, c.CliCalle, ifnull(c.CliUrbanizacion, ''), "
                 + "pd.PedSecuencia, pd.PedCantidad, p.ProCodigo , pd.PedPrecio AS 'PedPrecio', p.ProDescripcion, "
                 + "p.ProDescripcion1, c.CliSector AS 'CliSector', pd.PedItbis AS 'Itbis', pd.peddesporciento, "
                 + "ifnull(pd.PedCantidadDetalle, 0) AS 'PedCantidadDetalle', ifnull(pd.PedDescuento, 0) AS 'PedDescuento', "
                 + "ifnull(p.ProUnidades, 0) AS 'ProUnidades', PED.ofvCodigo, PED.SecCodigo, ifnull(PED.ConID, '') as ConID, "
                 + "ifnull(c.CliContacto, '') as CliContacto, ifnull(c.CliRNC, '') as CliRNC, ifnull(c.CliTelefono, '') as CliTelefono, "
                 + "ifnull(pd.PedSelectivo, 0) as PedSelectivo, ifnull(pd.PedAdvalorem, 0) as PedAdvalorem, pd.PedIndicadorOferta as PedIndicadorOferta "
                 + "from clientes c inner join productos p inner join pedidosdetalle pd " +
                 "inner join Pedidos PED on PED.PedSecuencia=pd.PedSecuencia " +
                 "where c.CliID = pd.CliID and c.CliID = " + CliID + " and p.ProID = pd.ProID " +
                 "and  pd.PedSecuencia = " + PedSecuencia + " " +
                 "Order by p.LinID, p.Cat1ID, p.ProID";

            return SqliteManager.GetInstance().Query<PedidosDetalle>(query, new string[] { });
        }

        public string GetCodigoBarrabyProCodigo(string ProCodigo)
        {
            string query = "SELECT ProReferencia FROM Productos where ProCodigo = '" + ProCodigo + "'";
            var list = SqliteManager.GetInstance().Query<Productos>(query, new string[] { });

            return list[0].ProReferencia;
        }

        public string GetMonSiglaPedido(int PedSecuencia, bool pedidoConfirmado)
        {
            string query = "SELECT m.MonSigla as MonSigla FROM " + (pedidoConfirmado ? "PedidosConfirmados" : "Pedidos") + " p " +
                "inner join Monedas m on m.MonCodigo = p.MonCodigo where PedSecuencia = '" + PedSecuencia + "'";
            var list = SqliteManager.GetInstance().Query<Monedas>(query, new string[] { });

            return list[0].MonSigla;
        }

        public string GetMonNombrePedido(int PedSecuencia, bool pedidoConfirmado)
        {
            string query = "SELECT m.MonNombre as MonNombre FROM " + (pedidoConfirmado ? "PedidosConfirmados" : "Pedidos") + " p " +
                "inner join Monedas m on m.MonCodigo = p.MonCodigo where PedSecuencia = '" + PedSecuencia + "'";
            var list = SqliteManager.GetInstance().Query<Monedas>(query, new string[] { });

            return list[0].MonNombre;
        }
        public double GetMontoPedido(int Pedsecuencia)
        {
            string query = "SELECT SUM(PedCantidad * (PedPrecio + (PedPrecio * (PedItbis / 100.0)) - ABS(PedDescuento))) FROM PedidosDetalle WHERE PedSecuencia = " + Pedsecuencia;
            var list = SqliteManager.GetInstance().Query<double>(query, new string[] { });

            return list != null && list.Count > 0 ? list[0] : 0.00;
        }

        public bool ExistsDiaFeriado(string FechaEntrega)
        {
            return SqliteManager.GetInstance().Query<Pedidos>("select DiaFecha as PedFecha from DiasFeriados " +
                "where STRFTIME('%Y-%m-%d', DiaFecha)  = '" + FechaEntrega + "'  ",
                new string[] { }).Count > 0;
        }

        public void GetMontosTotalesParaPedidos(int Pedidosecuencia, double montototal)
        {
            var result = SqliteManager.GetInstance().Query<ProductosTemp>($@"select sum(PedCantidad * (PedPrecio - Peddescuento)) AS PedMontoSinITBIS,
                             sum(Pedtotalitbis) as PedTotalItbis from PedidosDetalle where repcodigo = '{Arguments.CurrentUser.RepCodigo}'
                            and Pedsecuencia = {Pedidosecuencia}").FirstOrDefault();

            SqliteManager.GetInstance().Execute($@"update Pedidos set PedMontoSinItbis = {result.PedMontoSinITBIS}, PedMontoItbis = {result.PedTotalItbis},
                            PedMontoTotal = {montototal} where Pedsecuencia = {Pedidosecuencia} and repcodigo = '{Arguments.CurrentUser.RepCodigo}'");
        }

        public bool ExistPedidosCreditosEnVisitas()
        {
            return SqliteManager.GetInstance().Query<Pedidos>("select 1 from Pedidos " +
                "where ConId <> 1 and VisSecuencia = '" + Arguments.Values.CurrentVisSecuencia + "'  ",
                new string[] { }).Count > 0;
        }

        public void GuardarPedidosBusquedas(PedidosBusquedas data)
        {
            var map = new Hash("PedidosBusquedas");

            map.Add("RepCodigo", data.RepCodigo);
            map.Add("PedSecuencia", data.PedSecuencia);
            map.Add("PedCampo", data.PedCampo);
            map.Add("PedTipo", data.PedTipo);
            map.Add("PedFiltro", data.PedFiltro);
            map.Add("PedCodigo", data.PedCodigo);
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map.Add("PedFechaActualizacion", Functions.CurrentDate());
            map.Add("rowguid", Guid.NewGuid().ToString());
            map.ExecuteInsert();

        }
    }
}
