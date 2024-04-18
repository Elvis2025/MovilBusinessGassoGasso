using MovilBusiness.viewmodel;
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
	public partial class DependientesTabPage : ContentPage
	{
		public DependientesTabPage ()
		{
			InitializeComponent ();
		}

        protected override void OnBindingContextChanged()
        {
            if(BindingContext is PedidosViewModel vm)
            {
                BindingContext = vm.DependientesViewModel;
                return;
            }else if(BindingContext is RecibosPushMoneyViewModel vm2)
            {
                BindingContext = vm2.DependientesViewModel;
            }

            base.OnBindingContextChanged();
        }
    }
}