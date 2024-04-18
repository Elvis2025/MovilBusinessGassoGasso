
using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AgregarGastosModal : ContentPage
	{
        private DS_TransaccionesImagenes myTranImg;
        private DS_Gastos myGas;

        public Action<Gastos> OnAgregarGasto { get; set; }
        public Action OnEditarGasto { get; set; }

        public List<UsosMultiples> FormasPago { get; private set; }
        public List<UsosMultiples> TiposGastos { get; private set; }
        public List<UsosMultiples> CentrosDeCosto { get; private set; }

        private NcfContainer CurrentNCF = null;

        private Gastos editedgasto;
        public Gastos EditedGasto { get => editedgasto; set { editedgasto = value; ConfigEditedGasto();  } }

        public bool IsNotDetailing { get; set; } = true;
        public bool IsEditing { get => EditedGasto != null; }
        public bool UseCentroDeCosto { get; private set; }

        private int NextGasSecuencia { get; set; }

       // private bool IsConsulting = false;

        private NCFModal dialogNcf;

        private bool isEditingRealGasto = false;

		public AgregarGastosModal (DS_TransaccionesImagenes myTranImg, Gastos detailGasto = null, bool IsConsulting = false, bool isEditing = false)
		{
            //this.IsConsulting = IsConsulting;
            isEditingRealGasto = isEditing;

            this.myTranImg = myTranImg;
            myGas = new DS_Gastos();

            var myUso = new DS_UsosMultiples();

            IsNotDetailing = detailGasto == null || isEditing;

            NextGasSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Gastos");

            myTranImg.DeleteTemp(false, "Gastos", NextGasSecuencia.ToString());

            UseCentroDeCosto = DS_RepresentantesParametros.GetInstance().GetParGastosCapturarCentroDeCosto();
            FormasPago = myUso.GetFormasDePago();
            TiposGastos = myUso.GetTiposGastos();

            if (UseCentroDeCosto)
            {
                CentrosDeCosto = myUso.GetCentrosDeCostos();
            }

            BindingContext = this;

            InitializeComponent ();

            if (detailGasto != null)
            {
                EditedGasto = detailGasto;
            }

            if (IsConsulting && EditedGasto != null)
            {
                viewContainer.InputTransparent = true;
                lblTitle.Text = AppResource.ExpenseDetailNumber + EditedGasto.GasSecuencia;
                lblTitle.HorizontalOptions = LayoutOptions.StartAndExpand;
                lblTitle.Margin = new Thickness(10, 0, 0, 0);
                btnFoto.IsVisible = false;
            }
                        
            pickerFecha.Date = DateTime.Now;
            pickerFechaDocumento.Date = DateTime.Today.AddDays(1);

            NavigationPage.SetBackButtonTitle(this, AppResource.Back);

            ///Calcular Monto Total
            editMontoSinItbis.TextChanged += (sender, e) => CalcularMontotal();
            editItbis.TextChanged += (sender, e) => CalcularMontotal();
            editPropina.TextChanged += (sender, e) => CalcularMontotal();
        }

        private async void AgregarGasto(object sender, EventArgs args)
        {
            try
            {
                if (!ValidarDatos())
                {
                    return;
                }

                int gasTipo = -1;
                int formaPago = -1;
                string tipoDesc = null;
                string formaPagoDesc = null;

                double.TryParse(editMontoTotal.Text, out double montoTotal);

                double.TryParse(editMontoSinItbis.Text, out double montoSinItbis);
                double.TryParse(editBaseImponible.Text, out double montoSujetoaItbis);

                double.TryParse(editItbis.Text, out double itbis);
                double montoTotal2 = Convert.ToDouble((montoSinItbis + itbis).ToString("N2"));
                double.TryParse(editPropina.Text, out double propina);               

                if (comboTipoGasto.SelectedItem != null)
                {
                    if (comboTipoGasto.SelectedItem is UsosMultiples raw)
                    {
                        int.TryParse(raw.CodigoUso, out int tipo);
                        gasTipo = tipo;
                        tipoDesc = raw.Descripcion;
                    }
                }

                if (comboFormaPago.SelectedItem != null)
                {
                    if (comboFormaPago.SelectedItem is UsosMultiples raw)
                    {
                        int.TryParse(raw.CodigoUso, out int forma);
                        formaPago = forma;
                        formaPagoDesc = raw.Descripcion;
                    }
                }

                var centroCosto = comboCentroDeCosto.SelectedItem as UsosMultiples;

                if (EditedGasto != null)
                {
                    EditedGasto.GasFecha = pickerFecha.Date.ToString("yyyy-MM-dd");
                    EditedGasto.GasFechaDocumento = pickerFechaDocumento.Date.ToString("yyyy-MM-dd");
                    EditedGasto.GasRNC = editRnc.Text;
                    EditedGasto.GasNombreProveedor = editProveedor.Text;
                    EditedGasto.GasNCF = CurrentNCF.NCF;
                    EditedGasto.GasNCFFechaVencimiento = CurrentNCF.FechaVencimiento.ToString("yyyy-MM-dd");
                    EditedGasto.GasTipo = gasTipo;
                    EditedGasto.GasTipoDescripcion = tipoDesc;
                    EditedGasto.GasComentario = editComentario.Text;
                    EditedGasto.GasMontoTotal = montoTotal;
                    EditedGasto.GasMontoItebis = montoSinItbis;
                    EditedGasto.GasBaseImponible = montoSujetoaItbis;
                    EditedGasto.GasItebis = itbis;
                    EditedGasto.FopID = formaPago;
                    EditedGasto.GasNoDocumento = editNoFactura.Text;
                    EditedGasto.FopDescripcion = formaPagoDesc;
                    EditedGasto.GasPropina = propina;
                    EditedGasto.GasTipoComprobante = CurrentNCF.tipoCombrobanteNCF.ToString();

                    if (UseCentroDeCosto && centroCosto != null)
                    {
                        EditedGasto.GasCentroCosto = centroCosto.CodigoUso;
                    }

                    if (isEditingRealGasto)
                    {
                        //new DS_Gastos().GuardarGastos(new List<Gastos>() { EditedGasto }, myTranImg, true);
                        await SaveGasto(EditedGasto, true);
                    }

                    Dismiss(null, null);
                    OnEditarGasto?.Invoke();
                    Reset();
                    return;
                }

                Gastos gasto = new Gastos()
                {
                    GasSecuencia = NextGasSecuencia,
                    GasFecha = pickerFecha.Date.ToString("yyyy-MM-dd"),
                    GasFechaDocumento = pickerFechaDocumento.Date.ToString("yyyy-MM-dd"),
                    GasRNC = editRnc.Text.Trim(),
                    GasNombreProveedor = editProveedor.Text,
                    GasNCF = CurrentNCF.NCF,
                    GasNCFFechaVencimiento = CurrentNCF.FechaVencimiento.ToString("yyyy-MM-dd"),
                    GasTipo = gasTipo,
                    GasTipoDescripcion = tipoDesc,
                    GasComentario = editComentario.Text,
                    GasMontoTotal = montoTotal,
                    GasMontoItebis = montoSinItbis,
                    GasBaseImponible = montoSujetoaItbis,
                    GasItebis = itbis,
                    FopID = formaPago,
                    GasNoDocumento = editNoFactura.Text,
                    FopDescripcion = formaPagoDesc,
                    GasPropina = propina,
                    rowguid = Guid.NewGuid().ToString(),
                    GasCentroCosto = UseCentroDeCosto && centroCosto != null ? centroCosto.CodigoUso : null,
                    GasTipoComprobante = CurrentNCF.tipoCombrobanteNCF.ToString()
                };

                //new DS_Gastos().GuardarGastos(new List<Gastos>() { gasto }, myTranImg);

                await SaveGasto(gasto);

                Dismiss(null, null);
                //OnAgregarGasto?.Invoke(gasto);
                //Reset();

            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.ErrorSavingExpense, e.InnerException != null ? e.InnerException.Message : e.Message, AppResource.Aceptar);
            }

        }

        private async Task SaveGasto(Gastos gasto, bool isEditing = false)
        {
            var task = new TaskLoader() { SqlTransactionWhenRun = true };

            await task.Execute(() => { myGas.GuardarGastos(new List<Gastos>() { gasto }, myTranImg, isEditing); });

            await DisplayAlert(AppResource.Success, AppResource.ExpenseSavedCorrectly, AppResource.Aceptar);
        }

        private void CalcularMontotal()
        {
            double.TryParse(editMontoSinItbis.Text, out double montoSinItbis);
            double.TryParse(editItbis.Text, out double itbis);
            double.TryParse(editPropina.Text, out double propina);
            editMontoTotal.Text = (montoSinItbis + itbis + propina).ToString();
        }

        private bool ValidarDatos()
        {
            if (string.IsNullOrWhiteSpace(editProveedor.Text) || string.IsNullOrWhiteSpace(editRnc.Text) ||
                string.IsNullOrWhiteSpace(editNoFactura.Text) || string.IsNullOrWhiteSpace(editMontoTotal.Text) ||
                string.IsNullOrWhiteSpace(editMontoSinItbis.Text) || string.IsNullOrWhiteSpace(editBaseImponible.Text))
            {
                DisplayAlert(AppResource.Warning, AppResource.CompleteAllFieldsWarning, AppResource.Aceptar);
                return false;
            }

            if (pickerFechaDocumento.Date >= DateTime.Today.AddDays(1))
            {
                DisplayAlert(AppResource.Warning, "Debes de seleccionar una Fecha de Factura valida", AppResource.Aceptar);
                return false;
            }

            if (editRnc.Text.Trim().Length != 9 && editRnc.Text.Trim().Length != 11)           
            {
                DisplayAlert(AppResource.Warning, AppResource.RncMustHave9Digits, AppResource.Aceptar);
                return false;
            }

            if (!Functions.ValidarDocumento(editRnc.Text.Trim()))
            {
                DisplayAlert(AppResource.Warning, AppResource.RncNotValid, AppResource.Aceptar);
                return false;
            }

            if (CurrentNCF == null || CurrentNCF.NCF.Length != 11)
            {
                DisplayAlert(AppResource.Warning, AppResource.NcfDigitsWrong, AppResource.Aceptar);
                return false;
            }
            
            if (comboTipoGasto.SelectedItem == null || comboTipoGasto.SelectedIndex == -1)
            {
                DisplayAlert(AppResource.Warning, AppResource.SelectExpenseType, AppResource.Aceptar);
                return false;
            }

            if(comboFormaPago.SelectedItem == null || comboFormaPago.SelectedIndex == -1)
            {
                DisplayAlert(AppResource.Warning, AppResource.SelectPaymentway, AppResource.Aceptar);
                return false;
            }

            if(UseCentroDeCosto && (comboCentroDeCosto.SelectedItem == null || comboCentroDeCosto.SelectedIndex == -1))
            {
                DisplayAlert(AppResource.Warning, AppResource.SelectCostCenterWarning, AppResource.Aceptar);
                return false;
            }
            
            int gasTipo = -1;
            double.TryParse(editMontoTotal.Text, out double montoTotal);

            //double montoTotal = Convert.ToDouble(editMontoTotal.Text);
            // = Convert.ToDouble(editMontoSinItbis.Text);
            double.TryParse(editMontoSinItbis.Text, out double montoSinItbis);

            double.TryParse(editBaseImponible.Text, out double montoSujetoaItbis);

            // Convert.ToDouble(editItbis.Text);
            double.TryParse(editItbis.Text, out double itbis);

            // Convert.ToDouble(editPropina.Text);
            double.TryParse(editPropina.Text, out double propina);

            double montoTotal2 = Convert.ToDouble((montoSinItbis + itbis + propina).ToString("N2"));

            if (comboTipoGasto.SelectedItem != null)
            {
                int.TryParse(TiposGastos[comboTipoGasto.SelectedIndex].CodigoUso, out int tipo);
                gasTipo = tipo;
            }

            if (gasTipo == 1 && itbis > 0)
            {
                DisplayAlert(AppResource.Warning, AppResource.ExpenseTypeNotHaveItbis, AppResource.Aceptar);
                return false;
            }

            if (gasTipo != 1 && montoTotal != montoTotal2)
            {
                DisplayAlert(AppResource.Aceptar, AppResource.ExpenseTotalAmountNotEqual, AppResource.Aceptar);
                return false;
            }

            if (gasTipo == 1 && montoTotal != (montoSinItbis + propina))
            {
                DisplayAlert(AppResource.Warning, AppResource.ExpenseTotalAmountNotEqualWithoutItbis, AppResource.Aceptar);
                return false;
            }

            var fotosObligatorias = DS_RepresentantesParametros.GetInstance().GetParGastosCantidadFotoObligatoria();

            if (fotosObligatorias > 0 && myTranImg.GetCantidadImagenesInTemp("Gastos", DS_RepresentantesSecuencias.GetLastSecuencia("Gastos").ToString()) < fotosObligatorias)
            {
                throw new Exception(AppResource.MustTakeAtLeastPhotosWarning.Replace("@", fotosObligatorias.ToString()));
            }

            return true;
        }

        private void Reset()
        {
            comboTipoGasto.SelectedIndex = -1;
            comboFormaPago.SelectedIndex = -1;
            comboCentroDeCosto.SelectedIndex = -1;

            btnNcf.Text = AppResource.SetUp;
            editRnc.Text = "";
            editProveedor.Text = "";
            editNoFactura.Text = "";
            editMontoTotal.Text = "";
            editMontoSinItbis.Text = "";
            editBaseImponible.Text = "";
            editItbis.Text = "";
            editComentario.Text = "";
            CurrentNCF = null;
            EditedGasto = null;
            editPropina.Text = "";
            pickerFecha.Date = DateTime.Now;
            pickerFechaDocumento.Date = DateTime.Now;
        }

        private void Dismiss(object sender, EventArgs args)
        {
            myTranImg.DeleteTemp(false, "Gastos", NextGasSecuencia.ToString());

            Navigation.PopAsync(true);
        }

        private void GoCamera(object sender, EventArgs args)
        {
            try
            {
                if (EditedGasto == null)
                {
                    myTranImg.DeleteTemp(false, "Gastos", NextGasSecuencia.ToString());
                }

                Navigation.PushAsync(new CameraPage(NextGasSecuencia.ToString(), "Gastos"));

            }catch(Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }
        }

        private void ConfigNcf(object sender, EventArgs args)
        {
            if(dialogNcf == null)
            {
                dialogNcf = new NCFModal() { NcfAceptar = (s) => { CurrentNCF = s; btnNcf.Text = CurrentNCF.NCF; } };
            }

            if(CurrentNCF != null) 
            {
                dialogNcf.EditedNCF = CurrentNCF;
            }

            Navigation.PushModalAsync(dialogNcf);
        }

        private void ConfigEditedGasto()
        {
            if (!IsEditing)
            {
                return;
            }

            DateTime.TryParse(EditedGasto.GasFecha, out DateTime fecha);
            DateTime.TryParse(EditedGasto.GasFechaDocumento, out DateTime fechadocumento);
            DateTime.TryParse(EditedGasto.GasNCFFechaVencimiento, out DateTime fechaVencimiento);

            pickerFecha.Date = fecha;
            pickerFechaDocumento.Date = fechadocumento;
            editRnc.Text = EditedGasto.GasRNC;
            editProveedor.Text = EditedGasto.GasNombreProveedor;
            editNoFactura.Text = EditedGasto.GasNoDocumento;
            editMontoSinItbis.Text = EditedGasto.GasMontoItebis.ToString();
            editBaseImponible.Text = EditedGasto.GasBaseImponible.ToString();
            editItbis.Text = EditedGasto.GasItebis.ToString();
            editMontoTotal.Text = EditedGasto.GasMontoTotal.ToString();
            editPropina.Text = EditedGasto.GasPropina.ToString();
            editComentario.Text = EditedGasto.GasComentario;
            btnNcf.Text = EditedGasto.GasNCF;

            var formaPago = FormasPago.Where(x => x.CodigoUso == EditedGasto.FopID.ToString()).FirstOrDefault();
            comboFormaPago.SelectedItem = formaPago;

            var tipoGasto = TiposGastos.Where(x => x.CodigoUso == EditedGasto.GasTipo.ToString()).FirstOrDefault();
            comboTipoGasto.SelectedItem = tipoGasto;

            NextGasSecuencia = EditedGasto.GasSecuencia;

            if (UseCentroDeCosto)
            {
                var centro = CentrosDeCosto.Where(x => x.CodigoUso == EditedGasto.GasCentroCosto).FirstOrDefault();
                comboCentroDeCosto.SelectedItem = centro;
            }

            CurrentNCF = new NcfContainer() { FechaVencimiento = fechaVencimiento, NCF = EditedGasto.GasNCF };
        }
    }
}