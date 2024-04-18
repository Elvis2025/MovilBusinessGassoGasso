using MovilBusiness.Configuration;
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
    public partial class ConsultaInventarioFisicoPage : ContentPage
    {
        public ConsultaInventarioFisicoPage()
        {
            BindingContext = new ConsultaInventarioFisicoViewModel(this);
            InitializeComponent();
        }

        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if(e.SelectedItem == null)
            {
                return;
            }

            if(BindingContext is ConsultaInventarioFisicoViewModel vm)
            {
                vm.GoDetalle(e.SelectedItem as InventarioFisico);
            }

            list.SelectedItem = null;
        }
    }
}