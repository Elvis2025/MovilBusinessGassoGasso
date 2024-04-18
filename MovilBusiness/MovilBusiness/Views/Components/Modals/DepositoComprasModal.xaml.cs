using MovilBusiness.ViewModel;
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DepositoComprasModal : ContentPage
	{
        public DepositoComprasModal(int depsecuencia = -1)
		{
            BindingContext = new DepositosComprasViewModel(this, depsecuencia);
			InitializeComponent ();

            dialogImpresion.OnAceptar = (copias) => { ((DepositosComprasViewModel)BindingContext).AceptarImpresion(copias); };
            dialogImpresion.OnCancelar = () => { Navigation.PopModalAsync(true); };
            dialogImpresion.SetCopiasImpresionByTitId(12);
		}

        private void Dismiss(object sender, EventArgs args) => Navigation.PopModalAsync(true);
    }
}