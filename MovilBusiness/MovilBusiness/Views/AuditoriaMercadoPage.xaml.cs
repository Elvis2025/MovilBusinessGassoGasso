using MovilBusiness.Model.Internal;
using MovilBusiness.Resx;
using MovilBusiness.ViewModel;
using MovilBusiness.Views.Components.ListItemRows;
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
	public partial class AuditoriaMercadoPage : TabbedPage
    {
		public AuditoriaMercadoPage ()
		{
            var vm = new AuditoriaMercadoViewModel(this);

            var parAuditoria = vm.ParAuditoria;

            BindingContext = vm;

			InitializeComponent ();

            switch (vm.ParAuditoria)
            {
                case 1:
                    list.ItemTemplate = new DataTemplate(typeof(RowAuditoriaMercado1));
                    break;
                case 2:
                    list.ItemTemplate = new DataTemplate(typeof(RowAuditoriaMercado2));
                    break;
                case 3:
                    list.ItemTemplate = new DataTemplate(typeof(RowAuditoriaMercado3));
                    break;
            }
		}

        private void OnListItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            if(args.SelectedItem == null)
            {
                return;
            }

            if(BindingContext is AuditoriaMercadoViewModel vm)
            {
                vm.OnListItemSelected(args.SelectedItem as AuditoriasMercadosTemp);
            }
            
            list.SelectedItem = null;
        }

        protected override bool OnBackButtonPressed()
        {
            AlertSalir();
            return true;
        }

        private bool finishing;
        private async void AlertSalir()
        {
            if (finishing)
            {
                return;
            }
            finishing = true;
            var result = await DisplayAlert(AppResource.Warning, AppResource.WantToLeaveQuestion, AppResource.Yes, AppResource.No);

            if (result)
            {
                ((AuditoriaMercadoViewModel)BindingContext).ClearTemp();
                await Navigation.PopAsync(true);
            }
            finishing = false;
        }

    }
}