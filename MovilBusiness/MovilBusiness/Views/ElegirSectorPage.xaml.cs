using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.views;
using MovilBusiness.Views.Components.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace MovilBusiness.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ElegirSectorPage : BasePage, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;

        private DS_Sectores mySec;
        private DS_Visitas myVis;

        private Sectores currentsector;
        public Sectores CurrentSector { get => currentsector; set { currentsector = value; Arguments.Values.CurrentSector = value; RaiseOnPropertyChanged(); } }

        private bool IsOpeningVisit;
        public bool IsFromHome { get; set; }
        public static bool ClosingVisit;
        public bool IsFirstUse { get; set; } = true;

        public ElegirSectorPage()
        {
            mySec = new DS_Sectores();
            myVis = new DS_Visitas();
            
            IsOpeningVisit = true;

            BindingContext = this;

            InitializeComponent();

            if (EnableBackButtonOverride)
            {
                CustomBackButtonAction = () => { SalirVisita(); };
            }

            NavigationPage.SetBackButtonTitle(this, AppResource.Operations);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            HandleSectores();
            IsFirstUse = true;
        }

        private void HandleSectores()
        {
            if (IsOpeningVisit)
            {
                IsOpeningVisit = false;
                LoadComponents(false);
            }
            else if (ClosingVisit)
            {
                ClosingVisit = false;
                LoadComponents(true);
            }
        }

        private bool showingAlert = false;
        private async void LoadComponents(bool closing)
        {
            try
            {
                btnSalir.IsVisible = closing;

                var sectores = mySec.GetSectoresByCliente(Arguments.Values.CurrentClient.CliID);

                if (sectores == null || sectores.Count == 0)
                {
                    if (!showingAlert)
                    {
                        showingAlert = true;
                        await DisplayAlert(AppResource.Warning, AppResource.SectorsNotLoadedForClient, AppResource.Aceptar);
                        showingAlert = false;
                        SalirVisita();
                    }
                    return;
                }

                var parSectores = DS_RepresentantesParametros.GetInstance().GetParSectores();

                if (parSectores == 3 && string.IsNullOrWhiteSpace(Arguments.Values.SecCodigoParaCrearVisita))
                {
                    comboSector.ItemsSource = sectores;

                    loader.IsVisible = false;

                    if (DS_RepresentantesParametros.GetInstance().GetParSeleccionarSectorAutomaticamenteSiTieneEntrega())
                    {
                        var next = mySec.GetNextSectorAVisitar(Arguments.Values.CurrentClient.CliID, closing, true);

                        if (next != null)
                        {
                            var item = sectores.Where(x => x.SecCodigo == next.SecCodigo).FirstOrDefault();
                            CurrentSector = item;
                            Arguments.Values.CurrentSector = item;
                            EntrarVisita();
                            return;
                        }
                    }

                }
                else if (parSectores == 4 || !string.IsNullOrEmpty(Arguments.Values.SecCodigoParaCrearVisita))
                {
                    if (sectores.Count == 1 && string.IsNullOrWhiteSpace(Arguments.Values.SecCodigoParaCrearVisita))
                    {
                        if (!closing)
                        {
                            comboSector.ItemsSource = sectores;
                            Arguments.Values.CurrentSector = sectores[0];
                            EntrarVisita();
                            return;
                        }
                        else
                        {
                            SalirVisita();
                            return;
                        }
                    }

                    var sectoresTemp = mySec.GetSectoresByCliente(Arguments.Values.CurrentClient.CliID, "", true);

                    var lastVisited = mySec.GetLastSectorVisitado(Arguments.Values.CurrentClient.CliID, closing);
                    Sectores nextToVisit = null;

                    if (DS_RepresentantesParametros.GetInstance().GetParSeleccionarSectorAutomaticamenteSiTieneEntrega())
                    {
                        nextToVisit = mySec.GetNextSectorAVisitar(Arguments.Values.CurrentClient.CliID, closing, true);
                    }
                    else
                    {
                        nextToVisit = lastVisited != null ? mySec.GetNextSectorAVisitar(Arguments.Values.CurrentClient.CliID, closing) : sectoresTemp[0];
                    }

                    if (nextToVisit != null || lastVisited == null)
                    {
                        if (!closing)
                        {
                            var source = new List<Sectores>() { sectores[0] };

                            if(nextToVisit != null)
                            {
                                source = new List<Sectores>() { nextToVisit };
                            }

                            comboSector.ItemsSource = source;
                            Arguments.Values.CurrentSector = source[0];
                            EntrarVisita();
                        }
                        else
                        {
                            comboSector.ItemsSource = new List<Sectores>() { sectores[0] };
                            CurrentSector = null;
                            loader.IsVisible = false;
                            if (nextToVisit != null)
                            {
                                Arguments.Values.CurrentSector = nextToVisit;
                                EntrarVisita();
                            }
                        }

                        return;
                    }

                    var index = sectores.IndexOf(sectores.Where(x => x.SecCodigo == lastVisited.SecCodigo).FirstOrDefault());

                    var items = new List<Sectores>();

                    for (int i = 0; i <= index; i++)
                    {
                        items.Add(sectores[i]);
                    }

                    if (sectores.Count - 1 > index)
                    {
                        items.Add(sectores[index + 1]);
                    }

                    comboSector.ItemsSource = items;
                    CurrentSector = null;
                    loader.IsVisible = false;
                }
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }
        }

        private void BtnAceptar_Clicked(object sender, EventArgs e)
        {
            if (IsFirstUse)
            {
                IsFirstUse = false;
                EntrarVisita();
            }
        }

        private async void EntrarVisita()
        {
            try
            {
                if (Arguments.Values.CurrentSector == null)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.MustSelectSector, AppResource.Aceptar);
                    return;
                }

                IsBusy = true;

                var task = new TaskLoader() { SqlTransactionWhenRun = true };

                await task.Execute(() => { myVis.CrearVisitaSector(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentSector.SecCodigo, Arguments.Values.CurrentVisSecuencia);});

                await Navigation.PushAsync(new OperacionesPage(false, Arguments.Values.CurrentSector));

            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.ErrorEnteringVisit, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
        }

        private async void SalirVisita()
        {
            try
            {
                if (IsFromHome && !ClosingVisit)
                {
                    var task = new TaskLoader() { SqlTransactionWhenRun = true };
                    await task.Execute(() => { myVis.CerrarVisita(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentVisSecuencia, Arguments.Values.CurrentLocation, "1"); });
                    IsFromHome = false;
                }

                Arguments.Values.CurrentSector = null;
                Arguments.Values.CurrentVisSecuencia = -1;
                Arguments.Values.CurrentClient = null;

                await Navigation.PopAsync(true);

            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Error, e.Message, AppResource.Aceptar);
            }

        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override bool OnBackButtonPressed()
        {
            SalirVisita();
            return true;
        }



        private void ButtonSalirClicked(object sender, EventArgs e)
        {
            SalirVisita();
        }
    }
}