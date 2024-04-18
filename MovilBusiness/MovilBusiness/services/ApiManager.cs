using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.model.Internal.Structs.Args.services;
using MovilBusiness.model.webservice;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal.Structs.services;
using MovilBusiness.services;
using MovilBusiness.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MovilBusiness.Services
{

    public class ApiManager 
    {
        private DS_RepresentantesParametros myParm;

        private HttpClient client;

        private JsonSerializer Serializer = new JsonSerializer();

        private static ApiManager _instance;
        public static ApiManager GetInstance(string url)
        {
            if (_instance == null)
            {
                lock (typeof(ApiManager))
                {
                    if (_instance == null)
                    {
                        _instance = new ApiManager(url);
                    }
                }
            }

            return _instance;
        }

        public static void Close()
        {
            _instance = null;
        }

        private ApiManager(string url)
        {
            //SE AGREGO EL HttpClientHandler YA QUE ESTABA DANDO ERROR CON CONEXIONES HTTPS
            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;           
            client = new HttpClient(handler) { BaseAddress = new Uri(url) };
            client.Timeout.Add(new TimeSpan(0, 8, 0));
            myParm = DS_RepresentantesParametros.GetInstance();
            //client = new HttpClient() { BaseAddress = new Uri(url) };
            //client.Timeout.Add(new TimeSpan(0, 8, 0));
        }

        private async Task ValidateMicrosoftToken(bool forceNewToken = false)
        {
            if (!Arguments.UseMicrosoftAuth)
            {
                return;
            }

            //var preference = new PreferenceManager();

            string currentToken = (await new MicrosoftAuthServices().GetAuthenticationToken()).Token;//preference.GetMicrosoftToken();

            /*if (string.IsNullOrWhiteSpace(currentToken) || forceNewToken)
            {
                currentToken = (;
            }*/

            if (client.DefaultRequestHeaders.Contains("Authorization"))
            {
                client.DefaultRequestHeaders.Remove("Authorization");
            }

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + currentToken);
        }

        public async Task<VerificarSuscriptorResult> VerificarSuscriptor(UsuarioArgs args, VersionReplicacion versionReplicacion)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                throw new Exception("No estas conectado a internet");
            }

            await ValidateMicrosoftToken();

            try
            {
                var values = new StringContent(JsonConvert.SerializeObject(args), Encoding.UTF8, "application/json");

                HttpResponseMessage response;

                if(versionReplicacion == VersionReplicacion.V7 || versionReplicacion == VersionReplicacion.V8 || versionReplicacion == VersionReplicacion.V9)
                {
                    response = await client.PostAsync("VerificarSuscriptor", values);
                }
                else
                {
                    response = await client.PostAsync("VerificarSuscriptorLegacy", values);
                }

                if(Arguments.UseMicrosoftAuth && response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await ValidateMicrosoftToken(true);
                    return await VerificarSuscriptor(args, versionReplicacion);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(error))
                    {
                        throw new Exception(error);
                    }
                    else
                    {
                        throw new Exception("Error validando el suscriptor solicitado.");
                    }
                }

                var result = await Read<VerificarSuscriptorResult>(response);//JsonConvert.DeserializeObject<VerificarSuscriptorResult>(response.Result.Content.ReadAsStringAsync().Result);

                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<List<ReplicacionesTablas>> ReplicacionesTablasLeer(string repCodigo, string repClave, string repSuscriptor)
        {
            try
            {
                if (!CrossConnectivity.Current.IsConnected)
                {
                    throw new Exception("No estas conectado a internet");
                }

                await ValidateMicrosoftToken();

                var args = new ReplicacionesTablasLeerArgs
                {
                    RepNombre = "",
                    RepCodigo = repCodigo,
                    RepClave = Functions.StringToMd5(repClave),
                    RepSuscriptor = repSuscriptor
                };

                var values = new StringContent(JsonConvert.SerializeObject(args), Encoding.UTF8, "application/json");

                var response = await client.PostAsync("ReplicacionesTablasLeerNew", values);

                if (Arguments.UseMicrosoftAuth && response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await ValidateMicrosoftToken(true);
                    return await ReplicacionesTablasLeer(repCodigo, repClave, repSuscriptor);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(error))
                    {
                        throw new Exception(error);
                    }
                    else
                    {
                        throw new Exception("Error obteniendo tablas de la base de datos");
                    }
                }

                try
                {
                    return await Read<List<ReplicacionesTablas>>(response);//JsonConvert.DeserializeObject<List<ReplicacionesTablas>>(response.Result.Content.ReadAsStringAsync().Result);

                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                    throw e;
                }
            }catch(Exception e)
            {
                throw new Exception(e.InnerException != null && !string.IsNullOrWhiteSpace(e.InnerException.Message) ? e.InnerException.Message : e.Message);
            }
        }

        public async Task ExecuteQuery(List<ExecuteQueryValues> querys, string repcodigo, string repclave, string repSuscriptor)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                throw new Exception("No estas conectado a internet");
            }

            await ValidateMicrosoftToken();

            try
            {
                var args = new ExecuteQueryArgs()
                {
                    User = new UsuarioArgs() { RepCodigo = repcodigo, RepClave = Functions.StringToMd5(repclave), Suscriptor = repSuscriptor },
                    Values = querys
                };

                //using (var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(500)))
                //{
                var response = await client.PostAsync("ExecuteQuery", new StringContent(JsonConvert.SerializeObject(args), Encoding.UTF8, "application/json"));//, tokenSource.Token);


                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await ValidateMicrosoftToken(true);
                    await ExecuteQuery(querys, repcodigo, repclave, repSuscriptor);
                    return;
                }

                // var _dataResponse = JToken.Parse(JsonConvert.SerializeObject(args));//Para Conseguir el Json y realizar pruebas
                if (!response.IsSuccessStatusCode)
                    {
                        var raw = await response.Content.ReadAsStringAsync();

                        if (string.IsNullOrEmpty(raw))
                        {
                            throw new Exception("Error ejecutando cambios en el servidor");
                        }
                        else
                        {
                            throw new Exception(raw);
                        }
                    }
                //}

            }catch(AggregateException e)
            {
                throw new Exception(e.InnerException != null && !string.IsNullOrWhiteSpace(e.InnerException.Message) ? e.InnerException.Message : e.Message);
            }
        }

        public async Task ExecUpdate(string query, string repcodigo, string repclave)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                throw new Exception("No estas conectado a internet");
            }

            await ValidateMicrosoftToken();

            try
            {
                var args = new ExecUpdateArgs()
                {
                    User = new UsuarioArgs() { RepCodigo = repcodigo, RepClave = Functions.StringToMd5(repclave), Suscriptor = "" },
                    Query = query
                };

                var response = await client.PostAsync("ExecUpdate", new StringContent(JsonConvert.SerializeObject(args), Encoding.UTF8, "application/json"));

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && Arguments.UseMicrosoftAuth)
                {
                    await ValidateMicrosoftToken(true);
                    await ExecUpdate(query, repcodigo, repclave);
                    return;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var raw = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(raw))
                    {
                        throw new Exception("Error ejecutando cambios en el servidor");
                    }
                    else
                    {
                        throw new Exception(raw);
                    }
                }
            }catch(Exception e)
            {
                throw new Exception(e.InnerException != null && !string.IsNullOrWhiteSpace(e.InnerException.Message) ? e.InnerException.Message : e.Message);
            }
        }
        
        /// <summary>
        /// Obtener datos desde el servidor por suscriptor
        /// </summary>
        /// <param name="repcodigo">El codigo del representante</param>
        /// <param name="repclave">La clave del representante (Debe ser MD5)</param>
        /// <param name="repSuscriptor">El suscriptor del representante</param>
        /// <returns>una lista con los cambios pendientes del suscriptor</returns>
        public async Task<List<ReplicacionesSuscriptoresCambios>> SuscriptoresCambiosLeer(string repcodigo, string repclave, string repSuscriptor, string deleteQuery, VersionReplicacion versionReplicacion, int cantidadCambios, bool issincronizar = false)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                throw new Exception("No estas conectado a internet");
            }

            await ValidateMicrosoftToken();

            try
            {

                var args = new ReplicacionesSuscriptoresCambiosLeerArgs()
                {
                    User = new UsuarioArgs() { RepCodigo = repcodigo, RepClave = Functions.StringToMd5(repclave), Suscriptor = repSuscriptor },
                    Limit = cantidadCambios,
                    DeleteQuery = deleteQuery,
                    IsSincronizar = issincronizar,
                };

                //using(var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(500)))
                //{
                    HttpResponseMessage response;

                    switch (versionReplicacion)
                    {

                        case VersionReplicacion.V9:
                        response = await client.PostAsync("SuscriptoresCambiosLeerV9", new StringContent(
                                                               JsonConvert.SerializeObject(args),
                                                               Encoding.UTF8, "application/json"
                                                               ));//, tokenSource.Token);
                            break;

                        case VersionReplicacion.V8:
                            response = await client.PostAsync("SuscriptoresCambiosLeerV8", new StringContent(
                                                                   JsonConvert.SerializeObject(args),
                                                                   Encoding.UTF8, "application/json"
                                                                   ));//, tokenSource.Token);
                        break;

                        case VersionReplicacion.V7:
                            response = await client.PostAsync("SuscriptoresCambiosLeer", new StringContent(
                                                                   JsonConvert.SerializeObject(args),
                                                                   Encoding.UTF8, "application/json"
                                                                   ));//, tokenSource.Token);
                        break;

                        default:
                            response = await client.PostAsync("SuscriptoresCambiosLeerLegacy", new StringContent(
                                                       JsonConvert.SerializeObject(args),
                                                       Encoding.UTF8, "application/json"
                                                       ));//, tokenSource.Token);
                        break;
                    }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && Arguments.UseMicrosoftAuth)
                {
                    await ValidateMicrosoftToken(true);
                    return await SuscriptoresCambiosLeer(repcodigo, repclave, repSuscriptor, deleteQuery, versionReplicacion, cantidadCambios, issincronizar);
                }

                if (!response.IsSuccessStatusCode)
                    {
                        var raw = await response.Content.ReadAsStringAsync();

                        if (string.IsNullOrEmpty(raw))
                        {
                            throw new Exception("Error ejecutando cambios en el servidor");
                        }
                        else
                        {
                            throw new Exception(raw);
                        }
                    }

                    return await Read<List<ReplicacionesSuscriptoresCambios>>(response);//JsonConvert.DeserializeObject<List<ReplicacionesSuscriptoresCambios>>(response.Result.Content.ReadAsStringAsync().Result);
                //}

            } catch (Exception e)
            {
                throw new Exception(e.InnerException != null && !string.IsNullOrWhiteSpace(e.InnerException.Message) ? e.InnerException.Message : e.Message);
            }
        }

        public async Task<List<Empresa>> EmpresasCargar(string repCodigo, string repClave)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                throw new Exception("No estas conectado a internet");
            }

            await ValidateMicrosoftToken();

            try
            {
                var args = new UsuarioArgs()
                {
                    RepCodigo = repCodigo,
                    RepClave = Functions.StringToMd5(repClave)
                };

                var response = await client.PostAsync("EmpresasCargar", new StringContent(JsonConvert.SerializeObject(args), Encoding.UTF8, "application/json"));

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && Arguments.UseMicrosoftAuth)
                {
                    await ValidateMicrosoftToken(true);
                    return await EmpresasCargar(repCodigo, repClave);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var raw = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(raw))
                    {
                        throw new Exception("Error ejecutando cambios en el servidor");
                    }
                    else
                    {
                        throw new Exception(raw);
                    }
                }

                return await Read<List<Empresa>>(response);//JsonConvert.DeserializeObject<List<Empresa>>(response.Result.Content.ReadAsStringAsync().Result);

            }catch(Exception e)
            {
                throw new Exception(e.InnerException != null && !string.IsNullOrWhiteSpace(e.InnerException.Message) ? e.InnerException.Message : e.Message);
            }
        }

        public async Task TransaccionesImagenesInsertar(List<TransaccionesImagenesTemp> imagenes, string suscriptor)
        {
            var imagetocount = new List<TransaccionesImagenesTemp>();
            var imagetorest = new List<TransaccionesImagenesTemp>();
            var imaget = new List<TransaccionesImagenesTemp>();
            ImagenesInsertarArgs args;
           
            if (!CrossConnectivity.Current.IsConnected)
            {
                throw new Exception("No estas conectado a internet");
            }

            await ValidateMicrosoftToken();

            try
            {
                if (imagenes == null || imagenes.Count == 0)
                {
                    return;
                }

                foreach (var img in imagenes.GroupBy(s => s.RepTablaKey).Select(g => g.FirstOrDefault()))
                {
                    IEnumerable<Task> tasks = imagenes.Where(i => i.RepTablaKey == img.RepTablaKey).Select(async imagen =>
                    {
                        args = new ImagenesInsertarArgs()
                        {
                            Imagenes = new List<TransaccionesImagenesTemp>()
                        {
                            imagen
                        }
                        ,
                            User = new UsuarioArgs()
                            {
                                RepCodigo = Arguments.CurrentUser.RepCodigo
                        ,
                                RepClave = Functions.StringToMd5(Arguments.CurrentUser.RepClave)
                        ,
                                Suscriptor = suscriptor
                            }
                        };

                        var response = client.PostAsync("ImagenesInsertar", new StringContent(JsonConvert.SerializeObject(args), Encoding.UTF8, "application/json"));
                        response.Wait();
                    });

                    await Task.WhenAll(tasks);
                }
            }
            catch(Exception e)
            {
                throw new Exception(e.InnerException != null && !string.IsNullOrWhiteSpace(e.InnerException.Message) ? e.InnerException.Message : e.Message);
            }
        }

        /// <summary>
        /// Obtener imagenes desde el servidor por suscriptor
        /// </summary>
        /// <param name="repcodigo">El codigo del representante</param>
        /// <param name="repclave">La clave del representante (Debe ser MD5)</param>
        /// <param name="repSuscriptor">El suscriptor del representante</param>
        /// <returns>una lista con los cambios pendientes del suscriptor</returns>
        public async Task<List<ReplicacionesSuscriptoresCambios>> SuscriptoresCambiosImagenesLeer(string repcodigo, string repclave, string repSuscriptor)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                throw new Exception("No estas conectado a internet");
            }

            await ValidateMicrosoftToken();

            try
            {
                var args = new ReplicacionesSuscriptoresCambiosLeerArgs()
                {
                    User = new UsuarioArgs() { RepCodigo = repcodigo, RepClave = Functions.StringToMd5(repclave), Suscriptor = repSuscriptor },
                    Limit = 500
                };
                
                var response = await client.PostAsync("SuscriptoresCambiosLeerImagenes", new StringContent(JsonConvert.SerializeObject(args), Encoding.UTF8, "application/json"));

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && Arguments.UseMicrosoftAuth)
                {
                    await ValidateMicrosoftToken(true);
                    return await SuscriptoresCambiosImagenesLeer(repcodigo, repclave, repSuscriptor);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var raw = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(raw))
                    {
                        throw new Exception("Error cargando imagenes desde el servidor");
                    }
                    else
                    {
                        throw new Exception(raw);
                    }
                }

                return await Read<List<ReplicacionesSuscriptoresCambios>>(response);//JsonConvert.DeserializeObject<List<ReplicacionesSuscriptoresCambios>>(response.Result.Content.ReadAsStringAsync().Result);

            }
            catch(Exception e)
            {
                throw new Exception(e.InnerException != null && !string.IsNullOrWhiteSpace(e.InnerException.Message) ? e.InnerException.Message : e.Message);
            }
        }

        public async Task<byte[]> ImagenCargar(string repcodigo, string repclave, string serverPath)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                throw new Exception("No estas conectado a internet");
            }

            await ValidateMicrosoftToken();

            try
            {
                var args = new ImagenCargarArgs()
                {
                    User = new UsuarioArgs() { RepCodigo = repcodigo, RepClave = Functions.StringToMd5(repclave) },
                    ImagePath = serverPath
                };

                var response = await client.PostAsync("ImagenesCargar", new StringContent(JsonConvert.SerializeObject(args), Encoding.UTF8, "application/json"));

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && Arguments.UseMicrosoftAuth)
                {
                    await ValidateMicrosoftToken(true);
                    return await ImagenCargar(repcodigo, repclave, serverPath);
                }


                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(await response.Content.ReadAsStringAsync());
                }

                return Convert.FromBase64String(JsonConvert.DeserializeObject<string>(await response.Content.ReadAsStringAsync()));
                
            }catch(Exception e)
            {
                throw new Exception(e.InnerException != null && !string.IsNullOrWhiteSpace(e.InnerException.Message) ? e.InnerException.Message : e.Message);
            }
        }

        public async Task<List<T>> RawQuery<T>(string repcodigo, string repclave, string query)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                throw new Exception("No estas conectado a internet");
            }

            await ValidateMicrosoftToken();

            try
            {
                var args = new QueryArgs()
                {
                    User = new UsuarioArgs()
                    {
                        RepCodigo = repcodigo,
                        RepClave = Functions.StringToMd5(repclave)
                    },
                    Query = query
                };

                var response = await client.PostAsync("RawQuery", new StringContent(JsonConvert.SerializeObject(args), Encoding.UTF8, "application/json"));

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && Arguments.UseMicrosoftAuth)
                {
                    await ValidateMicrosoftToken(true);
                    return await RawQuery<T>(repcodigo, repclave, query);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var raw = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(raw))
                    {
                        throw new Exception("Error cargando datos desde el servidor");
                    }
                    else
                    {
                        throw new Exception(raw);
                    }
                }

                return JsonConvert.DeserializeObject<List<T>>(await response.Content.ReadAsStringAsync());

            }catch(Exception e)
            {
                throw new Exception(e.InnerException != null && !string.IsNullOrWhiteSpace(e.InnerException.Message) ? e.InnerException.Message : e.Message);
            }
        }
        
        public async Task<bool> RawQueryForsure(string repcodigo, string repclave, string query)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                throw new Exception("No estas conectado a internet");
            }

            await ValidateMicrosoftToken();

            try
            {
                var args = new QueryArgs()
                {
                    User = new UsuarioArgs()
                    {
                        RepCodigo = repcodigo,
                        RepClave = Functions.StringToMd5(repclave)
                    },
                    Query = query
                };

                var response = await client.PostAsync("RawQuery", new StringContent(JsonConvert.SerializeObject(args), Encoding.UTF8, "application/json"));

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && Arguments.UseMicrosoftAuth)
                {
                    await ValidateMicrosoftToken(true);
                    return await RawQueryForsure(repcodigo, repclave, query);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var raw = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(raw))
                    {
                        throw new Exception("Error cargando datos desde el servidor");
                    }
                    else
                    {
                        throw new Exception(raw);
                    }
                }

                string responses = await response.Content.ReadAsStringAsync();
                return responses.Contains("true");

                //var result = JsonConvert.DeserializeObject<bool>(responses);

            }
            catch (Exception e)
            {
                throw new Exception(e.InnerException != null && !string.IsNullOrWhiteSpace(e.InnerException.Message) ? e.InnerException.Message : e.Message);
            }
        }

        public async Task SendDatabaseBackup(SubirSqliteDbArgs args)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                throw new Exception("No estas conectado a internet");
            }

            await ValidateMicrosoftToken();

            try
            {
                var response = await client.PostAsync("SubirSqliteDb", new StringContent(JsonConvert.SerializeObject(args), Encoding.UTF8, "application/json"));

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && Arguments.UseMicrosoftAuth)
                {
                    await ValidateMicrosoftToken(true);
                    await SendDatabaseBackup(args);
                    return;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var raw = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(raw))
                    {
                        throw new Exception("Error cargando datos desde el servidor");
                    }
                    else
                    {
                        throw new Exception(raw);
                    }
                }
            }catch(Exception e)
            {
                throw new Exception(e.InnerException != null && !string.IsNullOrWhiteSpace(e.InnerException.Message) ? e.InnerException.Message : e.Message);
            }
        }

        /*public void SubirSqliteDb(string repcodigo, string repclave, string repSuscriptor)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                throw new Exception("No estas conectado a internet");
            }

            try
            {
                var path = SqliteManager.ZipDbPath();

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                using (var zip = ZipFile.Open(path, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(SqliteManager.DbPath(), "MovilBusinessDb.zip", CompressionLevel.Optimal);
                }

                if (!File.Exists(path))
                {
                    return;
                }

                var content = new MultipartFormDataContent();

                var bytes = File.ReadAllBytes(path);

                var args = new SubirSqliteDbArgs()
                {
                    User = new UsuarioArgs()
                    {
                        RepCodigo = repcodigo,
                        RepClave = Functions.StringToMd5(repclave),
                        Suscriptor = repSuscriptor
                    },
                    Base64DataBase = Convert.ToBase64String(bytes)
                };

                var response = client.PostAsync("SubirSqliteDb", new StringContent(JsonConvert.SerializeObject(args), Encoding.UTF8, "application/json"));

                if (!response.Result.IsSuccessStatusCode)
                {
                    var raw = response.Result.Content.ReadAsStringAsync().Result;

                    if (string.IsNullOrEmpty(raw))
                    {
                        throw new Exception("Error cargando datos desde el servidor");
                    }
                    else
                    {
                        throw new Exception(raw);
                    }
                }

            }
            catch (Exception e)
            {
                throw new Exception(e.InnerException != null && !string.IsNullOrWhiteSpace(e.InnerException.Message) ? e.InnerException.Message : e.Message);
            }
        }*/

        public async Task CambiarContraseñaUsuario(string repcodigo, string oldclave, string newclave, string suscriptor)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                throw new Exception("No estas conectado a internet");
            }

            await ValidateMicrosoftToken();

            try
            {
                var args = new CambiarContrasenaArgs()
                {
                    RepCodigo = repcodigo,
                    OldPass = oldclave,
                    NewPass = newclave,
                    Suscriptor = suscriptor
                };

                var response = await client.PostAsync("CambiarContraseñaUsuario", new StringContent(JsonConvert.SerializeObject(args), Encoding.UTF8, "application/json"));

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && Arguments.UseMicrosoftAuth)
                {
                    await ValidateMicrosoftToken(true);
                    await CambiarContraseñaUsuario(repcodigo, oldclave, newclave, suscriptor);
                    return;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var raw = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(raw))
                    {
                        throw new Exception("Error cargando datos desde el servidor");
                    }
                    else
                    {
                        throw new Exception(raw);
                    }
                }               

            }
            catch (Exception e)
            {
                throw new Exception(e.InnerException != null && !string.IsNullOrWhiteSpace(e.InnerException.Message) ? e.InnerException.Message : e.Message);
            }
        }

        public static async Task<string> GetClientesUrl(string key)
        {
            var client = new HttpClient() { BaseAddress = new Uri("http://movilbusiness.com.do/eMovilBusiness/MovilBusinessApi2020/MDSOFT/api/ReplicacionesSuscriptores/") };

            client.Timeout.Add(new TimeSpan(0, 8, 0));

            var args = new GetClientesUrlArgs() { Key = key };

            var response = await client.PostAsync("GetClientesUrl", new StringContent(JsonConvert.SerializeObject(args), Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var raw = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(raw))
                {
                    throw new Exception("Error validando el key en el servidor");
                }
                else
                {
                    throw new Exception(raw);
                }
            }

            var url = await response.Content.ReadAsStringAsync();

            var newUrl = await response.Content.ReadAsStringAsync();

            if (!Functions.IsUrlValid(newUrl))
            {
                throw new Exception("Respuesta del servidor: " + newUrl);
            }

            return newUrl;
        }

        public static async Task<string> GetVersion(string key)
        {
            var client = new HttpClient() { BaseAddress = new Uri("http://movilbusiness.com.do/eMovilBusiness/MovilBusinessApi/api/ReplicacionesSuscriptores/") };

            client.Timeout.Add(new TimeSpan(0, 8, 0));

            var args = new GetClientesUrlArgs() { Key = key };

            var response = await client.PostAsync("GetClientesVersion", new StringContent(JsonConvert.SerializeObject(args), Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var raw = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(raw))
                {
                    throw new Exception("Error validando el key en el servidor");
                }
                else
                {
                    throw new Exception(raw);
                }
            }

            var version = await response.Content.ReadAsStringAsync();

            var newVersion = JsonConvert.DeserializeObject<string>(version);

            if (string.IsNullOrWhiteSpace(newVersion))
            {
                throw new Exception("Tipo de versión no válido");
            }

            return newVersion;
        }

        public async Task<List<T>> PresupuestosCargarOnline<T>(string repcodigo, string repclave, int cliid , string preTipo, string preAnio, string preMes)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                throw new Exception("No estas conectado a internet");
            }

            await ValidateMicrosoftToken();

            try
            {
                var args = new PresupuestosOnlineArgs()
                {
                    Cliid = cliid,
                    PreTipo = preTipo,
                    PreAnio = preAnio,
                    PreMes = preMes,
                 User = new UsuarioArgs()
                    {
                        RepCodigo = Arguments.CurrentUser.RepCodigo == repcodigo ? Arguments.CurrentUser.RepCodigo : repcodigo, //Arguments.CurrentUser.RepCodigo,
                        RepClave = Functions.StringToMd5(Arguments.CurrentUser.RepCodigo == repcodigo ? Arguments.CurrentUser.RepClave : repcodigo),
                        RepCurrent = repcodigo,
                 },
              
                };
                ///var _dataResponse = JToken.Parse(JsonConvert.SerializeObject(args));//Para Conseguir el Json y realizar pruebas
                var response = await client.PostAsync("PresupuestosCargarOnline", new StringContent(JsonConvert.SerializeObject(args), Encoding.UTF8, "application/json"));

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && Arguments.UseMicrosoftAuth)
                {
                    await ValidateMicrosoftToken(true);
                    return await PresupuestosCargarOnline<T>(repcodigo, repclave, cliid, preTipo, preAnio, preMes);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var raw = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(raw))
                    {
                        throw new Exception("Error cargando datos desde el servidor");
                    }
                    else
                    {
                        throw new Exception(raw);
                    }
                }

                return JsonConvert.DeserializeObject<List<T>>(await response.Content.ReadAsStringAsync());

            }
            catch (Exception e)
            {
                throw new Exception(e.InnerException != null && !string.IsNullOrWhiteSpace(e.InnerException.Message) ? e.InnerException.Message : e.Message);
            }
        }

        public async Task<List<T>> PresupuestosCombos<T>(string repcodigo, string repclave, int tipo, int campo,string pretipo="", string preAnio="")
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                throw new Exception("No estas conectado a internet");
            }

            await ValidateMicrosoftToken();

            try
            {
                var args = new PresupuestosCombosArgs()
                {
                    Tipo=tipo,
                    Campo=campo,    
                    PreTipo= pretipo,
                    PreAnio = preAnio,                   
                    User = new UsuarioArgs()
                    {
                        RepCodigo = Arguments.CurrentUser.RepCodigo,
                        RepClave = Functions.StringToMd5(Arguments.CurrentUser.RepClave),
                        RepCurrent = repcodigo,
                    },

                };
                //var _dataResponse = JToken.Parse(JsonConvert.SerializeObject(args));//Para Conseguir el Json y realizar pruebas
                var response = await client.PostAsync("PresupuestosCombos", new StringContent(JsonConvert.SerializeObject(args), Encoding.UTF8, "application/json"));

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && Arguments.UseMicrosoftAuth)
                {
                    await ValidateMicrosoftToken(true);
                    return await PresupuestosCombos<T>(repcodigo, repclave, tipo, campo, pretipo, preAnio);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var raw = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(raw))
                    {
                        throw new Exception("Error cargando datos desde el servidor");
                    }
                    else
                    {
                        throw new Exception(raw);
                    }
                }

                return JsonConvert.DeserializeObject<List<T>>(await response.Content.ReadAsStringAsync());

            }
            catch (Exception e)
            {
                throw new Exception(e.InnerException != null && !string.IsNullOrWhiteSpace(e.InnerException.Message) ? e.InnerException.Message : e.Message);
            }
        }

        private async Task<T> Read<T>(HttpResponseMessage response)
        {
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                using (var reader = new StreamReader(stream))
                {
                    using (var json = new JsonTextReader(reader))
                    {
                        return Serializer.Deserialize<T>(json);
                    }
                }
            }
        }


    }
}
