using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.model.Internal.Structs.Args;
using MovilBusiness.Model;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using MovilBusiness.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class AsignacionRutasViewModel : BaseViewModel
    {
        private DS_RutaVisitas myRut;

        private ObservableCollection<RutaVisitasFecha> clientes;
        public ObservableCollection<RutaVisitasFecha> Clientes { get => clientes; set { clientes = value; RaiseOnPropertyChanged(); } }

        private DateTime currentdate = DateTime.Now.AddDays(1);
        public DateTime CurrentDate
        {   get => currentdate;
            set
            {
                if (!DateValidForLimit(value))
                {
                    DisplayAlert(AppResource.Warning, AppResource.DateExceedLimitOfDays + ParLimiteDias);
                    CurrentDate = currentdate;
                    return;
                }
                //lastDateUsed = value;
                currentdate = value; LoadClients(Resume); RaiseOnPropertyChanged();
            }
        }

        private DateTime? maxDatePermitied = null;
        public DateTime MinDate { get => DateTime.Now.AddDays(1); }

        public List<FiltrosDinamicos> FiltrosSource { get; private set; }

        private FiltrosDinamicos currentfilter;
        public FiltrosDinamicos CurrentFilter { get => currentfilter; set { currentfilter = value; OnFilterValueSelected(); RaiseOnPropertyChanged(); } }
        private List<KV> secondfiltersource;
        public List<KV> SecondFiltroSource { get { return secondfiltersource; } set { secondfiltersource = value; RaiseOnPropertyChanged(); } }
        private KV currentsecondfiltro;
        public KV CurrentSecondFiltro { get { return currentsecondfiltro; } set { currentsecondfiltro = value; if (value != null) { LoadClients(Resume); } RaiseOnPropertyChanged(); } }

        public bool ShowSecondFilter { get { return CurrentFilter != null && CurrentFilter.FilTipo == 2; } set { RaiseOnPropertyChanged(); } }

        private string search;
        public string Search { get => search; set { search = value; RaiseOnPropertyChanged(); } }

        private bool asignartodo;
        public bool AsignarTodo { get => asignartodo; set { asignartodo = value; OnAsignarTodoChanged(); RaiseOnPropertyChanged(); } }


        private bool ocultarAsignarTodos;
        public bool OcultarAsignarTodos { get => ocultarAsignarTodos; set { ocultarAsignarTodos = value; OnAsignarTodoChanged(); RaiseOnPropertyChanged(); } }


        public ICommand AsignarTodoCommand { get; private set; }
        public ICommand SaveRouteCommand { get; private set; }
        public ICommand SearchCommand { get; private set; }

        private bool Resume = false;
        private int ParLimiteDias;

        public AsignacionRutasViewModel(Page page) : base(page)
        {
            myRut = new DS_RutaVisitas();

            FiltrosSource = new DS_FiltrosDinamicos().GetFiltrosClientes();

            ParLimiteDias = myParametro.GetParLimiteDiasParaAsignarRuta();

            if(ParLimiteDias > 0)
            {
                maxDatePermitied = DateTime.Now.AddDays(ParLimiteDias + 1); //+1 porque es por encima de hoy, hoy no cuenta
            }

            if (FiltrosSource != null && FiltrosSource.Count > 0)
            {
                var item = FiltrosSource.Where(x => x.FilIndicadorDefault).FirstOrDefault();

                if (item != null)
                {
                    CurrentFilter = item;
                }
            }

            OcultarAsignarTodos = !myParametro.GetParOcultarAsignarTodosAsignacionRuta();

            myRut.ClearTemp();

            SearchCommand = new Command(() => { LoadClients(Resume); });
            AsignarTodoCommand = new Command(()=> { AsignarTodo = !AsignarTodo; });
            SaveRouteCommand = new Command(GuardarAsignacionRutas);

            LoadClients(true);

            Arguments.Values.CliDatosOtros = new DS_UsosMultiples().GetCliCaracteristicas();
        }

        private bool DateValidForLimit(DateTime date)
        {
            if (maxDatePermitied != null)
            {
                if(date > maxDatePermitied)
                {
                    return false;
                }
            }

            return true;
        }

        public void SearchAsigned(bool asigned)
        {
            Resume = asigned;
            LoadClients(asigned);
        }

        private void OnAsignarTodoChanged()
        {
            try
            {
                if (Clientes != null)
                {
                    foreach (var cliente in Clientes.Where(x => x.IsAsignado != AsignarTodo))
                    {
                        AsignarCliente(cliente);
                    }

                    var copy = new ObservableCollection<RutaVisitasFecha>(Clientes);

                    Clientes = copy;

                    RaiseOnPropertyChanged("Clientes"); 
                }

            }catch(Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message);
            }
        }

        private async void LoadClients(bool resumen = false)
        {
            var args = new ClientesArgs() { filter = CurrentFilter, secondFilter = CurrentSecondFiltro != null ? CurrentSecondFiltro.Key : "", SearchValue = Search,  RepCodigo = Arguments.CurrentUser.RepCodigo };

            try
            {
                IsBusy = true;

                await Task.Run(() => { Clientes = new ObservableCollection<RutaVisitasFecha>(myRut.GetClientesSinAsignar(args, CurrentDate, resumen)); });

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        public void AsignarCliente(RutaVisitasFecha data)
        {
            if (data.RutEstado.Trim() != "1")
            {
                DisplayAlert(AppResource.Warning, AppResource.CustomerCannotBeModifyedInCurrentDate);
                return;
            }

            if (data.IsAsignado)
            {
                data.IsAsignado = false;

                myRut.DeleteInTemp(data.CliID, data.RutFecha, data.rowguid, data.RutPosicion);
            }
            else
            {
                if(myRut.HayVisitaProgramada(data.CliID, data.RutFecha))
                {
                    //DisplayAlert(AppResource.Warning, "Ya hay una visita programa para este cliente y este dia");
                    data.IsAsignado = true;
                    return;
                }

                data.IsAsignado = true;
                myRut.InsertInTemp(data.CliID, data.RutFecha);
            }
        }

        public void OnListItemSelected(RutaVisitasFecha data)
        {
            try
            {
                if(data.RutEstado.Trim() != "-1" && data.RutEstado.Trim() != "1")
                {
                    DisplayAlert(AppResource.Warning, AppResource.CannotModifyRecordHasBeenTransmitted);
                    return;
                }

                AsignarCliente(data);

                var newData = data.Copy();

                var index = Clientes.IndexOf(data);

                Clientes[index] = newData;

            }
            catch (Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message);
            }
        }

        private async void GuardarAsignacionRutas()
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;

                //var task = new TaskLoader() { SqlTransactionWhenRun = true };

                //await task.Execute(() => { myRut.GuardarRutaVisitaFromTemp(); });

                var asignados = Clientes.Where(x => x.IsAsignado).ToList();

                foreach (var data in asignados)
                {
                    if (!myRut.ExistsInTemp(data.CliID, data.RutFecha))
                    {
                        myRut.InsertInTemp(data.CliID, data.RutFecha, rowguid: data.rowguid, rutPosicion: data.RutPosicion);
                    }
                }

                var ruta = myRut.GetAsignacionInTemp();

                if(ruta == null || ruta.Count == 0)
                {
                    throw new Exception(AppResource.NotAddedAnyClients);
                }

                await PushAsync(new AsignacionRutasDetallePage());
                //await DisplayAlert("Exito!", "La ruta fue almacenada con éxito, favor de sincronizar!");

                //await PopAsync(true);



            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        private void OnFilterValueSelected()
        {
            if (CurrentFilter.FilTipo == 2)
            {
                ShowSecondFilter = true;
                SecondFiltroSource = Functions.DinamicQuery(CurrentFilter.FilComboSelect);
                // SecondFilterPosition = 0;
                //CurrentSecondFiltro = null;
                if (SecondFiltroSource != null && SecondFiltroSource.Count > 0)
                {
                    CurrentSecondFiltro = SecondFiltroSource[0];
                }
            }
            else
            {
                CurrentSecondFiltro = null;
                SecondFiltroSource = new List<KV>();
                //SecondFilterPosition = -1;
                CurrentSecondFiltro = null;
                ShowSecondFilter = false;

                //BtnSearchLogo = "";
            }
        }
    }
}
