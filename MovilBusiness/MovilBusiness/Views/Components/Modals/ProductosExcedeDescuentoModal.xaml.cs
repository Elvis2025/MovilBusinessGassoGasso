using MovilBusiness.model.Internal;
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
	public partial class ProductosExcedeDescuentoModal : ContentPage
	{

        public ProductosExcedeDescuentoModal (List<ProductosTemp> productos)
		{
			InitializeComponent ();

            list.ItemsSource = productos;
		}

        private async void Dismiss(object sender, EventArgs args)
        {
            await Navigation.PopModalAsync(false);
        }
	}
}