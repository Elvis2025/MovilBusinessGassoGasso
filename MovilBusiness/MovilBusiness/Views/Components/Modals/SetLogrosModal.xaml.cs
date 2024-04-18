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
	public partial class SetLogrosModal : ContentPage
	{
        private PresupuestosViewModel vm;
        public SetLogrosModal (PresupuestosViewModel context)
		{
            BindingContext = context;
            vm = context;
            InitializeComponent ();

		}

        private void Dismiss(object sender, EventArgs args)
        {
            Navigation.PopModalAsync(false);
        }

        private void Button_Clicked(object sender, EventArgs arg)
        {            
                Navigation.PopModalAsync(false);
                 vm.CargarPresupuestos();               
            
        }
    }
}