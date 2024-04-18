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
	public partial class DepositoGastosPage : ContentPage
	{
		public DepositoGastosPage ()
		{
            BindingContext = new DepositoGastosViewModel(this);

            InitializeComponent ();

            dialogImpresion.OnAceptar = AceptarImpresion;
            dialogImpresion.OnCancelar = CancelarImpresion;
            dialogImpresion.SetCopiasImpresionByTitId(25);
		}

        private void CancelarImpresion()
        {
            Navigation.PopAsync(true);
        }

        private void AceptarImpresion(int copias)
        {
            if(BindingContext is DepositoGastosViewModel vm)
            {
                vm.AceptarImpresion(copias);
            }
        }
	}
}