using MovilBusiness.Configuration;
using MovilBusiness.Resx;
using MovilBusiness.Services;
using MovilBusiness.Utils;
using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ChangePasswordModal : ContentPage, INotifyPropertyChanged
	{
        public new event PropertyChangedEventHandler PropertyChanged;

        private string oldpass;
        public string OldPass { get => oldpass; set { oldpass = value; RaiseOnPropertyChanged(); } }

        private string newpass;
        public string NewPass { get => newpass; set { newpass = value; RaiseOnPropertyChanged(); } }

        private string newpassconfirmed;
        public string NewPassConfirmed { get => newpassconfirmed; set { newpassconfirmed = value; RaiseOnPropertyChanged(); } }

        private bool isbusy;
        public new bool IsBusy { get => isbusy; set { isbusy = value; RaiseOnPropertyChanged(); } }

        private Action passwordChanged;

        public ChangePasswordModal (Action passwordChanged)
		{
            BindingContext = this;
            this.passwordChanged = passwordChanged;
			InitializeComponent ();
		}

        private void Dismiss(object sender, EventArgs args)
        {
            Navigation.PopModalAsync(false);
        }

        private async void AttempChangePassword(object sender, EventArgs args)
        {
            try
            {
                if (!CrossConnectivity.Current.IsConnected)
                {
                    throw new Exception(AppResource.NoInternetMessage);
                }

                if (string.IsNullOrEmpty(OldPass))
                {
                    throw new Exception(AppResource.CurrentPasswordCannotBeEmpty);
                }

                if (string.IsNullOrEmpty(NewPass))
                {
                    throw new Exception(AppResource.NewPasswordCannotBeEmpty);
                }

                if (string.IsNullOrWhiteSpace(NewPassConfirmed))
                {
                    throw new Exception(AppResource.MustConfirmNewPassword);
                }

                if (!OldPass.Equals(Arguments.CurrentUser.RepClave))
                {
                    throw new Exception(AppResource.CurrentPasswordIsIncorrect);
                }

                if (!NewPass.Equals(NewPassConfirmed))
                {
                    throw new Exception(AppResource.NewPasswordNotMatch);
                }

                IsBusy = true;

                await new ServicesManager().CambiarContraseñaUsuario(OldPass, NewPass, Arguments.CurrentUser.RepCodigo);

                Arguments.CurrentUser.RepClave = NewPass;

                await DisplayAlert(AppResource.Success, AppResource.PasswordChangedSuccessfullyMessage, AppResource.Aceptar);

                passwordChanged?.Invoke();

                await Navigation.PopModalAsync(true);

            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Error, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}