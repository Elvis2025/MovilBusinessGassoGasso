using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using MovilBusiness.views;
using Plugin.Fingerprint;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LoginPage : MasterDetailPage
	{
        public static bool Finish = false;

		public LoginPage ()
		{
            BindingContext = new LoginViewModel(this);

            InitializeComponent();

            if (DS_RepresentantesParametros.GetInstance().GetParHuellaForLogin() && DeviceInfo.Platform.ToString() == "iOS")
            {
                if (Application.Current.Resources["ModelsIos"].ToString().Contains(DeviceInfo.Model))
                {
                    lblbutton.ImageSource = "Huella.png";
                }
                else
                {
                    lblbutton.ImageSource = "BlueFaceid.png";
                }
            }

            NavigationPage.SetBackButtonTitle(this, AppResource.Start);
        }

        private void OnOptionMenuItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            try
            {
                if (args.SelectedItem == null)
                {
                    return;
                }

                if (!(BindingContext is LoginViewModel vm))
                {
                    return;
                }

                vm.OnOptionMenuItemSelected((model.Internal.MenuItem)args.SelectedItem);

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            drawerLayout.SelectedItem = null;

            IsPresented = false;
        }

        private void HandleMenuVisibility(object sender, EventArgs args)
        {
            IsPresented = !IsPresented;
        }

        protected async override void OnAppearing()
        {
            try
            {
                if (Arguments.CurrentUser != null)
                {
                    await Navigation.PushAsync(new HomePage());
                }
            }
            catch(Exception e)
            {
                Console.Write(e.Message);
            }
        }
    }
}