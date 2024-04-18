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
	public partial class RowConducesDetalle2 : ViewCell
	{
		public RowConducesDetalle2 ()
		{
			InitializeComponent ();
		}

        private void OnOkClicked(object sender, EventArgs e)
        {
            if (Parent is ListView list && BindingContext is EntregasDetalleTemp data)
            {
                if (list.BindingContext is EntregasRepartidorDetalleViewModel vm1)
                {
                    vm1.AgregarProducto(data, null);
                }
                else if (list.BindingContext is EntregaRepartidorDetalleRevisionViewModel vm2)
                {
                    vm2.AgregarProducto(data);
                }
            }
        }
    }
}