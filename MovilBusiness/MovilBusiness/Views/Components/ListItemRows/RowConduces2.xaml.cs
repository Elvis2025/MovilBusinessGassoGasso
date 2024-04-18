using MovilBusiness.Model.Internal;
using MovilBusiness.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.ListItemRows
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RowConduces2 : ViewCell
    {
        public RowConduces2()
        {
            InitializeComponent();
        }

        private void OnOkClicked(object sender, EventArgs e)
        {
            if (Parent is ListView list && list.BindingContext is EntregasRepartidorDetalleViewModel vm && BindingContext is EntregasDetalleTemp data)
            {
                vm.AgregarProducto(data, null);
            }
        }
    }
}