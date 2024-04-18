using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.viewmodel;
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
    public partial class ConsultaTransaccionesEstatusTab : ContentPage
    {
        public ConsultaTransaccionesEstatusTab()
        {
            InitializeComponent();
        }


        private void OnListItemTapped(object sender, ItemTappedEventArgs args)
        {
            if (args.Item == null)
            {
                return;
            }

            if (BindingContext is ConsultaTransaccionesViewModel vm)
            {
                vm.ItemTapped(args.Item as ExpandListItem<Estados>);
            }

            list.SelectedItem = null;

        }
    }
}