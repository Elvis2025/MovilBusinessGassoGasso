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
    public partial class ConsultaEntregasRepartidorPage : ContentPage
    {
        private bool FirstTime = true;

        public ConsultaEntregasRepartidorPage(int enrSecuencia, bool isConfirmado)
        {
            BindingContext = new ConsultaEntregasRepartidorViewModel(this, enrSecuencia, isConfirmado);

            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (FirstTime && BindingContext is ConsultaEntregasRepartidorViewModel vm)
            {
                FirstTime = false;
                vm.CargarFacturas();
            }
        }
    }
}