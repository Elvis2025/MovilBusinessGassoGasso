using MovilBusiness.DataAccess;
using MovilBusiness.Views.Components.Popup;
using Rg.Plugins.Popup.Contracts;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CameraPage : ContentPage
	{
        private DS_TransaccionesImagenes myTranImg;
        public string tableName { get; set; }
        public string repTablaKey { get; set; } = null;

        public static List<ImageSource> PreviewRenderer;
        public static List<byte[]> PreviewRendererToAdd;

        public ImageSource source;
		public CameraPage (string repTablaKey, string tableName)
		{
            myTranImg = new DS_TransaccionesImagenes();

            this.repTablaKey = repTablaKey;
            this.tableName = tableName;

            InitializeComponent ();

            MessagingCenter.Subscribe<string>(this, "SavePictures", (msg) => 
            {
                if (msg != null && msg == "SavePictures")
                {
                    myTranImg.MarkToSendToServer(tableName, repTablaKey);
                    MessagingCenter.Unsubscribe<string>(this, "SavePictures");
                    MessagingCenter.Unsubscribe<string>(this, "Finish");
                    Navigation.PopAsync(false);
                }
            });

            MessagingCenter.Subscribe<string>(this, "Finish", (msg) => 
            {
                if (msg == "Finish")
                {
                    MessagingCenter.Unsubscribe<string>(this, "Finish");
                    MessagingCenter.Unsubscribe<string>(this, "SavePictures");
                    Navigation.PopAsync(false);
                }
            });


            MessagingCenter.Subscribe<string>(this, "StartPreview", (msg) =>
            {
                if (msg == "StartPreview")
                {
                    Navigation.PushPopupAsync(new ImagePreview
                    (PreviewRenderer,
                    (actionlistpreview) =>
                    {
                        if (PopupNavigation.PopupStack.Count > 0)
                            PopupNavigation.Instance.PopAllAsync();
                        PreviewRenderer = actionlistpreview;
                        MessagingCenter.Send("StopPreview", "StopPreview");
                    }));
                }
            });

            if (Device.RuntimePlatform != Device.iOS)
            {
                ToolbarItems.Clear();
            }
		}

        protected override void OnDisappearing()
        {
            MessagingCenter.Send("StopPreview", "StopPreview");            
            base.OnDisappearing();
        }

        private void OnSaveClicked(object sender, EventArgs args)
        {
            foreach(var imagepreview in PreviewRendererToAdd)
            {
                myTranImg.SaveImagenInTemp(imagepreview, tableName, repTablaKey);
            }            

            myTranImg.MarkToSendToServer(tableName, repTablaKey);
            Navigation.PopAsync(true);
        }

        protected override bool OnBackButtonPressed()
        {
            if (PopupNavigation.Instance.PopupStack.Count > 0)
            {
                PopupNavigation.Instance.PopAllAsync(true);                
            }

            return base.OnBackButtonPressed();
        }
    }
}