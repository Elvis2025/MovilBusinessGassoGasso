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
    public partial class AsignacionRutasDetallePage : ContentPage
    {
        private bool FirstTime = true;

        public AsignacionRutasDetallePage()
        {
            BindingContext = new AsignacionRutasDetalleViewModel(this);

            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (FirstTime && BindingContext is AsignacionRutasDetalleViewModel vm)
            {
                FirstTime = false;

                vm.CargarRuta();
            }
        }

        private void List_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if(e.SelectedItem == null)
            {
                return;
            }

            list.SelectedItem = null;
        }

        private void TapUpRow(object sender, EventArgs e)
        {
            if(sender is ContentView v && !string.IsNullOrWhiteSpace(v.BindingContext.ToString()) && BindingContext is AsignacionRutasDetalleViewModel vm){
                vm.SubirRow(v.BindingContext.ToString());
            }
        }

        private void TapDownRow(object sender, EventArgs e)
        {
            if (sender is ContentView v && !string.IsNullOrWhiteSpace(v.BindingContext.ToString()) && BindingContext is AsignacionRutasDetalleViewModel vm)
            {
                vm.BajarRow(v.BindingContext.ToString());
            }
        }
    }
}