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

namespace MovilBusiness.DataAccess
{
    public class DS_InventariosFisicos : DS_Controller
    {
        public int SaveInventario(int invArea, DS_Visitas myVis,bool IsEditing, int editedSecuencia, List<ProductosTemp> ProductosFaltantes = null)
        {
            var inv = new Hash("InventarioFisico");
            inv.Add("InvEstatus", 1);
            int invSecuencia;

            if (!IsEditing)
            {
                invSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("InventarioFisico");

                myVis.ActualizarVisitaEfectiva(Arguments.Values.CurrentVisSecuencia);

                inv.Add("CliID", Arguments.Values.CurrentClient.CliID);
                inv.Add("VisSecuencia", Arguments.Values.CurrentVisSecuencia);
            }
            else
            {
                invSecuencia = editedSecuencia;
            }

            inv.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            inv.Add("invSecuencia", invSecuencia);            
            inv.Add("rowguid", Guid.NewGuid().ToString());
            inv.Add("infFecha", Functions.CurrentDate());
            inv.Add("CuaSecuencia", Arguments.Values.CurrentCuaSecuencia == -1 ? 0 : Arguments.Values.CurrentCuaSecuencia);
            inv.Add("InvFechaActualizacion", Functions.CurrentDate());
            inv.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);

            if(invArea > 0)
            {
                inv.Add("InvArea", invArea);
            }
            
            inv.Add("mbVersion", Functions.AppVersion);

            if(IsEditing)
            {
                InventarioFisico inventario = GetInventarioBySecuencia(invSecuencia);

                inv.ExecuteUpdate(new string[] { "rowguid" }, new Model.Internal.DbUpdateValue[] { new Model.Internal.DbUpdateValue()
                { Value = inventario.rowguid.Trim(), IsText = true } }, true);
            }
            else
            {
                inv.ExecuteInsert();
            }

            var myProd = new DS_Productos();

            var productos = myProd.GetResumenProductos((int)Modules.INVFISICO, isfromsave:true);

            var par = myParametro.GetParInventarioFisicoArea();

            bool parInvArea = !string.IsNullOrWhiteSpace(par) && par.ToUpper().Trim() == "D";

            if (productos != null)
            {
                int pos = 1;

                int parTomarCantidades = myParametro.GetParInventariosTomarCantidades();
                var parTomarCantidadesSinDetalle = myParametro.GetParInventariosTomarCantidadesSinDetalle() > 0;
                var parCapturarCantidadFacing = myParametro.GetParInventarioFisicoCapturarFacing();

                foreach (var prod in productos)
                {
                    Hash invd = new Hash("InventarioFisicoDetalle");

                    invd.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                    invd.Add("invSecuencia", invSecuencia);
                    invd.Add("invPosicion", pos); pos++;
                    invd.Add("ProID", prod.ProID);
                    invd.Add("infTipoInventario", 1);
                    invd.Add("rowguid", Guid.NewGuid().ToString());

                    if(myParametro.GetParInventarioFisicosLotes()==1)
                    {
                        invd.Add("infLote", prod.Lote);
                        invd.Add("infLoteLogico", prod.invLote);
                    }
                    else
                    {
                        invd.Add("infLote", prod.CheckValueForInvFis);
                    }

                    if(parTomarCantidades <= 2 && myParametro.GetParInventarioFisico() != 2)
                    {
                        if (parTomarCantidadesSinDetalle)
                        {
                            if (prod.InvAreaId == 1)
                            {
                                invd.Add("infCantidad", prod.CantidadAlm);
                                invd.Add("infCantidadDetalle", 0);
                            }
                            else if (prod.InvAreaId == 2)
                            {
                                invd.Add("infCantidad", prod.CanTidadGond);
                                invd.Add("infCantidadDetalle", 0);
                            }

                        }
                        else
                        {
                            invd.Add("infCantidad", prod.CanTidadGond);
                            invd.Add("infCantidadDetalle", prod.CantidadAlm);
                        }

                    }else
                    {
                        invd.Add("infCantidad", prod.Cantidad);
                        invd.Add("infCantidadDetalle", prod.CantidadDetalle);
                    }

                    if (parCapturarCantidadFacing)
                    {
                        invd.Add("infCantidadFacing", prod.CantidadFacing);
                    }

                    invd.Add("infCantidadLogica", myParametro.GetParInventarioFisico() == 2 ? prod.InvCantidad : 0);
                    invd.Add("CliID", Arguments.Values.CurrentClient.CliID);
                    invd.Add("InvFechaActualizacion", Functions.CurrentDate());
                    invd.Add("InvPrecioVenta", prod.PrecioTemp);
                    invd.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);

                    if (parInvArea)
                    {
                        invd.Add("InvArea", prod.InvAreaId);
                    }

                    if (IsEditing)
                    {
                       InventarioFisicoDetalle infdetalle =  GetInventarioDetalleBySecuencia(invSecuencia);

                        invd.ExecuteUpdate(new string[] { "rowguid" }, new Model.Internal.DbUpdateValue[] { new Model.Internal.DbUpdateValue()
                                          { Value = infdetalle.rowguid.Trim(), IsText = true } }, true);
                    }
                    else
                    {
                        invd.ExecuteInsert();
                    }
                }

