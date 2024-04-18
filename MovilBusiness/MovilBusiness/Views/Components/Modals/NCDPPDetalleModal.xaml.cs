using MovilBusiness.DataAccess;
using MovilBusiness.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class NCDPPDetalleModal : ContentPage, INotifyPropertyChanged
    {
     
        public new event PropertyChangedEventHandler PropertyChanged;
        public List<NCDPP> ncdpp { get; set; }
        public List<NCDPP> NCDPPSource { get => ncdpp; set { ncdpp = value;} }
        private DS_Recibos myNC;
        public NCDPPDetalleModal (int recsecuencia)
		{
            BindingContext = this;
			InitializeComponent ();
            myNC = new DS_Recibos();
            NCDPPSource = myNC.GetNCDppRecibos(recsecuencia);
          
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NCDPPSource"));
            
        }

        private void Dismiss(object sender, EventArgs args)
        {
            Navigation.PopModalAsync(true);
        }

        private void OnList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                return;
            }

            list.SelectedItem = null;
           
        }



       
    }
}