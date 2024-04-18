using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.Resx;
using MovilBusiness.ViewModel;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProductosNoVendidosPage : ContentPage
    {
        private string tabla;
        private string tipofiltro;
        private bool isonline;
        private bool IsRutaVisita { get; set; }
        private string CliidNovendidos { get; set; }
      
        public ProductosNoVendidosPage(string cliids = "",bool IsRutaVisita=false)
        {

            BindingContext = new ProductosNoVendidosViewModel(this);
            this.IsRutaVisita = IsRutaVisita;
            CliidNovendidos = cliids;
        
            InitializeComponent();


            var searchByOptions = new List<string>();
            searchByOptions.Add(AppResource.CurrentMonth);
            searchByOptions.Add("General");
            comboBuscarPor.ItemsSource = searchByOptions;


            var filterOptions = new List<string>();
            filterOptions.Add(AppResource.Products);
            filterOptions.Add(AppResource.Line);
            comboFiltrar.ItemsSource = filterOptions;

            if (DS_RepresentantesParametros.GetInstance().GetParProductosNoVendidosOnline())
            {
                Comboconexion.SelectedIndex = 2;
            }
           
        }

        private async void ComboBuscarPor_SelectedIndexChangedAsync(object sender, EventArgs e)
        {
            try
            {
              
                if (Comboconexion.IsVisible)
                {
                   
                    if(Comboconexion.SelectedIndex==-1)
                    {
                        await DisplayAlert(AppResource.Connection, AppResource.MustSelectConnectionType, AppResource.Aceptar);
                    }
                    isonline = Comboconexion.SelectedIndex == 0 ? false : true;
                }
                tabla = comboBuscarPor.SelectedIndex == -1 ? "" : comboBuscarPor.SelectedIndex == 1 ? "ClientesProductosVendidos" : "ClientesProductosVendidosMesActual";
                if (comboFiltrar.SelectedIndex == -1 || tabla == "")
                {
                    return;
                }
                else
                {
                    tipofiltro = comboFiltrar.SelectedIndex == 0 ? AppResource.Products : AppResource.Line;
                }
                if (BindingContext is ProductosNoVendidosViewModel vm)
                {
                    Descripcion.Text = tipofiltro == AppResource.Line ? AppResource.Line : AppResource.Products;



                  await vm.FillListAsync(tabla, tipofiltro, isonline);
                }
               
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
          
        }

        private async void ListProductosNovendidos_ItemTapped(object sender, ItemTappedEventArgs e)
        {
               bool FromProducts = tipofiltro == AppResource.Products;
                if (e.Item == null || Arguments.Values.CurrentModule == Modules.PEDIDOS )
                {
                    return;
                }

                listProductosNovendidos.IsEnabled = false;
                Descripcion.Text = AppResource.Products;
                if (BindingContext is ProductosNoVendidosViewModel vm)
                {
                   await vm.ClientesNovendidos(tabla, e.Item as Productos, FromProducts,IsRutaVisita, CliidNovendidos);
                }
                listProductosNovendidos.IsEnabled = true ;
                listProductosNovendidos.SelectedItem = null;
                
            

        }
    }
}
