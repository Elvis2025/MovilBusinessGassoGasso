using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Model;
using MovilBusiness.Resx;
using MovilBusiness.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class HistoricoPromedioPage : ContentPage
	{
       
        public List<HistoricoPromedioDetalle> Documentos { get; set; }
        public HistoricoPromedioPage ()
		{
           
            Documentos = GetHistoricoPromedio(Arguments.Values.CurrentClient.CliID);
            BindingContext = this;
            InitializeComponent ();

            var orderbyList = new List<string>();
            orderbyList.Add(AppResource.Description);
            orderbyList.Add(AppResource.SaleUnit);
            orderbyList.Add(AppResource.Price);
            orderbyList.Add(AppResource.QtySold);
            orderbyList.Add(AppResource.AverageQty);
            orderbyList.Add(AppResource.Reference);

            comboOrderBy.ItemsSource = orderbyList;
        }



        public List<HistoricoPromedioDetalle> GetHistoricoPromedio(int cliId)
        {
            return SqliteManager.GetInstance().Query<HistoricoPromedioDetalle>("select p.ProDescripcion as ProDescripcion, HipMonto, HipCantidadVendida, p.procodigo as ProCodigo, ifnull(p.ProDatos2, '') as UnidadVenta, HipCantidadPromedio  " +
                    "from HistoricoPromedioDetalle hpd " +
                    "inner join productos p on hpd.ProID = p.ProID " +
                    "where hpd.CliID = ? order by  cast(HipCantidadVendida as interger) DESC", new string[] { cliId.ToString() });
        }

        private void comboOrderBy_SelectedIndexChanged(object sender, EventArgs a)
        {
            try
            {
                if (comboOrderBy.SelectedIndex == -1 || Documentos == null || Documentos.Count() == 0)
                {
                    return;
                }

                var index = comboOrderBy.SelectedIndex;

                IOrderedEnumerable<HistoricoPromedioDetalle> res = null;

                switch (index)
                {
                    case 0: //descripcion
                        res = from str in Documentos orderby str.ProDescripcion ascending select str;
                        break;
                    case 1://unidad venta
                        res = from str in Documentos orderby str.UnidadVenta ascending select str;
                        break;
                    case 2: //precio
                        res = from str in Documentos orderby str.HipMonto ascending select str;
                        break;
                    case 3://cant vendida
                        res = from str in Documentos orderby str.HiPCantidadVendida ascending select str;
                        break;
                    case 4://cant promedio
                        res = from str in Documentos orderby str.HipCantidadPromedio ascending select str;
                        break;
                    case 5://referencia
                        res = from str in Documentos orderby str.ProCodigo ascending select str;
                        break;
                }

                if (res != null)
                {
                    Documentos = new List<HistoricoPromedioDetalle>(res);

                    list.ItemsSource = Documentos;
                }

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }
        }
    }
}