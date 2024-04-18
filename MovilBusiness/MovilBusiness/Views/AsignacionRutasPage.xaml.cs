using MovilBusiness.Model;
using MovilBusiness.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AsignacionRutasPage : ContentPage
    {
        public static bool Finish{get;set;} = false;
		public AsignacionRutasPage ()
		{
            BindingContext = new AsignacionRutasViewModel(this);
			InitializeComponent ();
		}

        private void OnListItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            if(args.SelectedItem == null)
            {
                return;
            }

            ((AsignacionRutasViewModel)BindingContext).OnListItemSelected(args.SelectedItem as RutaVisitasFecha);

            list.SelectedItem = null;
        }

        private void Filter_OnSegmentSelected(object sender, int e)
        {
            ((AsignacionRutasViewModel)BindingContext).SearchAsigned(filter.SelectedSegment == 0);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (Finish)
            {
                Finish = false;
                Navigation.PopAsync(false);
            }
        }

    }
}