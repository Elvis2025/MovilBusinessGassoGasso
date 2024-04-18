using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.Resx;
using MovilBusiness.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ConsultaDescuentosModal : ContentPage, INotifyPropertyChanged
	{
        public new event PropertyChangedEventHandler PropertyChanged;

        private DS_DescuentosRecargos myDes;

        private List<DescuentosRecargos> descuentos;
        public List<DescuentosRecargos> Descuentos { get => descuentos; set { descuentos = value;  RaiseOnPropertyChanged(); } }

        private List<DescuentosRecargosDetalle> descuentodetalles;
        public List<DescuentosRecargosDetalle> DescuentoDetalles { get => descuentodetalles; set { descuentodetalles = value; RaiseOnPropertyChanged(); } }

        private DescuentosRecargos currentdescuento;
        public DescuentosRecargos CurrentDescuento { get => currentdescuento; set { currentdescuento = value; CargarDetalle(); RaiseOnPropertyChanged(); } }
        public ICommand VerProductosDescuentoCommand { get; private set; }

        public ConsultaDescuentosModal (int titId)
		{
            myDes = new DS_DescuentosRecargos();

            var ofecurrentpro = PedidosViewModel.Instance().CurrentPedidoAEntregar;

            Descuentos = myDes.GetDescuentosDisponibles(titId, Arguments.Values.CurrentClient.CliID,entrega: ofecurrentpro);

            VerProductosDescuentoCommand = new Command(GoProductosDescuento);

            BindingContext = this;

            CurrentDescuento = Descuentos.FirstOrDefault();

            if(CurrentDescuento == null)
            {
                CurrentDescuento = new DescuentosRecargos();
            }

			InitializeComponent ();
            
		}

        private void CargarDetalle()
        {
            try
            {
                if (CurrentDescuento == null)
                {
                    return;
                }

                DescuentoDetalles = myDes.GetDetalles(CurrentDescuento.DesID);

            }catch(Exception e)
            {
                DisplayAlert(AppResource.Error, e.Message, AppResource.Aceptar);
            }
        }

        private void Dismiss(object sender, EventArgs args)
        {
            Navigation.PopModalAsync(true);
        }


        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void GoProductosDescuento()
        {
            if (CurrentDescuento == null || !CurrentDescuento.IsMancomunado)
            {
                return;
            }

            var response = await DisplayActionSheet(AppResource.Select, AppResource.Cancel, null, new string[] { AppResource.ProductThatApply });
            bool aRegalar = false;

            string grpCodigo;

            if(response == AppResource.ProductThatApply)
            {
                grpCodigo = CurrentDescuento.GrpCodigo;
            }
            else
            {
                return;
            }

            await Navigation.PushModalAsync(new DescuentoMancomunadoProductosModal(grpCodigo, aRegalar));
        }
    }
}