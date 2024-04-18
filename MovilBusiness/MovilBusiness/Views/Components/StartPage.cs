using MovilBusiness.configuration;
using MovilBusiness.views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace MovilBusiness.Views.Components
{
    public class StartPage : ContentPage
    {

        protected override void OnAppearing()
        {
            if (Arguments.CurrentUser != null)
            {
                Navigation.PushAsync(new HomePage());
            }
            else
            {
                Navigation.PushAsync(new LoginPage());
            }

            base.OnAppearing();
        }
    }
}
