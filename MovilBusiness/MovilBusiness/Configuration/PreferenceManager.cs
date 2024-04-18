using MovilBusiness.Enums;
using MovilBusiness.Model.Internal;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MovilBusiness.Configuration
{
    public class PreferenceManager
    {
        public Task Put(string key, string value)
        {
            return Task.Run(() =>
            {
                Xamarin.Forms.Application app = Xamarin.Forms.Application.Current;

                app.Properties[key] = value;
                app.SavePropertiesAsync();

            });

        }

        public T Get<T>(string key)
        {
            Xamarin.Forms.Application app = Xamarin.Forms.Application.Current;

            if (app.Properties.ContainsKey(key))
            {
                return JsonConvert.DeserializeObject<T>(app.Properties[key].ToString());
            }
            else
            {
                return default(T);
            }
        }

        public string Get(string key)
        {
            Xamarin.Forms.Application app = Xamarin.Forms.Application.Current;

            if (app.Properties.ContainsKey(key))
            {
                return app.Properties[key].ToString();
            }
            else
            {
                return null;
            }
        }

        public bool Remove(string key)
        {
            Xamarin.Forms.Application app = Xamarin.Forms.Application.Current;

            if (app.Properties.ContainsKey(key))
            {
                return app.Properties.Remove(key);
            }
            else
            {
                return false;
            }
        }

        public bool IsSincronizacionTest(bool validateSuscriptor = false)
        {
            try
            {
                string value = Get("TestSync");

                if (string.IsNullOrWhiteSpace(value) || value != "1")
                {
                    return false;
                }

                return true;
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return false;
        }

        public Task SaveTestSync(bool value)
        {
            return Put("TestSync", value ? "1" : "0");
        }

        public Task SaveConnection(string value)
        {
            RemoveCustomConnection();


            return Put("Connection", value);
        }

        public Task SaveMainStandardUrl(string url)
        {
            var main = GetMainStandardUrl();

            if (!string.IsNullOrWhiteSpace(main))
            {
                Remove("MainUrl");
            }

            return Put("MainUrl", url);
        }

        public bool IsMainStandardUrlConfigured()
        {
            return !string.IsNullOrWhiteSpace(GetMainStandardUrl());
        }

        public string GetMainStandardUrl()
        {
            return Get("MainUrl");
        }

        public Task SetClientUrlSetted()
        {
            return Put("ClientUrlSetted", "1");
        }

        public Task SetVersion(string version)
        {
            return Put("versionreplicacion", version);
        }

        public bool IsClientUrlConfig()
        {
            var value = Get("ClientUrlSetted");

            return !string.IsNullOrWhiteSpace(value) && value == "1";
        }

        /*public Task SetOrientation(string orientation)
        {
            if(Get("orientation") != null)
            {
                Remove("orientation");
            }

            return Put("orientation", orientation);
        }

        public string GetOrientation()
        {
            var result =  Get("orientation");

            if (string.IsNullOrWhiteSpace(result))
            {
                result = "Portrait";
            }

            return result;
        }*/

        public ConnectionInfo GetConnection()
        {
            var mainUrl = new ConnectionInfo() { Key = "[1]", Descripcion = "Principal", Url = Application.Current.Resources["WebServiceUrl"].ToString() };

            try
            {
                var result = Get<ConnectionInfo>("Connection");

                if (result == null)
                {
                    return mainUrl;
                }

                if (string.IsNullOrEmpty(result.Url))
                {
                    result.Url = "";
                }

                return result;
            }
            catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return mainUrl;
        }

        public void RemoveCustomConnection()
        {
            try
            {
                var result = Get<ConnectionInfo>("Connection");

                if (result != null)
                {
                    Remove("Connection");
                }
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }
        }

        public Task SaveReplicacion(string value)
        {
            if(Get("repnombre") != null)
            {
                Remove("repnombre");
            }

            return Put("repnombre", value);
        }

        public string GetReplicacion()
        {
            var original = Application.Current.Resources["Replicacion"].ToString();

            var custom = Get("repnombre");

            if (string.IsNullOrWhiteSpace(custom))
            {
                return original;
            }
            else
            {
                return custom;
            }
        }

        public Task SaveCantidadDatosCiclo(string value)
        {
            if (Get("datosciclo") != null)
            {
                Remove("datosciclo");
            }

            return Put("datosciclo", value);
        }

        public int GetCantidadDatosCiclo()
        {
            var raw = Application.Current.Resources["CantidadDatos"];

            int.TryParse(raw == null ? "" : raw.ToString(), out int cantidadOriginal);          

            int.TryParse(Get("datosciclo"), out int custom);

            if (custom < 1)
            {
                return cantidadOriginal;
            }
            else
            {
                return custom;
            }
        }

        public VersionReplicacion GetVersionReplicacion()
        {
            var original = Application.Current.Resources["VersionReplicacion"].ToString();

            var custom = Get("versionreplicacion");

            var version = "";
            
            if (string.IsNullOrWhiteSpace(custom))
            {
                version = original;
            }
            else
            {
                version = custom;
            }

            switch (version)
            {
                case "V9":
                    return VersionReplicacion.V9;
                case "V8":
                    return VersionReplicacion.V8;
                case "V7":
                    return VersionReplicacion.V7;
                default:
                    return VersionReplicacion.Standard;
            }
        }

        public Task SaveVersionReplicacion(string value)
        {
            if (Get("versionreplicacion") != null)
            {
                Remove("versionreplicacion");
            }

            return Put("versionreplicacion", value);
        }

        public Task Save(string key, bool value)
        {
            return Task.Run(() =>
            {
               Application app = Application.Current;

                app.Properties[key] = value;
                app.SavePropertiesAsync();

            });

        }
        public void RemoveIsVisita(string key)
        {
            Application app = Application.Current;
            app.Properties.Remove(key);
        }

        public Task SaveLanguage(string value)
        {
            if (Get("language") != null)
            {
                Remove("language");
            }

            return Put("language", value);
        }

        public string GetLanguage()
        {
            var value = Get("language");

            if (string.IsNullOrWhiteSpace(value))
            {
                value = "es-ES";
            }

            return value;
        }

        public void SaveMicrosoftToken(string value)
        {
            if (Get("MicrosoftToken") != null)
            {
                Remove("MicrosoftToken");
            }

            Put("MicrosoftToken", value);
        }

        public string GetMicrosoftToken()
        {
            var value = Get("MicrosoftToken");

            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }

            return value;
        }
    }
}
