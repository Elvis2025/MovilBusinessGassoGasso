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
	public partial class DetalleTareasModal : ContentPage
	{
		public DetalleTareasModal (TareasViewModel context)
		{
            BindingContext = context;
            InitializeComponent ();
            context.DetalleTarea();
        }

        private void Dimiss(object sender, EventArgs e)
        {
            Navigation.PopModalAsync(true);
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            if (BindingContext is TareasViewModel vm)
            {
                string status = ((Button)sender).BindingContext as string;
                vm.ActualizarTarea(status);
                
            }

        }
    }
}



