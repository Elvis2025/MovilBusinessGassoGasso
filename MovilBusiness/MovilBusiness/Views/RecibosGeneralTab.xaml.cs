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
	public partial class RecibosGeneralTab : ContentPage
	{
		public RecibosGeneralTab ()
		{
			InitializeComponent ();
		}

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is RecibosViewModel vm)
            {
                if (vm.Documentos == null || vm.ReloadDocuments)
                {
                    vm.IsFirstChkDiferidoGeneral = false;
                    vm.CargarDocumentos(!vm.ReloadDocuments);
                }
            }

        }
    }
}