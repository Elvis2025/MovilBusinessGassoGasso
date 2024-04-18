using MovilBusiness.DataAccess;
using MovilBusiness.model.Internal;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class ProductosProximosVencerViewModel : BaseViewModel
    {
        private bool showdetallefactura;
        public bool ShowDetalleFactura { get => showdetallefactura; set { showdetallefactura = value; RaiseOnPropertyChanged(); } }

        private ObservableCollection<ProductosTemp> productos;
        public ObservableCollection<ProductosTemp> Productos { get => productos; set { productos = value; RaiseOnPropertyChanged(); } }

        private DS_Clientes myCli;
        private readonly int CurrentCliId;

        public ProductosProximosVencerViewModel(int cliId, Page page) : base(page)
        {
            CurrentCliId = cliId;
            myCli = new DS_Clientes();
        }

        public async void CargarProductos()
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;

                await new TaskLoader().Execute(()=> { Productos = new ObservableCollection<ProductosTemp>(myCli.GetListaProductosProximosAVencer(CurrentCliId)); });

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.ErrorLoadingProducts, e.Message);
            }

            IsBusy = false;
        }

        public void OpenDetalleFactura(ProductosTemp data)
        {
            ShowDetalleFactura = true;



        }
    }
}
