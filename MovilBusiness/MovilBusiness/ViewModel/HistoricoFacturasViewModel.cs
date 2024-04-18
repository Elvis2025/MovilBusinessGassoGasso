using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Model;
using MovilBusiness.Resx;
using MovilBusiness.viewmodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class HistoricoFacturasViewModel : BaseViewModel
    {
        private DS_HistoricoFacturas myHis;

        public ICommand AceptarProductosCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public bool IsDevolucion { get; private set; }

        private bool productosconcantidad;
        public bool ProductosConCantidad { get => productosconcantidad; set { productosconcantidad = value; RaiseOnPropertyChanged(); } }

        private readonly bool HistoricoPedidos = false;

        public ObservableCollection<HistoricoFacturas> Documentos { get; set; }

        private List<HistoricoFacturasDetalle> detalles;
        public List<HistoricoFacturasDetalle> Detalles { get => detalles; set { detalles = value; RaiseOnPropertyChanged(); } }

        public Action onCancel { get; set; }
        public Action<string> onAceptarProductos { get; set; }

        private HistoricoFacturas CurrentFactura = null;

        public HistoricoFacturasViewModel(Page page, bool HistoricoPedidos, bool isForDevolucion = false) : base(page)
        {
            IsDevolucion = isForDevolucion;

            AceptarProductosCommand = new Command(AceptarProductosParaDevolucion);
            CancelCommand = new Command(Cancel);

            this.HistoricoPedidos = HistoricoPedidos;

            myHis = new DS_HistoricoFacturas();

            if (DS_RepresentantesParametros.GetInstance().GetParHistoricoFacturasFromCuentasxCobrar() && IsDevolucion)
            {
                Documentos = myHis.GetHistoricoByClienteAndCuentasxCobrar(Arguments.Values.CurrentClient.CliID);
            }
            else
            {
                Documentos = HistoricoPedidos ? myHis.GetHistoricoPedidos(Arguments.Values.CurrentClient.CliID) : myHis.GetHistoricoByCliente(Arguments.Values.CurrentClient.CliID, myParametro.GetParTakeLastFacturasInDevoluciones(), myParametro.GetParDevolucionesFacturasLimiteDias());
            }
  

        }

        public void OnDocumentoSelected(HistoricoFacturas doc)
        {
            try
            {
                CurrentFactura = doc;

                if (HistoricoPedidos)
                {
                    Detalles = myHis.GetHistoricoPedidosDetalle(doc.idReferencia, doc.RepCodigo);
                }
                else
                {
                    if (DS_RepresentantesParametros.GetInstance().GetParHistoricoFacturasFromCuentasxCobrar() && IsDevolucion)
                    {
                        if (doc.idReferencia.Contains("B"))
                        {
                            Detalles = myHis.GetHistoricoFacturasDetalleByVentasDetallexNCF(doc.idReferencia, doc.RepCodigo);
                        }
                        else
                        {
                            Detalles = myHis.GetHistoricoFacturasDetalleByVentasDetallexNumeroErp(doc.idReferencia, doc.RepCodigo);
                        }

                        
                    }
                    else
                    {
                        Detalles = myHis.GetHistoricoFacturasDetalle(doc.idReferencia, doc.RepCodigo);
                    }


                }
            }catch(Exception e)
            {

                DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }
           
        }

        private void AceptarProductosParaDevolucion()
        {
            if(CurrentFactura == null)
            {
                DisplayAlert(AppResource.Warning, AppResource.MustSpecifyInvoiceToContinue);
                return;
            }

            if (DS_RepresentantesParametros.GetInstance().GetParHistoricoFacturasFromCuentasxCobrar() && IsDevolucion)
            {
                if (CurrentFactura.idReferencia.Contains("B"))
                {
                    myHis.InsertProductosInTempByVentasDetallexNCF(CurrentFactura.idReferencia, ProductosConCantidad, (int)Arguments.Values.CurrentModule);
                }
                else
                {
                    myHis.InsertProductosInTempByVentasDetallexNumeroERP(CurrentFactura.idReferencia, ProductosConCantidad, (int)Arguments.Values.CurrentModule);
                }

                
            }
            else
            {
                myHis.InsertProductosInTemp(CurrentFactura.idReferencia, ProductosConCantidad, (int)Arguments.Values.CurrentModule);
            }

            

            onAceptarProductos?.Invoke(CurrentFactura.idReferencia);

            PopModalAsync(false);
        }

        private async void Cancel()
        {
            var result = await DisplayAlert(AppResource.Warning, AppResource.WantToLeaveQuestion, AppResource.Leave, AppResource.Cancel);

            if (result)
            {
                await PopModalAsync(false);
                onCancel?.Invoke();
            }
            
        }
    }
}
