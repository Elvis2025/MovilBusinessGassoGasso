using MovilBusiness.model;
using MovilBusiness.Model;
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
	public partial class TareasPage : ContentPage
	{
        public TareasPage()
        {

            BindingContext = new TareasViewModel(this);

            InitializeComponent();
        }

           private void OnTareaTap(object sender, SelectedItemChangedEventArgs args)
            {
                if (args.SelectedItem == null)
                {
                    return;
                }

                var vm = BindingContext as TareasViewModel;

                vm?.DetalleTarea(args.SelectedItem as Tareas);

               ListaTareas.SelectedItem = null;
            }

        private void Dismiss(object sender, EventArgs args)
        {
            Navigation.PopModalAsync(false);
        }

    }
}
