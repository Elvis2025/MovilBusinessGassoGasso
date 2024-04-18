using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Resx;
using System;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AgregarProductoEntregaModal : ContentPage
	{
        private EntregasDetalleTemp CurrentProducto;
        private Action<EntregasDetalleTemp, AgregarProductoEntregaModal> OnProductAccepted;

        private DS_ProductosLotes myLot;
        private DS_EntregasRepartidorTransacciones myEnt;

        private bool IsRecepcionDevolucion;
        public bool IsEditing { get; set; } = false;

		public AgregarProductoEntregaModal (EntregasDetalleTemp producto, Action<EntregasDetalleTemp, AgregarProductoEntregaModal> onProductAccepted, bool isRecepcionDevolucion, bool IsEditing)
		{
            this.IsEditing = IsEditing;
            IsRecepcionDevolucion = isRecepcionDevolucion;
            CurrentProducto = producto;
            OnProductAccepted = onProductAccepted;

            myLot = new DS_ProductosLotes();
            myEnt = new DS_EntregasRepartidorTransacciones();

            InitializeComponent ();

            lblDescripcion.Text = producto.ProDescripcion;
            lblCantidadSolicitada.Text = producto.CantidadSolicitada.ToString();

            if(producto.Cantidad > 0)
            {
                editCantidad.Text = producto.Cantidad.ToString();
            }

            if(producto.UsaLote && !string.IsNullOrWhiteSpace(producto.Lote))
            {
                editLote.Text = producto.Lote;
            }

            if (!producto.UsaLote)
            {
                lblLote.IsVisible = false;
                editLote.IsVisible = false;
            }

            lblEditando.IsVisible = IsEditing;

            LoadViewsRecepcionDevolucion();
        }

        private void LoadViewsRecepcionDevolucion()
        {
            if (!IsRecepcionDevolucion)
            {
                return;
            }

            lblMotivo.IsVisible = true;
            lblFechaVencimiento.IsVisible = true;
            lblFactura.IsVisible = true;
            pickerMotivo.IsVisible = true;
            pickerFechaVencimiento.IsVisible = true;
            editFactura.IsVisible = true;
            lblEstado.IsVisible = true;
            comboEstado.IsVisible = true;

            pickerMotivo.SelectedIndexChanged += PickerMotivo_SelectedIndexChanged;

            pickerFechaVencimiento.Date = DateTime.Now;

            var motivos = new DS_Devoluciones().GetMotivosDevolucion();

            pickerMotivo.ItemsSource = motivos;

            if (!string.IsNullOrWhiteSpace(CurrentProducto.Documento))
            {
                editFactura.Text = CurrentProducto.Documento;
            }

            if(CurrentProducto.MotIdDevolucion != 0)
            {
                var motivo = motivos.Where(x => x.MotID == CurrentProducto.MotIdDevolucion).FirstOrDefault();

                if(motivo != null)
                {
                    var index = motivos.IndexOf(motivo);

                    pickerMotivo.SelectedIndex = index;

                    if(!string.IsNullOrWhiteSpace(motivo.MotCaracteristicas) && motivo.MotCaracteristicas.ToUpper().Contains("M"))
                    {
                        comboEstado.IsEnabled = false;
                    }
                }

            }

            comboEstado.SelectedIndex = CurrentProducto.IndicadorMalEstado ? 1 : 0;

            if (!string.IsNullOrWhiteSpace(CurrentProducto.FechaVencimiento))
            {
                DateTime.TryParse(CurrentProducto.FechaVencimiento, out DateTime fecha);
                pickerFechaVencimiento.Date = fecha;
            }

        }

        private void PickerMotivo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(pickerMotivo.SelectedItem == null)
            {
                comboEstado.IsEnabled = true;
                return;
            }


            if (!(pickerMotivo.SelectedItem is MotivosDevolucion motivo))
            {
                comboEstado.IsEnabled = true;
                return;
            }

            if (!string.IsNullOrWhiteSpace(motivo.MotCaracteristicas) && motivo.MotCaracteristicas.ToUpper().Contains("M"))
            {
                comboEstado.IsEnabled = false;
                comboEstado.SelectedIndex = 1;
            }
            else
            {
                comboEstado.IsEnabled = true;
            }
        }

        private void AceptarProducto(object sender, EventArgs args)
        {
            double.TryParse(editCantidad.Text, out double cantidad);

            if(CurrentProducto.UsaLote && string.IsNullOrWhiteSpace(editLote.Text))
            {
                DisplayAlert(AppResource.Warning, AppResource.MustSpecifyLotWarning, AppResource.Aceptar);
                return;
            }

            if (CurrentProducto.UsaLote && myLot.GetByProIDAndLote(editLote.Text, CurrentProducto.ProID) == null)
            {
                bool parProNoValLote = DS_RepresentantesParametros.GetInstance().GetParProductoNoValidarLote();
                if (!parProNoValLote)
                {
                    DisplayAlert(AppResource.Warning, AppResource.TheLotIsNotValid, AppResource.Aceptar);
                    return;
                }
            }

            if (IsRecepcionDevolucion)
            {
                if(pickerMotivo.SelectedItem == null)
                {
                    DisplayAlert(AppResource.Warning, AppResource.MustSelectReasonOfReturn, AppResource.Aceptar);
                    return;
                }
            }

            var item = CurrentProducto.Copy();
            item.Cantidad = cantidad;
            item.Lote = editLote.Text;

            if (IsRecepcionDevolucion)
            {
                item.FechaVencimiento = pickerFechaVencimiento.Date.ToString("yyyy-MM-dd HH:mm:ss");
                item.Documento = editFactura.Text;
                item.MotIdDevolucion = (pickerMotivo.SelectedItem as MotivosDevolucion).MotID;
                item.MotDescripcion = (pickerMotivo.SelectedItem as MotivosDevolucion).MotDescripcion;
                item.IndicadorMalEstado = comboEstado.SelectedIndex == 1;
            }

            if(item.UsaLote && !myEnt.ExistsInTempWithLote(item.ProID, item.Posicion, item.Lote, item.TraSecuencia))
            {
                IsEditing = false;
            }

            OnProductAccepted?.Invoke(item, this);
        }

        public async void Dismiss(object sender = null, EventArgs args = null)
        {
            await Navigation.PopModalAsync(false);
        }

        private void FillCantidad(object sender, EventArgs e)
        {
            if(CurrentProducto == null)
            {
                return;
            }

            editCantidad.Text = CurrentProducto.CantidadSolicitada.ToString();
        }
    }
}