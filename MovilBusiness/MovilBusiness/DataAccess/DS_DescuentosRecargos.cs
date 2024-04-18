using MovilBusiness.Configuration;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MovilBusiness.DataAccess
{
    public class DS_DescuentosRecargos : DS_Controller
    {

        List<ProductosTemp> PedDescuentos { get; set; }

        public List<DescuentosRecargos> GetDescuentosDisponibles(int titId, int cliId, int proId = -1, int conId = -1, bool forProductsInTemp = false, bool forDescManualMaximo = false, EntregasRepartidorTransacciones entrega = null, int titIdForDesc = -1, bool isConsultaGeneralByDescuento = false)
        {
            try
            {
                var where = "";

                if (proId != -1)
                {
                    where = " AND ((NOT(dv.GrpCodigo IS NULL OR ifnull(dv.GrpCodigo, '') = '' or ifnull(dv.GrpCodigo, '0') = '0') AND " + proId.ToString() + " " +
                        "IN (SELECT ProID FROM GrupoProductosDetalle WHERE GrpCodigo = dv.GrpCodigo)) OR((dv.ProID = " + proId.ToString() + " OR IFNULL(dv.ProID, 0) = 0) AND (IFNULL(dv.GrpCodigo, '') = '' or ifnull(dv.GrpCodigo, '0') = '0'))  ) ";
                }

                if (Arguments.Values.CurrentSector != null)
                {
                    where += " AND (dv.SecCodigos like '%{" + Arguments.Values.CurrentSector.SecCodigo + "}%' or ifnull(dv.SecCodigos, '') = '' )";
                }

                if (conId != -1)
                {
                    where += " and (ifnull(dv.ConID, 0) = 0 or dv.ConID = " + conId.ToString() + ") ";
                }

                if (forProductsInTemp)
                {
                    where += " and ((ifnull(dv.ProID, 0) = 0 and (ifnull(dv.GrpCodigo, '') = '' or ifnull(dv.GrpCodigo, '0') = '0')) or ((NOT(dv.GrpCodigo IS NULL OR ifnull(dv.GrpCodigo, '') = '' or ifnull(dv.GrpCodigo, '0') = '0') and dv.GrpCodigo in (select GrpCodigo from GrupoProductosDetalle g inner join ProductosTemp t on t.ProID = g.ProID and t.TitID = " + titId.ToString() + " and ifnull(IndicadorOferta, 0) = 0 where g.GrpCodigo = dv.GrpCodigo)) or (ifnull(dv.ProID, 0) in (select distinct ProID from ProductosTemp where TitID = " + titId.ToString() + " and ifnull(IndicadorOferta, 0) = 0) or ifnull(dv.ProID, 0) = 0))) ";
                }

                if (forDescManualMaximo)
                {
                    where += " and dv.DoROperacion = 3 ";
                }
                else if (!myParametro.GetParMostrarDescuentosMaximos())
                {
                    where += " and dv.DoROperacion <> 3 ";
                }

                if(titIdForDesc != -1)
                {
                    where += $" and dv.TitId = {titIdForDesc} ";
                }

                string query = $"select distinct case when ifnull(dv.ConID, 0) = 0 then 'Todas' else ifnull(c.ConDescripcion, 'Todas') end as ConIdDescripcion " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? " , dv.UnmCodigo " : "") + " ,  dv.DesID as DesID, dv.DesID || ' - ' || ifnull(dv.DesDescripcion, '') as DesDescripcion, ifnull(replace( strftime('%d-%m-%Y', SUBSTR(DesFechaInicio,1,10)),' ','' ), '') as DesFechaInicio, " +
                    "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(DesFechaFin,1,10)),' ','' ), '') as DesFechaFin, u.Descripcion as DesTipoDescripcion, DesTipo, DesMetodo, GrpCodigo, ProID, DoROperacion, " +
                    " " + (!isConsultaGeneralByDescuento ? " 0 as IsConsultaGeneral " : " case when (ifnull(dv.GrcCodigo,0) = 0 or dv.GrcCodigo = '') and ifnull(dv.CliID,0) = 0 then 0 else 1 end as IsConsultaGeneral ") + " , ifnull(dv.CliID,0) as CliID, ifnull(dv.GrcCodigo,0) as GrcCodigo from DescuentosRecargos dv " +
                    "left join UsosMultiples u on ltrim(rtrim(upper(u.CodigoGrupo))) = 'DESTIPO' and u.CodigoUso = dv.DesTipo " +
                    "left join CondicionesPago c on c.ConId = dv.ConID " +
                    "where " +
                    "STRFTIME('%Y-%m-%d',  " + (entrega != null ? "'" + entrega.VenFecha + "'" : " DATETIME('NOW', 'localtime')") + ")" +
                    "BETWEEN STRFTIME('%Y-%m-%d', dv.DesFechaInicio) " + (myParametro.GetDescuentoConDia() ? " AND STRFTIME('%Y-%m-%d', dv.DesFechaFin, '+1 day') " : " AND STRFTIME('%Y-%m-%d', dv.DesFechaFin) ") + "	" +
                    (!isConsultaGeneralByDescuento ? $@" and (IFNULL(dv.DesCaracteristicas,'') Not Like '%N%' or (IFNULL(dv.DesCaracteristicas,'') Like '%N%' and not exists(select 1 from ClientesProductosVendidos where   CliID = {cliId} and Proid = {proId}  ))) 
                    and (((IFNULL(dv.CliID, 0) = 0 and IFNULL(dv.GrcCodigo, '') <> '' and ifnull(dv.GrcCodigo, '0') <> '0' 
                    AND {cliId} IN(SELECT CliID FROM GrupoClientesDetalle WHERE GrcCodigo = dv.GrcCodigo)) 
                    OR (ifnull(dv.CliID, 0) = 0 and (ifnull(grcCodigo, '')='' or ifnull(grcCodigo, '0') = '0'))) 
                    OR ((ifnull(grcCodigo, '') = '' or ifnull(grcCodigo, '0') = '0') and dv.CliID = {cliId})) " : " ") +
                    where + " order by " + (forProductsInTemp ? "dv.DesOrden ASC" : "dv.DesOrden DESC, dv.DesID DESC");

                return SqliteManager.GetInstance().Query<DescuentosRecargos>(query);

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return new List<DescuentosRecargos>();
            }
        }


        public List<DescuentosRecargosDetalle> GetDetalles(int desId, int proIdDetalleGrp = -1, double quantityRange = -1)
        {
            var where = "";

            if (proIdDetalleGrp != -1)
            {
                where = " and " + proIdDetalleGrp.ToString() + " in (select ProID from GrupoProductosDetalle where GrpCodigo = d.grpCodigo and ProID = " + proIdDetalleGrp.ToString() + ") ";
            }

            if(quantityRange > 0)
            {
                where += " and " + quantityRange + " between DRDCantidadInicial and DRDCantidadFinal";
            }

            var query = "select DRDPorciento, DRDValor, DRDCantidadInicial, DRDCantidadFinal, DRDMontoInicial, DRDMontoFinal, DesID, grpCodigo " +
                " " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? " ,  IFNULL((select UnmCodigo from DescuentosRecargos dr where dr.DesID=d.DesID limit 1),'') as UnmCodigo " : "") + " " +
                " from DescuentosRecargosDetalle d where DesID = ? " + where + " order by DRDCantidadInicial asc";
            return SqliteManager.GetInstance().Query<DescuentosRecargosDetalle>(query, new string[] { desId.ToString() });
        }

        public void CalcularDescuentosForProductsInTemp(int cliId, int conId, int titId, EntregasRepartidorTransacciones entrega = null)
        {
            if (!myParametro.GetParPedidosDescuentoManual())
            {
                var descuentos = GetDescuentosDisponibles(titId, cliId, -1, conId, true, entrega: entrega);
                string DesIDAplicados = "";
                if (myParametro.GetProductosEnInventario() && descuentos != null && descuentos.Count > 0)
                {
                    if (!myParametro.GetDescuentoxPrecioNegociado() && !myParametro.GetParPedDescLip())
                    {
                        SqliteManager.GetInstance().Execute("update ProductosTemp set Descuento = 0, DesPorciento = 0", new string[] { });
                    }

                }

                foreach (var descuento in descuentos)
                {
                    foreach (var detalle in GetDetalles(descuento.DesID))
                    {
                        var query = CalcularDescuentoNormalByTipo(detalle.DRDPorciento, detalle.DRDValor, descuento.DesTipo, descuento.DoROperacion, descuento.DesTipo == 5 ? descuento : null, detalle.DRDCantidadInicial, detalle.DRDCantidadFinal, desmetodo: descuento.DesMetodo);

                        if (string.IsNullOrWhiteSpace(query))
                        {
                            continue;
                        }

                        query = "update ProductosTemp SET " + query + " where ifnull(IndicadorOferta, 0) = 0 and TitID = " + titId.ToString() + " ";

                        var where = "";

                        if (descuento.ProID == 0 && !string.IsNullOrWhiteSpace(descuento.GrpCodigo))
                        {
                            where = " and ProID in (select ProID from GrupoProductosDetalle where GrpCodigo = '" + descuento.GrpCodigo + "') ";
                        }
                        else if (descuento.ProID != 0 && string.IsNullOrEmpty(descuento.GrpCodigo))
                        {
                            where = " and ProID = " + descuento.ProID.ToString();
                        }

                        if(DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida())
                        {

                            if (!string.IsNullOrEmpty(descuento.UnmCodigo))
                            {
                                where += " and Unmcodigo = '" + descuento.UnmCodigo + "' ";
                            }
                        }


                        switch (descuento.DesMetodo)
                        {
                            case 1: //normal
                                var myProduc = new DS_Productos();
                                var num = 0.0;

                                if (descuento.ProID != 0 && string.IsNullOrWhiteSpace(descuento.GrpCodigo))
                                {
                                    num = myProduc.GetCantidadTotalInTemp(titId, false, false, descuento.ProID,  UnmCodigo : DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? descuento.UnmCodigo : "");
                                }
                                else if (!string.IsNullOrWhiteSpace(descuento.GrpCodigo))
                                {
                                    num = GetCantidadTotalInTempByGrpCodigo(descuento.GrpCodigo, titId, UnmCodigo: DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? descuento.UnmCodigo : "");
                                }

                                if (num >= detalle.DRDCantidadInicial && num <= detalle.DRDCantidadFinal)
                                {
                                    SqliteManager.GetInstance().Execute(query + where);
                                }
                                else if (descuento.ProID == 0 && string.IsNullOrWhiteSpace(descuento.GrpCodigo))
                                {
                                    SqliteManager.GetInstance().Execute(query);
                                }

                                break;
                            case 2:// mancomunado por monto vendido
                                where += " and exists (select '1' from ProductosTemp where " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() && !string.IsNullOrWhiteSpace(descuento.UnmCodigo) ? " UnmCodigo = '" + descuento.UnmCodigo + "' and " : "") + " TitID = " + titId.ToString() + " and ProID in (select ProID from GrupoProductosDetalle where GrpCodigo = '" + descuento.GrpCodigo + "') group by '1' having sum(Precio * Cantidad) between " + detalle.DRDMontoInicial.ToString() + " and " + detalle.DRDMontoFinal.ToString() + " ) ";
                                SqliteManager.GetInstance().Execute(query + where);
                                break;
                            case 3: //mancomunado por cantidad
                                where += " and exists (select '1' from ProductosTemp where " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() && !string.IsNullOrWhiteSpace(descuento.UnmCodigo) ? " UnmCodigo = '" + descuento.UnmCodigo + "' and " : "") + " TitID = " + titId.ToString() + " and ProID in (select ProID from GrupoProductosDetalle where GrpCodigo = '" + descuento.GrpCodigo + "') group by '1' having sum(Cantidad) between " + detalle.DRDCantidadInicial.ToString() + " and " + detalle.DRDCantidadFinal.ToString() + " ) ";
                                SqliteManager.GetInstance().Execute(query + where);
                                break;
                            case 4: //monto vendido
                                where += " and ProID in (select ProID from ProductosTemp where " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() && !string.IsNullOrWhiteSpace(descuento.UnmCodigo) ? " UnmCodigo = '" + descuento.UnmCodigo + "' and " : "") + " TitID = " + titId.ToString() + " and IndicadorOferta = 0 group by ProID having sum(Precio * Cantidad) between " + detalle.DRDMontoInicial.ToString() + " and " + detalle.DRDMontoFinal.ToString() + ")";
                                SqliteManager.GetInstance().Execute(query + where);
                                break;
                            case 5: //Grupo de Productos Detalle
                                var cantidadTotalGrp = GetCantidadTotalInTempByGrpCodigo(descuento.GrpCodigo, titId, UnmCodigo: descuento.UnmCodigo);
                                where += " and ProID in (select proID from GrupoProductosDetalle where GrpCodigo = '" + detalle.grpCodigo + "' group by ProID having " + cantidadTotalGrp.ToString() + " between " + detalle.DRDCantidadInicial + " and " + detalle.DRDCantidadFinal + ") ";
                                SqliteManager.GetInstance().Execute(query + where);
                                break;
                            case 6: //mancomunado por cantidad de cajas
                                where += @" and exists (select '1' from ProductosTemp t inner join Productos p on p.ProID = t.ProID where " 
                                      + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() && 
                                      !string.IsNullOrWhiteSpace(descuento.UnmCodigo) ? " t.UnmCodigo = '" + descuento.UnmCodigo + "' and " : "") 
                                      + " t.TitID = " + titId.ToString() + " and t.ProID in (select ProID from GrupoProductosDetalle where GrpCodigo = '" +
                                      descuento.GrpCodigo + "') group by '1' having sum(round(t.Cantidad / p.ProUnidades, 2)) between " 
                                      + detalle.DRDCantidadInicial.ToString() + " and " + detalle.DRDCantidadFinal.ToString() + " ) ";
                                SqliteManager.GetInstance().Execute(query + where);
                                break;
                            case 7: 
                                 myProduc = new DS_Productos();
                                 num = 0.0;

                                if (descuento.ProID != 0 && string.IsNullOrWhiteSpace(descuento.GrpCodigo))
                                {
                                    num = myProduc.GetCantidadTotalInTemp(titId, false, false, descuento.ProID, UnmCodigo: DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? descuento.UnmCodigo : "");
                                }
                                else if (!string.IsNullOrWhiteSpace(descuento.GrpCodigo))
                                {
                                    num = GetCantidadTotalInTempByGrpCodigo(descuento.GrpCodigo, titId, UnmCodigo: DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? descuento.UnmCodigo : "");
                                }

                                if (descuento.ProID == 0 && string.IsNullOrWhiteSpace(descuento.GrpCodigo))
                                {
                                   SqliteManager.GetInstance().Execute(query);
                                }else
                                {
                                   SqliteManager.GetInstance().Execute(query + where);
                                }

                                break;
                            case 8: //Individual por cantidad de cajas
                                //where += @" and exists (select '1' from ProductosTemp t inner join Productos p on p.ProID = t.ProID where "
                                //      + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() &&
                                //      !string.IsNullOrWhiteSpace(descuento.UnmCodigo) ? " t.UnmCodigo = '" + descuento.UnmCodigo + "' and " : "")
                                //      + " t.TitID = " + titId.ToString() + " and t.ProID in (select ProID from GrupoProductosDetalle where GrpCodigo = '" +
                                //      descuento.GrpCodigo + "') and round(t.Cantidad / p.ProUnidades, 2) between "
                                //      + detalle.DRDCantidadInicial.ToString() + " and " + detalle.DRDCantidadFinal.ToString() + " ) ";
                                where = $@" and (ProID in
                                     (select ProID from GrupoProductosDetalle where GrpCodigo = '{descuento.GrpCodigo}') 
                                     or ProID in ({descuento.ProID}))
                                     and (cantidad / ProUnidades) between {detalle.DRDCantidadInicial} and {detalle.DRDCantidadFinal}
                                     {(DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() 
                                     && !string.IsNullOrWhiteSpace(descuento.UnmCodigo) ? " and UnmCodigo = '" + descuento.UnmCodigo + "' " : "")}
                                     and TitID = {titId}";

                                SqliteManager.GetInstance().Execute(query + where);
                                break;
                            default:
                                 continue;
                        }


                            var DescuentoMaximo = SqliteManager.GetInstance().Query<ProductosTemp>("Select (( Precio * (select ProDescuentoMaximo from Productos where ProID = ProductosTemp.ProID)) / 100.0) as Descuento,(DesPorciento-(select ProDescuentoMaximo from Productos where ProID = ProductosTemp.ProID)) as DesPorciento from " +
                                         " ProductosTemp Where " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() &&  !string.IsNullOrWhiteSpace(descuento.UnmCodigo) ? " UnmCodigo = '" + descuento.UnmCodigo + "' and " : "") + " TitID = " + titId.ToString() + " and exists(select ProID from Productos where ProID = ProductosTemp.ProID and ifnull(ProductosTemp.DesPorciento,0.0) > ifnull(ProDescuentoMaximo, 0.0)) " +
                                         " and ifnull(IndicadorOferta, 0) = 0 and ifnull(Descuento, 0.0) > 0.0 ");


                            var PedDescuento = SqliteManager.GetInstance().Query<ProductosTemp>("Select Descuento,Precio,ProID from ProductosTemp where " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() && !string.IsNullOrWhiteSpace(descuento.UnmCodigo) ? " UnmCodigo = '" + descuento.UnmCodigo + "' and " : "") + " TitID = " + titId.ToString() + "" +
                                                                                                " and ProID in (select ProID from GrupoProductosDetalle where GrpCodigo = '" + descuento.GrpCodigo + "') " +
                                                                                                " or ProID = " + descuento.ProID.ToString());

                            if(DescuentoMaximo.Count < 1)
                            {
                                if (descuento.ProID != 0 || !string.IsNullOrWhiteSpace(descuento.GrpCodigo))
                                {
                                    foreach (var PedDes in PedDescuento)
                                    {
                                        ProductosTemp ped = null;
                                        if (PedDescuentos != null)
                                        {
                                            ped = PedDescuentos.Where(p => p.ProID == PedDes.ProID).FirstOrDefault();
                                        }


                                        var PedDesc = new PedidosDescuentos
                                        {
                                            DesID = descuento.DesID,
                                            DesEscalon = descuento.DesTipo == 3 ? 1 : 0,
                                            PedDescuento = descuento.DesTipo == 3 || descuento.DesTipo == 5 ? (detalle.DRDPorciento * (PedDes.Precio - ped.Descuento)) / 100 : (detalle.DRDPorciento * PedDes.Precio) / 100,
                                            DesMetodo = descuento.DesMetodo,
                                            PedDesPorciento = detalle.DRDPorciento,
                                        };
                                        DesIDAplicados = JsonConvert.SerializeObject(PedDesc);

                                        where = " and ProID = " + PedDes.ProID;

                                        SqliteManager.GetInstance().Execute("Update  ProductosTemp Set DesIdsAplicados = ifnull(DesIdsAplicados, '') || '" + DesIDAplicados + "|' where " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() && !string.IsNullOrWhiteSpace(descuento.UnmCodigo) ? " UnmCodigo = '" + descuento.UnmCodigo + "' and " : "") + " ifnull(IndicadorOferta, 0) = 0  and  TitID = " + titId.ToString() + "  " + where + "");
                                    }
                                }
                                else
                                {
                                    var PedDescuentoprue = SqliteManager.GetInstance().Query<ProductosTemp>("Select Descuento,Precio,ProID from ProductosTemp where TitID = " + titId.ToString());
                                    foreach (var PedDes in PedDescuentoprue)
                                    {
                                        ProductosTemp ped = null;
                                        if (PedDescuentoprue != null)
                                        {
                                            ped = PedDescuentoprue.Where(p => p.ProID == PedDes.ProID).FirstOrDefault();
                                        }

                                        var PedDesc = new PedidosDescuentos
                                        {

                                            DesID = descuento.DesID,
                                            DesEscalon = descuento.DesTipo == 3 ? 1 : 0,             
                                            PedDescuento = descuento.DesTipo == 3 || descuento.DesTipo == 5 ? (detalle.DRDPorciento * (PedDes.Precio - ped.Descuento)) / 100 : (detalle.DRDPorciento * PedDes.Precio) / 100, 
                                            DesMetodo = descuento.DesMetodo,
                                            PedDesPorciento = detalle.DRDPorciento,
                                        };
                                        DesIDAplicados = JsonConvert.SerializeObject(PedDesc);

                                        where = " and ProID = " + PedDes.ProID;

                                        SqliteManager.GetInstance().Execute("Update  ProductosTemp Set DesIdsAplicados = ifnull(DesIdsAplicados, '') || '" + DesIDAplicados + "|' where " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() && !string.IsNullOrWhiteSpace(descuento.UnmCodigo) ? " UnmCodigo = '" + descuento.UnmCodigo + "' and " : "") + " ifnull(IndicadorOferta, 0) = 0 and TitID = " + titId.ToString() + "  " + where + "");
                                    }

                                    //Se realizaba asi anteriormente pero se cambio ya que no calculaba el descuento valor de cada producto
                                        //var PedDescuentoprue = SqliteManager.GetInstance().Query<ProductosTemp>("Select Descuento,Precio,ProID from ProductosTemp where TitID = " + titId.ToString());

                                        //var PedDesc = new PedidosDescuentos
                                        //{

                                        //    DesID = descuento.DesID,
                                        //    DesEscalon = descuento.DesTipo == 3 ? 1 : 0,             ///Pendiente Corregir 
                                        //    PedDescuento = PedDescuentoprue[0].Descuento, ///El Monto de descuento se esta guardando mal. Actualmente solo se necesita el monto del porciento
                                        //    DesMetodo = descuento.DesMetodo,
                                        //    PedDesPorciento = detalle.DRDPorciento,
                                        //};
                                        //DesIDAplicados = JsonConvert.SerializeObject(PedDesc);

                                        //SqliteManager.GetInstance().Execute("Update  ProductosTemp Set DesIdsAplicados = ifnull(DesIdsAplicados, '') || '" + DesIDAplicados + "|' where " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() && !string.IsNullOrWhiteSpace(descuento.UnmCodigo) ? " UnmCodigo = '" + descuento.UnmCodigo + "' and " : "") + " ifnull(IndicadorOferta, 0) = 0 and TitID = " + titId.ToString() + "  " + where + "");
                                }
                            }

                            if (myParametro.GetParPedidosDescuentoMaximo())
                            {
                                //Actualizando los descuentos
                                DescuentoMaximo = SqliteManager.GetInstance().Query<ProductosTemp>("Select (( Precio * (select ifnull(ProDescuentoMaximo, 0.0) from Productos where ProID = ProductosTemp.ProID)) / 100.0) as Descuento,(DesPorciento-(select ifnull(ProDescuentoMaximo, 0.0) from Productos where ProID = ProductosTemp.ProID)) as DesPorciento from " +
                                " ProductosTemp Where " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() && !string.IsNullOrWhiteSpace(descuento.UnmCodigo) ? " UnmCodigo = '" + descuento.UnmCodigo + "' and " : "") + " TitID = " + titId.ToString() + " and exists(select ProID from Productos where ProID = ProductosTemp.ProID and ifnull(ProductosTemp.DesPorciento,0.0) > ifnull(ProDescuentoMaximo, 0.0)) " +
                                " and ifnull(IndicadorOferta, 0) = 0 and ifnull(Descuento, 0.0) > 0.0 ");

                                int rowsaffecteds = SqliteManager.GetInstance().Execute("update ProductosTemp set Descuento = (( Precio * (select ifnull(ProDescuentoMaximo, 0.0) from Productos where ProID = ProductosTemp.ProID)) / 100.0), " +
                                    "DesPorciento = (select ifnull(ProDescuentoMaximo, 0.0) from Productos where ProID = ProductosTemp.ProID) where " +
                                    "exists(select ProID from Productos where ProID = ProductosTemp.ProID and ifnull(ProductosTemp.DesPorciento,0.0) > ifnull(ProDescuentoMaximo, 0.0)) " +
                                    "and ifnull(IndicadorOferta, 0) = 0 and ifnull(Descuento, 0.0) > 0.0 and ProductosTemp.TitID = " + titId.ToString());


                                if (rowsaffecteds > 0)
                                {

                                    var UpdateJson = new PedidosDescuentos
                                    {
                                        DesID = descuento.DesID,
                                        DesEscalon = descuento.DesTipo == 3 ? 1 : 0,
                                        PedDescuento = DescuentoMaximo[0].Descuento,
                                        DesMetodo = descuento.DesMetodo,
                                        PedDesPorciento = detalle.DRDPorciento - DescuentoMaximo[0].DesPorciento,
                                    };
                                    DesIDAplicados = JsonConvert.SerializeObject(UpdateJson);
                                    SqliteManager.GetInstance().Execute("Update  ProductosTemp Set DesIdsAplicados = ifnull(DesIdsAplicados, '') || '" + DesIDAplicados + "|' where " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() && !string.IsNullOrWhiteSpace(descuento.UnmCodigo) ? " UnmCodigo = '" + descuento.UnmCodigo + "' and " : "") + " TitID = " + titId.ToString() + " and ifnull(IndicadorOferta, 0) = 0  " + where + "");

                                }

                            }
                        }
                        Arguments.Values.CurrentCountDes = -1;
                }

                if (Arguments.Values.CurrentModule == Modules.PEDIDOS && myParametro.GetParPedidosDescuentosManuales() == 2)
                {
                    SqliteManager.GetInstance().Execute("update ProductosTemp set DesPorciento = ifnull(DesPorcientoManual, 0.0) where  ifnull(DesPorcientoManual, 0.0) > 0.0 and ifnull(IndicadorOferta, 0) = 0 and  TitID = ? ", new string[] { titId.ToString() });
                    SqliteManager.GetInstance().Execute("update ProductosTemp set Descuento = (ifnull(DesPorciento, 0.0) * ifnull(Precio, 0.0)) / 100.0 where  ifnull(DesPorcientoManual, 0.0) > 0.0 and ifnull(IndicadorOferta, 0) = 0 and TitID = ? ", new string[] { titId.ToString() });
                }
                else if (myParametro.GetParPedidosDescuentoManual() || myParametro.GetParCotizacionesDescuentoManual()) //actualizando el descuento manual
                {
                    SqliteManager.GetInstance().Execute("update ProductosTemp set DesPorciento = ifnull(DesPorciento, 0.0) + ifnull(DesPorcientoManual, 0.0) where  ifnull(DesPorcientoManual, 0.0) > 0.0 and ifnull(IndicadorOferta, 0) = 0 and  TitID = ? ", new string[] { titId.ToString() });
                    SqliteManager.GetInstance().Execute("update ProductosTemp set Descuento = (ifnull(DesPorciento, 0.0) * ifnull(Precio, 0.0)) / 100.0 where  ifnull(DesPorcientoManual, 0.0) > 0.0 and ifnull(IndicadorOferta, 0) = 0 and TitID = ? ", new string[] { titId.ToString() });
                }
                else if (myParametro.GetParDevolucionesDescuentoManual())
                {
                    SqliteManager.GetInstance().Execute("update ProductosTemp set DesPorciento = ifnull(DesPorcientoManual, 0.0) where  ifnull(DesPorcientoManual, 0.0) > 0.0 and ifnull(IndicadorOferta, 0) = 0 and  TitID = ? ", new string[] { titId.ToString() });
                    SqliteManager.GetInstance().Execute("update ProductosTemp set Descuento = (ifnull(DesPorciento, 0.0) * ifnull(Precio, 0.0)) / 100.0 where  ifnull(DesPorcientoManual, 0.0) > 0.0 and ifnull(IndicadorOferta, 0) = 0 and TitID = ? ", new string[] { titId.ToString() });
                }
            }
        }

        public void ActualizarDescuentoGeneralManual(double descPorciento, int titId)
        {
            if (descPorciento > 0.0)
            {
                var where = "";

                if (myParametro.GetParPedidosBloquearProductosDescuentoGeneralConE())
                {
                    where += " and upper(ProDatos3) not like '%E%' ";
                }

                SqliteManager.GetInstance().Execute("update ProductosTemp set DesPorciento = ifnull(" + descPorciento.ToString() + ", 0.0) where 1=1 and ifnull(IndicadorOferta, 0) = 0 and  TitID = ? " + where, new string[] { titId.ToString() });
                SqliteManager.GetInstance().Execute("update ProductosTemp set Descuento = (ifnull(DesPorciento, 0.0) * ifnull(Precio, 0.0)) / 100.0 where 1=1 and ifnull(IndicadorOferta, 0) = 0 and TitID = ? " + where, new string[] { titId.ToString() });
            }
        }

        private double GetCantidadTotalInTempByGrpCodigo(string grpCodigo, int titId, string UnmCodigo = "")
        {
            var list = SqliteManager.GetInstance().Query<ProductosTemp>("select sum(cast(pt.cantidad as real) + cast( IFNULL(PT.CantidadDetalle, 0)as real) / cast(IFNULL(P.prounidades, 1) as real)) as Cantidad " +
                "from ProductosTemp pt " +
                "inner join Productos p on pt.proid = p.proid " +
                "where " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() && !string.IsNullOrWhiteSpace(UnmCodigo) ? " pt.UnmCodigo = '" + UnmCodigo + "' and " : "") + " pt.TitID = ? and pt.ProID in (select ProID from GrupoProductosDetalle " +
                "where trim(upper(GrpCodigo)) = '"+grpCodigo.Trim().ToUpper()+"' ) ",
                new string[] { titId.ToString() });

            if (list != null && list.Count > 0)
            {
                return list[0].Cantidad;
            }

            return -1;
        }

        private string CalcularDescuentoNormalByTipo(/*int desId,*/ double desPorciento, double desValor, int tipo, int operacion, DescuentosRecargos descuento,double inicial,double final, int desmetodo)
        {
            if (operacion == 2) //si es recargo
            {
                desPorciento = desPorciento * -1;
                desValor = desValor * -1;
            }

            var result = "";

            bool forValor = desPorciento == 0 && desValor > 0;
            if (desmetodo == 7)
            {
                switch (tipo)
                {
                    case 1: //exclusivo
                        result = $@"Descuento = (((((CAST(Cantidad / ProUnidades as int) * ProUnidades) * Precio) * {desPorciento}) 
                                 * (Precio - Descuento)) /100.0), DesPorciento = (((CAST(Cantidad / ProUnidades as int) 
                                 * ProUnidades) * Precio) * {desPorciento})";                       
                        break;
                    case 2: //acumulado
                        result = $@"Descuento = ifnull(Descuento, 0.0) + (((((CAST(Cantidad / ProUnidades as int) * ProUnidades) 
                                 * Precio) * '{desPorciento}') * (Precio - Descuento)) /100.0), DesPorciento = 
                                 ifnull(DesPorciento, 0.0) + (((CAST(Cantidad / ProUnidades as int) * ProUnidades) * Precio)
                                 * '{desPorciento}') ";
                        break;
                    case 3: //escalonado

                        PedDescuentos = SqliteManager.GetInstance().Query<ProductosTemp>("Select Descuento,ProID from ProductosTemp").ToList();

                        if (forValor)
                        {
                            result = $@" DesPorciento = DesPorciento + (((CAST(Cantidad / ProUnidades as int) * ProUnidades) * Precio) * {desPorciento}), 
                                    Descuento = Descuento + (((((CAST(Cantidad / ProUnidades as int) * ProUnidades) * Precio) * {desPorciento}) * (Precio - Descuento)) /100.0)";
                        }
                        else
                        {
                            var montoDescuento = $"(((((CAST(Cantidad / ProUnidades as int) * ProUnidades) * Precio) * {desPorciento}) * (Precio - Descuento)) /100.0) ";
                            result = $" DesPorciento = DesPorciento + (((CAST(Cantidad / ProUnidades as int) * ProUnidades) * Precio) * {desPorciento}), Descuento = Descuento + (((((INT(Cantidad / ProUnidades) * ProUnidades) * Precio) * '{desPorciento}') * (Precio - Descuento)) /100.0)"; ;
                        }
                        break;
                    case 5: //escalonado exclusivo

                        PedDescuentos = SqliteManager.GetInstance().Query<ProductosTemp>("Select Descuento,ProID from ProductosTemp").ToList();

                        if (descuento != null && (Arguments.Values.CurrentCountDes != 5 || Arguments.Values.CurrentCountDes == -1))
                        {
                            if (descuento.ProID <= 0 && string.IsNullOrWhiteSpace(descuento.GrpCodigo))
                            {
                                new DS_Productos().ActualizarDescuentosForExInTemp();
                            }

                            double num = 0.0;

                            if (descuento.ProID != 0 && string.IsNullOrWhiteSpace(descuento.GrpCodigo))
                            {
                                num = new DS_Productos().GetCantidadTotalInTemp((int)Arguments.Values.CurrentModule, false, false, descuento.ProID, UnmCodigo: DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? descuento.UnmCodigo : "");
                            }
                            else if (!string.IsNullOrWhiteSpace(descuento.GrpCodigo))
                            {
                                num = GetCantidadTotalInTempByGrpCodigo(descuento.GrpCodigo, (int)Arguments.Values.CurrentModule, UnmCodigo: DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? descuento.UnmCodigo : "");
                            }

                            if (num >= inicial && num <= final)
                            {
                                new DS_Productos().ActualizarDescuentosForExInTemp(descuento);
                            }

                        }
                        if (forValor)
                        {
                            result = $@" DesPorciento = DesPorciento + (((CAST(Cantidad / ProUnidades as int) * ProUnidades) * Precio) * {desPorciento}), 
                             Descuento = Descuento + (((CAST(Cantidad / ProUnidades as int) * ProUnidades) * Precio) * {desPorciento})";
                        }
                        else
                        {
                            var montoDescuento = $"DesPorciento = DesPorciento + (((CAST(Cantidad / ProUnidades as int) * ProUnidades) * Precio) * {desPorciento}) ";
                            result = $@" DesPorciento = DesPorciento + {montoDescuento}
                              Descuento = Descuento + ((DesPorciento = DesPorciento + (((CAST(Cantidad / ProUnidades as int) * ProUnidades) * Precio) * {desPorciento})  * (Precio - Descuento)) /100.0)";
                        }
                        break;
                }

                Arguments.Values.CurrentCountDes = tipo;

            }else
            {
                switch (tipo)
                {
                    case 1: //exclusivo
                        result = (forValor ? "Descuento = " + desValor.ToString() + ", DesPorciento = ((" + desValor.ToString() + " / Precio) * 100.0)" : "DesPorciento = " + desPorciento.ToString() + ", Descuento = ((" + desPorciento.ToString() + " * Precio) / 100.0)");
                        break;
                    case 2: //acumulado
                        result = (forValor ? "Descuento = ifnull(Descuento, 0.0) + " + desValor.ToString() + ", DesPorciento = ifnull(DesPorciento, 0.0) + ((" + desValor.ToString() + " / Precio) * 100)" : "DesPorciento = ifnull(DesPorciento, 0.0) + " + desPorciento.ToString() + ", Descuento = ifnull(Descuento, 0.0) + ((" + desPorciento.ToString() + " * Precio) / 100.0)");
                        break;
                    case 3: //escalonado

                        PedDescuentos = SqliteManager.GetInstance().Query<ProductosTemp>("Select Descuento,ProID from ProductosTemp").ToList();

                        if (forValor)
                        {
                            result = " DesPorciento = DesPorciento + (" + desValor.ToString() + " / (Precio - ((Precio * DesPorciento) / 100.0)) * 100.0), Descuento = Descuento + " + desValor.ToString();
                        }
                        else
                        {
                            var montoDescuento = "((" + desPorciento.ToString() + " * (Precio - Descuento)) /100.0) ";
                            result = " DesPorciento = DesPorciento + ((" + montoDescuento.ToString() + " / Precio ) * 100.0), Descuento = Descuento + ((" + desPorciento.ToString() + " * (Precio - Descuento)) /100.0)"; ;
                        }
                        break;
                    case 5: //escalonado exclusivo

                        PedDescuentos = SqliteManager.GetInstance().Query<ProductosTemp>("Select Descuento,ProID from ProductosTemp").ToList();

                        if (descuento != null && (Arguments.Values.CurrentCountDes != 5 || Arguments.Values.CurrentCountDes == -1))
                        {
                            if (descuento.ProID <= 0 && string.IsNullOrWhiteSpace(descuento.GrpCodigo))
                            {
                                new DS_Productos().ActualizarDescuentosForExInTemp();
                            }

                            double num = 0.0;

                            if (descuento.ProID != 0 && string.IsNullOrWhiteSpace(descuento.GrpCodigo))
                            {
                                num = new DS_Productos().GetCantidadTotalInTemp((int)Arguments.Values.CurrentModule, false, false, descuento.ProID, UnmCodigo: DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? descuento.UnmCodigo : "");
                            }
                            else if (!string.IsNullOrWhiteSpace(descuento.GrpCodigo))
                            {
                                num = GetCantidadTotalInTempByGrpCodigo(descuento.GrpCodigo, (int)Arguments.Values.CurrentModule, UnmCodigo: DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? descuento.UnmCodigo : "");
                            }

                            if (num >= inicial && num <= final)
                            {
                                new DS_Productos().ActualizarDescuentosForExInTemp(descuento);
                            }

                        }
                        if (forValor)
                        {
                            result = " DesPorciento = DesPorciento + (" + desValor.ToString() + "  / (Precio - ((Precio * DesPorciento) / 100.0)) * 100.0), Descuento = Descuento + " + desValor.ToString();
                        }
                        else
                        {
                            var montoDescuento = "((" + desPorciento.ToString() + " * (Precio - Descuento)) /100.0) ";
                            result = " DesPorciento = DesPorciento + ((" + montoDescuento.ToString() + " / Precio ) * 100.0)," +
                            " Descuento = Descuento + ((" + desPorciento.ToString() + " * (Precio - Descuento)) /100.0)";
                        }
                        break;
                }

                Arguments.Values.CurrentCountDes = tipo;

            }

            return result;
        }

        public void GuardarProductosValidosParaDescuento(int cliId, EntregasRepartidorTransacciones entrega = null)
        {
            try
            {

                var lipcodigo = myParametro.GetParSectores() >= 2 && Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.LipCodigo : Arguments.Values.CurrentClient != null && !string.IsNullOrEmpty(Arguments.Values.CurrentClient.LiPCodigo) ? Arguments.Values.CurrentClient.LiPCodigo : "Default";

                var useProPrecios = !string.IsNullOrWhiteSpace(lipcodigo) && (lipcodigo == "*P.ProPrecio*" || lipcodigo == "*P.ProPrecio2*" || lipcodigo == "*P.ProPrecio3*");


                if (myParametro.GetParPedidosDescuentoManual() || myParametro.GetParPedidosDescuentoManualGeneral() > 0.0)
                {
                    return;
                }

                var where = " AND ((NOT(dv.GrpCodigo IS NULL OR ifnull(dv.GrpCodigo, '') = '' or ifnull(dv.GrpCodigo, '0') = '0') AND p.ProID " +
                        "IN (SELECT ProID FROM GrupoProductosDetalle WHERE GrpCodigo = dv.GrpCodigo)) OR((dv.ProID = p.ProID OR IFNULL(dv.ProID, 0) = 0) AND (IFNULL(dv.GrpCodigo, '') = '' or ifnull(dv.GrpCodigo, '0') = '0'))  ) ";

                if (Arguments.Values.CurrentSector != null)
                {
                    where += " AND (dv.SecCodigos like '%{" + Arguments.Values.CurrentSector.SecCodigo + "}%' or ifnull(dv.SecCodigos, '') = '' )";
                }

                var update = "select distinct p.ProID as ProID, " + cliId.ToString() + " as CliID, 1 as TieneDescuento "+(DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? ", dv.UnmCodigo " : ", lpc.UnmCodigo ") + ", dv.TitId as TitId " +
                    "from DescuentosRecargos dv " +
                    "inner join Productos p on p.ProID > -1 and p.ProID not in (select ProID from ProductosValidosOfertas where ProID = p.ProID and ifnull(TieneDescuento, 0) = 1) " +
                    (Arguments.Values.CurrentModule == Modules.DEVOLUCIONES || Arguments.Values.CurrentModule == Modules.INVFISICO || useProPrecios ? "left" : "inner") + " join ListaPrecios lpc on " +
                     " " + (!string.IsNullOrWhiteSpace(myParametro.GetParUnidadesMedidasVendedorUtiliza()) ? " ifnull(lower(lpc.UnmCodigo), '') in (" + myParametro.GetParUnidadesMedidasVendedorUtiliza() + ") and " : "") + " lpc.ProID = P.ProID and lpc.LipCodigo = '" + lipcodigo.Replace("*", "") + "' " +
                    "left join ProductosValidosOfertas v on v.ProID = p.ProID and v.CliID = " +cliId.ToString()+" " +
                    "where " + /*p.ProID not in (select ProID from ProductosValidosOfertas where ProID = p.ProID and CliID = " + cliId.ToString() + " and ifnull(TieneDescuento, 0) = 1) and (" + */
                    "STRFTIME('%Y-%m-%d',  " + (entrega != null ? "'" + entrega.VenFecha + "'" : " DATETIME('NOW', 'localtime')") + ")" +
                    "BETWEEN STRFTIME('%Y-%m-%d', dv.DesFechaInicio) " + (myParametro.GetDescuentoConDia() ? " AND STRFTIME('%Y-%m-%d', dv.DesFechaFin, '+1 day') " : " AND STRFTIME('%Y-%m-%d', dv.DesFechaFin) ") + "	" +
                    "and (IFNULL(dv.DesCaracteristicas,'') Not Like '%N%' or (IFNULL(dv.DesCaracteristicas,'') Like '%N%' and not exists(select 1 from ClientesProductosVendidos where   CliID = " + cliId.ToString() + " and p.Proid = ProID  ))) " +
                    "and (((IFNULL(dv.CliID, 0) = 0 and IFNULL(dv.GrcCodigo, '') <> '' " +
                    "AND ? IN(SELECT CliID FROM GrupoClientesDetalle WHERE GrcCodigo = dv.GrcCodigo)) " +
                    " " + (Arguments.Values.CurrentModule == Modules.PRODUCTOS && cliId == -1 ? " OR (dv.CliID != -1 and dv.GrcCodigo != -1) " : "") + " OR (ifnull(dv.CliID, 0) = 0 and (ifnull(grcCodigo, '')='' or ifnull(grcCodigo, '0') = '0'))) " +
                    "OR ((ifnull(grcCodigo, '') = '' or ifnull(grcCodigo, '0') = '0') and dv.CliID = ?)) " + where + "  " + (!myParametro.GetParMostrarDescuentosMaximos() ? " and dv.DoROperacion <> 3 " : "") + "   order by dv.DesID";
                
                var list = SqliteManager.GetInstance().Query<ProductosValidosOfertas>(update, new string[] { cliId.ToString(), cliId.ToString() });

                string proIds = "";
                //string UnmCodigo = "";

                if (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida())
                {
                    foreach (ProductosValidosOfertas p in list)
                    {
                        SqliteManager.GetInstance().Execute("update ProductosValidosOfertas set TieneDescuento = 1 , UnmCodigo = '" + p.UnmCodigo.ToString() + "'  " +
                            "where ProID = " + p.ProID.ToString() + "  and UnmCodigoOferta = '" + p.UnmCodigo.ToString() + "'  and CliID = " + cliId.ToString() + " and ifnull(TieneDescuento, 0) = 0");
                    }
                        

                }
                else
                {
                    foreach (ProductosValidosOfertas p in list)
                    {
                        proIds += !string.IsNullOrWhiteSpace(proIds) ? ", " + p.ProID.ToString() : p.ProID.ToString();
                    }

                }


                if (!DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() && !string.IsNullOrWhiteSpace(proIds))
                {
     
                    SqliteManager.GetInstance().Execute("update ProductosValidosOfertas set TieneDescuento = 1 " +
                        "where ProID in (" + proIds + ") and CliID = " + cliId.ToString() + " and ifnull(TieneDescuento, 0) = 0");
                }

                var insert = "insert into ProductosValidosOfertas(ProID, CliID, TieneOferta, TieneDescuento, UnmCodigo, TitId) " +
                    "select distinct p.ProID, " + cliId.ToString() + ", 0, 1 " + (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? " , IFNULL(dv.UnmCodigo,'') UnmCodigo " : ", IFNULL(lpc.UnmCodigo,'') UnmCodigo ") + ", dv.TitId as TitId  " +
                    "from DescuentosRecargos dv " +
                    "inner join Productos p on p.ProID > -1 " + "and p.ProID not in (select ProID from ProductosValidosOfertas where ProID = p.ProID )" +
                    (Arguments.Values.CurrentModule == Modules.DEVOLUCIONES || Arguments.Values.CurrentModule == Modules.INVFISICO || useProPrecios ? "left" : "inner") + " join ListaPrecios lpc on " +
                    " " + (!string.IsNullOrWhiteSpace(myParametro.GetParUnidadesMedidasVendedorUtiliza()) ? " ifnull(lower(lpc.UnmCodigo), '') in (" + myParametro.GetParUnidadesMedidasVendedorUtiliza() + ") and " : "") + " lpc.ProID = P.ProID and lpc.LipCodigo = '" + lipcodigo.Replace("*", "") + "' " +
                    "where " + /* " p.ProID not in (select ProID from ProductosValidosOfertas where ProID = p.ProID and CliID = " + cliId.ToString() + ")  and "( */
                    "STRFTIME('%Y-%m-%d',  " + (entrega != null ? "'" + entrega.VenFecha + "'" : " DATETIME('NOW', 'localtime')") + ")" +
                    "BETWEEN STRFTIME('%Y-%m-%d', dv.DesFechaInicio) " + (myParametro.GetDescuentoConDia() ? " AND STRFTIME('%Y-%m-%d', dv.DesFechaFin, '+1 day') " : " AND STRFTIME('%Y-%m-%d', dv.DesFechaFin) ") + "	" +
                    "and (IFNULL(dv.DesCaracteristicas,'') Not Like '%N%' or (IFNULL(dv.DesCaracteristicas,'') Like '%N%' and not exists(select 1 from ClientesProductosVendidos where   CliID = " + cliId.ToString() + " and p.Proid = ProID  ))) " +
                    "and (((IFNULL(dv.CliID, 0) = 0 and IFNULL(dv.GrcCodigo, '') <> '' and ifnull(dv.GrcCodigo, '0') <> '0' " +
                    "AND ? IN(SELECT CliID FROM GrupoClientesDetalle WHERE GrcCodigo = dv.GrcCodigo)) " +
                    "OR (ifnull(dv.CliID, 0) = 0 and (ifnull(grcCodigo, '')='' or ifnull(grcCodigo, '0') = '0'))) "+(Arguments.Values.CurrentModule == Modules.PRODUCTOS && cliId == -1 ? " OR (dv.CliID != -1 and dv.GrcCodigo != -1) " : "")+" " +
                    "OR ((ifnull(grcCodigo, '') = '' or ifnull(grcCodigo, '0') = '0') and dv.CliID = ?)) " + where + "  " + (!myParametro.GetParMostrarDescuentosMaximos() ? " and dv.DoROperacion <> 3 " : "") + "  order by dv.DesID";

                SqliteManager.GetInstance().Execute(insert, new string[] { cliId.ToString(), cliId.ToString() });

                //var list = SqliteManager.GetInstance().Query<ProductosValidosOfertas>(query, new string[] { cliId.ToString(), cliId.ToString() });

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        public void GuardarProductosValidosParaDescuentoPreview(int cliId, EntregasRepartidorTransacciones entrega = null)
        {
            try
            {         
                if(myParametro.GetParPedidosDescuentoManualGeneral() > 0.0)
                {
                    return;
                }

                var grupoClientes = GetGrupoClientesByCliId(cliId);

                var whereGrupoClientes = "";
                foreach(var grupo in grupoClientes)
                {
                    whereGrupoClientes += (string.IsNullOrWhiteSpace(whereGrupoClientes) ? "" : ", ") + "'" + grupo.GrcCodigo + "'";
                }

                var whereSector = "";

                if(Arguments.Values.CurrentSector != null)
                {
                    whereSector = " AND (d.SecCodigos like '%{" + Arguments.Values.CurrentSector.SecCodigo + "}%' or ifnull(d.SecCodigos, '') = '' ) ";
                }

                var query = "insert or replace into ProductosValidosOfertas(ProID, TieneDescuento, TieneOferta, TieneDescuentoEscala, PorcientoDescuento, CliID, TitId) " +
                    "select distinct p.ProID as ProID, 1 as TieneDescuento, v.TieneOferta as TieneOferta, " +
                    "case when (select count(DesID) from DescuentosRecargosDetalle where DesID = d2.DesID) > 1 then 1 else 0 end as TieneDescuentoEscala, d2.DRDPorciento as PorcientoDescuento, " + cliId.ToString() + " as CliID, d.TitId as TitId " +
                    "from Productos p " +
                    "inner join DescuentosRecargos d on 1=1 " + whereSector + " " +
                    "inner join DescuentosRecargosDetalle d2 on d2.DesID = d.DesID " +
                    "left join ProductosValidosOfertas v on v.ProID = p.ProID and v.CliID = " + cliId.ToString() + " " +
                    "where ifnull(d2.DRDCantidadInicial, 0) = 1  " + (!myParametro.GetParMostrarDescuentosMaximos() ? " and d.DoROperacion <> 3 " : "") + "  and STRFTIME('%Y-%m-%d', " + (entrega != null ? "'" + entrega.VenFecha + "'" : " DATETIME('NOW', 'localtime')") + ") " +
                    "BETWEEN STRFTIME('%Y-%m-%d', d.DesFechaInicio) " + (myParametro.GetDescuentoConDia() ? " AND STRFTIME('%Y-%m-%d', d.DesFechaFin, '+1 day') " : " AND STRFTIME('%Y-%m-%d', d.DesFechaFin) ") + "	" +
                    "and ((ifnull(d.ProID, 0) != 0 and ifnull(d.ProID, 0) = p.ProID and (ifnull(d.GrpCodigo, '') = '' or ifnull(d.GrpCodigo, '') = '0')) or (ifnull(d.ProID, 0) = 0 and p.ProID in (select distinct ProID from GrupoProductosDetalle where GrpCodigo = d.GrpCodigo))) " +
                    "and ((" + cliId.ToString() + " = -1) OR (ifnull(d.CliID, 0) = " + cliId.ToString() + " and (ifnull(d.GrcCodigo, '') = '' or ifnull(d.GrcCodigo, '') = '0')) or (ifnull(d.CliID, 0) = 0 and ifnull(d.GrcCodigo, '') in (" + whereGrupoClientes + ")) or (ifnull(d.CliID, 0) = 0 and (ifnull(d.GrcCodigo, '') = '' or ifnull(d.GrcCodigo, '') = '0'))) " +
                    "group by p.ProID ";
                    //"order by cast(d2.DRDCantidadInicial as real) "+(cliId == -1 ? "desc" : "asc")+", count(d2.DesID) desc";

                SqliteManager.GetInstance().Execute(query, new string[] { });

                
                query = "insert or replace into ProductosValidosOfertas(ProID, TieneDescuento, TieneOferta, TieneDescuentoEscala, PorcientoDescuento, CliID, TitId) " +
                    "select distinct p.ProID as ProID, 1 as TieneDescuento, v.TieneOferta as TieneOferta, " +
                    "1 as TieneDescuentoEscala, case when ifnull(d2.DRDCantidadInicial, 0) = 1 then d2.DRDPorciento else v.PorcientoDescuento end as PorcientoDescuento, " + cliId.ToString()+ " as CliID, d.TitId as TitId " +
                    "from Productos p " +
                    "inner join DescuentosRecargos d on 1=1 " + whereSector + " " + 
                    "inner join DescuentosRecargosDetalle d2 on d2.DesID = d.DesID " +
                    "left join ProductosValidosOfertas v on v.ProID = p.ProID and v.CliID = "+cliId.ToString()+" " +
                    "where (ifnull(d2.DRDCantidadInicial,0) > 1 or (select count(DesID) from DescuentosRecargosDetalle where DesID = d.DesID) > 1) " + (!myParametro.GetParMostrarDescuentosMaximos() ? " and d.DoROperacion <> 3 " : "") + " and STRFTIME('%Y-%m-%d', " + (entrega != null ? "'" + entrega.VenFecha + "'" : " DATETIME('NOW', 'localtime')") + ")" +
                    "BETWEEN STRFTIME('%Y-%m-%d', d.DesFechaInicio) " + (myParametro.GetDescuentoConDia() ? " AND STRFTIME('%Y-%m-%d', d.DesFechaFin, '+1 day') " : " AND STRFTIME('%Y-%m-%d', d.DesFechaFin) ") + "	" +
                    "and ((ifnull(d.ProID, 0) != 0 and ifnull(d.ProID, 0) = p.ProID and (ifnull(d.GrpCodigo, '') = '' or ifnull(d.GrpCodigo, '') = '0')) or (ifnull(d.ProID, 0) = 0 and p.ProID in (select distinct ProID from GrupoProductosDetalle where GrpCodigo = d.GrpCodigo))) " +
                    "and ((" + cliId.ToString() + " = -1) OR (ifnull(d.CliID, 0) = " + cliId.ToString()+ " and (ifnull(d.GrcCodigo, '') = '' or ifnull(d.GrcCodigo, '') = '0')) or (ifnull(d.CliID, 0) = 0 and ifnull(d.GrcCodigo, '') in (" + whereGrupoClientes+")) or (ifnull(d.CliID, 0) = 0 and (ifnull(d.GrcCodigo, '') = '' or ifnull(d.GrcCodigo, '') = '0'))) " +
                    "group by p.ProID ";
                    //"order by cast(d2.DRDCantidadInicial as real) "+(cliId == -1 ? "desc" : "asc")+", count(d2.DesID) desc";

                SqliteManager.GetInstance().Execute(query, new string[] { });                

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }


        private List<GrupoClientes> GetGrupoClientesByCliId(int cliId)
        {
            return SqliteManager.GetInstance().Query<GrupoClientes>("select distinct GrcCodigo from GrupoClientesDetalle where CliID = ? ", 
                new string[] { cliId.ToString() });
        }

        private List<ProductosValidosOfertas> GetProductosValidosOfertas()
        {
            return SqliteManager.GetInstance().Query<ProductosValidosOfertas>("select * from ProductosValidosOfertas", new string[] { });
        }

        public double getLipDescuento(string LipCodigo, int ProID)
        {
            string sql = "SELECT ifnull(LipDescuento,0) as LipDescuento FROM ListaPrecios " +
                    "WHERE LTRIM(RTRIM(LipCodigo)) = '" + LipCodigo + "' AND ProID = " + ProID + " ";

            var LipDescuento = SqliteManager.GetInstance().Query<ListaPrecios>(sql);
            double x = 0.0;
            foreach (var desc in LipDescuento)
            {
                x = desc.LipDescuento;
            }

            return x;
        }

        public double GetProDescuentoMaximo_DigitadoMayor(int ProID, double DescuentoDigitado)
        {
            string sql = "SELECT ifnull(ProDescuentoMaximo,0) as ProDescuentoMaximo FROM Productos " +
                    "WHERE ProID = " + ProID + " ";

            var list = SqliteManager.GetInstance().Query<Productos>(sql);

            if (DescuentoDigitado > list[0].ProDescuentoMaximo)
            {
                return list[0].ProDescuentoMaximo;
            }
            return DescuentoDigitado;
        }

        public double GetMontoDescuentoXPorciento(string PorcientoDescuento, int proid)
        {
            var list = SqliteManager.GetInstance().Query<ProductosTemp>("select ((" + PorcientoDescuento + " * Precio) /100.0) as Descuento from ProductosTemp where proid = " + proid + " ", new string[] { });

            return list[0].Descuento;
        }


        public bool IsDiscountManualValid(double descuentoManual, ProductosTemp producto, int titId, int cliId,double cantidad, out double descMaximoPermitido)
        {
            descMaximoPermitido = 0.0;

            var descuentos = GetDescuentosDisponibles(titId, cliId, proId: producto.ProID, forDescManualMaximo: true);

            foreach(var descuento in descuentos)
            {
                var detalles = GetDetalles(descuento.DesID, quantityRange: cantidad);

                foreach(var det in detalles)
                {
                    var desc = 0.0;

                    if(det.DRDPorciento > 0)
                    {
                        desc = Math.Round(producto.Precio * (det.DRDPorciento / 100.0), 2);
                        descuentoManual = Math.Round(descuentoManual, 2);
                    }
                    else
                    {
                        desc = det.DRDValor;
                    }

                    if(descuentoManual > desc)
                    {
                        descMaximoPermitido = desc;
                        return false;
                    }

                }
            }

            return true;
        }
    }
}
