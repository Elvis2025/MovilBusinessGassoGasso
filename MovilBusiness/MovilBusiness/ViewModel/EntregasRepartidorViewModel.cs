using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Model;
using MovilBusiness.Printer;
using MovilBusiness.Printer.Formats;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using MovilBusiness.views;
using MovilBusiness.Views;
using MovilBusiness.Views.Components.Modals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class EntregasRepartidorViewModel : BaseViewModel
    {
        public ICommand SearchCommand { get; private set; }
        public ICommand GoEntregaMultipleCommand { get; private set; }
        public ICommand ImprimirEntregaCommand { get; private set; }

        private DS_EntregasRepartidorTransacciones myEnt;

        private List<EntregasRepartidorTransacciones> entregas;
        public List<EntregasRepartidorTransacciones> Entregas { get => entregas; set { entregas = value; RaiseOnPropertyChanged(); } }
        
        private string searchvalue;
        public string SearchValue { get => searchvalue; set { searchvalue = value; RaiseOnPropertyChanged(); } }

        private double enttotal;
        public double EntTotal { get => enttotal; set { enttotal = value; RaiseOnPropertyChanged(); } }

        public bool ParShowRowDetalle { get; private set; }
        public bool IsConsulta { get; private set; }

        public EntregasRepartidorViewModel(Page page, bool IsConsulta) : base(page)
        {
            this.IsConsulta = IsConsulta;
            myEnt = new DS_EntregasRepartidorTransacciones();

            SearchCommand = new Command(CargarEntregas);
            GoEntregaMultipleCommand = new Command(GoDetalleMultiple);
            ImprimirEntregaCommand = new Command(ImprimirEntregas);

            ParShowRowDetalle = myParametro.GetParEntregasRepartidorUsarRowDetallado() && !IsConsulta;
            
        }

        public async void GoDetalle(EntregasRepartidorTransacciones entrega)
        {
            if (IsBusy)
            {
                return;
            }

            if (IsConsulta)
            {
                VerDetalleEntrega(entrega);
                return;
            }

            IsBusy = true;

            try
            {
                 myEnt.DeleteFromTemp();
                await PushAsync(new EntregasRepartidorDetallePage(entrega));
            }catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        public async void GoDetalleMultiple()
        {
            if (IsBusy)
            {
                return;
            }

            if(Entregas == null || Entregas.Count == 0)
            {
                await DisplayAlert(AppResource.Warning, AppResource.NoDeliveriesAvailables);
                return;
            }

            IsBusy = true;

            try
            {
                myEnt.DeleteFromTemp();
                await PushAsync(new EntregasRepartidorDetallePage(Entregas));
            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        public void CargarEntregas() { CargarEntregas(false); }
        public async void CargarEntregas(bool deleteTemp = false)
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;

                string secCodigo = null;

                if (myParametro.GetParEntregasRepartidorPorSector() && Arguments.Values.CurrentSector != null)
                {
                    secCodigo = Arguments.Values.CurrentSector.SecCodigo;
                }

                if (IsConsulta)
                {
                    await Task.Run(() => { Entregas = myEnt.GetEntregasRealizadas(Arguments.Values.CurrentClient.CliID, SearchValue); });
                }
                else
                {
                    await Task.Run(() => { Entregas = myEnt.GetEntregasDisponibles(Arguments.Values.CurrentClient.CliID, SearchValue, isRecepcionDevolucion: Arguments.Values.CurrentModule == Modules.RECEPCIONDEVOLUCION, secCodigo: secCodigo); });
                }

                if (deleteTemp && !IsConsulta)
                {
                    myEnt.DeleteFromTemp();

                    if(myParametro.GetParCargasInventario() && new DS_Cargas().HayCargasDisponibles() && !myParametro.GetParNoCargasEntregaFactura() && Arguments.Values.CurrentModule != Modules.RECEPCIONDEVOLUCION)
                    {
                        var result = await DisplayAlert(AppResource.Warning, AppResource.LoadsPendingsWantGoToLoadsScreent, AppResource.GoToLoads, AppResource.Close);

                        if (result)
                        {
                            await PushAsync(new CargasPage());
                        }
                    }
                }

                EntTotal = Entregas != null ? Math.Round(Entregas.Sum(x => x.EntMontoTotal), 2) : 0;

            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        public async void RechazarEntrega(string rowguid, string motivo = null)
        {
            if (IsBusy)
            {
                return;
            }

            var entrega = Entregas.Where(x => x.rowguid == rowguid).FirstOrDefault();

            if (IsConsulta)
            {
                AnularEntrega(entrega);
                return;
            }

            try
            {
                IsBusy = true;

                var entregas = myEnt.GetEntregasDisponibles(Arguments.Values.CurrentClient.CliID);

                var parEntregasMultiples = myParametro.GetParEntregasMultiples();
                var parMotivoDevolucion = myParametro.GetParEntregasRepartidorMotivoDevolucion() && (!parEntregasMultiples || entregas.Count == 1);

                if (parMotivoDevolucion && string.IsNullOrWhiteSpace(motivo) && entrega != null)
                {
                    IsBusy = false;
                    if (parEntregasMultiples)
                    {
                        ShowMotivoDevolucion(rowguid);
                    }
                    else
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.MustSpecifyReasonForReturn);
                        await PushModalAsync(new SeleccionarMotivoDevolucionModal() { OnMotivoAceptado = (motId) => { RechazarEntrega(rowguid, motId.ToString()); } });
                    }
                    return;
                }

                if (entrega != null)
                {
                    await myEnt.RechazarEntrega(entrega, 4, motivo);
                }

                await DisplayAlert(AppResource.Warning, AppResource.DeliveryRejectedSuccessfully);

                IsBusy = false;

                if(myEnt.GetEntregasDisponibles(Arguments.Values.CurrentClient.CliID).Count == 0)
                {
                    OperacionesPage.CloseVisit = true;
                    await PopAsync(false);
                    return;
                }

                CargarEntregas();

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.InnerException != null ? e.InnerException.Message : e.Message);
            }

            IsBusy = false;
        }

        private async void ShowMotivoDevolucion(string rowguid, Action<int> onMotivoSelected = null)
        {
            var motivos = new DS_Devoluciones().GetMotivosDevolucion();

            var options = motivos.Select(x => x.MotDescripcion).ToArray();

            var eleccion = await DisplayActionSheet(AppResource.ReasonForReturn, buttons: options);

            var motivo = motivos.Where(x => x.MotDescripcion.Equals(eleccion)).FirstOrDefault();

            if (motivo != null)
            {
                if (onMotivoSelected == null)
                {
                    RechazarEntrega(rowguid, motivo.MotID.ToString());
                }
                else
                {
                    onMotivoSelected?.Invoke(motivo.MotID);
                }
            }
        }

        private async void AnularEntrega(EntregasRepartidorTransacciones entrega)
        {
            IsBusy = true;

            try
            {
                var puedeAnular = myEnt.SePuedeAnularEntrega(entrega.EnrSecuencia, entrega.TraSecuencia);

                if(entrega.enrEstatusEntrega == 0)
                {
                    throw new Exception(AppResource.DeliveryHasBeenAlreadyCanceled);
                }

                if (!puedeAnular)
                {
                    throw new Exception(AppResource.LeadsCreatedForDeliveryCannotRevoke);
                }

                var task = new TaskLoader() { SqlTransactionWhenRun = true };

                await task.Execute(() => 
                {
                    myEnt.AnularEntrega(entrega);
                });

                IsBusy = false;

                await DisplayAlert(AppResource.Warning, AppResource.DeliveryRevokeSuccessfully);

                CargarEntregas();

                return;

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        public void VerDetalleEntrega(EntregasRepartidorTransacciones entrega)
        {
            try
            {
                myEnt.DeleteFromTemp();

                if (IsConsulta)
                {
                    myEnt.InsertProductInTempForDetail(entrega.EntSecuencia, entrega.Confirmada);
                }
                else
                {
                    myEnt.InsertProductInTemp(entrega.EnrSecuencia, entrega.TraSecuencia, entrega.TitID, entrega.CliID, true, true);
                }
                Arguments.Values.CurrentModule = Modules.ENTREGASREPARTIDOR;

                PushAsync(new EntregasRepartidorDetalleRevisionPage(entrega, true));

            }catch(Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message);
            }
        }


        private async void ImprimirEntregas()
        {
            try
            {
                var entregas = myEnt.GetEntregasTransaccionesRealizadas();

                if(entregas.Count == 0)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.NoDeliveriesMade);
                    return;
                }

                IsBusy = true;

                var copias = await DisplayActionSheet(AppResource.ChoosePrinterCopies, buttons: new string[] { "1", "2", "3", "4", "5" });

                if (int.TryParse(copias, out int intCopias) && intCopias > 0)
                {
                    PrinterManager printer = null;
                    var formats = new EntregasRepartidorFormats(myEnt);

                    for (int i = 0; i < intCopias; i++)
                    {
                        await Task.Run(() =>
                        {
                            if (printer == null)
                            {
                                printer = new PrinterManager();
                            }

                            formats.ImprimirEntregasDeLaVisita(true, printer);
                        });

                        IsBusy = false;

                        if (intCopias > 1 && i != intCopias - 1)
                        {
                            await DisplayAlert(AppResource.PrintCopy, AppResource.CutPapelMessage, AppResource.Print);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        public async void ShowAlertSalir()
        {
            try
            {

                var entregasRechazadas = myEnt.GetEntregasRechazadasSinMotivo();

                if (entregasRechazadas.Count > 0)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.AreRejectedDeliveriesMustSpecifyReturnReason);
                    ShowMotivoDevolucion(null, (motId) => 
                    {
                        ActualizarMotivoEntregasRechazadas(motId, entregasRechazadas);
                    });
                    return;
                }

                await PopAsync(false);

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }
        }

        private async void ActualizarMotivoEntregasRechazadas(int motId, List<EntregasRepartidorTransacciones> entregas)
        {
            try
            {

                IsBusy = true;

                var task = new TaskLoader() { SqlTransactionWhenRun = true };

                await task.Execute(()=> 
                {
                    myEnt.ActualizarMotivoEntregaRechazada(motId, entregas);
                });                

                await PopAsync(false);
            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
            
        }

    }
}
