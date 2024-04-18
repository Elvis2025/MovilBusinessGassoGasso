using MovilBusiness.ViewModel;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LiquidacionEntregasPage : ContentPage
    {
        public LiquidacionEntregasPage()
        {
            BindingContext = new LiquidacionEntregaViewModel(this);
            InitializeComponent();
        }
    }
}