using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.QrCode;

namespace MovilBusiness.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProCodigosBarraModal : ContentPage
    {
        public ProCodigosBarraModal( string barCode)
        {
            InitializeComponent();
            var barCodeOptions = new QrCodeEncodingOptions
            {
                Width = 600,
                Height = 300
            };

            barCodeView.BarcodeOptions = barCodeOptions;
            barCodeView.BarcodeValue = barCode;
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }
    }
}