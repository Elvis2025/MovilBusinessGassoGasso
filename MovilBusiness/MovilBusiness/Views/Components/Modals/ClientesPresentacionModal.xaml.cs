using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Model;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ClientesPresentacionModal : ContentPage
	{
        private VisitasPresentacion CurrentDatos;

        private DS_Visitas myVis;

        public Action<VisitasPresentacion> OnSave { get; set; }

		public ClientesPresentacionModal (VisitasPresentacion visita = null)
		{
            myVis = new DS_Visitas();

            CurrentDatos = visita;

            InitializeComponent ();

            CargarDatos();

        }

       /* protected override bool OnBackButtonPressed()
        {
            return true;
        }*/

        private void CargarDatos()
        {
            if(CurrentDatos == null)
            {
                ClearComponents();
                return;
            }

            editNombre.Text = CurrentDatos.VisNombre;
            editPropietario.Text = CurrentDatos.VisPropietario;
            editContacto.Text = CurrentDatos.VisContacto;
            editEmail.Text = CurrentDatos.VisEmail;
            editCalle.Text = CurrentDatos.VisCalle;
            editCiudad.Text = CurrentDatos.VisCiudad;
            editTelefono.Text = CurrentDatos.VisTelefono;
            editRnc.Text = CurrentDatos.VisRNC;
        }

        private async void Dismiss(object sender, EventArgs args)
        {
            if(CurrentDatos == null)
            {
                //await DisplayAlert(AppResource.Warning, "Debes de completar los datos", AppResource.Aceptar);
              //  return;
            }

            await Navigation.PopModalAsync(false);
        }

        private void ClearComponents()
        {
            editNombre.Text = "";
            editPropietario.Text = "";
            editContacto.Text = "";
            editEmail.Text = "";
            editCalle.Text = "";
            editCiudad.Text = "";
            editTelefono.Text = "";
            editRnc.Text = "";
        }

        private async void Guardar(object sender, EventArgs args)
        {
            if (IsBusy)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(editNombre.Text))
            {
                await DisplayAlert(AppResource.Warning, AppResource.NameCannotBeEmpty, AppResource.Aceptar);
                return;
            }

            if (string.IsNullOrWhiteSpace(editPropietario.Text))
            {
                await DisplayAlert(AppResource.Warning, AppResource.OwnerCannotBeEmpty, AppResource.Aceptar);
                return;
            }

            if (string.IsNullOrWhiteSpace(editCalle.Text))
            {
                await DisplayAlert(AppResource.Warning, AppResource.StreetCannotBeEmpty, AppResource.Aceptar);
                return;
            }

            if (string.IsNullOrWhiteSpace(editCiudad.Text))
            {
                await DisplayAlert(AppResource.Warning, AppResource.CityCantBeEmpty, AppResource.Aceptar);
                return;
            }

            if (string.IsNullOrWhiteSpace(editTelefono.Text))
            {
                await DisplayAlert(AppResource.Warning, AppResource.PhoneCannotBeEmpty, AppResource.Aceptar);
                return;
            }

            if (string.IsNullOrWhiteSpace(editRnc.Text))
            {
                await DisplayAlert(AppResource.Warning, AppResource.RncCannotBeEmpty, AppResource.Aceptar);
                return;
            }

            if (!Functions.RncIsValid(editRnc.Text))
            {
                await DisplayAlert(AppResource.Warning, AppResource.RncMustHave9Digits, AppResource.Aceptar);
                return;
            }

            if (!string.IsNullOrWhiteSpace(editEmail.Text) && !Functions.ValidateEmail(editEmail.Text))
            {
                await DisplayAlert(AppResource.Warning, AppResource.EmailNotValid, AppResource.Aceptar);
                return;
            }

            IsBusy = true;

            try
            {
                var visita = new VisitasPresentacion();
                visita.VisNombre = editNombre.Text;
                visita.VisContacto = editContacto.Text;
                visita.VisPropietario = editPropietario.Text;
                visita.VisEmail = editEmail.Text;
                visita.VisCalle = editCalle.Text;
                visita.VisCiudad = editCiudad.Text;
                visita.VisTelefono = editTelefono.Text;
                visita.VisRNC = editRnc.Text;
                visita.VisSecuencia = Arguments.Values.CurrentVisSecuencia;
                visita.RepCodigo = Arguments.CurrentUser.RepCodigo;

                var task = new TaskLoader() { SqlTransactionWhenRun = true };

                await task.Execute(() => { myVis.GuardarVisitaPresentacion(visita, CurrentDatos != null); });

                await DisplayAlert(AppResource.Warning, AppResource.PresentationCustomerSavedCorrectly, AppResource.Aceptar);

                await Navigation.PopModalAsync(false);

                OnSave?.Invoke(visita);

            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
        }

        private void FocusNextField(object sender, EventArgs e)
        {
            if(sender != null && sender is Entry entry)
            {
                if(entry.BindingContext == null)
                {
                    return;
                }

                var Id = entry.BindingContext.ToString();

                if (string.IsNullOrEmpty(Id))
                {
                    return;
                }
                switch (Id)
                {
                    case "1":
                        editPropietario.Focus();
                        break;
                    case "2":
                        editContacto.Focus();
                        break;
                    case "3":
                        editEmail.Focus();
                        break;
                    case "4":
                        editCalle.Focus();
                        break;
                    case "5":
                        editCiudad.Focus();
                        break;
                    case "6":
                        editTelefono.Focus();
                        break;
                    case "7":
                        editRnc.Focus();
                        break;
                }
            }
        }
        
    }
}