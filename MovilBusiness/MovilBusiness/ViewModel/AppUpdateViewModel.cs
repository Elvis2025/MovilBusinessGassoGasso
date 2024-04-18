using MovilBusiness.Abstraction;
using MovilBusiness.Model.Internal.Structs.services;
using MovilBusiness.Services;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using Newtonsoft.Json;
using Plugin.Connectivity;
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class AppUpdateViewModel : BaseViewModel
    {
        private bool isupdateavailable;
        public bool IsUpdateAvailable { get => isupdateavailable; set { isupdateavailable = value; RaiseOnPropertyChanged(); } }

        private double currentprogress = 0;
        public double CurrentProgress { get => currentprogress; set { currentprogress = value; RaiseOnPropertyChanged(); } }

        private bool isdownloading;
        public bool IsDownloading { get => isdownloading; set { isdownloading = value; RaiseOnPropertyChanged(); } }

        public CancellationTokenSource cancelToken { get; private set; }

        public AppUpdateViewModel(Page page) : base(page)
        {
            this.cancelToken = new CancellationTokenSource();
            IsBusy = true;
        }

        public async void CheckForUpdates()
        {
            IsBusy = true;
            //bool finish = false;
            try
            {
                if (!CrossConnectivity.Current.IsConnected)
                {
                    await DisplayAlert("Aviso", "No estas conectado a internet");
                    await PopAsync(true);
                    return;
                }

                UpdateInfo result = null;               

                int.TryParse(Functions.AppVersion.Replace(".", ""), out int currentVersion);

                await Task.Run(() => 
                {
                    var client = new WebClient() { Encoding = Encoding.UTF8 };

                    var raw = Application.Current.Resources["updateUrl"].ToString();

                    var pref = new MovilBusiness.Configuration.PreferenceManager();

                    var api = ApiManager.GetInstance(pref.GetConnection().Url);
                    Enums.VersionReplicacion versionReplicacion = pref.GetVersionReplicacion();
                    string urlBase = pref.GetConnection().Url;

                    /*  if (versionReplicacion == Enums.VersionReplicacion.V7)
                      {

                          string URL = urlBase.Replace("api/ReplicacionesSuscriptores/", "");
                          raw = URL + "update/autoupdate_info.txt";
                      }*/
                    //var urlBase = api.

                    string URL = urlBase.Replace("api/ReplicacionesSuscriptores/", "");
                    raw = URL + "update/autoupdate_info.txt";

                    try
                    {
                        result = GetUpdateInfo(client.DownloadString(raw));

                    }
                    catch(Exception)
                    {
                        throw new Exception("No hay ninguna actualización disponible");
                    }

                    if(result == null)
                    {
                        throw new Exception("No hay ninguna actualización disponible");
                    }

                    var downloader = DependencyService.Get<IApplicationUpgrader>();

                    int.TryParse(result.versionName.Replace(".",""), out int serverVersion);

                    if (serverVersion <= currentVersion || downloader == null)
                    {
                        throw new Exception("No hay ninguna actualización disponible");
                    }

                    CurrentProgress = 0;
                    IsBusy = false;
                    IsDownloading = true;
                    downloader.DownloadFile(result.downloadURL, result.fileName, (p)=> 
                    {
                        CurrentProgress = p;
                    }, cancelToken);

                });

            }
            catch(Exception e)
            {
                await DisplayAlert("Aviso", e.Message);
                await PopAsync(true);
            }

            IsDownloading = false;
            IsBusy = false;
        }

        private UpdateInfo GetUpdateInfo(string result)
        {
            try
            {
                return JsonConvert.DeserializeObject<UpdateInfo>(result);

            }catch(Exception e)
            {
                Console.Write(e.Message);
                return null;
            }

        }
    }
}
