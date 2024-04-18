using MovilBusiness.viewmodel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MovilBusiness.DataAccess;
using System;
using MovilBusiness.Configuration;
using MovilBusiness.model;
using MovilBusiness.Resx;

namespace MovilBusiness.views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DepositosPage : ContentPage
    {
        public static bool Finish = false;
        public DepositosPage(string sector = null, bool isSociedad = false, string monCodigo = null, int depsecuencia = -1, bool isconfirmado = false, bool isFromConteo = false)
        {
            try
            {

                Device.BeginInvokeOnMainThread(() =>
                {
                    var vm = new DepositosViewModel(this, sector, isSociedad, monCodigo, depsecuencia: depsecuencia, isconfirmado: isconfirmado, isFromConteo: isFromConteo);
                    BindingContext = vm;
                    InitializeComponent();
                    //comboTipo.SelectedItem = 1;
                    if (depsecuencia > 0)
                    {
                        ToolbarItems.RemoveAt(0);
                        comboTipo.IsEnabled = false;
                        numerodep.IsEnabled = false;
                        includeOrdenPagoButton.IsEnabled = false;
                    }else
                    {
                        dialogImpresion.OnCancelar = OnCancelarImpresion;
                        dialogImpresion.OnAceptar = OnAceptarImpresion;
                        dialogImpresion.SetCopiasImpresionByTitId(9);
                        if (DS_RepresentantesParametros.GetInstance().GetParDepositosOrdenPago())
                        {
                            includeOrdenPagoButton.IsVisible = true;
                            includeOrdenPagoLabel.IsVisible = true;

                            if (comboTipo.SelectedItem != null && comboTipo.SelectedItem.ToString() == "Banco")
                            {
                                includeOrdenPagoButton.IsEnabled = false;
                            }
                        }

                        if (DS_RepresentantesParametros.GetInstance().GetParDepositosCapturarFoto() && depsecuencia == -1)
                        {
                            ToolbarItems.Insert(0, new ToolbarItem() { Text = AppResource.Photo, Order = ToolbarItemOrder.Primary, IconImageSource = "ic_photo_camera_white_24dp", Command = vm.GoFotoCommand });
                        }
                    }

                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        private void OnCancelarImpresion()
        {
            ((DepositosViewModel)BindingContext).CancelarImpresion();
        }

        private void OnAceptarImpresion(int copias)
        {
            ((DepositosViewModel)BindingContext).AceptarImpresion(copias);
        }

        private void IncludeOrdenPagoButton_Toggled(object sender, ToggledEventArgs e)
        {
            if (!(BindingContext is DepositosViewModel DVM))
            {
                return;
            }

            if (includeOrdenPagoButton.IsToggled)
            {
                DVM.IncludeOrdenPago = true;
                DisplayAlert(AppResource.Warning, AppResource.PaymentOrdersWillBeIncludeInDeposit, AppResource.Aceptar);
                MontoOrdenPagoLabel.IsVisible = true;
                MontoOrdenPago.IsVisible = true;
                var parSociedad = DS_RepresentantesParametros.GetInstance().GetParDepositosPorSociedad();

                if (DS_RepresentantesParametros.GetInstance().GetParDepositoSectores() || parSociedad)
                {
                    DVM.CalcularDeposito(true);
                }
                else
                {
                    DVM.CalcularDeposito(true);
                }
            }
            else
            {
                DVM.IncludeOrdenPago = true;
                MontoOrdenPago.IsVisible = false;
                MontoOrdenPagoLabel.IsVisible = false;
                var parSociedad = DS_RepresentantesParametros.GetInstance().GetParDepositosPorSociedad();
                if (DS_RepresentantesParametros.GetInstance().GetParDepositoSectores() || parSociedad)
                {
                    DVM.CalcularDeposito(false);
                }
                else
                {
                    DVM.CalcularDeposito(false);
                }
            }
        }

        private void comboTipo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboTipo.SelectedItem.ToString() == "Banco")
            {
                includeOrdenPagoButton.IsEnabled = false;
               if (!String.IsNullOrEmpty(DS_RepresentantesParametros.GetInstance().GetParDepositosFormasPago()))
                {
                    try
                    {
                        if (!(BindingContext is DepositosViewModel DVM))
                        {
                            return;
                        }
                        DVM.CalcularDeposito(tiposrecibos: DS_RepresentantesParametros.GetInstance().GetParDepositosFormasPago());

                    }
                    catch (Exception a)
                    {
                        Console.WriteLine(a.Message);
                    }
                }
            }
            else
            {
                includeOrdenPagoButton.IsEnabled = true;
            }

        }

        protected override void OnAppearing()
        {          
            if (Finish)
            {
                Finish = false;
                Navigation.PopAsync(false);
            }
            else
            {
                base.OnAppearing();
            }
        }
    }

    }