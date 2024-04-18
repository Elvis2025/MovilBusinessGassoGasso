using MovilBusiness.model.Internal.structs;
using MovilBusiness.viewmodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Dialogs
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ModalCargainicial : ContentPage
    {
		public ModalCargainicial (CargaInicialArgs args)
		{
            var vm = new CargaInicialViewModel(this, args);

            BindingContext = vm;

			InitializeComponent ();

            vm.StartCargaInicial();
		}

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}