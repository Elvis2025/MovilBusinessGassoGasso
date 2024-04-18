using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model.Internal;
using MovilBusiness.model.Internal.structs;
using MovilBusiness.Resx;
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CalculadoraNegociacionModal : ContentPage
	{
        public ClientesCreditoData CliData { get; set; }
        public Totales DatosPedido { get; set; }

        private double MontoPedidoMaximo, MontoAbono, MontoAbonoCalculado;

        private int ConIdDelPedido = -1;
        private int ConIdContado;

        private double montoAdicionalPedido, montoAdicionalLimiteCredito, parPagoMinimo;

		public CalculadoraNegociacionModal (DS_Productos myProd, int conIdEntrega = -1,bool IsVenta = true)
		{
            ConIdDelPedido = conIdEntrega;
            ConIdContado = DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado();

            CliData = new DS_CuentasxCobrar().GetDatosCreditoClienteByFecha(Arguments.Values.CurrentClient.CliID);

            var adicionalPedido = DS_RepresentantesParametros.GetInstance().GetParVentasPorcientoAdicionalPedido();
            var adicionalLimiteCredito = DS_RepresentantesParametros.GetInstance().GetParVentasPorcientoAdicionalLimiteCredito();

            if (IsVenta)
            {
                DatosPedido = myProd.GetTempTotales((int)Modules.VENTAS, true, true);
            }
            else
            {
                DatosPedido = myProd.GetTempTotales((int)Modules.VENTAS, true, false);
            }

            if (adicionalPedido > 0 && DatosPedido.Total > 0)
            {
                montoAdicionalPedido = DatosPedido.Total * (adicionalPedido / 100.0);
            }

            if(adicionalLimiteCredito > 0 && CliData.LimiteCredito > 0)
            {
                montoAdicionalLimiteCredito = CliData.LimiteCredito * (adicionalLimiteCredito / 100.0);
                lblLimiteCredito.Text = AppResource.CreditLimitLabel + "(" + adicionalLimiteCredito + "% -- " + montoAdicionalLimiteCredito + " ) :";
            }

            parPagoMinimo = DS_RepresentantesParametros.GetInstance().GetParVentasPorcientoBalancePagoMinimo();
            
            //DatosPedido = myProd.GetTempTotales(7);
            
            

            BindingContext = this;

			InitializeComponent ();

            if(DatosPedido.Total > CliData.CreditoDisponible)
            {
                MontoAbono = DatosPedido.Total - CliData.CreditoDisponible;
                MontoPedidoMaximo = DatosPedido.Total;
            }
            else
            {
                MontoAbono = 0;
                MontoPedidoMaximo = CliData.CreditoDisponible;
            }

            if (parPagoMinimo > 0)
            {
                CargarAbonoParaPagoMinimo();
                ValidarPagoMinimo();
            }

            editAbono.Text = MontoAbono.ToString("F2");
            editMontoPedidoMaximo.Text = MontoPedidoMaximo.ToString("F2");

            if(ConIdDelPedido != -1 )//&& Arguments.Values.CurrentModule == Enums.Modules.VENTAS)
            {
                lblTipoPedido.IsVisible = true;
                lblTipoPedido.Text = ConIdDelPedido == ConIdContado ? AppResource.CashOrder : AppResource.CreditOrder;
            }
		}

        private void Dismiss(object sender, EventArgs args)
        {
            Navigation.PopModalAsync(false);
        }

        private void LimpiarAbono(object sender, EventArgs e)
        {
            editAbono.Text = "";
        }

        private void LimpiarPedidoMaximo(object sender, EventArgs e)
        {
            editMontoPedidoMaximo.Text = "";
        }

        private void CargarAbonoParaPagoMinimo()
        {
            if(parPagoMinimo > 0 )// && Arguments.Values.CurrentModule == Enums.Modules.VENTAS)
            {
                if(ConIdDelPedido == ConIdContado)
                {
                    MontoAbono = Math.Round((CliData.Balance * (parPagoMinimo / 100.0)), 2);
                    lblAbono.Text = AppResource.AbonoLabel + "(" + parPagoMinimo + "% -- " + MontoAbono + " ) :";
                }
                else
                {
                    MontoAbono = CliData.Balance;
                    MontoAbonoCalculado = Math.Round((CliData.Balance * (parPagoMinimo / 100.0)), 2);
                    lblAbono.Text = AppResource.AbonoLabel + "(" + parPagoMinimo + "% -- "+ MontoAbonoCalculado + " ) :";
                    
                }

                if(MontoAbono < 0)
                {
                    MontoAbono = 0;
                }
            }
        }

        private void ValidarPagoMinimo()
        {
            if (ConIdDelPedido != -1 )//&& Arguments.Values.CurrentModule == Enums.Modules.VENTAS)
            {
                if (parPagoMinimo > 0)
                {
                    MontoPedidoMaximo = 0;

                    if (MontoAbono >= Math.Round((CliData.Balance * (parPagoMinimo / 100.0)), 2) && ConIdDelPedido == ConIdContado && MontoAbono <= CliData.Balance)
                    {
                        MontoPedidoMaximo = DatosPedido.Total + montoAdicionalPedido;
                    }
                    else if (MontoAbono < (CliData.Balance * (parPagoMinimo / 100.0)) && MontoAbono <= CliData.Balance)
                    {
                        MontoPedidoMaximo = 0;
                    }
                    else if (ConIdDelPedido != ConIdContado && (Math.Round(MontoAbono, 2) == Math.Round(CliData.Balance, 2) || CliData.Balance == 0)) //si el pago es igual al balance y el pedido es a credito
                    {
                        if (DatosPedido.Total > 0)
                        {
                            if (DatosPedido.Total < (CliData.LimiteCredito + montoAdicionalLimiteCredito)) //si el monto del pedido es menor que al limite de credito
                            {
                                MontoPedidoMaximo = DatosPedido.Total + montoAdicionalPedido;
                            }
                            else
                            {
                                MontoPedidoMaximo = CliData.LimiteCredito + montoAdicionalLimiteCredito;
                            }
                        }
                    }
                    else if (ConIdDelPedido != ConIdContado && Math.Round(MontoAbono, 2) >= Math.Round((CliData.Balance * (parPagoMinimo / 100.0)), 2) &&
                       MontoAbono < CliData.Balance)
                    {
                        if (MontoAbono - (CliData.Balance * (parPagoMinimo / 100.0)) > DatosPedido.Total)
                        {
                            MontoPedidoMaximo = DatosPedido.Total + montoAdicionalPedido;
                        }
                        else
                        {
                            MontoPedidoMaximo = MontoAbono - (CliData.Balance * (parPagoMinimo / 100.0));
                        }
                    }
                    else
                    {
                        DisplayAlert(AppResource.Warning, AppResource.PaymentCannotBeGreaterThanBalance, AppResource.Aceptar);
                        MontoAbono = CliData.Balance;
                        if (ConIdDelPedido != ConIdContado && (Math.Round(MontoAbono, 2) == Math.Round(CliData.Balance, 2) || CliData.Balance == 0)) //si el pago es igual al balance y el pedido es a credito
                        {
                            if (DatosPedido.Total > 0)
                            {
                                if (DatosPedido.Total < (CliData.LimiteCredito + montoAdicionalLimiteCredito)) //si el monto del pedido es menor que al limite de credito
                                {
                                    MontoPedidoMaximo = DatosPedido.Total + montoAdicionalPedido;
                                }
                                else
                                {
                                    MontoPedidoMaximo = CliData.LimiteCredito + montoAdicionalLimiteCredito;
                                }
                            }
                        }
                        else if (MontoAbono >= Math.Round((CliData.Balance * (parPagoMinimo / 100.0)), 2) && ConIdDelPedido == ConIdContado && MontoAbono <= CliData.Balance)
                        {
                            MontoPedidoMaximo = DatosPedido.Total + montoAdicionalPedido;
                        }
                        else if (MontoAbono < (CliData.Balance * (parPagoMinimo / 100.0)) && MontoAbono <= CliData.Balance)
                        {
                            MontoPedidoMaximo = 0;
                        }


                    }
                }
            }
        }

        private void Calcular(object sender = null, EventArgs e = null)
        {
            if ((string.IsNullOrWhiteSpace(editAbono.Text) || (double.TryParse(editAbono.Text, out double abono) && abono == 0)) 
                && (string.IsNullOrWhiteSpace(editMontoPedidoMaximo.Text) || (ConIdDelPedido != -1 ) //&& Arguments.Values.CurrentModule == Enums.Modules.VENTAS) 
                || (double.TryParse(editMontoPedidoMaximo.Text, out double max) && max == 0)))
            {
                if (DatosPedido.Total > CliData.CreditoDisponible)
                {
                    MontoAbono = DatosPedido.Total - CliData.CreditoDisponible;
                    MontoPedidoMaximo = DatosPedido.Total;
                }
                else
                {
                    MontoAbono = 0;
                    MontoPedidoMaximo = DatosPedido.Total + (CliData.CreditoDisponible - DatosPedido.Total);
                }
                CargarAbonoParaPagoMinimo();
            }
            else
            {
                var calcularAbonoNecesario = false;

                double.TryParse(editAbono.Text, out double montoAb);
                double.TryParse(editMontoPedidoMaximo.Text, out double montoMax);

                if ((!string.IsNullOrWhiteSpace(editAbono.Text) && montoAb > 0 && (string.IsNullOrWhiteSpace(editMontoPedidoMaximo.Text) || montoMax == 0))
                    || (ConIdDelPedido != -1 ))//&& Arguments.Values.CurrentModule == Enums.Modules.VENTAS))
                {
                    calcularAbonoNecesario = true;
                }

                if (calcularAbonoNecesario)
                {
                    double.TryParse(editAbono.Text, out MontoAbono);
                    MontoPedidoMaximo = CliData.CreditoDisponible + MontoAbono;
                }
                else
                {
                    double.TryParse(editMontoPedidoMaximo.Text, out MontoPedidoMaximo);

                    MontoAbono = MontoPedidoMaximo - CliData.CreditoDisponible;

                    if (MontoAbono < 0)
                    {
                        MontoAbono = 0;
                    }

                    CargarAbonoParaPagoMinimo();
                }
 
            }

            ValidarPagoMinimo();

            editAbono.Text = MontoAbono.ToString("F2");
            editMontoPedidoMaximo.Text = MontoPedidoMaximo.ToString("F2");
        }
    }
}