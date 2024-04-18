using MovilBusiness.Configuration;
using MovilBusiness.Controls;
using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.ViewModel;
using MovilBusiness.Views.Components.Popup;
using MovilBusiness.Views.Components.Views;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Services;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProspectosTabPage : ScrollableTabPage
    {
        Label label;
        StackLayout stack;
        CheckBoxs check;
        Clientes cliente;
        public static string resultofsemanas;
        public static bool copytofirts;
        public static bool Finish = false;

        private bool rutorden;
        public ProspectosTabPage(Clientes editedProspecto = null)
        {
            if(Arguments.CurrentUser.IsAuditor && !App.Current.Properties.ContainsKey("CurrentRep"))
            {
                throw new Exception(AppResource.MustSelectSellerToAudit);
            }

            var vm = new ProspectosViewModel(this, editedProspecto) { IsGoingToSave = () => 
            {
                resultofsemanas = IsToSave();

                if (CurrentPage != Children[0])
                {
                    CurrentPage = Children[0];
                }
            },
                IsSaved = () => 
                {
                    foreach(var tab in Children)
                    {
                        tab.IsEnabled = false;
                    }
                },
                IsPositionOut = (result) =>
                {
                    if(result)
                    {
                        TapGestureRecognizer_Tapped();
                    }else
                    {
                        (BindingContext as ProspectosViewModel).CurrenClienteAVisitarDespuesDe = null;
                        (BindingContext as ProspectosViewModel).CliRutPosiciones = "";
                    }
                },
                IsOrdenOut = (result) =>
                {
                    if(result)
                    {
                        TapGestureRecognizer_Tapped_1();
                    }else
                    {
                        (BindingContext as ProspectosViewModel).CurrenClienteAOrdenarDespuesDe = null;
                        (BindingContext as ProspectosViewModel).CliOrdenRutas = "";
                    }
                },
            };
            if(editedProspecto != null)
            {
                vm.InputTransparent = true;
            }

            BindingContext = vm;

            InitializeComponent();

            //vm.ResetPickers();
            //(stack.Children as CheckBoxs).Checked += Checki_Checked;

            //vm.SetValueSpikersDefault();

            var ds = DS_RepresentantesParametros.GetInstance();

            if (ds.GetParProspectosOcultarFamiliar())
            {
                Children.RemoveAt(4);
            }

            if (ds.GetParProspectosOcultarPropietario())
            {
                Children.RemoveAt(3);
            }

            if (ds.GetParProspectosOcultarCrediticios())
            {
                Children.RemoveAt(2);
            }

            if (ds.GetParProspectosOcultarReferencias())
            {
                Children.RemoveAt(1);
            }
            
        }

       /* private void Checki_Checked(object sender, EventArgs e)
        {
            
        }*/

        private void List_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if(e.SelectedItem == null)
            {
                return;
            }

            list.SelectedItem = null;
        }

        private void OnItemDeleteListener(object sender, EventArgs args)
        {
            if(sender is Image view && BindingContext is ProspectosViewModel vm)
            {
                var rowguid = view.BindingContext.ToString();

                vm.EliminarReferencia(rowguid);
            }
        }

        protected override void OnAppearing()
        {
            if (Finish)
            {
                Finish = false;
                Navigation.PopAsync(false);
                return;
            }

            base.OnAppearing();

            if(BindingContext is ProspectosViewModel vm && vm.HasGps)
            {
                Functions.StartListeningForLocations();
                vm.SetValueSpikersDefault();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (BindingContext is ProspectosViewModel vm && vm.HasGps)
            {
                StopLocationUpdates();
            }
        }

        private async void StopLocationUpdates()
        {
            await Functions.StopListeningForLocations();
        }

        public string IsToSave()
        {
            string countcheck = "";
            foreach(var chil in checkboxContainer.Children.Where(c => c is StackLayout))
            {
                foreach(var check in (chil as StackLayout).Children.Where(s => s is CheckBoxs))
                {
                    countcheck += Convert.ToInt32((check as CheckBoxs).IsChecked).ToString();
                }
            }
            return !string.IsNullOrEmpty(countcheck) ? countcheck : "00000000000000000000000000000000000";
        }

        private void BorderlessPicker_SelectedIndexChanged(object sender, EventArgs e)
        {

            if(checkboxContainer.Children.Count <= 0)
            {
                label = new Label();
                stack = new StackLayout();
                check = new CheckBoxs();
                string[] semanas = "L,M,M,J,V,S,D".Split(',');

                for (int i = 0; i <= 3; i++)
                {
                    stack = new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Spacing = 0,
                        IsEnabled = false,
                    };

                    check = new CheckBoxs();

                    label = new Label
                    {
                        FontAttributes = FontAttributes.Bold,
                        VerticalOptions = LayoutOptions.Center,
                        Text = AppResource.Week + (i + 1),
                        IsEnabled = false,
                    };

                    checkboxContainer.Children.Add(label);

                    for (int j = 0; j <= 6; j++)
                    {
                        var lbl = new Label
                        {
                            FontAttributes = FontAttributes.Bold,
                            VerticalOptions = LayoutOptions.Center,
                            IsEnabled = false,
                            Text = semanas[j],
                        };

                        check = new CheckBoxs 
                        {
                            IsEnabled = false,
                        };

                        stack.Children.Add(lbl);
                        stack.Children.Add(check);
                    }
                    checkboxContainer.Children.Add(stack);
                }

                if(cliente != null)
                {
                    string semanastocount = cliente.CliRutSemana1 + cliente.CliRutSemana2 + cliente.CliRutSemana3 + cliente.CliRutSemana4;
                    char[] chars = semanastocount.ToCharArray();
                    int num = 0;
                    if(chars.Length > 0)
                    {
                        foreach (var chil in checkboxContainer.Children.Where(c => c is StackLayout))
                        {
                            foreach (var check in (chil as StackLayout).Children.Where(s => s is CheckBoxs))
                            {
                                (check as CheckBoxs).IsChecked = Convert.ToBoolean(int.Parse(chars[num].ToString()));
                                num++;
                            }
                        }
                    }
                }
            }
            else
            {
                foreach(var chil in checkboxContainer.Children)
                {    
                    if(chil is StackLayout)
                    {
                        foreach (var chilcheck in (chil as StackLayout).Children)
                        {
                            if (chilcheck is CheckBoxs check)
                            {
                                check.IsChecked = false;
                            }

                            chilcheck.IsEnabled = false;
                        }
                    }
                    chil.IsEnabled = false;
                }
            }

            var item = pickercheck.SelectedItem as UsosMultiples;
            int numerofitem = int.Parse(item.CodigoUso);
            copytofirts = numerofitem == 2;

            switch (numerofitem)
            {
                case 1:
                    numerofitem = 2;
                    break;
                case 2:
                    numerofitem = 4;
                    break;
                case 3:
                    numerofitem = 8;
                    break;
            }

            for (int i = 0; i < numerofitem; i++)
            {
                checkboxContainer.Children[i].IsEnabled = true;

                if(checkboxContainer.Children[i] is StackLayout stack)
                {
                    foreach (var chilcheck in stack.Children)
                    {
                        chilcheck.IsEnabled = true;
                    }
                }

            }
        }

        private void TapGestureRecognizer_Tapped(object sender = null, EventArgs e = null)
        {
            rutorden = false;

            Navigation.PushPopupAsync(new PopupSearchFrame() {ItemSelected = (s) => 
            {
                SelectedIndexChanged(s);
                (BindingContext as ProspectosViewModel).CurrenClienteAVisitarDespuesDe = s;
                (BindingContext as ProspectosViewModel).CliRutPosiciones = (BindingContext as ProspectosViewModel).IsToggledVisitar ? 
                                       (s.CliRutPosicion + 1).ToString() : (s.CliRutPosicion - 1).ToString();
            }
            });
        }

        private async void SelectedIndexChanged(Clientes clientes, bool iscancel = false)
        {
            rutorden = false;

            cliente = new DS_Clientes().GetAllClientesRutPosicion(lblantes.IsEnabled ? clientes.CliRutPosicion - 1 : clientes.CliRutPosicion + 1);
            if (!iscancel && cliente != null)
            {
                bool result = await DisplayAlert(AppResource.Warning, AppResource.PositionAlreadyUsedByAnotherClient + " " + cliente.CliRutPosicion+" - "+cliente.CliNombre, AppResource.Aceptar, AppResource.Cancel);
                if (result)
                {
                    TapGestureRecognizer_Tapped();
                    return;
                }else
                {
                    (BindingContext as ProspectosViewModel).CurrenClienteAVisitarDespuesDe = null;
                    (BindingContext as ProspectosViewModel).CliRutPosiciones = "";
                }
            }
         
            if (checkboxContainer.Children.Count > 0)
            {                
                string semanas = cliente != null && (BindingContext as ProspectosViewModel).CurrenClienteAVisitarDespuesDe != null? cliente.CliRutSemana1 + cliente.CliRutSemana2 + cliente.CliRutSemana3 + cliente.CliRutSemana4 : "";
                char[] chars = semanas.ToCharArray();
                if(chars.Length > 0)
                {
                    int num = 0;
                    foreach (var chil in checkboxContainer.Children.Where(c => c is StackLayout))
                    {
                        foreach (var check in (chil as StackLayout).Children.Where(s => s is CheckBoxs))
                        {
                            (check as CheckBoxs).IsChecked = Convert.ToBoolean(int.Parse(chars[num].ToString()));
                            num++;
                        }
                    }
                }else
                {
                    foreach (var chil in checkboxContainer.Children.Where(c => c is StackLayout))
                    {
                        foreach (var check in (chil as StackLayout).Children.Where(s => s is CheckBoxs))
                        {
                            (check as CheckBoxs).IsChecked = false;                            
                        }
                    }
                }
                    
            }
        }

        protected override bool OnBackButtonPressed()
        {
            var popupnav = PopupNavigation.Instance;

            if (popupnav.PopupStack.Count > 0)
            {
                popupnav.PopAllAsync(true);

                if (!rutorden)
                {
                    (BindingContext as ProspectosViewModel).CurrenClienteAVisitarDespuesDe = null;
                    (BindingContext as ProspectosViewModel).CliRutPosiciones = "";
                } else
                {
                    (BindingContext as ProspectosViewModel).CurrenClienteAOrdenarDespuesDe = null;
                    (BindingContext as ProspectosViewModel).CliOrdenRutas = "";
                }    
                
                return true;
            }            
            return base.OnBackButtonPressed();
        }

        private void BorderlessEntry_Unfocused(object sender, FocusEventArgs e)
        {
            if(string.IsNullOrEmpty(bebrnccedula.Text))
            {
                return;
            }

            bebrnccedula.Text = bebrnccedula.Text.Replace("-","");

            bebrnccedula.Text = bebrnccedula.Text.Length == 9 ?  bebrnccedula.Text.Substring(0, 1) 
                + "-" + bebrnccedula.Text.Substring(1, 2) + "-" + bebrnccedula.Text.Substring(3, 5) + "-" +
                bebrnccedula.Text.Substring(8, 1) : bebrnccedula.Text.Length == 11? bebrnccedula.Text.Substring(0, 3) + "-" +
                bebrnccedula.Text.Substring(3, 7) + "-" + bebrnccedula.Text.Substring(10, 1) : bebrnccedula.Text;
        }

        private void bebrnccedula_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(bebrnccedula.Text.Replace("-", "").Length <= 11)
            {
                return;
            }

            bebrnccedula.Text = bebrnccedula.Text.Substring(0, bebrnccedula.Text.Length - 1);
        }

        private void TapGestureRecognizer_Tapped_1(object sender = null, EventArgs e = null)
        {
            rutorden = true;

            Navigation.PushPopupAsync(new PopupSearchFrame()
            {
                ItemSelected = (s) =>
                {
                    s.CliNombre = s.CliOrdenRuta + "/" + s.CliNombre;
                    (BindingContext as ProspectosViewModel).CurrenClienteAOrdenarDespuesDe = s;
                    (BindingContext as ProspectosViewModel).CliOrdenRutas = s.CliOrdenRuta.ToString();
                }
            });
        }
    }
}