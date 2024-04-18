using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	public partial class RevisionProductosConFaltantes : ContentPage, INotifyPropertyChanged
    {

        public new event PropertyChangedEventHandler PropertyChanged;
        private Action OnProductAccepted;

        public ObservableCollection<ProductosTemp> productosConFaltantes;
        public ObservableCollection<ProductosTemp> ProductosConFaltantes { get => productosConFaltantes; set { productosConFaltantes = value; RaiseOnPropertyChanged(); } }

        public string cantidad_unidades;
        public string Cantidad_Unidades { get => cantidad_unidades; set { cantidad_unidades = value; RaiseOnPropertyChanged(); } }
        private DS_Productos myProd;

        public RevisionProductosConFaltantes (List<ProductosTemp> ProFaltantes, Action OnProductAccepted)
		{
            ProductosConFaltantes = new ObservableCollection<ProductosTemp>(ProFaltantes);
            this.OnProductAccepted = OnProductAccepted;
            myProd = new DS_Productos();

            InitializeComponent ();
            BindingContext = this;
        }

        private async void AceptarProductos(object sender, EventArgs args)
        {
            try
            {
                await Navigation.PopModalAsync(false);

                OnProductAccepted?.Invoke();
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }

        }

        protected override bool OnBackButtonPressed()
        {
            if (!DS_RepresentantesParametros.GetInstance().GetParCotOfertasManuales() && !DS_RepresentantesParametros.GetInstance().GetParVenOfertasManuales() && !DS_RepresentantesParametros.GetInstance().GetParPedOfertasManuales())
            {
                myProd.ClearTempOfertas((int)Arguments.Values.CurrentModule);
            }

            Navigation.PopModalAsync(true);
            PedidosDetallePage.Finish = true;
            return true;
        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnBackButton(object sender, EventArgs e)
        {
            OnBackButtonPressed();
        }
    }
}