
using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.Resx;
using MovilBusiness.viewmodel;
using MovilBusiness.Views.Components.Modals;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class ConsultaVisitasViewModel : BaseViewModel
    {
        private List<Visitas> visitas;
        public List<Visitas> Visitas { get => visitas; set { visitas = value; RaiseOnPropertyChanged(); } }

        public Clientes CurrentClient { get; set; }

        private DS_Visitas myVis;

        public ConsultaVisitasViewModel(Page page, Clientes cliente) : base(page)
        {
            myVis = new DS_Visitas();

            CurrentClient = cliente;
        }

        public async void LoadVisitas()
        {
            if (CurrentClient == null)
            {
                return;
            }

            IsBusy = true;

            try
            {
                if (myParametro.GetParTipoConsultaVisitas())
                {

                    await Task.Run(() => { Visitas = myVis.GetVisitasByCliente(CurrentClient.CliID); });

                }
                else
                {
                    await Task.Run(() => { Visitas = myVis.GetVisitasByClienteAndRepresentante(CurrentClient.CliID); });

                }
                

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.ErrorLoadingVisitsSummary, e.Message);
            }

            IsBusy = false;
        }

        public void VerResumenVisita(int visSecuencia)
        {
            PushModalAsync(new ResumenVisitasModal(visSecuencia, CurrentClient.CliID, myVis));
        }
    }
}
