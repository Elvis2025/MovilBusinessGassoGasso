using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.Resx;
using MovilBusiness.viewmodel;
using MovilBusiness.Views;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RecibosTabPageOP : TabbedPage
    {
        private bool Finish = false;       
        private bool IsFromEditar = false;
        public RecibosTabPageOP(int VenSecuencia = -1, Monedas moneda = null, bool IsFromEditar = false)
        {
            
            this.IsFromEditar = IsFromEditar;
             var vm = new RecibosViewModel(this, () => { Finish = true; }, VenSecuencia, moneda, IsFromEditar) { OnCurrentPageChanged = SetPage };
           
            BindingContext = vm;

            InitializeComponent();
         

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
           
            AlertSalir();
            return true;
        }

        private async void AlertSalir()
        {
            var result = await DisplayAlert(AppResource.Warning, AppResource.WantToLeaveQuestion, AppResource.Yes, "No");

            if (result)
            {
                await Navigation.PopAsync(true);
            }
        }
    }
}