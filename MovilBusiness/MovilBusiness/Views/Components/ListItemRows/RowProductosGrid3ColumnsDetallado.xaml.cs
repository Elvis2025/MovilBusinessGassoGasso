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
	public partial class RowProductosGrid3ColumnsDetallado : Grid
	{
		public RowProductosGrid3ColumnsDetallado ()
		{
			InitializeComponent ();
		}

        private void AgregarCantidad(object sender, EventArgs args)
        {
            MessagingCenter.Send("", "AgregarCantidad", lblMenos.BindingContext.ToString());
        }

        private void RestarCantidad(object sender, EventArgs args)
        {
            MessagingCenter.Send("", "RestarCantidad", lblMenos.BindingContext.ToString());
        }

        private void ShowDetalle(object sender, EventArgs args)
        {
            MessagingCenter.Send("", "ShowDetalle", lblMenos.BindingContext.ToString());
        }
    }
}