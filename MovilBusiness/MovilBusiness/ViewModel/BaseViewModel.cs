
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.Views.Components.Modals;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MovilBusiness.viewmodel
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Modules CurrentModules = Modules.NULL;
        protected Page page { get; private set; }

        private INavigation Navigation;

        private bool isbusy;
        public bool IsBusy { get { return isbusy; } set { isbusy = value; RaiseOnPropertyChanged(); } }

        private bool isup = true;
        public bool IsUp { get { return isup; } set { isup = value; SaveCommand.ChangeCanExecute();} }

        public Command SaveCommand { get; set; }

        protected DS_RepresentantesParametros myParametro;

        public BaseViewModel(Page page)
        {
            this.page = page;
            Navigation = page.Navigation;
            myParametro = DS_RepresentantesParametros.GetInstance();

        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected Task PushAsync(Page page)
        {
            return Navigation.PushAsync(page);
        }

        protected Task PushModalAsync(Page page)
        {
            return Navigation.PushModalAsync(page);
        }

        protected Task PopAsync(bool animate)
        {
            return Navigation.PopAsync(animate);
        }

        protected Task PopModalAsync(bool animate)
        {
            return Navigation.PopModalAsync(animate);
        }

        protected Task DisplayAlert(string title, string message, string cancel = "Ok")
        {
            return page.DisplayAlert(title, message, cancel);
        }

        protected Task<bool> DisplayAlert(string title, string message, string accept, string cancel = "Ok")
        {
            return page.DisplayAlert(title, message, accept, cancel);
        }

        protected Task<string> DisplayActionSheet(string Title, string Button = "", string destruction = null, params string[] buttons)
        {
            if (string.IsNullOrWhiteSpace(Button))
            {
                return page.DisplayActionSheet(Title, "", destruction, buttons);
            }
            else if (Button == AppResource.Aceptar)
            {
                return page.DisplayActionSheet(Title, Button, destruction, buttons);
            }
            else
            {
                return page.DisplayActionSheet(Title, AppResource.Cancel, destruction, buttons);
            }
        }

        protected Task<string> DisplayActionSheet(string title, string button, params string[] buttons)
        {
            return page.DisplayActionSheet(title, button, null, buttons);
        }

        public virtual bool OnBackButtonPressed()
        {
            return true;
        }

        public virtual void ListenForLocationUpdatesIfPermission()
        {
            if (myParametro.GetParGPS())
            {
                Functions.StartListeningForLocations();
            }
        }
    }
}
