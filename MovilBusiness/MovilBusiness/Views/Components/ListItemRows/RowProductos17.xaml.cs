using Rg.Plugins.Popup.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Services;
using MovilBusiness.Views.Components.Popup;

namespace MovilBusiness.Views.Components.ListItemRows
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RowProductos17 : Frame
    {
        public RowProductos17()
        {
            InitializeComponent();
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

        private async void Button_Clicked(object sender, EventArgs e)
        {
            if (sender is ContentView con && con.Content is Label lbl)
            {
                await Navigation.PushPopupAsync(new PopupImageFrame(lbl.BindingContext.ToString()));
            }
            
            
        }
    }
}