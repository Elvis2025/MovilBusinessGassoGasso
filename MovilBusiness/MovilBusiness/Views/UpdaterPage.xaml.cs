using MovilBusiness.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class UpdaterPage : ContentPage
	{
		public UpdaterPage ()
		{
            BindingContext = new AppUpdateViewModel(this);

			InitializeComponent ();
		}

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ((AppUpdateViewModel)BindingContext).CheckForUpdates();

        }

        protected override bool OnBackButtonPressed()
        {
            ((AppUpdateViewModel)BindingContext).cancelToken.Cancel();

            return base.OnBackButtonPressed();
        }
    }
}