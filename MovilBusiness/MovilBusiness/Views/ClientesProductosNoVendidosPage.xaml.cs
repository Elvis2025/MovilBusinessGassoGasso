using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.Resx;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ClientesProductosNoVendidosPage : ContentPage, INotifyPropertyChanged
	{
        public new event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Clientes> productos;
        public ObservableCollection<Clientes> ClientesProductosNovendidos { get => productos; set { productos = value; RaiseOnPropertyChanged(); } }
        private DS_Clientes myCli;

        private string cantidadclientes;
        public string cantidadClientes { get => cantidadclientes; set { cantidadclientes = value; RaiseOnPropertyChanged(); } }

        private string selectedline;
        public string SelectedLine { get => selectedline; set { selectedline = value; RaiseOnPropertyChanged(); } }

        private string cantidadclientesincomprar;
        public string cantidadClientesSinComprar { get=> cantidadclientesincomprar; set { cantidadclientesincomprar = value; RaiseOnPropertyChanged(); } }
        
        private string cantidadclientescompraron;
        public string cantidadClientesCompraron { get=> cantidadclientescompraron; set { cantidadclientescompraron = value; RaiseOnPropertyChanged(); } }

        public string lbltitle { get; set; }

        public ClientesProductosNoVendidosPage(string tabla, string linID, bool isOnline, bool IsFromProduct,bool IsRutaVisita,string Cliids,string linDescripcion)
		{
            myCli = new DS_Clientes();
            BindingContext = this;
           
            lbltitle = IsFromProduct ? AppResource.ProductLabel : AppResource.LineLabel;
            
            InitializeComponent();

            CargarProductos(tabla, linID, isOnline, IsFromProduct, IsRutaVisita, Cliids, linDescripcion);
        }

        private async void CargarProductos(string tabla, string linID, bool isOnline, bool IsFromProduct, bool IsRutaVisita, string Cliids, string linDescripcion)
        {
            ClientesProductosNovendidos = new ObservableCollection<Clientes>(await myCli.GetClientesNoVendidos(tabla, linID, isOnline, IsRutaVisita, Cliids, IsFromProduct));
            cantidadClientesSinComprar = ClientesProductosNovendidos.Count.ToString();
            int totalclientes = Math.Abs(!IsRutaVisita? myCli.GetClienteForCounts() : myCli.GetClientesPendientesAContar());
            cantidadClientes = totalclientes.ToString();
            cantidadClientesCompraron = (totalclientes - ClientesProductosNovendidos.Count).ToString();
            SelectedLine = linDescripcion;

            IsBusy = false;
        }


        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}