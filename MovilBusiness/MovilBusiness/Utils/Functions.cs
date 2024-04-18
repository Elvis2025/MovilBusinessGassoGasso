
using DLToolkit.Forms.Controls;
using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Views.Components.ListItemRows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Connectivity;
using Plugin.Geolocator;
using SQLite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Xamarin.Forms;

namespace MovilBusiness.Utils
{
    public static class Functions
    {
        public static string StringToMd5(string value)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(value);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                string sb = "";
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb += (hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public static bool IsJsonArrayValid(string list)
        {
            try
            {
                JArray.Parse(list);

                return true;
            }
            catch (System.Exception e)
            {
                Console.Write(e.Message);
            }
            return false;
        }
         
        public static string ToHexString(this Xamarin.Forms.Color color)
        {
            var red = (int)(color.R * 255);
            var green = (int)(color.G * 255);
            var blue = (int)(color.B * 255);
            var alpha = (int)(color.A * 255);
            var hex = $"#{alpha:X2}{red:X2}{green:X2}{blue:X2}";

            return hex;
        }

        public static void StartListeningForLocations(Action<double, double> onLocationChanged = null)
        {

            if (!DS_RepresentantesParametros.GetInstance().GetParGPS() || !CrossGeolocator.IsSupported || CrossGeolocator.Current == null || !CrossGeolocator.Current.IsGeolocationEnabled)
            {
                return;
            }

            var location = CrossGeolocator.Current;

            if (location.IsListening)
            {
                return;
            }

            location.PositionChanged += (sender, a) => 
            {
                if(a.Position.Latitude != 0 || a.Position.Longitude != 0)
                {
                    Arguments.Values.CurrentLocation = new Location(a.Position.Latitude, a.Position.Longitude);

                    if(onLocationChanged != null)
                    {
                        onLocationChanged.Invoke(a.Position.Latitude, a.Position.Longitude);
                    }
                }      
            };
            
            location.StartListeningAsync(TimeSpan.FromSeconds(20), DS_RepresentantesParametros.GetInstance().GetParGpsDistanciaMinimaActualizacion());
        }

        /*public static void OpenMap(double latitud, double longitud, string label)
        {
            if(latitud == 0 && longitud == 0)
            {
                return;
            }

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    Device.OpenUri(new Uri("http://maps.apple.com/?daddr=" + latitud + "," + longitud));
                    break;
                case Device.Android:
                    var request = string.Format("geo:" + latitud + "," + longitud + "?=" + latitud + "," + longitud + (!string.IsNullOrWhiteSpace(label)?" (" + label + ")":""));
                    Device.OpenUri(new Uri(request));
                    break;
            }
        }*/

        public static async Task StopListeningForLocations()
        {
            if (!DS_RepresentantesParametros.GetInstance().GetParGPS() || !CrossGeolocator.Current.IsGeolocationEnabled)
            {
                return;
            }

            await CrossGeolocator.Current.StopListeningAsync();
        }

        public static string AppVersion
        {
            get
            {
                try
                {
                    var info = DependencyService.Get<IAppInfo>();

                    if (info != null)
                    {
                        return info.AppVersion();                        
                    }

                }catch(Exception e)
                {
                    Console.Write(e.Message);
                }

                return "";
            }
        }

        public static string UltimaHoraSincronizacion
        {
            get
            {
                try
                {
                    string fecha = SqliteManager.GetInstance().Query<ReplicacionesSuscriptores>("select RepFechaUltimaSincronizacion from ReplicacionesSuscriptores ").FirstOrDefault().RepFechaUltimaSincronizacion;

                    bool result = DateTime.TryParse(fecha, out DateTime date);

                    return result ? "Hace: " + ((int)Math.Abs((date - DateTime.Now).TotalMinutes) / 60)  + ":" + ((int)(date - DateTime.Now).TotalMinutes % 60): "Hace: 0:00";

                }
                catch(Exception e)
                {
                    Console.Write(e.Message);
                }

                return "";
            }
        }

        public static string CurrentDate(string format)
        {
            if (format == null || format.Trim().Length == 0)
            {
                format = "yyyy-MM-dd HH:mm:ss";
            }
            return DateTime.Now.ToString(format);
        }
        public static string CurrentDate()
        {
            return CurrentDate(null);
        }

        public static void SaveSuscriptorCambioScript(string script, string tableName, string rowguid, string tipoScript)
        {
            SuscriptorCambios model = new SuscriptorCambios();
            model.FechaActualizacion = CurrentDate();
            if (tipoScript == "I")
            {
                model.RowguidTabla = rowguid;
            }
            model.Script = script;
            model.TipoScript = tipoScript;
            model.Tabla = tableName;
            model.rowguid = Guid.NewGuid().ToString();
            SqliteManager.GetInstance().Insert(model);
        }

        public static string FormatDate(string fecha, string format)
        {
            try
            {
                return Convert.ToDateTime(fecha.Replace("T", " ")).ToString(format);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return "";

            }
        }

        public static bool IsConnectingToInternet
        {
            get { return CrossConnectivity.Current.IsConnected; }
        }

        public static bool ValidateEmail(string email)
        {
            Regex EmailRegex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");

            if (string.IsNullOrWhiteSpace(email))
                return false;

            return EmailRegex.IsMatch(email);
        }

        public static string ValidateUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return "";
            }

