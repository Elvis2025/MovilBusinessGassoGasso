using MovilBusiness.model.Internal;
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
    public partial class AuditoriaPreciosPage : ContentPage
    {
        public AuditoriaPreciosPage()
        {
            BindingContext = new AuditoriaPreciosViewModel(this);
            InitializeComponent();
        }

        private void list_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if(e.SelectedItem == null)
            {
                return;
            }

            if(BindingContext is AuditoriaPreciosViewModel vm)
            {
                vm.OnProductSelected(e.SelectedItem as ProductosTemp);
            }

            list.SelectedItem = null;
        }

        private bool FirstTime = true;
        protected override void OnAppearing()
        {
            base.OnAppearing();

            if(FirstTime && BindingContext is AuditoriaPreciosViewModel vm)
            {
                FirstTime = false;
                vm.CurrentCategoria = null;
                vm.CurrentMarca = null;
                vm.SearchProducts();
            }
        }

        protected override bool OnBackButtonPressed()
        {
            AlertSalir();
            return true;
        }

        private async void AlertSalir()
        {
            var result = await DisplayAlert(AppResource.Warning, AppResource.WantToLeaveQuestion, AppResource.Yes, AppResource.No);

            if (result)
            {
                ((AuditoriaPreciosViewModel)BindingContext).ClearTemp();
                
                await Navigation.PopAsync(true);
            }
        }
    }
}