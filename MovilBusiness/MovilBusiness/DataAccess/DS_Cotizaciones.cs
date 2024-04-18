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
    public class DS_Cotizaciones : DS_Controller
    {
        private DS_Productos myProd;
        private DS_UsosMultiples myUso;

        public DS_Cotizaciones(DS_Productos myProd = null, DS_UsosMultiples myUso= null)
        {
            if(myProd == null)
            {
                myProd = new DS_Productos();
            }

            if (myUso == null)
            {
                myUso = new DS_UsosMultiples();
            }

            this.myProd = myProd;
            this.myUso = myUso;
        }

        public int SaveCotizacion(string fechaEntrega, int conId, int tipoCotizacion, int prioridad, bool isEditing = false, int editedTraSecuecia = -1,bool isPreliminar = false, string MonCodigo = null, bool IsMultiEntrega = false, double subtotal = 0.00, double total = 0.00, double itbis = 0.00, double totalDescuentoGlobal = 0.00, double porDescuentoGlobal = 0.00, string camposAdicionales = null)
        {
            int cotSecuencia;

            int posForEdit = 0;
            List<int> Listpos = new List<int>();

            if (isEditing && editedTraSecuecia != -1)
            {
                cotSecuencia = editedTraSecuecia;
            }
            else
            {
                cotSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Cotizaciones");
                new DS_Visitas().ActualizarVisitaEfectiva(Arguments.Values.CurrentVisSecuencia);
            }

            var productos = myProd.GetResumenProductos((int)Modules.COTIZACIONES, false, isForGuardar: true);

            var cot = new Hash("Cotizaciones");
            cot.Add("CotEstatus", isPreliminar ? 3 : 1);

            if (!isEditing)
            {
                cot.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                cot.Add("CotSecuencia", cotSecuencia);
                cot.Add("CliID", Arguments.Values.CurrentClient.CliID);
                cot.Add("VisSecuencia", Arguments.Values.CurrentVisSecuencia);
            }

            cot.Add("CotFecha", Functions.CurrentDate());            
            cot.Add("ConTotal", productos.Count);
            cot.Add("ConPrioridad", prioridad);
            cot.Add("CotIndicadorCompleto", 0);
            cot.Add("CotFechaEntrega", fechaEntrega);
            cot.Add("ConIndicadorRevision", false);
            cot.Add("conTipo", tipoCotizacion);
            
            cot.Add("mbVersion", Functions.AppVersion);

            var cliDetalle = new DS_Clientes().GetDetalleFromCliente(Arguments.Values.CurrentClient.CliID, Arguments.CurrentUser.RepCodigo);

            if (!isEditing)
            {
                if (myParametro.GetParSectores() > 0)
                {
                    cot.Add("SecCodigo", Arguments.Values.CurrentSector?.SecCodigo);

                    cot.Add("MonCodigo", Arguments.Values.CurrentSector?.MonCodigo);
                }
                else
                {
                    cot.Add("MonCodigo", Arguments.Values.CurrentClient.MonCodigo);
                    cot.Add("SecCodigo", cliDetalle?.SecCodigo);
                }
            }

            if (!string.IsNullOrWhiteSpace(MonCodigo))
            {
                cot.Add("MonCodigo", MonCodigo);
            }

            cot.Add("orvCodigo", cliDetalle?.OrvCodigo);
            cot.Add("ofvCodigo", cliDetalle?.ofvCodigo);
            cot.Add("CotIndicadorContado", false);
            cot.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            cot.Add("cotFechaActualizacion", Functions.CurrentDate());
            cot.Add("RepVendedor", Arguments.CurrentUser.RepCodigo);
            cot.Add("CotVencimiento", 0);
            
            cot.Add("CldDirTipo", Arguments.Values.CurrentClient.CldDirTipo);
            cot.Add("ConID", conId);

            cot.Add("CotSubTotal", subtotal);
            cot.Add("CotPorCientoDsctoGlobal", porDescuentoGlobal);
            cot.Add("CotMontoDsctoGlobal", totalDescuentoGlobal);

            if (myUso.GetCotizacionesCamposAdicionales() != null)
            {
                cot.Add("CotOtrosDatos", camposAdicionales);
            }

            if (isEditing)
            {
                Listpos = GetPosicionCotFromDetalle(cotSecuencia).Select(c => c.CotPosicion).ToList();

                cot.ExecuteUpdate("CotSecuencia = " + cotSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");

                Hash delete = new Hash("CotizacionesDetalle"); //eliminando las ofertas
                delete.ExecuteDelete("CotSecuencia = " + cotSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and CotIndicadorOferta = 1");

                //eliminando los productos que quito de la cotizacion
                var proIds = GetProIdQueryForDeleteWhileEditing(cotSecuencia, (int)Modules.COTIZACIONES);
                if (!string.IsNullOrWhiteSpace(proIds))
                {
                    delete.ExecuteDelete("CotSecuencia = " + cotSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ProID in ("+proIds+")");
                }  
            }
            else
            {
                cot.Add("rowguid", Guid.NewGuid().ToString());
                cot.ExecuteInsert();
            }     

            int pos = 1;

            var parEditarPrecio = myParametro.GetParCotizacionesEditarPrecio();
            var parRevenimiento = myParametro.GetParRevenimiento();

            foreach (var det in productos)
            {
                var d = new Hash("CotizacionesDetalle");
                d.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                d.Add("CotSecuencia", cotSecuencia);
                d.Add("ProID", det.ProID);
                d.Add("CliID", Arguments.Values.CurrentClient.CliID);
                d.Add("CotCantidadDetalle", parRevenimiento ? det.CantidadDetalleR : det.CantidadDetalle);

                if (IsMultiEntrega)
                {
                    d.Add("CotFechaEntrega", det.PedFechaEntrega);
                }

                d.Add("CotDesPorciento", det.DesPorciento);
                d.Add("CotCantidad", det.Cantidad);

                if(parEditarPrecio && det.PrecioTemp > 0)
                {
                    d.Add("CotPrecio", det.PrecioTemp);
                }
                else
                {
                    d.Add("CotPrecio", det.Precio);
                }
                
                d.Add("CotItbis", det.Itbis);   
                d.Add("CotSelectivo", det.Selectivo);
                d.Add("CotAdValorem", det.AdValorem);
                d.Add("CotDescuento", det.Descuento);
                d.Add("CotIndicadorCompleto", false);
                d.Add("CotIndicadorOferta", det.IndicadorOferta);
                d.Add("Ofeid", det.OfeID);
                d.Add("UnmCodigo", det.UnmCodigo);
                d.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                d.Add("CotFechaActualizacion", Functions.CurrentDate());
                //d.Add("CotPrecioLista", det.PrecioMoneda);
                //d.Add("CotCantidadOferta", det.CantidadOferta);
                //d.Add("CotDesPorcientoOriginal", det.DesPorcientoManual);

                //if (isEditing && det.IndicadorOferta)
                //{
                //    pos = GetMaxPosicionFromDetalle(cotSecuencia) + 1;

                //    if (posForEdit >= Listpos.Count)
                //    {
                //        continue;
                //    }
                //    d.Add("CotPosicion", Listpos[posForEdit]);
                //    posForEdit++;
                //}else

                if (!isEditing)
                {
                   d.Add("CotPosicion", pos);                
                }

                if (isEditing && ExistsDetalleInCotizacion(det.ProID, cotSecuencia) && !det.IndicadorOferta)
                {
                    
                    d.ExecuteUpdate("ProID = " + det.ProID + " and CotSecuencia = " + cotSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "'");
                }
                else
                {
                    if(isEditing)
                    {
                        d.Remove("CotPosicion");
                        d.Add("CotPosicion", GetMaxPosicionFromDetalle(cotSecuencia) + 1);
                        d.Add("rowguid", Guid.NewGuid().ToString());
                        d.ExecuteInsert();
                        continue;
                    }

                    d.Add("rowguid", Guid.NewGuid().ToString());
                    d.ExecuteInsert();
                }
                pos++;
            }

            if (!isEditing)
            {
                DS_RepresentantesSecuencias.UpdateSecuencia("Cotizaciones", cotSecuencia);
            }

            var de = new Hash("Cotizaciones");
            de.Add("CotTotal", productos.Count);
            de.ExecuteUpdate("CotSecuencia = " + cotSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "'");

            if (DS_RepresentantesParametros.GetInstance().GetParVisitasResultados())
            {
                ActualizarVisitasResultados();
            }

            myProd.ClearTemp((int)Modules.COTIZACIONES);

            return cotSecuencia;
        }

        private void ActualizarVisitasResultados()
        {
            var list = SqliteManager.GetInstance().Query<VisitasResultados>("select 5 as TitID, count(*) as VisCantidadTransacciones, " +
                "sum(((d.CotPrecio + d.CotAdValorem + d.CotSelectivo) - d.CotDescuento) * ((case when d.CotCantidadDetalle > 0 then d.CotCantidadDetalle / o.ProUnidades else 0 end) + d.CotCantidad)) as VisMontoSinItbis, sum(((d.CotItbis / 100.0) * ((d.CotPrecio + d.CotAdValorem + d.CotSelectivo) - d.CotDescuento)) * ((case when d.CotCantidadDetalle > 0 then d.CotCantidadDetalle / o.ProUnidades else 0 end) + d.CotCantidad)) as VisMontoItbis from Cotizaciones p " +
                "inner join CotizacionesDetalle d on d.RepCodigo = p.RepCodigo and d.CotSecuencia = p.CotSecuencia " +
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

        private string GetProIdQueryForDeleteWhileEditing(int cotSecuencia, int titId)
        {
            var list = SqliteManager.GetInstance().Query<CotizacionesDetalle>("select ProID from CotizacionesDetalle " +
                "where CotSecuencia = ? and ltrim(rtrim(RepCodigo)) = ? and ProID not in (select distinct ProID from ProductosTemp where TitID = ?)", 
                new string[] { cotSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim(), titId.ToString() });

            if(list != null && list.Count > 0)
            {
                bool first = true;

                var proIds = "";

                foreach(var pro in list)
                {
                    if (first)
                    {
                        first = false;
                        proIds = pro.ProID.ToString();
                    }
                    else
                    {
                        proIds += ", " + pro.ProID.ToString();
                    }
                }

                return proIds;
            }

            return null;
        }

        private int GetMaxPosicionFromDetalle(int cotSecuencia)
        {
            var list = SqliteManager.GetInstance().Query<CotizacionesDetalle>("select max(CotPosicion) as CotPosicion from CotizacionesDetalle " +
                "where CotSecuencia = ? and ltrim(rtrim(RepCodigo)) = ? ", new string[] { cotSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            if(list != null && list.Count > 0)
            {
                return list[0].CotPosicion;
            }

            return 0;
        }
        private List<CotizacionesDetalle> GetPosicionCotFromDetalle(int cotSecuencia)
        {
            var list = SqliteManager.GetInstance().Query<CotizacionesDetalle>("select CotPosicion from CotizacionesDetalle " +
                "where CotSecuencia = ? and ltrim(rtrim(RepCodigo)) = ?  and CotIndicadorOferta = 1 ", new string[] { cotSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            if(list != null && list.Count > 0)
            {
                return list;
            }

            return new List<CotizacionesDetalle>();
        }

        private bool ExistsDetalleInCotizacion(int ProID, int cotSecuencia)
        {
            return SqliteManager.GetInstance().Query<Cotizaciones>("select CotSecuencia from CotizacionesDetalle where ProID = ? and CotSecuencia = ? and ltrim(rtrim(RepCodigo)) = ? ", 
                new string[] { ProID.ToString(), cotSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() }).Count > 0;
        }

        public void EstCotizacion(string rowguid,int est)
        {
            Hash ped = new Hash("Cotizaciones");
            ped.Add("CotEstatus", est);
            ped.Add("cotFechaActualizacion", Functions.CurrentDate());
            ped.Add("UsuInicioSesion",/* Arguments.CurrentUser.RepCodigo*/"mdsoft");
            //ped.ExecuteUpdate("CotSecuencia = " + cotSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");

            if (est == 0)
            {
                if (new DS_SuscriptoresCambios().UpdateCambioEstadoInsertByRowguid(rowguid, est))
                {
                    ped.SaveScriptForServer = false;
                }
            }

            ped.ExecuteUpdate("rowguid = '" + rowguid + "'");
        }

        public Cotizaciones GetBySecuencia(int cotSecuencia, bool Confirmado)
        {
            var list = SqliteManager.GetInstance().Query<Cotizaciones>("select CotSecuencia, p.CotTotal as CotTotal, p.rowguid as rowguid, CliNombre, p.MonCodigo, p.ConID, " +
                "CliRnc, CliCodigo, CliCalle, CotFecha, CliUrbanizacion, CliCodigo, CliTelefono, p.VisSecuencia as VisSecuencia from " + (Confirmado ? "CotizacionesConfirmados" : "Cotizaciones") + " p " +
                "inner join Clientes c on c.CliID = p.CliID where ltrim(rtrim(p.RepCodigo)) = ? and p.CotSecuencia = ?", new string[] { Arguments.CurrentUser.RepCodigo.Trim(), cotSecuencia.ToString() });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public Cotizaciones GetBySecuencia2(int pedSecuencia, bool pedidoConfirmado)
        {
            var list = SqliteManager.GetInstance().Query<Cotizaciones>("select p.rowguid as rowguid, CotSecuencia,VisSecuencia, c.CliIndicadorPresentacion as CliIndicadorPresentacion,p.CotTotal as CotTotal, p.rowguid as rowguid, CliNombre, CliRnc, CliCodigo, ConDescripcion as RepCodigo, CliCalle, CotFecha, CotFechaEntrega, CliUrbanizacion, p.MonCodigo " +
                //",ifnull(PedTipoPedido,-1) PedTipoPedido " +
                "from " + (pedidoConfirmado ? "CotizacionesConfirmados" : "Cotizaciones") + " p " +
                "inner join Clientes c on c.CliID = p.CliID " +
                "left join CondicionesPago cp on p.ConID = cp.ConID " +
                "where ltrim(rtrim(p.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' " +
                "and p.CotSecuencia = ?", new string[] { pedSecuencia.ToString() });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public Cotizaciones GetBySecuencias(int cotSecuencia, bool Confirmado)
        {
            var list = SqliteManager.GetInstance().Query<Cotizaciones>("select CotSecuencia, p.CotTotal as CotTotal, p.rowguid as rowguid, CliNombre, CliRnc, CliCodigo, CliCalle, CotFecha, CliUrbanizacion, CotCantidadImpresion from " + (Confirmado ? "CotizacionesConfirmados" : "Cotizaciones") + " p " +
                "inner join Clientes c on c.CliID = p.CliID where ltrim(rtrim(p.RepCodigo)) = ? and p.CotSecuencia = ?", new string[] { Arguments.CurrentUser.RepCodigo.Trim(), cotSecuencia.ToString() });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public Cotizaciones GetBySecuenciaSued(int cotSecuencia, bool Confirmado)
        {
            var list = SqliteManager.GetInstance().Query<Cotizaciones>("select CotSecuencia, p.CotTotal as CotTotal, p.rowguid as rowguid, CliNombre, CliCodigo, CliCalle, CotFecha, CliUrbanizacion,CotCantidadImpresion from " + (Confirmado ? "CotizacionesConfirmados" : "Cotizaciones") + " p " +
                "inner join Clientes c on c.CliID = p.CliID where ltrim(rtrim(p.RepCodigo)) = ? and p.CotSecuencia = ?", new string[] { Arguments.CurrentUser.RepCodigo.Trim(), cotSecuencia.ToString() });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public List<CotizacionesDetalle> GetDetalleBySecuencia(int CotSecuencia, bool Confirmado)
        {
            return SqliteManager.GetInstance().Query<CotizacionesDetalle>("select trim(ProDescripcion) as ProDescripcion, d.rowguid as rowguid, " +
                "d.CotDescuento as CotDescuento, d.CotDesPorciento as CotDesPorciento, ifnull(p.ProUnidades, 1) as ProUnidades, ifnull(CotCantidad, 0.0) as CotCantidad, p.ProReferencia as RepCodigo,  " +
                "ifnull(CotCantidadDetalle, 0.0) as CotCantidadDetalle, trim(ifnull(p.ProCodigo, '')) as ProCodigo, d.CotItbis as CotItbis, CotIndicadorOferta, d.UnmCodigo, " +
                "ifnull(CotPrecio, 0.0) as CotPrecio " +
                "from " + (Confirmado ? "CotizacionesDetalleConfirmados" : "CotizacionesDetalle") + " d " +
                "inner join Productos p on p.ProID = d.ProID " +
                "where d.CotSecuencia = ? and ltrim(rtrim(d.RepCodigo)) = ? order by p.ProDescripcion", new string[] { CotSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public List<CotizacionesDetalle> GetDetalleBySecuenciaANDEMP(int CotSecuencia, bool Confirmado)
        {
            return SqliteManager.GetInstance().Query<CotizacionesDetalle>("select trim(ProDescripcion) as ProDescripcion, p.UnmCodigo, d.rowguid as rowguid, " +
                "d.CotDescuento as CotDescuento, d.CotAdValorem, d.CotSelectivo, d.CotDesPorciento as CotDesPorciento, p.ProDescripcion1 as Referencia, ifnull(p.ProUnidades, 1) as ProUnidades, ifnull(CotCantidad, 0.0) as CotCantidad, p.ProCantidadMultiploVenta as ProID, p.ProDatos1 as RepCodigo, " +
                "ifnull(CotCantidadDetalle, 0.0) as CotCantidadDetalle, trim(ifnull(p.ProCodigo, '')) as ProCodigo, d.CotItbis as CotItbis,  " +
                "ifnull(CotPrecio, 0.0) as CotPrecio " +
                "from " + (Confirmado ? "CotizacionesDetalleConfirmados" : "CotizacionesDetalle") + " d " +
                "inner join Productos p on p.ProID = d.ProID " +
                "where d.CotSecuencia = ? and ltrim(rtrim(d.RepCodigo)) = ? order by p.ProDescripcion", new string[] { CotSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public void InsertarCotizacionInTemp(int cotSecuencia, bool confirmado)
        {
            myProd.ClearTemp((int)Modules.COTIZACIONES);

            var query = "";
            if (myParametro.GetParRevenimiento())
            {
                query = "select distinct " + ((int)Modules.COTIZACIONES).ToString() + " as TitID, pd.CotCantidad as Cantidad, pd.CotCantidadDetalle as CantidadDetalleR, pd.rowguid as rowguid, pd.ProID as ProID, pd.cotPrecio as Precio, ifnull(cot.SecCodigo,'') as SecCodigo, " +
                "p.ProDescripcion as Descripcion, pd.CotItbis as Itbis, pd.CotSelectivo as Selectivo, pd.CotDesPorciento as DesPorciento, ifnull(p.UnmCodigo, '') as UnmCodigo, " +
                "ifnull(pd.CotIndicadorOferta, 0) as IndicadorOferta, pd.CotDescuento as Descuento, case when pd.OfeID != 0 then 1 else 0 end as IndicadorOfertaForShow from " + (confirmado ? "CotizacionesDetalleConfirmados" : "CotizacionesDetalle") + " pd " +
                "inner join " + (confirmado ? "CotizacionesConfirmados" : "Cotizaciones") + " cot on cot.CotSecuencia=pd.CotSecuencia and cot.RepCodigo=pd.RepCodigo " +
                "inner join Productos p on p.ProID = pd.ProID where ltrim(rtrim(pd.RepCodigo)) = ? and pd.CotSecuencia = ? order by p.ProDescripcion";
            }
            else if (myParametro.GetParCotOfertasManuales())
            {
                query = "select distinct " + ((int)Modules.COTIZACIONES).ToString() + " as TitID, pd.CotCantidad as Cantidad, pd.CotCantidadDetalle as CantidadDetalle, pd.rowguid as rowguid, pd.ProID as ProID, pd.cotPrecio as Precio, ifnull(cot.SecCodigo,'') as SecCodigo, " +
                  "p.ProDescripcion as Descripcion, pd.CotItbis as Itbis, pd.CotSelectivo as Selectivo, pd.CotDesPorciento as DesPorciento, ifnull(p.UnmCodigo, '') as UnmCodigo, " +
                  "ifnull((select pd2.CotCantidad from " + (confirmado ? "CotizacionesDetalleConfirmados" : "CotizacionesDetalle") + " pd2 where pd2.CotSecuencia=pd.CotSecuencia and pd2.RepCodigo=pd.RepCodigo and  pd2.ProID = pd.ProID and ifnull(pd2.CotIndicadorOferta, 0) = 1),0) as CantidadOferta, " +
                  "ifnull(pd.CotIndicadorOferta, 0) as IndicadorOferta, pd.CotDescuento as Descuento, case when pd.OfeID != 0 then 1 else 0 end as IndicadorOfertaForShow from " + (confirmado ? "CotizacionesDetalleConfirmados" : "CotizacionesDetalle") + " pd " +
                  "inner join " + (confirmado ? "CotizacionesConfirmados" : "Cotizaciones") + " cot on cot.CotSecuencia=pd.CotSecuencia and cot.RepCodigo=pd.RepCodigo " +
                  "inner join Productos p on p.ProID = pd.ProID where ltrim(rtrim(pd.RepCodigo)) = ? and pd.CotSecuencia = ? order by p.ProDescripcion";
            }
            else
            {
                query = "select distinct " + ((int)Modules.COTIZACIONES).ToString() + " as TitID, pd.CotCantidad as Cantidad, pd.CotCantidadDetalle as CantidadDetalle, pd.rowguid as rowguid, pd.ProID as ProID, pd.cotPrecio as Precio, ifnull(cot.SecCodigo,'') as SecCodigo, " +
                  "p.ProDescripcion as Descripcion, pd.CotItbis as Itbis, pd.CotSelectivo as Selectivo, pd.CotDesPorciento as DesPorciento, ifnull(p.UnmCodigo, '') as UnmCodigo, " +
                  "ifnull(pd.CotIndicadorOferta, 0) as IndicadorOferta, pd.CotDescuento as Descuento, case when pd.OfeID != 0 then 1 else 0 end as IndicadorOfertaForShow from " + (confirmado ? "CotizacionesDetalleConfirmados" : "CotizacionesDetalle") + " pd " +
                  "inner join " + (confirmado ? "CotizacionesConfirmados" : "Cotizaciones") + " cot on cot.CotSecuencia=pd.CotSecuencia and cot.RepCodigo=pd.RepCodigo " +
                  "inner join Productos p on p.ProID = pd.ProID where ltrim(rtrim(pd.RepCodigo)) = ? and pd.CotSecuencia = ? order by p.ProDescripcion";
            }


            var list = SqliteManager.GetInstance().Query<ProductosTemp>(query, new string[] { Arguments.CurrentUser.RepCodigo.Trim(), cotSecuencia.ToString() });

            SqliteManager.GetInstance().InsertAll(list);

        }
    }
}
