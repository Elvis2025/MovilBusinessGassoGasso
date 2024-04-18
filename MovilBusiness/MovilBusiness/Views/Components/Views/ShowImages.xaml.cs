using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShowImages : ContentPage, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;

        private string imageproduct;
        public string ImageProduct { get => imageproduct; set { imageproduct = value; RaiseOnPropertyChanged(); } }

        public ShowImages(string productos)
        {
            ImageProduct = productos;
            BindingContext = this;
            InitializeComponent();

        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}