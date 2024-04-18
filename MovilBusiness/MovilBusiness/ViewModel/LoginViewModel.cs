
using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Services;
using MovilBusiness.Utils;
using MovilBusiness.views;
using MovilBusiness.Views.Components.Dialogs;
using MovilBusiness.Views;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using MovilBusiness.Views.Components.Modals;
using MovilBusiness.Model.Internal;
using Newtonsoft.Json;
using Plugin.Fingerprint;
using System.Globalization;
using MovilBusiness.Resx;

namespace MovilBusiness.viewmodel
{
    public class LoginViewModel : BaseViewModel
    {
        public ObservableCollection<model.Internal.MenuItem> MenuList { get; private set; }

        private bool btnhuellasenabled = true;
        public bool BtnHuellasEnabled { get => btnhuellasenabled; set { btnhuellasenabled = value; RaiseOnPropertyChanged(); } }

        private string username;
        private string password;
        public string UserName { get { return username; } set { username = value; RaiseOnPropertyChanged(); } }
        public string Password { get { return password; } set { password = value; RaiseOnPropertyChanged(); } }
        public ICommand LogInCommand { get; private set; }
        public ICommand AttempHuellaLoginCommand { get; private set; }

        public Action<int> OnAutorizacionUsed;
        private DS_SuscriptoresCambios suscriptoresCambios;

        public LoginViewModel(Page page) : base(page)
        {
            LogInCommand = new Command(AttempLogin);
            AttempHuellaLoginCommand = new Command(AttempHuellaLogin);

            LoadLanguage();

            suscriptoresCambios = new DS_SuscriptoresCambios();

            BtnHuellasEnabled = DS_RepresentantesParametros.GetInstance().GetParHuellaForLogin();

            BindMenu();            

            /*try
            {
                var screen = DependencyService.Get<IScreen>();

                if (screen != null)
                {
                    screen.KeepLightsOn(true);
                }

                string orientation = new PreferenceManager().GetOrientation();

                if (orientation != null)
                {
                    switch (orientation)
                    {
                        case "Portrait":
                            screen.ChangeDeviceOrientation(Enums.ScreenOrientation.PORTRAIT);
                            break;
                        case "Landscape":
                            screen.ChangeDeviceOrientation(Enums.ScreenOrientation.LANDSCAPE);
                            break;
                    }
                }
            }
            catch (Exception) { }*/

        }

        private void LoadLanguage()
        {
            try
            {
                var language = new PreferenceManager().GetLanguage();

                if (!string.IsNullOrWhiteSpace(language))
                {
                    var info = new CultureInfo(language);

                    if (info != null)
                    {
                        AppResource.Culture = info;

                        DependencyService.Get<IScreen>().ChangeLanguage(language);
                    }
                }
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }
        }

        private void BindMenu()
        {

            MenuList = new ObservableCollection<model.Internal.MenuItem>
            {
                new model.Internal.MenuItem() { Title = AppResource.SetInitialLoadUrl, Id = -1, Icon = "ic_sync_black_24dp" },
                new model.Internal.MenuItem() { Title = AppResource.InitialLoad, Id = 0, Icon = "ic_cloud_download_black_24dp" },
                new model.Internal.MenuItem() { Title = AppResource.Version, Id = 1, Icon = "ic_turned_in_not_black_24dp" },
                new model.Internal.MenuItem() { Title = AppResource.CheckForUpdate, Id = 2, Icon = "ic_system_update_black_24dp"},
                new model.Internal.MenuItem() { Title = AppResource.Setting, Id = 3, SeparatorVisible = true },
                new model.Internal.MenuItem() { Title = AppResource.General, Id = 4, Icon = "ic_build_black_24dp" },
                new model.Internal.MenuItem() { Title = AppResource.Connection, Id = 5, Icon = "ic_settings_remote_black_24dp" }
            };

            MenuList.Add(new model.Internal.MenuItem() { Title = AppResource.ChangeLanguage, Id = 6, Icon = "outline_language_black_24" });
        }

