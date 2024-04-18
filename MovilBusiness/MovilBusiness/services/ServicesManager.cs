

using Microsoft.AppCenter.Crashes;
using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.model.Internal.structs;
using MovilBusiness.model.Internal.Structs.Args.services;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Model.Internal.Structs.services;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using Plugin.Connectivity;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MovilBusiness.Services
{
    public class ServicesManager
    {
        private ApiManager api;
        private DS_TransaccionesImagenes myTranImg;
        private DS_RepresentantesParametros myParametro;
        public event EventHandler<string> MessageReport;
        public double CantidadTotalCambios { get; set; } = 0;
        private PreferenceManager pref;
        public Action<double> OnTotalProgressChanged { get; set; }
        public Action<string> OnMensajesincronizar { get; set; }
        public Action<double, int> OnCurrentProgressChanged { get; set; }
        private INotificationManager notifications;
        private VersionReplicacion versionReplicacion;

        private int CantidadCambiosCiclo = 500;

        private bool ErrorOcurred = false;

        public ServicesManager(Action<string> onMensajesincronizar = null)
        {
            OnMensajesincronizar = onMensajesincronizar;

            pref = new PreferenceManager();
            api = ApiManager.GetInstance(pref.GetConnection().Url);
            myParametro = DS_RepresentantesParametros.GetInstance();
            myTranImg = new DS_TransaccionesImagenes();            

            versionReplicacion = pref.GetVersionReplicacion();

            if(versionReplicacion == VersionReplicacion.V9)
            {
                pref.SaveCantidadDatosCiclo("20000");
                CantidadCambiosCiclo = 20000;
            }else
            CantidadCambiosCiclo = pref.GetCantidadDatosCiclo();

        }

        public Task CargaInicial(CargaInicialArgs args)
        {
            try
            {
                return Task.Run(async () =>
                {
                try
                {
                    OnTotalProgressChanged?.Invoke(0);

                    ErrorOcurred = false;

                    var db = SqliteManager.GetInstance();

                    db.DeleteDatabase();

                    db = SqliteManager.GetInstance();

                    MessageReport?.Invoke(this, AppResource.CreatingDbTables);

                    OnTotalProgressChanged?.Invoke(0.1);
                    //totalProgress?.Invoke(2);
                    // individualProgress?.Invoke(0);

                        var response = await api.ReplicacionesTablasLeer(args.repcodigo, args.repclave, args.repsuscriptor);

                        if (!CrearTablas(response))
                        {
                            throw new Exception(AppResource.ErrorLoadingDbTables);
                        }

                        OnTotalProgressChanged?.Invoke(0.3);

                        MessageReport?.Invoke(this, AppResource.RequestingDataFromServer);

                        OnTotalProgressChanged?.Invoke(0.35);

                        Device.BeginInvokeOnMainThread(() => { SaveImagesFromServer(args.repcodigo, args.repclave, args.repsuscriptor); });

                        OnTotalProgressChanged?.Invoke(0.60);

                        MessageReport?.Invoke(this, AppResource.RequestingDataFromServer);

                        OnTotalProgressChanged?.Invoke(0.75);

                        await ObtenerCambiosDesdeServidorCargaInicial(args.repcodigo, args.repclave, args.repsuscriptor);

                        MessageReport?.Invoke(this, AppResource.LoadingDataToCompanyTable);

                        GuardarEmpresa(await api.EmpresasCargar(args.repcodigo, args.repclave));

                        OnTotalProgressChanged?.Invoke(0.85);

                        MessageReport?.Invoke(this, AppResource.CreatingDbIndexes);
                        db.GenerateDatabaseIndexs();

                        db.GenerateDatabaseIndexsRowGuid(response);

                        OnTotalProgressChanged?.Invoke(0.9);


                        MessageReport?.Invoke(this, AppResource.UpdatingSubscriberOnServer);
                        await ActualizarSuscriptorEnElServidor(args.repsuscriptor, args.repcodigo, args.repclave);

                        if (!Functions.IsSincronizacionTest(pref))
                        {
                            ActualizarVersionEnElServidor(args.repcodigo, args.repclave);
                        }

                        Arguments.CurrentUser = DS_Representantes.LogIn(args.repcodigo, args.repclave, myParametro.GetParPermitLoginForAud());

                        if (Arguments.CurrentUser == null)
                        {
                            throw new Exception(AppResource.ErrorLoadingRepresentativeData);
                        }

                        //if (DS_Representantes.RepresentateIsInactive(Arguments.CurrentUser.RepCodigo))
                        //{
                        //    throw new Exception(AppResource.UserIsInactive);
                        //}

                        CambiarContraseñaUsuarios(args.repcodigo);
                        
                        myParametro.SaveSupervisorIndicador(Arguments.CurrentUser.RepCodigo);

                        OnTotalProgressChanged?.Invoke(1);

                        GuardarSuscriptor(args.repcodigo, args.repsuscriptor, !Functions.IsSincronizacionTest(pref));                        

                        MessageReport?.Invoke(this, AppResource.InitialLoadCompletedSuccessfully);
                    }
                    catch (Exception e)
                    {
                        ErrorOcurred = true;
                        throw e;
                    }
                });
            
            }catch(Exception ex)
            {
                //Crashes.TrackError(ex);
                ErrorOcurred = true;
                throw ex;

            }
        }

        public async Task ActualizarSuscriptorEnElServidor(string suscriptor, string repcodigo, string repClave)
        {
            try
            {
                await api.ExecuteQuery(new List<ExecuteQueryValues>() { new ExecuteQueryValues() { Query = "update ReplicacionesSuscriptores set ResEstado = 2 where RepSuscriptor = '" + suscriptor + "'", rowguid = null, TableName = "ReplicacionesSuscriptores", TipoScript = "U" } }, repcodigo, repClave, suscriptor);

                var list = await api.RawQuery<ReplicacionesSuscriptores>(repcodigo, repClave, "select RepSuscriptor, RsuTipo, resEstado, RepID, rowguid from ReplicacionesSuscriptores where ltrim(rtrim(upper(RepSuscriptor))) = '" + suscriptor.Trim().ToUpper() + "'");
                SqliteManager db3 = SqliteManager.GetInstance();
                db3.BeginTransaction();
                if (list != null && list.Count > 0)
                {
                    try
                    {
                        db3.InsertOrReplace(list[0]);
                    }
                    finally
                    {
                        db3.Commit();
                    }

                }


            }
            catch (Exception e)
            {
                //Crashes.TrackError(e);
                Console.Write(e.Message);
            }

        }

        public async Task<CargaInicialArgs> VerificarSuscriptor(string repCodigo, string repClave, string key)
        {
            try
            {
                string encriptePass = Functions.StringToMd5(repClave);

                MessageReport?.Invoke(this, AppResource.VerifingSubscriberOnServer);

                var args = new UsuarioArgs() { RepCodigo = repCodigo, RepClave = Functions.StringToMd5(repClave), Suscriptor = key, RepCurrent = "" };

                var suscriptorResponse = await api.VerificarSuscriptor(args, versionReplicacion);

                return new CargaInicialArgs() { repcodigo = repCodigo, repclave = repClave, repsuscriptor = suscriptorResponse.RepSuscriptor, cantidadCambios = suscriptorResponse.CantidadCambios };

            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                throw new Exception(e.InnerException != null && !string.IsNullOrWhiteSpace(e.InnerException.Message) ? e.InnerException.Message : e.Message);
            }
        }

        public async Task<bool> Sincronizar(Action<int> stepIndicator)
        {
            string repSuscriptor = GetSuscriptorFromReplicacionesSuscriptores();
            int rssSecuencia = GuardarDatosSincronzacion(repSuscriptor);

            try
            {
                // string repSuscriptor = DS_RepresentantesParametros.GetInstance().GetRepSuscriptor();

                if (Functions.IsSincronizacionTest(pref))
                {
                    await Functions.DisplayAlert(AppResource.Warning, AppResource.TrialSubscriberOnlyReceiveData);
                }

                bool isvalidtosinc = true;

                await Task.Run(async () =>
                    {
                        ErrorOcurred = false;                        
                        if (repSuscriptor == null)
                        {
                            throw new Exception(AppResource.ErrorLoadingSubscriber);
                        }

                        //if (Arguments.CurrentUser != null)
                        //{
                        //    if (DS_Representantes.RepresentateIsInactive(Arguments.CurrentUser.RepCodigo))
                        //    {
                        //        throw new Exception(AppResource.UserIsInactive);
                        //    }
                        //}

                        stepIndicator?.Invoke(1);

                        stepIndicator?.Invoke(2);

                        if (!Functions.IsSincronizacionTest(pref))
                        {                            
                            if(!await EnviarCambiosAlServidor(repSuscriptor))
                            {
                                isvalidtosinc = false;
                                return;
                            }
                            await SendImagenes(repSuscriptor);
                        }

                        MessageReport?.Invoke(this, "");
                        stepIndicator?.Invoke(3);

                        if (Functions.IsSincronizacionTest(pref))
                        {
                            var IsLocked = await api.RawQueryForsure(Arguments.CurrentUser.RepCodigo, Arguments.CurrentUser.RepClave, "select[dbo].[fnisTableExclusiveBlock]('ReplicacionesSuscriptoresCambios" + repSuscriptor.ToUpper() + "')");
                            if (IsLocked)
                            {
                                string mensajesincrocisacion = new DS_Mensajes().GetMensajeForSincronizacion(104);
                                string Mensaje = string.IsNullOrEmpty(mensajesincrocisacion) ? AppResource.WaitAnswerMessage : mensajesincrocisacion;
                                Device.BeginInvokeOnMainThread(() => { OnMensajesincronizar.Invoke(Mensaje); });
                                isvalidtosinc = false;
                                return;
                            }
                        }

                        await ObtenerCambiosDesdeServidor(Arguments.CurrentUser.RepCodigo, Arguments.CurrentUser.RepClave,repSuscriptor, true);

                        MessageReport?.Invoke(this, "");
                        stepIndicator?.Invoke(4);

                        await ActualizarFechaUltimaSincronizacion(repSuscriptor, Arguments.CurrentUser.RepCodigo, Arguments.CurrentUser.RepClave);

                        ActualizarDatosSincronizacion(rssSecuencia, repSuscriptor, false);

                        if (!Functions.IsSincronizacionTest(pref))
                        {
                            ActualizarVersionEnElServidor(Arguments.CurrentUser.RepCodigo, Arguments.CurrentUser.RepClave);
                        }

                        await Task.Run(() =>
                        {
                            Arguments.CurrentUser = DS_Representantes.LogIn(Arguments.CurrentUser.RepCodigo, Arguments.CurrentUser.RepClave, myParametro.GetParPermitLoginForAud());
                        });

                        stepIndicator?.Invoke(5);

                        MessageReport?.Invoke(this, AppResource.SynchronizationFinished);                       

                        Task.Delay(500).Wait();
                    });

                if(!isvalidtosinc)
                {
                    return false;
                }

                SaveImagesFromServer(Arguments.CurrentUser.RepCodigo, Arguments.CurrentUser.RepClave, repSuscriptor);
                return true;
            }
            catch(Exception e)
            {
                Crashes.TrackError(e);
                ErrorOcurred = true;
                ActualizarDatosSincronizacion(rssSecuencia, repSuscriptor, true, e.Message);
                throw e;
            }
        }

        private void ActualizarDatosSincronizacion(int secuencia, string suscriptor, bool syncFallida = false, string messages = "")
        {
            try
            {
                var map = new Hash("ReplicacionesSuscriptoresSincronizaciones");
                //map.SaveScriptForServer = false;
                map.Add("RssFechaFin", Functions.CurrentDate());
                map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                map.Add("RssEstatus", syncFallida ? 0 : 1);
                map.Add("rssMensaje", messages.Length > 500 ? messages.Substring(0,499) : messages);

                var list = SqliteManager.GetInstance().Query<ReplicacionesSuscriptores>("select RepID from ReplicacionesSuscriptores where ltrim(rtrim(upper(RepSuscriptor))) = ?", new string[] { suscriptor.Trim().ToUpper() });

                var repId = 1;

                if (list != null && list.Count > 0)
                {
                    repId = list[0].RepID;
                }

                OnCurrentProgressChanged?.Invoke(0.75, 0);

                var rss = SqliteManager.GetInstance().Query<ReplicacionesSuscriptores>("select rowguid from ReplicacionesSuscriptoresSincronizaciones " +
                    "where RssSecuencia = ? and trim(upper(RepSuscriptor)) = ? and RepID = ?", 
                    new string[] { secuencia.ToString(), suscriptor.Trim().ToUpper(), repId.ToString() });

                //var query = map.ExecuteUpdate("RssSecuencia = " + secuencia + " and ltrim(rtrim(Upper(RepSuscriptor))) = '" + suscriptor.Trim().ToUpper() + "' and RepID = " + repId);

                // var query = map.ExecuteUpdate("rowguid = '" + rss[0].rowguid.Trim() + "'", true);

                var query = map.ExecuteUpdate(new string[] { "rowguid" }, new Model.Internal.DbUpdateValue[] { new Model.Internal.DbUpdateValue() { Value = rss[0].rowguid.Trim(), IsText = true } }, true);

                //api.ExecuteQuery(new List<ExecuteQueryValues>() { new ExecuteQueryValues() { Query = query, rowguid = Guid.NewGuid().ToString(), rowguidtemp = Guid.NewGuid().ToString(), TableName = "ReplicacionesSuscriptoresSincronizaciones", TipoScript = "U" } }, Arguments.CurrentUser.RepCodigo, Arguments.CurrentUser.RepClave, suscriptor);

                OnCurrentProgressChanged?.Invoke(1, 0);

            }
            catch(Exception e)
            {
                Crashes.TrackError(e);
                Console.Write(e.Message);
            }
        }

        private int GuardarDatosSincronzacion(string repSuscriptor)
        {
            try
            {
                OnCurrentProgressChanged?.Invoke(0.15, 0);

                var secuencia = DS_RepresentantesSecuencias.GetLastSecuencia("ReplicacionesSuscriptoresSincronizaciones");

                var list = SqliteManager.GetInstance().Query<ReplicacionesSuscriptores>("select RepID from ReplicacionesSuscriptores where ltrim(rtrim(upper(RepSuscriptor))) = ?", new string[] { repSuscriptor.Trim().ToUpper() });

                OnCurrentProgressChanged?.Invoke(0.30, 0);

                var repId = 1;

                if (list != null && list.Count > 0)
                {
                    repId = list[0].RepID;
                }

                var cambios = SqliteManager.GetInstance().Query<Totales>("select count(*) as Total from SuscriptorCambios", new string[] { });

                OnCurrentProgressChanged?.Invoke(0.55, 0);

                var cantidad = 0;

                if (cambios != null && cambios.Count > 0)
                {
                    cantidad = (int)cambios[0].Total;
                }

                OnCurrentProgressChanged?.Invoke(0.68, 0);

                Hash map = new Hash("ReplicacionesSuscriptoresSincronizaciones");
                //map.SaveScriptForServer = false;
                map.Add("RepID", repId);
                map.Add("RepSuscriptor", repSuscriptor);
                map.Add("RssSecuencia", secuencia);
                map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                map.Add("RssLatitud", Arguments.Values.CurrentLocation == null ? 0 : Arguments.Values.CurrentLocation.Latitude);
                map.Add("RssLongitud", Arguments.Values.CurrentLocation == null ? 0 : Arguments.Values.CurrentLocation.Longitude);
                map.Add("RssEstatus", 1);
                map.Add("RssFechaInicio", Functions.CurrentDate());
                map.Add("RssCantidadRegistros", cantidad);
                var rowguid = Guid.NewGuid().ToString();
                map.Add("rowguid", rowguid);
                map.Add("RepFechaActualizacion", Functions.CurrentDate());
                var query = map.ExecuteInsert();

                OnCurrentProgressChanged?.Invoke(0.81, 0);

                //await api.ExecuteQuery(new List<ExecuteQueryValues>() { new ExecuteQueryValues() { Query = query, rowguid = rowguid, rowguidtemp = Guid.NewGuid().ToString(), TableName = "ReplicacionesSuscriptoresSincronizaciones", TipoScript = "I" } }, Arguments.CurrentUser.RepCodigo, Arguments.CurrentUser.RepClave, repSuscriptor);

                OnCurrentProgressChanged?.Invoke(0.93, 0);

                DS_RepresentantesSecuencias.UpdateSecuencia("ReplicacionesSuscriptoresSincronizaciones", secuencia);

                OnCurrentProgressChanged?.Invoke(1, 0);

                return secuencia;

            }catch(Exception e)
            {
                Crashes.TrackError(e);
                Console.Write(e.Message);
                return 0;
            }

        }

        private async Task ActualizarFechaUltimaSincronizacion(string suscriptor, string repcodigo, string repclave)
        {
            OnCurrentProgressChanged?.Invoke(0, 0);

            await api.ExecuteQuery(new List<ExecuteQueryValues>() { new ExecuteQueryValues() { Query = "Update ReplicacionesSuscriptores set RepFechaUltimaSincronizacion = getdate() where ltrim(rtrim(upper(RepSuscriptor))) = '" + suscriptor.Trim().ToUpper() + "'", rowguid = null, rowguidtemp = Guid.NewGuid().ToString(), TableName = "ReplicacionesSuscriptores", TipoScript = "U" } }, repcodigo, repclave, suscriptor);
            SqliteManager.GetInstance().Execute($"Update ReplicacionesSuscriptores set RepFechaUltimaSincronizacion = '{Functions.CurrentDate()}'");
            OnCurrentProgressChanged?.Invoke(0.5, 0);
        }

        private void ActualizarVersionEnElServidor(string repCodigo, string repClave)
        {
            try
            {
                var raw = myParametro.GetVersion();

                int.TryParse(!string.IsNullOrWhiteSpace(raw) ? raw.Replace(".", "") : raw, out int version);
                int.TryParse(!string.IsNullOrWhiteSpace(Functions.AppVersion) ? Functions.AppVersion.Replace(".", "") : Functions.AppVersion, out int appVersion);

                if (version != appVersion)
                {
                    //api.RawQuery<Totales>(repCodigo, repClave, "exec [sp_MDSOFT_ActualizarVersionMovil] @RepCodigo = '"+repCodigo+"', @Version = '"+ Functions.AppVersion + "'");
                    myParametro.SaveVersion(repCodigo, Functions.AppVersion);
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        private void GuardarSuscriptor(string repcodigo, string repsuscriptor, bool sendToServer)
        {

            var map = new Hash("RepresentantesParametros") { SaveScriptForServer = sendToServer };
            map.Add("RepCodigo", repcodigo);
            map.Add("ParReferencia", "SUSCRIPTOR");
            map.Add("ParDescripcion", "RepSuscriptor del representante");
            map.Add("ParValor", repsuscriptor);
            map.Add("UsuInicioSesion", repcodigo);
            map.Add("RepFechaActualizacion", Functions.CurrentDate());
            map.Add("rowguid", Guid.NewGuid().ToString());

            var oldSuscriptor = myParametro.GetRepSuscriptor();

            if (string.IsNullOrWhiteSpace(oldSuscriptor))
            {
                map.ExecuteInsert();
            }
            else
            {               
                map.ExecuteUpdate("ParReferencia = 'SUSCRIPTOR' and RepCodigo = '" + repcodigo.Trim() + "'");
            }
        }

        private void GuardarEmpresa(List<Empresa> list)
        {
            foreach(var emp in list)
            {
                emp.rowguid = Guid.NewGuid().ToString();
                SqliteManager.GetInstance().InsertOrReplace(emp);
            }
        }

        private bool CrearTablas(List<ReplicacionesTablas> tablas)
        {
  
            if (tablas.Count > 0)
            {
                var db = SqliteManager.GetInstance();

                var mdc = new SQLiteCommand(db);
                db.BeginTransaction();
                var table = tablas.Where(x => x.RepTabla.Trim().ToUpper() != "ReplicacionesSuscriptores".Trim().ToUpper()).Select(p => p.RepScriptCreacion).ToList();
                for (var i = 0; i < table.Count; i++)
                {
                    mdc.CommandText = table[i];
                    mdc.ExecuteNonQuery();
                }
                db.Commit();

                return true;
            }
            return false;
        }

        private async Task ObtenerCambiosDesdeServidor(string repcodigo, string repclave, string repsuscriptor, bool fromSync = false)
        {
            try
            {
               var cantidadCambios = await api.RawQuery<Totales>(repcodigo, repclave, "select count(*) as CantidadTotal from [ReplicacionesSuscriptoresCambios" + (versionReplicacion == VersionReplicacion.V7 || versionReplicacion == VersionReplicacion.V8 || versionReplicacion == VersionReplicacion.V9 ? repsuscriptor.ToUpper() : "") + "] where RepSuscriptor = '" + repsuscriptor.ToUpper() + "'");

               if (cantidadCambios != null && cantidadCambios.Count > 0)
                  {
                       CantidadTotalCambios = cantidadCambios[0].CantidadTotal;
                    }

                MessageReport?.Invoke(this, AppResource.RequestingDataFromServer);

                var result = await api.SuscriptoresCambiosLeer(repcodigo, repclave, repsuscriptor, "", versionReplicacion, CantidadCambiosCiclo, true);
                //var result = await api.SuscriptoresCambiosLeer(repcodigo, repclave, repsuscriptor, "", versionReplicacion, CantidadCambiosCiclo);
                while (result.Count > 0)
                {
                    //EliminarCambiosConfirmados(repcodigo, repclave, SaveSuscriptoresCambios(result), repsuscriptor); 
                    MessageReport?.Invoke(this, AppResource.RequestingDataFromServer);
                    OnCurrentProgressChanged?.Invoke(0, 0);

                    var deleteQuery = SaveSuscriptoresCambios(result,repsuscriptor, fromSync);

                    MessageReport?.Invoke(this, AppResource.RequestingDataFromServer);

                    //result = await api.SuscriptoresCambiosLeer(repcodigo, repclave, repsuscriptor, deleteQuery, versionReplicacion, CantidadCambiosCiclo);
                    result = await api.SuscriptoresCambiosLeer(repcodigo, repclave, repsuscriptor, deleteQuery, versionReplicacion, CantidadCambiosCiclo, true);
                }

            }
            catch (Exception e)
            {
                throw e;
            }

            OnCurrentProgressChanged?.Invoke(1, 0);
        }

        private async Task ObtenerCambiosDesdeServidorCargaInicial(string repcodigo, string repclave, string repsuscriptor, bool fromSync = false)
        {
            try
            {
                // try
                // {
                var cantidadCambios = await api.RawQuery<Totales>(repcodigo, repclave, "select count(*) as CantidadTotal from [ReplicacionesSuscriptoresCambios" + (versionReplicacion == VersionReplicacion.V7 || versionReplicacion == VersionReplicacion.V8  || versionReplicacion == VersionReplicacion.V9 ? repsuscriptor.ToUpper() : "") + "] where RepSuscriptor = '" + repsuscriptor.ToUpper() + "'");

                if (cantidadCambios != null && cantidadCambios.Count > 0)
                {
                    CantidadTotalCambios = cantidadCambios[0].CantidadTotal;
                }

                //}
                //catch (Exception e) { throw new Exception(e.Message); }

                MessageReport?.Invoke(this, AppResource.RequestingDataFromServer);

                var result = await api.SuscriptoresCambiosLeer(repcodigo, repclave, repsuscriptor, "", versionReplicacion, CantidadCambiosCiclo);

                while (result.Count > 0)
                {
                    //EliminarCambiosConfirmados(repcodigo, repclave, SaveSuscriptoresCambios(result), repsuscriptor); 
                    MessageReport?.Invoke(this, AppResource.RequestingDataFromServer);
                    OnCurrentProgressChanged?.Invoke(0, 0);

                    var deleteQuery = "";
                    if (versionReplicacion == VersionReplicacion.V9)
                        deleteQuery = SaveSuscriptoresCambiosCargaInicial(result, repsuscriptor, fromSync, result.Count);
                    else
                        deleteQuery = SaveSuscriptoresCambios(result, repsuscriptor, fromSync);
                    MessageReport?.Invoke(this, "Solicitando datos al servidor");

                    result = await api.SuscriptoresCambiosLeer(repcodigo, repclave, repsuscriptor, deleteQuery, versionReplicacion, CantidadCambiosCiclo);
                }

            }
            catch (Exception e)
            {
                throw e;
            }

            OnCurrentProgressChanged?.Invoke(1, 0);
        }

        private async Task<bool> EnviarCambiosAlServidor(string suscriptor)
        {
            OnCurrentProgressChanged?.Invoke(0, 0);

            DS_SuscriptoresCambios myCam = new DS_SuscriptoresCambios();

            List<ExecuteQueryValues> cambiosPendientes = myCam.GetPending30();

            double cambiosConsumidos = 1;

            var cantidadTotalCambios = myCam.GetAll().Count();

            if(cantidadTotalCambios > 0)
            {
                MessageReport?.Invoke(this, AppResource.SendingTransactionData);
            }

            while (cambiosPendientes.Count > 0)
            {
                if (cantidadTotalCambios > 0)
                {
                    OnCurrentProgressChanged?.Invoke((cambiosConsumidos / cantidadTotalCambios), 0);
                    cambiosConsumidos++;
                }

                /*await Task.Run(async () =>
                {
                    var IsLocked = await api.RawQueryForsure(Arguments.CurrentUser.RepCodigo, Arguments.CurrentUser.RepClave, "select[dbo].[fnisTableExclusiveBlock]('ReplicacionesSuscriptoresCambios" + suscriptor.ToUpper() + "')");
                    if (IsLocked)
                    {
                        throw new Exception("Su solicitud no puede ser atendida en este instante. Intente sincronizar en unos momentos.");
                    }
                });*/
                var IsLocked = await api.RawQueryForsure(Arguments.CurrentUser.RepCodigo, Arguments.CurrentUser.RepClave, "select[dbo].[fnisTableExclusiveBlock]('ReplicacionesSuscriptoresCambios" + suscriptor.ToUpper() + "')");
                if (IsLocked)
                {
                   string mensajesincrocisacion = new DS_Mensajes().GetMensajeForSincronizacion(104);
                   string Mensaje = string.IsNullOrEmpty(mensajesincrocisacion) ? AppResource.WaitAnswerMessage : mensajesincrocisacion;

                    Device.BeginInvokeOnMainThread(() => { OnMensajesincronizar.Invoke(Mensaje); });
                    return false;
                }

                await api.ExecuteQuery(cambiosPendientes, Arguments.CurrentUser.RepCodigo, Arguments.CurrentUser.RepClave, suscriptor);//Arguments.CurrentUser.RepCodigo, Functions.StringToMd5(Arguments.CurrentUser.RepClave), suscriptor, JsonConvert.SerializeObject(cambiosPendientes));

                myCam.Delete(cambiosPendientes);

                cambiosPendientes = myCam.GetPending30();
            }

            OnCurrentProgressChanged?.Invoke(1, 0);

            return true;
        }

        private double cambiosConsumidos = 1;
        int _Count = 0;
        private string SaveSuscriptoresCambiosCargaInicial(List<ReplicacionesSuscriptoresCambios> list, string repSuscriptor, bool fromSync = false, int Count = 0)
        {
            string keysToDelete = "";

            SqliteManager db = SqliteManager.GetInstance();

            try
            {
                _Count += Count;

                if (!fromSync)
                {
                    cambiosConsumidos = 1;
                }

                keysToDelete = $"delete top(20000) from [ReplicacionesSuscriptoresCambios{ repSuscriptor }] WHERE RscTipTran <> 'F' And RscConTranID <= {_Count}";

                var mdc = new SQLiteCommand(db);
                db.BeginTransaction();

                //foreach (var cambio in list)
                //{
                //    /*if (CantidadTotalCambios > 0)
                //    {
                //        OnCurrentProgressChanged?.Invoke((cambiosConsumidos / (!fromSync ? list.Count : CantidadTotalCambios)), 1);
                //        cambiosConsumidos++;
                //    }*/
                //    mdc.CommandText = cambio.RscScript;
                //    mdc.ExecuteNonQuery();
                //}

                //var tasks = new List<Task>();

                /*foreach (var item in list)
                {
                    tasks.Add(new Task(() =>
                    {
                        if (CantidadTotalCambios > 0)
                        {
                            OnCurrentProgressChanged?.Invoke((cambiosConsumidos / (!fromSync ? list.Count : CantidadTotalCambios)), 1);
                            cambiosConsumidos++;
                        }

                        mdc.CommandText = item.RscScript;
                        mdc.ExecuteNonQuery();
                    }));
                }*/

                //await Task.WhenAll(tasks);
                //Parallel.ForEach(list, item =>
                //{
                //    mdc.CommandText = item.RscScript;
                //    mdc.ExecuteNonQuery();
                //});
                DS_SuscriptorCambiosFallidos SuscriptorCambiosFallidos = new DS_SuscriptorCambiosFallidos();

                foreach (var item in list)
                {
                    mdc.CommandText = item.RscScript;

                    SuscriptorCambiosFallidos.SaveSuscriptorCambiosFallidos(item.RscScript);

                    mdc.ExecuteNonQuery();
                }

                db.Commit();
                OnCurrentProgressChanged?.Invoke(1, 0);
                return keysToDelete;

            }catch(Exception e)
            {
                Crashes.TrackError(e);
                db.Rollback();
                throw e;
            }
        }

        private string SaveSuscriptoresCambios(List<ReplicacionesSuscriptoresCambios> list, string repSuscriptor, bool fromSync = false)
        {
            string keysToDelete = "";

            SqliteManager db = SqliteManager.GetInstance();

            db.BeginTransaction();

            var sql = "";

            try
            {
                OnCurrentProgressChanged?.Invoke(0, 0);

                if (!fromSync)
                {
                    cambiosConsumidos = 1;
                }

                foreach (ReplicacionesSuscriptoresCambios cambio in list)
                {
                    keysToDelete += versionReplicacion == VersionReplicacion.V7 || versionReplicacion == VersionReplicacion.V8 || versionReplicacion == VersionReplicacion.V9 ?
                                      $"delete from [ReplicacionesSuscriptoresCambios{ repSuscriptor }] " +
                                      $"where RscKey = '{ cambio.RscKey }';\r\n" : $"delete from " +
                                      $"[ReplicacionesSuscriptoresCambios] where RscKey = '{ cambio.RscKey }'" +
                                      $" and RepSuscriptor = '{ repSuscriptor }';\r\n";


                    //MessageReport?.Invoke(this, "Cargando datos a la tabla: " + cambio.RscTabla);

                    if (CantidadTotalCambios > 0)
                    {
                        OnCurrentProgressChanged?.Invoke((cambiosConsumidos / (!fromSync ? list.Count : CantidadTotalCambios)), 1);
                        cambiosConsumidos++;
                    }

                    sql = cambio.RscScript;

                    if (versionReplicacion == VersionReplicacion.V7 || versionReplicacion == VersionReplicacion.Standard)
                    {

                        if (cambio.RscTipTran.Trim().ToUpper().StartsWith("I") || cambio.RscTipTran.Trim().ToUpper().StartsWith("C"))
                        {
                            sql = cambio.RscScript;
                        }
                        else if (cambio.RscTipTran.Trim().ToUpper().StartsWith("U"))
                        {
                            sql = cambio.RscScript + " WHERE UPPER(TRIM(rowguid)) = UPPER('" + cambio.RscTablarowguid.Trim().ToUpper() + "')";
                        }
                        else if (cambio.RscTipTran.Trim().ToUpper().StartsWith("D"))
                        {
                            sql = "DELETE FROM " + cambio.RscTabla + " WHERE UPPER(TRIM(rowguid)) = '" + cambio.RscTablarowguid.Trim().ToUpper() + "'";
                        }

                    }                    

                    if (sql.Contains("Noticias") && sql.Contains("INSERT"))
                    {                     
                       NoticiasCreate(sql, db);
                    }

                    if (!string.IsNullOrWhiteSpace(sql))
                    {
                        //var result = db.Execute(sql, new string[] { });
                        var cmd = db.CreateCommand(sql, new object[] { });
                        int id = cmd.ExecuteNonQuery();
                    }
                }

                db.Commit();

                return keysToDelete;

            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                db.Rollback();
                throw e;
            }
        }

        private async Task EliminarCambiosConfirmados(string repcodigo, string repclave, string query, string suscriptor)
        {         
            if (!string.IsNullOrEmpty(query))
            {
                await api.ExecUpdate(query, repcodigo, repclave);
            }
        }

        private async Task SendImagenes(string repSuscriptor)
        {
            OnCurrentProgressChanged?.Invoke(0, 0);

            var list = myTranImg.GetImagesReadyToSend();

            if(list == null || list.Count == 0)
            {
                OnCurrentProgressChanged?.Invoke(1, 0);
                return;
            }

            MessageReport?.Invoke(this, AppResource.SendingImages);

            OnCurrentProgressChanged?.Invoke(0.2, 0);

            await api.TransaccionesImagenesInsertar(list, repSuscriptor);

            OnCurrentProgressChanged?.Invoke(0.9, 0);

            foreach (TransaccionesImagenesTemp img in list)
            {
                myTranImg.DeleteById(img.Rowguid);
            }

            OnCurrentProgressChanged?.Invoke(1, 0);
        }

        private async void SaveImagesFromServer(string repcodigo, string repclave, string repsuscriptor)
        {
            if (Arguments.IsDownloadingImages || ErrorOcurred)
            {
                return;
            }

            if(notifications == null)
            {
                notifications = DependencyService.Get<INotificationManager>();
            }

            Arguments.IsDownloadingImages = true;

            try
            {               
                var result = await api.SuscriptoresCambiosImagenesLeer(repcodigo, repclave, repsuscriptor);

                while (result.Count > 0)
                {
                    if (ErrorOcurred)
                    {
                        throw new Exception("Fallo");
                    }
                    await EliminarCambiosConfirmados(repcodigo, repclave, await SaveSuscriptoresCambiosImagenes(result, repcodigo, repclave), repsuscriptor);
                    result = await api.SuscriptoresCambiosImagenesLeer(repcodigo, repclave, repsuscriptor);
                }

                Arguments.IsDownloadingImages = false;
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                Arguments.IsDownloadingImages = false;                
            }
        }

        private async Task<string> SaveSuscriptoresCambiosImagenes(List<ReplicacionesSuscriptoresCambios> imagenes, string repcodigo, string repclave)
        {
            IAppInfo info = DependencyService.Get<IAppInfo>();

            string keysToDelete = "";

            foreach(var cambio in imagenes)
            {
                if (ErrorOcurred)
                {
                    throw new Exception(AppResource.Error);
                }
                //keysToDelete += "delete from [ReplicacionesSuscriptoresCambios" + cambio.RepSuscriptor + "] where RscKey = '" + cambio.RscKey + "';\r\n";
               // keysToDelete += "delete from [ReplicacionesSuscriptoresCambios] where RscKey = '" + cambio.RscKey + "' and RepSuscriptor = '" + cambio.RepSuscriptor + "';\r\n";

                keysToDelete += versionReplicacion == VersionReplicacion.V7 || versionReplicacion == VersionReplicacion.V8  || versionReplicacion == VersionReplicacion.V9 ? $"delete from [ReplicacionesSuscriptoresCambios{ cambio.RepSuscriptor }] where RscKey = '{ cambio.RscKey }';\r\n" : $"delete from [ReplicacionesSuscriptoresCambios] where RscKey = '{ cambio.RscKey }' and RepSuscriptor = '{ cambio.RepSuscriptor }';\r\n";

                byte[] imagen = await api.ImagenCargar(repcodigo, repclave, cambio.RscScript);

                var fileName = Path.GetFileName(@cambio.RscScript.Replace(@"\", "/"));

                /*if (!fileName.StartsWith("@"))
                {
                    fileName = "@" + fileName;
                }*/

                var path = info.ProductsImagePath();

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var file = path + fileName;

                if (File.Exists(file))
                {
                    File.Delete(file);
                }

                using(var fileHandler = File.Create(file))
                {
                    await fileHandler.WriteAsync(imagen, 0, imagen.Length);
                }
                //File.WriteAllBytes(file, imagen);
            }

            return keysToDelete;
        }

        public async Task SubirSqliteDb(string repCodigo, string repclave)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                throw new Exception(AppResource.NoInternetMessage);
            }

            var path = Path.Combine(SqliteManager.DbDirectoryPath, "backup");

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);

            var dbPath = SqliteManager.DbPath();

            File.Copy(dbPath, Path.Combine(path, SqliteManager.DbName));

            var dbZipPath = Path.Combine(SqliteManager.DbDirectoryPath, "MovilBusinessDb.zip");

            if (File.Exists(dbZipPath))
            {
                File.Delete(dbZipPath);
            }

            ZipFile.CreateFromDirectory(path, dbZipPath, CompressionLevel.Optimal, false);

            Directory.Delete(path, true);

            var bytes = File.ReadAllBytes(dbZipPath);

            var suscriptor = DS_RepresentantesParametros.GetInstance().GetRepSuscriptor();

            var args = new SubirSqliteDbArgs()
            {
                User = new UsuarioArgs() { RepCodigo = repCodigo, RepClave = Functions.StringToMd5(repclave), Suscriptor = suscriptor },
                Base64DataBase = Convert.ToBase64String(bytes)
            };

            await api.SendDatabaseBackup(args);

        }

        public async Task CambiarContraseñaUsuario(string oldpass, string newPass, string repcodigo)
        {
            await api.CambiarContraseñaUsuario(repcodigo, oldpass, newPass, myParametro.GetRepSuscriptor());

            SqliteManager.GetInstance().CreateCommand("update Representantes set RepClave = ? where RepCodigo = ? and RepClave = ?", new string[] { newPass, repcodigo, oldpass }).ExecuteNonQuery();
        } 
        
        public void CambiarContraseñaUsuarios(string repcodigo)
        {
            SqliteManager.GetInstance().CreateCommand("update Representantes set RepClave = '@@#@@' where RepCodigo != ? and trim(upper(RepCargo)) != 'AUDITOR' ", new string[] {repcodigo}).ExecuteNonQuery();
        }

        private string  GetSuscriptorFromReplicacionesSuscriptores()
        {
           var list = SqliteManager.GetInstance().Query<ReplicacionesSuscriptores>("select RepSuscriptor from ReplicacionesSuscriptores ", new string[] { });
           
            if(list!=null && list.Count>0)
            {
                return list[0].RepSuscriptor;
            }
            return null;
        }

        private void NoticiasCreate(string sql, SqliteManager db)
        {
            var cmd = db.CreateCommand(sql.Replace("Noticias", "NoticiasTemp"), new object[] { });
            cmd.ExecuteNonQuery();
        }
    }
}
