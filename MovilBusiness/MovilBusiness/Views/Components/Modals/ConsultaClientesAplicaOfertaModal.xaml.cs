using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.Resx;
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
    public partial class ConsultaClientesAplicaOfertaModal : ContentPage, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;

        private List<Clientes> clientes;
        public List<Clientes> Clientes { get => clientes; set { clientes = value; RaiseOnPropertyChanged(); } }

        private string GrcCodigo;
        private string TipoNegocio;
        private int CliID;
        private int TinID;
        private bool FirstTime = true;

        private bool isbusy;
        public new bool IsBusy { get => isbusy; set { isbusy = value; RaiseOnPropertyChanged(); } }

        private readonly DS_Clientes myGrc;
        private readonly DS_TiposNegocio myTipN;

        public ConsultaClientesAplicaOfertaModal(int cliid, string grcCodigo,int tinID = 0)
        {
            GrcCodigo = grcCodigo;
            CliID = cliid;
            TinID = tinID;
            myGrc = new DS_Clientes();
            myTipN = new DS_TiposNegocio();

            InitializeComponent();

            BindingContext = this;

            lblTitle.Text = AppResource.ApplyingClients;

            var tipoNeg = myTipN.GetTipoById(tinID);
            TipoNegocio = tipoNeg != null ? tipoNeg.TinDescripcion : "";

            if (CliID == 0 && (GrcCodigo == "" || GrcCodigo == "0"))
            {
                lblGrpDescripcion.Text = $"{AppResource.GeneralOffer} - {TipoNegocio}";
            }
            else
            {
                lblGrpDescripcion.Text = CliID != 0 ? $"Cliente ID: {cliid} - {TipoNegocio}"  : $"{AppResource.GroupOffer}: {grcCodigo} - {TipoNegocio}";
            }
                
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (FirstTime)
            {
                FirstTime = false;
                LoadClientes();
            }
        }

        private async void LoadClientes()
        {
            IsBusy = true;

            try
            {
                await Task.Run(() => 
                {
                    if (CliID == 0 && (GrcCodigo == "" || GrcCodigo == "0") )
                    {
                        Clientes = myGrc.GetClientesByTodosClientesOferta(TinID);
                    }
                    else
                    {
                        Clientes = CliID != 0 ? myGrc.GetClientesByClienteOferta(CliID, TinID) : myGrc.GetClientesByGrupoClienteOferta(GrcCodigo, TinID);
                    }

                    if(Clientes == null || Clientes.Count == 0)
                    {
                        var sinCliente = new Clientes
                        {
                            CliCodigo = "0",
                            CliNombre = AppResource.YouDoNotHaveAssignedClientsToThisOffer
                        };
                        Clientes.Add(sinCliente);
                    }
                    
                });

            }catch(Exception e)
            {
                IsBusy = false;
                await DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
        }

        private async void Dismiss(object sender, EventArgs args)
        {
            await Navigation.PopModalAsync(false);
        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}