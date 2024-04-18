using MovilBusiness.Abstraction;
using MovilBusiness.DataAccess;
using MovilBusiness.Model.Internal;
using MovilBusiness.Resx;
using MovilBusiness.Services;
using MovilBusiness.views;
using Plugin.Connectivity;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SincronizarModal : ContentPage, INotifyPropertyChanged
	{
        public new event PropertyChangedEventHandler PropertyChanged;

        private double progresstotal;

        public double ProgressTotal { get => progresstotal; set { progresstotal = value; RaiseOnPropertyChanged(); } }

        private string messagetotalprogress = AppResource.TotalProgressLabel + "0%";

        public string MessageTotalProgress { get => messagetotalprogress; set { messagetotalprogress = value; RaiseOnPropertyChanged(); } }

        private int CurrentStep = 1;

        private SyncStepItem step1;
        private SyncStepItem step2;
        private SyncStepItem step3;
        private SyncStepItem step4;

        public SyncStepItem Step1 { get => step1; set { step1 = value; RaiseOnPropertyChanged(); } }
        public SyncStepItem Step2 { get => step2; set { step2 = value; RaiseOnPropertyChanged(); } }
        public SyncStepItem Step3 { get => step3; set { step3 = value; RaiseOnPropertyChanged(); } }
        public SyncStepItem Step4 { get => step4; set { step4 = value; RaiseOnPropertyChanged(); } }

        private Action OnSyncCompleted;

        int _valcua;

        public SincronizarModal (int valcua = 0, Action OnSyncCompleted = null)
		{
            _valcua = valcua;

            int nav = Navigation.NavigationStack.Count;

            this.OnSyncCompleted = OnSyncCompleted;

            Step1 = new SyncStepItem()
            {
                IndicatorColor = Color.FromHex("#1976D2"),
                TitleColor = Color.Black,
                IsVisible = true,
                Message = "",
                Progress = 0
            };

            Step2 = new SyncStepItem()
            {
                IndicatorColor = Color.DarkGray,
                TitleColor = Color.DarkGray,
                IsVisible = false,
                Message = "",
                Progress = 0
            };

            Step3 = new SyncStepItem()
            {
                IndicatorColor = Color.DarkGray,
                TitleColor = Color.DarkGray,
                IsVisible = false,
                Message = "",
                Progress = 0
            };

            Step4 = new SyncStepItem()
            {
                IndicatorColor = Color.DarkGray,
                TitleColor = Color.DarkGray,
                IsVisible = false,
                Message = "",
                Progress = 0
            };

            BindingContext = this;

            InitializeComponent ();
		}

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Device.BeginInvokeOnMainThread(() =>
            {
                Sincronizar();
            });
        }

        private async void Sincronizar()
        {
            var screen = DependencyService.Get<IScreen>();

            try
            {

                if (!CrossConnectivity.Current.IsConnected)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.NoInternetMessage, AppResource.Aceptar);
                    await Navigation.PopModalAsync(true);
                    return;
                }

                if(screen != null)
                {
                    screen.KeepLightsOn(true);
                }

                var ws = new ServicesManager
                         (async (mensaje) => {
                            await DisplayAlert(AppResource.Success, mensaje, AppResource.Aceptar);
                            await Navigation.PopModalAsync(true);
                         })
                         { OnCurrentProgressChanged = SetProgress,};

                ws.MessageReport += SetStepMessage;

                if(!await ws.Sincronizar(SetStep))
                {
                    return;
                }

                await DisplayAlert(AppResource.Success, AppResource.SynchronizationSuccessful, AppResource.Aceptar);

            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.SynchronizationFailed, e.Message, AppResource.Aceptar);
            }

            if (screen != null)
            {
                screen.KeepLightsOn(false);
            }

            if(_valcua <= 1)
            {
                await Navigation.PopModalAsync(true);
            }

            OnSyncCompleted?.Invoke();

            foreach (var noticia in SqliteManager.GetInstance().Query<NoticiasTemp>("select * from NoticiasTemp"))
            {
                MessagingCenter.Send("Notification", "Notification",
                    new string[] {noticia.NotDescripcion, noticia.NotID.ToString() });
                SqliteManager.GetInstance().Delete(noticia);             
            }
        }

        private void SetStep(int step)
        {
            CurrentStep = step;

            switch (step)
            {
                case 1:
                    ProgressTotal = 0.10;
                    MessageTotalProgress = AppResource.TotalProgressLabel + "10%";
                    break;
                case 2:
                    Step2.IsVisible = true;
                    Step2.Progress = 0;
                    ProgressTotal = 0.25;
                    MessageTotalProgress = AppResource.TotalProgressLabel + "25%";
                    Step2.IndicatorColor = Color.FromHex("#1976D2");
                    Step2.TitleColor = Color.Black;
                    RaiseOnPropertyChanged(nameof(Step2));
                    break;
                case 3:

                    Step3.IsVisible = true;
                    Step3.Progress = 0;
                    ProgressTotal = 0.50;
                    MessageTotalProgress = AppResource.TotalProgressLabel + "50%";
                    Step3.IndicatorColor = Color.FromHex("#1976D2");
                    Step3.TitleColor = Color.Black;
                    RaiseOnPropertyChanged(nameof(Step3));
                    break;
                case 4:
                    Step4.IsVisible = true;
                    Step4.Progress = 0;
                    ProgressTotal = 0.75;
                    MessageTotalProgress = AppResource.TotalProgressLabel + "75%";
                    Step4.IndicatorColor = Color.FromHex("#1976D2");
                    Step4.TitleColor = Color.Black;
                    RaiseOnPropertyChanged(nameof(Step4));
                    break;
                case 5:
                    ProgressTotal = 1.0;
                    MessageTotalProgress = AppResource.TotalProgressLabel + "100%";
                    break;
            }
        }

        private void SetStepMessage(object sender, string value)
        {
            switch (CurrentStep)
            {
                case 1:
                    break;
                case 2:
                    Step2.Message = value;
                    RaiseOnPropertyChanged(nameof(Step2));
                    break;
                case 3:
                    Step3.Message = value;
                    RaiseOnPropertyChanged(nameof(Step3));
                    break;
                case 4:
                    break;
            }
        }

        private void SetProgress(double value, int unit)
        {
            switch (CurrentStep)
            {
                case 1:
                    Step1.Progress = value;
                    RaiseOnPropertyChanged(nameof(Step1));
                    break;
                case 2:
                    Step2.Progress = value;
                    RaiseOnPropertyChanged(nameof(Step2));
                    break;
                case 3:
                    Step3.Progress = value;
                    RaiseOnPropertyChanged(nameof(Step3));
                    break;
                case 4:
                    Step4.Progress = value;
                    RaiseOnPropertyChanged(nameof(Step4));
                    break;
            }
        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}