using MovilBusiness.Abstraction;
using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.Resx;
using MovilBusiness.viewmodel;
using MovilBusiness.Views;
using MovilBusiness.Views.Components.Modals;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class ProductosNoVendidosViewModel : BaseViewModel
    {

        private DS_Productos myPro;
        public List<Productos> ProductosNovendidos { get { return Productosnovendidos; } set { Productosnovendidos = value; RaiseOnPropertyChanged(); ; } }
        private List<Productos> Productosnovendidos;

        private double opacityCode;
        public double OpacityCode { get { return opacityCode; } set { opacityCode = value; RaiseOnPropertyChanged(); ; } }

        private double opacityproduct;
        public double OpacityProduct { get { return opacityproduct; } set { opacityproduct = value; RaiseOnPropertyChanged(); ; } }
        
        private bool IsOnline;

        public ICommand OrderCommand { get; private set; }

        public ProductosNoVendidosViewModel(Page page) : base(page)
        {
            OrderCommand = new Command((s) => { ChangeOder(s); });

            IsOnline = myParametro.GetParProductosNoVendidosOnline();
            myPro = new DS_Productos();
            ProductosNovendidos = new List<Productos>();

        }

        private void ChangeOder(object s)
        {
            if ((string)s == "1")
            {
                OpacityCode = 1;
                OpacityProduct = 0.5;
                ProductosNovendidos = ProductosNovendidos.OrderBy(p => p.ProDescripcion).ToList();
                DependencyService.Get<IAppToast>().ShowToast("Descripcion");
            }
            else
            {
                OpacityProduct = 1;
                OpacityCode = 0.5;                
                ProductosNovendidos = ProductosNovendidos.OrderBy(p => p.ProCodigo).ToList();
                DependencyService.Get<IAppToast>().ShowToast("Codigo");
            }
        }

        public async Task FillListAsync(string tabla, string tipo, bool IsOnline)
        {
            try
            {
                IsBusy = true;
                ProductosNovendidos = tipo == AppResource.Products ? await myPro.GetProductosNoVendidos(tabla, IsOnline) : await myPro.GetAllLineas(tabla, IsOnline);

                OpacityCode = 1;
                OpacityProduct = 0.5;
                ProductosNovendidos = ProductosNovendidos.OrderBy(p => p.ProDescripcion).ToList();
                DependencyService.Get<IAppToast>().ShowToast("Descripcion");
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;

        }

        public async Task ClientesNovendidos(string tabla, Productos tran, bool IsProduct, bool IsRutaVisita,string Cliids)
        {
        
            try
            {
                IsBusy = true;
                await PushAsync(new ClientesProductosNoVendidosPage(tabla, IsProduct? tran.ProID.ToString():tran.ProCodigo, IsOnline, IsProduct, IsRutaVisita, Cliids, tran.ProDescripcion));
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

       

    }
}
