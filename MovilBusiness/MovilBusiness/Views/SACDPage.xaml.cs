using MovilBusiness.Utils;
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
    public partial class SACDPage : ContentPage
    {
        public SACDPage()
        {
            BindingContext = new SacdViewModel(this);
            InitializeComponent();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (BindingContext is SacdViewModel vm && vm.ParGPS)
            {
                StopLocationsUpdates();
            }
        }

        private async void StopLocationsUpdates()
        {
            await Functions.StopListeningForLocations();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is SacdViewModel vm && vm.ParGPS)
            {
                Functions.StartListeningForLocations();
            }
        }
    }
}