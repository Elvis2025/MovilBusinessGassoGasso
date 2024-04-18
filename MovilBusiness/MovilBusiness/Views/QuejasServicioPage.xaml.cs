using MovilBusiness.ViewModel;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QuejasServicioPage : ContentPage
    {
        public QuejasServicioPage(int editedQueSecuencia = -1, bool isDetail = false)
        {
            BindingContext = new QuejasServicioViewModel(this, editedQueSecuencia, isDetail);
            InitializeComponent();

            if (isDetail)
            {
                ToolbarItems.Clear();
            }
        }
    }
}