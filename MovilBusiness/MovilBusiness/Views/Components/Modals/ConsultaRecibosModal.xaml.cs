using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
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
    public partial class ConsultaRecibosModal : TabbedPage
    {
        public string CliNombre { get; set; }
        public int RecSecuencia { get; set; }

        private DS_Recibos myRec;

        public List<RecibosAplicacion> Documentos { get; set; }
        public List<RecibosFormaPago> FormasPago { get; set; }
        public RecibosResumen Resumen { get; set; }

        public ConsultaRecibosModal(int recSecuencia, string recTipo, int cliId, bool confirmado)
        {
            RecSecuencia = recSecuencia;

            myRec = new DS_Recibos();

            var cliente = new DS_Clientes().GetClienteById(cliId);

            if(cliente != null)
            {
                CliNombre = cliente.CliNombre;
            }

            Documentos = myRec.GetRecibosAplicacionBySecuencia(RecSecuencia, confirmado, recTipo, true);
            FormasPago = myRec.GetRecibosFormasPagoBySecuencia(RecSecuencia, confirmado, recTipo);
            Resumen = myRec.GetResumenRecibos(RecSecuencia, confirmado, recTipo);

            BindingContext = this;

            InitializeComponent();
        }

    }
}