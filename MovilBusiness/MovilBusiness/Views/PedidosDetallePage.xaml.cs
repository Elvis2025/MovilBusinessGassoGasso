using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.Model.Internal.Structs.Args;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PedidosDetallePage : ContentPage
    {
        public bool IsDetail { set { SetIsDetail(value); } }
        public static bool Finish = false;
        private bool firstTime = true;
        public static bool ClearFromDetalle = false;
        private DS_Productos myProd;


        public PedidosDetallePage(PedidosDetalleArgs args, bool fromTransacciones = false, string cxcDocumento = null, CuentasxCobrar documento = null)
        {
            BindingContext = new PedidosDetalleViewModel(this, args, fromTransacciones, documento);
            myProd = new DS_Productos();

            InitializeComponent();

            if (!string.IsNullOrWhiteSpace(cxcDocumento))
            {
                lblDocumento.Text = AppResource.DocumentLabel + cxcDocumento;
                lblDocumento.IsVisible = true;
            }

            //SetListDataTemplate(RowDesign);
            Functions.SetListViewItemTemplateById(ListaProductos, args.DisenoDelRow == 20 ? 1 : args.DisenoDelRow, false);
            ClearFromDetalle = fromTransacciones;
            SetTitle();
        }

        private void SetTitle()
        {
            switch (Arguments.Values.CurrentModule)
            {
                case Modules.PEDIDOS:
                    Title = AppResource.OrderDetail;
                    break;
                case Modules.DEVOLUCIONES:
                    Title = AppResource.ReturnsDetail;
                    break;
                case Modules.INVFISICO:
                    Title = AppResource.PhysicalInventoryDetail;
                    break;
                case Modules.COLOCACIONMERCANCIAS:
                    Title = AppResource.PlacementsOfMerchandiseDetail;
                    break;
                case Modules.VENTAS:
                    Title = AppResource.SalesDetail;
                    break;
                case Modules.COTIZACIONES:
                    Title = AppResource.QuotesDetail;
                    break;
                case Modules.COMPRAS:
                    Title = DS_RepresentantesParametros.GetInstance().GetParCambiarNombreComprasPorPushMoney() ? "PushMoney detalle" : "Compras detalle";
                    break;
                case Modules.CONTEOSFISICOS:
                    Title = AppResource.PhysicalCountDetail;
                    break;
                case Modules.CAMBIOSMERCANCIA:
                    Title = AppResource.MerchandiseChangeDetail;
                    break;
                case Modules.TRASPASOS:
                    Title = AppResource.TransfersDetail;
                    break;
                case Modules.PROMOCIONES:
                    Title = AppResource.PromotionsDetail;
                    break;
                case Modules.ENTREGASMERCANCIA:
                    Title = AppResource.MerchandiseDeliveryDetail;
                    break;
                case Modules.REQUISICIONINVENTARIO:
                    Title = AppResource.InventoryRequisitionDetail;
                    break;
            }
        }

        private void SetIsDetail(bool value)
        {
            if (value)
            {
                ToolbarItems.Clear();

            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Finish)
            {
                Finish = false;

                if (BindingContext is PedidosDetalleViewModel vm)
                {
                    Navigation.PopAsync(false);

                    if (ClearFromDetalle)
                    {
                        vm.ClearTemp();
                    }
                }
            }
            else
            {
                if (BindingContext is PedidosDetalleViewModel vm && firstTime)
                {
                    vm.LoadProducts(firstTime, false);
                    vm.SubscribeToListeners();
                    firstTime = false;

                }
            }
        }

        protected override bool OnBackButtonPressed()
        {

            if(Arguments.Values.ANTSMODULES == Modules.ANTMODULE)
            {
                Arguments.Values.ANTSMODULES = Modules.NULL;
                Arguments.Values.CurrentModule = Modules.CONTEOSFISICOS;
            }
  
            if (ClearFromDetalle)
            {
                myProd.ClearTemp((int)Arguments.Values.CurrentModule);
            }

            if(Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.VENTAS)
            {
                SqliteManager.GetInstance().Execute("update ProductosTemp set OfeCaracteristica= '' , PedOfeCantidad= 0, CantidadOferta = 0, Descuento = Precio * DesPorcientoManual / 100 , DesPorciento = DesPorcientoManual,  Cantidad = ifnull(Cantidad,0) - ifnull(PedOfeCantidad, 0), Precio = PrecioBase, OfeID= 0 " +
                    "where ifnull(PedOfeCantidad, 0) > 0 and TitID = ? and ifnull(OfeCaracteristica,'') <> '' ", new string[] { ((int)Arguments.Values.CurrentModule).ToString() });

            }
            

            if (!DS_RepresentantesParametros.GetInstance().GetParPedidosOfertasyDescuentosManuales() && !DS_RepresentantesParametros.GetInstance().GetParCotOfertasManuales() && !DS_RepresentantesParametros.GetInstance().GetParVenOfertasManuales() && !DS_RepresentantesParametros.GetInstance().GetParPedOfertasManuales())
            {
                myProd.ClearTempOfertas((int)Arguments.Values.CurrentModule);
            }
            
            Navigation.PopAsync(true);
            return true;
        }
    }
}
