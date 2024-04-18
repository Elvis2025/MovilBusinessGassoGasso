
using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.Resx;
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
	public partial class VisitaFallidaModal : ContentPage, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;

        private Action<string> OnAceptar;

        public List<TiposMensaje> Motivos { get; set; }

        private TiposMensaje currentmotivo;
        public TiposMensaje CurrentMotivo { get => currentmotivo; set { currentmotivo = value; RaiseOnPropertyChanged(); ConfigDetalle(); } }

        private bool isdetailed = false;
        public bool IsDetailed { get => isdetailed; set { isdetailed = value; RaiseOnPropertyChanged(); } }

        public VisitaFallidaModal (Action<string> OnAceptar)
		{
            try
            {
                this.OnAceptar = OnAceptar;

                InitializeComponent();

                Motivos = new DS_TiposMensaje().GetTipoMensaje(42);
                bool RemoverMotivoOtros = DS_RepresentantesParametros.GetInstance().GetParVisitasFallidasRemoverMensajeOtros();
                if (RemoverMotivoOtros && Motivos != null && Motivos.Count > 0)
                {
                    Motivos = Motivos.Where(m => m.MenID != -1).ToList();
                }

                BindingContext = this;

            }catch(Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }
		}

        private void AttempSave(object sender, EventArgs args)
        {
            string motivo = null;


            if (CurrentMotivo != null)
            {
                motivo = CurrentMotivo.MenDescripcion;
            }

            if (IsDetailed)
            {
                motivo = editDetalle.Text;
            }

            OnAceptar?.Invoke(motivo);

            Navigation.PopModalAsync(true);
        }

        private void ConfigDetalle()
        {
            if (CurrentMotivo == null)
            {
                return;
            }

            IsDetailed = CurrentMotivo.MenDescripcion != null && CurrentMotivo.MenDescripcion.ToUpper().Replace("S", "") == "OTRO";

        }

        private void Dismiss(object sender, EventArgs args)
        {
            Navigation.PopModalAsync(true);
        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}