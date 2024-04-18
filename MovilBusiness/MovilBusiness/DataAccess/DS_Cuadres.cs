
using MovilBusiness.Configuration;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using MovilBusiness.views;
using System.Collections.Generic;
using System;
using Xamarin.Forms;
using System.Linq;
using MovilBusiness.Views.Components.Modals;
using MovilBusiness.Abstraction;
using MovilBusiness.ViewModel;
using System.Threading.Tasks;
using MovilBusiness.model;
using MovilBusiness.Resx;

namespace MovilBusiness.DataAccess
{
    public class DS_Cuadres : DS_Controller
    {
        private DS_Vehiculos myVeh;
        private DS_Almacenes myAlm;
        private DS_Cargas MyCar;
        private DS_Rutas MyRut;
       
        public DS_Cuadres()
        {
            myVeh = new DS_Vehiculos();
            myAlm = new DS_Almacenes();
            MyCar = new DS_Cargas();
            MyRut = new DS_Rutas();
        }

        public void AbrirCuadre(Location location, int vehId, string ayudante1, string ayudante2, int kilometroInicio, int contadorInicio, string RepAuditor = "")
        {
            if (myParametro.GetParUnSoloCuadrePorDia() && HoyHizoCuadre())
            {
                throw new Exception(AppResource.DailySquareAlreadyOpenedMessage);
            }

            int cuaSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Cuadres");
            /////////////////////////////////////////////////////////////////////////////////////////////////
            //Hash closeCuadresAbiertos = new Hash("Cuadres");
            //closeCuadresAbiertos.Add("CuaEstatus", 2);
            //closeCuadresAbiertos.ExecuteUpdate(" CuaEstatus = 1 AND RepCodigo = "+ Arguments.CurrentUser.RepCodigo + " AND CuaSecuencia <> "+ cuaSecuencia+" ");
            //////////////////////////////////////////////////////////////////////////////////////////////////

            double RutPeaje = 0.00;
            double RutDieta = 0.00;
            if (SqliteManager.ExistsTable("Rutas"))
            {
                var ruta = MyRut.GetRutaByRepresentante(Arguments.CurrentUser.RepCodigo);

                if(ruta != null && ruta.Count > 0)
                {
                    RutPeaje = ruta[0].RutPeaje;
                    RutDieta = ruta[0].RutDieta;
                }
            }

            Hash map = new Hash("Cuadres");
            map.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            map.Add("CuaSecuencia", cuaSecuencia);
            map.Add("CuaFechaInicio", Functions.CurrentDate());
            map.Add("CuaKilometrosInicio", kilometroInicio);
            map.Add("CuaContadorInicial", contadorInicio);

            if (location != null)
            {
                map.Add("CuaLatitudApertura", location.Latitude);
                map.Add("CuaLongitudApertura", location.Longitude);
            }
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);

            map.Add("CuaEstatus", 1);
            map.Add("RevSecuencia", 0);
            map.Add("VehID", vehId);
            map.Add("RutID", 0);
            map.Add("RepAyudante1", ayudante1);
            map.Add("RepAyudante2", ayudante2);
            map.Add("RepSupervisor", RepAuditor);
            map.Add("CuaFechaActualizacion", Functions.CurrentDate());
            map.Add("CuaPeaje", RutPeaje);
            map.Add("CuaDieta", RutDieta);
            map.Add("rowguid", Guid.NewGuid().ToString());
            map.Add("mbVersion", Functions.AppVersion);
            map.ExecuteInsert();

            //var inventario = new DS_Inventarios().GetInventario(true);
            var parCuadrarInventario = myParametro.GetParCuadrarInventarioAlCerrarCuadre();
            var parMultiAlmacenes = myParametro.GetParUsarMultiAlmacenes();
            var almIddespacho = myParametro.GetParAlmacenIdParaDespacho();
            var almIdVenta = myParametro.GetParAlmacenVentaRanchera();
            var almIdDev = myParametro.GetParAlmacenIdParaDevolucion();

            var myInv = new DS_Inventarios();

            if (myParametro.GetParCargasInventario())
            {
                List<Inventarios> inventarioventa;
                var inventariodespacho = new List<Inventarios>();
                var inventariodevolucion = new List<Inventarios>();

                if (almIdVenta != -1)
                {
                    inventariodespacho = myInv.GetInventario(almId: parMultiAlmacenes && almIddespacho != -1 ? almIddespacho : -1);
                    inventarioventa = myInv.GetInventario(almId: parMultiAlmacenes && almIdVenta != -1 ? almIdVenta : -1);

                }
                else
                {
                    inventariodevolucion = myInv.GetInventario(almId: parMultiAlmacenes && almIdDev != -1 ? almIdDev : -1);
                    inventarioventa = myInv.GetInventario(almId: parMultiAlmacenes && almIdVenta != -1 ? almIdVenta : -1);
                }

                bool parAlmacenesAgrupados = myParametro.GetParCuadrePorMultiplesAlmacenesAgrupados();
                if (parMultiAlmacenes && parAlmacenesAgrupados)
                {
                    var parametrosAlmacenes = myParametro.GetAlmacenesAgrupados("ALMID").ToList();
                    foreach (var parametro in parametrosAlmacenes)
                    {
                        int almid = -1;
                        int.TryParse(parametro.ParValor, out almid);
                        var inventarios = myInv.GetInventario(almId: parMultiAlmacenes && almid != -1 ? almid : -1);


                        foreach (var p in inventarios)
                        {

                            Hash detalle = new Hash("CuadresDetalle");
                            detalle.Add("CuaCantidadInicial", p.invCantidad);
                            detalle.Add("CuaCantidadDetalleInicial", p.InvCantidadDetalle);
                            detalle.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                            detalle.Add("CuaFechaActualizacion", Functions.CurrentDate());
                            detalle.Add("CuaCantidadFisica", 0);
                            detalle.Add("CuaCantidadDetalleFisica", 0);

                            if (ExistsProductInDetalleV2(cuaSecuencia, p.ProID, p.InvLote, p.AlmId))
                            {
                                detalle.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ProID = " + p.ProID +
                                    " and CuaSecuencia = " + cuaSecuencia + " and CuaTipoInventario = " + p.AlmId + " and CuaLote = '" + p.InvLote + "'");
                            }
                            else
                            {
                                detalle.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                                detalle.Add("CuaSecuencia", cuaSecuencia);
                                detalle.Add("CuaTipoInventario", p.AlmId);
                                detalle.Add("ProID", p.ProID);
                                detalle.Add("CuaCantidadFinal", 0);
                                detalle.Add("CuaCantidadDetalleFinal", 0);
                                detalle.Add("CuaLote", p.InvLote);
                                detalle.Add("rowguid", Guid.NewGuid().ToString());
                                detalle.ExecuteInsert();
                            }

                        }
                    }
                }
                else
                {
                    if (parMultiAlmacenes)
                    {
                        foreach (var p in inventarioventa)
                        {
                            var detalle = new Hash("CuadresDetalle");
                            detalle.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                            detalle.Add("CuaSecuencia", cuaSecuencia);
                            detalle.Add("ProID", p.ProID);
                            detalle.Add("CuaTipoInventario", p.AlmId);
                            detalle.Add("CuaCantidadFisica", 0);
                            detalle.Add("CuaCantidadDetalleFisica", 0);
                            detalle.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                            detalle.Add("CuaFechaActualizacion", Functions.CurrentDate());
                            detalle.Add("CuaCantidadInicial", p.invCantidad);
                            detalle.Add("CuaCantidadDetalleInicial", p.InvCantidadDetalle);
                            detalle.Add("CuaCantidadFinal", 0);
                            detalle.Add("CuaCantidadDetalleFinal", 0);
                            detalle.Add("CuaLote", p.InvLote);
                            detalle.Add("rowguid", Guid.NewGuid().ToString());
                            detalle.ExecuteInsert();

                        }

                        if (almIdVenta != -1)
                        {
                            if(almIdVenta != almIddespacho)
                            {
                                foreach (var p in inventariodespacho)
                                {
                                    var detalle = new Hash("CuadresDetalle");
                                    detalle.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                                    detalle.Add("CuaSecuencia", cuaSecuencia);
                                    detalle.Add("ProID", p.ProID);
                                    detalle.Add("CuaTipoInventario", p.AlmId);
                                    detalle.Add("CuaCantidadFisica", 0);
                                    detalle.Add("CuaCantidadDetalleFisica", 0);
                                    detalle.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                                    detalle.Add("CuaFechaActualizacion", Functions.CurrentDate());
                                    detalle.Add("CuaCantidadInicial", p.invCantidad);
                                    detalle.Add("CuaCantidadDetalleInicial", p.InvCantidadDetalle);
                                    detalle.Add("CuaCantidadFinal", 0);
                                    detalle.Add("CuaCantidadDetalleFinal", 0);
                                    detalle.Add("CuaLote", p.InvLote);
                                    detalle.Add("rowguid", Guid.NewGuid().ToString());
                                    detalle.ExecuteInsert();

                                }
                            }
                        }
                        else
                        {
                            foreach (var p in inventariodevolucion)
                            {
                                var detalle = new Hash("CuadresDetalle");
                                detalle.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                                detalle.Add("CuaSecuencia", cuaSecuencia);
                                detalle.Add("ProID", p.ProID);
                                detalle.Add("CuaTipoInventario", p.AlmId);
                                detalle.Add("CuaCantidadFisica", 0);
                                detalle.Add("CuaCantidadDetalleFisica", 0);
                                detalle.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                                detalle.Add("CuaFechaActualizacion", Functions.CurrentDate());
                                detalle.Add("CuaCantidadInicial", p.invCantidad);
                                detalle.Add("CuaCantidadDetalleInicial", p.InvCantidadDetalle);
                                detalle.Add("CuaCantidadFinal", 0);
                                detalle.Add("CuaCantidadDetalleFinal", 0);
                                detalle.Add("CuaLote", p.InvLote);
                                detalle.Add("rowguid", Guid.NewGuid().ToString());
                                detalle.ExecuteInsert();



                            }
                        }
                    }
                    else
                    {
                        foreach (var p in inventariodevolucion)
                        {
                            var detalle = new Hash("CuadresDetalle");
                            detalle.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                            detalle.Add("CuaSecuencia", cuaSecuencia);
                            detalle.Add("ProID", p.ProID);
                            detalle.Add("CuaTipoInventario", p.AlmId);
                            detalle.Add("CuaCantidadFisica", 0);
                            detalle.Add("CuaCantidadDetalleFisica", 0);
                            detalle.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                            detalle.Add("CuaFechaActualizacion", Functions.CurrentDate());
                            detalle.Add("CuaCantidadInicial", p.invCantidad);
                            detalle.Add("CuaCantidadDetalleInicial", p.InvCantidadDetalle);
                            detalle.Add("CuaCantidadFinal", 0);
                            detalle.Add("CuaCantidadDetalleFinal", 0);
                            detalle.Add("CuaLote", p.InvLote);
                            detalle.Add("rowguid", Guid.NewGuid().ToString());
                            detalle.ExecuteInsert();

                        }
                    }
                }
            }

