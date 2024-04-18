
using Microsoft.AppCenter.Crashes;
using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ClientesPage : ContentPage
    {
        private bool FirstTime = true;

		public ClientesPage(bool IsRutaVisita)
        { 
            BindingContext = new ClientesViewModel(this, IsRutaVisita);
          
            InitializeComponent();

            if (IsRutaVisita)
            {
                Title = AppResource.VisitsRoute;

                if (DS_RepresentantesParametros.GetInstance().GetParRutaVisitaTipo() == 2)
                {
                    if (!DS_RepresentantesParametros.GetInstance().GetParRutaVisitaTipo2Mixto())
                    {
                        filterContainer.ColumnDefinitions[0].Width = GridLength.Auto;
                        container.Children.Remove(containerSearch1);
                        //container.Children.Remove(comboSup1);
                        search2.GetComboFiltro().SelectedIndex = 0;
                    }
                    else
                    {
                        container2.Children.Remove(search2);
                        container2.Children.Remove(comboSup2);
                    }
                }
                else
                {
                    container2.Children.Remove(search2);
                    container2.Children.Remove(comboSup2);
                }
            }
            else
            {
                container2.Children.Remove(search2);
                container2.Children.Remove(comboSup2);
            }

            NavigationPage.SetBackButtonTitle(this, AppResource.Back);


            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.Write(e.ToString());
        }

        public void ListItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            if (args.SelectedItem == null)
            {
                return;
            }

            var vm = BindingContext as ClientesViewModel;

            vm?.ShowDialogOpcionesVisita(args.SelectedItem as Clientes);

            clientList.SelectedItem = null;
        }

        private void SetFirstPage(object sender, EventArgs args)
        {
            SetPage(0);
        }
        private void SetSecondPage(object sender, EventArgs args)
        {
            SetPage(1);
        }
        private void SetThirdPage(object sender, EventArgs args)
        {
            SetPage(2);
        }

        private void SetPage(int pos)
        {
            if(BindingContext is ClientesViewModel vm)
            {
                btnTodos.TextColor = Color.FromHex("#BDBDBD");
                btnPendientes.TextColor = Color.FromHex("#BDBDBD");
                btnVisitados.TextColor = Color.FromHex("#BDBDBD");
                todosIndicador.IsVisible = false;
                pendientesIndicador.IsVisible = false;
                visitadosIndicador.IsVisible = false;

                switch (pos)
                {
                    case 0: //todos
                        btnTodos.TextColor = Color.White;
                        todosIndicador.IsVisible = true;
                        vm.FiltroEstatusVisitas = FiltroEstatusVisitaClientes.TODOS;
                        break;
                    case 1: //pendientes
                        btnPendientes.TextColor = Color.White;
                        pendientesIndicador.IsVisible = true;
                        vm.FiltroEstatusVisitas = FiltroEstatusVisitaClientes.PENDIENTES;
                        break;
                    case 2: //visitados
                        btnVisitados.TextColor = Color.White;
                        visitadosIndicador.IsVisible = true;
                        vm.FiltroEstatusVisitas = FiltroEstatusVisitaClientes.VISITADO;
                        break;
                }
                
                vm.ShowOrHideSecondSearch();

                vm.IsFirstTime = false;
            }         

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                try
                {
                    if (DS_RepresentantesParametros.GetInstance().GetParProductoNoVendido() > 0)
                    {
                        if (FirstTime)
                        {
                            ToolbarItems.RemoveAt(2);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                }               

                Functions.StartListeningForLocations();

                if(BindingContext is ClientesViewModel vm)
                {
                    vm.CargarCliDatosOtros();
                    vm.SubscribeToListeners();
                    vm.AplicarCargasAutomaticas();
                    //isScanning

                    if (FirstTime)
                    {
                        FirstTime = false;
                        vm.SelectDefaultFilter();
                        SetPage(1);
                    }
                    else
                    {
                        if (vm.CurrentFilter.IsCodigoBarra)
                        {
                            
                            vm.SearchUnAsync(false, false);
                            FirstTime = true;
                            //vm.SelectDefaultFilter();
                           // SetPage(1);

                        }
                        else
                        {

                            vm.Search(vm.FiltroEstatusVisitas, false);
                            FirstTime = false;
                        }
                    }

                  
                }
                
            }catch(Exception e)
            {
                Crashes.TrackError(e);
                DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }

        }


        protected override void OnDisappearing()
        {
            if (BindingContext is ClientesViewModel vm)
            {
                vm.UnSubscribeFromListeners();
                vm.ReciclarCliDatosOtros();
            }

            base.OnDisappearing();

            StopLocationsUpdates();
        }

        private async void StopLocationsUpdates()
        {
            await Functions.StopListeningForLocations();
        }


        private void StartCall(object sender, EventArgs e)
        {
            if (sender is ContentView con && con.Content is Label lbl)
            {
                MessagingCenter.Send("", "BtnStartCall", lbl.Text);
            }
        }

        private void OnClientLocationClicked(object sender, EventArgs e)
        {
            if(sender is Image img && img.BindingContext != null && img.BindingContext is Clientes data && BindingContext is ClientesViewModel vm)
            {
                vm.OpenClientLocation(data);
            }
        }
    }
}