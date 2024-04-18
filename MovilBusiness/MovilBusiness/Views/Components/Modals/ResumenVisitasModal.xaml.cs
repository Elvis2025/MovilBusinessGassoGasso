
using MovilBusiness.DataAccess;
using MovilBusiness.model.Internal.structs;
using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ResumenVisitasModal : ContentPage
	{
        public List<ResumenVisitas> ResumenVisita { get; set; }

		public ResumenVisitasModal (int visSecuencia, int cliid, DS_Visitas myVis)
		{
            BindingContext = this;

            ResumenVisita = myVis.GetResumenVisita(visSecuencia, cliid);

            InitializeComponent();
            
		}

        private void Dismiss(object sender, EventArgs args)
        {
            Navigation.PopModalAsync(true);
        }
	}
}