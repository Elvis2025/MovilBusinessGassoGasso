using MovilBusiness.model.Internal;
using MovilBusiness.model.Internal.Structs.Args;
using MovilBusiness.viewmodel;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RecibosDocumentosTabPage : ContentPage
    {

        public RecibosDocumentosTabPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is RecibosViewModel vm)
            {
                if (vm.Documentos == null || vm.ReloadDocuments)
                {
                    vm.IsFirstChkDiferidoGeneral = false;
                    vm.CargarDocumentos(!vm.ReloadDocuments);
                }
            }

        }

        private void OnListItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                return;
            }

            if (BindingContext is RecibosViewModel vm)
            {
                vm.DocumentSelected(e.SelectedItem as RecibosDocumentosTemp);
            }

            listaFacturas.SelectedItem = null;
        }

        private void DismissDialog(object sender, EventArgs args)
        {
            ((RecibosViewModel)BindingContext).DismissDialogs();
        }

        private void AcceptNC(object sender, AplicarNCArgs args)
        {
            ((RecibosViewModel)BindingContext).AplicarNC(args);
        }

        private void AcceptDocument(object sender, RecibosDocumentosDetalleArgs args)
        {
            ((RecibosViewModel)BindingContext).AceptarDetalleFactura(args);
        }

        protected override bool OnBackButtonPressed()
        {

            if (((RecibosViewModel)BindingContext).ShowDetalleRecibo)
            {
                DismissDialog(null, null);
                return true;
            }

            return base.OnBackButtonPressed();
        }

        private void Dismiss(object sender, EventArgs args)
        {
            ((RecibosViewModel)BindingContext).DetailsDocumentVisible = false;
        }

        private void Control_OnValueChanged(object sender, int e)
        {
            if(e == 0)
            {
                ((RecibosViewModel)BindingContext).DetailsDescuentosVisible = true;
                ((RecibosViewModel)BindingContext).DetailsAplicacionVisible = false;
            }
            else
            {
                ((RecibosViewModel)BindingContext).DetailsAplicacionVisible = true;
                ((RecibosViewModel)BindingContext).DetailsDescuentosVisible = false;
            }
        }
    }
}