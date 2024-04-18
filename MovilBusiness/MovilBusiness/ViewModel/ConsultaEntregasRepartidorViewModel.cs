using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class ConsultaEntregasRepartidorViewModel : BaseViewModel
    {
        private ObservableCollection<EntregasRepartidorTransacciones> facturas;
        public ICommand SearchCommand { get; private set; }
        public string BtnSearchLogo { get => CurrentFilter != null && CurrentFilter.FilTipo == 3 ? "ic_close_white" : CurrentFilter != null && CurrentFilter.FilTipo == 1 && CurrentFilter.IsCodigoBarra ? "ic_photo_camera_black_24dp" : "ic_search_black_24dp"; set { RaiseOnPropertyChanged(); } }

        private string searchvalue;
        public string SearchValue { get { return searchvalue; } set { searchvalue = value; if (DS_RepresentantesParametros.GetInstance().GetBuscarClienteAlEscribir()) { Search(); } RaiseOnPropertyChanged(); } }

        private KV currentsecondfiltro;
        public KV CurrentSecondFiltro { get { return currentsecondfiltro; } set { currentsecondfiltro = value; if (value != null) { Search(); } RaiseOnPropertyChanged(); } }

        private List<KV> secondfiltersource;
        public List<KV> SecondFiltroSource { get { return secondfiltersource; } set { secondfiltersource = value; RaiseOnPropertyChanged(); } }
        public bool ShowSecondFilter { get { return CurrentFilter != null && CurrentFilter.FilTipo == 2; } set { RaiseOnPropertyChanged(); } }

        private FiltrosDinamicos currentfilter;
        public FiltrosDinamicos CurrentFilter { get => currentfilter; set { currentfilter = value; OnFilterValueSelected(); RaiseOnPropertyChanged(); } }
        public List<FiltrosDinamicos> FiltrosSource { get; private set; }
        public ObservableCollection<EntregasRepartidorTransacciones> Facturas { get => facturas; set { facturas = value; RaiseOnPropertyChanged(); } }

        private string entregatotallabel = "Total: 0.00";
        public string EntregaTotalLabel { get => entregatotallabel; set { entregatotallabel = value; RaiseOnPropertyChanged(); } }

        private int cantidadFacturas ;
        public int CantidadFacturas { get => cantidadFacturas; set { cantidadFacturas = value; RaiseOnPropertyChanged(); } }
        
        public EntregasRepartidor CurrentEntrega { get; set; }
        private DS_EntregasRepartidorTransacciones myEnt;

        private bool IsConfirmado;

        public ConsultaEntregasRepartidorViewModel(Page page, int enrSecuencia, bool isConfirmado) : base(page)
        {
            IsConfirmado = isConfirmado;
            SearchCommand = new Command(() => { Search(); });
            myEnt = new DS_EntregasRepartidorTransacciones();

            CurrentEntrega = myEnt.GetEntregaRepartidor(enrSecuencia, isConfirmado);

            BindFiltrosClientes();
        }

        public async void CargarFacturas(ClientesArgs args)
        {
            IsBusy = true;

            try
            {
                string whereCondition = "";
                if (args.filter != null)
                {
                    if (args.filter != null)
                    {
                        whereCondition = Functions.DinamicFiltersGenerateScript(args.filter, args.SearchValue, args.secondFilter);
                    }
                }

                await Task.Run(() => 
                {
                    Facturas = new ObservableCollection<EntregasRepartidorTransacciones>(myEnt.GetEntregasRepartidorTransaccionesBySecuencia(CurrentEntrega.EnrSecuencia, IsConfirmado, whereCondition));
                });

                var amount = Facturas.Sum(x => x.EntMontoTotal).ToString("N2");
                EntregaTotalLabel = "Total: " + amount;
                CantidadFacturas = Facturas.Count;

                var factSectores = Facturas.Where(x => !string.IsNullOrEmpty(x.SecDescripcion)).GroupBy(x => x.SecCodigo).ToList();

                if (factSectores != null && factSectores.Count > 0)
                {
                    EntregaTotalLabel = "";

                    foreach (var fact in factSectores)
                    {
                        var sector = fact.Where(x => !string.IsNullOrEmpty(x.SecDescripcion)).Select(x => x.SecDescripcion).FirstOrDefault();
                        var monto = fact.Sum(x => x.EntMontoTotal).ToString("N2");
                        var label = sector + " Total: " + monto;
                        EntregaTotalLabel += (string.IsNullOrEmpty(EntregaTotalLabel) ? label : Environment.NewLine + label);
                    }

                    if (string.IsNullOrEmpty(EntregaTotalLabel))
                    {
                        var monto = Facturas.Sum(x => x.EntMontoTotal).ToString("N2");
                        EntregaTotalLabel = "Total: " + monto;
                    }
                }

            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.ErrorLoadingInvoices, e.Message);
            }

            IsBusy = false;
        }

        public async void CargarFacturas()
        {
            IsBusy = true;

            try
            {
                await Task.Run(() => 
                {
                    Facturas = new ObservableCollection<EntregasRepartidorTransacciones>(myEnt.GetEntregasRepartidorTransaccionesBySecuencia(CurrentEntrega.EnrSecuencia, IsConfirmado));
                });

                var amount = Facturas.Sum(x => x.EntMontoTotal).ToString("N2");
                EntregaTotalLabel = "Total: " + amount;
                CantidadFacturas = Facturas.Count;

                var factSectores = Facturas.Where(x => !string.IsNullOrEmpty(x.SecDescripcion)).GroupBy(x => x.SecCodigo).ToList();

                if (factSectores != null && factSectores.Count > 0)
                {
                    EntregaTotalLabel = "";

                    foreach (var fact in factSectores)
                    {
                        var sector = fact.Where(x => !string.IsNullOrEmpty(x.SecDescripcion)).Select(x => x.SecDescripcion).FirstOrDefault();
                        var monto = fact.Sum(x => x.EntMontoTotal).ToString("N2");
                        var label = sector + " Total: " + monto;
                        EntregaTotalLabel += (string.IsNullOrEmpty(EntregaTotalLabel) ? label : Environment.NewLine + label);
                    }

                    if (string.IsNullOrEmpty(EntregaTotalLabel))
                    {
                        var monto = Facturas.Sum(x => x.EntMontoTotal).ToString("N2");
                        EntregaTotalLabel = "Total: " + monto;
                    }
                }

            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.ErrorLoadingInvoices, e.Message);
            }

            IsBusy = false;
        }
        public void VerDetalleEntrega(EntregasRepartidorTransacciones entrega)
        {
            try
            {
                myEnt.DeleteFromTemp();

                myEnt.InsertProductInTemp(entrega.EnrSecuencia, entrega.TraSecuencia, entrega.TitID, entrega.CliID, true, true);

                Arguments.Values.CurrentModule = Modules.ENTREGASREPARTIDOR;

                PushAsync(new EntregasRepartidorDetalleRevisionPage(entrega, true, true));

            }
            catch (Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message);
            }
        }
        private void BindFiltrosClientes()
        {
            FiltrosSource = new DS_FiltrosDinamicos().GetFiltrosClientes();

            if (FiltrosSource != null && FiltrosSource.Count > 0)
            {
                CurrentFilter = FiltrosSource.Where(x => x.FilIndicadorDefault).FirstOrDefault();

                if (CurrentFilter == null)
                {
                    CurrentFilter = FiltrosSource.FirstOrDefault();
                }
            }
        }
        public void SelectDefaultFilter()
        {
            if (FiltrosSource == null)
            {
                CurrentFilter = null;
                return;
            }

            if (myParametro.GetParBusquedaCombinadaPorDefault())
                CurrentFilter = FiltrosSource.Where(x => x.FilDescripcion == "Combinada").FirstOrDefault();
            else
                CurrentFilter = FiltrosSource.Where(x => x.FilIndicadorDefault).FirstOrDefault();

            if (CurrentFilter == null)
            {
                CurrentFilter = FiltrosSource.FirstOrDefault();
            }
        }
        private void OnFilterValueSelected()
        {
            try
            {
                if (CurrentFilter != null && CurrentFilter.FilTipo == 2)
                {
                    ShowSecondFilter = true;
                    SecondFiltroSource = Functions.DinamicQuery(CurrentFilter.FilComboSelect);
                    if (SecondFiltroSource != null && SecondFiltroSource.Count > 0)
                    {
                        CurrentSecondFiltro = SecondFiltroSource[0];
                    }
                }
                else
                {
                    CurrentSecondFiltro = null;
                    SecondFiltroSource = new List<KV>();
                    CurrentSecondFiltro = null;
                    ShowSecondFilter = false;

                    BtnSearchLogo = "";
                }
            }
            catch (Exception e)
            {
                DisplayAlert(AppResource.ErrorLoadingFilters, e.Message);
            }
        }

        public async void Search(bool showAlert = true)
        {
            try
            {
                
              if (IsBusy)
              {
                  return;
              }

              IsBusy = true;

              var repcodigo = Arguments.CurrentUser.RepCodigo;

              var args = new ClientesArgs() { filter = CurrentFilter, secondFilter = CurrentSecondFiltro != null ? CurrentSecondFiltro.Key : "", SearchValue = SearchValue, RepCodigo = repcodigo };

              await Task.Run(() => { CargarFacturas(args); });
            }
            catch (Exception e)
            {
                IsBusy = false;
                await DisplayAlert(AppResource.ErrorLoadingClients, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;

        }

    }
}
