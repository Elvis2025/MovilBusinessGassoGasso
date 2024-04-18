using MovilBusiness.Configuration;
using MovilBusiness.Enums;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.DataAccess
{
    class DS_Cambios : DS_Controller
    {
        private DS_Productos myProd;

        public DS_Cambios(DS_Productos myProd = null)
        {
            if (this.myProd == null)
            {
                this.myProd = new DS_Productos();
            }
            
        }

        public int SaveCambiosMercancia(int cuaSecuencia, string repAuditor = null)
        {
            var productos = myProd.GetResumenProductos((int)Modules.CAMBIOSMERCANCIA, false);

            int CamSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Cambios");

            new DS_Visitas().ActualizarVisitaEfectiva(Arguments.Values.CurrentVisSecuencia);

            Hash cam = new Hash("Cambios");
            cam.Add("CamEstatus", 1);
            cam.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            cam.Add("CamSecuencia", CamSecuencia);
            cam.Add("CliID", Arguments.Values.CurrentClient.CliID);
            cam.Add("VisSecuencia", Arguments.Values.CurrentVisSecuencia);
            cam.Add("CamFecha", Functions.CurrentDate());            
            cam.Add("CamTotal", productos.Count);
            cam.Add("mbVersion", Functions.AppVersion);
            cam.Add("CamReferencia", "");
            cam.Add("CamNCF", "");
            cam.Add("CuaSecuencia", cuaSecuencia);
            cam.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            cam.Add("RepSupervisor", "");
            cam.Add("CamFechaActualizacion", Functions.CurrentDate());
            cam.Add("rowguid", Guid.NewGuid().ToString());

            cam.ExecuteInsert();

            int pos = 1;
            var inv = new DS_Inventarios();

            var useMotivo = myParametro.GetParCambiosUsarMotivos() > 0;

            var montoTotal = 0.0;
            var montoSinItbis = 0.0;
            var parCambiosLote = myParametro.GetParCambiosMercanciaLotes();

            foreach (var pro in productos)
            {
                Hash det = new Hash("CambiosDetalle");
                det.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                det.Add("CamSecuencia", CamSecuencia);
                det.Add("CamPosicion", pos); pos++;
                det.Add("ProID", pro.ProID);

                /*   if (parDecimales)
                   {
                       det.Add("CamCantidad", pro.Cantidad);
                       det.Add("CamCantidadDetalle", pro.CantidadDetalle);
                   }
                   else
                   {*/
                det.Add("CamCantidad", (int)pro.Cantidad);
                det.Add("CamCantidadDetalle", (int)pro.CantidadDetalle);
                //}
                montoTotal += ((pro.Precio + pro.Itbis) * pro.Cantidad);
                montoSinItbis += pro.Precio * pro.Cantidad;
                det.Add("CamPrecio", pro.Precio);
                det.Add("CamItbis", pro.Itbis);
                det.Add("CamSelectivo", 0);
                det.Add("CamAdValorem", 0);
                det.Add("CamDescuento", 0);
                det.Add("CamTotalItbis", 0);
                det.Add("CamTotalDescuento", 0);
               
                det.Add("CamindicadorOferta", 0);
               // det.Add("CamDescPorciento", 0);
               // det.Add("CamCantidadEntregada", 0);
                det.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                det.Add("CamFechaActualizacion", Functions.CurrentDate());
                det.Add("rowguid", Guid.NewGuid().ToString());

                if (parCambiosLote == 1 || parCambiosLote == 2)
                {
                    det.Add("CamLote", pro.Lote);
                    det.Add("CamTipoTransaccion", pro.TipoCambio);
                }
                else
                {
                    det.Add("CamTipoTransaccion", 6);
                }

                if (useMotivo)
                {
                    det.Add("MotID", pro.MotIdDevolucion);
                }
                det.ExecuteInsert();


                if (parCambiosLote == 1 || parCambiosLote == 2)
                {
                    if(pro.TipoCambio == 0)
                    {
                        if (myParametro.GetParAlmacenIdParaMelma() > 0)
                        {
                            
                            inv.AgregarInventario(pro.ProID, pro.Cantidad, pro.CantidadDetalle, myParametro.GetParAlmacenIdParaMelma(), pro.LoteRecibido);
                        }
                        else
                        {
                            inv.AgregarInventario(pro.ProID, pro.Cantidad, pro.CantidadDetalle,-1,pro.LoteRecibido);
                        }

                    }else if(pro.TipoCambio == 1)
                    {
                        if (myParametro.GetParAlmacenIdParaMelma() > 0)
                        {

                            inv.RestarInventario(pro.ProID, pro.Cantidad, pro.CantidadDetalle, myParametro.GetParAlmacenVentaRanchera(), pro.LoteEntregado);
                        }
                        else
                        {
                            inv.RestarInventario(pro.ProID, pro.Cantidad, pro.CantidadDetalle, -1, pro.LoteEntregado);
                        }

                    }
                }
                else
                {
                    if (myParametro.GetParAlmacenIdParaMelma() > 0)
                    {
                        inv.RestarInventario(pro.ProID, pro.Cantidad, pro.CantidadDetalle, myParametro.GetParAlmacenVentaRanchera());
                        inv.AgregarInventario(pro.ProID, pro.Cantidad, pro.CantidadDetalle, myParametro.GetParAlmacenIdParaMelma());
                    }
                    else
                    {
                        inv.RestarInventario(pro.ProID, pro.Cantidad, pro.CantidadDetalle, -1);
                    }
                }

            }

            DS_RepresentantesSecuencias.UpdateSecuencia("Cambios", CamSecuencia);

            if (DS_RepresentantesParametros.GetInstance().GetParVisitasResultados())
            {
                ActualizarVisitasResultados();
            }

            myProd.ClearTemp((int)Modules.CAMBIOSMERCANCIA);

            return CamSecuencia;
        }

        private void ActualizarVisitasResultados()
        {
            var list = SqliteManager.GetInstance().Query<VisitasResultados>("select 6 as TitID, count(*) as VisCantidadTransacciones, " +
                "sum(((d.CamPrecio + d.CamAdValorem + d.CamSelectivo) - d.CamDescuento) * ((case when d.CamCantidadDetalle > 0 then d.CamCantidadDetalle / o.ProUnidades else 0 end) + d.CamCantidad)) as VisMontoSinItbis, sum(((d.CamItbis / 100.0) * ((d.CamPrecio + d.CamAdValorem + d.CamSelectivo) - d.CamDescuento)) * ((case when d.CamCantidadDetalle > 0 then d.CamCantidadDetalle / o.ProUnidades else 0 end) + d.CamCantidad)) as VisMontoItbis from Cambios p " +
                "inner join CambiosDetalle d on d.RepCodigo = p.RepCodigo and d.CamSecuencia = p.CamSecuencia " +
                "inner join Productos o on o.ProID = d.ProID " +
                "where p.VisSecuencia = ? and p.RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'", new string[] { Arguments.Values.CurrentVisSecuencia.ToString() });

            if(list != null && list.Count > 0)
            {
                var item = list[0];

                item.VisMontoTotal = item.VisMontoSinItbis + item.VisMontoItbis;
                item.VisComentario = "";

                new DS_Visitas().GuardarVisitasResultados(item);
            }
        }

        public Cambios GetBySecuencia(int camSecuencia, bool confirmado)
        {
            var list = SqliteManager.GetInstance().Query<Cambios>("select CamSecuencia, v.RepCodigo as RepCodigo, v.CliID as CliID, CamFecha, " +
                "CamTotal,  CliNombre, ifnull(CliNombreComercial,'') as CliNombreComercial, CliTelefono, CliContacto as CliPropietario, CliCodigo, CliRnc, CliCalle, CliUrbanizacion from " + (confirmado ? "CambiosConfirmados" : "Cambios") + " v " +
                "inner join Clientes cli on cli.CliID = v.CliID " +
                "where CamSecuencia = ? and ltrim(rtrim(v.RepCodigo)) = ? ", new string[] { camSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public List<CambiosDetalle> GetDetalleBySecuencia(int camSecuencia, bool confirmado)
        {
            return SqliteManager.GetInstance().Query<CambiosDetalle>("select CamSecuencia, ProCodigo, ProDescripcion, CamCantidad, " +
                "CamCantidadDetalle, v.ProID as ProID , v.CamPrecio as CamPrecio , v.CamTipoTransaccion  as CamTipoTransaccion " +
                "from " + (confirmado ? "CambiosDetalleConfirmados" : "CambiosDetalle") + " v " +
                "inner join Productos p on p.ProID = v.ProID " +
                "where CamSecuencia = ? and ltrim(rtrim(v.RepCodigo)) = ? " +
                "order by ProDescripcion", new string[] { camSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });
        }

        public void InsertarCambiosInTemp(int CamSecuencia, bool confirmado)
        {
            myProd.ClearTemp((int)Modules.CAMBIOSMERCANCIA);

            var query = "select distinct " + ((int)Modules.CAMBIOSMERCANCIA).ToString() + " as TitID, pd.CamCantidad as Cantidad, pd.CamCantidadDetalle as CantidadDetalle, pd.rowguid as rowguid, pd.ProID as ProID, " +
                "p.ProDescripcion as Descripcion from " + (confirmado ? "CambiosDetalleConfirmados" : "CambiosDetalle") + " pd " +
                "inner join Productos p on p.ProID = pd.ProID where ltrim(rtrim(pd.RepCodigo)) = ? and pd.CamSecuencia = ? order by p.ProDescripcion";
            

            var list = SqliteManager.GetInstance().Query<ProductosTemp>(query, new string[] { Arguments.CurrentUser.RepCodigo.Trim(), CamSecuencia.ToString() });

            SqliteManager.GetInstance().InsertAll(list);

        }

        public void EstCambios(int camSecuencia, int est, string rowguid)
        {
            Hash ped = new Hash("Cambios");
            ped.Add("CamEstatus", est);
            ped.Add("camFechaActualizacion", Functions.CurrentDate());
            ped.Add("UsuInicioSesion", /*Arguments.CurrentUser.RepCodigo*/"mdsoft");

            if (est == 0)
            {
                if (new DS_SuscriptoresCambios().UpdateCambioEstadoInsertByRowguid(rowguid, est))
                {
                    ped.SaveScriptForServer = false;
                }
            }

            ped.ExecuteUpdate("rowguid = '" + rowguid + "'");//"CamSecuencia = " + camSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");


            var dsInv = new DS_Inventarios();

            var list = GetDetalleBySecuencia(camSecuencia, false);

            if (list != null && list.Count > 0)
            {
                int almid = myParametro.GetParAlmacenVentaRanchera();
                var parMultiAlmacenes = myParametro.GetParUsarMultiAlmacenes();

                foreach (var det in list)
                {

                    if(parMultiAlmacenes)
                        dsInv.RestarInventario(det.ProID, det.CamCantidad, det.CamCantidadDetalle, myParametro.GetParAlmacenIdParaMelma());

                    dsInv.AgregarInventario(det.ProID, det.CamCantidad, det.CamCantidadDetalle, parMultiAlmacenes? almid : -1);
                }
            }
        }

        public List<Cambios> GetAllCambiosMercanciaByCuadreByClientes(int CuaSecuencia)
        {
            return SqliteManager.GetInstance().Query<Cambios>(@"	SELECT CliCodigo, ifnull(CliNombre, '') as CliNombre, c.CamSecuencia  FROM Cambios c 
	            INNER JOIN Clientes cl ON cl.CliID = c.CliID 
	            WHERE c.CuaSecuencia = " + CuaSecuencia + " AND c.CamEstatus <> 0 " +
                "AND c.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' " +
                @"UNION SELECT CliCodigo, ifnull(CliNombre, '') as CliNombre, c.CamSecuencia FROM CambiosConfirmados c
                INNER JOIN Clientes cl ON cl.CliID = c.CliID
                WHERE c.CuaSecuencia = " + CuaSecuencia + " AND c.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "'", new string[] { });

        }

        public List<CambiosDetalle> GetAllCambiosMercanciaByCuadreByProductos(int CuaSecuencia)
        {
            return SqliteManager.GetInstance().Query<CambiosDetalle>(@"select ProCodigo, ProDescripcion , CamCantidad, CamCantidadDetalle , proid from (	SELECT ProCodigo, ifnull(ProDescripcion, '') as ProDescripcion, 
	           sum(cd.camcantidad) as CamCantidad, sum(ifnull(CamCantidadDetalle, 0)) as CamCantidadDetalle, cd.proid as proid FROM Cambios c 
	            INNER JOIN CambiosDetalle cd ON cd.CamSecuencia = c.CamSecuencia and c.RepCodigo = cd.RepCodigo 
	            INNER JOIN Productos p on p.ProID = cd.ProID 
	            WHERE c.CuaSecuencia = " + CuaSecuencia + " AND c.CamEstatus <> 0 " +
                "AND c.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' group by cd.proid " +
                @"UNION SELECT ProCodigo, ifnull(ProDescripcion, '') as ProDescripcion,
                sum(cd.camcantidad) as CamCantidad, sum(ifnull(CamCantidadDetalle, 0)) as CamCantidadDetalle, cd.proid as proid FROM CambiosConfirmados c
                INNER JOIN CambiosDetalleConfirmados cd ON cd.CamSecuencia = c.CamSecuencia AND c.RepCodigo = cd.RepCodigo
                INNER JOIN Productos p on P.ProID = cd.ProID
                WHERE c.CuaSecuencia = " + CuaSecuencia + " AND c.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' group by cd.proid ) as r group by r.proid  ", new string[] { });

        }

        public bool GetAllCambiosMercanciaByCuadreByCuaSecuencia(int camsecuencia)
        {
            try
            {
                var listcua = SqliteManager.GetInstance().Query<Cuadres>("SELECT CuaSecuencia FROM Cuadres "
                              + "INNER JOIN RepresentantesSecuencias ON RepSecuencia = CuaSecuencia and UPPER(trim(RepTabla)) = 'CUADRES' "
                              + "WHERE CuaEstatus = 1", new string[] { });

                if (listcua.Count > 0)
                {
                    foreach (var i in listcua)
                    {
                        var listcam = SqliteManager.GetInstance().Query<Cambios>("SELECT * FROM Cambios " +
                                      " WHERE CuaSecuencia = " + i.CuaSecuencia + " AND CamEstatus <> 0 " +
                                       "AND CamSecuencia = '" + camsecuencia + "'" +
                                      "AND RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "'", new string[] { });

                        if (listcam.Count > 0)
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
    }
}
