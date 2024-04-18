using MovilBusiness.Resx;
using MovilBusiness.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ConfigurationPage : ContentPage
	{
		public ConfigurationPage (bool configConexion = false)
		{
            BindingContext = new ConfigurationViewModel(this);

			InitializeComponent ();

            if (configConexion)
            {
                //table.Root[0].RemoveAt(3);
                table.Root[0].RemoveAt(2);
                table.Root.RemoveAt(1); //sync para pruebas

                // Se comenta lo de abajo porque estaba dando error de fuera de rango y se desconoce la funcionalidad del mismo.
                //table.Root.RemoveAt(1); // orientacion de pantalla  
            }
        }

        private void Conexion_tapped(object sender, EventArgs e)
        {
            ((ConfigurationViewModel)BindingContext).SeleccionarConexion();
        }

        private void Webservice_tapped(object sender, EventArgs e)
        {
            ((ConfigurationViewModel)BindingContext).EditarWebServiceUrl();
        }

        private void Replicacion_tapped(object sender, EventArgs e)
        {
            ((ConfigurationViewModel)BindingContext).EditarReplicacion();
        }

        private void Cantidad_tapped(object sender, EventArgs e)
        {
            ((ConfigurationViewModel)BindingContext).EditarCantidadDatosPorCiclo();
        }

        private void TestSync_Tapped(object sender, EventArgs e)
        {
            ((ConfigurationViewModel)BindingContext).SyncTest = !((ConfigurationViewModel)BindingContext).SyncTest;
        }

        private void Version_Replicacion_tapped(object sender, EventArgs args)
        {
            ((ConfigurationViewModel)BindingContext).EditarVersionDeLaReplicacion();
        }

        private async void ContarRegistrosDb(object sender, EventArgs args)
        {
            try
            {
                await Navigation.PushAsync(new RegistrosBaseDatosPage());
            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }
            
        }

        /*private void CambiarOrientacion(object sender, EventArgs e)
        {
            ((ConfigurationViewModel)BindingContext).CambiarOrientacionPantalla();
        }*/
    }
}