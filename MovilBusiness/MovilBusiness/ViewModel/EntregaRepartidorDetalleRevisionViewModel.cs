using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
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
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class EntregaRepartidorDetalleRevisionViewModel : BaseViewModel
    {

        private DS_EntregasRepartidorTransacciones myEnt;
        private DS_Inventarios myInv;
        private DS_Conduces myCon;
        public Command ImprimirCommand { get; set; }
        public EntregasRepartidorTransacciones CurrentEntrega { get; private set; }
        public List<EntregasRepartidorTransacciones> Entregas { get; private set; }

        private int SavedEntSecuencia = -1;
        private List<int> SavedEntSecuencias = null;
        private int TraSecuencias;
        private int EnrSecuencias;

        private List<EntregasDetalleTemp> productos;
        public List<EntregasDetalleTemp> Productos { get => productos; set { productos = value; RaiseOnPropertyChanged(); } }

        private bool IsDetalle = false;

        public string SaveIcon { get; private set; } = "ic_done_white_24dp";

        private Totales totales;
        public Totales Totales { get => totales; set { totales = value; RaiseOnPropertyChanged(); } }

        public EntregaRepartidorDetalleRevisionViewModel(Page page, List<EntregasRepartidorTransacciones> entregas) : base(page)
        {
            Entregas = entregas;
            Init();

            SaveCommand = new Command(() =>
            {
                GuardarEntrega();

            }, () => IsUp);
        }

        public EntregaRepartidorDetalleRevisionViewModel(Page page, EntregasRepartidorTransacciones entrega, bool IsDetalle = false) : base(page)
        {
            this.IsDetalle = IsDetalle;
            CurrentEntrega = entrega;
            TraSecuencias = entrega.TraSecuencia;
            EnrSecuencias = entrega.EnrSecuencia;
            Init();

            SaveCommand = new Command(() =>
            {
                GuardarEntrega();

            }, () => IsUp);

            ImprimirCommand = new Command(async () =>
            {
                await ImprimirEntrega(true);
            }, () => IsUp);
        }

        private void Init()
        {            
            myEnt = new DS_EntregasRepartidorTransacciones();
            myInv = new DS_Inventarios();
            myCon = new DS_Conduces();

            if (myParametro.GetParConducesGuardarYImprimirDirectamente() || myParametro.GetParEntregasMultiples())
            {
                SaveIcon = "baseline_print_white_24";
            }

            if (myParametro.GetParConducesUsarRowSinDialog())
            {
                myEnt.ConducesActualizarCantidadSolicitada();
            }
        }

        private void GuardarEntrega() { GuardarEntrega(null); }
        private async void GuardarEntrega(string motivoDevolucion = null)
        {
            var successSave = false;
            IsUp = false;

            try
            {
                if (IsBusy)
                {
                    return;
                }

                IsBusy = true;

                List<EntregasRepartidorTransaccionesDetalle> NoEntregados = null;

                if(Entregas == null)
                {
                    NoEntregados = myEnt.GetProductosNoEntregados(CurrentEntrega.EnrSecuencia, CurrentEntrega.TraSecuencia, CurrentEntrega.TitID);
                }
                else
                {
                    NoEntregados = myEnt.GetProductosNoEntregados(Entregas);
                }

                var parMotivoDevolucion = myParametro.GetParEntregasRepartidorMotivoDevolucion();

                if (NoEntregados != null && NoEntregados.Count > 0 && !parMotivoDevolucion && !myParametro.GetParNoDevolucionEnEntrega())
                {
                    var seguir = await DisplayAlert(AppResource.Warning, AppResource.ProductsNotDeliveredWillCreateReturn, AppResource.Aceptar, AppResource.Cancel);

                    if (!seguir)
                    {
                        IsBusy = false;
                        IsUp = true;
                        return;
                    }
                }
                
                if(Arguments.Values.CurrentModule == Modules.CONDUCES && myParametro.GetParEntregasOfertasTodoONada())
                {
                    if (myEnt.SeQuitaraOferta())
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.ProductsThatGiveOffersHasNotBeenDelivered);
                        IsBusy = false;
                        IsUp = true;
                        return;
                    }

                    if (myEnt.HayOfertasSinEntregarCompletamente(validarOfertaCantidad:false, isAdded:true))
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.OfferProductNotFullyDelivered);
                        IsBusy = false;
                        IsUp = true;
                        return;
                    }
                }
                int enrSecuencia =  0;
                if (Entregas == null)
                {
                    enrSecuencia = CurrentEntrega.EnrSecuencia;
                }
                else
                {
                    enrSecuencia = Entregas.FirstOrDefault().EnrSecuencia;
                }                

                var hayEntregasRechazadas = myEnt.HayEntregasRechazadas(enrSecuencia) && myParametro.GetParEntregasMultiples();
                if (parMotivoDevolucion && ((NoEntregados != null && NoEntregados.Count > 0) || hayEntregasRechazadas) && string.IsNullOrWhiteSpace(motivoDevolucion))
                {
                    await DisplayAlert(AppResource.Warning, (hayEntregasRechazadas ? AppResource.HaveRejectedDeliveriesOr : "") + AppResource.ProductsNotDeliveredFullyMustSpecifyReason);

                    if (myParametro.GetParEntregasMultiples())
                    {
                        ShowMotivoDevolucion();
                    }
                    else
                    {
                        await PushModalAsync(new SeleccionarMotivoDevolucionModal() { OnMotivoAceptado = (motId) => { GuardarEntrega(motId.ToString()); } });

                    }
                    IsBusy = false;
                    IsUp = true;
                    return;
                }

                if(Arguments.Values.CurrentModule == Modules.CONDUCES && myEnt.HayProductosSinLote() && myParametro.GetParConducesUsarRowSinDialog() && !IsDetalle)
                {
                    var keep = await DisplayAlert(AppResource.Warning, AppResource.ProductAddedWithoutLotWantContinue, AppResource.Continue, AppResource.Cancel);

                    if (!keep)
                    {
                        IsBusy = false;
                        IsUp = true;
                        return;
                    }
                }

                if(myParametro.GetParConducesUsarRowSinDialog() && !myEnt.IsSomethingAdded(true))
                {
                    await DisplayAlert(AppResource.Warning, AppResource.YouHaveNotAddAnyProductWarning);
                    IsBusy = false;
                    IsUp = true;
                    return;
                }

                var task = new TaskLoader() { SqlTransactionWhenRun = true };

                await task.Execute(() => 
                {
                    if(Arguments.Values.CurrentModule == Modules.CONDUCES)
                    {
                        SavedEntSecuencia = myCon.GuardarConduce(CurrentEntrega);
                    }
                    else
                    {
                        if(Entregas == null)
                        {
                            SavedEntSecuencia = myEnt.GuardarEntrega(CurrentEntrega, Arguments.Values.CurrentModule == Modules.RECEPCIONDEVOLUCION, NoEntregados, motivoDevolucion);
                        }
                        else
                        {
                            SavedEntSecuencias = myEnt.GuardarEntrega(Entregas, Arguments.Values.CurrentModule == Modules.RECEPCIONDEVOLUCION, motivoDevolucion);
                        }  
                    }
                });

                successSave = true;

                EntregasRepartidorDetallePage.Finish = true;

                if(myParametro.GetParEntregasMultiples())
                {
                    if (myParametro.GetParEntregasFirmaDeContadoObligatoria())
                    {
                        var esDeContado = false;
                        if (myParametro.GetParEntregasMultiples())
                        {
                            esDeContado = Entregas.FirstOrDefault(x => x.ConID == myParametro.GetParConIdFormaPagoContado()) != null;
                        }
                        else
                        {
                            esDeContado = CurrentEntrega.ConID == myParametro.GetParConIdFormaPagoContado();
                        }

                        if (esDeContado)
                        {
                            GuardarFirma(SavedEntSecuencias, esDeContado);
                            return;
                        }
                        else
                        {
                            var firmar = await DisplayAlert(AppResource.Warning, AppResource.WantSpecifyCustomerSignature, AppResource.Sign, AppResource.Continue);

                            if (firmar)
                            {
                                GuardarFirma(SavedEntSecuencias, esDeContado);
                                return;
                            }
                        }
                    }
                }

                AfterSave(successSave);
            }
            catch(Exception e)
            {
                IsBusy = false;
                IsUp = true;
                await DisplayAlert(AppResource.Warning, e.Message);
            }
 
        }

        private async void AfterSave(bool successSave)
        {
            var title = AppResource.DeliverySavedUpper;

            if (Arguments.Values.CurrentModule == Modules.RECEPCIONDEVOLUCION)
            {
                title = AppResource.ReceptionSavedUpper;
            }
            else if (Arguments.Values.CurrentModule == Modules.CONDUCES)
            {
                title = "CONDUCE " + AppResource.SavedUpper;
            }

            if (myParametro.GetParEntregasRepartidorGuardarReciboDeContado())
            {
                var goRecibo = false;
                var entSecuencia = -1;
                if (myParametro.GetParEntregasMultiples())
                {
                    var ent = Entregas.FirstOrDefault(x => x.ConID == myParametro.GetParConIdFormaPagoContado());

                    if (ent != null)
                    {
                        goRecibo = true;
                    }
                }
                else
                {
                    if (CurrentEntrega.ConID == myParametro.GetParConIdFormaPagoContado())
                    {
                        goRecibo = true;
                        entSecuencia = SavedEntSecuencia;
                    }
                }

                if (goRecibo)
                {
                    EntregasRepartidorDetalleRevisionPage.Finish = true;

                    var moneda = new DS_Monedas().GetMoneda(Arguments.Values.CurrentClient.MonCodigo);

                    await PushAsync(new RecibosTabPage(entSecuencia, moneda, EntregasSecuencias:SavedEntSecuencias));                    

                    if (successSave)
                    {
                        if (myParametro.GetParConducesGuardarYImprimirDirectamente() || myParametro.GetParEntregasMultiples())
                        {
                            if (myEnt.GetEntregasDisponibles(Arguments.Values.CurrentClient.CliID).Count == 0)
                            {
                                EntregasRepartidorPage.CloseVisit = true;
                            }

                            // await PopAsync(false);
                        }
                    }

                    IsUp = false;
                    IsBusy = false;

                    return;
                }
            }

            if (myParametro.GetParConducesGuardarYImprimirDirectamente() || myParametro.GetParEntregasMultiples())
            {
                await ImprimirEntrega();
            }
            else
            {
                await PushAsync(new SuccessPage(title, SavedEntSecuencia));
                EntregasRepartidorDetalleRevisionPage.Finish = true;
            }

            if (successSave)
            {
                if (myParametro.GetParConducesGuardarYImprimirDirectamente() || myParametro.GetParEntregasMultiples())
                {
                    if (myEnt.GetEntregasDisponibles(Arguments.Values.CurrentClient.CliID).Count == 0)
                    {
                        EntregasRepartidorPage.CloseVisit = true;
                    }

                    await PopAsync(false);
                }
            }

            IsUp = false;
            IsBusy = true;
        }

        private async void ShowMotivoDevolucion()
        {
            var motivos = new DS_Devoluciones().GetMotivosDevolucion();

            var options = motivos.Select(x => x.MotDescripcion).ToArray();

            var eleccion = await DisplayActionSheet(AppResource.ReasonForReturn, buttons: options);

            var motivo = motivos.Where(x => x.MotDescripcion.Equals(eleccion)).FirstOrDefault();

            if (motivo != null)
            {
                GuardarEntrega(motivo.MotID.ToString());
            }
        }

        private void SetTotales()
        {
            try
            {
                Totales = myEnt.GetTempTotales(IsDetalle || (Arguments.Values.CurrentModule == Modules.CONDUCES && myParametro.GetParConducesUsarRowSinDialog()), -1, !IsDetalle);
                
            }catch(Exception e)
            {
                DisplayAlert(AppResource.ErrorLoadingTotals, e.Message);
            }
 
        }

        private async Task ImprimirEntrega(bool IsConsulta = false)
        {
            IPrinterFormatter TraPrinter = null;

            if(Arguments.Values.CurrentModule == Modules.CONDUCES)
            {
                TraPrinter = new ConducesFormats(new DS_Conduces());
            }
            else
            {
                TraPrinter = new EntregasRepartidorFormats(new DS_EntregasRepartidorTransacciones());
            }

            var intCopias = 0;

            var copiasAutomaticas = 0;

            if(Arguments.Values.CurrentModule == Modules.ENTREGASREPARTIDOR)
            {
                copiasAutomaticas = myParametro.GetParEntregasImpresionAutomatica();
            }else if(Arguments.Values.CurrentModule == Modules.CONDUCES)
            {
                copiasAutomaticas = myParametro.GetParConducesImpresionAutomatica();
            }

            if (copiasAutomaticas > 0)
            {
                intCopias = copiasAutomaticas;
            }
            else
            {
                var copias = await DisplayActionSheet(AppResource.ChoosePrinterCopies, AppResource.Cancel, buttons: new string[] { "1", "2", "3", "4", "5" });
                int.TryParse(copias, out intCopias);
            }

            if(intCopias > 0)
            {
                try
                {
                    IsBusy = true;
                    PrinterManager printer = null;

                    for (int x = 0; x < intCopias; x++)
                    {
                        await Task.Run(() =>
                        {
                            if (printer == null)
                            {
                                printer = new PrinterManager(DS_RepresentantesParametros.GetInstance().GetParEmpresasBySector() ? Arguments.Values.CurrentSector?.SecCodigo ?? "" : "");
                            }

                            if(!IsConsulta)
                            {
                                TraPrinter.Print(SavedEntSecuencia, false, printer);
                            }
                            else
                            {
                                TraPrinter.Print(TraSecuencias, false, printer,traSecuencia2: EnrSecuencias);
                            }

                        });

                        IsBusy = false;

                        if (intCopias > 1 && x != intCopias - 1)
                        {
                            await DisplayAlert(AppResource.PrintCopy, AppResource.CutPapelMessage, AppResource.Print);
                        }
                    }
                    IsBusy = false;
                }
                catch (Exception e)
                {
                    await DisplayAlert(AppResource.ErrorPrintingTransaction, e.Message, AppResource.Aceptar);
                    IsBusy = false;
                }
            }
        }

        public async void CargarProductos()
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;

                var validarLotes = true;

                if (myParametro.GetParConducesUsarRowSinDialog())
                {
                    validarLotes = false;
                }

                await Task.Run(() =>
                {
                    var withQuantity = true;

                    if(myParametro.GetParEntregasOfertasTodoONada() && Arguments.Values.CurrentModule == Modules.CONDUCES)
                    {
                        withQuantity = false;
                    }

                    Productos = myEnt.GetProductosEntregaInTemp(withQuantity, (myParametro.GetParEntregasRepartidorValidarOfertas() || myParametro.GetParEntregasOfertasTodoONada()) && Arguments.Values.CurrentModule != Modules.CONDUCES, validarLotes, isDetalle:IsDetalle, isAdded:!withQuantity);
                });

                SetTotales();

            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        public void AgregarProducto(EntregasDetalleTemp producto)
        {
            try
            {
                bool isEditing = true;
                var parRowSinDialog = myParametro.GetParEntregasRepartidorUsarRowDeProductosSinDialog() || myParametro.GetParConducesUsarRowSinDialog();

                if (!myEnt.CantidadIsValida(producto.ProID, producto.Cantidad, producto.Posicion, producto.TraSecuencia, producto.CantidadSolicitada, producto.Lote, isEditing, producto.rowguid))
                {
                    if(myParametro.GetParNoEntregasParaciales())
                        DisplayAlert(AppResource.Warning, AppResource.QuantityDifferentFromRequestedQuantity);
                    else
                        DisplayAlert(AppResource.Warning, AppResource.QuantityExceedRequestedQuantity);

                    if (parRowSinDialog)
                    {
                        CargarProductos();
                    }
                    return;
                }

                var almId = Arguments.Values.CurrentModule == Modules.CONDUCES ? myParametro.GetParAlmacenIdParaDevolucion() : myParametro.GetParAlmacenIdParaDespacho();
                var parMultiAlmacenes = myParametro.GetParUsarMultiAlmacenes();
                                
                if (almId == -1 && parMultiAlmacenes)
                {
                    DisplayAlert(AppResource.Warning, AppResource.WarehouseIdNotConfigured);
                    if (parRowSinDialog)
                    {
                        CargarProductos();
                    }
                    return;
                }

                if (Arguments.Values.CurrentModule != Modules.RECEPCIONDEVOLUCION && (producto.Cantidad > 0 || producto.CantidadDetalle > 0) && (parMultiAlmacenes || myParametro.GetParCargasInventario()) && !myInv.HayExistencia(producto.ProID, producto.Cantidad, (int)producto.CantidadDetalle, almId))
                {                   
                    DisplayAlert(AppResource.Warning, AppResource.QuantityGreaterThanStock);
                    if (parRowSinDialog)
                    {
                        CargarProductos();
                    }
                    return;
                }

                myEnt.AgregarProducto(producto, Arguments.Values.CurrentModule == Modules.RECEPCIONDEVOLUCION, isEditing);

               // SearchValue = "";

                CargarProductos();

                //dialog?.Dismiss();

            }
            catch (Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message);
            }
        }

        private void GuardarFirma(List<int>secuencias, bool obligatory)
        {
            var modal = new FirmaModal("EntregasTransacciones", secuencias, 17);
            modal.signSaved = () => { AfterSave(true);  };
            modal.obligatory = obligatory;
            modal.signCancel = () => { AfterSave(true); };

            IsBusy = false;
            IsUp = true;
            PushModalAsync(modal);
        }
    }
}
