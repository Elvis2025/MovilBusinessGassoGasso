using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.Model;
using MovilBusiness.Resx;
using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AgregarOperativoDetalleModal : ContentPage
    {
        private List<OperativosDetalleProductos> productos;

        private DS_OperativosMedicos myOpe;
        private DS_Inventarios myInv;

        private OperativosDetalleProductos CurrentProduct;

        private Action<OperativosDetalle> OnDetalleAdded;

        public AgregarOperativoDetalleModal(Action<OperativosDetalle> onDetalleAdded, List<Productos> filtrados)
        {
            OnDetalleAdded = onDetalleAdded;
            myOpe = new DS_OperativosMedicos();
            myInv = new DS_Inventarios();

            productos = myOpe.GetProductosForOperativos(DS_RepresentantesParametros.GetInstance().GetParCargasInventario(), filtrados);

            InitializeComponent();

            list.ItemsSource = productos;
            comboEspecialidad.ItemsSource = new DS_Especialidades().GetEspecialidades();
        }

        private void AgregarDetalle(object sender, EventArgs e)
        {
            try
            {
                var productosAgregados = productos.Where(x => x.OpeProductoCantidad > 0).ToList();

                if (productosAgregados.Count == 0)
                {
                    DisplayAlert(AppResource.Warning, AppResource.YouHaveNotAddAnyProductWarning, AppResource.Aceptar);
                    return;
                }

                var det = new OperativosDetalle();
                det.OpePacienteEmail = editEmail.Text;
                det.OpePacienteNombre = editNombre.Text;
                det.OpePacienteTelefono = editTelefono.Text;
                det.EspID = comboEspecialidad.SelectedItem != null ? (comboEspecialidad.SelectedItem as Especialidades).EspID : -1;

                if (string.IsNullOrWhiteSpace(editDoctor.Text))
                {
                    DisplayAlert(AppResource.Warning, AppResource.MustSpecifyDoctorNameWarning, AppResource.Aceptar);
                    return;
                }

                if(comboEspecialidad.SelectedItem != null && string.IsNullOrEmpty(editDoctor.Text))
                {
                    DisplayAlert(AppResource.Warning, AppResource.MustSpecifyDoctorNameIfHaveSpecialty, AppResource.Aceptar);
                    return;
                }

                if (string.IsNullOrWhiteSpace(det.OpePacienteTelefono))
                {
                    det.OpePacienteTelefono = "";
                }

                det.OpeSector = editSector.Text;
                det.rowguid = Guid.NewGuid().ToString();
                det.OpeNombreDoctor = editDoctor.Text;

                foreach (var p in productosAgregados)
                {
                    // det.ProductosDesc += string.IsNullOrWhiteSpace(det.ProductosDesc) ? p.ProDescripcion : ", " + p.ProDescripcion;     
                    det.ProductosDesc += Environment.NewLine + p.ProDescripcion + Environment.NewLine + AppResource.DeliveredLabel + " " + p.OpeProductoCantidad.ToString() + "   "+AppResource.PrescribedLabel + " " + p.OpeProductoCantidadPrescrita.ToString();

                }

                det.Productos = productosAgregados;

                OnDetalleAdded?.Invoke(det);

                editCantidad.Unfocus();

                Navigation.PopModalAsync(false);

            }catch(Exception xe)
            {
                DisplayAlert(AppResource.Warning, xe.Message, AppResource.Aceptar);
            }
        }

        private async void Dismiss(object sender, EventArgs args)
        {
            editCantidad.Unfocus();
            await Navigation.PopModalAsync(false);
        }

        private void ShowLayoutPaciente(object sender, EventArgs e)
        {
            layoutPaciente.IsVisible = !layoutPaciente.IsVisible;
        }

        private void list_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            try
            {
                if (e.SelectedItem == null)
                {
                    return;
                }

                CurrentProduct = (e.SelectedItem as OperativosDetalleProductos);

                if (CurrentProduct == null)
                {
                    return;
                }

                LoadProductData();

                editCantidad.Focus();
                dialogAddCantidad.IsVisible = true;

                list.SelectedItem = null;

            }catch(Exception ex)
            {
                DisplayAlert(AppResource.Warning, ex.Message, AppResource.Aceptar);
            }
        }

        private void LoadProductData()
        {
            lblProductName.Text = CurrentProduct.ProDescripcion;
            editCantidad.Text = CurrentProduct.OpeProductoCantidad > 0 ? CurrentProduct.OpeProductoCantidad.ToString() : "";
            editCantidadPrescrita.Text = CurrentProduct.OpeProductoCantidadPrescrita > 0 ? CurrentProduct.OpeProductoCantidadPrescrita.ToString() : "";
        }

        private void DismissDialogCantidad(object sender, EventArgs e)
        {
            dialogAddCantidad.IsVisible = false;
            editCantidad.Unfocus();
        }

        private void AgregarCantidad(object sender, EventArgs e)
        {
            try
            {
                if (!int.TryParse(editCantidad.Text, out int cantidad))
                {
                    DisplayAlert(AppResource.Warning, AppResource.QuantityNotValid, AppResource.Aceptar);
                    return;
                }

                if (DS_RepresentantesParametros.GetInstance().GetParCargasInventario() && !myInv.HayExistencia(CurrentProduct.ProID, cantidad, out Inventarios existencia))
                {
                    DisplayAlert(AppResource.Warning, AppResource.QuantityGreaterThanInventoryWarning +" " + existencia.invCantidad.ToString(), AppResource.Aceptar);
                    return;
                }

                int.TryParse(editCantidadPrescrita.Text, out int cantidadPrescrita);

                var index = productos.IndexOf(CurrentProduct);

                var item = CurrentProduct.Copy();
                item.OpeProductoCantidad = cantidad;
                item.OpeProductoCantidadPrescrita = cantidadPrescrita;

                productos[index] = item;

                list.ItemsSource = new List<OperativosDetalleProductos>(productos);

                dialogAddCantidad.IsVisible = false;

            }catch(Exception ex)
            {
                DisplayAlert(AppResource.Warning, ex.Message, AppResource.Aceptar);
            }
        }
    }
}