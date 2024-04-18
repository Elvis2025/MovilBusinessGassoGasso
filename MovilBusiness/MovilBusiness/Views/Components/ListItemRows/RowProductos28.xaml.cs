using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.ListItemRows
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RowProductos28 : Frame
    {
        public RowProductos28()
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
    }
}