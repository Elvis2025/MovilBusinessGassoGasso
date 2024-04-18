using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.Model;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using MovilBusiness.Views;
using Plugin.Geolocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class SacdViewModel : BaseViewModel
    {

        private List<Provincias> provincias;
        public List<Provincias> Provincias { get => provincias; set { provincias = value; RaiseOnPropertyChanged(); } }

        private List<Municipios> municipios;
        public List<Municipios> Municipios { get => municipios; set { municipios = value; RaiseOnPropertyChanged(); } }

        private Provincias currentprovincia;
        public Provincias CurrentProvincia { get => currentprovincia; set { currentprovincia = value; CargarMunicipios(); RaiseOnPropertyChanged(); } }

        private Municipios currentmunicipio;
        public Municipios CurrentMunicipio { get => currentmunicipio; set { currentmunicipio = value; CargarSectores(); RaiseOnPropertyChanged(); } }

        private List<SectoresMunicipios> sectores;
        public List<SectoresMunicipios> Sectores { get => sectores; set { sectores = value; RaiseOnPropertyChanged(); } }

        private SectoresMunicipios currentmunsector;
        public SectoresMunicipios CurrentMunSector { get => currentmunsector; set { currentmunsector = value; RaiseOnPropertyChanged(); } }

        public List<ClientesDirecciones> Direcciones { get; set; }

        private ClientesDirecciones currentdireccion;
        public ClientesDirecciones CurrentDireccion { get => currentdireccion; set { currentdireccion = value; OnCurrentDireccionChanged();  RaiseOnPropertyChanged(); } }

        private bool listeninggps;
        public bool ListeningGPS { get => listeninggps; set { listeninggps = value; RaiseOnPropertyChanged(); } }

        private double clilatitud;
        public double CliLatitud { get => clilatitud; set { clilatitud = value; RaiseOnPropertyChanged(); } }

        private double clilontitud;
        public double CliLongitud { get => clilontitud; set { clilontitud = value; RaiseOnPropertyChanged(); } }

        private string telefono;
        public string Telefono { get => telefono; set { telefono = value; RaiseOnPropertyChanged(); } }

        private string whatsapp;
        public string Whatsapp { get => whatsapp; set { whatsapp = value; RaiseOnPropertyChanged(); } }

        private string calle;
        public string Calle { get => calle; set { calle = value; RaiseOnPropertyChanged(); } }

        private string casa;
        public string Casa { get => casa; set { casa = value; RaiseOnPropertyChanged(); } }

        private string sector;
        public string Sector { get => sector; set { sector = value; RaiseOnPropertyChanged(); } }

        private string contacto;
        public string Contacto { get => contacto; set { contacto = value; RaiseOnPropertyChanged(); } }

        public ICommand GeoRefreshCommand { get; private set; }
        public ICommand OpenMapCommand { get; private set; }


        private DS_Municipios myMun;

        private bool pargps;
        public bool ParGPS { get => pargps; set { pargps = value; RaiseOnPropertyChanged(); } }
        
        public SacdViewModel(Page page) : base(page)
        {
            SaveCommand = new Command(() =>
            {
                SaveSacd();

            }, () => IsUp);

            myMun = new DS_Municipios();

            GeoRefreshCommand = new Command(RefreshGeoReference);
            OpenMapCommand = new Command(OpenMap);

            var myProv = new DS_Provincias();
            Provincias = myProv.GetProvincias();

            Direcciones = new DS_Clientes().getDirPedCli(Arguments.Values.CurrentClient.CliID);

            ParGPS = myParametro.GetParGPS();

            if (ParGPS && CrossGeolocator.Current.IsGeolocationEnabled)
            {
                if(Arguments.Values.CurrentLocation != null)
                {
                    CliLatitud = Arguments.Values.CurrentLocation.Latitude;
                    CliLongitud = Arguments.Values.CurrentLocation.Longitude;
                }

                RefreshGeoReference();
            }          
            
        }

        private void OnCurrentDireccionChanged()
        {
            if(CurrentDireccion == null)
            {
                ClearValues();
                return;
            }

            try
            {

                CurrentProvincia = Provincias.Where(x => x.ProID == CurrentDireccion.ProID).FirstOrDefault();

                CurrentMunicipio = Municipios.Where(x => x.MunID == CurrentDireccion.MunID).FirstOrDefault();

                CurrentMunSector = Sectores.Where(x => x.SecCodigo == CurrentDireccion.CldSector).FirstOrDefault();


                Calle = CurrentDireccion.CldCalle;
                Casa = CurrentDireccion.CldCasa;
                Contacto = CurrentDireccion.CldContacto;
                Telefono = CurrentDireccion.CldTelefono;

                if (ParGPS && CrossGeolocator.Current.IsGeolocationEnabled)
                {
                    CliLatitud = CurrentDireccion.CldLatitud;
                    CliLongitud = CurrentDireccion.CldLongitud;
                }

            }
            catch(Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message);
            }
        }

        private void ClearValues()
        {
            CurrentProvincia = null;
            CurrentMunicipio = null;
            CurrentMunSector = null;
            Calle = "";
            Casa = "";
            Contacto = "";
            Telefono = "";
            Whatsapp = "";           
        }

        private void CargarMunicipios()
        {
            if(CurrentProvincia == null)
            {
                Municipios = new List<Municipios>();
                return;
            }


            Municipios = myMun.GetMunicipiosByProvincia(CurrentProvincia.ProID);

            CurrentMunicipio = Municipios.Where(x => x.MunID == Arguments.Values.CurrentClient.MunID).FirstOrDefault();
        }

        private void CargarSectores()
        {
            if(CurrentMunicipio == null)
            {
                Sectores = new List<SectoresMunicipios>();
                return;
            }

            Sectores = myMun.GetSectoresMunicipios(CurrentMunicipio.MunID);

            if (!string.IsNullOrWhiteSpace(Arguments.Values.CurrentClient.cliSector))
            {
                CurrentMunSector = Sectores.Where(x => x.SecCodigo == Arguments.Values.CurrentClient.cliSector).FirstOrDefault();
            }
        }

        private async void SaveSacd()
        {
            if (IsBusy)
            {
                return;
            }

            try
            {

                if(CurrentDireccion == null)
                {
                    throw new Exception(AppResource.MustSelectAddressToUpdate);
                }

                var data = new SolicitudActualizacionClienteDireccion();
                data.PaiID = 1;
                data.CldDirTipo = CurrentDireccion.CldDirTipo;
                data.CldCalle = Calle;
                data.CldCasa = Casa;
                data.CldContacto = Contacto;
                data.ProID = CurrentProvincia != null ? CurrentProvincia.ProID : -1;
                data.MunID = CurrentMunicipio != null ? CurrentMunicipio.MunID : -1;
                data.CldSector = CurrentMunSector != null ? CurrentMunSector.SecCodigo : null;
                data.CldTelefono = Telefono;
                data.CldWhatsapp = Whatsapp;

                if (ParGPS && Arguments.Values.CurrentLocation != null)
                {
                    data.CliLatitud = CliLatitud;
                    data.CliLongitud = CliLongitud;
                }

                IsBusy = true;

                var task = new TaskLoader() { SqlTransactionWhenRun = true };

                await task.Execute(() => { new DS_SolicitudActualizacionClienteDireccion().GuardarSolicitud(data); });

                await DisplayAlert(AppResource.Success, AppResource.ApplicationSavedSuccessfully);

                await PopAsync(false);

            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        private async void RefreshGeoReference()
        {
            if (!CrossGeolocator.Current.IsGeolocationEnabled)
            {
                await DisplayAlert(AppResource.Warning, AppResource.GpsOffMessage, AppResource.Aceptar);
                return;
            }

            ListeningGPS = false;

            await Functions.StopListeningForLocations();

            ListeningGPS = true;

            Functions.StartListeningForLocations(async (latitud, longitud)=> 
            {
                CliLatitud = latitud;
                CliLongitud = longitud;

                ListeningGPS = false;

                await Functions.StopListeningForLocations();
            });

        }

        private void OpenMap()
        {
            if (CliLatitud != 0 && CliLongitud != 0)
            {
                PushAsync(new MapsPage(CliLatitud, CliLongitud, AppResource.CurrentLocation));
            }

        }
    }
}
