using MovilBusiness.Controls.Behavior;
using MovilBusiness.DataAccess;
using MovilBusiness.Model;
using MovilBusiness.Resx;
using MovilBusiness.ViewModel;
using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class EncuestasPage : ContentPage
	{
        private bool FirstTime = true;

		public EncuestasPage (bool isFromHome)
		{
            BindingContext = new EncuestasViewModel(this, (p)=> { AgregarPreguntas(p); }, isFromHome);
			InitializeComponent ();

            if (DS_RepresentantesParametros.GetInstance().GetParFotosEncuestasCount() > 0)
            {
                ToolbarItems.Insert(0, new ToolbarItem() { Text = AppResource.Photo, Order = ToolbarItemOrder.Primary, IconImageSource = "ic_photo_camera_white_24dp", Command = ((EncuestasViewModel)BindingContext).GoFotoCommand });
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if(FirstTime && BindingContext is EncuestasViewModel vm)
            {
                FirstTime = false;
                vm.SeleccionarEncuesta();
            }
        }

        private void AgregarPreguntas(List<Preguntas> preguntas)
        {
            try
            {
                foreach (Preguntas pregunta in preguntas)
                {
                    var layout = new StackLayout() { HorizontalOptions = LayoutOptions.FillAndExpand, Orientation = StackOrientation.Vertical, BindingContext = pregunta };
                    layout.Children.Add(new Label() { Text = pregunta.PreDescripcion, FontAttributes = FontAttributes.Bold });

                    List<PreguntasOpciones> opciones = new List<PreguntasOpciones>();

                    if (pregunta.PreTipoPregunta == 2 || pregunta.PreTipoPregunta == 5 || pregunta.PreTipoPregunta == 6)
                    {
                        opciones = ((EncuestasViewModel)BindingContext).GetPreguntasOpciones(pregunta.EstID, pregunta.PreID);
                    }

                    switch (pregunta.PreTipoPregunta)
                    {
                        case 1: //pregunta abierta
                            layout.Children.Add(new Entry() { HorizontalOptions = LayoutOptions.FillAndExpand, Margin = new Thickness(5, 0, 5, 10), Keyboard = Keyboard.Default});
                            break;
                        case 2: //pregunta seleccion simple (combobox)
                            var picker = new Picker() { HorizontalOptions = LayoutOptions.FillAndExpand, Margin = new Thickness(5, 0, 5, 10), Title = AppResource.Select };
                            picker.ItemsSource = opciones;
                            picker.ItemDisplayBinding = new Binding("PreDescripcion");
                            layout.Children.Add(picker);
                            break;
                        case 3: //pregunta abierta numerica
                            var entry = new Entry() { HorizontalOptions = LayoutOptions.FillAndExpand, Margin = new Thickness(5, 0, 5, 10), Keyboard = Keyboard.Numeric };
                            entry.Behaviors.Add(new NumericValidation());
                            layout.Children.Add(entry);
                            break;
                        case 4: //pregunta tipo booleana (Si/No)
                            var k = new StackLayout() { Orientation = StackOrientation.Horizontal, HorizontalOptions = LayoutOptions.FillAndExpand, Margin = new Thickness(5,0,5,10) };
                            k.Children.Add(new Label() { Text = AppResource.No, VerticalOptions = LayoutOptions.Center});
                            k.Children.Add(new Switch());
                            k.Children.Add(new Label() { Text = AppResource.Yes, VerticalOptions = LayoutOptions.Center });
                            layout.Children.Add(k);
                            break;
                        case 5:
                        case 6://pregunta seleccion multiple / multiple obligatoria
                            var g = new StackLayout() { Orientation = StackOrientation.Vertical, HorizontalOptions = LayoutOptions.FillAndExpand, Margin = new Thickness(5,0,5,10)};
                            foreach (var opcion in opciones)
                            {
                                var ll = new StackLayout() { HorizontalOptions = LayoutOptions.FillAndExpand, Orientation = StackOrientation.Horizontal, BindingContext = opcion };
                                ll.Children.Add(new Label() { Text = opcion.PreDescripcion, VerticalOptions = LayoutOptions.Center });
                                ll.Children.Add(new Switch());
                                g.Children.Add(ll);
                            }
                            layout.Children.Add(g);
                            break;
                        case 7: //pregunta abierta numerica con decimales
                            var edit = new Entry() { HorizontalOptions = LayoutOptions.FillAndExpand, Margin = new Thickness(5, 0, 5, 10), Keyboard = Keyboard.Numeric };
                            layout.Children.Add(edit);
                            break;
                    }

                    viewContainer.Children.Add(layout);
                }
            }catch(Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }
        }

        private void GuardarEncuesta(object sender, EventArgs args)
        {
            ((EncuestasViewModel)BindingContext).GuardarEncuesta(viewContainer);
        }

        protected override bool OnBackButtonPressed()
        {
            Finish();
            return true;
        }

        private bool finishing;
        private async void Finish()
        {
            if (finishing)
            {
                return;
            }
            finishing = true;
            await Navigation.PopAsync(false);
            finishing = false;
        }
    }
}