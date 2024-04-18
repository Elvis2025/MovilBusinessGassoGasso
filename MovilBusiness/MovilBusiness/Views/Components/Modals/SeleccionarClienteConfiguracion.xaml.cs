using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.Resx;
using MovilBusiness.viewmodel;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SeleccionarClienteConfiguracion : ContentPage
	{
        public Action<Clientes> OnValueSelected { private get; set; }    

       
        public  SeleccionarClienteConfiguracion()
		{
           

            BindingContext = new ClientesViewModel(this, false) { FiltroEstatusVisitas = FiltroEstatusVisitaClientes.TODOS };

			InitializeComponent ();
            return;
		}

        private void Dismiss(object sender, EventArgs args)
        {
            Navigation.PopModalAsync(false);
        }

        public void List_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            try
            {
                if (e.SelectedItem == null)
                {
                    return;
                }

                ShowAlertConfirm(e.SelectedItem as Clientes);


                list.SelectedItem = null;
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.Warning, ex.Message, AppResource.Aceptar);
            }

        }

        private async void ShowAlertConfirm(Clientes item)
        {
            var result = await DisplayAlert(AppResource.Warning, AppResource.WantSelectCustomerQuestion, AppResource.Yes, AppResource.No);

            if (!result)
            {
                return;
            }

            OnValueSelected?.Invoke(item);

            await Navigation.PopModalAsync(false);
           

        }
    }
}