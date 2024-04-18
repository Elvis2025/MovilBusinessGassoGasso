using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Popup
{
    public partial class ImagePreview : PopupPage, INotifyPropertyChanged
    {

        Action<List<ImageSource>> ActionPreview;

        private List<ImageSource> collimageview;

        public List<ImageSource> CollImageView { get => collimageview; set { collimageview = value; RaiseOnPropertyChanged(); } }

        public new event PropertyChangedEventHandler PropertyChanged;

        public ImagePreview(List<ImageSource> imagePreview, Action<List<ImageSource>> actionPreview)
        {
            CollImageView = imagePreview;
            ActionPreview = actionPreview;

            InitializeComponent();
            BindingContext = this;
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            CollImageView.Remove(((Frame)sender).BindingContext as ImageSource);
            ActionPreview.Invoke(CollImageView);
        }

        protected override void OnDisappearing()
        {
            MessagingCenter.Send("Start", "Start");
            base.OnDisappearing();
        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
