using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Controls
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DrawerLayout : StackLayout
	{
        public DrawerLayout ()
		{
			InitializeComponent ();
		}

        private void OnOptionMenuItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            if (args.SelectedItem == null)
            {
                return;
            }

            if(Parent.Parent != null && Parent.Parent is MasterDetailPage page)
            {
                page.IsPresented = false;
            }

            drawerLayout.SelectedItem = null;
        }
    }
}