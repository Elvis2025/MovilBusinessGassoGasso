using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Model;
using MovilBusiness.Printer.Formats;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.Views.Components.Modals;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.viewmodel
{
    public class CargasViewModel : BaseViewModel
    {
        public ICommand ClickHandlerCommand { get; private set; }

        private ObservableCollection<Cargas> cargasdisponibles;
        public ObservableCollection<Cargas> CargasDisponibles { get => cargasdisponibles; private set { cargasdisponibles = value; RaiseOnPropertyChanged(); } }

        private Cargas currentcarga;
        public Cargas CurrentCarga { get => currentcarga; set { currentcarga = value; LoadProductosCarga(); RaiseOnPropertyChanged(); } }

        private ObservableCollection<CargasDetalle> productoscargas;
        public ObservableCollection<CargasDetalle> ProductosCargas { get => productoscargas; set { productoscargas = value; RaiseOnPropertyChanged(); } }

        private bool showprinter;
        public bool ShowPrinter { get => showprinter; set { showprinter = value; RaiseOnPropertyChanged(); } }

        private bool enableRechazarCarga;
        public bool EnableRechazarCarga { get => enableRechazarCarga; set { enableRechazarCarga = value; RaiseOnPropertyChanged(); } }

        public bool ShowButtons { get => ConsultedCarSecuencia == -1; }

        private DS_Cargas myCar;
        private readonly CargasFormats Printer;

        private Cargas cargaParaImprimir = null;

        private DS_Cuadres myCua;

        private int ConsultedCarSecuencia = -1;

        public CargasViewModel(Page page, int consultedCarSecuencia = -1) : base(page)
        {
            ConsultedCarSecuencia = consultedCarSecuencia;
            ClickHandlerCommand = new Command(OnOptionClick);
            myCar = new DS_Cargas();
            myCua = new DS_Cuadres();

            Printer = new CargasFormats(myCar);
            if(DS_RepresentantesParametros.GetInstance().GetParDesactivarRechazoCarga())
            {
                EnableRechazarCarga = false;
            }
            else
            {
                EnableRechazarCarga = true;
            }
            CargarCargasDisponibles();
        }

        private async void OnOptionClick(object Id)
        {
            switch (Id.ToString())
            {
                case "0": //salir
                    await PopAsync(true);
                    break;
                case "1": //aceptar carga
                    AceptarRechazarCarga();
                    break;
                case "2": //rechazar carga
                    if (!await DisplayAlert(AppResource.Warning, AppResource.RejectLoadQuestion, AppResource.Reject, AppResource.Cancel))
                    {
                        return;
                    }
                    AceptarRechazarCarga(true);
                    break;
            }
        }

        private void LoadProductosCarga()
        {
            try
            {
                if (CurrentCarga == null)
                {
                    ProductosCargas = null;
                    return;
                }

                ProductosCargas = new ObservableCollection<CargasDetalle>(myCar.GetProductosCarga(CurrentCarga.CarSecuencia));

            }catch(Exception e)
            {
                Console.Write(e.Message);
                DisplayAlert(AppResource.Warning, e.Message);
            }
        }

        private async void AceptarRechazarCarga(bool rechazar = false)
        {
            if (IsBusy)
            {
                return;
            }
            try
            {
                if (CurrentCarga == null)
                {
                    throw new Exception(AppResource.SelectLoadWarning);
                }

                var parCuadreDiarios = myParametro.GetParCuadresDiarios();
                var parCuadres = myParametro.GetParCuadres();
                var parCargasPermitidas = myParametro.GetParCargasPermitidasByCuadre();
               

                if (parCuadres > 0 && (Arguments.Values.CurrentCuaSecuencia == -1 || (parCuadreDiarios && myCua.GetCuadreAbierto(DateTime.Now.ToString("dd-MM-yyyy")) == null)))
                {

                    myCua.AbrirCerrarCuadre(parCuadreDiarios, (isCerrarCuadre, isimprimir) =>
                    {
                        if ((isCerrarCuadre && myParametro.GetParSincronizarAlCerrarCuadre()) || (!isCerrarCuadre && myParametro.GetParSincronizarAlAbrirCuadre()))
                        {
                            SincronizarSimple();
                        }
                    });
                    return;
                }

                if (parCuadres > 0 && Arguments.Values.CurrentCuaSecuencia == -1)
                {
                    throw new Exception(rechazar ? AppResource.OpenSquareToRejectLoad : AppResource.OpenSquareToAcceptLoad);
                }

                if (!rechazar && !myParametro.GetParUsarMultiAlmacenes())
                {
                    if (myCar.NoAceptarCargasInventarioNegativo(CurrentCarga.CarSecuencia))
                    {
                        throw new Exception(AppResource.CannotApplyLoadInventoryNegative);
                    }
                }

                if (ProductosCargas.ToList() == null)
                {
                    throw new Exception(AppResource.LoadWithoutDetail);
                }

                var parCargasAceptadas = myCar.GetCargasAceptadasByCuaSecuencia(Arguments.Values.CurrentCuaSecuencia);
                if (parCargasAceptadas.Count() >= parCargasPermitidas && parCargasPermitidas != -1)
                {
                    throw new Exception(AppResource.LimitAcceptedLoadInSquareMessage.Replace("@", parCargasPermitidas.ToString()));
                }

                var task = new TaskLoader() { SqlTransactionWhenRun = true };

                IsBusy = true;

                await task.Execute(() =>
                {
                    if (rechazar)
                    {
                        if (!myParametro.GetParCargasObligatorias())
                        {
                            myCar.RechazarCarga(CurrentCarga.rowguid);
                        }
                        else
                        {
                            throw new Exception(AppResource.CannotRejectBeforeAcceptLoads);
                        }
                    }
                    else
                    {
                        var referenciaEntrega = "";
                        if (myParametro.GetParCargasConReferenciaEntrega())
                        {
                            referenciaEntrega = myCar.GetCargaBySecuenciaConRefEntrega(CurrentCarga.CarSecuencia).CarReferenciaEntrega;
                        }

                        switch (myParametro.GetParCargasNoValidaTotales())
                        {

                            case 1:                                
                                myCar.AceptarCarga(CurrentCarga.rowguid, ProductosCargas.ToList(), CurrentCarga.AlmID, referenciaEntrega);
                                break;
                            case 2:
                                if (CurrentCarga.CarCantidadTotal == myCar.GetTotalProductosCargaInCount(CurrentCarga.CarSecuencia))
                                {
                                    myCar.AceptarCarga(CurrentCarga.rowguid, ProductosCargas.ToList(), CurrentCarga.AlmID, referenciaEntrega);
                                }
                                else
                                {
                                    throw new Exception(AppResource.CannotAcceptLoadDifferentInTotals);
                                }
                                break;
                            default:

                                if (CurrentCarga.CarCantidadTotal == myCar.GetTotalProductosCarga(CurrentCarga.CarSecuencia))
                                {
                                    myCar.AceptarCarga(CurrentCarga.rowguid, ProductosCargas.ToList(), CurrentCarga.AlmID, referenciaEntrega);
                                }
                                else
                                {
                                    throw new Exception(AppResource.CannotAcceptLoadDifferentInTotals);
                                }
                                break;
                        }
                    }

                    cargaParaImprimir = CurrentCarga.Copy();

                    CargarCargasDisponibles();
                });

                IsBusy = false;

                if (myParametro.GetParSincronizarAlFinalizarCargaInventario())
                {
                    await Sincronizar(true, rechazar);
                }
                else
                {
                    FinishProcess(rechazar);
                }

            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.InnerException != null ? e.InnerException.Message : e.Message);
                IsBusy = false;
            }
   
        }

        private async void FinishProcess(bool rechazar)
        {
            var result = await DisplayAlert(AppResource.Warning, rechazar ? AppResource.LoadRejectedCorrectly : AppResource.LoadAcceptedCorrectly, AppResource.Print, AppResource.Aceptar);

            if (result)
            {
                ShowPrinter = true;
            }
        }

        public async void AceptarImpresion(int Copias)
        {
            try
            {

                for (int x = 0; x < Copias; x++)
                {
                    IsBusy = true;

                    await Task.Run(() => 
                    {
                        Printer.Print(cargaParaImprimir.CarSecuencia, false, new Printer.PrinterManager());
                    });

                    IsBusy = false;

                    if (Copias > 1 && x != Copias - 1)
                    {
                        await DisplayAlert(AppResource.PrintCopy, AppResource.CutPapelMessage, AppResource.Print);
                    }

                }

                cargaParaImprimir = null;
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.ErrorPrintingLoad, e.Message, AppResource.Aceptar);
            }
            IsBusy = false;
            ShowPrinter = false;
        }

        private void CargarCargasDisponibles()
        {
            try
            {
                if(ConsultedCarSecuencia != -1)
                {
                    var carga = myCar.GetCargaBySecuencia(ConsultedCarSecuencia, true);

                    CargasDisponibles = new ObservableCollection<Cargas>() { carga };
                }
                else
                {
                    CargasDisponibles = new ObservableCollection<Cargas>(myCar.GetCargasDisponibles());
                }               

                if (CargasDisponibles != null && CargasDisponibles.Count > 0)
                {
                    CurrentCarga = CargasDisponibles[0];
                }

            }catch(Exception e)
            {
                DisplayAlert(AppResource.ErrorLoadingAvailablesLoads, e.Message);
            }
        }

        private async void SincronizarSimple()
        {
            await Sincronizar(false, false);
        }
        private async Task Sincronizar(bool forSave, bool rechazar)
        {
            try
            {
                Action action = null;

                if (forSave)
                {
                    action = () => { FinishProcess(rechazar); };
                }
                await PushModalAsync(new SincronizarModal(OnSyncCompleted: action));
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.SynchronizationFailed, e.Message, AppResource.Aceptar);
            }
        }

    }
}
