using MovilBusiness.DataAccess;
using MovilBusiness.Resx;
using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CanastosModal : ContentPage
    {

        public CanastosModal()
        {
            BindingContext = this;

            InitializeComponent();

        }

        private void Dismiss(object sender, EventArgs args)
        {
            Navigation.PopModalAsync(false);
        }

        private int savedTraSecuencia = -1;
        private async void Save(object sender, EventArgs args)
        {
            try
            {

                if(!int.TryParse(editCantidad.Text, out int cantidad))
                {
                    throw new Exception(AppResource.SpecifyNumberBaskets);
                }

                savedTraSecuencia = -1;

                var tipo = ControlTipoTransaccion.SelectedSegment == 0 ? Enums.TipoCapturaCanastos.ENTREGARCANASTOS : Enums.TipoCapturaCanastos.RECIBIRCANASTOS;

                if (DS_RepresentantesParametros.GetInstance().GetParCanastosNoDetalle())
                {
                    savedTraSecuencia = new DS_TransaccionesCanastos().SaveCanastos(tipo, cantidad, new List<string>());
                    var imprimir = await DisplayAlert(AppResource.Success, (ControlTipoTransaccion.SelectedSegment == 0 ? AppResource.DeliverBasketsSavedCorrectly: AppResource.ReceiptBasketsSavedCorrectly), AppResource.Print, AppResource.Continue);

                    if (imprimir)
                    {
                        GoPrint(savedTraSecuencia);
                    }
                    else
                    {
                        Dismiss(null, null);
                    }
                   
                }
                else
                {
                    await Navigation.PushModalAsync(new AgregarDetalleCanastosModal(tipo, cantidad, ()=> { Navigation.PopModalAsync(false); }));
                }

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }
        }

        private async void GoPrint(int traSecuencia)
        {
            try
            {
                var copias = await DisplayActionSheet(AppResource.ChoosePrinterCopies, AppResource.Cancel, null, new string[] { "1", "2", "3", "4", "5" });

                if (int.TryParse(copias, out int elegidas) && elegidas > 0)
                {
                    await new DS_TransaccionesCanastos().AceptarImpresion(traSecuencia, elegidas);
                }
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.ErrorPrinting, e.Message, AppResource.Aceptar);
            }

            await Navigation.PopModalAsync(false);

        }
    }
}