        public async void OnOptionMenuItemSelected(model.Internal.MenuItem Item)
        {
            try
            {
                if (Item == null || IsBusy)
                {
                    return;
                }

                IsBusy = true;

                switch (Item.Id)
                {
                    case -1: //configurar url
                        ShowAlertConfigUrl();
                        break;
                    case 1: //version
                        await DisplayAlert(AppResource.Version, AppResource.AppVersionMessage + Functions.AppVersion);
                        //await PushAsync(new LiquidacionEntregasPage());
                        break;
                    case 0://carga inicial
                        ShowAlertCargaInicial();
                        break;
                    case 2:
                        await PushAsync(new UpdaterPage());
                        break;
                    /*case 6: //impresora
                        await PushAsync(new ImpresorasPage());
                        break;*/
                    case 5: //conexion
                        await PushAsync(new ConfigurationPage(true));
                        break;
                    case 4:
                        var alert = DependencyService.Get<IDialogInput>();
                        alert.Show(AppResource.Security, AppResource.EnterPasswordToContinue, (s) => 
                        {
                            if (string.IsNullOrWhiteSpace(s))
                            {
                                DisplayAlert(AppResource.Warning, AppResource.PasswordNotEmptyWarning);
                                return;
                            }

                            if(s.Trim().ToLower() != "mdsoft09052008" && s.Trim().ToLower() != "mdg101273")
                            {
                                DisplayAlert(AppResource.Warning, AppResource.IncorrectPassword);
                                return;
                            }

                            PushAsync(new ConfigurationPage());

                        }, Keyboard.Default, isPassword: true);
                        break;
                    case 6: //cambiar lenguaje
                        ChangeLanguage();
                        break;
                }
            }catch(Exception e)
            {
                await DisplayAlert("Aviso", e.Message);
            }

            IsBusy = false;
        }

        private async void ShowAlertCargaInicial()
        {
            bool debug = System.Diagnostics.Debugger.IsAttached || Functions.IsSincronizacionTest();

            var cambiosPendientes = !Functions.IsSincronizacionTest()? suscriptoresCambios.GetAll(tablesToExclude: new string[] { "ReplicacionesSuscriptoresSincronizaciones", "RepresentantesParametros" }).Count : -1;

            if (cambiosPendientes > 0 && !debug)
            {
                var Result = await DisplayAlert(AppResource.Warning, AppResource.PendingToSyncWarningMessage, AppResource.AuthorizeInitialLoad, AppResource.Aceptar);
                if (Result)
                {
                    await PushModalAsync(new AutorizacionesModal(false, -1, 49, "", true)
                    {
                        OnAutorizacionUsed = (autSec) =>
                        {
                            DependencyService.Get<IDialogCargaInicial>().Show(AttempCargaInicial);
                        }
                    });
                }
            }
            else
            {
                IDialogCargaInicial dialog = DependencyService.Get<IDialogCargaInicial>();
                dialog.Show(AttempCargaInicial);
            }
        }

        private void ShowAlertConfigUrl()
        {
            if (!Functions.IsConnectingToInternet)
            {
                DisplayAlert(AppResource.Warning, AppResource.NoInternetMessage);
                return;
            }

            var dialog = DependencyService.Get<IDialogInput>();

            dialog.Show(AppResource.SetUrl, AppResource.EnterSecretKey, async (key)=> 
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        throw new Exception(AppResource.SpecifyKeyWarning);
                    }

                    IsBusy = true;

                    var pref = new PreferenceManager();

                    string[] arrayofurl = (await ApiManager.GetClientesUrl(key)).Split(','); //new string[] { "http://movilbusiness.com.do/eMovilBusiness/MovilBusinessApi2020/MDSOFT/api/ReplicacionesSuscriptores/", "mdg101273" };// 

                    var url = arrayofurl[0];

                    var version = await ApiManager.GetVersion(key);
                    Application.Current.Resources["VersionReplicacion"] = version;

                    var conn = new ConnectionInfo();
                    conn.Url = url;
                    conn.Descripcion = "Secundaria";
                    conn.Key = "[2]";
                    conn.Version = version;


                    if (arrayofurl[1] == "2")
                        await pref.Put("KeyUrl", key);
                    else
                        pref.Remove("KeyUrl");

                    SqliteManager.GetInstance(true);

                    await pref.SaveConnection(JsonConvert.SerializeObject(conn));
                    await pref.SaveMainStandardUrl(url);
                    await pref.SetVersion(version);
                    await pref.SetClientUrlSetted();

                    ApiManager.Close();

                    var result = await DisplayAlert(AppResource.Success, AppResource.UrlSettedMessage, AppResource.Yes, AppResource.No);

                    if (result)
                    {
                        ShowAlertCargaInicial();
                    }

                }catch(Exception e)
                {
                    await DisplayAlert(AppResource.Warning, e.Message);
                }

