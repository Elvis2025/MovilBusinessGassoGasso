using MovilBusiness.Configuration;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Model.Internal.Structs.Args;
using MovilBusiness.Services;
using MovilBusiness.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MovilBusiness.DataAccess
{
    public class DS_Productos : DS_Controller
    {
        private ApiManager api;

        public DS_Productos()
        {
            try
            {
                api = ApiManager.GetInstance(new PreferenceManager().GetConnection().Url);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        public List<ProductosTemp> GetResumenProductos(int titId, bool withoutOfertas = true, bool validarCantidades = true, bool showDescuentos = false, bool showValidForOfertas = false, bool showCombo = true, bool showDescuentoIndicator = false, EntregasRepartidorTransacciones entrega = null, bool mostrarPromociones = true, string orderBy = null, bool forCalcularofertas = false, bool groupBy = false, bool VerDetalleInCont = false, bool isfromsave = false, bool isVerDetalleTrans = false, bool AgrupaLote = false, bool isEntrega = false, bool isFromPedidoDetalle = false, bool isForGuardar = false)
        {
            var par = myParametro.GetParInventarioFisicoArea();

            if (Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS)
            {
                par = myParametro.GetParColocacionProductosCapturarArea();
            }

            bool UseMultiplo = myParametro.GetParProductoMultiplos();
            string useMultiplo = "";
            if (UseMultiplo)
            {
                useMultiplo = "ifnull(P.ProCantidadMultiploVenta,'') as ProCantidadMultiploVenta,";
            }

            //Esto es para calcular el descuento maximo, favor no removerlo
            bool ParPedidosDescuentoMaximo = myParametro.GetParPedidosDescuentoMaximo();
            string ColumnDescMax = "";
            if (ParPedidosDescuentoMaximo && Arguments.Values.CurrentModule == Modules.PEDIDOS)
            {
                ColumnDescMax = "ifnull(P.ProDescuentoMaximo,'') as ProDescuentoMaximo,";
            }

            bool parInvArea = !string.IsNullOrWhiteSpace(par) && (par.ToUpper().Trim() == "D" || (isFromPedidoDetalle && par.ToUpper().Trim() == "C")) && (Arguments.Values.CurrentModule == Modules.INVFISICO || Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS);

            if (!string.IsNullOrWhiteSpace(orderBy) && orderBy.ToUpper().Contains("ORDER BY"))
            {
                orderBy = orderBy.ToUpper().Replace("ORDER BY", "");
            }

            var parventasLote = myParametro.GetParVentasLote();

            var agrupar = (((parventasLote == 1 || parventasLote == 2) && forCalcularofertas) || groupBy) && !myParametro.GetParNoAgruparEnVentas();

            var advalorem = " ifnull(ProAdValorem, 0) ";
            var joinTinIDAdValorem = "";
            if (myParametro.GetParTipoAdValorem() == 1)
            {
                advalorem = " (CAST(Precio AS REAL) * (CAST(IFNULL(AdValorem, 0.0) AS REAL) / 100.0)) ";
            }
            else if (myParametro.GetParTipoAdValorem() == 3)
            {
                advalorem = " ifnull(ptna.ProAdValorem, 0) ";
                joinTinIDAdValorem = " LEFT OUTER JOIN ProductosTiposNegocioAdvalorem ptna ON ptna.ProID = P.ProID and ptna.TinID = '" + Arguments.Values.CurrentClient.TiNID + "' ";
            }

            bool cantidadSugerida = Arguments.Values.CurrentModule == Modules.PEDIDOS && myParametro.GetParPedCantidadSugeridos();
            bool cantidadMinima = myParametro.GetParProdShowCantMinima();

            bool IsDescuentoDevoluciones = myParametro.GetParDescuentosEnDevoluciones();

            bool itbisCero = (Arguments.Values.CurrentClient != null && !string.IsNullOrEmpty(Arguments.Values.CurrentClient.CliTipoComprobanteFAC)
                && Arguments.Values.CurrentClient.CliTipoComprobanteFAC == "14" || Arguments.Values.CurrentClient.CliIndicadorExonerado)
                && (Arguments.Values.CurrentModule == Modules.VENTAS || Arguments.Values.CurrentModule == Modules.PEDIDOS
                || Arguments.Values.CurrentModule == Modules.COTIZACIONES || Arguments.Values.CurrentModule == Modules.DEVOLUCIONES);


            string query = "select " + (!agrupar ? "DISTINCT" : "") + " ifnull(t.EnrSecuencia,0) as EnrSecuencia, ifnull(t.TraSecuencia,0) as TraSecuencia,  p.ProImg as ProImg, t.UseAttribute1 as UseAttribute1, t.UseAttribute2 as UseAttribute2, t.ProAtributo1 as ProAtributo1, t.ProAtributo1Desc as ProAtributo1Desc, t.ProAtributo2Desc as ProAtributo2Desc, t.ProAtributo2 as Proatributo2, t.CantidadConfirmada, t.ShowCantidadConfirmada as ShowCantidadConfirmada, t.TipoCambio as TipoCambio, t.rowguid as rowguid, ifnull(t.PedFechaEntrega,'') as PedFechaEntrega, " + (Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.VENTAS ? "case when ifnull(DesPorciento, 0.0) > 0.0 then 1 else 0 end as ShowDescuentoPreview, " : "") + " t.IndicadorDocena as IndicadorDocena, t.Posicion, t.CantidadPiezas as CantidadPiezas, t.IndicadorPromocion as IndicadorPromocion, t.TitID as TitID, ifnull(p.ProDatos3, '') as ProDatos3, p.ProReferencia as ProReferencia, ifnull(P.ProCantidadMinVenta,'') as ProCantidadMinVenta,  ifnull(P.ProCantidadMaxVenta,'') as ProCantidadMaxVenta,  " + ColumnDescMax + "  1 as ShowCantidad, t.ProIDOferta as ProIDOferta, t.DesPorciento as DesPorciento, " + (showValidForOfertas ? "case when o.ProID is not null  and (ifnull(o.TieneOferta, 0) = 1 " + (isEntrega ? "" : "and IndicadorOfertaForShow = 1") + ") then 1 else 0 end" : " " + (isForGuardar ? " t.IndicadorOferta " : " case when ifnull(OfeCaracteristica,'') <> '' then 1 else t.IndicadorOferta end ") + "  ") + " as IndicadorOferta, " +
                "ifnull(t.DesIdsAplicados, '') as DesIdsAplicados, t.CantidadFacing as CantidadFacing, ifnull(trim(P.ProCodigo), '') as ProCodigo, p.ProPrecio3 as ProPrecio3, " + ((IsDescuentoDevoluciones || (showDescuentoIndicator && showValidForOfertas)) && !myParametro.GetParPedidosOfertasyDescuentosManuales() && myParametro.GetParPedidosDescuentoManualGeneral() <= 0.0 ? "case when o.ProID is not null  and  ifnull(o.TieneDescuento, 0) = 1 " + (IsDescuentoDevoluciones ? " and o.titId = 2 " : " ") + " then 1 else 0 end as IndicadorDescuento, " : "") + " " +
                "t.InvAreaDescr as InvAreaDescr, InvAreaId, " + (parInvArea ? 1 : 0) + " as UseInvArea, P.ProID as ProID, ifnull(trim(P.ProDescripcion), '') as Descripcion, " +
                "ifnull(Precio, 0.0) as Precio, ifnull(DesPorcientoOriginal,0.0) as DesPorcientoOriginal, ifnull(PrecioMoneda, 0.0) as PrecioMoneda, " + (agrupar && !VerDetalleInCont ? "sum(" : "") + (myParametro.GetParInventariosTomarCantidades() <= 0 || myParametro.GetParColocacionProductosTomarCantidades() <= 0 ? "ifnull(t.Cantidad, 0)" : " t.Cantidad ") + (agrupar && !VerDetalleInCont ? ")" : "") + " as Cantidad, " + (myParametro.GetParInventariosTomarCantidades() > 0 || myParametro.GetParColocacionProductosTomarCantidades() > 0 ? "ifnull(CantidadDetalle, 0) as CantidadDetalle" : "CantidadDetalle") + " , ifnull(CantidadDetalleR, 0) as CantidadDetalleR, " + (Arguments.Values.CurrentModule == Modules.COMPRAS || Arguments.Values.CurrentModule == Modules.INVFISICO || Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS || Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS || Arguments.Values.CurrentModule == Modules.TRASPASOS || Arguments.Values.CurrentModule == Modules.REQUISICIONINVENTARIO ? "0 as Selectivo" : "ifnull(ProSelectivo, 0) as Selectivo") +
                ", " + (Arguments.Values.CurrentModule == Modules.COMPRAS || Arguments.Values.CurrentModule == Modules.INVFISICO || Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS || Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS || Arguments.Values.CurrentModule == Modules.TRASPASOS || Arguments.Values.CurrentModule == Modules.REQUISICIONINVENTARIO ? "0 as AdValorem" : advalorem + " as AdValorem") + ", " + useMultiplo + " " +
                 (Arguments.Values.CurrentModule == Modules.COMPRAS || itbisCero ? "0" : "ifnull(ProItbis, 0)") + " as Itbis, t.IndicadorOfertaForShow as IndicadorOfertaForShow, ifnull(t.UnmCodigo, '') as UnmCodigo, ifnull(P.ProDescripcion1, '') as ProDescripcion1, ifnull(P.ProDescripcion2, '') as ProDescripcion2, " +
                 "ifnull(P.ProDescripcion3, '') as ProDescripcion3, t.PrecioCaja, t.DesPorcientoManual as DesPorcientoManual, ifnull(p.ProDatos1, '') as ProDatos1, ifnull(p.ProDatos2, '') as ProDatos2, t.InvCantidad as InvCantidad, t.InvCantidadDetalle as InvCantidadDetalle, " +
                 "Accion, Lote, ifnull(t.invLote,'') as invLote , FechaVencimiento, Documento " + (myParametro.GetFormatoVisualizacionProductosLocal() == 11 || myParametro.GetParMostrarProductosEnInventarioParaInventarios() ? ", t.LipPrecioSugerido as LipPrecioSugerido " : "") + ", t.OfeID as OfeID, case when ifnull(DesPorcientoManual, 0) > 0 or ShowDescuento = 1 then 1 else 0 end or " + (showDescuentos ? "1" : "0") + " as ShowDescuento, " +
                 "t.Descuento as Descuento, t.CantidadEntrega as CantidadEntrega, CantidadOferta, t.LipPrecioMinimo as LipPrecioMinimo, t.PrecioTemp as PrecioTemp, p.ProIndicadorDetalle as IndicadorDetalle, ifnull(MotIdDevolucion,-1) as MotIdDevolucion, ifnull(p.ProUnidades, 0) as ProUnidades " + (Arguments.Values.CurrentModule == Modules.DEVOLUCIONES ? ", m.MotDescripcion" : "") + ", t.AlmID as AlmID, ifnull(t.CedCodigo,'') as CedCodigo, ifnull(t.CedDescripcion,'') as CedDescripcion, CantidadAlm, UnidadAlm, CanTidadGond, UnidadGond, CanTidadTramo, LoteEntregado, LoteRecibido, t.ProPosicion, ifnull(p.ProPeso,0) as ProPeso,ifnull(p.LinId,0) as Linea, ifnull(t.PedFlete,0) as PedFlete " +
                 (showValidForOfertas || showDescuentoIndicator ? ",o.TieneDescuentoEscala as TieneDescuentoEscala " : " ") + (cantidadSugerida ? ",ps.PedCantidad as PedCantidad, ps.PedCantidadDetalle as PedCantidadDetalle " : "") + (cantidadMinima ? " ,cm.CliCanTidadMinima as CliCanTidadMinima " : "") + ((myParametro.GetParDevolucionCondicion() || myParametro.GetParDevolucionesCondicionUnico()) && Arguments.Values.CurrentModule == Modules.DEVOLUCIONES ? ",t.DevCondicion, t.DevDescripcion " : " ") + (isFromPedidoDetalle && myParametro.GetParInventariosTomarCantidades() == 3 ? " , CheckValueForInvFis, 1 as CheckDetailsForInvFis, RadioButtonNotEnabled " : " , CheckValueForInvFis ") + " ,t.CantidadConfirmada " +
                 "from ProductosTemp t " +
                 "inner join Productos p on p.ProID = t.ProID " +
                 (cantidadSugerida ? "left join PedidosSugeridos ps on ps.ProID = p.ProID " : " ") + (cantidadMinima ? "left join ClientesInventariosMinimos cm on cm.ProID = p.ProID and cm.cliid = " + Arguments.Values.CurrentClient.CliID + " " : "") +
                 // (entrega != null ? "left join (select e.ProID as ProID, (ifnull(e.TraCantidadDetalle, 0) / case when ifnull(pp.ProUnidades, 0) = 0 then 1 else pp.ProUnidades end) + ifnull(e.TraCantidad, 0) as TraCantidad from EntregasRepartidorTransaccionesDetalle e inner join Productos pp on pp.ProID = e.ProID where e.EnrSecuencia = " + entrega.EnrSecuencia + " and e.TitID = 4 and trim(e.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and e.TraSecuencia = " + entrega.TraSecuencia + " group by e.ProID, pp.ProUnidades) e on e.ProID = p.ProID " : "") + " " +
                 (IsDescuentoDevoluciones || showValidForOfertas || showDescuentoIndicator ? "left join ProductosValidosOfertas o on o.ProID = t.ProID " + (myParametro.GetParProdUseUnmCodigo() ? " and o.UnmCodigo = t.UnmCodigo " : " ") + " and o.CliID = " + Arguments.Values.CurrentClient.CliID + " " : " ") +
                 (Arguments.Values.CurrentModule == Modules.DEVOLUCIONES ? "left join MotivosDevolucion m on MotID = MotIdDevolucion " : "") +
                 (!string.IsNullOrEmpty(joinTinIDAdValorem) ? joinTinIDAdValorem : "") +
                 "where t.TitID = " + titId.ToString() + " " + (!mostrarPromociones ? " and ifnull(t.IndicadorPromocion, 0) = 0 " : "") + " and p.ProID is not null " + ((validarCantidades && !myParametro.GetParInventarioFisicoAceptarProductosCantidadCero()) && Arguments.Values.CurrentModule != Modules.CONTEOSFISICOS && !VerDetalleInCont ? " and (Cantidad > 0 or CantidadDetalle > 0)" : "") +
                 " " + (myParametro.GetParSectores() > 0 && !isVerDetalleTrans && Application.Current.Properties.ContainsKey("SecCodigo") ? "and t.SecCodigo = '" + Application.Current.Properties["SecCodigo"] + "'" : "") + " " + (withoutOfertas && !isVerDetalleTrans ? " and ifnull(t.IndicadorOferta, 0) = 0 " : "");

            if (entrega != null)
            {
                query += " and case when ifnull(t.ProDatos3, '') like '%L%' then ifnull(t.Lote, '') != '' else 1=1 end ";
            }
            else if (Arguments.Values.CurrentModule == Modules.VENTAS && myParametro.GetParVentaRancheraConLote())
            {
                query += " and case when ifnull(t.ProDatos3, '') like '%L%' and  t.CantidadEntrega = 0  then ifnull(t.Lote, '') != '' else 1=1 end ";
            }

            if ((agrupar && !AgrupaLote) || ((myParametro.GetParInventariosTomarCantidades() > 0 || myParametro.GetParColocacionProductosTomarCantidades() > 0) && !isfromsave))
            {
                query += " group by p.ProID " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? ", t.UnmCodigo " : "") + " ";
            }
            else if (AgrupaLote)
            {
                query += " group by p.ProID, t.Lote, ifnull(t.IndicadorOferta, 0)  " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? ", t.UnmCodigo " : "") + " ";
            }

            query += (string.IsNullOrWhiteSpace(orderBy) ? " order by p.ProDescripcion" : " order by " + orderBy);
            var productosTemp = SqliteManager.GetInstance().Query<ProductosTemp>(query, new string[] { });
            if (DS_RepresentantesParametros.GetInstance().GetParMostrarVariosInventariosEnRow())
            {
                var almacenesSD = SqliteManager.GetInstance().Query<Almacenes>("select AlmID, AlmDescripcion, AlmReferencia, ifnull(AlmCaracteristicas, '') as AlmCaracteristicas from Almacenes where AlmDescripcion like '%SD%'");
                var almacenesLV = SqliteManager.GetInstance().Query<Almacenes>("select AlmID, AlmDescripcion, AlmReferencia, ifnull(AlmCaracteristicas, '') as AlmCaracteristicas from Almacenes where AlmDescripcion like '%LV%'");

                foreach (var prod in productosTemp)
                {
                    prod.InvCantidadAlmSD = new DS_inventariosAlmacenes().GetInventarioProductoByAlmacen(prod.ProID, almacenesSD.FirstOrDefault()?.AlmID ?? 0) ?? 0;
                    prod.InvCantidadAlmLV = new DS_inventariosAlmacenes().GetInventarioProductoByAlmacen(prod.ProID, almacenesLV.FirstOrDefault()?.AlmID ?? 0) ?? 0;
                }
            }
            if (DS_RepresentantesParametros.GetInstance().GetParMostrarVariosInventariosEnRow())
            {
                var query1 = $@"SELECT um.Descripcion,* FROM PedidosDetalleConfirmados pdc
                                    INNER JOIN UsosMultiples um ON um.CodigoUso = pdc.idMotivo
                                    Where pdc.PedSecuencia = 18007";
                foreach (var prod in productosTemp)
                {

                    prod.MotivoPedidosDetalle = new DS_Pedidos().GetMotivoPedidos(prod.rowguid);



                }
            }

                return productosTemp;
        }

        public double CantidadHolgura(int proId)
        {
            var query = "select p.ProHolgura from Productos p where p.ProID = ? ";

            var args = new string[] { proId.ToString() };

            var list = SqliteManager.GetInstance().Query<Productos>(query,
                args);

            if (list != null && list.Count > 0)
            {
                var inv = list[0];
                double cantidadHolgura = ((inv.ProHolgura / 100) + 1);
                return cantidadHolgura;
            }

            return 1;
        }

        public Productos ProCantidadHolgura(int proId)
        {
            var query = "select p.ProHolgura from Productos p where p.ProID = ? ";

            var args = new string[] { proId.ToString() };

            var list = SqliteManager.GetInstance().Query<Productos>(query, args);

            if (list != null && list.Count > 0)
            {
                return list.FirstOrDefault();
            }

            return null;
        }
        public List<ProductosTemp> GetProductosOfertas(int titId)
        {
            return SqliteManager.GetInstance().Query<ProductosTemp>("select * from ProductosTemp where IndicadorOferta = 1 and TitID = ? and Cantidad > 0",
                new string[] { titId.ToString() });
        }

        public bool ExistsOfertaInTemp(int ofeId, int TitId)
        {
            try
            {
                return SqliteManager.GetInstance().Query<ProductosTemp>("select ProID from ProductosTemp " +
                    "where OfeID = ? and TitID = ? and IndicadorOferta = 1 limit 1",
                    new string[] { ofeId.ToString(), TitId.ToString() }).FirstOrDefault() != null;

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return false;
            }
        }

        public double GetCantidadTotalInTemp(int titId, bool withoutOfertas = false, bool cantidadDetallada = false, int proID = 0, int posicion = -1, string UnmCodigo = "")
        {
            List<ProductosTemp> list = SqliteManager.GetInstance().Query<ProductosTemp>("select " + (cantidadDetallada ? "SUM(ifnull(CantidadDetalle, 0)) + sum(ifnull(Cantidad, 0) * ProUnidades)" : "SUM(ifnull(Cantidad, 0))") + " as Cantidad from " +
                "ProductosTemp t where " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() && !string.IsNullOrWhiteSpace(UnmCodigo) ? " UnmCodigo = '" + UnmCodigo + "' and " : "") + " TitID = " + titId.ToString() + " " + (withoutOfertas ? " and ifnull(IndicadorOferta, 0) = 0 " : "") + " " +
                "and ProID = " + proID + (posicion != -1 ? " and Posicion = " + posicion.ToString() : "") + " " +
                "and ((case when exists(select ProID from ProductosTemp w where w.Posicion = t.Posicion and w.TitID = t.TitID " +
                "and w.ProID = t.ProID and ifnull(w.Lote, '') != '' and ifnull(Cantidad, 0) > 0) then Lote != '' else 1=1 end and t.ProDatos3 like '%L%') " +
                "OR t.ProDatos3 not like '%L%')", new string[] { });

            if (list != null && list.Count > 0)
            {
                return list[0].Cantidad;
            }
            return 0;
        }

        public ProductosTemp GetProductoById(int proID, string monCodigo = null)
        {
            var args = new ProductosArgs
            {
                valueToSearch = null,
                lipCodigo = myParametro.GetParSectores() >= 2 && Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.LipCodigo : Arguments.Values.CurrentClient != null ? Arguments.Values.CurrentClient.LiPCodigo : "Default",
                filter = null,
                MonCodigo = string.IsNullOrWhiteSpace(monCodigo) ? Arguments.Values.CurrentClient.MonCodigo : monCodigo,
                FiltrarProductosPorSector = myParametro.GetParSectores() > 0 && myParametro.GetParFiltrarProductosPorSector(),
                ProID = proID,
            };

            var result = GetProductos(args);

            return result.FirstOrDefault();
        }
        public List<ProductosTemp> GetProductos(ProductosArgs args, BusquedaAvanzadaProductosArgs filtroAvanzado = null)
        {
            string whereCondition = "";

            if (filtroAvanzado == null)
            {
                if (args.filter != null)
                {
                    whereCondition = Functions.DinamicFiltersGenerateScript(args.filter, args.valueToSearch, args.secondFilter);
                }
            }
            else
            {
                /*  if (filtroAvanzado.Cat1ID != 0 && myParametro.GetParPedidosFiltrarProductosPorCategoria1())
                  {
                      whereCondition += " and p.Cat1ID like '%" + filtroAvanzado.Cat1ID.ToString() + "%'";
                  }
                  else if (filtroAvanzado.Cat2ID != 0 && myParametro.GetParPedidosFiltrarProductosPorCategoria2())
                  {
                      whereCondition += " and p.Cat2ID like '%" + filtroAvanzado.Cat2ID.ToString() + "%'";
                  }
                  else*/
                if (filtroAvanzado.Cat3ID != 0)
                {
                    whereCondition += " and p.Cat3ID like '%" + filtroAvanzado.Cat3ID.ToString() + "%'";
                }

                if (!string.IsNullOrWhiteSpace(filtroAvanzado.ProDescripcion1))
                {
                    whereCondition += " and p.ProDescripcion1 like '%" + filtroAvanzado.ProDescripcion1 + "%'";
                }

                if (!string.IsNullOrWhiteSpace(filtroAvanzado.ProDescripcion2))
                {
                    whereCondition += " and p.ProDescripcion2 like '%" + filtroAvanzado.ProDescripcion2 + "%'";
                }

                if (!string.IsNullOrWhiteSpace(filtroAvanzado.ProCodigo))
                {
                    whereCondition += " and (p.ProCodigo like '%" + filtroAvanzado.ProCodigo + "%' or p.ProDescripcion2 like '%" + filtroAvanzado.ProCodigo + "%')";
                }

                if (!string.IsNullOrWhiteSpace(filtroAvanzado.ProDescripcion))
                {
                    whereCondition += " and p.ProDescripcion like '%" + filtroAvanzado.ProDescripcion + "%'";
                }

                if (!string.IsNullOrWhiteSpace(filtroAvanzado.ProDatos1))
                {
                    whereCondition += " and p.ProDatos1 like '%" + filtroAvanzado.ProDatos1 + "%'";
                }

                if (!string.IsNullOrWhiteSpace(filtroAvanzado.ProReferencia))
                {
                    whereCondition += " and p.ProReferencia like '%" + filtroAvanzado.ProReferencia + "%'";
                }
                if (!string.IsNullOrWhiteSpace(filtroAvanzado.ProDescripcion3))
                {
                    whereCondition += " and p.ProDescripcion3 like '%" + filtroAvanzado.ProDescripcion3 + "%'";
                }
                if (!string.IsNullOrWhiteSpace(filtroAvanzado.ProDatos2))
                {
                    whereCondition += " and p.ProDatos2 like '%" + filtroAvanzado.ProDatos2 + "%'";
                }

                //////
                if (!string.IsNullOrWhiteSpace(filtroAvanzado.ProColor))
                {
                    whereCondition += " and p.ProColor like '%" + filtroAvanzado.ProColor + "%'";
                }
                if (!string.IsNullOrWhiteSpace(filtroAvanzado.ProPaisOrigen))
                {
                    whereCondition += " and p.ProPaisOrigen like '%" + filtroAvanzado.ProPaisOrigen + "%'";
                }
                if (!string.IsNullOrWhiteSpace(filtroAvanzado.ProAnio))
                {
                    whereCondition += " and (p.ProAnio like '%" + filtroAvanzado.ProAnio + "%' or p.ProDescripcion2 like '%" + filtroAvanzado.ProAnio + "%')";
                }
                if (!string.IsNullOrWhiteSpace(filtroAvanzado.ProMedida))
                {
                    whereCondition += " and p.ProMedida like '%" + filtroAvanzado.ProMedida + "%'";
                }

            }
            if (ExistenProductosBloqueados() > 0 && Arguments.Values.CurrentModule != Modules.CONTEOSFISICOS && Arguments.Values.CurrentModule != Modules.REQUISICIONINVENTARIO && Arguments.Values.CurrentModule != Modules.TRASPASOS)
            {
                whereCondition += " And P.Proid not in (Select Proid From ClientesProductosBloqueos Where Clicodigo = '" + Arguments.Values.CurrentClient.CliCodigo + "')";
            }

            if (Arguments.Values.CurrentModule != Modules.CONTEOSFISICOS && Arguments.Values.CurrentModule != Modules.REQUISICIONINVENTARIO && Arguments.Values.CurrentModule != Modules.TRASPASOS && Arguments.Values.CurrentClient != null && SqliteManager.ExistsTable("ClientesProductosExclusivos"))
            {
                whereCondition += " and (p.ProID in (select distinct ProID from ClientesProductosExclusivos where CliID = " + Arguments.Values.CurrentClient.CliID + ") or " +
                    "p.ProID not in (select distinct ProID from ClientesProductosExclusivos where CliID != " + Arguments.Values.CurrentClient.CliID + ")) ";
            }

            if (args.ProID != -1)
            {
                whereCondition += " and p.ProID = " + args.ProID.ToString() + " ";
            }

            if (!string.IsNullOrEmpty(args.ProUndMedidas))
            {
                whereCondition += " and l.UnmCodigo = '" + args.ProUndMedidas.ToString() + "' ";
            }

            if (Arguments.Values.CurrentModule == Modules.DEVOLUCIONES)
            {
                var unmCodigoDev = myParametro.GetParUnmCodigoEspecificaDevoluciones();
                if (!string.IsNullOrWhiteSpace(unmCodigoDev))
                    whereCondition += " and l.UnmCodigo = '" + unmCodigoDev + "' ";
            }

            if (!string.IsNullOrWhiteSpace(args.referenceSplit))
            {
                whereCondition += " and (ifnull(p.ProCodigo, '') like '" + args.referenceSplit.Trim() + "-%' or ifnull(p.ProCodigo, '') = '" + args.referenceSplit.Trim() + "') ";
            }

            if (!string.IsNullOrWhiteSpace(args.ProCodigo))
            {
                whereCondition += " and ifnull(trim(p.ProCodigo), '') = '" + args.ProCodigo.Trim() + "'";
            }

            var useProPrecios = !string.IsNullOrWhiteSpace(args.lipCodigo) ? args.lipCodigo == "*P.ProPrecio*" || args.lipCodigo == "*P.ProPrecio2*" || args.lipCodigo == "*P.ProPrecio3*" : false;
            bool parNoListaPrecios = myParametro.GetParNoListaPrecios() && string.IsNullOrEmpty(args.ProUndMedidas);
            bool useListaPrecios = !parNoListaPrecios && Arguments.Values.CurrentModule != Modules.DEVOLUCIONES && Arguments.Values.CurrentModule != Modules.INVFISICO && Arguments.Values.CurrentModule != Modules.COLOCACIONMERCANCIAS && Arguments.Values.CurrentModule != Modules.CONTEOSFISICOS && Arguments.Values.CurrentModule != Modules.TRASPASOS && Arguments.Values.CurrentModule != Modules.PROMOCIONES && Arguments.Values.CurrentModule != Modules.REQUISICIONINVENTARIO;

            var par = myParametro.GetParInventarioFisicoArea();

            if (Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS)
            {
                par = myParametro.GetParColocacionProductosCapturarArea();
            }

            bool parInvArea = !string.IsNullOrWhiteSpace(par) && par.ToUpper().Trim() == "D" && (Arguments.Values.CurrentModule == Modules.INVFISICO || Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS);

            var parOfertaColor = ((Arguments.Values.CurrentModule == Modules.PEDIDOS && !myParametro.GetParPedidosOfertasyDescuentosManuales()) || Arguments.Values.CurrentModule == Modules.VENTAS || Arguments.Values.CurrentModule == Modules.PRODUCTOS);

            var showDescuentoIndicador = Arguments.Values.CurrentModule == Modules.PEDIDOS
                || Arguments.Values.CurrentModule == Modules.VENTAS
                || (Arguments.Values.CurrentModule == Modules.PRODUCTOS
                && myParametro.GetParDescuentosProductosMostrarPreview())
                || myParametro.GetParCotizacionesOfertasyDescuentos()
                || myParametro.GetParDescuentosEnDevoluciones();

            //if (myParametro.GetParCotizacionesOfertasyDescuentos())
            //{
            //    showDescuentoIndicador = Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.VENTAS || Arguments.Values.CurrentModule == Modules.COTIZACIONES;
            //}

            bool useClientesPrecios = Arguments.Values.CurrentClient != null &&
                (Arguments.Values.CurrentModule == Modules.PEDIDOS
                || Arguments.Values.CurrentModule == Modules.VENTAS
                || Arguments.Values.CurrentModule == Modules.COTIZACIONES
                || Arguments.Values.CurrentModule == Modules.ENTREGASMERCANCIA
                || (Arguments.Values.CurrentModule == Modules.DEVOLUCIONES && myParametro.GetParDevolucionesConListaPrecios()));

            bool ConvertirMoneda = myParametro.GetParConvertiPedidoMultiMoneda() && (Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.VENTAS || Arguments.Values.CurrentModule == Modules.COTIZACIONES || Arguments.Values.CurrentModule == Modules.PRODUCTOS);
            var unidadesMedidasValidas = myParametro.GetParUnidadesMedidasVendedorUtiliza();

            var parPreciosPorUnmCodigo = !parNoListaPrecios && !string.IsNullOrWhiteSpace(unidadesMedidasValidas) && (useClientesPrecios || Arguments.Values.CurrentModule == Modules.COTIZACIONES);

            bool UseMultiplo = myParametro.GetParProductoMultiplos();
            string useMultiplo = "";
            // int cantidadDivisora = 0;

            if (UseMultiplo)
            {
                useMultiplo = "ifnull(P.ProCantidadMultiploVenta,'') as ProCantidadMultiploVenta,";
            }

            //Esto es para calcular el descuento maximo, favor no removerlo 
            bool ParPedidosDescuentoMaximo = myParametro.GetParPedidosDescuentoMaximo();
            string ColumnDescMax = "";
            if (ParPedidosDescuentoMaximo && Arguments.Values.CurrentModule == Modules.PEDIDOS)
            {
                ColumnDescMax = "ifnull(P.ProDescuentoMaximo,'') as ProDescuentoMaximo,";
            }
            string unmSelect = "";

            bool noShowProdInTemp = myParametro.GetParNoShowProInTemp();

            if (!noShowProdInTemp)
            {
                if (parNoListaPrecios)
                {
                    unmSelect = "ifnull(t.UnmCodigo, ifnull(P.UnmCodigo, '')) as UnmCodigo, ";
                }
                else
                {
                    unmSelect = "ifnull(t.UnmCodigo, ifnull(l.UnmCodigo, ifnull(P.UnmCodigo, ''))) as UnmCodigo, ";
                }

            }

            if ((useClientesPrecios && !parNoListaPrecios) || (Arguments.Values.CurrentModule == Modules.DEVOLUCIONES && myParametro.GetParDevolucionesConListaPrecios()))
            {
                unmSelect = "ifnull(lpc.UnmCodigo, l.UnmCodigo) as UnmCodigo, ";
            }

            string selectPrecioCaja = "";

            if (myParametro.GetParCajasUnidadesProductos() && !parNoListaPrecios && useClientesPrecios)
            {
                selectPrecioCaja = "ifnull(((ifnull(" + (useClientesPrecios ? "IFNULL(lpc.LipPrecio, l.LipPrecio)" : "l.LipPrecio") + ", 0)) * case when ifnull(P.ProUnidades, 0) == 0 then 1 else P.ProUnidades end), 0) as PrecioCaja, ";
            }
            string QueryConvertirPrecio = "", JoinConversion = "";

            if (ConvertirMoneda)
            {
                QueryConvertirPrecio = " IFNULL((CAST(M2.MonTasa as real)/cast(MONEDABASE.MonTasa as real) * cast(IFNULL(LPC.LipPrecio, L.LipPrecio) as real)),(CAST(M.MonTasa as real)/cast(MONEDABASE.MonTasa as real) * cast(IFNULL(LPC.LipPrecio, L.LipPrecio) AS REAL )))";
                JoinConversion = (useProPrecios ? " LEFT " : " INNER ") + " JOIN Monedas M on M.MonCodigo = L.MonCodigo LEFT OUTER JOIN Monedas M2 on M2.MonCodigo = LPC.MonCodigo LEFT OUTER JOIN MONEDAS MONEDABASE ON MONEDABASE.MonCodigo = '" + args.MonCodigo + "' ";

                if (Arguments.Values.CurrentModule == Modules.PRODUCTOS)
                {
                    QueryConvertirPrecio = "(CAST(M.MonTasa as real)/cast(MONEDABASE.MonTasa as real) * cast(IFNULL(L.LipPrecio, 0.0) AS REAL ))";
                    JoinConversion = (useProPrecios ? " LEFT " : " INNER ") + " JOIN Monedas M on M.MonCodigo = L.MonCodigo LEFT OUTER JOIN MONEDAS MONEDABASE ON MONEDABASE.MonCodigo = '" + args.MonCodigo + "' ";
                }
            }

            var advalorem = " ifnull(ProAdValorem, 0) ";
            var joinTinIDAdValorem = "";
            if (myParametro.GetParTipoAdValorem() == 1)
            {
                advalorem = " (CAST(Precio AS REAL) * (CAST(IFNULL(AdValorem, 0.0) AS REAL) / 100.0)) ";
            }
            else if (myParametro.GetParTipoAdValorem() == 3 && Arguments.Values.CurrentModule != Modules.PRODUCTOS && Arguments.Values.CurrentModule != Modules.CONTEOSFISICOS && Arguments.Values.CurrentModule != Modules.INVFISICO && Arguments.Values.CurrentModule != Modules.COLOCACIONMERCANCIAS && Arguments.Values.CurrentModule != Modules.TRASPASOS && Arguments.Values.CurrentModule != Modules.PROMOCIONES && Arguments.Values.CurrentModule != Modules.REQUISICIONINVENTARIO)
            {
                advalorem = " ifnull(ptna.ProAdValorem, 0) ";
                joinTinIDAdValorem = " LEFT OUTER JOIN ProductosTiposNegocioAdvalorem ptna ON ptna.ProID = P.ProID and ptna.TinID = '" + Arguments.Values.CurrentClient.TiNID + "' ";
            }

            var itbisCero = (Arguments.Values.CurrentClient != null
                && (Arguments.Values.CurrentClient.CliTipoComprobanteFAC == "14"
                || Arguments.Values.CurrentClient.CliIndicadorExonerado))
                && (Arguments.Values.CurrentModule == Modules.VENTAS
                || Arguments.Values.CurrentModule == Modules.PEDIDOS
                || Arguments.Values.CurrentModule == Modules.COTIZACIONES
                || Arguments.Values.CurrentModule == Modules.DEVOLUCIONES);

            var parMultiAlmacenes = myParametro.GetParUsarMultiAlmacenes() && Arguments.Values.CurrentModule == Modules.VENTAS;
            var preciolistasinitbis = myParametro.GetParPedidosEditarPrecioNegconItebis() && Arguments.Values.CurrentModule == Modules.PEDIDOS && !itbisCero;

            bool cantidadSugerida = Arguments.Values.CurrentModule == Modules.PEDIDOS && myParametro.GetParPedCantidadSugeridos();
            bool cantidadMinima = myParametro.GetParProdShowCantMinima();
            string select;

            bool IsDescuentoDevoluciones = myParametro.GetParDescuentosEnDevoluciones();

            if (Arguments.Values.CurrentModule == Modules.DEVOLUCIONES && myParametro.GetParDevolucionesConListaPrecios())
            {
                itbisCero = myParametro.GetParDevolucionSinItbis();

                select = "select distinct " + selectPrecioCaja + " p.ProImg as ProImg, t.PedFechaEntrega as PedFechaEntrega, p.ProReferencia as ProReferencia, t.CantidadPiezas as CantidadPiezas, ifnull(t.IndicadorPromocion, 0) as IndicadorPromocion, " + ((int)Arguments.Values.CurrentModule).ToString() + " as TitID, ifnull(p.ProDatos3, '') as ProDatos3, t.Descuento, t.DesPorciento as DesPorciento, " + (useListaPrecios ? "l.LipPrecioMinimo as LipPrecioMinimo, " : "") + " t.DesPorcientoManual as DesPorcientoManual, case when ifnull(DesPorcientoManual, 0) > 0 then 1 else 0 end as ShowDescuento, " +
                "t.rowguid as rowguid, t.IndicadorDocena as IndicadorDocena, " + (Arguments.Values.CurrentModule == Modules.PRODUCTOS ? "0" : "1") + " as ShowCantidad, ifnull(P.ProCodigo, '') as ProCodigo, " + (DS_RepresentantesParametros.GetInstance().GetParPedProAlmBySector() ? "ifnull(a.AlmDescripcion, '') as AlmDescripcion, " : "") + "P.ProPrecio3 as ProPrecio3, t.InvAreaDescr as InvAreaDescr, InvAreaId, " + (parInvArea ? 1 : 0) + " as UseInvArea, ifnull(P.ProCantidadMinVenta,'') as ProCantidadMinVenta,  ifnull(P.ProCantidadMaxVenta,'') as ProCantidadMaxVenta,  " + ColumnDescMax + "  " +
                "P.ProID as ProID, ifnull(P.ProDescripcion, '') as Descripcion, " + (myParametro.GetParCantInvAlmacenes() ? "ifnull(I.AlmID, 0) as AlmID," : "") + " ifnull(t.Precio, ifnull(" + (useProPrecios ? args.lipCodigo.Replace("*", "") : Arguments.Values.CurrentModule == Modules.INVFISICO || Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS || Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS || Arguments.Values.CurrentModule == Modules.TRASPASOS || Arguments.Values.CurrentModule == Modules.REQUISICIONINVENTARIO || Arguments.Values.CurrentModule == Modules.PROMOCIONES ? "0" : parNoListaPrecios && Arguments.Values.CurrentModule != Modules.DEVOLUCIONES ? "P.ProPrecio" : ConvertirMoneda ? QueryConvertirPrecio : (preciolistasinitbis ? (useClientesPrecios ? "IFNULL(lpc.LipPrecio / ((p.ProItbis / 100.00) + 1.00), l.LipPrecio / ((p.ProItbis / 100.00) + 1.00))" : "l.LipPrecio") : (useClientesPrecios ? "IFNULL(lpc.LipPrecio , l.LipPrecio)" : "l.LipPrecio"))) + ", 0.0)) as Precio, " +
                 " ifnull(" + (useProPrecios ? args.lipCodigo.Replace("*", "") : Arguments.Values.CurrentModule == Modules.INVFISICO || Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS || Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS || Arguments.Values.CurrentModule == Modules.TRASPASOS || Arguments.Values.CurrentModule == Modules.PROMOCIONES || Arguments.Values.CurrentModule == Modules.REQUISICIONINVENTARIO ? "0" : parNoListaPrecios && Arguments.Values.CurrentModule != Modules.DEVOLUCIONES ? "P.ProPrecio" : ConvertirMoneda ? QueryConvertirPrecio : (preciolistasinitbis ? (useClientesPrecios ? "IFNULL(lpc.LipPrecio / ((p.ProItbis / 100.00) + 1.00), l.LipPrecio / ((p.ProItbis / 100.00) + 1.00))" : "l.LipPrecio") : (useClientesPrecios ? "IFNULL(lpc.LipPrecio, l.LipPrecio)" : "l.LipPrecio"))) + ", ifnull(t.Precio, 0.0)) as PrecioMoneda, " +
                 "ifnull(t.Cantidad, 0) as Cantidad, ifnull(CantidadDetalle, 0) as CantidadDetalle, " + (Arguments.Values.CurrentModule == Modules.COMPRAS || Arguments.Values.CurrentModule == Modules.INVFISICO || Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS || Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS || Arguments.Values.CurrentModule == Modules.TRASPASOS || Arguments.Values.CurrentModule == Modules.REQUISICIONINVENTARIO ? "0 as Selectivo" : "ifnull(ProSelectivo, 0) as Selectivo") + ", " + (Arguments.Values.CurrentModule == Modules.COMPRAS || Arguments.Values.CurrentModule == Modules.INVFISICO || Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS || Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS || Arguments.Values.CurrentModule == Modules.TRASPASOS || Arguments.Values.CurrentModule == Modules.REQUISICIONINVENTARIO ? "0 as AdValorem" : advalorem + " as AdValorem") + ", " + useMultiplo + " " +
                 (Arguments.Values.CurrentModule == Modules.COMPRAS || itbisCero ? "0" : "ifnull(ProItbis, 0)") + " as Itbis, " + unmSelect + " ifnull(p.ProDatos2, '') as ProDatos2, ifnull(P.ProDescripcion1, '') as ProDescripcion1, ifnull(P.ProDescripcion2, '') as ProDescripcion2, " +
                 "ifnull(P.ProDescripcion3, '') as ProDescripcion3, ifnull(P.ProDatos1, '') as ProDatos1, t.PrecioTemp as PrecioTemp, ifnull(p.ProUnidades, 0) as ProUnidades, " + ((Arguments.Values.CurrentModule == Modules.VENTAS && !parMultiAlmacenes) || Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS || Arguments.Values.CurrentModule == Modules.TRASPASOS || Arguments.Values.CurrentModule == Modules.REQUISICIONINVENTARIO || Arguments.Values.CurrentModule == Modules.PROMOCIONES || Arguments.Values.CurrentModule == Modules.ENTREGASMERCANCIA || Arguments.Values.CurrentModule == Modules.CAMBIOSMERCANCIA ? "g.InvCantidadDetalle as InvCantidadDetalle, g.InvCantidad" : "ifnull(" + (myParametro.GetParPedidosNoUsarInventariosAlmacenes() ? "P.ProCantidad" : "I.InvCantidad") + ", 0)") + " as InvCantidad, Accion, " +
                 " ifnull(P.ProColor,'') as ProColor, ifnull(P.ProPaisOrigen,'') as ProPaisOrigen, ifnull(P.ProAnio,'') as ProAnio, ifnull(P.ProMedida,'') as ProMedida, " +
                  (cantidadSugerida ? "ps.PedCantidad as PedCantidad, ps.PedCantidadDetalle as PedCantidadDetalle, " : "") +
                  (cantidadMinima ? " cm.CliCanTidadMinima as CliCanTidadMinima, " : "") +
                 " FechaVencimiento, Documento, CantidadOferta, t.ProIDOferta as ProIDOferta, P.ProIndicadorDetalle as IndicadorDetalle , " + (myParametro.GetParDevolucionCondicion() && Arguments.Values.CurrentModule == Modules.DEVOLUCIONES ? "t.DevCondicion, u.Descripcion as DevDescripcion, " : "") + " MotIdDevolucion, t.Lote as Lote " + ((parOfertaColor || showDescuentoIndicador || IsDescuentoDevoluciones) && !myParametro.GetParPedidosOfertasyDescuentosManuales() && myParametro.GetParPedidosDescuentoManualGeneral() <= 0.0 ? ", case when o.ProID is not null and ifnull(o.TieneOferta, 0) = 1 then 1 else 0 end as IndicadorOferta, case when o.ProID is not null and ifnull(o.TieneDescuento, 0) = 1 " + (IsDescuentoDevoluciones ? " and o.titId = 2 " : " ") + " then 1 else 0 end as IndicadorDescuento " : " ");
            }
            else
            {
                if (noShowProdInTemp)
                {
                    select = "select distinct " + selectPrecioCaja + " p.ProImg as ProImg, p.ProReferencia as ProReferencia, " + ((int)Arguments.Values.CurrentModule).ToString() + " as TitID, ifnull(p.ProDatos3, '') as ProDatos3, " + (useListaPrecios ? "l.LipPrecioMinimo as LipPrecioMinimo, " : "") + " 0 as ShowDescuento, " +
                    " " + (Arguments.Values.CurrentModule == Modules.PRODUCTOS ? "0" : "1") + " as ShowCantidad, ifnull(P.ProCodigo, '') as ProCodigo, " + (DS_RepresentantesParametros.GetInstance().GetParPedProAlmBySector() ? "ifnull(a.AlmDescripcion, '') as AlmDescripcion, " : "") + "P.ProPrecio3 as ProPrecio3, " + (parInvArea ? 1 : 0) + " as UseInvArea, ifnull(P.ProCantidadMinVenta,'') as ProCantidadMinVenta,  ifnull(P.ProCantidadMaxVenta,'') as ProCantidadMaxVenta,  " + ColumnDescMax + "  " +
                    "P.ProID as ProID, ifnull(P.ProDescripcion, '') as Descripcion, " + (myParametro.GetParCantInvAlmacenes() ? "ifnull(I.AlmID, 0) as AlmID," : "") + "ifnull(" + (useProPrecios ? args.lipCodigo.Replace("*", "") : Arguments.Values.CurrentModule == Modules.DEVOLUCIONES || Arguments.Values.CurrentModule == Modules.INVFISICO || Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS || Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS || Arguments.Values.CurrentModule == Modules.TRASPASOS || Arguments.Values.CurrentModule == Modules.REQUISICIONINVENTARIO || Arguments.Values.CurrentModule == Modules.PROMOCIONES ? "0" : parNoListaPrecios && !myParametro.GetParProductosUnidosConUnidadConListaPrecios() ? "P.ProPrecio" : ConvertirMoneda ? QueryConvertirPrecio : (preciolistasinitbis ? (useClientesPrecios ? "IFNULL(lpc.LipPrecio / ((p.ProItbis / 100.00) + 1.00), l.LipPrecio / ((p.ProItbis / 100.00) + 1.00))" : "l.LipPrecio") : (useClientesPrecios ? "IFNULL(lpc.LipPrecio , l.LipPrecio)" : "l.LipPrecio"))) + ", 0.0) as Precio, " +
                    "0 as Cantidad, " + (Arguments.Values.CurrentModule == Modules.COMPRAS || Arguments.Values.CurrentModule == Modules.INVFISICO || Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS || Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS || Arguments.Values.CurrentModule == Modules.TRASPASOS || Arguments.Values.CurrentModule == Modules.REQUISICIONINVENTARIO ? "0 as Selectivo" : "ifnull(ProSelectivo, 0) as Selectivo") + ", " + (Arguments.Values.CurrentModule == Modules.COMPRAS || Arguments.Values.CurrentModule == Modules.INVFISICO || Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS || Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS || Arguments.Values.CurrentModule == Modules.TRASPASOS || Arguments.Values.CurrentModule == Modules.REQUISICIONINVENTARIO ? "0 as AdValorem" : advalorem + " as AdValorem") + ", " + useMultiplo + " " +
                    (Arguments.Values.CurrentModule == Modules.COMPRAS || itbisCero ? "0" : "ifnull(ProItbis, 0)") + " as Itbis, " + unmSelect + " ifnull(p.ProDatos2, '') as ProDatos2, ifnull(P.ProDescripcion1, '') as ProDescripcion1, ifnull(P.ProDescripcion2, '') as ProDescripcion2, " +
                    "ifnull(P.ProDescripcion3, '') as ProDescripcion3, ifnull(P.ProDatos1, '') as ProDatos1, ifnull(p.ProUnidades, 0) as ProUnidades, " + ((Arguments.Values.CurrentModule == Modules.VENTAS && !parMultiAlmacenes) || Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS || Arguments.Values.CurrentModule == Modules.TRASPASOS || Arguments.Values.CurrentModule == Modules.REQUISICIONINVENTARIO || Arguments.Values.CurrentModule == Modules.PROMOCIONES || Arguments.Values.CurrentModule == Modules.ENTREGASMERCANCIA || Arguments.Values.CurrentModule == Modules.CAMBIOSMERCANCIA ? "g.InvCantidadDetalle as InvCantidadDetalle, g.InvCantidad" : "ifnull(" + (myParametro.GetParPedidosNoUsarInventariosAlmacenes() ? "P.ProCantidad" : "I.InvCantidad") + ", 0)") + " as InvCantidad, " +
                    " ifnull(P.ProColor,'') as ProColor, ifnull(P.ProPaisOrigen,'') as ProPaisOrigen, ifnull(P.ProAnio,'') as ProAnio, ifnull(P.ProMedida,'') as ProMedida, " +
                    (cantidadSugerida ? "ps.PedCantidad as PedCantidad, ps.PedCantidadDetalle as PedCantidadDetalle, " : "") +
                    (cantidadMinima ? " cm.CliCanTidadMinima as CliCanTidadMinima, " : "") +
                    " P.ProIndicadorDetalle as IndicadorDetalle " + ((parOfertaColor || showDescuentoIndicador || IsDescuentoDevoluciones) && !myParametro.GetParPedidosOfertasyDescuentosManuales() && myParametro.GetParPedidosDescuentoManualGeneral() <= 0.0 ? ", case when o.ProID is not null and ifnull(o.TieneOferta, 0) = 1 then 1 else 0 end as IndicadorOferta, " + (myParametro.GetParDevolucionCondicion() && Arguments.Values.CurrentModule == Modules.DEVOLUCIONES ? "t.DevCondicion," : "") + " case when o.ProID is not null  and ifnull(o.TieneDescuento, 0) = 1 " + (IsDescuentoDevoluciones ? " and o.titId = 2 " : " ") + " then 1 else 0 end as IndicadorDescuento " : " ");
                }
                else
                {
                    select = "select distinct " + selectPrecioCaja + " p.ProImg as ProImg, t.PedFechaEntrega as PedFechaEntrega, p.ProReferencia as ProReferencia, t.CantidadPiezas as CantidadPiezas, ifnull(t.IndicadorPromocion, 0) as IndicadorPromocion, " + ((int)Arguments.Values.CurrentModule).ToString() + " as TitID, ifnull(p.ProDatos3, '') as ProDatos3, t.Descuento, t.DesPorciento as DesPorciento, " + (useListaPrecios ? "l.LipPrecioMinimo as LipPrecioMinimo, " : "") + " t.DesPorcientoManual as DesPorcientoManual, case when ifnull(DesPorcientoManual, 0) > 0 then 1 else 0 end as ShowDescuento, " +
                    "t.rowguid as rowguid, t.IndicadorDocena as IndicadorDocena, " + (Arguments.Values.CurrentModule == Modules.PRODUCTOS ? "0" : "1") + " as ShowCantidad, ifnull(P.ProCodigo, '') as ProCodigo, " + (DS_RepresentantesParametros.GetInstance().GetParPedProAlmBySector() ? "ifnull(a.AlmDescripcion, '') as AlmDescripcion, " : "") + "P.ProPrecio3 as ProPrecio3, t.InvAreaDescr as InvAreaDescr, InvAreaId, " + (parInvArea ? 1 : 0) + " as UseInvArea, ifnull(P.ProCantidadMinVenta,'') as ProCantidadMinVenta,  ifnull(P.ProCantidadMaxVenta,'') as ProCantidadMaxVenta,  " + ColumnDescMax + "  " +
                    "P.ProID as ProID, ifnull(P.ProDescripcion, '') as Descripcion, " + (myParametro.GetParCantInvAlmacenes() ? "ifnull(I.AlmID, 0) as AlmID," : "") + " ifnull(t.Precio, ifnull(" + (useProPrecios ? args.lipCodigo.Replace("*", "") : Arguments.Values.CurrentModule == Modules.INVFISICO || Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS || Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS || Arguments.Values.CurrentModule == Modules.TRASPASOS || Arguments.Values.CurrentModule == Modules.REQUISICIONINVENTARIO || Arguments.Values.CurrentModule == Modules.PROMOCIONES || (Arguments.Values.CurrentModule == Modules.DEVOLUCIONES && !myParametro.GetParDevolucionesConPrecio()) ? "0" : parNoListaPrecios && !myParametro.GetParProductosUnidosConUnidadConListaPrecios() ? "P.ProPrecio" : ConvertirMoneda ? QueryConvertirPrecio : (preciolistasinitbis ? (useClientesPrecios ? "IFNULL(lpc.LipPrecio / ((p.ProItbis / 100.00) + 1.00), l.LipPrecio / ((p.ProItbis / 100.00) + 1.00))" : "l.LipPrecio") : (useClientesPrecios ? "IFNULL(lpc.LipPrecio , l.LipPrecio)" : "l.LipPrecio"))) + ", 0.0)) as Precio, " +
                    " ifnull(" + (useProPrecios ? args.lipCodigo.Replace("*", "") : Arguments.Values.CurrentModule == Modules.DEVOLUCIONES || Arguments.Values.CurrentModule == Modules.INVFISICO || Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS || Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS || Arguments.Values.CurrentModule == Modules.TRASPASOS || Arguments.Values.CurrentModule == Modules.REQUISICIONINVENTARIO || Arguments.Values.CurrentModule == Modules.PROMOCIONES ? "0" : parNoListaPrecios && !myParametro.GetParProductosUnidosConUnidadConListaPrecios() ? "P.ProPrecio" : ConvertirMoneda ? QueryConvertirPrecio : (preciolistasinitbis ? (useClientesPrecios ? "IFNULL(lpc.LipPrecio / ((p.ProItbis / 100.00) + 1.00), l.LipPrecio / ((p.ProItbis / 100.00) + 1.00))" : "l.LipPrecio") : (useClientesPrecios ? "IFNULL(lpc.LipPrecio, l.LipPrecio)" : "l.LipPrecio"))) + ", ifnull(t.Precio, 0.0)) as PrecioMoneda, " +
                    "ifnull(t.Cantidad, 0) as Cantidad, ifnull(CantidadDetalle, 0) as CantidadDetalle, " + (Arguments.Values.CurrentModule == Modules.COMPRAS || Arguments.Values.CurrentModule == Modules.INVFISICO || Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS || Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS || Arguments.Values.CurrentModule == Modules.TRASPASOS || Arguments.Values.CurrentModule == Modules.REQUISICIONINVENTARIO ? "0 as Selectivo" : "ifnull(ProSelectivo, 0) as Selectivo") + ", " + (Arguments.Values.CurrentModule == Modules.COMPRAS || Arguments.Values.CurrentModule == Modules.INVFISICO || Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS || Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS || Arguments.Values.CurrentModule == Modules.TRASPASOS || Arguments.Values.CurrentModule == Modules.REQUISICIONINVENTARIO ? "0 as AdValorem" : advalorem + " as AdValorem") + ", " + useMultiplo + " " +
                    (Arguments.Values.CurrentModule == Modules.COMPRAS || itbisCero ? "0" : "ifnull(ProItbis, 0)") + " as Itbis, " + unmSelect + " ifnull(p.ProDatos2, '') as ProDatos2, ifnull(P.ProDescripcion1, '') as ProDescripcion1, ifnull(P.ProDescripcion2, '') as ProDescripcion2, " +
                    "ifnull(P.ProDescripcion3, '') as ProDescripcion3, ifnull(P.ProDatos1, '') as ProDatos1, t.PrecioTemp as PrecioTemp, ifnull(p.ProUnidades, 0) as ProUnidades, " + ((Arguments.Values.CurrentModule == Modules.VENTAS && !parMultiAlmacenes) || Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS || Arguments.Values.CurrentModule == Modules.TRASPASOS || Arguments.Values.CurrentModule == Modules.REQUISICIONINVENTARIO || Arguments.Values.CurrentModule == Modules.PROMOCIONES || Arguments.Values.CurrentModule == Modules.ENTREGASMERCANCIA || Arguments.Values.CurrentModule == Modules.CAMBIOSMERCANCIA ? "g.InvCantidadDetalle as InvCantidadDetalle, g.InvCantidad" : "ifnull(" + (myParametro.GetParPedidosNoUsarInventariosAlmacenes() ? "P.ProCantidad" : "I.InvCantidad") + ", 0)") + " as InvCantidad, Accion, " +
                    " ifnull(P.ProColor,'') as ProColor, ifnull(P.ProPaisOrigen,'') as ProPaisOrigen, ifnull(P.ProAnio,'') as ProAnio, ifnull(P.ProMedida,'') as ProMedida, " +
                    (cantidadSugerida ? "ps.PedCantidad as PedCantidad, ps.PedCantidadDetalle as PedCantidadDetalle, " : "") +
                    (cantidadMinima ? " cm.CliCanTidadMinima as CliCanTidadMinima, " : "") +
                    (myParametro.GetParNoMostrarProductos() ? " 0 as VerPreciosProductos, " : " 1 as VerPreciosProductos, ") +
                    " FechaVencimiento, Documento, CantidadOferta, t.ProIDOferta as ProIDOferta, P.ProIndicadorDetalle as IndicadorDetalle , MotIdDevolucion, " + (myParametro.GetParDevolucionCondicion() && Arguments.Values.CurrentModule == Modules.DEVOLUCIONES ? "t.DevCondicion," : "") + " t.Lote as Lote " + (((parOfertaColor || showDescuentoIndicador || IsDescuentoDevoluciones)) && !myParametro.GetParPedidosOfertasyDescuentosManuales() ? ", case when o.ProID is not null and ifnull(o.TieneOferta, 0) = 1 then 1 else 0 end as IndicadorOferta, case when o.ProID is not null  and ifnull(o.TieneDescuento, 0) = 1 " + (IsDescuentoDevoluciones ? " and o.titId = 2 " : " ") + " then " + (myParametro.GetParPedidosDescuentoManualGeneral() <= 0.0 ? "1" : "0") + " else 0 end as IndicadorDescuento " : " ");
                }
            }
            var whereProductosInactivos = "";
            string joinDevolucion = "";
            string joinOfertas = "";

            if (myParametro.GetParEntregasMercanciasPorFactura() || myParametro.GetParDevolucionesProductosFacturas())
            {
                if (!string.IsNullOrWhiteSpace(args.IdFactura) && args.IdFactura != "-1")
                {
                    if (!myParametro.GetParDevolucionesPermitirCapturaProdAdicionales())
                    {
                        if (myParametro.GetParHistoricoFacturasFromCuentasxCobrar())
                        {
                            if (args.IdFactura.Contains("B"))
                            {
                                whereCondition += " and p.ProID in (select distinct ProID from VentasDetalle vd1 " +
                                    "inner join Ventas v1 on v1.Vensecuencia = vd1.Vensecuencia and v1.RepCodigo= vd1.RepCodigo " +
                                    "where v1.VenNCF = '" + args.IdFactura + "' and ltrim(rtrim(v1.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "')";
                            }
                            else
                            {
                                whereCondition += " and p.ProID in (select distinct ProID from VentasDetalleConfirmados vd1 " +
                                    "inner join VentasConfirmados v1 on v1.Vensecuencia = vd1.Vensecuencia and v1.RepCodigo= vd1.RepCodigo " +
                                    "where v1.VenNumeroErp = '" + args.IdFactura + "' and ltrim(rtrim(v1.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "')";
                            }
                        }
                        else
                        {
                            whereCondition += " and p.ProID in (select distinct ProID from HistoricoFacturasDetalle where idReferencia = " + args.IdFactura + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "')";
                        }
                    }


                }
            }

            if (Arguments.Values.CurrentModule == Modules.PRODUCTOS && myParametro.GetParDescuentosProductosMostrarPreview())
            {
                joinOfertas = $" {(args.filter.FilDescripcion.Contains("Ofertas") ? "inner" : "left")} join ProductosValidosOfertas o on p.ProID = o.ProID and o.CliID = -1 ";
            }
            else if (!myParametro.GetParDescuentosEnDevoluciones() && Arguments.Values.CurrentModule == Modules.DEVOLUCIONES)
            {
                select += ", m.MotDescripcion as MotDescripcion ";
                whereProductosInactivos = " and ifnull(p.ProDatos3, '') not like '%D%' ";
                joinDevolucion = " " + (Arguments.Values.CurrentModule == Modules.DEVOLUCIONES && myParametro.GetParDevolucionCondicion() ? "left join UsosMultiples u on u.CodigoGrupo = 'DEVCONDICION' and u.CodigoUso=t.DevCondicion " : "") + " left join MotivosDevolucion m on m.MotID = t.MotIdDevolucion ";

                if (!string.IsNullOrWhiteSpace(args.IdFactura) && args.IdFactura != "-1")
                {
                    if (!myParametro.GetParDevolucionesPermitirCapturaProdAdicionales())
                    {
                        if (myParametro.GetParHistoricoFacturasFromCuentasxCobrar())
                        {
                            if (args.IdFactura.Contains("B"))
                            {
                                whereCondition += " and p.ProID in (select distinct ProID from VentasDetalle vd1 " +
                                    "inner join Ventas v1 on v1.Vensecuencia = vd1.Vensecuencia and v1.RepCodigo= vd1.RepCodigo " +
                                    "where v1.VenNCF = '" + args.IdFactura + "' and ltrim(rtrim(v1.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "')";
                            }
                            else
                            {
                                whereCondition += " and p.ProID in (select distinct ProID from VentasDetalleConfirmados vd1 " +
                                    "inner join VentasConfirmados v1 on v1.Vensecuencia = vd1.Vensecuencia and v1.RepCodigo= vd1.RepCodigo " +
                                    "where v1.VenNumeroErp = '" + args.IdFactura + "' and ltrim(rtrim(v1.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "')";
                            }
                        }
                        else
                        {
                            whereCondition += " and p.ProID in (select distinct ProID from HistoricoFacturasDetalle where idReferencia = " + args.IdFactura + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "')";
                        }
                    }
                }
            }
            else if (myParametro.GetParDescuentosEnDevoluciones() || Arguments.Values.CurrentModule == Modules.PRODUCTOS || Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.VENTAS || Arguments.Values.CurrentModule == Modules.COTIZACIONES)
            {
                if (!myParametro.GetParPedidosIgnorarBloqueoProductos())
                {
                    whereProductosInactivos = " and ifnull(p.ProDatos3, '') not like '%" + (Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.PRODUCTOS ? "P" : (Arguments.Values.CurrentModule == Modules.COTIZACIONES ? "G" : "V")) + "%' ";
                }

                if (args.filter != null)
                {
                    joinOfertas = $" {(Arguments.Values.CurrentModule != Modules.PRODUCTOS && args.filter.FilDescripcion.Contains("Ofertas") ? "inner" : "left")} join ProductosValidosOfertas o on p.ProID = o.ProID and o.CliID = " + (Arguments.Values.CurrentClient == null ? -1 : Arguments.Values.CurrentClient.CliID) + "  " + (!parNoListaPrecios ? " and (o.UnmCodigo = L.UnmCodigo " + (Arguments.Values.CurrentModule == Modules.PRODUCTOS || myParametro.GetParDescuentosProductosMostrarPreview() ? " or ifnull(o.UnmCodigo,'') = '' " : "") + ")" : "") + " ";
                    if (myParametro.GetParPedidosVisualizarXClientesDetalles() || myParametro.GetParProductoNoVendido() == 1 && args.filter.FilDescripcion.ToUpper().Contains("no vendidos".ToUpper()))
                    {
                        joinOfertas += " inner join GrupoProductosDetalle gp on gp.ProID = p.ProID "
                                + "inner join ClientesDetalle cd on cd.GrpCodigo = gp.GrpCodigo and cd.CliID = " + (Arguments.Values.CurrentClient == null ? -1 : Arguments.Values.CurrentClient.CliID) + " and cd.SecCodigo = " + Arguments.Values.CurrentSector.SecCodigo + " ";
                    }

                }
                else
                {
                    joinOfertas = $" left join ProductosValidosOfertas o on p.ProID = o.ProID and o.CliID = " + (Arguments.Values.CurrentClient == null ? -1 : Arguments.Values.CurrentClient.CliID) + "  " + (!parNoListaPrecios ? " and (o.UnmCodigo = L.UnmCodigo " + (Arguments.Values.CurrentModule == Modules.PRODUCTOS || myParametro.GetParDescuentosProductosMostrarPreview() ? " or ifnull(o.UnmCodigo,'') = '' " : "") + ")" : "") + " ";
                    if (myParametro.GetParPedidosVisualizarXClientesDetalles())
                    {
                        joinOfertas += " inner join GrupoProductosDetalle gp on gp.ProID = p.ProID "
                                + "inner join ClientesDetalle cd on cd.GrpCodigo = gp.GrpCodigo  and cd.CliID = " + (Arguments.Values.CurrentClient == null ? -1 : Arguments.Values.CurrentClient.CliID) + " and cd.SecCodigo = " + Arguments.Values.CurrentSector.SecCodigo + " ";
                    }
                }




                if (Arguments.Values.CurrentModule == Modules.VENTAS && !parMultiAlmacenes)
                {
                    joinOfertas += " " + (myParametro.GetProductosEnInventario() ? "inner" : "left") + " join Inventarios g on g.ProID = p.ProID and (g.invCantidad > 0 or ifnull(g.invCantidadDetalle, 0) > 0) " +
                        "and ltrim(rtrim(g.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' ";
                }
                else if (Arguments.Values.CurrentModule == Modules.VENTAS && parMultiAlmacenes)
                {
                    var almId = myParametro.GetParAlmacenIdParaDevolucion();

                    if (myParametro.GetParAlmacenVentaRanchera() > 0)
                    {
                        almId = myParametro.GetParAlmacenVentaRanchera();
                    }

                    if (args.useAlmacenDespacho)
                    {
                        almId = myParametro.GetParAlmacenIdParaDespacho();
                    }

                    joinOfertas += " " + (myParametro.GetProductosEnInventario() ? "inner" : "left") + " join InventariosAlmacenesRepresentantes g on g.ProID = p.ProID and 1=1  and trim(g.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and g.AlmID = " + almId.ToString() + " and (g.invCantidad > 0 or g.invCantidadDetalle > 0) ";

                }

                if (myParametro.GetParPedidosOfertasyDescuentosManuales())
                {
                    joinOfertas = "";
                }
            }
            else if (Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS && myParametro.GetParUsarMultiAlmacenes() && myParametro.GetProductosEnInventarioEnConteoFisico())
            {
                joinOfertas += " " + (myParametro.GetProductosEnInventarioEnConteoFisico() ? "inner" : "left") + " join InventariosAlmacenesRepresentantes g on g.ProID = p.ProID and 1=1  and trim(g.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' ";
            }
            else if (Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS
                || Arguments.Values.CurrentModule == Modules.TRASPASOS
                || Arguments.Values.CurrentModule == Modules.REQUISICIONINVENTARIO
                || Arguments.Values.CurrentModule == Modules.ENTREGASMERCANCIA
                || Arguments.Values.CurrentModule == Modules.PROMOCIONES
                || Arguments.Values.CurrentModule == Modules.CAMBIOSMERCANCIA)
            {
                var condition = " left ";

                if (Arguments.Values.CurrentModule == Modules.ENTREGASMERCANCIA || Arguments.Values.CurrentModule == Modules.PROMOCIONES)
                {
                    condition = " inner ";
                }

                joinOfertas += " " + condition + " join Inventarios g on g.ProID = p.ProID and trim(g.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' ";
            }
            else if (myParametro.GetParCotizacionesOfertasyDescuentos())
            {
                if (Arguments.Values.CurrentModule == Modules.COTIZACIONES)
                {
                    whereProductosInactivos = " and ifnull(p.ProDatos3, '') not like '%" + (Arguments.Values.CurrentModule == Modules.PEDIDOS ? "P" : (Arguments.Values.CurrentModule == Modules.COTIZACIONES ? "G" : "V")) + "%' ";

                    joinOfertas = $" {(args.filter.FilDescripcion.Contains("Ofertas") ? "inner" : "left")} join ProductosValidosOfertas o on p.ProID = o.ProID and o.CliID = " + Arguments.Values.CurrentClient.CliID + "  " + (!parNoListaPrecios ? " and (o.UnmCodigo = L.UnmCodigo " + (Arguments.Values.CurrentModule == Modules.PRODUCTOS ? " or ifnull(o.UnmCodigo,'') = '' " : "") + ")" : "") + " ";


                    if (parMultiAlmacenes)
                    {
                        var almId = myParametro.GetParAlmacenIdParaDevolucion();

                        if (myParametro.GetParAlmacenVentaRanchera() > 0)
                        {
                            almId = myParametro.GetParAlmacenVentaRanchera();
                        }

                        if (args.useAlmacenDespacho)
                        {
                            almId = myParametro.GetParAlmacenIdParaDespacho();
                        }

                        joinOfertas += " " + (myParametro.GetProductosEnInventario() ? "inner" : "left") + " join InventariosAlmacenesRepresentantes g on g.ProID = p.ProID and case when ifnull(p.ProDatos3, '') like '%L%' then ifnull(g.InvLote, '') <> '' else 1=1 end and trim(g.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and g.AlmID = " + almId.ToString() + " ";
                    }
                    else if (Arguments.Values.CurrentModule == Modules.VENTAS)
                    {
                        bool productosinv = myParametro.GetProductosEnInventario();

                        joinOfertas += " " + (productosinv ? "inner" : "left") + " join Inventarios g on g.ProID = p.ProID and ltrim(rtrim(g.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' ";
                        joinOfertas += " " + (productosinv ? "and (g.invCantidad > 0 or g.invCantidadDetalle >0) " : " ");
                    }

                    if (myParametro.GetParPedidosOfertasyDescuentosManuales())
                    {
                        joinOfertas = "";
                    }
                }

            }

            string whereMonCodigo = "";

            if (!string.IsNullOrWhiteSpace(args.MonCodigo) && !ConvertirMoneda)
            {
                whereMonCodigo = " and (upper(l.MonCodigo) = '" + args.MonCodigo.Trim().ToUpper() + "' or ifnull(l.MonCodigo, '') = '') ";
            }

            if (args.FiltrarProductosPorSector)
            {
                whereCondition += " and (p.SecCodigo = '" + Arguments.Values.CurrentSector?.SecCodigo + "' or p.SecCodigo is NULL or p.SecCodigo = '') ";
            }

            if ((myParametro.GetParProductosFromClientesProductosVendidos() && Arguments.Values.CurrentModule == Modules.INVFISICO) ||
                (myParametro.GetParColocacionProductosFromClientesProductosVendidos() && Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS))
            {
                whereCondition += "  And  P.Proid in (select Proid from Clientesproductosvendidos where CLiid = " + Arguments.Values.CurrentClient.CliID + ")";
            }

            string query;

            if (Arguments.Values.CurrentModule == Modules.INVFISICO && !noShowProdInTemp)
            {
                select += ", t.CantidadFacing as CantidadFacing ";
            }

            if (Arguments.Values.CurrentModule == Modules.CAMBIOSMERCANCIA)
            {
                select += ", t.TipoCambio as TipoCambio ";
            }

            if (Arguments.Values.CurrentModule == Modules.PEDIDOS)
            {
                select += ", case when ifnull(p.ProAtributo1,'') != '' then 1 else 0 end as UseAttribute1, case when ifnull(p.ProAtributo2, '') != '' then 1 else 0 end as UseAttribute2, t.ProAtributo1Desc as ProAtributo1Desc, t.ProAtributo2Desc as ProAtributo2Desc, t.ProAtributo1 as ProAtributo1, t.ProAtributo2 as ProAtributo2 ";
            }

            if (Arguments.Values.CurrentModule == Modules.DEVOLUCIONES && myParametro.GetParDevolucionesConListaPrecios())
            {
                query = select + " ,ifnull(p.ProPeso,0) as ProPeso,ifnull(p.LinId,0) as Linea,t.Posicion, l.LipPrecioSugerido " + ((Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.VENTAS || Arguments.Values.CurrentModule == Modules.PRODUCTOS) && myParametro.GetParDescuentosProductosMostrarPreview() ? ", case when o.ProID is not null and ifnull(o.TieneDescuentoEscala, 0) = 1 then 1 else 0 end as TieneDescuentoEscala, o.PorcientoDescuento as DesPorciento, case when ifnull(o.PorcientoDescuento, 0.0) > 0.0 then 1 else 0 end as ShowDescuentoPreview, ((o.PorcientoDescuento * " + (ConvertirMoneda ? QueryConvertirPrecio : (Arguments.Values.CurrentModule == Modules.PRODUCTOS ? "IFNULL(l.LipPrecio, 0.0)" : "IFNULL(lpc.LipPrecio , l.LipPrecio)")) + ") / 100.0) as Descuento " : "") + " from Productos P " +
                  (Arguments.Values.CurrentModule == Modules.INVFISICO || Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS || useProPrecios ? "left" : "inner") + " join ListaPrecios l on " + (!string.IsNullOrWhiteSpace(unidadesMedidasValidas) ? "ifnull(lower(l.UnmCodigo), '') in (" + unidadesMedidasValidas + ") and " : "") + " l.ProID = P.ProID and l.LipCodigo = '" + args.lipCodigo.Replace("*", "") + "' " + whereMonCodigo +
                  (useClientesPrecios ? "left join ListaPreciosClientes lpc on " + (!string.IsNullOrWhiteSpace(unidadesMedidasValidas) ? " ifnull(lower(lpc.UnmCodigo), '') in (" + unidadesMedidasValidas + ") and " : "") + " lpc.ProID = P.ProID and lpc.CliID = " + Arguments.Values.CurrentClient.CliID + " " +
                  (!string.IsNullOrWhiteSpace(args.MonCodigo) ? " and (lpc.MonCodigo = '" + args.MonCodigo.Trim() + "' OR ifnull(lpc.MonCodigo, '') = '') " : " ") : " ") +
                  (ConvertirMoneda ? JoinConversion : "") +
                   (cantidadSugerida ? "left join PedidosSugeridos ps on ps.ProID = p.ProID " : " ") +
                   (cantidadMinima ? " left join ClientesInventariosMinimos cm on cm.ProID = p.ProID " + (Arguments.Values.CurrentClient != null ? " and cm.cliid = " + (Arguments.Values.CurrentClient.CliID) + "" : "") + "" : "") +
                  " left join InventariosAlmacenes I on I.ProID = P.ProID AND I.AlmID = '" + (myParametro.GetParAlmacenDefault()) + "' " +
                  (DS_RepresentantesParametros.GetInstance().GetParPedProAlmBySector() ? " left join Almacenes a on a.AlmID = I.AlmID " : "") +
                  "left join ProductosTemp t on  t.ProID = P.ProID and ifnull(lpc.UnmCodigo, l.UnmCodigo) = t.UnmCodigo" + (parPreciosPorUnmCodigo ? (useClientesPrecios ? " and ifnull(lpc.UnmCodigo, l.UnmCodigo) = t.UnmCodigo " : " and t.UnmCodigo = l.UnmCodigo ") : "") + " and t.TitID = " + ((int)Arguments.Values.CurrentModule).ToString() + " and ifnull(t.IndicadorOferta, 0) = 0 " + (args.NotUseTemp ? " and 1=2 " : "") + joinDevolucion + joinOfertas +
                  (!string.IsNullOrEmpty(joinTinIDAdValorem) ? joinTinIDAdValorem : "") +
                  "where 1=1 And  " + (parNoListaPrecios && Arguments.Values.CurrentModule != Modules.DEVOLUCIONES ? "P.ProPrecio" : ConvertirMoneda ? QueryConvertirPrecio : (useClientesPrecios ? "IFNULL(lpc.LipPrecio, l.LipPrecio)" : "l.LipPrecio")) + " >0 " + " " + whereProductosInactivos + whereCondition;
            }
            else if ((parNoListaPrecios || Arguments.Values.CurrentModule == Modules.DEVOLUCIONES || Arguments.Values.CurrentModule == Modules.INVFISICO || Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS || Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS || Arguments.Values.CurrentModule == Modules.TRASPASOS || Arguments.Values.CurrentModule == Modules.PROMOCIONES) && myParametro.GetParProductosUnidosConUnidadConListaPrecios())
            {
                query = select + " ,ifnull(p.ProPeso,0) as ProPeso,ifnull(p.LinId,0) as Linea from Productos P " +
                    "inner join ListaPrecios l on ifnull(lower(l.UnmCodigo), '') in (lower(P.UnmCodigo)) and l.Proid=P.Proid and l.LipCodigo = '" + args.lipCodigo.Replace("*", "") + "' " + whereMonCodigo +
                    (useClientesPrecios ? "left join ListaPreciosClientes lpc on  ifnull(lower(lpc.UnmCodigo), '') in (lower(P.UnmCodigo)) and  lpc.CliID = " + Arguments.Values.CurrentClient.CliID + " " +
                    (!string.IsNullOrWhiteSpace(args.MonCodigo) ? " and (lpc.MonCodigo = '" + args.MonCodigo.Trim() + "' OR ifnull(lpc.MonCodigo, '') = '') " : " ") : " ") +
                    (ConvertirMoneda ? JoinConversion : "") +
                    "left join InventariosAlmacenes I on I.ProID = P.ProID AND I.AlmID = '" + (myParametro.GetParAlmacenDefault()) + "' " +
                    (DS_RepresentantesParametros.GetInstance().GetParPedProAlmBySector() ? "left join Almacenes a on a.AlmID = I.AlmID " : "") +
                    (cantidadMinima ? " left join ClientesInventariosMinimos cm on cm.ProID = p.ProID " + (Arguments.Values.CurrentClient != null ? " and cm.cliid = " + (Arguments.Values.CurrentClient.CliID) + "" : "") + "" : "") +
                    " left join ProductosTemp t on t.ProID = P.ProID and t.TitID = " + ((int)Arguments.Values.CurrentModule).ToString() + " and ifnull(t.IndicadorOferta, 0) = 0 " + (args.NotUseTemp ? " and 1=2 " : "") + joinDevolucion + joinOfertas +
                    (!string.IsNullOrEmpty(joinTinIDAdValorem) ? joinTinIDAdValorem : "") +
                    "where 1=1 "
                    + whereProductosInactivos + whereCondition;
            }
            else if ((parNoListaPrecios || Arguments.Values.CurrentModule == Modules.DEVOLUCIONES || Arguments.Values.CurrentModule == Modules.INVFISICO || Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS || Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS || Arguments.Values.CurrentModule == Modules.TRASPASOS || Arguments.Values.CurrentModule == Modules.REQUISICIONINVENTARIO || Arguments.Values.CurrentModule == Modules.PROMOCIONES) && !myParametro.GetParProductosUnidosConUnidadConListaPrecios())
            {
                query = select + ",ifnull(p.ProPeso,0) as ProPeso,ifnull(p.LinId,0) as Linea, l.LipPrecioSugerido as LipPrecioSugerido " + (!noShowProdInTemp ? ",CantidadAlm, UnidadAlm, CanTidadGond, UnidadGond, CanTidadTramo " : " ") + " from Productos P " +
                     " " + (myParametro.GetParMostrarProductosEnInventarioParaInventarios() ? " inner join InventariosAlmacenes I on I.ProID = P.ProID AND I.AlmID = '" + (myParametro.GetParAlmacenDefault()) + "' AND (I.invCantidad > 0 or I.InvCantidadDetalle > 0) " : " left join InventariosAlmacenes I on I.ProID = P.ProID AND I.AlmID = '" + (myParametro.GetParAlmacenDefault()) + "'") + " " +
                     (DS_RepresentantesParametros.GetInstance().GetParPedProAlmBySector() ? "left join Almacenes a on a.AlmID = I.AlmID " : "") +
                     (cantidadMinima ? " left join ClientesInventariosMinimos cm on cm.ProID = p.ProID " + (Arguments.Values.CurrentClient != null ? " and cm.cliid = " + (Arguments.Values.CurrentClient.CliID) + "" : "") + "" : "") +
                     " " + (!noShowProdInTemp ? "left join ProductosTemp t on t.ProID = P.ProID and t.TitID = " + ((int)Arguments.Values.CurrentModule).ToString() + " and ifnull(t.IndicadorOferta, 0) = 0 " + (args.NotUseTemp ? " and 1=2 " : "") + "" : " ") + "" + joinDevolucion + //joinOfertas +
                     " left join ListaPrecios l on l.proid = p.proid " + (!string.IsNullOrWhiteSpace(args.lipCodigo) ? " and l.LipCodigo = '" + args.lipCodigo.Replace("*", "") + "'" : "") + " " +
                     (!string.IsNullOrEmpty(joinTinIDAdValorem) ? joinTinIDAdValorem : "") + joinOfertas +
                     "where 1=1 " + (!noShowProdInTemp ? "" : "and p.proid not in (select proid from ProductosTemp)") + "" + whereProductosInactivos + whereCondition;
            }
            else
            {
                if (args.filter != null)
                {
                    query = select + ",ifnull(p.ProPeso,0) as ProPeso,ifnull(p.LinId,0) as Linea,t.Posicion, l.LipPrecioSugerido " + ((Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.VENTAS || Arguments.Values.CurrentModule == Modules.PRODUCTOS) && myParametro.GetParDescuentosProductosMostrarPreview() ? ", case when o.ProID is not null and ifnull(o.TieneDescuentoEscala, 0) = 1 then 1 else 0 end as TieneDescuentoEscala, o.PorcientoDescuento as DesPorciento, case when ifnull(o.PorcientoDescuento, 0.0) > 0.0 then 1 else 0 end as ShowDescuentoPreview, ((o.PorcientoDescuento * " + (ConvertirMoneda ? QueryConvertirPrecio : (Arguments.Values.CurrentModule == Modules.PRODUCTOS ? "IFNULL(l.LipPrecio, 0.0)" : "IFNULL(lpc.LipPrecio , l.LipPrecio)")) + ") / 100.0) as Descuento " : "") + " from Productos P " +
                     (Arguments.Values.CurrentModule == Modules.DEVOLUCIONES || Arguments.Values.CurrentModule == Modules.INVFISICO || Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS || useProPrecios ? "left" : "inner") + " join ListaPrecios l on " + (!string.IsNullOrWhiteSpace(unidadesMedidasValidas) ? "ifnull(lower(l.UnmCodigo), '') in (" + unidadesMedidasValidas + ") and " : "") + " l.ProID = P.ProID and l.LipCodigo = '" + args.lipCodigo.Replace("*", "") + "' " + whereMonCodigo +
                     (useClientesPrecios ? "left join ListaPreciosClientes lpc on " + (!string.IsNullOrWhiteSpace(unidadesMedidasValidas) ? " ifnull(lower(lpc.UnmCodigo), '') in (" + unidadesMedidasValidas + ") and " : "") + " lpc.ProID = P.ProID and lpc.CliID = " + Arguments.Values.CurrentClient.CliID + " " +
                     (!string.IsNullOrWhiteSpace(args.MonCodigo) ? " and (lpc.MonCodigo = '" + args.MonCodigo.Trim() + "' OR ifnull(lpc.MonCodigo, '') = '') " : " ") : " ") +
                     (ConvertirMoneda ? JoinConversion : "") +
                      (cantidadSugerida ? "left join PedidosSugeridos ps on ps.ProID = p.ProID " : " ") +
                      (cantidadMinima ? " left join ClientesInventariosMinimos cm on cm.ProID = p.ProID " + (Arguments.Values.CurrentClient != null ? " and cm.cliid = " + (Arguments.Values.CurrentClient.CliID) + "" : "") + "" : "") +
                     " left join InventariosAlmacenes I on I.ProID = P.ProID AND I.AlmID = '" + (myParametro.GetParAlmacenDefault()) + "' " +
                     (DS_RepresentantesParametros.GetInstance().GetParPedProAlmBySector() ? " left join Almacenes a on a.AlmID = I.AlmID " : "") +
                     "left join ProductosTemp t on " + (DS_RepresentantesParametros.GetInstance().GetParPedidosProductosUnidades() ? " t.UnmCodigo= '" + args.ProUndMedidas.ToString() + "' and " : "") + "t.ProID = P.ProID " + (parPreciosPorUnmCodigo || !useProPrecios ? (useClientesPrecios && useProPrecios ? " and ifnull(lpc.UnmCodigo, l.UnmCodigo) = t.UnmCodigo " : " and t.UnmCodigo = l.UnmCodigo ") : "") + " and t.TitID = " + ((int)Arguments.Values.CurrentModule).ToString() + " and ifnull(t.IndicadorOferta, 0) = 0 " + (args.NotUseTemp ? " and 1=2 " : "") + joinDevolucion + joinOfertas +
                     (!string.IsNullOrEmpty(joinTinIDAdValorem) ? joinTinIDAdValorem : "") +
                     "where 1=1 " +
                     "" + (args.filter.FilDescripcion.ToUpper().Contains("no vendidos".ToUpper()) ? (myParametro.GetParProductoNoVendido() > 1 ? " and p.proid not in (select proid from ClientesProductosVendidos c where c.CliID = " + Arguments.Values.CurrentClient.CliID + ") " : $"and cd.cliid = {Arguments.Values.CurrentClient.CliID} and cd.RepCodigo = {Arguments.CurrentUser.RepCodigo}  and p.ProDatos3 not like '%P%' and p.ProID not in (select h.ProID  from  ClientesProductosVendidos  h where h.ProID = p.ProID  and h.CliID = {Arguments.Values.CurrentClient.CliID})  { (myParametro.GetParSectores() >= 2 && Arguments.Values.CurrentSector != null ? $" and c.SecCodigo = '{Arguments.Values.CurrentSector.SecCodigo}' " : "") }") : "") + ""
                     + (args.precioMayorQueCero ? " and " + (useProPrecios ? args.lipCodigo.Replace("*", "") : parNoListaPrecios ? "P.ProPrecio" : ConvertirMoneda ? QueryConvertirPrecio : (useClientesPrecios && !useProPrecios ? "IFNULL(lpc.LipPrecio, l.LipPrecio)" : "l.LipPrecio")) + " >0 " : " ") + " " + whereProductosInactivos + whereCondition;
                }
                else
                {
                    query = select + ",ifnull(p.ProPeso,0) as ProPeso,ifnull(p.LinId,0) as Linea,t.Posicion, l.LipPrecioSugerido " + ((Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.VENTAS || Arguments.Values.CurrentModule == Modules.PRODUCTOS) && myParametro.GetParDescuentosProductosMostrarPreview() ? ", case when o.ProID is not null and ifnull(o.TieneDescuentoEscala, 0) = 1 then 1 else 0 end as TieneDescuentoEscala, o.PorcientoDescuento as DesPorciento, case when ifnull(o.PorcientoDescuento, 0.0) > 0.0 then 1 else 0 end as ShowDescuentoPreview, ((o.PorcientoDescuento * " + (ConvertirMoneda ? QueryConvertirPrecio : (Arguments.Values.CurrentModule == Modules.PRODUCTOS ? "IFNULL(l.LipPrecio, 0.0)" : "IFNULL(lpc.LipPrecio , l.LipPrecio)")) + ") / 100.0) as Descuento " : "") + " from Productos P " +
                     (Arguments.Values.CurrentModule == Modules.DEVOLUCIONES || Arguments.Values.CurrentModule == Modules.INVFISICO || Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS || useProPrecios ? "left" : "inner") + " join ListaPrecios l on " + (!string.IsNullOrWhiteSpace(unidadesMedidasValidas) ? "ifnull(lower(l.UnmCodigo), '') in (" + unidadesMedidasValidas + ") and " : "") + " l.ProID = P.ProID and l.LipCodigo = '" + args.lipCodigo.Replace("*", "") + "' " + whereMonCodigo +
                     (useClientesPrecios ? "left join ListaPreciosClientes lpc on " + (!string.IsNullOrWhiteSpace(unidadesMedidasValidas) ? " ifnull(lower(lpc.UnmCodigo), '') in (" + unidadesMedidasValidas + ") and " : "") + " lpc.ProID = P.ProID and lpc.CliID = " + Arguments.Values.CurrentClient.CliID + " " +
                     (!string.IsNullOrWhiteSpace(args.MonCodigo) ? " and (lpc.MonCodigo = '" + args.MonCodigo.Trim() + "' OR ifnull(lpc.MonCodigo, '') = '') " : " ") : " ") +
                     (ConvertirMoneda ? JoinConversion : "") +
                      (cantidadSugerida ? "left join PedidosSugeridos ps on ps.ProID = p.ProID " : " ") +
                      (cantidadMinima ? " left join ClientesInventariosMinimos cm on cm.ProID = p.ProID " + (Arguments.Values.CurrentClient != null ? " and cm.cliid = " + (Arguments.Values.CurrentClient.CliID) + "" : "") + "" : "") +
                     " left join InventariosAlmacenes I on I.ProID = P.ProID AND I.AlmID = '" + (myParametro.GetParAlmacenDefault()) + "' " +
                     (DS_RepresentantesParametros.GetInstance().GetParPedProAlmBySector() ? " left join Almacenes a on a.AlmID = I.AlmID " : "") +
                     "left join ProductosTemp t on " + (DS_RepresentantesParametros.GetInstance().GetParPedidosProductosUnidades() ? " t.UnmCodigo= '" + args.ProUndMedidas.ToString() + "' and " : "") + "t.ProID = P.ProID " + (parPreciosPorUnmCodigo || !useProPrecios ? (useClientesPrecios && useProPrecios ? " and ifnull(lpc.UnmCodigo, l.UnmCodigo) = t.UnmCodigo " : " and t.UnmCodigo = l.UnmCodigo ") : "") + " and t.TitID = " + ((int)Arguments.Values.CurrentModule).ToString() + " and ifnull(t.IndicadorOferta, 0) = 0 " + (args.NotUseTemp ? " and 1=2 " : "") + joinDevolucion + joinOfertas +
                     (!string.IsNullOrEmpty(joinTinIDAdValorem) ? joinTinIDAdValorem : "") +
                     "where 1=1 " +
                     "" + (args.precioMayorQueCero ? " and " + (useProPrecios ? args.lipCodigo.Replace("*", "") : parNoListaPrecios ? "P.ProPrecio" : ConvertirMoneda ? QueryConvertirPrecio : (useClientesPrecios && !useProPrecios ? "IFNULL(lpc.LipPrecio, l.LipPrecio)" : "l.LipPrecio")) + " >0 " : " ") + " " + whereProductosInactivos + whereCondition;

                }

            }

            if (myParametro.GetParInventariosTomarCantidades() > 0 || myParametro.GetParColocacionProductosTomarCantidades() > 0)
            {
                query += " group by p.ProID " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? ", t.UnmCodigo " : "") + " ";
            }

            if (!string.IsNullOrWhiteSpace(args.orderBy) && args.orderBy.ToUpper().Contains("ORDER BY"))
            {
                args.orderBy = args.orderBy.ToUpper().Replace("ORDER BY", "");
            }

            query += (string.IsNullOrWhiteSpace(args.orderBy) ? " order by P.ProDescripcion" : " order by " + args.orderBy);

            var productosTemp = SqliteManager.GetInstance().Query<ProductosTemp>(query, new string[] { });
            if (DS_RepresentantesParametros.GetInstance().GetParMostrarVariosInventariosEnRow())
            {
                var almacenesSD = SqliteManager.GetInstance().Query<Almacenes>("select AlmID, AlmDescripcion, AlmReferencia, ifnull(AlmCaracteristicas, '') as AlmCaracteristicas from Almacenes where AlmDescripcion like '%SD%'");
                var almacenesLV = SqliteManager.GetInstance().Query<Almacenes>("select AlmID, AlmDescripcion, AlmReferencia, ifnull(AlmCaracteristicas, '') as AlmCaracteristicas from Almacenes where AlmDescripcion like '%LV%'");

                foreach (var prod in productosTemp)
                {
                    prod.InvCantidadAlmSD = new DS_inventariosAlmacenes().GetInventarioProductoByAlmacen(prod.ProID, almacenesSD.FirstOrDefault()?.AlmID ?? 0) ?? 0;
                    prod.InvCantidadAlmLV = new DS_inventariosAlmacenes().GetInventarioProductoByAlmacen(prod.ProID, almacenesLV.FirstOrDefault()?.AlmID ?? 0) ?? 0;
                }
            }

            return productosTemp;
        }

        public bool ExistsInTemp(int proId, int titId, bool indicadorPromocion, int posicion = -1)
        {
            return SqliteManager.GetInstance().Query<ProductosTemp>("select ProID from ProductosTemp " +
                "where ProID = ? and TitID = ? and ifnull(IndicadorPromocion, 0) = " + (indicadorPromocion ? "1" : "0") + (posicion != -1 ? " and Posicion = " + posicion.ToString() : ""),
                new string[] { proId.ToString(), titId.ToString() }).Count > 0;
        }

        public bool NothingAddedInTemp(int titId)
        {
            var where = " And (Cantidad > 0 or CantidadDetalle > 0)";

            if (myParametro.GetParSectores() > 0 && Application.Current.Properties.ContainsKey("SecCodigo"))
            {
                where += $"And SecCodigo = '{Application.Current.Properties["SecCodigo"]}'";
            }

            if (myParametro.GetParInventarioFisicoAceptarProductosCantidadCero())
            {
                where = "";
            }

            return SqliteManager.GetInstance().Query<ProductosTemp>("select ProID from ProductosTemp where TitID = " + titId.ToString() + " " + where + " limit 1", new object[] { }).Count == 0;
        }

        public int AddedInTempForGetAlmid(int titId)
        {
            var where = " And (Cantidad > 0 or CantidadDetalle > 0)";

            if (myParametro.GetParInventarioFisicoAceptarProductosCantidadCero())
            {
                where = "";
            }

            return Arguments.Values.AlmId = SqliteManager.GetInstance().Query<ProductosTemp>("select AlmId from ProductosTemp where TitID = " + titId.ToString() + " " + where + " ", new object[] { }).FirstOrDefault().AlmID;
        }

        public List<Productos> GetByProDescripcion(string value)
        {
            return SqliteManager.GetInstance().Query<Productos>("select ProDescripcion, ProID from Productos where ltrim(rtrim(upper(ProDescripcion))) like ? order by ProDescripcion asc limit 10", new string[] { "%" + value.Trim().ToUpper() + "%" });
        }

        public void ClearTemp(int titId)
        {
            SqliteManager.GetInstance().Execute("delete from ProductosTemp where TitID = ? ", new object[] { titId.ToString() });
        }

        public void ClearProductosTempGeneral()
        {
            SqliteManager.GetInstance().Execute("delete from ProductosTemp", new object[] { });
        }

        public void ClearTempOfertas(int titId)
        {
            SqliteManager.GetInstance().Execute("delete from ProductosTemp where TitID = ? and ifnull(IndicadorOferta, 0) = 1", new object[] { titId.ToString() });
        }

        public void DeleteTempByProdIDAndLote(int proid, string lote)
        {
            SqliteManager.GetInstance().Execute("delete from ProductosTemp where ProID = ? " + (string.IsNullOrEmpty(lote) ? "" : $" and Lote= '{lote}' "), new object[] { proid.ToString() });
        }

        public void DeleteTempByRowguid(string rowguid)
        {
            try
            {
                SqliteManager.GetInstance().Execute("delete from ProductosTemp where rowguid = ? ", new string[] { rowguid });
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        public void InsertInTemp(List<ProductosTemp> productos, bool byOferta = false, bool isEntrega = false, bool IsFromCont = false, bool IsMultiEntrega = false, int proID = -1, bool isOfertaUnificada = false)
        {

            var prodtodelete = productos.Select(p => new
            {
                proid = p.ProID,
                prolote = p.Lote
            });


            foreach (var prod in productos)
            {
                if (myParametro.GetParEliminaTempEnConteoConLote() && Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS)
                {
                    DeleteTempByProdIDAndLote(prod.ProID, prod.Lote);
                }

                if (isOfertaUnificada && prod.ProID == proID)
                {
                    continue;
                }

                InsertInTemp(prod, byOferta, isEntrega: isEntrega == true && Arguments.Values.CurrentModule == Modules.VENTAS, IsFromCont, IsMultiEntrega);
            }
        }
        public void InsertInTemp(ProductosTemp producto, bool byOferta = false, bool isEntrega = false, bool IsFromCont = false, bool IsMultiEntrega = false, bool isfromprecios = false)
        {
            bool parInvArea = false;

            if (producto.IndicadorOferta && !string.IsNullOrEmpty(Arguments.Values.ProidForExclurIdOfertas))
            {
                foreach (var str in Arguments.Values.ProidForExclurIdOfertas.Split('|').Where(p => p != " "))
                {
                    if (!string.IsNullOrEmpty(str) && string.Equals(str.Substring(str.LastIndexOf("(") + 1, str.Substring(str.LastIndexOf("(") + 1).Length)
                        , producto.ProID.ToString() + ")"))
                    {
                        return;
                    }
                    else if (!string.IsNullOrEmpty(str.Substring(str.LastIndexOf(")") + 1, str.Substring(str.LastIndexOf(")") + 1).Length)))
                    {
                        producto.Cantidad = int.Parse(str.Substring(str.LastIndexOf(")") + 1, str.Substring(str.LastIndexOf(")") + 1).Length));
                    }
                }
            }

            if (Arguments.Values.CurrentModule == Modules.INVFISICO || Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS)
            {
                var parArea = myParametro.GetParInventarioFisicoArea();

                if (Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS)
                {
                    parArea = myParametro.GetParColocacionProductosCapturarArea();
                }

                if (!string.IsNullOrEmpty(parArea) && parArea.ToUpper().Trim() == "D")
                {
                    parInvArea = true;
                }
            }

            if (string.IsNullOrWhiteSpace(producto.rowguid))
            {
                producto.rowguid = Guid.NewGuid().ToString();
            }

            bool parPromociones = myParametro.GetParPedidosPromociones();

            /*if(producto.UsaLote && !string.IsNullOrWhiteSpace(producto.Lote) 
                && (Arguments.Values.CurrentModule == Modules.VENTAS 
                || Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS
                || Arguments.Values.CurrentModule == Modules.CAMBIOSMERCANCIA)
                && ExistInTempWithOtherLot(producto.rowguid, producto.Lote))
            {
                producto.rowguid = Guid.NewGuid().ToString();
            }*/

            /*if (parPromociones && !ExistsInTemp(producto.ProID, (int)Arguments.Values.CurrentModule, producto.IndicadorPromocion) && RowguidExistsInTemp(producto.rowguid))
            {
                producto.rowguid = Guid.NewGuid().ToString();
            }*/
            if (myParametro.GetParSectores() > 0 && Application.Current.Properties.ContainsKey("SecCodigo"))
            {
                producto.SecCodigo = Application.Current
                    .Properties["SecCodigo"].ToString();
            }

            if (!byOferta)
            {
                string where = "";

                var parVentasLotes = Arguments.Values.CurrentModule == Modules.CAMBIOSMERCANCIA ? myParametro.GetParCambiosMercanciaLotes() : Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS ? myParametro.GetParConteosFisicosLotes() : Arguments.Values.CurrentModule == Modules.INVFISICO ? myParametro.GetParInventarioFisicosLotes() : myParametro.GetParVentasLote();

                if (isEntrega && myParametro.GetParVentasLotesAutomaticos() > 0 && producto.UsaLote)
                {
                    where = " and Posicion = " + producto.Posicion;
                }
                else if (Arguments.Values.CurrentModule == Modules.DEVOLUCIONES || ((Arguments.Values.CurrentModule == Modules.VENTAS || Arguments.Values.CurrentModule == Modules.INVFISICO || Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS || Arguments.Values.CurrentModule == Modules.CAMBIOSMERCANCIA) && !string.IsNullOrWhiteSpace(producto.ProDatos3) && producto.ProDatos3.ToUpper().Contains("L") && (parVentasLotes == 1 || parVentasLotes == 2)))
                {
                    where = " and (ifnull(ltrim(rtrim(upper(Lote))), '') = '" + (string.IsNullOrWhiteSpace(producto.Lote) ? "" : producto.Lote.Trim().ToUpper()) + "' " + (producto.Cantidad > 0 || producto.CantidadDetalle > 0 ? " or Lote is null" : "") + ")";
                }

                if (Arguments.Values.CurrentModule == Modules.CAMBIOSMERCANCIA && (parVentasLotes == 1 || parVentasLotes == 2))
                {
                    where += " and TipoCambio = " + producto.TipoCambio + " ";
                }

                if (!string.IsNullOrWhiteSpace(producto.UnmCodigo))
                {
                    where += " and ifnull(trim(UnmCodigo), '') = '" + producto.UnmCodigo.Trim() + "' ";
                }

                var parComprasFacturas = myParametro.GetParComprasUsarFacturas();

                if (parComprasFacturas)
                {
                    where += " and ifnull(trim(Documento), '') = '" + (string.IsNullOrWhiteSpace(producto.Documento) ? "" : producto.Documento.Trim()) + "' ";
                }

                if (IsMultiEntrega)
                {
                    where += " and ifnull(PedFechaEntrega, '') = '" + producto.PedFechaEntrega + "' ";
                }

                if (Arguments.Values.CurrentModule == Modules.PEDIDOS)
                {
                    if (producto.UseAttribute1)
                    {
                        where += " and ifnull(ProAtributo1, '') = '" + producto.ProAtributo1 + "' ";
                    }
                    if (producto.UseAttribute2)
                    {
                        where += " and ifnull(ProAtributo2, '') = '" + producto.ProAtributo2 + "' ";
                    }
                }

                int affecteds = 0;
                if (!myParametro.GetParDevolucionCondicion())
                {
                    affecteds = (producto.IndicadorOferta ? 0 : SqliteManager.GetInstance().Execute("delete from ProductosTemp where " + (Arguments.Values.CurrentModule == Modules.AUDITORIAPRECIOS ? " ProCodigo = '" + producto.ProCodigo + "' and CatCodigo = '" + producto.CatCodigo + "' and MarCodigo = '" + producto.MarCodigo + "' " : " ProID = " + producto.ProID.ToString()) + " " + (parPromociones ? " and ifnull(IndicadorPromocion, 0) = " + (producto.IndicadorPromocion ? "1" : "0") : "") + " " + (parInvArea ? " and InvAreaId = " + producto.InvAreaId : "") + where, new string[] { }));
                }

                if (producto.IndicadorOferta)
                {
                    SqliteManager.GetInstance().Execute("Update ProductosTemp set CantidadOferta = " + producto.CantidadOferta + " where ProID = " + producto.ProID.ToString() + " and IndicadorOferta <> 1 ");
                }

                if ((((Arguments.Values.CurrentModule == Modules.INVFISICO || Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS) && parInvArea) || ((parVentasLotes == 1 || parVentasLotes == 2) && !string.IsNullOrWhiteSpace(producto.ProDatos3) && producto.ProDatos3.ToUpper().Contains("L")) || parComprasFacturas || Arguments.Values.CurrentModule == Modules.DEVOLUCIONES || IsMultiEntrega || parPromociones || producto.UseAttribute1 || producto.UseAttribute2) && (affecteds == 0 && !myParametro.GetParDevolucionCondicion()))
                {
                    producto.rowguid = Guid.NewGuid().ToString();
                }
            }

            if (producto.IndicadorEliminar && !byOferta)
            {
                producto.InvAreaId = -1;
                producto.PrecioTemp = 0;
            }

            producto.TitID = (int)Arguments.Values.CurrentModule;

            var insertar = true;

            if (isEntrega && myParametro.GetParVentasLotesAutomaticos() > 0 && producto.UsaLote && producto.Cantidad > 0)
            {
                insertar = false;
            }

            producto.ProPosicion = DateTime.Now.ToString();

            if (insertar)
            {
                var parCant = myParametro.GetParInventariosTomarCantidades();

                if (Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS)
                {
                    parCant = myParametro.GetParColocacionProductosTomarCantidades();
                }

                if (parCant is int numofparm && numofparm > 0 && parInvArea)
                {
                    List<UsosMultiples> InvAreas = new DS_UsosMultiples().GetInvAreas();
                    UsosMultiples invarea = null;

                    switch (numofparm)
                    {
                        case 1:
                            if (producto.CantidadAlm != null)
                            {
                                invarea = InvAreas.FirstOrDefault(i => i.CodigoUso == "1");
                                producto.InvAreaId = int.Parse(invarea.CodigoUso);
                                producto.InvAreaDescr = invarea.Descripcion;
                                producto.CantidadDetalle = (int)producto.CantidadAlm;
                                SqliteManager.GetInstance().InsertOrReplace(producto);
                            }

                            if (producto.CanTidadGond != null)
                            {
                                invarea = InvAreas.FirstOrDefault(i => i.CodigoUso == "2");
                                producto.rowguid = Guid.NewGuid().ToString();
                                producto.InvAreaId = int.Parse(invarea.CodigoUso);
                                producto.InvAreaDescr = invarea.Descripcion;
                                producto.Cantidad = (int)producto.CanTidadGond;
                                SqliteManager.GetInstance().InsertOrReplace(producto);
                            }
                            return;
                        case 2:
                            if (producto.CantidadAlm != null)
                            {
                                invarea = InvAreas.FirstOrDefault(i => i.CodigoUso == "1");
                                producto.InvAreaId = int.Parse(invarea.CodigoUso);
                                producto.InvAreaDescr = invarea.Descripcion;
                                producto.Cantidad = (int)producto.CantidadAlm;
                                producto.CantidadDetalle = producto.UnidadAlm != null ? (int)producto.UnidadAlm : 0;
                                SqliteManager.GetInstance().InsertOrReplace(producto);
                            }

                            if (producto.CanTidadGond != null)
                            {
                                invarea = InvAreas.FirstOrDefault(i => i.CodigoUso == "2");
                                producto.rowguid = Guid.NewGuid().ToString();
                                producto.InvAreaId = int.Parse(invarea.CodigoUso);
                                producto.InvAreaDescr = invarea.Descripcion;
                                producto.Cantidad = (int)producto.CanTidadGond;
                                producto.CantidadDetalle = producto.UnidadGond != null ? (int)producto.UnidadGond : 0;
                                SqliteManager.GetInstance().InsertOrReplace(producto);
                            }
                            return;
                        case 3:
                            if (producto.CantidadAlm != null)
                            {
                                invarea = InvAreas.FirstOrDefault(i => i.CodigoUso == "1");
                                producto.InvAreaId = int.Parse(invarea.CodigoUso);
                                producto.InvAreaDescr = invarea.Descripcion;
                                producto.Cantidad = (int)producto.CantidadAlm;
                                SqliteManager.GetInstance().InsertOrReplace(producto);
                            }

                            if (producto.CanTidadGond != null)
                            {
                                invarea = InvAreas.FirstOrDefault(i => i.CodigoUso == "2");
                                producto.rowguid = Guid.NewGuid().ToString();
                                producto.InvAreaId = int.Parse(invarea.CodigoUso);
                                producto.InvAreaDescr = invarea.Descripcion;
                                producto.Cantidad = (int)producto.CanTidadGond;
                                SqliteManager.GetInstance().InsertOrReplace(producto);
                            }

                            if (producto.CanTidadTramo != null)
                            {
                                invarea = InvAreas.FirstOrDefault(i => i.CodigoUso == "3");
                                producto.rowguid = Guid.NewGuid().ToString();
                                producto.InvAreaId = int.Parse(invarea.CodigoUso);
                                producto.InvAreaDescr = invarea.Descripcion;
                                producto.Cantidad = (int)producto.CanTidadTramo;
                                SqliteManager.GetInstance().InsertOrReplace(producto);
                            }
                            return;
                    }
                }

                if (myParametro.GetParCambioMercanciaInsertarLotesParaRecivir())
                {
                    SqliteManager.GetInstance().Execute("delete from ProductosTemp where ProID = " + producto.ProID + " ");
                    producto.Lote = producto.LoteRecibido;
                    producto.TipoCambio = 0;
                    SqliteManager.GetInstance().InsertOrReplace(producto);
                    producto.rowguid = Guid.NewGuid().ToString();
                    producto.Lote = producto.LoteEntregado;
                    producto.TipoCambio = 1;
                    SqliteManager.GetInstance().InsertOrReplace(producto);

                    return;
                }


                if (myParametro.GetParDevolucionCondicion() && Arguments.Values.CurrentModule == Modules.DEVOLUCIONES)
                {
                    ProductosTemp productostemp = SqliteManager.GetInstance().Query<ProductosTemp>(" Select proid, DevCondicion from productosTemp t  where t.proid = ? and t.DevCondicion = ?", new string[] { producto.ProID.ToString(), producto.DevCondicion.ToString() }).FirstOrDefault();
                    UsosMultiples usosmultiple = SqliteManager.GetInstance().Query<UsosMultiples>(" Select u.Descripcion as Descripcion from  UsosMultiples u  where u.CodigoGrupo = ?  and u.CodigoUso= ? ", new string[] { "DEVCONDICION", producto.DevCondicion.ToString() }).FirstOrDefault();
                    producto.DevDescripcion = usosmultiple.Descripcion;

                    if (productostemp != null)
                    {
                        SqliteManager.GetInstance().Execute("delete from ProductosTemp where ProID= " + producto.ProID + " and  DevCondicion = " + producto.DevCondicion.ToString(), new string[] { });
                        SqliteManager.GetInstance().InsertOrReplace(producto);
                    }
                    else
                    {
                        producto.rowguid = Guid.NewGuid().ToString();
                        SqliteManager.GetInstance().Insert(producto);
                    }

                    return;
                }

                if ((isEntrega || producto.Cantidad > 0 || producto.CantidadDetalle > 0 || isfromprecios || IsFromCont || myParametro.GetParInventarioFisicoAceptarProductosCantidadCero() || myParametro.GetParConteoFisicoAceptarProductosCantidadCero()) && !producto.IndicadorEliminar)
                {
                    if (myParametro.GetParProdUseUnmCodigo())
                    {
                        SqliteManager.GetInstance().Execute
                            ("delete from ProductosTemp where ProID = " + producto.ProID + " and UnmCodigo != '" + producto.UnmCodigo + "' ");
                    }

                    if (!producto.IndicadorOferta)
                    {
                        SqliteManager.GetInstance().InsertOrReplace(producto);
                    }
                    else
                    {
                        SqliteManager.GetInstance().Insert(producto);
                    }

                }
            }

            if (myParametro.GetParVentasLotesAutomaticos() > 0 && producto.UsaLote)
            {
                InsertInTempWithAutomaticLots(producto, isEntrega);
            }

            if (isEntrega && producto.UsaLote && ((!(producto.Cantidad == 0 && string.IsNullOrWhiteSpace(producto.Lote))) || myParametro.GetParVentasLotesAutomaticos() > 0))
            {
                HandleProductWithLoteForDelivery(producto);
            }
        }

        /* private bool ExistInTempWithOtherLot(string rowguid, string lote)
         {
             var list = SqliteManager.GetInstance().Query<ProductosTemp>("select ProID from ProductosTemp " +
                 "where rowguid = '"+rowguid+"' and ifnull(Lote, '') != '"+lote+"'", new string[] { });

             return list != null && list.Count > 0;
         }
        */
        private void HandleProductWithLoteForDelivery(ProductosTemp producto)
        {
            SqliteManager.GetInstance().Execute("Delete from ProductosTemp " +
                       "where ifnull(ProDatos3, '') like '%L%' and (trim(ifnull(Lote, '')) = '') and Posicion = ? and ProID = ? " +
                       "and TitID = " + ((int)Arguments.Values.CurrentModule) + " ",
                       new string[] { producto.Posicion.ToString(), producto.ProID.ToString() });

            //se insertan los productos faltantes con lote ''
            SqliteManager.GetInstance().Execute("insert into ProductosTemp(rowguid, TitID, ProID, ProCodigo, Cantidad, CantidadDetalle, ProUnidades, " +
                "Descripcion, ProDatos3, IndicadorOferta," +
                "Precio, Itbis, Selectivo, AdValorem, Descuento, DesPorciento, UnmCodigo, IndicadorDetalle, ProPrecio3, ProDescripcion2, " +
                "ProDescripcion3, ProDatos2, ProDatos1, ProDescripcion1, Lote, Posicion, CantidadEntrega) " +
                "select ?, ?, ?, ?, (" + producto.CantidadEntrega + " - s.Cantidad), 0, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ? " +
                "from Productos t " +
                "inner join (select ifnull(sum(Cantidad), 0) as Cantidad, ProID, Posicion " +
                "from ProductosTemp where ProID = " + producto.ProID + " and Posicion = " + producto.Posicion + " and upper(ifnull(ProDatos3, '')) like '%L%' " +
                "and ifnull(Lote, '') != '' group by ProID, Posicion) s on s.ProID = t.ProID and s.Posicion = " + producto.Posicion + " " +
                "where t.ProID = " + producto.ProID + " and " + (producto.CantidadEntrega) + " - s.Cantidad > 0 ",
                new string[] { Guid.NewGuid().ToString(), ((int)Arguments.Values.CurrentModule).ToString(), producto.ProID.ToString(), producto.ProCodigo, producto.ProUnidades.ToString(),
                producto.Descripcion, producto.ProDatos3, producto.IndicadorOferta ? "1" : "0", producto.Precio.ToString(), producto.Itbis.ToString(), producto.Selectivo.ToString(), producto.AdValorem.ToString(), producto.Descuento.ToString(),
                producto.DesPorciento.ToString(), producto.UnmCodigo, producto.IndicadorDetalle ? "1" : "0", producto.ProPrecio3.ToString(), producto.ProDescripcion2, producto.ProDescripcion3, producto.ProDatos2,
                producto.ProDatos1, producto.ProDescripcion1, "", producto.Posicion.ToString(), producto.CantidadEntrega.ToString()});

            if (myParametro.GetParVentasLotesAutomaticos() > 0 && !ExistsInTemp(producto.ProID, producto.TitID, producto.IndicadorPromocion, producto.Posicion))
            {
                producto.Lote = "";
                producto.Cantidad = producto.CantidadEntrega;
                producto.rowguid = Guid.NewGuid().ToString();

                SqliteManager.GetInstance().Insert(producto);
            }
        }

        private void InsertInTempWithAutomaticLots(ProductosTemp producto, bool isEntrega = true)
        {
            var dsInv = new DS_Inventarios();
            var inventario = new List<Inventarios>();
            var almId = -1;
            if (isEntrega)
            {
                almId = myParametro.GetParAlmacenIdParaDespacho();
            }
            else
            {
                almId = myParametro.GetParAlmacenVentaRanchera();
            }

            if (myParametro.GetParVentasLotesAutomaticos() == 2)
            {
                inventario = dsInv.GetInventarioAlmacenTotalForLotByEntrega(producto.EnrSecuencia, producto.TraSecuencia, producto.IndicadorOferta ? 1 : 0, producto.ProID, almId);
            }
            else
            {
                inventario = dsInv.GetInventarioAlmacenTotalForLot(producto.ProID, almId);
            }


            //Se cambia el orden de toma de el lote  para que tome del que tiene menos cantidad primero.
            inventario = inventario.OrderBy(i => i.invCantidad).ToList();

            var cantidadRestante = producto.Cantidad;

            if (producto.Cantidad <= 0)
            {
                SqliteManager.GetInstance().Execute("delete from ProductosTemp where ProID = ?",
                new string[] { producto.ProID.ToString() });
                return;
            }

            if (inventario.Count > 0 && !producto.IndicadorOferta)
            {
                SqliteManager.GetInstance().Execute("delete from ProductosTemp where ProID = ? and Posicion = ?",
                    new string[] { producto.ProID.ToString(), producto.Posicion.ToString() });
            }

            //Se cambia el orden de toma de el lote cuando se calcula la oferta para que tome del que tiene mas cantidad primero
            if (producto.IndicadorOferta)
            {
                inventario = inventario.OrderByDescending(i => i.invCantidad).ToList();
                //foreach (var invlote in inventario)
                //{ if (CantidadMayor < invlote.invCantidad){ LoteUsar = invlote.InvLote;} CantidadMayor += invlote.invCantidad;}
            }

            if (myParametro.GetParAsignacionLotesByFechaVencimiento())
            {
                inventario = inventario.OrderBy(i => i.InvLoteFechaVencimiento).ToList();
            }

            foreach (var inv in inventario)
            {
                if (myParametro.GetParRebajarCantidadTempParaOferta() && producto.IndicadorOferta)
                {
                    var cantidadproductotemp = GetCantidadProductosAgregadosConLote(inv.ProID, inv.InvLote);
                    inv.invCantidad = inv.invCantidad - cantidadproductotemp;
                }

                var cantidadUsar = 0.0;

                if (inv.invCantidad >= cantidadRestante)
                {
                    cantidadUsar = cantidadRestante;
                }
                else if (inv.invCantidad > 0)
                {
                    cantidadUsar = inv.invCantidad;
                }
                else
                {
                    continue;
                }

                cantidadRestante -= cantidadUsar;

                producto.rowguid = Guid.NewGuid().ToString();
                producto.Cantidad = cantidadUsar;
                producto.Lote = inv.InvLote;
                if (!isEntrega)
                {
                    producto.Posicion = 1;
                }
                SqliteManager.GetInstance().InsertOrReplace(producto);

                if (cantidadRestante == 0)
                {
                    break;
                }
            }

        }

        public Totales GetTempTotales(int titId, bool forCalculator = false, bool IsVenta = true, List<ProductosTemp> productos = null, CuentasxCobrar documento = null, bool withoferta = false, double porcDescuentoGeneral = 0)
        {
            //string joinTinIDAdValorem = "";
            List<Totales> list = null;

            if (IsVenta)
            {
                list = SqliteManager.GetInstance().Query<Totales>("select /*SUM(((CAST(ifnull(CantidadDetalle, 0)AS REAL)  / case when ifnull(P.ProUnidades, 0) = 0 then 1 else ifnull(P.ProUnidades, 1) end) + Cantidad)) as CantidadTotal, " +
                " SUM(ifnull(Selectivo, 0) * ifnull(Cantidad, 0)) as Selectivo, " +
                " SUM(round((Precio + ifnull(Selectivo,0) + ifnull(AdValorem,0)) * ((CAST(ifnull(CantidadDetalle, 0) AS REAL)  / case when ifnull(P.ProUnidades, 0) = 0 then 1 else ifnull(P.ProUnidades, 1) end) + Cantidad),2)) as SubTotal," +
                " SUM(((((Precio + Selectivo + AdValorem) - Descuento) + round(((Precio + ifnull(Selectivo, 0) + ifnull(AdValorem, 0)) - Descuento)) * (Itbis / 100))) * ((CAST(ifnull(CantidadDetalle, 0) AS REAL)  / case when ifnull(CAST(P.ProUnidades AS REAL), 0) = 0 then 1 else CAST(ifnull(P.ProUnidades, 1) AS REAL) end) + Cantidad)) as Total, " +
                " SUM(Round((Descuento * ((Cast(ifnull(t.CantidadDetalle, 0) as real)  / case when ifnull(P.ProUnidades, 1) = 0 then 1 else ifnull(P.ProUnidades, 1) end) + Cantidad)),2)) as Descuento," +
                "  SUM((Round(((Precio + ifnull(Selectivo,0) + ifnull(AdValorem,0)) - Descuento) * (Itbis / 100),2)) *  ((CAST(ifnull(CantidadDetalle, 0)AS REAL)  / case when ifnull(P.ProUnidades, 0) = 0 then 1 else ifnull(P.ProUnidades, 1) end) + Cantidad)) as Itbis " +
                " ,*/ Cantidad,  CantidadDetalle, " + (myParametro.GetParADVALOREMTIPO() == 1 ? " ptna.ProAdValorem as AdValoremU, " : " AdValorem as AdValoremU, ") + " Selectivo as SelectivoU, Precio, Descuento as DescuentoU, Itbis as ItbisT, p.ProUnidades, DesPorciento, ifnull(t.PedFlete, 0) as Flete " +
                " from ProductosTemp t " +
                " inner join Productos P on P.ProID = t.ProID " + (myParametro.GetParADVALOREMTIPO() == 1 ? " left join ProductosTiposNegocioAdvalorem ptna on ptna.proid = p.proid and ptna.TinId = " + Arguments.Values.CurrentClient.TiNID + "" : " ") + " " +
                " WHERE " + (!withoferta ? " ifnull(IndicadorOferta, 0) = 0 and " : "") + " " + (DS_RepresentantesParametros.GetInstance().GetParVentasLote() > 0 && !forCalculator ? " t.Lote <> '' and " : "") + " TitID = ? ", new string[] { titId.ToString() });

            }
            else
            {
                list = SqliteManager.GetInstance().Query<Totales>("select " +
                " TraCantidad Cantidad, TraCantidadDetalle  CantidadDetalle, TraAdValorem as AdValoremU, TraSelectivo as SelectivoU," +
                " TraPrecio Precio, TraDescuento as DescuentoU, TraItbis as ItbisT, p.ProUnidades, TraDesPorciento DesPorciento " +
                " from EntregasRepartidorTransaccionesDetalle t " +
                " inner join Productos P on P.ProID = t.ProID " +
                " inner join EntregasRepartidorTransacciones et on et.RepCodigo = t.RepCodigo and et.EnrSecuencia=t.EnrSecuencia and et.cliid= " + Arguments.Values.CurrentClient.CliID.ToString() + " " +
                " WHERE ifnull(TraIndicadorOferta, 0) = 0   and t.TitID = 1 and enrEstatusEntrega = 1 and t.CliID = ? ", new string[] { Arguments.Values.CurrentClient.CliID.ToString() });

            }

            if (list.Count > 0)
            {
                if (documento != null)
                {
                    double descTotal = 0.0;

                    var query = "select ProCodigo, ProDescripcion, CxcCantidad as ProCantidad, CxcPrecio as ProPrecio, CxcItbis as ProItbis, CxcDescuento as ProDescuentoMaximo, UnmCodigo   " +
                "from CuentasxCobrarDetalle cxc " +
                "inner join Productos p on p.ProID = cxc.ProID " +
                "where ltrim(rtrim(upper(cxc.CxcReferencia))) = ? " +
                "Order By CxcPosicion";

                    if (Arguments.Values.CurrentModule == Modules.PUSHMONEYPORPAGAR)
                    {
                        query = "select ProCodigo, ProDescripcion, PxpCantidad as ProCantidad, PxpPrecio as ProPrecio, PxpItbis as ProItbis, PxpDescuento as ProDescuentoMaximo, UnmCodigo   " +
                "from PushMoneyPorPagarDetalle cxc " +
                "inner join Productos p on p.ProID = cxc.ProID " +
                "where ltrim(rtrim(upper(cxc.PxpReferencia))) = ? " +
                "Order By PxpPosicion";
                    }

                    var productos1 = SqliteManager.GetInstance().Query<Productos>(query, new string[] { documento.CxcReferencia.Trim().ToUpper() });

                    if (productos1 != null && productos1.Count > 0)
                    {
                        foreach (var pro in productos1)
                        {

                            var valorneto = (pro.ProPrecio * pro.ProCantidad);
                            var itbis1 = (valorneto) * (pro.ProItbis / 100);
                            var desc = (valorneto + itbis1) * (pro.ProDescuentoMaximo / 100);
                            descTotal += desc;
                        }
                    }

                    list[0].Total = documento.CxcMontoTotal;
                    list[0].Descuento = descTotal;
                    list[0].Itbis = documento.CxcMontoTotal - documento.CxcMontoSinItbis;
                    list[0].SubTotal = documento.CxcMontoSinItbis;

                    var total = list[0];
                    return total;
                }
            }

            double DescuentoOfertas = 0;
            List<Totales> ofeDesc;
            //TODO : Agregar parametro para que use redondeo de 2 unidades
            if (DS_RepresentantesParametros.GetInstance().GetParRedondeoCantidadesDecimales())
            {
                ofeDesc = SqliteManager.GetInstance().Query<Totales>("select sum( case when ifnull(OfeCaracteristica,'') <> '' then PrecioBase * Round(PedOfeCantidad,2) else PrecioTemp * Round(Cantidad,2) end   ) as DescuentoOfertas from ProductosTemp " +
                "where (ifnull(IndicadorOferta, 0) = 1 or ifnull(OfeCaracteristica,'') in ('P','O') ) and TitID = ?  " + (DS_RepresentantesParametros.GetInstance().GetParVentasLote() > 0 && !forCalculator ? " and Lote <> '' " : "") + " ", new string[] { titId.ToString() });
            }
            else
            {
                ofeDesc = SqliteManager.GetInstance().Query<Totales>("select sum( case when ifnull(OfeCaracteristica,'') <> '' then PrecioBase * PedOfeCantidad else PrecioTemp * Cantidad end ) as DescuentoOfertas from ProductosTemp " +
                "where (ifnull(IndicadorOferta, 0) = 1 or ifnull(OfeCaracteristica,'') in ('P','O') ) and TitID = ? " + (DS_RepresentantesParametros.GetInstance().GetParVentasLote() > 0 && !forCalculator ? " and Lote <> '' " : "") + " ", new string[] { titId.ToString() });
            }


            double DecuentoTotal = 0.0;
            double DecuentoTotalGeneral = 0.0;
            double PrecioTotal = 0.0;
            double ItebisTotal = 0.0;
            double SubTotal = 0.0;
            double Total = 0.0;
            double adValorem = 0.0;
            double selectivo = 0.0;
            double CantidadTotal = 0.0;
            double PrecioLista = 0.0;
            double AdValoremU = 0.0;
            double TotalUnitarioSinItbis = 0.0;
            double SelectivoU = 0.0;
            double FleteTotal = 0.0;
            foreach (var det in list)
            {
                double Descuentos;
                double Descuentos1;
                AdValoremU = det.AdValoremU;
                SelectivoU = det.SelectivoU;

                PrecioLista = (det.Precio + AdValoremU + SelectivoU);
                if (!myParametro.GetParPrecioSinRedondeo())
                {
                    PrecioLista = Math.Round(PrecioLista, 2, MidpointRounding.AwayFromZero);
                }

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

                if (productos == null)
                {

                    if (myParametro.GetParADVALOREMTIPO() == 1)
                    {
                        adValorem += AdValoremU * ((det.CantidadDetalle / det.ProUnidades) + det.Cantidad);
                    }
                    else
                    {
                        adValorem += AdValoremU * CantidadReal;
                    }
                    selectivo += SelectivoU * CantidadReal;
                }

                PrecioTotal += PrecioLista * CantidadReal;
                PrecioTotal = Math.Round(PrecioTotal, 2, MidpointRounding.AwayFromZero);

                Descuentos1 = (det.Precio * det.Descuento) / 100;
                Descuentos1 = Math.Round(Descuentos1, 2, MidpointRounding.AwayFromZero);


                Descuentos = (det.DesPorciento / 100) * det.Precio;
                if (!myParametro.GetParDescuentoSinRedondeo())
                {
                    Descuentos = Math.Round(Descuentos, 2, MidpointRounding.AwayFromZero);
                }

                if (Descuentos == 0.0)
                {
                    Descuentos = det.Descuento;
                }

                if (myParametro.GetDescuentoxPrecioNegociado())
                {
                    Descuentos = det.DescuentoU;
                }

                double descTotalUnitario = Descuentos * CantidadReal;
                double descGeneralTotalUnitario = (((det.Precio - Descuentos) * CantidadReal) * (porcDescuentoGeneral / 100));
                if (!myParametro.GetParDescuentoSinRedondeo())
                {
                    descTotalUnitario = Math.Round(descTotalUnitario, 2, MidpointRounding.AwayFromZero);
                    descGeneralTotalUnitario = Math.Round(descGeneralTotalUnitario, 2, MidpointRounding.AwayFromZero);
                }

                DecuentoTotal += descTotalUnitario;
                DecuentoTotalGeneral += (((det.Precio - Descuentos) * CantidadReal) * (porcDescuentoGeneral / 100)); // TODO: Para descuento Global
                //DecuentoTotal += (((det.Precio - Descuentos) * CantidadReal) * (0.10)); // TODO: Para descuento Global
                if (!myParametro.GetParDescuentoSinRedondeo())
                {
                    DecuentoTotal = Math.Round(DecuentoTotal, 2, MidpointRounding.AwayFromZero);
                    DecuentoTotalGeneral = Math.Round(DecuentoTotalGeneral, 2, MidpointRounding.AwayFromZero);
                }

                TotalUnitarioSinItbis += Math.Round((PrecioLista * CantidadReal) - (descTotalUnitario + descGeneralTotalUnitario), 2, MidpointRounding.AwayFromZero);
                double tasaItbis = det.ItbisT;

                //double MontoItbis = (PrecioLista - Descuentos) * CantidadReal;
                double MontoItbis = ((PrecioLista - Descuentos) - ((PrecioLista - Descuentos) * (porcDescuentoGeneral / 100))) * CantidadReal; //TODO: Para descuento Global
                if (!myParametro.GetParItbisSinRedondeo())
                {
                    MontoItbis = Math.Round(MontoItbis, 2, MidpointRounding.AwayFromZero);
                }


                ItebisTotal += MontoItbis * (tasaItbis / 100.0);
                if (!myParametro.GetParItbisSinRedondeo())
                {
                    ItebisTotal = Math.Round(ItebisTotal, 2, MidpointRounding.AwayFromZero);
                }


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

                FleteTotal += det.Flete;
            }

            if (myParametro.GetParItbisRedondeoGeneral())
            {
                ItebisTotal = Math.Round(ItebisTotal, 2, MidpointRounding.AwayFromZero);
            }

            if (myParametro.GetParDescuentoRedondeoGeneral())
            {
                DecuentoTotal = Math.Round(DecuentoTotal, 2, MidpointRounding.AwayFromZero);
                DecuentoTotalGeneral = Math.Round(DecuentoTotalGeneral, 2, MidpointRounding.AwayFromZero);
            }

            Total = (PrecioTotal) - (DecuentoTotal + DecuentoTotalGeneral) + ItebisTotal + FleteTotal;

            if (productos != null)
            {
                Total = 0.0;
                PrecioTotal = 0.0;

                foreach (var pro in productos)
                {
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

                    PrecioLista = (pro.Precio + AdValoremU + SelectivoU);
                    PrecioLista = Math.Round(PrecioLista, 2, MidpointRounding.AwayFromZero);

                    adValorem += AdValoremU * cantidad;
                    selectivo += SelectivoU * cantidad;

                    PrecioTotal += PrecioLista * cantidad;
                    PrecioTotal += DecuentoTotal + ItebisTotal;
                }
                Total = (PrecioTotal) - DecuentoTotal + ItebisTotal;
            }

            if (myParametro.GetParTotalUnitarioSinItbis())
            {
                Total = Math.Round(TotalUnitarioSinItbis + ItebisTotal + FleteTotal, 2, MidpointRounding.AwayFromZero);
            }
            else if (myParametro.GetParDescuentoSinRedondeo() || myParametro.GetParItbisSinRedondeo())
            {
                Total = Math.Round(Total, 2, MidpointRounding.AwayFromZero);
            }

            SubTotal = (PrecioTotal);

            if (list.Count > 0)
            {
                var montototal = Total; //Math.Round(list[0].SubTotal + list[0].Itbis - list[0].Descuento,2);//list[0].Total;//list[0].SubTotal + list[0].Itbis - list[0].Descuento;
                list[0].Total = montototal;
                list[0].Itbis = ItebisTotal;
                list[0].Descuento = DecuentoTotal;
                list[0].DescuentoGeneral = DecuentoTotalGeneral;
                list[0].PorCientoDsctoGeneral = porcDescuentoGeneral;
                list[0].SubTotal = PrecioTotal;
                list[0].AdValorem = adValorem;
                list[0].Selectivo = selectivo;
                list[0].CantidadTotal = (int)CantidadTotal;
                list[0].Flete = FleteTotal;
                var total = list[0];
                total.DescuentoOfertas = DescuentoOfertas;
                return total;
            }

            return new Totales();
        }

        public ProductosTemp ExistsProductoAgregadoPorOferta(int proId, int titId, int ofeId = -1)
        {
            try
            {
                var where = "";

                if (ofeId != -1)
                {
                    where = " and OfeID = " + ofeId.ToString();
                }

                return SqliteManager.GetInstance().Query<ProductosTemp>("select * from ProductosTemp where ProID = ? and TitID = ? and IndicadorOferta = 1 " + where + " limit 1", new string[] { proId.ToString(), titId.ToString() }).FirstOrDefault();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return null;
            }
        }
        public ProductosTemp ExistsProductoAgregadoPorOfertaManu(int titId, int ofeId = -1)
        {
            try
            {
                var where = "";

                if (ofeId != -1)
                {
                    where = " and OfeID = " + ofeId.ToString();
                }

                return SqliteManager.GetInstance().Query<ProductosTemp>("select * from ProductosTemp where TitID = ? and IndicadorOferta = 1 " + where + " limit 1", new string[] { titId.ToString() }).FirstOrDefault();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return null;
            }
        }

        public ProductosTemp ExistsProductoAgregadoPorOfertaForEdit(int proId, int titId, int ofeId)
        {
            try
            {
                return SqliteManager.GetInstance().Query<ProductosTemp>("select Cantidad from ProductosTemp where ProID = ?  and OfeID = ? and TitID = ? limit 1",
                       new string[] { proId.ToString(), ofeId.ToString(), titId.ToString() }).FirstOrDefault();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return null;
            }
        }
        public void ActualizarCantidadProductoOferta(int proId, double cantidad, int titId)
        {
            SqliteManager.GetInstance().Execute($@"update ProductosTemp set Cantidad = Cantidad + {cantidad}, 
                              CantidadOferta = CantidadOferta + {cantidad} where ProID = {proId} and IndicadorOferta = 1
                              and TitID = {titId}");
        }

        public void DeleteOfertaInTemp(int titId = -1, int ofeId = -1, int proId = -1, bool sinCantidad = false, int proIdOrigenOferta = -1, string UnmCodigo = "", bool isFromMancomunada = false, string grpCodigo = "")
        {
            try
            {
                var where = "";

                var list = SqliteManager.GetInstance().Query<ProductosTemp>
                ($@"select 1 from ProductosTemp where proid = {proId} and ifnull(IndicadorOferta, 0) = 1 
                and ProIDOferta not in (select distinct p.ProID from Productos p 
                inner join GrupoProductosDetalle g on ltrim(rtrim(upper(g.GrpCodigo))) = upper('{grpCodigo}') and g.ProID = p.ProID)");

                where += isFromMancomunada && list.Count > 0 ?
                " and OfeID = " + ofeId.ToString() : !isFromMancomunada && ofeId != -1 ? " and OfeID = " + ofeId.ToString() : "";

                if (proId != -1)
                {
                    where += " and ProID = " + proId.ToString();
                }

                if (sinCantidad)
                {
                    where += " and ifnull(Cantidad, 0) = 0";
                }

                if (UnmCodigo != "")
                {
                    where += " and UnmCodigo = '" + UnmCodigo.ToString() + "' ";
                }

                SqliteManager.GetInstance().Execute("delete from ProductosTemp where ifnull(IndicadorOferta, 0) = 1 and TitID = ? " + where, new string[] { titId.ToString() });

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        public void ReiniciarCantidadOfertaRebajaVenta()
        {
            try
            {
                SqliteManager.GetInstance().Execute("update ProductosTemp set Cantidad = Cantidad + ofeCantidadRebajaVenta, ofeCantidadRebajaVenta = 0 " +
                    "where ifnull(IndicadorOferta, 0) = 0 and ifnull(ofeCantidadRebajaVenta, 0) > 0 and TitID = ?",
                    new string[] { ((int)Arguments.Values.CurrentModule).ToString() });
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        public void EliminarDescuentoInTemp(int titId)
        {
            if (!myParametro.GetParPedDescLip())
            {
                if (!myParametro.GetDescuentoxPrecioNegociado())
                {
                    SqliteManager.GetInstance().Execute("update ProductosTemp set Descuento = 0, DesPorciento = 0, DesIdsAplicados = ''", new string[] { });
                }
            }

            if (myParametro.GetParPedidosDescuentoManual() || myParametro.GetParCotizacionesDescuentoManual() || myParametro.GetParDevolucionesDescuentoManual())
            {
                SqliteManager.GetInstance().Execute("update ProductosTemp set DesPorciento = ifnull(DesPorcientoManual, 0), Descuento = (DesPorcientoManual * Precio) / 100 where ifnull(DesPorcientoManual, 0) > 0", new string[] { });
            }


        }

        public void ActualizarCantidadOferta(int proId, int ofeId, double cantidad, int titId, string lote = null)
        {
            SqliteManager.GetInstance().Execute("update ProductosTemp set Cantidad = " + cantidad.ToString() + " " +
                "where ProID = ? and OfeID = ? and IndicadorOferta = 1 and TitID = ? " + (!string.IsNullOrWhiteSpace(lote) ? " and upper(trim(ifnull(Lote, ''))) = '" + lote.Trim().ToUpper() + "'" : ""),
                new string[] { proId.ToString(), ofeId.ToString(), titId.ToString() });
        }

        public void ActualizarCantidadOfertaConUnidades(int proId, int proIdOferta, int ofeId, double cantidad, int titId, string lote = null, string unmCodigo = "")
        {
            SqliteManager.GetInstance().Execute("update ProductosTemp set Cantidad = " + cantidad.ToString() + " " +
                "where ProID = ? and UnmCodigo = ? and ProIDOferta=? and OfeID = ? and IndicadorOferta = 1 and TitID = ? " + (!string.IsNullOrWhiteSpace(lote) ? " and upper(trim(ifnull(Lote, ''))) = '" + lote.Trim().ToUpper() + "'" : ""),
                new string[] { proId.ToString(), proIdOferta.ToString(), unmCodigo.ToString(), ofeId.ToString(), titId.ToString() });
        }

        public int GetCantidadProductoOferta(int titId, int proId = -1, int ofeId = -1, int proIdToExclude = -1, bool fordetalle = false)
        {
            var item = SqliteManager.GetInstance().Query<ProductosTemp>("select " + (fordetalle ? "sum(CantidadDetalle) + sum(Cantidad * ProUnidades)" : "sum(Cantidad)") + " as Cantidad from ProductosTemp " +
                "where ifnull(IndicadorOferta, 0) = 1 " +
                "and TitID = " + titId.ToString() + " " + (proIdToExclude != -1 ? " and ProID <> " + proIdToExclude.ToString() : "") +
                (proId != -1 ? " and ProID = " + proId.ToString() : "") + " " + (ofeId != -1 ? " and OfeID = " + ofeId.ToString() : ""),
                new string[] { }).FirstOrDefault();

            if (item != null)
            {
                return (int)item.Cantidad;
            }

            return 0;
        }

        public void ActualizarIndicadorRebajaVenta()
        {
            try
            {
                SqliteManager.GetInstance().Execute("update ProductosTemp set Cantidad = Cantidad + ofeCantidadRebajaVenta, ofeCantidadRebajaVenta = 0 " +
                    "where IndicadorOferta = '0' and ifnull(ofeCantidadRebajaVenta, 0.0) > 0 and TitID = ? ",
                    new string[] { ((int)Arguments.Values.CurrentModule).ToString() });

                var ofertados = SqliteManager.GetInstance().Query<ProductosTemp>("select t.Cantidad as Cantidad, t.ProID as ProID, ProIDOferta, t.OfeID as OfeID,t.lote as Lote from ProductosTemp t " +
                        "inner join Ofertas o on o.OfeID = t.OfeID and o.OfeIndicadorRebajaVenta = 1 and o.OfeTipo in ('1', '2') " +
                        "where ifnull(t.IndicadorOferta, 0) = 1 and t.Cantidad > 0 and t.TitID = ?",
                        new string[] { ((int)Arguments.Values.CurrentModule).ToString() });




                foreach (var p in ofertados)
                {
                    var LineasProductos = SqliteManager.GetInstance().Query<ProductosTemp>("select  t.ProID as ProID from ProductosTemp t " +
                        "where ProID = ? and ifnull(t.IndicadorOferta, 0) = 0 and t.Cantidad > 0 and t.TitID = ?",
                        new string[] { p.ProIDOferta.ToString(), ((int)Arguments.Values.CurrentModule).ToString() });

                    if (LineasProductos.Count > 1)
                    {
                        var ProductosTemp = SqliteManager.GetInstance().Query<ProductosTemp>("select Lote from ProductosTemp where IndicadorOferta=0 and ProID = " + p.ProIDOferta.ToString() + "  order by Cantidad desc limit 1");

                        SqliteManager.GetInstance().Execute("update ProductosTemp set Cantidad = Cantidad - " + p.Cantidad + ", ofeCantidadRebajaVenta = " + p.Cantidad + " " +
                        "where ProID = ? and IndicadorOferta = 0 and TitID = ? and Lote = ?",
                        new string[] { p.ProIDOferta.ToString(), ((int)Arguments.Values.CurrentModule).ToString(), ProductosTemp[0].Lote.ToString() });
                    }
                    else
                    {
                        SqliteManager.GetInstance().Execute("update ProductosTemp set Cantidad = Cantidad - " + p.Cantidad + ", ofeCantidadRebajaVenta = " + p.Cantidad + " " +
                        "where ProID = ? and IndicadorOferta = 0 and TitID = ?",
                        new string[] { p.ProIDOferta.ToString(), ((int)Arguments.Values.CurrentModule).ToString() });
                    }

                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

        }

        public void ActualizarDescuentoInTemp(int proId, double valor, double porciento, int titId)
        {
            SqliteManager.GetInstance().Execute("update ProductosTemp set Descuento = ?, DesPorciento = ?, DesPorcientoManual = ? " +
                "where ProID = ? and ifnull(IndicadorOferta, 0) = 0 and TitID = ?",
                new string[] { valor.ToString(), porciento.ToString(), porciento.ToString(), proId.ToString(), titId.ToString() });
        }

        public List<ProductosRevisionDescuentos> GetProductosConDescuentos(int titId)
        {
            return SqliteManager.GetInstance().Query<ProductosRevisionDescuentos>("select 0 as DescuentoManual, Precio, Descuento as DescuentoValorOriginal, Descuento as DescuentoValorEditado, " +
                "DesPorciento as PorcDescuentoOriginal, DesPorciento as PorcDescuentoEditado, Descripcion as ProDescripcion, ProID " +
                "from ProductosTemp where (ifnull(Descuento, 0.0) > 0.0 OR ifnull(DesPorciento, 0.0) > 0.0) and ifnull(IndicadorOferta, 0) = 0 and TitID = ? ", new string[] { titId.ToString() });
        }

        public bool HayProductosConDescuento(int titId)
        {
            return SqliteManager.GetInstance().Query<Totales>("select 1 as Total from ProductosTemp " +
                "where (ifnull(Descuento, 0.0) > 0.0 OR ifnull(DesPorciento, 0.0) > 0.0) and ifnull(IndicadorOferta, 0) = 0 and TitID = ? ", new string[] { titId.ToString() }).Count > 0;
        }

        public List<ProductosTemp> ValidarProductosOfertasManuales(int titId, bool dppIsActive)
        {

            var dpp = myParametro.GetParPedidosDpp();

            var extra = "";

            if (dppIsActive && dpp > 0)
            {
                extra = " + " + dpp.ToString();
            }

            var ParCajasUnidades = myParametro.GetParCajasUnidadesProductos();

            if (!myParametro.GetParPedidosRangoPrecioMinimoOfertasManuales())
            {
                var query = "select * from ProductosTemp where IndicadorOferta = 0 and ifnull(IndicadorPromocion, 0) = 0 and (ifnull(ValorOfertaManual, 0.0) > 0.0 OR ifnull(DesPorcientoManual, 0) " + extra + "  > 0) " +
                    "and ((Cantidad * Precio) - ifnull(ValorOfertaManual, 0.0) - ((Cantidad * Precio) * ((ifnull(DesPorcientoManual, 0.0) " + extra + ") / 100.0))) < ((case when ifnull(LipPrecioMinimo, 0.0) <= 0 then Precio else ifnull(LipPrecioMinimo, 0.0) end )  * ifnull(Cantidad, 0)) and TitID = ? ";

                return SqliteManager.GetInstance().Query<ProductosTemp>(query, new string[] { titId.ToString() }).ToList();
            }
            else
            {
                var query = "select t.*, l.LipRangoPrecioMinimo as LipRangoPrecioMinimo from ProductosTemp t " +
                    "inner join ListaPrecios l on l.ProID = t.ProID and l.LipCodigo = ? and l.UnmCodigo = t.UnmCodigo " +
                    "where IndicadorOferta = 0 and ifnull(IndicadorPromocion, 0) = 0 and t.TitID = ?";

                var list = SqliteManager.GetInstance().Query<ProductosTemp>(query,
                    new string[] { Arguments.Values.CurrentClient.LiPCodigo, titId.ToString() }).ToList();

                var invalidos = new List<ProductosTemp>();

                foreach (var prod in list)
                {
                    var rangos = JsonConvert.DeserializeObject<List<RangoPrecioMinimo>>(prod.LipRangoPrecioMinimo);

                    if (rangos == null)
                    {
                        continue;
                    }

                    var cantidad = prod.Cantidad;

                    if (ParCajasUnidades)
                    {
                        var unidades = prod.ProUnidades;

                        if (unidades == 0)
                        {
                            unidades = 1;
                        }

                        cantidad = (int)(cantidad / unidades);
                    }

                    var rango = rangos.Where(x => cantidad >= x.LipCantidadInicial && cantidad <= x.LipCantidadFinal).FirstOrDefault();

                    if (rango == null)
                    {
                        invalidos.Add(prod);
                        continue;
                    }

                    var porcientoDpp = 0;
                    if (dppIsActive)
                    {
                        porcientoDpp = myParametro.GetParPedidosDpp();

                        if (porcientoDpp < 0)
                        {
                            porcientoDpp = 0;
                        }
                    }

                    var valorDesc = (cantidad * prod.Precio) * ((prod.DesPorcientoManual + porcientoDpp) / 100.0);

                    var VarDescTotal = (cantidad * prod.Precio) - prod.ValorOfertaManual - valorDesc;

                    if (VarDescTotal < (rango.LipPrecioMinimo * cantidad))
                    {
                        invalidos.Add(prod);
                        continue;
                    }
                }

                return invalidos.ToList();
            }

        }
        public List<ProductosTemp> ValidarProductosDescuentosManuales(int titId, bool dppIsActive)
        {
            var dpp = myParametro.GetParPedidosDpp();

            var extra = "";

            if (dppIsActive && dpp > 0)
            {
                extra = " + " + dpp.ToString();
            }
            var query = "select * from ProductosTemp where IndicadorOferta = 0 and ifnull(IndicadorPromocion, 0) = 0 and (ifnull(ValorOfertaManual, 0.0) > 0.0 OR ifnull(DesPorcientoManual, 0) " + extra + " > 0) " +
                         "and (((ifnull(ValorOfertaManual, 0.0) / (Cantidad * Precio)) * 100) + ifnull(DesPorcientoManual, 0.0) " + extra + ") > ifnull(LipDescuento, 0.0) " +
                         " and TitID = ? ";

            return SqliteManager.GetInstance().Query<ProductosTemp>(query, new string[] { titId.ToString() }).ToList();
        }

        public async Task<List<Productos>> GetProductosNoVendidos(string Tabla, bool isOnline)
        {

            string sql;

            if (myParametro.GetParProductosNoVendidosNewQuery())
            {
                if (isOnline)
                {
                    sql = $"[sp_GrupoProductos]'{Arguments.CurrentUser.RepCodigo}','{Tabla}',{(Arguments.Values.CurrentClient != null ? Arguments.Values.CurrentClient.CliID : 0)}";
                }
                else
                {
                    sql = "select procodigo, prodescripcion, ProID from productos where prodatos3 not like '%P%' " +
                          "order by upper(ProDescripcion)";
                }

            }
            else
            {

                if (myParametro.GetParProductoNoVendido() > 1)
                {
                    sql = $@"select proid, procodigo, ProDescripcion
                            from Productos p 
                            where p.ProDatos3 not like '%P%' 
                            and proid not in (select proid  from {Tabla} c where c.CliID = {Arguments.Values.CurrentClient.CliID})
                            order by 2";
                }
                else
                {

                    sql = "  select distinct ProDescripcion, gp.ProID as ProID, ProCodigo from GrupoProductosDetalle gp "
                                + "inner join ClientesDetalle c on c.GrpCodigo = gp.GrpCodigo "
                                + "inner join Productos p on gp.ProID = p.ProID "
                                + "where " + (Arguments.Values.CurrentClient != null ? " c.cliid = " + Arguments.Values.CurrentClient.CliID + " and " : "") + " c.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' " + (Arguments.Values.CurrentClient != null && Arguments.Values.CurrentModule != Modules.PEDIDOS ? " and c.CliID = " + Arguments.Values.CurrentClient.CliID + "" : "")
                                + " " + (myParametro.GetParSectores() >= 2 && Arguments.Values.CurrentSector != null ? $" and c.SecCodigo = '{Arguments.Values.CurrentSector.SecCodigo}' " : "") + " "
                                + " and p.ProDatos3 not like '%P%' "
                                + "and p.ProID not in (select h.ProID from " + Tabla + " h where h.ProID = p.ProID " + (Arguments.Values.CurrentClient != null ? " and h.CliID = " + Arguments.Values.CurrentClient.CliID + "" : "") + ") "
                                + "order by ProDescripcion";
                }
            }

            if (isOnline)
            {
                return await api.RawQuery<Productos>(Arguments.CurrentUser.RepCodigo, Arguments.CurrentUser.RepClave, sql);
            }


            return new List<Productos>(SqliteManager.GetInstance().Query<Productos>(sql));

        }


        public async Task<List<Productos>> GetAllLineas(string tabla, bool isOnline)
        {

            string sql;

            if (myParametro.GetParProductosNoVendidosNewQuery())
            {
                if (isOnline)
                {
                    sql = $"[sp_ProductosVendidosMesActual]'{Arguments.CurrentUser.RepCodigo}','{tabla}',{(Arguments.Values.CurrentClient != null ? Arguments.Values.CurrentClient.CliID : 0)}";
                }
                else
                {
                    sql = "select LinDescripcion as ProDescripcion, LinID as ProCodigo from lineas where exists (select 1 from productos where linid = lineas.linid and prodatos3 not like '%P%') " +
                          "order by LinDescripcion";
                }

            }
            else
            {
                if (myParametro.GetParProductoNoVendido() > 1)
                {
                    sql = $@"select LinDescripcion AS ProDescripcion, L.LinID as ProCodigo from Lineas L
                             where l.LinID not in(select proid from {tabla} {(Arguments.Values.CurrentClient != null && Arguments.Values.CurrentModule != Modules.PEDIDOS ? " where CliID = " + Arguments.Values.CurrentClient.CliID + "" : "")} )
                             order by LinDescripcion";
                }
                else
                {
                    sql = "select DISTINCT LinDescripcion AS ProDescripcion, L.LinID as ProCodigo from Lineas L "
                                    + "inner join Productos P on P.LinID = L.LinID "
                                    + "inner join GrupoProductosDetalle gp on P.ProID = gp.ProID "
                                    + "inner join ClientesDetalle c on gp.GrpCodigo = c.GrpCodigo "
                                    + "where p.ProDatos3 not like '%P%' "
                                    + "and L.LinID not in (select linID from productos p "
                                    + "inner join " + tabla + " h on h.Proid = p.ProID " + (Arguments.Values.CurrentClient != null ? " and h.CliID = " + Arguments.Values.CurrentClient.CliID + "" : "") + " and h.CliID = c.CliID) "
                                    + "and c.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' " + (Arguments.Values.CurrentClient != null && Arguments.Values.CurrentModule != Modules.PEDIDOS ? " and c.CliID = " + Arguments.Values.CurrentClient.CliID + "" : "") + " order by LinDescripcion";
                }
            }

            if (isOnline)
            {
                return await api.RawQuery<Productos>(Arguments.CurrentUser.RepCodigo, Arguments.CurrentUser.RepClave, sql);
            }
            return new List<Productos>(SqliteManager.GetInstance().Query<Productos>(sql));

        }



        public int ExistenProductosBloqueados()
        {
            try
            {

                var item = SqliteManager.GetInstance().Query<ProductosTemp>("Select Sum(Proid) as Cantidad From ClientesProductosBloqueos Where CliCodigo = ? ", new string[] { Arguments.Values.CurrentClient.CliCodigo }).FirstOrDefault();
                if (item != null)
                {
                    return (int)item.Cantidad;
                }


            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return 0;

        }

        public List<ProductosTemp> GetProductosMasInventario(int titId)
        {
            return SqliteManager.GetInstance().Query<ProductosTemp>("select ProID, sum(Cantidad) as Cantidad, sum(CantidadDetalle) as CantidadDetalle, " +
                "sum(InvCantidad) as InvCantidad, sum(InvCantidadDetalle) as InvCantidadDetalle from (" +
                "select p.ProID as ProID, Cantidad, CantidadDetalle, 0 as InvCantidad, 0 as InvCantidadDetalle " +
                "from ProductosTemp p where TitID = " + titId.ToString() + " " +
                "union " +
                "select i.ProID as ProID, 0 as Cantidad, 0 as CantidadDetalle, i.invCantidad as InvCantidad, i.InvCantidadDetalle as InvCantidadDetalle " +
                "from Inventarios i where trim(i.RepCodigo) = ? ) tabla group by ProID ", new string[] { Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public double ConvertirCajasAunidadesByCarga(double cajasInv, double unidadesInv, double proUnidades, double Cajasfisicas, double unidadesFisicas)
        {
            var TotalUnidadesInv = unidadesInv + (cajasInv * proUnidades);
            var TotalUnidadesFisicas = unidadesFisicas + (Cajasfisicas * proUnidades);

            return Math.Abs(TotalUnidadesFisicas) - Math.Abs(TotalUnidadesInv);
        }

        public double ConvertirCajasAunidades(double cajasInv, double unidadesInv, double proUnidades, double Cajasfisicas, double unidadesFisicas)
        {
            var TotalUnidadesInv = unidadesInv + (cajasInv * proUnidades);
            var TotalUnidadesFisicas = unidadesFisicas + (Cajasfisicas * proUnidades);

            /* if (TotalUnidadesInv > 0) {
                 TotalUnidadesInv = TotalUnidadesInv - TotalUnidadesFisicas;
             if (TotalUnidadesInv > 0) {
                 TotalUnidadesInv -= TotalUnidadesFisicas;
             }

             else if (TotalUnidadesInv == 0)
             {*/
            {
                TotalUnidadesInv = TotalUnidadesFisicas - TotalUnidadesInv;
                //   }
            }

            return TotalUnidadesInv;
        }

        public double ConvertirUnidadesACajas(double Unidades, double proUnidades)
        {
            if (/*Unidades >= proUnidades || Unidades<0*/ Unidades != 0)
            {
                Unidades = Unidades / proUnidades;

            }

            return Unidades;
        }


        public bool ExitsProductoOfertaInTemp(int proid)
        {
            string query = "Select Proid from ProductosTemp where Proid = " + proid + " and IndicadorOferta = 1 ";

            var list = SqliteManager.GetInstance().Query<ProductosTemp>(query, new string[] { });

            if (list.Count > 0)
            {
                if ((int)list[0].ProID == proid)
                {
                    return true;
                }
            }

            return false;
        }

        /* public void DeleteProductoOfertaInTemp(int proid)
         {
             SqliteManager.GetInstance().Execute("Delete from ProductosTemp where ProID = "+ proid + " and IndicadorOferta = 1 ", new string[] { });
         }*/

        public int GetProUnidades(int proid)
        {
            string query = "Select ProUnidades from Productos where Proid = " + proid + "  ";

            var list = SqliteManager.GetInstance().Query<ProductosTemp>(query, new string[] { });

            return list[0].ProUnidades;

        }

        public void InsertProductosParaPruebas(int cantidad, out double seconds)
        {
            seconds = 0;

            bool useClientesPrecios = Arguments.Values.CurrentClient != null && (Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.VENTAS || Arguments.Values.CurrentModule == Modules.COTIZACIONES);

            var unidadesMedidasValidas = myParametro.GetParUnidadesMedidasVendedorUtiliza();

            SqliteManager.GetInstance().Execute("delete from ProductosTemp where TitID = " + ((int)Arguments.Values.CurrentModule));

            string query = "select 10 as CantidadPiezas, 0 as IndicadorPromocion, " + ((int)Arguments.Values.CurrentModule) + " as TitID, p.ProReferencia as ProReferencia, 1 as ShowCantidad, 0 as DesPorciento, 0 as IndicadorOferta, " +
                "ifnull(trim(P.ProCodigo), '') as ProCodigo, p.ProPrecio3 as ProPrecio3, " +
                "P.ProID as ProID, ifnull(trim(P.ProDescripcion), '') as Descripcion, " +
                "ifnull(ifnull(lpc.LipPrecio, l.LipPrecio), 0.0) as Precio, 10 as Cantidad, 0 as CantidadDetalle, 0 as CantidadDetalleR, " + (Arguments.Values.CurrentModule == Modules.COMPRAS || Arguments.Values.CurrentModule == Modules.INVFISICO || Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS || Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS || Arguments.Values.CurrentModule == Modules.TRASPASOS || Arguments.Values.CurrentModule == Modules.REQUISICIONINVENTARIO ? "0 as Selectivo" : "ifnull(ProSelectivo, 0) as Selectivo") +
                ", " + (Arguments.Values.CurrentModule == Modules.COMPRAS || Arguments.Values.CurrentModule == Modules.INVFISICO || Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS || Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS || Arguments.Values.CurrentModule == Modules.TRASPASOS || Arguments.Values.CurrentModule == Modules.REQUISICIONINVENTARIO ? "0 as AdValorem" : "ifnull(ProAdValorem, 0) as AdValorem") + ", " +
                 (Arguments.Values.CurrentModule == Modules.COMPRAS ? "0" : "ifnull(ProItbis, 0)") + " as Itbis, ifnull(ifnull(lpc.UnmCodigo, l.UnmCodigo), '') as UnmCodigo, ifnull(P.ProDescripcion1, '') as ProDescripcion1, ifnull(P.ProDescripcion2, '') as ProDescripcion2, " +
                 "ifnull(P.ProDescripcion3, '') as ProDescripcion3, ifnull(p.ProDatos1, '') as ProDatos1, ifnull(p.ProDatos2, '') as ProDatos2, " +
                 "0 as Descuento, p.ProIndicadorDetalle as IndicadorDetalle, ifnull(p.ProUnidades, 0) as ProUnidades " +
                 "from Productos p " +
                 "left join ListaPrecios l on " + (!string.IsNullOrWhiteSpace(unidadesMedidasValidas) ? "ifnull(lower(l.UnmCodigo), '') in (" + unidadesMedidasValidas + ") and " : "") + " l.ProID = P.ProID and l.LipCodigo = '" + Arguments.Values.CurrentClient.LiPCodigo + "' " +
                 (useClientesPrecios ? "left join ListaPreciosClientes lpc on " + (!string.IsNullOrWhiteSpace(unidadesMedidasValidas) ? " ifnull(lower(lpc.UnmCodigo), '') in (" + unidadesMedidasValidas + ") and " : "") + " lpc.ProID = P.ProID and lpc.CliID = " + Arguments.Values.CurrentClient.CliID + " " : " ") +
                 "where p.ProID is not null order by p.ProDescripcion limit " + cantidad;


            var list = SqliteManager.GetInstance().Query<ProductosTemp>(query, new string[] { });


            var start = DateTime.Now;

            foreach (var prod in list)
            {
                prod.rowguid = Guid.NewGuid().ToString();
            }

            var end = DateTime.Now;

            seconds = (start - end).TotalSeconds;

            SqliteManager.GetInstance().InsertAll(list);



        }

        public bool ValidarProductoinTempSinPieza()
        {
            string query = "Select CantidadPiezas from ProductosTemp where CantidadPiezas = null or CantidadPiezas = 0 ";

            var list = SqliteManager.GetInstance().Query<ProductosTemp>(query, new string[] { });

            return list.Count > 0 ? true : false;
        }

        public bool HayOfertasConLotes()
        {
            return Arguments.Values.CurrentModule == Modules.VENTAS
                && SqliteManager.GetInstance().Query<ProductosTemp>("select t.ProID from ProductosTemp t " +
                "inner join Productos p on p.ProID = t.ProID and ifnull(p.ProDatos3, '') like '%L%' " +
                "where ifnull(t.IndicadorOferta, 0) = 1 limit 1", new string[] { }).Count > 0;
        }

        public Productos GetById(int proId)
        {
            var query = "select ProID, ProDatos3, ProDescripcion, ProCodigo from Productos where ProID = " + proId.ToString();

            var list = SqliteManager.GetInstance().Query<Productos>(query, new string[] { });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public ProductosTemp GetTotalQuintalesInTemp()
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<ProductosTemp>("select SUM((case when ifnull(CantidadDetalle, 0) > 0 then (CantidadDetalle / p.ProUnidades) + ifnull(Cantidad, 0.0) else Cantidad end) * (p.ProPeso / 100.0)) as Cantidad from ProductosTemp t " +
                    "inner join Productos p on p.ProID = t.ProID where t.Cantidad > 0 or t.CantidadDetalle > 0", new string[] { });

                if (list != null && list.Count > 0)
                {
                    var total = list[0];
                    total.Cantidad = list[0].Cantidad;
                    return total;
                    //return (int)list[0].Cantidad;
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return null;
        }

        public bool QuantityIsValidForDelivery(int proId, int posicion, double cantidadaAgregar, string lote = null, double cantidadHolgura = 0)
        {
            var list = SqliteManager.GetInstance().Query<ProductosTemp>("select CantidadEntrega, sum(Cantidad) as Cantidad from " +
                "ProductosTemp where ProID = ? " + (!string.IsNullOrWhiteSpace(lote) ? " and ifnull(upper(Lote), '') != '" + lote.ToUpper() + "'" : "") + " " +
                "and Posicion = ? and case when ifnull(ProDatos3, '') like '%L%' then ifnull(Lote, '') != '' else 1=1 end " +
                "group by ProID, Posicion, CantidadEntrega",
                new string[] { proId.ToString(), posicion.ToString(), cantidadaAgregar.ToString() });



            if (list != null && list.Count > 0)
            {
                return list[0].CantidadEntrega * cantidadHolgura >= (list[0].Cantidad + cantidadaAgregar);
            }

            return true;
        }

        public bool HayProductosSinLoteAgregados()
        {
            var list = SqliteManager.GetInstance().Query<ProductosTemp>("select ProID from ProductosTemp where " +
                "ifnull(ProDatos3, '') like '%L%' and Cantidad > 0 and ifnull(Lote, '') = '' and TitID = ? limit 1",
                new string[] { ((int)Arguments.Values.CurrentModule).ToString() });

            return list != null && list.Count > 0;
        }

        public void InsertarOfertaDevolucion()
        {
            DeleteOfertaInTemp((int)Arguments.Values.CurrentModule);

            var conOfertas = SqliteManager.GetInstance().Query<ProductosTemp>("select * from ProductosTemp where TitID = ? " +
                "and ifnull(CantidadOferta, 0) > 0 and ifnull(IndicadorOferta, 0) = 0", new string[] { ((int)Arguments.Values.CurrentModule).ToString() });

            foreach (var prod in conOfertas)
            {
                prod.rowguid = Guid.NewGuid().ToString();
                prod.Cantidad = prod.CantidadOferta;
                prod.CantidadDetalle = 0;
                prod.IndicadorOferta = true;

                if (myParametro.GetParOfertasManualesConDescuento100Porciento())
                {
                    prod.DesPorciento = 100;
                    prod.Descuento = prod.Precio;
                }
                else
                {
                    prod.Precio = 0;
                    prod.DesPorciento = 0;
                    prod.Descuento = 0;
                }

            }

            SqliteManager.GetInstance().InsertAll(conOfertas);
        }

        public int GetCantidadProductosAgregados(int titId)
        {
            var list = SqliteManager.GetInstance().Query<ProductosTemp>("select count(ProID) + 1 as Cantidad from ProductosTemp where TitID = ?", new string[] { titId.ToString() });

            if (list != null && list.Count > 0)
            {
                return (int)list[0].Cantidad;
            }

            return 0;
        }

        public int GetCantidadProductosAgregadosSinOfertas(int titId)
        {
            var list = SqliteManager.GetInstance().Query<ProductosTemp>("select count(ProID) + 1 as Cantidad from ProductosTemp where IndicadorOferta = 0 and TitID = ?", new string[] { titId.ToString() });

            if (list != null && list.Count > 0)
            {
                return (int)list[0].Cantidad;
            }

            return 0;
        }

        public string CondicionPagoInTemp()
        {
            var list = SqliteManager.GetInstance().Query<ProductosTemp>("select ConID from ProductosTemp limit 1", new object[] { });

            return list.Count > 0 ? list[0].ConID.ToString() : "";
        }

        public List<Productos> GetProductsForFilter(bool withInventory = false)
        {
            var query = "select ifnull(ProDescripcion, '') as ProDescripcion, p.ProID as ProID, p.ProCodigo as ProCodigo from Productos p ";

            if (withInventory)
            {
                query += " inner join Inventarios i on i.ProID = p.ProID and trim(i.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ifnull(i.invCantidad, 0) > 0 ";
            }

            query += " order by p.ProDescripcion";

            return SqliteManager.GetInstance().Query<Productos>(query, new string[] { });
        }

        public int GetProIDByColorAndSize(string proCodigo, string color, string size)
        {
            var raw = proCodigo.Split('-');

            if (raw == null || raw.Length == 0)
            {
                return -1;
            }

            var code = raw[0] + "-" + size + "-" + color;

            var list = SqliteManager.GetInstance().Query<Productos>("select ProID from Productos where ProCodigo = ? ", new string[] { code });

            if (list != null && list.Count > 0)
            {
                return list[0].ProID;
            }

            return -1;
        }

        public bool IsAddedInTempByColor(string reference, string colorRef, double quantity)
        {
            var list = SqliteManager.GetInstance().Query<ProductosTemp>("select ProCodigo from ProductosTemp where " +
                "ProCodigo like '" + reference + "-%-" + colorRef + "' and ifnull(Cantidad, 0.0) = " + quantity.ToString(), new string[] { });

            return list != null && list.Count > 0;
        }

        public List<string> GetProductosSizesAndColorByReferenceSplit(string reference, bool color = false, string colorRef = null)
        {
            var where = "ProCodigo like '" + reference + "-%'";

            if (!string.IsNullOrWhiteSpace(colorRef))
            {
                where = "ProCodigo like '" + reference + "-%-" + colorRef + "'";
            }

            var list = SqliteManager.GetInstance().Query<Productos>("select ProCodigo from Productos where " + where, new string[] { });

            var result = new List<string>();

            foreach (var prod in list)
            {
                var raw = prod.ProCodigo.Split('-');

                if (raw != null && raw.Length > 2)
                {
                    if (color)
                    {
                        result.Add(raw[2]);
                    }
                    else
                    {
                        result.Add(raw[1]);
                    }
                }
            }

            return result;
        }

        public void SetCondicionPagoInTemp(int CondicionPago)
        {
            SqliteManager.GetInstance().Execute("update ProductosTemp set ConID = " + CondicionPago, new string[] { });
        }

        public void ActualizarPreciosInTemp(string rowguid, double precio, double lipPrecioMinimo)
        {
            SqliteManager.GetInstance().Execute("update ProductosTemp set Precio = ?, PrecioTemp = ?, LipPrecioMinimo = ? " +
                "where rowguid = '" + rowguid + "'", new string[] { precio.ToString(), precio.ToString(), lipPrecioMinimo.ToString() });
        }

        public void ActualizarDescuentosForExInTemp(DescuentosRecargos descuento = null)
        {

            if (descuento == null)
            {
                SqliteManager.GetInstance().Execute("update ProductosTemp set DesPorciento = 0, Descuento = 0 ", new string[] { });
            }
            else if (descuento.ProID != 0 && string.IsNullOrWhiteSpace(descuento.GrpCodigo))
            {
                SqliteManager.GetInstance().Execute("update ProductosTemp set DesPorciento = 0, Descuento = 0 " +
               "where ProID = '" + descuento.ProID.ToString() + "'", new string[] { });
            }
            else
            {
                SqliteManager.GetInstance().Execute("update ProductosTemp set DesPorciento = 0, Descuento = 0 " +
                "where  ProID in (select ProID from GrupoProductosDetalle  where trim(upper(GrpCodigo)) = '" + descuento.GrpCodigo.Trim() + "' ) ",
                new string[] { });

            }
        }

        public ProductosTemp GetProductInTempByProCodigo(string proCodigo)
        {
            var list = SqliteManager.GetInstance().Query<ProductosTemp>("select ProID, ProCodigo, Cantidad, IndicadorDocena from ProductosTemp where ProCodigo = '" + proCodigo + "'",
                new string[] { });

            if (list != null && list.Count() > 0)
            {
                return list[0];
            }

            return null;
        }
        public List<ProductosTemp> GetProductInTempByProId(int proid)
        {
            var list = SqliteManager.GetInstance().Query<ProductosTemp>("SELECT IndicadorDocena, Cantidad, p.UnmCodigo from ProductosTemp p inner join ListaPrecios lp on lp.Proid = p.proid where p.proid = " + proid + "",
                new string[] { });

            if (list != null && list.Count() > 0)
            {
                return list;
            }
            return new List<ProductosTemp>();
        }
        public void ActualizarPreciosMinInTemp()
        {
            var list = SqliteManager.GetInstance().Query<ProductosTemp>("select ProID,UnmCodigo,OfeID from ProductosTemp", new string[] { });

            foreach (var prod in list)
            {
                var lipCodigo = myParametro.GetParSectores() >= 2 && Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.LipCodigo : Arguments.Values.CurrentClient != null ? Arguments.Values.CurrentClient.LiPCodigo : "Default";
                double precio = new DS_ListaPrecios().GetLipPrecioCompl(prod.ProID, lipCodigo, prod.UnmCodigo);
                double precioMinimo = new DS_ListaPrecios().GetPrecioMinimo(prod.ProID, lipCodigo, prod.UnmCodigo);
                double lipdescuento = new DS_ListaPrecios().GetLipDescuento(prod.ProID, lipCodigo, prod.UnmCodigo);
                var cantidad = ExistsProductoAgregadoPorOfertaForEdit(prod.ProID, (int)Arguments.Values.CurrentModule, prod.OfeID > 0 ? prod.OfeID : -1);

                double result = precio * (cantidad != null ? cantidad.Cantidad : 0);

                SqliteManager.GetInstance().Execute("update ProductosTemp set PrecioTemp = ?, LipPrecioMinimo = ?,ValorOfertaManual = ?, LipDescuento = ? " +
                "where proid = '" + prod.ProID + "'", new string[] { precio.ToString(), precioMinimo.ToString(), result.ToString(), lipdescuento.ToString() });

            }

            var list1 = SqliteManager.GetInstance().Query<ProductosTemp>("select ProID,ValorOfertaManual,UnmCodigo,OfeID from ProductosTemp", new string[] { });
            foreach (var prod in list1)
            {
                if (!prod.IndicadorOferta && prod.ValorOfertaManual == 0)
                {
                    var product = list1.Where(p => p.OfeID == prod.ProID && p.ProID != prod.ProID).FirstOrDefault();

                    if (product != null)
                    {
                        SqliteManager.GetInstance().Execute("update ProductosTemp set ValorOfertaManual = ? " +
                        "where proid = '" + prod.ProID + "'", new string[] { product.ValorOfertaManual.ToString() });
                    }
                }
            }
        }

        public string GetProductCodigoAndDescByProId(int proid)
        {
            var Producto = SqliteManager.GetInstance().Query<ProductosTemp>("SELECT ProDescripcion,ProCodigo from Productos where proid = " + proid + "",
                new string[] { }).FirstOrDefault();

            if (Producto != null)
            {
                return Producto.ProCodigoDescripcion;
            }
            return " ";
        }

        public int GetCantidadProductosAgregadosConLote(int proid, string lote)
        {
            var list = SqliteManager.GetInstance().Query<ProductosTemp>("select SUM(Cantidad) as Cantidad from ProductosTemp where ProID=? and Lote=? ", new string[] { proid.ToString(), lote });

            if (list != null && list.Count > 0)
            {
                return (int)list[0].Cantidad;
            }

            return 0;
        }

        public List<KV> GetAttributosProducto(int proId, bool atributos2 = false)
        {
            var query = "select d.atrDescripcion as Value, d.rowguid as Key " +
                "from ProductosAtributos a " +
                "inner join AtributosProductosDetalle d on d.atrRowguid = a.Atr" + (atributos2 ? "2" : "1") + "Rowguid " +
                "where a.ProID = ? order by d.atrDescripcion";

            var list = SqliteManager.GetInstance().Query<KV>(query, new string[] { proId.ToString() });

            return list;
        }
    }
}
