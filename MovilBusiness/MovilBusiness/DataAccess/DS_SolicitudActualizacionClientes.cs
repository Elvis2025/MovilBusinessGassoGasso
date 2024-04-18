using MovilBusiness.Configuration;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.model.Internal.Structs.Args;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MovilBusiness.DataAccess
{
    public class DS_SolicitudActualizacionClientes : DS_Controller
    {

        public int GuardarSAC(SACArgs args)
        {
            bool somethingChanged = false;

            Hash map = new Hash("SolicitudActualizacionClientes");

            int sacSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("SolicitudActualizacionClientes");

            var changesNames = new List<string>();

            if(args.Horarios != null && args.Horarios.Count > 0)
            {
                somethingChanged = true;

                var horariosString = "";

                foreach(var horario in args.Horarios)
                {
                    horariosString += (string.IsNullOrWhiteSpace(horariosString) ? "" : ", ") + "" + horario.ClhDia + "|"+horario.ClhHorarioApertura+"|"+horario.ClhHorarioCierre;
                }

                map.Add("CliHorarios", horariosString);
            }

            if (args.Nombre != (Arguments.Values.CurrentClient.CliNombre ?? ""))
            {
                somethingChanged = true;
                changesNames.Add("CliNombre");
            }

            if(args.Propietario != (Arguments.Values.CurrentClient.CliPropietario ?? ""))
            {
                somethingChanged = true;
                changesNames.Add("CliPropietario");
            }

            if(args.Email != (Arguments.Values.CurrentClient.CliCorreoElectronico ?? ""))
            {
                somethingChanged = true;
                changesNames.Add("CliCorreoElectronico");
            }

            if(args.Contacto != (Arguments.Values.CurrentClient.CliContacto ?? ""))
            {
                somethingChanged = true;
                changesNames.Add("CliContacto");
            }

            if(args.Telefono != (Arguments.Values.CurrentClient.CliTelefono ?? ""))
            {
                somethingChanged = true;
                changesNames.Add("CliTelefono");
            }

            if(args.Fax != (Arguments.Values.CurrentClient.CliFax ?? ""))
            {
                somethingChanged = true;
                changesNames.Add("CliFax");
            }

            if(args.Depositos != Arguments.Values.CurrentClient.CliIndicadorDeposito)
            {
                somethingChanged = true;
                changesNames.Add("CliIndicadorDeposito");
            }

            if(args.EncargadoPago != (Arguments.Values.CurrentClient.CliEncargadoPago ?? ""))
            {
                somethingChanged = true;
                changesNames.Add("CliEncargadoPago");
            }

            if(args.Latitud != Arguments.Values.CurrentClient.CliLatitud)
            {
                somethingChanged = true;
                changesNames.Add("CliLatitud");
            }

            if(args.Longitud != Arguments.Values.CurrentClient.CliLongitud)
            {
                somethingChanged = true;
                changesNames.Add("CliLongitud");
            }

            if(args.Cedula != (Arguments.Values.CurrentClient.CliCedulaPropietario ?? ""))
            {
                somethingChanged = true;
                changesNames.Add("CliCedulaPropietario");
            }

            if(args.SitioWeb != (Arguments.Values.CurrentClient.CliPaginaWeb ?? ""))
            {
                somethingChanged = true;
                changesNames.Add("CliPaginaWeb");
            }

            if(args.OrdenCompra != Arguments.Values.CurrentClient.CliIndicadorOrdenCompra)
            {
                somethingChanged = true;
                changesNames.Add("CliIndicadorOrdenCompra");
            }

            if(args.DepositaFactura != Arguments.Values.CurrentClient.CliIndicadorDepositaFactura)
            {
                somethingChanged = true;
                changesNames.Add("CliDepositaFactura");
            }

            if(args.Sector != (Arguments.Values.CurrentClient.SecCodigo ?? "") && !string.IsNullOrWhiteSpace(args.Sector))
            {
                somethingChanged = true;
                changesNames.Add("SecCodigo");
            }

            if(args.Direccion != (Arguments.Values.CurrentClient.CliCalle ?? ""))
            {
                somethingChanged = true;
                changesNames.Add("CliCalle");
            }

            if(args.ProvinciaId != -1 && args.ProvinciaId != Arguments.Values.CurrentClient.ProID)
            {
                somethingChanged = true;
                changesNames.Add("ProID");
            }

            if(args.MunicipioId != -1 && args.MunicipioId != Arguments.Values.CurrentClient.MunID)
            {
                somethingChanged = true;
                changesNames.Add("MunID");
            }

            if (!string.IsNullOrWhiteSpace(args.CliRegMercantil) && args.CliRegMercantil != (Arguments.Values.CurrentClient.CliRegMercantil ?? ""))
            {
                somethingChanged = true;
                changesNames.Add("CliRegMercantil");
            }

            if (args.CliRutPosicion > 0 && args.CliRutPosicion != Arguments.Values.CurrentClient.CliRutPosicion)
            {
                somethingChanged = true;
                changesNames.Add("CliRutPosicion");
            }
                                 
            if(!DateTime.TryParse(Arguments.Values.CurrentClient.CliContactoFechaNacimiento, out DateTime fechaNac))
            {
                fechaNac = DateTime.Now;
            }

            var fechaNacChanged = false;
            if (args.FechaNacContacto != null && args.FechaNacContacto.ToString("yyyy-MM-dd") != fechaNac.ToString("yyyy-MM-dd"))
            {
                changesNames.Add("CliContactoFechaNacimiento");
                somethingChanged = true;
                fechaNacChanged = true;
            }

            if (!somethingChanged)
            {
                throw new Exception("No has modificado ningún valor");
            }

            map.Add("CliNombre", args.Nombre);
            map.Add("CliPropietario", args.Propietario);
            map.Add("CliCorreoElectronico", args.Email);
            map.Add("CliContacto", args.Contacto);
            map.Add("CliRNC", DS_RepresentantesParametros.GetInstance().GetParSACCliRNCCedula() ? args.Cedula.ToString() : "");
            map.Add("CliTelefono", args.Telefono);
            map.Add("CliFax", args.Fax);
            map.Add("CliIndicadorDeposito", args.Depositos);
            map.Add("CliEncargadoPago", args.EncargadoPago);
            map.Add("CliLatitud", args.Latitud);
            map.Add("CliLongitud", args.Longitud);
            map.Add("CliCedulaPropietario", args.Cedula.ToString());
            map.Add("CliPaginaWeb", args.SitioWeb);
            map.Add("CliIndicadorOrdenCompra", args.OrdenCompra);
            map.Add("CliIndicadorDepositaFactura", args.DepositaFactura);          
            map.Add("cliSector", args.CliSector);
            map.Add("CliUrbanizacion", args.CliUrbanizacion);
            map.Add("SecCodigo", args.Sector);
            map.Add("CliCalle", args.Direccion);
            map.Add("CliCasa", args.CliCasa);
            map.Add("IndicadorCredito", 0);
            map.Add("ProID", args.ProvinciaId);
            map.Add("MunID", args.MunicipioId);
            map.Add("CliOrdenRuta", args.CliOrdenRuta);
            if (fechaNacChanged)
            {
                map.Add("CliContactoFechaNacimiento", args.FechaNacContacto.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            map.Add("ClaID", args.ClaID);

            if(args.CanID > 0)
            {
                map.Add("CanID", args.CanID);
            }
            map.Add("CliTipoLocal", args.CliTipoLocal);
            map.Add("CliTipoCliente", args.CliTipoCliente);
            map.Add("CliTipoComprobanteFAC", args.CliTipoComprobanteFAC);

            map.Add("SACSecuencia", sacSecuencia);
            map.Add("rowguid", Guid.NewGuid().ToString());
            map.Add("mbVersion", Functions.AppVersion);
            map.Add("SACFecha", Functions.CurrentDate());
            map.Add("SACFechaActualizacion", Functions.CurrentDate());
            map.Add("SolFechaActualizacion", Functions.CurrentDate());
            map.Add("VisSecuencia", Arguments.Values.CurrentVisSecuencia);
            map.Add("SacEstado", 1);
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            map.Add("CliID", Arguments.Values.CurrentClient.CliID);
            map.Add("CliRegMercantil", args.CliRegMercantil);
            map.Add("CliRutPosicion", args.CliRutPosicion);
            map.Add("CliFrecuenciaVisita", args.CliFrecuenciaVisita);
            map.Add("CliRutSemana1", args.CliRutSemana1);
            map.Add("CliRutSemana2", args.CliRutSemana2);
            map.Add("CliRutSemana3", args.CliRutSemana3);
            map.Add("CliRutSemana4", args.CliRutSemana4);

            if (args.TinID > 0)
            {
                map.Add("TinID", args.TinID);
            }

            map.ExecuteInsert();

            if (DS_RepresentantesParametros.GetInstance().GetParVisitasResultados())
            {
                ActualizarVisitasResultados(changesNames);
            }
            
            DS_RepresentantesSecuencias.UpdateSecuencia("SolicitudActualizacionClientes", sacSecuencia);

            if(myParametro.GetParNoObligarAuditorHaacerComentarios())
            {
                new DS_Visitas().ActualizarVisitaEfectiva(Arguments.Values.CurrentVisSecuencia);
            }

            return sacSecuencia;
        }

        private void ActualizarVisitasResultados(List<string> changes)
        {
            var list = SqliteManager.GetInstance().Query<VisitasResultados>("select 15 as TitID, count(*) as VisCantidadTransacciones, '' as VisComentario " +
                "from SolicitudActualizacionClientes where RepCodigo = '"+Arguments.CurrentUser.RepCodigo.Trim()+"' and VisSecuencia = ?", 
                new string[] { Arguments.Values.CurrentVisSecuencia.ToString() });

            var myVis = new DS_Visitas();

            if(list != null && list.Count > 0)
            {
                var item = list[0];

                myVis.GuardarVisitasResultados(item);

                foreach(var change in changes)
                {
                    item.VisComentario = change;
                    myVis.GuardarVisitasResultados(item);
                }
            }
        }

        public SolicitudActualizacionClientes GetSACById(int sacSecuencia)
        {
            var list = new ObservableCollection<SolicitudActualizacionClientes>(SqliteManager.GetInstance().Query<SolicitudActualizacionClientes>("select CliID, SACSecuencia, RepCodigo, CliNombre, CliRNC, CliPropietario,CliCedulaPropietario, " +
                "CliPaginaWeb,CliCorreoElectronico,CliContacto, ProID, MunID, cliSector, CliUrbanizacion, CliCalle, CliCasa, CliContactoFechaNacimiento, CliTelefono, CliFax, CliEncargadoPago, TiNID, " +
                "CliTipoLocal, CliTipoCliente, CliTipoComprobanteFAC, CliLatitud, CliLongitud from SolicitudActualizacionClientes c " +
               "where SACSecuencia = ? " +
               "order by CliNombre", new string[] { sacSecuencia.ToString() }));

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public Clientes GetClienteById(int cliID)
        {
            var list = new ObservableCollection<Clientes>(SqliteManager.GetInstance().Query<Clientes>("select CliID,CliCodigo, RepCodigo, CliNombre, CliRNC, CliPropietario, CliCedulaPropietario, " +
                "CliPaginaWeb,CliCorreoElectronico,CliContacto, ProID, MunID, cliSector, CliUrbanizacion, CliCalle, CliCasa, CliContactoFechaNacimiento, CliTelefono, CliFax, CliEncargadoPago, TiNID, " +
                "CliTipoLocal, CliTipoCliente, CliTipoComprobanteFAC, CliLatitud, CliLongitud from clientes c " +
               "where CliID = ? " +
               "order by CliNombre", new string[] { cliID.ToString() }));

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }
    }
}
