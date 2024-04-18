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
    public partial class OperativosMedicosPage : ContentPage
    {
        public OperativosMedicosPage()
        {
            BindingContext = new OperativosMedicosViewModel(this);

            InitializeComponent();
        }

        private async void OnDeleteDetalle(object sender, EventArgs e)
        {
            if(BindingContext is OperativosMedicosViewModel vm && sender is Image btn && btn.BindingContext != null)
            {
                var result = await DisplayAlert(AppResource.Warning, AppResource.WantRemoveDetailQuestion, AppResource.Remove, AppResource.Close);

                if (!result)
                {
                    return;
                }

                vm.DeleteDetalle(btn.BindingContext.ToString());
            }
        }

        private void list_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if(e.SelectedItem == null)
            {
                return;
            }

            list.SelectedItem = null;
        }
    }
}