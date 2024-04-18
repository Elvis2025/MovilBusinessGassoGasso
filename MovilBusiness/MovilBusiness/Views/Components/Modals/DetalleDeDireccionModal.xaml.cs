using MovilBusiness.viewmodel;
using MovilBusiness.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DetalleDeDireccionModal : ContentPage
	{
        public DetalleDeDireccionModal(PedidosViewModel context)
		{
            BindingContext = context;

            context.ClientesDetalleDireccion();
           
            InitializeComponent ();
		}

        private void Dimiss(object sender, EventArgs e)
        {
            Navigation.PopModalAsync(true);
        }
        

        
    }
}