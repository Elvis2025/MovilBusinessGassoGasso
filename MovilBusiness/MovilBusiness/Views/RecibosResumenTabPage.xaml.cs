using MovilBusiness.viewmodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class RecibosResumenTabPage : ContentPage
	{
		public RecibosResumenTabPage()
		{
			InitializeComponent ();

           /* dialogImpresion.OnCancelar = OnCancelarImpresion;
            dialogImpresion.OnAceptar = OnAceptarImpresion;*/
        }

       /* private void OnCancelarImpresion()
        {
            ((RecibosViewModel)BindingContext).CancelarImpresion();
        }

        private void OnAceptarImpresion(int copias)
        {
            ((RecibosViewModel)BindingContext).AceptarImpresion(copias);
        }*/
    }
}