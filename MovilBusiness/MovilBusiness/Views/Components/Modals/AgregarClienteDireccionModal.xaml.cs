using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Model;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AgregarClienteDireccionModal : ContentPage
    {
        private Action onAddressCreated;
        private DS_Clientes myCli;

        public AgregarClienteDireccionModal(Action onAddressCreated, DS_Clientes myCli)
        {
            this.myCli = myCli;
            this.onAddressCreated = onAddressCreated;
            InitializeComponent();
        }

        private void Dismiss(object sender, EventArgs args)
        {
            Navigation.PopModalAsync(false);
        }

        private async void Save(object sender, EventArgs args)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(editCalle.Text))
                {
                    await DisplayAlert(AppResource.Warning, AppResource.StreetCannotBeEmpty, AppResource.Aceptar);
                    return;
                }

                var dir = new ClientesDirecciones();
                dir.CldCalle = editCalle.Text;
                dir.CldCasa = editCasa.Text;
                dir.CldSector = editSector.Text;
                dir.CldTelefono = editTelefono.Text;
                dir.CldContacto = editContacto.Text;
                dir.CliiD = Arguments.Values.CurrentClient.CliID;

                myCli.GuardarClienteDireccion(dir);

                await DisplayAlert(AppResource.Success, AppResource.AddressSavedSuccessfully, AppResource.Aceptar);

                await Navigation.PopModalAsync(false);

                onAddressCreated?.Invoke();

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.Error, e.Message, AppResource.Aceptar);
            }
        }
    }
}