                if (myParametro.GetParInventarioFisico() == 2 && ProductosFaltantes != null && ProductosFaltantes.Count > 0)
                {
                    int pedSecuencia = 0;
                    var faltantes = ProductosFaltantes.Where(p => ((p.InvCantidad * p.ProUnidades) + p.InvCantidadDetalle) > ((p.Cantidad * p.ProUnidades) + p.CantidadDetalle)).ToList();
                    var parCrearPedidoPorFaltante = DS_RepresentantesParametros.GetInstance().GetParCrearPedidoByInventarioFisico();
                    var tipoPedido = myParametro.GetParTipoPedidoParaInventarioFisico();

                    if (parCrearPedidoPorFaltante && faltantes.Count > 0)
                    {

                        var list = new List<PedCamposLoteyCantidad>();
                        foreach (var pro in faltantes)
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

                            double totalitbis = pro.IndicadorOferta ? 0 :
                            Math.Round((pro.Precio + pro.Selectivo + pro.AdValorem - pro.Descuento) *
                            (cantidadDetalle > 0 ? cantidad + (double.Parse(cantidadDetalle.ToString())
                            / double.Parse(pro.ProUnidades.ToString())) : pro.ProUnidades > 0 ? cantidad / pro.ProUnidades :
                            cantidad) * (pro.Itbis / 100), 2);

                            var item = new PedCamposLoteyCantidad()
                            {
                                ProID = pro.ProID,
                                Lote = pro.Lote,
                                Cantidad = cantidad
                            };

                            list.Add(item);
                        }
                        
                        pedSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Pedidos");

                        var ped = new Hash("Pedidos");
                        ped.Add("PedEstatus", 1);
                        ped.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                        ped.Add("PedSecuencia", pedSecuencia);
                        ped.Add("CliID", Arguments.Values.CurrentClient.CliID);
                        ped.Add("VisSecuencia", Arguments.Values.CurrentVisSecuencia);
                        if (myParametro.GetParSectores() > 0)
                        {
                            ped.Add("SecCodigo", Arguments.Values.CurrentSector?.SecCodigo);
                        }
                        ped.Add("PedFecha", Functions.CurrentDate());
                        ped.Add("PedTotal", faltantes.Count);
                        ped.Add("PedTipoPedido", tipoPedido); //TODO -Validar
                        ped.Add("PedOrdenCompra", "");
                        ped.Add("MonCodigo", Arguments.Values.CurrentClient.MonCodigo);
                        ped.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                        ped.Add("PedFechaActualizacion", Functions.CurrentDate());
                        ped.Add("PedPrioridad", 0); 
                        ped.Add("PedOtrosDatos", JsonConvert.SerializeObject(list)); 
                        ped.Add("CldDirTipo", ""); 
                        ped.Add("ConID", Arguments.Values.CurrentClient.ConID);
                        ped.Add("mbVersion", Functions.AppVersion);
                        ped.Add("LipCodigo", Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.LipCodigo : Arguments.Values.CurrentClient.LiPCodigo);
                        if (myParametro.GetParPedidosDiasEntrega() != -1)
                        {
                            DateTime Dateinndays = DateTime.Now.AddDays(myParametro.GetParPedidosDiasEntrega());
                            ped.Add("PedFechaEntrega", Dateinndays.ToString("yyyy-MM-dd HH:mm:ss"));
                        }
                        else
                        {
                            ped.Add("PedFechaEntrega", Functions.CurrentDate());
                        }
                        if (Arguments.Values.CurrentCuaSecuencia != 0)
                        {
                            ped.Add("CuaSecuencia", Arguments.Values.CurrentCuaSecuencia);
                        }
                        ped.Add("rowguid", Guid.NewGuid().ToString());
                        ped.Add("PedIndicadorCompleto", 0);
                        ped.ExecuteInsert();


                        pos = 1;

                        foreach (var pro in faltantes)
                        {
                            Hash det = new Hash("PedidosDetalle");
                            det.Add("PedSecuencia", pedSecuencia);
                            det.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                            det.Add("PedPosicion", pos); pos++;
                            det.Add("ProID", pro.ProID);
                            det.Add("CliID", Arguments.Values.CurrentClient.CliID);

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

                            double totalitbis = pro.IndicadorOferta ? 0 :
                            Math.Round((pro.Precio + pro.Selectivo + pro.AdValorem - pro.Descuento) *
                            (cantidadDetalle > 0 ? cantidad + (double.Parse(cantidadDetalle.ToString())
                            / double.Parse(pro.ProUnidades.ToString())) : pro.ProUnidades > 0 ? cantidad / pro.ProUnidades :
                            cantidad) * (pro.Itbis / 100), 2);

                            det.Add("PedCantidad", cantidad);
                            det.Add("PedCantidadDetalle", cantidadDetalle);
                            det.Add("PedtotalItbis", totalitbis);
                            det.Add("CedCodigo", !string.IsNullOrEmpty(pro.CedCodigo) ? pro.CedCodigo : "");
                            det.Add("PedPrecio", pro.Precio);
                            det.Add("PedItbis", pro.Itbis);
                            det.Add("PedSelectivo", pro.Selectivo);
                            det.Add("PedAdValorem", pro.AdValorem);
                            det.Add("PedIndicadorOferta", pro.IndicadorOferta);
                            det.Add("PedIndicadorCompleto", 0);
                            det.Add("OfeID", pro.OfeID);

                            det.Add("PedDesPorciento", pro.DesPorciento);      //  temp.DesPorciento);
                            det.Add("PedDescuento", pro.Descuento * cantidad);
                            det.Add("pedTotalDescuento", pro.Descuento);
                            det.Add("UnmCodigo", pro.UnmCodigo);
                            det.Add("PedFechaActualizacion", Functions.CurrentDate());
                            det.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                            det.Add("rowguid", Guid.NewGuid().ToString());
                            det.ExecuteInsert();
                        }

                        DS_RepresentantesSecuencias.UpdateSecuencia("Pedidos", pedSecuencia);

                    }
                }

            }

            if (DS_RepresentantesParametros.GetInstance().GetParVisitasResultados())
            {
                ActualizarVisitasResultados();
            }

            myProd.ClearTemp((int)Modules.INVFISICO);
            if(myParametro.GetParInventarioFisico() == 2 && ProductosFaltantes != null)
            {
                myProd.ClearTemp((int)Modules.PEDIDOS);
            }
            
            DS_RepresentantesSecuencias.UpdateSecuencia("InventarioFisico", invSecuencia);

            return invSecuencia;
        }

        private void ActualizarVisitasResultados()
        {
            var list = SqliteManager.GetInstance().Query<VisitasResultados>("select '' as VisComentario, 7 as TitID, count(*) as VisCantidadTransacciones, '' as VisComentarios " +
                "from InventarioFisico where RepCodigo = '"+Arguments.CurrentUser.RepCodigo.Trim()+"' and VisSecuencia = ? ", 
                new string[] { Arguments.Values.CurrentVisSecuencia.ToString() });

            if(list != null && list.Count > 0)
            {
                new DS_Visitas().GuardarVisitasResultados(list[0]);
            }
        }
        public List<InventarioFisicoDetalle>  GetInventarioFisicoDetalles(int invSecuencia, bool confirmado= false)
        {
            var parArea = myParametro.GetParInventarioFisicoArea();

            string query = $@"select p.ProDescripcion as ProDescripcion, p.ProCodigo as ProCodigo, 
                           infCantidad, ind.ProID, infCantidadDetalle, infCantidadLogica from
                           {(confirmado ? "InventarioFisicoDetalleConfirmados" : "InventarioFisicoDetalle")} ind inner join Productos p on p.proid = ind.ProID  
                           where invSecuencia = {invSecuencia}";

            int InventariosTomarCantidades = myParametro.GetParInventariosTomarCantidades();
            if (InventariosTomarCantidades > 0 && InventariosTomarCantidades < 2)
            {
                query = $@"select InvArea, ProUnidades, ProDescripcion, ProCodigo,
                      SUM(infCantidad) as infCantidad, SUM(infCantidadDetalle) as infCantidadDetalle from(   
                      select ProUnidades, ind.ProID as ProID, p.ProDescripcion as ProDescripcion, p.ProCodigo as ProCodigo, 
                      infCantidad, NULL infCantidadDetalle, InvArea from {(confirmado ? "InventarioFisicoDetalleConfirmados" : "InventarioFisicoDetalle")} 
                      ind inner join Productos p on p.proid = ind.ProID where invSecuencia = {invSecuencia}
                      union
                      select ProUnidades, ind.ProID as ProID, p.ProDescripcion as ProDescripcion, p.ProCodigo as ProCodigo,
                      NULL infCantidad, infCantidadDetalle, InvArea from {(confirmado ? "InventarioFisicoDetalleConfirmados" : "InventarioFisicoDetalle")} 
                      ind inner join Productos p on p.proid = ind.ProID where invSecuencia = {invSecuencia}) t
                      group by ProID";

            }

            if (InventariosTomarCantidades == 3 && parArea.ToUpper().Trim() != "C")
            {
                query = $@"select InvArea, ProUnidades, ProDescripcion, ProCodigo, SUM(InfLote) as InfLote,
                      SUM(infCantidad) as infCantidad, SUM(infCantidadDetalle) as infCantidadDetalle, SUM(infCantidadLogica) as infCantidadLogica from
					  
					  (   
                      select ProUnidades, ind.ProID as ProID, p.ProDescripcion as ProDescripcion, p.ProCodigo as ProCodigo, CAST(ind.infLote as INT) as InfLote,
                      infCantidad, NULL infCantidadDetalle, NULL infCantidadLogica, InvArea from {(confirmado ? "InventarioFisicoDetalleConfirmados" : "InventarioFisicoDetalle")} 
                      ind inner join Productos p on p.proid = ind.ProID where invSecuencia = {invSecuencia} and InvArea = '1'
					  
                      union
					  
                      select ProUnidades, ind.ProID as ProID, p.ProDescripcion as ProDescripcion, p.ProCodigo as ProCodigo, CAST(ind.infLote as INT) as InfLote,
                      NULL infCantidad, infCantidad as infCantidadDetalle, NULL infCantidadLogica, InvArea from {(confirmado ? "InventarioFisicoDetalleConfirmados" : "InventarioFisicoDetalle")} 
                      ind inner join Productos p on p.proid = ind.ProID where invSecuencia = {invSecuencia} and InvArea = '2'
					  
					  union 
					  
					  select ProUnidades, ind.ProID as ProID, p.ProDescripcion as ProDescripcion, p.ProCodigo as ProCodigo, CAST(ind.infLote as INT) as InfLote,
                      NULL infCantidad, NULL infCantidadDetalle, infCantidad as infCantidadLogica, InvArea from {(confirmado ? "InventarioFisicoDetalleConfirmados" : "InventarioFisicoDetalle")} 
                      ind inner join Productos p on p.proid = ind.ProID where invSecuencia = {invSecuencia} and InvArea = '3'
					  
					  ) t
                      group by ProID";
            }

            return SqliteManager.GetInstance().Query<InventarioFisicoDetalle>(query);
        }
        public List<InventarioFisicoDetalle>  GetInventarioFisicoDetallesByProID(int invSecuencia, int proid)
        {
            string query = $@"select p.ProDescripcion as ProDescripcion, p.ProCodigo as ProCodigo, 
                           infCantidad, ind.ProID, infCantidadDetalle, infCantidadLogica, ifnull(ind.infLote,'') as infLote from
                           InventarioFisicoDetalle ind inner join Productos p on p.proid = ind.ProID  
                           where invSecuencia = {invSecuencia} and ind.proid = {proid} Group By ind.ProID, infLote";

            return SqliteManager.GetInstance().Query<InventarioFisicoDetalle>(query);
        }
        public void InsertarInventarioFisicoInTemp(int invSecuencia, bool confirmado)
        {
            new DS_Productos().ClearTemp((int)Modules.INVFISICO);

            string query = "";
            List<ProductosTemp> list = new List<ProductosTemp>();

            var parArea = myParametro.GetParInventarioFisicoArea();

            if(parArea.ToUpper().Trim() == "C")
            {
                query = "select  distinct " + ((int)Modules.INVFISICO).ToString() + " as TitID, i.ProID as ProID, p.ProDescripcion as ProDescripcion, p.UnmCodigo as UnmCodigo, p.ProCodigo as ProCodigo, i.infCantidad as Cantidad," +
                        "i.InvPrecioVenta as PrecioTemp, i.rowguid as rowguid, p.ProItbis as Itbis, ifnull(if.InvArea,-1) as InvAreaId, ifnull(u.Descripcion,'') as InvAreaDescr from " + (confirmado ? "InventarioFisicoDetalleConfirmados" : "InventarioFisicoDetalle") +" i " +
                        "inner join " + (confirmado ? "InventarioFisicoConfirmados" : "InventarioFisico") + " if on if.invSecuencia=i.invSecuencia and if.RepCodigo= i.RepCodigo " +
                        "Left join UsosMultiples u on u.CodigoUso=if.InvArea and u.CodigoGrupo='InvArea' " +
                        "inner join Productos p on p.proid = i.Proid where ltrim(rtrim(i.RepCodigo)) = ? and i.invSecuencia = ? ";

                list = SqliteManager.GetInstance().Query<ProductosTemp>(query, new string[] { Arguments.CurrentUser.RepCodigo.Trim(),invSecuencia.ToString() });
            }
            else if (myParametro.GetParInventariosTomarCantidades() < 3)
            {

                query = "select  distinct " + ((int)Modules.INVFISICO).ToString() + " as TitID, i.ProID as ProID, p.ProDescripcion as ProDescripcion, p.UnmCodigo as UnmCodigo, p.ProCodigo as ProCodigo, i.infCantidad as Cantidad," +
                        "i.InvPrecioVenta as PrecioTemp, i.rowguid as rowguid, p.ProItbis as Itbis from " + (confirmado ? "InventarioFisicoDetalleConfirmados" : "InventarioFisicoDetalle") + " i " +
                        "inner join Productos p on p.proid = i.Proid where ltrim(rtrim(i.RepCodigo)) = ? and invSecuencia = ? ";

                list = SqliteManager.GetInstance().Query<ProductosTemp>(query, new string[] { Arguments.CurrentUser.RepCodigo.Trim(),
                                                                                                        invSecuencia.ToString() });

            }
            else
            {
                query = $@"select distinct {((int)Modules.INVFISICO).ToString()} as TitID, InvArea as InvAreaId, ProID, ProDescripcion, ProCodigo, case when SUM(InfLote) > 0 then 1 else 0 end as CheckValueForInvFis,
                      SUM(infCantidad) as CantidadAlm, SUM(infCantidadDetalle) as CanTidadGond, SUM(infCantidadLogica) as CanTidadTramo, rowguid, 0 as RadioButtonNotEnabled from
					  
					  (   
                      select ProUnidades, ind.ProID as ProID, p.ProDescripcion as ProDescripcion, p.ProCodigo as ProCodigo, CAST(ind.infLote as INT) as InfLote,
                      infCantidad, NULL infCantidadDetalle, NULL infCantidadLogica, InvArea, ind.rowguid from {(confirmado ? "InventarioFisicoDetalleConfirmados" : "InventarioFisicoDetalle")} 
                      ind inner join Productos p on p.proid = ind.ProID where invSecuencia = {invSecuencia} and InvArea = '1'
					  
                      union
					  
                      select ProUnidades, ind.ProID as ProID, p.ProDescripcion as ProDescripcion, p.ProCodigo as ProCodigo, CAST(ind.infLote as INT) as InfLote,
                      NULL infCantidad, infCantidad as infCantidadDetalle, NULL infCantidadLogica, InvArea, ind.rowguid as rowguid from {(confirmado ? "InventarioFisicoDetalleConfirmados" : "InventarioFisicoDetalle")} 
                      ind inner join Productos p on p.proid = ind.ProID where invSecuencia = {invSecuencia} and InvArea = '2'
					  
					  union 
					  
					  select ProUnidades, ind.ProID as ProID, p.ProDescripcion as ProDescripcion, p.ProCodigo as ProCodigo, CAST(ind.infLote as INT) as InfLote,
                      NULL infCantidad, NULL infCantidadDetalle, infCantidad as infCantidadLogica, InvArea, ind.rowguid as rowguid from {(confirmado ? "InventarioFisicoDetalleConfirmados" : "InventarioFisicoDetalle")} 
                      ind inner join Productos p on p.proid = ind.ProID where invSecuencia = {invSecuencia} and InvArea = '3'
					  
					  ) t
                      group by ProID";

                list = SqliteManager.GetInstance().Query<ProductosTemp>(query, new string[] { });

            }


            foreach (var ent in list)
            {
                ent.rowguid = Guid.NewGuid().ToString();
            }

            SqliteManager.GetInstance().InsertAll(list);
        }

        public void EstInventarioFisico(string rowguid, int est)
        {
            Hash ped = new Hash("InventarioFisico");
            ped.Add("InvEstatus", est);
            ped.Add("EntFechaActualizacion", Functions.CurrentDate());
            ped.Add("UsuInicioSesion","mdsoft");

            if (est == 0)
            {
                if (new DS_SuscriptoresCambios().UpdateCambioEstadoInsertByRowguid(rowguid, est))
                {
                    ped.SaveScriptForServer = false;
                }
            }

            ped.ExecuteUpdate("rowguid = '" + rowguid + "'");
        }

        public InventarioFisico GetInventarioBySecuencia(int invSecuencia, bool confirmado= false)
        {
            string query = "select i.VisSecuencia, i.CliID, i.rowguid, ifnull(replace( strftime('%d-%m-%Y', SUBSTR(i.infFecha,1,10)),' ','' ), '') as infFecha, ifnull(i.InvArea,-1) as InvArea, ifnull(u.Descripcion,'') as InvAreaDescr from " + (confirmado ? "InventarioFisicoConfirmados" : "InventarioFisico") + " i " +
                "Left join UsosMultiples u on u.CodigoUso=i.InvArea and u.CodigoGrupo='InvArea' where invSecuencia = ? ";

            var invfisico = SqliteManager.GetInstance().Query<InventarioFisico>(query, new string[] { invSecuencia.ToString() }).FirstOrDefault();

            return invfisico;
        }
        public InventarioFisicoDetalle GetInventarioDetalleBySecuencia(int invSecuencia)
        {
            string query = "select rowguid from InventarioFisico where invSecuencia = ? ";

            var invfisico = SqliteManager.GetInstance().Query<InventarioFisicoDetalle>(query, new string[] { invSecuencia.ToString() }).FirstOrDefault();

            return invfisico;
        }

        public List<InventarioFisico> GetByInventariosByClient(int cliId)
        {
            var query = "select (select count(*) from InventarioFisicoDetalle where InvSecuencia = i.invSecuencia and RepCodigo = i.RepCodigo) as InvTotal, invSecuencia, " +
                "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(infFecha,1,10)),' ','' ), '') as infFecha, EstDescripcion as InvEstado from " +
                "InventarioFisico i " +
                "inner join Estados e on e.EstTabla = 'InventarioFisico' and e.EstEstado = i.InvEstatus " +
                "where i.CliID = ? and i.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' order by i.invSecuencia";
            return SqliteManager.GetInstance().Query<InventarioFisico>(query, new string[] { cliId.ToString() });
        }

        public List<ProductosTemp> GetProductosInInventarioConFaltantes(int CliID, bool IsForValid = false)
        {
            var query = "";

            if (IsForValid)
            {
              query = "select ifnull(p.ProDatos3, '') as ProDatos3, p.UnmCodigo as UnmCodigo, i.ProID as ProID, p.ProCodigo as ProCodigo, p.ProDescripcion as Descripcion, i.invCantidad as InvCantidad, 0 as InvCantidadDetalle, " +
              "l.lipprecio as Precio, p.proitbis as Itbis, p.prounidades as ProUnidades, ifnull(t.Cantidad,0) as Cantidad, ifnull(t.Cantidad,0) as CantidadManual, " +
              "0 as CantidadManualDetalle, 0 as CantidadDetalle, ifnull(t.Lote, '') as Lote,  ifnull(i.invLote, '') as invLote " +
              "from InventariosClientes i " +
              "inner join Productos p on p.proid = i.proid " +
              "left join productostemp t on t.proid = i.proid and ifnull(i.invLote, '') = ifnull(t.Lote, '') and t.TitID = " + ((int)Modules.INVFISICO).ToString() + " " +
              "left join listaprecios l on l.proid = i.proid " +
              "where i.CliID = '" + CliID + "' and l.lipcodigo = (select LipCodigo from Clientes Where CliID = '" + CliID + "' limit 1) " +
              "and trim(upper(l.unmCodigo)) = trim(upper(t.UnmCodigo)) and " +
              "(((i.invCantidad * p.ProUnidades)) > ((ifnull(t.Cantidad, 0) * p.ProUnidades) + ifnull(t.CantidadDetalle, 0))) " +
              @" union 
	                select ifnull(p.ProDatos3, '') as ProDatos3, p.UnmCodigo as UnmCodigo, i.ProID as ProID, p.ProCodigo as ProCodigo, p.ProDescripcion as Descripcion, i.invCantidad as InvCantidad, 0 as InvCantidadDetalle, 
                l.lipprecio as Precio, p.proitbis as Itbis, p.prounidades as ProUnidades, 0 as Cantidad, i.invCantidad as CantidadManual,
                0 as CantidadManualDetalle, 0 as CantidadDetalle, ifnull(i.invLote, '') as Lote, ifnull(i.invLote, '') as invLote
                from InventariosClientes i 
                inner join Productos p on p.proid = i.proid 
                left join listaprecios l on l.proid = i.proid 
                where i.CliID = '" + CliID + "' and l.lipcodigo = (select LipCodigo from Clientes Where CliID = '" + CliID + "' limit 1) " +
              @" and trim(upper(l.unmCodigo)) = trim(upper(p.UnmCodigo)) and 
                (((i.invCantidad * p.ProUnidades)) > 0) and not exists (select 1 from Productostemp t where i.proid = t.proid and ifnull(i.invLote, '') = ifnull(t.Lote, '') and TitID = " + ((int)Modules.INVFISICO).ToString() + " ) ";

            }
            else
            {
                //primero compara los iguales
                //valida los inventariologico que no se caputaron
                //valida los productos que no estan en inventario logico
                query = "select ifnull(p.ProDatos3, '') as ProDatos3, p.UnmCodigo as UnmCodigo, i.ProID as ProID, p.ProCodigo as ProCodigo, p.ProDescripcion as Descripcion, i.invCantidad as InvCantidad, 0 as InvCantidadDetalle, " +
              "l.lipprecio as Precio, p.proitbis as Itbis, p.prounidades as ProUnidades, ifnull(t.Cantidad,0) as Cantidad, ifnull(t.Cantidad,0) as CantidadManual, " +
              "0 as CantidadManualDetalle, 0 as CantidadDetalle, ifnull(t.Lote, '') as Lote,  ifnull(i.invLote, '') as invLote  " +
              "from InventariosClientes i " +
              "inner join Productos p on p.proid = i.proid " +
              "left join productostemp t on t.proid = i.proid and ifnull(i.invLote, '') = ifnull(t.Lote, '') and t.TitID = " + ((int)Modules.INVFISICO).ToString() + " " +
              "left join listaprecios l on l.proid = i.proid " +
              "where i.CliID = '" + CliID + "' and l.lipcodigo = (select LipCodigo from Clientes Where CliID = '" + CliID + "' limit 1) " +
              "and trim(upper(l.unmCodigo)) = trim(upper(t.UnmCodigo)) and " +
              "(((i.invCantidad * p.ProUnidades)) > ((ifnull(t.Cantidad, 0) * p.ProUnidades) + ifnull(t.CantidadDetalle, 0)) or " +
              "((i.invCantidad * p.ProUnidades)) < ((ifnull(t.Cantidad, 0) * p.ProUnidades) + ifnull(t.CantidadDetalle, 0))) " +
              @" union 
	                select ifnull(p.ProDatos3, '') as ProDatos3, p.UnmCodigo as UnmCodigo, i.ProID as ProID, p.ProCodigo as ProCodigo, p.ProDescripcion as Descripcion, i.invCantidad as InvCantidad, 0 as InvCantidadDetalle, 
                l.lipprecio as Precio, p.proitbis as Itbis, p.prounidades as ProUnidades, 0 as Cantidad, i.invCantidad as CantidadManual,
                0 as CantidadManualDetalle, 0 as CantidadDetalle, ifnull(i.invLote, '') as Lote, ifnull(i.invLote, '') as invLote
                from InventariosClientes i 
                inner join Productos p on p.proid = i.proid 
                left join listaprecios l on l.proid = i.proid 
                where i.CliID = '" + CliID + "' and l.lipcodigo = (select LipCodigo from Clientes Where CliID = '" + CliID + "' limit 1) " +
              @" and trim(upper(l.unmCodigo)) = trim(upper(p.UnmCodigo)) and 
                ((((i.invCantidad * p.ProUnidades)) > 0) or (((i.invCantidad * p.ProUnidades)) < 0))  and not exists (select 1 from Productostemp t where i.proid = t.proid and ifnull(i.invLote, '') = ifnull(t.Lote, '') and TitID = " + ((int)Modules.INVFISICO).ToString() + " ) " +
                "union " +
                    "select ifnull(p.ProDatos3, '') as ProDatos3, p.UnmCodigo as UnmCodigo, t.ProID as ProID, p.ProCodigo as ProCodigo, p.ProDescripcion as Descripcion, 0 as InvCantidad, 0 as InvCantidadDetalle, " +
                "l.lipprecio as Precio, p.proitbis as Itbis, p.prounidades as ProUnidades, ifnull(t.Cantidad,0) as Cantidad, ifnull(t.Cantidad,0) as CantidadManual, " +
                "0 as CantidadManualDetalle, 0 as CantidadDetalle, ifnull(t.Lote, '') as Lote, '' as invLote " +
                "from productostemp t " +
                "inner join Productos p on p.proid = t.proid " +
                "left join listaprecios l on l.proid = t.proid " +
                "where not exists (select 1 from InventariosClientes i where t.proid=i.proid and ifnull(i.invLote, '') = ifnull(t.Lote, '') and  i.CliID = '" + CliID + "' ) " +
                "and l.lipcodigo = (select LipCodigo from Clientes Where CliID = '" + CliID + "' limit 1) " +
                "and trim(upper(l.unmCodigo)) = trim(upper(t.UnmCodigo)) ";
            }

            return SqliteManager.GetInstance().Query<ProductosTemp>(query, new string[] { });
        }

        public List<InventarioFisicoDetalle> GetProductosSobrantes(int invfisSecuencia)
        {
            var sql = "select  t.infCantidadLogica, t.ProID as ProID, t.infCantidad as infCantidad, t.infCantidadDetalle as infCantidadDetalle, p.ProDescripcion as ProDescripcion,  p.ProCodigo as ProCodigo, p.ProUnidades as ProUnidades " +
              "from InventarioFisicoDetalle t " +
              "inner join Productos p on p.Proid = t.Proid " +
              "where invSecuencia = " + invfisSecuencia + " and (t.infCantidadLogica < t.infCantidad) Order by p.LinID, p.Cat1ID, p.ProID";
            return SqliteManager.GetInstance().Query<InventarioFisicoDetalle>(sql, new string[] { });

        }

        public List<InventarioFisicoDetalle> GetProductosSobrantesByProId(int invfisSecuencia, int proid)
        {
            var sql = "select  t.infCantidadLogica, t.ProID as ProID, t.infCantidad as infCantidad, t.infCantidadDetalle as infCantidadDetalle, p.ProDescripcion as ProDescripcion,  p.ProCodigo as ProCodigo, p.ProUnidades as ProUnidades, ifnull(t.infLote,'') as infLote " +
              "from InventarioFisicoDetalle t " +
              "inner join Productos p on p.Proid = t.Proid " +
              "where invSecuencia = " + invfisSecuencia + " and t.Proid = " + proid + " and (t.infCantidadLogica < t.infCantidad) Group by t.ProID, t.infLote";
            return SqliteManager.GetInstance().Query<InventarioFisicoDetalle>(sql, new string[] { });

        }


        public List<ProductosTemp> GetProductosInInventarioCantidadesIguales(int CliID)
        {
             
          string  query = "select ifnull(p.ProDatos3, '') as ProDatos3, p.UnmCodigo as UnmCodigo, i.ProID as ProID, p.ProCodigo as ProCodigo, p.ProDescripcion as Descripcion, i.invCantidad as InvCantidad, 0 as InvCantidadDetalle, " +
          "l.lipprecio as Precio, p.proitbis as Itbis, p.prounidades as ProUnidades, ifnull(t.Cantidad,0) as Cantidad, ifnull(t.Cantidad,0) as CantidadManual, " +
          "0 as CantidadManualDetalle, 0 as CantidadDetalle, ifnull(t.Lote, '') as Lote,  ifnull(i.invLote, '') as invLote  " +
          "from InventariosClientes i " +
          "inner join Productos p on p.proid = i.proid " +
          "left join productostemp t on t.proid = i.proid and ifnull(i.invLote, '') = ifnull(t.Lote, '') and t.TitID = " + ((int)Modules.INVFISICO).ToString() + " " +
          "left join listaprecios l on l.proid = i.proid " +
          "where i.CliID = '" + CliID + "' and l.lipcodigo = (select LipCodigo from Clientes Where CliID = '" + CliID + "' limit 1) " +
          "and trim(upper(l.unmCodigo)) = trim(upper(t.UnmCodigo)) and " +
          "((i.invCantidad * p.ProUnidades)) = ((ifnull(t.Cantidad, 0) * p.ProUnidades) + ifnull(t.CantidadDetalle, 0))";
          
               
            

            return SqliteManager.GetInstance().Query<ProductosTemp>(query, new string[] { });
        }

    }
}
