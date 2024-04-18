using MovilBusiness.Model;
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
    public partial class PushMoneyPorPagarPage : MasterDetailPage
    {
        public PushMoneyPorPagarPage(int pussecuencia = -1)
        {
            var vm = new PushMoneyPorPagarViewModel(this, pussecuencia: pussecuencia);

            BindingContext = vm;
            InitializeComponent();

            if(pussecuencia > 0)
            {
                this.Master.IsVisible = false;
            }

            dialogImpresion.OnCancelar = delegate { vm.ShowPrinter = false; };
            dialogImpresion.OnAceptar = vm.AceptarImpresion;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if(BindingContext is PushMoneyPorPagarViewModel vm)
            {
                vm.CargarDocumentos();
            }
        }

        private void list_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if(e.SelectedItem == null)
            {
                return;
            }

            if(BindingContext is PushMoneyPorPagarViewModel vm)
            {
                vm.OnDocumentSelected(e.SelectedItem as PushMoneyPorPagar);
            }

            list.SelectedItem = null;
        }
        /*
private void OnListItemSelected(object sender, SelectedItemChangedEventArgs e)
{
   if(e.SelectedItem == null)
   {
       return;
   }

   if(BindingContext is PushMoneyPorPagarViewModel vm)
   {

   }

   list.SelectedItem = null;
}*/
    }
}