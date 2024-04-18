using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.Model;
using MovilBusiness.Printer;
using MovilBusiness.Printer.Formats;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class CuadresViewModel : BaseViewModel
    {
        public ICommand SearchCommand { get; private set; }

        public bool IsCerrarCuadre { get => CurrentCuadre != null; }

        private string fichasearch;
        public string FichaSearch { get => fichasearch; set { fichasearch = value; RaiseOnPropertyChanged(); } }

        private string kilometrajeactual;
        public string KilometrajeActual { get => kilometrajeactual; set { kilometrajeactual = value; RaiseOnPropertyChanged(); } }

        private string ultimokilometraje;
        public string UltimoKilometraje { get => ultimokilometraje; set { ultimokilometraje = value; RaiseOnPropertyChanged(); } }

        public bool ShowAyudantes { get { var par = myParametro.GetParCuadres(); return par == 3; } }
        public bool ShowContador { get => !myParametro.GetParCuadresOcultarContador(); }
        public bool AyudantesEnabled { get => !IsCerrarCuadre; }

        private string contadorlogico;
        public string ContadorLogico { get => contadorlogico; set { contadorlogico = value; RaiseOnPropertyChanged(); } }

        private string contadorfisico;
        public string ContadorFisico { get => contadorfisico; set { contadorfisico = value; RaiseOnPropertyChanged(); } }

        private string ayudante1;
        public string Ayudante1 { get => ayudante1; set { ayudante1 = value; RaiseOnPropertyChanged(); } }

        private string ayudante2;
        public string Ayudante2 { get => ayudante2; set { ayudante2 = value; RaiseOnPropertyChanged(); } }

        private string RepAuditor;

        private List<Vehiculos> vehiculos;
        public List<Vehiculos> Vehiculos { get => vehiculos; set { vehiculos = value; RaiseOnPropertyChanged(); } }

        private Vehiculos currentvehiculo;
        public Vehiculos CurrentVehiculo { get => currentvehiculo; set { currentvehiculo = value; OnCurrentVehicleChanged(); RaiseOnPropertyChanged(); } }

        public string NumeroCuadre { get => " "+AppResource.Square+" #: " + (CurrentCuadre == null ? DS_RepresentantesSecuencias.GetLastSecuencia("Cuadres").ToString() : CurrentCuadre.CuaSecuencia.ToString()); }

        private DS_Vehiculos myVeh;
        private DS_Cuadres myCua;

        private DS_Representantes myRep;

        private readonly Cuadres CurrentCuadre;
        private Action<bool,int> CuadreGuardado;
        private bool showprinter;
        PrinterManager printer;
        public bool ShowPrinter { get => showprinter; set { showprinter = value; RaiseOnPropertyChanged(); } }
        private   CuadresFormats Printer;

        public CuadresViewModel(Page page, Cuadres CurrentCuadre, Action<bool,int> CuadreGuardado, string RepAuditor = "") : base(page)
        {

            myVeh = new DS_Vehiculos();
            myCua = new DS_Cuadres();
            Printer = new CuadresFormats(myCua);
            myRep = new DS_Representantes();


            this.CuadreGuardado = CuadreGuardado;
            this.RepAuditor = RepAuditor;

            this.CurrentCuadre = CurrentCuadre;
            SaveCommand = new Command(() =>
            {
                SaveCuadre();

            }, () => IsUp);

            SearchCommand = new Command(FiltrarVehiculos);

            //Vehiculos = myParametro.GetParCuadresVehiculosRepresentante() ? new List<Vehiculos>() { myVeh.GetVehicleById(Arguments.CurrentUser.VehID) } : myVeh.GetAllVehiculos();
            if(Arguments.CurrentUser.VehID <= 0)
            {
                Arguments.CurrentUser = DS_Representantes.RefrescarRepresentante(Arguments.CurrentUser.RepCodigo);
            }
            var _vehiculos = myVeh.GetAllVehiculos();

            var vm = DS_Representantes.RefrescarRepresentante(Arguments.CurrentUser.RepCodigo);

            var sm = _vehiculos.Where(c => c.VehID == vm.VehID).ToList();

            if (sm.Count > 0)
            {
                Arguments.CurrentUser = DS_Representantes.RefrescarRepresentante(Arguments.CurrentUser.RepCodigo);
            }

            if (CurrentCuadre != null)
            {
                Vehiculos = _vehiculos;
                CurrentVehiculo = Vehiculos.FirstOrDefault(x => x.VehID == CurrentCuadre.VehID);
                if (CurrentVehiculo != null)
                {
                    CurrentVehiculo.VehKilometraje = CurrentCuadre.CuaKilometrosInicio;
                }
                KilometrajeActual = CurrentCuadre.CuaKilometrosFin.ToString();
                UltimoKilometraje = CurrentCuadre.CuaKilometrosInicio.ToString();
                Ayudante1 = CurrentCuadre.RepAyudante1;
                Ayudante2 = CurrentCuadre.RepAyudante2;
            }
            else
            {
                Vehiculos = myParametro.GetParCuadresVehiculosRepresentante() ? _vehiculos.Where(v => v.VehID == Arguments.CurrentUser.VehID).ToList() : _vehiculos;
            }
        }

        public void LoadCurrentVehicle()
        {
            if (CurrentCuadre != null)
                CurrentVehiculo = Vehiculos.FirstOrDefault(x => x.VehID == CurrentCuadre.VehID);
            else            
                CurrentVehiculo = Vehiculos.FirstOrDefault(x => x.VehID
                == DS_Representantes.RefrescarRepresentante
                (Arguments.CurrentUser.RepCodigo).VehID);            
        }

        private bool ValidarVehiculoComGls(int lastKilometraje, int kilometrajeActual)
        {
            if (!IsCerrarCuadre || !myParametro.GetParCuadresValidarCantidadKmPromedioPorDia())
            {
                return true;
            }

            if(CurrentVehiculo == null || CurrentVehiculo.VehID == -1) //|| CurrentVehiculo.VehRecorridoPromXdia <= 0)
            {
                return false;
            }

            DateTime.TryParse(CurrentVehiculo.VehFechaActualizacion, new CultureInfo("en"), DateTimeStyles.None, out DateTime lastUpdate);

            //var days = (DateTime.Now - lastUpdate).TotalDays;

            //var maximoKilometrajePermitido = (lastKilometraje + days) * CurrentVehiculo.VehRecorridoPromXdia;

            //if(kilometrajeActual > maximoKilometrajePermitido)
            //{
            //    DisplayAlert(AppResource.Warning, AppResource.ActualKilometerExceedMaximumAllowed + ": " + maximoKilometrajePermitido.ToString("N2"));
            //    return false;
            //}

            return true;

        }

        private async void SaveCuadre()
        {
            IsUp = false;

            if (IsBusy)
            {
                return;
            }

            if ((CurrentVehiculo == null || CurrentVehiculo.VehID == -1)  && !IsCerrarCuadre)
            {
                IsUp = true;
                await DisplayAlert(AppResource.Warning, AppResource.MustSelectVehicle);
                return;
            }

            if (string.IsNullOrWhiteSpace(KilometrajeActual))
            {
                IsUp = true;
                await DisplayAlert(AppResource.Warning, AppResource.CurrentMileageCannotBeEmpty);
                return;
            }


            if (ShowContador && string.IsNullOrWhiteSpace(ContadorFisico))
            {
                IsUp = true;
                await DisplayAlert(AppResource.Warning, AppResource.PhysicalCounterCannotBeEmpty);
                return;
            }

            if (DS_RepresentantesParametros.GetInstance().GetParValidaAyudantes())
            {
                if (!string.IsNullOrWhiteSpace(Ayudante1))
                {
                    if (myRep.GetRepNombre(Ayudante1) == "")
                    {
                        IsUp = true;
                        await DisplayAlert(AppResource.Warning, AppResource.Helper1CodeIsNotValid);
                        return;
                    }
                }

                if (!string.IsNullOrWhiteSpace(Ayudante2))
                {
                    if (myRep.GetRepNombre(Ayudante2) == "")
                    {
                        IsUp = true;
                        await DisplayAlert(AppResource.Warning, AppResource.Helper2CodeIsNotValid);
                        return;
                    }
                }
            }
            

            int.TryParse(UltimoKilometraje, out int ultKilometrajeInt);
            int.TryParse(KilometrajeActual, out int kilometrajeActualInt);
            int.TryParse(ContadorLogico, out int contadorLogicoInt);
            int.TryParse(ContadorFisico, out int contadorFisicoInt);

            if (kilometrajeActualInt < ultKilometrajeInt)
            {
                IsUp = true;
                await DisplayAlert(AppResource.Warning, AppResource.CurrentMileageMustBeGreaterThanLastMileage);
                return;
            }

            if (myParametro.GetParValidarKilometrajeEnCuadres() > 0 && (kilometrajeActualInt - ultKilometrajeInt) > myParametro.GetParValidarKilometrajeEnCuadres())
            {
                IsUp = true;
                if (!await DisplayAlert(AppResource.Warning, $"El recorrido del dia de hoy es de: {kilometrajeActualInt.ToString()}, está seguro de que quiere abrir/cerrar cuadre? ", "Aceptar", "Cancelar"))
                return;
            }

            if (!ValidarVehiculoComGls(ultKilometrajeInt, kilometrajeActualInt))
            {
                IsUp = true;
                return;
            }

            if (ShowContador && contadorFisicoInt < contadorLogicoInt)
            {
                IsUp = true;
                await DisplayAlert(AppResource.Warning, AppResource.PhysicalCounterMustBeGreaterThanLogicalCounter);
                return;
            }

            try
            {
                if (IsCerrarCuadre)
                {
                    var ent = new DS_EntregasRepartidorTransacciones();

                    if (myParametro.GetParCuadresValidarEntregasPendienteParaCerrar())
                    {
                        var IsVentaValidaEntrega = myParametro.GetParCuadresValidarVentasPendienteParaCerrar();
                        var entregasPendientes = ent.GetEntregasDisponiblesEnGeneral(forVenta: IsVentaValidaEntrega);
                        if (entregasPendientes.Count > 0)
                        {
                            List<string> clientesPendientes = entregasPendientes.Select(c => (c.CliCodigo + "-" + c.CliNombre).ToString()).Distinct().ToList();
                            //await DisplayAlert(AppResource.Warning, "Tienes entregas pendientes, no puedes cerrar el cuadre.", "Aceptar");
                            IsUp = true;
                            await DisplayActionSheet(AppResource.PendingCustomers, "OK", clientesPendientes.ToArray());
                            return;
                        }
                    }
                }

                IsBusy = true;

                var task = new TaskLoader() { SqlTransactionWhenRun = true };

                await task.Execute(() =>
                {
                    if (IsCerrarCuadre)
                    {
                        myCua.CerrarCuadre(CurrentCuadre.CuaSecuencia, Arguments.Values.CurrentLocation, kilometrajeActualInt, contadorFisicoInt, CurrentCuadre.VehID, CurrentCuadre.rowguid);
                        Arguments.Values.CurrentCuaSecuencia = CurrentCuadre.CuaSecuencia;
                    }
                    else
                    {
                        myCua.AbrirCuadre(Arguments.Values.CurrentLocation, CurrentVehiculo.VehID, Ayudante1, Ayudante2, kilometrajeActualInt, contadorFisicoInt, RepAuditor);

                        if (myParametro.GetParCargasAutomaticasAperturaCuadre())
                        {
                            DS_Cargas myCar = new DS_Cargas();
                            var cargas = myCar.GetCargasDisponibles();
                            if (cargas != null && cargas.Count > 0)
                            {
                                foreach (var carga in cargas)
                                {
                                    //if (myCar.VerificarCargasSiDejanInventarioNegativo(CurrentCarga.CarSecuencia))
                                    //{
                                    //    throw new Exception("No puede aplicar esta carga debido a que deja productos en inventario negativo");
                                    //}
                                    var productosCarga = myCar.GetProductosCarga(carga.CarSecuencia);
                                    myCar.AceptarCarga(carga.rowguid, productosCarga.ToList(), carga.AlmID);
                                }

                            }

                        }
                    }
                });

                bool result = false;
                if (!IsCerrarCuadre && myParametro.GetParNoImpresionAperturaCuadre())
                {
                    await DisplayAlert(AppResource.Success, AppResource.SquareOpenSuccessfully, AppResource.Continue);
                }
                else
                {
                    result = await DisplayAlert(AppResource.Success, IsCerrarCuadre ? AppResource.SquareClosedSuccessfully : AppResource.SquareOpenSuccessfully, AppResource.Print, AppResource.Continue);
                }

                if (result)
                {
                    Imprimir();
                }

                if (!result)
                {
                    await PopAsync(true);
                    CuadreGuardado?.Invoke(IsCerrarCuadre, 0);
                }

            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.InnerException != null ? e.InnerException.Message : e.Message);
            }

            IsBusy = false;
            IsUp = true;
        }

        public async void Imprimir()
        {
            var copias = await DisplayActionSheet(AppResource.ChoosePrinterCopies,  buttons: new string[] { "1", "2", "3", "4", "5" });

            int.TryParse(copias, out int intCopias);

            if (intCopias > 0)
            {
               AceptarImpresion(intCopias);
            }

        }
        public async void AceptarImpresion(int Copias)
        {
            try
            {
                IsBusy = true;

                await Task.Run(() =>
                {
                    printer = new PrinterManager();
                });

                for (int x = 0; x < Copias; x++)
                {
                    if (DS_RepresentantesParametros.GetInstance().GetImpresionSoloFacturaCreditos())
                    {
                            await Task.Run(() =>
                            {
                                Printer.PrintFacturas(Arguments.Values.CurrentCuaSecuencia, false, printer);
                            });

                         IsBusy = false;
                    }
                    else
                    {
                        await Task.Run(() =>
                        {
                            Printer.Print(Arguments.Values.CurrentCuaSecuencia, false, printer);
                        });

                        IsBusy = false;

                        if (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCuadres() == 4)
                        {
                            await DisplayAlert(AppResource.PrintingInvoices, AppResource.CutPapelMessage, AppResource.Print);
                            IsBusy = true;
                            await Task.Run(() =>
                            {
                                Printer.PrintFacturas(Arguments.Values.CurrentCuaSecuencia, false, printer);
                            });
                            IsBusy = false;
                        }
                    }

                    if (Copias > 1 && x != Copias - 1)
                    {
                        await DisplayAlert(AppResource.PrintCopy, AppResource.CutPapelMessage, AppResource.Print);
                    }

                }
                IsBusy = true;
                await PopAsync(true);
                CuadreGuardado?.Invoke(IsCerrarCuadre,2);
                IsBusy = false;

            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.ErrorPrinting, e.Message, AppResource.Aceptar);
                CuadreGuardado?.Invoke(IsCerrarCuadre, 2);
            }
            IsBusy = false;
            ShowPrinter = false;
        }

     

        private void OnCurrentVehicleChanged()
        {
            if (CurrentVehiculo == null)
            {
                UltimoKilometraje = "";
                KilometrajeActual = "";
                ContadorFisico = "";
                ContadorLogico = "";
                return;
            }
            if (IsCerrarCuadre)
            {
                UltimoKilometraje = CurrentVehiculo.VehKilometraje.ToString();
                KilometrajeActual = "";
            }
            else
            {
                UltimoKilometraje = CurrentVehiculo.VehKilometraje.ToString();
                KilometrajeActual = CurrentVehiculo.VehKilometraje.ToString();
            }

            ContadorLogico = CurrentVehiculo.VehContador.ToString();
            ContadorFisico = "";
        }

        private void FiltrarVehiculos()
        {
            if (string.IsNullOrWhiteSpace(FichaSearch))
            {
                CurrentVehiculo = null;
            }
            else
            {
                CurrentVehiculo = Vehiculos.Where(x => x.VehFicha.Trim().ToUpper().Contains(FichaSearch.Trim().ToUpper())).FirstOrDefault();
            }
        }
    }
}
