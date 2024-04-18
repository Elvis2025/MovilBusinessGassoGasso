using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class OfertasInfoView : ContentView
	{
		public OfertasInfoView ()
		{
			InitializeComponent ();
		}

        /*private void OnListItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            if(args.SelectedItem == null)
            {
                return;
            }

            list.SelectedItem = null;
        }*/

        ///public Picker GetComboOfertas() { return comboOferta; }

    }
}