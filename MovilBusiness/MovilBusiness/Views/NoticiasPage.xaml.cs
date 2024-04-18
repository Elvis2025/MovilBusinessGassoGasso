using MovilBusiness.model;
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
	public partial class NoticiasPage : ContentPage
	{
		public NoticiasPage ()
		{

            BindingContext = new NoticiasViewModel(this);

			InitializeComponent ();
		}

        private void OnNoticiaSelected(object sender, SelectedItemChangedEventArgs args)
        {
            if (args.SelectedItem == null)
            {
                return;
            }

            var vm = BindingContext as NoticiasViewModel;

            vm?.AlertNoticiaSeleccionada(args.SelectedItem as Noticias);

            noticiasList.SelectedItem = null;
        }

    }
}