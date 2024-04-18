using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Resx;
using MovilBusiness.ViewModel;
using MovilBusiness.Views;
using Rg.Plugins.Popup.Services;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PedidosPage : MasterDetailPage
    {
        private bool FirstTime = true;
        //public static bool comesInDetail = false;
        public bool Autorizado = false;

        public static bool Finish = false;

       // private bool IsConteoFisico = false;

        public PedidosPage(bool isEditing = false, int EditedTranSecuencia = 1, bool fromCopy = false, string repAuditor = null, bool autorizado = false, bool FromPedido = false, int almId = -1, bool IsPushMoneyRotacion = false, bool isFromCot2Ven = false, int conId = -1)
        {
            //IsConteoFisico = Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS;

            if (FromPedido)
            {
                Navigation.PopAsync(true);
            }
            Autorizado = autorizado;

            BindingContext = new PedidosViewModel(this, EditedTranSecuencia, isEditing, fromCopy, repAuditor, almId:almId, isFromCot2Ven: isFromCot2Ven, conId:conId) { OnOptionMenuItemSelected = () => { IsPresented = false; }, IsPushMoneyRotacion = IsPushMoneyRotacion };
            
            InitializeComponent();

            if (Arguments.Values.CurrentModule == Modules.COMPRAS || Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS || Arguments.Values.CurrentModule == Modules.INVFISICO)
            {
                if (Arguments.Values.CurrentModule == Modules.COMPRAS)
                {
                    if (!DS_RepresentantesParametros.GetInstance().GetParComprasNoUsarDependiente())
                    {
                        ((TabbedPage)((NavigationPage)Detail).CurrentPage).Children.Add(new DependientesTabPage());
                    }
                    if (DS_RepresentantesParametros.GetInstance().GetParComprasFormaPagoAutomatica() < 1)
                    {
                        ((TabbedPage)((NavigationPage)Detail).CurrentPage).Children.Add(new RecibosPushMoneyFormaPagos());
                    }
                }

                if (DS_RepresentantesParametros.GetInstance().OcultarTabConfigurar())
                {
                    ((TabbedPage)((NavigationPage)Detail).CurrentPage).Children.Remove(configurarTab);
                }
            }

            switch (Arguments.Values.CurrentModule)
            {
                case Modules.PEDIDOS:
                    Title = AppResource.Orders;
                    break;
                case Modules.DEVOLUCIONES:
                    Title = AppResource.Returns;
                    break;
                case Modules.INVFISICO:
                    Title = AppResource.PhysicalInventory;
                    break;
                case Modules.COMPRAS:
                    Title = IsPushMoneyRotacion ? AppResource.PushMoneyRotation : DS_RepresentantesParametros.GetInstance().GetParCambiarNombreComprasPorPushMoney() ? "PushMoney" : AppResource.Purchases;
                    break;
                case Modules.VENTAS:
                    Title = AppResource.Sales;
                    break;
                case Modules.COTIZACIONES:
                    Title = AppResource.Quotes;
                    break;
                case Modules.CONTEOSFISICOS:
                    Title = AppResource.PhysicalCount;
                    break;
                case Modules.REQUISICIONINVENTARIO:
                    Title = AppResource.InventoryRequisition;
                    break;
                case Modules.CAMBIOSMERCANCIA:
                    Title = AppResource.MerchandiseChange;
                    break;
                case Modules.TRASPASOS:
                    Title = AppResource.Transfers;
                    break;
                case Modules.PROMOCIONES:
                    Title = AppResource.Promotions;
                    break;
                case Modules.ENTREGASMERCANCIA:
                    Title = AppResource.MerchandiseDelivery;
                    break;
                case Modules.COLOCACIONMERCANCIAS:
                    Title = AppResource.PlacementsOfMerchandise;
                    break;
            }

            ((TabbedPage)((NavigationPage)Detail).CurrentPage).Title = Title;

            NavigationPage.SetBackButtonTitle(this, AppResource.Back);

            if ((DS_RepresentantesParametros.GetInstance().GetParPedidosIniciarEnConfiguracion() || Arguments.Values.CurrentModule == Modules.TRASPASOS)/* && !comesInDetail*/)
            {
                ((TabbedPage)((NavigationPage)Detail).CurrentPage).CurrentPage = ((TabbedPage)((NavigationPage)Detail).CurrentPage).Children[1];
                //comesInDetail = true;
            }
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                var antsmod = Arguments.Values.ANTSMODULES;
                if (Finish && antsmod == Modules.NULL && antsmod != Modules.AGAIN)
                {
                    Finish = false;

                    int porciento = DS_RepresentantesParametros.GetInstance().GetParCantidadDividirPrecioDevolucion();
                   if (Arguments.Values.CurrentModule == Modules.DEVOLUCIONES && Arguments.Values.IsUpdatePrecioForDev && porciento != -1)
                   {
                      new DS_ListaPrecios().UpdatePrecioForDev(" * " + porciento.ToString(),0,1);
                      Arguments.Values.IsUpdatePrecioForDev = false;
                   }

                    await Navigation.PopAsync(false);
                }
                else
                {
                  if (BindingContext is PedidosViewModel vm)
                  {
                        if (vm.FromConsultInventory)
                        {
                            vm.FromConsultInventory = false;
                            Arguments.Values.CurrentModule = Modules.PEDIDOS;
                        }

                      var myParametro = DS_RepresentantesParametros.GetInstance();

                      if (!((Arguments.Values.CurrentModule == Modules.DEVOLUCIONES ||
                             myParametro.GetParCotOfertasManuales() ||
                             myParametro.GetParVenOfertasManuales() ||
                             myParametro.GetParPedOfertasManuales()) ||
                            myParametro.GetParPedidosDescuentoManual()))
                      {
                          vm.DeleteOfertaAndDescuentosInTemp();
                      }
                      vm.FirstTime = false;


                      if (FirstTime && (myParametro.GetParDevolucionesProductosFacturas() || myParametro.GetParEntregasMercanciasPorFactura()) && !vm.IsEditing)
                      {
                          vm.SeleccionarProductosHistoricoFactura();
                      }
                      else if (Arguments.Values.CurrentModule == Modules.VENTAS && vm.HayPedidosPorEntregar() && FirstTime)
                      {
                          vm.SeleccionarPedidoParaEntregar();
                      }
                      else
                      {
                          if ((vm.IsEditing || vm.HasProductsInTemp()) && FirstTime)
                          {
                              vm.SearchUnAsync(true);
                          }
                          else if (FirstTime && (myParametro.GetParModulosCargasProductosAuto() || Arguments.Values.CurrentModule == Modules.ENTREGASMERCANCIA || Arguments.Values.CurrentModule == Modules.PROMOCIONES))
                          {
                              vm.SearchUnAsync(false);
                          }
                      }
                      if (FirstTime)
                      {
                          if (vm.Checkdays() || vm.CheckChequesDevueltosSinSaldar())
                          {
                              await Navigation.PopAsync(true);
                          }
                      }
                  }

                  if (FirstTime)
                  {
                      FirstTime = false;
                  }
                }
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }
        }

        protected override bool OnBackButtonPressed()
        {
            AlertSalir();
            return true;
        }

        private async void AlertSalir()
        {
            if (PopupNavigation.Instance.PopupStack.Count > 0)
            {
                await PopupNavigation.Instance.PopAllAsync(true);                
                return;
            }

            var result = await DisplayAlert(AppResource.Warning, AppResource.WantToLeaveQuestion, AppResource.Yes, AppResource.No);

            if (result)
            {
                //comesInDetail = false;

                int porciento = DS_RepresentantesParametros.GetInstance().GetParCantidadDividirPrecioDevolucion();
                if (Arguments.Values.CurrentModule == Modules.DEVOLUCIONES && Arguments.Values.IsUpdatePrecioForDev && porciento != -1)
                {
                    new DS_ListaPrecios().UpdatePrecioForDev(" * " + porciento.ToString(),0,1);
                    Arguments.Values.IsUpdatePrecioForDev = false;
                }

                ((PedidosViewModel)BindingContext).ClearTemp();
                Arguments.Values.IsPedidoAutorizado = false;
                await Navigation.PopAsync(true);
            }
        }
    }
}