                IsBusy = false;
            }, 
            Keyboard.Default, isPassword:false);
        }

        private async void AttempCargaInicial(string user, string pass, string key)
        {
            if (!Functions.IsConnectingToInternet)
            {
                await DisplayAlert(AppResource.Warning, AppResource.NoInternetMessage);
                return;
            }

            var raw = Application.Current.Resources["CargaInicialStandarizada"].ToString();

            if (!string.IsNullOrWhiteSpace(raw) && raw == "1" && !new PreferenceManager().IsClientUrlConfig())
            {
                await DisplayAlert(AppResource.Warning, AppResource.UrlNotSetMessage);
                return;
            }

            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                if (string.IsNullOrEmpty(user))
                {
                    throw new Exception(AppResource.UserNotEmptyMessage);
                }

                if (string.IsNullOrEmpty(pass))
                {
                    throw new Exception(AppResource.PasswordNotEmptyWarning);
                }

                if (string.IsNullOrEmpty(key))
                {
                    throw new Exception(AppResource.SubcriberKeyNotEmptyMessage);
                }

                var args = await new ServicesManager().VerificarSuscriptor(user, pass, key);

                await PushAsync(new ModalCargainicial(args));
                
               // await Application.Current.MainPage.Navigation.PushAsync(new HomePage());

            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
        }

        private async void AttempLogin()
        {
            try
            {
                if (IsBusy)
                {
                    return;
                }

                IsBusy = true;

                if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password))
                {
                    throw new Exception(AppResource.UserAndPasswordNotEmptyMessage);
                }

                if(string.IsNullOrWhiteSpace(new PreferenceManager().GetConnection().Url))
                {
                    throw new Exception(AppResource.MustConfigureConnectionUrl);
                }

                string repCodigoSupervisor = myParametro.GetSupervisorAvailableForLogin();

                if(!string.IsNullOrWhiteSpace(repCodigoSupervisor) && repCodigoSupervisor.Trim() != username.Trim())
                {
                    throw new Exception(AppResource.SupervisorUserMessage);
                }

                await Task.Run(() => 
                {
                    Arguments.CurrentUser = DS_Representantes.LogIn(username, password, myParametro.GetParPermitLoginForAud());
                }); 

                if (Arguments.CurrentUser == null)
                {
                    throw new Exception(AppResource.IncorrectUserOrPassword);
                }

                //if (DS_Representantes.RepresentateIsInactive(Arguments.CurrentUser.RepCodigo))
                //{
                //    await DisplayAlert(AppResource.Warning, AppResource.UserIsInactive, AppResource.Aceptar);
                //    return;
                //}

                if (!myParametro.GetParPermitLoginForAud() && Arguments.CurrentUser.RepCargo.ToUpper() == "AUDITOR")
                {
                    throw new Exception(AppResource.CannotLoginUserMessage);
                }

                UserName = "";
                Password = "";

                new DS_SuscriptorCambiosFallidos().Delete();

                await PushAsync(new HomePage());
            }
            catch (Exception e)
            {
                //await Properties.Page.DisplayAlert("Error!", e.Message, "Aceptar");
                await DisplayAlert(AppResource.Error, e.Message, AppResource.Aceptar);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async void AttempHuellaLogin()
        {
            if (BtnHuellasEnabled)
            {
                var result = await CrossFingerprint.Current.IsAvailableAsync(true);

                if (result)
                {
                    var auth = await CrossFingerprint.Current.AuthenticateAsync(AppResource.TouchTheSensor);
                    if (auth.Authenticated)
                    {
                        IsBusy = true;
                        Arguments.CurrentUser = DS_Representantes.LogInForHuella();
                        await PushAsync(new HomePage());
                        IsBusy = false;
                    }
                }
            }
        }

        private async void ChangeLanguage()
        {
            var language = await DisplayActionSheet(AppResource.ChangeLanguage, AppResource.Cancel, new string[] { "Español", "Ingles" });

            CultureInfo info = null;

            switch (language)
            {
                case "Español":
                    info = new CultureInfo("es-ES");
                    break;
                case "Ingles":
                    info = new CultureInfo("en-US");
                    break;
            }

            if(info == null)
            {
                return;
            }

            await new PreferenceManager().SaveLanguage(info.Name);

            Application.Current.MainPage = new NavigationPage(new LoginPage())
            {
                BarBackgroundColor = Color.FromHex("#1976D2"),
                BarTextColor = Color.White
            };
        }
    }
}
