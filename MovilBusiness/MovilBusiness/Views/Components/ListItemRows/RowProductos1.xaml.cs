
using MovilBusiness.DataAccess;
using MovilBusiness.model.Internal;
using MovilBusiness.Views.Components.Modals;
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.ListItemRows
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class RowProductos1 : Frame
    {
         
		public RowProductos1 ()
		{
			InitializeComponent();
            lbShowCodeBar.IsVisible = DS_RepresentantesParametros.GetInstance().GetCodeBar();
            lbCodeBar.IsVisible = !DS_RepresentantesParametros.GetInstance().GetCodeBar();         
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            if (sender is ContentView con && con.Content is Label lbl)
            {
                MessagingCenter.Send("", "ShowProductoCombo", lbl.BindingContext.ToString());
            }
        }

        private void ShowInventario(object sender, EventArgs e)
        {
            if (sender is ContentView con && con.Content is Label lbl)
            {
                MessagingCenter.Send("", "ShowInventario", lbl.BindingContext.ToString());
                
            }
        }

        private void TapGestureRecognizer_Tapped_1(object sender, EventArgs e)
        {

            if (sender is Label lbl && BindingContext is ProductosTemp p)
            {
                Application.Current.MainPage.Navigation.PushModalAsync(new ProCodigosBarraModal(p.ProReferencia));
            }
        }
    }
}