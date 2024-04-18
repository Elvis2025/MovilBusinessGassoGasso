using MovilBusiness.DataAccess;
using MovilBusiness.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class InventarioAlmacenesModal : ContentPage, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;

        public bool IsWorking { get; set; }

        private List<InventariosAlmacenes> inventariodisponible;
        public List<InventariosAlmacenes> InventarioDisponible { get => inventariodisponible; private set { inventariodisponible = value; RaiseOnPropertyChanged(); } }

        private DS_inventariosAlmacenes myPrc;
        public InventarioAlmacenesModal ()
        {
            myPrc = new DS_inventariosAlmacenes();

            BindingContext = this;
            InitializeComponent ();
		}

        public void LoadInvDisponible(int proId)
        {
            if (IsWorking)
            {
                return;
            }

            IsWorking = true;

            InventarioDisponible = myPrc.GetInventarioDisponibleByProductos(proId);

            IsWorking = false;
        }

        private async void Dismiss(object sender, EventArgs args)
        {
            if (IsWorking)
            {
                return;
            }

            IsWorking = true;
            await Navigation.PopModalAsync(true);
            IsWorking = false;
        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}