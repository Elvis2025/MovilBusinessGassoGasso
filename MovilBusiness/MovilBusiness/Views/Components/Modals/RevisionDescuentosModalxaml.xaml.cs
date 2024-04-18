using MovilBusiness.DataAccess;
using MovilBusiness.Model.Internal;
using MovilBusiness.Resx;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class RevisionDescuentosModalxaml : ContentPage, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<ProductosRevisionDescuentos> productos;
        public ObservableCollection<ProductosRevisionDescuentos> Productos { get => productos; set { productos = value; RaiseOnPropertyChanged(); } }

        private ProductosRevisionDescuentos CurrentProduct;

        public List<double> CurrentPorcDescuentos;

        private double currentdescporciento = 0;
        public double CurrentDescPorciento { get => currentdescporciento; set { currentdescporciento = value;  RaiseOnPropertyChanged(); } }

        private bool isworking = false;
        public bool IsWorking { get => isworking; set { isworking = value; RaiseOnPropertyChanged(); } }

        private DS_Productos myProd;

        public Action OnProductsAccepted { get; set; }

        private bool somethingChange = false;
        public bool FromRevisionOfertas { get; set; } = false;

        private int titId;

        public RevisionDescuentosModalxaml (int titId)
		{
            this.titId = titId;
            myProd = new DS_Productos();

            Productos = new ObservableCollection<ProductosRevisionDescuentos>(myProd.GetProductosConDescuentos(titId));

            InitializeComponent ();

            BindingContext = this;

            somethingChange = false;
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        private void OnListItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            if (args.SelectedItem == null)
            {
                return;
            }

            CurrentProduct = args.SelectedItem as ProductosRevisionDescuentos;

            CurrentProductDescEditable = CurrentProduct.DescuentoManual;

            editDescValor.Text = CurrentProduct.DescuentoValorEditado.ToString();

            if(CurrentProduct.PorcDescuentoOriginal > 0)
            {
                CurrentPorcDescuentos = new List<double>();

                for (var i = 0; i <= CurrentProduct.PorcDescuentoOriginal; i++){
                    CurrentPorcDescuentos.Add(i);
                }
            }
            else
            {
                CurrentPorcDescuentos = new List<double>() { 0 };
            }

            if (CurrentProductDescEditable)
            {
                controlDescEditable.SelectedSegment = 1;
            }
            else
            {
                controlDescEditable.SelectedSegment = 0;
            }

            comboDescuento.ItemsSource = CurrentPorcDescuentos;

            var index = CurrentPorcDescuentos.IndexOf(CurrentProduct.PorcDescuentoEditado);

            comboDescuento.SelectedIndex = index;

            lblMessage.Text = AppResource.MaximunDiscountLabel + " " + CurrentProduct.DescuentoValorOriginal + "(" + CurrentProduct.PorcDescuentoOriginal + "%)" + "\n" + AppResource.EnterDiscountValue;

            dialogCantidad.IsVisible = true;

            list.SelectedItem = null;
        }

        private bool CurrentProductDescEditable = false;
        private void OnDescEditableChanged(object sender, int e)
        {
            try
            {
                if (CurrentProduct == null)
                {
                    return;
                }

                switch (e)
                {
                    case 0: //porcentual
                        comboDescuento.IsEnabled = true;
                        editDescValor.IsEnabled = false;
                        CurrentProductDescEditable = false;
                        //CurrentProduct.DescuentoManual = false;
                        break;
                    case 1: //editable
                        comboDescuento.IsEnabled = false;
                        editDescValor.IsEnabled = true;
                        CurrentProductDescEditable = true;
                        //CurrentProduct.DescuentoManual = true;
                        break;
                }
            }catch(Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        private async void AceptarProductos(object sender, EventArgs args)
        {
            try
            {
                if (IsWorking)
                {
                    return;
                }

                IsWorking = true;

                if (somethingChange)
                {
                    foreach (var producto in Productos)
                    {
                        myProd.ActualizarDescuentoInTemp(producto.ProID, producto.DescuentoValorEditado, producto.PorcDescuentoEditado, titId);
                    }
                }

                OnProductsAccepted?.Invoke();

                if (FromRevisionOfertas)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.ThereAreOfferToReviewMessage, AppResource.Aceptar);
                }

                await Navigation.PopModalAsync(false);
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }

            IsWorking = false;

        }

        private void OcultarDialogCantidad(object sender, EventArgs args)
        {
            dialogCantidad.IsVisible = false;
        }

        private void AceptarCantidad(object sender, EventArgs args)
        {
            try
            {
                double descuento = 0;

                if(controlDescEditable.SelectedSegment == 0) //porcentual
                {
                    descuento = CurrentDescPorciento;
                }
                else if(controlDescEditable.SelectedSegment == 1) //editable
                {
                    double.TryParse(editDescValor.Text, out double desc);
                    descuento = desc;
                }

                if(controlDescEditable.SelectedSegment == 0 && descuento > CurrentProduct.PorcDescuentoOriginal)
                {
                    DisplayAlert(AppResource.Warning, AppResource.DiscountPercentExceedMaximun, AppResource.Aceptar);
                    return;
                }

                if(controlDescEditable.SelectedSegment == 1 && descuento > CurrentProduct.DescuentoValorOriginal)
                {
                    DisplayAlert(AppResource.Warning, AppResource.RequestDiscountExceedMaximun, AppResource.Aceptar);
                    return;
                }

                var index = Productos.IndexOf(CurrentProduct);

                if(controlDescEditable.SelectedSegment == 0)
                {
                    CurrentProduct.PorcDescuentoEditado = descuento;
                    CurrentProduct.DescuentoValorEditado = CurrentProduct.Precio * (descuento / 100);
                }else if(controlDescEditable.SelectedSegment == 1)
                {
                    CurrentProduct.DescuentoValorEditado = descuento;
                    CurrentProduct.PorcDescuentoEditado = (descuento / CurrentProduct.Precio) * 100;
                }

                CurrentProduct.DescuentoManual = CurrentProductDescEditable;

                Productos[index] = CurrentProduct.Copy();

                OcultarDialogCantidad(null, null);

                somethingChange = true;
                
            }
            catch (Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }
        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}