            if (!url.Contains("http") && !url.Contains("https"))
            {
                url = "http://" + url;
            }

            return url;
        }

        public static float GetDiferenciaHorariaSqlite()
        {
            List<float> list = SqliteManager.GetInstance().Query<float>("select (julianday('" + CurrentDate() + "') - julianday(datetime('now'))) * 24 ", 
                new string[] { });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }
            return 0;
        }

        public static void CreateFile(string path)
        {
            using (var streamWriter = new StreamWriter(path, true))
            {
                streamWriter.WriteLine(DateTime.UtcNow);
            };
        }

        public static List<KV> DinamicQuery(string sql, bool withDefault = false)
        {
            if (DS_RepresentantesParametros.GetInstance().GetParSectores() > 0 && Arguments.Values.CurrentSector != null && sql.Contains("@SecCodigo"))
            {
                sql = sql.Replace("@SecCodigo", "'" + Arguments.Values.CurrentSector.SecCodigo + "'");
            }

            if (withDefault)
            {
                var list = new List<KV>() { new KV("-1", "(Seleccione)") };

                list.AddRange(SqliteManager.GetInstance().Query<KV>(sql, new string[] { }));

                return list;
            }

            return SqliteManager.GetInstance().Query<KV>(sql, new string[] { });
        }

        public static int GetWeekOfYear(DateTime d)
        {
            CultureInfo cul = CultureInfo.CurrentCulture;

            return cul.Calendar.GetWeekOfYear(
             d,
             CalendarWeekRule.FirstDay,
             DayOfWeek.Monday);

        }
        public static int GetWeekOfMonth(DateTime date)
        {
            CultureInfo ciCurr = CultureInfo.CurrentCulture;
            int weekNum = ciCurr.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var cicloSemana = 4;

            var parCiclo = DS_RepresentantesParametros.GetInstance().GetParCiclosSemanas();

            if (parCiclo > 0)
            {
                cicloSemana = parCiclo;
            }

            var result = weekNum % cicloSemana;

            if (result == 0)
            {
                return cicloSemana;
            }
            else
            {
                return result;
            }
        }
        public static int GetWeekOfMonth(DateTime date,Representantes current)
        {
            CultureInfo ciCurr = CultureInfo.CurrentCulture;
            int weekNum = ciCurr.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var cicloSemana = 4;

            int result;

            var parCiclo = DS_RepresentantesParametros.GetInstance().GetParCiclosSemanas();

            if (parCiclo > 0)
            {
                cicloSemana = parCiclo;
            }

            if (current != null)
            {
                var parcicove = DS_RepresentantesParametros.GetInstance().GetParCiclosSemanasxCodigoVendedor(current.RepCodigo);

                if (parcicove > 0)
                {
                    cicloSemana = parcicove;
                }
            }    

           result = weekNum % cicloSemana;

            if (result == 0)
            {
                return cicloSemana;
            }
            else if(result > 0 || result < 0)
            {
                return result;
            }

            result = weekNum % cicloSemana;
            
            if(result == 0)
            {
                return cicloSemana;
            }
            else
            {
                return result;
            }
            
        }
        /// <summary>
        /// Recibe Cédula o RNC y valida si es correcto
        /// </summary>
        /// <param name="documento"></param>
        /// <returns></returns>
        public static bool ValidarDocumento(string documento)
        {
            bool Resultado = false;

            if(string.IsNullOrEmpty(documento))
            {
                return true;
            }

            documento = documento.Replace("-","");

            switch (documento.Length)
            {
                case 9: //Es un RNC
                    Resultado = Valida_Rnc(documento);
                    break;
                case 11: //Es un RNC
                    Resultado = Valida_Cedula(documento);
                    break;
                default: //El número es inválido
                    Resultado = false;
                    break;
            }
            return Resultado;
        }

        private static bool Valida_Cedula(string ced)
        {
            string c = ced.Replace("-", "");
            string Cedula = c.Substring(0, c.Length - 1);
            string Verificador = c.Substring(c.Length - 1, 1);
            decimal suma = 0;

            int mod, dig, res;
            res = 0;

            for (int i = 0; i < Cedula.Length; i++)
            {
                mod = 0;
                if ((i % 2) == 0) mod = 1;
                else mod = 2;
                if (int.TryParse(Cedula.Substring(i, 1), out dig))
                {
                    res = dig * mod;
                }
                else
                {
                    return false;
                }
                if (res > 9)
                {
                    res = Convert.ToInt32(res.ToString().Substring(0, 1)) +
                    Convert.ToInt32(res.ToString().Substring(1, 1));
                }
                suma += res;

            }
            decimal el_numero = (10 - (suma % 10)) % 10;
            if ((el_numero.ToString() == Verificador) && (Cedula.Substring(0, 3) != "000"))
            {
                return true;
            }
            else
            {
                Console.WriteLine("La Cedula es ilegal \n" + "el digito verificador debio ser " + el_numero.ToString());
                return false;
            }
        }
        /// <summary>
        /// Recibe RNC y valida si es correcto
        /// </summary>
        /// <param name="rnc"></param>
        /// <returns></returns>       
        private static bool Valida_Rnc(string rnc)
        {
            int iDigital, p, t, d, r;
            string sCon;

            rnc = rnc.Replace("-", "");

            if (rnc.Length < 9)
                return false;

            iDigital = int.Parse(rnc.Substring(rnc.Length - 1));
            sCon = "79865432";
            p = 0; t = 0; r = 0; d = 0;

            for (int j = 0; j < 8; j++)
            {
                p = int.Parse(rnc.Substring(j, 1)) *
                    int.Parse(sCon.Substring(j, 1));
                t = t + p;
            }

            r = t % 11;
            d = 11 - r;
            switch (r)
            {
                case 0:
                    d = 2;
                    break;
                case 1:
                    d = 1;
                    break;
            }

            if (iDigital != d)
                return false;
            else
                return true;
        }

        public static Task DisplayAlert(string title, string message, string cancel = "Aceptar")
        {
            if(Application.Current == null || Application.Current.MainPage == null)
            {
                return Task.Delay(0);
            }
            return Application.Current.MainPage.DisplayAlert(title, message, cancel);
        }

        public static Task<bool> DisplayAlert(string title, string message, string accept, string cancel = "Aceptar")
        {
            return Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
        }

        public static Task<string> DisplayActionSheet(string title, string[] buttons)
        {
            return Application.Current.MainPage.DisplayActionSheet(title, "Aceptar", null, buttons);
        }

        public static string RellenaCeros(int posicion, string valor)
        {
            string returnValue = "";
            int p = posicion - valor.Length;

            for (int i = 0; i < p; i++)
            {
                returnValue += "0";
            }
            returnValue += valor;
            return returnValue;
        }

        public static string DinamicFiltersGenerateScript(FiltrosDinamicos filter, string valueToSearch, string secondFilter) 
        {
            string whereCondition = "";

            if(filter == null || (string.IsNullOrWhiteSpace(valueToSearch) && string.IsNullOrWhiteSpace(secondFilter)))
            {
                return whereCondition;
            }

            if (string.IsNullOrEmpty(valueToSearch))
            {
                valueToSearch = "";
            }

            valueToSearch = valueToSearch.Trim().ToUpper();

            switch (filter.FilTipo)
            {
                case 3:
                case 1:

                    string[] campos = filter.FilCampo.Split(',');

                    whereCondition = " and (";

                    if(filter.FilCondicion == "LIKEWORD")
                    {
                        var start = true;

                        var values = valueToSearch.Split(' ');

                        foreach(var campo in campos)
                        {
                            if (start)
                            {
                                start = false;
                            }
                            else
                            {
                                whereCondition += " OR ";
                            }

                            whereCondition += "(";

                            var firstValue = true;

                            foreach(var value in values)
                            {
                                if (firstValue)
                                {
                                    firstValue = false;
                                    whereCondition += campo + " LIKE '%" + value + "%'";
                                }
                                else
                                {
                                    whereCondition += " AND " + campo + " LIKE '%" + value + "%'";                                  
                                }
                                
                            }
                            
                            whereCondition += ")";
                        }

                        whereCondition += ") ";

                        return whereCondition;
                    }

                    string whereConditionArgs = "";

                    switch (filter.FilCondicion)
                    {
                        case "LIKE":
                            whereConditionArgs = " LIKE '%" + valueToSearch + "%'";
                            break;
                        case "=":
                            whereConditionArgs = " = '" + valueToSearch + "'";
                            break;
                        case "START":
                            whereConditionArgs = " LIKE '" + valueToSearch + "%'";
                            break;
                        case "LIKEWORD":

                            break;
                    }

                    bool first = true;

                    foreach (string campo in campos)
                    {
                        if (first)
                        {
                            whereCondition += campo.ToUpper() + whereConditionArgs;
                            first = false;
                        }
                        else
                        {
                            whereCondition += " OR " + campo.ToUpper() + whereConditionArgs;
                        }
                    }

                    whereCondition += ") ";

                    break;
                case 2:
                    switch (filter.FilCondicion)
                    {
                        case "LIKE":
                            whereCondition = " AND (" + filter.FilCampo + " LIKE '%" + secondFilter + "%') ";
                            break;
                        case "=":
                        default:
                            whereCondition = " AND (" + filter.FilCampo + " = '" + secondFilter + "') ";
                            break;
                        case "START":
                            whereCondition = " AND (" + filter.FilCampo + " LIKE '" + secondFilter + "%') ";
                            break;
                    }


                    break;
            }

            return whereCondition;
        }

        public static string GetEstatusVisitaIcon(int visEstatus)
        {
            switch (visEstatus)
            {
                case 0: //visita fallida
                    return "light_red";
                case 1: //visita no efectiva
                    return "light_yellow";
                case 2: //visita efectiva
                    return "light_green";
                default:
                    return "light_gray";
            }
        }

        public static Modules GetModuleByTableName(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                return Modules.NULL;
            }

            switch (tableName.Trim().ToUpper())
            {
                case "PEDIDOS":
                case "PEDIDOSCONFIRMADOS":
                    return Modules.PEDIDOS;
                case "DEVOLUCIONES":
                case "DEVOLUCIONESCONFIRMADOS":
                    return Modules.DEVOLUCIONES;
                case "RECIBOS":
                case "RECIBOSCONFIRMADOS":
                    return Modules.COBROS;
                case "DEPOSITOS":
                    return Modules.DEPOSITOS;
                case "INVENTARIOFISICO":
                case "INVENTARIOFISICOCONFIRMADO":
                    return Modules.INVFISICO;
                case "VENTAS":
                case "VENTASCONFIRMADOS":
                    return Modules.VENTAS;
                case "COTIZACIONES":
                case "COTIZACIONESCONFIRMADOS":
                    return Modules.COTIZACIONES;
                case "COMPRAS":
                case "COMPRASCONFIRMADOS":
                case "COMPRASPUSHMONEY":
                case "COMPRASPUSHMONEYCONFIRMADOS":
                    return Modules.COMPRAS;
                case "CAMBIOS":
                case "CAMBIOSCONFIRMADOS":
                    return Modules.CAMBIOSMERCANCIA;
                case "TRANSFERENCIASALMACENES":
                    return Modules.TRASPASOS;
                case "CONTEOSFISICOS":
                case "CONTEOSFISICOSCONFIRMADOS":
                    return Modules.CONTEOSFISICOS;
                case "REQUISICIONESINVENTARIO":
                    return Modules.REQUISICIONINVENTARIO;
                case "COLOCACIONPRODUCTOS":
                    return Modules.COLOCACIONMERCANCIAS;
                default:
                    return Modules.NULL;
            }
        }

        public static void SetListViewItemTemplateById(FlowListView listView, int rowId, bool reload = true)
        {
            listView.FlowColumnCount = 1;

            if (Arguments.Values.CurrentModule == Modules.INVFISICO /*|| Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS*/)
            {
                switch (rowId)
                {
                    default:
                        listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductosInventario));
                        break;
                    case 2:
                        listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductosInventario2));
                        break;
                    case 3:
                        listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductosInventario3));
                        break;
                    case 4:
                        listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductosInventario4));
                        break;
                    case 5:
                        listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductosInventario5));
                        break;
                    case 6:
                        listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductosInventario6));
                        break;
                }
                
                return;
            }

            if(Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS)
            {
                listView.FlowColumnTemplate = new DataTemplate(typeof(RowColocacionProductos));
                return;
            }

            var parRowDetallado = DS_RepresentantesParametros.GetInstance().GetParVisualizacionGridDetallada();

            switch (rowId)
            {
                case 18:
                    listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductos1));
                    break;
                case 3:
                    listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductos3));
                    break;
                case 2:
                    listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductos2));
                    break;
                case 4:
                    listView.FlowColumnTemplate = new DataTemplate(parRowDetallado ? typeof(RowProductos4Detallado) : typeof(RowProductos4));
                    break;
                case 5:
                    listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductos5));
                    break;
                case 6:
                    listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductos6));
                    break;
                case 7:
                    listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductos7));
                    break;
                case 8:
                    listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductos8));
                    break;
                case 9:
                    listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductos9));
                    break;
                case 10:
                    listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductos10));
                    break;
                case 11:
                    listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductos11));
                    break;
                case 12:
                    listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductos12));
                    break;
                case 13:
                    listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductos13));
                    break;
                case 14:
                    listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductos14));
                    break;
                case 15:
                    listView.FlowColumnCount = 3;
                    listView.FlowColumnTemplate = new DataTemplate(parRowDetallado ? typeof(RowProductosGrid3ColumnsDetallado) : typeof(RowProductosGrid3Columns));
                    break;
                case 16:
                    listView.FlowColumnCount = 2;
                    listView.FlowColumnTemplate = new DataTemplate(parRowDetallado ? typeof(RowProductosGrid2ColumnsDetallado) : typeof(RowProductosGrid2Columns));
                    break;
                case 17:
                    listView.FlowColumnTemplate = new DataTemplate(parRowDetallado ? typeof(RowProductosGrid2ColumnsDetallado) : typeof(RowProductosGrid2Columns));
                    break;
                case 19:                    
                    listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductos19));
                    break;
                case 20:
                    listView.FlowColumnTemplate = new DataTemplate(parRowDetallado ? typeof(RowProductos4Detallado) : typeof(RowProductos20));
                    break;
                case 21:
                    listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductos15));
                    break;
                case 22:
                    listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductos16));
                    break;
                case 23:
                    listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductos17));
                    break;
                case 24:
                    listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductos18));
                    break;
                case 25:
                    listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductos25));
                    break;
                case 26:
                    listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductos26));
                    break;
                case 27:
                    listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductos27));
                    break;
                case 28:
                    listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductos28));
                    break;
                case 29:
                    listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductos29));
                    break;
                case 30:
                    listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductos30));
                    break;
                default:
                    listView.FlowColumnTemplate = new DataTemplate(typeof(RowProductos1));
                    break;
            }

            if (reload)
            {
                listView.ForceReload();
            }
        }

        public static async Task<int> ShowAlertChangeRowDesign()
        {
            var Id = await DisplayActionSheet("Elegir diseño Row de productos", new string[] { "Diseño No.1", "Diseño No.2", "Diseño No.3", "Diseño No.5", "Diseño No.6", "Diseño No.7", "Diseño No.9", "Diseño No.10", "Diseño No.11", "Diseño No.12" , "Diseño Grid" });

            switch (Id)
            {
                case "Diseño No.1":
                    return 1;
                case "Diseño No.2":
                    return 2;
                case "Diseño No.3":
                    return 3;
                case "Diseño No.5":
                    return 5;
                case "Diseño No.6":
                    return 6;
                case "Diseño No.7":
                    return 7;
                case "Diseño No.9":
                    return 9;
                case "Diseño No.10":
                    return 10;
                case "Diseño No.11":
                    return 11;
                case "Diseño No.12":
                    return 12;
                case "Diseño No.13":
                    return 13;
                case "Diseño Grid":
                    return 15;
                default:
                    return -1;
            }
        }

        public static bool IsCompraFactura { get => Arguments.Values.CurrentModule == Modules.COMPRAS && Arguments.Values.CurrentClient != null && !string.IsNullOrWhiteSpace(Arguments.Values.CurrentClient.LipCodigoPM) && Arguments.Values.CurrentClient.LipCodigoPM.Trim().ToUpper() == "COMFACT"; }

        public static void WriteExceptionLog(Exception e, bool restartApp = false)
        {
            if (e == null)
            {
                return;
            }

            try
            {
                var log = new ErrorLog
                {
                    Message = e.Message,
                    Source = e.Source,
                    StackTrace = e.StackTrace,
                    ErrorFecha = CurrentDate(),
                    ErrorGuid = Guid.NewGuid().ToString()
                };

                if (e.InnerException != null)
                {
                    log.InnerMessage = e.InnerException.Message;
                    log.InnerSource = e.InnerException.Source;
                    log.InnerStackTrace = e.InnerException.StackTrace;
                }

                SqliteManager.GetInstance().Insert(log);

                SqliteManager.GetInstance().Execute("delete from ErrorLog where cast((julianday('now') - julianday(ErrorFecha)) as integer) > 30", new string[] { });
            }
            catch (Exception) {}

            if (restartApp)
            {
                Arguments.LogOut();

                Application.Current.MainPage = new NavigationPage(new Views.LoginPage())
                {
                    BarBackgroundColor = Color.FromHex("#1976D2"),
                    BarTextColor = Color.White
                };
            }
        }

        public static bool IsUrlValid(string source)
        {
            var value = Uri.TryCreate(source, UriKind.Absolute, out Uri uriResult);
            
            return value && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static string AddDaysOnDate(string date, int Dias)
        {
            DateTime date_time = Convert.ToDateTime(date);
            string FechaMasDias = date_time.AddDays(Dias).ToString("yyyy-MM-dd H:mm:ss");
            return FechaMasDias;
        }

        public static int GetTraIdByTabla(string tableName)
        {
            switch (tableName.ToUpper().Trim())
            {
                case "PEDIDOS":
                case "PEDIDOSCONFIRMADOS":
                    return 1;
                case "DEVOLUCIONES":
                case "DEVOLUCIONESCONFIRMADAS":
                    return 2;
                case "RECIBOS":
                case "RECIBOSCONFIRMADOS":
                    return 3;
                case "VENTAS":
                case "VENTASCONFIRMADOS":
                    return 4;
                case "COTIZACIONES":
                case "COTIZACIONESCONFIRMADOS":
                    return 5;
                case "CAMBIOS":
                case "CAMBIOSCONFIRMADOS":
                    return 6;
                case "INVENTARIOFISICO":
                case "INVENTARIOFISICOCONFIRMADOS":
                    return 7;
                case "DEPOSITOS":
                case "DEPOSITOSCONFIRMADOS":
                    return 9;
                case "ENTREGASDOCUMENTOS":
                case "ENTREGASDOCUMENTOSCONFIRMADOS":
                    return 10;
                case "COMPRAS":
                case "COMPRASCONFIRMADOS":
                    return 11;
                case "DEPOSITOSCOMPRAS":
                case "DEPOSITOSCOMPRASDETALLE":
                    return 12;
                case "VISITAS":
                case "VISITASCONFIRMADOS":
                    return 13;
                case "SOLICITUDACTUALIZACIONCLIENTES":
                    return 15;
                case "GASTOS":
                case "GASTOSCONFIRMADOS":
                    return 16;
                case "RECLAMACIONES":
                    return 23;
                case "REQUISICIONESINVENTARIOS":
                case "REQUISICIONESINVENTARIOSCONFIRMADOS":
                    return 24;
                case "DEPOSITOSGASTOS":
                case "DEPOSITOSGASTOSCONFIRMADOS":
                    return 25;
                case "CLIENTES": //prospectos
                    return 38;
                case "ENTREGASREPARTIDORTRANSACCIONES":
                    return 27;
                default:
                    return -1;
            }
        }

        public static string RoundTwoPositions(double valor, int decimalPlaces)
        {
            string formatter = "{0:f" + decimalPlaces + "}";
            return string.Format(formatter, valor);
        }

       public static bool RncIsValid(string rnc)
        {

            if (string.IsNullOrWhiteSpace(rnc) || (rnc.Length != 9 && rnc.Length != 11))
            {
                return false;
            }

            var rncValido = "";

            foreach (var caracter in rnc.ToCharArray())
            {
                if (!char.IsDigit(caracter))
                {
                    return false;
                }

                rncValido += caracter;
            }

            return rncValido.Length == 9 || rncValido.Length == 11;
        }

        public static bool IsSincronizacionTest(PreferenceManager pref = null)
        {
            var isTest = false;

            if(pref == null)
            {
                pref = new PreferenceManager();
            }
            
            try
            {
                var suscriptores = SqliteManager.GetInstance().Query<ReplicacionesSuscriptores>("select RsuTipo from ReplicacionesSuscriptores ", new string[] { });
                isTest = suscriptores != null && suscriptores.Count > 0 && suscriptores[0].RsuTipo == 2;
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return pref.IsSincronizacionTest() || isTest;
        }

        public static string GetCliDatosOtrosLabel(string CliDatosOtros)
        {
            if (Arguments.Values.CliDatosOtros == null || Arguments.Values.CliDatosOtros.Count == 0 || string.IsNullOrWhiteSpace(CliDatosOtros))
            {
                return "";
            }

            var result = "";

            foreach (var d in Arguments.Values.CliDatosOtros)
            {
                if (d.CodigoUso != null && CliDatosOtros.Contains(d.CodigoUso))
                {
                    result += d.Descripcion + ", ";
                }
            }

            if (!string.IsNullOrWhiteSpace(result))
            {
                result = result.Substring(0, result.Length - 2);
            }

            return result;
        }

        public static void UpdateParametroProOrden(string Valor, string rowguid)
        {
            Hash rep = new Hash("RepresentantesParametros");
            rep.Add("ParValor", Valor);
            rep.ExecuteUpdate("rowguid = '" + rowguid + "' ");
        }

        public static string GetrowguidParametro(string Parreferencia)
        {
            var list = SqliteManager.GetInstance().Query<RepresentantesParametros>("select ifnull(rowguid,'') as rowguid from RepresentantesParametros where ltrim(rtrim(Upper(ParReferencia))) = ? ", new string[] { Parreferencia });

            return  list[0].rowguid.ToString();
        }

        public static string GetrowguidTransaccion(string Tabla, string where, int TraSecuencia)
        {
            var list = SqliteManager.GetInstance().Query<RepresentantesParametros>("select ifnull(rowguid,'') as rowguid from "+Tabla+" where ltrim(rtrim(Upper("+ where +"))) = ? ", new string[] { TraSecuencia.ToString() });

            return list[0].rowguid.ToString();
        }

        public static double GetProUnidades(int proid)
        {
            var list = SqliteManager.GetInstance().Query<Productos>("select ifnull(ProUnidades, 1) as ProUnidades from Productos where proid = " + proid + " ", new string[] { });
            return list[0].ProUnidades;
        }

        public static string CrearNoLiquidacion(string RepCodigo, int CuaSecuencia)
        {
            string Rep, CuaSecu;
            string NoLiquidacion = "";
            Rep = RellenaCeros(4, Convert.ToInt32(RepCodigo));
            CuaSecu = RellenaCeros(5, CuaSecuencia);
            NoLiquidacion = "2" + Rep + CuaSecu;
            return NoLiquidacion;
        }

        public static string RellenaCeros(int Ceros, int Numero)
        {
            string R = "";
            int C = 0;

            C = Ceros - (System.Convert.ToString(Numero).Length);
            if (C < 0)
                return null;
            for (int i = 1; i <= C; i++)
            {
                R = R + "0";

            }
            R = R + System.Convert.ToString(Numero);

            return R;
        }


        public static double DistanceTo(double lat1, double lon1, double lat2, double lon2, char unit = 'K')
        {
            double rlat1 = Math.PI * lat1 / 180;
            double rlat2 = Math.PI * lat2 / 180;
            double theta = lon1 - lon2;
            double rtheta = Math.PI * theta / 180;
            double dist =
                Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
                Math.Cos(rlat2) * Math.Cos(rtheta);
            dist = Math.Acos(dist);
            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;

            double result;

            switch (unit)
            {
                case 'K': //Kilometers -> default
                    result = dist * 1.609344;
                    break;
                case 'N': //Nautical Miles 
                    result = dist * 0.8684;
                    break;
                case 'M': //Meters
                    result = dist * 1609.34;
                    break;
                default: //miles
                    result = dist;
                    break;
            }

            return Math.Round(result, 2);
        }

        public static string NumberToText(int number)
        {
            if (number == 0) return "Zero";


            if (number == -2147483648) return "Minus Two Hundred and Fourteen Crore Seventy Four Lakh Eighty Three Thousand Six Hundred and Forty Eight";

            int[] num = new int[4];
            int first = 0;
            int u, h, t;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (number < 0)
            {
                sb.Append("Menos ");
                number = -number;
            }


            string[] words0 = {"" ,"Uno ", "Dos ", "Tres ", "Cuatro ",
                              "Cinco " ,"Seis ", "Siete ", "Ocho ", "Nueve "};


            string[] words1 = {"Dies ", "Onces ", "Doce ", "Trece ", "Catorce ",
                                "Quince ","Diez y Seis ","Diez y Siete","Diez y Ocho ", "Nineteen "};
 

            string[] words2 = {"Vente ", "Trenta ", "Cuarenta ", "Cincuenta ", "Sesenta ",
                               "Setenta ","Ochenta ", "Noventa "};


            string[] words3 = { "Mil ", "Lakh ", "Crore " };


            num[0] = number % 1000; // units
            num[1] = number / 1000;
            num[2] = number / 100000;
            num[1] = num[1] - 100 * num[2]; // thousands
            num[3] = number / 10000000; // crores
            num[2] = num[2] - 100 * num[3]; // lakhs


            for (int i = 3; i > 0; i--)
            {
                if (num[i] != 0)
                {
                    first = i;
                    break;
                }
            }

            for (int i = first; i >= 0; i--)
            {
                if (num[i] == 0) continue;


                u = num[i] % 10; // ones
                t = num[i] / 10;
                h = num[i] / 100; // hundreds
                t += - 10 * h; // tens


                if (h > 0) sb.Append(h == 1? "" : words0[h] + "Cientos ");


                if (u > 0 || t > 0)
                {
                    //if (h > 0 || i == 0) sb.Append("y ");


                   // if (t == 0)//sb.Append(words0[u]);
                   if (t == 1)
                        sb.Append(words1[u]);
                    else if (t != 0)
                        sb.Append(words2[t - 2] + words0[u]);
                }


                if (i != 0) sb.Append(words3[i - 1]);


            }
            return sb.ToString().TrimEnd();
        }

        public static string NumberToTextV2(double numero)
        {

            if (numero == -2147483648) return "Minus Two Hundred and Fourteen Crore Seventy Four Lakh Eighty Three Thousand Six Hundred and Forty Eight";

            string[] unidades = { "Cero", "Uno", "Dos", "Tres", "Cuatro", "Cinco", "Seis", "Siete", "Ocho", "Nueve" };
            string[] especiales = { "Once", "Doce", "Trece", "Catorce", "Quince", "Dieciséis", "Diecisiete", "Dieciocho", "Diecinueve" };
            string[] decenas = { "Diez", "Veinte", "Treinta", "Cuarenta", "Cincuenta", "Sesenta", "Setenta", "Ochenta", "Noventa" };
            string[] centenas = { "", "Ciento", "Doscientos", "Trescientos", "Cuatrocientos", "Quinientos", "Seiscientos", "Setecientos", "Ochocientos", "Novecientos" };
            string[] miles = { "", "Mil", "Millon", "Mil Millones" };

            int[] num = new int[4];
            int x, y, z;

            string letras = "";

            if (numero == 0)
            {
                letras = "Cero";
            }
            else if (numero < 0)
            {
                letras = "Menos " + NumberToTextV2(Math.Abs(numero));
            }
            else
            {
                long parteEntera = (long)numero;
                double parteDecimal = (numero - parteEntera) * 100;

                if (parteEntera == 0)
                {
                    letras = "Cero";
                }
                else
                {
                    x = 0;
                    while (parteEntera > 0)
                    {
                        num[x] = (int)(parteEntera % 1000);
                        parteEntera /= 1000;
                        x++;
                    }

                    for (y = x - 1; y >= 0; y--)
                    {
                        z = num[y];

                        if (z == 0)
                        {
                            continue;
                        }

                        if (z < 10)
                        {
                            letras += unidades[z] + " ";
                        }
                        else if (z < 20)
                        {
                            letras += especiales[z - 11] + " ";
                        }
                        else if (z < 100)
                        {
                            letras += decenas[z / 10 - 1] + " ";

                            if ((z % 10) > 0)
                            {
                                letras += "y " + unidades[z % 10] + " ";
                            }
                        }
                        else
                        {
                            letras += centenas[z / 100] + " ";

                            if ((z % 100) >= 10 && (z % 100) <= 19)
                            {
                                letras += especiales[z % 100 - 11] + " ";
                            }
                            else if ((z % 100) >= 20)
                            {
                                letras += decenas[(z % 100) / 10 - 1] + " ";

                                if ((z % 10) > 0)
                                {
                                    letras += "y " + unidades[z % 10] + " ";
                                }
                            }
                        }

                        if (y > 0)
                        {
                            letras += miles[y] + " ";
                        }
                    }

                    letras += "Con " + parteDecimal.ToString("00");
                    
                }
            }

            return letras.Trim();
        }

        public static void RetransmitirTransacciones<T>(T Transacciones, int transaccionID, bool isConfirmado = false) where T : new()
        {
            var props = Transacciones.GetType().GetProperties();

            string query = $@"select * from {Transacciones.GetType().Name}{(isConfirmado? "Confirmados" : " ")}
                           where {props.FirstOrDefault(p => p.Name.Contains("Secuencia")).Name} = {transaccionID}";

            var values = SqliteManager.GetInstance().Query(new TableMapping(Transacciones.GetType()), query).FirstOrDefault();

            var tran = new Hash(Transacciones.GetType().Name);            

            foreach (var prop in props)
            {
                tran.Add(prop.Name, prop.GetValue(values));
            }
            tran.SaveScriptInTransaccionesForServer = true;            
            tran.ExecuteInsert();

            if(Transacciones.GetType().Name.Contains("Recibos"))
            {
                string aplicacion = $"{Transacciones.GetType().Name}Aplicacion";
                string FormaPago = $"{Transacciones.GetType().Name}FormaPago";

                var types = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(t => t.IsClass && t.Name.Contains(aplicacion) && t.Name.Length == aplicacion.Length
                    || t.Name.Contains(FormaPago) && t.Name.Length == FormaPago.Length)
                    .ToList();

                foreach (var type in types)
                {
                    RetransmitirTransaccionesDetalle(Activator.CreateInstance(type), transaccionID, isConfirmado);
                }
            }
            else
            {
                string name = $"{Transacciones.GetType().Name}Detalle";
                string nameLotes = $"{Transacciones.GetType().Name}DetalleLotes";

                var types = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(t => t.IsClass && t.Name.Contains(name) && t.Name.Length == name.Length
                    || t.Name.Contains(nameLotes) && t.Name.Length == nameLotes.Length)
                    .ToList();

                foreach (var type in types)
                {
                    RetransmitirTransaccionesDetalle(Activator.CreateInstance(type), transaccionID, isConfirmado);
                }
            }      
        }
        

        public static void RetransmitirTransaccionesDetalle<T>(T Transacciones, int transaccionID, bool isConfirmado = false) where T : new()
        {
            var props = Transacciones.GetType().GetProperties();

            string query = $@"select * from {Transacciones.GetType().Name}{(isConfirmado? "Confirmados" : " ")}
                           where {props.FirstOrDefault(p => p.Name.Contains("Secuencia")).Name} = {transaccionID}";

            foreach(var value in SqliteManager.GetInstance().Query(new TableMapping(Transacciones.GetType()),query))
            {
                var tran = new Hash(Transacciones.GetType().Name);
                foreach (var prop in props)
                {
                    tran.Add(prop.Name, prop.GetValue(value));
                }
                tran.SaveScriptInTransaccionesForServer = true;
                tran.ExecuteInsert();
            }
        }

        public static void FocusInternal(Entry entry)
        {
            Task.Run(async () =>
            {
                int threshold = 20;

                while (!entry.Focus() && threshold-- > 0)
                {
                    await Task.Delay(50);
                }

                if (entry.Focus())
                {
                    entry.CursorPosition = entry?.Text?.Length ?? 0;
                }
                    
            });
        }

        public static string SignXml(string xml)
        {
            var device = DependencyService.Get<IAppInfo>();

            X509Certificate2 cert = new X509Certificate2(device.ReadCertificate(), "Fragaind20"); //Encoding.UTF8.GetString(Convert.FromBase64String("QFphaWROaXRyYW0xOTczQA==")));

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            // Create a SignedXml object.
            SignedXml signedXml = new SignedXml(xmlDoc);

            // Add the key to the SignedXml document.
            signedXml.SigningKey = cert.PrivateKey;

            // Create a reference to be signed.
            Reference reference = new Reference();
            reference.Uri = "";

            // Add an enveloped transformation to the reference.
            var env = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);

            // Include the public key of the certificate in the assertion.
            signedXml.KeyInfo = new KeyInfo();
            signedXml.KeyInfo.AddClause(new KeyInfoX509Data(cert/*, X509IncludeOption.WholeChain*/));

            // Add the reference to the SignedXml object.
            signedXml.AddReference(reference);

            // Compute the signature.
            signedXml.ComputeSignature();

            // Get the XML representation of the signature and save
            // it to an XmlElement object.
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            // Append the element to the XML document.
            xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(xmlDigitalSignature, true));

            return xmlDoc.OuterXml;
        }

       /*public  static IEnumerable<string> SplitString(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }*/

        public static List<string> SplitByLength(this string str, int maxLength)
        {
            var list = new List<string>();

            for (int index = 0; index < str.Length; index += maxLength)
            {
                list.Add(str.Substring(index, Math.Min(maxLength, str.Length - index)));
            }

            return list;
        }
    }

}
