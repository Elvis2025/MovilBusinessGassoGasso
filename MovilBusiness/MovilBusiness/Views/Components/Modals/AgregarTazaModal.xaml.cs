using MovilBusiness.model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AgregarTazaModal : ContentPage
	{
        public new event PropertyChangedEventHandler PropertyChanged;
        private string taza;
        public string Taza { get { return taza; } set { taza = value;  RaiseOnPropertyChanged(); } }
        public double valueTaza;
        private List<Monedas> monedas;
        public List<Monedas> Monedas { get => monedas; set { monedas = value; RaiseOnPropertyChanged(); } }
        private Monedas currentmoneda;
        public Monedas CurrentMoneda { get => currentmoneda; set { currentmoneda = value; RaiseOnPropertyChanged(); } }
        public int TraSecuencia;
        public ICommand AceptarCommand { get; private set; }

        public AgregarTazaModal(double ParTaza, int Secuencia, /*List<Monedas> monedas*/ Monedas moneda)
		{
            valueTaza = ParTaza;
            TraSecuencia = Secuencia;
            Monedas = monedas;
            CurrentMoneda = moneda;
            InitializeComponent ();
		}

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async void AutorizarTaza(double Result)
        {
            if (Result > 0)
            {
                await Navigation.PushModalAsync(new AutorizacionesModal(false, TraSecuencia, 3, "")
                {
                    OnAutorizacionUsed = (autSec) =>
                    {
                        CurrentMoneda.MonTasa = Result;
                        Dismiss(null,null);
                    }

                });
            }
            
        }

        public void AceptarTaza(object sender, EventArgs args)
        {
            if (!string.IsNullOrEmpty(entryTaza.Text))
            {
                if (double.Parse(entryTaza.Text) > valueTaza)
                {
                    AutorizarTaza(double.Parse(entryTaza.Text));
                }
                else
                {
                    Navigation.PopModalAsync();
                }
            }
        }

        public Monedas GetTazaMoneda()
        {
            return CurrentMoneda;
        }

        private void Dismiss(object sender, EventArgs args)
        {
            Navigation.PopModalAsync(true);
        }

    }
}