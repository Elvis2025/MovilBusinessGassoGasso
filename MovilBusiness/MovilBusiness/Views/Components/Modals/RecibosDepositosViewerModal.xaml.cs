using MovilBusiness.DataAccess;
using MovilBusiness.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class RecibosDepositosViewerModal : ContentPage
	{
		public RecibosDepositosViewerModal (List<Recibos> recSecuencias)
		{
            var recibos = new DS_Recibos().GetRecibosBySecuencias(recSecuencias.Where(x => !x.confirmado).ToList(), false);
            var confirmados = new DS_Recibos().GetRecibosBySecuencias(recSecuencias.Where(x => x.confirmado).ToList(), true);
            recibos.AddRange(confirmados);

            InitializeComponent ();

            list.ItemsSource = recibos;
		}

        private void Dismiss(object sender, EventArgs args)
        {
            Navigation.PopModalAsync(false);
        }

        private async void List_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if(e.SelectedItem == null)
            {
                return;
            }

            if(e.SelectedItem is Recibos recibo)
            {
                await Navigation.PushAsync(new ConsultaRecibosModal(recibo.RecSecuencia, recibo.RecTipo, recibo.CliID, recibo.confirmado));
            }                   

            list.SelectedItem = null;
        }
    }
}