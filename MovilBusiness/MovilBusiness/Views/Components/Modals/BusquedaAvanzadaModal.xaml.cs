using MovilBusiness.DataAccess;
using MovilBusiness.Model.Internal.Structs.Args;
using MovilBusiness.Resx;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class BusquedaAvanzadaModal : ContentPage, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;
        public Action<BusquedaAvanzadaProductosArgs> OnAceptarFiltros { get; set; }

        private bool cat3IdEsVisible;   
        public bool Cat3IdEsVisible { get => cat3IdEsVisible; set { cat3IdEsVisible = value; RaiseOnPropertyChanged(); } }

        private bool proDescripcion3EsVisible;
        public bool ProDescripcion3EsVisible { get => proDescripcion3EsVisible; set { proDescripcion3EsVisible = value; RaiseOnPropertyChanged(); } }

        private bool proDatos2EsVisible;
        public bool ProDatos2EsVisible { get => proDatos2EsVisible; set { proDatos2EsVisible = value; RaiseOnPropertyChanged(); } }

        private bool proReferenciaEsVisible;
        public bool ProReferenciaEsVisible { get => proReferenciaEsVisible; set { proReferenciaEsVisible = value; RaiseOnPropertyChanged(); } }


        private bool proColorEsVisible;
        public bool ProColorEsVisible { get => proColorEsVisible; set { proColorEsVisible = value; RaiseOnPropertyChanged(); } }

        private bool proPaisOrigenEsVisible;
        public bool ProPaisOrigenEsVisible { get => proPaisOrigenEsVisible; set { proPaisOrigenEsVisible = value; RaiseOnPropertyChanged(); } }
        private bool proAnioEsVisible;
        public bool ProAnioEsVisible { get => proAnioEsVisible; set { proAnioEsVisible = value; RaiseOnPropertyChanged(); } }
        private bool proMedidaEsVisible;
        public bool ProMedidaEsVisible { get => proMedidaEsVisible; set { proMedidaEsVisible = value; RaiseOnPropertyChanged(); } }

        private bool proDescripcion2EsVisible;
        public bool ProDescripcion2EsVisible { get => proDescripcion2EsVisible; set { proDescripcion2EsVisible = value; RaiseOnPropertyChanged(); } }


        private string proDescripcion1;
        public string ProDescripcion1 { get => proDescripcion1; set { proDescripcion1 = value; RaiseOnPropertyChanged(); } }

        private string proDescripcion2;
        public string ProDescripcion2 { get => proDescripcion2; set { proDescripcion2 = value; RaiseOnPropertyChanged(); } }

        private string proCodigo;
        public string ProCodigo { get => proCodigo; set { proCodigo = value; RaiseOnPropertyChanged(); } }

        private string proDescripcion;
        public string ProDescripcion { get => proDescripcion; set { proDescripcion = value; RaiseOnPropertyChanged(); } }

        private string proDatos1;
        public string ProDatos1 { get => proDatos1; set { proDatos1 = value; RaiseOnPropertyChanged(); } }


        private string proReferencia;
        public string ProReferencia { get => proReferencia; set { proReferencia = value; RaiseOnPropertyChanged(); } }

        private string proDescripcion3;
        public string ProDescripcion3 { get => proDescripcion3; set { proDescripcion3 = value; RaiseOnPropertyChanged(); } }

        private string proDatos2;
        public string ProDatos2 { get => proDatos2; set { proDatos2 = value; RaiseOnPropertyChanged(); } }

        private string cat3Id;
        public string Cat3Id { get => cat3Id; set { cat3Id = value; RaiseOnPropertyChanged(); } }

        private string proColor;
        public string ProColor { get => proColor; set {   proColor = value; RaiseOnPropertyChanged(); } }

        private string proPaisOrigen;
        public string ProPaisOrigen { get => proPaisOrigen; set {  proPaisOrigen = value; RaiseOnPropertyChanged(); } }
        private string proAnio;
        public string ProAnio { get => proAnio; set {  proAnio = value; RaiseOnPropertyChanged(); } }
        private string proMedida;
        public string ProMedida { get => proMedida; set { proMedida  = value; RaiseOnPropertyChanged(); } }
        protected DS_RepresentantesParametros myParametro;
        public BusquedaAvanzadaModal ()
		{
			InitializeComponent ();
            BindingContext = this;

            myParametro = DS_RepresentantesParametros.GetInstance();
            Cat3IdEsVisible = true;
            

            bool ocultaProReferencia = myParametro.GetParBusquedaAvanzadaOcultarProReferencia();
            bool ocultaCat3ID = myParametro.GetParBusquedaAvanzadaOcultarCat3Id();
            bool mostrarProDatos2 = myParametro.GetParBusquedaAvanzadaMostrarProDatos2();
            bool mostrarProDescripcion3 = myParametro.GetParBusquedaAvanzadaMostrarProDescripcion3();

            ProReferenciaEsVisible = !ocultaProReferencia;
            Cat3IdEsVisible = !ocultaCat3ID;
            ProDatos2EsVisible = mostrarProDatos2;
            ProDescripcion3EsVisible = mostrarProDescripcion3;

            ProColorEsVisible = myParametro.GetParBusquedaAvanzadaMostrarProColor();
            ProPaisOrigenEsVisible = myParametro.GetParBusquedaAvanzadaMostrarProPaisOrigen();
            ProAnioEsVisible = myParametro.GetParBusquedaAvanzadaMostrarProAnioFabricacion();
            ProMedidaEsVisible = myParametro.GetParBusquedaAvanzadaMostrarProMedida();

            ProDescripcion2EsVisible = !myParametro.GetParBusquedaAvanzadaOcultarProDescripcion2();

            //
            //DeseEsVisible = false;

            ProDescripcion1 = AppResource.Brand;
            ProDescripcion = AppResource.Item;
            ProDescripcion2 = AppResource.Model;
            ProCodigo = AppResource.Code;
            ProDatos1 = AppResource.Text;
            ProReferencia = AppResource.ProductBrand;
            Cat3Id = AppResource.Since;

            ProDescripcion3 = "DESC 3";
            ProDatos2 = AppResource.Data2Upper;

            ProColor = "Color";
            ProAnio = AppResource.Year;
            ProMedida = AppResource.Measure;
            ProPaisOrigen = AppResource.Country;

            string parProDescripcion = myParametro.GetParBusquedaAvanzadaLabelProDescripcion();
            string parProDescripcion1 = myParametro.GetParBusquedaAvanzadaLabelProDescripcion1();
            string parProDescripcion2 = myParametro.GetParBusquedaAvanzadaLabelProDescripcion2();
            string parProDescripcion3 = myParametro.GetParBusquedaAvanzadaLabelProDescripcion3();
            string parProCodigo = myParametro.GetParBusquedaAvanzadaLabelProCodigo();
            string parProDatos1 = myParametro.GetParBusquedaAvanzadaLabelProDatos1();
            string parProDatos2 = myParametro.GetParBusquedaAvanzadaLabelProDatos2();

            string parProReferencia = myParametro.GetParBusquedaAvanzadaLabelProReferencia();
            string parProCatId3 = myParametro.GetParBusquedaAvanzadaLabelProCat3Id();

            string parProColor = myParametro.GetParBusquedaAvanzadaLabelProColor();
            string parProPaisOrigen = myParametro.GetParBusquedaAvanzadaLabelProPaisOrigen();
            string parProMedida = myParametro.GetParBusquedaAvanzadaLabelProMedida();
            string parProAnio = myParametro.GetParBusquedaAvanzadaLabelProAnio();
           

            if (!string.IsNullOrWhiteSpace(parProDescripcion))
            {
                ProDescripcion = parProDescripcion;
            }
            if (!string.IsNullOrWhiteSpace(parProDescripcion1))
            {
                ProDescripcion1 = parProDescripcion1;
            }
            if (!string.IsNullOrWhiteSpace(parProDescripcion2))
            {
                ProDescripcion2 = parProDescripcion2;
            }
            if (!string.IsNullOrWhiteSpace(parProDescripcion3))
            {
                ProDescripcion3 = parProDescripcion3;
            }
            if (!string.IsNullOrWhiteSpace(parProCodigo))
            {
                ProCodigo = parProCodigo;
            }

            if (!string.IsNullOrWhiteSpace(parProDatos1))
            {
                ProDatos1 = parProDatos1;
            }
            if (!string.IsNullOrWhiteSpace(parProDatos2))
            {
                ProDatos2 = parProDatos2;
            }
            if (!string.IsNullOrWhiteSpace(parProReferencia))
            {
                ProReferencia = parProReferencia;
            }
            if (!string.IsNullOrWhiteSpace(parProCatId3))
            {
                Cat3Id = parProCatId3;
            }


            if (!string.IsNullOrWhiteSpace(parProColor))
            {
                ProColor = parProCodigo;
            }
            if (!string.IsNullOrWhiteSpace(parProPaisOrigen))
            {
                proPaisOrigen = parProPaisOrigen;
            }
            if (!string.IsNullOrWhiteSpace(parProAnio))
            {
                ProAnio = parProAnio;
            }
            if (!string.IsNullOrWhiteSpace(parProMedida))
            {
                ProMedida = parProMedida;
            }
        }
        //PONER TIPO DE PEDIDOS CREDITO POR DEFECTO.
      

        private void AceptarFiltros(object sender, EventArgs e)
        {
            var args = new BusquedaAvanzadaProductosArgs();

            int.TryParse(Cat3Id, out int cat3Id);

            /*args.Cat1ID = catId;
            args.Cat2ID = catId;*/
            /*
            args.Cat3ID = cat3Id;
            args.ProDescripcion1 = editMarca.Text;
            args.ProDescripcion2 = editModelo.Text;
            args.ProCodigo = editCodigo.Text;
            args.ProDescripcion = editArticulo.Text;

            args.ProDatos1 = editTexto.Text;
            args.ProReferencia = editMarcaProducto.Text;
            */
            args.Cat3ID = cat3Id;
            args.ProDescripcion1 = editProDescripcion1.Text;
            args.ProDescripcion2 = editProDescripcion2.Text;
            args.ProCodigo = editProCodigo.Text;
            args.ProDescripcion = editProDescripcion.Text;

            args.ProDatos1 = editProDatos1.Text;
            args.ProReferencia = editProReferencia.Text;

            args.ProDatos2 = editProDatos2.Text;
            args.ProDescripcion3 = editProDescripcion3.Text;

            args.ProColor = editProColor.Text;
            args.ProMedida = editProMedida.Text;
            args.ProAnio = editProAnio.Text;
            args.ProPaisOrigen = editProPaisOrigen.Text;

            //// ProDatos2 = productoActual[12].ToStr(), //Referencia de Fabrica del Material
            //             ProDescripcion2 = productoActual[13].ToStr(),//REFERENCIA MATERIAL 
            //             ProDatos1 = productoActual[20].ToStr(), //productoActual[14].ToStr(),   //Aplicación/uso 
            //             ProDescripcion1 = productoActual[18].ToStr(), //productoActual[20].ToStr(), //Marca
            //             ProDescripcion3 = productoActual[21].ToStr(), //Modelo
            OnAceptarFiltros?.Invoke(args);
            //cat3Id = 0;
            //editProDescripcion1.Text = "";
            //editProDescripcion2.Text = "";
            //editProCodigo.Text = "";
            //editProDescripcion.Text = "";
            //editProDatos1.Text = "";
            //editProReferencia.Text = "";
            //editProDescripcion3.Text = "";
            //editProDatos2.Text = "";
            ResetearPropiedades();
            Navigation.PopModalAsync(true);
        }

        private void ResetearPropiedades()
        {
            cat3Id = "";
            editProDescripcion1.Text = "";
            editProDescripcion2.Text = "";
            editProCodigo.Text = "";
            editProDescripcion.Text = "";
            editProDatos1.Text = "";
            editProReferencia.Text = "";
            editProDatos2.Text = "";
            editProDescripcion3.Text = "";
            editProColor.Text = "";
            editProMedida.Text = "";
            editProAnio.Text = "";
            editProPaisOrigen.Text = "";
        }

        private void Dismiss(object sender, EventArgs args)
        {
            Navigation.PopModalAsync(true);
        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}