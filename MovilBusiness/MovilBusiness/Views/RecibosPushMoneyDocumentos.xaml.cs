using MovilBusiness.model.Internal;
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
    public partial class RecibosPushMoneyDocumentos : ContentPage
    {
        public RecibosPushMoneyDocumentos()
        {
            InitializeComponent();
        }

        private void OnListItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if(e.SelectedItem == null)
            {
                return;
            }

            if(BindingContext is RecibosPushMoneyViewModel vm)
            {
                vm.SeleccionarDocumento(e.SelectedItem as RecibosDocumentosTemp);
            }

            listaFacturas.SelectedItem = null;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if(BindingContext is RecibosPushMoneyViewModel vm  && vm.Documentos == null)
            {
                vm.CargarDocumentos();
            }
        }
    }
}