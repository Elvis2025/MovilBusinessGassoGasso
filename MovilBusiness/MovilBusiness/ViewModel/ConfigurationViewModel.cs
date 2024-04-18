using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Model.Internal;
using MovilBusiness.Resx;
using MovilBusiness.viewmodel;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class ConfigurationViewModel : BaseViewModel
    {
        private bool synctest;
        public bool SyncTest { get => synctest; set { synctest = value; pref.SaveTestSync(value); RaiseOnPropertyChanged(); } }

        public string Suscriptor { get => DS_RepresentantesParametros.GetInstance().GetRepSuscriptor(); }

        //private string orientation;
        //public string Orientation { get => orientation; set { orientation = value; RaiseOnPropertyChanged(); } }

        private List<string> Connections;
        private List<ConnectionInfo> FullConnections;

        private PreferenceManager pref;

        public ConfigurationViewModel(Page page) : base(page)
        {
            pref = new PreferenceManager();

            Connections = new List<string>
            {
                "Principal", AppResource.Custom
            };

            FullConnections = new List<ConnectionInfo>()
            {
                new ConnectionInfo(){ Descripcion = "Principal", Key = "[1]", Url =  pref.GetMainStandardUrl() },
                new ConnectionInfo(){ Descripcion = AppResource.Custom, Key = "[2]", Url = pref.GetMainStandardUrl() }
            };

            var customConnections = myParametro.GetParConexiones();

            if(customConnections != null && customConnections.Count > 0)
            {
                FullConnections.AddRange(customConnections);

                foreach(var conn in customConnections)
                {
                    Connections.Add(conn.Descripcion);
                }
            }

            SyncTest = pref.IsSincronizacionTest();

            //Orientation = pref.GetOrientation();
        }

        public async void SeleccionarConexion()
        {
            var parCargainicialStandard = Application.Current.Resources["CargaInicialStandarizada"].ToString();

            /*if (!string.IsNullOrWhiteSpace(parCargainicialStandard) && parCargainicialStandard == "1")
            {
                return;
            }*/

            var result = await DisplayActionSheet(AppResource.SelectConnectionToUse, buttons: Connections.ToArray());

            var item = FullConnections.Where(x => x.Descripcion == result).FirstOrDefault();

            if(item == null)
            {
                return;
            }

            SaveConnection(item);

        }

        public void EditarWebServiceUrl()
        {
            var currentConnection = pref.GetConnection();

            var cargaInicial = Application.Current.Resources["CargaInicialStandarizada"].ToString();
            var parCargaInicialStandard = !string.IsNullOrWhiteSpace(cargaInicial) && cargaInicial == "1";

            if (currentConnection.Key != "[2]")
            {
                if(parCargaInicialStandard && !pref.IsClientUrlConfig())
                {
                    DisplayAlert(AppResource.WebServiceUrl, "");
                }
                else
                {
                    DisplayAlert(AppResource.WebServiceUrl, currentConnection.Url);
                }
                
                return;
            }

            //editar url
            currentConnection = pref.GetConnection();

            if (string.IsNullOrWhiteSpace(currentConnection.Url))
            {
                currentConnection.Url = Application.Current.Resources["Dominio"].ToString();
            }

            IDialogInput alert = DependencyService.Get<IDialogInput>();
            alert.Show(AppResource.Security, AppResource.WebServiceUrl, (s) => { currentConnection.Url = s; SaveConnection(currentConnection); }, Keyboard.Default, currentConnection.Url);
        }

        private async void SaveConnection(ConnectionInfo conn)
        {
            if (conn.Key == "[2]" && string.IsNullOrWhiteSpace(conn.Url))
            {
                conn.Url = Application.Current.Resources["Dominio"].ToString();
            }

            var parCargainicialStandard = Application.Current.Resources["CargaInicialStandarizada"].ToString();

            if (conn.Key == "[1]" && !string.IsNullOrWhiteSpace(parCargainicialStandard) && parCargainicialStandard == "1")
            {
                conn.Url = pref.GetMainStandardUrl();
            }

            await pref.SaveConnection(JsonConvert.SerializeObject(conn));
        }

        public void EditarReplicacion()
        {
            var replicacion = pref.GetReplicacion();

            IDialogInput alert = DependencyService.Get<IDialogInput>();
            alert.Show(AppResource.Security, AppResource.MbReplication, (s) => 
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    DisplayAlert(AppResource.Warning, AppResource.ReplicationNameNotEmpty);
                    return;
                }

                pref.SaveReplicacion(s);

            }, Keyboard.Default, replicacion);
        }

        public void EditarCantidadDatosPorCiclo()
        {
            var datosCiclo = pref.GetCantidadDatosCiclo();

            IDialogInput alert = DependencyService.Get<IDialogInput>();
            alert.Show(AppResource.Security, AppResource.WebServiceUrl, (s) => 
            {
                int.TryParse(s, out int result);

                if(result < 1)
                {
                    DisplayAlert(AppResource.Warning, AppResource.DataMustBeThanZero);
                    return;
                }

                pref.SaveCantidadDatosCiclo(s);

            }, Keyboard.Numeric, datosCiclo > 0 ? datosCiclo.ToString() : "");
        }

        public void EditarVersionDeLaReplicacion()
        {
            var alert = DependencyService.Get<IDialogInput>();

            alert.Show(AppResource.Security, AppResource.EnterSecurityKeyToModify, async (s)=> 
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    await DisplayAlert(AppResource.Warning, AppResource.PasswordNotEmptyWarning);
                    return;
                }

                if(s.Trim().ToLower() != "Mdg101273^")
                {
                    await DisplayAlert(AppResource.Warning, AppResource.IncorrectPassword);
                    return;
                }

                var version = pref.GetVersionReplicacion();

                var name = "";

                //var name = version == Enums.VersionReplicacion.V7 ? "V7" : "Standard";

                switch (version)
                {
                    case Enums.VersionReplicacion.V7:
                        name = "V7";
                        break;

                    case Enums.VersionReplicacion.V8:
                        name = "V8";
                        break;

                    case Enums.VersionReplicacion.V9:
                        name = "V9";
                        break;

                    default:
                        name = "Standard";
                        break;
                }

                var result = await DisplayActionSheet(AppResource.CurrentVersionLabel + name, buttons: new string[] { "Standard", "V7", "V8" });

                if (result == "Standard" || result == "V7" || result == "V8")
                {
                    await pref.SaveVersionReplicacion(result);
                }

            }, Keyboard.Default, isPassword:true);
        }

        /*public async void CambiarOrientacionPantalla()
        {
            var result = await DisplayActionSheet(AppResource.ChangeScreenOrientation, buttons: new string[] { "Portrait", "Landscape" });

            var screen = DependencyService.Get<IScreen>();

            switch (result)
            {
                case "Portrait":
                    screen.ChangeDeviceOrientation(Enums.ScreenOrientation.PORTRAIT);
                    await pref.SetOrientation(result);
                    Orientation = result;
                    break;
                case "Landscape":
                    screen.ChangeDeviceOrientation(Enums.ScreenOrientation.LANDSCAPE);
                    await pref.SetOrientation(result);
                    Orientation = result;
                    break;
            }
        }*/

    }
}
