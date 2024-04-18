using MovilBusiness.Controls;
using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using MovilBusiness.ViewModel;
using MovilBusiness.Views.Components.Popup;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Services;
using System;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SacPage : TabbedPage
    {

        Label label;
        StackLayout stack;
        CheckBoxs check;
        Clientes cliente;
        public static string resultofsemanas;
        public static bool copytofirts;


        private bool rutorden;
        public static bool Finish = false;
        public SacPage(Clientes Cliente)
        {

            BindingContext = new SacViewModel(this, Cliente)
            {
                IsGoingToSave = () =>
                {
                    resultofsemanas = IsToSave();
                },
                IsPositionOut = (result) =>
                {
                    if (result)
                    {
                        TapGestureRecognizer_Tapped();
                    }
                    else
                    {
                        (BindingContext as SacViewModel).CurrenClienteAVisitarDespuesDe = null;
                        (BindingContext as SacViewModel).CliRutPosiciones = "";
                    }
                },
                IsOrdenOut = (result) =>
                {
                    if (result)
                    {
                        TapGestureRecognizer_Tapped_1();
                    }
                    else
                    {
                        (BindingContext as SacViewModel).CurrenClienteAOrdenarDespuesDe = null;
                        (BindingContext as SacViewModel).CliOrdenRutas = "";
                    }
                },
            };
            InitializeComponent();

            var binding = (BindingContext as SacViewModel);

            binding.CurrentMunicipio =
                binding.Municipios?.Where(x => x.MunID 
                == binding.CurrentClient?.MunID).FirstOrDefault();


            if (binding.CurrentProvincia != null && binding.Municipios != null)
            {
                binding.CurrentMunicipio = binding.Municipios.Where(x => x.MunID == binding.CurrentClient.MunID).FirstOrDefault();

                if (binding.CurrentMunicipio != null && binding.Sectores != null && binding.Sectores.Count > 0)
                {
                    //CurrentMunSector = Sectores.Where(x => x.MunCodigo == CurrentClient.SecCodigo).FirstOrDefault();
                    binding.CurrentMunSector = binding.Sectores.Where(x => x.SecCodigo == binding.CurrentClient.SecCodigo).FirstOrDefault();
                }
            }

            binding.IsVisibleEntrySector = binding.Sectores == null || binding.Sectores.Count <= 0;
            binding.IsVisiblePickerSector = !binding.IsVisibleEntrySector;

            if (!DS_RepresentantesParametros.GetInstance().GetParSacCapturarHorario())
            {
                Children.RemoveAt(1);
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
                Functions.StartListeningForLocations();
            }
            
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopLocationsUpdates();
        }

        private async void StopLocationsUpdates()
        {
            await Functions.StopListeningForLocations();
        }

        private bool finishing;
        private async void AlertSalir()
        {
            if (finishing)
            {
                return;
            }

            finishing = true;

            var result = await DisplayAlert("Aviso", "Esta seguro que deseas salir?", "Si", "No");

            if (result)
            {
                await Navigation.PopAsync(true);
            }

            finishing = false;
        }

        private void BorderlessPicker_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (checkboxContainer.Children.Count <= 0)
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
                        Text = "Semana" + (i + 1),
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

                if (cliente != null)
                {
                    string semanastocount = cliente.CliRutSemana1 + cliente.CliRutSemana2 + cliente.CliRutSemana3 + cliente.CliRutSemana4;
                    char[] chars = semanastocount.ToCharArray();
                    int num = 0;
                    if (chars.Length > 0)
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
                foreach (var chil in checkboxContainer.Children)
                {
                    if (chil is StackLayout)
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

                if (checkboxContainer.Children[i] is StackLayout stack)
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
            Navigation.PushPopupAsync(new PopupSearchFrame()
            {
                ItemSelected = (s) =>
                {
                    SelectedIndexChanged(s);
                    (BindingContext as SacViewModel).CurrenClienteAVisitarDespuesDe = s;
                    (BindingContext as SacViewModel).CliRutPosiciones = 
                    (BindingContext as SacViewModel).IsToggledVisitar ? (s.CliRutPosicion + 1).ToString() :
                    (s.CliRutPosicion - 1).ToString();
                }
            });
        }


        private async void SelectedIndexChanged(Clientes clientes, bool iscancel = false)
        {
            cliente = new DS_Clientes().GetAllClientesRutPosicion(lblantes.IsEnabled ? clientes.CliRutPosicion - 1 : clientes.CliRutPosicion + 1);
            if (!iscancel && cliente != null)
            {
                bool result = await DisplayAlert("Aviso", $"Debe utilizar otra posicion, la que intenta utilizar, ya esta ocupada por este cliente: {cliente.CliRutPosicion} - {cliente.CliNombre} ", "aceptar", "cancelar");
                if (result)
                {
                    TapGestureRecognizer_Tapped();
                    return;
                }
                else
                {
                    (BindingContext as SacViewModel).CurrenClienteAVisitarDespuesDe = null;
                    (BindingContext as SacViewModel).CliRutPosiciones = "";
                }
            }

            if (checkboxContainer.Children.Count > 0)
            {
                string semanas = cliente != null && (BindingContext as SacViewModel).CurrenClienteAVisitarDespuesDe != null ? cliente.CliRutSemana1 + cliente.CliRutSemana2 + cliente.CliRutSemana3 + cliente.CliRutSemana4 : "";
                char[] chars = semanas.ToCharArray();
                if (chars.Length > 0)
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
                }
                else
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

        public string IsToSave()
        {
            string countcheck = "";
            foreach (var chil in checkboxContainer.Children.Where(c => c is StackLayout))
            {
                foreach (var check in (chil as StackLayout).Children.Where(s => s is CheckBoxs))
                {
                    countcheck += Convert.ToInt32((check as CheckBoxs).IsChecked).ToString();
                }
            }
            return !string.IsNullOrEmpty(countcheck) ? countcheck : "00000000000000000000000000000000000";
        }

        protected override bool OnBackButtonPressed()
        {
            if (PopupNavigation.Instance.PopupStack.Count > 0)
            {
                PopupNavigation.Instance.PopAllAsync(true);

                if(rutorden)
                {
                    (BindingContext as SacViewModel).CurrenClienteAOrdenarDespuesDe = null;
                    (BindingContext as SacViewModel).CliOrdenRutas = "";
                }else
                {

                    (BindingContext as SacViewModel).CurrenClienteAVisitarDespuesDe = null;
                    (BindingContext as SacViewModel).CliRutPosiciones = "";
                }
                return true;
            }
            AlertSalir();
            ClientesViewModel.isFromSAC = false;
            return true;
        }

        private void TapGestureRecognizer_Tapped_1(object sender = null, EventArgs e = null)
        {
            rutorden = true;
            Navigation.PushPopupAsync(new PopupSearchFrame()
            {
                ItemSelected = (s) =>
                {
                    s.CliNombre = s.CliOrdenRuta + "/" + s.CliNombre;
                    (BindingContext as SacViewModel).CurrenClienteAOrdenarDespuesDe = s;                    
                    (BindingContext as SacViewModel).CliOrdenRutas = s.CliOrdenRuta.ToString();
                }
            });
        }

        private void horarioListItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if(e.SelectedItem == null)
            {
                return;
            }


            if(BindingContext is SacViewModel vm)
            {
                vm.AttempDeleteShedule(e.SelectedItem as SolicitudActualizacionClientesHorarios);
            }

            listSchedule.SelectedItem = null;
        }
    }
}