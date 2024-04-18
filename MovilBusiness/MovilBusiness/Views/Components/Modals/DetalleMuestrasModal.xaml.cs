using MovilBusiness.DataAccess;
using MovilBusiness.Model;
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
    public partial class DetalleMuestrasModal : ContentPage
    {
        public Muestras CurrentMuestra { get; set; }
        public List<MuestrasRespuestas> Respuestas { get; set; }

        public DetalleMuestrasModal(int mueSecuencia)
        {
            var dsEst = new DS_Muestras();

            CurrentMuestra = dsEst.GetById(mueSecuencia);

            Respuestas = dsEst.GetRespuestas(mueSecuencia);

            BindingContext = this;

            InitializeComponent();

        }

        private void Dismiss(object sender, EventArgs args)
        {
            Navigation.PopModalAsync(false);
        }
    }
}