            if (myParametro.GetParCuadresUsarContador() && myParametro.GetParCuadres() == 1)
            {
                myVeh.ActualizarContador(vehId, contadorInicio);
            }

            if (myParametro.GetParCuadresUsarKilometraje() && myParametro.GetParCuadres() == 1)
            {
                myVeh.ActualizarKilometraje(vehId, kilometroInicio);
            }

            if (myParametro.GetParCuadresVehiculosCapacidad() && vehId > 0)
            {
                var capacidad = myVeh.GetCapacidadVehiculo(vehId);

                foreach (var cap in capacidad)
                {
                    if (myParametro.GetParUsarMultiAlmacenes())
                    {
                        //myInv.ActualizarParametroInventarioCamion(AlmacenVenta.AlmID);
                        myInv.ActualizarParametroAlmacenesContar(myParametro.GetParAlmacenVentaRanchera(), myParametro.GetParAlmacenIdParaMelma()); ;
                        almIdVenta = myParametro.GetParAlmacenVentaRanchera();

                        myInv.AgregarInventario(cap.ProID, cap.VCACantidad, cap.VCACantidadDetalle, almIdVenta);

                        var detalle = new Hash("CuadresDetalle");
                        detalle.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                        detalle.Add("CuaSecuencia", cuaSecuencia);
                        detalle.Add("ProID", cap.ProID);
                        detalle.Add("CuaTipoInventario", almIdVenta);
                        detalle.Add("CuaCantidadFisica", 0);
                        detalle.Add("CuaCantidadDetalleFisica", 0);
                        detalle.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                        detalle.Add("CuaFechaActualizacion", Functions.CurrentDate());
                        detalle.Add("CuaCantidadInicial", cap.VCACantidad);
                        detalle.Add("CuaCantidadDetalleInicial", cap.VCACantidadDetalle);
                        detalle.Add("CuaCantidadFinal", 0);
                        detalle.Add("CuaCantidadDetalleFinal", 0);
                        detalle.Add("CuaLote", "");
                        detalle.Add("rowguid", Guid.NewGuid().ToString());
                        detalle.ExecuteInsert();
                    }
                    else
                    {
                        myInv.AgregarInventario(cap.ProID, cap.VCACantidad, cap.VCACantidadDetalle);
                    }
                }

            }

            DS_RepresentantesSecuencias.UpdateSecuencia("Cuadres", cuaSecuencia);

