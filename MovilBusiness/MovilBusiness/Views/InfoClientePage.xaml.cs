
using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.Model;
using MovilBusiness.Resx;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class InfoClientePage : TabbedPage
	{
        public Clientes CurrentClient { get; set; }
        public string CliEstatus { get => CurrentClient != null ? CurrentClient.CliEstatus == 1 ? AppResource.Active : AppResource.Inactive : ""; }
        public string TipoNegocioDescripcion { get; set; }
        public string Clasificacion { get; set; }
        public string MunDescripcion { get; set; }
        public string PaiNombre { get; set; }
        public string ProNombre { get; set; }
        public string ListaPrecios { get; set; }
        public string FACTipo { get; set; }
        public string NCFTipo { get; set; }
        public double BalancePendiente { get; set; }
        public string CondicionPago { get; set; }
        public string VisitasSemana1 { get; set; }
        public string VisitasSemana2 { get; set; }
        public string VisitasSemana3 { get; set; }
        public string VisitasSemana4 { get; set; }
        public string RutPosicion { get; set; }
        public string RNC { get; set; }
        public string Clicontacto { get; set; }

        public string Horario { get; set; }


        public InfoClientePage (Clientes cli)
		{
            CurrentClient = cli;
            RNC = CurrentClient.CliRNC;
            Clicontacto = CurrentClient.CliContacto;           


            if (cli == null)
            {
                throw new Exception(AppResource.ErrorLoadingCustomerData);
            }

            var dsCli = new DS_Clientes();

            var tipo = new DS_TiposNegocio().GetTipoById(cli.TiNID);

            TipoNegocioDescripcion = tipo != null ? tipo.TinDescripcion : "";

            var Claid = dsCli.GetClaID(CurrentClient.CliID);
            Clasificacion = dsCli.GetClasificacion(Claid);

            var CliMunID = dsCli.GetCliMunID(CurrentClient.CliID);
            var municipio = new DS_Municipios().GetMunicipioById(CliMunID);

            var CliPaiID = dsCli.GetCliPaiID(CurrentClient.CliID);
            var pais = new DS_Pais().GetById(CliPaiID);
            var provincia = new DS_Provincias().GetProvinciaById(cli.ProID);

            BalancePendiente = new DS_CuentasxCobrar().GetBalanceTotalByCliid(cli.CliID, WithChD: !DS_RepresentantesParametros.GetInstance().GetParNoTomarEnCuentaChequesDiferidos());
            var condicion = new DS_CondicionesPago().GetByConId(cli.ConID);

            if(condicion != null)
            {
                CondicionPago = condicion.ConDescripcion;
            }

            var calendario = new DS_RutaVisitas().GetCalendarioVisitas(cli.CliID);

            VisitasSemana1 = calendario.DiasVisitaSemana1;
            VisitasSemana2 = calendario.DiasVisitaSemana2;
            VisitasSemana3 = calendario.DiasVisitaSemana3;
            VisitasSemana4 = calendario.DiasVisitaSemana4;
            RutPosicion = calendario.RutPosicion.ToString();

            var usos = new DS_UsosMultiples();
            ListaPrecios = usos.GetListaPreciosDescripcion(cli.LiPCodigo);
            FACTipo = usos.GetTipoComprobanteFacDescripcion(cli.CliTipoComprobanteFAC);
            NCFTipo = usos.GetTipoComprobanteNCFDescripcion(cli.CliTipoComprobanteNC);

            if(municipio != null)
            {
                MunDescripcion = municipio.MunDescripcion;
            }

            if(pais != null)
            {
                PaiNombre = pais.PaiNombre;
            }

            if(provincia != null)
            {
                ProNombre = provincia.ProNombre;
            }

            LoadHorario(dsCli);

            BindingContext = this;

			InitializeComponent ();
		}

        private void LoadHorario(DS_Clientes myCli)
        {
            try
            {
                var horarios = myCli.GetClienteHorarios(CurrentClient.CliID);
                Horario = "";

                for (var i = 0; i < horarios.Count; i++)
                {
                    var horario = horarios[i];
                    //String.Format("{0,-27}", s);
                    Horario += string.Format("{0,-20}", horario.ClhDia) + (horario.GetHorarioFormat(horario.clhHorarioApertura) + " - " + horario.GetHorarioFormat(horario.clhHorarioCierre)) + (i == horarios.Count - 1 ? "" : "\n");
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        protected override bool OnBackButtonPressed()
        {
            Finish();
            return true;
        }

        private bool finishing;
        private async void Finish()
        {
            if (finishing)
            {
                return;
            }
            finishing = true;
            await Navigation.PopAsync(false);
            finishing = false;
        }
    }
}