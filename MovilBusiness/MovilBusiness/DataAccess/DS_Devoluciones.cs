



using MovilBusiness.Configuration;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.model.Internal.Structs.Args;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;

namespace MovilBusiness.DataAccess
{
    public class DS_Devoluciones : DS_Controller
    {
        private DS_Visitas myVis;
        private DS_Productos myProd;
        private DS_CuentasxCobrar myCxCobrar;

        public DS_Devoluciones()
        {
            myVis = new DS_Visitas();
            myProd = new DS_Productos();
            myCxCobrar = new DS_CuentasxCobrar();
        }

        public int SaveDevolucion(DevolucionesArgs? args, bool IsEditing, int editedSecuencia, string camposAdicionales, double subtotal = 0.00, double total = 0.00,  double itbis = 0.00, double totalDescuentoGlobal = 0.00, double porDescuentoGlobal = 0.00)
        {
            int devSecuencia;
            

            if (!IsEditing)
            {
                if (myParametro.GetParDevolucionesSecuenciaSector())
                {
                    devSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Devoluciones-" + Arguments.Values.CurrentSector.SecCodigo);
                }
                else
                {
                    devSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Devoluciones");
                }
                myVis.ActualizarVisitaEfectiva(Arguments.Values.CurrentVisSecuencia);
            }
            else
            {
                devSecuencia = editedSecuencia;
            }

            var productsTemp = myProd.GetResumenProductos((int)Arguments.Values.CurrentModule, false, isForGuardar: true);

            if (args == null)
            {
                throw new Exception("No se cargaron los datos de la devolución");
            }

            var dev = new Hash("Devoluciones");

            if (DS_RepresentantesParametros.GetInstance().GetParHistoricoFacturasFromCuentasxCobrar())
            {
                string ncf = "";
                var documento = args?.Documento;
                if (!string.IsNullOrWhiteSpace(documento))
                { 
                    if (documento.Contains("B"))
                    {
                        ncf = new DS_Ventas().GetNCFdeVentabyNCF(documento);
                    }
                    else
                    {
                        ncf = new DS_Ventas().GetNCFdeVentabyNumeroErp(documento);
                    }
                }
                dev = new Hash("Devoluciones")
                {
                    { "DevEstatus", 1 },
                    { "DevFecha", Functions.CurrentDate() },
                    { "DevTotal", productsTemp.Count },
                    { "DevReferencia", ncf },
                    { "DevNCF", ncf },
                    { "DevAccion", args?.Accion },
                    { "DevCintillo", args?.DevCintillo}
                };
            }
            else
            {
                 dev = new Hash("Devoluciones")
                {
                    { "DevEstatus", 1 },
                    { "DevFecha", Functions.CurrentDate() },
                    { "DevTotal", productsTemp.Count },
                    { "DevReferencia", args?.Documento },
                    { "DevNCF", args?.DevNCF },
                    { "DevAccion", args?.Accion },
                    { "DevCintillo", args?.DevNCF}
                };
            }

            

            if (myParametro.GetParDevolucionesCamposAdicionales() && new Hash().ExistColumnsSchema("Devoluciones", "DevOtrosDatos"))
            {
                dev.Add("DevOtrosDatos", camposAdicionales);
            }

            if (!IsEditing)
            {
                dev.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                dev.Add("DevSecuencia", devSecuencia);
                dev.Add("CliID", Arguments.Values.CurrentClient.CliID);
                dev.Add("VisSecuencia", Arguments.Values.CurrentVisSecuencia);
                dev.Add("CuaSecuencia", Arguments.Values.CurrentCuaSecuencia);
                if (myParametro.GetParSectores() > 0)
                {
                    dev.Add("SecCodigo", Arguments.Values.CurrentSector?.SecCodigo);
                }
            }            

            string devTipo = "1";

            if (args != null && !string.IsNullOrWhiteSpace(args?.DevTipo))
            {
                devTipo = args?.DevTipo;
            }

            dev.Add("DevTipo", devTipo);
            
            dev.Add("DevFechaActualizacion", Functions.CurrentDate());
            dev.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            dev.Add("mbVersion", Functions.AppVersion);
            dev.Add("DevFechaSincronizacion", Functions.CurrentDate()); 
            dev.Add("monCodigo", Arguments.Values.CurrentClient.MonCodigo);

            dev.Add("DevMontoSinITBIS", total - itbis);
            dev.Add("DevMontoITBIS", itbis);
            dev.Add("DevMontoTotal", total);
            dev.Add("DevSubTotal", subtotal);
            dev.Add("DevPorCientoDsctoGlobal", porDescuentoGlobal);
            dev.Add("DevMontoDsctoGlobal", totalDescuentoGlobal);

            if (IsEditing)
            {
                
                dev.ExecuteUpdate("DevSecuencia = " + devSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");
                
                //eliminando los productos que quito de la devolucion o que son ofertas
                var proIds = GetProIdQueryForDeleteWhileEditing(devSecuencia, (int)Arguments.Values.CurrentModule);
                if (!string.IsNullOrWhiteSpace(proIds))
                {
                    var delete = new Hash("DevolucionesDetalle");
                    delete.ExecuteDelete("DevSecuencia = " + devSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ProID in (" + proIds + ")");
                }
            }
            else
            {
                dev.Add("rowguid", Guid.NewGuid().ToString());
                dev.ExecuteInsert();
            }

            var inv = new DS_Inventarios();
            int pos = 1;
            bool parLoteMayusculas = myParametro.GetParDevLoteMayusculas();
            var pos_2 = 0;
            foreach (var det in productsTemp)
            {
                Hash map = new Hash("DevolucionesDetalle");
                map.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                map.Add("DevSecuencia", devSecuencia);
                map.Add("DevTipo", 1);
               
                map.Add("ProID", det.ProID);

                if (parLoteMayusculas && det.Lote != null)
                {
                    map.Add("DevLote", det.Lote.ToUpper());
                }
                else
                {
                    map.Add("DevLote", det.Lote);
                }

                map.Add("DevItebis", det.Itbis);
                map.Add("DevDescuento", det.Descuento);
                map.Add("DevAccion", det.Accion);
                map.Add("MotID", det.MotIdDevolucion);
                map.Add("DevCantidad", det.Cantidad);
                map.Add("DevCantidadDetalle", det.CantidadDetalle);
                map.Add("DevDocumento", det.Documento);
                map.Add("DevIndicadorOferta",det.IndicadorOferta);
                map.Add("CliID", Arguments.Values.CurrentClient.CliID);
                map.Add("DevPrecio", det.Precio);
                map.Add("DevEstatus", 1);
                map.Add("DevFecha", det.FechaVencimiento);
                map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                map.Add("DevFechaActualizacion", Functions.CurrentDate());
                map.Add("DevAdValorem", det.AdValorem);
                map.Add("DevSelectivo", det.Selectivo);
                map.Add("UnmCodigo", det.UnmCodigo);
                map.Add("DevDescPorciento", det.DesPorciento);
                if (myParametro.GetParDevolucionCondicion() || myParametro.GetParDevolucionesCondicionUnico())
                {
                    map.Add("DevCondicion", det.DevCondicion);
                }
                    

                string rowguidEdited = null;
               
                if (IsEditing)
                {
                    rowguidEdited = ExistsDetalleInDevolucion(det.ProID, devSecuencia, det.IndicadorOferta);
                }

                if (IsEditing && rowguidEdited == null)
                {
                    pos_2 += GetPosMaxDevolucion(det.ProID, devSecuencia, det.IndicadorOferta)+1;
                }

                if(IsEditing && rowguidEdited != null)
                {
                    map.ExecuteUpdate(new string[] { "rowguid"},  new DbUpdateValue[] { new DbUpdateValue() { Value = rowguidEdited, IsText = true } }, true);
                }
                else
                {
                    map.Add("DevPosicion", rowguidEdited == null && pos_2 != 0 ? pos_2 : pos); 
                    map.Add("rowguid", Guid.NewGuid().ToString());
                    map.ExecuteInsert();
                }

                if ((myParametro.GetParDevolucionCondicion() || myParametro.GetParDevolucionesCondicionUnico()) && myParametro.GetParAlmacenIdParaMalEstado() > 0 && det.DevCondicion== "2" )
                {
                    inv.AgregarInventario(det.ProID, det.Cantidad, det.CantidadDetalle, myParametro.GetParAlmacenIdParaMalEstado(), parLoteMayusculas && det.Lote != null ? det.Lote.ToUpper() : det.Lote);
                }
                else if((myParametro.GetParDevolucionCondicion() || myParametro.GetParDevolucionesCondicionUnico()) && myParametro.GetParAlmacenIdParaDevolucion() > 0 && det.DevCondicion == "1")
                {
                    inv.AgregarInventario(det.ProID, det.Cantidad, det.CantidadDetalle, myParametro.GetParAlmacenIdParaDevolucion(), parLoteMayusculas && det.Lote != null ? det.Lote.ToUpper() : det.Lote);
                }
                

                pos++;
            }
            bool parNCddev = myParametro.GetParNotaCreditoPorDevolucion();
            var totales = myProd.GetTempTotales((int)Modules.DEVOLUCIONES);

            if (parNCddev && !IsEditing)
            {

                var ncf = new DS_Clientes().GetSiguienteNCF(Arguments.Values.CurrentClient, forNC: true);
                Hash cxc = new Hash("CuentasxCobrar");
                cxc.Add("CxcReferencia", ncf.NCFCompleto);
                cxc.Add("CxcTipoTransaccion", 2);
                cxc.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                cxc.Add("CxcDias", 0);
                cxc.Add("CxcSIGLA", "NC");
                cxc.Add("CliID", Arguments.Values.CurrentClient.CliID);
                cxc.Add("CxcFecha", Functions.CurrentDate());
                cxc.Add("CxcDocumento", Arguments.CurrentUser.RepCodigo + "-" + devSecuencia);
                cxc.Add("CxcBalance", totales.Total);
                cxc.Add("CxcMontoSinItbis", totales.SubTotal);
                cxc.Add("CxcMontoTotal", totales.Total);
                cxc.Add("MonCodigo", Arguments.Values.CurrentClient.MonCodigo);
                cxc.Add("AreaCtrlCredit", 0);
                cxc.Add("CxcNotaCredito", 0);
                cxc.Add("CXCNCF", ncf.NCFCompleto);
                cxc.Add("rowguid", Guid.NewGuid().ToString());
                cxc.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                cxc.Add("CueFechaActualizacion", Functions.CurrentDate());
                cxc.Add("CxcFechaVencimiento", myCxCobrar.GetCxcFechaVencimiento(Arguments.Values.CurrentClient.ConID));
                cxc.Add("ConID", Arguments.Values.CurrentClient.ConID);
                ActualizarNcfDpp(ncf.Secuencia.ToString(), ncf.rowguid);
                cxc.ExecuteInsert();


            }

            if (!IsEditing && editedSecuencia != -1)
            {
                if (myParametro.GetParDevolucionesSecuenciaSector())
                {
                    DS_RepresentantesSecuencias.UpdateSecuencia("Devoluciones-" + Arguments.Values.CurrentSector.SecCodigo, devSecuencia);
                }
                else
                {
                    DS_RepresentantesSecuencias.UpdateSecuencia("Devoluciones", devSecuencia);
                }
            }

            if (DS_RepresentantesParametros.GetInstance().GetParVisitasResultados())
            {
                ActualizarVisitasResultados();
            }

            myProd.ClearTemp((int)Arguments.Values.CurrentModule);

            return devSecuencia;
        }


        private void ActualizarNcfDpp(string NCFDPP, string rowguid)
        {
            Hash n = new Hash(" " + (myParametro.GetParTakeFromNCF2021() ? "RepresentantesDetalleNCF2021" : "RepresentantesDetalleNCF2018") + " ");
            n.Add("ReDNCFActual", NCFDPP, true);
            n.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            n.ExecuteUpdate("rowguid = '" + rowguid + "' ");
        }

        private void ActualizarVisitasResultados()
        {
            var list = SqliteManager.GetInstance().Query<VisitasResultados>("select 2 as TitID, count(*) as VisCantidadTransacciones, " +
                "sum(((d.DevPrecio + d.DevAdValorem + d.DevSelectivo) - d.DevDescuento) * ((case when d.DevCantidadDetalle > 0 then d.DevCantidadDetalle / o.ProUnidades else 0 end) + d.DevCantidad)) as VisMontoSinItbis, sum(((d.DevItebis / 100.0) * ((d.DevPrecio + d.DevAdValorem + d.DevSelectivo) - d.DevDescuento)) * ((case when d.DevCantidadDetalle > 0 then d.DevCantidadDetalle / o.ProUnidades else 0 end) + d.DevCantidad)) as VisMontoItbis from Devoluciones p " +
                "inner join DevolucionesDetalle d on d.RepCodigo = p.RepCodigo and d.DevSecuencia = p.DevSecuencia " +
                "inner join Productos o on o.ProID = d.ProID " +
                "where p.VisSecuencia = ? and p.RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'", new string[] { Arguments.Values.CurrentVisSecuencia.ToString() });

            if (list != null && list.Count > 0)
            {
                var item = list[0];

                item.VisMontoTotal = item.VisMontoSinItbis + item.VisMontoItbis;
                item.VisComentario = "";

                myVis.GuardarVisitasResultados(item);
            }
        }

        public bool GetProductoProDatos3(ProductosTemp pro)
        {
            var list = SqliteManager.GetInstance().Query<Productos>("Select ProID from Productos where Proid = " + pro.ProID.ToString() + " AND ProDatos3 like '%A%'");
            if(list.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string GetProIdQueryForDeleteWhileEditing(int devSecuencia, int titId)
        {
            var list = SqliteManager.GetInstance().Query<DevolucionesDetalle>("select ProID from DevolucionesDetalle " +
                "where DevSecuencia = ? and ltrim(rtrim(RepCodigo)) = ? and (ProID not in (select distinct ProID from ProductosTemp where TitID = "+titId.ToString()+") " +
                "or ifnull(DevIndicadorOferta, 0) = 1)",
                new string[] { devSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            if (list != null && list.Count > 0)
            {
                bool first = true;

                var proIds = "";

                foreach (var pro in list)
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

        private string ExistsDetalleInDevolucion(int proId, int devSecuencia, bool indicadorOferta)
        {
            var list = SqliteManager.GetInstance().Query<Devoluciones>("select rowguid from DevolucionesDetalle " +
             "where ProID = ? and DevSecuencia = ? and ltrim(rtrim(RepCodigo)) = ?  and ifnull(DevIndicadorOferta, 0) = " + (indicadorOferta ? "1" : "0") + " ",
             new string[] { proId.ToString(), devSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });

            if(list != null && list.Count > 0)
            {
                return list[0].rowguid;
            }

            return null;

        }

        public List<DevolucionesDetalle> GetDevolucionesDetalleBySecuencia(int devSecuencia, bool devolucionConfirmados, bool dateMonthYear = false, bool forConduce = false)
        {
            var where = "";

            if(forConduce)
            {
                where = " and ifnull(d.DevIndicadorOferta, 0) = 0 and (ifnull(d.DevCantidad, 0) - ifnull(d.DevCantidadConfirmada, 0) > 0 or ifnull(d.DevCantidadDetalle, 0) - ifnull(d.DevCantidadDetalleConfirmada, 0) > 0) ";
            }

            return SqliteManager.GetInstance().Query<DevolucionesDetalle>("select ltrim(rtrim(ProDescripcion)) as ProDescripcion, ltrim(rtrim(ProCodigo)) as ProCodigo, DevAccion, " +
                (forConduce ? "ifnull(DevCantidad, 0) - ifnull(DevCantidadConfirmada, 0) as DevCantidad, ifnull(DevCantidadDetalle, 0) - ifnull(DevCantidadDetalleConfirmada, 0) as DevCantidadDetalle, " : "DevCantidad, DevCantidadDetalle, ") + " ifnull(DevDocumento, '') as DevDocumento, ifnull(mv.MotReferencia, '') as MotReferencia, " +
                "ifnull(DevLote, '') as DevLote, DevPrecio, d.DevPosicion, DevItebis, DevDescuento, DevDescPorciento, ifnull(strftime('" + (dateMonthYear ? "%m/%Y" : "%d-%m-%Y") + "', DevFecha), '') as DevFecha, " +
                "p.ProUnidades as ProUnidades, " + (devolucionConfirmados ? "DevNumeroERP, " : "") + "SUBSTR(ifnull(mv.MotDescripcion, ''),1,60)  as MotDescripcion, DevIndicadorOferta, mv.MotID " +
                "from " + (devolucionConfirmados ? "DevolucionesDetalleConfirmadas" : "DevolucionesDetalle") + " d " +
                "inner join Productos p on p.ProID = d.ProID " +
                "left Join MotivosDevolucion mv on  d.MotID = mv.MotID  " +
                "where DevSecuencia = ? and d.RepCodigo = ? "+where+" Order by DevPosicion", new string[] { devSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });
        }
        
        public List<DevolucionesDetalle> GetDevolucionesDetalleBySecuenciaToUseProId(int devSecuencia, bool devolucionConfirmados, bool dateMonthYear = false, bool forConduce = false)
        {
            var where = "";

            if(forConduce)
            {
                where = " and ifnull(d.DevIndicadorOferta, 0) = 0 and (ifnull(d.DevCantidad, 0) - ifnull(d.DevCantidadConfirmada, 0) > 0 or ifnull(d.DevCantidadDetalle, 0) - ifnull(d.DevCantidadDetalleConfirmada, 0) > 0) ";
            }

            return SqliteManager.GetInstance().Query<DevolucionesDetalle>("select ltrim(rtrim(ProDescripcion)) as ProDescripcion, d.ProID as ProID, ltrim(rtrim(ProCodigo)) as ProCodigo, " +
                (forConduce ? "ifnull(DevCantidad, 0) - ifnull(DevCantidadConfirmada, 0) as DevCantidad, ifnull(DevCantidadDetalle, 0) - ifnull(DevCantidadDetalleConfirmada, 0) as DevCantidadDetalle, " : "DevCantidad, DevCantidadDetalle, ") + " ifnull(DevDocumento, '') as DevDocumento, ifnull(mv.MotReferencia, '') as MotReferencia, " +
                "ifnull(DevLote, '') as DevLote, DevPrecio, d.DevPosicion, DevItebis, DevDescuento, ifnull(strftime('"+(dateMonthYear ? "%m/%Y" : "%d-%m-%Y") + "', DevFecha), '') as DevFecha, " +
                "p.ProUnidades as ProUnidades, " + (devolucionConfirmados ? "DevNumeroERP, " : "") + "SUBSTR(ifnull(mv.MotDescripcion, ''),1,60)  as MotDescripcion, DevIndicadorOferta  " +
                "from " + (devolucionConfirmados ? "DevolucionesDetalleConfirmadas" : "DevolucionesDetalle") + " d " +
                "inner join Productos p on p.ProID = d.ProID " +
                "left Join MotivosDevolucion mv on  d.MotID = mv.MotID  " +
                "where DevSecuencia = ? and d.RepCodigo = ? "+where+" Order by DevPosicion", new string[] { devSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });
        }

        public string GetMotivosDevolucionFromDetalle(int devSecuencia, bool confirmada)
        {
            try
            {
                var result = "";

                var list = SqliteManager.GetInstance().Query<MotivosDevolucion>("select SUBSTR(ifnull(MotDescripcion, ''),1,60) as MotDescripcion from MotivosDevolucion m " +
                    "inner join " + (confirmada ? "DevolucionesDetalleConfirmadas" : "DevolucionesDetalle") + " d on d.MotID = m.MotID where d.DevSecuencia = ? and ltrim(rtrim(d.RepCodigo)) = ?", new string[] { devSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });

                var firstTime = true;

                foreach (var motivo in list)
                {
                    if (firstTime)
                    {
                        firstTime = false;
                        result = motivo.MotDescripcion;
                    }
                    else
                    {
                        result += ", " + motivo.MotDescripcion;
                    }
                }

                return result;
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }
            return "";
        }

        public void EstDevolucion(string rowguid, int est)
        {
            var ped = new Hash("Devoluciones");
            ped.Add("DevEstatus", est);
            ped.Add("UsuInicioSesion", /*Arguments.CurrentUser.RepCodigo*/"mdsoft");

            if (est == 0)
            {
                if (new DS_SuscriptoresCambios().UpdateCambioEstadoInsertByRowguid(rowguid, est))
                {
                    ped.SaveScriptForServer = false;
                }
            }

            ped.ExecuteUpdate("rowguid = '" + rowguid + "'");
            //ped.ExecuteUpdate("DevSecuencia = " + devSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");
        }

        public Devoluciones GetDevolucionBySecuencia(int devSecuencia, bool devolucionConfirmado)
        {
            List<Devoluciones> list = SqliteManager.GetInstance().Query<Devoluciones>("select CliID, SUBSTR(ifnull(MotDescripcion, ''),1,60) as Motivo, ifnull(DevReferencia, '') as DevReferencia, strftime('%d-%m-%Y', DevFecha) as DevFecha, strftime('%m-%Y', DevFecha) as DevFechaFormatted, ifnull(ltrim(rtrim(RepCodigo)), '') as RepCodigo, DevSecuencia, DevCintillo, DevAccion, DevCantidadImpresion, ifnull(d.rowguid,'') as rowguid " +
                "from " + (devolucionConfirmado ? "DevolucionesConfirmadas" : "Devoluciones") + " d left join MotivosDevolucion m on m.MotReferencia = d.DevReferencia " +
                "where DevSecuencia = ? and RepCodigo = ?",
                new string[] { devSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });

            if (list != null & list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public Devoluciones GetDevolucionBySecuenciaConTotales(int devSecuencia, bool devolucionConfirmado)
        {
            List<Devoluciones> list = SqliteManager.GetInstance().Query<Devoluciones>("select CliID, SUBSTR(ifnull(MotDescripcion, ''),1,60) as Motivo, ifnull(DevReferencia, '') as DevReferencia, strftime('%d-%m-%Y', DevFecha) as DevFecha, ifnull(ltrim(rtrim(RepCodigo)), '') as RepCodigo, DevSecuencia, DevCintillo, DevAccion, DevCantidadImpresion, d.MonCodigo, ifnull(d.rowguid,'') as rowguid " +
                ", ifnull(DevMontoSinITBIS,0) as DevMontoSinITBIS, ifnull(DevMontoITBIS,0) as DevMontoITBIS, ifnull(DevMontoTotal,0) as DevMontoTotal, ifnull(DevSubTotal,0) as DevSubTotal, ifnull(DevPorCientoDsctoGlobal,0) as DevPorCientoDsctoGlobal, ifnull(DevMontoDsctoGlobal,0) as DevMontoDsctoGlobal from " + (devolucionConfirmado ? "DevolucionesConfirmadas" : "Devoluciones") + " d left join MotivosDevolucion m on m.MotReferencia = d.DevReferencia " +
                "where DevSecuencia = ? and RepCodigo = ?",
                new string[] { devSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });

            if (list != null & list.Count > 0)
            {
                return list[0];
            }

            return null;
        }


        public Devoluciones GetDevolucionBySecuenciaMang(int devSecuencia, bool devolucionConfirmado)
        {

            List<Devoluciones> list = new List<Devoluciones>();

            if (!new Hash().ExistColumnsSchema((devolucionConfirmado ? "DevolucionesConfirmadas" : "Devoluciones"), "DevOtrosDatos"))
            {
                 list = SqliteManager.GetInstance().Query<Devoluciones>("select CliID, SUBSTR(ifnull(MotDescripcion, ''),1,60) as Motivo, ifnull(DevReferencia, '') as DevReferencia, strftime('%d-%m-%Y', DevFecha) as DevFecha, ifnull(ltrim(rtrim(RepCodigo)), '') as RepCodigo, DevSecuencia, DevCintillo, DevAccion, DevCantidadImpresion, ifnull(d.rowguid,'') as rowguid " +
                    "from " + (devolucionConfirmado ? "DevolucionesConfirmadas" : "Devoluciones") + " d left join MotivosDevolucion m on m.MotReferencia = d.DevReferencia " +
                    "where DevSecuencia = ? and RepCodigo = ?",
                    new string[] { devSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });
            }else
            {
                 list = SqliteManager.GetInstance().Query<Devoluciones>("select CliID, SUBSTR(ifnull(MotDescripcion, ''),1,60) as Motivo, ifnull(DevReferencia, '') as DevReferencia, strftime('%d-%m-%Y', DevFecha) as DevFecha, ifnull(ltrim(rtrim(RepCodigo)), '') as RepCodigo, DevSecuencia, DevCintillo, DevAccion, DevCantidadImpresion, ifnull(d.rowguid,'') as rowguid,ifnull(d.DevOtrosDatos,'') as DevOtrosDatos " +
                    "from " + (devolucionConfirmado ? "DevolucionesConfirmadas" : "Devoluciones") + " d left join MotivosDevolucion m on m.MotReferencia = d.DevReferencia " +
                    "where DevSecuencia = ? and RepCodigo = ?",
                    new string[] { devSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });
            }

            if (list != null & list.Count > 0)
            {
                return list[0];
            }

            return null;
        }
        //Temporal
        public Devoluciones GetDevolucionBySecuenciaSued(int devSecuencia, bool devolucionConfirmado)
        {
            List<Devoluciones> list = SqliteManager.GetInstance().Query<Devoluciones>("select CliID, SUBSTR(ifnull(MotDescripcion, ''),1,60) as Motivo, ifnull(DevReferencia, '') as DevReferencia, strftime('%d-%m-%Y', DevFecha) as DevFecha, ifnull(ltrim(rtrim(RepCodigo)), '') as RepCodigo, DevSecuencia, DevCantidadImpresion " +
                "from " + (devolucionConfirmado ? "DevolucionesConfirmadas" : "Devoluciones") + " d left join MotivosDevolucion m on m.MotReferencia = d.DevReferencia " +
                "where DevSecuencia = ? and RepCodigo = ?",
                new string[] { devSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });

            if (list != null & list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public List<MotivosDevolucion> GetMotivosDevolucion()
        {
            try
            {
                return SqliteManager.GetInstance().Query<MotivosDevolucion>("select MotID, SUBSTR(MotDescripcion,1,60) MotDescripcion, ifnull(MotCaracteristicas, '') as MotCaracteristicas, MotCamposObligatorios from MotivosDevolucion where MotEstatus = 1 order by MotDescripcion", new string[] { });
            }
            catch(Exception e)
            {
                Console.Write(e.Message);
                return SqliteManager.GetInstance().Query<MotivosDevolucion>("select MotID, SUBSTR(MotDescripcion,1,60) MotDescripcion, ifnull(MotCaracteristicas, '') as MotCaracteristicas, MotCamposObligatorios from MotivosDevolucion where MotEstatus = 1 order by MotDescripcion", new string[] { });
            }
            
        }

        public MotivosDevolucion GetMotivoDevolucionbyId(int motId)
        {
            List<MotivosDevolucion> list = SqliteManager.GetInstance().Query<MotivosDevolucion>("select MotID, SUBSTR(MotDescripcion,1,60) MotDescripcion, ifnull(MotCaracteristicas, '') as MotCaracteristicas, MotCamposObligatorios from MotivosDevolucion where MotID = ?", new string[] { motId.ToString() });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        /** al intentar agregar un producto valida si el motivo de devolucion es diferente al de los productos agregados de ser asi y si tiene el parametro 
         de motivo unico no permite agregarlo*/
        public bool ValidarMotivoUnico(int motId)
        {
            try
            {
                return SqliteManager.GetInstance().Query<MotivosDevolucion>("select MotIdDevolucion as MotID from ProductosTemp " +
                    "where MotIdDevolucion <> ? and TitID = ? AND (Cantidad > 0 Or CantidadDetalle > 0)", new string[] { motId.ToString(), ((int)Modules.DEVOLUCIONES).ToString() }).Count == 0;

            }catch(Exception e)
            {
                Console.Write(e.Message);
                return true;
            }
   
        }

        public bool ValidarFechaVencimientoContraPoliticaDevolucion(int proId, string fecha)
        {
            List<PoliticasDevolucionDetalle> list = SqliteManager.GetInstance().Query<PoliticasDevolucionDetalle>("select ifnull(PodTipo, -1) as PodTipo, " +
                "ifnull(PodCantidadMeses, -1) as PodCantidadMeses from PoliticasDevolucionDetalle where PodID in (select PodID from PoliticasDevolucion " +
                "where PodEstado = 1 and datetime('now', '" + Functions.GetDiferenciaHorariaSqlite() + " hours') > PodFechaInicio) " +
                "and ProID = ? or ProID = 0 order by ProID desc", new string[] { proId.ToString() });

            if (list == null || list.Count == 0)
            {
                return true;
            }

            PoliticasDevolucionDetalle politica = list[0];

            string query = "";

            string formattedDate = Functions.FormatDate(fecha, "yyyy-MM-dd");

            if (politica.PodTipo == 1)
            {
                query = "select date('" + formattedDate + "', '-" + politica.PodCantidadMeses + " day') as FechaVencimiento ";
            }
            else if (politica.PodTipo == 2)
            {
                query = "select date('" + formattedDate + "', '+" + politica.PodCantidadMeses + " day') as FechaVencimiento";                
            }
            else if (politica.PodTipo == 3)
            {
                query = "select strftime('%Y-%m-%d', '" + Functions.CurrentDate("yyyy-MM-dd") + "') as FechaVencimiento, " +
                    "date('" + formattedDate + "', '-" + politica.PodCantidadMeses + " day') as FechaAntesVencimiento, " +
                    "date('" + formattedDate + "', '+" + politica.PodCantidadMeses + " day') as FechaDespuesVencimiento";
            }

            if (query.Trim().Length == 0)
            {
                return true;
            }

            

            List<ProductosTemp> listFechaRestante = SqliteManager.GetInstance().Query<ProductosTemp>(query, new string[] { });

            if (list != null && list.Count > 0)
            {
                string fechaRestante = Functions.FormatDate(listFechaRestante[0].FechaVencimiento, "yyyy-MM-dd");
                string fechaRestanteAntes = Functions.FormatDate(listFechaRestante[0].FechaAntesVencimiento, "yyyy-MM-dd");
                string fechaRestanteDespues = Functions.FormatDate(listFechaRestante[0].FechaDespuesVencimiento, "yyyy-MM-dd");


                query = "select 1 as FechaVencimiento where strftime('%Y-%m-%d', '" + Functions.CurrentDate("yyyy-MM-dd") + "') between ";

                if (politica.PodTipo == 1)
                {

                    query += "strftime('%Y-%m-%d', '" + fechaRestante + "') and strftime('%Y-%m-%d', '" + formattedDate + "')";

                }
                else if (politica.PodTipo == 2)
                {
                    query += "strftime('%Y-%m-%d', '" + formattedDate + "') and strftime('%Y-%m-%d', '" + fechaRestante + "')";
                    //DateTime fechaCapturada = DateTime.Parse(fecha).Date;
                    //if (fechaCapturada >= DateTime.Now.Date)
                    //{
                    //    return true;
                    //}
                }
                else if (politica.PodTipo == 3)
                {
                    query += "strftime('%Y-%m-%d', '" + fechaRestanteAntes + "') and strftime('%Y-%m-%d', '" + formattedDate + "') " +
                        "or strftime('%Y-%m-%d', '" + Functions.CurrentDate("yyyy-MM-dd") + "') between " +
                        "strftime('%Y-%m-%d', '" + formattedDate + "') and strftime('%Y-%m-%d', '" + fechaRestanteDespues + "')";
                }

                if (query.Length == 0)
                {
                    return false;
                }

                return SqliteManager.GetInstance().Query<ProductosTemp>(query, new string[] { }).Count > 0;
            }

            return false;
        }

        public void InsertarDevolucionInTemp(int devSecuencia, bool confirmado)
        {
            myProd.ClearTemp((int)Modules.DEVOLUCIONES);
            SqliteManager.GetInstance().Execute("delete from ProductosTemp", new string[] { });

             var query = "select distinct "+((int)Modules.DEVOLUCIONES)+" as TitID, pd.DevCantidad as Cantidad, pd.rowguid as rowguid, pd.DevCantidadDetalle as CantidadDetalle, pd.ProID as ProID, pd.DevPrecio as Precio, " +
                 "p.ProDescripcion as Descripcion, pd.DevItebis as Itbis, pd.DevFecha as FechaVencimiento, pd.DevDocumento as Documento, pd.MotID as MotIdDevolucion, pd.DevLote as Lote, " +
                 "pd.DevAccion as Accion, pd.DevSelectivo as Selectivo, ifnull(p.UnmCodigo, '') as UnmCodigo, " +
                 "ifnull(pd.DevIndicadorOferta, 0) as IndicadorOferta, pd.DevDescuento as Descuento, ofe.DevCantidad as CantidadOferta, d.SecCodigo as SecCodigo " +
                 "from " + (confirmado ? "DevolucionesDetalleConfirmadas" : "DevolucionesDetalle") + " pd " +
                 "inner join " + (confirmado ? "DevolucionesConfirmadas" : "Devoluciones") + " d on d.DevSecuencia = pd.DevSecuencia and ltrim(rtrim(d.RepCodigo)) = ltrim(rtrim(pd.RepCodigo)) " +
                 "inner join Productos p on p.ProID = pd.ProID " +
                 "left join (select sum(DevCantidad) as DevCantidad, ProID from "+(confirmado? "DevolucionesDetalleConfirmadas" : "DevolucionesDetalle") +" " +
                 "where trim(RepCodigo) = ? and DevSecuencia = ? and ifnull(DevIndicadorOferta, 0) = 1 group by ProID) ofe on ofe.ProID = pd.ProID " +
                 "where ltrim(rtrim(pd.RepCodigo)) = ? and pd.DevSecuencia = ? " +
                 "and ifnull(pd.DevIndicadorOferta, 0) = 0 " +
                 "order by p.ProDescripcion";

            var list = SqliteManager.GetInstance().Query<ProductosTemp>(query, new string[] { Arguments.CurrentUser.RepCodigo.Trim(),
                devSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim(), devSecuencia.ToString() });
           
            SqliteManager.GetInstance().InsertAll(list);
        }


        public List<MotivosDevolucion> GetDevolucionDetalleMotivo(int devSecuencia, bool confirmada)
        {
            try
            {
             
              return SqliteManager.GetInstance().Query<MotivosDevolucion>("SELECT DISTINCT MD.MotId, SUBSTR(MD.MotDescripcion,1,60) MotDescripcion FROM  " + (confirmada ? "DevolucionesDetalleConfirmadas DD" : "DevolucionesDetalle DD")   +
                  " INNER JOIN MotivosDevolucion MD ON MD.MotID = DD.MotID  where DD.DevSecuencia = ? and ltrim(rtrim(DD.RepCodigo)) = ? Order by DD.DevPosicion", new string[] {devSecuencia.ToString(), Arguments.CurrentUser.RepCodigo});

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            return null;
        }

        public List<UsosMultiples> GetDevolucionDetalleCondicion(int devSecuencia, bool confirmada)
        {
            try
            {

                return SqliteManager.GetInstance().Query<UsosMultiples>("SELECT DISTINCT DevCondicion as CodigoUso, Descripcion FROM  " + (confirmada ? "DevolucionesDetalleConfirmadas DD" : "DevolucionesDetalle DD") +
                    " INNER JOIN UsosMultiples u ON CodigoUso = DevCondicion and trim(upper(CodigoGrupo)) = 'DEVCONDICION'  where DD.DevSecuencia = ? and ltrim(rtrim(DD.RepCodigo)) = ? Order by DD.DevPosicion", new string[] { devSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            return null;
        }

        public string getValidarFechaVencimientoConProDatos3(int ProId)
        {
            List<Productos> list = SqliteManager.GetInstance().Query<Productos>("select ifnull(ProDatos3, '') from Productos where ProID = ?", new string[] { ProId.ToString() });
            if (list != null && list.Count > 0)
            {
                return list[0].ProDatos3;
            }
           
            return "";
        }

        /*public List<Devoluciones> GetDevolucionesParaConduce(string secCodigo = null)
        {
            var where = "";

            if(!string.IsNullOrWhiteSpace(secCodigo))
            {
                where = " and trim(d.SecCodigo) = '"+secCodigo.Trim()+"' ";
            }


            var query = "select RepCodigo, DevSecuencia, DevFecha, CliID, DevTipo, DevNCF, DevReferencia " +
                "from Devoluciones d where trim(RepCodigo) = ? and DevEstatus <> 0 " + where + " and DevSecuencia in (select distinct DevSecuencia from DevolucionesDetalle where RepCodigo = d.RepCodigo " +
                "and DevSecuencia = d.DevSecuencia and ifnull(DevIndicadorOferta, 0) = 0 " +
                "and ((ifnull(DevCantidad, 0) - ifnull(DevCantidadConfirmada, 0) > 0) or (ifnull(DevCantidadDetalle, 0) - ifnull(DevCantidadDetalleConfirmada, 0) > 0)))";

            return SqliteManager.GetInstance().Query<Devoluciones>(query, new string[] { Arguments.CurrentUser.RepCodigo.Trim() });
        }*/

        public List<Devoluciones> getDevolucionesRealizados(int cuaSecuencia)
        {
            /*

            string query = $@"select  d.DevSecuencia, d.CliID,  c.CliCodigo, c.CliNombre, ifnull(sum( cast(dd.DevPrecio as float)  * 
							  ((cast((dd.DevCantidad - ifnull(DevCantidadConfirmada,0)) as float) + (ifnull(dd.DevCantidadDetalle,0)-ifnull(DevCantidadDetalleConfirmada,0))/p.ProUnidades)) +  cast(DevTotalItebis as float)),0) as DevMontoTotal 
							  from Devoluciones d
							  inner join DevolucionesDetalle dd on dd.Repcodigo = d.Repcodigo and dd.DevSecuencia = d.DevSecuencia
							  inner join Productos p on p.Proid = dd.Proid
                              inner join Clientes c on c.Cliid  = d.CliiD where d.Repcodigo = '{Arguments.CurrentUser.RepCodigo}' and d.CuaSecuencia = {cuaSecuencia}
                              and d.DevEstatus != 0
							  group by  d.DevSecuencia, d.CliId, c.CliCodigo, c.CliNombre
                              having sum((dd.DevCantidad - ifnull(dd.DevCantidadConfirmada,0))) > 0	
                              UNION							  
                              select  d.DevSecuencia, d.CliID,  c.CliCodigo, c.CliNombre, ifnull(sum( cast(dd.DevPrecio as float)  * 
							  ((cast((dd.DevCantidad - ifnull(DevCantidadConfirmada,0)) as float) + (ifnull(dd.DevCantidadDetalle,0)-ifnull(DevCantidadDetalleConfirmada,0))/p.ProUnidades)) +  cast(DevTotalItebis as float)),0)  as DevMontoTotal 
							  from DevolucionesConfirmadas d
							  inner join DevolucionesDetalleConfirmadas dd on dd.Repcodigo = d.Repcodigo and dd.DevSecuencia = d.DevSecuencia
							  inner join Productos p on p.Proid = dd.Proid
                              inner join Clientes c on c.Cliid  = d.CliiD where d.Repcodigo = '{Arguments.CurrentUser.RepCodigo}' and d.CuaSecuencia =  {cuaSecuencia}
							  and d.DevEstatus != 0
                              group by  d.DevSecuencia, d.CliId, c.CliCodigo, c.CliNombre	
                              having sum((dd.DevCantidad - ifnull(dd.DevCantidadConfirmada,0))) > 0	";

            */
            string query = $@"select  d.DevSecuencia, d.CliID,  c.CliCodigo, c.CliNombre, 
                              ifnull(sum((((cast(dd.DevPrecio as float) + cast(dd.DevAdValorem as float) - cast(dd.DevDescuento as float)) 
							+ (cast(dd.DevPrecio as float)   + cast(dd.DevAdValorem as float) - cast(dd.DevDescuento as float)) * (dd.DevItebis /100.0))
							* ((cast((dd.DevCantidad - ifnull(DevCantidadConfirmada,0)) as float) + (ifnull(dd.DevCantidadDetalle,0)-ifnull(DevCantidadDetalleConfirmada,0))/p.ProUnidades))  ) ),0)  as DevMontoTotal  
							  from Devoluciones d
							  inner join DevolucionesDetalle dd on dd.Repcodigo = d.Repcodigo and dd.DevSecuencia = d.DevSecuencia
							  inner join Productos p on p.Proid = dd.Proid
                              inner join Clientes c on c.Cliid  = d.CliiD where d.Repcodigo = '{Arguments.CurrentUser.RepCodigo}' and d.CuaSecuencia = {cuaSecuencia}
                              and d.DevEstatus != 0
							  group by  d.DevSecuencia, d.CliId, c.CliCodigo, c.CliNombre
                              having sum((dd.DevCantidad - ifnull(dd.DevCantidadConfirmada,0))) > 0	
                              UNION							  
                              select  d.DevSecuencia, d.CliID,  c.CliCodigo, c.CliNombre, 
                            ifnull(sum((((cast(dd.DevPrecio as float) + cast(dd.DevAdValorem as float) - cast(dd.DevDescuento as float)) 
							+ (cast(dd.DevPrecio as float)   + cast(dd.DevAdValorem as float) - cast(dd.DevDescuento as float)) * (dd.DevItebis /100.0))
							* ((cast((dd.DevCantidad - ifnull(DevCantidadConfirmada,0)) as float) + (ifnull(dd.DevCantidadDetalle,0)-ifnull(DevCantidadDetalleConfirmada,0))/p.ProUnidades))  ) ),0)  as DevMontoTotal  
							  from DevolucionesConfirmadas d
							  inner join DevolucionesDetalleConfirmadas dd on dd.Repcodigo = d.Repcodigo and dd.DevSecuencia = d.DevSecuencia
							  inner join Productos p on p.Proid = dd.Proid
                              inner join Clientes c on c.Cliid  = d.CliiD where d.Repcodigo = '{Arguments.CurrentUser.RepCodigo}' and d.CuaSecuencia =  {cuaSecuencia}
							  and d.DevEstatus != 0
                              group by  d.DevSecuencia, d.CliId, c.CliCodigo, c.CliNombre	
                              having sum((dd.DevCantidad - ifnull(dd.DevCantidadConfirmada,0))) > 0	";

            return SqliteManager.GetInstance().Query<Devoluciones>(query, new string[] { });
        }
        public List<DevolucionesDetalle> getProductosDevolucionesRealizadas(int cuaSecuencia)
        {
            /*
            string query = $@"select dd.proid, p.ProCodigo, p.ProDescripcion, sum(dd.DevCantidad + (ifnull(dd.DevCantidadDetalle,0) / p.ProUnidades)) DevCantidad    
			   from DevolucionesDetalle dd
			   inner join productos p on p.proid = dd.Proid	
			   inner join Devoluciones d on d.Repcodigo = dd.Repcodigo and d.DevSecuencia = dd.DevSecuencia
			   where dd.Repcodigo ='{Arguments.CurrentUser.RepCodigo}' and d.CuaSecuencia=  {cuaSecuencia}
               and d.DevEstatus != 0
               and (dd.DevCantidad - ifnull(dd.DevCantidadConfirmada,0)) > 0
			   group by dd.proid, p.ProCodigo, p.ProDescripcion			   
			   union 			   
			   select dd.proid, p.ProCodigo, p.ProDescripcion, sum(dd.DevCantidad + (ifnull(dd.DevCantidadDetalle,0) / p.ProUnidades)) DevCantidad    
			   from DevolucionesDetalleConfirmadas dd
			   inner join productos p on p.proid = dd.Proid	
			   inner join DevolucionesConfirmadas d on d.Repcodigo = dd.Repcodigo and d.DevSecuencia = dd.DevSecuencia
			   where dd.Repcodigo ='{Arguments.CurrentUser.RepCodigo}' and d.CuaSecuencia= {cuaSecuencia}
               and d.DevEstatus != 0
               and (dd.DevCantidad - ifnull(dd.DevCantidadConfirmada,0)) > 0
			   group by dd.proid, p.ProCodigo, p.ProDescripcion	";
            */
            string query = $@"select dd.proid, p.ProCodigo, p.ProDescripcion, p.ProUnidades, 
               sum((dd.DevCantidad -ifnull(DevCantidadConfirmada,0)) + ( (ifnull(dd.DevCantidadDetalle,0)- ifnull(DevCantidadDetalleConfirmada,0)) / p.ProUnidades)) DevCantidad    
			   from DevolucionesDetalle dd
			   inner join productos p on p.proid = dd.Proid	
			   inner join Devoluciones d on d.Repcodigo = dd.Repcodigo and d.DevSecuencia = dd.DevSecuencia
			   where dd.Repcodigo ='{Arguments.CurrentUser.RepCodigo}' and d.CuaSecuencia=  {cuaSecuencia}
               and d.DevEstatus != 0
               and (dd.DevCantidad - ifnull(dd.DevCantidadConfirmada,0)) > 0
			   group by dd.proid, p.ProCodigo, p.ProDescripcion	,p.ProUnidades		   
			   union 			   
			   select dd.proid, p.ProCodigo, p.ProDescripcion, p.ProUnidades,
               sum((dd.DevCantidad -ifnull(DevCantidadConfirmada,0)) + ( (ifnull(dd.DevCantidadDetalle,0)- ifnull(DevCantidadDetalleConfirmada,0)) / p.ProUnidades)) DevCantidad    
			   from DevolucionesDetalleConfirmadas dd
			   inner join productos p on p.proid = dd.Proid	
			   inner join DevolucionesConfirmadas d on d.Repcodigo = dd.Repcodigo and d.DevSecuencia = dd.DevSecuencia
			   where dd.Repcodigo ='{Arguments.CurrentUser.RepCodigo}' and d.CuaSecuencia= {cuaSecuencia}
               and d.DevEstatus != 0
               and (dd.DevCantidad - ifnull(dd.DevCantidadConfirmada,0)) > 0
			   group by dd.proid, p.ProCodigo, p.ProDescripcion, p.ProUnidades	";
            return SqliteManager.GetInstance().Query<DevolucionesDetalle>(query, new string[] { });
        }


        public List<DevolucionesDetalle> getProductosDevolucionesRealizadasCajasyUnidades(int cuaSecuencia)
        {
            
            string query = $@"select dd.proid, p.ProCodigo, p.ProDescripcion, p.ProUnidades, 
               case ifnull(dd.UnmCodigo,'') when 'CJ' then  sum((dd.DevCantidad - ifnull(DevCantidadConfirmada,0)) + ( (ifnull(dd.DevCantidadDetalle,0)- ifnull(DevCantidadDetalleConfirmada,0)) / p.ProUnidades)) else 0 end DevCantidad, 
			   case when ifnull(dd.UnmCodigo,'')  != 'CJ' then  sum((dd.DevCantidad - ifnull(DevCantidadConfirmada,0)) + ( (ifnull(dd.DevCantidadDetalle,0)- ifnull(DevCantidadDetalleConfirmada,0)) / p.ProUnidades)) else 0 end DevCantidadDetalle 
			   from DevolucionesDetalle dd
			   inner join productos p on p.proid = dd.Proid	
			   inner join Devoluciones d on d.Repcodigo = dd.Repcodigo and d.DevSecuencia = dd.DevSecuencia
			   where dd.Repcodigo ='{Arguments.CurrentUser.RepCodigo}' and d.CuaSecuencia=  {cuaSecuencia}
               and d.DevEstatus != 0
               and (dd.DevCantidad - ifnull(dd.DevCantidadConfirmada,0)) > 0
			   group by dd.proid, p.ProCodigo, p.ProDescripcion	,p.ProUnidades		   
			   union 			   
			   select dd.proid, p.ProCodigo, p.ProDescripcion, p.ProUnidades,
               case ifnull(dd.UnmCodigo,'') when 'CJ' then  sum((dd.DevCantidad - ifnull(DevCantidadConfirmada,0)) + ( (ifnull(dd.DevCantidadDetalle,0)- ifnull(DevCantidadDetalleConfirmada,0)) / p.ProUnidades)) else 0 end DevCantidad, 
			   case when ifnull(dd.UnmCodigo,'')  != 'CJ' then  sum((dd.DevCantidad - ifnull(DevCantidadConfirmada,0)) + ( (ifnull(dd.DevCantidadDetalle,0)- ifnull(DevCantidadDetalleConfirmada,0)) / p.ProUnidades)) else 0 end DevCantidadDetalle 
			   from DevolucionesDetalleConfirmadas dd
			   inner join productos p on p.proid = dd.Proid	
			   inner join DevolucionesConfirmadas d on d.Repcodigo = dd.Repcodigo and d.DevSecuencia = dd.DevSecuencia
			   where dd.Repcodigo ='{Arguments.CurrentUser.RepCodigo}' and d.CuaSecuencia= {cuaSecuencia}
               and d.DevEstatus != 0
               and (dd.DevCantidad - ifnull(dd.DevCantidadConfirmada,0)) > 0
			   group by dd.proid, p.ProCodigo, p.ProDescripcion, p.ProUnidades	";
            return SqliteManager.GetInstance().Query<DevolucionesDetalle>(query, new string[] { });
        }
        public double GetDevolucionDetalleCantidadOferta(int devSecuencia, int DevPosicion)
        {
            double devpos = DevPosicion + 1;
            string query = "SELECT DevCantidad FROM DevolucionesDetalle where DevSecuencia = " + devSecuencia + " and DevPosicion = " + devpos + " and DevIndicadorOferta = 1 " +
                "and RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' ";

            List<DevolucionesDetalle> devcant = SqliteManager.GetInstance().Query<DevolucionesDetalle>(query, new string[] { });

            if(devcant.Count > 0 && devcant != null)
            {
                return devcant[0].DevCantidad;
            }
            else
            {
                return 0;
            }
        }

        private int GetPosMaxDevolucion(int proId, int devSecuencia, bool indicadorOferta)
        {
            var list = SqliteManager.GetInstance().Query<DevolucionesDetalle>("select max(DevPosicion) as DevPosicion from DevolucionesDetalle " +
             "where  DevSecuencia = ? and ltrim(rtrim(RepCodigo)) = ? ",
             new string[] {devSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });

            if (list != null && list.Count > 0)
            {
                return list[0].DevPosicion;
            }

            return 0;

        }

        public List<DevolucionesDetalle> GetDevolucionesByClientes()
        {
            return SqliteManager.GetInstance().Query<DevolucionesDetalle>("SELECT d.DevSecuencia,c.CliNombre as Name,d.DevCantidad from DevolucionesDetalle d inner join Clientes c on c.CliID = d.CliID" 
                                                                             , new string[] { Arguments.CurrentUser.RepCodigo });
        }
        public List<DevolucionesDetalle> GetDevolucionesByProductos()
        {
            return SqliteManager.GetInstance().Query<DevolucionesDetalle>("SELECT d.Proid,p.ProDescripcion ,d.DevCantidad from DevolucionesDetalle d inner join Productos p on p.Proid = d.ProID ", new string[] { Arguments.CurrentUser.RepCodigo });
        }
        public void ActualizarCantidadImpresion(string rowguid)
        {
            Hash map = new Hash("Devoluciones")
            {
                SaveScriptForServer = false
            };
            map.Add("DevCantidadImpresion", "DevCantidadImpresion + 1", true);
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map.SaveScriptForServer = true;
            map.ExecuteUpdate("rowguid = '" + rowguid + "'");

            Hash map2 = new Hash("DevolucionesConfirmadas");
            map2.Add("DevCantidadImpresion", "DevCantidadImpresion + 1", true);
            map2.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map2.ExecuteUpdate("rowguid = '" + rowguid + "'");

        }
    }
}