            Arguments.Values.CurrentCuaSecuencia = cuaSecuencia;
        }

        public void CerrarCuadre(int cuaSecuencia, Location location, int kilometroFinal, int contadorFinal, int vehId, string rowguid)
        {
            /////////////////////////////////////////////////////////////////////////////////////////////////
          /*  Hash closeCuadresAbiertos = new Hash("Cuadres");
            closeCuadresAbiertos.Add("CuaEstatus", 2);
            closeCuadresAbiertos.ExecuteUpdate(" CuaEstatus = 1 AND CuaSecuencia <> " + cuaSecuencia + " ");*/
            ////////////////////////////////////////////////////////////////////////////////////////////////

            Hash myCua = new Hash("Cuadres");
            myCua.Add("CuaFechaFin", Functions.CurrentDate());
            myCua.Add("CuaKilometrosFin", kilometroFinal);
            myCua.Add("CuaEstatus", 2);
            if (location != null)
            {
                myCua.Add("CuaLatitudCierre", location.Latitude);
                myCua.Add("CuaLongitudCierre", location.Longitude);
            }
            myCua.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            myCua.Add("CuaFechaActualizacion", Functions.CurrentDate());

         // myCua.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and CuaSecuencia = " + cuaSecuencia);
            myCua.ExecuteUpdate("rowguid = '" + rowguid + "' ");


            var parCuadrarInventario = myParametro.GetParCuadrarInventarioAlCerrarCuadre();
            var parEliminarInventarioEnCero = myParametro.GetParEliminarInventarioEnCeroAlCerrarCuadre();
            var parMultiAlmacenes = myParametro.GetParUsarMultiAlmacenes();
            var almIddespacho = myParametro.GetParAlmacenIdParaDespacho();
            var almIdVenta = myParametro.GetParAlmacenVentaRanchera();
            var almIdDev = myParametro.GetParAlmacenIdParaDevolucion();
            var almIdMelma = myParametro.GetParAlmacenIdParaMelma();

            //actualizar cantidad final inventario
            if (myParametro.GetParCargasInventario())
            {
                var inventarioventa = new List<Model.Inventarios>();
                var inventariodespacho = new List<Model.Inventarios>();
                var inventariodevolucion = new List<Model.Inventarios>();

                if (almIdVenta != -1)
                {
                    inventariodespacho = new DS_Inventarios().GetInventario(almId: parMultiAlmacenes && almIddespacho != -1 ? almIddespacho : -1);
                    inventarioventa = new DS_Inventarios().GetInventario(almId: parMultiAlmacenes && almIdVenta != -1 ? almIdVenta : -1);

                }
                else if (myParametro.GetParConteoConAlmacenDespachoyDevolucion())
                {
                    inventariodevolucion = new DS_Inventarios().GetInventario(almId: parMultiAlmacenes && almIdDev != -1 ? almIdDev : -1);
                    inventariodespacho = new DS_Inventarios().GetInventario(almId: parMultiAlmacenes && almIddespacho != -1 ? almIddespacho : -1);
                }
                else
                {
                    inventariodevolucion = new DS_Inventarios().GetInventario(almId: parMultiAlmacenes && almIdDev != -1 ? almIdDev : -1);
                    inventarioventa = new DS_Inventarios().GetInventario(almId: parMultiAlmacenes && almIdVenta != -1 ? almIdVenta : -1);
                }

                bool parAlmacenesAgrupados = myParametro.GetParCuadrePorMultiplesAlmacenesAgrupados();
                if (parMultiAlmacenes && parAlmacenesAgrupados)
                {
                    var parametrosAlmacenes = myParametro.GetAlmacenesAgrupados("ALMID").ToList();
                    foreach (var parametro in parametrosAlmacenes)
                    {
                        int almid = -1;
                        int.TryParse(parametro.ParValor, out almid);
                        var inventarios = new DS_Inventarios().GetInventario(almId: parMultiAlmacenes && almid != -1 ? almid : -1);
                        foreach (var p in inventarios)
                        {

                            Hash detalle = new Hash("CuadresDetalle");
                            detalle.Add("CuaCantidadFinal", p.invCantidad);
                            detalle.Add("CuaCantidadDetalleFinal", p.InvCantidadDetalle);
                            detalle.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                            detalle.Add("CuaFechaActualizacion", Functions.CurrentDate());

                            if (ExistsProductInDetalleV2(cuaSecuencia, p.ProID, p.InvLote,p.AlmId))
                            {
                                detalle.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ProID = " + p.ProID + 
                                    " and CuaSecuencia = " + cuaSecuencia + " and CuaTipoInventario = " + p.AlmId + " and CuaLote = '" + p.InvLote+ "'");
                            }
                            else
                            {
                                detalle.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                                detalle.Add("CuaSecuencia", cuaSecuencia);
                                detalle.Add("CuaTipoInventario", p.AlmId);
                                detalle.Add("ProID", p.ProID);
                                detalle.Add("CuaCantidadInicial", 0);
                                detalle.Add("CuaCantidadDetalleInicial", 0);
                                detalle.Add("CuaLote", p.InvLote);
                                detalle.Add("rowguid", Guid.NewGuid().ToString());
                                detalle.ExecuteInsert();
                            }
                            var almacenesExcluir = new DS_Almacenes().GetAlmacenesByAlmIDParameter(myParametro.GetParNoCuadrarAlmacen());
                            if (almacenesExcluir == null || !almacenesExcluir.Exists(x => x.AlmID == p.AlmId))
                            {
                                if (parCuadrarInventario)
                                {
                                    var inv = new Hash("InventariosAlmacenesRepresentantes");
                                    inv.Add("invCantidad", 0);
                                    inv.Add("InvCantidadDetalle", 0);
                                    inv.Add("InvFechaActualizacion", Functions.CurrentDate());
                                    inv.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                                    inv.ExecuteUpdate("rowguid = '" + p.rowguid + "' ");
                                    //inv.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ProID = " + p.ProID + " and AlmID = " + p.AlmId.ToString());
                                }
                            }

                            // Esto elimina el inventario en 0 cuando cierra cuadre
                            if (parEliminarInventarioEnCero)
                            {
                                var inven = new Hash("InventariosAlmacenesRepresentantes");
                                inven.ExecuteDelete("rowguid = '" + p.rowguid + "' and invCantidad = 0");
                            }

                        }

                    }
                }
                else
                {
                    if (parMultiAlmacenes)
                    {
                        foreach (var p in inventarioventa)
                        {
                            Hash detalle = new Hash("CuadresDetalle");
                            detalle.Add("CuaCantidadFinal", p.invCantidad);
                            detalle.Add("CuaCantidadDetalleFinal", p.InvCantidadDetalle);
                            detalle.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                            detalle.Add("CuaFechaActualizacion", Functions.CurrentDate());

                            if (ExistsProductInDetalleV2(cuaSecuencia, p.ProID, p.InvLote, p.AlmId))
                            {
                                detalle.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ProID = " + p.ProID +
                                    " and CuaSecuencia = " + cuaSecuencia + " and CuaTipoInventario = " + p.AlmId + " and CuaLote = '" + p.InvLote + "'");
                            }
                            else
                            {
                                detalle.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                                detalle.Add("CuaSecuencia", cuaSecuencia);
                                detalle.Add("CuaTipoInventario", p.AlmId);
                                detalle.Add("ProID", p.ProID);
                                detalle.Add("CuaCantidadInicial", 0);
                                detalle.Add("CuaCantidadDetalleInicial", 0);
                                detalle.Add("CuaLote", p.InvLote);
                                detalle.Add("rowguid", Guid.NewGuid().ToString());
                                detalle.ExecuteInsert();
                            }

                            if (parCuadrarInventario)
                            {
                                if (parMultiAlmacenes && almIdVenta != -1)
                                {
                                    var inv = new Hash("InventariosAlmacenesRepresentantes");
                                    inv.Add("invCantidad", 0);
                                    inv.Add("InvCantidadDetalle", 0);
                                    inv.Add("InvFechaActualizacion", Functions.CurrentDate());
                                    inv.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                                    inv.ExecuteUpdate("rowguid = '" + p.rowguid + "' ");
                                    //inv.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ProID = " + p.ProID + " and AlmID = " + almIdVenta.ToString());
                                }
                                else
                                {
                                    var inv = new Hash("Inventarios");
                                    inv.Add("invCantidad", 0);
                                    inv.Add("InvCantidadDetalle", 0);
                                    inv.Add("InvFechaActualizacion", Functions.CurrentDate());
                                    inv.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                                    inv.ExecuteUpdate("rowguid = '" + p.rowguid + "' ");
                                    //inv.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ProID = " + p.ProID);
                                }
                            }


                            // Esto elimina el inventario en 0 cuando cierra cuadre
                            if (parEliminarInventarioEnCero)
                            {
                                if (parMultiAlmacenes && almIdVenta != -1)
                                {
                                    var inven = new Hash("InventariosAlmacenesRepresentantes");
                                    inven.ExecuteDelete("rowguid = '" + p.rowguid + "' and invCantidad = 0");
                                }
                                else
                                {
                                    var inven = new Hash("Inventarios");
                                    inven.ExecuteDelete("rowguid = '" + p.rowguid + "' and invCantidad = 0");
                                } 
                            }

                        }

                        if (almIdVenta != -1 )
                        {
                            foreach (var p in inventariodespacho)
                            {
                                Hash detalle = new Hash("CuadresDetalle");
                                detalle.Add("CuaCantidadFinal", p.invCantidad);
                                detalle.Add("CuaCantidadDetalleFinal", p.InvCantidadDetalle);
                                detalle.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                                detalle.Add("CuaFechaActualizacion", Functions.CurrentDate());

                                if (ExistsProductInDetalleV2(cuaSecuencia, p.ProID, p.InvLote, p.AlmId))
                                {
                                    detalle.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ProID = " + p.ProID +
                                    " and CuaSecuencia = " + cuaSecuencia + " and CuaTipoInventario = " + p.AlmId + " and CuaLote = '" + p.InvLote + "'");
                                }
                                else
                                {
                                    detalle.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                                    detalle.Add("CuaSecuencia", cuaSecuencia);
                                    detalle.Add("CuaTipoInventario", p.AlmId);
                                    detalle.Add("ProID", p.ProID);
                                    detalle.Add("CuaCantidadInicial", 0);
                                    detalle.Add("CuaCantidadDetalleInicial", 0);
                                    detalle.Add("CuaLote", p.InvLote);
                                    detalle.Add("rowguid", Guid.NewGuid().ToString());
                                    detalle.ExecuteInsert();
                                }

                                if (parCuadrarInventario)
                                {
                                    if (parMultiAlmacenes && almIdVenta != -1)
                                    {
                                        var inv = new Hash("InventariosAlmacenesRepresentantes");
                                        inv.Add("invCantidad", 0);
                                        inv.Add("InvCantidadDetalle", 0);
                                        inv.Add("InvFechaActualizacion", Functions.CurrentDate());
                                        inv.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                                        inv.ExecuteUpdate("rowguid = '" + p.rowguid + "' ");
                                        //inv.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ProID = " + p.ProID + " and (AlmID = " + almIdVenta.ToString()+ " or AlmID = " + almIddespacho.ToString()+" ) ");
                                    }
                                    else
                                    {
                                        var inv = new Hash("Inventarios");
                                        inv.Add("invCantidad", 0);
                                        inv.Add("InvCantidadDetalle", 0);
                                        inv.Add("InvFechaActualizacion", Functions.CurrentDate());
                                        inv.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                                        inv.ExecuteUpdate("rowguid = '" + p.rowguid + "' ");
                                        //inv.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ProID = " + p.ProID);
                                    }
                                }

                                // Esto elimina el inventario en 0 cuando cierra cuadre
                                if (parEliminarInventarioEnCero)
                                {
                                    if (parMultiAlmacenes && almIdVenta != -1)
                                    {
                                        var inven = new Hash("InventariosAlmacenesRepresentantes");
                                        inven.ExecuteDelete("rowguid = '" + p.rowguid + "' and invCantidad = 0");
                                    }
                                    else
                                    {
                                        var inven = new Hash("Inventarios");
                                        inven.ExecuteDelete("rowguid = '" + p.rowguid + "' and invCantidad = 0");
                                    }
                                }

                            }
                        }
                        else if (!myParametro.GetParConteoConAlmacenDespachoyDevolucion())
                        {
                            foreach (var p in inventariodevolucion)
                            {
                                Hash detalle = new Hash("CuadresDetalle");
                                detalle.Add("CuaCantidadFinal", p.invCantidad);
                                detalle.Add("CuaCantidadDetalleFinal", p.InvCantidadDetalle);
                                detalle.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                                detalle.Add("CuaFechaActualizacion", Functions.CurrentDate());

                                if (ExistsProductInDetalleV2(cuaSecuencia, p.ProID, p.InvLote, p.AlmId))
                                {
                                    detalle.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ProID = " + p.ProID +
                                    " and CuaSecuencia = " + cuaSecuencia + " and CuaTipoInventario = " + p.AlmId + " and CuaLote = '" + p.InvLote + "'");
                                }
                                else
                                {
                                    detalle.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                                    detalle.Add("CuaSecuencia", cuaSecuencia);
                                    detalle.Add("CuaTipoInventario", p.AlmId);
                                    detalle.Add("ProID", p.ProID);
                                    detalle.Add("CuaCantidadInicial", 0);
                                    detalle.Add("CuaCantidadDetalleInicial", 0);
                                    detalle.Add("CuaLote", p.InvLote);
                                    detalle.Add("rowguid", Guid.NewGuid().ToString());
                                    detalle.ExecuteInsert();
                                }

                                if (parCuadrarInventario)
                                {
                                    if (parMultiAlmacenes && almIdVenta != -1)
                                    {
                                        var inv = new Hash("InventariosAlmacenesRepresentantes");
                                        inv.Add("invCantidad", 0);
                                        inv.Add("InvCantidadDetalle", 0);
                                        inv.Add("InvFechaActualizacion", Functions.CurrentDate());
                                        inv.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                                        inv.ExecuteUpdate("rowguid = '" + p.rowguid + "' ");
                                        //inv.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ProID = " + p.ProID + " and (AlmID = " + almIdVenta.ToString() + " or AlmID = " + almIdDev.ToString() + " ) ");
                                    }
                                    else
                                    {
                                        var inv = new Hash("Inventarios");
                                        inv.Add("invCantidad", 0);
                                        inv.Add("InvCantidadDetalle", 0);
                                        inv.Add("InvFechaActualizacion", Functions.CurrentDate());
                                        inv.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                                        inv.ExecuteUpdate("rowguid = '" + p.rowguid + "' ");
                                        //inv.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ProID = " + p.ProID);
                                    }
                                }

                                // Esto elimina el inventario en 0 cuando cierra cuadre
                                if (parEliminarInventarioEnCero)
                                {
                                    if (parMultiAlmacenes && almIdVenta != -1)
                                    {
                                        var inven = new Hash("InventariosAlmacenesRepresentantes");
                                        inven.ExecuteDelete("rowguid = '" + p.rowguid + "' and invCantidad = 0");
                                    }
                                    else
                                    {
                                        var inven = new Hash("Inventarios");
                                        inven.ExecuteDelete("rowguid = '" + p.rowguid + "' and invCantidad = 0");
                                    }
                                }

                            }
                        }


                        if (myParametro.GetParConteoConAlmacenDespachoyDevolucion())
                        {
                            foreach (var p in inventariodespacho)
                            {
                                Hash detalle = new Hash("CuadresDetalle");
                                detalle.Add("CuaCantidadFinal", p.invCantidad);
                                detalle.Add("CuaCantidadDetalleFinal", p.InvCantidadDetalle);
                                detalle.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                                detalle.Add("CuaFechaActualizacion", Functions.CurrentDate());

                                if (ExistsProductInDetalleV2(cuaSecuencia, p.ProID, p.InvLote, p.AlmId))
                                {
                                    detalle.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ProID = " + p.ProID +
                                    " and CuaSecuencia = " + cuaSecuencia + " and CuaTipoInventario = " + p.AlmId + " and CuaLote = '" + p.InvLote + "'");
                                }
                                else
                                {
                                    detalle.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                                    detalle.Add("CuaSecuencia", cuaSecuencia);
                                    detalle.Add("CuaTipoInventario", p.AlmId);
                                    detalle.Add("ProID", p.ProID);
                                    detalle.Add("CuaCantidadInicial", 0);
                                    detalle.Add("CuaCantidadDetalleInicial", 0);
                                    detalle.Add("CuaLote", p.InvLote);
                                    detalle.Add("rowguid", Guid.NewGuid().ToString());
                                    detalle.ExecuteInsert();
                                }

                                if (parCuadrarInventario)
                                {
                                    if (parMultiAlmacenes)
                                    {
                                        var inv = new Hash("InventariosAlmacenesRepresentantes");
                                        inv.Add("invCantidad", 0);
                                        inv.Add("InvCantidadDetalle", 0);
                                        inv.Add("InvFechaActualizacion", Functions.CurrentDate());
                                        inv.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                                        inv.ExecuteUpdate("rowguid = '" + p.rowguid + "' ");
                                        //inv.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ProID = " + p.ProID + " and (AlmID = " + almIdVenta.ToString()+ " or AlmID = " + almIddespacho.ToString()+" ) ");
                                    }
                                    else
                                    {
                                        var inv = new Hash("Inventarios");
                                        inv.Add("invCantidad", 0);
                                        inv.Add("InvCantidadDetalle", 0);
                                        inv.Add("InvFechaActualizacion", Functions.CurrentDate());
                                        inv.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                                        inv.ExecuteUpdate("rowguid = '" + p.rowguid + "' ");
                                        //inv.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ProID = " + p.ProID);
                                    }
                                }

                                // Esto elimina el inventario en 0 cuando cierra cuadre
                                if (parEliminarInventarioEnCero)
                                {
                                    if (parMultiAlmacenes)
                                    {
                                        var inven = new Hash("InventariosAlmacenesRepresentantes");
                                        inven.ExecuteDelete("rowguid = '" + p.rowguid + "' and invCantidad = 0");
                                    }
                                    else
                                    {
                                        var inven = new Hash("Inventarios");
                                        inven.ExecuteDelete("rowguid = '" + p.rowguid + "' and invCantidad = 0");
                                    }
                                }

                            }

                            foreach (var p in inventariodevolucion)
                            {
                                Hash detalle = new Hash("CuadresDetalle");
                                detalle.Add("CuaCantidadFinal", p.invCantidad);
                                detalle.Add("CuaCantidadDetalleFinal", p.InvCantidadDetalle);
                                detalle.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                                detalle.Add("CuaFechaActualizacion", Functions.CurrentDate());

                                if (ExistsProductInDetalleV2(cuaSecuencia, p.ProID, p.InvLote, p.AlmId))
                                {
                                    detalle.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ProID = " + p.ProID +
                                    " and CuaSecuencia = " + cuaSecuencia + " and CuaTipoInventario = " + p.AlmId + " and CuaLote = '" + p.InvLote + "'");
                                }
                                else
                                {
                                    detalle.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                                    detalle.Add("CuaSecuencia", cuaSecuencia);
                                    detalle.Add("CuaTipoInventario", p.AlmId);
                                    detalle.Add("ProID", p.ProID);
                                    detalle.Add("CuaCantidadInicial", 0);
                                    detalle.Add("CuaCantidadDetalleInicial", 0);
                                    detalle.Add("CuaLote", p.InvLote);
                                    detalle.Add("rowguid", Guid.NewGuid().ToString());
                                    detalle.ExecuteInsert();
                                }

                                if (parCuadrarInventario)
                                {
                                    if (parMultiAlmacenes && almIdVenta != -1)
                                    {
                                        var inv = new Hash("InventariosAlmacenesRepresentantes");
                                        inv.Add("invCantidad", 0);
                                        inv.Add("InvCantidadDetalle", 0);
                                        inv.Add("InvFechaActualizacion", Functions.CurrentDate());
                                        inv.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                                        inv.ExecuteUpdate("rowguid = '" + p.rowguid + "' ");
                                        //inv.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ProID = " + p.ProID + " and (AlmID = " + almIdVenta.ToString() + " or AlmID = " + almIdDev.ToString() + " ) ");
                                    }
                                    else
                                    {
                                        var inv = new Hash("Inventarios");
                                        inv.Add("invCantidad", 0);
                                        inv.Add("InvCantidadDetalle", 0);
                                        inv.Add("InvFechaActualizacion", Functions.CurrentDate());
                                        inv.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                                        inv.ExecuteUpdate("rowguid = '" + p.rowguid + "' ");
                                        //inv.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ProID = " + p.ProID);
                                    }
                                }

                                // Esto elimina el inventario en 0 cuando cierra cuadre
                                if (parEliminarInventarioEnCero)
                                {
                                    if (parMultiAlmacenes && almIdVenta != -1)
                                    {
                                        var inven = new Hash("InventariosAlmacenesRepresentantes");
                                        inven.ExecuteDelete("rowguid = '" + p.rowguid + "' and invCantidad = 0");
                                    }
                                    else
                                    {
                                        var inven = new Hash("Inventarios");
                                        inven.ExecuteDelete("rowguid = '" + p.rowguid + "' and invCantidad = 0");
                                    }
                                }

                            }
                        }



                    }
                    else
                    {
                        foreach (var p in inventariodevolucion)
                        {
                            Hash detalle = new Hash("CuadresDetalle");
                            detalle.Add("CuaCantidadFinal", p.invCantidad);
                            detalle.Add("CuaCantidadDetalleFinal", p.InvCantidadDetalle);
                            detalle.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                            detalle.Add("CuaFechaActualizacion", Functions.CurrentDate());

                            if (ExistsProductInDetalle(cuaSecuencia, p.ProID))
                            {
                                detalle.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ProID = " + p.ProID + " and CuaSecuencia = " + cuaSecuencia);
                            }
                            else
                            {
                                detalle.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                                detalle.Add("CuaSecuencia", cuaSecuencia);
                                detalle.Add("CuaTipoInventario", p.AlmId);
                                detalle.Add("ProID", p.ProID);
                                detalle.Add("CuaCantidadInicial", 0);
                                detalle.Add("CuaCantidadDetalleInicial", 0);
                                detalle.Add("CuaLote", p.InvLote);
                                detalle.Add("rowguid", Guid.NewGuid().ToString());
                                detalle.ExecuteInsert();
                            }

                            if (parCuadrarInventario)
                            {
                                if (parMultiAlmacenes && almIdVenta != -1)
                                {
                                    var inv = new Hash("InventariosAlmacenesRepresentantes");
                                    inv.Add("invCantidad", 0);
                                    inv.Add("InvCantidadDetalle", 0);
                                    inv.Add("InvFechaActualizacion", Functions.CurrentDate());
                                    inv.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                                    inv.ExecuteUpdate("rowguid = '" + p.rowguid + "' ");
                                    //inv.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ProID = " + p.ProID + " and (AlmID = " + almIdVenta.ToString() + " or AlmID = " + almIdDev.ToString() + " ) ");
                                }
                                else
                                {
                                    var inv = new Hash("Inventarios");
                                    inv.Add("invCantidad", 0);
                                    inv.Add("InvCantidadDetalle", 0);
                                    inv.Add("InvFechaActualizacion", Functions.CurrentDate());
                                    inv.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                                    inv.ExecuteUpdate("rowguid = '" + p.rowguid + "' ");
                                    //inv.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ProID = " + p.ProID);
                                }
                            }

                            // Esto elimina el inventario en 0 cuando cierra cuadre
                            if (parEliminarInventarioEnCero)
                            {
                                if (parMultiAlmacenes && almIdVenta != -1)
                                {
                                    var inven = new Hash("InventariosAlmacenesRepresentantes");
                                    inven.ExecuteDelete("rowguid = '" + p.rowguid + "' and invCantidad = 0");
                                }
                                else
                                {
                                    var inven = new Hash("Inventarios");
                                    inven.ExecuteDelete("rowguid = '" + p.rowguid + "' and invCantidad = 0");
                                }
                            }

                        }
                    }
                }

            }
            else
            {
                if (parCuadrarInventario)
                {
                    var inventario = new List<Model.Inventarios>();
                    var inventariodespacho = new List<Model.Inventarios>();
                    var inventariodev = new List<Model.Inventarios>();
                    var inventarioventa = new List<Model.Inventarios>();
                    var inventariomelma = new List<Model.Inventarios>();
                    if (parMultiAlmacenes)
                    {
                        if (almIddespacho != -1)
                        {
                            inventariodespacho = new DS_Inventarios().GetInventario(almId: almIddespacho);
                            foreach (var p in inventariodespacho)
                            {
                                if (parCuadrarInventario)
                                {
                                    var inv = new Hash("InventariosAlmacenesRepresentantes");
                                    inv.Add("invCantidad", 0);
                                    inv.Add("InvCantidadDetalle", 0);
                                    inv.Add("InvFechaActualizacion", Functions.CurrentDate());
                                    inv.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                                    inv.ExecuteUpdate("rowguid = '" + p.rowguid + "' ");

                                }

                                // Esto elimina el inventario en 0 cuando cierra cuadre
                                if (parEliminarInventarioEnCero)
                                {
                                    var inven = new Hash("InventariosAlmacenesRepresentantes");
                                    inven.ExecuteDelete("rowguid = '" + p.rowguid + "' and invCantidad = 0");
                                }
                            }
                        }

                        if (almIdDev != -1)
                        {
                            inventariodev = new DS_Inventarios().GetInventario(almId: almIdDev);
                            foreach (var p in inventariodev)
                            {
                                if (parCuadrarInventario)
                                {
                                    var inv = new Hash("InventariosAlmacenesRepresentantes");
                                    inv.Add("invCantidad", 0);
                                    inv.Add("InvCantidadDetalle", 0);
                                    inv.Add("InvFechaActualizacion", Functions.CurrentDate());
                                    inv.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                                    inv.ExecuteUpdate("rowguid = '" + p.rowguid + "' ");

                                }

                                // Esto elimina el inventario en 0 cuando cierra cuadre
                                if (parEliminarInventarioEnCero)
                                {
                                    var inven = new Hash("InventariosAlmacenesRepresentantes");
                                    inven.ExecuteDelete("rowguid = '" + p.rowguid + "' and invCantidad = 0");
                                }
                            }
                        }

                        if (almIdMelma != -1)
                        {
                            inventariomelma = new DS_Inventarios().GetInventario(almId: almIdMelma);
                            foreach (var p in inventariomelma)
                            {
                                if (parCuadrarInventario)
                                {
                                    var inv = new Hash("InventariosAlmacenesRepresentantes");
                                    inv.Add("invCantidad", 0);
                                    inv.Add("InvCantidadDetalle", 0);
                                    inv.Add("InvFechaActualizacion", Functions.CurrentDate());
                                    inv.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                                    inv.ExecuteUpdate("rowguid = '" + p.rowguid + "' ");

                                }

                                // Esto elimina el inventario en 0 cuando cierra cuadre
                                if (parEliminarInventarioEnCero)
                                {
                                    var inven = new Hash("InventariosAlmacenesRepresentantes");
                                    inven.ExecuteDelete("rowguid = '" + p.rowguid + "' and invCantidad = 0");
                                }
                            }
                        }

                        if (almIdVenta != -1)
                        {
                            inventarioventa = new DS_Inventarios().GetInventario(almId: almIdVenta);
                            foreach (var p in inventarioventa)
                            {
                                if (parCuadrarInventario)
                                {
                                    var inv = new Hash("InventariosAlmacenesRepresentantes");
                                    inv.Add("invCantidad", 0);
                                    inv.Add("InvCantidadDetalle", 0);
                                    inv.Add("InvFechaActualizacion", Functions.CurrentDate());
                                    inv.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                                    inv.ExecuteUpdate("rowguid = '" + p.rowguid + "' ");

                                }

                                // Esto elimina el inventario en 0 cuando cierra cuadre
                                if (parEliminarInventarioEnCero)
                                {
                                    var inven = new Hash("InventariosAlmacenesRepresentantes");
                                    inven.ExecuteDelete("rowguid = '" + p.rowguid + "' and invCantidad = 0");
                                }
                            }
                        }


                    }
                    else
                    {


                        inventario = new DS_Inventarios().GetInventario(almId: -1);
                        foreach (var p in inventario)
                        {
                            if (parCuadrarInventario)
                            {
                                var inv = new Hash("Inventarios");
                                inv.Add("invCantidad", 0);
                                inv.Add("InvCantidadDetalle", 0);
                                inv.Add("InvFechaActualizacion", Functions.CurrentDate());
                                inv.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                                inv.ExecuteUpdate("rowguid = '" + p.rowguid + "' ");

                            }

                            // Esto elimina el inventario en 0 cuando cierra cuadre
                            if (parEliminarInventarioEnCero)
                            {
                                var inven = new Hash("InventariosAlmacenesRepresentantes");
                                inven.ExecuteDelete("rowguid = '" + p.rowguid + "' and invCantidad = 0");
                            }
                        }
                    }
                }

            }



            if (vehId != -1 && myParametro.GetParCuadresUsarContador() && myParametro.GetParCuadres() == 1)
            {
                myVeh.ActualizarContador(vehId, contadorFinal);
            }

            if (vehId != -1 && myParametro.GetParCuadresUsarKilometraje() && myParametro.GetParCuadres() == 1)
            {
                myVeh.ActualizarKilometraje(vehId, kilometroFinal);
            }

            Arguments.Values.CurrentCuaSecuencia = -1;

        }

        public int ActualizarCantidadFisicaCuadre(int CuaSecuencia, int titId)
        {
            var inventario = new DS_Productos().GetProductosMasInventario(titId);

            foreach (var p in inventario)
            {
                Hash cuadre = new Hash("CuadresDetalle");
                cuadre.Add("CuaCantidadFisica", p.Cantidad);
                cuadre.Add("CuaCantidadDetalleFisica", p.CantidadDetalle);
                cuadre.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                cuadre.Add("CuaFechaActualizacion", Functions.CurrentDate());

                if (ExistsProductInDetalle(CuaSecuencia, p.ProID))
                {
                    cuadre.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ProID = " + p.ProID + " and CuaSecuencia = " + CuaSecuencia);
                }
                else
                {
                    cuadre.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                    cuadre.Add("CuaSecuencia", CuaSecuencia);
                    cuadre.Add("CuaTipoInventario", 0);
                    cuadre.Add("ProID", p.ProID);
                    cuadre.Add("CuaCantidadInicial", 0);
                    cuadre.Add("CuaCantidadDetalleInicial", 0);
                    cuadre.Add("CuaCantidadFinal", p.InvCantidad);
                    cuadre.Add("CuaCantidadDetalleFinal", p.InvCantidadDetalle);
                    cuadre.Add("rowguid", Guid.NewGuid().ToString());
                    cuadre.ExecuteInsert();
                }

            }

            return CuaSecuencia;
        }

        public Cuadres GetCuadreAbierto(string fecha = null /*formato de la fecha dd-MM-yyyy*/, bool invert = false)
        {
            var where = "";

            if (!string.IsNullOrWhiteSpace(fecha))
            {
                where = " and ifnull(replace( strftime('%d-%m-%Y', SUBSTR(CuaFechaInicio,1,10)),' ','' ), '') " + (invert ? "<>" : "=") + " '" + fecha + "' ";
            }

            var query = "select CuaSecuencia, RepCodigo, VehID, RepAyudante1, " +
                        "RepAyudante2, CuaContadorInicial, CuaKilometrosInicio, CuaFechaInicio, CuaFechaFin, CuaEstatus, rowguid from Cuadres where ltrim(rtrim(RepCodigo)) = ? " +
                        "and CuaEstatus = ? " + where + " order by cast(CuaSecuencia as integer) desc limit 1";

            var list = SqliteManager.GetInstance().Query<Cuadres>(query, new string[] { Arguments.CurrentUser.RepCodigo.Trim(), "1" });

            if (list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public async void AbrirCerrarCuadre(bool validarCuadreDiario, Action<bool,int> CuadreGuardado, bool preguntar = true, bool goForced = false, bool validatePhotoDeposit = false, string RepAuditor = "")
        {
            try
            {
                var parCuadres = myParametro.GetParCuadres();

                if (validarCuadreDiario)
                {
                    var cuadreDeOtraFecha = GetCuadreAbierto(DateTime.Now.ToString("dd-MM-yyyy"), true);

                    if (cuadreDeOtraFecha != null)
                    {
                        var result = await Application.Current.MainPage.DisplayAlert(AppResource.Warning, AppResource.CloseDailySquareToContinue, AppResource.CloseSquare, AppResource.Cancel);

                        if (result)
                        {
                            if (myParametro.GetParCuadresCerrarSiTieneDeposito())
                            {
                                var myDep = new DS_Depositos();
                                bool hayRecibosSinDeposito = myDep.HayRecibosParaDepositarForCuadres();
                                var depSecuencia = GetDepositoFromCuadre(cuadreDeOtraFecha.CuaSecuencia);
                                if (depSecuencia == -1 && hayRecibosSinDeposito)
                                {
                                    await Functions.DisplayAlert(AppResource.Warning, AppResource.NotDepositInSquare, AppResource.Aceptar);
                                    return;
                                }
                            }

                            if (myParametro.GetParCuadresNoCerrarConCargas())
                            {
                                if (myParametro.GetParCuadresDiarios())
                                {
                                    if (MyCar.HayCargasDisponiblesInCuadreByDia(cuadreDeOtraFecha.CuaFechaInicio))
                                    {
                                        await Application.Current.MainPage.DisplayAlert(AppResource.Warning, AppResource.LoadsBeforeSquares, AppResource.Aceptar);
                                        return;
                                    }
                                }
                                else
                                {
                                    if (MyCar.HayCargasDisponiblesInCuadre(cuadreDeOtraFecha.CuaFechaInicio))
                                    {
                                        await Application.Current.MainPage.DisplayAlert(AppResource.Warning, AppResource.LoadsBeforeSquares, AppResource.Aceptar);
                                        return;
                                    }
                                }
                            }

                            if (myParametro.GetParCuadres() == 2)
                            {
                                CerrarCuadre(cuadreDeOtraFecha.CuaSecuencia, Arguments.Values.CurrentLocation, 0, 0, 0, cuadreDeOtraFecha.rowguid);
                                var resultado = await Functions.DisplayAlert(AppResource.Warning, AppResource.SquareClosedSuccessfully, AppResource.Print, AppResource.Aceptar);

                                if(resultado)
                                {
                                    Arguments.Values.CurrentCuaSecuencia = cuadreDeOtraFecha.CuaSecuencia;
                                    CuadreGuardado?.Invoke(true, 1);
                                }
                                else
                                {
                                    CuadreGuardado?.Invoke(true, 0);
                                }
                            }
                            else
                            {
                                await Application.Current.MainPage.Navigation.PushAsync(new CuadresPage(cuadreDeOtraFecha, CuadreGuardado));
                            }
                        }

                        return;
                    }
                }



                var cuadreAbierto = GetCuadreAbierto(validarCuadreDiario ? DateTime.Now.ToString("dd-MM-yyyy") : null);

                if (cuadreAbierto == null) //abrir cuadre
                {
                    var result = true;

                    if ((preguntar || parCuadres > 1) && !goForced)
                    {
                        result = await Application.Current.MainPage.DisplayAlert(AppResource.Warning, "No tienes un cuadre abierto" + (myParametro.GetParCuadresDiarios() ? " para el dia de hoy" : "") + ", deseas abrir uno?", AppResource.OpenSquare, AppResource.Cancel);
                    }

                    if (!result)
                    {
                        return;
                    }

                    if (!MyCar.HayCargasDisponibles() && DS_RepresentantesParametros.GetInstance().GetParNoAperturarCuadreSinCarga())
                    {
                        await Application.Current.MainPage.DisplayAlert(AppResource.Warning, AppResource.NotLoadAvailablesToOpenSquare, AppResource.Aceptar);
                        return;
                    }

                    if (parCuadres == 2)
                    {
                        AbrirCuadre(Arguments.Values.CurrentLocation, 0, "", "", 0, 0, RepAuditor: RepAuditor);
                        await Functions.DisplayAlert(AppResource.Success, AppResource.SquareOpenSuccessfully, AppResource.Aceptar);
                        CuadreGuardado?.Invoke(false,0);
                    }
                    else
                    {
                        await Application.Current.MainPage.Navigation.PushAsync(new CuadresPage(null, CuadreGuardado, RepAuditor: RepAuditor));
                    }
                }
                else //cerra cuadre
                {
                    var result = true;

                    if ((preguntar || parCuadres == 2) && !goForced)
                    {
                        result = await Functions.DisplayAlert(AppResource.Warning, AppResource.CloseSquareQuestion, AppResource.CloseSquare, AppResource.Cancel);
                    }

                    if (!result)
                    {
                        return;
                    }

                    if (myParametro.GetParCuadresNoCerrarConCargas())
                    {
                        if (myParametro.GetParCuadresDiarios())
                        {
                            if (MyCar.HayCargasDisponiblesInCuadreByDia(cuadreAbierto.CuaFechaInicio))
                            {
                                await Application.Current.MainPage.DisplayAlert(AppResource.Warning, AppResource.LoadsBeforeSquares, AppResource.Aceptar);
                                return;
                            }
                        }
                        else
                        {
                            if (MyCar.HayCargasDisponiblesInCuadre(cuadreAbierto.CuaFechaInicio))
                            {
                                await Application.Current.MainPage.DisplayAlert(AppResource.Warning, AppResource.LoadsBeforeSquares, AppResource.Aceptar);
                                return;
                            }
                        }
                    }

                    var parValidarClientesAVisitar = myParametro.GetParCuadresValidarClientesVisitadosParaCerrar();

                    if (!string.IsNullOrWhiteSpace(parValidarClientesAVisitar) && (parValidarClientesAVisitar == "1" || parValidarClientesAVisitar.Contains("2|")))
                    {
                        var clientesSinVisitar = new DS_RutaVisitas().GetClientesSinVisitar();

                        if (clientesSinVisitar.Count > 0)
                        {
                            if (parValidarClientesAVisitar == "1")
                            {
                                await Application.Current.MainPage.DisplayAlert(AppResource.Warning, AppResource.ClientsWithoutVisitsWarning, AppResource.Aceptar);
                                return;
                            }
                            else if (parValidarClientesAVisitar.Contains("2|"))
                            {
                                var crearVisitasAuto = await Application.Current.MainPage.DisplayAlert(AppResource.Warning, AppResource.CreateAutomaticFailedVisits, AppResource.CreateAutomaticVisits, AppResource.Cancel);

                                if (crearVisitasAuto)
                                {
                                    var array = parValidarClientesAVisitar.Split('|');
                                    new DS_Visitas().CrearVisitasFallidas(clientesSinVisitar, array[1], Arguments.Values.CurrentLocation);
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                    }

                    if (myParametro.GetParCuadresCerrarSiTieneDeposito())
                    {
                        var myDep = new DS_Depositos();
                        bool hayRecibosSinDeposito = myDep.HayRecibosParaDepositarForCuadres();
                        var depSecuencia = GetDepositoFromCuadre(cuadreAbierto.CuaSecuencia);
                        if (depSecuencia == -1 && hayRecibosSinDeposito)
                        {
                            await Functions.DisplayAlert(AppResource.Warning, AppResource.NotDepositInSquare, AppResource.Aceptar);
                            return;
                        }
                    }

                    if (parCuadres == 2)
                    {
                        CerrarCuadre(cuadreAbierto.CuaSecuencia, Arguments.Values.CurrentLocation, 0, 0, 0, cuadreAbierto.rowguid);
                        var resultado = await Functions.DisplayAlert(AppResource.Warning, AppResource.SquareClosedSuccessfully, AppResource.Print, AppResource.Aceptar);

                        if (resultado)
                        {
                            Arguments.Values.CurrentCuaSecuencia = cuadreAbierto.CuaSecuencia;
                            CuadreGuardado?.Invoke(true, 1);
                        }
                        else
                        {
                            CuadreGuardado?.Invoke(true,0);
                        }
                        //////////////////////////////////////////////////////////////////////////////////////////////////////////
                        var cuaAbierto = GetCuadreAbierto(validarCuadreDiario ? DateTime.Now.ToString("dd-MM-yyyy") : null);
                        if (cuaAbierto != null)
                        {
                            // CerrarCuadre(cuadreAbierto.CuaSecuencia, Arguments.Values.CurrentLocation, 0, 0, 0, cuadreAbierto.rowguid);
                            AbrirCerrarCuadre(validarCuadreDiario, CuadreGuardado, preguntar, goForced);
                        }
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    }
                    else
                    {
                        await Application.Current.MainPage.Navigation.PushAsync(new CuadresPage(cuadreAbierto, CuadreGuardado));
                    }
                }
            }
            catch (Exception e)
            {
                await Functions.DisplayAlert(AppResource.Warning, e.Message);
            }
        }

        private int GetDepositoFromCuadre(int cuaSecuencia)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<Cuadres>("select DepSecuencia as CuaSecuencia from Depositos " +
                    "where trim(RepCodigo) = ? and CuaSecuencia = " + cuaSecuencia.ToString(),
                    new string[] { Arguments.CurrentUser.RepCodigo.Trim() });

                if (list != null && list.Count > 0)
                {
                    return list[0].CuaSecuencia;
                }

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return -1;
        }

        private bool DepositoTieneFoto(int depSecuencia)
        {
            return SqliteManager.GetInstance().Query<Cuadres>("select TieneFoto from Depositos " +
                "where DepSecuencia = ? and trim(RepCodigo) = ? and ifnull(TieneFoto, '0') = '1' ", 
                new string[] { depSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() }).Count > 0;
        }

        public bool HoyHizoCuadre()
        {
            var list = SqliteManager.GetInstance().Query<Cuadres>("select CuaSecuencia from Cuadres " +
                "where ifnull(replace( strftime('%d-%m-%Y', SUBSTR(CuaFechaInicio,1,10)),' ','' ), '') = '" + Functions.CurrentDate("dd-MM-yyyy") + "' and trim(RepCodigo) = ? ", new string[] { Arguments.CurrentUser.RepCodigo.Trim() });

            return list != null && list.Count > 0;
        }

        private bool ExistsProductInDetalleV2(int cuaSecuencia, int proId, string cuaLote , int almid )
        {
            var list = SqliteManager.GetInstance().Query<CuadresDetalle>("select ProID from CuadresDetalle " +
                "where CuaSecuencia = ? and ProID = ? and trim(RepCodigo) = ? and trim(CuaLote) = ? and CuaTipoInventario = ?",
                new string[] { cuaSecuencia.ToString(), proId.ToString(), Arguments.CurrentUser.RepCodigo.Trim(), cuaLote , almid.ToString() });

            return list != null && list.Count > 0;
        }

        private bool ExistsProductInDetalle(int cuaSecuencia, int proId)
        {
            var list = SqliteManager.GetInstance().Query<CuadresDetalle>("select ProID from CuadresDetalle " +
                "where CuaSecuencia = ? and ProID = ? and trim(RepCodigo) = ?",
                new string[] { cuaSecuencia.ToString(), proId.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            return list != null && list.Count > 0;
        }  

        public Cuadres GetCuadresBySecuencia(int cuaSecuencia)
        {
            var list = SqliteManager.GetInstance().Query<Cuadres>("select RepCodigo, CuaEstatus, CuaFechaInicio, CuaFechaFin,  " +
                " e.EstDescripcion as EstadoDescripcion, CuaSecuencia, RutID,  VehID, CuaSecuencia from " +
                "Cuadres c inner join Estados e on e.EstTabla = 'Cuadres' and e.EstEstado = c.CuaEstatus " +
                "where ltrim(rtrim(c.RepCodigo)) = ? and c.CuaSecuencia = ?", new string[] { Arguments.CurrentUser.RepCodigo.Trim(), cuaSecuencia.ToString() });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public Cuadres GetCuadresBySecuenciaConAyudante(int cuaSecuencia)
        {
            var list = SqliteManager.GetInstance().Query<Cuadres>("select RepCodigo, CuaEstatus, CuaFechaInicio, CuaFechaFin,  " +
                " e.EstDescripcion as EstadoDescripcion, CuaSecuencia, RutID,  VehID, CuaSecuencia, Isnull(RepAyudante1,'') RepAyudante1, Isnull(RepAyudante2,'') RepAyudante2 from " +
                "Cuadres c inner join Estados e on e.EstTabla = 'Cuadres' and e.EstEstado = c.CuaEstatus " +
                "where ltrim(rtrim(c.RepCodigo)) = ? and c.CuaSecuencia = ?", new string[] { Arguments.CurrentUser.RepCodigo.Trim(), cuaSecuencia.ToString() });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public List<CuadresDetalle> GetCuadresDetalleBySecuencia(int cuaSecuencia)
        {
            return SqliteManager.GetInstance().Query<CuadresDetalle>("select RepCodigo,	CuaSecuencia, CuaTipoInventario, c.ProID,	CuaCantidadInicial,	CuaCantidadDetalleInicial,	CuaCantidadFinal, CuaCantidadDetalleFinal, CuaCantidadFisica, CuaCantidadDetalleFisica,p.UnmCodigo as UnmCodigo, p.ProCodigo as ProCodigo, " +
                " p.ProDescripcion as ProDescripcion, CAST(substr(ProCodigo, INSTR(ProCodigo, '-')+1, " +
                "length(Procodigo)) AS INT ) as FAMILIA, CAST(substr(ProCodigo, 0, INSTR(ProCodigo, '-')) AS INT) " +
                "as CODIGO, P.ProDatos3 AS ProDatos3 " +
                "from CuadresDetalle c inner join Productos p on p.ProID = c.ProID  " +
                "where ltrim(rtrim(c.RepCodigo)) = ? and c.CuaSecuencia = ? order by FAMILIA asc, CODIGO asc", new string[] { Arguments.CurrentUser.RepCodigo.Trim(), cuaSecuencia.ToString() });

        }

        public List<CuadresDetalle> GetCuadresDetalleBySecuenciaByAlmacen(int cuaSecuencia, int Almacen)
        {
            return SqliteManager.GetInstance().Query<CuadresDetalle>("select c.RepCodigo,	CuaSecuencia, CuaTipoInventario, c.ProID,	CuaCantidadInicial,	CuaCantidadDetalleInicial,	CuaCantidadFinal, CuaCantidadDetalleFinal, CuaCantidadFisica, CuaCantidadDetalleFisica,p.UnmCodigo as UnmCodigo, p.ProCodigo as ProCodigo, " +
                " p.ProDescripcion as ProDescripcion, CAST(substr(ProCodigo, INSTR(ProCodigo, '-')+1, " +
                "length(Procodigo)) AS INT ) as FAMILIA, CAST(substr(ProCodigo, 0, INSTR(ProCodigo, '-')) AS INT) " +
                "as CODIGO, ifnull(ia.InvLote,'') as InvLote, ProUnidades " +
                "from CuadresDetalle c inner join Productos p on p.ProID = c.ProID  " +
                "inner join InventariosAlmacenesRepresentantes ia on ia.proid = c.Proid and ia.AlmId = c.CuaTipoInventario and ifnull(ia.InvLote, '') = ifnull(c.CuaLote, '')  " +
                "where ltrim(rtrim(c.RepCodigo)) = ? and c.CuaSecuencia = ? and ia.AlmId = ? order by FAMILIA asc, CODIGO asc", new string[] { Arguments.CurrentUser.RepCodigo.Trim(), cuaSecuencia.ToString(), Almacen.ToString() });

        }

        public List<CuadresDetalle> GetCuadresDetalleBySecuenciaByAlmacenAgrupadoPorProducto(int cuaSecuencia, int Almacen)
        {
            return SqliteManager.GetInstance().Query<CuadresDetalle>("select c.RepCodigo,	CuaSecuencia, CuaTipoInventario, c.ProID,	Sum(CuaCantidadInicial) as CuaCantidadInicial,	CuaCantidadDetalleInicial, Sum(CuaCantidadFinal) as	CuaCantidadFinal, CuaCantidadDetalleFinal, CuaCantidadFisica, CuaCantidadDetalleFisica,p.UnmCodigo as UnmCodigo, p.ProCodigo as ProCodigo, " +
                " p.ProDescripcion as ProDescripcion, CAST(substr(ProCodigo, INSTR(ProCodigo, '-')+1, " +
                "length(Procodigo)) AS INT ) as FAMILIA, CAST(substr(ProCodigo, 0, INSTR(ProCodigo, '-')) AS INT) " +
                "as CODIGO, ProUnidades " +
                "from CuadresDetalle c inner join Productos p on p.ProID = c.ProID  " +
                "inner join InventariosAlmacenesRepresentantes ia on ia.proid = c.Proid and ia.AlmId = c.CuaTipoInventario and ifnull(ia.InvLote, '') = ifnull(c.CuaLote, '')  " +
                "where ltrim(rtrim(c.RepCodigo)) = ? and c.CuaSecuencia = ? and ia.AlmId = ? Group by c.proid order by FAMILIA asc, CODIGO asc", new string[] { Arguments.CurrentUser.RepCodigo.Trim(), cuaSecuencia.ToString(), Almacen.ToString() });

        }


        /// <summary>
        /// Verificar si existe un cuadre abierto
        /// </summary>
        /// <returns></returns>
        public bool GetVerSiHayCuadreAbierto()
        {
            var list = SqliteManager.GetInstance().Query<Cuadres>("select cuaestatus as CuaEstatus from cuadres order by cast(cuasecuencia as integer) desc",
                new string[] { });
            if (list[0].CuaEstatus == 1)
            {
                return true;
            }
            return false;
        }

        public List<CuadresDetalle> GetConteoFisicoByCuadre(int CuaSecuencia)
        {
                return SqliteManager.GetInstance().Query<CuadresDetalle>("select cd.CuaCantidadFinal as CuaCantidadFinal, "
                        + "cd.CuaCantidadFisica as CuaCantidadFisica, P.ProDescripcion as ProDescripcion, "
                        + "P.ProCodigo as ProCodigo, P.UnmCodigo as UnmCodigo, cd.ProID as ProID, cd.CuaCantidadDetalleFinal as CuaCantidadDetalleFinal, " +
                        "cd.CuaCantidadDetalleFisica as CuaCantidadDetalleFisica "
                        + "from CuadresDetalle cd inner join Productos P on P.ProID = cd.ProID "
                        + "where cd.CuaSecuencia = " + CuaSecuencia + " Order by P.LinID, P.Cat1ID, P.ProID", new string[] { });

        }

      
        public string GetPrecioInProductos(int ProID)
        {
            var Precio = SqliteManager.GetInstance().Query<model.Productos>("Select ProPrecio from Productos Where ProID = "+ProID+" ", new string[] { });

            if (Precio != null && Precio.Count > 0)
            {
                var Itbis = GetItbisInProductos(ProID);
                double precio = Math.Round(Precio[0].ProPrecio, 2);
                double PrecioProducto = Math.Round(precio + (precio * (Itbis / 100)), 2);
                if (Precio.Count > 0)
                {
                    return PrecioProducto.ToString();
                }
            }
            else
            {
                return Math.Round(0.0, 2).ToString();
            }

            return Math.Round(0.0, 2).ToString();
            
        }

        public double GetItbisInProductos(int ProID)
        {
            var Producto = SqliteManager.GetInstance().Query<model.Productos>("Select ProItbis from Productos Where ProID = " + ProID + " ", new string[] { });

            if (Producto.Count > 0)
            {
                return Math.Round(Producto[0].ProItbis,2);
            }

            return 0;

        }
        
        public string GetPrecioinListaPrecio(int ProID, string LipCodigo)
        {
            var Precio = SqliteManager.GetInstance().Query<model.ListaPrecios>("Select LipPrecio from ListaPrecios Where ProID = " + ProID + " and Lipcodigo = '"+LipCodigo+"' ", new string[] { });

            if (Precio != null && Precio.Count > 0)
            {
                var Itbis = GetItbisInProductos(ProID);
                double precio = Math.Round(Precio[0].LipPrecio, 2);
                double PrecioProducto = Math.Round(precio + (precio * (Itbis / 100)), 2);
                if (Precio.Count > 0)
                {
                    return PrecioProducto.ToString();
                }
            }
            else
            {
                return Math.Round(0.0, 2).ToString();
            }

            return Math.Round(0.0, 2).ToString();

        }
        public CuadresDetalle GetCuadreForCant(int cuaSecuencia, int proId)
        {
            var cuadre = SqliteManager.GetInstance().Query<CuadresDetalle>("select CuaCantidadInicial from CuadresDetalle " +
                "where CuaSecuencia = ? and ProID = ? and trim(RepCodigo) = ?",
                new string[] { cuaSecuencia.ToString(), proId.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            return cuadre.FirstOrDefault();
        }
        public int GetCuaContadorInicial(int CuaSecuencia)
        {

            var myResult = SqliteManager.GetInstance().Query<Cuadres>("select CuaCantidadInicial from CuadresDetalle where CuaSecuencia = ?",
                            new string[] { CuaSecuencia.ToString()}).FirstOrDefault();

            if(myResult != null)
            {
               return myResult.CuaContadorIniciar;
            }

            return 0;
        }
        public int GetCuaContadorFinal(int CuaSecuencia)
        {

            var myResult = SqliteManager.GetInstance().Query<Cuadres>("select CuaContadorFinal from Cuadres where CuaSecuencia = ?",
                            new string[] { CuaSecuencia.ToString() }).FirstOrDefault();

            if (myResult != null)
            {
                return myResult.CuaContadorFinal;
            }

            return 0;
        }

        public int GetVehContador(String Vehiculo)
        {
            var myResult = SqliteManager.GetInstance().Query<Vehiculos>("select ifnull(VehContador, 0) from vehiculos where vehFicha = '" + Vehiculo + "' ",
                            new string[] { }).FirstOrDefault();

            if(myResult != null)
            {
                return myResult.VehContador;
            }

            return 0;
        }

        public List<PedidosDetalle> GetPedidosaContadoByCuaSecuencia(int cuaSecuencia)
        {
            return SqliteManager.GetInstance().Query<PedidosDetalle>("select c.clinombre, (pd.pedprecio * pd.pedcantidad) as PedidosTotal, p.pedsecuencia as PedSecuencia " +
                                                                     "from pedidos p inner join clientes c on c.cliid = p.cliid " +
                                                                     "inner join pedidosdetalle pd on p.pedsecuencia = pd.pedsecuencia " +
                                                                     "inner join condicionespago cd on cd.conid = p.conid "+
                                                                     "where cd.condiasvencimiento = 0 and p.cuasecuencia =? ", new string[] { cuaSecuencia.ToString() });
        }

        public List<PedidosDetalle> GetPedidosaCreditoByCuaSecuencia(int cuaSecuencia)
        {
            return SqliteManager.GetInstance().Query<PedidosDetalle>("select c.clinombre, (pd.pedprecio * pd.pedcantidad) as PedidosTotal, p.pedsecuencia as PedSecuencia " +
                                                                     "from pedidos p inner join clientes c on c.cliid = p.cliid " +
                                                                     "inner join pedidosdetalle pd on p.pedsecuencia = pd.pedsecuencia " +
                                                                     "inner join condicionespago cd on cd.conid = p.conid "+
                                                                     "where cd.condiasvencimiento <> 0 and p.cuasecuencia =? ", new string[] { cuaSecuencia.ToString() });
        }

        public List<Recibos> GetCuandreImprimirPedidosCobros(int CuaSecuencia)
        {
            return SqliteManager.GetInstance().Query<Recibos>("SELECT c.clinombre, r.RecTotal, r.RecSecuencia AS 'RecSecuencia' from Recibos r " +
                                                              "INNER JOIN clientes c on r.cliid = c.cliid " +
                                                              "WHERE r.RecTipo = '2' AND r.CuaSecuencia = ? ", new string[] { CuaSecuencia.ToString() });
        }

        public Cuadres GetCuandreImprimirVentas(int CuaSecuencia)
        {
            return SqliteManager.GetInstance().Query<Cuadres>("SELECT '' as ProDescripcion, c.RepAyudante1, c.RepAyudante2, '' as CuaCantidadInicial, " +
                                                              "c.RutID, h.VehFicha, '' as CuaCantidadfinal, ifnull(c.CuaEstatus, 0) AS 'CuaEstatus' " +
                                                              "FROM Cuadres c INNER JOIN Vehiculos h ON h.VehID = c.VehID " +
                                                              "WHERE c.CuaSecuencia = ", new string[] { CuaSecuencia.ToString() }).FirstOrDefault();
        }

        public int GetKmInicial(int CuaSecuencia)
        {
            var myResult = SqliteManager.GetInstance().Query<Cuadres>("select CuaKilometrosInicio from cuadres where CuaSecuencia = ? ",
                                                              new string[] { CuaSecuencia.ToString() }).FirstOrDefault();

            if (myResult != null)
            {
                return myResult.CuaKilometrosInicio;
            }

            return 0;
        }

        public int GetKmFin(int CuaSecuencia)
        {
            var myResult = SqliteManager.GetInstance().Query<Cuadres>("select CuaKilometrosInicio from cuadres where CuaSecuencia = ? ",
                                                              new string[] { CuaSecuencia.ToString() }).FirstOrDefault();

            if (myResult != null)
            {
                return myResult.CuaKilometrosFin;
            }

            return 0;
        }

        public int GetKmVehiculo(string Vehiculo)
        {
            var myResult = SqliteManager.GetInstance().Query<Vehiculos>("select VehKilometraje from vehiculos where vehFicha = '" + Vehiculo + "'",
                                                              new string[] { }).FirstOrDefault();

            if (myResult != null)
            {
                return myResult.VehKilometraje;
            }

            return 0;
        }

    }
}
