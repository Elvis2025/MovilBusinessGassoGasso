using MovilBusiness.model.Internal;
using MovilBusiness.Views.Components.Views;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Popup
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PopupImageFrame : PopupPage,INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;

        private string imageproduct;
        public string ImageProduct { get => imageproduct; set { imageproduct = value; RaiseOnPropertyChanged();} }

        public PopupImageFrame(string productos = null)
        {
            ImageProduct = productos;
            BindingContext = this;
            InitializeComponent();
        }
        private  void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
             PopupNavigation.Instance.RemovePageAsync(this);
             Navigation.PushModalAsync(new ShowImages(ImageProduct));
        }
        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}