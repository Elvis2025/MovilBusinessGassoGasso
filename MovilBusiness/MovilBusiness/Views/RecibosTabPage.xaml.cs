using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using MovilBusiness.Views;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RecibosTabPage : TabbedPage
    {
        private bool Finish = false;
        private bool FromVenta = false;
        // private bool IsFromEditar = false;
        
        public RecibosTabPage(int VenSecuencia = -1, Monedas moneda = null, bool IsFromEditar = false, double tasaOriginal = 0, List<int> EntregasSecuencias = null, int ConId = -1, bool IsFromCopy = false, bool IsConsulting = false)
        {
            FromVenta = VenSecuencia != -1;
            //this.IsFromEditar = IsFromEditar;
            var vm = new RecibosViewModel(this, () => { Finish = true; }, VenSecuencia, moneda, IsFromEditar, ConId, IsFromCopy: IsFromCopy, IsConsulting) { OnCurrentPageChanged = SetPage };
            vm.EntregasSecuencias = EntregasSecuencias;
           
            BindingContext = vm;

            if (moneda != null && tasaOriginal != 0 && vm.CurrentMoneda != null)
            {
                vm.CurrentMoneda.MonTasa = tasaOriginal;//CobrosViewModel.TasaOriginal; //plz no variables estaticas cojollo
            }

            InitializeComponent();

            Init(IsFromEditar);
        }

        private void Init(bool IsFromEditar)
        {
            if (Arguments.Values.CurrentModule == Enums.Modules.RECONCILIACION)
            {
                Title = AppResource.Reconciliation;
                Children.RemoveAt(1);
            }

            if (DS_RepresentantesParametros.GetInstance().GetParRecibosTabGeneral() && !IsFromEditar && Arguments.Values.CurrentModule != Enums.Modules.RECONCILIACION)
            {
                var general = new RecibosGeneralTab();
                Children.Insert(0, general);
                SetPage(0);
            }
        }

        protected override void OnAppearing()
        {
            if (Finish)
            {
                Navigation.PopAsync(false);
            }
            else
            {
                base.OnAppearing();
                if (BindingContext is RecibosViewModel vm)
                {
                    if (vm.FirstTime)
                    {
                        if (vm.ShowTaza && vm.CurrentMoneda != null)
                        {
                            vm.AlertaTaza();
                        }
                        vm.FirstTime = false;
                    }
                }
            }
        }

        private void SetPage(int pos)
        {
            if (CurrentPage != Children[pos])
            {
                CurrentPage = Children[pos];
            }
        }

        protected override bool OnBackButtonPressed()
        {
            if (FromVenta)
            {
                return true;
            }

            AlertSalir();
            return true;
        }

        private async void AlertSalir()
        {
            var result = false;
            if (Arguments.Values.CurrentModule == Enums.Modules.VENTAS || Arguments.Values.CurrentModule == Enums.Modules.PEDIDOS || Arguments.Values.CurrentModule == Enums.Modules.ENTREGASREPARTIDOR) {
                await DisplayAlert(AppResource.Warning, AppResource.OrphanReceiptWarning, AppResource.Aceptar);
            }
            else
            {
                result = await DisplayAlert(AppResource.Warning, AppResource.WantToLeaveQuestion, AppResource.Yes, AppResource.No);
            }

            if (result)
            {
                await Navigation.PopAsync(true);
            }
        }
    }
}