using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Dialogs
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DialogImpresion : Grid
    {
        public int Copias { get; set; } = 1;

        public Action OnCancelar { get; set; }
        public Action<int> OnAceptar { get; set; }

		public DialogImpresion ()
		{
			InitializeComponent ();

            Container.BindingContext = this;

            comboCopias.SelectedIndex = 0;

        }

        private void CancelarClicked(object sender, EventArgs args)
        {
            OnCancelar?.Invoke();
        }

        private void AceptarClicked(object sender, EventArgs args)
        {
            OnAceptar?.Invoke(Copias);
        }

        public void SetCopiasImpresionByTitId(int titId)
        {
            var cantidadCopias = DS_RepresentantesParametros.GetInstance().GetTransaccionCantidadCopias(titId);


            bool sourceChanged = false;

            if(cantidadCopias > 0)
            {
                var last = comboCopias.Items.LastOrDefault();

                if(last != null && int.TryParse(last.ToString(), out int cantidadActual))
                {
                    if(cantidadCopias > cantidadActual)
                    {
                        var list = new List<string>();

                        for (var i = 1; i <= cantidadCopias; i++)
                        {
                            list.Add(i.ToString());
                        }

                        comboCopias.ItemsSource = list;
                        comboCopias.SelectedIndex = list.Count - 1;
                        sourceChanged = true;
                    }
                    else
                    {
                        var index = comboCopias.Items.IndexOf(cantidadCopias.ToString());

                        if(index != -1)
                        {
                            comboCopias.SelectedIndex = index;
                        }
                    }
                }             
                                
            }

            var porDefecto = DS_RepresentantesParametros.GetInstance().GetParDevolucionesCopiasPorDefecto();

            if (Arguments.Values.CurrentModule == Modules.COMPRAS)
            {
                porDefecto = DS_RepresentantesParametros.GetInstance().GetParComprasCopiasPorDefecto();
            }
            else if (Arguments.Values.CurrentModule == Modules.COBROS)
            {
                porDefecto = DS_RepresentantesParametros.GetInstance().GetParRecibosCopiasPorDefecto();
            }

            if(porDefecto > 0)
            {
                var index = sourceChanged ? ((List<string>)comboCopias.ItemsSource).IndexOf(porDefecto.ToString()) : comboCopias.Items.IndexOf(porDefecto.ToString());

                if(index != -1)
                {
                    comboCopias.SelectedIndex = index;
                }
            }
        }
	}
}