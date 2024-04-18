using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovilBusiness.DataAccess;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.ListItemRows
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RowProductosInventario6 : Frame
    {
        public RowProductosInventario6()
        {
            InitializeComponent();
        }

        bool isChecked = false;

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            isChecked = radiobuttonInvF.IsChecked;
        }

        private void RadioButton_Clicked(System.Object sender, System.EventArgs e)
        {
            if (isChecked && radiobuttonInvF.IsChecked)
                radiobuttonInvF.IsChecked = false;
            isChecked = radiobuttonInvF.IsChecked;
        }
    }

}
