using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Resx;
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
    public partial class LoginAuditorModal : ContentPage
    {
        private Action<string> onAuditorValidated { get; set; }
        

        public LoginAuditorModal(Action<string> onAuditorValidated)
        {
            this.onAuditorValidated = onAuditorValidated;

            InitializeComponent();
        }

        private async void Dismiss(object sender, EventArgs args)
        {
            await Navigation.PopModalAsync(false);
        }

        private void FocusPass(object sender, EventArgs args)
        {
            editPass.Focus();
        }

        private async void AttempLogin(object sender, EventArgs args)
        {
            try
            {
                var user = editUser.Text;
                var pass = editPass.Text;

                if (string.IsNullOrEmpty(user))
                {
                    await DisplayAlert(AppResource.Warning, AppResource.UserNotEmptyMessage, AppResource.Aceptar);
                    return;
                }

                if (string.IsNullOrWhiteSpace(pass))
                {
                    await DisplayAlert(AppResource.Warning, AppResource.PasswordNotEmptyWarning, AppResource.Aceptar);
                    return;
                }

                var rep = DS_Representantes.LogIn(user, pass, true);

                if(rep == null)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.IncorrectUserOrPassword, AppResource.Aceptar);
                    return;
                }

                if (DS_Representantes.RepresentateIsInactive(rep.RepCodigo))
                {
                    await DisplayAlert(AppResource.Warning, AppResource.UserIsInactive, AppResource.Aceptar);
                    return;
                }

                await Navigation.PopModalAsync(false);
                Arguments.CurrentAud = rep.RepCodigo;
                onAuditorValidated?.Invoke(rep.RepCodigo);
            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }
        }
    }
}