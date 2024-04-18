using MovilBusiness.DataAccess;
using MovilBusiness.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProductosCombosModal : ContentPage, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;

        public bool IsWorking { get; set; }

        private List<ProductosCombos> productos;
        public List<ProductosCombos> Productos { get => productos; private set { productos = value; RaiseOnPropertyChanged(); } }

        private DS_ProductosCombos myPrc;

        public ProductosCombosModal()
        {
            myPrc = new DS_ProductosCombos();

            BindingContext = this;

            InitializeComponent();
        }

        public void LoadCombo(int proId)
        {
            if (IsWorking)
            {
                return;
            }

            IsWorking = true;

            Productos = myPrc.GetProductosCombo(proId);

            IsWorking = false;
        }

        private async void Dismiss(object sender, EventArgs args)
        {
            if (IsWorking)
            {
                return;
            }

            IsWorking = true;
            await Navigation.PopModalAsync(true);
            IsWorking = false;
        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}