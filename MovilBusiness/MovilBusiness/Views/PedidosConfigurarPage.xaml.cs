using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Resx;
using MovilBusiness.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PedidosConfigurarPage : ContentPage
	{
        private string CamposObligatorios;
        private bool HideFechaEntrega = false;

        public PedidosConfigurarPage ()
		{
			InitializeComponent ();
            
            if(DS_RepresentantesParametros.GetInstance().GetParCambiosUsarMotivos() == 1)
            {
                lblMotivo.Text = AppResource.ReasonForChange;
            }
		}

        protected async override void OnBindingContextChanged()
        {
            try
            {
                base.OnBindingContextChanged();

                if (BindingContext is PedidosViewModel vm && (Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.VENTAS || Arguments.Values.CurrentModule == Modules.COTIZACIONES))
                {
                    vm.SetCondicionPagoCliente();
                    vm.SetCentroDisribucion();
                }

                CargarCamposAdicionales();

                if (HideFechaEntrega)
                {
                    lblFechaEntrega.IsVisible = false;
                    pickerFechaEntrega.IsVisible = false;
                }

            }
            catch(Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }
        }

        private async Task CargarCamposAdicionales()
        {
            var ds = new DS_UsosMultiples();

            var parDevolucionesCamposAdicionales = DS_RepresentantesParametros.GetInstance().GetParDevolucionesCamposAdicionales();

            var camposCotizaciones = ds.GetCotizacionesCamposAdicionales();

            if (!DS_RepresentantesParametros.GetInstance().GetParPedidosCamposAdicionales() && !parDevolucionesCamposAdicionales && camposCotizaciones == null)
            {
                return;
            }           

            var fieldList = camposCotizaciones != null ? camposCotizaciones : parDevolucionesCamposAdicionales ? ds.GetDevolucionesCamposAdicionales() : ds.GetPedidosCamposAdicionales();

            foreach (var field in fieldList)
            {
                var codigoUso = field.CodigoUso.Split('|');

                var context = new PedCamposAdicionalesData
                {
                    Id = codigoUso[0],
                    Tipo = codigoUso[1]
                };

                if (codigoUso.Length > 2)
                {
                    context.CodigoGrupo = codigoUso[2];
                }

                var title = new Label() { Text = field.Descripcion, HorizontalOptions = LayoutOptions.FillAndExpand, FontAttributes = FontAttributes.Bold };
                camposAdicionales.Children.Add(title);

                switch (context.Tipo.ToLower())
                {
                    case "text":
                        var edit = new Entry() { HorizontalOptions = LayoutOptions.FillAndExpand };
                        edit.BindingContext = context;
                        camposAdicionales.Children.Add(edit);
                        break;
                    case "combo":
                        var picker = new Picker() { HorizontalOptions = LayoutOptions.FillAndExpand };
                        picker.ItemsSource = ds.GetUsoByCodigoGrupoByEndLike(context.CodigoGrupo);
                        picker.Title = AppResource.Select;
                        picker.BindingContext = context;
                        camposAdicionales.Children.Add(picker);
                        break;
                    case "boolean":
                        var check = new Switch() { HorizontalOptions = LayoutOptions.Start };
                        check.BindingContext = context;
                        camposAdicionales.Children.Add(check);

                        if (field.Descripcion.ToLower().Contains("pronto pago") || field.Descripcion.ToLower().Contains("dpp"))
                        {
                            check.Toggled += DppToggle;
                            if (DS_RepresentantesParametros.GetInstance().BloquearCondicionPago() == 3 && Arguments.Values.CurrentModule == Modules.PEDIDOS)
                            {
                                if (BindingContext is PedidosViewModel cd)
                                {
                                    check.IsEnabled = cd.EnableProntoPago;
                                    if(DS_RepresentantesParametros.GetInstance().GetParMoatrarAlertaConfiguracion() && cd.EnableProntoPago)
                                    {
                                        var ISValid = await DisplayAlert("Aviso", "Este Pedido aplica para Pronto Pago?", "Si", "No");
                                        if (ISValid)
                                            check.IsToggled = true;
                                    }                                  
                                }
                            }else if(DS_RepresentantesParametros.GetInstance().GetParMoatrarAlertaConfiguracion())
                            {
                                var ISValid = await DisplayAlert("Aviso", "Este Pedido aplica para Pronto Pago?", "Si", "No");
                                if (ISValid)
                                    check.IsToggled = true;
                            }


                        }else if(field.Descripcion.ToLower().Contains("multi entrega") && BindingContext is PedidosViewModel vm)
                        {
                            HideFechaEntrega = true;
                            check.Toggled += (s, a) => 
                            {
                                vm.UseMultiEntrega = a.Value;
                            };
                        }
                        break;
                    case "date":
                        var date = new DatePicker() { HorizontalOptions = LayoutOptions.FillAndExpand };
                        date.Date = DateTime.Now;
                        date.Format = "dd/MM/yyyy";
                        date.BindingContext = context;
                        camposAdicionales.Children.Add(date);
                        break;
                }
            }
        }

        private void DppToggle(object sender, ToggledEventArgs e)
        {
            if(BindingContext is PedidosViewModel vm)
            {
                vm.IsDpp = e.Value;
            }
        }

        public string GetCamposAdicionales()
        {
            var list = new List<PedCamposAdicionalesData>();
           
            CamposObligatorios = DS_RepresentantesParametros.GetInstance().GetParCamposAdicionalesCamposObligatorios();
            bool DestinoObligatorio = CamposObligatorios.Contains("Destino");

            foreach (var raw in camposAdicionales.Children)
            {
                if (raw is Label)
                {
                    continue;
                }

                var context = raw.BindingContext as PedCamposAdicionalesData;

                var item = new PedCamposAdicionalesData()
                {
                    Id = context.Id
                };

                if (raw is Entry e)
                {
                    item.Tipo = "Text";
                    item.Valor = e.Text;
                }
                else if (raw is Picker p)
                {
                    var selected = p.SelectedItem as UsosMultiples;

                    item.Tipo = "Combo";
                    item.CodigoGrupo = context.CodigoGrupo;

                    if((DestinoObligatorio) && selected == null)
                    {
                        item.Valor = "ERROR";
                    } 
                    else if (selected == null) {
                        item.Valor = "";
                    }
                    else
                    {
                        item.Valor = selected.CodigoUso;
                    }
                   
                }
                else if (raw is Switch s)
                {
                    item.Tipo = "Boolean";
                    item.Valor = s.IsToggled ? "true" : "false";
                }
                else if (raw is DatePicker d)
                {
                    item.Tipo = "Date";
                    item.Valor = d.Date.ToString("dd/MM/yyyy");
                }

                list.Add(item);

            }

            return JsonConvert.SerializeObject(list);
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            if (BindingContext is PedidosViewModel cd && Arguments.Values.CurrentModule == Modules.PEDIDOS)
            {
                cd.OnButtonClicked(sender,e);
            }
        }
    }
}