using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ExcluirClienteModal : ContentPage, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;

        public List<UsosMultiples> Motivos { get; set; }

        private UsosMultiples currentmotivo;
        public UsosMultiples CurrentMotivo { get => currentmotivo; set { currentmotivo = value; RaiseOnPropertyChanged(); ConfigDetalle(); } }

        private bool isdetailed = false;
        public bool IsDetailed { get => isdetailed; set { isdetailed = value; RaiseOnPropertyChanged(); } }

        private string comentario;
        public string Comentario { get => comentario; set { comentario = value; RaiseOnPropertyChanged(); } }

        private Clientes CurrentClient;
        public ExcluirClienteModal(Clientes CurrentClient)
        {
            this.CurrentClient = CurrentClient;

            try
            {
                InitializeComponent();

                Motivos = new DS_UsosMultiples().GetMotivosExclusionClientes();

                BindingContext = this;

            }
            catch (Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }
        }

        private async void Dismiss(object sender, EventArgs args)
        {
            await Navigation.PopModalAsync(false);
        }

        private async void AttempSave(object sender, EventArgs args)
        {
            string motivo = null;

            if (CurrentMotivo != null)
            {
                motivo = CurrentMotivo.Descripcion;
            }

            if (IsDetailed)
            {
                motivo = editDetalle.Text;
            }

            if(CurrentMotivo == null)
            {
                await DisplayAlert(AppResource.Warning, AppResource.SpecifyReasonWarning, AppResource.Aceptar);
                return;
            }

            if(string.IsNullOrEmpty(editDetalle.Text) && CurrentMotivo != null && CurrentMotivo.Descripcion == "Otros")
            {
                await DisplayAlert(AppResource.Warning, AppResource.SpecifyReasonWarning, AppResource.Aceptar);
                return;
            }

            try
            {
                var task = new TaskLoader() { SqlTransactionWhenRun = true };
                await task.Execute(() =>
                {
                    new DS_SolicitudExclusionCliente().GuardarSolicitud(CurrentClient.CliID, motivo, Comentario);
                });

                await DisplayAlert(AppResource.Success, AppResource.ApplicationSavedSuccessfully, AppResource.Aceptar);

                await Navigation.PopModalAsync(true);
            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.Error, e.Message, AppResource.Aceptar);
            }            
        }

        private void ConfigDetalle()
        {
            try
            {
                if (CurrentMotivo == null)
                {
                    return;
                }

                IsDetailed = CurrentMotivo.Descripcion != null && CurrentMotivo.Descripcion.ToUpper().Trim().Replace("S", "") == "OTRO";
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}