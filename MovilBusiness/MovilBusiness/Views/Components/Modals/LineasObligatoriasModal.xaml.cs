using MovilBusiness.model;
using MovilBusiness.views;
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
	public partial class LineasObligatoriasModal : ContentPage, INotifyPropertyChanged
	{
        public new event PropertyChangedEventHandler PropertyChanged;

        public List<Productos> Productos { get; set; }
        public Action OnAccept { get; set; }

        public LineasObligatoriasModal(List<Productos>Productos)
		{
            this.Productos = Productos;

            BindingContext = this;

            InitializeComponent ();

            if(this.Productos == null || this.Productos.Count == 0)
            {
                Navigation.PopModalAsync(true);
            }
		}

        protected override bool OnBackButtonPressed()
        {
            Navigation.PopModalAsync(true);
            return true;
        }

        private void Dismiss(object sender, EventArgs args)
        {
            OnBackButtonPressed();
        }   

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}