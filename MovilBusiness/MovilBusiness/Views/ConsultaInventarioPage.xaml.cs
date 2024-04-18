using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.Model;
using MovilBusiness.Printer.Formats;
using MovilBusiness.Resx;
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

namespace MovilBusiness.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ConsultaInventarioPage : ContentPage, INotifyPropertyChanged
	{
        public ICommand PrintCommand { get; private set; }

        private List<Inventarios> inventario;
        public List<Inventarios> Inventario { get => inventario; set { inventario = value; RaiseOnPropertyChanged(); } }

        private Almacenes currentalmacen;
        public Almacenes CurrentAlmacen { get => currentalmacen; set { currentalmacen = value; OnCurrentAlmacenChanged(); RaiseOnPropertyChanged(); } }

        public List<Almacenes> Almacenes { get; set; }

        public new event PropertyChangedEventHandler PropertyChanged;

        public bool ListIsEmpty { get => Inventario == null || Inventario.Count == 0; set { RaiseOnPropertyChanged(); } }

        public bool ShowComboAlmacen { get => DS_RepresentantesParametros.GetInstance().GetParUsarMultiAlmacenes(); }

        private InventariosFormats Printer;

        private DS_Inventarios myInv;
        public List<Almacenes> idalmacenes;
        public ConsultaInventarioPage ()
		{
            BindingContext = this;

            myInv = new DS_Inventarios();
            var myAlm = new DS_Almacenes();

            if (!DS_RepresentantesParametros.GetInstance().GetParUsarMultiAlmacenes())
            {
                Inventario = myInv.GetInventario(true);
            }
            
            Printer = new InventariosFormats(myInv);

            PrintCommand = new Command(ImprimirResumen);

            ListIsEmpty = false;

            Almacenes = myAlm.GetAlmacenes();

            var parMultiAlmacenes = DS_RepresentantesParametros.GetInstance().GetParUsarMultiAlmacenes();
            List<RepresentantesParametros> parametrosAlmacenes = DS_RepresentantesParametros.GetInstance().GetAlmacenesAgrupados("ALMID").ToList();
            
            if (DS_RepresentantesParametros.GetInstance().GetParConteoFisicoAlmacenesParaContar() != "")
            {
                idalmacenes = new DS_Almacenes().GetAlmacenesByAlmIDParameter(DS_RepresentantesParametros.GetInstance().GetParConteoFisicoAlmacenesParaContar());
            }

            if (parMultiAlmacenes && ((parametrosAlmacenes != null && parametrosAlmacenes.Count > 0) || (idalmacenes != null && idalmacenes.Count > 0)))
            {
                var parValores = idalmacenes != null && idalmacenes.Count > 0 ? idalmacenes.Select(p => p.AlmID).ToList() : parametrosAlmacenes.Select(p => int.Parse(p.ParValor)).ToList();
                
                if (idalmacenes != null && idalmacenes.Count > 0) {
                    parValores.Add(DS_RepresentantesParametros.GetInstance().GetParAlmacenVentaRanchera());
                    parValores.Add(DS_RepresentantesParametros.GetInstance().GetParAlmacenIdParaDespacho());
                    parValores.Add(DS_RepresentantesParametros.GetInstance().GetParAlmacenIdParaDevolucion());
                }

                if (parValores != null && parValores.Count > 0)
                {
                    Almacenes = Almacenes.Where(a => parValores.Contains(a.AlmID)).ToList();
                }
            }
            InitializeComponent ();
		}

        private async void ImprimirResumen()
        {
            if (IsBusy)
            {
                return;
            }

            if (ListIsEmpty)
            {
                await DisplayAlert(AppResource.Warning, AppResource.NoInventoryToPrint, AppResource.Aceptar);
                return;
            }

            var result = await DisplayAlert(AppResource.Warning, AppResource.WantPrintInventoryResume, AppResource.Print, AppResource.Cancel);

            if (!result)
            {
                return;
            }

            IsBusy = true;
            RaiseOnPropertyChanged(nameof(IsBusy));

            await Task.Run(() => 
            {
                Printer.Print(DS_RepresentantesParametros.GetInstance().GetFormatoImpresionInventario(), new Printer.PrinterManager(), CurrentAlmacen != null ? CurrentAlmacen.AlmID : -1);
            });

            IsBusy = false;
            RaiseOnPropertyChanged(nameof(IsBusy));
        }
        
        private async void OnCurrentAlmacenChanged()
        {
            if (IsBusy)
            {
                return;
            }

            if(CurrentAlmacen == null)
            {
                Inventario = new List<Inventarios>();
                return;
            }

            IsBusy = true;
            RaiseOnPropertyChanged(nameof(IsBusy));
            try
            {

                await Task.Run(() => 
                {
                    Inventario = myInv.GetInventario(true, almId: CurrentAlmacen.AlmID);
                });

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
            RaiseOnPropertyChanged(nameof(IsBusy));
            RaiseOnPropertyChanged(nameof(ListIsEmpty));
        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}