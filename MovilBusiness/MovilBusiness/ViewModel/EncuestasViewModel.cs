using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using MovilBusiness.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class EncuestasViewModel : BaseViewModel
    {
        private DS_Estudios myEst;
        public ICommand GoFotoCommand { get; private set; }
        public ICommand ShowDatosClienteCommand { get; private set; }

        private Estudios currentencuesta;
        public Estudios CurrentEncuesta { get => currentencuesta; set { currentencuesta = value; CargarPreguntas(); RaiseOnPropertyChanged(); } }

        private bool isfoto;
        public bool IsFoto { get => isfoto; set { isfoto = value; RaiseOnPropertyChanged(); } }

        private string comentario;
        public string Comentario { get => comentario; set { comentario = value; RaiseOnPropertyChanged(); } }

        private List<Preguntas> preguntas;
        public List<Preguntas> Preguntas { get => preguntas; set { preguntas = value; RaiseOnPropertyChanged(); } }

        private EncuestaInfoCliente infocliente;
        public EncuestaInfoCliente infoCliente { get => infocliente; set { infocliente = value; RaiseOnPropertyChanged(); } }

        private Action<List<Preguntas>> OnSeleccionarEncuesta;

        private bool showdatoscliente = false;
        public bool ShowDatosCliente { get => showdatoscliente; set { showdatoscliente = value; RaiseOnPropertyChanged(); } }

        public bool FromHome { get; set; }

        public EncuestasViewModel(Page page, Action<List<Preguntas>> OnSeleccionarEncuesta, bool isFromHome) : base(page)
        {
            myEst = new DS_Estudios();
            FromHome = isFromHome;
            this.OnSeleccionarEncuesta = OnSeleccionarEncuesta;

            IsFoto = DS_RepresentantesParametros.GetInstance().GetParFotosEncuestasCount() > 0;

            GoFotoCommand = new Command(GoPhoto);
            ShowDatosClienteCommand = new Command(() => { ShowDatosCliente = !ShowDatosCliente; });

            infoCliente = new EncuestaInfoCliente();

            if (!FromHome)
            {
                infoCliente.CliNombre = Arguments.Values.CurrentClient.CliNombre;
                infoCliente.CliContacto = Arguments.Values.CurrentClient.CliContacto;
                infoCliente.CliTelefono = Arguments.Values.CurrentClient.CliTelefono;
                infoCliente.CliWhatsapp = Arguments.Values.CurrentClient.CliPropietarioCelular;
                infoCliente.CliDireccion = Arguments.Values.CurrentClient.CliCalle;
                infoCliente.CliSector = Arguments.Values.CurrentClient.cliSector;
                infoCliente.CliCorreoElectronico = Arguments.Values.CurrentClient.CliCorreoElectronico;
            }
        }

        public async void GuardarEncuesta(StackLayout viewContainer)
        {
            if (IsBusy)
            {
                return;
            }

            try
            {

                if (FromHome)
                {
                    if (string.IsNullOrWhiteSpace(infoCliente.CliNombre))
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.CustomerNameCannotBeEmpty);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(infoCliente.CliContacto))
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.ContactCannotBeEmpty);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(infoCliente.CliTelefono))
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.PhoneCannotBeEmpty);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(infoCliente.CliWhatsapp))
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.WhatsappCannotBeEmpty);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(infoCliente.CliDireccion))
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.AddressCannotBeEmpty);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(infoCliente.CliSector))
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.SectorCannotBeEmpty);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(infoCliente.CliCorreoElectronico))
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.EmailCannotBeEmpty);
                        return;
                    }
                }

                var respuestas = new List<MuestrasRespuestas>();

                foreach(View v in viewContainer.Children)
                {
                    if(v.BindingContext is Preguntas && v is StackLayout)
                    {
                        var pre = v.BindingContext as Preguntas;

                        switch (pre.PreTipoPregunta)
                        {
                            case 1:
                            case 3:
                            case 7:
                                var entry = (v as StackLayout).Children[1] as Entry;
                                respuestas.Add(new MuestrasRespuestas() { ResRespuesta = string.IsNullOrWhiteSpace(entry.Text)?"":entry.Text, PreID = pre.PreID });
                                break;
                            case 2:
                                var picker = (v as StackLayout).Children[1] as Picker;

                                if(picker.ItemsSource == null)
                                {
                                    continue;
                                }

                                var opciones = picker.ItemsSource as List<PreguntasOpciones>;
                                var item = opciones.Count > 0 ? opciones[0] : new PreguntasOpciones();
                                if (picker.SelectedIndex > -1)
                                {
                                    item = opciones[picker.SelectedIndex];
                                }
                                else
                                {
                                    await DisplayAlert(AppResource.Warning, AppResource.CannotLeaveSelectFieldsUnanswered, AppResource.Aceptar);
                                    return;
                                }

                                if(item == null) { continue; }

                                respuestas.Add(new MuestrasRespuestas() { ResRespuesta = item.PreDescripcion, PreID = pre.PreID });

                                break;
                            case 4:
                                var ll = (v as StackLayout).Children[1] as StackLayout;

                                var opt = ll.Children[1] as Switch;

                                respuestas.Add(new MuestrasRespuestas() { ResRespuesta = opt.IsToggled ? AppResource.Yes : AppResource.No, PreID = pre.PreID });

                                break;
                            case 5:
                            case 6:
                                var ly = (v as StackLayout).Children[1] as StackLayout;

                                var respuesta = new MuestrasRespuestas() { PreID = pre.PreID };

                                bool first = true;

                                foreach (var raw in ly.Children)
                                {
                                    var r = raw as StackLayout;
                                    PreguntasOpciones opcion = r.BindingContext as PreguntasOpciones;

                                    if((r.Children[1] as Switch).IsToggled)
                                    {
                                        if (first)
                                        {
                                            first = false;
                                            respuesta.ResRespuesta = opcion.PreDescripcion;
                                        }
                                        else
                                        {
                                            respuesta.ResRespuesta += ", " + opcion.PreDescripcion;
                                        }
                                    }
                                     
                                }

                                if (pre.PreID == 6 && string.IsNullOrEmpty(respuesta.ResRespuesta))
                                {
                                    await DisplayAlert(AppResource.Warning, AppResource.MustSelectAnswerWhenIsRequiredMultipleChoise, AppResource.Aceptar);
                                    return;
                                }

                                if (!first)
                                {
                                    respuestas.Add(respuesta);
                                }

                                break;
                        }
                    }
                }

                IsBusy = true;

                if(respuestas.Count == 0)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.NotAddedAnyAnswerMessage);
                    IsBusy = false;
                    return;
                }

                var task = new TaskLoader() { SqlTransactionWhenRun = true };

                await task.Execute(() => { myEst.GuardarEncuesta(respuestas, CurrentEncuesta.EstID, Comentario, infoCliente); });

                await DisplayAlert(AppResource.Success, AppResource.SampleSavedSuccessfully, AppResource.Aceptar);

                await PopAsync(true);

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.ErrorSavingSurvey, e.Message);
            }

            IsBusy = false;
        }

        public async void SeleccionarEncuesta()
        {
            try
            {
                var list = myEst.GetEncuestasVigentes(FromHome ? -1 : Arguments.Values.CurrentClient.CliID);

                if (list.Count == 0)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.NoSurveyAvailables, AppResource.Aceptar);
                    await PopAsync(true);
                    return;
                }

                List<string> opciones = new List<string>();

                foreach (var encuesta in list)
                {
                    opciones.Add(encuesta.EstNombre + " - " + encuesta.EstCantidadPreguntas.ToString());
                }

                var result = await DisplayActionSheet(AppResource.SelectSurvey, buttons: opciones.ToArray());

                var item = list.Where(x => x.EstNombre + " - " + x.EstCantidadPreguntas.ToString() == result).FirstOrDefault();

                if (item == null)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.MustSelectSurvey);
                    await PopAsync(true);
                    return;
                }

                CurrentEncuesta = item;

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.ErrorLoadingSurveys, e.Message);
                await PopAsync(false);
            }
        }

        private async void CargarPreguntas()
        {
            try
            {
                if (CurrentEncuesta == null)
                {
                    await DisplayAlert(AppResource.Warning, AppResource.MustSelectSurvey);
                    return;
                }

                Preguntas = myEst.GetPreguntasEncuesta(CurrentEncuesta.EstID);

                if(Preguntas == null || Preguntas.Count == 0)
                {
                    var result = await DisplayAlert(AppResource.Warning, AppResource.SurveyHasNoQuestions, AppResource.Yes, AppResource.No);

                    if (result)
                    {
                        SeleccionarEncuesta();
                    }
                    else
                    {
                        await PopAsync(false);
                    }
                    
                    return;
                }

                OnSeleccionarEncuesta?.Invoke(Preguntas);

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.ErrorLoadingQuestions, e.Message);
            }
        }

        public List<PreguntasOpciones> GetPreguntasOpciones(int estId, int preId)
        {
            return myEst.GetPreguntasOpciones(estId, preId);
        }
        private void GoPhoto()
        {
            try
            {
                var EncSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Muestras");

                PushAsync(new CameraPage(EncSecuencia.ToString(), "Muestras"));
            }
            catch (Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }
        }
    }
}
