using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Popup
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ImagesPriview : PopupPage
    {
        List<MemoryStream> ImagePreview;

        Action<List<MemoryStream>> ActionPreview;

        public ImagesPriview(List<MemoryStream> imagePreview, Action<List<MemoryStream>> actionPreview)
        {
            ImagePreview = imagePreview;

            ActionPreview = actionPreview;

            InitializeComponent();
            BindingContext = this;
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            ImagePreview.Remove(collview.CurrentItem as MemoryStream);
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            PopupNavigation.Instance.RemovePageAsync(this);
            ActionPreview.Invoke(ImagePreview);
        }

        protected override void OnDisappearing()
        {
            MessagingCenter.Send("Start", "Start");
            base.OnDisappearing();
        }
    }
}