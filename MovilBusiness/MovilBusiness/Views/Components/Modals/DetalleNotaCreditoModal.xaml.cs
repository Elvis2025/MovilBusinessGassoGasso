
using MovilBusiness.DataAccess;
using MovilBusiness.model.Internal;
using MovilBusiness.model.Internal.Structs.Args;
using MovilBusiness.Resx;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DetalleNotaCreditoModal : ContentPage, INotifyPropertyChanged
    {
        private DS_Recibos myRec;

        public new event PropertyChangedEventHandler PropertyChanged;

        private RecibosDocumentosTemp notacredito;
        public RecibosDocumentosTemp NotaCredito { get => notacredito; set { notacredito = value; LoadFacturas(); RaiseOnPropertyChanged(); } }

        private RecibosDocumentosTemp currentfactura;
        public RecibosDocumentosTemp CurrentFactura { get => currentfactura; set { currentfactura = value; OnCurrentFacturaChanged(); RaiseOnPropertyChanged(); } }

        private ObservableCollection<RecibosDocumentosTemp> facturas;
        public ObservableCollection<RecibosDocumentosTemp> Facturas { get => facturas; set { facturas = value; RaiseOnPropertyChanged(); } }

        private bool PermiteSplit = false;

        //public Action OnCancel { get; set; }
        public Action<AplicarNCArgs> OnAccepted { get; set; }

        public DetalleNotaCreditoModal ()
		{
            myRec = new DS_Recibos();

            InitializeComponent ();

            BindingContext = this;
		}

        private void AceptarNC(object sender, EventArgs e)
        {
            if(CurrentFactura == null)
            {
                DisplayAlert(AppResource.Warning, AppResource.MustSelectInvoiceToApply, AppResource.Aceptar);
                return;
            }

            if (DS_RepresentantesParametros.GetInstance().GetParNotaCreditoAutoFactura())
            {
                if (!string.IsNullOrWhiteSpace(NotaCredito.CXCNCFAfectado) && NotaCredito.CXCNCFAfectado != CurrentFactura.CXCNCF)
                {
                    DisplayAlert(AppResource.Warning, AppResource.InvoiceBelongsToAnotherDocument, AppResource.Aceptar);
                    return;
                }
            }

            var args = new AplicarNCArgs() { NC = NotaCredito, Factura = CurrentFactura };


            if (PermiteSplit)
            {
                double.TryParse(editMonto.Text, out double monto);
                decimal montoAbs = decimal.Parse(Math.Abs(monto).ToString());
                decimal ncBalance = decimal.Parse(Math.Abs(NotaCredito.Balance).ToString());
                decimal ncAplicado = decimal.Parse(Math.Abs(NotaCredito.Aplicado).ToString());
                decimal facAplicado = decimal.Parse(Math.Abs(CurrentFactura.Aplicado).ToString());
                decimal facAplicadoSinDPP = facAplicado + decimal.Parse(Math.Abs(CurrentFactura.Descuento).ToString());

                if (montoAbs <= 0)
                {
                    DisplayAlert(AppResource.Warning, AppResource.AmountMustBeGreaterThanZero, AppResource.Aceptar);
                    return;
                }

                if (montoAbs > (ncBalance - ncAplicado) )
                {
                    DisplayAlert(AppResource.Warning, AppResource.AmountAppliedLessThanPending, AppResource.Aceptar);
                    return;
                }

                if(montoAbs > facAplicadoSinDPP)
                {
                    DisplayAlert(AppResource.Warning, AppResource.AmountAppliedLessThanBalance + facAplicadoSinDPP.ToString("N2"), AppResource.Aceptar);
                    return;
                }

                args.ValorAplicarManual = Math.Abs(monto);
            }
            else
            {
                args.ValorAplicarManual = NotaCredito.Pendiente;
            }

            Navigation.PopModalAsync(false);

            OnAccepted?.Invoke(args);
        }

        private void LoadFacturas()
        {
            PermiteSplit = DS_RepresentantesParametros.GetInstance().GetParRecibosSplitNotasDeCredito(NotaCredito.Referencia, out bool allowEditAmount);

            Facturas = new ObservableCollection<RecibosDocumentosTemp>(myRec.GetDocumentsInTemp(true, PermiteSplit));

            if(Facturas != null && Facturas.Count > 0)
            {
                CurrentFactura = Facturas[0];
            }

            editMonto.IsEnabled = allowEditAmount;
        }

        private void OnCurrentFacturaChanged()
        {
            if (!PermiteSplit)
            {
                return;
            }

            if(CurrentFactura == null)
            {
                editMonto.Text = "";
                return;
            }

            var valorNc = NotaCredito.Balance - NotaCredito.Aplicado;

            var balanceFact = CurrentFactura.Balance;

            if(valorNc > balanceFact)
            {
                editMonto.Text = balanceFact.ToString();
            }
            else
            {
                editMonto.Text = valorNc.ToString();
            }
        }

        private void Dismiss(object sender, EventArgs args)
        {
            ClearValues();
            Navigation.PopModalAsync(false);
        }

        public void ClearValues()
        {
            Facturas = null;
            CurrentFactura = null;
        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}