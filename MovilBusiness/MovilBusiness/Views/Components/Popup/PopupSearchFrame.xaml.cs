using MovilBusiness.DataAccess;
using MovilBusiness.model;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Popup
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PopupSearchFrame : PopupPage, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;
        private ObservableCollection<Clientes> clientes { get; set; }
        public ObservableCollection<Clientes> Clientes { get => clientes; set { clientes = value; RaiseOnPropertyChanged(); } }

        private List<Clientes> ClientesTogive;
        public Action<Clientes> ItemSelected { get; set; }
        public ICommand SearchCommand { get; private set; }
        public PopupSearchFrame()
        {
            ClientesTogive = new DS_Clientes().GetAllClientesRutPosicion();

            Clientes = new ObservableCollection<Clientes>(ClientesTogive);
            SearchCommand = new Command(() =>
            {
                Clientes = new ObservableCollection<Clientes>(ClientesTogive.Where(l => l.CliNombre.ToUpper().Contains(entrysearch.Text.ToUpper()) || 
                l.CliEnumerator.ToString().Contains(entrysearch.Text.ToUpper())).ToList());
            });

            InitializeComponent();

            BindingContext = this;            
        }
        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if(e.SelectedItem != null)
            {
                PopupNavigation.Instance.RemovePageAsync(this);
                ItemSelected?.Invoke(e.SelectedItem as Clientes);
            }
        }

        private void BorderlessEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            Clientes = new ObservableCollection<Clientes>(ClientesTogive.Where(l => l.CliNombre.ToUpper().Contains(entrysearch.Text.ToUpper()) ||
            l.CliEnumerator.ToString().Contains(entrysearch.Text.ToUpper())).ToList());
        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}