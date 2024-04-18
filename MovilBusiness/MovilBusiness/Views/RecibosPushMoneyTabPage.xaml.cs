using MovilBusiness.ViewModel;
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
    public partial class RecibosPushMoneyTabPage : TabbedPage
    {
        private bool Finish = false;

        public RecibosPushMoneyTabPage ()
        {
            BindingContext = new RecibosPushMoneyViewModel(this, ()=> { Finish = true; });
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Finish)
            {
                Navigation.PopAsync(false);
            }
        }
    }
}