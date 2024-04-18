using MovilBusiness.Abstraction;
using MovilBusiness.model.Internal;
using MovilBusiness.model.Internal.structs;
using MovilBusiness.Resx;
using MovilBusiness.Services;
using MovilBusiness.views;
using MovilBusiness.Views.Components;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.viewmodel
{
    public class CargaInicialViewModel : BaseViewModel
    {
        private string loadingmessage;     
        public string LoadingMessage { get { return loadingmessage; } set { loadingmessage = value; RaiseOnPropertyChanged(); } }

        private double currentprogress = 0;
        public double CurrentProgress { get => currentprogress; set { currentprogress = value; RaiseOnPropertyChanged(); } }

        private double totalprogress = 0;
        public double TotalProgress { get { return totalprogress; } set { totalprogress = value; RaiseOnPropertyChanged(); } }

        private string detailedprogress = "";
        public string DetailedProgress { get => detailedprogress; set { detailedprogress = value; RaiseOnPropertyChanged(); } }

        private int current;

        private ServicesManager api;

        private readonly CargaInicialArgs args;

        public CargaInicialViewModel(Page page, CargaInicialArgs args) : base(page)
        {
            this.args = args;
            api = new ServicesManager();
            
            api.MessageReport += (sender, message) => { LoadingMessage = message; };
            api.CantidadTotalCambios = args.cantidadCambios;
            api.OnCurrentProgressChanged = (s, d) => 
            {
                CurrentProgress = s;
                current += d;
                DetailedProgress = d > 0 ? current + "/" + args.cantidadCambios : "";
            };
            api.OnTotalProgressChanged = (s) => { TotalProgress = s; };

            IScreen screen = DependencyService.Get<IScreen>();

            if (screen != null)
            {
                screen.KeepLightsOn(true);
            }
        }

        public async void StartCargaInicial()
        {
            try
            {
                /* CurrentProgress = 0;
                 TotalProgress = 0;*/
                CurrentProgress = 0;
                TotalProgress = 0;
                DetailedProgress = "";
                current = 0;

                await api.CargaInicial(args);

                /*Application.Current.MainPage = new NavigationPage(new StartPage())
                {
                    BarBackgroundColor = (Color)Application.Current.Resources["ColorPrimary"],
                    BarTextColor = Color.White
                };*/

               // LoginPage.Finish = true;

                await PopAsync(false);
                //await PopModalAsync(false);
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.InitialLoadError, e.InnerException != null && !string.IsNullOrWhiteSpace(e.InnerException.Message) ? e.InnerException.Message : e.Message, AppResource.Aceptar);
                await PopAsync(true);
               // await PopModalAsync(true);
            }
        }

    }
}
