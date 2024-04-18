using MovilBusiness.DataAccess;
using MovilBusiness.model;
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
    public partial class DescuentoMancomunadoProductosModal : ContentPage, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;

        private List<Productos> productos;
        public List<Productos> Productos { get => productos; set { productos = value; RaiseOnPropertyChanged(); } }

        private string GrpCodigo;
        private bool FirstTime = true;

        private bool isbusy;
        public new bool IsBusy { get => isbusy; set { isbusy = value; RaiseOnPropertyChanged(); } }

        private readonly DS_GrupoProductos myGrp;

        public DescuentoMancomunadoProductosModal(string grpCodigo, bool aRegalar)
        {
            GrpCodigo = grpCodigo;
            myGrp = new DS_GrupoProductos();

            InitializeComponent();

            BindingContext = this;

            lblTitle.Text = aRegalar ? AppResource.ProductToGiveUpper : AppResource.ProductThatApply;

            lblGrpDescripcion.Text = AppResource.GroupLabelUpper + myGrp.GetGrpDescripcion(grpCodigo);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (FirstTime)
            {
                FirstTime = false;
                LoadProducts();
            }
        }

        private async void LoadProducts()
        {
            IsBusy = true;

            try
            {
                await Task.Run(() => 
                {
                    Productos = myGrp.GetProductosByGrpCodigo(GrpCodigo);
                });

            }catch(Exception e)
            {
                IsBusy = false;
                await DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
        }

        private async void Dismiss(object sender, EventArgs args)
        {
            await Navigation.PopModalAsync(false);
        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}